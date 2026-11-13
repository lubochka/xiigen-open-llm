// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;
using TensorCore.Init;

namespace TensorCore.Layers;

// Линейный слой: output = input @ weight^T + bias.
// Аналог torch.nn.Linear.
public class Linear : Module
{
    // Матрица весов [outFeatures, inFeatures]; транспонируется при умножении
    public Parameter Weight { get; }

    // Вектор смещения [outFeatures]; null если useBias=false
    public Parameter? Bias { get; }

    public int InFeatures  { get; }
    public int OutFeatures { get; }

    public Linear(int inFeatures, int outFeatures, bool useBias = true)
    {
        InFeatures  = inFeatures;
        OutFeatures = outFeatures;

        // Kaiming Uniform — стандартная инициализация для сетей с ReLU
        var weightData = Initializers.KaimingUniform(outFeatures, inFeatures);
        Weight = new Parameter(new Tensor(weightData, new TensorShape(outFeatures, inFeatures)));
        RegisterParameter("weight", Weight);

        if (useBias)
        {
            Bias = new Parameter(new Tensor(new float[outFeatures], new TensorShape(outFeatures)));
            RegisterParameter("bias", Bias);
        }
    }

    // Прямой проход: [..., inFeatures] → [..., outFeatures]
    public Tensor Forward(Tensor input)
        => Forward(input, CancellationToken.None);

    public Tensor Forward(Tensor input, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // input @ weight^T: вес транспонируется с [out, in] → [in, out]
        var weightT = TensorOps.Transpose(Weight.Value, 0, 1, cancellationToken);
        var output  = TensorOps.MatMul(input, weightT, cancellationToken);

        if (Bias != null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            // Расширяем bias для broadcast сложения
            var biasExpanded = ExpandBias(Bias.Value, output.Shape, cancellationToken);
            output = TensorOps.Add(output, biasExpanded, cancellationToken);
        }

        return output;
    }

    // Повторяем вектор [outFeatures] по всем позициям батча/seq
    private static Tensor ExpandBias(
        Tensor bias,
        TensorShape outputShape,
        CancellationToken cancellationToken)
    {
        var data    = new float[outputShape.TotalElements];
        int outSize = outputShape.Dimensions[^1];

        // Количество повторений = все размерности кроме последней
        int repeats = outputShape.TotalElements / outSize;

        for (int r = 0; r < repeats; r++)
        for (int o = 0; o < outSize; o++)
        {
            if ((o & 4095) == 0)
                cancellationToken.ThrowIfCancellationRequested();
            data[r * outSize + o] = bias.Data[o];
        }

        var expanded = new Tensor(data, outputShape, bias.RequiresGrad);
        if (bias.RequiresGrad)
        {
            expanded.Children.Add(bias);
            expanded.BackwardFunc = () =>
            {
                if (!bias.RequiresGrad) return;

                var gradient = new float[bias.Data.Length];
                for (int r = 0; r < repeats; r++)
                for (int o = 0; o < outSize; o++)
                    gradient[o] += expanded.Grad!.Data[r * outSize + o];

                TensorOps.AccumulateGrad(bias, new Tensor(gradient, bias.Shape));
            };
        }

        return expanded;
    }
}
