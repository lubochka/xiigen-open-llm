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
using Domain = SemanticCompiler29.Domain;
using SemanticCompilerAdapter;
using SmartSkillsDomain;

public sealed class DynamicGraphMeasurementCoordinatorTests
{
    private const string InvalidAblationTargetLevelCode =
        "DYNAMIC_GRAPH_ABLATION_TARGET_LEVEL_INVALID";
    private const string InvalidAblationTargetLevelMessage =
        "Dynamic graph ablation can only be associated with an L2 logical-unit or L3 logical-flow target.";
    private const string InvalidNoveltyTargetLevelCode =
        "DYNAMIC_GRAPH_NOVELTY_TARGET_LEVEL_INVALID";
    private const string InvalidNoveltyTargetLevelMessage =
        "Dynamic graph novelty can only be associated with an L2 logical-unit or L3 logical-flow target.";
    private const string EdgeOrderQualityId = "edge-order-quality-coordinator";
    private const string EdgeOrderQualityUnitId = "edge-order-quality-unit";
    private const string InvalidEdgeAuthorityControlTargetLevelCode =
        "DYNAMIC_GRAPH_EDGE_AUTHORITY_CONTROL_TARGET_LEVEL_INVALID";
    private const string InvalidEdgeAuthorityControlTargetLevelMessage =
        "Dynamic graph edge authority control can only be associated with an L3 logical-flow target.";
    private const string EdgeAuthorityControlTargetMismatchCode =
        "DYNAMIC_GRAPH_EDGE_AUTHORITY_CONTROL_TARGET_MISMATCH";
    private const string EdgeAuthorityControlTargetMismatchMessage =
        "Dynamic graph edge authority control target does not match the measured edge.";
    private const string InvalidEdgeQualityEvidenceTargetLevelCode =
        "DYNAMIC_GRAPH_EDGE_QUALITY_EVIDENCE_TARGET_LEVEL_INVALID";
    private const string InvalidEdgeQualityEvidenceTargetLevelMessage =
        "Dynamic graph edge quality evidence can only be associated with an L3 logical-flow target.";
    private const string EdgeQualityEvidenceTargetMismatchCode =
        "DYNAMIC_GRAPH_EDGE_QUALITY_EVIDENCE_TARGET_MISMATCH";
    private const string EdgeQualityEvidenceTargetMismatchMessage =
        "Dynamic graph edge quality evidence target does not match the measured edge.";
    private const string InvalidUntrainedHonestyTargetLevelCode =
        "DYNAMIC_GRAPH_UNTRAINED_HONESTY_TARGET_LEVEL_INVALID";
    private const string InvalidUntrainedHonestyTargetLevelMessage =
        "Dynamic graph untrained honesty can only be associated with an L2 logical-unit target.";
    private const string InvalidUntrainedHonestyProbeResultCode =
        "INVALID_UNTRAINED_HONESTY_L2_PROBE_RESULT";
    private const string InvalidUntrainedHonestyProbeResultMessage =
        "Dynamic graph L2 untrained honesty probe result is invalid.";
    private const string UntrainedHonestyTargetMismatchCode =
        "DYNAMIC_GRAPH_UNTRAINED_HONESTY_TARGET_MISMATCH";
    private const string UntrainedHonestyTargetMismatchMessage =
        "Dynamic graph untrained honesty target does not match the measured L2 producer.";
    private const string UntrainedHonestyUnitId = "untrained-honesty-l2-unit";
    private const string NoTrainedState = "NO_TRAINED_STATE_LOADED";
    private const string NoSemanticClassification = "NO_SEMANTIC_CLASSIFICATION_PERFORMED";
    private const string SelectionEvaluationId = "edge-selection-coordinator-evaluation";
    private const string A = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    private const string B = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
    private const string C = "cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc";

    private static readonly string[] EdgeOrderQualityLeakageSignals =
        ["candidate_index", "source_path", "source_id", "file_order", "correlation_id", "exact_prompt"];
    private static readonly string[] IdentityProbeIds =
        ["source-id-control-01", "source-path-control-02", "correlation-id-control-03"];
    private static readonly string[] IdentityControlledSignals =
        ["source_id", "source_path", "correlation_id"];
    private static readonly string[] IdentityControlValues =
        [
            "edge-authority-source-id-control",
            "identity-control://edge-authority-source-path",
            "edge-authority-correlation-id-control"
        ];

