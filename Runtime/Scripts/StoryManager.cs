using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;
using System.IO;
using BranchMaker.UI;
using System.Linq;
using BranchMaker.Api;
using BranchMaker.LoadSave;
using Debug = UnityEngine.Debug;

namespace BranchMaker.Story
{
    public class StoryManager : MonoBehaviour
    {
        public static StoryManager manager;
        public static BranchMakerCloudSave forceLoad;

        [Header("API Configuration")]
        public string bookkey = "Place Storybook API key here";
        public string startingNode;
        public bool loadFromPublished = true;
        public static BranchNode currentnode;

        static float actionCooldown = 0f;

        private static List<BranchNodeBlock> speakQueue = new List<BranchNodeBlock>();

        private static Dictionary<string, BranchNode> nodeLib = new Dictionary<string, BranchNode>();

        [Header("Handlers")]
        public GameObject[] dialogueHandlers;

        static bool gameover;
        private List<DialogueButton> _actionButtons;
        static bool loadingStory;
        public GameObject dialogueWindow;
        public GameObject clickToContinue;

        [Header("Speaker portrait plugin")]
        public Image speakerPortrait;
        public Sprite[] faces;
        public Sprite defaultActionIcon;

        static float clickCooldown = 0f;

        private static bool reloadPurpose = true;

        private List<string> _seenNodes = new List<string>();

        private List<IWindowOverlay> _windowOverlays;
        private List<ICustomDialogueAction> _customDialogueOptions;

        public bool HideScriptActions = true;
        
        public void Awake()
        {
            manager = this;
            currentnode = null;
            reloadPurpose = true;
            nodeLib.Clear();
            StoryButton.playerkeys.Clear();
            Application.targetFrameRate = 30;
            if (speakerPortrait != null) speakerPortrait.enabled = false;
            
            _actionButtons = FindObjectsOfType<DialogueButton>(true).ToList();
            _windowOverlays = FindObjectsOfType<MonoBehaviour>(true).OfType<IWindowOverlay>().ToList();
            _customDialogueOptions = FindObjectsOfType<MonoBehaviour>(true).OfType<ICustomDialogueAction>().ToList();
            
            if (clickToContinue != null) clickToContinue.SetActive(false);
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
            HideButtons();
            if (!loadingStory) StartCoroutine(GetAllTheNodes());
        }

        public static void ForceRefresh()
        {
            if (!loadingStory) manager.StartCoroutine(manager.GetAllTheNodes());
        }

        static void HideButtons()
        {
            foreach (var but in manager._actionButtons)
            {
                but.gameObject.SetActive(false);
            }
        }

        public static void BuildButtons()
        {
            var buttonIndex = 0;
            HideButtons();

            if (currentnode == null) return;

            if (speakQueue.Count > 0)
            {
                if (manager.clickToContinue != null) manager.clickToContinue.SetActive(!ZeldaTyper.currentlyWriting);
                return;
            }

            if (ZeldaTyper.currentlyWriting) return;

            if (manager.clickToContinue != null) manager.clickToContinue.SetActive(false);

            foreach (var block in currentnode.ActionBlocks())
            {
                if (StorySceneManager.SceneHasNodeButton(block.target_node)) continue;
                if (!StoryEventManager.ValidBlockCheck(block)) continue;
                if (block.clean_action.StartsWith("#") && manager.HideScriptActions) continue;

                var buttonLabel = block.dialogue.CapitalizeFirst();
                if (!string.IsNullOrEmpty(block.meta_scripts))
                {
                    if (block.meta_scripts.Contains("needword:")) buttonLabel = "<color=#00FFFF>" + buttonLabel + "</color>";
                }

                manager._actionButtons[buttonIndex].gameObject.SetActive(true);
                manager._actionButtons[buttonIndex].BroadcastMessage("SetLabel",buttonLabel);;
                if (manager._actionButtons[buttonIndex].gameObject.transform.Find("Icon") != null)
                {
                    var icon = StoryEventManager.BlockIcon(block);
                    manager._actionButtons[buttonIndex].gameObject.transform.Find("Icon").GetComponent<Image>().sprite =
                        (icon == null ? manager.defaultActionIcon : icon);
                }

                manager._actionButtons[buttonIndex].GetComponent<Button>().onClick.RemoveAllListeners();
                manager._actionButtons[buttonIndex].GetComponent<Button>().onClick.AddListener(
                    () =>
                    {
                        LoadNodeKey(block.target_node);
                    });
                buttonIndex++;
            }

            foreach (var dialogueOption in manager._customDialogueOptions)
            {
                dialogueOption.ProcessDialogueOptions(currentnode);
            }

            if (SuggestionManager.SuggestionMode) return;
        }

