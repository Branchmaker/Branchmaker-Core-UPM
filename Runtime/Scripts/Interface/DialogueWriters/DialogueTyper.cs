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
    }
}
