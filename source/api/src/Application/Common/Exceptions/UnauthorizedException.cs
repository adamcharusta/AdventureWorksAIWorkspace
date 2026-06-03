namespace AdventureWorksAIWorkspace.Application.Common.Exceptions;

public sealed class UnauthorizedException(string message) : Exception(message);
