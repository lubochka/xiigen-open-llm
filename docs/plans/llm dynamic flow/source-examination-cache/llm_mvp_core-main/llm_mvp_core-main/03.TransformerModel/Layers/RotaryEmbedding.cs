using TensorCore.Core;
using TensorCore.Layers;

namespace TransformerModel.Layers;

// RoPE — Rotary Position Embedding.
// Поворачивает векторы Q и K на углы пропорциональные позиции токена,
// кодируя относительные расстояния между токенами без добавления отдельных позиционных весов.
public class RotaryEmbedding : Module
{
    private readonly int   _dim;
    private readonly float _theta;

    public RotaryEmbedding(int dim, float theta)
    {
        _dim   = dim;
        _theta = theta;
    }

    // Возвращает (cos, sin) матрицы формы [seqLen, dim/2]
    public (Tensor cos, Tensor sin) GetFrequencies(int seqLen)
    {
        int half = _dim / 2;

        // freqs[i] = 1 / theta^(2i/dim), частоты вращения для каждой пары измерений
        var freqData = new float[half];
        for (int i = 0; i < half; i++)
            freqData[i] = 1f / MathF.Pow(_theta, 2f * i / _dim);

        var freqs = new Tensor(freqData, new TensorShape(half));

        // Позиции [0, 1, ..., seqLen-1]
        var posData = new float[seqLen];
        for (int i = 0; i < seqLen; i++) posData[i] = i;
        var pos = new Tensor(posData, new TensorShape(seqLen));

        // Внешнее произведение: [seqLen, half]
        var outer = TensorOps.OuterProduct(pos, freqs);
        return (TensorOps.Cos(outer), TensorOps.Sin(outer));
    }

    // Применить RoPE к тензору [batch, heads, seq, dim]
    public static Tensor Apply(Tensor x, Tensor cos, Tensor sin)
    {
        int batch = x.Shape.Dimensions[0];
        int heads = x.Shape.Dimensions[1];
        int seq   = x.Shape.Dimensions[2];
        int dim   = x.Shape.Dimensions[3];
        int half  = dim / 2;

        var data = new float[x.Shape.TotalElements];

        for (int b = 0; b < batch; b++)
        for (int h = 0; h < heads; h++)
        for (int t = 0; t < seq; t++)
        {
            int baseOff = ((b * heads + h) * seq + t) * dim;

            for (int i = 0; i < half; i++)
            {
                float x1 = x.Data[baseOff + i];
                float x2 = x.Data[baseOff + half + i];
                float c  = cos.Data[t * half + i];
                float s  = sin.Data[t * half + i];

                // Формула вращения в 2D: (x1, x2) → (x1*c - x2*s, x1*s + x2*c)
                data[baseOff + i]        = x1 * c - x2 * s;
                data[baseOff + half + i] = x1 * s + x2 * c;
            }
        }

        return new Tensor(data, x.Shape);
    }
}
