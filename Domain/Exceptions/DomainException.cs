namespace Domain.Exceptions
{
    /// <summary>
    /// Thrown for business rule violations.
    /// GlobalExceptionMiddleware maps this to HTTP 400.
    /// </summary>
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message) { }
    }
}
