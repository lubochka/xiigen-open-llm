// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// ProbeNeuralNetwork — a minimal neural network for measuring learning speed.
// Architecture: Embedding(vocabSize, 64) → Linear(64, 64) → ReLU → Linear(64, vocabSize)
// Loss: CrossEntropy on the next token.
// All operations are implemented by hand (no external libraries).
public sealed class ProbeNeuralNetwork
{
    private readonly int _vocabSize;
    private readonly int _hiddenSize;
    private readonly float _lr;
    private readonly Random _rng;

    private float[,] _embed;
    private float[,] _w1;
    private float[]  _b1;
    private float[,] _w2;
    private float[]  _b2;

    public ProbeNeuralNetwork(int vocabSize, int hiddenSize = 64, float lr = 0.01f, int seed = 42)
    {
        _vocabSize  = vocabSize;
        _hiddenSize = hiddenSize;
        _lr         = lr;
        _rng        = new Random(seed);

        _embed = InitMatrix(vocabSize,  hiddenSize, scale: MathF.Sqrt(2f / (vocabSize + hiddenSize)));
        _w1    = InitMatrix(hiddenSize, hiddenSize, scale: MathF.Sqrt(2f / (hiddenSize + hiddenSize)));
        _b1    = new float[hiddenSize];
        _w2    = InitMatrix(vocabSize,  hiddenSize, scale: MathF.Sqrt(2f / (hiddenSize + vocabSize)));
        _b2    = new float[vocabSize];
    }

    // A single training step on the pair (currentToken → nextToken). Returns loss.
    public float TrainStep(int currentToken, int nextToken)
    {
        // Forward
        var emb = new float[_hiddenSize];
        for (int j = 0; j < _hiddenSize; j++)
            emb[j] = _embed[currentToken, j];

        var hPre = new float[_hiddenSize];
        for (int i = 0; i < _hiddenSize; i++)
        {
            float sum = _b1[i];
            for (int j = 0; j < _hiddenSize; j++)
                sum += _w1[i, j] * emb[j];
            hPre[i] = sum;
        }

        var h = new float[_hiddenSize];
        for (int i = 0; i < _hiddenSize; i++)
            h[i] = hPre[i] > 0 ? hPre[i] : 0f;

        var logits = new float[_vocabSize];
        for (int i = 0; i < _vocabSize; i++)
        {
            float sum = _b2[i];
            for (int j = 0; j < _hiddenSize; j++)
                sum += _w2[i, j] * h[j];
            logits[i] = sum;
        }

        float maxLogit  = logits.Max();
        var expLogits   = logits.Select(x => MathF.Exp(x - maxLogit)).ToArray();
        float sumExp    = expLogits.Sum();
        var probs       = expLogits.Select(x => x / sumExp).ToArray();
        float loss      = -MathF.Log(MathF.Max(probs[nextToken], 1e-7f));

        // Backward
        var dLogits = probs.ToArray();
        dLogits[nextToken] -= 1f;

        var dH = new float[_hiddenSize];
        for (int i = 0; i < _vocabSize; i++)
        {
            _b2[i] -= _lr * dLogits[i];
            for (int j = 0; j < _hiddenSize; j++)
            {
                _w2[i, j] -= _lr * dLogits[i] * h[j];
                dH[j]     += _w2[i, j] * dLogits[i];
            }
        }

        var dHPre = new float[_hiddenSize];
        for (int i = 0; i < _hiddenSize; i++)
            dHPre[i] = hPre[i] > 0 ? dH[i] : 0f;

        var dEmb = new float[_hiddenSize];
        for (int i = 0; i < _hiddenSize; i++)
        {
            _b1[i] -= _lr * dHPre[i];
            for (int j = 0; j < _hiddenSize; j++)
            {
                _w1[i, j] -= _lr * dHPre[i] * emb[j];
                dEmb[j]   += _w1[i, j] * dHPre[i];
            }
        }

        for (int j = 0; j < _hiddenSize; j++)
            _embed[currentToken, j] -= _lr * dEmb[j];

        return loss;
    }

    // Evaluate accuracy on the validation set (without updating weights).
    public double Evaluate(IReadOnlyList<int> tokens)
    {
        int correct = 0;
        for (int i = 0; i < tokens.Count - 1; i++)
        {
            int predicted = Predict(tokens[i]);
            if (predicted == tokens[i + 1]) correct++;
        }
        return tokens.Count > 1 ? correct / (double)(tokens.Count - 1) : 0;
    }

    private int Predict(int token)
    {
        var emb = new float[_hiddenSize];
        for (int j = 0; j < _hiddenSize; j++) emb[j] = _embed[token, j];

        var h = new float[_hiddenSize];
        for (int i = 0; i < _hiddenSize; i++)
        {
            float sum = _b1[i];
            for (int j = 0; j < _hiddenSize; j++) sum += _w1[i, j] * emb[j];
            h[i] = sum > 0 ? sum : 0f;
        }

        int best = 0; float bestVal = float.MinValue;
        for (int i = 0; i < _vocabSize; i++)
        {
            float sum = _b2[i];
            for (int j = 0; j < _hiddenSize; j++) sum += _w2[i, j] * h[j];
            if (sum > bestVal) { bestVal = sum; best = i; }
        }
        return best;
    }

    private float[,] InitMatrix(int rows, int cols, float scale)
    {
        var m = new float[rows, cols];
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                m[i, j] = (float)(_rng.NextDouble() * 2 - 1) * scale;
        return m;
    }
}
