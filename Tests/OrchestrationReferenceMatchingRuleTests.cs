// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

using AgentInfrastructure;
using ContinuousLearning;
using TrainableRouter.Training;

namespace LlmFromScratch.Tests;

public sealed class OrchestrationReferenceMatchingRuleTests
{
    private static readonly DateTimeOffset CapturedAtUtc =
        new(2026, 9, 8, 12, 0, 0, TimeSpan.Zero);

    private static readonly DateTimeOffset FrozenAtUtc =
        new(2026, 9, 8, 12, 30, 0, TimeSpan.Zero);

    [Fact]
    public void Identity_DeterministicallyBindsDomainSubjectPatternRolesAndExactPolicy()
    {
        var first = Require(OrchestrationReferenceMatchingRule.CreateIdentity());
        var second = Require(OrchestrationReferenceMatchingRule.CreateIdentity());

        Assert.Equal(first.IdentitySha256, second.IdentitySha256);
        Assert.Equal(first.CanonicalRepresentation, second.CanonicalRepresentation);
        Assert.Equal(OrchestrationReferenceMatchingRule.DomainTag, first.DomainTag);
        Assert.Equal(OrchestrationReferenceMatchingRule.RuleId, first.RuleId);
        Assert.Contains(OrchestrationReferenceDecompositionPayload.Schema, first.HumanDescription, StringComparison.Ordinal);
        Assert.Contains(OrchestrationReferenceDecompositionPayload.Schema, first.RuleVersion, StringComparison.Ordinal);
        Assert.Contains(OrchestrationReferenceMatchingRule.SubjectId, first.RuleVersion, StringComparison.Ordinal);
        Assert.Contains(OrchestrationReferenceMatchingRule.PatternId, first.RuleVersion, StringComparison.Ordinal);
        Assert.Contains("policy-exact-bindings-order-dag-forward-deps-phases", first.RuleVersion, StringComparison.Ordinal);
        Assert.DoesNotContain("schema-xiigen.orchestration-reference.v1", first.RuleVersion, StringComparison.Ordinal);
        Assert.Contains("DAG/topologically-forward dependency edge set", first.HumanDescription, StringComparison.Ordinal);
        Assert.Contains("does not use OrchestrationAcceptanceGateEvaluator as authority", first.HumanDescription, StringComparison.Ordinal);
        Assert.Contains("not a paid corpus", first.HumanDescription, StringComparison.Ordinal);
    }

    [Fact]
    public void IsMatch_PositiveExactSemanticsIgnoresNonAuthorityProseDifferences()
    {
        var reference = Payload(
            OrchestrationReferenceMatchingRule.PaidReferenceRole,
            descriptions:
            [
                "Paid reference says to inspect the issue and repository.",
                "Paid reference plans the safe code slice.",
                "Paid reference verifies exact focused evidence."
            ]);
        var candidate = Payload(
            OrchestrationReferenceMatchingRule.StudentCandidateRole,
            descriptions:
            [
                "Candidate prose can use different wording for intake.",
                "Candidate prose can phrase planning differently.",
                "Candidate prose can describe verification differently."
            ]);

        var result = Require(OrchestrationReferenceMatchingRule.EvaluateMatch(candidate, reference));

        Assert.True(result.IsMatch);
        Assert.Equal(OrchestrationReferenceMatchingRule.MatchStatus, result.Status);
        Assert.True(OrchestrationReferenceMatchingRule.IsMatch(candidate, reference));
    }

    [Fact]
    public void IsMatch_RoleSwapAndEnvelopeMismatchesFailClosed()
    {
        var reference = Payload(OrchestrationReferenceMatchingRule.PaidReferenceRole);
        var candidate = Payload(OrchestrationReferenceMatchingRule.StudentCandidateRole);

        Assert.False(OrchestrationReferenceMatchingRule.IsMatch(reference, candidate));
        Assert.Equal(
            OrchestrationReferenceMatchingRule.EnvelopeMismatchStatus,
            Require(OrchestrationReferenceMatchingRule.EvaluateMatch(
                Payload(OrchestrationReferenceMatchingRule.StudentCandidateRole, domainTag: "programming-task-orchestration-lookalike"),
                reference)).Status);
        Assert.Equal(
            OrchestrationReferenceMatchingRule.EnvelopeMismatchStatus,
            Require(OrchestrationReferenceMatchingRule.EvaluateMatch(
                Payload(OrchestrationReferenceMatchingRule.StudentCandidateRole, subjectId: "orchestration-task-decomposition-lookalike"),
                reference)).Status);
        Assert.Equal(
            OrchestrationReferenceMatchingRule.EnvelopeMismatchStatus,
            Require(OrchestrationReferenceMatchingRule.EvaluateMatch(
                Payload(OrchestrationReferenceMatchingRule.StudentCandidateRole, patternId: "keyword-planner"),
                reference)).Status);
    }

