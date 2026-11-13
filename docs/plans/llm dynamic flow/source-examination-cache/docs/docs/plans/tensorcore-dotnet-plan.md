# Проект 00 — TensorCore (тензорная библиотека с автоматическим дифференцированием)
## Solution: LlmFromScratch.sln — Отдельный план подпроекта v1.0

**Проект:** llm-mvp-core — подпроект 00 (выполняется первым, до всех остальных)  
**Язык:** C# / .NET 9  
**Ограничение:** Ноль внешних пакетов. Никакого TorchSharp. Никакого NumPy. Только чистый C#.  
**Цель:** Заменить TorchSharp собственной реализацией тензорных операций и автоматического дифференцирования.  
**Этот документ относится только к проекту llm-mvp-core и не касается XIIGen.**

---

## Почему TorchSharp нужно заменить

TorchSharp — это обёртка поверх LibTorch (бинарный C++ файл от PyTorch, размер ~300 МБ). Это закрытый бинарник. Мы можем использовать его API, но не можем прочитать или изменить внутренний код. Это противоречит цели проекта — понимать каждый алгоритм изнутри.

Наша замена — TensorCore — реализует те же операции с нуля на чистом C#. Код можно читать, изменять, тестировать построчно.

---

## Открытые источники для изучения

```
micrograd — Andrej Karpathy, MIT, Python, ~150 строк
https://github.com/karpathy/micrograd

micrograd/engine.py
  - class Value       — скалярное значение + его градиент
  - __add__, __mul__, __pow__ — операции с записью backward функции
  - backward()        — обход графа вычислений от конца к началу

Это минимальная реализация автоматического дифференцирования.
Мы расширяем идею с скаляров на многомерные массивы (тензоры).
```

```
tinygrad — George Hotz, MIT, Python
https://github.com/tinygrad/tinygrad

tinygrad/tensor.py    — полный тензорный движок ~1000 строк
  - class Tensor       — многомерный массив + граф градиентов
  - Function           — базовый класс для дифференцируемых операций
  - matmul, sum, relu  — реализации операций с forward и backward
```

```
nanoGPT — Andrej Karpathy, MIT, Python
https://github.com/karpathy/nanoGPT

model.py — показывает какие именно операции нужны для трансформера
  - torch.mm           — матричное умножение
  - torch.nn.Linear    — линейный слой (матмул + смещение)
  - F.softmax          — нормализация вероятностей
  - .mean(), .pow()    — поэлементные операции
  - .backward()        — обратное распространение градиента
```

---

## Что TorchSharp сейчас делает в нашем коде

Просматривая проекты 03–11 видно что используются следующие операции:

| Операция TorchSharp | Что делает | Нужна для |
|---|---|---|
| `Tensor`, `ones`, `zeros`, `empty` | создание тензоров | 03, 04, 09, 10, 11 |
| `x.pow(2)`, `x.mean()`, `x.rsqrt()` | поэлементная математика | RmsNorm |
| `x.mm(y)`, `matmul(x, y)` | матричное умножение | Attention, Linear, LoRA |
| `x.transpose(-2, -1)` | перестановка измерений | Attention |
| `softmax(x, dim=-1)` | нормализация вероятностей | Attention, Sampler |
| `Parameter(...)` | обучаемый вес с градиентом | все модели |
| `Module<T>` | базовый класс слоя нейросети | все слои |
| `.backward()` | обратное распространение градиента | обучение |
| `init.kaiming_uniform_` | инициализация весов | LoRA |
| `Dropout` | случайное обнуление (регуляризация) | LoRA |

Всё это реализуемо с нуля. Ни одна операция не требует GPU или специального железа — всё работает на CPU.

---

## Структура подпроекта

```
00.TensorCore/
├── Core/
│   ├── Tensor.cs              — основной тип: многомерный массив float + градиент
│   ├── TensorOps.cs           — все математические операции
│   ├── TensorShape.cs         — форма тензора (размерности)
│   └── TensorStorage.cs       — хранение данных в плоском float[] массиве
├── Autograd/
│   ├── ComputationGraph.cs    — граф вычислений для обратного прохода
│   ├── GradientAccumulator.cs — накопление градиентов при backward
│   └── BackwardFunctions/
│       ├── MatMulBackward.cs  — градиент матричного умножения
│       ├── AddBackward.cs     — градиент сложения
│       ├── MulBackward.cs     — градиент поэлементного умножения
│       ├── PowBackward.cs     — градиент возведения в степень
│       ├── MeanBackward.cs    — градиент среднего значения
│       └── SoftmaxBackward.cs — градиент softmax
├── Layers/
│   ├── Parameter.cs           — обучаемый вес (тензор с флагом requires_grad)
│   ├── Module.cs              — базовый класс слоя нейросети
│   ├── Linear.cs              — линейный слой (y = xW^T + b)
│   └── Dropout.cs             — слой случайного обнуления нейронов
├── Init/
│   └── Initializers.cs        — инициализация весов (Kaiming, нормальное, нулевое)
└── Tests/
    ├── TensorOpsTests.cs
    ├── AutogradTests.cs
    └── LayerTests.cs
```

---

## Фаза 1 — Хранение данных и форма тензора (1 день)

Тензор — это многомерный массив чисел. Внутри он хранится как обычный плоский массив `float[]`. Форма (shape) описывает как этот плоский массив интерпретировать как многомерный.

Например тензор формы [2, 3] (матрица 2 строки × 3 столбца) хранится как массив из 6 чисел. Элемент `[i, j]` находится по индексу `i * 3 + j` — это называется stride (шаг).

