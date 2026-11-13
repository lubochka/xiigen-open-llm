// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Diagnostics;
using AgentInfrastructure;
using ContinuousLearning;
using LoRA;
using Microsoft.Win32.SafeHandles;
using TransformerModel;
using TransformerModel.Config;

public sealed class RetainedDomainModelRegistryTests
{
    public enum RollbackInterruption
    {
        AfterEvidenceDurable,
        AfterRejectedDelete,
        RestoreRenameFailure,
        RestoreFileFlushFailure,
        RestoreRootFlushFailure,
    }

    public enum CommitInterruption
    {
        AfterAcceptedStagingDurable,
        AfterAcceptedEvidenceRename,
        CommitRootFlushFailure,
        AfterAcceptedCommitDurable,
    }

    public enum AcceptedCleanupInterruption
    {
        AfterBackupDelete,
        AfterRollbackDelete,
        AfterAcceptedDelete,
    }

    private static readonly ISessionStorage Storage = new RetainedRegistryNullStorage();

    [RetainedRegistryWindowsFact]
    public void FirstRegistrationUpdateAndFreshRestartPreserveCanonicalState()
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        byte[] acceptedBytes;

        using (var access = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess())
        using (var capability = access.AcquireRegistryRootCapability())
        {
            var registry = new DomainModelRegistry(Storage, capability);
            Assert.True(registry.Register(fixture.Adapter("PLAN", "plan.lora", 1)).IsSuccess);
            Assert.True(registry.Register(fixture.Adapter("BUG", "bug.lora", 2)).IsSuccess);
            Assert.True(registry.Register(fixture.Adapter("BUG", "bug-v2.lora", 3)).IsSuccess);

            var listed = registry.ListAll();
            Assert.True(listed.IsSuccess, listed.ErrorMessage);
            var adapters = Assert.IsAssignableFrom<IReadOnlyList<ModelAdapter>>(listed.Value);
            Assert.Equal(["BUG", "PLAN"], adapters.Select(value => value.Domain));
            Assert.Equal(3, adapters[0].TrainingExamples);
            acceptedBytes = File.ReadAllBytes(fixture.RegistryPath);
            AssertCanonicalDomainOrder(acceptedBytes, "BUG", "PLAN");
            AssertStableNamespace(fixture);
        }

