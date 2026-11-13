// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using OrchestratorModel;
using AgentInfrastructure;
using System.Text;
using System.Text.Json;
using SC29 = SemanticCompiler29;

namespace LlmFromScratch.Tests;

// 15.OrchestratorModel — E32 episode decomposition, E33 stop signals, E35 dependency sort, E39 quality.
// Gate B: dotnet test --filter "OrchestratorModel_DeployTask_ArchFirstLevel" → PASSED
// All test names use class-prefixed naming to avoid collision with 22.AgentAlgorithms tests.
public class OrchestratorModelTests : IDisposable
{
    // Shared null storage — OrchestratorModel.NullSessionStorage is internal; use local stub.
    private static readonly ISessionStorage Storage = new NullSessionStorageOM();
    private readonly string _episodeRoot =
        Path.Combine(Path.GetTempPath(), $"file-episode-memory-{Guid.NewGuid():N}");

    public OrchestratorModelTests()
        => Directory.CreateDirectory(_episodeRoot);

    public void Dispose()
    {
        if (Directory.Exists(_episodeRoot))
            Directory.Delete(_episodeRoot, recursive: true);
    }

    // ── Phase 1: InMemoryEpisodeMemory ────────────────────────────────────────

    [Fact]
    public void EpisodeMemory_Store_ThenRecall_FindsBySubstring()
    {
        // Gate B: recalled list contains the stored task description.
        var memory = new InMemoryEpisodeMemory(Storage);
        var tasks  = new List<AgentAlgorithms.SubTask>
            { new("design the auth", "architect") };

        memory.Store("Design and implement auth system", tasks);
        var recalled = memory.Recall("auth system");

        Assert.True(recalled.IsSuccess);
        Assert.Single(recalled.Value!);
    }

    [Fact]
    public void EpisodeMemory_EmptyDescription_ReturnsFail()
    {
        // Gate B: error code is EMPTY_TASK for blank description.
        var memory = new InMemoryEpisodeMemory(Storage);
        var result = memory.Store("", new List<AgentAlgorithms.SubTask>());

        Assert.False(result.IsSuccess);
        Assert.Equal("EMPTY_TASK", result.ErrorCode);
    }

    [Fact]
    public void EpisodeMemory_ExceedsMaxEntries_EvictsOldest()
    {
        // Gate B: after capacity+1 stores, oldest entry is no longer recalled.
        var memory = new InMemoryEpisodeMemory(Storage, maxEntries: 2);
        memory.Store("task 1", new List<AgentAlgorithms.SubTask>());
        memory.Store("task 2", new List<AgentAlgorithms.SubTask>());
        memory.Store("task 3", new List<AgentAlgorithms.SubTask>());

        Assert.Empty(memory.Recall("task 1").Value!);
        Assert.Single(memory.Recall("task 3").Value!);
    }

    // ── Phase 1b: FileEpisodeMemory ───────────────────────────────────────────

