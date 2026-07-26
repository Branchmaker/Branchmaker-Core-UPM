using BranchMaker.Interface.OptionHandlers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BranchMaker.Interface
{
    [RequireComponent(typeof(Button))]
    public class DialogueButton : MonoBehaviour, IBranchDialogueButton
    {
        public TextMeshProUGUI TMLabel;
        public Text LegacyTextLabel;
        public Button button;

        public virtual void SetLabel(string newLabel, BranchNodeBlock fromBlock)
        {
            if (TMLabel)
            {
                TMLabel.text = newLabel;
                return;
            }
            if (LegacyTextLabel) LegacyTextLabel.text = newLabel;
        }
        public void LoadBlock(BranchNodeBlock block, ButtonListManager manager)
        {
            button ??= GetComponent<Button>();
            if (manager.blockUnsafeActions) button.interactable = block.safe_for_playing;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => { ExecuteNode(block); });
        }

        protected virtual void ExecuteNode(BranchNodeBlock block)
        {
            StoryManager.PerformAction(block);
        }
    }

    public interface IBranchDialogueButton
    {
        public void LoadBlock(BranchNodeBlock fromBlock, ButtonListManager manager);
    }
}