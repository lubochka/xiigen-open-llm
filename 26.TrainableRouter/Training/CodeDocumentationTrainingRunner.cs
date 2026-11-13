// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TrainableRouter.Training;

using System.Diagnostics;
using System.Globalization; // Encode the actual finite model/scorer values as round-trip strings.
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using ModelServer.Contracts;
using ModelServer.Remote;

public sealed class CodeDocumentationTrainingRunner
{
    private readonly RoutingProfileTrainer _profileTrainer;
    internal int SelectedProfileTrainCalls { get; private set; }
    internal int SelectedProfileTrainExampleCount { get; private set; }
    internal IReadOnlyDictionary<string, UnitScoringProfile>? SelectedProfileTrainResult { get; private set; }

    public CodeDocumentationTrainingRunner(RoutingProfileTrainer? profileTrainer = null)
    {
        _profileTrainer = profileTrainer ?? new RoutingProfileTrainer();
    }

    /// <summary>Train the selected routing profile once without constructing a runtime or routing held-out rows.</summary>
    internal OperationResult<IReadOnlyDictionary<string, UnitScoringProfile>> TrainSelectedProfileOnly(
        CodeDocumentationBatchFileRunner.CodeDocumentationProfileInputsOpenResult material,
        CodeDocumentationBatchFileRunner owner,
        CodeDocumentationOneLessonSession custody,
        CodeDocumentationBatchFileRunner.SelectedProfileFiles heldFiles,
        CancellationToken cancellationToken)
    {
        if (material is null || !material.IsOwnedBy(owner, this) ||
            heldFiles is null || !heldFiles.IsOwnedBy(owner, material) ||
            material.TrainingExamples.Length != 50 || material.EvaluationExamples.Length != 50)
            return OperationResult<IReadOnlyDictionary<string, UnitScoringProfile>>.Fail(
                "The selected profile material belongs to a different runner or is incomplete.",
                CodeDocumentationBatchFileRunner.ProfileMaterialUnavailableErrorCode);
        var metadata = DescribeMetadata();
        if (!metadata.IsSuccess || metadata.Value is null)
            return metadata.MapError<IReadOnlyDictionary<string, UnitScoringProfile>>();
        var selected = material.Selection.Snapshot;
        var rebuilt = CodeDocumentationOneLessonProfileSelection.Create(selected with
        {
            UnitMetadata = metadata.Value,
            RoutingConfiguration = DescribeConfiguration(),
            RuntimeArtifactMap = DescribeRuntimeInputs("https://models.local"),
        });
        if (!rebuilt.IsSuccess || rebuilt.Value is null ||
            !CodeDocumentationOneLessonV2Wire.CanonicalProfileSelectionBytes(material.Selection).AsSpan()
                .SequenceEqual(CodeDocumentationOneLessonV2Wire.CanonicalProfileSelectionBytes(rebuilt.Value)))
            return OperationResult<IReadOnlyDictionary<string, UnitScoringProfile>>.Fail(
                "The current injected trainer metadata or runtime inputs changed.",
                CodeDocumentationBatchFileRunner.ProfileMaterialUnavailableErrorCode);
        cancellationToken.ThrowIfCancellationRequested();
        if (!heldFiles.Validate())
            return OperationResult<IReadOnlyDictionary<string, UnitScoringProfile>>.Fail(
                "Selected profile input custody or full raw bytes changed before Train.",
                CodeDocumentationBatchFileRunner.ProfileMaterialUnavailableErrorCode);
        var guarded = custody.GuardProfileTrainV2(material.Selection);
        if (!guarded.IsSuccess) return guarded.MapError<IReadOnlyDictionary<string, UnitScoringProfile>>();
        var examples = material.TrainingExamples.Select(ToRoutingTrainingExample).ToArray();
        SelectedProfileTrainCalls++;
        SelectedProfileTrainExampleCount = examples.Length;
        var result = _profileTrainer.Train(examples);
        if (!heldFiles.Validate())
            return OperationResult<IReadOnlyDictionary<string, UnitScoringProfile>>.Fail(
                "Selected profile input custody or full raw bytes changed during Train.",
                CodeDocumentationBatchFileRunner.ProfileMaterialUnavailableErrorCode);
        guarded = custody.GuardProfileTrainV2(material.Selection);
        if (!guarded.IsSuccess) return guarded.MapError<IReadOnlyDictionary<string, UnitScoringProfile>>();
        if (result.IsSuccess) SelectedProfileTrainResult = result.Value;
        return result;
    }

