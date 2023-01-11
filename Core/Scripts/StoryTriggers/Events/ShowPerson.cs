using BranchMaker;
using BranchMaker.Story;

public class ShowPerson : StoryEventTrigger
{
    public override TriggerMethod Method => TriggerMethod.OnBlockRun;

    public override string TriggerKey => "showperson";
    public override void Run(string trigger, BranchNodeBlock block, string[] bits)
    {
        string whospeaks = bits[1];
        StoryManager.ParseSpeaker(whospeaks);
    }

    public override bool PassValidation(string trigger, BranchNodeBlock block)
    {
        throw new System.NotImplementedException();
    }

}