```csharp
// Core/TensorShape.cs
// Форма тензора — описывает размеры каждого измерения
// Например [8, 512, 64] означает: пачка из 8 примеров,
// каждый из 512 позиций, каждая позиция — вектор 64 числа
public sealed class TensorShape
{
    // Размеры по каждому измерению
    // Например для матрицы 3×4: Dimensions = [3, 4]
    public int[] Dimensions { get; }

    // Количество измерений (rank — ранг тензора)
    // Скаляр: 0 измерений, вектор: 1, матрица: 2, ...
    public int Rank => Dimensions.Length;

    // Общее количество элементов во всём тензоре
    // Произведение всех размерностей: 3 × 4 = 12 для матрицы 3×4
    public int TotalElements { get; }

    // Шаги (strides) — на сколько нужно сдвинуться в плоском массиве
    // чтобы перейти к следующему элементу по каждому измерению
    // Для матрицы 3×4: strides = [4, 1]
    // Элемент [i,j] = storage[i*4 + j*1]
    public int[] Strides { get; }

    public TensorShape(params int[] dimensions)
    {
        // Проверяем что все размерности положительные
        if (dimensions.Any(d => d <= 0))
            throw new ArgumentException(
                $"Все размерности должны быть положительными. Получено: [{string.Join(", ", dimensions)}]");

        Dimensions    = dimensions.ToArray();
        TotalElements = dimensions.Aggregate(1, (acc, d) => acc * d);

        // Вычисляем strides справа налево
        // Последнее измерение всегда имеет шаг 1 (соседние элементы рядом)
        Strides = new int[dimensions.Length];
        Strides[^1] = 1;
        for (int i = dimensions.Length - 2; i >= 0; i--)
            Strides[i] = Strides[i + 1] * dimensions[i + 1];
    }

    // Вычислить позицию в плоском массиве по многомерным индексам
    // indices = [2, 1] для матрицы 3×4 → позиция 2*4 + 1*1 = 9
    public int FlatIndex(int[] indices)
    {
        if (indices.Length != Rank)
            throw new ArgumentException(
                $"Количество индексов {indices.Length} не совпадает с рангом {Rank}");

        int flatIndex = 0;
        for (int i = 0; i < Rank; i++)
        {
            if (indices[i] < 0 || indices[i] >= Dimensions[i])
                throw new IndexOutOfRangeException(
                    $"Индекс {indices[i]} выходит за пределы размерности {Dimensions[i]} на оси {i}");
            flatIndex += indices[i] * Strides[i];
        }
        return flatIndex;
    }

    // Строковое представление для отладки: "Shape[2, 3, 4]"
    public override string ToString() =>
        $"Shape[{string.Join(", ", Dimensions)}]";
}

// Core/TensorStorage.cs
// Хранилище чисел — плоский массив float
// Разделено от TensorShape чтобы разные виды тензоров могли
// ссылаться на одни и те же данные (view операции)
public sealed class TensorStorage
{
    // Плоский массив всех чисел тензора
    // Все операции работают с этим массивом через индексы
    public float[] Data { get; }

    public TensorStorage(int totalElements)
    {
        Data = new float[totalElements];
    }

    // Конструктор с начальными данными
    public TensorStorage(float[] data)
    {
        Data = data.ToArray();  // копируем чтобы не было внешних изменений
    }

    // Количество элементов в хранилище
    public int Length => Data.Length;
}
```

**Тесты для фазы 1:**
- `TensorShape([3, 4]).TotalElements == 12`
- `TensorShape([3, 4]).FlatIndex([2, 1]) == 9`
- `TensorShape([3, 4]).Strides == [4, 1]`

---

## Фаза 2 — Основной класс Tensor и поэлементные операции (2 дня)

```csharp
// Core/Tensor.cs
// Основной тип — многомерный массив чисел с поддержкой автоматического дифференцирования
// Аналог torch.Tensor в PyTorch
// Каждый тензор хранит:
//   - данные (числа)
//   - форму (как интерпретировать числа)
//   - градиент (если участвует в обучении)
//   - функцию для вычисления градиента (если создан операцией)
public sealed class Tensor
{
    // Числовые данные тензора
    public TensorStorage Storage { get; }

    // Форма тензора — размеры каждого измерения
    public TensorShape Shape { get; }

    // Удобный доступ к данным как к плоскому массиву
    public float[] Data => Storage.Data;

    // Нужно ли вычислять градиент для этого тензора при обратном проходе
    // true для обучаемых параметров (весов модели)
    // false для входных данных и промежуточных результатов
    public bool RequiresGrad { get; set; }

    // Накопленный градиент — заполняется при вызове backward()
    // Имеет ту же форму что и сам тензор
    // null если градиент ещё не вычислялся
    public Tensor? Grad { get; internal set; }

    // Функция обратного распространения — как вычислить градиент
    // null для "листовых" тензоров (созданных пользователем, не операцией)
    internal Action? BackwardFunc { get; set; }

    // Тензоры от которых зависит этот тензор (родители в графе вычислений)
    // Нужны для построения порядка обратного обхода
    internal List<Tensor> Children { get; } = new();

    // Создать тензор заданной формы заполненный нулями
    public Tensor(TensorShape shape, bool requiresGrad = false)
    {
        Shape       = shape;
        Storage     = new TensorStorage(shape.TotalElements);
        RequiresGrad = requiresGrad;
    }

    // Создать тензор из готового массива данных
    public Tensor(float[] data, TensorShape shape, bool requiresGrad = false)
    {
        if (data.Length != shape.TotalElements)
            throw new ArgumentException(
                $"Размер данных {data.Length} не совпадает с формой {shape}");

        Shape        = shape;
        Storage      = new TensorStorage(data);
        RequiresGrad = requiresGrad;
    }

    // Получить или установить элемент по многомерным индексам
    // tensor[2, 3] для матрицы — элемент строки 2, столбца 3
    public float this[params int[] indices]
    {
        get => Data[Shape.FlatIndex(indices)];
        set => Data[Shape.FlatIndex(indices)] = value;
    }

    // Обнулить накопленный градиент перед новым шагом обучения
    // Вызывается в начале каждой итерации обучения
    public void ZeroGrad()
    {
        if (Grad != null)
            Array.Clear(Grad.Data, 0, Grad.Data.Length);
    }

    // Запустить обратное распространение градиента
    // Вычисляет градиенты для всех тензоров с RequiresGrad=true
    // в графе вычислений который привёл к этому тензору
    public void Backward()
    {
        // Начальный градиент выходного тензора = 1.0
        // (производная Loss по Loss = 1)
        Grad = TensorOps.OnesLike(this);

        // Топологическая сортировка графа вычислений
        // (от выхода к входам, чтобы каждый узел обработался после всех его потребителей)
        var sortedNodes = TopologicalSort();

        // Обходим граф в обратном порядке и вызываем backward для каждого узла
        foreach (var node in sortedNodes)
        {
            node.BackwardFunc?.Invoke();
        }
    }

    // Топологическая сортировка графа — обход в глубину
    // Возвращает узлы в порядке "сначала потребители, потом производители"
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
        sorted.Reverse();  // разворачиваем чтобы получить порядок от выхода к входам
        return sorted;
    }

    // Строковое представление для отладки
    public override string ToString() =>
        $"Tensor{Shape}, RequiresGrad={RequiresGrad}";
}
```

