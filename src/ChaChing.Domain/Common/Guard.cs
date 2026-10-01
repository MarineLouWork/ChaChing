namespace ChaChing.Domain.Common
{
    /// <summary>Technical preconditions shared by the whole domain. Business rules stay in the aggregates.</summary>
    public static class Guard
    {
        public static string ValidateEmptyString(string text, string paramName)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException($"The {paramName} is empty.", paramName);
            return text;
        }

        public static Guid? ValidateNullableGuid(Guid? guid, string paramName)
        {
            if (guid == Guid.Empty)
                throw new ArgumentException($"The {paramName} is empty.", paramName);
            return guid;
        }

        public static Guid ValidateGuid(Guid guid, string paramName)
        {
            if (guid == Guid.Empty)
                throw new ArgumentException($"The {paramName} is empty.", paramName);
            return guid;
        }

        public static TEnum ValidateEnum<TEnum>(TEnum value, string paramName) where TEnum : struct, Enum
        {
            if (!Enum.IsDefined(value))
                throw new ArgumentException($"The {paramName} is not valid.", paramName);
            return value;
        }
    }
}
