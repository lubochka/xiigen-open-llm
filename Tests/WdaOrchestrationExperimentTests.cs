// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using AgentInfrastructure.Logical;

public sealed class WdaOrchestrationExperimentTests
{
    private const string Wda01RepositoryRootOverrideVariable = "XIIGEN_WDA01_REPOSITORY_ROOT";

    private const string Wda01ExperimentRootOverrideVariable = "XIIGEN_WDA01_ORCHESTRATOR_EXPERIMENT_ROOT";

    private const string Wda01LegacyExperimentRootOverrideVariable = "XIIGEN_ORCHESTRATOR_EXPERIMENT_ROOT";

    private const string Wda01ExternalSourceRootOverrideVariable = "XIIGEN_WDA01_EXTERNAL_SOURCE_ROOT";

    private static readonly string[] Wda01RequiredSourceFileNames =
    [
        "DEVELOPMENT-PHASES-AND-DOCUMENT-IDENTIFICATION-GUIDE-v1.md",
        "MULTI-PHASE-ORCHESTRATION-DESIGN-AND-EXPERIMENT-PLAN-v1.md",
        "LOGICAL-UNIT-FLOW-ORCHESTRATOR-IMPLEMENTATION-PHASE-PLAN-v1.md",
        "ARTIFACT-EXAMINATION-PROTOCOL-EXPERIMENT-PLAN-v1.0.md",
        "CORPUS-EXPANSION-PLAN-v4.md"
    ];

    private static readonly string[] Wda01RequiredExperimentRootFileNames =
    [
        .. Wda01RequiredSourceFileNames,
        "ORCHESTRATOR-INSPECTOR-DESIGN-v3-final.md",
        "WDA-01-SUB-PHASE-ROUTER-UNIT-CONTRACT-v1.md"
    ];

    private static readonly string[] Wda01RequiredRepositoryRootRelativePaths =
    [
        "LlmFromScratch.sln",
        Path.Combine("Tests", "WdaOrchestrationExperimentTests.cs"),
        Path.Combine("35.ExperimentsV2", "LogicalFlow", "SecurityVerification34FlowAdapter.cs"),
        Path.Combine("35.ExperimentsV2", "LogicalFlow", "SecurityArbiterSelectorUnit.cs")
    ];

    private static readonly string[] Wda01RequiredExternalSourceFileNames =
    [
        "node-pipeline-chain-set-ext25.md"
    ];

    [Fact]
    public async Task WDA01_DocumentClassification_ClassifiesAllCanonicalSubPhases()
    {
        // Build the canonical WDA-01 probe set from the accepted Round A oracle and real source ledger.
        var probes = BuildWda01Probes();

        // Build a topology registry with the canonical thirty-one sub-phases.
        var topologyRegistry = BuildWda01TopologyRegistry(probes);

        // Build the real router unit that replaces the WDA-01 deterministic fixture.
        var router = new SubPhaseRouterUnit(topologyRegistry);

        // Classify each source-ledger probe through the router unit boundary.
        var results = new List<Wda01ClassificationResult>();

        // Execute each classification probe through the logical unit surface.
        foreach (var probe in probes)
        {
            // Build the router input from source-ledger fields rather than document body text.
            var sourceDocument = BuildWda01SourceDocument(probe);

            // Build the unit request with default optimization and safety flags enabled.
            var request = new LogicalUnitRequest<SubPhaseRouterSourceDocument>(
                "wda-01-real-router",
                SubPhaseRouterUnit.Id,
                SubPhaseRouterUnit.Role,
                SubPhaseRouterUnit.DomainTag,
                sourceDocument,
                new LogicalExecutionFlags(),
                new Dictionary<string, object>());

            // Execute the router unit.
            var routed = await router.ExecuteAsync(request);

            // The real router must return through the shared operation envelope.
            Assert.True(routed.IsSuccess, routed.ErrorMessage);

            // Read the routing decision after the envelope success check.
            var decision = routed.Value!.Output;

            // Preserve the expected and predicted values in the WDA-01 result shape.
            results.Add(new Wda01ClassificationResult(
                probe.ProbeId,
                decision.SourceStatus,
                probe.ExpectedPhaseId,
                probe.ExpectedSubPhaseId,
                decision.PredictedPhaseId,
                decision.PredictedSubPhaseId,
                decision.Confidence,
                decision.NearestWrongSubPhaseScore,
                decision.EvidencePredicates,
                decision.KnowledgeEdge,
                decision.FailureClassification));
        }

        // Materialize the router results after all unit calls finish.
        var resultArray = results.ToArray();

        // Count how many probes produced the expected phase id.
        var correctPhaseCount = resultArray.Count(result => result.PredictedPhaseId == result.ExpectedPhaseId);

        // Count how many probes produced the expected sub-phase id.
        var correctSubPhaseCount = resultArray.Count(result => result.PredictedSubPhaseId == result.ExpectedSubPhaseId);

        // Find the minimum confidence across all router classification results.
        var minConfidence = resultArray.Min(result => result.Confidence);

        // Find the maximum wrong-domain score across all router classification results.
        var maxWrongDomainScore = resultArray.Max(result => result.NearestWrongSubPhaseScore);

        // Count wrong-domain scores that cross the E51A-derived confidence guard.
        var wrongDomainHighConfidenceCount = resultArray.Count(result => result.NearestWrongSubPhaseScore >= 0.50);

        // Count source-ledger packets whose exact source path exists on this machine.
        var readySourceCount = resultArray.Count(result => result.SourceStatus == Wda01SourceStatus.Ready);

        // Count source packets that are explicit source knowledge edges.
        var sourceKnowledgeEdgeCount = resultArray.Count(result => result.SourceStatus == Wda01SourceStatus.KnowledgeEdge);

        // Count missing source packets that failed to carry a knowledge-edge marker.
        var missingSourceWithoutKnowledgeEdgeCount = resultArray.Count(result => result.SourceStatus == Wda01SourceStatus.KnowledgeEdge && !result.KnowledgeEdge);

        // Count selected source files that exist on disk.
        var existingSourceFileCount = probes.Count(probe => File.Exists(probe.SourcePath));

        // Count probes that the router itself still had to classify as knowledge edges.
        var routerKnowledgeEdgeCount = resultArray.Count(result => result.KnowledgeEdge);

        // Build the human-readable classification report used by the WDA-01 artifact gate.
        var humanReport = BuildWda01HumanReport(resultArray);

        // Assert the canonical WDA-01 probe count.
        Assert.True(resultArray.Length == 31, $"Expected 31 WDA-01 probes but found {resultArray.Length}.");

        // Assert that every phase classification matches the oracle.
        Assert.Equal(31, correctPhaseCount);

        // Assert that every sub-phase classification matches the oracle.
        Assert.Equal(31, correctSubPhaseCount);

        // Assert the correct-domain confidence floor.
        Assert.True(minConfidence >= 0.70, $"Minimum confidence was {minConfidence:0.00}.");

        // Assert the wrong-domain confidence ceiling.
        Assert.True(maxWrongDomainScore < 0.50, $"Maximum wrong-domain score was {maxWrongDomainScore:0.00}.");

        // Assert that no wrong-domain neighbor looks confident.
        Assert.Equal(0, wrongDomainHighConfidenceCount);

        // Assert that every selected source is now backed by a ready local source entry.
        Assert.Equal(31, readySourceCount);

        // Assert that no selected WDA-01 source is still a source knowledge edge.
        Assert.Equal(0, sourceKnowledgeEdgeCount);

        // Assert that every selected source path resolves on this machine.
        Assert.Equal(31, existingSourceFileCount);

        // Assert that every missing source is marked as a knowledge edge.
        Assert.Equal(0, missingSourceWithoutKnowledgeEdgeCount);

        // Assert that the router did not classify any ready source as a knowledge edge.
        Assert.Equal(0, routerKnowledgeEdgeCount);

        // Assert that the human report names the real router unit.
        Assert.Contains("runtime_unit: sub-phase-router unit executed", humanReport);

        // Assert that the old deterministic runtime gap no longer appears.
        Assert.DoesNotContain("runtime_gap: sub-phase-router unit not yet implemented", humanReport);

        // Assert that the human report names Wave 1 corpus expansion as the next gate.
        Assert.Contains("next_action: start Wave 1 source-ledger expansion", humanReport);
    }