    [Fact]
    public void IsMatch_BindingMismatchesFailBeforeStructuralComparison()
    {
        var reference = Payload(OrchestrationReferenceMatchingRule.PaidReferenceRole);

        Assert.Equal(
            OrchestrationReferenceMatchingRule.ReferenceBindingMismatchStatus,
            Require(OrchestrationReferenceMatchingRule.EvaluateMatch(
                Payload(
                    OrchestrationReferenceMatchingRule.StudentCandidateRole,
                    referenceCaseId: "case:other"),
                reference)).Status);
        Assert.Equal(
            OrchestrationReferenceMatchingRule.ReferenceBindingMismatchStatus,
            Require(OrchestrationReferenceMatchingRule.EvaluateMatch(
                Payload(
                    OrchestrationReferenceMatchingRule.StudentCandidateRole,
                    scenarioId: "scenario:other"),
                reference)).Status);
        Assert.Equal(
            OrchestrationReferenceMatchingRule.ReferenceBindingMismatchStatus,
            Require(OrchestrationReferenceMatchingRule.EvaluateMatch(
                Payload(
                    OrchestrationReferenceMatchingRule.StudentCandidateRole,
                    sourceEntityFingerprint: Sha('9')),
                reference)).Status);
    }

    [Fact]
    public void IsMatch_SameStepsWrongOrderMissingAndExtraStepAreDifferentFailures()
    {
        var reference = Payload(OrchestrationReferenceMatchingRule.PaidReferenceRole);

        Assert.Equal(
            OrchestrationReferenceMatchingRule.StepOrderMismatchStatus,
            Require(OrchestrationReferenceMatchingRule.EvaluateMatch(
                Payload(
                    OrchestrationReferenceMatchingRule.StudentCandidateRole,
                    steps:
                    [
                        Step("step:intake", 2, "phase:intake"),
                        Step("step:plan", 1, "phase:plan"),
                        Step("step:verify", 3, "phase:verify")
                    ],
                    edges:
                    [
                        Edge("step:plan", "step:intake"),
                        Edge("step:intake", "step:verify")
                    ]),
                reference)).Status);
        Assert.Equal(
            OrchestrationReferenceMatchingRule.StepCoverageMismatchStatus,
            Require(OrchestrationReferenceMatchingRule.EvaluateMatch(
                Payload(
                    OrchestrationReferenceMatchingRule.StudentCandidateRole,
                    steps:
                    [
                        Step("step:intake", 1, "phase:intake"),
                        Step("step:plan", 2, "phase:plan")
                    ],
                    edges: [Edge("step:intake", "step:plan")]),
                reference)).Status);
        Assert.Equal(
            OrchestrationReferenceMatchingRule.StepCoverageMismatchStatus,
            Require(OrchestrationReferenceMatchingRule.EvaluateMatch(
                Payload(
                    OrchestrationReferenceMatchingRule.StudentCandidateRole,
                    steps:
                    [
                        Step("step:intake", 1, "phase:intake"),
                        Step("step:plan", 2, "phase:plan"),
                        Step("step:verify", 3, "phase:verify"),
                        Step("step:deploy", 4, "phase:verify")
                    ],
                    edges:
                    [
                        Edge("step:intake", "step:plan"),
                        Edge("step:plan", "step:verify"),
                        Edge("step:verify", "step:deploy")
                    ]),
                reference)).Status);
    }

