namespace Catering.UserService.Application.Exceptions;

public sealed class ValidationException(string message) : Exception(message);