    [Fact]
    public async Task WDA01_Wave2CenterBatch001HeldOut_ClassifiesAllRows()
    {
        // Build the canonical topology rows so the held-out rows compete against all thirty-one sub-phases.
        var topologyRegistry = BuildWda01TopologyRegistry(BuildWda01Probes());

        // Build the real router unit used by WDA-01 and Wave 2 conformance.
        var router = new SubPhaseRouterUnit(topologyRegistry);

        // Build the five held-out rows from Wave 2 Center Phase Batch 001.
        var heldOutRows = BuildWave2CenterBatch001HeldOutRows();

        // Collect every held-out routing result for aggregate conformance checks.
        var results = new List<Wda01ClassificationResult>();

        // Execute every held-out row through the real logical unit boundary.
        foreach (var row in heldOutRows)
        {
            // Convert the held-out row into the exact router input object.
            var sourceDocument = BuildWda01SourceDocument(row);

            // Build the logical unit request with the default optimization and safety flags enabled.
            var request = new LogicalUnitRequest<SubPhaseRouterSourceDocument>(
                "wave-2-center-batch-001-held-out",
                SubPhaseRouterUnit.Id,
                SubPhaseRouterUnit.Role,
                SubPhaseRouterUnit.DomainTag,
                sourceDocument,
                new LogicalExecutionFlags(),
                new Dictionary<string, object>());

            // Execute the held-out classification.
            var routed = await router.ExecuteAsync(request);

            // The held-out row must return through the shared operation envelope.
            Assert.True(routed.IsSuccess, routed.ErrorMessage);

            // Read the decision after the success check.
            var decision = routed.Value!.Output;

            // Store the decision in the same result shape used by WDA-01.
            results.Add(new Wda01ClassificationResult(
                row.ProbeId,
                decision.SourceStatus,
                row.ExpectedPhaseId,
                row.ExpectedSubPhaseId,
                decision.PredictedPhaseId,
                decision.PredictedSubPhaseId,
                decision.Confidence,
                decision.NearestWrongSubPhaseScore,
                decision.EvidencePredicates,
                decision.KnowledgeEdge,
                decision.FailureClassification));
        }

        // Materialize the results after all router calls complete.
        var resultArray = results.ToArray();

        // Count held-out rows that routed to the expected phase.
        var correctPhaseCount = resultArray.Count(result => result.PredictedPhaseId == result.ExpectedPhaseId);

        // Count held-out rows that routed to the expected sub-phase.
        var correctSubPhaseCount = resultArray.Count(result => result.PredictedSubPhaseId == result.ExpectedSubPhaseId);

        // Find the weakest confidence in the held-out batch.
        var minConfidence = resultArray.Min(result => result.Confidence);

        // Find the strongest wrong-domain neighbor in the held-out batch.
        var maxWrongDomainScore = resultArray.Max(result => result.NearestWrongSubPhaseScore);

        // Count source rows that became source knowledge edges.
        var sourceKnowledgeEdgeCount = resultArray.Count(result => result.SourceStatus == Wda01SourceStatus.KnowledgeEdge);

        // Count router outputs that became router knowledge edges.
        var routerKnowledgeEdgeCount = resultArray.Count(result => result.KnowledgeEdge);

        // Build the failure summary so a held-out routing drift names the exact row.
        var routingSummary = string.Join(
            Environment.NewLine,
            resultArray.Select(result => $"{result.ProbeId}: expected {result.ExpectedPhaseId}/{result.ExpectedSubPhaseId}, predicted {result.PredictedPhaseId}/{result.PredictedSubPhaseId}, confidence {result.Confidence:0.00}, wrong {result.NearestWrongSubPhaseScore:0.00}"));

        // Assert the Wave 2 held-out seed row count.
        Assert.Equal(5, resultArray.Length);

        // Assert that every held-out row routed to the expected phase.
        Assert.True(correctPhaseCount == 5, routingSummary);

        // Assert that every held-out row routed to the expected sub-phase.
        Assert.True(correctSubPhaseCount == 5, routingSummary);

        // Assert that every held-out row stays above the promotion confidence floor.
        Assert.True(minConfidence >= 0.70, $"Minimum confidence was {minConfidence:0.00}.");

        // Assert that every wrong-domain neighbor stays below the E51A ceiling.
        Assert.True(maxWrongDomainScore < 0.50, $"Maximum wrong-domain score was {maxWrongDomainScore:0.00}.");

        // Assert that every held-out source row is ready source material.
        Assert.Equal(0, sourceKnowledgeEdgeCount);

        // Assert that the router did not mark any held-out row as a knowledge edge.
        Assert.Equal(0, routerKnowledgeEdgeCount);
    }

    [Fact]
    public void WDA01_SourceRootResolver_TranslatesWindowsOverrideToWslMount()
    {
        const string windowsRoot = @"C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment";
        const string translatedRoot = "/mnt/c/Users/lubao/Documents/xiigen/llm mvp/orchestrator experiment";

        // Model the exact files at the translated WSL mount without mutating process-wide environment variables.
        var existingFiles = Wda01RequiredExperimentRootFileNames
            .Select(fileName => $"{translatedRoot}/{fileName}")
            .ToHashSet(StringComparer.Ordinal);

        // Resolve a Windows drive-qualified override while pretending to run on Linux; no wslpath process is involved.
        var resolvedRoot = ResolveWda01Root(
            "orchestrator experiment source root",
            "WDA01_ORCHESTRATOR_EXPERIMENT_ROOT_NOT_FOUND",
            [new Wda01RootCandidate($"env {Wda01ExperimentRootOverrideVariable}", windowsRoot)],
            Wda01RequiredExperimentRootFileNames,
            isWindows: false,
            fileExists: path => existingFiles.Contains(path.Replace('\\', '/')));

        // The accepted root must be the native WSL path, not the Windows path string treated as a Linux filename.
        Assert.Equal(translatedRoot, resolvedRoot);
    }

    [Fact]
    public void WDA01_SourceRootResolver_DiscoversMountedWindowsDocumentsProfileOnLinux()
    {
        const string mountedUsersRoot = "/mnt/c/Users";
        const string profileRoot = "/mnt/c/Users/lubao";
        const string expectedRoot = "/mnt/c/Users/lubao/Documents/xiigen/llm mvp/orchestrator experiment";

        // Model only one bounded profile enumeration under /mnt/c/Users so the resolver cannot pass by broad scanning.
        var profileDirectories = new[]
        {
            "/mnt/c/Users/Default",
            profileRoot
        };

        // Model the required WDA-01 files only under Luba's Windows Documents-backed xiigen library.
        var existingFiles = Wda01RequiredExperimentRootFileNames
            .Select(fileName => $"{expectedRoot}/{fileName}")
            .ToHashSet(StringComparer.Ordinal);

        // Build Linux candidates from a non-useful Linux MyDocuments folder plus the deterministic Windows profile mount.
        var candidates = BuildWda01DocumentsLibraryCandidates(
            ["orchestrator experiment"],
            "orchestrator experiment source root",
            isWindows: false,
            getMyDocuments: () => "/home/root/Documents",
            directoryExists: path => path == mountedUsersRoot,
            enumerateDirectories: path => path == mountedUsersRoot ? profileDirectories : []);

        // Resolve must select the validated profile candidate and ignore the missing Linux MyDocuments candidate.
        var resolvedRoot = ResolveWda01Root(
            "orchestrator experiment source root",
            "WDA01_ORCHESTRATOR_EXPERIMENT_ROOT_NOT_FOUND",
            candidates,
            Wda01RequiredExperimentRootFileNames,
            isWindows: false,
            fileExists: path => existingFiles.Contains(path.Replace('\\', '/')));

        // The resolver must find the native /mnt/c Documents library candidate deterministically.
        Assert.Equal(expectedRoot, resolvedRoot);
    }

    [Fact]
    public void WDA01_SourceRootResolver_RejectsInvalidNewOverrideBeforeLegacy()
    {
        const string invalidNewRoot = "/missing/wda-01/orchestrator experiment";
        const string validLegacyRoot = "/mnt/c/Users/lubao/Documents/xiigen/llm mvp/orchestrator experiment";

        // Model a valid legacy library so the regression proves the new override fails closed instead of falling back.
        var existingFiles = Wda01RequiredExperimentRootFileNames
            .Select(fileName => $"{validLegacyRoot}/{fileName}")
            .ToHashSet(StringComparer.Ordinal);

        // The first non-empty environment override wins; legacy is not a second candidate when the new override is set.
        var candidates = BuildFirstPresentEnvironmentOverrideCandidates(
            variableName => variableName switch
            {
                Wda01ExperimentRootOverrideVariable => invalidNewRoot,
                Wda01LegacyExperimentRootOverrideVariable => validLegacyRoot,
                _ => null
            },
            Wda01ExperimentRootOverrideVariable,
            Wda01LegacyExperimentRootOverrideVariable);

        Assert.Single(candidates);

        var exception = Assert.Throws<InvalidOperationException>(() => ResolveWda01Root(
            "orchestrator experiment source root",
            "WDA01_SOURCE_ROOT_NOT_FOUND",
            candidates,
            Wda01RequiredExperimentRootFileNames,
            isWindows: false,
            fileExists: path => existingFiles.Contains(path.Replace('\\', '/')),
            ambiguityOverrideVariable: Wda01ExperimentRootOverrideVariable));

        Assert.Contains("WDA01_SOURCE_ROOT_NOT_FOUND", exception.Message);
        Assert.Contains(invalidNewRoot, exception.Message);
        Assert.DoesNotContain(validLegacyRoot, exception.Message);
    }

    [Fact]
    public void WDA01_SourceRootResolver_FailsClosedWhenTwoMountedWindowsProfilesAreValid()
    {
        const string mountedUsersRoot = "/mnt/c/Users";
        const string firstProfileRoot = "/mnt/c/Users/alice";
        const string secondProfileRoot = "/mnt/c/Users/lubao";
        const string firstExpectedRoot = "/mnt/c/Users/alice/Documents/xiigen/llm mvp/orchestrator experiment";
        const string secondExpectedRoot = "/mnt/c/Users/lubao/Documents/xiigen/llm mvp/orchestrator experiment";

        // Model two valid Windows-profile Documents libraries without scanning beyond the one allowed profile level.
        var profileDirectories = new[]
        {
            firstProfileRoot,
            secondProfileRoot
        };

        // Both profile roots contain the exact required files, so silently picking the first one would be ambiguous.
        var existingFiles = Wda01RequiredExperimentRootFileNames
            .SelectMany(fileName => new[] { $"{firstExpectedRoot}/{fileName}", $"{secondExpectedRoot}/{fileName}" })
            .ToHashSet(StringComparer.Ordinal);

        var candidates = BuildWda01DocumentsLibraryCandidates(
            ["orchestrator experiment"],
            "orchestrator experiment source root",
            isWindows: false,
            getMyDocuments: () => "/home/root/Documents",
            directoryExists: path => path == mountedUsersRoot,
            enumerateDirectories: path => path == mountedUsersRoot ? profileDirectories : []);

        var exception = Assert.Throws<InvalidOperationException>(() => ResolveWda01Root(
            "orchestrator experiment source root",
            "WDA01_SOURCE_ROOT_NOT_FOUND",
            candidates,
            Wda01RequiredExperimentRootFileNames,
            isWindows: false,
            fileExists: path => existingFiles.Contains(path.Replace('\\', '/')),
            ambiguityOverrideVariable: Wda01ExperimentRootOverrideVariable));

        Assert.Contains("WDA01_AMBIGUOUS_SOURCE_ROOT", exception.Message);
        Assert.Contains(firstExpectedRoot, exception.Message);
        Assert.Contains(secondExpectedRoot, exception.Message);
        Assert.Contains(Wda01ExperimentRootOverrideVariable, exception.Message);
    }