    public async Task<OperationResult<CodeDocumentationTrainingRunResult>> TrainAndEvaluateAsync(
        IReadOnlyList<CodeDocumentationTrainingExample> trainingExamples,
        IReadOnlyList<CodeDocumentationTrainingExample> evaluationExamples,
        CancellationToken ct = default,
        CodeDocumentationRepositoryOwnedDpoRunOptions? repositoryOwnedDpo = null)
    {
        if (trainingExamples is null)
            return OperationResult<CodeDocumentationTrainingRunResult>.Fail("Training examples are required.", "TRAINING_EXAMPLES_REQUIRED");

        if (evaluationExamples is null)
            return OperationResult<CodeDocumentationTrainingRunResult>.Fail("Evaluation examples are required.", "EVALUATION_EXAMPLES_REQUIRED");

        if (trainingExamples.Count != 50)
            return OperationResult<CodeDocumentationTrainingRunResult>.Fail("Code documentation training batches must contain exactly 50 examples.", "TRAINING_BATCH_SIZE_NOT_50");

        if (evaluationExamples.Count != 50)
            return OperationResult<CodeDocumentationTrainingRunResult>.Fail("Code documentation held-out batches must contain exactly 50 examples.", "HELDOUT_BATCH_SIZE_NOT_50");

        var repositoryAddress = ResolveRepositoryOwnedDpoAddress(repositoryOwnedDpo);
        if (!repositoryAddress.IsSuccess)
            return repositoryAddress.MapError<CodeDocumentationTrainingRunResult>();

        var stopwatch = Stopwatch.StartNew();
        var process = Process.GetCurrentProcess();
        process.Refresh();
        var cpuBefore = process.TotalProcessorTime;
        var peakWorkingSetMb = BytesToMb(process.WorkingSet64);

        var routingTrainingExamples = trainingExamples.Select(ToRoutingTrainingExample).ToArray();
        var profiles = _profileTrainer.Train(routingTrainingExamples);
        if (!profiles.IsSuccess)
            return profiles.MapError<CodeDocumentationTrainingRunResult>();

        var runtime = await BuildRuntimeAsync(profiles.Value!, ct);
        if (!runtime.IsSuccess)
            return runtime.MapError<CodeDocumentationTrainingRunResult>();

        var flow = new CodeDocumentationRoutingFlow(runtime.Value!);
        var cases = new List<CodeDocumentationTrainingCaseResult>();
        var decisions = new List<RoutingDecisionRecord>();

        foreach (var example in evaluationExamples)
        {
            if (ct.IsCancellationRequested)
                return OperationResult<CodeDocumentationTrainingRunResult>.Fail("Code documentation training was cancelled.", "CODE_DOC_TRAINING_CANCELLED");

            var routed = await flow.RouteDocumentationAsync(example.Request, ct);
            if (!routed.IsSuccess)
                return routed.MapError<CodeDocumentationTrainingRunResult>();

            var value = routed.Value!;
            var routedCorrect = string.Equals(value.ChosenUnitId, example.ExpectedUnitId, StringComparison.Ordinal);
            var sourceCorrect = value.SourceGate.IsPassed == example.ExpectSourceGatePass;
            var versionCorrect = value.VersionGate.IsPassed == example.ExpectVersionGatePass;
            var transitCorrect = value.TransitGate.IsPassed == example.ExpectTransitGatePass;
            cases.Add(new CodeDocumentationTrainingCaseResult(
                example.ExampleId,
                example.ExpectedUnitId,
                value.ChosenUnitId,
                routedCorrect,
                sourceCorrect,
                versionCorrect,
                transitCorrect,
                value.SourceGate.Score,
                value.VersionGate.Score,
                value.TransitGate.BottomUpOrderScore,
                value.TransitGate.CalleeTransitScore,
                value.TransitGate.LocalVsDelegatedScore,
                value.RoutingConfidence,
                value.Verdict));

            decisions.Add(new RoutingDecisionRecord(
                $"code-doc-eval-{example.ExampleId}",
                example.ExampleId,
                BuildPrompt(example.Request),
                BuildTaskSlice(example.Request),
                example.ExpectedUnitId,
                routedCorrect ? 1 : 0,
                BuildRejectedCandidates(example.ExpectedUnitId),
                example.CorrectReason,
                example.WrongRouteConsequence,
                DateTimeOffset.UtcNow));

            process.Refresh();
            peakWorkingSetMb = Math.Max(peakWorkingSetMb, BytesToMb(process.WorkingSet64));
        }

        var pairs = new RoutingDpoPairBuilder().BuildPairs(decisions);
        if (!pairs.IsSuccess)
            return pairs.MapError<CodeDocumentationTrainingRunResult>();

        var repositoryDpo = await ResolveRepositoryOwnedDpoAsync(
                repositoryOwnedDpo,
                repositoryAddress.Value,
                decisions,
                ct)
            .ConfigureAwait(false);
        if (!repositoryDpo.IsSuccess)
            return repositoryDpo.MapError<CodeDocumentationTrainingRunResult>();

        process.Refresh();
        stopwatch.Stop();
        var cpuMs = (process.TotalProcessorTime - cpuBefore).TotalMilliseconds;
        var routedCount = cases.Count(item => item.RoutedCorrect);
        var sourceCount = cases.Count(item => item.SourceGateCorrect);
        var versionCount = cases.Count(item => item.VersionGateCorrect);
        var transitCount = cases.Count(item => item.TransitGateCorrect);

        var result = new CodeDocumentationTrainingRunResult(
            trainingExamples.Count,
            evaluationExamples.Count,
            routedCount,
            sourceCount,
            versionCount,
            transitCount,
            routedCount / (double)evaluationExamples.Count,
            sourceCount / (double)evaluationExamples.Count,
            versionCount / (double)evaluationExamples.Count,
            transitCount / (double)evaluationExamples.Count,
            Average(cases.Select(item => item.BottomUpOrderScore)),
            Average(cases.Select(item => item.CalleeTransitScore)),
            Average(cases.Select(item => item.LocalVsDelegatedScore)),
            cpuMs,
            peakWorkingSetMb,
            stopwatch.Elapsed.TotalMilliseconds,
            pairs.Value!,
            repositoryDpo.Value?.Receipt,
            repositoryDpo.Value?.RepositoryOwnedDpoPairs ?? [],
            cases.ToArray());

        return OperationResult<CodeDocumentationTrainingRunResult>.Ok(result);
    }

