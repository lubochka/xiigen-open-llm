// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using AgentInfrastructure;
using TransformerModel;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("LlmFromScratch.Tests")]

namespace ContinuousLearning;

public sealed record RepositoryOwnedDpoRandomStateOwnershipProof(
    string Path,
    int Step,
    string Sha256,
    long ByteLength,
    string Identity);

public sealed record RepositoryOwnedDpoRandomStateWriteReadback(
    string Path,
    string FileName,
    string Sha256,
    long ByteLength,
    LlmModelRandomStateSnapshot State,
    RepositoryOwnedDpoRandomStateOwnershipProof OwnershipProof);

/// <summary>
/// Owner-only, bounded, create-new persistence for the deterministic adapter
/// cursor. The manifest/coordinator owns run/parent/pair binding; this helper
/// owns only the self-describing XRGN bytes and the custody proof.
/// </summary>
public static class RepositoryOwnedDpoRandomStateFile
{
    public const string InvalidErrorCode =
        "CODE_DOCUMENTATION_DPO_RANDOM_STATE_INVALID";
    public const string UnavailableErrorCode =
        "CODE_DOCUMENTATION_DPO_RANDOM_STATE_UNAVAILABLE";
    public const string CleanupFailedErrorCode =
        "CODE_DOCUMENTATION_DPO_RANDOM_STATE_CLEANUP_FAILED";
    public const string Magic = "XRGN";
    public const string Algorithm = "splitmix64/v1";
    public const uint CurrentFormatVersion = 1;
    private const int MaximumEntryCount = 4096;
    private const int MaximumAdapterKeyBytes = 256;
    private const int MaximumStreamDomainBytes = 128;
    private const long MaximumFileBytes = 1L * 1024L * 1024L;
    private const int MaximumStep = 999_999;
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private static readonly AsyncLocal<Action<FileStream>?>
        AfterCreateNewForTestingScope = new();
    private static readonly AsyncLocal<Action<string>?>
        BeforeDeleteUnpublishedForTestingScope = new();

    internal static Action<FileStream>? AfterCreateNewForTesting
    {
        get => AfterCreateNewForTestingScope.Value;
        set => AfterCreateNewForTestingScope.Value = value;
    }

    internal static Action<string>? BeforeDeleteUnpublishedForTesting
    {
        get => BeforeDeleteUnpublishedForTestingScope.Value;
        set => BeforeDeleteUnpublishedForTestingScope.Value = value;
    }

    public static string BuildFileName(int step)
    {
        if (step <= 0 || step > MaximumStep)
            throw new ArgumentOutOfRangeException(nameof(step));
        return $"dpo_random_state_step_{step:D6}.bin";
    }

    public static OperationResult<RepositoryOwnedDpoRandomStateWriteReadback>
        WriteCreateNew(
            string absolutePath,
            LlmModelRandomStateSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        string? createdPath = null;
        var cleanupCreatedPath = false;
        OperationResult<RepositoryOwnedDpoRandomStateWriteReadback>? result = null;
        try
        {
            result = WriteCreateNewCore(
                absolutePath,
                snapshot,
                out createdPath,
                out cleanupCreatedPath);
        }
        finally
        {
            if (cleanupCreatedPath && createdPath is not null)
            {
                var cleanup = TryDeleteUnpublishedFile(createdPath);
                if (!cleanup.IsSuccess)
                {
                    result = OperationResult<
                        RepositoryOwnedDpoRandomStateWriteReadback>.Fail(
                        cleanup.ErrorMessage,
                        cleanup.ErrorCode);
                }
            }
        }
        return result ?? Unavailable<RepositoryOwnedDpoRandomStateWriteReadback>();
    }