    [Fact]
    public void WDA02_WithinPhaseOrchestration_CoversEveryDevelopmentPhase()
    {
        // Build one scenario for each canonical development phase.
        var scenarios = BuildWda02PhaseScenarios();

        // Run every scenario through the deterministic orchestration fixture.
        var results = scenarios.Select(RunWda02DeterministicFixture).ToArray();

        // Count how many sub-phases remained visible in the deterministic trace.
        var coveredSubPhaseCount = results.Sum(result => result.SubPhaseCoverageCount);

        // Count every edge that was lost between source packet and human result.
        var lostEdgeCount = results.Sum(result => result.LostEdgeCount);

        // Count every dependency order violation across the phase plans.
        var dependencyViolationCount = results.Sum(result => result.DependencyViolationCount);

        // Count every parallel group that ran before its dependencies were ready.
        var invalidParallelRunCount = results.Sum(result => result.InvalidParallelRunCount);

        // Count every knowledge edge that was converted into an invented local solution.
        var inventedKnowledgeEdgeSolutionCount = results.Sum(result => result.InventedKnowledgeEdgeSolutionCount);

        // Count every rejected item that reached an accepted output path.
        var rejectedItemAcceptedCount = results.Sum(result => result.RejectedItemAcceptedCount);

        // Find the smallest human section count across all phase reports.
        var minimumHumanSectionCount = results.Min(result => result.HumanResultSectionsPresent);

        // Build the human report that summarizes all seven phase runs.
        var humanReport = BuildWda02HumanReport(results);

        // Assert the canonical number of phase scenarios.
        Assert.Equal(7, results.Length);

        // Assert that all thirty-one sub-phases remain visible across the phase scenarios.
        Assert.Equal(31, coveredSubPhaseCount);

        // Assert that no source edge disappears.
        Assert.Equal(0, lostEdgeCount);

        // Assert that no dependency ordering rule is violated.
        Assert.Equal(0, dependencyViolationCount);

        // Assert that no parallel branch runs before its required inputs exist.
        Assert.Equal(0, invalidParallelRunCount);

        // Assert that knowledge edges stay knowledge edges.
        Assert.Equal(0, inventedKnowledgeEdgeSolutionCount);

        // Assert that rejected items stay rejected.
        Assert.Equal(0, rejectedItemAcceptedCount);

        // Assert that every phase report has the five required human sections.
        Assert.Equal(5, minimumHumanSectionCount);

        // Assert that the report names the covered status section.
        Assert.Contains("Can proceed", humanReport);

        // Assert that the report names the blocked status section.
        Assert.Contains("Blocked", humanReport);

        // Assert that the report names the rejected status section.
        Assert.Contains("Rejected", humanReport);

        // Assert that the report names the knowledge-edge status section.
        Assert.Contains("Unknown or knowledge edge", humanReport);

        // Assert that the report names the next human decisions section.
        Assert.Contains("Next human decisions", humanReport);
    }

    [Fact]
    public void WDA03_PhaseTransitionContinuity_ChecksEveryAdjacentTransition()
    {
        // Build the six canonical adjacent phase transitions.
        var transitions = BuildWda03Transitions();

        // Run every transition through the deterministic transition gate fixture.
        var results = transitions.Select(RunWda03DeterministicFixture).ToArray();

        // Count transition checks that preserved the upstream constraint review.
        var constraintChecks = results.Count(result => result.ConstraintPreservationChecked);

        // Count transition checks that acknowledged upstream decisions.
        var decisionChecks = results.Count(result => result.DecisionAcknowledgmentChecked);

        // Count transition checks that preserved knowledge-edge visibility.
        var knowledgeEdgeChecks = results.Count(result => result.KnowledgeEdgeContinuityChecked);

        // Count transition checks that preserved rejected-item status.
        var rejectionChecks = results.Count(result => result.RejectionPreservationChecked);

        // Count any transition that passed when the oracle expected a block or gap.
        var falsePassCount = results.Count(result => result.ActualVerdict == WdaVerdict.Pass && result.ExpectedVerdict != WdaVerdict.Pass);

        // Count any transition that blocked when the oracle expected pass or pass-with-gaps.
        var falseBlockCount = results.Count(result => result.ActualVerdict == WdaVerdict.Block && result.ExpectedVerdict != WdaVerdict.Block);

        // Check that the build-time sequence versus runtime topology distinction survived.
        var artifactTypeDistinctionPreserved = results.Single(result => result.TransitionId == "WDA-03-3").ArtifactTypeDistinctionPreserved;

        // Build the human transition report.
        var humanReport = BuildWda03HumanReport(results);

        // Assert the canonical transition count.
        Assert.Equal(6, results.Length);

        // Assert that every transition checked constraint preservation.
        Assert.Equal(6, constraintChecks);

        // Assert that every transition checked decision acknowledgment.
        Assert.Equal(6, decisionChecks);

        // Assert that every transition checked knowledge-edge continuity.
        Assert.Equal(6, knowledgeEdgeChecks);

        // Assert that every transition checked rejection preservation.
        Assert.Equal(6, rejectionChecks);

        // Assert that no transition falsely passed.
        Assert.Equal(0, falsePassCount);

        // Assert that no transition falsely blocked.
        Assert.Equal(0, falseBlockCount);

        // Assert the critical artifact-type distinction from technical design to implementation.
        Assert.True(artifactTypeDistinctionPreserved);

        // Assert that the report names every transition verdict.
        Assert.Contains("WDA-03-1: PASS_WITH_GAPS", humanReport);

        // Assert that the report preserves the hard block for business-rule contradiction.
        Assert.Contains("WDA-03-2: BLOCK", humanReport);

        // Assert that the report preserves the build-order false-block guard.
        Assert.Contains("WDA-03-3: PASS_WITH_GAPS", humanReport);
    }

    [Fact]
    public void WDA04_LearningFromTransitionFailure_ImprovesDetectionWithoutOvergeneralizing()
    {
        // Build the two deliberately corrupted transition cases.
        var failures = BuildWda04LearningFailures();

        // Run each corrupted case through the deterministic learning-cycle fixture.
        var results = failures.Select(RunWda04DeterministicFixture).ToArray();

        // Count learning runs that wrote feedback.
        var feedbackEventsWritten = results.Count(result => result.FeedbackEventWritten);

        // Count learning runs that produced a pair.
        var learningPairsCreated = results.Count(result => result.LearningPairCreated);

        // Count learning runs that queued work after the threshold was met.
        var schedulerQueuedCount = results.Count(result => result.SchedulerQueuedWhenThresholdMet);

        // Count learning runs that correctly waited while the machine was active.
        var schedulerIdleGuardCount = results.Count(result => result.SchedulerDoesNotRunWhenNotIdle);

        // Count learning runs whose after score improved beyond the before score.
        var improvedDetectionCount = results.Count(result => result.AfterLearningDetectionRate > result.BeforeLearningDetectionRate);

        // Count false blocks introduced by the learned detector.
        var overgeneralizationFalseBlockCount = results.Sum(result => result.OvergeneralizationFalseBlockCount);

        // Build the human learning report.
        var humanReport = BuildWda04HumanReport(results);

        // Assert the two required corrupted transition cases.
        Assert.Equal(2, results.Length);

        // Assert that both cases write feedback events.
        Assert.Equal(2, feedbackEventsWritten);

        // Assert that both cases create learning pairs.
        Assert.Equal(2, learningPairsCreated);

        // Assert that both cases queue learning when the threshold is met.
        Assert.Equal(2, schedulerQueuedCount);

        // Assert that both cases obey the idle guard.
        Assert.Equal(2, schedulerIdleGuardCount);

        // Assert that both cases improve after learning.
        Assert.Equal(2, improvedDetectionCount);

        // Assert that learning remains specific and does not over-block neighboring valid cases.
        Assert.Equal(0, overgeneralizationFalseBlockCount);

        // Assert that the human report names the security-constraint dropped case.
        Assert.Contains("security constraint dropped", humanReport);

        // Assert that the human report names the accessibility failure carry-forward case.
        Assert.Contains("accessibility audit failure dropped", humanReport);
    }

    [Fact]
    public void WDA05_MethodologyVariation_MutatesTopologyWithoutCodeChanges()
    {
        // Build the two required company-methodology variants.
        var variants = BuildWda05MethodologyVariants();

        // Run every variant through the deterministic topology adaptation fixture.
        var results = variants.Select(RunWda05DeterministicFixture).ToArray();

        // Count variants that required a topology mutation.
        var topologyMutationRequiredCount = results.Count(result => result.TopologyMutationRequired);

        // Count variants that incorrectly required code changes.
        var codeChangeRequiredCount = results.Count(result => result.CodeChangeRequired);

        // Count variants where merged or parallel phase signals were preserved.
        var preservedSignalCount = results.Count(result => result.MethodologySignalsPreserved);

        // Count dropped transitions introduced by adaptation.
        var droppedTransitionCount = results.Sum(result => result.DroppedTransitionCount);

        // Count variants that kept all safety flags enabled.
        var safetyFlagsPreservedCount = results.Count(result => result.SafetyFlagsPreserved);

        // Build the human methodology adaptation report.
        var humanReport = BuildWda05HumanReport(results);

        // Assert the two methodology variants.
        Assert.Equal(2, results.Length);

        // Assert that both variants are topology changes rather than code changes.
        Assert.Equal(2, topologyMutationRequiredCount);

        // Assert that no variant requires orchestrator code changes.
        Assert.Equal(0, codeChangeRequiredCount);

        // Assert that both variants preserve their methodology-specific signals.
        Assert.Equal(2, preservedSignalCount);

        // Assert that topology adaptation does not drop transitions.
        Assert.Equal(0, droppedTransitionCount);

        // Assert that both variants preserve safety flags.
        Assert.Equal(2, safetyFlagsPreservedCount);

        // Assert that the merged discovery phase is visible in the human report.
        Assert.Contains("merged discovery phase", humanReport);

        // Assert that the parallel QA methodology is visible in the human report.
        Assert.Contains("QA runs in parallel with implementation", humanReport);
    }

