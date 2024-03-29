using System.Collections.Generic;
using System.Linq;
using BranchMaker;
using BranchMaker.Story;
using BranchMaker.UI;
using UnityEngine;
using UnityEngine.UI;

public class ButtonListManager : MonoBehaviour, IOptionHandler
{
    private static ButtonListManager manager;
    private List<DialogueButton> _actionButtons;
    public Sprite defaultActionIcon;
    public GameObject clickToContinue;
    [SerializeField] private bool blockUnsafeActions; 
    
    public void Awake()
    {
        manager = this;
        _actionButtons = GetComponentsInChildren<DialogueButton>(true).ToList();
    }

    public bool CanHandleBlock(BranchNodeBlock block) => true;

    public void ProcessNode(BranchNode node)
    {
        if (node == null) return;
        var buttonIndex = 0;
        Cleanup();

        if (StoryManager.HasSpeakingQueue() || StoryManager.IsCurrentlyWriting()) return;
        gameObject.SetActive(true);

        if (clickToContinue) clickToContinue.SetActive(false);

        foreach (var block in node.ActionBlocks())
        {
            if (StorySceneManager.SceneHasActionButton(block)) continue;
            if (!StoryEventManager.ValidBlockCheck(block)) continue;
            if (block.clean_action.StartsWith("#") && StoryManager.manager.HideScriptActions) continue;

            var buttonLabel = block.dialogue.CapitalizeFirst();
            if (block.dialogue.StartsWith("<")) buttonLabel = block.dialogue;
            if (!string.IsNullOrEmpty(block.meta_scripts))
            {
                if (block.meta_scripts.Contains("needword:"))
                    buttonLabel = "<color=#00FFFF>" + buttonLabel + "</color>";
            }

            manager._actionButtons[buttonIndex].gameObject.SetActive(true);
            manager._actionButtons[buttonIndex].SetLabel(buttonLabel);
            
            if (manager._actionButtons[buttonIndex].gameObject.transform.Find("Icon") != null)
            {
                var icon = StoryEventManager.BlockIcon(block);
                manager._actionButtons[buttonIndex].gameObject.transform.Find("Icon").GetComponent<Image>().sprite =
                    (icon == null ? manager.defaultActionIcon : icon);
            }

            if (blockUnsafeActions) manager._actionButtons[buttonIndex].GetComponent<Button>().interactable = block.isSafe;
            manager._actionButtons[buttonIndex].GetComponent<Button>().onClick.RemoveAllListeners();
            manager._actionButtons[buttonIndex].GetComponent<Button>().onClick.AddListener(
                () => { StoryManager.PerformAction(block); });
            buttonIndex++;
        }

        foreach (var dialogueOption in StoryManager.manager._customDialogueOptions)
        {
            dialogueOption.ProcessDialogueOptions(node);
        }
    }

    public void Cleanup()
    {
        foreach (var but in manager._actionButtons)
        {
            but.gameObject.SetActive(false);
        }
        gameObject.SetActive(false);
    }
}