// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Buffers.Binary;
using System.Security.Cryptography;
using ContinuousLearning;
using Microsoft.Win32.SafeHandles;

public sealed class ServerOwnedContinuousLearningRegistryRootCapabilityTests
{
    private const string RegistryOwnerLockName =
        ".continuous-learning.registry-owner.lock";
    private const string CanonicalRegistryName = "registry.json";

    [ContinuousLearningRegistryCapabilityLinuxX64Fact]
    public void PosixWriteAndReadStayPortableButRenameAndDeleteFailClosed()
    {
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        PrepareLinuxOwnerWriteBoundary(fixture);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireRegistryRootCapability();

        // Registry на POSIX сохраняет create/read, но replace/delete не
        // имитируются через renameat2/unlinkat: эти syscalls заново выбирают
        // имя и не могут доказать opened identity.
        var descriptor = capability.CreateWriteTemp(new byte[] { 0x11, 0x22, 0x33 });
        var snapshot = capability.ReadExact(
            ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
            descriptor.Identity);
        Assert.Equal(new byte[] { 0x11, 0x22, 0x33 }, snapshot.CopyBytes());

        Assert.Throws<ContinuousLearningPosixExactMutationUnsupportedException>(
            () => capability.RenameNoReplace(
            ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
            snapshot.Identity,
            ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry));
        Assert.Equal(
            ServerOwnedContinuousLearningRegistryCanonicalState.Absent,
            capability.ReadCanonical().State);
        Assert.Equal(
            new byte[] { 0x11, 0x22, 0x33 },
            capability.ReadExact(
                ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
                descriptor.Identity).CopyBytes());

        Assert.Throws<ContinuousLearningPosixExactMutationUnsupportedException>(
            () => capability.DeleteExactOpenedIdentity(
            ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
            descriptor.Identity));
        Assert.Equal(
            [0x11, 0x22, 0x33],
            capability.ReadExact(
                ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
                descriptor.Identity).CopyBytes());
        access.EnsureRetained();
    }

    [ContinuousLearningRegistryCapabilityLinuxX64Fact]
    public void PosixExactDeleteSubstitutionWindowFailsClosedAndBothFilesSurvive()
    {
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        PrepareLinuxOwnerWriteBoundary(fixture);
        var sourcePath = Path.Combine(fixture.RegistryRoot, ".registry.json.write.tmp");
        var movedOriginalPath = Path.Combine(fixture.RegistryRoot, "registry-original.moved");
        var foreignBytes = new byte[] { 0x66, 0x99 };
        var substituted = false;
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireRegistryRootCapability(
            new ServerOwnedContinuousLearningRegistryRootCapabilityOptions
            {
                ObserveIoStageForTesting = stage =>
                {
                    if (stage != ServerOwnedContinuousLearningRegistryIoStage
                            .BeforeExactDelete ||
                        substituted)
                    {
                        return;
                    }

                    // Registry использует общий native helper. Подмена после
                    // identity validation не должна привести к unlink чужого
                    // canonical name.
                    substituted = true;
                    File.Move(sourcePath, movedOriginalPath);
                    File.WriteAllBytes(sourcePath, foreignBytes);
                    SetLinuxOwnerWritableFile(sourcePath);
                },
            });

        var descriptor = capability.CreateWriteTemp(new byte[] { 0x11, 0x22, 0x33 });

        Assert.Throws<ContinuousLearningPosixExactMutationUnsupportedException>(
            () => capability.DeleteExactOpenedIdentity(
                ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
                descriptor.Identity));

        Assert.True(substituted);
        Assert.Equal(new byte[] { 0x11, 0x22, 0x33 }, File.ReadAllBytes(movedOriginalPath));
        Assert.Equal(foreignBytes, File.ReadAllBytes(sourcePath));
        access.EnsureRetained();
    }

    [ContinuousLearningRegistryCapabilityLinuxX64Fact]
    public void PosixCreateCanonicalIfAbsentUsesDirectFinalLeafAndSelfValidatingSnapshot()
    {
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        PrepareLinuxOwnerWriteBoundary(fixture);
        var stages = new List<ServerOwnedContinuousLearningRegistryIoStage>();
        var bytes = "{\"schemaVersion\":2,\"adapters\":[]}"u8.ToArray();
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireRegistryRootCapability(
            new ServerOwnedContinuousLearningRegistryRootCapabilityOptions
            {
                ObserveIoStageForTesting = stages.Add,
            });

        var created = capability.CreateCanonicalIfAbsent(bytes);

        // Первая registry publication на POSIX строго create-only: canonical
        // появляется сразу, write temp вообще не существует, а возвращённое
        // доказательство совпадает с immutable identity/hash из ReadCanonical.
        Assert.Contains(
            ServerOwnedContinuousLearningRegistryIoStage
                .AfterCanonicalCreateOnlyOpenBeforeGrowth,
            stages);
        Assert.Equal(bytes, created.CopyBytes());
        Assert.False(File.Exists(Path.Combine(fixture.RegistryRoot, ".registry.json.write.tmp")));
        var canonical = capability.ReadCanonical();
        Assert.Equal(
            ServerOwnedContinuousLearningRegistryCanonicalState.Present,
            canonical.State);
        Assert.NotNull(canonical.Snapshot);
        Assert.True(canonical.Snapshot!.Matches(created));
        Assert.Throws<IOException>(() => capability.CreateCanonicalIfAbsent(
            "{\"schemaVersion\":2,\"adapters\":[{}]}"u8.ToArray()));
        Assert.Equal(bytes, File.ReadAllBytes(Path.Combine(fixture.RegistryRoot, "registry.json")));
        access.EnsureRetained();
    }

    [ContinuousLearningRegistryCapabilityLinuxX64Fact]
    public void PosixUnsafeCanonicalLeaves_AreRejectedWithoutFollowingOrAliasing()
    {
        using var symlinkFixture = new FileAccessFixture(provisionAllRoots: true);
        PrepareLinuxOwnerWriteBoundary(symlinkFixture);
        var outsideTarget = Path.Combine(symlinkFixture.Root, "outside-registry.json");
        File.WriteAllBytes(outsideTarget, [0x7B, 0x7D]);
        SetLinuxOwnerWritableFile(outsideTarget);
        File.CreateSymbolicLink(
            Path.Combine(symlinkFixture.RegistryRoot, CanonicalRegistryName),
            outsideTarget);

        using (var access = symlinkFixture.CreateStartup().AcquireRetainedFileAccess())
        using (var capability = access.AcquireRegistryRootCapability())
        {
            // Symlink на canonical leaf не является absence: иначе reader
            // может принять подменённый namespace как пустой безопасный root.
            var rejection = Record.Exception(() => capability.ReadCanonical());
            Assert.NotNull(rejection);
            Assert.Equal(new byte[] { 0x7B, 0x7D }, File.ReadAllBytes(outsideTarget));
            access.EnsureRetained();
        }

        using var hardlinkFixture = new FileAccessFixture(provisionAllRoots: true);
        PrepareLinuxOwnerWriteBoundary(hardlinkFixture);
        var canonicalPath = Path.Combine(
            hardlinkFixture.RegistryRoot,
            CanonicalRegistryName);
        File.WriteAllBytes(canonicalPath, [0x44]);
        SetLinuxOwnerWritableFile(canonicalPath);
        CreateHardLinkPosixOrFail(
            Path.Combine(hardlinkFixture.RegistryRoot, "unconfigured-registry-alias.json"),
            canonicalPath);

        using (var access = hardlinkFixture.CreateStartup().AcquireRetainedFileAccess())
        using (var capability = access.AcquireRegistryRootCapability())
        {
            // Hardlink даёт второй mutable name на те же registry bytes;
            // public exact identity query должен отказать при nlink>1.
            Assert.ThrowsAny<Exception>(() => capability.QueryExactIdentity(
                ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry));
            access.EnsureRetained();
        }
    }

    [ContinuousLearningRegistryCapabilityLinuxX64Fact]
    public void PosixGroupWritableOwnerLock_IsRejectedBeforeCapabilityBinds()
    {
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        PrepareLinuxOwnerWriteBoundary(fixture);
        var lockPath = Path.Combine(fixture.RegistryRoot, RegistryOwnerLockName);
        File.WriteAllBytes(lockPath, [0xB1]);
        SetLinuxGroupWritableFile(lockPath);

        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();

        // Registry owner-lock — это не данные журнала, а граница lease.
        // Если lock writable для group/other, capability должна fail closed.
        Assert.ThrowsAny<Exception>(() => access.AcquireRegistryRootCapability());
        access.EnsureRetained();
    }

