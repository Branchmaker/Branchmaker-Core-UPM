using BranchMaker;
using BranchMaker.Story;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeedWord : StoryEventTrigger
{
    public override TriggerMethod Method => StoryEventTrigger.TriggerMethod.Validator;
    public override string TriggerKey => "needword";

    public override bool PassValidation(string trigger, BranchNodeBlock block)
    {
        string newclue = trigger.Replace("needword:", "").ToLower().Trim();
        return StoryButton.playerkeys.Contains(newclue);
    }

    public override void Run(string trigger, BranchNodeBlock block, string[] bits)
    {
        throw new System.NotImplementedException();
    }
}
