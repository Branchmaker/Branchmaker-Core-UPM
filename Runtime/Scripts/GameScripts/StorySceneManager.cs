using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using BranchMaker.Runtime;

namespace BranchMaker.Story
{
    public class StorySceneManager : MonoBehaviour
    {
        private static List<StoryScene> _sceneCollection = new();
        private static StoryScene _currentScene;

        private void Awake()
        {
            _sceneCollection = transform.GetComponentsInChildren<StoryScene>(true).ToList();
        }

        private void Start()
        {
            StoryManager.Instance.OnNodeChange.AddListener(NodeChanged);
        }

        private void NodeChanged(BranchNode node)
        {
            var targetScene = _sceneCollection.FirstOrDefault(a => a.MatchesNode(node));
            if (targetScene)
            {
                _currentScene = targetScene;
                targetScene.gameObject.SetActive(true);
                _currentScene.SceneRootActivated();
                foreach (var storyScene in _sceneCollection.FindAll(a => !a.MatchesNode(node)))
                {
                    storyScene.gameObject.SetActive(false);
                }   
            }
        }

        public static string CurrentSceneLoaded()
        {
            return _currentScene ? _currentScene.storyNodeId : string.Empty;
        }
        public static StoryScene CurrentScene()
        {
            return _currentScene ? _currentScene : null;
        }

        private void OnDestroy()
        {
            _currentScene = null;
        }

        public static bool SceneHasActionButton(BranchNodeBlock action)
        {
            if (!_currentScene) return false;
            var button = _currentScene.GetComponentsInChildren<StoryButton>(true)
                .FirstOrDefault(a => a.gotoNode == action.target_node);
            if (!button) return false;
            Debug.Log("Found a button for "+button.name);
            button.designatedAction = action;
            return true;
        }
    }
}
