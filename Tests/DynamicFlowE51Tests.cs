// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using ExperimentsV2;
using LogicExtractor = SemanticCompiler.LogicExtractor;

namespace LlmFromScratch.Tests;

public sealed class DynamicFlowE51Tests
{
    private const int E51ATier = 2;

    [Fact]
    public void E51A_Run_Passes_CodeContextPipeline_Gates()
    {
        var experiment = new E51ACodeContextPipeline();
        var result = experiment.Run(tier: E51ATier);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(E51ACodeContextPipeline.Id, result.Value!.ExperimentId);
        Assert.True(result.Value.EnhancedPrimary >= 0.60, result.Value.Notes);
        Assert.True(result.Value.ScalingSignal >= 0.70, result.Value.Notes);
        Assert.True(result.Value.EnhancedPrimary > result.Value.BaselinePrimary, result.Value.Notes);
    }

    [Fact]
    public void E51A_AllExamples_Produce_NonEmpty_CodePredicates()
    {
        foreach (var example in E51ACodeContextPipeline.AllExamplesPublic())
        {
            var result = LogicExtractor.Extract(example.CodeContext);
            Assert.True(result.IsSuccess, $"{example.Id}: {result.ErrorMessage}");
            Assert.True(result.Value!.Count > 0, example.Id);
        }
    }

    [Fact]
    public void E51A_WriteEvidence_Creates_Json_File()
    {
        TestOwnedOutputRoot.Run(output =>
        {
            var experiment = new E51ACodeContextPipeline();
            var result = experiment.WriteEvidence(output.RootPath, tier: E51ATier);
            Assert.True(result.IsSuccess, result.ErrorMessage);
            var json = output.RequireOwnedPath(result.Value!);
            var md = output.RequireOwnedPath(Path.ChangeExtension(json, ".md"));
            Assert.True(File.Exists(json), json);
            Assert.True(File.Exists(md), md);
        });
    }

    [Fact]
    public void E51A_NamedFailures_Are_Empty_After_Tier1_Data_Fixes()
    {
        var experiment = new E51ACodeContextPipeline();
        var evidence = experiment.BuildEvidence(tier: E51ATier);
        Assert.True(evidence.IsSuccess, evidence.ErrorMessage);

        var failures = evidence.Value!.Traces
            .Where(t => !t.Passed)
            .Select(t => $"{t.Id}-{t.FailureReason}")
            .OrderBy(x => x)
            .ToArray();

        Assert.Empty(failures);
    }

    [Fact]
    public void E51A_CompoundDiagnosticRepairPrompt_RoutesToBugFix()
    {
        var experiment = new E51ACodeContextPipeline();
        var evidence = experiment.BuildEvidence(tier: E51ATier);
        Assert.True(evidence.IsSuccess, evidence.ErrorMessage);

        var trace = Assert.Single(evidence.Value!.Traces, t => t.Id == "V06");
        Assert.Equal("BUG_FIX", trace.PredictedDomain);
        Assert.True(trace.Passed, trace.FailureReason);
    }

    [Fact]
    public void E51A_RegexLoggingContext_Produces_Call_Return_And_Log_Signal()
    {
        var example = Assert.Single(E51ACodeContextPipeline.AllExamplesPublic(), e => e.Id == "V05");
        var predicates = LogicExtractor.Extract(example.CodeContext);
        Assert.True(predicates.IsSuccess, predicates.ErrorMessage);
        Assert.Contains(LogicExtractor.CALL, predicates.Value!);
        Assert.Contains(LogicExtractor.RETURN, predicates.Value!);
        Assert.Contains(LogicExtractor.LOG_OP, predicates.Value!);
    }

    [Fact]
    public void E51A_EncodeCallChainContext_Produces_Call_And_Return_Signal()
    {
        var example = Assert.Single(E51ACodeContextPipeline.AllExamplesPublic(), e => e.Id == "V08");
        var predicates = LogicExtractor.Extract(example.CodeContext);
        Assert.True(predicates.IsSuccess, predicates.ErrorMessage);
        Assert.Contains(LogicExtractor.CALL, predicates.Value!);
        Assert.Contains(LogicExtractor.RETURN, predicates.Value!);
    }

    [Fact]
    public void E51A_V07_ArchitectureTrace_Has_AtLeast_Two_Strong_Neighbors()
    {
        var experiment = new E51ACodeContextPipeline();
        var evidence = experiment.BuildEvidence(tier: E51ATier);
        Assert.True(evidence.IsSuccess, evidence.ErrorMessage);

        var trace = Assert.Single(evidence.Value!.Traces, t => t.Id == "V07");
        Assert.True(trace.NearestNeighbors.Count(n => n.Score >= 0.60) >= 2,
            string.Join(", ", trace.NearestNeighbors.Select(n => $"{n.Id}:{n.Score:F3}")));
    }

