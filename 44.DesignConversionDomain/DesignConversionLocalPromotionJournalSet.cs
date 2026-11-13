// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using Microsoft.Win32.SafeHandles;

internal enum DesignConversionLocalPromotionJournalKind
{
    Operation,
    Projection
}

internal enum DesignConversionLocalPromotionJournalRotationStage
{
    PendingPublished,
    OperationSegmentCopied,
    ProjectionSegmentCopied,
    SealPublished,
    OperationActiveDeleted,
    ProjectionActiveDeleted,
    PendingDeleted
}

internal enum DesignConversionLocalPromotionJournalAppendStage
{
    PendingPublished
}

internal sealed record DesignConversionLocalPromotionJournalLine(
    string Text,
    int Generation,
    long Ordinal,
    bool IsActive);

internal sealed record DesignConversionLocalPromotionJournalRead(
    IReadOnlyList<DesignConversionLocalPromotionJournalLine> OperationLines,
    IReadOnlyList<DesignConversionLocalPromotionJournalLine> ProjectionLines,
    long ActiveOperationByteLength,
    long ActiveProjectionByteLength,
    int ActiveGeneration,
    string OperationTerminalRecordSha256);

/// <summary>
/// Coordinates the two local-promotion journals as one crash-safe generation.
/// The caller owns the in-process mutation gate and cross-process mutation lease.
/// </summary>
internal sealed class DesignConversionLocalPromotionJournalSet
{
    internal const string PendingFileName =
        "training-intake-local-promotion-journals.rotation.pending.json";
    internal const string AppendPendingFileName =
        "training-intake-local-promotion-journals.append.pending.json";
    internal const string MetadataSchemaVersion =
        "design-conversion-local-promotion-journal-rotation-v1";
    internal const int DefaultMaximumSealedSegmentCount = 256;
    internal const int MaximumMetadataByteLength = 64 * 1024;
    internal const int MaximumTraversedRootEntries = 32 * 1024;

    private const string Sha256Prefix = "sha256:";
    private const string SealStem =
        "training-intake-local-promotion-journals.g";
    private const string OperationSegmentStem =
        "training-intake-local-promotion-operations.g";
    private const string ProjectionSegmentStem =
        "training-intake-local-promotions.g";
    private const string AppendTempSuffix = ".append-publication.tmp";
    private const string PublicationStageSuffix = ".transaction-stage.tmp";
    private const int GenerationDigits = 10;
    private const int FileStreamInfo = 7;

    private static readonly Encoding StrictUtf8 = new UTF8Encoding(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);
    private static readonly JsonSerializerOptions JsonOptions = new(
        DesignConversionCheckpointJsonOptions.Standard)
    {
        WriteIndented = false
    };

    private readonly string _rootDirectory;
    private readonly string _operationActiveLeaf;
    private readonly string _projectionActiveLeaf;
    private readonly string _mutationLockLeaf;
    private readonly long _operationActiveMaximumBytes;
    private readonly long _projectionActiveMaximumBytes;
    private readonly int _maximumSealedSegmentCount;
    private readonly long _maximumAggregateSealedBytes;
    private readonly Action<DesignConversionLocalPromotionJournalRotationStage>?
        _rotationObserver;
    private readonly Action? _appendPendingPublicationStartedObserver;
    private readonly Action<DesignConversionLocalPromotionJournalAppendStage>?
        _appendObserver;
    private readonly Action? _appendBeforeDestinationSnapshotObserver;
    private DesignConversionDeterministicMutationLease? _boundMutationLease;

