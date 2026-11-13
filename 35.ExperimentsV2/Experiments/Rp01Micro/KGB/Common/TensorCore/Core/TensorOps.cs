// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TensorCore.Core;

// Все математические операции над тензорами.
// Каждая операция: (1) вычисляет результат, (2) регистрирует backward если нужно.
public static class TensorOps
{
    // Тензор нулей той же формы что и образец
    public static Tensor ZerosLike(Tensor t) =>
        new(new float[t.Shape.TotalElements], t.Shape);

    // Тензор единиц той же формы что и образец
    public static Tensor OnesLike(Tensor t)
    {
        var data = new float[t.Shape.TotalElements];
        Array.Fill(data, 1.0f);
        return new Tensor(data, t.Shape);
    }

    // Создать тензор нулей заданной формы
    public static Tensor Zeros(params int[] dims) =>
        new(new TensorShape(dims));

    // Создать тензор единиц заданной формы
    public static Tensor Ones(params int[] dims)
    {
        var shape = new TensorShape(dims);
        var data  = new float[shape.TotalElements];
        Array.Fill(data, 1.0f);
        return new Tensor(data, shape);
    }

    // Поэлементное сложение: grad передаётся обоим входам без изменений (d(a+b)/da = 1)
    public static Tensor Add(Tensor a, Tensor b)
        => Add(a, b, CancellationToken.None);

    public static Tensor Add(Tensor a, Tensor b, CancellationToken cancellationToken)
    {
        AssertSameShape(a, b, "Add");

        var data = new float[a.Shape.TotalElements];
        for (int i = 0; i < data.Length; i++)
        {
            if ((i & 4095) == 0)
                cancellationToken.ThrowIfCancellationRequested();
            data[i] = a.Data[i] + b.Data[i];
        }

        var result = new Tensor(data, a.Shape);

        if (a.RequiresGrad || b.RequiresGrad)
        {
            result.RequiresGrad = true;
            result.Children.Add(a);
            result.Children.Add(b);
            result.BackwardFunc = () =>
            {
                if (a.RequiresGrad) AccumulateGrad(a, result.Grad!);
                if (b.RequiresGrad) AccumulateGrad(b, result.Grad!);
            };
        }

        return result;
    }

    // Прибавить скаляр к каждому элементу (broadcast)
    public static Tensor AddScalar(Tensor a, float scalar)
        => AddScalar(a, scalar, CancellationToken.None);

    public static Tensor AddScalar(
        Tensor a,
        float scalar,
        CancellationToken cancellationToken)
    {
        var data = new float[a.Shape.TotalElements];
        for (int i = 0; i < data.Length; i++)
        {
            if ((i & 4095) == 0)
                cancellationToken.ThrowIfCancellationRequested();
            data[i] = a.Data[i] + scalar;
        }

        var result = new Tensor(data, a.Shape);

        if (a.RequiresGrad)
        {
            result.RequiresGrad = true;
            result.Children.Add(a);
            result.BackwardFunc = () => AccumulateGrad(a, result.Grad!);
        }

        return result;
    }

    // Поэлементное умножение: grad_a = grad * b, grad_b = grad * a
    public static Tensor Multiply(Tensor a, Tensor b)
        => Multiply(a, b, CancellationToken.None);

    public static Tensor Multiply(Tensor a, Tensor b, CancellationToken cancellationToken)
    {
        AssertSameShape(a, b, "Multiply");

        var data = new float[a.Shape.TotalElements];
        for (int i = 0; i < data.Length; i++)
        {
            if ((i & 4095) == 0)
                cancellationToken.ThrowIfCancellationRequested();
            data[i] = a.Data[i] * b.Data[i];
        }

        var result = new Tensor(data, a.Shape);

        if (a.RequiresGrad || b.RequiresGrad)
        {
            result.RequiresGrad = true;
            result.Children.Add(a);
            result.Children.Add(b);
            result.BackwardFunc = () =>
            {
                if (a.RequiresGrad)
                {
                    var g = new float[a.Shape.TotalElements];
                    for (int i = 0; i < g.Length; i++)
                        g[i] = result.Grad!.Data[i] * b.Data[i];
                    AccumulateGrad(a, new Tensor(g, a.Shape));
                }
                if (b.RequiresGrad)
                {
                    var g = new float[b.Shape.TotalElements];
                    for (int i = 0; i < g.Length; i++)
                        g[i] = result.Grad!.Data[i] * a.Data[i];
                    AccumulateGrad(b, new Tensor(g, b.Shape));
                }
            };
        }

        return result;
    }

    // Asymmetric right-aligned broadcast multiplication. The result shape is A's shape.
    public static Tensor MultiplyBroadcast(Tensor a, Tensor b)
        => MultiplyBroadcast(a, b, CancellationToken.None);

    public static Tensor MultiplyBroadcast(
        Tensor a,
        Tensor b,
        CancellationToken cancellationToken)
    {
        if (b.Shape.Rank > a.Shape.Rank)
            throw new ArgumentException("MultiplyBroadcast requires B rank to be no greater than A rank.");

        int rankOffset = a.Shape.Rank - b.Shape.Rank;
        for (int dimension = 0; dimension < b.Shape.Rank; dimension++)
        {
            int aDimension = a.Shape.Dimensions[rankOffset + dimension];
            int bDimension = b.Shape.Dimensions[dimension];
            if (bDimension != 1 && bDimension != aDimension)
            {
                throw new ArgumentException(
                    $"MultiplyBroadcast: incompatible dimensions {a.Shape} and {b.Shape}.");
            }
        }

        var data = new float[a.Shape.TotalElements];
        var mappedBIndices = new int[data.Length];
        for (int flat = 0; flat < data.Length; flat++)
        {
            if ((flat & 4095) == 0)
                cancellationToken.ThrowIfCancellationRequested();
            var indices = FlatToIndices(flat, a.Shape);
            int bFlat = 0;
            for (int dimension = 0; dimension < b.Shape.Rank; dimension++)
            {
                int bDimension = b.Shape.Dimensions[dimension];
                int bIndex = bDimension == 1 ? 0 : indices[rankOffset + dimension];
                bFlat = bFlat * bDimension + bIndex;
            }

            mappedBIndices[flat] = bFlat;
            data[flat] = a.Data[flat] * b.Data[bFlat];
        }

        var result = new Tensor(data, a.Shape);
        if (a.RequiresGrad || b.RequiresGrad)
        {
            result.RequiresGrad = true;
            if (a.RequiresGrad) result.Children.Add(a);
            if (b.RequiresGrad) result.Children.Add(b);
            result.BackwardFunc = () =>
            {
                if (a.RequiresGrad)
                {
                    var gradA = new float[a.Shape.TotalElements];
                    for (int flat = 0; flat < gradA.Length; flat++)
                        gradA[flat] = result.Grad!.Data[flat] * b.Data[mappedBIndices[flat]];
                    AccumulateGrad(a, new Tensor(gradA, a.Shape));
                }

                if (b.RequiresGrad)
                {
                    var gradB = new float[b.Shape.TotalElements];
                    for (int flat = 0; flat < a.Shape.TotalElements; flat++)
                        gradB[mappedBIndices[flat]] += result.Grad!.Data[flat] * a.Data[flat];
                    AccumulateGrad(b, new Tensor(gradB, b.Shape));
                }
            };
        }

        return result;
    }

