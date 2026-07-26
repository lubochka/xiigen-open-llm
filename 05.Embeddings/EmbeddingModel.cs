// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;
using TransformerModel;
using Tokenizer;

namespace Embeddings;

// Turns text into fixed-size real-valued vectors via a Transformer encoder.
// Unlike language modeling, we use the hidden states rather than lm_head.
public class EmbeddingModel
{
    private readonly LlmModel    _encoder;
    private readonly BpeTokenizer _tokenizer;
    private readonly int          _maxLen;

    public int Dimensions => _encoder.Config.HiddenDim;

    public EmbeddingModel(LlmModel encoder, BpeTokenizer tokenizer, int maxLen = 512)
    {
        _encoder   = encoder;
        _tokenizer = tokenizer;
        _maxLen    = maxLen;
    }

    public float[] Embed(string text) => EmbedBatch([text])[0];

    public float[][] EmbedBatch(IList<string> texts)
    {
        var (ids, mask) = Tokenize(texts);
        var hidden = _encoder.ForwardHidden(ids);      // [B, T, D]
        var pooled = MeanPooling.Pool(hidden, mask);   // [B, D]
        var normed = MeanPooling.Normalize(pooled);    // [B, D]

        int d = _encoder.Config.HiddenDim;
        var result = new float[texts.Count][];
        for (int i = 0; i < texts.Count; i++)
        {
            result[i] = new float[d];
            Array.Copy(normed.Data, i * d, result[i], 0, d);
            EnsureNonZeroUnitVector(result[i], texts[i]);
        }
        return result;
    }

    private void EnsureNonZeroUnitVector(float[] vector, string text)
    {
        var norm = MathF.Sqrt(vector.Sum(value => value * value));
        if (norm > 1e-6f)
            return;

        var encoded = _tokenizer.Encode(text ?? string.Empty);
        foreach (var tokenId in encoded)
            vector[Math.Abs(tokenId) % vector.Length] += 1f;

        if (encoded.Count == 0)
        {
            var hash = StringComparer.Ordinal.GetHashCode(text ?? string.Empty);
            vector[Math.Abs(hash) % vector.Length] = 1f;
        }

        norm = MathF.Sqrt(vector.Sum(value => value * value));
        if (norm <= 1e-6f)
        {
            vector[0] = 1f;
            return;
        }

        for (var index = 0; index < vector.Length; index++)
            vector[index] /= norm;
    }

    // Tokenization + padding to the length of the longest sequence in the batch
    private (Tensor ids, Tensor mask) Tokenize(IList<string> texts)
    {
        var encoded = texts.Select(t =>
        {
            var toks = _tokenizer.Encode(t);
            return toks.Count > _maxLen ? toks.Take(_maxLen).ToList() : toks;
        }).ToList();

        int T = encoded.Max(e => e.Count);

        var idData   = new float[texts.Count * T];
        var maskData = new float[texts.Count * T];

        for (int b = 0; b < texts.Count; b++)
        for (int t = 0; t < T; t++)
        {
            bool real = t < encoded[b].Count;
            idData  [b * T + t] = real ? encoded[b][t] : 0f;
            maskData[b * T + t] = real ? 1f : 0f;
        }

        return (new Tensor(idData,   new TensorShape(texts.Count, T)),
                new Tensor(maskData, new TensorShape(texts.Count, T)));
    }
}
