using TensorCore.Core;

namespace TensorCore.Layers;

// Обучаемый параметр нейросети — тензор для которого всегда вычисляется градиент.
// Аналог torch.nn.Parameter.
public sealed class Parameter
{
    // Числовые значения параметра (веса нейронной сети)
    public Tensor Value { get; }

    // Всегда устанавливаем RequiresGrad = true — параметр по определению обучаемый
    public Parameter(Tensor initialValue)
    {
        Value = initialValue;
        Value.RequiresGrad = true;
    }

    // Градиент параметра — заполняется при backward()
    public Tensor? Grad => Value.Grad;

    // Обнулить градиент перед новой итерацией
    public void ZeroGrad() => Value.ZeroGrad();
}