```csharp
// Core/TensorOps.cs
// Все математические операции над тензорами
// Каждая операция реализует forward (прямой проход — вычисление результата)
// и регистрирует backward (обратный проход — как вычислить градиент)
//
// Правило написания операций:
// 1. Вычислить результат (out = ...)
// 2. Если кто-то из входов требует градиент — зарегистрировать backward функцию
// 3. Вернуть результат
public static class TensorOps
{
    // Создать тензор заполненный нулями той же формы что и образец
    public static Tensor ZerosLike(Tensor template) =>
        new(new float[template.Shape.TotalElements], template.Shape);

    // Создать тензор заполненный единицами той же формы что и образец
    public static Tensor OnesLike(Tensor template)
    {
        var data = new float[template.Shape.TotalElements];
        Array.Fill(data, 1.0f);
        return new Tensor(data, template.Shape);
    }

    // Поэлементное сложение двух тензоров одинаковой формы
    // Градиент сложения: d(a+b)/da = 1, d(a+b)/db = 1
    // Поэтому градиент просто передаётся обоим входам без изменений
    public static Tensor Add(Tensor a, Tensor b)
    {
        AssertSameShape(a, b, "Add");

        // Forward: вычисляем результат
        var resultData = new float[a.Shape.TotalElements];
        for (int i = 0; i < resultData.Length; i++)
            resultData[i] = a.Data[i] + b.Data[i];

        var result = new Tensor(resultData, a.Shape);

        // Backward: регистрируем как вычислять градиенты
        if (a.RequiresGrad || b.RequiresGrad)
        {
            result.RequiresGrad = true;
            result.Children.Add(a);
            result.Children.Add(b);

            result.BackwardFunc = () =>
            {
                // Градиент суммы для a: просто передаём gradOutput дальше
                if (a.RequiresGrad)
                    AccumulateGrad(a, result.Grad!);

                // Градиент суммы для b: тоже просто передаём gradOutput
                if (b.RequiresGrad)
                    AccumulateGrad(b, result.Grad!);
            };
        }

        return result;
    }

    // Поэлементное умножение двух тензоров
    // Градиент: d(a*b)/da = b, d(a*b)/db = a
    // То есть градиент для a = gradOutput * b, для b = gradOutput * a
    public static Tensor Multiply(Tensor a, Tensor b)
    {
        AssertSameShape(a, b, "Multiply");

        var resultData = new float[a.Shape.TotalElements];
        for (int i = 0; i < resultData.Length; i++)
            resultData[i] = a.Data[i] * b.Data[i];

        var result = new Tensor(resultData, a.Shape);

        if (a.RequiresGrad || b.RequiresGrad)
        {
            result.RequiresGrad = true;
            result.Children.Add(a);
            result.Children.Add(b);

            result.BackwardFunc = () =>
            {
                // Градиент для a = gradOutput поэлементно умножить на b
                if (a.RequiresGrad)
                {
                    var gradA = new float[a.Shape.TotalElements];
                    for (int i = 0; i < gradA.Length; i++)
                        gradA[i] = result.Grad!.Data[i] * b.Data[i];
                    AccumulateGrad(a, new Tensor(gradA, a.Shape));
                }

                // Градиент для b = gradOutput поэлементно умножить на a
                if (b.RequiresGrad)
                {
                    var gradB = new float[b.Shape.TotalElements];
                    for (int i = 0; i < gradB.Length; i++)
                        gradB[i] = result.Grad!.Data[i] * a.Data[i];
                    AccumulateGrad(b, new Tensor(gradB, b.Shape));
                }
            };
        }

        return result;
    }

    // Возведение каждого элемента в степень
    // Градиент: d(x^n)/dx = n * x^(n-1)
    public static Tensor Pow(Tensor x, float exponent)
    {
        var resultData = new float[x.Shape.TotalElements];
        for (int i = 0; i < resultData.Length; i++)
            resultData[i] = MathF.Pow(x.Data[i], exponent);

        var result = new Tensor(resultData, x.Shape);

        if (x.RequiresGrad)
        {
            result.RequiresGrad = true;
            result.Children.Add(x);

            result.BackwardFunc = () =>
            {
                // Градиент: exponent * x^(exponent-1) * gradOutput
                var gradData = new float[x.Shape.TotalElements];
                for (int i = 0; i < gradData.Length; i++)
                    gradData[i] = exponent
                        * MathF.Pow(x.Data[i], exponent - 1)
                        * result.Grad!.Data[i];

                AccumulateGrad(x, new Tensor(gradData, x.Shape));
            };
        }

        return result;
    }

    // Среднее значение по заданному измерению
    // Например mean по оси -1 для тензора [batch, seqLen, dim]
    // даёт тензор [batch, seqLen, 1]
    public static Tensor Mean(Tensor x, int axis, bool keepDims = true)
    {
        // Нормализуем отрицательный axis (Python-style: -1 = последняя ось)
        int normalizedAxis = axis < 0 ? x.Shape.Rank + axis : axis;

        int axisSize = x.Shape.Dimensions[normalizedAxis];

        // Вычисляем форму результата
        int[] resultDims = x.Shape.Dimensions.ToArray();
        if (keepDims)
            resultDims[normalizedAxis] = 1;
        else
            resultDims = resultDims.Where((_, i) => i != normalizedAxis).ToArray();

        var resultShape = new TensorShape(resultDims);
        var resultData  = new float[resultShape.TotalElements];

        // Суммируем по указанной оси и делим на размер оси
        SumAlongAxis(x.Data, x.Shape, resultData, resultShape, normalizedAxis);
        for (int i = 0; i < resultData.Length; i++)
            resultData[i] /= axisSize;

        var result = new Tensor(resultData, resultShape);

        if (x.RequiresGrad)
        {
            result.RequiresGrad = true;
            result.Children.Add(x);

            result.BackwardFunc = () =>
            {
                // Градиент среднего = 1/n * gradOutput, распределённый по всей оси
                var gradData = new float[x.Shape.TotalElements];
                float scale  = 1.0f / axisSize;

                // Распределяем градиент по всем элементам вдоль оси
                BroadcastGradAlongAxis(result.Grad!.Data, result.Shape,
                    gradData, x.Shape, normalizedAxis, scale);

                AccumulateGrad(x, new Tensor(gradData, x.Shape));
            };
        }

        return result;
    }

    // Матричное умножение двух двумерных тензоров (матриц)
    // A имеет форму [m, k], B имеет форму [k, n], результат [m, n]
    // Градиент: dL/dA = gradOutput @ B^T, dL/dB = A^T @ gradOutput
    // Источник: nanoGPT model.py — torch.mm() / nanoGPT использует именно это
    public static Tensor MatMul(Tensor a, Tensor b)
    {
        // Проверяем что размерности совместимы для умножения
        if (a.Shape.Rank < 2 || b.Shape.Rank < 2)
            throw new ArgumentException("MatMul требует тензоры не менее 2 измерений");

        int aRows = a.Shape.Dimensions[^2];  // строки A (предпоследнее измерение)
        int aCols = a.Shape.Dimensions[^1];  // столбцы A (последнее измерение)
        int bRows = b.Shape.Dimensions[^2];  // строки B
        int bCols = b.Shape.Dimensions[^1];  // столбцы B

        if (aCols != bRows)
            throw new ArgumentException(
                $"Несовместимые размеры для MatMul: {a.Shape} × {b.Shape}. " +
                $"Нужно чтобы последнее измерение A ({aCols}) совпадало с предпоследним B ({bRows})");

        // Форма результата: [...batches, aRows, bCols]
        var resultDims   = a.Shape.Dimensions[..^2]
            .Concat(new[] { aRows, bCols })
            .ToArray();
        var resultShape  = new TensorShape(resultDims);
        var resultData   = new float[resultShape.TotalElements];

        // Выполняем матричное умножение по батчам
        int batchSize   = a.Shape.TotalElements / (aRows * aCols);

        for (int batch = 0; batch < batchSize; batch++)
        {
            int aOffset = batch * aRows * aCols;
            int bOffset = batch * bRows * bCols;
            int rOffset = batch * aRows * bCols;

            // Классический алгоритм O(m*n*k) — тройной цикл
            // Для больших матриц можно заменить на BLAS, но для понимания достаточно этого
            for (int i = 0; i < aRows; i++)
            for (int j = 0; j < bCols; j++)
            {
                float sum = 0f;
                for (int k = 0; k < aCols; k++)
                    sum += a.Data[aOffset + i * aCols + k]
                         * b.Data[bOffset + k * bCols + j];

                resultData[rOffset + i * bCols + j] = sum;
            }
        }

        var result = new Tensor(resultData, resultShape);

        if (a.RequiresGrad || b.RequiresGrad)
        {
            result.RequiresGrad = true;
            result.Children.Add(a);
            result.Children.Add(b);

            result.BackwardFunc = () =>
            {
                // dL/dA = gradOutput @ B^T  (формула из матрического исчисления)
                if (a.RequiresGrad)
                {
                    var bTransposed = Transpose(b, -2, -1);
                    var gradA = MatMul(result.Grad!, bTransposed);
                    AccumulateGrad(a, gradA);
                }

                // dL/dB = A^T @ gradOutput
                if (b.RequiresGrad)
                {
                    var aTransposed = Transpose(a, -2, -1);
                    var gradB = MatMul(aTransposed, result.Grad!);
                    AccumulateGrad(b, gradB);
                }
            };
        }

        return result;
    }

    // Перестановка двух измерений тензора
    // Transpose(x, -2, -1) меняет местами предпоследнее и последнее измерения
    // Для матрицы [m, n] → [n, m]
    public static Tensor Transpose(Tensor x, int dim1, int dim2)
    {
        int rank = x.Shape.Rank;
        int d1   = dim1 < 0 ? rank + dim1 : dim1;
        int d2   = dim2 < 0 ? rank + dim2 : dim2;

        // Меняем размерности местами для новой формы
        var newDims       = x.Shape.Dimensions.ToArray();
        (newDims[d1], newDims[d2]) = (newDims[d2], newDims[d1]);

        var newShape = new TensorShape(newDims);
        var newData  = new float[x.Shape.TotalElements];

        // Переставляем элементы с учётом новых strides
        for (int flat = 0; flat < x.Shape.TotalElements; flat++)
        {
            // Переводим плоский индекс в многомерный
            var indices = FlatToIndices(flat, x.Shape);

            // Меняем нужные индексы местами
            (indices[d1], indices[d2]) = (indices[d2], indices[d1]);

            // Записываем в новую позицию
            newData[newShape.FlatIndex(indices)] = x.Data[flat];
        }

        var result = new Tensor(newData, newShape);

        if (x.RequiresGrad)
        {
            result.RequiresGrad = true;
            result.Children.Add(x);

            // Градиент транспозиции = транспозиция градиента
            result.BackwardFunc = () =>
            {
                var gradX = Transpose(result.Grad!, dim1, dim2);
                AccumulateGrad(x, gradX);
            };
        }

        return result;
    }

    // Нормализация вероятностей по оси — softmax
    // Превращает произвольные числа в вероятности (все ≥ 0, сумма = 1)
    // Формула: softmax(x_i) = exp(x_i) / sum(exp(x_j))
    // Для численной стабильности вычитаем максимум перед exp (трюк max subtraction)
    // Источник: nanoGPT model.py — F.softmax()
    public static Tensor Softmax(Tensor x, int dim)
    {
        int normalizedDim = dim < 0 ? x.Shape.Rank + dim : dim;
        var resultData    = x.Data.ToArray();

        // Обрабатываем каждую строку вдоль указанного измерения
        int outerSize = 1;
        for (int i = 0; i < normalizedDim; i++)
            outerSize *= x.Shape.Dimensions[i];

        int dimSize  = x.Shape.Dimensions[normalizedDim];
        int innerSize = x.Shape.TotalElements / (outerSize * dimSize);

        for (int outer = 0; outer < outerSize; outer++)
        for (int inner = 0; inner < innerSize; inner++)
        {
            // Находим максимум для численной стабильности
            float maxVal = float.NegativeInfinity;
            for (int d = 0; d < dimSize; d++)
            {
                int idx = (outer * dimSize + d) * innerSize + inner;
                if (resultData[idx] > maxVal) maxVal = resultData[idx];
            }

            // Вычисляем exp(x - max) и их сумму
            float expSum = 0f;
            for (int d = 0; d < dimSize; d++)
            {
                int idx = (outer * dimSize + d) * innerSize + inner;
                resultData[idx] = MathF.Exp(resultData[idx] - maxVal);
                expSum += resultData[idx];
            }

            // Нормализуем
            for (int d = 0; d < dimSize; d++)
            {
                int idx = (outer * dimSize + d) * innerSize + inner;
                resultData[idx] /= expSum;
            }
        }

        var result = new Tensor(resultData, x.Shape);

        if (x.RequiresGrad)
        {
            result.RequiresGrad = true;
            result.Children.Add(x);

            result.BackwardFunc = () =>
            {
                // Градиент softmax: для каждой позиции i
                // grad_x[i] = softmax[i] * (grad_out[i] - sum(grad_out[j] * softmax[j]))
                // Это jacobian-vector product для softmax
                var gradData = new float[x.Shape.TotalElements];

                for (int outer = 0; outer < outerSize; outer++)
                for (int inner = 0; inner < innerSize; inner++)
                {
                    // Вычисляем dot product = sum(grad_out[j] * softmax[j])
                    float dotProduct = 0f;
                    for (int d = 0; d < dimSize; d++)
                    {
                        int idx = (outer * dimSize + d) * innerSize + inner;
                        dotProduct += result.Grad!.Data[idx] * result.Data[idx];
                    }

                    for (int d = 0; d < dimSize; d++)
                    {
                        int idx = (outer * dimSize + d) * innerSize + inner;
                        gradData[idx] = result.Data[idx]
                            * (result.Grad!.Data[idx] - dotProduct);
                    }
                }

                AccumulateGrad(x, new Tensor(gradData, x.Shape));
            };
        }

        return result;
    }

    // Добавить градиент к накопленному градиенту тензора
    // Используем += потому что один тензор может использоваться несколько раз
    // и градиенты от всех использований нужно сложить
    internal static void AccumulateGrad(Tensor target, Tensor gradToAdd)
    {
        if (!target.RequiresGrad) return;

        if (target.Grad == null)
        {
            // Первый градиент — создаём копию
            target.Grad = new Tensor(gradToAdd.Data.ToArray(), gradToAdd.Shape);
        }
        else
        {
            // Последующие — добавляем
            for (int i = 0; i < target.Grad.Data.Length; i++)
                target.Grad.Data[i] += gradToAdd.Data[i];
        }
    }

    // Вспомогательный метод: проверить что два тензора имеют одинаковую форму
    private static void AssertSameShape(Tensor a, Tensor b, string opName)
    {
        if (!a.Shape.Dimensions.SequenceEqual(b.Shape.Dimensions))
            throw new ArgumentException(
                $"Операция {opName} требует тензоры одинаковой формы. " +
                $"Получено: {a.Shape} и {b.Shape}");
    }

    // Перевести плоский индекс в многомерный массив индексов
    private static int[] FlatToIndices(int flatIndex, TensorShape shape)
    {
        var indices = new int[shape.Rank];
        int remaining = flatIndex;
        for (int i = shape.Rank - 1; i >= 0; i--)
        {
            indices[i] = remaining % shape.Dimensions[i];
            remaining  /= shape.Dimensions[i];
        }
        return indices;
    }

    // Суммирование по оси — вспомогательный метод для Mean
    private static void SumAlongAxis(
        float[] srcData, TensorShape srcShape,
        float[] dstData, TensorShape dstShape,
        int axis)
    {
        // Для каждого элемента результата суммируем соответствующие исходные элементы
        for (int dstFlat = 0; dstFlat < dstShape.TotalElements; dstFlat++)
        {
            var dstIndices = FlatToIndices(dstFlat, dstShape);

            float sum = 0f;
            int axisSize = srcShape.Dimensions[axis];

            for (int k = 0; k < axisSize; k++)
            {
                var srcIndices = dstIndices.ToArray();
                if (srcShape.Rank == dstShape.Rank)
                    srcIndices[axis] = k;           // keepDims=true
                else
                    srcIndices = InsertAt(srcIndices, axis, k); // keepDims=false

                sum += srcData[srcShape.FlatIndex(srcIndices)];
            }

            dstData[dstFlat] = sum;
        }
    }

    // Распределить градиент по оси — вспомогательный метод для backward Mean
    private static void BroadcastGradAlongAxis(
        float[] gradOut, TensorShape gradOutShape,
        float[] gradIn, TensorShape gradInShape,
        int axis, float scale)
    {
        for (int srcFlat = 0; srcFlat < gradInShape.TotalElements; srcFlat++)
        {
            var srcIndices = FlatToIndices(srcFlat, gradInShape);
            var dstIndices = srcIndices.ToArray();
            dstIndices[axis] = 0;  // проецируем на ось которую усреднили

            gradIn[srcFlat] = gradOut[gradOutShape.FlatIndex(dstIndices)] * scale;
        }
    }

    private static int[] InsertAt(int[] arr, int pos, int val)
    {
        var result = new int[arr.Length + 1];
        Array.Copy(arr, 0, result, 0, pos);
        result[pos] = val;
        Array.Copy(arr, pos, result, pos + 1, arr.Length - pos);
        return result;
    }
}
```

