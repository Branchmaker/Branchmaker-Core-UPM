#if UNITY_EDITOR
using System;
using SockpuppetNoir.BranchMaker.Installer;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BranchMaker.Editor.InstallWizard
{
    public sealed class CreateStoryManagerStep : IBranchMakerInstallStep
    {
        public int Order => 10;
        public string Title => "Step 1 — Create StoryManager";
        public string Description => "Creates a GameObject named \"StoryManager\" and adds BranchMaker.StoryManager to it.";

        public StepState GetState()
        {
            var storyManagerType = GetStoryManagerType();
            if (storyManagerType == null) return StepState.NotDone;

            var go = GameObject.Find("StoryManager");
            if (go && go.GetComponent(storyManagerType))
                return StepState.Done;

            var any = InstallWizardUtil.FindFirstComponentInActiveScene(storyManagerType);
            return any ? StepState.Done : StepState.NotDone;
        }

        public void Run()
        {
            var storyManagerType = GetStoryManagerType();
            if (storyManagerType == null)
            {
                EditorUtility.DisplayDialog(
                    "BranchMaker Install Wizard",
                    "Could not find type 'BranchMaker.StoryManager'.\n\n" +
                    "Make sure the runtime assembly that contains BranchMaker.StoryManager is compiling and referenced.",
                    "OK"
                );
                return;
            }

            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                EditorUtility.DisplayDialog("BranchMaker Install Wizard", "No valid active scene found.", "OK");
                return;
            }

            var go = GameObject.Find("StoryManager");
            if (go == null)
            {
                go = new GameObject("StoryManager");
                Undo.RegisterCreatedObjectUndo(go, "Create StoryManager");
                EditorSceneManager.MarkSceneDirty(scene);
            }
            else
            {
                Undo.RecordObject(go, "Configure StoryManager");
            }

            if (!go.GetComponent(storyManagerType))
            {
                Undo.AddComponent(go, storyManagerType);
                EditorSceneManager.MarkSceneDirty(scene);
            }

            Selection.activeObject = go;
            EditorGUIUtility.PingObject(go);
        }

        public UnityEngine.Object GetSelectTarget()
        {
            var go = GameObject.Find("StoryManager");
            return go != null ? go : null;
        }

        private static Type GetStoryManagerType()
        {
            // Keep your existing approach:
            return typeof(BranchMaker.StoryManager);
        }
    }
}
#endif
