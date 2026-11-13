// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Buffers.Binary;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using DPO;
using DPO.Models;
using LoRA;
using Microsoft.Win32.SafeHandles;
using TensorCore.Core;
using TransformerModel;
using TransformerModel.Config;

public sealed record DesignConversionDpoPolicyRuntimeCheckpointBridgeResult(
    int SourceRecordCount,
    int BridgePreferenceCount,
    int DpoStepCount,
    string PolicyCheckpointPath,
    string PolicyCheckpointSha256,
    string RuntimeCheckpointPath,
    LearningMachineExport RuntimeExport,
    DesignConversionMachine RuntimeMachine,
    DesignConversionMachineCheckpoint RuntimeCheckpoint,
    bool PolicyCheckpointFreshLoadVerified,
    IReadOnlyList<string> IngredientIds,
    IReadOnlyList<string> EvidenceRefs,
    DesignConversionDpoPolicyCheckpointRef? PolicyCheckpointRef = null,
    DesignConversionDpoSourceEncoderDescriptor? SourceEncoderDescriptor = null)
{
    internal DesignConversionDpoCanonicalBpeTokenizerSnapshotReference?
        CanonicalBpeTokenizerSnapshot { get; init; }
}

/// <summary>
/// Shared publication boundary for every DesignConversion DPO producer. The
/// public runtime pointer is the final commit: a same-directory staged runtime
/// is fresh-loaded and RouteB-validated before it can replace that pointer.
/// </summary>
internal static class DesignConversionDpoRuntimeCheckpointPublisher
{
    internal const string PublicationFailedErrorCode =
        "DESIGN_CONVERSION_RUNTIME_CHECKPOINT_WRITE_FAILED";
    internal const string PolicyCollisionErrorCode =
        "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_COLLISION";
    internal const string PublicationStateUncertainErrorCode =
        "DESIGN_CONVERSION_RUNTIME_PUBLICATION_STATE_UNCERTAIN";

    private const int RuntimeLimit = 64 * 1024 * 1024;
    private const int PolicyLimit = 256 * 1024 * 1024;
    private const uint FileShareReadWriteDelete = 0x00000007;
    private const uint GenericRead = 0x80000000;
    private const uint OpenExisting = 3;
    private const uint FileFlagBackupSemantics = 0x02000000;
    private const uint FileFlagOpenReparsePoint = 0x00200000;
    private const uint Synchronize = 0x00100000;
    private const uint MutexModifyState = 0x00000001;
    private const uint Infinite = 0xffffffff;
    private const uint WaitObject0 = 0x00000000;
    private const uint WaitAbandoned0 = 0x00000080;
    private const uint WaitFailed = 0xffffffff;
    private const uint SddlRevision1 = 1;
    private const int ErrorFileNotFound = 2;
    private const int ErrorPathNotFound = 3;
    private const string GlobalMutexSecurityDescriptor =
        "D:P(A;;0x00100001;;;AU)(A;;0x00100001;;;SY)(A;;0x00100001;;;BA)";
    private static readonly AsyncLocal<int?> PolicyLimitOverride = new();
    private static readonly AsyncLocal<int?> RuntimeLimitOverride = new();
    private static readonly AsyncLocal<Func<bool>?> MutexReleaseOverride = new();

    internal static int? PolicyLimitForTesting
    {
        get => PolicyLimitOverride.Value;
        set => PolicyLimitOverride.Value = value;
    }

    internal static int? RuntimeLimitForTesting
    {
        get => RuntimeLimitOverride.Value;
        set => RuntimeLimitOverride.Value = value;
    }

    internal static Func<bool>? MutexReleaseForTesting
    {
        get => MutexReleaseOverride.Value;
        set => MutexReleaseOverride.Value = value;
    }

    private static int EffectivePolicyLimit => PolicyLimitOverride.Value ?? PolicyLimit;
    private static int EffectiveRuntimeLimit => RuntimeLimitOverride.Value ?? RuntimeLimit;

    internal static OperationResult<byte[]> ReadPolicyPayload(
        string path,
        CancellationToken ct)
    {
        try
        {
            ct.ThrowIfCancellationRequested();
            using var stream = new FileStream(
                path, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (stream.Length <= 0)
                return OperationResult<byte[]>.Ok([]);
            if (stream.Length > EffectivePolicyLimit)
                return PolicyPayloadTooLarge();
            var bytes = new byte[checked((int)stream.Length)];
            stream.ReadExactly(bytes);
            ct.ThrowIfCancellationRequested();
            if (stream.Length != bytes.Length)
                return PolicyPayloadReadFailed();
            return OperationResult<byte[]>.Ok(bytes);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return OperationResult<byte[]>.Fail("Bridge cancelled.", "CANCELLED");
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return PolicyPayloadReadFailed();
        }
    }

    internal static Task<OperationResult<DesignConversionMachineLoadResult>> PublishAsync(
        string policyCheckpointPath,
        string policyCheckpointFileName,
        byte[] policyCheckpointPayload,
        string runtimeCheckpointPath,
        DesignConversionMachineCheckpoint bridgedCheckpoint,
        int dpoStepCount,
        IReadOnlyList<string> bridgeEvidenceRefs,
        Action<string>? stageObserver,
        CancellationToken cancellationToken)
        => Task.Factory.StartNew(
            () => PublishCore(
                policyCheckpointPath,
                policyCheckpointFileName,
                policyCheckpointPayload,
                runtimeCheckpointPath,
                bridgedCheckpoint,
                dpoStepCount,
                bridgeEvidenceRefs,
                stageObserver,
                cancellationToken),
            CancellationToken.None,
            TaskCreationOptions.DenyChildAttach,
            TaskScheduler.Default);

    internal static Task<OperationResult<DesignConversionMachineLoadResult>>
        PublishWithFinalPermitAsync(
        string policyCheckpointPath,
        string policyCheckpointFileName,
        byte[] policyCheckpointPayload,
        string runtimeCheckpointPath,
        DesignConversionMachineCheckpoint bridgedCheckpoint,
        int dpoStepCount,
        IReadOnlyList<string> bridgeEvidenceRefs,
        Action<string>? stageObserver,
        Func<OperationResult<IDisposable>> finalPermitFactory,
        CancellationToken cancellationToken)
        => Task.Factory.StartNew(
            () => PublishWithFinalPermitCore(
                policyCheckpointPath,
                policyCheckpointFileName,
                policyCheckpointPayload,
                runtimeCheckpointPath,
                bridgedCheckpoint,
                dpoStepCount,
                bridgeEvidenceRefs,
                stageObserver,
                finalPermitFactory,
                cancellationToken),
            CancellationToken.None,
            TaskCreationOptions.DenyChildAttach,
            TaskScheduler.Default);

    private static OperationResult<DesignConversionMachineLoadResult>
        PublishWithFinalPermitCore(
            string policyCheckpointPath,
            string policyCheckpointFileName,
            byte[] policyCheckpointPayload,
            string runtimeCheckpointPath,
            DesignConversionMachineCheckpoint bridgedCheckpoint,
            int dpoStepCount,
            IReadOnlyList<string> bridgeEvidenceRefs,
            Action<string>? observer,
            Func<OperationResult<IDisposable>> finalPermitFactory,
            CancellationToken ct)
    {
        var provisional = PublishProvisionalCore(
            policyCheckpointPath,
            policyCheckpointFileName,
            policyCheckpointPayload,
            runtimeCheckpointPath,
            bridgedCheckpoint,
            dpoStepCount,
            bridgeEvidenceRefs,
            observer,
            ct);
        if (!provisional.IsSuccess || provisional.Value is null)
            return provisional.MapError<DesignConversionMachineLoadResult>();

        using var publicationLease = provisional.Value;
        OperationResult<IDisposable> finalPermit;
        try
        {
            finalPermit = finalPermitFactory();
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            if (!publicationLease.AbortAndProve())
            {
                return OperationResult<DesignConversionMachineLoadResult>.Fail(
                    "Runtime checkpoint publication rollback could not be proven.",
                    PublicationStateUncertainErrorCode);
            }
            return OperationResult<DesignConversionMachineLoadResult>.Fail(
                "Bridge cancelled.",
                "CANCELLED");
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            if (!publicationLease.AbortAndProve())
            {
                return OperationResult<DesignConversionMachineLoadResult>.Fail(
                    "Runtime checkpoint publication rollback could not be proven.",
                    PublicationStateUncertainErrorCode);
            }
            return PublicationFailed();
        }

        if (!finalPermit.IsSuccess || finalPermit.Value is null)
        {
            if (!publicationLease.AbortAndProve())
            {
                return OperationResult<DesignConversionMachineLoadResult>.Fail(
                    "Runtime checkpoint publication rollback could not be proven.",
                    PublicationStateUncertainErrorCode);
            }
            return finalPermit.MapError<DesignConversionMachineLoadResult>();
        }

        using (finalPermit.Value)
            return publicationLease.Commit(ct);
    }

    private static OperationResult<ProvisionalPublicationLease> PublishProvisionalCore(
        string policyCheckpointPath,
        string policyCheckpointFileName,
        byte[] policyCheckpointPayload,
        string runtimeCheckpointPath,
        DesignConversionMachineCheckpoint bridgedCheckpoint,
        int dpoStepCount,
        IReadOnlyList<string> bridgeEvidenceRefs,
        Action<string>? observer,
        CancellationToken ct)
    {
        PublicationState? state = null;
        OwnedMutex? destinationLock = null;
        var ownershipTransferred = false;
        try
        {
            ct.ThrowIfCancellationRequested();
            if (policyCheckpointPayload is null || policyCheckpointPayload.Length == 0 ||
                string.IsNullOrWhiteSpace(policyCheckpointFileName) ||
                !string.Equals(
                    policyCheckpointFileName,
                    Path.GetFileName(policyCheckpointFileName),
                    StringComparison.Ordinal))
            {
                return PublicationFailed().MapError<ProvisionalPublicationLease>();
            }
            if (policyCheckpointPayload.Length > EffectivePolicyLimit)
                return PolicyPayloadTooLarge<ProvisionalPublicationLease>();

            var runtimeFullPath = Path.GetFullPath(runtimeCheckpointPath);
            var runtimeDirectory = Path.GetDirectoryName(runtimeFullPath);
            var sourceFullPath = Path.GetFullPath(policyCheckpointPath);
            if (string.IsNullOrWhiteSpace(runtimeDirectory))
                return PublicationFailed().MapError<ProvisionalPublicationLease>();

            EnsureNoReparseAmbiguity(sourceFullPath, includeLeaf: true);
            EnsureDestinationDirectory(runtimeDirectory, ct);
            var destinationIdentity = ReadPhysicalDirectoryIdentity(runtimeDirectory);
            var siblingFullPath = Path.GetFullPath(
                Path.Combine(runtimeDirectory, policyCheckpointFileName));
            var runtimeLeaf = Path.GetFileName(runtimeFullPath);
            var siblingLeaf = Path.GetFileName(siblingFullPath);
            if (LeafEquals(runtimeLeaf, siblingLeaf))
                return PublicationFailed().MapError<ProvisionalPublicationLease>();

            var sourceParent = Path.GetDirectoryName(sourceFullPath);
            if (string.IsNullOrWhiteSpace(sourceParent))
                return PublicationFailed().MapError<ProvisionalPublicationLease>();
            var sourceIdentity = ReadPhysicalDirectoryIdentity(sourceParent);
            var ownsSibling = !string.Equals(
                BuildPhysicalLeafIdentity(sourceIdentity, Path.GetFileName(sourceFullPath)),
                BuildPhysicalLeafIdentity(destinationIdentity, siblingLeaf),
                StringComparison.Ordinal);

            observer?.Invoke("before-lock-attempt");
            destinationLock = AcquireNamedMutex(
                BuildMutexName("destination", BuildPhysicalLeafIdentity(
                    destinationIdentity, runtimeLeaf)),
                ct,
                observer);
            observer?.Invoke("lock-acquired");
            EnsureNoReparseAmbiguity(runtimeDirectory, includeLeaf: true);
            if (!string.Equals(
                    destinationIdentity,
                    ReadPhysicalDirectoryIdentity(runtimeDirectory),
                    StringComparison.Ordinal))
            {
                return PublicationFailed().MapError<ProvisionalPublicationLease>();
            }

            state = new PublicationState(
                siblingFullPath,
                runtimeFullPath,
                policyCheckpointPayload,
                ownsSibling)
            {
                PreviousRuntime = ReadRuntimeSnapshotNoFollow(
                    runtimeFullPath,
                    EffectiveRuntimeLimit,
                    ct)
            };
            if (!CurrentRuntimeMatchesNoFollow(
                    runtimeFullPath,
                    state.PreviousRuntime,
                    ct))
            {
                return FailAfterRollback(
                    state,
                    PublicationFailed().MapError<ProvisionalPublicationLease>(),
                    "publication_failed",
                    observer);
            }

            if (PathExists(siblingFullPath))
            {
                EnsureNoReparseAmbiguity(siblingFullPath, includeLeaf: true);
                state.SiblingSnapshot = ReadRuntimeSnapshotNoFollow(
                    siblingFullPath,
                    EffectivePolicyLimit,
                    ct);
                if (!SnapshotMatches(state.SiblingSnapshot, policyCheckpointPayload) ||
                    !PolicyArtifactStillMatches(state, ct))
                {
                    return PolicyCollision().MapError<ProvisionalPublicationLease>();
                }
                state.PolicyMustRemain = true;
            }
            else if (ownsSibling)
            {
                state.PolicyStagePath = BuildStagePath(siblingFullPath, "policy");
                WriteAndFlushNew(state.PolicyStagePath, policyCheckpointPayload, ct);
                state.PolicyStageSnapshot = ReadRuntimeSnapshotNoFollow(
                    state.PolicyStagePath,
                    EffectivePolicyLimit,
                    ct);
                File.Move(state.PolicyStagePath, siblingFullPath, overwrite: false);
                state.PolicyStagePath = null;
                state.PolicyStageSnapshot = null;
                state.CreatedSibling = true;
                state.SiblingSnapshot = ReadRuntimeSnapshotNoFollow(
                    siblingFullPath,
                    EffectivePolicyLimit,
                    ct);
                if (!PolicyArtifactStillMatches(state, ct))
                {
                    return FailAfterRollback(
                        state,
                        PublicationFailed().MapError<ProvisionalPublicationLease>(),
                        "publication_failed",
                        observer);
                }
            }
            else
            {
                return PolicyCollision().MapError<ProvisionalPublicationLease>();
            }

            observer?.Invoke("after-policy");
            if (!PolicyArtifactStillMatches(state, ct))
            {
                return FailAfterRollback(
                    state,
                    PolicyCollision().MapError<ProvisionalPublicationLease>(),
                    "policy_collision",
                    observer);
            }

            state.PublishedRuntime = JsonSerializer.SerializeToUtf8Bytes(
                bridgedCheckpoint,
                DesignConversionCheckpointJsonOptions.Standard);
            if (state.PublishedRuntime.Length > EffectiveRuntimeLimit)
            {
                return FailAfterRollback(
                    state,
                    RuntimePayloadTooLarge().MapError<ProvisionalPublicationLease>(),
                    "runtime_payload_too_large",
                    observer);
            }
            state.RuntimeStagePath = BuildStagePath(runtimeFullPath, "runtime");
            WriteAndFlushNew(state.RuntimeStagePath, state.PublishedRuntime, ct);
            state.RuntimeStageSnapshot = ReadRuntimeSnapshotNoFollow(
                state.RuntimeStagePath,
                EffectiveRuntimeLimit,
                ct);
            if (!CurrentRuntimeMatchesNoFollow(
                    state.RuntimeStagePath,
                    state.RuntimeStageSnapshot,
                    ct))
            {
                return FailAfterRollback(
                    state,
                    PublicationFailed().MapError<ProvisionalPublicationLease>(),
                    "publication_failed",
                    observer);
            }

            observer?.Invoke("before-load-gate");
            var loaded = DesignConversionMachine.LoadCheckpointAsync(
                    state.RuntimeStagePath,
                    ct)
                .GetAwaiter()
                .GetResult();
            if (!loaded.IsSuccess || loaded.Value is null)
            {
                return FailAfterRollback(
                    state,
                    LoadGateFailed().MapError<ProvisionalPublicationLease>(),
                    "load_gate_failed",
                    observer);
            }
            var gate = DesignConversionDpoTrainerRouteBGate.ValidateTypedRuntimeCheckpoint(
                loaded.Value.Checkpoint.DpoPolicyCheckpointRef,
                dpoStepCount,
                bridgeEvidenceRefs,
                callerFreshLoadVerified: true);
            if (!gate.IsSuccess)
            {
                return FailAfterRollback(
                    state,
                    RouteBGateFailed().MapError<ProvisionalPublicationLease>(),
                    "route_b_gate_failed",
                    observer);
            }
            observer?.Invoke("after-load-gate");

            if (!CurrentRuntimeMatchesNoFollow(
                    state.RuntimeStagePath,
                    state.RuntimeStageSnapshot,
                    ct) ||
                !PolicyArtifactStillMatches(state, ct) ||
                !RuntimeStillMatchesPrior(state, ct, requireSameIdentity: true))
            {
                return FailAfterRollback(
                    state,
                    PublicationFailed().MapError<ProvisionalPublicationLease>(),
                    "precommit_identity_failed",
                    observer);
            }

            var publicationLease = new ProvisionalPublicationLease(
                state,
                destinationLock,
                loaded.Value,
                observer);
            destinationLock = null;
            ownershipTransferred = true;
            return OperationResult<ProvisionalPublicationLease>.Ok(publicationLease);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            var cancelled = OperationResult<ProvisionalPublicationLease>.Fail(
                "Bridge cancelled.",
                "CANCELLED");
            return state is null
                ? cancelled
                : FailAfterRollback(state, cancelled, "cancelled", observer);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            var failed = PublicationFailed().MapError<ProvisionalPublicationLease>();
            return state is null
                ? failed
                : FailAfterRollback(state, failed, "publication_failed", observer);
        }
        finally
        {
            if (!ownershipTransferred && state is not null)
                _ = RollbackAndProve(state, observer: null);
            destinationLock?.Dispose();
        }
    }

    private static OperationResult<DesignConversionMachineLoadResult> PublishCore(
        string policyCheckpointPath,
        string policyCheckpointFileName,
        byte[] policyCheckpointPayload,
        string runtimeCheckpointPath,
        DesignConversionMachineCheckpoint bridgedCheckpoint,
        int dpoStepCount,
        IReadOnlyList<string> bridgeEvidenceRefs,
        Action<string>? observer,
        CancellationToken ct)
    {
        PublicationState? state = null;
        OwnedMutex? destinationLock = null;
        try
        {
            ct.ThrowIfCancellationRequested();
            if (policyCheckpointPayload is null || policyCheckpointPayload.Length == 0 ||
                string.IsNullOrWhiteSpace(policyCheckpointFileName) ||
                !string.Equals(
                    policyCheckpointFileName,
                    Path.GetFileName(policyCheckpointFileName),
                    StringComparison.Ordinal))
                return PublicationFailed();
            if (policyCheckpointPayload.Length > EffectivePolicyLimit)
                return PolicyPayloadTooLarge<DesignConversionMachineLoadResult>();

            var runtimeFullPath = Path.GetFullPath(runtimeCheckpointPath);
            var runtimeDirectory = Path.GetDirectoryName(runtimeFullPath);
            var sourceFullPath = Path.GetFullPath(policyCheckpointPath);
            if (string.IsNullOrWhiteSpace(runtimeDirectory))
                return PublicationFailed();

            EnsureNoReparseAmbiguity(sourceFullPath, includeLeaf: true);
            EnsureDestinationDirectory(runtimeDirectory, ct);
            var destinationIdentity = ReadPhysicalDirectoryIdentity(runtimeDirectory);
            var siblingFullPath = Path.GetFullPath(
                Path.Combine(runtimeDirectory, policyCheckpointFileName));
            var runtimeLeaf = Path.GetFileName(runtimeFullPath);
            var siblingLeaf = Path.GetFileName(siblingFullPath);
            if (LeafEquals(runtimeLeaf, siblingLeaf))
                return PublicationFailed();

            var sourceParent = Path.GetDirectoryName(sourceFullPath);
            if (string.IsNullOrWhiteSpace(sourceParent))
                return PublicationFailed();
            var sourceIdentity = ReadPhysicalDirectoryIdentity(sourceParent);
            var ownsSibling = !string.Equals(
                BuildPhysicalLeafIdentity(sourceIdentity, Path.GetFileName(sourceFullPath)),
                BuildPhysicalLeafIdentity(destinationIdentity, siblingLeaf),
                StringComparison.Ordinal);

            observer?.Invoke("before-lock-attempt");
            destinationLock = AcquireNamedMutex(
                BuildMutexName("destination", BuildPhysicalLeafIdentity(
                    destinationIdentity, runtimeLeaf)),
                ct,
                observer);
            observer?.Invoke("lock-acquired");
            EnsureNoReparseAmbiguity(runtimeDirectory, includeLeaf: true);
            if (!string.Equals(
                    destinationIdentity,
                    ReadPhysicalDirectoryIdentity(runtimeDirectory),
                    StringComparison.Ordinal))
                return PublicationFailed();
            state = new PublicationState(
                siblingFullPath,
                runtimeFullPath,
                policyCheckpointPayload,
                ownsSibling);
            state.PreviousRuntime = ReadRuntimeSnapshotNoFollow(
                runtimeFullPath, EffectiveRuntimeLimit, ct);
            if (!CurrentRuntimeMatchesNoFollow(
                    runtimeFullPath, state.PreviousRuntime, ct))
                return FailAfterRollback(
                    state, PublicationFailed(), "publication_failed", observer);

            if (PathExists(siblingFullPath))
            {
                EnsureNoReparseAmbiguity(siblingFullPath, includeLeaf: true);
                state.SiblingSnapshot = ReadRuntimeSnapshotNoFollow(
                    siblingFullPath,
                    EffectivePolicyLimit,
                    ct);
                if (!SnapshotMatches(state.SiblingSnapshot, policyCheckpointPayload) ||
                    !CurrentRuntimeMatchesNoFollow(
                        siblingFullPath,
                        state.SiblingSnapshot,
                        ct))
                    return PolicyCollision();
                state.PolicyMustRemain = true;
            }
            else if (ownsSibling)
            {
                state.PolicyStagePath = BuildStagePath(siblingFullPath, "policy");
                WriteAndFlushNew(state.PolicyStagePath, policyCheckpointPayload, ct);
                state.PolicyStageSnapshot = ReadRuntimeSnapshotNoFollow(
                    state.PolicyStagePath,
                    EffectivePolicyLimit,
                    ct);
                File.Move(state.PolicyStagePath, siblingFullPath, overwrite: false);
                state.PolicyStagePath = null;
                state.PolicyStageSnapshot = null;
                state.CreatedSibling = true;
                state.SiblingSnapshot = ReadRuntimeSnapshotNoFollow(
                    siblingFullPath,
                    EffectivePolicyLimit,
                    ct);
                if (!PolicyArtifactStillMatches(state, ct))
                    return FailAfterRollback(
                        state, PublicationFailed(), "publication_failed", observer);
            }
            else if (!PolicyArtifactStillMatches(state, ct))
            {
                return PolicyCollision();
            }

            observer?.Invoke("after-policy");
            if (!PolicyArtifactStillMatches(state, ct))
                return FailAfterRollback(
                    state, PolicyCollision(), "policy_collision", observer);

            state.PublishedRuntime = JsonSerializer.SerializeToUtf8Bytes(
                bridgedCheckpoint,
                DesignConversionCheckpointJsonOptions.Standard);
            if (state.PublishedRuntime.Length > EffectiveRuntimeLimit)
                return FailAfterRollback(
                    state,
                    RuntimePayloadTooLarge(),
                    "runtime_payload_too_large",
                    observer);
            state.RuntimeStagePath = BuildStagePath(runtimeFullPath, "runtime");
            WriteAndFlushNew(state.RuntimeStagePath, state.PublishedRuntime, ct);
            state.RuntimeStageSnapshot = ReadRuntimeSnapshotNoFollow(
                state.RuntimeStagePath,
                EffectiveRuntimeLimit,
                ct);
            if (!CurrentRuntimeMatchesNoFollow(
                    state.RuntimeStagePath,
                    state.RuntimeStageSnapshot,
                    ct))
                return FailAfterRollback(
                    state, PublicationFailed(), "publication_failed", observer);

            observer?.Invoke("before-load-gate");
            var loaded = DesignConversionMachine.LoadCheckpointAsync(
                    state.RuntimeStagePath,
                    ct)
                .GetAwaiter()
                .GetResult();
            if (!loaded.IsSuccess || loaded.Value is null)
                return FailAfterRollback(
                    state, LoadGateFailed(), "load_gate_failed", observer);
            var gate = DesignConversionDpoTrainerRouteBGate.ValidateTypedRuntimeCheckpoint(
                loaded.Value.Checkpoint.DpoPolicyCheckpointRef,
                dpoStepCount,
                bridgeEvidenceRefs,
                callerFreshLoadVerified: true);
            if (!gate.IsSuccess)
                return FailAfterRollback(
                    state, RouteBGateFailed(), "route_b_gate_failed", observer);
            observer?.Invoke("after-load-gate");

            if (!CurrentRuntimeMatchesNoFollow(
                    state.RuntimeStagePath,
                    state.RuntimeStageSnapshot,
                    ct) ||
                !PolicyArtifactStillMatches(state, ct) ||
                !RuntimeStillMatchesPrior(state, ct, requireSameIdentity: true))
                return FailAfterRollback(
                    state, PublicationFailed(), "precommit_identity_failed", observer);

            observer?.Invoke("before-runtime-commit");
            if (!CurrentRuntimeMatchesNoFollow(
                    state.RuntimeStagePath,
                    state.RuntimeStageSnapshot,
                    ct) ||
                !PolicyArtifactStillMatches(state, ct))
                return FailAfterRollback(
                    state, PublicationFailed(), "precommit_artifact_identity_failed", observer);
            if (!RuntimeStillMatchesPrior(state, ct, requireSameIdentity: true))
                return FailAfterRollback(
                    state, PublicationFailed(), "precommit_runtime_identity_failed", observer);
            if (state.PreviousRuntime.Exists)
                File.Replace(state.RuntimeStagePath, runtimeFullPath, null);
            else
                File.Move(state.RuntimeStagePath, runtimeFullPath, overwrite: false);
            state.RuntimeStagePath = null;
            state.PublishedRuntimeSnapshot = state.RuntimeStageSnapshot;
            state.RuntimeStageSnapshot = null;
            state.PublicRuntimeCommitted = true;
            TryNotify(observer, "after-runtime");
            return loaded;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            var cancelled = OperationResult<DesignConversionMachineLoadResult>.Fail(
                "Bridge cancelled.", "CANCELLED");
            return state is null
                ? cancelled
                : FailAfterRollback(state, cancelled, "cancelled", observer);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return state is null
                ? PublicationFailed()
                : FailAfterRollback(
                    state, PublicationFailed(), "publication_failed", observer);
        }
        finally
        {
            if (state?.RuntimeStagePath is not null)
                TryDeleteUnpublishedStage(state.RuntimeStagePath, state.RuntimeStageSnapshot);
            if (state?.PolicyStagePath is not null)
                TryDeleteUnpublishedStage(state.PolicyStagePath, state.PolicyStageSnapshot);
            destinationLock?.Dispose();
        }
    }

    private static OperationResult<T> FailAfterRollback<T>(
        PublicationState state,
        OperationResult<T> primary,
        string primaryCategory,
        Action<string>? observer)
    {
        var rollback = RollbackAndProve(state, observer);
        return rollback.Proven
            ? primary
            : OperationResult<T>.Fail(
                $"Publication state is uncertain; primary category: {primaryCategory}.",
                PublicationStateUncertainErrorCode);
    }

    private static RollbackOutcome RollbackAndProve(
        PublicationState state,
        Action<string>? observer)
    {
        var proven = true;
        try
        {
            observer?.Invoke("before-runtime-rollback");
            if (state.PublicRuntimeCommitted)
            {
                if (state.PublishedRuntimeSnapshot is null ||
                    !CurrentRuntimeMatchesNoFollow(
                        state.RuntimePath,
                        state.PublishedRuntimeSnapshot,
                        CancellationToken.None))
                {
                    proven = false;
                }
                else if (state.PreviousRuntime?.Exists == true)
                {
                    if (!RestorePointer(state.RuntimePath, state.PreviousRuntime.Bytes))
                        proven = false;
                }
                else
                {
                    File.Delete(state.RuntimePath);
                }
            }
            if (state.PreviousRuntime is not null &&
                !RuntimeStillMatchesPrior(
                    state,
                    CancellationToken.None,
                    requireSameIdentity: !state.PublicRuntimeCommitted))
                proven = false;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            proven = false;
        }

        try
        {
            observer?.Invoke("before-policy-rollback");
            if (state.CreatedSibling)
            {
                if (!PathExists(state.SiblingPath))
                {
                    // Already absent is the required rollback state.
                }
                else if (PolicyArtifactStillMatches(
                             state,
                             CancellationToken.None))
                {
                    File.Delete(state.SiblingPath);
                }
                else
                {
                    proven = false;
                }
                if (PathExists(state.SiblingPath))
                    proven = false;
            }
            else if (state.PolicyMustRemain &&
                     !PolicyArtifactStillMatches(
                         state,
                         CancellationToken.None))
            {
                proven = false;
            }
            else if (!state.PolicyMustRemain && PathExists(state.SiblingPath))
            {
                proven = false;
            }
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            proven = false;
        }

        proven &= DeleteAndProveOwnedStage(
            state.RuntimeStagePath, state.RuntimeStageSnapshot);
        proven &= DeleteAndProveOwnedStage(
            state.PolicyStagePath, state.PolicyStageSnapshot);
        return new RollbackOutcome(proven);
    }

    private static bool RuntimeStillMatchesPrior(
        PublicationState state,
        CancellationToken ct,
        bool requireSameIdentity)
    {
        if (state.PreviousRuntime is null)
            return true;
        return CurrentRuntimeMatchesNoFollow(
            state.RuntimePath,
            state.PreviousRuntime,
            ct,
            requireSameIdentity);
    }

    private static bool RestorePointer(string path, byte[] bytes)
    {
        var temp = BuildStagePath(path, "rollback");
        FileSnapshot? tempSnapshot = null;
        try
        {
            WriteAndFlushNew(temp, bytes, CancellationToken.None);
            tempSnapshot = ReadRuntimeSnapshotNoFollow(
                temp,
                Math.Max(bytes.Length, 1),
                CancellationToken.None);
            File.Replace(temp, path, null);
            return !PathExists(temp) &&
                   CurrentRuntimeMatchesNoFollow(
                       path, SnapshotOf(bytes), CancellationToken.None);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return false;
        }
        finally
        {
            TryDeleteUnpublishedStage(temp, tempSnapshot);
        }
    }

    private static bool DeleteAndProveOwnedStage(
        string? path,
        FileSnapshot? expected)
    {
        if (path is null || !PathExists(path))
            return true;
        if (expected is null)
            return false;
        try
        {
            if (!CurrentRuntimeMatchesNoFollow(
                    path,
                    expected,
                    CancellationToken.None))
                return false;
            File.Delete(path);
            return !PathExists(path);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return false;
        }
    }

    private static void TryDeleteUnpublishedStage(
        string path,
        FileSnapshot? expected)
    {
        if (expected is null || !PathExists(path))
            return;
        try
        {
            if (CurrentRuntimeMatchesNoFollow(
                    path,
                    expected,
                    CancellationToken.None))
                File.Delete(path);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)) { }
    }

    private static void TryNotify(Action<string>? observer, string stage)
    {
        try { observer?.Invoke(stage); }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            // Testing notification only; public commit is already terminal.
        }
    }

    private static void EnsureDestinationDirectory(string directory, CancellationToken ct)
    {
        EnsureNoReparseAmbiguity(directory, includeLeaf: false);
        var setupIdentity = BuildPhysicalSetupIdentity(directory);
        using var setup = AcquireNamedMutex(
            BuildMutexName("setup", setupIdentity), ct, observer: null);
        Directory.CreateDirectory(directory);
        EnsureNoReparseAmbiguity(directory, includeLeaf: true);
    }

    private static string BuildPhysicalSetupIdentity(string directory)
    {
        var full = Path.GetFullPath(directory);
        var existing = full;
        while (!Directory.Exists(existing))
        {
            var parent = Path.GetDirectoryName(existing);
            if (string.IsNullOrWhiteSpace(parent) ||
                string.Equals(parent, existing, StringComparison.Ordinal))
                throw new IOException("Physical destination ancestor is unavailable.");
            existing = parent;
        }
        EnsureNoReparseAmbiguity(existing, includeLeaf: true);
        var relative = Path.GetRelativePath(existing, full);
        var normalizedRelative = OperatingSystem.IsWindows()
            ? relative.ToUpperInvariant()
            : relative;
        return $"{ReadPhysicalDirectoryIdentity(existing)}|{normalizedRelative}";
    }

    private static OwnedMutex AcquireNamedMutex(
        string name,
        CancellationToken ct,
        Action<string>? observer)
    {
        if (OperatingSystem.IsLinux())
            return AcquireLinuxPublicationLock(name, ct, observer);

        IntPtr securityDescriptor = IntPtr.Zero;
        SafeWaitHandle? mutex = null;
        try
        {
            if (!OperatingSystem.IsWindows() ||
                !ConvertStringSecurityDescriptorToSecurityDescriptor(
                    GlobalMutexSecurityDescriptor,
                    SddlRevision1,
                    out securityDescriptor,
                    out _))
                throw new IOException("Global publication lock security is unavailable.");
            var attributes = new SecurityAttributes
            {
                Length = Marshal.SizeOf<SecurityAttributes>(),
                SecurityDescriptor = securityDescriptor,
                InheritHandle = false
            };
            mutex = CreateMutexEx(
                ref attributes,
                name,
                0,
                Synchronize | MutexModifyState);
            if (mutex.IsInvalid)
                throw new IOException("Global publication lock creation failed.");

            var cancellationWaitHandle = ct.WaitHandle;
            var cancellationHandle = cancellationWaitHandle.SafeWaitHandle;
            var mutexPinned = false;
            var cancellationPinned = false;
            uint wait;
            try
            {
                mutex.DangerousAddRef(ref mutexPinned);
                cancellationHandle.DangerousAddRef(ref cancellationPinned);
                var handles = new[]
                {
                    mutex.DangerousGetHandle(),
                    cancellationHandle.DangerousGetHandle()
                };
                wait = WaitForMultipleObjects(
                    (uint)handles.Length, handles, waitAll: false, Infinite);
            }
            finally
            {
                if (cancellationPinned)
                    cancellationHandle.DangerousRelease();
                if (mutexPinned)
                    mutex.DangerousRelease();
                GC.KeepAlive(cancellationWaitHandle);
            }
            if (wait == WaitObject0 || wait == WaitAbandoned0)
                return new OwnedMutex(
                    mutex,
                    ReleaseWindowsPublicationLock,
                    Marshal.GetLastWin32Error,
                    observer);
            if (wait == WaitObject0 + 1)
                throw new OperationCanceledException(ct);
            if (wait == WaitFailed)
                throw new IOException("Global publication lock wait failed.");
            throw new IOException("Global publication lock was not acquired.");
        }
        catch
        {
            mutex?.Dispose();
            throw;
        }
        finally
        {
            if (securityDescriptor != IntPtr.Zero)
                _ = LocalFree(securityDescriptor);
        }
    }

    private static OwnedMutex AcquireLinuxPublicationLock(
        string name,
        CancellationToken ct,
        Action<string>? observer)
    {
        var handle = DesignConversionHandleBoundDirectory
            .AcquireLinuxPublicationSerializationLock(name, ct);
        try
        {
            // Bridge сохраняет общее владение release/observer, но raw POSIX
            // lock authority теперь живёт только в handle-bound слое.
            return new OwnedMutex(
                handle,
                ReleaseLinuxPublicationLock,
                Marshal.GetLastPInvokeError,
                observer);
        }
        catch
        {
            handle.Dispose();
            throw;
        }
    }

    private static string BuildMutexName(string scope, string identity)
    {
        var digest = Convert.ToHexString(SHA256.HashData(
            Encoding.UTF8.GetBytes($"{scope}|{identity}")));
        return $"Global\\XIIGEN_DPO_RUNTIME_{digest}";
    }

    private static string ReadPhysicalDirectoryIdentity(string directory)
    {
        var full = Path.GetFullPath(directory);
        EnsureNoReparseAmbiguity(full, includeLeaf: true);
        if (OperatingSystem.IsLinux())
            return ReadLinuxPhysicalDirectoryIdentity(full);
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException(
                "Physical directory identity is not available on this platform.");

        using var handle = CreateFileForIdentity(
            full,
            0,
            FileShareReadWriteDelete,
            IntPtr.Zero,
            OpenExisting,
            FileFlagBackupSemantics,
            IntPtr.Zero);
        if (handle.IsInvalid || !GetFileInformationByHandleForIdentity(handle, out var info))
            throw new IOException("Physical destination identity is unavailable.");
        return $"win:{info.VolumeSerialNumber:x8}:{info.FileIndexHigh:x8}{info.FileIndexLow:x8}";
    }

    private static string ReadLinuxPhysicalDirectoryIdentity(string directory)
        => DesignConversionHandleBoundDirectory.ReadLinuxPhysicalDirectoryIdentity(directory);

    private static void EnsureNoReparseAmbiguity(string path, bool includeLeaf)
    {
        var full = Path.GetFullPath(path);
        if (OperatingSystem.IsLinux())
        {
            EnsureNoLinuxSymbolicLinkAmbiguity(full, includeLeaf);
            return;
        }

        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException(
                "Runtime checkpoint publication is supported only on Windows and Linux.");

        var current = includeLeaf ? full : Path.GetDirectoryName(full);
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (PathExists(current))
            {
                var attributes = File.GetAttributes(current);
                if ((attributes & FileAttributes.ReparsePoint) != 0)
                    throw new IOException("Reparse-point destination is not publishable.");
            }
            var parent = Path.GetDirectoryName(current);
            if (string.Equals(parent, current, StringComparison.Ordinal))
                break;
            current = parent;
        }
    }

    private static void EnsureNoLinuxSymbolicLinkAmbiguity(
        string path,
        bool includeLeaf)
        => DesignConversionHandleBoundDirectory.EnsureLinuxSymbolicLinkFreePath(
            path,
            includeLeaf);

    private static string BuildPhysicalLeafIdentity(string parentIdentity, string leaf)
        => $"{parentIdentity}|{(OperatingSystem.IsWindows() ? leaf.ToUpperInvariant() : leaf)}";

    private static bool LeafEquals(string left, string right)
        => string.Equals(
            left,
            right,
            OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal);

    private static bool PolicyArtifactStillMatches(
        PublicationState state,
        CancellationToken ct)
        => state.SiblingSnapshot is not null &&
           SnapshotMatches(state.SiblingSnapshot, state.PolicyPayload) &&
           CurrentRuntimeMatchesNoFollow(
               state.SiblingPath,
               state.SiblingSnapshot,
               ct);

    private static FileSnapshot ReadBoundedSnapshot(
        string path,
        int limit,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        using var stream = new FileStream(
            path, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (stream.Length < 0 || stream.Length > limit)
            throw new IOException("Artifact exceeds publication bound.");
        var bytes = new byte[checked((int)stream.Length)];
        stream.ReadExactly(bytes);
        ct.ThrowIfCancellationRequested();
        if (stream.Length != bytes.Length)
            throw new IOException("Artifact identity changed during read.");
        return SnapshotOf(bytes);
    }

    private static FileSnapshot ReadRuntimeSnapshotNoFollow(
        string path,
        int limit,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        if (OperatingSystem.IsLinux())
            return ReadRuntimeSnapshotNoFollowLinux(path, limit, ct);
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException(
                "No-follow runtime identity is not available on this platform.");
        EnsureNoReparseAmbiguity(path, includeLeaf: false);
        var handle = CreateFileForIdentity(
            path,
            GenericRead,
            FileShareReadWriteDelete,
            IntPtr.Zero,
            OpenExisting,
            FileFlagOpenReparsePoint,
            IntPtr.Zero);
        if (handle.IsInvalid)
        {
            var error = Marshal.GetLastWin32Error();
            handle.Dispose();
            if (error is ErrorFileNotFound or ErrorPathNotFound)
                return FileSnapshot.Missing;
            throw new IOException("Runtime checkpoint identity is unavailable.");
        }
        try
        {
            if (!GetFileInformationByHandleForIdentity(handle, out var info) ||
                (info.FileAttributes & (uint)FileAttributes.ReparsePoint) != 0 ||
                (info.FileAttributes & (uint)FileAttributes.Directory) != 0)
                throw new IOException("Runtime checkpoint leaf is not a regular file.");
            using var stream = new FileStream(handle, FileAccess.Read);
            if (stream.Length < 0 || stream.Length > limit)
                throw new IOException("Runtime checkpoint exceeds publication bound.");
            var bytes = new byte[checked((int)stream.Length)];
            stream.ReadExactly(bytes);
            ct.ThrowIfCancellationRequested();
            if (stream.Length != bytes.Length)
                throw new IOException("Runtime checkpoint identity changed during read.");
            return new FileSnapshot(
                true,
                bytes,
                ComputeSha256(bytes),
                $"win:{info.VolumeSerialNumber:x8}:{info.FileIndexHigh:x8}{info.FileIndexLow:x8}");
        }
        catch
        {
            handle.Dispose();
            throw;
        }
    }

    private static FileSnapshot ReadRuntimeSnapshotNoFollowLinux(
        string path,
        int limit,
        CancellationToken ct)
    {
        var snapshot = DesignConversionHandleBoundDirectory
            .ReadLinuxRegularFileSnapshotNoFollow(path, limit, ct);
        return snapshot.Exists
            ? new FileSnapshot(
                true,
                snapshot.Bytes,
                ComputeSha256(snapshot.Bytes),
                snapshot.PhysicalIdentity)
            : FileSnapshot.Missing;
    }

    private static bool CurrentRuntimeMatchesNoFollow(
        string path,
        FileSnapshot expected,
        CancellationToken ct,
        bool requireSameIdentity = true)
    {
        var current = ReadRuntimeSnapshotNoFollow(
            path, Math.Max(expected.Bytes.Length, 1), ct);
        if (current.Exists != expected.Exists)
            return false;
        if (!expected.Exists)
            return true;
        return current.Bytes.AsSpan().SequenceEqual(expected.Bytes) &&
               string.Equals(current.Sha256, expected.Sha256, StringComparison.Ordinal) &&
               (!requireSameIdentity || expected.PhysicalIdentity is null ||
                string.Equals(
                    current.PhysicalIdentity,
                    expected.PhysicalIdentity,
                    StringComparison.Ordinal));
    }

    private static bool CurrentMatches(
        string path,
        FileSnapshot expected,
        CancellationToken ct)
    {
        if (!File.Exists(path) || Directory.Exists(path))
            return false;
        var current = ReadBoundedSnapshot(
            path, Math.Max(expected.Bytes.Length, 1), ct);
        return current.Bytes.AsSpan().SequenceEqual(expected.Bytes) &&
               string.Equals(current.Sha256, expected.Sha256, StringComparison.Ordinal);
    }

    private static bool SnapshotMatches(FileSnapshot snapshot, byte[] expected)
        => snapshot.Exists && snapshot.Bytes.Length == expected.Length &&
           snapshot.Bytes.AsSpan().SequenceEqual(expected) &&
           string.Equals(snapshot.Sha256, ComputeSha256(expected), StringComparison.Ordinal);

    private static FileSnapshot SnapshotOf(byte[] bytes)
        => new(true, bytes, ComputeSha256(bytes));

    private static void WriteAndFlushNew(
        string path,
        byte[] bytes,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        using var stream = new FileStream(
            path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        stream.Write(bytes);
        stream.Flush(flushToDisk: true);
        ct.ThrowIfCancellationRequested();
    }

    private static string BuildStagePath(string destination, string kind)
        => destination + $".{kind}.{Guid.NewGuid():N}.tmp";

    private static bool PathExists(string path)
        => File.Exists(path) || Directory.Exists(path);

    private static string ComputeSha256(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static OperationResult<DesignConversionMachineLoadResult> PublicationFailed()
        => OperationResult<DesignConversionMachineLoadResult>.Fail(
            "Runtime checkpoint publication failed.", PublicationFailedErrorCode);

    private static OperationResult<byte[]> PolicyPayloadTooLarge()
        => PolicyPayloadTooLarge<byte[]>();

    private static OperationResult<T> PolicyPayloadTooLarge<T>()
        => OperationResult<T>.Fail(
            "Policy checkpoint payload exceeds the supported publication bound.",
            "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_TOO_LARGE");

    private static OperationResult<byte[]> PolicyPayloadReadFailed()
        => OperationResult<byte[]>.Fail(
            "Policy checkpoint payload could not be read safely.",
            "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_READ_FAILED");

    private static OperationResult<DesignConversionMachineLoadResult> RuntimePayloadTooLarge()
        => OperationResult<DesignConversionMachineLoadResult>.Fail(
            "Runtime checkpoint payload exceeds the supported publication bound.",
            "DESIGN_CONVERSION_RUNTIME_CHECKPOINT_TOO_LARGE");

    private static OperationResult<DesignConversionMachineLoadResult> PolicyCollision()
        => OperationResult<DesignConversionMachineLoadResult>.Fail(
            "Policy checkpoint destination contains different content.",
            PolicyCollisionErrorCode);

    private static OperationResult<DesignConversionMachineLoadResult> LoadGateFailed()
        => OperationResult<DesignConversionMachineLoadResult>.Fail(
            "Staged runtime checkpoint fresh-load failed.",
            "DESIGN_CONVERSION_RUNTIME_CHECKPOINT_FRESH_LOAD_FAILED");

    private static OperationResult<DesignConversionMachineLoadResult> RouteBGateFailed()
        => OperationResult<DesignConversionMachineLoadResult>.Fail(
            "Staged runtime checkpoint RouteB validation failed.",
            "DESIGN_CONVERSION_RUNTIME_CHECKPOINT_ROUTE_B_GATE_FAILED");

    private static OperationResult<DesignConversionMachineLoadResult>
        CommitProvisionalPublication(
            PublicationState state,
            DesignConversionMachineLoadResult loaded,
            Action<string>? observer,
            CancellationToken ct)
    {
        try
        {
            ct.ThrowIfCancellationRequested();
            if (state.RuntimeStagePath is null || state.RuntimeStageSnapshot is null ||
                !CurrentRuntimeMatchesNoFollow(
                    state.RuntimeStagePath,
                    state.RuntimeStageSnapshot,
                    ct) ||
                !PolicyArtifactStillMatches(state, ct) ||
                !RuntimeStillMatchesPrior(state, ct, requireSameIdentity: true))
            {
                return FailAfterRollback(
                    state,
                    PublicationFailed(),
                    "precommit_identity_failed",
                    observer);
            }

            observer?.Invoke("before-runtime-commit");
            if (!CurrentRuntimeMatchesNoFollow(
                    state.RuntimeStagePath,
                    state.RuntimeStageSnapshot,
                    ct) ||
                !PolicyArtifactStillMatches(state, ct) ||
                !RuntimeStillMatchesPrior(state, ct, requireSameIdentity: true))
            {
                return FailAfterRollback(
                    state,
                    PublicationFailed(),
                    "precommit_artifact_identity_failed",
                    observer);
            }

            if (state.PreviousRuntime?.Exists == true)
                File.Replace(state.RuntimeStagePath, state.RuntimePath, null);
            else
                File.Move(state.RuntimeStagePath, state.RuntimePath, overwrite: false);
            state.RuntimeStagePath = null;
            state.PublishedRuntimeSnapshot = state.RuntimeStageSnapshot;
            state.RuntimeStageSnapshot = null;
            state.PublicRuntimeCommitted = true;
            TryNotify(observer, "after-runtime");
            return OperationResult<DesignConversionMachineLoadResult>.Ok(loaded);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return FailAfterRollback(
                state,
                OperationResult<DesignConversionMachineLoadResult>.Fail(
                    "Bridge cancelled.",
                    "CANCELLED"),
                "cancelled",
                observer);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return FailAfterRollback(
                state,
                PublicationFailed(),
                "publication_failed",
                observer);
        }
    }

    internal sealed class ProvisionalPublicationLease : IDisposable
    {
        private readonly PublicationState _state;
        private readonly DesignConversionMachineLoadResult _loaded;
        private readonly Action<string>? _observer;
        private OwnedMutex? _destinationLock;
        private int _terminal;

        internal ProvisionalPublicationLease(
            PublicationState state,
            OwnedMutex destinationLock,
            DesignConversionMachineLoadResult loaded,
            Action<string>? observer)
        {
            _state = state;
            _destinationLock = destinationLock;
            _loaded = loaded;
            _observer = observer;
        }

        internal OperationResult<DesignConversionMachineLoadResult> Commit(
            CancellationToken ct)
        {
            if (Interlocked.CompareExchange(ref _terminal, 1, 0) != 0)
            {
                return OperationResult<DesignConversionMachineLoadResult>.Fail(
                    "Runtime checkpoint provisional publication is already terminal.",
                    PublicationStateUncertainErrorCode);
            }

            try
            {
                return CommitProvisionalPublication(
                    _state,
                    _loaded,
                    _observer,
                    ct);
            }
            finally
            {
                if (!_state.PublicRuntimeCommitted)
                    _ = RollbackAndProve(_state, observer: null);
                ReleaseDestinationLock();
            }
        }

        internal bool AbortAndProve()
        {
            if (Interlocked.CompareExchange(ref _terminal, 1, 0) != 0)
                return _state.PublicRuntimeCommitted;
            try
            {
                return RollbackAndProve(_state, _observer).Proven;
            }
            finally
            {
                ReleaseDestinationLock();
            }
        }

        public void Dispose()
            => _ = AbortAndProve();

        private void ReleaseDestinationLock()
            => Interlocked.Exchange(ref _destinationLock, null)?.Dispose();
    }

    internal sealed class PublicationState(
        string siblingPath,
        string runtimePath,
        byte[] policyPayload,
        bool ownsSibling)
    {
        internal string SiblingPath { get; } = siblingPath;
        internal string RuntimePath { get; } = runtimePath;
        internal byte[] PolicyPayload { get; } = policyPayload;
        internal bool OwnsSibling { get; } = ownsSibling;
        internal bool PolicyMustRemain { get; set; } = !ownsSibling;
        internal bool CreatedSibling { get; set; }
        internal FileSnapshot? SiblingSnapshot { get; set; }
        internal FileSnapshot? PreviousRuntime { get; set; }
        internal byte[]? PublishedRuntime { get; set; }
        internal FileSnapshot? PublishedRuntimeSnapshot { get; set; }
        internal bool PublicRuntimeCommitted { get; set; }
        internal string? PolicyStagePath { get; set; }
        internal FileSnapshot? PolicyStageSnapshot { get; set; }
        internal string? RuntimeStagePath { get; set; }
        internal FileSnapshot? RuntimeStageSnapshot { get; set; }
    }

    internal sealed record FileSnapshot(
        bool Exists,
        byte[] Bytes,
        string Sha256,
        string? PhysicalIdentity = null)
    {
        internal static FileSnapshot Missing { get; } = new(false, [], string.Empty);
    }

    private sealed record RollbackOutcome(bool Proven);

    internal sealed class OwnedMutex(
        SafeHandle mutex,
        Func<SafeHandle, bool> releaseNative,
        Func<int> getLastNativeError,
        Action<string>? observer) : IDisposable
    {
        private SafeHandle? _mutex = mutex;
        private readonly Func<SafeHandle, bool> _releaseNative = releaseNative;
        private readonly Func<int> _getLastNativeError = getLastNativeError;
        private Action<string>? _observer = observer;

        public void Dispose()
        {
            var current = Interlocked.Exchange(ref _mutex, null);
            if (current is null)
                return;
            var observer = Interlocked.Exchange(ref _observer, null);
            TryNotify(observer, "before-lock-release");
            try
            {
                try
                {
                    var releaseOverride = observer is null
                        ? null
                        : MutexReleaseForTesting;
                    var released = releaseOverride?.Invoke() ??
                                   _releaseNative(current);
                    if (!released)
                    {
                        var error = releaseOverride is null
                            ? _getLastNativeError()
                            : 0;
                        ReportMutexReleaseFailure(observer, error, "native_false");
                    }
                }
                catch (Exception exception)
                    when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
                {
                    ReportMutexReleaseFailure(
                        observer,
                        0,
                        $"managed_{exception.GetType().Name}");
                }
            }
            finally
            {
                try { current.Dispose(); }
                catch (Exception exception)
                    when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)) { }
            }
        }
    }

    private static void ReportMutexReleaseFailure(
        Action<string>? observer,
        int win32Error,
        string category)
    {
        try
        {
            System.Diagnostics.Trace.TraceError(
                "DesignConversion DPO runtime publication mutex release failed; " +
                "category={0}; win32_error={1}.",
                category,
                win32Error);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)) { }
        TryNotify(observer, "mutex-release-failed");
    }

    private static bool ReleaseWindowsPublicationLock(SafeHandle handle)
        => handle is SafeWaitHandle waitHandle && ReleaseMutex(waitHandle);

    private static bool ReleaseLinuxPublicationLock(SafeHandle handle)
        => DesignConversionHandleBoundDirectory
            .ReleaseLinuxPublicationSerializationLock(handle);

    [StructLayout(LayoutKind.Sequential)]
    private struct SecurityAttributes
    {
        internal int Length;
        internal IntPtr SecurityDescriptor;
        [MarshalAs(UnmanagedType.Bool)] internal bool InheritHandle;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ByHandleFileInformation
    {
        internal uint FileAttributes;
        internal System.Runtime.InteropServices.ComTypes.FILETIME CreationTime;
        internal System.Runtime.InteropServices.ComTypes.FILETIME LastAccessTime;
        internal System.Runtime.InteropServices.ComTypes.FILETIME LastWriteTime;
        internal uint VolumeSerialNumber;
        internal uint FileSizeHigh;
        internal uint FileSizeLow;
        internal uint NumberOfLinks;
        internal uint FileIndexHigh;
        internal uint FileIndexLow;
    }

    [DllImport("kernel32.dll", EntryPoint = "CreateFileW", SetLastError = true,
        CharSet = CharSet.Unicode)]
    private static extern SafeFileHandle CreateFileForIdentity(
        string fileName,
        uint desiredAccess,
        uint shareMode,
        IntPtr securityAttributes,
        uint creationDisposition,
        uint flagsAndAttributes,
        IntPtr templateFile);

    [DllImport("kernel32.dll", EntryPoint = "GetFileInformationByHandle", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetFileInformationByHandleForIdentity(
        SafeFileHandle fileHandle,
        out ByHandleFileInformation fileInformation);

    [DllImport("advapi32.dll", EntryPoint = "ConvertStringSecurityDescriptorToSecurityDescriptorW",
        SetLastError = true, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ConvertStringSecurityDescriptorToSecurityDescriptor(
        string stringSecurityDescriptor,
        uint stringSdRevision,
        out IntPtr securityDescriptor,
        out uint securityDescriptorSize);

    [DllImport("kernel32.dll", EntryPoint = "LocalFree", SetLastError = true)]
    private static extern IntPtr LocalFree(IntPtr memory);

    [DllImport("kernel32.dll", EntryPoint = "CreateMutexExW", SetLastError = true,
        CharSet = CharSet.Unicode)]
    private static extern SafeWaitHandle CreateMutexEx(
        ref SecurityAttributes mutexAttributes,
        string name,
        uint flags,
        uint desiredAccess);

    [DllImport("kernel32.dll", EntryPoint = "WaitForMultipleObjects", SetLastError = true)]
    private static extern uint WaitForMultipleObjects(
        uint count,
        [In] IntPtr[] handles,
        [MarshalAs(UnmanagedType.Bool)] bool waitAll,
        uint milliseconds);

    [DllImport("kernel32.dll", EntryPoint = "ReleaseMutex", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ReleaseMutex(SafeWaitHandle mutex);
}

internal enum DesignConversionDpoEvaluatedForwardPhase
{
    BeforeChildChosen,
    AfterChildChosen,
    BeforeChildRejected,
    AfterChildRejected,
    BeforeReferenceChosen,
    AfterReferenceChosen,
    BeforeReferenceRejected,
    AfterReferenceRejected
}

internal sealed record DesignConversionDpoEvaluatedForwardObservation(
    DesignConversionDpoEvaluatedForwardPhase Phase,
    string RecordIdentitySha256,
    string InputSha256);

internal enum DesignConversionDpoEvaluatedOwnershipPhase
{
    TrainerArtifactOwnershipProven,
    BeforeTrainerStepAdoption,
    AfterTrainerStepRetained,
    AfterScratchPolicyRetained,
    AfterScratchRuntimeRetained,
    AfterFinalPolicyRetained,
    AfterFinalRuntimeRetained,
    BeforeLeaseCreated,
    AfterScratchOwnershipCheckBeforeDelete
}

internal sealed record DesignConversionDpoEvaluatedOwnershipObservation(
    DesignConversionDpoEvaluatedOwnershipPhase Phase,
    string ScratchDirectory,
    string? ArtifactPath,
    int Step = 0,
    DpoTrainerArtifactOwnershipProof? TrainerArtifactProof = null);

internal sealed record DesignConversionDpoOptimizerMutationSinkObservation(
    string Stage,
    IReadOnlyList<object> AdmittedInputs,
    IReadOnlyList<DpoVectorBatch> Batches);

internal sealed record DesignConversionDpoContinuationMutationProofObservation(
    string Phase,
    string ExpectedModelSetStateFingerprint,
    string? ActualModelSetStateFingerprint);

internal sealed record DesignConversionDpoEvaluatedPackedBatch(
    DpoVectorBatch VectorBatch,
    string RecordIdentitySha256,
    string InputSha256,
    string Prompt,
    string Chosen,
    string Rejected,
    TrainingTeacherProvenance? Provenance)
{
    // Подготовленный evaluated batch хранит ожидаемых авторов отдельно от
    // валидируемого admission, чтобы подмененный паспорт не заверял сам себя.
    internal DpoPairSideAuthorKind? ChosenAuthorKind { get; init; }

    // Ожидаемый автор rejected остается отдельным ожиданием: generated/incumbent
    // направление юридически значимо и не выводится из tensor payload.
    internal DpoPairSideAuthorKind? RejectedAuthorKind { get; init; }
}

internal sealed record DesignConversionDpoEvaluatedMetricSummary(
    int CorrectCount,
    double Accuracy,
    int AbsoluteChildPreferenceCorrectCount,
    double AbsoluteChildPreferenceAccuracy,
    double MeanRelativeMargin,
    double MeanChildMargin,
    double MeanReferenceMargin);

internal sealed record DesignConversionDpoOwnedFile(
    string DirectoryPath,
    string LeafName,
    string Sha256,
    DesignConversionFileIdentity Identity);

internal sealed record DesignConversionDpoOwnedFilePayload(
    byte[] Payload,
    DesignConversionDpoOwnedFile OwnedFile);

internal sealed class DesignConversionDpoRetainedOwnedFile(
    DesignConversionDpoOwnedFile ownedFile,
    FileStream? retainedStream) : IDisposable
{
    internal DesignConversionDpoOwnedFile OwnedFile { get; set; } = ownedFile;
    internal FileStream? RetainedStream { get; } = retainedStream;
    internal string DirectoryPath => OwnedFile.DirectoryPath;
    internal string LeafName => OwnedFile.LeafName;
    internal string Sha256 => OwnedFile.Sha256;
    internal DesignConversionFileIdentity Identity => OwnedFile.Identity;

    public void Dispose() => RetainedStream?.Dispose();
}

internal sealed class DesignConversionDpoEvaluatedBridgeLease : IDisposable
{
    private readonly string _policyPath;
    private readonly string _runtimePath;
    private readonly string _directoryPath;
    private readonly string _policyLeaf;
    private readonly string _runtimeLeaf;
    private readonly string _policySha256;
    private readonly string _runtimeSha256;
    private readonly DesignConversionFileIdentity _policyIdentity;
    private readonly DesignConversionFileIdentity _runtimeIdentity;
    private readonly ServerOwnedLegalTrainingTeacherAdmissionLease?
        _ownedAdmissionLease;
    private DesignConversionHandleBoundDirectory? _retainedDirectory;
    private DesignConversionDpoRetainedOwnedFile? _retainedPolicy;
    private DesignConversionDpoRetainedOwnedFile? _retainedRuntime;
    private int _committed;
    private int _disposed;

    internal DesignConversionDpoEvaluatedBridgeLease(
        DesignConversionDpoPolicyRuntimeCheckpointBridgeResult result,
        DesignConversionDpoHeldOutEvaluation evaluation,
        string policyPath,
        string runtimePath,
        string policySha256,
        string runtimeSha256)
        : this(
            result,
            evaluation,
            policyPath,
            runtimePath,
            policySha256,
            runtimeSha256,
            expectedPolicyIdentity: null,
            expectedRuntimeIdentity: null,
            ownedAdmissionLease: null,
            retainedDirectory: null,
            retainedPolicy: null,
            retainedRuntime: null)
    {
    }

    internal DesignConversionDpoEvaluatedBridgeLease(
        DesignConversionDpoPolicyRuntimeCheckpointBridgeResult result,
        DesignConversionDpoHeldOutEvaluation evaluation,
        string policyPath,
        string runtimePath,
        string policySha256,
        string runtimeSha256,
        DesignConversionFileIdentity? expectedPolicyIdentity,
        DesignConversionFileIdentity? expectedRuntimeIdentity)
        : this(
            result,
            evaluation,
            policyPath,
            runtimePath,
            policySha256,
            runtimeSha256,
            expectedPolicyIdentity,
            expectedRuntimeIdentity,
            ownedAdmissionLease: null,
            retainedDirectory: null,
            retainedPolicy: null,
            retainedRuntime: null)
    {
    }

    internal DesignConversionDpoEvaluatedBridgeLease(
        DesignConversionDpoPolicyRuntimeCheckpointBridgeResult result,
        DesignConversionDpoHeldOutEvaluation evaluation,
        string policyPath,
        string runtimePath,
        string policySha256,
        string runtimeSha256,
        DesignConversionFileIdentity? expectedPolicyIdentity,
        DesignConversionFileIdentity? expectedRuntimeIdentity,
        ServerOwnedLegalTrainingTeacherAdmissionLease? ownedAdmissionLease,
        DesignConversionHandleBoundDirectory? retainedDirectory = null,
        DesignConversionDpoRetainedOwnedFile? retainedPolicy = null,
        DesignConversionDpoRetainedOwnedFile? retainedRuntime = null)
    {
        Result = result;
        Evaluation = evaluation;
        _policyPath = policyPath;
        _runtimePath = runtimePath;
        _directoryPath = Path.GetDirectoryName(policyPath) ?? string.Empty;
        _policyLeaf = Path.GetFileName(policyPath);
        _runtimeLeaf = Path.GetFileName(runtimePath);
        _policySha256 = policySha256;
        _runtimeSha256 = runtimeSha256;
        _ownedAdmissionLease = ownedAdmissionLease;
        if (OperatingSystem.IsLinux() &&
            retainedDirectory is not null &&
            retainedPolicy?.RetainedStream is not null &&
            retainedRuntime?.RetainedStream is not null)
        {
            var policyStream = retainedPolicy.RetainedStream;
            var runtimeStream = retainedRuntime.RetainedStream;
            _policyIdentity = retainedDirectory.GetIdentity(policyStream, _policyLeaf);
            _runtimeIdentity = retainedDirectory.GetIdentity(runtimeStream, _runtimeLeaf);
            if (_policyIdentity != retainedPolicy.Identity ||
                _runtimeIdentity != retainedRuntime.Identity ||
                retainedDirectory.GetLinkCount(policyStream, _policyLeaf) != 1 ||
                retainedDirectory.GetLinkCount(runtimeStream, _runtimeLeaf) != 1 ||
                !string.Equals(HashStream(policyStream), policySha256, StringComparison.Ordinal) ||
                !string.Equals(HashStream(runtimeStream), runtimeSha256, StringComparison.Ordinal))
            {
                throw new IOException("Evaluated final artifact retained custody changed.");
            }

            // Объект результата получает те же открытые файловые дескрипторы,
            // которые появились при создании файлов. Они доказывают владение
            // до подтверждения результата или его отмены и очистки.
            _retainedDirectory = retainedDirectory;
            _retainedPolicy = retainedPolicy;
            _retainedRuntime = retainedRuntime;
            return;
        }

        // На Windows эти retained handles не становятся частью lease custody:
        // проверка ниже заново удерживает path/identity без живого descriptor.
        // Закрываем перед readback, иначе успешная публикация оставляет
        // открытый каталог и блокирует последующую очистку namespace.
        retainedRuntime?.Dispose();
        retainedPolicy?.Dispose();
        retainedDirectory?.Dispose();

        using var directory = DesignConversionHandleBoundDirectory.AcquireOrCreate(
            _directoryPath,
            beforeIo: null);
        using var policyReadback = directory.TryOpenExisting(
            _policyLeaf,
            FileShare.Read,
            writeThrough: false,
            expectedPolicyIdentity);
        using var runtimeReadback = directory.TryOpenExisting(
            _runtimeLeaf,
            FileShare.Read,
            writeThrough: false,
            expectedRuntimeIdentity);
        if (policyReadback is null || runtimeReadback is null)
            throw new IOException("Evaluated final artifact ownership could not be retained.");
        _policyIdentity = directory.GetIdentity(policyReadback, _policyLeaf);
        _runtimeIdentity = directory.GetIdentity(runtimeReadback, _runtimeLeaf);
        if (!string.Equals(HashStream(policyReadback), policySha256, StringComparison.Ordinal) ||
            !string.Equals(HashStream(runtimeReadback), runtimeSha256, StringComparison.Ordinal))
        {
            throw new IOException("Evaluated final artifact ownership hash changed.");
        }
    }

    private static string HashStream(Stream stream)
    {
        stream.Position = 0;
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    internal DesignConversionDpoPolicyRuntimeCheckpointBridgeResult Result { get; }
    internal DesignConversionDpoHeldOutEvaluation Evaluation { get; }

    internal void CommitPublishedArtifacts()
        => Interlocked.Exchange(ref _committed, 1);

    internal OperationResult<IDisposable> AcquireMutationPermit()
        => _ownedAdmissionLease is null
            ? ServerOwnedLegalTrainingTeacherAdmissionAuthority.RevokedPermit()
            : _ownedAdmissionLease.AcquireMutationPermit();

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;
        try
        {
            if (Volatile.Read(ref _committed) == 0)
            {
                if (_retainedDirectory is not null &&
                    _retainedPolicy is not null &&
                    _retainedRuntime is not null)
                {
                    DeleteRetainedIdentityAndHashMatchedBestEffort(
                        _retainedDirectory,
                        _retainedRuntime);
                    DeleteRetainedIdentityAndHashMatchedBestEffort(
                        _retainedDirectory,
                        _retainedPolicy);
                }
                else
                {
                    if (OperatingSystem.IsWindows())
                    {
                        DeleteIdentityMatchedBestEffort(
                            _directoryPath, _runtimeLeaf, _runtimePath, _runtimeSha256, _runtimeIdentity);
                        DeleteIdentityMatchedBestEffort(
                            _directoryPath, _policyLeaf, _policyPath, _policySha256, _policyIdentity);
                    }
                    else
                    {
                        PreserveUnretainedCleanupOnNonWindows(_runtimePath);
                        PreserveUnretainedCleanupOnNonWindows(_policyPath);
                    }
                }
            }
        }
        finally
        {
            try
            {
                _retainedRuntime?.Dispose();
                _retainedPolicy?.Dispose();
                _retainedRuntime = null;
                _retainedPolicy = null;
                _retainedDirectory?.Dispose();
                _retainedDirectory = null;
            }
            finally
            {
                _ownedAdmissionLease?.Dispose();
            }
        }
    }

    private static void DeleteRetainedIdentityAndHashMatchedBestEffort(
        DesignConversionHandleBoundDirectory directory,
        DesignConversionDpoRetainedOwnedFile artifact)
    {
        try
        {
            var stream = artifact.RetainedStream;
            if (stream is null ||
                directory.GetIdentity(stream, artifact.LeafName) != artifact.Identity ||
                directory.GetLinkCount(stream, artifact.LeafName) != 1 ||
                !string.Equals(HashStream(stream), artifact.Sha256, StringComparison.Ordinal))
            {
                return;
            }

            directory.DeleteHeldRegular(stream, artifact.LeafName, artifact.Identity);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
        }
    }

    private static void DeleteIdentityMatchedBestEffort(
        string directoryPath,
        string leaf,
        string path,
        string expectedSha256,
        DesignConversionFileIdentity expectedIdentity)
    {
        if (PreserveUnretainedCleanupOnNonWindows(path))
            return;

        try
        {
            if (!File.Exists(path))
                return;
            var actual = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)))
                .ToLowerInvariant();
            if (string.Equals(actual, expectedSha256, StringComparison.Ordinal))
            {
                using var directory = DesignConversionHandleBoundDirectory.AcquireOrCreate(directoryPath, beforeIo: null);
                directory.DeleteIfRegular(leaf, expectedIdentity);
            }
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
        }
    }

    private static bool PreserveUnretainedCleanupOnNonWindows(string path)
    {
        if (OperatingSystem.IsWindows())
            return false;

        // Linux/WSL может снова выдать тот же номер устройства, индексный номер
        // файла и контрольную сумму по тому же пути после закрытия исходного
        // файлового дескриптора. Без живого файлового дескриптора и удержанного
        // дескриптора каталога безопаснее сохранить долг очистки, чем удалить
        // замену, которая уже может принадлежать другому владельцу.
        Trace.TraceWarning(
            "DesignConversion DPO cleanup preserved unretained artifact for manual cleanup: {0}",
            path);
        return true;
    }
}

public sealed record DesignConversionDpoPolicyCheckpointBridgeOptions(
    int MaxBridgePreferenceCount = DesignConversionDpoPolicyCheckpointBridgeOptions.DefaultMaxBridgePreferenceCount,
    int DpoStepCount = DesignConversionDpoPolicyCheckpointBridgeOptions.DefaultDpoStepCount)
{
    public const int MaxAllowedBridgePreferenceCount = 1_024;
    public const int MaxAllowedDpoStepCount = LogicalDpoTrainingUnit.MaximumSupportedMaxStepCount;
    public const int DefaultMaxBridgePreferenceCount = MaxAllowedBridgePreferenceCount;
    public const int DefaultDpoStepCount = MaxAllowedDpoStepCount;
    public float DpoConvergenceThreshold { get; init; } = 0.002f;
    public int DpoConvergenceWindowSteps { get; init; } = 1;
}

internal sealed record DesignConversionDpoRuntimeInferenceEncodingBinding(
    string BindingSchema,
    string SourceDescriptorSchema,
    string PolicyTokenMapping,
    int TokenIdMinimum,
    int TokenIdMaximum,
    int ContextLength,
    string TokenizerArtifactSha256,
    string TokenizerVocabularyIdentitySha256);

public static class DesignConversionDpoPolicyCheckpointBridge
{
    private static readonly JsonSerializerOptions RuntimeInferenceCanonicalJsonOptions =
        new(DesignConversionCheckpointJsonOptions.Standard)
        {
            WriteIndented = false
        };
    private static readonly AsyncLocal<Action<string>?> RuntimeTransactionObserver = new();
    private static readonly AsyncLocal<Action<string>?> LeaseAcquisitionObserver = new();
    private static readonly AsyncLocal<Action<string>?> MutationBoundaryObserver = new();
    private static readonly AsyncLocal<Action<
        DesignConversionDpoOptimizerMutationSinkObservation>?>
        OptimizerMutationSinkObserver = new();
    private static readonly AsyncLocal<Action<
        DesignConversionDpoContinuationMutationProofObservation>?>
        ContinuationMutationProofObserver = new();
    internal static Action<string>? RuntimeTransactionStageObserverForTesting
    {
        get => RuntimeTransactionObserver.Value;
        set => RuntimeTransactionObserver.Value = value;
    }
    internal static Action<string>? LeaseAcquisitionObserverForTesting
    {
        get => LeaseAcquisitionObserver.Value;
        set => LeaseAcquisitionObserver.Value = value;
    }
    internal static Action<string>? MutationBoundaryObserverForTesting
    {
        get => MutationBoundaryObserver.Value;
        set => MutationBoundaryObserver.Value = value;
    }
    internal static Action<DesignConversionDpoOptimizerMutationSinkObservation>?
        OptimizerMutationSinkObserverForTesting
    {
        get => OptimizerMutationSinkObserver.Value;
        set => OptimizerMutationSinkObserver.Value = value;
    }

    internal static Action<DesignConversionDpoContinuationMutationProofObservation>?
        ContinuationMutationProofObserverForTesting
    {
        get => ContinuationMutationProofObserver.Value;
        set => ContinuationMutationProofObserver.Value = value;
    }
    private const uint ScratchDeleteAccess = 0x00010000;
    private const uint ScratchReadAttributesAccess = 0x00000080;
    private const uint ScratchSynchronizeAccess = 0x00100000;
    private const uint ScratchFileAttributeDirectory = 0x00000010;
    private const uint ScratchFileAttributeReparsePoint = 0x00000400;
    private const uint ScratchFileFlagBackupSemantics = 0x02000000;
    private const uint ScratchFileFlagOpenReparsePoint = 0x00200000;
    private const uint ScratchOpenExisting = 3;
    private const int ScratchFileAttributeTagInfoClass = 9;
    private const int ScratchFileDispositionInfoClass = 4;
    private const long ContinuationProofMaximumCheckpointBytes =
        16L * 1024 * 1024 * 1024;
    private const int ContinuationProofHashBufferBytes = 128 * 1024;
    private const string InvalidOptionsErrorCode = "DESIGN_CONVERSION_DPO_POLICY_BRIDGE_OPTIONS_INVALID";
    private const string InvalidOptionsErrorMessage = "DesignConversion DpoTrainer policy bridge options are invalid.";
    public const string PairAuthorshipMissingOrInvalidErrorCode =
        "DESIGN_CONVERSION_DPO_PAIR_AUTHORSHIP_MISSING_OR_INVALID";
    internal const string PairChosenSourceSideMetadataKey =
        "dpo_pair_chosen_source_side";
    internal const string PairRejectedSourceSideMetadataKey =
        "dpo_pair_rejected_source_side";
    internal const string PairAuthorshipSourceMetadataKey =
        "dpo_pair_authorship_source";
    internal const string PairChosenAuthorKindMetadataKey =
        "dpo_pair_chosen_author_kind";
    internal const string PairRejectedAuthorKindMetadataKey =
        "dpo_pair_rejected_author_kind";
    internal const string PairSourceSideGenerated =
        DesignConversionIndependentPreferenceEvidenceContract.GeneratedSide;
    internal const string PairSourceSideIncumbent =
        DesignConversionIndependentPreferenceEvidenceContract.IncumbentSide;
    public const string PolicyRandomContextRequiredErrorCode =
        "DESIGN_CONVERSION_DPO_POLICY_RANDOM_CONTEXT_REQUIRED";
    public const string PolicyRandomContextInvalidErrorCode =
        "DESIGN_CONVERSION_DPO_POLICY_RANDOM_CONTEXT_INVALID";
    private const string PolicyRandomContextRequiredErrorMessage =
        "DesignConversion DPO policy random context is required.";
    private const string PolicyRandomContextInvalidErrorMessage =
        "DesignConversion DPO policy random context is invalid.";
    public const string ContinuationProviderRequiredErrorCode =
        "DESIGN_CONVERSION_DPO_POLICY_CONTINUATION_PROVIDER_REQUIRED";
    public const string ContinuationRuntimeEncodingInvalidErrorCode =
        "DESIGN_CONVERSION_DPO_POLICY_CONTINUATION_RUNTIME_ENCODING_INVALID";
    private const string ContinuationProviderRequiredErrorMessage =
        "A DesignConversion DPO continuation-capable policy provider is required.";
    private const string ContinuationRuntimeEncodingInvalidErrorMessage =
        "The DesignConversion DPO continuation runtime encoding binding is invalid.";
    private const string LegalTeacherConfigurationRequiredErrorCode =
        "LEGAL_TEACHER_CONFIGURATION_REQUIRED";
    private const string LegalTeacherConfigurationRequiredErrorMessage =
        "Server-owned legal teacher admission configuration is required.";
    private const string LegacyAuthorityShimObsoleteMessage =
        "A server-owned legal teacher admission authority is required. Use the overload whose first parameter is ServerOwnedLegalTrainingTeacherAdmissionAuthority.";
    internal const string DirectInferenceRuntimeBridgeKind =
        DesignConversionMachine.DpoPolicyDirectInferenceRuntimeBridgeKind;
    [Obsolete(LegacyAuthorityShimObsoleteMessage, error: false)]
    public static Task<OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>> ExecuteAndConvertAsync(
        IReadOnlyList<DesignConversionDpoTrainingRecord>? records,
        string outputDirectory,
        string runId,
        string runtimeCheckpointPath,
        IDesignConversionDpoPolicyBaseProvider? policyBaseProvider,
        CancellationToken ct = default)
        => LegacyAuthorityRequired(ct);

    [Obsolete(LegacyAuthorityShimObsoleteMessage, error: false)]
    public static Task<OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>> ExecuteAndConvertAsync(
        IReadOnlyList<DesignConversionDpoTrainingRecord>? records,
        string outputDirectory,
        string runId,
        string runtimeCheckpointPath,
        IDesignConversionDpoPolicyBaseProvider? policyBaseProvider,
        DesignConversionDpoPolicyCheckpointBridgeOptions? options,
        CancellationToken ct = default)
        => LegacyAuthorityRequired(ct);

    [Obsolete(LegacyAuthorityShimObsoleteMessage, error: false)]
    public static Task<OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>> ExecuteAndConvertAsync(
        IReadOnlyList<DesignConversionDpoTrainingRecord>? records,
        string outputDirectory,
        string runId,
        string runtimeCheckpointPath,
        IDesignConversionDpoPolicyBaseProvider? policyBaseProvider,
        DesignConversionDpoPolicyCheckpointBridgeOptions? options,
        DesignConversionDpoPolicyContinuationRequest continuationRequest,
        CancellationToken ct = default)
        => LegacyAuthorityRequired(ct);

    public static Task<OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>> ExecuteAndConvertAsync(
        ServerOwnedLegalTrainingTeacherAdmissionAuthority legalTeacherAdmissionAuthority,
        IReadOnlyList<DesignConversionDpoTrainingRecord>? records,
        string outputDirectory,
        string runId,
        string runtimeCheckpointPath,
        IDesignConversionDpoPolicyBaseProvider? policyBaseProvider,
        CancellationToken ct = default)
        => ExecuteAndConvertAsync(
            legalTeacherAdmissionAuthority,
            records,
            outputDirectory,
            runId,
            runtimeCheckpointPath,
            policyBaseProvider,
            options: null,
            ct);

    public static Task<OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>> ExecuteAndConvertAsync(
        ServerOwnedLegalTrainingTeacherAdmissionAuthority legalTeacherAdmissionAuthority,
        IReadOnlyList<DesignConversionDpoTrainingRecord>? records,
        string outputDirectory,
        string runId,
        string runtimeCheckpointPath,
        IDesignConversionDpoPolicyBaseProvider? policyBaseProvider,
        DesignConversionDpoPolicyCheckpointBridgeOptions? options,
        CancellationToken ct = default)
        => ExecuteWithAuthorityCoreAsync(
            legalTeacherAdmissionAuthority,
            records,
            outputDirectory,
            runId,
            runtimeCheckpointPath,
            policyBaseProvider,
            options,
            continuationRequest: null,
            useContinuation: false,
            ct);

    public static Task<OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>> ExecuteAndConvertAsync(
        ServerOwnedLegalTrainingTeacherAdmissionAuthority legalTeacherAdmissionAuthority,
        IReadOnlyList<DesignConversionDpoTrainingRecord>? records,
        string outputDirectory,
        string runId,
        string runtimeCheckpointPath,
        IDesignConversionDpoPolicyBaseProvider? policyBaseProvider,
        DesignConversionDpoPolicyCheckpointBridgeOptions? options,
        DesignConversionDpoPolicyContinuationRequest continuationRequest,
        CancellationToken ct = default)
        => ExecuteWithAuthorityCoreAsync(
            legalTeacherAdmissionAuthority,
            records,
            outputDirectory,
            runId,
            runtimeCheckpointPath,
            policyBaseProvider,
            options,
            continuationRequest,
            useContinuation: true,
            ct);

    internal static Task<OperationResult<DesignConversionDpoEvaluatedBridgeLease>>
        ExecuteEvaluatedAndConvertAsync(
            ServerOwnedLegalTrainingTeacherAdmissionAuthority?
                legalTeacherAdmissionAuthority,
            DesignConversionDpoResolvedPartition partition,
            string outputDirectory,
            string runId,
            string runtimeCheckpointPath,
            IDesignConversionDpoPolicyBaseProvider? policyBaseProvider,
            DesignConversionDpoPolicyCheckpointBridgeOptions? options,
            Action<DesignConversionDpoEvaluatedForwardObservation>? forwardObserverForTesting = null,
            Action<DesignConversionDpoEvaluatedOwnershipObservation>? ownershipObserverForTesting = null,
            CancellationToken ct = default,
            Action<TrainingTeacherProvenance?>?
                legalTeacherEnumerationObserverForTesting = null)
        => ExecuteEvaluatedCoreAsync(
            legalTeacherAdmissionAuthority,
            partition,
            outputDirectory,
            runId,
            runtimeCheckpointPath,
            policyBaseProvider,
            options,
            continuationRequest: null,
            useContinuation: false,
            forwardObserverForTesting,
            ownershipObserverForTesting,
            ct,
            legalTeacherEnumerationObserverForTesting);

    internal static Task<OperationResult<DesignConversionDpoEvaluatedBridgeLease>>
        ExecuteEvaluatedAndConvertAsync(
            ServerOwnedLegalTrainingTeacherAdmissionAuthority?
                legalTeacherAdmissionAuthority,
            DesignConversionDpoResolvedPartition partition,
            string outputDirectory,
            string runId,
            string runtimeCheckpointPath,
            IDesignConversionDpoPolicyBaseProvider? policyBaseProvider,
            DesignConversionDpoPolicyCheckpointBridgeOptions? options,
            DesignConversionDpoPolicyContinuationRequest continuationRequest,
            Action<DesignConversionDpoEvaluatedForwardObservation>? forwardObserverForTesting = null,
            Action<DesignConversionDpoEvaluatedOwnershipObservation>? ownershipObserverForTesting = null,
            CancellationToken ct = default,
            Action<TrainingTeacherProvenance?>?
                legalTeacherEnumerationObserverForTesting = null)
        => ExecuteEvaluatedCoreAsync(
            legalTeacherAdmissionAuthority,
            partition,
            outputDirectory,
            runId,
            runtimeCheckpointPath,
            policyBaseProvider,
            options,
            continuationRequest,
            useContinuation: true,
            forwardObserverForTesting,
            ownershipObserverForTesting,
            ct,
            legalTeacherEnumerationObserverForTesting);

    internal static async Task<OperationResult<PreparedEvaluatedCompositeExecution>>
        PrepareEvaluatedCompositeExecutionAsync(
            ServerOwnedLegalTrainingTeacherAdmissionAuthority?
                legalTeacherAdmissionAuthority,
            DesignConversionDpoResolvedPartition partition,
            IDesignConversionDpoPolicyBaseProvider? policyBaseProvider,
            DesignConversionDpoPolicyCheckpointBridgeOptions? options,
            DesignConversionDpoPolicyContinuationRequest? continuationRequest,
            bool useContinuation,
            CancellationToken ct = default,
            Action<TrainingTeacherProvenance?>?
                legalTeacherEnumerationObserverForTesting = null,
            IReadOnlyList<DesignConversionDpoTrainingRecord>?
                committedExternalTransferTrainRecords = null)
    {
        if (ct.IsCancellationRequested)
            return Cancelled<PreparedEvaluatedCompositeExecution>();

        if (legalTeacherAdmissionAuthority is null ||
            !legalTeacherAdmissionAuthority.IsAvailable)
        {
            return OperationResult<PreparedEvaluatedCompositeExecution>.Fail(
                LegalTeacherConfigurationRequiredErrorMessage,
                LegalTeacherConfigurationRequiredErrorCode);
        }

        var validatedOptions = ValidateOptions(options);
        if (!validatedOptions.IsSuccess || validatedOptions.Value is null)
            return validatedOptions.MapError<PreparedEvaluatedCompositeExecution>();
        var effectiveOptions = validatedOptions.Value;
        if (partition is null || string.IsNullOrWhiteSpace(partition.DatasetId) ||
            partition.TrainRecords is null || partition.TrainRecords.Count == 0 ||
            partition.HeldOutRecords is null || partition.HeldOutRecords.Count == 0 ||
            !DesignConversionDpoConsumedPartitionProjectionContract.TryCreate(
                partition,
                effectiveOptions.MaxBridgePreferenceCount,
                out var consumedPartition) ||
            consumedPartition is null)
        {
            return EvaluatedPackingInvalid<PreparedEvaluatedCompositeExecution>();
        }
        if (committedExternalTransferTrainRecords is not null)
        {
            var transferProjection =
                BuildCommittedExternalTransferConsumedPartition(
                    consumedPartition,
                    committedExternalTransferTrainRecords,
                    effectiveOptions.MaxBridgePreferenceCount);
            if (!transferProjection.IsSuccess ||
                transferProjection.Value is null)
            {
                return transferProjection.MapError<
                    PreparedEvaluatedCompositeExecution>();
            }

            consumedPartition = transferProjection.Value;
        }

        // Trainability закрывается до legal-preflight/optimizer: даже held-out
        // остается evaluation-only, но не может обходить лицензию или quarantine.
        var trainability = ValidateEvaluatedConsumedPartitionTrainability(
            consumedPartition);
        if (!trainability.IsSuccess)
            return trainability.MapError<PreparedEvaluatedCompositeExecution>();

        var combinedPreflight = ValidateCombinedTeacherProvenancePreflight(
            legalTeacherAdmissionAuthority,
            consumedPartition,
            legalTeacherEnumerationObserverForTesting,
            ct);
        if (!combinedPreflight.IsSuccess)
            return combinedPreflight.MapError<PreparedEvaluatedCompositeExecution>();

        DesignConversionDpoPolicyContinuationRequest? continuationSnapshot;
        try
        {
            continuationSnapshot = useContinuation
                ? SnapshotContinuationRequest(continuationRequest)
                : null;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return EvaluatedPackingInvalid<PreparedEvaluatedCompositeExecution>();
        }
        if (useContinuation && continuationSnapshot is null)
            return EvaluatedPackingInvalid<PreparedEvaluatedCompositeExecution>();

        var providerPreparation = PreparePolicyBaseDescriptor(
            policyBaseProvider,
            continuationSnapshot,
            useContinuation,
            requireEvaluatedContinuationParent: true);
        if (!providerPreparation.IsSuccess || providerPreparation.Value is null)
            return providerPreparation.MapError<PreparedEvaluatedCompositeExecution>();

        var proofPreparation =
            await AttachBridgeOwnedContinuationMutationProofAsync(
                providerPreparation.Value,
                continuationSnapshot,
                useContinuation,
                ct);
        if (!proofPreparation.IsSuccess || proofPreparation.Value is null)
            return proofPreparation.MapError<PreparedEvaluatedCompositeExecution>();

        var config = proofPreparation.Value.Config;
        if (config.ContextLen < 2 || config.VocabSize < 256)
            return EvaluatedPackingInvalid<PreparedEvaluatedCompositeExecution>();
        var selectedTrainRows = consumedPartition.TrainRecords;
        var heldOutRows = consumedPartition.HeldOutRecords;
        var descriptor = DesignConversionDpoSourceEncoderContractBuilder
            .BuildEvaluatedDescriptor(selectedTrainRows, config);
        if (!descriptor.IsSuccess || descriptor.Value is null)
            return descriptor.MapError<PreparedEvaluatedCompositeExecution>();

        DesignConversionDpoRuntimeEncodingBinding? runtimeEncodingBinding = null;
        if (useContinuation)
        {
            var runtimeEncoding = ValidateContinuationRuntimeEncodingForPacking(
                continuationSnapshot!.RuntimeEncodingBinding,
                descriptor.Value,
                config);
            if (!runtimeEncoding.IsSuccess || runtimeEncoding.Value is null)
                return runtimeEncoding.MapError<PreparedEvaluatedCompositeExecution>();
            runtimeEncodingBinding = runtimeEncoding.Value;
        }

        var trainPackedResult = BuildEvaluatedPackedBatches(
            selectedTrainRows,
            config,
            includeTrainingVectorAndWeight: true,
            runtimeEncodingBinding);
        if (!trainPackedResult.IsSuccess || trainPackedResult.Value is null)
            return trainPackedResult.MapError<PreparedEvaluatedCompositeExecution>();
        var heldOutPackedResult = BuildEvaluatedPackedBatches(
            heldOutRows,
            config,
            includeTrainingVectorAndWeight: false,
            runtimeEncodingBinding);
        if (!heldOutPackedResult.IsSuccess || heldOutPackedResult.Value is null)
            return heldOutPackedResult.MapError<PreparedEvaluatedCompositeExecution>();
        var trainPacked = trainPackedResult.Value;
        var heldOutPacked = heldOutPackedResult.Value;
        var trainAdmissionValidation =
            ValidateEvaluatedTrainPackedAdmissions(trainPacked);
        if (!trainAdmissionValidation.IsSuccess)
        {
            return trainAdmissionValidation.MapError<
                PreparedEvaluatedCompositeExecution>();
        }
        var heldOutAdmissionValidation =
            ValidateEvaluatedHeldOutAdmissions(heldOutPacked);
        if (!heldOutAdmissionValidation.IsSuccess)
        {
            return heldOutAdmissionValidation.MapError<
                PreparedEvaluatedCompositeExecution>();
        }
        var trainInputSha256 = RecomputeEvaluatedInputFingerprint(
            selectedTrainRows,
            trainPacked.Select(static value => value.VectorBatch).ToArray(),
            config,
            includeTrainingVectorAndWeight: true);
        var heldOutInputSha256 = RecomputeEvaluatedInputFingerprint(
            heldOutRows,
            heldOutPacked.Select(static value => value.VectorBatch).ToArray(),
            config,
            includeTrainingVectorAndWeight: false);
        if (string.IsNullOrEmpty(trainInputSha256) ||
            string.IsNullOrEmpty(heldOutInputSha256))
        {
            return OperationResult<PreparedEvaluatedCompositeExecution>.Fail(
                "DesignConversion DPO evaluated input fingerprint is invalid.",
                EvaluatedInputFingerprintInvalidErrorCode);
        }

        var finalOptimizer = DesignConversionDpoTrainingContentAuthority
            .ProjectFinalOptimizerInputs(
                trainPacked,
                trainPacked.Select(static input => input.Provenance).ToArray(),
                static input => input.Prompt,
                static input => input.Chosen,
                static input => input.Rejected,
                static input => input.VectorBatch.TeachingVector,
                static input => input.VectorBatch.Weight,
                DesignConversionDpoTrainingContentAuthority
                    .BuildEvaluatedBridgeFinalOptimizerContext);
        if (!finalOptimizer.IsSuccess || finalOptimizer.Value is null)
            return finalOptimizer.MapError<PreparedEvaluatedCompositeExecution>();
        var heldOutLegalAdmission = ValidateExactHeldOutEvaluationBatch(
            legalTeacherAdmissionAuthority,
            heldOutPacked);
        if (!heldOutLegalAdmission.IsSuccess || heldOutLegalAdmission.Value is null)
            return heldOutLegalAdmission.MapError<PreparedEvaluatedCompositeExecution>();

        return OperationResult<PreparedEvaluatedCompositeExecution>.Ok(
            new PreparedEvaluatedCompositeExecution(
                consumedPartition,
                effectiveOptions,
                continuationSnapshot,
                useContinuation,
                new PreparedEvaluatedOptimizer(
                    proofPreparation.Value,
                    descriptor.Value,
                    runtimeEncodingBinding,
                    finalOptimizer.Value.Inputs,
                    heldOutPacked,
                    trainInputSha256,
                    heldOutInputSha256),
                finalOptimizer.Value,
                heldOutLegalAdmission.Value));
    }

    private static OperationResult<DesignConversionDpoConsumedPartitionProjection>
        BuildCommittedExternalTransferConsumedPartition(
            DesignConversionDpoConsumedPartitionProjection consumedPartition,
            IReadOnlyList<DesignConversionDpoTrainingRecord>
                committedExternalTransferTrainRecords,
            int maximumTrainRecords)
    {
        if (committedExternalTransferTrainRecords is null ||
            committedExternalTransferTrainRecords.Count !=
            consumedPartition.TrainRecords.Count)
        {
            return EvaluatedPackingInvalid<
                DesignConversionDpoConsumedPartitionProjection>();
        }

        // Узкий committed-transfer override меняет только carrying rows для
        // evaluated packing: identity/source sets остаются из уже принятой
        // partition, а authority пришла из store-owned transaction/group/child.
        var transferPartition = new DesignConversionDpoResolvedPartition(
            consumedPartition.DatasetId,
            committedExternalTransferTrainRecords,
            consumedPartition.HeldOutRecords,
            consumedPartition.TrainRecordSetSha256,
            consumedPartition.HeldOutRecordSetSha256,
            consumedPartition.TrainSourceSetSha256,
            consumedPartition.HeldOutSourceSetSha256,
            PartitionSnapshotSha256: string.Empty);
        if (!DesignConversionDpoConsumedPartitionProjectionContract.TryCreate(
                transferPartition,
                maximumTrainRecords,
                out var transferProjection) ||
            transferProjection is null ||
            !string.Equals(
                transferProjection.DatasetId,
                consumedPartition.DatasetId,
                StringComparison.Ordinal) ||
            !string.Equals(
                transferProjection.TrainRecordSetSha256,
                consumedPartition.TrainRecordSetSha256,
                StringComparison.Ordinal) ||
            !string.Equals(
                transferProjection.HeldOutRecordSetSha256,
                consumedPartition.HeldOutRecordSetSha256,
                StringComparison.Ordinal) ||
            !string.Equals(
                transferProjection.TrainSourceSetSha256,
                consumedPartition.TrainSourceSetSha256,
                StringComparison.Ordinal) ||
            !string.Equals(
                transferProjection.HeldOutSourceSetSha256,
                consumedPartition.HeldOutSourceSetSha256,
                StringComparison.Ordinal) ||
            !transferProjection.TrainRecords
                .Select(DesignConversionDpoDatasetBuilder
                    .ComputeRecordIdentityHash)
                .SequenceEqual(
                    consumedPartition.TrainRecords.Select(
                        DesignConversionDpoDatasetBuilder
                            .ComputeRecordIdentityHash),
                    StringComparer.Ordinal) ||
            !transferProjection.TrainRecords
                .Select(DesignConversionDpoDatasetBuilder
                    .ComputeSourceIdentityHash)
                .SequenceEqual(
                    consumedPartition.TrainRecords.Select(
                        DesignConversionDpoDatasetBuilder
                            .ComputeSourceIdentityHash),
                    StringComparer.Ordinal))
        {
            return EvaluatedPackingInvalid<
                DesignConversionDpoConsumedPartitionProjection>();
        }

        return OperationResult<
            DesignConversionDpoConsumedPartitionProjection>.Ok(
            transferProjection);
    }

    internal static Task<OperationResult<DesignConversionDpoEvaluatedBridgeLease>>
        ExecutePreparedEvaluatedCompositeWithTransferredLeaseAsync(
            ServerOwnedLegalTrainingTeacherAdmissionLease trainingLease,
            DesignConversionPipelineTrainingBinding trainingBinding,
            PreparedEvaluatedCompositeExecution prepared,
            string outputDirectory,
            string runId,
            string runtimeCheckpointPath,
            Action<DesignConversionDpoEvaluatedForwardObservation>? forwardObserverForTesting = null,
            Action<DesignConversionDpoEvaluatedOwnershipObservation>? ownershipObserverForTesting = null,
            CancellationToken ct = default)
    {
        if (trainingBinding is null || prepared is null ||
            !trainingBinding.MatchesExactLease(trainingLease) ||
            !trainingBinding.MatchesExactPreparedBridge(
                prepared.FinalOptimizer) ||
            !ReferenceEquals(
                prepared.Optimizer.TrainPacked,
                prepared.FinalOptimizer.Inputs))
        {
            return Task.FromResult(
                OperationResult<
                    DesignConversionDpoEvaluatedBridgeLease>.Fail(
                    "The exact composite-bound prepared bridge child is required.",
                    LegalTrainingTeacherAdmissionAuthority
                        .TrainingContentInvalidErrorCode));
        }

        OperationResult<PreparedEvaluatedCompositeExecution> captured;
        // CaptureExactPreparedCompositeExecution — это immutable snapshot и
        // повторная проверка уже подготовленного content binding. Здесь нельзя
        // брать server-owned mutation permit: provider actual-state fingerprint
        // ещё не сверен, значит mutable authority ещё не доказана.
        captured = CaptureExactPreparedCompositeExecution(prepared);
        if (!captured.IsSuccess || captured.Value is null)
        {
            return Task.FromResult(
                captured.MapError<
                    DesignConversionDpoEvaluatedBridgeLease>());
        }

        return ExecuteEvaluatedSnapshotCoreAsync(
            trainingLease,
            captured.Value.ConsumedPartition,
            outputDirectory,
            runId,
            runtimeCheckpointPath,
            captured.Value.EffectiveOptions,
            captured.Value.ContinuationRequest,
            captured.Value.UseContinuation,
            forwardObserverForTesting,
            ownershipObserverForTesting,
            captured.Value.Optimizer,
            captured.Value.HeldOutLegalAdmission,
            trainingBinding,
            ct);
    }

    internal static Task<OperationResult<DesignConversionDpoEvaluatedBridgeLease>>
        ExecutePreparedEvaluatedCompositeWithTransferredLeaseAsync(
            ServerOwnedLegalTrainingTeacherAdmissionLease trainingLease,
            PreparedEvaluatedCompositeExecution prepared,
            string outputDirectory,
            string runId,
            string runtimeCheckpointPath,
            Action<DesignConversionDpoEvaluatedForwardObservation>? forwardObserverForTesting = null,
            Action<DesignConversionDpoEvaluatedOwnershipObservation>? ownershipObserverForTesting = null,
            CancellationToken ct = default)
        => ExecuteEvaluatedSnapshotCoreAsync(
            trainingLease,
            prepared.ConsumedPartition,
            outputDirectory,
            runId,
            runtimeCheckpointPath,
            prepared.EffectiveOptions,
            prepared.ContinuationRequest,
            prepared.UseContinuation,
            forwardObserverForTesting,
            ownershipObserverForTesting,
            prepared.Optimizer,
            prepared.HeldOutLegalAdmission,
            trainingBinding: null,
            ct);

    [Obsolete("Use the exact prepared composite execution overload.")]
    internal static Task<OperationResult<DesignConversionDpoEvaluatedBridgeLease>>
        ExecuteEvaluatedAndConvertWithTransferredLeaseAsync(
            ServerOwnedLegalTrainingTeacherAdmissionLease trainingLease,
            DesignConversionDpoResolvedPartition partition,
            string outputDirectory,
            string runId,
            string runtimeCheckpointPath,
            IDesignConversionDpoPolicyBaseProvider? policyBaseProvider,
            DesignConversionDpoPolicyCheckpointBridgeOptions? options,
            Action<DesignConversionDpoEvaluatedForwardObservation>? forwardObserverForTesting = null,
            Action<DesignConversionDpoEvaluatedOwnershipObservation>? ownershipObserverForTesting = null,
            CancellationToken ct = default)
        => RejectUnpreparedTransferredLease(trainingLease);

    [Obsolete("Use the exact prepared composite execution overload.")]
    internal static Task<OperationResult<DesignConversionDpoEvaluatedBridgeLease>>
        ExecuteEvaluatedAndConvertWithTransferredLeaseAsync(
            ServerOwnedLegalTrainingTeacherAdmissionLease trainingLease,
            DesignConversionDpoResolvedPartition partition,
            string outputDirectory,
            string runId,
            string runtimeCheckpointPath,
            IDesignConversionDpoPolicyBaseProvider? policyBaseProvider,
            DesignConversionDpoPolicyCheckpointBridgeOptions? options,
            DesignConversionDpoPolicyContinuationRequest continuationRequest,
            Action<DesignConversionDpoEvaluatedForwardObservation>? forwardObserverForTesting = null,
            Action<DesignConversionDpoEvaluatedOwnershipObservation>? ownershipObserverForTesting = null,
            CancellationToken ct = default)
        => RejectUnpreparedTransferredLease(trainingLease);

    private static async Task<OperationResult<DesignConversionDpoEvaluatedBridgeLease>>
        ExecuteEvaluatedCoreAsync(
            ServerOwnedLegalTrainingTeacherAdmissionAuthority?
                legalTeacherAdmissionAuthority,
            DesignConversionDpoResolvedPartition? partition,
            string outputDirectory,
            string runId,
            string runtimeCheckpointPath,
            IDesignConversionDpoPolicyBaseProvider? policyBaseProvider,
            DesignConversionDpoPolicyCheckpointBridgeOptions? options,
            DesignConversionDpoPolicyContinuationRequest? continuationRequest,
            bool useContinuation,
            Action<DesignConversionDpoEvaluatedForwardObservation>? forwardObserverForTesting,
            Action<DesignConversionDpoEvaluatedOwnershipObservation>? ownershipObserverForTesting,
            CancellationToken ct,
            Action<TrainingTeacherProvenance?>?
                legalTeacherEnumerationObserverForTesting)
    {
        if (ct.IsCancellationRequested)
            return Cancelled<DesignConversionDpoEvaluatedBridgeLease>();

        if (legalTeacherAdmissionAuthority is null ||
            !legalTeacherAdmissionAuthority.IsAvailable)
        {
            return OperationResult<DesignConversionDpoEvaluatedBridgeLease>.Fail(
                LegalTeacherConfigurationRequiredErrorMessage,
                LegalTeacherConfigurationRequiredErrorCode);
        }

        var validatedOptions = ValidateOptions(options);
        if (!validatedOptions.IsSuccess)
            return validatedOptions.MapError<DesignConversionDpoEvaluatedBridgeLease>();
        var effectiveOptions = validatedOptions.Value!;
        if (partition is null || string.IsNullOrWhiteSpace(partition.DatasetId) ||
            partition.TrainRecords is null || partition.TrainRecords.Count == 0 ||
            partition.HeldOutRecords is null || partition.HeldOutRecords.Count == 0 ||
            string.IsNullOrWhiteSpace(outputDirectory) ||
            string.IsNullOrWhiteSpace(runtimeCheckpointPath))
        {
            return EvaluatedPackingInvalid<DesignConversionDpoEvaluatedBridgeLease>();
        }
        if (!DesignConversionDpoConsumedPartitionProjectionContract.TryCreate(
                partition,
                effectiveOptions.MaxBridgePreferenceCount,
                out var consumedPartition) ||
            consumedPartition is null)
        {
            return EvaluatedPackingInvalid<DesignConversionDpoEvaluatedBridgeLease>();
        }
        // Этот guard стоит перед lease/file authority, чтобы запрещенная строка
        // не могла дойти до trainer mutation surface через evaluated path.
        var trainability = ValidateEvaluatedConsumedPartitionTrainability(
            consumedPartition);
        if (!trainability.IsSuccess)
            return trainability.MapError<DesignConversionDpoEvaluatedBridgeLease>();
        var selectedTrainRows = consumedPartition.TrainRecords;
        var heldOutRows = consumedPartition.HeldOutRecords;

        var combinedPreflight = ValidateCombinedTeacherProvenancePreflight(
            legalTeacherAdmissionAuthority,
            consumedPartition,
            legalTeacherEnumerationObserverForTesting,
            ct);
        if (!combinedPreflight.IsSuccess)
            return combinedPreflight.MapError<DesignConversionDpoEvaluatedBridgeLease>();

        DesignConversionDpoPolicyContinuationRequest? continuationSnapshot;
        try
        {
            continuationSnapshot = useContinuation
                ? SnapshotContinuationRequest(continuationRequest)
                : null;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            continuationSnapshot = null;
        }

        if (ct.IsCancellationRequested)
            return Cancelled<DesignConversionDpoEvaluatedBridgeLease>();

        var providerPreparation = PreparePolicyBaseDescriptor(
            policyBaseProvider,
            continuationSnapshot,
            useContinuation,
            requireEvaluatedContinuationParent: true);
        if (!providerPreparation.IsSuccess ||
            providerPreparation.Value is null)
        {
            return providerPreparation.MapError<
                DesignConversionDpoEvaluatedBridgeLease>();
        }
        var proofPreparation =
            await AttachBridgeOwnedContinuationMutationProofAsync(
                providerPreparation.Value,
                continuationSnapshot,
                useContinuation,
                ct);
        if (!proofPreparation.IsSuccess || proofPreparation.Value is null)
        {
            return proofPreparation.MapError<
                DesignConversionDpoEvaluatedBridgeLease>();
        }

        var config = proofPreparation.Value.Config;
        if (config.ContextLen < 2 || config.VocabSize < 256)
            return EvaluatedPackingInvalid<
                DesignConversionDpoEvaluatedBridgeLease>();
        var descriptor = DesignConversionDpoSourceEncoderContractBuilder
            .BuildEvaluatedDescriptor(selectedTrainRows, config);
        if (!descriptor.IsSuccess || descriptor.Value is null)
            return descriptor.MapError<
                DesignConversionDpoEvaluatedBridgeLease>();
        DesignConversionDpoRuntimeEncodingBinding? runtimeEncodingBinding =
            null;
        if (useContinuation)
        {
            var runtimeEncoding =
                ValidateContinuationRuntimeEncodingForPacking(
                    continuationSnapshot!.RuntimeEncodingBinding,
                    descriptor.Value,
                    config);
            if (!runtimeEncoding.IsSuccess || runtimeEncoding.Value is null)
            {
                return runtimeEncoding.MapError<
                    DesignConversionDpoEvaluatedBridgeLease>();
            }
            runtimeEncodingBinding = runtimeEncoding.Value;
        }
        var trainPackedResult = BuildEvaluatedPackedBatches(
            selectedTrainRows,
            config,
            includeTrainingVectorAndWeight: true,
            runtimeEncodingBinding);
        var heldOutPackedResult = BuildEvaluatedPackedBatches(
            heldOutRows,
            config,
            includeTrainingVectorAndWeight: false,
            runtimeEncodingBinding);
        if (!trainPackedResult.IsSuccess ||
            trainPackedResult.Value is null)
        {
            return trainPackedResult.MapError<
                DesignConversionDpoEvaluatedBridgeLease>();
        }
        if (!heldOutPackedResult.IsSuccess ||
            heldOutPackedResult.Value is null)
        {
            return heldOutPackedResult.MapError<
                DesignConversionDpoEvaluatedBridgeLease>();
        }
        var trainPacked = trainPackedResult.Value;
        var heldOutPacked = heldOutPackedResult.Value;
        var trainAdmissionValidation =
            ValidateEvaluatedTrainPackedAdmissions(trainPacked);
        if (!trainAdmissionValidation.IsSuccess)
        {
            return trainAdmissionValidation.MapError<
                DesignConversionDpoEvaluatedBridgeLease>();
        }
        var heldOutAdmissionValidation =
            ValidateEvaluatedHeldOutAdmissions(heldOutPacked);
        if (!heldOutAdmissionValidation.IsSuccess)
        {
            return heldOutAdmissionValidation.MapError<
                DesignConversionDpoEvaluatedBridgeLease>();
        }
        var trainInputSha256 = RecomputeEvaluatedInputFingerprint(
            selectedTrainRows,
            trainPacked.Select(static value => value.VectorBatch).ToArray(),
            config,
            includeTrainingVectorAndWeight: true);
        var heldOutInputSha256 = RecomputeEvaluatedInputFingerprint(
            heldOutRows,
            heldOutPacked.Select(static value => value.VectorBatch).ToArray(),
            config,
            includeTrainingVectorAndWeight: false);
        if (string.IsNullOrEmpty(trainInputSha256) ||
            string.IsNullOrEmpty(heldOutInputSha256))
        {
            return OperationResult<
                DesignConversionDpoEvaluatedBridgeLease>.Fail(
                "DesignConversion DPO evaluated input fingerprint is invalid.",
                EvaluatedInputFingerprintInvalidErrorCode);
        }
        var finalOptimizer = DesignConversionDpoTrainingContentAuthority
            .ProjectFinalOptimizerInputs(
                trainPacked,
                trainPacked
                    .Select(static input => input.Provenance)
                    .ToArray(),
                static input => input.Prompt,
                static input => input.Chosen,
                static input => input.Rejected,
                static input => input.VectorBatch.TeachingVector,
                static input => input.VectorBatch.Weight,
                DesignConversionDpoTrainingContentAuthority
                    .BuildEvaluatedBridgeFinalOptimizerContext);
        if (!finalOptimizer.IsSuccess || finalOptimizer.Value is null)
            return finalOptimizer.MapError<
                DesignConversionDpoEvaluatedBridgeLease>();

        var heldOutLegalAdmission = ValidateExactHeldOutEvaluationBatch(
            legalTeacherAdmissionAuthority,
            heldOutPacked);
        if (!heldOutLegalAdmission.IsSuccess || heldOutLegalAdmission.Value is null)
        {
            return heldOutLegalAdmission.MapError<
                DesignConversionDpoEvaluatedBridgeLease>();
        }

        LeaseAcquisitionObserver.Value?.Invoke("dpo_bridge:evaluated:lease_acquisition");
        var leaseResult = legalTeacherAdmissionAuthority.AcquireTrainingLease(
            finalOptimizer.Value.Content,
            finalOptimizer.Value.Provenance);
        if (!leaseResult.IsSuccess || leaseResult.Value is null)
            return leaseResult.MapError<DesignConversionDpoEvaluatedBridgeLease>();

        return await ExecuteEvaluatedSnapshotCoreAsync(
            leaseResult.Value,
            consumedPartition,
            outputDirectory,
            runId,
            runtimeCheckpointPath,
            effectiveOptions,
            continuationSnapshot,
            useContinuation,
            forwardObserverForTesting,
            ownershipObserverForTesting,
            new PreparedEvaluatedOptimizer(
                proofPreparation.Value,
                descriptor.Value,
                runtimeEncodingBinding,
                finalOptimizer.Value.Inputs,
                heldOutPacked,
                trainInputSha256,
                heldOutInputSha256),
            heldOutLegalAdmission.Value,
            trainingBinding: null,
            ct);
    }

    [Obsolete("Use the exact prepared composite execution overload.")]
    internal static Task<OperationResult<DesignConversionDpoEvaluatedBridgeLease>>
        ExecuteEvaluatedSnapshotWithTransferredLeaseAsync(
            ServerOwnedLegalTrainingTeacherAdmissionLease trainingLease,
            DesignConversionDpoConsumedPartitionProjection consumedPartition,
            string outputDirectory,
            string runId,
            string runtimeCheckpointPath,
            DesignConversionDpoPolicyCheckpointBridgeOptions effectiveOptions,
            Action<DesignConversionDpoEvaluatedForwardObservation>? forwardObserverForTesting = null,
            Action<DesignConversionDpoEvaluatedOwnershipObservation>? ownershipObserverForTesting = null,
            CancellationToken ct = default)
        => RejectUnpreparedTransferredLease(trainingLease);

    [Obsolete("Use the exact prepared composite execution overload.")]
    internal static Task<OperationResult<DesignConversionDpoEvaluatedBridgeLease>>
        ExecuteEvaluatedContinuationSnapshotWithTransferredLeaseAsync(
            ServerOwnedLegalTrainingTeacherAdmissionLease trainingLease,
            DesignConversionDpoConsumedPartitionProjection consumedPartition,
            string outputDirectory,
            string runId,
            string runtimeCheckpointPath,
            DesignConversionDpoPolicyCheckpointBridgeOptions effectiveOptions,
            DesignConversionDpoPolicyContinuationRequest continuationRequest,
            Action<DesignConversionDpoEvaluatedForwardObservation>? forwardObserverForTesting = null,
            Action<DesignConversionDpoEvaluatedOwnershipObservation>? ownershipObserverForTesting = null,
            CancellationToken ct = default)
        => RejectUnpreparedTransferredLease(trainingLease);

    private static Task<OperationResult<DesignConversionDpoEvaluatedBridgeLease>>
        RejectUnpreparedTransferredLease(
            ServerOwnedLegalTrainingTeacherAdmissionLease trainingLease)
    {
        trainingLease.Dispose();
        return Task.FromResult(
            OperationResult<DesignConversionDpoEvaluatedBridgeLease>.Fail(
                "Exact prepared DesignConversion optimizer content is required before transferring a training lease.",
                BoundedDpoTrainingContentProjector.InvalidErrorCode));
    }

    private static OperationResult<bool>
        ValidateCombinedTeacherProvenancePreflight(
            ServerOwnedLegalTrainingTeacherAdmissionAuthority authority,
            DesignConversionDpoConsumedPartitionProjection consumedPartition,
            Action<TrainingTeacherProvenance?>? enumerationObserver,
            CancellationToken ct)
    {
        var unique = new Dictionary<string, TrainingTeacherProvenance>(
            StringComparer.Ordinal);
        try
        {
            foreach (var row in consumedPartition.TrainRecords
                         .Concat(consumedPartition.HeldOutRecords))
            {
                if (ct.IsCancellationRequested)
                    return Cancelled<bool>();

                var provenance = row.TeacherProvenance;
                enumerationObserver?.Invoke(provenance);
                if (ct.IsCancellationRequested)
                    return Cancelled<bool>();
                if (provenance is null ||
                    string.IsNullOrWhiteSpace(provenance.RequestId))
                {
                    return OperationResult<bool>.Fail(
                        "DesignConversion DPO teacher provenance is invalid.",
                        LegalTrainingTeacherAdmissionAuthority
                            .ProvenanceInvalidErrorCode);
                }

                if (unique.TryGetValue(provenance.RequestId, out var existing))
                {
                    if (!Equals(existing, provenance))
                    {
                        return OperationResult<bool>.Fail(
                            "Conflicting DesignConversion DPO teacher provenance reused one request id.",
                            LegalTrainingTeacherAdmissionAuthority
                                .ProvenanceInvalidErrorCode);
                    }
                }
                else
                {
                    unique.Add(provenance.RequestId, provenance with { });
                }
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Cancelled<bool>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<bool>.Fail(
                "DesignConversion DPO teacher provenance could not be enumerated.",
                LegalTrainingTeacherAdmissionAuthority
                    .EnumerationFailedErrorCode);
        }

        if (ct.IsCancellationRequested)
            return Cancelled<bool>();
        var admission = authority.ValidateBatch(unique.Values.ToArray());
        if (!admission.IsSuccess || admission.Value is null)
            return admission.MapError<bool>();
        return ct.IsCancellationRequested
            ? Cancelled<bool>()
            : OperationResult<bool>.Ok(true);
    }

    // Предполётная правовая проверка учителей для закрытого исполнителя с
    // правовой властью. Берёт только примитив правовой власти и не требует ни
    // собранных партий, ни содержания обучения, ни учебной аренды.
    private static OperationResult<bool>
        ValidateStandaloneLegalTeacherPreflight(
            ServerOwnedLegalTrainingTeacherAdmissionAuthority authority,
            IReadOnlyList<DesignConversionDpoTrainingRecord> trainableRows,
            CancellationToken ct)
    {
        // Различные паспорта учителя собираем по опознавателю запроса, чтобы
        // один и тот же паспорт не судился правовой властью дважды.
        var unique = new Dictionary<string, TrainingTeacherProvenance>(
            StringComparer.Ordinal);
        // Перебираем строки обучения в их собственном порядке.
        for (var index = 0; index < trainableRows.Count; index++)
        {
            // Отмену уважаем на каждой строке: правовая проверка не
            // продолжается после запроса на отмену.
            if (ct.IsCancellationRequested)
                return Cancelled<bool>();
            // Паспорт учителя берём ровно у этой строки обучения.
            var provenance = trainableRows[index].TeacherProvenance;
            // Строго детерминированный путь без внешнего учителя законен и
            // обязан проходить как сейчас, поэтому в правовую партию не идёт.
            if (IsExactDeterministicNoExternalTeacher(provenance))
                continue;
            // Паспорт без опознавателя запроса этот предполёт не судит: его
            // отказ остаётся за прежними проверками ниже по пути.
            if (provenance is null ||
                string.IsNullOrWhiteSpace(provenance.RequestId))
                continue;
            // Повтор того же опознавателя запроса пропускаем: расхождение
            // паспортов между собой тоже судит не этот предполёт.
            if (!unique.ContainsKey(provenance.RequestId))
                unique.Add(provenance.RequestId, provenance);
        }

        // Пустая партия означает, что внешних учителей здесь нет вовсе, и
        // беспокоить правовую власть нечем.
        if (unique.Count == 0)
            return OperationResult<bool>.Ok(true);
        // Тот же примитив правовой власти, что и в соседней предполётной
        // проверке совмещённых паспортов: он и выдаёт правовые коды отказа.
        var admission = authority.ValidateBatch(unique.Values.ToArray());
        // Правовой отказ отдаём как есть, чтобы его код дошёл до вызывающего
        // без подмены.
        if (!admission.IsSuccess || admission.Value is null)
            return admission.MapError<bool>();
        // Все внешние учителя законны: предполёт пропускает путь дальше.
        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<LegalTrainingTeacherAdmission>
        ValidateExactHeldOutEvaluationBatch(
            ServerOwnedLegalTrainingTeacherAdmissionAuthority authority,
            IReadOnlyList<DesignConversionDpoEvaluatedPackedBatch> heldOutPacked)
    {
        var projected = DesignConversionDpoTrainingContentAuthority
            .ProjectFinalOptimizerInputs(
                heldOutPacked,
                heldOutPacked
                    .Select(static input => input.Provenance)
                    .ToArray(),
                static input => input.Prompt,
                static input => input.Chosen,
                static input => input.Rejected,
                static _ => (IReadOnlyList<float>?)null,
                static _ => (float?)null,
                static _ => "design-conversion-evaluated-heldout-v1");
        if (!projected.IsSuccess || projected.Value is null)
        {
            return projected.MapError<LegalTrainingTeacherAdmission>();
        }

        return authority.ValidateEvaluationBatch(
            projected.Value.Content,
            projected.Value.Provenance);
    }

    private static async Task<OperationResult<DesignConversionDpoEvaluatedBridgeLease>>
        ExecuteEvaluatedSnapshotCoreAsync(
            ServerOwnedLegalTrainingTeacherAdmissionLease trainingLease,
            DesignConversionDpoConsumedPartitionProjection consumedPartition,
            string outputDirectory,
            string runId,
            string runtimeCheckpointPath,
            DesignConversionDpoPolicyCheckpointBridgeOptions effectiveOptions,
            DesignConversionDpoPolicyContinuationRequest? continuationRequest,
            bool useContinuation,
            Action<DesignConversionDpoEvaluatedForwardObservation>? forwardObserverForTesting,
            Action<DesignConversionDpoEvaluatedOwnershipObservation>? ownershipObserverForTesting,
            PreparedEvaluatedOptimizer prepared,
            LegalTrainingTeacherAdmission heldOutLegalAdmission,
            DesignConversionPipelineTrainingBinding? trainingBinding,
            CancellationToken ct)
    {
        using var trainingLeaseTransfer = new TrainingLeaseTransferGuard(trainingLease);
        var selectedTrainRows = consumedPartition.TrainRecords;
        var heldOutRows = consumedPartition.HeldOutRecords;
        var consumedSourceRecordCount = consumedPartition.SourceRecordCount;
        var legalTeacherAdmission = trainingLease.Admission;
        var legalTeacherAdmissionCore =
            DesignConversionDpoLegalTeacherAdmissionCommitmentCodec.CaptureAdmissions(
                legalTeacherAdmission,
                heldOutLegalAdmission);
        if (!legalTeacherAdmissionCore.IsSuccess || legalTeacherAdmissionCore.Value is null)
            return legalTeacherAdmissionCore.MapError<DesignConversionDpoEvaluatedBridgeLease>();
        if (ct.IsCancellationRequested)
            return Cancelled<DesignConversionDpoEvaluatedBridgeLease>();

        var trainability = ValidateEvaluatedConsumedPartitionTrainability(
            consumedPartition);
        if (!trainability.IsSuccess)
            return trainability.MapError<DesignConversionDpoEvaluatedBridgeLease>();
        var trainAdmissionValidation =
            ValidateEvaluatedTrainPackedAdmissions(prepared.TrainPacked);
        if (!trainAdmissionValidation.IsSuccess)
        {
            return trainAdmissionValidation.MapError<
                DesignConversionDpoEvaluatedBridgeLease>();
        }
        var heldOutAdmissionValidation =
            ValidateEvaluatedHeldOutAdmissions(prepared.HeldOutPacked);
        if (!heldOutAdmissionValidation.IsSuccess)
        {
            return heldOutAdmissionValidation.MapError<
                DesignConversionDpoEvaluatedBridgeLease>();
        }

        string outputFullPath;
        string finalRuntimeFullPath;
        try
        {
            outputFullPath = Path.GetFullPath(outputDirectory);
            finalRuntimeFullPath = Path.GetFullPath(runtimeCheckpointPath);
            if (!string.Equals(
                    Path.TrimEndingDirectorySeparator(
                        Path.GetDirectoryName(finalRuntimeFullPath) ?? string.Empty),
                    Path.TrimEndingDirectorySeparator(outputFullPath),
                    OperatingSystem.IsWindows()
                        ? StringComparison.OrdinalIgnoreCase
                        : StringComparison.Ordinal))
            {
                return EvaluatedPackingInvalid<DesignConversionDpoEvaluatedBridgeLease>();
            }
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return EvaluatedPackingInvalid<DesignConversionDpoEvaluatedBridgeLease>();
        }

        var preparation = prepared.ProviderPreparation;
        var outputDirectoryExistedBefore = Directory.Exists(outputFullPath);
        var retainEvaluatedArtifacts = false;

        OperationResult<DesignConversionDpoPolicyBaseModelSet>? provided;
        try
        {
            // Provider-load здесь ещё не mutation authority: это только
            // недоверенный кандидат. Первый server-owned permit разрешён ниже,
            // после bridge-owned fingerprint сравнения фактических весов.
            provided = useContinuation
                ? await ((IDesignConversionDpoPolicyContinuationProvider)preparation.Provider)
                    .CreateContinuedModelSetAsync(
                        preparation.Descriptor,
                        continuationRequest!,
                        ct)
                : await preparation.Provider.CreateModelSetAsync(
                    preparation.Descriptor,
                    ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Cancelled<DesignConversionDpoEvaluatedBridgeLease>();
        }
        catch (Exception exception)
            when (exception is not OperationCanceledException &&
                  SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return useContinuation
                ? ContinuationCheckpointMismatch<DesignConversionDpoEvaluatedBridgeLease>()
                : OperationResult<DesignConversionDpoEvaluatedBridgeLease>.Fail(
                    "DesignConversion DPO policy base provider failed.",
                    "DESIGN_CONVERSION_DPO_POLICY_BASE_PROVIDER_FAILED");
        }
        ValidatedPolicyBase policyBase;
        if (ct.IsCancellationRequested ||
            provided is not null &&
            !provided.IsSuccess &&
            string.Equals(provided.ErrorCode, "CANCELLED", StringComparison.Ordinal))
        {
            return Cancelled<DesignConversionDpoEvaluatedBridgeLease>();
        }
        if (useContinuation && (provided is null || !provided.IsSuccess))
        {
            return provided is not null && IsStableContinuationError(provided.ErrorCode)
                ? StableContinuationFailure<DesignConversionDpoEvaluatedBridgeLease>(
                    provided.ErrorCode)
                : ContinuationCheckpointMismatch<
                    DesignConversionDpoEvaluatedBridgeLease>();
        }
        if (provided is null || !provided.IsSuccess)
        {
            return OperationResult<DesignConversionDpoEvaluatedBridgeLease>.Fail(
                "DesignConversion DPO policy base provider failed.",
                "DESIGN_CONVERSION_DPO_POLICY_BASE_PROVIDER_FAILED");
        }
        var validated = ValidatePolicyBaseAgainstPreparation(
            provided.Value,
            preparation);
        if (!validated.IsSuccess || validated.Value is null)
            return validated.MapError<DesignConversionDpoEvaluatedBridgeLease>();

        var providerResultPermit = AcquireEvaluatedBridgeMutationPermit(
            trainingLease,
            trainingBinding,
            "dpo_bridge:evaluated:policy_provider_result");
        if (!providerResultPermit.IsSuccess || providerResultPermit.Value is null)
        {
            return providerResultPermit.MapError<
                DesignConversionDpoEvaluatedBridgeLease>();
        }
        using (providerResultPermit.Value)
            policyBase = validated.Value;

        string? scratchDirectory = null;
        DesignConversionHandleBoundDirectory? retainedScratchDirectory = null;
        DesignConversionHandleBoundDirectory? retainedFinalDirectory = null;
        var ownedScratchFiles = new List<DesignConversionDpoRetainedOwnedFile>();
        var acceptedTrainerOwnership = new Dictionary<
            DpoTrainerArtifactIdentity,
            AcceptedTrainerArtifactOwnership>();
        var createdFinals = new List<DesignConversionDpoRetainedOwnedFile>();
        var scratchCleanupProven = false;
        try
        {
            ct.ThrowIfCancellationRequested();
            var config = policyBase.ModelSet.Policy.Config;
            if (config.ContextLen < 2 || config.VocabSize < 256)
                return EvaluatedPackingInvalid<DesignConversionDpoEvaluatedBridgeLease>();
            var descriptor = prepared.Descriptor;
            var runtimeEncodingBinding = prepared.RuntimeEncodingBinding;
            var trainPacked = prepared.TrainPacked;
            var heldOutPacked = prepared.HeldOutPacked;
            var selectedTrainInputSha256 = prepared.TrainInputSha256;
            var heldOutEvaluationInputSha256 = prepared.HeldOutInputSha256;
            if (string.IsNullOrEmpty(selectedTrainInputSha256) ||
                string.IsNullOrEmpty(heldOutEvaluationInputSha256))
            {
                return OperationResult<DesignConversionDpoEvaluatedBridgeLease>.Fail(
                    "DesignConversion DPO evaluated input fingerprint is invalid.",
                    EvaluatedInputFingerprintInvalidErrorCode);
            }

            var namespacePermit = AcquireEvaluatedBridgeMutationPermit(
                trainingLease,
                trainingBinding,
                "dpo_bridge:evaluated:artifact_namespace");
            if (!namespacePermit.IsSuccess || namespacePermit.Value is null)
                return namespacePermit.MapError<DesignConversionDpoEvaluatedBridgeLease>();
            using (namespacePermit.Value)
            {
                Directory.CreateDirectory(outputFullPath);
                scratchDirectory = Path.Combine(
                    outputFullPath,
                    BuildEvaluatedScratchDirectoryName(runId));
                Directory.CreateDirectory(scratchDirectory);
                retainedScratchDirectory = DesignConversionHandleBoundDirectory.AcquireOrCreate(
                    scratchDirectory,
                    beforeIo: null);
                var scratchOwnershipPayload = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString("N"));
                ownedScratchFiles.Add(WriteExclusiveOwned(
                    retainedScratchDirectory,
                    scratchDirectory,
                    ".evaluated-ownership",
                    scratchOwnershipPayload));
            }

            var policy = policyBase.ModelSet.Policy;
            var reference = policyBase.ModelSet.Reference;
            var freshPolicy = policyBase.ModelSet.FreshLoadTarget;
            var loraPermit = AcquireEvaluatedBridgeMutationPermit(
                trainingLease,
                trainingBinding,
                "dpo_bridge:evaluated:lora_attach");
            if (!loraPermit.IsSuccess || loraPermit.Value is null)
                return loraPermit.MapError<DesignConversionDpoEvaluatedBridgeLease>();
            OperationResult<PreparedLoraPolicyBase> loraPreparation;
            DpoTrainer trainer;
            using (loraPermit.Value)
            {
                loraPreparation = PrepareLoraTrainingTopology(
                    policy,
                    reference,
                    freshPolicy,
                    policyBase.ModelSet.RandomContext!,
                    policyBase.ModelSet.Identity.BaseCheckpointSha256,
                    useContinuation);
                if (!loraPreparation.IsSuccess)
                    return loraPreparation.MapError<DesignConversionDpoEvaluatedBridgeLease>();
                var trainerResult = CreateDpoTrainer(
                    trainingLease,
                    policy,
                    reference,
                    scratchDirectory,
                    proof => AcceptTrainerArtifactOwnership(
                        proof,
                        scratchDirectory,
                        effectiveOptions.DpoStepCount,
                        acceptedTrainerOwnership,
                        ownedScratchFiles,
                        retainedScratchDirectory,
                        ownershipObserverForTesting));
                if (!trainerResult.IsSuccess || trainerResult.Value is null)
                    return trainerResult.MapError<DesignConversionDpoEvaluatedBridgeLease>();
                trainer = trainerResult.Value;
            }
            var cachedTrainBatches = trainPacked.Select(static packed => packed.VectorBatch).ToArray();
            var actualDpoStepCountResult = ExecuteDpoStepLoop(
                trainer,
                cachedTrainBatches,
                effectiveOptions,
                step =>
                {
                    PublishEvaluatedMutationSinkObservation(trainPacked);
                    ownershipObserverForTesting?.Invoke(
                        new DesignConversionDpoEvaluatedOwnershipObservation(
                            DesignConversionDpoEvaluatedOwnershipPhase.BeforeTrainerStepAdoption,
                            scratchDirectory,
                            null,
                            step));
                    ownershipObserverForTesting?.Invoke(
                        new DesignConversionDpoEvaluatedOwnershipObservation(
                            DesignConversionDpoEvaluatedOwnershipPhase.AfterTrainerStepRetained,
                            scratchDirectory,
                            null,
                            step));
                },
                boundary => AcquireEvaluatedBridgeMutationPermit(
                    trainingLease,
                    trainingBinding,
                    boundary),
                "dpo_bridge:evaluated:dpo_step",
                ct);
            if (!actualDpoStepCountResult.IsSuccess)
                return actualDpoStepCountResult.MapError<DesignConversionDpoEvaluatedBridgeLease>();
            var actualDpoStepCount = actualDpoStepCountResult.Value;
            ct.ThrowIfCancellationRequested();
            if (actualDpoStepCount <= 0)
                return OperationResult<DesignConversionDpoEvaluatedBridgeLease>.Fail(
                    "DesignConversion DPO evaluated bridge did not execute any optimizer step.",
                    "DESIGN_CONVERSION_DPO_EVALUATED_STEP_NOT_EXECUTED");
            if (!ReferenceMatchesSnapshot(reference, loraPreparation.Value!.ReferenceSnapshot))
                return OperationResult<DesignConversionDpoEvaluatedBridgeLease>.Fail(
                    "DesignConversion DPO policy reference changed during training.",
                    "DESIGN_CONVERSION_DPO_POLICY_REFERENCE_MUTATED");

            var scratchPolicyPath = Path.Combine(
                scratchDirectory,
                DpoTrainer.BuildPolicyCheckpointFileName(actualDpoStepCount));
            var provenScratchPolicy = ownedScratchFiles.LastOrDefault(artifact =>
                string.Equals(
                    artifact.LeafName,
                    Path.GetFileName(scratchPolicyPath),
                    StringComparison.Ordinal));
            if (provenScratchPolicy is null)
                throw new IOException("DpoTrainer final policy ownership was not proven.");
            var scratchPolicy = ReadBoundedOwnedFile(
                retainedScratchDirectory,
                Path.GetFileName(scratchPolicyPath),
                256L * 1024 * 1024,
                ct,
                provenScratchPolicy.Identity);
            var policyCheckpointPayload = scratchPolicy.Payload;
            var policyCheckpointSha256 = scratchPolicy.OwnedFile.Sha256;
            ownershipObserverForTesting?.Invoke(
                new DesignConversionDpoEvaluatedOwnershipObservation(
                    DesignConversionDpoEvaluatedOwnershipPhase.AfterScratchPolicyRetained,
                    scratchDirectory,
                    scratchPolicyPath));
            var freshLoadPermit = AcquireEvaluatedBridgeMutationPermit(
                trainingLease,
                trainingBinding,
                "dpo_bridge:evaluated:fresh_load");
            if (!freshLoadPermit.IsSuccess || freshLoadPermit.Value is null)
                return freshLoadPermit.MapError<DesignConversionDpoEvaluatedBridgeLease>();
            using (freshLoadPermit.Value)
            using (var checkpointStream = new MemoryStream(policyCheckpointPayload, writable: false))
                freshPolicy.LoadCheckpoint(checkpointStream);
            if (!ModelsHaveEqualParameters(policy, freshPolicy))
                return OperationResult<DesignConversionDpoEvaluatedBridgeLease>.Fail(
                    "DesignConversion DPO policy checkpoint fresh-load verification failed.",
                    "DESIGN_CONVERSION_DPO_POLICY_FRESH_LOAD_VERIFICATION_FAILED");

            var heldOutScores = new (float ChildChosen, float ChildRejected,
                float ReferenceChosen, float ReferenceRejected)[heldOutPacked.Count];
            for (var index = 0; index < heldOutPacked.Count; index++)
            {
                ct.ThrowIfCancellationRequested();
                var packed = heldOutPacked[index];
                var batch = packed.VectorBatch.Batch;
                var childChosen = EvaluateObserved(
                    freshPolicy, batch.ChosenIds, batch.ChosenMask, packed,
                    DesignConversionDpoEvaluatedForwardPhase.BeforeChildChosen,
                    DesignConversionDpoEvaluatedForwardPhase.AfterChildChosen,
                    forwardObserverForTesting, ct);
                var childRejected = EvaluateObserved(
                    freshPolicy, batch.RejectedIds, batch.RejectedMask, packed,
                    DesignConversionDpoEvaluatedForwardPhase.BeforeChildRejected,
                    DesignConversionDpoEvaluatedForwardPhase.AfterChildRejected,
                    forwardObserverForTesting, ct);
                var referenceChosen = EvaluateObserved(
                    reference, batch.ChosenIds, batch.ChosenMask, packed,
                    DesignConversionDpoEvaluatedForwardPhase.BeforeReferenceChosen,
                    DesignConversionDpoEvaluatedForwardPhase.AfterReferenceChosen,
                    forwardObserverForTesting, ct);
                var referenceRejected = EvaluateObserved(
                    reference, batch.RejectedIds, batch.RejectedMask, packed,
                    DesignConversionDpoEvaluatedForwardPhase.BeforeReferenceRejected,
                    DesignConversionDpoEvaluatedForwardPhase.AfterReferenceRejected,
                    forwardObserverForTesting, ct);
                heldOutScores[index] = (
                    childChosen,
                    childRejected,
                    referenceChosen,
                    referenceRejected);
            }

            var metricSummary = ComputeEvaluatedMetricSummary(heldOutScores);
            if (!metricSummary.IsSuccess)
                return metricSummary.MapError<DesignConversionDpoEvaluatedBridgeLease>();
            var metrics = metricSummary.Value!;

            if (!ReferenceMatchesSnapshot(reference, loraPreparation.Value.ReferenceSnapshot) ||
                !ModelsHaveEqualParameters(policy, freshPolicy) ||
                !ReadBoundedOwnedFile(
                        retainedScratchDirectory,
                        Path.GetFileName(scratchPolicyPath),
                        256L * 1024 * 1024,
                        ct,
                        scratchPolicy.OwnedFile.Identity)
                    .Payload.SequenceEqual(policyCheckpointPayload))
            {
                return OperationResult<DesignConversionDpoEvaluatedBridgeLease>.Fail(
                    "DesignConversion DPO evaluated checkpoint changed during verification.",
                    "DESIGN_CONVERSION_DPO_EVALUATED_CHECKPOINT_MUTATED");
            }

            var referenceSha256 = policyBase.ModelSet.Identity.BaseCheckpointSha256;
            var evaluation = new DesignConversionDpoHeldOutEvaluation(
                EvaluatedSchema,
                EvaluatedPackingId,
                config.VocabSize,
                config.ContextLen,
                actualDpoStepCount,
                effectiveOptions.MaxBridgePreferenceCount,
                policyCheckpointSha256,
                referenceSha256,
                useContinuation ? "immediate_parent" : "provider_fresh_step_zero",
                consumedPartition.DatasetId,
                consumedPartition.PartitionSnapshotSha256,
                consumedPartition.TrainRecordSetSha256,
                consumedPartition.HeldOutRecordSetSha256,
                consumedPartition.TrainSourceSetSha256,
                consumedPartition.HeldOutSourceSetSha256,
                selectedTrainInputSha256,
                heldOutEvaluationInputSha256,
                selectedTrainRows.Count,
                heldOutRows.Count,
                heldOutPacked.Count,
                metrics.CorrectCount,
                metrics.Accuracy,
                metrics.MeanRelativeMargin,
                metrics.MeanChildMargin,
                metrics.MeanReferenceMargin,
                FreshLoadVerified: true,
                ReferenceFrozen: true,
                SourceDisjoint: true,
                HeldOutNotTrained: true)
            {
                G3Metrics = new DesignConversionG3AcceptanceMetrics(
                    HeldOutCount: heldOutPacked.Count,
                    BaselineWinCount: metrics.CorrectCount,
                    BaselineWinRate: metrics.Accuracy,
                    AbsoluteChildPreferenceCorrectCount:
                        metrics.AbsoluteChildPreferenceCorrectCount,
                    AbsoluteChildPreferenceAccuracy:
                        metrics.AbsoluteChildPreferenceAccuracy)
                {
                    MeasurementNoiseScorerIdentity =
                        EvaluatedMeasurementNoiseScorerIdentity,
                    MeasurementNoiseEvaluationSetId = consumedPartition.DatasetId
                },
                MeasurementNoiseStatus =
                    MeasurementNoisePolicy.DifferenceNotProvenNoCalibrationStatus
            };
            var lineage = BuildCycleAccuracyLineage(
                evaluation,
                continuationRequest,
                useContinuation);
            if (!lineage.IsSuccess)
                return lineage.MapError<DesignConversionDpoEvaluatedBridgeLease>();
            evaluation = evaluation with
            {
                CycleAccuracyLineage = lineage.Value
            };
            if (!EvaluatedMetricsAreFinite(evaluation))
                return EvaluatedNonFinite<DesignConversionDpoEvaluatedBridgeLease>();

            var policyCheckpointFileName = DpoTrainer.BuildPolicyCheckpointFileName(
                actualDpoStepCount);
            var runtimeInferenceEncodingBinding =
                BuildRuntimeInferenceEncodingBinding(
                    config,
                    evaluated: true);
            if (!runtimeInferenceEncodingBinding.IsSuccess ||
                runtimeInferenceEncodingBinding.Value is null)
            {
                return runtimeInferenceEncodingBinding.MapError<
                    DesignConversionDpoEvaluatedBridgeLease>();
            }
            var bridgeEvidenceRefs = MergeEvidenceRefs(
                BuildBridgeEvidenceRefs(
                    consumedSourceRecordCount,
                    selectedTrainRows.Count,
                    actualDpoStepCount,
                    policyCheckpointFileName,
                    policyCheckpointSha256,
                    policyBase.ModelSet.Identity,
                    policyBase.ConfigSha256,
                    loraPreparation.Value.InitialWeightSha256,
                    loraPreparation.Value.LoraContext,
                    loraPreparation.Value.RandomContext,
                    runtimeEncodingBinding,
                    runtimeInferenceEncodingBinding.Value),
                legalTeacherAdmission.EvidenceRefs)
                .Concat([
                    "DESIGN_CONVERSION_DPO_HELDOUT_EVALUATED",
                    "DESIGN_CONVERSION_DPO_EVALUATED_PAIR_AWARE_PACKING_V1",
                    "DESIGN_CONVERSION_DPO_HELDOUT_NOT_TRAINED"
                ])
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray();
            var policyCheckpointRefResult = BuildPolicyCheckpointRef(
                consumedSourceRecordCount,
                selectedTrainRows.Count,
                actualDpoStepCount,
                policyCheckpointFileName,
                policyCheckpointSha256,
                policyCheckpointPayload.Length,
                config,
                policyBase.ModelSet.Identity,
                policyBase.ConfigSha256,
                loraPreparation.Value.InitialWeightSha256,
                loraPreparation.Value.LoraContext,
                loraPreparation.Value.RandomContext,
                bridgeEvidenceRefs,
                legalTeacherAdmissionCore.Value,
                evaluation);
            if (!policyCheckpointRefResult.IsSuccess || policyCheckpointRefResult.Value is null)
                return policyCheckpointRefResult.MapError<DesignConversionDpoEvaluatedBridgeLease>();
            var policyCheckpointRef = policyCheckpointRefResult.Value;

            // The final runtime behavior is supplied by the verified sibling
            // DPO policy checkpoint and its direct-inference scorer. Do not run
            // a second, checkpoint-derived optimizer batch under the earlier
            // train-row admission: that batch did not exist when the lease was
            // bound and therefore cannot be an exact admitted object.
            var runtimeMachine = new DesignConversionMachine();
            var checkpoint = runtimeMachine.ExportCheckpoint();
            if (!checkpoint.IsSuccess)
                return checkpoint.MapError<DesignConversionDpoEvaluatedBridgeLease>();
            var heldOutAccuracyEvidenceRefs = BuildHeldOutRuntimeCheckpointAccuracyEvidence(evaluation);
            var bridgedCheckpoint = AddDpoPolicyCheckpointEvidence(
                checkpoint.Value!,
                bridgeEvidenceRefs,
                policyCheckpointRef,
                evaluation.Accuracy,
                heldOutAccuracyEvidenceRefs);
            var scratchRuntimePath = Path.Combine(scratchDirectory, "runtime-checkpoint.json");
            var runtimePayload = JsonSerializer.SerializeToUtf8Bytes(
                bridgedCheckpoint,
                DesignConversionCheckpointJsonOptions.Standard);
            ct.ThrowIfCancellationRequested();
            var scratchCheckpointPermit = AcquireEvaluatedBridgeMutationPermit(
                trainingLease,
                trainingBinding,
                "dpo_bridge:evaluated:runtime_scratch_checkpoint");
            if (!scratchCheckpointPermit.IsSuccess || scratchCheckpointPermit.Value is null)
                return scratchCheckpointPermit.MapError<DesignConversionDpoEvaluatedBridgeLease>();
            DesignConversionDpoRetainedOwnedFile scratchRuntime;
            using (scratchCheckpointPermit.Value)
            {
                scratchRuntime = WriteExclusiveOwned(
                    retainedScratchDirectory,
                    scratchDirectory,
                    Path.GetFileName(scratchRuntimePath),
                    runtimePayload);
            }
            ownedScratchFiles.Add(scratchRuntime);
            var runtimeSha256 = scratchRuntime.Sha256;
            ownershipObserverForTesting?.Invoke(
                new DesignConversionDpoEvaluatedOwnershipObservation(
                    DesignConversionDpoEvaluatedOwnershipPhase.AfterScratchRuntimeRetained,
                    scratchDirectory,
                    scratchRuntimePath));

            var finalPolicyPath = Path.Combine(outputFullPath, policyCheckpointFileName);
            var finalRuntimePath = finalRuntimeFullPath;
            retainedFinalDirectory = DesignConversionHandleBoundDirectory.AcquireExisting(
                outputFullPath,
                beforeIo: null);
            var terminalPublicationPermit = AcquireEvaluatedBridgeMutationPermit(
                trainingLease,
                trainingBinding,
                "dpo_bridge:evaluated:terminal_publication");
            if (!terminalPublicationPermit.IsSuccess || terminalPublicationPermit.Value is null)
                return terminalPublicationPermit.MapError<DesignConversionDpoEvaluatedBridgeLease>();
            DesignConversionDpoRetainedOwnedFile finalPolicy;
            DesignConversionDpoRetainedOwnedFile finalRuntime;
            using (terminalPublicationPermit.Value)
            {
                try
                {
                    finalPolicy = WriteExclusiveOwned(
                        retainedFinalDirectory,
                        outputFullPath,
                        Path.GetFileName(finalPolicyPath),
                        policyCheckpointPayload);
                }
                catch (DesignConversionHandleBoundNameCollisionException)
                {
                    return EvaluatedFinalArtifactCollision<DesignConversionDpoEvaluatedBridgeLease>();
                }
                createdFinals.Add(finalPolicy);
                ownershipObserverForTesting?.Invoke(
                    new DesignConversionDpoEvaluatedOwnershipObservation(
                        DesignConversionDpoEvaluatedOwnershipPhase.AfterFinalPolicyRetained,
                        scratchDirectory,
                        finalPolicyPath));
                try
                {
                    finalRuntime = WriteExclusiveOwned(
                        retainedFinalDirectory,
                        outputFullPath,
                        Path.GetFileName(finalRuntimePath),
                        runtimePayload);
                }
                catch (DesignConversionHandleBoundNameCollisionException)
                {
                    return EvaluatedFinalArtifactCollision<DesignConversionDpoEvaluatedBridgeLease>();
                }
                createdFinals.Add(finalRuntime);
                ownershipObserverForTesting?.Invoke(
                    new DesignConversionDpoEvaluatedOwnershipObservation(
                        DesignConversionDpoEvaluatedOwnershipPhase.AfterFinalRuntimeRetained,
                        scratchDirectory,
                        finalRuntimePath));
            }
            var terminalPublicationResultPermit = AcquireEvaluatedBridgeMutationPermit(
                trainingLease,
                trainingBinding,
                "dpo_bridge:evaluated:terminal_publication_result");
            if (!terminalPublicationResultPermit.IsSuccess || terminalPublicationResultPermit.Value is null)
                return terminalPublicationResultPermit.MapError<DesignConversionDpoEvaluatedBridgeLease>();
            using (terminalPublicationResultPermit.Value)
            {
                if (!ReadBoundedOwnedFile(
                        retainedFinalDirectory,
                        finalPolicy.LeafName,
                        256L * 1024 * 1024,
                        ct,
                        finalPolicy.Identity)
                    .Payload.SequenceEqual(policyCheckpointPayload) ||
                !ReadBoundedOwnedFile(
                        retainedFinalDirectory,
                        finalRuntime.LeafName,
                        64L * 1024 * 1024,
                        ct,
                        finalRuntime.Identity)
                    .Payload.SequenceEqual(runtimePayload))
                {
                    return OperationResult<DesignConversionDpoEvaluatedBridgeLease>.Fail(
                        "DesignConversion DPO evaluated final artifacts changed during verification.",
                        "DESIGN_CONVERSION_DPO_EVALUATED_FINAL_ARTIFACT_MUTATED");
                }
            }

            var finalFreshLoadPermit = AcquireEvaluatedBridgeMutationPermit(
                trainingLease,
                trainingBinding,
                "dpo_bridge:evaluated:final_runtime_fresh_load");
            if (!finalFreshLoadPermit.IsSuccess || finalFreshLoadPermit.Value is null)
                return finalFreshLoadPermit.MapError<DesignConversionDpoEvaluatedBridgeLease>();
            OperationResult<DesignConversionMachineLoadResult> loaded;
            using (finalFreshLoadPermit.Value)
                loaded = await DesignConversionMachine.LoadCheckpointAsync(finalRuntimePath, ct);
            var finalFreshLoadResultPermit = AcquireEvaluatedBridgeMutationPermit(
                trainingLease,
                trainingBinding,
                "dpo_bridge:evaluated:final_runtime_fresh_load_result");
            if (!finalFreshLoadResultPermit.IsSuccess || finalFreshLoadResultPermit.Value is null)
                return finalFreshLoadResultPermit.MapError<DesignConversionDpoEvaluatedBridgeLease>();
            using var finalFreshLoadResultPermitLifetime = finalFreshLoadResultPermit.Value;
            if (!loaded.IsSuccess)
                return loaded.MapError<DesignConversionDpoEvaluatedBridgeLease>();
            var routeBGate = DesignConversionDpoTrainerRouteBGate.ValidateTypedRuntimeCheckpoint(
                loaded.Value!.Checkpoint.DpoPolicyCheckpointRef,
                actualDpoStepCount,
                bridgeEvidenceRefs,
                callerFreshLoadVerified: true);
            if (!routeBGate.IsSuccess)
                return routeBGate.MapError<DesignConversionDpoEvaluatedBridgeLease>();

            var runtimeExport = new LearningMachineExport(
                DesignConversionMachine.MachineId,
                DesignConversionMachine.DomainTag,
                loaded.Value!.Checkpoint.Version,
                Path.GetFileName(finalRuntimePath),
                $"{DesignConversionMachine.MachineId}/{loaded.Value.Checkpoint.Version}/manifest.json",
                loaded.Value.Checkpoint.AccuracyAfterLastBatch,
                loaded.Value.Checkpoint.ExportedAtUtc);
            var result = new DesignConversionDpoPolicyRuntimeCheckpointBridgeResult(
                consumedSourceRecordCount,
                selectedTrainRows.Count,
                actualDpoStepCount,
                policyCheckpointFileName,
                policyCheckpointSha256,
                Path.GetFileName(finalRuntimePath),
                runtimeExport,
                loaded.Value.Machine,
                loaded.Value.Checkpoint,
                true,
                DesignConversionDpoTrainerRouteBGate.RuntimeIngredientIds(),
                bridgeEvidenceRefs.Concat(loaded.Value.Checkpoint.EvidenceRefs)
                    .Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray(),
                loaded.Value.Checkpoint.DpoPolicyCheckpointRef,
                descriptor)
            {
                CanonicalBpeTokenizerSnapshot =
                    policyBase.ModelSet.CanonicalBpeTokenizerSnapshot
            };
            ownershipObserverForTesting?.Invoke(
                new DesignConversionDpoEvaluatedOwnershipObservation(
                    DesignConversionDpoEvaluatedOwnershipPhase.BeforeLeaseCreated,
                    scratchDirectory,
                    null));
            if (!DeleteOwnedScratchDirectoryNoGap(
                    scratchDirectory,
                    retainedScratchDirectory,
                    ownedScratchFiles,
                    ownershipObserverForTesting))
            {
                return OperationResult<DesignConversionDpoEvaluatedBridgeLease>.Fail(
                    "DesignConversion DPO evaluated scratch cleanup could not prove owned namespace removal.",
                    "DESIGN_CONVERSION_DPO_EVALUATED_SCRATCH_CLEANUP_NOT_PROVEN");
            }
            scratchCleanupProven = true;
            var lease = new DesignConversionDpoEvaluatedBridgeLease(
                result,
                evaluation,
                finalPolicyPath,
                finalRuntimePath,
                policyCheckpointSha256,
                runtimeSha256,
                finalPolicy.Identity,
                finalRuntime.Identity,
                trainingLease,
                retainedFinalDirectory,
                finalPolicy,
                finalRuntime);
            var success = OperationResult<DesignConversionDpoEvaluatedBridgeLease>.Ok(lease);
            createdFinals.Clear();
            retainedFinalDirectory = null;
            trainingLeaseTransfer.TransferOwnership();
            retainEvaluatedArtifacts = true;
            return success;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Cancelled<DesignConversionDpoEvaluatedBridgeLease>();
        }
        catch (Exception exception)
            when ((exception is IOException or UnauthorizedAccessException or InvalidOperationException
                   or ArgumentException or OverflowException) &&
                  SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<DesignConversionDpoEvaluatedBridgeLease>.Fail(
                "DesignConversion DPO evaluated policy bridge failed.",
                "DESIGN_CONVERSION_DPO_EVALUATED_BRIDGE_FAILED");
        }
        finally
        {
            foreach (var artifact in createdFinals.AsEnumerable().Reverse())
                DeleteIdentityAndHashMatchedBestEffort(
                    artifact,
                    retainedFinalDirectory);
            if (!scratchCleanupProven &&
                scratchDirectory is not null &&
                retainedScratchDirectory is not null)
            {
                try
                {
                    DeleteOwnedScratchDirectoryNoGap(
                        scratchDirectory,
                        retainedScratchDirectory,
                        ownedScratchFiles,
                        ownershipObserverForTesting);
                }
                catch (Exception exception)
                    when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
                {
                }
            }
            retainedScratchDirectory?.Dispose();
            foreach (var artifact in ownedScratchFiles)
                artifact.Dispose();
            foreach (var artifact in createdFinals)
                artifact.Dispose();
            retainedFinalDirectory?.Dispose();
            if (!retainEvaluatedArtifacts && !outputDirectoryExistedBefore)
                DeleteEmptyDirectoryBestEffort(outputFullPath);
        }
    }

    private static OperationResult<PreparedEvaluatedCompositeExecution>
        CaptureExactPreparedCompositeExecution(
            PreparedEvaluatedCompositeExecution prepared)
    {
        try
        {
            var capturedTrain = prepared.Optimizer.TrainPacked
                .Select(CloneEvaluatedPackedBatch)
                .ToArray();
            var capturedHeldOut = prepared.Optimizer.HeldOutPacked
                .Select(CloneEvaluatedPackedBatch)
                .ToArray();
            var trainAdmissionValidation =
                ValidateEvaluatedTrainPackedAdmissions(capturedTrain);
            if (!trainAdmissionValidation.IsSuccess)
            {
                return trainAdmissionValidation.MapError<
                    PreparedEvaluatedCompositeExecution>();
            }
            var heldOutAdmissionValidation =
                ValidateEvaluatedHeldOutAdmissions(capturedHeldOut);
            if (!heldOutAdmissionValidation.IsSuccess)
            {
                return heldOutAdmissionValidation.MapError<
                    PreparedEvaluatedCompositeExecution>();
            }
            var config = prepared.Optimizer.ProviderPreparation.Config;
            var trainInputSha256 = RecomputeEvaluatedInputFingerprint(
                prepared.ConsumedPartition.TrainRecords,
                capturedTrain.Select(static value => value.VectorBatch).ToArray(),
                config,
                includeTrainingVectorAndWeight: true);
            var heldOutInputSha256 = RecomputeEvaluatedInputFingerprint(
                prepared.ConsumedPartition.HeldOutRecords,
                capturedHeldOut.Select(static value => value.VectorBatch).ToArray(),
                config,
                includeTrainingVectorAndWeight: false);
            if (!string.Equals(
                    trainInputSha256,
                    prepared.Optimizer.TrainInputSha256,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    heldOutInputSha256,
                    prepared.Optimizer.HeldOutInputSha256,
                    StringComparison.Ordinal))
            {
                return CompositeExecutionInvalid<
                    PreparedEvaluatedCompositeExecution>();
            }

            var projected = DesignConversionDpoTrainingContentAuthority
                .ProjectFinalOptimizerInputs(
                    capturedTrain,
                    capturedTrain
                        .Select(static input => input.Provenance)
                        .ToArray(),
                    static input => input.Prompt,
                    static input => input.Chosen,
                    static input => input.Rejected,
                    static input => input.VectorBatch.TeachingVector,
                    static input => input.VectorBatch.Weight,
                    DesignConversionDpoTrainingContentAuthority
                        .BuildEvaluatedBridgeFinalOptimizerContext);
            if (!projected.IsSuccess || projected.Value is null ||
                !MatchesExactProjectedContent(
                    projected.Value.Content,
                    prepared.FinalOptimizer.Content))
            {
                return CompositeExecutionInvalid<
                    PreparedEvaluatedCompositeExecution>();
            }

            return OperationResult<PreparedEvaluatedCompositeExecution>.Ok(
                prepared with
                {
                    Optimizer = prepared.Optimizer with
                    {
                        TrainPacked = projected.Value.Inputs,
                        HeldOutPacked = Array.AsReadOnly(capturedHeldOut)
                    }
                });
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return CompositeExecutionInvalid<
                PreparedEvaluatedCompositeExecution>();
        }
    }

    private static DesignConversionDpoEvaluatedPackedBatch
        CloneEvaluatedPackedBatch(
            DesignConversionDpoEvaluatedPackedBatch input)
        => input with
        {
            VectorBatch = CloneVectorBatch(input.VectorBatch),
            Provenance = input.Provenance is null
                ? null
                : input.Provenance with { }
        };

    private static DpoVectorBatch CloneVectorBatch(DpoVectorBatch input)
        => new(
            new DpoBatch
            {
                ChosenIds = CloneTensor(input.Batch.ChosenIds),
                ChosenMask = CloneTensor(input.Batch.ChosenMask),
                RejectedIds = CloneTensor(input.Batch.RejectedIds),
                RejectedMask = CloneTensor(input.Batch.RejectedMask),
                // Clone не является новым issuer: он сохраняет уже выданный
                // паспорт, чтобы transferred/snapshot путь валидировал тот же raw triple.
                Admission = input.Batch.Admission
            },
            input.TeachingVector.ToArray(),
            input.Weight);

    internal static DpoVectorBatch CloneVectorBatchPreviewForTests(
        DpoVectorBatch input)
        => CloneVectorBatch(input);

    private static void PublishEvaluatedMutationSinkObservation(
        IReadOnlyList<DesignConversionDpoEvaluatedPackedBatch> inputs)
    {
        var observer = OptimizerMutationSinkObserver.Value;
        if (observer is null)
            return;

        var detachedInputs = inputs
            .Select(CloneEvaluatedPackedBatch)
            .ToArray();
        observer(new DesignConversionDpoOptimizerMutationSinkObservation(
            "evaluated_dpo",
            detachedInputs.Cast<object>().ToArray(),
            detachedInputs
                .Select(static input => input.VectorBatch)
                .ToArray()));
    }

    private static void PublishStandaloneMutationSinkObservation(
        IReadOnlyList<FinalVectorOptimizerInput> inputs)
    {
        var observer = OptimizerMutationSinkObserver.Value;
        if (observer is null)
            return;

        var detachedInputs = inputs
            .Select(static input => input with
            {
                Batch = CloneVectorBatch(input.Batch),
                Provenance = input.Provenance is null
                    ? null
                    : input.Provenance with { }
            })
            .ToArray();
        observer(new DesignConversionDpoOptimizerMutationSinkObservation(
            "standalone_dpo",
            detachedInputs.Cast<object>().ToArray(),
            detachedInputs.Select(static input => input.Batch).ToArray()));
    }

    private static Tensor CloneTensor(Tensor tensor)
        => new(
            tensor.Data.ToArray(),
            new TensorShape(tensor.Shape.Dimensions));

    private static bool MatchesExactProjectedContent(
        DpoTrainingContentBatch actual,
        DpoTrainingContentBatch expected)
    {
        if (!string.Equals(
                actual.Sha256,
                expected.Sha256,
                StringComparison.Ordinal) ||
            actual.Rows.Count != expected.Rows.Count)
        {
            return false;
        }

        for (var index = 0; index < actual.Rows.Count; index++)
        {
            if (!string.Equals(
                    actual.Rows[index].Sha256,
                    expected.Rows[index].Sha256,
                    StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    private static OperationResult<T> CompositeExecutionInvalid<T>()
        => OperationResult<T>.Fail(
            "The exact composite-bound prepared bridge child is required.",
            LegalTrainingTeacherAdmissionAuthority
                .TrainingContentInvalidErrorCode);

    private static async Task<OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>> ExecuteWithAuthorityCoreAsync(
        ServerOwnedLegalTrainingTeacherAdmissionAuthority? legalTeacherAdmissionAuthority,
        IReadOnlyList<DesignConversionDpoTrainingRecord>? records,
        string outputDirectory,
        string runId,
        string runtimeCheckpointPath,
        IDesignConversionDpoPolicyBaseProvider? policyBaseProvider,
        DesignConversionDpoPolicyCheckpointBridgeOptions? options,
        DesignConversionDpoPolicyContinuationRequest? continuationRequest,
        bool useContinuation,
        CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return Cancelled<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();

        if (legalTeacherAdmissionAuthority is null ||
            !legalTeacherAdmissionAuthority.IsAvailable)
        {
            return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail(
                LegalTeacherConfigurationRequiredErrorMessage,
                LegalTeacherConfigurationRequiredErrorCode);
        }

        var validatedOptions = ValidateOptions(options);
        if (!validatedOptions.IsSuccess)
            return validatedOptions.MapError<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
        var effectiveOptions = validatedOptions.Value!;

        if (records is null)
            return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail(
                "DesignConversion DPO training records are required for the policy bridge.",
                "DESIGN_CONVERSION_DPO_POLICY_BRIDGE_RECORDS_REQUIRED");

        if (string.IsNullOrWhiteSpace(outputDirectory))
            return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail(
                "DesignConversion DpoTrainer policy bridge requires an output directory.",
                "DESIGN_CONVERSION_DPO_POLICY_BRIDGE_OUTPUT_DIRECTORY_REQUIRED");

        if (string.IsNullOrWhiteSpace(runtimeCheckpointPath))
            return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail(
                "DesignConversion DpoTrainer policy bridge requires a runtime checkpoint path.",
                "DESIGN_CONVERSION_DPO_POLICY_BRIDGE_RUNTIME_CHECKPOINT_REQUIRED");

        var boundedSnapshot = DesignConversionTrainingPipeline
            .SnapshotTrainingRecords(records);
        if (!boundedSnapshot.IsSuccess || boundedSnapshot.Value is null)
            return boundedSnapshot.MapError<
                DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
        var recordsSnapshot = boundedSnapshot.Value;

        DesignConversionDpoPolicyContinuationRequest? continuationSnapshot;
        try
        {
            continuationSnapshot = useContinuation
                ? SnapshotContinuationRequest(continuationRequest)
                : null;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            continuationSnapshot = null;
        }

        IReadOnlyList<DesignConversionDpoTrainingRecord> trainableRows = SelectTrainableRows(
            recordsSnapshot,
            effectiveOptions.MaxBridgePreferenceCount);
        if (trainableRows.Count == 0)
        {
            return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail(
                "No DesignConversion DPO training rows are trainable for the policy bridge.",
                "DESIGN_CONVERSION_DPO_POLICY_BRIDGE_NO_TRAINABLE_ROWS");
        }

        if (ct.IsCancellationRequested)
            return Cancelled<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();

        var qualityGate = EvaluateLocalAblationQualityGate(trainableRows);
        if (!qualityGate.PassesQualityGate)
        {
            return OperationResult<
                DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail(
                "DesignConversion DpoTrainer policy bridge local ablation gate failed.",
                LocalAblationGateFailureErrorCode);
        }
        var providerPreparation = PreparePolicyBaseDescriptor(
            policyBaseProvider,
            continuationSnapshot,
            useContinuation,
            requireEvaluatedContinuationParent: false);
        if (!providerPreparation.IsSuccess ||
            providerPreparation.Value is null)
        {
            return providerPreparation.MapError<
                DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
        }
        var proofPreparation =
            await AttachBridgeOwnedContinuationMutationProofAsync(
                providerPreparation.Value,
                continuationSnapshot,
                useContinuation,
                ct);
        if (!proofPreparation.IsSuccess || proofPreparation.Value is null)
        {
            return proofPreparation.MapError<
                DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
        }

        var config = proofPreparation.Value.Config;
        var descriptor = DesignConversionDpoSourceEncoderContractBuilder
            .BuildDescriptor(trainableRows, config);
        if (!descriptor.IsSuccess || descriptor.Value is null)
            return descriptor.MapError<
                DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
        DesignConversionDpoRuntimeEncodingBinding? runtimeEncodingBinding =
            null;
        if (useContinuation)
        {
            var runtimeEncoding =
                ValidateContinuationRuntimeEncodingForPacking(
                    continuationSnapshot!.RuntimeEncodingBinding,
                    descriptor.Value,
                    config);
            if (!runtimeEncoding.IsSuccess || runtimeEncoding.Value is null)
            {
                return runtimeEncoding.MapError<
                    DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
            }
            runtimeEncodingBinding = runtimeEncoding.Value;
        }
        // Предполётная правовая проверка законности учителей: абсолютный
        // правовой запрет обязан отказывать раньше, чем проверка
        // действительности паспорта пары внутри цикла сборки входов обучателя.
        var legalTeacherPreflight = ValidateStandaloneLegalTeacherPreflight(
            // Та же правовая власть, у которой ниже берётся учебная аренда.
            legalTeacherAdmissionAuthority,
            // Судим ровно те строки, которые пойдут в обучение.
            trainableRows,
            // Отмену пробрасываем, чтобы предполёт её уважал.
            ct);
        // Правовой отказ выносим наружу немедленно и без переименования кода,
        // чтобы он остался опознаваем как правовой, а не как отказ по
        // неисправной записи.
        if (!legalTeacherPreflight.IsSuccess)
        {
            return legalTeacherPreflight.MapError<
                DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
        }
        var optimizerInputs = new FinalVectorOptimizerInput[
            trainableRows.Count];
        for (var index = 0; index < trainableRows.Count; index++)
        {
            var row = trainableRows[index];
            var preference = BuildPolicyPreferenceText(row);
            var batchResult = runtimeEncodingBinding is null
                ? BuildVectorBatch(row, config)
                : BuildVectorBatch(row, runtimeEncodingBinding);
            if (!batchResult.IsSuccess || batchResult.Value is null)
            {
                return batchResult.MapError<
                    DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
            }

            optimizerInputs[index] = new FinalVectorOptimizerInput(
                batchResult.Value,
                BuildPolicyPrompt(row),
                preference.Chosen,
                preference.Rejected,
                row.TeacherProvenance,
                "design-conversion-policy-vector-final-v1")
            {
                ChosenAuthorKind = row.ChosenAuthorKind,
                RejectedAuthorKind = row.RejectedAuthorKind
            };
        }
        var finalOptimizer = DesignConversionDpoTrainingContentAuthority
            .ProjectFinalOptimizerInputs(
                optimizerInputs,
                optimizerInputs
                    .Select(static input => input.Provenance)
                    .ToArray(),
                static input => input.Prompt,
                static input => input.Chosen,
                static input => input.Rejected,
                static input => input.Batch.TeachingVector,
                static input => input.Batch.Weight,
                static input => input.Context);
        if (!finalOptimizer.IsSuccess || finalOptimizer.Value is null)
            return finalOptimizer.MapError<
                DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();

        LeaseAcquisitionObserver.Value?.Invoke("dpo_bridge:standalone:lease_acquisition");
        var leaseResult = legalTeacherAdmissionAuthority.AcquireTrainingLease(
            finalOptimizer.Value.Content,
            finalOptimizer.Value.Provenance);
        if (!leaseResult.IsSuccess || leaseResult.Value is null)
            return leaseResult.MapError<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();

        using var trainingLease = leaseResult.Value;
        return await ExecuteStandaloneSnapshotWithTransferredLeaseAsync(
            trainingLease,
            recordsSnapshot,
            trainableRows,
            outputDirectory,
            runId,
            runtimeCheckpointPath,
            effectiveOptions,
            continuationSnapshot,
            useContinuation,
            new PreparedStandaloneOptimizer(
                proofPreparation.Value,
                descriptor.Value,
                runtimeEncodingBinding,
                finalOptimizer.Value.Inputs),
            ct);
    }

    private static OperationResult<ValidatedProviderPreparation>
        PreparePolicyBaseDescriptor(
            IDesignConversionDpoPolicyBaseProvider? provider,
            DesignConversionDpoPolicyContinuationRequest? continuation,
            bool useContinuation,
            bool requireEvaluatedContinuationParent)
    {
        if (useContinuation)
        {
            if (provider is not IDesignConversionDpoPolicyContinuationProvider)
                return ContinuationProviderRequired<ValidatedProviderPreparation>();
            if (continuation is null ||
                continuation.PolicyCheckpointContract?.Binding is null ||
                continuation.RuntimeEncodingBinding is null)
            {
                return ContinuationContractInvalid<ValidatedProviderPreparation>();
            }
            if (!DesignConversionDpoPromotionArtifactContractCodec
                    .ValidateContractPair(
                        continuation.SourceEncoderContract,
                        continuation.PolicyCheckpointContract))
            {
                if (string.Equals(
                        continuation.PolicyCheckpointContract.Schema,
                        DesignConversionDpoPromotionArtifactContractCodec
                            .LegacyCommittedPolicyCheckpointContractSchema,
                        StringComparison.Ordinal) ||
                    string.Equals(
                        continuation.PolicyCheckpointContract.Schema,
                        DesignConversionDpoPromotionArtifactContractCodec
                            .LegacyPolicyCheckpointContractSchema,
                        StringComparison.Ordinal))
                {
                    return DesignConversionDpoCheckpointMutationAdmission
                        .Legacy<ValidatedProviderPreparation>();
                }

                return continuation.CheckpointMutationAdmission is null
                    ? ContinuationContractInvalid<ValidatedProviderPreparation>()
                    : DesignConversionDpoCheckpointMutationAdmission
                        .Mismatch<ValidatedProviderPreparation>();
            }

            // Parent continuation уже прошла current-contract проверку, но
            // bridge заново требует live admission до provider/trainer: custom
            // provider не может заменить отсутствие authority своим ModelSet.
            var admissionValidation =
                DesignConversionDpoCheckpointMutationAdmission
                    .ValidateCurrentForContinuation(
                        continuation.CheckpointMutationAdmission,
                        continuation.SourceEncoderContract,
                        continuation.PolicyCheckpointContract);
            if (!admissionValidation.IsSuccess)
                return admissionValidation.MapError<ValidatedProviderPreparation>();

            if (requireEvaluatedContinuationParent &&
                !TryReadParentCycleOrdinal(
                    continuation,
                    out _,
                    out _))
            {
                return ContinuationContractInvalid<ValidatedProviderPreparation>();
            }
        }
        else if (provider is null)
        {
            return PolicyBaseProviderRequired<ValidatedProviderPreparation>();
        }

        try
        {
            var described = provider!.DescribePreparation();
            if (!described.IsSuccess || described.Value is null)
                return PolicyBaseProviderRequired<ValidatedProviderPreparation>();
            var descriptor = described.Value;
            var config = descriptor.ToConfig();
            var baseIdentity = descriptor.ToIdentity();
            if (!string.Equals(
                    descriptor.Schema,
                    DesignConversionDpoPolicyBasePreparationDescriptor.CurrentSchema,
                    StringComparison.Ordinal) ||
                !DesignConversionDpoPolicyBaseMetadata.IsValidIdentity(baseIdentity) ||
                !DesignConversionDpoPolicyBaseMetadata.IsValidConfig(config) ||
                !DesignConversionDpoPolicyBaseMetadata.IsCanonicalSha256(
                    descriptor.ConfigSha256) ||
                !string.Equals(
                    descriptor.ConfigSha256,
                    DesignConversionDpoPolicyBaseMetadata.ComputeConfigSha256(config),
                    StringComparison.Ordinal))
            {
                return PolicyBaseProviderRequired<ValidatedProviderPreparation>();
            }

            var expectedIdentity = useContinuation
                ? baseIdentity with
                {
                    BaseCheckpointSha256 = continuation!
                        .PolicyCheckpointContract.Binding.PolicyCheckpointSha256
                }
                : baseIdentity;
            var runtimeEncodingBinding = useContinuation
                ? continuation!.RuntimeEncodingBinding
                : null;
            return OperationResult<ValidatedProviderPreparation>.Ok(
                new ValidatedProviderPreparation(
                    provider,
                    descriptor,
                    expectedIdentity,
                    config,
                    descriptor.ConfigSha256,
                    runtimeEncodingBinding,
                    ContinuationMutationProof: null));
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return PolicyBaseProviderRequired<ValidatedProviderPreparation>();
        }
    }

    private static async Task<OperationResult<ValidatedProviderPreparation>>
        AttachBridgeOwnedContinuationMutationProofAsync(
            ValidatedProviderPreparation preparation,
            DesignConversionDpoPolicyContinuationRequest? continuation,
            bool useContinuation,
            CancellationToken ct)
    {
        if (!useContinuation)
            return OperationResult<ValidatedProviderPreparation>.Ok(preparation);
        if (ct.IsCancellationRequested)
            return Cancelled<ValidatedProviderPreparation>();
        if (continuation is null)
            return ContinuationContractInvalid<ValidatedProviderPreparation>();

        var proof = await CreateBridgeOwnedContinuationMutationProofAsync(
            continuation,
            preparation,
            ct);
        if (!proof.IsSuccess || proof.Value is null)
            return proof.MapError<ValidatedProviderPreparation>();

        return OperationResult<ValidatedProviderPreparation>.Ok(
            new ValidatedProviderPreparation(
                preparation.Provider,
                preparation.Descriptor,
                preparation.ExpectedIdentity,
                preparation.Config,
                preparation.ConfigSha256,
                preparation.RuntimeEncodingBinding,
                proof.Value));
    }

    private static async Task<
            OperationResult<BridgeOwnedContinuationMutationProof>>
        CreateBridgeOwnedContinuationMutationProofAsync(
            DesignConversionDpoPolicyContinuationRequest continuation,
            ValidatedProviderPreparation preparation,
            CancellationToken ct)
    {
        var admissionValidation =
            DesignConversionDpoCheckpointMutationAdmission
                .ValidateCurrentForContinuation(
                    continuation.CheckpointMutationAdmission,
                    continuation.SourceEncoderContract,
                    continuation.PolicyCheckpointContract);
        if (!admissionValidation.IsSuccess)
        {
            return admissionValidation.MapError<
                BridgeOwnedContinuationMutationProof>();
        }

        var binding = continuation.PolicyCheckpointContract.Binding;
        var trainingContext = continuation.PolicyCheckpointContract
            .TrainingContext;
        if (binding is null ||
            trainingContext?.RandomContext is null ||
            !DesignConversionDpoPolicyBaseMetadata.TrainingContextMatchesConfig(
                trainingContext,
                preparation.Config))
        {
            return ContinuationContractInvalid<
                BridgeOwnedContinuationMutationProof>();
        }

        var proofSource = continuation.BridgeProofCheckpointSource;
        if (proofSource is null)
        {
            return DesignConversionDpoCheckpointMutationAdmission
                .Required<BridgeOwnedContinuationMutationProof>();
        }

        OperationResult<Stream>? sourceResult;
        try
        {
            // Bridge-proof намеренно открывает только скрытый proof-source.
            // Публичный provider CheckpointSource здесь не трогаем: one-shot
            // promoted-parent не съедается до передачи built-in provider-у.
            sourceResult = await proofSource
                .OpenCheckpointSnapshotAsync(ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Cancelled<BridgeOwnedContinuationMutationProof>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return DesignConversionDpoCheckpointMutationAdmission
                .Mismatch<BridgeOwnedContinuationMutationProof>();
        }

        if (sourceResult is null ||
            !sourceResult.IsSuccess ||
            sourceResult.Value is null)
        {
            return ct.IsCancellationRequested
                ? Cancelled<BridgeOwnedContinuationMutationProof>()
                : DesignConversionDpoCheckpointMutationAdmission
                    .Mismatch<BridgeOwnedContinuationMutationProof>();
        }

        var stream = sourceResult.Value;
        try
        {
            if (!IsBridgeProofCheckpointStream(
                    stream,
                    binding.PolicyCheckpointByteLength))
            {
                return DesignConversionDpoCheckpointMutationAdmission
                    .Mismatch<BridgeOwnedContinuationMutationProof>();
            }

            var initialHash = await ComputeBridgeProofSha256Async(
                stream,
                binding.PolicyCheckpointByteLength,
                ct);
            if (!string.Equals(
                    initialHash,
                    binding.PolicyCheckpointSha256,
                    StringComparison.Ordinal))
            {
                return DesignConversionDpoCheckpointMutationAdmission
                    .Mismatch<BridgeOwnedContinuationMutationProof>();
            }

            LlmModel expectedPolicy;
            LlmModel expectedReference;
            LlmModel expectedFreshLoad;
            try
            {
                // Сверочные модели приходят от внедрённого поставщика: мост
                // больше не строит языковую модель сам (правило 2 проекта —
                // сначала фабрика, зависимости через интерфейсы).
                // Поставщик без продолжения сверочный набор отдать не умеет,
                // поэтому путь продолжения на нём отказывает закрытым кодом.
                if (preparation.Provider is not
                    IDesignConversionDpoPolicyContinuationProvider
                        verificationProvider)
                {
                    return OperationResult<
                        BridgeOwnedContinuationMutationProof>.Fail(
                        "The DesignConversion DPO continuation verification model set is unavailable.",
                        IDesignConversionDpoPolicyContinuationProvider
                            .VerificationModelSetUnavailableErrorCode);
                }

                // Настройку и случайный контекст передаём те же, которыми
                // строился набор до переноса, иначе отпечаток изменится.
                var verificationSet = await verificationProvider
                    .CreateContinuationVerificationModelSetAsync(
                        preparation.Config,
                        trainingContext.RandomContext,
                        ct);
                // Отказ поставщика выносим наружу без подмены кода.
                if (!verificationSet.IsSuccess || verificationSet.Value is null)
                {
                    return verificationSet.MapError<
                        BridgeOwnedContinuationMutationProof>();
                }

                // Три роли разбираем по своим переменным: дальше в них
                // загружается допущенная контрольная точка, как и раньше.
                expectedPolicy = verificationSet.Value.Policy;
                expectedReference = verificationSet.Value.Reference;
                expectedFreshLoad = verificationSet.Value.FreshLoadTarget;

                LoadBridgeOwnedContinuationCheckpoint(
                    expectedPolicy,
                    stream,
                    ct);
                LoadBridgeOwnedContinuationCheckpoint(
                    expectedReference,
                    stream,
                    ct);
                LoadBridgeOwnedContinuationCheckpoint(
                    expectedFreshLoad,
                    stream,
                    ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                return Cancelled<BridgeOwnedContinuationMutationProof>();
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return DesignConversionDpoCheckpointMutationAdmission
                    .Mismatch<BridgeOwnedContinuationMutationProof>();
            }

            var finalHash = await ComputeBridgeProofSha256Async(
                stream,
                binding.PolicyCheckpointByteLength,
                ct);
            if (!string.Equals(finalHash, initialHash, StringComparison.Ordinal))
            {
                return DesignConversionDpoCheckpointMutationAdmission
                    .Mismatch<BridgeOwnedContinuationMutationProof>();
            }

            if (!ContinuationModelsHaveSameBridgeOwnedState(
                    expectedPolicy,
                    expectedReference,
                    expectedFreshLoad))
            {
                return DesignConversionDpoCheckpointMutationAdmission
                    .Mismatch<BridgeOwnedContinuationMutationProof>();
            }

            // Этот fingerprint принадлежит bridge, а не provider-у: provider
            // позже отдаст только модели, и мы сравним фактические веса с
            // состоянием, загруженным здесь из admitted checkpoint A.
            var expectedFingerprint =
                ComputeContinuationModelSetStateFingerprint(
                    preparation.ExpectedIdentity,
                    preparation.ConfigSha256,
                    expectedPolicy,
                    expectedReference,
                    expectedFreshLoad);
            ContinuationMutationProofObserver.Value?.Invoke(
                new DesignConversionDpoContinuationMutationProofObservation(
                    "expected",
                    expectedFingerprint,
                    ActualModelSetStateFingerprint: null));
            return OperationResult<BridgeOwnedContinuationMutationProof>.Ok(
                new BridgeOwnedContinuationMutationProof(
                    binding.PolicyCheckpointSha256,
                    binding.PolicyCheckpointByteLength,
                    binding.PolicyCheckpointFileName,
                    expectedFingerprint));
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Cancelled<BridgeOwnedContinuationMutationProof>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return DesignConversionDpoCheckpointMutationAdmission
                .Mismatch<BridgeOwnedContinuationMutationProof>();
        }
        finally
        {
            try
            {
                stream.Dispose();
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                // Disposal не участвует в authority: уже нормализованный
                // quarantine/mismatch результат нельзя заменять деталями IO.
            }
        }
    }

    private static async Task<OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>>
        ExecuteStandaloneSnapshotWithTransferredLeaseAsync(
            ServerOwnedLegalTrainingTeacherAdmissionLease trainingLease,
            IReadOnlyList<DesignConversionDpoTrainingRecord> records,
            IReadOnlyList<DesignConversionDpoTrainingRecord> trainableRows,
            string outputDirectory,
            string runId,
            string runtimeCheckpointPath,
            DesignConversionDpoPolicyCheckpointBridgeOptions effectiveOptions,
            DesignConversionDpoPolicyContinuationRequest? continuationSnapshot,
            bool useContinuation,
            PreparedStandaloneOptimizer prepared,
            CancellationToken ct)
    {
        var legalTeacherAdmission = trainingLease.Admission;
        var legalTeacherAdmissionCore =
            DesignConversionDpoLegalTeacherAdmissionCommitmentCodec.CaptureAdmission(
                legalTeacherAdmission);
        if (!legalTeacherAdmissionCore.IsSuccess || legalTeacherAdmissionCore.Value is null)
            return legalTeacherAdmissionCore.MapError<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
        if (ct.IsCancellationRequested)
            return Cancelled<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();

        var preparedAdmissionValidation =
            ValidateStandaloneOptimizerAdmissions(prepared.Inputs);
        if (!preparedAdmissionValidation.IsSuccess)
        {
            return preparedAdmissionValidation.MapError<
                DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
        }

        var qualityGate = EvaluateLocalAblationQualityGate(trainableRows);
        if (!qualityGate.PassesQualityGate)
        {
            return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail(
                "DesignConversion DpoTrainer policy bridge local ablation gate failed: " +
                $"{qualityGate.GateReason}. " +
                $"preferred_score:{Format(qualityGate.PreferredScore)} " +
                $"baseline_score:{Format(qualityGate.BaselineScore)} " +
                $"score_delta:{Format(qualityGate.ScoreDelta)} " +
                $"threshold:{Format(qualityGate.Threshold)}",
                LocalAblationGateFailureErrorCode);
        }

        string outputFullPath;
        try
        {
            outputFullPath = Path.GetFullPath(outputDirectory);
            _ = Path.GetFullPath(runtimeCheckpointPath);
        }
        catch (Exception exception)
            when ((exception is ArgumentException
                   or NotSupportedException
                   or PathTooLongException) &&
                  SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail(
                "DesignConversion DpoTrainer policy bridge output path is invalid.",
                "DESIGN_CONVERSION_DPO_POLICY_BRIDGE_FAILED");
        }

        var preparation = prepared.ProviderPreparation;
        OperationResult<DesignConversionDpoPolicyBaseModelSet>? provided;
        try
        {
            // Provider возвращает только load/read candidate. Server-owned
            // mutation permit до fingerprint validation был бы authority leak:
            // foreign actual weights не должны открыть ни одной mutation
            // boundary, даже ранней "policy_provider".
            provided = useContinuation
                ? await ((IDesignConversionDpoPolicyContinuationProvider)preparation.Provider)
                    .CreateContinuedModelSetAsync(
                        preparation.Descriptor,
                        continuationSnapshot!,
                        ct)
                : await preparation.Provider.CreateModelSetAsync(
                    preparation.Descriptor,
                    ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Cancelled<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
        }
        catch (Exception exception)
            when (exception is not OperationCanceledException &&
                  SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return useContinuation
                ? ContinuationCheckpointMismatch<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>()
                : OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail(
                    "DesignConversion DPO policy base provider failed.",
                    "DESIGN_CONVERSION_DPO_POLICY_BASE_PROVIDER_FAILED");
        }

        ValidatedPolicyBase policyBase;
        if (ct.IsCancellationRequested ||
            provided is not null &&
            !provided.IsSuccess &&
            string.Equals(provided.ErrorCode, "CANCELLED", StringComparison.Ordinal))
        {
            return Cancelled<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
        }
        if (useContinuation && (provided is null || !provided.IsSuccess))
        {
            return provided is not null && IsStableContinuationError(provided.ErrorCode)
                ? StableContinuationFailure<
                    DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>(
                    provided.ErrorCode)
                : ContinuationCheckpointMismatch<
                    DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
        }
        if (provided is null || !provided.IsSuccess)
        {
            return OperationResult<
                DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail(
                "DesignConversion DPO policy base provider failed.",
                "DESIGN_CONVERSION_DPO_POLICY_BASE_PROVIDER_FAILED");
        }
        var validated = ValidatePolicyBaseAgainstPreparation(
            provided.Value,
            preparation);
        if (!validated.IsSuccess || validated.Value is null)
        {
            return validated.MapError<
                DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
        }

        var providerResultPermit = AcquireBridgeMutationPermit(
            trainingLease,
            "dpo_bridge:standalone:policy_provider_result");
        if (!providerResultPermit.IsSuccess || providerResultPermit.Value is null)
        {
            return providerResultPermit.MapError<
                DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
        }
        using (providerResultPermit.Value)
            policyBase = validated.Value;

        var acceptedTrainerOwnership = new Dictionary<
            DpoTrainerArtifactIdentity,
            AcceptedTrainerArtifactOwnership>();
        var standaloneOwnedArtifacts = new List<DesignConversionDpoRetainedOwnedFile>();
        DesignConversionHandleBoundDirectory? retainedStandaloneDirectory = null;
        var retainStandaloneArtifacts = false;
        try
        {
            ct.ThrowIfCancellationRequested();
            var config = policyBase.ModelSet.Policy.Config;
            var descriptor = prepared.Descriptor;
            var runtimeEncodingBinding = prepared.RuntimeEncodingBinding;

            var policyCheckpointDirectory = Path.Combine(
                outputFullPath,
                $"{ToSafeFileStem(runId)}-dpo-policy-checkpoints");

            var policy = policyBase.ModelSet.Policy;
            var reference = policyBase.ModelSet.Reference;
            var freshPolicy = policyBase.ModelSet.FreshLoadTarget;
            var loraPermit = AcquireBridgeMutationPermit(
                trainingLease,
                "dpo_bridge:standalone:lora_attach");
            if (!loraPermit.IsSuccess || loraPermit.Value is null)
                return loraPermit.MapError<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
            OperationResult<PreparedLoraPolicyBase> loraPreparation;
            DpoTrainer trainer;
            using (loraPermit.Value)
            {
                loraPreparation = PrepareLoraTrainingTopology(
                    policy,
                    reference,
                    freshPolicy,
                    policyBase.ModelSet.RandomContext!,
                    policyBase.ModelSet.Identity.BaseCheckpointSha256,
                    useContinuation);
                if (!loraPreparation.IsSuccess)
                    return loraPreparation.MapError<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
                var namespacePermit = AcquireBridgeMutationPermit(
                    trainingLease,
                    "dpo_bridge:standalone:artifact_namespace");
                if (!namespacePermit.IsSuccess || namespacePermit.Value is null)
                    return namespacePermit.MapError<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
                using (namespacePermit.Value)
                {
                    Directory.CreateDirectory(outputFullPath);
                    var trainerResult = CreateDpoTrainer(
                        trainingLease,
                        policy,
                        reference,
                        policyCheckpointDirectory,
                        proof => AcceptTrainerArtifactOwnership(
                            proof,
                            policyCheckpointDirectory,
                            effectiveOptions.DpoStepCount,
                            acceptedTrainerOwnership,
                            standaloneOwnedArtifacts,
                            retainedStandaloneDirectory,
                            ownershipObserver: null));
                    if (!trainerResult.IsSuccess || trainerResult.Value is null)
                        return trainerResult.MapError<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
                    // Каталог создаёт тренер по серверному разрешению. До первого
                    // шага мост удерживает открытый каталог: это позволяет при
                    // отмене удалить только подтверждённые файлы этого запуска,
                    // а не чужой файл, появившийся под тем же именем.
                    retainedStandaloneDirectory = DesignConversionHandleBoundDirectory
                        .AcquireExisting(policyCheckpointDirectory, beforeIo: null);
                    trainer = trainerResult.Value;
                }
            }
            var batches = prepared.Inputs
                .Select(static input => input.Batch)
                .ToArray();
            var actualDpoStepCountResult = ExecuteDpoStepLoop(
                trainer,
                batches,
                effectiveOptions,
                executedStepObserver: _ =>
                    PublishStandaloneMutationSinkObservation(prepared.Inputs),
                boundary => AcquireBridgeMutationPermit(trainingLease, boundary),
                "dpo_bridge:standalone:dpo_step",
                ct);
            if (!actualDpoStepCountResult.IsSuccess)
                return actualDpoStepCountResult.MapError<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
            var actualDpoStepCount = actualDpoStepCountResult.Value;
            ct.ThrowIfCancellationRequested();
            if (actualDpoStepCount <= 0)
                return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail(
                    "DesignConversion DPO policy bridge did not execute any optimizer step.",
                    "DESIGN_CONVERSION_DPO_POLICY_STEP_NOT_EXECUTED");

            if (!ReferenceMatchesSnapshot(reference, loraPreparation.Value!.ReferenceSnapshot))
                return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail(
                    "DesignConversion DPO policy reference changed during training.",
                    "DESIGN_CONVERSION_DPO_POLICY_REFERENCE_MUTATED");

            var policyCheckpointPath = Path.Combine(
                policyCheckpointDirectory,
                DpoTrainer.BuildPolicyCheckpointFileName(actualDpoStepCount));
            if (!File.Exists(policyCheckpointPath))
                return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail(
                    "DpoTrainer policy checkpoint was not written.",
                    "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_NOT_WRITTEN");

            var policyPayloadResult = DesignConversionDpoRuntimeCheckpointPublisher
                .ReadPolicyPayload(policyCheckpointPath, ct);
            if (!policyPayloadResult.IsSuccess || policyPayloadResult.Value is null)
                return policyPayloadResult.MapError<
                    DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
            var policyCheckpointPayload = policyPayloadResult.Value;
            if (policyCheckpointPayload.Length == 0)
                return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail(
                    "DpoTrainer policy checkpoint payload is empty.",
                    "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_EMPTY");

            var freshLoadPermit = AcquireBridgeMutationPermit(
                trainingLease,
                "dpo_bridge:standalone:fresh_load");
            if (!freshLoadPermit.IsSuccess || freshLoadPermit.Value is null)
                return freshLoadPermit.MapError<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
            using (freshLoadPermit.Value)
                freshPolicy.LoadCheckpoint(policyCheckpointPath);
            if (!ModelsHaveEqualParameters(policy, freshPolicy))
                return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail(
                    "DesignConversion DPO policy checkpoint fresh-load verification failed.",
                    "DESIGN_CONVERSION_DPO_POLICY_FRESH_LOAD_VERIFICATION_FAILED");

            var policyCheckpointSha256 = ComputeSha256(policyCheckpointPayload);
            var policyCheckpointFileName = Path.GetFileName(policyCheckpointPath);
            if (string.IsNullOrWhiteSpace(policyCheckpointFileName))
                return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail(
                    "DpoTrainer policy checkpoint sibling name is invalid.",
                    "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_NOT_WRITTEN");
            // Runtime inference loads the exact verified DPO policy checkpoint.
            // A synthesized checkpoint-derived LearningBatch would require a
            // distinct exact-content admission and cannot reuse this lease.
            var runtimeMachine = new DesignConversionMachine();
            var checkpoint = runtimeMachine.ExportCheckpoint();
            if (!checkpoint.IsSuccess)
                return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail(
                    checkpoint.ErrorMessage,
                    checkpoint.ErrorCode);

            var bridgeEvidenceRefs = MergeEvidenceRefs(
                BuildBridgeEvidenceRefs(
                    records.Count,
                    trainableRows.Count,
                    actualDpoStepCount,
                    policyCheckpointPath,
                    policyCheckpointSha256,
                    policyBase.ModelSet.Identity,
                    policyBase.ConfigSha256,
                    loraPreparation.Value.InitialWeightSha256,
                    loraPreparation.Value.LoraContext,
                    loraPreparation.Value.RandomContext,
                    runtimeEncodingBinding),
                legalTeacherAdmission.EvidenceRefs);
            var policyCheckpointRefResult = BuildPolicyCheckpointRef(
                records.Count,
                trainableRows.Count,
                actualDpoStepCount,
                policyCheckpointFileName,
                policyCheckpointSha256,
                policyCheckpointPayload.Length,
                config,
                policyBase.ModelSet.Identity,
                policyBase.ConfigSha256,
                loraPreparation.Value.InitialWeightSha256,
                loraPreparation.Value.LoraContext,
                loraPreparation.Value.RandomContext,
                bridgeEvidenceRefs,
                legalTeacherAdmissionCore.Value);
            if (!policyCheckpointRefResult.IsSuccess || policyCheckpointRefResult.Value is null)
                return policyCheckpointRefResult.MapError<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
            var policyCheckpointRef = policyCheckpointRefResult.Value;
            var preparedRouteBGate = DesignConversionDpoTrainerRouteBGate
                .ValidateTypedRuntimeCheckpoint(
                    policyCheckpointRef,
                    actualDpoStepCount,
                    bridgeEvidenceRefs,
                    callerFreshLoadVerified: true);
            if (!preparedRouteBGate.IsSuccess)
                return preparedRouteBGate.MapError<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
            var bridgedCheckpoint = AddDpoPolicyCheckpointEvidence(
                checkpoint.Value!,
                bridgeEvidenceRefs,
                policyCheckpointRef);
            var terminalPublicationPermit = AcquireBridgeMutationPermit(
                trainingLease,
                "dpo_bridge:standalone:terminal_publication");
            if (!terminalPublicationPermit.IsSuccess || terminalPublicationPermit.Value is null)
                return terminalPublicationPermit.MapError<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
            OperationResult<DesignConversionMachineLoadResult> published;
            using (terminalPublicationPermit.Value)
            {
                published = await PublishRuntimeTransactionWithFinalPermitAsync(
                    policyCheckpointPath,
                    policyCheckpointFileName,
                    policyCheckpointPayload,
                    runtimeCheckpointPath,
                    bridgedCheckpoint,
                    actualDpoStepCount,
                    bridgeEvidenceRefs,
                    () => AcquireBridgeMutationPermit(
                        trainingLease,
                        "dpo_bridge:standalone:terminal_publication_result"),
                    ct);
            }
            if (!published.IsSuccess || published.Value is null)
                return published.MapError<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
            var loaded = published;

            var runtimeExport = new LearningMachineExport(
                MachineId: DesignConversionMachine.MachineId,
                DomainTag: DesignConversionMachine.DomainTag,
                Version: loaded.Value!.Checkpoint.Version,
                CheckpointRef: Path.GetFileName(runtimeCheckpointPath),
                ManifestRef: $"{DesignConversionMachine.MachineId}/{loaded.Value.Checkpoint.Version}/manifest.json",
                AccuracyAtExport: loaded.Value.Checkpoint.AccuracyAfterLastBatch,
                ExportedAtUtc: loaded.Value.Checkpoint.ExportedAtUtc);

            retainStandaloneArtifacts = true;
            return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Ok(
                new DesignConversionDpoPolicyRuntimeCheckpointBridgeResult(
                    SourceRecordCount: records.Count,
                    BridgePreferenceCount: trainableRows.Count,
                    actualDpoStepCount,
                    PolicyCheckpointPath: policyCheckpointFileName,
                    PolicyCheckpointSha256: policyCheckpointSha256,
                    RuntimeCheckpointPath: Path.GetFileName(runtimeCheckpointPath),
                    RuntimeExport: runtimeExport,
                    RuntimeMachine: loaded.Value.Machine,
                    RuntimeCheckpoint: loaded.Value.Checkpoint,
                    PolicyCheckpointFreshLoadVerified: true,
                    IngredientIds: DesignConversionDpoTrainerRouteBGate.RuntimeIngredientIds(),
                    EvidenceRefs: bridgeEvidenceRefs
                        .Concat(loaded.Value.Checkpoint.EvidenceRefs)
                        .Distinct(StringComparer.Ordinal)
                        .Order(StringComparer.Ordinal)
                        .ToArray(),
                    PolicyCheckpointRef: loaded.Value.Checkpoint.DpoPolicyCheckpointRef,
                    SourceEncoderDescriptor: descriptor));
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Cancelled<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();
        }
        catch (Exception exception)
            when ((exception is IOException
                   or UnauthorizedAccessException
                   or InvalidOperationException
                   or ArgumentException) &&
                  SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            if (ct.IsCancellationRequested)
                return Cancelled<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>();

            return OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail(
                "DesignConversion DpoTrainer policy bridge failed.",
                "DESIGN_CONVERSION_DPO_POLICY_BRIDGE_FAILED");
        }
        finally
        {
            if (!retainStandaloneArtifacts)
            {
                foreach (var artifact in standaloneOwnedArtifacts.AsEnumerable().Reverse())
                    DeleteIdentityAndHashMatchedBestEffort(
                        artifact,
                        retainedStandaloneDirectory);
            }
            foreach (var artifact in standaloneOwnedArtifacts)
                artifact.Dispose();
            retainedStandaloneDirectory?.Dispose();
        }
    }

    private static Task<OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>>
        LegacyAuthorityRequired(CancellationToken ct)
        => Task.FromResult(
            ct.IsCancellationRequested
                ? OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail(
                    "DesignConversion DpoTrainer policy bridge cancelled.",
                    "CANCELLED")
                : OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>.Fail(
                    LegalTeacherConfigurationRequiredErrorMessage,
                    LegalTeacherConfigurationRequiredErrorCode));

    private static OperationResult<IDisposable> AcquireBridgeMutationPermit(
        ServerOwnedLegalTrainingTeacherAdmissionLease trainingLease,
        string boundary)
    {
        MutationBoundaryObserver.Value?.Invoke(boundary);
        return trainingLease.AcquireMutationPermit();
    }

    private static OperationResult<IDisposable>
        AcquireEvaluatedBridgeMutationPermit(
            ServerOwnedLegalTrainingTeacherAdmissionLease trainingLease,
            DesignConversionPipelineTrainingBinding? trainingBinding,
            string boundary)
    {
        if (trainingBinding is null)
            return AcquireBridgeMutationPermit(trainingLease, boundary);

        var permit = trainingBinding.AcquireMutationPermit();
        if (!permit.IsSuccess || permit.Value is null)
            return permit;

        MutationBoundaryObserver.Value?.Invoke(boundary);
        if (trainingBinding.MatchesExactBoundContent())
            return permit;

        permit.Value.Dispose();
        return CompositeExecutionInvalid<IDisposable>();
    }

    private static DesignConversionDpoPolicyContinuationRequest?
        SnapshotContinuationRequest(
            DesignConversionDpoPolicyContinuationRequest? continuationRequest)
    {
        if (continuationRequest is null ||
            continuationRequest.CheckpointSource is null ||
            continuationRequest.SourceEncoderContract is null ||
            continuationRequest.PolicyCheckpointContract is null)
        {
            return null;
        }

        return new DesignConversionDpoPolicyContinuationRequest(
            continuationRequest.CheckpointSource,
            DeepCloneBridgeValue(continuationRequest.SourceEncoderContract),
            DeepCloneBridgeValue(continuationRequest.PolicyCheckpointContract),
            continuationRequest.TokenizerArtifact is null
                ? null
                : DeepCloneBridgeValue(continuationRequest.TokenizerArtifact),
            continuationRequest.RuntimeEncodingBinding is null
                ? null
                : DeepCloneBridgeValue(continuationRequest.RuntimeEncodingBinding))
        {
            ParentCanonicalBpeTokenizerSnapshot =
                continuationRequest.ParentCanonicalBpeTokenizerSnapshot,
            CheckpointMutationAdmission =
                continuationRequest.CheckpointMutationAdmission,
            BridgeProofCheckpointSource =
                continuationRequest.BridgeProofCheckpointSource
        };
    }

    private static T DeepCloneBridgeValue<T>(T value)
        where T : class
    {
        var payload = JsonSerializer.SerializeToUtf8Bytes(
            value,
            DesignConversionCheckpointJsonOptions.Standard);
        return JsonSerializer.Deserialize<T>(
                   payload,
                   DesignConversionCheckpointJsonOptions.Standard)
               ?? throw new InvalidOperationException(
                   "DesignConversion DPO bridge value snapshot failed.");
    }

    private static IEnumerable<TrainingTeacherProvenance?>
        EnumerateConsumedTeacherProvenance(
            IReadOnlyList<DesignConversionDpoTrainingRecord> selectedTrainRows,
            IReadOnlyList<DesignConversionDpoTrainingRecord> heldOutRows,
            Action<TrainingTeacherProvenance?>? observer)
    {
        foreach (var row in selectedTrainRows)
        {
            observer?.Invoke(row.TeacherProvenance);
            yield return row.TeacherProvenance;
        }

        foreach (var row in heldOutRows)
        {
            observer?.Invoke(row.TeacherProvenance);
            yield return row.TeacherProvenance;
        }
    }

    // Обычный путь и путь с отдельно проверенным набором обучающих пар
    // используют общую проверку настроек. Это не даёт расходовать одноразовое
    // разрешение сервера на заведомо неверный запрос.
    internal static OperationResult<DesignConversionDpoPolicyCheckpointBridgeOptions> ValidateOptions(
        DesignConversionDpoPolicyCheckpointBridgeOptions? options)
    {
        var effectiveOptions = options ?? new DesignConversionDpoPolicyCheckpointBridgeOptions();
        if (effectiveOptions.MaxBridgePreferenceCount <= 0 ||
            effectiveOptions.MaxBridgePreferenceCount >
            DesignConversionDpoPolicyCheckpointBridgeOptions.MaxAllowedBridgePreferenceCount)
        {
            return OperationResult<DesignConversionDpoPolicyCheckpointBridgeOptions>.Fail(
                InvalidOptionsErrorMessage,
                InvalidOptionsErrorCode);
        }

        var validatedStepCount = LogicalDpoTrainingUnit.ValidateMaxStepCount(effectiveOptions.DpoStepCount);
        if (!validatedStepCount.IsSuccess)
        {
            return OperationResult<DesignConversionDpoPolicyCheckpointBridgeOptions>.Fail(
                InvalidOptionsErrorMessage,
                InvalidOptionsErrorCode);
        }

        if (!float.IsFinite(effectiveOptions.DpoConvergenceThreshold) ||
            effectiveOptions.DpoConvergenceThreshold < 0f ||
            effectiveOptions.DpoConvergenceWindowSteps < 1)
        {
            return OperationResult<DesignConversionDpoPolicyCheckpointBridgeOptions>.Fail(
                InvalidOptionsErrorMessage,
                InvalidOptionsErrorCode);
        }

        return OperationResult<DesignConversionDpoPolicyCheckpointBridgeOptions>.Ok(
            new DesignConversionDpoPolicyCheckpointBridgeOptions(
                effectiveOptions.MaxBridgePreferenceCount,
                effectiveOptions.DpoStepCount)
            {
                DpoConvergenceThreshold = effectiveOptions.DpoConvergenceThreshold,
                DpoConvergenceWindowSteps = effectiveOptions.DpoConvergenceWindowSteps
            });
    }

    private static bool IsBridgeProofCheckpointStream(
        Stream stream,
        long expectedByteLength)
    {
        try
        {
            return stream.CanRead &&
                   stream.CanSeek &&
                   stream.Position == 0 &&
                   expectedByteLength > 0 &&
                   expectedByteLength <= ContinuationProofMaximumCheckpointBytes &&
                   stream.Length == expectedByteLength;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return false;
        }
    }

    private static async Task<string> ComputeBridgeProofSha256Async(
        Stream stream,
        long expectedByteLength,
        CancellationToken ct)
    {
        stream.Position = 0;
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var buffer = new byte[ContinuationProofHashBufferBytes];
        long totalBytes = 0;
        while (true)
        {
            ct.ThrowIfCancellationRequested();
            var read = await stream.ReadAsync(buffer.AsMemory(), ct);
            if (read == 0)
                break;

            totalBytes = checked(totalBytes + read);
            if (totalBytes > expectedByteLength ||
                totalBytes > ContinuationProofMaximumCheckpointBytes)
            {
                throw new InvalidDataException(
                    "Continuation checkpoint exceeded its admitted proof bound.");
            }

            hash.AppendData(buffer, 0, read);
        }

        if (totalBytes != expectedByteLength)
            throw new InvalidDataException(
                "Continuation checkpoint length changed during proof.");
        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    private static void LoadBridgeOwnedContinuationCheckpoint(
        LlmModel model,
        Stream stream,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        stream.Position = 0;
        model.LoadCheckpoint(stream);
        ct.ThrowIfCancellationRequested();
    }

    private static bool ContinuationModelsHaveSameBridgeOwnedState(
        LlmModel policy,
        LlmModel reference,
        LlmModel freshLoadTarget)
    {
        if (ReferenceEquals(policy, reference) ||
            ReferenceEquals(policy, freshLoadTarget) ||
            ReferenceEquals(reference, freshLoadTarget) ||
            !DesignConversionDpoPolicyBaseMetadata.ConfigsEqual(
                policy.Config,
                reference.Config) ||
            !DesignConversionDpoPolicyBaseMetadata.ConfigsEqual(
                policy.Config,
                freshLoadTarget.Config))
        {
            return false;
        }

        var policyEntries = policy.GetCheckpointParameterEntries();
        var referenceEntries = reference.GetCheckpointParameterEntries();
        var freshEntries = freshLoadTarget.GetCheckpointParameterEntries();
        return policyEntries.Count > 0 &&
               ParametersAreExclusive(
                   policyEntries,
                   referenceEntries,
                   freshEntries) &&
               AllParameterValuesAreFinite(policyEntries) &&
               AllParameterValuesAreFinite(referenceEntries) &&
               AllParameterValuesAreFinite(freshEntries) &&
               ParameterValuesBitwiseEqual(policyEntries, referenceEntries) &&
               ParameterValuesBitwiseEqual(policyEntries, freshEntries);
    }

    private static string ComputeContinuationModelSetStateFingerprint(
        DesignConversionDpoPolicyBaseIdentity identity,
        string configSha256,
        LlmModel policy,
        LlmModel reference,
        LlmModel freshLoadTarget)
    {
        var payload = JsonSerializer.SerializeToUtf8Bytes(
            new BridgeOwnedContinuationModelSetFingerprintPayload(
                "design-conversion-dpo-continuation-model-set-state-v1",
                identity.ProviderKind,
                identity.ConfigurationKind,
                identity.BaseModelId,
                identity.BaseRevision,
                identity.BaseCheckpointSha256,
                configSha256,
                DesignConversionDpoPolicyBaseMetadata
                    .ComputeInitialWeightSha256(policy),
                DesignConversionDpoPolicyBaseMetadata
                    .ComputeInitialWeightSha256(reference),
                DesignConversionDpoPolicyBaseMetadata
                    .ComputeInitialWeightSha256(freshLoadTarget)),
            DesignConversionCheckpointJsonOptions.Standard);
        return Convert.ToHexString(SHA256.HashData(payload)).ToLowerInvariant();
    }

    private static OperationResult<ValidatedPolicyBase> ValidatePolicyBase(
        DesignConversionDpoPolicyBaseModelSet? modelSet)
    {
        if (modelSet is null ||
            modelSet.Policy is null ||
            modelSet.Reference is null ||
            modelSet.FreshLoadTarget is null)
        {
            return PolicyBaseModelSetInvalid();
        }

        if (modelSet.RandomContext is null)
        {
            return OperationResult<ValidatedPolicyBase>.Fail(
                PolicyRandomContextRequiredErrorMessage,
                PolicyRandomContextRequiredErrorCode);
        }

        if (!DesignConversionDpoPolicyRandomContextContract.IsValidCurrent(
                modelSet.RandomContext))
        {
            return OperationResult<ValidatedPolicyBase>.Fail(
                PolicyRandomContextInvalidErrorMessage,
                PolicyRandomContextInvalidErrorCode);
        }

        if (!DesignConversionDpoPolicyBaseMetadata.IsValidIdentity(modelSet.Identity))
            return OperationResult<ValidatedPolicyBase>.Fail(
                "DesignConversion DPO policy base identity is invalid.",
                "DESIGN_CONVERSION_DPO_POLICY_BASE_IDENTITY_INVALID");

        try
        {
            if (ReferenceEquals(modelSet.Policy, modelSet.Reference) ||
                ReferenceEquals(modelSet.Policy, modelSet.FreshLoadTarget) ||
                ReferenceEquals(modelSet.Reference, modelSet.FreshLoadTarget) ||
                !DesignConversionDpoPolicyBaseMetadata.IsValidConfig(modelSet.Policy.Config) ||
                !DesignConversionDpoPolicyBaseMetadata.ConfigsEqual(modelSet.Policy.Config, modelSet.Reference.Config) ||
                !DesignConversionDpoPolicyBaseMetadata.ConfigsEqual(modelSet.Policy.Config, modelSet.FreshLoadTarget.Config))
            {
                return PolicyBaseModelSetInvalid();
            }

            var policyEntries = modelSet.Policy.GetCheckpointParameterEntries();
            var referenceEntries = modelSet.Reference.GetCheckpointParameterEntries();
            var freshEntries = modelSet.FreshLoadTarget.GetCheckpointParameterEntries();
            if (policyEntries.Count == 0 ||
                !ParameterTopologiesMatch(policyEntries, referenceEntries) ||
                !ParameterTopologiesMatch(policyEntries, freshEntries) ||
                !ParametersAreExclusive(policyEntries, referenceEntries, freshEntries) ||
                !AllParameterValuesAreFinite(policyEntries) ||
                !AllParameterValuesAreFinite(referenceEntries) ||
                !AllParameterValuesAreFinite(freshEntries) ||
                !ParameterValuesBitwiseEqual(policyEntries, referenceEntries))
            {
                return PolicyBaseModelSetInvalid();
            }
            return OperationResult<ValidatedPolicyBase>.Ok(new ValidatedPolicyBase(
                modelSet,
                DesignConversionDpoPolicyBaseMetadata.ComputeConfigSha256(
                    modelSet.Policy.Config)));
        }
        catch (Exception exception)
            when ((exception is InvalidOperationException
                   or ArgumentException
                   or OverflowException) &&
                  SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return PolicyBaseModelSetInvalid();
        }
    }

    private static OperationResult<ValidatedPolicyBase>
        ValidatePolicyBaseAgainstPreparation(
            DesignConversionDpoPolicyBaseModelSet? modelSet,
            ValidatedProviderPreparation preparation)
    {
        var validated = ValidatePolicyBase(modelSet);
        if (!validated.IsSuccess || validated.Value is null)
            return validated;
        var value = validated.Value;
        if (modelSet is null ||
            modelSet.Identity != preparation.ExpectedIdentity ||
            !DesignConversionDpoPolicyBaseMetadata.ConfigsEqual(
                modelSet.Policy.Config,
                preparation.Config) ||
            !string.Equals(
                value.ConfigSha256,
                preparation.ConfigSha256,
                StringComparison.Ordinal))
        {
            return PolicyBaseModelSetInvalid();
        }

        if (preparation.RuntimeEncodingBinding is null)
        {
            if (modelSet.RuntimeEncodingBinding is not null)
                return PolicyBaseModelSetInvalid();
        }
        else if (modelSet.RuntimeEncodingBinding !=
                 preparation.RuntimeEncodingBinding)
        {
            return ContinuationRuntimeEncodingInvalid<ValidatedPolicyBase>();
        }

        if (preparation.ContinuationMutationProof is not null)
        {
            try
            {
                // Provider здесь не предъявляет authority вообще. Bridge
                // сравнивает фактические mutable веса с состоянием, которое сам
                // загрузил из admitted checkpoint A до provider/trainer surface.
                var actualFingerprint =
                    ComputeContinuationModelSetStateFingerprint(
                        preparation.ExpectedIdentity,
                        preparation.ConfigSha256,
                        modelSet.Policy,
                        modelSet.Reference,
                        modelSet.FreshLoadTarget);
                ContinuationMutationProofObserver.Value?.Invoke(
                    new DesignConversionDpoContinuationMutationProofObservation(
                        "actual",
                        preparation.ContinuationMutationProof
                            .ExpectedModelSetStateFingerprint,
                        actualFingerprint));
                if (!string.Equals(
                        actualFingerprint,
                        preparation.ContinuationMutationProof
                            .ExpectedModelSetStateFingerprint,
                        StringComparison.Ordinal))
                {
                    return DesignConversionDpoCheckpointMutationAdmission
                        .Mismatch<ValidatedPolicyBase>();
                }
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return DesignConversionDpoCheckpointMutationAdmission
                    .Mismatch<ValidatedPolicyBase>();
            }
        }

        return validated;
    }

    private static bool ParameterTopologiesMatch(
        IReadOnlyList<LlmCheckpointParameterEntry> expected,
        IReadOnlyList<LlmCheckpointParameterEntry> actual)
    {
        if (expected.Count != actual.Count)
            return false;

        for (var index = 0; index < expected.Count; index++)
        {
            if (!string.Equals(expected[index].Key, actual[index].Key, StringComparison.Ordinal) ||
                !expected[index].Parameter.Value.Shape.Dimensions.SequenceEqual(
                    actual[index].Parameter.Value.Shape.Dimensions))
            {
                return false;
            }
        }

        return true;
    }

    private static bool ParametersAreExclusive(
        params IReadOnlyList<LlmCheckpointParameterEntry>[] modelEntries)
    {
        var parameters = new HashSet<object>(ReferenceEqualityComparer.Instance);
        var tensors = new HashSet<object>(ReferenceEqualityComparer.Instance);
        foreach (var entries in modelEntries)
        {
            foreach (var entry in entries)
            {
                if (!parameters.Add(entry.Parameter) ||
                    !tensors.Add(entry.Parameter.Value) ||
                    !tensors.Add(entry.Parameter.Value.Storage))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static bool AllParameterValuesAreFinite(IReadOnlyList<LlmCheckpointParameterEntry> entries)
        => entries.All(entry => entry.Parameter.Value.Data.All(float.IsFinite));

    private static bool ParameterValuesBitwiseEqual(
        IReadOnlyList<LlmCheckpointParameterEntry> expected,
        IReadOnlyList<LlmCheckpointParameterEntry> actual)
    {
        if (!ParameterTopologiesMatch(expected, actual))
            return false;

        for (var entryIndex = 0; entryIndex < expected.Count; entryIndex++)
        {
            var expectedValues = expected[entryIndex].Parameter.Value.Data;
            var actualValues = actual[entryIndex].Parameter.Value.Data;
            if (expectedValues.Length != actualValues.Length)
                return false;
            for (var valueIndex = 0; valueIndex < expectedValues.Length; valueIndex++)
            {
                if (BitConverter.SingleToInt32Bits(expectedValues[valueIndex]) !=
                    BitConverter.SingleToInt32Bits(actualValues[valueIndex]))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static OperationResult<PreparedLoraPolicyBase> PrepareLoraTrainingTopology(
        LlmModel policy,
        LlmModel reference,
        LlmModel freshLoadTarget,
        DesignConversionDpoPolicyRandomContext randomContext,
        string baseCheckpointSha256,
        bool useContinuation)
    {
        try
        {
            IReadOnlyList<LoraLinear> policyAdapters;
            IReadOnlyList<LoraLinear> referenceAdapters;
            IReadOnlyList<LoraLinear> freshAdapters;
            if (useContinuation)
            {
                policyAdapters = DesignConversionDpoPolicyBaseMetadata
                    .ValidateSeededCanonicalLoraTopology(
                        policy,
                        randomContext,
                        DesignConversionDpoPolicyModelRole.Policy);
                referenceAdapters = DesignConversionDpoPolicyBaseMetadata
                    .ValidateSeededCanonicalLoraTopology(
                        reference,
                        randomContext,
                        DesignConversionDpoPolicyModelRole.Reference);
                freshAdapters = DesignConversionDpoPolicyBaseMetadata
                    .ValidateSeededCanonicalLoraTopology(
                        freshLoadTarget,
                        randomContext,
                        DesignConversionDpoPolicyModelRole.FreshLoad);
            }
            else
            {
                DesignConversionDpoPolicyBaseMetadata
                    .ValidateSeededCanonicalLoraFreshApplication(
                        policy,
                        randomContext,
                        DesignConversionDpoPolicyModelRole.Policy);
                DesignConversionDpoPolicyBaseMetadata
                    .ValidateSeededCanonicalLoraFreshApplication(
                        reference,
                        randomContext,
                        DesignConversionDpoPolicyModelRole.Reference);
                DesignConversionDpoPolicyBaseMetadata
                    .ValidateSeededCanonicalLoraFreshApplication(
                        freshLoadTarget,
                        randomContext,
                        DesignConversionDpoPolicyModelRole.FreshLoad);

                var application = LoraApplicator.ApplyFreshSeededTriple(
                    policy,
                    reference,
                    freshLoadTarget,
                    DesignConversionDpoPolicyBaseMetadata.CreateCanonicalLoraConfig(),
                    randomContext.RootInitializationSeed,
                    randomContext.PolicyDropoutStreamDomain,
                    randomContext.ReferenceDropoutStreamDomain,
                    randomContext.FreshLoadDropoutStreamDomain);
                policyAdapters = application.PolicyModelAdapters;
                referenceAdapters = application.ReferenceModelAdapters;
                freshAdapters = application.FreshLoadModelAdapters;
            }
            if (policyAdapters.Count == 0 ||
                policyAdapters.Count != referenceAdapters.Count ||
                policyAdapters.Count != freshAdapters.Count ||
                !AdapterInitializationMatricesBitwiseEqual(
                    policyAdapters,
                    referenceAdapters,
                    freshAdapters))
            {
                return OperationResult<PreparedLoraPolicyBase>.Fail(
                    "DesignConversion DPO policy LoRA topology attached no live adapters.",
                    "DESIGN_CONVERSION_DPO_POLICY_LORA_APPLICATION_FAILED");
            }

            if (!CopyFullModelState(policy, reference, freezeDestination: true) ||
                !CopyFullModelState(policy, freshLoadTarget, freezeDestination: false))
            {
                return OperationResult<PreparedLoraPolicyBase>.Fail(
                    "DesignConversion DPO policy LoRA topology could not be synchronized.",
                    "DESIGN_CONVERSION_DPO_POLICY_LORA_STATE_SYNC_FAILED");
            }

            var referenceSnapshot = SnapshotParameters(reference);
            if (!ReferenceMatchesSnapshot(reference, referenceSnapshot) ||
                !ModelsHaveEqualParameters(policy, reference) ||
                !ModelsHaveEqualParameters(policy, freshLoadTarget))
            {
                return OperationResult<PreparedLoraPolicyBase>.Fail(
                    "DesignConversion DPO policy LoRA topology was not frozen at step zero.",
                    "DESIGN_CONVERSION_DPO_POLICY_LORA_REFERENCE_FREEZE_FAILED");
            }

            var initialWeightSha256 =
                DesignConversionDpoPolicyBaseMetadata.ComputeInitialWeightSha256(reference);
            if (string.Equals(
                    baseCheckpointSha256,
                    initialWeightSha256,
                    StringComparison.Ordinal))
            {
                return OperationResult<PreparedLoraPolicyBase>.Fail(
                    "DesignConversion DPO policy base model set is invalid or incompatible.",
                    "DESIGN_CONVERSION_DPO_POLICY_BASE_MODEL_SET_INVALID");
            }
            var loraContext =
                DesignConversionDpoPolicyBaseMetadata.CreateCanonicalLoraContext(
                    policy.Config,
                    policyAdapters.Count);

            return OperationResult<PreparedLoraPolicyBase>.Ok(new PreparedLoraPolicyBase(
                referenceSnapshot,
                initialWeightSha256,
                loraContext,
                randomContext));
        }
        catch (Exception exception)
            when ((exception is InvalidOperationException
                   or ArgumentException
                   or OverflowException) &&
                  SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<PreparedLoraPolicyBase>.Fail(
                "DesignConversion DPO policy LoRA topology could not be prepared.",
                "DESIGN_CONVERSION_DPO_POLICY_LORA_APPLICATION_FAILED");
        }
    }

    private static bool AdapterInitializationMatricesBitwiseEqual(
        params IReadOnlyList<LoraLinear>[] adapterSets)
    {
        if (adapterSets.Length < 2 || adapterSets.Any(set => set.Count != adapterSets[0].Count))
            return false;

        for (var adapterIndex = 0; adapterIndex < adapterSets[0].Count; adapterIndex++)
        {
            var expected = adapterSets[0][adapterIndex].AdapterParameters
                .Single(parameter => string.Equals(
                    parameter.Suffix,
                    "lora_a",
                    StringComparison.Ordinal))
                .Parameter.Value;
            for (var setIndex = 1; setIndex < adapterSets.Length; setIndex++)
            {
                var actual = adapterSets[setIndex][adapterIndex].AdapterParameters
                    .Single(parameter => string.Equals(
                        parameter.Suffix,
                        "lora_a",
                        StringComparison.Ordinal))
                    .Parameter.Value;
                if (!expected.Shape.Dimensions.SequenceEqual(actual.Shape.Dimensions) ||
                    expected.Data.Length != actual.Data.Length)
                {
                    return false;
                }

                for (var valueIndex = 0; valueIndex < expected.Data.Length; valueIndex++)
                {
                    if (BitConverter.SingleToInt32Bits(expected.Data[valueIndex]) !=
                        BitConverter.SingleToInt32Bits(actual.Data[valueIndex]))
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    private static bool CopyFullModelState(
        LlmModel source,
        LlmModel destination,
        bool freezeDestination)
    {
        var sourceEntries = source.GetCheckpointParameterEntries();
        var destinationEntries = destination.GetCheckpointParameterEntries();
        if (!ParameterTopologiesMatch(sourceEntries, destinationEntries))
            return false;

        for (var entryIndex = 0; entryIndex < sourceEntries.Count; entryIndex++)
        {
            var sourceParameter = sourceEntries[entryIndex].Parameter;
            var destinationParameter = destinationEntries[entryIndex].Parameter;
            if (ReferenceEquals(sourceParameter, destinationParameter) ||
                ReferenceEquals(sourceParameter.Value, destinationParameter.Value) ||
                ReferenceEquals(sourceParameter.Value.Storage, destinationParameter.Value.Storage) ||
                ReferenceEquals(sourceParameter.Value.Data, destinationParameter.Value.Data))
            {
                return false;
            }

            var sourceData = sourceParameter.Value.Data;
            var destinationData = destinationParameter.Value.Data;
            if (sourceData.Length != destinationData.Length)
                return false;

            Array.Copy(sourceData, destinationData, sourceData.Length);
            if (freezeDestination)
            {
                destinationParameter.Value.RequiresGrad = false;
                destinationParameter.Value.Grad = null;
            }
        }

        return AllParameterValuesAreFinite(destinationEntries);
    }

    private static IReadOnlyList<ParameterSnapshot> SnapshotParameters(LlmModel model)
        => model.GetCheckpointParameterEntries()
            .Select(entry => new ParameterSnapshot(
                entry.Key,
                entry.Parameter.Value.Shape.Dimensions.ToArray(),
                entry.Parameter.Value.Data
                    .Select(BitConverter.SingleToInt32Bits)
                    .ToArray()))
            .ToArray();

    private static bool ReferenceMatchesSnapshot(
        LlmModel reference,
        IReadOnlyList<ParameterSnapshot> snapshot)
    {
        var entries = reference.GetCheckpointParameterEntries();
        if (entries.Count != snapshot.Count)
            return false;

        for (var entryIndex = 0; entryIndex < entries.Count; entryIndex++)
        {
            var entry = entries[entryIndex];
            var expected = snapshot[entryIndex];
            if (entry.Parameter.Value.RequiresGrad ||
                !string.Equals(entry.Key, expected.Key, StringComparison.Ordinal) ||
                !entry.Parameter.Value.Shape.Dimensions.SequenceEqual(expected.Shape) ||
                entry.Parameter.Value.Data.Length != expected.ValueBits.Count)
            {
                return false;
            }

            for (var valueIndex = 0; valueIndex < expected.ValueBits.Count; valueIndex++)
            {
                if (BitConverter.SingleToInt32Bits(entry.Parameter.Value.Data[valueIndex]) !=
                    expected.ValueBits[valueIndex])
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static bool ModelsHaveEqualParameters(LlmModel expected, LlmModel actual)
        => ParameterValuesBitwiseEqual(
            expected.GetCheckpointParameterEntries(),
            actual.GetCheckpointParameterEntries());

    private static OperationResult<ValidatedPolicyBase> PolicyBaseModelSetInvalid()
        => OperationResult<ValidatedPolicyBase>.Fail(
            "DesignConversion DPO policy base model set is invalid or incompatible.",
            "DESIGN_CONVERSION_DPO_POLICY_BASE_MODEL_SET_INVALID");

    private static OperationResult<T> PolicyBaseProviderRequired<T>()
        => OperationResult<T>.Fail(
            "DesignConversion DPO policy base provider is required.",
            "DESIGN_CONVERSION_DPO_POLICY_BASE_PROVIDER_REQUIRED");

    private static OperationResult<T> ContinuationProviderRequired<T>()
        => OperationResult<T>.Fail(
            ContinuationProviderRequiredErrorMessage,
            ContinuationProviderRequiredErrorCode);

    private static OperationResult<T> ContinuationContractInvalid<T>()
        => OperationResult<T>.Fail(
            "The DesignConversion DPO continuation contract is invalid.",
            DesignConversionCheckpointBackedDpoPolicyBaseProvider
                .ContinuationContractInvalidErrorCode);

    private static OperationResult<T> ContinuationCheckpointMismatch<T>()
        => OperationResult<T>.Fail(
            "The DesignConversion DPO continuation checkpoint does not match its contract.",
            DesignConversionCheckpointBackedDpoPolicyBaseProvider
                .ContinuationCheckpointMismatchErrorCode);

    private static OperationResult<T> ContinuationLoraInvalid<T>()
        => OperationResult<T>.Fail(
            "The DesignConversion DPO continuation LoRA topology is invalid.",
            DesignConversionCheckpointBackedDpoPolicyBaseProvider
                .ContinuationLoraInvalidErrorCode);

    private static OperationResult<T> ContinuationRuntimeEncodingInvalid<T>()
        => OperationResult<T>.Fail(
            ContinuationRuntimeEncodingInvalidErrorMessage,
            ContinuationRuntimeEncodingInvalidErrorCode);

    private static bool IsStableContinuationError(string errorCode)
        => string.Equals(
               errorCode,
               DesignConversionCheckpointBackedDpoPolicyBaseProvider
                   .ContinuationContractInvalidErrorCode,
               StringComparison.Ordinal) ||
           string.Equals(
               errorCode,
               DesignConversionCheckpointBackedDpoPolicyBaseProvider
                   .ContinuationCheckpointMismatchErrorCode,
               StringComparison.Ordinal) ||
           string.Equals(
               errorCode,
               DesignConversionCheckpointBackedDpoPolicyBaseProvider
                   .ContinuationLoraInvalidErrorCode,
               StringComparison.Ordinal) ||
           string.Equals(
               errorCode,
               DesignConversionCheckpointBackedDpoPolicyBaseProvider
                   .ContinuationRandomContextMismatchErrorCode,
               StringComparison.Ordinal) ||
           string.Equals(
               errorCode,
               DesignConversionDpoCheckpointMutationAdmission
                   .AttestationRequiredErrorCode,
               StringComparison.Ordinal) ||
           string.Equals(
               errorCode,
               DesignConversionDpoCheckpointMutationAdmission
                   .LegacyQuarantinedErrorCode,
               StringComparison.Ordinal) ||
           string.Equals(
               errorCode,
               DesignConversionDpoCheckpointMutationAdmission
                   .MismatchErrorCode,
               StringComparison.Ordinal) ||
           string.Equals(
               errorCode,
               ContinuationRuntimeEncodingInvalidErrorCode,
               StringComparison.Ordinal);

    private static OperationResult<T> StableContinuationFailure<T>(string errorCode)
        => errorCode switch
        {
            DesignConversionCheckpointBackedDpoPolicyBaseProvider
                .ContinuationContractInvalidErrorCode
                => ContinuationContractInvalid<T>(),
            DesignConversionCheckpointBackedDpoPolicyBaseProvider
                .ContinuationRandomContextMismatchErrorCode
                => ContinuationRandomContextMismatch<T>(),
            DesignConversionDpoCheckpointMutationAdmission
                .AttestationRequiredErrorCode
                => DesignConversionDpoCheckpointMutationAdmission.Required<T>(),
            DesignConversionDpoCheckpointMutationAdmission
                .LegacyQuarantinedErrorCode
                => DesignConversionDpoCheckpointMutationAdmission.Legacy<T>(),
            DesignConversionDpoCheckpointMutationAdmission
                .MismatchErrorCode
                => DesignConversionDpoCheckpointMutationAdmission.Mismatch<T>(),
            DesignConversionCheckpointBackedDpoPolicyBaseProvider
                .ContinuationLoraInvalidErrorCode
                => ContinuationLoraInvalid<T>(),
            ContinuationRuntimeEncodingInvalidErrorCode
                => ContinuationRuntimeEncodingInvalid<T>(),
            _ => ContinuationCheckpointMismatch<T>()
        };

    private static OperationResult<T> ContinuationRandomContextMismatch<T>()
        => OperationResult<T>.Fail(
            "The DesignConversion DPO continuation random context is invalid.",
            DesignConversionCheckpointBackedDpoPolicyBaseProvider
                .ContinuationRandomContextMismatchErrorCode);

    private static OperationResult<T> Cancelled<T>()
        => OperationResult<T>.Fail(
            "DesignConversion DpoTrainer policy bridge cancelled.",
            "CANCELLED");

    private sealed record ParameterSnapshot(
        string Key,
        IReadOnlyList<int> Shape,
        IReadOnlyList<int> ValueBits);

    private sealed record PreparedLoraPolicyBase(
        IReadOnlyList<ParameterSnapshot> ReferenceSnapshot,
        string InitialWeightSha256,
        DesignConversionDpoPolicyCheckpointLoraContext LoraContext,
        DesignConversionDpoPolicyRandomContext RandomContext);

    private sealed record FinalVectorOptimizerInput(
        DpoVectorBatch Batch,
        string Prompt,
        string Chosen,
        string Rejected,
        TrainingTeacherProvenance? Provenance,
        string Context)
    {
        // Подготовленный standalone input держит ожидаемых авторов вне
        // DpoBatch.Admission, чтобы поймать удаление или замену до StepVector.
        internal DpoPairSideAuthorKind? ChosenAuthorKind { get; init; }

        // Ожидаемый автор rejected хранится явно: переворот направления не
        // должен прятаться за тем же prompt/chosen/rejected текстом.
        internal DpoPairSideAuthorKind? RejectedAuthorKind { get; init; }
    }

    private sealed record PreparedStandaloneOptimizer(
        ValidatedProviderPreparation ProviderPreparation,
        DesignConversionDpoSourceEncoderDescriptor Descriptor,
        DesignConversionDpoRuntimeEncodingBinding? RuntimeEncodingBinding,
        IReadOnlyList<FinalVectorOptimizerInput> Inputs);

    internal sealed record PreparedEvaluatedOptimizer(
        ValidatedProviderPreparation ProviderPreparation,
        DesignConversionDpoSourceEncoderDescriptor Descriptor,
        DesignConversionDpoRuntimeEncodingBinding? RuntimeEncodingBinding,
        IReadOnlyList<DesignConversionDpoEvaluatedPackedBatch> TrainPacked,
        IReadOnlyList<DesignConversionDpoEvaluatedPackedBatch> HeldOutPacked,
        string TrainInputSha256,
        string HeldOutInputSha256);

    internal sealed record PreparedEvaluatedCompositeExecution(
        DesignConversionDpoConsumedPartitionProjection ConsumedPartition,
        DesignConversionDpoPolicyCheckpointBridgeOptions EffectiveOptions,
        DesignConversionDpoPolicyContinuationRequest? ContinuationRequest,
        bool UseContinuation,
        PreparedEvaluatedOptimizer Optimizer,
        DesignConversionFinalOptimizerBatch<DesignConversionDpoEvaluatedPackedBatch>
            FinalOptimizer,
        LegalTrainingTeacherAdmission HeldOutLegalAdmission);

    internal sealed record ValidatedPolicyBase(
        DesignConversionDpoPolicyBaseModelSet ModelSet,
        string ConfigSha256);

    internal sealed record ValidatedProviderPreparation(
        IDesignConversionDpoPolicyBaseProvider Provider,
        DesignConversionDpoPolicyBasePreparationDescriptor Descriptor,
        DesignConversionDpoPolicyBaseIdentity ExpectedIdentity,
        ModelConfig Config,
        string ConfigSha256,
        DesignConversionDpoRuntimeEncodingBinding? RuntimeEncodingBinding,
        BridgeOwnedContinuationMutationProof? ContinuationMutationProof);

    internal sealed record BridgeOwnedContinuationMutationProof(
        string PolicyCheckpointSha256,
        long PolicyCheckpointByteLength,
        string PolicyCheckpointFileName,
        string ExpectedModelSetStateFingerprint);

    private sealed record BridgeOwnedContinuationModelSetFingerprintPayload(
        string Schema,
        string ProviderKind,
        string ConfigurationKind,
        string BaseModelId,
        string BaseRevision,
        string BaseCheckpointSha256,
        string ConfigSha256,
        string PolicyStateSha256,
        string ReferenceStateSha256,
        string FreshLoadStateSha256);

    private static bool IsTrainableRow(DesignConversionDpoTrainingRecord row)
        => row is not null &&
           row.License.AllowsTraining &&
           !row.License.ContainsUserPrivateData &&
           !row.Quarantine.IsQuarantined;

    private static OperationResult<bool>
        ValidateEvaluatedConsumedPartitionTrainability(
            DesignConversionDpoConsumedPartitionProjection? consumedPartition)
    {
        if (consumedPartition is null ||
            consumedPartition.TrainRecords is null ||
            consumedPartition.TrainRecords.Count == 0 ||
            consumedPartition.TrainRecords.Any(row => !IsTrainableRow(row)) ||
            consumedPartition.HeldOutRecords is null ||
            consumedPartition.HeldOutRecords.Any(row => !IsTrainableRow(row)))
        {
            return EvaluatedPackingInvalid<bool>();
        }

        return OperationResult<bool>.Ok(true);
    }

    private static DesignConversionDpoTrainingRecord[] SelectTrainableRows(
        IReadOnlyList<DesignConversionDpoTrainingRecord> records,
        int maximumCount)
        => records
            .Where(IsTrainableRow)
            .OrderByDescending(row => row.UsefulnessScore)
            .ThenBy(row => row.RecordId, StringComparer.Ordinal)
            .Take(maximumCount)
            .ToArray();

    internal static OperationResult<DesignConversionDpoSourceEncoderDescriptor>
        BuildSourceEncoderDescriptorPreviewForTests(
            IReadOnlyList<DesignConversionDpoTrainingRecord> records,
            ModelConfig config,
            DesignConversionDpoPolicyCheckpointBridgeOptions? options = null)
    {
        var validated = ValidateOptions(options);
        if (!validated.IsSuccess)
            return validated.MapError<DesignConversionDpoSourceEncoderDescriptor>();
        var selected = SelectTrainableRows(
            records,
            validated.Value!.MaxBridgePreferenceCount);
        return DesignConversionDpoSourceEncoderContractBuilder.BuildDescriptor(
            selected,
            config);
    }

    private const double LocalAblationScoreDeltaThreshold = 0.10;
    private const string LocalAblationGateFailureErrorCode = "DESIGN_CONVERSION_DPO_POLICY_BRIDGE_LOCAL_ABLATION_GATE_FAILED";

    private static BridgeLocalAblationQualityGate EvaluateLocalAblationQualityGate(
        IReadOnlyList<DesignConversionDpoTrainingRecord> rows)
    {
        if (rows.Count == 0)
        {
            return new BridgeLocalAblationQualityGate(
                EvaluatedRecordCount: 0,
                PreferredScore: 0,
                BaselineScore: 0,
                ScoreDelta: 0,
                Threshold: LocalAblationScoreDeltaThreshold,
                PassesQualityGate: false,
                GateReason: "no_trainable_records_for_quality_gate");
        }

        var preferredScore = Math.Round(rows.Average(row => row.Chosen.Verdict.Score), 6);
        var baselineScore = Math.Round(rows.Average(row => row.Rejected.Verdict.Score), 6);
        var scoreDelta = Math.Round(preferredScore - baselineScore, 6);
        var reasons = new List<string>();

        if (rows.Any(row => !IsUsefulGateScore(row.Chosen.Verdict.Score) || !IsUsefulGateScore(row.Rejected.Verdict.Score)) ||
            !IsUsefulGateScore(preferredScore) ||
            !IsUsefulGateScore(baselineScore) ||
            !IsFiniteGateValue(scoreDelta))
        {
            reasons.Add("local_ablation_score_out_of_range_or_non_finite");
        }

        if (rows.Any(row => row.Chosen.Verdict.Score <= row.Rejected.Verdict.Score))
            reasons.Add("preferred_score_not_above_baseline");
        if (!IsFiniteGateValue(scoreDelta) || scoreDelta < LocalAblationScoreDeltaThreshold)
            reasons.Add($"score_delta_below_threshold:{Format(scoreDelta)}<{Format(LocalAblationScoreDeltaThreshold)}");
        if (rows.Any(row => !row.Chosen.Accepted || row.Rejected.Accepted))
            reasons.Add("preferred_rejected_acceptance_flags_not_separated");
        if (rows.Any(row => !row.Chosen.Verdict.Passed || row.Rejected.Verdict.Passed))
            reasons.Add("preferred_rejected_verdicts_not_separated");

        var passes = reasons.Count == 0;
        return new BridgeLocalAblationQualityGate(
            EvaluatedRecordCount: rows.Count,
            PreferredScore: preferredScore,
            BaselineScore: baselineScore,
            ScoreDelta: scoreDelta,
            Threshold: LocalAblationScoreDeltaThreshold,
            PassesQualityGate: passes,
            GateReason: passes
                ? "local_ablation_quality_gate_passed"
                : string.Join(";", reasons.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)));
    }

    private static bool IsUsefulGateScore(double value)
        => IsFiniteGateValue(value) && value is >= 0 and <= 1;

    private static bool IsFiniteGateValue(double value)
        => !double.IsNaN(value) && !double.IsInfinity(value);

    private static string Format(double value)
        => value.ToString("0.######", CultureInfo.InvariantCulture);

    private static OperationResult<DpoTrainer> CreateDpoTrainer(
        ServerOwnedLegalTrainingTeacherAdmissionLease trainingLease,
        LlmModel policy,
        LlmModel reference,
        string policyCheckpointDirectory,
        Func<DpoTrainerArtifactOwnershipProof, bool>? ownershipObserver = null)
    {
        var mutationAdmission =
            trainingLease.IssueDpoTrainerMutationAdmissionCapability();
        if (!mutationAdmission.IsSuccess || mutationAdmission.Value is null)
            return mutationAdmission.MapError<DpoTrainer>();
        return OperationResult<DpoTrainer>.Ok(new DpoTrainer(
                policy,
                reference,
                policyCheckpointDirectory,
                mutationAdmission.Value,
                1e-5f,
                ownershipObserver is null
                    ? null
                    : new DpoTrainerPublicationObserver
                    {
                        ArtifactOwnershipProven = proof =>
                        {
                            if (proof.Kind is DpoTrainerArtifactKind.ReservationLock
                                or DpoTrainerArtifactKind.PolicyStaging
                                or DpoTrainerArtifactKind.MetricsStaging)
                            {
                                // Служебный файл блокировки тоже создан тренером
                                // во временном каталоге. На Linux закрытие само
                                // его не удаляет, поэтому очистка принимает файл
                                // только после проверки удержанного дескриптора.
                                ownershipObserver(proof);
                            }
                        },
                        TryAcceptArtifactOwnership = ownershipObserver
                    }));
    }

    // Сообщает, может ли эта настройка вообще сократить прогон по сходимости.
    // Монитор впервые отвечает "пора остановиться" только на третьем
    // наблюдении, а цикл ниже спрашивает его раз в `convergenceWindowSteps`
    // шагов, поэтому самый ранний возможный выход — шаг 3 * интервал. Когда
    // бюджет шагов не больше трёх интервалов наблюдения, этот выход либо
    // совпадает с последним шагом, либо не достигается вовсе, и цикл всё равно
    // проходит весь бюджет: на ровной потере первый бюджет с настоящим
    // сокращением равен 3 * интервал + 1 для интервалов 1, 2, 7, 25 и 331.
    // Порог 0 выключает проверку намеренно и недостижимостью не считается;
    // интервал меньше 1 уже отклоняет ValidateOptions.
    internal static bool ConvergenceDetectionIsUnreachable(
        int dpoStepCount,
        int convergenceWindowSteps,
        float convergenceThreshold)
        => convergenceThreshold != 0f &&
           convergenceWindowSteps >= 1 &&
           dpoStepCount <= 3L * convergenceWindowSteps;

    // Выдаёт предупреждение ровно для недостижимых настроек и молчит для
    // достижимых. Отказа настройке не выдаём: заведомо короткие конфигурации
    // должны продолжать работать как раньше, но перестать делать это молча.
    internal static void TraceUnreachableConvergenceDetection(
        int dpoStepCount,
        int convergenceWindowSteps,
        float convergenceThreshold)
    {
        // Достижимая или намеренно выключенная проверка не нуждается в сигнале.
        if (!ConvergenceDetectionIsUnreachable(
                dpoStepCount,
                convergenceWindowSteps,
                convergenceThreshold))
        {
            return;
        }

        // Один человекочитаемый след с обоими числами, по которым видно, почему
        // ранняя остановка в этом прогоне невозможна.
        Trace.TraceWarning(
            "DesignConversion DPO convergence detection is unreachable for this run: " +
            "step budget {0} does not exceed three observation intervals of {1} steps " +
            "(threshold {2}); the loop always runs the full step budget.",
            dpoStepCount.ToString(CultureInfo.InvariantCulture),
            convergenceWindowSteps.ToString(CultureInfo.InvariantCulture),
            convergenceThreshold.ToString("0.######", CultureInfo.InvariantCulture));
    }

    private static OperationResult<int> ExecuteDpoStepLoop(
        DpoTrainer trainer,
        IReadOnlyList<DpoVectorBatch> batches,
        DesignConversionDpoPolicyCheckpointBridgeOptions effectiveOptions,
        Action<int>? executedStepObserver,
        Func<string, OperationResult<IDisposable>> permitFactory,
        string permitBoundaryPrefix,
        CancellationToken ct)
    {
        var convergenceWindowSteps = effectiveOptions.DpoConvergenceWindowSteps;
        var convergenceMonitor = effectiveOptions.DpoConvergenceThreshold == 0f
            ? null
            : new DpoConvergenceMonitor(
                effectiveOptions.DpoConvergenceThreshold,
                convergenceWindowSteps);
        // Молча выключенная проверка сходимости — худший исход: прогон выглядит
        // настроенным на раннюю остановку, а на самом деле всегда доходит до
        // конца бюджета шагов. Поэтому недостижимость один раз проговаривается
        // в журнал диагностики перед первым шагом.
        TraceUnreachableConvergenceDetection(
            effectiveOptions.DpoStepCount,
            convergenceWindowSteps,
            effectiveOptions.DpoConvergenceThreshold);
        var actualDpoStepCount = 0;
        // FIX-MEASURER-03: признак того, что о пропуске проверки сходимости из-за
        // отсутствующей немасштабированной потери уже сказано. Один след за
        // прогон, а не по одному на каждый момент наблюдения.
        var unscaledLossUnavailableReported = false;
        for (var step = 1; step <= effectiveOptions.DpoStepCount; step++)
        {
            ct.ThrowIfCancellationRequested();
            var stepPermit = permitFactory($"{permitBoundaryPrefix}:{step}");
            if (!stepPermit.IsSuccess || stepPermit.Value is null)
                return stepPermit.MapError<int>();
            DpoMetrics metrics;
            using (stepPermit.Value)
            {
                metrics = trainer.StepVector(batches, step);
                actualDpoStepCount = step;
                executedStepObserver?.Invoke(step);
            }
            if (convergenceMonitor is not null &&
                step % convergenceWindowSteps == 0)
            {
                // FIX-MEASURER-03: сторож сравнивает с НЕПОДВИЖНЫМ порогом только
                // немасштабированную потерю. Обычное поле `Loss` в векторном пути
                // домножается на плотность обучающего вектора на ветви точных пар,
                // а награды, запас и доля верных — нет. Поэтому прежде порог
                // зависел от плотности вектора, то есть от свойства данных, а не
                // от хода обучения. Новое поле копится построчно до
                // индивидуального масштабирования и на обеих ветвях одинаково.
                // Порядок условий этого цикла сохранён как был: сначала монитор,
                // затем момент наблюдения, и лишь потом сам вопрос — он отличается
                // от порядка у соседнего потребителя, и выравнивать его эта правка
                // не бралась.
                var comparableLoss = metrics.UnscaledLoss;
                if (comparableLoss is null)
                {
                    // Пустое поле означает «не считалось», а НЕ ноль. Остановка по
                    // несравнимому числу хуже, чем проход объявленного бюджета,
                    // поэтому решение названо вслух один раз за прогон и прогон
                    // продолжается. Молчаливого нуля здесь нет.
                    if (!unscaledLossUnavailableReported)
                    {
                        unscaledLossUnavailableReported = true;
                        Trace.TraceWarning(
                            "DesignConversion DPO convergence check is skipped for this run: " +
                            "the step receipt carries no unscaled loss, so there is no value " +
                            "comparable to the fixed threshold. An absent value is not zero, " +
                            "and stopping on an incomparable number would be worse than running " +
                            "the declared budget, so the loop runs the full step budget of {0} " +
                            "steps instead.",
                            effectiveOptions.DpoStepCount.ToString(CultureInfo.InvariantCulture));
                    }
                }
                else if (convergenceMonitor.ShouldStop(comparableLoss.Value))
                {
                    break;
                }
            }
        }

        return OperationResult<int>.Ok(actualDpoStepCount);
    }

    private static OperationResult<DesignConversionDpoRuntimeEncodingBinding>
        ValidateContinuationRuntimeEncodingForPacking(
            DesignConversionDpoPolicyBaseModelSet modelSet,
            DesignConversionDpoSourceEncoderDescriptor descriptor,
            ModelConfig config)
    {
        var binding = modelSet.RuntimeEncodingBinding;
        if (binding is null ||
            !DesignConversionDpoPromotionArtifactContractCodec
                .RuntimeEncodingBindingMatchesDescriptor(binding, descriptor))
        {
            return ContinuationRuntimeEncodingInvalid<
                DesignConversionDpoRuntimeEncodingBinding>();
        }

        return ValidateRuntimeInferenceEncodingBinding(binding, config);
    }

    private static OperationResult<DesignConversionDpoRuntimeEncodingBinding>
        ValidateContinuationRuntimeEncodingForPacking(
            DesignConversionDpoRuntimeEncodingBinding? binding,
            DesignConversionDpoSourceEncoderDescriptor descriptor,
            ModelConfig config)
    {
        if (binding is null ||
            !DesignConversionDpoPromotionArtifactContractCodec
                .RuntimeEncodingBindingMatchesDescriptor(binding, descriptor))
        {
            return ContinuationRuntimeEncodingInvalid<
                DesignConversionDpoRuntimeEncodingBinding>();
        }
        return ValidateRuntimeInferenceEncodingBinding(binding, config);
    }

    private static OperationResult<DesignConversionDpoRuntimeEncodingBinding>
        ValidateRuntimeInferenceEncodingBinding(
            DesignConversionDpoRuntimeEncodingBinding? runtimeEncodingBinding,
            ModelConfig? config)
    {
        if (runtimeEncodingBinding is null ||
            !IsRuntimeInferenceEncodingBindingValid(
                runtimeEncodingBinding.BindingSchema,
                runtimeEncodingBinding.SourceDescriptorSchema,
                runtimeEncodingBinding.PolicyTokenMapping,
                runtimeEncodingBinding.TokenIdMinimum,
                runtimeEncodingBinding.TokenIdMaximum,
                runtimeEncodingBinding.ContextLength,
                runtimeEncodingBinding.TokenizerArtifactSha256,
                runtimeEncodingBinding.TokenizerVocabularyIdentitySha256,
                config))
        {
            return ContinuationRuntimeEncodingInvalid<
                DesignConversionDpoRuntimeEncodingBinding>();
        }

        return OperationResult<DesignConversionDpoRuntimeEncodingBinding>.Ok(
            runtimeEncodingBinding);
    }

    internal static OperationResult<DesignConversionDpoRuntimeInferenceEncodingBinding>
        ValidateRuntimeInferenceEncodingBinding(
            DesignConversionDpoRuntimeInferenceEncodingBinding? runtimeEncodingBinding,
            ModelConfig? config)
    {
        if (runtimeEncodingBinding is null ||
            !IsRuntimeInferenceEncodingBindingValid(
                runtimeEncodingBinding.BindingSchema,
                runtimeEncodingBinding.SourceDescriptorSchema,
                runtimeEncodingBinding.PolicyTokenMapping,
                runtimeEncodingBinding.TokenIdMinimum,
                runtimeEncodingBinding.TokenIdMaximum,
                runtimeEncodingBinding.ContextLength,
                runtimeEncodingBinding.TokenizerArtifactSha256,
                runtimeEncodingBinding.TokenizerVocabularyIdentitySha256,
                config))
        {
            return ContinuationRuntimeEncodingInvalid<
                DesignConversionDpoRuntimeInferenceEncodingBinding>();
        }

        return OperationResult<DesignConversionDpoRuntimeInferenceEncodingBinding>.Ok(
            runtimeEncodingBinding);
    }

    private static bool IsRuntimeInferenceEncodingBindingValid(
        string? bindingSchema,
        string? sourceDescriptorSchema,
        string? policyTokenMapping,
        int tokenIdMinimum,
        int tokenIdMaximum,
        int contextLength,
        string? tokenizerArtifactSha256,
        string? tokenizerVocabularyIdentitySha256,
        ModelConfig? config)
    {
        if (config is null ||
            !string.Equals(
                bindingSchema,
                DesignConversionDpoPromotionArtifactContractCodec
                    .RuntimeEncodingBindingSchema,
                StringComparison.Ordinal))
        {
            return false;
        }

        var expected = BuildSupportedRuntimeInferenceEncodingBinding(
            sourceDescriptorSchema,
            config);
        return expected.IsSuccess &&
               expected.Value is { } value &&
               string.Equals(
                   policyTokenMapping,
                   value.PolicyTokenMapping,
                   StringComparison.Ordinal) &&
               tokenIdMinimum == value.TokenIdMinimum &&
               tokenIdMaximum == value.TokenIdMaximum &&
               contextLength == value.ContextLength &&
               string.Equals(
                   tokenizerArtifactSha256,
                   value.TokenizerArtifactSha256,
                   StringComparison.Ordinal) &&
               string.Equals(
                   tokenizerVocabularyIdentitySha256,
                   value.TokenizerVocabularyIdentitySha256,
                   StringComparison.Ordinal);
    }

    internal static OperationResult<DesignConversionDpoRuntimeInferenceEncodingBinding>
        BuildRuntimeInferenceEncodingBindingPreviewForTests(
            ModelConfig config,
            bool evaluated)
        => BuildRuntimeInferenceEncodingBinding(config, evaluated);

    internal static OperationResult<DesignConversionDpoRuntimeInferenceEncodingBinding>
        BuildRuntimeInferenceEncodingBinding(
            ModelConfig config,
            bool evaluated)
        => BuildSupportedRuntimeInferenceEncodingBinding(
            evaluated
                ? DesignConversionDpoSourceEncoderContractBuilder
                    .EvaluatedDescriptorSchema
                : DesignConversionDpoSourceEncoderContractBuilder
                    .DescriptorSchema,
            config);

    private static OperationResult<DesignConversionDpoRuntimeInferenceEncodingBinding>
        BuildSupportedRuntimeInferenceEncodingBinding(
            string? sourceDescriptorSchema,
            ModelConfig config)
    {
        var evaluated = string.Equals(
            sourceDescriptorSchema,
            DesignConversionDpoSourceEncoderContractBuilder
                .EvaluatedDescriptorSchema,
            StringComparison.Ordinal);
        var legacy = string.Equals(
            sourceDescriptorSchema,
            DesignConversionDpoSourceEncoderContractBuilder.DescriptorSchema,
            StringComparison.Ordinal);
        if ((!evaluated && !legacy) ||
            config.ContextLen < (evaluated ? 2 : 1) ||
            config.VocabSize < (evaluated ? 256 : 2))
        {
            return ContinuationRuntimeEncodingInvalid<
                DesignConversionDpoRuntimeInferenceEncodingBinding>();
        }

        var tokenIdMaximum = evaluated ? 255 : config.VocabSize - 1;
        var descriptor = new DesignConversionDpoSourceEncoderDescriptor(
            ContractSchema: sourceDescriptorSchema!,
            EncoderName: string.Empty,
            EncoderSchema: string.Empty,
            EncoderRole: string.Empty,
            EncoderAuthority: string.Empty,
            FeatureNamespace: string.Empty,
            FeatureNames: string.Empty,
            FeatureMapHash: string.Empty,
            ShortcutGuard: string.Empty,
            VectorDimension: 0,
            BridgePreferenceCount: 1,
            SourceSignalSetSha256: string.Empty,
            PolicyTextEncoding:
                DesignConversionDpoSourceEncoderContractBuilder.PolicyTextEncoding,
            PolicyTokenMapping: evaluated
                ? DesignConversionDpoSourceEncoderContractBuilder
                    .EvaluatedPolicyTokenMapping
                : DesignConversionDpoSourceEncoderContractBuilder
                    .PolicyTokenMapping,
            PolicyContextTruncation: evaluated
                ? DesignConversionDpoSourceEncoderContractBuilder
                    .EvaluatedPolicyContextTruncation
                : DesignConversionDpoSourceEncoderContractBuilder
                    .PolicyContextTruncation,
            PolicyTrainingPadding: evaluated
                ? DesignConversionDpoSourceEncoderContractBuilder
                    .EvaluatedPolicyTrainingPadding
                : DesignConversionDpoSourceEncoderContractBuilder
                    .PolicyTrainingPadding,
            TokenIdMinimum: 0,
            TokenIdMaximum: tokenIdMaximum,
            ContextLength: config.ContextLen,
            TokenizerArtifactPresent: true,
            RawPayloadPersisted: false,
            AbsolutePathPersisted: false);
        var tokenizerArtifact =
            DesignConversionDpoPromotionArtifactContractCodec
                .BuildCanonicalTokenizerArtifact(descriptor);
        if (!tokenizerArtifact.IsSuccess || tokenizerArtifact.Value is null)
        {
            return ContinuationRuntimeEncodingInvalid<
                DesignConversionDpoRuntimeInferenceEncodingBinding>();
        }

        var tokenizerArtifactBytes = JsonSerializer.SerializeToUtf8Bytes(
            tokenizerArtifact.Value,
            RuntimeInferenceCanonicalJsonOptions);
        return OperationResult<
            DesignConversionDpoRuntimeInferenceEncodingBinding>.Ok(
                new DesignConversionDpoRuntimeInferenceEncodingBinding(
                    DesignConversionDpoPromotionArtifactContractCodec
                        .RuntimeEncodingBindingSchema,
                    sourceDescriptorSchema!,
                    descriptor.PolicyTokenMapping,
                    descriptor.TokenIdMinimum,
                    descriptor.TokenIdMaximum,
                    descriptor.ContextLength,
                    Convert.ToHexString(SHA256.HashData(tokenizerArtifactBytes))
                        .ToLowerInvariant(),
                    tokenizerArtifact.Value.VocabularyIdentitySha256));
    }

    private static OperationResult<DpoVectorBatch> BuildVectorBatch(
        DesignConversionDpoTrainingRecord row,
        ModelConfig config)
    {
        var batch = BuildBatch(row, config);
        if (!batch.IsSuccess || batch.Value is null)
            return batch.MapError<DpoVectorBatch>();

        return OperationResult<DpoVectorBatch>.Ok(new DpoVectorBatch(
            batch.Value,
            row.InternalVectorPayload.Count == 0
                ? [1f]
                : row.InternalVectorPayload
                    .Take(config.ContextLen)
                    .Select(value => (float)Math.Clamp(value, -2.0, 2.0))
                    .ToArray(),
            Weight: Math.Max(0.01f, (float)Math.Clamp(
                row.UsefulnessScore,
                0.01,
                1.0))));
    }

    private static OperationResult<DpoVectorBatch> BuildVectorBatch(
        DesignConversionDpoTrainingRecord row,
        DesignConversionDpoRuntimeEncodingBinding runtimeEncodingBinding)
    {
        var batch = BuildBatch(row, runtimeEncodingBinding);
        if (!batch.IsSuccess || batch.Value is null)
            return batch.MapError<DpoVectorBatch>();

        return OperationResult<DpoVectorBatch>.Ok(new DpoVectorBatch(
            batch.Value,
            row.InternalVectorPayload.Count == 0
                ? [1f]
                : row.InternalVectorPayload
                    .Take(runtimeEncodingBinding.ContextLength)
                    .Select(value => (float)Math.Clamp(value, -2.0, 2.0))
                    .ToArray(),
            Weight: Math.Max(0.01f, (float)Math.Clamp(
                row.UsefulnessScore,
                0.01,
                1.0))));
    }

    internal static OperationResult<DpoVectorBatch>
        BuildStandaloneVectorBatchPreviewForTests(
            DesignConversionDpoTrainingRecord row,
            ModelConfig config)
        => BuildVectorBatch(row, config);

    internal static OperationResult<DpoBatch>
        BuildStandaloneBatchPreviewForTests(
            DesignConversionDpoTrainingRecord row,
            ModelConfig config)
        => BuildBatch(row, config);

    internal const string EvaluatedPackingId =
        "design-conversion-dpo-pair-aware-packing-v1";
    internal const string LegacyEvaluatedSchema =
        "design-conversion-dpo-heldout-evaluation-v1";
    internal const string PreviousEvaluatedSchema =
        "design-conversion-dpo-heldout-evaluation-v2";
    internal const string EvaluatedSchema =
        "design-conversion-dpo-heldout-evaluation-v3";
    internal const string EvaluatedPackingInvalidErrorCode =
        "DESIGN_CONVERSION_DPO_EVALUATED_PACKING_INVALID";
    internal const string EvaluatedInputFingerprintInvalidErrorCode =
        "DESIGN_CONVERSION_DPO_EVALUATED_INPUT_FINGERPRINT_INVALID";
    internal static HeldOutScorerIdentity EvaluatedMeasurementNoiseScorerIdentity { get; } =
        new(
            DomainTag: DesignConversionMachine.DomainTag,
            ScorerId: "design-conversion-dpo-heldout-g3-scorer",
            ScorerVersion: EvaluatedSchema,
            NormalizationProfileId: EvaluatedPackingId,
            MetricProfileId: "baseline-win-rate");

    internal static OperationResult<DesignConversionDpoEvaluatedPackedBatch>
        BuildEvaluatedPackedBatchPreviewForTests(
            DesignConversionDpoTrainingRecord row,
            ModelConfig config,
            bool includeTrainingVectorAndWeight = true)
        => BuildEvaluatedPackedBatch(row, config, includeTrainingVectorAndWeight);

    internal static OperationResult<DesignConversionDpoEvaluatedPackedBatch>
        BuildEvaluatedPackedUtf8PreviewForTests(
            DesignConversionDpoTrainingRecord row,
            ModelConfig config,
            string prompt,
            string chosenResponse,
            string rejectedResponse)
        => BuildEvaluatedPackedBatch(
            row,
            config,
            includeTrainingVectorAndWeight: false,
            Encoding.UTF8.GetBytes(prompt),
            Encoding.UTF8.GetBytes(chosenResponse),
            Encoding.UTF8.GetBytes(rejectedResponse));

    internal static OperationResult<DpoBatch>
        BuildRuntimePolicyPairBatchForInference(
            string prompt,
            string chosenContinuation,
            string rejectedContinuation,
            DesignConversionDpoRuntimeInferenceEncodingBinding runtimeEncodingBinding,
            ModelConfig config)
    {
        var validated = ValidateRuntimeInferenceEncodingBinding(
            runtimeEncodingBinding,
            config);
        if (!validated.IsSuccess ||
            validated.Value is null ||
            !string.Equals(
                validated.Value.SourceDescriptorSchema,
                DesignConversionDpoSourceEncoderContractBuilder
                    .EvaluatedDescriptorSchema,
                StringComparison.Ordinal))
        {
            return ContinuationRuntimeEncodingInvalid<DpoBatch>();
        }

        var batch = TryBuildPairAwareFixedContextBatch(
            Encoding.UTF8.GetBytes(prompt ?? string.Empty),
            Encoding.UTF8.GetBytes(chosenContinuation ?? string.Empty),
            Encoding.UTF8.GetBytes(rejectedContinuation ?? string.Empty),
            validated.Value.ContextLength,
            validated.Value.TokenIdMinimum,
            validated.Value.TokenIdMaximum);
        return batch is null
            ? ContinuationRuntimeEncodingInvalid<DpoBatch>()
            : OperationResult<DpoBatch>.Ok(batch);
    }

    private static OperationResult<DesignConversionDpoEvaluatedPackedBatch>
        BuildEvaluatedPackedBatch(
            DesignConversionDpoTrainingRecord? row,
            ModelConfig? config,
            bool includeTrainingVectorAndWeight,
            byte[]? promptOverride = null,
            byte[]? chosenOverride = null,
            byte[]? rejectedOverride = null,
            DesignConversionDpoRuntimeEncodingBinding? runtimeEncodingBinding = null)
    {
        if (row is null || config is null || config.ContextLen < 2 ||
            config.VocabSize < 256 || !IsDpoEvaluatedRecordUsable(row))
        {
            return EvaluatedPackingInvalid<DesignConversionDpoEvaluatedPackedBatch>();
        }

        var promptText = promptOverride is null
            ? BuildPolicyPrompt(row)
            : Encoding.UTF8.GetString(promptOverride);
        var chosenText = chosenOverride is null
            ? BuildEvaluatedResponse(row.Chosen)
            : Encoding.UTF8.GetString(chosenOverride);
        var rejectedText = rejectedOverride is null
            ? BuildEvaluatedResponse(row.Rejected)
            : Encoding.UTF8.GetString(rejectedOverride);
        var promptBytes = promptOverride ?? Encoding.UTF8.GetBytes(promptText);
        var chosenBytes = chosenOverride ?? Encoding.UTF8.GetBytes(chosenText);
        var rejectedBytes = rejectedOverride ?? Encoding.UTF8.GetBytes(rejectedText);
        if (chosenBytes.SequenceEqual(rejectedBytes))
            return EvaluatedPackingInvalid<DesignConversionDpoEvaluatedPackedBatch>();

        var contextLength = runtimeEncodingBinding?.ContextLength ?? config.ContextLen;
        var tokenIdMinimum = runtimeEncodingBinding?.TokenIdMinimum ?? 0;
        var tokenIdMaximum = runtimeEncodingBinding?.TokenIdMaximum ?? config.VocabSize - 1;
        if (contextLength < 2 ||
            tokenIdMinimum < 0 ||
            tokenIdMaximum < tokenIdMinimum ||
            tokenIdMaximum >= config.VocabSize)
        {
            return EvaluatedPackingInvalid<DesignConversionDpoEvaluatedPackedBatch>();
        }

        var pairBatch = TryBuildPairAwareFixedContextBatch(
            promptBytes,
            chosenBytes,
            rejectedBytes,
            contextLength,
            tokenIdMinimum,
            tokenIdMaximum);
        if (pairBatch is null)
            return EvaluatedPackingInvalid<DesignConversionDpoEvaluatedPackedBatch>();
        if (includeTrainingVectorAndWeight)
        {
            var admission = CreateDpoTrainerPairAdmission(
                row,
                promptText,
                chosenText,
                rejectedText);
            if (!admission.IsSuccess || admission.Value is null)
                return admission.MapError<DesignConversionDpoEvaluatedPackedBatch>();

            // Evaluated training допускает ровно raw evaluated triple; held-out
            // packing использует те же tensors, но без паспорта trainer.
            pairBatch = pairBatch with { Admission = admission.Value };
        }

        var vectorBatch = new DpoVectorBatch(
            pairBatch,
            row.InternalVectorPayload.Count == 0
                ? [1f]
                : row.InternalVectorPayload
                    .Take(contextLength)
                    .Select(value => (float)Math.Clamp(value, -2.0, 2.0))
                    .ToArray(),
            Math.Max(0.01f, (float)Math.Clamp(row.UsefulnessScore, 0.01, 1.0)));
        var recordIdentitySha256 = HashText($"record|{row.RecordId}|{row.UnitId}");
        var inputSha256 = RecomputeEvaluatedInputFingerprint(
            [row],
            [vectorBatch],
            config,
            includeTrainingVectorAndWeight);
        if (string.IsNullOrEmpty(inputSha256))
            return EvaluatedPackingInvalid<DesignConversionDpoEvaluatedPackedBatch>();
        return OperationResult<DesignConversionDpoEvaluatedPackedBatch>.Ok(
            new DesignConversionDpoEvaluatedPackedBatch(
                vectorBatch,
                recordIdentitySha256,
                inputSha256,
                promptText,
                chosenText,
                rejectedText,
                row.TeacherProvenance)
            {
                ChosenAuthorKind = row.ChosenAuthorKind,
                RejectedAuthorKind = row.RejectedAuthorKind
            });
    }

    private static DpoBatch? TryBuildPairAwareFixedContextBatch(
        byte[] promptBytes,
        byte[] chosenBytes,
        byte[] rejectedBytes,
        int contextLength,
        int tokenIdMinimum,
        int tokenIdMaximum)
    {
        if (contextLength < 2 ||
            tokenIdMinimum < 0 ||
            tokenIdMaximum < tokenIdMinimum ||
            chosenBytes.SequenceEqual(rejectedBytes))
        {
            return null;
        }

        var promptCapacity = Math.Max(1, contextLength / 3);
        var responseCapacity = contextLength - promptCapacity;
        if (responseCapacity <= 0)
            return null;

        var sharedPrefixLength = 0;
        while (sharedPrefixLength < chosenBytes.Length &&
               sharedPrefixLength < rejectedBytes.Length &&
               chosenBytes[sharedPrefixLength] == rejectedBytes[sharedPrefixLength])
        {
            sharedPrefixLength++;
        }

        var windowStart = Math.Max(0, sharedPrefixLength - responseCapacity + 1);
        var chosenWindow = chosenBytes.Skip(windowStart).Take(responseCapacity).ToArray();
        var rejectedWindow = rejectedBytes.Skip(windowStart).Take(responseCapacity).ToArray();
        var strictPrefixDivergence = sharedPrefixLength ==
                                     Math.Min(chosenBytes.Length, rejectedBytes.Length) &&
                                     chosenBytes.Length != rejectedBytes.Length;
        if (chosenWindow.Length == 0 && rejectedWindow.Length == 0 ||
            chosenWindow.SequenceEqual(rejectedWindow) && !strictPrefixDivergence)
        {
            return null;
        }

        var promptWindow = promptBytes.Take(promptCapacity).ToArray();
        var chosenIds = new int[contextLength];
        var rejectedIds = new int[contextLength];
        var chosenMask = new float[contextLength];
        var rejectedMask = new float[contextLength];
        for (var index = 0; index < promptWindow.Length; index++)
        {
            chosenIds[index] = Math.Clamp(
                (int)promptWindow[index],
                tokenIdMinimum,
                tokenIdMaximum);
            rejectedIds[index] = Math.Clamp(
                (int)promptWindow[index],
                tokenIdMinimum,
                tokenIdMaximum);
        }
        for (var index = 0; index < chosenWindow.Length; index++)
        {
            chosenIds[promptCapacity + index] = Math.Clamp(
                (int)chosenWindow[index],
                tokenIdMinimum,
                tokenIdMaximum);
            chosenMask[promptCapacity + index] = 1f;
        }
        for (var index = 0; index < rejectedWindow.Length; index++)
        {
            rejectedIds[promptCapacity + index] = Math.Clamp(
                (int)rejectedWindow[index],
                tokenIdMinimum,
                tokenIdMaximum);
            rejectedMask[promptCapacity + index] = 1f;
        }

        if (chosenIds.SequenceEqual(rejectedIds) &&
            chosenMask.SequenceEqual(rejectedMask))
        {
            return null;
        }

        return new DpoBatch
        {
            ChosenIds = new Tensor(
                chosenIds.Select(static value => (float)value).ToArray(),
                new TensorShape(1, contextLength)),
            ChosenMask = new Tensor(
                chosenMask,
                new TensorShape(1, contextLength)),
            RejectedIds = new Tensor(
                rejectedIds.Select(static value => (float)value).ToArray(),
                new TensorShape(1, contextLength)),
            RejectedMask = new Tensor(
                rejectedMask,
                new TensorShape(1, contextLength))
        };
    }

    internal static string RecomputeEvaluatedInputFingerprint(
        IReadOnlyList<DesignConversionDpoTrainingRecord>? rows,
        IReadOnlyList<DpoVectorBatch>? batches,
        ModelConfig? config,
        bool includeTrainingVectorAndWeight)
    {
        if (rows is null || batches is null || config is null ||
            rows.Count == 0 || rows.Count != batches.Count ||
            config.ContextLen < 2 || config.VocabSize < 256)
            return string.Empty;

        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        AppendEvaluatedString(hash, "design-conversion-dpo-evaluated-input-fingerprint-v2");
        AppendEvaluatedString(hash, EvaluatedSchema);
        AppendEvaluatedString(hash, EvaluatedPackingId);
        AppendEvaluatedString(hash, DesignConversionDpoPolicyBaseMetadata.ComputeConfigSha256(config));
        AppendEvaluatedInt32(hash, includeTrainingVectorAndWeight ? 1 : 0);
        AppendEvaluatedInt32(hash, rows.Count);
        for (var ordinal = 0; ordinal < rows.Count; ordinal++)
        {
            var row = rows[ordinal];
            var batch = batches[ordinal];
            if (row is null || batch?.Batch is null)
                return string.Empty;
            AppendEvaluatedInt32(hash, ordinal);
            AppendEvaluatedString(hash, HashText($"record|{row.RecordId}|{row.UnitId}"));
            AppendEvaluatedString(hash, ComputeCanonicalEvaluatedRowSha256(row));
            AppendEvaluatedTensor(hash, batch.Batch.ChosenIds);
            AppendEvaluatedTensor(hash, batch.Batch.ChosenMask);
            AppendEvaluatedTensor(hash, batch.Batch.RejectedIds);
            AppendEvaluatedTensor(hash, batch.Batch.RejectedMask);
            if (includeTrainingVectorAndWeight)
            {
                AppendEvaluatedInt32(hash, batch.TeachingVector.Length);
                foreach (var value in batch.TeachingVector)
                    AppendEvaluatedInt32(hash, BitConverter.SingleToInt32Bits(value));
                AppendEvaluatedInt32(hash, BitConverter.SingleToInt32Bits(batch.Weight));
            }
        }
        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    private static string ComputeCanonicalEvaluatedRowSha256(
        DesignConversionDpoTrainingRecord row)
        => Convert.ToHexString(SHA256.HashData(
            JsonSerializer.SerializeToUtf8Bytes(
                row,
                DesignConversionCheckpointJsonOptions.Standard))).ToLowerInvariant();

    private static OperationResult<IReadOnlyList<DesignConversionDpoEvaluatedPackedBatch>>
        BuildEvaluatedPackedBatches(
            IReadOnlyList<DesignConversionDpoTrainingRecord> rows,
            ModelConfig config,
            bool includeTrainingVectorAndWeight,
            DesignConversionDpoRuntimeEncodingBinding? runtimeEncodingBinding = null)
    {
        var packed = new DesignConversionDpoEvaluatedPackedBatch[rows.Count];
        for (var index = 0; index < rows.Count; index++)
        {
            var item = BuildEvaluatedPackedBatch(
                rows[index],
                config,
                includeTrainingVectorAndWeight,
                runtimeEncodingBinding: runtimeEncodingBinding);
            if (!item.IsSuccess || item.Value is null)
                return item.MapError<IReadOnlyList<DesignConversionDpoEvaluatedPackedBatch>>();
            packed[index] = item.Value;
        }
        return OperationResult<IReadOnlyList<DesignConversionDpoEvaluatedPackedBatch>>.Ok(packed);
    }

    private static float EvaluateObserved(
        LlmModel model,
        Tensor ids,
        Tensor mask,
        DesignConversionDpoEvaluatedPackedBatch packed,
        DesignConversionDpoEvaluatedForwardPhase before,
        DesignConversionDpoEvaluatedForwardPhase after,
        Action<DesignConversionDpoEvaluatedForwardObservation>? observer,
        CancellationToken ct)
    {
        observer?.Invoke(new DesignConversionDpoEvaluatedForwardObservation(
            before, packed.RecordIdentitySha256, packed.InputSha256));
        ct.ThrowIfCancellationRequested();
        var result = DpoTrainer.EvaluateAverageNextTokenLogProbability(
            model, ids, mask, ct);
        observer?.Invoke(new DesignConversionDpoEvaluatedForwardObservation(
            after, packed.RecordIdentitySha256, packed.InputSha256));
        ct.ThrowIfCancellationRequested();
        return result;
    }

    internal static OperationResult<DesignConversionDpoEvaluatedMetricSummary>
        ComputeEvaluatedMetricSummaryForTests(
            IReadOnlyList<(float ChildChosen, float ChildRejected,
                float ReferenceChosen, float ReferenceRejected)> scores)
        => ComputeEvaluatedMetricSummary(scores);

    private static OperationResult<DesignConversionDpoEvaluatedMetricSummary>
        ComputeEvaluatedMetricSummary(
            IReadOnlyList<(float ChildChosen, float ChildRejected,
                float ReferenceChosen, float ReferenceRejected)>? scores)
    {
        if (scores is null || scores.Count == 0)
            return EvaluatedNonFinite<DesignConversionDpoEvaluatedMetricSummary>();
        var childMargins = new double[scores.Count];
        var referenceMargins = new double[scores.Count];
        var relativeMargins = new double[scores.Count];
        var correctCount = 0;
        var absoluteChildPreferenceCorrectCount = 0;
        for (var index = 0; index < scores.Count; index++)
        {
            var score = scores[index];
            if (!float.IsFinite(score.ChildChosen) ||
                !float.IsFinite(score.ChildRejected) ||
                !float.IsFinite(score.ReferenceChosen) ||
                !float.IsFinite(score.ReferenceRejected))
            {
                return EvaluatedNonFinite<DesignConversionDpoEvaluatedMetricSummary>();
            }
            var childMargin = (double)score.ChildChosen - score.ChildRejected;
            var referenceMargin = (double)score.ReferenceChosen - score.ReferenceRejected;
            var relativeMargin = childMargin - referenceMargin;
            if (!double.IsFinite(childMargin) || !double.IsFinite(referenceMargin) ||
                !double.IsFinite(relativeMargin))
            {
                return EvaluatedNonFinite<DesignConversionDpoEvaluatedMetricSummary>();
            }
            childMargins[index] = childMargin;
            referenceMargins[index] = referenceMargin;
            relativeMargins[index] = relativeMargin;
            if (relativeMargin > 0d)
                correctCount++;
            if (childMargin > 0d)
                absoluteChildPreferenceCorrectCount++;
        }

        var result = new DesignConversionDpoEvaluatedMetricSummary(
            correctCount,
            (double)correctCount / scores.Count,
            absoluteChildPreferenceCorrectCount,
            (double)absoluteChildPreferenceCorrectCount / scores.Count,
            relativeMargins.Average(),
            childMargins.Average(),
            referenceMargins.Average());
        return double.IsFinite(result.Accuracy) &&
               double.IsFinite(result.AbsoluteChildPreferenceAccuracy) &&
               double.IsFinite(result.MeanRelativeMargin) &&
               double.IsFinite(result.MeanChildMargin) &&
               double.IsFinite(result.MeanReferenceMargin)
            ? OperationResult<DesignConversionDpoEvaluatedMetricSummary>.Ok(result)
            : EvaluatedNonFinite<DesignConversionDpoEvaluatedMetricSummary>();
    }

    private static bool EvaluatedMetricsAreFinite(
        DesignConversionDpoHeldOutEvaluation evaluation)
        => double.IsFinite(evaluation.Accuracy) &&
           evaluation.G3Metrics is { } g3Metrics &&
           DesignConversionG3AcceptancePolicy.MetricsAreValid(g3Metrics) &&
           DesignConversionG3AcceptancePolicy.MeasurementNoiseScopeMatches(
               g3Metrics,
               EvaluatedMeasurementNoiseScorerIdentity,
               evaluation.DatasetId) &&
           double.IsFinite(evaluation.MeanRelativeMargin) &&
           double.IsFinite(evaluation.MeanChildMargin) &&
           double.IsFinite(evaluation.MeanReferenceMargin) &&
           EvaluatedMeasurementNoiseIsValid(evaluation);

    private static bool EvaluatedMeasurementNoiseIsValid(
        DesignConversionDpoHeldOutEvaluation evaluation)
    {
        if (evaluation.MeasurementNoisePassport is null)
        {
            return string.Equals(
                evaluation.MeasurementNoiseStatus,
                MeasurementNoisePolicy.DifferenceNotProvenNoCalibrationStatus,
                StringComparison.Ordinal);
        }

        if (!MeasurementNoisePolicy.TryValidatePassportForMeasurement(
                evaluation.MeasurementNoisePassport,
                EvaluatedMeasurementNoiseScorerIdentity,
                evaluation.DatasetId,
                evaluation.EvaluatedHeldOutRecordCount,
                evaluation.Accuracy,
                out _))
        {
            return false;
        }

        // Bridge не принимает свободный status из артефакта: он должен
        // совпасть с тем, что паспорт даёт для текущей канонической G3-дельты.
        var expected = MeasurementNoisePolicy.ClassifyDifference(
            evaluation.G3Metrics!.BaselineWinRate -
            DesignConversionG3AcceptancePolicyConfig.Canonical.MinimumBaselineWinRate,
            evaluation.MeasurementNoisePassport);
        return string.Equals(
            evaluation.MeasurementNoiseStatus,
            expected.Status,
            StringComparison.Ordinal);
    }

    private static OperationResult<DesignConversionDpoHeldOutCycleAccuracyLineage>
        BuildCycleAccuracyLineage(
            DesignConversionDpoHeldOutEvaluation evaluation,
            DesignConversionDpoPolicyContinuationRequest? continuationRequest,
            bool useContinuation)
    {
        if (!useContinuation)
        {
            return OperationResult<DesignConversionDpoHeldOutCycleAccuracyLineage>.Ok(
                new DesignConversionDpoHeldOutCycleAccuracyLineage(
                    CycleOrdinal: 1,
                    CurrentMeasuredAccuracy: evaluation.Accuracy,
                    HasParentMeasurement: false,
                    ParentCycleOrdinal: null,
                    ParentMeasuredAccuracy: null,
                    ParentPolicyCheckpointSha256: null,
                    ParentDatasetId: null,
                    ParentHeldOutRecordSetSha256: null,
                    ParentHeldOutSourceSetSha256: null,
                    ParentHeldOutEvaluationInputSha256: null,
                    ComparableHeldOut: false,
                    AccuracyDelta: null));
        }

        if (!TryReadParentCycleOrdinal(
                continuationRequest,
                out var parentEvaluation,
                out var parentCycleOrdinal) ||
            !string.Equals(
                continuationRequest!.PolicyCheckpointContract.Binding.PolicyCheckpointSha256,
                evaluation.ReferencePolicyCheckpointSha256,
                StringComparison.Ordinal))
        {
            return ContinuationContractInvalid<
                DesignConversionDpoHeldOutCycleAccuracyLineage>();
        }

        int cycleOrdinal;
        try
        {
            cycleOrdinal = checked(parentCycleOrdinal + 1);
        }
        catch (OverflowException)
        {
            return ContinuationContractInvalid<
                DesignConversionDpoHeldOutCycleAccuracyLineage>();
        }

        var comparableHeldOut =
            string.Equals(parentEvaluation.DatasetId, evaluation.DatasetId, StringComparison.Ordinal) &&
            string.Equals(parentEvaluation.HeldOutRecordSetSha256, evaluation.HeldOutRecordSetSha256, StringComparison.Ordinal) &&
            string.Equals(parentEvaluation.HeldOutSourceSetSha256, evaluation.HeldOutSourceSetSha256, StringComparison.Ordinal) &&
            string.Equals(parentEvaluation.HeldOutEvaluationInputSha256, evaluation.HeldOutEvaluationInputSha256, StringComparison.Ordinal);
        return OperationResult<DesignConversionDpoHeldOutCycleAccuracyLineage>.Ok(
            new DesignConversionDpoHeldOutCycleAccuracyLineage(
                CycleOrdinal: cycleOrdinal,
                CurrentMeasuredAccuracy: evaluation.Accuracy,
                HasParentMeasurement: true,
                ParentCycleOrdinal: parentCycleOrdinal,
                ParentMeasuredAccuracy: parentEvaluation.Accuracy,
                ParentPolicyCheckpointSha256: evaluation.ReferencePolicyCheckpointSha256,
                ParentDatasetId: parentEvaluation.DatasetId,
                ParentHeldOutRecordSetSha256: parentEvaluation.HeldOutRecordSetSha256,
                ParentHeldOutSourceSetSha256: parentEvaluation.HeldOutSourceSetSha256,
                ParentHeldOutEvaluationInputSha256: parentEvaluation.HeldOutEvaluationInputSha256,
                ComparableHeldOut: comparableHeldOut,
                AccuracyDelta: comparableHeldOut
                    ? evaluation.Accuracy - parentEvaluation.Accuracy
                    : null));
    }

    private static bool TryReadParentCycleOrdinal(
        DesignConversionDpoPolicyContinuationRequest? continuationRequest,
        out DesignConversionDpoHeldOutEvaluation parentEvaluation,
        out int cycleOrdinal)
    {
        parentEvaluation = null!;
        cycleOrdinal = 0;
        if (continuationRequest is null ||
            !DesignConversionDpoPromotionArtifactContractCodec
                .ValidateEvaluatedContractPair(
                    continuationRequest.SourceEncoderContract,
                    continuationRequest.PolicyCheckpointContract) ||
            continuationRequest.PolicyCheckpointContract.Evaluation is not { } candidate ||
            (!string.Equals(
                 candidate.Schema,
                 PreviousEvaluatedSchema,
                 StringComparison.Ordinal) &&
             !string.Equals(
                 candidate.Schema,
                 EvaluatedSchema,
                 StringComparison.Ordinal)) ||
            candidate.CycleAccuracyLineage is not { } lineage ||
            lineage.CycleOrdinal < 1)
        {
            return false;
        }

        parentEvaluation = candidate;
        cycleOrdinal = lineage.CycleOrdinal;
        return true;
    }

    private static DesignConversionDpoOwnedFile ConvertTrainerOwnershipProof(
        DpoTrainerArtifactOwnershipProof proof,
        string scratchDirectory,
        int maximumStepCount)
    {
        if (proof is null || proof.Step < 1 || proof.Step > maximumStepCount ||
            proof.LinkCount != 1 || proof.Length < 0 ||
            proof.Sha256.Length != 64 || proof.Sha256.Any(character => !Uri.IsHexDigit(character)))
        {
            throw new IOException("DpoTrainer artifact ownership proof is invalid.");
        }

        var fullPath = Path.GetFullPath(proof.Path);
        var fullScratchDirectory = Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(scratchDirectory));
        if (!string.Equals(
                Path.TrimEndingDirectorySeparator(Path.GetDirectoryName(fullPath) ?? string.Empty),
                fullScratchDirectory,
                OperatingSystem.IsWindows()
                    ? StringComparison.OrdinalIgnoreCase
                    : StringComparison.Ordinal))
        {
            throw new IOException("DpoTrainer artifact ownership escaped scratch.");
        }

        var leaf = Path.GetFileName(fullPath);
        var policyLeaf = DpoTrainer.BuildPolicyCheckpointFileName(proof.Step);
        var metricsLeaf = DpoTrainer.BuildMetricsFileName(proof.Step);
        var validName = proof.Kind switch
        {
            DpoTrainerArtifactKind.ReservationLock => string.Equals(
                leaf,
                $".{DpoTrainer.MetricsFilePrefix}{proof.Step:D6}.lock",
                StringComparison.Ordinal),
            DpoTrainerArtifactKind.PolicySerialization =>
                IsTrainerSerializationLeaf(leaf, policyLeaf),
            DpoTrainerArtifactKind.PolicyStaging => IsTrainerStagingLeaf(leaf, policyLeaf),
            DpoTrainerArtifactKind.MetricsStaging => IsTrainerStagingLeaf(leaf, metricsLeaf),
            DpoTrainerArtifactKind.PolicyFinal => string.Equals(leaf, policyLeaf, StringComparison.Ordinal),
            DpoTrainerArtifactKind.MetricsFinal => string.Equals(leaf, metricsLeaf, StringComparison.Ordinal),
            _ => false
        };
        var maximumBytes = proof.Kind is
            DpoTrainerArtifactKind.PolicySerialization or
            DpoTrainerArtifactKind.PolicyStaging or DpoTrainerArtifactKind.PolicyFinal
                ? 256L * 1024 * 1024
                : proof.Kind is DpoTrainerArtifactKind.ReservationLock
                    ? 4L * 1024
                    : 1024L * 1024;
        if (!validName || proof.Length > maximumBytes ||
            proof.Kind != DpoTrainerArtifactKind.ReservationLock && proof.Length == 0)
        {
            throw new IOException("DpoTrainer artifact ownership proof is outside bounds.");
        }

        return new DesignConversionDpoOwnedFile(
            fullScratchDirectory,
            leaf,
            proof.Sha256.ToLowerInvariant(),
            new DesignConversionFileIdentity(
                proof.Identity.VolumeSerialNumber,
                proof.Identity.FileIdLow,
                proof.Identity.FileIdHigh));
    }

    private static bool AcceptTrainerArtifactOwnership(
        DpoTrainerArtifactOwnershipProof proof,
        string scratchDirectory,
        int maximumStepCount,
        IDictionary<DpoTrainerArtifactIdentity, AcceptedTrainerArtifactOwnership> acceptedByIdentity,
        ICollection<DesignConversionDpoRetainedOwnedFile> cleanupKnowledge,
        DesignConversionHandleBoundDirectory? retainedDirectory,
        Action<DesignConversionDpoEvaluatedOwnershipObservation>? ownershipObserver)
    {
        var owned = ConvertTrainerOwnershipProof(proof, scratchDirectory, maximumStepCount);
        if (acceptedByIdentity.TryGetValue(proof.Identity, out var previous))
        {
            var expectedPreviousKind = proof.Kind switch
            {
                DpoTrainerArtifactKind.PolicyFinal => DpoTrainerArtifactKind.PolicyStaging,
                DpoTrainerArtifactKind.MetricsFinal => DpoTrainerArtifactKind.MetricsStaging,
                _ => (DpoTrainerArtifactKind?)null
            };
            var pathComparison = OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;
            if (expectedPreviousKind is null ||
                previous.Proof.Kind != expectedPreviousKind ||
                previous.Proof.Step != proof.Step ||
                proof.TransitionFromKind != expectedPreviousKind ||
                proof.TransitionFromIdentity != previous.Proof.Identity ||
                !string.Equals(proof.TransitionFromPath, previous.Proof.Path, pathComparison) ||
                proof.Length != previous.Proof.Length ||
                !string.Equals(proof.Sha256, previous.Proof.Sha256, StringComparison.Ordinal))
            {
                throw new IOException("DpoTrainer artifact ownership transition is invalid.");
            }

            if (previous.Custody.RetainedStream is not null && retainedDirectory is not null)
            {
                var retainedStream = previous.Custody.RetainedStream;
                if (retainedDirectory.GetIdentity(retainedStream, owned.LeafName) != owned.Identity ||
                    retainedDirectory.GetLinkCount(retainedStream, owned.LeafName) != 1 ||
                    !string.Equals(HashStream(retainedStream), owned.Sha256, StringComparison.Ordinal))
                {
                    throw new IOException("DpoTrainer retained artifact custody changed during publication.");
                }
            }
            previous.Custody.OwnedFile = owned;
            acceptedByIdentity[proof.Identity] = previous with { Proof = proof };
        }
        else if (proof.Kind is DpoTrainerArtifactKind.PolicyFinal or DpoTrainerArtifactKind.MetricsFinal ||
                 proof.TransitionFromKind is not null ||
                 proof.TransitionFromPath is not null ||
                 proof.TransitionFromIdentity is not null)
        {
            throw new IOException("DpoTrainer artifact ownership transition source was not accepted.");
        }
        else
        {
            FileStream? retainedStream = null;
            try
            {
                if (OperatingSystem.IsLinux() && retainedDirectory is not null)
                {
                    retainedStream = retainedDirectory.TryOpenExisting(
                        owned.LeafName,
                        FileShare.Read | FileShare.Write | FileShare.Delete,
                        writeThrough: false,
                        owned.Identity) ??
                        throw new IOException("DpoTrainer artifact disappeared before custody retention.");
                    if (retainedDirectory.GetLinkCount(retainedStream, owned.LeafName) != 1 ||
                        retainedStream.Length != proof.Length ||
                        !string.Equals(HashStream(retainedStream), owned.Sha256, StringComparison.Ordinal))
                    {
                        throw new IOException("DpoTrainer retained artifact proof changed before acceptance.");
                    }
                }

                // На Linux мост открывает второй дескриптор, пока тренер ещё
                // держит исходный. До очистки доказательством служит именно
                // этот открытый файл, а не повторно найденное строковое имя.
                var custody = new DesignConversionDpoRetainedOwnedFile(owned, retainedStream);
                retainedStream = null;
                cleanupKnowledge.Add(custody);
                acceptedByIdentity[proof.Identity] =
                    new AcceptedTrainerArtifactOwnership(proof, custody);
            }
            finally
            {
                retainedStream?.Dispose();
            }
        }

        ownershipObserver?.Invoke(
            new DesignConversionDpoEvaluatedOwnershipObservation(
                DesignConversionDpoEvaluatedOwnershipPhase.TrainerArtifactOwnershipProven,
                scratchDirectory,
                proof.Path,
                proof.Step,
                proof));

        return true;
    }

    private sealed record AcceptedTrainerArtifactOwnership(
        DpoTrainerArtifactOwnershipProof Proof,
        DesignConversionDpoRetainedOwnedFile Custody);

    private sealed class TrainingLeaseTransferGuard(
        ServerOwnedLegalTrainingTeacherAdmissionLease trainingLease) : IDisposable
    {
        private ServerOwnedLegalTrainingTeacherAdmissionLease? _trainingLease = trainingLease;

        internal void TransferOwnership()
            => Interlocked.Exchange(ref _trainingLease, null);

        public void Dispose()
            => Interlocked.Exchange(ref _trainingLease, null)?.Dispose();
    }

    private static bool IsTrainerStagingLeaf(string leafName, string finalLeafName)
    {
        var prefix = $".{finalLeafName}.";
        const string suffix = ".staging";
        if (!leafName.StartsWith(prefix, StringComparison.Ordinal) ||
            !leafName.EndsWith(suffix, StringComparison.Ordinal))
        {
            return false;
        }
        var attemptId = leafName.AsSpan(
            prefix.Length,
            leafName.Length - prefix.Length - suffix.Length);
        if (attemptId.Length != 32)
            return false;
        foreach (var value in attemptId)
        {
            if (!Uri.IsHexDigit(value))
                return false;
        }
        return true;
    }

    private static bool IsTrainerSerializationLeaf(string leafName, string finalLeafName)
    {
        const string suffix = ".serialization";
        if (!leafName.EndsWith(suffix, StringComparison.Ordinal))
            return false;
        var withoutSuffix = leafName[..^suffix.Length];
        var separator = withoutSuffix.LastIndexOf('.');
        if (separator < 0)
            return false;
        var attemptId = withoutSuffix.AsSpan(separator + 1);
        return attemptId.Length == 32 &&
               attemptId.ToString().All(Uri.IsHexDigit) &&
               IsTrainerStagingLeaf(withoutSuffix[..separator], finalLeafName);
    }

    private static bool DeleteOwnedScratchDirectoryNoGap(
        string scratchDirectory,
        DesignConversionHandleBoundDirectory retainedDirectory,
        IReadOnlyList<DesignConversionDpoRetainedOwnedFile> ownedFiles,
        Action<DesignConversionDpoEvaluatedOwnershipObservation>? ownershipObserver)
    {
        if (OperatingSystem.IsLinux())
        {
            return DeleteOwnedScratchDirectoryNoGapLinux(
                scratchDirectory,
                retainedDirectory,
                ownedFiles,
                ownershipObserver);
        }

        if (!OperatingSystem.IsWindows())
            return false;

        var ownership = ownedFiles.FirstOrDefault(static artifact =>
            string.Equals(artifact.LeafName, ".evaluated-ownership", StringComparison.Ordinal));
        if (ownership is null)
            return false;

        using var deleteHandle = CreateFileWindows(
            scratchDirectory,
            ScratchDeleteAccess | ScratchReadAttributesAccess | ScratchSynchronizeAccess,
            (uint)(FileShare.Read | FileShare.Write),
            IntPtr.Zero,
            ScratchOpenExisting,
            ScratchFileFlagBackupSemantics | ScratchFileFlagOpenReparsePoint,
            IntPtr.Zero);
        if (deleteHandle.IsInvalid ||
            !GetFileInformationByHandleExWindows(
                deleteHandle,
                ScratchFileAttributeTagInfoClass,
                out var directoryAttributes,
                (uint)Marshal.SizeOf<ScratchFileAttributeTagInfo>()) ||
            (directoryAttributes.FileAttributes & ScratchFileAttributeDirectory) == 0 ||
            (directoryAttributes.FileAttributes & ScratchFileAttributeReparsePoint) != 0)
        {
            return false;
        }

        using (var retainedOwnershipStream = retainedDirectory.TryOpenExisting(
                   ownership.LeafName,
                   FileShare.Read | FileShare.Write | FileShare.Delete,
                   writeThrough: false,
                   ownership.Identity))
        using (var pathOwnershipStream = new FileStream(
                   Path.Combine(scratchDirectory, ownership.LeafName),
                   FileMode.Open,
                   FileAccess.Read,
                   FileShare.Read | FileShare.Write | FileShare.Delete))
        {
            if (retainedOwnershipStream is null ||
                retainedDirectory.GetIdentity(retainedOwnershipStream, ownership.LeafName) != ownership.Identity ||
                retainedDirectory.GetIdentity(pathOwnershipStream, ownership.LeafName) != ownership.Identity ||
                retainedDirectory.GetLinkCount(retainedOwnershipStream, ownership.LeafName) != 1 ||
                retainedDirectory.GetLinkCount(pathOwnershipStream, ownership.LeafName) != 1 ||
                !string.Equals(HashStream(retainedOwnershipStream), ownership.Sha256, StringComparison.Ordinal) ||
                !string.Equals(HashStream(pathOwnershipStream), ownership.Sha256, StringComparison.Ordinal))
            {
                return false;
            }

            ownershipObserver?.Invoke(
                new DesignConversionDpoEvaluatedOwnershipObservation(
                    DesignConversionDpoEvaluatedOwnershipPhase.AfterScratchOwnershipCheckBeforeDelete,
                    scratchDirectory,
                    null));
        }

        foreach (var artifact in ownedFiles.AsEnumerable().Reverse())
            DeleteIdentityAndHashMatchedBestEffort(artifact, retainedDirectory);

        var disposition = new ScratchFileDispositionInfo { DeleteFile = 1 };
        if (!SetFileInformationByHandleWindows(
                deleteHandle,
                ScratchFileDispositionInfoClass,
                ref disposition,
                (uint)Marshal.SizeOf<ScratchFileDispositionInfo>()))
        {
            // Owned leaves may already be gone, but the namespace itself remains
            // visibly preserved when the kernel did not accept delete disposition.
            return false;
        }
        return true;
    }

    private static bool DeleteOwnedScratchDirectoryNoGapLinux(
        string scratchDirectory,
        DesignConversionHandleBoundDirectory retainedDirectory,
        IReadOnlyList<DesignConversionDpoRetainedOwnedFile> ownedFiles,
        Action<DesignConversionDpoEvaluatedOwnershipObservation>? ownershipObserver)
    {
        var ownership = ownedFiles.FirstOrDefault(static artifact =>
            string.Equals(artifact.LeafName, ".evaluated-ownership", StringComparison.Ordinal));
        if (ownership is null)
            return false;

        var retainedOwnershipStream = ownership.RetainedStream;
        if (retainedOwnershipStream is null ||
            retainedDirectory.GetIdentity(retainedOwnershipStream, ownership.LeafName) != ownership.Identity ||
            retainedDirectory.GetLinkCount(retainedOwnershipStream, ownership.LeafName) != 1 ||
            !string.Equals(HashStream(retainedOwnershipStream), ownership.Sha256, StringComparison.Ordinal))
        {
            return false;
        }

        ownershipObserver?.Invoke(
            new DesignConversionDpoEvaluatedOwnershipObservation(
                DesignConversionDpoEvaluatedOwnershipPhase.AfterScratchOwnershipCheckBeforeDelete,
                scratchDirectory,
                null));

        // На Linux закрытие файла не даёт Windows-гарантии автоматического
        // удаления. Поэтому файлы временного каталога удаляются через удержанный
        // дескриптор каталога лишь после совпадения физической личности файла и
        // контрольной суммы с ранее доказанным собственным файлом.
        foreach (var artifact in ownedFiles.AsEnumerable().Reverse())
            DeleteIdentityAndHashMatchedBestEffort(artifact, retainedDirectory);

        return retainedDirectory.TryDeleteEmptyRootDirectoryNoGap();
    }

    private static void DeleteEmptyDirectoryBestEffort(string directory)
    {
        try
        {
            Directory.Delete(directory, recursive: false);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
        }
    }

    private static string HashStream(Stream stream)
    {
        stream.Position = 0;
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static DesignConversionDpoOwnedFilePayload ReadBoundedOwnedFile(
        DesignConversionHandleBoundDirectory directory,
        string leaf,
        long maximumBytes,
        CancellationToken ct,
        DesignConversionFileIdentity? expectedIdentity = null)
    {
        using var stream = directory.TryOpenExisting(
            leaf,
            FileShare.Read,
            writeThrough: false,
            expectedIdentity) ??
            throw new IOException("Evaluated owned artifact disappeared.");
        if (stream.Length <= 0 || stream.Length > maximumBytes ||
            stream.Length > int.MaxValue)
            throw new InvalidOperationException("Evaluated checkpoint size is invalid.");
        var identity = directory.GetIdentity(stream, leaf);
        var expectedLength = stream.Length;
        var bytes = new byte[checked((int)expectedLength)];
        var offset = 0;
        while (offset < bytes.Length)
        {
            ct.ThrowIfCancellationRequested();
            var read = stream.Read(bytes, offset, bytes.Length - offset);
            if (read <= 0)
                throw new EndOfStreamException();
            offset += read;
        }
        if (stream.ReadByte() != -1 || stream.Length != expectedLength ||
            directory.GetIdentity(stream, leaf) != identity ||
            directory.GetLinkCount(stream, leaf) != 1)
        {
            throw new IOException("Evaluated checkpoint changed while reading.");
        }
        var directoryPath = Path.GetDirectoryName(stream.Name) ?? string.Empty;
        return new DesignConversionDpoOwnedFilePayload(
            bytes,
            new DesignConversionDpoOwnedFile(
                directoryPath,
                leaf,
                ComputeSha256(bytes),
                identity));
    }

    private static DesignConversionDpoRetainedOwnedFile WriteExclusiveOwned(
        DesignConversionHandleBoundDirectory directory,
        string directoryPath,
        string leaf,
        byte[] payload)
    {
        if (string.IsNullOrWhiteSpace(directoryPath) || payload.Length == 0)
            throw new InvalidOperationException("Evaluated final artifact directory is invalid.");
        FileStream? stream = directory.CreateNew(
            leaf,
            FileShare.Read,
            writeThrough: true);
        try
        {
            stream.Write(payload);
            stream.Flush(flushToDisk: true);
            if (stream.Length != payload.LongLength || directory.GetLinkCount(stream, leaf) != 1)
                throw new IOException("Evaluated owned artifact changed during creation.");
            var owned = new DesignConversionDpoOwnedFile(
                Path.GetFullPath(directoryPath),
                leaf,
                ComputeSha256(payload),
                directory.GetIdentity(stream, leaf));
            if (OperatingSystem.IsLinux())
            {
                // На Linux исходный дескриптор записи остаётся открытым до
                // принятия результата или отката. Повторно найденное имя уже
                // не может выдать подменённый файл за созданный нами.
                var retained = new DesignConversionDpoRetainedOwnedFile(owned, stream);
                stream = null;
                return retained;
            }

            return new DesignConversionDpoRetainedOwnedFile(owned, retainedStream: null);
        }
        finally
        {
            stream?.Dispose();
        }
    }

    private static void DeleteIdentityAndHashMatchedBestEffort(
        DesignConversionDpoRetainedOwnedFile artifact,
        DesignConversionHandleBoundDirectory? retainedDirectory = null)
    {
        try
        {
            if (OperatingSystem.IsLinux() &&
                retainedDirectory is not null &&
                artifact.RetainedStream is not null)
            {
                var stream = artifact.RetainedStream;
                if (retainedDirectory.GetIdentity(stream, artifact.LeafName) != artifact.Identity ||
                    retainedDirectory.GetLinkCount(stream, artifact.LeafName) != 1 ||
                    !string.Equals(HashStream(stream), artifact.Sha256, StringComparison.Ordinal))
                {
                    return;
                }
                retainedDirectory.DeleteHeldRegular(
                    stream,
                    artifact.LeafName,
                    artifact.Identity);
                return;
            }

            DeleteIdentityAndHashMatchedBestEffort(
                artifact.OwnedFile,
                retainedDirectory);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
        }
    }

    private static void DeleteIdentityAndHashMatchedBestEffort(
        DesignConversionDpoOwnedFile artifact,
        DesignConversionHandleBoundDirectory? retainedDirectory = null)
    {
        if (!OperatingSystem.IsWindows())
        {
            // На Linux запасной путь только по сохранённым признакам намеренно
            // закрыт: удалить имя файла может только `DeleteHeldRegular`,
            // потому что он сверяет живой файловый дескриптор с удержанным
            // дескриптором каталога.
            Trace.TraceWarning(
                "DesignConversion DPO cleanup preserved unretained owned leaf for manual cleanup: {0}",
                Path.Combine(artifact.DirectoryPath, artifact.LeafName));
            return;
        }

        try
        {
            var ownsDirectory = retainedDirectory is null;
            var directory = retainedDirectory ??
                DesignConversionHandleBoundDirectory.AcquireExisting(
                    artifact.DirectoryPath,
                    beforeIo: null);
            try
            {
                using var stream = directory.TryOpenExisting(
                    artifact.LeafName,
                    FileShare.Read,
                    writeThrough: false,
                    artifact.Identity);
                if (stream is null || stream.Length < 0 || stream.Length > int.MaxValue)
                    return;
                var actualSha256 = Convert.ToHexString(SHA256.HashData(stream))
                    .ToLowerInvariant();
                if (!string.Equals(actualSha256, artifact.Sha256, StringComparison.Ordinal) ||
                    directory.GetIdentity(stream, artifact.LeafName) != artifact.Identity ||
                    directory.GetLinkCount(stream, artifact.LeafName) != 1)
                {
                    return;
                }
                stream.Dispose();
                directory.DeleteIfRegular(artifact.LeafName, artifact.Identity);
            }
            finally
            {
                if (ownsDirectory)
                    directory.Dispose();
            }
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
        }
    }

    private static OperationResult<T> EvaluatedNonFinite<T>()
        => OperationResult<T>.Fail(
            "DesignConversion DPO evaluated metrics are non-finite.",
            "DESIGN_CONVERSION_DPO_EVALUATED_METRICS_NON_FINITE");

    private static OperationResult<T> EvaluatedFinalArtifactCollision<T>()
        => OperationResult<T>.Fail(
            DesignConversionDpoPromotionArtifactContractCodec.ArtifactCollisionSafeMessage,
            DesignConversionDpoPromotionArtifactContractCodec.ArtifactCollisionErrorCode);

    private static bool IsDpoEvaluatedRecordUsable(DesignConversionDpoTrainingRecord row)
        => !string.IsNullOrWhiteSpace(row.RecordId) &&
           !string.IsNullOrWhiteSpace(row.UnitId) &&
           row.Chosen is not null && row.Rejected is not null &&
           row.Chosen.Verdict is not null && row.Rejected.Verdict is not null;

    private static string BuildEvaluatedResponse(DesignConversionImprovementOutput output)
        => SerializePolicySideText(output.Html, output.Css);

    private static void AppendEvaluatedTensor(IncrementalHash hash, Tensor tensor)
    {
        AppendEvaluatedInt32(hash, tensor.Shape.Rank);
        foreach (var dimension in tensor.Shape.Dimensions)
            AppendEvaluatedInt32(hash, dimension);
        AppendEvaluatedInt32(hash, tensor.Data.Length);
        foreach (var value in tensor.Data)
            AppendEvaluatedInt32(hash, BitConverter.SingleToInt32Bits(value));
    }

    private static void AppendEvaluatedString(IncrementalHash hash, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        AppendEvaluatedInt32(hash, bytes.Length);
        hash.AppendData(bytes);
    }

    private static void AppendEvaluatedInt32(IncrementalHash hash, int value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32BigEndian(bytes, value);
        hash.AppendData(bytes);
    }

    private static OperationResult<DpoTrainerPairAdmission>
        CreateDpoTrainerPairAdmission(
            DesignConversionDpoTrainingRecord row,
            string prompt,
            string chosen,
            string rejected)
    {
        if (!TryResolveBridgePairAuthors(
                row,
                out var chosenAuthor,
                out var rejectedAuthor))
        {
            return PairAuthorshipMissingOrInvalid<
                DpoTrainerPairAdmission>();
        }

        try
        {
            return OperationResult<DpoTrainerPairAdmission>.Ok(
                DpoTrainerPairCustoms.CreateAdmitted(
                    prompt,
                    chosen,
                    rejected,
                    chosenAuthor,
                    rejectedAuthor,
                    nameof(DesignConversionDpoPolicyCheckpointBridge))
                .Admission);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return PairAuthorshipMissingOrInvalid<
                DpoTrainerPairAdmission>();
        }
    }

    private static OperationResult<bool> ValidateDpoTrainerPairAdmission(
        DpoTrainerPairAdmission? admission,
        DesignConversionDpoTrainingRecord row,
        string prompt,
        string chosen,
        string rejected)
    {
        if (!TryResolveBridgePairAuthors(
                row,
                out var chosenAuthor,
                out var rejectedAuthor))
        {
            return PairAuthorshipMissingOrInvalid<bool>();
        }

        return ValidateDpoTrainerPairAdmission(
            admission,
            chosenAuthor,
            rejectedAuthor,
            row.TeacherProvenance,
            prompt,
            chosen,
            rejected);
    }

    private static OperationResult<bool> ValidateDpoTrainerPairAdmission(
        DpoTrainerPairAdmission? admission,
        DpoPairSideAuthorKind? expectedChosenAuthor,
        DpoPairSideAuthorKind? expectedRejectedAuthor,
        TrainingTeacherProvenance? provenance,
        string prompt,
        string chosen,
        string rejected)
    {
        if (admission is null ||
            !ResolveExpectedBridgePairAuthors(
                expectedChosenAuthor,
                expectedRejectedAuthor,
                provenance,
                out var chosenAuthor,
                out var rejectedAuthor) ||
            admission.ChosenAuthor != chosenAuthor ||
            admission.RejectedAuthor != rejectedAuthor ||
            !string.Equals(
                admission.IssuerName,
                nameof(DesignConversionDpoPolicyCheckpointBridge),
                StringComparison.Ordinal))
        {
            return PairAuthorshipMissingOrInvalid<bool>();
        }

        try
        {
            var expected = DpoTrainerPairCustoms.CreateAdmitted(
                prompt,
                chosen,
                rejected,
                chosenAuthor,
                rejectedAuthor,
                nameof(DesignConversionDpoPolicyCheckpointBridge)).Admission;
            return string.Equals(
                       expected.ContentFingerprintSha256,
                       admission.ContentFingerprintSha256,
                       StringComparison.Ordinal)
                ? OperationResult<bool>.Ok(true)
                : PairAuthorshipMissingOrInvalid<bool>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return PairAuthorshipMissingOrInvalid<bool>();
        }
    }

    private static OperationResult<bool> ValidateStandaloneVectorBatchAdmission(
        DesignConversionDpoTrainingRecord row,
        DpoVectorBatch? batch)
    {
        if (batch is null)
            return PairAuthorshipMissingOrInvalid<bool>();

        var preference = BuildPolicyPreferenceText(row);
        return ValidateDpoTrainerPairAdmission(
            batch.Batch.Admission,
            row,
            BuildPolicyPrompt(row),
            preference.Chosen,
            preference.Rejected);
    }

    internal static OperationResult<bool>
        ValidateStandaloneVectorBatchAdmissionPreviewForTests(
            DesignConversionDpoTrainingRecord row,
            DpoVectorBatch batch)
        => ValidateStandaloneVectorBatchAdmission(row, batch);

    private static OperationResult<bool> ValidateStandaloneOptimizerAdmissions(
        IReadOnlyList<FinalVectorOptimizerInput>? inputs)
    {
        if (inputs is null || inputs.Count == 0)
            return PairAuthorshipMissingOrInvalid<bool>();

        foreach (var input in inputs)
        {
            if (input is null)
                return PairAuthorshipMissingOrInvalid<bool>();

            var admission = ValidateDpoTrainerPairAdmission(
                input.Batch.Batch.Admission,
                input.ChosenAuthorKind,
                input.RejectedAuthorKind,
                input.Provenance,
                input.Prompt,
                input.Chosen,
                input.Rejected);
            if (!admission.IsSuccess)
                return admission;
        }

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<bool> ValidateEvaluatedTrainPackedAdmissions(
        IReadOnlyList<DesignConversionDpoEvaluatedPackedBatch>? inputs)
    {
        if (inputs is null || inputs.Count == 0)
            return PairAuthorshipMissingOrInvalid<bool>();

        foreach (var input in inputs)
        {
            if (input is null)
                return PairAuthorshipMissingOrInvalid<bool>();

            var admission = ValidateDpoTrainerPairAdmission(
                input.VectorBatch.Batch.Admission,
                input.ChosenAuthorKind,
                input.RejectedAuthorKind,
                input.Provenance,
                input.Prompt,
                input.Chosen,
                input.Rejected);
            if (!admission.IsSuccess)
                return admission;
        }

        return OperationResult<bool>.Ok(true);
    }

    internal static OperationResult<bool>
        ValidateEvaluatedTrainPackedAdmissionsPreviewForTests(
            IReadOnlyList<DesignConversionDpoEvaluatedPackedBatch> inputs)
        => ValidateEvaluatedTrainPackedAdmissions(inputs);

    private static OperationResult<bool> ValidateEvaluatedHeldOutAdmissions(
        IReadOnlyList<DesignConversionDpoEvaluatedPackedBatch>? inputs)
    {
        if (inputs is null)
            return PairAuthorshipMissingOrInvalid<bool>();

        foreach (var input in inputs)
        {
            if (input is null || input.VectorBatch.Batch.Admission is not null)
                return PairAuthorshipMissingOrInvalid<bool>();
        }

        return OperationResult<bool>.Ok(true);
    }

    private static bool TryResolveBridgePairAuthors(
        DesignConversionDpoTrainingRecord? row,
        out DpoPairSideAuthorKind chosenAuthor,
        out DpoPairSideAuthorKind rejectedAuthor)
    {
        chosenAuthor = default;
        rejectedAuthor = default;
        if (row is null ||
            !IsValidOptionalPairAuthorKind(row.ChosenAuthorKind) ||
            !IsValidOptionalPairAuthorKind(row.RejectedAuthorKind))
        {
            return false;
        }

        if (!row.ChosenAuthorKind.HasValue &&
            !row.RejectedAuthorKind.HasValue &&
            IsExactDeterministicNoExternalTeacher(row.TeacherProvenance))
        {
            chosenAuthor = DpoPairSideAuthorKind.DeterministicGenerator;
            rejectedAuthor = DpoPairSideAuthorKind.DeterministicGenerator;
            return true;
        }

        if (!row.ChosenAuthorKind.HasValue ||
            !row.RejectedAuthorKind.HasValue)
        {
            return false;
        }

        chosenAuthor = row.ChosenAuthorKind.Value;
        rejectedAuthor = row.RejectedAuthorKind.Value;
        if (chosenAuthor is DpoPairSideAuthorKind.DeterministicGenerator &&
            rejectedAuthor is DpoPairSideAuthorKind.DeterministicGenerator)
        {
            return IsExactDeterministicNoExternalTeacher(
                row.TeacherProvenance);
        }

        if (!PairHasExactlyOneOpenTeacherAndOneLocalModel(
                chosenAuthor,
                rejectedAuthor) ||
            !IsOpenTeacherTrainingProvenance(row.TeacherProvenance) ||
            HasTeacherCandidateOnlyTrainingBoundary(row))
        {
            return false;
        }

        return DirectionMetadataMatchesAuthors(
            row,
            chosenAuthor,
            rejectedAuthor);
    }

    private static bool ResolveExpectedBridgePairAuthors(
        DpoPairSideAuthorKind? expectedChosenAuthor,
        DpoPairSideAuthorKind? expectedRejectedAuthor,
        TrainingTeacherProvenance? provenance,
        out DpoPairSideAuthorKind chosenAuthor,
        out DpoPairSideAuthorKind rejectedAuthor)
    {
        chosenAuthor = default;
        rejectedAuthor = default;
        if (!IsValidOptionalPairAuthorKind(expectedChosenAuthor) ||
            !IsValidOptionalPairAuthorKind(expectedRejectedAuthor))
        {
            return false;
        }

        if (!expectedChosenAuthor.HasValue &&
            !expectedRejectedAuthor.HasValue &&
            IsExactDeterministicNoExternalTeacher(provenance))
        {
            chosenAuthor = DpoPairSideAuthorKind.DeterministicGenerator;
            rejectedAuthor = DpoPairSideAuthorKind.DeterministicGenerator;
            return true;
        }

        if (!expectedChosenAuthor.HasValue ||
            !expectedRejectedAuthor.HasValue)
        {
            return false;
        }

        chosenAuthor = expectedChosenAuthor.Value;
        rejectedAuthor = expectedRejectedAuthor.Value;
        if (chosenAuthor is DpoPairSideAuthorKind.DeterministicGenerator &&
            rejectedAuthor is DpoPairSideAuthorKind.DeterministicGenerator)
        {
            return IsExactDeterministicNoExternalTeacher(provenance);
        }

        return PairHasExactlyOneOpenTeacherAndOneLocalModel(
                   chosenAuthor,
                   rejectedAuthor) &&
               IsOpenTeacherTrainingProvenance(provenance);
    }

    private static bool IsValidOptionalPairAuthorKind(
        DpoPairSideAuthorKind? authorKind)
        => authorKind is null ||
           Enum.IsDefined(typeof(DpoPairSideAuthorKind), authorKind.Value);

    private static bool PairHasExactlyOneOpenTeacherAndOneLocalModel(
        DpoPairSideAuthorKind chosenAuthor,
        DpoPairSideAuthorKind rejectedAuthor)
        => chosenAuthor is DpoPairSideAuthorKind.OpenTeacher &&
           rejectedAuthor is DpoPairSideAuthorKind.LocalModel ||
           chosenAuthor is DpoPairSideAuthorKind.LocalModel &&
           rejectedAuthor is DpoPairSideAuthorKind.OpenTeacher;

    private static bool DirectionMetadataMatchesAuthors(
        DesignConversionDpoTrainingRecord row,
        DpoPairSideAuthorKind chosenAuthor,
        DpoPairSideAuthorKind rejectedAuthor)
    {
        var chosenSide = ReadPairMetadata(row, PairChosenSourceSideMetadataKey);
        var rejectedSide = ReadPairMetadata(row, PairRejectedSourceSideMetadataKey);
        return AuthorMatchesSourceSide(chosenAuthor, chosenSide) &&
               AuthorMatchesSourceSide(rejectedAuthor, rejectedSide) &&
               !string.Equals(chosenSide, rejectedSide, StringComparison.Ordinal);
    }

    private static bool AuthorMatchesSourceSide(
        DpoPairSideAuthorKind author,
        string side)
        => string.Equals(side, PairSourceSideGenerated, StringComparison.Ordinal)
            ? author is DpoPairSideAuthorKind.OpenTeacher
            : string.Equals(
                side,
                PairSourceSideIncumbent,
                StringComparison.Ordinal) &&
              author is DpoPairSideAuthorKind.LocalModel;

    private static bool IsOpenTeacherTrainingProvenance(
        TrainingTeacherProvenance? provenance)
    {
        if (provenance is not
            {
                SourceKind: TrainingTeacherSourceKind.ExternalTeacherOutput,
                Family: not null,
                AccessMode: not null,
                HostingProvider: not null,
                ModelLicense: not null,
                ClosedProprietaryModel: false,
                TeacherOutputUsedForTraining: true
            } ||
            provenance.InvocationCount <= 0 ||
            !Enum.IsDefined(
                typeof(OpenWeightTeacherFamily),
                provenance.Family.Value) ||
            !Enum.IsDefined(
                typeof(TrainingTeacherAccessMode),
                provenance.AccessMode.Value) ||
            !Enum.IsDefined(
                typeof(TrainingTeacherHostingProvider),
                provenance.HostingProvider.Value) ||
            !Enum.IsDefined(
                typeof(TrainingTeacherModelLicense),
                provenance.ModelLicense.Value) ||
            string.IsNullOrWhiteSpace(provenance.ModelId) ||
            string.IsNullOrWhiteSpace(provenance.ModelVersion))
        {
            return false;
        }

        var isLocalOllama =
            provenance.AccessMode is TrainingTeacherAccessMode
                .LocalHostedOpenWeight &&
            provenance.HostingProvider is TrainingTeacherHostingProvider
                .Ollama;
        return isLocalOllama
            ? !provenance.ProviderTermsChecked &&
              !provenance.ProviderTermsAllowTraining &&
              LegalTrainingTeacherAdmissionAuthority
                  .IsCanonicalLocalOllamaArtifactIdentity(
                      provenance.ModelVersion,
                      provenance.ModelArtifactDigestKind,
                      provenance.ModelArtifactDigest)
            : provenance.ProviderTermsChecked &&
              provenance.ProviderTermsAllowTraining;
    }

    private static bool IsExactDeterministicNoExternalTeacher(
        TrainingTeacherProvenance? provenance)
        => provenance is
           {
               SourceKind: TrainingTeacherSourceKind.DeterministicNoExternalTeacher,
               Family: null,
               AccessMode: null,
               HostingProvider: null,
               ModelLicense: null,
               ProviderTermsChecked: false,
               ProviderTermsAllowTraining: false,
               ClosedProprietaryModel: false,
               TeacherOutputUsedForTraining: false,
               InvocationCount: 0
           } &&
           string.IsNullOrEmpty(provenance.ModelId) &&
           string.IsNullOrEmpty(provenance.ModelVersion) &&
           string.IsNullOrEmpty(provenance.ModelArtifactDigestKind) &&
           string.IsNullOrEmpty(provenance.ModelArtifactDigest);

    private static bool HasTeacherCandidateOnlyTrainingBoundary(
        DesignConversionDpoTrainingRecord row)
        => MetadataTruthy(row, "chosen_rejected_pair_not_proven") ||
           MetadataTruthy(row, "independent_preference_not_proven") ||
           MetadataPresent(row, "teacher_candidate_only_boundary") ||
           MetadataTruthy(row, "training_candidate_only") &&
           !MetadataTruthy(row, "independent_preference_proven");

    private static bool MetadataTruthy(
        DesignConversionDpoTrainingRecord row,
        string key)
        => bool.TryParse(ReadPairMetadata(row, key), out var parsed) && parsed;

    private static bool MetadataPresent(
        DesignConversionDpoTrainingRecord row,
        string key)
        => !string.IsNullOrWhiteSpace(ReadPairMetadata(row, key));

    private static string ReadPairMetadata(
        DesignConversionDpoTrainingRecord row,
        string key)
    {
        if (row.Provenance?.Metadata is not null &&
            row.Provenance.Metadata.TryGetValue(key, out var sourceValue))
        {
            return sourceValue ?? string.Empty;
        }

        if (row.License?.Metadata is not null &&
            row.License.Metadata.TryGetValue(key, out var licenseValue))
        {
            return licenseValue ?? string.Empty;
        }

        return string.Empty;
    }

    private static OperationResult<T> PairAuthorshipMissingOrInvalid<T>()
        => OperationResult<T>.Fail(
            "DesignConversion DPO pair authorship is missing or invalid.",
            PairAuthorshipMissingOrInvalidErrorCode);

    private static OperationResult<T> EvaluatedPackingInvalid<T>()
        => OperationResult<T>.Fail(
            "DesignConversion DPO evaluated pair packing is invalid.",
            EvaluatedPackingInvalidErrorCode);

    private static OperationResult<DpoBatch> BuildBatch(
        DesignConversionDpoTrainingRecord row,
        ModelConfig config)
    {
        var prompt = BuildPolicyPrompt(row);
        var preferenceText = BuildPolicyPreferenceText(row);
        var admission = CreateDpoTrainerPairAdmission(
            row,
            prompt,
            preferenceText.Chosen,
            preferenceText.Rejected);
        if (!admission.IsSuccess || admission.Value is null)
            return admission.MapError<DpoBatch>();

        var chosenIds = EncodeIds($"{prompt} {preferenceText.Chosen}", config);
        var rejectedIds = EncodeIds($"{prompt} {preferenceText.Rejected}", config);
        var length = Math.Max(chosenIds.Count, rejectedIds.Count);

        return OperationResult<DpoBatch>.Ok(new DpoBatch
        {
            ChosenIds = ToTensor(chosenIds, length),
            ChosenMask = ToMask(chosenIds.Count, length),
            RejectedIds = ToTensor(rejectedIds, length),
            RejectedMask = ToMask(rejectedIds.Count, length),
            // Admission привязан к raw prompt/chosen/rejected до tokenization:
            // ids/masks могут меняться, но паспорт пары не пересчитывается из них.
            Admission = admission.Value
        });
    }

    private static OperationResult<DpoBatch> BuildBatch(
        DesignConversionDpoTrainingRecord row,
        DesignConversionDpoRuntimeEncodingBinding runtimeEncodingBinding)
    {
        var prompt = BuildPolicyPrompt(row);
        var preferenceText = BuildPolicyPreferenceText(row);
        var admission = CreateDpoTrainerPairAdmission(
            row,
            prompt,
            preferenceText.Chosen,
            preferenceText.Rejected);
        if (!admission.IsSuccess || admission.Value is null)
            return admission.MapError<DpoBatch>();

        var chosenIds = EncodeIds(
            $"{prompt} {preferenceText.Chosen}",
            runtimeEncodingBinding);
        var rejectedIds = EncodeIds(
            $"{prompt} {preferenceText.Rejected}",
            runtimeEncodingBinding);
        var length = Math.Max(chosenIds.Count, rejectedIds.Count);

        return OperationResult<DpoBatch>.Ok(new DpoBatch
        {
            ChosenIds = ToTensor(chosenIds, length),
            ChosenMask = ToMask(chosenIds.Count, length),
            RejectedIds = ToTensor(rejectedIds, length),
            RejectedMask = ToMask(rejectedIds.Count, length),
            // Runtime encoding меняет только ids/masks; raw triple admission
            // остается тем же мостовым паспортом.
            Admission = admission.Value
        });
    }

    internal static (string Chosen, string Rejected) BuildPolicyPreferenceTextPreviewForTests(
        DesignConversionDpoTrainingRecord row)
        => BuildPolicyPreferenceText(row);

    internal static string BuildPolicyPromptPreviewForTests(DesignConversionDpoTrainingRecord row)
        => DesignConversionDpoSourceEncoderContractBuilder.BuildPolicyPrompt(row);

    internal static LearningTrainingItem BuildPolicyRuntimeBridgeItemPreviewForTests(
        DesignConversionDpoTrainingRecord row,
        string policyCheckpointPath,
        string policyCheckpointSha256)
        => BuildPolicyRuntimeBridgeItem(row, policyCheckpointPath, policyCheckpointSha256);

    internal static LearningBatch<DesignConversionDomain> BuildPolicyRuntimeBridgeBatchPreviewForTests(
        IReadOnlyList<DesignConversionDpoTrainingRecord> rows,
        string policyCheckpointPath,
        string policyCheckpointSha256)
        => BuildPolicyRuntimeBridgeBatch(rows, policyCheckpointPath, policyCheckpointSha256);

    internal static string BuildPolicyRuntimeBridgePromptPreviewForTests(
        DesignConversionDpoTrainingRecord row,
        string policyCheckpointPath,
        string policyCheckpointSha256)
        => BuildPolicyRuntimeBridgePrompt(row, policyCheckpointPath, policyCheckpointSha256);

    internal static IReadOnlyList<int> EncodeRuntimePolicyTextForInference(
        string value,
        ModelConfig config)
        => EncodeIds(value, config);

    internal static IReadOnlyList<int> EncodeRuntimePolicyTextForInference(
        string value,
        ModelConfig config,
        int maxTokenCount,
        bool padToMinimum = true)
        => EncodeIds(value, config, maxTokenCount, padToMinimum);

    internal static OperationResult<IReadOnlyList<int>> EncodeRuntimePolicyTextForInference(
        string value,
        DesignConversionDpoRuntimeEncodingBinding runtimeEncodingBinding,
        ModelConfig config,
        int? maxTokenCount = null,
        bool padToMinimum = true)
    {
        var validated = ValidateRuntimeInferenceEncodingBinding(
            runtimeEncodingBinding,
            config);
        if (!validated.IsSuccess || validated.Value is null)
            return validated.MapError<IReadOnlyList<int>>();

        return OperationResult<IReadOnlyList<int>>.Ok(
            EncodeIds(
                value,
                validated.Value,
                maxTokenCount ?? Math.Max(2, validated.Value.ContextLength),
                padToMinimum));
    }

    internal static OperationResult<IReadOnlyList<int>> EncodeRuntimePolicyTextForInference(
        string value,
        DesignConversionDpoRuntimeInferenceEncodingBinding runtimeEncodingBinding,
        ModelConfig config,
        int? maxTokenCount = null,
        bool padToMinimum = true)
    {
        var validated = ValidateRuntimeInferenceEncodingBinding(
            runtimeEncodingBinding,
            config);
        if (!validated.IsSuccess || validated.Value is null)
            return validated.MapError<IReadOnlyList<int>>();

        return OperationResult<IReadOnlyList<int>>.Ok(
            EncodeIds(
                value,
                validated.Value,
                maxTokenCount ?? Math.Max(2, validated.Value.ContextLength),
                padToMinimum));
    }

    internal static string SerializePolicySideText(string? html, string? css)
        => JsonSerializer.Serialize(
            new
            {
                html = NormalizePolicySideLineEndings(html),
                css = NormalizePolicySideLineEndings(css)
            },
            RuntimeInferenceCanonicalJsonOptions);

    internal static IReadOnlyList<string> BuildSourceSignalSourceIdsPreviewForTests(DesignConversionDpoTrainingRecord row)
        => DesignConversionDpoSourceEncoderContractBuilder.BuildSourceIds(row);

    private static (string Chosen, string Rejected) BuildPolicyPreferenceText(DesignConversionDpoTrainingRecord row)
        => (
            SerializePolicySideText(row.Chosen.Html, row.Chosen.Css),
            SerializePolicySideText(row.Rejected.Html, row.Rejected.Css));

    private static string NormalizePolicySideLineEndings(string? value)
        => (value ?? string.Empty)
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n');

    private static string BuildPolicyPrompt(DesignConversionDpoTrainingRecord row)
        => DesignConversionDpoSourceEncoderContractBuilder.BuildPolicyPrompt(row);

    private static IReadOnlyList<int> EncodeIds(string value, ModelConfig config)
        => EncodeIds(
            value,
            config,
            Math.Max(2, config.ContextLen),
            padToMinimum: true);

    private static IReadOnlyList<int> EncodeIds(
        string value,
        DesignConversionDpoRuntimeEncodingBinding runtimeEncodingBinding)
        => EncodeIds(
            value,
            runtimeEncodingBinding,
            Math.Max(2, runtimeEncodingBinding.ContextLength),
            padToMinimum: true);

    private static IReadOnlyList<int> EncodeIds(
        string value,
        DesignConversionDpoRuntimeInferenceEncodingBinding runtimeEncodingBinding,
        int maxTokenCount,
        bool padToMinimum)
        => EncodeIds(
            value,
            runtimeEncodingBinding.TokenIdMinimum,
            runtimeEncodingBinding.TokenIdMaximum,
            runtimeEncodingBinding.ContextLength,
            maxTokenCount,
            padToMinimum);

    private static IReadOnlyList<int> EncodeIds(
        string value,
        ModelConfig config,
        int maxTokenCount,
        bool padToMinimum)
    {
        var bytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
        var maxLength = Math.Clamp(
            maxTokenCount,
            padToMinimum ? 2 : 0,
            Math.Max(2, config.ContextLen));
        var ids = bytes
            .Take(maxLength)
            .Select(value => Math.Clamp((int)value, 0, config.VocabSize - 1))
            .ToList();
        while (padToMinimum && ids.Count < 2)
            ids.Add(0);
        return ids;
    }

    private static IReadOnlyList<int> EncodeIds(
        string value,
        DesignConversionDpoRuntimeEncodingBinding runtimeEncodingBinding,
        int maxTokenCount,
        bool padToMinimum)
        => EncodeIds(
            value,
            runtimeEncodingBinding.TokenIdMinimum,
            runtimeEncodingBinding.TokenIdMaximum,
            runtimeEncodingBinding.ContextLength,
            maxTokenCount,
            padToMinimum);

    private static IReadOnlyList<int> EncodeIds(
        string value,
        int tokenIdMinimum,
        int tokenIdMaximum,
        int contextLength,
        int maxTokenCount,
        bool padToMinimum)
    {
        var bytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
        var maxLength = Math.Clamp(
            maxTokenCount,
            padToMinimum ? 2 : 0,
            Math.Max(2, contextLength));
        var ids = bytes
            .Take(maxLength)
            .Select(value => Math.Clamp(
                (int)value,
                tokenIdMinimum,
                tokenIdMaximum))
            .ToList();
        while (padToMinimum && ids.Count < 2)
            ids.Add(tokenIdMinimum);
        return ids;
    }

    private static Tensor ToTensor(IReadOnlyList<int> ids, int length)
    {
        var data = new float[length];
        for (var index = 0; index < length; index++)
            data[index] = index < ids.Count ? ids[index] : 0;

        return new Tensor(data, new TensorShape(1, length));
    }

    private static Tensor ToMask(int sourceLength, int length)
    {
        var data = new float[length];
        for (var index = 0; index < length; index++)
            data[index] = index < sourceLength ? 1f : 0f;

        return new Tensor(data, new TensorShape(1, length));
    }

    private static LearningBatch<DesignConversionDomain> BuildPolicyRuntimeBridgeBatch(
        IReadOnlyList<DesignConversionDpoTrainingRecord> rows,
        string policyCheckpointPath,
        string policyCheckpointSha256)
        => new(
            BatchId: $"dpo-policy-runtime-bridge:{policyCheckpointSha256[..12]}",
            DomainTag: DesignConversionMachine.DomainTag,
            TrainingItems: rows
                .Select(row => BuildPolicyRuntimeBridgeItem(row, policyCheckpointPath, policyCheckpointSha256))
                .ToArray(),
            SourceIds: rows
                .Select(row => $"design_conversion_dpo_record:{HashText(row.RecordId)}")
                .Concat(rows.SelectMany(
                    DesignConversionDpoSourceEncoderContractBuilder.BuildSourceIds))
                .Concat(
                [
                    DesignConversionDpoTrainerRouteBGate.DerivationSourceSystem,
                    $"dpo_policy_sha256:{policyCheckpointSha256}",
                    $"dpo_policy_file:{Path.GetFileName(policyCheckpointPath)}"
                ])
                .Distinct(StringComparer.Ordinal)
                .ToArray());

    private static LearningTrainingItem BuildPolicyRuntimeBridgeItem(
        DesignConversionDpoTrainingRecord row,
        string policyCheckpointPath,
        string policyCheckpointSha256)
    {
        var prompt = BuildPolicyRuntimeBridgePrompt(row, policyCheckpointPath, policyCheckpointSha256);
        var chosen = SerializePolicySideText(row.Chosen.Html, row.Chosen.Css);
        var rejected = SerializePolicySideText(row.Rejected.Html, row.Rejected.Css);

        return new LearningTrainingItem(
            ItemId: $"{HashText(row.RecordId)}:dpo-policy-runtime-bridge",
            Prompt: prompt,
            ChosenResponse: chosen,
            RejectedResponse: rejected);
    }

    private static string BuildPolicyRuntimeBridgePrompt(
        DesignConversionDpoTrainingRecord row,
        string policyCheckpointPath,
        string policyCheckpointSha256)
        => DesignConversionDpoDatasetBuilder.BuildOptimizerPromptPayload(
            row,
            DesignConversionMachine.DomainTag);

    private static bool IsMissingSignalValue(string? value)
        => DesignConversionDpoSourceEncoderContractBuilder.IsMissingSignalValue(value);

    private static string SanitizeSignalScalar(string? value, int maxLength)
        => DesignConversionDpoSourceEncoderContractBuilder.SanitizeScalar(
            value,
            maxLength);

    private sealed record BridgeLocalAblationQualityGate(
        int EvaluatedRecordCount,
        double PreferredScore,
        double BaselineScore,
        double ScoreDelta,
        double Threshold,
        bool PassesQualityGate,
        string GateReason);

    private static IReadOnlyList<string> BuildBridgeEvidenceRefs(
        int sourceRecordCount,
        int bridgePreferenceCount,
        int dpoStepCount,
        string policyCheckpointPath,
        string policyCheckpointSha256,
        DesignConversionDpoPolicyBaseIdentity identity,
        string configSha256,
        string initialWeightSha256,
        DesignConversionDpoPolicyCheckpointLoraContext? loraContext,
        DesignConversionDpoPolicyRandomContext randomContext,
        DesignConversionDpoRuntimeEncodingBinding? runtimeEncodingBinding = null,
        DesignConversionDpoRuntimeInferenceEncodingBinding?
            runtimeInferenceEncodingBinding = null)
    {
        var refs = new List<string>
        {
            DesignConversionMachine.DpoTrainerPolicyCheckpointFreshLoadedEvidence,
            DesignConversionMachine.DpoTrainerPolicyCheckpointConvertedEvidence,
            "DESIGN_CONVERSION_DPO_TRAINER_POLICY_CHECKPOINT_SOURCE_11_DPO",
            "DESIGN_CONVERSION_DPO_POLICY_BRIDGE_PRODUCT_READY_FALSE",
            DesignConversionMachine.DpoPolicyDirectInferenceConfiguredEvidence,
            "DESIGN_CONVERSION_DPO_POLICY_BRIDGE_PAYLOAD_FILE_PERSISTED",
            "DESIGN_CONVERSION_DPO_POLICY_BRIDGE_SOURCE_ENCODER_SIGNAL",
            "DESIGN_CONVERSION_DPO_POLICY_BASE_PROVIDER_INJECTED",
            "DESIGN_CONVERSION_DPO_POLICY_BASE_IDENTITY_VALIDATED",
            "DESIGN_CONVERSION_DPO_POLICY_BASE_MODELS_DISTINCT",
            "DESIGN_CONVERSION_DPO_POLICY_INITIAL_POLICY_REFERENCE_BITWISE_EQUAL",
            "DESIGN_CONVERSION_DPO_POLICY_REFERENCE_FROZEN_AND_UNCHANGED",
            "DESIGN_CONVERSION_DPO_POLICY_BASE_AND_INITIAL_WEIGHT_HASHES_DISTINCT",
            DesignConversionDpoTrainerRouteBGate.TrainerSourceSystem,
            DesignConversionDpoTrainerRouteBGate.StepVectorIngredient,
            DesignConversionDpoTrainerRouteBGate.GateId,
            $"dpo_policy_base_provider_kind:{identity.ProviderKind}",
            $"dpo_policy_base_configuration_kind:{identity.ConfigurationKind}",
            $"dpo_policy_base_model_id:{identity.BaseModelId}",
            $"dpo_policy_base_revision:{identity.BaseRevision}",
            $"dpo_policy_base_checkpoint_sha256:{identity.BaseCheckpointSha256}",
            $"dpo_policy_config_sha256:{configSha256}",
            $"dpo_policy_initial_weight_sha256:{initialWeightSha256}",
            $"dpo_policy_checkpoint_sha256:{policyCheckpointSha256}",
            $"dpo_policy_checkpoint_file:{Path.GetFileName(policyCheckpointPath)}",
            $"dpo_policy_bridge_source_record_count:{sourceRecordCount.ToString(CultureInfo.InvariantCulture)}",
            $"dpo_policy_bridge_preference_count:{bridgePreferenceCount.ToString(CultureInfo.InvariantCulture)}",
            $"dpo_policy_step_count:{dpoStepCount.ToString(CultureInfo.InvariantCulture)}"
        };
        refs.AddRange(
            DesignConversionDpoPolicyRandomContextContract.BuildRequiredEvidenceRefs(
                randomContext));

        if (loraContext is not null)
        {
            refs.Add(DesignConversionMachine.DpoPolicyLoraAppliedEvidence);
            refs.Add("10.LoRA:LoraApplicator");
            refs.Add($"dpo_policy_lora_rank:{loraContext.Rank.ToString(CultureInfo.InvariantCulture)}");
            refs.Add($"dpo_policy_lora_alpha:{loraContext.Alpha.ToString("0.######", CultureInfo.InvariantCulture)}");
            refs.Add($"dpo_policy_lora_dropout:{loraContext.Dropout.ToString("0.######", CultureInfo.InvariantCulture)}");
            refs.Add($"dpo_policy_lora_target_modules:{string.Join(",", loraContext.TargetModules)}");
            refs.Add($"dpo_policy_lora_adapter_count:{loraContext.AdapterCount.ToString(CultureInfo.InvariantCulture)}");
        }

        if (runtimeEncodingBinding is not null)
        {
            refs.Add("DESIGN_CONVERSION_DPO_CONTINUATION_RUNTIME_ENCODING_BOUND");
            refs.Add($"dpo_runtime_encoding_schema:{runtimeEncodingBinding.BindingSchema}");
            refs.Add($"dpo_runtime_encoding_source_descriptor_schema:{runtimeEncodingBinding.SourceDescriptorSchema}");
            refs.Add($"dpo_runtime_encoding_token_mapping:{runtimeEncodingBinding.PolicyTokenMapping}");
            refs.Add($"dpo_runtime_encoding_token_id_min:{runtimeEncodingBinding.TokenIdMinimum.ToString(CultureInfo.InvariantCulture)}");
            refs.Add($"dpo_runtime_encoding_token_id_max:{runtimeEncodingBinding.TokenIdMaximum.ToString(CultureInfo.InvariantCulture)}");
            refs.Add($"dpo_runtime_encoding_context_length:{runtimeEncodingBinding.ContextLength.ToString(CultureInfo.InvariantCulture)}");
            refs.Add($"dpo_runtime_encoding_tokenizer_sha256:{runtimeEncodingBinding.TokenizerArtifactSha256}");
            refs.Add($"dpo_runtime_encoding_vocabulary_sha256:{runtimeEncodingBinding.TokenizerVocabularyIdentitySha256}");
        }

        if (runtimeInferenceEncodingBinding is not null)
        {
            refs.Add("DESIGN_CONVERSION_DPO_CONTINUATION_RUNTIME_ENCODING_BOUND");
            refs.Add($"dpo_runtime_encoding_schema:{runtimeInferenceEncodingBinding.BindingSchema}");
            refs.Add($"dpo_runtime_encoding_source_descriptor_schema:{runtimeInferenceEncodingBinding.SourceDescriptorSchema}");
            refs.Add($"dpo_runtime_encoding_token_mapping:{runtimeInferenceEncodingBinding.PolicyTokenMapping}");
            refs.Add($"dpo_runtime_encoding_token_id_min:{runtimeInferenceEncodingBinding.TokenIdMinimum.ToString(CultureInfo.InvariantCulture)}");
            refs.Add($"dpo_runtime_encoding_token_id_max:{runtimeInferenceEncodingBinding.TokenIdMaximum.ToString(CultureInfo.InvariantCulture)}");
            refs.Add($"dpo_runtime_encoding_context_length:{runtimeInferenceEncodingBinding.ContextLength.ToString(CultureInfo.InvariantCulture)}");
            refs.Add($"dpo_runtime_encoding_tokenizer_sha256:{runtimeInferenceEncodingBinding.TokenizerArtifactSha256}");
            refs.Add($"dpo_runtime_encoding_vocabulary_sha256:{runtimeInferenceEncodingBinding.TokenizerVocabularyIdentitySha256}");
        }

        return refs.Distinct(StringComparer.Ordinal).ToArray();
    }

    private static OperationResult<DesignConversionDpoPolicyCheckpointRef> BuildPolicyCheckpointRef(
        int sourceRecordCount,
        int bridgePreferenceCount,
        int DpoStepCount,
        string policyCheckpointFileName,
        string policyCheckpointSha256,
        int policyCheckpointByteLength,
        ModelConfig config,
        DesignConversionDpoPolicyBaseIdentity identity,
        string configSha256,
        string initialWeightSha256,
        DesignConversionDpoPolicyCheckpointLoraContext? loraContext,
        DesignConversionDpoPolicyRandomContext randomContext,
        IReadOnlyList<string> bridgeEvidenceRefs,
        DesignConversionDpoLegalTeacherAdmissionCore legalTeacherAdmissionCore,
        DesignConversionDpoHeldOutEvaluation? evaluatedHeldOutAcceptanceAuthority = null)
    {
        var finalEvidenceRefs = MergeEvidenceRefs(bridgeEvidenceRefs, [
            "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_REF_TYPED",
            "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_REF_NO_ABSOLUTE_PATH",
            "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_REF_PAYLOAD_FILE_SIBLING",
            "DESIGN_CONVERSION_DPO_LEGAL_TEACHER_ADMISSION_COMMITMENT_TYPED",
            "DESIGN_CONVERSION_DPO_LEGAL_TEACHER_ADMISSION_COMMITMENT_NO_RAW_IDENTITY"
        ]);
        var commitment = DesignConversionDpoLegalTeacherAdmissionCommitmentCodec.FinalizeCommitment(
            legalTeacherAdmissionCore,
            finalEvidenceRefs);
        if (!commitment.IsSuccess || commitment.Value is null)
            return commitment.MapError<DesignConversionDpoPolicyCheckpointRef>();

        var checkpointRef = new DesignConversionDpoPolicyCheckpointRef(
            ArtifactKind: DesignConversionDpoTrainerRouteBGate.PolicyCheckpointArtifactKind,
            PolicyCheckpointFileName: policyCheckpointFileName,
            Sha256: policyCheckpointSha256,
            SourceSystem: DesignConversionDpoTrainerRouteBGate.TrainerSourceSystem,
            SourceMechanism: DesignConversionDpoTrainerRouteBGate.FreshLoadedSourceMechanism,
            RuntimeBridgeKind: DirectInferenceRuntimeBridgeKind,
            FreshLoadVerified: true,
            ConvertedToRuntimeWeights: true,
            SourceRecordCount: sourceRecordCount,
            BridgePreferenceCount: bridgePreferenceCount,
            DpoStepCount: DpoStepCount,
            TrainingContext: (new DesignConversionDpoPolicyCheckpointTrainingContext(
                ConfigKind: identity.ConfigurationKind,
                VocabSize: config.VocabSize,
                ContextLength: config.ContextLen,
                HiddenDim: config.HiddenDim,
                NumLayers: config.NumLayers,
                NumHeads: config.NumHeads,
                NumKvHeads: config.NumKvHeads,
                FfnHiddenDim: config.FfnHiddenDim,
                RawPayloadPersisted: true,
                AbsolutePathPersisted: false,
                SchemaVersion: 4,
                RopeTheta: config.RopeTheta,
                RmsNormEps: config.RmsNormEps,
                ProviderKind: identity.ProviderKind,
                BaseModelId: identity.BaseModelId,
                BaseRevision: identity.BaseRevision,
                BaseCheckpointSha256: identity.BaseCheckpointSha256,
                ConfigSha256: configSha256,
                InitialWeightSha256: initialWeightSha256,
                Lora: loraContext)) with
                {
                    LegalTeacherAdmissionCommitment = commitment.Value,
                    RandomContext = randomContext
                },
            EvidenceRefs: finalEvidenceRefs,
            PolicyCheckpointByteLength: policyCheckpointByteLength)
        {
            EvaluatedHeldOutAcceptanceAuthority = evaluatedHeldOutAcceptanceAuthority is null
                ? null
                : evaluatedHeldOutAcceptanceAuthority with
                {
                    G3Metrics = evaluatedHeldOutAcceptanceAuthority.G3Metrics is null
                        ? null
                        : evaluatedHeldOutAcceptanceAuthority.G3Metrics with { },
                    CycleAccuracyLineage = evaluatedHeldOutAcceptanceAuthority.CycleAccuracyLineage is null
                        ? null
                        : evaluatedHeldOutAcceptanceAuthority.CycleAccuracyLineage with { }
                }
        };
        return OperationResult<DesignConversionDpoPolicyCheckpointRef>.Ok(checkpointRef);
    }

    private static DesignConversionMachineCheckpoint AddDpoPolicyCheckpointEvidence(
        DesignConversionMachineCheckpoint checkpoint,
        IReadOnlyList<string> bridgeEvidenceRefs,
        DesignConversionDpoPolicyCheckpointRef policyCheckpointRef,
        double? evaluatedAccuracyAfterLastBatch = null,
        IReadOnlyList<string>? evaluatedAccuracyEvidenceRefs = null)
    {
        var evidenceRefs = MergeEvidenceRefs(checkpoint.EvidenceRefs, bridgeEvidenceRefs);
        if (evaluatedAccuracyAfterLastBatch.HasValue)
        {
            if (!double.IsFinite(evaluatedAccuracyAfterLastBatch.Value) ||
                evaluatedAccuracyAfterLastBatch.Value is < 0d or > 1d ||
                evaluatedAccuracyEvidenceRefs is null ||
                evaluatedAccuracyEvidenceRefs.Count == 0)
            {
                throw new InvalidOperationException(
                    "Evaluated runtime checkpoint accuracy evidence is invalid.");
            }

            evidenceRefs = MergeEvidenceRefs(
                evidenceRefs.Where(evidence =>
                    !string.Equals(
                        evidence,
                        DesignConversionMachine.HeldOutAccuracyNotMeasuredEvidence,
                        StringComparison.Ordinal) &&
                    !string.Equals(
                        evidence,
                        DesignConversionMachine.LocalPreferenceFitSignalOnlyEvidence,
                        StringComparison.Ordinal)),
                evaluatedAccuracyEvidenceRefs);
        }

        return checkpoint with
        {
            CriticUnit = AddDpoPolicyCheckpointEvidence(checkpoint.CriticUnit, bridgeEvidenceRefs),
            ImproverUnit = AddDpoPolicyCheckpointEvidence(checkpoint.ImproverUnit, bridgeEvidenceRefs),
            AccuracyAfterLastBatch =
                evaluatedAccuracyAfterLastBatch ?? checkpoint.AccuracyAfterLastBatch,
            EvidenceRefs = evidenceRefs,
            DpoPolicyCheckpointRef = policyCheckpointRef
        };
    }

    private static string[] BuildHeldOutRuntimeCheckpointAccuracyEvidence(
        DesignConversionDpoHeldOutEvaluation evaluation)
    {
        if (evaluation is null ||
            !double.IsFinite(evaluation.Accuracy) ||
            evaluation.Accuracy is < 0d or > 1d ||
            evaluation.EvaluatedHeldOutRecordCount <= 0 ||
            string.IsNullOrWhiteSpace(evaluation.HeldOutEvaluationInputSha256))
        {
            throw new InvalidOperationException(
                "Evaluated held-out runtime checkpoint accuracy source is invalid.");
        }

        return
        [
            DesignConversionMachine.DpoRuntimeCheckpointAccuracyFromHeldOutEvaluationEvidence,
            DesignConversionMachine.DpoRuntimeCheckpointAccuracyCurrentCycleHeldOutEvaluationScopeEvidence,
            $"dpo_runtime_checkpoint_accuracy_value:{evaluation.Accuracy.ToString("R", CultureInfo.InvariantCulture)}",
            $"dpo_runtime_checkpoint_accuracy_evaluated_heldout_count:{evaluation.EvaluatedHeldOutRecordCount.ToString(CultureInfo.InvariantCulture)}",
            $"dpo_runtime_checkpoint_accuracy_input_sha256:{evaluation.HeldOutEvaluationInputSha256}"
        ];
    }

    private static DesignConversionRuntimeUnitCheckpoint AddDpoPolicyCheckpointEvidence(
        DesignConversionRuntimeUnitCheckpoint unit,
        IReadOnlyList<string> bridgeEvidenceRefs)
    {
        var weights = unit.LearnedWeights ?? [];
        if (weights.Count == 0)
            return unit;

        return unit with
        {
            ImplementationStatus = unit.ImplementationStatus.Contains("dpo_trainer_policy_checkpoint", StringComparison.Ordinal)
                ? unit.ImplementationStatus
                : $"{unit.ImplementationStatus}_with_dpo_trainer_policy_checkpoint_converted",
            EvidenceRefs = MergeEvidenceRefs(unit.EvidenceRefs, bridgeEvidenceRefs),
            LearnedWeights = weights
                .Select(weight => weight with
                {
                    Derivation = AppendDpoTrainerDerivation(weight.Derivation),
                    EvidenceRefs = MergeEvidenceRefs(weight.EvidenceRefs, bridgeEvidenceRefs)
                })
                .ToArray()
        };
    }

    private static string AppendDpoTrainerDerivation(string derivation)
    {
        return DesignConversionDpoTrainerRouteBGate.AppendFreshLoadedPolicyCheckpointDerivation(derivation);
    }

    private static string[] MergeEvidenceRefs(
        IEnumerable<string>? current,
        IEnumerable<string>? additions)
        => (current ?? [])
            .Concat(additions ?? [])
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static async Task<OperationResult<DesignConversionMachineLoadResult>>
        PublishRuntimeTransactionWithFinalPermitAsync(
            string policyCheckpointPath,
            string policyCheckpointFileName,
            byte[] policyCheckpointPayload,
            string runtimeCheckpointPath,
            DesignConversionMachineCheckpoint bridgedCheckpoint,
            int dpoStepCount,
            IReadOnlyList<string> bridgeEvidenceRefs,
            Func<OperationResult<IDisposable>> finalPermitFactory,
            CancellationToken cancellationToken)
        => await DesignConversionDpoRuntimeCheckpointPublisher.PublishWithFinalPermitAsync(
            policyCheckpointPath,
            policyCheckpointFileName,
            policyCheckpointPayload,
            runtimeCheckpointPath,
            bridgedCheckpoint,
            dpoStepCount,
            bridgeEvidenceRefs,
            RuntimeTransactionStageObserverForTesting,
            finalPermitFactory,
            cancellationToken);

    private static string ComputeSha256(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static string NormalizeUnitId(string unitId)
        => DesignConversionDpoUnits.TryNormalize(unitId, out var normalized)
            ? normalized
            : unitId;

    private static string HashText(string? value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value ?? string.Empty))).ToLowerInvariant();

    private static string BuildEvaluatedScratchDirectoryName(string runId)
        => $".dpoe-{HashText(runId)[..6]}-{Guid.NewGuid().ToString("N")[..12]}";

    private static string ToSafeFileStem(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string((string.IsNullOrWhiteSpace(value) ? "design-conversion-dpo-policy-bridge" : value)
            .Select(ch => invalid.Contains(ch) || char.IsWhiteSpace(ch) ? '-' : ch)
            .ToArray()).Trim('-');
        return string.IsNullOrWhiteSpace(cleaned)
            ? "design-conversion-dpo-policy-bridge"
            : cleaned;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ScratchFileAttributeTagInfo
    {
        internal uint FileAttributes;
        internal uint ReparseTag;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ScratchFileDispositionInfo
    {
        [MarshalAs(UnmanagedType.U1)]
        internal byte DeleteFile;
    }

    [DllImport("kernel32.dll", EntryPoint = "CreateFileW", SetLastError = true,
        CharSet = CharSet.Unicode)]
    private static extern SafeFileHandle CreateFileWindows(
        string fileName,
        uint desiredAccess,
        uint shareMode,
        IntPtr securityAttributes,
        uint creationDisposition,
        uint flagsAndAttributes,
        IntPtr templateFile);

    [DllImport("kernel32.dll", EntryPoint = "GetFileInformationByHandleEx", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetFileInformationByHandleExWindows(
        SafeFileHandle fileHandle,
        int fileInformationClass,
        out ScratchFileAttributeTagInfo fileInformation,
        uint bufferSize);

    [DllImport("kernel32.dll", EntryPoint = "SetFileInformationByHandle", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetFileInformationByHandleWindows(
        SafeFileHandle fileHandle,
        int fileInformationClass,
        ref ScratchFileDispositionInfo fileInformation,
        uint bufferSize);
}