    [Fact]
    public void IsMatch_WrongDependencyPhaseSetAndStepPhaseAssignmentFail()
    {
        var reference = Payload(OrchestrationReferenceMatchingRule.PaidReferenceRole);

        Assert.Equal(
            OrchestrationReferenceMatchingRule.DependencyMismatchStatus,
            Require(OrchestrationReferenceMatchingRule.EvaluateMatch(
                Payload(
                    OrchestrationReferenceMatchingRule.StudentCandidateRole,
                    edges:
                    [
                        Edge("step:intake", "step:verify"),
                        Edge("step:plan", "step:verify")
                    ]),
                reference)).Status);
        Assert.Equal(
            OrchestrationReferenceMatchingRule.PhaseTagSetMismatchStatus,
            Require(OrchestrationReferenceMatchingRule.EvaluateMatch(
                Payload(
                    OrchestrationReferenceMatchingRule.StudentCandidateRole,
                    steps:
                    [
                        Step("step:intake", 1, "phase:intake"),
                        Step("step:plan", 2, "phase:plan"),
                        Step("step:verify", 3, "phase:qa")
                    ]),
                reference)).Status);
        Assert.Equal(
            OrchestrationReferenceMatchingRule.StepPhaseAssignmentMismatchStatus,
            Require(OrchestrationReferenceMatchingRule.EvaluateMatch(
                Payload(
                    OrchestrationReferenceMatchingRule.StudentCandidateRole,
                    steps:
                    [
                        Step("step:intake", 1, "phase:intake"),
                        Step("step:plan", 2, "phase:verify"),
                        Step("step:verify", 3, "phase:plan")
                    ]),
                reference)).Status);
    }

    [Fact]
    public void PayloadCreate_RejectsDuplicateStepDuplicateOrderAndNoncontiguousOrder()
    {
        AssertCreateFails(
            steps:
            [
                Step("step:intake", 1, "phase:intake"),
                Step("step:intake", 2, "phase:plan")
            ],
            edges: [],
            "ORCHESTRATION_REFERENCE_STEP_ID_DUPLICATE");
        AssertCreateFails(
            steps:
            [
                Step("step:intake", 1, "phase:intake"),
                Step("step:plan", 1, "phase:plan")
            ],
            edges: [],
            "ORCHESTRATION_REFERENCE_STEP_ORDER_DUPLICATE");
        AssertCreateFails(
            steps:
            [
                Step("step:intake", 1, "phase:intake"),
                Step("step:plan", 3, "phase:plan")
            ],
            edges: [],
            "ORCHESTRATION_REFERENCE_STEP_ORDER_NONCONTIGUOUS");
    }

    [Fact]
    public void PayloadCreate_RejectsDuplicateMissingEndpointAndSelfDependencyEdges()
    {
        AssertCreateFails(
            steps: DefaultSteps(),
            edges:
            [
                Edge("step:intake", "step:plan"),
                Edge("step:intake", "step:plan")
            ],
            "ORCHESTRATION_REFERENCE_DEPENDENCY_DUPLICATE");
        AssertCreateFails(
            steps: DefaultSteps(),
            edges: [Edge("step:intake", "step:missing")],
            "ORCHESTRATION_REFERENCE_DEPENDENCY_ENDPOINT_UNKNOWN");
        AssertCreateFails(
            steps: DefaultSteps(),
            edges: [Edge("step:intake", "step:intake")],
            "ORCHESTRATION_REFERENCE_DEPENDENCY_SELF_EDGE");
    }

    [Fact]
    public void PayloadCreate_RejectsTwoNodeCycleWithStableCycleCode()
    {
        AssertCreateFails(
            steps:
            [
                Step("step:intake", 1, "phase:intake"),
                Step("step:plan", 2, "phase:plan")
            ],
            edges:
            [
                Edge("step:intake", "step:plan"),
                Edge("step:plan", "step:intake")
            ],
            OrchestrationReferenceDecompositionPayload.DependencyCycleErrorCode);
    }

    [Fact]
    public void PayloadCreate_RejectsLongerCycleWithStableCycleCode()
    {
        AssertCreateFails(
            steps: DefaultSteps(),
            edges:
            [
                Edge("step:intake", "step:plan"),
                Edge("step:plan", "step:verify"),
                Edge("step:verify", "step:intake")
            ],
            OrchestrationReferenceDecompositionPayload.DependencyCycleErrorCode);
    }