    [Fact]
    public void FileEpisodeMemory_ConstructorValidatesCapacityWithoutTouchingPath()
    {
        var missingParent = Path.Combine(_episodeRoot, "missing-parent");
        var snapshotPath = Path.Combine(missingParent, "episodes.json");

        _ = new FileEpisodeMemory(Storage, snapshotPath, maxEntries: 2);

        Assert.False(Directory.Exists(missingParent));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new FileEpisodeMemory(Storage, snapshotPath, maxEntries: 0));
        Assert.False(Directory.Exists(missingParent));
    }

    [Fact]
    public void FileEpisodeMemory_RecallValidatesTopKAndZeroStillHydrates()
    {
        var snapshotPath = EpisodeSnapshotPath("top-k");
        var writer = new FileEpisodeMemory(Storage, snapshotPath, maxEntries: 3);
        Assert.True(writer.Store("task alpha", []).IsSuccess);

        var reader = new FileEpisodeMemory(Storage, snapshotPath, maxEntries: 3);
        var negative = reader.Recall("", topK: -1);
        Assert.False(negative.IsSuccess);
        Assert.Equal("EPISODE_MEMORY_TOP_K_INVALID", negative.ErrorCode);
        Assert.Equal("Episode memory topK cannot be negative.", negative.ErrorMessage);

        var zero = reader.Recall("", topK: 0);
        Assert.True(zero.IsSuccess);
        Assert.Empty(zero.Value!);

        var corruptPath = EpisodeSnapshotPath("top-k-zero-corrupt");
        var corruptBytes = Encoding.UTF8.GetBytes("{ corrupt before hydration");
        File.WriteAllBytes(corruptPath, corruptBytes);
        var corruptZero = new FileEpisodeMemory(Storage, corruptPath, maxEntries: 3)
            .Recall("", topK: 0);
        Assert.False(corruptZero.IsSuccess);
        Assert.Equal("EPISODE_MEMORY_CORRUPT", corruptZero.ErrorCode);
        Assert.Equal(corruptBytes, File.ReadAllBytes(corruptPath));
    }

    [Fact]
    public void FileEpisodeMemory_MissingSnapshotAndParentRecallEmptyWithoutCreating()
    {
        var missingParent = Path.Combine(_episodeRoot, "missing-recall-parent");
        var snapshotPath = Path.Combine(missingParent, "episodes.json");
        var memory = new FileEpisodeMemory(Storage, snapshotPath, maxEntries: 2);

        var result = memory.Recall("", topK: 10);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
        Assert.False(Directory.Exists(missingParent));
        Assert.False(File.Exists(snapshotPath));
    }

    [Fact]
    public void FileEpisodeMemory_StoreThenFreshInstanceRecallsExactSchemaSnapshot()
    {
        var snapshotPath = EpisodeSnapshotPath("restart");
        var decomposition = new[]
        {
            new AgentAlgorithms.SubTask("design auth", "architect"),
            new AgentAlgorithms.SubTask("implement auth", "implementer")
        };

        var stored = new FileEpisodeMemory(Storage, snapshotPath, maxEntries: 4)
            .Store("task alpha", decomposition);
        var recalled = new FileEpisodeMemory(Storage, snapshotPath, maxEntries: 4)
            .Recall("ALPHA", topK: 2);

        Assert.True(stored.IsSuccess);
        Assert.True(recalled.IsSuccess);
        Assert.Equal(["task alpha"], recalled.Value!);
        using var document = JsonDocument.Parse(File.ReadAllBytes(snapshotPath));
        Assert.Equal(
            ["schemaVersion", "maxEntries", "nextSequence", "entries"],
            document.RootElement.EnumerateObject().Select(property => property.Name).ToArray());
        Assert.Equal(1, document.RootElement.GetProperty("schemaVersion").GetInt32());
        Assert.Equal(4, document.RootElement.GetProperty("maxEntries").GetInt32());
        Assert.Equal(1, document.RootElement.GetProperty("nextSequence").GetInt64());
        var entry = Assert.Single(document.RootElement.GetProperty("entries").EnumerateArray());
        Assert.Equal(
            ["sequence", "taskDescription", "decomposition"],
            entry.EnumerateObject().Select(property => property.Name).ToArray());
        Assert.Equal(0, entry.GetProperty("sequence").GetInt64());
        Assert.Equal("task alpha", entry.GetProperty("taskDescription").GetString());
        Assert.Equal(2, entry.GetProperty("decomposition").GetArrayLength());
    }

    [Fact]
    public void FileEpisodeMemory_CapacitySurvivesRestartWithDeterministicSequences()
    {
        var snapshotPath = EpisodeSnapshotPath("capacity");
        var first = new FileEpisodeMemory(Storage, snapshotPath, maxEntries: 2);
        Assert.True(first.Store("task A", []).IsSuccess);
        Assert.True(first.Store("task B", []).IsSuccess);

        var second = new FileEpisodeMemory(Storage, snapshotPath, maxEntries: 2);
        Assert.True(second.Store("task C", []).IsSuccess);

        var third = new FileEpisodeMemory(Storage, snapshotPath, maxEntries: 2);
        Assert.Equal(["task B", "task C"], third.Recall("", topK: 10).Value!);
        using var document = JsonDocument.Parse(File.ReadAllBytes(snapshotPath));
        var entries = document.RootElement.GetProperty("entries").EnumerateArray().ToArray();
        Assert.Equal([1L, 2L], entries.Select(entry => entry.GetProperty("sequence").GetInt64()).ToArray());
        Assert.Equal(3, document.RootElement.GetProperty("nextSequence").GetInt64());
    }

    [Fact]
    public void FileEpisodeMemory_CorruptJsonFailsHydrationAndStoreWithoutOverwrite()
    {
        var snapshotPath = EpisodeSnapshotPath("corrupt");
        var bytes = Encoding.UTF8.GetBytes("{ definitely not valid JSON");
        File.WriteAllBytes(snapshotPath, bytes);
        var memory = new FileEpisodeMemory(Storage, snapshotPath, maxEntries: 2);

        var recall = memory.Recall("", topK: 3);
        var store = memory.Store("must not overwrite", []);

        Assert.False(recall.IsSuccess);
        Assert.Equal("EPISODE_MEMORY_CORRUPT", recall.ErrorCode);
        Assert.Equal("Episode memory snapshot JSON is corrupt.", recall.ErrorMessage);
        Assert.False(store.IsSuccess);
        Assert.Equal("EPISODE_MEMORY_CORRUPT", store.ErrorCode);
        Assert.Equal(bytes, File.ReadAllBytes(snapshotPath));
    }

    [Fact]
    public void FileEpisodeMemory_ValidWrongSchemaOrderAndCapacityFailWithoutOverwrite()
    {
        AssertSnapshotRejectedWithoutOverwrite(
            "wrong-version",
            """
            {"schemaVersion":2,"maxEntries":2,"nextSequence":0,"entries":[]}
            """,
            configuredCapacity: 2,
            "EPISODE_MEMORY_SCHEMA_INVALID");
        AssertSnapshotRejectedWithoutOverwrite(
            "wrong-order",
            """
            {
              "schemaVersion":1,
              "maxEntries":2,
              "nextSequence":2,
              "entries":[
                {"sequence":1,"taskDescription":"newer","decomposition":[]},
                {"sequence":0,"taskDescription":"older","decomposition":[]}
              ]
            }
            """,
            configuredCapacity: 2,
            "EPISODE_MEMORY_SCHEMA_INVALID");
        AssertSnapshotRejectedWithoutOverwrite(
            "wrong-capacity",
            """
            {"schemaVersion":1,"maxEntries":3,"nextSequence":0,"entries":[]}
            """,
            configuredCapacity: 2,
            "EPISODE_MEMORY_CONFIGURATION_MISMATCH");
    }

    [Fact]
    public void FileEpisodeMemory_LargerValidPersistedCapacityIsConfigurationMismatch()
    {
        AssertSnapshotRejectedWithoutOverwrite(
            "larger-valid-persisted-capacity",
            """
            {
              "schemaVersion":1,
              "maxEntries":3,
              "nextSequence":3,
              "entries":[
                {"sequence":0,"taskDescription":"task 0","decomposition":[]},
                {"sequence":1,"taskDescription":"task 1","decomposition":[]},
                {"sequence":2,"taskDescription":"task 2","decomposition":[]}
              ]
            }
            """,
            configuredCapacity: 2,
            "EPISODE_MEMORY_CONFIGURATION_MISMATCH");
    }

    [Theory]
    [InlineData(
        "duplicate-root",
        """{"schemaVersion":1,"schemaVersion":1,"maxEntries":2,"nextSequence":0,"entries":[]}""")]
    [InlineData(
        "duplicate-entry",
        """{"schemaVersion":1,"maxEntries":2,"nextSequence":1,"entries":[{"sequence":0,"sequence":0,"taskDescription":"task","decomposition":[]}]}""")]
    [InlineData(
        "duplicate-subtask",
        """{"schemaVersion":1,"maxEntries":2,"nextSequence":1,"entries":[{"sequence":0,"taskDescription":"task","decomposition":[{"description":"step","description":"step","requiredRole":"role"}]}]}""")]
    [InlineData(
        "missing-member",
        """{"schemaVersion":1,"nextSequence":0,"entries":[]}""")]
    [InlineData(
        "unknown-member",
        """{"schemaVersion":1,"maxEntries":2,"nextSequence":1,"entries":[{"sequence":0,"taskDescription":"task","decomposition":[],"unknown":true}]}""")]
    [InlineData(
        "wrong-case-member",
        """{"schemaVersion":1,"maxEntries":2,"nextSequence":1,"entries":[{"sequence":0,"taskDescription":"task","decomposition":[{"Description":"step","requiredRole":"role"}]}]}""")]
    public void FileEpisodeMemory_ExactSchemaRejectsDuplicateMissingUnknownAndWrongCaseMembers(
        string name,
        string json)
    {
        AssertSnapshotRejectedWithoutOverwrite(
            name,
            json,
            configuredCapacity: 2,
            "EPISODE_MEMORY_SCHEMA_INVALID");
    }

    [Fact]
    public void FileEpisodeMemory_ExhaustedSequenceFailsWithoutOverwrite()
    {
        var snapshotPath = EpisodeSnapshotPath("sequence-exhausted");
        var bytes = Encoding.UTF8.GetBytes(
            """
            {
              "schemaVersion":1,
              "maxEntries":1,
              "nextSequence":9223372036854775807,
              "entries":[
                {
                  "sequence":9223372036854775806,
                  "taskDescription":"last writable sequence",
                  "decomposition":[]
                }
              ]
            }
            """);
        File.WriteAllBytes(snapshotPath, bytes);
        var memory = new FileEpisodeMemory(Storage, snapshotPath, maxEntries: 1);

        var result = memory.Store("must not overflow", []);

        Assert.False(result.IsSuccess);
        Assert.Equal("EPISODE_MEMORY_SEQUENCE_EXHAUSTED", result.ErrorCode);
        Assert.Equal("Episode memory sequence is exhausted.", result.ErrorMessage);
        Assert.Equal(bytes, File.ReadAllBytes(snapshotPath));
        Assert.Equal(["last writable sequence"], memory.Recall("", topK: 1).Value!);
    }

    [Fact]
    public void FileEpisodeMemory_InvalidPathAndCanonicalDirectoryUseStableNonLeakingErrors()
    {
        var invalid = new FileEpisodeMemory(Storage, "\0", maxEntries: 2)
            .Recall("", topK: 1);
        Assert.False(invalid.IsSuccess);
        Assert.Equal("EPISODE_MEMORY_PATH_INVALID", invalid.ErrorCode);
        Assert.Equal("Episode memory snapshot path is invalid.", invalid.ErrorMessage);

        var directoryPath = Path.Combine(_episodeRoot, "canonical-is-directory");
        Directory.CreateDirectory(directoryPath);
        var read = new FileEpisodeMemory(Storage, directoryPath, maxEntries: 2)
            .Recall("", topK: 1);
        Assert.False(read.IsSuccess);
        Assert.Equal("EPISODE_MEMORY_READ_FAILED", read.ErrorCode);
        Assert.Equal("Episode memory snapshot could not be read.", read.ErrorMessage);
        Assert.DoesNotContain(directoryPath, read.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain("IOException", read.ErrorMessage, StringComparison.Ordinal);
    }

    [Fact]
    public void FileEpisodeMemory_MoveFailurePreservesCanonicalAndMemoryAndCleansTemp()
    {
        var snapshotPath = EpisodeSnapshotPath("move-failure");
        Assert.True(new FileEpisodeMemory(Storage, snapshotPath, maxEntries: 3)
            .Store("task A", [])
            .IsSuccess);
        var canonicalBytes = File.ReadAllBytes(snapshotPath);
        var fileSystem = new FaultingEpisodeMemoryFileSystem
        {
            MoveException = new IOException("injected move failure")
        };
        var memory = new FileEpisodeMemory(
            Storage,
            snapshotPath,
            maxEntries: 3,
            fileSystem);

        var store = memory.Store("task B", []);

        Assert.False(store.IsSuccess);
        Assert.Equal("EPISODE_MEMORY_WRITE_FAILED", store.ErrorCode);
        Assert.Equal("Episode memory snapshot could not be written.", store.ErrorMessage);
        Assert.Equal(canonicalBytes, File.ReadAllBytes(snapshotPath));
        Assert.Equal(["task A"], memory.Recall("", topK: 3).Value!);
        Assert.NotEmpty(fileSystem.TemporaryPaths);
        Assert.All(fileSystem.TemporaryPaths, path => Assert.False(File.Exists(path)));
    }

    [Fact]
    public void FileEpisodeMemory_CleanupFailureDoesNotMaskPrimaryWriteFailure()
    {
        var snapshotPath = EpisodeSnapshotPath("cleanup-failure");
        Assert.True(new FileEpisodeMemory(Storage, snapshotPath, maxEntries: 3)
            .Store("task A", [])
            .IsSuccess);
        var canonicalBytes = File.ReadAllBytes(snapshotPath);
        var fileSystem = new FaultingEpisodeMemoryFileSystem
        {
            MoveException = new IOException("primary move failure"),
            DeleteException = new IOException("secondary cleanup failure")
        };
        var memory = new FileEpisodeMemory(
            Storage,
            snapshotPath,
            maxEntries: 3,
            fileSystem);

        var store = memory.Store("task B", []);

        Assert.False(store.IsSuccess);
        Assert.Equal("EPISODE_MEMORY_WRITE_FAILED", store.ErrorCode);
        Assert.Equal("Episode memory snapshot could not be written.", store.ErrorMessage);
        Assert.Equal(canonicalBytes, File.ReadAllBytes(snapshotPath));
        Assert.All(fileSystem.TemporaryPaths, path => Assert.False(File.Exists(path)));
    }

    [Fact]
    public void FileEpisodeMemory_ClonesCallerDecompositionBeforePersistence()
    {
        var snapshotPath = EpisodeSnapshotPath("caller-ownership");
        var decomposition = new List<AgentAlgorithms.SubTask>
        {
            new("original description", "original role")
        };
        var memory = new FileEpisodeMemory(Storage, snapshotPath, maxEntries: 2);

        Assert.True(memory.Store("task owned", decomposition).IsSuccess);
        decomposition[0] = new AgentAlgorithms.SubTask("mutated description", "mutated role");
        decomposition.Clear();
        Assert.True(memory.Store("task second", []).IsSuccess);

        using var document = JsonDocument.Parse(File.ReadAllBytes(snapshotPath));
        var persistedSubTask = document.RootElement
            .GetProperty("entries")[0]
            .GetProperty("decomposition")[0];
        Assert.Equal("original description", persistedSubTask.GetProperty("description").GetString());
        Assert.Equal("original role", persistedSubTask.GetProperty("requiredRole").GetString());

        var firstRecall = memory.Recall("", topK: 2).Value!;
        var mutableRecall = Assert.IsType<string[]>(firstRecall);
        mutableRecall[0] = "forged caller mutation";
        Assert.Equal(
            ["task owned", "task second"],
            memory.Recall("", topK: 2).Value!);
        Assert.Equal(
            ["task owned", "task second"],
            new FileEpisodeMemory(Storage, snapshotPath, maxEntries: 2)
                .Recall("", topK: 2)
                .Value!);
    }

    [Fact]
    public void FileEpisodeMemory_OneInstanceConcurrentStoresRemainBoundedAndOrdered()
    {
        var snapshotPath = EpisodeSnapshotPath("concurrent");
        var memory = new FileEpisodeMemory(Storage, snapshotPath, maxEntries: 10);
        var results = new OperationResult<bool>[32];

        Parallel.For(
            0,
            results.Length,
            index => results[index] = memory.Store($"concurrent task {index:D2}", []));

        Assert.All(results, result => Assert.True(result.IsSuccess, result.ErrorMessage));
        using var document = JsonDocument.Parse(File.ReadAllBytes(snapshotPath));
        var entries = document.RootElement.GetProperty("entries").EnumerateArray().ToArray();
        Assert.Equal(10, entries.Length);
        Assert.Equal(32, document.RootElement.GetProperty("nextSequence").GetInt64());
        var sequences = entries
            .Select(entry => entry.GetProperty("sequence").GetInt64())
            .ToArray();
        Assert.Equal(10, sequences.Distinct().Count());
        Assert.True(sequences.Zip(sequences.Skip(1)).All(pair => pair.Second == pair.First + 1));
        var tasks = entries
            .Select(entry => entry.GetProperty("taskDescription").GetString()!)
            .ToArray();
        Assert.Equal(10, tasks.Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(
            tasks,
            new FileEpisodeMemory(Storage, snapshotPath, maxEntries: 10)
                .Recall("", topK: 100)
                .Value!);
    }

    [Fact]
    public void FileEpisodeMemory_ExceptionGraphPolicyMapsOrdinaryAndEscapesFatalOrCancellation()
    {
        var ordinary = new FileEpisodeMemory(
            Storage,
            EpisodeSnapshotPath("ordinary-read"),
            maxEntries: 2,
            new FaultingEpisodeMemoryFileSystem
            {
                ReadException = new IOException("ordinary I/O")
            });
        Assert.Equal("EPISODE_MEMORY_READ_FAILED", ordinary.Recall("", 1).ErrorCode);

        var cancellation = new FileEpisodeMemory(
            Storage,
            EpisodeSnapshotPath("cancel-read"),
            maxEntries: 2,
            new FaultingEpisodeMemoryFileSystem
            {
                ReadException = new IOException(
                    "outer",
                    new AggregateException(
                        new InvalidOperationException("ordinary sibling"),
                        new OperationCanceledException("nested aggregate cancellation")))
            });
        Assert.Throws<IOException>(() => cancellation.Recall("", 1));

        var fatal = new FileEpisodeMemory(
            Storage,
            EpisodeSnapshotPath("fatal-read"),
            maxEntries: 2,
            new FaultingEpisodeMemoryFileSystem
            {
                ReadException = new IOException(
                    "outer",
                    new OutOfMemoryException("nested fatal"))
            });
        Assert.Throws<IOException>(() => fatal.Recall("", 1));

        var cleanupPath = EpisodeSnapshotPath("fatal-cleanup");
        var fatalCleanup = new FileEpisodeMemory(
            Storage,
            cleanupPath,
            maxEntries: 2,
            new FaultingEpisodeMemoryFileSystem
            {
                MoveException = new IOException("primary move failure"),
                DeleteException = new IOException(
                    "cleanup outer",
                    new OperationCanceledException("cleanup cancellation"))
            });
        Assert.Throws<IOException>(() => fatalCleanup.Store("task", []));
    }

    [Fact]
    public void FileEpisodeMemory_InvalidInputsReturnStableCodes()
    {
        var memory = new FileEpisodeMemory(
            Storage,
            EpisodeSnapshotPath("invalid-input"),
            maxEntries: 2);

        Assert.Equal("EMPTY_TASK", memory.Store(" ", []).ErrorCode);
        Assert.Equal(
            "EPISODE_MEMORY_DECOMPOSITION_INVALID",
            memory.Store("task", null!).ErrorCode);
        Assert.Equal(
            "EPISODE_MEMORY_DECOMPOSITION_INVALID",
            memory.Store("task", [new AgentAlgorithms.SubTask("", "role")]).ErrorCode);
        Assert.Equal(
            "EPISODE_MEMORY_QUERY_INVALID",
            memory.Recall(null!, topK: 1).ErrorCode);
    }

    // ── Phase 2: EpisodeClassifier ────────────────────────────────────────────

    [Fact]
    public void EpisodeClassifier_UnknownDomain_FallsBackToCodeQ()
    {
        // Gate B: Domain.UNKNOWN from pipeline → CODE_Q (safe default, never unknown to caller).
        var compiler   = new StubSC29Compiler(SC29.Domain.UNKNOWN, ratio: 5.0);
        var classifier = new EpisodeClassifier(compiler, Storage);
        var result     = classifier.Classify("some obscure task");

        Assert.True(result.IsSuccess);
        Assert.Equal(TaskDomain.CODE_Q, result.Value!);
    }

    [Fact]
    public void EpisodeClassifier_LowCompressionRatio_LowConfidence()
    {
        // Gate B: CompressionRatio=5.0 → confidence=0.25, below 0.6 threshold for DynamicDomains.
        var compiler   = new StubSC29Compiler(SC29.Domain.CODE_Q, ratio: 5.0);
        var classifier = new EpisodeClassifier(compiler, Storage);
        var result     = classifier.ClassifyWithConfidence("Write Dockerfile for .NET app");

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.Confidence < 0.6,
            $"Expected confidence < 0.6, got {result.Value!.Confidence}");
    }

    // ── Phase 2: EpisodeDecomposer ────────────────────────────────────────────

    [Fact]
    public void EpisodeDecomposer_PlanDomain_Returns4SubEpisodes()
    {
        // Gate B: PLAN domain caps at 4 sub-episodes (E32 +11% validated count).
        var decomposer = new EpisodeDecomposer(Storage);
        var result     = decomposer.Decompose(
            "Design auth system and implement it and test it and document it",
            TaskDomain.PLAN);

        Assert.True(result.IsSuccess);
        Assert.Equal(4, result.Value!.Count);
    }

    [Fact]
    public void EpisodeDecomposer_FalseConjunction_StaysAsSingleTask()
    {
        // Gate B: "log them" is a dependent clause — whole description stays as one task.
        var decomposer = new EpisodeDecomposer(Storage);
        var result     = decomposer.Decompose(
            "Find methods that return null and log them", TaskDomain.CODE_Q);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
    }

    [Fact]
    public void EpisodeDecomposer_BugDomain_FirstRoleIsDebugger()
    {
        // Gate B: position-0 sub-task for BUG domain must have role "debugger" (E32 role mapping).
        var decomposer = new EpisodeDecomposer(Storage);
        var result     = decomposer.Decompose(
            "Fix NullReference in UserService then add regression test", TaskDomain.BUG);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
        Assert.Equal("debugger", result.Value![0].RequiredRole);
    }

    // ── Phase 3: StopSignalDetector ───────────────────────────────────────────

    [Fact]
    public void StopSignalDetector_PhaseSequence_ReturnsTrue()
    {
        // Gate B: "first…then" phrase triggers PHASES stop signal (no independent sub-episodes).
        var detector = new StopSignalDetector(Storage);
        var result   = detector.ShouldStop("First read the requirements then decide the approach");

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!);
    }

    [Fact]
    public void StopSignalDetector_IndependentGoal_ReturnsFalse()
    {
        // Gate B: 5-word substantive goal with no stop-signal patterns → false.
        var detector = new StopSignalDetector(Storage);
        var result   = detector.ShouldStop("Design the authentication module architecture");

        Assert.True(result.IsSuccess);
        Assert.False(result.Value!);
    }

    // ── Phase 3: DependencyGraph reuse from 22.AgentAlgorithms ───────────────

    [Fact]
    public void DependencyGraph_ArchBeforeImplementer_UsableFrom15()
    {
        // Gate B: architect in level 0, implementer in level 1 — no duplication from 22.
        var graph = new AgentAlgorithms.DependencyGraph();
        graph.AddDependency("architect", "implementer");

        var tasks = new List<AgentAlgorithms.SubTask>
        {
            new("design the auth system",     "architect"),
            new("implement the auth service", "implementer"),
        };
        var result = graph.Sort(tasks);

        Assert.True(result.IsSuccess);
        Assert.Equal("architect",   result.Value![0][0].RequiredRole);
        Assert.Equal("implementer", result.Value![1][0].RequiredRole);
    }

    // ── Phase 4: AgentLauncher ────────────────────────────────────────────────

    [Fact]
    public async Task AgentLauncher_EmptyDescription_ReturnsFail()
    {
        // Gate B: empty sub-task description is rejected before reaching the orchestrator.
        var orch     = BuildStubOrchestrator();
        var launcher = new AgentLauncher(Storage, orch);
        var result   = await launcher.LaunchAsync(
            new AgentAlgorithms.SubTask("", "architect"), "");

        Assert.False(result.IsSuccess);
        Assert.Equal("EMPTY_TASK", result.ErrorCode);
    }

    // ── Phase 4: ResultMerger ─────────────────────────────────────────────────

    [Fact]
    public void ResultMerger_SingleResult_ReturnedUnchanged()
    {
        // Gate B: single result bypasses beautifier and is returned verbatim.
        var beautifier = BuildStubBeautifier();
        var merger     = new ResultMerger(Storage, beautifier);
        var task       = new AgentAlgorithms.SubTask("design auth", "architect");

        var result = merger.Merge([(task, "auth schema: JWT + RBAC")]);

        Assert.True(result.IsSuccess);
        Assert.Equal("auth schema: JWT + RBAC", result.Value!);
    }

    [Fact]
    public void ResultMerger_EmptyResults_ReturnsFail()
    {
        // Gate B: empty list returns EMPTY_RESULTS error code.
        var beautifier = BuildStubBeautifier();
        var merger     = new ResultMerger(Storage, beautifier);
        var result     = merger.Merge([]);

        Assert.False(result.IsSuccess);
        Assert.Equal("EMPTY_RESULTS", result.ErrorCode);
    }

    // ── Phase 5: QualityOrchestrator ─────────────────────────────────────────

    [Fact]
    public void QualityOrchestrator_ShortResult_FailsOutputContract()
    {
        // Gate B: result under 10 characters fails OUTPUT_CONTRACT check.
        var quality = new QualityOrchestrator(Storage);
        var report  = quality.Check("add null check", "ok", "CODE_SERVICE");

        Assert.True(report.IsSuccess);
        Assert.False(report.Value!.Passed);
        Assert.Contains("OUTPUT_CONTRACT", report.Value!.FailedChecks);
    }

    [Fact]
    public void QualityOrchestrator_SubstantiveResultWithReturn_Passes()
    {
        // Gate B: C# guard clause ≥ 10 chars containing "return" → all checks pass.
        var quality = new QualityOrchestrator(Storage);
        string res  = "if (user == null) return OperationResult<User>.Fail(\"not found\");";
        var report  = quality.Check("add null check to GetUser", res, "CODE_SERVICE");

        Assert.True(report.IsSuccess);
        Assert.True(report.Value!.Passed,
            $"Expected pass, failed: {string.Join(", ", report.Value!.FailedChecks)}");
    }

    // ── Phase 5: Gate B integration test ─────────────────────────────────────

    [Fact]
    public async Task OrchestratorModel_DeployTask_ArchFirstLevel()
    {
        // Gate B: "Deploy new application" → PLAN decomposition → planner in level 0.
        // Verifies the full Classify→Decompose→Sort chain without the launcher.
        var graph = new AgentAlgorithms.DependencyGraph();
        graph.AddDependency("architect", "implementer");

        var decomposer = new EpisodeDecomposer(Storage);
        var tasks      = decomposer.Decompose(
            "Design deployment architecture and implement the service and deploy and monitor",
            TaskDomain.PLAN);

        Assert.True(tasks.IsSuccess);

        var sorted = graph.Sort(tasks.Value!.ToList());
        Assert.True(sorted.IsSuccess);

        var firstLevel = sorted.Value![0];
        Assert.Contains(firstLevel, t =>
            t.RequiredRole == "architect" || t.RequiredRole == "planner");
        await Task.CompletedTask;
    }

    [Fact]
    public async Task OrchestratorFacade_MemoryFailurePreservesExactCodeAndMessage()
    {
        const string expectedCode = "EPISODE_MEMORY_WRITE_FAILED";
        const string expectedMessage = "Episode memory snapshot could not be written.";
        var facade = new OrchestratorFacade(
            Storage,
            new FacadeClassifierOM(),
            new FacadeDecomposerOM(),
            new FacadeStopSignalDetectorOM(),
            new AgentAlgorithms.DependencyGraph(),
            new FacadeLauncherOM(),
            new FacadeMergerOM(),
            new FacadeQualityOrchestratorOM(),
            new FailingEpisodeMemoryOM(expectedMessage, expectedCode));

        var result = await facade.ProcessAsync("persist this completed episode");

        Assert.False(result.IsSuccess);
        Assert.Equal(expectedCode, result.ErrorCode);
        Assert.Equal(expectedMessage, result.ErrorMessage);
        Assert.Null(result.Value);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private string EpisodeSnapshotPath(string name)
        => Path.Combine(_episodeRoot, $"{name}.json");

    private void AssertSnapshotRejectedWithoutOverwrite(
        string name,
        string json,
        int configuredCapacity,
        string expectedCode)
    {
        var snapshotPath = EpisodeSnapshotPath(name);
        var bytes = Encoding.UTF8.GetBytes(json);
        File.WriteAllBytes(snapshotPath, bytes);
        var memory = new FileEpisodeMemory(
            Storage,
            snapshotPath,
            configuredCapacity);

        var recall = memory.Recall("", topK: 10);
        var store = memory.Store("must not overwrite", []);

        Assert.False(recall.IsSuccess);
        Assert.Equal(expectedCode, recall.ErrorCode);
        Assert.False(store.IsSuccess);
        Assert.Equal(expectedCode, store.ErrorCode);
        Assert.Equal(bytes, File.ReadAllBytes(snapshotPath));
    }

    private static AgentAlgorithms.AgentOrchestrator BuildStubOrchestrator()
    {
        // Minimal orchestrator: stub bus + default decomposer + default dependency graph.
        var bus       = new StubMessageBusOM();
        var decomposer = new AgentAlgorithms.ComplexTaskDecomposer();
        return new AgentAlgorithms.AgentOrchestrator(bus, decomposer);
    }

    private static ITemplateBeautifier BuildStubBeautifier()
    {
        // Wrap the real SC29 TemplateBeautifier behind the fabric interface.
        var inner = new SC29.TemplateBeautifier(
            new NullCompilerStorageOM(), new NullCompilerEventLoggerOM());
        return new TemplateBeautifierAdapter(inner);
    }
}