    private static OperationResult<RepositoryOwnedDpoDatasetAddress?>
        ResolveRepositoryOwnedDpoAddress(
            CodeDocumentationRepositoryOwnedDpoRunOptions? options)
    {
        if (options is null)
            return OperationResult<RepositoryOwnedDpoDatasetAddress?>.Ok(null);

        if (string.IsNullOrWhiteSpace(options.RepositoryRoot) ||
            options.AuthoringContext is null)
        {
            return OperationResult<RepositoryOwnedDpoDatasetAddress?>.Fail(
                "Repository-owned code documentation DPO mode needs a repository root and authoring context.",
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .InvalidErrorCode);
        }

        var contextValidation = options.AuthoringContext.Validate();
        if (!contextValidation.IsSuccess)
            return contextValidation
                .MapError<RepositoryOwnedDpoDatasetAddress?>();

        var profile = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
            .ResolveContextDomainProfile(options.AuthoringContext.DomainProfile);
        if (!profile.IsSuccess || profile.Value is null)
            return profile.MapError<RepositoryOwnedDpoDatasetAddress?>();

        var address = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
            .CreateAddress(
                options.RepositoryRoot,
                profile.Value,
                options.DatasetVersion);
        return address.IsSuccess && address.Value is not null
            ? OperationResult<RepositoryOwnedDpoDatasetAddress?>.Ok(
                address.Value)
            : address.MapError<RepositoryOwnedDpoDatasetAddress?>();
    }

