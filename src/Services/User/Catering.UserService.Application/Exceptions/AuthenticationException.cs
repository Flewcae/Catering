namespace Catering.UserService.Application.Exceptions;

public sealed class AuthenticationException(string message) : Exception(message);
