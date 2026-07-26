// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Buffers.Binary;
using System.Collections;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using AgentInfrastructure.Logical;
using Microsoft.Win32.SafeHandles;
using OrchestratorModel.Logical;

namespace LlmFromScratch.DynamicGraphCompositionLiveLocalServer;

public sealed class DynamicGraphCompositionSessionPersistence : IDisposable
{
    internal const string CheckpointSchema = "dynamic-graph-composition-session.v1";
    private const string LockFileName = "host.lock";
    private const string IndexKeyFileName = "index.key";
    private const string SessionsDirectoryName = "sessions";
    private const int MaxCheckpointBytes = 2 * 1024 * 1024;
    private const long MaxAggregateCheckpointBytes = 64L * 1024 * 1024;
    private static readonly Regex SessionIdPattern = new(
        "^dgs_[0-9a-f]{32}$",
        RegexOptions.CultureInvariant);
    private static readonly Regex DigestPattern = new(
        "^[0-9A-F]{64}$",
        RegexOptions.CultureInvariant);

    private readonly string? _sessionsDirectory;
    private readonly string? _stateDirectory;
    private readonly string? _lockPath;
    private readonly ManagedDirectoryTreeLease? _treeLease;
    private readonly FileStream? _lifetimeLock;
    private readonly ManagedPathIdentity _lockIdentity;
    private readonly byte[] _indexKey;
    private readonly object _lifecycleGate = new();
    private int _activeOperations;
    private int _disposed;

    public DynamicGraphCompositionSessionPersistence(DynamicGraphCompositionServerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();
        IsEnabled = options.StateDirectory is not null;

        if (!IsEnabled)
        {
            _indexKey = RandomNumberGenerator.GetBytes(32);
            return;
        }

        byte[]? assignedIndexKey = null;
        try
        {
            var stateDirectory = DynamicGraphCompositionFileSystemPolicy.NormalizeStateDirectory(
                options.StateDirectory!);
            var readyFile = DynamicGraphCompositionFileSystemPolicy.NormalizeReadyFile(options.ReadyFile);
            DynamicGraphCompositionFileSystemPolicy.EnsureReadyFileOutsideState(
                readyFile,
                stateDirectory);
            DynamicGraphCompositionFileSystemPolicy.EnsurePrivateDirectoryTree(stateDirectory);
            _stateDirectory = stateDirectory;
            _treeLease = DynamicGraphCompositionFileSystemPolicy.AcquireTrustedDirectoryTreeLease(
                stateDirectory);
            var lockPath = DynamicGraphCompositionFileSystemPolicy.DirectChild(
                stateDirectory,
                LockFileName);
            DynamicGraphCompositionFileSystemPolicy.EnsureRegularFileOrMissing(lockPath);
            if (!DynamicGraphCompositionFileSystemPolicy.PathExists(lockPath))
                WriteThrough(stateDirectory, LockFileName, []);
            DynamicGraphCompositionFileSystemPolicy.VerifyExistingPrivateFile(lockPath);
            _lifetimeLock = new FileStream(
                lockPath,
                FileMode.Open,
                FileAccess.ReadWrite,
                FileShare.None,
                bufferSize: 1,
                FileOptions.WriteThrough);
            DynamicGraphCompositionFileSystemPolicy.EnsureRegularFile(lockPath);
            DynamicGraphCompositionFileSystemPolicy.VerifyExistingPrivateFile(lockPath);
            _lockPath = lockPath;
            _lockIdentity = DynamicGraphCompositionFileSystemPolicy.CaptureHandleIdentity(
                _lifetimeLock.SafeFileHandle,
                expectDirectory: false);

            _sessionsDirectory = DynamicGraphCompositionFileSystemPolicy.DirectChild(
                stateDirectory,
                SessionsDirectoryName);
            DynamicGraphCompositionFileSystemPolicy.EnsurePrivateDirectoryTree(_sessionsDirectory);
            _treeLease!.AddDirectory(_sessionsDirectory);
            assignedIndexKey = LoadOrCreateIndexKey(stateDirectory);
            _indexKey = assignedIndexKey;
            DynamicGraphCompositionFileSystemPolicy.EnsureExpectedStateRootContents(
                stateDirectory,
                LockFileName,
                IndexKeyFileName,
                SessionsDirectoryName);
            AssertStateTreeIdentity();
        }
        catch (DynamicGraphCompositionPersistenceException exception) when (IsNonFatalException(exception))
        {
            CleanupAfterFailedConstruction(assignedIndexKey);
            throw;
        }
        catch (Exception exception) when (
            (exception is IOException
                or UnauthorizedAccessException
                or CryptographicException)
            && IsNonFatalException(exception))
        {
            CleanupAfterFailedConstruction(assignedIndexKey);
            throw new DynamicGraphCompositionPersistenceException(
                "The configured dynamic graph state directory could not be opened exclusively.",
                exception);
        }
    }

    public bool IsEnabled { get; }

    internal string DeriveIndex(params string[] components)
    {
        using var operationLease = EnterOperation();
        ArgumentNullException.ThrowIfNull(components, "source");
        byte[]?[] encoded = new byte[components.Length][];
        var encodedCount = 0;
        byte[]? payload = null;
        byte[]? digest = null;
        Exception? operationFailure = null;
        try
        {
            while (encodedCount < components.Length)
            {
                encoded[encodedCount] = Encoding.UTF8.GetBytes(components[encodedCount]);
                encodedCount++;
            }

            var length = 0;
            for (var index = 0; index < encodedCount; index++)
                length = checked(length + sizeof(int) + encoded[index]!.Length);
            payload = new byte[length];
            var offset = 0;
            for (var index = 0; index < encodedCount; index++)
            {
                var component = encoded[index]!;
                BinaryPrimitives.WriteInt32BigEndian(payload.AsSpan(offset, sizeof(int)), component.Length);
                offset += sizeof(int);
                component.CopyTo(payload, offset);
                offset += component.Length;
            }

            digest = HMACSHA256.HashData(_indexKey, payload);
            return Convert.ToHexString(digest);
        }
        catch (Exception exception)
        {
            operationFailure = exception;
            throw;
        }
        finally
        {
            if (operationFailure is null || IsNonFatalException(operationFailure))
            {
                ExceptionDispatchInfo? cleanupFailure = null;

                void ZeroOwnedBuffer(byte[] buffer)
                {
                    try
                    {
                        CryptographicOperations.ZeroMemory(buffer);
                    }
                    catch (Exception exception)
                    {
                        if (!IsNonFatalException(exception))
                            throw;
                        cleanupFailure = ExceptionDispatchInfo.Capture(exception);
                    }
                }

                if (digest is not null)
                    ZeroOwnedBuffer(digest);
                if (payload is not null)
                    ZeroOwnedBuffer(payload);
                for (var index = encodedCount - 1; index >= 0; index--)
                    ZeroOwnedBuffer(encoded[index]!);

                cleanupFailure?.Throw();
            }
        }
    }

