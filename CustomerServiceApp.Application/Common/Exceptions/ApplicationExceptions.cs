namespace CustomerServiceApp.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when a requested entity is not found
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string entityName, object key)
        : base($"Entity '{entityName}' with key '{key}' was not found.")
    {
        EntityName = entityName;
        Key = key;
    }
    
    public string EntityName { get; }
    public object Key { get; }
}

/// <summary>
/// Exception thrown when validation fails
/// </summary>
public class ValidationException : Exception
{
    public ValidationException(string message) : base(message)
    {
        Errors = new List<string> { message };
    }
    
    public ValidationException(IEnumerable<string> errors) 
        : base("One or more validation errors occurred.")
    {
        Errors = errors.ToList();
    }
    
    public IReadOnlyList<string> Errors { get; }
}

/// <summary>
/// Exception thrown when business rules are violated
/// </summary>
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message)
    {
    }
    
    public BusinessRuleException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when a conflict occurs (e.g., duplicate entity)
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
    
    public ConflictException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