    [Fact]
    public void PayloadCreate_RejectsBackwardEdgeWithoutCycleAsOrderInvalid()
    {
        AssertCreateFails(
            steps: DefaultSteps(),
            edges: [Edge("step:plan", "step:intake")],
            OrchestrationReferenceDecompositionPayload.DependencyOrderInvalidErrorCode);
    }

    [Fact]
    public void IsMatch_RawIdenticalInvalidCyclicPayloadFailsClosed()
    {
        var cyclic = RawPayloadUnchecked(
            OrchestrationReferenceMatchingRule.StudentCandidateRole,
            edges:
            [
                Edge("step:intake", "step:plan"),
                Edge("step:plan", "step:intake")
            ]);

        var parse = OrchestrationReferenceDecompositionPayload.Parse(cyclic);

        Assert.False(parse.IsSuccess);
        Assert.Equal(
            OrchestrationReferenceDecompositionPayload.DependencyCycleErrorCode,
            parse.ErrorCode);
        Assert.False(OrchestrationReferenceMatchingRule.IsMatch(cyclic, cyclic));
    }

    [Fact]
    public void Parse_MalformedWrongSchemaExtraFieldsAndHighScorePropertyAreRejected()
    {
        var reference = Payload(OrchestrationReferenceMatchingRule.PaidReferenceRole);
        var candidate = Payload(OrchestrationReferenceMatchingRule.StudentCandidateRole);

        Assert.False(OrchestrationReferenceMatchingRule.IsMatch("not json", reference));
        Assert.False(OrchestrationReferenceMatchingRule.IsMatch(
            candidate.Replace(
                OrchestrationReferenceDecompositionPayload.Schema,
                "xiigen.orchestration-reference-decomposition.v0",
                StringComparison.Ordinal),
            reference));
        Assert.False(OrchestrationReferenceMatchingRule.IsMatch(
            candidate.Insert(candidate.Length - 1, ",\"extra\":true"),
            reference));
        Assert.False(OrchestrationReferenceMatchingRule.IsMatch(
            candidate.Insert(candidate.Length - 1, ",\"self_score\":1.0"),
            reference));
        Assert.False(OrchestrationReferenceMatchingRule.IsMatch(
            candidate.Replace(
                "\"description\":\"Read source.\"",
                "\"description\":\"Read source.\",\"extra_step_field\":true",
                StringComparison.Ordinal),
            reference));
        Assert.False(OrchestrationReferenceMatchingRule.IsMatch(
            candidate.Replace(
                "\"to_step_id\":\"step:plan\"",
                "\"to_step_id\":\"step:plan\",\"extra_edge_field\":true",
                StringComparison.Ordinal),
            reference));
    }

    [Fact]
    public void PayloadCreate_RejectsEmptyUnsafeOversizedInvalidUnicodeAndBadSourceFingerprint()
    {
        AssertCreateFails(
            steps: [],
            edges: [],
            "ORCHESTRATION_REFERENCE_STEPS_EMPTY");
        AssertCreateFails(
            steps: [Step("bad id with spaces", 1, "phase:intake")],
            edges: [],
            "ORCHESTRATION_REFERENCE_STEP_ID_INVALID");
        AssertCreateFails(
            steps: [Step("step:intake", 1, "bad phase")],
            edges: [],
            "ORCHESTRATION_REFERENCE_PHASE_TAG_INVALID");

        var invalidUnicode = OrchestrationReferenceDecompositionPayload.Create(
            OrchestrationReferenceMatchingRule.DomainTag,
            OrchestrationReferenceMatchingRule.SubjectId,
            OrchestrationReferenceMatchingRule.PatternId,
            OrchestrationReferenceMatchingRule.StudentCandidateRole,
            "case:alpha",
            "scenario:alpha",
            Sha('1'),
            [Step("step:intake", 1, "phase:intake", "\ud800")],
            []);
        var badFingerprint = OrchestrationReferenceDecompositionPayload.Create(
            OrchestrationReferenceMatchingRule.DomainTag,
            OrchestrationReferenceMatchingRule.SubjectId,
            OrchestrationReferenceMatchingRule.PatternId,
            OrchestrationReferenceMatchingRule.StudentCandidateRole,
            "case:alpha",
            "scenario:alpha",
            "sha256:NOT-CANONICAL",
            DefaultSteps(),
            DefaultEdges());
        var oversized = OrchestrationReferenceDecompositionPayload.Create(
            OrchestrationReferenceMatchingRule.DomainTag,
            OrchestrationReferenceMatchingRule.SubjectId,
            OrchestrationReferenceMatchingRule.PatternId,
            OrchestrationReferenceMatchingRule.StudentCandidateRole,
            "case:alpha",
            "scenario:alpha",
            Sha('1'),
            [Step("step:intake", 1, "phase:intake", new string('a', 4_097))],
            []);

        Assert.False(invalidUnicode.IsSuccess);
        Assert.Equal("ORCHESTRATION_REFERENCE_STEP_DESCRIPTION_INVALID", invalidUnicode.ErrorCode);
        Assert.False(badFingerprint.IsSuccess);
        Assert.Equal("ORCHESTRATION_REFERENCE_SOURCE_FINGERPRINT_INVALID", badFingerprint.ErrorCode);
        Assert.False(oversized.IsSuccess);
        Assert.Equal("ORCHESTRATION_REFERENCE_STEP_DESCRIPTION_INVALID", oversized.ErrorCode);
    }