    internal IReadOnlyList<PersistedCompositionSession> ReadAll(int capacity)
    {
        using var operationLease = EnterOperation();
        if (!IsEnabled)
            return [];
        if (capacity is < 1 or > 4096)
            throw InvalidState("The persisted session capacity is invalid.");

        try
        {
            AssertStateTreeIdentity();
            DynamicGraphCompositionFileSystemPolicy.VerifyExistingPrivateDirectory(
                _sessionsDirectory!,
                requireInheritableForChildren: true);
            if (Directory.EnumerateDirectories(_sessionsDirectory!).Any())
                throw InvalidState("Unexpected content exists in the session checkpoint directory.");

            var paths = new List<string>(capacity);
            var enumerator = Directory
                .EnumerateFiles(_sessionsDirectory!, "*", SearchOption.TopDirectoryOnly)
                .GetEnumerator();
            Exception? enumerationFailure = null;
            try
            {
                while (enumerator.MoveNext())
                {
                    if (paths.Count >= capacity)
                        throw InvalidState("Persisted session state exceeds the configured capacity.");
                    paths.Add(enumerator.Current);
                }
            }
            catch (Exception exception)
            {
                enumerationFailure = exception;
                throw;
            }
            finally
            {
                if (enumerationFailure is null || IsNonFatalException(enumerationFailure))
                    enumerator.Dispose();
            }
            var sessionIds = new HashSet<string>(StringComparer.Ordinal);
            var idempotencyIndexes = new HashSet<string>(StringComparer.Ordinal);
            var restored = new List<PersistedCompositionSession>(paths.Count);
            long aggregateBytes = 0;
            foreach (var path in paths)
            {
                if (!string.Equals(Path.GetExtension(path), ".json", StringComparison.Ordinal))
                    throw InvalidState("A partial or unexpected session checkpoint was found.");

                var fileName = Path.GetFileName(path);
                var expectedPath = DynamicGraphCompositionFileSystemPolicy.DirectChild(
                    _sessionsDirectory!,
                    fileName);
                if (!string.Equals(path, expectedPath, DynamicGraphCompositionFileSystemPolicy.PathComparison))
                    throw InvalidState("A session checkpoint escaped the managed directory.");
                var remainingAggregateBytes = MaxAggregateCheckpointBytes - aggregateBytes;
                if (remainingAggregateBytes <= 0)
                    throw InvalidState("Persisted session state exceeds the aggregate byte budget.");
                var checkpointBytes = DynamicGraphCompositionFileSystemPolicy.ReadBoundedFile(
                    _sessionsDirectory!,
                    fileName,
                    minimumBytes: 1,
                    maximumBytes: (int)Math.Min(MaxCheckpointBytes, remainingAggregateBytes));
                aggregateBytes += checkpointBytes.Length;
                PersistedCompositionSessionEnvelope envelope;
                Exception? checkpointFailure = null;
                try
                {
                    EnsureStrictJson(checkpointBytes);
                    envelope = JsonSerializer.Deserialize<PersistedCompositionSessionEnvelope>(
                                   checkpointBytes,
                                   DynamicGraphCompositionJson.CheckpointOptions)
                               ?? throw InvalidState("A session checkpoint envelope is required.");
                }
                catch (Exception exception)
                {
                    checkpointFailure = exception;
                    throw;
                }
                finally
                {
                    if (checkpointFailure is null || IsNonFatalException(checkpointFailure))
                        CryptographicOperations.ZeroMemory(checkpointBytes);
                }
                var hmacSha256 = envelope.HmacSha256;
                if (!string.Equals(envelope.Schema, CheckpointSchema, StringComparison.Ordinal)
                    || envelope.Payload is null
                    || hmacSha256 is null
                    || !DigestPattern.IsMatch(hmacSha256))
                {
                    throw InvalidState("A session checkpoint envelope is invalid.");
                }

                var canonicalPayload = DynamicGraphCompositionProfilePolicy.Canonicalize(envelope.Payload);
                Exception? canonicalFailure = null;
                try
                {
                    var expectedMac = ComputeCheckpointMac(canonicalPayload);
                    byte[]? providedMac = null;
                    Exception? macFailure = null;
                    try
                    {
                        providedMac = Convert.FromHexString(hmacSha256);
                        if (!CryptographicOperations.FixedTimeEquals(
                                expectedMac,
                                providedMac))
                        {
                            throw InvalidState("A session checkpoint signature is invalid.");
                        }
                    }
                    catch (Exception exception)
                    {
                        macFailure = exception;
                        throw;
                    }
                    finally
                    {
                        if (macFailure is null || IsNonFatalException(macFailure))
                        {
                            Exception? providedMacZeroingFailure = null;
                            try
                            {
                                if (providedMac is not null)
                                    CryptographicOperations.ZeroMemory(providedMac);
                            }
                            catch (Exception exception)
                            {
                                providedMacZeroingFailure = exception;
                                throw;
                            }
                            finally
                            {
                                if (providedMacZeroingFailure is null
                                    || IsNonFatalException(providedMacZeroingFailure))
                                {
                                    CryptographicOperations.ZeroMemory(expectedMac);
                                }
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    canonicalFailure = exception;
                    throw;
                }
                finally
                {
                    if (canonicalFailure is null || IsNonFatalException(canonicalFailure))
                        CryptographicOperations.ZeroMemory(canonicalPayload);
                }

                ValidatePayload(envelope.Payload, path);
                if (!sessionIds.Add(envelope.Payload.SessionId)
                    || !idempotencyIndexes.Add(envelope.Payload.OpaqueIdempotencyIndex))
                {
                    throw InvalidState("Duplicate persisted session identity was found.");
                }
                restored.Add(envelope.Payload);
            }
            AssertStateTreeIdentity();
            return restored;
        }
        catch (DynamicGraphCompositionPersistenceException)
        {
            throw;
        }
        catch (Exception exception) when (
            (exception is IOException
                or UnauthorizedAccessException
                or JsonException
                or FormatException
                or CryptographicException)
            && IsNonFatalException(exception))
        {
            throw new DynamicGraphCompositionPersistenceException(
                "Persisted dynamic graph session state is invalid.",
                exception);
        }
    }

    internal void PersistNew(PersistedCompositionSession session)
    {
        using var operationLease = EnterOperation();
        if (!IsEnabled)
            return;

        try
        {
            AssertStateTreeIdentity();
            var ownedProfile = SnapshotProfile(session.Profile);
            var ownedSession = session with { Profile = ownedProfile };
            ValidatePayload(ownedSession, ExpectedPath(ownedSession.SessionId));
            var canonicalPayload = DynamicGraphCompositionProfilePolicy.Canonicalize(ownedSession);
            string mac;
            Exception? canonicalFailure = null;
            try
            {
                var macBytes = ComputeCheckpointMac(canonicalPayload);
                Exception? macFailure = null;
                try
                {
                    mac = Convert.ToHexString(macBytes);
                }
                catch (Exception exception)
                {
                    macFailure = exception;
                    throw;
                }
                finally
                {
                    if (macFailure is null || IsNonFatalException(macFailure))
                        CryptographicOperations.ZeroMemory(macBytes);
                }
            }
            catch (Exception exception)
            {
                canonicalFailure = exception;
                throw;
            }
            finally
            {
                if (canonicalFailure is null || IsNonFatalException(canonicalFailure))
                    CryptographicOperations.ZeroMemory(canonicalPayload);
            }

            var envelope = new PersistedCompositionSessionEnvelope(
                CheckpointSchema,
                ownedSession,
                mac);
            byte[]? checkpointBytes = null;
            var checkpointOwnershipTransferred = false;
            Exception? checkpointFailure = null;
            try
            {
                checkpointBytes = JsonSerializer.SerializeToUtf8Bytes(
                    envelope,
                    DynamicGraphCompositionJson.CheckpointOptions);
                if (checkpointBytes.Length > MaxCheckpointBytes)
                    throw InvalidState("A session checkpoint exceeds the bounded size.");
                WriteNewAtomic(
                    _sessionsDirectory!,
                    $"{ownedSession.SessionId}.json",
                    checkpointBytes);
                checkpointOwnershipTransferred = true;
                AssertStateTreeIdentity();
            }
            catch (Exception exception)
            {
                checkpointFailure = exception;
                throw;
            }
            finally
            {
                if (!checkpointOwnershipTransferred
                    && checkpointBytes is not null
                    && (checkpointFailure is null || IsNonFatalException(checkpointFailure)))
                {
                    CryptographicOperations.ZeroMemory(checkpointBytes);
                }
            }
        }
        catch (DynamicGraphCompositionPersistenceException)
        {
            throw;
        }
        catch (Exception exception) when (
            (exception is IOException
                or UnauthorizedAccessException
                or JsonException
                or NotSupportedException
                or CryptographicException)
            && IsNonFatalException(exception))
        {
            throw new DynamicGraphCompositionPersistenceException(
                "The dynamic graph session checkpoint could not be persisted.",
                exception);
        }
    }

    internal void Delete(string sessionId)
    {
        using var operationLease = EnterOperation();
        if (!IsEnabled)
            return;
        if (!SessionIdPattern.IsMatch(sessionId))
            throw InvalidState("The session checkpoint identity is invalid.");

        try
        {
            AssertStateTreeIdentity();
            var fileName = $"{sessionId}.json";
            DynamicGraphCompositionFileSystemPolicy.DeletePrivateManagedFileIfPresent(
                _sessionsDirectory!,
                fileName);
            AssertStateTreeIdentity();
        }
        catch (Exception exception) when (
            (exception is IOException or UnauthorizedAccessException)
            && IsNonFatalException(exception))
        {
            throw new DynamicGraphCompositionPersistenceException(
                "The dynamic graph session checkpoint could not be removed.",
                exception);
        }
    }

    private void ValidatePayload(PersistedCompositionSession payload, string path)
    {
        var sessionId = payload.SessionId;
        if (!string.Equals(payload.Schema, CheckpointSchema, StringComparison.Ordinal)
            || sessionId is null
            || !SessionIdPattern.IsMatch(sessionId)
            || !string.Equals(path, ExpectedPath(sessionId), StringComparison.Ordinal)
            || !DigestPattern.IsMatch(payload.TenantIndex ?? string.Empty)
            || !DigestPattern.IsMatch(payload.OpaqueIdempotencyIndex ?? string.Empty)
            || !DigestPattern.IsMatch(payload.CanonicalDigest ?? string.Empty)
            || payload.CreatedAtUtc.Offset != TimeSpan.Zero
            || payload.ExpiresAtUtc.Offset != TimeSpan.Zero
            || payload.ExpiresAtUtc <= payload.CreatedAtUtc
            || payload.Profile is null
            || payload.Evidence is null
            || payload.Evidence.Revision != 1
            || !payload.Evidence.InitialRevisionAdvanced
            || payload.Evidence.NodeCount is < 0 or > DynamicGraphCompositionServerOptions.MaxGraphNodes
            || payload.Evidence.EdgeCount is < 0 or > DynamicGraphCompositionServerOptions.MaxGraphEdges
            || !IsKnownSourceKind(payload.Evidence.SourceKind))
        {
            throw InvalidState("A session checkpoint payload is invalid.");
        }

        var admission = DynamicGraphCompositionProfilePolicy.ValidateOwnedSnapshot(
            payload.Profile,
            payload.Profile.TenantId);
        if (!admission.IsAccepted
            || !string.Equals(admission.CanonicalDigest, payload.CanonicalDigest, StringComparison.Ordinal)
            || !string.Equals(
                DeriveIndex("tenant", payload.Profile.TenantId),
                payload.TenantIndex,
                StringComparison.Ordinal)
            || !string.Equals(payload.Profile.ProfileId, payload.Evidence.ProfileId, StringComparison.Ordinal))
        {
            throw InvalidState("A persisted composition profile is invalid.");
        }
    }

    private byte[] ComputeCheckpointMac(byte[] canonicalPayload)
    {
        var domain = Encoding.UTF8.GetBytes(CheckpointSchema);
        var signed = new byte[sizeof(int) + domain.Length + canonicalPayload.Length];
        Exception? computationFailure = null;
        try
        {
            BinaryPrimitives.WriteInt32BigEndian(signed.AsSpan(0, sizeof(int)), domain.Length);
            domain.CopyTo(signed, sizeof(int));
            canonicalPayload.CopyTo(signed, sizeof(int) + domain.Length);
            return HMACSHA256.HashData(_indexKey, signed);
        }
        catch (Exception exception)
        {
            computationFailure = exception;
            throw;
        }
        finally
        {
            if (computationFailure is null || IsNonFatalException(computationFailure))
            {
                Exception? signedZeroingFailure = null;
                try
                {
                    CryptographicOperations.ZeroMemory(signed);
                }
                catch (Exception exception)
                {
                    signedZeroingFailure = exception;
                    throw;
                }
                finally
                {
                    if (signedZeroingFailure is null || IsNonFatalException(signedZeroingFailure))
                        CryptographicOperations.ZeroMemory(domain);
                }
            }
        }
    }

    private static bool IsKnownSourceKind(string? sourceKind)
        => Enum.TryParse<DynamicGraphBuildSourceKind>(sourceKind, ignoreCase: false, out var parsed)
           && Enum.IsDefined(parsed)
           && string.Equals(sourceKind, parsed.ToString(), StringComparison.Ordinal);

    private static void EnsureStrictJson(byte[] payload)
    {
        var document = JsonDocument.Parse(payload, new JsonDocumentOptions
        {
            AllowTrailingCommas = false,
            CommentHandling = JsonCommentHandling.Disallow,
            MaxDepth = 64,
        });
        Exception? traversalFailure = null;
        try
        {
            EnsureNoDuplicateProperties(document.RootElement);
        }
        catch (Exception exception)
        {
            traversalFailure = exception;
            throw;
        }
        finally
        {
            if (traversalFailure is null || IsNonFatalException(traversalFailure))
                document.Dispose();
        }
    }

    private static void EnsureNoDuplicateProperties(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in element.EnumerateObject())
            {
                if (!names.Add(property.Name))
                    throw InvalidState("A session checkpoint contains duplicate JSON properties.");
                EnsureNoDuplicateProperties(property.Value);
            }
            return;
        }

        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
                EnsureNoDuplicateProperties(item);
        }
    }