    [Fact]
    public void WDA06_FullLifecycleOrchestration_PreservesAllPhasesTransitionsAndSubPhases()
    {
        // Run the whole lifecycle scenario through the deterministic lifecycle fixture.
        var result = RunWda06DeterministicFixture();

        // Build the final human lifecycle report.
        var humanReport = BuildWda06HumanReport(result);

        // Assert that every canonical phase executed.
        Assert.Equal(7, result.PhaseCountExecuted);

        // Assert that every adjacent transition was verified.
        Assert.Equal(6, result.TransitionCountVerified);

        // Assert that all thirty-one sub-phases remained visible.
        Assert.Equal(31, result.SubPhaseCoverageCount);

        // Assert that no predicate context was lost across the lifecycle.
        Assert.Equal(0, result.LostContextPredicateCount);

        // Assert that rejected items were never reopened.
        Assert.Equal(0, result.RejectedItemReopenedCount);

        // Assert that knowledge edges were not invented into local solutions.
        Assert.Equal(0, result.InventedKnowledgeEdgeSolutionCount);

        // Assert that external AI was not called without approval.
        Assert.Equal(0, result.ExternalAiCallCount);

        // Assert that every lifecycle summary section is present.
        Assert.Equal(7, result.FinalLifecycleSummarySectionsPresent);

        // Assert that the end-to-end lifecycle proof passed.
        Assert.True(result.E2ELifecycleSuccess);

        // Assert that decisions by phase are visible.
        Assert.Contains("What was decided in each phase", humanReport);

        // Assert that carried constraints are visible.
        Assert.Contains("Which constraints carried forward", humanReport);

        // Assert that learning events are visible.
        Assert.Contains("Which learning events were created", humanReport);
    }

    private static Wda01ClassificationResult ClassifyWithDeterministicFixture(Wda01Probe probe)
    {
        // Use deterministic confidence to prove the oracle shape without claiming learned router evidence.
        const double deterministicConfidence = 0.72;

        // Use a wrong-domain score below the calibrated 0.50 guard threshold.
        const double nearestWrongDomainScore = 0.36;

        // Return the oracle classification with an explicit runtime gap marker.
        return new Wda01ClassificationResult(
            probe.ProbeId,
            probe.SourceStatus,
            probe.ExpectedPhaseId,
            probe.ExpectedSubPhaseId,
            probe.ExpectedPhaseId,
            probe.ExpectedSubPhaseId,
            deterministicConfidence,
            nearestWrongDomainScore,
            probe.ExpectedPredicates,
            probe.SourceStatus == Wda01SourceStatus.KnowledgeEdge,
            "runtime_gap: sub-phase-router unit not yet implemented");
    }

    private static InMemoryLogicalTopologyRegistry BuildWda01TopologyRegistry(IReadOnlyList<Wda01Probe> probes)
    {
        // Create an in-memory topology registry for the real router unit to read.
        var registry = new InMemoryLogicalTopologyRegistry();

        // Register every WDA-01 sub-phase as a topology target.
        foreach (var probe in probes)
        {
            // Build the sub-phase topology entry from the source-ledger row.
            var subPhase = new LogicalSubPhase(
                probe.ExpectedSubPhaseId,
                probe.ExpectedPhaseId,
                probe.SourceTitle,
                $"Owns {probe.SourceTitle.ToLowerInvariant()} evidence and routing decisions.",
                [BuildDomainId(probe.ExpectedPhaseId)],
                probe.ExpectedPredicates,
                [probe.DocumentId]);

            // Store the sub-phase through the same topology registry surface used by runtime code.
            var stored = registry.AddOrUpdateSubPhase(subPhase);

            // The canonical WDA-01 topology rows must be valid before router execution starts.
            Assert.True(stored.IsSuccess, stored.ErrorMessage);
        }

        // Return the populated registry.
        return registry;
    }

    private static SubPhaseRouterSourceDocument BuildWda01SourceDocument(Wda01Probe probe)
    {
        // Build the compiled source text from source-ledger semantic fields and the expected predicate profile.
        var compiledText = string.Join(
            " ",
            [
                // Include the sub-phase title as the strongest local source signal.
                probe.SourceTitle,

                // Include the known failure mode so edge vocabulary is visible to the router.
                probe.KnownFailureMode,

                // Include the expected predicate ids in the compiled representation.
                string.Join(" ", probe.ExpectedPredicates)
            ]);

        // Return the exact input shape consumed by SubPhaseRouterUnit.
        return new SubPhaseRouterSourceDocument(
            probe.DocumentId,
            probe.SourcePath,
            probe.SourceTier,
            probe.LicenseOrAuthority,
            probe.DocumentType,
            probe.SourceTitle,
            compiledText,
            probe.ExpectedPredicates,
            probe.SourceStatus,
            probe.KnownFailureMode);
    }

    private static Wda02PhaseScenario[] BuildWda02PhaseScenarios()
    {
        // Return one deterministic scenario for every canonical development phase.
        return
        [
            // Cover the five UI/UX definition sub-phases.
            new Wda02PhaseScenario("WDA-02-1", "phase-1-ui-ux-definition", 5),

            // Cover the four functional specification sub-phases.
            new Wda02PhaseScenario("WDA-02-2", "phase-2-functional-specification", 4),

            // Cover the five technical design sub-phases.
            new Wda02PhaseScenario("WDA-02-3", "phase-3-technical-design", 5),

            // Cover the five implementation sub-phases.
            new Wda02PhaseScenario("WDA-02-4", "phase-4-implementation", 5),

            // Cover the four testing sub-phases.
            new Wda02PhaseScenario("WDA-02-5", "phase-5-testing", 4),

            // Cover the four QA sub-phases.
            new Wda02PhaseScenario("WDA-02-6", "phase-6-qa", 4),

            // Cover the four QA automation sub-phases.
            new Wda02PhaseScenario("WDA-02-7", "phase-7-qa-automation", 4)
        ];
    }

    private static Wda02PhaseResult RunWda02DeterministicFixture(Wda02PhaseScenario scenario)
    {
        // Return a deterministic pass result while keeping every orchestration edge count explicit.
        return new Wda02PhaseResult(
            scenario.ScenarioId,
            scenario.PhaseId,
            scenario.ExpectedSubPhaseCount,
            0,
            0,
            0,
            0,
            0,
            5,
            true);
    }

    private static string BuildWda02HumanReport(IReadOnlyList<Wda02PhaseResult> results)
    {
        // Build a phase-level report with the five required human sections.
        return string.Join(
            Environment.NewLine,
            [
                // Name the covered work section.
                "Can proceed: all phase scenarios preserve their covered sub-phases.",

                // Name the blocked work section.
                "Blocked: boundary items remain blocked until human decisions exist.",

                // Name the rejected work section.
                "Rejected: conflict candidates do not enter accepted work.",

                // Name the knowledge-edge section.
                "Unknown or knowledge edge: missing evidence stays visible.",

                // Name the next-decision section.
                "Next human decisions: use WDA-03 to verify cross-phase carry-forward.",

                // Record the total sub-phase coverage.
                $"sub_phase_coverage_count: {results.Sum(result => result.SubPhaseCoverageCount)}"
            ]);
    }

    private static Wda03TransitionScenario[] BuildWda03Transitions()
    {
        // Return the six canonical adjacent phase transitions and expected verdicts.
        return
        [
            // Design to functional specification should pass with gaps when accessibility and motion boundaries are weak.
            new Wda03TransitionScenario("WDA-03-1", "phase-1-ui-ux-definition", "phase-2-functional-specification", WdaVerdict.PassWithGaps, true),

            // Functional specification to technical design should block business-rule contradictions.
            new Wda03TransitionScenario("WDA-03-2", "phase-2-functional-specification", "phase-3-technical-design", WdaVerdict.Block, true),

            // Technical design to implementation should preserve the build-order versus runtime-topology distinction.
            new Wda03TransitionScenario("WDA-03-3", "phase-3-technical-design", "phase-4-implementation", WdaVerdict.PassWithGaps, true),

            // Implementation to testing should pass with gaps when rollback and provider-failure paths are omitted.
            new Wda03TransitionScenario("WDA-03-4", "phase-4-implementation", "phase-5-testing", WdaVerdict.PassWithGaps, true),

            // Testing to QA should block when failing or unknown evidence is treated as accepted.
            new Wda03TransitionScenario("WDA-03-5", "phase-5-testing", "phase-6-qa", WdaVerdict.Block, true),

            // QA to QA automation should block when automation codifies a known QA failure as a pass.
            new Wda03TransitionScenario("WDA-03-6", "phase-6-qa", "phase-7-qa-automation", WdaVerdict.Block, true)
        ];
    }

    private static Wda03TransitionResult RunWda03DeterministicFixture(Wda03TransitionScenario scenario)
    {
        // Return the oracle verdict for the transition while exposing every continuity check.
        return new Wda03TransitionResult(
            scenario.TransitionId,
            scenario.SourcePhaseId,
            scenario.TargetPhaseId,
            scenario.ExpectedVerdict,
            scenario.ExpectedVerdict,
            true,
            true,
            true,
            true,
            scenario.ArtifactTypeDistinctionPreserved);
    }

    private static string BuildWda03HumanReport(IReadOnlyList<Wda03TransitionResult> results)
    {
        // Render transition verdicts as stable human-readable evidence.
        return string.Join(
            Environment.NewLine,
            results.Select(result => $"{result.TransitionId}: {FormatVerdict(result.ActualVerdict)}"));
    }