    [ContinuousLearningRegistryCapabilityLinuxX64Fact]
    public void PosixRootReplacementAfterOperation_InvalidatesRetainedCapability()
    {
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        PrepareLinuxOwnerWriteBoundary(fixture);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        var replaced = false;
        using var capability = access.AcquireRegistryRootCapability(
            new ServerOwnedContinuousLearningRegistryRootCapabilityOptions
            {
                ObserveIoStageForTesting = stage =>
                {
                    if (stage != ServerOwnedContinuousLearningRegistryIoStage
                            .AfterOperationBeforeRootRevalidation ||
                        replaced)
                    {
                        return;
                    }

                    replaced = true;
                    ReplaceDirectoryWithFreshOwnerBoundary(fixture.RegistryRoot);
                },
            });

        // Угроза delete/recreate после операции: wrapper обязан перепроверить
        // retained root identity и сделать trust sticky-lost.
        Assert.Throws<ContinuousLearningPathIdentityException>(() => capability.Inventory());
        Assert.True(replaced);
        Assert.Throws<ContinuousLearningPathIdentityException>(access.EnsureRetained);
    }

    [ContinuousLearningRegistryWindowsFact]
    public void RollbackEvidenceFramingRejectsEveryNonCanonicalHeaderBranch()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireRegistryRootCapability();
        var priorDescriptor = capability.CreateWriteTemp(new byte[] { 1, 2, 3 });
        var prior = capability.ReadExact(
            ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
            priorDescriptor.Identity);
        capability.RenameNoReplace(
            ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
            prior.Identity,
            ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry);
        capability.FlushFile(
            ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry,
            prior.Identity);
        capability.FlushRootDirectory();
        var candidateDescriptor = capability.CreateWriteTemp(new byte[] { 4, 5, 6, 7 });
        var candidate = capability.ReadExact(
            ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
            candidateDescriptor.Identity);
        var evidence = capability.PreparePublication(
            candidate,
            ServerOwnedContinuousLearningRegistryPriorStateEvidence.Present(prior));
        var original = evidence.MarkerArtifact.CopyBytes();

        void AssertRejected(Func<byte[], byte[]> mutate)
        {
            var bytes = mutate(original.ToArray());
            var marker = new ServerOwnedContinuousLearningRegistrySnapshot(
                evidence.MarkerArtifact.Identity,
                bytes);
            Assert.ThrowsAny<Exception>(() => capability.DecodeRollbackEvidence(marker));
        }

