using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System.IO;
using System.Linq;
using BranchMaker.Actors;
using BranchMaker.Api;
using BranchMaker.LoadSave;
using Debug = UnityEngine.Debug;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace BranchMaker.Story
{
    public class StoryManager : MonoBehaviour
    {
        public enum LoadFlow
        {
            LoadOnLaunch,
            AwaitLoadCommand
        };

        public static StoryManager manager;
        public static BranchMakerCloudSave forceLoad;

        [Header("API Configuration")]
        [SerializeField] private LoadFlow loadFlow;
        [SerializeField] private string storybookId = "Place Storybook API key here";
        [SerializeField] private string startingNodeID;
        [SerializeField] private bool loadFromPublished = true;
        [SerializeField] private bool debugLog = true;
        private static List<BranchNodeBlock> _speakQueue = new();

        private static Dictionary<string, BranchNode> _nodeLib = new();

        [Header("Handlers")]
        static bool _loadingStory;
        //public Sprite[] IconSprites;
        public static BranchNode Currentnode;

        private static float _actionCooldown;
        private static bool _reloadPurpose = true;

        private List<IDialogueHandler> _dialogueHandlers;
        private List<IWindowOverlay> _windowOverlays;
        public List<ICustomDialogueAction> _customDialogueOptions;
        private List<IOptionHandler> _optionHandlers;
        private List<IActorHandler> _actorHandlers;
        static ILoadSaveHandler _loadSaveHandler;

        public bool HideScriptActions = true;

        [Header("Events")]
        [SerializeField] private UnityEvent OnStoryReady;
        [SerializeField] public UnityEvent<BranchNode> OnNodeChange;
        [SerializeField] private UnityEvent<BranchNodeBlock> OnBlockChange;
        
        public static string ActiveStoryId => manager.storybookId;
        
        public void Awake()
        {
            manager = this;
            Currentnode = null;
            _reloadPurpose = true;
            _nodeLib.Clear();
            StoryButton.playerkeys.Clear();
            
            _windowOverlays = FindObjectsOfType<MonoBehaviour>(true).OfType<IWindowOverlay>().ToList();
            _actorHandlers = FindObjectsOfType<MonoBehaviour>(true).OfType<IActorHandler>().ToList();
            _dialogueHandlers = FindObjectsOfType<MonoBehaviour>(true).OfType<IDialogueHandler>().ToList();
            _customDialogueOptions = FindObjectsOfType<MonoBehaviour>(true).OfType<ICustomDialogueAction>().ToList();
            _optionHandlers = FindObjectsOfType<MonoBehaviour>(true).OfType<IOptionHandler>().ToList();
            _loadSaveHandler = FindObjectsOfType<MonoBehaviour>(true).OfType<ILoadSaveHandler>().FirstOrDefault();
            
            _actorHandlers.ForEach(a => a.ResetActors());
        }
        
        private void Start()
        {
            _optionHandlers.ForEach(a => a.Cleanup());
            if (loadFlow == LoadFlow.LoadOnLaunch) ForceRefresh();
        }

        public void LaunchWithBookKey(string newBookKey)
        {
            Debug.Log("Hello "+newBookKey);
            if (newBookKey.Length != 36) return;
            if (loadFlow == LoadFlow.AwaitLoadCommand)
            {
                startingNodeID = null;
                storybookId = newBookKey;
                ForceRefresh();
            }
        }

        public static List<BranchNode> AllNodes()
        {
            return _nodeLib.Values.ToList();
        }

        public static void ForceRefresh()
        {
            if (!_loadingStory) manager.StartCoroutine(manager.GetAllTheNodes());
        }


        private IEnumerator GetAllTheNodes()
        {
            foreach (var handler in manager._dialogueHandlers) handler.WriteDialogue(null, "Loading...");
            _loadingStory = true;
            var content = "";
            var fetch = UnityWebRequest.Get(BranchmakerPaths.StoryNodes(loadFromPublished,storybookId));
            fetch.SetRequestHeader("Cache-Control", "max-age=0, no-cache, no-store");
            fetch.SetRequestHeader("Pragma", "no-cache");
            yield return fetch.SendWebRequest();
            
            var backupFileName = Application.persistentDataPath + "/" + storybookId + ".txt";
            if (!string.IsNullOrEmpty(fetch.error))
            {
                if (File.Exists(backupFileName))
                {
                    content = File.ReadAllText(backupFileName);
                }
                else
                {
                    foreach (var handler in manager._dialogueHandlers) handler.WriteDialogue(null, "Could not reach BranchMaker server, please check your internet connection...");
                }
            }
            else
            {
                var writer = new StreamWriter(backupFileName, false);
                writer.Write(fetch.downloadHandler.text);
                writer.Close();
                content = fetch.downloadHandler.text;
            }

            var allNodes = JSONNode.Parse(content);
            foreach (var storyNode in allNodes["nodes"]) ProcessIncomingNode(BranchNode.createFromJson(storyNode));
            
            foreach (var block in _nodeLib.Values.SelectMany(node => node.blocks))
            {
                StoryEventManager.PreloadScriptCheck(block);
            }

            if (Currentnode != null)
            {
                foreach (var node in _nodeLib.Values.Where(node => node.id == Currentnode.id))
                {
                    LoadNode(node);
                    break;
                }
            }

            _loadingStory = false;

            startingNodeID ??= _nodeLib.First().Key;
            
            if (forceLoad != null) {
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
        }

        private static bool Busy()
        {
            if (_loadingStory || manager == null) return true;
            return manager._windowOverlays.Any(overlay => overlay.WindowOverlayOpen());
        }

        public static bool IsCurrentlyWriting()
        {
            return manager._dialogueHandlers.Any(dialogueHandler => dialogueHandler.BusyWriting());
        }

        private void Update()
        {
            if (Busy()) return;

            if (IsCurrentlyWriting())
            {
                _actionCooldown = 0.1f;
                return;
            }

            if (_actionCooldown > 0)
            {
                _actionCooldown -= Time.deltaTime;
                return;
            }
            
            if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.Return) || Input.GetMouseButtonDown(0))
            {
                SpeakActiveNode();
            }

            if (Input.GetKeyUp(KeyCode.F5)) ForceReloadFromServer();
        }

        private void ForceReloadFromServer()
        {
            _speakQueue.Clear();
            if (!_loadingStory) manager.StartCoroutine(manager.GetAllTheNodes());
            loadFromPublished = false;
            StartCoroutine(GetAllTheNodes());
        }

        private static void SpeakActiveNode()
        {
            if (_speakQueue.Count <= 0)
            {
                BuildButtons();
                return;
            }
            var activeBlock = _speakQueue[0];
            _speakQueue.RemoveAt(0);
            RemoteVoicePlayer.StopSpeaking();

            var canRun = StoryEventManager.ValidBlockCheck(activeBlock);

            if (canRun)
            {
                manager.OnBlockChange.Invoke(activeBlock);
                var dialogue = activeBlock.dialogue;
                StoryEventManager.ParseBlockscript(activeBlock);

                if (!string.IsNullOrEmpty(activeBlock.character))
                {
                    var actor = ActorDatabase.ActorByKey(activeBlock.character);
                    if (actor != null)
                    {
                        if (!string.IsNullOrEmpty(activeBlock.emotion)) actor.CurrentEmotion = activeBlock.emotion;
                        manager._actorHandlers.ForEach(a => a.ActorUpdate(activeBlock.character, activeBlock));
                        dialogue = "<color=#" + ColorUtility.ToHtmlStringRGB(actor.themeColor) + ">" + actor.displayName + "</color>\n" + dialogue;
                    }
                }

                if (!string.IsNullOrEmpty(activeBlock.voice_file))
                    RemoteVoicePlayer.PlayRemoteOgg(activeBlock.voice_file);

                foreach (var handler in manager._dialogueHandlers) handler.WriteDialogue(activeBlock, dialogue);
            }
            else
            {
                SpeakActiveNode();
            }

        }

        public static void PerformAction(BranchNodeBlock action)
        {
            if (_actionCooldown > 0) return;
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
            if (!_nodeLib.ContainsKey(key)) return;
            LoadNode(_nodeLib[key]);
        }

        private static void LoadNode(BranchNode node)
        {
            _actionCooldown = .6f;
            Currentnode = node;
            _speakQueue.Clear();
            manager.OnNodeChange.Invoke(node);
            manager._optionHandlers.ForEach(a => a.Cleanup());
            
            foreach (var block in node.StoryBlocks())
            {
                _speakQueue.Add(block);
            }
            SpeakActiveNode();

            node.processed = true;
            _loadSaveHandler?.UpdateSaveFile();
            BuildButtons();
        }

        private static void ProcessIncomingNode(BranchNode bNode)
        {
            if (!_nodeLib.ContainsKey(bNode.id)) _nodeLib.Add(bNode.id, bNode);
            bNode.processed = false;
        }

        public static void BuildButtons()
        {
            manager._optionHandlers.ForEach(a => a.ProcessNode(Currentnode));
        }

        public static bool HasSpeakingQueue()
        {
            return _speakQueue.Count > 0;
        }

        protected static void Log(string log)
        {
            if (manager.debugLog) Debug.Log("<color=#00FFFF><b>StoryManager</b></color>: "+log);
        }
    }
}