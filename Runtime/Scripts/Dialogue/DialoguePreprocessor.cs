using UnityEngine;

namespace BranchMaker
{
    public abstract class DialoguePreprocessor : MonoBehaviour
    {
        public abstract string PreprocessDialogue(string input, BranchNodeBlock block);
    }
}