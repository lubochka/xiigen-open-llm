// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using DPO.Models;
using ModelServer.Contracts;
using ProjectWikiDomain;
using TrainableRouter.Training;

public sealed class CrossStackFlowSeedHonestyTests : IDisposable
{
    private readonly string _artifactRoot = Path.Combine(
        Path.GetTempPath(),
        $"csf-{Guid.NewGuid():N}");

    public void Dispose()
    {
        if (Directory.Exists(_artifactRoot))
            Directory.Delete(_artifactRoot, recursive: true);
    }

    [Fact]
    public async Task SeedBatch_LabelsDeterministicSeedAndNeverPaidExternalModelSide()
    {
        var batch = await BuildBatchAsync("cross-stack-seed-label");

        Assert.Equal(DpoBatchSourceKind.DeterministicSeed, batch.SourceKind);
        Assert.Equal(DpoBatchSplitRole.Training, batch.SplitRole);
        Assert.False(batch.IndependentlyEvaluated);
        Assert.Null(batch.IndependentValidation);
        Assert.Equal(50, batch.RowCount);
        Assert.All(batch.Pairs, pair =>
        {
            Assert.DoesNotContain("PaidExternalModel", pair.A, StringComparison.Ordinal);
            Assert.DoesNotContain("PaidExternalModel", pair.Chosen, StringComparison.Ordinal);
            Assert.DoesNotContain("PaidExternalModel", pair.Rejected, StringComparison.Ordinal);
        });
    }

    [Fact]
    public async Task Validator_OwnsWrongAnnotationRateAndMatrixAcceptance()
    {
        var generated = await BuildBatchAsync("cross-stack-validator-owns-rate");
        var selfScoredBatch = generated with { WrongAnnotationRate = 0 };

        var validator = new CrossStackFlowPairBatchValidator();
        var validation = validator.Validate(selfScoredBatch, heldOut: false);

        Assert.True(validation.IsSuccess, validation.ErrorMessage);
        Assert.True(validation.Value!.Accepted);
        Assert.True(validation.Value.MatrixComplete);
        Assert.Equal(DpoBatchSplitRole.Training, validation.Value.SplitRole);
        Assert.Equal(50, validation.Value.ExpectedRowCount);
        Assert.Equal(50, validation.Value.ActualRowCount);
        Assert.Equal(1.0, validation.Value.RecomputedWrongAnnotationRate);
        Assert.Equal(0, selfScoredBatch.WrongAnnotationRate);
        Assert.NotEmpty(validation.Value.RowFingerprintSha256);
        Assert.NotEmpty(validation.Value.SourceProvenanceFingerprintSha256);
        Assert.NotEmpty(validation.Value.ValidationFingerprintSha256);
        Assert.Empty(validation.Value.ErrorCode);

        var unknownSource = validator.Evaluate(
            selfScoredBatch with { SourceKind = DpoBatchSourceKind.Unknown },
            heldOut: false);
        Assert.False(unknownSource.Accepted);
        Assert.Equal("CROSS_STACK_BATCH_SOURCE_UNKNOWN", unknownSource.ErrorCode);

        var duplicateBatch = selfScoredBatch with
        {
            Pairs = selfScoredBatch.Pairs.Take(49).Append(selfScoredBatch.Pairs[0]).ToArray()
        };
        var duplicate = validator.Evaluate(duplicateBatch, heldOut: false);
        Assert.False(duplicate.Accepted);
        Assert.NotEmpty(duplicate.DuplicateMatrixCells);

        var unknownTypeBatch = selfScoredBatch with
        {
            Pairs = selfScoredBatch.Pairs
                .Select((pair, index) => index == 0 ? pair with { RejectedType = "semantic_magic" } : pair)
                .ToArray()
        };
        var unknownType = validator.Evaluate(unknownTypeBatch, heldOut: false);
        Assert.False(unknownType.Accepted);
        Assert.Contains("semantic_magic", unknownType.UnknownRejectedTypes);
    }

    [Fact]
    public async Task Generator_ExpandsTypedMatrixWithoutFiftyRowTruncation()
    {
        var expectedTypes = new[]
        {
            "ui_api_only",
            "server_sql_only",
            "procedure_without_result_shape",
            "separate_repositories",
            "incomplete_cross_stack_business_flow"
        };
        var expectedDomains = new[]
        {
            "campaign",
            "client",
            "diet",
            "media",
            "task",
            "game",
            "profile",
            "report",
            "notification",
            "booking",
            "workflow"
        };
        var batch = await BuildBatchForDomainsAsync("cross-stack-expanded-matrix", expectedDomains);

        // Eleven domains are essential: ten domains times five rejected types is
        // exactly fifty rows, so a hidden output Take(50) passed the old test.
        Assert.Equal(55, batch.RowCount);
        Assert.Equal(55, batch.Pairs.Length);
        Assert.Equal(DpoBatchSourceKind.DeterministicSeed, batch.SourceKind);
        Assert.Equal(DpoBatchSplitRole.Training, batch.SplitRole);
        Assert.Contains("batch_row_count_not_50", batch.Warnings);
        var cells = batch.Pairs
            .Select(pair => $"{ResolveDomain(pair)}|{pair.RejectedType}")
            .ToArray();
        var duplicateCells = cells
            .GroupBy(cell => cell, StringComparer.Ordinal)
            .Where(group => group.Count() != 1)
            .Select(group => group.Key)
            .ToArray();
        var expectedCells = expectedDomains
            .SelectMany(domain => expectedTypes.Select(type => $"{domain}|{type}"))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Empty(duplicateCells);
        Assert.Equal(expectedCells.Length, cells.Length);
        Assert.Equal(expectedCells, cells.Order(StringComparer.Ordinal).ToArray());
        Assert.All(expectedCells, expectedCell =>
            Assert.Single(cells, actual => string.Equals(actual, expectedCell, StringComparison.Ordinal)));
        Assert.All(batch.Pairs, pair =>
            Assert.EndsWith(Slug(pair.RejectedType), pair.BoundaryMap.SystemId, StringComparison.Ordinal));
    }