---

## Фаза 3 — Линейный слой, Parameter, Module (2 дня)

```csharp
// Layers/Parameter.cs
// Обучаемый параметр нейросети — тензор для которого всегда вычисляется градиент
// Аналог torch.nn.Parameter
public sealed class Parameter
{
    // Значения параметра (веса нейронной сети)
    public Tensor Value { get; }

    // Конструктор — всегда устанавливает RequiresGrad = true
    // Параметр по определению требует вычисления градиента чтобы обучаться
    public Parameter(Tensor initialValue)
    {
        Value = initialValue;
        Value.RequiresGrad = true;  // параметры всегда обучаемые
    }

    // Градиент параметра — заполняется при backward()
    public Tensor? Grad => Value.Grad;

    // Обнулить градиент перед новой итерацией обучения
    public void ZeroGrad() => Value.ZeroGrad();
}

// Layers/Module.cs
// Базовый класс для всех слоёв нейронной сети
// Аналог torch.nn.Module
// Хранит коллекцию параметров и дочерних слоёв
public abstract class Module
{
    // Все параметры этого слоя — зарегистрированные через RegisterParameter
    private readonly Dictionary<string, Parameter> _parameters = new();

    // Вложенные слои (подмодули) — например трансформерный блок содержит attention и FFN
    private readonly Dictionary<string, Module> _submodules = new();

    // Зарегистрировать параметр под именем
    // Вызывается в конструкторе дочернего класса для каждого обучаемого веса
    protected void RegisterParameter(string name, Parameter parameter)
    {
        _parameters[name] = parameter;
    }

    // Зарегистрировать дочерний слой
    protected void RegisterModule(string name, Module module)
    {
        _submodules[name] = module;
    }

    // Получить все параметры этого слоя и всех дочерних слоёв рекурсивно
    // Используется оптимизатором чтобы знать что обновлять
    public IEnumerable<Parameter> Parameters()
    {
        foreach (var p in _parameters.Values)
            yield return p;

        foreach (var sub in _submodules.Values)
        foreach (var p in sub.Parameters())
            yield return p;
    }

    // Обнулить градиенты всех параметров
    // Вызывается в начале каждой итерации обучения
    public void ZeroGrad()
    {
        foreach (var p in Parameters())
            p.ZeroGrad();
    }
}

// Layers/Linear.cs
// Линейный слой нейронной сети
// Вычисляет: output = input @ weight^T + bias
// Аналог torch.nn.Linear
// Источник для понимания: nanoGPT model.py — все Linear слои работают так же
public class Linear : Module
{
    // Матрица весов: [выходные_нейроны, входные_нейроны]
    // Транспонируется при умножении чтобы получить output = input @ weight^T
    public Parameter Weight { get; }

    // Вектор смещения: [выходные_нейроны]
    // Добавляется к каждой строке результата
    // null если слой создан без смещения (bias=false)
    public Parameter? Bias { get; }

    // Количество входных нейронов (размер входного вектора)
    public int InFeatures { get; }

    // Количество выходных нейронов (размер выходного вектора)
    public int OutFeatures { get; }

    public Linear(int inFeatures, int outFeatures, bool useBias = true)
    {
        InFeatures  = inFeatures;
        OutFeatures = outFeatures;

        // Инициализируем веса методом Kaiming (He initialization)
        // Это стандартная инициализация для нейросетей — обеспечивает
        // правильный масштаб активаций в глубоких сетях
        var weightData = Initializers.KaimingUniform(outFeatures, inFeatures);
        Weight = new Parameter(new Tensor(weightData, new TensorShape(outFeatures, inFeatures)));
        RegisterParameter("weight", Weight);

        if (useBias)
        {
            var biasData = new float[outFeatures];  // инициализируем нулями
            Bias = new Parameter(new Tensor(biasData, new TensorShape(outFeatures)));
            RegisterParameter("bias", Bias);
        }
    }

    // Прямой проход: вычисляем output = input @ weight^T + bias
    // input может быть любой формы [..., in_features]
    // output будет [..., out_features]
    public Tensor Forward(Tensor input)
    {
        // Транспонируем матрицу весов: [out, in] → [in, out]
        // чтобы матмул input[..., in] @ weight^T[in, out] давал [.., out]
        var weightT = TensorOps.Transpose(Weight.Value, 0, 1);

        // Матричное умножение: вход × веса
        var output = TensorOps.MatMul(input, weightT);

        // Добавляем смещение если оно есть
        if (Bias != null)
        {
            // Нужно расширить bias с [out] до [..., out] для broadcast
            var biasExpanded = ExpandBias(Bias.Value, output.Shape);
            output = TensorOps.Add(output, biasExpanded);
        }

        return output;
    }

    // Расширить вектор смещения до формы совместимой с output
    // [out_features] → [1, 1, ..., out_features] для broadcast сложения
    private static Tensor ExpandBias(Tensor bias, TensorShape outputShape)
    {
        // Для простоты пока реализуем только для 2D и 3D выходов
        if (outputShape.Rank == 2)
        {
            // Повторяем bias по каждой строке batch
            var expandedData = new float[outputShape.TotalElements];
            int batchSize    = outputShape.Dimensions[0];
            int outSize      = outputShape.Dimensions[1];

            for (int b = 0; b < batchSize; b++)
            for (int o = 0; o < outSize; o++)
                expandedData[b * outSize + o] = bias.Data[o];

            return new Tensor(expandedData, outputShape);
        }

        // Для 3D: [batch, seq, out]
        if (outputShape.Rank == 3)
        {
            var expandedData = new float[outputShape.TotalElements];
            int batch = outputShape.Dimensions[0];
            int seq   = outputShape.Dimensions[1];
            int outSz = outputShape.Dimensions[2];

            for (int b = 0; b < batch; b++)
            for (int s = 0; s < seq; s++)
            for (int o = 0; o < outSz; o++)
                expandedData[b * seq * outSz + s * outSz + o] = bias.Data[o];

            return new Tensor(expandedData, outputShape);
        }

        throw new NotImplementedException(
            $"ExpandBias для ранга {outputShape.Rank} пока не реализован");
    }
}
```

