// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using System.Buffers.Binary;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure;

/// <summary>
/// Request-scoped authority to issue at most one externally observable teacher
/// call. Implementations must make <see cref="TryIssue"/> atomic.
/// </summary>
public interface ILegalTeacherCallPermit
{
    bool CallIssued { get; }

    OperationResult<int> TryIssue();
}

/// <summary>
/// Request-scoped authority to issue at most one training-teacher call.
/// Implementations must make <see cref="TryIssue"/> atomic.
/// </summary>
public interface ITrainingTeacherCallPermit
{
    bool CallIssued { get; }

    OperationResult<int> TryIssue();
}

/// <summary>
/// Generates one server-owned legal training-teacher output for a prompt.
/// Implementations own any transport resources and must release them on dispose.
/// </summary>
public interface ITrainingTeacherInferenceClient : IDisposable
{
    TrainingTeacherInferenceProfileEvidence? AuthoringProfile => null; // Legacy adapters remain source-compatible; CPU consumers require an exact bound passport before issuing a call.
    Task<OperationResult<TrainingTeacherInferenceResult>> InferAsync(
        string prompt,
        ITrainingTeacherCallPermit callPermit,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Generates one server-owned local teacher output for evaluation only.
/// </summary>
public interface ILegalTeacherEvaluationInferenceClient : IDisposable
{
    TrainingTeacherInferenceProfileEvidence? EvaluationProfile => null;

    Task<OperationResult<TeacherEvaluationInferenceResult>> EvaluateAsync(
        string prompt,
        ILegalTeacherCallPermit callPermit,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Immutable teacher output paired with the exact provenance admitted for it.
/// </summary>
public sealed record TrainingTeacherInferenceProfileEvidence(
    string ProfileId,
    string ConfigurationSha256);

internal enum LocalQwenOllamaRequestFormatMode
{
    Omit,
    ExplicitIdentity
}

internal sealed class LocalQwenOllamaInferenceProfileDescriptor
{
    internal LocalQwenOllamaInferenceProfileDescriptor(
        string profileId,
        string purpose,
        string formatIdentity,
        LocalQwenOllamaRequestFormatMode requestFormatMode,
        int numContext,
        double temperature,
        int seed,
        bool? think,
        int? numPredict,
        TimeSpan requestTimeout,
        bool bindExtendedEvaluationParameters, // Preserve the existing evidence framing for every legacy descriptor.
        bool bindCpuAuthoringParameters = false,
        bool callerCancellationOnly = false)
    {
        ProfileId = profileId;
        Purpose = purpose;
        FormatIdentity = formatIdentity;
        RequestFormatMode = requestFormatMode;
        NumContext = numContext;
        Temperature = temperature;
        Seed = seed;
        Think = think;
        NumPredict = numPredict;
        RequestTimeout = requestTimeout;
        CallerCancellationOnly = callerCancellationOnly;
        BindCpuAuthoringParameters = bindCpuAuthoringParameters; // Keep the immutable CPU request shape with its descriptor.
        Evidence = LocalQwenOllamaInferenceProfileAuthority.CreateEvidence(
            profileId,
            purpose,
            formatIdentity,
            numContext.ToString(CultureInfo.InvariantCulture),
            temperature.ToString("R", CultureInfo.InvariantCulture),
            seed.ToString(CultureInfo.InvariantCulture),
            bindExtendedEvaluationParameters ? think : null,
            bindExtendedEvaluationParameters ? numPredict : null,
            bindExtendedEvaluationParameters ? requestTimeout : null, // Retain the legacy hash inputs byte for byte.
            bindCpuAuthoringParameters,
            callerCancellationOnly);
    }

    internal string ProfileId { get; }
    internal string Purpose { get; }
    internal string FormatIdentity { get; }
    internal LocalQwenOllamaRequestFormatMode RequestFormatMode { get; }
    internal int NumContext { get; }
    internal double Temperature { get; }
    internal int Seed { get; }
    internal bool? Think { get; }
    internal int? NumPredict { get; }
    internal TimeSpan RequestTimeout { get; }
    internal bool CallerCancellationOnly { get; }
    internal bool BindCpuAuthoringParameters { get; } // One canonical switch supplies the fixed CPU controls to hashing and serialization.
    internal TrainingTeacherInferenceProfileEvidence Evidence { get; }
}

/// <summary>
/// Single byte-exact authority for the legacy local Qwen/Ollama profiles and the separate finite preparation profile.
/// profiles. Every component, including the NUL-terminated domain, is encoded
/// independently as UTF-8 after a signed Int64 big-endian byte length.
/// </summary>
public static class LocalQwenOllamaInferenceProfileAuthority
{
    public const string AuthoringProfileId = "qwen35-local-authoring-v1";
    public const string TokenBoundAuthoringProfileId = "qwen35-local-authoring-token-bound-v1"; // Separate finite preparation profile; legacy authoring and frozen evaluation retain their exact evidence.
    public const string EvaluationProfileId = "qwen35-local-evaluation-v1";
    public const string CpuAuthoringProfileId = "qwen35-local-authoring-cpu-bounded-v1"; // Explicit opt-in; this identity never aliases legacy authoring or evaluation.
    public const string ManualCpuAuthoringProfileId = "qwen35-local-authoring-cpu-caller-cancel-v1";
    public const string ManualEvaluationProfileId = "qwen35-local-evaluation-caller-cancel-v1";
    private const string Domain =
        "xiigen:local-qwen-ollama-inference-profile:v1\0";
    internal static LocalQwenOllamaInferenceProfileDescriptor
        AuthoringDescriptor { get; } =
        new(
            AuthoringProfileId,
            "training_authoring",
            "json",
            LocalQwenOllamaRequestFormatMode.ExplicitIdentity,
            8192,
            0,
            0,
            think: null,
            numPredict: null,
            TimeSpan.FromSeconds(30),
            bindExtendedEvaluationParameters: false);

    internal static LocalQwenOllamaInferenceProfileDescriptor
        EvaluationDescriptor { get; } =
        new(
            EvaluationProfileId,
            "evaluation_only",
            "json",
            LocalQwenOllamaRequestFormatMode.ExplicitIdentity,
            8192,
            0,
            0,
            think: false,
            numPredict: 256,
            TimeSpan.FromSeconds(120),
            bindExtendedEvaluationParameters: true);

    public static TrainingTeacherInferenceProfileEvidence Authoring
        => AuthoringDescriptor.Evidence;

    public static TrainingTeacherInferenceProfileEvidence Evaluation
        => EvaluationDescriptor.Evidence;

    internal static LocalQwenOllamaInferenceProfileDescriptor TokenBoundAuthoringDescriptor { get; } = // Bind the output reserve to the same body counted by the prepared consumer.
        new(TokenBoundAuthoringProfileId, "training_authoring", "json", LocalQwenOllamaRequestFormatMode.ExplicitIdentity, // Keep one JSON authoring purpose without promoting a prepared receipt to call authority.
            8192, 0, 0, false, 2048, TimeSpan.FromSeconds(30), bindExtendedEvaluationParameters: true); // Finite reserve, disabled thinking and timeout participate in the existing length-prefixed evidence hash.

    internal static TrainingTeacherInferenceProfileEvidence TokenBoundAuthoring => TokenBoundAuthoringDescriptor.Evidence; // Expose immutable evidence only inside the existing preparation owners.

    internal static LocalQwenOllamaInferenceProfileDescriptor CpuAuthoringDescriptor { get; } = // Reuse the measured A0 context/CPU settings with a finite two-sided JSON protocol budget.
        new(CpuAuthoringProfileId, "training_authoring", "json", LocalQwenOllamaRequestFormatMode.ExplicitIdentity, // The role and JSON format remain authoring, never an evaluation licence.
            2048, 0, 0, false, 512, TimeSpan.FromSeconds(600), true, bindCpuAuthoringParameters: true); // A0 cold load was 39.69s; 512 output tokens/600s are bounded choices, not measured lesson feasibility or model minima.

    public static TrainingTeacherInferenceProfileEvidence CpuAuthoring => CpuAuthoringDescriptor.Evidence; // Expose the exact allowlisted opt-in passport for startup selection.

    internal static LocalQwenOllamaInferenceProfileDescriptor ManualCpuAuthoringDescriptor { get; } =
        new(ManualCpuAuthoringProfileId, "training_authoring", "json", LocalQwenOllamaRequestFormatMode.ExplicitIdentity,
            2048, 0, 0, false, 512, Timeout.InfiniteTimeSpan, true,
            bindCpuAuthoringParameters: true, callerCancellationOnly: true);

    internal static LocalQwenOllamaInferenceProfileDescriptor ManualEvaluationDescriptor { get; } =
        new(ManualEvaluationProfileId, "evaluation_only", "json", LocalQwenOllamaRequestFormatMode.ExplicitIdentity,
            8192, 0, 0, false, 256, Timeout.InfiniteTimeSpan, true, callerCancellationOnly: true);

    public static TrainingTeacherInferenceProfileEvidence ManualCpuAuthoring => ManualCpuAuthoringDescriptor.Evidence;
    public static TrainingTeacherInferenceProfileEvidence ManualEvaluation => ManualEvaluationDescriptor.Evidence;

    public static bool IsCanonicalCpuAuthoringProfile(TrainingTeacherInferenceProfileEvidence? evidence)
        => IsCanonicalCpuAuthoring(evidence) || evidence == ManualCpuAuthoring;

    public static bool IsCanonicalCpuAuthoring(TrainingTeacherInferenceProfileEvidence? evidence) // Keep the legacy predicate below unchanged.
        => evidence is not null && evidence == CpuAuthoring; // Both ID and independently hashed configuration must match.

    internal static OperationResult<LocalQwenOllamaInferenceProfileDescriptor> ResolveAuthoringDescriptor(TrainingTeacherInferenceProfileEvidence? evidence) // Freeze only exact known authoring profiles in process-owned configuration.
        => evidence is null || IsCanonicalAuthoring(evidence) // Absent selection preserves the previous authoring default.
            ? OperationResult<LocalQwenOllamaInferenceProfileDescriptor>.Ok(AuthoringDescriptor) // Return the same legacy descriptor without rewriting its evidence.
            : IsCanonicalCpuAuthoring(evidence) // Reject forged, evaluation and preparation-only passports before transport exists.
                ? OperationResult<LocalQwenOllamaInferenceProfileDescriptor>.Ok(CpuAuthoringDescriptor) // Use one owner for evidence, body, deadline and consumer identity.
                : evidence == ManualCpuAuthoring
                    ? OperationResult<LocalQwenOllamaInferenceProfileDescriptor>.Ok(ManualCpuAuthoringDescriptor)
                    : OperationResult<LocalQwenOllamaInferenceProfileDescriptor>.Fail("The selected authoring profile is not canonical.", LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode); // An unknown selector never falls back silently.

    internal static OperationResult<LocalQwenOllamaInferenceProfileDescriptor> ResolveEvaluationDescriptor(TrainingTeacherInferenceProfileEvidence? evidence)
        => evidence is null || evidence == Evaluation
            ? OperationResult<LocalQwenOllamaInferenceProfileDescriptor>.Ok(EvaluationDescriptor)
            : evidence == ManualEvaluation
                ? OperationResult<LocalQwenOllamaInferenceProfileDescriptor>.Ok(ManualEvaluationDescriptor)
                : OperationResult<LocalQwenOllamaInferenceProfileDescriptor>.Fail("The selected evaluation profile is not canonical.", LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode);

    internal static bool IsCanonicalTokenBoundAuthoring(TrainingTeacherInferenceProfileEvidence? evidence) // Do not broaden the legacy IsCanonicalAuthoring predicate.
        => evidence is not null && evidence == TokenBoundAuthoring; // Require the independently identified finite profile.

    public static bool IsCanonicalAuthoring(
        TrainingTeacherInferenceProfileEvidence? evidence)
        => evidence is not null && evidence == Authoring;

    internal static TrainingTeacherInferenceProfileEvidence CreateForTesting(
        string profileId,
        string purpose,
        string format,
        string numContext,
        string temperature,
        string seed)
        => CreateEvidence(
            profileId,
            purpose,
            format,
            numContext,
            temperature,
            seed,
            think: null,
            numPredict: null,
            requestTimeout: null);

    internal static TrainingTeacherInferenceProfileEvidence
        CreateEvaluationForTesting(
            string profileId,
            string purpose,
            string format,
            string numContext,
            string temperature,
            string seed,
            bool think,
            int numPredict,
            TimeSpan requestTimeout)
        => CreateEvidence(
            profileId,
            purpose,
            format,
            numContext,
            temperature,
            seed,
            think,
            numPredict,
            requestTimeout);

    internal static TrainingTeacherInferenceProfileEvidence CreateEvidence(
        string profileId,
        string purpose,
        string format,
        string numContext,
        string temperature,
        string seed,
        bool? think,
        int? numPredict,
        TimeSpan? requestTimeout, // Existing callers retain their exact length-prefixed evidence bytes.
        bool bindCpuAuthoringParameters = false,
        bool callerCancellationOnly = false)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        Append(hash, Domain);
        Append(hash, "profile_id");
        Append(hash, profileId);
        Append(hash, "purpose");
        Append(hash, purpose);
        Append(hash, "format");
        Append(hash, format);
        Append(hash, "num_ctx");
        Append(hash, numContext);
        Append(hash, "temperature");
        Append(hash, temperature);
        Append(hash, "seed");
        Append(hash, seed);
        if (think.HasValue && numPredict.HasValue && requestTimeout.HasValue)
        {
            // Эти поля входят только в паспорт режима оценки: так изменение
            // ответа модели не сможет пройти под прежним отпечатком профиля.
            Append(hash, "think");
            Append(hash, think.Value ? "true" : "false");
            Append(hash, "num_predict");
            Append(hash, numPredict.Value.ToString(CultureInfo.InvariantCulture));
            Append(hash, "request_timeout_seconds");
            Append(
                hash,
                requestTimeout.Value.TotalSeconds.ToString(
                    "R",
                    CultureInfo.InvariantCulture));
        }
        if (bindCpuAuthoringParameters) // Bind the controls actually serialized by the bounded CPU request.
        { // No legacy descriptor enters this additional preimage branch.
            foreach (var component in new[] { "stream", "false", "truncate", "false", "shift", "false", "num_thread", "2", "num_batch", "16", "num_gpu", "0", "keep_alive_seconds", "0" }) // Fix names, values and order without accepting arbitrary caller options.
                Append(hash, component); // Use the same signed Int64 big-endian UTF8 framing as the old profiles.
        } // Context, temperature, seed, thinking, finite reserve and deadline were bound above.
        if (callerCancellationOnly)
        {
            Append(hash, "request_cancellation_mode");
            Append(hash, "caller_or_client_lifetime_only");
        }
        return new TrainingTeacherInferenceProfileEvidence(
            profileId,
            Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant());
    }

    private static void Append(IncrementalHash hash, string component)
    {
        var bytes = Encoding.UTF8.GetBytes(component);
        Span<byte> length = stackalloc byte[sizeof(long)];
        BinaryPrimitives.WriteInt64BigEndian(length, bytes.LongLength);
        hash.AppendData(length);
        hash.AppendData(bytes);
    }
}

public sealed record TrainingTeacherInferenceResult(
    string Output,
    TrainingTeacherProvenance TrainingTeacherProvenance)
{
    public TrainingTeacherInferenceProfileEvidence? InferenceProfile
    {
        get;
        init;
    }
}

/// <summary>
/// Immutable evaluation-only teacher output paired with its nontraining
/// provenance. This result is deliberately distinct from the training result.
/// </summary>
public sealed record TeacherEvaluationInferenceResult(
    string Output,
    TrainingTeacherProvenance TrainingTeacherProvenance)
{
    public TrainingTeacherInferenceProfileEvidence? InferenceProfile
    {
        get;
        init;
    }
}
