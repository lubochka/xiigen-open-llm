// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using SemanticCompiler29;

internal enum CanonicalSemanticCompilerCachePublicationDisposition
{
    NotCommitted,
    NotCommittedCleanupRequired,
    CommitOutcomeUnknown,
    CommittedCleanupRequired,
}

internal sealed class CanonicalSemanticCompilerCachePublicationException
    : IOException
{
    internal CanonicalSemanticCompilerCachePublicationException(
        CanonicalSemanticCompilerCachePublicationDisposition disposition,
        string message,
        Exception innerException)
        : base(message, innerException)
        => Disposition = disposition;

    internal CanonicalSemanticCompilerCachePublicationDisposition Disposition
    {
        get;
    }
}

internal enum CanonicalSemanticCompilerCacheStage
{
    AfterTempFileDurable,
    AfterCanonicalMovedToBackup,
    AfterBackupMoveDurable,
    AfterTempPromotedToCanonical,
    AfterCanonicalCommitDurable,
    AfterBackupDeleted,
    AfterBackupCleanupDurable,
}

internal sealed record CanonicalSemanticCompilerCacheOptions
{
    internal Action<CanonicalSemanticCompilerCacheStage>?
        ObserveStageForTesting
    {
        get;
        init;
    }
}

/// <summary>
/// Owner-locked, path-free canonical storage for supplied semantic-compiler
/// cache keys and their bounded pipeline outputs. Audit is not part of this
/// store or its publication transaction.
/// </summary>
internal sealed class CanonicalSemanticCompilerCache : ICompilerStorage, IDisposable
{
    internal const int HeaderLength = 64;
    internal const int MaximumFileBytes = 4 * 1024 * 1024;
    internal const int MaximumEntryCount = 4096;
    internal const int MaximumJsonDepth = 6;
    internal const ushort FormatVersion = 1;
    internal const int KeyLength = 64;

    private static readonly IReadOnlyList<string> PredicateVocabulary =
        Array.AsReadOnly<string>(
    [
        "NULL_CHECK",
        "RANGE_CHECK",
        "TYPE_CHECK",
        "ASSIGN_OP",
        "RETURN_OP",
        "LOOP_OP",
        "BRANCH_OP",
        "CALL_OP",
        "AWAIT_OP",
        "THROW_OP",
        "CATCH_OP",
        "VALIDATE_OP",
        "CREATE_OP",
        "UPDATE_OP",
        "DELETE_OP",
        "GET_OP",
        "CONVERT_OP",
        "SEND_OP",
        "LOG_OP",
        "AUTH_OP",
        "FILTER_OP",
        "AGGREGATE_OP",
    ]);

    private static readonly byte[] Magic = Encoding.ASCII.GetBytes("SC29CC01");
    private static readonly HashSet<string> StackLanguages =
        new(StringComparer.Ordinal)
        {
            "csharp",
            "typescript",
            "python",
            "unknown",
        };

    private readonly object _gate = new();
    private readonly ServerOwnedContinuousLearningCacheRootCapability _capability;
    private readonly CanonicalSemanticCompilerCacheOptions _options;
    private SortedDictionary<string, PipelineOutput> _entries =
        new(StringComparer.Ordinal);
    private ulong _generation;
    private byte[]? _committedImage;
    private ServerOwnedContinuousLearningCacheFileIdentity? _canonicalIdentity;
    private bool _sealed;
    private bool _disposed;

    private CanonicalSemanticCompilerCache(
        ServerOwnedContinuousLearningCacheRootCapability capability,
        CanonicalSemanticCompilerCacheOptions options)
    {
        _capability = capability;
        _options = options;
    }

    internal static IReadOnlyList<string> GetPredicateVocabularyForTesting()
        => PredicateVocabulary.ToArray();