    [Fact]
    public void EvaluateCalibration_WrongIdentityAndWrongCorpusSubjectFailBeforeMetrics()
    {
        var reference = Payload(OrchestrationReferenceMatchingRule.PaidReferenceRole);
        var entry = CreateEntry(
            "identity",
            "Break this programming task into executable orchestration steps.",
            reference);
        var wrongIdentity = Require(ReferenceMatchingRuleIdentity.Create(
            OrchestrationReferenceMatchingRule.DomainTag,
            "orchestration-foreign-rule",
            "v1",
            "Wrong identity fixture; no metrics should be produced."));
        var wrongIdentityInput = Require(ReferenceMatchingCalibrationInput.Create(
            CreateCorpus(entry),
            wrongIdentity,
            knownPositivePairs:
            [
                Pair(
                    "positive-wrong-identity",
                    entry.EntryId,
                    Payload(OrchestrationReferenceMatchingRule.StudentCandidateRole),
                    entry.PaidModelReferenceAnswer)
            ],
            knownNegativePairs:
            [
                Pair(
                    "negative-wrong-identity",
                    entry.EntryId,
                    Payload(
                        OrchestrationReferenceMatchingRule.StudentCandidateRole,
                        scenarioId: "scenario:foreign"),
                    entry.PaidModelReferenceAnswer)
            ]));
        var wrongSubjectEntry = CreateEntry(
            "wrong-subject",
            "Break this foreign orchestration task down.",
            reference,
            subjectId: "orchestration-task-decomposition-foreign");
        var wrongSubjectInput = Require(ReferenceMatchingCalibrationInput.Create(
            CreateCorpus(wrongSubjectEntry),
            Require(OrchestrationReferenceMatchingRule.CreateIdentity()),
            knownPositivePairs:
            [
                Pair(
                    "positive-wrong-subject",
                    wrongSubjectEntry.EntryId,
                    Payload(OrchestrationReferenceMatchingRule.StudentCandidateRole),
                    wrongSubjectEntry.PaidModelReferenceAnswer)
            ],
            knownNegativePairs:
            [
                Pair(
                    "negative-wrong-subject",
                    wrongSubjectEntry.EntryId,
                    Payload(
                        OrchestrationReferenceMatchingRule.StudentCandidateRole,
                        referenceCaseId: "case:foreign"),
                    wrongSubjectEntry.PaidModelReferenceAnswer)
            ]));

        var wrongIdentityResult =
            OrchestrationReferenceMatchingRule.EvaluateCalibration(wrongIdentityInput);
        var wrongSubjectResult =
            OrchestrationReferenceMatchingRule.EvaluateCalibration(wrongSubjectInput);

        Assert.False(wrongIdentityResult.IsSuccess);
        Assert.Null(wrongIdentityResult.Value);
        Assert.Equal(
            OrchestrationReferenceMatchingRule.RuleIdentityMismatchErrorCode,
            wrongIdentityResult.ErrorCode);
        Assert.False(wrongSubjectResult.IsSuccess);
        Assert.Null(wrongSubjectResult.Value);
        Assert.Equal(
            OrchestrationReferenceMatchingRule.CorpusSubjectMismatchErrorCode,
            wrongSubjectResult.ErrorCode);
    }