    [Fact]
    public async Task Generator_TestDomainSeamRejectsInvalidDomainPrograms()
    {
        await AssertDomainProgramRejectedAsync(null, "CROSS_STACK_SEED_DOMAIN_PROGRAM_REQUIRED");
        await AssertDomainProgramRejectedAsync([], "CROSS_STACK_SEED_DOMAIN_PROGRAM_EMPTY");
        await AssertDomainProgramRejectedAsync(["campaign", " "], "CROSS_STACK_SEED_DOMAIN_PROGRAM_BLANK");
        await AssertDomainProgramRejectedAsync(["campaign", "Campaign"], "CROSS_STACK_SEED_DOMAIN_PROGRAM_DUPLICATE");
    }

    [Fact]
    public async Task Pipeline_RefusesSemanticPublicationForDeterministicSeed()
    {
        var result = await RunSeedPipelineAsync(_artifactRoot, "csf");

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Null(result.Value!.Publication);
        Assert.False(result.Value.SemanticPublisherInvoked);
        Assert.Equal(DpoBatchSourceKind.DeterministicSeed, result.Value.TrainingBatch.SourceKind);
        Assert.True(result.Value.TrainingBatch.IndependentlyEvaluated);
        Assert.True(result.Value.TrainingValidation!.Accepted);
        Assert.True(result.Value.HeldOutValidation!.Accepted);
        Assert.Equal(
            CrossStackBusinessFlowGraphArtifactDisposition.BaselineOnlyNotPromotable,
            result.Value.DpoMetrics!.CrossStackBaselineDisposition!.Disposition);
        Assert.Contains(
            "cross_stack_business_flow_graph_deterministic_seed_baseline_no_semantic_publication",
            result.Value.Warnings);
        AssertBaselineArtifactsAreExplicitlyNonPromotable(_artifactRoot);
        AssertNoCrossStackPromotionArtifacts(_artifactRoot);
    }

    [Fact]
    public async Task Pipeline_WritesBaselineArtifactsAtomicallyWithoutTempFilesAndExactAllowlist()
    {
        var result = await RunSeedPipelineAsync(_artifactRoot, "csf-atomic");

        Assert.True(result.IsSuccess, result.ErrorMessage);
        AssertBaselineArtifactsAreExplicitlyNonPromotable(_artifactRoot);
        AssertBaselineAllowlistsMatchWrittenArtifacts(_artifactRoot);
        AssertNoAtomicTempFiles(_artifactRoot);
    }

    [Fact]
    public async Task Publisher_RejectsSeedBeforeTrustedFileUploadOrRegister()
    {
        var trustedRoot = Path.Combine(_artifactRoot, "trusted-publication");
        var repository = new CountingModelRepository();
        var locator = new CountingRemoteUnitLocator();
        var training = await BuildBatchAsync("cross-stack-publisher-seed-refusal");
        var heldOut = await BuildBatchAsync("cross-stack-publisher-seed-refusal-heldout", heldOut: true);
        var validator = new CrossStackFlowPairBatchValidator();
        var trainValidation = validator.Validate(training, heldOut: false).Value!;
        var heldOutValidation = validator.Validate(heldOut, heldOut: true).Value!;
        var trainingResult = PassingTrainingRun(training, heldOut, trainValidation, heldOutValidation);
        var dpoMetrics = PassingDpoMetrics(training, heldOut, trainValidation, heldOutValidation);
        var authority = CreateReflectedAuthority(
            DpoBatchSourceKind.DeterministicSeed,
            training,
            heldOut,
            trainValidation,
            heldOutValidation,
            trainingResult,
            dpoMetrics);

        var result = await InvokeInternalPublishWithAuthorityAsync(
            new CrossStackBusinessFlowGraphManifestPublisher(repository, locator),
            trainingResult,
            dpoMetrics,
            training,
            heldOut,
            authority,
            trustedRoot);

        Assert.False(result.IsSuccess);
        Assert.Equal("CROSS_STACK_GRAPH_PUBLICATION_SOURCE_NOT_AUTHORIZED", result.ErrorCode);
        Assert.False(Directory.Exists(trustedRoot));
        Assert.Equal(0, repository.UploadCount);
        Assert.Equal(0, locator.RegisterCount);
    }

    [Fact]
    public async Task Publisher_PublicPathRequiresOpaqueAuthorityBeforeSideEffects()
    {
        var trustedRoot = Path.Combine(_artifactRoot, "trusted-public-api");
        var repository = new CountingModelRepository();
        var locator = new CountingRemoteUnitLocator();
        var training = await BuildBatchAsync("cross-stack-public-api");
        var heldOut = await BuildBatchAsync("cross-stack-public-api-heldout", heldOut: true);
        var validator = new CrossStackFlowPairBatchValidator();
        var trainValidation = validator.Validate(training, heldOut: false).Value!;
        var heldOutValidation = validator.Validate(heldOut, heldOut: true).Value!;

        var result = await new CrossStackBusinessFlowGraphManifestPublisher(repository, locator)
            .PublishAsync(
                PassingTrainingRun(training, heldOut, trainValidation, heldOutValidation),
                PassingDpoMetrics(training, heldOut, trainValidation, heldOutValidation),
                training,
                heldOut,
                trustedRoot);

        Assert.False(result.IsSuccess);
        Assert.Equal("CROSS_STACK_GRAPH_PUBLICATION_AUTHORITY_REQUIRED", result.ErrorCode);
        Assert.False(Directory.Exists(trustedRoot));
        Assert.Equal(0, repository.UploadCount);
        Assert.Equal(0, locator.RegisterCount);
    }