// ── File-scoped fakes ─────────────────────────────────────────────────────────

// ISessionStorage no-op for OrchestratorModel tests (OrchestratorModel.NullSessionStorage is internal).
file sealed class NullSessionStorageOM : ISessionStorage
{
    public Task<OperationResult<bool>> SaveAsync(string sessionId, AgentState state)
        => Task.FromResult(OperationResult<bool>.Ok(true));

    public Task<OperationResult<AgentState>> LoadAsync(string sessionId)
        => Task.FromResult(OperationResult<AgentState>.Fail("Not persisted.", "NULL_STORAGE"));

    public Task<OperationResult<IReadOnlyList<string>>> ListSessionsAsync()
        => Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok(
               (IReadOnlyList<string>)Array.Empty<string>()));
}

file sealed class FaultingEpisodeMemoryFileSystem : IFileEpisodeMemoryFileSystem
{
    private readonly IFileEpisodeMemoryFileSystem _inner =
        RealFileEpisodeMemoryFileSystem.Instance;

    public Exception? ReadException { get; init; }
    public Exception? MoveException { get; init; }
    public Exception? DeleteException { get; init; }
    public List<string> TemporaryPaths { get; } = [];

    public byte[]? ReadCanonicalOrMissing(string canonicalPath)
    {
        if (ReadException is not null)
            throw ReadException;
        return _inner.ReadCanonicalOrMissing(canonicalPath);
    }

    public void CreateDirectory(string directoryPath)
        => _inner.CreateDirectory(directoryPath);

    public FileEpisodeMemoryTemporaryFile CreateUniqueWriteThroughTemporaryFile(
        string canonicalPath)
    {
        var temporary = _inner.CreateUniqueWriteThroughTemporaryFile(canonicalPath);
        TemporaryPaths.Add(temporary.Path);
        return temporary;
    }

    public void MoveOverwrite(string temporaryPath, string canonicalPath)
    {
        if (MoveException is not null)
            throw MoveException;
        _inner.MoveOverwrite(temporaryPath, canonicalPath);
    }

    public void DeleteTemporaryFileIfExists(string temporaryPath)
    {
        _inner.DeleteTemporaryFileIfExists(temporaryPath);
        if (DeleteException is not null)
            throw DeleteException;
    }
}

