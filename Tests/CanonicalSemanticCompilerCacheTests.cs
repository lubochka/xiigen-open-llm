// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Buffers.Binary;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using ContinuousLearning;
using Microsoft.Win32.SafeHandles;
using SemanticCompiler29;

public sealed class CanonicalSemanticCompilerCacheTests
{
    private const string CanonicalName = "semantic-compiler-cache-v1.bin";
    private const string TempName = ".semantic-compiler-cache-v1.write.tmp";
    private const string BackupName = ".semantic-compiler-cache-v1.publication-backup";
    private const string AuditName = "semantic-compiler-audit-v1.log";
    private static readonly string KeyA = new('a', 64);
    private static readonly string KeyB = new('b', 64);
    private static readonly string KeyC = new('c', 64);

    [CanonicalSemanticCompilerCacheWindowsFact]
    public void Envelope_UsesExactBinaryHeaderDigestAndCanonicalJson()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var cache = CanonicalSemanticCompilerCache.Open(
            access.AcquireCompilerCacheRootCapability());
        cache.Save(KeyA, OutputA());

        var bytes = File.ReadAllBytes(Path.Combine(fixture.CacheRoot, CanonicalName));
        Assert.InRange(bytes.Length, 65, CanonicalSemanticCompilerCache.MaximumFileBytes);
        Assert.Equal("SC29CC01", Encoding.ASCII.GetString(bytes, 0, 8));
        Assert.Equal((ushort)1, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(8, 2)));
        Assert.Equal((ushort)64, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(10, 2)));
        Assert.Equal(
            bytes.Length - 64,
            checked((int)BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(12, 4))));
        Assert.Equal(1UL, BinaryPrimitives.ReadUInt64LittleEndian(bytes.AsSpan(16, 8)));
        Assert.Equal(1U, BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(24, 4)));
        Assert.Equal(0U, BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(28, 4)));
        Assert.Equal(SHA256.HashData(bytes.AsSpan(64)), bytes.AsSpan(32, 32).ToArray());
        Assert.Equal(
            $"{{\"schemaVersion\":1,\"generation\":1,\"entries\":[{{\"key\":\"{KeyA}\",\"output\":{{\"domain\":\"BUG\",\"predicates\":[\"NULL_CHECK\",\"RETURN_OP\"],\"stackLanguage\":\"csharp\",\"compressionRatio\":2.5}}}}]}}",
            Encoding.UTF8.GetString(bytes, 64, bytes.Length - 64));
        Assert.DoesNotContain("prompt", Encoding.UTF8.GetString(bytes), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("path", Encoding.UTF8.GetString(bytes), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("audit", Encoding.UTF8.GetString(bytes), StringComparison.OrdinalIgnoreCase);
    }

    [CanonicalSemanticCompilerCacheWindowsFact]
    public void CanonicalEncoding_IsIndependentOfInsertionOrderAndSortsKeysOrdinally()
    {
        RequireWindows();
        var first = CreateImage((KeyB, OutputB()), (KeyA, OutputA()));
        var second = CreateImage((KeyA, OutputA()), (KeyB, OutputB()));

        Assert.Equal(first, second);
        var json = Encoding.UTF8.GetString(first, 64, first.Length - 64);
        Assert.True(
            json.IndexOf(KeyA, StringComparison.Ordinal) <
            json.IndexOf(KeyB, StringComparison.Ordinal));
    }

    [CanonicalSemanticCompilerCacheWindowsFact]
    public void SuppliedKeyValidation_RejectsEveryNonCanonicalShapeBeforeIo()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var cache = CanonicalSemanticCompilerCache.Open(
            access.AcquireCompilerCacheRootCapability());
        var invalid = new string?[]
        {
            null,
            string.Empty,
            new('a', 63),
            new('a', 65),
            new('A', 64),
            new('g', 64),
            "../" + new string('a', 61),
            new('é', 64),
        };

        foreach (var key in invalid)
        {
            Assert.Throws<ArgumentException>(() => cache.Load(key!));
            Assert.Throws<ArgumentException>(() => cache.Save(key!, OutputA()));
        }
        Assert.False(File.Exists(Path.Combine(fixture.CacheRoot, CanonicalName)));
        Assert.False(File.Exists(Path.Combine(fixture.CacheRoot, TempName)));
    }

    [CanonicalSemanticCompilerCacheWindowsFact]
    public void OutputValidation_RejectsNonCanonicalValuesBeforeIo()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var cache = CanonicalSemanticCompilerCache.Open(
            access.AcquireCompilerCacheRootCapability());
        var invalid = new PipelineOutput?[]
        {
            null,
            new((Domain)999, ["NULL_CHECK"], "csharp", 1),
            new(Domain.BUG, null!, "csharp", 1),
            new(Domain.BUG, [], "csharp", 1),
            new(Domain.BUG, ["RETURN_OP", "NULL_CHECK"], "csharp", 1),
            new(Domain.BUG, ["NULL_CHECK", "NULL_CHECK"], "csharp", 1),
            new(Domain.BUG, ["NOT_A_PREDICATE"], "csharp", 1),
            new(Domain.BUG, Enumerable.Repeat("NULL_CHECK", 23).ToArray(), "csharp", 1),
            new(Domain.BUG, ["NULL_CHECK"], "CSharp", 1),
            new(Domain.BUG, ["NULL_CHECK"], "rust", 1),
            new(Domain.BUG, ["NULL_CHECK"], "csharp", 0),
            new(Domain.BUG, ["NULL_CHECK"], "csharp", -1),
            new(Domain.BUG, ["NULL_CHECK"], "csharp", double.NaN),
            new(Domain.BUG, ["NULL_CHECK"], "csharp", double.PositiveInfinity),
            new(Domain.BUG, ["NULL_CHECK"], "csharp", (double)int.MaxValue + 1),
        };

        foreach (var output in invalid)
            Assert.ThrowsAny<ArgumentException>(() => cache.Save(KeyA, output!));
        Assert.False(File.Exists(Path.Combine(fixture.CacheRoot, TempName)));
        Assert.False(File.Exists(Path.Combine(fixture.CacheRoot, CanonicalName)));
    }

    [Fact]
    public void PredicateVocabulary_IsExactV1CompilerVocabulary()
    {
        var compilerVocabulary = Assert.IsType<string[]>(
            typeof(LogicExtractor)
                .GetField("PredicateKeys", BindingFlags.NonPublic | BindingFlags.Static)!
                .GetValue(null));
        var cacheVocabulary = CanonicalSemanticCompilerCache.GetPredicateVocabularyForTesting();
        Assert.Equal(LogicExtractor.VocabSize, cacheVocabulary.Count);
        Assert.Equal(compilerVocabulary, cacheVocabulary);
    }

    [CanonicalSemanticCompilerCacheWindowsFact]
    public void PredicateVocabulary_DefensiveProjectionCannotMutateAuthorityOrCanonicalImage()
    {
        RequireWindows();
        var projection = Assert.IsType<string[]>(
            CanonicalSemanticCompilerCache.GetPredicateVocabularyForTesting());
        projection[0] = "INJECTED_OP";
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using (var access = fixture.CreateStartup().AcquireRetainedFileAccess())
        using (var cache = CanonicalSemanticCompilerCache.Open(
                   access.AcquireCompilerCacheRootCapability()))
        {
            cache.Save(KeyA, OutputA());
            var before = File.ReadAllBytes(Path.Combine(fixture.CacheRoot, CanonicalName));
            Assert.Throws<ArgumentException>(() => cache.Save(
                KeyB,
                new PipelineOutput(Domain.BUG, ["INJECTED_OP"], "csharp", 1)));
            Assert.Equal(before, File.ReadAllBytes(Path.Combine(fixture.CacheRoot, CanonicalName)));
        }

        using var restartedAccess = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var restarted = CanonicalSemanticCompilerCache.Open(
            restartedAccess.AcquireCompilerCacheRootCapability());
        Assert.Equal("NULL_CHECK", restarted.Load(KeyA)!.Predicates[0]);
        Assert.Null(restarted.Load(KeyB));
    }

    [CanonicalSemanticCompilerCacheWindowsFact]
    public void SaveAndLoad_DeepCopyPredicateArraysAndConflictDoesNotMutate()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var cache = CanonicalSemanticCompilerCache.Open(
            access.AcquireCompilerCacheRootCapability());
        var predicates = new[] { "NULL_CHECK", "RETURN_OP" };
        cache.Save(KeyA, new PipelineOutput(Domain.BUG, predicates, "csharp", 2.5));
        predicates[0] = "DELETE_OP";
        var loaded = cache.Load(KeyA)!;
        Assert.Equal(new[] { "NULL_CHECK", "RETURN_OP" }, loaded.Predicates);
        loaded.Predicates[0] = "DELETE_OP";
        Assert.Equal("NULL_CHECK", cache.Load(KeyA)!.Predicates[0]);

        var canonicalBefore = File.ReadAllBytes(Path.Combine(fixture.CacheRoot, CanonicalName));
        cache.Save(KeyA, OutputA());
        Assert.Equal(canonicalBefore, File.ReadAllBytes(Path.Combine(fixture.CacheRoot, CanonicalName)));
        Assert.Throws<InvalidOperationException>(() => cache.Save(
            KeyA,
            new PipelineOutput(Domain.PLAN, ["CALL_OP"], "unknown", 1)));
        Assert.Equal(canonicalBefore, File.ReadAllBytes(Path.Combine(fixture.CacheRoot, CanonicalName)));
    }

    [CanonicalSemanticCompilerCacheWindowsFact]
    public void StrictPayloadParsing_RejectsMutationsAndFailedOpenReleasesOwnerLock()
    {
        RequireWindows();
        var valid = CreateImage((KeyA, OutputA()));
        var validJson = Encoding.UTF8.GetString(valid, 64, valid.Length - 64);
        var entriesSuffix = validJson[validJson.IndexOf("\"entries\"", StringComparison.Ordinal)..];
        var mutations = new byte[][]
        {
            Reframe(valid, Encoding.UTF8.GetBytes(validJson.Insert(1, " "))),
            Reframe(valid, Encoding.UTF8.GetBytes($"{{\"generation\":1,\"schemaVersion\":1,{entriesSuffix}")),
            Reframe(valid, Encoding.UTF8.GetBytes(validJson.Replace("{\"schemaVersion\":1", "{\"schemaVersion\":1,\"unknown\":0", StringComparison.Ordinal))),
            Reframe(valid, Encoding.UTF8.GetBytes(validJson.Replace("{\"schemaVersion\":1", "{\"schemaVersion\":1,\"schemaVersion\":1", StringComparison.Ordinal))),
            Reframe(valid, [0xEF, 0xBB, 0xBF, .. valid.AsSpan(64).ToArray()]),
            Reframe(valid, [0xFF, 0xFE, 0xFD]),
            Reframe(valid, Encoding.UTF8.GetBytes(validJson.Replace("2.5", "2.500", StringComparison.Ordinal))),
            Reframe(valid, Encoding.UTF8.GetBytes("{\"schemaVersion\":1,\"generation\":1,\"entries\":[[[[[[[{}]]]]]]]}")),
        };
        foreach (var mutation in mutations)
        {
            using var fixture = new FileAccessFixture(provisionAllRoots: true);
            File.WriteAllBytes(Path.Combine(fixture.CacheRoot, CanonicalName), mutation);
            using (var access = fixture.CreateStartup().AcquireRetainedFileAccess())
            {
                var capability = access.AcquireCompilerCacheRootCapability();
                var ownerLockHandle = GetCompilerCacheOwnerLockHandle(capability);
                Assert.False(ownerLockHandle.IsClosed);

                // A bad cache image must release only the one-shot compiler-cache lock.
                // The broader file-access owner is still alive until this using block ends.
                Assert.ThrowsAny<Exception>(() => CanonicalSemanticCompilerCache.Open(capability));
                Assert.True(ownerLockHandle.IsClosed);
                Assert.True(access.ValidateExistingRegularLeaf(
                    ContinuousLearningRetainedRootRole.Cache,
                    CanonicalName,
                    CanonicalSemanticCompilerCache.MaximumFileBytes));
            }

            // A new mutation owner is allowed only after the previous parent owner
            // has been disposed; creating it earlier would test the wrong lifetime.
            using var secondAccess = fixture.CreateStartup().AcquireRetainedFileAccess();
            using var secondCapability = secondAccess.AcquireCompilerCacheRootCapability();
        }
    }

    [CanonicalSemanticCompilerCacheWindowsTheory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    [InlineData(true, true, false)]
    [InlineData(false, true, true)]
    [InlineData(true, false, true)]
    [InlineData(true, true, true)]
    public void RestartRecovery_ImplementsEveryPresenceRow(
        bool hasCanonical,
        bool hasTemp,
        bool hasBackup)
    {
        RequireWindows();
        var generationOne = CreateImage((KeyA, OutputA()));
        var generationTwo = CreateImage((KeyA, OutputA()), (KeyB, OutputB()));
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        if (hasCanonical)
        {
            File.WriteAllBytes(
                Path.Combine(fixture.CacheRoot, CanonicalName),
                hasBackup ? generationTwo : generationOne);
        }
        if (hasTemp)
            File.WriteAllBytes(Path.Combine(fixture.CacheRoot, TempName), [0xDE, 0xAD]);
        if (hasBackup)
            File.WriteAllBytes(Path.Combine(fixture.CacheRoot, BackupName), generationOne);
        var before = ProtocolSnapshot(fixture.CacheRoot);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        var capability = access.AcquireCompilerCacheRootCapability();

        if (hasCanonical && hasTemp && hasBackup)
        {
            Assert.Throws<InvalidDataException>(() => CanonicalSemanticCompilerCache.Open(capability));
            Assert.Equal(before, ProtocolSnapshot(fixture.CacheRoot));
            return;
        }

        using var cache = CanonicalSemanticCompilerCache.Open(capability);
        var expectsA = hasCanonical || hasBackup;
        Assert.Equal(expectsA, cache.Load(KeyA) is not null);
        Assert.Equal(hasCanonical && hasBackup, cache.Load(KeyB) is not null);
        Assert.False(File.Exists(Path.Combine(fixture.CacheRoot, TempName)));
        Assert.False(File.Exists(Path.Combine(fixture.CacheRoot, BackupName)));
        Assert.Equal(expectsA, File.Exists(Path.Combine(fixture.CacheRoot, CanonicalName)));
    }

    [CanonicalSemanticCompilerCacheWindowsFact]
    public void Recovery_CorruptionOrUnprovenRelationshipFailsClosedWithoutMutation()
    {
        RequireWindows();
        var generationOne = CreateImage((KeyA, OutputA()));
        var generationOneOther = CreateImage((KeyB, OutputB()));
        var generationTwoPriorChanged = CreateImage(
            (KeyA, new PipelineOutput(Domain.PLAN, ["CALL_OP"], "unknown", 1)),
            (KeyB, OutputB()));
        var scenarios = new (byte[] Canonical, byte[] Backup)[]
        {
            ([1, 2, 3], generationOne),
            (generationOne, [1, 2, 3]),
            (generationOneOther, generationOne),
            (generationTwoPriorChanged, generationOne),
        };
        foreach (var scenario in scenarios)
        {
            using var fixture = new FileAccessFixture(provisionAllRoots: true);
            File.WriteAllBytes(Path.Combine(fixture.CacheRoot, CanonicalName), scenario.Canonical);
            File.WriteAllBytes(Path.Combine(fixture.CacheRoot, BackupName), scenario.Backup);
            var before = ProtocolSnapshot(fixture.CacheRoot);
            using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
            var capability = access.AcquireCompilerCacheRootCapability();
            Assert.ThrowsAny<Exception>(() => CanonicalSemanticCompilerCache.Open(capability));
            Assert.Equal(before, ProtocolSnapshot(fixture.CacheRoot));
        }
    }

    [CanonicalSemanticCompilerCacheWindowsFact]
    public void TempBeyondCapabilityBound_FailsOpenWithoutMutation()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        var tempPath = Path.Combine(fixture.CacheRoot, TempName);
        using (var stream = new FileStream(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
            stream.SetLength(ServerOwnedContinuousLearningCacheRootNative.MaximumOperationBytes + 1L);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        var capability = access.AcquireCompilerCacheRootCapability();

        Assert.Throws<InvalidDataException>(() => CanonicalSemanticCompilerCache.Open(capability));
        Assert.Equal(
            ServerOwnedContinuousLearningCacheRootNative.MaximumOperationBytes + 1L,
            new FileInfo(tempPath).Length);
    }

    [CanonicalSemanticCompilerCacheWindowsTheory]
    [InlineData(CanonicalName)]
    [InlineData(BackupName)]
    public void CanonicalOrBackupBeyondStoreBound_FailsOpenWithoutMutation(string leafName)
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        var leafPath = Path.Combine(fixture.CacheRoot, leafName);
        using (var stream = new FileStream(leafPath, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
            stream.SetLength(CanonicalSemanticCompilerCache.MaximumFileBytes + 1L);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        var capability = access.AcquireCompilerCacheRootCapability();

        Assert.Throws<InvalidDataException>(() => CanonicalSemanticCompilerCache.Open(capability));
        Assert.Equal(
            CanonicalSemanticCompilerCache.MaximumFileBytes + 1L,
            new FileInfo(leafPath).Length);
        Assert.Equal(
            leafName == CanonicalName,
            File.Exists(Path.Combine(fixture.CacheRoot, CanonicalName)));
        Assert.Equal(
            leafName == BackupName,
            File.Exists(Path.Combine(fixture.CacheRoot, BackupName)));
    }

    [CanonicalSemanticCompilerCacheWindowsFact]
    public void PublicationStages_HaveExactFileRenameCommitAndCleanupOrder()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        var stages = new List<string>();
        var record = false;
        var capability = access.AcquireCompilerCacheRootCapability(
            new ServerOwnedContinuousLearningCacheRootCapabilityOptions
            {
                ObserveIoStageForTesting = stage =>
                {
                    if (record)
                        stages.Add($"io:{stage}");
                },
            });
        using var cache = CanonicalSemanticCompilerCache.Open(
            capability,
            new CanonicalSemanticCompilerCacheOptions
            {
                ObserveStageForTesting = stage =>
                {
                    if (record)
                        stages.Add($"store:{stage}");
                },
            });
        cache.Save(KeyA, OutputA());
        record = true;

        cache.Save(KeyB, OutputB());

        Assert.Equal(
            new[]
            {
                "io:AfterBoundedExactReadPostValidationBeforeComparison",
                "io:BeforeFileFlush",
                "io:AfterFileFlush",
                "store:AfterTempFileDurable",
                "io:AfterBoundedExactReadPostValidationBeforeComparison",
                "io:BeforeHandleRename",
                "io:AfterHandleRename",
                "store:AfterCanonicalMovedToBackup",
                "io:BeforeRootDirectoryFlush",
                "io:AfterRootDirectoryFlush",
                "store:AfterBackupMoveDurable",
                "io:BeforeHandleRename",
                "io:AfterHandleRename",
                "store:AfterTempPromotedToCanonical",
                "io:BeforeFileFlush",
                "io:AfterFileFlush",
                "io:BeforeRootDirectoryFlush",
                "io:AfterRootDirectoryFlush",
                "store:AfterCanonicalCommitDurable",
                "io:BeforeExactDelete",
                "io:AfterExactDelete",
                "store:AfterBackupDeleted",
                "io:BeforeRootDirectoryFlush",
                "io:AfterRootDirectoryFlush",
                "store:AfterBackupCleanupDurable",
            },
            stages);
    }

    [CanonicalSemanticCompilerCacheWindowsFact]
    public void FailureAfterTempDurable_IsNotCommittedAndDurableRollbackKeepsStoreUsable()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        var fail = true;
        using var cache = CanonicalSemanticCompilerCache.Open(
            access.AcquireCompilerCacheRootCapability(),
            new CanonicalSemanticCompilerCacheOptions
            {
                ObserveStageForTesting = stage =>
                {
                    if (fail && stage == CanonicalSemanticCompilerCacheStage.AfterTempFileDurable)
                    {
                        fail = false;
                        throw new InjectedFailureException();
                    }
                },
            });

        AssertDisposition(
            () => cache.Save(KeyA, OutputA()),
            CanonicalSemanticCompilerCachePublicationDisposition.NotCommitted);
        Assert.Null(cache.Load(KeyA));
        Assert.False(File.Exists(Path.Combine(fixture.CacheRoot, TempName)));
        cache.Save(KeyA, OutputA());
        Assert.NotNull(cache.Load(KeyA));
    }

    [CanonicalSemanticCompilerCacheWindowsFact]
    public void NativeTempFileFlushFailure_IsNotCommittedAndTempIsDurablyRemoved()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        var capability = access.AcquireCompilerCacheRootCapability(
            new ServerOwnedContinuousLearningCacheRootCapabilityOptions
            {
                NativeErrorForTesting = call =>
                    call == ServerOwnedContinuousLearningCacheNativeCall.FileFlush ? 5 : null,
            });
        using var cache = CanonicalSemanticCompilerCache.Open(capability);

        AssertDisposition(
            () => cache.Save(KeyA, OutputA()),
            CanonicalSemanticCompilerCachePublicationDisposition.NotCommitted);
        Assert.Null(cache.Load(KeyA));
        Assert.False(File.Exists(Path.Combine(fixture.CacheRoot, TempName)));
        Assert.False(File.Exists(Path.Combine(fixture.CacheRoot, CanonicalName)));
    }

    [CanonicalSemanticCompilerCacheWindowsFact]
    public void PrePromotionCleanupFlushFailure_SealsAsNotCommittedCleanupRequired()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        var capability = access.AcquireCompilerCacheRootCapability(
            new ServerOwnedContinuousLearningCacheRootCapabilityOptions
            {
                NativeErrorForTesting = call =>
                    call == ServerOwnedContinuousLearningCacheNativeCall.RootDirectoryFlush ? 5 : null,
            });
        using var cache = CanonicalSemanticCompilerCache.Open(
            capability,
            new CanonicalSemanticCompilerCacheOptions
            {
                ObserveStageForTesting = stage =>
                {
                    if (stage == CanonicalSemanticCompilerCacheStage.AfterTempFileDurable)
                        throw new InjectedFailureException();
                },
            });

        AssertDisposition(
            () => cache.Save(KeyA, OutputA()),
            CanonicalSemanticCompilerCachePublicationDisposition.NotCommittedCleanupRequired);
        Assert.Throws<InvalidOperationException>(() => cache.Load(KeyA));
    }

    [CanonicalSemanticCompilerCacheWindowsFact]
    public void BackupDurabilityFailure_RollsBackAsNotCommitted()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        var armed = false;
        var failures = 0;
        var capability = access.AcquireCompilerCacheRootCapability(
            new ServerOwnedContinuousLearningCacheRootCapabilityOptions
            {
                NativeErrorForTesting = call =>
                    armed && call == ServerOwnedContinuousLearningCacheNativeCall.RootDirectoryFlush &&
                    Interlocked.Increment(ref failures) == 1
                        ? 5
                        : null,
            });
        using var cache = CanonicalSemanticCompilerCache.Open(capability);
        cache.Save(KeyA, OutputA());
        armed = true;

        AssertDisposition(
            () => cache.Save(KeyB, OutputB()),
            CanonicalSemanticCompilerCachePublicationDisposition.NotCommitted);
        Assert.NotNull(cache.Load(KeyA));
        Assert.Null(cache.Load(KeyB));
        Assert.False(File.Exists(Path.Combine(fixture.CacheRoot, TempName)));
        Assert.False(File.Exists(Path.Combine(fixture.CacheRoot, BackupName)));
    }

    [CanonicalSemanticCompilerCacheWindowsFact]
    public void PromotionCallFailure_IsCommitOutcomeUnknownAndRestartDiscardsTemp()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using (var access = fixture.CreateStartup().AcquireRetainedFileAccess())
        {
            var capability = access.AcquireCompilerCacheRootCapability(
                new ServerOwnedContinuousLearningCacheRootCapabilityOptions
                {
                    ObserveIoStageForTesting = stage =>
                    {
                        if (stage == ServerOwnedContinuousLearningCacheIoStage.BeforeHandleRename)
                            throw new InjectedFailureException();
                    },
                });
            using var cache = CanonicalSemanticCompilerCache.Open(capability);
            AssertDisposition(
                () => cache.Save(KeyA, OutputA()),
                CanonicalSemanticCompilerCachePublicationDisposition.CommitOutcomeUnknown);
            Assert.Throws<InvalidOperationException>(() => cache.Load(KeyA));
        }

        using var restartedAccess = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var restarted = CanonicalSemanticCompilerCache.Open(
            restartedAccess.AcquireCompilerCacheRootCapability());
        Assert.Null(restarted.Load(KeyA));
        Assert.False(File.Exists(Path.Combine(fixture.CacheRoot, TempName)));
    }

    [CanonicalSemanticCompilerCacheWindowsFact]
    public void AfterNativeCommitFlushObserverFailure_IsUnknownAndRestartRollsForward()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using (var access = fixture.CreateStartup().AcquireRetainedFileAccess())
        {
            var capability = access.AcquireCompilerCacheRootCapability(
                new ServerOwnedContinuousLearningCacheRootCapabilityOptions
                {
                    ObserveIoStageForTesting = stage =>
                    {
                        if (stage == ServerOwnedContinuousLearningCacheIoStage.AfterRootDirectoryFlush)
                            throw new InjectedFailureException();
                    },
                });
            using var cache = CanonicalSemanticCompilerCache.Open(capability);
            AssertDisposition(
                () => cache.Save(KeyA, OutputA()),
                CanonicalSemanticCompilerCachePublicationDisposition.CommitOutcomeUnknown);
        }

        using var restartedAccess = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var restarted = CanonicalSemanticCompilerCache.Open(
            restartedAccess.AcquireCompilerCacheRootCapability());
        Assert.NotNull(restarted.Load(KeyA));
    }

    [CanonicalSemanticCompilerCacheWindowsFact]
    public void NativeCommitRootFlushFailure_IsUnknownAndRestartRollsForward()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using (var access = fixture.CreateStartup().AcquireRetainedFileAccess())
        {
            var capability = access.AcquireCompilerCacheRootCapability(
                new ServerOwnedContinuousLearningCacheRootCapabilityOptions
                {
                    NativeErrorForTesting = call =>
                        call == ServerOwnedContinuousLearningCacheNativeCall.RootDirectoryFlush ? 5 : null,
                });
            using var cache = CanonicalSemanticCompilerCache.Open(capability);
            AssertDisposition(
                () => cache.Save(KeyA, OutputA()),
                CanonicalSemanticCompilerCachePublicationDisposition.CommitOutcomeUnknown);
        }

        using var restartedAccess = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var restarted = CanonicalSemanticCompilerCache.Open(
            restartedAccess.AcquireCompilerCacheRootCapability());
        Assert.NotNull(restarted.Load(KeyA));
    }

    [CanonicalSemanticCompilerCacheWindowsFact]
    public void NativeCleanupRootFlushFailure_IsCommittedAndRestartReconciles()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using (var access = fixture.CreateStartup().AcquireRetainedFileAccess())
        {
            var armed = false;
            var rootFlush = 0;
            var capability = access.AcquireCompilerCacheRootCapability(
                new ServerOwnedContinuousLearningCacheRootCapabilityOptions
                {
                    NativeErrorForTesting = call =>
                        armed && call == ServerOwnedContinuousLearningCacheNativeCall.RootDirectoryFlush &&
                        Interlocked.Increment(ref rootFlush) == 3
                            ? 5
                            : null,
                });
            using var cache = CanonicalSemanticCompilerCache.Open(capability);
            cache.Save(KeyA, OutputA());
            armed = true;
            AssertDisposition(
                () => cache.Save(KeyB, OutputB()),
                CanonicalSemanticCompilerCachePublicationDisposition.CommittedCleanupRequired);
        }

        using var restartedAccess = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var restarted = CanonicalSemanticCompilerCache.Open(
            restartedAccess.AcquireCompilerCacheRootCapability());
        Assert.NotNull(restarted.Load(KeyA));
        Assert.NotNull(restarted.Load(KeyB));
        Assert.False(File.Exists(Path.Combine(fixture.CacheRoot, BackupName)));
    }

    [CanonicalSemanticCompilerCacheWindowsTheory]
    [InlineData((int)CanonicalSemanticCompilerCacheStage.AfterCanonicalCommitDurable)]
    [InlineData((int)CanonicalSemanticCompilerCacheStage.AfterBackupDeleted)]
    public void NonterminalCommitOrUnflushedDeleteObserverFailure_RequiresCleanup(
        int stageValue)
    {
        RequireWindows();
        var failStage = (CanonicalSemanticCompilerCacheStage)stageValue;
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using (var access = fixture.CreateStartup().AcquireRetainedFileAccess())
        {
            var armed = false;
            using var cache = CanonicalSemanticCompilerCache.Open(
                access.AcquireCompilerCacheRootCapability(),
                new CanonicalSemanticCompilerCacheOptions
                {
                    ObserveStageForTesting = stage =>
                    {
                        if (armed && stage == failStage)
                            throw new InjectedFailureException();
                    },
                });
            cache.Save(KeyA, OutputA());
            armed = true;
            AssertDisposition(
                () => cache.Save(KeyB, OutputB()),
                CanonicalSemanticCompilerCachePublicationDisposition.CommittedCleanupRequired);
            Assert.Throws<InvalidOperationException>(() => cache.Load(KeyB));
        }

        using var restartedAccess = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var restarted = CanonicalSemanticCompilerCache.Open(
            restartedAccess.AcquireCompilerCacheRootCapability());
        Assert.NotNull(restarted.Load(KeyA));
        Assert.NotNull(restarted.Load(KeyB));
        Assert.False(File.Exists(Path.Combine(fixture.CacheRoot, BackupName)));
    }

    [CanonicalSemanticCompilerCacheWindowsTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void TerminalSuccessObserverFailure_PropagatesRawWithoutSealingOrRelabeling(
        bool afterBackupCleanup)
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using (var access = fixture.CreateStartup().AcquireRetainedFileAccess())
        {
            var armed = false;
            var terminalStage = afterBackupCleanup
                ? CanonicalSemanticCompilerCacheStage.AfterBackupCleanupDurable
                : CanonicalSemanticCompilerCacheStage.AfterCanonicalCommitDurable;
            using var cache = CanonicalSemanticCompilerCache.Open(
                access.AcquireCompilerCacheRootCapability(),
                new CanonicalSemanticCompilerCacheOptions
                {
                    ObserveStageForTesting = stage =>
                    {
                        if (!armed || stage != terminalStage)
                            return;
                        armed = false;
                        throw new InjectedFailureException();
                    },
                });
            if (afterBackupCleanup)
                cache.Save(KeyA, OutputA());
            armed = true;

            Assert.Throws<InjectedFailureException>(() => cache.Save(
                afterBackupCleanup ? KeyB : KeyA,
                afterBackupCleanup ? OutputB() : OutputA()));
            Assert.NotNull(cache.Load(KeyA));
            Assert.Equal(afterBackupCleanup, cache.Load(KeyB) is not null);
            Assert.False(File.Exists(Path.Combine(fixture.CacheRoot, BackupName)));
            cache.Save(KeyC, OutputB());
            Assert.NotNull(cache.Load(KeyC));
        }

        using var restartedAccess = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var restarted = CanonicalSemanticCompilerCache.Open(
            restartedAccess.AcquireCompilerCacheRootCapability());
        Assert.NotNull(restarted.Load(KeyA));
        Assert.Equal(afterBackupCleanup, restarted.Load(KeyB) is not null);
        Assert.NotNull(restarted.Load(KeyC));
        Assert.False(File.Exists(Path.Combine(fixture.CacheRoot, BackupName)));
    }

    [CanonicalSemanticCompilerCacheWindowsFact]
    public void AuditLeaf_IsIdentityAndByteStableAndNeverPartOfCachePublication()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        var auditBytes = new byte[] { 0xA0, 0xB1, 0xC2 };
        File.WriteAllBytes(Path.Combine(fixture.CacheRoot, AuditName), auditBytes);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        var capability = access.AcquireCompilerCacheRootCapability();
        var before = capability.Inventory(5)
            .Single(entry => entry.Leaf == ServerOwnedContinuousLearningCacheLeaf.AuditLog);
        using var cache = CanonicalSemanticCompilerCache.Open(capability);

        cache.Save(KeyA, OutputA());
        cache.Save(KeyB, OutputB());

        var after = capability.Inventory(5)
            .Single(entry => entry.Leaf == ServerOwnedContinuousLearningCacheLeaf.AuditLog);
        Assert.Equal(before.Identity, after.Identity);
        Assert.Equal(before.Length, after.Length);
        Assert.Equal(auditBytes, File.ReadAllBytes(Path.Combine(fixture.CacheRoot, AuditName)));
    }

    [CanonicalSemanticCompilerCacheWindowsFact]
    public async Task ConcurrentSaves_AreSerializedWithoutLostUpdates()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var cache = CanonicalSemanticCompilerCache.Open(
            access.AcquireCompilerCacheRootCapability());
        var keys = Enumerable.Range(1, 24)
            .Select(index => index.ToString("x64"))
            .ToArray();

        await Task.WhenAll(keys.Select(key => Task.Run(() => cache.Save(key, OutputB()))));

        foreach (var key in keys)
            Assert.NotNull(cache.Load(key));
        Assert.False(File.Exists(Path.Combine(fixture.CacheRoot, TempName)));
        Assert.False(File.Exists(Path.Combine(fixture.CacheRoot, BackupName)));
    }

    [CanonicalSemanticCompilerCacheWindowsFact]
    public void OwnerExclusionDisposeRevocationAndFailedOpenHaveBoundedLifetimes()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using (var firstAccess = fixture.CreateStartup().AcquireRetainedFileAccess())
        using (var cache = CanonicalSemanticCompilerCache.Open(
            firstAccess.AcquireCompilerCacheRootCapability()))
        {
            // While the first parent owner is alive, a second parent owner must be
            // rejected at the admission boundary instead of reaching cache-lock code.
            var owned = Assert.Throws<ContinuousLearningMutationAdmissionDomainAlreadyOwnedException>(
                () => fixture.CreateStartup().AcquireRetainedFileAccess());
            Assert.Equal(
                ContinuousLearningMutationAdmissionDomainAlreadyOwnedException.StableErrorCode,
                owned.Message);

            firstAccess.Dispose();
            Assert.Throws<ContinuousLearningPathIdentityException>(() => cache.Load(KeyA));
            Assert.Throws<InvalidOperationException>(() => cache.Save(KeyA, OutputA()));
        }

        // Once both the parent owner and cache have ended, the next parent and
        // its one-shot child lock can be acquired again in a clean lifetime.
        using var retryAccess = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var retryCapability = retryAccess.AcquireCompilerCacheRootCapability();
    }

    [CanonicalSemanticCompilerCacheWindowsFact]
    public void CanonicalDiskDrift_SealsStoreBeforePublication()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var cache = CanonicalSemanticCompilerCache.Open(
            access.AcquireCompilerCacheRootCapability());
        cache.Save(KeyA, OutputA());
        File.WriteAllBytes(Path.Combine(fixture.CacheRoot, CanonicalName), [1, 2, 3]);

        Assert.ThrowsAny<Exception>(() => cache.Save(KeyB, OutputB()));
        Assert.Throws<InvalidOperationException>(() => cache.Load(KeyA));
        Assert.False(File.Exists(Path.Combine(fixture.CacheRoot, TempName)));
    }

    [CanonicalSemanticCompilerCacheWindowsFact]
    public void IdempotentSave_CanonicalImageDriftFailsClosedBeforeEarlyReturn()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var cache = CanonicalSemanticCompilerCache.Open(
            access.AcquireCompilerCacheRootCapability());
        cache.Save(KeyA, OutputA());
        var corruption = new byte[] { 1, 2, 3 };
        var canonicalPath = Path.Combine(fixture.CacheRoot, CanonicalName);
        File.WriteAllBytes(canonicalPath, corruption);
        Assert.Equal(corruption, File.ReadAllBytes(canonicalPath));
        var before = ProtocolSnapshot(fixture.CacheRoot);

        Assert.ThrowsAny<Exception>(() => cache.Save(KeyA, OutputA()));

        Assert.Equal(before, ProtocolSnapshot(fixture.CacheRoot));
        Assert.Throws<InvalidOperationException>(() => cache.Load(KeyA));
        Assert.Throws<InvalidOperationException>(() => cache.Save(KeyB, OutputB()));
    }

    [CanonicalSemanticCompilerCacheWindowsTheory]
    [InlineData(TempName)]
    [InlineData(BackupName)]
    public void IdempotentSave_RecognizedNamespaceDriftFailsClosedBeforeEarlyReturn(
        string leafName)
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var cache = CanonicalSemanticCompilerCache.Open(
            access.AcquireCompilerCacheRootCapability());
        cache.Save(KeyA, OutputA());
        var drift = new byte[] { 0xd1, 0xf7 };
        var driftPath = Path.Combine(fixture.CacheRoot, leafName);
        File.WriteAllBytes(driftPath, drift);
        Assert.Equal(drift, File.ReadAllBytes(driftPath));
        var before = ProtocolSnapshot(fixture.CacheRoot);

        Assert.Throws<InvalidDataException>(() => cache.Save(KeyA, OutputA()));

        Assert.Equal(before, ProtocolSnapshot(fixture.CacheRoot));
        Assert.Throws<InvalidOperationException>(() => cache.Load(KeyA));
        Assert.Throws<InvalidOperationException>(() => cache.Save(KeyB, OutputB()));
    }

    private static PipelineOutput OutputA()
        => new(Domain.BUG, ["NULL_CHECK", "RETURN_OP"], "csharp", 2.5);

    private static PipelineOutput OutputB()
        => new(Domain.PLAN, ["CALL_OP", "VALIDATE_OP"], "unknown", 1.25);

    private static byte[] CreateImage(params (string Key, PipelineOutput Output)[] entries)
    {
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var cache = CanonicalSemanticCompilerCache.Open(
            access.AcquireCompilerCacheRootCapability());
        foreach (var entry in entries)
            cache.Save(entry.Key, entry.Output);
        return File.ReadAllBytes(Path.Combine(fixture.CacheRoot, CanonicalName));
    }

    private static byte[] Reframe(byte[] original, byte[] payload)
    {
        var framed = new byte[64 + payload.Length];
        original.AsSpan(0, 64).CopyTo(framed);
        BinaryPrimitives.WriteUInt32LittleEndian(
            framed.AsSpan(12, 4),
            checked((uint)payload.Length));
        SHA256.HashData(payload).CopyTo(framed, 32);
        payload.CopyTo(framed, 64);
        return framed;
    }

    private static string[] ProtocolSnapshot(string root)
        => new[] { CanonicalName, TempName, BackupName, AuditName }
            .Select(name =>
            {
                var path = Path.Combine(root, name);
                return File.Exists(path)
                    ? $"{name}|{Convert.ToHexString(File.ReadAllBytes(path))}"
                    : $"{name}|missing";
            })
            .ToArray();

    private static SafeFileHandle GetCompilerCacheOwnerLockHandle(
        ServerOwnedContinuousLearningCacheRootCapability capability)
    {
        var field = typeof(ServerOwnedContinuousLearningCacheRootCapability)
            .GetField("_ownerLockHandle", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingFieldException(
                nameof(ServerOwnedContinuousLearningCacheRootCapability),
                "_ownerLockHandle");
        return Assert.IsType<SafeFileHandle>(field.GetValue(capability));
    }

    private static void AssertDisposition(
        Action action,
        CanonicalSemanticCompilerCachePublicationDisposition expected)
    {
        var exception = Assert.Throws<CanonicalSemanticCompilerCachePublicationException>(action);
        Assert.Equal(expected, exception.Disposition);
    }

    private static void RequireWindows()
        => Assert.True(
            OperatingSystem.IsWindows(),
            "R1.3B canonical semantic-compiler cache tests require Windows execution.");

    private sealed class InjectedFailureException : IOException;
}

file sealed class CanonicalSemanticCompilerCacheWindowsFactAttribute : FactAttribute
{
    public CanonicalSemanticCompilerCacheWindowsFactAttribute()
    {
        if (!OperatingSystem.IsWindows())
        {
            // Эти проверки доказывают Windows-опеку над файловой личностью.
            // Linux-запуск должен видеть честный пропуск платформы, а не
            // красный Assert внутри тела теста.
            Skip = "WINDOWS_ONLY_CAPABILITY_ORACLE: R1.3B canonical semantic-compiler cache tests require Windows execution.";
        }
    }
}

file sealed class CanonicalSemanticCompilerCacheWindowsTheoryAttribute : TheoryAttribute
{
    public CanonicalSemanticCompilerCacheWindowsTheoryAttribute()
    {
        if (!OperatingSystem.IsWindows())
        {
            // Каждая строка theory зависит от той же Windows-опеки cache.
            // Поэтому вне Windows весь набор строк должен стать NotExecuted
            // уже при обнаружении тестов.
            Skip = "WINDOWS_ONLY_CAPABILITY_ORACLE: R1.3B canonical semantic-compiler cache tests require Windows execution.";
        }
    }
}
