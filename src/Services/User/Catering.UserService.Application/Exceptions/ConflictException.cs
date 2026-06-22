namespace Catering.UserService.Application.Exceptions;

public sealed class ConflictException(string message) : Exception(message);
