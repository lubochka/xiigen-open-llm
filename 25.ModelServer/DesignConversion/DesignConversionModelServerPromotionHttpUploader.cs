// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.DesignConversion;

using System.Globalization;
using System.Runtime.InteropServices;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using DesignConversionDomain;
using Microsoft.Win32.SafeHandles;

public sealed record DesignConversionModelServerPromotionHttpUploaderOptions(
    Uri Endpoint,
    string TenantId,
    string BearerToken,
    string AllowedArtifactRoot)
{
    public const int HardMaxArtifactCount = 3;
    public const long HardMaxArtifactBytes = 512L * 1024L * 1024L;
    public const long HardMaxTotalArtifactBytes = 1024L * 1024L * 1024L;

    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);
    public long MaxArtifactBytes { get; init; } = HardMaxArtifactBytes;
    public long MaxTotalArtifactBytes { get; init; } = HardMaxTotalArtifactBytes;
    public long MaxResponseBodyBytes { get; init; } = 64L * 1024L;
    public int MaxArtifactCount { get; init; } = HardMaxArtifactCount;

    public override string ToString()
        => $"{nameof(DesignConversionModelServerPromotionHttpUploaderOptions)} {{ " +
           $"{nameof(Endpoint)} = {Endpoint}, " +
           $"{nameof(TenantId)} = {TenantId}, " +
           $"{nameof(BearerToken)} = [REDACTED], " +
           $"{nameof(AllowedArtifactRoot)} = [REDACTED], " +
           $"{nameof(Timeout)} = {Timeout}, " +
           $"{nameof(MaxArtifactBytes)} = {MaxArtifactBytes.ToString(CultureInfo.InvariantCulture)}, " +
           $"{nameof(MaxTotalArtifactBytes)} = {MaxTotalArtifactBytes.ToString(CultureInfo.InvariantCulture)}, " +
           $"{nameof(MaxResponseBodyBytes)} = {MaxResponseBodyBytes.ToString(CultureInfo.InvariantCulture)}, " +
           $"{nameof(MaxArtifactCount)} = {MaxArtifactCount.ToString(CultureInfo.InvariantCulture)} }}";
}

public sealed class DesignConversionModelServerPromotionHttpUploader : IDesignConversionPromotionUploader
{
    public const string ContractVersion = "design_conversion_model_server_promotion_upload_http_v1";
    public const string IdempotencyHeaderName = "Idempotency-Key";
    public const string TenantHeaderName = "x-design-conversion-tenant-id";
    public const string ContractHeaderName = "x-design-conversion-promotion-upload-contract";

    private const int ArtifactBufferSize = 64 * 1024;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly DesignConversionModelServerPromotionHttpUploaderOptions _options;

    public DesignConversionModelServerPromotionHttpUploader(
        HttpClient httpClient,
        DesignConversionModelServerPromotionHttpUploaderOptions options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<OperationResult<DesignConversionPromotionUploadReceipt>> UploadAsync(
        DesignConversionTrainingManifest manifest,
        DesignConversionPromotionGateReport gateReport,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
        {
            return OperationResult<DesignConversionPromotionUploadReceipt>.Fail(
                "DesignConversion promotion upload was cancelled before HTTP send.",
                "CANCELLED");
        }

        var config = ValidateConfiguration();
        if (!config.IsSuccess)
            return config.MapError<DesignConversionPromotionUploadReceipt>();

        if (manifest is null)
        {
            return OperationResult<DesignConversionPromotionUploadReceipt>.Fail(
                "DesignConversion promotion manifest is required.",
                "DESIGN_CONVERSION_PROMOTION_UPLOAD_MANIFEST_REQUIRED");
        }

        var metadata = ValidatePromotionMetadata(manifest, gateReport);
        if (!metadata.IsSuccess)
            return metadata.MapError<DesignConversionPromotionUploadReceipt>();

        var artifactSet = await BuildArtifactSetAsync(manifest, ct);
        if (!artifactSet.IsSuccess)
            return artifactSet.MapError<DesignConversionPromotionUploadReceipt>();

        using var artifacts = artifactSet.Value!;
        var idempotencyKey = BuildIdempotencyKey(manifest, artifacts);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(_options.Timeout);

        HttpResponseMessage response;
        try
        {
            using var content = BuildMultipartContent(manifest, gateReport, artifacts, idempotencyKey);
            using var request = new HttpRequestMessage(HttpMethod.Post, _options.Endpoint)
            {
                Content = content
            };
            request.Headers.TryAddWithoutValidation(ContractHeaderName, ContractVersion);
            request.Headers.TryAddWithoutValidation(TenantHeaderName, _options.TenantId.Trim());
            request.Headers.TryAddWithoutValidation(IdempotencyHeaderName, idempotencyKey);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.BearerToken.Trim());

            response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                timeout.Token);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return OperationResult<DesignConversionPromotionUploadReceipt>.Fail(
                "DesignConversion promotion upload was cancelled.",
                "CANCELLED");
        }
        catch (OperationCanceledException) when (timeout.IsCancellationRequested)
        {
            return OperationResult<DesignConversionPromotionUploadReceipt>.Fail(
                "DesignConversion promotion upload timed out.",
                "DESIGN_CONVERSION_PROMOTION_UPLOAD_TIMEOUT");
        }
        catch (Exception ex) when (ex is
            HttpRequestException or
            IOException or
            UnauthorizedAccessException or
            InvalidOperationException or
            NotSupportedException or
            FormatException)
        {
            return OperationResult<DesignConversionPromotionUploadReceipt>.Fail(
                $"DesignConversion promotion upload HTTP request failed: {ex.GetType().Name}.",
                "DESIGN_CONVERSION_PROMOTION_UPLOAD_HTTP_REQUEST_FAILED");
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                return OperationResult<DesignConversionPromotionUploadReceipt>.Fail(
                    $"DesignConversion promotion upload endpoint returned HTTP {(int)response.StatusCode}.",
                    "DESIGN_CONVERSION_PROMOTION_UPLOAD_HTTP_NON_SUCCESS");
            }

