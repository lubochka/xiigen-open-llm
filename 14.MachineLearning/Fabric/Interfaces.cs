// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

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
