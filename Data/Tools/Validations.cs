namespace Data.Tools
{
    public class Validations 
    {

        public static bool IsValidHexColor(string color)
        {
            if (string.IsNullOrWhiteSpace(color))
                return false;

            // Pattern: #RGB, #RRGGBB, #RRGGBBAA
            var hexPattern = @"^#([A-Fa-f0-9]{3}|[A-Fa-f0-9]{6}|[A-Fa-f0-9]{8})$";
            return System.Text.RegularExpressions.Regex.IsMatch(color, hexPattern);
        }
    }
}
