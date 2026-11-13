using TensorCore.Core;

namespace Inference;

// Стратегии выборки следующего токена: greedy, temperature, top-k, top-p.
// DNA rule 1: все настройки именованные, никаких magic numbers.
public static class Sampler
{
    private static readonly Random Rng = new();

    // logits: [vocabSize] float тензор
    public static int Sample(Tensor logits, float temperature = 1.0f, int topK = 0, float topP = 1.0f)
    {
        // Температура ≤ 0 → greedy (argmax)
        if (temperature <= 0f)
            return TensorOps.Argmax(logits);

        // Применяем температуру: логиты делим на температуру
        var scaled = TensorOps.Scale(logits, 1f / temperature);

        Tensor probs;
        if (topK > 0)
            probs = TopKFilter(scaled, topK);
        else if (topP < 1.0f)
            probs = TopPFilter(scaled, topP);
        else
            probs = TensorOps.Softmax(scaled, dim: -1);

        return MultinomialSample(probs);
    }

    // Top-K фильтр: оставляем только k самых вероятных токенов
    private static Tensor TopKFilter(Tensor logits, int k)
    {
        int n = logits.Shape.TotalElements;
        k = Math.Min(k, n);

        // Находим k-й по убыванию элемент через частичную сортировку
        var sorted    = logits.Data.OrderByDescending(v => v).Take(k).ToArray();
        float threshold = sorted[^1]; // наименьший из top-k

        var filtered = new float[n];
        for (int i = 0; i < n; i++)
            filtered[i] = logits.Data[i] >= threshold ? logits.Data[i] : float.NegativeInfinity;

        return TensorOps.Softmax(new Tensor(filtered, logits.Shape), dim: -1);
    }

    // Top-P (nucleus) фильтр: берём токены пока суммарная вероятность < p
    private static Tensor TopPFilter(Tensor logits, float p)
    {
        int n = logits.Shape.TotalElements;

        // Отсортировать по убыванию
        var (sortedVals, sortedIdx) = TensorOps.SortDescending(logits);

        // Softmax отсортированных → cumulative sum
        var sortedProbs = TensorOps.Softmax(sortedVals, dim: -1);
        var cumProbs    = TensorOps.CumSum(sortedProbs, dim: 0);

        // Маскируем токены за пределами ядра (nucleus)
        // Сдвигаем маску вправо чтобы включить первый токен, превысивший p
        var filtered = new float[n];
        for (int i = 0; i < n; i++)
        {
            float cumPrev = i == 0 ? 0f : cumProbs.Data[i - 1];
            int   origIdx = (int)sortedIdx.Data[i];
            filtered[origIdx] = cumPrev < p ? logits.Data[origIdx] : float.NegativeInfinity;
        }

        // Хотя бы один токен должен остаться (первый — наиболее вероятный)
        int topIdx = (int)sortedIdx.Data[0];
        if (float.IsNegativeInfinity(filtered[topIdx]))
            filtered[topIdx] = logits.Data[topIdx];

        return TensorOps.Softmax(new Tensor(filtered, logits.Shape), dim: -1);
    }

    // Мультиномиальная выборка: выбираем токен пропорционально вероятностям
    private static int MultinomialSample(Tensor probs)
    {
        double r   = Rng.NextDouble();
        double acc = 0;
        for (int i = 0; i < probs.Shape.TotalElements; i++)
        {
            acc += probs.Data[i];
            if (r <= acc) return i;
        }
        return probs.Shape.TotalElements - 1;
    }
}
