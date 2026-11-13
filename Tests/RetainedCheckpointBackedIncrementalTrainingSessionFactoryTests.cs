// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using ContinuousLearning;
using LlmFromScratch.Tests;
using LoRA;
using Tokenizer;
using TensorCore.Layers;
using TrainingLoop;
using TransformerModel;
using TransformerModel.Config;
using DpoPairSideAuthorKind = DPO.Models.DpoPairSideAuthorKind;

[Trait("Platform", "Windows")]
public sealed class RetainedCheckpointBackedIncrementalTrainingSessionFactoryTests
{
    private const string RepositoryDomainSlug = "html-css";
    private const string RepositoryDesignPatternSlug = "design-to-code";
    private const string RepositoryModelIdSlug = "qwen3-5-9b";
    private const string RepositoryModelVersion = "v0007";
    private static readonly string FixtureManifestDigest = new('a', 64);
    private static readonly string FixtureLayerDigest = new('1', 64);

    private static readonly ModelConfig ModelConfiguration = new()
    {
        VocabSize = 256,
        ContextLen = 32,
        HiddenDim = 8,
        NumLayers = 1,
        NumHeads = 2,
        NumKvHeads = 1,
        FfnHiddenDim = 16,
    };

    [RetainedCheckpointTrainingSessionWindowsFact]
    public void AbsentAdapter_CreatesFreshPathFreeSessionsWithoutTraining()
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        InitializeTrainingArtifacts(fixture.FileAccess);
        using var access = fixture.FileAccess.CreateStartup()
            .AcquireRetainedFileAccess();
        using var factory = Bind(access, fixture.FileAccess.Root);

