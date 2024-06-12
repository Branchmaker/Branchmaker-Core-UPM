using BranchMaker.Runtime;
using UnityEngine;

namespace BranchMaker
{
    public abstract class DialogueTyper : MonoBehaviour
    {
        protected static bool CurrentlyWriting;
        void Start()
        {
            StoryManager.Instance.OnBlockChange.AddListener(ProcessBlock);
        }

        public bool BusyWriting()
        {
            return CurrentlyWriting;
        }
        private void ProcessBlock(BranchNodeBlock block)
        {
            WriteDialogue(block, block.dialogue);
        }
        protected virtual void WriteDialogue(BranchNodeBlock currentBlock, string dialogue)
        {
            throw new System.NotImplementedException();
        }
    }
}