        using var restartedAccess = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess();
        using var restartedCapability = restartedAccess.AcquireRegistryRootCapability();
        var restarted = new DomainModelRegistry(Storage, restartedCapability);
        var restored = restarted.ListAll();
        Assert.True(restored.IsSuccess, restored.ErrorMessage);
        Assert.Equal(["BUG", "PLAN"], restored.Value!.Select(value => value.Domain));
        Assert.Equal(acceptedBytes, File.ReadAllBytes(fixture.RegistryPath));
        AssertStableNamespace(fixture);
    }

    [RetainedRegistryWindowsTheory]
    [InlineData(1)]
    [InlineData(2)]
    public void HeldAdapterPreventsMutationAtBothAcceptanceChecksAndRestoresPrior(
        int mutationValidationOccurrence)
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        byte[] priorBytes;
        using (var firstAccess = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess())
        using (var firstCapability = firstAccess.AcquireRegistryRootCapability())
        {
            var first = new DomainModelRegistry(Storage, firstCapability);
            Assert.True(first.Register(fixture.Adapter("BUG", "prior.lora", 1)).IsSuccess);
            priorBytes = File.ReadAllBytes(fixture.RegistryPath);
        }

        var replacement = fixture.Adapter("BUG", "replacement.lora", 2);
        var validationOccurrence = 0;
        Exception? mutationFailure = null;
        using var access = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireRegistryRootCapability();
        var registry = new DomainModelRegistry(
            Storage,
            capability,
            () =>
            {
                validationOccurrence++;
                if (validationOccurrence == mutationValidationOccurrence)
                {
                    try
                    {
                        File.AppendAllText(replacement.AdapterPath, "mutation-attempt");
                    }
                    catch (Exception exception)
                    {
                        mutationFailure = exception;
                        throw;
                    }
                }
            });

        var result = registry.Register(replacement);

        Assert.False(result.IsSuccess);
        Assert.Equal("REGISTRY_PERSISTENCE_FAILED", result.ErrorCode);
        Assert.IsAssignableFrom<IOException>(mutationFailure);
        Assert.Equal(priorBytes, File.ReadAllBytes(fixture.RegistryPath));
        AssertRegistryMemory(registry, "BUG");
        Assert.Equal(1, registry.GetAdapter("BUG").Value!.TrainingExamples);
        AssertStableNamespace(fixture);
    }

    [RetainedRegistryWindowsFact]
    public async Task TwoRegistriesSharingCapabilitySerializeDistinctDomainUpdates()
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        using var access = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireRegistryRootCapability();
        var first = new DomainModelRegistry(Storage, capability);
        var second = new DomainModelRegistry(Storage, capability);
        var bug = fixture.Adapter("BUG", "shared-bug.lora", 1);
        var plan = fixture.Adapter("PLAN", "shared-plan.lora", 1);

        var results = await Task.WhenAll(
            Task.Run(() => first.Register(bug)),
            Task.Run(() => second.Register(plan)));

        Assert.All(results, result => Assert.True(result.IsSuccess, result.ErrorMessage));
        Assert.Equal(
            ["BUG", "PLAN"],
            first.ListAll().Value!.Select(value => value.Domain));
        Assert.Equal(
            ["BUG", "PLAN"],
            second.ListAll().Value!.Select(value => value.Domain));
        AssertStableNamespace(fixture);
    }

    [RetainedRegistryWindowsFact]
    public void FailedFirstPublicationRestoresConfirmedAbsence()
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        using var access = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireRegistryRootCapability();
        var registry = new DomainModelRegistry(
            Storage,
            capability,
            () => throw new InvalidDataException("Injected acceptance rejection."));

        var result = registry.Register(fixture.Adapter("BUG", "first.lora", 1));

        Assert.False(result.IsSuccess);
        Assert.Equal("REGISTRY_PERSISTENCE_FAILED", result.ErrorCode);
        Assert.False(File.Exists(fixture.RegistryPath));
        AssertRegistryMemory(registry);
        Assert.Empty(registry.ListAll().Value!);
        AssertStableNamespace(fixture, canonicalExpected: false);
    }

    [RetainedRegistryWindowsFact]
    public void FreshRestartRecoversDeterministicTempOnlyAndBackupOnlyStates()
    {
        RequireWindows();
        using (var tempFixture = new RetainedRegistryFixture())
        {
            using (var access = tempFixture.FileAccess.CreateStartup().AcquireRetainedFileAccess())
            using (var capability = access.AcquireRegistryRootCapability())
            {
                var registry = new DomainModelRegistry(Storage, capability);
                Assert.True(registry.Register(tempFixture.Adapter("BUG", "temp.lora", 1)).IsSuccess);
                Assert.Single(registry.ListAll().Value!);
            }
            File.Move(tempFixture.RegistryPath, tempFixture.TempPath);

            using var restartedAccess = tempFixture.FileAccess.CreateStartup().AcquireRetainedFileAccess();
            using var restartedCapability = restartedAccess.AcquireRegistryRootCapability();
            var restarted = new DomainModelRegistry(Storage, restartedCapability);
            Assert.Empty(restarted.ListAll().Value!);
            AssertStableNamespace(tempFixture, canonicalExpected: false);
        }

        using var backupFixture = new RetainedRegistryFixture();
        byte[] priorBytes;
        using (var access = backupFixture.FileAccess.CreateStartup().AcquireRetainedFileAccess())
        using (var capability = access.AcquireRegistryRootCapability())
        {
            var registry = new DomainModelRegistry(Storage, capability);
            Assert.True(registry.Register(backupFixture.Adapter("BUG", "backup.lora", 1)).IsSuccess);
            Assert.Single(registry.ListAll().Value!);
            priorBytes = File.ReadAllBytes(backupFixture.RegistryPath);
        }
        File.Move(backupFixture.RegistryPath, backupFixture.BackupPath);

        using var backupRestartAccess = backupFixture.FileAccess.CreateStartup().AcquireRetainedFileAccess();
        using var backupRestartCapability = backupRestartAccess.AcquireRegistryRootCapability();
        var recovered = new DomainModelRegistry(Storage, backupRestartCapability);
        Assert.NotNull(recovered.GetAdapter("BUG").Value);
        Assert.Equal(priorBytes, File.ReadAllBytes(backupFixture.RegistryPath));
        AssertStableNamespace(backupFixture);
    }

    [RetainedRegistryWindowsFact]
    public void AmbiguousOrForeignRestartEvidenceFailsWithoutLeafMutation()
    {
        RequireWindows();
        using (var fixture = new RetainedRegistryFixture())
        {
            byte[] prior;
            byte[] candidate;
            using (var access = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess())
            using (var capability = access.AcquireRegistryRootCapability())
            {
                var registry = new DomainModelRegistry(Storage, capability);
                Assert.True(registry.Register(fixture.Adapter("BUG", "ambiguous-a.lora", 1)).IsSuccess);
                Assert.Single(registry.ListAll().Value!);
                prior = File.ReadAllBytes(fixture.RegistryPath);
                Assert.True(registry.Register(fixture.Adapter("PLAN", "ambiguous-b.lora", 1)).IsSuccess);
                Assert.Equal(2, registry.ListAll().Value!.Count);
                candidate = File.ReadAllBytes(fixture.RegistryPath);
            }
            File.WriteAllBytes(fixture.BackupPath, prior);
            File.WriteAllBytes(fixture.TempPath, candidate);
            var before = SnapshotLeaves(fixture);

            using var failedAccess = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess();
            using var failedCapability = failedAccess.AcquireRegistryRootCapability();
            Assert.Throws<InvalidDataException>(
                () => new DomainModelRegistry(Storage, failedCapability));
            Assert.Equal(before, SnapshotLeaves(fixture));
        }

        using var foreignFixture = new RetainedRegistryFixture();
        using (var access = foreignFixture.FileAccess.CreateStartup().AcquireRetainedFileAccess())
        using (var capability = access.AcquireRegistryRootCapability())
        {
            var registry = new DomainModelRegistry(Storage, capability);
            Assert.True(registry.Register(foreignFixture.Adapter("BUG", "foreign.lora", 1)).IsSuccess);
            Assert.Single(registry.ListAll().Value!);
        }
        File.WriteAllBytes(foreignFixture.TempPath, [0xde, 0xad, 0xbe, 0xef]);
        var foreignBefore = SnapshotLeaves(foreignFixture);
        using var foreignAccess = foreignFixture.FileAccess.CreateStartup().AcquireRetainedFileAccess();
        using var foreignCapability = foreignAccess.AcquireRegistryRootCapability();
        Assert.Throws<InvalidDataException>(
            () => new DomainModelRegistry(Storage, foreignCapability));
        Assert.Equal(foreignBefore, SnapshotLeaves(foreignFixture));
    }

    [RetainedRegistryWindowsFact]
    public void RollbackMarkerRejectsReversedForeignAndMalformedLeafCombinationsWithoutMutation()
    {
        RequireWindows();
        byte[] foreignMarker;
        using (var sourceFixture = new RetainedRegistryFixture())
        {
            _ = CreatePendingRollbackEvidence(sourceFixture, "marker-source");
            foreignMarker = File.ReadAllBytes(sourceFixture.RollbackEvidencePath);
        }

        using (var reversedFixture = new RetainedRegistryFixture())
        {
            var priorBytes = CreatePendingRollbackEvidence(
                reversedFixture,
                "marker-reversed");
            File.Delete(reversedFixture.TempPath);
            File.Move(reversedFixture.RegistryPath, reversedFixture.TempPath);
            File.WriteAllBytes(reversedFixture.RegistryPath, priorBytes);
            AssertRestartRejectedWithoutLeafMutation(reversedFixture);
        }

        using (var foreignFixture = new RetainedRegistryFixture())
        {
            _ = CreatePendingRollbackEvidence(foreignFixture, "marker-foreign");
            File.WriteAllBytes(foreignFixture.RollbackEvidencePath, foreignMarker);
            AssertRestartRejectedWithoutLeafMutation(foreignFixture);
        }

        using var malformedFixture = new RetainedRegistryFixture();
        _ = CreatePendingRollbackEvidence(malformedFixture, "marker-malformed");
        var malformed = File.ReadAllBytes(malformedFixture.RollbackEvidencePath);
        malformed[0] ^= 0xff;
        File.WriteAllBytes(malformedFixture.RollbackEvidencePath, malformed);
        AssertRestartRejectedWithoutLeafMutation(malformedFixture);
    }

    [RetainedRegistryWindowsFact]
    public void AcceptedEvidenceRejectsForeignCandidateRollbackAndCommitLeavesWithoutMutation()
    {
        RequireWindows();
        byte[] foreignRollback;
        byte[] foreignAccepted;
        using (var source = new RetainedRegistryFixture())
        {
            CreatePendingAcceptedEvidence(source, "accepted-foreign-source");
            foreignRollback = File.ReadAllBytes(source.RollbackEvidencePath);
            foreignAccepted = File.ReadAllBytes(source.AcceptedEvidencePath);
        }

        using (var candidateFixture = new RetainedRegistryFixture())
        {
            CreatePendingAcceptedEvidence(candidateFixture, "accepted-candidate");
            File.WriteAllBytes(candidateFixture.RegistryPath, [0xde, 0xad, 0xbe, 0xef]);
            AssertRestartRejectedWithoutLeafMutation(candidateFixture);
        }

        using (var rollbackFixture = new RetainedRegistryFixture())
        {
            CreatePendingAcceptedEvidence(rollbackFixture, "accepted-rollback");
            File.WriteAllBytes(rollbackFixture.RollbackEvidencePath, foreignRollback);
            AssertRestartRejectedWithoutLeafMutation(rollbackFixture);
        }

        using var acceptedFixture = new RetainedRegistryFixture();
        CreatePendingAcceptedEvidence(acceptedFixture, "accepted-marker");
        File.WriteAllBytes(acceptedFixture.AcceptedEvidencePath, foreignAccepted);
        AssertRestartRejectedWithoutLeafMutation(acceptedFixture);
    }

    [RetainedRegistryWindowsFact]
    public void RetainedRestartPreservesStrictJsonFailureAndDoesNotRewriteCanonical()
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        using (var access = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess())
        using (var capability = access.AcquireRegistryRootCapability())
        {
            var registry = new DomainModelRegistry(Storage, capability);
            Assert.True(registry.Register(fixture.Adapter("BUG", "strict.lora", 1)).IsSuccess);
            Assert.Single(registry.ListAll().Value!);
        }
        var malformed = File.ReadAllText(fixture.RegistryPath).Replace(
            "{\"SchemaVersion\":1,",
            "{\"SchemaVersion\":1,\"schemaVersion\":1,",
            StringComparison.Ordinal);
        File.WriteAllText(fixture.RegistryPath, malformed);
        var exactMalformedBytes = File.ReadAllBytes(fixture.RegistryPath);

        using var failedAccess = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess();
        using var failedCapability = failedAccess.AcquireRegistryRootCapability();
        var exception = Assert.Throws<InvalidDataException>(
            () => new DomainModelRegistry(Storage, failedCapability));

        Assert.StartsWith("REGISTRY_STATE_CORRUPT:", exception.Message, StringComparison.Ordinal);
        Assert.Equal(exactMalformedBytes, File.ReadAllBytes(fixture.RegistryPath));
        Assert.False(File.Exists(fixture.TempPath));
        Assert.False(File.Exists(fixture.BackupPath));
    }

    [RetainedRegistryWindowsFact]
    public void Version2RetainedPublicationFailureRestoresExactPriorBytes()
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        byte[] priorBytes;
        using (var seedAccess = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess())
        using (var seedCapability = seedAccess.AcquireRegistryRootCapability())
        {
            var seed = new DomainModelRegistry(Storage, seedCapability);
            Assert.True(seed.Register(fixture.Adapter("BUG", "v2-retained-prior.lora", 1)).IsSuccess);
            Assert.Single(seed.ListAll().Value!);
            priorBytes = ConvertRegistryToVersion2Legacy(
                File.ReadAllBytes(fixture.RegistryPath));
            PublishExactRegistryBytes(seedCapability, priorBytes);
            var accepted = seed.ListAll();
            Assert.True(accepted.IsSuccess, accepted.ErrorMessage);
            Assert.Single(accepted.Value!);
            Assert.Equal(priorBytes, File.ReadAllBytes(fixture.RegistryPath));
        }

        using var access = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireRegistryRootCapability();
        var callbackCount = 0;
        var candidatePublicationObserved = false;
        Exception? publicationInspectionFailure = null;
        var registry = new DomainModelRegistry(
            Storage,
            capability,
            () =>
            {
                callbackCount++;
                try
                {
                    var inventory = capability.Inventory();
                    var candidate = inventory.CanonicalRegistry.Snapshot;
                    var backup = inventory.PublicationBackup.Snapshot;
                    var rollback = inventory.RollbackEvidence.Snapshot;
                    Assert.NotNull(candidate);
                    Assert.NotNull(backup);
                    Assert.NotNull(rollback);
                    var candidateBytes = candidate!.CopyBytes();
                    Assert.False(candidateBytes.AsSpan().SequenceEqual(priorBytes));
                    AssertVersion2LegacyRegistryDomains(candidateBytes, "BUG", "PLAN");
                    AssertVersion2LegacyRegistryDomains(priorBytes, "BUG");
                    Assert.Equal(priorBytes, backup!.CopyBytes());
                    var evidence = capability.DecodeRollbackEvidence(rollback!);
                    Assert.Equal(
                        ServerOwnedContinuousLearningRegistryCanonicalState.Present,
                        evidence.PriorState);
                    Assert.True(evidence.PriorContentMatches(backup));
                    Assert.True(evidence.RejectedArtifactMatches(candidate));
                    candidatePublicationObserved = true;
                }
                catch (Exception exception)
                {
                    publicationInspectionFailure = exception;
                }
                throw new InvalidDataException("Injected v2 retained rejection.");
            });
        var result = registry.Register(fixture.Adapter("PLAN", "v2-retained-rejected.lora", 1));

        Assert.False(result.IsSuccess);
        Assert.Equal("REGISTRY_PERSISTENCE_FAILED", result.ErrorCode);
        Assert.Equal(1, callbackCount);
        Assert.Null(publicationInspectionFailure);
        Assert.True(candidatePublicationObserved);
        Assert.Equal(priorBytes, File.ReadAllBytes(fixture.RegistryPath));
        AssertRegistryMemory(registry, "BUG");
        AssertStableNamespace(fixture);
    }

    [RetainedRegistryWindowsFact]
    public void Version2InterruptedRetainedRollbackRestartRestoresExactPriorBytes()
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        byte[] priorBytes;
        using (var seedAccess = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess())
        using (var seedCapability = seedAccess.AcquireRegistryRootCapability())
        {
            var seed = new DomainModelRegistry(Storage, seedCapability);
            Assert.True(seed.Register(fixture.Adapter("BUG", "v2-recovery-prior.lora", 1)).IsSuccess);
            Assert.Single(seed.ListAll().Value!);
            priorBytes = ConvertRegistryToVersion2Legacy(
                File.ReadAllBytes(fixture.RegistryPath));
            PublishExactRegistryBytes(seedCapability, priorBytes);
            var accepted = seed.ListAll();
            Assert.True(accepted.IsSuccess, accepted.ErrorMessage);
            Assert.Single(accepted.Value!);
            Assert.Equal(priorBytes, File.ReadAllBytes(fixture.RegistryPath));
        }

        using (var access = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess())
        using (var capability = access.AcquireRegistryRootCapability(
                   new ServerOwnedContinuousLearningRegistryRootCapabilityOptions
                   {
                       ObserveIoStageForTesting = stage =>
                       {
                           if (stage == ServerOwnedContinuousLearningRegistryIoStage
                                   .AfterRollbackEvidenceDurable)
                           {
                               GetRetainedRootHandle(
                                   access,
                                   ContinuousLearningRetainedObjectRole.RegistryRoot).Dispose();
                           }
                       },
                   }))
        {
            var registry = new DomainModelRegistry(Storage, capability);
            var result = registry.Register(
                fixture.Adapter("PLAN", "v2-recovery-rejected.lora", 1));
            Assert.False(result.IsSuccess);
            Assert.Equal("REGISTRY_PERSISTENCE_FAILED", result.ErrorCode);
            AssertRegistryMemory(registry, "BUG");
            Assert.True(File.Exists(fixture.RollbackEvidencePath));
        }

        using var restartedAccess = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess();
        using var restartedCapability = restartedAccess.AcquireRegistryRootCapability();
        var restarted = new DomainModelRegistry(Storage, restartedCapability);
        Assert.Equal(priorBytes, File.ReadAllBytes(fixture.RegistryPath));
        AssertRegistryMemory(restarted, "BUG");
        AssertStableNamespace(fixture);
    }

    [RetainedRegistryWindowsFact]
    public void CrossVersionRecoveryTransitionIsRejectedWithoutLeafMutation()
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        using (var access = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess())
        using (var capability = access.AcquireRegistryRootCapability())
        {
            var registry = new DomainModelRegistry(Storage, capability);
            Assert.True(registry.Register(
                fixture.Adapter("BUG", "cross-version-prior.lora", 1)).IsSuccess);
            Assert.Single(registry.ListAll().Value!);
            var candidate = ConvertRegistryToVersion2Legacy(
                File.ReadAllBytes(fixture.RegistryPath));
            _ = capability.CreateWriteTemp(candidate);
        }
        var before = SnapshotLeaves(fixture);

        using var failedAccess = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess();
        using var failedCapability = failedAccess.AcquireRegistryRootCapability();
        var exception = Assert.Throws<InvalidDataException>(
            () => new DomainModelRegistry(Storage, failedCapability));

        Assert.StartsWith("REGISTRY_STATE_CORRUPT:", exception.Message, StringComparison.Ordinal);
        Assert.Equal(before, SnapshotLeaves(fixture));
    }

    [RetainedRegistryWindowsTheory]
    [InlineData((int)ServerOwnedContinuousLearningRegistryNativeCall.FileFlush, 3)]
    [InlineData((int)ServerOwnedContinuousLearningRegistryNativeCall.RootDirectoryFlush, 3)]
    public void PreCommitFlushFailureNeverSucceedsAndRestoresExactPriorBytes(
        int failedCallValue,
        int failedOccurrence)
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        byte[] priorBytes;
        using (var firstAccess = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess())
        using (var firstCapability = firstAccess.AcquireRegistryRootCapability())
        {
            var first = new DomainModelRegistry(Storage, firstCapability);
            Assert.True(first.Register(fixture.Adapter("BUG", "flush-prior.lora", 1)).IsSuccess);
            Assert.Single(first.ListAll().Value!);
            priorBytes = File.ReadAllBytes(fixture.RegistryPath);
        }

        var occurrence = 0;
        var failedCall = (ServerOwnedContinuousLearningRegistryNativeCall)failedCallValue;
        using var access = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireRegistryRootCapability(
            new ServerOwnedContinuousLearningRegistryRootCapabilityOptions
            {
                FailNativeCallForTesting = call =>
                    call == failedCall && ++occurrence == failedOccurrence,
            });
        var registry = new DomainModelRegistry(Storage, capability);
        var result = registry.Register(fixture.Adapter("PLAN", "flush-new.lora", 1));

        Assert.False(result.IsSuccess);
        Assert.Equal("REGISTRY_PERSISTENCE_FAILED", result.ErrorCode);
        Assert.Equal(priorBytes, File.ReadAllBytes(fixture.RegistryPath));
        AssertRegistryMemory(registry, "BUG");
        Assert.Equal(["BUG"], registry.ListAll().Value!.Select(value => value.Domain));
        AssertStableNamespace(fixture);
    }

    [RetainedRegistryWindowsTheory]
    [InlineData(RollbackInterruption.AfterEvidenceDurable, 1, false)]
    [InlineData(RollbackInterruption.AfterRejectedDelete, 2, true)]
    [InlineData(RollbackInterruption.RestoreRenameFailure, 1, true)]
    [InlineData(RollbackInterruption.RestoreFileFlushFailure, 2, false)]
    [InlineData(RollbackInterruption.RestoreRootFlushFailure, 1, false)]
    public void InterruptedRollbackFreshRestartRestoresExactPriorState(
        RollbackInterruption interruption,
        int priorDomainCount,
        bool addDomain)
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        byte[] priorBytes;
        string[] priorDomains;
        using (var seedAccess = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess())
        using (var seedCapability = seedAccess.AcquireRegistryRootCapability())
        {
            var seed = new DomainModelRegistry(Storage, seedCapability);
            Assert.True(seed.Register(fixture.Adapter("BUG", "rollback-bug.lora", 1)).IsSuccess);
            if (priorDomainCount == 2)
            {
                Assert.True(seed.Register(
                    fixture.Adapter("PLAN", "rollback-plan.lora", 1)).IsSuccess);
            }
            priorBytes = File.ReadAllBytes(fixture.RegistryPath);
            priorDomains = seed.ListAll().Value!.Select(adapter => adapter.Domain).ToArray();
        }

        var rollbackEvidenceDurable = false;
        var restoreRenamed = false;
        var restoreFileFlushed = false;
        var renameCount = 0;
        using (var access = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess())
        using (var capability = access.AcquireRegistryRootCapability(
                   new ServerOwnedContinuousLearningRegistryRootCapabilityOptions
                   {
                        FailNativeCallForTesting = call =>
                        {
                            if (call == ServerOwnedContinuousLearningRegistryNativeCall.FileFlush &&
                                restoreRenamed &&
                                interruption == RollbackInterruption.RestoreFileFlushFailure)
                            {
                                return true;
                            }
                            if (call != ServerOwnedContinuousLearningRegistryNativeCall.NoReplaceRename)
                            {
                                return false;
                           }
                           renameCount++;
                           return interruption == RollbackInterruption.RestoreRenameFailure &&
                                  renameCount == 3;
                       },
                       ObserveIoStageForTesting = stage =>
                       {
                           if (stage == ServerOwnedContinuousLearningRegistryIoStage
                                   .AfterRollbackEvidenceDurable)
                           {
                               rollbackEvidenceDurable = true;
                               if (interruption ==
                                   RollbackInterruption.AfterEvidenceDurable)
                               {
                                   GetRetainedRootHandle(
                                       access,
                                       ContinuousLearningRetainedObjectRole.RegistryRoot)
                                       .Dispose();
                               }
                           }
                           else if (rollbackEvidenceDurable &&
                                    stage == ServerOwnedContinuousLearningRegistryIoStage
                                        .AfterRejectedArtifactDelete &&
                                    interruption ==
                                        RollbackInterruption.AfterRejectedDelete)
                           {
                               GetRetainedRootHandle(
                                   access,
                                   ContinuousLearningRetainedObjectRole.RegistryRoot).Dispose();
                           }
                           else if (stage == ServerOwnedContinuousLearningRegistryIoStage
                                        .AfterPriorArtifactRestore)
                           {
                               restoreRenamed = true;
                           }
                            else if (restoreRenamed &&
                                     stage == ServerOwnedContinuousLearningRegistryIoStage
                                        .AfterFileFlush)
                           {
                               restoreFileFlushed = true;
                           }
                           else if (restoreFileFlushed &&
                                    stage == ServerOwnedContinuousLearningRegistryIoStage
                                        .BeforeRootDirectoryFlush &&
                                    interruption ==
                                        RollbackInterruption.RestoreRootFlushFailure)
                           {
                               GetRetainedRootHandle(
                                   access,
                                   ContinuousLearningRetainedObjectRole.RegistryRoot).Dispose();
                           }
                       },
                   }))
        {
            var validation = 0;
            var registry = new DomainModelRegistry(
                Storage,
                capability,
                () =>
                {
                    validation++;
                    if (validation == 2)
                        throw new InvalidDataException("Injected late acceptance rejection.");
                });
            var candidate = addDomain
                ? fixture.Adapter("TEST", $"rollback-add-{interruption}.lora", 1)
                : fixture.Adapter("BUG", $"rollback-update-{interruption}.lora", 2);

            var result = registry.Register(candidate);

            Assert.False(result.IsSuccess);
            Assert.Equal("REGISTRY_PERSISTENCE_FAILED", result.ErrorCode);
            AssertRegistryMemory(registry, priorDomains);
            Assert.True(File.Exists(fixture.RollbackEvidencePath));
        }

        using var restartedAccess = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess();
        using var restartedCapability = restartedAccess.AcquireRegistryRootCapability();
        var restarted = new DomainModelRegistry(Storage, restartedCapability);

        Assert.Equal(priorBytes, File.ReadAllBytes(fixture.RegistryPath));
        AssertRegistryMemory(restarted, priorDomains);
        Assert.Equal(priorDomains, restarted.ListAll().Value!.Select(adapter => adapter.Domain));
        AssertStableNamespace(fixture);
    }

    [RetainedRegistryWindowsTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void RootLossAfterCandidateRenameReturnsFailedAndFreshRestartRollsBack(
        bool hasPrior)
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        byte[]? priorBytes = null;
        if (hasPrior)
        {
            using var seedAccess = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess();
            using var seedCapability = seedAccess.AcquireRegistryRootCapability();
            var seed = new DomainModelRegistry(Storage, seedCapability);
            Assert.True(seed.Register(fixture.Adapter("BUG", "rename-prior.lora", 1)).IsSuccess);
            Assert.Single(seed.ListAll().Value!);
            priorBytes = File.ReadAllBytes(fixture.RegistryPath);
        }

        var renameCount = 0;
        using (var access = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess())
        using (var capability = access.AcquireRegistryRootCapability(
                   new ServerOwnedContinuousLearningRegistryRootCapabilityOptions
                   {
                       ObserveIoStageForTesting = stage =>
                       {
                           if (stage != ServerOwnedContinuousLearningRegistryIoStage
                                   .AfterNoReplaceRename)
                           {
                               return;
                           }
                           renameCount++;
                           var candidateRename = hasPrior ? 2 : 1;
                           if (renameCount == candidateRename)
                           {
                               GetRetainedRootHandle(
                                   access,
                                   ContinuousLearningRetainedObjectRole.RegistryRoot).Dispose();
                           }
                       },
                   }))
        {
            var registry = new DomainModelRegistry(Storage, capability);
            var result = registry.Register(fixture.Adapter(
                "BUG",
                hasPrior ? "rename-update.lora" : "rename-first.lora",
                hasPrior ? 2 : 1));

            Assert.False(result.IsSuccess);
            Assert.Equal("REGISTRY_PERSISTENCE_FAILED", result.ErrorCode);
            AssertRegistryMemory(registry, hasPrior ? ["BUG"] : []);
            Assert.True(File.Exists(fixture.RollbackEvidencePath));
            Assert.False(File.Exists(fixture.AcceptedEvidencePath));
        }

        using var restartedAccess = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess();
        using var restartedCapability = restartedAccess.AcquireRegistryRootCapability();
        var restarted = new DomainModelRegistry(Storage, restartedCapability);
        AssertRegistryMemory(restarted, hasPrior ? ["BUG"] : []);
        if (hasPrior)
            Assert.Equal(priorBytes, File.ReadAllBytes(fixture.RegistryPath));
        else
            Assert.False(File.Exists(fixture.RegistryPath));
        AssertStableNamespace(fixture, canonicalExpected: hasPrior);
    }

    [RetainedRegistryWindowsTheory]
    [InlineData(CommitInterruption.AfterAcceptedStagingDurable, 1, false)]
    [InlineData(CommitInterruption.AfterAcceptedStagingDurable, 2, true)]
    [InlineData(CommitInterruption.AfterAcceptedEvidenceRename, 1, true)]
    [InlineData(CommitInterruption.CommitRootFlushFailure, 2, false)]
    [InlineData(CommitInterruption.AfterAcceptedCommitDurable, 1, false)]
    [InlineData(CommitInterruption.AfterAcceptedCommitDurable, 2, true)]
    public void CommitBoundaryIndeterminateRestartUsesAtomicAcceptedLeaf(
        CommitInterruption interruption,
        int priorDomainCount,
        bool addDomain)
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        byte[] priorBytes;
        string[] priorDomains;
        using (var seedAccess = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess())
        using (var seedCapability = seedAccess.AcquireRegistryRootCapability())
        {
            var seed = new DomainModelRegistry(Storage, seedCapability);
            Assert.True(seed.Register(fixture.Adapter("BUG", "commit-bug.lora", 1)).IsSuccess);
            if (priorDomainCount == 2)
            {
                Assert.Single(seed.ListAll().Value!, adapter => adapter.Domain == "BUG");
                Assert.True(seed.Register(fixture.Adapter("PLAN", "commit-plan.lora", 1)).IsSuccess);
            }
            _ = seed.ListAll();
            priorBytes = File.ReadAllBytes(fixture.RegistryPath);
            priorDomains = seed.ListAll().Value!.Select(adapter => adapter.Domain).ToArray();
        }

        var acceptedRenamed = false;
        var commitRootFailed = false;
        using (var access = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess())
        using (var capability = access.AcquireRegistryRootCapability(
                   new ServerOwnedContinuousLearningRegistryRootCapabilityOptions
                   {
                       FailNativeCallForTesting = call =>
                       {
                           if (interruption != CommitInterruption.CommitRootFlushFailure ||
                               call != ServerOwnedContinuousLearningRegistryNativeCall
                                   .RootDirectoryFlush ||
                               !acceptedRenamed || commitRootFailed)
                           {
                               return false;
                           }
                           commitRootFailed = true;
                           return true;
                       },
                       ObserveIoStageForTesting = stage =>
                       {
                           if (stage == ServerOwnedContinuousLearningRegistryIoStage
                                   .AfterAcceptedEvidenceRename)
                           {
                               acceptedRenamed = true;
                           }
                           if ((interruption ==
                                    CommitInterruption.AfterAcceptedStagingDurable &&
                                stage == ServerOwnedContinuousLearningRegistryIoStage
                                    .AfterAcceptedStagingDurable) ||
                               (interruption ==
                                    CommitInterruption.AfterAcceptedEvidenceRename &&
                                stage == ServerOwnedContinuousLearningRegistryIoStage
                                    .AfterAcceptedEvidenceRename) ||
                               (interruption ==
                                    CommitInterruption.AfterAcceptedCommitDurable &&
                                stage == ServerOwnedContinuousLearningRegistryIoStage
                                    .AfterAcceptedCommitDurable))
                           {
                               GetRetainedRootHandle(
                                   access,
                                   ContinuousLearningRetainedObjectRole.RegistryRoot).Dispose();
                           }
                       },
                   }))
        {
            var registry = new DomainModelRegistry(Storage, capability);
            var candidate = addDomain
                ? fixture.Adapter("TEST", $"commit-add-{interruption}.lora", 1)
                : fixture.Adapter("BUG", $"commit-update-{interruption}.lora", 2);
            var result = registry.Register(candidate);

            Assert.False(result.IsSuccess);
            Assert.Equal("REGISTRY_COMMIT_OUTCOME_INDETERMINATE", result.ErrorCode);
            AssertRegistryMemory(registry, priorDomains);
            Assert.True(File.Exists(fixture.RollbackEvidencePath));
            var acceptedExpected = interruption !=
                                   CommitInterruption.AfterAcceptedStagingDurable;
            Assert.Equal(acceptedExpected, File.Exists(fixture.AcceptedEvidencePath));
            Assert.Equal(!acceptedExpected, File.Exists(fixture.AcceptedStagingPath));
        }

        var committed = interruption != CommitInterruption.AfterAcceptedStagingDurable;
        using var restartedAccess = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess();
        using var restartedCapability = restartedAccess.AcquireRegistryRootCapability();
        var restarted = new DomainModelRegistry(Storage, restartedCapability);
        var expectedDomains = committed
            ? (addDomain ? priorDomains.Append("TEST").Order(StringComparer.Ordinal).ToArray() : priorDomains)
            : priorDomains;
        AssertRegistryMemory(restarted, expectedDomains);
        if (!committed)
            Assert.Equal(priorBytes, File.ReadAllBytes(fixture.RegistryPath));
        else if (!addDomain)
            Assert.Equal(2, restarted.GetAdapter("BUG").Value!.TrainingExamples);
        AssertStableNamespace(fixture);
    }

    [RetainedRegistryWindowsTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void SuccessfulRegisterLeavesExactAcceptedEvidenceForDeferredRestartCleanup(
        bool hasPrior)
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        if (hasPrior)
        {
            using var seedAccess = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess();
            using var seedCapability = seedAccess.AcquireRegistryRootCapability();
            var seed = new DomainModelRegistry(Storage, seedCapability);
            Assert.True(seed.Register(fixture.Adapter("BUG", "accepted-prior.lora", 1)).IsSuccess);
            Assert.Single(seed.ListAll().Value!);
        }

        using (var access = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess())
        using (var capability = access.AcquireRegistryRootCapability())
        {
            var registry = new DomainModelRegistry(Storage, capability);
            var result = registry.Register(fixture.Adapter(
                "BUG",
                hasPrior ? "accepted-update.lora" : "accepted-first.lora",
                hasPrior ? 2 : 1));

            Assert.True(result.IsSuccess, result.ErrorMessage);
            AssertRegistryMemory(registry, "BUG");
            var inventory = capability.Inventory();
            Assert.Equal(
                ServerOwnedContinuousLearningRegistryLeafState.Present,
                inventory.RollbackEvidence.State);
            Assert.Equal(
                ServerOwnedContinuousLearningRegistryLeafState.Present,
                inventory.AcceptedEvidence.State);
            Assert.Equal(
                ServerOwnedContinuousLearningRegistryLeafState.Absent,
                inventory.AcceptedStaging.State);
            var accepted = capability.DecodeAcceptedEvidence(
                inventory.AcceptedEvidence.Snapshot!);
            Assert.True(accepted.RollbackEvidence.MarkerArtifact.Matches(
                inventory.RollbackEvidence.Snapshot!));
            Assert.True(accepted.RollbackEvidence.RejectedArtifactMatches(
                inventory.CanonicalRegistry.Snapshot!));
            Assert.Equal(
                hasPrior
                    ? ServerOwnedContinuousLearningRegistryLeafState.Present
                    : ServerOwnedContinuousLearningRegistryLeafState.Absent,
                inventory.PublicationBackup.State);
        }

        using var restartedAccess = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess();
        using var restartedCapability = restartedAccess.AcquireRegistryRootCapability();
        var restarted = new DomainModelRegistry(Storage, restartedCapability);
        AssertRegistryMemory(restarted, "BUG");
        Assert.Equal(hasPrior ? 2 : 1, restarted.GetAdapter("BUG").Value!.TrainingExamples);
        AssertStableNamespace(fixture);
    }

    [RetainedRegistryWindowsTheory]
    [InlineData(80)]
    [InlineData(79)]
    [InlineData(0)]
    public void NearAndMaximumPriorRegistryCommitsMaximumAcceptedEvidenceAndFreshCleanup(
        int bytesBelowRegistryMaximum)
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        var targetLength = checked(
            DomainModelRegistry.MaximumRegistryBytes - bytesBelowRegistryMaximum);
        var prior = WriteExactLengthRegistry(
            fixture,
            targetLength,
            $"maximum-prior-{bytesBelowRegistryMaximum}.lora");

        using (var access = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess())
        using (var capability = access.AcquireRegistryRootCapability())
        {
            var registry = new DomainModelRegistry(Storage, capability);
            Assert.Equal(1, registry.GetAdapter(prior.Adapter.Domain).Value!.TrainingExamples);

            var result = registry.Register(prior.Adapter with { TrainingExamples = 2 });

            Assert.True(result.IsSuccess, result.ErrorMessage);
            AssertRegistryMemory(registry, prior.Adapter.Domain);
            Assert.Equal(targetLength, new FileInfo(fixture.RegistryPath).Length);
            var inventory = capability.Inventory();
            var accepted = Assert.IsType<ServerOwnedContinuousLearningRegistrySnapshot>(
                inventory.AcceptedEvidence.Snapshot);
            var expectedAcceptedLength = checked(
                targetLength +
                ServerOwnedContinuousLearningRegistryRollbackEvidence.EncodedHeaderLength +
                ServerOwnedContinuousLearningRegistryAcceptedEvidence.EncodedHeaderLength);
            Assert.Equal(expectedAcceptedLength, accepted.Length);
            Assert.Equal(
                expectedAcceptedLength,
                capability.QueryExactIdentity(
                    ServerOwnedContinuousLearningRegistryLeaf.AcceptedEvidence).Length);
            Assert.True(capability.ReadExact(
                    ServerOwnedContinuousLearningRegistryLeaf.AcceptedEvidence,
                    accepted.Identity)
                .Matches(accepted));
            capability.FlushFile(
                ServerOwnedContinuousLearningRegistryLeaf.AcceptedEvidence,
                accepted.Identity);
        }

        using var restartedAccess = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess();
        using var restartedCapability = restartedAccess.AcquireRegistryRootCapability();
        var restarted = new DomainModelRegistry(Storage, restartedCapability);
        Assert.Equal(2, restarted.GetAdapter(prior.Adapter.Domain).Value!.TrainingExamples);
        Assert.Equal(targetLength, new FileInfo(fixture.RegistryPath).Length);
        AssertStableNamespace(fixture);
    }

    [RetainedRegistryWindowsFact]
    public void MaximumAcceptedStagingFromInterruptedCommitFreshRestartRollsBack()
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        var prior = WriteExactLengthRegistry(
            fixture,
            DomainModelRegistry.MaximumRegistryBytes,
            "maximum-staging-prior.lora");
        var interrupted = false;

        using (var access = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess())
        using (var capability = access.AcquireRegistryRootCapability(
                   new ServerOwnedContinuousLearningRegistryRootCapabilityOptions
                   {
                       ObserveIoStageForTesting = stage =>
                       {
                           if (stage != ServerOwnedContinuousLearningRegistryIoStage
                                   .AfterAcceptedStagingDurable)
                           {
                               return;
                           }
                           interrupted = true;
                           GetRetainedRootHandle(
                               access,
                               ContinuousLearningRetainedObjectRole.RegistryRoot).Dispose();
                       },
                   }))
        {
            var registry = new DomainModelRegistry(Storage, capability);
            var result = registry.Register(prior.Adapter with { TrainingExamples = 2 });

            Assert.False(result.IsSuccess);
            Assert.Equal("REGISTRY_COMMIT_OUTCOME_INDETERMINATE", result.ErrorCode);
            Assert.True(interrupted);
            Assert.Equal(
                ServerOwnedContinuousLearningRegistryAcceptedEvidence.MaximumEncodedBytes,
                new FileInfo(fixture.AcceptedStagingPath).Length);
            Assert.False(File.Exists(fixture.AcceptedEvidencePath));
        }

        using var restartedAccess = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess();
        using var restartedCapability = restartedAccess.AcquireRegistryRootCapability();
        var restarted = new DomainModelRegistry(Storage, restartedCapability);
        Assert.Equal(1, restarted.GetAdapter(prior.Adapter.Domain).Value!.TrainingExamples);
        Assert.Equal(prior.Bytes, File.ReadAllBytes(fixture.RegistryPath));
        AssertStableNamespace(fixture);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void EscapeExpandedRegistryBeyondMaximumFailsClosedDuringPreflight(
        int schemaVersion)
    {
        const int jsonNewlineEncodedBytes = 2;
        var template = new ModelAdapter(
            "D",
            "",
            1,
            DateTimeOffset.UnixEpoch);
        var emptyEnvelopeBytes = SerializePreflightRegistry(
            schemaVersion,
            template).LongLength;
        var escapingCharacterCount = checked((int)(
            (DomainModelRegistry.MaximumRegistryBytes - emptyEnvelopeBytes) /
            jsonNewlineEncodedBytes +
            1));
        var adapter = template with
        {
            AdapterPath = new string('\n', escapingCharacterCount),
        };
        var rawFieldBytes = checked(
            Encoding.UTF8.GetByteCount(adapter.Domain) +
            Encoding.UTF8.GetByteCount(adapter.AdapterPath));
        var escapedValueBytes = checked(
            (long)escapingCharacterCount * jsonNewlineEncodedBytes);
        var escapedEnvelopeBytes = checked(emptyEnvelopeBytes + escapedValueBytes);

        Assert.True(rawFieldBytes < DomainModelRegistry.MaximumRegistryBytes);
        Assert.True(escapedEnvelopeBytes > DomainModelRegistry.MaximumRegistryBytes);

        var result = DomainModelRegistry.PreflightRegistrySerializationForTesting(
            schemaVersion,
            [adapter]);

        Assert.False(result.IsSuccess);
        Assert.Equal("REGISTRY_PERSISTENCE_FAILED", result.ErrorCode);
        Assert.Equal(default, result.Value);
    }

    [RetainedRegistryWindowsTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void AcceptedLeafOneByteOverMaximumFailsClosedWithoutMutation(
        bool committedLeaf)
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        var path = committedLeaf
            ? fixture.AcceptedEvidencePath
            : fixture.AcceptedStagingPath;
        using (var stream = new FileStream(
                   path,
                   FileMode.CreateNew,
                   System.IO.FileAccess.Write,
                   FileShare.None))
        {
            stream.SetLength(checked(
                ServerOwnedContinuousLearningRegistryAcceptedEvidence.MaximumEncodedBytes + 1));
            stream.Flush(flushToDisk: true);
        }
        var before = SnapshotLeaves(fixture);

        using var access = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireRegistryRootCapability();
        Assert.Throws<InvalidDataException>(() => new DomainModelRegistry(Storage, capability));

        Assert.Equal(before, SnapshotLeaves(fixture));
    }

    [RetainedRegistryWindowsFact]
    public void AcceptedBackupCleanupFailureCannotTriggerLateRollbackJournalCreation()
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        using (var seedAccess = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess())
        using (var seedCapability = seedAccess.AcquireRegistryRootCapability())
        {
            var seed = new DomainModelRegistry(Storage, seedCapability);
            Assert.True(seed.Register(fixture.Adapter("BUG", "cleanup-prior.lora", 1)).IsSuccess);
            Assert.Single(seed.ListAll().Value!);
        }

        var cleanupArmed = false;
        var backupDeleted = false;
        var cleanupRootFailureInjected = false;
        var forbiddenLateMarkerFlushAttempts = 0;
        using (var access = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess())
        using (var capability = access.AcquireRegistryRootCapability(
                   new ServerOwnedContinuousLearningRegistryRootCapabilityOptions
                   {
                       ObserveIoStageForTesting = stage =>
                       {
                           if (cleanupArmed &&
                               stage == ServerOwnedContinuousLearningRegistryIoStage
                                   .AfterAcceptedBackupDelete)
                           {
                               backupDeleted = true;
                           }
                       },
                       FailNativeCallForTesting = call =>
                       {
                           if (cleanupArmed && backupDeleted &&
                               call == ServerOwnedContinuousLearningRegistryNativeCall
                                   .RootDirectoryFlush &&
                               !cleanupRootFailureInjected)
                           {
                               cleanupRootFailureInjected = true;
                               return true;
                           }
                           if (cleanupRootFailureInjected &&
                               call == ServerOwnedContinuousLearningRegistryNativeCall.FileFlush)
                           {
                               forbiddenLateMarkerFlushAttempts++;
                               return true;
                           }
                           return false;
                       },
                   }))
        {
            var registry = new DomainModelRegistry(Storage, capability);
            var result = registry.Register(
                fixture.Adapter("BUG", "cleanup-accepted.lora", 2));
            Assert.True(result.IsSuccess, result.ErrorMessage);
            AssertRegistryMemory(registry, "BUG");
            cleanupArmed = true;

            var cleanup = registry.ListAll();

            Assert.False(cleanup.IsSuccess);
            Assert.Equal("REGISTRY_STATE_CORRUPT", cleanup.ErrorCode);
            AssertRegistryMemory(registry, "BUG");
            Assert.True(cleanupRootFailureInjected);
            Assert.Equal(0, forbiddenLateMarkerFlushAttempts);
            Assert.False(File.Exists(fixture.BackupPath));
            Assert.True(File.Exists(fixture.RollbackEvidencePath));
            Assert.True(File.Exists(fixture.AcceptedEvidencePath));
        }

        using var restartedAccess = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess();
        using var restartedCapability = restartedAccess.AcquireRegistryRootCapability();
        var restarted = new DomainModelRegistry(Storage, restartedCapability);
        Assert.Equal(2, restarted.GetAdapter("BUG").Value!.TrainingExamples);
        AssertStableNamespace(fixture);
    }

    [RetainedRegistryWindowsTheory]
    [InlineData(AcceptedCleanupInterruption.AfterBackupDelete)]
    [InlineData(AcceptedCleanupInterruption.AfterRollbackDelete)]
    [InlineData(AcceptedCleanupInterruption.AfterAcceptedDelete)]
    public void AcceptedCleanupInterruptionAlwaysRestartsToCommittedCandidate(
        AcceptedCleanupInterruption interruption)
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        using (var seedAccess = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess())
        using (var seedCapability = seedAccess.AcquireRegistryRootCapability())
        {
            var seed = new DomainModelRegistry(Storage, seedCapability);
            Assert.True(seed.Register(fixture.Adapter("BUG", "cleanup-stage-bug.lora", 1)).IsSuccess);
            Assert.Single(seed.ListAll().Value!);
            Assert.True(seed.Register(fixture.Adapter("PLAN", "cleanup-stage-plan.lora", 1)).IsSuccess);
            Assert.Equal(2, seed.ListAll().Value!.Count);
        }

        var cleanupArmed = false;
        using (var access = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess())
        using (var capability = access.AcquireRegistryRootCapability(
                   new ServerOwnedContinuousLearningRegistryRootCapabilityOptions
                   {
                       ObserveIoStageForTesting = stage =>
                       {
                           if (!cleanupArmed)
                               return;
                           var interrupt = interruption switch
                           {
                               AcceptedCleanupInterruption.AfterBackupDelete =>
                                   stage == ServerOwnedContinuousLearningRegistryIoStage
                                       .AfterAcceptedBackupDelete,
                               AcceptedCleanupInterruption.AfterRollbackDelete =>
                                   stage == ServerOwnedContinuousLearningRegistryIoStage
                                       .AfterAcceptedRollbackDelete,
                               AcceptedCleanupInterruption.AfterAcceptedDelete =>
                                   stage == ServerOwnedContinuousLearningRegistryIoStage
                                       .AfterAcceptedEvidenceDelete,
                               _ => false,
                           };
                           if (interrupt)
                           {
                               GetRetainedRootHandle(
                                   access,
                                   ContinuousLearningRetainedObjectRole.RegistryRoot).Dispose();
                           }
                       },
                   }))
        {
            var registry = new DomainModelRegistry(Storage, capability);
            Assert.True(registry.Register(
                fixture.Adapter("BUG", "cleanup-stage-accepted.lora", 2)).IsSuccess);
            AssertRegistryMemory(registry, "BUG", "PLAN");
            cleanupArmed = true;
            var cleanup = registry.ListAll();
            Assert.False(cleanup.IsSuccess);
            AssertRegistryMemory(registry, "BUG", "PLAN");
        }

        using var restartedAccess = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess();
        using var restartedCapability = restartedAccess.AcquireRegistryRootCapability();
        var restarted = new DomainModelRegistry(Storage, restartedCapability);
        Assert.Equal(2, restarted.GetAdapter("BUG").Value!.TrainingExamples);
        Assert.Equal(["BUG", "PLAN"], restarted.ListAll().Value!.Select(adapter => adapter.Domain));
        AssertStableNamespace(fixture);
    }

    [RetainedRegistryWindowsFact]
    public void AcceptedCleanupSurvivesTwoConsecutiveFreshOwnerInterruptions()
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        using (var seedAccess = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess())
        using (var seedCapability = seedAccess.AcquireRegistryRootCapability())
        {
            var seed = new DomainModelRegistry(Storage, seedCapability);
            Assert.True(seed.Register(fixture.Adapter("BUG", "double-prior.lora", 1)).IsSuccess);
            Assert.Single(seed.ListAll().Value!);
        }

        var firstCleanupArmed = false;
        using (var access = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess())
        using (var capability = access.AcquireRegistryRootCapability(
                   new ServerOwnedContinuousLearningRegistryRootCapabilityOptions
                   {
                       ObserveIoStageForTesting = stage =>
                       {
                           if (firstCleanupArmed &&
                               stage == ServerOwnedContinuousLearningRegistryIoStage
                                   .AfterAcceptedBackupDelete)
                           {
                               GetRetainedRootHandle(
                                   access,
                                   ContinuousLearningRetainedObjectRole.RegistryRoot).Dispose();
                           }
                       },
                   }))
        {
            var registry = new DomainModelRegistry(Storage, capability);
            Assert.True(registry.Register(
                fixture.Adapter("BUG", "double-accepted.lora", 2)).IsSuccess);
            firstCleanupArmed = true;
            Assert.False(registry.ListAll().IsSuccess);
        }

        using (var secondAccess = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess())
        using (var secondCapability = secondAccess.AcquireRegistryRootCapability(
                   new ServerOwnedContinuousLearningRegistryRootCapabilityOptions
                   {
                       ObserveIoStageForTesting = stage =>
                       {
                           if (stage == ServerOwnedContinuousLearningRegistryIoStage
                                   .AfterAcceptedRollbackDelete)
                           {
                               GetRetainedRootHandle(
                                   secondAccess,
                                   ContinuousLearningRetainedObjectRole.RegistryRoot).Dispose();
                           }
                       },
                   }))
        {
            Assert.Throws<InvalidDataException>(
                () => new DomainModelRegistry(Storage, secondCapability));
        }

        using var finalAccess = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess();
        using var finalCapability = finalAccess.AcquireRegistryRootCapability();
        var final = new DomainModelRegistry(Storage, finalCapability);
        Assert.Equal(2, final.GetAdapter("BUG").Value!.TrainingExamples);
        AssertStableNamespace(fixture);
    }

    [RetainedRegistryWindowsFact]
    public void RetainedRootLossIsStickyAndDoesNotUpdateRegistryMemory()
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        var armed = false;
        using var access = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireRegistryRootCapability(
            new ServerOwnedContinuousLearningRegistryRootCapabilityOptions
            {
                ObserveIoStageForTesting = stage =>
                {
                    if (armed && stage == ServerOwnedContinuousLearningRegistryIoStage
                            .AfterOperationBeforeRootRevalidation)
                    {
                        GetRetainedRootHandle(
                            access,
                            ContinuousLearningRetainedObjectRole.RegistryRoot).Dispose();
                    }
                },
            });
        var registry = new DomainModelRegistry(Storage, capability);
        armed = true;

        var result = registry.Register(fixture.Adapter("BUG", "revoked.lora", 1));

        Assert.False(result.IsSuccess);
        Assert.Equal("REGISTRY_PERSISTENCE_FAILED", result.ErrorCode);
        Assert.False(File.Exists(fixture.RegistryPath));
        AssertRegistryMemory(registry);
        Assert.Throws<ContinuousLearningPathIdentityException>(capability.ReadCanonical);
        Assert.Throws<ContinuousLearningPathIdentityException>(access.EnsureRetained);
    }

    [RetainedRegistryWindowsFact]
    public void RetainedOwnerLockExplicitlyExcludesLegacyRegistryProtocol()
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        using var access = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireRegistryRootCapability();
        var retained = new DomainModelRegistry(Storage, capability);
        Assert.Equal(10, RunCrossProcessOwnerLockProbe(fixture.FileAccess.RegistryRoot));

        var exception = Assert.Throws<InvalidDataException>(() =>
            new DomainModelRegistry(
                Storage,
                fixture.FileAccess.RegistryRoot,
                beforeFinalPublicationValidation: null,
                processLeaseTimeout: TimeSpan.FromMilliseconds(150)));

        Assert.StartsWith("REGISTRY_STATE_CORRUPT:", exception.Message, StringComparison.Ordinal);
        Assert.True(retained.Register(fixture.Adapter("BUG", "exclusive.lora", 1)).IsSuccess);
        Assert.NotNull(retained.GetAdapter("BUG").Value);
        AssertStableNamespace(fixture);
    }

    [RetainedRegistryWindowsFact]
    public void LegacyOwnerLockExcludesRetainedAcquisitionInProcessAndAcrossProcess()
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        Exception? retainedAcquisitionFailure = null;
        int? childProbeExitCode = null;
        var legacy = new DomainModelRegistry(
            Storage,
            fixture.FileAccess.RegistryRoot,
            () =>
            {
                childProbeExitCode ??= RunCrossProcessOwnerLockProbe(
                    fixture.FileAccess.RegistryRoot);
                retainedAcquisitionFailure ??= Record.Exception(() =>
                {
                    using var retainedAccess = fixture.FileAccess.CreateStartup()
                        .AcquireRetainedFileAccess();
                    using var retainedCapability = retainedAccess.AcquireRegistryRootCapability();
                });
            });

        var result = legacy.Register(fixture.Adapter("BUG", "legacy-exclusive.lora", 1));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.IsAssignableFrom<IOException>(retainedAcquisitionFailure);
        Assert.Equal(10, childProbeExitCode);
        Assert.Equal(0, RunCrossProcessOwnerLockProbe(fixture.FileAccess.RegistryRoot));
        using var access = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireRegistryRootCapability();
        var retained = new DomainModelRegistry(Storage, capability);
        Assert.NotNull(retained.GetAdapter("BUG").Value);
    }

    [RetainedRegistryWindowsFact]
    public void RetainedReferenceResolvesExactArtifactAndSurvivesFreshRestart()
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        RetainedRegistryPublishedAdapter published;
        byte[] expectedBytes;

        using (var access = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess())
        using (var registryRoot = access.AcquireRegistryRootCapability())
        using (var adapterRoot = access.AcquireAdapterRootCapability())
        {
            published = fixture.PublishRetainedAdapter(
                adapterRoot,
                "BUG",
                trainingExamples: 7,
                publicationId: "restart");
            expectedBytes = File.ReadAllBytes(Path.Combine(
                fixture.FileAccess.AdapterRoot,
                published.Reference.CanonicalLeafName));
            PublishExactRegistryBytes(
                registryRoot,
                SerializeVersion2Adapters(published));

            var registry = new DomainModelRegistry(
                Storage,
                registryRoot,
                adapterRoot);
            AssertResolvedRetained(registry, published, expectedBytes);
        }

        using var restartedAccess = fixture.FileAccess.CreateStartup()
            .AcquireRetainedFileAccess();
        using var restartedRegistryRoot = restartedAccess.AcquireRegistryRootCapability();
        using var restartedAdapterRoot = restartedAccess.AcquireAdapterRootCapability();
        var restarted = new DomainModelRegistry(
            Storage,
            restartedRegistryRoot,
            restartedAdapterRoot);
        AssertResolvedRetained(restarted, published, expectedBytes);
    }

    [RetainedRegistryWindowsFact]
    public void RetainedStateQuarantinesPathAndRegistryOnlyConstructionWithoutMutation()
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        byte[] registryBytes;
        using (var access = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess())
        using (var registryRoot = access.AcquireRegistryRootCapability())
        using (var adapterRoot = access.AcquireAdapterRootCapability())
        {
            var published = fixture.PublishRetainedAdapter(
                adapterRoot,
                "BUG",
                trainingExamples: 1,
                publicationId: "quarantine");
            registryBytes = SerializeVersion2Adapters(published);
            PublishExactRegistryBytes(registryRoot, registryBytes);
        }
        var before = SnapshotLeaves(fixture);

        var pathException = Assert.Throws<InvalidDataException>(() =>
            new DomainModelRegistry(Storage, fixture.FileAccess.RegistryRoot));
        Assert.StartsWith(
            "ADAPTER_REFERENCE_RESOLVER_REQUIRED:",
            pathException.Message,
            StringComparison.Ordinal);
        Assert.Equal(registryBytes, File.ReadAllBytes(fixture.RegistryPath));
        Assert.Equal(before, SnapshotLeaves(fixture));

        using var lateFixture = new RetainedRegistryFixture();
        using var lateAccess = lateFixture.FileAccess.CreateStartup()
            .AcquireRetainedFileAccess();
        using var lateRegistryRoot = lateAccess.AcquireRegistryRootCapability();
        using var lateAdapterRoot = lateAccess.AcquireAdapterRootCapability();
        var latePublished = lateFixture.PublishRetainedAdapter(
            lateAdapterRoot,
            "BUG",
            trainingExamples: 1,
            publicationId: "late-reference");
        var registryWithoutResolver = new DomainModelRegistry(Storage, lateRegistryRoot);
        PublishExactRegistryBytes(
            lateRegistryRoot,
            SerializeVersion2Adapters(latePublished));
        var resolverRequired = registryWithoutResolver.ResolveRetainedAdapter("BUG");
        Assert.False(resolverRequired.IsSuccess);
        Assert.Equal(
            "ADAPTER_REFERENCE_RESOLVER_REQUIRED",
            resolverRequired.ErrorCode);

        using var retainedAccess = fixture.FileAccess.CreateStartup()
            .AcquireRetainedFileAccess();
        using var retainedRegistryRoot = retainedAccess.AcquireRegistryRootCapability();
        var capabilityException = Assert.Throws<InvalidDataException>(() =>
            new DomainModelRegistry(Storage, retainedRegistryRoot));
        Assert.StartsWith(
            "ADAPTER_REFERENCE_RESOLVER_REQUIRED:",
            capabilityException.Message,
            StringComparison.Ordinal);
        Assert.Equal(registryBytes, File.ReadAllBytes(fixture.RegistryPath));
        Assert.Equal(before, SnapshotLeaves(fixture));
    }

    [RetainedRegistryWindowsTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void MixedRetainedQuarantinePrecedesLegacyPathValidationForBothConstructors(
        bool hostilePath)
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        byte[] registryBytes;
        string retainedArtifactPath;
        byte[] retainedArtifactBytes;
        var legacy = fixture.Adapter("PLAN", "quarantine-legacy.lora", 1);
        var invalidLegacyPath = hostilePath
            ? fixture.FileAccess.Root + Path.DirectorySeparatorChar + "\0hostile.lora"
            : Path.Combine(
                fixture.FileAccess.Root,
                "missing-directory",
                "missing.lora");
        legacy = legacy with { AdapterPath = invalidLegacyPath };
        using (var access = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess())
        using (var registryRoot = access.AcquireRegistryRootCapability())
        using (var adapterRoot = access.AcquireAdapterRootCapability())
        {
            var retained = fixture.PublishRetainedAdapter(
                adapterRoot,
                "BUG",
                trainingExamples: 1,
                publicationId: hostilePath ? "hostile-path" : "missing-path");
            retainedArtifactPath = Path.Combine(
                fixture.FileAccess.AdapterRoot,
                retained.Reference.CanonicalLeafName);
            retainedArtifactBytes = File.ReadAllBytes(retainedArtifactPath);
            registryBytes = SerializeVersion2Adapters(retained, legacy);
            PublishExactRegistryBytes(registryRoot, registryBytes);
        }
        var before = SnapshotLeaves(fixture);

        AssertResolverRequired(() => new DomainModelRegistry(
            Storage,
            fixture.FileAccess.RegistryRoot));
        AssertQuarantineUnchanged();

        using var retainedAccess = fixture.FileAccess.CreateStartup()
            .AcquireRetainedFileAccess();
        using var retainedRegistryRoot = retainedAccess.AcquireRegistryRootCapability();
        AssertResolverRequired(() => new DomainModelRegistry(
            Storage,
            retainedRegistryRoot));
        AssertQuarantineUnchanged();

        void AssertResolverRequired(Action construct)
        {
            var exception = Assert.Throws<InvalidDataException>(construct);
            Assert.StartsWith(
                "ADAPTER_REFERENCE_RESOLVER_REQUIRED:",
                exception.Message,
                StringComparison.Ordinal);
        }

        void AssertQuarantineUnchanged()
        {
            Assert.Equal(registryBytes, File.ReadAllBytes(fixture.RegistryPath));
            Assert.Equal(retainedArtifactBytes, File.ReadAllBytes(retainedArtifactPath));
            Assert.Equal(before, SnapshotLeaves(fixture));
        }
    }

    [RetainedRegistryWindowsFact]
    public void MixedRegistryLegacyApisFailClosedAndRegistrationPreservesExactBytes()
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        using var access = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess();
        using var registryRoot = access.AcquireRegistryRootCapability();
        using var adapterRoot = access.AcquireAdapterRootCapability();
        var retained = fixture.PublishRetainedAdapter(
            adapterRoot,
            "BUG",
            trainingExamples: 2,
            publicationId: "mixed");
        var legacy = fixture.Adapter("PLAN", "mixed-legacy.lora", 3);
        var exactBytes = SerializeVersion2Adapters(retained, legacy);
        PublishExactRegistryBytes(registryRoot, exactBytes);
        var registry = new DomainModelRegistry(Storage, registryRoot, adapterRoot);

        var retainedGet = registry.GetAdapter("BUG");
        Assert.False(retainedGet.IsSuccess);
        Assert.Equal("ADAPTER_REFERENCE_RESOLVER_REQUIRED", retainedGet.ErrorCode);
        var legacyGet = registry.GetAdapter("PLAN");
        Assert.True(legacyGet.IsSuccess, legacyGet.ErrorMessage);
        Assert.Equal(legacy.Domain, legacyGet.Value!.Domain);
        Assert.Equal(legacy.AdapterPath, legacyGet.Value.AdapterPath);
        Assert.Equal(legacy.TrainingExamples, legacyGet.Value.TrainingExamples);
        Assert.Equal(
            JsonSerializer.Serialize(legacy.Provenance),
            JsonSerializer.Serialize(legacyGet.Value.Provenance));
        Assert.Null(registry.GetAdapter("MISSING").Value);
        var listed = registry.ListAll();
        Assert.False(listed.IsSuccess);
        Assert.Equal("ADAPTER_REFERENCE_RESOLVER_REQUIRED", listed.ErrorCode);

        var legacyResolution = registry.ResolveRetainedAdapter("PLAN");
        Assert.False(legacyResolution.IsSuccess);
        Assert.Equal("ADAPTER_REFERENCE_NOT_RETAINED", legacyResolution.ErrorCode);
        var missingResolution = registry.ResolveRetainedAdapter("MISSING");
        Assert.True(missingResolution.IsSuccess, missingResolution.ErrorMessage);
        Assert.Null(missingResolution.Value);
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        var invalidDomain = registry.ResolveRetainedAdapter("", cancelled.Token);
        Assert.False(invalidDomain.IsSuccess);
        Assert.Equal("INVALID_ADAPTER_DOMAIN", invalidDomain.ErrorCode);
        AssertResolvedRetained(
            registry,
            retained,
            File.ReadAllBytes(Path.Combine(
                fixture.FileAccess.AdapterRoot,
                retained.Reference.CanonicalLeafName)));

        var registration = registry.Register(
            fixture.Adapter("NEW", "mixed-new.lora", 4));
        Assert.False(registration.IsSuccess);
        Assert.Equal(
            "REGISTRY_RETAINED_REFERENCE_WRITE_UNSUPPORTED",
            registration.ErrorCode);
        Assert.Equal(exactBytes, File.ReadAllBytes(fixture.RegistryPath));
        AssertRegistryMemory(registry, "PLAN");
        AssertStableNamespace(fixture);
    }

    [RetainedRegistryWindowsFact]
    public void WrongRootAndIdenticallyNamedOutsideArtifactAreUntrusted()
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        using var foreignFixture = new RetainedRegistryFixture();
        RetainedRegistryPublishedAdapter published;
        byte[] registryBytes;
        using (var access = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess())
        using (var registryRoot = access.AcquireRegistryRootCapability())
        using (var adapterRoot = access.AcquireAdapterRootCapability())
        {
            published = fixture.PublishRetainedAdapter(
                adapterRoot,
                "BUG",
                trainingExamples: 1,
                publicationId: "authority");
            registryBytes = SerializeVersion2Adapters(published);
            PublishExactRegistryBytes(registryRoot, registryBytes);
        }

        var actualPath = Path.Combine(
            fixture.FileAccess.AdapterRoot,
            published.Reference.CanonicalLeafName);
        var foreignPath = Path.Combine(
            foreignFixture.FileAccess.AdapterRoot,
            published.Reference.CanonicalLeafName);
        File.Copy(actualPath, foreignPath);
        using (var registryAccess = fixture.FileAccess.CreateStartup()
                   .AcquireRetainedFileAccess())
        using (var registryRoot = registryAccess.AcquireRegistryRootCapability())
        using (var foreignAccess = foreignFixture.FileAccess.CreateStartup()
                   .AcquireRetainedFileAccess())
        using (var foreignAdapterRoot = foreignAccess.AcquireAdapterRootCapability())
        {
            var wrongRoot = new DomainModelRegistry(
                Storage,
                registryRoot,
                foreignAdapterRoot);
            AssertUntrusted(wrongRoot.ResolveRetainedAdapter("BUG"));
        }

        var outsidePath = Path.Combine(
            fixture.FileAccess.Root,
            published.Reference.CanonicalLeafName);
        File.Copy(actualPath, outsidePath);
        File.Delete(actualPath);
        using var missingAccess = fixture.FileAccess.CreateStartup()
            .AcquireRetainedFileAccess();
        using var missingRegistryRoot = missingAccess.AcquireRegistryRootCapability();
        using var missingAdapterRoot = missingAccess.AcquireAdapterRootCapability();
        var missing = new DomainModelRegistry(
            Storage,
            missingRegistryRoot,
            missingAdapterRoot);
        AssertUntrusted(missing.ResolveRetainedAdapter("BUG"));
        Assert.True(File.Exists(outsidePath));
        Assert.Equal(registryBytes, File.ReadAllBytes(fixture.RegistryPath));
    }

    [RetainedRegistryWindowsFact]
    public void ForgedArtifactIdentityAndRevokedAdapterCapabilityAreUntrusted()
    {
        RequireWindows();
        using (var fixture = new RetainedRegistryFixture())
        {
            using var access = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess();
            using var registryRoot = access.AcquireRegistryRootCapability();
            using var adapterRoot = access.AcquireAdapterRootCapability();
            var published = fixture.PublishRetainedAdapter(
                adapterRoot,
                "BUG",
                trainingExamples: 1,
                publicationId: "forged");
            var identity = published.Reference.FinalArtifactIdentity;
            var forged = published with
            {
                Reference = published.Reference with
                {
                    FinalArtifactIdentity =
                        new ServerOwnedContinuousLearningAdapterArtifactIdentity(
                            identity.VolumeSerialNumber,
                            identity.FileIdLow == ulong.MaxValue
                                ? identity.FileIdLow - 1
                                : identity.FileIdLow + 1,
                            identity.FileIdHigh),
                },
            };
            PublishExactRegistryBytes(
                registryRoot,
                SerializeVersion2Adapters(forged));
            var registry = new DomainModelRegistry(Storage, registryRoot, adapterRoot);
            AssertUntrusted(registry.ResolveRetainedAdapter("BUG"));
        }

        using var revokedFixture = new RetainedRegistryFixture();
        using var revokedAccess = revokedFixture.FileAccess.CreateStartup()
            .AcquireRetainedFileAccess();
        using var revokedRegistryRoot = revokedAccess.AcquireRegistryRootCapability();
        var revokedAdapterRoot = revokedAccess.AcquireAdapterRootCapability();
        var retained = revokedFixture.PublishRetainedAdapter(
            revokedAdapterRoot,
            "BUG",
            trainingExamples: 1,
            publicationId: "revoked");
        PublishExactRegistryBytes(
            revokedRegistryRoot,
            SerializeVersion2Adapters(retained));
        var revoked = new DomainModelRegistry(
            Storage,
            revokedRegistryRoot,
            revokedAdapterRoot);
        revokedAdapterRoot.Dispose();
        AssertUntrusted(revoked.ResolveRetainedAdapter("BUG"));
    }

    [RetainedRegistryWindowsFact]
    public void RegistryChangeOrFinalReadFailureRejectsResolvedSnapshot()
    {
        RequireWindows();
        AssertFinalRevalidation(changeToCorruptBytes: false);
        AssertFinalRevalidation(changeToCorruptBytes: true);

        static void AssertFinalRevalidation(bool changeToCorruptBytes)
        {
            using var fixture = new RetainedRegistryFixture();
            using var access = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess();
            using var registryRoot = access.AcquireRegistryRootCapability();
            var changed = false;
            RetainedRegistryPublishedAdapter? published = null;
            using var adapterRoot = access.AcquireAdapterRootCapability(
                new ServerOwnedContinuousLearningAdapterRootCapabilityOptions
                {
                    ObserveIoStageForTesting = observation =>
                    {
                        if (changed || observation.Stage !=
                            ServerOwnedContinuousLearningAdapterIoStage.DuringExactRead)
                        {
                            return;
                        }
                        changed = true;
                        File.WriteAllBytes(
                            fixture.RegistryPath,
                            changeToCorruptBytes
                                ? [0xde, 0xad]
                                : SerializeVersion2Adapters(
                                    published! with
                                    {
                                        TrainingExamples =
                                            published!.TrainingExamples + 1,
                                    }));
                    },
                });
            published = fixture.PublishRetainedAdapter(
                adapterRoot,
                "BUG",
                trainingExamples: 1,
                publicationId: changeToCorruptBytes ? "corrupt" : "changed");
            PublishExactRegistryBytes(
                registryRoot,
                SerializeVersion2Adapters(published));
            var registry = new DomainModelRegistry(Storage, registryRoot, adapterRoot);

            var result = registry.ResolveRetainedAdapter("BUG");

            Assert.False(result.IsSuccess);
            Assert.Equal(
                "ADAPTER_REFERENCE_CHANGED_DURING_RESOLUTION",
                result.ErrorCode);
            Assert.Null(result.Value);
            Assert.True(changed);
        }
    }

    [RetainedRegistryWindowsFact]
    public async Task ResolutionHonoursCancellationRunsConcurrentlyAndReleasesRegistryGate()
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        using var access = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess();
        using var registryRoot = access.AcquireRegistryRootCapability();
        DomainModelRegistry? registry = null;
        var gateWasAvailable = 0;
        var probeAdmission = 0;
        using var gateEntered = new ManualResetEventSlim();
        Thread? gateProbe = null;
        using var adapterRoot = access.AcquireAdapterRootCapability(
            new ServerOwnedContinuousLearningAdapterRootCapabilityOptions
            {
                ObserveIoStageForTesting = observation =>
                {
                    if (observation.Stage !=
                            ServerOwnedContinuousLearningAdapterIoStage.DuringExactRead ||
                        Interlocked.CompareExchange(ref probeAdmission, 1, 0) != 0)
                    {
                        return;
                    }
                    var gate = typeof(DomainModelRegistry)
                        .GetField("_pathGate", BindingFlags.Instance | BindingFlags.NonPublic)!
                        .GetValue(registry!);
                    var probe = new Thread(() =>
                    {
                        lock (gate!)
                        {
                            Volatile.Write(ref gateWasAvailable, 1);
                            gateEntered.Set();
                        }
                    })
                    {
                        IsBackground = true,
                        Name = "retained-registry-path-gate-probe",
                    };
                    Volatile.Write(ref gateProbe, probe);
                    probe.Start();
                },
            });
        var published = fixture.PublishRetainedAdapter(
            adapterRoot,
            "BUG",
            trainingExamples: 1,
            publicationId: "concurrent");
        PublishExactRegistryBytes(
            registryRoot,
            SerializeVersion2Adapters(published));
        registry = new DomainModelRegistry(Storage, registryRoot, adapterRoot);
        using var alreadyCancelled = new CancellationTokenSource();
        alreadyCancelled.Cancel();
        Assert.Throws<OperationCanceledException>(() =>
            registry.ResolveRetainedAdapter("BUG", alreadyCancelled.Token));

        var results = await Task.WhenAll(
            Task.Run(() => registry.ResolveRetainedAdapter("BUG")),
            Task.Run(() => registry.ResolveRetainedAdapter("BUG")));

        var probe = Volatile.Read(ref gateProbe);
        Assert.NotNull(probe);
        var signaled = gateEntered.Wait(TimeSpan.FromSeconds(10));
        var joined = probe.Join(TimeSpan.FromSeconds(10));
        Assert.True(signaled, "The dedicated path-gate probe did not enter in time.");
        Assert.True(joined, "The dedicated path-gate probe did not terminate in time.");
        Assert.All(results, result => Assert.True(result.IsSuccess, result.ErrorMessage));
        Assert.Equal(1, Volatile.Read(ref probeAdmission));
        Assert.Equal(1, Volatile.Read(ref gateWasAvailable));
    }

    [RetainedRegistryWindowsFact]
    public void InFlightExactReadCancellationPropagatesWithoutPartialResult()
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        using var cancellation = new CancellationTokenSource();
        using var access = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess();
        using var registryRoot = access.AcquireRegistryRootCapability();
        using var adapterRoot = access.AcquireAdapterRootCapability(
            new ServerOwnedContinuousLearningAdapterRootCapabilityOptions
            {
                ObserveIoStageForTesting = observation =>
                {
                    if (observation.Stage ==
                        ServerOwnedContinuousLearningAdapterIoStage.DuringExactRead)
                    {
                        cancellation.Cancel();
                    }
                },
            });
        var published = fixture.PublishRetainedAdapter(
            adapterRoot,
            "BUG",
            trainingExamples: 1,
            publicationId: "cancel");
        PublishExactRegistryBytes(
            registryRoot,
            SerializeVersion2Adapters(published));
        var registry = new DomainModelRegistry(Storage, registryRoot, adapterRoot);

        Assert.Throws<OperationCanceledException>(() =>
            registry.ResolveRetainedAdapter("BUG", cancellation.Token));
    }

    [RetainedRegistryWindowsFact]
    public void CancellationDuringFinalRegistryRevalidationPreventsPartialSuccess()
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        using var cancellation = new CancellationTokenSource();
        var finalReadArmed = false;
        var finalRevalidationObserved = false;
        using var access = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess();
        using var registryRoot = access.AcquireRegistryRootCapability(
            new ServerOwnedContinuousLearningRegistryRootCapabilityOptions
            {
                ObserveIoStageForTesting = stage =>
                {
                    if (finalReadArmed
                        && !finalRevalidationObserved
                        && stage == ServerOwnedContinuousLearningRegistryIoStage
                            .AfterOperationBeforeRootRevalidation)
                    {
                        finalRevalidationObserved = true;
                        cancellation.Cancel();
                    }
                },
            });
        using var adapterRoot = access.AcquireAdapterRootCapability(
            new ServerOwnedContinuousLearningAdapterRootCapabilityOptions
            {
                ObserveIoStageForTesting = observation =>
                {
                    if (observation.Stage ==
                        ServerOwnedContinuousLearningAdapterIoStage.DuringExactRead)
                    {
                        finalReadArmed = true;
                    }
                },
            });
        var published = fixture.PublishRetainedAdapter(
            adapterRoot,
            "BUG",
            trainingExamples: 1,
            publicationId: "cancel-final");
        PublishExactRegistryBytes(
            registryRoot,
            SerializeVersion2Adapters(published));
        var registry = new DomainModelRegistry(Storage, registryRoot, adapterRoot);

        Assert.Throws<OperationCanceledException>(() =>
            registry.ResolveRetainedAdapter("BUG", cancellation.Token));
        Assert.True(finalReadArmed);
        Assert.True(finalRevalidationObserved);
    }

    [RetainedRegistryWindowsFact]
    public void RecoveryComparisonRejectsTwoRetainedDomainChangesWithoutMutation()
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        RetainedRegistryPublishedAdapter bug;
        RetainedRegistryPublishedAdapter plan;
        using (var access = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess())
        using (var registryRoot = access.AcquireRegistryRootCapability())
        using (var adapterRoot = access.AcquireAdapterRootCapability())
        {
            bug = fixture.PublishRetainedAdapter(adapterRoot, "BUG", 1, "recovery-bug");
            plan = fixture.PublishRetainedAdapter(adapterRoot, "PLAN", 1, "recovery-plan");
            PublishExactRegistryBytes(
                registryRoot,
                SerializeVersion2Adapters(bug, plan));
        }
        File.WriteAllBytes(
            fixture.TempPath,
            SerializeVersion2Adapters(
                bug with { TrainingExamples = 2 },
                plan with { TrainingExamples = 2 }));
        var before = SnapshotLeaves(fixture);

        using var restartedAccess = fixture.FileAccess.CreateStartup()
            .AcquireRetainedFileAccess();
        using var restartedRegistryRoot = restartedAccess.AcquireRegistryRootCapability();
        using var restartedAdapterRoot = restartedAccess.AcquireAdapterRootCapability();
        Assert.Throws<InvalidDataException>(() => new DomainModelRegistry(
            Storage,
            restartedRegistryRoot,
            restartedAdapterRoot));
        Assert.Equal(before, SnapshotLeaves(fixture));
    }

    [RetainedRegistryWindowsTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void RecoveryBranchPreflightFindsRetainedTempBeforeCanonicalLegacyPathIo(
        bool hostilePath)
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        using var access = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess();
        using var registryRoot = access.AcquireRegistryRootCapability();
        using var adapterRoot = access.AcquireAdapterRootCapability();
        var legacy = fixture.Adapter("BUG", "recovery-preflight-legacy.lora", 1);
        legacy = legacy with
        {
            AdapterPath = hostilePath
                ? fixture.FileAccess.Root + Path.DirectorySeparatorChar + "\0hostile.lora"
                : Path.Combine(
                    fixture.FileAccess.Root,
                    "missing-recovery-directory",
                    "missing.lora"),
        };
        var retained = fixture.PublishRetainedAdapter(
            adapterRoot,
            "PLAN",
            trainingExamples: 1,
            publicationId: hostilePath
                ? "recovery-hostile-path"
                : "recovery-missing-path");
        var retainedArtifactPath = Path.Combine(
            fixture.FileAccess.AdapterRoot,
            retained.Reference.CanonicalLeafName);
        var retainedArtifactBytes = File.ReadAllBytes(retainedArtifactPath);
        PublishExactRegistryBytes(
            registryRoot,
            SerializeVersion2Adapters(legacy));
        File.WriteAllBytes(
            fixture.TempPath,
            SerializeVersion2Adapters(legacy, retained));
        var before = registryRoot.Inventory();

        var exception = Assert.Throws<InvalidDataException>(() =>
            new DomainModelRegistry(Storage, registryRoot));

        Assert.StartsWith(
            "ADAPTER_REFERENCE_RESOLVER_REQUIRED:",
            exception.Message,
            StringComparison.Ordinal);
        AssertRegistryInventoryMatches(before, registryRoot.Inventory());
        Assert.Equal(retainedArtifactBytes, File.ReadAllBytes(retainedArtifactPath));
    }

    [RetainedRegistryWindowsTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void AcceptedAndRollbackRecoveryPreflightAllRelevantEmbeddedStates(
        bool acceptedBranch)
    {
        RequireWindows();
        using var fixture = new RetainedRegistryFixture();
        using var access = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess();
        using var registryRoot = access.AcquireRegistryRootCapability();
        using var adapterRoot = access.AcquireAdapterRootCapability();
        var legacy = fixture.Adapter("BUG", "embedded-preflight-legacy.lora", 1) with
        {
            AdapterPath = fixture.FileAccess.Root
                          + Path.DirectorySeparatorChar
                          + "\0embedded-hostile.lora",
        };
        var retained = fixture.PublishRetainedAdapter(
            adapterRoot,
            "BUG",
            trainingExamples: 1,
            publicationId: acceptedBranch
                ? "accepted-embedded-prior"
                : "rollback-rejected");
        var retainedArtifactPath = Path.Combine(
            fixture.FileAccess.AdapterRoot,
            retained.Reference.CanonicalLeafName);
        var retainedArtifactBytes = File.ReadAllBytes(retainedArtifactPath);

        var priorBytes = acceptedBranch
            ? SerializeVersion2Adapters(retained)
            : SerializeVersion2Adapters(legacy);
        var candidateBytes = acceptedBranch
            ? SerializeVersion2Adapters(legacy)
            : SerializeVersion2Adapters(retained);
        PublishExactRegistryBytes(registryRoot, priorBytes);
        StagePendingRegistryPublication(
            registryRoot,
            candidateBytes,
            commitAccepted: acceptedBranch);
        var before = registryRoot.Inventory();

        var exception = Assert.Throws<InvalidDataException>(() =>
            new DomainModelRegistry(Storage, registryRoot));

        Assert.StartsWith(
            "ADAPTER_REFERENCE_RESOLVER_REQUIRED:",
            exception.Message,
            StringComparison.Ordinal);
        AssertRegistryInventoryMatches(before, registryRoot.Inventory());
        Assert.Equal(retainedArtifactBytes, File.ReadAllBytes(retainedArtifactPath));
    }

    [Fact]
    public void ResolvedArtifactSurfaceExposesNoFilesystemOrMutableByteAuthority()
    {
        var type = typeof(ResolvedRetainedAdapterArtifact);
        Assert.False(typeof(IDisposable).IsAssignableFrom(type));
        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        Assert.Equal(
            [
                typeof(string),
                typeof(int),
                typeof(DateTimeOffset),
                typeof(ModelAdapterProvenance),
                typeof(AdapterArtifactReference),
                typeof(ServerOwnedContinuousLearningAdapterSnapshot),
            ],
            properties.Select(property => property.PropertyType));
        Assert.DoesNotContain(properties, property =>
            property.PropertyType == typeof(byte[])
            || typeof(Stream).IsAssignableFrom(property.PropertyType)
            || typeof(SafeFileHandle).IsAssignableFrom(property.PropertyType)
            || property.Name.Contains("Path", StringComparison.OrdinalIgnoreCase)
            || property.Name.Contains("Handle", StringComparison.OrdinalIgnoreCase));
    }

    private static byte[] CreatePendingRollbackEvidence(
        RetainedRegistryFixture fixture,
        string filePrefix)
    {
        byte[] priorBytes;
        using (var seedAccess = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess())
        using (var seedCapability = seedAccess.AcquireRegistryRootCapability())
        {
            var seed = new DomainModelRegistry(Storage, seedCapability);
            Assert.True(seed.Register(
                fixture.Adapter("BUG", $"{filePrefix}-prior.lora", 1)).IsSuccess);
            priorBytes = File.ReadAllBytes(fixture.RegistryPath);
        }

        using var access = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireRegistryRootCapability(
            new ServerOwnedContinuousLearningRegistryRootCapabilityOptions
            {
                ObserveIoStageForTesting = stage =>
                {
                    if (stage == ServerOwnedContinuousLearningRegistryIoStage
                            .AfterRollbackEvidenceDurable)
                    {
                        GetRetainedRootHandle(
                            access,
                            ContinuousLearningRetainedObjectRole.RegistryRoot).Dispose();
                    }
                },
            });
        var validation = 0;
        var registry = new DomainModelRegistry(
            Storage,
            capability,
            () =>
            {
                validation++;
                if (validation == 2)
                    throw new InvalidDataException("Injected late rejection.");
            });
        var result = registry.Register(
            fixture.Adapter("BUG", $"{filePrefix}-rejected.lora", 2));
        Assert.False(result.IsSuccess);
        Assert.True(File.Exists(fixture.RollbackEvidencePath));
        return priorBytes;
    }

    private static void CreatePendingAcceptedEvidence(
        RetainedRegistryFixture fixture,
        string filePrefix)
    {
        using var access = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireRegistryRootCapability();
        var registry = new DomainModelRegistry(Storage, capability);
        var result = registry.Register(
            fixture.Adapter("BUG", $"{filePrefix}.lora", 1));
        Assert.True(result.IsSuccess, result.ErrorMessage);
        AssertRegistryMemory(registry, "BUG");
        Assert.True(File.Exists(fixture.RollbackEvidencePath));
        Assert.True(File.Exists(fixture.AcceptedEvidencePath));
    }

    private static void AssertRestartRejectedWithoutLeafMutation(
        RetainedRegistryFixture fixture)
    {
        var before = SnapshotLeaves(fixture);
        using var access = fixture.FileAccess.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireRegistryRootCapability();
        Assert.Throws<InvalidDataException>(() => new DomainModelRegistry(Storage, capability));
        Assert.Equal(before, SnapshotLeaves(fixture));
    }

    private static void AssertCanonicalDomainOrder(byte[] bytes, params string[] expected)
    {
        using var document = JsonDocument.Parse(bytes);
        Assert.Equal(
            expected,
            document.RootElement.GetProperty("Adapters")
                .EnumerateArray()
                .Select(value => value.GetProperty("Domain").GetString())
                .ToArray());
    }

    private static (ModelAdapter Adapter, byte[] Bytes) WriteExactLengthRegistry(
        RetainedRegistryFixture fixture,
        long targetLength,
        string adapterFileName)
    {
        var template = fixture.Adapter("D", adapterFileName, 1);
        var minimum = SerializeRegistry(template);
        var domainLength = checked((int)(targetLength - minimum.LongLength + 1));
        Assert.True(domainLength > 0);
        var adapter = template with { Domain = new string('D', domainLength) };
        var bytes = SerializeRegistry(adapter);
        Assert.Equal(targetLength, bytes.LongLength);
        File.WriteAllBytes(fixture.RegistryPath, bytes);
        return (adapter, bytes);
    }

    private static byte[] SerializeRegistry(ModelAdapter adapter)
        => JsonSerializer.SerializeToUtf8Bytes(new
        {
            SchemaVersion = 1,
            Adapters = new[] { adapter },
        });

    private static byte[] SerializePreflightRegistry(
        int schemaVersion,
        ModelAdapter adapter)
        => schemaVersion switch
        {
            1 => SerializeRegistry(adapter),
            2 => JsonSerializer.SerializeToUtf8Bytes(new
            {
                SchemaVersion = 2,
                Adapters = new[]
                {
                    new
                    {
                        adapter.Domain,
                        adapter.TrainingExamples,
                        adapter.TrainedAt,
                        adapter.Provenance,
                        StorageKind = 1,
                        LegacyAbsolutePath = adapter.AdapterPath,
                    },
                },
            }),
            _ => throw new ArgumentOutOfRangeException(nameof(schemaVersion)),
        };

    private static byte[] SerializeVersion2Adapters(params object[] adapters)
    {
        var nodes = adapters.Select(adapter => adapter switch
            {
                RetainedRegistryPublishedAdapter retained =>
                    JsonSerializer.SerializeToNode(new
                    {
                        retained.Domain,
                        retained.TrainingExamples,
                        retained.TrainedAt,
                        retained.Provenance,
                        StorageKind = 2,
                        RetainedReference = retained.Reference,
                    })!.AsObject(),
                ModelAdapter legacy => JsonSerializer.SerializeToNode(new
                    {
                        legacy.Domain,
                        legacy.TrainingExamples,
                        legacy.TrainedAt,
                        legacy.Provenance,
                        StorageKind = 1,
                        LegacyAbsolutePath = legacy.AdapterPath,
                    })!.AsObject(),
                _ => throw new ArgumentException("Unsupported registry test adapter."),
            })
            .OrderBy(node => node["Domain"]!.GetValue<string>(), StringComparer.Ordinal)
            .ToArray();
        return JsonSerializer.SerializeToUtf8Bytes(
            new
            {
                SchemaVersion = 2,
                Adapters = nodes,
            },
            new JsonSerializerOptions { WriteIndented = true });
    }

    private static void AssertResolvedRetained(
        DomainModelRegistry registry,
        RetainedRegistryPublishedAdapter expected,
        byte[] expectedBytes)
    {
        var result = registry.ResolveRetainedAdapter(expected.Domain);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        var resolved = Assert.IsType<ResolvedRetainedAdapterArtifact>(result.Value);
        Assert.Equal(expected.Domain, resolved.Domain);
        Assert.Equal(expected.TrainingExamples, resolved.TrainingExamples);
        Assert.Equal(expected.TrainedAt, resolved.TrainedAt);
        Assert.Equal(
            JsonSerializer.Serialize(expected.Provenance),
            JsonSerializer.Serialize(resolved.Provenance));
        Assert.Equal(expected.Reference, resolved.Reference);
        Assert.Equal(expected.Reference.ToPublication(), resolved.Snapshot.Publication);
        Assert.Equal(expectedBytes, resolved.Snapshot.CopyBytes());
        var mutableCopy = resolved.Snapshot.CopyBytes();
        mutableCopy[0] ^= 0xff;
        Assert.Equal(expectedBytes, resolved.Snapshot.CopyBytes());
    }

    private static void AssertUntrusted(
        OperationResult<ResolvedRetainedAdapterArtifact?> result)
    {
        Assert.False(result.IsSuccess);
        Assert.Equal("ADAPTER_REFERENCE_UNTRUSTED", result.ErrorCode);
        Assert.Null(result.Value);
    }

    private static byte[] ConvertRegistryToVersion2Legacy(byte[] version1Bytes)
    {
        var root = JsonNode.Parse(version1Bytes)!.AsObject();
        root["SchemaVersion"] = 2;
        foreach (var adapterNode in root["Adapters"]!.AsArray())
        {
            var adapter = adapterNode!.AsObject();
            var path = adapter["AdapterPath"]!.GetValue<string>();
            adapter.Remove("AdapterPath");
            adapter["StorageKind"] = 1;
            adapter["LegacyAbsolutePath"] = path;
        }
        var canonicalBytes = JsonSerializer.SerializeToUtf8Bytes(root);
        var nonCanonicalBytes = JsonSerializer.SerializeToUtf8Bytes(
            root,
            new JsonSerializerOptions { WriteIndented = true });
        Assert.False(nonCanonicalBytes.AsSpan().SequenceEqual(canonicalBytes));
        return nonCanonicalBytes;
    }

    private static void AssertVersion2LegacyRegistryDomains(
        byte[] bytes,
        params string[] expectedDomains)
    {
        using var document = JsonDocument.Parse(bytes);
        Assert.Equal(2, document.RootElement.GetProperty("SchemaVersion").GetInt32());
        var adapters = document.RootElement.GetProperty("Adapters").EnumerateArray().ToArray();
        Assert.Equal(
            expectedDomains,
            adapters.Select(adapter => adapter.GetProperty("Domain").GetString()!).ToArray());
        Assert.All(adapters, adapter =>
        {
            Assert.Equal(1, adapter.GetProperty("StorageKind").GetInt32());
            Assert.True(adapter.TryGetProperty("LegacyAbsolutePath", out _));
            Assert.False(adapter.TryGetProperty("RetainedReference", out _));
            Assert.False(adapter.TryGetProperty("AdapterPath", out _));
        });
    }

    private static void PublishExactRegistryBytes(
        ServerOwnedContinuousLearningRegistryRootCapability capability,
        byte[] bytes)
    {
        var canonical = capability.ReadCanonical();
        var prior = canonical.State ==
                    ServerOwnedContinuousLearningRegistryCanonicalState.Absent
            ? ServerOwnedContinuousLearningRegistryPriorStateEvidence.Absent()
            : ServerOwnedContinuousLearningRegistryPriorStateEvidence.Present(
                canonical.Snapshot!);
        var written = capability.CreateWriteTemp(bytes);
        var candidate = new ServerOwnedContinuousLearningRegistrySnapshot(
            written.Identity,
            bytes);
        Assert.True(capability.ReadExact(
            ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
            written.Identity).Matches(candidate));
        var rollback = capability.PreparePublication(candidate, prior);
        if (prior.State == ServerOwnedContinuousLearningRegistryCanonicalState.Present)
        {
            capability.RenameNoReplace(
                ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry,
                prior.Snapshot!.Identity,
                ServerOwnedContinuousLearningRegistryLeaf.PublicationBackup);
            capability.FlushRootDirectory();
        }
        capability.RenameNoReplace(
            ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
            candidate.Identity,
            ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry);
        capability.FlushFile(
            ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry,
            candidate.Identity);
        capability.FlushRootDirectory();
        var accepted = capability.CommitPublication(candidate, prior, rollback);
        _ = capability.ResumeAcceptedCleanup(accepted);
    }

    private static void StagePendingRegistryPublication(
        ServerOwnedContinuousLearningRegistryRootCapability capability,
        byte[] candidateBytes,
        bool commitAccepted)
    {
        var canonical = capability.ReadCanonical();
        var prior = ServerOwnedContinuousLearningRegistryPriorStateEvidence.Present(
            Assert.IsType<ServerOwnedContinuousLearningRegistrySnapshot>(canonical.Snapshot));
        var written = capability.CreateWriteTemp(candidateBytes);
        var candidate = new ServerOwnedContinuousLearningRegistrySnapshot(
            written.Identity,
            candidateBytes);
        Assert.True(capability.ReadExact(
            ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
            written.Identity).Matches(candidate));
        var rollback = capability.PreparePublication(candidate, prior);
        capability.RenameNoReplace(
            ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry,
            prior.Snapshot!.Identity,
            ServerOwnedContinuousLearningRegistryLeaf.PublicationBackup);
        capability.FlushRootDirectory();
        capability.RenameNoReplace(
            ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
            candidate.Identity,
            ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry);
        capability.FlushFile(
            ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry,
            candidate.Identity);
        capability.FlushRootDirectory();
        if (commitAccepted)
            _ = capability.CommitPublication(candidate, prior, rollback);
    }

    private static void AssertRegistryInventoryMatches(
        ServerOwnedContinuousLearningRegistryNamespaceInventory expected,
        ServerOwnedContinuousLearningRegistryNamespaceInventory actual)
    {
        var observations = new[]
        {
            (expected.CanonicalRegistry, actual.CanonicalRegistry),
            (expected.WriteTemp, actual.WriteTemp),
            (expected.PublicationBackup, actual.PublicationBackup),
            (expected.RollbackEvidence, actual.RollbackEvidence),
            (expected.AcceptedStaging, actual.AcceptedStaging),
            (expected.AcceptedEvidence, actual.AcceptedEvidence),
        };
        foreach (var (expectedLeaf, actualLeaf) in observations)
        {
            Assert.Equal(expectedLeaf.Leaf, actualLeaf.Leaf);
            Assert.Equal(expectedLeaf.State, actualLeaf.State);
            if (expectedLeaf.Snapshot is null)
            {
                Assert.Null(actualLeaf.Snapshot);
            }
            else
            {
                Assert.NotNull(actualLeaf.Snapshot);
                Assert.True(actualLeaf.Snapshot!.Matches(expectedLeaf.Snapshot));
            }
        }
    }

    private static void AssertStableNamespace(
        RetainedRegistryFixture fixture,
        bool canonicalExpected = true)
    {
        Assert.Equal(canonicalExpected, File.Exists(fixture.RegistryPath));
        Assert.False(File.Exists(fixture.TempPath));
        Assert.False(File.Exists(fixture.BackupPath));
        Assert.False(File.Exists(fixture.RollbackEvidencePath));
        Assert.False(File.Exists(fixture.AcceptedStagingPath));
        Assert.False(File.Exists(fixture.AcceptedEvidencePath));
    }

    private static void AssertRegistryMemory(
        DomainModelRegistry registry,
        params string[] expectedDomains)
    {
        var memory = Assert.IsAssignableFrom<IReadOnlyDictionary<string, ModelAdapter>>(
            typeof(DomainModelRegistry)
                .GetField("_registry", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(registry));
        Assert.Equal(expectedDomains, memory.Keys.Order(StringComparer.Ordinal));
    }

    private static string[] SnapshotLeaves(RetainedRegistryFixture fixture)
        => new[]
            {
                fixture.RegistryPath,
                fixture.TempPath,
                fixture.BackupPath,
                fixture.RollbackEvidencePath,
                fixture.AcceptedStagingPath,
                fixture.AcceptedEvidencePath,
            }
            .Select(path => File.Exists(path)
                ? $"{Path.GetFileName(path)}:{Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)))}"
                : $"{Path.GetFileName(path)}:ABSENT")
            .ToArray();

    private static SafeFileHandle GetRetainedRootHandle(
        ServerOwnedContinuousLearningFileAccess access,
        ContinuousLearningRetainedObjectRole expectedRole)
    {
        var entries = Assert.IsAssignableFrom<Array>(
            typeof(ServerOwnedContinuousLearningFileAccess)
                .GetField("_entries", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(access));
        foreach (var entry in entries)
        {
            var role = Assert.IsType<ContinuousLearningRetainedObjectRole>(
                entry!.GetType().GetProperty("Role")!.GetValue(entry));
            if (role == expectedRole)
            {
                return Assert.IsType<SafeFileHandle>(
                    entry.GetType().GetProperty("Handle")!.GetValue(entry));
            }
        }
        throw new Xunit.Sdk.XunitException("The retained RegistryRoot was not found.");
    }

    private static int RunCrossProcessOwnerLockProbe(string registryRoot)
    {
        var helperPath = ResolveRegistryLockHolderPath();
        var dotnetHost = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH")
                         ?? Environment.ProcessPath;
        Assert.False(string.IsNullOrWhiteSpace(dotnetHost));
        Assert.True(File.Exists(dotnetHost));
        Assert.True(File.Exists(helperPath));
        using var child = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = dotnetHost,
                UseShellExecute = false,
                RedirectStandardError = true,
                CreateNoWindow = true,
            },
        };
        child.StartInfo.ArgumentList.Add(helperPath);
        child.StartInfo.ArgumentList.Add("--probe-exclusive-owner");
        child.StartInfo.ArgumentList.Add(registryRoot);
        child.StartInfo.ArgumentList.Add("registry");
        Assert.True(child.Start());
        Assert.True(child.WaitForExit(10_000));
        Assert.True(string.IsNullOrWhiteSpace(child.StandardError.ReadToEnd()));
        return child.ExitCode;
    }

    private static string ResolveRegistryLockHolderPath()
    {
        var frameworkDirectory = new DirectoryInfo(AppContext.BaseDirectory);
        var configurationDirectory = frameworkDirectory.Parent
            ?? throw new InvalidOperationException("Test configuration directory is unavailable.");
        var repositoryRoot = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            ".."));
        return Path.GetFullPath(Path.Combine(
            repositoryRoot,
            "Tests.Tools",
            "RegistryLockHolder",
            "bin",
            configurationDirectory.Name,
            frameworkDirectory.Name,
            "RegistryLockHolder.dll"));
    }

    private static void RequireWindows()
        => Assert.True(
            OperatingSystem.IsWindows(),
            "Retained RegistryRoot publication requires Windows handle-relative primitives.");
}