    private string ExpectedPath(string sessionId)
        => DynamicGraphCompositionFileSystemPolicy.DirectChild(
            _sessionsDirectory!,
            $"{sessionId}.json");

    internal static DynamicGraphLogicalOrchestratorBootstrapProfile SnapshotProfile(
        DynamicGraphLogicalOrchestratorBootstrapProfile profile)
    {
        var serialized = JsonSerializer.SerializeToUtf8Bytes(
            profile,
            DynamicGraphCompositionJson.CheckpointOptions);
        Exception? deserializationFailure = null;
        try
        {
            return JsonSerializer.Deserialize<DynamicGraphLogicalOrchestratorBootstrapProfile>(
                       serialized,
                       DynamicGraphCompositionJson.CheckpointOptions)
                   ?? throw InvalidState("A persisted composition profile is required.");
        }
        catch (Exception exception)
        {
            deserializationFailure = exception;
            throw;
        }
        finally
        {
            if (deserializationFailure is null || IsNonFatalException(deserializationFailure))
                CryptographicOperations.ZeroMemory(serialized);
        }
    }

    private static byte[] LoadOrCreateIndexKey(string stateDirectory)
    {
        var path = DynamicGraphCompositionFileSystemPolicy.DirectChild(
            stateDirectory,
            IndexKeyFileName);
        DynamicGraphCompositionFileSystemPolicy.EnsureRegularFileOrMissing(path);
        if (DynamicGraphCompositionFileSystemPolicy.PathExists(path))
        {
            DynamicGraphCompositionFileSystemPolicy.VerifyExistingPrivateFile(path);
            var existing = DynamicGraphCompositionFileSystemPolicy.ReadBoundedFile(
                stateDirectory,
                IndexKeyFileName,
                minimumBytes: 32,
                maximumBytes: 32);
            if (existing.All(value => value == 0))
            {
                CryptographicOperations.ZeroMemory(existing);
                throw InvalidState("The persisted index key is invalid.");
            }
            return existing;
        }

        var created = RandomNumberGenerator.GetBytes(32);
        var temporaryName = $".{IndexKeyFileName}.{Guid.NewGuid():N}.tmp";
        var temporary = DynamicGraphCompositionFileSystemPolicy.DirectChild(
            stateDirectory,
            temporaryName);
        Exception? pendingFailure = null;
        try
        {
            WriteThrough(stateDirectory, temporaryName, created);
            DynamicGraphCompositionFileSystemPolicy.EnsureRegularFile(temporary);
            DynamicGraphCompositionFileSystemPolicy.EnsureRegularFileOrMissing(path);
            if (DynamicGraphCompositionFileSystemPolicy.PathExists(path))
            {
                DynamicGraphCompositionFileSystemPolicy.VerifyExistingPrivateFile(path);
                throw InvalidState("The persisted index key appeared during secure creation.");
            }
            File.Move(temporary, path, false);
            DynamicGraphCompositionFileSystemPolicy.EnsureRegularFile(path);
            DynamicGraphCompositionFileSystemPolicy.VerifyExistingPrivateFile(path);
            return created;
        }
        catch (Exception exception)
        {
            pendingFailure = exception;
            if (IsNonFatalException(exception))
            {
                try
                {
                    CryptographicOperations.ZeroMemory(created);
                }
                catch (Exception zeroingException)
                {
                    pendingFailure = zeroingException;
                    throw;
                }
            }
            throw;
        }
        finally
        {
            if (pendingFailure is null || IsNonFatalException(pendingFailure))
            {
                DynamicGraphCompositionFileSystemPolicy.DeletePrivateManagedFileIfPresent(
                    stateDirectory,
                    temporaryName);
            }
        }
    }

    private static void WriteNewAtomic(string directory, string fileName, byte[] payload)
    {
        var path = DynamicGraphCompositionFileSystemPolicy.DirectChild(directory, fileName);
        var temporaryName = $".{fileName}.{Guid.NewGuid():N}.tmp";
        var temporary = DynamicGraphCompositionFileSystemPolicy.DirectChild(
            directory,
            temporaryName);
        Exception? atomicFailure = null;
        try
        {
            DynamicGraphCompositionFileSystemPolicy.EnsureRegularFileOrMissing(path);
            WriteThrough(directory, temporaryName, payload);
            DynamicGraphCompositionFileSystemPolicy.EnsureRegularFile(temporary);
            DynamicGraphCompositionFileSystemPolicy.EnsureRegularFileOrMissing(path);
            if (DynamicGraphCompositionFileSystemPolicy.PathExists(path))
            {
                DynamicGraphCompositionFileSystemPolicy.VerifyExistingPrivateFile(path);
                throw InvalidState("A session checkpoint already exists.");
            }
            File.Move(temporary, path, false);
            DynamicGraphCompositionFileSystemPolicy.EnsureRegularFile(path);
            DynamicGraphCompositionFileSystemPolicy.VerifyExistingPrivateFile(path);
        }
        catch (Exception exception)
        {
            atomicFailure = exception;
            throw;
        }
        finally
        {
            if (atomicFailure is null || IsNonFatalException(atomicFailure))
            {
                Exception? zeroingFailure = null;
                try
                {
                    CryptographicOperations.ZeroMemory(payload);
                }
                catch (Exception exception)
                {
                    zeroingFailure = exception;
                    throw;
                }
                finally
                {
                    if (zeroingFailure is null || IsNonFatalException(zeroingFailure))
                    {
                        DynamicGraphCompositionFileSystemPolicy.DeletePrivateManagedFileIfPresent(
                            directory,
                            temporaryName);
                    }
                }
            }
        }
    }

    private static void WriteThrough(string directory, string fileName, byte[] payload)
    {
        var path = DynamicGraphCompositionFileSystemPolicy.DirectChild(directory, fileName);
        DynamicGraphCompositionFileSystemPolicy.EnsureRegularFileOrMissing(path);
        DynamicGraphCompositionFileSystemPolicy.VerifyExistingPrivateDirectory(
            directory,
            requireInheritableForChildren: true);
        var stream = DynamicGraphCompositionFileSystemPolicy.CreateNewPrivateFile(
            directory,
            fileName);
        Exception? writeFailure = null;
        try
        {
            stream.Write(payload);
            stream.Flush(flushToDisk: true);
        }
        catch (Exception exception)
        {
            writeFailure = exception;
            throw;
        }
        finally
        {
            if (writeFailure is null || IsNonFatalException(writeFailure))
                stream.Dispose();
        }
        DynamicGraphCompositionFileSystemPolicy.EnsureRegularFile(path);
        DynamicGraphCompositionFileSystemPolicy.VerifyExistingPrivateFile(path);
    }

    private void CleanupAfterFailedConstruction(byte[]? assignedIndexKey)
    {
        Exception? indexKeyFailure = null;
        try
        {
            if (assignedIndexKey is not null)
                CryptographicOperations.ZeroMemory(assignedIndexKey);
        }
        catch (Exception exception)
        {
            indexKeyFailure = exception;
            throw;
        }
        finally
        {
            if (indexKeyFailure is null || IsNonFatalException(indexKeyFailure))
            {
                Exception? lifetimeLockFailure = null;
                try
                {
                    _lifetimeLock?.Dispose();
                }
                catch (Exception exception)
                {
                    lifetimeLockFailure = exception;
                    throw;
                }
                finally
                {
                    if (lifetimeLockFailure is null || IsNonFatalException(lifetimeLockFailure))
                        _treeLease?.Dispose();
                }
            }
        }
    }

    internal static bool IsNonFatalException(Exception exception)
    {
        var pending = new Stack<Exception>();
        var visited = new HashSet<Exception>(ReferenceEqualityComparer.Instance);
        pending.Push(exception);

        while (pending.Count > 0)
        {
            var current = pending.Pop();
            if (!visited.Add(current))
                continue;

            if (current is OutOfMemoryException
                || current is StackOverflowException
                || current is AccessViolationException
                || current is AppDomainUnloadedException
                || current is BadImageFormatException
                || StringComparer.Ordinal.Equals(
                    current.GetType().FullName,
                    "System.Threading.ThreadAbortException"))
            {
                return false;
            }

            if (current is AggregateException aggregate)
            {
                foreach (var inner in aggregate.InnerExceptions)
                    pending.Push(inner);
            }
            else if (current.InnerException is not null)
            {
                pending.Push(current.InnerException);
            }
        }

        return true;
    }