    // Умножить каждый элемент на скаляр
    public static Tensor Scale(Tensor x, float scalar)
        => Scale(x, scalar, CancellationToken.None);

    public static Tensor Scale(
        Tensor x,
        float scalar,
        CancellationToken cancellationToken)
    {
        var data = new float[x.Shape.TotalElements];
        for (int i = 0; i < data.Length; i++)
        {
            if ((i & 4095) == 0)
                cancellationToken.ThrowIfCancellationRequested();
            data[i] = x.Data[i] * scalar;
        }

        var result = new Tensor(data, x.Shape);

        if (x.RequiresGrad)
        {
            result.RequiresGrad = true;
            result.Children.Add(x);
            result.BackwardFunc = () =>
            {
                var g = new float[x.Shape.TotalElements];
                for (int i = 0; i < g.Length; i++)
                    g[i] = result.Grad!.Data[i] * scalar;
                AccumulateGrad(x, new Tensor(g, x.Shape));
            };
        }

        return result;
    }

    // Поэлементное вычитание: grad_a = grad, grad_b = -grad
    public static Tensor Subtract(Tensor a, Tensor b)
    {
        AssertSameShape(a, b, "Subtract");

        var data = new float[a.Shape.TotalElements];
        for (int i = 0; i < data.Length; i++)
            data[i] = a.Data[i] - b.Data[i];

        var result = new Tensor(data, a.Shape);

        if (a.RequiresGrad || b.RequiresGrad)
        {
            result.RequiresGrad = true;
            result.Children.Add(a);
            result.Children.Add(b);
            result.BackwardFunc = () =>
            {
                if (a.RequiresGrad) AccumulateGrad(a, result.Grad!);
                if (b.RequiresGrad)
                {
                    var g = new float[b.Shape.TotalElements];
                    for (int i = 0; i < g.Length; i++)
                        g[i] = -result.Grad!.Data[i];
                    AccumulateGrad(b, new Tensor(g, b.Shape));
                }
            };
        }

        return result;
    }

    // Возведение каждого элемента в степень: d(x^n)/dx = n * x^(n-1)
    public static Tensor Pow(Tensor x, float exponent)
        => Pow(x, exponent, CancellationToken.None);

    public static Tensor Pow(
        Tensor x,
        float exponent,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var data = new float[x.Shape.TotalElements];
        for (int i = 0; i < data.Length; i++)
        {
            if (i > 0 && (i & 4095) == 0)
            {
                TensorKernelProgress.Report("TensorOps.Pow", i);
                cancellationToken.ThrowIfCancellationRequested();
            }
            data[i] = MathF.Pow(x.Data[i], exponent);
        }

        var result = new Tensor(data, x.Shape);

        if (x.RequiresGrad)
        {
            result.RequiresGrad = true;
            result.Children.Add(x);
            result.BackwardFunc = () =>
            {
                var g = new float[x.Shape.TotalElements];
                for (int i = 0; i < g.Length; i++)
                    g[i] = exponent * MathF.Pow(x.Data[i], exponent - 1) * result.Grad!.Data[i];
                AccumulateGrad(x, new Tensor(g, x.Shape));
            };
        }

        return result;
    }

    // Поэлементный натуральный логарифм
    public static Tensor Log(Tensor x)
    {
        var data = new float[x.Shape.TotalElements];
        for (int i = 0; i < data.Length; i++)
            data[i] = MathF.Log(x.Data[i]);

        var result = new Tensor(data, x.Shape);

        if (x.RequiresGrad)
        {
            result.RequiresGrad = true;
            result.Children.Add(x);
            result.BackwardFunc = () =>
            {
                var g = new float[x.Shape.TotalElements];
                for (int i = 0; i < g.Length; i++)
                    g[i] = result.Grad!.Data[i] / x.Data[i];
                AccumulateGrad(x, new Tensor(g, x.Shape));
            };
        }

        return result;
    }

    // Поэлементная экспонента
    public static Tensor Exp(Tensor x)
    {
        var data = new float[x.Shape.TotalElements];
        for (int i = 0; i < data.Length; i++)
            data[i] = MathF.Exp(x.Data[i]);

        var result = new Tensor(data, x.Shape);

        if (x.RequiresGrad)
        {
            result.RequiresGrad = true;
            result.Children.Add(x);
            result.BackwardFunc = () =>
            {
                var g = new float[x.Shape.TotalElements];
                for (int i = 0; i < g.Length; i++)
                    g[i] = result.Grad!.Data[i] * result.Data[i]; // d(e^x)/dx = e^x = result
                AccumulateGrad(x, new Tensor(g, x.Shape));
            };
        }

        return result;
    }

    // Поэлементный квадратный корень
    public static Tensor Sqrt(Tensor x)
    {
        var data = new float[x.Shape.TotalElements];
        for (int i = 0; i < data.Length; i++)
            data[i] = MathF.Sqrt(x.Data[i]);

        var result = new Tensor(data, x.Shape);

        if (x.RequiresGrad)
        {
            result.RequiresGrad = true;
            result.Children.Add(x);
            result.BackwardFunc = () =>
            {
                var g = new float[x.Shape.TotalElements];
                for (int i = 0; i < g.Length; i++)
                    g[i] = result.Grad!.Data[i] / (2f * result.Data[i]); // d(sqrt(x))/dx = 1/(2*sqrt(x))
                AccumulateGrad(x, new Tensor(g, x.Shape));
            };
        }

        return result;
    }

    // Поэлементная обратная величина: rsqrt(x) = 1/sqrt(x), используется в RmsNorm
    public static Tensor RSqrt(Tensor x)
        => RSqrt(x, CancellationToken.None);

    public static Tensor RSqrt(Tensor x, CancellationToken cancellationToken)
    {
        var data = new float[x.Shape.TotalElements];
        for (int i = 0; i < data.Length; i++)
        {
            if ((i & 4095) == 0)
                cancellationToken.ThrowIfCancellationRequested();
            data[i] = 1.0f / MathF.Sqrt(x.Data[i]);
        }

        var result = new Tensor(data, x.Shape);

        if (x.RequiresGrad)
        {
            result.RequiresGrad = true;
            result.Children.Add(x);
            result.BackwardFunc = () =>
            {
                var g = new float[x.Shape.TotalElements];
                for (int i = 0; i < g.Length; i++)
                    g[i] = -0.5f * result.Grad!.Data[i] * result.Data[i] * result.Data[i] * result.Data[i];
                AccumulateGrad(x, new Tensor(g, x.Shape));
            };
        }

        return result;
    }