    private static async Task<OperationResult<
        CodeDocumentationRepositoryOwnedDpoRunEvidence?>>
        ResolveRepositoryOwnedDpoAsync(
            CodeDocumentationRepositoryOwnedDpoRunOptions? options,
            RepositoryOwnedDpoDatasetAddress? address,
            IReadOnlyList<RoutingDecisionRecord> decisions,
            CancellationToken cancellationToken)
    {
        if (options is null)
            return OperationResult<CodeDocumentationRepositoryOwnedDpoRunEvidence?>
                .Ok(null);

        if (address is null)
        {
            return OperationResult<
                CodeDocumentationRepositoryOwnedDpoRunEvidence?>.Fail(
                "Repository-owned code documentation DPO address is invalid.",
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .InvalidErrorCode);
        }

        if (options.ResolveAuthoringSource is null)
        {
            return OperationResult<
                CodeDocumentationRepositoryOwnedDpoRunEvidence?>.Fail(
                "Repository-owned code documentation DPO authoring requires a server-owned validated teaching source resolver; raw evaluation decisions cannot grant teaching approval.",
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .AuthoringSourceRequiredErrorCode);
        }

        OperationResult<CodeDocumentationRepositoryOwnedDpoAuthoringSource>
            authoringSource;
        try
        {
            authoringSource = options.ResolveAuthoringSource(decisions);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<
                CodeDocumentationRepositoryOwnedDpoRunEvidence?>.Fail(
                "The server-owned teaching source resolver failed closed before repository-owned DPO authoring.",
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .AuthoringSourceInvalidErrorCode);
        }

        if (!authoringSource.IsSuccess || authoringSource.Value is null)
        {
            return authoringSource.MapError<
                CodeDocumentationRepositoryOwnedDpoRunEvidence?>();
        }

        if (!authoringSource.Value.MatchesReferenceRows(decisions))
        {
            return OperationResult<
                CodeDocumentationRepositoryOwnedDpoRunEvidence?>.Fail(
                "The server-owned teaching source is not bound to the exact routing decisions produced by this run.",
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .AuthoringSourceBindingMismatchErrorCode);
        }

        var resolved = await CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
            .ResolveOrAuthorAsync(
                address,
                authoringSource.Value,
                options.AuthoringContext,
                cancellationToken)
            .ConfigureAwait(false);
        if (!resolved.IsSuccess || resolved.Value is null)
        {
            return resolved.MapError<
                CodeDocumentationRepositoryOwnedDpoRunEvidence?>();
        }

        var dataset = resolved.Value.Dataset;
        var manifest = dataset.Manifest;
        var receipt = new CodeDocumentationRepositoryOwnedDpoDatasetReceipt(
            resolved.Value.Source,
            manifest.ManifestSha256,
            address.ManifestRelativePath,
            manifest.PairCount,
            manifest.RequestIdentity.RequestIdentitySha256,
            manifest.OrderedPairs.ToArray());

        return OperationResult<CodeDocumentationRepositoryOwnedDpoRunEvidence?>
            .Ok(new CodeDocumentationRepositoryOwnedDpoRunEvidence(
                receipt,
                dataset.Pairs.ToArray()));
    }

    internal static RoutingTrainingExample ToRoutingTrainingExample(CodeDocumentationTrainingExample example) // SAME normal training projection is available to passive material admission.
        => new(
            example.ExampleId,
            BuildPrompt(example.Request),
            BuildTaskSlice(example.Request),
            null,
            "documentation",
            CodeDocumentationRoutingFlow.DescribeCandidateUnits(), // Copy the actual routing candidate order without an independent map.
            example.ExpectedUnitId,
            OrchestrationRoutingCurriculum.OrchestrationV309UnitId,
            example.CorrectReason,
            example.WrongRouteConsequence);

