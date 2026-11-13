// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;
using TensorCore.Layers;
using TrainingLoop.Optimizers;
using TransformerModel;

namespace DPO;

// Живой снимок одного шага обучения DPO перед изменением обучаемой модели. Он
// нужен только для повтора шага в этом процессе и не является восстановлением
// после перезапуска или новым файловым протоколом.
internal sealed class DpoTrainerStepSnapshot
{
    private readonly LlmModel _policy;
    private readonly AdamW _optimizer;
    private readonly ParameterState[] _parameters;
    private readonly AdamWStateSnapshot _optimizerState;
    private readonly LlmModelRandomStateSnapshot? _randomState;
    private readonly Action<LlmModelRandomStateSnapshot>? _restoreRandomState;

    private DpoTrainerStepSnapshot(
        LlmModel policy,
        AdamW optimizer,
        ParameterState[] parameters,
        AdamWStateSnapshot optimizerState,
        LlmModelRandomStateSnapshot? randomState,
        Action<LlmModelRandomStateSnapshot>? restoreRandomState)
    {
        _policy = policy;
        _optimizer = optimizer;
        _parameters = parameters;
        _optimizerState = optimizerState;
        _randomState = randomState;
        _restoreRandomState = restoreRandomState;
    }

    internal static DpoTrainerStepSnapshot Capture(
        LlmModel policy,
        AdamW optimizer)
        => Capture(policy, optimizer, null, null);

    internal static DpoTrainerStepSnapshot Capture(
        LlmModel policy,
        AdamW optimizer,
        LlmModelRandomStateSnapshot? randomState,
        Action<LlmModelRandomStateSnapshot>? restoreRandomState)
    {
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(optimizer);

        var checkpointEntries = policy.GetCheckpointParameterEntries();
        var parameters = new ParameterState[checkpointEntries.Count];
        for (var index = 0; index < checkpointEntries.Count; index++)
        {
            var entry = checkpointEntries[index];
            parameters[index] = ParameterState.Capture(entry.Key, entry.Parameter);
        }

        return new DpoTrainerStepSnapshot(
            policy,
            optimizer,
            parameters,
            optimizer.CaptureState(),
            randomState,
            restoreRandomState);
    }

    // Откат восстанавливает живое состояние процесса, включая cursor
    // детерминированных adapter-потоков до восстановления памяти AdamW.
    internal void Restore()
    {
        var current = _policy.GetCheckpointParameterEntries();
        ValidateCurrentParameters(current);

        var restoredGradients = new Tensor?[_parameters.Length];
        for (var index = 0; index < _parameters.Length; index++)
            restoredGradients[index] = _parameters[index].BuildRestoredGradient();
        var optimizerRestore = _optimizer.PrepareRestore(_optimizerState);

        for (var index = 0; index < _parameters.Length; index++)
        {
            var snapshot = _parameters[index];
            var value = snapshot.Parameter.Value;
            Array.Copy(snapshot.Values, value.Data, snapshot.Values.Length);
            value.RequiresGrad = snapshot.RequiresGrad;
            value.Grad = restoredGradients[index];
        }

        if (_randomState is not null)
        {
            if (_restoreRandomState is null)
                throw new InvalidOperationException("DPO_RANDOM_STATE_RESTORE_SEAM_MISSING");
            _restoreRandomState(_randomState);
        }

        _optimizer.RestorePreparedState(optimizerRestore);
    }

    private void ValidateCurrentParameters(
        IReadOnlyList<LlmCheckpointParameterEntry> current)
    {
        if (current.Count != _parameters.Length)
            throw new InvalidOperationException("DPO_STEP_SNAPSHOT_PARAMETER_COUNT_MISMATCH");

        var seen = new HashSet<Parameter>(ReferenceEqualityComparer.Instance);
        for (var index = 0; index < _parameters.Length; index++)
        {
            var expected = _parameters[index];
            var observed = current[index];
            if (!string.Equals(expected.Key, observed.Key, StringComparison.Ordinal))
                throw new InvalidOperationException("DPO_STEP_SNAPSHOT_PARAMETER_KEY_MISMATCH");
            if (!ReferenceEquals(expected.Parameter, observed.Parameter))
                throw new InvalidOperationException("DPO_STEP_SNAPSHOT_PARAMETER_REFERENCE_MISMATCH");
            if (!seen.Add(observed.Parameter))
                throw new InvalidOperationException("DPO_STEP_SNAPSHOT_PARAMETER_DUPLICATE");
            expected.ValidateCurrentShape();
        }
    }

    private sealed class ParameterState
    {
        private ParameterState(
            string key,
            Parameter parameter,
            int[] dimensions,
            float[] values,
            bool requiresGrad,
            GradientState? gradient)
        {
            Key = key;
            Parameter = parameter;
            Dimensions = dimensions;
            Values = values;
            RequiresGrad = requiresGrad;
            Gradient = gradient;
        }

        internal string Key { get; }

        internal Parameter Parameter { get; }

        internal int[] Dimensions { get; }

        internal float[] Values { get; }

        internal bool RequiresGrad { get; }

        internal GradientState? Gradient { get; }

        internal static ParameterState Capture(string key, Parameter parameter)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new InvalidOperationException("DPO_STEP_SNAPSHOT_PARAMETER_KEY_INVALID");
            ArgumentNullException.ThrowIfNull(parameter);

            var value = parameter.Value;
            var gradient = value.Grad is null
                ? null
                : GradientState.Capture(value.Grad, value.Shape);
            return new ParameterState(
                key,
                parameter,
                value.Shape.Dimensions.ToArray(),
                value.Data.ToArray(),
                value.RequiresGrad,
                gradient);
        }

        internal void ValidateCurrentShape()
        {
            var value = Parameter.Value;
            if (!value.Shape.Dimensions.SequenceEqual(Dimensions) ||
                value.Data.Length != Values.Length)
            {
                throw new InvalidOperationException("DPO_STEP_SNAPSHOT_PARAMETER_SHAPE_MISMATCH");
            }
        }

        internal Tensor? BuildRestoredGradient()
        {
            if (Gradient is null)
                return null;
            return Gradient.BuildTensor();
        }
    }

    private sealed class GradientState
    {
        private GradientState(int[] dimensions, float[] values, bool requiresGrad)
        {
            Dimensions = dimensions;
            Values = values;
            RequiresGrad = requiresGrad;
        }

        private int[] Dimensions { get; }

        private float[] Values { get; }

        private bool RequiresGrad { get; }

        internal static GradientState Capture(Tensor gradient, TensorShape valueShape)
        {
            if (!gradient.Shape.Dimensions.SequenceEqual(valueShape.Dimensions))
                throw new InvalidOperationException("DPO_STEP_SNAPSHOT_GRADIENT_SHAPE_MISMATCH");

            return new GradientState(
                gradient.Shape.Dimensions.ToArray(),
                gradient.Data.ToArray(),
                gradient.RequiresGrad);
        }

        internal Tensor BuildTensor()
            => new(Values.ToArray(), new TensorShape(Dimensions), RequiresGrad);
    }
}
