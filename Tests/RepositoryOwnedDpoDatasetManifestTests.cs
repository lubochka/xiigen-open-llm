// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

using System.Diagnostics;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using DpoPairSideAuthorKind = DPO.Models.DpoPairSideAuthorKind;

namespace LlmFromScratch.Tests;

public sealed class RepositoryOwnedDpoDatasetManifestTests
{
    private readonly Xunit.Abstractions.ITestOutputHelper _output;

    public RepositoryOwnedDpoDatasetManifestTests(
        Xunit.Abstractions.ITestOutputHelper output)
    {
        _output = output;
    }

    private static readonly string ManifestDigest = new('a', 64);
    private static readonly string LayerDigest = new('1', 64);
    private const string CrossProcessDpoResolverDomainSlug =
        "cross-process-dpo";
    private const string CrossProcessDpoResolverDesignPatternSlug =
        "resolver-reuse";
    private const string CrossProcessDpoResolverDatasetVersion = "v0001";
    private const string CrossProcessDpoResolverChildModeEnvironmentName =
        "XIIGEN_REPOSITORY_OWNED_DPO_RESOLVER_VSTEST_CHILD_MODE";
    private const string CrossProcessDpoResolverFixtureRootEnvironmentName =
        "XIIGEN_REPOSITORY_OWNED_DPO_RESOLVER_VSTEST_FIXTURE_ROOT";
    private const string CrossProcessDpoResolverRepositoryRootEnvironmentName =
        "XIIGEN_REPOSITORY_OWNED_DPO_RESOLVER_VSTEST_REPOSITORY_ROOT";
    private const string CrossProcessDpoResolverDomainEnvironmentName =
        "XIIGEN_REPOSITORY_OWNED_DPO_RESOLVER_VSTEST_DOMAIN";
    private const string CrossProcessDpoResolverPatternEnvironmentName =
        "XIIGEN_REPOSITORY_OWNED_DPO_RESOLVER_VSTEST_PATTERN";
    private const string CrossProcessDpoResolverVersionEnvironmentName =
        "XIIGEN_REPOSITORY_OWNED_DPO_RESOLVER_VSTEST_VERSION";
    private const string CrossProcessDpoResolverScenarioEnvironmentName =
        "XIIGEN_REPOSITORY_OWNED_DPO_RESOLVER_VSTEST_SCENARIO";
    private const string CrossProcessDpoResolverIdentityEnvironmentName =
        "XIIGEN_REPOSITORY_OWNED_DPO_RESOLVER_VSTEST_IDENTITY_JSON64";
    private const string CrossProcessDpoResolverCoordinationEnvironmentName =
        "XIIGEN_REPOSITORY_OWNED_DPO_RESOLVER_VSTEST_COORDINATION";
    private const string CrossProcessDpoResolverResultEnvironmentName =
        "XIIGEN_REPOSITORY_OWNED_DPO_RESOLVER_VSTEST_RESULT";
    private const string CrossProcessDpoResolverConcurrentScenario =
        "concurrent-create-or-reuse";
    private const string CrossProcessDpoResolverReuseScenario =
        "reuse-existing";
    private const string CrossProcessDpoResolverIdentityMismatchScenario =
        "identity-mismatch";
    private const string CrossProcessDpoResolverReleaseFileName =
        "release-create";
    private static readonly string[] CrossProcessDpoResolverChildEnvironmentNames =
    [
        CrossProcessDpoResolverChildModeEnvironmentName,
        CrossProcessDpoResolverFixtureRootEnvironmentName,
        CrossProcessDpoResolverRepositoryRootEnvironmentName,
        CrossProcessDpoResolverDomainEnvironmentName,
        CrossProcessDpoResolverPatternEnvironmentName,
        CrossProcessDpoResolverVersionEnvironmentName,
        CrossProcessDpoResolverScenarioEnvironmentName,
        CrossProcessDpoResolverIdentityEnvironmentName,
        CrossProcessDpoResolverCoordinationEnvironmentName,
        CrossProcessDpoResolverResultEnvironmentName,
    ];

    [Fact]
    public void PersistAndSeal_PersistsTwoRichPairsAndFreshReaderReopensExactFieldsAndManifest()
    {
        using var fixture = new RepositoryOwnedDpoDatasetManifestFixture();
        var address = fixture.Address();
        var first = BoundLocalPair(
            "manifest-first",
            address.DomainSlug,
            "prompt first",
            "chosen first",
            "rejected first",
            DateTimeOffset.UnixEpoch);
        var second = BoundOpenTeacherRejectedPair(
            "manifest-second",
            address.DomainSlug,
            "prompt second",
            "chosen second",
            "rejected second",
            DateTimeOffset.UnixEpoch.AddSeconds(7));

        var sealedManifest = AssertSuccess(
            PersistAndSeal(
                address,
                [second, first]));
        var reopened = AssertSuccess(
            OpenSealed(address));
        var idempotent = AssertSuccess(
            PersistAndSeal(
                address,
                [second, first]));

        Assert.Equal(sealedManifest.ManifestSha256, idempotent.ManifestSha256);
        Assert.Equal(
            RepositoryOwnedDpoDatasetManifestSealer.CurrentSchema,
            reopened.Manifest.Schema);
        Assert.Equal(address.DomainSlug, reopened.Manifest.Domain);
        Assert.Equal(address.DesignPatternSlug, reopened.Manifest.DesignPattern);
        Assert.Equal(address.DatasetVersion, reopened.Manifest.DatasetVersion);
        Assert.Equal(2, reopened.Manifest.PairCount);
        Assert.Equal(
            reopened.Manifest.OrderedPairs.OrderBy(
                pair => pair.PairId,
                StringComparer.Ordinal),
            reopened.Manifest.OrderedPairs);

        var expectedById = new Dictionary<string, DpoPair>(StringComparer.Ordinal)
        {
            [PairId(first)] = first,
            [PairId(second)] = second,
        };
        Assert.Equal(
            reopened.Manifest.OrderedPairs.Select(static pair => pair.PairId),
            reopened.Pairs.Select(PairId));
        foreach (var pair in reopened.Pairs)
        {
            var expected = expectedById[PairId(pair)];
            Assert.Equal(expected, pair);
            Assert.Equal(expected.TeacherProvenance, pair.TeacherProvenance);
            Assert.Equal(expected.GovernanceDecision, pair.GovernanceDecision);
            Assert.Equal(expected.ChosenAuthorship, pair.ChosenAuthorship);
            Assert.Equal(expected.RejectedAuthorship, pair.RejectedAuthorship);
        }

        var pairFiles = Directory.GetFiles(
            address.PairsRoot,
            "*.dpo-pair.json",
            SearchOption.TopDirectoryOnly);
        Assert.Equal(2, pairFiles.Length);
        foreach (var manifestPair in reopened.Manifest.OrderedPairs)
        {
            var path = Path.Combine(
                address.RepositoryRoot,
                manifestPair.RepositoryRelativePairFilePath.Replace(
                    '/',
                    Path.DirectorySeparatorChar));
            Assert.True(File.Exists(path), path);
            Assert.Equal(ReadSha256(path), manifestPair.PairFileSha256);
        }

        var manifestJson = JsonNode.Parse(File.ReadAllText(address.ManifestPath))!
            .AsObject();
        Assert.Equal(
            reopened.Manifest.ManifestSha256,
            manifestJson["manifestSha256"]!.GetValue<string>());
        manifestJson["manifestSha256"] = string.Empty;
        Assert.Equal(
            reopened.Manifest.ManifestSha256,
            Sha256(CanonicalManifestJson(reopened.Manifest with
            {
                ManifestSha256 = string.Empty,
            })));
        Assert.Equal(
            reopened.Manifest.ManifestSha256,
            Sha256(Encoding.UTF8.GetBytes(manifestJson.ToJsonString(
                new JsonSerializerOptions { WriteIndented = false }))));
    }

    [Fact]
    public async Task PersistAndSeal_ConcurrentSameDatasetIsIdempotentAndOpensSealed()
    {
        using var fixture = new RepositoryOwnedDpoDatasetManifestFixture();
        var address = fixture.Address();
        var first = BoundLocalPair(
            "concurrent-same-first",
            address.DomainSlug,
            "same prompt first",
            "same chosen first",
            "same rejected first",
            DateTimeOffset.UnixEpoch);
        var second = BoundOpenTeacherRejectedPair(
            "concurrent-same-second",
            address.DomainSlug,
            "same prompt second",
            "same chosen second",
            "same rejected second",
            DateTimeOffset.UnixEpoch.AddSeconds(13));

        using var firstInsideLease = new ManualResetEventSlim(false);
        using var secondStarted = new ManualResetEventSlim(false);
        using var releaseFirst = new ManualResetEventSlim(false);
        var hookCalls = 0;
        RepositoryOwnedDpoDatasetManifestSealer
            .AfterDatasetLeaseAcquiredForTesting = () =>
            {
                if (Interlocked.Increment(ref hookCalls) != 1)
                    return;

                firstInsideLease.Set();
                if (!secondStarted.Wait(TimeSpan.FromSeconds(5)))
                    throw new TimeoutException("Second manifest writer did not start.");
                if (!releaseFirst.Wait(TimeSpan.FromSeconds(5)))
                    throw new TimeoutException("First manifest writer was not released.");
            };

        try
        {
            var firstTask = Task.Run(() =>
                PersistAndSeal(
                    address,
                    [second, first]));
            Assert.True(
                firstInsideLease.Wait(TimeSpan.FromSeconds(5)),
                "First writer did not acquire the dataset lease.");

            var secondTask = Task.Run(() =>
            {
                secondStarted.Set();
                return PersistAndSeal(
                    address,
                    [first, second]);
            });
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            Assert.False(
                secondTask.IsCompleted,
                "Second writer completed while the first writer held the dataset lease.");

            releaseFirst.Set();
            var results = await AssertCompletesWithin(
                Task.WhenAll(firstTask, secondTask),
                TimeSpan.FromSeconds(10));
            var firstManifest = AssertSuccess(results[0]);
            var secondManifest = AssertSuccess(results[1]);

            Assert.Equal(firstManifest.ManifestSha256, secondManifest.ManifestSha256);
            Assert.Equal(2, hookCalls);
        }
        finally
        {
            releaseFirst.Set();
            RepositoryOwnedDpoDatasetManifestSealer
                .AfterDatasetLeaseAcquiredForTesting = null;
        }

        var reopened = AssertSuccess(
            OpenSealed(address));
        Assert.Equal(2, reopened.Manifest.PairCount);
        Assert.Equal(
            reopened.Manifest.OrderedPairs.Select(static pair => pair.PairId),
            reopened.Pairs.Select(PairId));
        AssertExactPairFiles(address, [first, second]);
        AssertNoTemporaryFiles(address);
    }

    [Fact]
    public async Task PersistAndSeal_ConcurrentDifferentDatasetsAtSameAddressKeepsWinnerValidAndLoserDoesNotMutate()
    {
        using var fixture = new RepositoryOwnedDpoDatasetManifestFixture();
        var address = fixture.Address();
        var winnerFirst = BoundLocalPair(
            "concurrent-winner-first",
            address.DomainSlug,
            "winner prompt first",
            "winner chosen first",
            "winner rejected first",
            DateTimeOffset.UnixEpoch);
        var winnerSecond = BoundOpenTeacherRejectedPair(
            "concurrent-winner-second",
            address.DomainSlug,
            "winner prompt second",
            "winner chosen second",
            "winner rejected second",
            DateTimeOffset.UnixEpoch.AddSeconds(19));
        var loser = BoundLocalPair(
            "concurrent-loser",
            address.DomainSlug,
            "loser prompt",
            "loser chosen",
            "loser rejected",
            DateTimeOffset.UnixEpoch.AddSeconds(29));
        var loserId = PairId(loser);

        using var firstInsideLease = new ManualResetEventSlim(false);
        using var secondStarted = new ManualResetEventSlim(false);
        using var releaseFirst = new ManualResetEventSlim(false);
        var hookCalls = 0;
        RepositoryOwnedDpoDatasetManifestSealer
            .AfterDatasetLeaseAcquiredForTesting = () =>
            {
                if (Interlocked.Increment(ref hookCalls) != 1)
                    return;

                firstInsideLease.Set();
                if (!secondStarted.Wait(TimeSpan.FromSeconds(5)))
                    throw new TimeoutException("Second manifest writer did not start.");
                if (!releaseFirst.Wait(TimeSpan.FromSeconds(5)))
                    throw new TimeoutException("First manifest writer was not released.");
            };

        try
        {
            var winnerTask = Task.Run(() =>
                PersistAndSeal(
                    address,
                    [winnerSecond, winnerFirst]));
            Assert.True(
                firstInsideLease.Wait(TimeSpan.FromSeconds(5)),
                "Winner writer did not acquire the dataset lease.");

            var loserTask = Task.Run(() =>
            {
                secondStarted.Set();
                return PersistAndSeal(
                    address,
                    [loser]);
            });
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            Assert.False(
                loserTask.IsCompleted,
                "Loser writer completed while the winner writer held the dataset lease.");

            releaseFirst.Set();
            var results = await AssertCompletesWithin(
                Task.WhenAll(winnerTask, loserTask),
                TimeSpan.FromSeconds(10));

            var success = Assert.Single(results, static result => result.IsSuccess);
            var conflict = Assert.Single(results, static result => !result.IsSuccess);
            Assert.Equal(
                RepositoryOwnedDpoDatasetManifestSealer.ConflictErrorCode,
                conflict.ErrorCode);
            Assert.Equal(2, success.Value!.PairCount);
            Assert.Equal(2, hookCalls);
        }
        finally
        {
            releaseFirst.Set();
            RepositoryOwnedDpoDatasetManifestSealer
                .AfterDatasetLeaseAcquiredForTesting = null;
        }

        var reopened = AssertSuccess(
            OpenSealed(address));
        Assert.Equal(2, reopened.Manifest.PairCount);
        Assert.DoesNotContain(
            loserId,
            reopened.Manifest.OrderedPairs.Select(static pair => pair.PairId));
        Assert.DoesNotContain(loserId, reopened.Pairs.Select(PairId));
        AssertExactPairFiles(address, [winnerFirst, winnerSecond]);
        Assert.False(File.Exists(Path.Combine(
            address.PairsRoot,
            loserId + ".dpo-pair.json")));
        AssertNoTemporaryFiles(address);
    }