    [Fact]
    public async Task Publisher_InternalPathRejectsNullAuthorityBeforeTrustedFileUploadOrRegister()
    {
        var trustedRoot = Path.Combine(_artifactRoot, "trusted-null-authority");
        var repository = new CountingModelRepository();
        var locator = new CountingRemoteUnitLocator();
        var training = await BuildBatchAsync("cross-stack-null-authority");
        var heldOut = await BuildBatchAsync("cross-stack-null-authority-heldout", heldOut: true);
        var validator = new CrossStackFlowPairBatchValidator();
        var trainValidation = validator.Validate(training, heldOut: false).Value!;
        var heldOutValidation = validator.Validate(heldOut, heldOut: true).Value!;

        var result = await InvokeInternalPublishWithAuthorityAsync(
            new CrossStackBusinessFlowGraphManifestPublisher(repository, locator),
            PassingTrainingRun(training, heldOut, trainValidation, heldOutValidation),
            PassingDpoMetrics(training, heldOut, trainValidation, heldOutValidation),
            training,
            heldOut,
            authority: null,
            trustedRoot);

        Assert.False(result.IsSuccess);
        Assert.Equal("CROSS_STACK_GRAPH_PUBLICATION_AUTHORITY_REQUIRED", result.ErrorCode);
        Assert.False(Directory.Exists(trustedRoot));
        Assert.Equal(0, repository.UploadCount);
        Assert.Equal(0, locator.RegisterCount);
    }

    [Fact]
    public void OrchestrationPromptDpoPairBuilder_CarriesLocalDeterministicSeedLabel()
    {
        var scenarios = OrchestrationRoutingCurriculum.CreatePromptFileAcceptanceCases();

        var pairs = new OrchestrationPromptDpoPairBuilder()
            .BuildPromptFilePairs(scenarios);

        Assert.True(pairs.IsSuccess, pairs.ErrorMessage);
        Assert.All(pairs.Value!, pair =>
            Assert.Equal(OrchestrationPromptDpoPairSourceKind.DeterministicSeed, pair.SourceKind));
        Assert.NotEqual(
            typeof(DpoBatchSourceKind).FullName,
            typeof(OrchestrationPromptDpoPairSourceKind).FullName);
    }

    [Fact]
    public async Task Validator_RejectsSourceRelabelWithoutMatchingProvenance()
    {
        var seed = await BuildBatchAsync("cross-stack-relabel-source");
        var relabeled = seed with { SourceKind = DpoBatchSourceKind.RealRepositoryCorpus };

        var validation = new CrossStackFlowPairBatchValidator()
            .Evaluate(relabeled, heldOut: false);

        Assert.False(validation.Accepted);
        Assert.Equal("CROSS_STACK_BATCH_PROVENANCE_MISMATCH", validation.ErrorCode);
    }

    [Fact]
    public async Task Validator_RejectsCopiedSeedWhenBatchIdNoLongerMatchesProof()
    {
        var seed = await BuildBatchAsync("cross-stack-proof-copy");
        var copied = seed with { BatchId = "cross-stack-proof-copy-renamed" };

        var validation = new CrossStackFlowPairBatchValidator()
            .Evaluate(copied, heldOut: false);

        Assert.False(validation.Accepted);
        Assert.Equal("CROSS_STACK_BATCH_PROVENANCE_MISMATCH", validation.ErrorCode);
    }

    [Fact]
    public async Task Validator_IgnoresForgedAcceptedResultAndChangedRowsFail()
    {
        var seed = await BuildBatchAsync("cross-stack-forged-accepted");
        var valid = new CrossStackFlowPairBatchValidator()
            .Validate(seed, heldOut: false)
            .Value!;
        var forged = seed with
        {
            RowCount = 49,
            Pairs = seed.Pairs.Skip(1).ToArray(),
            IndependentValidation = valid with { Accepted = true, ErrorCode = "" }
        };

        var validation = new CrossStackFlowPairBatchValidator()
            .Evaluate(forged, heldOut: false);

        Assert.False(validation.Accepted);
        Assert.Equal("CROSS_STACK_BATCH_PROVENANCE_MISMATCH", validation.ErrorCode);
        Assert.NotEqual(valid.RowFingerprintSha256, validation.RowFingerprintSha256);
    }

    [Fact]
    public async Task Validator_RejectsTrainingBatchReplayedAsHeldOut()
    {
        var training = await BuildBatchAsync("cross-stack-train-proof-replayed");
        var trainValidation = new CrossStackFlowPairBatchValidator()
            .Validate(training, heldOut: false)
            .Value!;
        var replayed = training with { IndependentValidation = trainValidation };

        var heldOutValidation = new CrossStackFlowPairBatchValidator()
            .Evaluate(replayed, heldOut: true);

        Assert.False(heldOutValidation.Accepted);
        Assert.Equal("CROSS_STACK_BATCH_SPLIT_ROLE_MISMATCH", heldOutValidation.ErrorCode);
    }

