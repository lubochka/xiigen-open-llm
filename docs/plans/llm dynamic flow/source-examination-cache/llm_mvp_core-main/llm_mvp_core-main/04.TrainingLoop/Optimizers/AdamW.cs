using TensorCore.Core;
using TensorCore.Layers;

namespace TrainingLoop.Optimizers;

// AdamW оптимизатор — Adaptive Moment estimation с Weight decay.
// m  = экспоненциальное скользящее среднее градиентов (направление)
// v  = экспоненциальное скользящее среднее квадратов градиентов (шум пути)
// Bias-correction предотвращает занижение первых оценок.
// Все коэффициенты из конструктора — никаких магических чисел внутри (DNA rule 1).
public class AdamW
{
    private readonly float _beta1;
    private readonly float _beta2;
    private readonly float _weightDecay;
    private readonly float _eps;

    private int _step;

    // Состояние для каждого параметра: первый и второй момент (по ссылке)
    private readonly Dictionary<Parameter, (float[] m, float[] v)> _state = new(
        ReferenceEqualityComparer.Instance
    );

    public AdamW(float lr = 1e-3f, float beta1 = 0.9f, float beta2 = 0.95f,
                 float weightDecay = 0.1f, float eps = 1e-8f)
    {
        _beta1       = beta1;
        _beta2       = beta2;
        _weightDecay = weightDecay;
        _eps         = eps;
    }

    // Шаг с явным lr от планировщика
    public void Step(IEnumerable<Parameter> parameters, float lr)
    {
        _step++;

        float bc1 = 1f - MathF.Pow(_beta1, _step); // поправка первого момента
        float bc2 = 1f - MathF.Pow(_beta2, _step); // поправка второго момента

        foreach (var param in parameters)
        {
            if (param.Grad == null) continue;

            int n = param.Value.Data.Length;

            if (!_state.TryGetValue(param, out var state))
            {
                state = (new float[n], new float[n]);
                _state[param] = state;
            }

            var (m, v) = state;
            var w = param.Value.Data;
            var g = param.Grad.Data;

            for (int i = 0; i < n; i++)
            {
                // Обновить экспоненциальные скользящие средние
                m[i] = _beta1 * m[i] + (1f - _beta1) * g[i];
                v[i] = _beta2 * v[i] + (1f - _beta2) * g[i] * g[i];

                float mHat = m[i] / bc1;                         // скорр. первый момент
                float vHat = v[i] / bc2;                         // скорр. второй момент

                // Decoupled weight decay (AdamW отличается от Adam именно этим)
                w[i] *= 1f - lr * _weightDecay;

                // Шаг против градиента
                w[i] -= lr * mHat / (MathF.Sqrt(vHat) + _eps);
            }
        }
    }
}
