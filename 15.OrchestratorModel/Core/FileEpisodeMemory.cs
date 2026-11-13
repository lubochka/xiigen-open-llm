// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace OrchestratorModel;

using System.Security;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

// Durable, single-instance-coordinated episode history. Multiple instances and
// multiple processes require coordination by their caller.
public sealed class FileEpisodeMemory : BaseService, IEpisodeMemory
{
    private const int SchemaVersion = 1;

    private const string TopKInvalidCode = "EPISODE_MEMORY_TOP_K_INVALID";
    private const string QueryInvalidCode = "EPISODE_MEMORY_QUERY_INVALID";
    private const string DecompositionInvalidCode = "EPISODE_MEMORY_DECOMPOSITION_INVALID";
    private const string CorruptCode = "EPISODE_MEMORY_CORRUPT";
    private const string SchemaInvalidCode = "EPISODE_MEMORY_SCHEMA_INVALID";
    private const string ConfigurationMismatchCode = "EPISODE_MEMORY_CONFIGURATION_MISMATCH";
    private const string PathInvalidCode = "EPISODE_MEMORY_PATH_INVALID";
    private const string ReadFailedCode = "EPISODE_MEMORY_READ_FAILED";
    private const string WriteFailedCode = "EPISODE_MEMORY_WRITE_FAILED";
    private const string SequenceExhaustedCode = "EPISODE_MEMORY_SEQUENCE_EXHAUSTED";

