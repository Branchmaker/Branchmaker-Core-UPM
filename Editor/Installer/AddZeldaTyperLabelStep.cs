#if UNITY_EDITOR
using BranchMaker.Interface;
using BranchMaker.Interface.DialogueWriters;
using SockpuppetNoir.BranchMaker.Installer;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BranchMaker.Editor.InstallWizard
{
    public sealed class AddZeldaTyperLabelStep : IBranchMakerInstallStep
    {
        public int Order => 20;
        public string Title => "Step 2 — Add ZeldaTyper TMP Label";
        public string Description => "Adds a TextMeshProUGUI label under a Canvas (reuses one if present, otherwise creates one) and adds ZeldaTyperTMPro.";

        private const string LabelName = "ZeldaTyperLabel";

        public StepState GetState()
        {
            var typerType = typeof(ZeldaTyperTMPro);
            var existing = InstallWizardUtil.FindFirstComponentInActiveScene(typerType);
            return existing ? StepState.Done : StepState.NotDone;
        }

        public void Run()
        {
            var typerType = typeof(ZeldaTyperTMPro);
            var progressType = typeof(InputToProceedListener);

            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                EditorUtility.DisplayDialog("BranchMaker Install Wizard", "No valid active scene found.", "OK");
                return;
            }

            var canvas = InstallWizardUtil.FindAnyCanvasInActiveScene();
            if (!canvas) canvas = InstallWizardUtil.CreateCanvas();

            InstallWizardUtil.EnsureEventSystemExists(scene);

            var labelGo = InstallWizardUtil.FindInActiveSceneByName(LabelName);
            if (!labelGo)
            {
                labelGo = new GameObject(LabelName, typeof(RectTransform));
                Undo.RegisterCreatedObjectUndo(labelGo, "Create ZeldaTyper Label");
                labelGo.transform.SetParent(canvas.transform, false);
            }
            else
            {
                Undo.RecordObject(labelGo, "Configure ZeldaTyper Label");
            }

            var tmp = labelGo.GetComponent<TextMeshProUGUI>();
            if (!tmp) tmp = Undo.AddComponent<TextMeshProUGUI>(labelGo);

            if (!labelGo.GetComponent(typerType))
                Undo.AddComponent(labelGo, typerType);
            
            if (!labelGo.GetComponent(progressType))
                Undo.AddComponent(labelGo, progressType);

            ConfigureLabelRect(tmp.rectTransform);
            ConfigureTMPDefaults(tmp);

            EditorSceneManager.MarkSceneDirty(scene);

            Selection.activeObject = labelGo;
            EditorGUIUtility.PingObject(labelGo);
        }

        public UnityEngine.Object GetSelectTarget()
        {
            return InstallWizardUtil.FindInActiveSceneByName(LabelName);
        }

        private static void ConfigureLabelRect(RectTransform rt)
        {
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(0f, 0f);
            rt.sizeDelta = new Vector2(0f, 0f);
            rt.anchoredPosition = new Vector2(0, 0);
            rt.sizeDelta = new Vector2(0, 160f);
        }

        private static void ConfigureTMPDefaults(TextMeshProUGUI tmp)
        {
            tmp.text = "ZeldaTyper ready.";
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.fontSize = 36f;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMax = 36f;
            tmp.margin = new Vector4(10, 10, 10, 10);
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
        }
    }
}
#endif
