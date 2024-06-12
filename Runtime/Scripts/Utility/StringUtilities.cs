using System.Text;
using System.Text.RegularExpressions;

namespace BranchMaker.Runtime.Utility
{

    public static class StringExtension
    {
        public static string CapitalizeFirst(this string s)
        {
            var isNewSentense = true;
            var result = new StringBuilder(s.Length);
            foreach (var t in s)
            {
                if (isNewSentense && char.IsLetter(t))
                {
                    result.Append(char.ToUpper(t));
                    isNewSentense = false;
                }
                else
                    result.Append(t);

                if (t == '!' || t == '?' || t == '.')
                {
                    isNewSentense = true;
                }
            }

            return result.ToString();
        }
        

        public static string StripHTML(this string s)
        {
            return Regex.Replace(s, "<.*?>", string.Empty);
        }

        public static bool IsValidUUID(this string s)
        {
            return (s.Length == 36);
        }
        
        public static string UcFirst(this string s)
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
