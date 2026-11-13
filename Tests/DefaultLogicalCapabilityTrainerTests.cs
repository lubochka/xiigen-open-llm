// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Buffers.Binary;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using DPO;
using DPO.Models;
using SemanticCompiler;
using SemanticCompilerAdapter;
using Tokenizer;
using Domain = SemanticCompiler29.Domain;

[Collection("F42PublicationSerial")]
public sealed class DefaultLogicalCapabilityTrainerTests
{
    [Fact]
    public async Task DefaultLogicalCapabilityTrainer_MissingAuthorityFailsBeforeAnyMutation()
    {
        var checkpointRoot = Path.Combine(
            Path.GetTempPath(),
            $"default-capability-missing-authority-{Guid.NewGuid():N}");
        var flows = new InMemoryLogicalFlowRegistry();
        var units = new InMemoryLogicalUnitRegistry();

        try
        {
            var result = await new DefaultLogicalCapabilityTrainer(checkpointRoot)
                .TrainAndRegisterAsync(BuildFastTrainingRequest(
                    "missing-authority",
                    "Fix a runtime issue.",
                    BuildTopologyFor44(),
                    flows,
                    units));

            Assert.False(result.IsSuccess);
            Assert.Equal(
                DefaultLogicalCapabilityTrainer.AdmissionAuthorityRequiredErrorCode,
                result.ErrorCode);
            Assert.False(Directory.Exists(checkpointRoot));
            Assert.Empty(flows.ListDescriptors());
            Assert.Empty(units.ListDescriptors());
        }
        finally
        {
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task DefaultLogicalCapabilityTrainer_MissingWindowsIdentityCustodyFailsBeforeAdmissionOrMutation()
    {
        var checkpointRoot = Path.Combine(
            Path.GetTempPath(),
            $"default-capability-no-windows-custody-{Guid.NewGuid():N}");
        var flows = new InMemoryLogicalFlowRegistry();
        var units = new InMemoryLogicalUnitRegistry();
        var authority =
            new RecordingLogicalCapabilityTrainingAdmissionAuthority();
        var priorAvailability = DefaultLogicalCapabilityTrainer
            .WindowsIdentityCustodyAvailabilityForTesting;
        try
        {
            DefaultLogicalCapabilityTrainer
                .WindowsIdentityCustodyAvailabilityForTesting =
                static () => false;

            var result = await new DefaultLogicalCapabilityTrainer(
                    checkpointRoot,
                    admissionAuthority: authority)
                .TrainAndRegisterAsync(BuildFastTrainingRequest(
                    "no-windows-custody",
                    "Fix a runtime issue.",
                    BuildTopologyFor44(),
                    flows,
                    units));

            Assert.False(result.IsSuccess);
            Assert.Equal(
                DefaultLogicalCapabilityTrainer
                    .WindowsIdentityCustodyRequiredErrorCode,
                result.ErrorCode);
            Assert.Equal(0, authority.LeaseCount);
            Assert.Equal(0, authority.PermitCount);
            Assert.False(Directory.Exists(checkpointRoot));
            Assert.Empty(flows.ListDescriptors());
            Assert.Empty(units.ListDescriptors());
        }
        finally
        {
            DefaultLogicalCapabilityTrainer
                .WindowsIdentityCustodyAvailabilityForTesting =
                priorAvailability;
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public void LogicalCapabilityFileCustody_SymbolicLinkLeafIsNotOpenedAsOwnedFile()
    {
        if (!OperatingSystem.IsLinux() ||
            !LogicalCapabilityFileCustody.IsCurrentPlatformSupported)
            return;

        var root = Path.Combine(
            Path.GetTempPath(),
            $"logical-custody-symlink-leaf-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        var target = Path.Combine(root, "foreign-target.txt");
        var link = Path.Combine(root, "owned-link.txt");
        try
        {
            File.WriteAllText(target, "foreign");
            File.CreateSymbolicLink(link, target);

            using var opened = LogicalCapabilityFileCustody.OpenIdentityStream(
                link,
                LogicalCapabilityFileCustody.GenericReadAccess,
                FileShare.Read);

            // Linux не принимает ссылку как owned-файл: иначе тренер мог бы
            // читать или удалить чужой файл за пределами захваченной личности.
            Assert.Null(opened);
            Assert.Equal("foreign", File.ReadAllText(target));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void LogicalCapabilityFileCustody_HardLinkedLeafIsRejectedByIdentityProof()
    {
        if (!LogicalCapabilityFileCustody.IsCurrentPlatformSupported)
            return;

        var root = Path.Combine(
            Path.GetTempPath(),
            $"logical-custody-hardlink-leaf-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        var owned = Path.Combine(root, "owned.txt");
        var alias = Path.Combine(root, "alias.txt");
        try
        {
            File.WriteAllText(owned, "owned bytes");
            if (!LogicalCapabilityFileCustody.TryCreateHardLinkForTesting(
                    owned,
                    alias))
                return;

            using var opened = LogicalCapabilityFileCustody.OpenIdentityStream(
                owned,
                LogicalCapabilityFileCustody.GenericReadAccess,
                FileShare.Read | FileShare.Delete);
            Assert.NotNull(opened);

            // Две жёсткие ссылки означают, что у файла есть второе имя вне
            // нашей публикационной сделки; такой объект нельзя считать нашим.
            Assert.Throws<IOException>(() =>
                LogicalCapabilityFileCustody.CaptureIdentityProof(
                    opened!,
                    owned,
                    step: 0,
                    DpoTrainerArtifactKind.PolicySerialization));
            Assert.True(File.Exists(alias));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void LogicalCapabilityFileCustody_RollbackDoesNotDeleteForeignReplacement()
    {
        if (!OperatingSystem.IsLinux() ||
            !LogicalCapabilityFileCustody.IsCurrentPlatformSupported)
            return;

        var root = Path.Combine(
            Path.GetTempPath(),
            $"logical-custody-foreign-rollback-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        var owned = Path.Combine(root, "owned.txt");
        var movedOwned = Path.Combine(root, "owned-retained.txt");
        try
        {
            using var opened = LogicalCapabilityFileCustody.OpenIdentityStream(
                owned,
                LogicalCapabilityFileCustody.GenericReadAccess |
                LogicalCapabilityFileCustody.GenericWriteAccess |
                LogicalCapabilityFileCustody.DeleteAccess,
                FileShare.ReadWrite | FileShare.Delete,
                LogicalCapabilityFileCustody.CreateNewDisposition);
            Assert.NotNull(opened);
            opened!.Write(Encoding.UTF8.GetBytes("owned bytes"));
            opened.Flush(flushToDisk: true);
            _ = LogicalCapabilityFileCustody.CaptureIdentityProof(
                opened,
                owned,
                step: 0,
                DpoTrainerArtifactKind.PolicySerialization);

            File.Move(owned, movedOwned);
            File.WriteAllText(owned, "foreign replacement");

            // Откат сверяет открытый inode с текущим именем. Если имя уже
            // указывает на чужой файл, удаление останавливается fail-closed.
            Assert.Throws<IOException>(() =>
                LogicalCapabilityFileCustody.MarkIdentityForDeletion(
                    opened,
                    owned));
            Assert.Equal("foreign replacement", File.ReadAllText(owned));
            Assert.Equal("owned bytes", File.ReadAllText(movedOwned));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void LogicalCapabilityFileCustody_SavePayloadRejectsSymlinkAncestorBeforeCreate()
    {
        if (!OperatingSystem.IsLinux() ||
            !LogicalCapabilityFileCustody.IsCurrentPlatformSupported)
            return;

        var root = Path.Combine(
            Path.GetTempPath(),
            $"logical-custody-symlink-ancestor-{Guid.NewGuid():N}");
        var foreignTarget = Path.Combine(
            Path.GetTempPath(),
            $"logical-custody-symlink-foreign-{Guid.NewGuid():N}");
        var symlinkAncestor = Path.Combine(root, "link-to-foreign");
        var finalFileName =
            TeachingRegistryPersistence.GeneratedCapabilityManifestFileName;
        var finalPath = Path.Combine(
            symlinkAncestor,
            "capability",
            finalFileName);
        try
        {
            Directory.CreateDirectory(root);
            Directory.CreateDirectory(foreignTarget);
            Directory.CreateSymbolicLink(symlinkAncestor, foreignTarget);

            var result = LogicalCapabilityFileCustody
                .SavePayloadNoOverwriteForTesting(
                    finalPath,
                    ".generated_capability_manifest.tmp",
                    finalFileName,
                    Encoding.UTF8.GetBytes("{}"),
                    "CUSTODY_UNAVAILABLE",
                    "ALREADY_EXISTS",
                    "WRITE_FAILED",
                    beforeLinuxDirectorySegmentOpenForTesting: null,
                    afterLinuxDirectorySegmentOpenForTesting: null);

            // Descriptor-walk должен остановиться на symlink-предке до mkdirat:
            // иначе manifest мог бы появиться внутри чужого каталога.
            Assert.False(result.IsSuccess);
            Assert.False(Directory.Exists(Path.Combine(
                foreignTarget,
                "capability")));
            Assert.False(File.Exists(Path.Combine(
                foreignTarget,
                "capability",
                finalFileName)));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
            if (Directory.Exists(foreignTarget))
                Directory.Delete(foreignTarget, recursive: true);
        }
    }

    [Fact]
    public void LogicalCapabilityFileCustody_SavePayloadFailsClosedWhenAncestorIsReplacedAfterDescriptorProof()
    {
        if (!OperatingSystem.IsLinux() ||
            !LogicalCapabilityFileCustody.IsCurrentPlatformSupported)
            return;

        var root = Path.Combine(
            Path.GetTempPath(),
            $"logical-custody-ancestor-replacement-{Guid.NewGuid():N}");
        var ownedAncestor = Path.Combine(root, "owned-ancestor");
        var movedOwnedAncestor = Path.Combine(root, "owned-ancestor-retained");
        var foreignTarget = Path.Combine(root, "foreign-target");
        var finalFileName =
            TeachingRegistryPersistence.GeneratedCapabilityManifestFileName;
        var finalPath = Path.Combine(
            ownedAncestor,
            "capability",
            finalFileName);
        var hookFired = false;
        try
        {
            Directory.CreateDirectory(ownedAncestor);
            Directory.CreateDirectory(foreignTarget);

            var result = LogicalCapabilityFileCustody
                .SavePayloadNoOverwriteForTesting(
                    finalPath,
                    ".generated_capability_manifest.tmp",
                    finalFileName,
                    Encoding.UTF8.GetBytes("{\"owned\":true}"),
                    "CUSTODY_UNAVAILABLE",
                    "ALREADY_EXISTS",
                    "WRITE_FAILED",
                    beforeLinuxDirectorySegmentOpenForTesting: null,
                    afterLinuxDirectorySegmentOpenForTesting:
                    (_, _, openedPath) =>
                    {
                        if (hookFired ||
                            !StringComparer.Ordinal.Equals(
                                openedPath,
                                ownedAncestor))
                        {
                            return;
                        }

                        hookFired = true;
                        Directory.Move(ownedAncestor, movedOwnedAncestor);
                        Directory.CreateSymbolicLink(
                            ownedAncestor,
                            foreignTarget);
                    });

            // После proof у нас есть удержанный fd старого ancestor. Если имя
            // ancestor заменили symlink'ом, publish не должен уйти в новый путь.
            Assert.True(hookFired);
            Assert.False(result.IsSuccess);
            Assert.False(Directory.Exists(Path.Combine(
                foreignTarget,
                "capability")));
            Assert.False(File.Exists(Path.Combine(
                foreignTarget,
                "capability",
                finalFileName)));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void LogicalCapabilityFileCustody_SavePayloadClosesSegmentHandleWhenFinalDirectoryTrustFails()
    {
        if (!OperatingSystem.IsLinux() ||
            !LogicalCapabilityFileCustody.IsCurrentPlatformSupported)
            return;

        var root = Path.Combine(
            Path.GetTempPath(),
            $"logical-custody-fd-leak-{Guid.NewGuid():N}");
        var finalDirectory = Path.Combine(root, "unsafe-final");
        var finalFileName =
            TeachingRegistryPersistence.GeneratedCapabilityManifestFileName;
        var finalPath = Path.Combine(finalDirectory, finalFileName);
        try
        {
            Directory.CreateDirectory(finalDirectory);
            File.SetUnixFileMode(
                finalDirectory,
                UnixFileMode.UserRead |
                UnixFileMode.UserWrite |
                UnixFileMode.UserExecute |
                UnixFileMode.GroupWrite |
                UnixFileMode.GroupExecute);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var before = CountOpenFileDescriptorsForTesting();
            for (var attempt = 0; attempt < 64; attempt++)
            {
                var result = LogicalCapabilityFileCustody
                    .SavePayloadNoOverwriteForTesting(
                        finalPath,
                        $".generated_capability_manifest.{attempt}.tmp",
                        finalFileName,
                        Encoding.UTF8.GetBytes("{}"),
                        "CUSTODY_UNAVAILABLE",
                        "ALREADY_EXISTS",
                        "WRITE_FAILED",
                        beforeLinuxDirectorySegmentOpenForTesting: null,
                        afterLinuxDirectorySegmentOpenForTesting: null);

                Assert.False(result.IsSuccess);
            }

            var after = CountOpenFileDescriptorsForTesting();

            // Этот тест бьёт ровно ownership-transfer: если child fd проверяется
            // до добавления в chain и не закрывается в локальном finally, 64
            // fail-closed попытки оставляют заметную лестницу descriptor'ов.
            Assert.True(
                after <= before + 4,
                $"fd leak suspected: before={before}, after={after}");
        }
        finally
        {
            try
            {
                if (Directory.Exists(finalDirectory))
                {
                    File.SetUnixFileMode(
                        finalDirectory,
                        UnixFileMode.UserRead |
                        UnixFileMode.UserWrite |
                        UnixFileMode.UserExecute);
                }
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
            }

            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void LogicalCapabilityFileCustody_UnlinkHardLinkRaceFailsAndPreservesAlias()
    {
        if (!OperatingSystem.IsLinux() ||
            !LogicalCapabilityFileCustody.IsCurrentPlatformSupported)
            return;

        var root = Path.Combine(
            Path.GetTempPath(),
            $"logical-custody-unlink-hardlink-race-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        var owned = Path.Combine(root, "owned.txt");
        var alias = Path.Combine(root, "alias.txt");
        try
        {
            using var opened = LogicalCapabilityFileCustody.OpenIdentityStream(
                owned,
                LogicalCapabilityFileCustody.GenericReadAccess |
                LogicalCapabilityFileCustody.GenericWriteAccess |
                LogicalCapabilityFileCustody.DeleteAccess,
                FileShare.ReadWrite | FileShare.Delete,
                LogicalCapabilityFileCustody.CreateNewDisposition);
            Assert.NotNull(opened);
            opened!.Write(Encoding.UTF8.GetBytes("owned bytes"));
            opened.Flush(flushToDisk: true);
            _ = LogicalCapabilityFileCustody.CaptureIdentityProof(
                opened,
                owned,
                step: 0,
                DpoTrainerArtifactKind.PolicySerialization);
            var aliasCreated = false;

            var failure = Assert.Throws<IOException>(() =>
                LogicalCapabilityFileCustody.MarkIdentityForDeletionForTesting(
                    opened,
                    owned,
                    () => aliasCreated = LogicalCapabilityFileCustody
                        .TryCreateHardLinkForTesting(owned, alias)));

            // Если hardlink появился между precheck и unlinkat, исходное имя уже
            // можно убрать, но успехом это не считаем: второй алиас доказывает,
            // что физический файл больше не находится целиком под нашей опекой.
            Assert.True(aliasCreated);
            Assert.Contains("LINKED", failure.Message);
            Assert.False(File.Exists(owned));
            Assert.True(File.Exists(alias));
            Assert.Equal("owned bytes", File.ReadAllText(alias));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void TeachingRegistryPersistence_SaveGeneratedManifestPublishesAndDoesNotOverwrite()
    {
        if (!LogicalCapabilityFileCustody.IsCurrentPlatformSupported)
            return;

        var root = Path.Combine(
            Path.GetTempPath(),
            $"logical-custody-manifest-save-{Guid.NewGuid():N}");
        var manifestDirectory = Path.Combine(root, "run", "capability");
        var manifestPath = Path.Combine(
            manifestDirectory,
            TeachingRegistryPersistence.GeneratedCapabilityManifestFileName);
        try
        {
            var manifest = BuildMinimalGeneratedCapabilityManifest(
                manifestDirectory,
                "generated-capability-unit-custody-save");
            var saved = TeachingRegistryPersistence.SaveGeneratedCapabilityManifest(
                manifest,
                manifestPath);

            Assert.True(saved.IsSuccess, saved.ErrorMessage);
            Assert.True(File.Exists(manifestPath));

            var inventory =
                TeachingRegistryPersistence.ReadGeneratedCapabilityManifestInventory(root);
            Assert.True(inventory.IsSuccess, inventory.ErrorMessage);
            var source = Assert.Single(inventory.Value!.Sources);
            Assert.Equal(manifest.UnitId, source.Manifest.UnitId);

            var existingBytes = File.ReadAllText(manifestPath);
            var duplicate = TeachingRegistryPersistence.SaveGeneratedCapabilityManifest(
                BuildMinimalGeneratedCapabilityManifest(
                    manifestDirectory,
                    "generated-capability-unit-custody-duplicate"),
                manifestPath);

            // Повторная запись в тот же manifest не подменяет опубликованный
            // файл: новая публикация должна выбрать новый путь или остановиться.
            Assert.False(duplicate.IsSuccess);
            Assert.Equal(
                "GENERATED_CAPABILITY_MANIFEST_ALREADY_EXISTS",
                duplicate.ErrorCode);
            Assert.Equal(existingBytes, File.ReadAllText(manifestPath));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task DefaultLogicalCapabilityTrainer_UsesOneLeaseForExactCanonicalBatchAndAllMutations()
    {
        var checkpointRoot = Path.Combine(
            Path.GetTempPath(),
            $"default-capability-single-lease-{Guid.NewGuid():N}");
        var authority = new RecordingLogicalCapabilityTrainingAdmissionAuthority();
        var boundaries = new List<string>();
        var priorObserver = DefaultLogicalCapabilityTrainer.MutationBoundaryObserverForTesting;
        var priorRequestObserver = DefaultLogicalCapabilityTrainer
            .DpoRequestObserverForTesting;
        var priorSinkObserver = LogicalDpoTrainingUnit
            .OptimizerMutationSinkObserverForTesting;
        LogicalDpoTrainingRequest? observedRequest = null;
        IReadOnlyList<LogicalDpoPreparedOptimizerInput>? observedInputs = null;
        IReadOnlyList<DpoVectorBatch>? observedBatches = null;

        try
        {
            DefaultLogicalCapabilityTrainer.MutationBoundaryObserverForTesting =
                boundaries.Add;
            DefaultLogicalCapabilityTrainer.DpoRequestObserverForTesting =
                request => observedRequest = request;
            LogicalDpoTrainingUnit.OptimizerMutationSinkObserverForTesting =
                (inputs, batches) =>
                {
                    observedInputs = inputs;
                    observedBatches = batches;
                };
            var result = await new DefaultLogicalCapabilityTrainer(
                    checkpointRoot,
                    admissionAuthority: authority)
                .TrainAndRegisterAsync(BuildFastTrainingRequest(
                    "single-admitted-lease",
                    "Fix a runtime issue.",
                    BuildTopologyFor44(),
                    new InMemoryLogicalFlowRegistry(),
                    new InMemoryLogicalUnitRegistry()));

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.True(result.Value!.CapabilityRegistered);
            Assert.Equal(1, authority.LeaseCount);
            Assert.Equal(1, authority.BundleCount);
            Assert.Equal(1, authority.LeaseDisposeCount);
            Assert.Equal(8, authority.PermitCount);
            Assert.NotNull(authority.LastBundle);
            Assert.NotNull(authority.LastBatch);
            Assert.NotEmpty(authority.LastBatch!.Rows);
            Assert.All(authority.LastBatch.Rows, static row => Assert.True(row.IsCanonical));
            Assert.Equal(
                authority.LastBatch.Rows.Count,
                authority.LastBatch.Rows.Select(static row => row.Sha256)
                    .Distinct(StringComparer.Ordinal)
                    .Count());
            Assert.Equal([
                "capability_training:destination_reservation",
                "capability_training:build_stack",
                "capability_training:post_dpo_result",
                "capability_training:final_publication"
            ], boundaries);
            Assert.Same(
                authority.LastBundle,
                observedRequest?.AdmissionCapabilities);
            Assert.NotNull(observedRequest?.PreparedExecution);
            var preparedInput = Assert.Single(
                observedRequest!.PreparedExecution!.OptimizerInputs);
            Assert.Same(preparedInput, Assert.Single(observedInputs!));
            Assert.Same(preparedInput.Batch, Assert.Single(observedBatches!));
            var admission = Assert.IsType<DpoTrainerPairAdmission>(
                preparedInput.Batch.Batch.Admission);
            var preference = Assert.Single(observedRequest.Preferences);
            Assert.Equal(DpoPairSideAuthorKind.DeterministicGenerator,
                preference.ChosenAuthorKind);
            Assert.Equal(DpoPairSideAuthorKind.DeterministicGenerator,
                preference.RejectedAuthorKind);
            var expectedAdmission = DpoTrainerPairCustoms.CreateAdmitted(
                preference.Prompt,
                preference.Chosen,
                preference.Rejected,
                DpoPairSideAuthorKind.DeterministicGenerator,
                DpoPairSideAuthorKind.DeterministicGenerator,
                nameof(LogicalDpoTrainingUnit)).Admission;
            Assert.Equal(expectedAdmission.ContentFingerprintSha256,
                admission.ContentFingerprintSha256);
            Assert.Equal(nameof(LogicalDpoTrainingUnit),
                admission.IssuerName);
        }
        finally
        {
            DefaultLogicalCapabilityTrainer.MutationBoundaryObserverForTesting =
                priorObserver;
            DefaultLogicalCapabilityTrainer.DpoRequestObserverForTesting =
                priorRequestObserver;
            LogicalDpoTrainingUnit.OptimizerMutationSinkObserverForTesting =
                priorSinkObserver;
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task DefaultLogicalCapabilityTrainer_AdmissionBatchExactlyMatchesIndependentlyReconstructedCurrentAndReplayRows()
    {
        var checkpointRoot = Path.Combine(
            Path.GetTempPath(),
            $"default-capability-exact-admission-{Guid.NewGuid():N}");
        var priorRequestObserver = DefaultLogicalCapabilityTrainer
            .DpoRequestObserverForTesting;
        var priorSinkObserver = LogicalDpoTrainingUnit
            .OptimizerMutationSinkObserverForTesting;
        LogicalDpoTrainingRequest? observedRequest = null;
        IReadOnlyList<DpoVectorBatch>? observedBatches = null;
        try
        {
            var firstTopology = BuildTopologyFor44();
            var firstFlows = new InMemoryLogicalFlowRegistry();
            var firstUnits = new InMemoryLogicalUnitRegistry();
            var first = await new DefaultLogicalCapabilityTrainer(
                    checkpointRoot,
                    admissionAuthority:
                    new RecordingLogicalCapabilityTrainingAdmissionAuthority())
                .TrainAndRegisterAsync(BuildFastTrainingRequest(
                    "exact-admission-replay-source",
                    "Fix the original runtime issue.",
                    firstTopology,
                    firstFlows,
                    firstUnits));
            Assert.True(first.IsSuccess, first.ErrorMessage);

            var snapshot = TeachingRegistryPersistence.Capture(
                "exact-admission-fixture",
                1,
                firstUnits,
                firstFlows,
                checkpointRoot);
            Assert.True(snapshot.IsSuccess, snapshot.ErrorMessage);
            var replayManifest = Assert.Single(
                snapshot.Value!.GeneratedCapabilities);
            Assert.True(File.Exists(replayManifest.TokenizerPath));
            Assert.True(File.Exists(replayManifest.PolicyCheckpointPath));

            const string runId = "exact-admission-current";
            const string userRequest = "Fix another runtime issue.";
            var authority =
                new RecordingLogicalCapabilityTrainingAdmissionAuthority();
            var topology = BuildTopologyFor44();
            DefaultLogicalCapabilityTrainer.DpoRequestObserverForTesting =
                request => observedRequest = request;
            LogicalDpoTrainingUnit.OptimizerMutationSinkObserverForTesting =
                (_, batches) => observedBatches = batches;
            var second = await new DefaultLogicalCapabilityTrainer(
                    checkpointRoot,
                    admissionAuthority: authority)
                .TrainAndRegisterAsync(BuildFastTrainingRequest(
                    runId,
                    userRequest,
                    topology,
                    new InMemoryLogicalFlowRegistry(),
                    new InMemoryLogicalUnitRegistry()));
            Assert.True(second.IsSuccess, second.ErrorMessage);
            Assert.NotNull(authority.LastBatch);
            Assert.NotNull(observedRequest?.PreparedExecution);
            Assert.NotNull(observedBatches);
            Assert.Equal(
                authority.LastBatch!.Rows.Count,
                observedRequest!.Preferences.Length);
            Assert.All(observedRequest.Preferences, static preference =>
            {
                Assert.Equal(
                    DpoPairSideAuthorKind.DeterministicGenerator,
                    preference.ChosenAuthorKind);
                Assert.Equal(
                    DpoPairSideAuthorKind.DeterministicGenerator,
                    preference.RejectedAuthorKind);
            });
            Assert.Equal(
                observedRequest.Preferences.Select(static preference =>
                    DpoTrainerPairCustoms.CreateAdmitted(
                        preference.Prompt,
                        preference.Chosen,
                        preference.Rejected,
                        DpoPairSideAuthorKind.DeterministicGenerator,
                        DpoPairSideAuthorKind.DeterministicGenerator,
                        nameof(LogicalDpoTrainingUnit))
                    .Admission
                    .ContentFingerprintSha256),
                observedBatches!.Select(static batch =>
                    Assert.IsType<DpoTrainerPairAdmission>(
                            batch.Batch.Admission)
                        .ContentFingerprintSha256));

            var compiled = new SemanticTeachingPromptCompiler().Compile(
                userRequest);
            Assert.True(compiled.IsSuccess, compiled.ErrorMessage);
            var subPhase = topology.ResolveSubPhase("4.4");
            Assert.True(subPhase.IsSuccess, subPhase.ErrorMessage);
            var ownedSubPhase = subPhase.Value!.SubPhase;
            var promptSide = compiled.Value!.CanonicalText.Trim();
            var raw = string.Join(" ",
                "semantic_prompt:",
                promptSide,
                "branch_result:",
                "knowledge_edge: missing generated capability",
                "sub_phase:",
                ownedSubPhase.SubPhaseId,
                ownedSubPhase.Name,
                ownedSubPhase.Purpose,
                string.Join(" ", ownedSubPhase.PredicateIds));
            var vector = compiled.Value.TeachingVector.ToArray();
            var vectorText = string.Join(",", vector.Select(value =>
                value.ToString(
                    "0.###",
                    System.Globalization.CultureInfo.InvariantCulture)));
            var current = new LogicalDpoPreferenceExample(
                $"{raw} semantic_compressed: {promptSide} teaching_vector_60: {vectorText}".Trim(),
                "Generated capability for sub-phase 4.4: Refactoring and Code Quality. Execute the requested work through task type refactoring-and-code-quality.",
                "Do not generate a capability for sub-phase 4.4.",
                vector)
            {
                ChosenAuthorKind =
                    DpoPairSideAuthorKind.DeterministicGenerator,
                RejectedAuthorKind =
                    DpoPairSideAuthorKind.DeterministicGenerator
            };
            var replay = new LogicalDpoPreferenceExample(
                replayManifest.TrainingText,
                replayManifest.OutputText,
                $"Do not generate a capability for sub-phase {replayManifest.SubPhaseId}.",
                TrainingVectorBuilder.Build(replayManifest.TrainingText))
            {
                ChosenAuthorKind =
                    DpoPairSideAuthorKind.DeterministicGenerator,
                RejectedAuthorKind =
                    DpoPairSideAuthorKind.DeterministicGenerator
            };

            const int manifestCount = 1;
            const int currentBranchCount = 1;
            const int distinctClassCount = 1;
            var rareRowCount = manifestCount + 1;
            var currentRepeatCount = Math.Clamp(
                (int)Math.Ceiling(
                    (manifestCount + currentBranchCount) /
                    (double)(distinctClassCount * rareRowCount)),
                1,
                3);
            currentRepeatCount = Math.Max(currentRepeatCount, 2);
            var replayCount = Math.Min(
                manifestCount,
                Math.Max(
                    1,
                    (int)Math.Round(
                        currentRepeatCount * 0.30 / (1.0 - 0.30))));
            var expectedRows = new List<LogicalDpoPreferenceExample>();
            for (var index = 0; index < currentRepeatCount; index++)
            {
                expectedRows.Add(current with
                {
                    TeachingVector = current.TeachingVector!.ToArray()
                });
            }
            if (replayCount == 1)
                expectedRows.Add(replay);
            var random = new Random(42);
            for (var index = expectedRows.Count - 1; index > 0; index--)
            {
                var swapIndex = random.Next(index + 1);
                (expectedRows[index], expectedRows[swapIndex]) =
                    (expectedRows[swapIndex], expectedRows[index]);
            }

            var domain = PromptDomainClassifier.Classify(userRequest)
                .Domain.ToString();
            var expectedDigests = expectedRows
                .Select((row, ordinal) => ComputeAdmissionDigest(
                    runId,
                    domain,
                    ordinal,
                    row))
                .ToArray();
            Assert.Equal(
                expectedDigests,
                authority.LastBatch!.Rows
                    .Select(static row => row.Sha256)
                    .ToArray());
        }
        finally
        {
            DefaultLogicalCapabilityTrainer.DpoRequestObserverForTesting =
                priorRequestObserver;
            LogicalDpoTrainingUnit.OptimizerMutationSinkObserverForTesting =
                priorSinkObserver;
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task DefaultLogicalCapabilityTrainer_RevocationAfterDpoCleansOwnedArtifactsBeforePublication()
    {
        var checkpointRoot = Path.Combine(
            Path.GetTempPath(),
            $"default-capability-revoked-after-dpo-{Guid.NewGuid():N}");
        var flows = new InMemoryLogicalFlowRegistry();
        var units = new InMemoryLogicalUnitRegistry();
        var authority = new RecordingLogicalCapabilityTrainingAdmissionAuthority(
            ordinal => ordinal == 6
                ? AgentInfrastructure.OperationResult<IDisposable>.Fail(
                    "The legal teacher training admission was revoked.",
                    "LEGAL_TRAINING_ADMISSION_REVOKED")
                : AgentInfrastructure.OperationResult<IDisposable>.Ok(
                    new CancellationTokenSource()));

        try
        {
            var result = await new DefaultLogicalCapabilityTrainer(
                    checkpointRoot,
                    admissionAuthority: authority)
                .TrainAndRegisterAsync(BuildFastTrainingRequest(
                    "revoked-after-dpo",
                    "Fix a runtime issue.",
                    BuildTopologyFor44(),
                    flows,
                    units));

            Assert.False(result.IsSuccess);
            Assert.Equal("LEGAL_TRAINING_ADMISSION_REVOKED", result.ErrorCode);
            Assert.Equal(1, authority.LeaseCount);
            Assert.Equal(1, authority.LeaseDisposeCount);
            Assert.Equal(6, authority.PermitCount);
            Assert.False(Directory.Exists(ExpectedPublicationDirectory(
                checkpointRoot,
                "revoked-after-dpo",
                Domain.BUG.ToString(),
                "4.4")));
            Assert.Empty(flows.ListDescriptors());
            Assert.Empty(units.ListDescriptors());
        }
        finally
        {
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task DefaultLogicalCapabilityTrainer_FinalPublicationPermitDrainsRevocationBeforeSuccessEscapes()
    {
        var checkpointRoot = Path.Combine(
            Path.GetTempPath(),
            $"default-capability-final-drain-{Guid.NewGuid():N}");
        var authority = CreateRevocableCapabilityAuthority();
        Task? revocation = null;
        var priorObserver =
            DefaultLogicalCapabilityTrainer.FinalPublicationObserverForTesting;

        try
        {
            DefaultLogicalCapabilityTrainer.FinalPublicationObserverForTesting =
                () =>
                {
                    revocation = authority.BeginRevocation();
                    Assert.False(revocation.IsCompleted);
                };

            var result = await new DefaultLogicalCapabilityTrainer(
                    checkpointRoot,
                    admissionAuthority: authority)
                .TrainAndRegisterAsync(BuildFastTrainingRequest(
                    "final-permit-drain",
                    "Fix a runtime issue.",
                    BuildTopologyFor44(),
                    new InMemoryLogicalFlowRegistry(),
                    new InMemoryLogicalUnitRegistry()));

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.NotNull(revocation);
            await revocation.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.True(revocation.IsCompletedSuccessfully);
            Assert.True(Directory.Exists(ExpectedPublicationDirectory(
                checkpointRoot,
                "final-permit-drain",
                Domain.BUG.ToString(),
                "4.4")));
        }
        finally
        {
            DefaultLogicalCapabilityTrainer.FinalPublicationObserverForTesting =
                priorObserver;
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task DefaultLogicalCapabilityTrainer_RootWaitDoesNotHoldFinalPermitOrDelayRevocation()
    {
        var checkpointRoot = Path.Combine(
            Path.GetTempPath(),
            $"default-capability-root-wait-{Guid.NewGuid():N}");
        var authority = CreateRevocableCapabilityAuthority();
        using var mutexAcquired = new ManualResetEventSlim();
        using var releaseMutex = new ManualResetEventSlim();
        var waiting = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var priorObserver =
            DefaultLogicalCapabilityTrainer.RootLeaseWaitObserverForTesting;
        var mutexName = DefaultLogicalCapabilityTrainer
            .BuildRootPublicationMutexNameForTesting(checkpointRoot);
        var holder = new Thread(() =>
        {
            using var mutex = new Mutex(false, mutexName);
            mutex.WaitOne();
            mutexAcquired.Set();
            releaseMutex.Wait();
            mutex.ReleaseMutex();
        }) { IsBackground = true };

        try
        {
            holder.Start();
            Assert.True(mutexAcquired.Wait(TimeSpan.FromSeconds(10)));
            Task? revocation = null;
            DefaultLogicalCapabilityTrainer.RootLeaseWaitObserverForTesting =
                () =>
                {
                    revocation = authority.BeginRevocation();
                    _ = revocation.ContinueWith(
                        _ => releaseMutex.Set(),
                        CancellationToken.None,
                        TaskContinuationOptions.ExecuteSynchronously,
                        TaskScheduler.Default);
                    waiting.TrySetResult(true);
                };
            var training = new DefaultLogicalCapabilityTrainer(
                    checkpointRoot,
                    admissionAuthority: authority)
                .TrainAndRegisterAsync(BuildFastTrainingRequest(
                    "root-wait-revocation",
                    "Fix a runtime issue.",
                    BuildTopologyFor44(),
                    new InMemoryLogicalFlowRegistry(),
                    new InMemoryLogicalUnitRegistry()));

            await waiting.Task.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.NotNull(revocation);
            await revocation.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.True(revocation.IsCompletedSuccessfully);

            releaseMutex.Set();
            var result = await training.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.False(result.IsSuccess);
            Assert.Equal("LEGAL_TRAINING_ADMISSION_REVOKED", result.ErrorCode);
        }
        finally
        {
            DefaultLogicalCapabilityTrainer.RootLeaseWaitObserverForTesting =
                priorObserver;
            releaseMutex.Set();
            holder.Join(TimeSpan.FromSeconds(10));
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task DefaultLogicalCapabilityTrainer_AbandonedRootLeaseFailsClosed()
    {
        var checkpointRoot = Path.Combine(
            Path.GetTempPath(),
            $"default-capability-abandoned-root-{Guid.NewGuid():N}");
        var mutexName = DefaultLogicalCapabilityTrainer
            .BuildRootPublicationMutexNameForTesting(checkpointRoot);
        Mutex? abandonedHandle = null;
        var abandoned = 0;
        var priorObserver =
            DefaultLogicalCapabilityTrainer.AbandonedRootLeaseObserverForTesting;
        var holder = new Thread(() =>
        {
            abandonedHandle = new Mutex(false, mutexName);
            abandonedHandle.WaitOne();
        }) { IsBackground = true };

        try
        {
            holder.Start();
            Assert.True(holder.Join(TimeSpan.FromSeconds(10)));
            DefaultLogicalCapabilityTrainer.AbandonedRootLeaseObserverForTesting =
                () => Interlocked.Increment(ref abandoned);

            var result = await new DefaultLogicalCapabilityTrainer(
                    checkpointRoot,
                    admissionAuthority:
                    new RecordingLogicalCapabilityTrainingAdmissionAuthority())
                .TrainAndRegisterAsync(BuildFastTrainingRequest(
                    "abandoned-root",
                    "Fix a runtime issue.",
                    BuildTopologyFor44(),
                    new InMemoryLogicalFlowRegistry(),
                    new InMemoryLogicalUnitRegistry()));

            Assert.False(result.IsSuccess);
            Assert.Equal(
                "GENERATED_CAPABILITY_PUBLICATION_LEASE_ABANDONED",
                result.ErrorCode);
            Assert.Equal(1, abandoned);
            AssertNoGeneratedStagingResidue(checkpointRoot);
        }
        finally
        {
            DefaultLogicalCapabilityTrainer.AbandonedRootLeaseObserverForTesting =
                priorObserver;
            abandonedHandle?.Dispose();
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task DefaultLogicalCapabilityTrainer_SecondDpoStepRevocationCleansAcceptedStepOwnership()
    {
        var checkpointRoot = Path.Combine(
            Path.GetTempPath(),
            $"default-capability-step2-revoked-{Guid.NewGuid():N}");
        var authority = new RecordingLogicalCapabilityTrainingAdmissionAuthority(
            ordinal => ordinal == 5
                ? AgentInfrastructure.OperationResult<IDisposable>.Fail(
                    "The legal teacher training admission was revoked.",
                    "LEGAL_TRAINING_ADMISSION_REVOKED")
                : AgentInfrastructure.OperationResult<IDisposable>.Ok(
                    new CancellationTokenSource()));

        try
        {
            var result = await new DefaultLogicalCapabilityTrainer(
                    checkpointRoot,
                    admissionAuthority: authority)
                .TrainAndRegisterAsync(BuildFastTrainingRequestWithDpoSteps(
                    "step2-revoked",
                    "Fix a runtime issue.",
                    BuildTopologyFor44(),
                    new InMemoryLogicalFlowRegistry(),
                    new InMemoryLogicalUnitRegistry(),
                    2));

            Assert.False(result.IsSuccess);
            Assert.Equal("LEGAL_TRAINING_ADMISSION_REVOKED", result.ErrorCode);
            Assert.Equal(5, authority.PermitCount);
            AssertNoGeneratedStagingResidue(checkpointRoot);
        }
        finally
        {
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task DefaultLogicalCapabilityTrainer_ReferenceFailureCleansAcceptedDpoArtifacts()
    {
        var checkpointRoot = Path.Combine(
            Path.GetTempPath(),
            $"default-capability-reference-failure-{Guid.NewGuid():N}");
        var priorFailure =
            LogicalDpoTrainingUnit.ForceReferenceValidationFailureForTesting;
        try
        {
            LogicalDpoTrainingUnit.ForceReferenceValidationFailureForTesting =
                static () => true;
            var result = await new DefaultLogicalCapabilityTrainer(
                    checkpointRoot,
                    admissionAuthority:
                    new RecordingLogicalCapabilityTrainingAdmissionAuthority())
                .TrainAndRegisterAsync(BuildFastTrainingRequest(
                    "reference-failure",
                    "Fix a runtime issue.",
                    BuildTopologyFor44(),
                    new InMemoryLogicalFlowRegistry(),
                    new InMemoryLogicalUnitRegistry()));

            Assert.False(result.IsSuccess);
            Assert.Equal("DPO_REFERENCE_MUTATED", result.ErrorCode);
            AssertNoGeneratedStagingResidue(checkpointRoot);
        }
        finally
        {
            LogicalDpoTrainingUnit.ForceReferenceValidationFailureForTesting =
                priorFailure;
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task DefaultLogicalCapabilityTrainer_CancellationAfterDpoCleansBeforeFinalPublication()
    {
        var checkpointRoot = Path.Combine(
            Path.GetTempPath(),
            $"default-capability-post-dpo-cancel-{Guid.NewGuid():N}");
        var flows = new InMemoryLogicalFlowRegistry();
        var units = new InMemoryLogicalUnitRegistry();
        var authority = new RecordingLogicalCapabilityTrainingAdmissionAuthority();
        using var cancellation = new CancellationTokenSource();
        var priorObserver = DefaultLogicalCapabilityTrainer.PostDpoObserverForTesting;

        try
        {
            DefaultLogicalCapabilityTrainer.PostDpoObserverForTesting =
                cancellation.Cancel;
            var result = await new DefaultLogicalCapabilityTrainer(
                    checkpointRoot,
                    admissionAuthority: authority)
                .TrainAndRegisterAsync(
                    BuildFastTrainingRequest(
                        "post-dpo-cancel",
                        "Fix a runtime issue.",
                        BuildTopologyFor44(),
                        flows,
                        units),
                    cancellation.Token);

            Assert.False(result.IsSuccess);
            Assert.Equal("CANCELLED", result.ErrorCode);
            Assert.Equal(1, authority.LeaseDisposeCount);
            Assert.False(Directory.Exists(ExpectedPublicationDirectory(
                checkpointRoot,
                "post-dpo-cancel",
                Domain.BUG.ToString(),
                "4.4")));
            Assert.Empty(flows.ListDescriptors());
            Assert.Empty(units.ListDescriptors());
        }
        finally
        {
            DefaultLogicalCapabilityTrainer.PostDpoObserverForTesting =
                priorObserver;
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task DefaultLogicalCapabilityTrainer_DpoCleanupPinsFinalAndPreservesByteIdenticalForeignStaging()
    {
        if (!OperatingSystem.IsWindows())
            return;
        var checkpointRoot = Path.Combine(
            Path.GetTempPath(),
            $"default-capability-dpo-identity-cleanup-{Guid.NewGuid():N}");
        using var cancellation = new CancellationTokenSource();
        string? policyStagingPath = null;
        string? policyFinalPath = null;
        byte[]? byteIdenticalForeign = null;
        var finalReplacementBlocked = false;
        var priorAcceptedObserver = DefaultLogicalCapabilityTrainer
            .DpoArtifactOwnershipAcceptedObserverForTesting;
        var priorPostDpoObserver = DefaultLogicalCapabilityTrainer
            .PostDpoObserverForTesting;

        try
        {
            DefaultLogicalCapabilityTrainer
                .DpoArtifactOwnershipAcceptedObserverForTesting = proof =>
                {
                    if (proof.Kind == DpoTrainerArtifactKind.PolicyFinal)
                    {
                        policyStagingPath = proof.TransitionFromPath;
                        policyFinalPath = proof.Path;
                    }
                };
            DefaultLogicalCapabilityTrainer.PostDpoObserverForTesting = () =>
            {
                Assert.NotNull(policyStagingPath);
                Assert.NotNull(policyFinalPath);
                byteIdenticalForeign = ReadAllBytesSharedDelete(
                    policyFinalPath!);
                File.WriteAllBytes(
                    policyStagingPath!,
                    byteIdenticalForeign);
                try
                {
                    File.Delete(policyFinalPath!);
                    File.WriteAllBytes(
                        policyFinalPath!,
                        byteIdenticalForeign);
                }
                catch (IOException)
                {
                    finalReplacementBlocked = true;
                }
                catch (UnauthorizedAccessException)
                {
                    finalReplacementBlocked = true;
                }
                cancellation.Cancel();
            };

            var result = await new DefaultLogicalCapabilityTrainer(
                    checkpointRoot,
                    admissionAuthority:
                    new RecordingLogicalCapabilityTrainingAdmissionAuthority())
                .TrainAndRegisterAsync(
                    BuildFastTrainingRequest(
                        "dpo-identity-cleanup",
                        "Fix a runtime issue.",
                        BuildTopologyFor44(),
                        new InMemoryLogicalFlowRegistry(),
                        new InMemoryLogicalUnitRegistry()),
                    cancellation.Token);

            Assert.False(result.IsSuccess);
            Assert.Equal("CANCELLED", result.ErrorCode);
            Assert.True(finalReplacementBlocked);
            Assert.NotNull(byteIdenticalForeign);
            Assert.NotNull(policyStagingPath);
            Assert.Equal(
                byteIdenticalForeign,
                File.ReadAllBytes(policyStagingPath!));
            Assert.NotNull(policyFinalPath);
            Assert.False(File.Exists(policyFinalPath));
        }
        finally
        {
            DefaultLogicalCapabilityTrainer
                .DpoArtifactOwnershipAcceptedObserverForTesting =
                priorAcceptedObserver;
            DefaultLogicalCapabilityTrainer.PostDpoObserverForTesting =
                priorPostDpoObserver;
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task Publication_DpoReservationLockSidecarIsAcceptedAndCleanedAsOwnedTransientFile()
    {
        var checkpointRoot = Path.Combine(
            Path.GetTempPath(),
            $"default-capability-dpo-lock-owned-{Guid.NewGuid():N}");
        string? dpoDirectory = null;
        var linuxLockWasPresentBeforeCleanup = false;
        var priorAcceptedObserver = DefaultLogicalCapabilityTrainer
            .DpoArtifactOwnershipAcceptedObserverForTesting;
        var priorPostDpoObserver = DefaultLogicalCapabilityTrainer
            .PostDpoObserverForTesting;

        try
        {
            DefaultLogicalCapabilityTrainer
                .DpoArtifactOwnershipAcceptedObserverForTesting = proof =>
                {
                    if (proof.Kind is DpoTrainerArtifactKind.PolicyFinal or
                        DpoTrainerArtifactKind.MetricsFinal)
                    {
                        dpoDirectory ??= Path.GetDirectoryName(proof.Path);
                    }
                };
            DefaultLogicalCapabilityTrainer.PostDpoObserverForTesting = () =>
            {
                if (!OperatingSystem.IsLinux())
                    return;
                Assert.NotNull(dpoDirectory);
                linuxLockWasPresentBeforeCleanup = File.Exists(Path.Combine(
                    dpoDirectory!,
                    DpoReservationLockFileName(1)));
            };

            var result = await new DefaultLogicalCapabilityTrainer(
                    checkpointRoot,
                    admissionAuthority:
                    new RecordingLogicalCapabilityTrainingAdmissionAuthority())
                .TrainAndRegisterAsync(BuildFastTrainingRequest(
                    "dpo-lock-owned",
                    "Fix a runtime issue.",
                    BuildTopologyFor44(),
                    new InMemoryLogicalFlowRegistry(),
                    new InMemoryLogicalUnitRegistry()));

            Assert.True(result.IsSuccess, result.ErrorMessage);
            if (OperatingSystem.IsLinux())
                Assert.True(linuxLockWasPresentBeforeCleanup);
            AssertNoGeneratedStagingResidue(checkpointRoot);
        }
        finally
        {
            DefaultLogicalCapabilityTrainer
                .DpoArtifactOwnershipAcceptedObserverForTesting =
                priorAcceptedObserver;
            DefaultLogicalCapabilityTrainer.PostDpoObserverForTesting =
                priorPostDpoObserver;
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task Publication_UnexpectedDpoReservationLockSidecarFailsClosedAndIsPreserved()
    {
        var checkpointRoot = Path.Combine(
            Path.GetTempPath(),
            $"default-capability-dpo-lock-foreign-{Guid.NewGuid():N}");
        string? unexpectedLockPath = null;
        var unexpectedLockBytes = Encoding.UTF8.GetBytes("foreign lock");
        var priorAcceptedObserver = DefaultLogicalCapabilityTrainer
            .DpoArtifactOwnershipAcceptedObserverForTesting;

        try
        {
            DefaultLogicalCapabilityTrainer
                .DpoArtifactOwnershipAcceptedObserverForTesting = proof =>
                {
                    if (Interlocked.CompareExchange(
                            ref unexpectedLockPath,
                            Path.Combine(
                                Path.GetDirectoryName(proof.Path)!,
                                DpoReservationLockFileName(999999)),
                            null) is not null)
                    {
                        return;
                    }

                    // Неожиданное имя выглядит как reservation-lock, но не
                    // принадлежит текущей custody-сделке и должно остаться
                    // чужим инвентарём, а не попасть под wildcard-исключение.
                    File.WriteAllBytes(unexpectedLockPath!, unexpectedLockBytes);
                };

            var result = await new DefaultLogicalCapabilityTrainer(
                    checkpointRoot,
                    admissionAuthority:
                    new RecordingLogicalCapabilityTrainingAdmissionAuthority())
                .TrainAndRegisterAsync(BuildFastTrainingRequest(
                    "dpo-lock-foreign",
                    "Fix a runtime issue.",
                    BuildTopologyFor44(),
                    new InMemoryLogicalFlowRegistry(),
                    new InMemoryLogicalUnitRegistry()));

            Assert.False(result.IsSuccess);
            Assert.Equal("DPO_STAGING_ARTIFACT_IDENTITY_INVALID", result.ErrorCode);
            Assert.NotNull(unexpectedLockPath);
            Assert.Equal(unexpectedLockBytes, File.ReadAllBytes(unexpectedLockPath!));
        }
        finally
        {
            DefaultLogicalCapabilityTrainer
                .DpoArtifactOwnershipAcceptedObserverForTesting =
                priorAcceptedObserver;
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task Publication_DpoReservationLockReplacementBeforeCleanupFailsClosedAndPreservesReplacement()
    {
        if (!OperatingSystem.IsLinux())
            return;

        var checkpointRoot = Path.Combine(
            Path.GetTempPath(),
            $"default-capability-dpo-lock-replacement-{Guid.NewGuid():N}");
        string? dpoDirectory = null;
        string? lockPath = null;
        string? movedOwnedLockPath = null;
        var foreignBytes = Encoding.UTF8.GetBytes("foreign replacement lock");
        var priorAcceptedObserver = DefaultLogicalCapabilityTrainer
            .DpoArtifactOwnershipAcceptedObserverForTesting;
        var priorPostDpoObserver = DefaultLogicalCapabilityTrainer
            .PostDpoObserverForTesting;

        try
        {
            DefaultLogicalCapabilityTrainer
                .DpoArtifactOwnershipAcceptedObserverForTesting = proof =>
                {
                    if (proof.Kind is DpoTrainerArtifactKind.PolicyFinal or
                        DpoTrainerArtifactKind.MetricsFinal)
                    {
                        dpoDirectory ??= Path.GetDirectoryName(proof.Path);
                    }
                };
            DefaultLogicalCapabilityTrainer.PostDpoObserverForTesting = () =>
            {
                Assert.NotNull(dpoDirectory);
                lockPath = Path.Combine(
                    dpoDirectory!,
                    DpoReservationLockFileName(1));
                movedOwnedLockPath = lockPath + ".owned-before-replacement";
                Assert.True(File.Exists(lockPath));

                // Между TrackCompletedDpoArtifacts и cleanup имя меняется на
                // чужой файл. Cleanup обязан остановиться и не удалять новые
                // байты только потому, что имя ожидаемое.
                File.Move(lockPath, movedOwnedLockPath, overwrite: false);
                File.WriteAllBytes(lockPath, foreignBytes);
            };

            var result = await new DefaultLogicalCapabilityTrainer(
                    checkpointRoot,
                    admissionAuthority:
                    new RecordingLogicalCapabilityTrainingAdmissionAuthority())
                .TrainAndRegisterAsync(BuildFastTrainingRequest(
                    "dpo-lock-replacement",
                    "Fix a runtime issue.",
                    BuildTopologyFor44(),
                    new InMemoryLogicalFlowRegistry(),
                    new InMemoryLogicalUnitRegistry()));

            Assert.False(result.IsSuccess);
            Assert.Equal(
                "GENERATED_CAPABILITY_TRANSIENT_FOREIGN_CONTENT",
                result.ErrorCode);
            Assert.NotNull(lockPath);
            Assert.NotNull(movedOwnedLockPath);
            Assert.Equal(foreignBytes, File.ReadAllBytes(lockPath!));
            Assert.True(File.Exists(movedOwnedLockPath));
        }
        finally
        {
            DefaultLogicalCapabilityTrainer
                .DpoArtifactOwnershipAcceptedObserverForTesting =
                priorAcceptedObserver;
            DefaultLogicalCapabilityTrainer.PostDpoObserverForTesting =
                priorPostDpoObserver;
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task Publication_PreMarkerForeignRacePreservesSentinel()
    {
        var checkpointRoot = Path.Combine(
            Path.GetTempPath(),
            $"default-capability-pre-marker-race-{Guid.NewGuid():N}");
        string? racedDirectory = null;
        var priorObserver =
            DefaultLogicalCapabilityTrainer.PublicationStageObserverForTesting;
        try
        {
            DefaultLogicalCapabilityTrainer.PublicationStageObserverForTesting =
                (stage, directory) =>
                {
                    if (!StringComparer.Ordinal.Equals(
                            stage,
                            "after_dpo_directory_create_before_marker"))
                        return;
                    racedDirectory = directory;
                    File.WriteAllText(
                        Path.Combine(directory, "foreign-sentinel.txt"),
                        "foreign");
                };

            var result = await new DefaultLogicalCapabilityTrainer(
                    checkpointRoot,
                    admissionAuthority:
                    new RecordingLogicalCapabilityTrainingAdmissionAuthority())
                .TrainAndRegisterAsync(BuildFastTrainingRequest(
                    "pre-marker-race",
                    "Fix a runtime issue.",
                    BuildTopologyFor44(),
                    new InMemoryLogicalFlowRegistry(),
                    new InMemoryLogicalUnitRegistry()));

            Assert.False(result.IsSuccess);
            Assert.Equal("GENERATED_CAPABILITY_DESTINATION_BUSY", result.ErrorCode);
            Assert.NotNull(racedDirectory);
            Assert.Equal("foreign", File.ReadAllText(Path.Combine(
                racedDirectory!,
                "foreign-sentinel.txt")));
        }
        finally
        {
            DefaultLogicalCapabilityTrainer.PublicationStageObserverForTesting =
                priorObserver;
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task Publication_TransientCleanupSurfacesAndPreservesForeignContent()
    {
        var checkpointRoot = Path.Combine(
            Path.GetTempPath(),
            $"default-capability-transient-foreign-{Guid.NewGuid():N}");
        string? dpoDirectory = null;
        var flows = new InMemoryLogicalFlowRegistry();
        var units = new InMemoryLogicalUnitRegistry();
        var priorPublicationObserver =
            DefaultLogicalCapabilityTrainer.PublicationStageObserverForTesting;
        var priorRegistryObserver =
            DefaultLogicalCapabilityTrainer.RegistryPublicationObserverForTesting;
        try
        {
            DefaultLogicalCapabilityTrainer.PublicationStageObserverForTesting =
                (stage, directory) =>
                {
                    if (StringComparer.Ordinal.Equals(
                            stage,
                            "after_dpo_directory_create_before_marker"))
                    {
                        dpoDirectory = directory;
                    }
                };
            DefaultLogicalCapabilityTrainer.RegistryPublicationObserverForTesting =
                stage =>
                {
                    if (!StringComparer.Ordinal.Equals(
                            stage,
                            "after_registry_publication"))
                        return;
                    Assert.NotNull(dpoDirectory);
                    File.WriteAllText(
                        Path.Combine(dpoDirectory!, "foreign-sentinel.txt"),
                        "foreign");
                };

            var result = await new DefaultLogicalCapabilityTrainer(
                    checkpointRoot,
                    admissionAuthority:
                    new RecordingLogicalCapabilityTrainingAdmissionAuthority())
                .TrainAndRegisterAsync(BuildFastTrainingRequest(
                    "transient-foreign",
                    "Fix a runtime issue.",
                    BuildTopologyFor44(),
                    flows,
                    units));

            Assert.False(result.IsSuccess);
            Assert.Equal(
                "GENERATED_CAPABILITY_TRANSIENT_FOREIGN_CONTENT",
                result.ErrorCode);
            Assert.Equal("foreign", File.ReadAllText(Path.Combine(
                dpoDirectory!,
                "foreign-sentinel.txt")));
            Assert.Empty(flows.ListDescriptors());
            Assert.Empty(units.ListDescriptors());
        }
        finally
        {
            DefaultLogicalCapabilityTrainer.PublicationStageObserverForTesting =
                priorPublicationObserver;
            DefaultLogicalCapabilityTrainer.RegistryPublicationObserverForTesting =
                priorRegistryObserver;
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task Publication_SuccessPromotesMetricEvidenceAndLeavesNoStaging()
    {
        var checkpointRoot = Path.Combine(
            Path.GetTempPath(),
            $"default-capability-durable-metrics-{Guid.NewGuid():N}");
        var priorObserver =
            DefaultLogicalCapabilityTrainer.PublicationStageObserverForTesting;
        var transitionRenameAttempted = false;
        var retainedChildHandleBlockedEarlyDirectoryRename = false;
        try
        {
            DefaultLogicalCapabilityTrainer.PublicationStageObserverForTesting =
                (stage, _) =>
                {
                    if (!OperatingSystem.IsWindows() ||
                        !StringComparer.Ordinal.Equals(
                            stage,
                            "before_directory_move"))
                    {
                        return;
                    }

                    transitionRenameAttempted = true;
                    var staging = Assert.Single(Directory.EnumerateDirectories(
                        checkpointRoot,
                        ".generated-capability-publication-staging-*",
                        SearchOption.TopDirectoryOnly));
                    var probe = staging + "-premature-rename-probe";
                    try
                    {
                        Directory.Move(staging, probe);
                        Directory.Move(probe, staging);
                    }
                    catch (IOException)
                    {
                        retainedChildHandleBlockedEarlyDirectoryRename = true;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        retainedChildHandleBlockedEarlyDirectoryRename = true;
                    }
                };
            var result = await new DefaultLogicalCapabilityTrainer(
                    checkpointRoot,
                    admissionAuthority:
                    new RecordingLogicalCapabilityTrainingAdmissionAuthority())
                .TrainAndRegisterAsync(BuildFastTrainingRequest(
                    "durable-metrics",
                    "Fix a runtime issue.",
                    BuildTopologyFor44(),
                    new InMemoryLogicalFlowRegistry(),
                    new InMemoryLogicalUnitRegistry()));

            Assert.True(result.IsSuccess, result.ErrorMessage);
            if (OperatingSystem.IsWindows())
            {
                Assert.True(transitionRenameAttempted);
                Assert.True(retainedChildHandleBlockedEarlyDirectoryRename);
            }
            var metricEvidence = result.Value!.EvidenceIds
                .Where(path => path.Contains(
                    $"{Path.DirectorySeparatorChar}dpo-metrics{Path.DirectorySeparatorChar}",
                    StringComparison.Ordinal))
                .ToArray();
            Assert.NotEmpty(metricEvidence);
            Assert.All(metricEvidence, static path => Assert.True(File.Exists(path)));
            var publicationDirectory = ExpectedPublicationDirectory(
                checkpointRoot,
                "durable-metrics",
                Domain.BUG.ToString(),
                "4.4");
            var manifestPath = Path.Combine(
                publicationDirectory,
                TeachingRegistryPersistence.GeneratedCapabilityManifestFileName);
            var manifest = Assert.IsType<GeneratedCapabilityCheckpointManifest>(
                JsonSerializer.Deserialize<GeneratedCapabilityCheckpointManifest>(
                    File.ReadAllText(manifestPath)));
            Assert.Equal(
                manifest.TokenizerSha256,
                ComputeFileSha256(manifest.TokenizerPath));
            Assert.Equal(
                manifest.PolicyCheckpointSha256,
                ComputeFileSha256(manifest.PolicyCheckpointPath));
            var expectedTopLevel = new HashSet<string>(
                OperatingSystem.IsWindows()
                    ? StringComparer.OrdinalIgnoreCase
                    : StringComparer.Ordinal)
            {
                Path.Combine(publicationDirectory, ".generated-capability.publication-owner"),
                manifest.TokenizerPath,
                manifest.PolicyCheckpointPath,
                manifestPath,
                Path.Combine(publicationDirectory, "dpo-metrics")
            };
            Assert.True(expectedTopLevel.SetEquals(
                Directory.EnumerateFileSystemEntries(
                    publicationDirectory,
                    "*",
                    SearchOption.TopDirectoryOnly)));
            Assert.Empty(Directory.EnumerateDirectories(
                checkpointRoot,
                ".generated-capability-*-staging-*",
                SearchOption.TopDirectoryOnly));
        }
        finally
        {
            DefaultLogicalCapabilityTrainer.PublicationStageObserverForTesting =
                priorObserver;
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task DefaultLogicalCapabilityTrainer_CancellationAfterFinalPermitDoesNotAbortCommit()
    {
        var checkpointRoot = Path.Combine(
            Path.GetTempPath(),
            $"default-capability-final-cancel-{Guid.NewGuid():N}");
        using var cancellation = new CancellationTokenSource();
        var priorObserver =
            DefaultLogicalCapabilityTrainer.FinalPublicationObserverForTesting;

        try
        {
            DefaultLogicalCapabilityTrainer.FinalPublicationObserverForTesting =
                cancellation.Cancel;
            var result = await new DefaultLogicalCapabilityTrainer(
                    checkpointRoot,
                    admissionAuthority:
                    new RecordingLogicalCapabilityTrainingAdmissionAuthority())
                .TrainAndRegisterAsync(
                    BuildFastTrainingRequest(
                        "final-permit-cancel",
                        "Fix a runtime issue.",
                        BuildTopologyFor44(),
                        new InMemoryLogicalFlowRegistry(),
                        new InMemoryLogicalUnitRegistry()),
                    cancellation.Token);

            Assert.True(cancellation.IsCancellationRequested);
            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.True(result.Value!.CapabilityRegistered);
        }
        finally
        {
            DefaultLogicalCapabilityTrainer.FinalPublicationObserverForTesting =
                priorObserver;
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Theory]
    [InlineData("tokenizer.json")]
    [InlineData("policy.bin")]
    [InlineData(TeachingRegistryPersistence.GeneratedCapabilityManifestFileName)]
    public async Task Publication_ForeignFileReplacementAfterRegistryPublicationCannotCommitForeignBytes(
        string artifactName)
    {
        var checkpointRoot = Path.Combine(
            Path.GetTempPath(),
            $"default-capability-foreign-file-{Guid.NewGuid():N}");
        var flows = new InMemoryLogicalFlowRegistry();
        var units = new InMemoryLogicalUnitRegistry();
        var sentinel = Encoding.UTF8.GetBytes($"foreign-{artifactName}");
        var replacementWon = false;
        var publicationDirectory = ExpectedPublicationDirectory(
            checkpointRoot,
            "foreign-file-race",
            Domain.BUG.ToString(),
            "4.4");
        var priorObserver =
            DefaultLogicalCapabilityTrainer.RegistryPublicationObserverForTesting;

        try
        {
            DefaultLogicalCapabilityTrainer.RegistryPublicationObserverForTesting =
                stage =>
                {
                    if (!StringComparer.Ordinal.Equals(
                            stage,
                            "after_registry_publication"))
                        return;
                    var path = Path.Combine(publicationDirectory, artifactName);
                    try
                    {
                        File.Delete(path);
                        File.WriteAllBytes(path, sentinel);
                        replacementWon = true;
                    }
                    catch (IOException)
                    {
                    }
                    catch (UnauthorizedAccessException)
                    {
                    }
                };
            var result = await new DefaultLogicalCapabilityTrainer(
                    checkpointRoot,
                    admissionAuthority:
                    new RecordingLogicalCapabilityTrainingAdmissionAuthority())
                .TrainAndRegisterAsync(BuildFastTrainingRequest(
                    "foreign-file-race",
                    "Fix a runtime issue.",
                    BuildTopologyFor44(),
                    flows,
                    units));

            var finalPath = Path.Combine(publicationDirectory, artifactName);
            if (replacementWon)
            {
                Assert.False(result.IsSuccess);
                Assert.Equal(
                    "GENERATED_CAPABILITY_PUBLICATION_IDENTITY_CHANGED",
                    result.ErrorCode);
                Assert.Equal(sentinel, File.ReadAllBytes(finalPath));
                Assert.Empty(flows.ListDescriptors());
                Assert.Empty(units.ListDescriptors());
            }
            else
            {
                Assert.True(result.IsSuccess, result.ErrorMessage);
                Assert.False(sentinel.SequenceEqual(File.ReadAllBytes(finalPath)));
            }
        }
        finally
        {
            DefaultLogicalCapabilityTrainer.RegistryPublicationObserverForTesting =
                priorObserver;
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task Publication_ByteIdenticalReplacementAtFinalHandoffIsBlockedOrFailsClosed()
    {
        if (!OperatingSystem.IsWindows())
            return;
        var checkpointRoot = Path.Combine(
            Path.GetTempPath(),
            $"default-capability-final-identical-{Guid.NewGuid():N}");
        var replacementWon = false;
        byte[]? identicalBytes = null;
        var priorObserver =
            DefaultLogicalCapabilityTrainer.PublicationStageObserverForTesting;

        try
        {
            DefaultLogicalCapabilityTrainer.PublicationStageObserverForTesting =
                (stage, directory) =>
                {
                    if (!StringComparer.Ordinal.Equals(
                            stage,
                            "after_directory_move_before_handle_acquisition"))
                    {
                        return;
                    }
                    var path = Path.Combine(directory, "tokenizer.json");
                    using (var source = new FileStream(
                               path,
                               FileMode.Open,
                               FileAccess.Read,
                               FileShare.Read | FileShare.Write | FileShare.Delete))
                    using (var copy = new MemoryStream())
                    {
                        source.CopyTo(copy);
                        identicalBytes = copy.ToArray();
                    }
                    try
                    {
                        File.Delete(path);
                        File.WriteAllBytes(path, identicalBytes);
                        replacementWon = true;
                    }
                    catch (IOException)
                    {
                    }
                    catch (UnauthorizedAccessException)
                    {
                    }
                };

            var result = await new DefaultLogicalCapabilityTrainer(
                    checkpointRoot,
                    admissionAuthority:
                    new RecordingLogicalCapabilityTrainingAdmissionAuthority())
                .TrainAndRegisterAsync(BuildFastTrainingRequest(
                    "final-identical-race",
                    "Fix a runtime issue.",
                    BuildTopologyFor44(),
                    new InMemoryLogicalFlowRegistry(),
                    new InMemoryLogicalUnitRegistry()));

            Assert.NotNull(identicalBytes);
            if (replacementWon)
            {
                Assert.False(result.IsSuccess);
                Assert.Equal(
                    "GENERATED_CAPABILITY_PUBLICATION_IDENTITY_CHANGED",
                    result.ErrorCode);
            }
            else
            {
                Assert.True(result.IsSuccess, result.ErrorMessage);
            }
        }
        finally
        {
            DefaultLogicalCapabilityTrainer.PublicationStageObserverForTesting =
                priorObserver;
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Theory]
    [InlineData("tokenizer")]
    [InlineData("classifier")]
    public async Task Publication_ByteIdenticalStagedReplacementFailsClosedAndPreservesForeign(
        string artifact)
    {
        if (!OperatingSystem.IsWindows())
            return;
        var checkpointRoot = Path.Combine(
            Path.GetTempPath(),
            $"default-capability-staged-identical-{artifact}-{Guid.NewGuid():N}");
        var replacementWon = false;
        string? replacedPath = null;
        byte[]? identicalBytes = null;
        var priorObserver =
            DefaultLogicalCapabilityTrainer.PublicationStageObserverForTesting;

        try
        {
            DefaultLogicalCapabilityTrainer.PublicationStageObserverForTesting =
                (stage, _) =>
                {
                    if (!StringComparer.Ordinal.Equals(
                            stage,
                            "before_directory_publication"))
                    {
                        return;
                    }
                    replacedPath = artifact == "classifier"
                        ? Assert.Single(Directory.EnumerateFiles(
                            checkpointRoot,
                            ".domain-classifier-training-state-*.json",
                            SearchOption.AllDirectories))
                        : Path.Combine(
                            Assert.Single(Directory.EnumerateDirectories(
                                checkpointRoot,
                                ".generated-capability-publication-staging-*",
                                SearchOption.TopDirectoryOnly)),
                            "tokenizer.json");
                    identicalBytes = File.ReadAllBytes(replacedPath);
                    try
                    {
                        File.Delete(replacedPath);
                        File.WriteAllBytes(replacedPath, identicalBytes);
                        replacementWon = true;
                    }
                    catch (IOException)
                    {
                    }
                    catch (UnauthorizedAccessException)
                    {
                    }
                };

            var result = await new DefaultLogicalCapabilityTrainer(
                    checkpointRoot,
                    admissionAuthority:
                    new RecordingLogicalCapabilityTrainingAdmissionAuthority())
                .TrainAndRegisterAsync(BuildFastTrainingRequest(
                    $"staged-identical-{artifact}",
                    "Fix a runtime issue.",
                    BuildTopologyFor44(),
                    new InMemoryLogicalFlowRegistry(),
                    new InMemoryLogicalUnitRegistry()));

            Assert.NotNull(replacedPath);
            Assert.NotNull(identicalBytes);
            if (replacementWon)
            {
                Assert.False(result.IsSuccess);
                Assert.Equal(
                    artifact == "classifier"
                        ? "DOMAIN_CLASSIFIER_STATE_PUBLICATION_FAILED"
                        : "GENERATED_CAPABILITY_PUBLICATION_IDENTITY_CHANGED",
                    result.ErrorCode);
                Assert.Equal(identicalBytes, File.ReadAllBytes(replacedPath!));
            }
            else
            {
                Assert.True(result.IsSuccess, result.ErrorMessage);
            }
        }
        finally
        {
            DefaultLogicalCapabilityTrainer.PublicationStageObserverForTesting =
                priorObserver;
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task Publication_ForeignDirectoryReplacementAfterMoveIsNeverDeleted()
    {
        var checkpointRoot = Path.Combine(
            Path.GetTempPath(),
            $"default-capability-foreign-directory-{Guid.NewGuid():N}");
        const string sentinelName = "foreign-owner.txt";
        var replacementWon = false;
        var priorObserver =
            DefaultLogicalCapabilityTrainer.PublicationStageObserverForTesting;

        try
        {
            DefaultLogicalCapabilityTrainer.PublicationStageObserverForTesting =
                (stage, directory) =>
                {
                    if (!StringComparer.Ordinal.Equals(stage, "after_directory_move"))
                        return;
                    try
                    {
                        Directory.Delete(directory, recursive: true);
                        Directory.CreateDirectory(directory);
                        File.WriteAllText(
                            Path.Combine(directory, sentinelName),
                            "foreign");
                        replacementWon = true;
                    }
                    catch (IOException)
                    {
                    }
                    catch (UnauthorizedAccessException)
                    {
                    }
                };
            var result = await new DefaultLogicalCapabilityTrainer(
                    checkpointRoot,
                    admissionAuthority:
                    new RecordingLogicalCapabilityTrainingAdmissionAuthority())
                .TrainAndRegisterAsync(BuildFastTrainingRequest(
                    "foreign-directory-race",
                    "Fix a runtime issue.",
                    BuildTopologyFor44(),
                    new InMemoryLogicalFlowRegistry(),
                    new InMemoryLogicalUnitRegistry()));

            if (replacementWon)
            {
                Assert.False(result.IsSuccess);
                Assert.Equal(
                    "GENERATED_CAPABILITY_PUBLICATION_IDENTITY_CHANGED",
                    result.ErrorCode);
                Assert.Equal("foreign", File.ReadAllText(Path.Combine(
                    ExpectedPublicationDirectory(
                        checkpointRoot,
                        "foreign-directory-race",
                        Domain.BUG.ToString(),
                        "4.4"),
                    sentinelName)));
            }
            else
            {
                Assert.True(result.IsSuccess, result.ErrorMessage);
            }
        }
        finally
        {
            DefaultLogicalCapabilityTrainer.PublicationStageObserverForTesting =
                priorObserver;
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Theory]
    [InlineData("after_directory_move_before_handle_acquisition")]
    [InlineData("after_tokenizer_handle_acquisition")]
    [InlineData("after_policy_handle_acquisition")]
    [InlineData("after_manifest_handle_acquisition")]
    [InlineData("after_metric_handle_acquisition")]
    public async Task Publication_FatalAfterMoveOrHandleAcquisitionClosesHandlesAndRollsBack(
        string fatalStage)
    {
        var checkpointRoot = Path.Combine(
            Path.GetTempPath(),
            $"default-capability-fatal-handle-{Guid.NewGuid():N}");
        var flows = new InMemoryLogicalFlowRegistry();
        var units = new InMemoryLogicalUnitRegistry();
        var priorObserver =
            DefaultLogicalCapabilityTrainer.PublicationStageObserverForTesting;
        try
        {
            DefaultLogicalCapabilityTrainer.PublicationStageObserverForTesting =
                (stage, _) =>
                {
                    if (StringComparer.Ordinal.Equals(stage, fatalStage))
                    {
                        throw new OutOfMemoryException(
                            "injected fatal publication failure");
                    }
                };

            await Assert.ThrowsAsync<OutOfMemoryException>(() =>
                new DefaultLogicalCapabilityTrainer(
                        checkpointRoot,
                        admissionAuthority:
                        new RecordingLogicalCapabilityTrainingAdmissionAuthority())
                    .TrainAndRegisterAsync(BuildFastTrainingRequest(
                        $"fatal-{fatalStage}",
                        "Fix a runtime issue.",
                        BuildTopologyFor44(),
                        flows,
                        units)));

            Assert.Empty(flows.ListDescriptors());
            Assert.Empty(units.ListDescriptors());
            AssertNoGeneratedStagingResidue(checkpointRoot);
            Assert.Empty(Directory.Exists(checkpointRoot)
                ? Directory.EnumerateDirectories(
                    checkpointRoot,
                    "generated-capability-*",
                    SearchOption.AllDirectories)
                : []);
        }
        finally
        {
            DefaultLogicalCapabilityTrainer.PublicationStageObserverForTesting =
                priorObserver;
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Theory]
    [InlineData(".domain-classifier-training-backup-orphan.json", false)]
    [InlineData(".domain-classifier-training-backup-renamed.json", true)]
    [InlineData(".domain-classifier-training-new-orphan.json", true)]
    [InlineData(".domain-classifier-training-transaction-journal.json", true)]
    public async Task Publication_AbandonedClassifierRenameBoundariesFailClosed(
        string orphanName,
        bool createActive)
    {
        var checkpointRoot = Path.Combine(
            Path.GetTempPath(),
            $"default-capability-classifier-orphan-{Guid.NewGuid():N}");
        Directory.CreateDirectory(checkpointRoot);
        var orphanPath = Path.Combine(checkpointRoot, orphanName);
        var orphanBytes = Encoding.UTF8.GetBytes("[]");
        File.WriteAllBytes(orphanPath, orphanBytes);
        var activePath = Path.Combine(
            checkpointRoot,
            "domain-classifier-training.json");
        if (createActive)
            File.WriteAllBytes(activePath, Encoding.UTF8.GetBytes("[]"));

        try
        {
            var result = await new DefaultLogicalCapabilityTrainer(
                    checkpointRoot,
                    admissionAuthority:
                    new RecordingLogicalCapabilityTrainingAdmissionAuthority())
                .TrainAndRegisterAsync(BuildFastTrainingRequest(
                    $"orphan-{SanitizeForTest(orphanName)}",
                    "Fix a runtime issue.",
                    BuildTopologyFor44(),
                    new InMemoryLogicalFlowRegistry(),
                    new InMemoryLogicalUnitRegistry()));

            Assert.False(result.IsSuccess);
            Assert.Equal(
                "DOMAIN_CLASSIFIER_TRANSACTION_RECOVERY_REQUIRED",
                result.ErrorCode);
            Assert.Equal(orphanBytes, File.ReadAllBytes(orphanPath));
            Assert.Equal(createActive, File.Exists(activePath));
        }
        finally
        {
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Theory]
    [InlineData("bytes")]
    [InlineData("rows")]
    [InlineData("predicates")]
    public async Task Publication_BoundedClassifierStateFailsClosedBeforeMutation(
        string scenario)
    {
        if (!OperatingSystem.IsWindows())
            return;

        var checkpointRoot = Path.Combine(
            Path.GetTempPath(),
            $"default-capability-classifier-bound-{scenario}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(checkpointRoot);
        var classifierStatePath = Path.Combine(
            checkpointRoot,
            "domain-classifier-training.json");
        switch (scenario)
        {
            case "bytes":
                using (var stream = new FileStream(
                           classifierStatePath,
                           FileMode.CreateNew,
                           FileAccess.Write,
                           FileShare.None))
                {
                    stream.SetLength(
                        DefaultLogicalCapabilityTrainer
                            .MaximumDomainClassifierStateBytes + 1L);
                }
                break;
            case "rows":
                File.WriteAllBytes(
                    classifierStatePath,
                    JsonSerializer.SerializeToUtf8Bytes(Enumerable.Range(
                            0,
                            DefaultLogicalCapabilityTrainer
                                .MaximumDomainClassifierExampleCount + 1)
                        .Select(index => new
                        {
                            RunId = $"classifier-row-{index}",
                            Domain = (int)Domain.BUG,
                            Predicates = new[] { 1 },
                            CreatedAtUtc = DateTimeOffset.UnixEpoch
                        })
                        .ToArray()));
                break;
            case "predicates":
                File.WriteAllBytes(
                    classifierStatePath,
                    JsonSerializer.SerializeToUtf8Bytes(new[]
                    {
                        new
                        {
                            RunId = "classifier-predicate-bound",
                            Domain = (int)Domain.BUG,
                            Predicates = Enumerable.Range(
                                    0,
                                    DefaultLogicalCapabilityTrainer
                                        .MaximumDomainClassifierPredicateCount + 1)
                                .ToArray(),
                            CreatedAtUtc = DateTimeOffset.UnixEpoch
                        }
                    }));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(scenario));
        }

        var originalLength = new FileInfo(classifierStatePath).Length;
        var originalHash = Convert.ToHexString(SHA256.HashData(
            File.ReadAllBytes(classifierStatePath)));
        var flows = new InMemoryLogicalFlowRegistry();
        var units = new InMemoryLogicalUnitRegistry();
        try
        {
            var result = await new DefaultLogicalCapabilityTrainer(
                    checkpointRoot,
                    admissionAuthority:
                    new RecordingLogicalCapabilityTrainingAdmissionAuthority())
                .TrainAndRegisterAsync(BuildFastTrainingRequest(
                    $"classifier-bound-{scenario}",
                    "Fix a runtime issue.",
                    BuildTopologyFor44(),
                    flows,
                    units));

            Assert.False(result.IsSuccess);
            Assert.Equal("DOMAIN_CLASSIFIER_STATE_INVALID", result.ErrorCode);
            Assert.Empty(flows.ListDescriptors());
            Assert.Empty(units.ListDescriptors());
            Assert.Equal(originalLength, new FileInfo(classifierStatePath).Length);
            Assert.Equal(
                originalHash,
                Convert.ToHexString(SHA256.HashData(
                    File.ReadAllBytes(classifierStatePath))));
            AssertNoGeneratedStagingResidue(checkpointRoot);
            Assert.Empty(Directory.EnumerateDirectories(
                checkpointRoot,
                "*",
                SearchOption.TopDirectoryOnly));
        }
        finally
        {
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task Publication_OrdinaryFailureAfterFirstUnitRollsBackRegistriesCacheAndFiles()
    {
        var checkpointRoot = Path.Combine(
            Path.GetTempPath(),
            $"default-capability-registry-rollback-{Guid.NewGuid():N}");
        var flows = new InMemoryLogicalFlowRegistry();
        var units = new InMemoryLogicalUnitRegistry();
        var trainer = new DefaultLogicalCapabilityTrainer(
            checkpointRoot,
            admissionAuthority:
            new RecordingLogicalCapabilityTrainingAdmissionAuthority());
        var priorObserver =
            DefaultLogicalCapabilityTrainer.RegistryPublicationObserverForTesting;

        try
        {
            DefaultLogicalCapabilityTrainer.RegistryPublicationObserverForTesting =
                stage =>
                {
                    Assert.Equal("after_unit_registration", stage);
                    throw new InvalidOperationException("injected registry failure");
                };
            var result = await trainer.TrainAndRegisterAsync(
                BuildFastTrainingRequest(
                    "registry-rollback",
                    "Fix a runtime issue.",
                    BuildTopologyFor44(),
                    flows,
                    units));

            Assert.False(result.IsSuccess);
            Assert.Equal(
                "GENERATED_CAPABILITY_REGISTRY_PUBLICATION_FAILED",
                result.ErrorCode);
            Assert.Empty(flows.ListDescriptors());
            Assert.Empty(units.ListDescriptors());
            Assert.Equal(0, ReadPrivateDictionaryCount(
                trainer,
                "_teachingCache"));
            Assert.Equal(0, ReadPrivateDictionaryCount(
                trainer,
                "_manifestCacheByUnitId"));
            Assert.False(Directory.Exists(ExpectedPublicationDirectory(
                checkpointRoot,
                "registry-rollback",
                Domain.BUG.ToString(),
                "4.4")));
        }
        finally
        {
            DefaultLogicalCapabilityTrainer.RegistryPublicationObserverForTesting =
                priorObserver;
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task Publication_FailureAfterClassifierStateRollsBackEveryPublishedSurface()
    {
        var checkpointRoot = Path.Combine(
            Path.GetTempPath(),
            $"default-capability-full-rollback-{Guid.NewGuid():N}");
        Directory.CreateDirectory(checkpointRoot);
        var classifierStatePath = Path.Combine(
            checkpointRoot,
            "domain-classifier-training.json");
        var priorClassifierState = Encoding.UTF8.GetBytes("[]");
        File.WriteAllBytes(classifierStatePath, priorClassifierState);
        var flows = new InMemoryLogicalFlowRegistry();
        var units = new InMemoryLogicalUnitRegistry();
        var trainer = new DefaultLogicalCapabilityTrainer(
            checkpointRoot,
            admissionAuthority:
            new RecordingLogicalCapabilityTrainingAdmissionAuthority());
        var priorObserver =
            DefaultLogicalCapabilityTrainer.RegistryPublicationObserverForTesting;

        try
        {
            DefaultLogicalCapabilityTrainer.RegistryPublicationObserverForTesting =
                stage =>
                {
                    if (StringComparer.Ordinal.Equals(
                            stage,
                            "after_classifier_state_publication"))
                    {
                        throw new InvalidOperationException(
                            "injected post-state failure");
                    }
                };
            var result = await trainer.TrainAndRegisterAsync(
                BuildFastTrainingRequest(
                    "full-rollback",
                    "Fix a runtime issue.",
                    BuildTopologyFor44(),
                    flows,
                    units));

            Assert.False(result.IsSuccess);
            Assert.Equal(
                "GENERATED_CAPABILITY_REGISTRY_PUBLICATION_FAILED",
                result.ErrorCode);
            Assert.Equal(
                priorClassifierState,
                File.ReadAllBytes(classifierStatePath));
            Assert.Empty(flows.ListDescriptors());
            Assert.Empty(units.ListDescriptors());
            Assert.Equal(0, ReadPrivateDictionaryCount(
                trainer,
                "_teachingCache"));
            Assert.Equal(0, ReadPrivateDictionaryCount(
                trainer,
                "_manifestCacheByUnitId"));
            Assert.False(Directory.Exists(ExpectedPublicationDirectory(
                checkpointRoot,
                "full-rollback",
                Domain.BUG.ToString(),
                "4.4")));
        }
        finally
        {
            DefaultLogicalCapabilityTrainer.RegistryPublicationObserverForTesting =
                priorObserver;
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task Publication_FailureAfterFinalIdentityVerificationRestoresClassifierBackup()
    {
        var checkpointRoot = Path.Combine(
            Path.GetTempPath(),
            $"default-capability-precommit-rollback-{Guid.NewGuid():N}");
        Directory.CreateDirectory(checkpointRoot);
        var classifierStatePath = Path.Combine(
            checkpointRoot,
            "domain-classifier-training.json");
        var priorClassifierState = Encoding.UTF8.GetBytes("[]");
        File.WriteAllBytes(classifierStatePath, priorClassifierState);
        var flows = new InMemoryLogicalFlowRegistry();
        var units = new InMemoryLogicalUnitRegistry();
        var priorObserver =
            DefaultLogicalCapabilityTrainer.RegistryPublicationObserverForTesting;

        try
        {
            DefaultLogicalCapabilityTrainer.RegistryPublicationObserverForTesting =
                stage =>
                {
                    if (StringComparer.Ordinal.Equals(
                            stage,
                            "before_publication_commit"))
                    {
                        throw new InvalidOperationException(
                            "injected precommit failure");
                    }
                };

            var result = await new DefaultLogicalCapabilityTrainer(
                    checkpointRoot,
                    admissionAuthority:
                    new RecordingLogicalCapabilityTrainingAdmissionAuthority())
                .TrainAndRegisterAsync(BuildFastTrainingRequest(
                    "precommit-rollback",
                    "Fix a runtime issue.",
                    BuildTopologyFor44(),
                    flows,
                    units));

            Assert.False(result.IsSuccess);
            Assert.Equal(
                "GENERATED_CAPABILITY_REGISTRY_PUBLICATION_FAILED",
                result.ErrorCode);
            Assert.Equal(
                priorClassifierState,
                File.ReadAllBytes(classifierStatePath));
            Assert.Empty(flows.ListDescriptors());
            Assert.Empty(units.ListDescriptors());
        }
        finally
        {
            DefaultLogicalCapabilityTrainer.RegistryPublicationObserverForTesting =
                priorObserver;
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task Publication_ByteIdenticalPriorClassifierReplacementBeforePublishIsPreserved()
    {
        if (!OperatingSystem.IsWindows())
            return;
        var checkpointRoot = Path.Combine(
            Path.GetTempPath(),
            $"default-capability-prior-classifier-identical-{Guid.NewGuid():N}");
        Directory.CreateDirectory(checkpointRoot);
        var classifierStatePath = Path.Combine(
            checkpointRoot,
            "domain-classifier-training.json");
        var priorClassifierState = Encoding.UTF8.GetBytes("[]");
        File.WriteAllBytes(classifierStatePath, priorClassifierState);
        var replacementPath = Path.Combine(
            checkpointRoot,
            ".byte-identical-classifier-replacement.json");
        var replacementWon = false;
        var priorObserver =
            DefaultLogicalCapabilityTrainer.PublicationStageObserverForTesting;

        try
        {
            DefaultLogicalCapabilityTrainer.PublicationStageObserverForTesting =
                (stage, _) =>
                {
                    if (!StringComparer.Ordinal.Equals(
                            stage,
                            "before_directory_publication"))
                    {
                        return;
                    }
                    try
                    {
                        File.WriteAllBytes(
                            replacementPath,
                            priorClassifierState);
                        File.Move(
                            replacementPath,
                            classifierStatePath,
                            overwrite: true);
                        replacementWon = true;
                    }
                    catch (IOException)
                    {
                        if (File.Exists(replacementPath))
                            File.Delete(replacementPath);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        if (File.Exists(replacementPath))
                            File.Delete(replacementPath);
                    }
                };

            var result = await new DefaultLogicalCapabilityTrainer(
                    checkpointRoot,
                    admissionAuthority:
                    new RecordingLogicalCapabilityTrainingAdmissionAuthority())
                .TrainAndRegisterAsync(BuildFastTrainingRequest(
                    "prior-classifier-identical-race",
                    "Fix a runtime issue.",
                    BuildTopologyFor44(),
                    new InMemoryLogicalFlowRegistry(),
                    new InMemoryLogicalUnitRegistry()));

            if (replacementWon)
            {
                Assert.False(result.IsSuccess);
                Assert.Equal(
                    "DOMAIN_CLASSIFIER_STATE_PUBLICATION_FAILED",
                    result.ErrorCode);
                Assert.Equal(
                    priorClassifierState,
                    File.ReadAllBytes(classifierStatePath));
                Assert.Empty(Directory.EnumerateFiles(
                    checkpointRoot,
                    ".domain-classifier-training-backup-*",
                    SearchOption.TopDirectoryOnly));
            }
            else
            {
                Assert.True(result.IsSuccess, result.ErrorMessage);
            }
        }
        finally
        {
            DefaultLogicalCapabilityTrainer.PublicationStageObserverForTesting =
                priorObserver;
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task Publication_BackupProtectionFailureRestoresExactPriorClassifier()
    {
        if (!OperatingSystem.IsWindows())
            return;

        var checkpointRoot = Path.Combine(
            Path.GetTempPath(),
            $"default-capability-backup-protection-{Guid.NewGuid():N}");
        Directory.CreateDirectory(checkpointRoot);
        var classifierStatePath = Path.Combine(
            checkpointRoot,
            "domain-classifier-training.json");
        var priorClassifierState = Encoding.UTF8.GetBytes("[]");
        File.WriteAllBytes(classifierStatePath, priorClassifierState);
        var flows = new InMemoryLogicalFlowRegistry();
        var units = new InMemoryLogicalUnitRegistry();
        var priorFactory = DefaultLogicalCapabilityTrainer
            .ClassifierBackupProtectionFailureFactoryForTesting;

        try
        {
            DefaultLogicalCapabilityTrainer
                .ClassifierBackupProtectionFailureFactoryForTesting =
                static () => new IOException(
                    "injected backup protection acquisition failure");

            var result = await new DefaultLogicalCapabilityTrainer(
                    checkpointRoot,
                    admissionAuthority:
                    new RecordingLogicalCapabilityTrainingAdmissionAuthority())
                .TrainAndRegisterAsync(BuildFastTrainingRequest(
                    "backup-protection-rollback",
                    "Fix a runtime issue.",
                    BuildTopologyFor44(),
                    flows,
                    units));

            Assert.False(result.IsSuccess);
            Assert.Equal(
                "DOMAIN_CLASSIFIER_STATE_PUBLICATION_FAILED",
                result.ErrorCode);
            Assert.Equal(
                priorClassifierState,
                File.ReadAllBytes(classifierStatePath));
            Assert.Empty(Directory.EnumerateFileSystemEntries(
                checkpointRoot,
                ".domain-classifier-training-backup-*",
                SearchOption.TopDirectoryOnly));
            Assert.Empty(flows.ListDescriptors());
            Assert.Empty(units.ListDescriptors());
        }
        finally
        {
            DefaultLogicalCapabilityTrainer
                .ClassifierBackupProtectionFailureFactoryForTesting =
                priorFactory;
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task Publication_PriorClassifierSwapBeforeExactRenameNeverRedirectsBackup()
    {
        if (!OperatingSystem.IsWindows())
            return;
        var checkpointRoot = Path.Combine(
            Path.GetTempPath(),
            $"default-capability-prior-classifier-rename-{Guid.NewGuid():N}");
        Directory.CreateDirectory(checkpointRoot);
        var classifierStatePath = Path.Combine(
            checkpointRoot,
            "domain-classifier-training.json");
        var displacedPath = Path.Combine(
            checkpointRoot,
            ".attacker-displaced-prior-classifier.json");
        var replacementPath = Path.Combine(
            checkpointRoot,
            ".attacker-byte-identical-prior-classifier.json");
        var priorClassifierState = Encoding.UTF8.GetBytes("[]");
        File.WriteAllBytes(classifierStatePath, priorClassifierState);
        var replacementWon = false;
        var priorObserver = DefaultLogicalCapabilityTrainer
            .ClassifierIdentityRenameObserverForTesting;

        try
        {
            DefaultLogicalCapabilityTrainer
                .ClassifierIdentityRenameObserverForTesting =
                (stage, sourcePath, _) =>
                {
                    if (!StringComparer.Ordinal.Equals(
                            stage,
                            "before_prior_active_to_backup_identity_rename"))
                    {
                        return;
                    }
                    try
                    {
                        File.WriteAllBytes(
                            replacementPath,
                            priorClassifierState);
                        File.Move(
                            sourcePath,
                            displacedPath,
                            overwrite: false);
                        File.Move(
                            replacementPath,
                            sourcePath,
                            overwrite: false);
                        replacementWon = true;
                    }
                    catch (Exception exception)
                        when (exception is IOException or UnauthorizedAccessException)
                    {
                        try
                        {
                            if (File.Exists(displacedPath) &&
                                !File.Exists(sourcePath))
                            {
                                File.Move(
                                    displacedPath,
                                    sourcePath,
                                    overwrite: false);
                            }
                            if (File.Exists(replacementPath))
                                File.Delete(replacementPath);
                        }
                        catch (Exception cleanupException)
                            when (cleanupException is IOException or
                                  UnauthorizedAccessException)
                        {
                        }
                    }
                };

            var result = await new DefaultLogicalCapabilityTrainer(
                    checkpointRoot,
                    admissionAuthority:
                    new RecordingLogicalCapabilityTrainingAdmissionAuthority())
                .TrainAndRegisterAsync(BuildFastTrainingRequest(
                    "prior-classifier-rename-race",
                    "Fix a runtime issue.",
                    BuildTopologyFor44(),
                    new InMemoryLogicalFlowRegistry(),
                    new InMemoryLogicalUnitRegistry()));

            if (replacementWon)
            {
                Assert.False(result.IsSuccess);
                Assert.Equal(
                    "DOMAIN_CLASSIFIER_STATE_PUBLICATION_FAILED",
                    result.ErrorCode);
                Assert.Equal(
                    priorClassifierState,
                    File.ReadAllBytes(classifierStatePath));
                var exactBackup = Assert.Single(
                    Directory.EnumerateFiles(
                        checkpointRoot,
                        ".domain-classifier-training-backup-*",
                        SearchOption.TopDirectoryOnly));
                Assert.Equal(
                    priorClassifierState,
                    File.ReadAllBytes(exactBackup));
                Assert.False(File.Exists(displacedPath));
            }
            else
            {
                Assert.True(result.IsSuccess, result.ErrorMessage);
            }
        }
        finally
        {
            DefaultLogicalCapabilityTrainer
                .ClassifierIdentityRenameObserverForTesting = priorObserver;
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task Publication_RestoreDestinationCollisionPreservesForeignAndExactBackup()
    {
        if (!OperatingSystem.IsWindows())
            return;
        var checkpointRoot = Path.Combine(
            Path.GetTempPath(),
            $"default-capability-classifier-restore-collision-{Guid.NewGuid():N}");
        Directory.CreateDirectory(checkpointRoot);
        var classifierStatePath = Path.Combine(
            checkpointRoot,
            "domain-classifier-training.json");
        var priorClassifierState = Encoding.UTF8.GetBytes("[]");
        File.WriteAllBytes(classifierStatePath, priorClassifierState);
        var collisionCreated = false;
        var backupReplacementWon = false;
        var backupReplacementPath = Path.Combine(
            checkpointRoot,
            ".attacker-backup-replacement.json");
        var displacedBackupPath = Path.Combine(
            checkpointRoot,
            ".attacker-displaced-backup.json");
        var priorRegistryObserver = DefaultLogicalCapabilityTrainer
            .RegistryPublicationObserverForTesting;
        var priorRenameObserver = DefaultLogicalCapabilityTrainer
            .ClassifierIdentityRenameObserverForTesting;

        try
        {
            DefaultLogicalCapabilityTrainer.RegistryPublicationObserverForTesting =
                stage =>
                {
                    if (StringComparer.Ordinal.Equals(
                            stage,
                            "after_classifier_state_publication"))
                    {
                        throw new IOException(
                            "injected classifier rollback");
                    }
                };
            DefaultLogicalCapabilityTrainer
                .ClassifierIdentityRenameObserverForTesting =
                (stage, sourcePath, destinationPath) =>
                {
                    if (!StringComparer.Ordinal.Equals(
                            stage,
                            "before_prior_backup_to_active_identity_rename"))
                    {
                        return;
                    }
                    File.WriteAllBytes(
                        backupReplacementPath,
                        priorClassifierState);
                    try
                    {
                        File.Move(
                            sourcePath,
                            displacedBackupPath,
                            overwrite: false);
                        File.Move(
                            backupReplacementPath,
                            sourcePath,
                            overwrite: false);
                        backupReplacementWon = true;
                    }
                    catch (Exception exception)
                        when (exception is IOException or UnauthorizedAccessException)
                    {
                        if (File.Exists(backupReplacementPath))
                            File.Delete(backupReplacementPath);
                    }
                    File.WriteAllBytes(
                        destinationPath,
                        priorClassifierState);
                    collisionCreated = true;
                };

            var result = await new DefaultLogicalCapabilityTrainer(
                    checkpointRoot,
                    admissionAuthority:
                    new RecordingLogicalCapabilityTrainingAdmissionAuthority())
                .TrainAndRegisterAsync(BuildFastTrainingRequest(
                    "classifier-restore-collision",
                    "Fix a runtime issue.",
                    BuildTopologyFor44(),
                    new InMemoryLogicalFlowRegistry(),
                    new InMemoryLogicalUnitRegistry()));

            Assert.False(result.IsSuccess);
            Assert.Equal(
                "GENERATED_CAPABILITY_REGISTRY_PUBLICATION_FAILED",
                result.ErrorCode);
            Assert.False(backupReplacementWon);
            Assert.True(collisionCreated);
            Assert.Equal(
                priorClassifierState,
                File.ReadAllBytes(classifierStatePath));
            var exactBackup = Assert.Single(
                Directory.EnumerateFiles(
                    checkpointRoot,
                    ".domain-classifier-training-backup-*",
                    SearchOption.TopDirectoryOnly));
            Assert.Equal(
                priorClassifierState,
                File.ReadAllBytes(exactBackup));
            Assert.Empty(Directory.EnumerateFiles(
                checkpointRoot,
                ".domain-classifier-training-new-*",
                SearchOption.TopDirectoryOnly));
            Assert.Empty(Directory.EnumerateFiles(
                checkpointRoot,
                ".domain-classifier-training-state-*",
                SearchOption.AllDirectories));
        }
        finally
        {
            DefaultLogicalCapabilityTrainer.RegistryPublicationObserverForTesting =
                priorRegistryObserver;
            DefaultLogicalCapabilityTrainer
                .ClassifierIdentityRenameObserverForTesting = priorRenameObserver;
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task Publication_PriorClassifierHandoffSucceedsAndReleasesProtectiveHandle()
    {
        if (!OperatingSystem.IsWindows())
            return;
        var checkpointRoot = Path.Combine(
            Path.GetTempPath(),
            $"default-capability-classifier-handoff-{Guid.NewGuid():N}");
        Directory.CreateDirectory(checkpointRoot);
        var classifierStatePath = Path.Combine(
            checkpointRoot,
            "domain-classifier-training.json");
        var renamedStatePath = Path.Combine(
            checkpointRoot,
            "domain-classifier-training.renamed.json");
        File.WriteAllBytes(
            classifierStatePath,
            Encoding.UTF8.GetBytes("[]"));

        try
        {
            var result = await new DefaultLogicalCapabilityTrainer(
                    checkpointRoot,
                    admissionAuthority:
                    new RecordingLogicalCapabilityTrainingAdmissionAuthority())
                .TrainAndRegisterAsync(BuildFastTrainingRequest(
                    "classifier-handoff-success",
                    "Fix a runtime issue.",
                    BuildTopologyFor44(),
                    new InMemoryLogicalFlowRegistry(),
                    new InMemoryLogicalUnitRegistry()));

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.True(File.Exists(classifierStatePath));
            Assert.Empty(Directory.EnumerateFiles(
                checkpointRoot,
                ".domain-classifier-training-backup-*",
                SearchOption.TopDirectoryOnly));

            File.Move(
                classifierStatePath,
                renamedStatePath,
                overwrite: false);

            Assert.False(File.Exists(classifierStatePath));
            Assert.True(File.Exists(renamedStatePath));
        }
        finally
        {
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task Publication_ClassifierMutationAtFinalObserverCannotCommitChangedIdentity()
    {
        var checkpointRoot = Path.Combine(
            Path.GetTempPath(),
            $"default-capability-classifier-race-{Guid.NewGuid():N}");
        Directory.CreateDirectory(checkpointRoot);
        var classifierStatePath = Path.Combine(
            checkpointRoot,
            "domain-classifier-training.json");
        var priorClassifierState = Encoding.UTF8.GetBytes("[]");
        var foreignClassifierState = Encoding.UTF8.GetBytes("[\"foreign\"]");
        File.WriteAllBytes(classifierStatePath, priorClassifierState);
        var flows = new InMemoryLogicalFlowRegistry();
        var units = new InMemoryLogicalUnitRegistry();
        var observerInvoked = false;
        var deletionDenied = false;
        var replacementDenied = false;
        var replacementWon = false;
        var priorObserver =
            DefaultLogicalCapabilityTrainer.RegistryPublicationObserverForTesting;
        try
        {
            DefaultLogicalCapabilityTrainer.RegistryPublicationObserverForTesting =
                stage =>
                {
                    if (!StringComparer.Ordinal.Equals(
                            stage,
                            "before_publication_commit"))
                    {
                        return;
                    }
                    observerInvoked = true;
                    try
                    {
                        File.Delete(classifierStatePath);
                    }
                    catch (IOException)
                    {
                        deletionDenied = true;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        deletionDenied = true;
                    }
                    try
                    {
                        File.WriteAllBytes(
                            classifierStatePath,
                            foreignClassifierState);
                        replacementWon = true;
                    }
                    catch (IOException)
                    {
                        replacementDenied = true;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        replacementDenied = true;
                    }
                };

            var result = await new DefaultLogicalCapabilityTrainer(
                    checkpointRoot,
                    admissionAuthority:
                    new RecordingLogicalCapabilityTrainingAdmissionAuthority())
                .TrainAndRegisterAsync(BuildFastTrainingRequest(
                    "classifier-race",
                    "Fix a runtime issue.",
                    BuildTopologyFor44(),
                    flows,
                    units));

            Assert.True(observerInvoked);
            if (replacementWon)
            {
                // POSIX разрешает удалить имя открытого файла и создать новое.
                // В этом случае контракт не "deny", а fail-closed: чужой JSON
                // остаётся на месте и registry transaction откатывается.
                Assert.False(result.IsSuccess);
                Assert.Equal(
                    "DOMAIN_CLASSIFIER_STATE_PUBLICATION_FAILED",
                    result.ErrorCode);
                Assert.Equal(
                    foreignClassifierState,
                    File.ReadAllBytes(classifierStatePath));
                Assert.Empty(flows.ListDescriptors());
                Assert.Empty(units.ListDescriptors());
            }
            else
            {
                Assert.True(deletionDenied);
                Assert.True(replacementDenied);
                Assert.True(result.IsSuccess, result.ErrorMessage);
                Assert.NotEqual(
                    foreignClassifierState,
                    File.ReadAllBytes(classifierStatePath));
            }
        }
        finally
        {
            DefaultLogicalCapabilityTrainer.RegistryPublicationObserverForTesting =
                priorObserver;
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task Publication_ClassifierActiveHandleSurvivesBackupDeletionAndClosesMutationWindow()
    {
        var checkpointRoot = Path.Combine(
            Path.GetTempPath(),
            $"default-capability-post-backup-race-{Guid.NewGuid():N}");
        Directory.CreateDirectory(checkpointRoot);
        var classifierStatePath = Path.Combine(
            checkpointRoot,
            "domain-classifier-training.json");
        var priorClassifierState = Encoding.UTF8.GetBytes("[]");
        var foreignClassifierState = Encoding.UTF8.GetBytes(
            "[\"post-backup-foreign\"]");
        File.WriteAllBytes(classifierStatePath, priorClassifierState);
        var replacementWon = false;
        var priorObserver = DefaultLogicalCapabilityTrainer
            .PostClassifierBackupDeletionObserverForTesting;
        try
        {
            DefaultLogicalCapabilityTrainer
                .PostClassifierBackupDeletionObserverForTesting = () =>
                {
                    try
                    {
                        File.Delete(classifierStatePath);
                        File.WriteAllBytes(
                            classifierStatePath,
                            foreignClassifierState);
                        replacementWon = true;
                    }
                    catch (IOException)
                    {
                    }
                    catch (UnauthorizedAccessException)
                    {
                    }
                };

            var result = await new DefaultLogicalCapabilityTrainer(
                    checkpointRoot,
                    admissionAuthority:
                    new RecordingLogicalCapabilityTrainingAdmissionAuthority())
                .TrainAndRegisterAsync(BuildFastTrainingRequest(
                    "post-backup-race",
                    "Fix a runtime issue.",
                    BuildTopologyFor44(),
                    new InMemoryLogicalFlowRegistry(),
                    new InMemoryLogicalUnitRegistry()));

            if (!replacementWon)
            {
                Assert.True(result.IsSuccess, result.ErrorMessage);
                Assert.False(foreignClassifierState.SequenceEqual(
                    File.ReadAllBytes(classifierStatePath)));
            }
            else
            {
                Assert.False(result.IsSuccess);
                Assert.Equal(
                    "DOMAIN_CLASSIFIER_STATE_PUBLICATION_FAILED",
                    result.ErrorCode);
                Assert.Equal(
                    foreignClassifierState,
                    File.ReadAllBytes(classifierStatePath));
                // На POSIX active path уже занят чужой заменой, поэтому rollback
                // не перезаписывает её. Доказанная prior identity сохраняется в
                // backup-файле до ручного разбора отказа.
                var restoreCopy = Assert.Single(
                    Directory.EnumerateFiles(
                        checkpointRoot,
                        ".domain-classifier-training-backup-*",
                        SearchOption.TopDirectoryOnly));
                Assert.Equal(
                    priorClassifierState,
                    File.ReadAllBytes(restoreCopy));
            }
        }
        finally
        {
            DefaultLogicalCapabilityTrainer
                .PostClassifierBackupDeletionObserverForTesting = priorObserver;
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task Publication_ClassifierBackupReplacementAttemptIsBlockedAndDeletionProceeds()
    {
        var checkpointRoot = Path.Combine(
            Path.GetTempPath(),
            $"default-capability-backup-delete-{Guid.NewGuid():N}");
        Directory.CreateDirectory(checkpointRoot);
        var classifierStatePath = Path.Combine(
            checkpointRoot,
            "domain-classifier-training.json");
        var priorClassifierState = Encoding.UTF8.GetBytes("[]");
        File.WriteAllBytes(classifierStatePath, priorClassifierState);
        var replacementWon = false;
        string? backupPath = null;
        var priorFactory = DefaultLogicalCapabilityTrainer
            .ClassifierBackupDeletionFailureFactoryForTesting;
        try
        {
            DefaultLogicalCapabilityTrainer
                .ClassifierBackupDeletionFailureFactoryForTesting =
                () =>
                {
                    backupPath = Assert.Single(
                        Directory.EnumerateFiles(
                            checkpointRoot,
                            ".domain-classifier-training-backup-*",
                            SearchOption.TopDirectoryOnly));
                    try
                    {
                        File.Delete(backupPath);
                        File.WriteAllBytes(
                            backupPath,
                            priorClassifierState);
                        replacementWon = true;
                    }
                    catch (IOException)
                    {
                    }
                    catch (UnauthorizedAccessException)
                    {
                    }
                    return null;
                };
            var result = await new DefaultLogicalCapabilityTrainer(
                    checkpointRoot,
                    admissionAuthority:
                    new RecordingLogicalCapabilityTrainingAdmissionAuthority())
                .TrainAndRegisterAsync(BuildFastTrainingRequest(
                    "backup-delete-failure",
                    "Fix a runtime issue.",
                    BuildTopologyFor44(),
                    new InMemoryLogicalFlowRegistry(),
                    new InMemoryLogicalUnitRegistry()));

            Assert.NotNull(backupPath);
            if (replacementWon)
            {
                // Linux не может сделать открытый backup mandatory-locked.
                // Поэтому byte-identical replacement остаётся чужой identity:
                // commit запрещён, а replacement не удаляется по имени.
                Assert.False(result.IsSuccess);
                Assert.Equal(
                    "DOMAIN_CLASSIFIER_STATE_PUBLICATION_FAILED",
                    result.ErrorCode);
                Assert.True(File.Exists(backupPath));
                Assert.Equal(
                    priorClassifierState,
                    File.ReadAllBytes(backupPath));
                Assert.False(File.Exists(classifierStatePath));
            }
            else
            {
                Assert.True(result.IsSuccess, result.ErrorMessage);
                Assert.NotEqual(
                    priorClassifierState,
                    File.ReadAllBytes(classifierStatePath));
                Assert.False(replacementWon);
                Assert.Empty(Directory.EnumerateFiles(
                    checkpointRoot,
                    ".domain-classifier-training-backup-*",
                    SearchOption.TopDirectoryOnly));
            }
        }
        finally
        {
            DefaultLogicalCapabilityTrainer
                .ClassifierBackupDeletionFailureFactoryForTesting = priorFactory;
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task Publication_CleanupFailureDoesNotMaskPrimaryOrFatalFailure()
    {
        var checkpointRoot = Path.Combine(
            Path.GetTempPath(),
            $"default-capability-cleanup-failure-{Guid.NewGuid():N}");
        var flows = new InMemoryLogicalFlowRegistry();
        var units = new InMemoryLogicalUnitRegistry();
        var priorRegistryObserver =
            DefaultLogicalCapabilityTrainer.RegistryPublicationObserverForTesting;
        var priorCleanupFactory =
            DefaultLogicalCapabilityTrainer.CleanupFailureFactoryForTesting;
        var priorCleanupFailure =
            DefaultLogicalCapabilityTrainer.LastCleanupFailureForTesting;

        try
        {
            DefaultLogicalCapabilityTrainer.CleanupFailureFactoryForTesting =
                static () => new IOException("injected cleanup failure");
            DefaultLogicalCapabilityTrainer.RegistryPublicationObserverForTesting =
                static _ => throw new InvalidOperationException(
                    "injected primary failure");
            var ordinary = await new DefaultLogicalCapabilityTrainer(
                    checkpointRoot,
                    admissionAuthority:
                    new RecordingLogicalCapabilityTrainingAdmissionAuthority())
                .TrainAndRegisterAsync(BuildFastTrainingRequest(
                    "cleanup-primary",
                    "Fix a runtime issue.",
                    BuildTopologyFor44(),
                    flows,
                    units));

            Assert.False(ordinary.IsSuccess);
            Assert.Equal(
                "GENERATED_CAPABILITY_REGISTRY_PUBLICATION_FAILED",
                ordinary.ErrorCode);
            Assert.Empty(flows.ListDescriptors());
            Assert.Empty(units.ListDescriptors());
            AssertNoGeneratedStagingResidue(checkpointRoot);

            DefaultLogicalCapabilityTrainer.RegistryPublicationObserverForTesting =
                static _ => throw new OutOfMemoryException(
                    "injected fatal failure");
            var fatalFlows = new InMemoryLogicalFlowRegistry();
            var fatalUnits = new InMemoryLogicalUnitRegistry();
            await Assert.ThrowsAsync<OutOfMemoryException>(() =>
                new DefaultLogicalCapabilityTrainer(
                        checkpointRoot,
                        admissionAuthority:
                        new RecordingLogicalCapabilityTrainingAdmissionAuthority())
                    .TrainAndRegisterAsync(BuildFastTrainingRequest(
                        "cleanup-fatal",
                        "Fix a runtime issue.",
                        BuildTopologyFor44(),
                        fatalFlows,
                        fatalUnits)));
            Assert.Empty(fatalFlows.ListDescriptors());
            Assert.Empty(fatalUnits.ListDescriptors());
            AssertNoGeneratedStagingResidue(checkpointRoot);
        }
        finally
        {
            DefaultLogicalCapabilityTrainer.RegistryPublicationObserverForTesting =
                priorRegistryObserver;
            DefaultLogicalCapabilityTrainer.CleanupFailureFactoryForTesting =
                priorCleanupFactory;
            DefaultLogicalCapabilityTrainer.LastCleanupFailureForTesting =
                priorCleanupFailure;
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task Publication_FatalPrimaryWinsAndCleanupFatalsRemainOrdered()
    {
        var root = Path.Combine(Path.GetTempPath(), $"cleanup-arbiter-{Guid.NewGuid():N}");
        var priorObserver = DefaultLogicalCapabilityTrainer.RegistryPublicationObserverForTesting;
        var priorCleanup = DefaultLogicalCapabilityTrainer.CleanupFailureFactoryForTesting;
        var primary = new OutOfMemoryException("body-fatal");
        var publicationCleanup = new OutOfMemoryException("publication-cleanup-fatal");
        var admissionCleanup = new OutOfMemoryException("admission-cleanup-fatal");
        try
        {
            DefaultLogicalCapabilityTrainer.RegistryPublicationObserverForTesting =
                _ => throw primary;
            DefaultLogicalCapabilityTrainer.CleanupFailureFactoryForTesting =
                () => publicationCleanup;
            var authority = new RecordingLogicalCapabilityTrainingAdmissionAuthority(
                leaseDisposeFailureFactory: () => admissionCleanup);
            var thrown = await Assert.ThrowsAsync<OutOfMemoryException>(() =>
                new DefaultLogicalCapabilityTrainer(root, admissionAuthority: authority)
                    .TrainAndRegisterAsync(BuildFastTrainingRequest("arbiter-fatal",
                        "Fix runtime issue.", BuildTopologyFor44(),
                        new InMemoryLogicalFlowRegistry(),
                        new InMemoryLogicalUnitRegistry())));
            Assert.Same(primary, thrown);
            var aggregate = Assert.IsType<AggregateException>(
                thrown.Data["CAPABILITY_TRAINING_CLEANUP_FAILURE"]);
            Assert.Equal([publicationCleanup, admissionCleanup],
                aggregate.InnerExceptions);
        }
        finally
        {
            DefaultLogicalCapabilityTrainer.RegistryPublicationObserverForTesting = priorObserver;
            DefaultLogicalCapabilityTrainer.CleanupFailureFactoryForTesting = priorCleanup;
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task Publication_NoPrimaryFatalCleanupIsRethrownExactly()
    {
        var root = Path.Combine(Path.GetTempPath(), $"cleanup-only-{Guid.NewGuid():N}");
        var cleanup = new OutOfMemoryException("admission-cleanup-only");
        try
        {
            var authority = new RecordingLogicalCapabilityTrainingAdmissionAuthority(
                leaseDisposeFailureFactory: () => cleanup);
            var thrown = await Assert.ThrowsAsync<OutOfMemoryException>(() =>
                new DefaultLogicalCapabilityTrainer(root, admissionAuthority: authority)
                    .TrainAndRegisterAsync(BuildFastTrainingRequest("cleanup-only",
                        "Fix runtime issue.", BuildTopologyFor44(),
                        new InMemoryLogicalFlowRegistry(),
                        new InMemoryLogicalUnitRegistry())));
            Assert.Same(cleanup, thrown);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task Publication_NoPrimaryNestedFatalCleanupIsFlattenedInOrder()
    {
        var root = Path.Combine(Path.GetTempPath(), $"cleanup-nested-{Guid.NewGuid():N}");
        var priorObserver = DefaultLogicalCapabilityTrainer.RegistryPublicationObserverForTesting;
        var priorCleanup = DefaultLogicalCapabilityTrainer.CleanupFailureFactoryForTesting;
        var a = new OutOfMemoryException("publication-fatal-a");
        var b = new OutOfMemoryException("nested-fatal-b");
        var c = new OutOfMemoryException("admission-fatal-c");
        a.Data["CAPABILITY_TRAINING_CLEANUP_FAILURE"] = b;
        try
        {
            DefaultLogicalCapabilityTrainer.RegistryPublicationObserverForTesting =
                _ => throw new InvalidOperationException("ordinary body failure");
            DefaultLogicalCapabilityTrainer.CleanupFailureFactoryForTesting = () => a;
            var authority = new RecordingLogicalCapabilityTrainingAdmissionAuthority(
                leaseDisposeFailureFactory: () => c);
            var thrown = await Assert.ThrowsAsync<OutOfMemoryException>(() =>
                new DefaultLogicalCapabilityTrainer(root, admissionAuthority: authority)
                    .TrainAndRegisterAsync(BuildFastTrainingRequest("nested-cleanup",
                        "Fix runtime issue.", BuildTopologyFor44(),
                        new InMemoryLogicalFlowRegistry(),
                        new InMemoryLogicalUnitRegistry())));
            Assert.Same(a, thrown);
            var aggregate = Assert.IsType<AggregateException>(
                thrown.Data["CAPABILITY_TRAINING_CLEANUP_FAILURE"]);
            Assert.Equal([b, c], aggregate.InnerExceptions);
        }
        finally
        {
            DefaultLogicalCapabilityTrainer.RegistryPublicationObserverForTesting = priorObserver;
            DefaultLogicalCapabilityTrainer.CleanupFailureFactoryForTesting = priorCleanup;
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Theory]
    [InlineData("classification")]
    [InlineData("record")]
    [InlineData("diagnostics")]
    public async Task Publication_ArbiterInternalFaultCannotReplaceExactCleanupFatal(
        string stage)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            $"cleanup-arbiter-internal-{Guid.NewGuid():N}");
        var cleanup = new OutOfMemoryException("exact cleanup fatal");
        var injected = new OutOfMemoryException("arbiter internal fault");
        var priorObserver = DefaultLogicalCapabilityTrainer
            .CleanupArbiterFaultObserverForTesting;

        try
        {
            DefaultLogicalCapabilityTrainer
                .CleanupArbiterFaultObserverForTesting =
                currentStage =>
                {
                    if (StringComparer.Ordinal.Equals(currentStage, stage))
                        throw injected;
                };
            var authority =
                new RecordingLogicalCapabilityTrainingAdmissionAuthority(
                    leaseDisposeFailureFactory: () => cleanup);

            var thrown = await Assert.ThrowsAsync<OutOfMemoryException>(() =>
                new DefaultLogicalCapabilityTrainer(
                        root,
                        admissionAuthority: authority)
                    .TrainAndRegisterAsync(BuildFastTrainingRequest(
                        $"arbiter-internal-{stage}",
                        "Fix runtime issue.",
                        BuildTopologyFor44(),
                        new InMemoryLogicalFlowRegistry(),
                        new InMemoryLogicalUnitRegistry())));

            Assert.Same(cleanup, thrown);
        }
        finally
        {
            DefaultLogicalCapabilityTrainer
                .CleanupArbiterFaultObserverForTesting = priorObserver;
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Publication_ConcurrentSameRouteHasOneCompleteWinner()
    {
        var checkpointRoot = Path.Combine(
            Path.GetTempPath(),
            $"default-capability-concurrent-{Guid.NewGuid():N}");
        var flows = new InMemoryLogicalFlowRegistry();
        var units = new InMemoryLogicalUnitRegistry();
        var authority = new RecordingLogicalCapabilityTrainingAdmissionAuthority();
        var topology = BuildTopologyFor44();

        try
        {
            var first = new DefaultLogicalCapabilityTrainer(
                checkpointRoot,
                admissionAuthority: authority).TrainAndRegisterAsync(
                BuildFastTrainingRequest(
                    "same-route",
                    "Fix a runtime issue.",
                    topology,
                    flows,
                    units));
            var second = new DefaultLogicalCapabilityTrainer(
                checkpointRoot,
                admissionAuthority: authority).TrainAndRegisterAsync(
                BuildFastTrainingRequest(
                    "same-route",
                    "Fix a runtime issue.",
                    topology,
                    flows,
                    units));

            var results = await Task.WhenAll(first, second);
            Assert.Single(results, static result => result.IsSuccess);
            Assert.Single(results, static result => !result.IsSuccess);
            Assert.Single(flows.ListDescriptors());
            Assert.Single(
                units.ListDescriptors(),
                descriptor => descriptor.UnitRole ==
                    DefaultLogicalCapabilityTrainer.GeneratedCapabilityUnitRole);
            Assert.Single(
                units.ListDescriptors(),
                descriptor => descriptor.UnitRole ==
                    SemanticCompilerAdapter.DomainClassifierUnit.Role);
            var directory = ExpectedPublicationDirectory(
                checkpointRoot,
                "same-route",
                Domain.BUG.ToString(),
                "4.4");
            Assert.True(File.Exists(Path.Combine(directory, "tokenizer.json")));
            Assert.True(File.Exists(Path.Combine(directory, "policy.bin")));
            Assert.True(File.Exists(Path.Combine(
                directory,
                TeachingRegistryPersistence.GeneratedCapabilityManifestFileName)));
        }
        finally
        {
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task Publication_CanonicalRootLeaseRejectsStaleDistinctTrainerAndPreservesClassifierHistory()
    {
        var checkpointRoot = Path.Combine(
            Path.GetTempPath(),
            $"default-capability-shared-root-{Guid.NewGuid():N}");
        using var admissionBarrier = new Barrier(2);
        var firstAuthority = new RecordingLogicalCapabilityTrainingAdmissionAuthority(
            leaseObserver: _ => Assert.True(
                admissionBarrier.SignalAndWait(TimeSpan.FromSeconds(20))));
        var secondAuthority = new RecordingLogicalCapabilityTrainingAdmissionAuthority(
            leaseObserver: _ => Assert.True(
                admissionBarrier.SignalAndWait(TimeSpan.FromSeconds(20))));
        var firstFlows = new InMemoryLogicalFlowRegistry();
        var firstUnits = new InMemoryLogicalUnitRegistry();
        var secondFlows = new InMemoryLogicalFlowRegistry();
        var secondUnits = new InMemoryLogicalUnitRegistry();

        try
        {
            var first = Task.Run(() => new DefaultLogicalCapabilityTrainer(
                    checkpointRoot,
                    admissionAuthority: firstAuthority)
                .TrainAndRegisterAsync(BuildFastTrainingRequest(
                    "shared-root-first",
                    "Fix a runtime issue.",
                    BuildTopologyFor44(),
                    firstFlows,
                    firstUnits)));
            var second = Task.Run(() => new DefaultLogicalCapabilityTrainer(
                    Path.Combine(checkpointRoot, "."),
                    admissionAuthority: secondAuthority)
                .TrainAndRegisterAsync(BuildFastTrainingRequest(
                    "shared-root-second",
                    "Fix a different runtime issue.",
                    BuildTopologyFor44(),
                    secondFlows,
                    secondUnits)));

            var results = await Task.WhenAll(first, second);
            Assert.Single(results, static result => result.IsSuccess);
            var stale = Assert.Single(results, static result => !result.IsSuccess);
            Assert.Equal(
                "GENERATED_CAPABILITY_PUBLICATION_SNAPSHOT_CHANGED",
                stale.ErrorCode);

            var third = await new DefaultLogicalCapabilityTrainer(
                    checkpointRoot + Path.DirectorySeparatorChar,
                    admissionAuthority:
                    new RecordingLogicalCapabilityTrainingAdmissionAuthority())
                .TrainAndRegisterAsync(BuildFastTrainingRequest(
                    "shared-root-third",
                    "Design authorization architecture.",
                    BuildTopologyFor44(),
                    new InMemoryLogicalFlowRegistry(),
                    new InMemoryLogicalUnitRegistry()));
            Assert.True(third.IsSuccess, third.ErrorMessage);

            using var classifier = JsonDocument.Parse(File.ReadAllBytes(
                Path.Combine(
                    checkpointRoot,
                    "domain-classifier-training.json")));
            Assert.Equal(2, classifier.RootElement.GetArrayLength());
            Assert.Empty(Directory.EnumerateFiles(
                checkpointRoot,
                ".domain-classifier-training-*",
                SearchOption.TopDirectoryOnly));
        }
        finally
        {
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task DefaultLogicalCapabilityTrainer_RegistersProbationaryRetrievalUnitAndReportsHonestRetryMetrics()
    {
        // Build a topology that can route implementation work but intentionally has no implementation flow.
        var topology = new InMemoryLogicalTopologyRegistry();
        var subPhase = topology.AddOrUpdateSubPhase(new LogicalSubPhase(
            "4.1",
            "4",
            "Backend Services",
            "Owns backend service implementation, provider connectors, integration code, and application service changes.",
            ["backend-services"],
            ["IMPLEMENT"],
            ["default-capability-trainer-test"]));
        Assert.True(subPhase.IsSuccess, subPhase.ErrorMessage);

        // Keep checkpoint artifacts outside the repository and remove them after the test.
        var checkpointRoot = Path.Combine(Path.GetTempPath(), $"default-capability-trainer-{Guid.NewGuid():N}");
        var flows = new InMemoryLogicalFlowRegistry();
        var units = new InMemoryLogicalUnitRegistry();

        try
        {
            // Build the adaptive orchestrator with the default trainer.
            var orchestrator = new AdaptiveLogicalOrchestrator(
                topology,
                new PromptRoutingInputCompilerUnit(),
                new SubPhaseRouterUnit(topology),
                flows,
                units,
                capabilityTrainer: new DefaultLogicalCapabilityTrainer(
                    checkpointRoot,
                    admissionAuthority: new RecordingLogicalCapabilityTrainingAdmissionAuthority()));

            // The first pass should hit a knowledge edge, train, register, and then retry through the generated flow.
            var result = await orchestrator.RunWithCapabilityGenerationAsync(new LogicalOrchestratorRequest(
                "default-capability-training-run",
                AdaptiveLogicalOrchestrator.ScenarioId,
                "Implement the backend service and provider connector for the integration layer.",
                new LogicalExecutionFlags(),
                new Dictionary<string, object>()));

            // The adaptive run must succeed after retry.
            Assert.True(result.IsSuccess, result.ErrorMessage);

            // The retry must use the generated retrieval capability flow rather than returning the missing-flow knowledge edge.
            Assert.StartsWith(DefaultLogicalCapabilityTrainer.GeneratedCapabilityFlowIdPrefix, result.Value!.SelectedFlowId);
            Assert.Equal("PASS", result.Value.OverallVerdict);
            Assert.Equal("True", result.Value.FlowResult.Metrics["generated_capability_flow"]);

            // The KNN wrapper retrieves a stored output, so its registry descriptor must not claim learned inference.
            var retrievalDescriptor = Assert.Single(units.ListDescriptors(), descriptor =>
                descriptor.UnitRole == DefaultLogicalCapabilityTrainer.GeneratedCapabilityUnitRole &&
                descriptor.DomainTag == Domain.PLAN.ToString());
            Assert.False(retrievalDescriptor.IsLearned);
            Assert.Equal(LogicalUnitTrustState.Probationary, retrievalDescriptor.TrustState);
            Assert.Equal(typeof(string), retrievalDescriptor.InputType);
            Assert.Equal(typeof(string), retrievalDescriptor.OutputType);

            var metrics = result.Value.FlowResult.Metrics;
            Assert.Equal("True", metrics["retrieval_unit_selected"]);
            Assert.Equal(retrievalDescriptor.UnitId, metrics["retrieval_unit_id"]);
            Assert.True(double.TryParse(
                metrics["retrieval_unit_confidence"],
                System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture,
                out var retrievalConfidence));
            Assert.InRange(retrievalConfidence, 0.0, 0.99);
            Assert.Equal("False", metrics["learned_unit_selected"]);
            Assert.False(metrics.ContainsKey("learned_unit_id"));
            Assert.False(metrics.ContainsKey("learned_unit_confidence"));
            Assert.Equal("USE_PROBATIONARY_RETRIEVAL_CAPABILITY", result.Value.FlowResult.Summary.NextAction);
            Assert.Contains("retrieval", result.Value.FlowResult.Summary.Notes, StringComparison.OrdinalIgnoreCase);

            // The flow registry must now contain a retryable generated flow for the missing sub-phase.
            Assert.Contains(flows.ListDescriptors(), descriptor =>
                descriptor.FlowId.StartsWith(DefaultLogicalCapabilityTrainer.GeneratedCapabilityFlowIdPrefix, StringComparison.Ordinal) &&
                descriptor.SubPhaseId == "4.1");

            // The DPO teaching path must have written at least one metric file to the configured checkpoint root.
            Assert.True(Directory.Exists(checkpointRoot));
            Assert.NotEmpty(Directory.GetFiles(checkpointRoot, "dpo_step_*.json", SearchOption.AllDirectories));
        }
        finally
        {
            // Clean test artifacts without touching repository files.
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task DefaultLogicalCapabilityTrainer_RepeatedSubPhaseTeachingContinuesFromActiveCheckpoint()
    {
        // Build a topology with one repeat-taught implementation sub-phase.
        var topology = new InMemoryLogicalTopologyRegistry();
        var subPhase = topology.AddOrUpdateSubPhase(new LogicalSubPhase(
            "4.4",
            "4",
            "Refactoring and Code Quality",
            "Owns internal code improvement, simplification, dependency cleanup, and maintainability.",
            ["software-development"],
            ["IMPLEMENT"],
            ["default-capability-trainer-test"]));
        Assert.True(subPhase.IsSuccess, subPhase.ErrorMessage);

        var checkpointRoot = Path.Combine(Path.GetTempPath(), $"default-capability-continuation-{Guid.NewGuid():N}");
        var flows = new InMemoryLogicalFlowRegistry();
        var units = new InMemoryLogicalUnitRegistry();
        var trainer = new DefaultLogicalCapabilityTrainer(
            checkpointRoot,
            admissionAuthority: new RecordingLogicalCapabilityTrainingAdmissionAuthority());

        try
        {
            // Teach the first row for 4.4.
            var first = await trainer.TrainAndRegisterAsync(BuildTrainingRequest(
                "capability-continuation-row-0001",
                "Fix a runtime code quality issue in the Figma export path.",
                topology,
                flows,
                units));
            Assert.True(first.IsSuccess, first.ErrorMessage);
            Assert.True(first.Value!.CapabilityRegistered);

            var firstSnapshot = TeachingRegistryPersistence.Capture("continuation-test", 1, units, flows, checkpointRoot);
            Assert.True(firstSnapshot.IsSuccess, firstSnapshot.ErrorMessage);
            var firstManifest = Assert.Single(firstSnapshot.Value!.GeneratedCapabilities, manifest => manifest.SubPhaseId == "4.4");
            Assert.Null(firstManifest.ParentUnitId);
            Assert.Null(firstManifest.ParentPolicyCheckpointPath);
            Assert.Equal(1, firstManifest.TrainingStepCount);
            Assert.Equal(10_000f, firstManifest.RopeTheta);
            Assert.Equal(1e-5f, firstManifest.RmsNormEps);
            Assert.Equal(
                BpeTokenizer.ComputeArtifactSha256(firstManifest.TokenizerPath),
                firstManifest.TokenizerSha256);
            Assert.Equal(
                ComputeFileSha256(firstManifest.PolicyCheckpointPath),
                firstManifest.PolicyCheckpointSha256);
            AssertRuntimeProvenance(firstManifest);

            // Teach a second row for the same sub-phase with the same live registry and trainer.
            var second = await trainer.TrainAndRegisterAsync(BuildTrainingRequest(
                "capability-continuation-row-0002",
                "Fix another Figma export error and preserve the previous runtime cleanup behavior.",
                topology,
                flows,
                units));
            Assert.True(second.IsSuccess, second.ErrorMessage);
            Assert.True(second.Value!.CapabilityRegistered);
            Assert.Contains("semantic_compression=enabled", second.Value.Notes);
            Assert.Contains("replay_count=", second.Value.Notes);
            Assert.Contains("d09_max_weight=", second.Value.Notes);

            var secondSnapshot = TeachingRegistryPersistence.Capture("continuation-test", 2, units, flows, checkpointRoot);
            Assert.True(secondSnapshot.IsSuccess, secondSnapshot.ErrorMessage);
            var manifests = secondSnapshot.Value!.GeneratedCapabilities
                .Where(manifest => manifest.SubPhaseId == "4.4")
                .OrderBy(manifest => manifest.CreatedAtUtc)
                .ToArray();
            Assert.Equal(2, manifests.Length);

            var continuedManifest = manifests[^1];
            Assert.Equal(firstManifest.UnitId, continuedManifest.ParentUnitId);
            Assert.Equal(firstManifest.PolicyCheckpointPath, continuedManifest.ParentPolicyCheckpointPath);
            Assert.Equal(firstManifest.TokenizerPath, continuedManifest.ParentTokenizerPath);
            Assert.Equal(2, continuedManifest.TrainingStepCount);
            Assert.Equal("1.0.1", continuedManifest.Version);
            Assert.Contains("semantic_compressed:", continuedManifest.TrainingText);
            Assert.NotEqual(firstManifest.PolicyCheckpointPath, continuedManifest.PolicyCheckpointPath);
            Assert.True(File.Exists(continuedManifest.PolicyCheckpointPath));
            Assert.True(File.Exists(continuedManifest.TokenizerPath));
            Assert.NotNull(continuedManifest.RopeTheta);
            Assert.NotNull(continuedManifest.RmsNormEps);
            Assert.Equal(
                BpeTokenizer.ComputeArtifactSha256(continuedManifest.TokenizerPath),
                continuedManifest.TokenizerSha256);
            Assert.Equal(
                ComputeFileSha256(continuedManifest.PolicyCheckpointPath),
                continuedManifest.PolicyCheckpointSha256);
            AssertRuntimeProvenance(continuedManifest);

            // Runtime selection still uses the newest non-rejected version.
            var latest = units.ListDescriptors()
                .Where(descriptor => descriptor.UnitRole == DefaultLogicalCapabilityTrainer.GeneratedCapabilityUnitRole && descriptor.DomainTag == Domain.BUG.ToString())
                .OrderByDescending(descriptor => descriptor.RegistrationOrder)
                .First();
            Assert.Equal(continuedManifest.UnitId, latest.UnitId);
            Assert.Equal("1.0.1", latest.Version);
        }
        finally
        {
            // Clean test artifacts without touching repository files.
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public void DefaultLogicalCapabilityTrainer_RejectsLegacyParentManifestWithoutExplicitProvenance()
    {
        var legacyParent = BuildManifest(
            "legacy-parent-unit",
            Domain.PLAN.ToString(),
            "4.1",
            DateTimeOffset.UnixEpoch);
        var method = typeof(DefaultLogicalCapabilityTrainer).GetMethod(
            "BuildParentProvenance",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        Assert.NotNull(method);

        var result = Assert.IsType<AgentInfrastructure.OperationResult<LogicalDpoParentProvenance>>(
            method.Invoke(null, [legacyParent]));

        Assert.False(result.IsSuccess);
        Assert.Equal("PARENT_CAPABILITY_PROVENANCE_INVALID", result.ErrorCode);
        Assert.Equal("Parent capability provenance is incomplete or invalid.", result.ErrorMessage);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task DefaultLogicalCapabilityTrainer_FreshTrainerContinuesFromVerifiedDiskParent()
    {
        var topology = new InMemoryLogicalTopologyRegistry();
        var subPhase = topology.AddOrUpdateSubPhase(new LogicalSubPhase(
            "4.4",
            "4",
            "Refactoring and Code Quality",
            "Owns internal code improvement, simplification, dependency cleanup, and maintainability.",
            ["software-development"],
            ["IMPLEMENT"],
            ["default-capability-trainer-test"]));
        Assert.True(subPhase.IsSuccess, subPhase.ErrorMessage);

        var checkpointRoot = Path.Combine(
            Path.GetTempPath(),
            $"default-capability-disk-continuation-{Guid.NewGuid():N}");
        var flows = new InMemoryLogicalFlowRegistry();
        var units = new InMemoryLogicalUnitRegistry();

        try
        {
            var firstTrainer = new DefaultLogicalCapabilityTrainer(
                checkpointRoot,
                admissionAuthority: new RecordingLogicalCapabilityTrainingAdmissionAuthority());
            var first = await firstTrainer.TrainAndRegisterAsync(BuildTrainingRequest(
                "capability-disk-continuation-row-0001",
                "Fix a runtime bug in the Figma export path.",
                topology,
                flows,
                units));
            Assert.True(first.IsSuccess, first.ErrorMessage);
            var firstSnapshot = TeachingRegistryPersistence.Capture(
                "disk-continuation-test",
                1,
                units,
                flows,
                checkpointRoot);
            Assert.True(firstSnapshot.IsSuccess, firstSnapshot.ErrorMessage);
            var firstManifest = Assert.Single(
                firstSnapshot.Value!.GeneratedCapabilities,
                manifest => manifest.SubPhaseId == "4.4");
            AssertRuntimeProvenance(firstManifest);

            var freshTrainer = new DefaultLogicalCapabilityTrainer(
                checkpointRoot,
                admissionAuthority: new RecordingLogicalCapabilityTrainingAdmissionAuthority());
            var second = await freshTrainer.TrainAndRegisterAsync(BuildTrainingRequest(
                "capability-disk-continuation-row-0002",
                "Fix another runtime bug in the Figma export path and preserve the previous fix.",
                topology,
                flows,
                units));
            Assert.True(second.IsSuccess, second.ErrorMessage);
            var secondSnapshot = TeachingRegistryPersistence.Capture(
                "disk-continuation-test",
                2,
                units,
                flows,
                checkpointRoot);
            Assert.True(secondSnapshot.IsSuccess, secondSnapshot.ErrorMessage);
            var continuedManifest = Assert.Single(
                secondSnapshot.Value!.GeneratedCapabilities,
                manifest =>
                    manifest.SubPhaseId == "4.4"
                    && second.Value!.RegisteredUnitIds.Contains(
                        manifest.UnitId,
                        StringComparer.Ordinal));

            Assert.Equal(firstManifest.UnitId, continuedManifest.ParentUnitId);
            Assert.Equal(firstManifest.PolicyCheckpointPath, continuedManifest.ParentPolicyCheckpointPath);
            Assert.Equal(firstManifest.TokenizerPath, continuedManifest.ParentTokenizerPath);
            Assert.Equal(2, continuedManifest.TrainingStepCount);
            Assert.Equal("1.0.1", continuedManifest.Version);
            AssertRuntimeProvenance(continuedManifest);
        }
        finally
        {
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task Publication_ExistingFlowWithTamperedExecutableAttestationFailsBeforePublicationMutation()
    {
        var checkpointRoot = Path.Combine(
            Path.GetTempPath(),
            $"default-capability-flow-attestation-{Guid.NewGuid():N}");
        var topology = BuildTopologyFor44();
        var flows = new InMemoryLogicalFlowRegistry();
        try
        {
            var firstUnits = new InMemoryLogicalUnitRegistry();
            var first = await new DefaultLogicalCapabilityTrainer(
                    checkpointRoot,
                    admissionAuthority:
                    new RecordingLogicalCapabilityTrainingAdmissionAuthority())
                .TrainAndRegisterAsync(BuildFastTrainingRequest(
                    "flow-attestation-source",
                    "Fix the original runtime issue.",
                    topology,
                    flows,
                    firstUnits));
            Assert.True(first.IsSuccess, first.ErrorMessage);
            var existingFlows = flows.ListDescriptors();
            var existingFlowsSnapshot = SnapshotFlowDescriptors(existingFlows);
            var manifestPath = Assert.Single(Directory.EnumerateFiles(
                checkpointRoot,
                TeachingRegistryPersistence.GeneratedCapabilityManifestFileName,
                SearchOption.AllDirectories));
            var manifest = Assert.IsType<GeneratedCapabilityCheckpointManifest>(
                JsonSerializer.Deserialize<GeneratedCapabilityCheckpointManifest>(
                    File.ReadAllText(manifestPath)));
            var executableAttestation = Assert.IsType<
                LogicalCapabilityExecutableManifestAttestation>(
                manifest.ExecutableManifestAttestation);
            var tampered = manifest with
            {
                TrustState = LogicalUnitTrustState.Rejected.ToString(),
                ExecutableManifestAttestation = executableAttestation with
                {
                    MacSha256 = new string('0', 64)
                }
            };
            File.WriteAllText(manifestPath, JsonSerializer.Serialize(tampered));
            var publicationMutationReached = false;
            var priorObserver =
                DefaultLogicalCapabilityTrainer.PublicationStageObserverForTesting;
            DefaultLogicalCapabilityTrainer.PublicationStageObserverForTesting =
                (stage, _) => publicationMutationReached |=
                    StringComparer.Ordinal.Equals(
                        stage,
                        "before_directory_publication");
            try
            {
                var secondUnits = new InMemoryLogicalUnitRegistry();
                var second = await new DefaultLogicalCapabilityTrainer(
                        checkpointRoot,
                        admissionAuthority:
                        new RecordingLogicalCapabilityTrainingAdmissionAuthority())
                    .TrainAndRegisterAsync(BuildFastTrainingRequest(
                        "flow-attestation-rejected",
                        "Fix another runtime issue.",
                        topology,
                        flows,
                        secondUnits));

                Assert.False(second.IsSuccess);
                Assert.Equal(
                    "GENERATED_CAPABILITY_REGISTRY_PUBLICATION_INVALID",
                    second.ErrorCode);
                Assert.False(publicationMutationReached);
                Assert.Equal(
                    existingFlowsSnapshot,
                    SnapshotFlowDescriptors(flows.ListDescriptors()));
                Assert.Empty(secondUnits.ListDescriptors());
                Assert.False(Directory.Exists(ExpectedPublicationDirectory(
                    checkpointRoot,
                    "flow-attestation-rejected",
                    Domain.BUG.ToString(),
                    "4.4")));
                AssertNoGeneratedStagingResidue(checkpointRoot);
            }
            finally
            {
                DefaultLogicalCapabilityTrainer.PublicationStageObserverForTesting =
                    priorObserver;
            }
        }
        finally
        {
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task DefaultLogicalCapabilityTrainer_SameDomainBranchesShareAdapterFamily()
    {
        // Build two sub-phases that should teach the same BUG domain adapter while keeping separate flow routes.
        var topology = new InMemoryLogicalTopologyRegistry();
        var codeQuality = topology.AddOrUpdateSubPhase(new LogicalSubPhase(
            "4.4",
            "4",
            "Refactoring and Code Quality",
            "Owns internal code improvement and maintainability.",
            ["software-development"],
            ["IMPLEMENT"],
            ["default-capability-trainer-test"]));
        Assert.True(codeQuality.IsSuccess, codeQuality.ErrorMessage);
        var unitTests = topology.AddOrUpdateSubPhase(new LogicalSubPhase(
            "5.1",
            "5",
            "Unit Test Design",
            "Owns unit test design for runtime failures.",
            ["quality-assurance"],
            ["TEST"],
            ["default-capability-trainer-test"]));
        Assert.True(unitTests.IsSuccess, unitTests.ErrorMessage);

        var checkpointRoot = Path.Combine(Path.GetTempPath(), $"default-capability-domain-{Guid.NewGuid():N}");
        var flows = new InMemoryLogicalFlowRegistry();
        var units = new InMemoryLogicalUnitRegistry();
        var trainer = new DefaultLogicalCapabilityTrainer(
            checkpointRoot,
            admissionAuthority: new RecordingLogicalCapabilityTrainingAdmissionAuthority());

        try
        {
            var request = new LogicalCapabilityTrainingRequest(
                "capability-domain-row-0001",
                new LogicalOrchestratorRequest(
                    "capability-domain-row-0001",
                    AdaptiveLogicalOrchestrator.ScenarioId,
                    "Fix the Figma export error and add a regression test.",
                    new LogicalExecutionFlags(),
                    new Dictionary<string, object>()),
                [
                    BuildKnowledgeEdgeBranch(
                        "4.4",
                        "Fix the Figma export error and add a regression test.",
                        Domain.BUG,
                        topology),
                    BuildKnowledgeEdgeBranch(
                        "5.1",
                        "Fix the Figma export error and add a regression test.",
                        Domain.BUG,
                        topology)
                ],
                topology,
                flows,
                units)
            {
                Domain = Domain.BUG
            };

            var result = await trainer.TrainAndRegisterAsync(request);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.True(result.Value!.CapabilityRegistered);
            Assert.Equal(Domain.BUG.ToString(), result.Value.Domain);
            Assert.Equal(2, result.Value.BatchedBranchCount);
            Assert.True(result.Value.BatchedDpoStepCount >= 1);
            Assert.Equal([1, 1], result.Value.BranchTimings.Select(timing => timing.TrainingStepCount).ToArray());
            Assert.All(result.Value.BranchTimings, timing => Assert.True(timing.DpoMs > 0));
            Assert.All(
                units.ListDescriptors().Where(descriptor => descriptor.UnitRole == DefaultLogicalCapabilityTrainer.GeneratedCapabilityUnitRole),
                descriptor => Assert.Equal(Domain.BUG.ToString(), descriptor.DomainTag));
            var domainClassifierDescriptor = Assert.Single(units.ListDescriptors(), descriptor =>
                descriptor.UnitRole == SemanticCompilerAdapter.DomainClassifierUnit.Role &&
                descriptor.DomainTag == SemanticCompilerAdapter.DomainClassifierUnit.DomainTag);
            Assert.True(domainClassifierDescriptor.IsLearned);
            Assert.Equal(LogicalUnitTrustState.Probationary, domainClassifierDescriptor.TrustState);
            Assert.Contains(flows.ListDescriptors(), descriptor => descriptor.SubPhaseId == "4.4");
            Assert.Contains(flows.ListDescriptors(), descriptor => descriptor.SubPhaseId == "5.1");

            var snapshot = TeachingRegistryPersistence.Capture("domain-family-test", 1, units, flows, checkpointRoot);
            Assert.True(snapshot.IsSuccess, snapshot.ErrorMessage);
            var manifests = snapshot.Value!.GeneratedCapabilities.OrderBy(manifest => manifest.CreatedAtUtc).ToArray();
            Assert.Equal(2, manifests.Length);
            Assert.All(manifests, manifest => Assert.Equal(Domain.BUG.ToString(), manifest.DomainTag));
            Assert.All(manifests, manifest => Assert.Equal("generated-capability-bug", manifest.UnitFamilyId));
            Assert.All(manifests, manifest => Assert.Null(manifest.ParentUnitId));
            Assert.All(manifests, manifest => Assert.Equal(1, manifest.TrainingStepCount));
        }
        finally
        {
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task DefaultLogicalCapabilityTrainer_IdleGateDefersTrainingWhenMachineIsBusy()
    {
        // Build a topology with one missing capability.
        var topology = new InMemoryLogicalTopologyRegistry();
        var subPhase = topology.AddOrUpdateSubPhase(new LogicalSubPhase(
            "4.4",
            "4",
            "Refactoring and Code Quality",
            "Owns internal code improvement and maintainability.",
            ["software-development"],
            ["IMPLEMENT"],
            ["default-capability-trainer-test"]));
        Assert.True(subPhase.IsSuccess, subPhase.ErrorMessage);

        var checkpointRoot = Path.Combine(Path.GetTempPath(), $"default-capability-idle-{Guid.NewGuid():N}");
        var flows = new InMemoryLogicalFlowRegistry();
        var units = new InMemoryLogicalUnitRegistry();
        var trainer = new DefaultLogicalCapabilityTrainer(
            checkpointRoot,
            new BusyIdleGate(),
            admissionAuthority: new RecordingLogicalCapabilityTrainingAdmissionAuthority());

        try
        {
            // Run training while the injected D-14 idle gate says the machine is busy.
            var result = await trainer.TrainAndRegisterAsync(BuildTrainingRequest(
                "capability-idle-row-0001",
                "Fix a runtime code quality issue.",
                topology,
                flows,
                units));

            // The teaching pass should defer cleanly, not train synchronously.
            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.False(result.Value!.CapabilityRegistered);
            Assert.Empty(result.Value.RegisteredUnitIds);
            Assert.Empty(result.Value.RegisteredFlowIds);
            Assert.Contains("idle-learning gate", result.Value.Notes);
            Assert.Empty(units.ListDescriptors());
            Assert.Empty(flows.ListDescriptors());
        }
        finally
        {
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public void DefaultLogicalCapabilityTrainer_ManifestCacheAvoidsRepeatedDiskReads()
    {
        var checkpointRoot = Path.Combine(Path.GetTempPath(), $"default-capability-manifest-cache-{Guid.NewGuid():N}");
        var manifestDirectory = Path.Combine(checkpointRoot, "run-0001", "BUG", "4.4");
        Directory.CreateDirectory(manifestDirectory);
        var manifestPath = Path.Combine(manifestDirectory, TeachingRegistryPersistence.GeneratedCapabilityManifestFileName);
        var manifest = BuildManifest("unit-cache-0001", Domain.BUG.ToString(), "4.4", DateTimeOffset.UtcNow);
        File.WriteAllText(manifestPath, System.Text.Json.JsonSerializer.Serialize(manifest));
        var trainer = new DefaultLogicalCapabilityTrainer(
            checkpointRoot,
            admissionAuthority: new RecordingLogicalCapabilityTrainingAdmissionAuthority());

        try
        {
            var first = InvokeLoadGeneratedCapabilityManifests(trainer);
            Assert.Single(first);

            File.Delete(manifestPath);
            var second = InvokeLoadGeneratedCapabilityManifests(trainer);

            Assert.Single(second);
            Assert.Equal("unit-cache-0001", second[0].UnitId);
        }
        finally
        {
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task ManifestHydration_DuplicateUnitIdFailsWithoutWinner()
    {
        var root = Path.Combine(Path.GetTempPath(), $"f42-hydration-duplicate-{Guid.NewGuid():N}");
        WriteManifest(root, "a", BuildManifest("same-unit", Domain.PLAN.ToString(), "4.4", DateTimeOffset.UnixEpoch));
        WriteManifest(root, "b", BuildManifest("same-unit", Domain.PLAN.ToString(), "4.4", DateTimeOffset.UnixEpoch.AddSeconds(1)));
        var topology = BuildTopologyFor44();
        var flows = new InMemoryLogicalFlowRegistry();
        var units = new InMemoryLogicalUnitRegistry();
        try
        {
            var result = await new DefaultLogicalCapabilityTrainer(
                root,
                admissionAuthority: new RecordingLogicalCapabilityTrainingAdmissionAuthority()).TrainAndRegisterAsync(
                BuildTrainingRequest(
                    "duplicate-hydration",
                    "Fix a runtime issue.",
                    topology,
                    flows,
                    units,
                    Domain.BUG));

            Assert.False(result.IsSuccess);
            Assert.Equal("GENERATED_CAPABILITY_MANIFEST_DUPLICATE_UNIT_ID", result.ErrorCode);
            Assert.Empty(flows.ListDescriptors());
            Assert.Empty(units.ListDescriptors());
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ManifestHydration_DifferentDomainsSameUnitIdStillFailsGlobalIdentity()
    {
        var root = Path.Combine(Path.GetTempPath(), $"f42-hydration-global-{Guid.NewGuid():N}");
        WriteManifest(root, "bug", BuildManifest("same-global-unit", Domain.BUG.ToString(), "4.4", DateTimeOffset.UnixEpoch));
        WriteManifest(root, "plan", BuildManifest("same-global-unit", Domain.PLAN.ToString(), "4.4", DateTimeOffset.UnixEpoch.AddSeconds(1)));
        var topology = BuildTopologyFor44();
        try
        {
            var result = await new DefaultLogicalCapabilityTrainer(
                root,
                admissionAuthority: new RecordingLogicalCapabilityTrainingAdmissionAuthority()).TrainAndRegisterAsync(
                BuildTrainingRequest(
                    "global-duplicate",
                    "Fix a runtime issue.",
                    topology,
                    new InMemoryLogicalFlowRegistry(),
                    new InMemoryLogicalUnitRegistry(),
                    Domain.BUG));

            Assert.False(result.IsSuccess);
            Assert.Equal("GENERATED_CAPABILITY_MANIFEST_DUPLICATE_UNIT_ID", result.ErrorCode);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ManifestHydration_MalformedIsAtomicAndRetryableOnSameTrainer()
    {
        var root = Path.Combine(Path.GetTempPath(), $"f42-hydration-retry-{Guid.NewGuid():N}");
        WriteManifest(root, "valid", BuildManifest("unrelated-plan", Domain.PLAN.ToString(), "4.4", DateTimeOffset.UnixEpoch));
        var malformedDirectory = Path.Combine(root, "malformed");
        Directory.CreateDirectory(malformedDirectory);
        var malformedPath = Path.Combine(
            malformedDirectory,
            TeachingRegistryPersistence.GeneratedCapabilityManifestFileName);
        File.WriteAllText(malformedPath, "{ BAD SECRET");
        var topology = BuildTopologyFor44();
        var trainer = new DefaultLogicalCapabilityTrainer(
            root,
            admissionAuthority: new RecordingLogicalCapabilityTrainingAdmissionAuthority());
        try
        {
            var first = await trainer.TrainAndRegisterAsync(
                BuildTrainingRequest(
                    "retryable-first",
                    "Fix a runtime issue.",
                    topology,
                    new InMemoryLogicalFlowRegistry(),
                    new InMemoryLogicalUnitRegistry(),
                    Domain.BUG));
            Assert.False(first.IsSuccess);
            Assert.Equal("GENERATED_CAPABILITY_MANIFEST_INVALID", first.ErrorCode);

            File.WriteAllText(
                malformedPath,
                JsonSerializer.Serialize(BuildManifest(
                    "repaired-plan",
                    Domain.PLAN.ToString(),
                    "4.4",
                    DateTimeOffset.UnixEpoch.AddSeconds(1))));
            var second = await trainer.TrainAndRegisterAsync(
                BuildTrainingRequest(
                    "retryable-second",
                    "Fix a runtime issue.",
                    topology,
                    new InMemoryLogicalFlowRegistry(),
                    new InMemoryLogicalUnitRegistry(),
                    Domain.BUG));

            Assert.True(second.IsSuccess, second.ErrorMessage);
            Assert.True(second.Value!.CapabilityRegistered);
            Assert.Equal(3, InvokeLoadGeneratedCapabilityManifests(trainer).Length);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ManifestCache_ReadCountClearAndPostSaveHavePointInTimeSemantics()
    {
        var root = Path.Combine(Path.GetTempPath(), $"f42-cache-point-in-time-{Guid.NewGuid():N}");
        WriteManifest(root, "initial", BuildManifest("initial-plan", Domain.PLAN.ToString(), "4.4", DateTimeOffset.UnixEpoch));
        var trainer = new DefaultLogicalCapabilityTrainer(
            root,
            admissionAuthority: new RecordingLogicalCapabilityTrainingAdmissionAuthority());
        try
        {
            var first = InvokeLoadGeneratedCapabilityManifests(trainer);
            Assert.Single(first);

            var gateField = typeof(DefaultLogicalCapabilityTrainer).GetField(
                "_manifestCacheGate",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(gateField);
            var cacheGate = gateField.GetValue(trainer)!;
            var readerStarted = new ManualResetEventSlim(false);
            var counterStarted = new ManualResetEventSlim(false);
            var clearStarted = new ManualResetEventSlim(false);
            Task<GeneratedCapabilityCheckpointManifest[]> blockedReader;
            Task<AgentInfrastructure.OperationResult<int>> blockedCounter;
            Task blockedClear;
            Monitor.Enter(cacheGate);
            try
            {
                blockedReader = Task.Run(() =>
                {
                    readerStarted.Set();
                    return InvokeLoadGeneratedCapabilityManifests(trainer);
                });
                blockedCounter = Task.Run(() =>
                {
                    counterStarted.Set();
                    return InvokeCountGeneratedCapabilityManifestsResult(trainer);
                });
                blockedClear = Task.Run(() =>
                {
                    clearStarted.Set();
                    trainer.ClearCache();
                });
                Assert.True(readerStarted.Wait(TimeSpan.FromSeconds(5)));
                Assert.True(counterStarted.Wait(TimeSpan.FromSeconds(5)));
                Assert.True(clearStarted.Wait(TimeSpan.FromSeconds(5)));
                Thread.Sleep(100);
                Assert.False(blockedReader.IsCompleted);
                Assert.False(blockedCounter.IsCompleted);
                Assert.False(blockedClear.IsCompleted);
            }
            finally
            {
                Monitor.Exit(cacheGate);
            }

            Assert.Single(await blockedReader);
            var counted = await blockedCounter;
            Assert.True(counted.IsSuccess, counted.ErrorMessage);
            Assert.Equal(1, counted.Value);
            await blockedClear;

            trainer.ClearCache();
            File.Delete(Directory.GetFiles(
                root,
                TeachingRegistryPersistence.GeneratedCapabilityManifestFileName,
                SearchOption.AllDirectories).Single());
            Assert.Empty(InvokeLoadGeneratedCapabilityManifests(trainer));

            var topology = BuildTopologyFor44();
            var publish = await trainer.TrainAndRegisterAsync(
                BuildTrainingRequest(
                    "point-in-time-publish",
                    "Fix a runtime issue.",
                    topology,
                    new InMemoryLogicalFlowRegistry(),
                    new InMemoryLogicalUnitRegistry(),
                    Domain.BUG));
            Assert.True(publish.IsSuccess, publish.ErrorMessage);
            Assert.Single(InvokeLoadGeneratedCapabilityManifests(trainer));
            trainer.ClearCache();
            Assert.Single(InvokeLoadGeneratedCapabilityManifests(trainer));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void FindReplayManifests_PrivateArrayReflectionContractRemains()
    {
        var root = Path.Combine(Path.GetTempPath(), $"f42-replay-contract-{Guid.NewGuid():N}");
        WriteManifest(root, "bug", BuildManifest("bug-unit", Domain.BUG.ToString(), "4.4", DateTimeOffset.UnixEpoch));
        var trainer = new DefaultLogicalCapabilityTrainer(
            root,
            admissionAuthority: new RecordingLogicalCapabilityTrainingAdmissionAuthority());
        try
        {
            var method = typeof(DefaultLogicalCapabilityTrainer).GetMethod(
                "FindReplayManifests",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                binder: null,
                types: [typeof(string), typeof(string), typeof(bool)],
                modifiers: null);
            Assert.NotNull(method);
            Assert.Equal(typeof(GeneratedCapabilityCheckpointManifest[]), method.ReturnType);
            var result = method.Invoke(trainer, [Domain.BUG.ToString(), "compiled", false]);
            Assert.IsType<GeneratedCapabilityCheckpointManifest[]>(result);
            Assert.Single((GeneratedCapabilityCheckpointManifest[])result!);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void ManifestReaders_AreExactDomainScoped()
    {
        var root = Path.Combine(Path.GetTempPath(), $"f42-domain-scope-{Guid.NewGuid():N}");
        WriteManifest(root, "bug", BuildManifest("bug-unit", Domain.BUG.ToString(), "4.4", DateTimeOffset.UnixEpoch));
        WriteManifest(root, "plan", BuildManifest("plan-unit", Domain.PLAN.ToString(), "4.4", DateTimeOffset.UnixEpoch));
        var trainer = new DefaultLogicalCapabilityTrainer(
            root,
            admissionAuthority: new RecordingLogicalCapabilityTrainingAdmissionAuthority());
        try
        {
            Assert.Equal("bug-unit", InvokeFindReplayManifests(trainer, Domain.BUG.ToString()).Single().UnitId);
            Assert.Equal("plan-unit", InvokeFindReplayManifests(trainer, Domain.PLAN.ToString()).Single().UnitId);
            Assert.Equal(2, InvokeLoadGeneratedCapabilityManifests(trainer).Length);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Trainer_TwoAdapterDomainsSameSubphaseCreateDistinctBoundedFlowIds()
    {
        var root = Path.Combine(Path.GetTempPath(), $"f42-two-domains-{Guid.NewGuid():N}");
        var topology = BuildTopologyFor44();
        var flows = new InMemoryLogicalFlowRegistry();
        var units = new InMemoryLogicalUnitRegistry();
        var admissionAuthority =
            new RecordingLogicalCapabilityTrainingAdmissionAuthority();
        var trainer = new DefaultLogicalCapabilityTrainer(
            root,
            admissionAuthority: admissionAuthority);
        try
        {
            foreach (var domain in new[] { Domain.BUG, Domain.PLAN })
            {
                var result = await trainer.TrainAndRegisterAsync(
                    BuildTrainingRequest(
                        $"two-domain-{domain}",
                        "Implement a safe runtime change.",
                        topology,
                        flows,
                        units,
                        domain));
                Assert.True(result.IsSuccess, result.ErrorMessage);
            }

            var generatedFlows = flows.ListDescriptors()
                .Where(descriptor => descriptor.FlowId.StartsWith(
                    DefaultLogicalCapabilityTrainer.GeneratedCapabilityFlowIdPrefix + "-v2-",
                    StringComparison.Ordinal))
                .ToArray();
            Assert.Equal(2, generatedFlows.Length);
            Assert.Equal(2, generatedFlows.Select(flow => flow.FlowId).Distinct(StringComparer.Ordinal).Count());
            Assert.Equal(
                [
                    "generated-capability-flow-v2-0fef26dc0ab16f7416d3e2bbe2ed39e4f761f9cbeded78f3e76b9ef93ccdb5c9",
                    "generated-capability-flow-v2-cf68a3ccbee83ff74cb1efc128c9297efcba6b42c4550f50d8a9128ff7b7a659"
                ],
                generatedFlows
                    .Select(flow => flow.FlowId)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToArray());
            Assert.All(generatedFlows, flow =>
            {
                var suffix = flow.FlowId[(DefaultLogicalCapabilityTrainer.GeneratedCapabilityFlowIdPrefix.Length + 4)..];
                Assert.Equal(64, suffix.Length);
                Assert.All(suffix, ch => Assert.True(ch is >= '0' and <= '9' or >= 'a' and <= 'f'));
            });

            var snapshot = TeachingRegistryPersistence.Capture("two-domain", 1, units, flows, root);
            Assert.True(snapshot.IsSuccess, snapshot.ErrorMessage);
            Assert.Equal(
                generatedFlows.Select(flow => flow.FlowId).OrderBy(value => value, StringComparer.Ordinal),
                snapshot.Value!.GeneratedCapabilities.Select(manifest => manifest.FlowId).OrderBy(value => value, StringComparer.Ordinal));

            var restoredUnits = new InMemoryLogicalUnitRegistry();
            var restoredFlows = new InMemoryLogicalFlowRegistry();
            var replayVerification =
                LogicalCapabilityReplayVerificationCapability.Issue(
                    admissionAuthority);
            Assert.True(
                replayVerification.IsSuccess,
                replayVerification.ErrorMessage);
            var restore = TeachingRegistryPersistence.LoadIntoRegistries(
                snapshot.Value,
                topology,
                restoredUnits,
                restoredFlows,
                replayVerification.Value!);
            Assert.True(restore.IsSuccess, restore.ErrorMessage);
            foreach (var manifest in snapshot.Value.GeneratedCapabilities)
            {
                var binding = restoredFlows.ResolveFactoryBindingByFlowId(
                    manifest.FlowId,
                    manifest.ScenarioId,
                    new LogicalFlowFactoryRequest(
                        "two-domain-restore",
                        new LogicalExecutionFlags(),
                        restoredUnits,
                        new Dictionary<string, object>()));
                Assert.True(binding.IsSuccess, binding.ErrorMessage);
                var domainField = binding.Value!.Flow.GetType().GetField(
                    "_unitDomainTag",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                Assert.NotNull(domainField);
                Assert.Equal(manifest.DomainTag, domainField.GetValue(binding.Value.Flow));
            }
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Theory]
    [InlineData("tokenizer.json")]
    [InlineData("policy.bin")]
    [InlineData("policy.bin.tmp")]
    [InlineData(TeachingRegistryPersistence.GeneratedCapabilityManifestFileName)]
    public async Task Publication_SanitizeCollisionPreservesAllExistingArtifacts(string artifactName)
    {
        var root = Path.Combine(Path.GetTempPath(), $"f42-publication-collision-{Guid.NewGuid():N}");
        // A prior raw id "sanitize-a" owns the same sanitized destination that
        // the distinct incoming raw id "sanitize/a" would otherwise reuse.
        var directory = ExpectedPublicationDirectory(root, "sanitize-a", Domain.BUG.ToString(), "4.4");
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, artifactName);
        var sentinel = Encoding.UTF8.GetBytes($"sentinel-{artifactName}");
        File.WriteAllBytes(path, sentinel);
        try
        {
            var result = await new DefaultLogicalCapabilityTrainer(
                root,
                admissionAuthority: new RecordingLogicalCapabilityTrainingAdmissionAuthority()).TrainAndRegisterAsync(
                BuildTrainingRequest(
                    "sanitize/a",
                    "Fix a runtime issue.",
                    BuildTopologyFor44(),
                    new InMemoryLogicalFlowRegistry(),
                    new InMemoryLogicalUnitRegistry(),
                    Domain.BUG));

            Assert.False(result.IsSuccess);
            Assert.Equal("GENERATED_CAPABILITY_ARTIFACT_ALREADY_EXISTS", result.ErrorCode);
            Assert.Equal(sentinel, File.ReadAllBytes(path));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Publication_ExistingPolicyTempFailsBeforeTokenizerWrite()
    {
        var root = Path.Combine(Path.GetTempPath(), $"f42-policy-temp-{Guid.NewGuid():N}");
        var directory = ExpectedPublicationDirectory(root, "policy-temp", Domain.BUG.ToString(), "4.4");
        Directory.CreateDirectory(directory);
        var temporaryPath = Path.Combine(directory, "policy.bin.tmp");
        File.WriteAllText(temporaryPath, "sentinel-policy-temp");
        try
        {
            var result = await new DefaultLogicalCapabilityTrainer(
                root,
                admissionAuthority: new RecordingLogicalCapabilityTrainingAdmissionAuthority()).TrainAndRegisterAsync(
                BuildTrainingRequest(
                    "policy-temp",
                    "Fix a runtime issue.",
                    BuildTopologyFor44(),
                    new InMemoryLogicalFlowRegistry(),
                    new InMemoryLogicalUnitRegistry(),
                    Domain.BUG));

            Assert.False(result.IsSuccess);
            Assert.Equal("GENERATED_CAPABILITY_ARTIFACT_ALREADY_EXISTS", result.ErrorCode);
            Assert.False(File.Exists(Path.Combine(directory, "tokenizer.json")));
            Assert.False(File.Exists(Path.Combine(directory, "policy.bin")));
            Assert.False(File.Exists(Path.Combine(
                directory,
                TeachingRegistryPersistence.GeneratedCapabilityManifestFileName)));
            Assert.Equal("sentinel-policy-temp", File.ReadAllText(temporaryPath));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Publication_ReservationCleanupNeverDeletesUnownedEntry()
    {
        var root = Path.Combine(Path.GetTempPath(), $"f42-reservation-owned-{Guid.NewGuid():N}");
        var directory = ExpectedPublicationDirectory(root, "reservation-owned", Domain.BUG.ToString(), "4.4");
        Directory.CreateDirectory(directory);
        var reservationPath = Path.Combine(directory, ".generated-capability.destination.reservation");
        var sentinel = Encoding.UTF8.GetBytes("sentinel-reservation");
        File.WriteAllBytes(reservationPath, sentinel);
        try
        {
            var result = await new DefaultLogicalCapabilityTrainer(
                root,
                admissionAuthority: new RecordingLogicalCapabilityTrainingAdmissionAuthority()).TrainAndRegisterAsync(
                BuildTrainingRequest(
                    "reservation-owned",
                    "Fix a runtime issue.",
                    BuildTopologyFor44(),
                    new InMemoryLogicalFlowRegistry(),
                    new InMemoryLogicalUnitRegistry(),
                    Domain.BUG));

            Assert.False(result.IsSuccess);
            Assert.Equal("GENERATED_CAPABILITY_DESTINATION_BUSY", result.ErrorCode);
            Assert.Equal(sentinel, File.ReadAllBytes(reservationPath));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void ManifestFailures_DoNotCatchFatalExceptionGraph()
    {
        Assert.False(SnapshotExceptionGraphClassifier.IsNonFatal(
            new AggregateException(new InvalidOperationException("outer", new OutOfMemoryException()))));
        Assert.False(SnapshotExceptionGraphClassifier.IsNonFatal(new OperationCanceledException()));
        Assert.True(SnapshotExceptionGraphClassifier.IsNonFatal(new IOException("ordinary I/O")));
    }

    [Fact]
    public async Task Publication_TwoChildProcessesOneReservationOnePublisher()
    {
        if (string.Equals(Environment.GetEnvironmentVariable("XIIGEN_F42_CHILD"), "1", StringComparison.Ordinal))
        {
            await RunPublicationChildAsync();
            return;
        }

        // Монотонные часы родительского участника: от них отсчитываются все его этапы.
        var parentClock = Stopwatch.StartNew();
        var root = Path.Combine(Path.GetTempPath(), $"f42-child-publication-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        var goPath = Path.Combine(root, "go");
        var children = new List<PublicationChildProcess>();

        // Срок готовности выбран по измерению, а не наугад.
        // Наблюдение базового прогона 30.10.2026: при занятой машине дочерний процесс входит
        // в свой тест примерно через 85–90 секунд после запуска, поэтому прежнее окно
        // в 30 секунд истекало раньше, чем дочерний процесс вообще начинал работать.
        // Наблюдение красного прогона того же дня: при свободной машине те же дочерние
        // процессы укладывались в 30 секунд. Окно 30 секунд пограничное, отсюда запас.
        // Это срок на старт, а не бюджет обучения: ожидание сигнала «начинай» и число
        // вызовов обучения остаются прежними.
        var readinessDeadline = TimeSpan.FromSeconds(240);

        // Срок на завершение дочернего процесса после сигнала «начинай».
        // Покрывает сам вызов обучения и остановку системы тестов дочернего процесса.
        var childExitDeadline = TimeSpan.FromSeconds(180);
        try
        {
            for (var index = 0; index < 2; index++)
            {
                var childId = $"child-{index}";
                var ready = Path.Combine(root, $"{childId}.ready");
                var result = Path.Combine(root, $"{childId}.result");
                children.Add(StartPublicationChild(root, childId, ready, goPath, result));
                RecordPublicationStage(
                    "parent",
                    "child_process_started",
                    parentClock,
                    $"child={childId}; pid={children[^1].Process.Id}");
            }

            // Этап готовности: ожидание по одному дескриптору на дочерний процесс.
            var readiness = await WaitForPublicationReadinessAsync(children, readinessDeadline);
            RecordPublicationStage(
                "parent",
                "readiness_wait_finished",
                parentClock,
                $"all_ready={readiness.AllReady}; child_exited_early={readiness.ChildExitedEarly}; " +
                $"waited_ms={(long)readiness.Elapsed.TotalMilliseconds}; " +
                $"deadline_ms={(long)readinessDeadline.TotalMilliseconds}");
            if (!readiness.AllReady)
            {
                // Сначала ограниченно завершить дочерние процессы, и только потом собирать
                // их вывод. В обратном порядке отказ родителя откладывается до момента,
                // когда дочерний процесс сам решит завершиться и закрыть свой поток.
                await TerminatePublicationChildrenAsync(children, TimeSpan.FromSeconds(30));
                var diagnostics = await Task.WhenAll(children.Select(child =>
                    DescribePublicationChildAsync(child, TimeSpan.FromSeconds(15))));
                Assert.Fail(
                    "Дочерние участники публикации не отметили готовность за " +
                    $"{readinessDeadline.TotalSeconds:F0} с " +
                    $"(фактическое ожидание {readiness.Elapsed.TotalSeconds:F1} с; " +
                    $"ранний выход дочернего процесса: {(readiness.ChildExitedEarly ? "да" : "нет")}). " +
                    string.Join(" | ", diagnostics));
            }

            File.WriteAllText(goPath, "go");
            RecordPublicationStage("parent", "go_written", parentClock, $"go={goPath}");

            foreach (var child in children)
            {
                using var timeout = new CancellationTokenSource(childExitDeadline);
                try
                {
                    await child.Process.WaitForExitAsync(timeout.Token);
                }
                catch (OperationCanceledException)
                {
                    child.Process.Kill(entireProcessTree: true);
                    throw;
                }

                RecordPublicationStage(
                    "parent",
                    "child_process_exited",
                    parentClock,
                    $"pid={child.Process.Id}; exit={DescribeChildExitState(child)}");

                // Сохранённый результат читается до проверки кода выхода, чтобы сообщение
                // об отказе сразу называло этап, на котором дочерний участник остановился.
                var savedResult = File.Exists(child.ResultPath)
                    ? File.ReadAllText(child.ResultPath).Trim()
                    : "(файл результата не создан)";
                Assert.True(
                    child.Process.ExitCode == 0,
                    $"Дочерний тест обязан завершиться успехом. Код выхода: {child.Process.ExitCode}; " +
                    $"сохранённый результат: {savedResult}. Таймаут сигнала «начинай» не " +
                    "засчитывается ни публикацией, ни проигрышем резервирования.");
            }

            var results = children.Select(child => File.ReadAllText(child.ResultPath).Trim()).ToArray();
            var childDiagnostics = await Task.WhenAll(children.Select(child =>
                DescribePublicationChildAsync(child, TimeSpan.FromSeconds(15))));
            var completedDiagnostics = string.Join(" | ", childDiagnostics);
            Assert.True(
                results.Count(value => value == "PUBLISHED") == 1,
                $"Expected exactly one publisher. {completedDiagnostics}");
            Assert.True(
                results.Count(value => value.StartsWith(
                    "GENERATED_CAPABILITY_DESTINATION_BUSY",
                    StringComparison.Ordinal) || value.StartsWith(
                    "GENERATED_CAPABILITY_ARTIFACT_ALREADY_EXISTS",
                    StringComparison.Ordinal) || value.StartsWith(
                    "GENERATED_CAPABILITY_MANIFEST_ALREADY_EXISTS",
                    StringComparison.Ordinal)) == 1,
                $"Expected exactly one fail-closed reservation loser. {completedDiagnostics}");
            var directory = ExpectedPublicationDirectory(root, "child-publication", Domain.BUG.ToString(), "4.4");
            Assert.True(File.Exists(Path.Combine(directory, "tokenizer.json")));
            Assert.True(File.Exists(Path.Combine(directory, "policy.bin")));
            Assert.False(File.Exists(Path.Combine(directory, "policy.bin.tmp")));
            var manifestPath = Path.Combine(directory, TeachingRegistryPersistence.GeneratedCapabilityManifestFileName);
            Assert.True(File.Exists(manifestPath));
            Assert.NotNull(JsonSerializer.Deserialize<GeneratedCapabilityCheckpointManifest>(
                File.ReadAllText(manifestPath)));
            RecordPublicationStage(
                "parent",
                "parent_assertions_passed",
                parentClock,
                $"results={string.Join(",", results)}");
        }
        finally
        {
            // Сначала убедиться, что ни один дочерний процесс больше не живёт и не пишет
            // в общий каталог, и только потом удалять каталог. В обратном порядке удаление
            // идёт наперегонки с записью дочернего процесса.
            await TerminatePublicationChildrenAsync(children, TimeSpan.FromSeconds(30));
            foreach (var child in children)
            {
                child.Process.Dispose();
            }

            RecordPublicationStage("parent", "cleanup_started", parentClock, $"root={root}");
            if (Directory.Exists(root))
            {
                try
                {
                    Directory.Delete(root, recursive: true);
                }
                catch (IOException)
                {
                    // Каталог ещё занят. Уборка временного каталога не является предметом
                    // этой проверки, поэтому её сбой не подменяет настоящий результат теста.
                }
                catch (UnauthorizedAccessException)
                {
                    // То же самое для отказа в доступе при удалении.
                }
            }
        }
    }

    private static InMemoryLogicalTopologyRegistry BuildTopologyFor44()
    {
        var topology = new InMemoryLogicalTopologyRegistry();
        var result = topology.AddOrUpdateSubPhase(new LogicalSubPhase(
            "4.4",
            "4",
            "Refactoring and Code Quality",
            "Owns internal code improvement, simplification, dependency cleanup, and maintainability.",
            ["software-development"],
            ["IMPLEMENT"],
            ["f42-test"]));
        Assert.True(result.IsSuccess, result.ErrorMessage);
        return topology;
    }

    private static void WriteManifest(
        string root,
        string child,
        GeneratedCapabilityCheckpointManifest manifest)
    {
        var directory = Path.Combine(root, child);
        Directory.CreateDirectory(directory);
        File.WriteAllText(
            Path.Combine(directory, TeachingRegistryPersistence.GeneratedCapabilityManifestFileName),
            JsonSerializer.Serialize(manifest));
    }

    private static GeneratedCapabilityCheckpointManifest[] InvokeFindReplayManifests(
        DefaultLogicalCapabilityTrainer trainer,
        string domainTag)
    {
        var method = typeof(DefaultLogicalCapabilityTrainer).GetMethod(
            "FindReplayManifests",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
            binder: null,
            types: [typeof(string), typeof(string), typeof(bool)],
            modifiers: null);
        Assert.NotNull(method);
        return (GeneratedCapabilityCheckpointManifest[])method.Invoke(
            trainer,
            [domainTag, "compiled", false])!;
    }

    private static AgentInfrastructure.OperationResult<int> InvokeCountGeneratedCapabilityManifestsResult(
        DefaultLogicalCapabilityTrainer trainer)
    {
        var method = typeof(DefaultLogicalCapabilityTrainer).GetMethod(
            "CountGeneratedCapabilityManifestsResult",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(method);
        return (AgentInfrastructure.OperationResult<int>)method.Invoke(trainer, null)!;
    }

    private static string ExpectedPublicationDirectory(
        string root,
        string runId,
        string domainTag,
        string subPhaseId)
    {
        return Path.Combine(
            root,
            SanitizeForTest(runId),
            SanitizeForTest(domainTag),
            SanitizeForTest(subPhaseId));
    }

    private static int ReadPrivateDictionaryCount(
        DefaultLogicalCapabilityTrainer trainer,
        string fieldName)
    {
        var field = typeof(DefaultLogicalCapabilityTrainer).GetField(
            fieldName,
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic);
        Assert.NotNull(field);
        var dictionary = Assert.IsAssignableFrom<System.Collections.IDictionary>(
            field.GetValue(trainer));
        return dictionary.Count;
    }

    private static ServerOwnedLegalTrainingTeacherAdmissionAuthority
        CreateRevocableCapabilityAuthority()
    {
        var configuration = ContinuousLearningTp1DpoConfiguration.Create(
            "qwen3.5:9b",
            new string('d', 64),
            new string('c', 64),
            LegalTrainingTeacherAdmissionAuthority.MaxBatchRows,
            $"sha256:{new string('a', 64)}",
            $"sha256:{new string('b', 64)}",
            Enumerable.Range(1, 32)
                .Select(static value => (byte)value)
                .ToArray(),
            dpoMaxStepCount: 1);
        Assert.True(configuration.IsSuccess, configuration.ErrorMessage);
        var authority = ServerOwnedLegalTrainingTeacherAdmissionAuthority
            .IssueFromAdmittedConfiguration(
                configuration.Value!,
                static () => true);
        Assert.True(authority.IsAvailable);
        return authority;
    }

    private static string SanitizeForTest(string value)
    {
        var chars = value.Trim()
            .Select(ch => char.IsLetterOrDigit(ch) ? char.ToLowerInvariant(ch) : '-')
            .ToArray();
        return string.Join("-", new string(chars).Split('-', StringSplitOptions.RemoveEmptyEntries));
    }

    private static PublicationChildProcess StartPublicationChild(
        string root,
        string childId,
        string readyPath,
        string goPath,
        string resultPath)
    {
        var dotnetHost = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH");
        if (string.IsNullOrWhiteSpace(dotnetHost) || !File.Exists(dotnetHost))
            dotnetHost = OperatingSystem.IsWindows() ? "dotnet.exe" : "dotnet";
        var projectPath = FindTestProjectPath();
        var repositoryRoot = Directory.GetParent(Path.GetDirectoryName(projectPath)!)!.FullName;
        var startInfo = new ProcessStartInfo(dotnetHost)
        {
            WorkingDirectory = repositoryRoot,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        startInfo.ArgumentList.Add("test");
        startInfo.ArgumentList.Add(projectPath);
        startInfo.ArgumentList.Add("--configuration");
        startInfo.ArgumentList.Add(InferCurrentTestConfiguration());
        startInfo.ArgumentList.Add("--no-build");
        startInfo.ArgumentList.Add("--no-restore");
        startInfo.ArgumentList.Add("--filter");
        startInfo.ArgumentList.Add("FullyQualifiedName~Publication_TwoChildProcessesOneReservationOnePublisher");
        startInfo.ArgumentList.Add("--verbosity");
        startInfo.ArgumentList.Add("quiet");
        startInfo.Environment["XIIGEN_F42_CHILD"] = "1";
        startInfo.Environment["XIIGEN_F42_CHILD_ID"] = childId;
        startInfo.Environment["XIIGEN_F42_ROOT"] = root;
        startInfo.Environment["XIIGEN_F42_READY"] = readyPath;
        startInfo.Environment["XIIGEN_F42_GO"] = goPath;
        startInfo.Environment["XIIGEN_F42_RESULT"] = resultPath;
        var process = Process.Start(startInfo);
        Assert.NotNull(process);
        return new PublicationChildProcess(
            process,
            readyPath,
            resultPath,
            process.StandardOutput.ReadToEndAsync(),
            process.StandardError.ReadToEndAsync());
    }

    private static string FindTestProjectPath()
    {
        foreach (var startingPath in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            var current = new DirectoryInfo(startingPath);
            while (current is not null)
            {
                var nested = Path.Combine(current.FullName, "Tests", "LlmFromScratch.Tests.csproj");
                if (File.Exists(nested))
                    return nested;
                var direct = Path.Combine(current.FullName, "LlmFromScratch.Tests.csproj");
                if (File.Exists(direct))
                    return direct;
                current = current.Parent;
            }
        }

        throw new InvalidOperationException("Could not locate the test project for the child publication host.");
    }

    private static string InferCurrentTestConfiguration()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory.TrimEnd(
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar));
        while (current is not null)
        {
            if (string.Equals(current.Name, "Debug", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(current.Name, "Release", StringComparison.OrdinalIgnoreCase))
                return current.Name;

            current = current.Parent;
        }

        throw new InvalidOperationException("Could not infer the current test configuration for the child publication host.");
    }

    private static async Task RunPublicationChildAsync()
    {
        // Монотонные часы дочернего участника: от них отсчитываются все его этапы.
        var childClock = Stopwatch.StartNew();
        var childId = Environment.GetEnvironmentVariable("XIIGEN_F42_CHILD_ID") ?? "child";
        var root = Environment.GetEnvironmentVariable("XIIGEN_F42_ROOT")!;
        var readyPath = Environment.GetEnvironmentVariable("XIIGEN_F42_READY")!;
        var goPath = Environment.GetEnvironmentVariable("XIIGEN_F42_GO")!;
        var resultPath = Environment.GetEnvironmentVariable("XIIGEN_F42_RESULT")!;

        // Этап «вход в дочерний тест»: всё, что было до него, — это старт процесса
        // и запуск системы тестов, то есть расход, который родитель измеряет со своей стороны.
        RecordPublicationStage(childId, "child_entered", childClock, $"root={root}");

        File.WriteAllText(readyPath, childId);
        RecordPublicationStage(childId, "ready_written", childClock, $"ready={readyPath}");

        // Срок ожидания сигнала «начинай» остаётся прежним: бюджет самого обучения не трогается.
        var goTimeout = TimeSpan.FromSeconds(30);
        var goClock = Stopwatch.StartNew();
        var goObserved = await WaitForFilesAsync([goPath], goTimeout);
        goClock.Stop();
        if (!goObserved)
        {
            // Диагностический результат по-прежнему сохраняется на диск: он нужен родителю.
            File.WriteAllText(resultPath, "GO_TIMEOUT");
            RecordPublicationStage(
                childId,
                "go_timeout",
                childClock,
                $"waited_ms={goClock.ElapsedMilliseconds}; trainer_called=false");

            // И дочерний тест завершается отказом. Обычный возврат здесь означал бы успех
            // для системы тестов, то есть зелёную галочку без единого вызова обучения.
            // Таймаут не засчитывается ни победителем, ни проигравшим резервирование.
            Assert.Fail(
                $"GO_TIMEOUT: дочерний участник {childId} не дождался сигнала «начинай» за " +
                $"{goTimeout.TotalSeconds:F0} с (фактическое ожидание {goClock.Elapsed.TotalSeconds:F1} с). " +
                "Обучение не вызывалось, поэтому результат не является ни публикацией, " +
                "ни проигрышем резервирования.");
        }

        RecordPublicationStage(
            childId,
            "go_observed",
            childClock,
            $"waited_ms={goClock.ElapsedMilliseconds}");

        // Этап вызова обучения. Запись делается до вызова, чтобы число фактических
        // вызовов обучения можно было пересчитать по журналу, а не со слов отчёта.
        var trainerClock = Stopwatch.StartNew();
        RecordPublicationStage(childId, "trainer_invoked", childClock, "call=TrainAndRegisterAsync");
        var result = await new DefaultLogicalCapabilityTrainer(
            root,
            admissionAuthority: new RecordingLogicalCapabilityTrainingAdmissionAuthority()).TrainAndRegisterAsync(
            BuildTrainingRequest(
                "child-publication",
                "Fix a runtime issue.",
                BuildTopologyFor44(),
                new InMemoryLogicalFlowRegistry(),
                new InMemoryLogicalUnitRegistry(),
                Domain.BUG));
        trainerClock.Stop();
        RecordPublicationStage(
            childId,
            "trainer_returned",
            childClock,
            $"duration_ms={trainerClock.ElapsedMilliseconds}; success={result.IsSuccess}; " +
            $"error={result.ErrorCode ?? "(нет)"}");

        var savedResult = result.IsSuccess && result.Value!.CapabilityRegistered
            ? "PUBLISHED"
            : $"{result.ErrorCode}|{result.ErrorMessage}";
        File.WriteAllText(resultPath, savedResult);
        RecordPublicationStage(childId, "result_written", childClock, $"result={savedResult}");
    }

    private static async Task<bool> WaitForFilesAsync(
        IEnumerable<string> paths,
        TimeSpan timeout)
    {
        var required = paths.ToArray();
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < timeout)
        {
            if (required.All(File.Exists))
                return true;
            await Task.Delay(50);
        }

        return required.All(File.Exists);
    }

    // Итог ожидания готовности дочерних процессов: дождались ли отметки готовности,
    // не вышел ли при этом какой-то дочерний процесс раньше срока, и сколько ожидание заняло
    // по монотонным часам.
    private sealed record PublicationReadinessOutcome(
        bool AllReady,
        bool ChildExitedEarly,
        TimeSpan Elapsed);

    // Ожидание готовности дочерних процессов по одному дескриптору на процесс
    // и с ограниченным сроком. Ожидание прекращается по любой из трёх причин:
    // все отметились; какой-то дочерний процесс умер, так и не отметившись; истёк срок.
    private static async Task<PublicationReadinessOutcome> WaitForPublicationReadinessAsync(
        IReadOnlyList<PublicationChildProcess> children,
        TimeSpan timeout)
    {
        // Монотонные часы: по ним измеряется фактическая длительность этапа ожидания.
        var clock = Stopwatch.StartNew();
        while (clock.Elapsed < timeout)
        {
            // Готовность достигнута: каждый дочерний процесс оставил свою отметку.
            if (children.All(child => File.Exists(child.ReadyPath)))
            {
                clock.Stop();
                return new PublicationReadinessOutcome(true, false, clock.Elapsed);
            }

            // Ранний выход: дескриптор процесса говорит, что он уже мёртв, а отметки нет.
            // Ждать дальше бессмысленно, поэтому ожидание прекращается немедленно,
            // не вырабатывая остаток срока над мёртвым процессом.
            if (children.Any(child => child.Process.HasExited && !File.Exists(child.ReadyPath)))
            {
                clock.Stop();
                return new PublicationReadinessOutcome(false, true, clock.Elapsed);
            }

            await Task.Delay(50);
        }

        clock.Stop();
        // Срок истёк: отдельно фиксируется, достигнута ли готовность в последний момент
        // и был ли среди причин ранний выход дочернего процесса.
        var allReady = children.All(child => File.Exists(child.ReadyPath));
        var exitedEarly = !allReady && children.Any(child => child.Process.HasExited);
        return new PublicationReadinessOutcome(allReady, exitedEarly, clock.Elapsed);
    }

    // Состояние дочернего процесса для диагностики.
    // Живой процесс обозначается словом «работает», а не выдуманным кодом выхода:
    // число минус один здесь было бы ложным фактом о процессе, который ещё не завершился.
    private static string DescribeChildExitState(PublicationChildProcess child)
    {
        return child.Process.HasExited
            ? child.Process.ExitCode.ToString(System.Globalization.CultureInfo.InvariantCulture)
            : "RUNNING";
    }

    // Запись одного измеренного этапа в долговечный журнал этапов.
    // Папка журнала задаётся переменной окружения XIIGEN_F42_STAGE_DIR и наследуется
    // дочерними процессами. Если переменной нет, запись просто пропускается и поведение
    // самой проверки не меняется: журнал этапов — вспомогательное свидетельство.
    private static void RecordPublicationStage(
        string actor,
        string stage,
        Stopwatch clock,
        string note)
    {
        var directory = Environment.GetEnvironmentVariable("XIIGEN_F42_STAGE_DIR");
        if (string.IsNullOrWhiteSpace(directory))
            return;
        try
        {
            Directory.CreateDirectory(directory);
            // Строка этапа: момент по часам машины, участник, номер процесса, название этапа,
            // монотонная длительность от начала работы участника и человеческое примечание.
            var line = string.Join(
                '\t',
                DateTime.UtcNow.ToString("O", System.Globalization.CultureInfo.InvariantCulture),
                $"actor={SanitizeForTest(actor)}",
                $"pid={Environment.ProcessId}",
                $"stage={stage}",
                $"elapsed_ms={clock.ElapsedMilliseconds}",
                note);
            File.AppendAllText(
                Path.Combine(directory, $"stages-{SanitizeForTest(actor)}-{Environment.ProcessId}.tsv"),
                line + Environment.NewLine);
        }
        catch (IOException)
        {
            // Сбой записи вспомогательного журнала не имеет права подменить результат проверки.
        }
        catch (UnauthorizedAccessException)
        {
            // То же самое для отказа в доступе к папке журнала.
        }
    }

    // Чтение уже начатого потока вывода с собственным сроком.
    // Без срока это ожидание завершается только когда дочерний процесс закроет поток,
    // то есть откладывает отказ родителя до самостоятельного завершения дочернего процесса.
    private static async Task<string> ReadOutputWithDeadlineAsync(
        Task<string> reader,
        TimeSpan deadline)
    {
        var finished = await Task.WhenAny(reader, Task.Delay(deadline));
        if (!ReferenceEquals(finished, reader))
            return $"(вывод не собран за {deadline.TotalSeconds:F0} с: процесс ещё держит поток)";
        return await reader;
    }

    // Человеческое описание дочернего процесса для диагностики: состояние, сохранённый
    // результат и доступный на данный момент вывод. Каждый шаг ограничен своим сроком.
    private static async Task<string> DescribePublicationChildAsync(
        PublicationChildProcess child,
        TimeSpan deadline)
    {
        var state = DescribeChildExitState(child);
        var savedResult = File.Exists(child.ResultPath)
            ? File.ReadAllText(child.ResultPath).Trim()
            : "(файл результата не создан)";
        var standardOutput = await ReadOutputWithDeadlineAsync(child.StandardOutput, deadline);
        var standardError = await ReadOutputWithDeadlineAsync(child.StandardError, deadline);
        return $"exit={state}; result={savedResult}; stdout={standardOutput}; stderr={standardError}";
    }

    // Ограниченное по сроку завершение дочерних процессов.
    // Выполняется до сбора вывода: сначала процесс должен перестать жить и писать,
    // и только потом собирается то, что он успел выдать.
    private static async Task TerminatePublicationChildrenAsync(
        IReadOnlyList<PublicationChildProcess> children,
        TimeSpan deadline)
    {
        foreach (var child in children)
        {
            if (child.Process.HasExited)
                continue;
            try
            {
                child.Process.Kill(entireProcessTree: true);
            }
            catch (InvalidOperationException)
            {
                // Процесс уже завершился между проверкой и снятием: отдельного действия не нужно.
            }
        }

        using var cancellation = new CancellationTokenSource(deadline);
        foreach (var child in children)
        {
            try
            {
                await child.Process.WaitForExitAsync(cancellation.Token);
            }
            catch (OperationCanceledException)
            {
                // Срок на завершение исчерпан. Это записывается в диагностику состоянием
                // процесса, а не бесконечным ожиданием.
                break;
            }
            catch (InvalidOperationException)
            {
            }
        }
    }

    // Имена переменных окружения, которыми родитель настраивает дочерний режим проверки.
    private static readonly string[] PublicationChildEnvironmentNames =
    [
        "XIIGEN_F42_CHILD_ID",
        "XIIGEN_F42_ROOT",
        "XIIGEN_F42_READY",
        "XIIGEN_F42_GO",
        "XIIGEN_F42_RESULT"
    ];

    // Снимок переменных окружения дочернего режима: контроль протокола обязан вернуть
    // окружение в прежний вид, чтобы не повлиять на соседние проверки этого же класса.
    private static Dictionary<string, string?> CapturePublicationChildEnvironment()
    {
        var snapshot = new Dictionary<string, string?>(StringComparer.Ordinal);
        foreach (var name in PublicationChildEnvironmentNames)
            snapshot[name] = Environment.GetEnvironmentVariable(name);
        return snapshot;
    }

    // Настройка окружения дочернего режима для контроля протокола, выполняемого в этом же процессе.
    private static void ApplyPublicationChildEnvironment(
        string childId,
        string root,
        string readyPath,
        string goPath,
        string resultPath)
    {
        Environment.SetEnvironmentVariable("XIIGEN_F42_CHILD_ID", childId);
        Environment.SetEnvironmentVariable("XIIGEN_F42_ROOT", root);
        Environment.SetEnvironmentVariable("XIIGEN_F42_READY", readyPath);
        Environment.SetEnvironmentVariable("XIIGEN_F42_GO", goPath);
        Environment.SetEnvironmentVariable("XIIGEN_F42_RESULT", resultPath);
    }

    // Восстановление снятого ранее окружения дочернего режима.
    private static void RestorePublicationChildEnvironment(Dictionary<string, string?> snapshot)
    {
        foreach (var pair in snapshot)
            Environment.SetEnvironmentVariable(pair.Key, pair.Value);
    }

    [Fact]
    public async Task PublicationProtocolControl_MissingGoSignalFailsChildTest()
    {
        // Отрицательный контроль протокола: сигнал «начинай» не приходит никогда.
        // Дочерняя процедура обязана завершиться отказом, а не успешным возвратом.
        // Обычный возврат здесь означает для системы тестов успех, то есть зелёную галочку
        // без единого вызова обучения. Обучение этот контроль не вызывает.
        var root = Path.Combine(Path.GetTempPath(), $"f42-control-no-go-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        var readyPath = Path.Combine(root, "control.ready");
        // Файл сигнала «начинай» намеренно не создаётся ни разу за весь контроль.
        var goPath = Path.Combine(root, "go");
        var resultPath = Path.Combine(root, "control.result");
        var savedEnvironment = CapturePublicationChildEnvironment();
        try
        {
            ApplyPublicationChildEnvironment("control-no-go", root, readyPath, goPath, resultPath);

            // Ожидается именно отказ. Если отказа нет, контроль падает сам — и это правильно:
            // значит дефект ложной зелёной галочки на месте.
            var failure = await Assert.ThrowsAnyAsync<Exception>(RunPublicationChildAsync);

            Assert.False(File.Exists(goPath), "Сигнал «начинай» в этом контроле не создаётся.");
            Assert.True(
                File.Exists(readyPath),
                "Дочерняя процедура обязана отметить готовность до ожидания сигнала «начинай».");
            Assert.True(
                File.Exists(resultPath),
                "Диагностический результат обязан сохраниться на диск, а не исчезнуть вместе с отказом.");
            Assert.StartsWith("GO_TIMEOUT", File.ReadAllText(resultPath).Trim(), StringComparison.Ordinal);
            Assert.Contains("GO_TIMEOUT", failure.Message, StringComparison.Ordinal);
        }
        finally
        {
            RestorePublicationChildEnvironment(savedEnvironment);
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task PublicationProtocolControl_EarlyChildExitStopsReadinessWait()
    {
        // Отрицательный контроль протокола: дочерний процесс умирает до отметки готовности.
        // Ожидание готовности обязано прекратиться по факту смерти процесса, а не вырабатывать
        // весь отведённый срок; живой процесс не обозначается выдуманным кодом выхода.
        // Обучение этот контроль не вызывает.
        var root = Path.Combine(Path.GetTempPath(), $"f42-control-early-exit-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        var readyPath = Path.Combine(root, "control.ready");
        var goPath = Path.Combine(root, "go");
        var resultPath = Path.Combine(root, "control.result");
        // Срок заведомо велик: смысл контроля в том, что ожидание обязано прекратиться
        // задолго до него, по наблюдению за дескриптором процесса.
        var deadline = TimeSpan.FromSeconds(40);
        PublicationChildProcess? child = null;
        try
        {
            // Этап 1: дочерний процесс запущен и в этот момент заведомо жив.
            child = StartPublicationChild(root, "control-early-exit", readyPath, goPath, resultPath);
            var aliveState = DescribeChildExitState(child);

            // Этап 2: ранний выход — процесс снимается до того, как успел отметить готовность.
            child.Process.Kill(entireProcessTree: true);
            await child.Process.WaitForExitAsync();
            Assert.False(
                File.Exists(readyPath),
                "Дочерний процесс снят до отметки готовности, файла готовности быть не должно.");

            // Этап 3: ожидание готовности над мёртвым процессом.
            var readiness = await WaitForPublicationReadinessAsync([child], deadline);
            Assert.False(readiness.AllReady, "Готовности быть не может: дочерний процесс мёртв.");
            Assert.True(
                readiness.ChildExitedEarly,
                "Ожидание обязано заметить ранний выход дочернего процесса, а не ждать вслепую.");
            Assert.True(
                readiness.Elapsed < TimeSpan.FromSeconds(10),
                "Ожидание обязано прекратиться сразу после смерти процесса, а не вырабатывать весь срок. " +
                $"Факт: {readiness.Elapsed.TotalSeconds:F1} с из {deadline.TotalSeconds:F0} с.");

            // Этап 4: пока процесс был жив, диагностика не имела права называть числовой код выхода.
            Assert.Equal("RUNNING", aliveState);
        }
        finally
        {
            child?.Process.Dispose();
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    private static LogicalCapabilityTrainingRequest BuildTrainingRequest(
        string runId,
        string userRequest,
        InMemoryLogicalTopologyRegistry topology,
        InMemoryLogicalFlowRegistry flows,
        InMemoryLogicalUnitRegistry units,
        Domain? exactDomain = null)
    {
        var summary = new LogicalFlowSummary(
            $"summary-{runId}",
            1,
            0,
            0,
            "knowledge_edge",
            false,
            false,
            false,
            "GENERATE_CAPABILITY",
            0.70,
            "missing_sub_phase=4.4",
            "Capability is missing for routed sub-phase.",
            0,
            0);

        var branch = new LogicalFlowResult(
            $"missing-flow-{runId}",
            "missing-capability",
            "KNOWLEDGE_EDGE",
            "knowledge_edge: missing generated capability",
            summary,
            new Dictionary<string, string> { ["missing_sub_phase"] = "4.4" },
            [],
            []);
        var domain = exactDomain ?? PromptDomainClassifier.Classify(userRequest).Domain;
        var resolvedSubPhase = topology.ResolveSubPhase("4.4");
        Assert.True(resolvedSubPhase.IsSuccess, resolvedSubPhase.ErrorMessage);
        var subPhase = Assert.IsType<LogicalSubPhase>(
            resolvedSubPhase.Value!.SubPhase);
        var provenanceInputs = new LogicalTrainingContentDeterministicInputs(
            userRequest,
            domain.ToString(),
            subPhase.PhaseId,
            subPhase.SubPhaseId,
            subPhase.Name,
            subPhase.Purpose,
            subPhase.PredicateIds,
            subPhase.DomainIds);
        branch = branch with
        {
            TrainingContentProvenance =
                LogicalCapabilityTrainingAuthorityTestFactory.IssueSingle(
                    branch,
                    provenanceInputs)
        };

        return new LogicalCapabilityTrainingRequest(
            runId,
            new LogicalOrchestratorRequest(
                runId,
                AdaptiveLogicalOrchestrator.ScenarioId,
                userRequest,
                new LogicalExecutionFlags(),
                new Dictionary<string, object>()),
            [branch],
            topology,
            flows,
            units)
        {
            Domain = domain
        };
    }

    private static LogicalCapabilityTrainingRequest BuildFastTrainingRequest(
        string runId,
        string userRequest,
        InMemoryLogicalTopologyRegistry topology,
        InMemoryLogicalFlowRegistry flows,
        InMemoryLogicalUnitRegistry units)
    {
        var request = BuildTrainingRequest(
            runId,
            userRequest,
            topology,
            flows,
            units);
        return request with
        {
            OriginalRequest = request.OriginalRequest with
            {
                Flags = new LogicalExecutionFlags
                {
                    UseIdleLearningScheduling = false
                },
                Metadata = new Dictionary<string, object>(StringComparer.Ordinal)
                {
                    ["dpo.max_steps"] = 1,
                    ["dpo.convergence_window_steps"] = 1
                }
            }
        };
    }

    private static LogicalCapabilityTrainingRequest
        BuildFastTrainingRequestWithDpoSteps(
            string runId,
            string userRequest,
            InMemoryLogicalTopologyRegistry topology,
            InMemoryLogicalFlowRegistry flows,
            InMemoryLogicalUnitRegistry units,
            int maximumStepCount)
    {
        var request = BuildFastTrainingRequest(
            runId,
            userRequest,
            topology,
            flows,
            units);
        return request with
        {
            OriginalRequest = request.OriginalRequest with
            {
                Metadata = new Dictionary<string, object>(
                    StringComparer.Ordinal)
                {
                    ["dpo.max_steps"] = maximumStepCount,
                    ["dpo.convergence_window_steps"] = maximumStepCount
                }
            }
        };
    }

    private static void AssertNoGeneratedStagingResidue(string root)
    {
        if (!Directory.Exists(root))
            return;
        Assert.DoesNotContain(
            Directory.EnumerateFileSystemEntries(
                root,
                "*",
                SearchOption.AllDirectories),
            path => Path.GetFileName(path).StartsWith(
                ".generated-capability-",
                StringComparison.Ordinal));
    }

    private static int CountOpenFileDescriptorsForTesting()
    {
        // Linux-only regression helper: считаем fd напрямую через /proc, чтобы
        // проверить именно dispose path, а не косвенный успех операции.
        return Directory.EnumerateFileSystemEntries("/proc/self/fd").Count();
    }

    private static LogicalFlowResult BuildKnowledgeEdgeBranch(
        string subPhaseId,
        string userRequest,
        Domain domain,
        InMemoryLogicalTopologyRegistry topology)
    {
        var summary = new LogicalFlowSummary(
            $"summary-{subPhaseId}",
            1,
            0,
            0,
            "knowledge_edge",
            false,
            false,
            false,
            "GENERATE_CAPABILITY",
            0.70,
            $"missing_sub_phase={subPhaseId}",
            "Capability is missing for routed sub-phase.",
            0,
            0);

        var branch = new LogicalFlowResult(
            $"missing-flow-{subPhaseId}",
            "missing-capability",
            "KNOWLEDGE_EDGE",
            "knowledge_edge: missing generated capability",
            summary,
            new Dictionary<string, string> { ["missing_sub_phase"] = subPhaseId },
            [],
            []);
        var resolved = topology.ResolveSubPhase(subPhaseId);
        Assert.True(resolved.IsSuccess, resolved.ErrorMessage);
        var subPhase = Assert.IsType<LogicalSubPhase>(resolved.Value!.SubPhase);
        return branch with
        {
            TrainingContentProvenance =
                LogicalCapabilityTrainingAuthorityTestFactory.IssueSingle(
                    branch,
                    new LogicalTrainingContentDeterministicInputs(
                        userRequest,
                        domain.ToString(),
                        subPhase.PhaseId,
                        subPhase.SubPhaseId,
                        subPhase.Name,
                        subPhase.Purpose,
                        subPhase.PredicateIds,
                        subPhase.DomainIds))
        };
    }

    private static LogicalFlowResult AttestSyntheticBranch(
        LogicalFlowResult branch)
        => branch with
        {
            TrainingContentProvenance = LogicalTrainingContentProvenance
                .IssueDeterministicSynthetic(branch)
        };

    private static GeneratedCapabilityCheckpointManifest[] InvokeLoadGeneratedCapabilityManifests(DefaultLogicalCapabilityTrainer trainer)
    {
        var method = typeof(DefaultLogicalCapabilityTrainer).GetMethod(
            "LoadGeneratedCapabilityManifests",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(method);
        return (GeneratedCapabilityCheckpointManifest[])method.Invoke(trainer, null)!;
    }

    private static GeneratedCapabilityCheckpointManifest BuildManifest(
        string unitId,
        string domainTag,
        string subPhaseId,
        DateTimeOffset createdAtUtc)
        => new(
            unitId,
            DefaultLogicalCapabilityTrainer.GeneratedCapabilityUnitRole,
            domainTag,
            DefaultLogicalCapabilityTrainer.GeneratedCapabilityVersion,
            LogicalUnitTrustState.Probationary.ToString(),
            $"{DefaultLogicalCapabilityTrainer.GeneratedCapabilityFlowIdPrefix}-{subPhaseId}",
            $"{DefaultLogicalCapabilityTrainer.GeneratedCapabilityScenarioIdPrefix}-{subPhaseId}",
            subPhaseId.Split('.')[0],
            subPhaseId,
            "refactoring-and-code-quality",
            "training text",
            "Generated capability for sub-phase 4.4: Refactoring and Code Quality. Execute the requested work through task type refactoring-and-code-quality.",
            "example",
            ["IMPLEMENT"],
            ["software-development"],
            "policy.bin",
            "tokenizer.json",
            512,
            128,
            16,
            1,
            1,
            1,
            32,
            2,
            4f,
            0.05f,
            ["q_proj", "v_proj"],
            createdAtUtc,
            $"generated-capability-{domainTag.ToLowerInvariant()}");

    private static string ComputeFileSha256(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static string DpoReservationLockFileName(int step)
        => $".{DpoTrainer.MetricsFilePrefix}{step:D6}.lock";

    private static string SnapshotFlowDescriptors(
        IEnumerable<LogicalFlowDescriptor> descriptors)
        => JsonSerializer.Serialize(descriptors.Select(static descriptor => new
        {
            descriptor.FlowId,
            descriptor.ScenarioId,
            descriptor.Version,
            InputType = descriptor.InputType.AssemblyQualifiedName,
            descriptor.PhaseId,
            descriptor.SubPhaseId,
            descriptor.TaskType,
            descriptor.DomainTags,
            descriptor.RegistrationOrder
        }));

    private static byte[] ReadAllBytesSharedDelete(string path)
    {
        using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read | FileShare.Delete);
        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);
        return buffer.ToArray();
    }

    private static string ComputeAdmissionDigest(
        string runId,
        string domain,
        int ordinal,
        LogicalDpoPreferenceExample row)
    {
        using var payload = new MemoryStream();
        WriteString("logical-capability-training-row-v1");
        WriteString(runId);
        WriteString(domain);
        WriteInt32(ordinal);
        WriteString(row.Prompt);
        WriteString(row.Chosen);
        WriteString(row.Rejected);
        WriteInt32(row.TeachingVector!.Length);
        foreach (var value in row.TeachingVector)
            WriteInt32(BitConverter.SingleToInt32Bits(value));
        return Convert.ToHexString(SHA256.HashData(payload.ToArray()))
            .ToLowerInvariant();

        void WriteString(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            WriteInt32(bytes.Length);
            payload.Write(bytes, 0, bytes.Length);
        }

        void WriteInt32(int value)
        {
            Span<byte> bytes = stackalloc byte[sizeof(int)];
            BinaryPrimitives.WriteInt32BigEndian(bytes, value);
            payload.Write(bytes);
        }
    }

    private static void AssertRuntimeProvenance(
        GeneratedCapabilityCheckpointManifest manifest)
    {
        var config = new TransformerModel.Config.ModelConfig
        {
            VocabSize = manifest.VocabSize,
            ContextLen = manifest.ContextLen,
            HiddenDim = manifest.HiddenDim,
            NumLayers = manifest.NumLayers,
            NumHeads = manifest.NumHeads,
            NumKvHeads = manifest.NumKvHeads,
            FfnHiddenDim = manifest.FfnHiddenDim,
            RopeTheta = manifest.RopeTheta!.Value,
            RmsNormEps = manifest.RmsNormEps!.Value
        };
        var loraConfig = new LoRA.LoraConfig
        {
            Rank = manifest.LoraRank,
            Alpha = manifest.LoraAlpha,
            Dropout = manifest.LoraDropout,
            TargetModules = manifest.LoraTargetModules
        };
        var computed = RuntimeProvenance.ComputeSha256(
            config,
            loraConfig,
            manifest.TokenizerSha256!,
            manifest.PolicyCheckpointSha256!);
        Assert.True(computed.IsSuccess, computed.ErrorMessage);
        Assert.Equal(computed.Value, manifest.RuntimeProvenanceSha256);
    }

    private static GeneratedCapabilityCheckpointManifest
        BuildMinimalGeneratedCapabilityManifest(
            string artifactDirectory,
            string unitId)
    {
        return new GeneratedCapabilityCheckpointManifest(
            unitId,
            DefaultLogicalCapabilityTrainer.GeneratedCapabilityUnitRole,
            Domain.PLAN.ToString(),
            DefaultLogicalCapabilityTrainer.GeneratedCapabilityVersion,
            nameof(LogicalUnitTrustState.Probationary),
            $"{DefaultLogicalCapabilityTrainer.GeneratedCapabilityFlowIdPrefix}-{unitId}",
            $"generated-capability-scenario-{unitId}",
            "4",
            "4.1",
            "backend-services",
            "training text",
            "PASS",
            $"example-{unitId}",
            ["IMPLEMENT"],
            ["software-development"],
            Path.Combine(artifactDirectory, "policy.bin"),
            Path.Combine(artifactDirectory, "tokenizer.json"),
            512,
            64,
            32,
            4,
            4,
            2,
            64,
            2,
            4,
            0,
            ["q_proj", "v_proj"],
            DateTimeOffset.UnixEpoch,
            UnitFamilyId: $"generated-capability-family-{unitId}");
    }

    private sealed class BusyIdleGate : ILogicalTeachingIdleGate
    {
        public AgentInfrastructure.OperationResult<bool> CanTrainNow(LogicalCapabilityTrainingRequest request)
        {
            return AgentInfrastructure.OperationResult<bool>.Ok(false);
        }
    }

    private sealed record PublicationChildProcess(
        Process Process,
        string ReadyPath,
        string ResultPath,
        Task<string> StandardOutput,
        Task<string> StandardError);
}

[CollectionDefinition("F42PublicationSerial", DisableParallelization = true)]
public sealed class F42PublicationSerialCollection
{
}