    [Fact]
    public void Contract_ExposesExactCoordinatorRecordShapes()
    {
        AssertRecordShape<DynamicGraphTargetAblationMeasurement>(
            ("Target", typeof(DynamicGraphMeasurementTarget)),
            ("Ablation", typeof(DynamicGraphAblationMeasurement)));
        AssertRecordShape<DynamicGraphTargetAccuracyMeasurement>(
            ("Target", typeof(DynamicGraphMeasurementTarget)),
            ("Accuracy", typeof(DynamicGraphAccuracyMeasurement)));
        AssertRecordShape<DynamicGraphTargetRunSpeedMeasurement>(
            ("Target", typeof(DynamicGraphMeasurementTarget)),
            ("RunSpeed", typeof(DynamicGraphRunSpeedMeasurement)));
        AssertRecordShape<DynamicGraphTargetTrainingSpeedMeasurement>(
            ("Target", typeof(DynamicGraphMeasurementTarget)),
            ("TrainingSpeed", typeof(DynamicGraphTrainingSpeedMeasurement)));
        AssertRecordShape<DynamicGraphTargetEdgeOrderQualityMeasurement>(
            ("Target", typeof(DynamicGraphMeasurementTarget)),
            ("EdgeOrderQuality", typeof(DynamicGraphEdgeOrderQualityMeasurement)));
        AssertRecordShape<DynamicGraphTargetEdgeIdentityQualityMeasurement>(
            ("Target", typeof(DynamicGraphMeasurementTarget)),
            ("EdgeIdentityQuality", typeof(DynamicGraphEdgeIdentityQualityMeasurement)));
        AssertOwnedRecordShape<DynamicGraphTargetEdgeAuthorityControlMeasurement>(
            ("Target", typeof(DynamicGraphMeasurementTarget)),
            ("EdgeAuthorityControl", typeof(DynamicGraphEdgeAuthorityControlMeasurement)));
        AssertOwnedRecordShape<DynamicGraphTargetEdgeQualityEvidenceMeasurement>(
            ("Target", typeof(DynamicGraphMeasurementTarget)),
            ("EdgeQualityEvidence", typeof(DynamicGraphEdgeQualityEvidenceMeasurement)));
        AssertOwnedRecordShape<
            DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>(
            ("Target", typeof(DynamicGraphMeasurementTarget)),
            ("EdgeAuthoritySelectionQualityEvidence",
                typeof(DynamicGraphEdgeAuthoritySelectionQualityEvidenceMeasurement)));
        Assert.Null(typeof(DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement)
            .GetProperty("Verdict"));
        Assert.Null(typeof(DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement)
            .GetProperty("MeetsOverallGate"));
        Assert.All(
            typeof(DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
            property => Assert.Null(property.SetMethod));
        Assert.All(
            typeof(DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement)
                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
            field => Assert.True(field.IsInitOnly));
        AssertRecordShape<DynamicGraphTargetNoveltyMeasurement>(
            ("Target", typeof(DynamicGraphMeasurementTarget)),
            ("Novelty", typeof(DynamicGraphNoveltyMeasurement)));
        AssertRecordShape<DynamicGraphTargetUntrainedHonestyMeasurement>(
            ("Target", typeof(DynamicGraphMeasurementTarget)),
            ("UntrainedHonesty", typeof(DynamicGraphUntrainedHonestyL2ProbeResult)));
    }

    [Fact]
    public void Coordinator_ExposesExactlyThePlannedPublicMethods()
    {
        var methods = typeof(DynamicGraphMeasurementCoordinator).GetMethods(
                BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .OrderBy(method => method.Name)
            .ToArray();
        Assert.Equal(11, methods.Length);

        AssertMethodShape(
            methods[0],
            "BindL2UntrainedHonesty",
            typeof(OperationResult<DynamicGraphTargetUntrainedHonestyMeasurement>),
            ("target", typeof(DynamicGraphMeasurementTarget)),
            ("probeResult", typeof(DynamicGraphUntrainedHonestyL2ProbeResult)));
        Assert.False(methods[0].IsGenericMethod);
        AssertMethodShape(
            methods[1],
            "MeasureAblation",
            typeof(OperationResult<DynamicGraphTargetAblationMeasurement>),
            ("target", typeof(DynamicGraphMeasurementTarget)),
            ("subject", typeof(DynamicGraphAccuracyMeasurement)),
            ("ablated", typeof(DynamicGraphAccuracyMeasurement)));
        AssertMethodShape(
            methods[2],
            "MeasureAccuracy",
            typeof(OperationResult<DynamicGraphTargetAccuracyMeasurement>),
            ("target", typeof(DynamicGraphMeasurementTarget)),
            ("liveMarginAccuracy", typeof(double)),
            ("before", typeof(LearningEvaluationResult)),
            ("after", typeof(LearningEvaluationResult)),
            ("splitProof", typeof(DynamicGraphMeasurementSplitProof)));
        AssertMethodShape(
            methods[3],
            "MeasureEdgeAuthorityControl",
            typeof(OperationResult<DynamicGraphTargetEdgeAuthorityControlMeasurement>),
            ("target", typeof(DynamicGraphMeasurementTarget)),
            ("result", typeof(EdgeAuthorityExecutionResult)));
        AssertMethodShape(
            methods[4],
            "MeasureEdgeAuthoritySelectionQualityEvidence",
            typeof(OperationResult<
                DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>),
            ("target", typeof(DynamicGraphMeasurementTarget)),
            ("authorityResult", typeof(EdgeAuthorityExecutionResult)),
            ("noveltyRequest", typeof(DynamicGraphEdgeAuthorityNoveltyEvaluationRequest)));
        AssertMethodShape(
            methods[5],
            "MeasureEdgeIdentityQuality",
            typeof(OperationResult<DynamicGraphTargetEdgeIdentityQualityMeasurement>),
            ("target", typeof(DynamicGraphMeasurementTarget)),
            ("result", typeof(EdgeAuthorityExecutionResult)));
        AssertMethodShape(
            methods[6],
            "MeasureEdgeOrderQuality",
            typeof(OperationResult<DynamicGraphTargetEdgeOrderQualityMeasurement>),
            ("target", typeof(DynamicGraphMeasurementTarget)),
            ("result", typeof(EdgeAuthorityExecutionResult)));
        AssertMethodShape(
            methods[7],
            "MeasureEdgeQualityEvidence",
            typeof(OperationResult<DynamicGraphTargetEdgeQualityEvidenceMeasurement>),
            ("target", typeof(DynamicGraphMeasurementTarget)),
            ("authorityResult", typeof(EdgeAuthorityExecutionResult)),
            ("noveltyObservation", typeof(DynamicGraphNoveltyObservation)),
            ("subject", typeof(DynamicGraphAccuracyMeasurement)),
            ("ablated", typeof(DynamicGraphAccuracyMeasurement)));
        AssertMethodShape(
            methods[8],
            "MeasureNovelty",
            typeof(OperationResult<DynamicGraphTargetNoveltyMeasurement>),
            ("target", typeof(DynamicGraphMeasurementTarget)),
            ("observation", typeof(DynamicGraphNoveltyObservation)));
        AssertMethodShape(
            methods[9],
            "MeasureRunSpeed",
            typeof(OperationResult<DynamicGraphTargetRunSpeedMeasurement>),
            ("target", typeof(DynamicGraphMeasurementTarget)),
            ("observation", typeof(DynamicGraphRunSpeedObservation)));
        AssertMethodShape(
            methods[10],
            "MeasureTrainingSpeed",
            typeof(OperationResult<DynamicGraphTargetTrainingSpeedMeasurement>),
            ("target", typeof(DynamicGraphMeasurementTarget)),
            ("result", typeof(LearningBatchResult)));
    }

    [Fact]
    public void Coordinator_PublicSurfaceContainsNoUnplannedConcerns()
    {
        var surface = string.Join(
            " ",
            typeof(DynamicGraphMeasurementCoordinator)
                .GetMembers(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Select(member => member.ToString()));
        var forbiddenTerms = new[]
        {
            "sourceid", "lifecycle", "dpo",
            "checkpoint", "promotion", "reflection", "heuristic"
        };

        foreach (var forbiddenTerm in forbiddenTerms)
            Assert.DoesNotContain(forbiddenTerm, surface, StringComparison.OrdinalIgnoreCase);

        var methods = typeof(DynamicGraphMeasurementCoordinator).GetMethods(
            BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
        Assert.DoesNotContain(methods, method => method.Name is "TrainBatchAsync" or "LearningBatch" or "ILearningMachine");
        Assert.DoesNotContain(
            methods.SelectMany(method => method.GetParameters()),
            parameter => parameter.ParameterType.IsGenericType
                         && parameter.ParameterType.GetGenericTypeDefinition() == typeof(LearningBatch<>));
        Assert.DoesNotContain(
            methods.SelectMany(method => method.GetParameters()).Select(parameter => parameter.ParameterType),
            parameterType => parameterType.IsGenericType
                             && (parameterType.GetGenericTypeDefinition() == typeof(ILearningMachine<>)
                                  || parameterType.GetGenericTypeDefinition() == typeof(ILearningMachinePair<,>)));

        var bindMethod = Assert.Single(methods, method =>
            method.Name == nameof(DynamicGraphMeasurementCoordinator.BindL2UntrainedHonesty));
        var bindSurface = string.Join(
            " ",
            bindMethod.Name,
            bindMethod.ReturnType,
            string.Join(" ", bindMethod.GetParameters().Select(parameter => parameter.ParameterType)));
        foreach (var forbiddenBindTerm in new[]
                 {
                     "TrainBatch", "Training", "Checkpoint", "Lifecycle", "Promotion",
                     "FinalPass", "ReadyForPromotion", "Dpo", "Request", "Resolved",
                     "Predicate", "Token"
                 })
        {
            Assert.DoesNotContain(forbiddenBindTerm, bindSurface, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task BindL2UntrainedHonesty_BindsRealPromptToTaskProbeToExactL2Target()
    {
        var machine = new PromptToTaskMachine();
        var unit = new PromptToTaskIntentUnit(machine);
        var registry = new InMemoryLogicalUnitRegistry();
        var registered = registry.Register<PromptToTaskRequest, PromptToTaskResponse>(
            PromptToTaskIntentUnit.UnitId,
            PromptToTaskIntentUnit.Role,
            PromptToTaskMachine.DomainTag,
            "coordinator-bind:v1",
            LogicalUnitTrustState.Candidate,
            isLearned: false,
            unit);
        Assert.True(registered.IsSuccess, registered.ErrorMessage);
        var resolved = registry.ResolveLatestEntry<PromptToTaskRequest, PromptToTaskResponse>(
            PromptToTaskIntentUnit.Role,
            PromptToTaskMachine.DomainTag,
            LogicalUnitTrustState.Candidate);
        Assert.True(resolved.IsSuccess, resolved.ErrorMessage);

        var probe = await DynamicGraphUntrainedHonestyL2Probe.MeasureAsync(
            "evaluation:coordinator-bind:001",
            resolved.Value,
            new LogicalUnitRequest<PromptToTaskRequest>(
                "run:coordinator-bind:001",
                PromptToTaskIntentUnit.UnitId,
                PromptToTaskIntentUnit.Role,
                PromptToTaskMachine.DomainTag,
                CreateCoordinatorPromptToTaskRequest(),
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
        Assert.True(probe.IsSuccess, probe.ErrorMessage);

        var target = Target(
            PromptToTaskIntentUnit.UnitId,
            DynamicGraphMeasurementTargetLevel.L2LogicalUnit);
        var result = DynamicGraphMeasurementCoordinator.BindL2UntrainedHonesty(
            target,
            probe.Value);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.NotSame(target, result.Value!.Target);
        Assert.Equal(target, result.Value.Target);
        Assert.NotSame(probe.Value, result.Value.UntrainedHonesty);
        Assert.NotSame(probe.Value!.Producer, result.Value.UntrainedHonesty.Producer);
        Assert.NotSame(probe.Value.Honesty, result.Value.UntrainedHonesty.Honesty);
        Assert.Equal(
            PromptToTaskIntentUnit.UnitId,
            result.Value.UntrainedHonesty.Producer.UnitId,
            StringComparer.Ordinal);
        Assert.True(result.Value.UntrainedHonesty.Honesty.MeetsUntrainedHonestyGate);
        AssertReadOnly(result.Value.UntrainedHonesty.Honesty.RuntimeEvidence);
        AssertReadOnly(result.Value.UntrainedHonesty.Honesty.InspectedTypeNames);
        AssertReadOnly(result.Value.UntrainedHonesty.Honesty.ForbiddenShortcutMatches);
    }

    [Theory]
    [InlineData("untrained-honesty-l2-unit-other")]
    [InlineData("UNTRAINED-HONESTY-L2-UNIT")]
    public void BindL2UntrainedHonesty_RequiresOrdinalTargetProducerMatch(string targetId)
    {
        AssertUntrainedHonestyFailure(
            DynamicGraphMeasurementCoordinator.BindL2UntrainedHonesty(
                Target(targetId, DynamicGraphMeasurementTargetLevel.L2LogicalUnit),
                UntrainedHonestyProbeResult()),
            UntrainedHonestyTargetMismatchCode,
            UntrainedHonestyTargetMismatchMessage);
    }

    [Theory]
    [InlineData(DynamicGraphMeasurementTargetLevel.Pair)]
    [InlineData(DynamicGraphMeasurementTargetLevel.L3LogicalFlow)]
    [InlineData(DynamicGraphMeasurementTargetLevel.L4LogicalOrchestrator)]
    [InlineData(DynamicGraphMeasurementTargetLevel.L5LearningMachine)]
    public void BindL2UntrainedHonesty_NonL2LevelPrecedesHostileAndNullProbe(
        DynamicGraphMeasurementTargetLevel level)
    {
        foreach (var probe in new DynamicGraphUntrainedHonestyL2ProbeResult?[]
                 {
                     null,
                     HostileUntrainedHonestyProbeResult(
                         new BadImageFormatException("must not traverse"))
                 })
        {
            AssertUntrainedHonestyFailure(
                DynamicGraphMeasurementCoordinator.BindL2UntrainedHonesty(
                    Target(level: level),
                    probe),
                InvalidUntrainedHonestyTargetLevelCode,
                InvalidUntrainedHonestyTargetLevelMessage);
        }
    }

    [Theory]
    [InlineData("null")]
    [InlineData("malformed")]
    [InlineData("shortcut")]
    [InlineData("deterministic")]
    public void BindL2UntrainedHonesty_TargetGuardPrecedesHostileProbe(string targetCase)
    {
        var target = targetCase switch
        {
            "null" => null,
            "malformed" => Target(" malformed", DynamicGraphMeasurementTargetLevel.L2LogicalUnit),
            "shortcut" => Target(
                UntrainedHonestyUnitId,
                DynamicGraphMeasurementTargetLevel.L2LogicalUnit,
                DynamicGraphMeasurementMechanismKind.Knn),
            "deterministic" => Target(
                UntrainedHonestyUnitId,
                DynamicGraphMeasurementTargetLevel.L2LogicalUnit,
                deterministicFinalAuthority: true),
            _ => throw new ArgumentOutOfRangeException(nameof(targetCase))
        };

        var result = DynamicGraphMeasurementCoordinator.BindL2UntrainedHonesty(
            target,
            HostileUntrainedHonestyProbeResult(
                new BadImageFormatException("must not traverse")));

        AssertUntrainedHonestyFailure(
            result,
            targetCase is "shortcut" or "deterministic"
                ? "FORBIDDEN_RUNTIME_SHORTCUT"
                : "INVALID_DYNAMIC_GRAPH_MEASUREMENT_TARGET",
            targetCase is "shortcut" or "deterministic"
                ? "Dynamic graph measurement target is a forbidden runtime shortcut."
                : "Dynamic graph measurement target is invalid.");
    }

    [Fact]
    public void BindL2UntrainedHonesty_MalformedProbePrecedesTargetMismatch()
    {
        var malformed = UntrainedHonestyProbeResult(
            producer: UntrainedHonestyProducer(unitRole: " malformed"));

        AssertUntrainedHonestyFailure(
            DynamicGraphMeasurementCoordinator.BindL2UntrainedHonesty(
                Target("different", DynamicGraphMeasurementTargetLevel.L2LogicalUnit),
                malformed),
            InvalidUntrainedHonestyProbeResultCode,
            InvalidUntrainedHonestyProbeResultMessage);
    }

    [Theory]
    [InlineData("UnitId")]
    [InlineData("UnitRole")]
    [InlineData("DomainTag")]
    [InlineData("Version")]
    [InlineData("ProducerTypeName")]
    [InlineData("InputTypeName")]
    [InlineData("OutputTypeName")]
    public void BindL2UntrainedHonesty_RejectsEachNonCanonicalProducerIdentity(string field)
    {
        var producer = UntrainedHonestyProducer();
        producer = field switch
        {
            "UnitId" => producer with { UnitId = " invalid" },
            "UnitRole" => producer with { UnitRole = " invalid" },
            "DomainTag" => producer with { DomainTag = " invalid" },
            "Version" => producer with { Version = " invalid" },
            "ProducerTypeName" => producer with { ProducerTypeName = " invalid" },
            "InputTypeName" => producer with { InputTypeName = " invalid" },
            "OutputTypeName" => producer with { OutputTypeName = " invalid" },
            _ => throw new ArgumentOutOfRangeException(nameof(field))
        };

        AssertInvalidUntrainedHonestyProbe(UntrainedHonestyProbeResult(producer: producer));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void BindL2UntrainedHonesty_RejectsNonPositiveProducerRegistrationOrder(long order)
        => AssertInvalidUntrainedHonestyProbe(UntrainedHonestyProbeResult(
            producer: UntrainedHonestyProducer(registrationOrder: order)));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" invalid")]
    [InlineData("invalid\u0001value")]
    public void BindL2UntrainedHonesty_RejectsNonCanonicalEvaluationId(string? evaluationId)
        => AssertInvalidUntrainedHonestyProbe(UntrainedHonestyProbeResult(
            honesty: UntrainedHonestyMeasurement() with { EvaluationId = evaluationId! }));

    [Theory]
    [InlineData("Producer")]
    [InlineData("Honesty")]
    public void BindL2UntrainedHonesty_RejectsNullNestedEvidence(string nested)
    {
        var probe = nested == "Producer"
            ? new DynamicGraphUntrainedHonestyL2ProbeResult(null!, UntrainedHonestyMeasurement())
            : new DynamicGraphUntrainedHonestyL2ProbeResult(UntrainedHonestyProducer(), null!);

        AssertInvalidUntrainedHonestyProbe(probe);
    }

    [Theory]
    [InlineData("RuntimeEvidence")]
    [InlineData("InspectedTypeNames")]
    [InlineData("ForbiddenShortcutMatches")]
    public void BindL2UntrainedHonesty_RejectsNullCarrier(string carrier)
    {
        var honesty = UntrainedHonestyMeasurement();
        honesty = carrier switch
        {
            "RuntimeEvidence" => honesty with { RuntimeEvidence = null! },
            "InspectedTypeNames" => honesty with { InspectedTypeNames = null! },
            "ForbiddenShortcutMatches" => honesty with { ForbiddenShortcutMatches = null! },
            _ => throw new ArgumentOutOfRangeException(nameof(carrier))
        };

        AssertInvalidUntrainedHonestyProbe(UntrainedHonestyProbeResult(honesty: honesty));
    }

    [Theory]
    [InlineData("RuntimeEvidence")]
    [InlineData("InspectedTypeNames")]
    [InlineData("ForbiddenShortcutMatches")]
    public void BindL2UntrainedHonesty_RejectsNonCanonicalValueInEachCarrier(string carrier)
    {
        var honesty = UntrainedHonestyMeasurement(
            runtimeEvidence: carrier == "RuntimeEvidence"
                ? [" invalid", NoSemanticClassification, NoTrainedState]
                : ValidUntrainedRuntimeEvidence(),
            inspectedTypeNames: carrier == "InspectedTypeNames"
                ? [" invalid", "Producer.Type"]
                : ValidInspectedTypeNames(),
            forbiddenShortcutMatches: carrier == "ForbiddenShortcutMatches" ? [" invalid"] : []);
        if (carrier == "ForbiddenShortcutMatches")
            honesty = honesty with { MeetsUntrainedHonestyGate = false };

        AssertInvalidUntrainedHonestyProbe(UntrainedHonestyProbeResult(honesty: honesty));
    }

    [Theory]
    [InlineData("RuntimeEvidence", "unsorted")]
    [InlineData("RuntimeEvidence", "duplicate")]
    [InlineData("InspectedTypeNames", "unsorted")]
    [InlineData("InspectedTypeNames", "duplicate")]
    [InlineData("ForbiddenShortcutMatches", "unsorted")]
    [InlineData("ForbiddenShortcutMatches", "duplicate")]
    public void BindL2UntrainedHonesty_RejectsUnsortedAndDuplicateCarriers(
        string carrier,
        string corruption)
    {
        string[] bad = (carrier, corruption) switch
        {
            ("RuntimeEvidence", "unsorted") =>
                [NoTrainedState, NoSemanticClassification, "RUNTIME_REPORTED_NOT_READY"],
            ("RuntimeEvidence", "duplicate") =>
                [NoSemanticClassification, NoTrainedState, NoTrainedState],
            ("InspectedTypeNames", "unsorted") => ["Supporting.Type", "Producer.Type"],
            ("InspectedTypeNames", "duplicate") => ["Producer.Type", "Producer.Type"],
            (_, "unsorted") => ["z-value", "a-value"],
            _ => ["same-value", "same-value"]
        };
        var honesty = UntrainedHonestyMeasurement(
            runtimeEvidence: carrier == "RuntimeEvidence" ? bad : ValidUntrainedRuntimeEvidence(),
            inspectedTypeNames: carrier == "InspectedTypeNames" ? bad : ValidInspectedTypeNames(),
            forbiddenShortcutMatches: carrier == "ForbiddenShortcutMatches" ? bad : []);
        if (carrier == "ForbiddenShortcutMatches")
            honesty = honesty with { MeetsUntrainedHonestyGate = false };

        AssertInvalidUntrainedHonestyProbe(UntrainedHonestyProbeResult(honesty: honesty));
    }

    [Fact]
    public void BindL2UntrainedHonesty_RequiresNonEmptyInspectionAndExactProducerType()
    {
        foreach (var inspectedTypes in new IReadOnlyList<string>[]
                 {
                     [],
                     ["Producer.Type.CaseChanged"],
                     ["Supporting.Type"]
                 })
        {
            AssertInvalidUntrainedHonestyProbe(UntrainedHonestyProbeResult(
                honesty: UntrainedHonestyMeasurement(inspectedTypeNames: inspectedTypes)));
        }
    }

    [Theory]
    [InlineData("trained-state")]
    [InlineData("semantic-classification")]
    [InlineData("gate")]
    public void BindL2UntrainedHonesty_RejectsEachInvertedDerivedBoolean(string derived)
    {
        var honesty = UntrainedHonestyMeasurement(
            hasNoTrainedStateEvidence: derived != "trained-state",
            hasNoSemanticClassificationEvidence: derived != "semantic-classification",
            meetsUntrainedHonestyGate: derived != "gate");

        AssertInvalidUntrainedHonestyProbe(UntrainedHonestyProbeResult(honesty: honesty));
    }

    [Theory]
    [InlineData("trained-state")]
    [InlineData("semantic-classification")]
    public void BindL2UntrainedHonesty_UsesExactOrdinalEvidenceTokens(string token)
    {
        var evidence = token == "trained-state"
            ? new[] { NoSemanticClassification, "no_trained_state_loaded" }
            : new[] { "no_semantic_classification_performed", NoTrainedState };
        Array.Sort(evidence, StringComparer.Ordinal);
        var honesty = UntrainedHonestyMeasurement(
            runtimeEvidence: evidence,
            hasNoTrainedStateEvidence: true,
            hasNoSemanticClassificationEvidence: true,
            meetsUntrainedHonestyGate: true);

        AssertInvalidUntrainedHonestyProbe(UntrainedHonestyProbeResult(honesty: honesty));
    }

    [Fact]
    public void BindL2UntrainedHonesty_ValidFalseGateBindsSuccessfully()
    {
        var result = DynamicGraphMeasurementCoordinator.BindL2UntrainedHonesty(
            Target(UntrainedHonestyUnitId, DynamicGraphMeasurementTargetLevel.L2LogicalUnit),
            UntrainedHonestyProbeResult(
                honesty: UntrainedHonestyMeasurement(
                    runtimeReportedNotReady: false,
                    meetsUntrainedHonestyGate: false)));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.False(result.Value!.UntrainedHonesty.Honesty.RuntimeReportedNotReady);
        Assert.False(result.Value.UntrainedHonesty.Honesty.MeetsUntrainedHonestyGate);
    }

    [Fact]
    public void BindL2UntrainedHonesty_SnapshotsCarriersInDeterministicContractOrder()
    {
        var reads = new List<string>();
        var honesty = UntrainedHonestyMeasurement(
            runtimeEvidence: new RecordingReadOnlyList<string>(
                "RuntimeEvidence",
                ValidUntrainedRuntimeEvidence(),
                reads),
            inspectedTypeNames: new RecordingReadOnlyList<string>(
                "InspectedTypeNames",
                ValidInspectedTypeNames(),
                reads),
            forbiddenShortcutMatches: new RecordingReadOnlyList<string>(
                "ForbiddenShortcutMatches",
                [],
                reads));

        var result = DynamicGraphMeasurementCoordinator.BindL2UntrainedHonesty(
            Target(UntrainedHonestyUnitId, DynamicGraphMeasurementTargetLevel.L2LogicalUnit),
            UntrainedHonestyProbeResult(honesty: honesty));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(
            ["RuntimeEvidence", "InspectedTypeNames", "ForbiddenShortcutMatches"],
            reads);
    }

    [Fact]
    public void BindL2UntrainedHonesty_SnapshotsWholeProbeBeforeScalarValidation()
    {
        var reads = new List<string>();
        var honesty = UntrainedHonestyMeasurement(
            runtimeEvidence: new RecordingReadOnlyList<string>(
                "RuntimeEvidence",
                ValidUntrainedRuntimeEvidence(),
                reads),
            inspectedTypeNames: new RecordingReadOnlyList<string>(
                "InspectedTypeNames",
                ValidInspectedTypeNames(),
                reads),
            forbiddenShortcutMatches: new RecordingReadOnlyList<string>(
                "ForbiddenShortcutMatches",
                [],
                reads));

        AssertInvalidUntrainedHonestyProbe(UntrainedHonestyProbeResult(
            producer: UntrainedHonestyProducer(unitRole: " malformed"),
            honesty: honesty));
        Assert.Equal(
            ["RuntimeEvidence", "InspectedTypeNames", "ForbiddenShortcutMatches"],
            reads);
    }

    [Theory]
    [InlineData("RuntimeEvidence")]
    [InlineData("InspectedTypeNames")]
    [InlineData("ForbiddenShortcutMatches")]
    public void BindL2UntrainedHonesty_OrdinaryCarrierFailureMapsToLocalInvalid(string carrier)
    {
        var ordinary = new InvalidOperationException($"ordinary-{carrier}");
        var honesty = UntrainedHonestyMeasurement(
            runtimeEvidence: carrier == "RuntimeEvidence"
                ? new FaultingReadOnlyList<string>(ordinary)
                : ValidUntrainedRuntimeEvidence(),
            inspectedTypeNames: carrier == "InspectedTypeNames"
                ? new FaultingReadOnlyList<string>(ordinary)
                : ValidInspectedTypeNames(),
            forbiddenShortcutMatches: carrier == "ForbiddenShortcutMatches"
                ? new FaultingReadOnlyList<string>(ordinary)
                : []);

        AssertInvalidUntrainedHonestyProbe(UntrainedHonestyProbeResult(honesty: honesty));
    }

    public static TheoryData<string, string> UntrainedHonestyExceptionBoundaryCases()
    {
        var cases = new TheoryData<string, string>();
        foreach (var carrier in new[]
                 {
                     "RuntimeEvidence", "InspectedTypeNames", "ForbiddenShortcutMatches"
                 })
        {
            foreach (var graph in new[]
                     {
                         "direct-cancellation", "wrapped-cancellation", "aggregate-cancellation",
                         "direct-fatal", "wrapped-fatal", "aggregate-fatal"
                     })
            {
                cases.Add(carrier, graph);
            }
        }

        return cases;
    }

    [Theory]
    [MemberData(nameof(UntrainedHonestyExceptionBoundaryCases))]
    public void BindL2UntrainedHonesty_CancellationAndFatalCarrierGraphsEscapeAsSameOuterInstance(
        string carrier,
        string graph)
    {
        var outer = UntrainedHonestyExceptionGraph(graph);
        var honesty = UntrainedHonestyMeasurement(
            runtimeEvidence: carrier == "RuntimeEvidence"
                ? new FaultingReadOnlyList<string>(outer)
                : ValidUntrainedRuntimeEvidence(),
            inspectedTypeNames: carrier == "InspectedTypeNames"
                ? new FaultingReadOnlyList<string>(outer)
                : ValidInspectedTypeNames(),
            forbiddenShortcutMatches: carrier == "ForbiddenShortcutMatches"
                ? new FaultingReadOnlyList<string>(outer)
                : []);

        var thrown = Assert.ThrowsAny<Exception>(() =>
            DynamicGraphMeasurementCoordinator.BindL2UntrainedHonesty(
                Target(UntrainedHonestyUnitId, DynamicGraphMeasurementTargetLevel.L2LogicalUnit),
                UntrainedHonestyProbeResult(honesty: honesty)));

        Assert.Same(outer, thrown);
    }

    [Fact]
    public void BindL2UntrainedHonesty_OwnsAllOutputsAndRejectsCallerMutation()
    {
        var runtimeEvidence = ValidUntrainedRuntimeEvidence().ToList();
        var inspectedTypeNames = ValidInspectedTypeNames().ToList();
        var forbiddenShortcutMatches = new List<string>();
        var producer = UntrainedHonestyProducer();
        var honesty = UntrainedHonestyMeasurement(
            runtimeEvidence: runtimeEvidence,
            inspectedTypeNames: inspectedTypeNames,
            forbiddenShortcutMatches: forbiddenShortcutMatches);
        var probe = UntrainedHonestyProbeResult(producer, honesty);
        var target = Target(
            UntrainedHonestyUnitId,
            DynamicGraphMeasurementTargetLevel.L2LogicalUnit);

        var result = DynamicGraphMeasurementCoordinator.BindL2UntrainedHonesty(target, probe);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.NotSame(target, result.Value!.Target);
        Assert.NotSame(probe, result.Value.UntrainedHonesty);
        Assert.NotSame(producer, result.Value.UntrainedHonesty.Producer);
        Assert.NotSame(honesty, result.Value.UntrainedHonesty.Honesty);

        runtimeEvidence[0] = "CALLER_MUTATED_RUNTIME";
        inspectedTypeNames[0] = "Caller.Mutated.Type";
        forbiddenShortcutMatches.Add("CALLER_MUTATED_MATCH");
        Assert.Equal(NoSemanticClassification, result.Value.UntrainedHonesty.Honesty.RuntimeEvidence[0]);
        Assert.Equal("Producer.Type", result.Value.UntrainedHonesty.Honesty.InspectedTypeNames[0]);
        Assert.Empty(result.Value.UntrainedHonesty.Honesty.ForbiddenShortcutMatches);
        AssertReadOnly(result.Value.UntrainedHonesty.Honesty.RuntimeEvidence);
        AssertReadOnly(result.Value.UntrainedHonesty.Honesty.InspectedTypeNames);
        AssertReadOnly(result.Value.UntrainedHonesty.Honesty.ForbiddenShortcutMatches);
    }

    [Fact]
    public async Task BindL2UntrainedHonesty_DoesNotReexecuteProbeUnitOrPredicate()
    {
        var unit = new CountingUntrainedHonestyUnit();
        var descriptor = new LogicalUnitDescriptor(
            UntrainedHonestyUnitId,
            "untrained-honesty-role",
            "untrained-honesty-domain",
            "untrained-honesty:v1",
            LogicalUnitTrustState.Candidate,
            IsLearned: false,
            typeof(string),
            typeof(string),
            RegistrationOrder: 7);
        var predicateCount = 0;
        var probe = await DynamicGraphUntrainedHonestyL2Probe.MeasureAsync(
            "evaluation:coordinator-counting:001",
            new ResolvedLogicalUnitRegistryEntry<string, string>(descriptor, unit),
            new LogicalUnitRequest<string>(
                "run:coordinator-counting:001",
                descriptor.UnitId,
                descriptor.UnitRole,
                descriptor.DomainTag,
                "input",
                new LogicalExecutionFlags(),
                new Dictionary<string, object>()),
            _ =>
            {
                predicateCount++;
                return true;
            },
            []);
        Assert.True(probe.IsSuccess, probe.ErrorMessage);
        Assert.Equal(1, unit.ExecutionCount);
        Assert.Equal(1, predicateCount);

        var target = Target(
            UntrainedHonestyUnitId,
            DynamicGraphMeasurementTargetLevel.L2LogicalUnit);
        var first = DynamicGraphMeasurementCoordinator.BindL2UntrainedHonesty(target, probe.Value);
        var second = DynamicGraphMeasurementCoordinator.BindL2UntrainedHonesty(target, probe.Value);

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.Equal(1, unit.ExecutionCount);
        Assert.Equal(1, predicateCount);
    }

    [Theory]
    [InlineData(DynamicGraphMeasurementTargetLevel.L2LogicalUnit)]
    [InlineData(DynamicGraphMeasurementTargetLevel.L3LogicalFlow)]
    public void MeasureAblation_AdmittedLevelReturnsOwnedTargetAndExactMeterEvidence(
        DynamicGraphMeasurementTargetLevel level)
    {
        var target = Target("target-different-from-evaluation", level);
        var trainRoots = new List<string> { "train-a", "train-b" };
        var evalRoots = new List<string> { "eval-a", "eval-b" };
        var hashes = new List<string> { "hash-a", "hash-b" };
        var callerProof = AblationProof(trainRoots, evalRoots, hashes);
        var subject = AblationAccuracy(heldOutAccuracy: 0.96d, splitProof: callerProof);
        var ablated = AblationAccuracy(heldOutAccuracy: 0.93d);
        var direct = DynamicGraphAblationMeter.Measure(subject, ablated);

        var result = DynamicGraphMeasurementCoordinator.MeasureAblation(target, subject, ablated);

        Assert.True(direct.IsSuccess, direct.ErrorMessage);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.NotSame(target, result.Value!.Target);
        Assert.Equal(target, result.Value.Target);
        AssertAblationEquivalent(direct.Value!, result.Value.Ablation);
        Assert.NotSame(callerProof, result.Value.Ablation.SplitProof);

        trainRoots[0] = "mutated-train";
        evalRoots[0] = "mutated-eval";
        hashes[0] = "mutated-hash";
        Assert.Equal("train-a", result.Value.Ablation.SplitProof.TrainSourceRoots[0]);
        Assert.Equal("eval-a", result.Value.Ablation.SplitProof.EvalSourceRoots[0]);
        Assert.Equal("hash-a", result.Value.Ablation.SplitProof.SourceHashes[0]);
        AssertReadOnly(result.Value.Ablation.SplitProof.TrainSourceRoots);
        AssertReadOnly(result.Value.Ablation.SplitProof.EvalSourceRoots);
        AssertReadOnly(result.Value.Ablation.SplitProof.SourceHashes);
    }

    [Fact]
    public void MeasureAblation_DifferentTargetAndEvaluationIdsAreExplicitlyAllowed()
    {
        var result = DynamicGraphMeasurementCoordinator.MeasureAblation(
            Target("logical-unit-42", DynamicGraphMeasurementTargetLevel.L2LogicalUnit),
            AblationAccuracy(evaluationId: "held-out-evaluation-17", heldOutAccuracy: 0.96d),
            AblationAccuracy(evaluationId: "held-out-evaluation-17", heldOutAccuracy: 0.93d));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("logical-unit-42", result.Value!.Target.TargetId, StringComparer.Ordinal);
        Assert.Equal("held-out-evaluation-17", result.Value.Ablation.EvaluationId, StringComparer.Ordinal);
    }

    [Theory]
    [InlineData(DynamicGraphMeasurementMechanismKind.Knn)]
    [InlineData(DynamicGraphMeasurementMechanismKind.Scorer)]
    [InlineData(DynamicGraphMeasurementMechanismKind.Counter)]
    [InlineData(DynamicGraphMeasurementMechanismKind.Regex)]
    public void MeasureAblation_ShortcutTargetPrecedesHostileEvidence(
        DynamicGraphMeasurementMechanismKind mechanismKind)
    {
        var result = DynamicGraphMeasurementCoordinator.MeasureAblation(
            Target(
                level: DynamicGraphMeasurementTargetLevel.L2LogicalUnit,
                mechanismKind: mechanismKind),
            HostileAblationAccuracy(new BadImageFormatException("must not traverse")),
            HostileAblationAccuracy(new OperationCanceledException("must not traverse")));

        AssertAblationFailure(
            result,
            "FORBIDDEN_RUNTIME_SHORTCUT",
            "Dynamic graph measurement target is a forbidden runtime shortcut.");
    }

    [Fact]
    public void MeasureAblation_DeterministicAuthorityPrecedesHostileEvidence()
    {
        var result = DynamicGraphMeasurementCoordinator.MeasureAblation(
            Target(
                level: DynamicGraphMeasurementTargetLevel.L3LogicalFlow,
                deterministicFinalAuthority: true),
            HostileAblationAccuracy(new OperationCanceledException("must not traverse")),
            HostileAblationAccuracy(new BadImageFormatException("must not traverse")));

        AssertAblationFailure(
            result,
            "FORBIDDEN_RUNTIME_SHORTCUT",
            "Dynamic graph measurement target is a forbidden runtime shortcut.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" malformed")]
    public void MeasureAblation_MalformedTargetPrecedesHostileEvidence(string? targetId)
    {
        var target = targetId is null
            ? null
            : Target(targetId, DynamicGraphMeasurementTargetLevel.L2LogicalUnit);

        var result = DynamicGraphMeasurementCoordinator.MeasureAblation(
            target,
            HostileAblationAccuracy(new BadImageFormatException("must not traverse")),
            HostileAblationAccuracy(new OperationCanceledException("must not traverse")));

        AssertAblationFailure(
            result,
            "INVALID_DYNAMIC_GRAPH_MEASUREMENT_TARGET",
            "Dynamic graph measurement target is invalid.");
    }

    [Theory]
    [InlineData(DynamicGraphMeasurementTargetLevel.Pair)]
    [InlineData(DynamicGraphMeasurementTargetLevel.L4LogicalOrchestrator)]
    [InlineData(DynamicGraphMeasurementTargetLevel.L5LearningMachine)]
    public void MeasureAblation_NonAblatableLevelPrecedesHostileEvidence(
        DynamicGraphMeasurementTargetLevel level)
    {
        var result = DynamicGraphMeasurementCoordinator.MeasureAblation(
            Target(level: level),
            HostileAblationAccuracy(new BadImageFormatException("must not traverse")),
            HostileAblationAccuracy(new OperationCanceledException("must not traverse")));

        AssertAblationFailure(result, InvalidAblationTargetLevelCode, InvalidAblationTargetLevelMessage);
    }

    [Theory]
    [InlineData("subject")]
    [InlineData("ablated")]
    public void MeasureAblation_AdmittedTargetPreservesNullEvidenceError(string position)
    {
        var result = DynamicGraphMeasurementCoordinator.MeasureAblation(
            Target(level: DynamicGraphMeasurementTargetLevel.L2LogicalUnit),
            position == "subject" ? null : AblationAccuracy(),
            position == "ablated" ? null : AblationAccuracy());

        AssertAblationFailure(
            result,
            "INVALID_ABLATION_MEASUREMENT",
            "Dynamic graph ablation measurement is invalid.");
    }

    [Theory]
    [InlineData("subject")]
    [InlineData("ablated")]
    public void MeasureAblation_AdmittedTargetPreservesInvalidShapeError(string position)
    {
        var invalid = AblationAccuracy(heldOutAccuracy: double.NaN);
        var result = DynamicGraphMeasurementCoordinator.MeasureAblation(
            Target(level: DynamicGraphMeasurementTargetLevel.L3LogicalFlow),
            position == "subject" ? invalid : AblationAccuracy(),
            position == "ablated" ? invalid : AblationAccuracy());

        AssertAblationFailure(
            result,
            "INVALID_ABLATION_MEASUREMENT",
            "Dynamic graph ablation measurement is invalid.");
    }

    [Theory]
    [InlineData("invalid", "INVALID_SPLIT_PROOF", "Dynamic graph measurement split proof is invalid.")]
    [InlineData("overlap", "TRAIN_HELDOUT_OVERLAP", "Training and held-out source roots overlap.")]
    [InlineData("near-duplicate", "TRAIN_HELDOUT_NEAR_DUPLICATE", "Training and held-out sources contain near-duplicates.")]
    public void MeasureAblation_AdmittedTargetPreservesSubjectSplitErrors(
        string condition,
        string code,
        string message)
    {
        var proof = condition switch
        {
            "overlap" => AblationProof(trainRoots: ["shared"], evalRoots: ["shared"]),
            "near-duplicate" => AblationProof(nearDuplicateCount: 1),
            _ => AblationProof(method: "SimHash")
        };
        var result = DynamicGraphMeasurementCoordinator.MeasureAblation(
            Target(level: DynamicGraphMeasurementTargetLevel.L2LogicalUnit),
            AblationAccuracy(splitProof: proof),
            HostileAblationAccuracy(new BadImageFormatException("must not traverse")));

        AssertAblationFailure(result, code, message);
    }

    [Theory]
    [InlineData("invalid", "INVALID_SPLIT_PROOF", "Dynamic graph measurement split proof is invalid.")]
    [InlineData("overlap", "TRAIN_HELDOUT_OVERLAP", "Training and held-out source roots overlap.")]
    [InlineData("near-duplicate", "TRAIN_HELDOUT_NEAR_DUPLICATE", "Training and held-out sources contain near-duplicates.")]
    public void MeasureAblation_AdmittedTargetPreservesAblatedSplitErrors(
        string condition,
        string code,
        string message)
    {
        var proof = condition switch
        {
            "overlap" => AblationProof(trainRoots: ["shared"], evalRoots: ["shared"]),
            "near-duplicate" => AblationProof(nearDuplicateCount: 1),
            _ => AblationProof(method: "SimHash")
        };
        var result = DynamicGraphMeasurementCoordinator.MeasureAblation(
            Target(level: DynamicGraphMeasurementTargetLevel.L3LogicalFlow),
            AblationAccuracy(),
            AblationAccuracy(splitProof: proof));

        AssertAblationFailure(result, code, message);
    }

    [Fact]
    public void MeasureAblation_SubjectAdmissionPrecedesAblatedTraversal()
    {
        var result = DynamicGraphMeasurementCoordinator.MeasureAblation(
            Target(level: DynamicGraphMeasurementTargetLevel.L3LogicalFlow),
            AblationAccuracy(splitProof: AblationProof(trainRoots: ["shared"], evalRoots: ["shared"])),
            HostileAblationAccuracy(new BadImageFormatException("must not traverse")));

        AssertAblationFailure(
            result,
            "TRAIN_HELDOUT_OVERLAP",
            "Training and held-out source roots overlap.");
    }

    [Fact]
    public void MeasureAblation_AdmittedTargetPreservesEvaluationMismatch()
    {
        var result = DynamicGraphMeasurementCoordinator.MeasureAblation(
            Target(level: DynamicGraphMeasurementTargetLevel.L2LogicalUnit),
            AblationAccuracy(evaluationId: "eval-subject"),
            AblationAccuracy(evaluationId: "eval-ablated"));

        AssertAblationFailure(
            result,
            "ABLATION_EVALUATION_MISMATCH",
            "Dynamic graph ablation measurements do not describe the same held-out evaluation.");
    }

    [Theory]
    [InlineData("subject", "ordinary")]
    [InlineData("subject", "aggregate-ordinary")]
    [InlineData("ablated", "ordinary")]
    [InlineData("ablated", "aggregate-ordinary")]
    public void MeasureAblation_AdmittedTargetMapsOrdinaryHostileEvidence(
        string position,
        string graph)
    {
        Exception exception = graph == "ordinary"
            ? new InvalidOperationException("ordinary")
            : new AggregateException(new InvalidOperationException("nested ordinary"));
        var hostile = HostileAblationAccuracy(exception);

        var result = DynamicGraphMeasurementCoordinator.MeasureAblation(
            Target(level: DynamicGraphMeasurementTargetLevel.L3LogicalFlow),
            position == "subject" ? hostile : AblationAccuracy(),
            position == "ablated" ? hostile : AblationAccuracy());

        AssertAblationFailure(
            result,
            "INVALID_SPLIT_PROOF",
            "Dynamic graph measurement split proof is invalid.");
    }

    [Theory]
    [InlineData("subject", "cancellation")]
    [InlineData("subject", "wrapped-cancellation")]
    [InlineData("subject", "aggregate-cancellation")]
    [InlineData("subject", "fatal")]
    [InlineData("subject", "wrapped-fatal")]
    [InlineData("subject", "aggregate-fatal")]
    [InlineData("ablated", "cancellation")]
    [InlineData("ablated", "wrapped-cancellation")]
    [InlineData("ablated", "aggregate-cancellation")]
    [InlineData("ablated", "fatal")]
    [InlineData("ablated", "wrapped-fatal")]
    [InlineData("ablated", "aggregate-fatal")]
    public void MeasureAblation_AdmittedTargetLetsCancellationAndFatalGraphsEscapeUnchanged(
        string position,
        string graph)
    {
        var exception = AblationExceptionGraph(graph);
        var hostile = HostileAblationAccuracy(exception);

        var thrown = Assert.Throws(exception.GetType(), () =>
            DynamicGraphMeasurementCoordinator.MeasureAblation(
                Target(level: DynamicGraphMeasurementTargetLevel.L2LogicalUnit),
                position == "subject" ? hostile : AblationAccuracy(),
                position == "ablated" ? hostile : AblationAccuracy()));

        Assert.Same(exception, thrown);
    }

    [Fact]
    public void MeasureAblation_CoordinatorBodyHasNoExceptionHandlingClauses()
    {
        var body = Assert.IsAssignableFrom<MethodBody>(
            typeof(DynamicGraphMeasurementCoordinator).GetMethod(
                nameof(DynamicGraphMeasurementCoordinator.MeasureAblation),
                BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)!
                .GetMethodBody());

        Assert.Empty(body.ExceptionHandlingClauses);
    }

    [Theory]
    [InlineData(0.95d, 0.93d)]
    [InlineData(0.949999999998d, 0.93d)]
    [InlineData(0.93d, 0.93d)]
    [InlineData(0.92d, 0.93d)]
    public void MeasureAblation_BoundaryBelowZeroAndNegativeExactlyMatchDirectMeter(
        double subjectAccuracy,
        double ablatedAccuracy)
    {
        var subject = AblationAccuracy(heldOutAccuracy: subjectAccuracy);
        var ablated = AblationAccuracy(heldOutAccuracy: ablatedAccuracy);
        var direct = DynamicGraphAblationMeter.Measure(subject, ablated);

        var result = DynamicGraphMeasurementCoordinator.MeasureAblation(
            Target(level: DynamicGraphMeasurementTargetLevel.L3LogicalFlow),
            subject,
            ablated);

        Assert.True(direct.IsSuccess, direct.ErrorMessage);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        AssertAblationEquivalent(direct.Value!, result.Value!.Ablation);
    }

    [Theory]
    [InlineData(DynamicGraphMeasurementTargetLevel.L2LogicalUnit)]
    [InlineData(DynamicGraphMeasurementTargetLevel.L3LogicalFlow)]
    public void MeasureNovelty_AdmittedLevelReturnsGuardOwnedTargetAndExactMeterOwnedEvidence(
        DynamicGraphMeasurementTargetLevel level)
    {
        var target = Target("logical-target-different-from-evaluation", level);
        var trainRoots = new List<string> { "train-novelty-a", "train-novelty-b" };
        var evalRoots = new List<string> { "eval-novelty-a", "eval-novelty-b" };
        var hashes = new List<string> { "hash-novelty-a", "hash-novelty-b" };
        var callerProof = NoveltyProof(trainRoots, evalRoots, hashes);
        var observation = NoveltyObservation(
            evaluationId: "held-out-novelty-evaluation",
            requestedRows: 137,
            rowsEvaluated: 137,
            newPatternRate: 0.31d,
            sourceTokenSeenRate: 0.69d,
            maxAccuracy: 0.75d,
            splitProof: callerProof);
        var direct = DynamicGraphNoveltyMeter.Measure(observation);

        var result = DynamicGraphMeasurementCoordinator.MeasureNovelty(target, observation);

        Assert.True(direct.IsSuccess, direct.ErrorMessage);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.NotSame(target, result.Value!.Target);
        Assert.Equal(target, result.Value.Target);
        AssertNoveltyEquivalent(direct.Value!, result.Value.Novelty);
        Assert.NotSame(callerProof, result.Value.Novelty.SplitProof);
        Assert.NotSame(direct.Value!.SplitProof, result.Value.Novelty.SplitProof);

        trainRoots[0] = "mutated-train";
        evalRoots[0] = "mutated-eval";
        hashes[0] = "mutated-hash";
        Assert.Equal("train-novelty-a", result.Value.Novelty.SplitProof.TrainSourceRoots[0]);
        Assert.Equal("eval-novelty-a", result.Value.Novelty.SplitProof.EvalSourceRoots[0]);
        Assert.Equal("hash-novelty-a", result.Value.Novelty.SplitProof.SourceHashes[0]);
        AssertReadOnly(result.Value.Novelty.SplitProof.TrainSourceRoots);
        AssertReadOnly(result.Value.Novelty.SplitProof.EvalSourceRoots);
        AssertReadOnly(result.Value.Novelty.SplitProof.SourceHashes);
    }

    [Fact]
    public void MeasureNovelty_DifferentTargetAndEvaluationIdsAreExplicitlyAllowed()
    {
        var result = DynamicGraphMeasurementCoordinator.MeasureNovelty(
            Target("logical-unit-42", DynamicGraphMeasurementTargetLevel.L2LogicalUnit),
            NoveltyObservation(evaluationId: "held-out-evaluation-17"));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("logical-unit-42", result.Value!.Target.TargetId, StringComparer.Ordinal);
        Assert.Equal("held-out-evaluation-17", result.Value.Novelty.EvaluationId, StringComparer.Ordinal);
    }

    [Theory]
    [InlineData(DynamicGraphMeasurementMechanismKind.Knn)]
    [InlineData(DynamicGraphMeasurementMechanismKind.Scorer)]
    [InlineData(DynamicGraphMeasurementMechanismKind.Counter)]
    [InlineData(DynamicGraphMeasurementMechanismKind.Regex)]
    public void MeasureNovelty_ShortcutTargetPrecedesHostileObservation(
        DynamicGraphMeasurementMechanismKind mechanismKind)
    {
        var result = DynamicGraphMeasurementCoordinator.MeasureNovelty(
            Target(
                level: DynamicGraphMeasurementTargetLevel.L2LogicalUnit,
                mechanismKind: mechanismKind),
            HostileNoveltyObservation(new BadImageFormatException("must not traverse")));

        AssertNoveltyFailure(
            result,
            "FORBIDDEN_RUNTIME_SHORTCUT",
            "Dynamic graph measurement target is a forbidden runtime shortcut.");
    }

    [Fact]
    public void MeasureNovelty_DeterministicAuthorityPrecedesHostileObservation()
    {
        var result = DynamicGraphMeasurementCoordinator.MeasureNovelty(
            Target(
                level: DynamicGraphMeasurementTargetLevel.L3LogicalFlow,
                deterministicFinalAuthority: true),
            HostileNoveltyObservation(new OperationCanceledException("must not traverse")));

        AssertNoveltyFailure(
            result,
            "FORBIDDEN_RUNTIME_SHORTCUT",
            "Dynamic graph measurement target is a forbidden runtime shortcut.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" malformed")]
    public void MeasureNovelty_MalformedTargetPrecedesHostileObservation(string? targetId)
    {
        var target = targetId is null
            ? null
            : Target(targetId, DynamicGraphMeasurementTargetLevel.L2LogicalUnit);

        var result = DynamicGraphMeasurementCoordinator.MeasureNovelty(
            target,
            HostileNoveltyObservation(new BadImageFormatException("must not traverse")));

        AssertNoveltyFailure(
            result,
            "INVALID_DYNAMIC_GRAPH_MEASUREMENT_TARGET",
            "Dynamic graph measurement target is invalid.");
    }

    [Theory]
    [InlineData(DynamicGraphMeasurementTargetLevel.Pair)]
    [InlineData(DynamicGraphMeasurementTargetLevel.L4LogicalOrchestrator)]
    [InlineData(DynamicGraphMeasurementTargetLevel.L5LearningMachine)]
    public void MeasureNovelty_NonNoveltyLevelPrecedesHostileSplit(
        DynamicGraphMeasurementTargetLevel level)
    {
        var result = DynamicGraphMeasurementCoordinator.MeasureNovelty(
            Target(level: level),
            HostileNoveltyObservation(new BadImageFormatException("must not traverse")));

        AssertNoveltyFailure(result, InvalidNoveltyTargetLevelCode, InvalidNoveltyTargetLevelMessage);
    }

    [Theory]
    [InlineData(DynamicGraphMeasurementTargetLevel.L2LogicalUnit)]
    [InlineData(DynamicGraphMeasurementTargetLevel.L3LogicalFlow)]
    public void MeasureNovelty_AdmittedTargetPreservesNullObservationError(
        DynamicGraphMeasurementTargetLevel level)
    {
        var result = DynamicGraphMeasurementCoordinator.MeasureNovelty(
            Target(level: level),
            null);

        AssertNoveltyFailure(
            result,
            "INVALID_NOVELTY_MEASUREMENT",
            "Dynamic graph novelty measurement is invalid.");
    }

    [Theory]
    [InlineData(DynamicGraphMeasurementTargetLevel.L2LogicalUnit)]
    [InlineData(DynamicGraphMeasurementTargetLevel.L3LogicalFlow)]
    public void MeasureNovelty_AdmittedTargetPreservesLocalInvalidMeasurementError(
        DynamicGraphMeasurementTargetLevel level)
    {
        var result = DynamicGraphMeasurementCoordinator.MeasureNovelty(
            Target(level: level),
            NoveltyObservation(evaluationId: " malformed"));

        AssertNoveltyFailure(
            result,
            "INVALID_NOVELTY_MEASUREMENT",
            "Dynamic graph novelty measurement is invalid.");
    }

    [Theory]
    [InlineData(DynamicGraphMeasurementTargetLevel.L2LogicalUnit, "invalid", "INVALID_SPLIT_PROOF", "Dynamic graph measurement split proof is invalid.")]
    [InlineData(DynamicGraphMeasurementTargetLevel.L2LogicalUnit, "overlap", "TRAIN_HELDOUT_OVERLAP", "Training and held-out source roots overlap.")]
    [InlineData(DynamicGraphMeasurementTargetLevel.L2LogicalUnit, "near-duplicate", "TRAIN_HELDOUT_NEAR_DUPLICATE", "Training and held-out sources contain near-duplicates.")]
    [InlineData(DynamicGraphMeasurementTargetLevel.L3LogicalFlow, "invalid", "INVALID_SPLIT_PROOF", "Dynamic graph measurement split proof is invalid.")]
    [InlineData(DynamicGraphMeasurementTargetLevel.L3LogicalFlow, "overlap", "TRAIN_HELDOUT_OVERLAP", "Training and held-out source roots overlap.")]
    [InlineData(DynamicGraphMeasurementTargetLevel.L3LogicalFlow, "near-duplicate", "TRAIN_HELDOUT_NEAR_DUPLICATE", "Training and held-out sources contain near-duplicates.")]
    public void MeasureNovelty_AdmittedTargetPreservesExactSplitErrors(
        DynamicGraphMeasurementTargetLevel level,
        string condition,
        string code,
        string message)
    {
        var proof = condition switch
        {
            "overlap" => NoveltyProof(trainRoots: ["shared"], evalRoots: ["shared"]),
            "near-duplicate" => NoveltyProof(nearDuplicateCount: 1),
            _ => NoveltyProof(method: "SimHash")
        };

        var result = DynamicGraphMeasurementCoordinator.MeasureNovelty(
            Target(level: level),
            NoveltyObservation(splitProof: proof));

        AssertNoveltyFailure(result, code, message);
    }

    [Theory]
    [InlineData(DynamicGraphMeasurementTargetLevel.L2LogicalUnit, "cancellation")]
    [InlineData(DynamicGraphMeasurementTargetLevel.L2LogicalUnit, "wrapped-cancellation")]
    [InlineData(DynamicGraphMeasurementTargetLevel.L2LogicalUnit, "aggregate-cancellation")]
    [InlineData(DynamicGraphMeasurementTargetLevel.L2LogicalUnit, "fatal")]
    [InlineData(DynamicGraphMeasurementTargetLevel.L2LogicalUnit, "wrapped-fatal")]
    [InlineData(DynamicGraphMeasurementTargetLevel.L2LogicalUnit, "aggregate-fatal")]
    [InlineData(DynamicGraphMeasurementTargetLevel.L3LogicalFlow, "cancellation")]
    [InlineData(DynamicGraphMeasurementTargetLevel.L3LogicalFlow, "wrapped-cancellation")]
    [InlineData(DynamicGraphMeasurementTargetLevel.L3LogicalFlow, "aggregate-cancellation")]
    [InlineData(DynamicGraphMeasurementTargetLevel.L3LogicalFlow, "fatal")]
    [InlineData(DynamicGraphMeasurementTargetLevel.L3LogicalFlow, "wrapped-fatal")]
    [InlineData(DynamicGraphMeasurementTargetLevel.L3LogicalFlow, "aggregate-fatal")]
    public void MeasureNovelty_AdmittedTargetLetsCancellationAndFatalGraphsEscapeUnchanged(
        DynamicGraphMeasurementTargetLevel level,
        string graph)
    {
        var exception = NoveltyExceptionGraph(graph);
        var hostile = HostileNoveltyObservation(exception);

        var thrown = Assert.Throws(exception.GetType(), () =>
            DynamicGraphMeasurementCoordinator.MeasureNovelty(
                Target(level: level),
                hostile));

        Assert.Same(exception, thrown);
    }

    [Fact]
    public void MeasureNovelty_CoordinatorBodyHasNoExceptionHandlingClauses()
    {
        var method = Assert.IsAssignableFrom<MethodInfo>(
            typeof(DynamicGraphMeasurementCoordinator).GetMethod(
                nameof(DynamicGraphMeasurementCoordinator.MeasureNovelty),
                BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
        var body = Assert.IsAssignableFrom<MethodBody>(method.GetMethodBody());

        Assert.Empty(body.ExceptionHandlingClauses);
    }

    [Theory]
    [InlineData(0.25d, 0.7499999999999999d, 0.75d)]
    [InlineData(0.25000000000000006d, 0.7499999999999999d, 0.75d)]
    [InlineData(0.25000000000000006d, 0.75d, 0.75d)]
    [InlineData(0.5d, 0.5d, 0d)]
    [InlineData(0.5d, 0.5d, 1d)]
    public void MeasureNovelty_RawStrictThresholdAndGateExactlyMatchDirectMeter(
        double newPatternRate,
        double sourceTokenSeenRate,
        double maxAccuracy)
    {
        var observation = NoveltyObservation(
            newPatternRate: newPatternRate,
            sourceTokenSeenRate: sourceTokenSeenRate,
            maxAccuracy: maxAccuracy);
        var direct = DynamicGraphNoveltyMeter.Measure(observation);

        var result = DynamicGraphMeasurementCoordinator.MeasureNovelty(
            Target(level: DynamicGraphMeasurementTargetLevel.L3LogicalFlow),
            observation);

        Assert.True(direct.IsSuccess, direct.ErrorMessage);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        AssertNoveltyEquivalent(direct.Value!, result.Value!.Novelty);
    }

    [Theory]
    [InlineData(DynamicGraphMeasurementTargetLevel.Pair)]
    [InlineData(DynamicGraphMeasurementTargetLevel.L2LogicalUnit)]
    [InlineData(DynamicGraphMeasurementTargetLevel.L3LogicalFlow)]
    [InlineData(DynamicGraphMeasurementTargetLevel.L4LogicalOrchestrator)]
    [InlineData(DynamicGraphMeasurementTargetLevel.L5LearningMachine)]
    public void MeasureAccuracy_AllFiveLearningSubjectLevelsReturnOwnedTargetAndMeterEvidence(
        DynamicGraphMeasurementTargetLevel level)
    {
        var target = Target(level: level);
        var proof = Proof();
        var before = Evaluation(accuracy: 0.93d, passesGate: true);
        var after = Evaluation(accuracy: 0.95d, rewardMargin: 0.025d, passesGate: false);

        var direct = DynamicGraphAccuracyMeter.Measure(0.42d, before, after, proof);
        var result = DynamicGraphMeasurementCoordinator.MeasureAccuracy(
            target,
            0.42d,
            before,
            after,
            proof);

        Assert.True(direct.IsSuccess, direct.ErrorMessage);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(target, result.Value!.Target);
        Assert.NotSame(target, result.Value.Target);
        Assert.Equal(level, result.Value.Target.Level);
        AssertAccuracyEquivalent(direct.Value!, result.Value.Accuracy);
        Assert.True(result.Value.Accuracy.MeetsQ30NumericalGate);
    }

    [Theory]
    [InlineData(DynamicGraphMeasurementMechanismKind.Knn)]
    [InlineData(DynamicGraphMeasurementMechanismKind.Scorer)]
    [InlineData(DynamicGraphMeasurementMechanismKind.Counter)]
    [InlineData(DynamicGraphMeasurementMechanismKind.Regex)]
    public void MeasureAccuracy_ShortcutTargetPrecedesHostileDownstreamEvidence(
        DynamicGraphMeasurementMechanismKind mechanismKind)
    {
        AssertFailure(
            DynamicGraphMeasurementCoordinator.MeasureAccuracy(
                Target(mechanismKind: mechanismKind),
                double.NaN,
                null,
                null,
                HostileProof(new InvalidOperationException("must not traverse"))),
            "FORBIDDEN_RUNTIME_SHORTCUT",
            "Dynamic graph measurement target is a forbidden runtime shortcut.");
    }

    [Fact]
    public void MeasureAccuracy_DeterministicAuthorityPrecedesCancellationFromDownstreamSplit()
    {
        AssertFailure(
            DynamicGraphMeasurementCoordinator.MeasureAccuracy(
                Target(deterministicFinalAuthority: true),
                double.NaN,
                null,
                null,
                HostileProof(new OperationCanceledException("must not traverse"))),
            "FORBIDDEN_RUNTIME_SHORTCUT",
            "Dynamic graph measurement target is a forbidden runtime shortcut.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" malformed")]
    [InlineData("malformed\nvalue")]
    public void MeasureAccuracy_MalformedTargetPrecedesFatalDownstreamSplit(string? targetId)
    {
        AssertFailure(
            DynamicGraphMeasurementCoordinator.MeasureAccuracy(
                targetId is null ? null : Target(targetId: targetId),
                double.NaN,
                null,
                null,
                HostileProof(new AggregateException(new OutOfMemoryException("must not traverse")))),
            "INVALID_DYNAMIC_GRAPH_MEASUREMENT_TARGET",
            "Dynamic graph measurement target is invalid.");
    }

    [Fact]
    public void MeasureAccuracy_ValidTargetPreservesOverlapError()
    {
        AssertFailure(
            DynamicGraphMeasurementCoordinator.MeasureAccuracy(
                Target(),
                double.NaN,
                null,
                null,
                Proof(trainRoots: ["shared"], evalRoots: ["shared"])),
            "TRAIN_HELDOUT_OVERLAP",
            "Training and held-out source roots overlap.");
    }

    [Fact]
    public void MeasureAccuracy_ValidTargetPreservesNearDuplicateError()
    {
        AssertFailure(
            DynamicGraphMeasurementCoordinator.MeasureAccuracy(
                Target(),
                double.NaN,
                null,
                null,
                Proof(nearDuplicateCount: 1)),
            "TRAIN_HELDOUT_NEAR_DUPLICATE",
            "Training and held-out sources contain near-duplicates.");
    }

    [Fact]
    public void MeasureAccuracy_ValidTargetPreservesEvaluationMismatchError()
    {
        AssertFailure(
            DynamicGraphMeasurementCoordinator.MeasureAccuracy(
                Target(),
                0d,
                Evaluation(evaluationId: "before", accuracy: 0.93d),
                Evaluation(evaluationId: "after", accuracy: 0.95d),
                Proof()),
            "HELDOUT_EVALUATION_MISMATCH",
            "Held-out evaluation results do not describe the same evaluation set.");
    }

    [Fact]
    public void MeasureAccuracy_ValidTargetPreservesInvalidAccuracyError()
    {
        AssertFailure(
            DynamicGraphMeasurementCoordinator.MeasureAccuracy(
                Target(),
                double.NaN,
                null,
                null,
                Proof()),
            "INVALID_ACCURACY_MEASUREMENT",
            "Dynamic graph accuracy measurement is invalid.");
    }

    [Fact]
    public void MeasureAccuracy_ValidTargetAllowsCancellationFromSplitTraversalToEscape()
    {
        var cancellation = new OperationCanceledException("cancel split traversal");

        var thrown = Assert.Throws<OperationCanceledException>(() =>
            DynamicGraphMeasurementCoordinator.MeasureAccuracy(
                Target(),
                0d,
                null,
                null,
                HostileProof(cancellation)));

        Assert.Same(cancellation, thrown);
    }

    [Fact]
    public void MeasureAccuracy_ValidTargetAllowsNestedFatalFromSplitTraversalToEscape()
    {
        var fatal = new OutOfMemoryException("fatal split traversal");
        var wrapper = new AggregateException(new InvalidOperationException("ordinary"), fatal);

        var thrown = Assert.Throws<AggregateException>(() =>
            DynamicGraphMeasurementCoordinator.MeasureAccuracy(
                Target(),
                0d,
                null,
                null,
                HostileProof(wrapper)));

        Assert.Same(wrapper, thrown);
    }

    [Fact]
    public void MeasureAccuracy_ReturnsMeterOwnedReadOnlySplitProof()
    {
        var train = new List<string> { "train-a" };
        var eval = new List<string> { "eval-a" };
        var hashes = new List<string> { "hash-a" };
        var callerProof = Proof(train, eval, hashes);

        var result = DynamicGraphMeasurementCoordinator.MeasureAccuracy(
            Target(),
            0d,
            Evaluation(accuracy: 0.93d),
            Evaluation(accuracy: 0.95d),
            callerProof);
        Assert.True(result.IsSuccess, result.ErrorMessage);

        train[0] = "mutated-train";
        eval[0] = "mutated-eval";
        hashes[0] = "mutated-hash";
        Assert.NotSame(callerProof, result.Value!.Accuracy.SplitProof);
        Assert.Equal(["train-a"], result.Value.Accuracy.SplitProof.TrainSourceRoots);
        Assert.Equal(["eval-a"], result.Value.Accuracy.SplitProof.EvalSourceRoots);
        Assert.Equal(["hash-a"], result.Value.Accuracy.SplitProof.SourceHashes);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<string>)result.Value.Accuracy.SplitProof.TrainSourceRoots).Add("forbidden"));
    }

    [Theory]
    [InlineData(DynamicGraphMeasurementTargetLevel.Pair)]
    [InlineData(DynamicGraphMeasurementTargetLevel.L2LogicalUnit)]
    [InlineData(DynamicGraphMeasurementTargetLevel.L3LogicalFlow)]
    [InlineData(DynamicGraphMeasurementTargetLevel.L4LogicalOrchestrator)]
    [InlineData(DynamicGraphMeasurementTargetLevel.L5LearningMachine)]
    public void MeasureRunSpeed_AllFiveLearningSubjectLevelsReturnOwnedTargetAndExactMeterEvidence(
        DynamicGraphMeasurementTargetLevel level)
    {
        var target = Target(level: level);
        var observation = Observation(
            rows: 31,
            inferenceMs: 250,
            routingMs: 47,
            candidateSamples: [2, 5, 8],
            batchSamples: [11, 19]);

        var direct = DynamicGraphRunSpeedMeter.Measure(observation);
        var result = DynamicGraphMeasurementCoordinator.MeasureRunSpeed(target, observation);

        Assert.True(direct.IsSuccess, direct.ErrorMessage);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(target, result.Value!.Target);
        Assert.NotSame(target, result.Value.Target);
        Assert.Equal(level, result.Value.Target.Level);
        AssertRunSpeedEquivalent(direct.Value!, result.Value.RunSpeed);
    }

    [Theory]
    [InlineData(DynamicGraphMeasurementMechanismKind.Knn)]
    [InlineData(DynamicGraphMeasurementMechanismKind.Scorer)]
    [InlineData(DynamicGraphMeasurementMechanismKind.Counter)]
    [InlineData(DynamicGraphMeasurementMechanismKind.Regex)]
    public void MeasureRunSpeed_ShortcutTargetPrecedesOrdinaryHostileObservation(
        DynamicGraphMeasurementMechanismKind mechanismKind)
    {
        AssertRunSpeedFailure(
            DynamicGraphMeasurementCoordinator.MeasureRunSpeed(
                Target(mechanismKind: mechanismKind),
                HostileObservation(new InvalidOperationException("must not traverse"))),
            "FORBIDDEN_RUNTIME_SHORTCUT",
            "Dynamic graph measurement target is a forbidden runtime shortcut.");
    }

    [Fact]
    public void MeasureRunSpeed_DeterministicAuthorityPrecedesCancellationFromObservation()
    {
        AssertRunSpeedFailure(
            DynamicGraphMeasurementCoordinator.MeasureRunSpeed(
                Target(deterministicFinalAuthority: true),
                HostileObservation(new OperationCanceledException("must not traverse"), faultBatch: true)),
            "FORBIDDEN_RUNTIME_SHORTCUT",
            "Dynamic graph measurement target is a forbidden runtime shortcut.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" malformed")]
    [InlineData("malformed\nvalue")]
    public void MeasureRunSpeed_MalformedTargetPrecedesFatalObservation(string? targetId)
    {
        AssertRunSpeedFailure(
            DynamicGraphMeasurementCoordinator.MeasureRunSpeed(
                targetId is null ? null : Target(targetId: targetId),
                HostileObservation(
                    new AggregateException(new OutOfMemoryException("must not traverse")),
                    faultBatch: true)),
            "INVALID_DYNAMIC_GRAPH_MEASUREMENT_TARGET",
            "Dynamic graph measurement target is invalid.");
    }

    [Fact]
    public void MeasureRunSpeed_ValidTargetPreservesNullObservationError()
        => AssertRunSpeedFailure(
            DynamicGraphMeasurementCoordinator.MeasureRunSpeed(Target(), null),
            "INVALID_RUN_SPEED_OBSERVATION",
            "Dynamic graph run speed observation is invalid.");

    [Fact]
    public void MeasureRunSpeed_ValidTargetPreservesInvalidScalarError()
        => AssertRunSpeedFailure(
            DynamicGraphMeasurementCoordinator.MeasureRunSpeed(Target(), Observation(rows: 0)),
            "INVALID_RUN_SPEED_OBSERVATION",
            "Dynamic graph run speed observation is invalid.");

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MeasureRunSpeed_ValidTargetPreservesNegativeSampleError(bool negativeBatch)
    {
        var observation = negativeBatch
            ? Observation(candidateSamples: [1], batchSamples: [2, -1])
            : Observation(candidateSamples: [1, -1], batchSamples: [2]);

        AssertRunSpeedFailure(
            DynamicGraphMeasurementCoordinator.MeasureRunSpeed(Target(), observation),
            "INVALID_RUN_SPEED_OBSERVATION",
            "Dynamic graph run speed observation is invalid.");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MeasureRunSpeed_ValidTargetPreservesOverflowError(bool overflowBatch)
    {
        var observation = overflowBatch
            ? Observation(candidateSamples: [1], batchSamples: [long.MaxValue, 1])
            : Observation(candidateSamples: [long.MaxValue, 1], batchSamples: [2]);

        AssertRunSpeedFailure(
            DynamicGraphMeasurementCoordinator.MeasureRunSpeed(Target(), observation),
            "RUN_SPEED_MEASUREMENT_OVERFLOW",
            "Dynamic graph run speed measurement overflowed.");
    }

    [Fact]
    public void MeasureRunSpeed_ValidTargetPreservesOrdinarySnapshotFailureError()
        => AssertRunSpeedFailure(
            DynamicGraphMeasurementCoordinator.MeasureRunSpeed(
                Target(),
                HostileObservation(new InvalidOperationException("snapshot race"))),
            "INVALID_RUN_SPEED_OBSERVATION",
            "Dynamic graph run speed observation is invalid.");

    [Fact]
    public void MeasureRunSpeed_ValidTargetAllowsCancellationToEscapeUnchanged()
    {
        var cancellation = new OperationCanceledException("cancel observation snapshot");

        var thrown = Assert.Throws<OperationCanceledException>(() =>
            DynamicGraphMeasurementCoordinator.MeasureRunSpeed(
                Target(),
                HostileObservation(cancellation)));

        Assert.Same(cancellation, thrown);
    }

    [Fact]
    public void MeasureRunSpeed_ValidTargetAllowsNestedFatalToEscapeUnchanged()
    {
        var fatal = new OutOfMemoryException("fatal observation snapshot");
        var wrapper = new AggregateException(new InvalidOperationException("ordinary"), fatal);

        var thrown = Assert.Throws<AggregateException>(() =>
            DynamicGraphMeasurementCoordinator.MeasureRunSpeed(
                Target(),
                HostileObservation(wrapper, faultBatch: true)));

        Assert.Same(wrapper, thrown);
    }

    [Fact]
    public void MeasureRunSpeed_ReturnedEvidenceSurvivesCallerBankMutations()
    {
        var candidateSamples = new List<long> { 1, 3 };
        var batchSamples = new List<long> { 10, 20 };
        var observation = Observation(
            candidateSamples: candidateSamples,
            batchSamples: batchSamples);

        var result = DynamicGraphMeasurementCoordinator.MeasureRunSpeed(Target(), observation);
        Assert.True(result.IsSuccess, result.ErrorMessage);

        candidateSamples[0] = 999;
        candidateSamples.Add(999);
        batchSamples.Clear();

        Assert.Equal(2, result.Value!.RunSpeed.CandidateSampleCount);
        Assert.Equal(2, result.Value.RunSpeed.BatchSampleCount);
        Assert.Equal(2L, result.Value.RunSpeed.RunSpeed.PerCandidateElapsedMs);
        Assert.Equal(15L, result.Value.RunSpeed.RunSpeed.PerBatchElapsedMs);
    }

    [Fact]
    public void MeasureRunSpeed_KeepsAsymmetricBanksAndRoutingAxisIndependent()
    {
        var result = DynamicGraphMeasurementCoordinator.MeasureRunSpeed(
            Target(),
            Observation(
                rows: 40,
                inferenceMs: 320,
                routingMs: 9_001,
                candidateSamples: [2, 4, 6],
                batchSamples: [101]));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(3, result.Value!.RunSpeed.CandidateSampleCount);
        Assert.Equal(1, result.Value.RunSpeed.BatchSampleCount);
        Assert.Equal(125d, result.Value.RunSpeed.RunSpeed.ThroughputRowsPerSec);
        Assert.Equal(9_001L, result.Value.RunSpeed.RunSpeed.RoutingElapsedMs);
        Assert.Equal(4L, result.Value.RunSpeed.RunSpeed.PerCandidateElapsedMs);
        Assert.Equal(101L, result.Value.RunSpeed.RunSpeed.PerBatchElapsedMs);
    }

    [Fact]
    public void MeasureTrainingSpeed_L5TargetReturnsGuardOwnedTargetAndExactMeterEvidence()
    {
        var target = Target(level: DynamicGraphMeasurementTargetLevel.L5LearningMachine);
        var batch = Batch(
            batchId: "batch-sentinel",
            rowsTrained: 47,
            cpuMs: 1_003,
            peakRamMb: 2_009,
            elapsedMs: 3_017);

        var direct = DynamicGraphSpeedMeter.Measure(batch);
        var result = DynamicGraphMeasurementCoordinator.MeasureTrainingSpeed(target, batch);

        Assert.True(direct.IsSuccess, direct.ErrorMessage);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(target, result.Value!.Target);
        Assert.NotSame(target, result.Value.Target);
        AssertTrainingSpeedEquivalent(direct.Value!, result.Value.TrainingSpeed);
    }

    [Theory]
    [InlineData(DynamicGraphMeasurementMechanismKind.Knn)]
    [InlineData(DynamicGraphMeasurementMechanismKind.Scorer)]
    [InlineData(DynamicGraphMeasurementMechanismKind.Counter)]
    [InlineData(DynamicGraphMeasurementMechanismKind.Regex)]
    public void MeasureTrainingSpeed_ShortcutTargetPrecedesMalformedHostileResult(
        DynamicGraphMeasurementMechanismKind mechanismKind)
    {
        AssertTrainingSpeedFailure(
            DynamicGraphMeasurementCoordinator.MeasureTrainingSpeed(
                Target(
                    level: DynamicGraphMeasurementTargetLevel.L5LearningMachine,
                    mechanismKind: mechanismKind),
                HostileMalformedBatch()),
            "FORBIDDEN_RUNTIME_SHORTCUT",
            "Dynamic graph measurement target is a forbidden runtime shortcut.");
    }

    [Fact]
    public void MeasureTrainingSpeed_DeterministicAuthorityPrecedesMalformedHostileResult()
    {
        AssertTrainingSpeedFailure(
            DynamicGraphMeasurementCoordinator.MeasureTrainingSpeed(
                Target(
                    level: DynamicGraphMeasurementTargetLevel.L5LearningMachine,
                    deterministicFinalAuthority: true),
                HostileMalformedBatch()),
            "FORBIDDEN_RUNTIME_SHORTCUT",
            "Dynamic graph measurement target is a forbidden runtime shortcut.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" malformed")]
    [InlineData("malformed\nvalue")]
    public void MeasureTrainingSpeed_MalformedTargetPrecedesMalformedHostileResult(string? targetId)
    {
        AssertTrainingSpeedFailure(
            DynamicGraphMeasurementCoordinator.MeasureTrainingSpeed(
                targetId is null
                    ? null
                    : Target(
                        targetId: targetId,
                        level: DynamicGraphMeasurementTargetLevel.L5LearningMachine),
                HostileMalformedBatch()),
            "INVALID_DYNAMIC_GRAPH_MEASUREMENT_TARGET",
            "Dynamic graph measurement target is invalid.");
    }

    [Theory]
    [InlineData(DynamicGraphMeasurementTargetLevel.Pair)]
    [InlineData(DynamicGraphMeasurementTargetLevel.L2LogicalUnit)]
    [InlineData(DynamicGraphMeasurementTargetLevel.L3LogicalFlow)]
    [InlineData(DynamicGraphMeasurementTargetLevel.L4LogicalOrchestrator)]
    public void MeasureTrainingSpeed_NonL5TargetPrecedesMalformedHostileResult(
        DynamicGraphMeasurementTargetLevel level)
    {
        AssertTrainingSpeedFailure(
            DynamicGraphMeasurementCoordinator.MeasureTrainingSpeed(
                Target(level: level),
                HostileMalformedBatch()),
            "DYNAMIC_GRAPH_TRAINING_SPEED_TARGET_LEVEL_INVALID",
            "Dynamic graph training speed can only be associated with an L5 learning-machine target.");
    }

    [Fact]
    public void MeasureTrainingSpeed_L5TargetPreservesNullAndMalformedMeterErrors()
    {
        LearningBatchResult?[] invalidResults =
        [
            null,
            Batch(batchId: " malformed"),
            Batch(rowsTrained: 0),
            Batch(rowsTrained: -1),
            Batch(cpuMs: -1),
            Batch(peakRamMb: -1),
            Batch(elapsedMs: -1)
        ];

        foreach (var invalidResult in invalidResults)
        {
            AssertTrainingSpeedFailure(
                DynamicGraphMeasurementCoordinator.MeasureTrainingSpeed(
                    Target(level: DynamicGraphMeasurementTargetLevel.L5LearningMachine),
                    invalidResult),
                "INVALID_TRAINING_SPEED_MEASUREMENT",
                "Dynamic graph training speed measurement is invalid.");
        }
    }

    [Fact]
    public void MeasureTrainingSpeed_ZeroResourcesRemainValid()
    {
        var result = DynamicGraphMeasurementCoordinator.MeasureTrainingSpeed(
            Target(level: DynamicGraphMeasurementTargetLevel.L5LearningMachine),
            Batch(rowsTrained: 1, cpuMs: 0, peakRamMb: 0, elapsedMs: 0));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(
            new DynamicGraphTrainingSpeedAxis(0, 0d, 0, 1),
            result.Value!.TrainingSpeed.Speed);
    }

    [Fact]
    public void MeasureTrainingSpeed_PreservesMaximumResourceValues()
    {
        var result = DynamicGraphMeasurementCoordinator.MeasureTrainingSpeed(
            Target(level: DynamicGraphMeasurementTargetLevel.L5LearningMachine),
            Batch(
                rowsTrained: int.MaxValue,
                cpuMs: long.MaxValue,
                peakRamMb: long.MaxValue,
                elapsedMs: long.MaxValue));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(long.MaxValue, result.Value!.TrainingSpeed.Speed.CpuMs);
        Assert.Equal((double)long.MaxValue, result.Value.TrainingSpeed.Speed.PeakRamMb);
        Assert.Equal(long.MaxValue, result.Value.TrainingSpeed.Speed.ElapsedMs);
        Assert.Equal(int.MaxValue, result.Value.TrainingSpeed.Speed.TrainRows);
    }

    [Fact]
    public void MeasureTrainingSpeed_IgnoresQualityAndDoesNotReadPromotedUnitIds()
    {
        var hostile = new InvalidOperationException("Promoted unit ids must not be read.");
        var first = DynamicGraphMeasurementCoordinator.MeasureTrainingSpeed(
            Target(level: DynamicGraphMeasurementTargetLevel.L5LearningMachine),
            Batch(
                accuracyBefore: double.NaN,
                accuracyAfter: double.PositiveInfinity,
                rewardMargin: double.NegativeInfinity,
                promotedUnitIds: new FaultingReadOnlyList<string>(hostile)));
        var second = DynamicGraphMeasurementCoordinator.MeasureTrainingSpeed(
            Target(level: DynamicGraphMeasurementTargetLevel.L5LearningMachine),
            new LearningBatchResult(
                "batch-1",
                11,
                double.NegativeInfinity,
                -123.45d,
                double.NaN,
                101,
                202,
                303,
                null!));

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.Equal(first.Value!.TrainingSpeed, second.Value!.TrainingSpeed);
    }

    [Fact]
    public void MeasureTrainingSpeed_ReturnedEvidenceSurvivesCallerCollectionMutation()
    {
        var promotedUnitIds = new List<string> { "unit-a" };
        var result = DynamicGraphMeasurementCoordinator.MeasureTrainingSpeed(
            Target(level: DynamicGraphMeasurementTargetLevel.L5LearningMachine),
            Batch(promotedUnitIds: promotedUnitIds));
        Assert.True(result.IsSuccess, result.ErrorMessage);

        var captured = result.Value;
        promotedUnitIds[0] = "mutated";
        promotedUnitIds.Add("added");

        Assert.Equal("target-a", captured!.Target.TargetId);
        Assert.Equal("batch-1", captured.TrainingSpeed.BatchId);
        Assert.Equal(new DynamicGraphTrainingSpeedAxis(101, 202d, 303, 11), captured.TrainingSpeed.Speed);
    }

    [Fact]
    public void MeasureEdgeAuthorityControl_CanonicalL3TargetReturnsOwnedTargetAndCombinedEvidence()
    {
        var target = Target(
            targetId: EdgeOrderQualityId,
            level: DynamicGraphMeasurementTargetLevel.L3LogicalFlow);
        var carrier = EdgeOrderQualityResult();

        var direct = DynamicGraphEdgeAuthorityControlMeter.Measure(carrier);
        var result = DynamicGraphMeasurementCoordinator.MeasureEdgeAuthorityControl(target, carrier);

        Assert.True(direct.IsSuccess, $"{direct.ErrorCode}: {direct.ErrorMessage}");
        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.Equal(target, result.Value!.Target);
        Assert.NotSame(target, result.Value.Target);
        Assert.NotSame(direct.Value, result.Value.EdgeAuthorityControl);
        AssertEdgeAuthorityControlEquivalent(direct.Value!, result.Value.EdgeAuthorityControl);

        var firstTarget = result.Value.Target;
        var secondTarget = result.Value.Target;
        var firstMeasurement = result.Value.EdgeAuthorityControl;
        var secondMeasurement = result.Value.EdgeAuthorityControl;
        Assert.NotSame(firstTarget, secondTarget);
        Assert.NotSame(firstMeasurement, secondMeasurement);
        Assert.NotSame(firstMeasurement.AuthorityEvidence, secondMeasurement.AuthorityEvidence);
        Assert.NotSame(firstMeasurement.EdgeOrderQuality, secondMeasurement.EdgeOrderQuality);
        Assert.NotSame(firstMeasurement.EdgeIdentityQuality, secondMeasurement.EdgeIdentityQuality);
        SetRecordProperty(firstTarget, nameof(DynamicGraphMeasurementTarget.TargetId), "mutated-target");
        GetPrivateStringArray(
            GetPrivateCarrier<DynamicGraphEdgeOrderQualityMeasurement>(
                firstMeasurement,
                "_edgeOrderQuality"),
            "_probeIds")[0] = "mutated-probe";
        GetPrivateStringArray(
            GetPrivateCarrier<DynamicGraphEdgeIdentityQualityMeasurement>(
                firstMeasurement,
                "_edgeIdentityQuality"),
            "_controlledSignals")[0] = "mutated-signal";
        Assert.Equal(EdgeOrderQualityId, result.Value.Target.TargetId, StringComparer.Ordinal);
        Assert.Equal("candidate-order-permutation-01", result.Value.EdgeAuthorityControl.EdgeOrderQuality.ProbeIds[0]);
        Assert.Equal("source_id", result.Value.EdgeAuthorityControl.EdgeIdentityQuality.ControlledSignals[0]);
    }

    [Theory]
    [InlineData(DynamicGraphMeasurementMechanismKind.Knn)]
    [InlineData(DynamicGraphMeasurementMechanismKind.Scorer)]
    [InlineData(DynamicGraphMeasurementMechanismKind.Counter)]
    [InlineData(DynamicGraphMeasurementMechanismKind.Regex)]
    public void MeasureEdgeAuthorityControl_ShortcutTargetPrecedesCorruptedCarrier(
        DynamicGraphMeasurementMechanismKind mechanismKind)
    {
        AssertEdgeAuthorityControlFailure(
            DynamicGraphMeasurementCoordinator.MeasureEdgeAuthorityControl(
                Target(
                    targetId: EdgeOrderQualityId,
                    level: DynamicGraphMeasurementTargetLevel.L3LogicalFlow,
                    mechanismKind: mechanismKind),
                CorruptedEdgeAuthorityEvidenceResult()),
            "FORBIDDEN_RUNTIME_SHORTCUT",
            "Dynamic graph measurement target is a forbidden runtime shortcut.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("edge\u0001id")]
    public void MeasureEdgeAuthorityControl_MalformedTargetPrecedesCorruptedCarrier(string? targetId)
    {
        AssertEdgeAuthorityControlFailure(
            DynamicGraphMeasurementCoordinator.MeasureEdgeAuthorityControl(
                targetId is null
                    ? null
                    : Target(
                        targetId: targetId,
                        level: DynamicGraphMeasurementTargetLevel.L3LogicalFlow),
                CorruptedEdgeAuthorityEvidenceResult()),
            "INVALID_DYNAMIC_GRAPH_MEASUREMENT_TARGET",
            "Dynamic graph measurement target is invalid.");
    }

    [Theory]
    [InlineData(DynamicGraphMeasurementTargetLevel.Pair)]
    [InlineData(DynamicGraphMeasurementTargetLevel.L2LogicalUnit)]
    [InlineData(DynamicGraphMeasurementTargetLevel.L4LogicalOrchestrator)]
    [InlineData(DynamicGraphMeasurementTargetLevel.L5LearningMachine)]
    public void MeasureEdgeAuthorityControl_NonL3TargetPrecedesCorruptedCarrier(
        DynamicGraphMeasurementTargetLevel level)
    {
        AssertEdgeAuthorityControlFailure(
            DynamicGraphMeasurementCoordinator.MeasureEdgeAuthorityControl(
                Target(targetId: EdgeOrderQualityId, level: level),
                CorruptedEdgeAuthorityEvidenceResult()),
            InvalidEdgeAuthorityControlTargetLevelCode,
            InvalidEdgeAuthorityControlTargetLevelMessage);
    }

    [Fact]
    public void MeasureEdgeAuthorityControl_ValidTargetPreservesCombinedAndNestedMeterErrors()
    {
        var target = Target(
            targetId: EdgeOrderQualityId,
            level: DynamicGraphMeasurementTargetLevel.L3LogicalFlow);

        AssertEdgeAuthorityControlFailure(
            DynamicGraphMeasurementCoordinator.MeasureEdgeAuthorityControl(target, null),
            "INVALID_EDGE_AUTHORITY_CONTROL_MEASUREMENT",
            "Dynamic graph edge authority control measurement is invalid.");
        AssertEdgeAuthorityControlFailure(
            DynamicGraphMeasurementCoordinator.MeasureEdgeAuthorityControl(
                target,
                CorruptedEdgeAuthorityEvidenceResult()),
            "INVALID_EDGE_AUTHORITY_CONTROL_MEASUREMENT",
            "Dynamic graph edge authority control measurement is invalid.");
        AssertEdgeAuthorityControlFailure(
            DynamicGraphMeasurementCoordinator.MeasureEdgeAuthorityControl(
                target,
                CorruptedEdgeOrderQualityResult()),
            "INVALID_EDGE_ORDER_QUALITY_MEASUREMENT",
            "Dynamic graph edge order quality measurement is invalid.");
        AssertEdgeAuthorityControlFailure(
            DynamicGraphMeasurementCoordinator.MeasureEdgeAuthorityControl(
                target,
                CorruptedEdgeIdentityQualityResult()),
            "INVALID_EDGE_IDENTITY_QUALITY_MEASUREMENT",
            "Dynamic graph edge identity quality measurement is invalid.");
    }

    [Theory]
    [InlineData("different-edge")]
    [InlineData("EDGE-ORDER-QUALITY-COORDINATOR")]
    [InlineData(EdgeOrderQualityUnitId)]
    public void MeasureEdgeAuthorityControl_ValidEvidenceRequiresOrdinalTargetMatch(string targetId)
    {
        AssertEdgeAuthorityControlFailure(
            DynamicGraphMeasurementCoordinator.MeasureEdgeAuthorityControl(
                Target(
                    targetId: targetId,
                    level: DynamicGraphMeasurementTargetLevel.L3LogicalFlow),
                EdgeOrderQualityResult()),
            EdgeAuthorityControlTargetMismatchCode,
            EdgeAuthorityControlTargetMismatchMessage);
    }

    [Fact]
    public void MeasureEdgeAuthorityControl_CoordinatorBodyHasNoExceptionHandlingClauses()
    {
        var method = Assert.IsAssignableFrom<MethodInfo>(
            typeof(DynamicGraphMeasurementCoordinator).GetMethod(
                nameof(DynamicGraphMeasurementCoordinator.MeasureEdgeAuthorityControl),
                BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
        var body = Assert.IsAssignableFrom<MethodBody>(method.GetMethodBody());

        Assert.Empty(body.ExceptionHandlingClauses);
    }

    [Fact]
    public void MeasureEdgeQualityEvidence_CanonicalL3TargetReturnsOwnedCorrelatedEvidence()
    {
        var target = Target(
            targetId: EdgeOrderQualityId,
            level: DynamicGraphMeasurementTargetLevel.L3LogicalFlow);
        var trainRoots = new List<string> { "train-quality-a", "train-quality-b" };
        var evalRoots = new List<string> { "eval-quality-a", "eval-quality-b" };
        var hashes = new List<string> { "hash-quality-a", "hash-quality-b" };
        var proof = AblationProof(trainRoots, evalRoots, hashes);
        var novelty = NoveltyObservation(
            evaluationId: "evaluation-different-from-edge-target",
            requestedRows: 137,
            rowsEvaluated: 137,
            splitProof: proof);
        var subject = AblationAccuracy(
            evaluationId: "evaluation-different-from-edge-target",
            rowsEvaluated: 137,
            heldOutAccuracy: 0.96d,
            splitProof: proof);
        var ablated = AblationAccuracy(
            evaluationId: "evaluation-different-from-edge-target",
            rowsEvaluated: 137,
            heldOutAccuracy: 0.93d,
            splitProof: proof);

        var result = DynamicGraphMeasurementCoordinator.MeasureEdgeQualityEvidence(
            target,
            EdgeOrderQualityResult(),
            novelty,
            subject,
            ablated);

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.NotSame(target, result.Value!.Target);
        Assert.Equal(EdgeOrderQualityId, result.Value.Target.TargetId, StringComparer.Ordinal);
        Assert.Equal(
            "evaluation-different-from-edge-target",
            result.Value.EdgeQualityEvidence.Novelty.EvaluationId,
            StringComparer.Ordinal);
        Assert.Equal(
            "evaluation-different-from-edge-target",
            result.Value.EdgeQualityEvidence.Ablation.EvaluationId,
            StringComparer.Ordinal);
        Assert.Equal(
            EdgeOrderQualityId,
            result.Value.EdgeQualityEvidence.EdgeAuthorityControl.AuthorityEvidence.EdgeId,
            StringComparer.Ordinal);

        trainRoots[0] = "caller-mutated-train";
        evalRoots[0] = "caller-mutated-eval";
        hashes[0] = "caller-mutated-hash";
        var firstTarget = result.Value.Target;
        var firstEvidence = result.Value.EdgeQualityEvidence;
        var secondEvidence = result.Value.EdgeQualityEvidence;
        Assert.NotSame(firstEvidence, secondEvidence);
        Assert.NotSame(firstEvidence.Novelty, secondEvidence.Novelty);
        Assert.NotSame(firstEvidence.Ablation, secondEvidence.Ablation);
        SetRecordProperty(firstTarget, nameof(DynamicGraphMeasurementTarget.TargetId), "returned-copy-mutated");
        SetRecordProperty(
            GetPrivateCarrier<DynamicGraphNoveltyMeasurement>(firstEvidence, "_novelty").SplitProof,
            nameof(DynamicGraphMeasurementSplitProof.TrainSourceRoots),
            new[] { "returned-copy-mutated" });
        Assert.Equal(EdgeOrderQualityId, result.Value.Target.TargetId, StringComparer.Ordinal);
        Assert.Equal(
            "train-quality-a",
            result.Value.EdgeQualityEvidence.Novelty.SplitProof.TrainSourceRoots[0],
            StringComparer.Ordinal);
        Assert.Equal(
            "eval-quality-a",
            result.Value.EdgeQualityEvidence.Ablation.SplitProof.EvalSourceRoots[0],
            StringComparer.Ordinal);
        Assert.Equal(
            "hash-quality-a",
            result.Value.EdgeQualityEvidence.Ablation.SplitProof.SourceHashes[0],
            StringComparer.Ordinal);
    }

    [Theory]
    [InlineData("shortcut")]
    [InlineData("malformed")]
    [InlineData("non-l3")]
    public void MeasureEdgeQualityEvidence_TargetBoundaryPrecedesCorruptDownstream(string targetCase)
    {
        var target = targetCase switch
        {
            "shortcut" => Target(
                EdgeOrderQualityId,
                DynamicGraphMeasurementTargetLevel.L3LogicalFlow,
                DynamicGraphMeasurementMechanismKind.Knn),
            "malformed" => Target(
                " malformed",
                DynamicGraphMeasurementTargetLevel.L3LogicalFlow),
            "non-l3" => Target(
                EdgeOrderQualityId,
                DynamicGraphMeasurementTargetLevel.L2LogicalUnit),
            _ => throw new ArgumentOutOfRangeException(nameof(targetCase))
        };

        var result = DynamicGraphMeasurementCoordinator.MeasureEdgeQualityEvidence(
            target,
            CorruptedEdgeAuthorityEvidenceResult(),
            HostileNoveltyObservation(new BadImageFormatException("must not traverse novelty")),
            HostileAblationAccuracy(new OperationCanceledException("must not traverse ablation")),
            AblationAccuracy());

        AssertEdgeQualityEvidenceFailure(
            result,
            targetCase switch
            {
                "shortcut" => "FORBIDDEN_RUNTIME_SHORTCUT",
                "malformed" => "INVALID_DYNAMIC_GRAPH_MEASUREMENT_TARGET",
                _ => InvalidEdgeQualityEvidenceTargetLevelCode
            },
            targetCase switch
            {
                "shortcut" => "Dynamic graph measurement target is a forbidden runtime shortcut.",
                "malformed" => "Dynamic graph measurement target is invalid.",
                _ => InvalidEdgeQualityEvidenceTargetLevelMessage
            });
    }

    [Fact]
    public void MeasureEdgeQualityEvidence_PreservesAuthorityNoveltyAblationAndCorrelationErrors()
    {
        var target = Target(
            EdgeOrderQualityId,
            DynamicGraphMeasurementTargetLevel.L3LogicalFlow);
        var proof = AblationProof();

        AssertEdgeQualityEvidenceFailure(
            DynamicGraphMeasurementCoordinator.MeasureEdgeQualityEvidence(
                target,
                null,
                NoveltyObservation(splitProof: proof),
                AblationAccuracy(splitProof: proof),
                AblationAccuracy(heldOutAccuracy: 0.93d, splitProof: proof)),
            "INVALID_EDGE_AUTHORITY_CONTROL_MEASUREMENT",
            "Dynamic graph edge authority control measurement is invalid.");

        AssertEdgeQualityEvidenceFailure(
            DynamicGraphMeasurementCoordinator.MeasureEdgeQualityEvidence(
                target,
                EdgeOrderQualityResult(),
                NoveltyObservation(splitProof: NoveltyProof(method: "SimHash")),
                HostileAblationAccuracy(new BadImageFormatException("must not traverse ablation")),
                AblationAccuracy()),
            "INVALID_SPLIT_PROOF",
            "Dynamic graph measurement split proof is invalid.");

        AssertEdgeQualityEvidenceFailure(
            DynamicGraphMeasurementCoordinator.MeasureEdgeQualityEvidence(
                target,
                EdgeOrderQualityResult(),
                NoveltyObservation(splitProof: proof),
                AblationAccuracy(evaluationId: "subject", splitProof: proof),
                AblationAccuracy(evaluationId: "ablated", splitProof: proof)),
            "ABLATION_EVALUATION_MISMATCH",
            "Dynamic graph ablation measurements do not describe the same held-out evaluation.");

        AssertEdgeQualityEvidenceFailure(
            DynamicGraphMeasurementCoordinator.MeasureEdgeQualityEvidence(
                target,
                EdgeOrderQualityResult(),
                NoveltyObservation(
                    evaluationId: "novelty-evaluation",
                    requestedRows: 50,
                    rowsEvaluated: 50,
                    splitProof: proof),
                AblationAccuracy(evaluationId: "ablation-evaluation", splitProof: proof),
                AblationAccuracy(
                    evaluationId: "ablation-evaluation",
                    heldOutAccuracy: 0.93d,
                    splitProof: proof)),
            "EDGE_QUALITY_EVALUATION_MISMATCH",
            "Dynamic graph edge quality evidence does not describe the same held-out evaluation.");
    }

    [Fact]
    public void MeasureEdgeQualityEvidence_AuthorityErrorPrecedesOrdinalTargetMismatch()
    {
        var proof = AblationProof();
        var novelty = NoveltyObservation(
            evaluationId: "eval-1",
            requestedRows: 50,
            rowsEvaluated: 50,
            splitProof: proof);
        var subject = AblationAccuracy(splitProof: proof);
        var ablated = AblationAccuracy(heldOutAccuracy: 0.93d, splitProof: proof);
        Assert.True(DynamicGraphNoveltyMeter.Measure(novelty).IsSuccess);
        Assert.True(DynamicGraphAblationMeter.Measure(subject, ablated).IsSuccess);

        var result = DynamicGraphMeasurementCoordinator.MeasureEdgeQualityEvidence(
            Target(
                "EDGE-ORDER-QUALITY-COORDINATOR",
                DynamicGraphMeasurementTargetLevel.L3LogicalFlow),
            CorruptedEdgeAuthorityEvidenceResult(),
            novelty,
            subject,
            ablated);

        AssertEdgeQualityEvidenceFailure(
            result,
            "INVALID_EDGE_AUTHORITY_CONTROL_MEASUREMENT",
            "Dynamic graph edge authority control measurement is invalid.");
    }

    [Theory]
    [InlineData("different-edge")]
    [InlineData("EDGE-ORDER-QUALITY-COORDINATOR")]
    public void MeasureEdgeQualityEvidence_RequiresOrdinalTargetToAuthorityEdgeMatch(string targetId)
    {
        var proof = AblationProof();
        var result = DynamicGraphMeasurementCoordinator.MeasureEdgeQualityEvidence(
            Target(targetId, DynamicGraphMeasurementTargetLevel.L3LogicalFlow),
            EdgeOrderQualityResult(),
            NoveltyObservation(
                evaluationId: "eval-1",
                requestedRows: 50,
                rowsEvaluated: 50,
                splitProof: proof),
            AblationAccuracy(splitProof: proof),
            AblationAccuracy(heldOutAccuracy: 0.93d, splitProof: proof));

        AssertEdgeQualityEvidenceFailure(
            result,
            EdgeQualityEvidenceTargetMismatchCode,
            EdgeQualityEvidenceTargetMismatchMessage);
    }

    [Fact]
    public void MeasureEdgeQualityEvidence_CoordinatorBodyHasNoExceptionHandlingClauses()
    {
        var method = Assert.IsAssignableFrom<MethodInfo>(
            typeof(DynamicGraphMeasurementCoordinator).GetMethod(
                nameof(DynamicGraphMeasurementCoordinator.MeasureEdgeQualityEvidence),
                BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
        var body = Assert.IsAssignableFrom<MethodBody>(method.GetMethodBody());

        Assert.Empty(body.ExceptionHandlingClauses);
    }

    [Fact]
    public void MeasureEdgeAuthoritySelectionQualityEvidence_MatchesDirect20AgFieldForField()
    {
        var target = Target(
            EdgeOrderQualityId,
            DynamicGraphMeasurementTargetLevel.L3LogicalFlow);
        var authority = EdgeOrderQualityResult();
        var request = SelectionNoveltyRequest(SelectionPaired());
        var direct = DynamicGraphEdgeQualityEvidenceMeter.MeasureEdgeAuthoritySelection(
            authority,
            request);

        var result = DynamicGraphMeasurementCoordinator
            .MeasureEdgeAuthoritySelectionQualityEvidence(target, authority, request);

        Assert.True(direct.IsSuccess, $"{direct.ErrorCode}: {direct.ErrorMessage}");
        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.NotSame(target, result.Value!.Target);
        Assert.Equal(target, result.Value.Target);
        AssertSelectionQualityEquivalent(
            direct.Value!,
            result.Value.EdgeAuthoritySelectionQualityEvidence);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("malformed")]
    [InlineData("shortcut")]
    [InlineData("deterministic")]
    [InlineData("non-l3")]
    public void MeasureEdgeAuthoritySelectionQualityEvidence_TargetPrecedesHostileDownstream(
        string targetCase)
    {
        var target = targetCase switch
        {
            "null" => null,
            "malformed" => Target(
                " malformed",
                DynamicGraphMeasurementTargetLevel.L3LogicalFlow),
            "shortcut" => Target(
                EdgeOrderQualityId,
                DynamicGraphMeasurementTargetLevel.L3LogicalFlow,
                DynamicGraphMeasurementMechanismKind.Knn),
            "deterministic" => Target(
                EdgeOrderQualityId,
                DynamicGraphMeasurementTargetLevel.L3LogicalFlow,
                deterministicFinalAuthority: true),
            "non-l3" => Target(
                EdgeOrderQualityId,
                DynamicGraphMeasurementTargetLevel.L2LogicalUnit),
            _ => throw new ArgumentOutOfRangeException(nameof(targetCase))
        };

        var result = DynamicGraphMeasurementCoordinator
            .MeasureEdgeAuthoritySelectionQualityEvidence(
                target,
                CorruptedEdgeAuthorityEvidenceResult(),
                HostileSelectionNoveltyRequest(
                    new BadImageFormatException("must not traverse downstream")));

        AssertSelectionTargetFailure(
            result,
            targetCase switch
            {
                "shortcut" or "deterministic" => "FORBIDDEN_RUNTIME_SHORTCUT",
                "non-l3" => InvalidEdgeQualityEvidenceTargetLevelCode,
                _ => "INVALID_DYNAMIC_GRAPH_MEASUREMENT_TARGET"
            },
            targetCase switch
            {
                "shortcut" or "deterministic" =>
                    "Dynamic graph measurement target is a forbidden runtime shortcut.",
                "non-l3" => InvalidEdgeQualityEvidenceTargetLevelMessage,
                _ => "Dynamic graph measurement target is invalid."
            });
    }

    [Fact]
    public void MeasureEdgeAuthoritySelectionQualityEvidence_DownstreamErrorsPrecedeFinalTargetBinding()
    {
        var mismatchedTarget = Target(
            "different-target",
            DynamicGraphMeasurementTargetLevel.L3LogicalFlow);

        AssertSelectionTargetFailure(
            DynamicGraphMeasurementCoordinator.MeasureEdgeAuthoritySelectionQualityEvidence(
                mismatchedTarget,
                null,
                HostileSelectionNoveltyRequest(
                    new BadImageFormatException("authority error must win"))),
            "INVALID_EDGE_AUTHORITY_CONTROL_MEASUREMENT",
            "Dynamic graph edge authority control measurement is invalid.");

        AssertSelectionTargetFailure(
            DynamicGraphMeasurementCoordinator.MeasureEdgeAuthoritySelectionQualityEvidence(
                mismatchedTarget,
                EdgeOrderQualityResult(),
                null),
            "EDGE_AUTHORITY_NOVELTY_REQUEST_REQUIRED",
            "Dynamic graph edge authority novelty evaluation request is required.");
    }

    [Fact]
    public void MeasureEdgeAuthoritySelectionQualityEvidence_Propagates20AgCorrelationFailure()
    {
        var result = DynamicGraphMeasurementCoordinator
            .MeasureEdgeAuthoritySelectionQualityEvidence(
                Target(EdgeOrderQualityId, DynamicGraphMeasurementTargetLevel.L3LogicalFlow),
                EdgeOrderQualityResult(),
                SelectionNoveltyRequest(SelectionPaired(edgeId: "different-paired-edge")));

        AssertSelectionTargetFailure(
            result,
            "EDGE_AUTHORITY_SELECTION_QUALITY_EVIDENCE_MISMATCH",
            "Dynamic graph edge authority selection quality evidence is not correlated.");
    }

    [Theory]
    [InlineData("different-edge")]
    [InlineData("EDGE-ORDER-QUALITY-COORDINATOR")]
    [InlineData(EdgeOrderQualityUnitId)]
    [InlineData(SelectionEvaluationId)]
    public void MeasureEdgeAuthoritySelectionQualityEvidence_UsesOnlyOrdinalTargetToEdgeBinding(
        string targetId)
    {
        var result = DynamicGraphMeasurementCoordinator
            .MeasureEdgeAuthoritySelectionQualityEvidence(
                Target(targetId, DynamicGraphMeasurementTargetLevel.L3LogicalFlow),
                EdgeOrderQualityResult(),
                SelectionNoveltyRequest(SelectionPaired()));

        AssertSelectionTargetFailure(
            result,
            EdgeQualityEvidenceTargetMismatchCode,
            EdgeQualityEvidenceTargetMismatchMessage);
    }

    [Fact]
    public void MeasureEdgeAuthoritySelectionQualityEvidence_FalseComponentGatesRemainSuccess()
    {
        var paired = SelectionPaired(learnedCorrect: 1, baselineCorrect: 1);
        var requested = paired.Rows.Select(row => row.RowId)
            .Append("requested-but-missing")
            .ToArray();

        var result = DynamicGraphMeasurementCoordinator
            .MeasureEdgeAuthoritySelectionQualityEvidence(
                Target(EdgeOrderQualityId, DynamicGraphMeasurementTargetLevel.L3LogicalFlow),
                EdgeOrderQualityResult(),
                SelectionNoveltyRequest(paired, requestedRowIds: requested));

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        var evidence = result.Value!.EdgeAuthoritySelectionQualityEvidence;
        Assert.False(evidence.NoveltyEvaluation.Measurement.MeetsNoveltyGate);
        Assert.False(evidence.SelectionAblation.MeetsQ11NumericalGate);
    }

    [Fact]
    public void MeasureEdgeAuthoritySelectionQualityEvidence_DeepOwnsInputAndReturnedSnapshots()
    {
        var patterns = new List<string> { A };
        var tokens = new List<string> { A };
        var training = new List<DynamicGraphEdgeAuthorityNoveltyTrainingBinding>
        {
            new("train-a", "train-hash", patterns, tokens)
        };
        var requested = new List<string> { "row-000", "row-001" };
        var rowTokens = new List<string> { B };
        var noveltyRows = new List<DynamicGraphEdgeAuthorityNoveltyRowInput>
        {
            new("row-000", B, rowTokens),
            new("row-001", C, [C])
        };
        var request = SelectionNoveltyRequest(
            SelectionPaired(),
            training,
            requested,
            noveltyRows);
        var target = Target(
            EdgeOrderQualityId,
            DynamicGraphMeasurementTargetLevel.L3LogicalFlow);

        var result = DynamicGraphMeasurementCoordinator
            .MeasureEdgeAuthoritySelectionQualityEvidence(
                target,
                EdgeOrderQualityResult(),
                request);
        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");

        patterns[0] = C;
        tokens[0] = C;
        requested[0] = "caller-mutated";
        rowTokens[0] = A;
        training[0] = new("caller-mutated", "caller-mutated", [C], [C]);
        noveltyRows[0] = new("caller-mutated", A, [A]);
        SetRecordProperty(target, nameof(DynamicGraphMeasurementTarget.TargetId), "caller-mutated");

        var firstTarget = result.Value!.Target;
        var firstEvidence = result.Value.EdgeAuthoritySelectionQualityEvidence;
        var secondEvidence = result.Value.EdgeAuthoritySelectionQualityEvidence;
        Assert.NotSame(firstEvidence, secondEvidence);
        Assert.NotSame(firstEvidence.NoveltyEvaluation, secondEvidence.NoveltyEvaluation);
        Assert.NotSame(
            firstEvidence.NoveltyEvaluation.PairedSelection,
            secondEvidence.NoveltyEvaluation.PairedSelection);
        Assert.Equal("row-000", secondEvidence.NoveltyEvaluation.RequestedRowIds[0]);
        Assert.Equal(
            A,
            secondEvidence.NoveltyEvaluation.TrainingEvidence[0]
                .CallerAttestedPatternFingerprints[0]);
        Assert.Equal(B, secondEvidence.NoveltyEvaluation.Rows[0].CallerAttestedTokenFingerprints[0]);

        SetRecordProperty(firstTarget, nameof(DynamicGraphMeasurementTarget.TargetId),
            "returned-copy-mutated");
        var firstNovelty = GetPrivateCarrier<DynamicGraphEdgeAuthorityNoveltyEvaluationResult>(
            firstEvidence,
            "_noveltyEvaluation");
        GetPrivateStringArray(firstNovelty, "_requestedRowIds")[0] = "returned-copy-mutated";
        var firstPaired = GetPrivateCarrier<DynamicGraphEdgeAuthorityAblationEvaluationResult>(
            firstNovelty,
            "_pairedSelection");
        SetRecordProperty(
            firstPaired,
            nameof(DynamicGraphEdgeAuthorityAblationEvaluationResult.EdgeId),
            "returned-copy-mutated");

        Assert.Equal(EdgeOrderQualityId, result.Value.Target.TargetId);
        Assert.Equal(
            "row-000",
            result.Value.EdgeAuthoritySelectionQualityEvidence.NoveltyEvaluation.RequestedRowIds[0]);
        Assert.Equal(
            EdgeOrderQualityId,
            result.Value.EdgeAuthoritySelectionQualityEvidence.NoveltyEvaluation.PairedSelection.EdgeId);
    }

    [Fact]
    public void MeasureEdgeAuthoritySelectionQualityEvidence_HasExactSynchronousBoundary()
    {
        var method = Assert.IsAssignableFrom<MethodInfo>(
            typeof(DynamicGraphMeasurementCoordinator).GetMethod(
                nameof(DynamicGraphMeasurementCoordinator
                    .MeasureEdgeAuthoritySelectionQualityEvidence),
                BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
        var body = Assert.IsAssignableFrom<MethodBody>(method.GetMethodBody());

        Assert.False(method.IsGenericMethod);
        Assert.Empty(body.ExceptionHandlingClauses);
        Assert.Equal(
            new[]
            {
                typeof(DynamicGraphMeasurementTarget),
                typeof(EdgeAuthorityExecutionResult),
                typeof(DynamicGraphEdgeAuthorityNoveltyEvaluationRequest)
            },
            method.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.DoesNotContain(
            method.GetParameters(),
            parameter => parameter.ParameterType == typeof(CancellationToken)
                || parameter.ParameterType == typeof(DynamicGraphAccuracyMeasurement)
                || parameter.ParameterType == typeof(DynamicGraphNoveltyObservation));
    }

    [Fact]
    public void MeasureEdgeIdentityQuality_CanonicalL3TargetReturnsOwnedTargetAndAllMeterEvidence()
    {
        var target = Target(
            targetId: EdgeOrderQualityId,
            level: DynamicGraphMeasurementTargetLevel.L3LogicalFlow);
        var carrier = EdgeOrderQualityResult();

        var direct = DynamicGraphEdgeIdentityQualityMeter.Measure(carrier);
        var result = DynamicGraphMeasurementCoordinator.MeasureEdgeIdentityQuality(target, carrier);

        Assert.True(direct.IsSuccess, $"{direct.ErrorCode}: {direct.ErrorMessage}");
        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.Equal(target, result.Value!.Target);
        Assert.NotSame(target, result.Value.Target);
        Assert.NotSame(direct.Value, result.Value.EdgeIdentityQuality);
        AssertEdgeIdentityQualityEquivalent(direct.Value!, result.Value.EdgeIdentityQuality);

        var firstProbeIds = result.Value.EdgeIdentityQuality.ProbeIds;
        var secondProbeIds = result.Value.EdgeIdentityQuality.ProbeIds;
        var firstControlledSignals = result.Value.EdgeIdentityQuality.ControlledSignals;
        var secondControlledSignals = result.Value.EdgeIdentityQuality.ControlledSignals;
        var firstControlValues = result.Value.EdgeIdentityQuality.ControlValues;
        var secondControlValues = result.Value.EdgeIdentityQuality.ControlValues;
        var firstExclusions = result.Value.EdgeIdentityQuality.ExcludedLeakageSignals;
        var secondExclusions = result.Value.EdgeIdentityQuality.ExcludedLeakageSignals;
        Assert.NotSame(firstProbeIds, secondProbeIds);
        Assert.NotSame(firstControlledSignals, secondControlledSignals);
        Assert.NotSame(firstControlValues, secondControlValues);
        Assert.NotSame(firstExclusions, secondExclusions);
        Assert.Throws<NotSupportedException>(() => ((IList<string>)firstProbeIds)[0] = "forged-probe");
        Assert.Throws<NotSupportedException>(() => ((IList<string>)firstControlledSignals)[0] = "forged-signal");
        Assert.Throws<NotSupportedException>(() => ((IList<string>)firstControlValues)[0] = "forged-control");
        Assert.Throws<NotSupportedException>(() => ((IList<string>)firstExclusions)[0] = "forged-exclusion");
        Assert.Equal(IdentityProbeIds[0], secondProbeIds[0]);
        Assert.Equal(IdentityControlledSignals[0], secondControlledSignals[0]);
        Assert.Equal(IdentityControlValues[0], secondControlValues[0]);
        Assert.Equal(EdgeOrderQualityLeakageSignals[0], secondExclusions[0]);
    }

    [Theory]
    [InlineData(DynamicGraphMeasurementMechanismKind.Knn)]
    [InlineData(DynamicGraphMeasurementMechanismKind.Scorer)]
    [InlineData(DynamicGraphMeasurementMechanismKind.Counter)]
    [InlineData(DynamicGraphMeasurementMechanismKind.Regex)]
    public void MeasureEdgeIdentityQuality_ShortcutTargetPrecedesCorruptedCarrier(
        DynamicGraphMeasurementMechanismKind mechanismKind)
    {
        AssertEdgeIdentityQualityFailure(
            DynamicGraphMeasurementCoordinator.MeasureEdgeIdentityQuality(
                Target(
                    targetId: EdgeOrderQualityId,
                    level: DynamicGraphMeasurementTargetLevel.L3LogicalFlow,
                    mechanismKind: mechanismKind),
                CorruptedEdgeOrderQualityResult()),
            "FORBIDDEN_RUNTIME_SHORTCUT",
            "Dynamic graph measurement target is a forbidden runtime shortcut.");
    }

    [Fact]
    public void MeasureEdgeIdentityQuality_DeterministicAuthorityPrecedesCorruptedCarrier()
    {
        AssertEdgeIdentityQualityFailure(
            DynamicGraphMeasurementCoordinator.MeasureEdgeIdentityQuality(
                Target(
                    targetId: EdgeOrderQualityId,
                    level: DynamicGraphMeasurementTargetLevel.L3LogicalFlow,
                    deterministicFinalAuthority: true),
                CorruptedEdgeOrderQualityResult()),
            "FORBIDDEN_RUNTIME_SHORTCUT",
            "Dynamic graph measurement target is a forbidden runtime shortcut.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("edge\u0001id")]
    public void MeasureEdgeIdentityQuality_MalformedTargetPrecedesCorruptedCarrier(string? targetId)
    {
        AssertEdgeIdentityQualityFailure(
            DynamicGraphMeasurementCoordinator.MeasureEdgeIdentityQuality(
                targetId is null
                    ? null
                    : Target(
                        targetId: targetId,
                        level: DynamicGraphMeasurementTargetLevel.L3LogicalFlow),
                CorruptedEdgeOrderQualityResult()),
            "INVALID_DYNAMIC_GRAPH_MEASUREMENT_TARGET",
            "Dynamic graph measurement target is invalid.");
    }

    [Theory]
    [InlineData(DynamicGraphMeasurementTargetLevel.Pair)]
    [InlineData(DynamicGraphMeasurementTargetLevel.L2LogicalUnit)]
    [InlineData(DynamicGraphMeasurementTargetLevel.L4LogicalOrchestrator)]
    [InlineData(DynamicGraphMeasurementTargetLevel.L5LearningMachine)]
    public void MeasureEdgeIdentityQuality_NonL3TargetPrecedesCorruptedCarrier(
        DynamicGraphMeasurementTargetLevel level)
    {
        AssertEdgeIdentityQualityFailure(
            DynamicGraphMeasurementCoordinator.MeasureEdgeIdentityQuality(
                Target(targetId: EdgeOrderQualityId, level: level),
                CorruptedEdgeOrderQualityResult()),
            "DYNAMIC_GRAPH_EDGE_IDENTITY_QUALITY_TARGET_LEVEL_INVALID",
            "Dynamic graph edge identity quality can only be associated with an L3 logical-flow target.");
    }

    [Fact]
    public void MeasureEdgeIdentityQuality_ValidL3TargetPreservesNullAndCorruptedMeterErrors()
    {
        EdgeAuthorityExecutionResult?[] invalidResults =
        [
            null,
            CorruptedEdgeOrderQualityResult()
        ];

        foreach (var invalidResult in invalidResults)
        {
            AssertEdgeIdentityQualityFailure(
                DynamicGraphMeasurementCoordinator.MeasureEdgeIdentityQuality(
                    Target(
                        targetId: EdgeOrderQualityId,
                        level: DynamicGraphMeasurementTargetLevel.L3LogicalFlow),
                    invalidResult),
                "INVALID_EDGE_IDENTITY_QUALITY_MEASUREMENT",
                "Dynamic graph edge identity quality measurement is invalid.");
        }
    }

    [Theory]
    [InlineData("different-edge")]
    [InlineData("EDGE-ORDER-QUALITY-COORDINATOR")]
    [InlineData(EdgeOrderQualityUnitId)]
    public void MeasureEdgeIdentityQuality_ValidEvidenceRequiresOrdinalEdgeTargetMatch(string targetId)
    {
        AssertEdgeIdentityQualityFailure(
            DynamicGraphMeasurementCoordinator.MeasureEdgeIdentityQuality(
                Target(
                    targetId: targetId,
                    level: DynamicGraphMeasurementTargetLevel.L3LogicalFlow),
                EdgeOrderQualityResult()),
            "DYNAMIC_GRAPH_EDGE_IDENTITY_QUALITY_TARGET_MISMATCH",
            "Dynamic graph edge identity quality target does not match the measured edge.");
    }

    [Fact]
    public void MeasureEdgeIdentityQuality_CoordinatorBodyHasNoExceptionHandlingClauses()
    {
        var method = Assert.IsAssignableFrom<MethodInfo>(
            typeof(DynamicGraphMeasurementCoordinator).GetMethod(
                nameof(DynamicGraphMeasurementCoordinator.MeasureEdgeIdentityQuality),
                BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
        var body = Assert.IsAssignableFrom<MethodBody>(method.GetMethodBody());

        Assert.Empty(body.ExceptionHandlingClauses);
    }

    [Fact]
    public void MeasureEdgeOrderQuality_CanonicalL3TargetReturnsOwnedTargetAndAllMeterEvidence()
    {
        var target = Target(
            targetId: EdgeOrderQualityId,
            level: DynamicGraphMeasurementTargetLevel.L3LogicalFlow);
        var carrier = EdgeOrderQualityResult();

        var direct = DynamicGraphEdgeOrderQualityMeter.Measure(carrier);
        var result = DynamicGraphMeasurementCoordinator.MeasureEdgeOrderQuality(target, carrier);

        Assert.True(direct.IsSuccess, $"{direct.ErrorCode}: {direct.ErrorMessage}");
        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.Equal(target, result.Value!.Target);
        Assert.NotSame(target, result.Value.Target);
        AssertEdgeOrderQualityEquivalent(direct.Value!, result.Value.EdgeOrderQuality);

        var firstProbeIds = result.Value.EdgeOrderQuality.ProbeIds;
        var secondProbeIds = result.Value.EdgeOrderQuality.ProbeIds;
        var firstExclusions = result.Value.EdgeOrderQuality.ExcludedLeakageSignals;
        var secondExclusions = result.Value.EdgeOrderQuality.ExcludedLeakageSignals;
        Assert.NotSame(firstProbeIds, secondProbeIds);
        Assert.NotSame(firstExclusions, secondExclusions);
        Assert.Throws<NotSupportedException>(() => ((IList<string>)firstProbeIds)[0] = "forged-probe");
        Assert.Throws<NotSupportedException>(() => ((IList<string>)firstExclusions)[0] = "forged-exclusion");
        Assert.Equal("candidate-order-permutation-01", secondProbeIds[0]);
        Assert.Equal("candidate_index", secondExclusions[0]);
    }

    [Theory]
    [InlineData(DynamicGraphMeasurementMechanismKind.Knn)]
    [InlineData(DynamicGraphMeasurementMechanismKind.Scorer)]
    [InlineData(DynamicGraphMeasurementMechanismKind.Counter)]
    [InlineData(DynamicGraphMeasurementMechanismKind.Regex)]
    public void MeasureEdgeOrderQuality_ShortcutTargetPrecedesCorruptedCarrier(
        DynamicGraphMeasurementMechanismKind mechanismKind)
    {
        AssertEdgeOrderQualityFailure(
            DynamicGraphMeasurementCoordinator.MeasureEdgeOrderQuality(
                Target(
                    targetId: EdgeOrderQualityId,
                    level: DynamicGraphMeasurementTargetLevel.L3LogicalFlow,
                    mechanismKind: mechanismKind),
                CorruptedEdgeOrderQualityResult()),
            "FORBIDDEN_RUNTIME_SHORTCUT",
            "Dynamic graph measurement target is a forbidden runtime shortcut.");
    }

    [Fact]
    public void MeasureEdgeOrderQuality_DeterministicAuthorityPrecedesCorruptedCarrier()
    {
        AssertEdgeOrderQualityFailure(
            DynamicGraphMeasurementCoordinator.MeasureEdgeOrderQuality(
                Target(
                    targetId: EdgeOrderQualityId,
                    level: DynamicGraphMeasurementTargetLevel.L3LogicalFlow,
                    deterministicFinalAuthority: true),
                CorruptedEdgeOrderQualityResult()),
            "FORBIDDEN_RUNTIME_SHORTCUT",
            "Dynamic graph measurement target is a forbidden runtime shortcut.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("edge\u0001id")]
    public void MeasureEdgeOrderQuality_MalformedTargetPrecedesCorruptedCarrier(string? targetId)
    {
        AssertEdgeOrderQualityFailure(
            DynamicGraphMeasurementCoordinator.MeasureEdgeOrderQuality(
                targetId is null
                    ? null
                    : Target(
                        targetId: targetId,
                        level: DynamicGraphMeasurementTargetLevel.L3LogicalFlow),
                CorruptedEdgeOrderQualityResult()),
            "INVALID_DYNAMIC_GRAPH_MEASUREMENT_TARGET",
            "Dynamic graph measurement target is invalid.");
    }

    [Theory]
    [InlineData(DynamicGraphMeasurementTargetLevel.Pair)]
    [InlineData(DynamicGraphMeasurementTargetLevel.L2LogicalUnit)]
    [InlineData(DynamicGraphMeasurementTargetLevel.L4LogicalOrchestrator)]
    [InlineData(DynamicGraphMeasurementTargetLevel.L5LearningMachine)]
    public void MeasureEdgeOrderQuality_NonL3TargetPrecedesCorruptedCarrier(
        DynamicGraphMeasurementTargetLevel level)
    {
        AssertEdgeOrderQualityFailure(
            DynamicGraphMeasurementCoordinator.MeasureEdgeOrderQuality(
                Target(targetId: EdgeOrderQualityId, level: level),
                CorruptedEdgeOrderQualityResult()),
            "DYNAMIC_GRAPH_EDGE_ORDER_QUALITY_TARGET_LEVEL_INVALID",
            "Dynamic graph edge order quality can only be associated with an L3 logical-flow target.");
    }

    [Fact]
    public void MeasureEdgeOrderQuality_ValidL3TargetPreservesNullAndCorruptedMeterErrors()
    {
        EdgeAuthorityExecutionResult?[] invalidResults =
        [
            null,
            CorruptedEdgeOrderQualityResult()
        ];

        foreach (var invalidResult in invalidResults)
        {
            AssertEdgeOrderQualityFailure(
                DynamicGraphMeasurementCoordinator.MeasureEdgeOrderQuality(
                    Target(
                        targetId: EdgeOrderQualityId,
                        level: DynamicGraphMeasurementTargetLevel.L3LogicalFlow),
                    invalidResult),
                "INVALID_EDGE_ORDER_QUALITY_MEASUREMENT",
                "Dynamic graph edge order quality measurement is invalid.");
        }
    }

    [Theory]
    [InlineData("different-edge")]
    [InlineData("EDGE-ORDER-QUALITY-COORDINATOR")]
    public void MeasureEdgeOrderQuality_ValidEvidenceRequiresOrdinalTargetMatch(string targetId)
    {
        AssertEdgeOrderQualityFailure(
            DynamicGraphMeasurementCoordinator.MeasureEdgeOrderQuality(
                Target(
                    targetId: targetId,
                    level: DynamicGraphMeasurementTargetLevel.L3LogicalFlow),
                EdgeOrderQualityResult()),
            "DYNAMIC_GRAPH_EDGE_ORDER_QUALITY_TARGET_MISMATCH",
            "Dynamic graph edge order quality target does not match the measured edge.");
    }

    [Fact]
    public void MeasureEdgeOrderQuality_CoordinatorBodyHasNoExceptionHandlingClauses()
    {
        var method = Assert.IsAssignableFrom<MethodInfo>(
            typeof(DynamicGraphMeasurementCoordinator).GetMethod(
                nameof(DynamicGraphMeasurementCoordinator.MeasureEdgeOrderQuality),
                BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
        var body = Assert.IsAssignableFrom<MethodBody>(method.GetMethodBody());

        Assert.Empty(body.ExceptionHandlingClauses);
    }

    private static DynamicGraphUntrainedHonestyL2ProducerEvidence UntrainedHonestyProducer(
        string unitId = UntrainedHonestyUnitId,
        string unitRole = "untrained-honesty-role",
        string domainTag = "untrained-honesty-domain",
        string version = "untrained-honesty:v1",
        long registrationOrder = 7,
        string producerTypeName = "Producer.Type",
        string inputTypeName = "Input.Type",
        string outputTypeName = "Output.Type")
        => new(
            unitId,
            unitRole,
            domainTag,
            version,
            registrationOrder,
            producerTypeName,
            inputTypeName,
            outputTypeName);

    private static DynamicGraphUntrainedHonestyMeasurement UntrainedHonestyMeasurement(
        string evaluationId = "evaluation:coordinator-bind:001",
        bool runtimeReportedNotReady = true,
        bool hasNoTrainedStateEvidence = true,
        bool hasNoSemanticClassificationEvidence = true,
        IReadOnlyList<string>? runtimeEvidence = null,
        IReadOnlyList<string>? inspectedTypeNames = null,
        IReadOnlyList<string>? forbiddenShortcutMatches = null,
        bool meetsUntrainedHonestyGate = true)
        => new(
            evaluationId,
            runtimeReportedNotReady,
            hasNoTrainedStateEvidence,
            hasNoSemanticClassificationEvidence,
            runtimeEvidence ?? ValidUntrainedRuntimeEvidence(),
            inspectedTypeNames ?? ValidInspectedTypeNames(),
            forbiddenShortcutMatches ?? [],
            meetsUntrainedHonestyGate);

    private static DynamicGraphUntrainedHonestyL2ProbeResult UntrainedHonestyProbeResult(
        DynamicGraphUntrainedHonestyL2ProducerEvidence? producer = null,
        DynamicGraphUntrainedHonestyMeasurement? honesty = null)
        => new(
            producer ?? UntrainedHonestyProducer(),
            honesty ?? UntrainedHonestyMeasurement());

    private static string[] ValidUntrainedRuntimeEvidence()
        => [NoSemanticClassification, NoTrainedState, "RUNTIME_REPORTED_NOT_READY"];

    private static string[] ValidInspectedTypeNames()
        => ["Producer.Type", "Supporting.Type"];

    private static DynamicGraphUntrainedHonestyL2ProbeResult HostileUntrainedHonestyProbeResult(
        Exception exception)
        => UntrainedHonestyProbeResult(
            honesty: UntrainedHonestyMeasurement(
                runtimeEvidence: new FaultingReadOnlyList<string>(exception)));

    private static PromptToTaskRequest CreateCoordinatorPromptToTaskRequest()
        => new(
            "prompt:coordinator-bind:001",
            "Create exactly one bounded work order.",
            "sha256:coordinator-bind-001",
            "test:dynamic-graph-untrained-honesty-coordinator",
            "en",
            ["no semantic overclaim"],
            new Dictionary<string, string>
            {
                ["source_root_id"] = "coordinator-bind-test"
            });

    private static Exception UntrainedHonestyExceptionGraph(string graph)
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

    private static void AssertInvalidUntrainedHonestyProbe(
        DynamicGraphUntrainedHonestyL2ProbeResult probe)
        => AssertUntrainedHonestyFailure(
            DynamicGraphMeasurementCoordinator.BindL2UntrainedHonesty(
                Target(UntrainedHonestyUnitId, DynamicGraphMeasurementTargetLevel.L2LogicalUnit),
                probe),
            InvalidUntrainedHonestyProbeResultCode,
            InvalidUntrainedHonestyProbeResultMessage);

    private static void AssertUntrainedHonestyFailure(
        OperationResult<DynamicGraphTargetUntrainedHonestyMeasurement> result,
        string expectedCode,
        string expectedMessage)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(expectedCode, result.ErrorCode);
        Assert.Equal(expectedMessage, result.ErrorMessage);
    }

    private static EdgeAuthorityExecutionResult EdgeOrderQualityResult()
    {
        var candidates = Enumerable.Range(0, 3)
            .Select(index => new EdgeCandidate(
                $"target-{(char)('a' + index)}",
                [$"PREDICATE_{(char)('A' + index)}"],
                0.2d + (index * 0.2d),
                RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole))
            .ToArray();
        var proposedCandidates = candidates.Select(CopyEdgeOrderCandidate).ToArray();
        var decision = new EdgeAuthorityDecision(
            [("target-c", 0.93d)],
            1,
            false,
            RoutingAuthorityVocabulary.TrainedEdgeUnitRole,
            candidates
                .OrderByDescending(candidate => candidate.BaselineScore)
                .ThenBy(candidate => candidate.TargetSubPhaseId, StringComparer.Ordinal)
                .Select(candidate => candidate.TargetSubPhaseId)
                .ToArray());
        var probes = Enumerable.Range(0, 5)
            .Select(index => new DynamicGraphEdgeCandidateOrderPermutationProbe(
                $"candidate-order-permutation-{index + 1:D2}",
                CreateEdgeOrderPermutation(candidates, index),
                CopyEdgeOrderDecision(decision)))
            .ToArray();
        var normalizedInput = new EdgeAuthorityInput(
            EdgeOrderQualityId,
            ["edge", "order", "quality"],
            candidates.SelectMany(candidate => candidate.EvidencePredicates).ToArray(),
            new ContextSignals(
                [Domain.CODE_Q],
                ["completed-sub-phase"],
                ["active-edge"],
                new Dictionary<Domain, double> { [Domain.CODE_Q] = 0.8d }),
            candidates,
            EdgeOrderQualityLeakageSignals);
        var report = new DynamicGraphEdgeCandidateOrderLeakageReport(
            EdgeOrderQualityId,
            EdgeOrderQualityUnitId,
            true,
            probes,
            EdgeOrderQualityLeakageSignals);
        var identityReport = new DynamicGraphEdgeIdentityLeakageReport(
            EdgeOrderQualityId,
            EdgeOrderQualityUnitId,
            true,
            Enumerable.Range(0, 3)
                .Select(index => new DynamicGraphEdgeIdentityControlProbe(
                    IdentityProbeIds[index],
                    IdentityControlledSignals[index],
                    IdentityControlValues[index],
                    decision))
                .ToArray(),
            EdgeOrderQualityLeakageSignals);

        return new EdgeAuthorityExecutionResult(
            normalizedInput,
            proposedCandidates,
            CopyEdgeOrderDecision(decision),
            new DynamicGraphEdgeAuthorityEvidence(
                EdgeOrderQualityId,
                EdgeOrderQualityUnitId,
                "L2",
                false,
                "measurement-coordinator-baseline-unit",
                "candidate_generator",
                EdgeOrderQualityLeakageSignals,
                "measurement-coordinator-source-proof",
                "measurement-coordinator-cutover-proof",
                "measurement-coordinator-ablation-proof"),
            report,
            identityReport,
            0.93d,
            ["evidence"],
            []);
    }

    private static EdgeAuthorityExecutionResult CorruptedEdgeOrderQualityResult()
    {
        var result = EdgeOrderQualityResult();
        var normalizedInputField = Assert.IsAssignableFrom<FieldInfo>(
            typeof(EdgeAuthorityExecutionResult).GetField(
                "_normalizedInput",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly));
        Assert.True(normalizedInputField.IsInitOnly);
        normalizedInputField.SetValue(result, null);
        return result;
    }

    private static EdgeAuthorityExecutionResult CorruptedEdgeAuthorityEvidenceResult()
    {
        var result = EdgeOrderQualityResult();
        SetResultCarrier(result, "_authorityEvidence", null);
        return result;
    }

    private static EdgeAuthorityExecutionResult CorruptedEdgeIdentityQualityResult()
    {
        var result = EdgeOrderQualityResult();
        SetResultCarrier(result, "_identityLeakageReport", null);
        return result;
    }

    private static void SetResultCarrier(
        EdgeAuthorityExecutionResult result,
        string fieldName,
        object? value)
    {
        var field = Assert.IsAssignableFrom<FieldInfo>(
            typeof(EdgeAuthorityExecutionResult).GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly));
        Assert.True(field.IsInitOnly);
        field.SetValue(result, value);
    }

    private static EdgeCandidate[] CreateEdgeOrderPermutation(
        IReadOnlyList<EdgeCandidate> candidates,
        int index)
    {
        var source = candidates.Select(CopyEdgeOrderCandidate).ToArray();
        var offset = index switch
        {
            0 => 0,
            1 => 1,
            2 => 2,
            3 => -1,
            4 => -2,
            _ => throw new ArgumentOutOfRangeException(nameof(index))
        };
        if (offset < 0)
        {
            var reversed = source.Reverse().ToArray();
            return RotateEdgeOrderCandidates(reversed, -offset - 1);
        }

        return RotateEdgeOrderCandidates(source, offset);
    }

    private static EdgeCandidate[] RotateEdgeOrderCandidates(
        IReadOnlyList<EdgeCandidate> source,
        int offset)
        => Enumerable.Range(0, source.Count)
            .Select(index => CopyEdgeOrderCandidate(source[(index + offset) % source.Count]))
            .ToArray();

    private static EdgeCandidate CopyEdgeOrderCandidate(EdgeCandidate source)
        => source with { EvidencePredicates = source.EvidencePredicates.ToArray() };

    private static EdgeAuthorityDecision CopyEdgeOrderDecision(EdgeAuthorityDecision source)
        => new(
            source.OrderedTargets.ToArray(),
            source.FanOutCount,
            source.KnowledgeEdge,
            source.AuthoritySource,
            source.AblationBaselineRank.ToArray());

    private static DynamicGraphEdgeAuthorityNoveltyEvaluationRequest SelectionNoveltyRequest(
        DynamicGraphEdgeAuthorityAblationEvaluationResult paired,
        IReadOnlyList<DynamicGraphEdgeAuthorityNoveltyTrainingBinding>? trainingBindings = null,
        IReadOnlyList<string>? requestedRowIds = null,
        IReadOnlyList<DynamicGraphEdgeAuthorityNoveltyRowInput>? noveltyRows = null)
        => new(
            paired,
            trainingBindings ??
            [
                new DynamicGraphEdgeAuthorityNoveltyTrainingBinding(
                    "train-a",
                    "train-hash",
                    [A],
                    [A])
            ],
            requestedRowIds ?? paired.Rows.Select(row => row.RowId).ToArray(),
            noveltyRows ?? Enumerable.Range(0, paired.Rows.Count)
                .Select(index => new DynamicGraphEdgeAuthorityNoveltyRowInput(
                    $"row-{index:D3}",
                    index % 2 == 0 ? B : C,
                    [index % 2 == 0 ? B : C]))
                .ToArray(),
            "fingerprint-normalization-v1",
            "tokenization-v1");

    private static DynamicGraphEdgeAuthorityNoveltyEvaluationRequest HostileSelectionNoveltyRequest(
        Exception exception)
        => new(
            null,
            new FaultingReadOnlyList<DynamicGraphEdgeAuthorityNoveltyTrainingBinding>(exception),
            new FaultingReadOnlyList<string>(exception),
            new FaultingReadOnlyList<DynamicGraphEdgeAuthorityNoveltyRowInput>(exception),
            "normalization",
            "tokenization");

    private static DynamicGraphEdgeAuthorityAblationEvaluationResult SelectionPaired(
        int rowCount = 2,
        int learnedCorrect = 2,
        int baselineCorrect = 1,
        string evaluationId = SelectionEvaluationId,
        string edgeId = EdgeOrderQualityId,
        string authorityUnitId = EdgeOrderQualityUnitId)
    {
        var split = new DynamicGraphMeasurementSplitProof(
            ["train-a"],
            ["eval-a"],
            ["train-hash", "eval-hash"],
            0,
            "simhash",
            0.85d);
        var rows = Enumerable.Range(0, rowCount)
            .Select(index =>
            {
                var learnedMatches = index < learnedCorrect;
                var baselineMatches = index < baselineCorrect;
                return new DynamicGraphEdgeAuthorityAblationRowEvidence(
                    $"row-{index:D3}",
                    "eval-a",
                    "eval-hash",
                    "document-000",
                    "repo://eval/a",
                    $"label-{index:D3}",
                    "target-a",
                    learnedMatches ? "target-a" : "target-b",
                    baselineMatches ? "target-a" : "target-b",
                    learnedMatches,
                    baselineMatches);
            })
            .ToArray();
        var learnedAccuracy = (double)learnedCorrect / rowCount;
        var baselineAccuracy = (double)baselineCorrect / rowCount;
        var rawDrop = learnedAccuracy - baselineAccuracy;
        var normalizedDrop = Math.Round(rawDrop, 12, MidpointRounding.AwayFromZero);
        var constructor = Assert.Single(
            typeof(DynamicGraphEdgeAuthorityAblationEvaluationResult)
                .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance),
            candidate => candidate.GetParameters().Length == 12);
        return Assert.IsType<DynamicGraphEdgeAuthorityAblationEvaluationResult>(
            constructor.Invoke(
            [
                evaluationId,
                edgeId,
                authorityUnitId,
                split,
                rows,
                learnedCorrect,
                learnedAccuracy,
                baselineCorrect,
                baselineAccuracy,
                rawDrop,
                normalizedDrop,
                normalizedDrop >= DynamicGraphAblationMeter.MinimumAccuracyDrop
            ]));
    }

    private static void AssertSelectionTargetFailure(
        OperationResult<DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement> result,
        string expectedCode,
        string expectedMessage)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(expectedCode, result.ErrorCode, StringComparer.Ordinal);
        Assert.Equal(expectedMessage, result.ErrorMessage, StringComparer.Ordinal);
    }

    private static void AssertSelectionQualityEquivalent(
        DynamicGraphEdgeAuthoritySelectionQualityEvidenceMeasurement expected,
        DynamicGraphEdgeAuthoritySelectionQualityEvidenceMeasurement actual)
    {
        AssertEdgeAuthorityControlEquivalent(
            expected.EdgeAuthorityControl,
            actual.EdgeAuthorityControl);
        var expectedNovelty = expected.NoveltyEvaluation;
        var actualNovelty = actual.NoveltyEvaluation;
        Assert.Equal(
            expectedNovelty.CallerAttestedFingerprintNormalizationId,
            actualNovelty.CallerAttestedFingerprintNormalizationId,
            StringComparer.Ordinal);
        Assert.Equal(
            expectedNovelty.CallerAttestedTokenizationId,
            actualNovelty.CallerAttestedTokenizationId,
            StringComparer.Ordinal);
        Assert.Equal(expectedNovelty.MaxAccuracyRuleId, actualNovelty.MaxAccuracyRuleId);
        Assert.True(expectedNovelty.RequestedRowIds.SequenceEqual(
            actualNovelty.RequestedRowIds,
            StringComparer.Ordinal));
        Assert.True(expectedNovelty.MissingRowIds.SequenceEqual(
            actualNovelty.MissingRowIds,
            StringComparer.Ordinal));
        Assert.Equal(expectedNovelty.TrainingEvidence.Count, actualNovelty.TrainingEvidence.Count);
        for (var index = 0; index < expectedNovelty.TrainingEvidence.Count; index++)
        {
            var expectedTraining = expectedNovelty.TrainingEvidence[index];
            var actualTraining = actualNovelty.TrainingEvidence[index];
            Assert.Equal(expectedTraining.TrainSourceRoot, actualTraining.TrainSourceRoot);
            Assert.Equal(
                expectedTraining.CallerAttestedSourceHash,
                actualTraining.CallerAttestedSourceHash);
            Assert.True(expectedTraining.CallerAttestedPatternFingerprints.SequenceEqual(
                actualTraining.CallerAttestedPatternFingerprints,
                StringComparer.Ordinal));
            Assert.True(expectedTraining.CallerAttestedTokenFingerprints.SequenceEqual(
                actualTraining.CallerAttestedTokenFingerprints,
                StringComparer.Ordinal));
        }

        Assert.Equal(expectedNovelty.Rows.Count, actualNovelty.Rows.Count);
        for (var index = 0; index < expectedNovelty.Rows.Count; index++)
        {
            var expectedRow = expectedNovelty.Rows[index];
            var actualRow = actualNovelty.Rows[index];
            Assert.Equal(expectedRow.RowId, actualRow.RowId);
            Assert.Equal(expectedRow.EvalSourceRoot, actualRow.EvalSourceRoot);
            Assert.Equal(expectedRow.CallerAttestedSourceHash, actualRow.CallerAttestedSourceHash);
            Assert.Equal(expectedRow.TraversalDocumentId, actualRow.TraversalDocumentId);
            Assert.Equal(expectedRow.TraversalSourcePath, actualRow.TraversalSourcePath);
            Assert.Equal(
                expectedRow.CallerAttestedLabelProvenanceId,
                actualRow.CallerAttestedLabelProvenanceId);
            Assert.Equal(
                expectedRow.CallerAttestedPatternFingerprint,
                actualRow.CallerAttestedPatternFingerprint);
            Assert.True(expectedRow.CallerAttestedTokenFingerprints.SequenceEqual(
                actualRow.CallerAttestedTokenFingerprints,
                StringComparer.Ordinal));
            Assert.Equal(expectedRow.IsNewPattern, actualRow.IsNewPattern);
            Assert.True(expectedRow.SeenTrainingTokenFingerprints.SequenceEqual(
                actualRow.SeenTrainingTokenFingerprints,
                StringComparer.Ordinal));
        }

        AssertSelectionPairedEquivalent(
            expectedNovelty.PairedSelection,
            actualNovelty.PairedSelection);
        AssertNoveltyObservationEquivalent(
            expectedNovelty.Observation,
            actualNovelty.Observation);
        AssertNoveltyEquivalent(expectedNovelty.Measurement, actualNovelty.Measurement);
        AssertAblationEquivalent(expected.SelectionAblation, actual.SelectionAblation);
    }

    private static void AssertNoveltyObservationEquivalent(
        DynamicGraphNoveltyObservation expected,
        DynamicGraphNoveltyObservation actual)
    {
        Assert.Equal(expected.EvaluationId, actual.EvaluationId, StringComparer.Ordinal);
        Assert.Equal(expected.RequestedRows, actual.RequestedRows);
        Assert.Equal(expected.RowsEvaluated, actual.RowsEvaluated);
        Assert.Equal(expected.SelectionShortfall, actual.SelectionShortfall);
        Assert.Equal(expected.NewPatternRate, actual.NewPatternRate);
        Assert.Equal(expected.SourceTokenSeenRate, actual.SourceTokenSeenRate);
        Assert.Equal(expected.MaxAccuracy, actual.MaxAccuracy);
        Assert.Equal(expected.SplitProof.TrainSourceRoots, actual.SplitProof.TrainSourceRoots);
        Assert.Equal(expected.SplitProof.EvalSourceRoots, actual.SplitProof.EvalSourceRoots);
        Assert.Equal(expected.SplitProof.SourceHashes, actual.SplitProof.SourceHashes);
        Assert.Equal(
            expected.SplitProof.CrossSplitNearDuplicateCount,
            actual.SplitProof.CrossSplitNearDuplicateCount);
        Assert.Equal(expected.SplitProof.SimhashOrMinhash, actual.SplitProof.SimhashOrMinhash);
        Assert.Equal(expected.SplitProof.NearDuplicateThreshold, actual.SplitProof.NearDuplicateThreshold);
    }

    private static void AssertSelectionPairedEquivalent(
        DynamicGraphEdgeAuthorityAblationEvaluationResult expected,
        DynamicGraphEdgeAuthorityAblationEvaluationResult actual)
    {
        Assert.Equal(expected.EvaluationId, actual.EvaluationId, StringComparer.Ordinal);
        Assert.Equal(expected.EdgeId, actual.EdgeId, StringComparer.Ordinal);
        Assert.Equal(expected.AuthorityUnitId, actual.AuthorityUnitId, StringComparer.Ordinal);
        Assert.Equal(expected.ScorerId, actual.ScorerId, StringComparer.Ordinal);
        Assert.Equal(expected.ScorerVersion, actual.ScorerVersion, StringComparer.Ordinal);
        Assert.Equal(expected.ThresholdSetId, actual.ThresholdSetId, StringComparer.Ordinal);
        Assert.Equal(expected.NormalizationId, actual.NormalizationId, StringComparer.Ordinal);
        Assert.Equal(expected.RowsEvaluated, actual.RowsEvaluated);
        Assert.Equal(expected.LearnedCorrectCount, actual.LearnedCorrectCount);
        Assert.Equal(expected.LearnedSelectionAccuracy, actual.LearnedSelectionAccuracy);
        Assert.Equal(expected.BaselineCorrectCount, actual.BaselineCorrectCount);
        Assert.Equal(expected.BaselineSelectionAccuracy, actual.BaselineSelectionAccuracy);
        Assert.Equal(expected.AccuracyDrop, actual.AccuracyDrop);
        Assert.Equal(expected.NormalizedAccuracyDrop, actual.NormalizedAccuracyDrop);
        Assert.Equal(expected.MeetsQ11NumericalGate, actual.MeetsQ11NumericalGate);
        Assert.Equal(expected.Rows, actual.Rows);
        Assert.Equal(expected.SplitProof.TrainSourceRoots, actual.SplitProof.TrainSourceRoots);
        Assert.Equal(expected.SplitProof.EvalSourceRoots, actual.SplitProof.EvalSourceRoots);
        Assert.Equal(expected.SplitProof.SourceHashes, actual.SplitProof.SourceHashes);
        Assert.Equal(
            expected.SplitProof.CrossSplitNearDuplicateCount,
            actual.SplitProof.CrossSplitNearDuplicateCount);
        Assert.Equal(expected.SplitProof.SimhashOrMinhash, actual.SplitProof.SimhashOrMinhash);
        Assert.Equal(expected.SplitProof.NearDuplicateThreshold, actual.SplitProof.NearDuplicateThreshold);
    }

    private static DynamicGraphMeasurementTarget Target(
        string targetId = "target-a",
        DynamicGraphMeasurementTargetLevel level = DynamicGraphMeasurementTargetLevel.Pair,
        DynamicGraphMeasurementMechanismKind mechanismKind = DynamicGraphMeasurementMechanismKind.LearningSubject,
        bool deterministicFinalAuthority = false)
        => new(targetId, level, mechanismKind, deterministicFinalAuthority);

    private static LearningEvaluationResult Evaluation(
        string evaluationId = "eval-1",
        int rowsEvaluated = 50,
        double accuracy = 0.95d,
        double rewardMargin = 0.025d,
        bool passesGate = true)
        => new(evaluationId, rowsEvaluated, accuracy, rewardMargin, passesGate, "ignored source note");

    private static DynamicGraphMeasurementSplitProof Proof(
        IReadOnlyList<string>? trainRoots = null,
        IReadOnlyList<string>? evalRoots = null,
        IReadOnlyList<string>? hashes = null,
        int nearDuplicateCount = 0)
        => new(
            trainRoots ?? ["train-a"],
            evalRoots ?? ["eval-a"],
            hashes ?? ["hash-a"],
            nearDuplicateCount,
            "simhash",
            0.85d);

    private static DynamicGraphMeasurementSplitProof HostileProof(Exception exception)
        => Proof(trainRoots: new FaultingReadOnlyList<string>(exception));

    private static DynamicGraphNoveltyObservation NoveltyObservation(
        string evaluationId = "eval-novelty-1",
        int requestedRows = 100,
        int rowsEvaluated = 100,
        int selectionShortfall = 0,
        double newPatternRate = 0.26d,
        double sourceTokenSeenRate = 0.74d,
        double maxAccuracy = 0.75d,
        DynamicGraphMeasurementSplitProof? splitProof = null)
        => new(
            evaluationId,
            requestedRows,
            rowsEvaluated,
            selectionShortfall,
            newPatternRate,
            sourceTokenSeenRate,
            maxAccuracy,
            splitProof ?? NoveltyProof());

    private static DynamicGraphMeasurementSplitProof NoveltyProof(
        IReadOnlyList<string>? trainRoots = null,
        IReadOnlyList<string>? evalRoots = null,
        IReadOnlyList<string>? hashes = null,
        int nearDuplicateCount = 0,
        string method = "simhash",
        double threshold = 0.85d)
        => new(
            trainRoots ?? ["train-novelty-a"],
            evalRoots ?? ["eval-novelty-a"],
            hashes ?? ["hash-novelty-a"],
            nearDuplicateCount,
            method,
            threshold);

    private static DynamicGraphNoveltyObservation HostileNoveltyObservation(Exception exception)
        => NoveltyObservation(splitProof: NoveltyProof(
            trainRoots: new FaultingReadOnlyList<string>(exception)));

    private static Exception NoveltyExceptionGraph(string graph)
        => graph switch
        {
            "cancellation" => new OperationCanceledException("cancel"),
            "wrapped-cancellation" => new WrapperException(
                new OperationCanceledException("cancel")),
            "aggregate-cancellation" => new AggregateException(
                new InvalidOperationException("ordinary"),
                new WrapperException(new OperationCanceledException("cancel"))),
            "fatal" => new BadImageFormatException("fatal"),
            "wrapped-fatal" => new WrapperException(new BadImageFormatException("fatal")),
            "aggregate-fatal" => new AggregateException(
                new InvalidOperationException("ordinary"),
                new WrapperException(new BadImageFormatException("fatal"))),
            _ => throw new ArgumentOutOfRangeException(nameof(graph))
        };

    private static DynamicGraphRunSpeedObservation Observation(
        int rows = 25,
        long inferenceMs = 200,
        long routingMs = 37,
        IReadOnlyList<long>? candidateSamples = null,
        IReadOnlyList<long>? batchSamples = null)
        => new(
            rows,
            inferenceMs,
            routingMs,
            candidateSamples ?? [1, 3],
            batchSamples ?? [10, 20]);

    private static DynamicGraphRunSpeedObservation HostileObservation(
        Exception exception,
        bool faultBatch = false)
        => faultBatch
            ? Observation(batchSamples: new FaultingReadOnlyList<long>(exception))
            : Observation(candidateSamples: new FaultingReadOnlyList<long>(exception));

    private static LearningBatchResult Batch(
        string batchId = "batch-1",
        int rowsTrained = 11,
        double accuracyBefore = 0.10d,
        double accuracyAfter = 0.90d,
        double rewardMargin = 0.40d,
        long cpuMs = 101,
        long peakRamMb = 202,
        long elapsedMs = 303,
        IReadOnlyList<string>? promotedUnitIds = null)
        => new(
            batchId,
            rowsTrained,
            accuracyBefore,
            accuracyAfter,
            rewardMargin,
            cpuMs,
            peakRamMb,
            elapsedMs,
            promotedUnitIds ?? ["unit-ignored"]);

    private static LearningBatchResult HostileMalformedBatch()
        => Batch(
            batchId: " malformed",
            rowsTrained: 0,
            cpuMs: -1,
            peakRamMb: -1,
            elapsedMs: -1,
            promotedUnitIds: new FaultingReadOnlyList<string>(
                new InvalidOperationException("must not traverse")));

    private static DynamicGraphAccuracyMeasurement AblationAccuracy(
        string evaluationId = "eval-1",
        int rowsEvaluated = 50,
        double liveMarginAccuracy = 0.5d,
        double heldOutAccuracy = 0.95d,
        double accuracyDelta = 0.02d,
        double rewardMargin = 0.025d,
        bool meetsQ30 = true,
        DynamicGraphMeasurementSplitProof? splitProof = null)
        => new(
            evaluationId,
            rowsEvaluated,
            new DynamicGraphAccuracyAxis(liveMarginAccuracy, heldOutAccuracy, accuracyDelta),
            rewardMargin,
            meetsQ30,
            splitProof ?? AblationProof());

    private static DynamicGraphMeasurementSplitProof AblationProof(
        IReadOnlyList<string>? trainRoots = null,
        IReadOnlyList<string>? evalRoots = null,
        IReadOnlyList<string>? hashes = null,
        int nearDuplicateCount = 0,
        string method = "simhash",
        double threshold = 0.85d)
        => new(
            trainRoots ?? ["train-a", "train-b"],
            evalRoots ?? ["eval-a", "eval-b"],
            hashes ?? ["hash-a", "hash-b"],
            nearDuplicateCount,
            method,
            threshold);

    private static DynamicGraphAccuracyMeasurement HostileAblationAccuracy(Exception exception)
        => AblationAccuracy(splitProof: AblationProof(
            trainRoots: new FaultingReadOnlyList<string>(exception)));

    private static Exception AblationExceptionGraph(string graph)
        => graph switch
        {
            "cancellation" => new OperationCanceledException("cancel"),
            "wrapped-cancellation" => new WrapperException(
                new OperationCanceledException("cancel")),
            "aggregate-cancellation" => new AggregateException(
                new InvalidOperationException("ordinary"),
                new WrapperException(new OperationCanceledException("cancel"))),
            "fatal" => new BadImageFormatException("fatal"),
            "wrapped-fatal" => new WrapperException(new BadImageFormatException("fatal")),
            "aggregate-fatal" => new AggregateException(
                new InvalidOperationException("ordinary"),
                new WrapperException(new BadImageFormatException("fatal"))),
            _ => throw new ArgumentOutOfRangeException(nameof(graph))
        };

    private static void AssertAblationFailure(
        OperationResult<DynamicGraphTargetAblationMeasurement> result,
        string expectedCode,
        string expectedMessage)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(expectedCode, result.ErrorCode);
        Assert.Equal(expectedMessage, result.ErrorMessage);
    }

    private static void AssertAblationEquivalent(
        DynamicGraphAblationMeasurement expected,
        DynamicGraphAblationMeasurement actual)
    {
        Assert.Equal(expected.EvaluationId, actual.EvaluationId, StringComparer.Ordinal);
        Assert.Equal(expected.RowsEvaluated, actual.RowsEvaluated);
        Assert.Equal(expected.SubjectHeldOutAccuracy, actual.SubjectHeldOutAccuracy);
        Assert.Equal(expected.AblatedHeldOutAccuracy, actual.AblatedHeldOutAccuracy);
        Assert.Equal(expected.AccuracyDrop, actual.AccuracyDrop);
        Assert.Equal(expected.MeetsQ11NumericalGate, actual.MeetsQ11NumericalGate);
        Assert.True(expected.SplitProof.TrainSourceRoots.SequenceEqual(
            actual.SplitProof.TrainSourceRoots,
            StringComparer.Ordinal));
        Assert.True(expected.SplitProof.EvalSourceRoots.SequenceEqual(
            actual.SplitProof.EvalSourceRoots,
            StringComparer.Ordinal));
        Assert.True(expected.SplitProof.SourceHashes.SequenceEqual(
            actual.SplitProof.SourceHashes,
            StringComparer.Ordinal));
        Assert.Equal(
            expected.SplitProof.CrossSplitNearDuplicateCount,
            actual.SplitProof.CrossSplitNearDuplicateCount);
        Assert.Equal(
            expected.SplitProof.SimhashOrMinhash,
            actual.SplitProof.SimhashOrMinhash,
            StringComparer.Ordinal);
        Assert.Equal(expected.SplitProof.NearDuplicateThreshold, actual.SplitProof.NearDuplicateThreshold);
    }

    private static void AssertNoveltyFailure(
        OperationResult<DynamicGraphTargetNoveltyMeasurement> result,
        string expectedCode,
        string expectedMessage)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(expectedCode, result.ErrorCode);
        Assert.Equal(expectedMessage, result.ErrorMessage);
    }

    private static void AssertNoveltyEquivalent(
        DynamicGraphNoveltyMeasurement expected,
        DynamicGraphNoveltyMeasurement actual)
    {
        Assert.Equal(expected.EvaluationId, actual.EvaluationId, StringComparer.Ordinal);
        Assert.Equal(expected.RequestedRows, actual.RequestedRows);
        Assert.Equal(expected.RowsEvaluated, actual.RowsEvaluated);
        Assert.Equal(expected.SelectionShortfall, actual.SelectionShortfall);
        Assert.Equal(expected.NewPatternRate, actual.NewPatternRate);
        Assert.Equal(expected.SourceTokenSeenRate, actual.SourceTokenSeenRate);
        Assert.Equal(expected.MaxAccuracy, actual.MaxAccuracy);
        Assert.Equal(expected.MinimumNewPatternRate, actual.MinimumNewPatternRate);
        Assert.Equal(expected.MaximumSeenTokenRate, actual.MaximumSeenTokenRate);
        Assert.Equal(expected.MeetsNoveltyGate, actual.MeetsNoveltyGate);
        Assert.True(expected.SplitProof.TrainSourceRoots.SequenceEqual(
            actual.SplitProof.TrainSourceRoots,
            StringComparer.Ordinal));
        Assert.True(expected.SplitProof.EvalSourceRoots.SequenceEqual(
            actual.SplitProof.EvalSourceRoots,
            StringComparer.Ordinal));
        Assert.True(expected.SplitProof.SourceHashes.SequenceEqual(
            actual.SplitProof.SourceHashes,
            StringComparer.Ordinal));
        Assert.Equal(
            expected.SplitProof.CrossSplitNearDuplicateCount,
            actual.SplitProof.CrossSplitNearDuplicateCount);
        Assert.Equal(
            expected.SplitProof.SimhashOrMinhash,
            actual.SplitProof.SimhashOrMinhash,
            StringComparer.Ordinal);
        Assert.Equal(expected.SplitProof.NearDuplicateThreshold, actual.SplitProof.NearDuplicateThreshold);
    }

    private static void AssertReadOnly(IReadOnlyList<string> values)
        => Assert.Throws<NotSupportedException>(() => ((IList<string>)values).Add("forbidden"));

    private static void AssertFailure(
        OperationResult<DynamicGraphTargetAccuracyMeasurement> result,
        string expectedCode,
        string expectedMessage)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(expectedCode, result.ErrorCode);
        Assert.Equal(expectedMessage, result.ErrorMessage);
    }

    private static void AssertAccuracyEquivalent(
        DynamicGraphAccuracyMeasurement expected,
        DynamicGraphAccuracyMeasurement actual)
    {
        Assert.Equal(expected.EvaluationId, actual.EvaluationId);
        Assert.Equal(expected.RowsEvaluated, actual.RowsEvaluated);
        Assert.Equal(expected.Accuracy, actual.Accuracy);
        Assert.Equal(expected.HeldOutRewardMargin, actual.HeldOutRewardMargin);
        Assert.Equal(expected.MeetsQ30NumericalGate, actual.MeetsQ30NumericalGate);
        Assert.Equal(expected.SplitProof.TrainSourceRoots, actual.SplitProof.TrainSourceRoots);
        Assert.Equal(expected.SplitProof.EvalSourceRoots, actual.SplitProof.EvalSourceRoots);
        Assert.Equal(expected.SplitProof.SourceHashes, actual.SplitProof.SourceHashes);
        Assert.Equal(expected.SplitProof.CrossSplitNearDuplicateCount, actual.SplitProof.CrossSplitNearDuplicateCount);
        Assert.Equal(expected.SplitProof.SimhashOrMinhash, actual.SplitProof.SimhashOrMinhash);
        Assert.Equal(expected.SplitProof.NearDuplicateThreshold, actual.SplitProof.NearDuplicateThreshold);
    }

    private static void AssertRunSpeedFailure(
        OperationResult<DynamicGraphTargetRunSpeedMeasurement> result,
        string expectedCode,
        string expectedMessage)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(expectedCode, result.ErrorCode);
        Assert.Equal(expectedMessage, result.ErrorMessage);
    }

    private static void AssertRunSpeedEquivalent(
        DynamicGraphRunSpeedMeasurement expected,
        DynamicGraphRunSpeedMeasurement actual)
    {
        Assert.Equal(expected.EvaluatedRowCount, actual.EvaluatedRowCount);
        Assert.Equal(expected.CandidateSampleCount, actual.CandidateSampleCount);
        Assert.Equal(expected.BatchSampleCount, actual.BatchSampleCount);
        Assert.Equal(expected.RunSpeed.InferenceElapsedMs, actual.RunSpeed.InferenceElapsedMs);
        Assert.Equal(expected.RunSpeed.ThroughputRowsPerSec, actual.RunSpeed.ThroughputRowsPerSec);
        Assert.Equal(expected.RunSpeed.RoutingElapsedMs, actual.RunSpeed.RoutingElapsedMs);
        Assert.Equal(expected.RunSpeed.PerCandidateElapsedMs, actual.RunSpeed.PerCandidateElapsedMs);
        Assert.Equal(expected.RunSpeed.PerBatchElapsedMs, actual.RunSpeed.PerBatchElapsedMs);
    }

    private static void AssertTrainingSpeedFailure(
        OperationResult<DynamicGraphTargetTrainingSpeedMeasurement> result,
        string expectedCode,
        string expectedMessage)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(expectedCode, result.ErrorCode);
        Assert.Equal(expectedMessage, result.ErrorMessage);
    }

    private static void AssertTrainingSpeedEquivalent(
        DynamicGraphTrainingSpeedMeasurement expected,
        DynamicGraphTrainingSpeedMeasurement actual)
    {
        Assert.Equal(expected.BatchId, actual.BatchId);
        Assert.Equal(expected.Speed.CpuMs, actual.Speed.CpuMs);
        Assert.Equal(expected.Speed.PeakRamMb, actual.Speed.PeakRamMb);
        Assert.Equal(expected.Speed.ElapsedMs, actual.Speed.ElapsedMs);
        Assert.Equal(expected.Speed.TrainRows, actual.Speed.TrainRows);
    }

    private static void AssertEdgeIdentityQualityFailure(
        OperationResult<DynamicGraphTargetEdgeIdentityQualityMeasurement> result,
        string expectedCode,
        string expectedMessage)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(expectedCode, result.ErrorCode);
        Assert.Equal(expectedMessage, result.ErrorMessage);
    }

    private static void AssertEdgeAuthorityControlFailure(
        OperationResult<DynamicGraphTargetEdgeAuthorityControlMeasurement> result,
        string expectedCode,
        string expectedMessage)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(expectedCode, result.ErrorCode);
        Assert.Equal(expectedMessage, result.ErrorMessage);
    }

    private static void AssertEdgeQualityEvidenceFailure(
        OperationResult<DynamicGraphTargetEdgeQualityEvidenceMeasurement> result,
        string expectedCode,
        string expectedMessage)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(expectedCode, result.ErrorCode);
        Assert.Equal(expectedMessage, result.ErrorMessage);
    }

    private static void AssertEdgeAuthorityControlEquivalent(
        DynamicGraphEdgeAuthorityControlMeasurement expected,
        DynamicGraphEdgeAuthorityControlMeasurement actual)
    {
        var expectedAuthority = expected.AuthorityEvidence;
        var actualAuthority = actual.AuthorityEvidence;
        Assert.Equal(expectedAuthority.EdgeId, actualAuthority.EdgeId, StringComparer.Ordinal);
        Assert.Equal(
            expectedAuthority.AuthorityUnitId,
            actualAuthority.AuthorityUnitId,
            StringComparer.Ordinal);
        Assert.Equal(
            expectedAuthority.AuthorityUnitLevel,
            actualAuthority.AuthorityUnitLevel,
            StringComparer.Ordinal);
        Assert.Equal(
            expectedAuthority.DeterministicFinalAuthority,
            actualAuthority.DeterministicFinalAuthority);
        Assert.Equal(
            expectedAuthority.BaselineUnitId,
            actualAuthority.BaselineUnitId,
            StringComparer.Ordinal);
        Assert.Equal(expectedAuthority.BaselineRole, actualAuthority.BaselineRole, StringComparer.Ordinal);
        Assert.True(expectedAuthority.ExcludedLeakageSignals.SequenceEqual(
            actualAuthority.ExcludedLeakageSignals,
            StringComparer.Ordinal));
        Assert.Equal(expectedAuthority.SourceProof, actualAuthority.SourceProof, StringComparer.Ordinal);
        Assert.Equal(expectedAuthority.CutoverProof, actualAuthority.CutoverProof, StringComparer.Ordinal);
        Assert.Equal(
            expectedAuthority.AblationProofRef,
            actualAuthority.AblationProofRef,
            StringComparer.Ordinal);
        AssertEdgeOrderQualityEquivalent(expected.EdgeOrderQuality, actual.EdgeOrderQuality);
        AssertEdgeIdentityQualityEquivalent(expected.EdgeIdentityQuality, actual.EdgeIdentityQuality);
    }

    private static void AssertEdgeIdentityQualityEquivalent(
        DynamicGraphEdgeIdentityQualityMeasurement expected,
        DynamicGraphEdgeIdentityQualityMeasurement actual)
    {
        Assert.Equal(expected.EdgeId, actual.EdgeId, StringComparer.Ordinal);
        Assert.Equal(expected.ResolvedAuthorityUnitId, actual.ResolvedAuthorityUnitId, StringComparer.Ordinal);
        Assert.Equal(expected.IdentityControlProbeCount, actual.IdentityControlProbeCount);
        Assert.Equal(expected.IdentityInvariant, actual.IdentityInvariant);
        Assert.True(expected.ProbeIds.SequenceEqual(actual.ProbeIds, StringComparer.Ordinal));
        Assert.True(expected.ControlledSignals.SequenceEqual(
            actual.ControlledSignals,
            StringComparer.Ordinal));
        Assert.True(expected.ControlValues.SequenceEqual(actual.ControlValues, StringComparer.Ordinal));
        Assert.True(expected.ExcludedLeakageSignals.SequenceEqual(
            actual.ExcludedLeakageSignals,
            StringComparer.Ordinal));
    }

    private static void AssertEdgeOrderQualityFailure(
        OperationResult<DynamicGraphTargetEdgeOrderQualityMeasurement> result,
        string expectedCode,
        string expectedMessage)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(expectedCode, result.ErrorCode);
        Assert.Equal(expectedMessage, result.ErrorMessage);
    }

    private static void AssertEdgeOrderQualityEquivalent(
        DynamicGraphEdgeOrderQualityMeasurement expected,
        DynamicGraphEdgeOrderQualityMeasurement actual)
    {
        Assert.Equal(expected.EdgeId, actual.EdgeId, StringComparer.Ordinal);
        Assert.Equal(expected.ResolvedAuthorityUnitId, actual.ResolvedAuthorityUnitId, StringComparer.Ordinal);
        Assert.Equal(expected.PermutationProbeCount, actual.PermutationProbeCount);
        Assert.Equal(expected.PermutationInvariant, actual.PermutationInvariant);
        Assert.True(expected.ProbeIds.SequenceEqual(actual.ProbeIds, StringComparer.Ordinal));
        Assert.True(expected.ExcludedLeakageSignals.SequenceEqual(
            actual.ExcludedLeakageSignals,
            StringComparer.Ordinal));
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
        Assert.Empty(typeof(T).GetFields(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly));

        foreach (var property in typeof(T).GetProperties(
                     BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            Assert.NotNull(property.SetMethod);
            Assert.Contains(
                typeof(IsExternalInit),
                property.SetMethod!.ReturnParameter.GetRequiredCustomModifiers());
        }
    }

    private static void AssertOwnedRecordShape<T>(params (string Name, Type Type)[] expected)
    {
        var constructor = Assert.Single(typeof(T).GetConstructors());
        Assert.Equal(
            expected,
            constructor.GetParameters()
                .Select(parameter => (parameter.Name!, parameter.ParameterType))
                .ToArray());
        Assert.Equal(
            expected,
            typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => (property.Name, property.PropertyType))
                .ToArray());
        Assert.Empty(typeof(T).GetFields(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly));
    }

    private static void SetRecordProperty(object owner, string propertyName, object? value)
    {
        var field = Assert.IsAssignableFrom<FieldInfo>(owner.GetType().GetField(
            $"<{propertyName}>k__BackingField",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly));
        Assert.True(field.IsInitOnly);
        field.SetValue(owner, value);
    }

    private static string[] GetPrivateStringArray(object owner, string fieldName)
    {
        var field = Assert.IsAssignableFrom<FieldInfo>(owner.GetType().GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly));
        Assert.True(field.IsInitOnly);
        return Assert.IsType<string[]>(field.GetValue(owner));
    }

    private static T GetPrivateCarrier<T>(object owner, string fieldName)
    {
        var field = Assert.IsAssignableFrom<FieldInfo>(owner.GetType().GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly));
        Assert.True(field.IsInitOnly);
        return Assert.IsType<T>(field.GetValue(owner));
    }

    private static void AssertMethodShape(
        MethodInfo method,
        string name,
        Type returnType,
        params (string Name, Type Type)[] expectedParameters)
    {
        Assert.Equal(name, method.Name);
        Assert.Equal(returnType, method.ReturnType);
        Assert.Equal(
            expectedParameters,
            method.GetParameters()
                .Select(parameter => (parameter.Name!, parameter.ParameterType))
                .ToArray());
    }

    private sealed class CountingUntrainedHonestyUnit : ILogicalUnit<string, string>
    {
        public int ExecutionCount { get; private set; }

        public Task<OperationResult<LogicalUnitResult<string>>> ExecuteAsync(
            LogicalUnitRequest<string> request,
            CancellationToken cancellationToken = default)
        {
            ExecutionCount++;
            return Task.FromResult(OperationResult<LogicalUnitResult<string>>.Ok(
                new LogicalUnitResult<string>(
                    UntrainedHonestyUnitId,
                    "untrained-honesty-role",
                    "untrained-honesty-domain",
                    "not-ready",
                    0.0d,
                    [],
                    ["untrained-honesty-domain"],
                    [NoTrainedState, NoSemanticClassification],
                    [])));
        }
    }

    private sealed class RecordingReadOnlyList<T>(
        string name,
        IReadOnlyList<T> values,
        ICollection<string> reads) : IReadOnlyList<T>
    {
        public int Count => values.Count;

        public T this[int index] => values[index];

        public IEnumerator<T> GetEnumerator()
        {
            reads.Add(name);
            return values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private sealed class FaultingReadOnlyList<T>(Exception exception) : IReadOnlyList<T>
    {
        public int Count => 1;
        public T this[int index] => throw exception;
        public IEnumerator<T> GetEnumerator() => throw exception;
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private sealed class WrapperException(Exception innerException)
        : Exception("wrapper", innerException);
}