    private static DynamicGraphCompositionPersistenceException InvalidState(string message)
        => new(message);

    private void AssertStateTreeIdentity()
    {
        if (!IsEnabled)
            return;
        if (_treeLease is null
            || _lifetimeLock is null
            || _lifetimeLock.SafeFileHandle.IsInvalid
            || _lifetimeLock.SafeFileHandle.IsClosed
            || _stateDirectory is null
            || _sessionsDirectory is null
            || _lockPath is null)
        {
            throw InvalidState("The managed state tree lease is unavailable.");
        }

        _treeLease.AssertUnchanged();
        DynamicGraphCompositionFileSystemPolicy.VerifyExistingPrivateDirectory(
            _stateDirectory,
            requireInheritableForChildren: true);
        DynamicGraphCompositionFileSystemPolicy.VerifyExistingPrivateDirectory(
            _sessionsDirectory,
            requireInheritableForChildren: true);
        DynamicGraphCompositionFileSystemPolicy.AssertHandleIdentity(
            _lifetimeLock.SafeFileHandle,
            _lockIdentity,
            expectDirectory: false);
        DynamicGraphCompositionFileSystemPolicy.AssertLockedFilePathIdentity(
            _lockPath,
            _lockIdentity);
        _treeLease.AssertUnchanged();
    }

    private OperationLease EnterOperation()
    {
        lock (_lifecycleGate)
        {
            ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);
            var lease = new OperationLease(this);
            _activeOperations = checked(_activeOperations + 1);
            return lease;
        }
    }

    private void ExitOperation()
    {
        lock (_lifecycleGate)
        {
            if (_activeOperations <= 0)
            {
                throw new InvalidOperationException(
                    "The dynamic graph persistence operation count is invalid.");
            }

            _activeOperations--;
            if (_activeOperations == 0)
                Monitor.PulseAll(_lifecycleGate);
        }
    }

    private void WaitForActiveOperations()
    {
        lock (_lifecycleGate)
        {
            while (_activeOperations != 0)
                Monitor.Wait(_lifecycleGate);
        }
    }

    private sealed class OperationLease : IDisposable
    {
        private DynamicGraphCompositionSessionPersistence? _owner;

        internal OperationLease(DynamicGraphCompositionSessionPersistence owner)
        {
            _owner = owner;
        }

        public void Dispose()
        {
            var owner = Interlocked.Exchange(ref _owner, null);
            owner?.ExitOperation();
        }
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        WaitForActiveOperations();

        Exception? indexKeyFailure = null;
        try
        {
            CryptographicOperations.ZeroMemory(_indexKey);
        }
        catch (Exception exception)
        {
            indexKeyFailure = exception;
            throw;
        }
        finally
        {
            if (indexKeyFailure is null || IsNonFatalException(indexKeyFailure))
            {
                Exception? lifetimeLockFailure = null;
                try
                {
                    _lifetimeLock?.Dispose();
                }
                catch (Exception exception)
                {
                    lifetimeLockFailure = exception;
                    throw;
                }
                finally
                {
                    if (lifetimeLockFailure is null || IsNonFatalException(lifetimeLockFailure))
                        _treeLease?.Dispose();
                }
            }
        }
    }
}

internal readonly record struct ManagedPathIdentity(ulong Device, ulong File);

internal sealed class ManagedDirectoryTreeLease : IDisposable
{
    private readonly List<(string Path, SafeFileHandle Handle, ManagedPathIdentity Identity)> _entries = [];
    private readonly object _lifecycleGate = new();
    private int _disposed;

    internal void Add(string path, SafeFileHandle handle, ManagedPathIdentity identity)
    {
        lock (_lifecycleGate)
        {
            ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);
            _entries.Add((path, handle, identity));
        }
    }

    internal void AddDirectory(string path) =>
        DynamicGraphCompositionFileSystemPolicy.AddDirectoryLease(this, path);

    internal void AssertUnchanged()
    {
        lock (_lifecycleGate)
        {
            ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);
            foreach (var entry in _entries)
            {
                DynamicGraphCompositionFileSystemPolicy.AssertHandleIdentity(
                    entry.Handle,
                    entry.Identity,
                    expectDirectory: true);
                DynamicGraphCompositionFileSystemPolicy.AssertPathIdentity(
                    entry.Path,
                    entry.Identity,
                    expectDirectory: true);
            }
        }
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        lock (_lifecycleGate)
        {
            ExceptionDispatchInfo? pendingFailure = null;
            for (var index = _entries.Count - 1; index >= 0; index--)
            {
                try
                {
                    _entries[index].Handle.Dispose();
                }
                catch (Exception exception)
                {
                    if (!DynamicGraphCompositionSessionPersistence.IsNonFatalException(exception))
                        throw;
                    pendingFailure = ExceptionDispatchInfo.Capture(exception);
                }
            }

            _entries.Clear();
            pendingFailure?.Throw();
        }
    }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record PersistedCompositionSessionEnvelope(
    [property: JsonPropertyName("schema")] string Schema,
    [property: JsonPropertyName("payload")] PersistedCompositionSession Payload,
    [property: JsonPropertyName("hmac_sha256")] string HmacSha256);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record PersistedCompositionSession(
    [property: JsonPropertyName("schema")] string Schema,
    [property: JsonPropertyName("session_id")] string SessionId,
    [property: JsonPropertyName("tenant_index")] string TenantIndex,
    [property: JsonPropertyName("opaque_idempotency_index")] string OpaqueIdempotencyIndex,
    [property: JsonPropertyName("canonical_digest")] string CanonicalDigest,
    [property: JsonPropertyName("created_at_utc")] DateTimeOffset CreatedAtUtc,
    [property: JsonPropertyName("expires_at_utc")] DateTimeOffset ExpiresAtUtc,
    [property: JsonPropertyName("profile")] DynamicGraphLogicalOrchestratorBootstrapProfile Profile,
    [property: JsonPropertyName("evidence")] CompositionEvidenceHttpResponse Evidence);

internal static class DynamicGraphCompositionFileSystemPolicy
{
    private const UnixFileMode PrivateDirectoryMode =
        UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute;
    private const UnixFileMode PrivateFileMode =
        UnixFileMode.UserRead | UnixFileMode.UserWrite;

    internal static StringComparison PathComparison =>
        OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

    internal static string NormalizeReadyFile(string path) =>
        NormalizeLocalAbsolute(path, requireAlreadyAbsolute: false, forbidFileSystemRoot: false);

    internal static string NormalizeStateDirectory(string path) =>
        NormalizeLocalAbsolute(path, requireAlreadyAbsolute: true, forbidFileSystemRoot: true);

    internal static void EnsureReadyFileOutsideState(string readyFile, string? stateDirectory)
    {
        if (stateDirectory is null)
            return;

        var normalizedReadyFile = NormalizeReadyFile(readyFile);
        var normalizedStateDirectory = NormalizeStateDirectory(stateDirectory);
        if (IsEqualOrDescendant(normalizedStateDirectory, normalizedReadyFile))
        {
            throw InvalidState(
                "The ready file must be outside the managed state directory and its reserved children.");
        }
    }

    internal static string DirectChild(string directory, string fileName)
    {
        var normalizedDirectory = NormalizeLocalAbsolute(
            directory,
            requireAlreadyAbsolute: true,
            forbidFileSystemRoot: false);
        if (string.IsNullOrWhiteSpace(fileName)
            || !string.Equals(Path.GetFileName(fileName), fileName, StringComparison.Ordinal)
            || fileName is "." or ".."
            || fileName.IndexOf(Path.DirectorySeparatorChar) >= 0
            || fileName.IndexOf(Path.AltDirectorySeparatorChar) >= 0)
        {
            throw InvalidState("A managed file name is invalid.");
        }

        var child = NormalizeLocalAbsolute(
            Path.Combine(normalizedDirectory, fileName),
            requireAlreadyAbsolute: true,
            forbidFileSystemRoot: false);
        var parent = Path.TrimEndingDirectorySeparator(Path.GetDirectoryName(child)!);
        if (!string.Equals(parent, normalizedDirectory, PathComparison))
            throw InvalidState("A managed path escaped its direct parent.");
        return child;
    }

    internal static ManagedDirectoryTreeLease AcquireTrustedDirectoryTreeLease(string path)
    {
        var normalized = NormalizeLocalAbsolute(
            path,
            requireAlreadyAbsolute: true,
            forbidFileSystemRoot: true);
        VerifyTrustedAncestorChain(normalized);
        VerifyExistingPrivateDirectory(normalized, requireInheritableForChildren: true);

        var lease = new ManagedDirectoryTreeLease();
        try
        {
            foreach (var directory in EnumerateExistingDirectoryChain(normalized))
                AddDirectoryLease(lease, directory);
            lease.AssertUnchanged();
            return lease;
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            lease.Dispose();
            throw;
        }
    }

    internal static void AddDirectoryLease(ManagedDirectoryTreeLease lease, string path)
    {
        ArgumentNullException.ThrowIfNull(lease);
        var normalized = NormalizeLocalAbsolute(
            path,
            requireAlreadyAbsolute: true,
            forbidFileSystemRoot: false);
        VerifyTrustedAncestorChain(normalized);
        EnsureDirectory(normalized);

        var handle = OpenDirectoryLeaseHandle(normalized);
        try
        {
            var identity = CaptureHandleIdentity(handle, expectDirectory: true);
            AssertPathIdentity(normalized, identity, expectDirectory: true);
            lease.Add(normalized, handle, identity);
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            handle.Dispose();
            throw;
        }
    }

