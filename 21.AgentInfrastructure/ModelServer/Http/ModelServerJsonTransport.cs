// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.Http;

using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;

/// <summary>
/// Shared safety gate for local-only ModelServer HTTP entrypoints.
/// </summary>
public static class ModelServerHttpEndpointSafety
{
    public const string LocalRootBaseUriRequirement =
        "ModelServer base URI must be http(s), loopback-only, root-only, and must not include userinfo, query, or fragment.";

    public const string RootedRelativeRouteRequirement =
        "ModelServer route must be a rooted relative path without authority, query, fragment, backslashes, or traversal segments.";

    public static bool IsSafeLocalRootBaseUri(Uri? baseUri)
        => TryNormalizeLocalRootBaseUri(baseUri, out _) is true;

    public static Uri? NormalizeLocalRootBaseUriOrNull(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            !Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri) ||
            !TryNormalizeLocalRootBaseUri(uri, out var normalized))
        {
            return null;
        }

        return normalized;
    }

    public static bool TryNormalizeLocalRootBaseUri(
        Uri? baseUri,
        out Uri? normalized)
    {
        normalized = null;
        if (baseUri is null ||
            !baseUri.IsAbsoluteUri ||
            !baseUri.IsLoopback ||
            !IsHttpScheme(baseUri) ||
            !string.IsNullOrEmpty(baseUri.UserInfo) ||
            !string.IsNullOrEmpty(baseUri.Query) ||
            !string.IsNullOrEmpty(baseUri.Fragment) ||
            !IsRootPath(baseUri))
        {
            return false;
        }

        var builder = new UriBuilder(baseUri)
        {
            Path = string.Empty,
            Query = string.Empty,
            Fragment = string.Empty,
            UserName = string.Empty,
            Password = string.Empty,
        };
        normalized = builder.Uri;
        return true;
    }

    public static bool IsSafeRootedRelativeRoute(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath) ||
            !StringComparer.Ordinal.Equals(relativePath, relativePath.Trim()) ||
            relativePath.Any(char.IsControl) ||
            !relativePath.StartsWith("/", StringComparison.Ordinal) ||
            relativePath.StartsWith("//", StringComparison.Ordinal) ||
            relativePath.Contains('\\', StringComparison.Ordinal) ||
            relativePath.Contains('?', StringComparison.Ordinal) ||
            relativePath.Contains('#', StringComparison.Ordinal) ||
            relativePath.Contains("://", StringComparison.Ordinal))
        {
            return false;
        }

        var segments = relativePath.Split(
            '/',
            StringSplitOptions.RemoveEmptyEntries);
        return segments.All(static segment =>
            segment is not "." and not "..");
    }

    private static bool IsHttpScheme(Uri uri)
        => string.Equals(
               uri.Scheme,
               Uri.UriSchemeHttp,
               StringComparison.OrdinalIgnoreCase) ||
           string.Equals(
               uri.Scheme,
               Uri.UriSchemeHttps,
               StringComparison.OrdinalIgnoreCase);

    private static bool IsRootPath(Uri uri)
        => string.IsNullOrEmpty(uri.AbsolutePath) ||
           string.Equals(uri.AbsolutePath, "/", StringComparison.Ordinal);
}

/// <summary>
/// Маленький JSON POST transport для клиентов, которым нужно вызвать
/// ModelServer, но не получить ссылку на сам проект 25.ModelServer.
/// </summary>
public interface IModelServerJsonTransport
{
    Task<OperationResult<ModelServerJsonPostResponse<TResponse>>>
        PostJsonAsync<TRequest, TResponse>(
            Uri baseUri,
            string relativePath,
            TRequest payload,
            TimeSpan timeout,
            CancellationToken cancellationToken = default)
        where TRequest : class
        where TResponse : class;
}

public interface ICallerLifetimeModelServerJsonTransport // Explicit opt-in, separate from every legacy bounded caller.
{ // Only a caller/host token controls this local operation's lifetime.
    Task<OperationResult<ModelServerJsonPostResponse<TResponse>>> PostJsonForCallerLifetimeAsync<TRequest, TResponse>(Uri baseUri, string relativePath, TRequest payload, CancellationToken cancellationToken = default) where TRequest : class where TResponse : class; // Preserve the same URI, JSON and response-size gates.
} // Existing transport implementers are not forced into unbounded semantics.

