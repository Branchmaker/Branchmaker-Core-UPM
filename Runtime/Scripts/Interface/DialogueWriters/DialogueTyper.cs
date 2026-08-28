using System.Linq;
using UnityEngine;

namespace BranchMaker.Interface.DialogueWriters
{
    public abstract class DialogueTyper : MonoBehaviour
    {
        private DialoguePreprocessor[] _preprocessors;
        protected static bool CurrentlyWriting;

        private void Awake()
        {
            Prepare();
        }

        public virtual void Prepare()
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
