using System.Collections.Generic;
using UnityEngine;

namespace BranchMaker.Story
{
    public class StorySceneManager : MonoBehaviour
    {
        static readonly Dictionary<string, StoryScene> sceneBank = new Dictionary<string, StoryScene>();
        static readonly Dictionary<string, StoryButton> sceneButtons = new Dictionary<string, StoryButton>();

        static StoryScene _currentScene;

        private void Awake()
        {
            foreach (var scene in transform.GetComponentsInChildren<StoryScene>(true))
            {
                RegisterScene(scene);
            }
        }

        internal static void RegisterScene(StoryScene scene)
        {
            if (string.IsNullOrEmpty(scene.storyNodeId)) return;
            if (sceneBank.ContainsKey(scene.storyNodeId)) return;
            sceneBank.Add(scene.storyNodeId, scene);
            foreach (var altId in scene.storyNodeIdAlts)
            {
                sceneBank.Add(altId, scene);
            }

            scene.gameObject.SetActive(false);
        }

        public static string CurrentSceneLoaded()
        {
            return _currentScene == null ? string.Empty : _currentScene.storyNodeId;
        }

        private void OnDestroy()
        {
            sceneBank.Clear();
            sceneButtons.Clear();
            _currentScene = null;
        }

        public static bool SceneHasNodeButton(string nodeId)
        {
            return sceneButtons.ContainsKey(nodeId);
        }

        public static void ShowPotentialScene(string nodeKey)
        {
            if (!sceneBank.ContainsKey(nodeKey)) return;
            if (_currentScene != null)
            {
                if (_currentScene.storyNodeId == nodeKey) return;
                _currentScene.gameObject.SetActive(false);
            }

            _currentScene = sceneBank[nodeKey];
            sceneButtons.Clear();
            _currentScene.gameObject.SetActive(true);

            foreach (var btn in _currentScene.GetComponentsInChildren<StoryButton>())
            {
                if (string.IsNullOrEmpty(btn.gotoNode)) continue;
                if (sceneButtons.ContainsKey(btn.gotoNode)) continue;
                sceneButtons.Add(btn.gotoNode, btn);
            }
        }
    }
}
