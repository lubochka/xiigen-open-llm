namespace TensorCore.Core;

// Плоское хранилище данных тензора.
// Отделено от формы чтобы в будущем view-операции могли делить одно хранилище.
public sealed class TensorStorage
{
    // Все числа тензора в row-major порядке
    public float[] Data { get; }

    public TensorStorage(int totalElements)
    {
        Data = new float[totalElements];
    }

    // Конструктор с начальными данными — копируем чтобы не было внешних алиасов
    public TensorStorage(float[] data)
    {
        Data = data.ToArray();
    }

    public int Length => Data.Length;
}
