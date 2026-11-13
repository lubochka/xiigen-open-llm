// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// ProbeNeuralNetwork — минимальная нейросеть для измерения скорости обучения.
// Архитектура: Embedding(vocabSize, 64) → Linear(64, 64) → ReLU → Linear(64, vocabSize)
// Loss: CrossEntropy на следующий токен.
// Все операции реализованы вручную (без внешних библиотек).
//
// Договор обратного прохода (изменение поведения от 30.10.2026, работа C04
// адресного плана исправлений номер 105 в папке docs/plans/
// replanning-possible-diffs/fixes-based-on-experiments). Прямой проход и ВСЕ
// производные считаются относительно одного состояния весов — того, что было
// до шага. Порядок строгий: производная по выходу, затем производная по
// скрытому состоянию, затем производная до среза отрицательных, затем
// производная по эмбеддингу — и только после этого применяются обновления ко
// второму слою, первому слою и эмбеддингу.
//
// Что было до правки и почему это важно. Обновление второго слоя применялось
// внутри того же цикла, который копил производную по скрытому состоянию, а
// обновление первого слоя — внутри цикла, копившего производную по
// эмбеддингу. В обе производные подмешивался посторонний член от уже
// применённого обновления, поэтому шаг обучения шёл не туда, куда указывает
// функция потерь. Числа этого расхождения на закреплённой сети из трёх
// токенов и двух скрытых нейронов: эмбеддинги 0,0203, веса первого слоя
// 0,0113, сдвиги первого слоя 0,0283 при верном втором слое 1,6e-7. После
// правки все семейства опускаются до порядка точности чисел.
//
// Следствие для прежних измерений. Любой результат, снятый этой сетью до
// 30.10.2026 (опыты E11, E28, E29, E31, E44, E45, E48 и их копии в
// experiments-v2), получен на неверном обратном проходе и не сравним с
// результатами после правки напрямую. Повторный научный прогон — отдельная
// работа и в этот ремонт не входит.
//
// Постоянная проверка договора: Tests/ExperimentFixes/
// ProbeNeuralNetworkGradientTests.cs — конечные разности по каждому параметру
// против применённого обновления, делённого на скорость обучения.
//
// Оптимизатор и скорость обучения этой правкой намеренно не менялись.
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

    // Один шаг обучения на паре (currentToken → nextToken). Возвращает loss.
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

        // Все производные ниже считаются относительно ОДНОГО состояния весов —
        // того, что было до шага. Поэтому ни одно обновление не применяется,
        // пока не посчитана последняя производная: иначе в неё подмешивается
        // посторонний член от уже сделанного обновления, и шаг идёт не туда,
        // куда указывает функция потерь.

        // Производная потерь по скрытому состоянию: накопитель на каждый нейрон.
        var dH = new float[_hiddenSize];
        // Вклад даёт каждый выходной токен словаря.
        for (int i = 0; i < _vocabSize; i++)
            // Внутри — каждая координата скрытого состояния.
            for (int j = 0; j < _hiddenSize; j++)
                // Берётся вес второго слоя ДО шага: его обновление применяется ниже.
                dH[j] += _w2[i, j] * dLogits[i];

        // Производная до среза отрицательных значений: накопитель на каждый нейрон.
        var dHPre = new float[_hiddenSize];
        // Срез пропускает производную только там, где вход слоя был положительным.
        for (int i = 0; i < _hiddenSize; i++)
            // Положительность берётся с того же состояния «до шага», что и выше.
            dHPre[i] = hPre[i] > 0 ? dH[i] : 0f;

        // Производная потерь по эмбеддингу входного токена: накопитель на координату.
        var dEmb = new float[_hiddenSize];
        // Вклад даёт каждый нейрон скрытого слоя.
        for (int i = 0; i < _hiddenSize; i++)
            // Внутри — каждая координата эмбеддинга.
            for (int j = 0; j < _hiddenSize; j++)
                // Берётся вес первого слоя ДО шага: его обновление тоже применяется ниже.
                dEmb[j] += _w1[i, j] * dHPre[i];

        // Все производные посчитаны на одном состоянии «до шага» — теперь обновления.
        // Второй слой: сдвиги и веса.
        for (int i = 0; i < _vocabSize; i++)
        {
            // Сдвиг выходного токена двигается на свою производную.
            _b2[i] -= _lr * dLogits[i];
            // Веса этого токена двигаются на производную, умноженную на скрытое состояние.
            for (int j = 0; j < _hiddenSize; j++)
                // Скрытое состояние h снято в прямом проходе, то есть тоже «до шага».
                _w2[i, j] -= _lr * dLogits[i] * h[j];
        }

        // Первый слой: сдвиги и веса.
        for (int i = 0; i < _hiddenSize; i++)
        {
            // Сдвиг нейрона двигается на свою производную.
            _b1[i] -= _lr * dHPre[i];
            // Веса нейрона двигаются на производную, умноженную на эмбеддинг.
            for (int j = 0; j < _hiddenSize; j++)
                // Массив emb — копия эмбеддинга, снятая в прямом проходе «до шага».
                _w1[i, j] -= _lr * dHPre[i] * emb[j];
        }

        // Эмбеддинг входного токена обновляется последним.
        for (int j = 0; j < _hiddenSize; j++)
            // Двигается только строка входного токена: остальные в этом шаге не участвовали.
            _embed[currentToken, j] -= _lr * dEmb[j];

        return loss;
    }

    // Оценить точность на валидационном наборе (без обновления весов).
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
