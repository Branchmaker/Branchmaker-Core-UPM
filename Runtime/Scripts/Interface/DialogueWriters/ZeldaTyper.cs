using BranchMaker.Story;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BranchMaker.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace BranchMaker.UI
{
    public class ZeldaTyper : MonoBehaviour
    {
        public static ZeldaTyper manager;
        static string displayDialogueString;
        static List<string> displayDialogueBits = new List<string>();

        float lettercooldown = 0f;
        float speedupCooldown = 0f;

        static bool zeldaInsideColor;
        static bool zeldaInsideBracket;
        static bool zeldaInsideWord;
        static string zeldaGeneratedText;
        static public bool currentlyWriting;
        static string purewrite;

        public Color[] fadecolors;

        List<GameObject> queue = new List<GameObject>();

        bool rushSpeaker;
        public Text dialogue;

        private void Awake()
        {
            manager = this;
        }


        public void DisplayDialogue(string text)
        {
            rushSpeaker = false;

            speedupCooldown = 0.5f;

            displayDialogueString = text.Replace("\r", "").Replace("  ", " ").Replace(" ?", "?");

            purewrite = Regex.Replace(text, @"<[^>]*>", String.Empty);

            string[] bits = displayDialogueString.Split(' ');

            string[] characters = new string[displayDialogueString.Length];
            for (int i = 0; i < displayDialogueString.Length; i++)
            {
                characters[i] = Convert.ToString(displayDialogueString[i]);
            }

            displayDialogueBits = new List<string>(characters);
            dialogue.text = "";
            zeldaGeneratedText = "";
            zeldaInsideBracket = false;
            zeldaInsideColor = false;

            Phaseout();
        }

        public void Clearout() {
            queue.Clear();
        }

        public static string StripHTML(string input)
        {
            return Regex.Replace(input, "<.*?>", String.Empty);
        }

        void Phaseout() {
            /*
            while (queue.Count > 4) {
                GameObject.Destroy(queue[0]);
                queue.RemoveAt(0);
            }
            */
            if (queue.Count > 1)
            {
                queue[0].GetComponent<Text>().text = StripHTML(queue[0].GetComponent<Text>().text);
            }

            /*

            if (queue.Count > 3)
            {
                queue[0].GetComponent<Text>().color = fadecolors[0];
                queue[1].GetComponent<Text>().color = fadecolors[1];
                queue[2].GetComponent<Text>().color = fadecolors[2];
                queue[1].GetComponent<Text>().text = StripHTML(queue[1].GetComponent<Text>().text);
                queue[2].GetComponent<Text>().text = StripHTML(queue[2].GetComponent<Text>().text);
                queue[3].GetComponent<Text>().text = StripHTML(queue[3].GetComponent<Text>().text);
            }
            if (queue.Count == 3)
            {
                queue[0].GetComponent<Text>().color = fadecolors[1];
                queue[1].GetComponent<Text>().color = fadecolors[2];
            }
            if (queue.Count == 2)
            {
                queue[0].GetComponent<Text>().color = fadecolors[2];
            }
            */
        }

        void ZeldaType()
        {
            zeldaInsideWord = false;
            if (displayDialogueBits.Count > 0)
            {
                currentlyWriting = true;

                if (displayDialogueBits[0] == "<") zeldaInsideBracket = true;
                if (displayDialogueBits[0] == ">") zeldaInsideBracket = false;
                if (displayDialogueBits.Count > 3)
                {
                    if (displayDialogueBits[0] + displayDialogueBits[1] == "<c") zeldaInsideColor = true;
                    if (displayDialogueBits[0] + displayDialogueBits[1] + displayDialogueBits[2] == "</c") zeldaInsideColor = false;
                }
                if (displayDialogueBits[0] != " ") zeldaInsideWord = true;
                zeldaGeneratedText = zeldaGeneratedText + displayDialogueBits[0];
                dialogue.text = zeldaGeneratedText + (zeldaInsideColor ? "</color>" : "") + "<color=#00ffff00>" + Regex.Replace(displayDialogueString.Replace(zeldaGeneratedText, ""), @"<[^>]*>", String.Empty) + "</color>";
                displayDialogueBits.RemoveAt(0);

                if (displayDialogueBits.Count == 0)
                {
                    currentlyWriting = false;
                    //StoryManager.BuildButtons();
                }
            }
            else
            {
                displayDialogueString = null;
            }
            if (zeldaInsideBracket) ZeldaType();
            if (zeldaInsideWord) ZeldaType();
        }

        private void Update()
        {

            if (displayDialogueString != null && lettercooldown <= 0)
            {
                lettercooldown = 0.034f;
                ZeldaType();
                /*
                if (MainMenuManager.textspeed >= 0.25f) ZeldaType();
                if (MainMenuManager.textspeed >= 0.5f) ZeldaType();
                if (MainMenuManager.textspeed >= 0.75f) ZeldaType();
                */
                if ((Input.anyKey && speedupCooldown <= 0) || rushSpeaker)
                {
                    ZeldaType();
                    ZeldaType();
                    ZeldaType();
                    ZeldaType();
                    ZeldaType();
                    ZeldaType();
                    ZeldaType();
                    ZeldaType();
                    ZeldaType();
                    ZeldaType();
                    ZeldaType();
                    ZeldaType();
                    ZeldaType();
                    ZeldaType();
                    ZeldaType();
                    ZeldaType();
                    ZeldaType();
                    ZeldaType();
                    rushSpeaker = true;

                }
            }
            speedupCooldown -= Time.deltaTime;
            lettercooldown -= Time.deltaTime;
        }
    }

}