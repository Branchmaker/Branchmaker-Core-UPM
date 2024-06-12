using BranchMaker.Runtime;
using UnityEngine;

namespace BranchMaker.Interface
{
    public class InputToProceedListener : MonoBehaviour
    {
        private bool _armed;
        public GameObject clickToProceedIndicator;

        private void Start()
        {
            StoryManager.Instance.OnBlockChange.AddListener(ProcessBlock);
            StoryManager.Instance.OnBlockComplete.AddListener(BlockComplete);
            if (clickToProceedIndicator) clickToProceedIndicator.SetActive(false);
        }

        private void BlockComplete(BranchNodeBlock arg0)
        {
            if (!StoryManager.Instance.HasSpeakingQueue()) return;
            _armed = true;
            if (clickToProceedIndicator) clickToProceedIndicator.SetActive(true);
        }

        private void ProcessBlock(BranchNodeBlock arg0)
        {
            _armed = false;
            if (clickToProceedIndicator) clickToProceedIndicator.SetActive(false);
        }

        private void Update()
        {
            if (!_armed) return;
            if (StoryManager.Busy()) return;
            
            if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.Return) || Input.GetMouseButtonDown(0))
            {
                StoryManager.Instance.SpeakActiveNode();
            }
            
            if (Input.GetKeyUp(KeyCode.F5)) StoryManager.Instance.ForceReloadFromServer();

        }
    }
}
