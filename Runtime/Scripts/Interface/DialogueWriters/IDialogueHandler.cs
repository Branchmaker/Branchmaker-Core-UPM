using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BranchMaker
{
    public interface IDialogueHandler
    {
        public void WriteDialogue(BranchNodeBlock block, string processedText);
        public bool BusyWriting();
    }
}
