// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Buffers.Binary;
using System.IO.Compression;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using DesignConversionDomain;
using ModelServer.DesignConversion;

public sealed class DesignConversionModelServerArtifactUploadStoreTests
{
    private const string ArtifactRefProofRequired = "DESIGN_CONVERSION_ARTIFACT_REF_PROOF_REQUIRED";

    [Fact]
    public void ValidateUploadMetadata_NullAndEmptyMetadata_AcceptsLegacyLogicalDimensions()
    {
        var nullResult = DesignConversionModelServerArtifactUploadStore.ValidateUploadMetadata(
            metadata: null,
            width: 9,
            height: 7);
        var emptyResult = DesignConversionModelServerArtifactUploadStore.ValidateUploadMetadata(
            new Dictionary<string, string>(),
            width: 9,
            height: 7);

        Assert.True(nullResult.IsSuccess, nullResult.ErrorMessage);
        Assert.NotNull(nullResult.Value);
        Assert.Empty(nullResult.Value);
        Assert.True(emptyResult.IsSuccess, emptyResult.ErrorMessage);
        Assert.NotNull(emptyResult.Value);
        Assert.Empty(emptyResult.Value);
    }

    [Fact]
    public void ValidateUploadMetadata_CanonicalScaledMetadata_ReturnsCanonicalOrderedValues()
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [DesignConversionModelServerArtifactUploadStore.ViewportIdMetadataKey] =
                "source-9x7@2~full-startup",
            [DesignConversionModelServerArtifactUploadStore.PngHeightMetadataKey] = "14",
            [DesignConversionModelServerArtifactUploadStore.DeviceScaleFactorMetadataKey] = "2",
            [DesignConversionModelServerArtifactUploadStore.ViewportHeightMetadataKey] = "7",
            [DesignConversionModelServerArtifactUploadStore.PngWidthMetadataKey] = "18",
            [DesignConversionModelServerArtifactUploadStore.ViewportWidthMetadataKey] = "9"
        };

        var result = DesignConversionModelServerArtifactUploadStore.ValidateUploadMetadata(
            metadata,
            width: 9,
            height: 7);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.NotNull(result.Value);
        Assert.Equal(
            [
                DesignConversionModelServerArtifactUploadStore.ViewportWidthMetadataKey,
                DesignConversionModelServerArtifactUploadStore.ViewportHeightMetadataKey,
                DesignConversionModelServerArtifactUploadStore.PngWidthMetadataKey,
                DesignConversionModelServerArtifactUploadStore.PngHeightMetadataKey,
                DesignConversionModelServerArtifactUploadStore.DeviceScaleFactorMetadataKey,
                DesignConversionModelServerArtifactUploadStore.ViewportIdMetadataKey
            ],
            result.Value.Keys);
        Assert.Equal("9", result.Value[DesignConversionModelServerArtifactUploadStore.ViewportWidthMetadataKey]);
        Assert.Equal("7", result.Value[DesignConversionModelServerArtifactUploadStore.ViewportHeightMetadataKey]);
        Assert.Equal("18", result.Value[DesignConversionModelServerArtifactUploadStore.PngWidthMetadataKey]);
        Assert.Equal("14", result.Value[DesignConversionModelServerArtifactUploadStore.PngHeightMetadataKey]);
        Assert.Equal("2", result.Value[DesignConversionModelServerArtifactUploadStore.DeviceScaleFactorMetadataKey]);
        Assert.Equal(
            "source-9x7@2~full-startup",
            result.Value[DesignConversionModelServerArtifactUploadStore.ViewportIdMetadataKey]);
    }

    [Fact]
    public void ValidateUploadMetadata_UnknownReservedKey_RejectsMetadata()
    {
        var result = DesignConversionModelServerArtifactUploadStore.ValidateUploadMetadata(
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["media_type"] = "image/png"
            },
            width: 9,
            height: 7);

        AssertMetadataInvalid(result);
    }

    [Fact]
    public void ValidateUploadMetadata_DuplicateByCaseKey_RejectsMetadata()
    {
        var result = DesignConversionModelServerArtifactUploadStore.ValidateUploadMetadata(
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [DesignConversionModelServerArtifactUploadStore.ViewportWidthMetadataKey] = "9",
                ["VIEWPORT_WIDTH"] = "9"
            },
            width: 9,
            height: 7);

        AssertMetadataInvalid(result);
    }

    [Fact]
    public void ValidateUploadMetadata_PartialViewportPair_RejectsMetadata()
    {
        var result = DesignConversionModelServerArtifactUploadStore.ValidateUploadMetadata(
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [DesignConversionModelServerArtifactUploadStore.ViewportWidthMetadataKey] = "9"
            },
            width: 9,
            height: 7);

        AssertMetadataInvalid(result);
    }

    [Fact]
    public void ValidateUploadMetadata_ViewportPairDoesNotMatchLogicalDimensions_RejectsMetadata()
    {
        var result = DesignConversionModelServerArtifactUploadStore.ValidateUploadMetadata(
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [DesignConversionModelServerArtifactUploadStore.ViewportWidthMetadataKey] = "10",
                [DesignConversionModelServerArtifactUploadStore.ViewportHeightMetadataKey] = "7"
            },
            width: 9,
            height: 7);

        AssertMetadataInvalid(result);
    }

    [Fact]
    public void ValidateUploadMetadata_PhysicalPngWithoutExplicitScale_RejectsMetadata()
    {
        var result = DesignConversionModelServerArtifactUploadStore.ValidateUploadMetadata(
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [DesignConversionModelServerArtifactUploadStore.PngWidthMetadataKey] = "18",
                [DesignConversionModelServerArtifactUploadStore.PngHeightMetadataKey] = "14"
            },
            width: 9,
            height: 7);

        AssertMetadataInvalid(result);
    }

    [Fact]
    public void ValidateUploadMetadata_NoncanonicalDeviceScaleFactor_RejectsMetadata()
    {
        var result = DesignConversionModelServerArtifactUploadStore.ValidateUploadMetadata(
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [DesignConversionModelServerArtifactUploadStore.PngWidthMetadataKey] = "18",
                [DesignConversionModelServerArtifactUploadStore.PngHeightMetadataKey] = "14",
                [DesignConversionModelServerArtifactUploadStore.DeviceScaleFactorMetadataKey] = "2.0"
            },
            width: 9,
            height: 7);

        AssertMetadataInvalid(result);
    }

    [Fact]
    public void ValidateUploadMetadata_UnsafeViewportIdentity_RejectsWithoutEchoingRawMarker()
    {
        const string rawMarker = "PRIVATE_UPLOAD_MARKER/../../viewport";
        var result = DesignConversionModelServerArtifactUploadStore.ValidateUploadMetadata(
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [DesignConversionModelServerArtifactUploadStore.ViewportIdMetadataKey] = rawMarker
            },
            width: 9,
            height: 7);

        AssertMetadataInvalid(result);
        Assert.DoesNotContain(rawMarker, result.ErrorMessage, StringComparison.Ordinal);
    }

    [Fact]
    public void ValidateUploadMetadata_EmbeddedScaleWithoutExplicitScale_RejectsBeforeResolver()
    {
        var result = DesignConversionModelServerArtifactUploadStore.ValidateUploadMetadata(
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [DesignConversionModelServerArtifactUploadStore.PngWidthMetadataKey] = "18",
                [DesignConversionModelServerArtifactUploadStore.PngHeightMetadataKey] = "14",
                [DesignConversionModelServerArtifactUploadStore.ViewportIdMetadataKey] = "source-9x7@2~full-startup"
            },
            width: 9,
            height: 7);

        AssertMetadataInvalid(result);
    }

    [Fact]
    public async Task StoreAsync_ValidNonblankPng_ReturnsResolverValidReceipt()
    {
        var root = CreateTempRoot("valid");
        try
        {
            var png = BuildRgbaPng(6, 4, blank: false);
            var store = new DesignConversionModelServerArtifactUploadStore(root, maxBytes: 1024 * 1024);
            var request = CreateRequest(png, width: 6, height: 4);

            var result = await store.StoreAsync(new MemoryStream(png, writable: false), request);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            var receipt = result.Value!;
            Assert.Equal(request.ArtifactId, receipt.ArtifactId);
            Assert.Equal(request.Kind, receipt.Kind);
            Assert.Equal("image/png", receipt.MediaType);
            Assert.Equal(png.Length, receipt.ByteCount);
            Assert.Equal(Sha256(png), receipt.Sha256);
            Assert.Equal(6, receipt.Width);
            Assert.Equal(4, receipt.Height);
            Assert.Equal(receipt.Sha256, receipt.ArtifactRef.Sha256);
            Assert.Equal(receipt.ArtifactId, receipt.ArtifactRef.ArtifactId);
            Assert.Equal(receipt.Kind, receipt.ArtifactRef.Kind);
            Assert.Equal(receipt.Width, receipt.ArtifactRef.Width);
            Assert.Equal(receipt.Height, receipt.ArtifactRef.Height);

            var localPath = new Uri(receipt.ArtifactRef.Uri).LocalPath;
            Assert.True(File.Exists(localPath));
            Assert.Equal(png, File.ReadAllBytes(localPath));

            var resolver = new DesignConversionServerOwnedLocalArtifactRefResolver(root);
            var resolved = resolver.Validate(receipt.ArtifactRef, "upload store test");
            Assert.True(resolved.IsSuccess, resolved.ErrorMessage);
            Assert.Empty(TempFiles(root));
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task StoreAsync_CrcCorruptPngWithMatchingExpectedSha_FailsAndCleans()
    {
        var root = CreateTempRoot("crc-corrupt-matching-sha");
        try
        {
            var valid = BuildRgbaPng(6, 4, blank: false);
            var corrupt = CorruptStoredPngChunkCrc(valid, "IDAT");
            var store = new DesignConversionModelServerArtifactUploadStore(root, maxBytes: 1024 * 1024);
            var request = CreateRequest(corrupt, width: 6, height: 4);
            Assert.Equal(Sha256(corrupt), request.ExpectedSha256);

            var result = await store.StoreAsync(new MemoryStream(corrupt, writable: false), request);

            Assert.False(result.IsSuccess);
            Assert.Equal(DesignConversionModelServerArtifactUploadStore.PngValidationErrorCode, result.ErrorCode);
            AssertCleanFailureRoot(root);
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    private static void AssertMetadataInvalid(
        AgentInfrastructure.OperationResult<IReadOnlyDictionary<string, string>> result)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(DesignConversionModelServerArtifactUploadStore.MetadataInvalidErrorCode, result.ErrorCode);
    }

    [Fact]
    public void ServerOwnedLocalArtifactRefResolver_AcceptsCanonicalScaledPhysicalPngDimensions()
    {
        var root = CreateTempRoot("resolver-scaled-canonical");
        try
        {
            var resolution = new RenderQaResolution("resolver-scaled", 101, 51, 1.5);
            var physical = DesignConversionBrowserPhysicalScreenshotResolution.Resolve(
                resolution,
                ArtifactRefProofRequired,
                "resolver scaled test");
            Assert.True(physical.IsSuccess, physical.ErrorMessage);
            var png = BuildRgbaPng(physical.Value!.PhysicalWidth, physical.Value.PhysicalHeight, blank: false);
            var artifact = CreateResolverArtifact(
                root,
                "resolver-scaled-canonical",
                resolution.Width,
                resolution.Height,
                png,
                ResolverProofMetadata(
                    resolution.Width,
                    resolution.Height,
                    physical.Value.PhysicalWidth,
                    physical.Value.PhysicalHeight,
                    resolution.DeviceScaleFactor));
            var resolver = new DesignConversionServerOwnedLocalArtifactRefResolver(root);

            var result = resolver.Validate(artifact, "resolver scaled canonical test");

            Assert.True(result.IsSuccess, result.ErrorMessage);
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public void ServerOwnedLocalArtifactRefResolver_RejectsWrongPositiveScaledPhysicalPngDimensions()
    {
        var root = CreateTempRoot("resolver-scaled-wrong");
        try
        {
            var resolution = new RenderQaResolution("resolver-scaled", 101, 51, 1.5);
            var png = BuildRgbaPng(151, 77, blank: false);
            var artifact = CreateResolverArtifact(
                root,
                "resolver-scaled-wrong",
                resolution.Width,
                resolution.Height,
                png,
                ResolverProofMetadata(
                    resolution.Width,
                    resolution.Height,
                    pngWidth: 151,
                    pngHeight: 77,
                    resolution.DeviceScaleFactor));
            var resolver = new DesignConversionServerOwnedLocalArtifactRefResolver(root);

            var result = resolver.Validate(artifact, "resolver scaled wrong test");

            Assert.False(result.IsSuccess);
            Assert.Equal(ArtifactRefProofRequired, result.ErrorCode);
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public void ServerOwnedLocalArtifactRefResolver_RejectsPartialPngDimensionMetadata()
    {
        var root = CreateTempRoot("resolver-partial-png-dimensions");
        try
        {
            var resolution = new RenderQaResolution("resolver-scaled", 101, 51, 1.5);
            var physical = DesignConversionBrowserPhysicalScreenshotResolution.Resolve(
                resolution,
                ArtifactRefProofRequired,
                "resolver partial PNG metadata test");
            Assert.True(physical.IsSuccess, physical.ErrorMessage);
            var png = BuildRgbaPng(physical.Value!.PhysicalWidth, physical.Value.PhysicalHeight, blank: false);
            var metadata = ResolverProofMetadata(
                resolution.Width,
                resolution.Height,
                physical.Value.PhysicalWidth,
                physical.Value.PhysicalHeight,
                resolution.DeviceScaleFactor);
            metadata.Remove("png_height");
            var artifact = CreateResolverArtifact(
                root,
                "resolver-partial-png-dimensions",
                resolution.Width,
                resolution.Height,
                png,
                metadata);
            var resolver = new DesignConversionServerOwnedLocalArtifactRefResolver(root);

            var result = resolver.Validate(artifact, "resolver partial PNG metadata test");

            Assert.False(result.IsSuccess);
            Assert.Equal(ArtifactRefProofRequired, result.ErrorCode);
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public void ServerOwnedLocalArtifactRefResolver_RejectsInvalidDeviceScaleFactorMetadata()
    {
        var root = CreateTempRoot("resolver-invalid-dsf");
        try
        {
            var png = BuildRgbaPng(101, 51, blank: false);
            var metadata = ResolverProofMetadata(
                viewportWidth: 101,
                viewportHeight: 51,
                pngWidth: 101,
                pngHeight: 51,
                deviceScaleFactor: 1.0);
            metadata[DesignConversionRenderResolutionFactory.DeviceScaleFactorMetadataKey] = "PRIVATE_BAD_SCALE_TOKEN";
            var artifact = CreateResolverArtifact(
                root,
                "resolver-invalid-dsf",
                width: 101,
                height: 51,
                png: png,
                metadata: metadata);
            var resolver = new DesignConversionServerOwnedLocalArtifactRefResolver(root);

            var result = resolver.Validate(artifact, "resolver invalid DSF test");

            Assert.False(result.IsSuccess);
            Assert.Equal(ArtifactRefProofRequired, result.ErrorCode);
            Assert.DoesNotContain("PRIVATE_BAD_SCALE_TOKEN", result.ErrorMessage, StringComparison.Ordinal);
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task StoreAsync_OversizeStream_FailsEarlyAndLeavesNoFinalOrTemp()
    {
        var root = CreateTempRoot("oversize");
        try
        {
            var payload = Enumerable.Repeat((byte)0x41, 512 * 1024).ToArray();
            var source = new CountingReadStream(payload);
            var store = new DesignConversionModelServerArtifactUploadStore(root, maxBytes: 128);
            var request = new DesignConversionModelServerArtifactUploadRequest(
                ArtifactId: "oversize-upload",
                Kind: "screenshot",
                MediaType: "image/png",
                ExpectedSha256: "sha256:" + new string('0', 64),
                Width: 1,
                Height: 1);

            var result = await store.StoreAsync(source, request);

            Assert.False(result.IsSuccess);
            Assert.Equal(DesignConversionModelServerArtifactUploadStore.OversizeErrorCode, result.ErrorCode);
            Assert.Equal(129L, source.BytesRead);
            AssertCleanFailureRoot(root);
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Theory]
    [MemberData(nameof(NonCanonicalSha256Cases))]
    public async Task StoreAsync_NonCanonicalExpectedSha256_FailsMetadataValidationAndCleans(
        string caseName,
        string expectedSha256)
    {
        var root = CreateTempRoot($"sha-{caseName}");
        try
        {
            var png = BuildRgbaPng(4, 3, blank: false);
            var store = new DesignConversionModelServerArtifactUploadStore(root, maxBytes: 1024 * 1024);
            var request = CreateRequest(png, width: 4, height: 3, expectedSha256: expectedSha256);

            var result = await store.StoreAsync(new MemoryStream(png, writable: false), request);

            Assert.False(result.IsSuccess);
            Assert.Equal(DesignConversionModelServerArtifactUploadStore.MetadataInvalidErrorCode, result.ErrorCode);
            AssertCleanFailureRoot(root);
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task StoreAsync_WrongExpectedSha_FailsAndCleans()
    {
        var root = CreateTempRoot("wrong-sha");
        try
        {
            var png = BuildRgbaPng(5, 3, blank: false);
            var store = new DesignConversionModelServerArtifactUploadStore(root, maxBytes: 1024 * 1024);
            var request = CreateRequest(
                png,
                width: 5,
                height: 3,
                expectedSha256: "sha256:" + new string('0', 64));

            var result = await store.StoreAsync(new MemoryStream(png, writable: false), request);

            Assert.False(result.IsSuccess);
            Assert.Equal(DesignConversionModelServerArtifactUploadStore.HashMismatchErrorCode, result.ErrorCode);
            AssertCleanFailureRoot(root);
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Theory]
    [MemberData(nameof(InvalidOrBlankPngCases))]
    public async Task StoreAsync_InvalidOrBlankPng_FailsAndCleans(
        string artifactId,
        byte[] bytes,
        int width,
        int height)
    {
        var root = CreateTempRoot(artifactId);
        try
        {
            var store = new DesignConversionModelServerArtifactUploadStore(root, maxBytes: 1024 * 1024);
            var request = CreateRequest(bytes, artifactId: artifactId, width: width, height: height);

            var result = await store.StoreAsync(new MemoryStream(bytes, writable: false), request);

            Assert.False(result.IsSuccess);
            Assert.Equal(DesignConversionModelServerArtifactUploadStore.PngValidationErrorCode, result.ErrorCode);
            AssertCleanFailureRoot(root);
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task StoreAsync_DimensionMismatch_FailsAndCleans()
    {
        var root = CreateTempRoot("dimension-mismatch");
        try
        {
            var png = BuildRgbaPng(4, 3, blank: false);
            var store = new DesignConversionModelServerArtifactUploadStore(root, maxBytes: 1024 * 1024);
            var request = CreateRequest(png, width: 5, height: 3);

            var result = await store.StoreAsync(new MemoryStream(png, writable: false), request);

            Assert.False(result.IsSuccess);
            Assert.Equal(DesignConversionModelServerArtifactUploadStore.PngValidationErrorCode, result.ErrorCode);
            AssertCleanFailureRoot(root);
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task StoreAsync_CancellationDuringUpload_Cleans()
    {
        var root = CreateTempRoot("cancel");
        try
        {
            var png = BuildRgbaPng(4, 3, blank: false);
            using var cts = new CancellationTokenSource();
            var source = new CancellingReadStream(png, cts);
            var store = new DesignConversionModelServerArtifactUploadStore(root, maxBytes: 1024 * 1024);
            var request = CreateRequest(png, width: 4, height: 3);

            var result = await store.StoreAsync(source, request, cts.Token);

            Assert.False(result.IsSuccess);
            Assert.Equal("CANCELLED", result.ErrorCode);
            AssertCleanFailureRoot(root);
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task StoreAsync_ConcurrentSameContentUploads_DeduplicateSafely()
    {
        var root = CreateTempRoot("concurrent");
        try
        {
            var png = BuildRgbaPng(7, 5, blank: false);
            var store = new DesignConversionModelServerArtifactUploadStore(root, maxBytes: 1024 * 1024);
            var request = CreateRequest(png, artifactId: "shared-artifact", width: 7, height: 5);

            var first = store.StoreAsync(new MemoryStream(png, writable: false), request);
            var second = store.StoreAsync(new MemoryStream(png, writable: false), request);
            var results = await Task.WhenAll(first, second);

            Assert.All(results, result => Assert.True(result.IsSuccess, result.ErrorMessage));
            Assert.Equal(results[0].Value!.ArtifactRef.Uri, results[1].Value!.ArtifactRef.Uri);
            Assert.Equal(results[0].Value!.Sha256, results[1].Value!.Sha256);
            Assert.Single(PublishedPngFiles(root));
            Assert.Empty(TempFiles(root));

            var resolver = new DesignConversionServerOwnedLocalArtifactRefResolver(root);
            var resolved = resolver.Validate(results[0].Value!.ArtifactRef, "concurrent upload store test");
            Assert.True(resolved.IsSuccess, resolved.ErrorMessage);
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task StoreAsync_SameContentWinnerTemporarilyLocked_ReconcilesAfterBoundedWait()
    {
        var root = CreateTempRoot("concurrent-locked-winner");
        try
        {
            var png = BuildRgbaPng(7, 5, blank: false);
            var store = new DesignConversionModelServerArtifactUploadStore(root, maxBytes: 1024 * 1024);
            var request = CreateRequest(png, artifactId: "locked-winner", width: 7, height: 5);
            var finalPath = ExpectedPublishedPath(root, request);
            Directory.CreateDirectory(Path.GetDirectoryName(finalPath)!);
            File.WriteAllBytes(finalPath, png);

            await using var winnerLease = new FileStream(
                finalPath,
                FileMode.Open,
                FileAccess.ReadWrite,
                FileShare.None);
            var releaseWinner = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(75));
                await winnerLease.DisposeAsync();
            });

            var result = await store.StoreAsync(new MemoryStream(png, writable: false), request);
            await releaseWinner;

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal(new Uri(finalPath).AbsoluteUri, result.Value!.ArtifactRef.Uri);
            Assert.Equal(request.ExpectedSha256, result.Value.Sha256);
            Assert.Equal(png, File.ReadAllBytes(finalPath));
            Assert.Single(PublishedPngFiles(root));
            Assert.Empty(TempFiles(root));
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task StoreAsync_RepeatedBarrierReleasedSameContentContenders_DeduplicateSafely()
    {
        const int rounds = 6;
        const int contendersPerRound = 8;
        var root = CreateTempRoot("concurrent-barrier-stress");
        try
        {
            var png = BuildRgbaPng(9, 6, blank: false);
            var store = new DesignConversionModelServerArtifactUploadStore(root, maxBytes: 1024 * 1024);

            for (var round = 0; round < rounds; round++)
            {
                var request = CreateRequest(
                    png,
                    artifactId: $"barrier-round-{round}",
                    width: 9,
                    height: 6);
                var start = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                var contenders = Enumerable.Range(0, contendersPerRound)
                    .Select(async _ =>
                    {
                        await start.Task;
                        return await store.StoreAsync(new MemoryStream(png, writable: false), request);
                    })
                    .ToArray();

                start.SetResult(true);
                var results = await Task.WhenAll(contenders);

                Assert.All(results, result => Assert.True(result.IsSuccess, result.ErrorMessage));
                Assert.Single(results.Select(result => result.Value!.ArtifactRef.Uri).Distinct(StringComparer.Ordinal));
                Assert.Single(results.Select(result => result.Value!.Sha256).Distinct(StringComparer.Ordinal));
            }

            Assert.Equal(rounds, PublishedPngFiles(root).Length);
            Assert.Empty(TempFiles(root));
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task StoreAsync_CorruptExistingDestination_FailsClosedWithoutOverwriteOrDeletion()
    {
        var root = CreateTempRoot("concurrent-corrupt-destination");
        try
        {
            var png = BuildRgbaPng(7, 5, blank: false);
            var corrupt = png.ToArray();
            corrupt[^1] ^= 0xff;
            var store = new DesignConversionModelServerArtifactUploadStore(root, maxBytes: 1024 * 1024);
            var request = CreateRequest(png, artifactId: "corrupt-destination", width: 7, height: 5);
            var finalPath = ExpectedPublishedPath(root, request);
            Directory.CreateDirectory(Path.GetDirectoryName(finalPath)!);
            File.WriteAllBytes(finalPath, corrupt);

            var result = await store.StoreAsync(new MemoryStream(png, writable: false), request);

            Assert.False(result.IsSuccess);
            Assert.Equal(DesignConversionModelServerArtifactUploadStore.PublishRaceErrorCode, result.ErrorCode);
            Assert.Equal(corrupt, File.ReadAllBytes(finalPath));
            Assert.Single(PublishedPngFiles(root));
            Assert.Empty(TempFiles(root));
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task StoreAsync_ErrorMessagesDoNotContainRootPathOrPrivateMarker()
    {
        const string privateMarker = "PRIVATE_UPLOAD_ROOT_MARKER";
        var root = CreateTempRoot(privateMarker);
        try
        {
            var png = BuildRgbaPng(4, 3, blank: true);
            var store = new DesignConversionModelServerArtifactUploadStore(root, maxBytes: 1024 * 1024);
            var request = CreateRequest(png, width: 4, height: 3);

            var result = await store.StoreAsync(new MemoryStream(png, writable: false), request);

            Assert.False(result.IsSuccess);
            Assert.DoesNotContain(root, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(privateMarker, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(privateMarker, result.ErrorCode, StringComparison.OrdinalIgnoreCase);
            AssertCleanFailureRoot(root);
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Theory]
    [MemberData(nameof(InvalidNonLocalRootCases))]
    public void Constructor_InvalidNonLocalRoot_RejectsWithoutPathLeak(string artifactRoot)
        => AssertConstructorRejectsWithoutLeak(artifactRoot, "PRIVATE_UPLOAD_ROOT_MARKER");

    [Fact]
    public void Constructor_FileUri_RejectsWithoutPathLeak()
    {
        const string privateMarker = "PRIVATE_UPLOAD_ROOT_MARKER";
        var parent = CreateTempRoot($"file-uri-{privateMarker}");
        try
        {
            var localPath = Path.Combine(parent, privateMarker, "artifacts");
            var fileUri = new Uri(localPath).AbsoluteUri;

            AssertConstructorRejectsWithoutLeak(fileUri, privateMarker);
        }
        finally
        {
            DeleteTempRoot(parent);
        }
    }

    [Fact]
    public void Constructor_ExistingRegularFile_RejectsWithoutPathLeak()
    {
        const string privateMarker = "PRIVATE_UPLOAD_ROOT_MARKER";
        var parent = CreateTempRoot($"regular-file-{privateMarker}");
        try
        {
            var filePath = Path.Combine(parent, $"{privateMarker}.root");
            File.WriteAllText(filePath, "not a directory");

            AssertConstructorRejectsWithoutLeak(filePath, privateMarker);
        }
        finally
        {
            DeleteTempRoot(parent);
        }
    }

    public static IEnumerable<object[]> InvalidOrBlankPngCases()
    {
        yield return ["invalid-upload", new byte[] { 1, 2, 3, 4, 5 }, 1, 1];
        yield return ["blank-upload", BuildRgbaPng(4, 3, blank: true), 4, 3];
    }

    public static IEnumerable<object[]> NonCanonicalSha256Cases()
    {
        yield return ["uppercase-prefix", "SHA256:" + new string('0', 64)];
        yield return ["uppercase-digest", "sha256:" + new string('A', 64)];
        yield return ["missing-prefix", new string('0', 64)];
        yield return ["short-digest", "sha256:" + new string('0', 63)];
        yield return ["long-digest", "sha256:" + new string('0', 65)];
        yield return ["non-hex-digest", "sha256:" + new string('0', 63) + "g"];
        yield return ["leading-space", " sha256:" + new string('0', 64)];
        yield return ["trailing-space", "sha256:" + new string('0', 64) + " "];
    }

    public static IEnumerable<object[]> InvalidNonLocalRootCases()
    {
        yield return [Path.Combine("relative-PRIVATE_UPLOAD_ROOT_MARKER", "artifacts")];
        yield return [@"\\PRIVATE_UPLOAD_ROOT_MARKER\share\artifacts"];
        yield return [@"\\?\C:\PRIVATE_UPLOAD_ROOT_MARKER\artifacts"];
        yield return [@"\\.\C:\PRIVATE_UPLOAD_ROOT_MARKER\artifacts"];
    }

    private static DesignConversionModelServerArtifactRef CreateResolverArtifact(
        string root,
        string artifactId,
        int width,
        int height,
        byte[] png,
        IReadOnlyDictionary<string, string> metadata)
    {
        var path = Path.Combine(root, $"{artifactId}.png");
        File.WriteAllBytes(path, png);
        return new DesignConversionModelServerArtifactRef(
            ArtifactId: artifactId,
            Kind: "screenshot",
            Width: width,
            Height: height,
            Uri: new Uri(path).AbsoluteUri,
            Sha256: Sha256(png),
            Metadata: metadata);
    }

    private static Dictionary<string, string> ResolverProofMetadata(
        int viewportWidth,
        int viewportHeight,
        int pngWidth,
        int pngHeight,
        double deviceScaleFactor)
        => new(StringComparer.Ordinal)
        {
            ["media_type"] = "image/png",
            ["content_type"] = "image/png",
            ["viewport_width"] = viewportWidth.ToString(CultureInfo.InvariantCulture),
            ["viewport_height"] = viewportHeight.ToString(CultureInfo.InvariantCulture),
            ["png_width"] = pngWidth.ToString(CultureInfo.InvariantCulture),
            ["png_height"] = pngHeight.ToString(CultureInfo.InvariantCulture),
            [DesignConversionRenderResolutionFactory.DeviceScaleFactorMetadataKey] = deviceScaleFactor.ToString("R", CultureInfo.InvariantCulture)
        };

    private static DesignConversionModelServerArtifactUploadRequest CreateRequest(
        byte[] bytes,
        string artifactId = "valid-upload",
        string kind = "screenshot",
        int width = 4,
        int height = 3,
        string? expectedSha256 = null)
        => new(
            ArtifactId: artifactId,
            Kind: kind,
            MediaType: "image/png",
            ExpectedSha256: expectedSha256 ?? Sha256(bytes),
            Width: width,
            Height: height);

    private static string ExpectedPublishedPath(
        string root,
        DesignConversionModelServerArtifactUploadRequest request)
        => Path.Combine(
            root,
            "ingress",
            request.Kind,
            request.ArtifactId,
            $"{request.Width.ToString(CultureInfo.InvariantCulture)}x" +
            $"{request.Height.ToString(CultureInfo.InvariantCulture)}-" +
            $"{request.ExpectedSha256["sha256:".Length..]}.png");

    private static string CreateTempRoot(string name)
    {
        var safeName = new string(name.Select(ch => char.IsAsciiLetterOrDigit(ch) || ch is '-' or '_' ? ch : '-').ToArray());
        var root = Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "design-conversion-upload-store-tests",
            $"{safeName}-{Guid.NewGuid():N}"));
        Directory.CreateDirectory(root);
        return root;
    }

    private static void DeleteTempRoot(string root)
    {
        if (!Directory.Exists(root))
            return;

        var fullRoot = Path.GetFullPath(root);
        var allowedRoot = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "design-conversion-upload-store-tests"));
        if (!fullRoot.StartsWith(allowedRoot, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
            return;

        Directory.Delete(fullRoot, recursive: true);
    }

    private static void AssertCleanFailureRoot(string root)
    {
        Assert.Empty(PublishedPngFiles(root));
        Assert.Empty(TempFiles(root));
    }

    private static void AssertConstructorRejectsWithoutLeak(string artifactRoot, string privateMarker)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new DesignConversionModelServerArtifactUploadStore(artifactRoot));
        var diagnostic = exception.ToString();

        Assert.DoesNotContain(artifactRoot, diagnostic, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(privateMarker, diagnostic, StringComparison.OrdinalIgnoreCase);
        Assert.Null(exception.InnerException);
    }

    private static string[] PublishedPngFiles(string root)
    {
        var ingress = Path.Combine(root, "ingress");
        return Directory.Exists(ingress)
            ? Directory.GetFiles(ingress, "*.png", SearchOption.AllDirectories)
            : [];
    }

    private static string[] TempFiles(string root)
        => Directory.Exists(root)
            ? Directory.GetFiles(root, "*.tmp", SearchOption.AllDirectories)
            : [];

    private static byte[] BuildRgbaPng(int width, int height, bool blank)
    {
        var raw = new byte[checked(height * ((width * 4) + 1))];
        for (var y = 0; y < height; y++)
        {
            var row = y * ((width * 4) + 1);
            raw[row] = 0;
            for (var x = 0; x < width; x++)
            {
                var offset = row + 1 + (x * 4);
                if (blank)
                {
                    raw[offset] = 255;
                    raw[offset + 1] = 255;
                    raw[offset + 2] = 255;
                    raw[offset + 3] = 255;
                    continue;
                }

                raw[offset] = (byte)(20 + ((x * 31) % 180));
                raw[offset + 1] = (byte)(40 + ((y * 47) % 160));
                raw[offset + 2] = (byte)(90 + (((x + y) * 29) % 150));
                raw[offset + 3] = 255;
            }
        }

        using var png = new MemoryStream();
        png.Write([137, 80, 78, 71, 13, 10, 26, 10]);
        WritePngChunk(png, "IHDR", BuildPngIhdr(width, height));
        WritePngChunk(png, "IDAT", CompressPngScanlines(raw));
        WritePngChunk(png, "IEND", []);
        return png.ToArray();
    }

    private static byte[] CorruptStoredPngChunkCrc(byte[] png, string chunkType)
    {
        var corrupt = png.ToArray();
        var offset = 8;
        while ((long)offset + 12L <= corrupt.Length)
        {
            var length = BinaryPrimitives.ReadInt32BigEndian(corrupt.AsSpan(offset, 4));
            var chunkEnd = (long)offset + 12L + length;
            if (length < 0 || chunkEnd > corrupt.Length)
                break;

            var type = Encoding.ASCII.GetString(corrupt, offset + 4, 4);
            if (string.Equals(type, chunkType, StringComparison.Ordinal))
            {
                corrupt[offset + 8 + length] ^= 0x01;
                return corrupt;
            }

            offset = (int)chunkEnd;
        }

        throw new InvalidOperationException("Requested PNG chunk was not found in the upload fixture.");
    }

    private static byte[] BuildPngIhdr(int width, int height)
    {
        var data = new byte[13];
        BinaryPrimitives.WriteInt32BigEndian(data.AsSpan(0, 4), width);
        BinaryPrimitives.WriteInt32BigEndian(data.AsSpan(4, 4), height);
        data[8] = 8;
        data[9] = 6;
        return data;
    }

    private static byte[] CompressPngScanlines(byte[] raw)
    {
        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.Fastest, leaveOpen: true))
            zlib.Write(raw);
        return compressed.ToArray();
    }

    private static void WritePngChunk(Stream stream, string type, byte[] data)
    {
        var typeBytes = Encoding.ASCII.GetBytes(type);
        Span<byte> lengthBytes = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(lengthBytes, data.Length);
        stream.Write(lengthBytes);
        stream.Write(typeBytes);
        stream.Write(data);

        var crcInput = new byte[typeBytes.Length + data.Length];
        Array.Copy(typeBytes, 0, crcInput, 0, typeBytes.Length);
        Array.Copy(data, 0, crcInput, typeBytes.Length, data.Length);
        Span<byte> crcBytes = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(crcBytes, Crc32(crcInput));
        stream.Write(crcBytes);
    }

    private static uint Crc32(ReadOnlySpan<byte> bytes)
    {
        var crc = 0xffffffffu;
        foreach (var b in bytes)
        {
            crc ^= b;
            for (var bit = 0; bit < 8; bit++)
                crc = (crc & 1) == 1 ? (crc >> 1) ^ 0xedb88320u : crc >> 1;
        }

        return ~crc;
    }

    private static string Sha256(byte[] bytes)
        => "sha256:" + Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private sealed class CountingReadStream(byte[] bytes) : Stream
    {
        private int _offset;

        public long BytesRead { get; private set; }
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => bytes.Length;
        public override long Position
        {
            get => _offset;
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var read = Math.Min(count, bytes.Length - _offset);
            if (read <= 0)
                return 0;

            Array.Copy(bytes, _offset, buffer, offset, read);
            _offset += read;
            BytesRead += read;
            return read;
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                return ValueTask.FromCanceled<int>(cancellationToken);

            var read = Math.Min(buffer.Length, bytes.Length - _offset);
            if (read <= 0)
                return ValueTask.FromResult(0);

            bytes.AsSpan(_offset, read).CopyTo(buffer.Span);
            _offset += read;
            BytesRead += read;
            return ValueTask.FromResult(read);
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin)
            => throw new NotSupportedException();

        public override void SetLength(long value)
            => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();
    }

    private sealed class CancellingReadStream(byte[] bytes, CancellationTokenSource cancellation) : Stream
    {
        private int _offset;
        private bool _cancelled;

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => bytes.Length;
        public override long Position
        {
            get => _offset;
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (_cancelled || cancellationToken.IsCancellationRequested)
                return ValueTask.FromException<int>(new OperationCanceledException(cancellationToken));

            var read = Math.Min(Math.Min(buffer.Length, 8), bytes.Length - _offset);
            if (read <= 0)
                return ValueTask.FromResult(0);

            bytes.AsSpan(_offset, read).CopyTo(buffer.Span);
            _offset += read;
            _cancelled = true;
            cancellation.Cancel();
            return ValueTask.FromResult(read);
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin)
            => throw new NotSupportedException();

        public override void SetLength(long value)
            => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();
    }
}