    // Нормальное распределение (LogSoftmax) — log(softmax(x))
    // Более численно стабильно чем log(softmax(x)) отдельно
    public static Tensor LogSoftmax(Tensor x, int dim)
    {
        int normalizedDim = dim < 0 ? x.Shape.Rank + dim : dim;
        var resultData = x.Data.ToArray();

        int outerSize = 1;
        for (int i = 0; i < normalizedDim; i++)
            outerSize *= x.Shape.Dimensions[i];

        int dimSize   = x.Shape.Dimensions[normalizedDim];
        int innerSize = x.Shape.TotalElements / (outerSize * dimSize);

        for (int outer = 0; outer < outerSize; outer++)
        for (int inner = 0; inner < innerSize; inner++)
        {
            // Вычитаем максимум для численной стабильности
            float maxVal = float.NegativeInfinity;
            for (int d = 0; d < dimSize; d++)
            {
                int idx = (outer * dimSize + d) * innerSize + inner;
                if (resultData[idx] > maxVal) maxVal = resultData[idx];
            }

            // log_softmax(x_i) = x_i - max - log(sum(exp(x_j - max)))
            float logSumExp = 0f;
            for (int d = 0; d < dimSize; d++)
            {
                int idx = (outer * dimSize + d) * innerSize + inner;
                logSumExp += MathF.Exp(resultData[idx] - maxVal);
            }
            logSumExp = MathF.Log(logSumExp);

            for (int d = 0; d < dimSize; d++)
            {
                int idx = (outer * dimSize + d) * innerSize + inner;
                resultData[idx] = resultData[idx] - maxVal - logSumExp;
            }
        }

        var result = new Tensor(resultData, x.Shape);

        if (x.RequiresGrad)
        {
            result.RequiresGrad = true;
            result.Children.Add(x);
            result.BackwardFunc = () =>
            {
                // grad_x[i] = grad_out[i] - softmax[i] * sum(grad_out)
                var gradData = new float[x.Shape.TotalElements];

                for (int outer = 0; outer < outerSize; outer++)
                for (int inner = 0; inner < innerSize; inner++)
                {
                    float sumGrad = 0f;
                    for (int d = 0; d < dimSize; d++)
                    {
                        int idx = (outer * dimSize + d) * innerSize + inner;
                        sumGrad += result.Grad!.Data[idx];
                    }
                    for (int d = 0; d < dimSize; d++)
                    {
                        int idx = (outer * dimSize + d) * innerSize + inner;
                        // softmax = exp(log_softmax)
                        gradData[idx] = result.Grad!.Data[idx]
                            - MathF.Exp(result.Data[idx]) * sumGrad;
                    }
                }

                AccumulateGrad(x, new Tensor(gradData, x.Shape));
            };
        }

        return result;
    }

    // Сумма по всем элементам → скаляр (тензор [1])
    public static Tensor Sum(Tensor x)
    {
        float total = 0f;
        foreach (var v in x.Data) total += v;

        var result = new Tensor(new float[] { total }, new TensorShape(1));

        if (x.RequiresGrad)
        {
            result.RequiresGrad = true;
            result.Children.Add(x);
            result.BackwardFunc = () =>
            {
                // Градиент суммы = 1 для каждого входного элемента
                var g = new float[x.Shape.TotalElements];
                Array.Fill(g, result.Grad!.Data[0]);
                AccumulateGrad(x, new Tensor(g, x.Shape));
            };
        }

        return result;
    }

    // Среднее по указанному измерению.
    // keepDims=true: форма результата совпадает, но ось = 1
    public static Tensor Mean(Tensor x, int axis, bool keepDims = true)
        => Mean(x, axis, keepDims, CancellationToken.None);

    public static Tensor Mean(
        Tensor x,
        int axis,
        bool keepDims,
        CancellationToken cancellationToken)
    {
        int normalizedAxis = axis < 0 ? x.Shape.Rank + axis : axis;
        int axisSize       = x.Shape.Dimensions[normalizedAxis];

        int[] resultDims = x.Shape.Dimensions.ToArray();
        if (keepDims)
            resultDims[normalizedAxis] = 1;
        else
            resultDims = resultDims.Where((_, i) => i != normalizedAxis).ToArray();

        var resultShape = new TensorShape(resultDims);
        var resultData  = new float[resultShape.TotalElements];

        SumAlongAxis(
            x.Data,
            x.Shape,
            resultData,
            resultShape,
            normalizedAxis,
            cancellationToken);
        for (int i = 0; i < resultData.Length; i++)
        {
            if ((i & 4095) == 0)
                cancellationToken.ThrowIfCancellationRequested();
            resultData[i] /= axisSize;
        }

        var result = new Tensor(resultData, resultShape);

        if (x.RequiresGrad)
        {
            result.RequiresGrad = true;
            result.Children.Add(x);
            result.BackwardFunc = () =>
            {
                // Градиент среднего = 1/n * gradOutput, распределённый по всей оси
                var gradData = new float[x.Shape.TotalElements];
                BroadcastGradAlongAxis(
                    result.Grad!.Data, result.Shape,
                    gradData, x.Shape,
                    normalizedAxis, 1.0f / axisSize);
                AccumulateGrad(x, new Tensor(gradData, x.Shape));
            };
        }

        return result;
    }

    // Матричное умножение: A[...,m,k] × B[...,k,n] → [...,m,n]
    // Gradient: dL/dA = grad @ B^T, dL/dB = A^T @ grad
    public static Tensor MatMul(Tensor a, Tensor b)
        => MatMul(a, b, CancellationToken.None);

    public static Tensor MatMul(Tensor a, Tensor b, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (a.Shape.Rank < 2 || b.Shape.Rank < 2)
            throw new ArgumentException("MatMul требует тензоры не менее 2 измерений");

        int aRows = a.Shape.Dimensions[^2];
        int aCols = a.Shape.Dimensions[^1];
        int bRows = b.Shape.Dimensions[^2];
        int bCols = b.Shape.Dimensions[^1];

        if (aCols != bRows)
            throw new ArgumentException(
                $"Несовместимые размеры: {a.Shape} × {b.Shape}. " +
                $"Нужно aCols({aCols}) == bRows({bRows})");

        var resultDims  = a.Shape.Dimensions[..^2].Concat(new[] { aRows, bCols }).ToArray();
        var resultShape = new TensorShape(resultDims);
        var resultData  = new float[resultShape.TotalElements];

        int batchSize  = a.Shape.TotalElements / (aRows * aCols);
        int bBatchSize = b.Shape.TotalElements / (bRows * bCols);

        for (int batch = 0; batch < batchSize; batch++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int aOff = batch * aRows * aCols;
            int bOff = (batch % bBatchSize) * bRows * bCols;  // broadcast b when it has fewer batch dims
            int rOff = batch * aRows * bCols;

            var transposedB = SimdOps.Transpose2D(
                b.Data,
                bOff,
                bRows,
                bCols,
                cancellationToken);
            SimdOps.MatVecRowUnpublished(
                a.Data,
                aOff,
                aRows,
                aCols,
                transposedB,
                0,
                bCols,
                resultData,
                rOff,
                cancellationToken);
        }

        var result = new Tensor(resultData, resultShape);

        if (a.RequiresGrad || b.RequiresGrad)
        {
            result.RequiresGrad = true;
            if (a.RequiresGrad) result.Children.Add(a);
            if (b.RequiresGrad) result.Children.Add(b);
            result.BackwardFunc = () =>
            {
                if (a.RequiresGrad)
                {
                    var gradA = new float[a.Shape.TotalElements];
                    for (int batch = 0; batch < batchSize; batch++)
                    {
                        int bBatch = batch % bBatchSize;
                        int aOffset = batch * aRows * aCols;
                        int bOffset = bBatch * bRows * bCols;
                        int resultOffset = batch * aRows * bCols;
                        // grad @ B^T: B already has the row layout expected by
                        // the existing SIMD helper, so no transpose is needed.
                        // Backward keeps its existing token-free contract.
                        SimdOps.MatVecRowUnpublished(
                            result.Grad!.Data,
                            resultOffset,
                            aRows,
                            bCols,
                            b.Data,
                            bOffset,
                            aCols,
                            gradA,
                            aOffset,
                            CancellationToken.None);
                    }

                    AccumulateGrad(a, new Tensor(gradA, a.Shape));
                }

                if (b.RequiresGrad)
                {
                    var gradB = new float[b.Shape.TotalElements];
                    for (int batch = 0; batch < batchSize; batch++)
                    {
                        int bBatch = batch % bBatchSize;
                        int aOffset = batch * aRows * aCols;
                        int bOffset = bBatch * bRows * bCols;
                        int resultOffset = batch * aRows * bCols;
                        for (int inner = 0; inner < aCols; inner++)
                        for (int column = 0; column < bCols; column++)
                        for (int row = 0; row < aRows; row++)
                        {
                            gradB[bOffset + inner * bCols + column] +=
                                a.Data[aOffset + row * aCols + inner]
                                * result.Grad!.Data[resultOffset + row * bCols + column];
                        }
                    }

                    AccumulateGrad(b, new Tensor(gradB, b.Shape));
                }
            };
        }

        return result;
    }

