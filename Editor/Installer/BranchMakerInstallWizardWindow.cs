#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BranchMaker.Editor.Editors;
using BranchMaker.Editor.InstallWizard;
using UnityEditor;
using UnityEngine;

namespace BranchMaker.Editor.Installer
{
    public sealed class BranchMakerInstallWizardWindow : EditorWindow
    {
        private const string MenuPath = "Tools/BranchMaker/Install wizard...";

        private List<IBranchMakerInstallStep> _steps;

        [MenuItem(MenuPath)]
        public static void Open()
        {
            var window = GetWindow<BranchMakerInstallWizardWindow>("BranchMaker Installer");
            window.minSize = new Vector2(560, 340);
            window.Show();
        }

        private void OnEnable()
        {
            BranchmakerEditorHeader.PrepLogo();
            _steps = DiscoverSteps();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(8);
            BranchmakerEditorHeader.DrawHeader("BranchMaker Install Wizard", "Sets up the active scene for BranchMaker.", 8f);

            EditorGUILayout.Space(8);

            if (_steps == null || _steps.Count == 0)
            {
                EditorGUILayout.HelpBox("No install steps found.", MessageType.Warning);
            }
            else
            {
                foreach (var step in _steps.OrderBy(s => s.Order))
                    DrawStep(step);
            }

            EditorGUILayout.Space(12);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Re-check", GUILayout.Width(120)))
                {
                    _steps = DiscoverSteps();
                    Repaint();
                }
            }
        }

        private void DrawStep(IBranchMakerInstallStep step)
        {
            var state = SafeGetState(step);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    DrawCheckmark(state == StepState.Done);

                    EditorGUILayout.LabelField(step.Title, EditorStyles.boldLabel);

                    GUILayout.FlexibleSpace();

                    using (new EditorGUI.DisabledScope(state == StepState.Done))
                    {
                        if (GUILayout.Button("Run", GUILayout.Width(90), GUILayout.Height(22)))
                        {
                            SafeRun(step);
                            GUI.FocusControl(null);
                        }
                    }
                }

                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField(step.Description, EditorStyles.wordWrappedMiniLabel);

                EditorGUILayout.Space(6);
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Status:", GUILayout.Width(60));
                    EditorGUILayout.LabelField(state == StepState.Done ? "Done" : "Not done", EditorStyles.miniLabel);

                    if (state == StepState.Done)
                    {
                        var target = SafeGetSelectTarget(step);
                        if (target != null)
                        {
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button("Select", GUILayout.Width(90)))
                                SelectAndPing(target);
                        }
                    }
                }
            }
        }

        private static List<IBranchMakerInstallStep> DiscoverSteps()
        {
            // Finds all non-abstract types implementing IBranchMakerInstallStep in loaded assemblies.
            var list = new List<IBranchMakerInstallStep>();

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try { types = asm.GetTypes(); }
                catch (ReflectionTypeLoadException e) { types = e.Types; }

                if (types == null) continue;

                foreach (var t in types)
                {
                    if (t == null) continue;
                    if (t.IsAbstract) continue;
                    if (!typeof(IBranchMakerInstallStep).IsAssignableFrom(t)) continue;

                    // Must have parameterless ctor
                    if (t.GetConstructor(Type.EmptyTypes) == null) continue;

                    try
                    {
                        if (Activator.CreateInstance(t) is IBranchMakerInstallStep step)
                            list.Add(step);
                    }
                    catch
                    {
                        // ignore step if it fails to construct
                    }
                }
            }

            return list.OrderBy(s => s.Order).ToList();
        }

        private static void SelectAndPing(UnityEngine.Object obj)
        {
            Selection.activeObject = obj;
            EditorGUIUtility.PingObject(obj);
        }

        private static void DrawCheckmark(bool done)
        {
            var icon = EditorGUIUtility.IconContent(done ? "TestPassed" : "TestNormal");
            var r = GUILayoutUtility.GetRect(18, 18, GUILayout.Width(18), GUILayout.Height(18));
            GUI.Label(r, icon);
        }

        private static StepState SafeGetState(IBranchMakerInstallStep step)
        {
            try { return step.GetState(); }
            catch { return StepState.NotDone; }
        }

        private static UnityEngine.Object SafeGetSelectTarget(IBranchMakerInstallStep step)
        {
            try { return step.GetSelectTarget(); }
            catch { return null; }
        }

        private static void SafeRun(IBranchMakerInstallStep step)
        {
            try { step.Run(); }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("BranchMaker Install Wizard", e.Message, "OK");
                Debug.LogException(e);
            }
        }
    }
}
#endif