    [Fact]
    public void PersistAndSeal_ExistingInvalidDatasetLockFailsClosedWithoutPairMutation()
    {
        using var fixture = new RepositoryOwnedDpoDatasetManifestFixture();
        var address = fixture.Address();
        var pair = BoundLocalPair(
            "invalid-dataset-lock",
            address.DomainSlug,
            "prompt",
            "chosen",
            "rejected",
            DateTimeOffset.UnixEpoch);
        Directory.CreateDirectory(address.DatasetRoot);
        File.WriteAllBytes(
            Path.Combine(address.DatasetRoot, ".dpo-dataset-seal.lock"),
            [1, 2]);

        var result = PersistAndSeal(
            address,
            [pair]);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            RepositoryOwnedDpoDatasetManifestSealer.UnavailableErrorCode,
            result.ErrorCode);
        Assert.False(File.Exists(address.ManifestPath));
        AssertExactPairFiles(address, []);
        AssertNoTemporaryFiles(address);
    }

    [Fact]
    public void PersistAndSeal_DifferentPairsAgainstExistingManifestFailsWithoutMutatingSealedDataset()
    {
        using var fixture = new RepositoryOwnedDpoDatasetManifestFixture();
        var address = fixture.Address();
        var existing = BoundLocalPair(
            "conflict-existing",
            address.DomainSlug,
            "existing prompt",
            "existing chosen",
            "existing rejected",
            DateTimeOffset.UnixEpoch);
        var replacement = BoundLocalPair(
            "conflict-replacement",
            address.DomainSlug,
            "replacement prompt",
            "replacement chosen",
            "replacement rejected",
            DateTimeOffset.UnixEpoch.AddSeconds(11));

        var sealedManifest = AssertSuccess(
            PersistAndSeal(
                address,
                [existing]));
        var manifestBytes = File.ReadAllBytes(address.ManifestPath);
        var manifestLastWriteTimeUtc = File.GetLastWriteTimeUtc(
            address.ManifestPath);
        var pairSnapshots = SnapshotPairFiles(address);
        var replacementId = PairId(replacement);
        Assert.NotEqual(PairId(existing), replacementId);

        var conflict = PersistAndSeal(
            address,
            [replacement]);

        Assert.False(conflict.IsSuccess);
        Assert.Equal(
            RepositoryOwnedDpoDatasetManifestSealer.ConflictErrorCode,
            conflict.ErrorCode);
        Assert.Equal(manifestBytes, File.ReadAllBytes(address.ManifestPath));
        Assert.Equal(
            manifestLastWriteTimeUtc,
            File.GetLastWriteTimeUtc(address.ManifestPath));
        AssertPairFileSnapshotsUnchanged(address, pairSnapshots);
        Assert.False(File.Exists(Path.Combine(
            address.PairsRoot,
            replacementId + ".dpo-pair.json")));

        var reopened = AssertSuccess(
            OpenSealed(address));
        Assert.Equal(sealedManifest.ManifestSha256, reopened.Manifest.ManifestSha256);
        Assert.Equal(1, reopened.Manifest.PairCount);
        var reopenedPair = Assert.Single(reopened.Pairs);
        Assert.Equal(existing, reopenedPair);
        Assert.Equal(PairId(existing), reopened.Manifest.OrderedPairs[0].PairId);
        Assert.DoesNotContain(
            replacementId,
            reopened.Manifest.OrderedPairs.Select(static pair => pair.PairId));
    }

    [Fact]
    public void PersistAndSeal_SamePairsInReverseOrderAgainstExistingManifestIsIdempotentWithoutMutatingFiles()
    {
        using var fixture = new RepositoryOwnedDpoDatasetManifestFixture();
        var address = fixture.Address();
        var first = BoundLocalPair(
            "idempotent-first",
            address.DomainSlug,
            "first prompt",
            "first chosen",
            "first rejected",
            DateTimeOffset.UnixEpoch);
        var second = BoundOpenTeacherRejectedPair(
            "idempotent-second",
            address.DomainSlug,
            "second prompt",
            "second chosen",
            "second rejected",
            DateTimeOffset.UnixEpoch.AddSeconds(17));

        var sealedManifest = AssertSuccess(
            PersistAndSeal(
                address,
                [first, second]));
        var manifestBytes = File.ReadAllBytes(address.ManifestPath);
        var manifestLastWriteTimeUtc = File.GetLastWriteTimeUtc(
            address.ManifestPath);
        var pairSnapshots = SnapshotPairFiles(address);

        var idempotent = AssertSuccess(
            PersistAndSeal(
                address,
                [second, first]));

        Assert.Equal(sealedManifest.ManifestSha256, idempotent.ManifestSha256);
        Assert.Equal(manifestBytes, File.ReadAllBytes(address.ManifestPath));
        Assert.Equal(
            manifestLastWriteTimeUtc,
            File.GetLastWriteTimeUtc(address.ManifestPath));
        AssertPairFileSnapshotsUnchanged(address, pairSnapshots);

        var reopened = AssertSuccess(
            OpenSealed(address));
        Assert.Equal(sealedManifest.ManifestSha256, reopened.Manifest.ManifestSha256);
        var expectedById = new Dictionary<string, DpoPair>(StringComparer.Ordinal)
        {
            [PairId(first)] = first,
            [PairId(second)] = second,
        };
        Assert.Equal(
            reopened.Manifest.OrderedPairs.Select(static pair => pair.PairId),
            reopened.Pairs.Select(PairId));
        foreach (var pair in reopened.Pairs)
            Assert.Equal(expectedById[PairId(pair)], pair);
    }

    [Fact]
    public void OpenSealed_CorruptPairBytesFailsClosed()
    {
        using var fixture = new RepositoryOwnedDpoDatasetManifestFixture();
        var address = fixture.Address();
        var pair = BoundLocalPair(
            "corrupt-pair",
            address.DomainSlug,
            "prompt",
            "chosen",
            "rejected",
            DateTimeOffset.UnixEpoch);
        var manifest = AssertSuccess(
            PersistAndSeal(
                address,
                [pair]));
        var pairPath = Path.Combine(
            address.RepositoryRoot,
            manifest.OrderedPairs[0].RepositoryRelativePairFilePath.Replace(
                '/',
                Path.DirectorySeparatorChar));
        File.AppendAllText(pairPath, "corrupt");

        var reopened = OpenSealed(
            address);

        Assert.False(reopened.IsSuccess);
        Assert.Equal(
            RepositoryOwnedDpoDatasetManifestSealer.InvalidErrorCode,
            reopened.ErrorCode);
    }

    [Fact]
    public void OpenSealed_CountMismatchFailsClosed()
    {
        using var fixture = new RepositoryOwnedDpoDatasetManifestFixture();
        var address = fixture.Address();
        var pair = BoundLocalPair(
            "count-mismatch",
            address.DomainSlug,
            "prompt",
            "chosen",
            "rejected",
            DateTimeOffset.UnixEpoch);
        var manifest = AssertSuccess(
            PersistAndSeal(
                address,
                [pair]));
        var tampered = manifest with
        {
            PairCount = 2,
            ManifestSha256 = string.Empty,
        };
        tampered = tampered with
        {
            ManifestSha256 = Sha256(CanonicalManifestJson(tampered)),
        };
        File.WriteAllBytes(address.ManifestPath, CanonicalManifestJson(tampered));

        var reopened = OpenSealed(
            address);

        Assert.False(reopened.IsSuccess);
        Assert.Equal(
            RepositoryOwnedDpoDatasetManifestSealer.InvalidErrorCode,
            reopened.ErrorCode);
    }

    [Fact]
    public void OpenManifestIdentityOnly_ReopensRequestIdentityWithoutCreatingStoreLocks()
    {
        using var fixture = new RepositoryOwnedDpoDatasetManifestFixture();
        var address = fixture.Address();
        var identity = RequestIdentity();
        var pair = BoundLocalPair(
            "manifest-identity-only-valid",
            address.DomainSlug,
            "prompt",
            "chosen",
            "rejected",
            DateTimeOffset.UnixEpoch);
        var manifest = AssertSuccess(PersistAndSeal(address, identity, [pair]));
        DeletePairStoreLock(address);
        DeleteDatasetSealLock(address);
        var before = SnapshotRelativeFileSystemEntries(address.DatasetRoot);

        var opened = OpenManifestIdentityOnly(address);

        Assert.True(opened.IsSuccess, opened.ErrorMessage);
        Assert.Equal(
            identity.RequestIdentitySha256,
            opened.Value!.RequestIdentity.RequestIdentitySha256);
        Assert.Equal(manifest.ManifestSha256, opened.Value.Manifest.ManifestSha256);
        Assert.False(File.Exists(PairStoreLockPath(address)));
        Assert.False(File.Exists(DatasetSealLockPath(address)));
        Assert.Equal(before, SnapshotRelativeFileSystemEntries(address.DatasetRoot));
    }

    [Fact]
    public void OpenManifestIdentityOnly_DoesNotReadPairBytesOrCreateStoreLocks()
    {
        using var fixture = new RepositoryOwnedDpoDatasetManifestFixture();
        var address = fixture.Address();
        var identity = RequestIdentity();
        var pair = BoundLocalPair(
            "manifest-identity-only-pair-bytes",
            address.DomainSlug,
            "prompt",
            "chosen",
            "rejected",
            DateTimeOffset.UnixEpoch);
        var manifest = AssertSuccess(PersistAndSeal(address, identity, [pair]));
        File.AppendAllText(
            PairFilePath(address, manifest.OrderedPairs[0]),
            "corrupt-pair-bytes-after-seal");
        DeletePairStoreLock(address);
        DeleteDatasetSealLock(address);
        var before = SnapshotRelativeFileSystemEntries(address.DatasetRoot);

        var opened = OpenManifestIdentityOnly(address);

        // WHY: this API is deliberately manifest-identity-only. Full pair-byte
        // integrity must be proved by OpenSealed/resolver before training rows
        // are consumed.
        Assert.True(opened.IsSuccess, opened.ErrorMessage);
        Assert.Equal(
            identity.RequestIdentitySha256,
            opened.Value!.RequestIdentity.RequestIdentitySha256);
        Assert.False(File.Exists(PairStoreLockPath(address)));
        Assert.False(File.Exists(DatasetSealLockPath(address)));
        Assert.Equal(before, SnapshotRelativeFileSystemEntries(address.DatasetRoot));
    }

    [Fact]
    public void OpenManifestFileIdentityOnly_ReturnsExactRawIdentityAndPublicProjection()
    {
        // WHY: retain actual assembly locations and raw identities from inside
        // the test host, separately from nominal before/after output snapshots.
        var productionAssemblyPath = typeof(RepositoryOwnedDpoDatasetManifestSealer)
            .Assembly.Location;
        var testAssemblyPath = typeof(RepositoryOwnedDpoDatasetManifestTests)
            .Assembly.Location;
        foreach (var runtimePath in new[]
        {
            productionAssemblyPath,
            testAssemblyPath,
            Path.ChangeExtension(testAssemblyPath, ".runtimeconfig.json"),
            Path.ChangeExtension(testAssemblyPath, ".deps.json"),
        })
        {
            _output.WriteLine("SOURCE_MANIFEST_RUNTIME|" + runtimePath + "|"
                + Sha256(File.ReadAllBytes(runtimePath)));
        }

        using var fixture = new RepositoryOwnedDpoDatasetManifestFixture();
        var address = fixture.Address();
        var identity = RequestIdentity();
        var pair = BoundLocalPair(
            "manifest-file-identity-valid", address.DomainSlug,
            "prompt", "chosen", "rejected", DateTimeOffset.UnixEpoch);
        var manifest = AssertSuccess(PersistAndSeal(address, identity, [pair]));
        var raw = File.ReadAllBytes(address.ManifestPath);

        var opened = AssertSuccess(RepositoryOwnedDpoDatasetManifestSealer
            .OpenManifestFileIdentityOnly(address));
        var publicOpened = AssertSuccess(OpenManifestIdentityOnly(address));

        Assert.Equal(Sha256(raw), opened.ManifestFileSha256);
        Assert.Equal(raw.LongLength, opened.ManifestFileByteLength);
        Assert.NotEqual(manifest.ManifestSha256, opened.ManifestFileSha256);
        Assert.Equal(manifest.ManifestSha256, opened.Manifest.ManifestSha256);
        Assert.Equal(identity.RequestIdentitySha256,
            opened.RequestIdentity.RequestIdentitySha256);
        Assert.Equal(opened.Manifest.ManifestSha256,
            publicOpened.Manifest.ManifestSha256);
        Assert.Equal(opened.RequestIdentity.RequestIdentitySha256,
            publicOpened.RequestIdentity.RequestIdentitySha256);
    }

    [Fact]
    public void OpenManifestFileIdentityOnly_LayoutChangesRawIdentityButNotSealedIdentity()
    {
        using var fixture = new RepositoryOwnedDpoDatasetManifestFixture();
        var address = fixture.Address();
        var identity = RequestIdentity();
        var pair = BoundLocalPair(
            "manifest-file-identity-layout", address.DomainSlug,
            "prompt", "chosen", "rejected", DateTimeOffset.UnixEpoch);
        AssertSuccess(PersistAndSeal(address, identity, [pair]));
        var originalBytes = File.ReadAllBytes(address.ManifestPath);
        var original = AssertSuccess(RepositoryOwnedDpoDatasetManifestSealer
            .OpenManifestFileIdentityOnly(address));
        var layoutBytes = Encoding.UTF8.GetBytes(
            "\n \t" + Encoding.UTF8.GetString(originalBytes) + "\r\n ");
        File.WriteAllBytes(address.ManifestPath, layoutBytes);

        var changed = AssertSuccess(RepositoryOwnedDpoDatasetManifestSealer
            .OpenManifestFileIdentityOnly(address));

        Assert.Equal(Sha256(originalBytes), original.ManifestFileSha256);
        Assert.Equal(originalBytes.LongLength, original.ManifestFileByteLength);
        Assert.Equal(Sha256(layoutBytes), changed.ManifestFileSha256);
        Assert.Equal(layoutBytes.LongLength, changed.ManifestFileByteLength);
        Assert.NotEqual(original.ManifestFileSha256, changed.ManifestFileSha256);
        Assert.NotEqual(original.ManifestFileByteLength, changed.ManifestFileByteLength);
        Assert.Equal(original.Manifest.ManifestSha256, changed.Manifest.ManifestSha256);
        Assert.Equal(original.RequestIdentity.RequestIdentitySha256,
            changed.RequestIdentity.RequestIdentitySha256);
        Assert.Equal(changed.Manifest.ManifestSha256,
            AssertSuccess(OpenManifestIdentityOnly(address)).Manifest.ManifestSha256);
    }

    [Fact]
    public void OpenManifestFileIdentityOnly_CorruptPairStillReadsOnlyManifestAndFullOpenRejects()
    {
        using var fixture = new RepositoryOwnedDpoDatasetManifestFixture();
        var address = fixture.Address();
        var identity = RequestIdentity();
        var pair = BoundLocalPair(
            "manifest-file-identity-pair-bytes", address.DomainSlug,
            "prompt", "chosen", "rejected", DateTimeOffset.UnixEpoch);
        var manifest = AssertSuccess(PersistAndSeal(address, identity, [pair]));
        AssertSuccess(OpenSealed(address, identity));
        AssertSuccess(RepositoryOwnedDpoDatasetManifestSealer
            .OpenManifestFileIdentityOnly(address));
        var pairPath = PairFilePath(address, manifest.OrderedPairs[0]);
        File.AppendAllText(pairPath, "corrupt-pair-bytes-after-seal");
        DeletePairStoreLock(address);
        DeleteDatasetSealLock(address);
        var before = SnapshotRelativeFileSystemEntries(address.DatasetRoot);
        var manifestBytes = File.ReadAllBytes(address.ManifestPath);
        var pairBytes = File.ReadAllBytes(pairPath);

        var opened = AssertSuccess(RepositoryOwnedDpoDatasetManifestSealer
            .OpenManifestFileIdentityOnly(address));

        Assert.Equal(Sha256(manifestBytes), opened.ManifestFileSha256);
        Assert.Equal(manifestBytes.LongLength, opened.ManifestFileByteLength);
        Assert.Equal(manifest.ManifestSha256, opened.Manifest.ManifestSha256);
        Assert.Equal(identity.RequestIdentitySha256,
            opened.RequestIdentity.RequestIdentitySha256);
        Assert.False(File.Exists(PairStoreLockPath(address)));
        Assert.False(File.Exists(DatasetSealLockPath(address)));
        Assert.Equal(before, SnapshotRelativeFileSystemEntries(address.DatasetRoot));
        Assert.Equal(manifestBytes, File.ReadAllBytes(address.ManifestPath));
        Assert.Equal(pairBytes, File.ReadAllBytes(pairPath));

        // WHY: snapshot assertions precede full open, which has separate pair
        // integrity responsibilities and may use its existing store locks.
        var fullOpen = OpenSealed(address, identity);
        Assert.False(fullOpen.IsSuccess);
        Assert.Equal(RepositoryOwnedDpoDatasetManifestSealer.InvalidErrorCode,
            fullOpen.ErrorCode);
    }

    [Theory]
    [InlineData("missing-manifest")]
    [InlineData("malformed-json")]
    [InlineData("address-binding-mismatch")]
    [InlineData("invalid-manifest-self-hash")]
    [InlineData("invalid-request-identity")]
    [InlineData("duplicate-property")]
    [InlineData("unknown-property")]
    [InlineData("case-alias-property")]
    [InlineData("invalid-utf8")]
    [InlineData("over-8-mib")]
    public void OpenManifestFileIdentityOnly_RejectsSingleFaultWithoutStoreMutation(
        string fault)
    {
        using var fixture = new RepositoryOwnedDpoDatasetManifestFixture();
        var address = fixture.Address();
        var identity = RequestIdentity();
        var pair = BoundLocalPair(
            "manifest-file-identity-" + fault, address.DomainSlug,
            "prompt", "chosen", "rejected", DateTimeOffset.UnixEpoch);
        var manifest = AssertSuccess(PersistAndSeal(address, identity, [pair]));
        var validBytes = File.ReadAllBytes(address.ManifestPath);
        var valid = AssertSuccess(RepositoryOwnedDpoDatasetManifestSealer
            .OpenManifestFileIdentityOnly(address));
        Assert.Equal(Sha256(validBytes), valid.ManifestFileSha256);
        Assert.Equal(validBytes.LongLength, valid.ManifestFileByteLength);
        var validJson = Encoding.UTF8.GetString(validBytes);

        switch (fault)
        {
            case "missing-manifest":
                File.Delete(address.ManifestPath);
                break;
            case "malformed-json":
                File.WriteAllText(address.ManifestPath, "{ malformed json }");
                break;
            case "address-binding-mismatch":
            case "invalid-request-identity":
                var mutated = fault == "address-binding-mismatch"
                    ? manifest with { Domain = "other-domain", ManifestSha256 = string.Empty }
                    : manifest with
                    {
                        RequestIdentity = identity with
                        {
                            RequestIdentitySha256 = new string('0', 64),
                        },
                        ManifestSha256 = string.Empty,
                    };
                // WHY: keep the outer self-hash valid so the address/request
                // guard, rather than an earlier hash failure, rejects this fault.
                mutated = mutated with
                {
                    ManifestSha256 = Sha256(CanonicalManifestJson(mutated)),
                };
                var mutatedBytes = CanonicalManifestJson(mutated);
                Assert.Equal(mutated.ManifestSha256,
                    Sha256(CanonicalManifestJson(mutated with { ManifestSha256 = string.Empty })));
                File.WriteAllBytes(address.ManifestPath, mutatedBytes);
                break;
            case "invalid-manifest-self-hash":
                File.WriteAllBytes(address.ManifestPath, CanonicalManifestJson(
                    manifest with { ManifestSha256 = new string('0', 64) }));
                break;
            case "duplicate-property":
                File.WriteAllText(address.ManifestPath,
                    "{\"domain\":\"" + address.DomainSlug + "\"," + validJson[1..]);
                break;
            case "unknown-property":
                File.WriteAllText(address.ManifestPath,
                    "{\"unknownField\":true," + validJson[1..]);
                break;
            case "case-alias-property":
                File.WriteAllText(address.ManifestPath,
                    validJson.Replace("\"domain\":", "\"Domain\":", StringComparison.Ordinal));
                break;
            case "invalid-utf8":
                var invalidUtf8 = (byte[])validBytes.Clone();
                var domainValueOffset = validJson.IndexOf("\"domain\":\"", StringComparison.Ordinal)
                    + "\"domain\":\"".Length;
                Assert.True(domainValueOffset >= "\"domain\":\"".Length);
                invalidUtf8[domainValueOffset] = 0xff;
                File.WriteAllBytes(address.ManifestPath, invalidUtf8);
                break;
            case "over-8-mib":
                var oversized = new byte[8 * 1024 * 1024 + 1];
                validBytes.CopyTo(oversized, 0);
                Array.Fill(oversized, (byte)' ', validBytes.Length,
                    oversized.Length - validBytes.Length);
                File.WriteAllBytes(address.ManifestPath, oversized);
                break;
            default:
                throw new InvalidOperationException("Unknown manifest fault: " + fault);
        }

        DeletePairStoreLock(address);
        DeleteDatasetSealLock(address);
        var before = SnapshotRelativeFileSystemEntries(address.DatasetRoot);
        var rejected = RepositoryOwnedDpoDatasetManifestSealer
            .OpenManifestFileIdentityOnly(address);
        var publicRejected = OpenManifestIdentityOnly(address);

        Assert.False(rejected.IsSuccess);
        Assert.Equal(RepositoryOwnedDpoDatasetManifestSealer.InvalidErrorCode,
            rejected.ErrorCode);
        Assert.False(publicRejected.IsSuccess);
        Assert.Equal(rejected.ErrorCode, publicRejected.ErrorCode);
        Assert.Equal(rejected.ErrorMessage, publicRejected.ErrorMessage);
        Assert.DoesNotContain(address.RepositoryRoot, rejected.ErrorMessage,
            StringComparison.OrdinalIgnoreCase);
        Assert.False(File.Exists(PairStoreLockPath(address)));
        Assert.False(File.Exists(DatasetSealLockPath(address)));
        Assert.Equal(before, SnapshotRelativeFileSystemEntries(address.DatasetRoot));

        File.WriteAllBytes(address.ManifestPath, validBytes);
        var restored = AssertSuccess(RepositoryOwnedDpoDatasetManifestSealer
            .OpenManifestFileIdentityOnly(address));
        Assert.Equal(valid.ManifestFileSha256, restored.ManifestFileSha256);
        Assert.Equal(valid.ManifestFileByteLength, restored.ManifestFileByteLength);
        Assert.Equal(valid.Manifest.ManifestSha256, restored.Manifest.ManifestSha256);
    }

    [Theory]
    [InlineData("missing-manifest")]
    [InlineData("corrupt-manifest")]
    [InlineData("address-binding-mismatch")]
    [InlineData("invalid-manifest-self-hash")]
    [InlineData("invalid-request-identity")]
    public void OpenManifestIdentityOnly_FailsClosedWithoutCreatingStoreLocks(
        string existingState)
    {
        using var fixture = new RepositoryOwnedDpoDatasetManifestFixture();
        var address = fixture.Address();
        var identity = RequestIdentity();
        var pair = BoundLocalPair(
            "manifest-identity-only-" + existingState,
            address.DomainSlug,
            "prompt",
            "chosen",
            "rejected",
            DateTimeOffset.UnixEpoch);
        var manifest = AssertSuccess(PersistAndSeal(address, identity, [pair]));
        if (existingState == "missing-manifest")
        {
            File.Delete(address.ManifestPath);
        }
        else if (existingState == "corrupt-manifest")
        {
            File.WriteAllText(
                address.ManifestPath,
                "{ not valid repository-owned manifest json }\n");
        }
        else if (existingState == "address-binding-mismatch")
        {
            var wrongAddressManifest = manifest with
            {
                Domain = "other-domain",
                ManifestSha256 = string.Empty,
            };
            wrongAddressManifest = wrongAddressManifest with
            {
                ManifestSha256 = Sha256(CanonicalManifestJson(
                    wrongAddressManifest)),
            };
            File.WriteAllBytes(
                address.ManifestPath,
                CanonicalManifestJson(wrongAddressManifest));
        }
        else if (existingState == "invalid-manifest-self-hash")
        {
            File.WriteAllBytes(
                address.ManifestPath,
                CanonicalManifestJson(manifest with
                {
                    ManifestSha256 = new string('0', 64),
                }));
        }
        else
        {
            var invalidRequestIdentity = identity with
            {
                RequestIdentitySha256 = new string('0', 64),
            };
            var invalidManifest = manifest with
            {
                RequestIdentity = invalidRequestIdentity,
                ManifestSha256 = string.Empty,
            };
            invalidManifest = invalidManifest with
            {
                ManifestSha256 = Sha256(CanonicalManifestJson(invalidManifest)),
            };
            File.WriteAllBytes(
                address.ManifestPath,
                CanonicalManifestJson(invalidManifest));
        }

        DeletePairStoreLock(address);
        DeleteDatasetSealLock(address);
        var before = SnapshotRelativeFileSystemEntries(address.DatasetRoot);

        var opened = OpenManifestIdentityOnly(address);

        Assert.False(opened.IsSuccess);
        Assert.Equal(
            RepositoryOwnedDpoDatasetManifestSealer.InvalidErrorCode,
            opened.ErrorCode);
        Assert.DoesNotContain(
            address.RepositoryRoot,
            opened.ErrorMessage,
            StringComparison.OrdinalIgnoreCase);
        Assert.False(File.Exists(PairStoreLockPath(address)));
        Assert.False(File.Exists(DatasetSealLockPath(address)));
        Assert.Equal(before, SnapshotRelativeFileSystemEntries(address.DatasetRoot));
    }

    [Fact]
    public void PersistAndSeal_RejectsFabricatedAddressWithoutWritingOutside()
    {
        using var fixture = new RepositoryOwnedDpoDatasetManifestFixture();
        var valid = fixture.Address();
        var outside = Path.Combine(fixture.Root, "outside-pairs");
        var fabricated = new RepositoryOwnedDpoDatasetAddress(
            valid.RepositoryRoot,
            valid.DpoArtifactRoot,
            valid.DomainSlug,
            valid.DesignPatternSlug,
            valid.DatasetVersion,
            valid.DatasetRoot,
            outside,
            outside,
            valid.ManifestPath,
            valid.DatasetRootRelativePath,
            "outside-pairs",
            "outside-pairs",
            valid.ManifestRelativePath);
        var pair = BoundLocalPair(
            "fabricated-address",
            valid.DomainSlug,
            "prompt",
            "chosen",
            "rejected",
            DateTimeOffset.UnixEpoch);

        var result = PersistAndSeal(
            fabricated,
            [pair]);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            RepositoryOwnedDpoDatasetManifestSealer.InvalidErrorCode,
            result.ErrorCode);
        Assert.False(Directory.Exists(outside));
        Assert.False(Directory.Exists(
            Path.Combine(valid.RepositoryRoot, "training-data")));
    }

    [Fact]
    public void OpenSealed_UnknownManifestPropertyFailsClosed()
    {
        using var fixture = new RepositoryOwnedDpoDatasetManifestFixture();
        var address = fixture.Address();
        var pair = BoundLocalPair(
            "unknown-property",
            address.DomainSlug,
            "prompt",
            "chosen",
            "rejected",
            DateTimeOffset.UnixEpoch);
        AssertSuccess(PersistAndSeal(
            address,
            [pair]));
        var json = JsonNode.Parse(File.ReadAllText(address.ManifestPath))!
            .AsObject();
        json["unknown"] = "tamper";
        File.WriteAllText(address.ManifestPath, json.ToJsonString());

        var reopened = OpenSealed(
            address);

        Assert.False(reopened.IsSuccess);
        Assert.Equal(
            RepositoryOwnedDpoDatasetManifestSealer.InvalidErrorCode,
            reopened.ErrorCode);
    }

    [Fact]
    public void PersistAndSeal_RequestIdentityIsV2HashOnlyAndMutatesManifestHash()
    {
        using var fixture = new RepositoryOwnedDpoDatasetManifestFixture();
        var address = fixture.Address();
        const string rawPaidAnswer =
            "paid reference answer that must not be serialized";
        var identity = RequestIdentity([Sha256Text(rawPaidAnswer)]);
        var pair = BoundLocalPair(
            "identity-v2-hash-only",
            address.DomainSlug,
            "identity prompt",
            "open teacher chosen",
            "local rejected",
            DateTimeOffset.UnixEpoch);

        var manifest = AssertSuccess(PersistAndSeal(
            address,
            identity,
            [pair]));
        var json = File.ReadAllText(address.ManifestPath);
        var mutatedIdentity = RequestIdentity(
            [Sha256Text("another paid reference")]);
        var unsignedCurrent = manifest with { ManifestSha256 = string.Empty };
        var unsignedMutated = manifest with
        {
            RequestIdentity = mutatedIdentity,
            ManifestSha256 = string.Empty,
        };

        Assert.Equal(
            RepositoryOwnedDpoDatasetManifestSealer.CurrentSchema,
            manifest.Schema);
        Assert.Contains(
            "repository-owned-dpo-dataset-request-identity-v1",
            json,
            StringComparison.Ordinal);
        Assert.DoesNotContain(rawPaidAnswer, json, StringComparison.Ordinal);
        Assert.NotEqual(
            Sha256(CanonicalManifestJson(unsignedCurrent)),
            Sha256(CanonicalManifestJson(unsignedMutated)));
    }

    [Theory]
    [InlineData("paid-reference")]
    [InlineData("teacher-artifact")]
    [InlineData("generation-policy")]
    [InlineData("creator-config")]
    public void Resolver_ExistingDatasetWithDifferentIdentityFailsWithoutCreatorOrMutation(
        string mutation)
    {
        using var fixture = new RepositoryOwnedDpoDatasetManifestFixture();
        var address = fixture.Address();
        var pair = BoundLocalPair(
            "identity-mismatch-" + mutation,
            address.DomainSlug,
            "mismatch prompt",
            "mismatch chosen",
            "mismatch rejected",
            DateTimeOffset.UnixEpoch);
        AssertSuccess(PersistAndSeal(address, [pair]));
        var manifestBytes = File.ReadAllBytes(address.ManifestPath);
        var pairSnapshots = SnapshotPairFiles(address);
        var creatorCalls = 0;

        var result = OpenExistingOrCreateAndSeal(
            address,
            MutatedIdentity(mutation),
            _ =>
            {
                Interlocked.Increment(ref creatorCalls);
                return OperationResult<IReadOnlyList<DpoPair>>.Ok([pair]);
            });

        Assert.False(result.IsSuccess);
        Assert.Equal(
            RepositoryOwnedDpoDatasetManifestSealer
                .RequestIdentityMismatchErrorCode,
            result.ErrorCode);
        Assert.Equal(0, creatorCalls);
        Assert.Equal(manifestBytes, File.ReadAllBytes(address.ManifestPath));
        AssertPairFileSnapshotsUnchanged(address, pairSnapshots);
    }

    [Fact]
    public void Resolver_V1ManifestWithoutRequestIdentityFailsClosedWithoutCreator()
    {
        using var fixture = new RepositoryOwnedDpoDatasetManifestFixture();
        var address = fixture.Address();
        var pair = BoundLocalPair(
            "legacy-v1-missing-identity",
            address.DomainSlug,
            "legacy prompt",
            "legacy chosen",
            "legacy rejected",
            DateTimeOffset.UnixEpoch);
        var manifest = AssertSuccess(PersistAndSeal(address, [pair]));
        var legacyUnsigned = manifest with
        {
            Schema = "repository-owned-dpo-dataset-manifest-v1",
            ManifestSha256 = string.Empty,
        };
        var legacyManifest = legacyUnsigned with
        {
            ManifestSha256 = Sha256(CanonicalLegacyV1ManifestJson(
                legacyUnsigned)),
        };
        File.WriteAllBytes(
            address.ManifestPath,
            CanonicalLegacyV1ManifestJson(legacyManifest));
        var creatorCalls = 0;

        var result = OpenExistingOrCreateAndSeal(
            address,
            _ =>
            {
                Interlocked.Increment(ref creatorCalls);
                return OperationResult<IReadOnlyList<DpoPair>>.Ok([pair]);
            });

        Assert.False(result.IsSuccess);
        Assert.Equal(
            RepositoryOwnedDpoDatasetManifestSealer.InvalidErrorCode,
            result.ErrorCode);
        Assert.Equal(0, creatorCalls);
    }

    [Theory]
    [InlineData("chosen")]
    [InlineData("rejected")]
    public void PersistAndSeal_PaidReferenceExactBytesInTrainableSideFailsClosed(
        string side)
    {
        using var fixture = new RepositoryOwnedDpoDatasetManifestFixture();
        var address = fixture.Address();
        const string rawPaidAnswer = "exact paid answer bytes";
        var identity = RequestIdentity([Sha256Text(rawPaidAnswer)]);
        var pair = BoundOpenTeacherRejectedPair(
            "paid-answer-side-" + side,
            address.DomainSlug,
            "paid side prompt",
            side == "chosen" ? rawPaidAnswer : "safe chosen",
            side == "rejected" ? rawPaidAnswer : "safe rejected",
            DateTimeOffset.UnixEpoch);

        var result = PersistAndSeal(address, identity, [pair]);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            RepositoryOwnedDpoDatasetManifestSealer.InvalidErrorCode,
            result.ErrorCode);
        Assert.False(File.Exists(address.ManifestPath));
    }

    [Theory]
    [InlineData("translation", "sentence-ranking")]
    [InlineData("project-wiki", "documentation-update")]
    public void PersistAndSeal_IdentityWorksAcrossDomainsWithoutHtmlCssHardcode(
        string domain,
        string designPattern)
    {
        using var fixture = new RepositoryOwnedDpoDatasetManifestFixture();
        var address = fixture.Address(
            domainSlug: domain,
            designPatternSlug: designPattern);
        var pair = BoundLocalPair(
            domain + "-" + designPattern,
            address.DomainSlug,
            "domain prompt",
            "domain chosen",
            "domain rejected",
            DateTimeOffset.UnixEpoch);

        var opened = AssertSuccess(OpenSealed(
            address,
            AssertSuccess(PersistAndSeal(address, [pair])).RequestIdentity));

        Assert.Equal(domain, opened.Manifest.Domain);
        Assert.Equal(designPattern, opened.Manifest.DesignPattern);
    }

    [Fact]
    public async Task Resolver_ConcurrentDifferentIdentityWaiterDoesNotReuseOrCreate()
    {
        using var fixture = new RepositoryOwnedDpoDatasetManifestFixture();
        var address = fixture.Address();
        var winnerIdentity = RequestIdentity();
        var waiterIdentity = RequestIdentity(
            generationConfigSha256: new string('7', 64));
        var winnerPair = BoundLocalPair(
            "different-identity-winner",
            address.DomainSlug,
            "winner prompt",
            "winner chosen",
            "winner rejected",
            DateTimeOffset.UnixEpoch);
        using var winnerEntered = new ManualResetEventSlim(false);
        using var releaseWinner = new ManualResetEventSlim(false);
        var winnerCreatorCalls = 0;
        var waiterCreatorCalls = 0;

        var winnerTask = Task.Run(() => OpenExistingOrCreateAndSeal(
            address,
            winnerIdentity,
            _ =>
            {
                Interlocked.Increment(ref winnerCreatorCalls);
                winnerEntered.Set();
                releaseWinner.Wait(TimeSpan.FromSeconds(5));
                return OperationResult<IReadOnlyList<DpoPair>>.Ok([winnerPair]);
            }));
        Assert.True(winnerEntered.Wait(TimeSpan.FromSeconds(5)));

        var waiterTask = Task.Run(() => OpenExistingOrCreateAndSeal(
            address,
            waiterIdentity,
            _ =>
            {
                Interlocked.Increment(ref waiterCreatorCalls);
                return OperationResult<IReadOnlyList<DpoPair>>.Ok([winnerPair]);
            }));
        await Task.Delay(TimeSpan.FromMilliseconds(100));
        Assert.False(waiterTask.IsCompleted);

        releaseWinner.Set();
        var winner = AssertSuccess(await AssertCompletesWithin(
            winnerTask,
            TimeSpan.FromSeconds(10)));
        var waiter = await AssertCompletesWithin(
            waiterTask,
            TimeSpan.FromSeconds(10));

        Assert.Equal(
            RepositoryOwnedDpoDatasetResolutionSource
                .NewlyPersistedRepositoryDataset,
            winner.Source);
        Assert.False(waiter.IsSuccess);
        Assert.Equal(
            RepositoryOwnedDpoDatasetManifestSealer
                .RequestIdentityMismatchErrorCode,
            waiter.ErrorCode);
        Assert.Equal(1, winnerCreatorCalls);
        Assert.Equal(0, waiterCreatorCalls);
    }

    [Fact]
    public void Resolver_ExistingSealedDatasetReusesDiskAndDoesNotCallCreator()
    {
        using var fixture = new RepositoryOwnedDpoDatasetManifestFixture();
        var address = fixture.Address();
        var pair = BoundOpenTeacherRejectedPair(
            "resolver-existing",
            address.DomainSlug,
            "prompt",
            "chosen",
            "rejected",
            DateTimeOffset.UnixEpoch);
        var manifest = AssertSuccess(
            PersistAndSeal(
                address,
                [pair]));
        var manifestBytes = File.ReadAllBytes(address.ManifestPath);
        var pairSnapshots = SnapshotPairFiles(address);
        var creatorCalls = 0;

        var resolved =
            OpenExistingOrCreateAndSeal(
                address,
                _ =>
                {
                    creatorCalls++;
                    return OperationResult<IReadOnlyList<DpoPair>>.Ok(
                        [BoundLocalPair(
                            "must-not-run",
                            address.DomainSlug,
                            "new prompt",
                            "new chosen",
                            "new rejected",
                            DateTimeOffset.UnixEpoch.AddDays(1))]);
                });

        var value = AssertSuccess(resolved);
        Assert.Equal(0, creatorCalls);
        Assert.Equal(
            RepositoryOwnedDpoDatasetResolutionSource
                .ExistingSealedRepositoryDataset,
            value.Source);
        Assert.Equal(manifest.ManifestSha256, value.Dataset.Manifest.ManifestSha256);
        Assert.Equal(pair, Assert.Single(value.Dataset.Pairs));
        Assert.Equal(manifestBytes, File.ReadAllBytes(address.ManifestPath));
        AssertPairFileSnapshotsUnchanged(address, pairSnapshots);
    }

    [Fact]
    public void Resolver_MissingDatasetCallsCreatorOnceAndSealsForNextReuse()
    {
        using var fixture = new RepositoryOwnedDpoDatasetManifestFixture();
        var address = fixture.Address();
        var pair = BoundOpenTeacherRejectedPair(
            "resolver-create",
            address.DomainSlug,
            "prompt",
            "chosen",
            "rejected",
            DateTimeOffset.UnixEpoch);
        var creatorCalls = 0;

        var created =
            OpenExistingOrCreateAndSeal(
                address,
                requestedAddress =>
                {
                    creatorCalls++;
                    Assert.Same(address, requestedAddress);
                    return OperationResult<IReadOnlyList<DpoPair>>.Ok([pair]);
                });
        var reused =
            OpenExistingOrCreateAndSeal(
                address,
                _ =>
                {
                    creatorCalls++;
                    return OperationResult<IReadOnlyList<DpoPair>>.Fail(
                        "Creator must not run after the manifest exists.",
                        "CREATOR_SHOULD_NOT_RUN");
                });

        var createdValue = AssertSuccess(created);
        var reusedValue = AssertSuccess(reused);
        Assert.Equal(1, creatorCalls);
        Assert.Equal(
            RepositoryOwnedDpoDatasetResolutionSource
                .NewlyPersistedRepositoryDataset,
            createdValue.Source);
        Assert.Equal(
            RepositoryOwnedDpoDatasetResolutionSource
                .ExistingSealedRepositoryDataset,
            reusedValue.Source);
        Assert.Equal(
            createdValue.Dataset.Manifest.ManifestSha256,
            reusedValue.Dataset.Manifest.ManifestSha256);
        Assert.Equal(pair, Assert.Single(reusedValue.Dataset.Pairs));
    }

    [Fact]
    public void Resolver_MissingDatasetCreatesOwnerOnlyLeaseDirectoryChain()
    {
        if (!OperatingSystem.IsLinux())
            return;

        using var fixture = new RepositoryOwnedDpoDatasetManifestFixture();
        var address = fixture.Address(
            "v0007",
            "lease-mode-domain",
            "lease-mode-pattern");
        var pair = BoundOpenTeacherRejectedPair(
            "resolver-lease-mode",
            address.DomainSlug,
            "prompt",
            "chosen",
            "rejected",
            DateTimeOffset.UnixEpoch);

        var created =
            OpenExistingOrCreateAndSeal(
                address,
                _ => OperationResult<IReadOnlyList<DpoPair>>.Ok([pair]));

        AssertSuccess(created);

        var leaseRoot = Path.Combine(
            address.RepositoryRoot,
            "training-artifacts",
            ".dpo-dataset-creation-leases");
        var domainLeaseDirectory = Path.Combine(leaseRoot, address.DomainSlug);
        var patternLeaseDirectory = Path.Combine(
            domainLeaseDirectory,
            address.DesignPatternSlug);

        // WHY: the cross-process test catches the symptom. This pins the Linux
        // filesystem mode regression that caused a clean clone to fail before
        // the creator could publish the reusable repository-owned dataset.
        AssertOwnerOnlyDirectoryMode(leaseRoot);
        AssertOwnerOnlyDirectoryMode(domainLeaseDirectory);
        AssertOwnerOnlyDirectoryMode(patternLeaseDirectory);
        AssertNoCreationLeaseFilesInsideDataset(address);
    }

    [Fact]
    public async Task Resolver_ConcurrentSameMissingDatasetReservesBeforeCreatorAndReusesWinner()
    {
        using var fixture = new RepositoryOwnedDpoDatasetManifestFixture();
        var address = fixture.Address();
        var pair = BoundOpenTeacherRejectedPair(
            "resolver-concurrent-same",
            address.DomainSlug,
            "prompt",
            "chosen",
            "rejected",
            DateTimeOffset.UnixEpoch);

        using var creatorEntered = new ManualResetEventSlim(false);
        using var releaseCreator = new ManualResetEventSlim(false);
        var creatorCalls = 0;

        OperationResult<IReadOnlyList<DpoPair>> Creator(
            RepositoryOwnedDpoDatasetAddress requestedAddress)
        {
            Assert.Same(address, requestedAddress);
            var call = Interlocked.Increment(ref creatorCalls);
            if (call != 1)
            {
                return OperationResult<IReadOnlyList<DpoPair>>.Fail(
                    "Concurrent resolver waiter must reopen disk instead of " +
                    "calling the DPO creator.",
                    "CREATOR_CALLED_TWICE");
            }

            creatorEntered.Set();
            if (!releaseCreator.Wait(TimeSpan.FromSeconds(5)))
            {
                return OperationResult<IReadOnlyList<DpoPair>>.Fail(
                    "Test did not release the held creator.",
                    "TEST_TIMEOUT");
            }

            return OperationResult<IReadOnlyList<DpoPair>>.Ok([pair]);
        }

        var firstTask = Task.Run(() =>
            OpenExistingOrCreateAndSeal(
                address,
                Creator));
        Assert.True(
            creatorEntered.Wait(TimeSpan.FromSeconds(5)),
            "First resolver call did not reach the creator.");

        var secondTask = Task.Run(() =>
            OpenExistingOrCreateAndSeal(
                address,
                Creator));
        await Task.Delay(TimeSpan.FromMilliseconds(100));
        Assert.Equal(1, Volatile.Read(ref creatorCalls));
        Assert.False(
            secondTask.IsCompleted,
            "Second resolver call completed while the first creator still held " +
            "the dataset reservation.");

        releaseCreator.Set();
        var results = await AssertCompletesWithin(
            Task.WhenAll(firstTask, secondTask),
            TimeSpan.FromSeconds(10));

        var first = AssertSuccess(results[0]);
        var second = AssertSuccess(results[1]);
        Assert.Equal(1, creatorCalls);
        Assert.Equal(
            RepositoryOwnedDpoDatasetResolutionSource.NewlyPersistedRepositoryDataset,
            first.Source);
        Assert.Equal(
            RepositoryOwnedDpoDatasetResolutionSource.ExistingSealedRepositoryDataset,
            second.Source);
        Assert.Equal(
            first.Dataset.Manifest.ManifestSha256,
            second.Dataset.Manifest.ManifestSha256);
        Assert.Equal(pair, Assert.Single(first.Dataset.Pairs));
        Assert.Equal(pair, Assert.Single(second.Dataset.Pairs));
        AssertNoTemporaryFiles(address);
        AssertNoCreationLeaseFilesInsideDataset(address);
        AssertCreationLeaseTreeContainsOnlyStableLocks(address);
    }

    [Fact]
    public async Task AsyncResolver_DelayedWaiterDuringActiveSealMustReuseWinner()
    {
        using var fixture = new RepositoryOwnedDpoDatasetManifestFixture();
        var address = fixture.Address();
        var identity = RequestIdentity();
        var pair = BoundLocalPair("delayed-seal-waiter", address.DomainSlug,
            "prompt", "chosen", "rejected", DateTimeOffset.UnixEpoch);
        using var sealEntered = new ManualResetEventSlim(false);
        using var releaseSeal = new ManualResetEventSlim(false);
        var prior = RepositoryOwnedDpoDatasetManifestSealer.AfterDatasetLeaseAcquiredForTesting;
        var creatorThread = 0;
        var firstCreatorCalls = 0;
        var waiterCreatorCalls = 0;
        Task<OperationResult<IReadOnlyList<DpoPair>>> Creator(
            RepositoryOwnedDpoDatasetAddress _, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref firstCreatorCalls);
            Volatile.Write(ref creatorThread, Environment.CurrentManagedThreadId);
            return Task.FromResult(OperationResult<IReadOnlyList<DpoPair>>.Ok([pair]));
        }
        Task<OperationResult<IReadOnlyList<DpoPair>>> UnexpectedCreator(
            RepositoryOwnedDpoDatasetAddress _, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref waiterCreatorCalls);
            return Task.FromResult(OperationResult<IReadOnlyList<DpoPair>>.Fail(
                "A delayed waiter must reopen the winner.", "UNEXPECTED_CREATOR_CALL"));
        }
        Task<OperationResult<RepositoryOwnedDpoDatasetResolution>>? first = null;
        try
        {
            // Use the existing seam only on the actual creator's executing thread.
            // The directory and seal lock have already been created by production.
            RepositoryOwnedDpoDatasetManifestSealer.AfterDatasetLeaseAcquiredForTesting = () =>
            {
                if (Environment.CurrentManagedThreadId != Volatile.Read(ref creatorThread)) return;
                sealEntered.Set();
                if (!releaseSeal.Wait(TimeSpan.FromSeconds(10)))
                    throw new TimeoutException("The held real dataset seal was not released.");
            };
            first = Task.Run(() => RepositoryOwnedDpoDatasetResolver.OpenExistingOrCreateAndSealAsync(
                address, identity, Creator));
            Assert.True(sealEntered.Wait(TimeSpan.FromSeconds(10)));
            Assert.False(first.IsCompleted);
            Assert.False(File.Exists(address.ManifestPath));
            var partialEntries = Directory.GetFileSystemEntries(address.DatasetRoot)
                .Select(Path.GetFileName).OrderBy(name => name, StringComparer.Ordinal).ToArray();
            Assert.Contains(Path.GetFileName(DatasetSealLockPath(address)), partialEntries);
            var waiterTask = RepositoryOwnedDpoDatasetResolver.OpenExistingOrCreateAndSealAsync(
                address, identity, UnexpectedCreator);
            Assert.False(waiterTask.IsCompleted);
            Assert.Equal(0, waiterCreatorCalls);
            _output.WriteLine("S04_MANIFEST_DELAYED_WAITER " + JsonSerializer.Serialize(new {
                processId = Environment.ProcessId, address.DatasetRoot, partialEntries,
                manifestAbsent = !File.Exists(address.ManifestPath), activeWinner = !first.IsCompleted,
                firstCreatorCalls, waiterCreatorCalls, waiterPending = !waiterTask.IsCompleted }));
            releaseSeal.Set();
            var winner = AssertSuccess(await AssertCompletesWithin(first, TimeSpan.FromSeconds(10)));
            var waiter = AssertSuccess(await AssertCompletesWithin(waiterTask, TimeSpan.FromSeconds(10)));
            var fresh = AssertSuccess(await RepositoryOwnedDpoDatasetResolver.OpenExistingOrCreateAndSealAsync(
                address, identity, UnexpectedCreator));
            Assert.Equal(RepositoryOwnedDpoDatasetResolutionSource.NewlyPersistedRepositoryDataset, winner.Source);
            Assert.Equal(RepositoryOwnedDpoDatasetResolutionSource.ExistingSealedRepositoryDataset, waiter.Source);
            Assert.Equal(RepositoryOwnedDpoDatasetResolutionSource.ExistingSealedRepositoryDataset, fresh.Source);
            Assert.Equal(winner.Dataset.Manifest.ManifestSha256, waiter.Dataset.Manifest.ManifestSha256);
            Assert.Equal(pair, Assert.Single(waiter.Dataset.Pairs));
            Assert.Equal(winner.Dataset.Manifest.ManifestSha256, fresh.Dataset.Manifest.ManifestSha256);
            Assert.Equal(pair, Assert.Single(fresh.Dataset.Pairs));
            Assert.Equal(1, firstCreatorCalls); Assert.Equal(0, waiterCreatorCalls);
            _output.WriteLine("S04_MANIFEST_AFTER_SEAL " + JsonSerializer.Serialize(new {
                winner.Source, freshSource = fresh.Source, fresh.Dataset.Manifest.ManifestSha256,
                firstCreatorCalls, waiterCreatorCalls, actualPairReadback = true }));
        }
        finally
        {
            releaseSeal.Set();
            if (first is not null) await first;
            RepositoryOwnedDpoDatasetManifestSealer.AfterDatasetLeaseAcquiredForTesting = prior;
        }
    }

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, false)]
    [InlineData(false, true, true)]
    [InlineData(true, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, true, true)]
    public async Task Resolver_ActivePartialSealWaitIsBoundedAndExistingOnlyNeverCreates(
        bool sync, bool existingOnly, bool cancel)
    {
        using var fixture = new RepositoryOwnedDpoDatasetManifestFixture();
        var address = fixture.Address();
        var identity = RequestIdentity();
        var pair = BoundLocalPair("bounded-active-seal", address.DomainSlug,
            "prompt", "chosen", "rejected", DateTimeOffset.UnixEpoch);
        using var sealEntered = new ManualResetEventSlim(false);
        using var releaseSeal = new ManualResetEventSlim(false);
        using var cancellation = new CancellationTokenSource();
        var prior = RepositoryOwnedDpoDatasetManifestSealer.AfterDatasetLeaseAcquiredForTesting;
        var creatorThread = 0;
        var firstCreatorCalls = 0;
        var waiterCreatorCalls = 0;
        OperationResult<IReadOnlyList<DpoPair>> UnexpectedCreator(RepositoryOwnedDpoDatasetAddress _)
        {
            Interlocked.Increment(ref waiterCreatorCalls);
            return OperationResult<IReadOnlyList<DpoPair>>.Fail("Unexpected creator.", "UNEXPECTED_CREATOR_CALL");
        }
        Task<OperationResult<IReadOnlyList<DpoPair>>> UnexpectedAsyncCreator(
            RepositoryOwnedDpoDatasetAddress a, CancellationToken _) => Task.FromResult(UnexpectedCreator(a));
        Task<OperationResult<RepositoryOwnedDpoDatasetResolution>>? first = null;
        try
        {
            var missingAddress = fixture.Address("missing-existing-only");
            var missingBefore = SnapshotRelativeFileSystemEntries(fixture.Root);
            var missing = sync
                ? RepositoryOwnedDpoDatasetResolver.OpenExistingOrCreateAndSeal(missingAddress, identity, null)
                : await RepositoryOwnedDpoDatasetResolver.OpenExistingOrCreateAndSealAsync(missingAddress, identity, null);
            Assert.Equal(RepositoryOwnedDpoDatasetResolver.MissingErrorCode, missing.ErrorCode);
            Assert.False(Directory.Exists(missingAddress.DatasetRoot));
            Assert.Equal(missingBefore, SnapshotRelativeFileSystemEntries(fixture.Root));
            RepositoryOwnedDpoDatasetManifestSealer.AfterDatasetLeaseAcquiredForTesting = () =>
            {
                if (Environment.CurrentManagedThreadId != Volatile.Read(ref creatorThread)) return;
                sealEntered.Set();
                if (!releaseSeal.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException("Held seal timeout.");
            };
            first = Task.Run(() => RepositoryOwnedDpoDatasetResolver.OpenExistingOrCreateAndSeal(address, identity, _ => {
                Interlocked.Increment(ref firstCreatorCalls);
                Volatile.Write(ref creatorThread, Environment.CurrentManagedThreadId);
                return OperationResult<IReadOnlyList<DpoPair>>.Ok([pair]); }));
            Assert.True(sealEntered.Wait(TimeSpan.FromSeconds(10)));
            var before = SnapshotRelativeFileSystemEntries(fixture.Root);
            Assert.False(first.IsCompleted);
            Assert.False(File.Exists(address.ManifestPath));
            var leaseTimeout = TimeSpan.FromMilliseconds(150);
            // .NET 9 SemaphoreSlim truncates the remaining TimeSpan to integer
            // milliseconds after the resolver subtracts its elapsed setup time.
            // That can lose less than one millisecond, but cannot excuse an
            // immediate or otherwise early BUSY return from the held lease.
            var semaphoreTimeoutQuantum = TimeSpan.FromMilliseconds(1);
            var minimumDeadlineWait = leaseTimeout - semaphoreTimeoutQuantum;
            var clock = Stopwatch.StartNew();
            long? cancellationRequestedAt = null;
            long? cancellationObservedAt = null;
            long? pendingWaitObservedAt = null;
            using var cancellationRegistration = cancellation.Token.Register(
                () => cancellationObservedAt = clock.ElapsedTicks);
            var cancellationPhase = existingOnly ? "existing-only-immediate-refusal" :
                !cancel ? "deadline" : sync ? "pre-entry" : "in-flight-creation-lease";

            void CancelExplicitly()
            {
                Assert.False(first.IsCompleted);
                Assert.False(File.Exists(address.ManifestPath));
                Assert.False(cancellation.IsCancellationRequested);
                cancellationRequestedAt = clock.ElapsedTicks;
                cancellation.Cancel();
                Assert.True(cancellation.IsCancellationRequested);
                Assert.NotNull(cancellationObservedAt);
            }

            // There is no sync wait-entry seam. Prove pre-entry cancellation
            // directly instead of assuming Task.Run or a timer has reached Wait.
            if (sync && cancel && !existingOnly) CancelExplicitly();
            var resolverCallStartedAt = clock.ElapsedTicks;
            OperationResult<RepositoryOwnedDpoDatasetResolution> refusal;
            if (sync)
            {
                refusal = RepositoryOwnedDpoDatasetResolver.OpenExistingOrCreateAndSeal(address, identity,
                    existingOnly ? null : UnexpectedCreator, cancellation.Token, leaseTimeout);
            }
            else
            {
                var waiter = RepositoryOwnedDpoDatasetResolver.OpenExistingOrCreateAndSealAsync(address, identity,
                    existingOnly ? null : UnexpectedAsyncCreator, cancellation.Token, leaseTimeout);
                if (cancel && !existingOnly)
                {
                    // The direct async call has executed synchronously through
                    // the guards to its first incomplete await: the held creation
                    // process lease. No scheduled Task.Run/timer is the handshake.
                    Assert.False(waiter.IsCompleted);
                    pendingWaitObservedAt = clock.ElapsedTicks;
                    CancelExplicitly();
                }
                refusal = await AssertCompletesWithin(waiter, TimeSpan.FromSeconds(4));
            }
            var resolverReturnedAt = clock.ElapsedTicks;
            // Capture only the resolver call; diagnostic output must not make
            // an early return appear to have waited for the deadline.
            var resolverElapsed = Stopwatch.GetElapsedTime(resolverCallStartedAt, resolverReturnedAt);
            _output.WriteLine("S04_MANIFEST_EXPLICIT_CANCELLATION " + JsonSerializer.Serialize(new {
                sync, existingOnly, cancel, cancellationPhase, cancellationRequestedAt, cancellationObservedAt,
                pendingWaitObservedAt, resolverCallStartedAt, resolverReturnedAt, Stopwatch.Frequency,
                tokenActuallyCancelled = cancellation.IsCancellationRequested, refusal.ErrorCode,
                leaseTimeoutMilliseconds = leaseTimeout.TotalMilliseconds,
                semaphoreTimeoutQuantumMilliseconds = semaphoreTimeoutQuantum.TotalMilliseconds,
                minimumDeadlineWaitMilliseconds = minimumDeadlineWait.TotalMilliseconds,
                resolverElapsedMilliseconds = resolverElapsed.TotalMilliseconds,
                activeWinnerBeforeRelease = !first.IsCompleted, manifestAbsent = !File.Exists(address.ManifestPath) }));
            Assert.Equal(existingOnly ? RepositoryOwnedDpoDatasetManifestSealer.InvalidErrorCode :
                cancel ? RepositoryOwnedDpoDatasetResolver.CancelledErrorCode : RepositoryOwnedDpoDatasetResolver.BusyErrorCode,
                refusal.ErrorCode);
            Assert.Equal(cancel && !existingOnly, cancellation.IsCancellationRequested);
            if (!cancel && !existingOnly)
                Assert.True(resolverElapsed >= minimumDeadlineWait,
                    $"Held creation lease returned BUSY after {resolverElapsed.TotalMilliseconds:R} ms; " +
                    $"the {leaseTimeout.TotalMilliseconds:R} ms deadline permits only the " +
                    $"{semaphoreTimeoutQuantum.TotalMilliseconds:R} ms SemaphoreSlim conversion quantum.");
            Assert.True(clock.Elapsed < TimeSpan.FromSeconds(4));
            Assert.False(first.IsCompleted); Assert.False(File.Exists(address.ManifestPath));
            Assert.Equal(before, SnapshotRelativeFileSystemEntries(fixture.Root));
            Assert.Equal(0, waiterCreatorCalls);
            releaseSeal.Set();
            var winner = AssertSuccess(await AssertCompletesWithin(first, TimeSpan.FromSeconds(10)));
            var fresh = sync
                ? AssertSuccess(RepositoryOwnedDpoDatasetResolver.OpenExistingOrCreateAndSeal(address, identity, null))
                : AssertSuccess(await RepositoryOwnedDpoDatasetResolver.OpenExistingOrCreateAndSealAsync(address, identity, null));
            Assert.Equal(winner.Dataset.Manifest.ManifestSha256, fresh.Dataset.Manifest.ManifestSha256);
            Assert.Equal(pair, Assert.Single(fresh.Dataset.Pairs));
            File.AppendAllText(address.ManifestPath, "stable-corrupt");
            var corruptBytes = File.ReadAllBytes(address.ManifestPath);
            var corrupt = sync
                ? RepositoryOwnedDpoDatasetResolver.OpenExistingOrCreateAndSeal(address, identity, existingOnly ? null : UnexpectedCreator)
                : await RepositoryOwnedDpoDatasetResolver.OpenExistingOrCreateAndSealAsync(address, identity, existingOnly ? null : UnexpectedAsyncCreator);
            Assert.Equal(RepositoryOwnedDpoDatasetManifestSealer.InvalidErrorCode, corrupt.ErrorCode);
            Assert.Equal(corruptBytes, File.ReadAllBytes(address.ManifestPath));
            Assert.Equal(1, firstCreatorCalls); Assert.Equal(0, waiterCreatorCalls);
            _output.WriteLine("S04_MANIFEST_BOUNDED_SEAL " + JsonSerializer.Serialize(new { sync, existingOnly, cancel,
                refusal.ErrorCode, firstCreatorCalls, waiterCreatorCalls, originalStillPendingBeforeRelease = true,
                freshExistingOnlyPairExact = true, stableCorruptBytesExact = true }));
        }
        finally
        {
            releaseSeal.Set();
            if (first is not null) await first;
            RepositoryOwnedDpoDatasetManifestSealer.AfterDatasetLeaseAcquiredForTesting = prior;
        }
    }

    [Fact]
    public async Task Resolver_SameAddressWaiterTimesOutWithoutCallingItsCreator()
    {
        using var fixture = new RepositoryOwnedDpoDatasetManifestFixture();
        var address = fixture.Address();
        var pair = BoundOpenTeacherRejectedPair(
            "resolver-busy-waiter",
            address.DomainSlug,
            "prompt",
            "chosen",
            "rejected",
            DateTimeOffset.UnixEpoch);
        using var creatorEntered = new ManualResetEventSlim(false);
        using var releaseCreator = new ManualResetEventSlim(false);
        var firstCreatorCalls = 0;
        var waiterCreatorCalls = 0;

        var firstTask = Task.Run(() =>
            OpenExistingOrCreateAndSeal(
                address,
                _ =>
                {
                    Interlocked.Increment(ref firstCreatorCalls);
                    creatorEntered.Set();
                    releaseCreator.Wait(TimeSpan.FromSeconds(5));
                    return OperationResult<IReadOnlyList<DpoPair>>.Ok([pair]);
                }));
        Assert.True(creatorEntered.Wait(TimeSpan.FromSeconds(5)));

        var waiter = OpenExistingOrCreateAndSeal(
            address,
            _ =>
            {
                Interlocked.Increment(ref waiterCreatorCalls);
                return OperationResult<IReadOnlyList<DpoPair>>.Ok([pair]);
            },
            CancellationToken.None,
            TimeSpan.FromMilliseconds(100));

        Assert.False(waiter.IsSuccess);
        Assert.Equal(
            RepositoryOwnedDpoDatasetResolver.BusyErrorCode,
            waiter.ErrorCode);
        Assert.Equal(0, waiterCreatorCalls);
        Assert.Equal(1, firstCreatorCalls);
        Assert.False(firstTask.IsCompleted);

        releaseCreator.Set();
        AssertSuccess(await AssertCompletesWithin(
            firstTask,
            TimeSpan.FromSeconds(10)));
        Assert.Equal(1, firstCreatorCalls);
        AssertNoTemporaryFiles(address);
        AssertCreationLeaseTreeContainsOnlyStableLocks(address);
    }

    [Fact]
    public async Task Resolver_SameAddressCancelledWaiterNeverCallsItsCreator()
    {
        using var fixture = new RepositoryOwnedDpoDatasetManifestFixture();
        var address = fixture.Address();
        var pair = BoundOpenTeacherRejectedPair(
            "resolver-cancelled-waiter",
            address.DomainSlug,
            "prompt",
            "chosen",
            "rejected",
            DateTimeOffset.UnixEpoch);
        using var creatorEntered = new ManualResetEventSlim(false);
        using var releaseCreator = new ManualResetEventSlim(false);
        using var waiterCancellation = new CancellationTokenSource();
        var firstCreatorCalls = 0;
        var waiterCreatorCalls = 0;

        var firstTask = Task.Run(() =>
            OpenExistingOrCreateAndSeal(
                address,
                _ =>
                {
                    Interlocked.Increment(ref firstCreatorCalls);
                    creatorEntered.Set();
                    releaseCreator.Wait(TimeSpan.FromSeconds(5));
                    return OperationResult<IReadOnlyList<DpoPair>>.Ok([pair]);
                }));
        Assert.True(creatorEntered.Wait(TimeSpan.FromSeconds(5)));

        var waiterTask = Task.Run(() =>
            OpenExistingOrCreateAndSeal(
                address,
                _ =>
                {
                    Interlocked.Increment(ref waiterCreatorCalls);
                    return OperationResult<IReadOnlyList<DpoPair>>.Ok([pair]);
                },
                waiterCancellation.Token));
        await Task.Delay(TimeSpan.FromMilliseconds(100));
        waiterCancellation.Cancel();

        var waiter = await AssertCompletesWithin(
            waiterTask,
            TimeSpan.FromSeconds(5));
        Assert.False(waiter.IsSuccess);
        Assert.Equal(
            RepositoryOwnedDpoDatasetResolver.CancelledErrorCode,
            waiter.ErrorCode);
        Assert.Equal(0, waiterCreatorCalls);
        Assert.Equal(1, firstCreatorCalls);
        Assert.False(firstTask.IsCompleted);

        releaseCreator.Set();
        AssertSuccess(await AssertCompletesWithin(
            firstTask,
            TimeSpan.FromSeconds(10)));
        Assert.Equal(1, firstCreatorCalls);
        AssertNoTemporaryFiles(address);
        AssertCreationLeaseTreeContainsOnlyStableLocks(address);
    }

    [Fact]
    public async Task Resolver_ConcurrentDifferentMissingDatasetsDoNotShareOneGlobalCreatorGate()
    {
        using var fixture = new RepositoryOwnedDpoDatasetManifestFixture();
        var firstAddress = fixture.Address();
        var secondAddress = fixture.Address("v0002");
        var firstPair = BoundOpenTeacherRejectedPair(
            "resolver-concurrent-different-first",
            firstAddress.DomainSlug,
            "first prompt",
            "first chosen",
            "first rejected",
            DateTimeOffset.UnixEpoch);
        var secondPair = BoundOpenTeacherRejectedPair(
            "resolver-concurrent-different-second",
            secondAddress.DomainSlug,
            "second prompt",
            "second chosen",
            "second rejected",
            DateTimeOffset.UnixEpoch.AddSeconds(5));

        using var firstCreatorEntered = new ManualResetEventSlim(false);
        using var secondCreatorEntered = new ManualResetEventSlim(false);
        using var releaseCreators = new ManualResetEventSlim(false);
        var creatorCalls = 0;

        OperationResult<IReadOnlyList<DpoPair>> FirstCreator(
            RepositoryOwnedDpoDatasetAddress requestedAddress)
        {
            Assert.Same(firstAddress, requestedAddress);
            Interlocked.Increment(ref creatorCalls);
            firstCreatorEntered.Set();
            releaseCreators.Wait(TimeSpan.FromSeconds(5));
            return OperationResult<IReadOnlyList<DpoPair>>.Ok([firstPair]);
        }

        OperationResult<IReadOnlyList<DpoPair>> SecondCreator(
            RepositoryOwnedDpoDatasetAddress requestedAddress)
        {
            Assert.Same(secondAddress, requestedAddress);
            Interlocked.Increment(ref creatorCalls);
            secondCreatorEntered.Set();
            releaseCreators.Wait(TimeSpan.FromSeconds(5));
            return OperationResult<IReadOnlyList<DpoPair>>.Ok([secondPair]);
        }

        var firstTask = Task.Run(() =>
            OpenExistingOrCreateAndSeal(
                firstAddress,
                FirstCreator));
        Assert.True(
            firstCreatorEntered.Wait(TimeSpan.FromSeconds(5)),
            "First creator did not enter.");

        var secondTask = Task.Run(() =>
            OpenExistingOrCreateAndSeal(
                secondAddress,
                SecondCreator));
        Assert.True(
            secondCreatorEntered.Wait(TimeSpan.FromSeconds(5)),
            "A different dataset address was blocked by a global creator gate.");
        Assert.Equal(2, Volatile.Read(ref creatorCalls));

        releaseCreators.Set();
        var results = await AssertCompletesWithin(
            Task.WhenAll(firstTask, secondTask),
            TimeSpan.FromSeconds(10));

        Assert.Equal(
            RepositoryOwnedDpoDatasetResolutionSource.NewlyPersistedRepositoryDataset,
            AssertSuccess(results[0]).Source);
        Assert.Equal(
            RepositoryOwnedDpoDatasetResolutionSource.NewlyPersistedRepositoryDataset,
            AssertSuccess(results[1]).Source);
        AssertNoCreationLeaseFilesInsideDataset(firstAddress);
        AssertNoCreationLeaseFilesInsideDataset(secondAddress);
        AssertCreationLeaseTreeContainsOnlyStableLocks(
            firstAddress,
            secondAddress);
    }

    [Fact]
    public void Resolver_MissingDatasetWithoutCreatorFailsWithoutCreatingFiles()
    {
        using var fixture = new RepositoryOwnedDpoDatasetManifestFixture();
        var address = fixture.Address();

        var result =
            OpenExistingOrCreateAndSeal(
                address,
                createPairs: null);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            RepositoryOwnedDpoDatasetResolver.MissingErrorCode,
            result.ErrorCode);
        Assert.False(Directory.Exists(address.DatasetRoot));
        Assert.False(File.Exists(address.ManifestPath));
        AssertNoCreationLeaseFilesInsideDataset(address);
    }

    [Fact]
    public void Resolver_CorruptExistingManifestFailsWithoutCallingCreator()
    {
        using var fixture = new RepositoryOwnedDpoDatasetManifestFixture();
        var address = fixture.Address();
        var pair = BoundLocalPair(
            "resolver-corrupt",
            address.DomainSlug,
            "prompt",
            "chosen",
            "rejected",
            DateTimeOffset.UnixEpoch);
        AssertSuccess(PersistAndSeal(
            address,
            [pair]));
        File.AppendAllText(address.ManifestPath, "corrupt");
        var creatorCalls = 0;

        var result =
            OpenExistingOrCreateAndSeal(
                address,
                _ =>
                {
                    creatorCalls++;
                    return OperationResult<IReadOnlyList<DpoPair>>.Ok([pair]);
                });

        Assert.False(result.IsSuccess);
        Assert.Equal(0, creatorCalls);
        Assert.Equal(
            RepositoryOwnedDpoDatasetManifestSealer.InvalidErrorCode,
            result.ErrorCode);
    }

    [Fact]
    public void Resolver_ManifestMissingButPairFilesExistFailsWithoutCallingCreator()
    {
        using var fixture = new RepositoryOwnedDpoDatasetManifestFixture();
        var address = fixture.Address();
        var pair = BoundLocalPair(
            "resolver-partial",
            address.DomainSlug,
            "prompt",
            "chosen",
            "rejected",
            DateTimeOffset.UnixEpoch);
        AssertSuccess(PersistAndSeal(
            address,
            [pair]));
        var pairSnapshots = SnapshotPairFiles(address);
        File.Delete(address.ManifestPath);
        var creatorCalls = 0;

        var result =
            OpenExistingOrCreateAndSeal(
                address,
                _ =>
                {
                    creatorCalls++;
                    return OperationResult<IReadOnlyList<DpoPair>>.Ok(
                        [BoundLocalPair(
                            "replacement",
                            address.DomainSlug,
                            "new prompt",
                            "new chosen",
                            "new rejected",
                            DateTimeOffset.UnixEpoch.AddDays(1))]);
                });

        Assert.False(result.IsSuccess);
        Assert.Equal(0, creatorCalls);
        Assert.Equal(
            RepositoryOwnedDpoDatasetManifestSealer.InvalidErrorCode,
            result.ErrorCode);
        AssertPairFileSnapshotsUnchanged(address, pairSnapshots);
        Assert.False(File.Exists(address.ManifestPath));
    }

    [Fact]
    public void Resolver_CreatorFailureDoesNotCreateDatasetRoot()
    {
        using var fixture = new RepositoryOwnedDpoDatasetManifestFixture();
        var address = fixture.Address();
        var creatorCalls = 0;

        var result =
            OpenExistingOrCreateAndSeal(
                address,
                _ =>
                {
                    creatorCalls++;
                    return OperationResult<IReadOnlyList<DpoPair>>.Fail(
                        "Teacher/generator failed before producing a dataset.",
                        "GENERATOR_FAILED");
                });

        Assert.False(result.IsSuccess);
        Assert.Equal(1, creatorCalls);
        Assert.Equal("GENERATOR_FAILED", result.ErrorCode);
        Assert.False(Directory.Exists(address.DatasetRoot));

        var pair = BoundOpenTeacherRejectedPair(
            "resolver-create-after-failure",
            address.DomainSlug,
            "retry prompt",
            "retry chosen",
            "retry rejected",
            DateTimeOffset.UnixEpoch.AddMinutes(1));
        var retry =
            OpenExistingOrCreateAndSeal(
                address,
                _ =>
                {
                    creatorCalls++;
                    return OperationResult<IReadOnlyList<DpoPair>>.Ok([pair]);
                });

        var retryValue = AssertSuccess(retry);
        Assert.Equal(2, creatorCalls);
        Assert.Equal(
            RepositoryOwnedDpoDatasetResolutionSource.NewlyPersistedRepositoryDataset,
            retryValue.Source);
        Assert.Equal(pair, Assert.Single(retryValue.Dataset.Pairs));
        AssertNoCreationLeaseFilesInsideDataset(address);
        AssertNoTemporaryFiles(address);
        AssertCreationLeaseTreeContainsOnlyStableLocks(address);
    }

    [Fact]
    public void Resolver_CreatorExceptionReleasesLeaseAndLeavesOnlyStableLock()
    {
        using var fixture = new RepositoryOwnedDpoDatasetManifestFixture();
        var address = fixture.Address();

        var failed =
            OpenExistingOrCreateAndSeal(
                address,
                _ => throw new InvalidOperationException(
                    "Deliberate creator failure for cleanup verification."));

        Assert.False(failed.IsSuccess);
        Assert.Equal(
            RepositoryOwnedDpoDatasetResolver.InvalidErrorCode,
            failed.ErrorCode);
        Assert.False(Directory.Exists(address.DatasetRoot));
        AssertCreationLeaseTreeContainsOnlyStableLocks(address);

        var pair = BoundOpenTeacherRejectedPair(
            "resolver-create-after-exception",
            address.DomainSlug,
            "retry prompt",
            "retry chosen",
            "retry rejected",
            DateTimeOffset.UnixEpoch.AddMinutes(2));
        var retry =
            OpenExistingOrCreateAndSeal(
                address,
                _ => OperationResult<IReadOnlyList<DpoPair>>.Ok([pair]));

        Assert.Equal(
            RepositoryOwnedDpoDatasetResolutionSource.NewlyPersistedRepositoryDataset,
            AssertSuccess(retry).Source);
        AssertNoTemporaryFiles(address);
        AssertCreationLeaseTreeContainsOnlyStableLocks(address);
    }

    [Fact]
    public void Resolver_ExistingZeroByteCreationLeaseLockFailsClosedWithoutRewriteOrCreator()
    {
        using var fixture = new RepositoryOwnedDpoDatasetManifestFixture();
        var address = fixture.Address("v-zero-byte-lock");
        var leasePaths = CreateGitCloneStyleCreationLease(address);
        File.WriteAllBytes(leasePaths.LockPath, []);
        var creatorCalls = 0;

        var result = OpenExistingOrCreateAndSeal(
            address,
            _ =>
            {
                Interlocked.Increment(ref creatorCalls);
                return OperationResult<IReadOnlyList<DpoPair>>.Ok(
                    [BoundOpenTeacherRejectedPair(
                        "must-not-create-after-empty-lock",
                        address.DomainSlug,
                        "prompt",
                        "chosen",
                        "rejected",
                        DateTimeOffset.UnixEpoch)]);
            });

        Assert.False(result.IsSuccess);
        Assert.Equal(
            RepositoryOwnedDpoDatasetResolver.UnavailableErrorCode,
            result.ErrorCode);
        Assert.Equal(0, creatorCalls);
        Assert.Equal(0, new FileInfo(leasePaths.LockPath).Length);
        Assert.False(Directory.Exists(address.DatasetRoot));
        Assert.False(File.Exists(address.ManifestPath));
    }

    [Fact]
    public async Task AsyncResolver_ExistingSealedDatasetReusesDiskAndDoesNotCallCreator()
    {
        using var fixture = new RepositoryOwnedDpoDatasetManifestFixture();
        var address = fixture.Address();
        var pair = BoundOpenTeacherRejectedPair(
            "async-resolver-existing",
            address.DomainSlug,
            "prompt",
            "chosen",
            "rejected",
            DateTimeOffset.UnixEpoch);
        var manifest = AssertSuccess(PersistAndSeal(address, [pair]));
        var manifestBytes = File.ReadAllBytes(address.ManifestPath);
        var pairSnapshots = SnapshotPairFiles(address);
        var creatorCalls = 0;

        var resolved = await OpenExistingOrCreateAndSealAsync(
            address,
            (_, _) =>
            {
                Interlocked.Increment(ref creatorCalls);
                return Task.FromResult(
                    OperationResult<IReadOnlyList<DpoPair>>.Ok(
                        [BoundLocalPair(
                            "async-must-not-run",
                            address.DomainSlug,
                            "new prompt",
                            "new chosen",
                            "new rejected",
                            DateTimeOffset.UnixEpoch.AddDays(1))]));
            });

        var value = AssertSuccess(resolved);
        Assert.Equal(0, creatorCalls);
        Assert.Equal(
            RepositoryOwnedDpoDatasetResolutionSource
                .ExistingSealedRepositoryDataset,
            value.Source);
        Assert.Equal(manifest.ManifestSha256, value.Dataset.Manifest.ManifestSha256);
        Assert.Equal(pair, Assert.Single(value.Dataset.Pairs));
        Assert.Equal(manifestBytes, File.ReadAllBytes(address.ManifestPath));
        AssertPairFileSnapshotsUnchanged(address, pairSnapshots);
    }

    [Fact]
    public async Task AsyncResolver_ConcurrentSameMissingDatasetAwaitsCreatorOnceAndReusesWinner()
    {
        using var fixture = new RepositoryOwnedDpoDatasetManifestFixture();
        var address = fixture.Address();
        var pair = BoundOpenTeacherRejectedPair(
            "async-resolver-concurrent-same",
            address.DomainSlug,
            "prompt",
            "chosen",
            "rejected",
            DateTimeOffset.UnixEpoch);
        var creatorEntered = AsyncSignal();
        var releaseCreator = AsyncSignal();
        var creatorCalls = 0;

        async Task<OperationResult<IReadOnlyList<DpoPair>>> Creator(
            RepositoryOwnedDpoDatasetAddress requestedAddress,
            CancellationToken cancellationToken)
        {
            Assert.Same(address, requestedAddress);
            var call = Interlocked.Increment(ref creatorCalls);
            if (call != 1)
            {
                return OperationResult<IReadOnlyList<DpoPair>>.Fail(
                    "Async waiter must reopen disk instead of calling creator.",
                    "ASYNC_CREATOR_CALLED_TWICE");
            }

            creatorEntered.SetResult(true);
            await releaseCreator.Task
                .WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);
            return OperationResult<IReadOnlyList<DpoPair>>.Ok([pair]);
        }

        var firstTask = OpenExistingOrCreateAndSealAsync(address, Creator);
        await AssertCompletesWithin(creatorEntered.Task, TimeSpan.FromSeconds(5));
        var secondTask = OpenExistingOrCreateAndSealAsync(address, Creator);

        Assert.Equal(1, Volatile.Read(ref creatorCalls));
        releaseCreator.SetResult(true);
        var results = await AssertCompletesWithin(
            Task.WhenAll(firstTask, secondTask),
            TimeSpan.FromSeconds(10));

        var first = AssertSuccess(results[0]);
        var second = AssertSuccess(results[1]);
        Assert.Equal(1, creatorCalls);
        Assert.Equal(
            RepositoryOwnedDpoDatasetResolutionSource.NewlyPersistedRepositoryDataset,
            first.Source);
        Assert.Equal(
            RepositoryOwnedDpoDatasetResolutionSource.ExistingSealedRepositoryDataset,
            second.Source);
        Assert.Equal(
            first.Dataset.Manifest.ManifestSha256,
            second.Dataset.Manifest.ManifestSha256);
        Assert.Equal(pair, Assert.Single(first.Dataset.Pairs));
        Assert.Equal(pair, Assert.Single(second.Dataset.Pairs));
        AssertNoTemporaryFiles(address);
        AssertNoCreationLeaseFilesInsideDataset(address);
        AssertCreationLeaseTreeContainsOnlyStableLocks(address);
    }

    [Fact(Timeout = 180000)]
    public async Task
        AsyncResolver_GitCloneStyleTrackedLeaseVstestChildProcessesReserveOnceReuseDiskAndFailClosed()
    {
        if (string.Equals(
                Environment.GetEnvironmentVariable(
                    CrossProcessDpoResolverChildModeEnvironmentName),
                "1",
                StringComparison.Ordinal))
        {
            await RunCrossProcessDpoResolverVstestChildAsync();
            return;
        }

        using var fixture = new RepositoryOwnedDpoDatasetManifestFixture();
        var address = fixture.Address(
            CrossProcessDpoResolverDatasetVersion,
            CrossProcessDpoResolverDomainSlug,
            CrossProcessDpoResolverDesignPatternSlug);
        var identity = CrossProcessDpoResolverRequestIdentity();
        Assert.Equal(
            "training-data/dpo/cross-process-dpo/resolver-reuse/v0001",
            address.DatasetRootRelativePath);

        CreationLeaseTestPaths? gitCloneStyleLease = null;
        if (OperatingSystem.IsLinux())
        {
            gitCloneStyleLease = CreateGitCloneStyleCreationLease(address);
            AssertGitCloneStyleCreationLeaseLooksLikeFreshClone(
                gitCloneStyleLease);
        }

        var coordinationDirectory = Path.Combine(
            fixture.Root,
            "cross-process-dpo-resolver-coordination");
        var resultDirectory = Path.Combine(
            fixture.Root,
            "cross-process-dpo-resolver-results");
        Directory.CreateDirectory(coordinationDirectory);
        Directory.CreateDirectory(resultDirectory);

        var firstResultPath = Path.Combine(resultDirectory, "first.json");
        var secondResultPath = Path.Combine(resultDirectory, "second.json");

        // WHY: the in-process tests prove the process-local semaphore path.
        // This launches two independent `dotnet vstest` children, gives them
        // only paths plus serialized identity/config text, and holds the one
        // real creator until the parent proves both resolver callers crossed
        // the OS process boundary.
        using var firstChild = StartCrossProcessDpoResolverVstestChild(
            fixture.Root,
            address,
            identity,
            coordinationDirectory,
            firstResultPath,
            CrossProcessDpoResolverConcurrentScenario);
        using var secondChild = StartCrossProcessDpoResolverVstestChild(
            fixture.Root,
            address,
            identity,
            coordinationDirectory,
            secondResultPath,
            CrossProcessDpoResolverConcurrentScenario);

        await WaitForCrossProcessDpoResolverMarkerCountAsync(
            coordinationDirectory,
            "resolver-entered.*.json",
            expectedCount: 2,
            timeout: TimeSpan.FromSeconds(30));
        await WaitForExactlyOneCrossProcessDpoResolverCreatorAsync(
            coordinationDirectory,
            timeout: TimeSpan.FromSeconds(30),
            ("first", firstChild, firstResultPath),
            ("second", secondChild, secondResultPath));
        ReleaseCrossProcessDpoResolverCreator(coordinationDirectory);

        var concurrentResults = await AssertCompletesWithin(
            Task.WhenAll(
                WaitForCrossProcessDpoResolverChildAsync(
                    firstChild,
                    firstResultPath,
                    TimeSpan.FromSeconds(75)),
                WaitForCrossProcessDpoResolverChildAsync(
                    secondChild,
                    secondResultPath,
                    TimeSpan.FromSeconds(75))),
            TimeSpan.FromSeconds(90));

        Assert.Equal(
            2,
            concurrentResults
                .Select(static result => result.ProcessId)
                .Distinct()
                .Count());
        Assert.Equal(
            1,
            CountCrossProcessDpoResolverMarkers(
                coordinationDirectory,
                "creator-entered.*.json"));
        Assert.All(
            concurrentResults,
            result =>
            {
                Assert.True(result.IsSuccess, result.ErrorMessage);
                Assert.Equal(
                    identity.RequestIdentitySha256,
                    result.RequestIdentitySha256);
                Assert.Equal(1, result.PairCount);
                AssertSingleCrossProcessDpoResolverPairFile(result);
            });
        Assert.Equal(
            1,
            concurrentResults.Sum(static result => result.CreatorCalls));

        var created = Assert.Single(
            concurrentResults,
            static result => result.Source
                == nameof(RepositoryOwnedDpoDatasetResolutionSource
                    .NewlyPersistedRepositoryDataset));
        var reused = Assert.Single(
            concurrentResults,
            static result => result.Source
                == nameof(RepositoryOwnedDpoDatasetResolutionSource
                    .ExistingSealedRepositoryDataset));
        Assert.Equal(1, created.CreatorCalls);
        Assert.Equal(0, reused.CreatorCalls);
        Assert.Equal(created.ManifestSha256, reused.ManifestSha256);
        Assert.Equal(created.PairFiles, reused.PairFiles);

        AssertNoTemporaryFiles(address);
        AssertNoCreationLeaseFilesInsideDataset(address);
        AssertCreationLeaseTreeContainsOnlyStableLocks(address);
        if (OperatingSystem.IsLinux() && gitCloneStyleLease is not null)
            AssertGitCloneStyleCreationLeaseNarrowed(gitCloneStyleLease);
        AssertNoCrossProcessDpoResolverTemporaryProtocolFiles(
            coordinationDirectory,
            resultDirectory);

        var sealedManifestBytes = File.ReadAllBytes(address.ManifestPath);
        var sealedPairSnapshots = SnapshotPairFiles(address);
        var sealedPair = Assert.Single(sealedPairSnapshots.Values);

        var reuseResultPath = Path.Combine(resultDirectory, "reuse.json");
        using (var reuseChild = StartCrossProcessDpoResolverVstestChild(
                   fixture.Root,
                   address,
                   identity,
                   coordinationDirectory,
                   reuseResultPath,
                   CrossProcessDpoResolverReuseScenario))
        {
            var reuseResult = await WaitForCrossProcessDpoResolverChildAsync(
                reuseChild,
                reuseResultPath,
                TimeSpan.FromSeconds(45));
            Assert.True(reuseResult.IsSuccess, reuseResult.ErrorMessage);
            Assert.Equal(0, reuseResult.CreatorCalls);
            Assert.Equal(
                nameof(RepositoryOwnedDpoDatasetResolutionSource
                    .ExistingSealedRepositoryDataset),
                reuseResult.Source);
            Assert.Equal(identity.RequestIdentitySha256,
                reuseResult.RequestIdentitySha256);
            Assert.Equal(created.ManifestSha256, reuseResult.ManifestSha256);
            Assert.Equal(created.PairFiles, reuseResult.PairFiles);
        }

        Assert.Equal(sealedManifestBytes, File.ReadAllBytes(address.ManifestPath));
        AssertPairFileSnapshotsUnchanged(address, sealedPairSnapshots);
        Assert.Equal(sealedPair.Sha256, Assert.Single(created.PairFiles).PairFileSha256);
        Assert.Equal(
            1,
            CountCrossProcessDpoResolverMarkers(
                coordinationDirectory,
                "creator-entered.*.json"));

        var mismatchResultPath = Path.Combine(resultDirectory, "mismatch.json");
        using (var mismatchChild = StartCrossProcessDpoResolverVstestChild(
                   fixture.Root,
                   address,
                   MutatedIdentity("generation-policy"),
                   coordinationDirectory,
                   mismatchResultPath,
                   CrossProcessDpoResolverIdentityMismatchScenario))
        {
            var mismatchResult = await WaitForCrossProcessDpoResolverChildAsync(
                mismatchChild,
                mismatchResultPath,
                TimeSpan.FromSeconds(45));
            Assert.False(mismatchResult.IsSuccess);
            Assert.Equal(0, mismatchResult.CreatorCalls);
            Assert.Equal(
                RepositoryOwnedDpoDatasetManifestSealer
                    .RequestIdentityMismatchErrorCode,
                mismatchResult.ErrorCode);
        }

        Assert.Equal(sealedManifestBytes, File.ReadAllBytes(address.ManifestPath));
        AssertPairFileSnapshotsUnchanged(address, sealedPairSnapshots);
        AssertNoTemporaryFiles(address);
        AssertNoCreationLeaseFilesInsideDataset(address);
        AssertCreationLeaseTreeContainsOnlyStableLocks(address);
        if (OperatingSystem.IsLinux() && gitCloneStyleLease is not null)
            AssertGitCloneStyleCreationLeaseNarrowed(gitCloneStyleLease);
        AssertNoCrossProcessDpoResolverTemporaryProtocolFiles(
            coordinationDirectory,
            resultDirectory);
        Assert.Equal(
            1,
            CountCrossProcessDpoResolverMarkers(
                coordinationDirectory,
                "creator-entered.*.json"));
    }

    [Fact]
    public async Task AsyncResolver_ConcurrentDifferentIdentityWaiterFailsWithoutCreator()
    {
        using var fixture = new RepositoryOwnedDpoDatasetManifestFixture();
        var address = fixture.Address();
        var winnerIdentity = RequestIdentity();
        var waiterIdentity = RequestIdentity(
            generationConfigSha256: new string('7', 64));
        var winnerPair = BoundLocalPair(
            "async-different-identity-winner",
            address.DomainSlug,
            "winner prompt",
            "winner chosen",
            "winner rejected",
            DateTimeOffset.UnixEpoch);
        var winnerEntered = AsyncSignal();
        var releaseWinner = AsyncSignal();
        var winnerCreatorCalls = 0;
        var waiterCreatorCalls = 0;

        var winnerTask = OpenExistingOrCreateAndSealAsync(
            address,
            winnerIdentity,
            async (_, cancellationToken) =>
            {
                Interlocked.Increment(ref winnerCreatorCalls);
                winnerEntered.SetResult(true);
                await releaseWinner.Task
                    .WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);
                return OperationResult<IReadOnlyList<DpoPair>>.Ok([winnerPair]);
            });
        await AssertCompletesWithin(winnerEntered.Task, TimeSpan.FromSeconds(5));

        var waiterTask = OpenExistingOrCreateAndSealAsync(
            address,
            waiterIdentity,
            (_, _) =>
            {
                Interlocked.Increment(ref waiterCreatorCalls);
                return Task.FromResult(
                    OperationResult<IReadOnlyList<DpoPair>>.Ok([winnerPair]));
            });

        releaseWinner.SetResult(true);
        var winner = AssertSuccess(await AssertCompletesWithin(
            winnerTask,
            TimeSpan.FromSeconds(10)));
        var waiter = await AssertCompletesWithin(
            waiterTask,
            TimeSpan.FromSeconds(10));

        Assert.Equal(
            RepositoryOwnedDpoDatasetResolutionSource
                .NewlyPersistedRepositoryDataset,
            winner.Source);
        Assert.False(waiter.IsSuccess);
        Assert.Equal(
            RepositoryOwnedDpoDatasetManifestSealer
                .RequestIdentityMismatchErrorCode,
            waiter.ErrorCode);
        Assert.Equal(1, winnerCreatorCalls);
        Assert.Equal(0, waiterCreatorCalls);
        AssertCreationLeaseTreeContainsOnlyStableLocks(address);
    }

    [Fact]
    public async Task AsyncResolver_WaitingCancellationDoesNotCallWaiterCreator()
    {
        using var fixture = new RepositoryOwnedDpoDatasetManifestFixture();
        var address = fixture.Address();
        var pair = BoundOpenTeacherRejectedPair(
            "async-resolver-cancelled-waiter",
            address.DomainSlug,
            "prompt",
            "chosen",
            "rejected",
            DateTimeOffset.UnixEpoch);
        var creatorEntered = AsyncSignal();
        var releaseCreator = AsyncSignal();
        using var waiterCancellation = new CancellationTokenSource();
        var firstCreatorCalls = 0;
        var waiterCreatorCalls = 0;

        var firstTask = OpenExistingOrCreateAndSealAsync(
            address,
            async (_, cancellationToken) =>
            {
                Interlocked.Increment(ref firstCreatorCalls);
                creatorEntered.SetResult(true);
                await releaseCreator.Task
                    .WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);
                return OperationResult<IReadOnlyList<DpoPair>>.Ok([pair]);
            });
        await AssertCompletesWithin(creatorEntered.Task, TimeSpan.FromSeconds(5));

        var waiterTask = OpenExistingOrCreateAndSealAsync(
            address,
            (_, _) =>
            {
                Interlocked.Increment(ref waiterCreatorCalls);
                return Task.FromResult(
                    OperationResult<IReadOnlyList<DpoPair>>.Ok([pair]));
            },
            waiterCancellation.Token);
        waiterCancellation.Cancel();

        var waiter = await AssertCompletesWithin(
            waiterTask,
            TimeSpan.FromSeconds(5));
        Assert.False(waiter.IsSuccess);
        Assert.Equal(
            RepositoryOwnedDpoDatasetResolver.CancelledErrorCode,
            waiter.ErrorCode);
        Assert.Equal(0, waiterCreatorCalls);
        Assert.Equal(1, firstCreatorCalls);
        Assert.False(firstTask.IsCompleted);

        releaseCreator.SetResult(true);
        AssertSuccess(await AssertCompletesWithin(
            firstTask,
            TimeSpan.FromSeconds(10)));
        AssertNoTemporaryFiles(address);
        AssertCreationLeaseTreeContainsOnlyStableLocks(address);
    }

    [Fact]
    public async Task AsyncResolver_BoundedBusyTimeoutDoesNotCallWaiterCreator()
    {
        using var fixture = new RepositoryOwnedDpoDatasetManifestFixture();
        var address = fixture.Address();
        var pair = BoundOpenTeacherRejectedPair(
            "async-resolver-busy-waiter",
            address.DomainSlug,
            "prompt",
            "chosen",
            "rejected",
            DateTimeOffset.UnixEpoch);
        var creatorEntered = AsyncSignal();
        var releaseCreator = AsyncSignal();
        var firstCreatorCalls = 0;
        var waiterCreatorCalls = 0;

        var firstTask = OpenExistingOrCreateAndSealAsync(
            address,
            async (_, cancellationToken) =>
            {
                Interlocked.Increment(ref firstCreatorCalls);
                creatorEntered.SetResult(true);
                await releaseCreator.Task
                    .WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);
                return OperationResult<IReadOnlyList<DpoPair>>.Ok([pair]);
            });
        await AssertCompletesWithin(creatorEntered.Task, TimeSpan.FromSeconds(5));

        var waiter = await OpenExistingOrCreateAndSealAsync(
            address,
            (_, _) =>
            {
                Interlocked.Increment(ref waiterCreatorCalls);
                return Task.FromResult(
                    OperationResult<IReadOnlyList<DpoPair>>.Ok([pair]));
            },
            CancellationToken.None,
            TimeSpan.FromMilliseconds(100));

        Assert.False(waiter.IsSuccess);
        Assert.Equal(
            RepositoryOwnedDpoDatasetResolver.BusyErrorCode,
            waiter.ErrorCode);
        Assert.Equal(0, waiterCreatorCalls);
        Assert.Equal(1, firstCreatorCalls);
        Assert.False(firstTask.IsCompleted);

        releaseCreator.SetResult(true);
        AssertSuccess(await AssertCompletesWithin(
            firstTask,
            TimeSpan.FromSeconds(10)));
        AssertNoTemporaryFiles(address);
        AssertCreationLeaseTreeContainsOnlyStableLocks(address);
    }

    [Theory]
    [InlineData("failure")]
    [InlineData("exception")]
    [InlineData("cancelled-task")]
    public async Task
        AsyncResolver_CreatorFailureExceptionOrCancellationCleansUnsealedRootAndRetry(
            string mode)
    {
        using var fixture = new RepositoryOwnedDpoDatasetManifestFixture();
        var address = fixture.Address();
        var creatorCalls = 0;

        var failed = await OpenExistingOrCreateAndSealAsync(
            address,
            async (_, _) =>
            {
                Interlocked.Increment(ref creatorCalls);
                Directory.CreateDirectory(address.DatasetRoot);
                await File.WriteAllTextAsync(
                    Path.Combine(address.DatasetRoot, "partial.txt"),
                    "partial async creator output");

                if (mode == "failure")
                {
                    return OperationResult<IReadOnlyList<DpoPair>>.Fail(
                        "Async teacher/generator failed before sealing.",
                        "ASYNC_GENERATOR_FAILED");
                }

                if (mode == "exception")
                    throw new InvalidOperationException(
                        "Deliberate async creator exception.");

                return await Task.FromCanceled<
                    OperationResult<IReadOnlyList<DpoPair>>>(
                    new CancellationToken(canceled: true));
            });

        Assert.False(failed.IsSuccess);
        Assert.Equal(
            mode switch
            {
                "failure" => "ASYNC_GENERATOR_FAILED",
                "exception" => RepositoryOwnedDpoDatasetResolver.InvalidErrorCode,
                _ => RepositoryOwnedDpoDatasetResolver.CancelledErrorCode,
            },
            failed.ErrorCode);
        Assert.Equal(1, creatorCalls);
        Assert.False(Directory.Exists(address.DatasetRoot));
        AssertCreationLeaseTreeContainsOnlyStableLocks(address);

        var pair = BoundOpenTeacherRejectedPair(
            "async-resolver-retry-" + mode,
            address.DomainSlug,
            "retry prompt",
            "retry chosen",
            "retry rejected",
            DateTimeOffset.UnixEpoch.AddMinutes(3));
        var retry = await OpenExistingOrCreateAndSealAsync(
            address,
            (_, _) =>
            {
                Interlocked.Increment(ref creatorCalls);
                return Task.FromResult(
                    OperationResult<IReadOnlyList<DpoPair>>.Ok([pair]));
            });

        var retryValue = AssertSuccess(retry);
        Assert.Equal(2, creatorCalls);
        Assert.Equal(
            RepositoryOwnedDpoDatasetResolutionSource.NewlyPersistedRepositoryDataset,
            retryValue.Source);
        Assert.Equal(pair, Assert.Single(retryValue.Dataset.Pairs));
        AssertNoTemporaryFiles(address);
        AssertCreationLeaseTreeContainsOnlyStableLocks(address);
    }

    [Fact]
    public async Task AsyncResolver_DifferentMissingDatasetsDoNotShareGlobalGate()
    {
        using var fixture = new RepositoryOwnedDpoDatasetManifestFixture();
        var firstAddress = fixture.Address();
        var secondAddress = fixture.Address("v0002");
        var firstPair = BoundOpenTeacherRejectedPair(
            "async-resolver-different-first",
            firstAddress.DomainSlug,
            "first prompt",
            "first chosen",
            "first rejected",
            DateTimeOffset.UnixEpoch);
        var secondPair = BoundOpenTeacherRejectedPair(
            "async-resolver-different-second",
            secondAddress.DomainSlug,
            "second prompt",
            "second chosen",
            "second rejected",
            DateTimeOffset.UnixEpoch.AddSeconds(5));
        var firstCreatorEntered = AsyncSignal();
        var secondCreatorEntered = AsyncSignal();
        var releaseFirst = AsyncSignal();
        var creatorCalls = 0;

        var firstTask = OpenExistingOrCreateAndSealAsync(
            firstAddress,
            async (requestedAddress, cancellationToken) =>
            {
                Assert.Same(firstAddress, requestedAddress);
                Interlocked.Increment(ref creatorCalls);
                firstCreatorEntered.SetResult(true);
                await releaseFirst.Task
                    .WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);
                return OperationResult<IReadOnlyList<DpoPair>>.Ok([firstPair]);
            });
        await AssertCompletesWithin(
            firstCreatorEntered.Task,
            TimeSpan.FromSeconds(5));

        var secondTask = OpenExistingOrCreateAndSealAsync(
            secondAddress,
            (requestedAddress, _) =>
            {
                Assert.Same(secondAddress, requestedAddress);
                Interlocked.Increment(ref creatorCalls);
                secondCreatorEntered.SetResult(true);
                return Task.FromResult(
                    OperationResult<IReadOnlyList<DpoPair>>.Ok([secondPair]));
            });
        await AssertCompletesWithin(
            secondCreatorEntered.Task,
            TimeSpan.FromSeconds(5));
        var second = AssertSuccess(await AssertCompletesWithin(
            secondTask,
            TimeSpan.FromSeconds(5)));

        releaseFirst.SetResult(true);
        var first = AssertSuccess(await AssertCompletesWithin(
            firstTask,
            TimeSpan.FromSeconds(10)));

        Assert.Equal(2, creatorCalls);
        Assert.Equal(
            RepositoryOwnedDpoDatasetResolutionSource.NewlyPersistedRepositoryDataset,
            first.Source);
        Assert.Equal(
            RepositoryOwnedDpoDatasetResolutionSource.NewlyPersistedRepositoryDataset,
            second.Source);
        AssertNoCreationLeaseFilesInsideDataset(firstAddress);
        AssertNoCreationLeaseFilesInsideDataset(secondAddress);
        AssertCreationLeaseTreeContainsOnlyStableLocks(
            firstAddress,
            secondAddress);
    }

    [Theory]
    [InlineData("identity-mismatch")]
    [InlineData("corrupt-manifest")]
    [InlineData("legacy-v1")]
    [InlineData("partial-pair-files")]
    public async Task AsyncResolver_MismatchCorruptV1AndPartialFailBeforeCreator(
        string existingState)
    {
        using var fixture = new RepositoryOwnedDpoDatasetManifestFixture();
        var address = fixture.Address();
        var identity = RequestIdentity();
        var pair = BoundLocalPair(
            "async-fail-closed-" + existingState,
            address.DomainSlug,
            "prompt",
            "chosen",
            "rejected",
            DateTimeOffset.UnixEpoch);
        var manifest = AssertSuccess(PersistAndSeal(address, identity, [pair]));
        var pairSnapshots = SnapshotPairFiles(address);
        var openIdentity = identity;

        if (existingState == "identity-mismatch")
        {
            openIdentity = MutatedIdentity("generation-policy");
        }
        else if (existingState == "corrupt-manifest")
        {
            File.AppendAllText(address.ManifestPath, "corrupt");
        }
        else if (existingState == "legacy-v1")
        {
            var legacyUnsigned = manifest with
            {
                Schema = "repository-owned-dpo-dataset-manifest-v1",
                ManifestSha256 = string.Empty,
            };
            var legacyManifest = legacyUnsigned with
            {
                ManifestSha256 = Sha256(CanonicalLegacyV1ManifestJson(
                    legacyUnsigned)),
            };
            File.WriteAllBytes(
                address.ManifestPath,
                CanonicalLegacyV1ManifestJson(legacyManifest));
        }
        else
        {
            File.Delete(address.ManifestPath);
        }

        var creatorCalls = 0;
        var result = await OpenExistingOrCreateAndSealAsync(
            address,
            openIdentity,
            (_, _) =>
            {
                Interlocked.Increment(ref creatorCalls);
                return Task.FromResult(
                    OperationResult<IReadOnlyList<DpoPair>>.Ok([pair]));
            });

        Assert.False(result.IsSuccess);
        Assert.Equal(0, creatorCalls);
        Assert.Equal(
            existingState == "identity-mismatch"
                ? RepositoryOwnedDpoDatasetManifestSealer
                    .RequestIdentityMismatchErrorCode
                : RepositoryOwnedDpoDatasetManifestSealer.InvalidErrorCode,
            result.ErrorCode);
        AssertPairFileSnapshotsUnchanged(address, pairSnapshots);
    }

    private static OperationResult<RepositoryOwnedDpoDatasetManifest>
        PersistAndSeal(
            RepositoryOwnedDpoDatasetAddress address,
            IReadOnlyList<DpoPair> pairs)
        => RepositoryOwnedDpoDatasetManifestSealer.PersistAndSeal(
            address,
            RequestIdentity(),
            pairs);

    private static OperationResult<RepositoryOwnedDpoDatasetManifest>
        PersistAndSeal(
            RepositoryOwnedDpoDatasetAddress address,
            RepositoryOwnedDpoDatasetRequestIdentity identity,
            IReadOnlyList<DpoPair> pairs)
        => RepositoryOwnedDpoDatasetManifestSealer.PersistAndSeal(
            address,
            identity,
            pairs);

    private static OperationResult<RepositoryOwnedDpoDatasetOpenResult>
        OpenSealed(RepositoryOwnedDpoDatasetAddress address)
        => RepositoryOwnedDpoDatasetManifestSealer.OpenSealed(
            address,
            RequestIdentity());

    private static OperationResult<RepositoryOwnedDpoDatasetOpenResult>
        OpenSealed(
            RepositoryOwnedDpoDatasetAddress address,
            RepositoryOwnedDpoDatasetRequestIdentity identity)
        => RepositoryOwnedDpoDatasetManifestSealer.OpenSealed(address, identity);

    private static OperationResult<RepositoryOwnedDpoDatasetManifestIdentityOpenResult>
        OpenManifestIdentityOnly(RepositoryOwnedDpoDatasetAddress address)
        => RepositoryOwnedDpoDatasetManifestSealer.OpenManifestIdentityOnly(address);

    private static OperationResult<RepositoryOwnedDpoDatasetResolution>
        OpenExistingOrCreateAndSeal(
            RepositoryOwnedDpoDatasetAddress address,
            Func<RepositoryOwnedDpoDatasetAddress,
                OperationResult<IReadOnlyList<DpoPair>>>? createPairs)
        => RepositoryOwnedDpoDatasetResolver.OpenExistingOrCreateAndSeal(
            address,
            RequestIdentity(),
            createPairs);

    private static OperationResult<RepositoryOwnedDpoDatasetResolution>
        OpenExistingOrCreateAndSeal(
            RepositoryOwnedDpoDatasetAddress address,
            RepositoryOwnedDpoDatasetRequestIdentity identity,
            Func<RepositoryOwnedDpoDatasetAddress,
                OperationResult<IReadOnlyList<DpoPair>>>? createPairs)
        => RepositoryOwnedDpoDatasetResolver.OpenExistingOrCreateAndSeal(
            address,
            identity,
            createPairs);

    private static OperationResult<RepositoryOwnedDpoDatasetResolution>
        OpenExistingOrCreateAndSeal(
            RepositoryOwnedDpoDatasetAddress address,
            Func<RepositoryOwnedDpoDatasetAddress,
                OperationResult<IReadOnlyList<DpoPair>>>? createPairs,
            CancellationToken cancellationToken)
        => RepositoryOwnedDpoDatasetResolver.OpenExistingOrCreateAndSeal(
            address,
            RequestIdentity(),
            createPairs,
            cancellationToken);

    private static OperationResult<RepositoryOwnedDpoDatasetResolution>
        OpenExistingOrCreateAndSeal(
            RepositoryOwnedDpoDatasetAddress address,
            Func<RepositoryOwnedDpoDatasetAddress,
                OperationResult<IReadOnlyList<DpoPair>>>? createPairs,
            CancellationToken cancellationToken,
            TimeSpan creationLeaseTimeout)
        => RepositoryOwnedDpoDatasetResolver.OpenExistingOrCreateAndSeal(
            address,
            RequestIdentity(),
            createPairs,
            cancellationToken,
            creationLeaseTimeout);

    private static Task<OperationResult<RepositoryOwnedDpoDatasetResolution>>
        OpenExistingOrCreateAndSealAsync(
            RepositoryOwnedDpoDatasetAddress address,
            Func<RepositoryOwnedDpoDatasetAddress, CancellationToken,
                Task<OperationResult<IReadOnlyList<DpoPair>>>>? createPairsAsync)
        => RepositoryOwnedDpoDatasetResolver.OpenExistingOrCreateAndSealAsync(
            address,
            RequestIdentity(),
            createPairsAsync);

    private static Task<OperationResult<RepositoryOwnedDpoDatasetResolution>>
        OpenExistingOrCreateAndSealAsync(
            RepositoryOwnedDpoDatasetAddress address,
            RepositoryOwnedDpoDatasetRequestIdentity identity,
            Func<RepositoryOwnedDpoDatasetAddress, CancellationToken,
                Task<OperationResult<IReadOnlyList<DpoPair>>>>? createPairsAsync)
        => RepositoryOwnedDpoDatasetResolver.OpenExistingOrCreateAndSealAsync(
            address,
            identity,
            createPairsAsync);

    private static Task<OperationResult<RepositoryOwnedDpoDatasetResolution>>
        OpenExistingOrCreateAndSealAsync(
            RepositoryOwnedDpoDatasetAddress address,
            Func<RepositoryOwnedDpoDatasetAddress, CancellationToken,
                Task<OperationResult<IReadOnlyList<DpoPair>>>>? createPairsAsync,
            CancellationToken cancellationToken)
        => RepositoryOwnedDpoDatasetResolver.OpenExistingOrCreateAndSealAsync(
            address,
            RequestIdentity(),
            createPairsAsync,
            cancellationToken);

    private static Task<OperationResult<RepositoryOwnedDpoDatasetResolution>>
        OpenExistingOrCreateAndSealAsync(
            RepositoryOwnedDpoDatasetAddress address,
            Func<RepositoryOwnedDpoDatasetAddress, CancellationToken,
                Task<OperationResult<IReadOnlyList<DpoPair>>>>? createPairsAsync,
            CancellationToken cancellationToken,
            TimeSpan creationLeaseTimeout)
        => RepositoryOwnedDpoDatasetResolver.OpenExistingOrCreateAndSealAsync(
            address,
            RequestIdentity(),
            createPairsAsync,
            cancellationToken,
            creationLeaseTimeout);

    private static TaskCompletionSource<bool> AsyncSignal()
        => new(TaskCreationOptions.RunContinuationsAsynchronously);

    private static RepositoryOwnedDpoDatasetRequestIdentity RequestIdentity(
        IReadOnlyList<string>? paidReferenceHashes = null,
        string? teacherArtifactSha256 = null,
        string? generationConfigSha256 = null,
        string? creatorConfigSha256 = null)
        => AssertSuccess(
            RepositoryOwnedDpoDatasetManifestSealer.CreateRequestIdentity(
                paidReferenceHashes
                ?? [Sha256Text("paid-reference-answer-default")],
                new RepositoryOwnedDpoOpenTeacherIdentity(
                    OpenWeightTeacherFamily.Qwen.ToString(),
                    "qwen3.5:9b",
                    ManifestDigest,
                    LegalTrainingTeacherAdmissionAuthority
                        .OllamaModelLayerSha256DigestKind,
                    teacherArtifactSha256 ?? LayerDigest),
                new RepositoryOwnedDpoGenerationPolicyIdentity(
                    "repository-owned-dpo-test-generation",
                    "v1",
                    generationConfigSha256 ?? new string('2', 64)),
                new RepositoryOwnedDpoCreatorIdentity(
                    "repository-owned-dpo-test-creator",
                    "v1",
                    creatorConfigSha256 ?? new string('3', 64))));

    private static RepositoryOwnedDpoDatasetRequestIdentity
        CrossProcessDpoResolverRequestIdentity()
        => RequestIdentity();

    private static RepositoryOwnedDpoDatasetRequestIdentity MutatedIdentity(
        string mutation)
        => mutation switch
        {
            "paid-reference" => RequestIdentity(
                [Sha256Text("paid-reference-answer-mutated")]),
            "teacher-artifact" => RequestIdentity(
                teacherArtifactSha256: new string('4', 64)),
            "generation-policy" => RequestIdentity(
                generationConfigSha256: new string('5', 64)),
            "creator-config" => RequestIdentity(
                creatorConfigSha256: new string('6', 64)),
            _ => throw new ArgumentOutOfRangeException(
                nameof(mutation),
                mutation,
                "Unknown request identity mutation."),
        };

    private static DpoPair BoundLocalPair(
        string name,
        string domain,
        string prompt,
        string chosen,
        string rejected,
        DateTimeOffset capturedAt)
    {
        var passport = OpenTeacherPassport(name);
        return AssertSuccess(DpoPairTrainingContentAuthority.Bind(new DpoPair(
            prompt,
            chosen,
            rejected,
            domain,
            capturedAt)
        {
            TeacherProvenance = passport,
            GovernanceDecision = Governance('a'),
            ChosenAuthorship = new DpoPairSideAuthorship(
                DpoPairSideAuthorKind.OpenTeacher,
                passport),
            RejectedAuthorship = new DpoPairSideAuthorship(
                DpoPairSideAuthorKind.LocalModel),
        }));
    }

    private static DpoPair BoundOpenTeacherRejectedPair(
        string name,
        string domain,
        string prompt,
        string chosen,
        string rejected,
        DateTimeOffset capturedAt)
    {
        var passport = OpenTeacherPassport(name);
        return AssertSuccess(DpoPairTrainingContentAuthority.Bind(new DpoPair(
            prompt,
            chosen,
            rejected,
            domain,
            capturedAt)
        {
            TeacherProvenance = passport,
            GovernanceDecision = Governance('b'),
            ChosenAuthorship = new DpoPairSideAuthorship(
                DpoPairSideAuthorKind.User),
            RejectedAuthorship = new DpoPairSideAuthorship(
                DpoPairSideAuthorKind.OpenTeacher,
                passport),
        }));
    }

    private static TrainingTeacherProvenance OpenTeacherPassport(string requestId)
        => new(
            TrainingTeacherSourceKind.ExternalTeacherOutput,
            OpenWeightTeacherFamily.Qwen,
            TrainingTeacherAccessMode.LocalHostedOpenWeight,
            TrainingTeacherHostingProvider.Ollama,
            "qwen3.5:9b",
            ManifestDigest,
            TrainingTeacherModelLicense.Apache20,
            ProviderTermsChecked: false,
            ProviderTermsAllowTraining: false,
            ClosedProprietaryModel: false,
            TeacherOutputUsedForTraining: true,
            RequestId(requestId),
            InvocationCount: 1)
        {
            ModelArtifactDigestKind = LegalTrainingTeacherAdmissionAuthority
                .OllamaModelLayerSha256DigestKind,
            ModelArtifactDigest = LayerDigest,
        };

    private static TrainingDataGovernanceDecision Governance(char evidence)
        => new(
            licenseAccepted: true,
            isQuarantined: false,
            [$"sha256:{new string(evidence, 64)}"]);

    private static string PairId(DpoPair pair)
        => FileDpoPairStore.ComputeIdentity(pair, schemaVersion: 5);

    private static string PairStoreLockPath(
        RepositoryOwnedDpoDatasetAddress address)
        => Path.Combine(address.PairsRoot, ".dpo-pair-store.lock");

    private static string DatasetSealLockPath(
        RepositoryOwnedDpoDatasetAddress address)
        => Path.Combine(address.DatasetRoot, ".dpo-dataset-seal.lock");

    private static string PairFilePath(
        RepositoryOwnedDpoDatasetAddress address,
        RepositoryOwnedDpoDatasetManifestPair pair)
        => Path.Combine(
            address.RepositoryRoot,
            pair.RepositoryRelativePairFilePath.Replace(
                '/',
                Path.DirectorySeparatorChar));

    private static void DeletePairStoreLock(
        RepositoryOwnedDpoDatasetAddress address)
    {
        var path = PairStoreLockPath(address);
        if (File.Exists(path))
            File.Delete(path);
    }

    private static void DeleteDatasetSealLock(
        RepositoryOwnedDpoDatasetAddress address)
    {
        var path = DatasetSealLockPath(address);
        if (File.Exists(path))
            File.Delete(path);
    }

    private static IReadOnlyList<string> SnapshotRelativeFileSystemEntries(
        string root)
        => Directory.Exists(root)
            ? Directory.EnumerateFileSystemEntries(
                    root,
                    "*",
                    SearchOption.AllDirectories)
                .Select(path => Path.GetRelativePath(root, path)
                    .Replace('\\', '/'))
                .Order(StringComparer.Ordinal)
                .ToArray()
            : [];

    private static IReadOnlyDictionary<string, PairFileSnapshot> SnapshotPairFiles(
        RepositoryOwnedDpoDatasetAddress address)
        => Directory.GetFiles(
                address.PairsRoot,
                "*.dpo-pair.json",
                SearchOption.TopDirectoryOnly)
            .Select(path =>
            {
                var bytes = File.ReadAllBytes(path);
                return new PairFileSnapshot(
                    PairFileName(path),
                    bytes,
                    Sha256(bytes),
                    File.GetLastWriteTimeUtc(path));
            })
            .ToDictionary(
                static snapshot => snapshot.FileName,
                static snapshot => snapshot,
                StringComparer.Ordinal);

    private static void AssertPairFileSnapshotsUnchanged(
        RepositoryOwnedDpoDatasetAddress address,
        IReadOnlyDictionary<string, PairFileSnapshot> snapshots)
    {
        var currentPaths = Directory.GetFiles(
                address.PairsRoot,
                "*.dpo-pair.json",
                SearchOption.TopDirectoryOnly)
            .ToDictionary(
                static path => PairFileName(path),
                static path => path,
                StringComparer.Ordinal);
        Assert.Equal(
            snapshots.Keys.OrderBy(static key => key, StringComparer.Ordinal),
            currentPaths.Keys.OrderBy(static key => key, StringComparer.Ordinal));
        foreach (var (fileName, snapshot) in snapshots)
        {
            var currentPath = currentPaths[fileName];
            var currentBytes = File.ReadAllBytes(currentPath);
            Assert.Equal(snapshot.Bytes, currentBytes);
            Assert.Equal(snapshot.Sha256, Sha256(currentBytes));
            Assert.Equal(
                snapshot.LastWriteTimeUtc,
                File.GetLastWriteTimeUtc(currentPath));
        }
    }

    private sealed record CrossProcessDpoResolverPairFile(
        string PairFileName,
        string PairFileSha256);

    private sealed record CrossProcessDpoResolverVstestChildResult(
        bool IsSuccess,
        string Scenario,
        int ProcessId,
        string? Source,
        string? ErrorCode,
        string? ErrorMessage,
        string? RequestIdentitySha256,
        int CreatorCalls,
        int PairCount,
        string? ManifestSha256,
        IReadOnlyList<CrossProcessDpoResolverPairFile> PairFiles);

    private static async Task RunCrossProcessDpoResolverVstestChildAsync()
    {
        // The result path is the only child-created protocol file outside the
        // temporary repository. It is written atomically so the parent never
        // reads a partial JSON packet from a live child process.
        var resultPath = Environment.GetEnvironmentVariable(
            CrossProcessDpoResolverResultEnvironmentName);
        if (string.IsNullOrWhiteSpace(resultPath))
        {
            throw new InvalidOperationException(
                $"Missing child environment {CrossProcessDpoResolverResultEnvironmentName}.");
        }

        try
        {
            var result = await ExecuteCrossProcessDpoResolverVstestChildAsync();
            await WriteCrossProcessDpoResolverChildResultAsync(
                resultPath,
                result);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            var scenario = Environment.GetEnvironmentVariable(
                CrossProcessDpoResolverScenarioEnvironmentName)
                           ?? "unknown";
            await WriteCrossProcessDpoResolverChildResultAsync(
                resultPath,
                new CrossProcessDpoResolverVstestChildResult(
                    IsSuccess: false,
                    scenario,
                    Environment.ProcessId,
                    Source: null,
                    "CHILD_UNHANDLED_EXCEPTION",
                    exception.ToString(),
                    RequestIdentitySha256: null,
                    CreatorCalls: 0,
                    PairCount: 0,
                    ManifestSha256: null,
                    PairFiles: []));
        }
        finally
        {
            foreach (var environmentName
                     in CrossProcessDpoResolverChildEnvironmentNames)
            {
                Environment.SetEnvironmentVariable(environmentName, null);
            }
        }
    }

    private static async Task<CrossProcessDpoResolverVstestChildResult>
        ExecuteCrossProcessDpoResolverVstestChildAsync()
    {
        var fixtureRoot = RequiredCrossProcessDpoResolverChildEnvironment(
            CrossProcessDpoResolverFixtureRootEnvironmentName);
        var repositoryRoot = RequiredCrossProcessDpoResolverChildEnvironment(
            CrossProcessDpoResolverRepositoryRootEnvironmentName);
        var domainSlug = RequiredCrossProcessDpoResolverChildEnvironment(
            CrossProcessDpoResolverDomainEnvironmentName);
        var designPatternSlug = RequiredCrossProcessDpoResolverChildEnvironment(
            CrossProcessDpoResolverPatternEnvironmentName);
        var datasetVersion = RequiredCrossProcessDpoResolverChildEnvironment(
            CrossProcessDpoResolverVersionEnvironmentName);
        var scenario = RequiredCrossProcessDpoResolverChildEnvironment(
            CrossProcessDpoResolverScenarioEnvironmentName);
        var coordinationDirectory =
            RequiredCrossProcessDpoResolverChildEnvironment(
                CrossProcessDpoResolverCoordinationEnvironmentName);
        var resultPath = RequiredCrossProcessDpoResolverChildEnvironment(
            CrossProcessDpoResolverResultEnvironmentName);
        ValidateCrossProcessDpoResolverChildPaths(
            fixtureRoot,
            repositoryRoot,
            coordinationDirectory,
            resultPath);

        var identity =
            DecodeCrossProcessDpoResolverChildJson<
                RepositoryOwnedDpoDatasetRequestIdentity>(
                RequiredCrossProcessDpoResolverChildEnvironment(
                    CrossProcessDpoResolverIdentityEnvironmentName));
        var address = AssertSuccess(
            RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(
                repositoryRoot,
                domainSlug,
                designPatternSlug,
                datasetVersion));

        await WriteCrossProcessDpoResolverMarkerAsync(
            coordinationDirectory,
            "resolver-entered",
            scenario,
            resultPath,
            new
            {
                ProcessId = Environment.ProcessId,
                Scenario = scenario,
                Address = address.DatasetRootRelativePath,
                identity.RequestIdentitySha256,
            });

        var creatorCalls = 0;
        Task<OperationResult<IReadOnlyList<DpoPair>>> Creator(
            RepositoryOwnedDpoDatasetAddress createAddress,
            CancellationToken cancellationToken)
        {
            return CreateCrossProcessDpoResolverPairsAsync(
                createAddress,
                coordinationDirectory,
                scenario,
                resultPath,
                () => Interlocked.Increment(ref creatorCalls),
                cancellationToken);
        }

        Task<OperationResult<IReadOnlyList<DpoPair>>> UnexpectedCreator(
            RepositoryOwnedDpoDatasetAddress unusedAddress,
            CancellationToken unusedCancellationToken)
        {
            Interlocked.Increment(ref creatorCalls);
            return Task.FromResult(
                OperationResult<IReadOnlyList<DpoPair>>.Fail(
                    "The cross-process resolver child reached a creator in a reuse/fail-closed scenario.",
                    "UNEXPECTED_CREATOR_CALL"));
        }

        Func<RepositoryOwnedDpoDatasetAddress, CancellationToken,
            Task<OperationResult<IReadOnlyList<DpoPair>>>> createPairs =
            scenario switch
        {
            CrossProcessDpoResolverConcurrentScenario => Creator,
            CrossProcessDpoResolverReuseScenario => UnexpectedCreator,
            CrossProcessDpoResolverIdentityMismatchScenario => UnexpectedCreator,
            _ => throw new InvalidOperationException(
                $"Unknown cross-process DPO resolver scenario '{scenario}'."),
        };
        var resolved =
            await RepositoryOwnedDpoDatasetResolver
                .OpenExistingOrCreateAndSealAsync(
                    address,
                    identity,
                    createPairs);
        return BuildCrossProcessDpoResolverChildResult(
            scenario,
            identity,
            Volatile.Read(ref creatorCalls),
            resolved);
    }

    private static async Task<OperationResult<IReadOnlyList<DpoPair>>>
        CreateCrossProcessDpoResolverPairsAsync(
            RepositoryOwnedDpoDatasetAddress address,
            string coordinationDirectory,
            string scenario,
            string resultPath,
            Func<int> recordCreatorCall,
            CancellationToken cancellationToken)
    {
        var creatorCall = recordCreatorCall();
        await WriteCrossProcessDpoResolverMarkerAsync(
            coordinationDirectory,
            "creator-entered",
            scenario,
            resultPath,
            new
            {
                ProcessId = Environment.ProcessId,
                Scenario = scenario,
                CreatorCall = creatorCall,
                Address = address.DatasetRootRelativePath,
            });
        await WaitForCrossProcessDpoResolverReleaseAsync(
            coordinationDirectory,
            TimeSpan.FromSeconds(60),
            cancellationToken);
        return OperationResult<IReadOnlyList<DpoPair>>.Ok(
            [CrossProcessDpoResolverPair(address.DomainSlug)]);
    }

    private static DpoPair CrossProcessDpoResolverPair(string domain)
        => BoundLocalPair(
            "cross-process-dpo-resolver",
            domain,
            "public prompt for cross-process resolver reuse",
            "public chosen answer from deterministic fake creator",
            "public rejected answer from deterministic fake creator",
            DateTimeOffset.UnixEpoch.AddSeconds(42));

    private static Process StartCrossProcessDpoResolverVstestChild(
        string fixtureRoot,
        RepositoryOwnedDpoDatasetAddress address,
        RepositoryOwnedDpoDatasetRequestIdentity identity,
        string coordinationDirectory,
        string resultPath,
        string scenario)
    {
        ValidateCrossProcessDpoResolverParentPaths(
            fixtureRoot,
            address.RepositoryRoot,
            coordinationDirectory,
            resultPath);
        var startInfo =
            new ProcessStartInfo(ResolveDotnetHostForCrossProcessDpoResolverChild())
            {
                WorkingDirectory =
                    Path.GetDirectoryName(FindRepositoryFile(
                        "LlmFromScratch.sln"))!,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };
        startInfo.ArgumentList.Add("vstest");
        startInfo.ArgumentList.Add(
            ResolveCurrentCrossProcessDpoResolverTestAssemblyPath());
        startInfo.ArgumentList.Add(
            "--TestCaseFilter:FullyQualifiedName~AsyncResolver_GitCloneStyleTrackedLeaseVstestChildProcessesReserveOnceReuseDiskAndFailClosed");
        startInfo.ArgumentList.Add("--Logger:Console;Verbosity=minimal");

        foreach (var environmentName
                 in CrossProcessDpoResolverChildEnvironmentNames)
        {
            startInfo.Environment.Remove(environmentName);
        }

        startInfo.Environment[
            CrossProcessDpoResolverChildModeEnvironmentName] = "1";
        startInfo.Environment[
            CrossProcessDpoResolverFixtureRootEnvironmentName] =
            fixtureRoot;
        startInfo.Environment[
            CrossProcessDpoResolverRepositoryRootEnvironmentName] =
            address.RepositoryRoot;
        startInfo.Environment[
            CrossProcessDpoResolverDomainEnvironmentName] =
            address.DomainSlug;
        startInfo.Environment[
            CrossProcessDpoResolverPatternEnvironmentName] =
            address.DesignPatternSlug;
        startInfo.Environment[
            CrossProcessDpoResolverVersionEnvironmentName] =
            address.DatasetVersion;
        startInfo.Environment[
            CrossProcessDpoResolverScenarioEnvironmentName] =
            scenario;
        startInfo.Environment[
            CrossProcessDpoResolverIdentityEnvironmentName] =
            EncodeCrossProcessDpoResolverChildJson(identity);
        startInfo.Environment[
            CrossProcessDpoResolverCoordinationEnvironmentName] =
            coordinationDirectory;
        startInfo.Environment[
            CrossProcessDpoResolverResultEnvironmentName] =
            resultPath;

        var process = Process.Start(startInfo);
        Assert.NotNull(process);
        return process;
    }

    private static async Task<CrossProcessDpoResolverVstestChildResult>
        WaitForCrossProcessDpoResolverChildAsync(
            Process process,
            string resultPath,
            TimeSpan timeout)
    {
        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();
        using var timeoutSource = new CancellationTokenSource(timeout);
        try
        {
            await process.WaitForExitAsync(timeoutSource.Token);
        }
        catch (OperationCanceledException)
        {
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch (InvalidOperationException)
            {
            }

            await process.WaitForExitAsync(CancellationToken.None);
            var timeoutStdout = await stdoutTask;
            var timeoutStderr = await stderrTask;
            Assert.Fail(
                $"Cross-process DPO resolver child timed out after {timeout}. stdout={timeoutStdout}; stderr={timeoutStderr}");
        }

        var stdout = await stdoutTask;
        var stderr = await stderrTask;
        Assert.Equal(0, process.ExitCode);
        Assert.True(
            File.Exists(resultPath),
            $"Cross-process DPO resolver child exited without result. stdout={stdout}; stderr={stderr}");
        try
        {
            var json = await File.ReadAllTextAsync(resultPath);
            var result = JsonSerializer.Deserialize<
                CrossProcessDpoResolverVstestChildResult>(json);
            Assert.NotNull(result);
            return result!;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            throw new InvalidOperationException(
                $"Cross-process DPO resolver child result was unreadable. stdout={stdout}; stderr={stderr}",
                exception);
        }
    }

    private static async Task WaitForCrossProcessDpoResolverMarkerCountAsync(
        string coordinationDirectory,
        string searchPattern,
        int expectedCount,
        TimeSpan timeout)
    {
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < timeout)
        {
            var count = CountCrossProcessDpoResolverMarkers(
                coordinationDirectory,
                searchPattern);
            if (count == expectedCount)
                return;
            if (count > expectedCount)
            {
                Assert.Fail(
                    $"Expected {expectedCount} cross-process DPO resolver marker(s) matching {searchPattern}, but found {count}.");
            }

            await Task.Delay(TimeSpan.FromMilliseconds(50));
        }

        Assert.Fail(
            $"Timed out after {timeout} waiting for {expectedCount} cross-process DPO resolver marker(s) matching {searchPattern}; found {CountCrossProcessDpoResolverMarkers(coordinationDirectory, searchPattern)}.");
    }

    private static async Task WaitForExactlyOneCrossProcessDpoResolverCreatorAsync(
        string coordinationDirectory,
        TimeSpan timeout,
        params (string Label, Process Process, string ResultPath)[] children)
    {
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < timeout)
        {
            var count = CountCrossProcessDpoResolverMarkers(
                coordinationDirectory,
                "creator-entered.*.json");
            if (count == 1)
                return;
            if (count > 1)
            {
                Assert.Fail(
                    $"Expected exactly one cross-process DPO resolver creator marker, but found {count}.");
            }

            var completedChildren = children
                .Where(static child =>
                    CrossProcessDpoResolverChildExited(child.Process)
                    || File.Exists(child.ResultPath))
                .ToArray();
            if (completedChildren.Length != 0)
            {
                var diagnostics =
                    await CollectCrossProcessDpoResolverChildDiagnosticsAsync(
                        children);
                Assert.Fail(
                    "Cross-process DPO resolver child exited or wrote a result before any creator marker appeared."
                    + Environment.NewLine
                    + "Observed child(s): "
                    + string.Join(
                        ", ",
                        completedChildren.Select(static child => child.Label))
                    + Environment.NewLine
                    + diagnostics);
            }

            await Task.Delay(TimeSpan.FromMilliseconds(50));
        }

        Assert.Fail(
            $"Timed out after {timeout} waiting for exactly one cross-process DPO resolver creator marker; found {CountCrossProcessDpoResolverMarkers(coordinationDirectory, "creator-entered.*.json")}."
            + Environment.NewLine
            + await CollectCrossProcessDpoResolverChildDiagnosticsAsync(children));
    }

    private static bool CrossProcessDpoResolverChildExited(Process process)
    {
        try
        {
            return process.HasExited;
        }
        catch (InvalidOperationException)
        {
            return true;
        }
    }

    private static async Task<string>
        CollectCrossProcessDpoResolverChildDiagnosticsAsync(
            IReadOnlyList<(string Label, Process Process, string ResultPath)>
                children)
    {
        var builder = new StringBuilder();
        foreach (var child in children)
        {
            await AppendCrossProcessDpoResolverChildDiagnosticsAsync(
                builder,
                child.Label,
                child.Process,
                child.ResultPath);
        }

        return builder.ToString();
    }

    private static async Task AppendCrossProcessDpoResolverChildDiagnosticsAsync(
        StringBuilder builder,
        string label,
        Process process,
        string resultPath)
    {
        // WHY: this is a failure-only path. If a child dies before the creator
        // marker, the parent must not keep waiting blindly; it terminates the
        // remaining child and reports the exact child packet plus process I/O.
        var hadExited = CrossProcessDpoResolverChildExited(process);
        if (!hadExited)
        {
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch (InvalidOperationException)
            {
            }

            await process.WaitForExitAsync(CancellationToken.None);
        }

        var stdout = await process.StandardOutput.ReadToEndAsync();
        var stderr = await process.StandardError.ReadToEndAsync();
        var resultJson = File.Exists(resultPath)
            ? await File.ReadAllTextAsync(resultPath)
            : "<missing>";
        builder.AppendLine(
            $"child={label}; pid={ProcessIdOrUnknown(process)}; hadExited={hadExited}; exitCode={ExitCodeOrUnknown(process)}; resultPath={resultPath}");
        builder.AppendLine("resultJson=");
        builder.AppendLine(resultJson);
        builder.AppendLine("stdout=");
        builder.AppendLine(stdout);
        builder.AppendLine("stderr=");
        builder.AppendLine(stderr);
    }

    private static string ProcessIdOrUnknown(Process process)
    {
        try
        {
            return process.Id.ToString();
        }
        catch (InvalidOperationException)
        {
            return "<unknown>";
        }
    }

    private static string ExitCodeOrUnknown(Process process)
    {
        try
        {
            return process.HasExited
                ? process.ExitCode.ToString()
                : "<still-running>";
        }
        catch (InvalidOperationException)
        {
            return "<unknown>";
        }
    }

    private static int CountCrossProcessDpoResolverMarkers(
        string coordinationDirectory,
        string searchPattern)
        => Directory.Exists(coordinationDirectory)
            ? Directory.GetFiles(
                    coordinationDirectory,
                    searchPattern,
                    SearchOption.TopDirectoryOnly)
                .Length
            : 0;

    private static void ReleaseCrossProcessDpoResolverCreator(
        string coordinationDirectory)
    {
        Directory.CreateDirectory(coordinationDirectory);
        File.WriteAllText(
            Path.Combine(
                coordinationDirectory,
                CrossProcessDpoResolverReleaseFileName),
            "release");
    }

    private static async Task WaitForCrossProcessDpoResolverReleaseAsync(
        string coordinationDirectory,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        var releasePath = Path.Combine(
            coordinationDirectory,
            CrossProcessDpoResolverReleaseFileName);
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < timeout)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (File.Exists(releasePath))
                return;
            await Task.Delay(TimeSpan.FromMilliseconds(50), cancellationToken);
        }

        throw new TimeoutException(
            $"Timed out after {timeout} waiting for cross-process DPO resolver creator release.");
    }

    private static void AssertSingleCrossProcessDpoResolverPairFile(
        CrossProcessDpoResolverVstestChildResult result)
    {
        var pairFile = Assert.Single(result.PairFiles);
        Assert.EndsWith(
            ".dpo-pair.json",
            pairFile.PairFileName,
            StringComparison.Ordinal);
        Assert.Equal(64, pairFile.PairFileSha256.Length);
        Assert.All(
            pairFile.PairFileSha256,
            value => Assert.True(Uri.IsHexDigit(value)));
    }

    private static void AssertNoCrossProcessDpoResolverTemporaryProtocolFiles(
        params string[] protocolDirectories)
    {
        foreach (var protocolDirectory in protocolDirectories)
        {
            if (!Directory.Exists(protocolDirectory))
                continue;

            var temporaryFiles = Directory
                .GetFiles(protocolDirectory, "*.tmp", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(
                    protocolDirectory,
                    ".*.tmp",
                    SearchOption.AllDirectories))
                .Distinct(StringComparerForCurrentFileSystem)
                .Order(StringComparer.Ordinal)
                .ToArray();
            Assert.Empty(temporaryFiles);
        }
    }

    private static CrossProcessDpoResolverVstestChildResult
        BuildCrossProcessDpoResolverChildResult(
            string scenario,
            RepositoryOwnedDpoDatasetRequestIdentity identity,
            int creatorCalls,
            OperationResult<RepositoryOwnedDpoDatasetResolution> resolution)
    {
        if (!resolution.IsSuccess || resolution.Value is null)
        {
            return new CrossProcessDpoResolverVstestChildResult(
                IsSuccess: false,
                scenario,
                Environment.ProcessId,
                Source: null,
                resolution.ErrorCode,
                resolution.ErrorMessage,
                identity.RequestIdentitySha256,
                creatorCalls,
                PairCount: 0,
                ManifestSha256: null,
                PairFiles: []);
        }

        var dataset = resolution.Value.Dataset;
        return new CrossProcessDpoResolverVstestChildResult(
            IsSuccess: true,
            scenario,
            Environment.ProcessId,
            resolution.Value.Source.ToString(),
            ErrorCode: null,
            ErrorMessage: null,
            identity.RequestIdentitySha256,
            creatorCalls,
            dataset.Pairs.Count,
            dataset.Manifest.ManifestSha256,
            dataset.Manifest.OrderedPairs
                .Select(static pair => new CrossProcessDpoResolverPairFile(
                    PairFileName(pair.RepositoryRelativePairFilePath),
                    pair.PairFileSha256))
                .ToArray());
    }

    private static async Task WriteCrossProcessDpoResolverMarkerAsync(
        string coordinationDirectory,
        string kind,
        string scenario,
        string resultPath,
        object payload)
    {
        Directory.CreateDirectory(coordinationDirectory);
        var markerPath = Path.Combine(
            coordinationDirectory,
            $"{kind}.{scenario}.{Environment.ProcessId}.{Sha256Text(resultPath)[..12]}.json");
        await using var stream = new FileStream(
            markerPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.Read);
        await JsonSerializer.SerializeAsync(
            stream,
            payload,
            new JsonSerializerOptions { WriteIndented = true });
    }

    private static async Task WriteCrossProcessDpoResolverChildResultAsync(
        string resultPath,
        CrossProcessDpoResolverVstestChildResult result)
    {
        var fullResultPath = Path.GetFullPath(resultPath);
        var resultDirectory = Path.GetDirectoryName(fullResultPath);
        if (string.IsNullOrWhiteSpace(resultDirectory))
            throw new InvalidOperationException("Child result path is invalid.");
        Directory.CreateDirectory(resultDirectory);
        var temporaryPath = Path.Combine(
            resultDirectory,
            $".{Path.GetFileName(fullResultPath)}.{Guid.NewGuid():N}.tmp");
        await File.WriteAllTextAsync(
            temporaryPath,
            JsonSerializer.Serialize(
                result,
                new JsonSerializerOptions { WriteIndented = true }),
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        File.Move(temporaryPath, fullResultPath, overwrite: true);
    }

    private static string RequiredCrossProcessDpoResolverChildEnvironment(
        string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"Missing child environment {name}.");
        }

        return value;
    }

    private static string EncodeCrossProcessDpoResolverChildJson<T>(T value)
        => Convert.ToBase64String(Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(value)));

    private static T DecodeCrossProcessDpoResolverChildJson<T>(string encoded)
        => JsonSerializer.Deserialize<T>(
               Encoding.UTF8.GetString(Convert.FromBase64String(encoded)))
           ?? throw new InvalidOperationException(
               $"Could not decode {typeof(T).Name} child JSON.");

    private static void ValidateCrossProcessDpoResolverParentPaths(
        string fixtureRoot,
        string repositoryRoot,
        string coordinationDirectory,
        string resultPath)
    {
        var fixtureFullPath = Path.GetFullPath(fixtureRoot);
        var repositoryFullPath = Path.GetFullPath(repositoryRoot);
        var coordinationFullPath = Path.GetFullPath(coordinationDirectory);
        var resultFullPath = Path.GetFullPath(resultPath);
        Assert.True(
            IsSameOrDescendantPath(repositoryFullPath, fixtureFullPath),
            "The temporary artifact repository must be inside the test fixture root.");
        Assert.True(
            IsSameOrDescendantPath(coordinationFullPath, fixtureFullPath),
            "The cross-process coordination directory must stay inside the test fixture root.");
        Assert.True(
            IsSameOrDescendantPath(resultFullPath, fixtureFullPath),
            "The child result must stay inside the test fixture root.");
        Assert.False(
            IsSameOrDescendantPath(coordinationFullPath, repositoryFullPath),
            "Coordination markers are parent/child protocol files, not dataset files.");
        Assert.False(
            IsSameOrDescendantPath(resultFullPath, repositoryFullPath),
            "The child result is protocol output outside the artifact repository.");
    }

    private static void ValidateCrossProcessDpoResolverChildPaths(
        string fixtureRoot,
        string repositoryRoot,
        string coordinationDirectory,
        string resultPath)
    {
        ValidateCrossProcessDpoResolverParentPaths(
            fixtureRoot,
            repositoryRoot,
            coordinationDirectory,
            resultPath);
        if (!Directory.Exists(Path.GetFullPath(repositoryRoot)))
        {
            throw new DirectoryNotFoundException(
                $"Cross-process DPO resolver repository root is missing: {repositoryRoot}");
        }
    }

    private static bool IsSameOrDescendantPath(string candidate, string root)
    {
        var candidateFullPath = Path.GetFullPath(candidate);
        var rootFullPath = Path.GetFullPath(root);
        if (PathsEqual(candidateFullPath, rootFullPath))
            return true;
        var relative = Path.GetRelativePath(rootFullPath, candidateFullPath);
        return !string.IsNullOrWhiteSpace(relative)
               && !string.Equals(relative, ".", StringComparison.Ordinal)
               && !relative.StartsWith("..", StringComparison.Ordinal)
               && !Path.IsPathRooted(relative);
    }

    private static bool PathsEqual(string first, string second)
        => StringComparerForCurrentFileSystem.Equals(
            Path.GetFullPath(first).TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar),
            Path.GetFullPath(second).TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar));

    private static StringComparer StringComparerForCurrentFileSystem
        => OperatingSystem.IsWindows()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;

    private static string ResolveCurrentCrossProcessDpoResolverTestAssemblyPath()
    {
        var assemblyLocation =
            typeof(RepositoryOwnedDpoDatasetManifestTests).Assembly.Location;
        if (!string.IsNullOrWhiteSpace(assemblyLocation)
            && File.Exists(assemblyLocation))
        {
            return assemblyLocation;
        }

        var candidate = Path.Combine(
            AppContext.BaseDirectory,
            "LlmFromScratch.Tests.dll");
        if (File.Exists(candidate))
            return candidate;

        throw new FileNotFoundException(
            "Could not resolve the current test assembly for cross-process DPO resolver child vstest.");
    }

    private static string ResolveDotnetHostForCrossProcessDpoResolverChild()
    {
        foreach (var environmentName in new[] { "DOTNET_HOST_PATH", "DOTNET_EXE" })
        {
            var configuredHost = Environment.GetEnvironmentVariable(
                environmentName);
            if (!string.IsNullOrWhiteSpace(configuredHost)
                && File.Exists(configuredHost))
            {
                return configuredHost;
            }
        }

        return OperatingSystem.IsWindows() ? "dotnet.exe" : "dotnet";
    }

    private static string FindRepositoryFile(params string[] relativePath)
    {
        foreach (var start in new[]
                 {
                     Directory.GetCurrentDirectory(),
                     AppContext.BaseDirectory
                 })
        {
            var current = new DirectoryInfo(start);
            while (current is not null)
            {
                var candidate = Path.Combine(
                    [current.FullName, .. relativePath]);
                if (File.Exists(candidate))
                    return candidate;
                current = current.Parent;
            }
        }

        throw new FileNotFoundException(
            $"Could not find repository file '{Path.Combine(relativePath)}'.");
    }

    private static async Task<T> AssertCompletesWithin<T>(
        Task<T> task,
        TimeSpan timeout)
    {
        var completed = await Task.WhenAny(task, Task.Delay(timeout));
        Assert.Same(task, completed);
        return await task;
    }

    private static void AssertExactPairFiles(
        RepositoryOwnedDpoDatasetAddress address,
        IReadOnlyList<DpoPair> expectedPairs)
    {
        var expectedFileNames = expectedPairs
            .Select(pair => PairId(pair) + ".dpo-pair.json")
            .Order(StringComparer.Ordinal)
            .ToArray();
        var actualFileNames = Directory.Exists(address.PairsRoot)
            ? Directory.GetFiles(
                    address.PairsRoot,
                    "*.dpo-pair.json",
                    SearchOption.TopDirectoryOnly)
                .Select(PairFileName)
                .Order(StringComparer.Ordinal)
                .ToArray()
            : [];
        Assert.Equal(expectedFileNames, actualFileNames);
    }

    private static void AssertNoTemporaryFiles(
        RepositoryOwnedDpoDatasetAddress address)
    {
        if (Directory.Exists(address.DatasetRoot))
        {
            Assert.Empty(Directory.GetFiles(
                address.DatasetRoot,
                ".manifest.*.tmp",
                SearchOption.TopDirectoryOnly));
        }

        if (Directory.Exists(address.PairsRoot))
        {
            Assert.Empty(Directory.GetFiles(
                address.PairsRoot,
                "*.tmp",
                SearchOption.TopDirectoryOnly));
        }
    }

    private static void AssertNoCreationLeaseFilesInsideDataset(
        RepositoryOwnedDpoDatasetAddress address)
    {
        if (!Directory.Exists(address.DatasetRoot))
            return;

        Assert.False(Directory.Exists(Path.Combine(
            address.DatasetRoot,
            ".dpo-dataset-creation-leases")));
        Assert.DoesNotContain(
            Directory.EnumerateFileSystemEntries(
                address.DatasetRoot,
                "*",
                SearchOption.AllDirectories),
            path => path.Contains(
                ".dpo-dataset-creation-leases",
                StringComparison.Ordinal));
    }

    private static CreationLeaseTestPaths CreateGitCloneStyleCreationLease(
        RepositoryOwnedDpoDatasetAddress address)
    {
        var trainingArtifactsRoot = Path.Combine(
            address.RepositoryRoot,
            "training-artifacts");
        var leaseRoot = Path.Combine(
            trainingArtifactsRoot,
            ".dpo-dataset-creation-leases");
        var domainLeaseDirectory = Path.Combine(
            leaseRoot,
            address.DomainSlug);
        var patternLeaseDirectory = Path.Combine(
            domainLeaseDirectory,
            address.DesignPatternSlug);
        var lockPath = Path.Combine(
            patternLeaseDirectory,
            address.DatasetVersion + ".lock");

        Directory.CreateDirectory(patternLeaseDirectory);
        File.WriteAllBytes(lockPath, [0]);

        if (OperatingSystem.IsLinux())
        {
            const UnixFileMode gitCloneDirectoryMode =
                UnixFileMode.UserRead |
                UnixFileMode.UserWrite |
                UnixFileMode.UserExecute |
                UnixFileMode.GroupRead |
                UnixFileMode.GroupExecute |
                UnixFileMode.OtherRead |
                UnixFileMode.OtherExecute;
            const UnixFileMode gitCloneFileMode =
                UnixFileMode.UserRead |
                UnixFileMode.UserWrite |
                UnixFileMode.GroupRead |
                UnixFileMode.OtherRead;

            File.SetUnixFileMode(trainingArtifactsRoot, gitCloneDirectoryMode);
            File.SetUnixFileMode(leaseRoot, gitCloneDirectoryMode);
            File.SetUnixFileMode(domainLeaseDirectory, gitCloneDirectoryMode);
            File.SetUnixFileMode(patternLeaseDirectory, gitCloneDirectoryMode);
            File.SetUnixFileMode(lockPath, gitCloneFileMode);
        }

        return new CreationLeaseTestPaths(
            trainingArtifactsRoot,
            leaseRoot,
            domainLeaseDirectory,
            patternLeaseDirectory,
            lockPath);
    }

    [SupportedOSPlatform("linux")]
    private static void AssertGitCloneStyleCreationLeaseLooksLikeFreshClone(
        CreationLeaseTestPaths paths)
    {
        Assert.True(OperatingSystem.IsLinux());
        const UnixFileMode gitCloneDirectoryMode =
            UnixFileMode.UserRead |
            UnixFileMode.UserWrite |
            UnixFileMode.UserExecute |
            UnixFileMode.GroupRead |
            UnixFileMode.GroupExecute |
            UnixFileMode.OtherRead |
            UnixFileMode.OtherExecute;
        const UnixFileMode gitCloneFileMode =
            UnixFileMode.UserRead |
            UnixFileMode.UserWrite |
            UnixFileMode.GroupRead |
            UnixFileMode.OtherRead;

        AssertUnixFileMode(paths.TrainingArtifactsRoot, gitCloneDirectoryMode);
        AssertUnixFileMode(paths.LeaseRoot, gitCloneDirectoryMode);
        AssertUnixFileMode(paths.DomainLeaseDirectory, gitCloneDirectoryMode);
        AssertUnixFileMode(paths.PatternLeaseDirectory, gitCloneDirectoryMode);
        AssertUnixFileMode(paths.LockPath, gitCloneFileMode);
        Assert.Equal([0], File.ReadAllBytes(paths.LockPath));
    }

    [SupportedOSPlatform("linux")]
    private static void AssertGitCloneStyleCreationLeaseNarrowed(
        CreationLeaseTestPaths paths)
    {
        Assert.True(OperatingSystem.IsLinux());
        const UnixFileMode gitCloneDirectoryMode =
            UnixFileMode.UserRead |
            UnixFileMode.UserWrite |
            UnixFileMode.UserExecute |
            UnixFileMode.GroupRead |
            UnixFileMode.GroupExecute |
            UnixFileMode.OtherRead |
            UnixFileMode.OtherExecute;

        // Это главный regression-check: resolver имеет право чинить только
        // чувствительную lease-цепочку и стабильный lock. Корень
        // `training-artifacts` остаётся clone-style 0755.
        AssertUnixFileMode(paths.TrainingArtifactsRoot, gitCloneDirectoryMode);
        AssertOwnerOnlyDirectoryMode(paths.LeaseRoot);
        AssertOwnerOnlyDirectoryMode(paths.DomainLeaseDirectory);
        AssertOwnerOnlyDirectoryMode(paths.PatternLeaseDirectory);
        AssertOwnerOnlyFileMode(paths.LockPath);
        Assert.Equal([0], File.ReadAllBytes(paths.LockPath));
    }

    [SupportedOSPlatform("linux")]
    private static void AssertOwnerOnlyDirectoryMode(string path)
    {
        Assert.True(Directory.Exists(path), path);
        Assert.True(OperatingSystem.IsLinux());
        const UnixFileMode ownerDirectoryMode =
            UnixFileMode.UserRead |
            UnixFileMode.UserWrite |
            UnixFileMode.UserExecute;
        const UnixFileMode groupOrOtherAccess =
            UnixFileMode.GroupRead |
            UnixFileMode.GroupWrite |
            UnixFileMode.GroupExecute |
            UnixFileMode.OtherRead |
            UnixFileMode.OtherWrite |
            UnixFileMode.OtherExecute;

        var mode = File.GetUnixFileMode(path);
        var permissionBits = mode & (UnixFileMode)0x1ff;
        Assert.True(
            (mode & ownerDirectoryMode) == ownerDirectoryMode
            && (mode & groupOrOtherAccess) == 0,
            $"{path} mode was {Convert.ToString((int)permissionBits, 8)}.");
    }

    [SupportedOSPlatform("linux")]
    private static void AssertOwnerOnlyFileMode(string path)
    {
        Assert.True(File.Exists(path), path);
        Assert.True(OperatingSystem.IsLinux());
        const UnixFileMode ownerFileMode =
            UnixFileMode.UserRead |
            UnixFileMode.UserWrite;
        const UnixFileMode groupOrOtherAccess =
            UnixFileMode.GroupRead |
            UnixFileMode.GroupWrite |
            UnixFileMode.GroupExecute |
            UnixFileMode.OtherRead |
            UnixFileMode.OtherWrite |
            UnixFileMode.OtherExecute;

        var mode = File.GetUnixFileMode(path);
        var permissionBits = mode & (UnixFileMode)0x1ff;
        Assert.True(
            (mode & ownerFileMode) == ownerFileMode
            && (mode & groupOrOtherAccess) == 0,
            $"{path} mode was {Convert.ToString((int)permissionBits, 8)}.");
    }

    [SupportedOSPlatform("linux")]
    private static void AssertUnixFileMode(string path, UnixFileMode expected)
    {
        Assert.True(OperatingSystem.IsLinux());
        var actual = File.GetUnixFileMode(path);
        var actualPermissionBits = actual & (UnixFileMode)0x1ff;
        var expectedPermissionBits = expected & (UnixFileMode)0x1ff;
        Assert.Equal(
            expectedPermissionBits,
            actualPermissionBits);
    }

    private static void AssertCreationLeaseTreeContainsOnlyStableLocks(
        params RepositoryOwnedDpoDatasetAddress[] addresses)
    {
        Assert.NotEmpty(addresses);
        var leaseRoot = Path.Combine(
            addresses[0].RepositoryRoot,
            "training-artifacts",
            ".dpo-dataset-creation-leases");
        Assert.True(Directory.Exists(leaseRoot));

        var expected = addresses
            .Select(address => Path.GetFullPath(Path.Combine(
                leaseRoot,
                address.DomainSlug,
                address.DesignPatternSlug,
                address.DatasetVersion + ".lock")))
            .Order(StringComparer.Ordinal)
            .ToArray();
        var actual = Directory.GetFiles(
                leaseRoot,
                "*",
                SearchOption.AllDirectories)
            .Select(Path.GetFullPath)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(expected, actual);
        Assert.All(actual, path =>
        {
            Assert.EndsWith(".lock", path, StringComparison.Ordinal);
            Assert.Equal(1, new FileInfo(path).Length);
            Assert.Equal([0], File.ReadAllBytes(path));
            Assert.DoesNotContain(".tmp", path, StringComparison.Ordinal);
        });
    }

    private static string PairFileName(string path)
        => Path.GetFileName(path)
           ?? throw new InvalidOperationException("Pair file name is missing.");

    private static byte[] CanonicalManifestJson(
        RepositoryOwnedDpoDatasetManifest manifest)
    {
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            writer.WriteString("schema", manifest.Schema);
            writer.WriteString("context", manifest.Context);
            writer.WriteString("domain", manifest.Domain);
            writer.WriteString("designPattern", manifest.DesignPattern);
            writer.WriteString("datasetVersion", manifest.DatasetVersion);
            writer.WritePropertyName("requestIdentity");
            WriteRequestIdentity(writer, manifest.RequestIdentity);
            writer.WriteNumber("pairCount", manifest.PairCount);
            writer.WriteStartArray("orderedPairs");
            foreach (var pair in manifest.OrderedPairs)
            {
                writer.WriteStartObject();
                writer.WriteString("pairId", pair.PairId);
                writer.WriteString(
                    "repositoryRelativePairFilePath",
                    pair.RepositoryRelativePairFilePath);
                writer.WriteString("pairFileSha256", pair.PairFileSha256);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteString("manifestSha256", manifest.ManifestSha256);
            writer.WriteEndObject();
            writer.Flush();
        }

        return buffer.ToArray();
    }

    private static byte[] CanonicalLegacyV1ManifestJson(
        RepositoryOwnedDpoDatasetManifest manifest)
    {
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            writer.WriteString("schema", manifest.Schema);
            writer.WriteString("context", manifest.Context);
            writer.WriteString("domain", manifest.Domain);
            writer.WriteString("designPattern", manifest.DesignPattern);
            writer.WriteString("datasetVersion", manifest.DatasetVersion);
            writer.WriteNumber("pairCount", manifest.PairCount);
            writer.WriteStartArray("orderedPairs");
            foreach (var pair in manifest.OrderedPairs)
            {
                writer.WriteStartObject();
                writer.WriteString("pairId", pair.PairId);
                writer.WriteString(
                    "repositoryRelativePairFilePath",
                    pair.RepositoryRelativePairFilePath);
                writer.WriteString("pairFileSha256", pair.PairFileSha256);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteString("manifestSha256", manifest.ManifestSha256);
            writer.WriteEndObject();
            writer.Flush();
        }

        return buffer.ToArray();
    }

    private static void WriteRequestIdentity(
        Utf8JsonWriter writer,
        RepositoryOwnedDpoDatasetRequestIdentity identity)
    {
        writer.WriteStartObject();
        writer.WriteString("schema", identity.Schema);
        writer.WritePropertyName("paidReferenceAnswers");
        writer.WriteStartObject();
        writer.WriteNumber(
            "answerCount",
            identity.PaidReferenceAnswers.AnswerCount);
        writer.WriteStartArray("orderedAnswerSha256");
        foreach (var answerHash in identity.PaidReferenceAnswers.OrderedAnswerSha256)
            writer.WriteStringValue(answerHash);
        writer.WriteEndArray();
        writer.WriteString(
            "canonicalListSha256",
            identity.PaidReferenceAnswers.CanonicalListSha256);
        writer.WriteEndObject();
        writer.WritePropertyName("openTeacher");
        writer.WriteStartObject();
        writer.WriteString("family", identity.OpenTeacher.Family);
        writer.WriteString("modelId", identity.OpenTeacher.ModelId);
        writer.WriteString("modelVersion", identity.OpenTeacher.ModelVersion);
        writer.WriteString(
            "modelArtifactDigestKind",
            identity.OpenTeacher.ModelArtifactDigestKind);
        writer.WriteString(
            "modelArtifactSha256",
            identity.OpenTeacher.ModelArtifactSha256);
        writer.WriteEndObject();
        writer.WritePropertyName("generationPolicy");
        writer.WriteStartObject();
        writer.WriteString("stableName", identity.GenerationPolicy.StableName);
        writer.WriteString("version", identity.GenerationPolicy.Version);
        writer.WriteString(
            "configSha256",
            identity.GenerationPolicy.ConfigSha256);
        writer.WriteEndObject();
        writer.WritePropertyName("creator");
        writer.WriteStartObject();
        writer.WriteString("stableName", identity.Creator.StableName);
        writer.WriteString("version", identity.Creator.Version);
        writer.WriteString(
            "effectiveConfigSha256",
            identity.Creator.EffectiveConfigSha256);
        writer.WriteEndObject();
        writer.WriteString(
            "requestIdentitySha256",
            identity.RequestIdentitySha256);
        writer.WriteEndObject();
    }

    private static string ReadSha256(string path)
        => Sha256(File.ReadAllBytes(path));

    private static string Sha256(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static string Sha256Text(string value)
        => Sha256(Encoding.UTF8.GetBytes(value));

    private static string RequestId(string value)
        => Sha256Text(value);

    private static T AssertSuccess<T>(OperationResult<T> result)
    {
        Assert.True(result.IsSuccess, result.ErrorMessage);
        return result.Value!;
    }

    private sealed class RepositoryOwnedDpoDatasetManifestFixture : IDisposable
    {
        internal RepositoryOwnedDpoDatasetManifestFixture()
        {
            Root = Path.Combine(
                Path.GetTempPath(),
                "repo-owned-dpo-manifest-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Root);
            RepositoryRoot = Path.Combine(Root, "repo");
            Directory.CreateDirectory(RepositoryRoot);
            CreateRepositorySentinels(RepositoryRoot);
        }

        internal string Root { get; }
        private string RepositoryRoot { get; }

        internal RepositoryOwnedDpoDatasetAddress Address(
            string datasetVersion = "v0001",
            string domainSlug = "bug",
            string designPatternSlug = "learning-unit")
            => AssertSuccess(RepositoryOwnedTrainingArtifactLayout
                .CreateDpoDatasetAddress(
                    RepositoryRoot,
                    domainSlug,
                    designPatternSlug,
                    datasetVersion));

        public void Dispose()
        {
            if (Directory.Exists(Root))
                Directory.Delete(Root, recursive: true);
        }

        private static void CreateRepositorySentinels(string repositoryRoot)
        {
            File.WriteAllText(
                Path.Combine(repositoryRoot, "LlmFromScratch.sln"),
                "fixture solution sentinel");
            File.WriteAllText(
                Path.Combine(repositoryRoot, "llm_mvp_core.sln"),
                "requested fixture solution sentinel");
            File.WriteAllText(
                Path.Combine(repositoryRoot, "AGENTS.md"),
                "fixture governance sentinel");
            Directory.CreateDirectory(Path.Combine(repositoryRoot, "14.ContinuousLearning"));
            Directory.CreateDirectory(Path.Combine(repositoryRoot, "11.DPO"));
        }
    }

    private sealed record CreationLeaseTestPaths(
        string TrainingArtifactsRoot,
        string LeaseRoot,
        string DomainLeaseDirectory,
        string PatternLeaseDirectory,
        string LockPath);

    private sealed record PairFileSnapshot(
        string FileName,
        byte[] Bytes,
        string Sha256,
        DateTime LastWriteTimeUtc);
}
