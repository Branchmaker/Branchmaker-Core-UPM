using BranchMaker;
using BranchMaker.Story;

public class IgnoreIfKey : StoryEventTrigger
{
    public override TriggerMethod Method => StoryEventTrigger.TriggerMethod.Validator;
    public override string TriggerKey => "ignoreifkey";

    public override bool PassValidation(string trigger, BranchNodeBlock block)
    {
        var newclue = trigger.Replace("ignoreifkey:", "").ToLower().Trim();
        return !StoryButton.playerkeys.Contains(newclue);
    }

    public override void Run(string trigger, BranchNodeBlock block, string[] bits)
    {
        throw new System.NotImplementedException();
    }
}