    [Fact]
    public void E51A_V03_V04_V09_Do_Not_Share_One_Identical_Loop_Neighborhood()
    {
        var experiment = new E51ACodeContextPipeline();
        var evidence = experiment.BuildEvidence(tier: E51ATier);
        Assert.True(evidence.IsSuccess, evidence.ErrorMessage);

        var signatures = evidence.Value!.Traces
            .Where(t => t.Id is "V03" or "V04" or "V09")
            .Select(t => string.Join(",", t.NearestNeighbors.Select(n => n.Id)))
            .Distinct()
            .ToArray();

        Assert.True(signatures.Length > 1, string.Join(" | ", signatures));
    }

    [Fact]
    public void E51A_V10_IEquatableTrace_Has_Full_Implementation_Overlap()
    {
        var experiment = new E51ACodeContextPipeline();
        var evidence = experiment.BuildEvidence(tier: E51ATier);
        Assert.True(evidence.IsSuccess, evidence.ErrorMessage);

        var trace = Assert.Single(evidence.Value!.Traces, t => t.Id == "V10");
        Assert.Equal(1.0, trace.PredicateOverlap, precision: 6);
        Assert.Contains(trace.ProducedPredicates, p => p is "RETURN" or "DECLARE");
    }

    [Fact]
    public void E51A_WrongDomainScopedRetrieval_Does_Not_Look_Confident()
    {
        var experiment = new E51ACodeContextPipeline();
        const string ambiguousPrompt = "explain why the data loader sometimes returns empty results";
        const string loaderFailureContext = """
namespace DatasetDemo { public sealed class DataLoader {
private readonly IDataStore _store;
private readonly ILogger _logger;
public OperationResult<string> Load(string id) {
var row = _store.Get(id);
if (row == null) return OperationResult<string>.Fail("empty result");
_logger.LogDebug("loaded row");
return OperationResult<string>.Ok(row.Body);
}} public interface IDataStore { DataRow? Get(string id); } public sealed class DataRow { public string Body { get; set; } = ""; } }
""";

        var neighbors = experiment.ProbeDomainScopedNeighborsPublic(
            ambiguousPrompt,
            loaderFailureContext,
            DomainClassifier.ARCHITECTURE,
            k: 5);

        Assert.True(neighbors.IsSuccess, neighbors.ErrorMessage);
        Assert.NotEmpty(neighbors.Value!);
        Assert.All(neighbors.Value!, n =>
            Assert.True(n.Score < 0.50, string.Join(", ", neighbors.Value!.Select(x => $"{x.Id}:{x.Score:F3}"))));
    }

    [Fact]
    public void E51A_Tier2Expansion_Has_CoverageAcross_Domains()
    {
        var examples = E51ACodeContextPipeline.AllExamplesPublic();
        var train = examples.Where(e => e.Id.StartsWith("T", StringComparison.Ordinal)).ToArray();
        var test = examples.Where(e => e.Id.StartsWith("V", StringComparison.Ordinal)).ToArray();
        var counts = train
            .GroupBy(e => e.ExpectedDomain)
            .ToDictionary(g => DomainClassifier.DomainNames[g.Key], g => g.Count());

        Assert.True(train.Length >= 25, $"train={train.Length}");
        Assert.True(test.Length >= 13, $"test={test.Length}");
        Assert.True(counts["ARCHITECTURE"] >= 5, $"ARCHITECTURE={counts["ARCHITECTURE"]}");
        Assert.True(counts["ORCHESTRATION"] >= 4, $"ORCHESTRATION={counts["ORCHESTRATION"]}");
        Assert.True(counts["BUG_FIX"] >= 5, $"BUG_FIX={counts["BUG_FIX"]}");
        Assert.True(counts["CODE_QUESTION"] >= 5, $"CODE_QUESTION={counts["CODE_QUESTION"]}");
        Assert.True(counts["CODE_CHANGE"] >= 5, $"CODE_CHANGE={counts["CODE_CHANGE"]}");
    }

