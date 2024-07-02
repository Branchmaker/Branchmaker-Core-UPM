using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BranchMaker.Api;
using BranchMaker.LoadSave;
using BranchMaker.Runtime.Utility;
using BranchMaker.Story;
using BranchMaker.WebServices;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Events;

namespace BranchMaker.Runtime
{
    public class StoryManager : BaseController<StoryManager>
    {
        public enum LoadFlow
        {
            LoadOnLaunch,
            AwaitLoadCommand
        };

        public static BranchMakerCloudSave forceLoad;

        [Header("API Configuration")]
        [SerializeField] private LoadFlow loadFlow;
        [SerializeField] private string storybookId = "Place Storybook API key here";
        [SerializeField] private string startingNodeID;
        [SerializeField] private bool loadFromPublished = true;
        private readonly DialogueQueue _dialogueQueue = new();

        private static Dictionary<string, BranchNode> _nodeLib = new();

        [Header("Handlers")]
        static bool _loadingStory;
        public static BranchNode CurrentNode;
        public static BranchNodeBlock CurrentBlock;
        
        private static bool _reloadPurpose = true;

        private List<IWindowOverlay> _windowOverlays;
        public List<ICustomDialogueAction> _customDialogueOptions;
        static ILoadSaveHandler _loadSaveHandler;

        public bool HideScriptActions = true;

        [Header("Events")]
        [NonSerialized] public readonly UnityEvent OnStoryReady = new();
        [NonSerialized] public readonly UnityEvent<BranchNode> OnNodeChange = new();
        [NonSerialized] public readonly UnityEvent<BranchNode> OnNodeComplete = new();
        [NonSerialized] public readonly UnityEvent<BranchNodeBlock> OnBlockChange = new();
        [NonSerialized] public readonly UnityEvent<BranchNodeBlock> OnBlockComplete = new();
        
        public static string ActiveStoryId => Instance.storybookId;

        protected override void Awake()
        {
            base.Awake();
            CurrentNode = null;
            _reloadPurpose = true;
            _nodeLib.Clear();
            StoryButton.playerkeys.Clear();
            
            _windowOverlays = FindObjectsOfType<MonoBehaviour>(true).OfType<IWindowOverlay>().ToList();
            _customDialogueOptions = FindObjectsOfType<MonoBehaviour>(true).OfType<ICustomDialogueAction>().ToList();
            _loadSaveHandler = FindObjectsOfType<MonoBehaviour>(true).OfType<ILoadSaveHandler>().FirstOrDefault();
        }
        
        private void Start()
        {
            if (loadFlow == LoadFlow.LoadOnLaunch) LaunchWithBookKey(storybookId, startingNodeID);
        }

        public void LaunchWithBookKey(string newBookKey, string newStartingNode = null)
        {
            if (!newBookKey.IsValidUUID()) return;
            startingNodeID = newStartingNode;
            storybookId = newBookKey;
            _ = GetAllTheNodes();
        }

        public static List<BranchNode> AllNodes() => _nodeLib.Values.ToList();

        private async Task GetAllTheNodes()
        {
            _loadingStory = true;

            var result = await APIRequest.FetchFromApi(
                BranchmakerPaths.StoryNodes(loadFromPublished, storybookId),
                "story"
            );

            var allNodes = JSONNode.Parse(result);
            foreach (var storyNode in allNodes["nodes"]) ProcessIncomingNode(BranchNode.createFromJson(storyNode));

            foreach (var block in _nodeLib.Values.SelectMany(node => node.blocks))
            {
                StoryEventManager.PreloadScriptCheck(block);
            }

            if (CurrentNode != null)
            {
                foreach (var node in _nodeLib.Values.Where(node => node.id == CurrentNode.id))
                {
                    LoadNode(node);
                    break;
                }
            }

            _loadingStory = false;

            startingNodeID ??= _nodeLib.First().Key;

            if (forceLoad != null)
            {
                startingNodeID = forceLoad.currentNode;
                forceLoad.Resume();
                forceLoad = null;
            }

            if (_reloadPurpose)
            {
                LoadNodeKey(startingNodeID);
                _reloadPurpose = false;
            }

            OnStoryReady.Invoke();
            Log("story is ready");
        }


        public static bool Busy()
        {
            if (_loadingStory || !Instance) return true;
            return Instance._windowOverlays.Any(overlay => overlay.WindowOverlayOpen());
        }

        public void ForceReloadFromServer()
        {
            _dialogueQueue.Clear();
            loadFromPublished = false;
            _ = GetAllTheNodes();
        }

        public void DoneRenderingBlock()
        {
            OnBlockComplete.Invoke(CurrentBlock);
            if (HasSpeakingQueue()) return;
            OnNodeComplete.Invoke(CurrentNode);
        }

        public void SpeakActiveNode()
        {
            if (_dialogueQueue.Count() <= 0)
            {
                Instance.OnNodeComplete.Invoke(CurrentNode);
                return;
            }
            CurrentBlock = _dialogueQueue.PopFirst();
            
            var canRun = StoryEventManager.ValidBlockCheck(CurrentBlock);
            if (canRun)
            {
                OnBlockChange.Invoke(CurrentBlock);
                StoryEventManager.ParseBlockscript(CurrentBlock);
            }
            else
            {
                SpeakActiveNode();
            }
        }

        public static void PerformAction(BranchNodeBlock action)
        {
            if (!StoryEventManager.ValidateActionBlock(action)) return;
            StoryEventManager.ParseBlockscript(action);
            if (StoryEventManager.SkipNextActionNodeChange)
            {
                StoryEventManager.SkipNextActionNodeChange = false;
                return; 
            } 
            LoadNodeKey(action.target_node);
        }

        public static void LoadNodeKey(string key)
        {
            if (!_nodeLib.ContainsKey(key))
            {
                Instance.LogError("Could not find that node."+key);
                return;
            }
            LoadNode(_nodeLib[key]);
        }

        private static void LoadNode(BranchNode node)
        {
            CurrentNode = node;
            Instance._dialogueQueue.Clear();
            Instance.OnNodeChange.Invoke(node);
            Instance._dialogueQueue.LoadBlocks(node.StoryBlocks());
            
            Instance.SpeakActiveNode();

            node.processed = true;
            _loadSaveHandler?.UpdateSaveFile();
        }

        private static void ProcessIncomingNode(BranchNode bNode)
        {
            if (!_nodeLib.ContainsKey(bNode.id)) _nodeLib.Add(bNode.id, bNode);
            bNode.processed = false;
        }


        public bool HasSpeakingQueue() => Instance._dialogueQueue.Count() > 0;
    }
}