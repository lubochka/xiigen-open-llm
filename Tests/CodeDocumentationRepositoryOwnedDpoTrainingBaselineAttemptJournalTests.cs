// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text.Json;
using AgentInfrastructure;
using ContinuousLearning;
using DPO.Models;
using Tokenizer;
using TrainableRouter.Training;
using TransformerModel;
using TransformerModel.Config;

namespace LlmFromScratch.Tests;

/// <summary>
/// Прямые тесты журнала baseline.json. Координатор проверяет весь маршрут,
/// а здесь проверяется только запись и повторное открытие исходной записи
/// попытки: обе половины должны работать на Linux, а не только запись.
/// </summary>
public sealed class CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournalTests
{
    private const string QualitySubjectSlug =
        "csharp-documentation-quality-fixture";
    private const string QualityCorpusVersion =
        "v20260909-quality-gate";

    // Same value the CodeDocumentation START boundary uses by default, so the
    // accepted passport carries the production serialization ceiling.
    private const long DefaultMaximumCheckpointBytes =
        16L * 1024L * 1024L * 1024L;

    [Fact]
    public async Task WriteAndReadBack_ValidSealedAttemptWritesOwnerOnlyBaselineAndReadsItBack()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var repositoryRoot = CreateRepositoryRoot("code-doc-journal-round-trip");
        try
        {
            var staged = await StageAttemptAsync(
                fixture,
                repositoryRoot,
                "round-trip");

            var written = CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                .WriteAndReadBack(staged.AttemptAddress, staged.State);

            Assert.True(
                written.IsSuccess,
                $"{written.ErrorCode}: {written.ErrorMessage}");
            Assert.NotNull(written.Value);
            // The write half and the reopen half must agree on one single file.
            Assert.True(File.Exists(staged.BaselineStatePath));
            AssertReadbackMatchesStagedAttempt(written.Value!, staged);
            if (OperatingSystem.IsLinux())
            {
                // 0600 exactly: no group and no other access on the reopened file.
                Assert.Equal(
                    SecureLocalFileBoundary.OwnerFileMode,
                    File.GetUnixFileMode(staged.BaselineStatePath));
            }

            Assert.True(SecureLocalFileBoundary.HasOwnerOnlyFileMode(
                staged.BaselineStatePath));
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public async Task OpenExisting_AfterSuccessfulWriteReturnsEquivalentStateAndSameHash()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var repositoryRoot = CreateRepositoryRoot("code-doc-journal-open-existing");
        try
        {
            var staged = await StageAttemptAsync(
                fixture,
                repositoryRoot,
                "open-existing");
            var written = CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                .WriteAndReadBack(staged.AttemptAddress, staged.State);
            Assert.True(
                written.IsSuccess,
                $"{written.ErrorCode}: {written.ErrorMessage}");

            var reopened = CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                .OpenExisting(staged.AttemptAddress);

            Assert.True(
                reopened.IsSuccess,
                $"{reopened.ErrorCode}: {reopened.ErrorMessage}");
            Assert.NotNull(reopened.Value);
            AssertReadbackMatchesStagedAttempt(reopened.Value!, staged);
            // Reopening must not re-hash different bytes than the write recorded.
            Assert.Equal(
                written.Value!.BaselineStateSha256,
                reopened.Value!.BaselineStateSha256);
            Assert.Equal(
                written.Value.BaselineStateByteLength,
                reopened.Value.BaselineStateByteLength);
            Assert.Equal(
                written.Value.BaselineStateRelativePath,
                reopened.Value.BaselineStateRelativePath);
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public async Task OpenExisting_MissingBaselineStateIsUnavailableAndCreatesNoFile()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var repositoryRoot = CreateRepositoryRoot("code-doc-journal-missing");
        try
        {
            var staged = await StageAttemptAsync(
                fixture,
                repositoryRoot,
                "missing");
            Assert.False(File.Exists(staged.BaselineStatePath));

            var reopened = CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                .OpenExisting(staged.AttemptAddress);

            Assert.False(reopened.IsSuccess);
            Assert.Equal(
                CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                    .UnavailableErrorCode,
                reopened.ErrorCode);
            // A read must never bring the file into existence.
            Assert.False(File.Exists(staged.BaselineStatePath));
            Assert.Empty(Directory.EnumerateFileSystemEntries(
                staged.AttemptAddress.AttemptRoot));
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Theory]
    [InlineData("non-json")]
    [InlineData("truncated-json")]
    [InlineData("unmapped-member")]
    [InlineData("empty")]
    public async Task OpenExisting_MalformedBaselineStateIsInvalid(string payloadKind)
    {
        using var fixture = new TinyTrainingFixtureCL();
        var repositoryRoot = CreateRepositoryRoot(
            "code-doc-journal-malformed-" + payloadKind);
        try
        {
            var staged = await StageAttemptAsync(
                fixture,
                repositoryRoot,
                "malformed-" + payloadKind);
            var payload = payloadKind switch
            {
                "non-json" => "this baseline state is not json at all"u8.ToArray(),
                "truncated-json" => "{\"FormatVersion\":1,\"TrainingRunId\":"u8
                    .ToArray(),
                "unmapped-member" => "{\"NotABaselineMember\":1}"u8.ToArray(),
                "empty" => Array.Empty<byte>(),
                _ => throw new ArgumentOutOfRangeException(nameof(payloadKind)),
            };
            WriteOwnerOnlyBytes(staged.BaselineStatePath, payload);

            var reopened = CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                .OpenExisting(staged.AttemptAddress);

            Assert.False(reopened.IsSuccess);
            // Unreadable content is a content fault, not a storage outage.
            Assert.Equal(
                CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                    .InvalidErrorCode,
                reopened.ErrorCode);
            Assert.Null(reopened.Value);
            // Failing closed must leave the operator's bytes untouched.
            Assert.Equal(payload, File.ReadAllBytes(staged.BaselineStatePath));
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Theory]
    [InlineData("foreign-training-run-id")]
    [InlineData("foreign-attempt-root-relative-path")]
    [InlineData("foreign-attempt-claim-relative-path")]
    public async Task WriteAndReadBack_ForeignAttemptIdentityIsInvalidAndCreatesNoFile(
        string mutation)
    {
        using var fixture = new TinyTrainingFixtureCL();
        var repositoryRoot = CreateRepositoryRoot(
            "code-doc-journal-foreign-" + mutation);
        try
        {
            var staged = await StageAttemptAsync(
                fixture,
                repositoryRoot,
                "foreign-" + mutation);
            var foreignState = mutation switch
            {
                "foreign-training-run-id" => staged.State with
                {
                    TrainingRunId = staged.State.TrainingRunId + ":foreign",
                },
                "foreign-attempt-root-relative-path" => staged.State with
                {
                    AttemptRootRelativePath =
                        staged.State.AttemptRootRelativePath + "-foreign",
                },
                "foreign-attempt-claim-relative-path" => staged.State with
                {
                    AttemptClaimRelativePath =
                        staged.State.AttemptClaimRelativePath + "-foreign",
                },
                _ => throw new ArgumentOutOfRangeException(nameof(mutation)),
            };

            var written = CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                .WriteAndReadBack(staged.AttemptAddress, foreignState);

            Assert.False(written.IsSuccess);
            Assert.Equal(
                CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                    .InvalidErrorCode,
                written.ErrorCode);
            // A foreign identity is rejected before anything is persisted.
            Assert.False(File.Exists(staged.BaselineStatePath));
            Assert.Empty(Directory.EnumerateFileSystemEntries(
                staged.AttemptAddress.AttemptRoot));
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public async Task WriteAndReadBack_ExistingBaselineStateIsNeverSilentlyOverwritten()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var repositoryRoot = CreateRepositoryRoot("code-doc-journal-no-overwrite");
        try
        {
            var staged = await StageAttemptAsync(
                fixture,
                repositoryRoot,
                "no-overwrite");
            var sentinel = "already sealed baseline state for this attempt"u8
                .ToArray();
            WriteOwnerOnlyBytes(staged.BaselineStatePath, sentinel);

            var written = CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                .WriteAndReadBack(staged.AttemptAddress, staged.State);

            Assert.False(written.IsSuccess);
            Assert.Equal(
                CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                    .UnavailableErrorCode,
                written.ErrorCode);
            // The already sealed record of the attempt survives byte for byte.
            Assert.Equal(sentinel, File.ReadAllBytes(staged.BaselineStatePath));
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public async Task OpenExisting_GroupReadableBaselineStateIsUnavailable()
    {
        if (!OperatingSystem.IsLinux())
            return;

        using var fixture = new TinyTrainingFixtureCL();
        var repositoryRoot = CreateRepositoryRoot("code-doc-journal-group-readable");
        try
        {
            var staged = await StageAttemptAsync(
                fixture,
                repositoryRoot,
                "group-readable");
            var written = CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                .WriteAndReadBack(staged.AttemptAddress, staged.State);
            Assert.True(
                written.IsSuccess,
                $"{written.ErrorCode}: {written.ErrorMessage}");
            var sealedBytes = File.ReadAllBytes(staged.BaselineStatePath);
            File.SetUnixFileMode(
                staged.BaselineStatePath,
                SecureLocalFileBoundary.OwnerFileMode | UnixFileMode.GroupRead);

            var reopened = CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                .OpenExisting(staged.AttemptAddress);

            Assert.False(reopened.IsSuccess);
            // The owner-only guard still protects the reopen half, not just the write.
            Assert.Equal(
                CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                    .UnavailableErrorCode,
                reopened.ErrorCode);
            Assert.False(SecureLocalFileBoundary.HasOwnerOnlyFileMode(
                staged.BaselineStatePath));
            Assert.Equal(sealedBytes, File.ReadAllBytes(staged.BaselineStatePath));
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public async Task WriteAndReadBack_ReparsePointBaselineStateSegmentIsUnavailable()
    {
        if (!OperatingSystem.IsLinux())
            return;

        using var fixture = new TinyTrainingFixtureCL();
        var repositoryRoot = CreateRepositoryRoot("code-doc-journal-reparse-point");
        try
        {
            var staged = await StageAttemptAsync(
                fixture,
                repositoryRoot,
                "reparse-point");
            // baseline.json itself is planted as a dangling symlink pointing out
            // of the attempt root. Nothing exists at either end, so only the
            // reparse-point guard can stop the write; without it FileMode.CreateNew
            // would follow the link and create the target outside the attempt.
            var linkTarget = Path.Combine(
                repositoryRoot,
                "journal-symlink-escape-target.json");
            File.CreateSymbolicLink(staged.BaselineStatePath, linkTarget);
            Assert.False(File.Exists(linkTarget));

            var written = CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                .WriteAndReadBack(staged.AttemptAddress, staged.State);

            Assert.False(written.IsSuccess);
            Assert.Equal(
                CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                    .UnavailableErrorCode,
                written.ErrorCode);
            // No bytes escaped through the link.
            Assert.False(File.Exists(linkTarget));

            var reopened = CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                .OpenExisting(staged.AttemptAddress);

            Assert.False(reopened.IsSuccess);
            Assert.Equal(
                CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                    .UnavailableErrorCode,
                reopened.ErrorCode);
            Assert.False(File.Exists(linkTarget));
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public async Task PreCancelledTokenThrowsOperationCanceledExceptionAndNeverSucceeds()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var repositoryRoot = CreateRepositoryRoot("code-doc-journal-cancelled");
        try
        {
            var staged = await StageAttemptAsync(
                fixture,
                repositoryRoot,
                "cancelled");
            using var cancellation = new CancellationTokenSource();
            await cancellation.CancelAsync();

            // Cancellation is observed as an exception, never as a result object.
            Assert.ThrowsAny<OperationCanceledException>(() =>
                CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                    .WriteAndReadBack(
                        staged.AttemptAddress,
                        staged.State,
                        cancellation.Token));
            Assert.False(File.Exists(staged.BaselineStatePath));
            Assert.Empty(Directory.EnumerateFileSystemEntries(
                staged.AttemptAddress.AttemptRoot));

            Assert.ThrowsAny<OperationCanceledException>(() =>
                CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                    .OpenExisting(
                        staged.AttemptAddress,
                        cancellation.Token));
            Assert.False(File.Exists(staged.BaselineStatePath));
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    private static void AssertReadbackMatchesStagedAttempt(
        CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournalReadback
            readback,
        StagedJournalAttempt staged)
    {
        Assert.Equal(staged.BaselineStatePath, readback.BaselineStatePath);
        Assert.Equal(
            staged.AttemptAddress.AttemptRootRelativePath + "/" +
            CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                .BaselineStateFileName,
            readback.BaselineStateRelativePath);
        Assert.Equal(
            ComputeFileSha256(staged.BaselineStatePath),
            readback.BaselineStateSha256);
        Assert.Equal(
            new FileInfo(staged.BaselineStatePath).Length,
            readback.BaselineStateByteLength);
        Assert.True(readback.BaselineStateByteLength > 0);

        var state = readback.State;
        Assert.Equal(
            CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                .CurrentFormatVersion,
            state.FormatVersion);
        Assert.Equal(staged.State.TrainingRunId, state.TrainingRunId);
        Assert.Equal(staged.State.TrainingRunIdSha256, state.TrainingRunIdSha256);
        Assert.Equal(
            staged.State.AttemptRootRelativePath,
            state.AttemptRootRelativePath);
        Assert.Equal(
            staged.State.AttemptClaimRelativePath,
            state.AttemptClaimRelativePath);
        Assert.Equal(staged.State.DomainSlug, state.DomainSlug);
        Assert.Equal(staged.State.DesignPatternSlug, state.DesignPatternSlug);
        Assert.Equal(staged.State.ModelIdSlug, state.ModelIdSlug);
        Assert.Equal(staged.State.ModelVersion, state.ModelVersion);
        Assert.Equal(
            staged.State.AcceptedModelPassport,
            state.AcceptedModelPassport);
        Assert.Equal(
            staged.State.DpoManifestRelativePath,
            state.DpoManifestRelativePath);
        Assert.Equal(staged.State.DpoManifestSha256, state.DpoManifestSha256);
        Assert.Equal(staged.State.TrainingPairId, state.TrainingPairId);
        Assert.Equal(
            staged.State.TrainingPairRelativePath,
            state.TrainingPairRelativePath);
        Assert.Equal(staged.State.TrainingPairSha256, state.TrainingPairSha256);
        Assert.Equal(
            staged.State.BaselineQuality.ExpectedEvaluationCallCount,
            state.BaselineQuality.ExpectedEvaluationCallCount);
        Assert.Equal(
            staged.State.BaselineQuality.ObservedEvaluationCallCount,
            state.BaselineQuality.ObservedEvaluationCallCount);
    }

    private static async Task<StagedJournalAttempt> StageAttemptAsync(
        TinyTrainingFixtureCL fixture,
        string repositoryRoot,
        string scope)
    {
        var tokenizer = BpeTokenizer.Load(fixture.TokenizerPath);
        var compatibility = CreateCompatibility(fixture.ModelConfig, scope);
        var parentDpoSetup = SealRepositoryOwnedDpo(
            repositoryRoot,
            "code-doc-journal-" + scope + "-parent",
            datasetVersion: "v0-parent");
        var dpoSetup = SealRepositoryOwnedDpo(
            repositoryRoot,
            "code-doc-journal-" + scope + "-new",
            datasetVersion: "v1-new");
        var qualityCorpus = SealQualityReferenceCorpus(repositoryRoot);
        var session = fixture.CreateFactory().Create("CODE").Value!;
        var parentManifest = SealModelCheckpoint(
            repositoryRoot,
            session.Model,
            fixture.ModelConfig,
            compatibility,
            parentDpoSetup.Identity,
            step: 1,
            coldReason: "test parent for CodeDocumentation DPO baseline journal");
        var trainingRunId = "code-doc-journal:" + scope;
        var attemptAddress = CreateModelTrainingAttemptAddress(
            repositoryRoot,
            trainingRunId);
        var acceptedPassport = CreateAcceptedPassportForTest(
            parentManifest,
            DefaultMaximumCheckpointBytes);

        // A real measured baseline exam, because the journal refuses any state
        // whose baseline quality does not match the accepted model.
        var baselineQuality =
            await new CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService()
                .CalibrateBaselineAsync(
                    CreateQualityGateContext(
                        qualityCorpus,
                        tokenizer,
                        CreateQualityOutcomes(
                            baselinePass: false,
                            trialPass: true)),
                    session.Model,
                    acceptedPassport,
                    trainingRunId + ":baseline",
                    CancellationToken.None)
                .WaitAsync(TimeSpan.FromSeconds(30));
        Assert.True(baselineQuality.IsSuccess, baselineQuality.ErrorMessage);
        Assert.NotNull(baselineQuality.Value);

        // The executor owns the attempt directory; the journal only writes
        // inside an already claimed owner-only root.
        SecureLocalFileBoundary.CreateOwnerOnlyDirectory(
            attemptAddress.AttemptRoot);

        return new StagedJournalAttempt(
            attemptAddress,
            CreateBaselineAttemptStateForTest(
                attemptAddress,
                acceptedPassport,
                dpoSetup,
                baselineQuality.Value!),
            Path.Combine(
                attemptAddress.AttemptRoot,
                CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                    .BaselineStateFileName));
    }

    private static CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptState
        CreateBaselineAttemptStateForTest(
            RepositoryOwnedModelTrainingAttemptAddress attemptAddress,
            CodeDocumentationSavedModelAssessmentModelPassport acceptedPassport,
            RepositoryOwnedCodeDocumentationDpoSetup dpoSetup,
            CodeDocumentationRepositoryOwnedDpoTrainingBaselineQuality
                baselineQuality)
    {
        var trainingPair = dpoSetup.Receipt.OrderedPairs[0];
        return new CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptState(
            CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                .CurrentFormatVersion,
            attemptAddress.TrainingRunId,
            attemptAddress.TrainingRunIdSha256,
            attemptAddress.AttemptRootRelativePath,
            attemptAddress.AttemptClaimRelativePath,
            attemptAddress.DomainSlug,
            attemptAddress.DesignPatternSlug,
            attemptAddress.ModelIdSlug,
            attemptAddress.ModelVersion,
            acceptedPassport,
            dpoSetup.Address.ManifestRelativePath,
            dpoSetup.Receipt.ManifestSha256,
            trainingPair.PairId,
            trainingPair.RepositoryRelativePairFilePath,
            trainingPair.PairFileSha256,
            baselineQuality);
    }

    private static CodeDocumentationSavedModelAssessmentModelPassport
        CreateAcceptedPassportForTest(
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

    private static RepositoryOwnedModelTrainingAttemptAddress
        CreateModelTrainingAttemptAddress(
            string repositoryRoot,
            string trainingRunId)
    {
        var address = RepositoryOwnedTrainingArtifactLayout
            .CreateModelTrainingAttemptAddress(
                repositoryRoot,
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .CSharpDomainSlug,
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .DesignPatternSlug,
                "code-documentation-llm",
                "v1",
                trainingRunId);
        Assert.True(address.IsSuccess, address.ErrorMessage);
        return address.Value!;
    }

    private static RepositoryOwnedModelCheckpointManifest SealModelCheckpoint(
        string repositoryRoot,
        LlmModel model,
        ModelConfig modelConfig,
        RepositoryOwnedModelRuntimeCompatibility compatibility,
        RepositoryOwnedDpoDatasetManifestIdentity dpoIdentity,
        int step,
        string coldReason)
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
        Assert.Equal(
            TrainingSourceProvenance.Identify(modelConfig).Sha256,
            compatibility.ConfigSha256);

        var manifest = RepositoryOwnedModelCheckpointManifestSealer
            .PersistAndSeal(
                address.Value!,
                model.SaveCheckpointToStream,
                new RepositoryOwnedModelCheckpointManifestRequest(
                    null,
                    coldReason,
                    dpoIdentity,
                    compatibility,
                    DateTimeOffset.UnixEpoch.AddMinutes(step),
                    ["test:seed-code-documentation-journal-parent"]));
        Assert.True(manifest.IsSuccess, manifest.ErrorMessage);
        return manifest.Value!;
    }

    private static CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext
        CreateQualityGateContext(
            RepositoryOwnedReferenceCorpusOpenResult corpus,
            BpeTokenizer tokenizer,
            IReadOnlyList<bool> scriptedOutcomes)
    {
        var evaluatorConfiguration = CreateTeacherConfiguration(
            maxTeacherCalls: 50);
        return new CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext(
            corpus.Corpus,
            corpus.Manifest,
            tokenizer,
            new ScriptedJournalQualityGateEvaluationClient(
                evaluatorConfiguration,
                scriptedOutcomes),
            evaluatorConfiguration,
            new CodeDocumentationSavedModelAssessmentGenerationSettings(
                MaxNewTokens: 1,
                Temperature: 0f,
                TopK: 1,
                TopP: 1f,
                Seed: 17,
                MaxHeldOutCases: 5,
                MaxAnswerChars: 256),
            MeasurementNoisePolicyConfig.Canonical,
            TimeSpan.FromSeconds(10),
            DateTimeOffset.UnixEpoch.AddHours(1),
            ["test:code-documentation-baseline-journal-quality-gate"]);
    }

    private static bool[] CreateQualityOutcomes(
        bool baselinePass,
        bool trialPass)
    {
        const int heldOutCases = 5;
        var callsPerSeries =
            heldOutCases * MeasurementNoisePolicyConfig.Canonical.RepeatCount;
        return Enumerable
            .Repeat(baselinePass, callsPerSeries)
            .Concat(Enumerable.Repeat(trialPass, callsPerSeries))
            .ToArray();
    }

    private static RepositoryOwnedReferenceCorpusOpenResult
        SealQualityReferenceCorpus(string repositoryRoot)
    {
        var entries = Enumerable
            .Range(1, 5)
            .Select(index => CreateQualityReferenceEntry(
                "case " + index.ToString("D3"),
                "ok " + index.ToString("D3"),
                FrozenReferencePartition.HeldOut,
                "heldout-" + index.ToString("D3")))
            .Prepend(CreateQualityReferenceEntry(
                "train 000",
                "ok train",
                FrozenReferencePartition.Train,
                "train"))
            .ToArray();
        var corpus = FrozenReferenceCorpus.Build(
            "csharp-documentation-quality-gate-fixture",
            QualityCorpusVersion,
            DateTimeOffset.UnixEpoch,
            entries);
        Assert.True(corpus.IsSuccess, corpus.ErrorMessage);

        var address = RepositoryOwnedTrainingArtifactLayout
            .CreateReferenceCorpusAddress(
                repositoryRoot,
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .CSharpDomainSlug,
                QualitySubjectSlug,
                QualityCorpusVersion);
        Assert.True(address.IsSuccess, address.ErrorMessage);

        var sealedManifest =
            RepositoryOwnedReferenceCorpusManifestSealer.PersistAndSeal(
                address.Value!,
                corpus.Value!);
        Assert.True(sealedManifest.IsSuccess, sealedManifest.ErrorMessage);

        var opened = RepositoryOwnedReferenceCorpusManifestSealer.OpenSealed(
            address.Value!);
        Assert.True(opened.IsSuccess, opened.ErrorMessage);
        return opened.Value!;
    }

    private static FrozenReferenceEntry CreateQualityReferenceEntry(
        string ownerInput,
        string referenceAnswer,
        FrozenReferencePartition partition,
        string scope)
    {
        var provenance = PaidReferenceProvenance.Create(
            "paid-reference-quality-fixture",
            "closed-doc-quality-reference-fixture",
            "2026-09-09",
            "quality-reference-request-" + scope,
            DateTimeOffset.UnixEpoch,
            FrozenReferenceEntry.ComputeNormalizedTextSha256(referenceAnswer));
        Assert.True(provenance.IsSuccess, provenance.ErrorMessage);
        var entry = FrozenReferenceEntry.Create(
            ownerInput,
            referenceAnswer,
            provenance.Value!,
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CSharpDomainSlug,
            QualitySubjectSlug,
            partition,
            "sha256:" + Sha256Hex("quality-source-" + scope));
        Assert.True(entry.IsSuccess, entry.ErrorMessage);
        return entry.Value!;
    }

    private static RepositoryOwnedCodeDocumentationDpoSetup SealRepositoryOwnedDpo(
        string repositoryRoot,
        string scope,
        string datasetVersion)
    {
        var address = RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(
            repositoryRoot,
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CSharpDomainSlug,
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.DesignPatternSlug,
            datasetVersion);
        Assert.True(address.IsSuccess, address.ErrorMessage);

        var identity = CreateRepositoryOwnedDpoRequestIdentity(scope);
        var pair = CreateRepositoryOwnedDpoPair(identity, scope);
        var manifest = RepositoryOwnedDpoDatasetManifestSealer.PersistAndSeal(
            address.Value!,
            identity,
            [pair]);
        Assert.True(manifest.IsSuccess, manifest.ErrorMessage);
        var dpoAddress = address.Value!;
        var dpoManifest = manifest.Value!;

        var receipt = new CodeDocumentationRepositoryOwnedDpoDatasetReceipt(
            RepositoryOwnedDpoDatasetResolutionSource.NewlyPersistedRepositoryDataset,
            dpoManifest.ManifestSha256,
            dpoAddress.ManifestRelativePath,
            dpoManifest.PairCount,
            dpoManifest.RequestIdentity.RequestIdentitySha256,
            dpoManifest.OrderedPairs.ToArray());
        return new RepositoryOwnedCodeDocumentationDpoSetup(
            dpoAddress,
            new RepositoryOwnedDpoDatasetManifestIdentity(
                dpoAddress.ManifestRelativePath,
                dpoManifest.ManifestSha256),
            receipt);
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
                    "code-documentation-baseline-journal",
                    "v1",
                    Sha256Hex("generation policy " + scope)),
                new RepositoryOwnedDpoCreatorIdentity(
                    "code-documentation-baseline-journal-tests",
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
            "repository-owned-baseline-journal-" + scope,
            InvocationCount: 1)
        {
            ModelArtifactDigestKind = identity.OpenTeacher.ModelArtifactDigestKind,
            ModelArtifactDigest = identity.OpenTeacher.ModelArtifactSha256
        };

        var bound = DpoPairTrainingContentAuthority.Bind(
            new ContinuousLearning.DpoPair(
                "Document the repository-owned code path.",
                "Use the saved DPO dataset and publish the trained model.",
                "Recreate the DPO dataset and drop the trained model.",
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .CSharpDomainSlug,
                DateTimeOffset.UnixEpoch)
            {
                TeacherProvenance = passport,
                GovernanceDecision = new TrainingDataGovernanceDecision(
                    licenseAccepted: true,
                    isQuarantined: false,
                    evidenceRefs:
                    [
                        "sha256:" + Sha256Hex("governance " + scope)
                    ]),
                ChosenAuthorship = new DpoPairSideAuthorship(
                    DpoPairSideAuthorKind.OpenTeacher,
                    passport),
                RejectedAuthorship = new DpoPairSideAuthorship(
                    DpoPairSideAuthorKind.LocalModel)
            });
        Assert.True(bound.IsSuccess, bound.ErrorMessage);
        return bound.Value!;
    }

    private static ContinuousLearningTp1DpoConfiguration
        CreateTeacherConfiguration(int maxTeacherCalls)
    {
        var created = ContinuousLearningTp1DpoConfiguration.Create(
            "qwen-code-doc-quality-test:9b",
            Hex('a'),
            Hex('b'),
            maxTeacherCalls,
            "sha256:" + Hex('c'),
            "sha256:" + Hex('d'),
            Enumerable.Range(1, 32)
                .Select(static value => checked((byte)value))
                .ToArray());
        Assert.True(created.IsSuccess, created.ErrorMessage);
        return created.Value!;
    }

    private static RepositoryOwnedModelRuntimeCompatibility CreateCompatibility(
        ModelConfig config,
        string scope)
        => new(
            TrainingSourceProvenance.Identify(config).Sha256,
            Sha256Hex("topology " + scope),
            Sha256Hex("tokenizer " + scope),
            Sha256Hex("trainer " + scope),
            Sha256Hex("schema " + scope),
            "code-documentation-journal-" + scope);

    private static string CreateRepositoryRoot(string scope)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            $"{scope}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        Directory.CreateDirectory(Path.Combine(root, "14.ContinuousLearning"));
        Directory.CreateDirectory(Path.Combine(root, "11.DPO"));
        File.WriteAllText(Path.Combine(root, "LlmFromScratch.sln"), string.Empty);
        return root;
    }

    private static void WriteOwnerOnlyBytes(string path, byte[] bytes)
    {
        File.WriteAllBytes(path, bytes);
        if (OperatingSystem.IsLinux())
            File.SetUnixFileMode(path, SecureLocalFileBoundary.OwnerFileMode);
    }

    private static string ComputeFileSha256(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static string Sha256Hex(string value)
        => Convert.ToHexString(
                SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant();

    private static string Hex(char character)
        => new(character, 64);

    private static void DeleteDirectoryIfExists(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);
    }

    private sealed record StagedJournalAttempt(
        RepositoryOwnedModelTrainingAttemptAddress AttemptAddress,
        CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptState State,
        string BaselineStatePath);

    private sealed record RepositoryOwnedCodeDocumentationDpoSetup(
        RepositoryOwnedDpoDatasetAddress Address,
        RepositoryOwnedDpoDatasetManifestIdentity Identity,
        CodeDocumentationRepositoryOwnedDpoDatasetReceipt Receipt);

    private sealed class ScriptedJournalQualityGateEvaluationClient(
        ContinuousLearningTp1DpoConfiguration configuration,
        IReadOnlyList<bool> scriptedOutcomes)
        : ILegalTeacherEvaluationInferenceClient
    {
        private readonly object _gate = new();
        private readonly Queue<bool> _scriptedOutcomes = new(scriptedOutcomes);
        private int _calls;

        public Task<OperationResult<TeacherEvaluationInferenceResult>>
            EvaluateAsync(
                string prompt,
                ILegalTeacherCallPermit callPermit,
                CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var issued = callPermit.TryIssue();
            if (!issued.IsSuccess)
                return Task.FromResult(
                    issued.MapError<TeacherEvaluationInferenceResult>());

            bool pass;
            int callNumber;
            lock (_gate)
            {
                callNumber = ++_calls;
                if (!_scriptedOutcomes.TryDequeue(out pass))
                {
                    return Task.FromResult(
                        OperationResult<TeacherEvaluationInferenceResult>.Fail(
                            "Scripted quality gate checker ran out of decisions.",
                            "SCRIPTED_QUALITY_GATE_DECISION_MISSING"));
                }
            }

            var payloadStart = prompt.IndexOf('{');
            using var document = JsonDocument.Parse(prompt[payloadStart..]);
            var caseId = document.RootElement.GetProperty("caseId").GetString()!;
            var requestId = "journal-quality-gate-evaluation-" +
                            callNumber.ToString("D4");
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
                requestId,
                InvocationCount: 1)
            {
                ModelArtifactDigestKind =
                    LegalTrainingTeacherAdmissionAuthority
                        .OllamaModelLayerSha256DigestKind,
                ModelArtifactDigest =
                    configuration.ExpectedOllamaModelLayerDigest
            };
            return Task.FromResult(
                OperationResult<TeacherEvaluationInferenceResult>.Ok(
                    new TeacherEvaluationInferenceResult(output, provenance)
                    {
                        InferenceProfile =
                            LocalQwenOllamaInferenceProfileAuthority.Evaluation
                    }));
        }

        public void Dispose()
        {
        }
    }
}