    [Fact]
    public void EvaluateCalibration_GenericEvaluatorReportsZeroFalseMissAndFalseHit()
    {
        var reference = Payload(
            OrchestrationReferenceMatchingRule.PaidReferenceRole,
            descriptions:
            [
                "Paid prose one.",
                "Paid prose two.",
                "Paid prose three."
            ]);
        var entry = CreateEntry(
            "calibration",
            "Break this task into orchestration steps.",
            reference);
        var input = CreateCalibrationInput(
            CreateCorpus(entry),
            positives:
            [
                Pair(
                    "positive-exact-structure-different-prose",
                    entry.EntryId,
                    Payload(
                        OrchestrationReferenceMatchingRule.StudentCandidateRole,
                        descriptions:
                        [
                            "Candidate prose one.",
                            "Candidate prose two.",
                            "Candidate prose three."
                        ]),
                    entry.PaidModelReferenceAnswer)
            ],
            negatives:
            [
                Pair(
                    "negative-wrong-dependency",
                    entry.EntryId,
                    Payload(
                        OrchestrationReferenceMatchingRule.StudentCandidateRole,
                        edges:
                        [
                            Edge("step:intake", "step:verify"),
                            Edge("step:plan", "step:verify")
                        ]),
                    entry.PaidModelReferenceAnswer)
            ]);

        var result = Require(OrchestrationReferenceMatchingRule.EvaluateCalibration(input));

        Assert.Equal(1, result.KnownPositiveCount);
        Assert.Equal(0, result.FalseMissCount);
        Assert.Equal(0D, result.FalseMissRate);
        Assert.Equal(1, result.KnownNegativeCount);
        Assert.Equal(0, result.FalseHitCount);
        Assert.Equal(0D, result.FalseHitRate);
    }

    private static ReferenceMatchingCalibrationInput CreateCalibrationInput(
        FrozenReferenceCorpus corpus,
        IReadOnlyList<ReferenceMatchingCalibrationPair> positives,
        IReadOnlyList<ReferenceMatchingCalibrationPair> negatives)
        => Require(ReferenceMatchingCalibrationInput.Create(
            corpus,
            Require(OrchestrationReferenceMatchingRule.CreateIdentity()),
            positives,
            negatives));

    private static FrozenReferenceCorpus CreateCorpus(
        params FrozenReferenceEntry[] entries)
        => Require(FrozenReferenceCorpus.Build(
            "orchestration-reference-fixture",
            "v2026-09-08-unit",
            FrozenAtUtc,
            entries));

    private static FrozenReferenceEntry CreateEntry(
        string seed,
        string ownerInput,
        string referenceAnswer,
        string subjectId = OrchestrationReferenceMatchingRule.SubjectId)
    {
        // Unit-only fake paid provenance: this fixture exercises the
        // frozen-reference matcher contract only. It is not a production paid
        // corpus, DPO row, model call, checkpoint, weight, or training artifact.
        var referenceHash =
            FrozenReferenceEntry.ComputeNormalizedTextSha256(referenceAnswer);
        var provenance = Require(PaidReferenceProvenance.Create(
            "unit-fake-paid-provider",
            "unit-fake-paid-model",
            "2026-09-08",
            "unit-request-" + seed,
            CapturedAtUtc,
            referenceHash));

        return Require(FrozenReferenceEntry.Create(
            ownerInput,
            referenceAnswer,
            provenance,
            OrchestrationReferenceMatchingRule.DomainTag,
            subjectId,
            FrozenReferencePartition.HeldOut,
            FrozenReferenceEntry.ComputeNormalizedTextSha256(
                "unit-source:" + seed)));
    }

    private static ReferenceMatchingCalibrationPair Pair(
        string pairId,
        string referenceEntryId,
        string candidate,
        string reference)
        => Require(ReferenceMatchingCalibrationPair.Create(
            pairId,
            referenceEntryId,
            candidate,
            reference));