    private sealed class MutationLeaseBinding(
        DesignConversionLocalPromotionJournalSet owner,
        DesignConversionDeterministicMutationLease mutationLease) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            if (!ReferenceEquals(owner._boundMutationLease, mutationLease))
            {
                throw new InvalidOperationException(
                    "The local-promotion journal mutation lease binding changed unexpectedly.");
            }
            owner._boundMutationLease = null;
        }
    }

    private sealed record MemberSeal(
        bool Present,
        string SegmentFileName,
        long ByteLength,
        string Sha256);

    private sealed record GenerationSeal(
        string SchemaVersion,
        int Version,
        int Generation,
        MemberSeal Operation,
        MemberSeal Projection,
        string PreviousSealSha256,
        string OperationTerminalRecordSha256,
        string SealSha256);

    private sealed record PendingMember(
        bool Present,
        string ActiveFileName,
        string SegmentFileName,
        long ByteLength,
        string Sha256,
        ulong VolumeSerialNumber,
        ulong FileIdLow,
        ulong FileIdHigh);

    private sealed record PendingRotation(
        string SchemaVersion,
        int Version,
        int Generation,
        PendingMember Operation,
        PendingMember Projection,
        string PreviousSealSha256,
        string OperationTerminalRecordSha256,
        string PendingSha256);

    private sealed record PendingAppend(
        string SchemaVersion,
        int Version,
        DesignConversionLocalPromotionJournalKind Kind,
        PendingMember Source,
        string TempFileName,
        long TargetByteLength,
        string TargetSha256,
        string PendingSha256);

    private sealed record HeldFile(
        string LeafName,
        FileStream Stream,
        DesignConversionFileIdentity Identity,
        long Length,
        string Sha256,
        byte[] Bytes) : IDisposable
    {
        public void Dispose() => Stream.Dispose();
    }

    private sealed record JournalSnapshot(
        IReadOnlyList<DesignConversionLocalPromotionJournalLine> OperationLines,
        IReadOnlyList<DesignConversionLocalPromotionJournalLine> ProjectionLines,
        HeldFile? ActiveOperation,
        HeldFile? ActiveProjection,
        int NextGeneration,
        long AggregateSealedBytes,
        string PreviousSealSha256,
        string OperationTerminalRecordSha256) : IDisposable
    {
        public void Dispose()
        {
            ActiveProjection?.Dispose();
            ActiveOperation?.Dispose();
        }
    }

    private sealed class JournalUtf8Exception(
        DesignConversionLocalPromotionJournalKind kind,
        DecoderFallbackException innerException)
        : IOException("A local-promotion journal is not valid UTF-8.", innerException)
    {
        internal DesignConversionLocalPromotionJournalKind Kind { get; } = kind;
    }

    private sealed class ProjectionReadException(Exception innerException)
        : IOException(
            "The local-promotion projection journal could not be read.",
            innerException);

    internal DesignConversionLocalPromotionJournalSet(
        string rootDirectory,
        string operationActiveLeaf,
        string projectionActiveLeaf,
        string mutationLockLeaf,
        long operationActiveMaximumBytes,
        long projectionActiveMaximumBytes,
        int maximumSealedSegmentCount = DefaultMaximumSealedSegmentCount,
        long? maximumAggregateSealedBytes = null,
        Action<DesignConversionLocalPromotionJournalRotationStage>?
            rotationObserver = null,
        Action? appendPendingPublicationStartedObserver = null,
        Action<DesignConversionLocalPromotionJournalAppendStage>?
            appendObserver = null,
        Action? appendBeforeDestinationSnapshotObserver = null)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory))
            throw new ArgumentException("A journal root is required.", nameof(rootDirectory));
        if (operationActiveMaximumBytes <= 0)
            throw new ArgumentOutOfRangeException(nameof(operationActiveMaximumBytes));
        if (projectionActiveMaximumBytes <= 0)
            throw new ArgumentOutOfRangeException(nameof(projectionActiveMaximumBytes));
        if (maximumSealedSegmentCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(maximumSealedSegmentCount));

        _rootDirectory = Path.GetFullPath(rootDirectory);
        _operationActiveLeaf = RequireDirectLeaf(operationActiveLeaf);
        _projectionActiveLeaf = RequireDirectLeaf(projectionActiveLeaf);
        _mutationLockLeaf = RequireDirectLeaf(mutationLockLeaf);
        _operationActiveMaximumBytes = operationActiveMaximumBytes;
        _projectionActiveMaximumBytes = projectionActiveMaximumBytes;
        _maximumSealedSegmentCount = maximumSealedSegmentCount;
        _maximumAggregateSealedBytes = maximumAggregateSealedBytes ??
            ComputeDefaultArchiveMaximum(
                operationActiveMaximumBytes,
                projectionActiveMaximumBytes,
                maximumSealedSegmentCount);
        if (_maximumAggregateSealedBytes <= 0)
            throw new ArgumentOutOfRangeException(nameof(maximumAggregateSealedBytes));
        _rotationObserver = rotationObserver;
        _appendPendingPublicationStartedObserver =
            appendPendingPublicationStartedObserver;
        _appendObserver = appendObserver;
        _appendBeforeDestinationSnapshotObserver =
            appendBeforeDestinationSnapshotObserver;
    }

    internal IDisposable BindMutationLease(DesignConversionDeterministicMutationLease mutationLease)
    {
        ArgumentNullException.ThrowIfNull(mutationLease);
        if (_boundMutationLease is not null)
        {
            throw new InvalidOperationException(
                "A local-promotion journal mutation lease is already bound.");
        }
        if (!mutationLease.Stream.CanRead || !mutationLease.Stream.CanWrite)
        {
            throw new ArgumentException(
                "The local-promotion journal mutation lease must be readable and writable.",
                nameof(mutationLease));
        }
        _boundMutationLease = mutationLease;
        return new MutationLeaseBinding(this, mutationLease);
    }

    internal async Task<OperationResult<DesignConversionLocalPromotionJournalRead>>
        ReadAsync(CancellationToken ct)
    {
        var loaded = await LoadSnapshotAsync(requireNoPending: true, ct);
        if (!loaded.IsSuccess)
            return loaded.MapError<DesignConversionLocalPromotionJournalRead>();
        using var snapshot = loaded.Value!;
        return OperationResult<DesignConversionLocalPromotionJournalRead>.Ok(
            new DesignConversionLocalPromotionJournalRead(
                snapshot.OperationLines,
                snapshot.ProjectionLines,
                snapshot.ActiveOperation?.Length ?? 0L,
                snapshot.ActiveProjection?.Length ?? 0L,
                snapshot.NextGeneration,
                snapshot.OperationTerminalRecordSha256));
    }

    internal async Task<OperationResult<bool>> RecoverAsync(CancellationToken ct)
    {
        try
        {
            using var directory = DesignConversionHandleBoundDirectory.AcquireOrCreate(
                _rootDirectory,
                beforeIo: null);
            ValidateBoundMutationLease(directory);
            var rotationPendingExists = await LeafExistsAsync(
                directory,
                PendingFileName,
                ct);
            var appendPendingExists = await LeafExistsAsync(
                directory,
                AppendPendingFileName,
                ct);
            if (rotationPendingExists && appendPendingExists)
                return StateInvalid<bool>();
            if (!await DeleteOwnedPublicationStageAsync(
                    directory,
                    PendingFileName,
                    MaximumMetadataByteLength) ||
                !await DeleteOwnedPublicationStageAsync(
                    directory,
                    AppendPendingFileName,
                    MaximumMetadataByteLength))
            {
                return StateInvalid<bool>();
            }
            var appendRecovered = appendPendingExists
                ? await RecoverAppendAsync(directory)
                : OperationResult<bool>.Ok(false);
            if (!appendRecovered.IsSuccess)
                return appendRecovered;
            using var pendingFile = await ReadHeldFileAsync(
                directory,
                PendingFileName,
                MaximumMetadataByteLength,
                required: false,
                rotationPendingExists ? CancellationToken.None : ct);
            if (pendingFile is null)
            {
                var clean = await LoadSnapshotAsync(requireNoPending: true, ct);
                if (!clean.IsSuccess)
                    return clean.MapError<bool>();
                clean.Value!.Dispose();
                return OperationResult<bool>.Ok(appendRecovered.Value);
            }

            var pending = DeserializeCanonicalPending(pendingFile.Bytes);
            if (pending is null || !PendingNamesMatchExpected(pending))
                return StateInvalid<bool>();
            if (!await DeleteOwnedPublicationStageAsync(
                    directory,
                    pending.Operation.SegmentFileName,
                    _operationActiveMaximumBytes) ||
                !await DeleteOwnedPublicationStageAsync(
                    directory,
                    pending.Projection.SegmentFileName,
                    _projectionActiveMaximumBytes) ||
                !await DeleteOwnedPublicationStageAsync(
                    directory,
                    BuildSealFileName(pending.Generation),
                    MaximumMetadataByteLength))
            {
                return StateInvalid<bool>();
            }
            var chain = await LoadSnapshotAsync(
                requireNoPending: false,
                ct,
                pending);
            if (!chain.IsSuccess)
                return chain.MapError<bool>();
            using var snapshot = chain.Value!;
            if (pending.Generation != snapshot.NextGeneration &&
                pending.Generation != snapshot.NextGeneration - 1)
            {
                return StateInvalid<bool>();
            }
            var expectedTerminal = snapshot.ActiveOperation is null
                ? snapshot.OperationTerminalRecordSha256
                : ExtractTerminalOperationSha256(snapshot.ActiveOperation.Bytes);
            if (pending.Generation == snapshot.NextGeneration &&
                !string.Equals(
                    pending.PreviousSealSha256,
                    snapshot.PreviousSealSha256,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    pending.OperationTerminalRecordSha256,
                    expectedTerminal,
                    StringComparison.Ordinal))
            {
                return StateInvalid<bool>();
            }

            using var sealFile = await ReadHeldFileAsync(
                directory,
                BuildSealFileName(pending.Generation),
                MaximumMetadataByteLength,
                required: false,
                CancellationToken.None);
            if (sealFile is null)
            {
                if (pending.Generation != snapshot.NextGeneration)
                {
                    return StateInvalid<bool>();
                }

                snapshot.Dispose();
                if (!await ActiveMembersMatchPendingAsync(
                        directory,
                        pending,
                        CancellationToken.None))
                {
                    return StateInvalid<bool>();
                }

                if (!await DeleteOwnedOrAbsentSegmentAsync(
                        directory,
                        pending.Operation,
                        CancellationToken.None) ||
                    !await DeleteOwnedOrAbsentSegmentAsync(
                        directory,
                        pending.Projection,
                        CancellationToken.None))
                {
                    return StateInvalid<bool>();
                }
            }
            else
            {
                var seal = DeserializeCanonicalSeal(sealFile.Bytes);
                if (seal is null || !SealMatchesPending(seal, pending))
                    return StateInvalid<bool>();
                snapshot.Dispose();
                if (!await DeleteMatchingActiveResidueAsync(
                        directory,
                        pending.Operation,
                        DesignConversionLocalPromotionJournalRotationStage
                            .OperationActiveDeleted) ||
                    !await DeleteMatchingActiveResidueAsync(
                        directory,
                        pending.Projection,
                        DesignConversionLocalPromotionJournalRotationStage
                            .ProjectionActiveDeleted))
                {
                    return StateInvalid<bool>();
                }
            }

            var pendingIdentity = pendingFile.Identity;
            pendingFile.Dispose();
            directory.DeleteIfRegular(PendingFileName, pendingIdentity);
            _rotationObserver?.Invoke(
                DesignConversionLocalPromotionJournalRotationStage.PendingDeleted);
            return OperationResult<bool>.Ok(true);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Cancelled<bool>();
        }
        catch (Exception exception) when (IsStorageException(exception))
        {
            return StateInvalid<bool>();
        }
    }

    internal async Task<OperationResult<bool>> EnsureCapacityAsync(
        long operationReservationBytes,
        long projectionReservationBytes,
        string quotaErrorCode,
        string quotaSafeMessage,
        CancellationToken ct)
    {
        if (operationReservationBytes < 0 || projectionReservationBytes < 0)
            return StateInvalid<bool>();
        if (operationReservationBytes > _operationActiveMaximumBytes ||
            projectionReservationBytes > _projectionActiveMaximumBytes)
        {
            return Quota<bool>(quotaSafeMessage, quotaErrorCode);
        }

        var recovered = await RecoverAsync(ct);
        if (!recovered.IsSuccess)
        {
            return IsArchiveQuota(recovered.ErrorCode)
                ? Quota<bool>(quotaSafeMessage, quotaErrorCode)
                : recovered;
        }
        var loaded = await LoadSnapshotAsync(requireNoPending: true, ct);
        if (!loaded.IsSuccess)
        {
            return IsArchiveQuota(loaded.ErrorCode)
                ? Quota<bool>(quotaSafeMessage, quotaErrorCode)
                : loaded.MapError<bool>();
        }
        using var snapshot = loaded.Value!;
        if (FitsActive(snapshot, operationReservationBytes, projectionReservationBytes))
            return OperationResult<bool>.Ok(false);
        if (snapshot.ActiveOperation is null && snapshot.ActiveProjection is null)
            return Quota<bool>(quotaSafeMessage, quotaErrorCode);
        if (snapshot.NextGeneration >= _maximumSealedSegmentCount ||
            !TryAdd(
                snapshot.AggregateSealedBytes,
                snapshot.ActiveOperation?.Length ?? 0L,
                out var aggregate) ||
            !TryAdd(
                aggregate,
                snapshot.ActiveProjection?.Length ?? 0L,
                out aggregate) ||
            aggregate > _maximumAggregateSealedBytes)
        {
            return Quota<bool>(quotaSafeMessage, quotaErrorCode);
        }

        ct.ThrowIfCancellationRequested();
        var rotated = await RotateAsync(snapshot, CancellationToken.None);
        if (!rotated.IsSuccess)
            return rotated;

        var recomputed = await LoadSnapshotAsync(
            requireNoPending: true,
            CancellationToken.None);
        if (!recomputed.IsSuccess)
        {
            return IsArchiveQuota(recomputed.ErrorCode)
                ? Quota<bool>(quotaSafeMessage, quotaErrorCode)
                : recomputed.MapError<bool>();
        }
        using var after = recomputed.Value!;
        return FitsActive(after, operationReservationBytes, projectionReservationBytes)
            ? OperationResult<bool>.Ok(true)
            : Quota<bool>(quotaSafeMessage, quotaErrorCode);
    }

    internal async Task<OperationResult<long>> AppendLineAsync(
        DesignConversionLocalPromotionJournalKind kind,
        string line,
        CancellationToken ct)
    {
        if (line is null || line.IndexOfAny(['\r', '\n']) >= 0)
            return StateInvalid<long>();
        var leaf = kind == DesignConversionLocalPromotionJournalKind.Operation
            ? _operationActiveLeaf
            : _projectionActiveLeaf;
        var maximum = kind == DesignConversionLocalPromotionJournalKind.Operation
            ? _operationActiveMaximumBytes
            : _projectionActiveMaximumBytes;
        var framed = StrictUtf8.GetBytes(line + "\r\n");
        if (framed.LongLength > maximum)
            return StateInvalid<long>();

        var recovered = await RecoverAsync(ct);
        if (!recovered.IsSuccess)
            return recovered.MapError<long>();
        try
        {
            using var directory = DesignConversionHandleBoundDirectory.AcquireOrCreate(
                _rootDirectory,
                beforeIo: null);
            ValidateBoundMutationLease(directory);
            HeldFile? source;
            try
            {
                source = await ReadHeldFileAsync(
                    directory,
                    leaf,
                    maximum,
                    required: false,
                    ct,
                    allowEmpty: kind ==
                        DesignConversionLocalPromotionJournalKind.Projection);
            }
            catch (Exception exception) when (
                IsAppendPublicationWriteFailure(exception))
            {
                return LocalArtifactDbWriteFailed<long>();
            }
            using (source)
            {
            var priorLength = source?.Length ?? 0L;
            if (!TryAdd(priorLength, framed.LongLength, out var projected) ||
                projected > maximum || projected > int.MaxValue)
            {
                return StateInvalid<long>();
            }

            var tempLeaf = leaf + AppendTempSuffix;
            if (await LeafExistsAsync(directory, AppendPendingFileName, ct) ||
                await LeafExistsAsync(directory, tempLeaf, ct))
            {
                return StateInvalid<long>();
            }
            using var targetHash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            if (source is not null)
                targetHash.AppendData(source.Bytes);
            targetHash.AppendData(framed);
            var destinationExpectation = source is null
                ? DesignConversionHandleBoundDestinationExpectation.Absent
                : DesignConversionHandleBoundDestinationExpectation.Exact(
                    source.Identity,
                    source.Length,
                    source.Sha256);
            var pending = new PendingAppend(
                MetadataSchemaVersion,
                1,
                kind,
                BuildPendingMember(source, leaf, tempLeaf),
                tempLeaf,
                projected,
                $"{Sha256Prefix}{Convert.ToHexString(targetHash.GetHashAndReset()).ToLowerInvariant()}",
                string.Empty);
            pending = pending with { PendingSha256 = BuildAppendPendingSha256(pending) };
            ct.ThrowIfCancellationRequested();
            try
            {
                // Тестовый сбой вводится внутри той же безопасной границы, что и реальная публикация pending-файла.
                _appendPendingPublicationStartedObserver?.Invoke();
                await PublishCanonicalFileAsync(
                    directory,
                    AppendPendingFileName,
                    JsonSerializer.SerializeToUtf8Bytes(pending, JsonOptions),
                    MaximumMetadataByteLength,
                    CancellationToken.None);
            }
            catch (Exception exception) when (
                IsAppendPublicationWriteFailure(exception))
            {
                return LocalArtifactDbWriteFailed<long>();
            }
            _appendObserver?.Invoke(
                DesignConversionLocalPromotionJournalAppendStage.PendingPublished);
            ValidateBoundMutationLease(directory);

            try
            {
                await using (var temp = directory.CreateNew(
                                 tempLeaf,
                                 FileShare.None,
                                 writeThrough: true))
                {
                    if (source is not null)
                        await temp.WriteAsync(source.Bytes, CancellationToken.None);
                    await temp.WriteAsync(framed, CancellationToken.None);
                    await temp.FlushAsync(CancellationToken.None);
                    temp.Flush(flushToDisk: true);
                    source?.Dispose();
                    ValidateBoundMutationLease(directory);
                    directory.AtomicReplaceAndFlush(
                        temp,
                        tempLeaf,
                        leaf,
                        checked((int)priorLength),
                        destinationExpectation,
                        _appendBeforeDestinationSnapshotObserver);
                }
            }
            catch (Exception exception) when (
                IsAppendPublicationWriteFailure(exception))
            {
                return LocalArtifactDbWriteFailed<long>();
            }

            using var readback = await ReadHeldFileAsync(
                directory,
                leaf,
                maximum,
                required: true,
                CancellationToken.None);
            if (readback is null || readback.Length != projected ||
                !string.Equals(readback.Sha256, pending.TargetSha256, StringComparison.Ordinal))
            {
                return StateInvalid<long>();
            }
            using var appendPending = await ReadHeldFileAsync(
                directory,
                AppendPendingFileName,
                MaximumMetadataByteLength,
                required: true,
                CancellationToken.None);
            if (appendPending is null ||
                DeserializeCanonicalAppendPending(appendPending.Bytes) != pending)
            {
                return StateInvalid<long>();
            }
            var appendPendingIdentity = appendPending.Identity;
            appendPending.Dispose();
            directory.DeleteIfRegular(
                AppendPendingFileName,
                appendPendingIdentity);
            return OperationResult<long>.Ok(projected);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Cancelled<long>();
        }
        catch (Exception exception) when (IsStorageException(exception))
        {
            return StateInvalid<long>();
        }
    }

    private async Task<OperationResult<bool>> RotateAsync(
        JournalSnapshot snapshot,
        CancellationToken ct)
    {
        try
        {
            using var directory = DesignConversionHandleBoundDirectory.AcquireOrCreate(
                _rootDirectory,
                beforeIo: null);
            ValidateBoundMutationLease(directory);
            var operation = snapshot.ActiveOperation;
            var projection = snapshot.ActiveProjection;
            if (!HeldFileStillMatches(directory, operation) ||
                !HeldFileStillMatches(directory, projection))
            {
                return StateInvalid<bool>();
            }
            ValidateBoundMutationLease(directory);

            var generation = snapshot.NextGeneration;
            var operationPending = BuildPendingMember(
                operation,
                _operationActiveLeaf,
                BuildOperationSegmentFileName(generation));
            var projectionPending = BuildPendingMember(
                projection,
                _projectionActiveLeaf,
                BuildProjectionSegmentFileName(generation));
            var terminal = operation is null
                ? snapshot.OperationTerminalRecordSha256
                : ExtractTerminalOperationSha256(operation.Bytes);
            if (operation is not null && !IsSha256(terminal))
                return StateInvalid<bool>();
            var pending = new PendingRotation(
                MetadataSchemaVersion,
                1,
                generation,
                operationPending,
                projectionPending,
                snapshot.PreviousSealSha256,
                terminal,
                string.Empty);
            pending = pending with { PendingSha256 = BuildPendingSha256(pending) };

            if (await LeafExistsAsync(directory, PendingFileName, ct) ||
                await LeafExistsAsync(directory, BuildSealFileName(generation), ct) ||
                await LeafExistsAsync(directory, operationPending.SegmentFileName, ct) ||
                await LeafExistsAsync(directory, projectionPending.SegmentFileName, ct))
            {
                return StateInvalid<bool>();
            }

            await PublishCanonicalFileAsync(
                directory,
                PendingFileName,
                JsonSerializer.SerializeToUtf8Bytes(pending, JsonOptions),
                MaximumMetadataByteLength,
                ct);
            _rotationObserver?.Invoke(
                DesignConversionLocalPromotionJournalRotationStage.PendingPublished);

            if (operation is not null)
            {
                await PublishCanonicalFileAsync(
                    directory,
                    operationPending.SegmentFileName,
                    operation.Bytes,
                    _operationActiveMaximumBytes,
                    ct);
                _rotationObserver?.Invoke(
                    DesignConversionLocalPromotionJournalRotationStage
                        .OperationSegmentCopied);
            }
            if (projection is not null)
            {
                await PublishCanonicalFileAsync(
                    directory,
                    projectionPending.SegmentFileName,
                    projection.Bytes,
                    _projectionActiveMaximumBytes,
                    ct,
                    allowEmpty: true);
                _rotationObserver?.Invoke(
                    DesignConversionLocalPromotionJournalRotationStage
                        .ProjectionSegmentCopied);
            }

            if (!HeldFileStillMatches(directory, operation) ||
                !HeldFileStillMatches(directory, projection))
            {
                return StateInvalid<bool>();
            }
            var seal = new GenerationSeal(
                MetadataSchemaVersion,
                1,
                generation,
                BuildMemberSeal(operationPending),
                BuildMemberSeal(projectionPending),
                snapshot.PreviousSealSha256,
                terminal,
                string.Empty);
            seal = seal with { SealSha256 = BuildSealSha256(seal) };
            await PublishCanonicalFileAsync(
                directory,
                BuildSealFileName(generation),
                JsonSerializer.SerializeToUtf8Bytes(seal, JsonOptions),
                MaximumMetadataByteLength,
                ct);
            _rotationObserver?.Invoke(
                DesignConversionLocalPromotionJournalRotationStage.SealPublished);

            ValidateBoundMutationLease(directory);
            operation?.Dispose();
            projection?.Dispose();
            if (operation is not null)
            {
                directory.DeleteIfRegular(_operationActiveLeaf, operation.Identity);
                _rotationObserver?.Invoke(
                    DesignConversionLocalPromotionJournalRotationStage
                        .OperationActiveDeleted);
            }
            if (projection is not null)
            {
                directory.DeleteIfRegular(_projectionActiveLeaf, projection.Identity);
                _rotationObserver?.Invoke(
                    DesignConversionLocalPromotionJournalRotationStage
                        .ProjectionActiveDeleted);
            }
            using var pendingReadback = await ReadHeldFileAsync(
                directory,
                PendingFileName,
                MaximumMetadataByteLength,
                required: true,
                ct);
            if (pendingReadback is null ||
                !pendingReadback.Bytes.AsSpan().SequenceEqual(
                    JsonSerializer.SerializeToUtf8Bytes(pending, JsonOptions)))
            {
                return StateInvalid<bool>();
            }
            var pendingReadbackIdentity = pendingReadback.Identity;
            pendingReadback.Dispose();
            directory.DeleteIfRegular(PendingFileName, pendingReadbackIdentity);
            _rotationObserver?.Invoke(
                DesignConversionLocalPromotionJournalRotationStage.PendingDeleted);
            return OperationResult<bool>.Ok(true);
        }
        catch (Exception exception) when (IsStorageException(exception))
        {
            return StateInvalid<bool>();
        }
    }

    private async Task<OperationResult<bool>> RecoverAppendAsync(
        DesignConversionHandleBoundDirectory directory)
    {
        using var pendingFile = await ReadHeldFileAsync(
            directory,
            AppendPendingFileName,
            MaximumMetadataByteLength,
            required: false,
            CancellationToken.None);
        if (pendingFile is null)
            return OperationResult<bool>.Ok(false);
        var pending = DeserializeCanonicalAppendPending(pendingFile.Bytes);
        if (pending is null || !AppendPendingNamesMatchExpected(pending))
            return StateInvalid<bool>();
        var maximum = pending.Kind ==
            DesignConversionLocalPromotionJournalKind.Operation
            ? _operationActiveMaximumBytes
            : _projectionActiveMaximumBytes;
        using var active = await ReadHeldFileAsync(
            directory,
            pending.Source.ActiveFileName,
            maximum,
            required: false,
            CancellationToken.None,
            expectedIdentity: null,
            allowEmpty: pending.Kind ==
                DesignConversionLocalPromotionJournalKind.Projection);
        using var temp = await ReadHeldFileAsync(
            directory,
            pending.TempFileName,
            maximum,
            required: false,
            CancellationToken.None,
            allowEmpty: true);
        var backupLeaf = pending.Source.ActiveFileName +
            DesignConversionHandleBoundDirectory.PublicationBackupSuffix;
        using var backup = await ReadHeldFileAsync(
            directory,
            backupLeaf,
            maximum,
            required: false,
            CancellationToken.None,
            allowEmpty: pending.Kind ==
                DesignConversionLocalPromotionJournalKind.Projection);
        var sourceMatches = PendingSourceMatches(active, pending.Source);
        var targetMatches = active is not null &&
            active.Length == pending.TargetByteLength &&
            string.Equals(
                active.Sha256,
                pending.TargetSha256,
                StringComparison.Ordinal);
        var tempMatchesTarget = temp is not null &&
            temp.Length == pending.TargetByteLength &&
            string.Equals(temp.Sha256, pending.TargetSha256, StringComparison.Ordinal);
        var backupMatchesSource = pending.Source.Present
            ? PendingSourceMatches(backup, pending.Source)
            : backup is null;
        if (sourceMatches)
        {
            if (backup is not null)
                return StateInvalid<bool>();
            if (temp is not null)
            {
                var tempIdentity = temp.Identity;
                temp.Dispose();
                directory.DeleteIfRegular(pending.TempFileName, tempIdentity);
            }
        }
        else if (targetMatches)
        {
            if (temp is not null)
                return StateInvalid<bool>();
            if (backup is not null)
            {
                if (!backupMatchesSource)
                    return StateInvalid<bool>();
                var backupIdentity = backup.Identity;
                backup.Dispose();
                directory.DeleteIfRegular(backupLeaf, backupIdentity);
            }
        }
        else if (active is null && pending.Source.Present &&
                 backupMatchesSource && tempMatchesTarget)
        {
            directory.RenameNoReplaceAndFlush(
                temp!.Stream,
                pending.TempFileName,
                pending.Source.ActiveFileName);
            temp.Dispose();
            using var completed = await ReadHeldFileAsync(
                directory,
                pending.Source.ActiveFileName,
                maximum,
                required: true,
                CancellationToken.None);
            if (completed is null || completed.Length != pending.TargetByteLength ||
                !string.Equals(
                    completed.Sha256,
                    pending.TargetSha256,
                    StringComparison.Ordinal))
            {
                return StateInvalid<bool>();
            }
            var backupIdentity = backup!.Identity;
            backup.Dispose();
            directory.DeleteIfRegular(backupLeaf, backupIdentity);
        }
        else
        {
            return StateInvalid<bool>();
        }

        var pendingIdentity = pendingFile.Identity;
        pendingFile.Dispose();
        directory.DeleteIfRegular(AppendPendingFileName, pendingIdentity);
        return OperationResult<bool>.Ok(true);
    }

    private bool AppendPendingNamesMatchExpected(PendingAppend pending)
    {
        var expectedActive = pending.Kind ==
            DesignConversionLocalPromotionJournalKind.Operation
            ? _operationActiveLeaf
            : _projectionActiveLeaf;
        return string.Equals(
                   pending.Source.ActiveFileName,
                   expectedActive,
                   StringComparison.Ordinal) &&
               string.Equals(
                   pending.TempFileName,
                   expectedActive + AppendTempSuffix,
                   StringComparison.Ordinal) &&
               string.Equals(
                   pending.Source.SegmentFileName,
                   pending.TempFileName,
                   StringComparison.Ordinal);
    }

    private static bool PendingSourceMatches(
        HeldFile? active,
        PendingMember source)
    {
        if (!source.Present)
            return active is null;
        if (active is null)
            return false;
        return active.Identity == new DesignConversionFileIdentity(
                   source.VolumeSerialNumber,
                   source.FileIdLow,
                   source.FileIdHigh) &&
               active.Length == source.ByteLength &&
               string.Equals(active.Sha256, source.Sha256, StringComparison.Ordinal);
    }

    private async Task<OperationResult<JournalSnapshot>> LoadSnapshotAsync(
        bool requireNoPending,
        CancellationToken ct,
        PendingRotation? acceptedPending = null)
    {
        HeldFile? activeOperation = null;
        HeldFile? activeProjection = null;
        try
        {
            using var directory = DesignConversionHandleBoundDirectory.AcquireExisting(
                _rootDirectory,
                beforeIo: null);
            ValidateBoundMutationLease(directory);
            if (requireNoPending &&
                (await LeafExistsAsync(directory, PendingFileName, ct) ||
                 await LeafExistsAsync(directory, AppendPendingFileName, ct)))
            {
                return StateInvalid<JournalSnapshot>();
            }

            var operationLines = new List<DesignConversionLocalPromotionJournalLine>();
            var projectionLines = new List<DesignConversionLocalPromotionJournalLine>();
            var allowedReservedLeaves = new HashSet<string>(StringComparer.Ordinal)
            {
                _operationActiveLeaf,
                _projectionActiveLeaf,
                _mutationLockLeaf
            };
            if (acceptedPending is not null)
            {
                allowedReservedLeaves.Add(PendingFileName);
                allowedReservedLeaves.Add(acceptedPending.Operation.SegmentFileName);
                allowedReservedLeaves.Add(acceptedPending.Projection.SegmentFileName);
                allowedReservedLeaves.Add(BuildSealFileName(acceptedPending.Generation));
            }

            var previousSealSha = string.Empty;
            var terminalOperationSha = string.Empty;
            long aggregateBytes = 0L;
            var generation = 0;
            for (; generation < _maximumSealedSegmentCount; generation++)
            {
                var sealLeaf = BuildSealFileName(generation);
                using var sealFile = await ReadHeldFileAsync(
                    directory,
                    sealLeaf,
                    MaximumMetadataByteLength,
                    required: false,
                    ct,
                    readOnlyHandle: true);
                if (sealFile is null)
                    break;
                allowedReservedLeaves.Add(sealLeaf);
                var seal = DeserializeCanonicalSeal(sealFile.Bytes);
                if (seal is null || seal.Generation != generation ||
                    !string.Equals(
                        seal.PreviousSealSha256,
                        previousSealSha,
                        StringComparison.Ordinal) ||
                    !string.Equals(
                        seal.Operation.SegmentFileName,
                        BuildOperationSegmentFileName(generation),
                        StringComparison.Ordinal) ||
                    !string.Equals(
                        seal.Projection.SegmentFileName,
                        BuildProjectionSegmentFileName(generation),
                        StringComparison.Ordinal))
                {
                    return StateInvalid<JournalSnapshot>();
                }

                var operationBytes = await ReadSealedMemberAsync(
                    directory,
                    seal.Operation,
                    _operationActiveMaximumBytes,
                    allowEmpty: false,
                    ct: ct);
                OperationResult<byte[]?> projectionBytes;
                try
                {
                    projectionBytes = await ReadSealedMemberAsync(
                        directory,
                        seal.Projection,
                        _projectionActiveMaximumBytes,
                        allowEmpty: true,
                        ct: ct);
                }
                catch (Exception exception) when (
                    IsOrdinaryProjectionReadFailure(exception))
                {
                    throw new ProjectionReadException(exception);
                }
                if (!operationBytes.IsSuccess || !projectionBytes.IsSuccess)
                    return StateInvalid<JournalSnapshot>();
                allowedReservedLeaves.Add(seal.Operation.SegmentFileName);
                allowedReservedLeaves.Add(seal.Projection.SegmentFileName);
                if (!TryAdd(aggregateBytes, seal.Operation.ByteLength, out aggregateBytes) ||
                    !TryAdd(aggregateBytes, seal.Projection.ByteLength, out aggregateBytes) ||
                    aggregateBytes > _maximumAggregateSealedBytes)
                {
                    return ArchiveQuota<JournalSnapshot>();
                }

                AddLines(
                    operationLines,
                    operationBytes.Value,
                    generation,
                    isActive: false,
                    operationJournal: true);
                AddLines(
                    projectionLines,
                    projectionBytes.Value,
                    generation,
                    isActive: false,
                    operationJournal: false);
                var expectedTerminal = operationBytes.Value is { Length: > 0 }
                    ? ExtractTerminalOperationSha256(operationBytes.Value)
                    : terminalOperationSha;
                if (!string.Equals(
                        seal.OperationTerminalRecordSha256,
                        expectedTerminal,
                        StringComparison.Ordinal))
                {
                    return StateInvalid<JournalSnapshot>();
                }
                terminalOperationSha = expectedTerminal;
                previousSealSha = seal.SealSha256;
            }

            if (generation == _maximumSealedSegmentCount &&
                await LeafExistsAsync(directory, BuildSealFileName(generation), ct))
            {
                return ArchiveQuota<JournalSnapshot>();
            }

            activeOperation = await ReadHeldFileAsync(
                directory,
                _operationActiveLeaf,
                _operationActiveMaximumBytes,
                required: false,
                ct,
                readOnlyHandle: true);
            try
            {
                activeProjection = await ReadHeldFileAsync(
                    directory,
                    _projectionActiveLeaf,
                    _projectionActiveMaximumBytes,
                    required: false,
                    ct,
                    allowEmpty: true,
                    readOnlyHandle: true);
            }
            catch (Exception exception) when (
                IsOrdinaryProjectionReadFailure(exception))
            {
                throw new ProjectionReadException(exception);
            }
            if (activeOperation is not null)
            {
                AddLines(
                    operationLines,
                    activeOperation.Bytes,
                    generation,
                    isActive: true,
                    operationJournal: true);
                terminalOperationSha = ExtractTerminalOperationSha256(
                    activeOperation.Bytes);
                if (!IsSha256(terminalOperationSha))
                    return StateInvalid<JournalSnapshot>();
            }
            if (activeProjection is not null)
            {
                AddLines(
                    projectionLines,
                    activeProjection.Bytes,
                    generation,
                    isActive: true,
                    operationJournal: false);
            }

            if (!ValidateReservedInventory(directory, allowedReservedLeaves))
                return StateInvalid<JournalSnapshot>();
            var result = new JournalSnapshot(
                operationLines.ToArray(),
                projectionLines.ToArray(),
                activeOperation,
                activeProjection,
                generation,
                aggregateBytes,
                previousSealSha,
                terminalOperationSha);
            activeOperation = null;
            activeProjection = null;
            return OperationResult<JournalSnapshot>.Ok(result);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Cancelled<JournalSnapshot>();
        }
        catch (ProjectionReadException)
        {
            return ProjectionReadFailed<JournalSnapshot>();
        }
        catch (JournalUtf8Exception exception)
        {
            return exception.Kind == DesignConversionLocalPromotionJournalKind.Projection
                ? InvalidUtf8<JournalSnapshot>()
                : StateInvalid<JournalSnapshot>();
        }
        catch (OverflowException)
        {
            return ArchiveQuota<JournalSnapshot>();
        }
        catch (Exception exception) when (IsStorageException(exception))
        {
            return StateInvalid<JournalSnapshot>();
        }
        finally
        {
            activeProjection?.Dispose();
            activeOperation?.Dispose();
        }
    }

    private async Task<OperationResult<byte[]?>> ReadSealedMemberAsync(
        DesignConversionHandleBoundDirectory directory,
        MemberSeal member,
        long maximumBytes,
        bool allowEmpty,
        CancellationToken ct)
    {
        if (!member.Present)
        {
            if (member.ByteLength != 0L || member.Sha256.Length != 0 ||
                await LeafExistsAsync(directory, member.SegmentFileName, ct))
            {
                return StateInvalid<byte[]?>();
            }
            return OperationResult<byte[]?>.Ok(null);
        }
        using var file = await ReadHeldFileAsync(
            directory,
            member.SegmentFileName,
            maximumBytes,
            required: true,
            ct,
            allowEmpty: allowEmpty,
            readOnlyHandle: true);
        return file is not null && file.Length == member.ByteLength &&
               string.Equals(file.Sha256, member.Sha256, StringComparison.Ordinal)
            ? OperationResult<byte[]?>.Ok(file.Bytes)
            : StateInvalid<byte[]?>();
    }

    private static void AddLines(
        List<DesignConversionLocalPromotionJournalLine> destination,
        byte[]? bytes,
        int generation,
        bool isActive,
        bool operationJournal)
    {
        if (bytes is null)
            return;
        string text;
        try
        {
            text = StrictUtf8.GetString(bytes);
        }
        catch (DecoderFallbackException exception)
        {
            throw new JournalUtf8Exception(
                operationJournal
                    ? DesignConversionLocalPromotionJournalKind.Operation
                    : DesignConversionLocalPromotionJournalKind.Projection,
                exception);
        }
        string[] lines;
        if (operationJournal)
        {
            if (bytes.Length == 0 || bytes.AsSpan().StartsWith(Encoding.UTF8.GetPreamble()) ||
                !text.EndsWith("\r\n", StringComparison.Ordinal))
            {
                throw new InvalidDataException("Operation journal framing is invalid.");
            }
            lines = text.Split("\r\n", StringSplitOptions.None)[..^1];
            if (lines.Any(string.IsNullOrWhiteSpace))
                throw new InvalidDataException("Operation journal contains an empty frame.");
        }
        else
        {
            if (bytes.Length == 0)
                return;
            if (bytes.AsSpan().StartsWith(Encoding.UTF8.GetPreamble()))
                throw new InvalidDataException("Projection journal framing is invalid.");
            using var reader = new StringReader(text);
            var collected = new List<string>();
            while (reader.ReadLine() is { } line)
            {
                if (!string.IsNullOrWhiteSpace(line))
                    collected.Add(line);
            }
            lines = collected.ToArray();
            if (lines.Length == 0)
                throw new InvalidDataException("Projection journal is empty.");
        }

        for (var index = 0; index < lines.Length; index++)
        {
            destination.Add(new DesignConversionLocalPromotionJournalLine(
                lines[index],
                generation,
                index,
                isActive));
        }
    }

    private async Task<HeldFile?> ReadHeldFileAsync(
        DesignConversionHandleBoundDirectory directory,
        string leaf,
        long maximumBytes,
        bool required,
        CancellationToken ct,
        DesignConversionFileIdentity? expectedIdentity = null,
        bool allowEmpty = false,
        bool readOnlyHandle = false)
    {
        var stream = readOnlyHandle
            ? directory.TryOpenExistingReadOnly(leaf)
            : directory.TryOpenExisting(
                leaf,
                FileShare.Read | FileShare.Delete,
                writeThrough: false,
                expectedIdentity);
        if (stream is null)
        {
            if (required)
                throw new DesignConversionHandleBoundNotFoundException(
                    "A required local-promotion journal member is missing.");
            return null;
        }
        try
        {
            if (directory.GetLinkCount(stream, leaf) != 1 ||
                HasForeignPlatformSpecificAlternateDataStream(stream.SafeFileHandle) ||
                (!allowEmpty && stream.Length <= 0) ||
                stream.Length < 0 || stream.Length > maximumBytes ||
                stream.Length > int.MaxValue)
            {
                throw new DesignConversionHandleBoundTrustException(
                    "A local-promotion journal member is not a canonical regular leaf.");
            }
            var length = stream.Length;
            var bytes = new byte[checked((int)length)];
            await stream.ReadExactlyAsync(bytes, ct);
            if (stream.Length != length)
                throw new IOException("Journal member length changed during read.");
            var identity = directory.GetIdentity(stream, leaf);
            if (expectedIdentity is not null && identity != expectedIdentity.Value)
                throw new DesignConversionHandleBoundTrustException(
                    "Journal member identity changed during read.");
            return new HeldFile(
                leaf,
                stream,
                identity,
                length,
                HashBytes(bytes),
                bytes);
        }
        catch
        {
            stream.Dispose();
            throw;
        }
    }

    private async Task PublishCanonicalFileAsync(
        DesignConversionHandleBoundDirectory directory,
        string leaf,
        byte[] bytes,
        long maximumBytes,
        CancellationToken ct,
        bool allowEmpty = false)
    {
        if ((!allowEmpty && bytes.LongLength <= 0) ||
            bytes.LongLength < 0 || bytes.LongLength > maximumBytes)
            throw new InvalidDataException("Journal publication exceeds its bound.");
        ValidateBoundMutationLease(directory);
        var stageLeaf = BuildPublicationStageFileName(leaf);
        if (await LeafExistsAsync(directory, leaf, ct) ||
            await LeafExistsAsync(directory, stageLeaf, ct))
        {
            throw new DesignConversionHandleBoundNameCollisionException(
                "A journal publication destination or staging leaf already exists.");
        }
        await using (var created = directory.CreateNew(
                         stageLeaf,
                         FileShare.Read,
                         writeThrough: true))
        {
            await created.WriteAsync(bytes, ct);
            await created.FlushAsync(ct);
            created.Flush(flushToDisk: true);
        }
        directory.FlushDirectoryMetadata();
        var staged = await ReadHeldFileAsync(
            directory,
            stageLeaf,
            maximumBytes,
            required: true,
            CancellationToken.None,
            allowEmpty: allowEmpty);
        if (staged is null || !staged.Bytes.AsSpan().SequenceEqual(bytes))
        {
            staged?.Dispose();
            throw new IOException("Journal publication readback did not match.");
        }
        directory.RenameNoReplaceAndFlush(staged.Stream, stageLeaf, leaf);
        staged.Dispose();
        ValidateBoundMutationLease(directory);
        using var readback = await ReadHeldFileAsync(
            directory,
            leaf,
            maximumBytes,
            required: true,
            CancellationToken.None,
            allowEmpty: allowEmpty);
        if (readback is null || !readback.Bytes.AsSpan().SequenceEqual(bytes))
            throw new IOException("Journal publication readback did not match.");
    }

    private async Task<bool> DeleteOwnedPublicationStageAsync(
        DesignConversionHandleBoundDirectory directory,
        string destinationLeaf,
        long maximumBytes)
    {
        var stageLeaf = BuildPublicationStageFileName(destinationLeaf);
        var stage = await ReadHeldFileAsync(
            directory,
            stageLeaf,
            maximumBytes,
            required: false,
            CancellationToken.None,
            allowEmpty: true);
        if (stage is null)
            return true;
        var identity = stage.Identity;
        stage.Dispose();
        directory.DeleteIfRegular(stageLeaf, identity);
        return true;
    }

    private static string BuildPublicationStageFileName(string destinationLeaf)
        => destinationLeaf + PublicationStageSuffix;

    private async Task<bool> ActiveMembersMatchPendingAsync(
        DesignConversionHandleBoundDirectory directory,
        PendingRotation pending,
        CancellationToken ct)
        => await ActiveMemberMatchesPendingAsync(directory, pending.Operation, ct) &&
           await ActiveMemberMatchesPendingAsync(directory, pending.Projection, ct);

    private async Task<bool> ActiveMemberMatchesPendingAsync(
        DesignConversionHandleBoundDirectory directory,
        PendingMember member,
        CancellationToken ct)
    {
        var expectedIdentity = member.Present
            ? new DesignConversionFileIdentity(
                member.VolumeSerialNumber,
                member.FileIdLow,
                member.FileIdHigh)
            : (DesignConversionFileIdentity?)null;
        using var file = await ReadHeldFileAsync(
            directory,
            member.ActiveFileName,
            member.ActiveFileName == _operationActiveLeaf
                ? _operationActiveMaximumBytes
                : _projectionActiveMaximumBytes,
            required: false,
            ct,
            expectedIdentity,
            allowEmpty: member.ActiveFileName == _projectionActiveLeaf);
        return member.Present
            ? file is not null && file.Length == member.ByteLength &&
              string.Equals(file.Sha256, member.Sha256, StringComparison.Ordinal)
            : file is null;
    }

    private async Task<bool> DeleteOwnedOrAbsentSegmentAsync(
        DesignConversionHandleBoundDirectory directory,
        PendingMember member,
        CancellationToken ct)
    {
        using var file = await ReadHeldFileAsync(
            directory,
            member.SegmentFileName,
            member.ActiveFileName == _operationActiveLeaf
                ? _operationActiveMaximumBytes
                : _projectionActiveMaximumBytes,
            required: false,
            ct,
            allowEmpty: member.ActiveFileName == _projectionActiveLeaf);
        if (!member.Present)
            return file is null;
        if (file is null)
            return true;
        if (file.Length != member.ByteLength ||
            !string.Equals(file.Sha256, member.Sha256, StringComparison.Ordinal))
        {
            return false;
        }
        var identity = file.Identity;
        file.Dispose();
        directory.DeleteIfRegular(member.SegmentFileName, identity);
        return true;
    }

    private async Task<bool> DeleteMatchingActiveResidueAsync(
        DesignConversionHandleBoundDirectory directory,
        PendingMember member,
        DesignConversionLocalPromotionJournalRotationStage stage)
    {
        using var file = await ReadHeldFileAsync(
            directory,
            member.ActiveFileName,
            member.ActiveFileName == _operationActiveLeaf
                ? _operationActiveMaximumBytes
                : _projectionActiveMaximumBytes,
            required: false,
            CancellationToken.None,
            allowEmpty: member.ActiveFileName == _projectionActiveLeaf);
        if (!member.Present)
            return file is null;
        if (file is null)
            return true;
        var expectedIdentity = new DesignConversionFileIdentity(
            member.VolumeSerialNumber,
            member.FileIdLow,
            member.FileIdHigh);
        if (file.Identity != expectedIdentity || file.Length != member.ByteLength ||
            !string.Equals(file.Sha256, member.Sha256, StringComparison.Ordinal))
        {
            return false;
        }
        file.Dispose();
        directory.DeleteIfRegular(member.ActiveFileName, expectedIdentity);
        _rotationObserver?.Invoke(stage);
        return true;
    }

    private bool ValidateReservedInventory(
        DesignConversionHandleBoundDirectory directory,
        IReadOnlySet<string> allowed)
    {
        var mutationLease = _boundMutationLease;
        if (mutationLease is null)
        {
            throw new DesignConversionHandleBoundTrustException(
                "The journal inventory requires the caller-held mutation lease.");
        }

        var names = directory.EnumerateNames(
            MaximumTraversedRootEntries,
            _mutationLockLeaf,
            mutationLease.Stream);
        foreach (var name in names)
        {
            if (IsReservedJournalName(name) && !allowed.Contains(name))
            {
                throw new DesignConversionHandleBoundTrustException(
                    "A noncanonical local-promotion journal leaf was found.");
            }
        }
        return true;
    }

    private void ValidateBoundMutationLease(
        DesignConversionHandleBoundDirectory directory)
    {
        var mutationLease = _boundMutationLease;
        if (mutationLease is null)
        {
            throw new DesignConversionHandleBoundTrustException(
                "The journal operation requires the caller-held mutation lease.");
        }
        directory.ValidateHeldLeaf(_mutationLockLeaf, mutationLease.Stream);
    }

    private bool IsReservedJournalName(string name)
        => name.StartsWith("training-intake-local-promotion", StringComparison.Ordinal) &&
           !name.StartsWith(
               "training-intake-local-promotion-output-",
               StringComparison.Ordinal);

    private static PendingMember BuildPendingMember(
        HeldFile? file,
        string activeLeaf,
        string segmentLeaf)
        => file is null
            ? new PendingMember(
                false, activeLeaf, segmentLeaf, 0L, string.Empty, 0UL, 0UL, 0UL)
            : new PendingMember(
                true,
                activeLeaf,
                segmentLeaf,
                file.Length,
                file.Sha256,
                file.Identity.VolumeSerialNumber,
                file.Identity.FileIdLow,
                file.Identity.FileIdHigh);

    private static MemberSeal BuildMemberSeal(PendingMember pending)
        => new(
            pending.Present,
            pending.SegmentFileName,
            pending.ByteLength,
            pending.Sha256);

    private static bool SealMatchesPending(
        GenerationSeal seal,
        PendingRotation pending)
        => seal.Generation == pending.Generation &&
           seal.Operation == BuildMemberSeal(pending.Operation) &&
           seal.Projection == BuildMemberSeal(pending.Projection) &&
           string.Equals(
               seal.PreviousSealSha256,
               pending.PreviousSealSha256,
               StringComparison.Ordinal) &&
           string.Equals(
               seal.OperationTerminalRecordSha256,
               pending.OperationTerminalRecordSha256,
               StringComparison.Ordinal);

    private bool PendingNamesMatchExpected(PendingRotation pending)
        => string.Equals(
               pending.Operation.ActiveFileName,
               _operationActiveLeaf,
               StringComparison.Ordinal) &&
           string.Equals(
               pending.Projection.ActiveFileName,
               _projectionActiveLeaf,
               StringComparison.Ordinal) &&
           string.Equals(
               pending.Operation.SegmentFileName,
               BuildOperationSegmentFileName(pending.Generation),
               StringComparison.Ordinal) &&
           string.Equals(
               pending.Projection.SegmentFileName,
               BuildProjectionSegmentFileName(pending.Generation),
               StringComparison.Ordinal);

    private static bool HeldFileStillMatches(
        DesignConversionHandleBoundDirectory directory,
        HeldFile? file)
    {
        if (file is null)
            return true;
        if (directory.GetIdentity(file.Stream, file.LeafName) != file.Identity ||
            file.Stream.Length != file.Length)
        {
            return false;
        }
        file.Stream.Position = 0L;
        var bytes = new byte[checked((int)file.Length)];
        file.Stream.ReadExactly(bytes);
        return string.Equals(HashBytes(bytes), file.Sha256, StringComparison.Ordinal);
    }

    private static GenerationSeal? DeserializeCanonicalSeal(byte[] bytes)
    {
        try
        {
            var seal = JsonSerializer.Deserialize<GenerationSeal>(bytes, JsonOptions);
            if (seal is null ||
                !string.Equals(seal.SchemaVersion, MetadataSchemaVersion, StringComparison.Ordinal) ||
                seal.Version != 1 || seal.Generation < 0 ||
                !IsCanonicalMember(seal.Operation, allowEmpty: false) ||
                !IsCanonicalMember(seal.Projection, allowEmpty: true) ||
                !IsOptionalSha256(seal.PreviousSealSha256) ||
                !IsOptionalSha256(seal.OperationTerminalRecordSha256) ||
                !IsSha256(seal.SealSha256) ||
                !string.Equals(seal.SealSha256, BuildSealSha256(seal), StringComparison.Ordinal) ||
                !bytes.AsSpan().SequenceEqual(JsonSerializer.SerializeToUtf8Bytes(seal, JsonOptions)))
            {
                return null;
            }
            return seal;
        }
        catch (Exception exception) when (exception is JsonException or NotSupportedException)
        {
            return null;
        }
    }

    private static PendingRotation? DeserializeCanonicalPending(byte[] bytes)
    {
        try
        {
            var pending = JsonSerializer.Deserialize<PendingRotation>(bytes, JsonOptions);
            if (pending is null ||
                !string.Equals(pending.SchemaVersion, MetadataSchemaVersion, StringComparison.Ordinal) ||
                pending.Version != 1 || pending.Generation < 0 ||
                !IsCanonicalPendingMember(pending.Operation, allowEmpty: false) ||
                !IsCanonicalPendingMember(pending.Projection, allowEmpty: true) ||
                !IsOptionalSha256(pending.PreviousSealSha256) ||
                !IsOptionalSha256(pending.OperationTerminalRecordSha256) ||
                !IsSha256(pending.PendingSha256) ||
                !string.Equals(pending.PendingSha256, BuildPendingSha256(pending), StringComparison.Ordinal) ||
                !bytes.AsSpan().SequenceEqual(JsonSerializer.SerializeToUtf8Bytes(pending, JsonOptions)))
            {
                return null;
            }
            return pending;
        }
        catch (Exception exception) when (exception is JsonException or NotSupportedException)
        {
            return null;
        }
    }

    private static PendingAppend? DeserializeCanonicalAppendPending(byte[] bytes)
    {
        try
        {
            var pending = JsonSerializer.Deserialize<PendingAppend>(bytes, JsonOptions);
            if (pending is null ||
                !string.Equals(pending.SchemaVersion, MetadataSchemaVersion, StringComparison.Ordinal) ||
                pending.Version != 1 ||
                !Enum.IsDefined(pending.Kind) ||
                !IsCanonicalPendingMember(
                    pending.Source,
                    allowEmpty: pending.Kind ==
                        DesignConversionLocalPromotionJournalKind.Projection) ||
                string.IsNullOrWhiteSpace(pending.TempFileName) ||
                Path.GetFileName(pending.TempFileName) != pending.TempFileName ||
                pending.TargetByteLength <= 0 ||
                !IsSha256(pending.TargetSha256) ||
                !IsSha256(pending.PendingSha256) ||
                !string.Equals(
                    pending.PendingSha256,
                    BuildAppendPendingSha256(pending),
                    StringComparison.Ordinal) ||
                !bytes.AsSpan().SequenceEqual(
                    JsonSerializer.SerializeToUtf8Bytes(pending, JsonOptions)))
            {
                return null;
            }
            return pending;
        }
        catch (Exception exception) when (exception is JsonException or NotSupportedException)
        {
            return null;
        }
    }

    private static bool IsCanonicalMember(
        MemberSeal member,
        bool allowEmpty)
        => !string.IsNullOrWhiteSpace(member.SegmentFileName) &&
           Path.GetFileName(member.SegmentFileName) == member.SegmentFileName &&
           (member.Present
               ? (allowEmpty ? member.ByteLength >= 0 : member.ByteLength > 0) &&
                 IsSha256(member.Sha256)
               : member.ByteLength == 0 && member.Sha256.Length == 0);

    private static bool IsCanonicalPendingMember(
        PendingMember member,
        bool allowEmpty)
        => !string.IsNullOrWhiteSpace(member.ActiveFileName) &&
           Path.GetFileName(member.ActiveFileName) == member.ActiveFileName &&
           !string.IsNullOrWhiteSpace(member.SegmentFileName) &&
           Path.GetFileName(member.SegmentFileName) == member.SegmentFileName &&
           (member.Present
               ? (allowEmpty ? member.ByteLength >= 0 : member.ByteLength > 0) &&
                 IsSha256(member.Sha256)
               : member.ByteLength == 0 && member.Sha256.Length == 0 &&
                 member.VolumeSerialNumber == 0 && member.FileIdLow == 0 &&
                 member.FileIdHigh == 0);

    private static string BuildSealSha256(GenerationSeal seal)
        => HashBytes(JsonSerializer.SerializeToUtf8Bytes(
            seal with { SealSha256 = string.Empty },
            JsonOptions));

    private static string BuildPendingSha256(PendingRotation pending)
        => HashBytes(JsonSerializer.SerializeToUtf8Bytes(
            pending with { PendingSha256 = string.Empty },
            JsonOptions));

    private static string BuildAppendPendingSha256(PendingAppend pending)
        => HashBytes(JsonSerializer.SerializeToUtf8Bytes(
            pending with { PendingSha256 = string.Empty },
            JsonOptions));

    private static string ExtractTerminalOperationSha256(byte[] bytes)
    {
        if (bytes.Length == 0)
            return string.Empty;
        string text;
        try
        {
            text = StrictUtf8.GetString(bytes);
        }
        catch (DecoderFallbackException exception)
        {
            throw new JournalUtf8Exception(
                DesignConversionLocalPromotionJournalKind.Operation,
                exception);
        }
        var lines = text.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0)
            return string.Empty;
        try
        {
            using var document = JsonDocument.Parse(lines[^1]);
            return document.RootElement.TryGetProperty("recordSha256", out var value)
                ? value.GetString() ?? string.Empty
                : string.Empty;
        }
        catch (JsonException)
        {
            return string.Empty;
        }
    }

    private bool FitsActive(
        JournalSnapshot snapshot,
        long operationReservationBytes,
        long projectionReservationBytes)
        => TryAdd(
               snapshot.ActiveOperation?.Length ?? 0L,
               operationReservationBytes,
               out var operation) &&
           operation <= _operationActiveMaximumBytes &&
           TryAdd(
               snapshot.ActiveProjection?.Length ?? 0L,
               projectionReservationBytes,
               out var projection) &&
           projection <= _projectionActiveMaximumBytes;

    private static async Task<bool> LeafExistsAsync(
        DesignConversionHandleBoundDirectory directory,
        string leaf,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await using var stream = directory.TryOpenExistingReadOnly(leaf);
        return stream is not null;
    }

    private static string BuildOperationSegmentFileName(int generation)
        => $"{OperationSegmentStem}{FormatGeneration(generation)}.jsonl";

    private static string BuildProjectionSegmentFileName(int generation)
        => $"{ProjectionSegmentStem}{FormatGeneration(generation)}.jsonl";

    private static string BuildSealFileName(int generation)
        => $"{SealStem}{FormatGeneration(generation)}.seal.json";

    private static string FormatGeneration(int generation)
        => generation.ToString($"D{GenerationDigits}", CultureInfo.InvariantCulture);

    private static string RequireDirectLeaf(string leaf)
    {
        if (string.IsNullOrWhiteSpace(leaf) ||
            !string.Equals(Path.GetFileName(leaf), leaf, StringComparison.Ordinal) ||
            leaf.IndexOf(':') >= 0)
        {
            throw new ArgumentException("A canonical direct journal leaf is required.", nameof(leaf));
        }
        return leaf;
    }

    private static long ComputeDefaultArchiveMaximum(
        long operationMaximum,
        long projectionMaximum,
        int segmentCount)
    {
        if (!TryAdd(operationMaximum, projectionMaximum, out var perGeneration) ||
            perGeneration > long.MaxValue / segmentCount)
        {
            return long.MaxValue;
        }
        return perGeneration * segmentCount;
    }

    private static bool TryAdd(long left, long right, out long sum)
    {
        sum = 0L;
        if (left < 0 || right < 0 || left > long.MaxValue - right)
            return false;
        sum = left + right;
        return true;
    }

    private static string HashBytes(ReadOnlySpan<byte> bytes)
        => $"{Sha256Prefix}{Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant()}";

    private static bool IsOptionalSha256(string value)
        => value.Length == 0 || IsSha256(value);

    private static bool IsSha256(string? value)
        => value is not null && value.Length == Sha256Prefix.Length + 64 &&
           value.StartsWith(Sha256Prefix, StringComparison.Ordinal) &&
           value.Skip(Sha256Prefix.Length).All(
               character => character is >= '0' and <= '9' or >= 'a' and <= 'f');

    private static bool HasForeignPlatformSpecificAlternateDataStream(
        SafeFileHandle handle)
    {
        if (OperatingSystem.IsWindows())
            return HasForeignWindowsAlternateDataStream(handle);
        if (OperatingSystem.IsLinux())
        {
            // NTFS alternate data streams are a Windows-only file-system
            // surface. On Linux the handle-bound directory has already proved
            // the regular leaf, stable identity, and single-link custody.
            return false;
        }

        throw new PlatformNotSupportedException(
            "Local-promotion journal file-stream inspection supports Windows and Linux only.");
    }

    private static bool HasForeignWindowsAlternateDataStream(SafeFileHandle handle)
    {
        var buffer = new byte[4096];
        if (!GetFileInformationByHandleEx(
                handle,
                FileStreamInfo,
                buffer,
                (uint)buffer.Length))
        {
            var error = Marshal.GetLastPInvokeError();
            if (error == 38) // ERROR_HANDLE_EOF: no stream records.
                return false;
            throw new IOException(
                "Could not inspect journal alternate data streams.",
                new System.ComponentModel.Win32Exception(error));
        }
        var offset = 0;
        var count = 0;
        while (true)
        {
            if (offset < 0 || offset > buffer.Length - 24)
                throw new InvalidDataException("File stream information is malformed.");
            var next = BitConverter.ToUInt32(buffer, offset);
            var nameLength = BitConverter.ToUInt32(buffer, offset + 4);
            if ((nameLength & 1) != 0 || nameLength > buffer.Length - offset - 24)
                throw new InvalidDataException("File stream information name is malformed.");
            var name = Encoding.Unicode.GetString(
                buffer,
                offset + 24,
                checked((int)nameLength));
            count++;
            if (count > 1 || !string.Equals(name, "::$DATA", StringComparison.Ordinal))
                return true;
            if (next == 0)
                return false;
            offset = checked(offset + (int)next);
        }
    }

    private static bool IsStorageException(Exception exception)
        => exception is IOException or UnauthorizedAccessException or
            SecurityException or PlatformNotSupportedException or
            InvalidDataException or JsonException or NotSupportedException or
            ArgumentException or OverflowException;

    private static bool IsOrdinaryProjectionReadFailure(Exception exception)
        => exception is UnauthorizedAccessException or SecurityException ||
           exception is IOException and
           not DesignConversionHandleBoundTrustException and
           not DesignConversionHandleBoundNotFoundException and
           not DesignConversionHandleBoundEnumerationLimitException and
           not DesignConversionHandleBoundNameCollisionException and
           not JournalUtf8Exception;

    private static bool IsAppendPublicationWriteFailure(Exception exception)
    {
        if (exception is DesignConversionHandleBoundTrustException or
            DesignConversionHandleBoundNotFoundException or
            DesignConversionHandleBoundEnumerationLimitException or
            DesignConversionHandleBoundNameCollisionException or
            DesignConversionHandleBoundDeleteDispositionException or
            DesignConversionHandleBoundPostCommitCleanupException or
            JournalUtf8Exception or ProjectionReadException or
            InvalidDataException or JsonException or NotSupportedException or
            ArgumentException or OverflowException)
        {
            return false;
        }

        if (exception.InnerException is AggregateException)
            return false;

        return exception is IOException or UnauthorizedAccessException or
            SecurityException or PlatformNotSupportedException;
    }

    private static OperationResult<T> StateInvalid<T>()
        => OperationResult<T>.Fail(
            "DesignConversion local promotion operation journal state is invalid.",
            "DESIGN_CONVERSION_TRAINING_INTAKE_PROMOTE_LOCAL_OPERATION_STATE_INVALID");

    private static OperationResult<T> LocalArtifactDbWriteFailed<T>()
        => OperationResult<T>.Fail(
            JsonlDesignConversionLocalArtifactDb.LocalArtifactDbWriteFailedSafeMessage,
            JsonlDesignConversionLocalArtifactDb.LocalArtifactDbWriteFailedErrorCode);

    private static OperationResult<T> ArchiveQuota<T>()
        => OperationResult<T>.Fail(
            JsonlDesignConversionLocalArtifactDb
                .TrainingIntakeLocalPromotionsQuotaExceededSafeMessage,
            JsonlDesignConversionLocalArtifactDb
                .TrainingIntakeLocalPromotionsQuotaExceededErrorCode);

    private static bool IsArchiveQuota(string? errorCode)
        => string.Equals(
            errorCode,
            JsonlDesignConversionLocalArtifactDb
                .TrainingIntakeLocalPromotionsQuotaExceededErrorCode,
            StringComparison.Ordinal);

    private static OperationResult<T> Quota<T>(string message, string code)
        => OperationResult<T>.Fail(message, code);

    private static OperationResult<T> InvalidUtf8<T>()
        => OperationResult<T>.Fail(
            "DesignConversion training intake local promotion records could not be read because the local promotion journal is not valid UTF-8.",
            JsonlDesignConversionLocalArtifactDb.InvalidUtf8ErrorCode);

    private static OperationResult<T> ProjectionReadFailed<T>()
        => OperationResult<T>.Fail(
            "DesignConversion training intake local promotion records could not be read.",
            "DESIGN_CONVERSION_TRAINING_INTAKE_LOCAL_PROMOTION_READ_FAILED");

    private static OperationResult<T> Cancelled<T>()
        => OperationResult<T>.Fail("The operation was cancelled.", "CANCELLED");

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetFileInformationByHandleEx(
        SafeFileHandle hFile,
        int fileInformationClass,
        [Out] byte[] lpFileInformation,
        uint dwBufferSize);

}
