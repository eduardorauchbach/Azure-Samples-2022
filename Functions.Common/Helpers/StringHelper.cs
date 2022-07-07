namespace Functions.Common.Helpers
{
    static public class StringHelper
    {
        public static string ToCleanString(this object value)
        {
            return value.ToString().Replace("_", " ");
        }
    }
}
