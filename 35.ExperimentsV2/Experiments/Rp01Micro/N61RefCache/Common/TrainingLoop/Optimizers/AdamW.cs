// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

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

    public float Beta1 => _beta1;

    public float Beta2 => _beta2;

    public float WeightDecay => _weightDecay;

    public float Epsilon => _eps;

    // Состояние для каждого параметра: первый и второй момент (по ссылке)
    private Dictionary<Parameter, (float[] m, float[] v)> _state = new(
        ReferenceEqualityComparer.Instance
    );

    public AdamW(float beta1 = 0.9f, float beta2 = 0.95f,
                 float weightDecay = 0.1f, float eps = 1e-8f)
    {
        if (!float.IsFinite(beta1) || beta1 < 0f || beta1 >= 1f)
            throw new ArgumentOutOfRangeException(nameof(beta1));
        if (!float.IsFinite(beta2) || beta2 < 0f || beta2 >= 1f)
            throw new ArgumentOutOfRangeException(nameof(beta2));
        if (!float.IsFinite(weightDecay) || weightDecay < 0f)
            throw new ArgumentOutOfRangeException(nameof(weightDecay));
        if (!float.IsFinite(eps) || eps <= 0f)
            throw new ArgumentOutOfRangeException(nameof(eps));

        _beta1 = beta1;
        _beta2 = beta2;
        _weightDecay = weightDecay;
        _eps = eps;
    }

    // Шаг с явным lr от планировщика
    public void Step(IEnumerable<Parameter> parameters, float lr)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        if (!float.IsFinite(lr) || lr < 0f)
            throw new ArgumentOutOfRangeException(nameof(lr));

        var parameterList = parameters as IReadOnlyList<Parameter>
            ?? parameters.ToArray();
        if (parameterList.Any(parameter => parameter is null))
        {
            throw new ArgumentException(
                "The parameter sequence cannot contain null values.",
                nameof(parameters));
        }

        _step++;

        float bc1 = 1f - MathF.Pow(_beta1, _step); // поправка первого момента
        float bc2 = 1f - MathF.Pow(_beta2, _step); // поправка второго момента

        foreach (var param in parameterList)
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

    // Снимок нужен вызывающим транзакциям: они могут вернуть оптимизатор к
    // предыдущему численному состоянию, если внешний шаг записи результата сорвался.
    public AdamWStateSnapshot CaptureState()
    {
        var entries = new List<AdamWParameterStateSnapshot>(_state.Count);
        foreach (var (parameter, state) in _state)
        {
            ValidateStateEntry(parameter, state.m, state.v);
            entries.Add(new AdamWParameterStateSnapshot(
                parameter,
                state.m.ToArray(),
                state.v.ToArray()));
        }

        return new AdamWStateSnapshot(this, _step, entries);
    }

    // Проверка отделена от применения: DpoTrainer сначала проверяет совместимость
    // и только потом начинает менять веса модели.
    public void ValidateStateSnapshot(AdamWStateSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (!ReferenceEquals(snapshot.Owner, this))
            throw new InvalidOperationException("ADAMW_SNAPSHOT_OWNER_MISMATCH");
        if (snapshot.Step < 0)
            throw new InvalidOperationException("ADAMW_SNAPSHOT_STEP_INVALID");

        var seen = new HashSet<Parameter>(ReferenceEqualityComparer.Instance);
        foreach (var entry in snapshot.Entries)
        {
            if (!seen.Add(entry.Parameter))
                throw new InvalidOperationException("ADAMW_SNAPSHOT_PARAMETER_DUPLICATE");
            ValidateStateEntry(entry.Parameter, entry.FirstMoment, entry.SecondMoment);
        }
    }

    // Готовит все новые массивы и словарь заранее. Само применение подготовленного
    // состояния затем состоит только из замены ссылок внутри текущего оптимизатора.
    public AdamWPreparedState PrepareRestore(AdamWStateSnapshot snapshot)
    {
        ValidateStateSnapshot(snapshot);

        var restored = new Dictionary<Parameter, (float[] m, float[] v)>(
            ReferenceEqualityComparer.Instance);
        foreach (var entry in snapshot.Entries)
        {
            restored.Add(
                entry.Parameter,
                (entry.FirstMoment.ToArray(), entry.SecondMoment.ToArray()));
        }

        return new AdamWPreparedState(this, snapshot.Step, restored);
    }

    // Удобный путь для обычного восстановления: сначала готовит отдельные копии
    // состояния, затем применяет их к тому же экземпляру оптимизатора.
    public void RestoreState(AdamWStateSnapshot snapshot)
        => RestorePreparedState(PrepareRestore(snapshot));

    // Применяет уже подготовленное состояние без новых копий массивов. Это нужно,
    // чтобы откат шага DPO не выделял память после начала восстановления весов.
    public void RestorePreparedState(AdamWPreparedState preparedState)
    {
        ArgumentNullException.ThrowIfNull(preparedState);
        var restored = preparedState.Consume(this);
        _state = restored.State;
        _step = restored.Step;
    }

    // Выгружает накопленные оценки градиентов и счётчик шага в переносимый вид,
    // привязанный к стабильным именам параметров из контрольной точки модели.
    // Это нужно, чтобы состояние оптимизатора можно было сохранить рядом с
    // весами и открыть заново после перезапуска процесса, а не начинать урок с
    // обнулённой памятью оптимизатора.
    //
    // Параметры без накопленного состояния пропускаются: до первого шага по
    // такому параметру у оптимизатора нет оценок, и придумывать нули нельзя.
    public AdamWPortableState ExportPortableState(
        IReadOnlyList<AdamWParameterBinding> bindings)
    {
        ArgumentNullException.ThrowIfNull(bindings);

        var seenKeys = new HashSet<string>(StringComparer.Ordinal);
        var seenParameters = new HashSet<Parameter>(ReferenceEqualityComparer.Instance);
        var entries = new List<AdamWPortableParameterState>(bindings.Count);

        foreach (var binding in bindings)
        {
            ArgumentNullException.ThrowIfNull(binding);
            ArgumentNullException.ThrowIfNull(binding.Parameter);
            if (string.IsNullOrEmpty(binding.Key))
                throw new InvalidOperationException("ADAMW_PORTABLE_BINDING_KEY_EMPTY");
            if (!seenKeys.Add(binding.Key))
                throw new InvalidOperationException("ADAMW_PORTABLE_BINDING_KEY_DUPLICATE");
            if (!seenParameters.Add(binding.Parameter))
                throw new InvalidOperationException("ADAMW_PORTABLE_BINDING_PARAMETER_DUPLICATE");

            if (!_state.TryGetValue(binding.Parameter, out var state))
                continue;

            ValidateStateEntry(binding.Parameter, state.m, state.v);
            if (!AdamWPortableParameterState.HasValidShape(
                    binding.Parameter.Value.Shape.Dimensions, state.m.Length) ||
                binding.Parameter.Value.Shape.TotalElements != state.m.Length)
                throw new InvalidOperationException("ADAMW_PORTABLE_SHAPE_INVALID");
            entries.Add(new AdamWPortableParameterState(
                binding.Key,
                binding.Parameter.Value.Shape.Dimensions,
                state.m,
                state.v));
        }

        return new AdamWPortableState(
            AdamWPortableState.CurrentFormatVersion,
            _step,
            entries);
    }

    // Переносит прочитанное состояние в этот оптимизатор. Возвращает false и не
    // меняет ничего, если имена не разрешаются в текущей модели, формы не
    // совпадают или связки заданы неоднозначно. Отказ обязателен: молчаливый
    // возврат к свежему оптимизатору означал бы продолжение урока с другой
    // траекторией обучения, чем до перезапуска.
    public bool TryImportPortableState(
        IReadOnlyList<AdamWParameterBinding> bindings,
        AdamWPortableState portableState)
    {
        ArgumentNullException.ThrowIfNull(bindings);
        ArgumentNullException.ThrowIfNull(portableState);

        if (portableState.FormatVersion != AdamWPortableState.CurrentFormatVersion ||
            portableState.Step < 0)
        {
            return false;
        }

        var bindingByKey = new Dictionary<string, Parameter>(StringComparer.Ordinal);
        var seenParameters = new HashSet<Parameter>(ReferenceEqualityComparer.Instance);
        foreach (var binding in bindings)
        {
            if (binding is null ||
                binding.Parameter is null ||
                string.IsNullOrEmpty(binding.Key) ||
                !bindingByKey.TryAdd(binding.Key, binding.Parameter) ||
                !seenParameters.Add(binding.Parameter))
            {
                return false;
            }
        }

        var restored = new Dictionary<Parameter, (float[] m, float[] v)>(
            ReferenceEqualityComparer.Instance);
        foreach (var entry in portableState.Entries)
        {
            if (!bindingByKey.TryGetValue(entry.Key, out var parameter))
                return false;

            var parameterLength = parameter.Value.Data.Length;
            if (entry.FirstMoment.Count != parameterLength ||
                entry.SecondMoment.Count != parameterLength ||
                parameter.Value.Shape.TotalElements != parameterLength ||
                !AdamWPortableParameterState.HasValidShape(
                    parameter.Value.Shape.Dimensions, parameterLength) ||
                !entry.Dimensions.SequenceEqual(parameter.Value.Shape.Dimensions))
            {
                return false;
            }

            if (!restored.TryAdd(
                    parameter,
                    (entry.FirstMoment.ToArray(), entry.SecondMoment.ToArray())))
            {
                return false;
            }
        }

        // Замена делается только после того, как каждая запись проверена.
        _state = restored;
        _step = portableState.Step;
        return true;
    }

    private static void ValidateStateEntry(
        Parameter parameter,
        float[] firstMoment,
        float[] secondMoment)
    {
        ArgumentNullException.ThrowIfNull(parameter);
        ArgumentNullException.ThrowIfNull(firstMoment);
        ArgumentNullException.ThrowIfNull(secondMoment);
        var parameterLength = parameter.Value.Data.Length;
        if (firstMoment.Length != parameterLength ||
            secondMoment.Length != parameterLength)
        {
            throw new InvalidOperationException("ADAMW_SNAPSHOT_SHAPE_MISMATCH");
        }
    }
}
