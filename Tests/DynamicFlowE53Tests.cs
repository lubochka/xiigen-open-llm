// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using ExperimentsV2;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace LlmFromScratch.Tests;

public sealed class DynamicFlowE53Tests
{
    [Fact]
    public void E53_Run_WritesJsonMarkdownAndMermaidEvidence()
    {
        TestOwnedOutputRoot.Run(output =>
        {
            var experiment = new E53HighLevelOrchestrationProof();
            var result = experiment.WriteEvidence(output.RootPath);
            Assert.True(result.IsSuccess, result.ErrorMessage);
            string json = output.RequireOwnedPath(result.Value!);
            string md = output.RequireOwnedPath(Path.ChangeExtension(json, ".md"));
            string mmd = output.RequireOwnedPath(Path.ChangeExtension(json, ".mmd"));
            Assert.True(File.Exists(json), json);
            Assert.True(File.Exists(md), md);
            Assert.True(File.Exists(mmd), mmd);
        });
    }

    [Fact]
    public void E53_EvidenceJson_Uses_RequiredMetricKeys()
    {
        TestOwnedOutputRoot.Run(output =>
        {
            var experiment = new E53HighLevelOrchestrationProof();
            var result = experiment.WriteEvidence(output.RootPath);
            Assert.True(result.IsSuccess, result.ErrorMessage);
            var json = output.RequireOwnedPath(result.Value!);
            output.RequireOwnedPath(Path.ChangeExtension(json, ".md"));
            output.RequireOwnedPath(Path.ChangeExtension(json, ".mmd"));
            using var document = JsonDocument.Parse(File.ReadAllText(json));
            var metrics = document.RootElement.GetProperty("metrics");
            Assert.True(metrics.TryGetProperty("e2e_orchestration_success", out var success), "Missing e2e_orchestration_success");
            Assert.True(success.GetBoolean());
            Assert.False(metrics.TryGetProperty("e2_e_orchestration_success", out _), "Wrong split acronym key was emitted.");
        });
    }

