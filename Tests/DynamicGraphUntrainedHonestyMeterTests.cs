// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace Tests;

using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using SmartSkillsDomain;

public sealed class DynamicGraphUntrainedHonestyMeterTests
{
    private const string InvalidCode = "INVALID_UNTRAINED_HONESTY_MEASUREMENT";
    private const string InvalidMessage = "Dynamic graph untrained honesty measurement is invalid.";
    private const string NoTrainedState = "NO_TRAINED_STATE_LOADED";
    private const string NoSemanticClassification = "NO_SEMANTIC_CLASSIFICATION_PERFORMED";

    [Fact]
    public void Contracts_ExposeExactPlannedShapes()
    {
        AssertRecordShape<DynamicGraphUntrainedHonestyObservation>(
            ("EvaluationId", typeof(string)),
            ("RuntimeReportedNotReady", typeof(bool)),
            ("RuntimeEvidence", typeof(IReadOnlyList<string>)),
            ("InspectedTypes", typeof(IReadOnlyList<Type>)));
        AssertRecordShape<DynamicGraphUntrainedHonestyMeasurement>(
            ("EvaluationId", typeof(string)),
            ("RuntimeReportedNotReady", typeof(bool)),
            ("HasNoTrainedStateEvidence", typeof(bool)),
            ("HasNoSemanticClassificationEvidence", typeof(bool)),
            ("RuntimeEvidence", typeof(IReadOnlyList<string>)),
            ("InspectedTypeNames", typeof(IReadOnlyList<string>)),
            ("ForbiddenShortcutMatches", typeof(IReadOnlyList<string>)),
            ("MeetsUntrainedHonestyGate", typeof(bool)));

        foreach (var contractType in new[]
                 {
                     typeof(DynamicGraphUntrainedHonestyObservation),
                     typeof(DynamicGraphUntrainedHonestyMeasurement)
                 })
        {
            Assert.Empty(contractType.GetFields(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly));
            Assert.All(
                contractType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                property => Assert.Contains(
                    typeof(IsExternalInit),
                    property.SetMethod!.ReturnParameter.GetRequiredCustomModifiers()));
        }
    }

