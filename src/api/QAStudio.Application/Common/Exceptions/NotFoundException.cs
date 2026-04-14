namespace QAStudio.Application.Common.Exceptions;

public class NotFoundException : Exception
{
    public string EntityName { get; }
    public object? EntityKey { get; }

    public NotFoundException(string entityName, object? entityKey = null)
        : base($"{entityName} with ID '{entityKey}' was not found.")
    {
        EntityName = entityName;
        EntityKey = entityKey;
    }
}
