namespace MachineLearning;

// Routes algorithm requests to the correct service.
// All external callers go through this dispatcher — no entity-specific handlers.
public sealed class MachineLearningDispatcher
{
    private readonly Dictionary<string, object> _services;

    public MachineLearningDispatcher(
        LinearRegressionService    linearRegression,
        NaiveBayesService          naiveBayes,
        MultilayerPerceptronService multilayerPerceptron,
        KMeansService              kMeans)
    {
        _services = new()
        {
            ["linear_regression"]     = linearRegression,
            ["naive_bayes"]           = naiveBayes,
            ["multilayer_perceptron"] = multilayerPerceptron,
            ["kmeans"]                = kMeans
        };
    }

    public OperationResult<object> Dispatch(string algorithmName, Dictionary<string, object> request)
    {
        if (!_services.TryGetValue(algorithmName, out var service))
            return OperationResult<object>.Fail($"Unknown algorithm: {algorithmName}");

        return OperationResult<object>.Ok(service);
    }

    public IReadOnlyList<string> AvailableAlgorithms => [.. _services.Keys];
}