    internal static CanonicalSemanticCompilerCache Open(
        ServerOwnedContinuousLearningCacheRootCapability capability,
        CanonicalSemanticCompilerCacheOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(capability);
        var cache = new CanonicalSemanticCompilerCache(
            capability,
            options ?? new CanonicalSemanticCompilerCacheOptions());
        try
        {
            lock (cache._gate)
                cache.RecoverAndLoadLocked();
            return cache;
        }
        catch
        {
            lock (cache._gate)
            {
                // Opening consumes only the child cache authority. If recovery
                // rejects the on-disk image, we close that child lock here but
                // leave the broader file-access owner under the caller's control.
                cache._sealed = true;
                cache._disposed = true;
                capability.Dispose();
            }
            throw;
        }
    }

    public PipelineOutput? Load(string key)
    {
        ValidateKey(key);
        lock (_gate)
        {
            ThrowIfUnavailableLocked();
            try
            {
                _ = _capability.Inventory(maximumEntryCount: 5);
            }
            catch (Exception exception)
                when (AgentInfrastructure.Logical.SnapshotExceptionGraphClassifier
                    .IsNonFatal(exception))
            {
                _sealed = true;
                throw;
            }
            return _entries.TryGetValue(key, out var output)
                ? CloneOutput(output)
                : null;
        }
    }