        private IEnumerator GetAllTheNodes()
        {
            loadingStory = true;
            var content = "";
            yield return new WaitForEndOfFrame();
#pragma warning disable 612, 618
            var nodefetcher = new WWW(BranchmakerPaths.StoryNodes(loadFromPublished) + bookkey);
            nodefetcher.threadPriority = ThreadPriority.High;
#pragma warning restore 612, 618
            
            yield return nodefetcher;

            var backupFileName = Application.persistentDataPath + "/" + bookkey + ".txt";

            if (!string.IsNullOrEmpty(nodefetcher.error))
            {
                Debug.LogError("Fetch error : (" + nodefetcher.url + ") " + nodefetcher.error);
                if (File.Exists(backupFileName))
                {
                    content = File.ReadAllText(backupFileName);
                }
                else
                {
                    ZeldaWriteDialogue("Could not reach BranchMaker server, please check your internet connection...");
                }
            }
            else
            {
                StreamWriter writer = new StreamWriter(backupFileName, false);
                writer.Write(nodefetcher.text);
                writer.Close();
                content = nodefetcher.text;
            }

            JSONNode allthenodes = JSONNode.Parse(content);
            foreach (JSONNode storynode in allthenodes["nodes"]) ProcessIncomingNode(BranchNode.createFromJson(storynode));

            loadingStory = false;

            if (forceLoad != null) {
                startingNode = forceLoad.currentNode;
                forceLoad.Resume();
                StorySceneManager.ShowPotentialScene(forceLoad.backgroundScene);
                forceLoad = null;
            }

            if (reloadPurpose)
            {
                LoadNodeKey(startingNode);
                reloadPurpose = false;
            }
        }


        private static void ZeldaWriteDialogue(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            actionCooldown = 0.5f;

            foreach (var obj in manager.dialogueHandlers)
            {
                obj.BroadcastMessage("DisplayDialogue", text, SendMessageOptions.DontRequireReceiver);
            }
        }
        
        private static bool Busy()
        {
            if (loadingStory) return true;
            if (gameover) return true;
            if (manager == null) return true;
            
            foreach (var overlay in manager._windowOverlays)
            {
                if (overlay.WindowOverlayOpen()) return true;
            }
            return false;
        }

        private void Update()
        {
            if (Busy()) return;

            if (ZeldaTyper.currentlyWriting)
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
            speakQueue.Clear();
            HideButtons();
            loadFromPublished = false;
            StartCoroutine(GetAllTheNodes());
        }

        private static void SpeakActiveNode()
        {
            if (speakQueue.Count <= 0) return;
            var activeBlock = speakQueue[0];
            RemoteVoicePlayer.StopSpeaking();
                
            if (activeBlock.meta_scripts.Contains("hide:dialogue"))
            {
                speakQueue.RemoveAt(0);
                BuildButtons();
                manager.dialogueWindow.SetActive(false);
                return;
            }
                
            manager.dialogueWindow.SetActive(true);
            var dialogue = activeBlock.dialogue;
            StoryEventManager.ParseBlockscript(activeBlock);
                
            if (!string.IsNullOrEmpty(activeBlock.character))
            {
                if (StoryActor.actorpool.ContainsKey(activeBlock.character))
                {
                    var actor = StoryActor.actorpool[activeBlock.character];
                    actor.SwitchEmotion(activeBlock.emotion);
                    StoryActor.NewSpeaker(activeBlock.character);
                    dialogue = "<color=#" + ColorUtility.ToHtmlStringRGB(actor.ActorObject.themeColor) + ">" + actor.ActorObject.displayName + "</color>\n" + dialogue;
                }
            }
            if (!string.IsNullOrEmpty(activeBlock.voice_file)) RemoteVoicePlayer.PlayRemoteOgg(activeBlock.voice_file);
            ZeldaWriteDialogue(dialogue);
            speakQueue.RemoveAt(0);
            BuildButtons();
        }

        public static void LoadNodeKey(string key)
        {
            if (clickCooldown > 0) return;
            if (!nodeLib.ContainsKey(key)) return;
            clickCooldown = 0.2f;
            HideButtons();
            if (key == "okay")
            {
                speakQueue.Clear();
                return;
            }
            LoadNode(nodeLib[key]);
        }

        private static void LoadNode(BranchNode node)
        {
            actionCooldown = .6f;
            currentnode = node;
            speakQueue.Clear();
            //Debug.Log("Load node "+node.id+ " blocks "+node.blocks.Count);
            foreach (var block in node.StoryBlocks())
            {
                if (!StoryEventManager.ValidBlockCheck(block)) continue;
                if (block.meta_scripts.Contains("dontrepeat"))
                {
                    if (manager._seenNodes.Contains(block.id)) continue;
                    manager._seenNodes.Add(block.id);
                }
                speakQueue.Add(block);
            }
            StorySceneManager.ShowPotentialScene(node.id);
            SpeakActiveNode();

            node.processed = true;
            CloudSaveManager.UpdateSaveFile();
        }


        private static void ProcessIncomingNode(BranchNode bNode)
        {
            if (!nodeLib.ContainsKey(bNode.id)) nodeLib.Add(bNode.id, bNode);

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

    }
}