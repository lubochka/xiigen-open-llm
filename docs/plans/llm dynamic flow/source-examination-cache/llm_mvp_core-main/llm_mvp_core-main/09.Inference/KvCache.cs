using TensorCore.Core;

namespace Inference;

// Кэш ключей и значений на слой для авторегрессивной генерации.
// При каждом шаге декодирования добавляем один токен и конкатенируем с кэшем.
public class KvCache : IDisposable
{
    private readonly Tensor?[] _keys;
    private readonly Tensor?[] _values;
    private bool _disposed;

    public int SeqLen { get; private set; }

    public KvCache(int numLayers)
    {
        _keys   = new Tensor?[numLayers];
        _values = new Tensor?[numLayers];
    }

    // Добавить новый K/V срез и вернуть полный накопленный кэш.
    // newK/newV: [batch, heads, 1, headDim]
    // Возвращает: [batch, heads, seqLen, headDim]
    public (Tensor keys, Tensor values) Update(int layer, Tensor newK, Tensor newV)
    {
        if (_keys[layer] is null)
        {
            // Первый токен — просто копируем данные
            _keys[layer]   = new Tensor(newK.Data.ToArray(), newK.Shape);
            _values[layer] = new Tensor(newV.Data.ToArray(), newV.Shape);
        }
        else
        {
            // Конкатенируем по оси 2 (ось последовательности)
            _keys[layer]   = TensorOps.Concat(_keys[layer]!,   newK, axis: 2);
            _values[layer] = TensorOps.Concat(_values[layer]!, newV, axis: 2);
        }

        // SeqLen отслеживается по слою 0
        if (layer == 0) SeqLen = _keys[0]!.Shape.Dimensions[2];

        return (_keys[layer]!, _values[layer]!);
    }

    // Очистить кэш (начать новую генерацию)
    public void Clear()
    {
        for (int i = 0; i < _keys.Length; i++)
        {
            _keys[i]   = null;
            _values[i] = null;
        }
        SeqLen = 0;
    }

    public void Dispose()
    {
        if (_disposed) return;
        Clear();
        _disposed = true;
    }
}
