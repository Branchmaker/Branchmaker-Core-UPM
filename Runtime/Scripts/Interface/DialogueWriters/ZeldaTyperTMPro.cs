using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BranchMaker.Runtime;
using TMPro;
using UnityEngine;

namespace BranchMaker.Interface.DialogueWriters
{
    public class ZeldaTyperTMPro : DialogueTyper
    {
        static string displayDialogueString;
        private static List<string> _displayDialogueBits = new();

        private float _lettercooldown = 0f;
        private float _speedupCooldown = 0f;

        private bool _zeldaInsideColor;
        private bool _zeldaInsideBracket;
        private bool _zeldaInsideWord;
        private string _zeldaGeneratedText;

        private bool _rushSpeaker;
        private TMP_Text _dialogueLabel;

        private void Awake()
        {
            _dialogueLabel = GetComponent<TMP_Text>();
        }

        protected override void WriteDialogue(BranchNodeBlock currentBlock, string dialogue)
        {
            CurrentlyWriting = true;
            _rushSpeaker = false;

            _speedupCooldown = 0.5f;

            displayDialogueString = dialogue.Replace("\r", "").Replace("  ", " ").Replace(" ?", "?");

            var characters = new string[displayDialogueString.Length];
            for (var i = 0; i < displayDialogueString.Length; i++)
            {
                characters[i] = Convert.ToString(displayDialogueString[i]);
            }

            _displayDialogueBits = new List<string>(characters);
            _dialogueLabel.text = "";
            _zeldaGeneratedText = "";
            _zeldaInsideBracket = false;
            _zeldaInsideColor = false;
        }


        void ZeldaType(int strength)
        {
            for (var i = 1; i <= strength; i++)
            {
                _zeldaInsideWord = false;
                if (_displayDialogueBits.Count > 0)
                {
                    CurrentlyWriting = true;

                    if (_displayDialogueBits[0] == "<") _zeldaInsideBracket = true;
                    if (_displayDialogueBits[0] == ">") _zeldaInsideBracket = false;
                    if (_displayDialogueBits.Count > 3)
                    {
                        if (_displayDialogueBits[0] + _displayDialogueBits[1] == "<c") _zeldaInsideColor = true;
                        if (_displayDialogueBits[0] + _displayDialogueBits[1] + _displayDialogueBits[2] == "</c")
                            _zeldaInsideColor = false;
                    }

                    if (_displayDialogueBits[0] != " ") _zeldaInsideWord = true;
                    _zeldaGeneratedText += _displayDialogueBits[0];
                    _dialogueLabel.text = _zeldaGeneratedText + (_zeldaInsideColor ? "</color>" : "") + "<color=#00ffff00>" +
                                         Regex.Replace(displayDialogueString.Replace(_zeldaGeneratedText, ""), @"<[^>]*>",
                                             string.Empty) + "</color>";
                    _displayDialogueBits.RemoveAt(0);
                }

                if (_displayDialogueBits.Count <= 0 && CurrentlyWriting)
                {
                    CurrentlyWriting = false;
                    displayDialogueString = null;
                    StoryManager.Instance.DoneRenderingBlock();
                }

                if (_zeldaInsideBracket) ZeldaType(1);
                if (_zeldaInsideWord) ZeldaType(1);
            }
        }

        private void Update()
        {
            if (displayDialogueString != null && _lettercooldown <= 0)
            {
                _lettercooldown = 0.034f;
                ZeldaType(1);

                if ((Input.anyKey && _speedupCooldown <= 0) || _rushSpeaker)
                {
                    ZeldaType(18);
                    _rushSpeaker = true;
                }
            }

            _speedupCooldown -= Time.deltaTime;
            _lettercooldown -= Time.deltaTime;
        }
    }
}