    [Fact]
    public void E51A_Tier2Evidence_Names_SourceFamilies_And_CoverageFindings()
    {
        var experiment = new E51ACodeContextPipeline();
        var evidence = experiment.BuildEvidence(tier: E51ATier);
        Assert.True(evidence.IsSuccess, evidence.ErrorMessage);

        var byId = evidence.Value!.Traces.ToDictionary(t => t.Id);
        Assert.Equal("02.DatasetPipeline", byId["V11"].SourceFamily);
        Assert.Equal("05.Embeddings", byId["V12"].SourceFamily);
        Assert.Equal("05.Embeddings", byId["V13"].SourceFamily);
        Assert.Contains(evidence.Value.RetrievalProbes, p => p.Id == "WD-01" && p.SourceFamily == "02.DatasetPipeline");
        Assert.Contains(evidence.Value.CoverageFindings, f => f.Contains("too-thin IBatchSource", StringComparison.Ordinal));
        Assert.Contains(evidence.Value.CoverageFindings, f => f.Contains("orchestration prompt", StringComparison.Ordinal));
    }

    [Fact]
    public void E51B_Run_Passes_DynamicFlowMiniE2E_Gates()
    {
        var experiment = new E51BDynamicFlowMiniE2E();
        var result = experiment.Run();
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(E51BDynamicFlowMiniE2E.Id, result.Value!.ExperimentId);
        Assert.Equal(1.0, result.Value.EnhancedPrimary, precision: 6);
        Assert.False(result.Value.ShouldScaleUp, result.Value.Notes);
    }

    [Fact]
    public void E51B_TeachingSet_Has_Expected_Count()
    {
        Assert.Equal(12, E51BDynamicFlowMiniE2E.TeachingRecordsPublic().Count);
    }

    [Fact]
    public void E51B_HeldOut_Arbiter_GroundTruth_Is_Specified()
    {
        var traces = E51BDynamicFlowMiniE2E.ExpectedHeldOutTracesPublic();
        Assert.Equal(4, traces.Count);
        Assert.Equal(["identity_consistency", "token_safety"], traces["TE-01"].ExpectedArbiters);
        Assert.Equal(["verification_order", "token_safety"], traces["TE-02"].ExpectedArbiters);
        Assert.Equal(["identity_consistency"], traces["TE-03"].ExpectedArbiters);
        Assert.Equal(["token_safety"], traces["TE-04"].ExpectedArbiters);
    }

    [Fact]
    public void E51B_TeachingAndRuntimeRecords_Use_TheSameCompilerShape()
    {
        var experiment = new E51BDynamicFlowMiniE2E();
        var inspection = experiment.InspectTeachingRuntimeSymmetryPublic("TR-01", "TE-01");
        Assert.True(inspection.IsSuccess, inspection.ErrorMessage);

        Assert.Equal(22, inspection.Value!.PredicateVocabularySize);
        Assert.Equal(60, inspection.Value.Teaching.VectorWidth);
        Assert.Equal(60, inspection.Value.Runtime.VectorWidth);
        Assert.Contains("E51A-CODE-CONTEXT-PIPELINE Tier 2", inspection.Value.CompilerValidationReference);
        Assert.True(inspection.Value.PredicateOverlap >= 0.50,
            $"{string.Join(",", inspection.Value.Teaching.Predicates)} vs {string.Join(",", inspection.Value.Runtime.Predicates)}");
    }

    [Fact]
    public void E51B_Evidence_Contains_Knowledge_Edge_And_Arbiter_Rejection()
    {
        var experiment = new E51BDynamicFlowMiniE2E();
        var evidence = experiment.BuildEvidence();
        Assert.True(evidence.IsSuccess, evidence.ErrorMessage);
        Assert.Contains(evidence.Value!.Traces, t => t.Id == "CH-01" && t.KnowledgeEdgeDetected && t.ArbiterVerdict == "learning_gap_packet_created");
        Assert.Contains(evidence.Value.Traces, t => t.Id == "CH-02" && t.ArbiterVerdict == "rejected" && t.Passed);
    }

    [Fact]
    public void E51B_HeldOut_PredictedSubjects_Match_Each_Record()
    {
        var experiment = new E51BDynamicFlowMiniE2E();
        var evidence = experiment.BuildEvidence();
        Assert.True(evidence.IsSuccess, evidence.ErrorMessage);

        var byId = evidence.Value!.Traces.ToDictionary(t => t.Id);
        Assert.Equal("AUTH_PROVIDER_ENTRY", byId["TE-01"].PredictedLocalSubject);
        Assert.Equal("EMAIL_VERIFICATION", byId["TE-02"].PredictedLocalSubject);
        Assert.Equal("IDENTITY_DUPLICATE_PREVENTION", byId["TE-03"].PredictedLocalSubject);
        Assert.Equal("TOKEN_SESSION_SAFETY", byId["TE-04"].PredictedLocalSubject);
    }