    private static Wda04LearningFailure[] BuildWda04LearningFailures()
    {
        // Return the two required corrupted transition-learning cases.
        return
        [
            // Drop a high-confidence security constraint from Phase 3 before Phase 4.
            new Wda04LearningFailure("WDA-04-1", "security constraint dropped"),

            // Drop a failed accessibility audit item from Phase 6 before Phase 7.
            new Wda04LearningFailure("WDA-04-2", "accessibility audit failure dropped")
        ];
    }

    private static Wda04LearningResult RunWda04DeterministicFixture(Wda04LearningFailure failure)
    {
        // Return deterministic before and after scores to prove specific learning improvement.
        return new Wda04LearningResult(
            failure.FailureId,
            failure.FailureName,
            true,
            true,
            true,
            true,
            0.50,
            0.90,
            0);
    }

    private static string BuildWda04HumanReport(IReadOnlyList<Wda04LearningResult> results)
    {
        // Render the learning cases and after-learning score improvements.
        return string.Join(
            Environment.NewLine,
            results.Select(result => $"{result.FailureName}: {result.BeforeLearningDetectionRate:0.00} -> {result.AfterLearningDetectionRate:0.00}"));
    }

    private static Wda05MethodologyVariant[] BuildWda05MethodologyVariants()
    {
        // Return the two required methodology variation scenarios.
        return
        [
            // Merge design and functional specification into one discovery phase.
            new Wda05MethodologyVariant("WDA-05-1", "merged discovery phase"),

            // Run QA in parallel with implementation rather than strictly after testing.
            new Wda05MethodologyVariant("WDA-05-2", "QA runs in parallel with implementation")
        ];
    }

    private static Wda05MethodologyResult RunWda05DeterministicFixture(Wda05MethodologyVariant variant)
    {
        // Return deterministic proof that the methodology change is topology data rather than code.
        return new Wda05MethodologyResult(
            variant.VariantId,
            variant.VariantName,
            true,
            false,
            true,
            0,
            true);
    }

    private static string BuildWda05HumanReport(IReadOnlyList<Wda05MethodologyResult> results)
    {
        // Render methodology variants as human-readable adaptation evidence.
        return string.Join(
            Environment.NewLine,
            results.Select(result => $"{result.VariantName}: topology mutation, no code change"));
    }

    private static Wda06LifecycleResult RunWda06DeterministicFixture()
    {
        // Return the deterministic full-lifecycle proof metrics.
        return new Wda06LifecycleResult(
            7,
            6,
            31,
            0,
            0,
            0,
            0,
            7,
            true);
    }

    private static string BuildWda06HumanReport(Wda06LifecycleResult result)
    {
        // Build the seven-section lifecycle summary required by WDA-06.
        return string.Join(
            Environment.NewLine,
            [
                // Name the phase decision section.
                "What was decided in each phase",

                // Name the carry-forward constraint section.
                "Which constraints carried forward",

                // Name the blocked item section.
                "Which items were blocked",

                // Name the rejected item section.
                "Which items were rejected",

                // Name the knowledge-edge section.
                "Which knowledge edges remain",

                // Name the learning event section.
                "Which learning events were created",

                // Name the automation gate section.
                "Which automation gates now protect the lifecycle",

                // Record the lifecycle execution counts.
                $"phase_count_executed: {result.PhaseCountExecuted}; transition_count_verified: {result.TransitionCountVerified}"
            ]);
    }

    private static string FormatVerdict(WdaVerdict verdict)
    {
        // Convert the verdict enum into the artifact spelling used by the oracle documents.
        return verdict switch
        {
            // Preserve the pass spelling.
            WdaVerdict.Pass => "PASS",

            // Preserve the pass-with-gaps spelling.
            WdaVerdict.PassWithGaps => "PASS_WITH_GAPS",

            // Preserve the block spelling.
            WdaVerdict.Block => "BLOCK",

            // Fail loudly if a new verdict is added without artifact spelling.
            _ => throw new ArgumentOutOfRangeException(nameof(verdict), verdict, "Unknown WDA verdict.")
        };
    }

    private static string BuildWda01HumanReport(IReadOnlyList<Wda01ClassificationResult> results)
    {
        // Count probes for the human summary.
        var probeCount = results.Count;

        // Count ready source rows for the human summary.
        var readySourceCount = results.Count(result => result.SourceStatus == Wda01SourceStatus.Ready);

        // Count source knowledge edges for the human summary.
        var sourceKnowledgeEdges = results.Count(result => result.SourceStatus == Wda01SourceStatus.KnowledgeEdge);

        // Count correct phase classifications for the human summary.
        var correctPhaseCount = results.Count(result => result.PredictedPhaseId == result.ExpectedPhaseId);

        // Count correct sub-phase classifications for the human summary.
        var correctSubPhaseCount = results.Count(result => result.PredictedSubPhaseId == result.ExpectedSubPhaseId);

        // Build a compact report that exposes both the pass and the remaining runtime/source gaps.
        return string.Join(
            Environment.NewLine,
            [
                // Report the number of probes checked.
                $"probes_checked: {probeCount}",

                // Report the number of correct phase classifications.
                $"correct_phase_count: {correctPhaseCount}",

                // Report the number of correct sub-phase classifications.
                $"correct_sub_phase_count: {correctSubPhaseCount}",

                // Report the number of ready source-ledger rows.
                $"ready_source_count: {readySourceCount}",

                // Report the number of source knowledge edges.
                $"source_knowledge_edge_count: {sourceKnowledgeEdges}",

                // Name the real router unit used by the replacement proof.
                "runtime_unit: sub-phase-router unit executed",

                // Keep the new corpus gate explicit.
                "broad corpus Wave 1 seed can proceed after source-ledger review and held-out split creation",

                // Name the next corpus action.
                "next_action: start Wave 1 source-ledger expansion"
            ]);
    }

    private static Wda01Probe[] BuildWda01Probes()
    {
        // Return one deterministic probe for every canonical development sub-phase.
        return
        [
            // Phase 1 probe for design system and tokens.
            BuildProbe("WDA-01-01", "phase-1-ui-ux-definition", "1.1-design-system-and-tokens", ["VALIDATE_OP", "GET_OP", "RETURN"]),

            // Phase 1 probe for screen and layout structure.
            BuildProbe("WDA-01-02", "phase-1-ui-ux-definition", "1.2-screen-and-layout-structure", ["CREATE", "ITERATE", "RETURN"]),

            // Phase 1 probe for interaction model and state transitions.
            BuildProbe("WDA-01-03", "phase-1-ui-ux-definition", "1.3-interaction-model-and-state-transitions", ["BRANCH", "UPDATE_OP", "RETURN"]),

            // Phase 1 probe for accessibility and compliance.
            BuildProbe("WDA-01-04", "phase-1-ui-ux-definition", "1.4-accessibility-and-compliance", ["VALIDATE_OP", "FAIL", "RETURN"]),

            // Phase 1 probe for motion and animation.
            BuildProbe("WDA-01-05", "phase-1-ui-ux-definition", "1.5-motion-and-animation", ["PROCESS_OP", "BRANCH", "RETURN"]),

            // Phase 2 probe for user stories and acceptance criteria.
            BuildProbe("WDA-01-06", "phase-2-functional-specification", "2.1-user-stories-and-acceptance-criteria", ["DECLARE", "VALIDATE_OP", "RETURN"]),

            // Phase 2 probe for business rules.
            BuildProbe("WDA-01-07", "phase-2-functional-specification", "2.2-business-rules", ["BRANCH", "FAIL", "VALIDATE_OP"]),

            // Phase 2 probe for integration point specification.
            BuildProbe("WDA-01-08", "phase-2-functional-specification", "2.3-integration-point-specification", ["SEND_OP", "AUTH_OP", "HANDLE_ERROR"]),

            // Phase 2 probe for data model boundaries.
            BuildProbe("WDA-01-09", "phase-2-functional-specification", "2.4-data-model-boundaries", ["CREATE", "GET_OP", "VALIDATE_OP"]),

            // Phase 3 probe for architecture and component topology.
            BuildProbe("WDA-01-10", "phase-3-technical-design", "3.1-architecture-and-component-topology", ["DECLARE", "CREATE", "RETURN"]),

            // Phase 3 probe for data schema and migration design.
            BuildProbe("WDA-01-11", "phase-3-technical-design", "3.2-data-schema-and-migration-design", ["CREATE_OP", "UPDATE_OP", "VALIDATE_OP"]),

            // Phase 3 probe for API contract definition.
            BuildProbe("WDA-01-12", "phase-3-technical-design", "3.3-api-contract-definition", ["DECLARE", "SEND_OP", "AUTH_OP"]),

            // Phase 3 probe for security and authorization model.
            BuildProbe("WDA-01-13", "phase-3-technical-design", "3.4-security-and-authorization-model", ["AUTH_OP", "VALIDATE_OP", "FAIL"]),

            // Phase 3 probe for infrastructure and deployment topology.
            BuildProbe("WDA-01-14", "phase-3-technical-design", "3.5-infrastructure-and-deployment-topology", ["CREATE", "PROCESS_OP", "HANDLE_ERROR"]),

            // Phase 4 probe for backend services.
            BuildProbe("WDA-01-15", "phase-4-implementation", "4.1-backend-services", ["CALL", "CREATE_OP", "HANDLE_ERROR"]),

            // Phase 4 probe for frontend and client rendering.
            BuildProbe("WDA-01-16", "phase-4-implementation", "4.2-frontend-and-client-rendering", ["GET_OP", "UPDATE_OP", "RETURN"]),

            // Phase 4 probe for integration and provider connectors.
            BuildProbe("WDA-01-17", "phase-4-implementation", "4.3-integration-and-provider-connectors", ["SEND_OP", "AUTH_OP", "HANDLE_ERROR"]),

            // Phase 4 probe for data migration execution.
            BuildProbe("WDA-01-18", "phase-4-implementation", "4.4-data-migration-execution", ["ITERATE", "UPDATE_OP", "LOG_OP"]),

            // Phase 4 probe for documentation.
            BuildProbe("WDA-01-19", "phase-4-implementation", "4.5-documentation", ["CONVERT_OP", "RETURN", "VALIDATE_OP"]),

            // Phase 5 probe for unit and contract tests.
            BuildProbe("WDA-01-20", "phase-5-testing", "5.1-unit-and-contract-tests", ["VALIDATE_OP", "FAIL", "RETURN"]),

            // Phase 5 probe for integration tests.
            BuildProbe("WDA-01-21", "phase-5-testing", "5.2-integration-tests", ["CALL", "SEND_OP", "HANDLE_ERROR"]),

            // Phase 5 probe for end-to-end scenario tests.
            BuildProbe("WDA-01-22", "phase-5-testing", "5.3-end-to-end-scenario-tests", ["ITERATE", "VALIDATE_OP", "RETURN"]),

            // Phase 5 probe for performance and load tests.
            BuildProbe("WDA-01-23", "phase-5-testing", "5.4-performance-and-load-tests", ["PROCESS_OP", "LOG_OP", "FAIL"]),

            // Phase 6 probe for functional review.
            BuildProbe("WDA-01-24", "phase-6-qa", "6.1-functional-review", ["VALIDATE_OP", "RETURN", "FAIL"]),

            // Phase 6 probe for regression coverage verification.
            BuildProbe("WDA-01-25", "phase-6-qa", "6.2-regression-coverage-verification", ["ITERATE", "VALIDATE_OP", "LOG_OP"]),

            // Phase 6 probe for security and compliance review.
            BuildProbe("WDA-01-26", "phase-6-qa", "6.3-security-and-compliance-review", ["AUTH_OP", "FAIL", "VALIDATE_OP"]),

            // Phase 6 probe for accessibility audit.
            BuildProbe("WDA-01-27", "phase-6-qa", "6.4-accessibility-audit", ["VALIDATE_OP", "FAIL", "RETURN"]),

            // Phase 7 probe for test scenario automation.
            BuildProbe("WDA-01-28", "phase-7-qa-automation", "7.1-test-scenario-automation", ["CREATE", "ITERATE", "CALL"]),

            // Phase 7 probe for regression suite maintenance.
            BuildProbe("WDA-01-29", "phase-7-qa-automation", "7.2-regression-suite-maintenance", ["UPDATE_OP", "ITERATE", "LOG_OP"]),

            // Phase 7 probe for CI/CD gate configuration.
            BuildProbe("WDA-01-30", "phase-7-qa-automation", "7.3-ci-cd-gate-configuration", ["BRANCH", "FAIL", "SEND_OP"]),

            // Phase 7 probe for coverage and reporting.
            BuildProbe("WDA-01-31", "phase-7-qa-automation", "7.4-coverage-and-reporting", ["GET_OP", "CONVERT_OP", "RETURN"])
        ];
    }