---

## Фаза 4 — Инициализаторы весов (0.5 дня)

```csharp
// Init/Initializers.cs
// Алгоритмы инициализации весов нейронной сети
// Правильная инициализация критична — неправильная приводит к затуханию или взрыву градиентов
public static class Initializers
{
    private static readonly Random Rng = new(42);  // фиксированный seed для воспроизводимости

    // Kaiming Uniform инициализация (He initialization)
    // Разработана для сетей с ReLU активацией
    // Диапазон: [-sqrt(6/fan_in), +sqrt(6/fan_in)]
    // fan_in — количество входных нейронов
    // Источник: оригинальная статья He et al. 2015
    public static float[] KaimingUniform(int rows, int cols)
    {
        var data  = new float[rows * cols];
        float fanIn  = cols;  // количество входных связей
        float bound  = MathF.Sqrt(6.0f / fanIn);  // граница диапазона

        for (int i = 0; i < data.Length; i++)
            data[i] = (float)(Rng.NextDouble() * 2 * bound - bound);  // равномерно в [-bound, +bound]

        return data;
    }

    // Нормальная инициализация — веса из нормального распределения
    // mean = 0, std = заданное стандартное отклонение
    // Используется в LoRA для матрицы A
    public static float[] Normal(int rows, int cols, float std = 0.02f)
    {
        var data = new float[rows * cols];

        for (int i = 0; i < data.Length; i += 2)
        {
            // Box-Muller преобразование: два равномерных числа → два нормальных
            double u1 = 1.0 - Rng.NextDouble();  // не берём 0 чтобы избежать log(0)
            double u2 = 1.0 - Rng.NextDouble();
            double r  = Math.Sqrt(-2.0 * Math.Log(u1));
            double theta = 2.0 * Math.PI * u2;

            data[i]     = (float)(r * Math.Cos(theta) * std);
            if (i + 1 < data.Length)
                data[i + 1] = (float)(r * Math.Sin(theta) * std);
        }

        return data;
    }

    // Нулевая инициализация — все веса = 0
    // Используется в LoRA для матрицы B (чтобы адаптер начинал как нулевое добавление)
    public static float[] Zeros(int rows, int cols)
    {
        return new float[rows * cols];  // new float[] заполняется нулями автоматически
    }
}
```

