using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Xml;

namespace BranchMaker.Story
{
    public class StorySceneManager : MonoBehaviour
    {
        private static readonly List<StorySceneManager> Managers = new();
        private static readonly Dictionary<string, StoryScene> SceneBank = new();
        private static readonly Dictionary<string, StoryButton> SceneButtons = new();
        private static List<StoryScene> SceneCollection = new();

        private static StoryScene _currentScene;

        public bool LogChanges;

        private void Awake()
        {
            if (!Managers.Contains(this)) Managers.Add(this);
            foreach (var scene in transform.GetComponentsInChildren<StoryScene>(true))
            {
                RegisterScene(scene);
            }

            SceneCollection = transform.GetComponentsInChildren<StoryScene>(true).ToList();
        }

        private void Start()
        {
            StoryManager.manager.OnNodeChange.AddListener(NodeChanged);
        }

        private void NodeChanged(BranchNode node)
        {
            bool removeOthers = false;
            foreach (var storyScene in SceneCollection.FindAll(a => a.MatchesNode(node)))
            {
                removeOthers = true;
                _currentScene = storyScene;
                storyScene.gameObject.SetActive(true);
            }

            if (removeOthers)
            {
                foreach (var storyScene in SceneCollection.FindAll(a => !a.MatchesNode(node)))
                {
                    storyScene.gameObject.SetActive(false);
                }   
            }
        }

        private void OnEnable()
        {/*
            foreach (var scene in transform.GetComponentsInChildren<StoryScene>(true))
            {
                RegisterScene(scene);
            }
        */
        }

        internal static void RegisterScene(StoryScene scene)
        {
            /*
            scene.gameObject.SetActive(false);
            if (string.IsNullOrEmpty(scene.storyNodeId)) return;
            if (SceneBank.ContainsKey(scene.storyNodeId))
            {
                SceneBank[scene.storyNodeId] = scene;
                foreach (var altId in scene.storyNodeIdAlts)
                {
                    SceneBank[altId] = scene;
                }
                return;
            }
            SceneBank.Add(scene.storyNodeId, scene);
            foreach (var altId in scene.storyNodeIdAlts)
            {
                SceneBank.Add(altId, scene);
            }
*/
        }

        public static string CurrentSceneLoaded()
        {
            return _currentScene ? _currentScene.storyNodeId : string.Empty;
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
            /*
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
            */
        }
    }
}