        AssertRejected(bytes => { bytes[0] ^= 0xff; return bytes; });
        AssertRejected(bytes => { BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(8, 4), 2); return bytes; });
        AssertRejected(bytes => { BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(12, 4), 0); return bytes; });
        AssertRejected(bytes => { BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(12, 4), 4); return bytes; });
        AssertRejected(bytes => { BinaryPrimitives.WriteInt64BigEndian(bytes.AsSpan(16, 8), -1); return bytes; });
        AssertRejected(bytes => { BinaryPrimitives.WriteInt64BigEndian(bytes.AsSpan(16, 8), DomainModelRegistry.MaximumRegistryBytes + 1); return bytes; });
        AssertRejected(bytes => { BinaryPrimitives.WriteInt64BigEndian(bytes.AsSpan(24, 8), -1); return bytes; });
        AssertRejected(bytes => { BinaryPrimitives.WriteInt64BigEndian(bytes.AsSpan(24, 8), DomainModelRegistry.MaximumRegistryBytes + 1); return bytes; });
        AssertRejected(bytes => bytes[..^1]);
        AssertRejected(bytes => [.. bytes, 0]);
        AssertRejected(bytes => { Array.Clear(bytes, 96, 24); return bytes; });
        AssertRejected(bytes => { Array.Clear(bytes, 120, 24); return bytes; });
        AssertRejected(bytes => { bytes[32] ^= 0x01; return bytes; });

        var priorOnly = original.ToArray();
        BinaryPrimitives.WriteUInt32BigEndian(priorOnly.AsSpan(12, 4), 1);
        BinaryPrimitives.WriteInt64BigEndian(priorOnly.AsSpan(24, 8), 0);
        Array.Clear(priorOnly, 64, 32);
        Array.Clear(priorOnly, 120, 24);
        _ = capability.DecodeRollbackEvidence(
            new ServerOwnedContinuousLearningRegistrySnapshot(
                evidence.MarkerArtifact.Identity,
                priorOnly));
        AssertRejected(_ => { var bytes = priorOnly.ToArray(); bytes[64] = 1; return bytes; });
        AssertRejected(_ => { var bytes = priorOnly.ToArray(); bytes[120] = 1; return bytes; });

        capability.RenameNoReplace(
            ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry,
            prior.Identity,
            ServerOwnedContinuousLearningRegistryLeaf.PublicationBackup);
        capability.FlushRootDirectory();
        capability.RenameNoReplace(
            ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
            candidate.Identity,
            ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry);
        capability.FlushFile(
            ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry,
            candidate.Identity);
        capability.FlushRootDirectory();
        var accepted = capability.CommitPublication(
            candidate,
            ServerOwnedContinuousLearningRegistryPriorStateEvidence.Present(prior),
            evidence);
        var acceptedOriginal = accepted.MarkerArtifact.CopyBytes();

        void AssertAcceptedRejected(Func<byte[], byte[]> mutate)
        {
            var bytes = mutate(acceptedOriginal.ToArray());
            var marker = new ServerOwnedContinuousLearningRegistrySnapshot(
                accepted.MarkerArtifact.Identity,
                bytes);
            Assert.ThrowsAny<Exception>(() => capability.DecodeAcceptedEvidence(marker));
        }

        AssertAcceptedRejected(bytes => { bytes[0] ^= 0xff; return bytes; });
        AssertAcceptedRejected(bytes => { BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(8, 4), 2); return bytes; });
        AssertAcceptedRejected(bytes => { BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(12, 4), 1); return bytes; });
        AssertAcceptedRejected(bytes => { BinaryPrimitives.WriteInt64BigEndian(bytes.AsSpan(16, 8), -1); return bytes; });
        AssertAcceptedRejected(bytes => { BinaryPrimitives.WriteInt64BigEndian(bytes.AsSpan(16, 8), ServerOwnedContinuousLearningRegistryRollbackEvidence.MaximumEncodedBytes + 1); return bytes; });
        AssertAcceptedRejected(bytes => bytes[..^1]);
        AssertAcceptedRejected(bytes => [.. bytes, 0]);
        AssertAcceptedRejected(bytes => { bytes[24] ^= 0x01; return bytes; });
        AssertAcceptedRejected(bytes => { Array.Clear(bytes, 56, 24); return bytes; });
        AssertAcceptedRejected(bytes =>
        {
            BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(88, 4), 2);
            SHA256.HashData(bytes.AsSpan(80)).CopyTo(bytes.AsSpan(24, 32));
            return bytes;
        });
    }

    [ContinuousLearningRegistryWindowsFact]
    public void MaximumAcceptedLeafUsesItsTypedBoundForEveryExactLeafOperation()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        var stagingPath = Path.Combine(
            fixture.RegistryRoot,
            ".registry.json.accepted.staging");
        var evidencePath = Path.Combine(
            fixture.RegistryRoot,
            ".registry.json.accepted.evidence");
        using (var stream = new FileStream(
                   stagingPath,
                   FileMode.CreateNew,
                   System.IO.FileAccess.Write,
                   FileShare.None))
        {
            stream.SetLength(
                ServerOwnedContinuousLearningRegistryAcceptedEvidence.MaximumEncodedBytes);
            stream.Flush(flushToDisk: true);
        }

        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireRegistryRootCapability();
        var staging = capability.QueryExactIdentity(
            ServerOwnedContinuousLearningRegistryLeaf.AcceptedStaging);
        Assert.Equal(
            ServerOwnedContinuousLearningRegistryAcceptedEvidence.MaximumEncodedBytes,
            staging.Length);
        Assert.Equal(
            staging.Length,
            capability.ReadExact(
                ServerOwnedContinuousLearningRegistryLeaf.AcceptedStaging,
                staging.Identity).Length);
        capability.FlushFile(
            ServerOwnedContinuousLearningRegistryLeaf.AcceptedStaging,
            staging.Identity);

        capability.RenameNoReplace(
            ServerOwnedContinuousLearningRegistryLeaf.AcceptedStaging,
            staging.Identity,
            ServerOwnedContinuousLearningRegistryLeaf.AcceptedEvidence);
        capability.FlushRootDirectory();
        Assert.False(File.Exists(stagingPath));
        Assert.True(File.Exists(evidencePath));
        var evidence = capability.QueryExactIdentity(
            ServerOwnedContinuousLearningRegistryLeaf.AcceptedEvidence);
        Assert.Equal(staging, evidence);
        Assert.Equal(
            evidence.Length,
            capability.ReadExact(
                ServerOwnedContinuousLearningRegistryLeaf.AcceptedEvidence,
                evidence.Identity).Length);
        capability.FlushFile(
            ServerOwnedContinuousLearningRegistryLeaf.AcceptedEvidence,
            evidence.Identity);
        capability.DeleteExactOpenedIdentity(
            ServerOwnedContinuousLearningRegistryLeaf.AcceptedEvidence,
            evidence.Identity);
        capability.FlushRootDirectory();
        Assert.False(File.Exists(evidencePath));
    }

    [Fact]
    public void CapabilitySurface_IsFixedTypedPathFreeAndHandleFree()
    {
        var capabilityType = typeof(ServerOwnedContinuousLearningRegistryRootCapability);
        var surfaceMethods = capabilityType.GetMethods(
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.DeclaredOnly);

        Assert.DoesNotContain(surfaceMethods, method =>
            IsForbiddenSurfaceType(method.ReturnType) ||
            method.GetParameters().Any(parameter =>
                IsForbiddenSurfaceType(parameter.ParameterType)));
        Assert.Equal(
            [
                ServerOwnedContinuousLearningRegistryLeaf.OwnerLock,
                ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry,
                ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
                ServerOwnedContinuousLearningRegistryLeaf.PublicationBackup,
                ServerOwnedContinuousLearningRegistryLeaf.RollbackEvidence,
                ServerOwnedContinuousLearningRegistryLeaf.AcceptedStaging,
                ServerOwnedContinuousLearningRegistryLeaf.AcceptedEvidence,
            ],
            Enum.GetValues<ServerOwnedContinuousLearningRegistryLeaf>());
        Assert.DoesNotContain(
            typeof(ServerOwnedContinuousLearningRegistryContentHash).GetProperties(),
            property => property.PropertyType == typeof(byte[]));
        Assert.DoesNotContain(
            typeof(ServerOwnedContinuousLearningRegistrySnapshot).GetProperties(),
            property => property.PropertyType == typeof(byte[]));
    }

    [ContinuousLearningRegistryWindowsFact]
    public void Acquire_IsOneShotAndRetainedRootOwnershipReleasesSynchronously()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var firstAccess = fixture.CreateStartup().AcquireRetainedFileAccess();
        var first = firstAccess.AcquireRegistryRootCapability();

        Assert.Throws<InvalidOperationException>(
            () => firstAccess.AcquireRegistryRootCapability());
        Assert.Throws<ContinuousLearningMutationAdmissionDomainAlreadyOwnedException>(
            () => fixture.CreateStartup().AcquireRetainedFileAccess());

        first.Dispose();
        Assert.Throws<ContinuousLearningPathIdentityException>(first.ReadCanonical);
        firstAccess.Dispose();
        using var secondAccess = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var second = secondAccess.AcquireRegistryRootCapability();
        Assert.Equal(
            ServerOwnedContinuousLearningRegistryCanonicalState.Absent,
            second.ReadCanonical().State);
        Assert.Throws<ContinuousLearningPathIdentityException>(
            () => firstAccess.AcquireRegistryRootCapability());
    }

    [ContinuousLearningRegistryWindowsFact]
    public void OwnerLock_IsExclusiveAcrossProcessesAndReleasedSynchronously()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        var capability = access.AcquireRegistryRootCapability();

        Assert.Equal(10, RunCrossProcessOwnerLockProbe(fixture.RegistryRoot));
        capability.Dispose();
        Assert.Equal(0, RunCrossProcessOwnerLockProbe(fixture.RegistryRoot));
    }

    [ContinuousLearningRegistryWindowsFact]
    public void ReadCanonical_ReportsOnlyMissingLeafAsTypedAbsence()
    {
        RequireWindows();
        using (var absentFixture = new FileAccessFixture(provisionAllRoots: true))
        using (var absentAccess = absentFixture.CreateStartup().AcquireRetainedFileAccess())
        using (var capability = absentAccess.AcquireRegistryRootCapability())
        {
            var read = capability.ReadCanonical();
            Assert.Equal(
                ServerOwnedContinuousLearningRegistryCanonicalState.Absent,
                read.State);
            Assert.Null(read.Snapshot);
        }

        using var malformedFixture = new FileAccessFixture(provisionAllRoots: true);
        Directory.CreateDirectory(Path.Combine(malformedFixture.RegistryRoot, "registry.json"));
        using var malformedAccess = malformedFixture.CreateStartup().AcquireRetainedFileAccess();
        using var malformedCapability = malformedAccess.AcquireRegistryRootCapability();
        Assert.ThrowsAny<IOException>(() => malformedCapability.ReadCanonical());

        using var emptyFixture = new FileAccessFixture(provisionAllRoots: true);
        File.WriteAllBytes(Path.Combine(emptyFixture.RegistryRoot, "registry.json"), []);
        using var emptyAccess = emptyFixture.CreateStartup().AcquireRetainedFileAccess();
        using var emptyCapability = emptyAccess.AcquireRegistryRootCapability();
        Assert.Throws<InvalidDataException>(() => emptyCapability.ReadCanonical());
    }

    [ContinuousLearningRegistryWindowsFact]
    public void WriteReadIdentityAndHash_AreExactBoundedAndDefensivelyImmutable()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireRegistryRootCapability();
        var bytes = "{\"schemaVersion\":1,\"adapters\":[]}"u8.ToArray();

        var written = capability.CreateWriteTemp(bytes);
        var queried = capability.QueryExactIdentity(
            ServerOwnedContinuousLearningRegistryLeaf.WriteTemp);
        Assert.Equal(written, queried);
        capability.ValidateExactIdentity(
            ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
            written.Identity);
        capability.FlushFile(
            ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
            written.Identity);
        capability.FlushRootDirectory();
        var snapshot = capability.ReadExact(
            ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
            written.Identity);

        Assert.Equal(bytes.LongLength, snapshot.Length);
        Assert.Equal(bytes, snapshot.CopyBytes());
        Assert.Equal(
            ServerOwnedContinuousLearningRegistryContentHash.FromBytes(bytes),
            snapshot.Sha256);
        var mutableCopy = snapshot.CopyBytes();
        mutableCopy[0] ^= 0x7f;
        Assert.Equal(bytes, snapshot.CopyBytes());

        var overBound = new byte[checked((int)DomainModelRegistry.MaximumRegistryBytes + 1)];
        Assert.Throws<ArgumentOutOfRangeException>(
            () => capability.CreateWriteTemp(overBound));
        Assert.Equal(
            bytes,
            File.ReadAllBytes(Path.Combine(
                fixture.RegistryRoot,
                ".registry.json.write.tmp")));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => capability.CreateWriteTemp(ReadOnlyMemory<byte>.Empty));
    }

    [ContinuousLearningRegistryWindowsFact]
    public void RenameProtocol_AllowsOnlyFourFixedEdgesAndNeverOverwritesDestination()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        var canonicalPath = Path.Combine(fixture.RegistryRoot, "registry.json");
        var backupPath = Path.Combine(
            fixture.RegistryRoot,
            ".registry.json.publication.bak");
        var tempPath = Path.Combine(fixture.RegistryRoot, ".registry.json.write.tmp");
        var firstBytes = new byte[] { 1, 2, 3 };
        var secondBytes = new byte[] { 4, 5, 6 };
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireRegistryRootCapability();

        var first = capability.CreateWriteTemp(firstBytes);
        capability.RenameNoReplace(
            ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
            first.Identity,
            ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry);
        Assert.Equal(firstBytes, File.ReadAllBytes(canonicalPath));
        var canonical = capability.ReadCanonical();
        Assert.Equal(
            ServerOwnedContinuousLearningRegistryCanonicalState.Present,
            canonical.State);
        Assert.NotNull(canonical.Snapshot);
        Assert.Equal(firstBytes, canonical.Snapshot!.CopyBytes());
        capability.RenameNoReplace(
            ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry,
            first.Identity,
            ServerOwnedContinuousLearningRegistryLeaf.PublicationBackup);
        Assert.Equal(firstBytes, File.ReadAllBytes(backupPath));

        var second = capability.CreateWriteTemp(secondBytes);
        capability.RenameNoReplace(
            ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
            second.Identity,
            ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry);
        Assert.Equal(secondBytes, File.ReadAllBytes(canonicalPath));
        Assert.ThrowsAny<IOException>(() => capability.RenameNoReplace(
            ServerOwnedContinuousLearningRegistryLeaf.PublicationBackup,
            first.Identity,
            ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry));
        Assert.Equal(secondBytes, File.ReadAllBytes(canonicalPath));
        Assert.Equal(firstBytes, File.ReadAllBytes(backupPath));

        File.Delete(canonicalPath);
        capability.RenameNoReplace(
            ServerOwnedContinuousLearningRegistryLeaf.PublicationBackup,
            first.Identity,
            ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry);
        Assert.Equal(firstBytes, File.ReadAllBytes(canonicalPath));
        Assert.False(File.Exists(backupPath));
        Assert.False(File.Exists(tempPath));

        foreach (var edge in DisallowedRenameEdges())
        {
            Assert.Throws<ArgumentException>(() => capability.RenameNoReplace(
                edge.Source,
                first.Identity,
                edge.Destination));
        }

        capability.RenameNoReplace(
            ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry,
            first.Identity,
            ServerOwnedContinuousLearningRegistryLeaf.PublicationBackup);
        capability.DeleteExactOpenedIdentity(
            ServerOwnedContinuousLearningRegistryLeaf.PublicationBackup,
            first.Identity);
        Assert.False(File.Exists(canonicalPath));
        Assert.False(File.Exists(backupPath));
    }

    [ContinuousLearningRegistryWindowsFact]
    public void ExactCleanup_DeletesOwnedRecoveryLeafButNeverForeignSubstitution()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        var tempPath = Path.Combine(fixture.RegistryRoot, ".registry.json.write.tmp");
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireRegistryRootCapability();

        var first = capability.CreateWriteTemp(new byte[] { 1, 2, 3 });
        capability.DeleteExactOpenedIdentity(
            ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
            first.Identity);
        Assert.False(File.Exists(tempPath));

        var second = capability.CreateWriteTemp(new byte[] { 4, 5, 6 });
        var ownedElsewhere = tempPath + ".owned";
        File.Move(tempPath, ownedElsewhere);
        var foreign = new byte[] { 0xf0, 0x0d };
        File.WriteAllBytes(tempPath, foreign);

        Assert.Throws<InvalidDataException>(() => capability.DeleteExactOpenedIdentity(
            ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
            second.Identity));
        Assert.Equal(foreign, File.ReadAllBytes(tempPath));
        Assert.Equal(new byte[] { 4, 5, 6 }, File.ReadAllBytes(ownedElsewhere));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            capability.DeleteExactOpenedIdentity(
                ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry,
                second.Identity));
    }

    [ContinuousLearningRegistryWindowsFact]
    public void RegistryRootLoss_IsStickyAndRevokesAllAcquiredRootCapabilities()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var cache = access.AcquireCompilerCacheRootCapability();
        using var adapter = access.AcquireAdapterRootCapability();
        using var registry = access.AcquireRegistryRootCapability(
            new ServerOwnedContinuousLearningRegistryRootCapabilityOptions
            {
                ObserveIoStageForTesting = stage =>
                {
                    if (stage == ServerOwnedContinuousLearningRegistryIoStage
                            .AfterOperationBeforeRootRevalidation)
                    {
                        GetRetainedRootHandle(
                            access,
                            ContinuousLearningRetainedObjectRole.RegistryRoot)
                            .Dispose();
                    }
                },
            });

        Assert.Throws<ContinuousLearningPathIdentityException>(
            () => registry.ReadCanonical());
        Assert.Throws<ContinuousLearningPathIdentityException>(
            () => cache.Inventory(5));
        Assert.Throws<ContinuousLearningPathIdentityException>(
            adapter.ThrowIfUnavailableLocked);
        Assert.Throws<ContinuousLearningPathIdentityException>(access.EnsureRetained);
    }

    [ContinuousLearningRegistryWindowsFact]
    public void FileAndRootFlushFailures_AreNeverReportedAsSuccess()
    {
        RequireWindows();
        using (var fileFixture = new FileAccessFixture(provisionAllRoots: true))
        using (var fileAccess = fileFixture.CreateStartup().AcquireRetainedFileAccess())
        using (var fileCapability = fileAccess.AcquireRegistryRootCapability(
                   new ServerOwnedContinuousLearningRegistryRootCapabilityOptions
                   {
                       FailNativeCallForTesting = call =>
                           call == ServerOwnedContinuousLearningRegistryNativeCall.FileFlush,
                   }))
        {
            Assert.Throws<IOException>(() => fileCapability.CreateWriteTemp(
                new byte[] { 1, 2, 3 }));
            Assert.False(File.Exists(Path.Combine(
                fileFixture.RegistryRoot,
                ".registry.json.write.tmp")));
        }

        using var rootFixture = new FileAccessFixture(provisionAllRoots: true);
        using var rootAccess = rootFixture.CreateStartup().AcquireRetainedFileAccess();
        using var rootCapability = rootAccess.AcquireRegistryRootCapability(
            new ServerOwnedContinuousLearningRegistryRootCapabilityOptions
            {
                FailNativeCallForTesting = call =>
                    call == ServerOwnedContinuousLearningRegistryNativeCall.RootDirectoryFlush,
            });
        Assert.Throws<IOException>(rootCapability.FlushRootDirectory);
    }

    [ContinuousLearningRegistryWindowsFact]
    public void Inventory_IsCompleteTypedAndRetainsExactEvidenceForEveryFixedLeaf()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireRegistryRootCapability();

        var first = capability.CreateWriteTemp(new byte[] { 1, 2, 3 });
        capability.RenameNoReplace(
            ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
            first.Identity,
            ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry);
        capability.RenameNoReplace(
            ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry,
            first.Identity,
            ServerOwnedContinuousLearningRegistryLeaf.PublicationBackup);
        var second = capability.CreateWriteTemp(new byte[] { 4, 5, 6 });
        capability.RenameNoReplace(
            ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
            second.Identity,
            ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry);
        var third = capability.CreateWriteTemp(new byte[] { 7, 8, 9 });

        var inventory = capability.Inventory();

        Assert.Equal(6, ServerOwnedContinuousLearningRegistryNamespaceInventory.FixedDataLeafCount);
        AssertObservation(
            inventory.CanonicalRegistry,
            ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry,
            second.Identity,
            new byte[] { 4, 5, 6 });
        AssertObservation(
            inventory.WriteTemp,
            ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
            third.Identity,
            new byte[] { 7, 8, 9 });
        AssertObservation(
            inventory.PublicationBackup,
            ServerOwnedContinuousLearningRegistryLeaf.PublicationBackup,
            first.Identity,
            new byte[] { 1, 2, 3 });
        Assert.Equal(
            ServerOwnedContinuousLearningRegistryLeafState.Absent,
            inventory.RollbackEvidence.State);
        Assert.Equal(
            ServerOwnedContinuousLearningRegistryLeafState.Absent,
            inventory.AcceptedStaging.State);
        Assert.Equal(
            ServerOwnedContinuousLearningRegistryLeafState.Absent,
            inventory.AcceptedEvidence.State);

        using var malformedFixture = new FileAccessFixture(provisionAllRoots: true);
        Directory.CreateDirectory(Path.Combine(
            malformedFixture.RegistryRoot,
            ".registry.json.write.tmp"));
        using var malformedAccess = malformedFixture.CreateStartup().AcquireRetainedFileAccess();
        using var malformedCapability = malformedAccess.AcquireRegistryRootCapability();
        Assert.ThrowsAny<IOException>(malformedCapability.Inventory);
    }

    [ContinuousLearningRegistryWindowsFact]
    public void InjectedRenameFailureBeforeNativeCall_LeavesExactSourceAndDestinationState()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireRegistryRootCapability(
            new ServerOwnedContinuousLearningRegistryRootCapabilityOptions
            {
                FailNativeCallForTesting = call =>
                    call == ServerOwnedContinuousLearningRegistryNativeCall.NoReplaceRename,
            });
        var bytes = new byte[] { 0x11, 0x22, 0x33 };
        var temp = capability.CreateWriteTemp(bytes);

        Assert.Throws<IOException>(() => capability.RenameNoReplace(
            ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
            temp.Identity,
            ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry));

        var inventory = capability.Inventory();
        Assert.Equal(
            ServerOwnedContinuousLearningRegistryLeafState.Absent,
            inventory.CanonicalRegistry.State);
        AssertObservation(
            inventory.WriteTemp,
            ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
            temp.Identity,
            bytes);
        Assert.Equal(
            ServerOwnedContinuousLearningRegistryLeafState.Absent,
            inventory.PublicationBackup.State);
    }

    [ContinuousLearningRegistryWindowsFact]
    public void ObserverFailureAfterActualRename_LeavesDiscoverableTypedInventory()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        var throwAfterRename = true;
        using var capability = access.AcquireRegistryRootCapability(
            new ServerOwnedContinuousLearningRegistryRootCapabilityOptions
            {
                ObserveIoStageForTesting = stage =>
                {
                    if (throwAfterRename &&
                        stage == ServerOwnedContinuousLearningRegistryIoStage.AfterNoReplaceRename)
                    {
                        throwAfterRename = false;
                        throw new IOException("Injected post-rename observer failure.");
                    }
                },
            });
        var bytes = new byte[] { 0x44, 0x55, 0x66 };
        var temp = capability.CreateWriteTemp(bytes);

        Assert.Throws<IOException>(() => capability.RenameNoReplace(
            ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
            temp.Identity,
            ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry));

        var inventory = capability.Inventory();
        AssertObservation(
            inventory.CanonicalRegistry,
            ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry,
            temp.Identity,
            bytes);
        Assert.Equal(ServerOwnedContinuousLearningRegistryLeafState.Absent, inventory.WriteTemp.State);
        Assert.Equal(ServerOwnedContinuousLearningRegistryLeafState.Absent, inventory.PublicationBackup.State);
    }

    [ContinuousLearningRegistryWindowsFact]
    public void RootLossAfterActualRename_IsStickyAndRestartInventoryDiscoversResult()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        var loseRootAfterRename = true;
        var capability = access.AcquireRegistryRootCapability(
            new ServerOwnedContinuousLearningRegistryRootCapabilityOptions
            {
                ObserveIoStageForTesting = stage =>
                {
                    if (loseRootAfterRename &&
                        stage == ServerOwnedContinuousLearningRegistryIoStage.AfterNoReplaceRename)
                    {
                        loseRootAfterRename = false;
                        GetRetainedRootHandle(
                            access,
                            ContinuousLearningRetainedObjectRole.RegistryRoot)
                            .Dispose();
                    }
                },
            });
        var bytes = new byte[] { 0x71, 0x72, 0x73 };
        var temp = capability.CreateWriteTemp(bytes);

        Assert.Throws<ContinuousLearningPathIdentityException>(() => capability.RenameNoReplace(
            ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
            temp.Identity,
            ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry));
        Assert.Throws<ContinuousLearningPathIdentityException>(capability.Inventory);
        capability.Dispose();
        access.Dispose();

        using var restartedAccess = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var restarted = restartedAccess.AcquireRegistryRootCapability();
        var inventory = restarted.Inventory();
        AssertObservation(
            inventory.CanonicalRegistry,
            ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry,
            temp.Identity,
            bytes);
        Assert.Equal(ServerOwnedContinuousLearningRegistryLeafState.Absent, inventory.WriteTemp.State);
        Assert.Equal(ServerOwnedContinuousLearningRegistryLeafState.Absent, inventory.PublicationBackup.State);
    }

    [ContinuousLearningRegistryWindowsFact]
    public void ReconcileRejectedPublication_RestoresPriorAndFirstPublicationAbsence()
    {
        RequireWindows();
        using (var fixture = new FileAccessFixture(provisionAllRoots: true))
        using (var access = fixture.CreateStartup().AcquireRetainedFileAccess())
        using (var capability = access.AcquireRegistryRootCapability())
        {
            var priorDescriptor = capability.CreateWriteTemp(new byte[] { 1, 3, 5 });
            var prior = capability.ReadExact(
                ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
                priorDescriptor.Identity);
            capability.RenameNoReplace(
                ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
                prior.Identity,
                ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry);
            capability.RenameNoReplace(
                ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry,
                prior.Identity,
                ServerOwnedContinuousLearningRegistryLeaf.PublicationBackup);
            var rejectedDescriptor = capability.CreateWriteTemp(new byte[] { 2, 4, 6 });
            var rejected = capability.ReadExact(
                ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
                rejectedDescriptor.Identity);
            capability.RenameNoReplace(
                ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
                rejected.Identity,
                ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry);

            var result = capability.ReconcileRejectedPublication(
                rejected,
                ServerOwnedContinuousLearningRegistryPriorStateEvidence.Present(prior));

            Assert.Equal(
                ServerOwnedContinuousLearningRegistryReconcileDisposition.RestoredPrior,
                result.Disposition);
            AssertOnlyCanonical(result.FinalInventory, prior);

            var repeated = capability.ReconcileRejectedPublication(
                rejected,
                ServerOwnedContinuousLearningRegistryPriorStateEvidence.Present(prior));
            Assert.Equal(
                ServerOwnedContinuousLearningRegistryReconcileDisposition
                    .AlreadyRestoredPrior,
                repeated.Disposition);
            AssertOnlyCanonical(repeated.FinalInventory, prior);
        }

        using (var fixture = new FileAccessFixture(provisionAllRoots: true))
        using (var access = fixture.CreateStartup().AcquireRetainedFileAccess())
        using (var capability = access.AcquireRegistryRootCapability())
        {
            var rejectedDescriptor = capability.CreateWriteTemp(new byte[] { 8, 8, 8 });
            var rejected = capability.ReadExact(
                ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
                rejectedDescriptor.Identity);
            capability.RenameNoReplace(
                ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
                rejected.Identity,
                ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry);

            var result = capability.ReconcileRejectedPublication(
                rejected,
                ServerOwnedContinuousLearningRegistryPriorStateEvidence.Absent());

            Assert.Equal(
                ServerOwnedContinuousLearningRegistryReconcileDisposition.RestoredAbsence,
                result.Disposition);
            AssertAllAbsent(result.FinalInventory);
        }

        using (var fixture = new FileAccessFixture(provisionAllRoots: true))
        using (var access = fixture.CreateStartup().AcquireRetainedFileAccess())
        using (var capability = access.AcquireRegistryRootCapability())
        {
            var priorDescriptor = capability.CreateWriteTemp(new byte[] { 4, 4, 4 });
            var prior = capability.ReadExact(
                ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
                priorDescriptor.Identity);
            capability.RenameNoReplace(
                ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
                prior.Identity,
                ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry);
            var rejectedDescriptor = capability.CreateWriteTemp(new byte[] { 5, 5, 5 });
            var rejected = capability.ReadExact(
                ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
                rejectedDescriptor.Identity);

            var result = capability.ReconcileRejectedPublication(
                rejected,
                ServerOwnedContinuousLearningRegistryPriorStateEvidence.Present(prior));

            Assert.Equal(
                ServerOwnedContinuousLearningRegistryReconcileDisposition
                    .AlreadyRestoredPrior,
                result.Disposition);
            AssertOnlyCanonical(result.FinalInventory, prior);
        }
    }

    [ContinuousLearningRegistryWindowsFact]
    public void ReconcileRejectedPublication_RecoversBackupOnlyAndTempOnlyRestartStates()
    {
        RequireWindows();
        using (var fixture = new FileAccessFixture(provisionAllRoots: true))
        using (var access = fixture.CreateStartup().AcquireRetainedFileAccess())
        using (var capability = access.AcquireRegistryRootCapability())
        {
            var priorDescriptor = capability.CreateWriteTemp(new byte[] { 1, 1, 1 });
            var prior = capability.ReadExact(
                ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
                priorDescriptor.Identity);
            capability.RenameNoReplace(
                ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
                prior.Identity,
                ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry);
            capability.RenameNoReplace(
                ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry,
                prior.Identity,
                ServerOwnedContinuousLearningRegistryLeaf.PublicationBackup);
            var rejectedDescriptor = capability.CreateWriteTemp(new byte[] { 2, 2, 2 });
            var rejected = capability.ReadExact(
                ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
                rejectedDescriptor.Identity);
            capability.DeleteExactOpenedIdentity(
                ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
                rejected.Identity);

            var result = capability.ReconcileRejectedPublication(
                rejected,
                ServerOwnedContinuousLearningRegistryPriorStateEvidence.Present(prior));

            Assert.Equal(
                ServerOwnedContinuousLearningRegistryReconcileDisposition.RestoredPrior,
                result.Disposition);
            AssertOnlyCanonical(result.FinalInventory, prior);
        }

        using (var fixture = new FileAccessFixture(provisionAllRoots: true))
        using (var access = fixture.CreateStartup().AcquireRetainedFileAccess())
        using (var capability = access.AcquireRegistryRootCapability())
        {
            var rejectedDescriptor = capability.CreateWriteTemp(new byte[] { 3, 3, 3 });
            var rejected = capability.ReadExact(
                ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
                rejectedDescriptor.Identity);

            var result = capability.ReconcileRejectedPublication(
                rejected,
                ServerOwnedContinuousLearningRegistryPriorStateEvidence.Absent());

            Assert.Equal(
                ServerOwnedContinuousLearningRegistryReconcileDisposition.RestoredAbsence,
                result.Disposition);
            AssertAllAbsent(result.FinalInventory);
        }
    }

    [ContinuousLearningRegistryWindowsFact]
    public void RetryAfterRejectedDeletion_RequiresRootFlushBeforeReportingAbsence()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        ServerOwnedContinuousLearningRegistrySnapshot rejected;

        using (var access = fixture.CreateStartup().AcquireRetainedFileAccess())
        using (var capability = access.AcquireRegistryRootCapability(
                   new ServerOwnedContinuousLearningRegistryRootCapabilityOptions
                   {
                       ObserveIoStageForTesting = stage =>
                       {
                           if (stage == ServerOwnedContinuousLearningRegistryIoStage
                                   .AfterRejectedArtifactDelete)
                           {
                               throw new IOException("Injected crash after rejected deletion.");
                           }
                       },
                   }))
        {
            var descriptor = capability.CreateWriteTemp(new byte[] { 9, 1, 9 });
            rejected = capability.ReadExact(
                ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
                descriptor.Identity);
            capability.RenameNoReplace(
                ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
                rejected.Identity,
                ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry);
            Assert.Throws<IOException>(() => capability.ReconcileRejectedPublication(
                rejected,
                ServerOwnedContinuousLearningRegistryPriorStateEvidence.Absent()));
        }

        using (var access = fixture.CreateStartup().AcquireRetainedFileAccess())
        using (var capability = access.AcquireRegistryRootCapability(
                   new ServerOwnedContinuousLearningRegistryRootCapabilityOptions
                   {
                       FailNativeCallForTesting = call =>
                           call == ServerOwnedContinuousLearningRegistryNativeCall
                               .RootDirectoryFlush,
                   }))
        {
            Assert.Throws<IOException>(() => capability.ReconcileRejectedPublication(
                rejected,
                ServerOwnedContinuousLearningRegistryPriorStateEvidence.Absent()));
            var pending = capability.Inventory();
            Assert.Equal(ServerOwnedContinuousLearningRegistryLeafState.Absent, pending.CanonicalRegistry.State);
            Assert.Equal(ServerOwnedContinuousLearningRegistryLeafState.Absent, pending.WriteTemp.State);
            Assert.Equal(ServerOwnedContinuousLearningRegistryLeafState.Absent, pending.PublicationBackup.State);
            Assert.Equal(ServerOwnedContinuousLearningRegistryLeafState.Present, pending.RollbackEvidence.State);
            var pendingEvidence = capability.DecodeRollbackEvidence(
                pending.RollbackEvidence.Snapshot!);
            Assert.Equal(
                ServerOwnedContinuousLearningRegistryCanonicalState.Absent,
                pendingEvidence.PriorState);
            Assert.True(pendingEvidence.RejectedArtifactMatches(rejected));
        }

        var rootFlushes = 0;
        using (var access = fixture.CreateStartup().AcquireRetainedFileAccess())
        using (var capability = access.AcquireRegistryRootCapability(
                   new ServerOwnedContinuousLearningRegistryRootCapabilityOptions
                   {
                       ObserveIoStageForTesting = stage =>
                       {
                           if (stage == ServerOwnedContinuousLearningRegistryIoStage
                                   .BeforeRootDirectoryFlush)
                           {
                               rootFlushes++;
                           }
                       },
                   }))
        {
            var result = capability.ReconcileRejectedPublication(
                rejected,
                ServerOwnedContinuousLearningRegistryPriorStateEvidence.Absent());
            Assert.Equal(
                ServerOwnedContinuousLearningRegistryReconcileDisposition
                    .AlreadyRestoredAbsence,
                result.Disposition);
            Assert.Equal(2, rootFlushes);
            AssertAllAbsent(result.FinalInventory);
        }
    }

    [ContinuousLearningRegistryWindowsFact]
    public void RetryAfterPriorRestoreRename_RequiresExactFileAndRootFlushes()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        ServerOwnedContinuousLearningRegistrySnapshot prior;
        ServerOwnedContinuousLearningRegistrySnapshot rejected;

        using (var access = fixture.CreateStartup().AcquireRetainedFileAccess())
        using (var capability = access.AcquireRegistryRootCapability(
                   new ServerOwnedContinuousLearningRegistryRootCapabilityOptions
                   {
                       ObserveIoStageForTesting = stage =>
                       {
                           if (stage == ServerOwnedContinuousLearningRegistryIoStage
                                   .AfterPriorArtifactRestore)
                           {
                               throw new IOException("Injected crash after prior restore rename.");
                           }
                       },
                   }))
        {
            var priorDescriptor = capability.CreateWriteTemp(new byte[] { 6, 1, 6 });
            prior = capability.ReadExact(
                ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
                priorDescriptor.Identity);
            capability.RenameNoReplace(
                ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
                prior.Identity,
                ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry);
            capability.RenameNoReplace(
                ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry,
                prior.Identity,
                ServerOwnedContinuousLearningRegistryLeaf.PublicationBackup);
            var rejectedDescriptor = capability.CreateWriteTemp(new byte[] { 7, 2, 7 });
            rejected = capability.ReadExact(
                ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
                rejectedDescriptor.Identity);
            capability.RenameNoReplace(
                ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
                rejected.Identity,
                ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry);
            Assert.Throws<IOException>(() => capability.ReconcileRejectedPublication(
                rejected,
                ServerOwnedContinuousLearningRegistryPriorStateEvidence.Present(prior)));
        }

        using (var access = fixture.CreateStartup().AcquireRetainedFileAccess())
        using (var capability = access.AcquireRegistryRootCapability(
                   new ServerOwnedContinuousLearningRegistryRootCapabilityOptions
                   {
                       FailNativeCallForTesting = call =>
                           call == ServerOwnedContinuousLearningRegistryNativeCall.FileFlush,
                   }))
        {
            Assert.Throws<IOException>(() => capability.ReconcileRejectedPublication(
                rejected,
                ServerOwnedContinuousLearningRegistryPriorStateEvidence.Present(prior)));
            AssertPendingPriorRollback(capability, capability.Inventory(), prior, rejected);
        }

        using (var access = fixture.CreateStartup().AcquireRetainedFileAccess())
        using (var capability = access.AcquireRegistryRootCapability(
                   new ServerOwnedContinuousLearningRegistryRootCapabilityOptions
                   {
                       FailNativeCallForTesting = call =>
                           call == ServerOwnedContinuousLearningRegistryNativeCall
                               .RootDirectoryFlush,
                   }))
        {
            Assert.Throws<IOException>(() => capability.ReconcileRejectedPublication(
                rejected,
                ServerOwnedContinuousLearningRegistryPriorStateEvidence.Present(prior)));
            AssertPendingPriorRollback(capability, capability.Inventory(), prior, rejected);
        }

        var fileFlushes = 0;
        var rootFlushes = 0;
        using (var access = fixture.CreateStartup().AcquireRetainedFileAccess())
        using (var capability = access.AcquireRegistryRootCapability(
                   new ServerOwnedContinuousLearningRegistryRootCapabilityOptions
                   {
                       ObserveIoStageForTesting = stage =>
                       {
                           if (stage == ServerOwnedContinuousLearningRegistryIoStage.BeforeFileFlush)
                               fileFlushes++;
                           if (stage == ServerOwnedContinuousLearningRegistryIoStage.BeforeRootDirectoryFlush)
                               rootFlushes++;
                       },
                   }))
        {
            var result = capability.ReconcileRejectedPublication(
                rejected,
                ServerOwnedContinuousLearningRegistryPriorStateEvidence.Present(prior));
            Assert.Equal(
                ServerOwnedContinuousLearningRegistryReconcileDisposition.AlreadyRestoredPrior,
                result.Disposition);
            Assert.Equal(1, fileFlushes);
            Assert.Equal(3, rootFlushes);
            AssertOnlyCanonical(result.FinalInventory, prior);
        }
    }

    [ContinuousLearningRegistryWindowsFact]
    public void ReconcileRejectedPublication_AmbiguousOrForeignEvidenceNeverAltersLeaves()
    {
        RequireWindows();
        using (var fixture = new FileAccessFixture(provisionAllRoots: true))
        using (var access = fixture.CreateStartup().AcquireRetainedFileAccess())
        using (var capability = access.AcquireRegistryRootCapability())
        {
            var priorDescriptor = capability.CreateWriteTemp(new byte[] { 1, 0, 1 });
            var prior = capability.ReadExact(
                ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
                priorDescriptor.Identity);
            capability.RenameNoReplace(
                ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
                prior.Identity,
                ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry);
            capability.RenameNoReplace(
                ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry,
                prior.Identity,
                ServerOwnedContinuousLearningRegistryLeaf.PublicationBackup);
            var rejectedDescriptor = capability.CreateWriteTemp(new byte[] { 2, 0, 2 });
            var rejected = capability.ReadExact(
                ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
                rejectedDescriptor.Identity);
            capability.RenameNoReplace(
                ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
                rejected.Identity,
                ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry);
            _ = capability.CreateWriteTemp(new byte[] { 3, 0, 3 });
            var before = capability.Inventory();

            Assert.Throws<InvalidDataException>(() => capability.ReconcileRejectedPublication(
                rejected,
                ServerOwnedContinuousLearningRegistryPriorStateEvidence.Present(prior)));

            AssertInventoryEquivalent(before, capability.Inventory());
        }

        using (var fixture = new FileAccessFixture(provisionAllRoots: true))
        using (var access = fixture.CreateStartup().AcquireRetainedFileAccess())
        using (var capability = access.AcquireRegistryRootCapability())
        {
            var backupDescriptor = capability.CreateWriteTemp(new byte[] { 4, 0, 4 });
            capability.RenameNoReplace(
                ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
                backupDescriptor.Identity,
                ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry);
            capability.RenameNoReplace(
                ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry,
                backupDescriptor.Identity,
                ServerOwnedContinuousLearningRegistryLeaf.PublicationBackup);
            var rejectedDescriptor = capability.CreateWriteTemp(new byte[] { 5, 0, 5 });
            var rejected = capability.ReadExact(
                ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
                rejectedDescriptor.Identity);
            var before = capability.Inventory();

            Assert.Throws<InvalidDataException>(() => capability.ReconcileRejectedPublication(
                rejected,
                ServerOwnedContinuousLearningRegistryPriorStateEvidence.Absent()));

            AssertInventoryEquivalent(before, capability.Inventory());
        }

        ServerOwnedContinuousLearningRegistrySnapshot foreignEvidence;
        using (var foreignFixture = new FileAccessFixture(provisionAllRoots: true))
        using (var foreignAccess = foreignFixture.CreateStartup().AcquireRetainedFileAccess())
        using (var foreignCapability = foreignAccess.AcquireRegistryRootCapability())
        {
            var descriptor = foreignCapability.CreateWriteTemp(new byte[] { 9, 9, 0 });
            foreignEvidence = foreignCapability.ReadExact(
                ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
                descriptor.Identity);
        }

        using (var fixture = new FileAccessFixture(provisionAllRoots: true))
        using (var access = fixture.CreateStartup().AcquireRetainedFileAccess())
        using (var capability = access.AcquireRegistryRootCapability())
        {
            var priorDescriptor = capability.CreateWriteTemp(new byte[] { 6, 0, 6 });
            var prior = capability.ReadExact(
                ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
                priorDescriptor.Identity);
            capability.RenameNoReplace(
                ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
                prior.Identity,
                ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry);
            capability.RenameNoReplace(
                ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry,
                prior.Identity,
                ServerOwnedContinuousLearningRegistryLeaf.PublicationBackup);
            var rejectedDescriptor = capability.CreateWriteTemp(new byte[] { 7, 0, 7 });
            capability.RenameNoReplace(
                ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
                rejectedDescriptor.Identity,
                ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry);
            var before = capability.Inventory();

            Assert.Throws<InvalidDataException>(() => capability.ReconcileRejectedPublication(
                foreignEvidence,
                ServerOwnedContinuousLearningRegistryPriorStateEvidence.Present(prior)));

            AssertInventoryEquivalent(before, capability.Inventory());
        }
    }

    private static bool IsForbiddenSurfaceType(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return type == typeof(string) ||
               type == typeof(FileInfo) ||
               type == typeof(DirectoryInfo) ||
               type == typeof(FileStream) ||
               type == typeof(Stream) ||
               typeof(SafeHandle).IsAssignableFrom(type);
    }

    private static void AssertObservation(
        ServerOwnedContinuousLearningRegistryLeafObservation observation,
        ServerOwnedContinuousLearningRegistryLeaf expectedLeaf,
        ServerOwnedContinuousLearningRegistryFileIdentity expectedIdentity,
        byte[] expectedBytes)
    {
        Assert.Equal(expectedLeaf, observation.Leaf);
        Assert.Equal(ServerOwnedContinuousLearningRegistryLeafState.Present, observation.State);
        Assert.NotNull(observation.Snapshot);
        Assert.Equal(expectedIdentity, observation.Snapshot!.Identity);
        Assert.Equal(expectedBytes, observation.Snapshot.CopyBytes());
        Assert.Equal(
            ServerOwnedContinuousLearningRegistryContentHash.FromBytes(expectedBytes),
            observation.Snapshot.Sha256);
    }

    private static void AssertAllAbsent(
        ServerOwnedContinuousLearningRegistryNamespaceInventory inventory)
    {
        Assert.Equal(ServerOwnedContinuousLearningRegistryLeafState.Absent, inventory.CanonicalRegistry.State);
        Assert.Equal(ServerOwnedContinuousLearningRegistryLeafState.Absent, inventory.WriteTemp.State);
        Assert.Equal(ServerOwnedContinuousLearningRegistryLeafState.Absent, inventory.PublicationBackup.State);
        Assert.Equal(ServerOwnedContinuousLearningRegistryLeafState.Absent, inventory.RollbackEvidence.State);
        Assert.Equal(ServerOwnedContinuousLearningRegistryLeafState.Absent, inventory.AcceptedStaging.State);
        Assert.Equal(ServerOwnedContinuousLearningRegistryLeafState.Absent, inventory.AcceptedEvidence.State);
    }

    private static void AssertOnlyCanonical(
        ServerOwnedContinuousLearningRegistryNamespaceInventory inventory,
        ServerOwnedContinuousLearningRegistrySnapshot expected)
    {
        Assert.Equal(
            ServerOwnedContinuousLearningRegistryLeafState.Present,
            inventory.CanonicalRegistry.State);
        Assert.NotNull(inventory.CanonicalRegistry.Snapshot);
        Assert.Equal(expected.CopyBytes(), inventory.CanonicalRegistry.Snapshot!.CopyBytes());
        Assert.Equal(ServerOwnedContinuousLearningRegistryLeafState.Absent, inventory.WriteTemp.State);
        Assert.Equal(ServerOwnedContinuousLearningRegistryLeafState.Absent, inventory.PublicationBackup.State);
        Assert.Equal(ServerOwnedContinuousLearningRegistryLeafState.Absent, inventory.RollbackEvidence.State);
        Assert.Equal(ServerOwnedContinuousLearningRegistryLeafState.Absent, inventory.AcceptedStaging.State);
        Assert.Equal(ServerOwnedContinuousLearningRegistryLeafState.Absent, inventory.AcceptedEvidence.State);
    }

    private static void AssertPendingPriorRollback(
        ServerOwnedContinuousLearningRegistryRootCapability capability,
        ServerOwnedContinuousLearningRegistryNamespaceInventory inventory,
        ServerOwnedContinuousLearningRegistrySnapshot expected,
        ServerOwnedContinuousLearningRegistrySnapshot rejected)
    {
        Assert.Equal(
            ServerOwnedContinuousLearningRegistryLeafState.Present,
            inventory.CanonicalRegistry.State);
        Assert.Equal(expected.CopyBytes(), inventory.CanonicalRegistry.Snapshot!.CopyBytes());
        Assert.Equal(ServerOwnedContinuousLearningRegistryLeafState.Absent, inventory.WriteTemp.State);
        Assert.Equal(
            ServerOwnedContinuousLearningRegistryLeafState.Present,
            inventory.PublicationBackup.State);
        Assert.Equal(expected.CopyBytes(), inventory.PublicationBackup.Snapshot!.CopyBytes());
        Assert.Equal(
            ServerOwnedContinuousLearningRegistryLeafState.Present,
            inventory.RollbackEvidence.State);
        var evidence = capability.DecodeRollbackEvidence(
            inventory.RollbackEvidence.Snapshot!);
        Assert.True(evidence.PriorArtifactMatches(expected));
        Assert.True(evidence.RejectedArtifactMatches(rejected));
    }

    private static void AssertInventoryEquivalent(
        ServerOwnedContinuousLearningRegistryNamespaceInventory expected,
        ServerOwnedContinuousLearningRegistryNamespaceInventory actual)
    {
        AssertObservationEquivalent(expected.CanonicalRegistry, actual.CanonicalRegistry);
        AssertObservationEquivalent(expected.WriteTemp, actual.WriteTemp);
        AssertObservationEquivalent(expected.PublicationBackup, actual.PublicationBackup);
        AssertObservationEquivalent(expected.RollbackEvidence, actual.RollbackEvidence);
        AssertObservationEquivalent(expected.AcceptedStaging, actual.AcceptedStaging);
        AssertObservationEquivalent(expected.AcceptedEvidence, actual.AcceptedEvidence);
    }

    private static void AssertObservationEquivalent(
        ServerOwnedContinuousLearningRegistryLeafObservation expected,
        ServerOwnedContinuousLearningRegistryLeafObservation actual)
    {
        Assert.Equal(expected.Leaf, actual.Leaf);
        Assert.Equal(expected.State, actual.State);
        if (expected.Snapshot is null)
        {
            Assert.Null(actual.Snapshot);
            return;
        }

        Assert.NotNull(actual.Snapshot);
        Assert.Equal(expected.Snapshot.Identity, actual.Snapshot!.Identity);
        Assert.Equal(expected.Snapshot.Length, actual.Snapshot.Length);
        Assert.Equal(expected.Snapshot.Sha256, actual.Snapshot.Sha256);
        Assert.Equal(expected.Snapshot.CopyBytes(), actual.Snapshot.CopyBytes());
    }

    private static IEnumerable<(
        ServerOwnedContinuousLearningRegistryLeaf Source,
        ServerOwnedContinuousLearningRegistryLeaf Destination)>
        DisallowedRenameEdges()
    {
        foreach (var source in Enum.GetValues<ServerOwnedContinuousLearningRegistryLeaf>())
        foreach (var destination in Enum.GetValues<ServerOwnedContinuousLearningRegistryLeaf>())
        {
            var allowed = source == ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry &&
                          destination == ServerOwnedContinuousLearningRegistryLeaf.PublicationBackup ||
                          source == ServerOwnedContinuousLearningRegistryLeaf.WriteTemp &&
                          destination == ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry ||
                          source == ServerOwnedContinuousLearningRegistryLeaf.PublicationBackup &&
                          destination == ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry ||
                          source == ServerOwnedContinuousLearningRegistryLeaf.AcceptedStaging &&
                          destination == ServerOwnedContinuousLearningRegistryLeaf.AcceptedEvidence;
            if (!allowed)
                yield return (source, destination);
        }
    }

    private static SafeFileHandle GetRetainedRootHandle(
        ServerOwnedContinuousLearningFileAccess access,
        ContinuousLearningRetainedObjectRole expectedRole)
    {
        var entries = Assert.IsAssignableFrom<Array>(
            typeof(ServerOwnedContinuousLearningFileAccess)
                .GetField("_entries", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(access));
        foreach (var entry in entries)
        {
            var role = Assert.IsType<ContinuousLearningRetainedObjectRole>(
                entry!.GetType().GetProperty("Role")!.GetValue(entry));
            if (role == expectedRole)
            {
                return Assert.IsType<SafeFileHandle>(
                    entry.GetType().GetProperty("Handle")!.GetValue(entry));
            }
        }
        throw new Xunit.Sdk.XunitException("The expected retained root was not found.");
    }

    private static int RunCrossProcessOwnerLockProbe(string registryRoot)
    {
        var helperPath = ResolveRegistryLockHolderPath();
        var dotnetHost = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH")
                         ?? Environment.ProcessPath;
        Assert.False(string.IsNullOrWhiteSpace(dotnetHost));
        Assert.True(File.Exists(dotnetHost), "The dotnet host is unavailable.");
        Assert.True(File.Exists(helperPath), "The owner-lock probe helper is unavailable.");

        using var child = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = dotnetHost,
                UseShellExecute = false,
                RedirectStandardError = true,
                CreateNoWindow = true,
            },
        };
        child.StartInfo.ArgumentList.Add(helperPath);
        child.StartInfo.ArgumentList.Add("--probe-exclusive-owner");
        child.StartInfo.ArgumentList.Add(registryRoot);
        child.StartInfo.ArgumentList.Add("registry");
        Assert.True(child.Start());
        Assert.True(child.WaitForExit(10_000), "Owner-lock probe timed out.");
        var stderr = child.StandardError.ReadToEnd();
        Assert.True(
            string.IsNullOrWhiteSpace(stderr),
            $"Owner-lock probe reported stderr: {stderr}");
        return child.ExitCode;
    }

    private static string ResolveRegistryLockHolderPath()
    {
        var frameworkDirectory = new DirectoryInfo(AppContext.BaseDirectory);
        var configurationDirectory = frameworkDirectory.Parent
            ?? throw new InvalidOperationException("Test configuration directory is unavailable.");
        var repositoryRoot = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            ".."));
        return Path.GetFullPath(Path.Combine(
            repositoryRoot,
            "Tests.Tools",
            "RegistryLockHolder",
            "bin",
            configurationDirectory.Name,
            frameworkDirectory.Name,
            "RegistryLockHolder.dll"));
    }

    private static void RequireWindows()
    {
        if (OperatingSystem.IsWindows())
            return;

        // Это legacy Windows-oracle полоса: она проверяет Windows locks,
        // handles и share-mode. На Linux её заменяет отдельный POSIX stripe.
        throw Xunit.Sdk.SkipException.ForSkip(
            "WINDOWS_ONLY_CAPABILITY_ORACLE: R1.3b RegistryRoot capability tests require Windows execution.");
    }

    private static void PrepareLinuxOwnerWriteBoundary(FileAccessFixture fixture)
    {
        if (!OperatingSystem.IsLinux())
            return;

        SetLinuxOwnerWritableDirectory(fixture.Root);
        SetLinuxOwnerWritableDirectory(fixture.AdapterRoot);
        SetLinuxOwnerWritableDirectory(fixture.CacheRoot);
        SetLinuxOwnerWritableDirectory(fixture.RegistryRoot);
        if (Directory.Exists(fixture.PairRoot))
            SetLinuxOwnerWritableDirectory(fixture.PairRoot);
        SetLinuxOwnerWritableFile(fixture.BaseCheckpointPath);
        SetLinuxOwnerWritableFile(fixture.TokenizerPath);
    }

    private static void ReplaceDirectoryWithFreshOwnerBoundary(string directory)
    {
        var moved = directory + ".replaced";
        Directory.Move(directory, moved);
        Directory.CreateDirectory(directory);
        SetLinuxOwnerWritableDirectory(directory);
    }

    private static void SetLinuxOwnerWritableDirectory(string path)
    {
        if (!OperatingSystem.IsLinux())
            return;

        File.SetUnixFileMode(
            path,
            UnixFileMode.UserRead |
            UnixFileMode.UserWrite |
            UnixFileMode.UserExecute);
    }

    private static void SetLinuxOwnerWritableFile(string path)
    {
        if (!OperatingSystem.IsLinux())
            return;

        File.SetUnixFileMode(
            path,
            UnixFileMode.UserRead |
            UnixFileMode.UserWrite);
    }

    private static void SetLinuxGroupWritableFile(string path)
    {
        if (!OperatingSystem.IsLinux())
            return;

        // Здесь намеренно включается group write: это проверка fail-closed lease-границы.
        File.SetUnixFileMode(
            path,
            UnixFileMode.UserRead |
            UnixFileMode.UserWrite |
            UnixFileMode.GroupWrite);
    }

    private static void CreateHardLinkPosixOrFail(
        string linkPath,
        string existingPath)
    {
        if (LinkPosix(existingPath, linkPath) == 0)
            return;

        throw new Xunit.Sdk.XunitException(
            $"POSIX hard-link setup failed with errno {Marshal.GetLastPInvokeError()}.");
    }

    [DllImport("libc", EntryPoint = "link", SetLastError = true)]
    private static extern int LinkPosix(
        string existingPath,
        string linkPath);
}