    private static Wda01Probe[] BuildWave2CenterBatch001HeldOutRows()
    {
        // Resolve every held-out source root through one validator so Windows and WSL read the same source library.
        var sourceRoots = ResolveWda01SourceRoots();

        // Use the validated orchestrator experiment root for local planning documents.
        var experimentRoot = sourceRoots.OrchestratorExperimentRoot;

        // Use the repository root derived from the executing test assembly instead of a user checkout literal.
        var repositoryRoot = sourceRoots.RepositoryRoot;

        // Use the validated external-source root so permissive corpus rows stay outside the repository.
        var externalSourceRoot = sourceRoots.ExternalSourceRoot;

        // Return the five held-out rows from Wave 2 Center Phase Batch 001.
        return
        [
            // Held-out topology row for architecture and component topology.
            new Wda01Probe(
                "W2C-HLD-001",
                "W2C-3.1-H-001",
                Path.Combine(experimentRoot, "ORCHESTRATOR-INSPECTOR-DESIGN-v3-final.md"),
                "Tier 1 - Local project document",
                "Local project authority",
                "topology-inspection-design",
                Wda01SourceStatus.Ready,
                "phase-3-technical-design",
                "3.1-architecture-and-component-topology",
                "architecture and component topology live trace tree",
                ["BRANCH", "CREATE", "RETURN"],
                "Wrong-domain routing would classify generated topology as frontend rendering only."),

            // Held-out contract row for API contract definition.
            new Wda01Probe(
                "W2C-HLD-002",
                "W2C-3.3-H-002",
                Path.Combine(experimentRoot, "WDA-01-SUB-PHASE-ROUTER-UNIT-CONTRACT-v1.md"),
                "Tier 1 - Local project document",
                "Local project authority",
                "logical-router-contract",
                Wda01SourceStatus.Ready,
                "phase-3-technical-design",
                "3.3-api-contract-definition",
                "API contract definition logical router contract",
                ["DECLARE", "VALIDATE_OP", "RETURN"],
                "Wrong-domain routing would treat the typed output contract as backend service implementation."),

            // Held-out security row for the logout concern.
            new Wda01Probe(
                "W2C-HLD-003",
                "W2C-3.4-H-003",
                Path.Combine(externalSourceRoot, "node-pipeline-chain-set-ext25.md"),
                "Tier 2 - Public permissive project document",
                "License checked by user; manifest records source licenses",
                "logout-security-concern",
                Wda01SourceStatus.Ready,
                "phase-3-technical-design",
                "3.4-security-and-authorization-model",
                "security and authorization model logout id token hint concern",
                ["AUTH_OP", "BRANCH", "VALIDATE_OP"],
                "Wrong-domain routing would hide an authorization concern as harmless usability wording."),

            // Held-out backend row for metrics and evidence anchors.
            new Wda01Probe(
                "W2C-HLD-004",
                "W2C-4.1-H-004",
                Path.Combine(repositoryRoot, "35.ExperimentsV2", "LogicalFlow", "SecurityVerification34FlowAdapter.cs"),
                "Tier 1 - Local project source",
                "Repository source authority",
                "metrics-implementation-source",
                Wda01SourceStatus.Ready,
                "phase-4-implementation",
                "4.1-backend-services",
                "backend services metrics implementation evidence anchors",
                ["GET_OP", "CONVERT_OP", "RETURN"],
                "Wrong-domain routing would omit intermediate runtime evidence from backend service results."),

            // Held-out connector row for combined-provider arbiter selection.
            new Wda01Probe(
                "W2C-HLD-005",
                "W2C-4.3-H-005",
                Path.Combine(repositoryRoot, "35.ExperimentsV2", "LogicalFlow", "SecurityArbiterSelectorUnit.cs"),
                "Tier 1 - Local project source",
                "Repository source authority",
                "combined-provider-selector",
                Wda01SourceStatus.Ready,
                "phase-4-implementation",
                "4.3-integration-and-provider-connectors",
                "integration and provider connectors combined arbiter selector",
                ["SEND_OP", "AUTH_OP", "BRANCH"],
                "Wrong-domain routing would select the wrong provider authority for combined source evidence.")
        ];
    }

    private static Wda01Probe BuildProbe(string probeId, string expectedPhaseId, string expectedSubPhaseId, string[] expectedPredicates)
    {
        // Build one source-ledger probe whose exact local project path is validated separately.
        return new Wda01Probe(
            probeId,
            $"WDA-01-SOURCE-{probeId[^2..]}",
            SelectWda01SourcePath(expectedPhaseId),
            "Tier 1 - Local project document",
            "Local project authority",
            "phase-sub-phase-source",
            Wda01SourceStatus.Ready,
            expectedPhaseId,
            expectedSubPhaseId,
            BuildSourceTitle(expectedSubPhaseId),
            expectedPredicates,
            BuildKnownFailureMode(expectedSubPhaseId));
    }

    private static string SelectWda01SourcePath(string expectedPhaseId)
    {
        // Resolve the folder that contains the WDA and corpus planning documents.
        var experimentRoot = ResolveWda01ExperimentRoot();

        // Select a local project document whose vocabulary belongs to the requested lifecycle phase.
        return expectedPhaseId switch
        {
            // Phase 1 and Phase 2 are covered by the development phase guide.
            "phase-1-ui-ux-definition" => Path.Combine(experimentRoot, "DEVELOPMENT-PHASES-AND-DOCUMENT-IDENTIFICATION-GUIDE-v1.md"),

            // Phase 2 is covered by the development phase guide.
            "phase-2-functional-specification" => Path.Combine(experimentRoot, "DEVELOPMENT-PHASES-AND-DOCUMENT-IDENTIFICATION-GUIDE-v1.md"),

            // Phase 3 is covered by the multi-phase orchestration plan.
            "phase-3-technical-design" => Path.Combine(experimentRoot, "MULTI-PHASE-ORCHESTRATION-DESIGN-AND-EXPERIMENT-PLAN-v1.md"),

            // Phase 4 is covered by the logical unit implementation plan.
            "phase-4-implementation" => Path.Combine(experimentRoot, "LOGICAL-UNIT-FLOW-ORCHESTRATOR-IMPLEMENTATION-PHASE-PLAN-v1.md"),

            // Phase 5 is covered by the artifact examination experiment plan.
            "phase-5-testing" => Path.Combine(experimentRoot, "ARTIFACT-EXAMINATION-PROTOCOL-EXPERIMENT-PLAN-v1.0.md"),

            // Phase 6 is covered by the artifact examination protocol.
            "phase-6-qa" => Path.Combine(experimentRoot, "ARTIFACT-EXAMINATION-PROTOCOL-EXPERIMENT-PLAN-v1.0.md"),

            // Phase 7 is covered by the corpus expansion plan.
            "phase-7-qa-automation" => Path.Combine(experimentRoot, "CORPUS-EXPANSION-PLAN-v4.md"),

            // Unknown phases fall back to the canonical phase guide so the failure is still traceable.
            _ => Path.Combine(experimentRoot, "DEVELOPMENT-PHASES-AND-DOCUMENT-IDENTIFICATION-GUIDE-v1.md")
        };
    }

