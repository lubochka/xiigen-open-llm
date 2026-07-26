// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;

/// <summary>
/// Executes one exact registry-resolved L2 logical-unit producer and couples its physical
/// provenance to untrained-honesty evidence. The probe does not associate a measurement
/// target or decide final PASS, trained state, lifecycle state, or promotion readiness.
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
        if (!IsCanonicalIdentity(evaluationId)
            || resolvedProducer?.Descriptor is null
            || resolvedProducer.Unit is null
            || request is null
            || runtimeNotReadyPredicate is null
            || supportingBoundaryTypes is null)
        {
            return InvalidProbe();
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
            return InvalidProbe();
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
                return InvalidProbe();
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
                    return InvalidProbe();
                }

                var supportingTypeName = supportingType.FullName;
                if (!IsCanonicalIdentity(supportingTypeName)
                    || !typeNames.Add(supportingTypeName!))
                {
                    return InvalidProbe();
                }
            }
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return InvalidProbe();
        }

        OperationResult<LogicalUnitResult<TOutput>> execution;
        try
        {
            execution = await resolvedProducer.Unit
                .ExecuteAsync(request, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return InvalidProbe();
        }

        if (execution is null)
            return InvalidProbe();

        if (!execution.IsSuccess)
            return execution.MapError<DynamicGraphUntrainedHonestyL2ProbeResult>();

        var logicalResult = execution.Value;
        if (logicalResult is null
            || !StringComparer.Ordinal.Equals(logicalResult.UnitId, descriptor.UnitId)
            || !StringComparer.Ordinal.Equals(logicalResult.UnitRole, descriptor.UnitRole)
            || !StringComparer.Ordinal.Equals(logicalResult.DomainTag, descriptor.DomainTag))
        {
            return InvalidProbe();
        }

        bool runtimeReportedNotReady;
        try
        {
            runtimeReportedNotReady = runtimeNotReadyPredicate(logicalResult);
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return InvalidProbe();
        }

        var inspectedTypes = new Type[supportingTypes.Length + 1];
        inspectedTypes[0] = producerType;
        Array.Copy(supportingTypes, 0, inspectedTypes, 1, supportingTypes.Length);

        var honesty = DynamicGraphUntrainedHonestyMeter.Measure(
            new DynamicGraphUntrainedHonestyObservation(
                evaluationId,
                runtimeReportedNotReady,
                logicalResult.EvidenceIds,
                inspectedTypes));

        if (!honesty.IsSuccess)
            return honesty.MapError<DynamicGraphUntrainedHonestyL2ProbeResult>();

        if (honesty.Value is null)
            return InvalidProbe();

        return OperationResult<DynamicGraphUntrainedHonestyL2ProbeResult>.Ok(
            new DynamicGraphUntrainedHonestyL2ProbeResult(
                new DynamicGraphUntrainedHonestyL2ProducerEvidence(
                    descriptor.UnitId,
                    descriptor.UnitRole,
                    descriptor.DomainTag,
                    descriptor.Version,
                    descriptor.RegistrationOrder,
                    producerTypeName,
                    inputTypeName,
                    outputTypeName),
                honesty.Value));
    }

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

    private static OperationResult<DynamicGraphUntrainedHonestyL2ProbeResult> InvalidProbe()
        => OperationResult<DynamicGraphUntrainedHonestyL2ProbeResult>.Fail(
            InvalidProbeMessage,
            InvalidProbeCode);
}