file sealed class FacadeClassifierOM : IEpisodeClassifier
{
    public OperationResult<TaskDomain> Classify(string taskDescription)
        => OperationResult<TaskDomain>.Ok(TaskDomain.PLAN);

    public OperationResult<(TaskDomain Domain, double Confidence)> ClassifyWithConfidence(
        string taskDescription)
        => OperationResult<(TaskDomain Domain, double Confidence)>.Ok(
            (TaskDomain.PLAN, 1.0));
}

file sealed class FacadeDecomposerOM : IEpisodeDecomposer
{
    public OperationResult<IReadOnlyList<AgentAlgorithms.SubTask>> Decompose(
        string taskDescription,
        TaskDomain domain)
        => OperationResult<IReadOnlyList<AgentAlgorithms.SubTask>>.Ok(
            [new AgentAlgorithms.SubTask("execute episode", "implementer")]);
}

file sealed class FacadeStopSignalDetectorOM : IStopSignalDetector
{
    public OperationResult<bool> ShouldStop(string subTaskDescription)
        => OperationResult<bool>.Ok(false);
}

file sealed class FacadeLauncherOM : IAgentLauncher
{
    public Task<OperationResult<string>> LaunchAsync(
        AgentAlgorithms.SubTask task,
        string context)
        => Task.FromResult(OperationResult<string>.Ok("completed episode result"));
}

