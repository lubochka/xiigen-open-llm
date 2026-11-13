// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using AgentInfrastructure;

/// <summary>
/// Composition boundary for local TP1 observations. The returned capability
/// exposes evaluation only and cannot be used as a training-teacher client.
/// </summary>
public static class ContinuousLearningTp1EvaluationComposition
{
    public static OperationResult<ILegalTeacherEvaluationInferenceClient> CreateClient(
        ContinuousLearningTp1DpoConfiguration configuration)
    {
        if (configuration is null)
            return InvalidConfiguration();

        var created = LocalQwenOllamaTeacherInferenceClient.Create(configuration);
        if (!created.IsSuccess || created.Value is null)
            return created.MapError<ILegalTeacherEvaluationInferenceClient>();

        return OperationResult<ILegalTeacherEvaluationInferenceClient>.Ok(
            new EvaluationOnlyOwnedClient(created.Value));
    }

    internal static ILegalTeacherEvaluationInferenceClient OwnForTesting(
        ILegalTeacherEvaluationInferenceClient client)
    {
        ArgumentNullException.ThrowIfNull(client);
        return new EvaluationOnlyOwnedClient(client);
    }

    private static OperationResult<ILegalTeacherEvaluationInferenceClient>
        InvalidConfiguration()
        => OperationResult<ILegalTeacherEvaluationInferenceClient>.Fail(
            "TP1 evaluation configuration is invalid.",
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode);

    private sealed class EvaluationOnlyOwnedClient(
        ILegalTeacherEvaluationInferenceClient inner)
        : ILegalTeacherEvaluationInferenceClient
    {
        private ILegalTeacherEvaluationInferenceClient? _inner = inner;
        public TrainingTeacherInferenceProfileEvidence? EvaluationProfile => Volatile.Read(ref _inner)?.EvaluationProfile;

        public Task<OperationResult<TeacherEvaluationInferenceResult>> EvaluateAsync(
            string prompt,
            ILegalTeacherCallPermit callPermit,
            CancellationToken cancellationToken = default)
        {
            var current = Volatile.Read(ref _inner);
            if (current is null)
            {
                return Task.FromResult(
                    OperationResult<TeacherEvaluationInferenceResult>.Fail(
                        "The TP1 evaluation client is unavailable.",
                        LocalQwenOllamaTeacherInferenceClient.DisposedErrorCode));
            }

            return current.EvaluateAsync(prompt, callPermit, cancellationToken);
        }

        public void Dispose()
            => Interlocked.Exchange(ref _inner, null)?.Dispose();
    }
}

/// <summary>
/// Public composition boundary for the already admitted local Qwen/Ollama
/// authoring lane. Translation can request this client lazily after proving a
/// repository DPO clean miss without exposing the internal transport type.
/// </summary>
public static class ContinuousLearningTp1AuthoringComposition
{
    public static OperationResult<ContinuousLearningTp1DpoConfiguration>
        ResolveConfiguration(Func<string, string?> readEnvironment)
    {
        if (readEnvironment is null)
            return InvalidConfiguration();

        var resolution = ContinuousLearningModelServerConfigurationLoader
            .Resolve(readEnvironment);
        return resolution.State is ContinuousLearningConfigurationState.Enabled &&
               resolution.DeferredPlan is not null
            ? OperationResult<ContinuousLearningTp1DpoConfiguration>.Ok(
                resolution.DeferredPlan.TeacherConfiguration)
            : OperationResult<ContinuousLearningTp1DpoConfiguration>.Fail(
                "TP1 local Qwen authoring configuration is unavailable.",
                string.IsNullOrWhiteSpace(resolution.ErrorCode)
                    ? LegalTrainingTeacherAdmissionAuthority
                        .ConfigurationRequiredErrorCode
                    : resolution.ErrorCode);
    }

    public static OperationResult<ITrainingTeacherInferenceClient> CreateClient(
        ContinuousLearningTp1DpoConfiguration configuration)
    {
        if (configuration is null)
            return InvalidClientConfiguration();

        var created = LocalQwenOllamaTeacherInferenceClient.Create(configuration);
        if (!created.IsSuccess || created.Value is null)
            return created.MapError<ITrainingTeacherInferenceClient>();

        return OperationResult<ITrainingTeacherInferenceClient>.Ok(
            new AuthoringOnlyOwnedClient(created.Value));
    }

    internal static ITrainingTeacherInferenceClient OwnForTesting(
        ITrainingTeacherInferenceClient client)
    {
        ArgumentNullException.ThrowIfNull(client);
        return new AuthoringOnlyOwnedClient(client);
    }

    private static OperationResult<ContinuousLearningTp1DpoConfiguration>
        InvalidConfiguration()
        => OperationResult<ContinuousLearningTp1DpoConfiguration>.Fail(
            "TP1 local Qwen authoring configuration is invalid.",
            LegalTrainingTeacherAdmissionAuthority
                .ConfigurationRequiredErrorCode);

    private static OperationResult<ITrainingTeacherInferenceClient>
        InvalidClientConfiguration()
        => OperationResult<ITrainingTeacherInferenceClient>.Fail(
            "TP1 local Qwen authoring configuration is invalid.",
            LegalTrainingTeacherAdmissionAuthority
                .ConfigurationRequiredErrorCode);

    private sealed class AuthoringOnlyOwnedClient(
        ITrainingTeacherInferenceClient inner)
        : ITrainingTeacherInferenceClient
    {
        private ITrainingTeacherInferenceClient? _inner = inner;
        public TrainingTeacherInferenceProfileEvidence? AuthoringProfile => Volatile.Read(ref _inner)?.AuthoringProfile; // Retain the process-selected passport through the real authoring wrapper, without exposing evaluation authority.

        public Task<OperationResult<TrainingTeacherInferenceResult>> InferAsync(
            string prompt,
            ITrainingTeacherCallPermit callPermit,
            CancellationToken cancellationToken = default)
        {
            var current = Volatile.Read(ref _inner);
            if (current is null)
            {
                return Task.FromResult(
                    OperationResult<TrainingTeacherInferenceResult>.Fail(
                        "The TP1 authoring client is unavailable.",
                        LocalQwenOllamaTeacherInferenceClient
                            .DisposedErrorCode));
            }

            return current.InferAsync(prompt, callPermit, cancellationToken);
        }

        public void Dispose()
            => Interlocked.Exchange(ref _inner, null)?.Dispose();
    }
}
