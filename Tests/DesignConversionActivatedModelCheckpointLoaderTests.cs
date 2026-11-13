// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using DesignConversionDomain;
using ModelServer.Contracts;
using ModelServer.DesignConversion;
using DesignDomain = DesignConversionDomain.DesignConversionDomain;

public sealed class DesignConversionActivatedModelCheckpointLoaderTests
{
    private const string ServerBaseUrl = "https://models.example/repository";
    private const string PrivateMarker = "PRIVATE_A3A_LOADER_MARKER";
    private const string HostileDependencyFailure =
        "InvalidOperationException PRIVATE_A3A_LOADER_MARKER C:\\private\\checkpoint.json " +
        "https://user:secret@models.example/repository?api_key=secret credential=secret payload=secret";
    private const string ActiveModelCheckpointRealWeightsRequiredErrorCode =
        DesignConversionActivatedModelRuntimeEvaluator.ActiveModelCheckpointRealWeightsRequiredErrorCode;
    private const long OversizedCheckpointByteCount = 17L * 1024 * 1024;
    private static readonly DateTimeOffset RegisteredAtUtc = DateTimeOffset.Parse("2026-07-27T09:15:00Z");
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    [Fact]
    public async Task LocalLoader_LoadsExistingCheckpointPathAndDoesNotCallRepository()
    {
        var fixture = await CreateFixtureAsync("local-loader");
        try
        {
            var repositoryCalls = new List<string>();
            _ = new RecordingRepository(repositoryCalls, fixture.CheckpointBytes);
            var loader = new DesignConversionLocalActivatedModelCheckpointLoader();

            var result = await loader.LoadAsync(fixture.ActiveVersion);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal(fixture.CheckpointSha256, result.Value!.CheckpointSha256);
            Assert.Equal(fixture.ActiveVersion.Version, result.Value.Checkpoint.Version);
            Assert.True(DesignConversionMachine.CheckpointHasRealLearnedWeights(result.Value.Checkpoint));
            Assert.Empty(repositoryCalls);
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task LocalLoader_RejectsActiveVersionWithoutFreshLoadVerifiedRealWeights(
        bool freshLoadVerified,
        bool containsRealWeights)
    {
        var fixture = await CreateFixtureAsync($"local-proof-{freshLoadVerified}-{containsRealWeights}");
        try
        {
            var loader = new DesignConversionLocalActivatedModelCheckpointLoader();
            var activeVersion = fixture.ActiveVersion with
            {
                CheckpointFreshLoadVerified = freshLoadVerified,
                CheckpointContainsRealTrainedWeights = containsRealWeights
            };

            var result = await loader.LoadAsync(activeVersion);

            Assert.False(result.IsSuccess);
            Assert.Equal(ActiveModelCheckpointRealWeightsRequiredErrorCode, result.ErrorCode);
            AssertNoPrivateFailureLeak(result, fixture);
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    [Fact]
    public async Task RepositoryLoader_LoadsBothRepositoryUnitsAndIgnoresCorruptLocalCheckpointPath()
    {
        var fixture = await CreateFixtureAsync("repo-success");
        try
        {
            var calls = new List<string>();
            var repository = new RecordingRepository(calls, fixture.CheckpointBytes);
            var locator = new RecordingLocator(calls, BuildManifestMap(fixture));
            var cacheRoot = Path.Combine(fixture.TempRoot, "server-owned-cache");
            var loader = new DesignConversionRepositoryActivatedModelCheckpointLoader(repository, locator, cacheRoot);
            var activeVersion = fixture.ActiveVersion with
            {
                CheckpointPath = fixture.CorruptLocalCheckpointPath,
                CheckpointPathRef = "checkpoint-path-ref:corrupt-local-path",
                LocalOnly = false
            };

            var result = await loader.LoadAsync(activeVersion);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal(
                [
                    "locate:critic",
                    "load:critic",
                    "locate:improver",
                    "load:improver"
                ],
                calls);
            Assert.Equal(fixture.CheckpointSha256, result.Value!.CheckpointSha256);
            Assert.Equal(activeVersion.Version, result.Value.Checkpoint.Version);
            Assert.Equal(activeVersion.MachineId, result.Value.Checkpoint.MachineId);
            Assert.Equal(activeVersion.DomainTag, result.Value.Checkpoint.DomainTag);
            Assert.True(DesignConversionMachine.CheckpointHasRealLearnedWeights(result.Value.Checkpoint));
            Assert.All(repository.LocalFallbackPathByUnit.Values, path =>
                Assert.Equal(fixture.CorruptLocalCheckpointPath, path));
            Assert.Equal(
                [
                    new RepositoryLoadInput(DesignConversionRuntimeScaffoldCriticUnit.UnitId, ServerBaseUrl),
                    new RepositoryLoadInput(DesignConversionRuntimeScaffoldImproverUnit.UnitId, ServerBaseUrl)
                ],
                repository.LoadInputs);

            var cachePath = ExpectedCachePath(cacheRoot, fixture.CheckpointSha256);
            Assert.True(File.Exists(cachePath), cachePath);
            Assert.Equal(fixture.CheckpointSha256, Sha256Hex(await File.ReadAllBytesAsync(cachePath)));
            var cachedLoad = await DesignConversionMachine.LoadCheckpointAsync(cachePath);
            Assert.True(cachedLoad.IsSuccess, cachedLoad.ErrorMessage);
            Assert.Equal(fixture.CheckpointSha256, cachedLoad.Value!.CheckpointSha256);
            Assert.Equal(activeVersion.Version, cachedLoad.Value.Checkpoint.Version);
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task RepositoryLoader_RejectsActiveVersionWithoutFreshLoadVerifiedRealWeightsBeforeRepositoryOrCache(
        bool freshLoadVerified,
        bool containsRealWeights)
    {
        var fixture = await CreateFixtureAsync($"repo-proof-{freshLoadVerified}-{containsRealWeights}");
        try
        {
            var calls = new List<string>();
            var repository = new RecordingRepository(calls, fixture.CheckpointBytes);
            var cacheRoot = Path.Combine(fixture.TempRoot, "proof-cache");
            var loader = new DesignConversionRepositoryActivatedModelCheckpointLoader(
                repository,
                new RecordingLocator(calls, BuildManifestMap(fixture)),
                cacheRoot);
            var activeVersion = fixture.ActiveVersion with
            {
                CheckpointFreshLoadVerified = freshLoadVerified,
                CheckpointContainsRealTrainedWeights = containsRealWeights,
                LocalOnly = false
            };

            var result = await loader.LoadAsync(activeVersion);

            Assert.False(result.IsSuccess);
            Assert.Equal(ActiveModelCheckpointRealWeightsRequiredErrorCode, result.ErrorCode);
            Assert.Empty(calls);
            Assert.Empty(repository.LoadInputs);
            Assert.False(Directory.Exists(cacheRoot));
            Assert.Empty(Directory.EnumerateFiles(fixture.TempRoot, "*.tmp", SearchOption.AllDirectories));
            AssertNoPrivateFailureLeak(result, fixture);
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    [Theory]
    [InlineData("hash", "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_MANIFEST_IDENTITY_MISMATCH")]
    [InlineData("version", "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_MANIFEST_IDENTITY_MISMATCH")]
    [InlineData("domain", "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_MANIFEST_IDENTITY_MISMATCH")]
    [InlineData("unit", "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_MANIFEST_IDENTITY_MISMATCH")]
    [InlineData("role", "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_MANIFEST_IDENTITY_MISMATCH")]
    [InlineData("raw-url", "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_SERVER_BASE_URL_INVALID")]
    [InlineData("encoded-url", "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_SERVER_BASE_URL_INVALID")]
    [InlineData("endpoint", "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_UNIT_ENDPOINT_INVALID")]
    [InlineData("safe-wrong-endpoint", "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_UNIT_ENDPOINT_INVALID")]
    [InlineData("endpoint-query", "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_UNIT_ENDPOINT_INVALID")]
    [InlineData("endpoint-fragment", "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_UNIT_ENDPOINT_INVALID")]
    [InlineData("endpoint-absolute", "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_UNIT_ENDPOINT_INVALID")]
    [InlineData("endpoint-authority", "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_UNIT_ENDPOINT_INVALID")]
    [InlineData("endpoint-extra-segment", "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_UNIT_ENDPOINT_INVALID")]
    [InlineData("endpoint-raw-backslash", "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_UNIT_ENDPOINT_INVALID")]
    [InlineData("endpoint-malformed-percent", "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_UNIT_ENDPOINT_INVALID")]
    [InlineData("endpoint-noncanonical-percent-spelling", "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_UNIT_ENDPOINT_INVALID")]
    public async Task RepositoryLoader_FailsClosedForLocatedManifestIdentityAndEndpointMismatches(
        string mutation,
        string expectedCode)
    {
        var fixture = await CreateFixtureAsync($"manifest-{mutation}");
        try
        {
            var calls = new List<string>();
            var manifests = BuildManifestMap(fixture);
            manifests[DesignConversionRuntimeScaffoldCriticUnit.UnitId] =
                MutateManifest(manifests[DesignConversionRuntimeScaffoldCriticUnit.UnitId], mutation);
            var loader = new DesignConversionRepositoryActivatedModelCheckpointLoader(
                new RecordingRepository(calls, fixture.CheckpointBytes),
                new RecordingLocator(calls, manifests),
                Path.Combine(fixture.TempRoot, "cache"));

            var result = await loader.LoadAsync(fixture.ActiveVersion);

            Assert.False(result.IsSuccess);
            Assert.Equal(expectedCode, result.ErrorCode);
            Assert.Equal(["locate:critic"], calls);
            AssertNoPrivateFailureLeak(result, fixture);
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    [Theory]
    [InlineData(DesignConversionRuntimeScaffoldCriticUnit.UnitId, "/models/design_conversion_runtime_scaffold_critic")]
    [InlineData("a/b", "/models/a%2Fb")]
    [InlineData("a\\b", "/models/a%5Cb")]
    [InlineData("a%2Fb", "/models/a%252Fb")]
    public void RepositoryLoader_EndpointValidatorAcceptsCanonicalEscapedUnitIdentities(
        string expectedUnitId,
        string endpoint)
    {
        var result = ValidateUnitEndpointForTest(endpoint, expectedUnitId);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(endpoint, $"/models/{Uri.EscapeDataString(expectedUnitId)}");
    }

    [Fact]
    public void RepositoryLoader_EndpointValidatorKeepsEscapedIdentityFormsDistinct()
    {
        var unitIds = new[] { "a/b", "a\\b", "a%2Fb" };
        var endpoints = unitIds
            .Select(unitId => $"/models/{Uri.EscapeDataString(unitId)}")
            .ToArray();

        Assert.Equal(endpoints.Length, endpoints.Distinct(StringComparer.Ordinal).Count());
        Assert.Contains("/models/a%2Fb", endpoints, StringComparer.Ordinal);
        Assert.Contains("/models/a%5Cb", endpoints, StringComparer.Ordinal);
        Assert.Contains("/models/a%252Fb", endpoints, StringComparer.Ordinal);
    }

    [Theory]
    [InlineData("query", "a", "/models/a?token=secret")]
    [InlineData("fragment", "a", "/models/a#secret")]
    [InlineData("absolute-uri", "a", "https://models.example/models/a")]
    [InlineData("authority-relative", "a", "//models.example/models/a")]
    [InlineData("wrong-prefix", "a", "/model/a")]
    [InlineData("empty-segment", "", "/models/")]
    [InlineData("raw-extra-segment", "a/b", "/models/a/b")]
    [InlineData("raw-traversal", "../a", "/models/../a")]
    [InlineData("raw-backslash", "a\\b", "/models/a\\b")]
    [InlineData("encoded-traversal", "a/../b", "/models/a%2F..%2Fb")]
    [InlineData("malformed-percent", "a/b", "/models/a%2")]
    [InlineData("lowercase-percent", "a/b", "/models/a%2fb")]
    [InlineData("noncanonical-percent-spelling", "ab", "/models/a%62")]
    [InlineData("double-identity-mismatch", "a%2Fb", "/models/a%2Fb")]
    [InlineData("leading-space", "a", " /models/a")]
    [InlineData("trailing-space", "a", "/models/a ")]
    public void RepositoryLoader_EndpointValidatorRejectsUnsafeOrNonCanonicalEndpoints(
        string scenario,
        string expectedUnitId,
        string endpoint)
    {
        var result = ValidateUnitEndpointForTest(endpoint, expectedUnitId);

        Assert.False(result.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_UNIT_ENDPOINT_INVALID", result.ErrorCode);
        Assert.DoesNotContain(scenario, result.ErrorMessage ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RepositoryLoader_FailsClosedWhenRepositoryBytesDifferFromManifestOrPeerUnit()
    {
        var fixture = await CreateFixtureAsync("byte-mismatch");
        try
        {
            var calls = new List<string>();
            var repository = new RecordingRepository(calls, fixture.CheckpointBytes)
            {
                BytesByUnit =
                {
                    [DesignConversionRuntimeScaffoldImproverUnit.UnitId] =
                        Utf8NoBom.GetBytes($"{PrivateMarker}:different-improver-checkpoint-bytes")
                }
            };
            var loader = new DesignConversionRepositoryActivatedModelCheckpointLoader(
                repository,
                new RecordingLocator(calls, BuildManifestMap(fixture)),
                Path.Combine(fixture.TempRoot, "cache"));

            var result = await loader.LoadAsync(fixture.ActiveVersion);

            Assert.False(result.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_LOAD_HASH_MISMATCH", result.ErrorCode);
            Assert.Equal(
                [
                    "locate:critic",
                    "load:critic",
                    "locate:improver",
                    "load:improver"
                ],
                calls);
            AssertNoPrivateFailureLeak(result, fixture);
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    [Fact]
    public async Task RepositoryLoader_FailsClosedWhenOnlyOneUnitReturnsAStream()
    {
        var fixture = await CreateFixtureAsync("null-stream");
        try
        {
            var calls = new List<string>();
            var repository = new RecordingRepository(calls, fixture.CheckpointBytes)
            {
                NullStreamUnitId = DesignConversionRuntimeScaffoldImproverUnit.UnitId
            };
            var loader = new DesignConversionRepositoryActivatedModelCheckpointLoader(
                repository,
                new RecordingLocator(calls, BuildManifestMap(fixture)),
                Path.Combine(fixture.TempRoot, "cache"));

            var result = await loader.LoadAsync(fixture.ActiveVersion);

            Assert.False(result.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_LOAD_STREAM_REQUIRED", result.ErrorCode);
            Assert.Equal(
                [
                    "locate:critic",
                    "load:critic",
                    "locate:improver",
                    "load:improver"
                ],
                calls);
            AssertNoPrivateFailureLeak(result, fixture);
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    [Fact]
    public async Task RepositoryLoader_CancellationFailsClosedWithoutCacheState()
    {
        var fixture = await CreateFixtureAsync("cancelled");
        try
        {
            var calls = new List<string>();
            var cacheRoot = Path.Combine(fixture.TempRoot, "cache");
            var loader = new DesignConversionRepositoryActivatedModelCheckpointLoader(
                new RecordingRepository(calls, fixture.CheckpointBytes),
                new RecordingLocator(calls, BuildManifestMap(fixture)),
                cacheRoot);
            using var cancellation = new CancellationTokenSource();
            await cancellation.CancelAsync();

            var result = await loader.LoadAsync(fixture.ActiveVersion, cancellation.Token);

            Assert.False(result.IsSuccess);
            Assert.Equal("CANCELLED", result.ErrorCode);
            Assert.Empty(calls);
            Assert.False(Directory.Exists(cacheRoot));
            AssertNoPrivateFailureLeak(result, fixture);
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    [Fact]
    public async Task RepositoryLoader_DoesNotOverwriteMismatchingVerifiedCachePath()
    {
        var fixture = await CreateFixtureAsync("cache-mismatch");
        try
        {
            var calls = new List<string>();
            var cacheRoot = Path.Combine(fixture.TempRoot, "cache");
            var cachePath = ExpectedCachePath(cacheRoot, fixture.CheckpointSha256);
            Directory.CreateDirectory(Path.GetDirectoryName(cachePath)!);
            var mismatchingCacheBytes = Utf8NoBom.GetBytes($"{PrivateMarker}:preexisting-cache-mismatch");
            await File.WriteAllBytesAsync(cachePath, mismatchingCacheBytes);
            var loader = new DesignConversionRepositoryActivatedModelCheckpointLoader(
                new RecordingRepository(calls, fixture.CheckpointBytes),
                new RecordingLocator(calls, BuildManifestMap(fixture)),
                cacheRoot);

            var result = await loader.LoadAsync(fixture.ActiveVersion);

            Assert.False(result.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_CACHE_MISMATCH", result.ErrorCode);
            Assert.Equal(
                [
                    "locate:critic",
                    "load:critic",
                    "locate:improver",
                    "load:improver"
                ],
                calls);
            Assert.Equal(mismatchingCacheBytes, await File.ReadAllBytesAsync(cachePath));
            AssertNoPrivateFailureLeak(result, fixture);
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    [Fact]
    public async Task RepositoryLoader_SanitizesRepositoryFailuresAndUnsafeRemoteMetadata()
    {
        var fixture = await CreateFixtureAsync("sanitized-failures");
        try
        {
            var repositoryFailureCalls = new List<string>();
            var repositoryFailure = new DesignConversionRepositoryActivatedModelCheckpointLoader(
                new RecordingRepository(repositoryFailureCalls, fixture.CheckpointBytes)
                {
                    FailLoadUnitId = DesignConversionRuntimeScaffoldCriticUnit.UnitId
                },
                new RecordingLocator(repositoryFailureCalls, BuildManifestMap(fixture)),
                Path.Combine(fixture.TempRoot, "repo-failure-cache"));
            var repositoryFailureResult = await repositoryFailure.LoadAsync(fixture.ActiveVersion);

            var unsafeManifestCalls = new List<string>();
            var manifests = BuildManifestMap(fixture);
            manifests[DesignConversionRuntimeScaffoldCriticUnit.UnitId] =
                manifests[DesignConversionRuntimeScaffoldCriticUnit.UnitId] with
                {
                    ServerBaseUrl = $"https://user:secret@models.example/repository?api_key={PrivateMarker}",
                    UnitEndpoint = $"/models/{PrivateMarker}?token=secret"
                };
            var unsafeManifestFailure = new DesignConversionRepositoryActivatedModelCheckpointLoader(
                new RecordingRepository(unsafeManifestCalls, fixture.CheckpointBytes),
                new RecordingLocator(unsafeManifestCalls, manifests),
                Path.Combine(fixture.TempRoot, "unsafe-manifest-cache"));
            var unsafeManifestResult = await unsafeManifestFailure.LoadAsync(fixture.ActiveVersion);

            Assert.False(repositoryFailureResult.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_LOAD_FAILED", repositoryFailureResult.ErrorCode);
            Assert.False(unsafeManifestResult.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_SERVER_BASE_URL_INVALID", unsafeManifestResult.ErrorCode);
            AssertNoPrivateFailureLeak(repositoryFailureResult, fixture);
            AssertNoPrivateFailureLeak(unsafeManifestResult, fixture);
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    [Theory]
    [InlineData("locator", "sync-throw", "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_LOCATE_FAILED")]
    [InlineData("locator", "async-throw", "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_LOCATE_FAILED")]
    [InlineData("locator", "null-task", "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_LOCATE_FAILED")]
    [InlineData("repository", "sync-throw", "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_LOAD_FAILED")]
    [InlineData("repository", "async-throw", "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_LOAD_FAILED")]
    [InlineData("repository", "null-task", "DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_LOAD_FAILED")]
    public async Task RepositoryLoader_ExternalDependencyBoundaryFailsClosedWithoutCache(
        string dependency,
        string failureMode,
        string expectedCode)
    {
        var fixture = await CreateFixtureAsync($"boundary-{dependency}-{failureMode}");
        try
        {
            var calls = new List<string>();
            var cacheRoot = Path.Combine(fixture.TempRoot, "boundary-cache");
            var repository = new RecordingRepository(calls, fixture.CheckpointBytes)
            {
                BoundaryFailureMode = dependency == "repository" ? failureMode : null
            };
            var locator = new RecordingLocator(calls, BuildManifestMap(fixture))
            {
                BoundaryFailureMode = dependency == "locator" ? failureMode : null
            };
            var loader = new DesignConversionRepositoryActivatedModelCheckpointLoader(
                repository,
                locator,
                cacheRoot);

            var result = await loader.LoadAsync(fixture.ActiveVersion);

            Assert.False(result.IsSuccess);
            Assert.Equal(expectedCode, result.ErrorCode);
            Assert.Equal(
                dependency == "locator"
                    ? ["locate:critic"]
                    : ["locate:critic", "load:critic"],
                calls);
            Assert.False(Directory.Exists(cacheRoot));
            Assert.Empty(Directory.EnumerateFiles(fixture.TempRoot, "*.tmp", SearchOption.AllDirectories));
            AssertNoPrivateFailureLeak(result, fixture);
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    [Fact]
    public async Task RepositoryLoader_ExternalCallerCancellationReturnsCancelledWithoutCache()
    {
        var fixture = await CreateFixtureAsync("boundary-cancelled");
        try
        {
            var calls = new List<string>();
            var cacheRoot = Path.Combine(fixture.TempRoot, "cancelled-boundary-cache");
            using var cancellation = new CancellationTokenSource();
            var repository = new RecordingRepository(calls, fixture.CheckpointBytes)
            {
                CancelAndThrowSource = cancellation
            };
            var loader = new DesignConversionRepositoryActivatedModelCheckpointLoader(
                repository,
                new RecordingLocator(calls, BuildManifestMap(fixture)),
                cacheRoot);

            var result = await loader.LoadAsync(fixture.ActiveVersion, cancellation.Token);

            Assert.False(result.IsSuccess);
            Assert.Equal("CANCELLED", result.ErrorCode);
            Assert.Equal(["locate:critic", "load:critic"], calls);
            Assert.False(Directory.Exists(cacheRoot));
            AssertNoPrivateFailureLeak(result, fixture);
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    [Fact]
    public async Task RepositoryLoader_RejectsSeekableOversizedStreamBeforeReadAndDisposesIt()
    {
        var fixture = await CreateFixtureAsync("seekable-oversized");
        try
        {
            var calls = new List<string>();
            var cacheRoot = Path.Combine(fixture.TempRoot, "seekable-oversized-cache");
            var oversized = new OversizedSeekableStream(OversizedCheckpointByteCount);
            var repository = new RecordingRepository(calls, fixture.CheckpointBytes);
            repository.StreamFactoriesByUnit[DesignConversionRuntimeScaffoldCriticUnit.UnitId] = () => oversized;
            var loader = new DesignConversionRepositoryActivatedModelCheckpointLoader(
                repository,
                new RecordingLocator(calls, BuildManifestMap(fixture)),
                cacheRoot);

            var result = await loader.LoadAsync(fixture.ActiveVersion);

            Assert.False(result.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_CHECKPOINT_TOO_LARGE", result.ErrorCode);
            Assert.Equal(["locate:critic", "load:critic"], calls);
            Assert.False(oversized.ReadAttempted);
            Assert.True(oversized.Disposed);
            Assert.False(Directory.Exists(cacheRoot));
            AssertNoPrivateFailureLeak(result, fixture);
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    [Fact]
    public async Task RepositoryLoader_RejectsNonSeekableOversizedStreamAtBoundAndDisposesIt()
    {
        var fixture = await CreateFixtureAsync("nonseekable-oversized");
        try
        {
            var calls = new List<string>();
            var cacheRoot = Path.Combine(fixture.TempRoot, "nonseekable-oversized-cache");
            var oversized = new GeneratedNonSeekableStream(OversizedCheckpointByteCount);
            var repository = new RecordingRepository(calls, fixture.CheckpointBytes);
            repository.StreamFactoriesByUnit[DesignConversionRuntimeScaffoldCriticUnit.UnitId] = () => oversized;
            var loader = new DesignConversionRepositoryActivatedModelCheckpointLoader(
                repository,
                new RecordingLocator(calls, BuildManifestMap(fixture)),
                cacheRoot);

            var result = await loader.LoadAsync(fixture.ActiveVersion);

            Assert.False(result.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_CHECKPOINT_TOO_LARGE", result.ErrorCode);
            Assert.Equal(["locate:critic", "load:critic"], calls);
            Assert.InRange(oversized.BytesProvided, 1, OversizedCheckpointByteCount - 1);
            Assert.True(oversized.Disposed);
            Assert.False(Directory.Exists(cacheRoot));
            AssertNoPrivateFailureLeak(result, fixture);
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    [Fact]
    public async Task RepositoryLoader_CacheFilesystemCollisionFailsClosedWithoutPartialTemp()
    {
        var fixture = await CreateFixtureAsync("cache-filesystem-collision");
        try
        {
            var calls = new List<string>();
            var cacheRoot = Path.Combine(fixture.TempRoot, $"{PrivateMarker}-cache-root-file");
            var collisionBytes = Utf8NoBom.GetBytes($"{PrivateMarker}:cache-root-is-a-file");
            await File.WriteAllBytesAsync(cacheRoot, collisionBytes);
            var loader = new DesignConversionRepositoryActivatedModelCheckpointLoader(
                new RecordingRepository(calls, fixture.CheckpointBytes),
                new RecordingLocator(calls, BuildManifestMap(fixture)),
                cacheRoot);

            var result = await loader.LoadAsync(fixture.ActiveVersion);

            Assert.False(result.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_ACTIVE_MODEL_REPOSITORY_CACHE_WRITE_FAILED", result.ErrorCode);
            Assert.Equal(
                ["locate:critic", "load:critic", "locate:improver", "load:improver"],
                calls);
            Assert.Equal(collisionBytes, await File.ReadAllBytesAsync(cacheRoot));
            Assert.Empty(Directory.EnumerateFiles(fixture.TempRoot, "*.tmp", SearchOption.AllDirectories));
            AssertNoPrivateFailureLeak(result, fixture);
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    [Fact]
    public void RepositoryLoader_ConstructorRequiresExplicitDependenciesAndCacheRoot()
    {
        var calls = new List<string>();
        var repository = new RecordingRepository(calls, []);
        var locator = new RecordingLocator(calls, new Dictionary<string, RemoteUnitManifest>(StringComparer.Ordinal));

        Assert.Throws<ArgumentNullException>(() => new DesignConversionRepositoryActivatedModelCheckpointLoader(
            null!,
            locator,
            "cache"));
        Assert.Throws<ArgumentNullException>(() => new DesignConversionRepositoryActivatedModelCheckpointLoader(
            repository,
            null!,
            "cache"));
        Assert.Throws<ArgumentException>(() => new DesignConversionRepositoryActivatedModelCheckpointLoader(
            repository,
            locator,
            " "));
    }

    private static RemoteUnitManifest MutateManifest(RemoteUnitManifest manifest, string mutation)
        => mutation switch
        {
            "hash" => manifest with
            {
                ManifestSha256 = new string('0', 64)
            },
            "version" => manifest with
            {
                Version = "9.9.9"
            },
            "domain" => manifest with
            {
                DomainTag = "OtherDomain"
            },
            "unit" => manifest with
            {
                UnitId = manifest.UnitId + "-wrong"
            },
            "role" => manifest with
            {
                UnitRole = manifest.UnitRole + "-wrong"
            },
            "raw-url" => manifest with
            {
                ServerBaseUrl = $"https://models.example/repository/../{PrivateMarker}"
            },
            "encoded-url" => manifest with
            {
                ServerBaseUrl = $"https://models.example/repository/%252e%252e/{PrivateMarker}"
            },
            "endpoint" => manifest with
            {
                UnitEndpoint = $"/models/../{PrivateMarker}"
            },
            "safe-wrong-endpoint" => manifest with
            {
                UnitEndpoint = "/models/another-safe-unit"
            },
            "endpoint-query" => manifest with
            {
                UnitEndpoint = $"{manifest.UnitEndpoint}?token=secret"
            },
            "endpoint-fragment" => manifest with
            {
                UnitEndpoint = $"{manifest.UnitEndpoint}#secret"
            },
            "endpoint-absolute" => manifest with
            {
                UnitEndpoint = $"https://models.example{manifest.UnitEndpoint}"
            },
            "endpoint-authority" => manifest with
            {
                UnitEndpoint = $"//models.example{manifest.UnitEndpoint}"
            },
            "endpoint-extra-segment" => manifest with
            {
                UnitEndpoint = $"{manifest.UnitEndpoint}/extra"
            },
            "endpoint-raw-backslash" => manifest with
            {
                UnitEndpoint = $"{manifest.UnitEndpoint}\\extra"
            },
            "endpoint-malformed-percent" => manifest with
            {
                UnitEndpoint = $"{manifest.UnitEndpoint}%2"
            },
            "endpoint-noncanonical-percent-spelling" => manifest with
            {
                UnitEndpoint = "/models/%64esign_conversion_runtime_scaffold_critic"
            },
            _ => throw new ArgumentOutOfRangeException(nameof(mutation), mutation, "Unknown mutation.")
        };

    private static OperationResult<bool> ValidateUnitEndpointForTest(
        string endpoint,
        string expectedUnitId,
        string serverBaseUrl = ServerBaseUrl)
    {
        var calls = new List<string>();
        var loader = new DesignConversionRepositoryActivatedModelCheckpointLoader(
            new RecordingRepository(calls, []),
            new RecordingLocator(calls, new Dictionary<string, RemoteUnitManifest>(StringComparer.Ordinal)),
            Path.GetTempPath());
        var validator = typeof(DesignConversionRepositoryActivatedModelCheckpointLoader).GetMethod(
            "ValidateUnitEndpoint",
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.NotNull(validator);
        var result = validator.Invoke(loader, [endpoint, serverBaseUrl, expectedUnitId]);
        return Assert.IsType<OperationResult<bool>>(result);
    }

    private static async Task<CheckpointFixture> CreateFixtureAsync(string name)
    {
        var tempRoot = Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "llm_mvp_core",
            "design-conversion-activated-checkpoint-loader-tests",
            Guid.NewGuid().ToString("N")));
        var checkpointDirectory = Path.Combine(tempRoot, "checkpoint");
        Directory.CreateDirectory(checkpointDirectory);
        var checkpointPath = Path.Combine(checkpointDirectory, "design-conversion-checkpoint.json");
        var corruptLocalPath = Path.Combine(tempRoot, $"{PrivateMarker}-corrupt-local-checkpoint.json");

        var machine = new DesignConversionMachine();
        var trained = await DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(machine, new LearningBatch<DesignDomain>(
            BatchId: $"{name}-batch",
            DomainTag: DesignConversionMachine.DomainTag,
            TrainingItems:
            [
                new LearningTrainingItem(
                    ItemId: $"{name}-item",
                    Prompt: JsonSerializer.Serialize(new Dictionary<string, object>
                    {
                        ["unit_id"] = DesignConversionRuntimeScaffoldCriticUnit.UnitId,
                        ["source_kind"] = "Figma",
                        ["source_manifest_hash"] = $"{name}-manifest-source",
                        ["source_identity_hash"] = $"{name}-identity",
                        ["source_structure_hash"] = $"{name}-structure"
                    }),
                    ChosenResponse: "{\"html\":\"<main class=\\\"layout-a\\\"></main>\",\"css\":\".layout-a{display:grid;gap:16px}\"}",
                    RejectedResponse: "{\"html\":\"<main class=\\\"layout-b\\\"></main>\",\"css\":\".layout-b{display:block;gap:2px}\"}")
            ],
            SourceIds: [$"{name}-source"]));
        Assert.True(trained.IsSuccess, trained.ErrorMessage);

        var saved = await machine.SaveCheckpointAsync(checkpointPath);
        Assert.True(saved.IsSuccess, saved.ErrorMessage);
        var load = await DesignConversionMachine.LoadCheckpointAsync(checkpointPath);
        Assert.True(load.IsSuccess, load.ErrorMessage);
        Assert.True(DesignConversionMachine.CheckpointHasRealLearnedWeights(load.Value!.Checkpoint));
        await File.WriteAllTextAsync(
            corruptLocalPath,
            $"{PrivateMarker}:this local checkpoint is intentionally corrupt and must not be used",
            Utf8NoBom);

        var checkpointBytes = await File.ReadAllBytesAsync(checkpointPath);
        var checkpointSha256 = Sha256Hex(checkpointBytes);
        var activeVersion = new DesignConversionActivatedModelVersion(
            ActivationRef: $"activation-ref:{name}",
            ModelId: $"design-conversion-model:{name}",
            ManifestId: $"manifest:{name}",
            ManifestRef: $"manifest://design-conversion/{name}",
            PromotionUploadRef: $"repo-upload:{name}",
            Version: load.Value.Checkpoint.Version,
            MachineId: load.Value.Checkpoint.MachineId,
            DomainTag: load.Value.Checkpoint.DomainTag,
            CheckpointPath: Path.GetFullPath(checkpointPath),
            CheckpointPathRef: "checkpoint-path-ref:valid-local-path",
            CheckpointSha256: checkpointSha256,
            CheckpointFreshLoadVerified: true,
            CheckpointContainsRealTrainedWeights: true,
            LocalOnly: true,
            EvidenceRefs:
            [
                "DESIGN_CONVERSION_MODEL_ACTIVATION_RECORD_VERIFIED",
                DesignConversionRepositoryActivatedModelCheckpointLoader.RepositoryFreshLoadEvidence
            ]);

        return new CheckpointFixture(
            TempRoot: tempRoot,
            CheckpointPath: Path.GetFullPath(checkpointPath),
            CorruptLocalCheckpointPath: Path.GetFullPath(corruptLocalPath),
            CheckpointBytes: checkpointBytes,
            CheckpointSha256: checkpointSha256,
            ActiveVersion: activeVersion);
    }

    private static Dictionary<string, RemoteUnitManifest> BuildManifestMap(CheckpointFixture fixture)
        => new(StringComparer.Ordinal)
        {
            [DesignConversionRuntimeScaffoldCriticUnit.UnitId] = BuildManifest(
                DesignConversionRuntimeScaffoldCriticUnit.UnitId,
                DesignConversionRuntimeScaffoldCriticUnit.Role,
                fixture),
            [DesignConversionRuntimeScaffoldImproverUnit.UnitId] = BuildManifest(
                DesignConversionRuntimeScaffoldImproverUnit.UnitId,
                DesignConversionRuntimeScaffoldImproverUnit.Role,
                fixture)
        };

    private static RemoteUnitManifest BuildManifest(
        string unitId,
        string unitRole,
        CheckpointFixture fixture)
        => new(
            UnitId: unitId,
            UnitRole: unitRole,
            DomainTag: fixture.ActiveVersion.DomainTag,
            Version: fixture.ActiveVersion.Version,
            ServerBaseUrl: ServerBaseUrl,
            UnitEndpoint: $"/models/{Uri.EscapeDataString(unitId)}",
            ManifestSha256: fixture.CheckpointSha256.ToUpperInvariant(),
            TrainingBranch: "DesignToCode-clean",
            TrainingRunId: "repository-loader-test-run",
            SourceDatasetSha256: Sha256Hex(Utf8NoBom.GetBytes("repository-loader-test-source-dataset")),
            ModelConfig: new RemoteModelConfig(0, 0, 0, 0, 0, 0, 0),
            LoraConfig: new RemoteLoraConfig(0, 0f, 0f, []),
            TokenizerPath: "tokenizer-ref-only",
            RegisteredAtUtc: RegisteredAtUtc);

    private static string ExpectedCachePath(string cacheRoot, string checkpointSha256)
        => Path.GetFullPath(Path.Combine(
            cacheRoot,
            "design-conversion-active-checkpoints",
            checkpointSha256[..2],
            checkpointSha256,
            "design-conversion-machine-checkpoint.json"));

    private static void AssertNoPrivateFailureLeak(
        OperationResult<DesignConversionMachineLoadResult> result,
        CheckpointFixture fixture)
    {
        var surface = JsonSerializer.Serialize(new
        {
            result.IsSuccess,
            result.ErrorCode,
            result.ErrorMessage
        });
        Assert.DoesNotContain(PrivateMarker, surface, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(fixture.TempRoot, surface, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(fixture.TempRoot.Replace('\\', '/'), surface, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(fixture.CheckpointPath, surface, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(fixture.CorruptLocalCheckpointPath, surface, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("user:secret@", surface, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("api_key", surface, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("token=", surface, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("credential", surface, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("IOException", surface, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("InvalidOperationException", surface, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("UnauthorizedAccessException", surface, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("payload", surface, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Convert.ToBase64String(fixture.CheckpointBytes), surface, StringComparison.Ordinal);
    }

    private static string Sha256Hex(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static string UnitAlias(string unitId)
        => string.Equals(unitId, DesignConversionRuntimeScaffoldCriticUnit.UnitId, StringComparison.Ordinal)
            ? "critic"
            : string.Equals(unitId, DesignConversionRuntimeScaffoldImproverUnit.UnitId, StringComparison.Ordinal)
                ? "improver"
                : unitId;

    private static void DeleteDirectoryIfExists(string path)
    {
        if (!Directory.Exists(path))
            return;

        try
        {
            Directory.Delete(path, recursive: true);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private sealed record CheckpointFixture(
        string TempRoot,
        string CheckpointPath,
        string CorruptLocalCheckpointPath,
        byte[] CheckpointBytes,
        string CheckpointSha256,
        DesignConversionActivatedModelVersion ActiveVersion);

    private sealed record RepositoryLoadInput(string UnitId, string? ServerBaseUrl);

    private sealed class RecordingRepository(
        List<string> calls,
        byte[] defaultBytes) : IModelRepository
    {
        public OperationResult<string> PreflightStableConfigurationIdentitySha256()
            => OperationResult<string>.Ok("sha256:" + new string('a', 64));

        public Dictionary<string, byte[]> BytesByUnit { get; } = new(StringComparer.Ordinal);
        public Dictionary<string, Func<Stream>> StreamFactoriesByUnit { get; } = new(StringComparer.Ordinal);
        public Dictionary<string, string> LocalFallbackPathByUnit { get; } = new(StringComparer.Ordinal);
        public List<RepositoryLoadInput> LoadInputs { get; } = [];
        public string? NullStreamUnitId { get; init; }
        public string? FailLoadUnitId { get; init; }
        public string? BoundaryFailureMode { get; init; }
        public CancellationTokenSource? CancelAndThrowSource { get; init; }

        public Task<OperationResult<Stream>> LoadWeightsAsync(
            string unitId,
            string? serverBaseUrl,
            string localFallbackPath,
            CancellationToken ct = default)
        {
            calls.Add($"load:{UnitAlias(unitId)}");
            LoadInputs.Add(new RepositoryLoadInput(unitId, serverBaseUrl));
            LocalFallbackPathByUnit[unitId] = Path.GetFullPath(localFallbackPath);
            if (CancelAndThrowSource is not null)
            {
                CancelAndThrowSource.Cancel();
                return Task.FromException<OperationResult<Stream>>(
                    new OperationCanceledException(HostileDependencyFailure, ct));
            }

            if (BoundaryFailureMode == "sync-throw")
                throw new InvalidOperationException(HostileDependencyFailure);
            if (BoundaryFailureMode == "async-throw")
            {
                return Task.FromException<OperationResult<Stream>>(
                    new InvalidOperationException(HostileDependencyFailure));
            }
            if (BoundaryFailureMode == "null-task")
                return null!;

            if (ct.IsCancellationRequested)
                return Task.FromResult(OperationResult<Stream>.Fail("cancelled", "CANCELLED"));

            if (string.Equals(FailLoadUnitId, unitId, StringComparison.Ordinal))
            {
                return Task.FromResult(OperationResult<Stream>.Fail(
                    $@"IOException at C:\private\{PrivateMarker}\checkpoint.json?api_key=secret payload={PrivateMarker}",
                    "IOException_SECRET_TOKEN_PAYLOAD"));
            }

            if (string.Equals(NullStreamUnitId, unitId, StringComparison.Ordinal))
                return Task.FromResult(OperationResult<Stream>.Ok(null!));

            if (StreamFactoriesByUnit.TryGetValue(unitId, out var streamFactory))
                return Task.FromResult(OperationResult<Stream>.Ok(streamFactory()));

            var bytes = BytesByUnit.TryGetValue(unitId, out var unitBytes)
                ? unitBytes
                : defaultBytes;
            return Task.FromResult(OperationResult<Stream>.Ok(new MemoryStream(bytes, writable: false)));
        }

        public Task<OperationResult<RemoteUnitManifest>> UploadAsync(
            GeneratedCapabilityCheckpointManifest manifest,
            string serverBaseUrl,
            CancellationToken ct = default)
            => Task.FromResult(OperationResult<RemoteUnitManifest>.Fail(
                "Upload is not used by activated checkpoint loader tests.",
                "UPLOAD_NOT_USED_BY_TEST"));
    }

    private sealed class RecordingLocator(
        List<string> calls,
        Dictionary<string, RemoteUnitManifest> manifests) : IRemoteUnitLocator
    {
        public OperationResult<string> PreflightStableConfigurationIdentitySha256()
            => OperationResult<string>.Ok("sha256:" + new string('b', 64));

        public string? BoundaryFailureMode { get; init; }

        public Task<OperationResult<RemoteUnitManifest>> LocateAsync(
            string unitId,
            CancellationToken ct = default)
        {
            calls.Add($"locate:{UnitAlias(unitId)}");
            if (BoundaryFailureMode == "sync-throw")
                throw new InvalidOperationException(HostileDependencyFailure);
            if (BoundaryFailureMode == "async-throw")
            {
                return Task.FromException<OperationResult<RemoteUnitManifest>>(
                    new InvalidOperationException(HostileDependencyFailure));
            }
            if (BoundaryFailureMode == "null-task")
                return null!;

            if (ct.IsCancellationRequested)
                return Task.FromResult(OperationResult<RemoteUnitManifest>.Fail("cancelled", "CANCELLED"));

            return manifests.TryGetValue(unitId, out var manifest)
                ? Task.FromResult(OperationResult<RemoteUnitManifest>.Ok(manifest))
                : Task.FromResult(OperationResult<RemoteUnitManifest>.Fail(
                    "Remote unit not found.",
                    "REMOTE_UNIT_NOT_FOUND"));
        }

        public Task<OperationResult<bool>> RegisterAsync(
            RemoteUnitManifest manifest,
            CancellationToken ct = default)
            => Task.FromResult(OperationResult<bool>.Fail(
                "Register is not used by activated checkpoint loader tests.",
                "REGISTER_NOT_USED_BY_TEST"));
    }

    private sealed class OversizedSeekableStream(long length) : Stream
    {
        private long _position;

        public bool ReadAttempted { get; private set; }
        public bool Disposed { get; private set; }
        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length => length;
        public override long Position
        {
            get => _position;
            set => _position = value;
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            ReadAttempted = true;
            throw new InvalidOperationException("Oversized seekable stream must be rejected before read.");
        }

        public override ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            ReadAttempted = true;
            return ValueTask.FromException<int>(
                new InvalidOperationException("Oversized seekable stream must be rejected before read."));
        }

        public override long Seek(long offset, SeekOrigin origin)
            => throw new NotSupportedException();

        public override void SetLength(long value)
            => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            Disposed = true;
            base.Dispose(disposing);
        }
    }

    private sealed class GeneratedNonSeekableStream(long byteCount) : Stream
    {
        private long _remaining = byteCount;

        public long BytesProvided { get; private set; }
        public bool Disposed { get; private set; }
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_remaining == 0)
                return 0;

            var read = (int)Math.Min(_remaining, count);
            buffer.AsSpan(offset, read).Fill(0x5a);
            _remaining -= read;
            BytesProvided += read;
            return read;
        }

        public override ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                return ValueTask.FromCanceled<int>(cancellationToken);
            if (_remaining == 0)
                return ValueTask.FromResult(0);

            var read = (int)Math.Min(_remaining, buffer.Length);
            buffer.Span[..read].Fill(0x5a);
            _remaining -= read;
            BytesProvided += read;
            return ValueTask.FromResult(read);
        }

        public override long Seek(long offset, SeekOrigin origin)
            => throw new NotSupportedException();

        public override void SetLength(long value)
            => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            Disposed = true;
            base.Dispose(disposing);
        }
    }
}
