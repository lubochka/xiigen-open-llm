// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;
using TensorCore.Init;

namespace TensorCore.Layers;

// Linear layer: output = input @ weight^T + bias.
// Analogous to torch.nn.Linear.
public class Linear : Module
{
    // Weight matrix [outFeatures, inFeatures]; transposed during multiplication
    public Parameter Weight { get; }

    // Bias vector [outFeatures]; null if useBias=false
    public Parameter? Bias { get; }

    public int InFeatures  { get; }
    public int OutFeatures { get; }

    public Linear(int inFeatures, int outFeatures, bool useBias = true)
    {
        InFeatures  = inFeatures;
        OutFeatures = outFeatures;

        // Kaiming Uniform — the standard initialization for ReLU networks
        var weightData = Initializers.KaimingUniform(outFeatures, inFeatures);
        Weight = new Parameter(new Tensor(weightData, new TensorShape(outFeatures, inFeatures)));
        RegisterParameter("weight", Weight);

        if (useBias)
        {
            Bias = new Parameter(new Tensor(new float[outFeatures], new TensorShape(outFeatures)));
            RegisterParameter("bias", Bias);
        }
    }

    // Forward pass: [..., inFeatures] → [..., outFeatures]
    public Tensor Forward(Tensor input)
    {
        // input @ weight^T: the weight is transposed from [out, in] → [in, out]
        var weightT = TensorOps.Transpose(Weight.Value, 0, 1);
        var output  = TensorOps.MatMul(input, weightT);

        if (Bias != null)
        {
            // Expand bias for broadcast addition
            var biasExpanded = ExpandBias(Bias.Value, output.Shape);
            output = TensorOps.Add(output, biasExpanded);
        }

        return output;
    }

    // Repeat the vector [outFeatures] across all batch/seq positions
    private static Tensor ExpandBias(Tensor bias, TensorShape outputShape)
    {
        var data    = new float[outputShape.TotalElements];
        int outSize = outputShape.Dimensions[^1];

        // Number of repeats = all dimensions except the last
        int repeats = outputShape.TotalElements / outSize;

        for (int r = 0; r < repeats; r++)
        for (int o = 0; o < outSize; o++)
            data[r * outSize + o] = bias.Data[o];

        // Bias does not take part in gradient computation along this path —
        // its gradient is accumulated through RegisterParameter
        return new Tensor(data, outputShape);
    }
}
