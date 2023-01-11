using System.Text;

namespace BranchMaker
{

    public static class StringExtension
    {
        public static string CapitalizeFirst(this string s)
        {
            var IsNewSentense = true;
            var result = new StringBuilder(s.Length);
            foreach (var t in s)
            {
                if (IsNewSentense && char.IsLetter(t))
                {
                    result.Append(char.ToUpper(t));
                    IsNewSentense = false;
                }
                else
                    result.Append(t);

                if (t == '!' || t == '?' || t == '.')
                {
                    IsNewSentense = true;
                }
            }

            return result.ToString();
        }
        public static string UCFirst(this string s)
        {
            var output = new StringBuilder();
            var pieces = s.Split(' ');
            foreach (var piece in pieces)
            {
                var theChars = piece.ToCharArray();
                theChars[0] = char.ToUpper(theChars[0]);
                output.Append(' ');
                output.Append(new string(theChars));
            }

            return output.ToString();
        }


    }
}