    [Fact]
    public void Meter_ExposesOnlyMeasureAndNoOverclaimSurface()
    {
        var method = Assert.Single(typeof(DynamicGraphUntrainedHonestyMeter).GetMethods(
            BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));

        Assert.Equal(nameof(DynamicGraphUntrainedHonestyMeter.Measure), method.Name);
        Assert.Equal(typeof(OperationResult<DynamicGraphUntrainedHonestyMeasurement>), method.ReturnType);
        var parameter = Assert.Single(method.GetParameters());
        Assert.Equal("observation", parameter.Name);
        Assert.Equal(typeof(DynamicGraphUntrainedHonestyObservation), parameter.ParameterType);
        Assert.Empty(typeof(DynamicGraphUntrainedHonestyMeter).GetFields(
            BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
        Assert.Empty(typeof(DynamicGraphUntrainedHonestyMeter).GetProperties(
            BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));

        var surface = string.Join(
            '|',
            typeof(DynamicGraphUntrainedHonestyMeasurement)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name));
        foreach (var forbiddenOverclaim in new[]
                 {
                     "Checkpoint", "Accuracy", "Quality", "Lifecycle", "Promotion",
                     "FinalPass", "Passed", "ReadyForPromotion"
                 })
        {
            Assert.DoesNotContain(forbiddenOverclaim, surface, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task Measure_UsesRealPromptToTaskL2RuntimeEvidenceAndBothConcreteBoundaryTypes()
    {
        var machine = new PromptToTaskMachine();
        var unit = new PromptToTaskIntentUnit(machine);
        var logicalUnit = Assert.IsAssignableFrom<
            ILogicalUnit<PromptToTaskRequest, PromptToTaskResponse>>(unit);
        var request = CreateRequest();

        var execution = await logicalUnit.ExecuteAsync(
            new LogicalUnitRequest<PromptToTaskRequest>(
                RunId: "run:untrained-honesty:001",
                UnitId: PromptToTaskIntentUnit.UnitId,
                UnitRole: PromptToTaskIntentUnit.Role,
                DomainTag: PromptToTaskMachine.DomainTag,
                Input: request,
                Flags: new LogicalExecutionFlags(),
                Metadata: new Dictionary<string, object>()));

        Assert.True(execution.IsSuccess, execution.ErrorMessage);
        var runtimeResponse = execution.Value!.Output;
        var runtimeReportedNotReady = StringComparer.Ordinal.Equals(
                                          runtimeResponse.ContractStatus,
                                          PromptToTaskMachine.NotTrainedStatus)
                                      && StringComparer.Ordinal.Equals(
                                          runtimeResponse.StructuredTask.TaskType,
                                          PromptToTaskMachine.NotReadyTaskType);

        var result = DynamicGraphUntrainedHonestyMeter.Measure(
            new DynamicGraphUntrainedHonestyObservation(
                "evaluation:physical-l2:001",
                runtimeReportedNotReady,
                runtimeResponse.ContractEvidence,
                [machine.GetType(), unit.GetType()]));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("evaluation:physical-l2:001", result.Value!.EvaluationId);
        Assert.True(result.Value.RuntimeReportedNotReady);
        Assert.True(result.Value.HasNoTrainedStateEvidence);
        Assert.True(result.Value.HasNoSemanticClassificationEvidence);
        Assert.Contains(NoTrainedState, result.Value.RuntimeEvidence);
        Assert.Contains(NoSemanticClassification, result.Value.RuntimeEvidence);
        Assert.Equal(
            new[] { typeof(PromptToTaskIntentUnit).FullName!, typeof(PromptToTaskMachine).FullName! }
                .OrderBy(value => value, StringComparer.Ordinal),
            result.Value.InspectedTypeNames);
        Assert.Empty(result.Value.ForbiddenShortcutMatches);
        Assert.True(result.Value.MeetsUntrainedHonestyGate);
    }

    [Fact]
    public void Measure_RuntimeNotReadyPredicateIsIndependentlyRequired()
    {
        var result = Measure(
            runtimeReportedNotReady: false,
            evidence: [NoTrainedState, NoSemanticClassification]);

        AssertSuccessfulFalseGate(result);
        Assert.False(result.Value!.RuntimeReportedNotReady);
        Assert.True(result.Value.HasNoTrainedStateEvidence);
        Assert.True(result.Value.HasNoSemanticClassificationEvidence);
        Assert.Empty(result.Value.ForbiddenShortcutMatches);
    }

    [Fact]
    public void Measure_NoTrainedStateTokenIsIndependentlyRequired()
    {
        var result = Measure(evidence: [NoSemanticClassification]);

        AssertSuccessfulFalseGate(result);
        Assert.True(result.Value!.RuntimeReportedNotReady);
        Assert.False(result.Value.HasNoTrainedStateEvidence);
        Assert.True(result.Value.HasNoSemanticClassificationEvidence);
        Assert.Empty(result.Value.ForbiddenShortcutMatches);
    }

    [Fact]
    public void Measure_NoSemanticClassificationTokenIsIndependentlyRequired()
    {
        var result = Measure(evidence: [NoTrainedState]);

        AssertSuccessfulFalseGate(result);
        Assert.True(result.Value!.RuntimeReportedNotReady);
        Assert.True(result.Value.HasNoTrainedStateEvidence);
        Assert.False(result.Value.HasNoSemanticClassificationEvidence);
        Assert.Empty(result.Value.ForbiddenShortcutMatches);
    }

    [Fact]
    public void Measure_ForbiddenShortcutAbsenceIsIndependentlyRequiredWithoutTurningEvidenceIntoAnError()
    {
        var result = Measure(
            evidence: [NoTrainedState, NoSemanticClassification],
            inspectedTypes: [typeof(PublicInstanceShortcutBoundary)]);

        AssertSuccessfulFalseGate(result);
        Assert.True(result.Value!.RuntimeReportedNotReady);
        Assert.True(result.Value.HasNoTrainedStateEvidence);
        Assert.True(result.Value.HasNoSemanticClassificationEvidence);
        Assert.NotEmpty(result.Value.ForbiddenShortcutMatches);
    }

    [Fact]
    public void Measure_EvidenceTokensAreExactOrdinalAndCaseSensitive()
    {
        var result = Measure(evidence:
        [
            "no_trained_state_loaded",
            "No_Semantic_Classification_Performed"
        ]);

        AssertSuccessfulFalseGate(result);
        Assert.False(result.Value!.HasNoTrainedStateEvidence);
        Assert.False(result.Value.HasNoSemanticClassificationEvidence);
    }

    [Fact]
    public void Measure_EmptyEvidenceIsValidButDoesNotMeetTheGate()
    {
        var result = Measure(evidence: []);

        AssertSuccessfulFalseGate(result);
        Assert.Empty(result.Value!.RuntimeEvidence);
        Assert.False(result.Value.HasNoTrainedStateEvidence);
        Assert.False(result.Value.HasNoSemanticClassificationEvidence);
    }

    [Fact]
    public void Measure_DetectsPrivateStaticRegexSubclassAndDictionaryFields()
    {
        var result = Measure(inspectedTypes: [typeof(PrivateStaticShortcutBoundary)]);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var typeName = typeof(PrivateStaticShortcutBoundary).FullName!;
        Assert.Contains(
            $"{typeName}|field-type:_regex:{typeof(Regex).FullName}",
            result.Value!.ForbiddenShortcutMatches);
        Assert.Contains(
            $"{typeName}|field-type:_derivedRegex:{typeof(DerivedRegex).FullName}",
            result.Value.ForbiddenShortcutMatches);
        Assert.Contains(
            $"{typeName}|field-type:_cache:{typeof(Dictionary<string, int>).FullName}",
            result.Value.ForbiddenShortcutMatches);
        Assert.False(result.Value.MeetsUntrainedHonestyGate);
    }

    [Fact]
    public void Measure_DetectsPublicInstanceRegexAndDictionaryFields()
    {
        var result = Measure(inspectedTypes: [typeof(PublicInstanceShortcutBoundary)]);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var typeName = typeof(PublicInstanceShortcutBoundary).FullName!;
        Assert.Contains(
            $"{typeName}|field-type:Pattern:{typeof(Regex).FullName}",
            result.Value!.ForbiddenShortcutMatches);
        Assert.Contains(
            $"{typeName}|field-type:Cache:{typeof(Dictionary<string, int>).FullName}",
            result.Value.ForbiddenShortcutMatches);
        Assert.False(result.Value.MeetsUntrainedHonestyGate);
    }

    [Fact]
    public void Measure_DetectsInheritedProtectedAndPublicStaticShortcutsThroughFlattenHierarchy()
    {
        var result = Measure(inspectedTypes: [typeof(InheritedStaticShortcutBoundary)]);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var typeName = typeof(InheritedStaticShortcutBoundary).FullName!;
        Assert.Contains(
            $"{typeName}|field-type:ProtectedPattern:{typeof(Regex).FullName}",
            result.Value!.ForbiddenShortcutMatches);
        Assert.Contains(
            $"{typeName}|field-type:PublicCache:{typeof(Dictionary<string, int>).FullName}",
            result.Value.ForbiddenShortcutMatches);
        Assert.False(result.Value.MeetsUntrainedHonestyGate);
    }

    [Fact]
    public void Measure_DetectsMixedCaseDictionaryTypeNameWithExactProvenance()
    {
        var result = Measure(inspectedTypes: [typeof(MixedCaseDictionaryTypeBoundary)]);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var expected = $"{typeof(MixedCaseDictionaryTypeBoundary).FullName}"
                       + $"|field-type:CustomCache:{typeof(lowerdictionaryCarrier).FullName}";
        Assert.Contains(expected, result.Value!.ForbiddenShortcutMatches);
        Assert.False(result.Value.MeetsUntrainedHonestyGate);
    }

    [Fact]
    public void Measure_DetectsEveryForbiddenMemberFragmentIgnoringCase()
    {
        var result = Measure(inspectedTypes: [typeof(MixedCaseMemberShortcutBoundary)]);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var typeName = typeof(MixedCaseMemberShortcutBoundary).FullName!;
        foreach (var methodName in new[]
                 {
                     "cLaSsIfIeR", "tRaInEr", "lOoKuP", "bM25", "eXaCtPrOmPt"
                 })
        {
            Assert.Contains(
                $"{typeName}|member:Method:{methodName}",
                result.Value!.ForbiddenShortcutMatches);
        }

        Assert.False(result.Value!.MeetsUntrainedHonestyGate);
    }

    [Fact]
    public void Measure_ScansBothFirstAndSecondPhysicalBoundaryTypes()
    {
        var machineOnly = Measure(
            inspectedTypes: [typeof(PublicInstanceShortcutBoundary), typeof(CleanBoundary)]);
        var unitOnly = Measure(
            inspectedTypes: [typeof(CleanBoundary), typeof(PrivateStaticShortcutBoundary)]);

        Assert.True(machineOnly.IsSuccess, machineOnly.ErrorMessage);
        Assert.Contains(
            machineOnly.Value!.ForbiddenShortcutMatches,
            match => match.StartsWith(typeof(PublicInstanceShortcutBoundary).FullName!, StringComparison.Ordinal));
        Assert.False(machineOnly.Value.MeetsUntrainedHonestyGate);
        Assert.True(unitOnly.IsSuccess, unitOnly.ErrorMessage);
        Assert.Contains(
            unitOnly.Value!.ForbiddenShortcutMatches,
            match => match.StartsWith(typeof(PrivateStaticShortcutBoundary).FullName!, StringComparison.Ordinal));
        Assert.False(unitOnly.Value.MeetsUntrainedHonestyGate);
    }

    [Fact]
    public void Measure_ReverseInputOrdersProduceCanonicalIdenticalCollections()
    {
        var first = Measure(
            evidence: ["Z_EVIDENCE", NoSemanticClassification, NoTrainedState, "A_EVIDENCE"],
            inspectedTypes: [typeof(CleanBoundaryB), typeof(CleanBoundary)]);
        var second = Measure(
            evidence: ["A_EVIDENCE", NoTrainedState, NoSemanticClassification, "Z_EVIDENCE"],
            inspectedTypes: [typeof(CleanBoundary), typeof(CleanBoundaryB)]);

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.Equal(first.Value!.RuntimeEvidence, second.Value!.RuntimeEvidence);
        Assert.Equal(first.Value.InspectedTypeNames, second.Value.InspectedTypeNames);
        Assert.Equal(first.Value.ForbiddenShortcutMatches, second.Value.ForbiddenShortcutMatches);
        Assert.True(first.Value.MeetsUntrainedHonestyGate);
        Assert.True(second.Value.MeetsUntrainedHonestyGate);
    }

    [Fact]
    public void Measure_ResultIsCallerMutationSafeAndAllReturnedCollectionsAreReadOnly()
    {
        var evidence = new List<string> { NoSemanticClassification, NoTrainedState };
        var inspectedTypes = new List<Type> { typeof(CleanBoundary) };
        var result = Measure(evidence: evidence, inspectedTypes: inspectedTypes);
        Assert.True(result.IsSuccess, result.ErrorMessage);

        evidence.Clear();
        evidence.Add("CALLER_MUTATION");
        inspectedTypes[0] = typeof(PublicInstanceShortcutBoundary);

        Assert.Equal([NoSemanticClassification, NoTrainedState], result.Value!.RuntimeEvidence);
        Assert.Equal([typeof(CleanBoundary).FullName!], result.Value.InspectedTypeNames);
        Assert.Empty(result.Value.ForbiddenShortcutMatches);
        Assert.True(result.Value.MeetsUntrainedHonestyGate);
        Assert.Throws<NotSupportedException>(
            () => ((IList<string>)result.Value.RuntimeEvidence)[0] = "MUTATION");
        Assert.Throws<NotSupportedException>(
            () => ((IList<string>)result.Value.InspectedTypeNames)[0] = "MUTATION");
        Assert.Throws<NotSupportedException>(
            () => ((IList<string>)result.Value.RuntimeEvidence).Add("MUTATION"));
        Assert.Throws<NotSupportedException>(
            () => ((IList<string>)result.Value.InspectedTypeNames).Add("MUTATION"));
        Assert.Throws<NotSupportedException>(
            () => ((IList<string>)result.Value.ForbiddenShortcutMatches).Add("MUTATION"));

        var forbiddenResult = Measure(inspectedTypes: [typeof(PublicInstanceShortcutBoundary)]);
        Assert.True(forbiddenResult.IsSuccess, forbiddenResult.ErrorMessage);
        Assert.NotEmpty(forbiddenResult.Value!.ForbiddenShortcutMatches);
        Assert.Throws<NotSupportedException>(
            () => ((IList<string>)forbiddenResult.Value.ForbiddenShortcutMatches)[0] = "MUTATION");
    }

    [Fact]
    public void Measure_SnapshotsRuntimeEvidenceBeforeInspectedTypes()
    {
        var evidence = new List<string> { NoTrainedState, NoSemanticClassification };
        var inspectedTypes = new CallbackReadOnlyList<Type>(
            [typeof(CleanBoundary)],
            () => evidence.Add("LATE_EVIDENCE"));

        var result = Measure(evidence: evidence, inspectedTypes: inspectedTypes);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.DoesNotContain("LATE_EVIDENCE", result.Value!.RuntimeEvidence);
        Assert.True(result.Value.MeetsUntrainedHonestyGate);
    }

    [Fact]
    public void Measure_DoesNotExecuteTheInspectedTarget()
    {
        ExecutionTrapBoundary.ExecutionCount = 0;

        var result = Measure(inspectedTypes: [typeof(ExecutionTrapBoundary)]);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.True(result.Value!.MeetsUntrainedHonestyGate);
        Assert.Equal(0, ExecutionTrapBoundary.ExecutionCount);
    }

    [Fact]
    public void Measure_NullObservationFailsWithStableLocalError()
        => AssertInvalid(DynamicGraphUntrainedHonestyMeter.Measure(null));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(" leading")]
    [InlineData("trailing ")]
    [InlineData("line\nbreak")]
    public void Measure_MalformedEvaluationIdFailsWithStableLocalError(string? evaluationId)
        => AssertInvalid(Measure(evaluationId: evaluationId!));

    [Fact]
    public void Measure_NullCollectionsFailWithStableLocalError()
    {
        AssertInvalid(DynamicGraphUntrainedHonestyMeter.Measure(
            new DynamicGraphUntrainedHonestyObservation(
                "evaluation:null-evidence",
                true,
                null!,
                [typeof(CleanBoundary)])));
        AssertInvalid(DynamicGraphUntrainedHonestyMeter.Measure(
            new DynamicGraphUntrainedHonestyObservation(
                "evaluation:null-types",
                true,
                [NoTrainedState, NoSemanticClassification],
                null!)));
    }

    [Fact]
    public void Measure_NullTypeEntryFailsWithStableLocalError()
        => AssertInvalid(Measure(inspectedTypes: [typeof(CleanBoundary), null!]));

    [Fact]
    public void Measure_EmptyInspectedTypesFailsWithStableLocalError()
        => AssertInvalid(Measure(inspectedTypes: []));

    [Theory]
    [InlineData(" ")]
    [InlineData(" evidence")]
    [InlineData("evidence ")]
    [InlineData("line\nbreak")]
    public void Measure_BlankOrNonCanonicalEvidenceFailsWithStableLocalError(string evidence)
        => AssertInvalid(Measure(evidence: [evidence]));

    [Fact]
    public void Measure_DuplicateEvidenceFailsWithStableLocalError()
        => AssertInvalid(Measure(evidence: [NoTrainedState, NoTrainedState]));

    [Fact]
    public void Measure_DuplicateTypeIdentityFailsWithStableLocalError()
        => AssertInvalid(Measure(inspectedTypes: [typeof(CleanBoundary), typeof(CleanBoundary)]));

    [Fact]
    public void Measure_DuplicateTypeFullNamesFailWithStableLocalError()
    {
        Type first = new NamedReflectionType(typeof(CleanBoundary), "Duplicate.Boundary");
        Type second = new NamedReflectionType(typeof(CleanBoundaryB), "Duplicate.Boundary");

        AssertInvalid(Measure(inspectedTypes: [first, second]));
    }

    [Fact]
    public void Measure_NonCanonicalTypeFullNameFailsWithStableLocalError()
    {
        Type type = new NamedReflectionType(typeof(CleanBoundary), " invalid.Type");

        AssertInvalid(Measure(inspectedTypes: [type]));
    }

    [Fact]
    public void Measure_OrdinaryEvidenceSnapshotFailureReturnsStableLocalError()
    {
        var fault = new InvalidOperationException("snapshot detail", new IOException("ordinary inner"));

        AssertInvalid(Measure(evidence: new FaultingReadOnlyList<string>(fault)));
    }

    [Fact]
    public void Measure_OrdinaryInspectedTypesSnapshotFailureReturnsStableLocalError()
    {
        var fault = new InvalidOperationException(
            "inspected types snapshot detail",
            new IOException("ordinary inner"));

        AssertInvalid(Measure(inspectedTypes: new FaultingReadOnlyList<Type>(fault)));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Measure_OrdinaryReflectionFailureReturnsStableLocalError(bool failMembers)
    {
        var fault = new InvalidOperationException("reflection detail", new IOException("ordinary inner"));
        Type type = new FaultingReflectionType(typeof(CleanBoundary), fault, failMembers);

        AssertInvalid(Measure(inspectedTypes: [type]));
    }

    [Fact]
    public void Measure_DirectWrappedAndAggregateCancellationOrFatalEvidenceFailuresEscapeSameInstance()
    {
        var exceptions = new Exception[]
        {
            new OperationCanceledException("direct cancellation"),
            new InvalidOperationException(
                "wrapped cancellation",
                new OperationCanceledException("inner cancellation")),
            new AggregateException(
                new InvalidOperationException("ordinary"),
                new OutOfMemoryException("aggregate fatal"))
        };

        foreach (var exception in exceptions)
        {
            var thrown = Record.Exception(
                () => Measure(evidence: new FaultingReadOnlyList<string>(exception)));
            Assert.Same(exception, thrown);
        }
    }

    [Fact]
    public void Measure_DirectWrappedAndAggregateCancellationAndFatalInspectedTypesFailuresEscapeSameInstance()
    {
        var exceptions = new Exception[]
        {
            new OperationCanceledException("direct inspected-types cancellation"),
            new InvalidOperationException(
                "wrapped inspected-types cancellation",
                new OperationCanceledException("inner cancellation")),
            new AggregateException(
                new InvalidOperationException("ordinary"),
                new OperationCanceledException("aggregate cancellation")),
            new OutOfMemoryException("direct inspected-types fatal"),
            new ApplicationException(
                "wrapped inspected-types fatal",
                new BadImageFormatException("inner fatal")),
            new AggregateException(
                new InvalidOperationException("ordinary"),
                new OutOfMemoryException("aggregate fatal"))
        };

        foreach (var exception in exceptions)
        {
            var thrown = Record.Exception(
                () => Measure(inspectedTypes: new FaultingReadOnlyList<Type>(exception)));
            Assert.Same(exception, thrown);
        }
    }

    [Fact]
    public void Measure_DirectWrappedAndAggregateFatalOrCancellationReflectionFailuresEscapeSameInstance()
    {
        var exceptions = new Exception[]
        {
            new OutOfMemoryException("direct fatal"),
            new ApplicationException(
                "wrapped fatal",
                new BadImageFormatException("inner fatal")),
            new AggregateException(
                new InvalidOperationException("ordinary"),
                new OperationCanceledException("aggregate cancellation"))
        };

        foreach (var exception in exceptions)
        {
            Type type = new FaultingReflectionType(typeof(CleanBoundary), exception, failMembers: true);
            var thrown = Record.Exception(() => Measure(inspectedTypes: [type]));
            Assert.Same(exception, thrown);
        }
    }

    private static OperationResult<DynamicGraphUntrainedHonestyMeasurement> Measure(
        string evaluationId = "evaluation:untrained-honesty:001",
        bool runtimeReportedNotReady = true,
        IReadOnlyList<string>? evidence = null,
        IReadOnlyList<Type>? inspectedTypes = null)
        => DynamicGraphUntrainedHonestyMeter.Measure(
            new DynamicGraphUntrainedHonestyObservation(
                evaluationId,
                runtimeReportedNotReady,
                evidence ?? [NoTrainedState, NoSemanticClassification],
                inspectedTypes ?? [typeof(CleanBoundary)]));

    private static PromptToTaskRequest CreateRequest()
        => new(
            PromptId: "prompt:untrained-honesty:001",
            RawPrompt: "Create exactly one bounded work order.",
            RawPromptSha256: "sha256:untrained-honesty-001",
            PromptProvenance: "test:dynamic-graph-untrained-honesty",
            UserLanguage: "en",
            ExplicitConstraints: ["no semantic overclaim"],
            ContextFields: new Dictionary<string, string>
            {
                ["source_root_id"] = "physical-l2-test"
            });

    private static void AssertSuccessfulFalseGate(
        OperationResult<DynamicGraphUntrainedHonestyMeasurement> result)
    {
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.NotNull(result.Value);
        Assert.False(result.Value!.MeetsUntrainedHonestyGate);
    }

    private static void AssertInvalid(
        OperationResult<DynamicGraphUntrainedHonestyMeasurement> result)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(InvalidCode, result.ErrorCode);
        Assert.Equal(InvalidMessage, result.ErrorMessage);
    }

    private static void AssertRecordShape<T>(params (string Name, Type Type)[] expected)
    {
        var constructor = Assert.Single(typeof(T).GetConstructors());
        Assert.Equal(
            expected,
            constructor.GetParameters()
                .Select(parameter => (parameter.Name!, parameter.ParameterType))
                .ToArray());
        Assert.Equal(
            expected.Select(field => field.Name),
            typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name));
    }

    private sealed class FaultingReadOnlyList<T>(Exception exception) : IReadOnlyList<T>
    {
        public int Count => 1;
        public T this[int index] => throw exception;
        public IEnumerator<T> GetEnumerator() => throw exception;
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private sealed class CallbackReadOnlyList<T>(IReadOnlyList<T> values, Action callback)
        : IReadOnlyList<T>
    {
        public int Count => values.Count;
        public T this[int index] => values[index];

        public IEnumerator<T> GetEnumerator()
        {
            callback();
            return values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private sealed class NamedReflectionType(Type delegatedType, string fullName)
        : TypeDelegator(delegatedType)
    {
        public override string? FullName => fullName;
    }

    private sealed class FaultingReflectionType(
        Type delegatedType,
        Exception exception,
        bool failMembers) : TypeDelegator(delegatedType)
    {
        public override FieldInfo[] GetFields(BindingFlags bindingAttr)
            => failMembers ? base.GetFields(bindingAttr) : throw exception;

        public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
            => failMembers ? throw exception : base.GetMembers(bindingAttr);
    }

    private sealed class CleanBoundary
    {
    }

    private sealed class CleanBoundaryB
    {
    }

    private sealed class DerivedRegex : Regex
    {
    }

    private sealed class PrivateStaticShortcutBoundary
    {
        private static readonly Regex _regex = new("fixed");
        private static readonly DerivedRegex _derivedRegex = new();
        private static readonly Dictionary<string, int> _cache = [];
    }

    private sealed class PublicInstanceShortcutBoundary
    {
        public Regex Pattern = new("fixed");
        public Dictionary<string, int> Cache = [];
    }

    private class InheritedStaticShortcutBase
    {
        protected static readonly Regex ProtectedPattern = new("fixed");
        public static readonly Dictionary<string, int> PublicCache = [];
    }

    private sealed class InheritedStaticShortcutBoundary : InheritedStaticShortcutBase
    {
    }

    private sealed class lowerdictionaryCarrier
    {
    }

    private sealed class MixedCaseDictionaryTypeBoundary
    {
        public lowerdictionaryCarrier CustomCache = new();
    }

    private sealed class MixedCaseMemberShortcutBoundary
    {
        private void cLaSsIfIeR() { }
        private static void tRaInEr() { }
        private void lOoKuP() { }
        private void bM25() { }
        private void eXaCtPrOmPt() { }
    }

    private sealed class ExecutionTrapBoundary
    {
        public static int ExecutionCount;

        public static void Execute()
            => ExecutionCount++;
    }
}
