// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace Tests;

using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using SmartSkillsDomain;

public sealed class DynamicGraphUntrainedHonestyL2ProbeTests
{
    private const string InvalidCode = "INVALID_UNTRAINED_HONESTY_L2_PROBE";
    private const string InvalidMessage = "Dynamic graph L2 untrained honesty probe is invalid.";
    private const string MeterInvalidCode = "INVALID_UNTRAINED_HONESTY_MEASUREMENT";
    private const string MeterInvalidMessage = "Dynamic graph untrained honesty measurement is invalid.";
    private const string EvaluationId = "evaluation:physical-l2-probe:001";
    private const string UnitId = "test-l2-unit";
    private const string UnitRole = "test-l2-role";
    private const string DomainTag = "test-l2-domain";
    private const string Version = "test-l2-version:1";
    private const string NoTrainedState = "NO_TRAINED_STATE_LOADED";
    private const string NoSemanticClassification = "NO_SEMANTIC_CLASSIFICATION_PERFORMED";

    [Fact]
    public void Contracts_ExposeExactPlannedShapesAndNoOverclaimSurface()
    {
        AssertRecordShape<DynamicGraphUntrainedHonestyL2ProducerEvidence>(
            ("UnitId", typeof(string)),
            ("UnitRole", typeof(string)),
            ("DomainTag", typeof(string)),
            ("Version", typeof(string)),
            ("RegistrationOrder", typeof(long)),
            ("ProducerTypeName", typeof(string)),
            ("InputTypeName", typeof(string)),
            ("OutputTypeName", typeof(string)));
        AssertRecordShape<DynamicGraphUntrainedHonestyL2ProbeResult>(
            ("Producer", typeof(DynamicGraphUntrainedHonestyL2ProducerEvidence)),
            ("Honesty", typeof(DynamicGraphUntrainedHonestyMeasurement)));

        foreach (var contractType in new[]
                 {
                     typeof(DynamicGraphUntrainedHonestyL2ProducerEvidence),
                     typeof(DynamicGraphUntrainedHonestyL2ProbeResult)
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

        var surface = string.Join(
            '|',
            typeof(DynamicGraphUntrainedHonestyL2ProducerEvidence)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Concat(typeof(DynamicGraphUntrainedHonestyL2ProbeResult)
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                .Select(property => property.Name));
        foreach (var forbidden in new[]
                 {
                     "Target", "Level", "FinalPass", "Passed", "Trained", "Lifecycle", "Promotion"
                 })
        {
            Assert.DoesNotContain(forbidden, surface, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void Probe_ExposesOnlyTheExactGenericMeasureAsyncSurface()
    {
        var method = Assert.Single(typeof(DynamicGraphUntrainedHonestyL2Probe).GetMethods(
            BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
        Assert.Equal(nameof(DynamicGraphUntrainedHonestyL2Probe.MeasureAsync), method.Name);
        Assert.True(method.IsGenericMethodDefinition);
        Assert.Equal(2, method.GetGenericArguments().Length);
        Assert.Equal(
            typeof(Task<OperationResult<DynamicGraphUntrainedHonestyL2ProbeResult>>),
            method.ReturnType);

        var parameters = method.GetParameters();
        Assert.Equal(
            new[]
            {
                "evaluationId", "resolvedProducer", "request", "runtimeNotReadyPredicate",
                "supportingBoundaryTypes", "cancellationToken"
            },
            parameters.Select(parameter => parameter.Name));
        Assert.Equal(typeof(string), parameters[0].ParameterType);
        Assert.Equal(
            typeof(ResolvedLogicalUnitRegistryEntry<,>),
            parameters[1].ParameterType.GetGenericTypeDefinition());
        Assert.Equal(typeof(LogicalUnitRequest<>), parameters[2].ParameterType.GetGenericTypeDefinition());
        Assert.Equal(typeof(Func<,>), parameters[3].ParameterType.GetGenericTypeDefinition());
        Assert.Equal(typeof(IReadOnlyList<Type>), parameters[4].ParameterType);
        Assert.Equal(typeof(CancellationToken), parameters[5].ParameterType);
        Assert.True(parameters[5].HasDefaultValue);
        Assert.Empty(typeof(DynamicGraphUntrainedHonestyL2Probe).GetFields(
            BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
        Assert.Empty(typeof(DynamicGraphUntrainedHonestyL2Probe).GetProperties(
            BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
    }

    [Fact]
    public async Task MeasureAsync_UsesRealResolvedPromptToTaskProducerAndItsActualEvidence()
    {
        var machine = new PromptToTaskMachine();
        var unit = new PromptToTaskIntentUnit(machine);
        var registry = new InMemoryLogicalUnitRegistry();
        var registered = registry.Register<PromptToTaskRequest, PromptToTaskResponse>(
            PromptToTaskIntentUnit.UnitId,
            PromptToTaskIntentUnit.Role,
            PromptToTaskMachine.DomainTag,
            "physical-l2-probe:v1",
            LogicalUnitTrustState.Candidate,
            isLearned: false,
            unit);
        Assert.True(registered.IsSuccess, registered.ErrorMessage);

        var resolved = registry.ResolveLatestEntry<PromptToTaskRequest, PromptToTaskResponse>(
            PromptToTaskIntentUnit.Role,
            PromptToTaskMachine.DomainTag,
            LogicalUnitTrustState.Candidate);
        Assert.True(resolved.IsSuccess, resolved.ErrorMessage);

        var result = await DynamicGraphUntrainedHonestyL2Probe.MeasureAsync(
            EvaluationId,
            resolved.Value,
            new LogicalUnitRequest<PromptToTaskRequest>(
                "run:physical-l2-probe:001",
                PromptToTaskIntentUnit.UnitId,
                PromptToTaskIntentUnit.Role,
                PromptToTaskMachine.DomainTag,
                CreatePromptToTaskRequest(),
                new LogicalExecutionFlags(),
                new Dictionary<string, object>()),
            logicalResult =>
                StringComparer.Ordinal.Equals(
                    logicalResult.Output.ContractStatus,
                    PromptToTaskMachine.NotTrainedStatus)
                && StringComparer.Ordinal.Equals(
                    logicalResult.Output.StructuredTask.TaskType,
                    PromptToTaskMachine.NotReadyTaskType),
            [machine.GetType()]);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var probe = Assert.IsType<DynamicGraphUntrainedHonestyL2ProbeResult>(result.Value);
        Assert.Equal(registered.Value!.UnitId, probe.Producer.UnitId);
        Assert.Equal(registered.Value.UnitRole, probe.Producer.UnitRole);
        Assert.Equal(registered.Value.DomainTag, probe.Producer.DomainTag);
        Assert.Equal(registered.Value.Version, probe.Producer.Version);
        Assert.Equal(registered.Value.RegistrationOrder, probe.Producer.RegistrationOrder);
        Assert.Equal(unit.GetType().FullName, probe.Producer.ProducerTypeName);
        Assert.Equal(typeof(PromptToTaskRequest).FullName, probe.Producer.InputTypeName);
        Assert.Equal(typeof(PromptToTaskResponse).FullName, probe.Producer.OutputTypeName);
        Assert.Equal(EvaluationId, probe.Honesty.EvaluationId);
        Assert.Equal(
            new[] { unit.GetType().FullName!, machine.GetType().FullName! }
                .OrderBy(value => value, StringComparer.Ordinal),
            probe.Honesty.InspectedTypeNames);
        Assert.Contains("PROMPT_TO_TASK_LOGICAL_UNIT_SOURCE_BACKED", probe.Honesty.RuntimeEvidence);
        Assert.Contains($"PROMPT_TO_TASK_UNIT_ROLE:{PromptToTaskIntentUnit.Role}", probe.Honesty.RuntimeEvidence);
        Assert.Contains(NoTrainedState, probe.Honesty.RuntimeEvidence);
        Assert.Contains(NoSemanticClassification, probe.Honesty.RuntimeEvidence);
        Assert.Empty(probe.Honesty.ForbiddenShortcutMatches);
        Assert.True(probe.Honesty.MeetsUntrainedHonestyGate);
    }

    [Fact]
    public async Task MeasureAsync_RejectsMissingInputsAndNonCanonicalEvaluationBeforeExecution()
    {
        var unit = new RecordingUnit();
        var resolved = CreateResolved(unit);
        var request = CreateRequest();
        var predicateCount = 0;
        Func<LogicalUnitResult<string>, bool> predicate = _ =>
        {
            predicateCount++;
            return true;
        };

        AssertInvalid(await Measure(" evaluation", resolved, request, predicate, []));
        AssertInvalid(await Measure(EvaluationId, null, request, predicate, []));
        AssertInvalid(await Measure(
            EvaluationId,
            new ResolvedLogicalUnitRegistryEntry<string, string>(null!, unit),
            request,
            predicate,
            []));
        AssertInvalid(await Measure(
            EvaluationId,
            new ResolvedLogicalUnitRegistryEntry<string, string>(CreateDescriptor(), null!),
            request,
            predicate,
            []));
        AssertInvalid(await Measure(EvaluationId, resolved, null, predicate, []));
        AssertInvalid(await Measure(EvaluationId, resolved, request, null, []));
        AssertInvalid(await Measure(EvaluationId, resolved, request, predicate, null));

        Assert.Equal(0, unit.ExecutionCount);
        Assert.Equal(0, predicateCount);
    }

    [Theory]
    [InlineData("UnitId")]
    [InlineData("UnitRole")]
    [InlineData("DomainTag")]
    public async Task MeasureAsync_RejectsEachRequestIdentityMismatchBeforeExecution(string field)
    {
        var unit = new RecordingUnit();
        var request = CreateRequest();
        request = field switch
        {
            "UnitId" => request with { UnitId = "other-unit" },
            "UnitRole" => request with { UnitRole = "other-role" },
            "DomainTag" => request with { DomainTag = "other-domain" },
            _ => throw new ArgumentOutOfRangeException(nameof(field))
        };
        var predicateCount = 0;

        var result = await Measure(
            EvaluationId,
            CreateResolved(unit),
            request,
            _ =>
            {
                predicateCount++;
                return true;
            },
            []);

        AssertInvalid(result);
        Assert.Equal(0, unit.ExecutionCount);
        Assert.Equal(0, predicateCount);
    }

    [Theory]
    [InlineData("UnitId")]
    [InlineData("UnitRole")]
    [InlineData("DomainTag")]
    public async Task MeasureAsync_RejectsEachReturnedIdentityMismatchBeforePredicate(string field)
    {
        var unit = new RecordingUnit();
        switch (field)
        {
            case "UnitId":
                unit.ReturnedUnitId = "other-unit";
                break;
            case "UnitRole":
                unit.ReturnedUnitRole = "other-role";
                break;
            case "DomainTag":
                unit.ReturnedDomainTag = "other-domain";
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(field));
        }

        var predicateCount = 0;
        var result = await Measure(
            EvaluationId,
            CreateResolved(unit),
            CreateRequest(),
            _ =>
            {
                predicateCount++;
                return true;
            },
            []);

        AssertInvalid(result);
        Assert.Equal(1, unit.ExecutionCount);
        Assert.Equal(0, predicateCount);
    }

    [Theory]
    [InlineData("UnitId")]
    [InlineData("UnitRole")]
    [InlineData("DomainTag")]
    [InlineData("Version")]
    [InlineData("RegistrationOrder")]
    [InlineData("InputType")]
    [InlineData("OutputType")]
    public async Task MeasureAsync_RejectsInvalidDescriptorBeforeExecution(string field)
    {
        var unit = new RecordingUnit();
        var descriptor = CreateDescriptor();
        descriptor = field switch
        {
            "UnitId" => descriptor with { UnitId = " unit" },
            "UnitRole" => descriptor with { UnitRole = "role\n" },
            "DomainTag" => descriptor with { DomainTag = " " },
            "Version" => descriptor with { Version = "version\u0085" },
            "RegistrationOrder" => descriptor with { RegistrationOrder = 0 },
            "InputType" => descriptor with { InputType = typeof(int) },
            "OutputType" => descriptor with { OutputType = typeof(int) },
            _ => throw new ArgumentOutOfRangeException(nameof(field))
        };
        var predicateCount = 0;

        var result = await Measure(
            EvaluationId,
            new ResolvedLogicalUnitRegistryEntry<string, string>(descriptor, unit),
            CreateRequest(),
            _ =>
            {
                predicateCount++;
                return true;
            },
            []);

        AssertInvalid(result);
        Assert.Equal(0, unit.ExecutionCount);
        Assert.Equal(0, predicateCount);
    }

    [Fact]
    public async Task MeasureAsync_AutoIncludesProducerAndAllowsEmptyOrDistinctSupportingTypes()
    {
        var withoutSupporting = await Measure(
            EvaluationId,
            CreateResolved(new RecordingUnit()),
            CreateRequest(),
            _ => true,
            []);
        Assert.True(withoutSupporting.IsSuccess, withoutSupporting.ErrorMessage);
        Assert.Equal(
            [typeof(RecordingUnit).FullName!],
            withoutSupporting.Value!.Honesty.InspectedTypeNames);

        var withSupporting = await Measure(
            EvaluationId,
            CreateResolved(new RecordingUnit()),
            CreateRequest(),
            _ => true,
            [typeof(SafeBoundary)]);
        Assert.True(withSupporting.IsSuccess, withSupporting.ErrorMessage);
        Assert.Equal(
            new[] { typeof(RecordingUnit).FullName!, typeof(SafeBoundary).FullName! }
                .OrderBy(value => value, StringComparer.Ordinal),
            withSupporting.Value!.Honesty.InspectedTypeNames);
    }

    [Fact]
    public async Task MeasureAsync_RejectsProducerRepetitionNullDuplicateReferenceFullNameAndNonCanonicalTypeBeforeExecution()
    {
        var duplicateNameA = new NamedReflectionType(typeof(SafeBoundary), "Tests.DuplicateBoundary");
        var duplicateNameB = new NamedReflectionType(typeof(OtherSafeBoundary), "Tests.DuplicateBoundary");
        var nonCanonical = new NamedReflectionType(typeof(SafeBoundary), " Tests.NonCanonicalBoundary");
        var cases = new IReadOnlyList<Type>[]
        {
            [typeof(RecordingUnit)],
            [null!],
            [typeof(SafeBoundary), typeof(SafeBoundary)],
            [duplicateNameA, duplicateNameB],
            [nonCanonical]
        };

        foreach (var supportingTypes in cases)
        {
            var unit = new RecordingUnit();
            var predicateCount = 0;
            var result = await Measure(
                EvaluationId,
                CreateResolved(unit),
                CreateRequest(),
                _ =>
                {
                    predicateCount++;
                    return true;
                },
                supportingTypes);

            AssertInvalid(result);
            Assert.Equal(0, unit.ExecutionCount);
            Assert.Equal(0, predicateCount);
        }
    }

    [Fact]
    public async Task MeasureAsync_ExecutesAndInvokesPredicateExactlyOnce()
    {
        var unit = new RecordingUnit();
        var predicateCount = 0;

        var result = await Measure(
            EvaluationId,
            CreateResolved(unit),
            CreateRequest(),
            _ =>
            {
                predicateCount++;
                return true;
            },
            []);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, unit.ExecutionCount);
        Assert.Equal(1, predicateCount);
    }

    [Fact]
    public async Task MeasureAsync_ForwardsExactCancellationTokenAndExecutesOnce()
    {
        using var source = new CancellationTokenSource();
        var cancellationToken = source.Token;
        var unit = new RecordingUnit();

        var result = await Measure(
            EvaluationId,
            CreateResolved(unit),
            CreateRequest(),
            _ => true,
            [],
            cancellationToken);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.NotEqual(default, cancellationToken);
        Assert.Equal(cancellationToken, unit.ReceivedCancellationToken);
        Assert.Equal(1, unit.ExecutionCount);
    }

    [Fact]
    public async Task MeasureAsync_InvokesMeterExactlyOnceAndUsesActualResultEvidence()
    {
        var evidence = new[]
        {
            NoTrainedState,
            NoSemanticClassification,
            "ACTUAL_SINGLE_METER_EVIDENCE"
        };
        var unit = new RecordingUnit { EvidenceIds = evidence };
        var countingBoundary = new CountingReflectionType(typeof(SafeBoundary));

        var result = await Measure(
            EvaluationId,
            CreateResolved(unit),
            CreateRequest(),
            _ => true,
            [countingBoundary]);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, unit.ExecutionCount);
        Assert.Equal(1, countingBoundary.GetFieldsCallCount);
        Assert.Equal(1, countingBoundary.GetMembersCallCount);
        Assert.Equal(
            evidence.OrderBy(value => value, StringComparer.Ordinal),
            result.Value!.Honesty.RuntimeEvidence);
        Assert.Contains("ACTUAL_SINGLE_METER_EVIDENCE", result.Value.Honesty.RuntimeEvidence);
    }

    [Fact]
    public async Task MeasureAsync_FalsePredicateReturnsSuccessfulFalseGate()
    {
        var result = await Measure(
            EvaluationId,
            CreateResolved(new RecordingUnit()),
            CreateRequest(),
            _ => false,
            []);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.False(result.Value!.Honesty.RuntimeReportedNotReady);
        Assert.False(result.Value.Honesty.MeetsUntrainedHonestyGate);
    }

    [Theory]
    [InlineData(NoTrainedState)]
    [InlineData(NoSemanticClassification)]
    public async Task MeasureAsync_MissingActualRuntimeTokenReturnsSuccessfulFalseGate(string omittedToken)
    {
        var unit = new RecordingUnit
        {
            EvidenceIds = new[] { NoTrainedState, NoSemanticClassification }
                .Where(token => !StringComparer.Ordinal.Equals(token, omittedToken))
                .ToArray()
        };

        var result = await Measure(
            EvaluationId,
            CreateResolved(unit),
            CreateRequest(),
            _ => true,
            []);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.False(result.Value!.Honesty.MeetsUntrainedHonestyGate);
        Assert.DoesNotContain(omittedToken, result.Value.Honesty.RuntimeEvidence);
    }

    [Fact]
    public async Task MeasureAsync_ForbiddenSupportingBoundaryReturnsSuccessfulFalseGate()
    {
        var result = await Measure(
            EvaluationId,
            CreateResolved(new RecordingUnit()),
            CreateRequest(),
            _ => true,
            [typeof(ForbiddenSupportingBoundary)]);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.False(result.Value!.Honesty.MeetsUntrainedHonestyGate);
        Assert.NotEmpty(result.Value.Honesty.ForbiddenShortcutMatches);
    }

    [Fact]
    public async Task MeasureAsync_PropagatesExecutionFailureExactlyAndSkipsPredicate()
    {
        var unit = new RecordingUnit
        {
            Failure = OperationResult<LogicalUnitResult<string>>.Fail(
                "exact execution failure",
                "EXACT_EXECUTION_FAILURE")
        };
        var predicateCount = 0;

        var result = await Measure(
            EvaluationId,
            CreateResolved(unit),
            CreateRequest(),
            _ =>
            {
                predicateCount++;
                return true;
            },
            []);

        Assert.False(result.IsSuccess);
        Assert.Equal("EXACT_EXECUTION_FAILURE", result.ErrorCode);
        Assert.Equal("exact execution failure", result.ErrorMessage);
        Assert.Equal(1, unit.ExecutionCount);
        Assert.Equal(0, predicateCount);
    }

    [Fact]
    public async Task MeasureAsync_RejectsSuccessfulNullExecutionBeforePredicate()
    {
        var unit = new RecordingUnit { ReturnNullSuccess = true };
        var predicateCount = 0;

        var result = await Measure(
            EvaluationId,
            CreateResolved(unit),
            CreateRequest(),
            _ =>
            {
                predicateCount++;
                return true;
            },
            []);

        AssertInvalid(result);
        Assert.Equal(1, unit.ExecutionCount);
        Assert.Equal(0, predicateCount);
    }

    [Fact]
    public async Task MeasureAsync_PropagatesMeterFailureExactly()
    {
        var unit = new RecordingUnit { EvidenceIds = null! };
        var predicateCount = 0;

        var result = await Measure(
            EvaluationId,
            CreateResolved(unit),
            CreateRequest(),
            _ =>
            {
                predicateCount++;
                return true;
            },
            []);

        Assert.False(result.IsSuccess);
        Assert.Equal(MeterInvalidCode, result.ErrorCode);
        Assert.Equal(MeterInvalidMessage, result.ErrorMessage);
        Assert.Equal(1, unit.ExecutionCount);
        Assert.Equal(1, predicateCount);
    }

    [Theory]
    [InlineData("snapshot")]
    [InlineData("execution")]
    [InlineData("predicate")]
    public async Task MeasureAsync_OrdinaryBoundaryExceptionsBecomeStableLocalInvalid(string phase)
    {
        var ordinary = new InvalidOperationException($"ordinary-{phase}");
        var unit = new RecordingUnit
        {
            ThrownException = phase == "execution" ? ordinary : null
        };
        var predicateCount = 0;
        IReadOnlyList<Type> supportingTypes = phase == "snapshot"
            ? new FaultingReadOnlyList<Type>(ordinary)
            : [];

        var result = await Measure(
            EvaluationId,
            CreateResolved(unit),
            CreateRequest(),
            _ =>
            {
                predicateCount++;
                if (phase == "predicate")
                    throw ordinary;
                return true;
            },
            supportingTypes);

        AssertInvalid(result);
        Assert.Equal(phase == "snapshot" ? 0 : 1, unit.ExecutionCount);
        Assert.Equal(phase == "predicate" ? 1 : 0, predicateCount);
    }

    public static IEnumerable<object[]> ExceptionBoundaryCases()
    {
        foreach (var phase in new[] { "snapshot", "execution", "predicate" })
            foreach (var graph in new[]
                     {
                     "direct-cancellation", "wrapped-cancellation", "aggregate-cancellation",
                     "direct-fatal", "wrapped-fatal", "aggregate-fatal"
                 })
            {
                yield return [phase, graph];
            }
    }

    [Theory]
    [MemberData(nameof(ExceptionBoundaryCases))]
    public async Task MeasureAsync_CancellationAndFatalGraphsEscapeAsSameOuterInstance(
        string phase,
        string graph)
    {
        var outer = CreateExceptionGraph(graph);
        var unit = new RecordingUnit
        {
            ThrownException = phase == "execution" ? outer : null
        };
        IReadOnlyList<Type> supportingTypes = phase == "snapshot"
            ? new FaultingReadOnlyList<Type>(outer)
            : [];

        var thrown = await Assert.ThrowsAnyAsync<Exception>(() => Measure(
            EvaluationId,
            CreateResolved(unit),
            CreateRequest(),
            _ => phase == "predicate" ? throw outer : true,
            supportingTypes));

        Assert.Same(outer, thrown);
    }

    [Fact]
    public async Task MeasureAsync_OwnsEvidenceAndSupportingTypeSnapshotsAfterReturn()
    {
        var evidence = new[] { NoTrainedState, NoSemanticClassification, "OWNED_RUNTIME_EVIDENCE" };
        var supportingTypes = new[] { typeof(SafeBoundary) };
        var unit = new RecordingUnit { EvidenceIds = evidence };

        var result = await Measure(
            EvaluationId,
            CreateResolved(unit),
            CreateRequest(),
            _ => true,
            supportingTypes);
        Assert.True(result.IsSuccess, result.ErrorMessage);

        evidence[0] = "CALLER_MUTATED_EVIDENCE";
        supportingTypes[0] = typeof(ForbiddenSupportingBoundary);

        Assert.Contains(NoTrainedState, result.Value!.Honesty.RuntimeEvidence);
        Assert.Contains("OWNED_RUNTIME_EVIDENCE", result.Value.Honesty.RuntimeEvidence);
        Assert.DoesNotContain("CALLER_MUTATED_EVIDENCE", result.Value.Honesty.RuntimeEvidence);
        Assert.Contains(typeof(SafeBoundary).FullName!, result.Value.Honesty.InspectedTypeNames);
        Assert.DoesNotContain(
            typeof(ForbiddenSupportingBoundary).FullName!,
            result.Value.Honesty.InspectedTypeNames);
        Assert.Empty(result.Value.Honesty.ForbiddenShortcutMatches);
        Assert.True(result.Value.Honesty.MeetsUntrainedHonestyGate);
    }

    private static Task<OperationResult<DynamicGraphUntrainedHonestyL2ProbeResult>> Measure(
        string evaluationId,
        ResolvedLogicalUnitRegistryEntry<string, string>? resolved,
        LogicalUnitRequest<string>? request,
        Func<LogicalUnitResult<string>, bool>? predicate,
        IReadOnlyList<Type>? supportingTypes,
        CancellationToken cancellationToken = default)
        => DynamicGraphUntrainedHonestyL2Probe.MeasureAsync(
            evaluationId,
            resolved,
            request,
            predicate,
            supportingTypes,
            cancellationToken);

    private static ResolvedLogicalUnitRegistryEntry<string, string> CreateResolved(
        RecordingUnit unit,
        LogicalUnitDescriptor? descriptor = null)
        => new(descriptor ?? CreateDescriptor(), unit);

    private static LogicalUnitDescriptor CreateDescriptor()
        => new(
            UnitId,
            UnitRole,
            DomainTag,
            Version,
            LogicalUnitTrustState.Candidate,
            IsLearned: false,
            typeof(string),
            typeof(string),
            RegistrationOrder: 7);

    private static LogicalUnitRequest<string> CreateRequest()
        => new(
            "run:physical-l2-probe:001",
            UnitId,
            UnitRole,
            DomainTag,
            "input",
            new LogicalExecutionFlags(),
            new Dictionary<string, object>());

    private static PromptToTaskRequest CreatePromptToTaskRequest()
        => new(
            "prompt:physical-l2-probe:001",
            "Create exactly one bounded work order.",
            "sha256:physical-l2-probe-001",
            "test:dynamic-graph-untrained-honesty-l2-probe",
            "en",
            ["no semantic overclaim"],
            new Dictionary<string, string>
            {
                ["source_root_id"] = "physical-l2-probe-test"
            });

    private static Exception CreateExceptionGraph(string graph)
        => graph switch
        {
            "direct-cancellation" => new OperationCanceledException("direct cancellation"),
            "wrapped-cancellation" => new InvalidOperationException(
                "wrapped cancellation",
                new OperationCanceledException("nested cancellation")),
            "aggregate-cancellation" => new AggregateException(
                "aggregate cancellation",
                new InvalidOperationException("ordinary sibling"),
                new OperationCanceledException("aggregate cancellation node")),
            "direct-fatal" => new OutOfMemoryException("direct fatal"),
            "wrapped-fatal" => new InvalidOperationException(
                "wrapped fatal",
                new OutOfMemoryException("nested fatal")),
            "aggregate-fatal" => new AggregateException(
                "aggregate fatal",
                new InvalidOperationException("ordinary sibling"),
                new OutOfMemoryException("aggregate fatal node")),
            _ => throw new ArgumentOutOfRangeException(nameof(graph))
        };

    private static void AssertInvalid(
        OperationResult<DynamicGraphUntrainedHonestyL2ProbeResult> result)
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

    private sealed class RecordingUnit : ILogicalUnit<string, string>
    {
        public int ExecutionCount { get; private set; }

        public CancellationToken ReceivedCancellationToken { get; private set; }

        public string ReturnedUnitId { get; set; } = UnitId;

        public string ReturnedUnitRole { get; set; } = UnitRole;

        public string ReturnedDomainTag { get; set; } = DomainTag;

        public string[] EvidenceIds { get; set; } = [NoTrainedState, NoSemanticClassification];

        public OperationResult<LogicalUnitResult<string>>? Failure { get; set; }

        public Exception? ThrownException { get; set; }

        public bool ReturnNullSuccess { get; set; }

        public Task<OperationResult<LogicalUnitResult<string>>> ExecuteAsync(
            LogicalUnitRequest<string> request,
            CancellationToken cancellationToken = default)
        {
            ExecutionCount++;
            ReceivedCancellationToken = cancellationToken;
            if (ThrownException is not null)
            {
                return Task.FromException<OperationResult<LogicalUnitResult<string>>>(
                    ThrownException);
            }

            if (Failure is not null)
                return Task.FromResult(Failure);

            if (ReturnNullSuccess)
            {
                return Task.FromResult(
                    OperationResult<LogicalUnitResult<string>>.Ok(null!));
            }

            return Task.FromResult(OperationResult<LogicalUnitResult<string>>.Ok(
                new LogicalUnitResult<string>(
                    ReturnedUnitId,
                    ReturnedUnitRole,
                    ReturnedDomainTag,
                    "output",
                    0.0,
                    [],
                    [ReturnedDomainTag],
                    EvidenceIds,
                    [])));
        }
    }

    private sealed class FaultingReadOnlyList<T>(Exception exception) : IReadOnlyList<T>
    {
        public int Count => 1;

        public T this[int index] => throw exception;

        public IEnumerator<T> GetEnumerator() => throw exception;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private sealed class NamedReflectionType(Type delegatedType, string fullName)
        : TypeDelegator(delegatedType)
    {
        public override string? FullName => fullName;
    }

    private sealed class CountingReflectionType(Type delegatedType) : TypeDelegator(delegatedType)
    {
        public int GetFieldsCallCount { get; private set; }

        public int GetMembersCallCount { get; private set; }

        public override FieldInfo[] GetFields(BindingFlags bindingAttr)
        {
            GetFieldsCallCount++;
            return base.GetFields(bindingAttr);
        }

        public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
        {
            GetMembersCallCount++;
            return base.GetMembers(bindingAttr);
        }
    }

    private sealed class SafeBoundary;

    private sealed class OtherSafeBoundary;

    private sealed class ForbiddenSupportingBoundary
    {
        public int ClassifierMarker { get; init; }
    }
}