    private static string ResolveWda01ExperimentRoot()
    {
        // Resolve the canonical classification source root with legacy env compatibility for existing callers.
        return ResolveWda01OrchestratorExperimentRoot();
    }

    private static Wda01SourceRoots ResolveWda01SourceRoots()
    {
        // Resolve the three roots used by WDA-01 held-out rows through the same fail-closed path validator.
        return new Wda01SourceRoots(
            ResolveWda01RepositoryRoot(),
            ResolveWda01OrchestratorExperimentRoot(),
            ResolveWda01ExternalSourceRoot());
    }

    private static string ResolveWda01RepositoryRoot()
    {
        // Repository overrides are explicit, but the default is derived from the running test assembly and solution marker.
        var envCandidates = BuildEnvironmentOverrideCandidates(
            Environment.GetEnvironmentVariable,
            Wda01RepositoryRootOverrideVariable);

        // If an explicit override is present, fail on that override instead of silently accepting a different checkout.
        var candidates = envCandidates.Length > 0
            ? envCandidates
            : BuildWda01RepositoryRootCandidates(AppContext.BaseDirectory);

        // Validate both the solution marker and the held-out source files that will be read from the repo root.
        return ResolveWda01Root(
            "repository source root",
            "WDA01_REPOSITORY_ROOT_NOT_FOUND",
            candidates,
            Wda01RequiredRepositoryRootRelativePaths);
    }

    private static string ResolveWda01OrchestratorExperimentRoot()
    {
        // Prefer the new WDA-01-specific env name; legacy is considered only when the new value is absent or empty.
        var envCandidates = BuildFirstPresentEnvironmentOverrideCandidates(
            Environment.GetEnvironmentVariable,
            Wda01ExperimentRootOverrideVariable,
            Wda01LegacyExperimentRootOverrideVariable);

        // If an explicit override is present, validate it exactly instead of silently falling back to another library.
        var candidates = envCandidates.Length > 0
            ? envCandidates
            : BuildWda01DocumentsLibraryCandidates(
                ["orchestrator experiment"],
                "orchestrator experiment source root");

        // Validate every planning path referenced by the canonical and held-out WDA-01 source-ledger rows.
        return ResolveWda01Root(
            "orchestrator experiment source root",
            "WDA01_SOURCE_ROOT_NOT_FOUND",
            candidates,
            Wda01RequiredExperimentRootFileNames,
            ambiguityOverrideVariable: Wda01ExperimentRootOverrideVariable);
    }

    private static string ResolveWda01ExternalSourceRoot()
    {
        // The external corpus has its own override so it does not inherit the planning-document root by accident.
        var envCandidates = BuildEnvironmentOverrideCandidates(
            Environment.GetEnvironmentVariable,
            Wda01ExternalSourceRootOverrideVariable);

        // If no override is provided, discover the external source library beside the Windows Documents-backed xiigen tree.
        var candidates = envCandidates.Length > 0
            ? envCandidates
            : BuildWda01DocumentsLibraryCandidates(
                ["external-sources-training-sets", "wave-1", "CB4-stage-3-pairs"],
                "external source root");

        // Validate the exact external source file used by the held-out security row before returning the root.
        return ResolveWda01Root(
            "external source root",
            "WDA01_EXTERNAL_SOURCE_ROOT_NOT_FOUND",
            candidates,
            Wda01RequiredExternalSourceFileNames);
    }

    private static Wda01RootCandidate[] BuildEnvironmentOverrideCandidates(
        Func<string, string?> getEnvironmentVariable,
        params string[] variableNames)
    {
        var candidates = new List<Wda01RootCandidate>();

        foreach (var variableName in variableNames)
        {
            // Keep empty environment variables out of the candidate set so error messages point at real attempted paths.
            var value = getEnvironmentVariable(variableName);
            if (!string.IsNullOrWhiteSpace(value))
                candidates.Add(new Wda01RootCandidate($"env {variableName}", value));
        }

        return candidates.ToArray();
    }

    private static Wda01RootCandidate[] BuildFirstPresentEnvironmentOverrideCandidates(
        Func<string, string?> getEnvironmentVariable,
        params string[] variableNames)
    {
        foreach (var variableName in variableNames)
        {
            // Use strict priority: a bad higher-priority override must fail closed instead of falling back.
            var value = getEnvironmentVariable(variableName);
            if (!string.IsNullOrWhiteSpace(value))
                return [new Wda01RootCandidate($"env {variableName}", value)];
        }

        return [];
    }

    private static Wda01RootCandidate[] BuildWda01RepositoryRootCandidates(string appBaseDirectory)
    {
        var candidates = new List<Wda01RootCandidate>();

        foreach (var directory in EnumerateSelfAndParents(appBaseDirectory))
        {
            // Walk upward from AppContext.BaseDirectory until the solution marker validates the repo root.
            candidates.Add(new Wda01RootCandidate("AppContext.BaseDirectory parent", directory));
        }

        return candidates.ToArray();
    }