---

## Фаза 5 — AdamW без TorchSharp (1 день)

Оптимизатор теперь работает с нашим `Parameter` вместо TorchSharp `Parameter`.

```csharp
// В проекте 04.TrainingLoop:

// Optimizers/AdamW.cs (переписанная версия без TorchSharp)
public class AdamW
{
    private readonly float _lr;
    private readonly float _beta1;
    private readonly float _beta2;
    private readonly float _weightDecay;
    private readonly float _eps;

    private int _step;
    // Состояние для каждого параметра: (первый момент m, второй момент v)
    // Используем object как ключ чтобы идентифицировать параметры по ссылке
    private readonly Dictionary<Parameter, (float[] m, float[] v)> _state = new(
        ReferenceEqualityComparer.Instance
    );

    public AdamW(float lr = 1e-3f, float beta1 = 0.9f, float beta2 = 0.95f,
                 float weightDecay = 0.1f, float eps = 1e-8f)
    {
        _lr          = lr;
        _beta1       = beta1;
        _beta2       = beta2;
        _weightDecay = weightDecay;
        _eps         = eps;
    }

    // Один шаг обновления весов
    // Вызывается после backward() когда градиенты уже вычислены
    public void Step(IEnumerable<Parameter> parameters)
    {
        _step++;

        foreach (var param in parameters)
        {
            // Пропускаем параметры без градиента (замороженные)
            if (param.Grad == null) continue;

            int n = param.Value.Data.Length;

            // Создаём состояние для нового параметра при первом обращении
            if (!_state.TryGetValue(param, out var state))
            {
                state = (new float[n], new float[n]);
                _state[param] = state;
            }

            var (m, v) = state;
            var weights = param.Value.Data;
            var grad    = param.Grad.Data;

            // Поправка смещения (bias correction) — компенсирует занижение первых шагов
            float bc1 = 1f - MathF.Pow(_beta1, _step);  // поправка для первого момента
            float bc2 = 1f - MathF.Pow(_beta2, _step);  // поправка для второго момента

            for (int i = 0; i < n; i++)
            {
                // Обновляем экспоненциальное скользящее среднее градиента (первый момент)
                m[i] = _beta1 * m[i] + (1f - _beta1) * grad[i];

                // Обновляем экспоненциальное скользящее среднее квадрата градиента (второй момент)
                v[i] = _beta2 * v[i] + (1f - _beta2) * grad[i] * grad[i];

                // Применяем поправку смещения
                float mHat = m[i] / bc1;  // скорректированный первый момент
                float vHat = v[i] / bc2;  // скорректированный второй момент

                // Weight decay: уменьшаем вес пропорционально его значению
                // Реализовано как прямое уменьшение (decoupled от градиентного шага)
                weights[i] *= 1f - _lr * _weightDecay;

                // Обновление веса: движемся против градиента
                weights[i] -= _lr * mHat / (MathF.Sqrt(vHat) + _eps);
            }
        }
    }
}
```

