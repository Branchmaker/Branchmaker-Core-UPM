using System.Linq;
using BranchMaker;
using BranchMaker.Story;

public class AnyKey : StoryEventTrigger
{
    public override TriggerMethod Method => TriggerMethod.Validator;
    public override string TriggerKey => "anykey";

    public override bool PassValidation(string trigger, BranchNodeBlock block)
    {
        var keystring = trigger.Replace("anykey:", "").ToLower().Trim();
        var keys = keystring.Split(",").ToList();
        return keys.Any(key => StoryButton.playerkeys.Contains(key));
    }

    public override void Run(string trigger, BranchNodeBlock block, string[] bits)
    {
        throw new System.NotImplementedException();
    }
}