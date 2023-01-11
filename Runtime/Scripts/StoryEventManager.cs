using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;

namespace BranchMaker.Story
{
    public class StoryEventManager
    {
        static List<StoryEventTrigger> _triggerPool = new List<StoryEventTrigger>();

        public static void Initialize()
        {
            if (_triggerPool.Count == 0) PreloadEvents();
        }

        public static void RegisterEventTrigger(Type eventClass)
        {
            if (_triggerPool.Count == 0) PreloadEvents();
            var trigger = Activator.CreateInstance(eventClass) as StoryEventTrigger;
            _triggerPool.Add(trigger);
        }

        static void PreloadEvents()
        {
            _triggerPool.Clear();
            var allAbilities = Assembly.GetAssembly(typeof(StoryEventTrigger)).GetTypes().Where(t => !t.IsAbstract && typeof(StoryEventTrigger).IsAssignableFrom(t));

            foreach (var triggerEvent in allAbilities)
            {
                var trigger = Activator.CreateInstance(triggerEvent) as StoryEventTrigger;
                _triggerPool.Add(trigger);
            }
        }


        static public Sprite BlockIcon(string blockscript)
        {
            if (blockscript == null) return null;
            if (blockscript == string.Empty) return null;
            if (blockscript == "null") return null;
            string[] codelines = blockscript.Split("\n"[0]);

            /*
             INSERT YOUR CUSTOM SCRIPTING TRIGGERS HERE
             */

            foreach (string line in codelines)
            {
                string cline = line.Trim();

                if (cline.Contains("icon:"))
                {
                    string newclue = cline.Replace("icon:", "").ToLower().Trim();
                    foreach (Sprite spr in StoryManager.manager.faces)
                    {
                        if (spr.name.ToLower() == newclue)
                        {
                            return spr;
                        }
                    }
                }
            }
            return null;
        }

        internal static bool ValidBlockCheck(string blockscript, BranchNodeBlock nodeBlock)
        {
            switch (blockscript)
            {
                case null:
                case "":
                case "null":
                    return true;
            }

            var codelines = blockscript.Split("\n"[0]);

            /*
             INSERT YOUR CUSTOM SCRIPTING TRIGGERS HERE
             */

            foreach (string line in codelines)
            {
                string cline = line.Trim();

                // Dynamic trigger handling
                foreach (StoryEventTrigger trigger in _triggerPool)
                {
                    if (trigger.Method == StoryEventTrigger.TriggerMethod.Validator && cline.StartsWith(trigger.TriggerKey))
                    {
                        if (trigger.PassValidation(cline, nodeBlock) == false) return false;
                    }
                }
            }
            return true;
        }


        static public void ParseBlockscript(string blockscript, BranchNodeBlock nodeBlock)
        {
            switch (blockscript)
            {
                case null:
                case "":
                case "null":
                    return;
            }

            string[] codelines = blockscript.Split("\n"[0]);

            /*
             INSERT YOUR CUSTOM SCRIPTING TRIGGERS HERE
             */

            foreach (string line in codelines)
            {
                string cline = line.Trim();

                string[] bits = cline.Split(':');


                // Dynamic trigger handling
                foreach (StoryEventTrigger trigger in _triggerPool)
                {
                    if (trigger.Method == StoryEventTrigger.TriggerMethod.OnBlockRun && bits[0] == trigger.TriggerKey)
                    {
                        trigger.Run(cline, nodeBlock, bits);
                    }
                }

                if (cline.StartsWith("speaks:"))
                {
                    string whospeaks = cline.Replace("speaks:", "").Trim();
                    StoryManager.ParseSpeaker(whospeaks);
                }

                if (cline.StartsWith("bg:"))
                {
                    string bgwish = cline.Replace("bg:", "").Trim();
                    StoryManager.manager.TryBackdrop(bgwish);
                }
                if (cline.StartsWith("evidence:"))
                {
                    string bgwish = cline.Replace("evidence:", "").Trim();
                    StoryManager.ParseSpeaker(bgwish);
                }

                if (cline.StartsWith("sound:"))
                {
                    string clipname = cline.Replace("sound:", "").ToLower().Trim();
                    SoundeffectsManager.PlayEffect(clipname, false, false);
                }
                if (cline.StartsWith("speak:"))
                {
                    SoundeffectsManager.stopSpeech();
                    string clipname = cline.Replace("speak:", "").ToLower().Trim();
                    SoundeffectsManager.PlayEffect(clipname, false, false);
                }
                if (cline.StartsWith("showcharacter:"))
                {
                    string charname = cline.Replace("showcharacter:", "").ToLower().Trim();
                    StoryActor.NewSpeaker(charname);
                }
            }
        }

        public void ReplayLevel()
        {
            BranchNode.nodecollection.Clear();
        }

    }
}