    [Fact]
    public void E53_Run_PreservesAllHardEdges()
    {
        var evidence = BuildEvidence();

        Assert.Equal(12, evidence.SourceFactCoverage.Length);
        Assert.All(evidence.SourceFactCoverage, fact => Assert.True(fact.Preserved, fact.FactId));
        Assert.Equal(0, evidence.Metrics.LostEdgeCount);
        Assert.Contains("duplicate account", evidence.FinalHumanResult, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Biometric passport verification is outside current local coverage", evidence.FinalHumanResult, StringComparison.Ordinal);
    }

    [Fact]
    public void E53_Run_PreservesArrayFanOutAndFanInCounts()
    {
        var evidence = BuildEvidence();

        Assert.Equal(11, evidence.Metrics.Node02InputCount);
        Assert.Equal(11, evidence.Metrics.Node02OutputCount);
        Assert.Equal(11, evidence.Metrics.Node04InputCount);
        Assert.Equal(11, evidence.Metrics.Node04OutputCount);
        Assert.Equal(11, evidence.Metrics.Node05InputCount);
        Assert.Equal(11, evidence.Metrics.Node05OutputCount);
        Assert.All(evidence.ArrayCounts.Where(a => a.NodeId is "node-02" or "node-04" or "node-05"),
            array => Assert.True(array.ContractHeld && !array.CollapsedArray, array.NodeId));
    }

    [Fact]
    public void E53_Run_EnforcesDependencyOrder()
    {
        var evidence = BuildEvidence();

        Assert.Equal(0, evidence.Metrics.DependencyViolationCount);
        Assert.All(evidence.DependencyTrace, dependency => Assert.True(dependency.ActualOrderValid, dependency.DependencyId));
        Assert.Contains(evidence.DependencyTrace, d =>
            d.DependencyId == "dep-provider-before-account" &&
            d.Source == "b01_validate_provider_identity" &&
            d.Target == "b03_resolve_existing_or_pending_account");
        Assert.Contains(evidence.DependencyTrace, d =>
            d.DependencyId == "dep-policy-before-activation" &&
            d.SourceState == "early_started_boundary" &&
            d.TargetRequiresSourceState == "resolved_evidence_or_confirmed_boundary");
    }

    [Fact]
    public void E53_Run_AllowsOnlyValidParallelGroups()
    {
        var evidence = BuildEvidence();

        Assert.Equal(0, evidence.Metrics.InvalidParallelRunCount);
        Assert.All(evidence.ParallelTrace, group =>
        {
            Assert.True(group.Allowed, group.ParallelGroupId);
            Assert.Empty(group.InvalidParallelDependencies);
        });
        Assert.Contains(evidence.ParallelTrace, group =>
            group.ParallelGroupId == "pg-initial-evidence" &&
            group.Items.Contains("b08_prepare_policy_acceptance_gap") &&
            group.Items.Contains("b10_prepare_biometric_context_pack"));
    }

    [Fact]
    public void E53_Run_RoutesBiometricPassportToKnowledgeEdge()
    {
        var evidence = BuildEvidence();

        Assert.Equal(0, evidence.Metrics.InventedBiometricPlanItems);
        Assert.Equal("cp01_biometric_passport_verification", evidence.KnowledgeEdgeContextPack.ContextPackId);
        Assert.False(evidence.KnowledgeEdgeContextPack.LiveExternalCall);
        Assert.True(evidence.KnowledgeEdgeContextPack.ApprovalRequiredBeforeLiveCall);
        Assert.Contains("biometric passport verification", evidence.KnowledgeEdgeContextPack.MissingCompetence);
        Assert.Contains(evidence.LearningGapPackets, p => p.PacketId == "lg01_biometric_passport_verification");
    }

    [Fact]
    public void E53_Run_RejectsDuplicateAccountCreation()
    {
        var evidence = BuildEvidence();

        Assert.False(evidence.Metrics.DuplicateCreationAccepted);
        Assert.Contains(evidence.AppliedResults, r => r.ResultId == "r04_duplicate_rejection" && r.ResultStatus == "rejected_branch");
        Assert.Contains(evidence.ArbiterVerdicts, v => v.Arbiter == "identity_consistency" && v.Verdict == "rejected_fragment_found");
        Assert.Contains("not by creating another account", evidence.FinalHumanResult);
    }

    [Fact]
    public void E53_Run_KeepsFinalResultStackIndependent()
    {
        var evidence = BuildEvidence();

        Assert.Equal(0, evidence.Metrics.ForbiddenStackTermCount);
        Assert.True(evidence.StackIndependent);
        Assert.DoesNotContain("C#", evidence.FinalHumanResult, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".NET", evidence.FinalHumanResult, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SQL", evidence.FinalHumanResult, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("AWS", evidence.FinalHumanResult, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void E53_Run_FinalHumanResultPassesChecklist()
    {
        var evidence = BuildEvidence();

        Assert.Equal("5/5", evidence.Metrics.HumanResultSectionsPresent);
        Assert.Contains("Can proceed:", evidence.FinalHumanResult);
        Assert.Contains("Blocked:", evidence.FinalHumanResult);
        Assert.Contains("Rejected:", evidence.FinalHumanResult);
        Assert.Contains("Unknown:", evidence.FinalHumanResult);
        Assert.Contains("Next actions:", evidence.FinalHumanResult);
        Assert.Contains(evidence.PassGates, gate => gate.Gate == "Result quality" && gate.Passed);
    }

    [Fact]
    public void E53_Run_ProducesLearningGapPacketsForMissingCoverage()
    {
        var evidence = BuildEvidence();

        Assert.Equal(4, evidence.Metrics.LearningGapPacketCount);
        Assert.Contains(evidence.LearningGapPackets, p => p.TargetStation == "knowledge_edge_detector" && p.TargetUnitRole == "identity_proofing_knowledge_edge");
        Assert.Contains(evidence.LearningGapPackets, p => p.TargetStation == "scheme_planner" && p.TargetUnitRole == "resume_state_boundary");
        Assert.Contains(evidence.LearningGapPackets, p => p.TargetStation == "scheme_planner" && p.TargetUnitRole == "policy_acceptance_boundary");
        Assert.Contains(evidence.LearningGapPackets, p => p.TargetStation == "arbiter_units" && p.TargetUnitRole == "identity_consistency");
    }

    [Fact]
    public void E53_Run_ModelsPolicyGapAsEarlyStartAndResolvedDependency()
    {
        var evidence = BuildEvidence();

        var policyPlan = Assert.Single(evidence.PlanItems, p => p.TaskId == "b08_prepare_policy_acceptance_gap");
        var activationPlan = Assert.Single(evidence.PlanItems, p => p.TaskId == "b09_gate_activation");

        Assert.Contains("early_start_boundary", policyPlan.StateModel);
        Assert.Contains("requires_b08_resolved", activationPlan.StateModel);
        Assert.Contains("b08_prepare_policy_acceptance_gap", activationPlan.DependsOn);
        Assert.Contains(evidence.DependencyTrace, d =>
            d.DependencyId == "dep-policy-before-activation" &&
            d.SourceState == "early_started_boundary" &&
            d.TargetRequiresSourceState == "resolved_evidence_or_confirmed_boundary");
    }

    [Fact]
    public void E53_Source_UsesHonestNamesForHandAuthoredRules()
    {
        var sourcePath = Path.Combine(
            FindRepositoryRoot(),
            "35.ExperimentsV2",
            "Experiments",
            "E53HighLevelOrchestrationProof.cs");
        var source = File.ReadAllText(sourcePath);

        var falseNameMatches = Regex.Matches(
            source,
            @"\b(?:trained|learned)\b",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        Assert.True(
            falseNameMatches.Count == 0,
            $"Expected zero whole-word trained/learned claims in {sourcePath}, but found {falseNameMatches.Count}: {string.Join(", ", falseNameMatches.Cast<Match>().Select(match => match.Value))}");

        var acceptedTrainingMatches = Regex.Matches(
            source,
            @"\baccepted[^\r\n]*\btraining\s+(?:pairs|set)\b",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        Assert.True(
            acceptedTrainingMatches.Count == 0,
            $"Expected zero accepted-training-pairs/set claims in {sourcePath}, but found {acceptedTrainingMatches.Count}: {string.Join(" | ", acceptedTrainingMatches.Cast<Match>().Select(match => match.Value))}");

        var handAuthoredNodeClasses = Regex.Matches(
            source,
            @"^\s*private static class Flow01Node\d{2}HandAuthored\w+\s*$",
            RegexOptions.Multiline | RegexOptions.CultureInvariant);
        Assert.True(
            handAuthoredNodeClasses.Count == 10,
            $"Expected exactly 10 hand-authored Flow-01 node classes in {sourcePath}, but found {handAuthoredNodeClasses.Count}.");

        var handAuthoredRuleRecords = Regex.Matches(
            source,
            @"^\s*private sealed record HandAuthored\w+Rule\(",
            RegexOptions.Multiline | RegexOptions.CultureInvariant);
        Assert.True(
            handAuthoredRuleRecords.Count == 9,
            $"Expected exactly 9 hand-authored private rule records in {sourcePath}, but found {handAuthoredRuleRecords.Count}.");

        var handAuthoredRuleTables = Regex.Matches(
            source,
            @"^\s*private static readonly HandAuthored\w+Rule\[\] HandAuthoredRules\s*=",
            RegexOptions.Multiline | RegexOptions.CultureInvariant);
        Assert.True(
            handAuthoredRuleTables.Count == 9,
            $"Expected exactly 9 hand-authored private rule tables in {sourcePath}, but found {handAuthoredRuleTables.Count}.");
    }

    private static E53HighLevelOrchestrationProof.Evidence BuildEvidence()
    {
        var experiment = new E53HighLevelOrchestrationProof();
        var evidence = experiment.BuildEvidence();
        Assert.True(evidence.IsSuccess, evidence.ErrorMessage);
        Assert.True(evidence.Value!.Metrics.E2EOrchestrationSuccess);
        return evidence.Value;
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var sourcePath = Path.Combine(
                current.FullName,
                "35.ExperimentsV2",
                "Experiments",
                "E53HighLevelOrchestrationProof.cs");
            if (File.Exists(sourcePath))
                return current.FullName;

            current = current.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not locate the repository root containing the E53 orchestration source.");
    }
}