    private static OperationResult<RepositoryOwnedDpoRandomStateWriteReadback>
        WriteCreateNewCore(
            string absolutePath,
            LlmModelRandomStateSnapshot snapshot,
            out string? createdPath,
            out bool cleanupCreatedPath)
    {
        createdPath = null;
        cleanupCreatedPath = false;
        try
        {
            var path = ValidateNewPath(absolutePath, snapshot.RandomStateStep);
            if (!path.IsSuccess || path.Value is null)
                return path.MapError<RepositoryOwnedDpoRandomStateWriteReadback>();

            var bytes = Serialize(snapshot);
            if (bytes.LongLength <= 0 || bytes.LongLength > MaximumFileBytes)
                return Invalid<RepositoryOwnedDpoRandomStateWriteReadback>();
            if (SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path.Value) ||
                Directory.Exists(path.Value) || File.Exists(path.Value))
            {
                return Unavailable<RepositoryOwnedDpoRandomStateWriteReadback>();
            }

            // Build and validate the complete bytes away from the requested target.
            // The final same-directory move is the only operation that creates the
            // target, so a write/flush/readback failure cannot leave a partial target.
            var stagingPath = BuildStagingPath(path.Value);
            using (var stream = SecureLocalFileBoundary.OpenOwnerOnlyFile(
                       stagingPath,
                       FileMode.CreateNew,
                       FileAccess.Write,
                       FileShare.None,
                       FileOptions.WriteThrough))
            {
                createdPath = stagingPath;
                cleanupCreatedPath = true;
                if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(
                        stream.SafeFileHandle,
                        stagingPath) ||
                    !SecureLocalFileBoundary.HasOwnerOnlyFileMode(stagingPath))
                {
                    return Unavailable<RepositoryOwnedDpoRandomStateWriteReadback>();
                }
                AfterCreateNewForTesting?.Invoke(stream);
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(flushToDisk: true);
            }

            var readback = ReadBytes(stagingPath);
            if (!readback.IsSuccess || readback.Value is null ||
                !CryptographicOperations.FixedTimeEquals(bytes, readback.Value.Bytes))
            {
                return Invalid<RepositoryOwnedDpoRandomStateWriteReadback>();
            }
            var opened = ParseAndValidate(
                readback.Value.Bytes,
                expectedRandomStateStep: snapshot.RandomStateStep,
                expectedTopologySha256: snapshot.TopologySha256,
                expectedLoraConfigurationSha256: snapshot.LoraConfigurationSha256);
            if (!opened.IsSuccess || opened.Value is null)
                return opened.MapError<RepositoryOwnedDpoRandomStateWriteReadback>();

            var proof = new RepositoryOwnedDpoRandomStateOwnershipProof(
                path.Value,
                opened.Value.RandomStateStep,
                readback.Value.Sha256,
                readback.Value.Bytes.LongLength,
                BuildIdentity(opened.Value));
            var persisted = OperationResult<RepositoryOwnedDpoRandomStateWriteReadback>.Ok(
                new(
                    path.Value,
                    Path.GetFileName(path.Value),
                    readback.Value.Sha256,
                    readback.Value.Bytes.LongLength,
                    opened.Value,
                    proof));

