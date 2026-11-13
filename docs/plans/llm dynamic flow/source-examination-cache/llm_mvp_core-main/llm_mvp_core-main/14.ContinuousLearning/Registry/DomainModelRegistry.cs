namespace ContinuousLearning;

using System.Text.Json;
using AgentInfrastructure;

// DomainModelRegistry: stores and retrieves LoRA adapters per domain.
// In-memory map keyed by domain; optionally persisted to registry.json via ICodeWriter.
// FC-12: ISessionStorage first parameter → base(storage).
public sealed class DomainModelRegistry : BaseService, IDomainModelRegistry
{
    // One adapter per canonical domain string — latest training always wins.
    private readonly Dictionary<string, ModelAdapter> _registry = new(StringComparer.Ordinal);

    // File writer for persisting registry.json to disk.
    private readonly ICodeWriter _writer;

    // Directory where registry.json and adapter files live.
    private readonly string _registryDir;

    public DomainModelRegistry(ISessionStorage storage, ICodeWriter writer, string registryDir = "adapters")
        : base(storage)
    {
        _writer      = writer;
        _registryDir = registryDir;
    }

    // Register or overwrite the adapter for adapter.Domain.
    public OperationResult<bool> Register(ModelAdapter adapter)
    {
        if (adapter is null)
            return OperationResult<bool>.Fail("Adapter cannot be null.", "NULL_ADAPTER");
        _registry[adapter.Domain] = adapter;
        return OperationResult<bool>.Ok(true);
    }

    // Return the adapter for domain, or null Value when none trained yet (still IsSuccess).
    public OperationResult<ModelAdapter?> GetAdapter(string domain)
    {
        _registry.TryGetValue(domain, out var adapter);
        return OperationResult<ModelAdapter?>.Ok(adapter);
    }

    // Return all registered adapters as an ordered list.
    public OperationResult<IReadOnlyList<ModelAdapter>> ListAll()
        => OperationResult<IReadOnlyList<ModelAdapter>>.Ok(
               _registry.Values.OrderBy(a => a.Domain).ToList());
}