    internal static void EnsurePrivateDirectoryTree(string path)
    {
        var normalized = NormalizeLocalAbsolute(
            path,
            requireAlreadyAbsolute: true,
            forbidFileSystemRoot: true);
        VerifyTrustedAncestorChain(normalized);
        EnsureSafeExistingComponents(normalized);
        if (PathExists(normalized))
        {
            VerifyExistingPrivateDirectory(
                normalized,
                requireInheritableForChildren: true);
            return;
        }

        var missing = new Stack<string>();
        var parent = normalized;
        while (!PathExists(parent))
        {
            var name = Path.GetFileName(parent);
            if (string.IsNullOrWhiteSpace(name))
                throw InvalidState("A private parent directory is required for secure creation.");
            missing.Push(name);
            parent = Path.GetDirectoryName(parent)
                     ?? throw InvalidState("A private parent directory is required for secure creation.");
        }

        VerifyExistingPrivateDirectory(parent, requireInheritableForChildren: true);
        while (missing.Count > 0)
        {
            VerifyExistingPrivateDirectory(parent, requireInheritableForChildren: true);
            var child = DirectChild(parent, missing.Pop());
            var parentHandle = OpenDirectoryLeaseHandle(parent);
            Exception? creationFailure = null;
            try
            {
                var parentIdentity = CaptureHandleIdentity(parentHandle, expectDirectory: true);
                AssertPathIdentity(parent, parentIdentity, expectDirectory: true);
                CreatePrivateDirectoryExclusive(child);
                AssertHandleIdentity(parentHandle, parentIdentity, expectDirectory: true);
                AssertPathIdentity(parent, parentIdentity, expectDirectory: true);

                EnsureSafeExistingComponents(child);
                VerifyExistingPrivateDirectory(child, requireInheritableForChildren: true);
                parent = child;
            }
            catch (Exception exception)
            {
                creationFailure = exception;
                throw;
            }
            finally
            {
                if (creationFailure is null || IsNonFatalException(creationFailure))
                    parentHandle.Dispose();
            }
        }
    }

