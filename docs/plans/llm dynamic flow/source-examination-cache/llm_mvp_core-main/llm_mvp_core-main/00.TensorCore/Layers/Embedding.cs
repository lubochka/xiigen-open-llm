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
