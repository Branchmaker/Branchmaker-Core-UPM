using System;
using System.Collections.Generic;
using System.Linq;
using BranchMaker;
using BranchMaker.Runtime;
using BranchMaker.Runtime.Utility;
using BranchMaker.Story;
using BranchMaker.UI;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class ButtonListManager : MonoBehaviour, IOptionHandler
{
    private List<DialogueButton> _actionButtons;
    public Sprite defaultActionIcon;
    [SerializeField] private bool blockUnsafeActions;
    private CanvasGroup _canvasGroup;
    
    public void Awake()
    {
        _actionButtons = GetComponentsInChildren<DialogueButton>(true).ToList();
        _canvasGroup = GetComponent<CanvasGroup>();
        Cleanup();
    }

    private void Start()
    {
        StoryManager.Instance.OnNodeComplete.AddListener(ProcessNode);
        StoryManager.Instance.OnNodeChange.AddListener(NodeChanged);
    }

    public bool CanHandleBlock(BranchNodeBlock block) => true;

    private void NodeChanged(BranchNode node)
    {
        if (gameObject.activeInHierarchy) return;
        Cleanup();
    }

    public void ProcessNode(BranchNode node)
    {
        if (gameObject.activeInHierarchy) return;
        if (node == null) return;
        var buttonIndex = 0;
        Cleanup();
        _canvasGroup.alpha = 1;

        foreach (var block in node.ActionBlocks())
        {
            if (StorySceneManager.SceneHasActionButton(block)) continue;
            if (!StoryEventManager.ValidBlockCheck(block)) continue;
            if (block.clean_action.StartsWith("#") && StoryManager.Instance.HideScriptActions) continue;

            var buttonLabel = block.dialogue.CapitalizeFirst();
            if (block.dialogue.StartsWith("<")) buttonLabel = block.dialogue;
            if (!string.IsNullOrEmpty(block.meta_scripts))
            {
                if (block.meta_scripts.Contains("needword:"))
                    buttonLabel = "<color=#00FFFF>" + buttonLabel + "</color>";
            }

            _actionButtons[buttonIndex].gameObject.SetActive(true);
            _actionButtons[buttonIndex].SetLabel(buttonLabel);
            
            if (_actionButtons[buttonIndex].gameObject.transform.Find("Icon") != null)
            {
                var icon = StoryEventManager.BlockIcon(block);
                _actionButtons[buttonIndex].gameObject.transform.Find("Icon").GetComponent<Image>().sprite =
                    (icon == null ? defaultActionIcon : icon);
            }

            if (blockUnsafeActions) _actionButtons[buttonIndex].GetComponent<Button>().interactable = block.isSafe;
            _actionButtons[buttonIndex].GetComponent<Button>().onClick.RemoveAllListeners();
            _actionButtons[buttonIndex].GetComponent<Button>().onClick.AddListener(
                () => { StoryManager.PerformAction(block); });
            buttonIndex++;
        }

        foreach (var dialogueOption in StoryManager.Instance._customDialogueOptions)
        {
            dialogueOption.ProcessDialogueOptions(node);
        }
    }

    public void Cleanup()
    {
        foreach (var but in _actionButtons)
        {
            but.gameObject.SetActive(false);
        }
        _canvasGroup.alpha = 0;
    }
}