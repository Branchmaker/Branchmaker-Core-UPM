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

public static class StringExtension
{
    public static string CapitalizeFirst(this string s)
    {
        bool IsNewSentense = true;
        var result = new StringBuilder(s.Length);
        for (int i = 0; i < s.Length; i++)
        {
            if (IsNewSentense && char.IsLetter(s[i]))
            {
                result.Append(char.ToUpper(s[i]));
                IsNewSentense = false;
            }
            else
                result.Append(s[i]);

            if (s[i] == '!' || s[i] == '?' || s[i] == '.')
            {
                IsNewSentense = true;
            }
        }

        return result.ToString();
    }
    public static string UCFirst(this string s)
    {
        StringBuilder output = new StringBuilder();
        string[] pieces = s.Split(' ');
        foreach (string piece in pieces)
        {
            char[] theChars = piece.ToCharArray();
            theChars[0] = char.ToUpper(theChars[0]);
            output.Append(' ');
            output.Append(new string(theChars));
        }

        return output.ToString();
    }


}