internal sealed record RetainedRegistryPublishedAdapter(
    string Domain,
    int TrainingExamples,
    DateTimeOffset TrainedAt,
    ModelAdapterProvenance Provenance,
    AdapterArtifactReference Reference);

internal sealed class RetainedRegistryFixture : IDisposable
{
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
    private static readonly LoraConfig LoraConfiguration = new()
    {
        Rank = 2,
        Alpha = 4f,
        Dropout = 0f,
        TargetModules = ["q_proj", "v_proj"],
    };

    internal RetainedRegistryFixture()
    {
        FileAccess = new FileAccessFixture(provisionAllRoots: true);
    }

    internal FileAccessFixture FileAccess { get; }
    internal string RegistryPath => Path.Combine(FileAccess.RegistryRoot, "registry.json");
    internal string TempPath => Path.Combine(FileAccess.RegistryRoot, ".registry.json.write.tmp");
    internal string BackupPath => Path.Combine(
        FileAccess.RegistryRoot,
        ".registry.json.publication.bak");
    internal string RollbackEvidencePath => Path.Combine(
        FileAccess.RegistryRoot,
        ".registry.json.rollback.evidence");
    internal string AcceptedStagingPath => Path.Combine(
        FileAccess.RegistryRoot,
        ".registry.json.accepted.staging");
    internal string AcceptedEvidencePath => Path.Combine(
        FileAccess.RegistryRoot,
        ".registry.json.accepted.evidence");