    private static string Payload(
        string role,
        string? domainTag = null,
        string? subjectId = null,
        string? patternId = null,
        string? referenceCaseId = null,
        string? scenarioId = null,
        string? sourceEntityFingerprint = null,
        IReadOnlyList<OrchestrationReferenceStep?>? steps = null,
        IReadOnlyList<OrchestrationReferenceDependencyEdge?>? edges = null,
        IReadOnlyList<string>? descriptions = null)
    {
        var selectedSteps = steps ?? DefaultSteps(descriptions);
        return Require(OrchestrationReferenceDecompositionPayload.Create(
            domainTag ?? OrchestrationReferenceMatchingRule.DomainTag,
            subjectId ?? OrchestrationReferenceMatchingRule.SubjectId,
            patternId ?? OrchestrationReferenceMatchingRule.PatternId,
            role,
            referenceCaseId ?? "case:alpha",
            scenarioId ?? "scenario:alpha",
            sourceEntityFingerprint ?? Sha('1'),
            selectedSteps,
            edges ?? DefaultEdges())).ToCanonicalString();
    }

    private static string RawPayloadUnchecked(
        string role,
        IReadOnlyList<OrchestrationReferenceDependencyEdge> edges)
    {
        var edgeJson = string.Join(
            ",",
            edges.Select(static edge =>
                $$"""{"from_step_id":"{{edge.FromStepId}}","to_step_id":"{{edge.ToStepId}}"}"""));

        return
            $$"""{"schema":"{{OrchestrationReferenceDecompositionPayload.Schema}}","domain_tag":"{{OrchestrationReferenceMatchingRule.DomainTag}}","subject_id":"{{OrchestrationReferenceMatchingRule.SubjectId}}","pattern_id":"{{OrchestrationReferenceMatchingRule.PatternId}}","role":"{{role}}","reference_case_id":"case:alpha","scenario_id":"scenario:alpha","source_entity_fingerprint":"{{Sha('1')}}","steps":[{"step_id":"step:intake","order":1,"phase_tag":"phase:intake","description":"Read source."},{"step_id":"step:plan","order":2,"phase_tag":"phase:plan","description":"Plan implementation."}],"dependency_edges":[{{edgeJson}}]}""";
    }

    private static void AssertCreateFails(
        IReadOnlyList<OrchestrationReferenceStep?> steps,
        IReadOnlyList<OrchestrationReferenceDependencyEdge?> edges,
        string expectedErrorCode)
    {
        var result = OrchestrationReferenceDecompositionPayload.Create(
            OrchestrationReferenceMatchingRule.DomainTag,
            OrchestrationReferenceMatchingRule.SubjectId,
            OrchestrationReferenceMatchingRule.PatternId,
            OrchestrationReferenceMatchingRule.StudentCandidateRole,
            "case:alpha",
            "scenario:alpha",
            Sha('1'),
            steps,
            edges);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(expectedErrorCode, result.ErrorCode);
    }

    private static OrchestrationReferenceStep Step(
        string stepId,
        int order,
        string phaseTag,
        string? description = null)
        => new(stepId, order, phaseTag, description ?? DescriptionFor(stepId));

    private static OrchestrationReferenceDependencyEdge Edge(
        string fromStepId,
        string toStepId)
        => new(fromStepId, toStepId);

    private static IReadOnlyList<OrchestrationReferenceStep?> DefaultSteps(
        IReadOnlyList<string>? descriptions = null)
        =>
        [
            Step("step:intake", 1, "phase:intake", descriptions?[0] ?? "Read source."),
            Step("step:plan", 2, "phase:plan", descriptions?[1] ?? "Plan implementation."),
            Step("step:verify", 3, "phase:verify", descriptions?[2] ?? "Verify focused evidence.")
        ];

    private static IReadOnlyList<OrchestrationReferenceDependencyEdge?> DefaultEdges()
        =>
        [
            Edge("step:intake", "step:plan"),
            Edge("step:plan", "step:verify")
        ];

    private static string DescriptionFor(string stepId)
        => "Description for " + stepId + ".";

    private static string Sha(char hex)
        => "sha256:" + new string(hex, 64);

    private static T Require<T>(OperationResult<T> result)
    {
        Assert.True(
            result.IsSuccess,
            $"Expected success but got {result.ErrorCode}: {result.ErrorMessage}");
        Assert.NotNull(result.Value);
        return result.Value!;
    }
}