file sealed class FacadeMergerOM : IResultMerger
{
    public OperationResult<string> Merge(
        IReadOnlyList<(AgentAlgorithms.SubTask Task, string Result)> results)
        => OperationResult<string>.Ok("merged completed episode result");
}

file sealed class FacadeQualityOrchestratorOM : IQualityOrchestrator
{
    public OperationResult<QualityReport> Check(
        string taskDescription,
        string result,
        string outputType)
        => OperationResult<QualityReport>.Ok(new QualityReport(true, []));
}

file sealed class FailingEpisodeMemoryOM(
    string message,
    string code) : IEpisodeMemory
{
    public OperationResult<bool> Store(
        string taskDescription,
        IReadOnlyList<AgentAlgorithms.SubTask> decomposition)
        => OperationResult<bool>.Fail(message, code);

    public OperationResult<IReadOnlyList<string>> Recall(
        string taskDescription,
        int topK = 3)
        => OperationResult<IReadOnlyList<string>>.Ok(Array.Empty<string>());
}

// ISemanticCompiler29 stub: returns a configurable domain and compression ratio.
file sealed class StubSC29Compiler : SC29.ISemanticCompiler29
{
    private readonly SC29.Domain _domain;
    private readonly double      _ratio;

    public StubSC29Compiler(SC29.Domain domain, double ratio = 15.0)
        => (_domain, _ratio) = (domain, ratio);

    public SC29.OperationResult<SC29.PipelineOutput> Compile(SC29.PipelineInput input)
        => SC29.OperationResult<SC29.PipelineOutput>.Ok(
               new SC29.PipelineOutput(_domain, [], "csharp", _ratio));
}

// IAgentMessageBus stub: returns "done:<task>" for every assignment.
file sealed class StubMessageBusOM : AgentAlgorithms.IAgentMessageBus
{
    public Task<AgentAlgorithms.OperationResult<string>> AssignTaskAsync(
        string subAgentName, string task)
        => Task.FromResult(AgentAlgorithms.OperationResult<string>.Ok($"done:{task}"));
}

// SC29 ICompilerStorage no-op (duplicated from SemanticCompiler29Tests — file-scoped there).
file sealed class NullCompilerStorageOM : SC29.ICompilerStorage
{
    public void Save(string key, SC29.PipelineOutput output) { }
    public SC29.PipelineOutput? Load(string key) => null;
}

// SC29 ICompilerEventLogger no-op (duplicated from SemanticCompiler29Tests — file-scoped there).
file sealed class NullCompilerEventLoggerOM : SC29.ICompilerEventLogger
{
    public void Log(SC29.StandardEvent evt) { }
}
