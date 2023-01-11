using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SimpleJSON;
using System;
using System.Text.RegularExpressions;
using UnityEngine.Networking;
//using Steamworks;

public class SuggestionManager : MonoBehaviour
{
    static public bool SuggestionMode;
    static public SuggestionManager manager;

    /*
        bool approvedEditor;

        string systemmessage = "";

        string editorKey = "";

        static string newopt_action = "";

        public GUISkin skin;
        public Texture2D logo;

        Rect windowRect = new Rect(20, 20, 600, 150);

        void Start()
        {
            manager = this;
        }

        void OnGUI()
        {
            if (!SteamManager.Initialized) return;

            GUI.skin = skin;
            if (!SuggestionMode) return;

            windowRect = GUI.Window(0, windowRect, DoMyWindow, "Suggestion tool ("+ SteamFriends.GetPersonaName() + ")");
        }

        public void postWrittenAsSuggestion() {

            if (!SteamManager.Initialized) return;
            StartCoroutine(createWritingTask());
        }

        void Update() {
            if (Input.GetKeyDown(KeyCode.F2)) {
                SuggestionMode = !SuggestionMode;
                StoryManager.jsonurl = StoryManager.jsonurl.Replace("/static/", "/");
            }
        }

        void DoMyWindow(int windowID)
        {

            if (StoryManager.currentnode == null)
            {
                GUILayout.Label("This area of the game is currently not open to suggestions.");
                return;
            }

            GUILayout.Label("Suggest dialogue option/question:");
            newopt_action = GUILayout.TextArea(newopt_action);
            GUILayout.Label(systemmessage);

            if (GUILayout.Button("Send in suggestion"))
            {
                StartCoroutine(createWritingTask());
            }

            if (GUILayout.Button("Close"))
            {
                SuggestionMode = false;
            }
            GUI.DragWindow(new Rect(0, 0, Screen.width, Screen.height));
        }

        IEnumerator createWritingTask()
        {
            // StoryManager.zeldaWriteDialogue("Uploading new data set...");

            systemmessage = "Thank you for your suggestion: "+ newopt_action;

            //suggestionButtonText.text = "Sending...";
            //suggestionButton.interactable = false;

            WriterNotificationManager.pendingNotification = true;

            WWWForm formData = new WWWForm();
            formData.AddField("steam_user", SteamUser.GetSteamID().ToString());
            formData.AddField("deep_node", StoryManager.currentnode.id);
            formData.AddField("root_node", StoryManager.currentnode.id);
            formData.AddField("suggestion", newopt_action);
            formData.AddField("identity", SteamFriends.GetPersonaName());
            formData.AddField("storybook", StoryManager.manager.bookkey);
            newopt_action = "";

            UnityWebRequest webcall = UnityWebRequest.Post("https://branchmaker.com/api/steam/suggestion", formData);
            webcall.SetRequestHeader("Cache-Control", "max-age=0, no-cache, no-store");
            webcall.SetRequestHeader("Pragma", "no-cache");

            yield return webcall.SendWebRequest();

            //suggestionButtonText.text = "Thank you";

            //OptionGridGenerator.waitForResponse();
            //StoryManager.forceRefresh();
        }

    */

}