    private static async Task<OperationResult<DistributedRoutingRuntime>> BuildRuntimeAsync(
        IReadOnlyDictionary<string, UnitScoringProfile> profiles,
        CancellationToken ct)
    {
        var locator = new InMemoryRemoteUnitLocator();
        foreach (var manifest in BuildDefaultManifests("https://models.local"))
        {
            var registered = await locator.RegisterAsync(manifest, ct);
            if (!registered.IsSuccess)
                return registered.MapError<DistributedRoutingRuntime>();
        }

        var flow = new TrainableRoutingFlow(
            new BaselineKeywordCandidateGenerator(profiles),
            new InMemoryRoutingDecisionLog());

        return OperationResult<DistributedRoutingRuntime>.Ok(new DistributedRoutingRuntime(flow, locator, profiles));
    }

    internal OperationResult<CodeDocumentationProfileMetadataV2[]> DescribeMetadata() // Snapshot every actual candidate entry from this trainer injected registry.
        => _profileTrainer.DescribeMetadata(CodeDocumentationRoutingFlow.DescribeCandidateUnits()); // Preserve injected trainer/registry identity on the owning runner.

    internal static CodeDocumentationProfileConfigurationV2 DescribeConfiguration() // Project the same actual trainer, scorer and threshold inputs used by normal execution.
    { // Project the same actual trainer, scorer and threshold inputs used by normal execution.
        static string D(double value) => value.ToString("R", CultureInfo.InvariantCulture); // Project the same actual trainer, scorer and threshold inputs used by normal execution.
        var thresholds = RoutingFlowThresholds.Default; // Describe the actual normal flow threshold input, not an external algorithm version.
        return new(RoutingProfileTrainer.DescribeStopWords(), RoutingProfileTrainer.MinimumKeywordLength, RoutingProfileTrainer.KeywordSeparators, // Project the same actual trainer, scorer and threshold inputs used by normal execution.
            RoutingProfileTrainer.CSharpFrom, RoutingProfileTrainer.CSharpTo, RoutingProfileTrainer.DotNetFrom, RoutingProfileTrainer.DotNetTo, // Project the same actual trainer, scorer and threshold inputs used by normal execution.
            true, nameof(StringComparer.OrdinalIgnoreCase), nameof(StringComparer.OrdinalIgnoreCase), RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole, // Project the same actual trainer, scorer and threshold inputs used by normal execution.
            D(BaselineKeywordCandidateGenerator.DomainBoost), D(BaselineKeywordCandidateGenerator.DirectionBoost), D(BaselineKeywordCandidateGenerator.MaximumConfidence), // Project the same actual trainer, scorer and threshold inputs used by normal execution.
            D(BaselineKeywordCandidateGenerator.BaseConfidence), D(BaselineKeywordCandidateGenerator.KeywordConfidenceMultiplier), // Project the same actual trainer, scorer and threshold inputs used by normal execution.
            D(BaselineKeywordCandidateGenerator.DomainConfidenceMultiplier), D(BaselineKeywordCandidateGenerator.DirectionConfidenceMultiplier), // Project the same actual trainer, scorer and threshold inputs used by normal execution.
            D(thresholds.MinimumRoutingConfidence), D(thresholds.MinimumScoreGap), thresholds.ThresholdsRef, // Project the same actual trainer, scorer and threshold inputs used by normal execution.
            CodeDocumentationRoutingFlow.FlowId, CodeDocumentationRoutingFlow.ScenarioId); // Project the same actual trainer, scorer and threshold inputs used by normal execution.
    } // Project the same actual trainer, scorer and threshold inputs used by normal execution.

    private static RemoteUnitManifest[] BuildDefaultManifests(string serverBaseUrl)
        => DescribeRuntimeInputs(serverBaseUrl).Select(BuildManifest).ToArray(); // Normal runtime and admission share the stable model input map.