    [Fact]
    public async Task Publisher_RejectsAuthorityBatchReplayBeforeTrustedFileUploadOrRegister()
    {
        var trustedRoot = Path.Combine(_artifactRoot, "trusted-replay");
        var repository = new CountingModelRepository();
        var locator = new CountingRemoteUnitLocator();
        var validator = new CrossStackFlowPairBatchValidator();
        var trainingA = await BuildBatchAsync("cross-stack-proof-a");
        var heldOutA = await BuildBatchAsync("cross-stack-proof-a-heldout", heldOut: true);
        var trainValidationA = validator.Validate(trainingA, heldOut: false).Value!;
        var heldOutValidationA = validator.Validate(heldOutA, heldOut: true).Value!;
        var authorityA = CreateReflectedAuthority(
            DpoBatchSourceKind.DeterministicSeed,
            trainingA,
            heldOutA,
            trainValidationA,
            heldOutValidationA,
            PassingTrainingRun(trainingA, heldOutA, trainValidationA, heldOutValidationA),
            PassingDpoMetrics(trainingA, heldOutA, trainValidationA, heldOutValidationA));
        var trainingB = await BuildBatchAsync("cross-stack-proof-b");
        var heldOutB = await BuildBatchAsync("cross-stack-proof-b-heldout", heldOut: true);
        var trainValidationB = validator.Validate(trainingB, heldOut: false).Value!;
        var heldOutValidationB = validator.Validate(heldOutB, heldOut: true).Value!;

        var result = await InvokeInternalPublishWithAuthorityAsync(
            new CrossStackBusinessFlowGraphManifestPublisher(repository, locator),
            PassingTrainingRun(trainingB, heldOutB, trainValidationB, heldOutValidationB),
            PassingDpoMetrics(trainingB, heldOutB, trainValidationB, heldOutValidationB),
            trainingB,
            heldOutB,
            authorityA,
            trustedRoot);

        Assert.False(result.IsSuccess);
        Assert.Equal("CROSS_STACK_GRAPH_PUBLICATION_AUTHORITY_REPLAY_OR_MISMATCH", result.ErrorCode);
        Assert.False(Directory.Exists(trustedRoot));
        Assert.Equal(0, repository.UploadCount);
        Assert.Equal(0, locator.RegisterCount);
    }

    [Fact]
    public void PublicationAuthority_PublicApiCannotConstructOrMintAuthority()
    {
        var authorityType = AuthorityType();

        Assert.False(authorityType.IsPublic);
        Assert.Empty(authorityType.GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        Assert.Empty(authorityType.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
        Assert.NotEmpty(authorityType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance));
        Assert.DoesNotContain(
            typeof(CrossStackFlowPairBatchValidator)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly),
            MethodMentionsPublicationAuthority);
        Assert.DoesNotContain(
            typeof(CrossStackBusinessFlowGraphManifestPublisher)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .SelectMany(method => method.GetParameters()),
            parameter => TypeMentionsPublicationAuthority(parameter.ParameterType));
    }

    [Fact]
    public async Task NoLiveSemanticAuthorityMinter_IsExposedForCurrentCrossStackSources()
    {
        var validator = new CrossStackFlowPairBatchValidator();
        var generated = await BuildBatchAsync("cross-stack-no-live-minter");
        var heldOut = await BuildBatchAsync("cross-stack-no-live-minter-heldout", heldOut: true);
        var trainValidation = validator.Validate(generated, heldOut: false).Value!;
        var heldOutValidation = validator.Validate(heldOut, heldOut: true).Value!;
        var authorityAttempt = typeof(CrossStackFlowPairBatchValidator)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Single(method => method.Name == "CreatePublicationAuthority");
        var attempt = authorityAttempt.Invoke(
            validator,
            [
                generated,
                heldOut,
                PassingTrainingRun(generated, heldOut, trainValidation, heldOutValidation),
                PassingDpoMetrics(generated, heldOut, trainValidation, heldOutValidation)
            ])!;

        Assert.Equal(DpoBatchSourceKind.DeterministicSeed, generated.SourceKind);
        Assert.False(authorityAttempt.IsPublic);
        AssertOperationFailed(attempt, "CROSS_STACK_GRAPH_PUBLICATION_SOURCE_NOT_AUTHORIZED");
    }

    private static async Task<CrossStackBusinessFlowGraphDpoBatch> BuildBatchAsync(
        string batchId,
        bool heldOut = false)
    {
        var batch = await new CrossStackBusinessFlowGraphDpoGenerator()
            .BuildBatchAsync(batchId, heldOut);
        Assert.True(batch.IsSuccess, batch.ErrorMessage);
        return batch.Value!;
    }

    private static async Task<CrossStackBusinessFlowGraphDpoBatch> BuildBatchForDomainsAsync(
        string batchId,
        string[] domains)
    {
        var batch = await InvokeBuildBatchForDomainsAsync(batchId, domains);
        Assert.True(batch.IsSuccess, batch.ErrorMessage);
        return batch.Value!;
    }

    private static async Task AssertDomainProgramRejectedAsync(
        string[]? domains,
        string expectedErrorCode)
    {
        var batch = await InvokeBuildBatchForDomainsAsync(
            $"cross-stack-invalid-domain-program-{expectedErrorCode}",
            domains);

        Assert.False(batch.IsSuccess);
        Assert.Equal(expectedErrorCode, batch.ErrorCode);
    }

