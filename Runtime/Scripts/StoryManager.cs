using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BranchMaker.Api;
using BranchMaker.LoadSave;
using BranchMaker.Runtime.Utility;
using BranchMaker.WebServices;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Events;

namespace BranchMaker
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
        [SerializeField] private bool verboseLogging;
        private readonly DialogueQueue _dialogueQueue = new();

        private static readonly Dictionary<string, BranchNode> NodeLib = new();

        [Header("Handlers")]
        static bool _loadingStory;
        public static BranchNode CurrentNode;
        public static BranchNodeBlock CurrentBlock;
        public BranchmakerCacheObject StoryCache;
        
        private static bool _reloadPurpose = true;

        private List<IWindowOverlay> _windowOverlays;
        public List<ICustomDialogueAction> CustomDialogueOptions;
        private static ILoadSaveHandler _loadSaveHandler;

        public bool RunStartingNodeAfterLoading = true;

        public bool HideScriptActions = true;

        [Header("Events")]
        [NonSerialized] public static UnityEvent OnStoryReady = new();
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
            NodeLib.Clear();
            StoryButton.playerkeys.Clear();
            
            _windowOverlays = FindObjectsOfType<MonoBehaviour>(true).OfType<IWindowOverlay>().ToList();
            CustomDialogueOptions = FindObjectsOfType<MonoBehaviour>(true).OfType<ICustomDialogueAction>().ToList();
            _loadSaveHandler = FindObjectsOfType<MonoBehaviour>(true).OfType<ILoadSaveHandler>().FirstOrDefault();
            
            
            if (loadFlow == LoadFlow.LoadOnLaunch) OnStoryReady.AddListener(LoadStartingNode);
        }
        
        private void Start()
        {
            LaunchWithBookKey(storybookId, startingNodeID);
        }

        public void LaunchWithBookKey(string newBookKey, string newStartingNode = null)
        {
            if (!newBookKey.IsValidUUID()) return;
            startingNodeID = newStartingNode;
            storybookId = newBookKey;
            _ = GetAllTheNodes();
        }

        public static List<BranchNode> AllNodes() => NodeLib.Values.ToList();

        private async Task GetAllTheNodes()
        {
            _loadingStory = true;

            var result = string.Empty;
            if (StoryCache && !string.IsNullOrEmpty(StoryCache.cacheData))
            {
                result = StoryCache.cacheData;
            }
            else
            {
                result = await FetchStoryFeed();
            }

            var allNodes = JSONNode.Parse(result);
            foreach (var storyNode in allNodes["nodes"]) ProcessIncomingNode(BranchNode.createFromJson(storyNode));

            Log(NodeLib.Count+" nodes in NodeLib");
            foreach (var block in NodeLib.Values.SelectMany(node => node.blocks))
            {
                StoryEventManager.PreloadScriptCheck(block);
            }

            OnStoryReady.Invoke();
            _loadingStory = false;
        }

        private async Task<string> FetchStoryFeed()
        {
            var path = BranchmakerPaths.StoryNodes(loadFromPublished, storybookId);
            var result = await APIRequest.FetchFromApi(
                path,
                "story"
            );
            return result;
        }

        private void LoadStartingNode()
        {
            if (CurrentNode != null)
            {
                var startingNode = NodeLib.Values.FirstOrDefault(node => node.id == CurrentNode.id);
                LoadNode(startingNode);
                return;
            }
            
            Log("Looking for starting node");
            if (string.IsNullOrEmpty(startingNodeID)) startingNodeID = NodeLib.First().Key;

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
            if (string.IsNullOrEmpty(key)) return;
            Instance.Log("Loading node: "+key);
            if (!NodeLib.ContainsKey(key))
            {
                Instance.LogError("Could not find node: "+key);
                return;
            }
            LoadNode(NodeLib[key]);
        }
        

        public static BranchNode GetNodeById(string id)
        {
            return NodeLib.GetValueOrDefault(id);
        }

        private static void LoadNode(BranchNode node)
        {
            if (Instance.verboseLogging) Instance.Log("Loading node "+node);
            CurrentNode = node;
            Instance._dialogueQueue.Clear();
            Instance.OnNodeChange.Invoke(node);
            Instance._dialogueQueue.LoadBlocks(node.StoryBlocks());
            
            Instance.SpeakActiveNode();
            node.processed = true;
            _loadSaveHandler?.UpdateSaveFile();
        }

        public static void ProcessIncomingNode(BranchNode bNode)
        {
            NodeLib.TryAdd(bNode.id, bNode);
            bNode.processed = false;
        }

        public static bool HasSpeakingQueue() => Instance._dialogueQueue.Count() > 0;
    }
}