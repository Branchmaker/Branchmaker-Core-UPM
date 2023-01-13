using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;

namespace BranchMaker.Story
{
    public static class StoryEventManager
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

        private static void PreloadEvents()
        {
            var allAbilities = Assembly.GetAssembly(typeof(StoryEventTrigger)).GetTypes().Where(t => !t.IsAbstract && typeof(StoryEventTrigger).IsAssignableFrom(t));

            foreach (var triggerEvent in allAbilities)
            {
                var trigger = Activator.CreateInstance(triggerEvent) as StoryEventTrigger;
                _triggerPool.Add(trigger);
            }
        }
        
        public static void PreloadScriptCheck(BranchNodeBlock nodeBlock)
        {
            if (!nodeBlock.HasMetaScript()) return;
            foreach (var line in nodeBlock.MetaScriptLines())
            {
                var cline = line.Trim();
                foreach (var trigger in _triggerPool.Where(a => a.Method == StoryEventTrigger.TriggerMethod.PreloadStory))
                {
                    if (cline.StartsWith(trigger.TriggerKey))
                    {
                        var bits = cline.Split(':');
                        trigger.Run(cline, nodeBlock, bits);
                    }
                }
            }
        }


        public static Sprite BlockIcon(BranchNodeBlock block)
        {
            if (!block.HasMetaScript()) return null;
            foreach (var line in block.MetaScriptLines())
            {
                var cline = line.Trim();

                if (cline.StartsWith("icon:"))
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

        internal static bool ValidBlockCheck(BranchNodeBlock nodeBlock)
        {
            if (!nodeBlock.HasMetaScript()) return true;

            foreach (string line in nodeBlock.MetaScriptLines())
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


        static public void ParseBlockscript(BranchNodeBlock nodeBlock)
        {
            if (!nodeBlock.HasMetaScript()) return;
            
            foreach (string line in nodeBlock.MetaScriptLines())
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

    }
}