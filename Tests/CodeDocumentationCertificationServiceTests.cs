// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using ContinuousLearning;
using DPO.Models;
using Tokenizer;
using TrainableRouter.Training;
using TransformerModel;
using TransformerModel.Config;

public sealed class CodeDocumentationCertificationServiceTests
{
    private const string Subject = "api-reference";
    private const string Owner = "synthetic-certification-owner";
    private static readonly DateTimeOffset Cut =
        new(2026, 9, 15, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Observed =
        new(2026, 9, 16, 12, 0, 0, TimeSpan.Zero);
    private static readonly CertificationHistoryCoverage Complete = new(
        CertificationHistoryCompleteness.Complete,
        CertificationHistoryCompleteness.Complete,
        CertificationHistoryCompleteness.Complete);

    [Fact]
    public async Task CertifyAsync_CleanPassingExamCompletesAndCertifies()
    {
        using var fixture = CertificationFixture.Create("passing");
        using var evaluator = new ScriptedCertificationEvaluator(
            fixture.Authority.Evaluator,
            pass: true);
        var inputs = fixture.CaptureCertificationInputs();

        var result = await fixture.CertifyAsync(evaluator);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.True(result.Value!.Certified);
        Assert.False(result.Value.Replayed);
        Assert.Equal(10, evaluator.IssuedCallCount);
        Assert.Equal(10, result.Value.Receipt.ObservedEvaluationCallCount);
        Assert.Equal(
            MeasurementNoisePolicy.ImprovedStatus,
            result.Value.TargetComparison.Status);
        Assert.All(
            result.Value.Receipt.MeasurementNoisePassport.RepeatedScores,
            score => Assert.Equal(1d, score));
        fixture.AssertCertificationInputsPreserved(inputs);
    }

    [Fact]
    public async Task CertifyAsync_NoCleanAdmissionDoesNotReuseDiagnosticScore()
    {
        using var fixture = CertificationFixture.Create("no-clean-admission");
        using var evaluator = new ScriptedCertificationEvaluator(
            fixture.Authority.Evaluator,
            pass: true);
        var request = fixture.CreateDiagnosticOnlyRequest(evaluator);

        var result = await new CodeDocumentationCertificationService()
            .CertifyAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            CodeDocumentationCertificationService.AdmissionRejectedErrorCode,
            result.ErrorCode);
        Assert.Equal(0, evaluator.IssuedCallCount);
    }

    [Fact]
    public async Task CertifyAsync_CleanBelowTargetCompletesDurableNegative()
    {
        using var fixture = CertificationFixture.Create("below-target");
        using var evaluator = new ScriptedCertificationEvaluator(
            fixture.Authority.Evaluator,
            pass: false);
        var inputs = fixture.CaptureCertificationInputs();

        var result = await fixture.CertifyAsync(evaluator);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.False(result.Value!.Certified);
        Assert.False(result.Value.Replayed);
        Assert.Equal(10, evaluator.IssuedCallCount);
        Assert.Equal(
            MeasurementNoisePolicy.RegressedStatus,
            result.Value.TargetComparison.Status);
        Assert.All(
            result.Value.Receipt.MeasurementNoisePassport.RepeatedScores,
            score => Assert.Equal(0d, score));
        fixture.AssertCertificationInputsPreserved(inputs);
    }

    [Fact]
    public async Task CertifyAsync_InvalidPartialSeriesFailsClosedWithoutCompletion()
    {
        using var fixture = CertificationFixture.Create("invalid-series");
        using var evaluator = new ScriptedCertificationEvaluator(
            fixture.Authority.Evaluator,
            pass: true,
            requestIdFactory: call => call == 3
                ? "cert-evaluation-0001"
                : "cert-evaluation-" + call.ToString("D4"));

        var failed = await fixture.CertifyAsync(evaluator);

        Assert.False(failed.IsSuccess);
        Assert.NotNull(failed.ErrorCode);
        Assert.True(evaluator.IssuedCallCount < 10);

        using var secondEvaluator =
            new ScriptedCertificationEvaluator(
                fixture.Authority.Evaluator,
                pass: true);
        var second = await fixture.CertifyAsync(secondEvaluator);
        Assert.False(second.IsSuccess);
        Assert.Equal(
            CodeDocumentationCertificationSessionStore.ErrorPrefix +
            "INCOMPLETE_EXPOSED",
            second.ErrorCode);
        Assert.Equal(0, secondEvaluator.IssuedCallCount);
    }