            // Never overwrite a path that was not created by this operation. On a
            // collision File.Move fails and the staging file remains the sole
            // cleanup target recorded above. The proof/result are built before the
            // move so target creation is the final fallible operation.
            File.Move(stagingPath, path.Value);
            createdPath = null;
            cleanupCreatedPath = false;
            return persisted;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception) when (IsNonFatalFileException(exception))
        {
            return Unavailable<RepositoryOwnedDpoRandomStateWriteReadback>();
        }
    }

    public static OperationResult<LlmModelRandomStateSnapshot> OpenAndValidate(
        string absolutePath,
        string expectedSha256,
        long expectedByteLength,
        int expectedRandomStateStep,
        string expectedTopologySha256,
        string expectedLoraConfigurationSha256)
    {
        try
        {
            if (!IsHash(expectedSha256) || expectedByteLength <= 0 ||
                expectedByteLength > MaximumFileBytes ||
                expectedRandomStateStep <= 0 ||
                !IsHash(expectedTopologySha256) ||
                !IsHash(expectedLoraConfigurationSha256))
            {
                return Invalid<LlmModelRandomStateSnapshot>();
            }
            var readback = ReadBytes(absolutePath);
            if (!readback.IsSuccess || readback.Value is null ||
                readback.Value.Bytes.LongLength != expectedByteLength ||
                !StringComparer.Ordinal.Equals(
                    expectedSha256,
                    readback.Value.Sha256))
            {
                return Invalid<LlmModelRandomStateSnapshot>();
            }
            return ParseAndValidate(
                readback.Value.Bytes,
                expectedRandomStateStep,
                expectedTopologySha256,
                expectedLoraConfigurationSha256);
        }
        catch (Exception exception) when (IsNonFatalFileException(exception))
        {
            return Unavailable<LlmModelRandomStateSnapshot>();
        }
    }

    public static OperationResult<bool> TryDeleteIfOwned(
        RepositoryOwnedDpoRandomStateOwnershipProof proof)
    {
        ArgumentNullException.ThrowIfNull(proof);
        try
        {
            if (!Path.IsPathFullyQualified(proof.Path) ||
                !File.Exists(proof.Path) ||
                Directory.Exists(proof.Path) ||
                SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(proof.Path) ||
                !SecureLocalFileBoundary.HasOwnerOnlyFileMode(proof.Path))
            {
                return OperationResult<bool>.Ok(false);
            }
            var bytes = ReadBytes(proof.Path);
            if (!bytes.IsSuccess || bytes.Value is null ||
                bytes.Value.Bytes.LongLength != proof.ByteLength ||
                !StringComparer.Ordinal.Equals(bytes.Value.Sha256, proof.Sha256))
            {
                return Invalid<bool>();
            }
            var state = ParseAndValidate(
                bytes.Value.Bytes,
                proof.Step,
                expectedTopologySha256: null,
                expectedLoraConfigurationSha256: null);
            if (!state.IsSuccess || state.Value is null ||
                !StringComparer.Ordinal.Equals(
                    proof.Identity,
                    BuildIdentity(state.Value)))
            {
                return Invalid<bool>();
            }
            File.Delete(proof.Path);
            return OperationResult<bool>.Ok(!File.Exists(proof.Path));
        }
        catch (Exception exception) when (IsNonFatalFileException(exception))
        {
            return Unavailable<bool>();
        }
    }

    private static byte[] Serialize(LlmModelRandomStateSnapshot snapshot)
    {
        ValidateSnapshot(snapshot);
        using var stream = new MemoryStream();
        stream.Write("XRGN"u8);
        WriteUInt32(stream, CurrentFormatVersion);
        WriteUInt32(stream, checked((uint)Encoding.ASCII.GetByteCount(Algorithm)));
        stream.Write(Encoding.ASCII.GetBytes(Algorithm));
        WriteInt32(stream, snapshot.RandomStateStep);
        WriteHash(stream, snapshot.TopologySha256);
        WriteHash(stream, snapshot.LoraConfigurationSha256);
        var entries = snapshot.Entries
            .OrderBy(static entry => entry.AdapterKey, StringComparer.Ordinal)
            .ThenBy(static entry => entry.StreamDomain, StringComparer.Ordinal)
            .ToArray();
        WriteUInt32(stream, checked((uint)entries.Length));
        foreach (var entry in entries)
        {
            WriteString(stream, entry.AdapterKey, MaximumAdapterKeyBytes);
            WriteString(stream, entry.StreamDomain, MaximumStreamDomainBytes);
            WriteUInt64(stream, entry.InitializationSeed);
            WriteUInt64(stream, entry.DropoutSeed);
            WriteUInt64(stream, entry.ConsumedCursor);
        }
        return stream.ToArray();
    }

    private static OperationResult<LlmModelRandomStateSnapshot> ParseAndValidate(
        byte[] bytes,
        int expectedRandomStateStep,
        string? expectedTopologySha256,
        string? expectedLoraConfigurationSha256)
    {
        if (bytes.Length == 0 || bytes.LongLength > MaximumFileBytes)
            return Invalid<LlmModelRandomStateSnapshot>();
        try
        {
            var reader = new Reader(bytes);
            if (!reader.ReadBytes(4).SequenceEqual("XRGN"u8.ToArray()) ||
                reader.ReadUInt32() != CurrentFormatVersion)
                return Invalid<LlmModelRandomStateSnapshot>();
            var algorithmLength = reader.ReadUInt32();
            if (algorithmLength != Encoding.ASCII.GetByteCount(Algorithm) ||
                !Encoding.ASCII.GetString(reader.ReadBytes(checked((int)algorithmLength)))
                    .Equals(Algorithm, StringComparison.Ordinal))
                return Invalid<LlmModelRandomStateSnapshot>();
            var step = reader.ReadInt32();
            if (step <= 0 || step != expectedRandomStateStep)
                return Invalid<LlmModelRandomStateSnapshot>();
            var topology = reader.ReadHash();
            var loraConfig = reader.ReadHash();
            if ((expectedTopologySha256 is not null &&
                 !StringComparer.Ordinal.Equals(expectedTopologySha256, topology)) ||
                (expectedLoraConfigurationSha256 is not null &&
                 !StringComparer.Ordinal.Equals(expectedLoraConfigurationSha256, loraConfig)))
                return Invalid<LlmModelRandomStateSnapshot>();
            var count = reader.ReadUInt32();
            if (count is 0 or > MaximumEntryCount)
                return Invalid<LlmModelRandomStateSnapshot>();
            var entries = new List<LlmModelRandomStateEntry>(checked((int)count));
            var seenAdapterKeys = new HashSet<string>(StringComparer.Ordinal);
            for (var index = 0; index < count; index++)
            {
                var key = reader.ReadString(MaximumAdapterKeyBytes);
                var domain = reader.ReadString(MaximumStreamDomainBytes);
                var entry = new LlmModelRandomStateEntry(
                    key,
                    domain,
                    reader.ReadUInt64(),
                    reader.ReadUInt64(),
                    reader.ReadUInt64());
                if (index > 0)
                {
                    var previous = entries[index - 1];
                    var keyOrder = StringComparer.Ordinal.Compare(
                        previous.AdapterKey,
                        entry.AdapterKey);
                    if (keyOrder > 0 ||
                        (keyOrder == 0 &&
                         StringComparer.Ordinal.Compare(
                             previous.StreamDomain,
                             entry.StreamDomain) >= 0))
                    {
                        return Invalid<LlmModelRandomStateSnapshot>();
                    }
                }
                if (!seenAdapterKeys.Add(entry.AdapterKey))
                    return Invalid<LlmModelRandomStateSnapshot>();
                entries.Add(entry);
            }
            if (!reader.End)
                return Invalid<LlmModelRandomStateSnapshot>();
            return OperationResult<LlmModelRandomStateSnapshot>.Ok(
                new(
                    CurrentFormatVersion,
                    Algorithm,
                    entries,
                    topology,
                    loraConfig,
                    step));
        }
        catch (Exception exception) when (exception is EndOfStreamException or
            DecoderFallbackException or OverflowException or ArgumentException or
            InvalidDataException)
        {
            return Invalid<LlmModelRandomStateSnapshot>();
        }
    }

    private static void ValidateSnapshot(LlmModelRandomStateSnapshot snapshot)
    {
        if (snapshot.FormatVersion != CurrentFormatVersion ||
            !StringComparer.Ordinal.Equals(snapshot.Algorithm, Algorithm) ||
            snapshot.RandomStateStep <= 0 || snapshot.Entries.Count is 0 or > MaximumEntryCount ||
            !IsHash(snapshot.TopologySha256) || !IsHash(snapshot.LoraConfigurationSha256))
            throw new InvalidDataException("DPO_RANDOM_STATE_SNAPSHOT_INVALID");
        var seenAdapterKeys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var entry in snapshot.Entries)
        {
            var keyBytes = StrictUtf8.GetByteCount(entry.AdapterKey);
            var domainBytes = StrictUtf8.GetByteCount(entry.StreamDomain);
            if (keyBytes is < 1 or > MaximumAdapterKeyBytes ||
                domainBytes is < 1 or > MaximumStreamDomainBytes ||
                !seenAdapterKeys.Add(entry.AdapterKey))
                throw new InvalidDataException("DPO_RANDOM_STATE_ENTRY_INVALID");
        }
    }

    private static OperationResult<string> ValidateNewPath(string absolutePath, int step)
    {
        if (string.IsNullOrWhiteSpace(absolutePath) ||
            !Path.IsPathFullyQualified(absolutePath) ||
            step <= 0 || step > MaximumStep)
            return Invalid<string>();
        var path = Path.GetFullPath(absolutePath);
        var directory = Path.GetDirectoryName(path);
        if (directory is null || !Directory.Exists(directory) ||
            SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(directory) ||
            !StringComparer.Ordinal.Equals(Path.GetFileName(path), BuildFileName(step)))
            return Unavailable<string>();
        return OperationResult<string>.Ok(path);
    }

    private static string BuildStagingPath(string targetPath)
    {
        var directory = Path.GetDirectoryName(targetPath)
            ?? throw new InvalidDataException(
                "DPO_RANDOM_STATE_STAGING_DIRECTORY_INVALID");
        return Path.Combine(
            directory,
            $".{Path.GetFileName(targetPath)}.{Guid.NewGuid():N}.tmp");
    }

    private static OperationResult<bool> TryDeleteUnpublishedFile(string path)
    {
        try
        {
            if (!Path.IsPathFullyQualified(path))
            {
                return CleanupFailed<bool>(
                    path,
                    "The unpublished cleanup path is not fully qualified.");
            }
            if (!File.Exists(path))
            {
                return OperationResult<bool>.Ok(true);
            }
            if (Directory.Exists(path) ||
                SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path) ||
                !SecureLocalFileBoundary.HasOwnerOnlyFileMode(path))
            {
                return CleanupFailed<bool>(
                    path,
                    "The unpublished artifact could not be revalidated as an owner-only file.");
            }
            BeforeDeleteUnpublishedForTesting?.Invoke(path);
            File.Delete(path);
            if (File.Exists(path))
            {
                return CleanupFailed<bool>(
                    path,
                    "Deletion completed without a confirmed absent target.");
            }
            return OperationResult<bool>.Ok(true);
        }
        catch (Exception exception) when (IsNonFatalFileException(exception))
        {
            return CleanupFailed<bool>(
                path,
                $"The unpublished artifact could not be deleted: " +
                $"{exception.GetType().Name}: {exception.Message}");
        }
    }

    private static OperationResult<StableBytes> ReadBytes(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Path.IsPathFullyQualified(path) ||
            !File.Exists(path) || Directory.Exists(path) ||
            SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path))
            return Unavailable<StableBytes>();
        using var stream = SecureLocalFileBoundary.OpenOwnerOnlyFile(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            FileOptions.SequentialScan);
        if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(
                stream.SafeFileHandle, path) ||
            !SecureLocalFileBoundary.HasOwnerOnlyFileMode(path))
            return Unavailable<StableBytes>();
        var bytes = SecureLocalFileBoundary.ReadBoundedStableBytes(stream, MaximumFileBytes);
        return bytes is null || bytes.Length == 0
            ? Invalid<StableBytes>()
            : OperationResult<StableBytes>.Ok(new(
                bytes,
                Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant()));
    }

    private static void WriteHash(Stream stream, string hash)
        => WriteString(stream, hash, 64);

    private static void WriteString(Stream stream, string value, int maximumBytes)
    {
        var bytes = StrictUtf8.GetBytes(value);
        if (bytes.Length < 1 || bytes.Length > maximumBytes)
            throw new InvalidDataException("DPO_RANDOM_STATE_STRING_INVALID");
        WriteUInt32(stream, checked((uint)bytes.Length));
        stream.Write(bytes);
    }

    private static void WriteUInt32(Stream stream, uint value)
    {
        Span<byte> bytes = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32LittleEndian(bytes, value);
        stream.Write(bytes);
    }

    private static void WriteInt32(Stream stream, int value)
        => WriteUInt32(stream, unchecked((uint)value));

    private static void WriteUInt64(Stream stream, ulong value)
    {
        Span<byte> bytes = stackalloc byte[8];
        BinaryPrimitives.WriteUInt64LittleEndian(bytes, value);
        stream.Write(bytes);
    }

    private static string BuildIdentity(LlmModelRandomStateSnapshot state)
        => $"{state.RandomStateStep}:{state.TopologySha256}:{state.LoraConfigurationSha256}";

    private static bool IsHash(string? value)
        => value is not null && value.Length == 64 &&
           value.All(static c => c is >= '0' and <= '9' or >= 'a' and <= 'f');

    private static bool IsNonFatalFileException(Exception exception)
        => exception is IOException or UnauthorizedAccessException or
            NotSupportedException or PathTooLongException or
            DirectoryNotFoundException or FileNotFoundException or
            ArgumentException or InvalidDataException;

    private static OperationResult<T> Invalid<T>()
        => OperationResult<T>.Fail(
            "The repository-owned DPO random state is malformed or does not match its sealed identity.",
            InvalidErrorCode);

    private static OperationResult<T> CleanupFailed<T>(
        string path,
        string reason)
        => OperationResult<T>.Fail(
            $"The repository-owned DPO random state cleanup failed for " +
            $"unpublished artifact '{path}'. No OwnershipProof was issued; " +
            $"custody remains unresolved. {reason}",
            CleanupFailedErrorCode);

    private static OperationResult<T> Unavailable<T>()
        => OperationResult<T>.Fail(
            "The repository-owned DPO random state could not be safely persisted or reopened.",
            UnavailableErrorCode);

    private sealed record StableBytes(byte[] Bytes, string Sha256);

    private sealed class Reader
    {
        private readonly byte[] _bytes;
        private int _offset;
        internal Reader(byte[] bytes) => _bytes = bytes;
        internal bool End => _offset == _bytes.Length;
        internal byte[] ReadBytes(int length)
        {
            if (length < 0 || length > _bytes.Length - _offset)
                throw new EndOfStreamException();
            var value = _bytes.AsSpan(_offset, length).ToArray();
            _offset += length;
            return value;
        }
        internal uint ReadUInt32()
            => BinaryPrimitives.ReadUInt32LittleEndian(ReadBytes(4));
        internal int ReadInt32() => unchecked((int)ReadUInt32());
        internal ulong ReadUInt64()
            => BinaryPrimitives.ReadUInt64LittleEndian(ReadBytes(8));
        internal string ReadHash()
        {
            var value = ReadString(64);
            if (!IsHash(value))
                throw new InvalidDataException("DPO_RANDOM_STATE_HASH_INVALID");
            return value;
        }
        internal string ReadString(int maximumBytes)
        {
            var length = ReadUInt32();
            if (length == 0 || length > (uint)int.MaxValue ||
                length > (uint)maximumBytes)
                throw new InvalidDataException("DPO_RANDOM_STATE_STRING_INVALID");
            return StrictUtf8.GetString(ReadBytes((int)length));
        }
    }
}