        var first = factory.Create("MISSING");
        var second = factory.Create("MISSING");

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.True(second.IsSuccess, second.ErrorMessage);
        var firstSession = Assert.IsType<IncrementalTrainingSession>(first.Value);
        var secondSession = Assert.IsType<IncrementalTrainingSession>(second.Value);
        Assert.Null(firstSession.ParentAdapterCheckpoint);
        Assert.Null(secondSession.ParentAdapterCheckpoint);
        Assert.Null(firstSession.CheckpointStart);
        Assert.Null(secondSession.CheckpointStart);
        Assert.Null(firstSession.ResumeParentCheckpoint);
        Assert.Null(secondSession.ResumeParentCheckpoint);
        Assert.Null(firstSession.ColdStartCheckpoint);
        Assert.Null(secondSession.ColdStartCheckpoint);
        Assert.NotSame(firstSession.Model, secondSession.Model);
        Assert.NotSame(firstSession.Tokenizer, secondSession.Tokenizer);
        Assert.NotSame(firstSession.Trainer, secondSession.Trainer);
        Assert.NotSame(firstSession.Optimizer, secondSession.Optimizer);
        Assert.Equal(
            Hash(fixture.FileAccess.BaseCheckpointPath),
            firstSession.BaseCheckpointSha256);
        Assert.Equal(
            Hash(fixture.FileAccess.TokenizerPath),
            firstSession.TokenizerSha256);
        Assert.Null(firstSession.Model.GetAttachedProjectionAdapter(0, "q_proj"));
        Assert.Null(firstSession.Model.GetAttachedProjectionAdapter(0, "v_proj"));
        Assert.DoesNotContain(
            fixture.FileAccess.Root,
            firstSession.BaseCheckpointSha256,
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            fixture.FileAccess.Root,
            firstSession.TokenizerSha256,
            StringComparison.OrdinalIgnoreCase);
        AssertNoDirectPathOrHandleState(factory);
    }

    [RetainedCheckpointTrainingSessionWindowsFact]
    public void RetainedParent_IsCapturedAsIndependentPathFreeSnapshot()
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        InitializeTrainingArtifacts(fixture.FileAccess);
        var retained = PublishRetainedParent(fixture);
        using var access = fixture.FileAccess.CreateStartup()
            .AcquireRetainedFileAccess();
        using var factory = Bind(access, fixture.FileAccess.Root);

        var result = factory.Create(retained.Domain);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var session = Assert.IsType<IncrementalTrainingSession>(result.Value);
        var parent = Assert.IsType<IncrementalTrainingParentAdapterCheckpoint>(
            session.ParentAdapterCheckpoint);
        using var firstRead = parent.OpenRead();
        using var secondRead = parent.OpenRead();
        Assert.NotSame(firstRead, secondRead);
        Assert.False(firstRead.CanWrite);
        Assert.False(secondRead.CanWrite);
        Assert.Equal(retained.Bytes, ReadAll(firstRead));
        Assert.Equal(retained.Bytes, ReadAll(secondRead));
        Assert.Equal(
            JsonSerializer.Serialize(retained.Provenance),
            JsonSerializer.Serialize(parent.Provenance));
        Assert.NotSame(
            retained.Provenance.TrainingSources.SourceIds,
            parent.Provenance.TrainingSources.SourceIds);
        Assert.Null(session.Model.GetAttachedProjectionAdapter(0, "q_proj"));
        Assert.Null(session.Model.GetAttachedProjectionAdapter(0, "v_proj"));
        Assert.DoesNotContain(
            parent.GetType().GetFields(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic),
            field => field.FieldType == typeof(string)
                     || typeof(SafeHandle).IsAssignableFrom(field.FieldType)
                     || typeof(Stream).IsAssignableFrom(field.FieldType));
    }

    [RetainedCheckpointTrainingSessionWindowsFact]
    public void RepositoryStartRequest_LoadsSealedCheckpointAndNeverOpensRetainedBase()
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        InitializeTrainingArtifacts(fixture.FileAccess);
        var manifest = SealRepositoryModelCheckpoint(
            fixture.FileAccess,
            out var address,
            out var expectedBaseBits,
            mutateModel: static model => AddToEveryBaseParameter(model, 0.25f));
        var retainedBaseOpenCount = 0;
        using var access = fixture.FileAccess.CreateStartup()
            .AcquireRetainedFileAccess();
        using var factory = Bind(
            access,
            fixture.FileAccess.Root,
            afterRetainedArtifactOpenForTesting: role =>
            {
                if (role == ContinuousLearningRetainedArtifactRole.BaseCheckpoint)
                    retainedBaseOpenCount++;
            });
        var request = RepositoryStartRequest(fixture.FileAccess);

        var first = factory.Create(request);
        Assert.True(first.IsSuccess, first.ErrorMessage);
        var firstSession = Assert.IsType<IncrementalTrainingSession>(first.Value);
        Assert.Equal(0, retainedBaseOpenCount);
        AssertRepositoryResumeMetadata(firstSession, manifest);
        Assert.Equal(Hash(address.WeightsPath), firstSession.BaseCheckpointSha256);
        Assert.NotEqual(
            Hash(fixture.FileAccess.BaseCheckpointPath),
            firstSession.BaseCheckpointSha256);
        AssertBitsEqual(expectedBaseBits, firstSession.Model.BaseParameters);
        AssertCheckpointWeightsWriteShareReleased(address.WeightsPath);

        var second = factory.Create(request);
        Assert.True(second.IsSuccess, second.ErrorMessage);
        var secondSession = Assert.IsType<IncrementalTrainingSession>(second.Value);
        Assert.Equal(0, retainedBaseOpenCount);
        Assert.NotSame(firstSession.Model, secondSession.Model);
        AssertRepositoryResumeMetadata(secondSession, manifest);
        Assert.Equal(firstSession.BaseCheckpointSha256, secondSession.BaseCheckpointSha256);
        AssertBitsEqual(expectedBaseBits, secondSession.Model.BaseParameters);
        AssertCheckpointWeightsWriteShareReleased(address.WeightsPath);
    }

    [RetainedCheckpointTrainingSessionWindowsFact]
    public void RepositoryStartResolver_DomainCreateLoadsSealedCheckpointAndNeverOpensRetainedBase()
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        InitializeTrainingArtifacts(fixture.FileAccess);
        var manifest = SealRepositoryModelCheckpoint(
            fixture.FileAccess,
            out var address,
            out var expectedBaseBits,
            mutateModel: static model => AddToEveryBaseParameter(model, 0.5f));
        var retainedBaseOpenCount = 0;
        var resolverCalls = 0;
        string? observedDomain = null;
        using var access = fixture.FileAccess.CreateStartup()
            .AcquireRetainedFileAccess();
        using var factory = Bind(
            access,
            fixture.FileAccess.Root,
            afterRetainedArtifactOpenForTesting: role =>
            {
                if (role == ContinuousLearningRetainedArtifactRole.BaseCheckpoint)
                    retainedBaseOpenCount++;
            },
            resolveRepositoryStartRequest: domain =>
            {
                resolverCalls++;
                observedDomain = domain;
                return OperationResult<
                    RepositoryOwnedIncrementalTrainingSessionStartRequest>.Ok(
                        RepositoryStartRequest(fixture.FileAccess));
            });

        var result = factory.Create(RepositoryDomainSlug);

        Assert.Equal(1, resolverCalls);
        Assert.Equal(RepositoryDomainSlug, observedDomain);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        var session = Assert.IsType<IncrementalTrainingSession>(result.Value);
        Assert.Equal(0, retainedBaseOpenCount);
        AssertRepositoryResumeMetadata(session, manifest);
        Assert.Equal(Hash(address.WeightsPath), session.BaseCheckpointSha256);
        Assert.NotEqual(
            Hash(fixture.FileAccess.BaseCheckpointPath),
            session.BaseCheckpointSha256);
        AssertBitsEqual(expectedBaseBits, session.Model.BaseParameters);
        AssertCheckpointWeightsWriteShareReleased(address.WeightsPath);
    }

    [RetainedCheckpointTrainingSessionWindowsFact]
    public void RepositoryCheckpointPublisher_PublishesNextStepFromResumedSession()
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        InitializeTrainingArtifacts(fixture.FileAccess);
        var parentManifest = SealRepositoryModelCheckpoint(
            fixture.FileAccess,
            out _,
            out _,
            mutateModel: static model => AddToEveryBaseParameter(model, 0.5f));
        var dpoIdentity = CreateSealedDpoManifestIdentity(
            fixture.FileAccess,
            "v0002");
        using var access = fixture.FileAccess.CreateStartup()
            .AcquireRetainedFileAccess();
        using var factory = Bind(access, fixture.FileAccess.Root);
        var request = RepositoryStartRequest(
            fixture.FileAccess,
            currentDpoDatasetManifest: dpoIdentity);
        var sessionResult = factory.Create(request);
        Assert.True(sessionResult.IsSuccess, sessionResult.ErrorMessage);
        var session = Assert.IsType<IncrementalTrainingSession>(
            sessionResult.Value);
        AddToEveryBaseParameter(session.Model, 0.125f);

        var published =
            RepositoryOwnedIncrementalTrainingCheckpointPublisher.PublishNext(
                session,
                dpoIdentity,
                DateTimeOffset.UnixEpoch.AddSeconds(1),
                ["repository-checkpoint-publisher-test"]);

        Assert.True(published.IsSuccess, published.ErrorMessage);
        var child = Assert.IsType<RepositoryOwnedModelCheckpointManifest>(
            published.Value);
        Assert.Equal(2, child.Step);
        Assert.Equal(dpoIdentity, child.DpoDatasetManifest);
        Assert.Equal(string.Empty, child.ColdStartReason);
        Assert.Equal(
            parentManifest.RepositoryRelativeManifestPath,
            child.ParentCheckpoint!.RepositoryRelativeManifestPath);
        Assert.Equal(parentManifest.Step, child.ParentCheckpoint.Step);
        Assert.Equal(
            parentManifest.WeightsSha256,
            child.ParentCheckpoint.CheckpointSha256);
        Assert.Equal(
            parentManifest.WeightsByteLength,
            child.ParentCheckpoint.CheckpointByteLength);
        Assert.NotEqual(parentManifest.WeightsSha256, child.WeightsSha256);
        Assert.Contains(
            "training-artifacts/models/html-css/design-to-code/" +
            "qwen3-5-9b/v0007/checkpoints/000002/manifest.json",
            child.RepositoryRelativeManifestPath,
            StringComparison.Ordinal);

        var childAddress = CreateRepositoryCheckpointAddress(
            fixture.FileAccess,
            step: 2);
        var opened = RepositoryOwnedModelCheckpointManifestSealer
            .OpenSealed(childAddress);
        Assert.True(opened.IsSuccess, opened.ErrorMessage);
        Assert.Equal(child.ManifestSha256, opened.Value!.Manifest.ManifestSha256);
        var loaded = new RepositoryOwnedModelCheckpointLoader()
            .LoadLatestCompatible(
                request.RepositoryRoot,
                request.DomainSlug,
                request.DesignPatternSlug,
                request.ModelIdSlug,
                request.ModelVersion,
                request.RuntimeCompatibility);
        Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
        Assert.Equal(2, loaded.Value!.Manifest.Step);
        Assert.Equal(child.WeightsSha256, loaded.Value.WeightsSha256);
        loaded.Value.Dispose();
        AssertCheckpointWeightsWriteShareReleased(childAddress.WeightsPath);
    }

    [RetainedCheckpointTrainingSessionWindowsFact]
    public void RepositoryCheckpointPublisher_RejectsNullCurrentDpoBindingBeforePublication()
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        InitializeTrainingArtifacts(fixture.FileAccess);
        _ = SealRepositoryModelCheckpoint(
            fixture.FileAccess,
            out _,
            out _,
            mutateModel: static model => AddToEveryBaseParameter(model, 0.5f));
        var callerDpoIdentity = CreateSealedDpoManifestIdentity(
            fixture.FileAccess,
            "v0002");
        using var access = fixture.FileAccess.CreateStartup()
            .AcquireRetainedFileAccess();
        using var factory = Bind(access, fixture.FileAccess.Root);
        var request = RepositoryStartRequest(fixture.FileAccess);
        var sessionResult = factory.Create(request);
        Assert.True(sessionResult.IsSuccess, sessionResult.ErrorMessage);
        var session = Assert.IsType<IncrementalTrainingSession>(
            sessionResult.Value);
        AddToEveryBaseParameter(session.Model, 0.125f);
        var childAddress = CreateRepositoryCheckpointAddress(
            fixture.FileAccess,
            step: 2);

        var published =
            RepositoryOwnedIncrementalTrainingCheckpointPublisher.PublishNext(
                session,
                callerDpoIdentity,
                DateTimeOffset.UnixEpoch.AddSeconds(5),
                ["repository-checkpoint-publisher-null-dpo-binding-test"]);

        Assert.False(published.IsSuccess);
        Assert.Equal(
            RepositoryOwnedIncrementalTrainingCheckpointPublisher.InvalidErrorCode,
            published.ErrorCode);
        // WHY: caller-supplied DPO identity proves only the publication
        // argument. It does not prove which sealed dataset the resumed session
        // selected, so the direct publisher must fail before writer-owned
        // checkpoint files or temporary staging leaves are created.
        Assert.False(File.Exists(childAddress.WeightsPath));
        Assert.False(File.Exists(childAddress.ManifestPath));
        Assert.False(OwnCheckpointStagingFilesRemain(childAddress));
    }

    [RetainedCheckpointTrainingSessionWindowsFact]
    public void RepositoryCheckpointPublisher_RevalidatesCurrentDpoManifestBindingBeforePublication()
    {
        RequireWindows();

        using (var mismatchFixture = new RetainedRegistryFixture())
        {
            InitializeTrainingArtifacts(mismatchFixture.FileAccess);
            _ = SealRepositoryModelCheckpoint(
                mismatchFixture.FileAccess,
                out _,
                out _,
                mutateModel: static model => AddToEveryBaseParameter(
                    model,
                    0.5f));
            var sessionBoundDpoIdentity = CreateSealedDpoManifestIdentity(
                mismatchFixture.FileAccess,
                "v0002");
            var callerDpoIdentity = CreateSealedDpoManifestIdentity(
                mismatchFixture.FileAccess,
                "v0003");
            using var access = mismatchFixture.FileAccess.CreateStartup()
                .AcquireRetainedFileAccess();
            using var factory = Bind(access, mismatchFixture.FileAccess.Root);
            var request = RepositoryStartRequest(
                mismatchFixture.FileAccess,
                currentDpoDatasetManifest: sessionBoundDpoIdentity);
            var sessionResult = factory.Create(request);
            Assert.True(sessionResult.IsSuccess, sessionResult.ErrorMessage);
            var session = Assert.IsType<IncrementalTrainingSession>(
                sessionResult.Value);
            AddToEveryBaseParameter(session.Model, 0.125f);
            var childAddress = CreateRepositoryCheckpointAddress(
                mismatchFixture.FileAccess,
                step: 2);

            var published =
                RepositoryOwnedIncrementalTrainingCheckpointPublisher.PublishNext(
                    session,
                    callerDpoIdentity,
                    DateTimeOffset.UnixEpoch.AddSeconds(3),
                    ["repository-checkpoint-publisher-dpo-binding-test"]);

            Assert.False(published.IsSuccess);
            Assert.Equal(
                RepositoryOwnedIncrementalTrainingCheckpointPublisher
                    .InvalidErrorCode,
                published.ErrorCode);
            // WHY: direct callers reach the publisher without the trainer's
            // accepted-pair recheck, so a mismatched session DPO binding must
            // fail before the child checkpoint writer creates durable bytes.
            // The final-leaf and own-temp assertions pin that this failed at
            // the writer boundary, not after partial checkpoint publication.
            Assert.False(File.Exists(childAddress.WeightsPath));
            Assert.False(File.Exists(childAddress.ManifestPath));
            Assert.False(OwnCheckpointStagingFilesRemain(childAddress));
        }

        using (var successFixture = new RetainedRegistryFixture())
        {
            InitializeTrainingArtifacts(successFixture.FileAccess);
            _ = SealRepositoryModelCheckpoint(
                successFixture.FileAccess,
                out _,
                out _,
                mutateModel: static model => AddToEveryBaseParameter(
                    model,
                    0.5f));
            var sessionBoundDpoIdentity = CreateSealedDpoManifestIdentity(
                successFixture.FileAccess,
                "v0002");
            using var access = successFixture.FileAccess.CreateStartup()
                .AcquireRetainedFileAccess();
            using var factory = Bind(access, successFixture.FileAccess.Root);
            var request = RepositoryStartRequest(
                successFixture.FileAccess,
                currentDpoDatasetManifest: sessionBoundDpoIdentity);
            var sessionResult = factory.Create(request);
            Assert.True(sessionResult.IsSuccess, sessionResult.ErrorMessage);
            var session = Assert.IsType<IncrementalTrainingSession>(
                sessionResult.Value);
            AddToEveryBaseParameter(session.Model, 0.125f);
            var childAddress = CreateRepositoryCheckpointAddress(
                successFixture.FileAccess,
                step: 2);

            var published =
                RepositoryOwnedIncrementalTrainingCheckpointPublisher.PublishNext(
                    session,
                    sessionBoundDpoIdentity,
                    DateTimeOffset.UnixEpoch.AddSeconds(4),
                    ["repository-checkpoint-publisher-dpo-binding-test"]);

            Assert.True(published.IsSuccess, published.ErrorMessage);
            Assert.Equal(
                sessionBoundDpoIdentity,
                published.Value!.DpoDatasetManifest);
            var opened = RepositoryOwnedModelCheckpointManifestSealer
                .OpenSealed(childAddress);
            Assert.True(opened.IsSuccess, opened.ErrorMessage);
            Assert.Equal(
                sessionBoundDpoIdentity,
                opened.Value!.Manifest.DpoDatasetManifest);
            AssertCheckpointWeightsWriteShareReleased(childAddress.WeightsPath);
        }
    }

    [RetainedCheckpointTrainingSessionWindowsFact]
    public void RepositoryCheckpointPublisher_RejectsHashMatchingUnsealedDpoManifestBeforeChildPublication()
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        InitializeTrainingArtifacts(fixture.FileAccess);
        _ = SealRepositoryModelCheckpoint(
            fixture.FileAccess,
            out _,
            out _,
            mutateModel: static model => AddToEveryBaseParameter(model, 0.5f));
        var fakeDpoIdentity = CreateHashMatchingUnsealedDpoManifestIdentity(
            fixture.FileAccess,
            "v0002");
        using var access = fixture.FileAccess.CreateStartup()
            .AcquireRetainedFileAccess();
        using var factory = Bind(access, fixture.FileAccess.Root);
        var request = RepositoryStartRequest(
            fixture.FileAccess,
            currentDpoDatasetManifest: fakeDpoIdentity);
        var sessionResult = factory.Create(request);
        Assert.True(sessionResult.IsSuccess, sessionResult.ErrorMessage);
        var session = Assert.IsType<IncrementalTrainingSession>(
            sessionResult.Value);
        AddToEveryBaseParameter(session.Model, 0.125f);
        var childAddress = CreateRepositoryCheckpointAddress(
            fixture.FileAccess,
            step: 2);

        var published =
            RepositoryOwnedIncrementalTrainingCheckpointPublisher.PublishNext(
                session,
                fakeDpoIdentity,
                DateTimeOffset.UnixEpoch.AddSeconds(2),
                ["repository-checkpoint-publisher-fake-dpo-test"]);

        Assert.False(published.IsSuccess);
        Assert.Equal(
            RepositoryOwnedModelCheckpointManifestSealer.InvalidErrorCode,
            published.ErrorCode);
        // WHY: целостность DPO проверяется до запуска writer-а child checkpoint,
        // поэтому fake manifest не может оставить наполовину опубликованные
        // model bytes.
        Assert.False(File.Exists(childAddress.WeightsPath));
        Assert.False(File.Exists(childAddress.ManifestPath));
        Assert.False(OwnCheckpointStagingFilesRemain(childAddress));
    }

    [RetainedCheckpointTrainingSessionWindowsFact]
    public void RepositoryCheckpointPublisher_PublishesStepOneFromAuthorizedColdStart()
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        InitializeTrainingArtifacts(fixture.FileAccess);
        using var access = fixture.FileAccess.CreateStartup()
            .AcquireRetainedFileAccess();
        using var factory = Bind(access, fixture.FileAccess.Root);
        const string reason =
            "operator approved first repository checkpoint";
        var dpoIdentity = CreateSealedDpoManifestIdentity(
            fixture.FileAccess,
            "v0002");
        var startRequest = RepositoryStartRequest(
            fixture.FileAccess,
            RepositoryOwnedIncrementalTrainingColdStartPolicy.Authorize(reason),
            currentDpoDatasetManifest: dpoIdentity);
        var coldStart = factory.Create(startRequest);
        Assert.True(coldStart.IsSuccess, coldStart.ErrorMessage);
        var session = Assert.IsType<IncrementalTrainingSession>(
            coldStart.Value);
        AddToEveryBaseParameter(session.Model, 0.125f);
        var stepOneAddress = CreateRepositoryCheckpointAddress(
            fixture.FileAccess,
            step: 1);

        var result =
            RepositoryOwnedIncrementalTrainingCheckpointPublisher.PublishNext(
                session,
                dpoIdentity,
                DateTimeOffset.UnixEpoch,
                ["repository-checkpoint-publisher-test"]);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var stepOne = Assert.IsType<RepositoryOwnedModelCheckpointManifest>(
            result.Value);
        Assert.Equal(1, stepOne.Step);
        Assert.Null(stepOne.ParentCheckpoint);
        Assert.Equal(reason, stepOne.ColdStartReason);
        Assert.Equal(dpoIdentity, stepOne.DpoDatasetManifest);
        Assert.Contains(
            "training-artifacts/models/html-css/design-to-code/" +
            "qwen3-5-9b/v0007/checkpoints/000001/manifest.json",
            stepOne.RepositoryRelativeManifestPath,
            StringComparison.Ordinal);

        var opened = RepositoryOwnedModelCheckpointManifestSealer
            .OpenSealed(stepOneAddress);
        Assert.True(opened.IsSuccess, opened.ErrorMessage);
        Assert.Equal(stepOne.ManifestSha256,
            opened.Value!.Manifest.ManifestSha256);
        Assert.Null(opened.Value.Manifest.ParentCheckpoint);
        Assert.Equal(reason, opened.Value.Manifest.ColdStartReason);
        Assert.Equal(dpoIdentity, opened.Value.Manifest.DpoDatasetManifest);
        var loaded = new RepositoryOwnedModelCheckpointLoader()
            .LoadLatestCompatible(
                startRequest.RepositoryRoot,
                startRequest.DomainSlug,
                startRequest.DesignPatternSlug,
                startRequest.ModelIdSlug,
                startRequest.ModelVersion,
                startRequest.RuntimeCompatibility);
        Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
        Assert.Equal(1, loaded.Value!.Manifest.Step);
        Assert.Null(loaded.Value.Manifest.ParentCheckpoint);
        Assert.Equal(reason, loaded.Value.Manifest.ColdStartReason);
        Assert.Equal(dpoIdentity, loaded.Value.Manifest.DpoDatasetManifest);
        loaded.Value.Dispose();
        AssertCheckpointWeightsWriteShareReleased(stepOneAddress.WeightsPath);
    }

    [RetainedCheckpointTrainingSessionWindowsFact]
    public void RepositoryCheckpointPublisher_RejectsInvalidColdStartBeforeStepOneManifest()
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        InitializeTrainingArtifacts(fixture.FileAccess);
        using var access = fixture.FileAccess.CreateStartup()
            .AcquireRetainedFileAccess();
        using var factory = Bind(access, fixture.FileAccess.Root);
        const string reason =
            "operator approved first repository checkpoint";
        var dpoIdentity = CreateSealedDpoManifestIdentity(
            fixture.FileAccess,
            "v0002");
        var authorizedRequest = RepositoryStartRequest(
            fixture.FileAccess,
            RepositoryOwnedIncrementalTrainingColdStartPolicy.Authorize(reason),
            currentDpoDatasetManifest: dpoIdentity);
        var coldStart = factory.Create(authorizedRequest);
        Assert.True(coldStart.IsSuccess, coldStart.ErrorMessage);
        var session = Assert.IsType<IncrementalTrainingSession>(
            coldStart.Value);
        var stepOneAddress = CreateRepositoryCheckpointAddress(
            fixture.FileAccess,
            step: 1);

        var deniedRequest = RepositoryStartRequest(
            fixture.FileAccess,
            RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied,
            currentDpoDatasetManifest: dpoIdentity);
        var deniedMetadata =
            IncrementalTrainingCheckpointStartMetadata.FromColdStart(
                reason,
                deniedRequest);
        var deniedResult =
            RepositoryOwnedIncrementalTrainingCheckpointPublisher.PublishNext(
                ReplaceCheckpointStart(session, deniedMetadata),
                dpoIdentity,
                DateTimeOffset.UnixEpoch,
                ["repository-checkpoint-publisher-denied-cold-start-test"]);

        Assert.False(deniedResult.IsSuccess);
        Assert.Equal(
            RepositoryOwnedIncrementalTrainingCheckpointPublisher.InvalidErrorCode,
            deniedResult.ErrorCode);
        Assert.False(File.Exists(stepOneAddress.WeightsPath));
        Assert.False(File.Exists(stepOneAddress.ManifestPath));
        Assert.False(Directory.Exists(stepOneAddress.ModelArtifactRoot));

        var mismatchRequest = RepositoryStartRequest(
            fixture.FileAccess,
            RepositoryOwnedIncrementalTrainingColdStartPolicy.Authorize(
                "different authorized reason"),
            currentDpoDatasetManifest: dpoIdentity);
        var mismatchMetadata =
            IncrementalTrainingCheckpointStartMetadata.FromColdStart(
                reason,
                mismatchRequest);
        var mismatchResult =
            RepositoryOwnedIncrementalTrainingCheckpointPublisher.PublishNext(
                ReplaceCheckpointStart(session, mismatchMetadata),
                dpoIdentity,
                DateTimeOffset.UnixEpoch,
                ["repository-checkpoint-publisher-reason-mismatch-test"]);

        Assert.False(mismatchResult.IsSuccess);
        Assert.Equal(
            RepositoryOwnedIncrementalTrainingCheckpointPublisher.InvalidErrorCode,
            mismatchResult.ErrorCode);
        Assert.False(File.Exists(stepOneAddress.WeightsPath));
        Assert.False(File.Exists(stepOneAddress.ManifestPath));
        Assert.False(Directory.Exists(stepOneAddress.ModelArtifactRoot));
    }

    [RetainedCheckpointTrainingSessionWindowsFact]
    public void RepositoryStartResolver_FailurePropagatesWithoutRetainedBaseFallback()
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        InitializeTrainingArtifacts(fixture.FileAccess);
        var retainedBaseOpenCount = 0;
        var resolverCalls = 0;
        using var access = fixture.FileAccess.CreateStartup()
            .AcquireRetainedFileAccess();
        using var factory = Bind(
            access,
            fixture.FileAccess.Root,
            afterRetainedArtifactOpenForTesting: role =>
            {
                if (role == ContinuousLearningRetainedArtifactRole.BaseCheckpoint)
                    retainedBaseOpenCount++;
            },
            resolveRepositoryStartRequest: domain =>
            {
                Assert.Equal(RepositoryDomainSlug, domain);
                resolverCalls++;
                return OperationResult<
                    RepositoryOwnedIncrementalTrainingSessionStartRequest>.Fail(
                        "configured repository start request was not available",
                        "REPOSITORY_START_REQUEST_RESOLUTION_FAILED");
            });

        var result = factory.Create(RepositoryDomainSlug);

        Assert.Equal(1, resolverCalls);
        Assert.False(result.IsSuccess);
        Assert.Equal(
            "REPOSITORY_START_REQUEST_RESOLUTION_FAILED",
            result.ErrorCode);
        Assert.Equal(
            "configured repository start request was not available",
            result.ErrorMessage);
        Assert.Null(result.Value);
        Assert.Equal(0, retainedBaseOpenCount);
    }

    [RetainedCheckpointTrainingSessionWindowsFact]
    public void RepositoryStartResolver_SuccessNullFailsClosedBeforeRetainedBaseFallback()
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        InitializeTrainingArtifacts(fixture.FileAccess);
        var retainedBaseOpenCount = 0;
        var resolverCalls = 0;
        using var access = fixture.FileAccess.CreateStartup()
            .AcquireRetainedFileAccess();
        using var factory = Bind(
            access,
            fixture.FileAccess.Root,
            afterRetainedArtifactOpenForTesting: role =>
            {
                if (role == ContinuousLearningRetainedArtifactRole.BaseCheckpoint)
                    retainedBaseOpenCount++;
            },
            resolveRepositoryStartRequest: domain =>
            {
                Assert.Equal(RepositoryDomainSlug, domain);
                resolverCalls++;
                return OperationResult<
                    RepositoryOwnedIncrementalTrainingSessionStartRequest>.Ok(
                        null!);
            });

        var result = factory.Create(RepositoryDomainSlug);

        Assert.Equal(1, resolverCalls);
        Assert.False(result.IsSuccess);
        Assert.Equal(
            RepositoryOwnedModelCheckpointLoader.InvalidRequestErrorCode,
            result.ErrorCode);
        Assert.Null(result.Value);
        Assert.Equal(0, retainedBaseOpenCount);
    }

    [RetainedCheckpointTrainingSessionWindowsFact]
    public void RepositoryStartResolver_DomainMismatchFailsClosedBeforeRepositoryLoad()
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        InitializeTrainingArtifacts(fixture.FileAccess);
        var retainedBaseOpenCount = 0;
        var resolverCalls = 0;
        var loaderCalls = 0;
        var mismatchedRequest = RepositoryStartRequest(fixture.FileAccess)
            with
            {
                DomainSlug = "html-css-mismatch",
            };
        using var access = fixture.FileAccess.CreateStartup()
            .AcquireRetainedFileAccess();
        using var factory = Bind(
            access,
            fixture.FileAccess.Root,
            afterRetainedArtifactOpenForTesting: role =>
            {
                if (role == ContinuousLearningRetainedArtifactRole.BaseCheckpoint)
                    retainedBaseOpenCount++;
            },
            loadRepositoryCheckpointForTesting: _ =>
            {
                loaderCalls++;
                return OperationResult<RepositoryOwnedModelCheckpointLoadResult>.Fail(
                    "repository loader should not run for mismatched domains",
                    "REPOSITORY_LOADER_REACHED");
            },
            resolveRepositoryStartRequest: domain =>
            {
                Assert.Equal(RepositoryDomainSlug, domain);
                resolverCalls++;
                return OperationResult<
                    RepositoryOwnedIncrementalTrainingSessionStartRequest>.Ok(
                        mismatchedRequest);
            });

        var result = factory.Create(RepositoryDomainSlug);

        Assert.Equal(1, resolverCalls);
        Assert.Equal(0, loaderCalls);
        Assert.False(result.IsSuccess);
        Assert.Equal(
            RepositoryOwnedModelCheckpointLoader.InvalidRequestErrorCode,
            result.ErrorCode);
        Assert.Null(result.Value);
        Assert.Equal(0, retainedBaseOpenCount);
    }

    [RetainedCheckpointTrainingSessionWindowsFact]
    public void RepositoryStartResolver_MissingTreeWithExplicitReasonColdStartsFromRetainedBaseOnce()
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        InitializeTrainingArtifacts(fixture.FileAccess);
        var retainedBaseOpenCount = 0;
        var resolverCalls = 0;
        RepositoryOwnedIncrementalTrainingSessionStartRequest? resolvedRequest =
            null;
        const string reason =
            "operator approved first repository checkpoint bootstrap from resolver";
        using var access = fixture.FileAccess.CreateStartup()
            .AcquireRetainedFileAccess();
        using var factory = Bind(
            access,
            fixture.FileAccess.Root,
            afterRetainedArtifactOpenForTesting: role =>
            {
                if (role == ContinuousLearningRetainedArtifactRole.BaseCheckpoint)
                    retainedBaseOpenCount++;
            },
            resolveRepositoryStartRequest: domain =>
            {
                Assert.Equal(RepositoryDomainSlug, domain);
                resolverCalls++;
                resolvedRequest = RepositoryStartRequest(
                    fixture.FileAccess,
                    RepositoryOwnedIncrementalTrainingColdStartPolicy
                        .Authorize(reason));
                return OperationResult<
                    RepositoryOwnedIncrementalTrainingSessionStartRequest>.Ok(
                        resolvedRequest);
            });

        var result = factory.Create(RepositoryDomainSlug);

        Assert.Equal(1, resolverCalls);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        var session = Assert.IsType<IncrementalTrainingSession>(result.Value);
        Assert.Equal(1, retainedBaseOpenCount);
        Assert.Equal(
            Hash(fixture.FileAccess.BaseCheckpointPath),
            session.BaseCheckpointSha256);
        var metadata = Assert.IsType<IncrementalTrainingCheckpointStartMetadata>(
            session.CheckpointStart);
        Assert.Equal(
            IncrementalTrainingCheckpointStartSource
                .RetainedBaseCheckpointColdStart,
            metadata.Source);
        Assert.Same(
            resolvedRequest,
            metadata.RepositoryStartRequest);
        Assert.Null(session.ResumeParentCheckpoint);
        var coldStart = Assert.IsType<IncrementalTrainingColdStartMetadata>(
            session.ColdStartCheckpoint);
        Assert.Equal(reason, coldStart.HumanReason);
        Assert.Same(
            resolvedRequest,
            coldStart.RepositoryStartRequest);
    }

    [RetainedCheckpointTrainingSessionWindowsFact]
    public void RepositoryStartResolver_CancellationAndDisposalDoNotReachResolver()
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        InitializeTrainingArtifacts(fixture.FileAccess);
        var resolverCalls = 0;
        using var access = fixture.FileAccess.CreateStartup()
            .AcquireRetainedFileAccess();
        using var factory = Bind(
            access,
            fixture.FileAccess.Root,
            resolveRepositoryStartRequest: _ =>
            {
                resolverCalls++;
                return OperationResult<
                    RepositoryOwnedIncrementalTrainingSessionStartRequest>.Ok(
                        RepositoryStartRequest(fixture.FileAccess));
            });
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        Assert.Throws<OperationCanceledException>(
            () => factory.Create(RepositoryDomainSlug, cancellation.Token));
        Assert.Equal(0, resolverCalls);

        factory.Dispose();
        var afterDispose = factory.Create(RepositoryDomainSlug);

        Assert.False(afterDispose.IsSuccess);
        Assert.Equal(
            RetainedCheckpointBackedIncrementalTrainingSessionFactory
                .DisposedErrorCode,
            afterDispose.ErrorCode);
        Assert.Null(afterDispose.Value);
        Assert.Equal(0, resolverCalls);
    }

    [RetainedCheckpointTrainingSessionWindowsFact]
    public void RepositoryStartRequest_MissingTreeWithExplicitReasonColdStartsFromRetainedBaseOnce()
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        InitializeTrainingArtifacts(fixture.FileAccess);
        var retainedBaseOpenCount = 0;
        const string reason =
            "operator approved first repository checkpoint bootstrap for html-css";
        using var access = fixture.FileAccess.CreateStartup()
            .AcquireRetainedFileAccess();
        using var factory = Bind(
            access,
            fixture.FileAccess.Root,
            afterRetainedArtifactOpenForTesting: role =>
            {
                if (role == ContinuousLearningRetainedArtifactRole.BaseCheckpoint)
                    retainedBaseOpenCount++;
            });

        var request = RepositoryStartRequest(
            fixture.FileAccess,
            RepositoryOwnedIncrementalTrainingColdStartPolicy.Authorize(reason));
        var result = factory.Create(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var session = Assert.IsType<IncrementalTrainingSession>(result.Value);
        Assert.Equal(1, retainedBaseOpenCount);
        Assert.Equal(
            Hash(fixture.FileAccess.BaseCheckpointPath),
            session.BaseCheckpointSha256);
        var metadata = Assert.IsType<IncrementalTrainingCheckpointStartMetadata>(
            session.CheckpointStart);
        Assert.Equal(
            IncrementalTrainingCheckpointStartSource
                .RetainedBaseCheckpointColdStart,
            metadata.Source);
        Assert.Same(request, metadata.RepositoryStartRequest);
        Assert.Null(metadata.ResumeParentCheckpoint);
        Assert.Null(session.ResumeParentCheckpoint);
        var coldStart = Assert.IsType<IncrementalTrainingColdStartMetadata>(
            metadata.ColdStart);
        Assert.Equal(reason, coldStart.HumanReason);
        Assert.Same(request, coldStart.RepositoryStartRequest);
        Assert.Equal(reason, session.ColdStartCheckpoint?.HumanReason);
    }

    [RetainedCheckpointTrainingSessionWindowsFact]
    public void RepositoryStartRequest_ExistingTreeWithoutSealedCandidateWithExplicitReasonColdStartsFromRetainedBaseOnce()
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        InitializeTrainingArtifacts(fixture.FileAccess);
        var address = CreateRepositoryCheckpointAddress(fixture.FileAccess);
        Directory.CreateDirectory(
            Directory.GetParent(address.CheckpointRoot)!.FullName);
        var retainedBaseOpenCount = 0;
        const string reason =
            "operator approved retained base because model tree has no sealed candidate";
        using var access = fixture.FileAccess.CreateStartup()
            .AcquireRetainedFileAccess();
        using var factory = Bind(
            access,
            fixture.FileAccess.Root,
            afterRetainedArtifactOpenForTesting: role =>
            {
                if (role == ContinuousLearningRetainedArtifactRole.BaseCheckpoint)
                    retainedBaseOpenCount++;
            });

        var request = RepositoryStartRequest(
            fixture.FileAccess,
            RepositoryOwnedIncrementalTrainingColdStartPolicy.Authorize(reason));
        var result = factory.Create(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var session = Assert.IsType<IncrementalTrainingSession>(result.Value);
        Assert.Equal(1, retainedBaseOpenCount);
        Assert.Equal(
            Hash(fixture.FileAccess.BaseCheckpointPath),
            session.BaseCheckpointSha256);
        var metadata = Assert.IsType<IncrementalTrainingCheckpointStartMetadata>(
            session.CheckpointStart);
        Assert.Equal(
            IncrementalTrainingCheckpointStartSource
                .RetainedBaseCheckpointColdStart,
            metadata.Source);
        Assert.Same(request, metadata.RepositoryStartRequest);
        Assert.Null(session.ResumeParentCheckpoint);
        var coldStart = Assert.IsType<IncrementalTrainingColdStartMetadata>(
            session.ColdStartCheckpoint);
        Assert.Equal(reason, coldStart.HumanReason);
        Assert.Same(
            request,
            coldStart.RepositoryStartRequest);
    }

    [RetainedCheckpointTrainingSessionWindowsFact]
    public void RepositoryStartRequest_MissingTreeWithoutColdStartAuthorizationFailsClosed()
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        InitializeTrainingArtifacts(fixture.FileAccess);
        var retainedBaseOpenCount = 0;
        using var access = fixture.FileAccess.CreateStartup()
            .AcquireRetainedFileAccess();
        using var factory = Bind(
            access,
            fixture.FileAccess.Root,
            afterRetainedArtifactOpenForTesting: role =>
            {
                if (role == ContinuousLearningRetainedArtifactRole.BaseCheckpoint)
                    retainedBaseOpenCount++;
            });

        var result = factory.Create(RepositoryStartRequest(fixture.FileAccess));

        Assert.False(result.IsSuccess);
        Assert.Equal(
            RepositoryOwnedModelCheckpointLoader.NoCheckpointTreeErrorCode,
            result.ErrorCode);
        Assert.Null(result.Value);
        Assert.Equal(0, retainedBaseOpenCount);
    }

    [RetainedCheckpointTrainingSessionWindowsFact]
    public void RepositoryStartRequest_InvalidColdStartPolicyFailsBeforeBaseFallback()
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        InitializeTrainingArtifacts(fixture.FileAccess);
        var retainedBaseOpenCount = 0;
        using var access = fixture.FileAccess.CreateStartup()
            .AcquireRetainedFileAccess();
        using var factory = Bind(
            access,
            fixture.FileAccess.Root,
            afterRetainedArtifactOpenForTesting: role =>
            {
                if (role == ContinuousLearningRetainedArtifactRole.BaseCheckpoint)
                    retainedBaseOpenCount++;
            });

        var result = factory.Create(RepositoryStartRequest(
            fixture.FileAccess,
            RepositoryOwnedIncrementalTrainingColdStartPolicy.Authorize(" ")));

        Assert.False(result.IsSuccess);
        Assert.Equal(
            RepositoryOwnedModelCheckpointLoader.InvalidRequestErrorCode,
            result.ErrorCode);
        Assert.Null(result.Value);
        Assert.Equal(0, retainedBaseOpenCount);
    }

    [RetainedCheckpointTrainingSessionWindowsFact]
    public void RepositoryStartRequest_LoaderSuccessWithoutValueFailsClosedBeforeBaseFallback()
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        InitializeTrainingArtifacts(fixture.FileAccess);
        var retainedBaseOpenCount = 0;
        using var access = fixture.FileAccess.CreateStartup()
            .AcquireRetainedFileAccess();
        using var factory = Bind(
            access,
            fixture.FileAccess.Root,
            afterRetainedArtifactOpenForTesting: role =>
            {
                if (role == ContinuousLearningRetainedArtifactRole.BaseCheckpoint)
                    retainedBaseOpenCount++;
            },
            loadRepositoryCheckpointForTesting: static _ =>
                OperationResult<RepositoryOwnedModelCheckpointLoadResult>.Ok(
                    null!));

        var result = factory.Create(RepositoryStartRequest(
            fixture.FileAccess,
            RepositoryOwnedIncrementalTrainingColdStartPolicy.Authorize(
                "operator approved cold start only if repository reports missing")));

        Assert.False(result.IsSuccess);
        Assert.Equal(
            RepositoryOwnedModelCheckpointLoader.InvalidRequestErrorCode,
            result.ErrorCode);
        Assert.Null(result.Value);
        Assert.Equal(0, retainedBaseOpenCount);
    }

    [RetainedCheckpointTrainingSessionWindowsFact]
    public void RepositoryStartRequest_UnsafeRepositoryFailuresNeverColdStartFromRetainedBase()
    {
        RequireWindows();

        AssertRepositoryFailureDoesNotOpenRetainedBase(
            arrangeRepository: fixture => SealRepositoryModelCheckpoint(
                fixture.FileAccess,
                out _,
                out _,
                runtimeCompatibility: IncompatibleRuntimeCompatibility()),
            expectedErrorCode:
                RepositoryOwnedModelCheckpointLoader.IncompatibleCandidateErrorCode);

        AssertRepositoryFailureDoesNotOpenRetainedBase(
            arrangeRepository: fixture =>
            {
                SealRepositoryModelCheckpoint(
                    fixture.FileAccess,
                    out var address,
                    out _);
                var bytes = File.ReadAllBytes(address.WeightsPath);
                bytes[0] ^= 0xff;
                File.WriteAllBytes(address.WeightsPath, bytes);
            },
            expectedErrorCode:
                RepositoryOwnedModelCheckpointLoader.CorruptCandidateErrorCode);

        AssertRepositoryFailureDoesNotOpenRetainedBase(
            arrangeRepository: fixture =>
            {
                SealRepositoryModelCheckpoint(
                    fixture.FileAccess,
                    out var address,
                    out _);
                Directory.CreateDirectory(Path.Combine(
                    Directory.GetParent(address.CheckpointRoot)!.FullName,
                    "1"));
            },
            expectedErrorCode:
                RepositoryOwnedModelCheckpointLoader.AmbiguousCandidateErrorCode);

        AssertRepositoryFailureDoesNotOpenRetainedBase(
            arrangeRepository: static _ => { },
            expectedErrorCode:
                RepositoryOwnedModelCheckpointLoader.UnavailableErrorCode,
            loadRepositoryCheckpointForTesting: static _ =>
                OperationResult<RepositoryOwnedModelCheckpointLoadResult>.Fail(
                    "repository storage unavailable",
                    RepositoryOwnedModelCheckpointLoader.UnavailableErrorCode));
    }

    [Fact]
    public async Task IncrementalTrainer_ForwardsExactTokenAndPreservesLegacyDefault()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var example = fixture.Example("BUG", "token", "forwarding");
        using var sameTokenCancellation = new CancellationTokenSource();
        var sameTokenFactory = new CancellationAwareSessionFactoryProbe(
            static _ => OperationResult<IncrementalTrainingSession>.Fail(
                "observed",
                "SESSION_TOKEN_OBSERVED"));
        var sameTokenTrainer = new IncrementalTrainer(
            new RetainedRegistryNullStorage(),
            sameTokenFactory,
            fixture.AdapterDirectory);

        var observed = await sameTokenTrainer.TrainAsync(
            "BUG",
            [example],
            [],
            sameTokenCancellation.Token);

        Assert.False(observed.IsSuccess);
        Assert.Equal("SESSION_TOKEN_OBSERVED", observed.ErrorCode);
        Assert.Equal(1, sameTokenFactory.CancellationAwareCreateCount);
        Assert.Equal(0, sameTokenFactory.LegacyCreateCount);
        Assert.Equal(sameTokenCancellation.Token, sameTokenFactory.ObservedToken);

        using var alreadyCancelled = new CancellationTokenSource();
        alreadyCancelled.Cancel();
        var neverReachedFactory = new CancellationAwareSessionFactoryProbe(
            static _ => throw new Xunit.Sdk.XunitException(
                "An already-cancelled request reached session creation."));
        var alreadyCancelledTrainer = new IncrementalTrainer(
            new RetainedRegistryNullStorage(),
            neverReachedFactory,
            fixture.AdapterDirectory);
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            alreadyCancelledTrainer.TrainAsync(
                "BUG",
                [example],
                [],
                alreadyCancelled.Token));
        Assert.Equal(0, neverReachedFactory.CancellationAwareCreateCount);
        Assert.Equal(0, neverReachedFactory.LegacyCreateCount);

        using var inFlightCancellation = new CancellationTokenSource();
        var inFlightFactory = new CancellationAwareSessionFactoryProbe(token =>
        {
            Assert.Equal(inFlightCancellation.Token, token);
            inFlightCancellation.Cancel();
            token.ThrowIfCancellationRequested();
            throw new Xunit.Sdk.XunitException("Cancellation did not propagate.");
        });
        var inFlightTrainer = new IncrementalTrainer(
            new RetainedRegistryNullStorage(),
            inFlightFactory,
            fixture.AdapterDirectory);
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            inFlightTrainer.TrainAsync(
                "BUG",
                [example],
                [],
                inFlightCancellation.Token));
        Assert.Equal(1, inFlightFactory.CancellationAwareCreateCount);
        Assert.Equal(inFlightCancellation.Token, inFlightFactory.ObservedToken);

        var legacyFactory = new LegacySessionFactoryProbe();
        var legacyTrainer = new IncrementalTrainer(
            new RetainedRegistryNullStorage(),
            legacyFactory,
            fixture.AdapterDirectory);
        var legacy = await legacyTrainer.TrainAsync(
            "BUG",
            [example],
            [],
            CancellationToken.None);
        Assert.False(legacy.IsSuccess);
        Assert.Equal("LEGACY_FACTORY_REACHED", legacy.ErrorCode);
        Assert.Equal(1, legacyFactory.CreateCount);
    }

    [RetainedCheckpointTrainingSessionWindowsFact]
    public async Task RetainedParent_LoadsAfterAttachmentBeforeAnyTrainingSideEffect()
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        InitializeTrainingArtifacts(fixture.FileAccess);
        var retained = PublishRetainedParent(fixture);
        using var access = fixture.FileAccess.CreateStartup()
            .AcquireRetainedFileAccess();
        using var factory = Bind(access, fixture.FileAccess.Root);
        var sessionResult = factory.Create(retained.Domain);
        Assert.True(sessionResult.IsSuccess, sessionResult.ErrorMessage);
        var session = Assert.IsType<IncrementalTrainingSession>(sessionResult.Value);
        var baseBits = SnapshotBits(session.Model.BaseParameters);
        var executor = new LoadedParentObservationExecutor(retained.AdapterBits);
        var dpo = new NeverDpoExecutor();
        var outputDirectory = Path.Combine(
            Path.GetTempPath(),
            $"retained-parent-output-{Guid.NewGuid():N}");
        var trainer = CreateTrainerWithStepExecutor(
            new FixedSessionFactory(session),
            executor,
            dpo,
            outputDirectory);
        var example = ValidExample("BUG", "retained", "parent");

        var result = await trainer.TrainAsync("BUG", [example], []);

        Assert.False(result.IsSuccess);
        Assert.Equal("SUPERVISED_ADAPTER_TRAINING_FAILED", result.ErrorCode);
        Assert.Equal(1, executor.ExecuteCount);
        Assert.True(executor.ObservedExactLoadedParent);
        Assert.Same(session, executor.ObservedSession);
        Assert.Equal(0, dpo.ExecuteCount);
        Assert.False(Directory.Exists(outputDirectory));
        AssertBitsEqual(baseBits, session.Model.BaseParameters);
        AssertBitsEqual(retained.AdapterBits, AdapterParameters(session.Model));
    }

    [RetainedCheckpointTrainingSessionWindowsFact]
    public void LegacyRegistryEntry_PropagatesExactResolverFailure()
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        InitializeTrainingArtifacts(fixture.FileAccess);
        using (var setupAccess = fixture.FileAccess.CreateStartup()
                   .AcquireRetainedFileAccess())
        using (var registryRoot = setupAccess.AcquireRegistryRootCapability())
        {
            var registry = new DomainModelRegistry(
                new RetainedRegistryNullStorage(),
                registryRoot);
            var registered = registry.Register(
                fixture.Adapter("BUG", "legacy-parent.lora", 1));
            Assert.True(registered.IsSuccess, registered.ErrorMessage);
        }

        using var access = fixture.FileAccess.CreateStartup()
            .AcquireRetainedFileAccess();
        using var factory = Bind(access, fixture.FileAccess.Root);

        var result = factory.Create("BUG");

        Assert.False(result.IsSuccess);
        Assert.Equal("ADAPTER_REFERENCE_NOT_RETAINED", result.ErrorCode);
        Assert.Null(result.Value);
    }

    [RetainedCheckpointTrainingSessionWindowsFact]
    public void ResolverFailures_PassThroughUntrustedAndChangedExactCodes()
    {
        RequireWindows();
        using (var untrustedFixture = new RetainedRegistryFixture())
        {
            InitializeTrainingArtifacts(untrustedFixture.FileAccess);
            var retained = PublishRetainedParent(untrustedFixture);
            var registryBytesBeforeCreate = File.ReadAllBytes(
                untrustedFixture.RegistryPath);
            var artifactPath = Path.Combine(
                untrustedFixture.FileAccess.AdapterRoot,
                retained.Reference.CanonicalLeafName);
            var untrustedBytes = retained.Bytes.ToArray();
            untrustedBytes[0] ^= 0xff;
            File.WriteAllBytes(
                artifactPath,
                untrustedBytes);
            using var access = untrustedFixture.FileAccess.CreateStartup()
                .AcquireRetainedFileAccess();
            using var factory = Bind(access, untrustedFixture.FileAccess.Root);

            var result = factory.Create(retained.Domain);

            Assert.False(result.IsSuccess);
            // Реестр остаётся каноничным; доверие теряют именно байты
            // опубликованного адаптера, поэтому владелец retained-доступа
            // закрывает весь AdapterRoot, а не делает вид, что корень безопасен.
            Assert.Equal(
                ContinuousLearningPathIdentityException.StableErrorCode,
                result.ErrorCode);
            Assert.Null(result.Value);
            Assert.Equal(
                registryBytesBeforeCreate,
                File.ReadAllBytes(untrustedFixture.RegistryPath));
            Assert.Equal(untrustedBytes, File.ReadAllBytes(artifactPath));
            Assert.Throws<ContinuousLearningPathIdentityException>(
                access.EnsureRetained);
        }

        using var changedFixture = new RetainedRegistryFixture();
        InitializeTrainingArtifacts(changedFixture.FileAccess);
        var changedRetained = PublishRetainedParent(changedFixture);
        var changed = false;
        using var changedAccess = changedFixture.FileAccess.CreateStartup()
            .AcquireRetainedFileAccess();
        using var changedFactory = Bind(
            changedAccess,
            changedFixture.FileAccess.Root,
            adapterRootOptions:
                new ServerOwnedContinuousLearningAdapterRootCapabilityOptions
                {
                    ObserveIoStageForTesting = observation =>
                    {
                        if (changed || observation.Stage !=
                            ServerOwnedContinuousLearningAdapterIoStage
                                .DuringExactRead)
                        {
                            return;
                        }

                        changed = true;
                        File.WriteAllBytes(
                            changedFixture.RegistryPath,
                            SerializeRetainedRegistry(
                                changedRetained with
                                {
                                    TrainingExamples =
                                        changedRetained.TrainingExamples + 1,
                                }));
                    },
                });

        var changedResult = changedFactory.Create(changedRetained.Domain);

        Assert.True(changed);
        Assert.False(changedResult.IsSuccess);
        Assert.Equal(
            "ADAPTER_REFERENCE_CHANGED_DURING_RESOLUTION",
            changedResult.ErrorCode);
    }

    [RetainedCheckpointTrainingSessionWindowsFact]
    public void StickyIdentityLoss_WinsOverResolverAndBindNormalization()
    {
        RequireWindows();
        using (var resolverFixture = new RetainedRegistryFixture())
        {
            InitializeTrainingArtifacts(resolverFixture.FileAccess);
            var retained = PublishRetainedParent(resolverFixture);
            using var access = resolverFixture.FileAccess.CreateStartup()
                .AcquireRetainedFileAccess();
            var trustLost = false;
            using var factory = Bind(
                access,
                resolverFixture.FileAccess.Root,
                adapterRootOptions:
                    new ServerOwnedContinuousLearningAdapterRootCapabilityOptions
                    {
                        ObserveIoStageForTesting = observation =>
                        {
                            if (trustLost || observation.Stage !=
                                ServerOwnedContinuousLearningAdapterIoStage
                                    .DuringExactRead)
                            {
                                return;
                            }

                            trustLost = true;
                            access.Dispose();
                        },
                    });

            var result = factory.Create(retained.Domain);

            Assert.True(trustLost);
            Assert.False(result.IsSuccess);
            Assert.Equal(
                ContinuousLearningPathIdentityException.StableErrorCode,
                result.ErrorCode);
            Assert.NotEqual("ADAPTER_REFERENCE_UNTRUSTED", result.ErrorCode);
        }

        using var bindFixture = new RetainedRegistryFixture();
        InitializeTrainingArtifacts(bindFixture.FileAccess);
        using var bindAccess = bindFixture.FileAccess.CreateStartup()
            .AcquireRetainedFileAccess();
        var bindTrustLost = false;
        var bind = TryBind(
            bindAccess,
            bindFixture.FileAccess.Root,
            new ServerOwnedContinuousLearningRegistryRootCapabilityOptions
            {
                ObserveIoStageForTesting = stage =>
                {
                    if (bindTrustLost || stage !=
                        ServerOwnedContinuousLearningRegistryIoStage
                            .AfterOperationBeforeRootRevalidation)
                    {
                        return;
                    }

                    bindTrustLost = true;
                    bindAccess.Dispose();
                },
            });

        Assert.True(bindTrustLost);
        Assert.False(bind.IsSuccess);
        Assert.Equal(
            ContinuousLearningPathIdentityException.StableErrorCode,
            bind.ErrorCode);
        Assert.NotEqual(
            RetainedCheckpointBackedIncrementalTrainingSessionFactory
                .BindFailedErrorCode,
            bind.ErrorCode);
    }

    [RetainedCheckpointTrainingSessionWindowsFact]
    public async Task Dispose_WaitsForCreateInsideRetainedIoThenDeniesLaterCreate()
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        InitializeTrainingArtifacts(fixture.FileAccess);
        var retained = PublishRetainedParent(fixture);
        using var insideExactRead = new ManualResetEventSlim();
        using var releaseExactRead = new ManualResetEventSlim();
        using var access = fixture.FileAccess.CreateStartup()
            .AcquireRetainedFileAccess();
        using var factory = Bind(
            access,
            fixture.FileAccess.Root,
            adapterRootOptions:
                new ServerOwnedContinuousLearningAdapterRootCapabilityOptions
                {
                    ObserveIoStageForTesting = observation =>
                    {
                        if (observation.Stage !=
                            ServerOwnedContinuousLearningAdapterIoStage
                                .DuringExactRead)
                        {
                            return;
                        }

                        insideExactRead.Set();
                        Assert.True(releaseExactRead.Wait(TimeSpan.FromSeconds(5)));
                    },
                });
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        Assert.Throws<OperationCanceledException>(
            () => factory.Create("MISSING", cancellation.Token));

        using var disposeStarted = new ManualResetEventSlim();
        var createTask = Task.Run(() => factory.Create(retained.Domain));
        Assert.True(insideExactRead.Wait(TimeSpan.FromSeconds(5)));
        var disposeTask = Task.Run(() =>
        {
            disposeStarted.Set();
            factory.Dispose();
        });
        try
        {
            Assert.True(disposeStarted.Wait(TimeSpan.FromSeconds(5)));
            var completed = await Task.WhenAny(
                disposeTask,
                Task.Delay(TimeSpan.FromMilliseconds(100)));
            Assert.NotSame(disposeTask, completed);
        }
        finally
        {
            releaseExactRead.Set();
        }

        var createResult = await createTask;
        await disposeTask;
        Assert.True(createResult.IsSuccess, createResult.ErrorMessage);
        factory.Dispose();
        access.EnsureRetained();
        var afterDispose = factory.Create("MISSING");
        Assert.False(afterDispose.IsSuccess);
        Assert.Equal(
            RetainedCheckpointBackedIncrementalTrainingSessionFactory
                .DisposedErrorCode,
            afterDispose.ErrorCode);
    }

    [RetainedCheckpointTrainingSessionWindowsFact]
    public void Bind_CleansPartialAcquisitionAndSuccessOwnsOnlyRootCapabilities()
    {
        RequireWindows();
        using (var beforeAdapterFixture = new RetainedRegistryFixture())
        {
            InitializeTrainingArtifacts(beforeAdapterFixture.FileAccess);
            using var access = beforeAdapterFixture.FileAccess.CreateStartup()
                .AcquireRetainedFileAccess();
            using var registryBlocker = access.AcquireRegistryRootCapability();

            // Этот отказ происходит ещё до adapter-child: тест защищает
            // от случайного "съедания" второй дочерней возможности.
            var failedBeforeAdapter =
                TryBind(access, beforeAdapterFixture.FileAccess.Root);

            Assert.False(failedBeforeAdapter.IsSuccess);
            Assert.Equal(
                RetainedCheckpointBackedIncrementalTrainingSessionFactory
                    .BindFailedErrorCode,
                failedBeforeAdapter.ErrorCode);
            Assert.True(GetPrivateBoolean(
                access,
                "_registryRootCapabilityAcquisitionConsumed"));
            Assert.False(GetPrivateBoolean(
                access,
                "_adapterRootCapabilityAcquisitionConsumed"));
            using var adapterStillAvailable = access.AcquireAdapterRootCapability();
            Assert.NotNull(adapterStillAvailable);
        }

        using (var afterRegistryFixture = new RetainedRegistryFixture())
        {
            InitializeTrainingArtifacts(afterRegistryFixture.FileAccess);
            using var access = afterRegistryFixture.FileAccess.CreateStartup()
                .AcquireRetainedFileAccess();
            using var adapterBlocker = access.AcquireAdapterRootCapability();

            // Здесь фабрика уже успела взять registry-child, но adapter-child
            // занят тем же parent lease; finally обязан закрыть registry-child.
            var failedAfterRegistry =
                TryBind(access, afterRegistryFixture.FileAccess.Root);

            Assert.False(failedAfterRegistry.IsSuccess);
            Assert.Equal(
                RetainedCheckpointBackedIncrementalTrainingSessionFactory
                    .BindFailedErrorCode,
                failedAfterRegistry.ErrorCode);
            var failedRegistryRoot =
                GetPrivateField<ServerOwnedContinuousLearningRegistryRootCapability>(
                    access,
                    "_registryRootCapability");
            Assert.True(GetPrivateBoolean(failedRegistryRoot, "_disposed"));
            access.EnsureRetained();
            Assert.Throws<InvalidOperationException>(
                () => access.AcquireRegistryRootCapability());
        }

        using var successFixture = new RetainedRegistryFixture();
        InitializeTrainingArtifacts(successFixture.FileAccess);
        using var successAccess = successFixture.FileAccess.CreateStartup()
            .AcquireRetainedFileAccess();
        var secondParentWhileLeaseAlive =
            Assert.Throws<ContinuousLearningMutationAdmissionDomainAlreadyOwnedException>(
                () => successFixture.FileAccess.CreateStartup()
                    .AcquireRetainedFileAccess());
        Assert.Equal(
            "CONTINUOUS_LEARNING_MUTATION_ADMISSION_DOMAIN_ALREADY_OWNED",
            secondParentWhileLeaseAlive.Message);
        var success = TryBind(successAccess, successFixture.FileAccess.Root);
        Assert.True(success.IsSuccess, success.ErrorMessage);
        var factory = Assert.IsType<
            RetainedCheckpointBackedIncrementalTrainingSessionFactory>(
                success.Value);
        var successRegistryRoot =
            GetPrivateField<ServerOwnedContinuousLearningRegistryRootCapability>(
                successAccess,
                "_registryRootCapability");
        var successAdapterRoot =
            GetPrivateField<ServerOwnedContinuousLearningAdapterRootCapability>(
                successAccess,
                "_adapterRootCapability");
        Assert.True(GetPrivateBoolean(
            successAccess,
            "_registryRootCapabilityAcquisitionConsumed"));
        Assert.True(GetPrivateBoolean(
            successAccess,
            "_adapterRootCapabilityAcquisitionConsumed"));
        var secondBind = TryBind(successAccess, successFixture.FileAccess.Root);
        Assert.False(secondBind.IsSuccess);
        Assert.Equal(
            RetainedCheckpointBackedIncrementalTrainingSessionFactory
                .BindFailedErrorCode,
            secondBind.ErrorCode);

        factory.Dispose();
        factory.Dispose();
        Assert.True(GetPrivateBoolean(successRegistryRoot, "_disposed"));
        Assert.True(GetPrivateBoolean(successAdapterRoot, "_disposed"));
        successAccess.EnsureRetained();
        Assert.Throws<ContinuousLearningMutationAdmissionDomainAlreadyOwnedException>(
            () => successFixture.FileAccess.CreateStartup()
                .AcquireRetainedFileAccess());

        successAccess.Dispose();
        using var postDisposeAccess = successFixture.FileAccess.CreateStartup()
            .AcquireRetainedFileAccess();
        using var postDisposeRegistry =
            postDisposeAccess.AcquireRegistryRootCapability();
        using var postDisposeAdapter =
            postDisposeAccess.AcquireAdapterRootCapability();
        Assert.NotNull(postDisposeRegistry);
        Assert.NotNull(postDisposeAdapter);
    }

    [RetainedCheckpointTrainingSessionWindowsFact]
    public void ExactReadCancellationAndFatalGraphs_PropagateWithoutNormalization()
    {
        RequireWindows();
        AggregateException[] graphs =
        [
            new(new InvalidOperationException(
                "outer",
                new OperationCanceledException("in-flight exact read"))),
            new(new InvalidOperationException(
                "outer",
                new OutOfMemoryException("fatal exact read"))),
        ];

        foreach (var graph in graphs)
        {
            using var fixture = new RetainedRegistryFixture();
            InitializeTrainingArtifacts(fixture.FileAccess);
            var retained = PublishRetainedParent(fixture);
            using var access = fixture.FileAccess.CreateStartup()
                .AcquireRetainedFileAccess();
            using var factory = Bind(
                access,
                fixture.FileAccess.Root,
                adapterRootOptions:
                    new ServerOwnedContinuousLearningAdapterRootCapabilityOptions
                    {
                        ObserveIoStageForTesting = observation =>
                        {
                            if (observation.Stage ==
                                ServerOwnedContinuousLearningAdapterIoStage
                                    .DuringExactRead)
                            {
                                throw graph;
                            }
                        },
                    });

            Assert.Same(
                graph,
                Assert.Throws<AggregateException>(
                    () => factory.Create(retained.Domain)));
        }
    }

    [RetainedCheckpointTrainingSessionWindowsFact]
    public void RetainedArtifactAuthority_BlocksNameMutationAndHasNoRawAuthoritySurface()
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        InitializeTrainingArtifacts(fixture.FileAccess);
        var expectedBaseHash = Hash(fixture.FileAccess.BaseCheckpointPath);
        var expectedTokenizerHash = Hash(fixture.FileAccess.TokenizerPath);
        using var access = fixture.FileAccess.CreateStartup()
            .AcquireRetainedFileAccess();
        using var factory = Bind(access, fixture.FileAccess.Root);
        foreach (var path in new[]
                 {
                     fixture.FileAccess.BaseCheckpointPath,
                     fixture.FileAccess.TokenizerPath,
                 })
        {
            var moved = path + ".moved";
            var replacement = path + ".replacement";
            File.WriteAllBytes(replacement, [0xde, 0xad, 0xbe, 0xef]);
            Assert.ThrowsAny<IOException>(() => File.Move(path, moved));
            Assert.ThrowsAny<IOException>(() => File.Delete(path));
            Assert.ThrowsAny<IOException>(
                () => File.Replace(replacement, path, destinationBackupFileName: null));
            Assert.ThrowsAny<IOException>(
                () => File.WriteAllBytes(path, [0xba, 0xad, 0xf0, 0x0d]));
        }

        var result = factory.Create("MISSING");

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(expectedBaseHash, result.Value!.BaseCheckpointSha256);
        Assert.Equal(expectedTokenizerHash, result.Value.TokenizerSha256);
        AssertNoDirectPathOrHandleState(factory);
        var fields = factory.GetType().GetFields(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.DoesNotContain(fields, field => field.FieldType == typeof(byte[]));
        var source = ReadRepositorySource(
            "14.ContinuousLearning",
            "Training",
            "RetainedCheckpointBackedIncrementalTrainingSessionFactory.cs");
        Assert.DoesNotContain("Path.", source, StringComparison.Ordinal);
        Assert.DoesNotContain("File.", source, StringComparison.Ordinal);
        Assert.DoesNotContain("FileStream", source, StringComparison.Ordinal);
        Assert.DoesNotContain("SafeFileHandle", source, StringComparison.Ordinal);
        Assert.DoesNotContain("byte[]", source, StringComparison.Ordinal);
        Assert.Contains(
            "_fileAccess.OpenRetainedArtifactForRead(role)",
            source,
            StringComparison.Ordinal);
    }

    [RetainedCheckpointTrainingSessionWindowsFact]
    public void MalformedRetainedCheckpoint_CannotCrossExactResolverBoundary()
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        InitializeTrainingArtifacts(fixture.FileAccess);
        var retained = PublishRetainedParent(fixture);
        var artifactPath = Path.Combine(
            fixture.FileAccess.AdapterRoot,
            retained.Reference.CanonicalLeafName);
        var malformedBytes = new byte[retained.Bytes.Length];
        malformedBytes[0] = 0xff;
        malformedBytes[^1] = 0x7f;
        File.WriteAllBytes(artifactPath, malformedBytes);
        using var malformedStream = new MemoryStream(
            malformedBytes,
            writable: false);
        var malformedHash = TrainingSourceProvenance.Sha256(malformedStream);
        var malformed = retained with
        {
            Bytes = malformedBytes,
            Provenance = retained.Provenance with
            {
                AdapterSha256 = malformedHash,
            },
            Reference = retained.Reference with
            {
                Sha256Hex = malformedHash,
            },
        };
        File.WriteAllBytes(
            fixture.RegistryPath,
            SerializeRetainedRegistry(malformed));
        using var access = fixture.FileAccess.CreateStartup()
            .AcquireRetainedFileAccess();
        using var factory = Bind(access, fixture.FileAccess.Root);

        var result = factory.Create(malformed.Domain);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            ContinuousLearningPathIdentityException.StableErrorCode,
            result.ErrorCode);
        Assert.NotEqual("ADAPTER_CHECKPOINT_INVALID", result.ErrorCode);
        Assert.Throws<ContinuousLearningPathIdentityException>(
            access.EnsureRetained);
    }

    [RetainedCheckpointTrainingSessionWindowsFact]
    public void ExistingInactiveGuard_RemainsExecutionDeniedWithoutArtifactRead()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        var before = fixture.Snapshot();
        var factory = new InactiveIncrementalTrainingSessionFactoryGuard();

        var result = factory.Create("BUG");

        Assert.False(result.IsSuccess);
        Assert.Equal("CONTINUOUS_LEARNING_TRAINING_INACTIVE", result.ErrorCode);
        Assert.Equal(before, fixture.Snapshot());
        var compositionSource = ReadRepositorySource(
            "14.ContinuousLearning",
            "Training",
            "ContinuousLearningProductionComposition.cs");
        Assert.Contains(
            "RetainedCheckpointBackedIncrementalTrainingSessionFactory.Bind(",
            compositionSource,
            StringComparison.Ordinal);
        Assert.Contains(
            "new InactiveIncrementalTrainingSessionFactoryGuard()",
            compositionSource,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "InactiveRetainedObjectDomainModelRegistry",
            compositionSource,
            StringComparison.Ordinal);
        Assert.Contains(
            "CONTINUOUS_LEARNING_TRAINING_INACTIVE",
            compositionSource,
            StringComparison.Ordinal);
    }

    private static RetainedCheckpointBackedIncrementalTrainingSessionFactory Bind(
        ServerOwnedContinuousLearningFileAccess access,
        string root,
        ServerOwnedContinuousLearningRegistryRootCapabilityOptions?
            registryRootOptions = null,
        ServerOwnedContinuousLearningAdapterRootCapabilityOptions?
            adapterRootOptions = null,
        Action<ContinuousLearningRetainedArtifactRole>?
            afterRetainedArtifactOpenForTesting = null,
        Func<
            RepositoryOwnedIncrementalTrainingSessionStartRequest,
            OperationResult<RepositoryOwnedModelCheckpointLoadResult>>?
            loadRepositoryCheckpointForTesting = null,
        Func<
            string,
            OperationResult<RepositoryOwnedIncrementalTrainingSessionStartRequest>>?
            resolveRepositoryStartRequest = null)
    {
        var result = TryBind(
            access,
            root,
            registryRootOptions,
            adapterRootOptions,
            afterRetainedArtifactOpenForTesting,
            loadRepositoryCheckpointForTesting,
            resolveRepositoryStartRequest);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        return Assert.IsType<
            RetainedCheckpointBackedIncrementalTrainingSessionFactory>(
                result.Value);
    }

    private static OperationResult<
        RetainedCheckpointBackedIncrementalTrainingSessionFactory> TryBind(
        ServerOwnedContinuousLearningFileAccess access,
        string root,
        ServerOwnedContinuousLearningRegistryRootCapabilityOptions?
            registryRootOptions = null,
        ServerOwnedContinuousLearningAdapterRootCapabilityOptions?
            adapterRootOptions = null,
        Action<ContinuousLearningRetainedArtifactRole>?
            afterRetainedArtifactOpenForTesting = null,
        Func<
            RepositoryOwnedIncrementalTrainingSessionStartRequest,
            OperationResult<RepositoryOwnedModelCheckpointLoadResult>>?
            loadRepositoryCheckpointForTesting = null,
        Func<
            string,
            OperationResult<RepositoryOwnedIncrementalTrainingSessionStartRequest>>?
            resolveRepositoryStartRequest = null)
        => RetainedCheckpointBackedIncrementalTrainingSessionFactory.Bind(
            access,
            new RetainedRegistryNullStorage(),
            ModelConfiguration,
            CreateTrainingConfiguration(root),
            registryRootOptions,
            adapterRootOptions,
            afterRetainedArtifactOpenForTesting,
            loadRepositoryCheckpointForTesting,
            resolveRepositoryStartRequest);

    private static RepositoryOwnedIncrementalTrainingSessionStartRequest
        RepositoryStartRequest(
            FileAccessFixture fixture,
            RepositoryOwnedIncrementalTrainingColdStartPolicy? coldStartPolicy = null,
            RepositoryOwnedModelRuntimeCompatibility? runtimeCompatibility = null,
            RepositoryOwnedDpoDatasetManifestIdentity?
                currentDpoDatasetManifest = null)
        => new(
            fixture.RepositoryRoot,
            RepositoryDomainSlug,
            RepositoryDesignPatternSlug,
            RepositoryModelIdSlug,
            RepositoryModelVersion,
            runtimeCompatibility ?? CompatibleRuntimeCompatibility(),
            coldStartPolicy
            ?? RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied,
            CurrentDpoDatasetManifest: currentDpoDatasetManifest);

    private static void AssertRepositoryFailureDoesNotOpenRetainedBase(
        Action<RetainedRegistryFixture> arrangeRepository,
        string expectedErrorCode,
        Func<
            RepositoryOwnedIncrementalTrainingSessionStartRequest,
            OperationResult<RepositoryOwnedModelCheckpointLoadResult>>?
            loadRepositoryCheckpointForTesting = null)
    {
        using var fixture = new RetainedRegistryFixture();
        InitializeTrainingArtifacts(fixture.FileAccess);
        arrangeRepository(fixture);
        var retainedBaseOpenCount = 0;
        using var access = fixture.FileAccess.CreateStartup()
            .AcquireRetainedFileAccess();
        using var factory = Bind(
            access,
            fixture.FileAccess.Root,
            afterRetainedArtifactOpenForTesting: role =>
            {
                if (role == ContinuousLearningRetainedArtifactRole.BaseCheckpoint)
                    retainedBaseOpenCount++;
            },
            loadRepositoryCheckpointForTesting: loadRepositoryCheckpointForTesting);

        var result = factory.Create(RepositoryStartRequest(
            fixture.FileAccess,
            RepositoryOwnedIncrementalTrainingColdStartPolicy.Authorize(
                "operator approved cold start only if no repository checkpoint exists")));

        Assert.False(result.IsSuccess);
        Assert.Equal(expectedErrorCode, result.ErrorCode);
        Assert.Null(result.Value);
        Assert.Equal(0, retainedBaseOpenCount);
    }

    private static void AssertRepositoryResumeMetadata(
        IncrementalTrainingSession session,
        RepositoryOwnedModelCheckpointManifest expectedManifest)
    {
        var metadata = Assert.IsType<IncrementalTrainingCheckpointStartMetadata>(
            session.CheckpointStart);
        Assert.Equal(
            IncrementalTrainingCheckpointStartSource.RepositoryOwnedModelCheckpoint,
            metadata.Source);
        Assert.Null(metadata.ColdStart);
        Assert.Null(session.ColdStartCheckpoint);
        var parent = Assert.IsType<RepositoryOwnedModelCheckpointManifest>(
            metadata.ResumeParentCheckpoint);
        Assert.Same(parent, session.ResumeParentCheckpoint);
        Assert.Equal(expectedManifest.ManifestSha256, parent.ManifestSha256);
        Assert.Equal(expectedManifest.WeightsSha256, parent.WeightsSha256);
        Assert.Equal(expectedManifest.WeightsByteLength, parent.WeightsByteLength);
        Assert.Equal(expectedManifest.RepositoryRelativeManifestPath,
            parent.RepositoryRelativeManifestPath);
        var startRequest = Assert.IsType<
            RepositoryOwnedIncrementalTrainingSessionStartRequest>(
            metadata.RepositoryStartRequest);
        Assert.Equal(expectedManifest.Domain, startRequest.DomainSlug);
        Assert.Equal(expectedManifest.DesignPattern, startRequest.DesignPatternSlug);
        Assert.Equal(expectedManifest.ModelId, startRequest.ModelIdSlug);
        Assert.Equal(expectedManifest.Version, startRequest.ModelVersion);
    }

    private static IncrementalTrainingSession ReplaceCheckpointStart(
        IncrementalTrainingSession session,
        IncrementalTrainingCheckpointStartMetadata checkpointStart)
        => new(
            session.Model,
            session.Tokenizer,
            session.Trainer,
            session.Optimizer,
            session.ParentAdapterCheckpoint,
            session.BaseCheckpointSha256,
            session.TokenizerSha256,
            session.ModelIdentity,
            checkpointStart);

    private static RepositoryOwnedModelCheckpointManifest
        SealRepositoryModelCheckpoint(
            FileAccessFixture fixture,
            out RepositoryOwnedModelCheckpointAddress address,
            out IReadOnlyList<int[]> modelBits,
            RepositoryOwnedModelRuntimeCompatibility? runtimeCompatibility = null,
            Action<LlmModel>? mutateModel = null)
    {
        var model = new LlmModel(ModelConfiguration);
        mutateModel?.Invoke(model);
        modelBits = SnapshotBits(model.BaseParameters);
        var bytes = SaveCheckpointBytes(model);
        address = CreateRepositoryCheckpointAddress(fixture);
        using var stream = new MemoryStream(bytes, writable: false);
        var result = RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(
            address,
            stream,
            CreateRepositoryCheckpointManifestRequest(
                fixture,
                runtimeCompatibility));
        Assert.True(result.IsSuccess, result.ErrorMessage);
        return result.Value!;
    }

    private static RepositoryOwnedModelCheckpointAddress
        CreateRepositoryCheckpointAddress(FileAccessFixture fixture, int step = 1)
    {
        var address = RepositoryOwnedTrainingArtifactLayout
            .CreateModelCheckpointAddress(
                fixture.RepositoryRoot,
                RepositoryDomainSlug,
                RepositoryDesignPatternSlug,
                RepositoryModelIdSlug,
                RepositoryModelVersion,
                step);
        Assert.True(address.IsSuccess, address.ErrorMessage);
        return address.Value!;
    }

    private static RepositoryOwnedDpoDatasetManifestIdentity
        CreateSealedDpoManifestIdentity(
            FileAccessFixture fixture,
            string datasetVersion)
    {
        var address = CreateRepositoryDpoDatasetAddress(
            fixture,
            datasetVersion);
        var pair = BoundOpenTeacherRejectedPair(
            $"retained-session-factory-{datasetVersion}",
            RepositoryDomainSlug,
            "prompt for retained checkpoint DPO fixture",
            "chosen answer from accepted reference",
            "rejected answer from local open teacher fixture",
            DateTimeOffset.UnixEpoch.AddSeconds(41));
        var manifest =
            RepositoryOwnedDpoDatasetManifestSealer.PersistAndSeal(
                address,
                CreateRepositoryDpoRequestIdentity(),
                [pair]);
        Assert.True(manifest.IsSuccess, manifest.ErrorMessage);
        var reopened =
            RepositoryOwnedDpoDatasetManifestSealer.OpenSealed(
                address,
                CreateRepositoryDpoRequestIdentity());
        Assert.True(reopened.IsSuccess, reopened.ErrorMessage);
        Assert.Equal(manifest.Value!.ManifestSha256,
            reopened.Value!.Manifest.ManifestSha256);
        Assert.Equal(1, reopened.Value.Manifest.PairCount);

        return new RepositoryOwnedDpoDatasetManifestIdentity(
            address.ManifestRelativePath,
            manifest.Value.ManifestSha256);
    }

    private static RepositoryOwnedDpoDatasetRequestIdentity
        CreateRepositoryDpoRequestIdentity()
        => AssertSuccess(
            RepositoryOwnedDpoDatasetManifestSealer.CreateRequestIdentity(
                [Sha256(Encoding.UTF8.GetBytes(
                    "retained-factory-paid-reference"))],
                new RepositoryOwnedDpoOpenTeacherIdentity(
                    OpenWeightTeacherFamily.Qwen.ToString(),
                    "qwen3.5:9b",
                    FixtureManifestDigest,
                    LegalTrainingTeacherAdmissionAuthority
                        .OllamaModelLayerSha256DigestKind,
                    FixtureLayerDigest),
                new RepositoryOwnedDpoGenerationPolicyIdentity(
                    "retained-factory-generation-policy",
                    "v1",
                    new string('2', 64)),
                new RepositoryOwnedDpoCreatorIdentity(
                    "retained-factory-creator",
                    "v1",
                    new string('3', 64))));

    private static RepositoryOwnedDpoDatasetManifestIdentity
        CreateHashMatchingUnsealedDpoManifestIdentity(
            FileAccessFixture fixture,
            string datasetVersion)
    {
        var dpoManifestRelativePath = string.Join(
            '/',
            "training-data",
            "dpo",
            RepositoryDomainSlug,
            RepositoryDesignPatternSlug,
            datasetVersion,
            "manifest.json");
        var dpoManifestBytes = Encoding.UTF8.GetBytes(
            "{\"schema\":\"retained-session-factory-repository-start-fixture\"," +
            $"\"datasetVersion\":\"{datasetVersion}\"}}");
        var dpoManifestPath = RepositoryRelativePath(
            fixture.RepositoryRoot,
            dpoManifestRelativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(dpoManifestPath)!);
        File.WriteAllBytes(dpoManifestPath, dpoManifestBytes);
        return new RepositoryOwnedDpoDatasetManifestIdentity(
            dpoManifestRelativePath,
            Sha256(dpoManifestBytes));
    }

    private static RepositoryOwnedDpoDatasetAddress
        CreateRepositoryDpoDatasetAddress(
            FileAccessFixture fixture,
            string datasetVersion)
    {
        var address = RepositoryOwnedTrainingArtifactLayout
            .CreateDpoDatasetAddress(
                fixture.RepositoryRoot,
                RepositoryDomainSlug,
                RepositoryDesignPatternSlug,
                datasetVersion);
        Assert.True(address.IsSuccess, address.ErrorMessage);
        return address.Value!;
    }

    private static RepositoryOwnedModelCheckpointManifestRequest
        CreateRepositoryCheckpointManifestRequest(
            FileAccessFixture fixture,
            RepositoryOwnedModelRuntimeCompatibility? runtimeCompatibility = null)
    {
        return new RepositoryOwnedModelCheckpointManifestRequest(
            ParentCheckpoint: null,
            ColdStartReason: "explicit-test-first-repository-checkpoint",
            DpoDatasetManifest: CreateSealedDpoManifestIdentity(
                fixture,
                "v0001"),
            RuntimeCompatibility: runtimeCompatibility
                ?? CompatibleRuntimeCompatibility(),
            CreatedAtUtc: DateTimeOffset.UnixEpoch,
            ProvenanceRefs:
            [
                "retained-session-factory-repository-start-test",
            ]);
    }

    private static DpoPair BoundOpenTeacherRejectedPair(
        string name,
        string domain,
        string prompt,
        string chosen,
        string rejected,
        DateTimeOffset capturedAt)
    {
        var passport = OpenTeacherPassport(name);
        return AssertSuccess(DpoPairTrainingContentAuthority.Bind(new DpoPair(
            prompt,
            chosen,
            rejected,
            domain,
            capturedAt)
        {
            TeacherProvenance = passport,
            GovernanceDecision = Governance('b'),
            ChosenAuthorship = new DpoPairSideAuthorship(
                DpoPairSideAuthorKind.User),
            RejectedAuthorship = new DpoPairSideAuthorship(
                DpoPairSideAuthorKind.OpenTeacher,
                passport),
        }));
    }

    private static TrainingTeacherProvenance OpenTeacherPassport(string requestId)
        => new(
            TrainingTeacherSourceKind.ExternalTeacherOutput,
            OpenWeightTeacherFamily.Qwen,
            TrainingTeacherAccessMode.LocalHostedOpenWeight,
            TrainingTeacherHostingProvider.Ollama,
            "qwen3.5:9b",
            FixtureManifestDigest,
            TrainingTeacherModelLicense.Apache20,
            ProviderTermsChecked: false,
            ProviderTermsAllowTraining: false,
            ClosedProprietaryModel: false,
            TeacherOutputUsedForTraining: true,
            RequestId(requestId),
            InvocationCount: 1)
        {
            ModelArtifactDigestKind = LegalTrainingTeacherAdmissionAuthority
                .OllamaModelLayerSha256DigestKind,
            ModelArtifactDigest = FixtureLayerDigest,
        };

    private static TrainingDataGovernanceDecision Governance(char evidence)
        => new(
            licenseAccepted: true,
            isQuarantined: false,
            [$"sha256:{new string(evidence, 64)}"]);

    private static T AssertSuccess<T>(OperationResult<T> result)
    {
        Assert.True(result.IsSuccess, result.ErrorMessage);
        return result.Value!;
    }

    private static string RequestId(string value)
        => Sha256(Encoding.UTF8.GetBytes(value));

    private static bool OwnCheckpointStagingFilesRemain(
        RepositoryOwnedModelCheckpointAddress address)
        => Directory.Exists(address.CheckpointRoot)
           && Directory
               .EnumerateFiles(
                   address.CheckpointRoot,
                   "*.tmp",
                   SearchOption.TopDirectoryOnly)
               .Select(Path.GetFileName)
               .Any(static name =>
                   name is not null
                   && (name.StartsWith(".weights.", StringComparison.Ordinal)
                       || name.StartsWith(
                           ".manifest.",
                           StringComparison.Ordinal)));

    private static RepositoryOwnedModelRuntimeCompatibility
        CompatibleRuntimeCompatibility()
        => new(
            ConfigSha256: new string('b', 64),
            TopologySha256: new string('c', 64),
            TokenizerSha256: new string('d', 64),
            TrainerSha256: new string('e', 64),
            SchemaSha256: new string('f', 64),
            CompatibilityKey: "retained-session-factory-v1");

    private static RepositoryOwnedModelRuntimeCompatibility
        IncompatibleRuntimeCompatibility()
        => CompatibleRuntimeCompatibility() with
        {
            TrainerSha256 = new string('9', 64),
        };

    private static void AddToEveryBaseParameter(LlmModel model, float delta)
    {
        foreach (var parameter in model.BaseParameters)
        {
            for (var index = 0; index < parameter.Value.Data.Length; index++)
                parameter.Value.Data[index] += delta;
        }
    }

    private static byte[] SaveCheckpointBytes(LlmModel model)
    {
        using var stream = new MemoryStream();
        model.SaveCheckpointToStream(stream);
        return stream.ToArray();
    }

    private static void AssertCheckpointWeightsWriteShareReleased(string path)
    {
        using var stream = new FileStream(
            path,
            FileMode.Open,
            System.IO.FileAccess.ReadWrite,
            FileShare.ReadWrite);
        Assert.True(stream.CanRead);
        Assert.True(stream.CanWrite);
    }

    private static PublishedParent PublishRetainedParent(
        RetainedRegistryFixture fixture)
    {
        using var access = fixture.FileAccess.CreateStartup()
            .AcquireRetainedFileAccess();
        using var registryRoot = access.AcquireRegistryRootCapability();
        using var adapterRoot = access.AcquireAdapterRootCapability();
        var model = new LlmModel(ModelConfiguration);
        var adapters = LoraApplicator.Apply(
            model,
            IncrementalTrainer.DefaultConfig);
        var adapterParameters = adapters
            .SelectMany(static adapter => adapter.AdapterParameters)
            .Select(static entry => entry.Parameter)
            .Distinct<Parameter>(ReferenceEqualityComparer.Instance)
            .ToArray();
        foreach (var parameter in adapterParameters)
        {
            for (var index = 0; index < parameter.Value.Data.Length; index++)
                parameter.Value.Data[index] += 0.375f;
        }
        var adapterBits = SnapshotBits(adapterParameters);
        var publication = adapterRoot.Publish(
            model,
            ServerOwnedContinuousLearningAdapterPublicationRequest.Create(
                "bug",
                "parent"));
        var reference = AdapterArtifactReference.FromPublication(publication);
        var source = TrainingSourceProvenance.CreateSourceId(
            "retained-session/BUG/parent",
            "retained parent publication");
        var manifest = TrainingSourceProvenance.BuildManifest([source]).Value!;
        var provenance = new ModelAdapterProvenance(
            manifest,
            publication.Sha256Hex,
            Hash(fixture.FileAccess.BaseCheckpointPath),
            Hash(fixture.FileAccess.TokenizerPath),
            TrainingSourceProvenance.Identify(ModelConfiguration),
            TrainingSourceProvenance.Identify(IncrementalTrainer.DefaultConfig));
        var retained = new PublishedParent(
            "BUG",
            3,
            DateTimeOffset.UtcNow,
            provenance,
            reference,
            File.ReadAllBytes(Path.Combine(
                fixture.FileAccess.AdapterRoot,
                reference.CanonicalLeafName)),
            adapterBits);
        var registryBytes = SerializeRetainedRegistry(retained);
        PublishInitialRegistry(registryRoot, registryBytes);
        return retained;
    }

    private static byte[] SerializeRetainedRegistry(PublishedParent retained)
        => JsonSerializer.SerializeToUtf8Bytes(new
        {
            SchemaVersion = 2,
            Adapters = new[]
            {
                new
                {
                    retained.Domain,
                    retained.TrainingExamples,
                    retained.TrainedAt,
                    retained.Provenance,
                    StorageKind = 2,
                    RetainedReference = retained.Reference,
                },
            },
        });

    private static void PublishInitialRegistry(
        ServerOwnedContinuousLearningRegistryRootCapability registryRoot,
        byte[] bytes)
    {
        var prior =
            ServerOwnedContinuousLearningRegistryPriorStateEvidence.Absent();
        var written = registryRoot.CreateWriteTemp(bytes);
        var candidate = new ServerOwnedContinuousLearningRegistrySnapshot(
            written.Identity,
            bytes);
        Assert.True(registryRoot.ReadExact(
            ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
            written.Identity).Matches(candidate));
        var rollback = registryRoot.PreparePublication(candidate, prior);
        registryRoot.RenameNoReplace(
            ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
            candidate.Identity,
            ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry);
        registryRoot.FlushFile(
            ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry,
            candidate.Identity);
        registryRoot.FlushRootDirectory();
        var accepted = registryRoot.CommitPublication(candidate, prior, rollback);
        _ = registryRoot.ResumeAcceptedCleanup(accepted);
    }

    private static void InitializeTrainingArtifacts(FileAccessFixture fixture)
    {
        new LlmModel(ModelConfiguration).SaveCheckpoint(
            fixture.BaseCheckpointPath);
        var tokenizer = new BpeTokenizer();
        tokenizer.Train("retained checkpoint tokenizer corpus", 256);
        tokenizer.Save(fixture.TokenizerPath);
    }

    private static TrainingConfig CreateTrainingConfiguration(string root)
        => new()
        {
            BatchSize = 1,
            SeqLen = 32,
            MaxLr = 1e-2f,
            MinLr = 1e-2f,
            WarmupSteps = 0,
            TotalSteps = 10,
            GradAccumSteps = 1,
            GradClipNorm = 1f,
            WeightDecay = 0f,
            CheckpointDir = Path.Combine(root, "unused-checkpoints"),
        };

    private static IncrementalTrainer CreateTrainerWithStepExecutor(
        IIncrementalTrainingSessionFactory sessionFactory,
        ISupervisedTrainingStepExecutor supervisedTrainingStepExecutor,
        IDpoTrainingExecutor dpoTrainingExecutor,
        string adapterDirectory)
    {
        return new IncrementalTrainer(
            new RetainedRegistryNullStorage(),
            sessionFactory,
            ServerOwnedLegalTrainingTeacherAdmissionAuthority.Unavailable(),
            dpoTrainingExecutor,
            supervisedTrainingStepExecutor,
            adapterDirectory,
            IncrementalTrainer.DefaultConfig);
    }

    private static TrainingExample ValidExample(
        string domain,
        string prompt,
        string response)
        => new(
            prompt,
            response,
            domain,
            1d,
            DateTimeOffset.UtcNow,
            TrainingSourceProvenance.CreateSourceId(
                $"retained-session/{domain}/{prompt}",
                string.Concat(prompt, "\n", response)));

    private static IReadOnlyList<Parameter> AdapterParameters(LlmModel model)
    {
        var baseParameters = new HashSet<Parameter>(
            model.BaseParameters,
            ReferenceEqualityComparer.Instance);
        return model.Parameters()
            .Where(parameter => !baseParameters.Contains(parameter))
            .ToArray();
    }

    private static void AssertBitsEqual(
        IReadOnlyList<int[]> expected,
        IEnumerable<Parameter> actual)
    {
        var actualParameters = actual.ToArray();
        Assert.Equal(expected.Count, actualParameters.Length);
        for (var index = 0; index < expected.Count; index++)
        {
            Assert.Equal(
                expected[index],
                actualParameters[index].Value.Data
                    .Select(BitConverter.SingleToInt32Bits)
                    .ToArray());
        }
    }

    private static void AssertNoDirectPathOrHandleState(
        RetainedCheckpointBackedIncrementalTrainingSessionFactory factory)
    {
        Assert.DoesNotContain(
            factory.GetType().GetFields(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic),
            field => field.FieldType == typeof(string)
                     || typeof(SafeHandle).IsAssignableFrom(field.FieldType)
                     || typeof(FileSystemInfo).IsAssignableFrom(field.FieldType)
                     || typeof(Stream).IsAssignableFrom(field.FieldType));
        Assert.Empty(factory.GetType().GetProperties(
            BindingFlags.Instance | BindingFlags.Public));
    }

    private static bool GetPrivateBoolean(object instance, string fieldName)
        => Assert.IsType<bool>(instance.GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(instance));

    private static T GetPrivateField<T>(object instance, string fieldName)
        => Assert.IsType<T>(instance.GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(instance));

    private static string Hash(string path)
    {
        using var stream = File.OpenRead(path);
        return TrainingSourceProvenance.Sha256(stream);
    }

    private static string Sha256(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static string RepositoryRelativePath(
        string repositoryRoot,
        string relativePath)
        => Path.Combine(
            new[] { repositoryRoot }
                .Concat(relativePath.Split('/'))
                .ToArray());

    private static byte[] ReadAll(Stream stream)
    {
        using var copy = new MemoryStream();
        stream.CopyTo(copy);
        return copy.ToArray();
    }

    private static string ReadRepositorySource(params string[] pathSegments)
    {
        var repositoryRoot = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            ".."));
        return File.ReadAllText(Path.Combine(
                [repositoryRoot, .. pathSegments]))
            .Replace("\r\n", "\n", StringComparison.Ordinal);
    }

    private static IReadOnlyList<int[]> SnapshotBits(
        IEnumerable<Parameter> parameters)
        => parameters
            .Select(parameter => parameter.Value.Data
                .Select(BitConverter.SingleToInt32Bits)
                .ToArray())
            .ToArray();

    private static void RequireWindows()
        => Assert.True(
            OperatingSystem.IsWindows(),
            "Retained training-session creation requires Windows capabilities.");

    private sealed class CancellationAwareSessionFactoryProbe
        : IIncrementalTrainingSessionFactory
    {
        private readonly Func<CancellationToken,
            OperationResult<IncrementalTrainingSession>> _create;

        internal CancellationAwareSessionFactoryProbe(
            Func<CancellationToken,
                OperationResult<IncrementalTrainingSession>> create)
            => _create = create;

        internal int LegacyCreateCount { get; private set; }
        internal int CancellationAwareCreateCount { get; private set; }
        internal CancellationToken? ObservedToken { get; private set; }

        public OperationResult<IncrementalTrainingSession> Create(string domain)
        {
            _ = domain;
            LegacyCreateCount++;
            return OperationResult<IncrementalTrainingSession>.Fail(
                "Legacy overload was called unexpectedly.",
                "LEGACY_OVERLOAD_CALLED");
        }

        public OperationResult<IncrementalTrainingSession> Create(
            string domain,
            CancellationToken cancellationToken)
        {
            _ = domain;
            CancellationAwareCreateCount++;
            ObservedToken = cancellationToken;
            return _create(cancellationToken);
        }
    }

    private sealed class LegacySessionFactoryProbe
        : IIncrementalTrainingSessionFactory
    {
        internal int CreateCount { get; private set; }

        public OperationResult<IncrementalTrainingSession> Create(string domain)
        {
            _ = domain;
            CreateCount++;
            return OperationResult<IncrementalTrainingSession>.Fail(
                "The compatible legacy factory was reached.",
                "LEGACY_FACTORY_REACHED");
        }
    }

    private sealed class FixedSessionFactory
        : IIncrementalTrainingSessionFactory
    {
        private readonly IncrementalTrainingSession _session;

        internal FixedSessionFactory(IncrementalTrainingSession session)
            => _session = session;

        public OperationResult<IncrementalTrainingSession> Create(string domain)
        {
            _ = domain;
            return OperationResult<IncrementalTrainingSession>.Ok(_session);
        }
    }

    private sealed class LoadedParentObservationExecutor
        : ISupervisedTrainingStepExecutor
    {
        private readonly IReadOnlyList<int[]> _expectedBits;

        internal LoadedParentObservationExecutor(IReadOnlyList<int[]> expectedBits)
            => _expectedBits = expectedBits;

        internal int ExecuteCount { get; private set; }
        internal bool ObservedExactLoadedParent { get; private set; }
        internal IncrementalTrainingSession? ObservedSession { get; private set; }

        public float Execute(
            IncrementalTrainingSession session,
            IReadOnlyList<int> tokenIds,
            float weight,
            IReadOnlyList<Parameter> adapterParameters)
        {
            _ = tokenIds;
            _ = weight;
            ExecuteCount++;
            ObservedSession = session;
            AssertBitsEqual(_expectedBits, adapterParameters);
            Assert.All(
                session.Model.BaseParameters.Concat(adapterParameters),
                parameter => Assert.True(
                    parameter.Grad is null
                    || parameter.Grad.Data.All(static value => value == 0f)));
            ObservedExactLoadedParent = true;
            throw new InvalidOperationException(
                "STOP_BEFORE_GRADIENT_OPTIMIZER_SAVE_OR_DPO");
        }
    }

    private sealed class NeverDpoExecutor : IDpoTrainingExecutor
    {
        internal int ExecuteCount { get; private set; }

        public Task<OperationResult<DpoTrainingExecutionArtifact>> ExecuteAsync(
            DpoTrainingExecutionRequest request,
            CancellationToken cancellationToken = default)
        {
            _ = request;
            _ = cancellationToken;
            ExecuteCount++;
            throw new Xunit.Sdk.XunitException(
                "DPO executed for an empty preference batch.");
        }
    }

    private sealed record PublishedParent(
        string Domain,
        int TrainingExamples,
        DateTimeOffset TrainedAt,
        ModelAdapterProvenance Provenance,
        AdapterArtifactReference Reference,
        byte[] Bytes,
        IReadOnlyList<int[]> AdapterBits);
}

file sealed class RetainedCheckpointTrainingSessionWindowsFactAttribute : FactAttribute
{
    public RetainedCheckpointTrainingSessionWindowsFactAttribute()
    {
        if (!OperatingSystem.IsWindows())
        {
            // Эти проверки фабрики сессий зависят от удержанной Windows-
            // capability реестра. Вне Windows честное доказательство — видимая
            // строка NotExecuted, а не падение RequireWindows.
            Skip = "WINDOWS_ONLY_CAPABILITY_ORACLE: Retained training-session creation requires Windows capabilities.";
        }
    }
}
