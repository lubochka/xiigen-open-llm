using TensorCore.Core;

namespace TensorCore.Layers;

// Базовый класс для всех слоёв нейронной сети.
// Аналог torch.nn.Module. Хранит параметры и дочерние слои.
public abstract class Module
{
    private readonly Dictionary<string, Parameter> _parameters = new();
    private readonly Dictionary<string, Module>    _submodules = new();

    // Зарегистрировать обучаемый параметр — вызывается в конструкторе дочернего класса
    protected void RegisterParameter(string name, Parameter parameter)
        => _parameters[name] = parameter;

    // Зарегистрировать дочерний слой (подмодуль)
    protected void RegisterModule(string name, Module module)
        => _submodules[name] = module;

    // Все параметры этого слоя и всех вложенных слоёв рекурсивно.
    // Используется оптимизатором для обновления весов.
    public IEnumerable<Parameter> Parameters()
    {
        foreach (var p in _parameters.Values)
            yield return p;

        foreach (var sub in _submodules.Values)
        foreach (var p in sub.Parameters())
            yield return p;
    }

    // Обнулить градиенты всех параметров перед новой итерацией
    public void ZeroGrad()
    {
        foreach (var p in Parameters())
            p.ZeroGrad();
    }
}
