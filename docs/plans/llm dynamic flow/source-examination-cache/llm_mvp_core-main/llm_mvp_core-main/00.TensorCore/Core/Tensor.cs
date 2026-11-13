namespace TensorCore.Core;

// Многомерный массив float с поддержкой автоматического дифференцирования.
// Аналог torch.Tensor. Хранит данные, форму, градиент и граф вычислений.
public sealed class Tensor
{
    public TensorStorage Storage { get; }
    public TensorShape   Shape   { get; }

    // Удобный доступ к плоскому массиву данных
    public float[] Data => Storage.Data;

    // true для обучаемых параметров — при backward вычисляется Grad
    public bool RequiresGrad { get; set; }

    // Накопленный градиент (та же форма что и тензор); null до первого backward
    public Tensor? Grad { get; set; }

    // Функция обратного прохода; null для листовых тензоров
    internal Action? BackwardFunc { get; set; }

    // Входные тензоры в графе вычислений (нужны для топологической сортировки)
    internal List<Tensor> Children { get; } = new();

    // Тензор нулей заданной формы
    public Tensor(TensorShape shape, bool requiresGrad = false)
    {
        Shape        = shape;
        Storage      = new TensorStorage(shape.TotalElements);
        RequiresGrad = requiresGrad;
    }

    // Тензор из готового массива — данные копируются внутрь
    public Tensor(float[] data, TensorShape shape, bool requiresGrad = false)
    {
        if (data.Length != shape.TotalElements)
            throw new ArgumentException(
                $"Размер данных {data.Length} не совпадает с формой {shape}");
        Shape        = shape;
        Storage      = new TensorStorage(data);
        RequiresGrad = requiresGrad;
    }

    // Доступ по многомерным индексам: tensor[2, 3]
    public float this[params int[] indices]
    {
        get => Data[Shape.FlatIndex(indices)];
        set => Data[Shape.FlatIndex(indices)] = value;
    }

    // Обнулить накопленный градиент перед новой итерацией обучения
    public void ZeroGrad()
    {
        if (Grad != null)
            Array.Clear(Grad.Data, 0, Grad.Data.Length);
    }

    // Запустить обратное распространение градиента от этого узла
    public void Backward()
    {
        // Начальный градиент выходного тензора = 1 (dLoss/dLoss = 1)
        Grad = TensorOps.OnesLike(this);

        // Обходим граф от выхода к входам
        foreach (var node in TopologicalSort())
            node.BackwardFunc?.Invoke();
    }

    // Топологическая сортировка DFS — сначала потребители, потом производители
    private List<Tensor> TopologicalSort()
    {
        var sorted  = new List<Tensor>();
        var visited = new HashSet<Tensor>(ReferenceEqualityComparer.Instance);

        void Visit(Tensor node)
        {
            if (visited.Contains(node)) return;
            visited.Add(node);
            foreach (var child in node.Children)
                Visit(child);
            sorted.Add(node);
        }

        Visit(this);
        sorted.Reverse();
        return sorted;
    }

    public override string ToString() => $"Tensor{Shape}, RequiresGrad={RequiresGrad}";
}
