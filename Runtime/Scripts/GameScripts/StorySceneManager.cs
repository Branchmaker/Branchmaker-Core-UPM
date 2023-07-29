using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace BranchMaker.Story
{
    public class StorySceneManager : MonoBehaviour
    {
        private static readonly List<StorySceneManager> Managers = new();
        private static readonly Dictionary<string, StoryScene> SceneBank = new();
        private static readonly Dictionary<string, StoryButton> SceneButtons = new();

        static StoryScene _currentScene;

        private void Awake()
        {
            if (!Managers.Contains(this)) Managers.Add(this);
            foreach (var scene in transform.GetComponentsInChildren<StoryScene>(true))
            {
                RegisterScene(scene);
            }
        }

        internal static void RegisterScene(StoryScene scene)
        {
            if (string.IsNullOrEmpty(scene.storyNodeId)) return;
            if (SceneBank.ContainsKey(scene.storyNodeId)) return;
            SceneBank.Add(scene.storyNodeId, scene);
            foreach (var altId in scene.storyNodeIdAlts)
            {
                SceneBank.Add(altId, scene);
            }

            scene.gameObject.SetActive(false);
        }

        public static string CurrentSceneLoaded()
        {
            return _currentScene == null ? string.Empty : _currentScene.storyNodeId;
        }

        private void OnDestroy()
        {
            Managers.Clear();
            SceneBank.Clear();
            SceneButtons.Clear();
            _currentScene = null;
        }

        public static bool SceneHasNodeButton(string nodeId)
        {
            return SceneButtons.ContainsKey(nodeId);
        }

        public static void ShowPotentialScene(string nodeKey)
        {
            if (!SceneBank.ContainsKey(nodeKey)) return;
            if (_currentScene != null)
            {
                if (_currentScene.storyNodeId == nodeKey) return;
                _currentScene.gameObject.SetActive(false);
            }

            foreach (var manager in Managers)
            {
                foreach (var scene in manager.gameObject.GetComponentsInChildren<StoryScene>(true).Where(a => a.storyNodeId == nodeKey))
                {
                    scene.gameObject.SetActive(true);
                }
            }

            _currentScene = SceneBank[nodeKey];
            SceneButtons.Clear();
            _currentScene.gameObject.SetActive(true);

            foreach (var btn in _currentScene.GetComponentsInChildren<StoryButton>())
            {
                if (string.IsNullOrEmpty(btn.gotoNode)) continue;
                if (SceneButtons.ContainsKey(btn.gotoNode)) continue;
                SceneButtons.Add(btn.gotoNode, btn);
            }
        }
    }
}
