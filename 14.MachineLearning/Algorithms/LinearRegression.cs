// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace MachineLearning;

// Ordinary Least Squares linear regression: Y = slope*X + intercept
// Closed-form solution: slope = Σ(xi-x̄)(yi-ȳ) / Σ(xi-x̄)²
public sealed class LinearRegressionAlgorithm
{
    private double _slope;
    private double _intercept;
    private bool   _fitted;

    public OperationResult<Dictionary<string, double>> Fit(
        List<double> inputs, List<double> outputs)
    {
        if (inputs.Count < 2)
            return OperationResult<Dictionary<string, double>>.Fail("Need >= 2 points");
        if (inputs.Count != outputs.Count)
            return OperationResult<Dictionary<string, double>>.Fail("Input/output size mismatch");

        int    n         = inputs.Count;
        double meanX     = inputs.Average();
        double meanY     = outputs.Average();
        double numerator = 0, denominator = 0;

        for (int i = 0; i < n; i++)
        {
            double dx  = inputs[i]  - meanX;
            double dy  = outputs[i] - meanY;
            numerator  += dx * dy;
            denominator += dx * dx;
        }

        if (Math.Abs(denominator) < 1e-12)
            return OperationResult<Dictionary<string, double>>.Fail("All inputs identical; slope undefined");

        _slope     = numerator / denominator;
        _intercept = meanY - _slope * meanX;
        _fitted    = true;

        return OperationResult<Dictionary<string, double>>.Ok(
            new() { ["slope"] = _slope, ["intercept"] = _intercept });
    }

    public OperationResult<double> Predict(double input)
    {
        if (!_fitted)
            return OperationResult<double>.Fail("Model not fitted");
        return OperationResult<double>.Ok(_slope * input + _intercept);
    }

    public void LoadParameters(double slope, double intercept)
        => (_slope, _intercept, _fitted) = (slope, intercept, true);
}

public sealed class LinearRegressionService : BaseService
{
    private readonly LinearRegressionAlgorithm _algo = new();

    public LinearRegressionService(IModelStateStorage storage, IPredictionLogger logger)
        : base(storage, logger) { }

    public OperationResult<Dictionary<string, double>> Train(
        string operationId, List<double> inputs, List<double> outputs)
    {
        if (Idempotency.TryGetResult(operationId, out var cached))
            return OperationResult<Dictionary<string, double>>.Ok(
                cached.ToDictionary(k => k.Key, k => (double)k.Value));

        var result = _algo.Fit(inputs, outputs);
        if (!result.IsSuccess) return result;

        var asObj = result.Value!.ToDictionary(k => k.Key, k => (object)k.Value);
        Storage.Save("linear_regression", asObj);
        Idempotency.Record(operationId, asObj);
        return result;
    }

    public OperationResult<double> Predict(double input)
    {
        var result = _algo.Predict(input);
        if (result.IsSuccess)
            Logger.Log("LinearRegression",
                new() { ["input"] = input },
                new() { ["output"] = result.Value! });
        return result;
    }
}
