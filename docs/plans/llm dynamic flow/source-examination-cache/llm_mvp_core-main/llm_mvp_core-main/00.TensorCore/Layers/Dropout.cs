using TensorCore.Core;

namespace TensorCore.Layers;

// Слой случайного обнуления нейронов (регуляризация).
// Во время обучения каждый нейрон обнуляется с вероятностью p.
// Во время инференса dropout отключён (все значения проходят).
public class Dropout : Module
{
    private readonly float _p;
    private readonly Random _rng;

    // p — вероятность обнуления; seed фиксирует воспроизводимость
    public Dropout(float p = 0.1f, int? seed = null)
    {
        _p   = p;
        _rng = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    // training=true — применяем dropout; training=false — пропускаем как есть
    public Tensor Forward(Tensor input, bool training = true)
    {
        if (!training || _p == 0f)
            return input;

        // Создаём маску: 1/(1-p) для выживших нейронов, 0 для обнулённых
        // Масштабирование на 1/(1-p) сохраняет ожидаемое значение активаций
        float scale = 1.0f / (1.0f - _p);
        var maskData = new float[input.Shape.TotalElements];
        for (int i = 0; i < maskData.Length; i++)
            maskData[i] = _rng.NextDouble() >= _p ? scale : 0f;

        var mask   = new Tensor(maskData, input.Shape);
        return TensorOps.Multiply(input, mask);
    }
}
