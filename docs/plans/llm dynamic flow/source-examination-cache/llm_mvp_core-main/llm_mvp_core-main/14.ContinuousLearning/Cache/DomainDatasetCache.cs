namespace ContinuousLearning;

using System.Text.Json;
using AgentInfrastructure;

// DomainDatasetCache: in-memory + JSON-backed store for training examples.
// One JSON file per canonical domain is written to cacheDir on FlushAsync.
// No SQLite dependency — ICodeWriter from 21.AgentInfrastructure handles persistence.
// FC-12: ISessionStorage first parameter → base(storage).
public sealed class DomainDatasetCache : BaseService, IDomainDatasetCache
{
    // In-memory store: domain string → ordered list of examples.
    private readonly Dictionary<string, List<TrainingExample>> _store = new(StringComparer.Ordinal);

    // File writer injected from 21.AgentInfrastructure — no concrete FileSystem here.
    private readonly ICodeWriter _writer;

    // Directory where JSON files are written by FlushAsync.
    private readonly string _cacheDir;

    public DomainDatasetCache(ISessionStorage storage, ICodeWriter writer, string cacheDir)
        : base(storage)
    {
        _writer   = writer;
        _cacheDir = cacheDir;
    }

    // Add one example to its domain bucket.
    public OperationResult<bool> Add(TrainingExample example)
    {
        if (example is null)
            return OperationResult<bool>.Fail("Example cannot be null.", "NULL_EXAMPLE");
        if (string.IsNullOrWhiteSpace(example.Domain))
            return OperationResult<bool>.Fail("Example domain cannot be empty.", "EMPTY_DOMAIN");

        if (!_store.TryGetValue(example.Domain, out var bucket))
        {
            bucket = new List<TrainingExample>();
            _store[example.Domain] = bucket;
        }
        bucket.Add(example);
        return OperationResult<bool>.Ok(true);
    }

    // Return all examples for one domain; empty list when domain is unknown.
    public OperationResult<IReadOnlyList<TrainingExample>> GetByDomain(string domain)
    {
        if (_store.TryGetValue(domain, out var bucket))
            return OperationResult<IReadOnlyList<TrainingExample>>.Ok(bucket);
        return OperationResult<IReadOnlyList<TrainingExample>>.Ok(
            Array.Empty<TrainingExample>());
    }

    // Sum of example counts across all domains.
    public OperationResult<int> TotalCount()
        => OperationResult<int>.Ok(_store.Values.Sum(b => b.Count));

    // Persist each domain bucket as a JSON file: {cacheDir}/{domain}.json.
    public async Task<OperationResult<bool>> FlushAsync()
    {
        foreach (var (domain, bucket) in _store)
        {
            string json = JsonSerializer.Serialize(bucket);
            string path = Path.Combine(_cacheDir, $"{domain}.json");
            var writeResult = await _writer.WriteFileAsync(path, json);
            if (!writeResult.IsSuccess)
                return OperationResult<bool>.Fail(writeResult.ErrorMessage, writeResult.ErrorCode);
        }
        return OperationResult<bool>.Ok(true);
    }
}
