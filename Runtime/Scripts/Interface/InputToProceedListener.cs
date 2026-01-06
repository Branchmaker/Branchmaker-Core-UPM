using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

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
            
            if (WasAdvancePressedThisFrame())
            {
                if (IsPointerOverUIElement()) return;
                StoryManager.Instance.SpeakActiveNode();
            }
        }
        
        private static bool WasAdvancePressedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            // Keyboard
            if (Keyboard.current != null)
            {
                if (Keyboard.current.spaceKey.wasReleasedThisFrame) return true;
                if (Keyboard.current.enterKey.wasReleasedThisFrame) return true;
                if (Keyboard.current.numpadEnterKey.wasReleasedThisFrame) return true;
            }

            // Pointer / touch (primary press)
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) return true;
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame) return true;
            return false;
#else
            return Input.GetKeyUp(KeyCode.Space)
                   || Input.GetKeyUp(KeyCode.Return)
                   || Input.GetKeyUp(KeyCode.KeypadEnter)
                   || Input.GetMouseButtonDown(0);
#endif
        }
        
        private bool IsPointerOverUIElement()
        {
            var pointerEventData = new PointerEventData(eventSystem)
            {
                position = Input.mousePosition
            };
            var results = new List<RaycastResult>();
            if (graphicRaycaster) graphicRaycaster.Raycast(pointerEventData, results);
            return results.Count > 0;
        }
    }
}
