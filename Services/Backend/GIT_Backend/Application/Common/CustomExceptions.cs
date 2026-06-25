namespace GIT_Backend.Application.Common
{
    public sealed class BadRequestException : Exception
    {
        public BadRequestException(string message) : base(message)
        {
        }
    }

    public sealed class NotFoundException : Exception
    {
        public NotFoundException(string resourceName, object key, string? additionalMsg = null)
            : base($"{resourceName} was not found. key={key}\nAdditional Message={additionalMsg ?? string.Empty}")
        {
            ResourceName = resourceName;
            Key = key;
            AdditionalMsg = additionalMsg;
        }

        public string ResourceName { get; }
        public object Key { get; }
        public string? AdditionalMsg { get; }
    }
}
