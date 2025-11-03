namespace Application.Utils
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

        /// <summary>
        /// Returns white as default if empty or not valid hex code
        /// </summary>
        /// <param name="color"> hex color code to validate</param>
        /// <returns>white hex code if not valid color</returns>
        public static string ValidateAndFormatColor(string? color)
        {

            if (string.IsNullOrWhiteSpace(color))return "#FFFFFF";

            if (!IsValidHexColor(color)) return "#FFFFFF";

            return color.ToUpper();
        }
    }
}