---

## Обновлённые зависимости после добавления TensorCore

```
00 TensorCore      — независим, только чистый C#
    ↓
03 TransformerModel (заменить TorchSharp на TensorCore)
04 TrainingLoop     (заменить TorchSharp на TensorCore)
05 Embeddings       (заменить TorchSharp на TensorCore)
09 Inference        (заменить TorchSharp на TensorCore)
10 LoRA             (заменить TorchSharp на TensorCore)
11 DPO              (заменить TorchSharp на TensorCore)
```

Проекты 01, 02, 06, 07, 08, 12 не используют TorchSharp — их не нужно трогать.

---

## Тесты которые нужно написать для Gate B

Gate B (алгоритмическая корректность) — проверяем что наши результаты совпадают с Python/PyTorch:

```python
# reference_outputs.py — запустить один раз на Python чтобы получить эталонные значения
import torch

# Тест 1: MatMul
a = torch.tensor([[1.0, 2.0], [3.0, 4.0]])
b = torch.tensor([[5.0, 6.0], [7.0, 8.0]])
print("MatMul result:", (a @ b).tolist())
# Ожидается: [[19, 22], [43, 50]]

# Тест 2: Softmax
x = torch.tensor([[1.0, 2.0, 3.0]])
print("Softmax result:", torch.softmax(x, dim=-1).tolist())
# Ожидается: [[0.0900, 0.2447, 0.6652]]

# Тест 3: backward через MatMul
a = torch.tensor([[1.0, 2.0], [3.0, 4.0]], requires_grad=True)
b = torch.tensor([[1.0], [1.0]])
out = (a @ b).sum()
out.backward()
print("grad a:", a.grad.tolist())
# Ожидается: [[1, 1], [1, 1]]
```

