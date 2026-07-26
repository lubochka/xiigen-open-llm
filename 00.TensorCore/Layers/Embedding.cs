// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;
using TensorCore.Init;

namespace TensorCore.Layers;

// Embedding table — turns integer IDs into continuous vectors.
// Analogous to torch.nn.Embedding.
// Holds a weight matrix [vocabSize, hiddenDim]; returns a row for each token.
public class Embedding : Module
{
    public Parameter Weight { get; }

    public int VocabSize  { get; }
    public int HiddenDim  { get; }

    public Embedding(int vocabSize, int hiddenDim)
    {
        VocabSize = vocabSize;
        HiddenDim = hiddenDim;

        // Normal initialization with a small std (standard practice)
        var weightData = Initializers.Normal(vocabSize, hiddenDim, std: 0.02f);
        Weight = new Parameter(new Tensor(weightData, new TensorShape(vocabSize, hiddenDim)));
        RegisterParameter("weight", Weight);
    }

    // Look up embeddings by token IDs.
    // ids: [batch, seqLen] — integers (stored as float)
    // Returns: [batch, seqLen, hiddenDim]
    public Tensor Forward(Tensor ids)
    {
        int batch  = ids.Shape.Dimensions[0];
        int seqLen = ids.Shape.Dimensions[1];
        var data   = new float[batch * seqLen * HiddenDim];

        for (int b = 0; b < batch; b++)
        for (int t = 0; t < seqLen; t++)
        {
            int tokenId = (int)ids.Data[b * seqLen + t];
            int srcOff  = tokenId * HiddenDim;
            int dstOff  = (b * seqLen + t) * HiddenDim;
            Array.Copy(Weight.Value.Data, srcOff, data, dstOff, HiddenDim);
        }

        return new Tensor(data, new TensorShape(batch, seqLen, HiddenDim));
    }
}
