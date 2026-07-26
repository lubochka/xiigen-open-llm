// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace MachineLearning;

// Multilayer Perceptron with one hidden layer, sigmoid activation, SGD.
// Architecture: input → hidden (sigmoid) → output (sigmoid)
// Trained with mini-batch backpropagation.
public sealed class MultilayerPerceptronAlgorithm
{
    private double[,] _w1 = new double[0, 0];  // input→hidden weights [hiddenSize, inputSize]
    private double[]  _b1 = [];                  // hidden biases
    private double[,] _w2 = new double[0, 0];  // hidden→output weights [outputSize, hiddenSize]
    private double[]  _b2 = [];                  // output biases
    private bool _fitted;

    public OperationResult<bool> Fit(
        List<double[]> X, List<double[]> Y,
        int hiddenSize = 8, double learningRate = 0.1, int epochs = 500)
    {
        if (X.Count == 0) return OperationResult<bool>.Fail("Empty training set");

        int inputSize  = X[0].Length;
        int outputSize = Y[0].Length;
        var rng        = new Random(42);

        _w1 = RandMatrix(hiddenSize, inputSize,  rng);
        _b1 = new double[hiddenSize];
        _w2 = RandMatrix(outputSize, hiddenSize, rng);
        _b2 = new double[outputSize];

        for (int epoch = 0; epoch < epochs; epoch++)
        {
            for (int s = 0; s < X.Count; s++)
            {
                // Forward
                double[] hidden = Sigmoid(Add(MatVec(_w1, X[s]), _b1));
                double[] output = Sigmoid(Add(MatVec(_w2, hidden), _b2));

                // Output layer error
                double[] outErr  = new double[outputSize];
                for (int o = 0; o < outputSize; o++)
                    outErr[o] = (Y[s][o] - output[o]) * SigmoidDeriv(output[o]);

                // Hidden layer error
                double[] hidErr = new double[hiddenSize];
                for (int h = 0; h < hiddenSize; h++)
                {
                    double sum = 0;
                    for (int o = 0; o < outputSize; o++)
                        sum += _w2[o, h] * outErr[o];
                    hidErr[h] = sum * SigmoidDeriv(hidden[h]);
                }

                // Weight updates
                for (int o = 0; o < outputSize; o++)
                {
                    for (int h = 0; h < hiddenSize; h++)
                        _w2[o, h] += learningRate * outErr[o] * hidden[h];
                    _b2[o] += learningRate * outErr[o];
                }
                for (int h = 0; h < hiddenSize; h++)
                {
                    for (int i = 0; i < inputSize; i++)
                        _w1[h, i] += learningRate * hidErr[h] * X[s][i];
                    _b1[h] += learningRate * hidErr[h];
                }
            }
        }

        _fitted = true;
        return OperationResult<bool>.Ok(true);
    }

    public OperationResult<double[]> Predict(double[] input)
    {
        if (!_fitted) return OperationResult<double[]>.Fail("Model not fitted");
        double[] hidden = Sigmoid(Add(MatVec(_w1, input), _b1));
        double[] output = Sigmoid(Add(MatVec(_w2, hidden), _b2));
        return OperationResult<double[]>.Ok(output);
    }

    // ── helpers ─────────────────────────────────────────────────────────────

    private static double[,] RandMatrix(int rows, int cols, Random rng)
    {
        var m = new double[rows, cols];
        double scale = Math.Sqrt(2.0 / cols);
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                m[r, c] = (rng.NextDouble() * 2 - 1) * scale;
        return m;
    }

    private static double[] MatVec(double[,] w, double[] v)
    {
        int rows = w.GetLength(0), cols = w.GetLength(1);
        var result = new double[rows];
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                result[r] += w[r, c] * v[c];
        return result;
    }

    private static double[] Add(double[] a, double[] b)
    {
        var result = new double[a.Length];
        for (int i = 0; i < a.Length; i++) result[i] = a[i] + b[i];
        return result;
    }

    private static double[] Sigmoid(double[] x)
    {
        var result = new double[x.Length];
        for (int i = 0; i < x.Length; i++)
            result[i] = 1.0 / (1.0 + Math.Exp(-x[i]));
        return result;
    }

    private static double SigmoidDeriv(double s) => s * (1 - s);
}

public sealed class MultilayerPerceptronService : BaseService
{
    private readonly MultilayerPerceptronAlgorithm _algo = new();

    public MultilayerPerceptronService(IModelStateStorage storage, IPredictionLogger logger)
        : base(storage, logger) { }

    public OperationResult<bool> Train(
        string operationId, List<double[]> X, List<double[]> Y,
        int hiddenSize = 8, double learningRate = 0.1, int epochs = 500)
    {
        if (Idempotency.TryGetResult(operationId, out _))
            return OperationResult<bool>.Ok(true);

        var result = _algo.Fit(X, Y, hiddenSize, learningRate, epochs);
        if (result.IsSuccess)
        {
            Idempotency.Record(operationId, new() { ["trained"] = true });
            Storage.Save("mlp", new() { ["fitted"] = true });
        }
        return result;
    }

    public OperationResult<double[]> Predict(double[] input) => _algo.Predict(input);
}
