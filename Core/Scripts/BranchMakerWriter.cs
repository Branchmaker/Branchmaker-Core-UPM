using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BranchMakerWriter : MonoBehaviour {
    /*
    bool showModal;
    bool approvedEditor;

    string editorKey = "";

    static string entrymode = "";
    static string newopt_action = "";

    public GUISkin skin;
    public Texture2D logo;

    Rect windowRect = new Rect(20, 20, 400, 700);

#if UNITY_EDITOR

    void OnGUI() {
        GUI.skin = skin;
        if (!showModal) return;

        if (!isValidEditor())
        {
            editorKey = GUILayout.TextField(editorKey);
            return;
        }

        windowRect = GUI.Window(0, windowRect, DoMyWindow, "BranchMaker");
    }

    void DoMyWindow(int windowID)
    {
        GUILayout.Label(logo);
        if (GUILayout.Button("Add choise option"))
        {
            entrymode = "new_option";
            newopt_action = "";
        }
        /*
        if (RecipientManager.destination != null)
        {
            if (GUILayout.Button("Edit current node"))
            {
                entrymode = "node_current";
                newopt_action = RecipientManager.destination.currentnode.blocks[0].dialogue;
            }
        }
        
        if (entrymode == "new_option")
        {
            GUILayout.Label("ADD NEW OPTION:");
            newopt_action = GUILayout.TextField(newopt_action);

            if (GUILayout.Button("Create new option"))
            {
                if (newopt_action == "") return;
                StartCoroutine(createWritingTask(newopt_action));
                newopt_action = "";
            }
        }


        if (entrymode == "node_current")
        {
            GUILayout.Label("EDIT CURRENT NODE:");
            newopt_action = GUILayout.TextArea(newopt_action);

            if (GUILayout.Button("Update current node"))
            {
                if (newopt_action == "") return;
                StartCoroutine(updateCurrentNode(newopt_action));
            }
        }
        if (GUILayout.Button("Close"))
        {
            editorKey = "";
        }

        GUI.DragWindow(new Rect(0, 0, Screen.width, Screen.height));
    }


    IEnumerator createWritingTask(string key)
    {
        StoryManager.zeldaWriteDialogue("Uploading new data set...");
        WWWForm orderform = new WWWForm();
        orderform.AddField("key", key);
        orderform.AddField("node", RecipientManager.destination.currentnode.id);

        Dictionary<string, string> headers = orderform.headers;
        byte[] rawData = orderform.data;

        WWW webcall = new WWW(StoryManager.jsonorderurl + StoryManager.currentRound, rawData, headers);
        yield return webcall;

        StoryManager.forceRefresh();
    }

    IEnumerator updateCurrentNode(string key)
    {
        StoryManager.zeldaWriteDialogue("Uploading new data set...");
        WWWForm orderform = new WWWForm();
        orderform.AddField("dialogue", key);
        orderform.AddField("block", RecipientManager.destination.currentnode.blocks[0].id);

        Dictionary<string, string> headers = orderform.headers;
        byte[] rawData = orderform.data;

        WWW webcall = new WWW(StoryManager.jsonupdateurl + StoryManager.currentRound, rawData, headers);
        yield return webcall;

        print(webcall.text);

        StoryManager.forceRefresh();
    }

    bool isValidEditor() {
        return (editorKey == "geckosan");
    }

    void Update() {
        if (Input.GetKeyUp(KeyCode.F9)) {
            showModal = !showModal;
        }
    }

#endif
*/
}
