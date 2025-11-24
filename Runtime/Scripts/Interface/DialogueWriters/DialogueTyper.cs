using System.Linq;
using BranchMaker.Runtime;
using UnityEngine;

namespace BranchMaker
{
    public abstract class DialogueTyper : MonoBehaviour
    {
        private DialoguePreprocessor[] _preprocessors;
        protected static bool CurrentlyWriting;
        void Start()
        {
            StoryManager.Instance.OnBlockChange.AddListener(ProcessBlock);
            _preprocessors = GetComponents<DialoguePreprocessor>();
        }

        public bool BusyWriting() => CurrentlyWriting;

        private void ProcessBlock(BranchNodeBlock block)
        {
            var processedText = block.dialogue;
            processedText = _preprocessors.Aggregate(processedText, (current, preprocessor) => preprocessor.PreprocessDialogue(current, block));

            WriteDialogue(block, processedText);
        }
        protected virtual void WriteDialogue(BranchNodeBlock currentBlock, string dialogue)
        {
            throw new System.NotImplementedException();
        }
        
        
        public static bool AnyInputPressed()
        {
            bool pressed = false;

            // --- Old Input System (works if the old system is enabled) ---
            // This will just return false if the old system is disabled.
            if (Input.anyKey) pressed = true;

#if ENABLE_INPUT_SYSTEM
            // --- New Input System (only compiled when package is enabled) ---
            if (!pressed)
            {
                // Keyboard
                if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
                    pressed = true;

                // Mouse
                if (!pressed && Mouse.current != null &&
                    (Mouse.current.leftButton.wasPressedThisFrame ||
                     Mouse.current.rightButton.wasPressedThisFrame ||
                     Mouse.current.middleButton.wasPressedThisFrame))
                    pressed = true;

                // Gamepad
                if (!pressed && Gamepad.current != null &&
                    Gamepad.current.allControls.Exists(c => c is ButtonControl b && b.wasPressedThisFrame))
                    pressed = true;
            }
#endif

            return pressed;
        }
    }
}
