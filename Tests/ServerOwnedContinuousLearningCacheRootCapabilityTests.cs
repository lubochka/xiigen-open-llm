// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using ContinuousLearning;
using Microsoft.Win32.SafeHandles;

public sealed class ServerOwnedContinuousLearningCacheRootCapabilityTests
{
    private const string OwnerLockName = ".semantic-compiler.owner.lock";
    private const string CanonicalCacheName = "semantic-compiler-cache-v1.bin";
    private const string WriteTempName = ".semantic-compiler-cache-v1.write.tmp";
    private const string PublicationBackupName =
        ".semantic-compiler-cache-v1.publication-backup";
    private const string AuditLogName = "semantic-compiler-audit-v1.log";

    [ContinuousLearningCacheCapabilityLinuxX64Fact]
    public void PosixWriteAndReadStayPortableButRenameAndDeleteFailClosed()
    {
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        PrepareLinuxOwnerWriteBoundary(fixture);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireCompilerCacheRootCapability();

        // Linux сохраняет только безопасную часть cache capability: create-only
        // leaf и exact read через retained dirfd. Replace/delete не получают
        // фальшивую "точность", потому что kernel мутирует имя, а не fd.
        var first = capability.CreateWriteTemp(new byte[] { 0x31, 0x32, 0x33 }, 64);
        Assert.Equal(
            [0x31, 0x32, 0x33],
            capability.ReadBoundedExact(
                ServerOwnedContinuousLearningCacheLeaf.WriteTemp,
                first,
                64));

        Assert.Throws<ContinuousLearningPosixExactMutationUnsupportedException>(
            () => capability.RenameNoReplace(
                ServerOwnedContinuousLearningCacheLeaf.WriteTemp,
                first,
                ServerOwnedContinuousLearningCacheLeaf.CanonicalCache,
                64));
        Assert.Equal(
            [0x31, 0x32, 0x33],
            File.ReadAllBytes(Path.Combine(fixture.CacheRoot, WriteTempName)));
        Assert.False(File.Exists(Path.Combine(fixture.CacheRoot, CanonicalCacheName)));

        Assert.Throws<ContinuousLearningPosixExactMutationUnsupportedException>(
            () => capability.DeleteExactOpenedIdentity(
                ServerOwnedContinuousLearningCacheLeaf.WriteTemp,
                first,
                64));
        Assert.Equal(
            [0x31, 0x32, 0x33],
            capability.ReadBoundedExact(
                ServerOwnedContinuousLearningCacheLeaf.WriteTemp,
                first,
                64));
        access.EnsureRetained();
    }

    [ContinuousLearningCacheCapabilityLinuxX64Fact]
    public void PosixExactDeleteSubstitutionWindowFailsClosedAndBothFilesSurvive()
    {
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        PrepareLinuxOwnerWriteBoundary(fixture);
        var movedOriginalPath = Path.Combine(fixture.CacheRoot, "original.moved");
        var canonicalPath = Path.Combine(fixture.CacheRoot, WriteTempName);
        var foreignBytes = new byte[] { 0xF0, 0x0D };
        var substituted = false;
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireCompilerCacheRootCapability(
            new ServerOwnedContinuousLearningCacheRootCapabilityOptions
            {
                ObserveIoStageForTesting = stage =>
                {
                    if (stage != ServerOwnedContinuousLearningCacheIoStage
                            .BeforeExactDelete ||
                        substituted)
                    {
                        return;
                    }

                    // Атака происходит после fd identity validation и до
                    // старого unlinkat: имя снова указывает на чужие bytes.
                    substituted = true;
                    File.Move(canonicalPath, movedOriginalPath);
                    File.WriteAllBytes(canonicalPath, foreignBytes);
                    SetLinuxOwnerWritableFile(canonicalPath);
                },
            });

        var originalIdentity = capability.CreateWriteTemp(
            new byte[] { 0x31, 0x32, 0x33 },
            64);

        Assert.Throws<ContinuousLearningPosixExactMutationUnsupportedException>(
            () => capability.DeleteExactOpenedIdentity(
                ServerOwnedContinuousLearningCacheLeaf.WriteTemp,
                originalIdentity,
                64));

        Assert.True(substituted);
        Assert.Equal(new byte[] { 0x31, 0x32, 0x33 }, File.ReadAllBytes(movedOriginalPath));
        Assert.Equal(foreignBytes, File.ReadAllBytes(canonicalPath));
        access.EnsureRetained();
    }

    [ContinuousLearningCacheCapabilityLinuxX64Fact]
    public void PosixRenameSourceSubstitutionWindowFailsClosedAndBothFilesSurvive()
    {
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        PrepareLinuxOwnerWriteBoundary(fixture);
        var movedOriginalPath = Path.Combine(fixture.CacheRoot, "rename-source.moved");
        var sourcePath = Path.Combine(fixture.CacheRoot, WriteTempName);
        var destinationPath = Path.Combine(fixture.CacheRoot, CanonicalCacheName);
        var foreignBytes = new byte[] { 0xAA, 0xBB };
        var substituted = false;
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireCompilerCacheRootCapability(
            new ServerOwnedContinuousLearningCacheRootCapabilityOptions
            {
                ObserveIoStageForTesting = stage =>
                {
                    if (stage != ServerOwnedContinuousLearningCacheIoStage
                            .BeforeHandleRename ||
                        substituted)
                    {
                        return;
                    }

                    // Старый renameat2 брал бы sourceName заново и мог бы
                    // переместить чужой replacement. Новый путь валится до
                    // mutation, сохраняя оба имени.
                    substituted = true;
                    File.Move(sourcePath, movedOriginalPath);
                    File.WriteAllBytes(sourcePath, foreignBytes);
                    SetLinuxOwnerWritableFile(sourcePath);
                },
            });

        var originalIdentity = capability.CreateWriteTemp(
            new byte[] { 0x41, 0x42, 0x43 },
            64);

        Assert.Throws<ContinuousLearningPosixExactMutationUnsupportedException>(
            () => capability.RenameNoReplace(
                ServerOwnedContinuousLearningCacheLeaf.WriteTemp,
                originalIdentity,
                ServerOwnedContinuousLearningCacheLeaf.CanonicalCache,
                64));

        Assert.True(substituted);
        Assert.Equal(new byte[] { 0x41, 0x42, 0x43 }, File.ReadAllBytes(movedOriginalPath));
        Assert.Equal(foreignBytes, File.ReadAllBytes(sourcePath));
        Assert.False(File.Exists(destinationPath));
        access.EnsureRetained();
    }

    [ContinuousLearningCacheCapabilityLinuxX64Fact]
    public void PosixUnsafeCanonicalLeaves_AreRejectedWithoutFollowingOrAliasing()
    {
        using var symlinkFixture = new FileAccessFixture(provisionAllRoots: true);
        PrepareLinuxOwnerWriteBoundary(symlinkFixture);
        var outsideTarget = Path.Combine(symlinkFixture.Root, "outside-cache-target.bin");
        File.WriteAllBytes(outsideTarget, [0xEE]);
        SetLinuxOwnerWritableFile(outsideTarget);
        File.CreateSymbolicLink(
            Path.Combine(symlinkFixture.CacheRoot, CanonicalCacheName),
            outsideTarget);

        using (var access = symlinkFixture.CreateStartup().AcquireRetainedFileAccess())
        using (var capability = access.AcquireCompilerCacheRootCapability())
        {
            // Угроза: path fallback прочитает target symlink за пределами CacheRoot.
            // Доверенная ветка обязана остаться в root fd + O_NOFOLLOW и бросить.
            Assert.Throws<IOException>(() => capability.Inventory(5));
            Assert.ThrowsAny<Exception>(() => capability.ReadBounded(
                ServerOwnedContinuousLearningCacheLeaf.CanonicalCache,
                64));
            Assert.Equal(new byte[] { 0xEE }, File.ReadAllBytes(outsideTarget));
            access.EnsureRetained();
        }

        using var hardlinkFixture = new FileAccessFixture(provisionAllRoots: true);
        PrepareLinuxOwnerWriteBoundary(hardlinkFixture);
        var canonical = Path.Combine(hardlinkFixture.CacheRoot, CanonicalCacheName);
        File.WriteAllBytes(canonical, [0x44]);
        SetLinuxOwnerWritableFile(canonical);
        CreateHardLinkPosixOrFail(
            Path.Combine(hardlinkFixture.CacheRoot, "unconfigured-cache-alias.bin"),
            canonical);

        using (var access = hardlinkFixture.CreateStartup().AcquireRetainedFileAccess())
        using (var capability = access.AcquireCompilerCacheRootCapability())
        {
            // Угроза: nlink>1 позволяет менять те же bytes через неучтённое имя.
            // POSIX capability поэтому принимает только одиночный regular inode.
            Assert.ThrowsAny<Exception>(() => capability.OpenExistingRegularLeaf(
                ServerOwnedContinuousLearningCacheLeaf.CanonicalCache,
                64));
            access.EnsureRetained();
        }
    }

