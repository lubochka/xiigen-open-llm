// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("LlmFromScratch.Tests")]

namespace ContinuousLearning;

using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

/// <summary>
/// Performs one serialized, fail-closed Ollama chat sequence surrounded by
/// bounded observations of the configured local model manifest identity.
/// </summary>
internal sealed class LocalQwenOllamaTeacherInferenceClient
    : ITrainingTeacherInferenceClient, ILegalTeacherEvaluationInferenceClient
{
    internal const int MaximumPromptUtf8Bytes = 65_536;
    internal const int MaximumRequestBodyBytes = 524_288;
    internal const int MaximumResponseBodyBytes = 262_144;
    internal const int MaximumJsonDepth = 16;
    internal const int MaximumModelEntries = 1_024;
    internal const int MaximumConnectionsPerServer = 1;

    internal const string ConfigurationInvalidErrorCode =
        "LOCAL_QWEN_OLLAMA_CONFIGURATION_INVALID";
    internal const string PromptInvalidErrorCode =
        "LOCAL_QWEN_OLLAMA_PROMPT_INVALID";
    internal const string RequestTooLargeErrorCode =
        "LOCAL_QWEN_OLLAMA_REQUEST_TOO_LARGE";
    internal const string ResponseTooLargeErrorCode =
        "LOCAL_QWEN_OLLAMA_RESPONSE_TOO_LARGE";
    internal const string ResponseInvalidErrorCode =
        "LOCAL_QWEN_OLLAMA_RESPONSE_INVALID";
    internal const string TransportFailedErrorCode =
        "LOCAL_QWEN_OLLAMA_TRANSPORT_FAILED";
    internal const string TimeoutErrorCode =
        "LOCAL_QWEN_OLLAMA_TIMEOUT";
    internal const string DisposedErrorCode =
        "LOCAL_QWEN_OLLAMA_DISPOSED";
    internal const string CallPermitRejectedErrorCode =
        "LOCAL_QWEN_OLLAMA_CALL_PERMIT_REJECTED";
    internal const string ModelIdentityResponseInvalidErrorCode =
        "MODEL_IDENTITY_RESPONSE_INVALID";
    internal const string ModelIdentityUnavailableErrorCode =
        "MODEL_IDENTITY_UNAVAILABLE";
    internal const string ModelIdentityMismatchErrorCode =
        "MODEL_IDENTITY_MISMATCH";
    internal const string ModelIdentityChangedErrorCode =
        "MODEL_IDENTITY_CHANGED";

    internal static readonly Uri TagsEndpoint =
        new("http://127.0.0.1:11434/api/tags", UriKind.Absolute);
    internal static readonly Uri ChatEndpoint =
        new("http://127.0.0.1:11434/api/chat", UriKind.Absolute);
    internal static readonly Uri RunningModelsEndpoint =
        new("http://127.0.0.1:11434/api/ps", UriKind.Absolute);
    internal static readonly Uri ShowEndpoint =
        new("http://127.0.0.1:11434/api/show", UriKind.Absolute);
    internal static readonly Uri Endpoint = ChatEndpoint;

    internal static readonly TimeSpan DefaultConnectTimeout =
        TimeSpan.FromSeconds(5);
    internal static readonly TimeSpan DefaultRequestTimeout =
        TimeSpan.FromSeconds(30);
    internal static readonly TimeSpan DefaultEvaluationRequestTimeout =
        TimeSpan.FromSeconds(120);

    private static readonly UTF8Encoding StrictUtf8 =
        new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    private readonly ContinuousLearningTp1DpoConfiguration _configuration;
    private readonly TimeSpan _authoringRequestTimeout;
    private readonly TimeSpan _evaluationRequestTimeout;
    private Func<string, CancellationToken, OperationResult<int[]>>? _syntheticCpuTokenCountForTesting; // Only CreateForTesting sets this asset-free counter; production always uses the retained native vocabulary owner.
    private readonly Func<TimeSpan, CancellationTokenSource> _deadlineFactory;
    private readonly object _stateGate = new();
    private readonly CancellationTokenSource _lifetimeCancellation = new();
    private readonly SemaphoreSlim _sequenceGate = new(1, 1);
    private HttpClient? _httpClient;
    private int _activeLeaseCount;
    private bool _disposed;
    private bool _lifetimeCancellationIssued;
    private bool _cleanupStarted;

    private LocalQwenOllamaTeacherInferenceClient(
        ContinuousLearningTp1DpoConfiguration configuration,
        HttpClient httpClient,
        TimeSpan authoringRequestTimeout,
        TimeSpan evaluationRequestTimeout,
        Func<TimeSpan, CancellationTokenSource> deadlineFactory)
    {
        _configuration = configuration;
        _httpClient = httpClient;
        _authoringRequestTimeout = authoringRequestTimeout;
        _evaluationRequestTimeout = evaluationRequestTimeout;
        _deadlineFactory = deadlineFactory;
    }

    internal static OperationResult<LocalQwenOllamaTeacherInferenceClient> Create(
        ContinuousLearningTp1DpoConfiguration configuration)
    {
        if (configuration is null)
            return ConfigurationFailure();

        var authority = configuration.CreateTeacherPolicyBudgetAuthority();
        if (!authority.IsSuccess || authority.Value is null)
            return ConfigurationFailure();

        return OperationResult<LocalQwenOllamaTeacherInferenceClient>.Ok(
            CreateOwnedClient(
                configuration,
                CreateSecureSocketsHttpHandler(),
                disposeHandler: true,
                configuration.AuthoringDescriptor.RequestTimeout, // Production deadline is the same immutable canonical profile used to serialize and attest the request.
                configuration.EvaluationDescriptor.RequestTimeout,
                CreateProductionDeadline));
    }

    internal async Task<OperationResult<TrainingTeacherInferenceResult>> InferAsync(
        string prompt,
        ITrainingTeacherCallPermit callPermit,
        CancellationToken cancellationToken = default)
    {
        var result = await InferCoreAsync(
                prompt,
                callPermit is null ? null : callPermit.TryIssue,
                TeacherInferencePurpose.TrainingAuthoring,
                cancellationToken)
            .ConfigureAwait(false);
        if (!result.IsSuccess || result.Value is null)
            return result.MapError<TrainingTeacherInferenceResult>();

        return OperationResult<TrainingTeacherInferenceResult>.Ok(
            new TrainingTeacherInferenceResult(
                result.Value.Output,
                result.Value.Provenance)
            {
                InferenceProfile = result.Value.InferenceProfile
            });
    }

    TrainingTeacherInferenceProfileEvidence? ITrainingTeacherInferenceClient.AuthoringProfile => _configuration.AuthoringProfile; // Consumers can refuse a mismatched CPU factory before a call permit or request is issued.
    TrainingTeacherInferenceProfileEvidence? ILegalTeacherEvaluationInferenceClient.EvaluationProfile => _configuration.EvaluationProfile;

    internal async Task<OperationResult<TeacherEvaluationInferenceResult>> EvaluateAsync(
        string prompt,
        ILegalTeacherCallPermit callPermit,
        CancellationToken cancellationToken = default)
    {
        var result = await InferCoreAsync(
                prompt,
                callPermit is null ? null : callPermit.TryIssue,
                TeacherInferencePurpose.EvaluationOnly,
                cancellationToken)
            .ConfigureAwait(false);
        if (!result.IsSuccess || result.Value is null)
            return result.MapError<TeacherEvaluationInferenceResult>();

        return OperationResult<TeacherEvaluationInferenceResult>.Ok(
            new TeacherEvaluationInferenceResult(
                result.Value.Output,
                result.Value.Provenance)
            {
                InferenceProfile = result.Value.InferenceProfile
            });
    }

    private async Task<OperationResult<LocalTeacherInferenceResult>> InferCoreAsync(
        string prompt,
        Func<OperationResult<int>>? issueCall,
        TeacherInferencePurpose purpose,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (issueCall is null)
            return PermitFailure<LocalTeacherInferenceResult>(errorCode: null);
        if (IsDisposed())
        {
            return Failure<LocalTeacherInferenceResult>(
                DisposedErrorCode,
                "The local teacher client is unavailable.");
        }

        var promptValidation = ValidatePrompt(prompt);
        if (!promptValidation.IsSuccess)
            return promptValidation.MapError<LocalTeacherInferenceResult>();

        TrainingTeacherProvenance provenance;
        OperationResult<LegalTrainingTeacherAdmissionAuthority> authority;
        OperationResult<byte[]> requestBody;
        LocalQwenOllamaInferenceProfileDescriptor inferenceProfile;
        try
        {
            provenance = CreateProvenance(_configuration, purpose);
            authority = _configuration.CreateTeacherPolicyBudgetAuthority();
            inferenceProfile = purpose == TeacherInferencePurpose.TrainingAuthoring
                ? _configuration.AuthoringDescriptor // Explicit startup selection changes only authoring, preserving frozen evaluation.
                : _configuration.EvaluationDescriptor;
            requestBody = BuildRequestBody(
                _configuration.ModelId,
                prompt,
                inferenceProfile);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return ConfigurationFailure<LocalTeacherInferenceResult>();
        }

        if (!authority.IsSuccess || authority.Value is null)
            return ConfigurationFailure<LocalTeacherInferenceResult>();
        var preflight = ValidatePurpose(authority.Value, provenance, purpose);
        if (!preflight.IsSuccess)
        {
            return OperationResult<LocalTeacherInferenceResult>.Fail(
                "The local teacher request was denied by the legal admission policy.",
                preflight.ErrorCode);
        }
        if (!requestBody.IsSuccess || requestBody.Value is null)
            return requestBody.MapError<LocalTeacherInferenceResult>();

        var cpuAuthoring = purpose == TeacherInferencePurpose.TrainingAuthoring && inferenceProfile.BindCpuAuthoringParameters; // Evaluation never enters CPU preparation even when authoring opted in.
        var vocabularyAdmission = cpuAuthoring && _syntheticCpuTokenCountForTesting is null // The normal client cannot infer fit from bytes, caller options or a synthetic receipt.
            ? ConfiguredQwenVocabularyLease.OpenInstalledWindows(_configuration, cancellationToken) : null; // Reuse the existing exact model/renderer/vocabulary owner without creating an inference context.
        if (vocabularyAdmission is not null && (!vocabularyAdmission.IsSuccess || vocabularyAdmission.Value is null)) // A missing native control blocks only the selected CPU effect.
            return vocabularyAdmission.MapError<LocalTeacherInferenceResult>(); // Preserve the precise configured-resource refusal before the permit or HTTP probes.
        using var vocabulary = vocabularyAdmission?.Value; // Hold the genuine resource throughout counting, call and final identity validation.
        if (cpuAuthoring) // Refuse overflow of the entire prompt plus output reserve rather than silently truncating task, reference or history.
        { // No legacy authoring or evaluation request changes its body or preparation behavior.
            try // Canonical JSON validation may reject malformed source text before any effect.
            { // Build the exact prepared body using the same selected profile as this call.
                var contentSha = Convert.ToHexString(SHA256.HashData(StrictUtf8.GetBytes(prompt))).ToLowerInvariant(); // Bind the complete unsent prompt; this digest grants no source rights.
                var prepared = PreparedAuthoringRequest.Prepare(prompt, contentSha, "ServerOwnedAuthoring", _configuration, vocabulary, _syntheticCpuTokenCountForTesting, cancellationToken); // Testing retains SyntheticTest origin; production requires NativeConfigured counting.
                if (!prepared.IsSuccess || prepared.Value is null) return prepared.MapError<LocalTeacherInferenceResult>(); // No permit or HTTP request is issued for a non-fitting context.
                if (prepared.Value.Profile != inferenceProfile.Evidence || !prepared.Value.Matches(contentSha, _configuration)) return ConfigurationFailure<LocalTeacherInferenceResult>(); // Refuse profile/body evidence drift before dispatch.
                requestBody = OperationResult<byte[]>.Ok(prepared.Value.CopyBody()); // Send exactly the body admitted by the existing full-prompt tokenizer owner.
            } // The retained vocabulary still protects native identity until this operation returns.
            catch (OperationCanceledException) { throw; } // Preserve the caller's existing cancellation contract.
            catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)) // Invalid JSON or encoding is a bounded pre-effect refusal.
            { return ConfigurationFailure<LocalTeacherInferenceResult>(); } // Never fall back to an uncounted CPU request.
        } // Complete selected CPU preparation before constructing its deadline.

        CancellationTokenSource deadline;
        try
        {
            var requestTimeout = purpose == TeacherInferencePurpose.EvaluationOnly
                ? _evaluationRequestTimeout
                : _authoringRequestTimeout;
            // Manual profiles have no timer; caller and client lifetime remain linked below.
            deadline = inferenceProfile.CallerCancellationOnly
                ? new CancellationTokenSource()
                : _deadlineFactory(requestTimeout);
            if (deadline is null)
                return ConfigurationFailure<LocalTeacherInferenceResult>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return ConfigurationFailure<LocalTeacherInferenceResult>();
        }

        using (deadline)
        {
            var reservation = TryAcquireActiveLease(
                cancellationToken,
                deadline.Token,
                out var activeLease);
            if (reservation != ActiveLeaseStatus.Acquired)
                return ActiveLeaseFailure<LocalTeacherInferenceResult>(reservation);

            using var lease = activeLease!;
            using var linkedCancellation = CancellationTokenSource
                .CreateLinkedTokenSource(
                    cancellationToken,
                    lease.LifetimeToken,
                    deadline.Token);
            var sequenceEntered = false;
            try
            {
                await _sequenceGate.WaitAsync(linkedCancellation.Token)
                    .ConfigureAwait(false);
                sequenceEntered = true;

                var preIdentity = await ObserveIdentityAsync(
                    lease.HttpClient,
                    TagsEndpoint,
                    "models",
                    ModelIdentityMismatchErrorCode,
                    linkedCancellation.Token).ConfigureAwait(false);
                if (!preIdentity.IsSuccess)
                    return preIdentity.MapError<LocalTeacherInferenceResult>();

                var preLayerIdentity = await ObserveModelLayerIdentityAsync(
                    lease.HttpClient,
                    ModelIdentityMismatchErrorCode,
                    linkedCancellation.Token).ConfigureAwait(false);
                if (!preLayerIdentity.IsSuccess)
                    return preLayerIdentity.MapError<LocalTeacherInferenceResult>();

                var cancellationFailure = CheckFinalCancellation<LocalTeacherInferenceResult>(
                    cancellationToken,
                    lease.LifetimeToken,
                    deadline.Token);
                if (cancellationFailure is not null)
                    return cancellationFailure;

                using var chatRequest = CreateChatRequest(requestBody.Value);
                OperationResult<int>? issued;
                try
                {
                    issued = issueCall();
                }
                catch (Exception exception)
                    when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
                {
                    return PermitFailure<LocalTeacherInferenceResult>(errorCode: null);
                }
                if (issued is null || !issued.IsSuccess)
                    return PermitFailure<LocalTeacherInferenceResult>(issued?.ErrorCode);

                using var chatResponse = await lease.HttpClient.SendAsync(
                    chatRequest,
                    HttpCompletionOption.ResponseHeadersRead,
                    linkedCancellation.Token).ConfigureAwait(false);
                var output = await ReadChatResponseAsync(
                    chatResponse,
                    linkedCancellation.Token).ConfigureAwait(false);
                if (!output.IsSuccess || output.Value is null)
                    return output.MapError<LocalTeacherInferenceResult>();

                var runningIdentity = await ObserveIdentityAsync(
                    lease.HttpClient,
                    RunningModelsEndpoint,
                    "models",
                    ModelIdentityChangedErrorCode,
                    linkedCancellation.Token, allowUnloadedCpuModel: cpuAuthoring).ConfigureAwait(false); // keep_alive:0 permits only an empty post-chat running list, not absent preflight identity.
                if (!runningIdentity.IsSuccess)
                    return runningIdentity.MapError<LocalTeacherInferenceResult>();

                var postIdentity = await ObserveIdentityAsync(
                    lease.HttpClient,
                    TagsEndpoint,
                    "models",
                    ModelIdentityChangedErrorCode,
                    linkedCancellation.Token).ConfigureAwait(false);
                if (!postIdentity.IsSuccess)
                    return postIdentity.MapError<LocalTeacherInferenceResult>();

                var postLayerIdentity = await ObserveModelLayerIdentityAsync(
                    lease.HttpClient,
                    ModelIdentityChangedErrorCode,
                    linkedCancellation.Token).ConfigureAwait(false);
                if (!postLayerIdentity.IsSuccess)
                    return postLayerIdentity.MapError<LocalTeacherInferenceResult>();

                var retainedVocabulary = vocabulary?.ValidateCurrent(linkedCancellation.Token); // Native counted resources must still match after the completed chat and stable tags/show observations.
                if (retainedVocabulary is not null && !retainedVocabulary.IsSuccess) return retainedVocabulary.MapError<LocalTeacherInferenceResult>(); // Withhold output if the genuine tokenizer resource lost identity.

                var returnedProvenance = ValidatePurpose(
                    authority.Value,
                    provenance,
                    purpose);
                if (!returnedProvenance.IsSuccess)
                {
                    return OperationResult<LocalTeacherInferenceResult>.Fail(
                        "The local teacher response failed legal provenance validation.",
                        returnedProvenance.ErrorCode);
                }

                return OperationResult<LocalTeacherInferenceResult>.Ok(
                    new LocalTeacherInferenceResult(
                        output.Value,
                        provenance,
                        inferenceProfile.Evidence));
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (OperationCanceledException)
                when (lease.LifetimeToken.IsCancellationRequested || IsDisposed())
            {
                return Failure<LocalTeacherInferenceResult>(
                    DisposedErrorCode,
                    "The local teacher client is unavailable.");
            }
            catch (OperationCanceledException)
                when (deadline.Token.IsCancellationRequested)
            {
                return Failure<LocalTeacherInferenceResult>(
                    TimeoutErrorCode,
                    "The local teacher request timed out.");
            }
            catch (OperationCanceledException)
            {
                return Failure<LocalTeacherInferenceResult>(
                    TransportFailedErrorCode,
                    "The local teacher request failed.");
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                if (IsDisposed())
                {
                    return Failure<LocalTeacherInferenceResult>(
                        DisposedErrorCode,
                        "The local teacher client is unavailable.");
                }

                return Failure<LocalTeacherInferenceResult>(
                    TransportFailedErrorCode,
                    "The local teacher request failed.");
            }
            finally
            {
                if (sequenceEntered)
                    _sequenceGate.Release();
            }
        }
    }

    Task<OperationResult<TrainingTeacherInferenceResult>>
        ITrainingTeacherInferenceClient.InferAsync(
            string prompt,
            ITrainingTeacherCallPermit callPermit,
            CancellationToken cancellationToken)
        => InferAsync(prompt, callPermit, cancellationToken);

    Task<OperationResult<TeacherEvaluationInferenceResult>>
        ILegalTeacherEvaluationInferenceClient.EvaluateAsync(
            string prompt,
            ILegalTeacherCallPermit callPermit,
            CancellationToken cancellationToken)
        => EvaluateAsync(prompt, callPermit, cancellationToken);

    internal void Dispose()
    {
        lock (_stateGate)
        {
            if (_disposed)
                return;
            _disposed = true;
        }

        HttpClient? cleanupClient = null;
        try
        {
            try
            {
                _lifetimeCancellation.Cancel();
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                // Disposal remains best-effort even if an injected callback fails.
            }
        }
        finally
        {
            lock (_stateGate)
            {
                _lifetimeCancellationIssued = true;
                cleanupClient = TryClaimCleanupLocked();
            }
            DisposeCleanup(cleanupClient);
        }
    }

    void IDisposable.Dispose() => Dispose();

    internal static OperationResult<LocalQwenOllamaTeacherInferenceClient>
        CreateForTesting(
            ContinuousLearningTp1DpoConfiguration configuration,
            HttpMessageHandler handler,
            bool disposeHandler,
            TimeSpan? requestTimeout = null,
            Func<TimeSpan, CancellationTokenSource>? deadlineFactory = null,
            TimeSpan? evaluationRequestTimeout = null, // Keep all existing test call sites source-compatible.
            Func<string, CancellationToken, OperationResult<int[]>>? syntheticCpuTokenCountForTesting = null) // Asset-free tests may simulate counting only in this explicitly synthetic transport factory.
    {
        if (configuration is null || handler is null)
            return ConfigurationFailure();

        var timeout = requestTimeout ?? configuration.AuthoringDescriptor.RequestTimeout; // A CPU test observes the same canonical production deadline.
        var evaluationTimeout = evaluationRequestTimeout ?? configuration.EvaluationDescriptor.RequestTimeout;
        if (!(configuration.AuthoringDescriptor.CallerCancellationOnly
                ? timeout == Timeout.InfiniteTimeSpan
                : timeout > TimeSpan.Zero && timeout <= (configuration.AuthoringDescriptor.BindCpuAuthoringParameters ? configuration.AuthoringDescriptor.RequestTimeout : TimeSpan.FromMinutes(2)) &&
                  (!configuration.AuthoringDescriptor.BindCpuAuthoringParameters || timeout == configuration.AuthoringDescriptor.RequestTimeout)) ||
            !(configuration.EvaluationDescriptor.CallerCancellationOnly
                ? evaluationTimeout == Timeout.InfiniteTimeSpan
                : evaluationTimeout > TimeSpan.Zero && evaluationTimeout <= TimeSpan.FromMinutes(2)))
            return ConfigurationFailure();
        var authority = configuration.CreateTeacherPolicyBudgetAuthority();
        if (!authority.IsSuccess || authority.Value is null)
            return ConfigurationFailure();

        var client = CreateOwnedClient( // Only this internal testing factory can attach a synthetic token counter.
                configuration,
                handler,
                disposeHandler,
                timeout,
                evaluationTimeout,
                deadlineFactory ?? CreateProductionDeadline); // Production Create leaves the synthetic counter null.
        client._syntheticCpuTokenCountForTesting = syntheticCpuTokenCountForTesting; // This test counter never issues NativeConfigured resource or material admission.
        return OperationResult<LocalQwenOllamaTeacherInferenceClient>.Ok(client); // Return the existing synthetic transport surface with its bounded test-only counter.
    }

    internal static SocketsHttpHandler CreateSecureSocketsHttpHandler()
        => new()
        {
            UseProxy = false,
            Proxy = null,
            AllowAutoRedirect = false,
            UseCookies = false,
            Credentials = null,
            DefaultProxyCredentials = null,
            AutomaticDecompression = DecompressionMethods.None,
            ConnectTimeout = DefaultConnectTimeout,
            MaxConnectionsPerServer = MaximumConnectionsPerServer,
            PooledConnectionLifetime = TimeSpan.FromMinutes(2),
            PooledConnectionIdleTimeout = TimeSpan.FromSeconds(30)
        };

    private static LocalQwenOllamaTeacherInferenceClient CreateOwnedClient(
        ContinuousLearningTp1DpoConfiguration configuration,
        HttpMessageHandler handler,
        bool disposeHandler,
        TimeSpan authoringRequestTimeout,
        TimeSpan evaluationRequestTimeout,
        Func<TimeSpan, CancellationTokenSource> deadlineFactory)
    {
        var httpClient = new HttpClient(handler, disposeHandler)
        {
            Timeout = Timeout.InfiniteTimeSpan
        };
        return new LocalQwenOllamaTeacherInferenceClient(
            configuration,
            httpClient,
            authoringRequestTimeout,
            evaluationRequestTimeout,
            deadlineFactory);
    }

    private ActiveLeaseStatus TryAcquireActiveLease(
        CancellationToken callerCancellation,
        CancellationToken deadlineCancellation,
        out ActiveLease? activeLease)
    {
        lock (_stateGate)
        {
            callerCancellation.ThrowIfCancellationRequested();
            if (_disposed || _httpClient is null)
            {
                activeLease = null;
                return ActiveLeaseStatus.Disposed;
            }
            if (deadlineCancellation.IsCancellationRequested)
            {
                activeLease = null;
                return ActiveLeaseStatus.DeadlineExpired;
            }

            _activeLeaseCount++;
            activeLease = new ActiveLease(
                this,
                _httpClient,
                _lifetimeCancellation.Token);
            return ActiveLeaseStatus.Acquired;
        }
    }

    private bool IsDisposed()
    {
        lock (_stateGate)
            return _disposed;
    }

    private void ReleaseActiveLease()
    {
        HttpClient? cleanupClient;
        lock (_stateGate)
        {
            _activeLeaseCount--;
            cleanupClient = TryClaimCleanupLocked();
        }
        DisposeCleanup(cleanupClient);
    }

    private HttpClient? TryClaimCleanupLocked()
    {
        if (!_disposed
            || !_lifetimeCancellationIssued
            || _activeLeaseCount != 0
            || _cleanupStarted)
        {
            return null;
        }

        _cleanupStarted = true;
        var httpClient = _httpClient!;
        _httpClient = null;
        return httpClient;
    }

    private void DisposeCleanup(HttpClient? httpClient)
    {
        if (httpClient is null)
            return;

        try
        {
            httpClient.Dispose();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            // Cleanup does not expose transport diagnostics.
        }
        try
        {
            _sequenceGate.Dispose();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            // Cleanup is exactly once even if a resource rejects disposal.
        }
        try
        {
            _lifetimeCancellation.Dispose();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            // Cleanup is exactly once even if a resource rejects disposal.
        }
    }

    private static CancellationTokenSource CreateProductionDeadline(TimeSpan timeout)
        => new(timeout);

    private static OperationResult<T>? CheckFinalCancellation<T>(
            CancellationToken callerCancellation,
            CancellationToken lifetimeCancellation,
            CancellationToken deadlineCancellation)
    {
        callerCancellation.ThrowIfCancellationRequested();
        if (lifetimeCancellation.IsCancellationRequested)
        {
            return Failure<T>(
                DisposedErrorCode,
                "The local teacher client is unavailable.");
        }
        if (deadlineCancellation.IsCancellationRequested)
        {
            return Failure<T>(
                TimeoutErrorCode,
                "The local teacher request timed out.");
        }
        return null;
    }

    private static OperationResult<T> ActiveLeaseFailure<T>(ActiveLeaseStatus status)
        => status switch
        {
            ActiveLeaseStatus.Disposed => Failure<T>(
                DisposedErrorCode,
                "The local teacher client is unavailable."),
            ActiveLeaseStatus.DeadlineExpired => Failure<T>(
                TimeoutErrorCode,
                "The local teacher request timed out."),
            _ => Failure<T>(
                TransportFailedErrorCode,
                "The local teacher request failed.")
        };

    private static OperationResult<bool> ValidatePurpose(
        LegalTrainingTeacherAdmissionAuthority authority,
        TrainingTeacherProvenance provenance,
        TeacherInferencePurpose purpose)
    {
        if (purpose == TeacherInferencePurpose.EvaluationOnly)
            return authority.ValidateLocalEvaluationOutput(provenance);

        if (purpose != TeacherInferencePurpose.TrainingAuthoring)
        {
            return OperationResult<bool>.Fail(
                "The local teacher inference purpose is invalid.",
                ConfigurationInvalidErrorCode);
        }

        var admission = authority.ValidateBatch([provenance]);
        return admission.IsSuccess
            ? OperationResult<bool>.Ok(true)
            : OperationResult<bool>.Fail(
                "The local teacher provenance was rejected.",
                admission.ErrorCode);
    }

    private enum ActiveLeaseStatus
    {
        Acquired,
        Disposed,
        DeadlineExpired
    }

    private sealed class ActiveLease(
        LocalQwenOllamaTeacherInferenceClient owner,
        HttpClient httpClient,
        CancellationToken lifetimeToken)
        : IDisposable
    {
        private LocalQwenOllamaTeacherInferenceClient? _owner = owner;

        internal HttpClient HttpClient { get; } = httpClient;
        internal CancellationToken LifetimeToken { get; } = lifetimeToken;

        public void Dispose()
            => Interlocked.Exchange(ref _owner, null)?.ReleaseActiveLease();
    }

    private async Task<OperationResult<bool>> ObserveModelLayerIdentityAsync(
        HttpClient httpClient,
        string mismatchErrorCode,
        CancellationToken cancellationToken)
    {
        try
        {
            var requestBody = BuildModelShowRequestBody(_configuration.ModelId);
            if (!requestBody.IsSuccess || requestBody.Value is null)
                return IdentityFailure(ModelIdentityResponseInvalidErrorCode);

            using var request = new HttpRequestMessage(HttpMethod.Post, ShowEndpoint)
            {
                Content = new ByteArrayContent(requestBody.Value)
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json")
            {
                CharSet = "utf-8"
            };
            using var response = await httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return IdentityFailure(ModelIdentityUnavailableErrorCode);
            if (!HasJsonContentType(response.Content.Headers.ContentType))
                return IdentityFailure(ModelIdentityResponseInvalidErrorCode);

            var body = await ReadBoundedResponseAsync(
                response.Content,
                ModelIdentityResponseInvalidErrorCode,
                ModelIdentityResponseInvalidErrorCode,
                cancellationToken).ConfigureAwait(false);
            if (!body.IsSuccess || body.Value is null)
                return body.MapError<bool>();
            return ValidateModelLayerIdentity(body.Value, mismatchErrorCode);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return IdentityFailure(ModelIdentityUnavailableErrorCode);
        }
    }

    private OperationResult<bool> ValidateModelLayerIdentity(
        byte[] responseBody,
        string mismatchErrorCode)
    {
        try
        {
            var json = StrictUtf8.GetString(responseBody);
            using var document = JsonDocument.Parse(
                json,
                new JsonDocumentOptions
                {
                    AllowTrailingCommas = false,
                    CommentHandling = JsonCommentHandling.Disallow,
                    MaxDepth = MaximumJsonDepth
                });
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object
                || !TryGetSingleString(root, "modelfile", out var modelfile)
                || !TryExtractSingleModelLayerDigest(modelfile, out var digest))
            {
                return IdentityFailure(ModelIdentityResponseInvalidErrorCode);
            }

            return StringComparer.Ordinal.Equals(
                    digest,
                    _configuration.ExpectedOllamaModelLayerDigest)
                ? OperationResult<bool>.Ok(true)
                : IdentityFailure(mismatchErrorCode);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return IdentityFailure(ModelIdentityResponseInvalidErrorCode);
        }
    }

    private static bool TryExtractSingleModelLayerDigest(
        string modelfile,
        out string digest)
    {
        digest = string.Empty;
        string? found = null;
        foreach (var rawLine in modelfile.Split('\n'))
        {
            var line = rawLine.EndsWith('\r') ? rawLine[..^1] : rawLine;
            var leftTrimmed = line.TrimStart(' ', '\t');
            if (leftTrimmed.StartsWith('#'))
                continue;

            if (!leftTrimmed.StartsWith("FROM", StringComparison.OrdinalIgnoreCase))
                continue;
            if (!StringComparer.Ordinal.Equals(line, leftTrimmed)
                || !line.StartsWith("FROM ", StringComparison.Ordinal)
                || found is not null)
            {
                return false;
            }

            var modelPath = line[5..];
            if (!TryExtractModelLayerDigestFromAbsolutePath(modelPath, out found))
                return false;
        }

        if (found is null)
            return false;
        digest = found;
        return true;
    }

    private static bool TryExtractModelLayerDigestFromAbsolutePath(
        string modelPath,
        out string digest)
    {
        digest = string.Empty;
        if (string.IsNullOrEmpty(modelPath)
            || !StringComparer.Ordinal.Equals(modelPath, modelPath.Trim())
            || modelPath.Any(char.IsControl))
        {
            return false;
        }

        var isUnixAbsolute = modelPath.StartsWith('/');
        var isDriveAbsolute = modelPath.Length >= 4
            && char.IsAsciiLetter(modelPath[0])
            && modelPath[1] == ':'
            && modelPath[2] is '\\' or '/';
        var isUncAbsolute = modelPath.StartsWith("\\\\", StringComparison.Ordinal);
        if (!isUnixAbsolute && !isDriveAbsolute && !isUncAbsolute)
            return false;

        const string forwardBlobSuffix = "/blobs/sha256-";
        const string backwardBlobSuffix = "\\blobs\\sha256-";
        var requiredBlobSuffix = isUnixAbsolute
            ? forwardBlobSuffix
            : isUncAbsolute || modelPath[2] == '\\'
                ? backwardBlobSuffix
                : forwardBlobSuffix;
        if (modelPath.Length < requiredBlobSuffix.Length + 64)
            return false;

        var digestIndex = modelPath.Length - 64;
        if (!modelPath.AsSpan(0, digestIndex).EndsWith(
                requiredBlobSuffix,
                StringComparison.Ordinal))
        {
            return false;
        }
        if (isUncAbsolute
            && !HasCanonicalUncBlobPrefix(
                modelPath,
                digestIndex - requiredBlobSuffix.Length))
        {
            return false;
        }

        var candidate = modelPath[digestIndex..];
        if (!ContinuousLearningTp1DpoConfiguration
                .IsCanonicalOllamaModelLayerDigest(candidate))
        {
            return false;
        }
        digest = candidate;
        return true;
    }

    private static bool HasCanonicalUncBlobPrefix(
        string modelPath,
        int blobSuffixIndex)
    {
        if (blobSuffixIndex <= 2
            || modelPath.Length < 3
            || modelPath[2] == '\\'
            || modelPath.Contains('/'))
        {
            return false;
        }

        var components = modelPath[2..blobSuffixIndex].Split('\\');
        if (components.Length < 2)
            return false;
        return components.All(static component =>
            !string.IsNullOrEmpty(component)
            && component is not "." and not ".."
            && StringComparer.Ordinal.Equals(component, component.Trim())
            && !component.Any(static character =>
                character is '<' or '>' or ':' or '"' or '|' or '?' or '*'));
    }

    private async Task<OperationResult<bool>> ObserveIdentityAsync(
        HttpClient httpClient,
        Uri endpoint,
        string collectionProperty,
        string mismatchErrorCode,
        CancellationToken cancellationToken, // Keep every existing identity observation fail closed by default.
        bool allowUnloadedCpuModel = false) // Only the completed CPU chat's /api/ps observation may report an already unloaded model.
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            using var response = await httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return IdentityFailure(ModelIdentityUnavailableErrorCode);
            if (!HasJsonContentType(response.Content.Headers.ContentType))
                return IdentityFailure(ModelIdentityResponseInvalidErrorCode);

            var body = await ReadBoundedResponseAsync(
                response.Content,
                ModelIdentityResponseInvalidErrorCode,
                ModelIdentityResponseInvalidErrorCode,
                cancellationToken).ConfigureAwait(false);
            if (!body.IsSuccess || body.Value is null)
                return body.MapError<bool>();
            return ValidateIdentityObservation(
                body.Value,
                collectionProperty,
                mismatchErrorCode, // Model entries still require exact target identity and canonical digest.
                allowUnloadedCpuModel && endpoint == RunningModelsEndpoint); // The exception cannot relax pre/post /api/tags identity.
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return IdentityFailure(ModelIdentityUnavailableErrorCode);
        }
    }

    private OperationResult<bool> ValidateIdentityObservation(
        byte[] responseBody,
        string collectionProperty,
        string mismatchErrorCode, // Keep mismatched or malformed running entries rejected even under CPU unload policy.
        bool allowUnloadedCpuModel = false) // An empty array is the only additional post-chat accepted observation.
    {
        try
        {
            var json = StrictUtf8.GetString(responseBody);
            using var document = JsonDocument.Parse(
                json,
                new JsonDocumentOptions
                {
                    AllowTrailingCommas = false,
                    CommentHandling = JsonCommentHandling.Disallow,
                    MaxDepth = MaximumJsonDepth
                });
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object
                || !TryGetSingleProperty(root, collectionProperty, out var models)
                || models.ValueKind != JsonValueKind.Array)
            {
                return IdentityFailure(ModelIdentityResponseInvalidErrorCode);
            }

            var targetCount = 0;
            var modelCount = 0;
            foreach (var model in models.EnumerateArray())
            {
                modelCount++;
                if (modelCount > MaximumModelEntries
                    || model.ValueKind != JsonValueKind.Object
                    || !TryGetSingleString(model, "name", out var name)
                    || !TryGetSingleString(model, "model", out var modelName)
                    || !TryGetSingleString(model, "digest", out var digest)
                    || !IsSafeObservedModelName(name)
                    || !IsSafeObservedModelName(modelName)
                    || !ContinuousLearningTp1DpoConfiguration
                        .IsCanonicalOllamaManifestDigest(digest))
                {
                    return IdentityFailure(ModelIdentityResponseInvalidErrorCode);
                }

                var nameTargets = StringComparer.Ordinal.Equals(
                    name,
                    _configuration.ModelId);
                var modelTargets = StringComparer.Ordinal.Equals(
                    modelName,
                    _configuration.ModelId);
                if (!nameTargets && !modelTargets)
                    continue;
                if (!nameTargets || !modelTargets)
                    return IdentityFailure(mismatchErrorCode);

                targetCount++;
                if (targetCount != 1
                    || !StringComparer.Ordinal.Equals(
                        digest,
                        _configuration.ExpectedOllamaManifestDigest))
                {
                    return IdentityFailure(mismatchErrorCode);
                }
            }

            return targetCount == 1 || allowUnloadedCpuModel && modelCount == 0 // A nonempty foreign-only list never proves the configured CPU model.
                ? OperationResult<bool>.Ok(true)
                : IdentityFailure(ModelIdentityUnavailableErrorCode);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return IdentityFailure(ModelIdentityResponseInvalidErrorCode);
        }
    }

    private static bool TryGetSingleString(
        JsonElement element,
        string propertyName,
        out string value)
    {
        value = string.Empty;
        if (!TryGetSingleProperty(element, propertyName, out var property)
            || property.ValueKind != JsonValueKind.String
            || property.GetString() is not { } exactValue)
        {
            return false;
        }
        value = exactValue;
        return true;
    }

    private static bool IsSafeObservedModelName(string value)
        => !string.IsNullOrWhiteSpace(value)
           && value.Length <= 256
           && StringComparer.Ordinal.Equals(value, value.Trim())
           && value.All(static character =>
               character is >= 'a' and <= 'z'
                   or >= 'A' and <= 'Z'
                   or >= '0' and <= '9'
                   or '.' or '_' or '-' or '/' or ':');

    private async Task<OperationResult<string>> ReadChatResponseAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode
            || !HasJsonContentType(response.Content.Headers.ContentType))
        {
            return OperationResult<string>.Fail(
                "The local teacher returned an invalid response.",
                ResponseInvalidErrorCode);
        }

        var body = await ReadBoundedResponseAsync(
            response.Content,
            ResponseTooLargeErrorCode,
            ResponseInvalidErrorCode,
            cancellationToken).ConfigureAwait(false);
        if (!body.IsSuccess || body.Value is null)
            return body.MapError<string>();
        return ParseResponse(body.Value, _configuration.ModelId);
    }

    private static HttpRequestMessage CreateChatRequest(byte[] requestBody)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, ChatEndpoint)
        {
            Content = new ByteArrayContent(requestBody)
        };
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json")
        {
            CharSet = "utf-8"
        };
        return request;
    }

    private static OperationResult<bool> ValidatePrompt(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return OperationResult<bool>.Fail(
                "A non-empty local teacher prompt is required.",
                PromptInvalidErrorCode);
        }

        try
        {
            if (prompt.Any(char.IsControl))
            {
                return OperationResult<bool>.Fail(
                    "The local teacher prompt contains disallowed text.",
                    PromptInvalidErrorCode);
            }
            if (StrictUtf8.GetByteCount(prompt) > MaximumPromptUtf8Bytes)
            {
                return OperationResult<bool>.Fail(
                    "The local teacher request exceeds the configured limit.",
                    RequestTooLargeErrorCode);
            }
        }
        catch (EncoderFallbackException)
        {
            return OperationResult<bool>.Fail(
                "The local teacher prompt is not valid UTF-8 text.",
                PromptInvalidErrorCode);
        }
        return OperationResult<bool>.Ok(true);
    }

    private static TrainingTeacherProvenance CreateProvenance(
        ContinuousLearningTp1DpoConfiguration configuration,
        TeacherInferencePurpose purpose)
        => new(
            TrainingTeacherSourceKind.ExternalTeacherOutput,
            OpenWeightTeacherFamily.Qwen,
            TrainingTeacherAccessMode.LocalHostedOpenWeight,
            TrainingTeacherHostingProvider.Ollama,
            configuration.ModelId,
            configuration.ExpectedOllamaManifestDigest,
            TrainingTeacherModelLicense.Apache20,
            ProviderTermsChecked: false,
            ProviderTermsAllowTraining: false,
            ClosedProprietaryModel: false,
            TeacherOutputUsedForTraining:
                purpose == TeacherInferencePurpose.TrainingAuthoring,
            RequestId: Convert.ToHexString(
                RandomNumberGenerator.GetBytes(16)).ToLowerInvariant(),
            InvocationCount: 1)
        {
            ModelArtifactDigestKind =
                LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind,
            ModelArtifactDigest = configuration.ExpectedOllamaModelLayerDigest
        };

    // This immutable receipt contains the exact unsent body; it grants no permit, inference or optimizer authority.
    internal sealed class PreparedAuthoringRequest // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
    { // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
        private readonly byte[] body; // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
        private readonly int[] ids; // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
        internal string Prompt { get; } // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
        internal string Rendered { get; } // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
        internal string TokenizerOrigin { get; } // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
        internal string SourceOrigin { get; } // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
        internal string BindingSha256 { get; } // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
        internal string BodySha256 { get; } // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
        internal string ResourceFingerprint { get; } // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
        internal int InputTokens => ids.Length; // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
        private readonly LocalQwenOllamaInferenceProfileDescriptor profile; // Freeze the selected preparation descriptor beside the counted body.
        internal int OutputReserve => profile.NumPredict!.Value; // CPU preparation uses the same finite reserve as real inference.
        internal TrainingTeacherInferenceProfileEvidence Profile => profile.Evidence; // Legacy preparation retains its old separate token-bound evidence.
        internal byte[] CopyBody() => body.ToArray(); // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
        internal int[] CopyTokenIds() => ids.ToArray(); // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
        private PreparedAuthoringRequest(byte[] body, string prompt, string rendered, int[] ids, string sourceOrigin, string tokenizerOrigin, // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
            string resourceFingerprint, string selectedContentSha, ContinuousLearningTp1DpoConfiguration configuration) // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
        { // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
            this.body = body.ToArray(); this.ids = ids.ToArray(); Prompt = prompt; Rendered = rendered; // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
            profile = configuration.PreparedAuthoringDescriptor; // Snapshot the exact process-owned profile before computing the binding.
            SourceOrigin = sourceOrigin; TokenizerOrigin = tokenizerOrigin; ResourceFingerprint = resourceFingerprint; // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
            BodySha256 = Convert.ToHexString(SHA256.HashData(this.body)).ToLowerInvariant(); // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
            BindingSha256 = ConfiguredQwenVocabularyLease.Binding("configured-qwen-prepared-authoring-v1", sourceOrigin, tokenizerOrigin, // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
                selectedContentSha, Profile.ProfileId, Profile.ConfigurationSha256, configuration.ModelId, // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
                configuration.ExpectedOllamaManifestDigest, configuration.ExpectedOllamaModelLayerDigest, resourceFingerprint, // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
                ConfiguredQwenVocabularyLease.RendererRevision, ConfiguredQwenVocabularyLease.RendererSha, // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
                profile.BindCpuAuthoringParameters ? "single-user-json-cpu-bounded-profile-v1" : "single-user-json-think-false-truncate-false-shift-false-8192-2048-seed0-temp0", // Preserve the old preimage while the CPU fields are independently bound by Profile.ConfigurationSha256.
                prompt, rendered, BodySha256, ids.Length.ToString(System.Globalization.CultureInfo.InvariantCulture), // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
                ConfiguredQwenVocabularyLease.TokenIdsSha(this.ids)); // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
        } // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
        internal bool Matches(string selectedContentSha, ContinuousLearningTp1DpoConfiguration configuration) // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
            => BindingSha256 == new PreparedAuthoringRequest(body, Prompt, Rendered, ids, SourceOrigin, TokenizerOrigin, // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
                ResourceFingerprint, selectedContentSha, configuration).BindingSha256; // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
        internal static OperationResult<PreparedAuthoringRequest> Prepare(string prompt, string selectedContentSha, string sourceOrigin, // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
            ContinuousLearningTp1DpoConfiguration configuration, ConfiguredQwenVocabularyLease? native, // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
            Func<string, CancellationToken, OperationResult<int[]>>? syntheticForTesting, CancellationToken token) // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
        { // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
            token.ThrowIfCancellationRequested(); // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
            if ((native is null) == (syntheticForTesting is null)) return Refuse(ConfigurationInvalidErrorCode); // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
            var valid = ValidatePrompt(prompt); // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
            if (!valid.IsSuccess) return valid.MapError<PreparedAuthoringRequest>(); // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
            // Canonical JSON starts and ends in braces, so the pinned Go TrimSpace branch cannot remove any context bytes.
            if (prompt.Length < 2 || prompt[0] != '{' || prompt[^1] != '}') return Refuse(PromptInvalidErrorCode); // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
            using var parsed = JsonDocument.Parse(prompt, new JsonDocumentOptions { MaxDepth = 64 }); // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
            if (parsed.RootElement.ValueKind != JsonValueKind.Object) return Refuse(PromptInvalidErrorCode); // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
            var profile = configuration.PreparedAuthoringDescriptor; // Count with the same CPU profile as the real request, retaining legacy preparation behavior by default.
            var encoded = BuildRequestBody(configuration.ModelId, prompt, profile, tokenBoundShape: true); // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
            if (!encoded.IsSuccess || encoded.Value is null) return encoded.MapError<PreparedAuthoringRequest>(); // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
            var rendered = RenderRestrictedSingleUser(prompt); // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
            int[] ids; string resource; string origin; // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
            if (native is not null) // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
            { // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
                var counted = native.Count(rendered, configuration, token); // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
                if (!counted.IsSuccess || counted.Value is null) return counted.MapError<PreparedAuthoringRequest>(); // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
                ids = counted.Value.CopyIds(); resource = counted.Value.ResourceFingerprint; origin = "NativeConfigured"; // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
            } // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
            else // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
            { // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
                var counted = syntheticForTesting!(rendered, token); // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
                if (!counted.IsSuccess || counted.Value is null) return counted.MapError<PreparedAuthoringRequest>(); // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
                ids = counted.Value.ToArray(); resource = "SyntheticTestOnly"; origin = "SyntheticTest"; // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
            } // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
            token.ThrowIfCancellationRequested(); // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
            if (ids.Length == 0 || ids.Any(id => id < 0 || id >= 248320)) return Refuse(ConfiguredQwenVocabularyLease.Unsupported); // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
            if ((long)ids.Length + profile.NumPredict!.Value > profile.NumContext) return Refuse(ConfiguredQwenVocabularyLease.BudgetExceeded); // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
            if (native is not null) // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
            { // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
                var final = native.ValidateCurrent(token); // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
                if (!final.IsSuccess) return final.MapError<PreparedAuthoringRequest>(); // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
            } // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
            token.ThrowIfCancellationRequested(); // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
            return OperationResult<PreparedAuthoringRequest>.Ok(new(encoded.Value, prompt, rendered, ids, sourceOrigin, origin, resource, selectedContentSha, configuration)); // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
        } // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
        private static OperationResult<PreparedAuthoringRequest> Refuse(string code) // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
            => OperationResult<PreparedAuthoringRequest>.Fail("The complete unsent authoring request was not token-admitted.", code); // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
    } // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
    internal static string RenderRestrictedSingleUser(string canonicalPrompt) // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.
        => "<|im_start|>user\n" + canonicalPrompt + "<|im_end|>\n<|im_start|>assistant\n<think>\n\n</think>\n\n"; // Bind the entire unsent body, finite profile and exact tokenizer origin before effects.


    private static OperationResult<byte[]> BuildRequestBody(
        string modelId,
        string prompt,
        LocalQwenOllamaInferenceProfileDescriptor inferenceProfile, bool tokenBoundShape = false) // Legacy callers retain their identical body; finite preparation disables server truncation and shifting.
    {
        try
        {
            using var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(
                       stream,
                       new JsonWriterOptions
                       {
                           Indented = false,
                           MaxDepth = MaximumJsonDepth,
                           SkipValidation = false
                       }))
            {
                writer.WriteStartObject();
                writer.WriteString("model", modelId);
                writer.WriteBoolean("stream", false);
                if (tokenBoundShape || inferenceProfile.BindCpuAuthoringParameters) { writer.WriteBoolean("truncate", false); writer.WriteBoolean("shift", false); } // CPU effects preserve the exact no-truncation/no-shift prepared body.
                writer.WriteStartArray("messages");
                writer.WriteStartObject();
                writer.WriteString("role", "user");
                writer.WriteString("content", prompt);
                writer.WriteEndObject();
                writer.WriteEndArray();
                if (inferenceProfile.RequestFormatMode ==
                    LocalQwenOllamaRequestFormatMode.ExplicitIdentity)
                {
                    writer.WriteString(
                        "format",
                        inferenceProfile.FormatIdentity);
                }
                if (inferenceProfile.Think.HasValue)
                    writer.WriteBoolean("think", inferenceProfile.Think.Value);
                writer.WriteStartObject("options");
                writer.WriteNumber("num_ctx", inferenceProfile.NumContext);
                writer.WriteNumber("temperature", inferenceProfile.Temperature);
                writer.WriteNumber("seed", inferenceProfile.Seed);
                if (inferenceProfile.BindCpuAuthoringParameters) // Serialize only the independently hashed canonical CPU controls.
                { // No arbitrary caller runtime options enter the body.
                    writer.WriteNumber("num_thread", 2); // Match the measured A0 two-thread CPU configuration.
                    writer.WriteNumber("num_batch", 16); // Keep the measured small batch budget.
                    writer.WriteNumber("num_gpu", 0); // Explicitly select CPU without mutating ambient Ollama settings.
                } // Legacy and frozen evaluation options remain byte-identical.
                if (inferenceProfile.NumPredict.HasValue)
                {
                    writer.WriteNumber(
                        "num_predict",
                        inferenceProfile.NumPredict.Value);
                }
                writer.WriteEndObject();
                if (inferenceProfile.BindCpuAuthoringParameters) writer.WriteNumber("keep_alive", 0); // Release the CPU runner after completion exactly as measured in A0.
                else writer.WriteString("keep_alive", "5m"); // Preserve every legacy/evaluation serialized byte.
                writer.WriteEndObject();
            }

            if (stream.Length > MaximumRequestBodyBytes)
            {
                return OperationResult<byte[]>.Fail(
                    "The local teacher request exceeds the configured limit.",
                    RequestTooLargeErrorCode);
            }
            return OperationResult<byte[]>.Ok(stream.ToArray());
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<byte[]>.Fail(
                "The local teacher request could not be encoded.",
                PromptInvalidErrorCode);
        }
    }

    private static OperationResult<byte[]> BuildModelShowRequestBody(string modelId)
    {
        try
        {
            using var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(
                       stream,
                       new JsonWriterOptions
                       {
                           Indented = false,
                           MaxDepth = MaximumJsonDepth,
                           SkipValidation = false
                       }))
            {
                writer.WriteStartObject();
                writer.WriteString("model", modelId);
                writer.WriteEndObject();
            }
            return stream.Length <= MaximumRequestBodyBytes
                ? OperationResult<byte[]>.Ok(stream.ToArray())
                : OperationResult<byte[]>.Fail(
                    "The local teacher identity request exceeds the configured limit.",
                    RequestTooLargeErrorCode);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<byte[]>.Fail(
                "The local teacher identity request could not be encoded.",
                ConfigurationInvalidErrorCode);
        }
    }

    private static bool HasJsonContentType(MediaTypeHeaderValue? contentType)
        => contentType is not null
           && StringComparer.OrdinalIgnoreCase.Equals(
               contentType.MediaType,
               "application/json")
           && (string.IsNullOrEmpty(contentType.CharSet)
               || StringComparer.OrdinalIgnoreCase.Equals(
                   contentType.CharSet.Trim('"'),
                   "utf-8"));

    private static async Task<OperationResult<byte[]>> ReadBoundedResponseAsync(
        HttpContent content,
        string tooLargeErrorCode,
        string invalidErrorCode,
        CancellationToken cancellationToken)
    {
        var declaredLength = content.Headers.ContentLength;
        if (declaredLength is < 0 or > MaximumResponseBodyBytes)
        {
            return OperationResult<byte[]>.Fail(
                "The local teacher response exceeded a bounded limit.",
                tooLargeErrorCode);
        }

        await using var responseStream = await content
            .ReadAsStreamAsync(cancellationToken)
            .ConfigureAwait(false);
        using var buffer = new MemoryStream(
            declaredLength is >= 0
                ? checked((int)declaredLength.Value)
                : Math.Min(MaximumResponseBodyBytes, 16_384));
        var chunk = new byte[8_192];
        var total = 0;
        while (true)
        {
            var read = await responseStream
                .ReadAsync(chunk.AsMemory(), cancellationToken)
                .ConfigureAwait(false);
            if (read == 0)
                break;
            total = checked(total + read);
            if (total > MaximumResponseBodyBytes)
            {
                return OperationResult<byte[]>.Fail(
                    "The local teacher response exceeded a bounded limit.",
                    tooLargeErrorCode);
            }
            buffer.Write(chunk, 0, read);
        }

        if (declaredLength.HasValue && total != declaredLength.Value)
        {
            return OperationResult<byte[]>.Fail(
                "The local teacher returned an invalid response.",
                invalidErrorCode);
        }
        return OperationResult<byte[]>.Ok(buffer.ToArray());
    }

    private static OperationResult<string> ParseResponse(
        byte[] responseBody,
        string expectedModelId)
    {
        try
        {
            var json = StrictUtf8.GetString(responseBody);
            using var document = JsonDocument.Parse(
                json,
                new JsonDocumentOptions
                {
                    AllowTrailingCommas = false,
                    CommentHandling = JsonCommentHandling.Disallow,
                    MaxDepth = MaximumJsonDepth
                });
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object
                || !TryGetSingleProperty(root, "model", out var model)
                || model.ValueKind != JsonValueKind.String
                || !StringComparer.Ordinal.Equals(model.GetString(), expectedModelId)
                || !TryGetSingleProperty(root, "done", out var done)
                || done.ValueKind is not JsonValueKind.True
                || !TryGetSingleProperty(root, "message", out var message)
                || message.ValueKind != JsonValueKind.Object
                || !TryGetSingleProperty(message, "role", out var role)
                || role.ValueKind != JsonValueKind.String
                || !StringComparer.Ordinal.Equals(role.GetString(), "assistant")
                || !TryGetSingleProperty(message, "content", out var content)
                || content.ValueKind != JsonValueKind.String
                || string.IsNullOrWhiteSpace(content.GetString()))
            {
                return OperationResult<string>.Fail(
                    "The local teacher returned an invalid response.",
                    ResponseInvalidErrorCode);
            }
            return OperationResult<string>.Ok(content.GetString()!);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<string>.Fail(
                "The local teacher returned an invalid response.",
                ResponseInvalidErrorCode);
        }
    }

    private static bool TryGetSingleProperty(
        JsonElement element,
        string propertyName,
        out JsonElement value)
    {
        value = default;
        var found = false;
        foreach (var property in element.EnumerateObject())
        {
            if (!StringComparer.Ordinal.Equals(property.Name, propertyName))
                continue;
            if (found)
                return false;
            value = property.Value;
            found = true;
        }
        return found;
    }

    private static OperationResult<bool> IdentityFailure(string errorCode)
        => OperationResult<bool>.Fail(
            "The local teacher model identity could not be verified.",
            errorCode);

    private static OperationResult<T> PermitFailure<T>(string? errorCode)
        => Failure<T>(
            IsSafeErrorCode(errorCode) ? errorCode! : CallPermitRejectedErrorCode,
            "The local teacher call permit was rejected.");

    private static bool IsSafeErrorCode(string? errorCode)
        => !string.IsNullOrWhiteSpace(errorCode)
           && errorCode.Length <= 128
           && errorCode.All(static character =>
               character is >= 'A' and <= 'Z'
                   or >= '0' and <= '9'
                   or '_');

    private static OperationResult<LocalQwenOllamaTeacherInferenceClient>
        ConfigurationFailure()
        => OperationResult<LocalQwenOllamaTeacherInferenceClient>.Fail(
            "The local teacher client configuration is invalid.",
            ConfigurationInvalidErrorCode);

    private static OperationResult<T> ConfigurationFailure<T>()
        => OperationResult<T>.Fail(
            "The local teacher client configuration is invalid.",
            ConfigurationInvalidErrorCode);

    private static OperationResult<T> Failure<T>(
        string code,
        string message)
        => OperationResult<T>.Fail(message, code);

    private enum TeacherInferencePurpose
    {
        TrainingAuthoring,
        EvaluationOnly
    }

    private sealed record LocalTeacherInferenceResult(
        string Output,
        TrainingTeacherProvenance Provenance,
        TrainingTeacherInferenceProfileEvidence InferenceProfile);
}
