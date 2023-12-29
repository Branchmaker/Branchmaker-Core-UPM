﻿using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
            StoryManager.manager.OnNodeChange.AddListener(NodeChanged);
        }

        private void NodeChanged(BranchNode node)
        {
            var removeOthers = false;
            foreach (var storyScene in _sceneCollection.FindAll(a => a.MatchesNode(node)))
            {
                removeOthers = true;
                _currentScene = storyScene;
                storyScene.gameObject.SetActive(true);
            }

            if (removeOthers)
            {
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

        private void OnDestroy()
        {
            _currentScene = null;
        }

        public static bool SceneHasNodeButton(string nodeId)
        {
            if (!_currentScene) return false;
            return _currentScene.GetComponentsInChildren<StoryButton>(true).Any(a => a.gotoNode == nodeId);
        }
    }
}
