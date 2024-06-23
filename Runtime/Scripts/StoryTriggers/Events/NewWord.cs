using BranchMaker;
using BranchMaker.GameScripts.Audio;
using BranchMaker.Story;

public class NewWord : StoryEventTrigger
{
    public override TriggerMethod Method => TriggerMethod.OnBlockRun;
    public override string TriggerKey => "newword";
    public override void Run(string trigger, BranchNodeBlock block, string[] bits)
    {
        string newclue = bits[1];
        if (!StoryButton.playerkeys.Contains(newclue))
        {

            SoundeffectsManager.PlayEffect("ClueChime", true, false);
            StoryButton.playerkeys.Add(newclue);
        }
    }

    public override bool PassValidation(string trigger, BranchNodeBlock block)
    {
        throw new System.NotImplementedException();
    }
}