            var body = await ReadBoundedResponseStringAsync(response.Content, timeout.Token, ct);
            if (!body.IsSuccess)
                return body.MapError<DesignConversionPromotionUploadReceipt>();

            return ParseReceipt(body.Value!, manifest, artifacts, idempotencyKey);
        }
    }

    private OperationResult<bool> ValidateConfiguration()
    {
        if (_options.Endpoint is null ||
            !_options.Endpoint.IsAbsoluteUri ||
            !string.Equals(_options.Endpoint.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ||
            !string.IsNullOrEmpty(_options.Endpoint.UserInfo) ||
            !string.IsNullOrEmpty(_options.Endpoint.Query) ||
            !string.IsNullOrEmpty(_options.Endpoint.Fragment))
        {
            return OperationResult<bool>.Fail(
                "DesignConversion promotion upload endpoint must be an absolute HTTPS URL without URL credentials, query, or fragment.",
                "DESIGN_CONVERSION_PROMOTION_UPLOAD_ENDPOINT_HTTPS_REQUIRED");
        }

        if (string.IsNullOrWhiteSpace(_options.TenantId) || !IsSafePublicToken(_options.TenantId.Trim()))
        {
            return OperationResult<bool>.Fail(
                "DesignConversion promotion upload tenant id is required.",
                "DESIGN_CONVERSION_PROMOTION_UPLOAD_TENANT_REQUIRED");
        }

        if (string.IsNullOrWhiteSpace(_options.BearerToken) ||
            _options.BearerToken.Any(char.IsControl) ||
            _options.BearerToken.Any(char.IsWhiteSpace))
        {
            return OperationResult<bool>.Fail(
                "DesignConversion promotion upload bearer token is required.",
                "DESIGN_CONVERSION_PROMOTION_UPLOAD_AUTH_REQUIRED");
        }

        if (string.IsNullOrWhiteSpace(_options.AllowedArtifactRoot))
        {
            return OperationResult<bool>.Fail(
                "DesignConversion promotion upload allowed artifact root is required.",
                "DESIGN_CONVERSION_PROMOTION_UPLOAD_ARTIFACT_ROOT_REQUIRED");
        }

        if (_options.Timeout <= TimeSpan.Zero ||
            _options.Timeout == Timeout.InfiniteTimeSpan ||
            _options.Timeout > TimeSpan.FromMinutes(5))
        {
            return OperationResult<bool>.Fail(
                "DesignConversion promotion upload timeout must be positive and bounded.",
                "DESIGN_CONVERSION_PROMOTION_UPLOAD_TIMEOUT_INVALID");
        }

        if (_options.MaxResponseBodyBytes is <= 0 or > 1024L * 1024L)
        {
            return OperationResult<bool>.Fail(
                "DesignConversion promotion upload response byte limit must be positive and bounded.",
                "DESIGN_CONVERSION_PROMOTION_UPLOAD_RESPONSE_LIMIT_INVALID");
        }

        if (_options.MaxArtifactCount != DesignConversionModelServerPromotionHttpUploaderOptions.HardMaxArtifactCount ||
            _options.MaxArtifactBytes <= 0 ||
            _options.MaxArtifactBytes > DesignConversionModelServerPromotionHttpUploaderOptions.HardMaxArtifactBytes ||
            _options.MaxTotalArtifactBytes <= 0 ||
            _options.MaxTotalArtifactBytes > DesignConversionModelServerPromotionHttpUploaderOptions.HardMaxTotalArtifactBytes ||
            _options.MaxTotalArtifactBytes < _options.MaxArtifactBytes)
        {
            return OperationResult<bool>.Fail(
                "DesignConversion promotion upload artifact limits must be positive and bounded.",
                "DESIGN_CONVERSION_PROMOTION_UPLOAD_ARTIFACT_LIMIT_INVALID");
        }

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<bool> ValidatePromotionMetadata(
        DesignConversionTrainingManifest manifest,
        DesignConversionPromotionGateReport gateReport)
    {
        if (manifest.AcceptedRecordCount < 0 || manifest.QuarantinedRecordCount < 0)
        {
            return OperationResult<bool>.Fail(
                "DesignConversion promotion upload metadata numeric values are invalid.",
                "DESIGN_CONVERSION_PROMOTION_UPLOAD_METADATA_INVALID");
        }

        if (gateReport is null)
        {
            return OperationResult<bool>.Fail(
                "DesignConversion promotion upload gate report is required.",
                "DESIGN_CONVERSION_PROMOTION_UPLOAD_GATE_REPORT_REQUIRED");
        }

        if (!IsFiniteNonNegative(gateReport.HeldOutScore) ||
            !IsFiniteNonNegative(gateReport.RequiredHeldOutScore))
        {
            return OperationResult<bool>.Fail(
                "DesignConversion promotion upload metadata numeric values are invalid.",
                "DESIGN_CONVERSION_PROMOTION_UPLOAD_METADATA_INVALID");
        }

        return OperationResult<bool>.Ok(true);
    }

    private static bool IsFiniteNonNegative(double value)
        => !double.IsNaN(value) && !double.IsInfinity(value) && value >= 0;

    private static OperationResult<bool> ValidateArtifactRoot(string root)
    {
        FileAttributes attributes;
        try
        {
            attributes = File.GetAttributes(root);
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
        {
            return OperationResult<bool>.Fail(
                "DesignConversion promotion upload artifact root does not exist.",
                "DESIGN_CONVERSION_PROMOTION_UPLOAD_ARTIFACT_ROOT_MISSING");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            return OperationResult<bool>.Fail(
                $"DesignConversion promotion upload artifact root could not be inspected: {ex.GetType().Name}.",
                "DESIGN_CONVERSION_PROMOTION_UPLOAD_ARTIFACT_ROOT_OPEN_FAILED");
        }

        if ((attributes & FileAttributes.Directory) == 0)
        {
            return OperationResult<bool>.Fail(
                "DesignConversion promotion upload artifact root does not exist.",
                "DESIGN_CONVERSION_PROMOTION_UPLOAD_ARTIFACT_ROOT_MISSING");
        }

        if ((attributes & FileAttributes.ReparsePoint) != 0)
        {
            return OperationResult<bool>.Fail(
                "DesignConversion promotion upload artifact root must not be a reparse point.",
                "DESIGN_CONVERSION_PROMOTION_UPLOAD_ARTIFACT_ROOT_REPARSE");
        }

        return OperationResult<bool>.Ok(true);
    }

    private async Task<OperationResult<PromotionArtifactSet>> BuildArtifactSetAsync(
        DesignConversionTrainingManifest manifest,
        CancellationToken ct)
    {
        if (manifest.PromotionArtifacts is null)
        {
            return OperationResult<PromotionArtifactSet>.Fail(
                "DesignConversion promotion upload requires promotion artifacts.",
                "DESIGN_CONVERSION_PROMOTION_UPLOAD_ARTIFACTS_REQUIRED");
        }

        string root;
        try
        {
            root = EnsureTrailingDirectorySeparator(Path.GetFullPath(_options.AllowedArtifactRoot));
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return OperationResult<PromotionArtifactSet>.Fail(
                "DesignConversion promotion upload artifact root is invalid.",
                "DESIGN_CONVERSION_PROMOTION_UPLOAD_ARTIFACT_ROOT_INVALID");
        }

        var rootValidation = ValidateArtifactRoot(root);
        if (!rootValidation.IsSuccess)
            return rootValidation.MapError<PromotionArtifactSet>();

        var artifactInputs = new[]
        {
            new ArtifactInput("checkpoint", "checkpoint.bin", manifest.PromotionArtifacts.CheckpointPath),
            new ArtifactInput("tokenizer", "tokenizer.json", manifest.PromotionArtifacts.TokenizerPath),
            new ArtifactInput("policy", "policy.json", manifest.PromotionArtifacts.PolicyPath)
        };

        if (artifactInputs.Length > _options.MaxArtifactCount)
        {
            return OperationResult<PromotionArtifactSet>.Fail(
                "DesignConversion promotion upload artifact count exceeded the configured limit.",
                "DESIGN_CONVERSION_PROMOTION_UPLOAD_ARTIFACT_COUNT_EXCEEDED");
        }

        var artifacts = new List<PromotionArtifact>();
        var seenFullPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        long totalBytes = 0;
        var ownershipTransferred = false;
        try
        {
            foreach (var input in artifactInputs)
            {
                var prepared = await PrepareArtifactAsync(root, input, ct);
                if (!prepared.IsSuccess)
                    return prepared.MapError<PromotionArtifactSet>();

                var artifact = prepared.Value!;
                artifacts.Add(artifact);
                if (!seenFullPaths.Add(artifact.FullPath))
                {
                    return OperationResult<PromotionArtifactSet>.Fail(
                        "DesignConversion promotion upload artifacts must be distinct files.",
                        "DESIGN_CONVERSION_PROMOTION_UPLOAD_ARTIFACT_DUPLICATE");
                }

                if (artifact.Length > _options.MaxTotalArtifactBytes - totalBytes)
                {
                    return OperationResult<PromotionArtifactSet>.Fail(
                        "DesignConversion promotion upload artifacts exceed the configured total byte limit.",
                        "DESIGN_CONVERSION_PROMOTION_UPLOAD_ARTIFACT_TOTAL_OVERSIZE");
                }

                totalBytes += artifact.Length;
            }

            var result = new PromotionArtifactSet(root, artifacts.ToArray());
            ownershipTransferred = true;
            return OperationResult<PromotionArtifactSet>.Ok(result);
        }
        finally
        {
            if (!ownershipTransferred)
                DisposeArtifacts(artifacts);
        }
    }

    private async Task<OperationResult<PromotionArtifact>> PrepareArtifactAsync(
        string root,
        ArtifactInput input,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(input.Path))
        {
            return OperationResult<PromotionArtifact>.Fail(
                $"DesignConversion promotion upload {input.Role} artifact path is required.",
                "DESIGN_CONVERSION_PROMOTION_UPLOAD_ARTIFACT_PATH_REQUIRED");
        }

        string fullPath;
        try
        {
            fullPath = Path.GetFullPath(input.Path);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return OperationResult<PromotionArtifact>.Fail(
                $"DesignConversion promotion upload {input.Role} artifact path is invalid.",
                "DESIGN_CONVERSION_PROMOTION_UPLOAD_ARTIFACT_PATH_INVALID");
        }

        if (!IsUnderRoot(root, fullPath))
        {
            return OperationResult<PromotionArtifact>.Fail(
                $"DesignConversion promotion upload {input.Role} artifact must stay under the allowed artifact root.",
                "DESIGN_CONVERSION_PROMOTION_UPLOAD_ARTIFACT_OUT_OF_ROOT");
        }

        FileStream? stream = null;
        try
        {
            try
            {
                stream = new FileStream(
                    fullPath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    ArtifactBufferSize,
                    FileOptions.Asynchronous | FileOptions.SequentialScan);
            }
            catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
            {
                return OperationResult<PromotionArtifact>.Fail(
                    $"DesignConversion promotion upload {input.Role} artifact disappeared before it could be opened.",
                    "DESIGN_CONVERSION_PROMOTION_UPLOAD_ARTIFACT_MISSING");
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
            {
                return OperationResult<PromotionArtifact>.Fail(
                    $"DesignConversion promotion upload {input.Role} artifact could not be opened: {ex.GetType().Name}.",
                    "DESIGN_CONVERSION_PROMOTION_UPLOAD_ARTIFACT_OPEN_FAILED");
            }

            var openedTarget = ValidateOpenedArtifactTarget(root, fullPath, stream.SafeFileHandle, input.Role);
            if (!openedTarget.IsSuccess)
                return openedTarget.MapError<PromotionArtifact>();

            long length;
            try
            {
                length = stream.Length;
            }
            catch (Exception ex) when (ex is IOException or NotSupportedException or ObjectDisposedException)
            {
                return OperationResult<PromotionArtifact>.Fail(
                    $"DesignConversion promotion upload {input.Role} artifact length could not be read: {ex.GetType().Name}.",
                    "DESIGN_CONVERSION_PROMOTION_UPLOAD_ARTIFACT_OPEN_FAILED");
            }

            if (length > _options.MaxArtifactBytes)
            {
                return OperationResult<PromotionArtifact>.Fail(
                    $"DesignConversion promotion upload {input.Role} artifact exceeds the configured byte limit.",
                    "DESIGN_CONVERSION_PROMOTION_UPLOAD_ARTIFACT_OVERSIZE");
            }

            var sha = await ComputeSha256HexAsync(stream, ct);
            if (!sha.IsSuccess)
                return sha.MapError<PromotionArtifact>();

            try
            {
                if (stream.Length != length)
                {
                    return OperationResult<PromotionArtifact>.Fail(
                        $"DesignConversion promotion upload {input.Role} artifact changed while it was being prepared.",
                        "DESIGN_CONVERSION_PROMOTION_UPLOAD_ARTIFACT_CHANGED");
                }

                stream.Position = 0;
            }
            catch (Exception ex) when (ex is IOException or NotSupportedException or ObjectDisposedException)
            {
                return OperationResult<PromotionArtifact>.Fail(
                    $"DesignConversion promotion upload {input.Role} artifact could not be prepared: {ex.GetType().Name}.",
                    "DESIGN_CONVERSION_PROMOTION_UPLOAD_ARTIFACT_HASH_FAILED");
            }

            var stableStream = stream;
            stream = null;
            return OperationResult<PromotionArtifact>.Ok(new PromotionArtifact(
                Role: input.Role,
                WireFileName: input.WireFileName,
                FullPath: openedTarget.Value!,
                Length: length,
                Sha256: sha.Value!,
                ContentStream: stableStream));
        }
        finally
        {
            stream?.Dispose();
        }
    }

    private MultipartFormDataContent BuildMultipartContent(
        DesignConversionTrainingManifest manifest,
        DesignConversionPromotionGateReport gateReport,
        PromotionArtifactSet artifacts,
        string idempotencyKey)
    {
        var content = new MultipartFormDataContent();
        try
        {
            var metadata = new PromotionUploadRequestMetadata(
                ContractVersion: ContractVersion,
                TenantId: _options.TenantId.Trim(),
                ManifestId: SafeScalar(manifest.ManifestId),
                MachineId: SafeScalar(manifest.MachineId),
                DomainTag: SafeScalar(manifest.DomainTag),
                Version: SafeScalar(manifest.Version),
                IdempotencyKey: idempotencyKey,
                AcceptedRecordCount: manifest.AcceptedRecordCount,
                QuarantinedRecordCount: manifest.QuarantinedRecordCount,
                HeldOutScore: gateReport?.HeldOutScore,
                RequiredHeldOutScore: gateReport?.RequiredHeldOutScore,
                Artifacts: artifacts.Items
                    .Select(artifact => new PromotionUploadArtifactMetadata(
                        artifact.Role,
                        artifact.WireFileName,
                        artifact.Length,
                        artifact.Sha256))
                    .ToArray());

            content.Add(
                new StringContent(JsonSerializer.Serialize(metadata, JsonOptions), Encoding.UTF8, "application/json"),
                "metadata");

            foreach (var artifact in artifacts.Items)
            {
                artifact.ContentStream.Position = 0;
                var streamContent = new StreamContent(artifact.ContentStream, ArtifactBufferSize);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                streamContent.Headers.ContentLength = artifact.Length;
                streamContent.Headers.TryAddWithoutValidation("x-design-conversion-artifact-role", artifact.Role);
                streamContent.Headers.TryAddWithoutValidation("x-design-conversion-artifact-sha256", artifact.Sha256);
                streamContent.Headers.TryAddWithoutValidation(
                    "x-design-conversion-artifact-length",
                    artifact.Length.ToString(CultureInfo.InvariantCulture));
                content.Add(streamContent, artifact.Role, artifact.WireFileName);
            }

            return content;
        }
        catch
        {
            content.Dispose();
            throw;
        }
    }

    private async Task<OperationResult<string>> ReadBoundedResponseStringAsync(
        HttpContent content,
        CancellationToken timeoutToken,
        CancellationToken callerToken)
    {
        if (content.Headers.ContentLength is > 0 &&
            content.Headers.ContentLength.Value > _options.MaxResponseBodyBytes)
        {
            return OperationResult<string>.Fail(
                "DesignConversion promotion upload response exceeded the bounded Content-Length.",
                "DESIGN_CONVERSION_PROMOTION_UPLOAD_RESPONSE_TOO_LARGE");
        }

        try
        {
            await using var stream = await content.ReadAsStreamAsync(timeoutToken);
            using var memory = new MemoryStream();
            var buffer = new byte[8192];
            while (true)
            {
                var read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), timeoutToken);
                if (read == 0)
                    break;

                if (memory.Length + read > _options.MaxResponseBodyBytes)
                {
                    return OperationResult<string>.Fail(
                        "DesignConversion promotion upload response exceeded the bounded byte limit.",
                        "DESIGN_CONVERSION_PROMOTION_UPLOAD_RESPONSE_TOO_LARGE");
                }

                memory.Write(buffer, 0, read);
            }

            return OperationResult<string>.Ok(Encoding.UTF8.GetString(memory.ToArray()));
        }
        catch (OperationCanceledException) when (callerToken.IsCancellationRequested)
        {
            return OperationResult<string>.Fail(
                "DesignConversion promotion upload response read was cancelled.",
                "CANCELLED");
        }
        catch (OperationCanceledException)
        {
            return OperationResult<string>.Fail(
                "DesignConversion promotion upload response read timed out.",
                "DESIGN_CONVERSION_PROMOTION_UPLOAD_TIMEOUT");
        }
        catch (HttpRequestException ex)
        {
            return OperationResult<string>.Fail(
                $"DesignConversion promotion upload response read failed: {ex.GetType().Name}.",
                "DESIGN_CONVERSION_PROMOTION_UPLOAD_RESPONSE_READ_FAILED");
        }
        catch (IOException ex)
        {
            return OperationResult<string>.Fail(
                $"DesignConversion promotion upload response read failed: {ex.GetType().Name}.",
                "DESIGN_CONVERSION_PROMOTION_UPLOAD_RESPONSE_READ_FAILED");
        }
    }

    private OperationResult<DesignConversionPromotionUploadReceipt> ParseReceipt(
        string responseJson,
        DesignConversionTrainingManifest manifest,
        PromotionArtifactSet artifacts,
        string idempotencyKey)
    {
        PromotionUploadResponse? response;
        try
        {
            response = JsonSerializer.Deserialize<PromotionUploadResponse>(responseJson, JsonOptions);
        }
        catch (JsonException)
        {
            return OperationResult<DesignConversionPromotionUploadReceipt>.Fail(
                "DesignConversion promotion upload response was malformed JSON.",
                "DESIGN_CONVERSION_PROMOTION_UPLOAD_RESPONSE_MALFORMED");
        }

        if (response is null)
        {
            return OperationResult<DesignConversionPromotionUploadReceipt>.Fail(
                "DesignConversion promotion upload response was empty.",
                "DESIGN_CONVERSION_PROMOTION_UPLOAD_RESPONSE_MALFORMED");
        }

        var coherence = ValidateResponseCoherence(response, manifest, artifacts, idempotencyKey);
        if (!coherence.IsSuccess)
            return coherence.MapError<DesignConversionPromotionUploadReceipt>();
        var validatedResponse = coherence.Value!;

        var provenanceHash = ShortSha256Hex(
            string.Join(
                "\n",
                _options.Endpoint.IdnHost,
                validatedResponse.UploadId,
                validatedResponse.RemoteRef,
                idempotencyKey));

        return OperationResult<DesignConversionPromotionUploadReceipt>.Ok(
            new DesignConversionPromotionUploadReceipt(
                UploadId: validatedResponse.UploadId,
                RemoteRef: validatedResponse.RemoteRef,
                UploadedAtUtc: validatedResponse.UploadedAtUtc,
                EvidenceRefs:
                [
                    "MODEL_SERVER_PROMOTION_HTTP_UPLOAD_RECEIPT_VERIFIED",
                    $"MODEL_SERVER_PROMOTION_HTTP_UPLOAD_ENDPOINT_HOST:{_options.Endpoint.IdnHost}",
                    $"MODEL_SERVER_PROMOTION_HTTP_UPLOAD_ARTIFACT_BUNDLE_SHA256:{BuildArtifactBundleSha256(artifacts)}"
                ])
            {
                PublicationKind = DesignConversionPromotionPublicationKind.VerifiedModelServerUpload,
                UploadAttempted = true,
                UploadSucceeded = true,
                VerificationSucceeded = true,
                PublicationStatus = DesignConversionPromotionPublicationStatus.UploadedVerified,
                ProvenanceRef = $"upload-http:{provenanceHash}"
            });
    }

    private OperationResult<ValidatedPromotionUploadResponse> ValidateResponseCoherence(
        PromotionUploadResponse response,
        DesignConversionTrainingManifest manifest,
        PromotionArtifactSet artifacts,
        string idempotencyKey)
    {
        if (string.IsNullOrWhiteSpace(response.UploadId) ||
            !IsSafePublicToken(response.UploadId.Trim()))
        {
            return IncoherentReceipt("DESIGN_CONVERSION_PROMOTION_UPLOAD_RESPONSE_UPLOAD_ID_INVALID");
        }

        string canonicalRemoteRef;
        if (string.IsNullOrWhiteSpace(response.RemoteRef) ||
            !TryGetSafeAbsoluteHttpsReceiptRef(response.RemoteRef.Trim(), out canonicalRemoteRef))
        {
            return IncoherentReceipt("DESIGN_CONVERSION_PROMOTION_UPLOAD_RESPONSE_REMOTE_REF_INVALID");
        }

        if (!response.UploadedAtUtc.HasValue)
            return IncoherentReceipt("DESIGN_CONVERSION_PROMOTION_UPLOAD_RESPONSE_UPLOADED_AT_INVALID");

        if (!string.Equals(response.TenantId, _options.TenantId.Trim(), StringComparison.Ordinal) ||
            !string.Equals(response.ManifestId, manifest.ManifestId, StringComparison.Ordinal) ||
            !string.Equals(response.MachineId, manifest.MachineId, StringComparison.Ordinal) ||
            !string.Equals(response.DomainTag, manifest.DomainTag, StringComparison.Ordinal) ||
            !string.Equals(response.Version, manifest.Version, StringComparison.Ordinal) ||
            !string.Equals(response.IdempotencyKey, idempotencyKey, StringComparison.Ordinal))
        {
            return IncoherentReceipt("DESIGN_CONVERSION_PROMOTION_UPLOAD_RESPONSE_IDENTITY_MISMATCH");
        }

        var expected = artifacts.Items.ToDictionary(artifact => artifact.Role, StringComparer.Ordinal);
        var actual = response.Artifacts ?? [];
        if (actual.Count != expected.Count)
            return IncoherentReceipt("DESIGN_CONVERSION_PROMOTION_UPLOAD_RESPONSE_ARTIFACT_MISMATCH");

        var seenRoles = new HashSet<string>(StringComparer.Ordinal);
        foreach (var artifact in actual)
        {
            if (artifact is null ||
                string.IsNullOrWhiteSpace(artifact.Role) ||
                !seenRoles.Add(artifact.Role) ||
                !expected.TryGetValue(artifact.Role, out var expectedArtifact) ||
                artifact.Length != expectedArtifact.Length ||
                !string.Equals(artifact.Sha256, expectedArtifact.Sha256, StringComparison.Ordinal))
            {
                return IncoherentReceipt("DESIGN_CONVERSION_PROMOTION_UPLOAD_RESPONSE_ARTIFACT_MISMATCH");
            }
        }

        if (seenRoles.Count != expected.Count)
            return IncoherentReceipt("DESIGN_CONVERSION_PROMOTION_UPLOAD_RESPONSE_ARTIFACT_MISMATCH");

        return OperationResult<ValidatedPromotionUploadResponse>.Ok(
            new ValidatedPromotionUploadResponse(
                response.UploadId!.Trim(),
                canonicalRemoteRef,
                response.UploadedAtUtc.Value));
    }

    private static OperationResult<ValidatedPromotionUploadResponse> IncoherentReceipt(string reason)
        => OperationResult<ValidatedPromotionUploadResponse>.Fail(
            "DesignConversion promotion upload response receipt was incoherent.",
            reason);

    private static string BuildIdempotencyKey(
        DesignConversionTrainingManifest manifest,
        PromotionArtifactSet artifacts)
    {
        var material = string.Join(
            "\n",
            ContractVersion,
            SafeScalar(manifest.ManifestId),
            SafeScalar(manifest.MachineId),
            SafeScalar(manifest.DomainTag),
            SafeScalar(manifest.Version),
            manifest.AcceptedRecordCount.ToString(CultureInfo.InvariantCulture),
            manifest.QuarantinedRecordCount.ToString(CultureInfo.InvariantCulture),
            string.Join(
                "\n",
                artifacts.Items
                    .OrderBy(artifact => artifact.Role, StringComparer.Ordinal)
                    .Select(artifact => string.Join(
                        ":",
                        artifact.Role,
                        artifact.Length.ToString(CultureInfo.InvariantCulture),
                        artifact.Sha256))));

        return "design-conversion-promotion-v1-" + ShortSha256Hex(material, 32);
    }

    private static async Task<OperationResult<string>> ComputeSha256HexAsync(
        Stream stream,
        CancellationToken ct)
    {
        try
        {
            var hash = await SHA256.HashDataAsync(stream, ct);
            return OperationResult<string>.Ok(Convert.ToHexString(hash).ToLowerInvariant());
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return OperationResult<string>.Fail(
                "DesignConversion promotion upload artifact hashing was cancelled.",
                "CANCELLED");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            return OperationResult<string>.Fail(
                $"DesignConversion promotion upload artifact hash failed: {ex.GetType().Name}.",
                "DESIGN_CONVERSION_PROMOTION_UPLOAD_ARTIFACT_HASH_FAILED");
        }
    }

    private static bool IsUnderRoot(string rootWithSeparator, string fullPath)
    {
        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        return fullPath.StartsWith(rootWithSeparator, comparison);
    }

    private static string EnsureTrailingDirectorySeparator(string value)
        => value.EndsWith(Path.DirectorySeparatorChar) || value.EndsWith(Path.AltDirectorySeparatorChar)
            ? value
            : value + Path.DirectorySeparatorChar;

    private static OperationResult<string> ValidateOpenedArtifactTarget(
        string rootWithSeparator,
        string requestedFullPath,
        SafeFileHandle handle,
        string role)
    {
        var openedPath = ResolveOpenedFinalPath(handle);
        if (!openedPath.IsSuccess)
            return openedPath.MapError<string>();

        string openedFullPath;
        try
        {
            openedFullPath = Path.GetFullPath(openedPath.Value!);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return OperationResult<string>.Fail(
                $"DesignConversion promotion upload {role} artifact opened target could not be canonicalized: {ex.GetType().Name}.",
                "DESIGN_CONVERSION_PROMOTION_UPLOAD_ARTIFACT_FINAL_PATH_UNAVAILABLE");
        }

        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        if (!IsUnderRoot(rootWithSeparator, openedFullPath) ||
            !string.Equals(openedFullPath, requestedFullPath, comparison))
        {
            return OperationResult<string>.Fail(
                $"DesignConversion promotion upload {role} artifact opened target escaped the allowed artifact root.",
                "DESIGN_CONVERSION_PROMOTION_UPLOAD_ARTIFACT_OPENED_TARGET_ESCAPE");
        }

        return OperationResult<string>.Ok(openedFullPath);
    }

    private static OperationResult<string> ResolveOpenedFinalPath(SafeFileHandle handle)
    {
        if (handle.IsInvalid || handle.IsClosed)
        {
            return OperationResult<string>.Fail(
                "DesignConversion promotion upload artifact opened target could not be resolved securely.",
                "DESIGN_CONVERSION_PROMOTION_UPLOAD_ARTIFACT_FINAL_PATH_UNAVAILABLE");
        }

        if (OperatingSystem.IsWindows())
            return ResolveOpenedFinalPathWindows(handle);
        if (OperatingSystem.IsLinux())
            return ResolveOpenedFinalPathLinux(handle);

        return OperationResult<string>.Fail(
            "DesignConversion promotion upload artifact opened target resolution is unavailable on this platform.",
            "DESIGN_CONVERSION_PROMOTION_UPLOAD_ARTIFACT_FINAL_PATH_UNAVAILABLE");
    }

    private static OperationResult<string> ResolveOpenedFinalPathWindows(SafeFileHandle handle)
    {
        const int maxFinalPathChars = 32768;
        var capacity = 512;
        while (capacity <= maxFinalPathChars)
        {
            var builder = new StringBuilder(capacity);
            var length = GetFinalPathNameByHandleW(handle, builder, (uint)builder.Capacity, 0);
            if (length == 0)
            {
                return OperationResult<string>.Fail(
                    "DesignConversion promotion upload artifact opened target could not be resolved securely.",
                    "DESIGN_CONVERSION_PROMOTION_UPLOAD_ARTIFACT_FINAL_PATH_UNAVAILABLE");
            }

            if (length < builder.Capacity)
                return NormalizeWindowsFinalPath(builder.ToString());

            if (length >= maxFinalPathChars)
                break;

            capacity = checked((int)length + 1);
        }

        return OperationResult<string>.Fail(
            "DesignConversion promotion upload artifact opened target path was ambiguous.",
            "DESIGN_CONVERSION_PROMOTION_UPLOAD_ARTIFACT_FINAL_PATH_UNAVAILABLE");
    }

    private static OperationResult<string> NormalizeWindowsFinalPath(string finalPath)
    {
        const string dosPrefix = @"\\?\";
        const string uncPrefix = @"\\?\UNC\";
        string normalized;
        if (finalPath.StartsWith(uncPrefix, StringComparison.OrdinalIgnoreCase))
            normalized = @"\\" + finalPath[uncPrefix.Length..];
        else if (finalPath.StartsWith(dosPrefix, StringComparison.OrdinalIgnoreCase))
            normalized = finalPath[dosPrefix.Length..];
        else
            return OperationResult<string>.Fail(
                "DesignConversion promotion upload artifact opened target path was ambiguous.",
                "DESIGN_CONVERSION_PROMOTION_UPLOAD_ARTIFACT_FINAL_PATH_UNAVAILABLE");

        if (!Path.IsPathFullyQualified(normalized))
        {
            return OperationResult<string>.Fail(
                "DesignConversion promotion upload artifact opened target path was ambiguous.",
                "DESIGN_CONVERSION_PROMOTION_UPLOAD_ARTIFACT_FINAL_PATH_UNAVAILABLE");
        }

        return OperationResult<string>.Ok(normalized);
    }

    private static OperationResult<string> ResolveOpenedFinalPathLinux(SafeFileHandle handle)
    {
        const int maxLinkBytes = 64 * 1024;
        var fd = handle.DangerousGetHandle().ToInt64();
        if (fd < 0)
        {
            return OperationResult<string>.Fail(
                "DesignConversion promotion upload artifact opened target could not be resolved securely.",
                "DESIGN_CONVERSION_PROMOTION_UPLOAD_ARTIFACT_FINAL_PATH_UNAVAILABLE");
        }

        var procFdPath = "/proc/self/fd/" + fd.ToString(CultureInfo.InvariantCulture);
        var bufferLength = 256;
        while (bufferLength <= maxLinkBytes)
        {
            var buffer = new byte[bufferLength];
            var read = readlink(procFdPath, buffer, (nuint)buffer.Length);
            if (read < 0)
            {
                return OperationResult<string>.Fail(
                    "DesignConversion promotion upload artifact opened target could not be resolved securely.",
                    "DESIGN_CONVERSION_PROMOTION_UPLOAD_ARTIFACT_FINAL_PATH_UNAVAILABLE");
            }

            if (read < buffer.Length)
            {
                var finalPath = Encoding.UTF8.GetString(buffer, 0, (int)read);
                if (!Path.IsPathFullyQualified(finalPath) ||
                    finalPath.EndsWith(" (deleted)", StringComparison.Ordinal))
                {
                    return OperationResult<string>.Fail(
                        "DesignConversion promotion upload artifact opened target path was ambiguous.",
                        "DESIGN_CONVERSION_PROMOTION_UPLOAD_ARTIFACT_FINAL_PATH_UNAVAILABLE");
                }

                return OperationResult<string>.Ok(finalPath);
            }

            bufferLength *= 2;
        }

        return OperationResult<string>.Fail(
            "DesignConversion promotion upload artifact opened target path was ambiguous.",
            "DESIGN_CONVERSION_PROMOTION_UPLOAD_ARTIFACT_FINAL_PATH_UNAVAILABLE");
    }

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern uint GetFinalPathNameByHandleW(
        SafeFileHandle hFile,
        [Out] StringBuilder lpszFilePath,
        uint cchFilePath,
        uint dwFlags);

    [DllImport("libc", EntryPoint = "readlink", SetLastError = true)]
    private static extern nint readlink(
        string path,
        byte[] buffer,
        nuint bufferSize);

    private static bool TryGetSafeAbsoluteHttpsReceiptRef(string remoteRef, out string canonicalRef)
    {
        canonicalRef = string.Empty;
        if (!TryGetSafeRawHttpsReceiptPath(remoteRef, out var rawPath) ||
            !HasSafeRemoteReceiptPath(rawPath))
        {
            return false;
        }

        if (!Uri.TryCreate(remoteRef, UriKind.Absolute, out var parsed) ||
            !string.Equals(parsed.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ||
            !HasSafeReceiptHost(parsed) ||
            !string.IsNullOrEmpty(parsed.UserInfo) ||
            !string.IsNullOrEmpty(parsed.Query) ||
            !string.IsNullOrEmpty(parsed.Fragment) ||
            !HasSafeRemoteReceiptPath(parsed.AbsolutePath))
        {
            return false;
        }

        var candidateCanonicalRef = parsed.AbsoluteUri;
        if (!HasSafeRawPromotionReceiptRef(candidateCanonicalRef) ||
            !Uri.TryCreate(candidateCanonicalRef, UriKind.Absolute, out var canonicalUri) ||
            !HasSafeRemoteReceiptPath(canonicalUri.AbsolutePath))
        {
            return false;
        }

        canonicalRef = canonicalUri.AbsoluteUri;
        return true;
    }

    private static bool TryGetSafeRawHttpsReceiptPath(string remoteRef, out string rawPath)
    {
        const string schemePrefix = "https://";
        rawPath = string.Empty;
        if (!HasSafeRawPromotionReceiptRef(remoteRef) ||
            !remoteRef.StartsWith(schemePrefix, StringComparison.OrdinalIgnoreCase) ||
            remoteRef.IndexOfAny(['?', '#']) >= 0)
        {
            return false;
        }

        var authorityStart = schemePrefix.Length;
        var pathStart = remoteRef.IndexOf('/', authorityStart);
        var authorityEnd = pathStart < 0 ? remoteRef.Length : pathStart;
        if (authorityEnd == authorityStart)
            return false;

        var rawAuthority = remoteRef[authorityStart..authorityEnd];
        if (rawAuthority.Contains('@', StringComparison.Ordinal))
            return false;

        rawPath = pathStart < 0 ? "/" : remoteRef[pathStart..];
        return true;
    }

    private static bool HasSafeRawPromotionReceiptRef(string remoteRef)
    {
        const int maxLength = 2048;
        return !string.IsNullOrWhiteSpace(remoteRef) &&
               remoteRef.Length <= maxLength &&
               string.Equals(remoteRef, remoteRef.Trim(), StringComparison.Ordinal) &&
               !remoteRef.Any(ch => char.IsControl(ch) || char.IsWhiteSpace(ch)) &&
               !remoteRef.Contains('\\', StringComparison.Ordinal);
    }

    private static bool HasSafeReceiptHost(Uri uri)
    {
        var host = uri.IdnHost;
        if (string.IsNullOrWhiteSpace(host) || host.Length > 253)
            return false;

        return uri.HostNameType switch
        {
            UriHostNameType.Dns => host
                .Split('.', StringSplitOptions.None)
                .All(label => label.Length is > 0 and <= 63 &&
                              char.IsAsciiLetterOrDigit(label[0]) &&
                              char.IsAsciiLetterOrDigit(label[^1]) &&
                              label.All(ch => char.IsAsciiLetterOrDigit(ch) || ch == '-')),
            UriHostNameType.IPv4 => host.All(ch => char.IsAsciiDigit(ch) || ch == '.'),
            UriHostNameType.IPv6 => host.All(ch => char.IsAsciiHexDigit(ch) || ch is ':' or '.'),
            _ => false
        };
    }

    private static bool HasSafeRemoteReceiptPath(string rawPath)
    {
        const int maxDecodeLayers = 8;
        var currentLayer = rawPath;

        for (var decodeCount = 0; ; decodeCount++)
        {
            if (!TryValidateRemoteReceiptPathLayer(currentLayer, out var hasPercentEscapes))
                return false;
            if (!hasPercentEscapes)
                return true;
            if (decodeCount >= maxDecodeLayers)
                return false;

            string decodedLayer;
            try
            {
                decodedLayer = Uri.UnescapeDataString(currentLayer);
            }
            catch (UriFormatException)
            {
                return false;
            }

            if (string.Equals(decodedLayer, currentLayer, StringComparison.Ordinal))
                return false;

            currentLayer = decodedLayer;
        }
    }

    private static bool TryValidateRemoteReceiptPathLayer(string path, out bool hasPercentEscapes)
    {
        hasPercentEscapes = false;
        if (path.Any(ch => char.IsControl(ch) || char.IsWhiteSpace(ch) || ch == '\\') ||
            path.Split('/', StringSplitOptions.None).Any(segment => segment is "." or ".."))
        {
            return false;
        }

        for (var index = 0; index < path.Length; index++)
        {
            if (path[index] != '%')
                continue;

            hasPercentEscapes = true;
            if (index > path.Length - 3)
                return false;

            var high = HexDigitValue(path[index + 1]);
            var low = HexDigitValue(path[index + 2]);
            if (high < 0 || low < 0)
                return false;

            var decodedByte = (high << 4) | low;
            if (decodedByte <= 0x20 ||
                decodedByte == 0x7f ||
                decodedByte is 0x2f or 0x5c)
            {
                return false;
            }

            index += 2;
        }

        return true;
    }

    private static int HexDigitValue(char value)
        => value switch
        {
            >= '0' and <= '9' => value - '0',
            >= 'a' and <= 'f' => value - 'a' + 10,
            >= 'A' and <= 'F' => value - 'A' + 10,
            _ => -1
        };

    private static bool IsSafePublicToken(string value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Length > 256 ||
            !string.Equals(value, value.Trim(), StringComparison.Ordinal) ||
            value.Any(char.IsControl) ||
            value.Any(char.IsWhiteSpace) ||
            value.IndexOfAny(['=', '?', '&', '/', '\\']) >= 0 ||
            value.Any(ch => !char.IsAsciiLetterOrDigit(ch) && ch is not '-' and not '_' and not '.' and not ':'))
        {
            return false;
        }

        string[] sensitiveFragments =
        [
            "credential",
            "bearer",
            "token",
            "password",
            "secret",
            "api-key",
            "api_key",
            "apikey"
        ];

        return !sensitiveFragments.Any(fragment => value.Contains(fragment, StringComparison.OrdinalIgnoreCase));
    }

    private static string SafeScalar(string? value)
        => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

    private static string ShortSha256Hex(string value, int length = 24)
    {
        var hex = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
        return hex[..Math.Min(length, hex.Length)];
    }

    private static string BuildArtifactBundleSha256(PromotionArtifactSet artifacts)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(
                "\n",
                artifacts.Items
                    .OrderBy(artifact => artifact.Role, StringComparer.Ordinal)
                    .Select(artifact => $"{artifact.Role}:{artifact.Length}:{artifact.Sha256}")))))
            .ToLowerInvariant();

    private static void DisposeArtifacts(IEnumerable<PromotionArtifact> artifacts)
    {
        foreach (var artifact in artifacts)
            artifact.ContentStream.Dispose();
    }

    private sealed record ArtifactInput(string Role, string WireFileName, string Path);

    private sealed record PromotionArtifactSet(string Root, IReadOnlyList<PromotionArtifact> Items) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            DisposeArtifacts(Items);
        }
    }

    private sealed record PromotionArtifact(
        string Role,
        string WireFileName,
        string FullPath,
        long Length,
        string Sha256,
        FileStream ContentStream);

    private sealed record PromotionUploadRequestMetadata(
        string ContractVersion,
        string TenantId,
        string ManifestId,
        string MachineId,
        string DomainTag,
        string Version,
        string IdempotencyKey,
        int AcceptedRecordCount,
        int QuarantinedRecordCount,
        double? HeldOutScore,
        double? RequiredHeldOutScore,
        IReadOnlyList<PromotionUploadArtifactMetadata> Artifacts);

    private sealed record PromotionUploadArtifactMetadata(
        string Role,
        string FileName,
        long Length,
        string Sha256);

    private sealed record PromotionUploadResponse(
        string? UploadId,
        string? RemoteRef,
        DateTimeOffset? UploadedAtUtc,
        string? TenantId,
        string? ManifestId,
        string? MachineId,
        string? DomainTag,
        string? Version,
        string? IdempotencyKey,
        IReadOnlyList<PromotionUploadArtifactResponse>? Artifacts);

    private sealed record ValidatedPromotionUploadResponse(
        string UploadId,
        string RemoteRef,
        DateTimeOffset UploadedAtUtc);

    private sealed record PromotionUploadArtifactResponse(
        string? Role,
        long Length,
        string? Sha256);
}
