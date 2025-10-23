namespace Data.Tools
{
    /// <summary>
    /// Utility class for various validations.
    /// </summary>
    public class Validations 
    {
        /// <summary>
        /// Validates if the provided string is a valid hexadecimal color code.
        /// </summary>
        /// <param name="color">represents Hex color code</param>
        /// <returns>Boolean representing if value is valid colour</returns>
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