    // Перестановка двух осей: Transpose(x, -2, -1) транспонирует матрицу
    public static Tensor Transpose(Tensor x, int dim1, int dim2)
        => Transpose(x, dim1, dim2, CancellationToken.None);

    public static Tensor Transpose(
        Tensor x,
        int dim1,
        int dim2,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        int rank = x.Shape.Rank;
        int d1   = dim1 < 0 ? rank + dim1 : dim1;
        int d2   = dim2 < 0 ? rank + dim2 : dim2;

        var newDims = x.Shape.Dimensions.ToArray();
        (newDims[d1], newDims[d2]) = (newDims[d2], newDims[d1]);

        var newShape = new TensorShape(newDims);
        var newData  = new float[x.Shape.TotalElements];

        if (d1 == d2)
        {
            for (var index = 0; index < x.Data.Length; index++)
            {
                if (index > 0 && (index & 4095) == 0)
                {
                    TensorKernelProgress.Report("TensorOps.Transpose", index);
                    cancellationToken.ThrowIfCancellationRequested();
                }
                newData[index] = x.Data[index];
            }
        }
        else if (d1 == rank - 2 && d2 == rank - 1)
        {
            var rows = x.Shape.Dimensions[d1];
            var cols = x.Shape.Dimensions[d2];
            var matrixSize = rows * cols;
            var batchCount = x.Shape.TotalElements / matrixSize;
            for (var batch = 0; batch < batchCount; batch++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                SimdOps.Transpose2DInPlaceUnpublished(
                    x.Data,
                    batch * matrixSize,
                    rows,
                    cols,
                    newData,
                    batch * matrixSize,
                    cancellationToken);
            }
        }
        else
        {
            for (int flat = 0; flat < x.Shape.TotalElements; flat++)
            {
                if (flat > 0 && (flat & 4095) == 0)
                {
                    TensorKernelProgress.Report("TensorOps.Transpose", flat);
                    cancellationToken.ThrowIfCancellationRequested();
                }
                var indices = FlatToIndices(flat, x.Shape);
                (indices[d1], indices[d2]) = (indices[d2], indices[d1]);
                newData[newShape.FlatIndex(indices)] = x.Data[flat];
            }
        }

        var result = new Tensor(newData, newShape);

        if (x.RequiresGrad)
        {
            result.RequiresGrad = true;
            result.Children.Add(x);
            result.BackwardFunc = () => AccumulateGrad(x, Transpose(result.Grad!, dim1, dim2));
        }

        return result;
    }

    // Softmax по указанной оси с трюком max-subtraction для численной стабильности
    public static Tensor Softmax(Tensor x, int dim)
        => Softmax(x, dim, CancellationToken.None);

    public static Tensor Softmax(
        Tensor x,
        int dim,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        int normalizedDim = dim < 0 ? x.Shape.Rank + dim : dim;
        var resultData = new float[x.Data.Length];
        for (var index = 0; index < resultData.Length; index++)
        {
            if (index > 0 && (index & 4095) == 0)
                cancellationToken.ThrowIfCancellationRequested();
            resultData[index] = x.Data[index];
        }

        int outerSize = 1;
        for (int i = 0; i < normalizedDim; i++)
            outerSize *= x.Shape.Dimensions[i];

        int dimSize   = x.Shape.Dimensions[normalizedDim];
        int innerSize = x.Shape.TotalElements / (outerSize * dimSize);

        for (int outer = 0; outer < outerSize; outer++)
        for (int inner = 0; inner < innerSize; inner++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            float maxVal = float.NegativeInfinity;
            for (int d = 0; d < dimSize; d++)
            {
                if (d > 0 && (d & 4095) == 0)
                {
                    TensorKernelProgress.Report("TensorOps.Softmax", d);
                    cancellationToken.ThrowIfCancellationRequested();
                }
                int idx = (outer * dimSize + d) * innerSize + inner;
                if (resultData[idx] > maxVal) maxVal = resultData[idx];
            }

            float expSum = 0f;
            for (int d = 0; d < dimSize; d++)
            {
                if (d > 0 && (d & 4095) == 0)
                {
                    TensorKernelProgress.Report("TensorOps.Softmax", d);
                    cancellationToken.ThrowIfCancellationRequested();
                }
                int idx = (outer * dimSize + d) * innerSize + inner;
                resultData[idx] = MathF.Exp(resultData[idx] - maxVal);
                expSum += resultData[idx];
            }

            for (int d = 0; d < dimSize; d++)
            {
                if (d > 0 && (d & 4095) == 0)
                {
                    TensorKernelProgress.Report("TensorOps.Softmax", d);
                    cancellationToken.ThrowIfCancellationRequested();
                }
                int idx = (outer * dimSize + d) * innerSize + inner;
                resultData[idx] /= expSum;
            }
        }

        var result = new Tensor(resultData, x.Shape);

        if (x.RequiresGrad)
        {
            result.RequiresGrad = true;
            result.Children.Add(x);
            result.BackwardFunc = () =>
            {
                // Jacobian-vector product: grad_x[i] = s[i] * (grad[i] - dot(grad, s))
                var gradData = new float[x.Shape.TotalElements];

                for (int outer = 0; outer < outerSize; outer++)
                for (int inner = 0; inner < innerSize; inner++)
                {
                    float dot = 0f;
                    for (int d = 0; d < dimSize; d++)
                    {
                        int idx = (outer * dimSize + d) * innerSize + inner;
                        dot += result.Grad!.Data[idx] * result.Data[idx];
                    }
                    for (int d = 0; d < dimSize; d++)
                    {
                        int idx = (outer * dimSize + d) * innerSize + inner;
                        gradData[idx] = result.Data[idx] * (result.Grad!.Data[idx] - dot);
                    }
                }

                AccumulateGrad(x, new Tensor(gradData, x.Shape));
            };
        }

        return result;
    }

    // Reshape — изменить форму не меняя данных (TotalElements должен совпадать)
    public static Tensor Reshape(Tensor x, params int[] newDims)
        => Reshape(x, CancellationToken.None, newDims);