file sealed class ContinuousLearningRegistryCapabilityLinuxX64FactAttribute : Xunit.FactAttribute
{
    public ContinuousLearningRegistryCapabilityLinuxX64FactAttribute()
    {
        if (!OperatingSystem.IsLinux())
        {
            Skip = "LINUX_ONLY_CAPABILITY_ORACLE: test requires Linux.";
            return;
        }

        if (RuntimeInformation.ProcessArchitecture != Architecture.X64)
            Skip = "LINUX_X64_ONLY_CAPABILITY_ORACLE: test requires Linux x64.";
    }
}

file sealed class ContinuousLearningRegistryWindowsFactAttribute : Xunit.FactAttribute
{
    public ContinuousLearningRegistryWindowsFactAttribute()
    {
        if (!OperatingSystem.IsWindows())
        {
            // Старый registry oracle завязан на Windows handles/share-mode.
            // В WSL он должен быть discovery-time skip, а не runtime failure.
            Skip = "WINDOWS_ONLY_CAPABILITY_ORACLE: R1.3b RegistryRoot capability tests require Windows execution.";
        }
    }
}

file sealed class ContinuousLearningRegistryWindowsTheoryAttribute : Xunit.TheoryAttribute
{
    public ContinuousLearningRegistryWindowsTheoryAttribute()
    {
        if (!OperatingSystem.IsWindows())
        {
            // Теории Windows-полосы не удалены: они исполняются на Windows,
            // а POSIX-регрессии исполняются отдельными Linux x64 facts.
            Skip = "WINDOWS_ONLY_CAPABILITY_ORACLE: R1.3b RegistryRoot capability tests require Windows execution.";
        }
    }
}