    internal static CodeDocumentationProfileRuntimeV2[] DescribeRuntimeInputs(string serverBaseUrl) // Project the same complete stable manifest input map consumed by normal runtime construction.
        =>
        [
            BuildRuntimeInput( // Project the same complete stable manifest input map consumed by normal runtime construction.
                OrchestrationRoutingCurriculum.OrchestrationV309UnitId,
                "orchestration-planning-unit",
                "orchestration",
                "v309",
                serverBaseUrl,
                "/models/unit-orchestration-plan-v309",
                "manifest-orchestration-v309",
                "orchestration/v309-plan-routing",
                OrchestrationRoutingCurriculum.V309TrainingRunId,
                OrchestrationRoutingCurriculum.V309DatasetSha256,
                "server://tokenizers/orchestration-v309/tokenizer.json"),
            BuildRuntimeInput( // Project the same complete stable manifest input map consumed by normal runtime construction.
                OrchestrationRoutingCurriculum.TranslationRuEnUnitId,
                "translation-unit",
                "translation",
                "v1",
                serverBaseUrl,
                "/models/unit-ru-en-general",
                "manifest-translation-ru-en-v1",
                "translation/ru-en-general",
                "translation-placeholder-run",
                "translation-placeholder-dataset",
                "server://tokenizers/translation-general/tokenizer.json"),
            BuildRuntimeInput( // Project the same complete stable manifest input map consumed by normal runtime construction.
                OrchestrationRoutingCurriculum.DocumentationDotNetUnitId,
                "documentation-unit",
                "documentation",
                "v1",
                serverBaseUrl,
                "/models/unit-dotnet-legacy-doc-l1",
                "manifest-dotnet-doc-v1",
                "documentation/dotnet-legacy-l1",
                "documentation-runner-adapter-v1",
                "documentation-runner-adapter-dataset",
                "server://tokenizers/dotnet-doc/tokenizer.json")
        ];

    private static CodeDocumentationProfileRuntimeV2 BuildRuntimeInput( // Retain every existing model and LoRA parameter without an output timestamp.
        string unitId,
        string unitRole,
        string domainTag,
        string version,
        string serverBaseUrl,
        string endpoint,
        string manifestSha256,
        string trainingBranch,
        string trainingRunId,
        string sourceDatasetSha256,
        string tokenizerPath)
        => new(
            unitId,
            unitRole,
            domainTag,
            version,
            serverBaseUrl,
            endpoint,
            manifestSha256,
            trainingBranch,
            trainingRunId,
            sourceDatasetSha256,
            tokenizerPath,
            2048, 96, 64, 2, 4, 4, 128, false, "", false, "", // Retain every existing model and LoRA parameter without an output timestamp.
            2, 4f.ToString("R", CultureInfo.InvariantCulture), 0.05f.ToString("R", CultureInfo.InvariantCulture), ["q_proj", "v_proj"]); // Stable input excludes only the output creation timestamp.

    private static RemoteUnitManifest BuildManifest(CodeDocumentationProfileRuntimeV2 input) // Construct the normal runtime manifest from its same stable input projection.
        => new(input.UnitId, input.UnitRole, input.DomainTag, input.Version, input.ServerBaseUrl, input.UnitEndpoint, // Construct the normal runtime manifest from its same stable input projection.
            input.ManifestSha256, input.TrainingBranch, input.TrainingRunId, input.SourceDatasetSha256, // Construct the normal runtime manifest from its same stable input projection.
            new RemoteModelConfig(input.VocabSize, input.ContextLen, input.HiddenDim, input.NumLayers, input.NumHeads, input.NumKvHeads, input.FfnHiddenDim, // Construct the normal runtime manifest from its same stable input projection.
                input.RopeThetaPresent ? float.Parse(input.RopeTheta, CultureInfo.InvariantCulture) : null, // Construct the normal runtime manifest from its same stable input projection.
                input.RmsNormEpsPresent ? float.Parse(input.RmsNormEps, CultureInfo.InvariantCulture) : null), // Construct the normal runtime manifest from its same stable input projection.
            new RemoteLoraConfig(input.LoraRank, float.Parse(input.LoraAlpha, CultureInfo.InvariantCulture), float.Parse(input.LoraDropout, CultureInfo.InvariantCulture), input.LoraTargetModules.ToArray()), // Construct the normal runtime manifest from its same stable input projection.
            input.TokenizerPath, DateTimeOffset.UtcNow); // Timestamp and locator identity remain runtime outputs, never selected input authority.

    private static UnitScoringResult[] BuildRejectedCandidates(string expectedUnitId)
        =>
        [
            new UnitScoringResult(
                expectedUnitId == OrchestrationRoutingCurriculum.OrchestrationV309UnitId
                    ? OrchestrationRoutingCurriculum.DocumentationDotNetUnitId
                    : OrchestrationRoutingCurriculum.OrchestrationV309UnitId,
                "wrong-specialist-unit",
                "wrong-domain",
                0,
                0.01)
        ];

