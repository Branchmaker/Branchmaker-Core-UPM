using System.Collections.Generic;
using UnityEngine;

namespace BranchMaker.Story
{
    public class StoryScene : MonoBehaviour
    {
        public string storyNodeId = string.Empty;
        public List<string> storyNodeIdAlts = new List<string>();

        private void Awake()
        {
            StorySceneManager.RegisterScene(this);
        }
    }
}