using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;
using System.IO;
using BranchMaker.UI;
using System.Linq;
using BranchMaker.Actors;
using BranchMaker.Api;
using BranchMaker.LoadSave;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace BranchMaker.Story
{
    public class StoryManager : MonoBehaviour
    {
        public static StoryManager manager;
        public static BranchMakerCloudSave forceLoad;

        [Header("API Configuration")]
        public string Bookkey = "Place Storybook API key here";
        [SerializeField] private string StartingNode;
        [SerializeField] private bool loadFromPublished = true;
        public static BranchNode currentnode;

        static float actionCooldown = 0f;

        private static List<BranchNodeBlock> _speakQueue = new();

        private static Dictionary<string, BranchNode> _nodeLib = new();

        [Header("Handlers")]

        static bool gameover;
        static bool loadingStory;
        public GameObject clickToContinue;
        public Sprite[] IconSprites;

        static float clickCooldown = 0f;

        private static bool reloadPurpose = true;

        private List<string> _seenNodes = new();

        private List<IDialogueHandler> _dialogueHandlers;
        private List<IWindowOverlay> _windowOverlays;
        public List<ICustomDialogueAction> _customDialogueOptions;
        private List<IOptionHandler> _optionHandlers;
        private List<IActorHandler> _actorHandlers;
        static ILoadSaveHandler _loadSaveHandler;

        public bool HideScriptActions = true;
        
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
            
            if (clickToContinue != null) clickToContinue.SetActive(false);
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

        private void Start()
        {
            _optionHandlers.ForEach(a => a.Cleanup());
            if (!loadingStory) StartCoroutine(GetAllTheNodes());
        }

        public static void ForceRefresh()
        {
            if (!loadingStory) manager.StartCoroutine(manager.GetAllTheNodes());
        }


        private IEnumerator GetAllTheNodes()
        {
            loadingStory = true;
            var content = "";
            //yield return new WaitForEndOfFrame();
            
            /*
            UnityWebRequest webcall = UnityWebRequest.Get("https://branchmaker.com/api/steam/suggestion", formData);
            webcall.SetRequestHeader("Cache-Control", "max-age=0, no-cache, no-store");
            webcall.SetRequestHeader("Pragma", "no-cache");
            */
            
#pragma warning disable 612, 618
            var nodefetcher = new WWW(BranchmakerPaths.StoryNodes(loadFromPublished,Bookkey));
            nodefetcher.threadPriority = ThreadPriority.High;
#pragma warning restore 612, 618
            
            yield return nodefetcher;

            var backupFileName = Application.persistentDataPath + "/" + Bookkey + ".txt";

            if (!string.IsNullOrEmpty(nodefetcher.error))
            {
                Debug.LogError("Fetch error : (" + nodefetcher.url + ") " + nodefetcher.error);
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
                writer.Write(nodefetcher.text);
                writer.Close();
                content = nodefetcher.text;
            }

            var allthenodes = JSONNode.Parse(content);
            foreach (var storynode in allthenodes["nodes"]) ProcessIncomingNode(BranchNode.createFromJson(storynode));

            loadingStory = false;

            if (forceLoad != null) {
                StartingNode = forceLoad.currentNode;
                forceLoad.Resume();
                StorySceneManager.ShowPotentialScene(forceLoad.backgroundScene);
                forceLoad = null;
            }

            if (reloadPurpose)
            {
                LoadNodeKey(StartingNode);
                reloadPurpose = false;
            }
        }

        private static bool Busy()
        {
            if (loadingStory) return true;
            if (gameover) return true;
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
            }
            else
            {
                if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.Return) || Input.GetMouseButtonDown(0))
                {
                    SpeakActiveNode();
                }
            }

            if (Input.GetKeyUp(KeyCode.F5))
            {
                ForceReloadFromServer();
            }
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
            RemoteVoicePlayer.StopSpeaking();
                
            if (activeBlock.meta_scripts.Contains("hide:dialogue"))
            {
                _speakQueue.RemoveAt(0);
                return;
            }
                
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
            if (!string.IsNullOrEmpty(activeBlock.voice_file)) RemoteVoicePlayer.PlayRemoteOgg(activeBlock.voice_file);

            foreach (var handler in manager._dialogueHandlers) handler.WriteDialogue(activeBlock, dialogue);
            
            _speakQueue.RemoveAt(0);
        }

        public static void LoadNodeKey(string key)
        {
            if (clickCooldown > 0) return;
            if (!_nodeLib.ContainsKey(key)) return;
            clickCooldown = 0.2f;
            manager._optionHandlers.ForEach(a => a.Cleanup());
            if (key == "okay")
            {
                _speakQueue.Clear();
                return;
            }
            LoadNode(_nodeLib[key]);
        }

        private static void LoadNode(BranchNode node)
        {
            actionCooldown = .6f;
            currentnode = node;
            _speakQueue.Clear();
            //Debug.Log("Load node "+node.id+ " blocks "+node.blocks.Count);
            foreach (var block in node.StoryBlocks())
            {
                if (!StoryEventManager.ValidBlockCheck(block)) continue;
                if (block.meta_scripts.Contains("dontrepeat"))
                {
                    if (manager._seenNodes.Contains(block.id)) continue;
                    manager._seenNodes.Add(block.id);
                }
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

            if (currentnode != null && bNode.id == currentnode.id)
            {
                LoadNode(bNode);
            }
            
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