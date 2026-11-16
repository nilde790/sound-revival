namespace SoundRevival.Repository.Repository
{
    /// <summary>
    /// Builds LIKE/ILIKE patterns from user text, so the text is matched literally (VAL-6).
    /// </summary>
    public static class LikePattern
    {
        /// <summary>The escape character passed to <c>EF.Functions.ILike</c> together with an escaped pattern.</summary>
        public const string EscapeCharacter = "\\";

        /// <summary>
        /// Escapes the LIKE wildcards <c>%</c> and <c>_</c> and the escape character itself.
        /// </summary>
        public static string Escape(string input)
        {
            // The escape character must be doubled first: escaping it after the wildcards
            // would also double the backslashes just added in front of % and _.
            return input
                .Replace(EscapeCharacter, EscapeCharacter + EscapeCharacter)
                .Replace("%", EscapeCharacter + "%")
                .Replace("_", EscapeCharacter + "_");
        }

        /// <summary>Pattern matching values that contain <paramref name="input"/> literally.</summary>
        public static string Contains(string input) => $"%{Escape(input)}%";
    }
}
