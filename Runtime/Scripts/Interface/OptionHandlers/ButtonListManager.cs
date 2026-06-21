using System.Collections.Generic;
using System.Linq;
using BranchMaker.GameScripts;
using BranchMaker.Runtime.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace BranchMaker.Interface.OptionHandlers
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ButtonListManager : MonoBehaviour, IOptionHandler
    {
        private List<DialogueButton> _actionButtons;
        private CanvasGroup _canvasGroup;
        [SerializeField] private  bool debugOutput;
        
        [Header("Filters")]
        public bool blockUnsafeActions;
        [SerializeField] private  bool hideScriptActions = true;
    
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
            if (!gameObject.activeInHierarchy) return;
            Cleanup();
        }

        public void ProcessNode(BranchNode node)
        {
            if (!gameObject.activeInHierarchy) return;
            if (node == null) return;
            var buttonIndex = 0;
            Cleanup();
            _canvasGroup.alpha = 1;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;

            var actions = node.ActionBlocks();

            if (debugOutput)
            {
                Debug.Log($"Node {node.id} has {actions.Count} actions");
            }

            foreach (var block in node.ActionBlocks())
            {
                Debug.Log($"Considering {block.id} {block.dialogue}");
                if (StorySceneManager.SceneHasActionButton(block)) continue;
                if (!StoryEventManager.ValidBlockCheck(block)) continue;
                if (block.clean_action.StartsWith("#") && hideScriptActions) continue;

                var buttonLabel = block.dialogue.CapitalizeFirst();
                if (block.dialogue.StartsWith("<")) buttonLabel = block.dialogue;
                
                if (!string.IsNullOrEmpty(block.meta_scripts))
                {
                    if (block.meta_scripts.Contains("needword:")) buttonLabel = "<color=#00FFFF>" + buttonLabel + "</color>";
                }

                _actionButtons[buttonIndex].gameObject.SetActive(true);
                _actionButtons[buttonIndex].SetLabel(buttonLabel, block);

                _actionButtons[buttonIndex].LoadBlock(block, this);
                buttonIndex++;
            }

            foreach (var dialogueOption in StoryManager.Instance.CustomDialogueOptions)
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
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
        }
    }
}