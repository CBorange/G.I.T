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
        public NotFoundException(string resourceName, object key)
            : base($"{resourceName} was not found. key={key}")
        {
            ResourceName = resourceName;
            Key = key;
        }

        public string ResourceName { get; }
        public object Key { get; }
    }
}