/// <summary>
/// Результат HTTP-обмена отделён от доменной успешности: сервер может
/// корректно ответить JSON-ошибкой, и доменный клиент решит, как её закрыть.
/// </summary>
public sealed record ModelServerJsonPostResponse<TResponse>(
    int StatusCode,
    bool IsSuccessStatusCode,
    TResponse? Body,
    string RawBody)
    where TResponse : class;

/// <summary>
/// Production transport. Он знает только HTTP/JSON и bounded timeout; все
/// права на модель, checkpoint и DPO остаются в доменном клиенте/сервере.
/// </summary>
public sealed class HttpClientModelServerJsonTransport : IModelServerJsonTransport, ICallerLifetimeModelServerJsonTransport
{
    public const string InvalidRequestErrorCode =
        "MODEL_SERVER_JSON_POST_REQUEST_INVALID";
    public const string UnavailableErrorCode =
        "MODEL_SERVER_JSON_POST_UNAVAILABLE";
    public const string TimeoutErrorCode =
        "MODEL_SERVER_JSON_POST_TIMEOUT";
    public const string CancelledErrorCode =
        "MODEL_SERVER_JSON_POST_CANCELLED";
    public const string InvalidJsonErrorCode =
        "MODEL_SERVER_JSON_POST_INVALID_JSON";
    public const string ResponseTooLargeErrorCode =
        "MODEL_SERVER_JSON_POST_RESPONSE_TOO_LARGE";

    public const int MaximumResponseByteCount = 1_048_576;
    public static readonly TimeSpan MaximumTimeout = TimeSpan.FromHours(24);

    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

    private readonly HttpClient _httpClient;

    public HttpClientModelServerJsonTransport(HttpClient? httpClient = null)
    {
        if (httpClient is not null)
        {
            _httpClient = httpClient;
            return;
        }

        // Важно для долгого обучения: у собственного HttpClient нет второго
        // 100-секундного таймера. Завершает только явный linked token ниже.
        _httpClient = new HttpClient
        {
            Timeout = Timeout.InfiniteTimeSpan
        };
    }

    public Task<OperationResult<ModelServerJsonPostResponse<TResponse>>>
        PostJsonAsync<TRequest, TResponse>(
            Uri baseUri,
            string relativePath,
            TRequest payload,
            TimeSpan timeout,
            CancellationToken cancellationToken = default)
        where TRequest : class
        where TResponse : class
        => PostCoreAsync<TRequest, TResponse>(baseUri, relativePath, payload, timeout, cancellationToken); // Preserve positive bounded timeout validation for all existing callers.

    public Task<OperationResult<ModelServerJsonPostResponse<TResponse>>> PostJsonForCallerLifetimeAsync<TRequest, TResponse>(Uri baseUri, string relativePath, TRequest payload, CancellationToken cancellationToken = default) where TRequest : class where TResponse : class // Name the new lifetime choice explicitly.
        => PostCoreAsync<TRequest, TResponse>(baseUri, relativePath, payload, null, cancellationToken); // No local deadline is inferred from manual evaluation profiles.

    private async Task<OperationResult<ModelServerJsonPostResponse<TResponse>>> PostCoreAsync<TRequest, TResponse>(Uri baseUri, string relativePath, TRequest payload, TimeSpan? timeout, CancellationToken cancellationToken) where TRequest : class where TResponse : class // Share every safety and response/body boundary across both lifetime policies.
    {
        if (!ModelServerHttpEndpointSafety.IsSafeLocalRootBaseUri(baseUri) ||
            !ModelServerHttpEndpointSafety.IsSafeRootedRelativeRoute(
                relativePath) ||
            payload is null ||
            (timeout is { } bounded && (bounded <= TimeSpan.Zero || bounded > MaximumTimeout || bounded == Timeout.InfiniteTimeSpan)) || // Keep all legacy timeout bounds unchanged.
            (timeout is null && _httpClient.Timeout != Timeout.InfiniteTimeSpan)) // Refuse an injected client with a hidden timer on the explicit caller-lifetime branch.
        {
            return OperationResult<ModelServerJsonPostResponse<TResponse>>
                .Fail(
                    "ModelServer JSON POST needs a local root HTTP(S) base URI, a rooted relative route, a payload and a positive bounded timeout.",
                    InvalidRequestErrorCode);
        }

        ModelServerHttpEndpointSafety.TryNormalizeLocalRootBaseUri(
            baseUri,
            out var normalizedBaseUri);
        using var timeoutSource =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        if (timeout is { } boundedTimeout) timeoutSource.CancelAfter(boundedTimeout); // Only legacy bounded calls arm a local deadline; both paths remain caller-cancellable.

        try
        {
            // Human reason: Uri composition is centralized here so feature
            // clients cannot accidentally smuggle a second server endpoint in
            // their domain DTOs.
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                new Uri(normalizedBaseUri!, relativePath))
            {
                Content = JsonContent.Create(payload, options: JsonOptions),
            };
            using var response = await _httpClient
                .SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    timeoutSource.Token)
                .ConfigureAwait(false);
            var rawBodyResult = await ReadAsStringBoundedAsync(
                    response.Content,
                    timeoutSource.Token)
                .ConfigureAwait(false);
            if (!rawBodyResult.IsSuccess || rawBodyResult.Value is null)
            {
                return OperationResult<ModelServerJsonPostResponse<TResponse>>
                    .Fail(
                        rawBodyResult.ErrorMessage,
                        rawBodyResult.ErrorCode);
            }