    private static IReadOnlyList<string> EnumerateExistingDirectoryChain(string path)
    {
        var normalized = NormalizeLocalAbsolute(
            path,
            requireAlreadyAbsolute: true,
            forbidFileSystemRoot: false);
        var root = Path.GetPathRoot(normalized)
                   ?? throw InvalidState("A managed path root is invalid.");
        var directories = new List<string> { root };
        EnsureDirectory(root);
        var current = root;
        foreach (var component in normalized[root.Length..].Split(
                     [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                     StringSplitOptions.RemoveEmptyEntries))
        {
            current = Path.Combine(current, component);
            EnsureDirectory(current);
            directories.Add(current);
        }
        return directories;
    }

    private static void VerifyTrustedAncestorChain(string path)
    {
        var normalized = NormalizeLocalAbsolute(
            path,
            requireAlreadyAbsolute: true,
            forbidFileSystemRoot: false);
        var root = Path.GetPathRoot(normalized)
                   ?? throw InvalidState("A managed path root is invalid.");
        var current = root;
        EnsureDirectory(current);

        foreach (var component in normalized[root.Length..].Split(
                     [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                     StringSplitOptions.RemoveEmptyEntries))
        {
            var next = Path.Combine(current, component);
            var childExists = TryGetAttributes(next, out var childAttributes);
            if (childExists)
            {
                CheckExistingComponent(next);
                if ((childAttributes & FileAttributes.Directory) == 0)
                    throw InvalidState("A managed ancestor component is not a directory.");
            }

            VerifyTrustedAncestorDirectory(current, childExists ? next : null);
            if (!childExists)
                return;
            current = next;
        }
    }

    private static void VerifyTrustedAncestorDirectory(string path, string? existingChildPath)
    {
        EnsureDirectory(path);
        if (OperatingSystem.IsWindows())
        {
            VerifyTrustedWindowsAncestorAcl(path, existingChildPath is not null);
            return;
        }
        if (!OperatingSystem.IsLinux())
            throw InvalidState("Trusted ancestor authority cannot be proven on this platform.");

        var effectiveUserId = GetEffectiveUserId();
        var parent = GetLinuxPathStatus(path);
        if (parent.UserId != 0 && parent.UserId != effectiveUserId)
            throw InvalidState("A managed ancestor is owned by an untrusted identity.");

        const ushort groupOrOtherWrite = 0x0012;
        const ushort sticky = 0x0200;
        if ((parent.Mode & groupOrOtherWrite) == 0)
            return;
        if ((parent.Mode & sticky) == 0)
            throw InvalidState("A managed ancestor grants untrusted rename or delete authority.");

        if (existingChildPath is null)
            return;
        var child = GetLinuxPathStatus(existingChildPath);
        if (child.UserId != 0 && child.UserId != effectiveUserId)
        {
            throw InvalidState(
                "A sticky ancestor does not protect a child owned by an untrusted identity.");
        }
    }

    private static void CreatePrivateDirectoryExclusive(string path)
    {
        if (OperatingSystem.IsWindows())
        {
            if (!CreateDirectoryWindows(path, IntPtr.Zero))
            {
                var error = Marshal.GetLastWin32Error();
                throw InvalidState(
                    $"A private managed directory could not be created exclusively (error {error}).");
            }
            return;
        }
        if (!OperatingSystem.IsLinux())
            throw InvalidState("Exclusive private directory creation is unsupported on this platform.");
        if (Mkdir(path, (uint)PrivateDirectoryMode) != 0)
        {
            var error = Marshal.GetLastWin32Error();
            throw InvalidState(
                $"A private managed directory could not be created exclusively (error {error}).");
        }
    }

    internal static void VerifyExistingPrivateDirectory(
        string path,
        bool requireInheritableForChildren)
    {
        EnsureDirectory(path);
        VerifyCurrentOwner(path);
        if (OperatingSystem.IsWindows())
        {
            VerifyPrivateWindowsAcl(path, isDirectory: true, requireInheritableForChildren);
            return;
        }

        var mode = File.GetUnixFileMode(path);
        if ((mode & ~PrivateDirectoryMode) != 0
            || (mode & PrivateDirectoryMode) != PrivateDirectoryMode)
        {
            throw InvalidState("An existing managed directory is not owner-only.");
        }
    }

    internal static void VerifyExistingPrivateFile(string path)
    {
        EnsureRegularFile(path);
        VerifyCurrentOwner(path);
        if (OperatingSystem.IsWindows())
        {
            VerifyPrivateWindowsAcl(
                path,
                isDirectory: false,
                requireInheritableForChildren: false);
            return;
        }

        var mode = File.GetUnixFileMode(path);
        const UnixFileMode ownerBits =
            UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute;
        if ((mode & ~ownerBits) != 0 || (mode & UnixFileMode.UserRead) == 0)
            throw InvalidState("An existing managed file is not owner-only.");
    }

    internal static FileStream CreateNewPrivateFile(string directory, string fileName)
    {
        VerifyExistingPrivateDirectory(directory, requireInheritableForChildren: true);
        var path = DirectChild(directory, fileName);
        EnsureRegularFileOrMissing(path);
        if (PathExists(path))
            throw InvalidState("A managed file already exists.");

        var streamOptions = new FileStreamOptions
        {
            Mode = FileMode.CreateNew,
            Access = FileAccess.Write,
            Share = FileShare.None,
            BufferSize = 4096,
            Options = FileOptions.WriteThrough,
        };
        if (!OperatingSystem.IsWindows())
            streamOptions.UnixCreateMode = PrivateFileMode;
        return new FileStream(path, streamOptions);
    }

    internal static void EnsureDirectory(string path)
    {
        var normalized = NormalizeLocalAbsolute(
            path,
            requireAlreadyAbsolute: true,
            forbidFileSystemRoot: false);
        EnsureSafeExistingComponents(normalized);
        if (!TryGetAttributes(normalized, out var attributes)
            || (attributes & FileAttributes.Directory) == 0
            || (attributes & FileAttributes.ReparsePoint) != 0)
        {
            throw InvalidState("A managed directory is missing or unsafe.");
        }
    }

    internal static void EnsureRegularFileOrMissing(string path)
    {
        var normalized = NormalizeLocalAbsolute(
            path,
            requireAlreadyAbsolute: true,
            forbidFileSystemRoot: false);
        EnsureSafeExistingComponents(normalized);
        if (TryGetAttributes(normalized, out var attributes)
            && ((attributes & FileAttributes.Directory) != 0
                || (attributes & FileAttributes.ReparsePoint) != 0))
        {
            throw InvalidState("A managed file path is not a regular file.");
        }
    }

    internal static void EnsureRegularFile(string path)
    {
        EnsureRegularFileOrMissing(path);
        if (!TryGetAttributes(path, out _))
            throw InvalidState("A managed file is missing.");
    }

    internal static bool PathExists(string path)
    {
        EnsureSafeExistingComponents(path);
        return TryGetAttributes(path, out _);
    }

    internal static byte[] ReadBoundedFile(
        string directory,
        string fileName,
        int minimumBytes,
        int maximumBytes)
    {
        if (minimumBytes < 0 || maximumBytes < minimumBytes)
            throw InvalidState("A managed file byte bound is invalid.");

        VerifyExistingPrivateDirectory(directory, requireInheritableForChildren: true);
        var path = DirectChild(directory, fileName);
        VerifyExistingPrivateFile(path);
        var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.None,
            bufferSize: 4096,
            FileOptions.SequentialScan);
        byte[]? payload = null;
        var payloadZeroingAttempted = false;
        Exception? pendingFailure = null;
        try
        {
            VerifyExistingPrivateFile(path);
            var length = stream.Length;
            if (length < minimumBytes || length > maximumBytes || length > int.MaxValue)
                throw InvalidState("A managed file exceeds its bounded size.");

            payload = new byte[(int)length];
            var offset = 0;
            while (offset < payload.Length)
            {
                var read = stream.Read(payload, offset, payload.Length - offset);
                if (read == 0)
                    throw InvalidState("A managed file ended before its declared length.");
                offset += read;
            }
            if (stream.ReadByte() != -1)
                throw InvalidState("A managed file grew beyond its bounded length.");
            VerifyExistingPrivateFile(path);
            return payload;
        }
        catch (Exception exception)
        {
            pendingFailure = exception;
            if (payload is not null && IsNonFatalException(exception))
            {
                payloadZeroingAttempted = true;
                try
                {
                    CryptographicOperations.ZeroMemory(payload);
                }
                catch (Exception zeroingException)
                {
                    pendingFailure = zeroingException;
                    throw;
                }
            }
            throw;
        }
        finally
        {
            if (pendingFailure is null || IsNonFatalException(pendingFailure))
            {
                Exception? streamFailure = null;
                try
                {
                    stream.Dispose();
                }
                catch (Exception exception)
                {
                    streamFailure = exception;
                    throw;
                }
                finally
                {
                    if (streamFailure is not null
                        && IsNonFatalException(streamFailure)
                        && payload is not null
                        && !payloadZeroingAttempted)
                    {
                        payloadZeroingAttempted = true;
                        CryptographicOperations.ZeroMemory(payload);
                    }
                }
            }
        }
    }

    internal static void DeleteManagedFileIfPresent(string directory, string fileName)
    {
        EnsureDirectory(directory);
        var path = DirectChild(directory, fileName);
        EnsureRegularFileOrMissing(path);
        if (!TryGetAttributes(path, out _))
            return;

        EnsureRegularFile(path);
        File.Delete(path);
        EnsureSafeExistingComponents(directory);
        if (TryGetAttributes(path, out _))
            throw InvalidState("A managed file remained after deletion.");
    }

    internal static void DeletePrivateManagedFileIfPresent(string directory, string fileName)
    {
        VerifyExistingPrivateDirectory(directory, requireInheritableForChildren: true);
        var path = DirectChild(directory, fileName);
        EnsureRegularFileOrMissing(path);
        if (!TryGetAttributes(path, out _))
            return;

        VerifyExistingPrivateFile(path);
        File.Delete(path);
        VerifyExistingPrivateDirectory(directory, requireInheritableForChildren: true);
        if (TryGetAttributes(path, out _))
            throw InvalidState("A private managed file remained after deletion.");
    }

    internal static void EnsureExpectedStateRootContents(
        string stateDirectory,
        string lockFileName,
        string indexKeyFileName,
        string sessionsDirectoryName)
    {
        EnsureDirectory(stateDirectory);
        var allowed = new HashSet<string>(
            [lockFileName, indexKeyFileName, sessionsDirectoryName],
            OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
        foreach (var entry in Directory.EnumerateFileSystemEntries(
                     stateDirectory,
                     "*",
                     SearchOption.TopDirectoryOnly))
        {
            var name = Path.GetFileName(entry);
            if (!allowed.Contains(name)
                || !string.Equals(entry, DirectChild(stateDirectory, name), PathComparison))
            {
                throw InvalidState("Unexpected content exists in the managed state directory.");
            }
            EnsureSafeExistingComponents(entry);
        }
        EnsureRegularFile(DirectChild(stateDirectory, lockFileName));
        EnsureRegularFile(DirectChild(stateDirectory, indexKeyFileName));
        EnsureDirectory(DirectChild(stateDirectory, sessionsDirectoryName));
    }

    internal static void EnsureSafeExistingComponents(string path)
    {
        var normalized = NormalizeLocalAbsolute(
            path,
            requireAlreadyAbsolute: true,
            forbidFileSystemRoot: false);
        var root = Path.GetPathRoot(normalized)
                   ?? throw InvalidState("A managed path root is invalid.");
        var current = root;

        CheckExistingComponent(current);
        var relative = normalized[root.Length..];
        foreach (var component in relative.Split(
                     [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                     StringSplitOptions.RemoveEmptyEntries))
        {
            current = Path.Combine(current, component);
            if (!CheckExistingComponent(current))
                break;
        }
    }

    private static string NormalizeLocalAbsolute(
        string path,
        bool requireAlreadyAbsolute,
        bool forbidFileSystemRoot)
    {
        if (string.IsNullOrWhiteSpace(path) || path.IndexOf('\0') >= 0)
            throw InvalidState("A local filesystem path is required.");
        if (requireAlreadyAbsolute && !Path.IsPathFullyQualified(path))
            throw InvalidState("A managed state path must be absolute.");

        string fullPath;
        try
        {
            fullPath = Path.GetFullPath(path);
        }
        catch (Exception exception) when (
            (exception is ArgumentException
                or NotSupportedException
                or PathTooLongException)
            && IsNonFatalException(exception))
        {
            throw InvalidState("A local filesystem path is invalid.", exception);
        }
        if (!Path.IsPathFullyQualified(fullPath))
            throw InvalidState("A local filesystem path must be fully qualified.");

        var root = Path.GetPathRoot(fullPath)
                   ?? throw InvalidState("A local filesystem path root is invalid.");
        if (OperatingSystem.IsWindows())
        {
            if (fullPath.StartsWith("\\\\", StringComparison.Ordinal)
                || fullPath.StartsWith("\\\\?\\", StringComparison.Ordinal)
                || fullPath.StartsWith("\\\\.\\", StringComparison.Ordinal)
                || fullPath[root.Length..].Contains(':'))
            {
                throw InvalidState("UNC, device, and alternate-stream paths are forbidden.");
            }
            DriveInfo drive;
            try { drive = new DriveInfo(root); }
            catch (Exception exception) when (
                (exception is ArgumentException or IOException)
                && IsNonFatalException(exception))
            {
                throw InvalidState("A local fixed drive is required.", exception);
            }
            if (drive.DriveType != DriveType.Fixed)
                throw InvalidState("A local fixed drive is required.");
            ValidateWindowsComponents(fullPath[root.Length..]);
        }

        var normalized = forbidFileSystemRoot
            ? Path.TrimEndingDirectorySeparator(fullPath)
            : fullPath;
        if (forbidFileSystemRoot
            && string.Equals(
                normalized,
                Path.TrimEndingDirectorySeparator(root),
                PathComparison))
        {
            throw InvalidState("The filesystem root cannot be used as managed state.");
        }
        return normalized;
    }

    private static void ValidateWindowsComponents(string relativePath)
    {
        foreach (var component in relativePath.Split(
                     [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                     StringSplitOptions.RemoveEmptyEntries))
        {
            if (component.EndsWith(' ') || component.EndsWith('.'))
                throw InvalidState("A Windows path component is ambiguous.");
            var stem = component.Split('.', 2)[0];
            if (stem.Equals("CON", StringComparison.OrdinalIgnoreCase)
                || stem.Equals("PRN", StringComparison.OrdinalIgnoreCase)
                || stem.Equals("AUX", StringComparison.OrdinalIgnoreCase)
                || stem.Equals("NUL", StringComparison.OrdinalIgnoreCase)
                || (stem.Length == 4
                    && (stem.StartsWith("COM", StringComparison.OrdinalIgnoreCase)
                        || stem.StartsWith("LPT", StringComparison.OrdinalIgnoreCase))
                    && stem[3] is >= '1' and <= '9'))
            {
                throw InvalidState("A reserved Windows path component is forbidden.");
            }
        }
    }

    private static bool IsEqualOrDescendant(string root, string candidate)
    {
        var relative = Path.GetRelativePath(root, candidate);
        if (string.Equals(relative, ".", StringComparison.Ordinal))
            return true;
        return !string.Equals(relative, "..", StringComparison.Ordinal)
               && !relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
               && !relative.StartsWith($"..{Path.AltDirectorySeparatorChar}", StringComparison.Ordinal)
               && !Path.IsPathFullyQualified(relative);
    }

    private static bool CheckExistingComponent(string path)
    {
        if (!TryGetAttributes(path, out var attributes))
            return false;
        if ((attributes & FileAttributes.ReparsePoint) != 0)
            throw InvalidState("A symlink, junction, or reparse point is forbidden in a managed path.");

        FileSystemInfo info = (attributes & FileAttributes.Directory) != 0
            ? new DirectoryInfo(path)
            : new FileInfo(path);
        if (info.LinkTarget is not null)
            throw InvalidState("A symbolic link is forbidden in a managed path.");
        return true;
    }

    private static bool TryGetAttributes(string path, out FileAttributes attributes)
    {
        try
        {
            attributes = File.GetAttributes(path);
            return true;
        }
        catch (FileNotFoundException exception) when (IsNonFatalException(exception))
        {
            attributes = default;
            return false;
        }
        catch (DirectoryNotFoundException exception) when (IsNonFatalException(exception))
        {
            attributes = default;
            return false;
        }
    }

    internal static ManagedPathIdentity CaptureHandleIdentity(
        SafeFileHandle handle,
        bool expectDirectory)
    {
        if (handle.IsInvalid || handle.IsClosed)
            throw InvalidState("A managed path identity handle is unavailable.");

        if (OperatingSystem.IsWindows())
        {
            if (!GetFileInformationByHandle(handle, out var information))
            {
                throw InvalidState(
                    $"A managed Windows path identity could not be read (error {Marshal.GetLastWin32Error()}).");
            }
            var isDirectory = (information.FileAttributes & (uint)FileAttributes.Directory) != 0;
            if (isDirectory != expectDirectory
                || (information.FileAttributes & (uint)FileAttributes.ReparsePoint) != 0)
            {
                throw InvalidState("A managed Windows path identity has an unsafe type.");
            }
            return new ManagedPathIdentity(
                information.VolumeSerialNumber,
                ((ulong)information.FileIndexHigh << 32) | information.FileIndexLow);
        }
        if (!OperatingSystem.IsLinux())
            throw InvalidState("Managed path identity cannot be proven on this platform.");

        var descriptor = handle.DangerousGetHandle().ToInt32();
        var status = GetLinuxHandleStatus(descriptor);
        EnsureLinuxType(status, expectDirectory);
        return LinuxIdentity(status);
    }

    internal static void AssertHandleIdentity(
        SafeFileHandle handle,
        ManagedPathIdentity expected,
        bool expectDirectory)
    {
        if (CaptureHandleIdentity(handle, expectDirectory) != expected)
            throw InvalidState("A managed handle changed filesystem identity.");
    }

    internal static void AssertPathIdentity(
        string path,
        ManagedPathIdentity expected,
        bool expectDirectory)
    {
        var actual = CapturePathIdentity(path, expectDirectory);
        if (actual != expected)
            throw InvalidState("A managed path was replaced after trust admission.");
    }

    internal static void AssertLockedFilePathIdentity(
        string path,
        ManagedPathIdentity expected)
    {
        EnsureRegularFile(path);
        if (OperatingSystem.IsWindows())
        {
            // FileShare.None on the lifetime lock denies delete/rename sharing for this name.
            return;
        }
        AssertPathIdentity(path, expected, expectDirectory: false);
    }

    private static ManagedPathIdentity CapturePathIdentity(string path, bool expectDirectory)
    {
        if (OperatingSystem.IsWindows())
        {
            var handle = OpenWindowsPathHandle(
                path,
                expectDirectory,
                denyDeleteSharing: false);
            Exception? identityFailure = null;
            try
            {
                return CaptureHandleIdentity(handle, expectDirectory);
            }
            catch (Exception exception)
            {
                identityFailure = exception;
                throw;
            }
            finally
            {
                if (identityFailure is null || IsNonFatalException(identityFailure))
                    handle.Dispose();
            }
        }
        if (!OperatingSystem.IsLinux())
            throw InvalidState("Managed path identity cannot be proven on this platform.");

        var status = GetLinuxPathStatus(path);
        EnsureLinuxType(status, expectDirectory);
        return LinuxIdentity(status);
    }

    private static SafeFileHandle OpenDirectoryLeaseHandle(string path)
    {
        EnsureDirectory(path);
        if (OperatingSystem.IsWindows())
            return OpenWindowsPathHandle(path, expectDirectory: true, denyDeleteSharing: true);
        if (!OperatingSystem.IsLinux())
            throw InvalidState("A stable directory lease is unsupported on this platform.");

        var descriptor = OpenUnixDirectory(
            path,
            UnixOpenReadOnly | UnixOpenDirectory | UnixOpenNoFollow | UnixOpenCloseOnExec);
        if (descriptor < 0)
        {
            throw InvalidState(
                $"A stable directory lease could not be opened (error {Marshal.GetLastWin32Error()}).");
        }
        var handle = new SafeFileHandle((IntPtr)descriptor, ownsHandle: true);
        try
        {
            CaptureHandleIdentity(handle, expectDirectory: true);
            return handle;
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            handle.Dispose();
            throw;
        }
    }

    private static SafeFileHandle OpenWindowsPathHandle(
        string path,
        bool expectDirectory,
        bool denyDeleteSharing)
    {
        if (expectDirectory)
            EnsureDirectory(path);
        else
            EnsureRegularFile(path);

        var share = WindowsFileShareRead | WindowsFileShareWrite;
        if (!denyDeleteSharing)
            share |= WindowsFileShareDelete;
        var flags = WindowsFileFlagOpenReparsePoint;
        if (expectDirectory)
            flags |= WindowsFileFlagBackupSemantics;
        var handle = CreateFileWindows(
            path,
            WindowsFileReadAttributes,
            share,
            IntPtr.Zero,
            WindowsOpenExisting,
            flags,
            IntPtr.Zero);
        if (handle.IsInvalid)
        {
            var error = Marshal.GetLastWin32Error();
            handle.Dispose();
            throw InvalidState($"A managed Windows path could not be leased (error {error}).");
        }
        try
        {
            CaptureHandleIdentity(handle, expectDirectory);
            return handle;
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            handle.Dispose();
            throw;
        }
    }

    private static LinuxStatx GetLinuxPathStatus(string path)
    {
        if (Statx(
                AtFileDescriptorCurrentWorkingDirectory,
                path,
                AtSymlinkNoFollow,
                StatxBasicStats,
                out var status) != 0
            || (status.Mask & StatxRequiredIdentity) != StatxRequiredIdentity)
        {
            throw InvalidState(
                $"A managed Linux path identity could not be read (error {Marshal.GetLastWin32Error()}).");
        }
        return status;
    }

    private static LinuxStatx GetLinuxHandleStatus(int descriptor)
    {
        if (Statx(
                descriptor,
                string.Empty,
                AtEmptyPath | AtSymlinkNoFollow,
                StatxBasicStats,
                out var status) != 0
            || (status.Mask & StatxRequiredIdentity) != StatxRequiredIdentity)
        {
            throw InvalidState(
                $"A managed Linux handle identity could not be read (error {Marshal.GetLastWin32Error()}).");
        }
        return status;
    }

    private static ManagedPathIdentity LinuxIdentity(LinuxStatx status) =>
        new(((ulong)status.DeviceMajor << 32) | status.DeviceMinor, status.Inode);

    private static void EnsureLinuxType(LinuxStatx status, bool expectDirectory)
    {
        const ushort fileTypeMask = 0xF000;
        const ushort directoryType = 0x4000;
        const ushort regularFileType = 0x8000;
        var expected = expectDirectory ? directoryType : regularFileType;
        if ((status.Mode & fileTypeMask) != expected)
            throw InvalidState("A managed Linux path identity has an unsafe type.");
    }

    private static void VerifyCurrentOwner(string path)
    {
        if (OperatingSystem.IsWindows())
            return;
        if (!OperatingSystem.IsLinux())
            throw InvalidState("The current owner cannot be proven on this platform.");
        if (GetLinuxPathStatus(path).UserId != GetEffectiveUserId())
            throw InvalidState("A managed path is not owned by the current identity.");
    }

    private static void VerifyTrustedWindowsAncestorAcl(string path, bool existingChildIsPresent)
    {
        try
        {
            var currentSid = GetCurrentWindowsSid();
            var trustedIdentities = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                currentSid,
                "S-1-5-18",
                "S-1-5-32-544",
                "S-1-5-80-956008885-3418522649-1831038044-1853292631-2271478464",
            };
            var acl = ReadWindowsAcl(path, isDirectory: true);
            if (!trustedIdentities.Contains(acl.OwnerIdentity))
                throw InvalidState("A managed Windows ancestor has an untrusted owner.");

            const long createFiles = 0x0002;
            const long createDirectories = 0x0004;
            const long deleteChildren = 0x0040;
            const long delete = 0x0001_0000;
            const long changePermissions = 0x0004_0000;
            const long takeOwnership = 0x0008_0000;
            const long genericAll = 0x1000_0000;
            const long genericWrite = 0x4000_0000;
            const long inheritOnly = 0x0002;
            var redirectAuthority = deleteChildren
                                    | delete
                                    | changePermissions
                                    | takeOwnership
                                    | genericAll;
            if (!existingChildIsPresent)
                redirectAuthority |= createFiles | createDirectories | genericWrite;
            foreach (var rule in acl.Rules)
            {
                if (string.Equals(rule.AccessType, "Allow", StringComparison.Ordinal)
                    && (rule.PropagationFlags & inheritOnly) == 0
                    && !trustedIdentities.Contains(rule.Identity)
                    && (rule.Rights & redirectAuthority) != 0)
                {
                    throw InvalidState(
                        "A managed Windows ancestor grants untrusted rename or delete authority.");
                }
            }
        }
        catch (DynamicGraphCompositionPersistenceException)
        {
            throw;
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            throw InvalidState("Trusted Windows ancestor authority could not be verified.", exception);
        }
    }

    private static void VerifyPrivateWindowsAcl(
        string path,
        bool isDirectory,
        bool requireInheritableForChildren)
    {
        try
        {
            var currentSid = GetCurrentWindowsSid();
            var acl = ReadWindowsAcl(path, isDirectory);
            if (!string.Equals(acl.OwnerIdentity, currentSid, StringComparison.OrdinalIgnoreCase))
                throw InvalidState("A managed Windows path is not owned by the current identity.");

            var allowedIdentities = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                currentSid,
                "S-1-5-18",
                "S-1-5-32-544",
            };
            const long fullControlBits = 0x001F_01FF;
            const long requiredInheritanceBits = 0x0003;
            const long inheritOnly = 0x0002;
            var hasOwnerFullControl = false;
            var hasOwnerChildInheritance = !isDirectory || !requireInheritableForChildren;

            foreach (var rule in acl.Rules)
            {
                if (!allowedIdentities.Contains(rule.Identity)
                    || !string.Equals(rule.AccessType, "Allow", StringComparison.Ordinal))
                {
                    throw InvalidState("A managed Windows path grants non-private access.");
                }

                if (!string.Equals(rule.Identity, currentSid, StringComparison.OrdinalIgnoreCase))
                    continue;
                if ((rule.Rights & fullControlBits) != fullControlBits)
                    continue;

                if ((rule.PropagationFlags & inheritOnly) == 0)
                    hasOwnerFullControl = true;
                if (isDirectory && requireInheritableForChildren)
                {
                    if ((rule.InheritanceFlags & requiredInheritanceBits)
                        == requiredInheritanceBits)
                    {
                        hasOwnerChildInheritance = true;
                    }
                }
            }

            if (!hasOwnerFullControl || !hasOwnerChildInheritance)
            {
                throw InvalidState(
                    "A managed Windows path does not grant private inheritable owner control.");
            }
        }
        catch (DynamicGraphCompositionPersistenceException)
        {
            throw;
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            throw InvalidState("Private Windows access control could not be verified.", exception);
        }
    }

    private static bool IsNonFatalException(Exception exception)
    {
        var pending = new Stack<Exception>();
        var visited = new HashSet<Exception>(ReferenceEqualityComparer.Instance);
        pending.Push(exception);

        while (pending.Count > 0)
        {
            var current = pending.Pop();
            if (!visited.Add(current))
                continue;

            if (current is OutOfMemoryException
                || current is StackOverflowException
                || current is AccessViolationException
                || current is AppDomainUnloadedException
                || current is BadImageFormatException
                || StringComparer.Ordinal.Equals(
                    current.GetType().FullName,
                    "System.Threading.ThreadAbortException"))
            {
                return false;
            }

            if (current is AggregateException aggregate)
            {
                foreach (var inner in aggregate.InnerExceptions)
                    pending.Push(inner);
            }
            else if (current.InnerException is not null)
            {
                pending.Push(current.InnerException);
            }
        }

        return true;
    }

    private static string GetCurrentWindowsSid()
    {
        IDisposable? identityScope = null;
        Exception? pendingFailure = null;
        try
        {
            var windowsIdentityType = Type.GetType(
                "System.Security.Principal.WindowsIdentity, System.Security.Principal.Windows",
                throwOnError: true)!;
            var sidType = Type.GetType(
                "System.Security.Principal.SecurityIdentifier, System.Security.Principal.Windows",
                throwOnError: true)!;
            var identity = windowsIdentityType.GetMethod(
                    "GetCurrent",
                    BindingFlags.Public | BindingFlags.Static,
                    binder: null,
                    types: Type.EmptyTypes,
                    modifiers: null)!
                .Invoke(null, null)
                ?? throw InvalidState("The current Windows identity is unavailable.");
            identityScope = identity as IDisposable;
            var sid = windowsIdentityType.GetProperty("User")!.GetValue(identity)
                      ?? throw InvalidState("The current Windows security identifier is unavailable.");
            return sidType.GetProperty("Value")!.GetValue(sid) as string
                   ?? throw InvalidState("The current Windows security identifier is invalid.");
        }
        catch (Exception exception)
        {
            pendingFailure = exception;
            throw;
        }
        finally
        {
            if (pendingFailure is null || IsNonFatalException(pendingFailure))
                identityScope?.Dispose();
        }
    }

    private static WindowsAclSnapshot ReadWindowsAcl(string path, bool isDirectory)
    {
        var aclExtensions = Type.GetType(
            "System.IO.FileSystemAclExtensions, System.IO.FileSystem.AccessControl",
            throwOnError: true)!;
        var sidType = Type.GetType(
            "System.Security.Principal.SecurityIdentifier, System.Security.Principal.Windows",
            throwOnError: true)!;
        var ruleType = Type.GetType(
            "System.Security.AccessControl.FileSystemAccessRule, System.IO.FileSystem.AccessControl",
            throwOnError: true)!;

        FileSystemInfo info = isDirectory ? new DirectoryInfo(path) : new FileInfo(path);
        var infoType = info.GetType();
        var getAccessControl = aclExtensions.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(method => method.Name == "GetAccessControl"
                              && method.GetParameters() is [{ ParameterType: var parameterType }]
                              && parameterType == infoType);
        var security = getAccessControl.Invoke(null, [info])
                       ?? throw InvalidState("Windows access control could not be read.");
        var descriptor = security.GetType()
                             .GetMethod("GetSecurityDescriptorBinaryForm", Type.EmptyTypes)!
                             .Invoke(security, null) as byte[]
                         ?? throw InvalidState("A Windows security descriptor is unavailable.");
        if (descriptor.Length < 20)
            throw InvalidState("A Windows security descriptor is truncated.");
        var control = BinaryPrimitives.ReadUInt16LittleEndian(descriptor.AsSpan(2, 2));
        var discretionaryAclOffset = BinaryPrimitives.ReadUInt32LittleEndian(
            descriptor.AsSpan(16, 4));
        if ((control & 0x0004) == 0 || discretionaryAclOffset == 0)
            throw InvalidState("A null or absent Windows DACL is forbidden.");

        var owner = security.GetType().GetMethod("GetOwner")!.Invoke(security, [sidType]);
        var ownerIdentity = owner is null
            ? null
            : sidType.GetProperty("Value")!.GetValue(owner) as string;
        if (ownerIdentity is null)
            throw InvalidState("A Windows path owner is unavailable.");

        var getRules = security.GetType().GetMethod(
            "GetAccessRules",
            [typeof(bool), typeof(bool), typeof(Type)])!;
        var rules = (IEnumerable)getRules.Invoke(security, [true, true, sidType])!;
        var snapshots = new List<WindowsAclRuleSnapshot>();
        foreach (var rule in rules)
        {
            var ruleIdentity = ruleType.GetProperty("IdentityReference")!.GetValue(rule);
            var identity = ruleIdentity is null
                ? null
                : sidType.GetProperty("Value")!.GetValue(ruleIdentity) as string;
            var accessType = ruleType.GetProperty("AccessControlType")!.GetValue(rule)?.ToString();
            if (identity is null || accessType is null)
                throw InvalidState("A Windows access rule is invalid.");
            snapshots.Add(new WindowsAclRuleSnapshot(
                identity,
                accessType,
                Convert.ToInt64(ruleType.GetProperty("FileSystemRights")!.GetValue(rule)),
                Convert.ToInt64(ruleType.GetProperty("InheritanceFlags")!.GetValue(rule)),
                Convert.ToInt64(ruleType.GetProperty("PropagationFlags")!.GetValue(rule))));
        }
        return new WindowsAclSnapshot(ownerIdentity, snapshots);
    }

    private sealed record WindowsAclSnapshot(
        string OwnerIdentity,
        IReadOnlyList<WindowsAclRuleSnapshot> Rules);

    private readonly record struct WindowsAclRuleSnapshot(
        string Identity,
        string AccessType,
        long Rights,
        long InheritanceFlags,
        long PropagationFlags);

    private const int AtFileDescriptorCurrentWorkingDirectory = -100;
    private const int AtSymlinkNoFollow = 0x100;
    private const int AtEmptyPath = 0x1000;
    private const uint StatxBasicStats = 0x0000_07FF;
    private const uint StatxRequiredIdentity = 0x0000_010A;
    private const int UnixOpenReadOnly = 0;
    private const int UnixOpenDirectory = 0x0001_0000;
    private const int UnixOpenNoFollow = 0x0002_0000;
    private const int UnixOpenCloseOnExec = 0x0008_0000;
    private const uint WindowsFileReadAttributes = 0x0000_0080;
    private const uint WindowsFileShareRead = 0x0000_0001;
    private const uint WindowsFileShareWrite = 0x0000_0002;
    private const uint WindowsFileShareDelete = 0x0000_0004;
    private const uint WindowsOpenExisting = 3;
    private const uint WindowsFileFlagOpenReparsePoint = 0x0020_0000;
    private const uint WindowsFileFlagBackupSemantics = 0x0200_0000;

    [DllImport("libc", EntryPoint = "geteuid")]
    private static extern uint GetEffectiveUserId();

    [DllImport("libc", EntryPoint = "open", SetLastError = true, CharSet = CharSet.Ansi)]
    private static extern int OpenUnixDirectory(string path, int flags);

    [DllImport("libc", EntryPoint = "mkdir", SetLastError = true, CharSet = CharSet.Ansi)]
    private static extern int Mkdir(string path, uint mode);

    [DllImport("libc", EntryPoint = "statx", SetLastError = true, CharSet = CharSet.Ansi)]
    private static extern int Statx(
        int directoryFileDescriptor,
        string path,
        int flags,
        uint mask,
        out LinuxStatx status);

    [DllImport("kernel32.dll", EntryPoint = "CreateFileW", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern SafeFileHandle CreateFileWindows(
        string fileName,
        uint desiredAccess,
        uint shareMode,
        IntPtr securityAttributes,
        uint creationDisposition,
        uint flagsAndAttributes,
        IntPtr templateFile);

    [DllImport("kernel32.dll", EntryPoint = "CreateDirectoryW", SetLastError = true, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CreateDirectoryWindows(
        string path,
        IntPtr securityAttributes);

    [DllImport("kernel32.dll", EntryPoint = "GetFileInformationByHandle", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetFileInformationByHandle(
        SafeFileHandle handle,
        out WindowsFileInformation information);

    [StructLayout(LayoutKind.Explicit, Size = 256)]
    private struct LinuxStatx
    {
        [FieldOffset(0)]
        internal uint Mask;

        [FieldOffset(20)]
        internal uint UserId;

        [FieldOffset(28)]
        internal ushort Mode;

        [FieldOffset(32)]
        internal ulong Inode;

        [FieldOffset(136)]
        internal uint DeviceMajor;

        [FieldOffset(140)]
        internal uint DeviceMinor;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WindowsFileInformation
    {
        internal uint FileAttributes;
        internal WindowsFileTime CreationTime;
        internal WindowsFileTime LastAccessTime;
        internal WindowsFileTime LastWriteTime;
        internal uint VolumeSerialNumber;
        internal uint FileSizeHigh;
        internal uint FileSizeLow;
        internal uint NumberOfLinks;
        internal uint FileIndexHigh;
        internal uint FileIndexLow;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WindowsFileTime
    {
        internal uint LowDateTime;
        internal uint HighDateTime;
    }

    private static DynamicGraphCompositionPersistenceException InvalidState(
        string message,
        Exception? innerException = null) =>
        innerException is null
            ? new DynamicGraphCompositionPersistenceException(message)
            : new DynamicGraphCompositionPersistenceException(message, innerException);
}

public sealed class DynamicGraphCompositionPersistenceException : Exception
{
    public DynamicGraphCompositionPersistenceException(string message) : base(message) { }

    public DynamicGraphCompositionPersistenceException(string message, Exception innerException)
        : base(message, innerException) { }
}
