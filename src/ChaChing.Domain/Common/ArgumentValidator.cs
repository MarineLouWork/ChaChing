namespace ChaChing.Domain.Common
{
    /// <summary>Technical preconditions shared by the whole domain. Business rules stay in the aggregates.</summary>
    public static class ArgumentValidator
    {
        /// <summary>Refuses null, empty and blank text; returns the text trimmed.</summary>
        public static string NonBlankText(string text, string paramName)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException($"The {paramName} is empty.", paramName);
            return text.Trim();
        }

        public static Guid? NullableGuid(Guid? guid, string paramName)
        {
            if (guid == System.Guid.Empty)
                throw new ArgumentException($"The {paramName} is empty.", paramName);
            return guid;
        }

        public static Guid Guid(Guid guid, string paramName)
        {
            if (guid == System.Guid.Empty)
                throw new ArgumentException($"The {paramName} is empty.", paramName);
            return guid;
        }

        public static TEnum Enum<TEnum>(TEnum value, string paramName) where TEnum : struct, System.Enum
        {
            if (!System.Enum.IsDefined(value))
                throw new ArgumentException($"The {paramName} is not valid.", paramName);
            return value;
        }
    }
}