    [Fact]
    public void E51B_KnowledgeEdge_ContextPack_Names_Missing_Competence()
    {
        var experiment = new E51BDynamicFlowMiniE2E();
        var evidence = experiment.BuildEvidence();
        Assert.True(evidence.IsSuccess, evidence.ErrorMessage);

        var challenge = Assert.Single(evidence.Value!.Traces, t => t.Id == "CH-01");
        Assert.True(challenge.KnowledgeEdgeDetected);
        Assert.Contains("provider sign-in", challenge.ExternalHelpContextPack);
        Assert.Contains("email verification", challenge.ExternalHelpContextPack);
        Assert.Contains("duplicate prevention", challenge.ExternalHelpContextPack);
        Assert.Contains("token safety", challenge.ExternalHelpContextPack);
        Assert.Contains("does not yet know how biometric passport verification changes the registration flow", challenge.ExternalHelpContextPack);
    }

    [Fact]
    public void E51B_KnowledgeEdge_Neighbors_Are_Present_But_LowConfidence()
    {
        var experiment = new E51BDynamicFlowMiniE2E();
        var evidence = experiment.BuildEvidence();
        Assert.True(evidence.IsSuccess, evidence.ErrorMessage);

        Assert.Contains("E51A-CODE-CONTEXT-PIPELINE Tier 2", evidence.Value!.CompilerValidationReference);
        Assert.Equal(22, evidence.Value.PredicateVocabularySize);
        Assert.Contains("60 slots", evidence.Value.FeatureVectorContract);

        var challenge = Assert.Single(evidence.Value.Traces, t => t.Id == "CH-01");
        Assert.NotEmpty(challenge.NearestNeighbors);
        Assert.All(challenge.NearestNeighbors, n =>
            Assert.True(n.Score is > 0 and < 0.50,
                string.Join(", ", challenge.NearestNeighbors.Select(x => $"{x.Id}:{x.Score:F3}"))));
    }

    [Fact]
    public void E51B_Arbiter_Rejection_Overrides_High_Knn_Similarity()
    {
        var experiment = new E51BDynamicFlowMiniE2E();
        var evidence = experiment.BuildEvidence();
        Assert.True(evidence.IsSuccess, evidence.ErrorMessage);

        var challenge = Assert.Single(evidence.Value!.Traces, t => t.Id == "CH-02");
        Assert.True(challenge.NearestNeighbors[0].Score >= 0.70, $"Nearest score was {challenge.NearestNeighbors[0].Score:F3}");
        Assert.Equal("rejected", challenge.ArbiterVerdict);
        Assert.True(challenge.Passed);
        Assert.Contains("must not create duplicate accounts", challenge.FinalResult);
    }

    [Fact]
    public void E51B_WriteEvidence_Creates_Json_File()
    {
        TestOwnedOutputRoot.Run(output =>
        {
            var experiment = new E51BDynamicFlowMiniE2E();
            var result = experiment.WriteEvidence(output.RootPath);
            Assert.True(result.IsSuccess, result.ErrorMessage);
            var json = output.RequireOwnedPath(result.Value!);
            var md = output.RequireOwnedPath(Path.ChangeExtension(json, ".md"));
            Assert.True(File.Exists(json), json);
            Assert.True(File.Exists(md), md);
        });
    }

    [Fact]
    public void E52_Run_Shows_Project2123Cycle_ImprovesOverBaseline()
    {
        var experiment = new E52Projects2123CycleComparison();
        var evidence = experiment.BuildEvidence();
        Assert.True(evidence.IsSuccess, evidence.ErrorMessage);

        Assert.True(evidence.Value!.EnhancedCycleSuccessRate > evidence.Value.BaselineCycleSuccessRate);
        Assert.Equal(1.0, evidence.Value.EnhancedCycleSuccessRate, precision: 6);
        Assert.Contains(evidence.Value.Traces, t => t.Id == "V06" && !t.BaselinePassed && t.EnhancedPassed);
        Assert.Contains(evidence.Value.Traces, t => t.Id == "CH-01" && t.EnhancedBehavior.Contains("requires approval", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(evidence.Value.Traces, t => t.Id == "CH-02" && t.EnhancedBehavior.Contains("external_answer_rejected_by_arbiter", StringComparison.Ordinal));
    }

    [Fact]
    public void E52_WriteEvidence_Creates_Json_File()
    {
        TestOwnedOutputRoot.Run(output =>
        {
            var experiment = new E52Projects2123CycleComparison();
            var result = experiment.WriteEvidence(output.RootPath);
            Assert.True(result.IsSuccess, result.ErrorMessage);
            var json = output.RequireOwnedPath(result.Value!);
            var md = output.RequireOwnedPath(Path.ChangeExtension(json, ".md"));
            Assert.True(File.Exists(json), json);
            Assert.True(File.Exists(md), md);
        });
    }
}
