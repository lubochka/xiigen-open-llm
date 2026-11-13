// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;

/// <summary>
/// Executes one exact registry-resolved L2 logical-unit producer, or admits a caller-provided
/// already-produced result, and associates producer metadata with untrained-honesty evidence.
/// The probe does not associate a measurement target or decide final PASS, trained state,
/// lifecycle state, or promotion readiness.
/// </summary>
public static class DynamicGraphUntrainedHonestyL2Probe
{
    private const string InvalidProbeCode = "INVALID_UNTRAINED_HONESTY_L2_PROBE";
    private const string InvalidProbeMessage = "Dynamic graph L2 untrained honesty probe is invalid.";

    /// <summary>
    /// Executes the resolved producer exactly once and measures only its returned runtime
    /// evidence plus the concrete producer and supporting boundary types.
    /// </summary>
    public static async Task<OperationResult<DynamicGraphUntrainedHonestyL2ProbeResult>> MeasureAsync<TInput, TOutput>(
        string evaluationId,
        ResolvedLogicalUnitRegistryEntry<TInput, TOutput>? resolvedProducer,
        LogicalUnitRequest<TInput>? request,
        Func<LogicalUnitResult<TOutput>, bool>? runtimeNotReadyPredicate,
        IReadOnlyList<Type>? supportingBoundaryTypes,
        CancellationToken cancellationToken = default)
    {
        var admission = Admit(
            evaluationId,
            resolvedProducer,
            request,
            runtimeNotReadyPredicate,
            supportingBoundaryTypes);
        if (!admission.IsSuccess)
            return admission.MapError<DynamicGraphUntrainedHonestyL2ProbeResult>();

        var admitted = admission.Value!;

        OperationResult<LogicalUnitResult<TOutput>> execution;
        try
        {
            execution = await admitted.Unit
                .ExecuteAsync(admitted.Request, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return InvalidProbe<DynamicGraphUntrainedHonestyL2ProbeResult>();
        }

        if (execution is null)
            return InvalidProbe<DynamicGraphUntrainedHonestyL2ProbeResult>();

        if (!execution.IsSuccess)
            return execution.MapError<DynamicGraphUntrainedHonestyL2ProbeResult>();

        return MeasureAdmitted(admitted, execution.Value);
    }

    /// <summary>
    /// Measures a caller-provided already-produced result without invoking the resolved producer.
    /// Producer evidence records the admitted association and does not independently prove causal
    /// execution.
    /// </summary>
    public static OperationResult<DynamicGraphUntrainedHonestyL2ProbeResult>
        MeasureAlreadyExecuted<TInput, TOutput>(
            string evaluationId,
            ResolvedLogicalUnitRegistryEntry<TInput, TOutput>? resolvedProducer,
            LogicalUnitRequest<TInput>? request,
            LogicalUnitResult<TOutput>? logicalResult,
            Func<LogicalUnitResult<TOutput>, bool>? runtimeNotReadyPredicate,
            IReadOnlyList<Type>? supportingBoundaryTypes)
    {
        var admission = Admit(
            evaluationId,
            resolvedProducer,
            request,
            runtimeNotReadyPredicate,
            supportingBoundaryTypes);
        if (!admission.IsSuccess)
            return admission.MapError<DynamicGraphUntrainedHonestyL2ProbeResult>();

        return MeasureAdmitted(admission.Value!, logicalResult);
    }

    private static OperationResult<ProbeAdmission<TInput, TOutput>> Admit<TInput, TOutput>(
        string evaluationId,
        ResolvedLogicalUnitRegistryEntry<TInput, TOutput>? resolvedProducer,
        LogicalUnitRequest<TInput>? request,
        Func<LogicalUnitResult<TOutput>, bool>? runtimeNotReadyPredicate,
        IReadOnlyList<Type>? supportingBoundaryTypes)
    {
        if (!IsCanonicalIdentity(evaluationId)
            || resolvedProducer?.Descriptor is null
            || resolvedProducer.Unit is null
            || request is null
            || runtimeNotReadyPredicate is null
            || supportingBoundaryTypes is null)
        {
            return InvalidProbe<ProbeAdmission<TInput, TOutput>>();
        }

        var descriptor = resolvedProducer.Descriptor;
        if (!IsCanonicalIdentity(descriptor.UnitId)
            || !IsCanonicalIdentity(descriptor.UnitRole)
            || !IsCanonicalIdentity(descriptor.DomainTag)
            || !IsCanonicalIdentity(descriptor.Version)
            || descriptor.RegistrationOrder <= 0
            || descriptor.InputType != typeof(TInput)
            || descriptor.OutputType != typeof(TOutput)
            || !StringComparer.Ordinal.Equals(request.UnitId, descriptor.UnitId)
            || !StringComparer.Ordinal.Equals(request.UnitRole, descriptor.UnitRole)
            || !StringComparer.Ordinal.Equals(request.DomainTag, descriptor.DomainTag))
        {
            return InvalidProbe<ProbeAdmission<TInput, TOutput>>();
        }

        Type producerType;
        string producerTypeName;
        string inputTypeName;
        string outputTypeName;
        Type[] supportingTypes;
        try
        {
            producerType = resolvedProducer.Unit.GetType();
            producerTypeName = producerType.FullName!;
            inputTypeName = descriptor.InputType.FullName!;
            outputTypeName = descriptor.OutputType.FullName!;
            supportingTypes = supportingBoundaryTypes.ToArray();

            if (!IsCanonicalIdentity(producerTypeName)
                || !IsCanonicalIdentity(inputTypeName)
                || !IsCanonicalIdentity(outputTypeName))
            {
                return InvalidProbe<ProbeAdmission<TInput, TOutput>>();
            }

            var typeReferences = new HashSet<Type>(ReferenceEqualityComparer.Instance)
            {
                producerType
            };
            var typeNames = new HashSet<string>(StringComparer.Ordinal)
            {
                producerTypeName
            };

            foreach (var supportingType in supportingTypes)
            {
                if (supportingType is null
                    || !typeReferences.Add(supportingType))
                {
                    return InvalidProbe<ProbeAdmission<TInput, TOutput>>();
                }

                var supportingTypeName = supportingType.FullName;
                if (!IsCanonicalIdentity(supportingTypeName)
                    || !typeNames.Add(supportingTypeName!))
                {
                    return InvalidProbe<ProbeAdmission<TInput, TOutput>>();
                }
            }
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return InvalidProbe<ProbeAdmission<TInput, TOutput>>();
        }

        return OperationResult<ProbeAdmission<TInput, TOutput>>.Ok(
            new ProbeAdmission<TInput, TOutput>(
                evaluationId,
                descriptor,
                resolvedProducer.Unit,
                request,
                runtimeNotReadyPredicate,
                producerType,
                producerTypeName,
                inputTypeName,
                outputTypeName,
                supportingTypes));
    }

    private static OperationResult<DynamicGraphUntrainedHonestyL2ProbeResult>
        MeasureAdmitted<TInput, TOutput>(
            ProbeAdmission<TInput, TOutput> admitted,
            LogicalUnitResult<TOutput>? logicalResult)
    {
        var descriptor = admitted.Descriptor;
        if (logicalResult is null
            || !StringComparer.Ordinal.Equals(logicalResult.UnitId, descriptor.UnitId)
            || !StringComparer.Ordinal.Equals(logicalResult.UnitRole, descriptor.UnitRole)
            || !StringComparer.Ordinal.Equals(logicalResult.DomainTag, descriptor.DomainTag))
        {
            return InvalidProbe<DynamicGraphUntrainedHonestyL2ProbeResult>();
        }

        bool runtimeReportedNotReady;
        try
        {
            runtimeReportedNotReady = admitted.RuntimeNotReadyPredicate(logicalResult);
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return InvalidProbe<DynamicGraphUntrainedHonestyL2ProbeResult>();
        }

        var inspectedTypes = new Type[admitted.SupportingTypes.Length + 1];
        inspectedTypes[0] = admitted.ProducerType;
        Array.Copy(
            admitted.SupportingTypes,
            0,
            inspectedTypes,
            1,
            admitted.SupportingTypes.Length);

        var honesty = DynamicGraphUntrainedHonestyMeter.Measure(
            new DynamicGraphUntrainedHonestyObservation(
                admitted.EvaluationId,
                runtimeReportedNotReady,
                logicalResult.EvidenceIds,
                inspectedTypes));

        if (!honesty.IsSuccess)
            return honesty.MapError<DynamicGraphUntrainedHonestyL2ProbeResult>();

        if (honesty.Value is null)
            return InvalidProbe<DynamicGraphUntrainedHonestyL2ProbeResult>();

        return OperationResult<DynamicGraphUntrainedHonestyL2ProbeResult>.Ok(
            new DynamicGraphUntrainedHonestyL2ProbeResult(
                new DynamicGraphUntrainedHonestyL2ProducerEvidence(
                    descriptor.UnitId,
                    descriptor.UnitRole,
                    descriptor.DomainTag,
                    descriptor.Version,
                    descriptor.RegistrationOrder,
                    admitted.ProducerTypeName,
                    admitted.InputTypeName,
                    admitted.OutputTypeName),
                honesty.Value));
    }

    private sealed record ProbeAdmission<TInput, TOutput>(
        string EvaluationId,
        LogicalUnitDescriptor Descriptor,
        ILogicalUnit<TInput, TOutput> Unit,
        LogicalUnitRequest<TInput> Request,
        Func<LogicalUnitResult<TOutput>, bool> RuntimeNotReadyPredicate,
        Type ProducerType,
        string ProducerTypeName,
        string InputTypeName,
        string OutputTypeName,
        Type[] SupportingTypes);

    private static bool IsCanonicalIdentity(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)
            || !StringComparer.Ordinal.Equals(value, value.Trim()))
        {
            return false;
        }

        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];
            if (char.IsControl(character)
                || character is '\u0085' or '\u2028' or '\u2029')
            {
                return false;
            }

            if (char.IsHighSurrogate(character))
            {
                if (index + 1 >= value.Length
                    || !char.IsLowSurrogate(value[index + 1]))
                {
                    return false;
                }

                index++;
                continue;
            }

            if (char.IsLowSurrogate(character))
                return false;
        }

        return true;
    }

    private static OperationResult<T> InvalidProbe<T>()
        => OperationResult<T>.Fail(
            InvalidProbeMessage,
            InvalidProbeCode);
}
