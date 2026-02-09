using System.Text;
using System.Text.Json;

namespace Valhalla.Routing;

/// <summary>
/// JSON naming policy that converts property names to snake_case.
/// </summary>
internal sealed class SnakeCaseNamingPolicy : JsonNamingPolicy
{
    /// <summary>
    /// Gets the singleton instance of the snake_case naming policy.
    /// </summary>
    public static SnakeCaseNamingPolicy Instance { get; } = new SnakeCaseNamingPolicy();

    /// <inheritdoc/>
    public override string ConvertName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return name;
        }

        var builder = new StringBuilder();
        var previousUpper = false;

        for (var i = 0; i < name.Length; i++)
        {
            var currentChar = name[i];

            if (char.IsUpper(currentChar))
            {
                // Add underscore before uppercase letter if:
                // 1. It's not the first character
                // 2. The previous character was not uppercase
                // 3. The next character exists and is lowercase (handles acronyms like "HTTP")
                if (i > 0 && !previousUpper)
                {
                    builder.Append('_');
                }
                else if (i > 0 && previousUpper && i < name.Length - 1 && char.IsLower(name[i + 1]))
                {
                    builder.Append('_');
                }

                builder.Append(char.ToLowerInvariant(currentChar));
                previousUpper = true;
            }
            else
            {
                builder.Append(currentChar);
                previousUpper = false;
            }
        }

        return builder.ToString();
    }
}