    public static Tensor Reshape(
        Tensor x,
        CancellationToken cancellationToken,
        params int[] newDims)
    {
        var newShape = new TensorShape(newDims);
        if (newShape.TotalElements != x.Shape.TotalElements)
            throw new ArgumentException(
                $"Reshape: {x.Shape} → {newShape} — разное число элементов");

        var data = new float[x.Data.Length];
        for (var index = 0; index < data.Length; index++)
        {
            if ((index & 4095) == 0)
                cancellationToken.ThrowIfCancellationRequested();
            data[index] = x.Data[index];
        }
        var result = new Tensor(data, newShape);

        if (x.RequiresGrad)
        {
            result.RequiresGrad = true;
            result.Children.Add(x);
            result.BackwardFunc = () => AccumulateGrad(x, Reshape(result.Grad!, x.Shape.Dimensions));
        }

        return result;
    }

    // Argmax — индекс максимального элемента (без градиента, только для inference)
    public static int Argmax(Tensor x)
    {
        int maxIdx = 0;
        float maxVal = x.Data[0];
        for (int i = 1; i < x.Data.Length; i++)
        {
            if (x.Data[i] > maxVal) { maxVal = x.Data[i]; maxIdx = i; }
        }
        return maxIdx;
    }

    // Нарезать тензор по первому измерению: x[startRow..endRow, :]
    public static Tensor Slice(Tensor x, int startRow, int endRow)
    {
        if (x.Shape.Rank < 2)
            throw new ArgumentException("Slice требует ранг >= 2");

        int rowLen   = x.Shape.TotalElements / x.Shape.Dimensions[0];
        int numRows  = endRow - startRow;
        var newDims  = x.Shape.Dimensions.ToArray();
        newDims[0]   = numRows;

        var data = new float[numRows * rowLen];
        Array.Copy(x.Data, startRow * rowLen, data, 0, numRows * rowLen);

        return new Tensor(data, new TensorShape(newDims));
    }

    // Конкатенация двух тензоров по указанной оси
    public static Tensor Concat(Tensor a, Tensor b, int axis)
    {
        int rank = a.Shape.Rank;
        int normAxis = axis < 0 ? rank + axis : axis;

        var newDims = a.Shape.Dimensions.ToArray();
        newDims[normAxis] += b.Shape.Dimensions[normAxis];

        var newShape = new TensorShape(newDims);
        var data     = new float[newShape.TotalElements];

        // Размер "блока" до конкатенируемой оси
        int outerSize  = 1;
        for (int i = 0; i < normAxis; i++) outerSize *= a.Shape.Dimensions[i];

        int aAxisSize  = a.Shape.Dimensions[normAxis];
        int bAxisSize  = b.Shape.Dimensions[normAxis];
        int innerSize  = a.Shape.TotalElements / (outerSize * aAxisSize);

        for (int outer = 0; outer < outerSize; outer++)
        {
            // Копируем блок из a
            int aSrc = outer * aAxisSize * innerSize;
            int dst  = outer * (aAxisSize + bAxisSize) * innerSize;
            Array.Copy(a.Data, aSrc, data, dst, aAxisSize * innerSize);

            // Копируем блок из b
            int bSrc = outer * bAxisSize * innerSize;
            Array.Copy(b.Data, bSrc, data, dst + aAxisSize * innerSize, bAxisSize * innerSize);
        }

        return new Tensor(data, newShape);
    }

    // Добавить к накопленному градиенту (grad += incoming)
    internal static void AccumulateGrad(Tensor target, Tensor gradToAdd)
    {
        if (!target.RequiresGrad) return;

        if (target.Grad == null)
        {
            target.Grad = new Tensor(gradToAdd.Data.ToArray(), gradToAdd.Shape);
        }
        else
        {
            for (int i = 0; i < target.Grad.Data.Length; i++)
                target.Grad.Data[i] += gradToAdd.Data[i];
        }
    }

    // --- Вспомогательные методы ---

    private static void AssertSameShape(Tensor a, Tensor b, string op)
    {
        if (!a.Shape.Dimensions.SequenceEqual(b.Shape.Dimensions))
            throw new ArgumentException(
                $"{op}: несовместимые формы {a.Shape} и {b.Shape}");
    }

    internal static int[] FlatToIndices(int flatIndex, TensorShape shape)
    {
        var indices   = new int[shape.Rank];
        int remaining = flatIndex;
        for (int i = shape.Rank - 1; i >= 0; i--)
        {
            indices[i] = remaining % shape.Dimensions[i];
            remaining  /= shape.Dimensions[i];
        }
        return indices;
    }

    private static void SumAlongAxis(
        float[] src, TensorShape srcShape,
        float[] dst, TensorShape dstShape,
        int axis,
        CancellationToken cancellationToken)
    {
        for (int dstFlat = 0; dstFlat < dstShape.TotalElements; dstFlat++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var dstIdx = FlatToIndices(dstFlat, dstShape);
            float sum  = 0f;
            int axisSize = srcShape.Dimensions[axis];

            for (int k = 0; k < axisSize; k++)
            {
                if ((k & 4095) == 0)
                    cancellationToken.ThrowIfCancellationRequested();
                var srcIdx = dstIdx.ToArray();
                if (srcShape.Rank == dstShape.Rank)
                    srcIdx[axis] = k;
                else
                    srcIdx = InsertAt(srcIdx, axis, k);

                sum += src[srcShape.FlatIndex(srcIdx)];
            }
            dst[dstFlat] = sum;
        }
    }

    private static void BroadcastGradAlongAxis(
        float[] gradOut, TensorShape gradOutShape,
        float[] gradIn,  TensorShape gradInShape,
        int axis, float scale)
    {
        for (int srcFlat = 0; srcFlat < gradInShape.TotalElements; srcFlat++)
        {
            var srcIdx = FlatToIndices(srcFlat, gradInShape);
            var dstIdx = srcIdx.ToArray();
            dstIdx[axis] = 0;
            gradIn[srcFlat] = gradOut[gradOutShape.FlatIndex(dstIdx)] * scale;
        }
    }

    // Добавить измерение размера 1 в позицию dim
    public static Tensor Unsqueeze(Tensor x, int dim)
    {
        int rank = x.Shape.Rank;
        int normDim = dim < 0 ? rank + 1 + dim : dim;

        var newDims = new int[rank + 1];
        for (int i = 0; i < normDim; i++) newDims[i] = x.Shape.Dimensions[i];
        newDims[normDim] = 1;
        for (int i = normDim; i < rank; i++) newDims[i + 1] = x.Shape.Dimensions[i];

        return new Tensor(x.Data.ToArray(), new TensorShape(newDims));
    }

    // Создать тензор [start, start+step, ...] с float значениями
    public static Tensor Arange(float start, float stop, float step = 1f)
    {
        int count = (int)Math.Ceiling((stop - start) / step);
        var data  = new float[count];
        for (int i = 0; i < count; i++)
            data[i] = start + i * step;
        return new Tensor(data, new TensorShape(count));
    }

    // Cosine поэлементно
    public static Tensor Cos(Tensor x)
        => Cos(x, CancellationToken.None);

    public static Tensor Cos(Tensor x, CancellationToken cancellationToken)
    {
        var data = new float[x.Shape.TotalElements];
        for (int i = 0; i < data.Length; i++)
        {
            if ((i & 4095) == 0)
                cancellationToken.ThrowIfCancellationRequested();
            data[i] = MathF.Cos(x.Data[i]);
        }
        return new Tensor(data, x.Shape);
    }

