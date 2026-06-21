using BranchMaker.Interface.OptionHandlers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BranchMaker.Interface
{
    [RequireComponent(typeof(Button))]
    public sealed class DialogueButton : MonoBehaviour, IBranchDialogueButton
    {
        public TextMeshProUGUI TMLabel;
        public Button button;

        public void SetLabel(string newLabel, BranchNodeBlock fromBlock)
        {
            if (TMLabel != null) TMLabel.text = newLabel;
            if (GetComponent<Text>() != null) GetComponent<Text>().text = newLabel;
        }
        public void LoadBlock(BranchNodeBlock block, ButtonListManager manager)
        {
            button ??= GetComponent<Button>();
            if (manager.blockUnsafeActions) button.interactable = block.safe_for_playing;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(
                () => { StoryManager.PerformAction(block); });
        }
    }

    public interface IBranchDialogueButton
    {
        public void LoadBlock(BranchNodeBlock fromBlock, ButtonListManager manager);
    }
}