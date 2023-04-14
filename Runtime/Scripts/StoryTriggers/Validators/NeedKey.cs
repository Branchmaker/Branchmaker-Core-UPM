using BranchMaker;
using BranchMaker.Story;

public class NeedKey : StoryEventTrigger
{
    public override TriggerMethod Method => StoryEventTrigger.TriggerMethod.Validator;
    public override string TriggerKey => "needkey";

    public override bool PassValidation(string trigger, BranchNodeBlock block)
    {
        string newclue = trigger.Replace("needkey:", "").ToLower().Trim();
        return StoryButton.playerkeys.Contains(newclue);
    }

    public override void Run(string trigger, BranchNodeBlock block, string[] bits)
    {
        throw new System.NotImplementedException();
    }
}