    // Sine поэлементно
    public static Tensor Sin(Tensor x)
        => Sin(x, CancellationToken.None);

    public static Tensor Sin(Tensor x, CancellationToken cancellationToken)
    {
        var data = new float[x.Shape.TotalElements];
        for (int i = 0; i < data.Length; i++)
        {
            if ((i & 4095) == 0)
                cancellationToken.ThrowIfCancellationRequested();
            data[i] = MathF.Sin(x.Data[i]);
        }
        return new Tensor(data, x.Shape);
    }

    // Sigmoid: 1/(1+exp(-x))
    public static Tensor Sigmoid(Tensor x)
    {
        var data = new float[x.Shape.TotalElements];
        for (int i = 0; i < data.Length; i++)
            data[i] = 1f / (1f + MathF.Exp(-x.Data[i]));
        return new Tensor(data, x.Shape);
    }

    // SiLU: x * sigmoid(x)  — используется в SwiGLU
    public static Tensor Silu(Tensor x)
        => Silu(x, CancellationToken.None);

    public static Tensor Silu(Tensor x, CancellationToken cancellationToken)
    {
        var data = new float[x.Shape.TotalElements];
        for (int i = 0; i < data.Length; i++)
        {
            if ((i & 4095) == 0)
                cancellationToken.ThrowIfCancellationRequested();
            data[i] = x.Data[i] / (1f + MathF.Exp(-x.Data[i]));
        }

        var result = new Tensor(data, x.Shape);
        if (x.RequiresGrad)
        {
            result.RequiresGrad = true;
            result.Children.Add(x);
            result.BackwardFunc = () =>
            {
                var grad = new float[x.Shape.TotalElements];
                for (int i = 0; i < grad.Length; i++)
                {
                    float sigmoid = 1f / (1f + MathF.Exp(-x.Data[i]));
                    grad[i] = result.Grad!.Data[i]
                              * sigmoid
                              * (1f + x.Data[i] * (1f - sigmoid));
                }

                AccumulateGrad(x, new Tensor(grad, x.Shape));
            };
        }

        return result;
    }

    // LogSigmoid: log(sigmoid(x)) = -log(1 + exp(-x))
    public static Tensor LogSigmoid(Tensor x)
    {
        var data = new float[x.Shape.TotalElements];
        for (int i = 0; i < data.Length; i++)
            data[i] = -MathF.Log(1f + MathF.Exp(-x.Data[i]));
        return new Tensor(data, x.Shape);
    }

    // Clamp: ограничить значения снизу
    public static Tensor ClampMin(Tensor x, float minVal)
    {
        var data = new float[x.Shape.TotalElements];
        for (int i = 0; i < data.Length; i++)
            data[i] = Math.Max(x.Data[i], minVal);
        return new Tensor(data, x.Shape);
    }

    // Внешнее произведение двух векторов: [m] ⊗ [n] → [m, n]
    public static Tensor OuterProduct(Tensor a, Tensor b)
        => OuterProduct(a, b, CancellationToken.None);

    public static Tensor OuterProduct(
        Tensor a,
        Tensor b,
        CancellationToken cancellationToken)
    {
        if (a.Shape.Rank != 1 || b.Shape.Rank != 1)
            throw new ArgumentException("OuterProduct требует два вектора (ранг 1)");

        int m    = a.Shape.Dimensions[0];
        int n    = b.Shape.Dimensions[0];
        var data = new float[m * n];

        for (int i = 0; i < m; i++)
        for (int j = 0; j < n; j++)
        {
            if ((j & 4095) == 0)
                cancellationToken.ThrowIfCancellationRequested();
            data[i * n + j] = a.Data[i] * b.Data[j];
        }

        return new Tensor(data, new TensorShape(m, n));
    }

    // Повторить элементы вдоль оси (repeat_interleave).
    // RepeatInterleave([a, b, c], 2, 0) → [a, a, b, b, c, c]
    public static Tensor RepeatInterleave(Tensor x, int repeats, int dim)
        => RepeatInterleave(x, repeats, dim, CancellationToken.None);

    public static Tensor RepeatInterleave(
        Tensor x,
        int repeats,
        int dim,
        CancellationToken cancellationToken)
    {
        int rank = x.Shape.Rank;
        int normDim = dim < 0 ? rank + dim : dim;
        if (repeats <= 0)
            throw new ArgumentOutOfRangeException(nameof(repeats), "RepeatInterleave repeats must be positive.");
        if (normDim < 0 || normDim >= rank)
            throw new ArgumentOutOfRangeException(nameof(dim), "RepeatInterleave dimension is out of range.");

        var newDims = x.Shape.Dimensions.ToArray();
        newDims[normDim] = checked(newDims[normDim] * repeats);
        var newShape = new TensorShape(newDims);
        var data     = new float[newShape.TotalElements];

        int outerSize = 1;
        for (int i = 0; i < normDim; i++) outerSize *= x.Shape.Dimensions[i];

        int dimSize   = x.Shape.Dimensions[normDim];
        int innerSize = x.Shape.TotalElements / (outerSize * dimSize);

        for (int outer = 0; outer < outerSize; outer++)
        for (int d = 0; d < dimSize; d++)
        for (int rep = 0; rep < repeats; rep++)
        for (int inner = 0; inner < innerSize; inner++)
        {
            if ((inner & 4095) == 0)
                cancellationToken.ThrowIfCancellationRequested();
            int srcIdx = (outer * dimSize + d) * innerSize + inner;
            int dstIdx = (outer * dimSize * repeats + d * repeats + rep) * innerSize + inner;
            data[dstIdx] = x.Data[srcIdx];
        }

        var result = new Tensor(data, newShape);
        if (x.RequiresGrad)
        {
            result.RequiresGrad = true;
            result.Children.Add(x);
            result.BackwardFunc = () =>
            {
                var grad = new float[x.Shape.TotalElements];
                for (int outer = 0; outer < outerSize; outer++)
                for (int d = 0; d < dimSize; d++)
                for (int rep = 0; rep < repeats; rep++)
                for (int inner = 0; inner < innerSize; inner++)
                {
                    int srcIdx = (outer * dimSize + d) * innerSize + inner;
                    int dstIdx = (outer * dimSize * repeats + d * repeats + rep) * innerSize + inner;
                    grad[srcIdx] += result.Grad!.Data[dstIdx];
                }

                AccumulateGrad(x, new Tensor(grad, x.Shape));
            };
        }

        return result;
    }

    // Apply split-half rotary embedding while treating cos and sin as constants.
    public static Tensor ApplyRotaryEmbedding(Tensor x, Tensor cos, Tensor sin)
        => ApplyRotaryEmbedding(x, cos, sin, CancellationToken.None);

