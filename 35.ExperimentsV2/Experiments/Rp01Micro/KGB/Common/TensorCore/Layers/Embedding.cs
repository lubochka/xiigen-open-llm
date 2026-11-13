// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;
using TensorCore.Init;

namespace TensorCore.Layers;

// Таблица эмбеддингов — превращает целочисленные ID в непрерывные векторы.
// Аналог torch.nn.Embedding.
// Хранит матрицу весов [vocabSize, hiddenDim]; для каждого токена возвращает строку.
public class Embedding : Module
{
    public Parameter Weight { get; }

    public int VocabSize  { get; }
    public int HiddenDim  { get; }

    public Embedding(int vocabSize, int hiddenDim)
    {
        VocabSize = vocabSize;
        HiddenDim = hiddenDim;

        // Нормальная инициализация с малым std (стандартная практика)
        var weightData = Initializers.Normal(vocabSize, hiddenDim, std: 0.02f);
        Weight = new Parameter(new Tensor(weightData, new TensorShape(vocabSize, hiddenDim)));
        RegisterParameter("weight", Weight);
    }

    // Поиск эмбеддингов по идентификаторам токенов.
    // ids: [batch, seqLen] — целые числа (хранятся как float)
    // Возвращает: [batch, seqLen, hiddenDim]
    public Tensor Forward(Tensor ids)
        => Forward(ids, CancellationToken.None);

    public Tensor Forward(Tensor ids, CancellationToken cancellationToken)
    {
        int batch  = ids.Shape.Dimensions[0];
        int seqLen = ids.Shape.Dimensions[1];
        var data   = new float[batch * seqLen * HiddenDim];
        var weight = Weight.Value;
        bool requiresGrad = weight.RequiresGrad;
        var tokenIds = requiresGrad ? new int[batch * seqLen] : null;

        for (int b = 0; b < batch; b++)
        for (int t = 0; t < seqLen; t++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int tokenId = (int)ids.Data[b * seqLen + t];
            if (tokenIds is not null)
                tokenIds[b * seqLen + t] = tokenId;
            int srcOff  = tokenId * HiddenDim;
            int dstOff  = (b * seqLen + t) * HiddenDim;
            Array.Copy(Weight.Value.Data, srcOff, data, dstOff, HiddenDim);
        }

        var output = new Tensor(data, new TensorShape(batch, seqLen, HiddenDim), requiresGrad);
        if (requiresGrad)
        {
            output.Children.Add(weight);
            output.BackwardFunc = () =>
            {
                if (!weight.RequiresGrad)
                    return;

                var gradient = new float[weight.Data.Length];
                for (int position = 0; position < tokenIds!.Length; position++)
                {
                    int rowOffset = tokenIds[position] * HiddenDim;
                    int outputOffset = position * HiddenDim;
                    for (int column = 0; column < HiddenDim; column++)
                        gradient[rowOffset + column] += output.Grad!.Data[outputOffset + column];
                }

                TensorOps.AccumulateGrad(weight, new Tensor(gradient, weight.Shape));
            };
        }

        return output;
    }
}
