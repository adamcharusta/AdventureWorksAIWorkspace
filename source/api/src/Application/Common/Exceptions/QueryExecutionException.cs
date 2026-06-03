namespace AdventureWorksAIWorkspace.Application.Common.Exceptions;

/// <summary>
/// Thrown when a validated, read-only query fails while executing against AdventureWorks (for
/// example, an invalid object name or a timeout). Distinct from infrastructure connectivity
/// failures, which are not wrapped in this exception.
/// </summary>
public sealed class QueryExecutionException(string message, Exception? innerException = null)
    : Exception(message, innerException);
