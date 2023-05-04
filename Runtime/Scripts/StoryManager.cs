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
using UnityEngine.Serialization;

namespace BranchMaker.Story
{
    public class StoryManager : MonoBehaviour
    {
        public static StoryManager manager;
        public static BranchMakerCloudSave forceLoad;

        [Header("API Configuration")]
        public string storybookId = "Place Storybook API key here";
        [SerializeField] private string startingNodeID;
        [SerializeField] private bool loadFromPublished = true;
        private static List<BranchNodeBlock> _speakQueue = new();

        private static Dictionary<string, BranchNode> _nodeLib = new();

        [Header("Handlers")]
        static bool loadingStory;
        public Sprite[] IconSprites;
        public static BranchNode currentnode;

        private static float actionCooldown;
        private static float clickCooldown;
        private static bool reloadPurpose = true;

        private List<IDialogueHandler> _dialogueHandlers;
        private List<IWindowOverlay> _windowOverlays;
        public List<ICustomDialogueAction> _customDialogueOptions;
        private List<IOptionHandler> _optionHandlers;
        private List<IActorHandler> _actorHandlers;
        static ILoadSaveHandler _loadSaveHandler;

        public bool HideScriptActions = true;

        [Header("Events")]
        [SerializeField] private UnityEvent<BranchNode> OnNodeChange;
        [SerializeField] private UnityEvent<BranchNodeBlock> OnBlockChange;
        
        private void Start()
        {
            _optionHandlers.ForEach(a => a.Cleanup());
            if (!loadingStory) StartCoroutine(GetAllTheNodes());
        }
        public void Awake()
        {
            manager = this;
            currentnode = null;
            reloadPurpose = true;
            _nodeLib.Clear();
            StoryButton.playerkeys.Clear();
            
            _windowOverlays = FindObjectsOfType<MonoBehaviour>(true).OfType<IWindowOverlay>().ToList();
            _actorHandlers = FindObjectsOfType<MonoBehaviour>(true).OfType<IActorHandler>().ToList();
            _dialogueHandlers = FindObjectsOfType<MonoBehaviour>(true).OfType<IDialogueHandler>().ToList();
            _customDialogueOptions = FindObjectsOfType<MonoBehaviour>(true).OfType<ICustomDialogueAction>().ToList();
            _optionHandlers = FindObjectsOfType<MonoBehaviour>(true).OfType<IOptionHandler>().ToList();
            _loadSaveHandler = FindObjectsOfType<MonoBehaviour>(true).OfType<ILoadSaveHandler>().First();
            
            _actorHandlers.ForEach(a => a.ResetActors());
        }

        public static List<BranchNode> AllNodes()
        {
            return _nodeLib.Values.ToList();
        }

        private void FixedUpdate()
        {
            if (clickCooldown > 0)
            {
                clickCooldown -= Time.deltaTime;
            }
        }


        public static void ForceRefresh()
        {
            if (!loadingStory) manager.StartCoroutine(manager.GetAllTheNodes());
        }


        private IEnumerator GetAllTheNodes()
        {
            foreach (var handler in manager._dialogueHandlers) handler.WriteDialogue(null, "Loading...");

            loadingStory = true;
            var content = "";
            
            var fetch = UnityWebRequest.Get(BranchmakerPaths.StoryNodes(loadFromPublished,storybookId));
            fetch.SetRequestHeader("Cache-Control", "max-age=0, no-cache, no-store");
            fetch.SetRequestHeader("Pragma", "no-cache");
            yield return fetch.SendWebRequest();

            var backupFileName = Application.persistentDataPath + "/" + storybookId + ".txt";

            if (!string.IsNullOrEmpty(fetch.error))
            {
                Debug.LogError("Fetch error : (" + fetch.url + ") " + fetch.error);
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

            loadingStory = false;

            if (forceLoad != null) {
                startingNodeID = forceLoad.currentNode;
                forceLoad.Resume();
                StorySceneManager.ShowPotentialScene(forceLoad.backgroundScene);
                forceLoad = null;
            }

            if (reloadPurpose)
            {
                LoadNodeKey(startingNodeID);
                reloadPurpose = false;
            }
        }

        private static bool Busy()
        {
            if (loadingStory) return true;
            if (manager == null) return true;

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
                actionCooldown = 0.1f;
                return;
            }

            if (actionCooldown > 0)
            {
                actionCooldown -= Time.deltaTime;
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
            if (!loadingStory) manager.StartCoroutine(manager.GetAllTheNodes());
            loadFromPublished = false;
            StartCoroutine(GetAllTheNodes());
        }

        private static void SpeakActiveNode()
        {
            if (_speakQueue.Count <= 0) return;
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
                        dialogue = "<color=#" + ColorUtility.ToHtmlStringRGB(actor.themeColor) + ">" +
                                   actor.displayName + "</color>\n" + dialogue;
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
            if (clickCooldown > 0) return;
            clickCooldown = 0.2f;
            if (!StoryEventManager.ValidateActionBlock(action)) return;
            StoryEventManager.ParseBlockscript(action);
            LoadNodeKey(action.target_node);
        }

        public static void LoadNodeKey(string key)
        {
            if (!_nodeLib.ContainsKey(key)) return;
            LoadNode(_nodeLib[key]);
        }

        private static void LoadNode(BranchNode node)
        {
            actionCooldown = .6f;
            currentnode = node;
            _speakQueue.Clear();
            manager.OnNodeChange.Invoke(node);
            manager._optionHandlers.ForEach(a => a.Cleanup());
            
            foreach (var block in node.StoryBlocks())
            {
                _speakQueue.Add(block);
            }
            StorySceneManager.ShowPotentialScene(node.id);
            SpeakActiveNode();

            node.processed = true;
            _loadSaveHandler.UpdateSaveFile();
            manager._optionHandlers.ForEach(a => a.ProcessNode(currentnode));
        }

        private static void ProcessIncomingNode(BranchNode bNode)
        {
            if (!_nodeLib.ContainsKey(bNode.id)) _nodeLib.Add(bNode.id, bNode);
            bNode.processed = false;

            if (currentnode != null && bNode.id == currentnode.id) LoadNode(bNode);

            foreach (var block in bNode.blocks)
            {
                StoryEventManager.PreloadScriptCheck(block);
            }
        }

        public static void BuildButtons()
        {
            manager._optionHandlers.ForEach(a => a.ProcessNode(currentnode));
        }

        public static bool HasSpeakingQueue()
        {
            return _speakQueue.Count > 0;
        }
    }
}