    private static string BuildPrompt(CodeDocumentationRequest request)
        => $"Document legacy C# code bottom-up for {request.StackLabel} {request.TargetFramework} {request.CSharpLanguageVersion}. Symbol: {request.SymbolId}. Level: {request.DocumentationLevel}.";

    private static string BuildTaskSlice(CodeDocumentationRequest request)
        => string.Join(
            " ",
            [
                "documentation",
                "legacy dotnet code documentation",
                "bottom-up callee caller propagation",
                request.AppSide,
                request.RuntimeFamily,
                request.TargetFramework,
                request.CSharpLanguageVersion,
                request.WebFramework,
                request.FrontendFramework,
                request.StackLabel,
                request.ArtifactKind,
                request.DocumentationLevel
            ]);

    private static double Average(IEnumerable<double> values)
    {
        var materialized = values.ToArray();
        return materialized.Length == 0 ? 0 : materialized.Average();
    }

    private static double BytesToMb(long bytes)
        => bytes / 1024.0 / 1024.0;
}

public sealed record CodeDocumentationTrainingExample(
    string ExampleId,
    CodeDocumentationRequest Request,
    string ExpectedUnitId,
    bool ExpectVersionGatePass,
    bool ExpectTransitGatePass,
    string CorrectReason,
    string WrongRouteConsequence,
    bool ExpectSourceGatePass = true);

public sealed record CodeDocumentationTrainingRunResult(
    int TrainingExampleCount,
    int EvaluationExampleCount,
    int RoutedCorrect,
    int SourceCorrect,
    int VersionCorrect,
    int TransitCorrect,
    double RoutingAccuracy,
    double SourceGateAccuracy,
    double VersionGateAccuracy,
    double TransitGateAccuracy,
    double BottomUpOrderAccuracy,
    double CalleeSummaryTransitAccuracy,
    double LocalVsDelegatedAccuracy,
    double CpuMilliseconds,
    double PeakWorkingSetMb,
    double ElapsedMilliseconds,
    RoutingDpoPair[] DpoPairs,
    CodeDocumentationRepositoryOwnedDpoDatasetReceipt? RepositoryOwnedDpoReceipt,
    ContinuousLearning.DpoPair[] RepositoryOwnedDpoPairs,
    CodeDocumentationTrainingCaseResult[] Cases);

public sealed record CodeDocumentationRepositoryOwnedDpoRunOptions(
    string RepositoryRoot,
    CodeDocumentationRepositoryOwnedDpoDatasetAuthoringContext AuthoringContext,
    string DatasetVersion =
        CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.DefaultDatasetVersion,
    Func<IReadOnlyList<RoutingDecisionRecord>,
        OperationResult<CodeDocumentationRepositoryOwnedDpoAuthoringSource>>?
        ResolveAuthoringSource = null);

public sealed record CodeDocumentationRepositoryOwnedDpoDatasetReceipt(
    RepositoryOwnedDpoDatasetResolutionSource Source,
    string ManifestSha256,
    string RepositoryRelativeManifestPath,
    int PairCount,
    string RequestIdentitySha256,
    RepositoryOwnedDpoDatasetManifestPair[] OrderedPairs);

internal sealed record CodeDocumentationRepositoryOwnedDpoRunEvidence(
    CodeDocumentationRepositoryOwnedDpoDatasetReceipt Receipt,
    ContinuousLearning.DpoPair[] RepositoryOwnedDpoPairs);

public sealed record CodeDocumentationTrainingCaseResult(
    string ExampleId,
    string ExpectedUnitId,
    string ActualUnitId,
    bool RoutedCorrect,
    bool SourceGateCorrect,
    bool VersionGateCorrect,
    bool TransitGateCorrect,
    double SourceScore,
    double VersionScore,
    double BottomUpOrderScore,
    double CalleeTransitScore,
    double LocalVsDelegatedScore,
    double RoutingConfidence,
    string Verdict);