    internal ModelAdapter Adapter(
        string domain,
        string fileName,
        int trainingExamples)
    {
        var model = new LlmModel(ModelConfiguration);
        _ = LoraApplicator.Apply(model, LoraConfiguration);
        var path = Path.GetFullPath(Path.Combine(FileAccess.AdapterRoot, fileName));
        model.SaveAdapterCheckpoint(path);
        var source = TrainingSourceProvenance.CreateSourceId(
            $"retained-registry/{domain}/{fileName}",
            "retained registry fixture");
        var manifest = TrainingSourceProvenance.BuildManifest([source]).Value!;
        return new ModelAdapter(
            domain,
            path,
            trainingExamples,
            DateTimeOffset.UtcNow,
            new ModelAdapterProvenance(
                manifest,
                Hash(path),
                Hash(FileAccess.BaseCheckpointPath),
                Hash(FileAccess.TokenizerPath),
                TrainingSourceProvenance.Identify(ModelConfiguration),
                TrainingSourceProvenance.Identify(LoraConfiguration)));
    }

    internal RetainedRegistryPublishedAdapter PublishRetainedAdapter(
        ServerOwnedContinuousLearningAdapterRootCapability adapterRoot,
        string domain,
        int trainingExamples,
        string publicationId)
    {
        var model = new LlmModel(ModelConfiguration);
        _ = LoraApplicator.Apply(model, LoraConfiguration);
        var publication = adapterRoot.Publish(
            model,
            ServerOwnedContinuousLearningAdapterPublicationRequest.Create(
                domain.ToLowerInvariant(),
                publicationId));
        var source = TrainingSourceProvenance.CreateSourceId(
            $"retained-registry/{domain}/{publicationId}",
            "retained registry publication fixture");
        var manifest = TrainingSourceProvenance.BuildManifest([source]).Value!;
        var provenance = new ModelAdapterProvenance(
            manifest,
            publication.Sha256Hex,
            Hash(FileAccess.BaseCheckpointPath),
            Hash(FileAccess.TokenizerPath),
            TrainingSourceProvenance.Identify(ModelConfiguration),
            TrainingSourceProvenance.Identify(LoraConfiguration));
        return new RetainedRegistryPublishedAdapter(
            domain,
            trainingExamples,
            DateTimeOffset.UtcNow,
            provenance,
            AdapterArtifactReference.FromPublication(publication));
    }

