// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DesignConversionDomain;
using ModelServer.DesignConversion;

public sealed class DesignConversionModelServerPromotionHttpUploaderTests
{
    private static readonly Uri Endpoint = new("https://promotion.example/v1/design-conversion/promotions");
    private static readonly DateTimeOffset UploadedAt = DateTimeOffset.Parse("2026-07-14T08:30:00Z");
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
    private const string TenantId = "tenant-alpha";
    private const string Bearer = "auth-value-123456789";

    [Fact]
    public void UploaderOptions_ToStringRedactsBearerTokenAndPreservesCopyWith()
    {
        var originalRootSegment = $"uploader-options-private-root-{Guid.NewGuid():N}";
        var originalNestedSegment = $"uploader-options-private-nested-{Guid.NewGuid():N}";
        var copiedRootSegment = $"uploader-options-copy-root-{Guid.NewGuid():N}";
        var copiedNestedSegment = $"uploader-options-copy-nested-{Guid.NewGuid():N}";
        var root = Path.Combine(Path.GetTempPath(), originalRootSegment, originalNestedSegment, "artifacts");
        var copiedRoot = Path.Combine(Path.GetTempPath(), copiedRootSegment, copiedNestedSegment, "artifacts");
        var sentinelBearer = $"uploader-options-secret-bearer-{Guid.NewGuid():N}";
        var options = new DesignConversionModelServerPromotionHttpUploaderOptions(
            Endpoint,
            TenantId,
            sentinelBearer,
            root)
        {
            Timeout = TimeSpan.FromSeconds(7),
            MaxArtifactBytes = 1024,
            MaxTotalArtifactBytes = 4096,
            MaxResponseBodyBytes = 8192,
            MaxArtifactCount = DesignConversionModelServerPromotionHttpUploaderOptions.HardMaxArtifactCount
        };
        var copied = options with
        {
            AllowedArtifactRoot = copiedRoot,
            Timeout = TimeSpan.FromSeconds(8)
        };

        Assert.Equal(root, options.AllowedArtifactRoot);
        Assert.Equal(copiedRoot, copied.AllowedArtifactRoot);
        Assert.Equal(TimeSpan.FromSeconds(7), options.Timeout);
        Assert.Equal(TimeSpan.FromSeconds(8), copied.Timeout);
        Assert.Equal(sentinelBearer, copied.BearerToken);
        foreach (var (surface, fullRoot, rootSegments) in new[]
        {
            (options.ToString(), root, new[] { originalRootSegment, originalNestedSegment }),
            (copied.ToString(), copiedRoot, new[] { copiedRootSegment, copiedNestedSegment })
        })
        {
            Assert.Contains(Endpoint.AbsoluteUri, surface, StringComparison.Ordinal);
            Assert.Contains(TenantId, surface, StringComparison.Ordinal);
            Assert.Contains(nameof(DesignConversionModelServerPromotionHttpUploaderOptions.Endpoint), surface, StringComparison.Ordinal);
            Assert.Contains(nameof(DesignConversionModelServerPromotionHttpUploaderOptions.TenantId), surface, StringComparison.Ordinal);
            Assert.Contains("BearerToken = [REDACTED]", surface, StringComparison.Ordinal);
            Assert.Contains("AllowedArtifactRoot = [REDACTED]", surface, StringComparison.Ordinal);
            Assert.Contains("[REDACTED]", surface, StringComparison.Ordinal);
            Assert.DoesNotContain(sentinelBearer, surface, StringComparison.Ordinal);
            Assert.DoesNotContain(fullRoot, surface, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(fullRoot.Replace('\\', '/'), surface, StringComparison.OrdinalIgnoreCase);
            foreach (var rootSegment in rootSegments)
                Assert.DoesNotContain(rootSegment, surface, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task UploadAsync_SuccessSendsMultipartArtifactsAndMapsVerifiedReceipt()
    {
        var fixture = CreateFixture("success");
        try
        {
            var handler = new CapturingPromotionUploadHandler(_ => JsonResponse(
                statusCode: HttpStatusCode.Created,
                body: SuccessResponse(fixture, "remote-upload-1")));
            var uploader = CreateUploader(fixture.Root, handler);

            var result = await uploader.UploadAsync(fixture.Manifest, fixture.GateReport);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal(1, handler.SendCount);
            Assert.Equal("POST", handler.Method);
            Assert.Equal("/v1/design-conversion/promotions", handler.RequestUri!.AbsolutePath);
            Assert.Equal($"Bearer {Bearer}", Assert.Single(handler.HeaderValues("Authorization")));
            Assert.Equal(TenantId, Assert.Single(handler.HeaderValues(DesignConversionModelServerPromotionHttpUploader.TenantHeaderName)));
            Assert.Equal(fixture.ExpectedIdempotencyKey, Assert.Single(handler.HeaderValues(DesignConversionModelServerPromotionHttpUploader.IdempotencyHeaderName)));
            Assert.Equal(
                DesignConversionModelServerPromotionHttpUploader.ContractVersion,
                Assert.Single(handler.HeaderValues(DesignConversionModelServerPromotionHttpUploader.ContractHeaderName)));
            Assert.Contains(
                "multipart/form-data",
                Assert.Single(handler.HeaderValues("Content-Type")),
                StringComparison.OrdinalIgnoreCase);

            var body = handler.BodyText;
            Assert.Contains("name=metadata", body, StringComparison.Ordinal);
            Assert.Contains("\"manifestId\":\"success-manifest\"", body, StringComparison.Ordinal);
            Assert.Contains("\"idempotencyKey\":\"" + fixture.ExpectedIdempotencyKey + "\"", body, StringComparison.Ordinal);
            foreach (var artifact in fixture.Artifacts.Values)
            {
                Assert.Contains($"name={artifact.Role}", body, StringComparison.Ordinal);
                Assert.Contains(artifact.WireFileName, body, StringComparison.Ordinal);
                Assert.Contains(artifact.ContentText, body, StringComparison.Ordinal);
                Assert.Contains(artifact.Sha256, body, StringComparison.Ordinal);
                Assert.Contains($"\"length\":{artifact.Length}", body, StringComparison.Ordinal);
            }

            AssertNoLocalPathsOnWire(handler, fixture);

            var receipt = result.Value!;
            Assert.Equal("remote-upload-1", receipt.UploadId);
            Assert.Equal("https://promotion.example/receipts/remote-upload-1", receipt.RemoteRef);
            Assert.Equal(UploadedAt, receipt.UploadedAtUtc);
            Assert.Equal(DesignConversionPromotionPublicationKind.VerifiedModelServerUpload, receipt.PublicationKind);
            Assert.True(receipt.UploadAttempted);
            Assert.True(receipt.UploadSucceeded);
            Assert.True(receipt.VerificationSucceeded);
            Assert.Equal(DesignConversionPromotionPublicationStatus.UploadedVerified, receipt.PublicationStatus);
            Assert.StartsWith("upload-http:", receipt.ProvenanceRef, StringComparison.Ordinal);
            Assert.DoesNotContain(Bearer, BuildPublicReceiptSurface(receipt), StringComparison.Ordinal);
            AssertNoLocalPathsInReceipt(receipt, fixture);
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    [Fact]
    public async Task UploadAsync_SourcePathReplacementDuringSendCannotChangeHashedBytes()
    {
        var fixture = CreateFixture("stable-handle");
        var checkpoint = fixture.Artifacts["checkpoint"];
        var replacementText = "replacement bytes that were not hashed";
        var replacementPath = Path.Combine(fixture.Root, "checkpoint-replacement.dat");
        File.WriteAllBytes(replacementPath, Utf8NoBom.GetBytes(replacementText));
        var replacementAttempted = false;
        var replacementSucceeded = false;
        Exception? replacementFailure = null;

        try
        {
            var handler = new CapturingPromotionUploadHandler(
                _ => JsonResponse(
                    HttpStatusCode.Created,
                    SuccessResponse(fixture, "remote-upload-stable-handle")),
                beforeContentRead: () =>
                {
                    replacementAttempted = true;
                    try
                    {
                        File.Move(replacementPath, checkpoint.FullPath, overwrite: true);
                        replacementSucceeded = true;
                    }
                    catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                    {
                        replacementFailure = ex;
                    }
                });
            var uploader = CreateUploader(fixture.Root, handler);

            var result = await uploader.UploadAsync(fixture.Manifest, fixture.GateReport);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.True(replacementAttempted);
            Assert.True(
                replacementSucceeded || replacementFailure is IOException or UnauthorizedAccessException,
                "The replacement race must either replace the path or be blocked by the open artifact handle.");
            Assert.Equal(1, handler.SendCount);
            Assert.Contains(checkpoint.ContentText, handler.BodyText, StringComparison.Ordinal);
            Assert.Contains(checkpoint.Sha256, handler.BodyText, StringComparison.Ordinal);
            Assert.DoesNotContain(replacementText, handler.BodyText, StringComparison.Ordinal);
            if (replacementSucceeded)
            {
                Assert.Equal(replacementText, File.ReadAllText(checkpoint.FullPath, Utf8NoBom));
            }
            else
            {
                Assert.NotNull(replacementFailure);
                Assert.Contains(checkpoint.ContentText, File.ReadAllText(checkpoint.FullPath, Utf8NoBom), StringComparison.Ordinal);
            }

            Assert.Equal(
                fixture.ExpectedIdempotencyKey,
                Assert.Single(handler.HeaderValues(DesignConversionModelServerPromotionHttpUploader.IdempotencyHeaderName)));

            var disposalProbe = Path.Combine(fixture.Root, "checkpoint-disposal-probe.dat");
            File.WriteAllBytes(disposalProbe, Utf8NoBom.GetBytes("handle released after upload"));
            File.Move(disposalProbe, checkpoint.FullPath, overwrite: true);
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    [Fact]
    public async Task UploadAsync_NonHttpsEndpointIsRejectedBeforeSend()
    {
        var fixture = CreateFixture("non-https");
        try
        {
            var handler = new CapturingPromotionUploadHandler(_ => JsonResponse(HttpStatusCode.Created, "{}"));
            var uploader = CreateUploader(
                fixture.Root,
                handler,
                endpoint: new Uri("http://promotion.example/v1/design-conversion/promotions"));

            var result = await uploader.UploadAsync(fixture.Manifest, fixture.GateReport);

            Assert.False(result.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_PROMOTION_UPLOAD_ENDPOINT_HTTPS_REQUIRED", result.ErrorCode);
            Assert.Equal(0, handler.SendCount);
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    [Theory]
    [InlineData("non-2xx", "DESIGN_CONVERSION_PROMOTION_UPLOAD_HTTP_NON_SUCCESS")]
    [InlineData("malformed-json", "DESIGN_CONVERSION_PROMOTION_UPLOAD_RESPONSE_MALFORMED")]
    [InlineData("identity-mismatch", "DESIGN_CONVERSION_PROMOTION_UPLOAD_RESPONSE_IDENTITY_MISMATCH")]
    [InlineData("oversized-response", "DESIGN_CONVERSION_PROMOTION_UPLOAD_RESPONSE_TOO_LARGE")]
    public async Task UploadAsync_ResponseFailuresFailClosed(string scenario, string expectedErrorCode)
    {
        var fixture = CreateFixture($"response-{scenario}");
        try
        {
            var handler = new CapturingPromotionUploadHandler(request => scenario switch
            {
                "non-2xx" => JsonResponse(
                    HttpStatusCode.InternalServerError,
                    """{"rawResponse":"PRIVATE_REMOTE_BODY_SHOULD_NOT_LEAK"}"""),
                "malformed-json" => JsonResponse(HttpStatusCode.OK, "{ not-json"),
                "identity-mismatch" => JsonResponse(
                    HttpStatusCode.OK,
                    SuccessResponse(fixture, "remote-upload-2", tenantId: "tenant-other")),
                "oversized-response" => JsonResponse(HttpStatusCode.OK, new string('x', 256)),
                _ => throw new InvalidOperationException(request.RequestUri?.ToString())
            });
            var uploader = CreateUploader(
                fixture.Root,
                handler,
                maxResponseBodyBytes: scenario == "oversized-response" ? 32 : 64 * 1024);

            var result = await uploader.UploadAsync(fixture.Manifest, fixture.GateReport);

            Assert.False(result.IsSuccess);
            Assert.Equal(1, handler.SendCount);
            Assert.DoesNotContain("PRIVATE_REMOTE_BODY_SHOULD_NOT_LEAK", result.ErrorMessage, StringComparison.Ordinal);
            Assert.DoesNotContain(Bearer, result.ErrorMessage, StringComparison.Ordinal);
            Assert.Equal(expectedErrorCode, result.ErrorCode);
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    [Fact]
    public async Task UploadAsync_DuplicateReceiptArtifactRolesFailClosedAfterOneSend()
    {
        var fixture = CreateFixture("duplicate-receipt-role");
        try
        {
            var handler = new CapturingPromotionUploadHandler(_ => JsonResponse(
                HttpStatusCode.OK,
                DuplicateCheckpointArtifactsResponse(fixture, "remote-upload-duplicate-role")));
            var uploader = CreateUploader(fixture.Root, handler);

            var result = await uploader.UploadAsync(fixture.Manifest, fixture.GateReport);

            Assert.False(result.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_PROMOTION_UPLOAD_RESPONSE_ARTIFACT_MISMATCH", result.ErrorCode);
            Assert.Equal(1, handler.SendCount);

            File.Delete(fixture.Artifacts["checkpoint"].FullPath);
            Assert.False(File.Exists(fixture.Artifacts["checkpoint"].FullPath));
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    [Fact]
    public async Task UploadAsync_SendTimeoutFailsClosed()
    {
        var fixture = CreateFixture("timeout");
        try
        {
            var handler = new DelayingPromotionUploadHandler(TimeSpan.FromSeconds(10));
            var uploader = CreateUploader(fixture.Root, handler, timeout: TimeSpan.FromMilliseconds(25));

            var result = await uploader.UploadAsync(fixture.Manifest, fixture.GateReport);

            Assert.False(result.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_PROMOTION_UPLOAD_TIMEOUT", result.ErrorCode);
            Assert.Equal(1, handler.SendCount);
            Assert.True(handler.ObservedTokenCanBeCanceled);
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    [Fact]
    public async Task UploadAsync_ResponseStreamCancellationIsHonored()
    {
        var fixture = CreateFixture("cancel-read");
        try
        {
            var responseReadStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var handler = new CapturingPromotionUploadHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(new DelayingReadStream(
                    TimeSpan.FromSeconds(10),
                    () => responseReadStarted.TrySetResult(true)))
            });
            var uploader = CreateUploader(fixture.Root, handler, timeout: TimeSpan.FromSeconds(30));
            using var cancellation = new CancellationTokenSource();

            var upload = uploader.UploadAsync(fixture.Manifest, fixture.GateReport, cancellation.Token);
            await responseReadStarted.Task.WaitAsync(TimeSpan.FromSeconds(30));
            cancellation.Cancel();
            var result = await upload.WaitAsync(TimeSpan.FromSeconds(30));

            Assert.False(result.IsSuccess);
            Assert.Equal("CANCELLED", result.ErrorCode);
            Assert.Equal(1, handler.SendCount);
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    [Fact]
    public async Task UploadAsync_PathBoundaryFailuresAndOpenedSymlinkTargetEscapeAreRejectedBeforeSend()
    {
        var fixture = CreateFixture("path-boundary");
        var outside = Path.Combine(fixture.TempRoot, "outside");
        Directory.CreateDirectory(outside);
        var outsideCheckpoint = Path.Combine(outside, "outside-checkpoint.bin");
        await File.WriteAllTextAsync(outsideCheckpoint, "outside checkpoint secret", Encoding.UTF8);

        try
        {
            await AssertRejectedBeforeSendAsync(
                fixture with
                {
                    Manifest = fixture.Manifest with
                    {
                        PromotionArtifacts = fixture.Manifest.PromotionArtifacts! with
                        {
                            CheckpointPath = Path.Combine(fixture.Root, "..", "outside", "outside-checkpoint.bin")
                        }
                    }
                },
                "DESIGN_CONVERSION_PROMOTION_UPLOAD_ARTIFACT_OUT_OF_ROOT");

            await AssertRejectedBeforeSendAsync(
                fixture,
                "DESIGN_CONVERSION_PROMOTION_UPLOAD_ARTIFACT_OVERSIZE",
                maxArtifactBytes: fixture.Artifacts["checkpoint"].Length - 1);

            var linkPath = Path.Combine(fixture.Root, "linked-checkpoint.bin");
            var linkCreated = false;
            Exception? linkFailure = null;
            try
            {
                File.CreateSymbolicLink(linkPath, outsideCheckpoint);
                linkCreated = true;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or PlatformNotSupportedException or NotSupportedException)
            {
                linkFailure = ex;
            }

            Assert.True(
                linkCreated,
                $"The promotion uploader reparse escape test requires a local file symbolic link. " +
                $"Creation failed with {linkFailure?.GetType().Name}: {linkFailure?.Message}");

            var symlinkFixture = fixture with
            {
                Manifest = fixture.Manifest with
                {
                    PromotionArtifacts = fixture.Manifest.PromotionArtifacts! with
                    {
                        CheckpointPath = linkPath
                    }
                }
            };
            var handler = new CapturingPromotionUploadHandler(_ => JsonResponse(HttpStatusCode.Created, "{}"));
            var uploader = CreateUploader(symlinkFixture.Root, handler);

            var result = await uploader.UploadAsync(symlinkFixture.Manifest, symlinkFixture.GateReport);

            Assert.False(result.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_PROMOTION_UPLOAD_ARTIFACT_OPENED_TARGET_ESCAPE", result.ErrorCode);
            Assert.Equal(0, handler.SendCount);
            Assert.DoesNotContain(outsideCheckpoint, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(fixture.Root, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(Bearer, result.ErrorMessage, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    [Fact]
    public async Task UploadAsync_ArtifactOpenRaceReturnsTypedFailureAndDisposesPreparedHandles()
    {
        var fixture = CreateFixture("artifact-open-race");
        try
        {
            using var tokenizerLock = new FileStream(
                fixture.Artifacts["tokenizer"].FullPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.None);
            var handler = new CapturingPromotionUploadHandler(_ => JsonResponse(HttpStatusCode.Created, "{}"));
            var uploader = CreateUploader(fixture.Root, handler);

            var result = await uploader.UploadAsync(fixture.Manifest, fixture.GateReport);

            Assert.False(result.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_PROMOTION_UPLOAD_ARTIFACT_OPEN_FAILED", result.ErrorCode);
            Assert.Equal(0, handler.SendCount);
            Assert.DoesNotContain(fixture.Root, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(Bearer, result.ErrorMessage, StringComparison.Ordinal);

            File.WriteAllBytes(
                fixture.Artifacts["checkpoint"].FullPath,
                Utf8NoBom.GetBytes("prior prepared handle was disposed"));
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    [Theory]
    [InlineData(double.NaN, 0.9)]
    [InlineData(double.PositiveInfinity, 0.9)]
    [InlineData(double.NegativeInfinity, 0.9)]
    [InlineData(0.93, double.NaN)]
    [InlineData(0.93, double.PositiveInfinity)]
    [InlineData(0.93, double.NegativeInfinity)]
    [InlineData(-0.01, 0.9)]
    [InlineData(0.93, -0.01)]
    public async Task UploadAsync_InvalidGateScoreMetadataIsRejectedBeforeSendWithoutThrowing(
        double heldOutScore,
        double requiredHeldOutScore)
    {
        var fixture = CreateFixture("invalid-gate-score");
        try
        {
            var handler = new CapturingPromotionUploadHandler(_ => JsonResponse(HttpStatusCode.Created, "{}"));
            var uploader = CreateUploader(fixture.Root, handler);

            var result = await uploader.UploadAsync(
                fixture.Manifest,
                fixture.GateReport with
                {
                    HeldOutScore = heldOutScore,
                    RequiredHeldOutScore = requiredHeldOutScore
                });

            Assert.False(result.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_PROMOTION_UPLOAD_METADATA_INVALID", result.ErrorCode);
            Assert.Equal(0, handler.SendCount);
            Assert.DoesNotContain(Bearer, result.ErrorMessage, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    [Theory]
    [InlineData(-1, 1)]
    [InlineData(7, -1)]
    public async Task UploadAsync_NegativeManifestRecordCountsAreRejectedBeforeSend(
        int acceptedRecordCount,
        int quarantinedRecordCount)
    {
        var fixture = CreateFixture("negative-record-counts");
        try
        {
            var handler = new CapturingPromotionUploadHandler(_ => JsonResponse(HttpStatusCode.Created, "{}"));
            var uploader = CreateUploader(fixture.Root, handler);

            var result = await uploader.UploadAsync(
                fixture.Manifest with
                {
                    AcceptedRecordCount = acceptedRecordCount,
                    QuarantinedRecordCount = quarantinedRecordCount
                },
                fixture.GateReport);

            Assert.False(result.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_PROMOTION_UPLOAD_METADATA_INVALID", result.ErrorCode);
            Assert.Equal(0, handler.SendCount);
            Assert.DoesNotContain(Bearer, result.ErrorMessage, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    [Theory]
    [InlineData("https://promotion.example/receipts/remote-upload?")]
    [InlineData("https://promotion.example/receipts/remote-upload#")]
    [InlineData("https://user@promotion.example/receipts/remote-upload")]
    [InlineData("https://@promotion.example/receipts/remote-upload")]
    [InlineData("https://user%40promotion.example/receipts/remote-upload")]
    [InlineData("https://%40promotion.example/receipts/remote-upload")]
    [InlineData("https://promotion.example/receipts/%0Aremote-upload")]
    [InlineData("https://promotion.example/receipts/%250Aremote-upload")]
    [InlineData("https://promotion.example/receipts/%2Fremote-upload")]
    [InlineData("https://promotion.example/receipts/%252Fremote-upload")]
    [InlineData("https://promotion.example/receipts/%5Cremote-upload")]
    [InlineData("https://promotion.example/receipts/%255Cremote-upload")]
    [InlineData("https://promotion.example/receipts/../remote-upload")]
    [InlineData("https://promotion.example/receipts/%2E%2E/remote-upload")]
    [InlineData("https://promotion.example/receipts/%")]
    [InlineData("https://promotion.example/receipts/white space")]
    [InlineData("https://promotion.example/receipts\\remote-upload")]
    public async Task UploadAsync_UnsafeRemoteReceiptRefsFailCoherenceAfterOneSendWithoutLeakingRawRef(
        string unsafeRemoteRef)
    {
        var fixture = CreateFixture("unsafe-remote-ref");
        try
        {
            var handler = new CapturingPromotionUploadHandler(_ => JsonResponse(
                HttpStatusCode.OK,
                SuccessResponse(fixture, "remote-upload-unsafe-ref", remoteRef: unsafeRemoteRef)));
            var uploader = CreateUploader(fixture.Root, handler);

            var result = await uploader.UploadAsync(fixture.Manifest, fixture.GateReport);

            Assert.False(result.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_PROMOTION_UPLOAD_RESPONSE_REMOTE_REF_INVALID", result.ErrorCode);
            Assert.Equal(1, handler.SendCount);
            Assert.DoesNotContain(unsafeRemoteRef, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(Bearer, result.ErrorMessage, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    [Fact]
    public async Task UploadAsync_SafeNoncanonicalRemoteReceiptRefIsCanonicalizedInReceiptAndProvenance()
    {
        var fixture = CreateFixture("safe-noncanonical-remote-ref");
        const string uploadId = "remote-upload-safe-noncanonical";
        const string rawRemoteRef = "HTTPS://PROMOTION.EXAMPLE:443/receipts/remote-upload-safe-noncanonical";
        const string expectedCanonicalRef = "https://promotion.example/receipts/remote-upload-safe-noncanonical";

        try
        {
            var handler = new CapturingPromotionUploadHandler(_ => JsonResponse(
                HttpStatusCode.Created,
                SuccessResponse(fixture, uploadId, remoteRef: rawRemoteRef)));
            var uploader = CreateUploader(fixture.Root, handler);

            var result = await uploader.UploadAsync(fixture.Manifest, fixture.GateReport);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal(1, handler.SendCount);

            var receipt = result.Value!;
            Assert.Equal(expectedCanonicalRef, receipt.RemoteRef);
            Assert.Equal(
                BuildExpectedProvenanceRef(Endpoint, uploadId, expectedCanonicalRef, fixture.ExpectedIdempotencyKey),
                receipt.ProvenanceRef);
            Assert.DoesNotContain("PROMOTION.EXAMPLE:443", BuildPublicReceiptSurface(receipt), StringComparison.Ordinal);
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    [Fact]
    public async Task UploadAsync_UnboundedArtifactConfigurationIsRejectedBeforeSend()
    {
        var fixture = CreateFixture("unbounded-config");
        try
        {
            var handler = new CapturingPromotionUploadHandler(_ => JsonResponse(HttpStatusCode.Created, "{}"));
            var uploader = CreateUploader(
                fixture.Root,
                handler,
                maxArtifactBytes: long.MaxValue,
                maxTotalArtifactBytes: long.MaxValue,
                maxArtifactCount: int.MaxValue);

            var result = await uploader.UploadAsync(fixture.Manifest, fixture.GateReport);

            Assert.False(result.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_PROMOTION_UPLOAD_ARTIFACT_LIMIT_INVALID", result.ErrorCode);
            Assert.Equal(0, handler.SendCount);
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    private static async Task AssertRejectedBeforeSendAsync(
        PromotionFixture fixture,
        string expectedCode,
        long? maxArtifactBytes = null)
    {
        var handler = new CapturingPromotionUploadHandler(_ => JsonResponse(HttpStatusCode.Created, "{}"));
        var uploader = CreateUploader(fixture.Root, handler, maxArtifactBytes: maxArtifactBytes);

        var result = await uploader.UploadAsync(fixture.Manifest, fixture.GateReport);

        Assert.False(result.IsSuccess);
        Assert.Equal(expectedCode, result.ErrorCode);
        Assert.Equal(0, handler.SendCount);
    }

    private static DesignConversionModelServerPromotionHttpUploader CreateUploader(
        string root,
        HttpMessageHandler handler,
        Uri? endpoint = null,
        long? maxResponseBodyBytes = null,
        long? maxArtifactBytes = null,
        long? maxTotalArtifactBytes = null,
        int? maxArtifactCount = null,
        TimeSpan? timeout = null)
        => new(
            new HttpClient(handler, disposeHandler: true),
            new DesignConversionModelServerPromotionHttpUploaderOptions(
                endpoint ?? Endpoint,
                TenantId,
                Bearer,
                root)
            {
                Timeout = timeout ?? TimeSpan.FromSeconds(5),
                MaxArtifactBytes = maxArtifactBytes ?? 1024 * 1024,
                MaxTotalArtifactBytes = maxTotalArtifactBytes ?? 3 * 1024 * 1024,
                MaxResponseBodyBytes = maxResponseBodyBytes ?? 64 * 1024,
                MaxArtifactCount = maxArtifactCount ?? 3
            });

    private static PromotionFixture CreateFixture(string name)
    {
        var tempRoot = Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "llm_mvp_core",
            "promotion-http-uploader-tests",
            Guid.NewGuid().ToString("N")));
        var root = Path.Combine(tempRoot, "artifacts");
        Directory.CreateDirectory(root);

        var artifacts = new[]
        {
            WriteArtifact(root, "checkpoint", "checkpoint.bin", $"{name} checkpoint artifact payload"),
            WriteArtifact(root, "tokenizer", "tokenizer.json", $"{name} tokenizer artifact payload"),
            WriteArtifact(root, "policy", "policy.json", $"{name} policy artifact payload")
        }.ToDictionary(artifact => artifact.Role, StringComparer.Ordinal);

        var manifest = new DesignConversionTrainingManifest(
            ManifestId: $"{name}-manifest",
            MachineId: $"{name}-machine",
            DomainTag: "design-conversion",
            Version: "v1",
            CheckpointRef: $"{name}-checkpoint-ref",
            ManifestRef: $"manifest://design-conversion/{name}",
            CreatedAtUtc: DateTimeOffset.Parse("2026-07-14T08:00:00Z"),
            AcceptedRecordCount: 7,
            QuarantinedRecordCount: 1,
            UnitIds: ["critic-unit", "improver-unit"],
            Batches:
            [
                new DesignConversionTrainingManifestBatch(
                    BatchId: $"{name}-batch",
                    UnitId: "critic-unit",
                    RecordCount: 7,
                    RecordIds: ["record-1", "record-2"],
                    SourceIds: ["source-1"])
            ],
            EvidenceRefs: ["manifest-safe-evidence"],
            ModelServerUploadAttempted: false,
            ModelServerPromotionClaimed: false,
            DeterministicLogicClaimedAsLearning: false)
        {
            PromotionArtifacts = new DesignConversionTrainingManifestArtifacts(
                artifacts["checkpoint"].FullPath,
                artifacts["tokenizer"].FullPath,
                artifacts["policy"].FullPath)
        };

        var gateReport = new DesignConversionPromotionGateReport(
            HeldOutScore: 0.93,
            RequiredHeldOutScore: 0.9,
            HeldOutSourceDisjoint: true,
            DeterministicShortcutDetected: false,
            HiddenShortcutEvidenceDetected: false,
            FreshLoadPassed: true,
            BudgetTierPolicyPassed: true,
            BudgetTier: "bounded-test-tier",
            DpoQuarantineCount: 0,
            MaxDpoQuarantineCount: 1,
            RequiredArtifactPaths: artifacts.Values.Select(artifact => artifact.FullPath).ToArray(),
            PresentArtifactPaths: artifacts.Values.Select(artifact => artifact.FullPath).ToArray(),
            EvidenceRefs: ["gate-safe-evidence"]);

        return new PromotionFixture(
            TempRoot: tempRoot,
            Root: root,
            Manifest: manifest,
            GateReport: gateReport,
            Artifacts: artifacts,
            ExpectedIdempotencyKey: BuildExpectedIdempotencyKey(manifest, artifacts));
    }

    private static ArtifactExpectation WriteArtifact(
        string root,
        string role,
        string wireFileName,
        string content)
    {
        var fullPath = Path.Combine(root, $"{role}-source.dat");
        var bytes = Utf8NoBom.GetBytes(content);
        File.WriteAllBytes(fullPath, bytes);
        return new ArtifactExpectation(
            role,
            wireFileName,
            fullPath,
            content,
            bytes.Length,
            Sha256Hex(bytes));
    }

    private static string SuccessResponse(
        PromotionFixture fixture,
        string uploadId,
        string? tenantId = null,
        string? remoteRef = null)
        => JsonSerializer.Serialize(
            new
            {
                uploadId,
                remoteRef = remoteRef ?? $"https://promotion.example/receipts/{uploadId}",
                uploadedAtUtc = UploadedAt,
                tenantId = tenantId ?? TenantId,
                manifestId = fixture.Manifest.ManifestId,
                machineId = fixture.Manifest.MachineId,
                domainTag = fixture.Manifest.DomainTag,
                version = fixture.Manifest.Version,
                idempotencyKey = fixture.ExpectedIdempotencyKey,
                artifacts = fixture.Artifacts.Values
                    .OrderBy(artifact => artifact.Role, StringComparer.Ordinal)
                    .Select(artifact => new
                    {
                        role = artifact.Role,
                        length = artifact.Length,
                        sha256 = artifact.Sha256
                    })
                    .ToArray()
            },
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

    private static string DuplicateCheckpointArtifactsResponse(
        PromotionFixture fixture,
        string uploadId)
    {
        var checkpoint = fixture.Artifacts["checkpoint"];
        return JsonSerializer.Serialize(
            new
            {
                uploadId,
                remoteRef = $"https://promotion.example/receipts/{uploadId}",
                uploadedAtUtc = UploadedAt,
                tenantId = TenantId,
                manifestId = fixture.Manifest.ManifestId,
                machineId = fixture.Manifest.MachineId,
                domainTag = fixture.Manifest.DomainTag,
                version = fixture.Manifest.Version,
                idempotencyKey = fixture.ExpectedIdempotencyKey,
                artifacts = Enumerable.Range(0, 3)
                    .Select(_ => new
                    {
                        role = checkpoint.Role,
                        length = checkpoint.Length,
                        sha256 = checkpoint.Sha256
                    })
                    .ToArray()
            },
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }

    private static HttpResponseMessage JsonResponse(HttpStatusCode statusCode, string body)
        => new(statusCode)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };

    private static string BuildExpectedIdempotencyKey(
        DesignConversionTrainingManifest manifest,
        IReadOnlyDictionary<string, ArtifactExpectation> artifacts)
    {
        var material = string.Join(
            "\n",
            "design_conversion_model_server_promotion_upload_http_v1",
            manifest.ManifestId,
            manifest.MachineId,
            manifest.DomainTag,
            manifest.Version,
            manifest.AcceptedRecordCount.ToString(System.Globalization.CultureInfo.InvariantCulture),
            manifest.QuarantinedRecordCount.ToString(System.Globalization.CultureInfo.InvariantCulture),
            string.Join(
                "\n",
                artifacts.Values
                    .OrderBy(artifact => artifact.Role, StringComparer.Ordinal)
                    .Select(artifact => string.Join(
                        ":",
                        artifact.Role,
                        artifact.Length.ToString(System.Globalization.CultureInfo.InvariantCulture),
                        artifact.Sha256))));

        return "design-conversion-promotion-v1-" + Sha256Hex(Encoding.UTF8.GetBytes(material))[..32];
    }

    private static string BuildExpectedProvenanceRef(
        Uri endpoint,
        string uploadId,
        string canonicalRemoteRef,
        string idempotencyKey)
    {
        var material = string.Join(
            "\n",
            endpoint.IdnHost,
            uploadId,
            canonicalRemoteRef,
            idempotencyKey);

        return "upload-http:" + Sha256Hex(Encoding.UTF8.GetBytes(material))[..24];
    }

    private static string Sha256Hex(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static void AssertNoLocalPathsOnWire(
        CapturingPromotionUploadHandler handler,
        PromotionFixture fixture)
    {
        var wireSurface = handler.RequestSurface;
        AssertNoPathLeak(wireSurface, fixture.Root);
        foreach (var artifact in fixture.Artifacts.Values)
            AssertNoPathLeak(wireSurface, artifact.FullPath);
    }

    private static void AssertNoLocalPathsInReceipt(
        DesignConversionPromotionUploadReceipt receipt,
        PromotionFixture fixture)
    {
        var receiptSurface = BuildPublicReceiptSurface(receipt);
        AssertNoPathLeak(receiptSurface, fixture.Root);
        foreach (var artifact in fixture.Artifacts.Values)
            AssertNoPathLeak(receiptSurface, artifact.FullPath);
    }

    private static void AssertNoPathLeak(string surface, string path)
    {
        Assert.DoesNotContain(path, surface, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(path.Replace('\\', '/'), surface, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(new Uri(path).AbsoluteUri, surface, StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildPublicReceiptSurface(DesignConversionPromotionUploadReceipt receipt)
        => string.Join(
            "|",
            new[]
            {
                receipt.UploadId,
                receipt.RemoteRef,
                receipt.ProvenanceRef,
                receipt.PublicationStatus
            }.Concat(receipt.EvidenceRefs ?? []));

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

    private sealed record PromotionFixture(
        string TempRoot,
        string Root,
        DesignConversionTrainingManifest Manifest,
        DesignConversionPromotionGateReport GateReport,
        IReadOnlyDictionary<string, ArtifactExpectation> Artifacts,
        string ExpectedIdempotencyKey);

    private sealed record ArtifactExpectation(
        string Role,
        string WireFileName,
        string FullPath,
        string ContentText,
        long Length,
        string Sha256);

    private sealed class CapturingPromotionUploadHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory,
        Action? beforeContentRead = null) : HttpMessageHandler
    {
        private readonly Dictionary<string, IReadOnlyList<string>> _headers = new(StringComparer.OrdinalIgnoreCase);

        public int SendCount { get; private set; }
        public string? Method { get; private set; }
        public Uri? RequestUri { get; private set; }
        public byte[] BodyBytes { get; private set; } = [];
        public string BodyText => Encoding.UTF8.GetString(BodyBytes);

        public string RequestSurface
            => string.Join(
                "\n",
                new[] { Method ?? string.Empty, RequestUri?.ToString() ?? string.Empty, BodyText }
                    .Concat(_headers.SelectMany(pair => pair.Value.Select(value => $"{pair.Key}:{value}"))));

        public IReadOnlyList<string> HeaderValues(string name)
            => _headers.TryGetValue(name, out var values) ? values : [];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            SendCount++;
            Method = request.Method.Method;
            RequestUri = request.RequestUri;
            _headers.Clear();
            foreach (var header in request.Headers)
                _headers[header.Key] = header.Value.ToArray();
            if (request.Content is not null)
            {
                foreach (var header in request.Content.Headers)
                    _headers[header.Key] = header.Value.ToArray();

                beforeContentRead?.Invoke();
                BodyBytes = await request.Content.ReadAsByteArrayAsync(cancellationToken);
            }

            return responseFactory(request);
        }
    }

    private sealed class DelayingPromotionUploadHandler(TimeSpan delay) : HttpMessageHandler
    {
        public int SendCount { get; private set; }
        public bool ObservedTokenCanBeCanceled { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            SendCount++;
            ObservedTokenCanBeCanceled = cancellationToken.CanBeCanceled;
            if (request.Content is not null)
                await request.Content.ReadAsByteArrayAsync(cancellationToken);

            await Task.Delay(delay, cancellationToken);
            return JsonResponse(HttpStatusCode.Created, "{}");
        }
    }

    private sealed class DelayingReadStream(TimeSpan delay, Action? readStarted = null) : Stream
    {
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
            => throw new NotSupportedException();

        public override async ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            readStarted?.Invoke();
            await Task.Delay(delay, cancellationToken);
            return 0;
        }

        public override long Seek(long offset, SeekOrigin origin)
            => throw new NotSupportedException();

        public override void SetLength(long value)
            => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();
    }
}