    [ContinuousLearningCacheCapabilityLinuxX64Fact]
    public void PosixGroupWritableOwnerLock_IsRejectedBeforeCapabilityBinds()
    {
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        PrepareLinuxOwnerWriteBoundary(fixture);
        var lockPath = Path.Combine(fixture.CacheRoot, OwnerLockName);
        File.WriteAllBytes(lockPath, [0x21]);
        SetLinuxGroupWritableFile(lockPath);

        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();

        // Owner-lock тоже часть mutation boundary: group/other writable lock
        // нельзя принимать, иначе сторонний uid/gid сможет участвовать в lease.
        Assert.ThrowsAny<Exception>(() => access.AcquireCompilerCacheRootCapability());
        access.EnsureRetained();
    }

    [ContinuousLearningCacheCapabilityLinuxX64Fact]
    public void PosixRootReplacementDuringFlush_InvalidatesRetainedCapability()
    {
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        PrepareLinuxOwnerWriteBoundary(fixture);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        var replaced = false;
        using var capability = access.AcquireCompilerCacheRootCapability(
            new ServerOwnedContinuousLearningCacheRootCapabilityOptions
            {
                ObserveIoStageForTesting = stage =>
                {
                    if (stage != ServerOwnedContinuousLearningCacheIoStage
                            .BeforeRootDirectoryFlush ||
                        replaced)
                    {
                        return;
                    }

                    replaced = true;
                    ReplaceDirectoryWithFreshOwnerBoundary(fixture.CacheRoot);
                },
            });

        // Угроза delete/recreate: старый fd ещё жив, но имя root уже указывает
        // на другой inode. После операции trust обязан стать sticky-lost.
        Assert.Throws<ContinuousLearningPathIdentityException>(
            capability.FlushRootDirectory);
        Assert.True(replaced);
        Assert.Throws<ContinuousLearningPathIdentityException>(access.EnsureRetained);
    }

    [ContinuousLearningCacheWindowsFact]
    public void Capability_SurfaceIsStringFreeFixedProtocolAndOwnerLockIsLifetimeExclusive()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var firstAccess = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = firstAccess.AcquireCompilerCacheRootCapability();
        var ownerLockHandle = GetOwnerLockHandle(capability);

        var type = typeof(ServerOwnedContinuousLearningCacheRootCapability);
        Assert.False(type.IsPublic);
        Assert.True(type.IsSealed);
        var fields = type.GetFields(BindingFlags.Instance |
                                    BindingFlags.Public |
                                    BindingFlags.NonPublic);
        Assert.DoesNotContain(fields, field => field.FieldType == typeof(string));
        var nativeHandleField = Assert.Single(
            fields,
            field => field.FieldType == typeof(SafeFileHandle));
        Assert.True(nativeHandleField.IsPrivate);
        Assert.DoesNotContain(
            fields,
            field => field.FieldType == typeof(SafeFileHandle) && !field.IsPrivate);

        var methods = type.GetMethods(BindingFlags.Instance |
                                      BindingFlags.Static |
                                      BindingFlags.Public |
                                      BindingFlags.NonPublic |
                                      BindingFlags.DeclaredOnly)
            .Where(IsPublicOrInternal)
            .ToArray();
        Assert.DoesNotContain(methods, method =>
            method.ReturnType == typeof(string) ||
            method.ReturnType == typeof(SafeFileHandle) ||
            method.GetParameters().Any(parameter =>
                parameter.ParameterType == typeof(string) ||
                parameter.ParameterType == typeof(SafeFileHandle)));
        Assert.DoesNotContain(
            type.GetProperties(BindingFlags.Instance |
                               BindingFlags.Static |
                               BindingFlags.Public |
                               BindingFlags.NonPublic),
            property => property.PropertyType == typeof(string) ||
                        property.PropertyType == typeof(SafeFileHandle));
        Assert.Equal(
            new[]
            {
                "OwnerLock",
                "CanonicalCache",
                "WriteTemp",
                "PublicationBackup",
                "AuditLog",
            },
            Enum.GetNames<ServerOwnedContinuousLearningCacheLeaf>());
        var exactReadSeam = typeof(ServerOwnedContinuousLearningCacheRootCapabilityOptions)
            .GetProperty(
                "OverrideExactReadPostValidationForTesting",
                BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(exactReadSeam);
        Assert.True(exactReadSeam!.GetMethod!.IsAssembly);
        Assert.DoesNotContain(
            exactReadSeam.PropertyType.GenericTypeArguments,
            argument => argument == typeof(string) || argument == typeof(SafeFileHandle));
        Assert.DoesNotContain(
            typeof(ServerOwnedContinuousLearningCacheExactReadPostValidation)
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic),
            field => field.FieldType == typeof(string) || field.FieldType == typeof(SafeFileHandle));
        AssertExactConfiguredPathsNotReachable(
            [firstAccess, capability],
            fixture.ConfiguredPaths);
        Assert.Equal(
            new[] { OwnerLockName },
            Directory.GetFileSystemEntries(fixture.CacheRoot)
                .Select(Path.GetFileName)
                .ToArray());
        Assert.Equal(
            ServerOwnedContinuousLearningCacheLeaf.OwnerLock,
            Assert.Single(capability.Inventory(1)).Leaf);

        using (var blocked = OpenOwnerLock(fixture.CacheRoot))
        {
            Assert.True(blocked.IsInvalid);
            Assert.Equal(32, Marshal.GetLastPInvokeError());
        }
        var collision = Assert.Throws<
            ContinuousLearningMutationAdmissionDomainAlreadyOwnedException>(
            () => fixture.CreateStartup().AcquireRetainedFileAccess());
        Assert.Equal(
            ContinuousLearningMutationAdmissionDomainAlreadyOwnedException
                .StableErrorCode,
            collision.Message);

