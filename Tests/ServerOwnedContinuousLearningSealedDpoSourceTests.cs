// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using ContinuousLearning;

public sealed class ServerOwnedContinuousLearningSealedDpoSourceTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DpoRootRole_ReadNamespaceCannotAliasMutableStore(bool sealedSource)
    {
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        const string leafName = "selected-pair.bin";
        File.WriteAllBytes(Path.Combine(fixture.PairRoot, leafName), [0x41]);
        var before = fixture.Snapshot();
        var lost = 0;
        using var access = Acquire(fixture, sealedSource
            ? ContinuousLearningDpoPairRootAccess.SealedRepositorySource
            : ContinuousLearningDpoPairRootAccess.MutablePairStore, () => lost++);
        var readRole = sealedSource
            ? ContinuousLearningRetainedRootRole.SealedDpoSource
            : ContinuousLearningRetainedRootRole.DpoPairs;
        var forbiddenRole = sealedSource
            ? ContinuousLearningRetainedRootRole.DpoPairs
            : ContinuousLearningRetainedRootRole.SealedDpoSource;

        Assert.True(access.ValidateExistingRegularLeaf(readRole, leafName, maximumBytes: 1));
        Assert.Throws<ContinuousLearningPathIdentityException>(
            () => access.ValidateExistingRegularLeaf(forbiddenRole, leafName, maximumBytes: 1));
        access.EnsureRetained();
        Assert.Equal(before, fixture.Snapshot());
        Assert.Equal(0, lost);
        access.Dispose();
        Assert.Equal(1, lost);
        Assert.Throws<ContinuousLearningPathIdentityException>(access.EnsureRetained);
    }

    [Fact]
    public void UnknownDpoRootRole_FailsClosedBeforeAdmission()
    {
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        var before = fixture.Snapshot();
        Assert.Throws<ContinuousLearningPathIdentityException>(() =>
            Acquire(fixture, (ContinuousLearningDpoPairRootAccess)999, () => { }));
        Assert.Equal(before, fixture.Snapshot());
    }

    [Fact]
    public void SealedDpoSource_PosixReplacementMakesIdentityLossSticky()
    {
        if (!OperatingSystem.IsLinux())
            return;
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        var lost = 0;
        using var access = Acquire(
            fixture, ContinuousLearningDpoPairRootAccess.SealedRepositorySource, () => lost++);
        Directory.Move(fixture.PairRoot, fixture.PairRoot + "-retained");
        Directory.CreateDirectory(fixture.PairRoot);

        Assert.Throws<ContinuousLearningPathIdentityException>(access.EnsureRetained);
        Assert.Throws<ContinuousLearningPathIdentityException>(access.EnsureRetained);
        Assert.Equal(1, lost);
    }

    private static ServerOwnedContinuousLearningFileAccess Acquire(
        FileAccessFixture fixture,
        ContinuousLearningDpoPairRootAccess role,
        Action onTrustLost)
        => ServerOwnedContinuousLearningFileAccess.Acquire(
            fixture.BaseCheckpointPath,
            fixture.TokenizerPath,
            fixture.AdapterRoot,
            fixture.CacheRoot,
            fixture.RegistryRoot,
            fixture.PairRoot,
            options: null,
            onTrustLost,
            role);
}