            var rawBody = rawBodyResult.Value;
            TResponse? body;
            try
            {
                body = string.IsNullOrWhiteSpace(rawBody)
                    ? null
                    : JsonSerializer.Deserialize<TResponse>(
                        rawBody,
                        JsonOptions);
            }
            catch (JsonException ex)
            {
                return OperationResult<ModelServerJsonPostResponse<TResponse>>
                    .Fail(
                        $"ModelServer returned non-contract JSON: {ex.Message}",
                        InvalidJsonErrorCode);
            }

            return OperationResult<ModelServerJsonPostResponse<TResponse>>.Ok(
                new ModelServerJsonPostResponse<TResponse>(
                    (int)response.StatusCode,
                    response.IsSuccessStatusCode,
                    body,
                    rawBody));
        }
        catch (OperationCanceledException) when
            (timeout is not null && !cancellationToken.IsCancellationRequested) // Only a bounded invocation can report an elapsed local deadline.
        {
            return OperationResult<ModelServerJsonPostResponse<TResponse>>
                .Fail(
                    "ModelServer JSON POST timed out before a contract response was received.",
                    TimeoutErrorCode);
        }
        catch (OperationCanceledException)
        {
            return OperationResult<ModelServerJsonPostResponse<TResponse>>
                .Fail(
                    "ModelServer JSON POST was cancelled before a contract response was received.",
                    CancelledErrorCode);
        }
        catch (HttpRequestException ex)
        {
            return OperationResult<ModelServerJsonPostResponse<TResponse>>
                .Fail(
                    $"ModelServer JSON POST was unavailable: {ex.Message}",
                    UnavailableErrorCode);
        }
        catch (IOException ex)
        {
            return OperationResult<ModelServerJsonPostResponse<TResponse>>
                .Fail(
                    $"ModelServer JSON POST response could not be read: {ex.Message}",
                    UnavailableErrorCode);
        }
        catch (InvalidOperationException ex)
        {
            return OperationResult<ModelServerJsonPostResponse<TResponse>>
                .Fail(
                    $"ModelServer JSON POST could not be sent: {ex.Message}",
                    InvalidRequestErrorCode);
        }
    }

    private static async Task<OperationResult<string>> ReadAsStringBoundedAsync(
        HttpContent content,
        CancellationToken cancellationToken)
    {
        if (content.Headers.ContentLength is long contentLength &&
            contentLength > MaximumResponseByteCount)
        {
            return ResponseTooLarge();
        }

        await using var stream = await content
            .ReadAsStreamAsync(cancellationToken)
            .ConfigureAwait(false);
        using var memory = new MemoryStream(
            content.Headers.ContentLength is long knownLength &&
            knownLength is > 0 and <= MaximumResponseByteCount
                ? (int)knownLength
                : 0);
        var buffer = new byte[Math.Min(81920, MaximumResponseByteCount + 1)];
        var total = 0;

        while (true)
        {
            var remainingProbeBytes = MaximumResponseByteCount + 1 - total;
            var readLength = Math.Min(buffer.Length, remainingProbeBytes);
            var read = await stream
                .ReadAsync(
                    buffer.AsMemory(0, readLength),
                    cancellationToken)
                .ConfigureAwait(false);
            if (read == 0)
                break;

            total += read;
            if (total > MaximumResponseByteCount)
                return ResponseTooLarge();

            memory.Write(buffer, 0, read);
        }

        return OperationResult<string>.Ok(
            Encoding.UTF8.GetString(memory.ToArray()));
    }

    private static OperationResult<string> ResponseTooLarge()
        => OperationResult<string>.Fail(
            "ModelServer JSON POST response exceeded the 1048576 byte contract limit before deserialization.",
            ResponseTooLargeErrorCode);
}