В тестах C# проверяем что наши результаты совпадают с этими значениями с точностью до 0.001.

---

## Критерии приёмки

1. **Gate A (автоматический):** `dotnet build` без ошибок и предупреждений, все тесты зелёные
2. **Gate B (корректность):** MatMul, Softmax, backward через цепочку операций совпадают с PyTorch до 0.001
3. **Gate C (одобрение):** явное "да" от Лубы

После Gate C — проекты 03, 04, 09, 10, 11 переписываются: убираем `using TorchSharp`, добавляем `using TensorCore`. Интерфейсы не меняются — только источник тензорных операций.

---

## Оценка времени

| Фаза | Содержание | Дней |
|---|---|---|
| 1 | TensorShape, TensorStorage | 1 |
| 2 | Tensor + все операции + autograd | 4–5 |
| 3 | Parameter, Module, Linear | 2 |
| 4 | Инициализаторы | 0.5 |
| 5 | AdamW без TorchSharp | 1 |
| 6 | Перевод проектов 03,04,09,10,11 | 3–4 |
| **Итого** | | **11–13 дней** |

---

## Обновлённая общая таблица

| Подпроект | Название | Зависимости | Дней |
|---|---|---|---|
| **00** | **TensorCore (заменяет TorchSharp)** | **нет** | **12** |
| 01–12 | (существующие) | (см. основной план) | 55 |
| 13 | Code Optimizer | нет | 20 |
| **Итого** | | | **87 дней** |

---

*Версия плана 1.0 — подпроект 00, не касается XIIGen*
