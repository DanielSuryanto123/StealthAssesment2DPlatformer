using System.Linq;

namespace CoLab.UI
{
    public static class StringValidator
    {
        public static string ValidateID(string id)
        {
            if (!id.All(char.IsLetterOrDigit))
            {
                return "Must be alphanumeric";
            }

            return "";
        }
    }
}