    public static Tensor ApplyRotaryEmbedding(
        Tensor x,
        Tensor cos,
        Tensor sin,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (x.Shape.Rank != 4)
            throw new ArgumentException("ApplyRotaryEmbedding requires X shape [batch, heads, sequence, dimension].");

        int batch = x.Shape.Dimensions[0];
        int heads = x.Shape.Dimensions[1];
        int sequence = x.Shape.Dimensions[2];
        int dimension = x.Shape.Dimensions[3];
        int half = dimension / 2;
        if (cos.Shape.Rank != 2
            || sin.Shape.Rank != 2
            || cos.Shape.Dimensions[0] != sequence
            || sin.Shape.Dimensions[0] != sequence
            || cos.Shape.Dimensions[1] != half
            || sin.Shape.Dimensions[1] != half)
        {
            throw new ArgumentException("ApplyRotaryEmbedding frequency shapes must be [sequence, dimension/2].");
        }

        var data = new float[x.Shape.TotalElements];
        var processedPairs = 0;
        for (int b = 0; b < batch; b++)
        for (int h = 0; h < heads; h++)
        for (int t = 0; t < sequence; t++)
        {
            int baseOffset = ((b * heads + h) * sequence + t) * dimension;
            int frequencyOffset = t * half;
            for (int i = 0; i < half; i++)
            {
                float first = x.Data[baseOffset + i];
                float second = x.Data[baseOffset + half + i];
                float cosine = cos.Data[frequencyOffset + i];
                float sine = sin.Data[frequencyOffset + i];
                data[baseOffset + i] = first * cosine - second * sine;
                data[baseOffset + half + i] = first * sine + second * cosine;
                processedPairs++;
                if ((processedPairs & 4095) == 0)
                {
                    TensorKernelProgress.Report(
                        "TensorOps.ApplyRotaryEmbedding",
                        processedPairs);
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
        }

        var result = new Tensor(data, x.Shape);
        if (x.RequiresGrad)
        {
            result.RequiresGrad = true;
            result.Children.Add(x);
            result.BackwardFunc = () =>
            {
                var grad = new float[x.Shape.TotalElements];
                for (int b = 0; b < batch; b++)
                for (int h = 0; h < heads; h++)
                for (int t = 0; t < sequence; t++)
                {
                    int baseOffset = ((b * heads + h) * sequence + t) * dimension;
                    int frequencyOffset = t * half;
                    for (int i = 0; i < half; i++)
                    {
                        float firstGradient = result.Grad!.Data[baseOffset + i];
                        float secondGradient = result.Grad.Data[baseOffset + half + i];
                        float cosine = cos.Data[frequencyOffset + i];
                        float sine = sin.Data[frequencyOffset + i];
                        grad[baseOffset + i] =
                            firstGradient * cosine + secondGradient * sine;
                        grad[baseOffset + half + i] =
                            -firstGradient * sine + secondGradient * cosine;
                    }
                }

                AccumulateGrad(x, new Tensor(grad, x.Shape));
            };
        }

        return result;
    }

    // Apply a lower-triangular causal mask to the final two equal dimensions.
    public static Tensor ApplyCausalMask(Tensor scores, float fillValue)
        => ApplyCausalMaskWithPast(scores, fillValue, pastKeyLength: 0, CancellationToken.None);

    public static Tensor ApplyCausalMask(
        Tensor scores,
        float fillValue,
        CancellationToken cancellationToken)
        => ApplyCausalMaskWithPast(scores, fillValue, pastKeyLength: 0, cancellationToken);

    // Apply a causal mask when each query can attend to a fixed cached prefix.
    public static Tensor ApplyCausalMaskWithPast(
        Tensor scores,
        float fillValue,
        int pastKeyLength)
        => ApplyCausalMaskWithPast(scores, fillValue, pastKeyLength, CancellationToken.None);

    // For [..., queryLength, keyLength], query row r may attend through pastKeyLength + r.
    public static Tensor ApplyCausalMaskWithPast(
        Tensor scores,
        float fillValue,
        int pastKeyLength,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (pastKeyLength < 0)
            throw new ArgumentOutOfRangeException(nameof(pastKeyLength));
        if (scores.Shape.Rank < 2)
            throw new ArgumentException("ApplyCausalMask requires rank at least two.");

        int keyLength = scores.Shape.Dimensions[^1];
        int queryLength = scores.Shape.Dimensions[^2];
        int expectedKeyLength = checked(pastKeyLength + queryLength);
        if (keyLength != expectedKeyLength)
            throw new ArgumentException("ApplyCausalMask requires keyLength == pastKeyLength + queryLength.");

        int matrixSize = checked(queryLength * keyLength);
        int matrixCount = scores.Shape.TotalElements / matrixSize;
        var data = new float[scores.Data.Length];
        for (var index = 0; index < data.Length; index++)
        {
            if (index > 0 && (index & 4095) == 0)
                cancellationToken.ThrowIfCancellationRequested();
            data[index] = scores.Data[index];
        }
        var maskedValues = 0;
        for (int matrix = 0; matrix < matrixCount; matrix++)
        for (int row = 0; row < queryLength; row++)
        {
            int allowedLastColumn = checked(pastKeyLength + row);
            for (int column = allowedLastColumn + 1; column < keyLength; column++)
            {
                int index = checked(matrix * matrixSize + row * keyLength + column);
                data[index] = fillValue;
                maskedValues++;
                if ((maskedValues & 4095) == 0)
                {
                    TensorKernelProgress.Report("TensorOps.ApplyCausalMask", maskedValues);
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
        }

        var result = new Tensor(data, scores.Shape);
        if (scores.RequiresGrad)
        {
            result.RequiresGrad = true;
            result.Children.Add(scores);
            result.BackwardFunc = () =>
            {
                var grad = new float[scores.Shape.TotalElements];
                for (int matrix = 0; matrix < matrixCount; matrix++)
                for (int row = 0; row < queryLength; row++)
                {
                    int allowedLastColumn = checked(pastKeyLength + row);
                    for (int column = 0; column <= allowedLastColumn; column++)
                    {
                        int index = checked(matrix * matrixSize + row * keyLength + column);
                        grad[index] = result.Grad!.Data[index];
                    }
                }

                AccumulateGrad(scores, new Tensor(grad, scores.Shape));
            };
        }

        return result;
    }

    // Нижнетреугольная маска [n, n]: 1 ниже главной диагонали включительно, 0 выше
    public static Tensor Tril(int n)
    {
        var data = new float[n * n];
        for (int i = 0; i < n; i++)
        for (int j = 0; j <= i; j++)
            data[i * n + j] = 1f;
        return new Tensor(data, new TensorShape(n, n));
    }

    // Заменить значения где mask != 0 на fillValue
    public static Tensor MaskedFill(Tensor x, Tensor mask, float fillValue)
    {
        var data = x.Data.ToArray();
        for (int i = 0; i < data.Length; i++)
            if (mask.Data[i] != 0f)
                data[i] = fillValue;
        return new Tensor(data, x.Shape);
    }

    // Gather: для каждого индекса в index берём элемент из src по этой оси
    // src: [B, T, V], index: [B, T, 1], dim=2 → result[b,t,0] = src[b,t,index[b,t,0]]
    public static Tensor Gather(Tensor src, int dim, Tensor index)
    {
        int normDim = dim < 0 ? src.Shape.Rank + dim : dim;
        var data    = new float[index.Shape.TotalElements];

        for (int flat = 0; flat < index.Shape.TotalElements; flat++)
        {
            var idx = FlatToIndices(flat, index.Shape);
            var srcIdx = idx.ToArray();
            srcIdx[normDim] = (int)index.Data[flat];
            data[flat] = src.Data[src.Shape.FlatIndex(srcIdx)];
        }

        return new Tensor(data, index.Shape);
    }

    // CumSum по оси — накопленная сумма
    public static Tensor CumSum(Tensor x, int dim)
    {
        int normDim = dim < 0 ? x.Shape.Rank + dim : dim;
        var data = x.Data.ToArray();

        int outerSize = 1;
        for (int i = 0; i < normDim; i++) outerSize *= x.Shape.Dimensions[i];

        int dimSize   = x.Shape.Dimensions[normDim];
        int innerSize = x.Shape.TotalElements / (outerSize * dimSize);

        for (int outer = 0; outer < outerSize; outer++)
        for (int inner = 0; inner < innerSize; inner++)
        {
            float acc = 0f;
            for (int d = 0; d < dimSize; d++)
            {
                int idx = (outer * dimSize + d) * innerSize + inner;
                acc += data[idx];
                data[idx] = acc;
            }
        }

        return new Tensor(data, x.Shape);
    }

    // Сортировка последней оси по убыванию; возвращает (sortedValues, sortedIndices)
    public static (Tensor values, Tensor indices) SortDescending(Tensor x)
    {
        // Только для одномерных тензоров (как в Sampler)
        if (x.Shape.Rank != 1)
            throw new NotImplementedException("SortDescending реализован только для 1D тензоров");

        int n    = x.Shape.TotalElements;
        var idxs = Enumerable.Range(0, n).OrderByDescending(i => x.Data[i]).ToArray();

        var vals   = idxs.Select(i => x.Data[i]).ToArray();
        var indArr = idxs.Select(i => (float)i).ToArray();

        return (new Tensor(vals, x.Shape), new Tensor(indArr, x.Shape));
    }

    // Scatter: result[indices[i]] = src[i] для 1D (используется в Sampler для TopP)
    public static Tensor Scatter1D(Tensor src, Tensor indices, int size)
    {
        var data = new float[size];
        for (int i = 0; i < indices.Shape.TotalElements; i++)
            data[(int)indices.Data[i]] = src.Data[i];
        return new Tensor(data, new TensorShape(size));
    }

    // Squeeze: убрать все размерности = 1 (или конкретную)
    public static Tensor Squeeze(Tensor x, int? dim = null)
    {
        int[] newDims;
        if (dim.HasValue)
        {
            int d = dim.Value < 0 ? x.Shape.Rank + dim.Value : dim.Value;
            if (x.Shape.Dimensions[d] != 1)
                return new Tensor(x.Data.ToArray(), x.Shape);
            newDims = x.Shape.Dimensions.Where((_, i) => i != d).ToArray();
        }
        else
        {
            newDims = x.Shape.Dimensions.Where(d => d != 1).ToArray();
        }

        if (newDims.Length == 0) newDims = new[] { 1 };
        return new Tensor(x.Data.ToArray(), new TensorShape(newDims));
    }

    // Нормировать по L2 минимум clampMin
    public static Tensor L2Normalize(Tensor x, float eps = 1e-9f)
    {
        // Для вектора или матрицы (нормируем последнюю ось)
        if (x.Shape.Rank == 2)
        {
            int rows = x.Shape.Dimensions[0];
            int cols = x.Shape.Dimensions[1];
            var data = new float[x.Shape.TotalElements];

            for (int r = 0; r < rows; r++)
            {
                float norm = 0f;
                for (int c = 0; c < cols; c++)
                    norm += x.Data[r * cols + c] * x.Data[r * cols + c];
                norm = MathF.Sqrt(norm);
                if (norm < eps) norm = eps;

                for (int c = 0; c < cols; c++)
                    data[r * cols + c] = x.Data[r * cols + c] / norm;
            }

            return new Tensor(data, x.Shape);
        }

        // Для 1D
        float n = 0f;
        foreach (var v in x.Data) n += v * v;
        n = Math.Max(MathF.Sqrt(n), eps);
        var d = x.Data.Select(v => v / n).ToArray();
        return new Tensor(d, x.Shape);
    }

    // CrossEntropy: log_softmax + NLL (для Trainer)
    // logits: [N, C], targets: int[] длины N → скалярный loss
    public static float CrossEntropy(Tensor logits, int[] targets)
    {
        // logits: [N, C]
        int n = logits.Shape.Dimensions[0];
        int c = logits.Shape.Dimensions[1];
        float totalLoss = 0f;

        for (int i = 0; i < n; i++)
        {
            // Вычисляем log_softmax для строки i
            float maxVal = float.NegativeInfinity;
            for (int j = 0; j < c; j++)
                if (logits.Data[i * c + j] > maxVal)
                    maxVal = logits.Data[i * c + j];

            float logSumExp = 0f;
            for (int j = 0; j < c; j++)
                logSumExp += MathF.Exp(logits.Data[i * c + j] - maxVal);
            logSumExp = MathF.Log(logSumExp);

            float logP = logits.Data[i * c + targets[i]] - maxVal - logSumExp;
            totalLoss -= logP; // NLL loss
        }

        return totalLoss / n;
    }

    // ClipGradNorm: масштабировать градиенты если их норма > maxNorm
    public static void ClipGradNorm(IEnumerable<Layers.Parameter> parameters, float maxNorm)
    {
        float totalNormSq = 0f;
        foreach (var p in parameters)
        {
            if (p.Grad == null) continue;
            foreach (var g in p.Grad.Data)
                totalNormSq += g * g;
        }

        float totalNorm = MathF.Sqrt(totalNormSq);
        if (totalNorm <= maxNorm) return;

        float scale = maxNorm / (totalNorm + 1e-6f);
        foreach (var p in parameters)
        {
            if (p.Grad == null) continue;
            for (int i = 0; i < p.Grad.Data.Length; i++)
                p.Grad.Data[i] *= scale;
        }
    }

    private static int[] InsertAt(int[] arr, int pos, int val)
    {
        var r = new int[arr.Length + 1];
        Array.Copy(arr, 0, r, 0, pos);
        r[pos] = val;
        Array.Copy(arr, pos, r, pos + 1, arr.Length - pos);
        return r;
    }
}

// Test observation is execution-context local and invisible from the public API.
// Reports occur only at cancellation checkpoints after real kernel progress.
internal static class TensorKernelProgress
{
    private static readonly AsyncLocal<Action<TensorKernelProgressEvent>?> CurrentObserver = new();

    internal static IDisposable Observe(Action<TensorKernelProgressEvent> observer)
    {
        ArgumentNullException.ThrowIfNull(observer);
        var previous = CurrentObserver.Value;
        CurrentObserver.Value = observer;
        return new ObserverScope(previous);
    }

    internal static void Report(string kernelName, long completedWork)
        => CurrentObserver.Value?.Invoke(new TensorKernelProgressEvent(
            kernelName,
            TensorKernelProgressStage.Progress,
            completedWork));

    private sealed class ObserverScope(Action<TensorKernelProgressEvent>? previous) : IDisposable
    {
        private Action<TensorKernelProgressEvent>? _previous = previous;
        private int _disposed;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;
            CurrentObserver.Value = Interlocked.Exchange(ref _previous, null);
        }
    }
}

internal enum TensorKernelProgressStage
{
    Progress,
}

internal readonly record struct TensorKernelProgressEvent(
    string KernelName,
    TensorKernelProgressStage Stage,
    long CompletedWork);
