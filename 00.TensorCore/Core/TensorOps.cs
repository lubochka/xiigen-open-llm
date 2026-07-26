// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TensorCore.Core;

// All mathematical operations on tensors.
// Each operation: (1) computes the result, (2) registers backward when needed.
public static class TensorOps
{
    // Zero tensor with the same shape as the sample
    public static Tensor ZerosLike(Tensor t) =>
        new(new float[t.Shape.TotalElements], t.Shape);

    // Ones tensor with the same shape as the sample
    public static Tensor OnesLike(Tensor t)
    {
        var data = new float[t.Shape.TotalElements];
        Array.Fill(data, 1.0f);
        return new Tensor(data, t.Shape);
    }

    // Create a zero tensor of the given shape
    public static Tensor Zeros(params int[] dims) =>
        new(new TensorShape(dims));

    // Create a ones tensor of the given shape
    public static Tensor Ones(params int[] dims)
    {
        var shape = new TensorShape(dims);
        var data  = new float[shape.TotalElements];
        Array.Fill(data, 1.0f);
        return new Tensor(data, shape);
    }

    // Element-wise addition: grad passes to both inputs unchanged (d(a+b)/da = 1)
    public static Tensor Add(Tensor a, Tensor b)
    {
        AssertSameShape(a, b, "Add");

        var data = new float[a.Shape.TotalElements];
        for (int i = 0; i < data.Length; i++)
            data[i] = a.Data[i] + b.Data[i];

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

    // Add a scalar to every element (broadcast)
    public static Tensor AddScalar(Tensor a, float scalar)
    {
        var data = new float[a.Shape.TotalElements];
        for (int i = 0; i < data.Length; i++)
            data[i] = a.Data[i] + scalar;

        var result = new Tensor(data, a.Shape);

        if (a.RequiresGrad)
        {
            result.RequiresGrad = true;
            result.Children.Add(a);
            result.BackwardFunc = () => AccumulateGrad(a, result.Grad!);
        }

        return result;
    }

    // Element-wise multiplication: grad_a = grad * b, grad_b = grad * a
    public static Tensor Multiply(Tensor a, Tensor b)
    {
        AssertSameShape(a, b, "Multiply");

        var data = new float[a.Shape.TotalElements];
        for (int i = 0; i < data.Length; i++)
            data[i] = a.Data[i] * b.Data[i];

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

    // Multiply every element by a scalar
    public static Tensor Scale(Tensor x, float scalar)
    {
        var data = new float[x.Shape.TotalElements];
        for (int i = 0; i < data.Length; i++)
            data[i] = x.Data[i] * scalar;

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

    // Element-wise subtraction: grad_a = grad, grad_b = -grad
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

    // Raise each element to a power: d(x^n)/dx = n * x^(n-1)
    public static Tensor Pow(Tensor x, float exponent)
    {
        var data = new float[x.Shape.TotalElements];
        for (int i = 0; i < data.Length; i++)
            data[i] = MathF.Pow(x.Data[i], exponent);

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

    // Element-wise natural logarithm
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

    // Element-wise exponential
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

    // Element-wise square root
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

    // Element-wise reciprocal square root: rsqrt(x) = 1/sqrt(x), used in RmsNorm
    public static Tensor RSqrt(Tensor x)
    {
        var data = new float[x.Shape.TotalElements];
        for (int i = 0; i < data.Length; i++)
            data[i] = 1.0f / MathF.Sqrt(x.Data[i]);

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

    // Normalized distribution (LogSoftmax) — log(softmax(x))
    // More numerically stable than log(softmax(x)) computed separately
    public static Tensor LogSoftmax(Tensor x, int dim)
    {
        int normalizedDim = dim < 0 ? x.Shape.Rank + dim : dim;
        var resultData    = x.Data.ToArray();

        int outerSize = 1;
        for (int i = 0; i < normalizedDim; i++)
            outerSize *= x.Shape.Dimensions[i];

        int dimSize   = x.Shape.Dimensions[normalizedDim];
        int innerSize = x.Shape.TotalElements / (outerSize * dimSize);

        for (int outer = 0; outer < outerSize; outer++)
        for (int inner = 0; inner < innerSize; inner++)
        {
            // Subtract the maximum for numerical stability
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

    // Sum over all elements → scalar (tensor [1])
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
                // Gradient of the sum = 1 for each input element
                var g = new float[x.Shape.TotalElements];
                Array.Fill(g, result.Grad!.Data[0]);
                AccumulateGrad(x, new Tensor(g, x.Shape));
            };
        }

        return result;
    }

    // Mean over the specified dimension.
    // keepDims=true: the result shape matches, but the axis = 1
    public static Tensor Mean(Tensor x, int axis, bool keepDims = true)
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

        SumAlongAxis(x.Data, x.Shape, resultData, resultShape, normalizedAxis);
        for (int i = 0; i < resultData.Length; i++)
            resultData[i] /= axisSize;

        var result = new Tensor(resultData, resultShape);

        if (x.RequiresGrad)
        {
            result.RequiresGrad = true;
            result.Children.Add(x);
            result.BackwardFunc = () =>
            {
                // Gradient of the mean = 1/n * gradOutput, spread across the whole axis
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

    // Matrix multiplication: A[...,m,k] × B[...,k,n] → [...,m,n]
    // Gradient: dL/dA = grad @ B^T, dL/dB = A^T @ grad
    public static Tensor MatMul(Tensor a, Tensor b)
    {
        if (a.Shape.Rank < 2 || b.Shape.Rank < 2)
            throw new ArgumentException("MatMul requires tensors of at least 2 dimensions");

        int aRows = a.Shape.Dimensions[^2];
        int aCols = a.Shape.Dimensions[^1];
        int bRows = b.Shape.Dimensions[^2];
        int bCols = b.Shape.Dimensions[^1];

        if (aCols != bRows)
            throw new ArgumentException(
                $"Incompatible sizes: {a.Shape} × {b.Shape}. " +
                $"Need aCols({aCols}) == bRows({bRows})");

        var resultDims  = a.Shape.Dimensions[..^2].Concat(new[] { aRows, bCols }).ToArray();
        var resultShape = new TensorShape(resultDims);
        var resultData  = new float[resultShape.TotalElements];

        int batchSize  = a.Shape.TotalElements / (aRows * aCols);
        int bBatchSize = b.Shape.TotalElements / (bRows * bCols);

        for (int batch = 0; batch < batchSize; batch++)
        {
            int aOff = batch * aRows * aCols;
            int bOff = (batch % bBatchSize) * bRows * bCols;  // broadcast b when it has fewer batch dims
            int rOff = batch * aRows * bCols;

            var transposedB = SimdOps.Transpose2D(b.Data, bOff, bRows, bCols);
            SimdOps.MatVecRow(a.Data, aOff, aRows, aCols, transposedB, 0, bCols, resultData, rOff);
        }

        var result = new Tensor(resultData, resultShape);

        if (a.RequiresGrad || b.RequiresGrad)
        {
            result.RequiresGrad = true;
            result.Children.Add(a);
            result.Children.Add(b);
            result.BackwardFunc = () =>
            {
                if (a.RequiresGrad)
                    AccumulateGrad(a, MatMul(result.Grad!, Transpose(b, -2, -1)));

                if (b.RequiresGrad)
                    AccumulateGrad(b, MatMul(Transpose(a, -2, -1), result.Grad!));
            };
        }

        return result;
    }

    // Swap two axes: Transpose(x, -2, -1) transposes a matrix
    public static Tensor Transpose(Tensor x, int dim1, int dim2)
    {
        int rank = x.Shape.Rank;
        int d1   = dim1 < 0 ? rank + dim1 : dim1;
        int d2   = dim2 < 0 ? rank + dim2 : dim2;

        var newDims = x.Shape.Dimensions.ToArray();
        (newDims[d1], newDims[d2]) = (newDims[d2], newDims[d1]);

        var newShape = new TensorShape(newDims);
        var newData  = new float[x.Shape.TotalElements];

        if (d1 == d2)
        {
            Array.Copy(x.Data, newData, x.Data.Length);
        }
        else if (d1 == rank - 2 && d2 == rank - 1)
        {
            var rows = x.Shape.Dimensions[d1];
            var cols = x.Shape.Dimensions[d2];
            var matrixSize = rows * cols;
            var batchCount = x.Shape.TotalElements / matrixSize;
            for (var batch = 0; batch < batchCount; batch++)
                SimdOps.Transpose2DInPlace(x.Data, batch * matrixSize, rows, cols, newData, batch * matrixSize);
        }
        else
        {
            for (int flat = 0; flat < x.Shape.TotalElements; flat++)
            {
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

    // Softmax over the given axis with the max-subtraction trick for numerical stability
    public static Tensor Softmax(Tensor x, int dim)
    {
        int normalizedDim = dim < 0 ? x.Shape.Rank + dim : dim;
        var resultData    = x.Data.ToArray();

        int outerSize = 1;
        for (int i = 0; i < normalizedDim; i++)
            outerSize *= x.Shape.Dimensions[i];

        int dimSize   = x.Shape.Dimensions[normalizedDim];
        int innerSize = x.Shape.TotalElements / (outerSize * dimSize);

        for (int outer = 0; outer < outerSize; outer++)
        for (int inner = 0; inner < innerSize; inner++)
        {
            float maxVal = float.NegativeInfinity;
            for (int d = 0; d < dimSize; d++)
            {
                int idx = (outer * dimSize + d) * innerSize + inner;
                if (resultData[idx] > maxVal) maxVal = resultData[idx];
            }

            float expSum = 0f;
            for (int d = 0; d < dimSize; d++)
            {
                int idx = (outer * dimSize + d) * innerSize + inner;
                resultData[idx] = MathF.Exp(resultData[idx] - maxVal);
                expSum += resultData[idx];
            }

            for (int d = 0; d < dimSize; d++)
            {
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

    // Reshape — change the shape without changing data (TotalElements must match)
    public static Tensor Reshape(Tensor x, params int[] newDims)
    {
        var newShape = new TensorShape(newDims);
        if (newShape.TotalElements != x.Shape.TotalElements)
            throw new ArgumentException(
                $"Reshape: {x.Shape} → {newShape} — different number of elements");

        var result = new Tensor(x.Data.ToArray(), newShape);

        if (x.RequiresGrad)
        {
            result.RequiresGrad = true;
            result.Children.Add(x);
            result.BackwardFunc = () => AccumulateGrad(x, Reshape(result.Grad!, x.Shape.Dimensions));
        }

        return result;
    }

    // Argmax — index of the maximum element (no gradient, inference only)
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

    // Slice the tensor along the first dimension: x[startRow..endRow, :]
    public static Tensor Slice(Tensor x, int startRow, int endRow)
    {
        if (x.Shape.Rank < 2)
            throw new ArgumentException("Slice requires rank >= 2");

        int rowLen   = x.Shape.TotalElements / x.Shape.Dimensions[0];
        int numRows  = endRow - startRow;
        var newDims  = x.Shape.Dimensions.ToArray();
        newDims[0]   = numRows;

        var data = new float[numRows * rowLen];
        Array.Copy(x.Data, startRow * rowLen, data, 0, numRows * rowLen);

        return new Tensor(data, new TensorShape(newDims));
    }

    // Concatenate two tensors along the given axis
    public static Tensor Concat(Tensor a, Tensor b, int axis)
    {
        int rank = a.Shape.Rank;
        int normAxis = axis < 0 ? rank + axis : axis;

        var newDims = a.Shape.Dimensions.ToArray();
        newDims[normAxis] += b.Shape.Dimensions[normAxis];

        var newShape = new TensorShape(newDims);
        var data     = new float[newShape.TotalElements];

        // Size of the "block" before the concatenation axis
        int outerSize  = 1;
        for (int i = 0; i < normAxis; i++) outerSize *= a.Shape.Dimensions[i];

        int aAxisSize  = a.Shape.Dimensions[normAxis];
        int bAxisSize  = b.Shape.Dimensions[normAxis];
        int innerSize  = a.Shape.TotalElements / (outerSize * aAxisSize);

        for (int outer = 0; outer < outerSize; outer++)
        {
            // Copy the block from a
            int aSrc = outer * aAxisSize * innerSize;
            int dst  = outer * (aAxisSize + bAxisSize) * innerSize;
            Array.Copy(a.Data, aSrc, data, dst, aAxisSize * innerSize);

            // Copy the block from b
            int bSrc = outer * bAxisSize * innerSize;
            Array.Copy(b.Data, bSrc, data, dst + aAxisSize * innerSize, bAxisSize * innerSize);
        }

        return new Tensor(data, newShape);
    }

    // Add to the accumulated gradient (grad += incoming)
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

    // --- Helper methods ---

    private static void AssertSameShape(Tensor a, Tensor b, string op)
    {
        if (!a.Shape.Dimensions.SequenceEqual(b.Shape.Dimensions))
            throw new ArgumentException(
                $"{op}: incompatible shapes {a.Shape} and {b.Shape}");
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
        int axis)
    {
        for (int dstFlat = 0; dstFlat < dstShape.TotalElements; dstFlat++)
        {
            var dstIdx = FlatToIndices(dstFlat, dstShape);
            float sum  = 0f;
            int axisSize = srcShape.Dimensions[axis];

            for (int k = 0; k < axisSize; k++)
            {
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

    // Insert a dimension of size 1 at position dim
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

    // Create a tensor [start, start+step, ...] with float values
    public static Tensor Arange(float start, float stop, float step = 1f)
    {
        int count = (int)Math.Ceiling((stop - start) / step);
        var data  = new float[count];
        for (int i = 0; i < count; i++)
            data[i] = start + i * step;
        return new Tensor(data, new TensorShape(count));
    }

    // Cosine, element-wise
    public static Tensor Cos(Tensor x)
    {
        var data = new float[x.Shape.TotalElements];
        for (int i = 0; i < data.Length; i++) data[i] = MathF.Cos(x.Data[i]);
        return new Tensor(data, x.Shape);
    }

    // Sine, element-wise
    public static Tensor Sin(Tensor x)
    {
        var data = new float[x.Shape.TotalElements];
        for (int i = 0; i < data.Length; i++) data[i] = MathF.Sin(x.Data[i]);
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

    // SiLU: x * sigmoid(x)  — used in SwiGLU
    public static Tensor Silu(Tensor x)
    {
        var data = new float[x.Shape.TotalElements];
        for (int i = 0; i < data.Length; i++)
            data[i] = x.Data[i] / (1f + MathF.Exp(-x.Data[i]));
        return new Tensor(data, x.Shape);
    }

    // LogSigmoid: log(sigmoid(x)) = -log(1 + exp(-x))
    public static Tensor LogSigmoid(Tensor x)
    {
        var data = new float[x.Shape.TotalElements];
        for (int i = 0; i < data.Length; i++)
            data[i] = -MathF.Log(1f + MathF.Exp(-x.Data[i]));
        return new Tensor(data, x.Shape);
    }

    // Clamp: bound values from below
    public static Tensor ClampMin(Tensor x, float minVal)
    {
        var data = new float[x.Shape.TotalElements];
        for (int i = 0; i < data.Length; i++)
            data[i] = Math.Max(x.Data[i], minVal);
        return new Tensor(data, x.Shape);
    }

    // Outer product of two vectors: [m] ⊗ [n] → [m, n]
    public static Tensor OuterProduct(Tensor a, Tensor b)
    {
        if (a.Shape.Rank != 1 || b.Shape.Rank != 1)
            throw new ArgumentException("OuterProduct requires two vectors (rank 1)");

        int m    = a.Shape.Dimensions[0];
        int n    = b.Shape.Dimensions[0];
        var data = new float[m * n];

        for (int i = 0; i < m; i++)
        for (int j = 0; j < n; j++)
            data[i * n + j] = a.Data[i] * b.Data[j];

        return new Tensor(data, new TensorShape(m, n));
    }

    // Repeat elements along an axis (repeat_interleave).
    // RepeatInterleave([a, b, c], 2, 0) → [a, a, b, b, c, c]
    public static Tensor RepeatInterleave(Tensor x, int repeats, int dim)
    {
        int rank = x.Shape.Rank;
        int normDim = dim < 0 ? rank + dim : dim;

        var newDims = x.Shape.Dimensions.ToArray();
        newDims[normDim] *= repeats;
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
            int srcIdx = (outer * dimSize + d) * innerSize + inner;
            int dstIdx = (outer * dimSize * repeats + d * repeats + rep) * innerSize + inner;
            data[dstIdx] = x.Data[srcIdx];
        }

        return new Tensor(data, newShape);
    }

    // Lower-triangular mask [n, n]: 1 on and below the main diagonal, 0 above
    public static Tensor Tril(int n)
    {
        var data = new float[n * n];
        for (int i = 0; i < n; i++)
        for (int j = 0; j <= i; j++)
            data[i * n + j] = 1f;
        return new Tensor(data, new TensorShape(n, n));
    }

    // Replace values where mask != 0 with fillValue
    public static Tensor MaskedFill(Tensor x, Tensor mask, float fillValue)
    {
        var data = x.Data.ToArray();
        for (int i = 0; i < data.Length; i++)
            if (mask.Data[i] != 0f)
                data[i] = fillValue;
        return new Tensor(data, x.Shape);
    }

    // Gather: for each index in index, take the element from src along this axis
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

    // CumSum along an axis — cumulative sum
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

    // Sort the last axis in descending order; returns (sortedValues, sortedIndices)
    public static (Tensor values, Tensor indices) SortDescending(Tensor x)
    {
        // Only for one-dimensional tensors (as in Sampler)
        if (x.Shape.Rank != 1)
            throw new NotImplementedException("SortDescending is implemented only for 1D tensors");

        int n    = x.Shape.TotalElements;
        var idxs = Enumerable.Range(0, n).OrderByDescending(i => x.Data[i]).ToArray();

        var vals   = idxs.Select(i => x.Data[i]).ToArray();
        var indArr = idxs.Select(i => (float)i).ToArray();

        return (new Tensor(vals, x.Shape), new Tensor(indArr, x.Shape));
    }

    // Scatter: result[indices[i]] = src[i] for 1D (used in Sampler for TopP)
    public static Tensor Scatter1D(Tensor src, Tensor indices, int size)
    {
        var data = new float[size];
        for (int i = 0; i < indices.Shape.TotalElements; i++)
            data[(int)indices.Data[i]] = src.Data[i];
        return new Tensor(data, new TensorShape(size));
    }

    // Squeeze: remove all dimensions = 1 (or a specific one)
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

    // L2-normalize with a minimum floor (clampMin)
    public static Tensor L2Normalize(Tensor x, float eps = 1e-9f)
    {
        // For a vector or matrix (normalize the last axis)
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

        // For 1D
        float n = 0f;
        foreach (var v in x.Data) n += v * v;
        n = Math.Max(MathF.Sqrt(n), eps);
        var d = x.Data.Select(v => v / n).ToArray();
        return new Tensor(d, x.Shape);
    }

    // CrossEntropy: log_softmax + NLL (for Trainer)
    // logits: [N, C], targets: int[] of length N → scalar loss
    public static float CrossEntropy(Tensor logits, int[] targets)
    {
        // logits: [N, C]
        int n = logits.Shape.Dimensions[0];
        int c = logits.Shape.Dimensions[1];
        float totalLoss = 0f;

        for (int i = 0; i < n; i++)
        {
            // Compute log_softmax for row i
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

    // ClipGradNorm: scale gradients if their norm > maxNorm
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