    [Fact]
    public async Task CertifyAsync_CheckpointChangedDuringExamFailsClosed()
    {
        using var fixture = CertificationFixture.Create("stale-checkpoint");
        using var evaluator = new ScriptedCertificationEvaluator(
            fixture.Authority.Evaluator,
            pass: true,
            afterPermit: call =>
            {
                if (call == 1)
                    fixture.CorruptAcceptedWeightsInPlace();
            });

        var result = await fixture.CertifyAsync(evaluator);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            CodeDocumentationCertificationService.SavedWeightsChangedErrorCode,
            result.ErrorCode);
        Assert.True(evaluator.IssuedCallCount > 0);
    }

    [Fact]
    public async Task CertifyAsync_ExactReplayReturnsReceiptWithoutNewCalls()
    {
        using var fixture = CertificationFixture.Create("replay");
        using var firstEvaluator = new ScriptedCertificationEvaluator(
            fixture.Authority.Evaluator,
            pass: true);
        var first = await fixture.CertifyAsync(firstEvaluator);
        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.Equal(10, firstEvaluator.IssuedCallCount);

        using var replayEvaluator =
            new ScriptedCertificationEvaluator(
                fixture.Authority.Evaluator,
                pass: false);
        var replay = await fixture.CertifyAsync(replayEvaluator);

        Assert.True(replay.IsSuccess, replay.ErrorMessage);
        Assert.True(replay.Value!.Certified);
        Assert.True(replay.Value.Replayed);
        Assert.Equal(0, replayEvaluator.IssuedCallCount);
        Assert.Equal(
            first.Value!.ReceiptIdentitySha256,
            replay.Value.ReceiptIdentitySha256);
    }

    [Fact]
    public Task CertifyAsync_DeferredEvaluatorFailureLeavesDurableIncompleteSessionAfterFailure()
        => AssertDeferredEvaluatorFailureLeavesDurableIncompleteSession("failure");

    [Fact]
    public Task CertifyAsync_DeferredEvaluatorFailureLeavesDurableIncompleteSessionAfterNull()
        => AssertDeferredEvaluatorFailureLeavesDurableIncompleteSession("null");

    [Fact]
    public Task CertifyAsync_DeferredEvaluatorFailureLeavesDurableIncompleteSessionAfterThrow()
        => AssertDeferredEvaluatorFailureLeavesDurableIncompleteSession("throw");

    private async Task AssertDeferredEvaluatorFailureLeavesDurableIncompleteSession(
        string failureMode)
    {
        using var fixture = CertificationFixture.Create(
            "deferred-evaluator-" + failureMode);
        var createCalls = 0;

        OperationResult<ILegalTeacherEvaluationInferenceClient> CreateEvaluator()
        {
            createCalls++;
            return failureMode switch
            {
                "failure" => OperationResult<
                    ILegalTeacherEvaluationInferenceClient>.Fail(
                    "synthetic evaluator failure",
                    "S03_TEST_EVALUATOR_FAILURE"),
                "null" => OperationResult<
                    ILegalTeacherEvaluationInferenceClient>.Ok(null!),
                _ => throw new InvalidOperationException(
                    "synthetic evaluator acquisition exception"),
            };
        }

        var failed = await fixture.CertifyAsync(CreateEvaluator);

        Assert.False(failed.IsSuccess);
        Assert.Equal(
            CodeDocumentationCertificationService
                .EvaluationClientUnavailableErrorCode,
            failed.ErrorCode);
        Assert.Equal(1, createCalls);

        var replayCreateCalls = 0;
        var replay = await fixture.CertifyAsync(() =>
        {
            replayCreateCalls++;
            return OperationResult<ILegalTeacherEvaluationInferenceClient>
                .Fail("must not be called", "S03_TEST_REPLAY_FACTORY");
        });

        Assert.False(replay.IsSuccess);
        Assert.Equal(
            CodeDocumentationCertificationSessionStore.ErrorPrefix +
            "INCOMPLETE_EXPOSED",
            replay.ErrorCode);
        Assert.Equal(0, replayCreateCalls);
    }

    [Fact]
    public async Task CertifyAsync_RequestBoundsRejectBeforeStoreOrEvaluator()
    {
        using var fixture = CertificationFixture.Create("request-bounds");
        var createCalls = 0;
        var request = fixture.CreateRequest(() =>
        {
            createCalls++;
            return OperationResult<ILegalTeacherEvaluationInferenceClient>
                .Fail("must not be called", "S03_TEST_BOUNDS_FACTORY");
        });
        var invalidRequests = new[]
        {
            request with { CertificationRunId = new string('r', 246) },
            request with { MaximumDuration = TimeSpan.Zero },
            request with
            {
                MaximumDuration = TimeSpan.FromMilliseconds(int.MaxValue)
                    .Add(TimeSpan.FromMilliseconds(1))
            },
            request with { EvidenceRefs = ["/not-a-safe-evidence-ref"] },
        };
        var storeRoot = Path.Combine(
            fixture.RepositoryRoot,
            "training-artifacts",
            "models",
            CodeDocumentationCertificationSessionStore.DirectoryName);

        foreach (var invalidRequest in invalidRequests)
        {
            var result = await new CodeDocumentationCertificationService()
                .CertifyAsync(invalidRequest);

            Assert.False(result.IsSuccess);
            Assert.Equal(
                CodeDocumentationCertificationService.InvalidErrorCode,
                result.ErrorCode);
            Assert.False(Directory.Exists(storeRoot));
        }

        Assert.Equal(0, createCalls);
    }

    [Fact]
    public async Task CertifyAsync_CompletionReturnFailureFailsClosedThenReplays()
    {
        using var fixture = CertificationFixture.Create("completion-failure");
        using var evaluator = new ScriptedCertificationEvaluator(
            fixture.Authority.Evaluator,
            pass: true);
        var previous = CodeDocumentationCertificationSessionStore
            .FaultInjection
            .Value;
        CodeDocumentationCertificationSessionStore.FaultInjection.Value =
            name =>
            {
                if (name == "completion-before-return")
                    throw new IOException("synthetic completion return failure");
            };

        OperationResult<CodeDocumentationCertificationServiceResult> failed;
        try
        {
            failed = await fixture.CertifyAsync(evaluator);
        }
        finally
        {
            CodeDocumentationCertificationSessionStore.FaultInjection.Value =
                previous;
        }

        Assert.False(failed.IsSuccess);
        Assert.Equal(
            CodeDocumentationCertificationService.CompletionRejectedErrorCode,
            failed.ErrorCode);
        Assert.Equal(10, evaluator.IssuedCallCount);

        using var replayEvaluator =
            new ScriptedCertificationEvaluator(
                fixture.Authority.Evaluator,
                pass: false);
        var replay = await fixture.CertifyAsync(replayEvaluator);
        Assert.True(replay.IsSuccess, replay.ErrorMessage);
        Assert.True(replay.Value!.Replayed);
        Assert.True(replay.Value.Certified);
        Assert.Equal(0, replayEvaluator.IssuedCallCount);
    }

    [Fact]
    public async Task CertifyAsync_CompletionPrePersistFailureFailsClosedWithoutReplay()
    {
        using var fixture = CertificationFixture.Create("completion-pre-persist-failure");
        using var evaluator = new ScriptedCertificationEvaluator(
            fixture.Authority.Evaluator,
            pass: true);
        var previous = CodeDocumentationCertificationSessionStore
            .FaultInjection
            .Value;
        CodeDocumentationCertificationSessionStore.FaultInjection.Value =
            name =>
            {
                if (name == "completed-stage-closed")
                    throw new IOException("synthetic completion pre-persist failure");
            };

        OperationResult<CodeDocumentationCertificationServiceResult> failed;
        try
        {
            failed = await fixture.CertifyAsync(evaluator);
        }
        finally
        {
            CodeDocumentationCertificationSessionStore.FaultInjection.Value =
                previous;
        }

        Assert.False(failed.IsSuccess);
        Assert.Equal(
            CodeDocumentationCertificationService.CompletionRejectedErrorCode,
            failed.ErrorCode);
        Assert.Equal(10, evaluator.IssuedCallCount);

        using var replayEvaluator =
            new ScriptedCertificationEvaluator(
                fixture.Authority.Evaluator,
                pass: false);
        var replay = await fixture.CertifyAsync(replayEvaluator);
        Assert.False(replay.IsSuccess);
        Assert.Equal(
            CodeDocumentationCertificationSessionStore.ErrorPrefix +
            "HISTORY_MISMATCH",
            replay.ErrorCode);
        Assert.Equal(0, replayEvaluator.IssuedCallCount);
    }

    private sealed class CertificationFixture : IDisposable
    {
        private readonly string _base;
        private readonly CodeDocumentationCertificationService _service = new();

        private CertificationFixture(
            string repositoryRoot,
            CodeDocumentationRepositoryOwnedModelCheckpointStartRequest request,
            BpeTokenizer tokenizer,
            ServerOwnedCodeDocumentationCertificationScope authority,
            RepositoryOwnedModelCheckpointManifest parent)
        {
            _base = Path.GetDirectoryName(repositoryRoot)!;
            RepositoryRoot = repositoryRoot;
            StartRequest = request;
            Tokenizer = tokenizer;
            Authority = authority;
            Parent = parent;
        }

        internal string RepositoryRoot { get; }
        internal CodeDocumentationRepositoryOwnedModelCheckpointStartRequest
            StartRequest { get; }
        internal BpeTokenizer Tokenizer { get; }
        internal ServerOwnedCodeDocumentationCertificationScope Authority { get; }
        internal RepositoryOwnedModelCheckpointManifest Parent { get; }
        internal string StoreId => Hex('1');

        internal static CertificationFixture Create(string scope)
        {
            var repositoryRoot = CreateRepositoryRoot(
                "code-documentation-certification-" + scope);
            var modelConfig = CreateModelConfig();
            var tokenizer = CreateTokenizer(modelConfig);
            var compatibility = CreateCompatibility(modelConfig, scope);
            var request =
                new CodeDocumentationRepositoryOwnedModelCheckpointStartRequest(
                    repositoryRoot,
                    CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                        .CSharpDomainSlug,
                    CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                        .DesignPatternSlug,
                    "code-documentation-llm",
                    "v1",
                    modelConfig,
                    compatibility,
                    RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied);

            var parentModel = new LlmModel(modelConfig);
            FillModelWithDeterministicWeights(parentModel, seed: 0.5f);
            var dpoIdentity = CreateSealedCodeDocumentationDpoIdentity(
                repositoryRoot,
                scope);
            var parent = SealModelCheckpoint(
                repositoryRoot,
                parentModel,
                compatibility,
                dpoIdentity,
                step: 1,
                parent: null);
            var passport = CreateAcceptedPassport(
                parent,
                request.MaximumCheckpointBytes);
            var entries = new[]
            {
                Entry(scope + "-add", "Document Add method and its side effects."),
                Entry(scope + "-subtract", "Document Subtract method and its dependencies."),
            };
            var opened = OpenCorpus(repositoryRoot, scope, entries);
            var generation = new CodeDocumentationSavedModelAssessmentGenerationSettings(
                MaxNewTokens: 8,
                Temperature: 0f,
                TopK: 0,
                TopP: 1f,
                Seed: 7,
                MaxHeldOutCases: 10,
                MaxAnswerChars: 256);
            var evaluator = CreateTeacherConfiguration();
            var scopeDeclaration =
                new CodeDocumentationCertificationScopeDeclaration(
                    CodeDocumentationCertificationScopeDeclaration
                        .CurrentSchema,
                    CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                        .CSharpDomainSlug,
                    CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                        .DesignPatternSlug,
                    Subject,
                    "synthetic-certification-scope-" + scope,
                    "v1",
                    CodeDocumentationCertificationScopeDeclaration
                        .CertificationPurpose,
                    Cut,
                    "coverage-v1",
                    [passport.ManifestSha256],
                    [new CertificationOwnerSnapshot(Owner, "v1")],
                    Complete,
                    opened.Corpus.Entries.Select(static entry =>
                        new CertificationSourceBinding(
                            entry.EntryId,
                            entry.SourceEntityFingerprint,
                            Owner,
                            "v1")));
            var exposureIndex = new CodeDocumentationCertificationExposureIndex(
                CodeDocumentationCertificationExposureIndex.CurrentSchema,
                "v1",
                scopeDeclaration,
                passport.ManifestSha256,
                Complete,
                opened.Corpus.Entries.Select(Node));
            var authority =
                ServerOwnedCodeDocumentationCertificationScope
                    .CreateFromServerOwnedSnapshot(
                        scopeDeclaration,
                        passport,
                        opened,
                        generation,
                        evaluator,
                        MeasurementNoisePolicyConfig.Canonical,
                        exposureIndex);
            Assert.True(authority.IsSuccess, authority.ErrorMessage);
            var admitted = CodeDocumentationCertificationAdmission.TryCreate(
                authority.Value!,
                passport,
                opened,
                exposureIndex);
            Assert.True(admitted.IsSuccess, admitted.ErrorMessage);
            return new CertificationFixture(
                repositoryRoot,
                request,
                tokenizer,
                authority.Value!,
                parent);
        }

        internal CodeDocumentationCertificationServiceRequest CreateRequest(
            ILegalTeacherEvaluationInferenceClient evaluator)
            => CreateRequest(
                () => OperationResult<ILegalTeacherEvaluationInferenceClient>
                    .Ok(evaluator));

        internal CodeDocumentationCertificationServiceRequest CreateRequest(
            Func<OperationResult<ILegalTeacherEvaluationInferenceClient>>
                createEvaluationClient)
            => new(
                StartRequest,
                Authority,
                StoreId,
                Tokenizer,
                createEvaluationClient,
                DefaultCertificationRunId(),
                Observed,
                ["test:code-documentation-certification-service"]);

        internal CodeDocumentationCertificationServiceRequest
            CreateDiagnosticOnlyRequest(
                ILegalTeacherEvaluationInferenceClient evaluator)
        {
            var declaration = new CodeDocumentationCertificationScopeDeclaration(
                CodeDocumentationCertificationScopeDeclaration.CurrentSchema,
                Authority.Declaration.Domain,
                Authority.Declaration.DesignPattern,
                Authority.Declaration.Subject,
                Authority.Declaration.ScopeId + "-diagnostic",
                Authority.Declaration.ScopeVersion,
                "teacher-visible-diagnostic-v1",
                Authority.Declaration.CoverageCutUtc,
                Authority.Declaration.CoverageRevision,
                Authority.Declaration.CheckpointLineageManifestIdentities,
                Authority.Declaration.OwnerSnapshots,
                Authority.Declaration.History,
                Authority.Declaration.CertificationSourceBindings);
            var index = new CodeDocumentationCertificationExposureIndex(
                CodeDocumentationCertificationExposureIndex.CurrentSchema,
                Authority.Index.Version,
                declaration,
                Authority.Index.CoveredCheckpointManifestSha256,
                Authority.Index.History,
                Authority.Index.Sources.Select(static source =>
                    new CodeDocumentationCertificationSource(
                        source.SourceEntityFingerprint,
                        source.Domain,
                        source.Subject,
                        source.OwnerSnapshotId,
                        source.OwnerSnapshotVersion,
                        source.SourceInput,
                        source.OriginEvidenceRef,
                        CertificationExposureRole.TeacherVisibleDiagnostic,
                        source.LineageKind,
                        source.Ancestors)));
            var diagnosticAuthority =
                ServerOwnedCodeDocumentationCertificationScope
                    .CreateFromServerOwnedSnapshot(
                        declaration,
                        Authority.Candidate,
                        Authority.Corpus,
                        Authority.Generation,
                        Authority.Evaluator,
                        Authority.Noise,
                        index);
            Assert.True(
                diagnosticAuthority.IsSuccess,
                diagnosticAuthority.ErrorMessage);
            return CreateRequest(evaluator) with
            {
                Authority = diagnosticAuthority.Value!,
                CertificationRunId =
                    "diagnostic-score-run-" + Parent.ManifestSha256[..12],
                EvidenceRefs = ["test:diagnostic-only-score-boundary"],
            };
        }

        internal Task<OperationResult<CodeDocumentationCertificationServiceResult>>
            CertifyAsync(ILegalTeacherEvaluationInferenceClient evaluator)
            => _service.CertifyAsync(CreateRequest(evaluator));

        internal Task<OperationResult<CodeDocumentationCertificationServiceResult>>
            CertifyAsync(
                Func<OperationResult<ILegalTeacherEvaluationInferenceClient>>
                    createEvaluationClient)
            => _service.CertifyAsync(CreateRequest(createEvaluationClient));

        internal CertificationInputSnapshot CaptureCertificationInputs()
        {
            var files = new SortedDictionary<string, string>(
                StringComparer.Ordinal);
            var inputRoots = new SortedSet<string>(StringComparer.Ordinal)
            {
                ContainingDirectory(
                    Parent.DpoDatasetManifest.RepositoryRelativeManifestPath),
                Authority.Corpus.Manifest.RepositoryRelativeCorpusDirectory,
            };
            AddFile(Parent.RepositoryRelativeManifestPath);
            AddFile(Parent.RepositoryRelativeWeightsPath);
            foreach (var inputRoot in inputRoots)
                AddTree(inputRoot);

            return new CertificationInputSnapshot(
                files,
                inputRoots
                    .SelectMany(root => ListTreeDirectories(root).Prepend(root))
                    .Order(StringComparer.Ordinal)
                    .ToArray(),
                ExistingForbiddenOutputPaths());

            void AddFile(string repositoryRelativePath)
            {
                var normalized = NormalizeRelative(repositoryRelativePath);
                files[normalized] = Sha256FileHex(FullPath(normalized));
            }

            void AddTree(string repositoryRelativeRoot)
            {
                var root = FullPath(repositoryRelativeRoot);
                if (!Directory.Exists(root))
                    return;
                foreach (var path in Directory
                             .EnumerateFiles(
                                 root,
                                 "*",
                                 SearchOption.AllDirectories)
                             .Order(StringComparer.Ordinal))
                {
                    AddFile(NormalizeRelative(Path.GetRelativePath(
                        RepositoryRoot,
                        path)));
                }
            }
        }

        internal void AssertCertificationInputsPreserved(
            CertificationInputSnapshot before)
        {
            var after = CaptureCertificationInputs();
            Assert.Equal(before.FileSha256ByRelativePath, after.FileSha256ByRelativePath);
            Assert.Equal(
                before.ObservedInputDirectories,
                after.ObservedInputDirectories);
            var newlyCreatedForbiddenOutputs = after.ForbiddenOutputPathsPresent
                .Except(before.ForbiddenOutputPathsPresent, StringComparer.Ordinal)
                .ToArray();
            Assert.Empty(newlyCreatedForbiddenOutputs);
        }

        internal void CorruptAcceptedWeightsInPlace()
        {
            var path = Path.Combine(
                RepositoryRoot,
                Parent.RepositoryRelativeWeightsPath.Replace(
                    '/',
                    Path.DirectorySeparatorChar));
            var bytes = File.ReadAllBytes(path);
            Assert.NotEmpty(bytes);
            bytes[0] ^= 0x7f;
            File.WriteAllBytes(path, bytes);
        }

        private string FullPath(string repositoryRelativePath)
            => Path.Combine(
                RepositoryRoot,
                NormalizeRelative(repositoryRelativePath).Replace(
                    '/',
                    Path.DirectorySeparatorChar));

        private IReadOnlyList<string> ListTreeDirectories(
            string repositoryRelativeRoot)
        {
            var root = FullPath(repositoryRelativeRoot);
            return Directory.Exists(root)
                ? Directory
                    .EnumerateDirectories(root)
                    .Select(path => NormalizeRelative(Path.GetRelativePath(
                        RepositoryRoot,
                        path)))
                    .Order(StringComparer.Ordinal)
                    .ToArray()
                : Array.Empty<string>();
        }

        private IReadOnlyList<string> ExistingForbiddenOutputPaths()
        {
            var paths = new List<string>();
            AddIfExists(NextCheckpointRootRelativePath());
            AddIfExists(DefaultTrainingAttemptRootRelativePath());
            return paths.Order(StringComparer.Ordinal).ToArray();

            void AddIfExists(string repositoryRelativePath)
            {
                var normalized = NormalizeRelative(repositoryRelativePath);
                var fullPath = FullPath(normalized);
                if (Directory.Exists(fullPath) || File.Exists(fullPath))
                    paths.Add(normalized);
            }
        }

        private string NextCheckpointRootRelativePath()
            => Require(RepositoryOwnedTrainingArtifactLayout
                    .CreateModelCheckpointAddress(
                        RepositoryRoot,
                        StartRequest.DomainSlug,
                        StartRequest.DesignPatternSlug,
                        StartRequest.ModelIdSlug,
                        StartRequest.ModelVersion,
                        Parent.Step + 1))
                .CheckpointRootRelativePath;

        private string DefaultTrainingAttemptRootRelativePath()
            => Require(RepositoryOwnedTrainingArtifactLayout
                    .CreateModelTrainingAttemptAddress(
                        RepositoryRoot,
                        StartRequest.DomainSlug,
                        StartRequest.DesignPatternSlug,
                        StartRequest.ModelIdSlug,
                        StartRequest.ModelVersion,
                        DefaultCertificationRunId()))
                .AttemptRootRelativePath;

        private string DefaultCertificationRunId()
            => "certification-run-" + Parent.ManifestSha256[..12];

        public void Dispose()
        {
            var resolved = Path.GetFullPath(RepositoryRoot);
            Assert.Equal(_base, Path.GetDirectoryName(resolved));
            if (Directory.Exists(resolved))
                Directory.Delete(resolved, recursive: true);
        }
    }

    private sealed record CertificationInputSnapshot(
        IReadOnlyDictionary<string, string> FileSha256ByRelativePath,
        IReadOnlyList<string> ObservedInputDirectories,
        IReadOnlyList<string> ForbiddenOutputPathsPresent);

    private sealed class ScriptedCertificationEvaluator(
        ContinuousLearningTp1DpoConfiguration configuration,
        bool pass,
        Func<int, string>? requestIdFactory = null,
        Action<int>? afterPermit = null)
        : ILegalTeacherEvaluationInferenceClient
    {
        private int _calls;
        private int _issuedCalls;

        internal int IssuedCallCount => Volatile.Read(ref _issuedCalls);

        public Task<OperationResult<TeacherEvaluationInferenceResult>>
            EvaluateAsync(
                string prompt,
                ILegalTeacherCallPermit callPermit,
                CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var payloadStart = prompt.IndexOf('{');
            using var document = JsonDocument.Parse(prompt[payloadStart..]);
            var caseId = document
                .RootElement
                .GetProperty("caseId")
                .GetString()!;
            var call = Interlocked.Increment(ref _calls);
            var issued = callPermit.TryIssue();
            if (!issued.IsSuccess)
                return Task.FromResult(
                    issued.MapError<TeacherEvaluationInferenceResult>());

            Interlocked.Increment(ref _issuedCalls);
            afterPermit?.Invoke(call);
            var output = JsonSerializer.Serialize(new
            {
                caseId,
                pass,
                reason = pass
                    ? CodeDocumentationSavedModelAssessmentService
                        .CorrectReasonCode
                    : CodeDocumentationSavedModelAssessmentService
                        .MissingEssentialBehaviorReasonCode,
            });
            var provenance = new TrainingTeacherProvenance(
                TrainingTeacherSourceKind.ExternalTeacherOutput,
                OpenWeightTeacherFamily.Qwen,
                TrainingTeacherAccessMode.LocalHostedOpenWeight,
                TrainingTeacherHostingProvider.Ollama,
                configuration.ModelId,
                configuration.ExpectedOllamaManifestDigest,
                TrainingTeacherModelLicense.Apache20,
                ProviderTermsChecked: false,
                ProviderTermsAllowTraining: false,
                ClosedProprietaryModel: false,
                TeacherOutputUsedForTraining: false,
                requestIdFactory?.Invoke(call) ??
                "cert-evaluation-" + call.ToString("D4"),
                InvocationCount: 1)
            {
                ModelArtifactDigestKind =
                    LegalTrainingTeacherAdmissionAuthority
                        .OllamaModelLayerSha256DigestKind,
                ModelArtifactDigest =
                    configuration.ExpectedOllamaModelLayerDigest,
            };
            return Task.FromResult(
                OperationResult<TeacherEvaluationInferenceResult>.Ok(
                    new TeacherEvaluationInferenceResult(output, provenance)
                    {
                        InferenceProfile =
                            LocalQwenOllamaInferenceProfileAuthority
                                .Evaluation,
                    }));
        }

        public void Dispose()
        {
        }
    }

    private static RepositoryOwnedReferenceCorpusOpenResult OpenCorpus(
        string repositoryRoot,
        string version,
        IEnumerable<FrozenReferenceEntry> entries)
    {
        var corpus = Require(FrozenReferenceCorpus.Build(
            "synthetic-certification",
            version,
            Cut,
            entries));
        var address = Require(
            RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(
                repositoryRoot,
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .CSharpDomainSlug,
                Subject,
                version));
        var sealedManifest = Require(
            RepositoryOwnedReferenceCorpusManifestSealer.PersistAndSeal(
                address,
                corpus));
        var opened = Require(
            RepositoryOwnedReferenceCorpusManifestSealer.OpenSealed(address));
        Assert.Equal(sealedManifest, opened.Manifest);
        Assert.Equal(
            corpus.CanonicalRepresentation,
            opened.Corpus.CanonicalRepresentation);
        return opened;
    }

    private static FrozenReferenceEntry Entry(string seed, string input)
    {
        var reference = "CLOSED_REFERENCE: " + input;
        var provenance = Require(PaidReferenceProvenance.Create(
            "synthetic-paid-provider",
            "synthetic-paid-model",
            "v1",
            "request-" + seed,
            Cut.AddHours(-1),
            FrozenReferenceEntry.ComputeNormalizedTextSha256(reference)));
        return Require(FrozenReferenceEntry.Create(
            input,
            reference,
            provenance,
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CSharpDomainSlug,
            Subject,
            FrozenReferencePartition.HeldOut,
            Fingerprint(seed)));
    }

    private static CodeDocumentationCertificationSource Node(
        FrozenReferenceEntry entry)
        => new(
            entry.SourceEntityFingerprint,
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CSharpDomainSlug,
            Subject,
            Owner,
            "v1",
            entry.OwnerInput,
            Fingerprint("origin"),
            CertificationExposureRole.ReferenceAcquisitionOnly,
            CertificationLineageKind.KnownRoot,
            []);

    private static CodeDocumentationSavedModelAssessmentModelPassport
        CreateAcceptedPassport(
            RepositoryOwnedModelCheckpointManifest parent,
            long maximumModelSerializationBytes)
        => new(
            parent.Domain,
            parent.DesignPattern,
            parent.ModelId,
            parent.Version,
            parent.Step,
            parent.RepositoryRelativeManifestPath,
            parent.ManifestSha256,
            parent.RepositoryRelativeWeightsPath,
            parent.WeightsSha256,
            parent.WeightsByteLength,
            maximumModelSerializationBytes,
            CodeDocumentationSavedModelAssessmentModelPassport
                .AcceptedRepositoryCheckpointSourceKind,
            parent.RepositoryRelativeManifestPath,
            parent.ManifestSha256);

    private static RepositoryOwnedModelCheckpointManifest SealModelCheckpoint(
        string repositoryRoot,
        LlmModel model,
        RepositoryOwnedModelRuntimeCompatibility compatibility,
        RepositoryOwnedDpoDatasetManifestIdentity dpoIdentity,
        int step,
        RepositoryOwnedModelCheckpointManifest? parent)
    {
        var address = RepositoryOwnedTrainingArtifactLayout
            .CreateModelCheckpointAddress(
                repositoryRoot,
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .CSharpDomainSlug,
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .DesignPatternSlug,
                "code-documentation-llm",
                "v1",
                step);
        Assert.True(address.IsSuccess, address.ErrorMessage);
        var manifestRequest =
            new RepositoryOwnedModelCheckpointManifestRequest(
                ParentCheckpoint: parent is null
                    ? null
                    : new RepositoryOwnedModelCheckpointParentIdentity(
                        parent.RepositoryRelativeManifestPath,
                        parent.Step,
                        parent.WeightsSha256,
                        parent.WeightsByteLength),
                ColdStartReason: parent is null
                    ? "first CodeDocumentation repository-owned checkpoint"
                    : string.Empty,
                DpoDatasetManifest: dpoIdentity,
                RuntimeCompatibility: compatibility,
                CreatedAtUtc: DateTimeOffset.UnixEpoch.AddMinutes(step),
                ProvenanceRefs:
                [
                    "test:code-documentation-certification-service"
                ]);
        var sealedManifest =
            RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(
                address.Value!,
                model.SaveCheckpointToStream,
                manifestRequest);
        Assert.True(sealedManifest.IsSuccess, sealedManifest.ErrorMessage);
        return sealedManifest.Value!;
    }

    private static RepositoryOwnedDpoDatasetManifestIdentity
        CreateSealedCodeDocumentationDpoIdentity(
            string repositoryRoot,
            string scope)
    {
        var address = RepositoryOwnedTrainingArtifactLayout
            .CreateDpoDatasetAddress(
                repositoryRoot,
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .CSharpDomainSlug,
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .DesignPatternSlug,
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .DefaultDatasetVersion);
        Assert.True(address.IsSuccess, address.ErrorMessage);
        var identity = CreateRepositoryOwnedDpoRequestIdentity(scope);
        var pair = CreateRepositoryOwnedDpoPair(identity, scope);
        var manifest = RepositoryOwnedDpoDatasetManifestSealer.PersistAndSeal(
            address.Value!,
            identity,
            [pair]);
        Assert.True(manifest.IsSuccess, manifest.ErrorMessage);
        return new RepositoryOwnedDpoDatasetManifestIdentity(
            address.Value!.ManifestRelativePath,
            manifest.Value!.ManifestSha256);
    }

    private static RepositoryOwnedDpoDatasetRequestIdentity
        CreateRepositoryOwnedDpoRequestIdentity(string scope)
    {
        var identity = RepositoryOwnedDpoDatasetManifestSealer
            .CreateRequestIdentity(
                [Sha256Hex("paid CodeDocumentation reference " + scope)],
                new RepositoryOwnedDpoOpenTeacherIdentity(
                    OpenWeightTeacherFamily.Qwen.ToString(),
                    "qwen-code-doc-test:9b",
                    Hex('c'),
                    LegalTrainingTeacherAdmissionAuthority
                        .OllamaModelLayerSha256DigestKind,
                    Hex('d')),
                new RepositoryOwnedDpoGenerationPolicyIdentity(
                    "code-documentation-certification",
                    "v1",
                    Sha256Hex("generation policy " + scope)),
                new RepositoryOwnedDpoCreatorIdentity(
                    "code-documentation-certification-tests",
                    "v1",
                    Sha256Hex("creator policy " + scope)));
        Assert.True(identity.IsSuccess, identity.ErrorMessage);
        return identity.Value!;
    }

    private static ContinuousLearning.DpoPair CreateRepositoryOwnedDpoPair(
        RepositoryOwnedDpoDatasetRequestIdentity identity,
        string scope)
    {
        var passport = new TrainingTeacherProvenance(
            TrainingTeacherSourceKind.ExternalTeacherOutput,
            OpenWeightTeacherFamily.Qwen,
            TrainingTeacherAccessMode.LocalHostedOpenWeight,
            TrainingTeacherHostingProvider.Ollama,
            identity.OpenTeacher.ModelId,
            identity.OpenTeacher.ModelVersion,
            TrainingTeacherModelLicense.Apache20,
            ProviderTermsChecked: false,
            ProviderTermsAllowTraining: false,
            ClosedProprietaryModel: false,
            TeacherOutputUsedForTraining: true,
            "repository-owned-checkpoint-" + scope,
            InvocationCount: 1)
        {
            ModelArtifactDigestKind =
                identity.OpenTeacher.ModelArtifactDigestKind,
            ModelArtifactDigest = identity.OpenTeacher.ModelArtifactSha256,
        };
        var bound = DpoPairTrainingContentAuthority.Bind(
            new ContinuousLearning.DpoPair(
                "Document a C# service method for " + scope,
                "Explain purpose, dependencies and side effects.",
                "Summarize vaguely without the required evidence.",
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .CSharpDomainSlug,
                DateTimeOffset.UnixEpoch)
            {
                TeacherProvenance = passport,
                GovernanceDecision = new TrainingDataGovernanceDecision(
                    licenseAccepted: true,
                    isQuarantined: false,
                    evidenceRefs: ["sha256:" + Sha256Hex("governance " + scope)]),
                ChosenAuthorship = new DpoPairSideAuthorship(
                    DpoPairSideAuthorKind.OpenTeacher,
                    passport),
                RejectedAuthorship = new DpoPairSideAuthorship(
                    DpoPairSideAuthorKind.LocalModel),
            });
        Assert.True(bound.IsSuccess, bound.ErrorMessage);
        return bound.Value!;
    }

    private static ModelConfig CreateModelConfig()
        => new()
        {
            VocabSize = 256,
            ContextLen = 64,
            HiddenDim = 8,
            NumLayers = 1,
            NumHeads = 1,
            NumKvHeads = 1,
            FfnHiddenDim = 16,
            RopeTheta = 10_000f,
            RmsNormEps = 1e-5f,
        };

    private static BpeTokenizer CreateTokenizer(ModelConfig config)
    {
        var tokenizer = new BpeTokenizer();
        tokenizer.Train(
            "Document Add method. Document Subtract method. dependencies side effects api reference",
            config.VocabSize);
        return tokenizer;
    }

    private static RepositoryOwnedModelRuntimeCompatibility
        CreateCompatibility(ModelConfig config, string keySuffix)
        => new(
            TrainingSourceProvenance.Identify(config).Sha256,
            Hex('2'),
            Hex('3'),
            Hex('4'),
            Hex('5'),
            "code-documentation-certification-" + keySuffix);

    private static ContinuousLearningTp1DpoConfiguration
        CreateTeacherConfiguration()
    {
        var created = ContinuousLearningTp1DpoConfiguration.Create(
            "qwen-assessment-test:9b",
            Hex('a'),
            Hex('b'),
            maxTeacherCalls: 50,
            "sha256:" + Hex('c'),
            "sha256:" + Hex('d'),
            Enumerable.Range(1, 32)
                .Select(static value => checked((byte)value))
                .ToArray());
        Assert.True(created.IsSuccess, created.ErrorMessage);
        return created.Value!;
    }

    private static void FillModelWithDeterministicWeights(
        LlmModel model,
        float seed)
    {
        var parameterIndex = 0;
        foreach (var parameter in model.BaseParameters)
        {
            var data = parameter.Value.Data;
            for (var valueIndex = 0; valueIndex < data.Length; valueIndex++)
                data[valueIndex] = seed + parameterIndex + valueIndex * 0.125f;
            parameterIndex++;
        }
    }

    private static string CreateRepositoryRoot(string scope)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            scope + "-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        File.WriteAllText(
            Path.Combine(root, "LlmFromScratch.sln"),
            "fixture solution sentinel");
        Directory.CreateDirectory(Path.Combine(root, "14.ContinuousLearning"));
        Directory.CreateDirectory(Path.Combine(root, "11.DPO"));
        return root;
    }

    private static string Fingerprint(string seed)
        => FrozenReferenceEntry.ComputeNormalizedTextSha256("source:" + seed);

    private static T Require<T>(OperationResult<T> result)
    {
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.NotNull(result.Value);
        return result.Value!;
    }

    private static string Hex(char value) => new(value, 64);

    private static string NormalizeRelative(string path)
        => path.Replace('\\', '/');

    private static string ContainingDirectory(string repositoryRelativePath)
    {
        var normalized = NormalizeRelative(repositoryRelativePath);
        var index = normalized.LastIndexOf('/');
        Assert.True(index > 0, "Expected repository-relative child path.");
        return normalized[..index];
    }

    private static string Sha256FileHex(string path)
        => Convert
            .ToHexString(SHA256.HashData(File.ReadAllBytes(path)))
            .ToLowerInvariant();

    private static string Sha256Hex(string value)
        => Convert
            .ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant();
}