    private const string TopKInvalidMessage = "Episode memory topK cannot be negative.";
    private const string QueryInvalidMessage = "Episode memory recall query cannot be null.";
    private const string DecompositionInvalidMessage = "Episode memory decomposition is invalid.";
    private const string CorruptMessage = "Episode memory snapshot JSON is corrupt.";
    private const string SchemaInvalidMessage = "Episode memory snapshot schema is invalid.";
    private const string ConfigurationMismatchMessage =
        "Episode memory snapshot capacity does not match the configured capacity.";
    private const string PathInvalidMessage = "Episode memory snapshot path is invalid.";
    private const string ReadFailedMessage = "Episode memory snapshot could not be read.";
    private const string WriteFailedMessage = "Episode memory snapshot could not be written.";
    private const string SequenceExhaustedMessage = "Episode memory sequence is exhausted.";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true
    };

    private readonly object _gate = new();
    private readonly string? _rawSnapshotPath;
    private readonly int _maxEntries;
    private readonly IFileEpisodeMemoryFileSystem _fileSystem;

    private string? _canonicalPath;
    private bool _hydrated;
    private List<StoredEntry> _history = [];
    private long _nextSequence;

    public FileEpisodeMemory(
        ISessionStorage storage,
        string snapshotPath,
        int maxEntries = 1000)
        : this(storage, snapshotPath, maxEntries, RealFileEpisodeMemoryFileSystem.Instance)
    {
    }

    internal FileEpisodeMemory(
        ISessionStorage storage,
        string snapshotPath,
        int maxEntries,
        IFileEpisodeMemoryFileSystem fileSystem)
        : base(storage)
    {
        if (maxEntries <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxEntries));

        _rawSnapshotPath = snapshotPath;
        _maxEntries = maxEntries;
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public OperationResult<bool> Store(
        string taskDescription,
        IReadOnlyList<AgentAlgorithms.SubTask> decomposition)
    {
        lock (_gate)
        {
            if (string.IsNullOrWhiteSpace(taskDescription))
                return OperationResult<bool>.Fail("Task description cannot be empty.", "EMPTY_TASK");

            var cloneResult = CloneAndValidateDecomposition(decomposition);
            if (!cloneResult.IsSuccess)
                return cloneResult.MapError<bool>();

            var hydrateResult = EnsureHydrated();
            if (!hydrateResult.IsSuccess)
                return hydrateResult.MapError<bool>();

            if (_nextSequence == long.MaxValue)
            {
                return OperationResult<bool>.Fail(
                    SequenceExhaustedMessage,
                    SequenceExhaustedCode);
            }

            var candidateHistory = _history
                .Select(CloneStoredEntry)
                .ToList();
            candidateHistory.Add(new StoredEntry(
                _nextSequence,
                taskDescription,
                cloneResult.Value!));
            if (candidateHistory.Count > _maxEntries)
            {
                var smallestSequence = candidateHistory.Min(entry => entry.Sequence);
                candidateHistory.Remove(
                    candidateHistory.First(entry => entry.Sequence == smallestSequence));
            }

            var candidateNextSequence = _nextSequence + 1;
            var snapshot = new SnapshotDocument
            {
                SchemaVersion = SchemaVersion,
                MaxEntries = _maxEntries,
                NextSequence = candidateNextSequence,
                Entries = candidateHistory
                    .OrderBy(entry => entry.Sequence)
                    .Select(ToSnapshotEntry)
                    .ToArray()
            };
            byte[] bytes;
            try
            {
                bytes = JsonSerializer.SerializeToUtf8Bytes(snapshot, JsonOptions);
            }
            catch (JsonException exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return OperationResult<bool>.Fail(WriteFailedMessage, WriteFailedCode);
            }
            catch (NotSupportedException exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return OperationResult<bool>.Fail(WriteFailedMessage, WriteFailedCode);
            }

            var canonicalPath = _canonicalPath!;
            var parentDirectory = Path.GetDirectoryName(canonicalPath)!;
            string? temporaryPath = null;

            try
            {
                _fileSystem.CreateDirectory(parentDirectory);
                var temporary = _fileSystem.CreateUniqueWriteThroughTemporaryFile(canonicalPath);
                temporaryPath = temporary.Path;
                using (temporary.Stream)
                {
                    temporary.Stream.Write(bytes, 0, bytes.Length);
                    temporary.Stream.Flush(flushToDisk: true);
                }

                _fileSystem.MoveOverwrite(temporaryPath, canonicalPath);

                _history = candidateHistory;
                _nextSequence = candidateNextSequence;
                return OperationResult<bool>.Ok(true);
            }
            catch (IOException exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return OperationResult<bool>.Fail(WriteFailedMessage, WriteFailedCode);
            }
            catch (UnauthorizedAccessException exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return OperationResult<bool>.Fail(WriteFailedMessage, WriteFailedCode);
            }
            catch (SecurityException exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return OperationResult<bool>.Fail(WriteFailedMessage, WriteFailedCode);
            }
            finally
            {
                if (temporaryPath is not null)
                    CleanupTemporaryFile(temporaryPath);
            }
        }
    }

    public OperationResult<IReadOnlyList<string>> Recall(
        string taskDescription,
        int topK = 3)
    {
        lock (_gate)
        {
            if (topK < 0)
            {
                return OperationResult<IReadOnlyList<string>>.Fail(
                    TopKInvalidMessage,
                    TopKInvalidCode);
            }

            if (taskDescription is null)
            {
                return OperationResult<IReadOnlyList<string>>.Fail(
                    QueryInvalidMessage,
                    QueryInvalidCode);
            }

            var hydrateResult = EnsureHydrated();
            if (!hydrateResult.IsSuccess)
                return hydrateResult.MapError<IReadOnlyList<string>>();

            if (topK == 0)
            {
                return OperationResult<IReadOnlyList<string>>.Ok(
                    Array.Empty<string>());
            }

            var matches = _history
                .Where(entry => entry.TaskDescription.Contains(
                    taskDescription,
                    StringComparison.OrdinalIgnoreCase))
                .TakeLast(topK)
                .Select(entry => entry.TaskDescription)
                .ToArray();
            return OperationResult<IReadOnlyList<string>>.Ok(matches);
        }
    }

    private OperationResult<bool> EnsureHydrated()
    {
        if (_hydrated)
            return OperationResult<bool>.Ok(true);

        var pathResult = ResolveCanonicalPath();
        if (!pathResult.IsSuccess)
            return pathResult.MapError<bool>();

        byte[]? bytes;
        try
        {
            bytes = _fileSystem.ReadCanonicalOrMissing(_canonicalPath!);
        }
        catch (IOException exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<bool>.Fail(ReadFailedMessage, ReadFailedCode);
        }
        catch (UnauthorizedAccessException exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<bool>.Fail(ReadFailedMessage, ReadFailedCode);
        }
        catch (SecurityException exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<bool>.Fail(ReadFailedMessage, ReadFailedCode);
        }

        if (bytes is null)
        {
            _history = [];
            _nextSequence = 0;
            _hydrated = true;
            return OperationResult<bool>.Ok(true);
        }

        JsonDocument parsed;
        try
        {
            parsed = JsonDocument.Parse(bytes);
        }
        catch (JsonException exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<bool>.Fail(CorruptMessage, CorruptCode);
        }

        SnapshotDocument? snapshot;
        using (parsed)
        {
            if (!HasExactJsonShape(parsed.RootElement))
                return OperationResult<bool>.Fail(SchemaInvalidMessage, SchemaInvalidCode);

            try
            {
                snapshot = parsed.RootElement.Deserialize<SnapshotDocument>(JsonOptions);
            }
            catch (JsonException exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return OperationResult<bool>.Fail(SchemaInvalidMessage, SchemaInvalidCode);
            }
        }

        var validationResult = ValidateSnapshot(snapshot);
        if (!validationResult.IsSuccess)
            return validationResult.MapError<bool>();

        _history = validationResult.Value!.History;
        _nextSequence = validationResult.Value.NextSequence;
        _hydrated = true;
        return OperationResult<bool>.Ok(true);
    }

    private OperationResult<string> ResolveCanonicalPath()
    {
        if (_canonicalPath is not null)
            return OperationResult<string>.Ok(_canonicalPath);

        if (string.IsNullOrWhiteSpace(_rawSnapshotPath))
            return OperationResult<string>.Fail(PathInvalidMessage, PathInvalidCode);

        string canonicalPath;
        try
        {
            canonicalPath = Path.GetFullPath(_rawSnapshotPath);
        }
        catch (ArgumentException exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<string>.Fail(PathInvalidMessage, PathInvalidCode);
        }
        catch (NotSupportedException exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<string>.Fail(PathInvalidMessage, PathInvalidCode);
        }
        catch (PathTooLongException exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<string>.Fail(PathInvalidMessage, PathInvalidCode);
        }
        catch (SecurityException exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<string>.Fail(PathInvalidMessage, PathInvalidCode);
        }

        if (string.IsNullOrWhiteSpace(Path.GetDirectoryName(canonicalPath)))
            return OperationResult<string>.Fail(PathInvalidMessage, PathInvalidCode);

        _canonicalPath = canonicalPath;
        return OperationResult<string>.Ok(canonicalPath);
    }

    private OperationResult<HydratedState> ValidateSnapshot(SnapshotDocument? snapshot)
    {
        if (snapshot is null
            || snapshot.SchemaVersion != SchemaVersion
            || snapshot.MaxEntries <= 0
            || snapshot.Entries is null
            || snapshot.Entries.Length > snapshot.MaxEntries)
        {
            return OperationResult<HydratedState>.Fail(
                SchemaInvalidMessage,
                SchemaInvalidCode);
        }

        if (snapshot.MaxEntries != _maxEntries)
        {
            return OperationResult<HydratedState>.Fail(
                ConfigurationMismatchMessage,
                ConfigurationMismatchCode);
        }

        if (snapshot.Entries.Length == 0)
        {
            if (snapshot.NextSequence != 0)
            {
                return OperationResult<HydratedState>.Fail(
                    SchemaInvalidMessage,
                    SchemaInvalidCode);
            }

            return OperationResult<HydratedState>.Ok(new HydratedState([], 0));
        }

        var history = new List<StoredEntry>(snapshot.Entries.Length);
        long? previousSequence = null;
        foreach (var entry in snapshot.Entries)
        {
            if (entry is null
                || entry.Sequence < 0
                || string.IsNullOrWhiteSpace(entry.TaskDescription)
                || entry.Decomposition is null)
            {
                return OperationResult<HydratedState>.Fail(
                    SchemaInvalidMessage,
                    SchemaInvalidCode);
            }

            if (previousSequence.HasValue
                && (previousSequence.Value == long.MaxValue
                    || entry.Sequence != previousSequence.Value + 1))
            {
                return OperationResult<HydratedState>.Fail(
                    SchemaInvalidMessage,
                    SchemaInvalidCode);
            }

            var decomposition = new AgentAlgorithms.SubTask[entry.Decomposition.Length];
            for (var index = 0; index < entry.Decomposition.Length; index++)
            {
                var subTask = entry.Decomposition[index];
                if (subTask is null
                    || string.IsNullOrWhiteSpace(subTask.Description)
                    || string.IsNullOrWhiteSpace(subTask.RequiredRole))
                {
                    return OperationResult<HydratedState>.Fail(
                        SchemaInvalidMessage,
                        SchemaInvalidCode);
                }

                decomposition[index] = new AgentAlgorithms.SubTask(
                    subTask.Description,
                    subTask.RequiredRole);
            }

            history.Add(new StoredEntry(
                entry.Sequence,
                entry.TaskDescription,
                decomposition));
            previousSequence = entry.Sequence;
        }

        var lastSequence = previousSequence!.Value;
        if (lastSequence == long.MaxValue
            || snapshot.NextSequence != lastSequence + 1)
        {
            return OperationResult<HydratedState>.Fail(
                SchemaInvalidMessage,
                SchemaInvalidCode);
        }

        return OperationResult<HydratedState>.Ok(
            new HydratedState(history, snapshot.NextSequence));
    }

    private static bool HasExactJsonShape(JsonElement root)
    {
        if (!HasExactUniqueProperties(
                root,
                "schemaVersion",
                "maxEntries",
                "nextSequence",
                "entries")
            || root.GetProperty("schemaVersion").ValueKind != JsonValueKind.Number
            || root.GetProperty("maxEntries").ValueKind != JsonValueKind.Number
            || root.GetProperty("nextSequence").ValueKind != JsonValueKind.Number
            || root.GetProperty("entries").ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        foreach (var entry in root.GetProperty("entries").EnumerateArray())
        {
            if (!HasExactUniqueProperties(
                    entry,
                    "sequence",
                    "taskDescription",
                    "decomposition")
                || entry.GetProperty("sequence").ValueKind != JsonValueKind.Number
                || entry.GetProperty("taskDescription").ValueKind != JsonValueKind.String
                || entry.GetProperty("decomposition").ValueKind != JsonValueKind.Array)
            {
                return false;
            }

            foreach (var subTask in entry.GetProperty("decomposition").EnumerateArray())
            {
                if (!HasExactUniqueProperties(subTask, "description", "requiredRole")
                    || subTask.GetProperty("description").ValueKind != JsonValueKind.String
                    || subTask.GetProperty("requiredRole").ValueKind != JsonValueKind.String)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static bool HasExactUniqueProperties(
        JsonElement element,
        params string[] expectedProperties)
    {
        if (element.ValueKind != JsonValueKind.Object)
            return false;

        var expected = new HashSet<string>(
            expectedProperties,
            StringComparer.Ordinal);
        var observed = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in element.EnumerateObject())
        {
            if (!expected.Contains(property.Name)
                || !observed.Add(property.Name))
            {
                return false;
            }
        }

        return observed.SetEquals(expected);
    }

    private static OperationResult<AgentAlgorithms.SubTask[]> CloneAndValidateDecomposition(
        IReadOnlyList<AgentAlgorithms.SubTask>? decomposition)
    {
        if (decomposition is null)
        {
            return OperationResult<AgentAlgorithms.SubTask[]>.Fail(
                DecompositionInvalidMessage,
                DecompositionInvalidCode);
        }

        var clone = new AgentAlgorithms.SubTask[decomposition.Count];
        for (var index = 0; index < decomposition.Count; index++)
        {
            var subTask = decomposition[index];
            if (subTask is null
                || string.IsNullOrWhiteSpace(subTask.Description)
                || string.IsNullOrWhiteSpace(subTask.RequiredRole))
            {
                return OperationResult<AgentAlgorithms.SubTask[]>.Fail(
                    DecompositionInvalidMessage,
                    DecompositionInvalidCode);
            }

            clone[index] = new AgentAlgorithms.SubTask(
                subTask.Description,
                subTask.RequiredRole);
        }

        return OperationResult<AgentAlgorithms.SubTask[]>.Ok(clone);
    }

    private void CleanupTemporaryFile(string temporaryPath)
    {
        try
        {
            _fileSystem.DeleteTemporaryFileIfExists(temporaryPath);
        }
        catch (IOException exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            // Cleanup is best-effort and never masks the primary result.
        }
        catch (UnauthorizedAccessException exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            // Cleanup is best-effort and never masks the primary result.
        }
        catch (SecurityException exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            // Cleanup is best-effort and never masks the primary result.
        }
    }

    private static StoredEntry CloneStoredEntry(StoredEntry entry)
        => new(
            entry.Sequence,
            entry.TaskDescription,
            entry.Decomposition
                .Select(subTask => new AgentAlgorithms.SubTask(
                    subTask.Description,
                    subTask.RequiredRole))
                .ToArray());

    private static SnapshotEntry ToSnapshotEntry(StoredEntry entry)
        => new()
        {
            Sequence = entry.Sequence,
            TaskDescription = entry.TaskDescription,
            Decomposition = entry.Decomposition
                .Select(subTask => new SnapshotSubTask
                {
                    Description = subTask.Description,
                    RequiredRole = subTask.RequiredRole
                })
                .ToArray()
        };

    private sealed record StoredEntry(
        long Sequence,
        string TaskDescription,
        AgentAlgorithms.SubTask[] Decomposition);

    private sealed record HydratedState(
        List<StoredEntry> History,
        long NextSequence);

    private sealed class SnapshotDocument
    {
        [JsonRequired]
        public int SchemaVersion { get; init; }

        [JsonRequired]
        public int MaxEntries { get; init; }

        [JsonRequired]
        public long NextSequence { get; init; }

        [JsonRequired]
        public SnapshotEntry?[]? Entries { get; init; }
    }

    private sealed class SnapshotEntry
    {
        [JsonRequired]
        public long Sequence { get; init; }

        [JsonRequired]
        public string? TaskDescription { get; init; }

        [JsonRequired]
        public SnapshotSubTask?[]? Decomposition { get; init; }
    }

    private sealed class SnapshotSubTask
    {
        [JsonRequired]
        public string? Description { get; init; }

        [JsonRequired]
        public string? RequiredRole { get; init; }
    }
}

internal interface IFileEpisodeMemoryFileSystem
{
    byte[]? ReadCanonicalOrMissing(string canonicalPath);
    void CreateDirectory(string directoryPath);
    FileEpisodeMemoryTemporaryFile CreateUniqueWriteThroughTemporaryFile(string canonicalPath);
    void MoveOverwrite(string temporaryPath, string canonicalPath);
    void DeleteTemporaryFileIfExists(string temporaryPath);
}

internal sealed record FileEpisodeMemoryTemporaryFile(
    string Path,
    FileStream Stream);

internal sealed class RealFileEpisodeMemoryFileSystem : IFileEpisodeMemoryFileSystem
{
    internal static RealFileEpisodeMemoryFileSystem Instance { get; } = new();

    private RealFileEpisodeMemoryFileSystem()
    {
    }

    public byte[]? ReadCanonicalOrMissing(string canonicalPath)
    {
        FileAttributes attributes;
        try
        {
            attributes = File.GetAttributes(canonicalPath);
        }
        catch (FileNotFoundException exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return null;
        }
        catch (DirectoryNotFoundException exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return null;
        }

        if ((attributes & FileAttributes.Directory) != 0)
            throw new IOException("The snapshot target is a directory.");
        return File.ReadAllBytes(canonicalPath);
    }

    public void CreateDirectory(string directoryPath)
        => Directory.CreateDirectory(directoryPath);

    public FileEpisodeMemoryTemporaryFile CreateUniqueWriteThroughTemporaryFile(
        string canonicalPath)
    {
        var directory = Path.GetDirectoryName(canonicalPath)!;
        var fileName = Path.GetFileName(canonicalPath);
        for (var attempt = 0; attempt < 16; attempt++)
        {
            var temporaryPath = Path.Combine(
                directory,
                $".{fileName}.{Guid.NewGuid():N}.tmp");
            try
            {
                var stream = new FileStream(
                    temporaryPath,
                    new FileStreamOptions
                    {
                        Mode = FileMode.CreateNew,
                        Access = FileAccess.Write,
                        Share = FileShare.None,
                        Options = FileOptions.WriteThrough
                    });
                return new FileEpisodeMemoryTemporaryFile(temporaryPath, stream);
            }
            catch (IOException exception)
                when (File.Exists(temporaryPath)
                    && SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                // Retry only a genuine CreateNew collision.
            }
        }

        throw new IOException("Could not allocate a unique temporary snapshot file.");
    }

    public void MoveOverwrite(string temporaryPath, string canonicalPath)
        => File.Move(temporaryPath, canonicalPath, overwrite: true);

    public void DeleteTemporaryFileIfExists(string temporaryPath)
    {
        if (File.Exists(temporaryPath))
            File.Delete(temporaryPath);
    }
}
