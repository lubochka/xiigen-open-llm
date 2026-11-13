namespace MachineLearning;

public interface IModelStateStorage
{
    OperationResult<bool>                       Save(string modelName, Dictionary<string, object> parameters);
    OperationResult<Dictionary<string, object>> Load(string modelName);
    OperationResult<List<string>>               List();
}

public interface ITrainingDataProvider
{
    OperationResult<(List<double[]> FeatureVectors, List<object> Labels)> Load(string datasetName);
}

public interface IPredictionLogger
{
    OperationResult<bool> Log(string algorithmName,
        Dictionary<string, object> input, Dictionary<string, object> output);
}
