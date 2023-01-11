using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BranchMaker.Story
{
    public abstract class StoryEventTrigger
    {
        public StoryEventTrigger() { }
        public enum TriggerMethod { Inactive, OnBlockRun, PreloadStory, Validator };
        public abstract TriggerMethod Method { get; }
        public abstract string TriggerKey { get; }

        public abstract void Run(string trigger, BranchNodeBlock block, string[] bits);

        public abstract bool PassValidation(string trigger, BranchNodeBlock block);
    }
}