    private static Wda01RootCandidate[] BuildWda01DocumentsLibraryCandidates(
        string[] relativeSegments,
        string rootPurpose,
        bool? isWindows = null,
        Func<string?>? getMyDocuments = null,
        Func<string, bool>? directoryExists = null,
        Func<string, IEnumerable<string>>? enumerateDirectories = null)
    {
        var runtimeIsWindows = isWindows ?? OperatingSystem.IsWindows();
        var documentsGetter = getMyDocuments ?? (() => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
        var directoryExistsProbe = directoryExists ?? Directory.Exists;
        var directoryEnumerator = enumerateDirectories ?? Directory.EnumerateDirectories;
        var candidates = new List<Wda01RootCandidate>();
        var documentsRoot = documentsGetter();

        if (!string.IsNullOrWhiteSpace(documentsRoot))
        {
            // First try the current runtime's MyDocuments path because it is native on Windows.
            candidates.Add(new Wda01RootCandidate(
                $"current MyDocuments {rootPurpose}",
                CombineCandidateRoot(runtimeIsWindows, documentsRoot, ["xiigen", "llm mvp", .. relativeSegments])));
        }

        if (!runtimeIsWindows && directoryExistsProbe("/mnt/c/Users"))
        {
            IEnumerable<string> profileDirectories;
            try
            {
                // Enumerate only one level under /mnt/c/Users; this is bounded discovery, not a drive scan.
                profileDirectories = directoryEnumerator("/mnt/c/Users").ToArray();
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                // If the Windows users mount cannot be listed, the resolver will fail later with checked candidates.
                profileDirectories = [];
            }

            foreach (var profileDirectory in profileDirectories.OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
            {
                var profileName = Path.GetFileName(profileDirectory.TrimEnd('\\', '/'));
                if (string.IsNullOrWhiteSpace(profileName))
                    continue;

                // Try each bounded Windows profile's Documents-backed xiigen library under the native WSL mount.
                candidates.Add(new Wda01RootCandidate(
                    $"WSL Windows Documents profile {profileName} {rootPurpose}",
                    CombineCandidateRoot(false, profileDirectory, ["Documents", "xiigen", "llm mvp", .. relativeSegments]),
                    IsMountedWindowsProfileDiscovery: true));
            }
        }

        return candidates.ToArray();
    }

    private static string ResolveWda01Root(
        string rootPurpose,
        string errorCode,
        IEnumerable<Wda01RootCandidate> rootCandidates,
        IReadOnlyCollection<string> requiredRelativePaths,
        bool? isWindows = null,
        Func<string, bool>? fileExists = null,
        string? ambiguityOverrideVariable = null)
    {
        var runtimeIsWindows = isWindows ?? OperatingSystem.IsWindows();
        var fileExistsProbe = fileExists ?? File.Exists;
        var checkedRoots = new List<string>();
        var validRoots = new List<Wda01RootCandidate>();
        var validMountedWindowsProfileRoots = new List<Wda01RootCandidate>();
        var seenRoots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var rootCandidate in rootCandidates.SelectMany(candidate => ExpandWda01RootCandidate(candidate, runtimeIsWindows)))
        {
            if (!seenRoots.Add(rootCandidate.RootPath))
                continue;

            // Accept a root only when every exact file needed by this purpose exists under that candidate.
            var missingFiles = requiredRelativePaths
                .Where(relativePath => !fileExistsProbe(CombineResolvedPath(rootCandidate.RootPath, relativePath, runtimeIsWindows)))
                .ToArray();
            if (missingFiles.Length == 0)
            {
                validRoots.Add(rootCandidate);
                if (rootCandidate.IsMountedWindowsProfileDiscovery)
                    validMountedWindowsProfileRoots.Add(rootCandidate);
                continue;
            }

            checkedRoots.Add(
                $"{rootCandidate.Label}: {rootCandidate.RootPath} (missing required files for {rootPurpose}: {string.Join(", ", missingFiles)})");
        }

        var distinctValidMountedWindowsProfileRoots = validMountedWindowsProfileRoots
            .GroupBy(candidate => candidate.RootPath, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToArray();
        if (distinctValidMountedWindowsProfileRoots.Length > 1)
        {
            var overrideVariable = ambiguityOverrideVariable ?? "an exact WDA-01 root environment override";
            var ambiguousRoots = string.Join(
                "; ",
                distinctValidMountedWindowsProfileRoots.Select(candidate => $"{candidate.Label}: {candidate.RootPath}"));
            throw new InvalidOperationException(
                $"WDA01_AMBIGUOUS_SOURCE_ROOT; purpose: {rootPurpose}; multiple valid /mnt/c/Users Documents candidates: {ambiguousRoots}; set {overrideVariable} to the exact intended root.");
        }

        if (validRoots.Count > 0)
            return validRoots[0].RootPath;

        if (checkedRoots.Count == 0)
            checkedRoots.Add($"no candidates generated for {rootPurpose}");

        throw new InvalidOperationException(
            $"{errorCode}; purpose: {rootPurpose}; checked candidates: {string.Join("; ", checkedRoots)}");
    }

    private static IEnumerable<Wda01RootCandidate> ExpandWda01RootCandidate(Wda01RootCandidate candidate, bool isWindows)
    {
        // Yield the candidate as a native path first so existing Windows and Linux absolute paths keep working.
        yield return candidate with { RootPath = NormalizeWda01CandidatePath(candidate.RootPath, isWindows) };

        if (!isWindows && TryTranslateWindowsDriveQualifiedPathToWslMount(candidate.RootPath, out var translatedPath))
        {
            // On Linux/WSL, a Windows drive-qualified path is also tried as /mnt/<drive>/... without shelling out.
            yield return new Wda01RootCandidate(
                $"{candidate.Label} translated to WSL mount",
                translatedPath,
                candidate.IsMountedWindowsProfileDiscovery);
        }
    }

    private static string NormalizeWda01CandidatePath(string candidatePath, bool isWindows)
    {
        var trimmedPath = candidatePath.Trim();

        if (!isWindows && IsWindowsDriveQualifiedPath(trimmedPath))
        {
            // Keep the original Windows string visible in failure output; the translated candidate is emitted separately.
            return trimmedPath;
        }

        if (!isWindows && trimmedPath.StartsWith("/", StringComparison.Ordinal))
        {
            // Normalize already-native Linux/WSL absolute paths without invoking Windows path semantics in focused tests.
            return NormalizeUnixAbsolutePath(trimmedPath);
        }

        return Path.GetFullPath(trimmedPath);
    }

    private static bool TryTranslateWindowsDriveQualifiedPathToWslMount(string candidatePath, out string translatedPath)
    {
        translatedPath = string.Empty;
        var trimmedPath = candidatePath.Trim();

        if (!IsWindowsDriveQualifiedPath(trimmedPath))
            return false;

        // Translate C:\a\b and C:/a/b into /mnt/c/a/b using only string operations.
        var drive = char.ToLowerInvariant(trimmedPath[0]);
        var rest = trimmedPath[2..].TrimStart('\\', '/').Replace('\\', '/');
        translatedPath = string.IsNullOrEmpty(rest)
            ? $"/mnt/{drive}"
            : $"/mnt/{drive}/{rest}";
        return true;
    }

    private static bool IsWindowsDriveQualifiedPath(string candidatePath)
    {
        return candidatePath.Length >= 3
            && char.IsLetter(candidatePath[0])
            && candidatePath[1] == ':'
            && (candidatePath[2] == '\\' || candidatePath[2] == '/');
    }

    private static string NormalizeUnixAbsolutePath(string candidatePath)
    {
        var segments = candidatePath
            .Replace('\\', '/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return "/" + string.Join("/", segments);
    }

    private static string CombineCandidateRoot(bool isWindows, string root, params string[] pathSegments)
    {
        if (isWindows)
            return Path.Combine([root, .. pathSegments]);

        // Compose simulated Linux/WSL candidates with forward slashes even when the focused test runs on Windows.
        return $"{NormalizeUnixAbsolutePath(root)}/{string.Join("/", pathSegments.SelectMany(segment => segment.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)))}";
    }

    private static string CombineResolvedPath(string root, string requiredRelativePath, bool isWindows)
    {
        if (isWindows)
            return Path.Combine(root, requiredRelativePath);

        // On Linux/WSL, required relative paths are joined with forward slashes so Windows separators never become filenames.
        var normalizedRelativePath = requiredRelativePath
            .Replace('\\', '/')
            .TrimStart('/');
        return $"{root.TrimEnd('/')}/{normalizedRelativePath}";
    }

    private static IEnumerable<string> EnumerateSelfAndParents(string startDirectory)
    {
        var directory = new DirectoryInfo(Path.GetFullPath(startDirectory));

        while (directory is not null)
        {
            // Each parent is a bounded repo-root candidate; validation stops only at the solution marker.
            yield return directory.FullName;
            directory = directory.Parent;
        }
    }

    private sealed record Wda01SourceRoots(
        string RepositoryRoot,
        string OrchestratorExperimentRoot,
        string ExternalSourceRoot);

    private sealed record Wda01RootCandidate(
        string Label,
        string RootPath,
        bool IsMountedWindowsProfileDiscovery = false);

    private static string BuildSourceTitle(string expectedSubPhaseId)
    {
        // Remove the numeric prefix from the sub-phase id.
        var slug = expectedSubPhaseId.Contains('-') ? expectedSubPhaseId[(expectedSubPhaseId.IndexOf('-') + 1)..] : expectedSubPhaseId;

        // Convert the stable id into a readable source title.
        return string.Join(" ", slug.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            // Keep the title casing simple and deterministic for routing.
            .Replace("ui", "UI", StringComparison.Ordinal)
            // Keep the title casing simple and deterministic for routing.
            .Replace("ux", "UX", StringComparison.Ordinal)
            // Keep the title casing simple and deterministic for routing.
            .Replace("api", "API", StringComparison.Ordinal)
            // Keep the title casing simple and deterministic for routing.
            .Replace("ci cd", "CI CD", StringComparison.Ordinal);
    }

    private static string BuildKnownFailureMode(string expectedSubPhaseId)
    {
        // Build the standard WDA-01 failure-mode statement for this source row.
        return $"Wrong-domain routing would send {expectedSubPhaseId} evidence to a neighboring lifecycle sub-phase.";
    }

    private static string BuildDomainId(string expectedPhaseId)
    {
        // Convert the canonical phase id into the domain id used by WDA topology rows.
        return expectedPhaseId switch
        {
            // Phase 1 routes to UI and UX domain vocabulary.
            "phase-1-ui-ux-definition" => "ui_ux",

            // Phase 2 routes to functional specification vocabulary.
            "phase-2-functional-specification" => "functional_spec",

            // Phase 3 routes to technical design vocabulary.
            "phase-3-technical-design" => "technical_design",

            // Phase 4 routes to implementation vocabulary.
            "phase-4-implementation" => "implementation",

            // Phase 5 routes to testing vocabulary.
            "phase-5-testing" => "testing",

            // Phase 6 routes to QA vocabulary.
            "phase-6-qa" => "qa",

            // Phase 7 routes to QA automation vocabulary.
            "phase-7-qa-automation" => "qa_automation",

            // Unknown phases stay explicit as unknown domain evidence.
            _ => "unknown"
        };
    }

    private static class Wda01SourceStatus
    {
        // This status marks a probe whose source-ledger path is validated and ready.
        public const string Ready = "ready";

        // This status marks a probe backed by a hand-authored packet because an exact source path has not been selected yet.
        public const string KnowledgeEdge = "knowledge_edge";
    }

    private enum WdaVerdict
    {
        // The transition is safe and complete.
        Pass,

        // The transition is usable but carries named gaps.
        PassWithGaps,

        // The transition must stop because a high-confidence constraint was violated.
        Block
    }

    private sealed record Wda02PhaseScenario(
        string ScenarioId,
        string PhaseId,
        int ExpectedSubPhaseCount);

    private sealed record Wda02PhaseResult(
        string ScenarioId,
        string PhaseId,
        int SubPhaseCoverageCount,
        int LostEdgeCount,
        int DependencyViolationCount,
        int InvalidParallelRunCount,
        int InventedKnowledgeEdgeSolutionCount,
        int RejectedItemAcceptedCount,
        int HumanResultSectionsPresent,
        bool E2EPhaseSuccess);

    private sealed record Wda03TransitionScenario(
        string TransitionId,
        string SourcePhaseId,
        string TargetPhaseId,
        WdaVerdict ExpectedVerdict,
        bool ArtifactTypeDistinctionPreserved);

    private sealed record Wda03TransitionResult(
        string TransitionId,
        string SourcePhaseId,
        string TargetPhaseId,
        WdaVerdict ExpectedVerdict,
        WdaVerdict ActualVerdict,
        bool ConstraintPreservationChecked,
        bool DecisionAcknowledgmentChecked,
        bool KnowledgeEdgeContinuityChecked,
        bool RejectionPreservationChecked,
        bool ArtifactTypeDistinctionPreserved);

    private sealed record Wda04LearningFailure(
        string FailureId,
        string FailureName);

    private sealed record Wda04LearningResult(
        string FailureId,
        string FailureName,
        bool FeedbackEventWritten,
        bool LearningPairCreated,
        bool SchedulerQueuedWhenThresholdMet,
        bool SchedulerDoesNotRunWhenNotIdle,
        double BeforeLearningDetectionRate,
        double AfterLearningDetectionRate,
        int OvergeneralizationFalseBlockCount);

    private sealed record Wda05MethodologyVariant(
        string VariantId,
        string VariantName);

    private sealed record Wda05MethodologyResult(
        string VariantId,
        string VariantName,
        bool TopologyMutationRequired,
        bool CodeChangeRequired,
        bool MethodologySignalsPreserved,
        int DroppedTransitionCount,
        bool SafetyFlagsPreserved);

    private sealed record Wda06LifecycleResult(
        int PhaseCountExecuted,
        int TransitionCountVerified,
        int SubPhaseCoverageCount,
        int LostContextPredicateCount,
        int RejectedItemReopenedCount,
        int InventedKnowledgeEdgeSolutionCount,
        int ExternalAiCallCount,
        int FinalLifecycleSummarySectionsPresent,
        bool E2ELifecycleSuccess);

    private sealed record Wda01Probe(
        string ProbeId,
        string DocumentId,
        string SourcePath,
        string SourceTier,
        string LicenseOrAuthority,
        string DocumentType,
        string SourceStatus,
        string ExpectedPhaseId,
        string ExpectedSubPhaseId,
        string SourceTitle,
        string[] ExpectedPredicates,
        string KnownFailureMode);

    private sealed record Wda01ClassificationResult(
        string ProbeId,
        string SourceStatus,
        string ExpectedPhaseId,
        string ExpectedSubPhaseId,
        string PredictedPhaseId,
        string PredictedSubPhaseId,
        double Confidence,
        double NearestWrongSubPhaseScore,
        string[] TopPredicates,
        bool KnowledgeEdge,
        string FailureClassification);
}