    public void Save(string key, PipelineOutput output)
    {
        ValidateKey(key);
        var ownedOutput = CloneAndValidateOutput(output);
        lock (_gate)
        {
            ThrowIfUnavailableLocked();
            EnsureDiskMatchesMemoryLocked();
            if (_entries.TryGetValue(key, out var existing))
            {
                if (OutputsEqual(existing, ownedOutput))
                    return;
                throw new InvalidOperationException(
                    "The supplied semantic-compiler cache key conflicts with its stored output.");
            }
            if (_entries.Count >= MaximumEntryCount)
                throw new InvalidOperationException("The semantic-compiler cache entry bound was exceeded.");

            var nextEntries = CloneEntries(_entries);
            nextEntries.Add(key, ownedOutput);
            var nextGeneration = checked(_generation + 1UL);
            var nextImage = SerializeImage(nextGeneration, nextEntries);
            PublishLocked(nextEntries, nextGeneration, nextImage);
        }
    }

    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed)
                return;
            _disposed = true;
            _sealed = true;
            _entries.Clear();
            _committedImage = null;
            _canonicalIdentity = null;
            // Normal cache shutdown releases the cache child lock. The parent
            // file-access owner may still protect other retained roots and is
            // disposed by the code that acquired it.
            _capability.Dispose();
        }
    }

    private void RecoverAndLoadLocked()
    {
        var inventory = InspectInventoryLocked();
        var canonical = inventory.Canonical;
        var temp = inventory.Temp;
        var backup = inventory.Backup;

        if (canonical is null && temp is null && backup is null)
        {
            AcceptEmptyLocked();
            return;
        }
        if (canonical is not null && temp is not null && backup is not null)
        {
            throw new InvalidDataException(
                "The semantic-compiler cache recovery namespace is ambiguous.");
        }

        if (canonical is not null && backup is not null)
        {
            var canonicalImage = ReadImageLocked(canonical.Value);
            var backupImage = ReadImageLocked(backup.Value);
            if (backupImage.Generation < 1UL ||
                canonicalImage.Generation != backupImage.Generation + 1UL ||
                !IsExactOneEntryExtension(backupImage.Entries, canonicalImage.Entries))
            {
                throw new InvalidDataException(
                    "The semantic-compiler cache publication relationship is invalid.");
            }

            _capability.FlushRootDirectory();
            _capability.DeleteExactOpenedIdentity(
                ServerOwnedContinuousLearningCacheLeaf.PublicationBackup,
                backup.Value.Identity,
                MaximumFileBytes);
            _capability.FlushRootDirectory();
            AcceptImageLocked(canonicalImage, canonical.Value.Identity);
            return;
        }

        if (canonical is not null)
        {
            var canonicalImage = ReadImageLocked(canonical.Value);
            if (temp is not null)
            {
                _capability.DeleteExactOpenedIdentity(
                    ServerOwnedContinuousLearningCacheLeaf.WriteTemp,
                    temp.Value.Identity,
                    ServerOwnedContinuousLearningCacheRootNative.MaximumOperationBytes);
            }
            _capability.FlushRootDirectory();
            AcceptImageLocked(canonicalImage, canonical.Value.Identity);
            return;
        }

        if (backup is not null)
        {
            var backupImage = ReadImageLocked(backup.Value);
            if (backupImage.Generation < 1UL)
                throw new InvalidDataException("The semantic-compiler cache backup generation is invalid.");
            _capability.RenameNoReplace(
                ServerOwnedContinuousLearningCacheLeaf.PublicationBackup,
                backup.Value.Identity,
                ServerOwnedContinuousLearningCacheLeaf.CanonicalCache,
                MaximumFileBytes);
            _capability.FlushRootDirectory();
            if (temp is not null)
            {
                _capability.DeleteExactOpenedIdentity(
                    ServerOwnedContinuousLearningCacheLeaf.WriteTemp,
                    temp.Value.Identity,
                    ServerOwnedContinuousLearningCacheRootNative.MaximumOperationBytes);
                _capability.FlushRootDirectory();
            }
            AcceptImageLocked(backupImage, backup.Value.Identity);
            return;
        }

        _capability.DeleteExactOpenedIdentity(
            ServerOwnedContinuousLearningCacheLeaf.WriteTemp,
            temp!.Value.Identity,
            ServerOwnedContinuousLearningCacheRootNative.MaximumOperationBytes);
        _capability.FlushRootDirectory();
        AcceptEmptyLocked();
    }

    private void EnsureDiskMatchesMemoryLocked()
    {
        NamespaceInventory inventory;
        try
        {
            inventory = InspectInventoryLocked();
            if (inventory.Temp is not null || inventory.Backup is not null)
                throw new InvalidDataException("The semantic-compiler cache namespace drifted.");
            if (_committedImage is null)
            {
                if (inventory.Canonical is not null || _canonicalIdentity is not null ||
                    _entries.Count != 0 || _generation != 0UL)
                {
                    throw new InvalidDataException("The semantic-compiler cache namespace drifted.");
                }
                return;
            }
            if (inventory.Canonical is null ||
                _canonicalIdentity is null ||
                inventory.Canonical.Value.Identity != _canonicalIdentity.Value)
            {
                throw new InvalidDataException("The semantic-compiler cache namespace drifted.");
            }
            var image = ReadImageLocked(inventory.Canonical.Value);
            if (!image.Bytes.AsSpan().SequenceEqual(_committedImage))
                throw new InvalidDataException("The semantic-compiler cache image drifted.");
        }
        catch (Exception exception)
            when (AgentInfrastructure.Logical.SnapshotExceptionGraphClassifier
                .IsNonFatal(exception))
        {
            _sealed = true;
            throw;
        }
    }

    private void PublishLocked(
        SortedDictionary<string, PipelineOutput> nextEntries,
        ulong nextGeneration,
        byte[] nextImage)
    {
        var promotionAttempted = false;
        var commitConfirmed = false;
        var priorIdentity = _canonicalIdentity;
        ServerOwnedContinuousLearningCacheFileIdentity? tempIdentity = null;
        try
        {
            tempIdentity = _capability.CreateWriteTemp(nextImage, MaximumFileBytes);
            Observe(CanonicalSemanticCompilerCacheStage.AfterTempFileDurable);
            var verifiedTemp = _capability.ReadBoundedExact(
                ServerOwnedContinuousLearningCacheLeaf.WriteTemp,
                tempIdentity.Value,
                MaximumFileBytes);
            if (!verifiedTemp.AsSpan().SequenceEqual(nextImage))
                throw new InvalidDataException("The semantic-compiler cache temp image changed.");

            if (priorIdentity is not null)
            {
                _capability.RenameNoReplace(
                    ServerOwnedContinuousLearningCacheLeaf.CanonicalCache,
                    priorIdentity.Value,
                    ServerOwnedContinuousLearningCacheLeaf.PublicationBackup,
                    MaximumFileBytes);
                Observe(CanonicalSemanticCompilerCacheStage.AfterCanonicalMovedToBackup);
                _capability.FlushRootDirectory();
                Observe(CanonicalSemanticCompilerCacheStage.AfterBackupMoveDurable);
            }

            promotionAttempted = true;
            _capability.RenameNoReplace(
                ServerOwnedContinuousLearningCacheLeaf.WriteTemp,
                tempIdentity.Value,
                ServerOwnedContinuousLearningCacheLeaf.CanonicalCache,
                MaximumFileBytes);
            Observe(CanonicalSemanticCompilerCacheStage.AfterTempPromotedToCanonical);
            _capability.FlushFile(
                ServerOwnedContinuousLearningCacheLeaf.CanonicalCache,
                tempIdentity.Value,
                MaximumFileBytes);
            _capability.FlushRootDirectory();
            commitConfirmed = true;

            _entries = nextEntries;
            _generation = nextGeneration;
            _committedImage = nextImage;
            _canonicalIdentity = tempIdentity.Value;

            if (priorIdentity is not null)
            {
                Observe(CanonicalSemanticCompilerCacheStage.AfterCanonicalCommitDurable);
                _capability.DeleteExactOpenedIdentity(
                    ServerOwnedContinuousLearningCacheLeaf.PublicationBackup,
                    priorIdentity.Value,
                    MaximumFileBytes);
                Observe(CanonicalSemanticCompilerCacheStage.AfterBackupDeleted);
                _capability.FlushRootDirectory();
            }
        }
        catch (Exception exception)
            when (AgentInfrastructure.Logical.SnapshotExceptionGraphClassifier
                .IsNonFatal(exception))
        {
            if (commitConfirmed)
            {
                _sealed = true;
                throw PublicationFailure(
                    CanonicalSemanticCompilerCachePublicationDisposition
                        .CommittedCleanupRequired,
                    "The semantic-compiler cache publication committed, but cleanup requires restart reconciliation.",
                    exception);
            }
            if (promotionAttempted)
            {
                _sealed = true;
                throw PublicationFailure(
                    CanonicalSemanticCompilerCachePublicationDisposition
                        .CommitOutcomeUnknown,
                    "The semantic-compiler cache publication outcome requires restart reconciliation.",
                    exception);
            }

            try
            {
                RollBackBeforePromotionLocked();
            }
            catch (Exception cleanupException)
                when (AgentInfrastructure.Logical.SnapshotExceptionGraphClassifier
                    .IsNonFatal(cleanupException))
            {
                _sealed = true;
                throw PublicationFailure(
                    CanonicalSemanticCompilerCachePublicationDisposition
                        .NotCommittedCleanupRequired,
                    "The semantic-compiler cache was not promoted, but cleanup requires restart reconciliation.",
                    new AggregateException(exception, cleanupException));
            }

            throw PublicationFailure(
                CanonicalSemanticCompilerCachePublicationDisposition.NotCommitted,
                "The semantic-compiler cache publication was not committed.",
                exception);
        }

        Observe(
            priorIdentity is null
                ? CanonicalSemanticCompilerCacheStage.AfterCanonicalCommitDurable
                : CanonicalSemanticCompilerCacheStage.AfterBackupCleanupDurable);
    }

    private void RollBackBeforePromotionLocked()
    {
        var inventory = InspectInventoryLocked();
        if (inventory.Canonical is not null && inventory.Backup is not null)
            throw new InvalidDataException("The pre-publication cleanup namespace is ambiguous.");

        if (_committedImage is null)
        {
            if (inventory.Canonical is not null || inventory.Backup is not null)
                throw new InvalidDataException("The pre-publication cleanup namespace drifted.");
            if (inventory.Temp is not null)
            {
                _capability.DeleteExactOpenedIdentity(
                    ServerOwnedContinuousLearningCacheLeaf.WriteTemp,
                    inventory.Temp.Value.Identity,
                    ServerOwnedContinuousLearningCacheRootNative.MaximumOperationBytes);
                _capability.FlushRootDirectory();
            }
            return;
        }

        if (inventory.Canonical is not null)
        {
            var canonical = ReadImageLocked(inventory.Canonical.Value);
            if (!canonical.Bytes.AsSpan().SequenceEqual(_committedImage))
                throw new InvalidDataException("The pre-publication canonical image drifted.");
        }
        else if (inventory.Backup is not null)
        {
            var backup = ReadImageLocked(inventory.Backup.Value);
            if (!backup.Bytes.AsSpan().SequenceEqual(_committedImage))
                throw new InvalidDataException("The pre-publication backup image drifted.");
            _capability.RenameNoReplace(
                ServerOwnedContinuousLearningCacheLeaf.PublicationBackup,
                inventory.Backup.Value.Identity,
                ServerOwnedContinuousLearningCacheLeaf.CanonicalCache,
                MaximumFileBytes);
            _capability.FlushRootDirectory();
        }
        else
        {
            throw new InvalidDataException("The prior semantic-compiler cache image disappeared.");
        }

        if (inventory.Temp is not null)
        {
            _capability.DeleteExactOpenedIdentity(
                ServerOwnedContinuousLearningCacheLeaf.WriteTemp,
                inventory.Temp.Value.Identity,
                ServerOwnedContinuousLearningCacheRootNative.MaximumOperationBytes);
            _capability.FlushRootDirectory();
        }
    }

    private NamespaceInventory InspectInventoryLocked()
    {
        var entries = _capability.Inventory(maximumEntryCount: 5);
        ServerOwnedContinuousLearningCacheInventoryEntry? canonical = null;
        ServerOwnedContinuousLearningCacheInventoryEntry? temp = null;
        ServerOwnedContinuousLearningCacheInventoryEntry? backup = null;
        foreach (var entry in entries)
        {
            switch (entry.Leaf)
            {
                case ServerOwnedContinuousLearningCacheLeaf.CanonicalCache:
                    canonical = SetOnce(canonical, entry);
                    break;
                case ServerOwnedContinuousLearningCacheLeaf.WriteTemp:
                    temp = SetOnce(temp, entry);
                    break;
                case ServerOwnedContinuousLearningCacheLeaf.PublicationBackup:
                    backup = SetOnce(backup, entry);
                    break;
            }
        }
        return new NamespaceInventory(canonical, temp, backup);
    }

    private ParsedImage ReadImageLocked(
        ServerOwnedContinuousLearningCacheInventoryEntry entry)
    {
        if (entry.Leaf is not ServerOwnedContinuousLearningCacheLeaf.CanonicalCache and
            not ServerOwnedContinuousLearningCacheLeaf.PublicationBackup ||
            entry.Length is <= HeaderLength or > MaximumFileBytes)
        {
            throw new InvalidDataException("The semantic-compiler cache image bound is invalid.");
        }
        var bytes = _capability.ReadBoundedExact(
            entry.Leaf,
            entry.Identity,
            MaximumFileBytes);
        if (bytes.LongLength != entry.Length)
            throw new InvalidDataException("The semantic-compiler cache image length changed.");
        return ParseImage(bytes);
    }

    private void AcceptEmptyLocked()
    {
        _entries = new SortedDictionary<string, PipelineOutput>(StringComparer.Ordinal);
        _generation = 0UL;
        _committedImage = null;
        _canonicalIdentity = null;
    }

    private void AcceptImageLocked(
        ParsedImage image,
        ServerOwnedContinuousLearningCacheFileIdentity identity)
    {
        _entries = CloneEntries(image.Entries);
        _generation = image.Generation;
        _committedImage = image.Bytes;
        _canonicalIdentity = identity;
    }

    private void Observe(CanonicalSemanticCompilerCacheStage stage)
        => _options.ObserveStageForTesting?.Invoke(stage);

    private void ThrowIfUnavailableLocked()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(CanonicalSemanticCompilerCache));
        if (_sealed)
            throw new InvalidOperationException("The semantic-compiler cache is sealed.");
    }

    private static CanonicalSemanticCompilerCachePublicationException PublicationFailure(
        CanonicalSemanticCompilerCachePublicationDisposition disposition,
        string message,
        Exception innerException)
        => new(disposition, message, innerException);

    private static ServerOwnedContinuousLearningCacheInventoryEntry SetOnce(
        ServerOwnedContinuousLearningCacheInventoryEntry? current,
        ServerOwnedContinuousLearningCacheInventoryEntry next)
        => current is null
            ? next
            : throw new InvalidDataException("The fixed cache inventory contains a duplicate leaf.");

    private static bool IsExactOneEntryExtension(
        SortedDictionary<string, PipelineOutput> prior,
        SortedDictionary<string, PipelineOutput> candidate)
    {
        if (candidate.Count != prior.Count + 1)
            return false;
        var newEntries = 0;
        foreach (var pair in candidate)
        {
            if (!prior.TryGetValue(pair.Key, out var oldOutput))
            {
                newEntries++;
                continue;
            }
            if (!OutputsEqual(oldOutput, pair.Value))
                return false;
        }
        return newEntries == 1;
    }

    private static byte[] SerializeImage(
        ulong generation,
        SortedDictionary<string, PipelineOutput> entries)
    {
        if (generation != checked((ulong)entries.Count) ||
            entries.Count is < 1 or > MaximumEntryCount)
        {
            throw new InvalidDataException("The semantic-compiler cache generation is invalid.");
        }

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(
                   stream,
                   new JsonWriterOptions { Indented = false, SkipValidation = false }))
        {
            writer.WriteStartObject();
            writer.WriteNumber("schemaVersion", FormatVersion);
            writer.WriteNumber("generation", generation);
            writer.WritePropertyName("entries");
            writer.WriteStartArray();
            foreach (var pair in entries)
            {
                writer.WriteStartObject();
                writer.WriteString("key", pair.Key);
                writer.WritePropertyName("output");
                WriteOutput(writer, pair.Value);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        var payload = stream.ToArray();
        var totalLength = checked(HeaderLength + payload.Length);
        if (totalLength > MaximumFileBytes)
            throw new InvalidOperationException("The semantic-compiler cache file bound was exceeded.");
        var bytes = new byte[totalLength];
        Magic.CopyTo(bytes, 0);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(8, 2), FormatVersion);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(10, 2), HeaderLength);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(12, 4), checked((uint)payload.Length));
        BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(16, 8), generation);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(24, 4), checked((uint)entries.Count));
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(28, 4), 0U);
        SHA256.HashData(payload).CopyTo(bytes, 32);
        payload.CopyTo(bytes, HeaderLength);
        return bytes;
    }

    private static ParsedImage ParseImage(byte[] bytes)
    {
        if (bytes.Length is <= HeaderLength or > MaximumFileBytes ||
            !bytes.AsSpan(0, Magic.Length).SequenceEqual(Magic) ||
            BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(8, 2)) != FormatVersion ||
            BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(10, 2)) != HeaderLength ||
            BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(28, 4)) != 0U)
        {
            throw new InvalidDataException("The semantic-compiler cache header is invalid.");
        }
        var payloadLength = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(12, 4));
        if (payloadLength > MaximumFileBytes - HeaderLength ||
            bytes.Length != HeaderLength + checked((int)payloadLength))
        {
            throw new InvalidDataException("The semantic-compiler cache payload length is invalid.");
        }
        var headerGeneration = BinaryPrimitives.ReadUInt64LittleEndian(bytes.AsSpan(16, 8));
        var headerCount = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(24, 4));
        var payload = bytes.AsSpan(HeaderLength, checked((int)payloadLength));
        Span<byte> digest = stackalloc byte[32];
        SHA256.HashData(payload, digest);
        if (!CryptographicOperations.FixedTimeEquals(digest, bytes.AsSpan(32, 32)))
            throw new InvalidDataException("The semantic-compiler cache digest is invalid.");

        try
        {
            using var document = JsonDocument.Parse(
                bytes.AsMemory(HeaderLength, checked((int)payloadLength)),
                new JsonDocumentOptions
                {
                    AllowTrailingCommas = false,
                    CommentHandling = JsonCommentHandling.Disallow,
                    MaxDepth = MaximumJsonDepth,
                });
            var root = document.RootElement;
            RequireObjectProperties(root, "schemaVersion", "generation", "entries");
            if (root.GetProperty("schemaVersion").GetUInt16() != FormatVersion)
                throw new InvalidDataException("The semantic-compiler cache schema version is invalid.");
            var generation = root.GetProperty("generation").GetUInt64();
            var entriesElement = root.GetProperty("entries");
            if (entriesElement.ValueKind != JsonValueKind.Array)
                throw new InvalidDataException("The semantic-compiler cache entries are invalid.");

            var entries = new SortedDictionary<string, PipelineOutput>(StringComparer.Ordinal);
            string? priorKey = null;
            foreach (var entry in entriesElement.EnumerateArray())
            {
                RequireObjectProperties(entry, "key", "output");
                var key = entry.GetProperty("key").GetString()
                    ?? throw new InvalidDataException("The semantic-compiler cache key is invalid.");
                ValidateKey(key);
                if (priorKey is not null && StringComparer.Ordinal.Compare(priorKey, key) >= 0)
                    throw new InvalidDataException("The semantic-compiler cache keys are not canonical.");
                priorKey = key;
                var output = ParseOutput(entry.GetProperty("output"));
                if (!entries.TryAdd(key, output))
                    throw new InvalidDataException("The semantic-compiler cache contains a duplicate key.");
                if (entries.Count > MaximumEntryCount)
                    throw new InvalidDataException("The semantic-compiler cache entry bound was exceeded.");
            }

            if (entries.Count is < 1 or > MaximumEntryCount ||
                generation != checked((ulong)entries.Count) ||
                generation != headerGeneration ||
                headerCount != checked((uint)entries.Count))
            {
                throw new InvalidDataException("The semantic-compiler cache generation or count is invalid.");
            }

            var canonical = SerializeImage(generation, entries);
            if (!canonical.AsSpan(HeaderLength).SequenceEqual(payload))
                throw new InvalidDataException("The semantic-compiler cache JSON is not canonical.");
            return new ParsedImage(generation, entries, bytes);
        }
        catch (InvalidDataException)
        {
            throw;
        }
        catch (Exception exception)
            when (exception is JsonException or InvalidOperationException or
                  FormatException or OverflowException or ArgumentException)
        {
            throw new InvalidDataException("The semantic-compiler cache payload is invalid.", exception);
        }
    }

    private static PipelineOutput ParseOutput(JsonElement element)
    {
        RequireObjectProperties(
            element,
            "domain",
            "predicates",
            "stackLanguage",
            "compressionRatio");
        var domainText = element.GetProperty("domain").GetString()
            ?? throw new InvalidDataException("The semantic-compiler cache domain is invalid.");
        if (!Enum.TryParse<Domain>(domainText, ignoreCase: false, out var domain) ||
            !Enum.IsDefined(domain) ||
            !string.Equals(Enum.GetName(domain), domainText, StringComparison.Ordinal))
        {
            throw new InvalidDataException("The semantic-compiler cache domain is invalid.");
        }
        var predicatesElement = element.GetProperty("predicates");
        if (predicatesElement.ValueKind != JsonValueKind.Array)
            throw new InvalidDataException("The semantic-compiler cache predicates are invalid.");
        var predicates = predicatesElement.EnumerateArray()
            .Select(item => item.GetString()
                ?? throw new InvalidDataException("The semantic-compiler cache predicate is invalid."))
            .ToArray();
        var stackLanguage = element.GetProperty("stackLanguage").GetString()
            ?? throw new InvalidDataException("The semantic-compiler cache stack is invalid.");
        var compressionRatio = element.GetProperty("compressionRatio").GetDouble();
        return CloneAndValidateOutput(
            new PipelineOutput(domain, predicates, stackLanguage, compressionRatio));
    }

    private static void WriteOutput(Utf8JsonWriter writer, PipelineOutput output)
    {
        writer.WriteStartObject();
        writer.WriteString("domain", output.Domain.ToString());
        writer.WritePropertyName("predicates");
        writer.WriteStartArray();
        foreach (var predicate in output.Predicates)
            writer.WriteStringValue(predicate);
        writer.WriteEndArray();
        writer.WriteString("stackLanguage", output.StackLanguage);
        writer.WriteNumber("compressionRatio", output.CompressionRatio);
        writer.WriteEndObject();
    }

    private static void RequireObjectProperties(JsonElement element, params string[] expected)
    {
        if (element.ValueKind != JsonValueKind.Object)
            throw new InvalidDataException("The semantic-compiler cache JSON object is invalid.");
        var actual = element.EnumerateObject().Select(property => property.Name).ToArray();
        if (!actual.SequenceEqual(expected, StringComparer.Ordinal))
            throw new InvalidDataException("The semantic-compiler cache JSON property order is invalid.");
    }

    private static void ValidateKey(string? key)
    {
        if (key is null || key.Length != KeyLength ||
            key.Any(character => character is not (>= '0' and <= '9') and
                                             not (>= 'a' and <= 'f')))
        {
            throw new ArgumentException(
                "A canonical supplied semantic-compiler cache key is required.",
                nameof(key));
        }
    }

    private static PipelineOutput CloneAndValidateOutput(PipelineOutput? output)
    {
        ArgumentNullException.ThrowIfNull(output);
        var predicates = output.Predicates?.ToArray()
            ?? throw new ArgumentException("Canonical semantic-compiler predicates are required.");
        if (!Enum.IsDefined(output.Domain) ||
            !StackLanguages.Contains(output.StackLanguage) ||
            !double.IsFinite(output.CompressionRatio) ||
            output.CompressionRatio <= 0D ||
            output.CompressionRatio > int.MaxValue ||
            predicates.Length is < 1 or > 22)
        {
            throw new ArgumentException("A canonical semantic-compiler output is required.");
        }
        var priorOrdinal = -1;
        foreach (var predicate in predicates)
        {
            var ordinal = PredicateOrdinal(predicate);
            if (ordinal <= priorOrdinal)
                throw new ArgumentException("Canonical semantic-compiler predicates are required.");
            priorOrdinal = ordinal;
        }
        return new PipelineOutput(
            output.Domain,
            predicates,
            output.StackLanguage,
            output.CompressionRatio);
    }

    private static int PredicateOrdinal(string predicate)
    {
        for (var index = 0; index < PredicateVocabulary.Count; index++)
        {
            if (string.Equals(PredicateVocabulary[index], predicate, StringComparison.Ordinal))
                return index;
        }
        return -1;
    }

    private static PipelineOutput CloneOutput(PipelineOutput output)
        => new(
            output.Domain,
            output.Predicates.ToArray(),
            output.StackLanguage,
            output.CompressionRatio);

    private static SortedDictionary<string, PipelineOutput> CloneEntries(
        IEnumerable<KeyValuePair<string, PipelineOutput>> entries)
    {
        var clone = new SortedDictionary<string, PipelineOutput>(StringComparer.Ordinal);
        foreach (var pair in entries)
            clone.Add(pair.Key, CloneOutput(pair.Value));
        return clone;
    }

    private static bool OutputsEqual(PipelineOutput left, PipelineOutput right)
        => left.Domain == right.Domain &&
           string.Equals(left.StackLanguage, right.StackLanguage, StringComparison.Ordinal) &&
           left.CompressionRatio.Equals(right.CompressionRatio) &&
           left.Predicates.SequenceEqual(right.Predicates, StringComparer.Ordinal);

    private readonly record struct NamespaceInventory(
        ServerOwnedContinuousLearningCacheInventoryEntry? Canonical,
        ServerOwnedContinuousLearningCacheInventoryEntry? Temp,
        ServerOwnedContinuousLearningCacheInventoryEntry? Backup);

    private sealed record ParsedImage(
        ulong Generation,
        SortedDictionary<string, PipelineOutput> Entries,
        byte[] Bytes);
}