    public void Dispose() => FileAccess.Dispose();

    private static string Hash(string path)
    {
        using var stream = new FileStream(
            path,
            FileMode.Open,
            System.IO.FileAccess.Read,
            FileShare.Read);
        return TrainingSourceProvenance.Sha256(stream);
    }
}

internal sealed class RetainedRegistryNullStorage : ISessionStorage
{
    public Task<OperationResult<bool>> SaveAsync(string sessionId, AgentState state)
        => Task.FromResult(OperationResult<bool>.Ok(true));

    public Task<OperationResult<AgentState>> LoadAsync(string sessionId)
        => Task.FromResult(OperationResult<AgentState>.Fail("Not persisted.", "NULL_STORAGE"));

    public Task<OperationResult<IReadOnlyList<string>>> ListSessionsAsync()
        => Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok([]));
}

file sealed class RetainedRegistryWindowsFactAttribute : FactAttribute
{
    public RetainedRegistryWindowsFactAttribute()
    {
        if (!OperatingSystem.IsWindows())
        {
            // Здесь проверяется именно Windows handle-relative семантика.
            // Пропуск при обнаружении сохраняет Linux-доказательство честным:
            // граница платформы не должна выглядеть как дефект продукта.
            Skip = "WINDOWS_ONLY_CAPABILITY_ORACLE: Retained RegistryRoot publication requires Windows handle-relative primitives.";
        }
    }
}

file sealed class RetainedRegistryWindowsTheoryAttribute : TheoryAttribute
{
    public RetainedRegistryWindowsTheoryAttribute()
    {
        if (!OperatingSystem.IsWindows())
        {
            // Все строки с параметрами опираются на тот же Windows-only
            // контракт опеки реестра, поэтому каждая строка вне Windows должна
            // быть NotExecuted.
            Skip = "WINDOWS_ONLY_CAPABILITY_ORACLE: Retained RegistryRoot publication requires Windows handle-relative primitives.";
        }
    }
}