    private static async Task<OperationResult<CrossStackBusinessFlowGraphDpoBatch>>
        InvokeBuildBatchForDomainsAsync(
            string batchId,
            string[]? domains)
    {
        var method = typeof(CrossStackBusinessFlowGraphDpoGenerator).GetMethod(
            "BuildBatchForDomainsAsync",
            BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(method);

        var task = Assert.IsAssignableFrom<Task<OperationResult<CrossStackBusinessFlowGraphDpoBatch>>>(
            method.Invoke(
                new CrossStackBusinessFlowGraphDpoGenerator(),
                [batchId, domains, DpoBatchSplitRole.Training, CancellationToken.None]));
        return await task;
    }

    private static async Task<OperationResult<CrossStackBusinessFlowGraphTrainingPipelineResult>>
        RunSeedPipelineAsync(string artifactRoot, string batchId)
    {
        var dpoMetricRunner = new ProjectWikiDpoMetricRunner(
            CreateProjectWikiLegalTeacherAdmissionAuthority(),
            epochCount: 1,
            learningRate: 5e-2f,
            vocabSize: 128,
            contextLen: 32,
            hiddenDim: 16,
            numLayers: 1,
            numHeads: 4,
            numKvHeads: 1,
            ffnHiddenDim: 32,
            deterministicFreshInitialization: true);

        return await new CrossStackBusinessFlowGraphTrainingPipeline(
                dpoMetricRunner: dpoMetricRunner)
            .RunOneBatchAsync(artifactRoot, batchId);
    }

    private static void AssertBaselineArtifactsAreExplicitlyNonPromotable(string artifactRoot)
    {
        var baselineFiles = Directory
            .EnumerateFiles(artifactRoot, "baseline-disposition.json", SearchOption.AllDirectories)
            .ToArray();
        Assert.True(baselineFiles.Length >= 2, $"Expected batch and DPO metric baseline dispositions under {artifactRoot}.");
        Assert.All(baselineFiles, path =>
        {
            var text = File.ReadAllText(path);
            Assert.Contains("BaselineOnlyNotPromotable", text, StringComparison.Ordinal);
            Assert.Contains("DeterministicSeed", text, StringComparison.Ordinal);
            Assert.Contains("cross-stack-business-flow-baseline-allowlist/v1", text, StringComparison.Ordinal);
            Assert.Contains("generated_capability_manifest.json", text, StringComparison.Ordinal);
            var disposition = JsonSerializer.Deserialize<CrossStackBusinessFlowGraphBaselineDisposition>(
                text,
                new JsonSerializerOptions(JsonSerializerDefaults.Web)
                {
                    Converters = { new JsonStringEnumConverter() }
                });
            Assert.NotNull(disposition);
            Assert.Equal(
                CrossStackBusinessFlowGraphArtifactDisposition.BaselineOnlyNotPromotable,
                disposition!.Disposition);
            Assert.NotEmpty(disposition.TrainingRowsSha256);
            Assert.NotEmpty(disposition.HeldOutRowsSha256);
            Assert.NotEmpty(disposition.TrainingExpectedMatrixCellsSha256);
            Assert.NotEmpty(disposition.TrainingActualMatrixCellsSha256);
            Assert.NotEmpty(disposition.HeldOutExpectedMatrixCellsSha256);
            Assert.NotEmpty(disposition.HeldOutActualMatrixCellsSha256);
            AssertAllowedArtifactsOnly(Path.GetDirectoryName(path)!, disposition.AllowedArtifactPatterns);
        });

        var summaries = Directory
            .EnumerateFiles(artifactRoot, "*.json", SearchOption.AllDirectories)
            .Where(path => Path.GetFileName(path) is "batch-summary.json" or "project-wiki-dpo-metric-summary.json")
            .ToArray();
        Assert.NotEmpty(summaries);
        Assert.All(summaries, path =>
            Assert.Contains("BaselineOnlyNotPromotable", File.ReadAllText(path), StringComparison.Ordinal));
    }

    private static void AssertAllowedArtifactsOnly(string scopeRoot, string[] allowedPatterns)
    {
        var unexpected = Directory
            .EnumerateFiles(scopeRoot, "*", SearchOption.AllDirectories)
            .Where(path => !allowedPatterns.Any(pattern => MatchesAllowedPattern(scopeRoot, path, pattern)))
            .ToArray();

        Assert.Empty(unexpected);
    }

    private static void AssertBaselineAllowlistsMatchWrittenArtifacts(string artifactRoot)
    {
        foreach (var path in Directory.EnumerateFiles(
                     artifactRoot,
                     "baseline-disposition.json",
                     SearchOption.AllDirectories))
        {
            var disposition = JsonSerializer.Deserialize<CrossStackBusinessFlowGraphBaselineDisposition>(
                File.ReadAllText(path),
                new JsonSerializerOptions(JsonSerializerDefaults.Web)
                {
                    Converters = { new JsonStringEnumConverter() }
            });
            Assert.NotNull(disposition);
            string[] expected = disposition!.ScopeName switch
            {
                "cross_stack_batch_root" =>
                [
                    "baseline-disposition.json",
                    "dpo-pairs.jsonl",
                    "heldout-dpo-pairs.jsonl",
                    "independent-validation-report.json",
                    "batch-summary.json",
                    "dpo-runs/**"
                ],
                "project_wiki_dpo_metric_run_root" =>
                [
                    "baseline-disposition.json",
                    "dpo-pairs.jsonl",
                    "dpo-pair-diagnostics.jsonl",
                    "project-wiki-dpo-metric-summary.json",
                    "checkpoints/policy_final.bin",
                    "checkpoints/reference_base.bin",
                    "checkpoints/dpo_policy_step_*.bin",
                    "checkpoints/dpo_step_*.json",
                    "checkpoints/.dpo_step_*.lock"
                ],
                _ => throw new InvalidOperationException(
                    $"Unknown CrossStack baseline scope: {disposition.ScopeName}.")
            };

            Assert.Equal(expected, disposition.AllowedArtifactPatterns);
            AssertAllowedArtifactsOnly(
                Path.GetDirectoryName(path)!,
                disposition.AllowedArtifactPatterns);
        }
    }

    private static void AssertNoAtomicTempFiles(string artifactRoot)
    {
        var tempFiles = Directory
            .EnumerateFiles(artifactRoot, "*.tmp", SearchOption.AllDirectories)
            .Where(path => Path.GetFileName(path).StartsWith(".", StringComparison.Ordinal))
            .ToArray();
        Assert.Empty(tempFiles);
    }

    private static bool MatchesAllowedPattern(string scopeRoot, string path, string pattern)
    {
        var relative = Path.GetRelativePath(scopeRoot, path).Replace('\\', '/');
        var normalized = pattern.Replace('\\', '/');
        if (!normalized.Contains('*', StringComparison.Ordinal))
            return string.Equals(relative, normalized, StringComparison.Ordinal);

        var parts = normalized.Split('*');
        return relative.StartsWith(parts[0], StringComparison.Ordinal) &&
               relative.EndsWith(parts[^1], StringComparison.Ordinal);
    }

    private static void AssertNoCrossStackPromotionArtifacts(string artifactRoot)
    {
        var entries = Directory.Exists(artifactRoot)
            ? Directory.EnumerateFileSystemEntries(artifactRoot, "*", SearchOption.AllDirectories).ToArray()
            : [];
        var promotionEntries = entries
            .Where(
            path =>
            {
                var name = Path.GetFileName(path);
                return string.Equals(name, "generated_capability_manifest.json", StringComparison.OrdinalIgnoreCase) ||
                       string.Equals(name, "policy.bin", StringComparison.OrdinalIgnoreCase) ||
                       string.Equals(name, "tokenizer.json", StringComparison.OrdinalIgnoreCase) ||
                       string.Equals(name, "Trusted", StringComparison.OrdinalIgnoreCase) ||
                       string.Equals(name, "remote_unit_register", StringComparison.OrdinalIgnoreCase) ||
                       string.Equals(name, "model_repository_upload", StringComparison.OrdinalIgnoreCase);
            })
            .ToArray();
        Assert.Empty(promotionEntries);
    }

    private static object CreateReflectedAuthority(
        DpoBatchSourceKind sourceKind,
        CrossStackBusinessFlowGraphDpoBatch trainingBatch,
        CrossStackBusinessFlowGraphDpoBatch heldOutBatch,
        CrossStackFlowPairBatchValidationResult trainingValidation,
        CrossStackFlowPairBatchValidationResult heldOutValidation,
        CrossStackBusinessFlowGraphTrainingRunResult trainingResult,
        ProjectWikiDpoMetricRunResult dpoMetrics)
    {
        var trainingResultIdentity = InvokeBatchIntegrity<string>(
            "ComputeTrainingResultIdentitySha256",
            trainingResult);
        var dpoMetricIdentity = InvokeBatchIntegrity<string>(
            "ComputeDpoMetricIdentitySha256",
            dpoMetrics);
        var authorityFingerprint = InvokeBatchIntegrity<string>(
            "ComputeAuthoritySha256",
            sourceKind,
            DpoBatchSplitRole.Training,
            DpoBatchSplitRole.HeldOut,
            trainingBatch.BatchId,
            heldOutBatch.BatchId,
            trainingValidation.RowFingerprintSha256,
            heldOutValidation.RowFingerprintSha256,
            trainingValidation.SourceProvenanceFingerprintSha256,
            heldOutValidation.SourceProvenanceFingerprintSha256,
            trainingValidation.ValidationFingerprintSha256,
            heldOutValidation.ValidationFingerprintSha256,
            trainingValidation.ExpectedMatrixCellsSha256,
            trainingValidation.ActualMatrixCellsSha256,
            heldOutValidation.ExpectedMatrixCellsSha256,
            heldOutValidation.ActualMatrixCellsSha256,
            trainingResultIdentity,
            dpoMetricIdentity);
        var ctor = AuthorityType()
            .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
            .Single();
        return ctor.Invoke(
            [
                sourceKind,
                DpoBatchSplitRole.Training,
                DpoBatchSplitRole.HeldOut,
                trainingBatch.BatchId,
                heldOutBatch.BatchId,
                trainingValidation.RowFingerprintSha256,
                heldOutValidation.RowFingerprintSha256,
                trainingValidation.SourceProvenanceFingerprintSha256,
                heldOutValidation.SourceProvenanceFingerprintSha256,
                trainingValidation.ValidationFingerprintSha256,
                heldOutValidation.ValidationFingerprintSha256,
                trainingValidation.ExpectedMatrixCellsSha256,
                trainingValidation.ActualMatrixCellsSha256,
                heldOutValidation.ExpectedMatrixCellsSha256,
                heldOutValidation.ActualMatrixCellsSha256,
                trainingResultIdentity,
                dpoMetricIdentity,
                authorityFingerprint
            ]);
    }

    private static async Task<OperationResult<CrossStackBusinessFlowGraphPublicationResult>>
        InvokeInternalPublishWithAuthorityAsync(
            CrossStackBusinessFlowGraphManifestPublisher publisher,
        CrossStackBusinessFlowGraphTrainingRunResult trainingResult,
        ProjectWikiDpoMetricRunResult dpoMetrics,
        CrossStackBusinessFlowGraphDpoBatch trainingBatch,
        CrossStackBusinessFlowGraphDpoBatch heldOutBatch,
        object? authority,
        string artifactRoot)
    {
        var method = typeof(CrossStackBusinessFlowGraphManifestPublisher)
            .GetMethod("PublishWithAuthorityAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(method);
        var task = (Task<OperationResult<CrossStackBusinessFlowGraphPublicationResult>>)method!.Invoke(
            publisher,
            [
                trainingResult,
                dpoMetrics,
                trainingBatch,
                heldOutBatch,
                authority,
                artifactRoot,
                "https://models.local",
                CancellationToken.None
            ])!;
        return await task;
    }

    private static T InvokeBatchIntegrity<T>(string methodName, params object[] arguments)
    {
        var helperType = typeof(CrossStackBusinessFlowGraphDpoBatch).Assembly
            .GetType("ProjectWikiDomain.CrossStackBusinessFlowGraphBatchIntegrity");
        Assert.NotNull(helperType);
        var method = helperType!.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        return (T)method!.Invoke(null, arguments)!;
    }

    private static ServerOwnedLegalTrainingTeacherAdmissionAuthority
        CreateProjectWikiLegalTeacherAdmissionAuthority()
    {
        var configuration = ContinuousLearningTp1DpoConfiguration.Create(
            "qwen3.5:9b",
            new string('d', 64),
            new string('c', 64),
            LegalTrainingTeacherAdmissionAuthority.MaxBatchRows,
            $"sha256:{new string('a', 64)}",
            $"sha256:{new string('b', 64)}",
            Enumerable.Range(1, 32)
                .Select(static value => checked((byte)value))
                .ToArray());
        Assert.True(configuration.IsSuccess, configuration.ErrorMessage);

        return ServerOwnedLegalTrainingTeacherAdmissionAuthority
            .IssueFromAdmittedConfiguration(
                configuration.Value!,
                static () => true);
    }

    private static string ResolveDomain(CrossStackBusinessFlowGraphDpoPair pair)
    {
        foreach (var rejectedType in new[]
        {
            "ui_api_only",
            "server_sql_only",
            "procedure_without_result_shape",
            "separate_repositories",
            "incomplete_cross_stack_business_flow"
        })
        {
            var suffix = $"-{Slug(rejectedType)}";
            const string prefix = "system-";
            if (pair.BoundaryMap.SystemId.StartsWith(prefix, StringComparison.Ordinal) &&
                pair.BoundaryMap.SystemId.EndsWith(suffix, StringComparison.Ordinal))
                return pair.BoundaryMap.SystemId[prefix.Length..^suffix.Length];
        }

        return "";
    }

    private static string Slug(string value)
        => value.Replace('_', '-');

    private static CrossStackBusinessFlowGraphTrainingRunResult PassingTrainingRun(
        CrossStackBusinessFlowGraphDpoBatch trainingBatch,
        CrossStackBusinessFlowGraphDpoBatch heldOutBatch,
        CrossStackFlowPairBatchValidationResult trainingValidation,
        CrossStackFlowPairBatchValidationResult heldOutValidation)
        => new(
            50,
            50,
            1.0,
            1.0,
            1.0,
            1.0,
            1.0,
            0,
            1,
            0,
            "CROSS_STACK_BUSINESS_FLOW_GRAPH_GATE_PASSED",
            heldOutBatch.Pairs
                .Select(pair => new CrossStackBusinessFlowGraphTrainingCaseResult(
                    pair.PairId,
                    1,
                    1,
                    1,
                    1,
                    1))
                .ToArray(),
            new WikiUnitTrace(
                trainingBatch.BatchId,
                50,
                50,
                [],
                [],
                [],
                0,
                false))
        {
            TrainingBatchId = trainingBatch.BatchId,
            HeldOutBatchId = heldOutBatch.BatchId,
            TrainingRowsSha256 = trainingValidation.RowFingerprintSha256,
            HeldOutRowsSha256 = heldOutValidation.RowFingerprintSha256,
            TrainingValidationFingerprintSha256 = trainingValidation.ValidationFingerprintSha256,
            HeldOutValidationFingerprintSha256 = heldOutValidation.ValidationFingerprintSha256,
            TrainingExpectedMatrixCellsSha256 = trainingValidation.ExpectedMatrixCellsSha256,
            TrainingActualMatrixCellsSha256 = trainingValidation.ActualMatrixCellsSha256,
            HeldOutExpectedMatrixCellsSha256 = heldOutValidation.ExpectedMatrixCellsSha256,
            HeldOutActualMatrixCellsSha256 = heldOutValidation.ActualMatrixCellsSha256
        };

    private static ProjectWikiDpoMetricRunResult PassingDpoMetrics(
        CrossStackBusinessFlowGraphDpoBatch trainingBatch,
        CrossStackBusinessFlowGraphDpoBatch heldOutBatch,
        CrossStackFlowPairBatchValidationResult trainingValidation,
        CrossStackFlowPairBatchValidationResult heldOutValidation)
        => new(
            "cross-stack-dpo-run",
            CrossStackBusinessFlowGraphUnit.UnitId,
            trainingBatch.BatchId,
            "",
            "",
            "",
            1,
            0.01,
            "deterministic_fresh",
            "deterministic_reference",
            0,
            "",
            "",
            50,
            50,
            0.01,
            1.0,
            0.5,
            0.01,
            1.0,
            0.5,
            0,
            0,
            1,
            0,
            "",
            [])
        {
            CrossStackTrainingBatchId = trainingBatch.BatchId,
            CrossStackHeldOutBatchId = heldOutBatch.BatchId,
            CrossStackTrainingRowsSha256 = trainingValidation.RowFingerprintSha256,
            CrossStackHeldOutRowsSha256 = heldOutValidation.RowFingerprintSha256,
            CrossStackTrainingValidationFingerprintSha256 = trainingValidation.ValidationFingerprintSha256,
            CrossStackHeldOutValidationFingerprintSha256 = heldOutValidation.ValidationFingerprintSha256,
            CrossStackBaselineDisposition = new CrossStackBusinessFlowGraphBaselineDisposition(
                CrossStackBusinessFlowGraphUnit.UnitId,
                "test_publication_candidate",
                CrossStackBusinessFlowGraphArtifactDisposition.BaselineOnlyNotPromotable,
                trainingBatch.SourceKind,
                trainingBatch.BatchId,
                heldOutBatch.BatchId,
                trainingValidation.RowFingerprintSha256,
                heldOutValidation.RowFingerprintSha256,
                trainingValidation.ValidationFingerprintSha256,
                heldOutValidation.ValidationFingerprintSha256,
                "cross-stack-business-flow-baseline-allowlist/v1",
                [],
                ["generated_capability_manifest.json", "Trusted"],
                DateTimeOffset.UtcNow.ToString("O"))
            {
                TrainingExpectedMatrixCellsSha256 = trainingValidation.ExpectedMatrixCellsSha256,
                TrainingActualMatrixCellsSha256 = trainingValidation.ActualMatrixCellsSha256,
                HeldOutExpectedMatrixCellsSha256 = heldOutValidation.ExpectedMatrixCellsSha256,
                HeldOutActualMatrixCellsSha256 = heldOutValidation.ActualMatrixCellsSha256
            }
        };

    private static Type AuthorityType()
    {
        var type = typeof(CrossStackBusinessFlowGraphDpoBatch).Assembly
            .GetType("ProjectWikiDomain.CrossStackBusinessFlowGraphPublicationAuthority");
        Assert.NotNull(type);
        return type!;
    }

    private static bool MethodMentionsPublicationAuthority(MethodInfo method)
        => TypeMentionsPublicationAuthority(method.ReturnType) ||
           method.GetParameters().Any(parameter => TypeMentionsPublicationAuthority(parameter.ParameterType));

    private static bool TypeMentionsPublicationAuthority(Type type)
        => string.Equals(type.Name, "CrossStackBusinessFlowGraphPublicationAuthority", StringComparison.Ordinal) ||
           type.GenericTypeArguments.Any(TypeMentionsPublicationAuthority);

    private static void AssertOperationFailed(object operationResult, string expectedErrorCode)
    {
        Assert.False((bool)operationResult.GetType().GetProperty(nameof(OperationResult<bool>.IsSuccess))!.GetValue(operationResult)!);
        Assert.Equal(
            expectedErrorCode,
            operationResult.GetType().GetProperty(nameof(OperationResult<bool>.ErrorCode))!.GetValue(operationResult));
    }

    private sealed class CountingModelRepository : IModelRepository
    {
        public int UploadCount { get; private set; }

        public OperationResult<string> PreflightStableConfigurationIdentitySha256()
            => OperationResult<string>.Ok(new string('a', 64).Insert(0, "sha256:"));

        public Task<OperationResult<Stream>> LoadWeightsAsync(
            string unitId,
            string? serverBaseUrl,
            string localFallbackPath,
            CancellationToken ct = default)
            => Task.FromResult(OperationResult<Stream>.Fail(
                "Test repository does not load weights.",
                "TEST_LOAD_NOT_AVAILABLE"));

        public Task<OperationResult<RemoteUnitManifest>> UploadAsync(
            GeneratedCapabilityCheckpointManifest manifest,
            string serverBaseUrl,
            CancellationToken ct = default)
        {
            UploadCount++;
            return Task.FromResult(OperationResult<RemoteUnitManifest>.Fail(
                "Publisher should reject seed before upload.",
                "TEST_UPLOAD_SHOULD_NOT_RUN"));
        }
    }

    private sealed class CountingRemoteUnitLocator : IRemoteUnitLocator
    {
        public int RegisterCount { get; private set; }

        public OperationResult<string> PreflightStableConfigurationIdentitySha256()
            => OperationResult<string>.Ok(new string('b', 64).Insert(0, "sha256:"));

        public Task<OperationResult<RemoteUnitManifest>> LocateAsync(
            string unitId,
            CancellationToken ct = default)
            => Task.FromResult(OperationResult<RemoteUnitManifest>.Fail(
                "Test locator does not locate units.",
                "TEST_LOCATE_NOT_AVAILABLE"));

        public Task<OperationResult<bool>> RegisterAsync(
            RemoteUnitManifest manifest,
            CancellationToken ct = default)
        {
            RegisterCount++;
            return Task.FromResult(OperationResult<bool>.Fail(
                "Publisher should reject seed before register.",
                "TEST_REGISTER_SHOULD_NOT_RUN"));
        }
    }
}
