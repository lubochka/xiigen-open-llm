using TensorCore.Core;
using TransformerModel;
using Tokenizer;

namespace Embeddings;

// Превращает текст в фиксированные вещественные векторы через Transformer-энкодер.
// В отличие от языкового моделирования используем скрытые состояния, а не lm_head.
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
        }
        return result;
    }

    // Токенизация + padding до длины самой длинной последовательности в батче
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