        capability.Dispose();
        Assert.True(ownerLockHandle.IsClosed);
        using (var available = OpenOwnerLock(fixture.CacheRoot))
            Assert.False(available.IsInvalid);
        firstAccess.Dispose();
        Assert.Throws<ContinuousLearningPathIdentityException>(
            () => firstAccess.AcquireCompilerCacheRootCapability());
        using var replacementAccess = fixture.CreateStartup()
            .AcquireRetainedFileAccess();
        using var replacementCapability = replacementAccess
            .AcquireCompilerCacheRootCapability();
        Assert.NotNull(replacementCapability);
    }

    [ContinuousLearningCacheWindowsFact]
    public void ProtocolHappyPath_UsesExactNamesAndExplicitDurabilityStages()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        var stages = new List<ServerOwnedContinuousLearningCacheIoStage>();
        using var capability = access.AcquireCompilerCacheRootCapability(
            new ServerOwnedContinuousLearningCacheRootCapabilityOptions
            {
                ObserveIoStageForTesting = stages.Add,
            });

        var temporaryIdentity = capability.CreateWriteTemp(
            new byte[] { 0x10, 0x20, 0x30, 0x40 },
            maximumBytes: 32);
        Assert.Equal(
            new[]
            {
                ServerOwnedContinuousLearningCacheIoStage.BeforeFileFlush,
                ServerOwnedContinuousLearningCacheIoStage.AfterFileFlush,
            },
            stages);
        stages.Clear();
        capability.FlushFile(
            ServerOwnedContinuousLearningCacheLeaf.WriteTemp,
            temporaryIdentity,
            maximumBytes: 32);
        Assert.Equal(
            new[]
            {
                ServerOwnedContinuousLearningCacheIoStage.BeforeFileFlush,
                ServerOwnedContinuousLearningCacheIoStage.AfterFileFlush,
            },
            stages);

        stages.Clear();
        capability.RenameNoReplace(
            ServerOwnedContinuousLearningCacheLeaf.WriteTemp,
            temporaryIdentity,
            ServerOwnedContinuousLearningCacheLeaf.CanonicalCache,
            maximumBytes: 32);
        Assert.Equal(
            new[]
            {
                ServerOwnedContinuousLearningCacheIoStage.BeforeHandleRename,
                ServerOwnedContinuousLearningCacheIoStage.AfterHandleRename,
            },
            stages);
        Assert.DoesNotContain(
            ServerOwnedContinuousLearningCacheIoStage.AfterRootDirectoryFlush,
            stages);
        stages.Clear();
        capability.FlushRootDirectory();
        Assert.Equal(
            new[]
            {
                ServerOwnedContinuousLearningCacheIoStage.BeforeRootDirectoryFlush,
                ServerOwnedContinuousLearningCacheIoStage.AfterRootDirectoryFlush,
            },
            stages);
        Assert.Equal(
            new byte[] { 0x10, 0x20, 0x30, 0x40 },
            capability.ReadBounded(
                ServerOwnedContinuousLearningCacheLeaf.CanonicalCache,
                32));

        stages.Clear();
        capability.RenameNoReplace(
            ServerOwnedContinuousLearningCacheLeaf.CanonicalCache,
            temporaryIdentity,
            ServerOwnedContinuousLearningCacheLeaf.PublicationBackup,
            maximumBytes: 32);
        capability.FlushRootDirectory();
        capability.RenameNoReplace(
            ServerOwnedContinuousLearningCacheLeaf.PublicationBackup,
            temporaryIdentity,
            ServerOwnedContinuousLearningCacheLeaf.CanonicalCache,
            maximumBytes: 32);
        capability.FlushRootDirectory();

        var recoveryTemp = capability.CreateWriteTemp(new byte[] { 0x77 }, 32);
        stages.Clear();
        capability.DeleteExactOpenedIdentity(
            ServerOwnedContinuousLearningCacheLeaf.WriteTemp,
            recoveryTemp,
            32);
        Assert.Equal(
            new[]
            {
                ServerOwnedContinuousLearningCacheIoStage.BeforeExactDelete,
                ServerOwnedContinuousLearningCacheIoStage.AfterExactDelete,
            },
            stages);
        Assert.DoesNotContain(
            ServerOwnedContinuousLearningCacheIoStage.AfterRootDirectoryFlush,
            stages);
        stages.Clear();
        capability.FlushRootDirectory();
        Assert.Equal(
            ServerOwnedContinuousLearningCacheIoStage.AfterRootDirectoryFlush,
            stages[^1]);

        Assert.Equal(
            new[] { OwnerLockName, CanonicalCacheName },
            Directory.GetFileSystemEntries(fixture.CacheRoot)
                .Select(Path.GetFileName)
                .Order(StringComparer.Ordinal)
                .ToArray());
        access.EnsureRetained();
    }

    [ContinuousLearningCacheWindowsFact]
    public void DurabilityNativeFailureInjection_PropagatesAndSuppressesSuccessStage()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        var stages = new List<ServerOwnedContinuousLearningCacheIoStage>();
        var failFileFlush = false;
        var failRootFlush = false;
        using var capability = access.AcquireCompilerCacheRootCapability(
            new ServerOwnedContinuousLearningCacheRootCapabilityOptions
            {
                ObserveIoStageForTesting = stages.Add,
                NativeErrorForTesting = call =>
                    failFileFlush &&
                    call == ServerOwnedContinuousLearningCacheNativeCall.FileFlush ||
                    failRootFlush &&
                    call == ServerOwnedContinuousLearningCacheNativeCall.RootDirectoryFlush
                        ? 5
                        : null,
            });
        var identity = capability.CreateWriteTemp(new byte[] { 1, 2, 3 }, 32);

        stages.Clear();
        failFileFlush = true;
        Assert.Throws<IOException>(() => capability.FlushFile(
            ServerOwnedContinuousLearningCacheLeaf.WriteTemp,
            identity,
            32));
        Assert.Equal(
            new[] { ServerOwnedContinuousLearningCacheIoStage.BeforeFileFlush },
            stages);
        failFileFlush = false;
        Assert.Equal(
            new byte[] { 1, 2, 3 },
            capability.ReadBounded(
                ServerOwnedContinuousLearningCacheLeaf.WriteTemp,
                32));

        stages.Clear();
        failRootFlush = true;
        Assert.Throws<IOException>(capability.FlushRootDirectory);
        Assert.Equal(
            new[]
            {
                ServerOwnedContinuousLearningCacheIoStage.BeforeRootDirectoryFlush,
            },
            stages);
        failRootFlush = false;
        stages.Clear();
        capability.FlushRootDirectory();
        Assert.Equal(
            ServerOwnedContinuousLearningCacheIoStage.AfterRootDirectoryFlush,
            stages[^1]);
        access.EnsureRetained();
    }

    [ContinuousLearningCacheWindowsTheory]
    [InlineData((int)ServerOwnedContinuousLearningCacheLeaf.WriteTemp)]
    [InlineData((int)ServerOwnedContinuousLearningCacheLeaf.PublicationBackup)]
    public void RecoveryDelete_EmitsDeleteStagesOnlyUntilExplicitRootFlush(
        int leafValue)
    {
        RequireWindows();
        var leaf = (ServerOwnedContinuousLearningCacheLeaf)leafValue;
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        var stages = new List<ServerOwnedContinuousLearningCacheIoStage>();
        using var capability = access.AcquireCompilerCacheRootCapability(
            new ServerOwnedContinuousLearningCacheRootCapabilityOptions
            {
                ObserveIoStageForTesting = stages.Add,
            });
        SeedSingleSource(capability, leaf);
        var identity = capability.QueryExactIdentity(leaf, 64);

        stages.Clear();
        capability.DeleteExactOpenedIdentity(leaf, identity, 64);
        Assert.Equal(
            new[]
            {
                ServerOwnedContinuousLearningCacheIoStage.BeforeExactDelete,
                ServerOwnedContinuousLearningCacheIoStage.AfterExactDelete,
            },
            stages);
        Assert.Throws<FileNotFoundException>(() =>
            capability.OpenExistingRegularLeaf(leaf, 64));
        stages.Clear();
        capability.FlushRootDirectory();
        Assert.Equal(
            new[]
            {
                ServerOwnedContinuousLearningCacheIoStage.BeforeRootDirectoryFlush,
                ServerOwnedContinuousLearningCacheIoStage.AfterRootDirectoryFlush,
            },
            stages);
        access.EnsureRetained();
    }

    [ContinuousLearningCacheWindowsFact]
    public void WriteSurface_CannotNameOrMutateForbiddenProtocolLeaves()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        File.WriteAllBytes(Path.Combine(fixture.CacheRoot, AuditLogName), [0xA0]);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireCompilerCacheRootCapability();
        var canonical = capability.CreateWriteTemp(new byte[] { 0xC0 }, 16);
        capability.RenameNoReplace(
            ServerOwnedContinuousLearningCacheLeaf.WriteTemp,
            canonical,
            ServerOwnedContinuousLearningCacheLeaf.CanonicalCache,
            16);
        capability.RenameNoReplace(
            ServerOwnedContinuousLearningCacheLeaf.CanonicalCache,
            canonical,
            ServerOwnedContinuousLearningCacheLeaf.PublicationBackup,
            16);
        _ = capability.CreateWriteTemp(new byte[] { 0xD0 }, 16);
        var before = ProtocolSnapshot(fixture.CacheRoot);

        var writeSurface = typeof(ServerOwnedContinuousLearningCacheRootCapability)
            .GetMethods(BindingFlags.Instance |
                        BindingFlags.Public |
                        BindingFlags.NonPublic |
                        BindingFlags.DeclaredOnly)
            .Where(IsPublicOrInternal)
            .Where(method => method.Name.Contains("Create", StringComparison.Ordinal) ||
                             method.Name.Contains("Write", StringComparison.Ordinal))
            .ToArray();
        var create = Assert.Single(writeSurface);
        Assert.Equal("CreateWriteTemp", create.Name);
        Assert.DoesNotContain(
            create.GetParameters(),
            parameter => parameter.ParameterType ==
                         typeof(ServerOwnedContinuousLearningCacheLeaf));

        foreach (var forbidden in new[]
                 {
                     ServerOwnedContinuousLearningCacheLeaf.OwnerLock,
                     ServerOwnedContinuousLearningCacheLeaf.CanonicalCache,
                     ServerOwnedContinuousLearningCacheLeaf.PublicationBackup,
                     ServerOwnedContinuousLearningCacheLeaf.AuditLog,
                 })
        {
            Assert.DoesNotContain(writeSurface, method =>
                method.GetParameters().Any(parameter =>
                    parameter.ParameterType ==
                    typeof(ServerOwnedContinuousLearningCacheLeaf)));
            Assert.Throws<IOException>(() => capability.CreateWriteTemp(
                new[] { checked((byte)((int)forbidden + 1)) },
                16));
            Assert.Equal(before, ProtocolSnapshot(fixture.CacheRoot));
            access.EnsureRetained();
        }
    }

    [ContinuousLearningCacheWindowsFact]
    public void EveryForbiddenRenameAndDelete_IsRejectedWithoutMutationOrTrustLoss()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        File.WriteAllBytes(Path.Combine(fixture.CacheRoot, AuditLogName), [0xA0]);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireCompilerCacheRootCapability();
        SeedAllMutableLeaves(capability);
        var before = ProtocolSnapshot(fixture.CacheRoot);
        var identity = capability.QueryExactIdentity(
            ServerOwnedContinuousLearningCacheLeaf.WriteTemp,
            64);

        foreach (var source in Enum.GetValues<ServerOwnedContinuousLearningCacheLeaf>())
        foreach (var destination in Enum.GetValues<ServerOwnedContinuousLearningCacheLeaf>())
        {
            if (IsAllowedRename(source, destination))
                continue;
            Assert.ThrowsAny<ArgumentException>(() => capability.RenameNoReplace(
                source,
                identity,
                destination,
                64));
            Assert.Equal(before, ProtocolSnapshot(fixture.CacheRoot));
            access.EnsureRetained();
        }

        foreach (var forbidden in new[]
                 {
                     ServerOwnedContinuousLearningCacheLeaf.OwnerLock,
                     ServerOwnedContinuousLearningCacheLeaf.CanonicalCache,
                     ServerOwnedContinuousLearningCacheLeaf.AuditLog,
                 })
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                capability.DeleteExactOpenedIdentity(forbidden, identity, 64));
            Assert.Equal(before, ProtocolSnapshot(fixture.CacheRoot));
            access.EnsureRetained();
        }
    }

    [ContinuousLearningCacheWindowsTheory]
    [InlineData(
        (int)ServerOwnedContinuousLearningCacheLeaf.WriteTemp,
        (int)ServerOwnedContinuousLearningCacheLeaf.CanonicalCache)]
    [InlineData(
        (int)ServerOwnedContinuousLearningCacheLeaf.CanonicalCache,
        (int)ServerOwnedContinuousLearningCacheLeaf.PublicationBackup)]
    [InlineData(
        (int)ServerOwnedContinuousLearningCacheLeaf.PublicationBackup,
        (int)ServerOwnedContinuousLearningCacheLeaf.CanonicalCache)]
    public void RenameNoReplace_WrongIdentityPreservesNamespaceAndContent(
        int sourceValue,
        int destinationValue)
    {
        RequireWindows();
        var source = (ServerOwnedContinuousLearningCacheLeaf)sourceValue;
        var destination = (ServerOwnedContinuousLearningCacheLeaf)destinationValue;
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireCompilerCacheRootCapability();
        SeedSingleSource(capability, source);
        var actual = capability.QueryExactIdentity(source, 64);
        var before = ProtocolSnapshot(fixture.CacheRoot);

        Assert.Throws<InvalidDataException>(() => capability.RenameNoReplace(
            source,
            WrongIdentity(actual),
            destination,
            64));

        Assert.Equal(before, ProtocolSnapshot(fixture.CacheRoot));
        access.EnsureRetained();
    }

    [ContinuousLearningCacheWindowsTheory]
    [InlineData((int)ServerOwnedContinuousLearningCacheLeaf.WriteTemp)]
    [InlineData((int)ServerOwnedContinuousLearningCacheLeaf.PublicationBackup)]
    public void DeleteExact_WrongIdentityPreservesNamespaceAndContent(
        int leafValue)
    {
        RequireWindows();
        var leaf = (ServerOwnedContinuousLearningCacheLeaf)leafValue;
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireCompilerCacheRootCapability();
        SeedSingleSource(capability, leaf);
        var actual = capability.QueryExactIdentity(leaf, 64);
        var before = ProtocolSnapshot(fixture.CacheRoot);

        Assert.Throws<InvalidDataException>(() =>
            capability.DeleteExactOpenedIdentity(
                leaf,
                WrongIdentity(actual),
                64));

        Assert.Equal(before, ProtocolSnapshot(fixture.CacheRoot));
        access.EnsureRetained();
    }

    [ContinuousLearningCacheWindowsTheory]
    [InlineData((int)ServerOwnedContinuousLearningCacheLeaf.WriteTemp)]
    [InlineData((int)ServerOwnedContinuousLearningCacheLeaf.CanonicalCache)]
    [InlineData((int)ServerOwnedContinuousLearningCacheLeaf.PublicationBackup)]
    public void FlushFile_WrongIdentityDoesNotReachNativeFlush(
        int leafValue)
    {
        RequireWindows();
        var leaf = (ServerOwnedContinuousLearningCacheLeaf)leafValue;
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        var stages = new List<ServerOwnedContinuousLearningCacheIoStage>();
        using var capability = access.AcquireCompilerCacheRootCapability(
            new ServerOwnedContinuousLearningCacheRootCapabilityOptions
            {
                ObserveIoStageForTesting = stages.Add,
            });
        SeedSingleSource(capability, leaf);
        stages.Clear();
        var actual = capability.QueryExactIdentity(leaf, 64);
        var before = ProtocolSnapshot(fixture.CacheRoot);

        Assert.Throws<InvalidDataException>(() => capability.FlushFile(
            leaf,
            WrongIdentity(actual),
            64));

        Assert.Empty(stages);
        Assert.Equal(before, ProtocolSnapshot(fixture.CacheRoot));
        access.EnsureRetained();
    }

    [ContinuousLearningCacheWindowsFact]
    public void ExclusiveTempCreateCollision_PreservesExistingBytesAndTrust()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireCompilerCacheRootCapability();
        _ = capability.CreateWriteTemp(new byte[] { 1, 2, 3 }, 16);
        var before = ProtocolSnapshot(fixture.CacheRoot);

        Assert.Throws<IOException>(() => capability.CreateWriteTemp(new byte[] { 9 }, 16));

        Assert.Equal(before, ProtocolSnapshot(fixture.CacheRoot));
        Assert.Equal(
            new byte[] { 1, 2, 3 },
            capability.ReadBounded(
                ServerOwnedContinuousLearningCacheLeaf.WriteTemp,
                16));
        access.EnsureRetained();
    }

    [ContinuousLearningCacheWindowsFact]
    public void ReadBoundedExact_BindsBytesToOneIdentityAndDeniesHeldLengthMutation()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        var mutationDenied = false;
        using var capability = access.AcquireCompilerCacheRootCapability(
            new ServerOwnedContinuousLearningCacheRootCapabilityOptions
            {
                ObserveIoStageForTesting = stage =>
                {
                    if (stage != ServerOwnedContinuousLearningCacheIoStage
                            .AfterBoundedExactReadPostValidationBeforeComparison)
                    {
                        return;
                    }
                    Assert.Throws<IOException>(() =>
                    {
                        using var writer = new FileStream(
                            Path.Combine(fixture.CacheRoot, WriteTempName),
                            FileMode.Open,
                            FileAccess.Write,
                            FileShare.ReadWrite | FileShare.Delete);
                        writer.SetLength(1);
                    });
                    mutationDenied = true;
                },
            });
        var expectedBytes = new byte[] { 1, 2, 3, 4 };
        var firstIdentity = capability.CreateWriteTemp(expectedBytes, 64);

        Assert.Equal(
            expectedBytes,
            capability.ReadBoundedExact(
                ServerOwnedContinuousLearningCacheLeaf.WriteTemp,
                firstIdentity,
                64));
        Assert.True(mutationDenied);

        capability.DeleteExactOpenedIdentity(
            ServerOwnedContinuousLearningCacheLeaf.WriteTemp,
            firstIdentity,
            64);
        var replacementIdentity = capability.CreateWriteTemp(new byte[] { 9 }, 64);
        Assert.NotEqual(firstIdentity, replacementIdentity);
        Assert.Throws<InvalidDataException>(() => capability.ReadBoundedExact(
            ServerOwnedContinuousLearningCacheLeaf.WriteTemp,
            firstIdentity,
            64));
        Assert.Equal(
            new byte[] { 9 },
            capability.ReadBoundedExact(
                ServerOwnedContinuousLearningCacheLeaf.WriteTemp,
                replacementIdentity,
                64));
        access.EnsureRetained();
    }

    [ContinuousLearningCacheWindowsTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void ReadBoundedExact_PostValidationIdentityOrLengthDriftIsRejected(
        bool changeIdentity)
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        var order = new List<string>();
        using var capability = access.AcquireCompilerCacheRootCapability(
            new ServerOwnedContinuousLearningCacheRootCapabilityOptions
            {
                ObserveIoStageForTesting = stage =>
                {
                    if (stage == ServerOwnedContinuousLearningCacheIoStage
                            .AfterBoundedExactReadPostValidationBeforeComparison)
                    {
                        order.Add("actual-same-handle-postvalidation");
                    }
                },
                OverrideExactReadPostValidationForTesting = actual =>
                {
                    order.Add("path-free-observed-override");
                    return changeIdentity
                        ? actual with
                        {
                            Identity = actual.Identity with
                            {
                                FileIdLow = actual.Identity.FileIdLow ^
                                            0x8000_0000_0000_0000UL,
                            },
                        }
                        : actual with { Length = actual.Length + 1L };
                },
            });
        var identity = capability.CreateWriteTemp(new byte[] { 1, 2, 3 }, 64);

        Assert.Throws<InvalidDataException>(() => capability.ReadBoundedExact(
            ServerOwnedContinuousLearningCacheLeaf.WriteTemp,
            identity,
            64));
        Assert.Equal(
            new[]
            {
                "actual-same-handle-postvalidation",
                "path-free-observed-override",
            },
            order);
        Assert.Equal(
            new byte[] { 1, 2, 3 },
            capability.ReadBounded(
                ServerOwnedContinuousLearningCacheLeaf.WriteTemp,
                64));
        access.EnsureRetained();
    }

    [ContinuousLearningCacheWindowsTheory]
    [InlineData(
        (int)ServerOwnedContinuousLearningCacheLeaf.WriteTemp,
        (int)ServerOwnedContinuousLearningCacheLeaf.CanonicalCache)]
    [InlineData(
        (int)ServerOwnedContinuousLearningCacheLeaf.CanonicalCache,
        (int)ServerOwnedContinuousLearningCacheLeaf.PublicationBackup)]
    [InlineData(
        (int)ServerOwnedContinuousLearningCacheLeaf.PublicationBackup,
        (int)ServerOwnedContinuousLearningCacheLeaf.CanonicalCache)]
    public void NoReplaceDestinationCollision_PreservesBothFilesAndTrust(
        int sourceValue,
        int destinationValue)
    {
        RequireWindows();
        var source = (ServerOwnedContinuousLearningCacheLeaf)sourceValue;
        var destination = (ServerOwnedContinuousLearningCacheLeaf)destinationValue;
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireCompilerCacheRootCapability();
        SeedAllMutableLeaves(capability);
        var identity = capability.QueryExactIdentity(source, 64);
        var before = ProtocolSnapshot(fixture.CacheRoot);

        Assert.Throws<IOException>(() => capability.RenameNoReplace(
            source,
            identity,
            destination,
            64));

        Assert.Equal(before, ProtocolSnapshot(fixture.CacheRoot));
        access.EnsureRetained();
    }

    [ContinuousLearningCacheWindowsFact]
    public void InvalidEnumBoundsAndMissingLeaf_DoNotPoisonRoot()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireCompilerCacheRootCapability();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            capability.OpenExistingRegularLeaf(
                (ServerOwnedContinuousLearningCacheLeaf)999,
                16));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            capability.OpenExistingRegularLeaf(
                ServerOwnedContinuousLearningCacheLeaf.CanonicalCache,
                0));
        Assert.Throws<ArgumentOutOfRangeException>(() => capability.Inventory(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => capability.Inventory(6));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            capability.CreateWriteTemp(new byte[] { 1, 2 }, maximumBytes: 1));
        Assert.Throws<FileNotFoundException>(() =>
            capability.OpenExistingRegularLeaf(
                ServerOwnedContinuousLearningCacheLeaf.CanonicalCache,
                16));
        Assert.Throws<ArgumentOutOfRangeException>(() => capability.ReadBoundedExact(
            ServerOwnedContinuousLearningCacheLeaf.OwnerLock,
            new ServerOwnedContinuousLearningCacheFileIdentity(1, 1, 0),
            16));
        Assert.Throws<ArgumentException>(() => capability.ReadBoundedExact(
            ServerOwnedContinuousLearningCacheLeaf.CanonicalCache,
            default,
            16));
        Assert.Equal(
            ServerOwnedContinuousLearningCacheLeaf.OwnerLock,
            Assert.Single(capability.Inventory(1)).Leaf);
        capability.FlushRootDirectory();
        access.EnsureRetained();
    }

    [ContinuousLearningCacheWindowsTheory]
    [InlineData("directory")]
    [InlineData("hard-link")]
    [InlineData("oversize")]
    [InlineData("reparse")]
    public void UnsafeFixedLeaf_IsRejectedWithoutFollowingOrPoisoningRoot(string shape)
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        var canonical = Path.Combine(fixture.CacheRoot, CanonicalCacheName);
        string? target = null;
        try
        {
            switch (shape)
            {
                case "directory":
                    Directory.CreateDirectory(canonical);
                    break;
                case "hard-link":
                    File.WriteAllBytes(canonical, [1]);
                    Assert.True(CreateHardLinkWindows(
                        Path.Combine(fixture.CacheRoot, "unconfigured-link.bin"),
                        canonical,
                        IntPtr.Zero));
                    break;
                case "oversize":
                    File.WriteAllBytes(canonical, [1, 2, 3, 4]);
                    break;
                case "reparse":
                    target = CreateSentinelRoot();
                    CreateJunctionAtPath(canonical, target);
                    break;
            }

            using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
            using var capability = access.AcquireCompilerCacheRootCapability();
            var rejection = Record.Exception(() =>
                capability.OpenExistingRegularLeaf(
                    ServerOwnedContinuousLearningCacheLeaf.CanonicalCache,
                    maximumBytes: 3));
            Assert.True(rejection is IOException or InvalidDataException);
            access.EnsureRetained();
            capability.FlushRootDirectory();
            if (target is not null)
                AssertSentinelUnchanged(target);
        }
        finally
        {
            if (target is not null)
            {
                if (Directory.Exists(canonical) || File.Exists(canonical))
                    RemoveDirectoryWindows(canonical);
                Directory.Delete(target, recursive: true);
            }
        }
    }

    [ContinuousLearningCacheWindowsFact]
    public void DeletePendingFixedLeaf_IsRejectedWithoutPoisoningRoot()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        var canonical = Path.Combine(fixture.CacheRoot, CanonicalCacheName);
        using var pendingDelete = new FileStream(
            canonical,
            new FileStreamOptions
            {
                Mode = FileMode.CreateNew,
                Access = FileAccess.ReadWrite,
                Share = FileShare.ReadWrite | FileShare.Delete,
                Options = FileOptions.DeleteOnClose,
            });
        pendingDelete.WriteByte(0x31);
        pendingDelete.Flush(flushToDisk: true);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireCompilerCacheRootCapability();

        Assert.ThrowsAny<IOException>(() =>
            capability.OpenExistingRegularLeaf(
                ServerOwnedContinuousLearningCacheLeaf.CanonicalCache,
                maximumBytes: 16));
        access.EnsureRetained();
        capability.FlushRootDirectory();
    }

    [ContinuousLearningCacheWindowsFact]
    public void CapabilityDispose_ClosesOwnerLockAndRemainsOneWay()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        var capability = access.AcquireCompilerCacheRootCapability();
        var ownerLock = GetOwnerLockHandle(capability);

        capability.Dispose();
        capability.Dispose();

        Assert.True(ownerLock.IsClosed);
        Assert.Throws<ContinuousLearningPathIdentityException>(() =>
            capability.Inventory(5));
        Assert.Throws<InvalidOperationException>(
            () => access.AcquireCompilerCacheRootCapability());
        using var externallyAvailable = OpenOwnerLock(fixture.CacheRoot);
        Assert.False(externallyAvailable.IsInvalid);
        access.EnsureRetained();
    }

    [ContinuousLearningCacheWindowsFact]
    public async Task ParentDispose_WaitsForInFlightOperationThenClosesOwnerLock()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var hookEntered = new ManualResetEventSlim(initialState: false);
        using var releaseHook = new ManualResetEventSlim(initialState: false);
        var capability = access.AcquireCompilerCacheRootCapability(
            new ServerOwnedContinuousLearningCacheRootCapabilityOptions
            {
                ObserveIoStageForTesting = stage =>
                {
                    if (stage != ServerOwnedContinuousLearningCacheIoStage
                            .BeforeRootDirectoryFlush)
                    {
                        return;
                    }
                    hookEntered.Set();
                    Assert.True(releaseHook.Wait(TimeSpan.FromSeconds(10)));
                },
            });
        var ownerLock = GetOwnerLockHandle(capability);

        var operation = Task.Run(capability.FlushRootDirectory);
        Assert.True(hookEntered.Wait(TimeSpan.FromSeconds(10)));
        var disposal = Task.Run(access.Dispose);
        await Task.Delay(TimeSpan.FromMilliseconds(250));
        Assert.False(disposal.IsCompleted);
        Assert.False(ownerLock.IsClosed);
        releaseHook.Set();
        await operation;
        await disposal;

        Assert.True(ownerLock.IsClosed);
        Assert.Throws<ContinuousLearningPathIdentityException>(() =>
            capability.Inventory(5));
        using var externallyAvailable = OpenOwnerLock(fixture.CacheRoot);
        Assert.False(externallyAvailable.IsInvalid);
        capability.Dispose();
    }

    [ContinuousLearningCacheWindowsFact]
    public void MetadataOnlyReparseMutationAttempts_AreDeniedByRetainedOwnerLock()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var target = new SentinelRoot();
        using var access = fixture.CreateStartup().AcquireRetainedFileAccess();
        using var capability = access.AcquireCompilerCacheRootCapability();
        var cacheRootHandle = GetCacheRootHandle(access);
        var ownerLockHandle = GetOwnerLockHandle(capability);

        Assert.False(TrySetJunctionTag(
            cacheRootHandle,
            target.Root,
            out var directError));
        Assert.Equal(145, directError);
        using (var external = OpenReparseDirectory(
                   fixture.CacheRoot,
                   desiredAccess: 0x00000100))
        {
            Assert.False(external.IsInvalid);
            Assert.False(TrySetJunctionTag(
                external,
                target.Root,
                out var externalError));
            Assert.Equal(145, externalError);
        }

        Assert.False(ownerLockHandle.IsClosed);
        Assert.Equal(
            ServerOwnedContinuousLearningCacheLeaf.OwnerLock,
            Assert.Single(capability.Inventory(1)).Leaf);
        access.EnsureRetained();
        target.AssertUnchanged();
    }

    [ContinuousLearningCacheWindowsFact]
    public void RetainedRootHandleLoss_RevokesAndClosesOwnerBeforeOutsideGateCallback()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        var startup = fixture.CreateStartup();
        using var access = startup.AcquireRetainedFileAccess();
        startup.BindRetainedFileAccess(access);
        using var capability = access.AcquireCompilerCacheRootCapability();
        var cacheRootHandle = GetCacheRootHandle(access);
        var ownerLockHandle = GetOwnerLockHandle(capability);
        var callbackField = typeof(ServerOwnedContinuousLearningFileAccess).GetField(
            "_onRetainedObjectTrustLost",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        var originalCallback = Assert.IsType<Action>(callbackField.GetValue(access));
        var callbackCount = 0;
        var callbackObservedClosedOwner = 0;
        var callbackObservedOutsideOwnerGate = 0;
        callbackField.SetValue(access, (Action)(() =>
        {
            Interlocked.Increment(ref callbackCount);
            if (ownerLockHandle.IsClosed)
                Volatile.Write(ref callbackObservedClosedOwner, 1);
            using var disposalCompleted = new ManualResetEventSlim(false);
            var disposer = new Thread(() =>
            {
                capability.Dispose();
                disposalCompleted.Set();
            })
            {
                IsBackground = true,
            };
            disposer.Start();
            if (disposalCompleted.Wait(TimeSpan.FromSeconds(3)))
                Volatile.Write(ref callbackObservedOutsideOwnerGate, 1);
            originalCallback();
        }));

        cacheRootHandle.Dispose();
        Assert.Throws<ContinuousLearningPathIdentityException>(() =>
            capability.Inventory(5));
        Assert.Equal(1, Volatile.Read(ref callbackCount));
        Assert.Equal(1, Volatile.Read(ref callbackObservedClosedOwner));
        Assert.Equal(1, Volatile.Read(ref callbackObservedOutsideOwnerGate));
        Assert.False(startup.ObserveReadiness().RetainedObjectIdentityBound);
    }

    [ContinuousLearningCacheWindowsFact]
    public void ExternalMetadataMutationAttemptDuringIo_IsDeniedWithoutFollowingSentinel()
    {
        RequireWindows();
        using var fixture = new FileAccessFixture(provisionAllRoots: true);
        using var target = new SentinelRoot();
        var startup = fixture.CreateStartup();
        using var access = startup.AcquireRetainedFileAccess();
        startup.BindRetainedFileAccess(access);
        var hookCount = 0;
        var mutationSucceeded = false;
        var mutationError = 0;
        using var capability = access.AcquireCompilerCacheRootCapability(
            new ServerOwnedContinuousLearningCacheRootCapabilityOptions
            {
                ObserveIoStageForTesting = stage =>
                {
                    if (stage == ServerOwnedContinuousLearningCacheIoStage
                            .BeforeRootDirectoryFlush &&
                        Interlocked.Increment(ref hookCount) == 1)
                    {
                        mutationSucceeded = TrySetJunctionTagByPathWithMetadataOnlyAccess(
                            fixture.CacheRoot,
                            target.Root,
                            out mutationError);
                    }
                },
            });
        var ownerLockHandle = GetOwnerLockHandle(capability);

        capability.FlushRootDirectory();
        Assert.Equal(1, Volatile.Read(ref hookCount));
        Assert.False(mutationSucceeded);
        Assert.Equal(145, mutationError);
        Assert.False(ownerLockHandle.IsClosed);
        Assert.True(startup.ObserveReadiness().RetainedObjectIdentityBound);
        access.EnsureRetained();
        target.AssertUnchanged();
    }

    private static bool IsPublicOrInternal(MethodBase method)
        => method.IsPublic || method.IsAssembly || method.IsFamilyOrAssembly;

    private static bool IsAllowedRename(
        ServerOwnedContinuousLearningCacheLeaf source,
        ServerOwnedContinuousLearningCacheLeaf destination)
        => source == ServerOwnedContinuousLearningCacheLeaf.CanonicalCache &&
           destination == ServerOwnedContinuousLearningCacheLeaf.PublicationBackup ||
           source == ServerOwnedContinuousLearningCacheLeaf.WriteTemp &&
           destination == ServerOwnedContinuousLearningCacheLeaf.CanonicalCache ||
           source == ServerOwnedContinuousLearningCacheLeaf.PublicationBackup &&
           destination == ServerOwnedContinuousLearningCacheLeaf.CanonicalCache;

    private static void SeedSingleSource(
        ServerOwnedContinuousLearningCacheRootCapability capability,
        ServerOwnedContinuousLearningCacheLeaf source)
    {
        var identity = capability.CreateWriteTemp(
            new[] { checked((byte)((int)source + 1)) },
            64);
        if (source == ServerOwnedContinuousLearningCacheLeaf.WriteTemp)
            return;
        capability.RenameNoReplace(
            ServerOwnedContinuousLearningCacheLeaf.WriteTemp,
            identity,
            ServerOwnedContinuousLearningCacheLeaf.CanonicalCache,
            64);
        if (source == ServerOwnedContinuousLearningCacheLeaf.CanonicalCache)
            return;
        capability.RenameNoReplace(
            ServerOwnedContinuousLearningCacheLeaf.CanonicalCache,
            identity,
            ServerOwnedContinuousLearningCacheLeaf.PublicationBackup,
            64);
    }

    private static void SeedAllMutableLeaves(
        ServerOwnedContinuousLearningCacheRootCapability capability)
    {
        var backup = capability.CreateWriteTemp(new byte[] { 0xB0 }, 64);
        capability.RenameNoReplace(
            ServerOwnedContinuousLearningCacheLeaf.WriteTemp,
            backup,
            ServerOwnedContinuousLearningCacheLeaf.CanonicalCache,
            64);
        capability.RenameNoReplace(
            ServerOwnedContinuousLearningCacheLeaf.CanonicalCache,
            backup,
            ServerOwnedContinuousLearningCacheLeaf.PublicationBackup,
            64);
        var canonical = capability.CreateWriteTemp(new byte[] { 0xC0 }, 64);
        capability.RenameNoReplace(
            ServerOwnedContinuousLearningCacheLeaf.WriteTemp,
            canonical,
            ServerOwnedContinuousLearningCacheLeaf.CanonicalCache,
            64);
        _ = capability.CreateWriteTemp(new byte[] { 0xD0 }, 64);
    }

    private static ServerOwnedContinuousLearningCacheFileIdentity WrongIdentity(
        ServerOwnedContinuousLearningCacheFileIdentity identity)
        => identity with
        {
            FileIdLow = identity.FileIdLow ^ 0x8000_0000_0000_0000UL,
        };

    private static string[] ProtocolSnapshot(string root)
        => new[]
            {
                CanonicalCacheName,
                WriteTempName,
                PublicationBackupName,
                AuditLogName,
            }
            .Select(name =>
            {
                var path = Path.Combine(root, name);
                return File.Exists(path)
                    ? $"{name}|{Convert.ToHexString(File.ReadAllBytes(path))}"
                    : $"{name}|missing";
            })
            .ToArray();

    private static void RequireWindows()
    {
        if (OperatingSystem.IsWindows())
            return;

        // Это legacy Windows-oracle полоса: share-mode/delete-pending/handle
        // семантика остаётся обязательной на Windows, а в WSL должна быть
        // честным xUnit skip рядом с отдельной POSIX-полосой, не failure.
        throw Xunit.Sdk.SkipException.ForSkip(
            "WINDOWS_ONLY_CAPABILITY_ORACLE: R1.3A CacheRoot capability tests require Windows execution.");
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

        // Тест делает owner-lock недоверенным через group write, не через смену владельца.
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

    private static SafeFileHandle GetOwnerLockHandle(
        ServerOwnedContinuousLearningCacheRootCapability capability)
        => Assert.IsType<SafeFileHandle>(
            typeof(ServerOwnedContinuousLearningCacheRootCapability)
                .GetField(
                    "_ownerLockHandle",
                    BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(capability));

    private static SafeFileHandle OpenOwnerLock(string cacheRoot)
        => CreateFileWindows(
            Path.Combine(cacheRoot, OwnerLockName),
            0x80000000,
            (uint)(FileShare.Read | FileShare.Write | FileShare.Delete),
            IntPtr.Zero,
            3,
            0x00200000,
            IntPtr.Zero);

    private static SafeFileHandle GetCacheRootHandle(
        ServerOwnedContinuousLearningFileAccess access)
    {
        var entries = Assert.IsAssignableFrom<Array>(
            typeof(ServerOwnedContinuousLearningFileAccess)
                .GetField("_entries", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(access));
        foreach (var entry in entries)
        {
            var role = Assert.IsType<ContinuousLearningRetainedObjectRole>(
                entry!.GetType().GetProperty("Role")!.GetValue(entry));
            if (role == ContinuousLearningRetainedObjectRole.CacheRoot)
            {
                return Assert.IsType<SafeFileHandle>(
                    entry.GetType().GetProperty("Handle")!.GetValue(entry));
            }
        }
        throw new Xunit.Sdk.XunitException("CacheRoot handle was not retained.");
    }

    private static void AssertExactConfiguredPathsNotReachable(
        IEnumerable<object?> roots,
        IReadOnlyCollection<string> exactPaths)
    {
        var pending = new Stack<object?>(roots);
        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
        var assembly = typeof(ContinuousLearningModelServerStartup).Assembly;
        while (pending.TryPop(out var candidate))
        {
            if (candidate is null)
                continue;
            if (candidate is string text)
            {
                Assert.DoesNotContain(exactPaths, path => string.Equals(
                    path,
                    text,
                    StringComparison.OrdinalIgnoreCase));
                continue;
            }
            var type = candidate.GetType();
            if (!type.IsValueType && !visited.Add(candidate))
                continue;
            Assert.True(visited.Count <= 10_000);
            if (candidate is Array array)
            {
                foreach (var element in array)
                    pending.Push(element);
                continue;
            }
            if (candidate is Delegate callback)
            {
                foreach (var invocation in callback.GetInvocationList())
                    pending.Push(invocation.Target);
                continue;
            }
            if (type.Assembly != assembly)
                continue;
            foreach (var field in type.GetFields(
                         BindingFlags.Instance |
                         BindingFlags.Public |
                         BindingFlags.NonPublic))
            {
                pending.Push(field.GetValue(candidate));
            }
        }
    }

    private static string CreateSentinelRoot()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            $"continuous-learning-cache-sentinel-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        File.WriteAllBytes(Path.Combine(root, CanonicalCacheName), [0xA1, 0xB2]);
        return root;
    }

    private static void AssertSentinelUnchanged(string root)
        => Assert.Equal(
            new byte[] { 0xA1, 0xB2 },
            File.ReadAllBytes(Path.Combine(root, CanonicalCacheName)));

    private static void CreateJunctionAtPath(string linkPath, string targetPath)
    {
        Directory.CreateDirectory(linkPath);
        using var handle = OpenReparseDirectory(
            linkPath,
            desiredAccess: 0x00000100);
        Assert.False(handle.IsInvalid);
        SetJunctionTag(handle, targetPath);
    }

    private static bool TrySetJunctionTagByPathWithMetadataOnlyAccess(
        string linkPath,
        string targetPath,
        out int error)
    {
        using var handle = OpenReparseDirectory(
            linkPath,
            desiredAccess: 0x00000100);
        Assert.False(
            handle.IsInvalid,
            $"External FILE_WRITE_ATTRIBUTES open failed with {Marshal.GetLastPInvokeError()}.");
        return TrySetJunctionTag(handle, targetPath, out error);
    }

    private static SafeFileHandle OpenReparseDirectory(
        string path,
        uint desiredAccess)
        => CreateFileWindows(
            path,
            desiredAccess,
            (uint)(FileShare.Read | FileShare.Write | FileShare.Delete),
            IntPtr.Zero,
            3,
            0x02000000 | 0x00200000,
            IntPtr.Zero);

    private static void SetJunctionTag(
        SafeFileHandle handle,
        string targetPath)
    {
        Assert.True(
            TrySetJunctionTag(handle, targetPath, out var error),
            $"FSCTL_SET_REPARSE_POINT failed with {error}.");
    }

    private static bool TrySetJunctionTag(
        SafeFileHandle handle,
        string targetPath,
        out int error)
    {
        var printName = Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(targetPath));
        var substituteName = @"\??\" + printName;
        var substituteBytes = Encoding.Unicode.GetBytes(substituteName);
        var printBytes = Encoding.Unicode.GetBytes(printName);
        var pathBytes = substituteBytes.Length + sizeof(ushort) +
                        printBytes.Length + sizeof(ushort);
        var reparseDataLength = checked((ushort)(8 + pathBytes));
        var buffer = new byte[8 + reparseDataLength];
        using (var memory = new MemoryStream(buffer, writable: true))
        using (var writer = new BinaryWriter(
                   memory,
                   Encoding.Unicode,
                   leaveOpen: true))
        {
            writer.Write(0xA0000003U);
            writer.Write(reparseDataLength);
            writer.Write((ushort)0);
            writer.Write((ushort)0);
            writer.Write(checked((ushort)substituteBytes.Length));
            writer.Write(checked((ushort)(substituteBytes.Length + sizeof(ushort))));
            writer.Write(checked((ushort)printBytes.Length));
            writer.Write(substituteBytes);
            writer.Write((ushort)0);
            writer.Write(printBytes);
            writer.Write((ushort)0);
        }
        var succeeded = DeviceIoControlWindows(
            handle,
            0x000900A4,
            buffer,
            buffer.Length,
            IntPtr.Zero,
            0,
            out _,
            IntPtr.Zero);
        error = succeeded ? 0 : Marshal.GetLastPInvokeError();
        return succeeded;
    }

    private static void DeleteJunctionTag(SafeFileHandle handle)
    {
        var buffer = new byte[8];
        using (var memory = new MemoryStream(buffer, writable: true))
        using (var writer = new BinaryWriter(
                   memory,
                   Encoding.Unicode,
                   leaveOpen: true))
        {
            writer.Write(0xA0000003U);
            writer.Write((ushort)0);
            writer.Write((ushort)0);
        }
        Assert.True(
            DeviceIoControlWindows(
                handle,
                0x000900AC,
                buffer,
                buffer.Length,
                IntPtr.Zero,
                0,
                out _,
                IntPtr.Zero),
            $"FSCTL_DELETE_REPARSE_POINT failed with {Marshal.GetLastPInvokeError()}.");
    }

    private sealed class SentinelRoot : IDisposable
    {
        internal SentinelRoot()
            => Root = CreateSentinelRoot();

        internal string Root { get; }

        internal void AssertUnchanged()
            => AssertSentinelUnchanged(Root);

        public void Dispose()
        {
            if (Directory.Exists(Root))
                Directory.Delete(Root, recursive: true);
        }
    }

    [DllImport("kernel32.dll", EntryPoint = "CreateFileW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern SafeFileHandle CreateFileWindows(
        string fileName,
        uint desiredAccess,
        uint shareMode,
        IntPtr securityAttributes,
        uint creationDisposition,
        uint flagsAndAttributes,
        IntPtr templateFile);

    [DllImport("kernel32.dll", EntryPoint = "DeviceIoControl", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeviceIoControlWindows(
        SafeFileHandle device,
        uint ioControlCode,
        byte[] inputBuffer,
        int inputBufferSize,
        IntPtr outputBuffer,
        int outputBufferSize,
        out int bytesReturned,
        IntPtr overlapped);

    [DllImport("kernel32.dll", EntryPoint = "CreateHardLinkW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CreateHardLinkWindows(
        string fileName,
        string existingFileName,
        IntPtr securityAttributes);

    [DllImport("kernel32.dll", EntryPoint = "RemoveDirectoryW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RemoveDirectoryWindows(string pathName);

    [DllImport("libc", EntryPoint = "link", SetLastError = true)]
    private static extern int LinkPosix(
        string existingPath,
        string linkPath);
}

file sealed class ContinuousLearningCacheCapabilityLinuxX64FactAttribute : Xunit.FactAttribute
{
    public ContinuousLearningCacheCapabilityLinuxX64FactAttribute()
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

file sealed class ContinuousLearningCacheWindowsFactAttribute : Xunit.FactAttribute
{
    public ContinuousLearningCacheWindowsFactAttribute()
    {
        if (!OperatingSystem.IsWindows())
        {
            // Discovery-time skip нужен именно для VSTest/xUnit v2 runner:
            // dynamic SkipException приходит как failure, а не NotExecuted.
            Skip = "WINDOWS_ONLY_CAPABILITY_ORACLE: R1.3A CacheRoot capability tests require Windows execution.";
        }
    }
}

file sealed class ContinuousLearningCacheWindowsTheoryAttribute : Xunit.TheoryAttribute
{
    public ContinuousLearningCacheWindowsTheoryAttribute()
    {
        if (!OperatingSystem.IsWindows())
        {
            // Теории старой полосы доказывают Windows share/delete semantics;
            // POSIX-инварианты покрыты отдельными Linux x64 facts выше.
            Skip = "WINDOWS_ONLY_CAPABILITY_ORACLE: R1.3A CacheRoot capability tests require Windows execution.";
        }
    }
}
