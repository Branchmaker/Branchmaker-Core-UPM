using BranchMaker;
using BranchMaker.Story;
using System;
using System.Linq;
using System.Text;

public class ReverseSpeak : StoryEventTrigger
{
    public override TriggerMethod Method => TriggerMethod.PreloadStory;

    public override string TriggerKey => "reversespeak";

    public override bool PassValidation(string trigger, BranchNodeBlock block)
    {
        throw new System.NotImplementedException();
    }

    public override void Run(string trigger, BranchNodeBlock block, string[] bits)
    {
        block.dialogue = Reverse(block.dialogue);
    }
    public static string Reverse(string s)
    {
        char[] charArray = s.ToCharArray();
        Array.Reverse(charArray);
        string reversed = new string(charArray);
        reversed = reversed.ToLower();

        reversed = reversed.Replace("'", "").Replace(" ,", ", ").Replace(" .", ". ").Replace(" ?", "? ").Replace(" !", "! ").CapitalizeFirst().TrimStart('?','.','!')+".";

        return reversed;
    }
}