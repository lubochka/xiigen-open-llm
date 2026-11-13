using TensorCore.Core;
using Tokenizer;
using TransformerModel;

namespace Inference;

// Авторегрессивная генерация текста.
// Prefill: все токены подсказки за один проход.
// Decode: по одному токену, останавливаемся на EOS или maxNewTokens.
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

        // Prefill: прогоняем все токены подсказки
        var prefillTensor = IdsToTensor(inputIds);
        var prefillLogits = _model.Forward(prefillTensor);  // [1, T, vocab]

        // Берём логиты последней позиции
        int seqLen   = inputIds.Count;
        int vocabSize = prefillLogits.Shape.Dimensions[2];
        var lastLogits = ExtractLastLogits(prefillLogits, seqLen, vocabSize);
        int nextToken  = Sampler.Sample(lastLogits, temperature, topK, topP);
        generated.Add(nextToken);

        // Decode: по одному токену
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

    // Преобразовать список токенов в Tensor [1, seqLen]
    private static Tensor IdsToTensor(IEnumerable<int> ids)
    {
        var arr  = ids.Select(i => (float)i).ToArray();
        return new Tensor(arr, new TensorShape(1, arr.Length));
    }

    // Вырезать логиты последней позиции: [1, T, vocab] → [vocab]
    private static Tensor ExtractLastLogits(Tensor logits, int seqLen, int vocabSize)
    {
        int offset = (seqLen - 1) * vocabSize;
        var data   = new float[vocabSize];
        Array.Copy(logits.Data, offset, data, 0, vocabSize);
        return new Tensor(data, new TensorShape(vocabSize));
    }
}
