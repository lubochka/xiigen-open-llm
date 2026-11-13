namespace MachineLearning;

public sealed class InMemoryModelStateStorage : IModelStateStorage
{
    private readonly Dictionary<string, Dictionary<string, object>> _store = [];

    public OperationResult<bool> Save(string name, Dictionary<string, object> p)
    {
        _store[name] = new Dictionary<string, object>(p);
        return OperationResult<bool>.Ok(true);
    }

    public OperationResult<Dictionary<string, object>> Load(string name)
        => _store.TryGetValue(name, out var p)
            ? OperationResult<Dictionary<string, object>>.Ok(new Dictionary<string, object>(p))
            : OperationResult<Dictionary<string, object>>.Fail($"Model '{name}' not found");

    public OperationResult<List<string>> List()
        => OperationResult<List<string>>.Ok([.. _store.Keys]);
}

public sealed class InMemoryPredictionLogger : IPredictionLogger
{
    public List<(string algo, Dictionary<string, object> input, Dictionary<string, object> output)> Entries = [];

    public OperationResult<bool> Log(string algo, Dictionary<string, object> input, Dictionary<string, object> output)
    {
        Entries.Add((algo, input, output));
        return OperationResult<bool>.Ok(true);
    }
}

public sealed class InMemoryTrainingDataProvider(
    List<double[]> features, List<object> labels) : ITrainingDataProvider
{
    public OperationResult<(List<double[]>, List<object>)> Load(string _)
        => OperationResult<(List<double[]>, List<object>)>.Ok((features, labels));
}
