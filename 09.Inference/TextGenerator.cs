// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;
using Tokenizer;
using TransformerModel;

namespace Inference;

// Autoregressive text generation.
// Prefill: all prompt tokens in a single pass.
// Decode: one token at a time, stopping at EOS or maxNewTokens.
public class TextGenerator
{
    private readonly LlmModel    _model;
    private readonly BpeTokenizer _tokenizer;

    public TextGenerator(LlmModel model, BpeTokenizer tokenizer)
    {
        _model     = model;
        _tokenizer = tokenizer;
    }

    public string Generate(
        string prompt,
        int   maxNewTokens = 200,
        float temperature  = 0.8f,
        int   topK         = 50,
        float topP         = 0.95f)
    {
        var inputIds  = _tokenizer.Encode(prompt);
        var generated = new List<int>(inputIds);
        int eosToken  = _tokenizer.EosToken;

        // Prefill: run all prompt tokens
        var prefillTensor = IdsToTensor(inputIds);
        var prefillLogits = _model.Forward(prefillTensor);  // [1, T, vocab]

        // Take the logits of the last position
        int seqLen   = inputIds.Count;
        int vocabSize = prefillLogits.Shape.Dimensions[2];
        var lastLogits = ExtractLastLogits(prefillLogits, seqLen, vocabSize);
        int nextToken  = Sampler.Sample(lastLogits, temperature, topK, topP);
        generated.Add(nextToken);

        // Decode: one token at a time
        for (int step = 0; step < maxNewTokens - 1; step++)
        {
            if (nextToken == eosToken) break;

            var inputTensor = IdsToTensor(new[] { nextToken });
            var logits      = _model.Forward(inputTensor);  // [1, 1, vocab]
            lastLogits      = ExtractLastLogits(logits, 1, vocabSize);
            nextToken       = Sampler.Sample(lastLogits, temperature, topK, topP);
            generated.Add(nextToken);
        }

        return _tokenizer.Decode(generated.Skip(inputIds.Count).ToList());
    }

    // Convert a list of tokens into a Tensor [1, seqLen]
    private static Tensor IdsToTensor(IEnumerable<int> ids)
    {
        var arr  = ids.Select(i => (float)i).ToArray();
        return new Tensor(arr, new TensorShape(1, arr.Length));
    }

    // Extract the logits of the last position: [1, T, vocab] → [vocab]
    private static Tensor ExtractLastLogits(Tensor logits, int seqLen, int vocabSize)
    {
        int offset = (seqLen - 1) * vocabSize;
        var data   = new float[vocabSize];
        Array.Copy(logits.Data, offset, data, 0, vocabSize);
        return new Tensor(data, new TensorShape(vocabSize));
    }
}
