using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;

namespace BranchMaker.Story
{
    public static class StoryEventManager
    {
        static List<StoryEventTrigger> _triggerPool = new();
        static List<string> _seenNodes = new();
        public static bool PassActionValidation;
        public static BranchNodeBlock StoredAction;

        static StoryEventManager()
        {
            _triggerPool.Clear();
            PreloadEvents();
        }

        public static void RegisterEventTrigger(Type eventClass)
        {
            if (_triggerPool.Count == 0) PreloadEvents();
            if (_triggerPool.Any(a => a.GetType() == eventClass)) return;
            var trigger = Activator.CreateInstance(eventClass) as StoryEventTrigger;
            _triggerPool.Add(trigger);
        }

        private static void PreloadEvents()
        {
            var allAbilities = Assembly.GetAssembly(typeof(StoryEventTrigger)).GetTypes().Where(t => !t.IsAbstract && typeof(StoryEventTrigger).IsAssignableFrom(t));

            foreach (var triggerEvent in allAbilities)
            {
                var trigger = Activator.CreateInstance(triggerEvent) as StoryEventTrigger;
                if (_triggerPool.Any(a => a.GetType() == triggerEvent)) continue;
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
        
        public static bool ValidateActionBlock(BranchNodeBlock nodeBlock)
        {
            if (!nodeBlock.HasMetaScript()) return true;
            PassActionValidation = true;
            StoredAction = nodeBlock;
            foreach (var line in nodeBlock.MetaScriptLines())
            {
                var cline = line.Trim();
                foreach (var trigger in _triggerPool.Where(a => a.Method == StoryEventTrigger.TriggerMethod.ActionCheck))
                {
                    if (cline.StartsWith(trigger.TriggerKey))
                    {
                        var bits = cline.Split(':');
                        trigger.Run(cline, nodeBlock, bits);
                    }
                }
            }

            return PassActionValidation;
        }


        public static Sprite BlockIcon(BranchNodeBlock block)
        {
            if (block.forcedIcon != null) return block.forcedIcon;
            if (!block.HasMetaScript()) return null;
            foreach (var line in block.MetaScriptLines())
            {
                var cline = line.Trim();

                if (cline.StartsWith("icon:"))
                {
                    string newclue = cline.Replace("icon:", "").ToLower().Trim();
                    foreach (var spr in StoryManager.manager.IconSprites)
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

        public static bool ValidBlockCheck(BranchNodeBlock nodeBlock)
        {
            if (!nodeBlock.HasMetaScript()) return true;
            foreach (var line in nodeBlock.MetaScriptLines())
            {
                var cline = line.Trim();

                foreach (var trigger in _triggerPool.FindAll(a => a.Method == StoryEventTrigger.TriggerMethod.Validator))
                {
                    if (cline.StartsWith(trigger.TriggerKey) || nodeBlock.clean_action == trigger.TriggerKey)
                    {
                        if (trigger.PassValidation(cline, nodeBlock) == false) return false;
                    }   
                }
            }
            
            if (nodeBlock.meta_scripts.Contains("dontrepeat"))
            {
                if (_seenNodes.Contains(nodeBlock.id)) return false;
                _seenNodes.Add(nodeBlock.id);
            }
            return true;
        }


        public static void ParseBlockscript(BranchNodeBlock nodeBlock)
        {
            if (!nodeBlock.HasMetaScript()) return;
            
            foreach (var line in nodeBlock.MetaScriptLines())
            {
                var cline = line.Trim();
                
                // Dynamic trigger handling
                foreach (var trigger in _triggerPool)
                {
                    if (trigger.Method == StoryEventTrigger.TriggerMethod.OnBlockRun && cline.StartsWith(trigger.TriggerKey))
                    {
                        var bits = cline.Split(':');
                        trigger.Run(cline, nodeBlock, bits);
                    }
                }
                
                if (cline.StartsWith("sound:"))
                {
                    var clipname = cline.Replace("sound:", "").ToLower().Trim();
                    SoundeffectsManager.PlayEffect(clipname, false, false);
                }
                if (cline.StartsWith("speak:"))
                {
                    SoundeffectsManager.stopSpeech();
                    var clipname = cline.Replace("speak:", "").ToLower().Trim();
                    SoundeffectsManager.PlayEffect(clipname, false, false);
                }
                if (cline.StartsWith("showcharacter:"))
                {
                    var charname = cline.Replace("showcharacter:", "").ToLower().Trim();
                    StoryActor.NewSpeaker(charname);
                }
            }
        }

    }
}