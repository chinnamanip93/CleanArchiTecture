namespace Domain.Exceptions
{
    /// <summary>
    /// Thrown when a requested resource does not exist.
    /// GlobalExceptionMiddleware maps this to HTTP 404.
    /// </summary>
    public class NotFoundException : Exception
    {
        public NotFoundException(string entityName, object key)
            : base($"{entityName} with key '{key}' was not found.") { }
    }
}
