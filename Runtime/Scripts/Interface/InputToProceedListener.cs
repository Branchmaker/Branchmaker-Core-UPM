using System.Collections.Generic;
using BranchMaker.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BranchMaker.Interface
{
    public class InputToProceedListener : MonoBehaviour
    {
        private bool _armed;
        public GameObject clickToProceedIndicator;
        public EventSystem eventSystem;
        public GraphicRaycaster graphicRaycaster;

        private void Start()
        {
            StoryManager.Instance.OnBlockChange.AddListener(ProcessBlock);
            StoryManager.Instance.OnBlockComplete.AddListener(BlockComplete);
            if (clickToProceedIndicator) clickToProceedIndicator.SetActive(false);
        }

        private void BlockComplete(BranchNodeBlock arg0)
        {
            if (!StoryManager.HasSpeakingQueue()) return;
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
                if (IsPointerOverUIElement()) return;
                StoryManager.Instance.SpeakActiveNode();
            }
            
            if (Input.GetKeyUp(KeyCode.F5)) StoryManager.Instance.ForceReloadFromServer();

        }
        
        
        private bool IsPointerOverUIElement()
        {
            var pointerEventData = new PointerEventData(eventSystem)
            {
                position = Input.mousePosition
            };
            var results = new List<RaycastResult>();
            graphicRaycaster.Raycast(pointerEventData, results);
            return results.Count > 0;
        }
    }
}
