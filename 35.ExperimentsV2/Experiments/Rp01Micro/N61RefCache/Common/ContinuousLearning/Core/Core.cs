// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using DPO.Models;

// ─── Training example ─────────────────────────────────────────────────────────
// One labelled sample stored in the domain dataset cache.
// weight: dynamic — computed by FeedbackRouter.ComputeMaxWeight (D-09, never hardcoded ×10).
public sealed record TrainingExample(
    string         Prompt,       // original user prompt or symbol identifier
    string         Response,     // model's response at capture time
    string         Domain,       // canonical D-04: CODE_Q / BUG / PLAN / ARCH / REFACTOR
    double         Weight,       // importance weight — 1.0 = standard, higher = rare domain
    DateTimeOffset CapturedAt,
    string?        SourceId = null); // canonical per-example source provenance; required for promotion

// ─── DPO pair side authorship ─────────────────────────────────────────────────
// Авторство стороны хранится отдельно от текста пары: будущая дверь тренера
// должна видеть источник chosen/rejected без угадывания по позиции строки.
public sealed record DpoPairSideAuthorship
{
    public DpoPairSideAuthorship(
        DpoPairSideAuthorKind authorKind,
        TrainingTeacherProvenance? openTeacherPassport = null)
    {
        if (!DpoPairSideAuthorshipPolicy.IsSupportedAuthorKind(authorKind))
            throw new ArgumentOutOfRangeException(nameof(authorKind));

        if (authorKind is DpoPairSideAuthorKind.OpenTeacher)
        {
            if (!DpoPairSideAuthorshipPolicy
                    .IsPermittedOpenTeacherPassportReference(openTeacherPassport))
            {
                throw new ArgumentException(
                    "OpenTeacher authorship requires a valid teacher passport.",
                    nameof(openTeacherPassport));
            }
        }
        else if (openTeacherPassport is not null)
        {
            throw new ArgumentException(
                "Only OpenTeacher authorship may carry a teacher passport.",
                nameof(openTeacherPassport));
        }

        AuthorKind = authorKind;
        OpenTeacherPassport = openTeacherPassport;
    }

    public DpoPairSideAuthorKind AuthorKind { get; }

    // Паспорт обязателен только для OpenTeacher: так v5 не превращает строковый
    // ярлык "teacher" в доверенный источник без существующей provenance-записи.
    public TrainingTeacherProvenance? OpenTeacherPassport { get; }
}

internal static class DpoPairSideAuthorshipPolicy
{
    internal const string InvalidErrorCode = "DPO_PAIR_AUTHORSHIP_INVALID";

    internal static bool IsSupportedAuthorKind(DpoPairSideAuthorKind authorKind)
        => authorKind is DpoPairSideAuthorKind.User
            or DpoPairSideAuthorKind.LocalModel
            or DpoPairSideAuthorKind.OpenTeacher
            or DpoPairSideAuthorKind.DeterministicGenerator;

    internal static bool IsPermittedOpenTeacherPassport(
        TrainingTeacherProvenance? passport,
        bool requireTrainingContentSha256)
        => passport is
           {
               SourceKind: TrainingTeacherSourceKind.ExternalTeacherOutput,
               Family: OpenWeightTeacherFamily.Qwen,
               AccessMode: TrainingTeacherAccessMode.LocalHostedOpenWeight,
               HostingProvider: TrainingTeacherHostingProvider.Ollama,
               ModelLicense: TrainingTeacherModelLicense.Apache20,
               ProviderTermsChecked: false,
               ProviderTermsAllowTraining: false,
               ClosedProprietaryModel: false,
               TeacherOutputUsedForTraining: true,
               InvocationCount: > 0,
           }
           && LegalTrainingTeacherAdmissionAuthority
               .IsCanonicalLocalOllamaModelIdForFamily(
                   OpenWeightTeacherFamily.Qwen,
                   passport.ModelId)
           && IsSafePersistedIdentity(passport.RequestId)
           && LegalTrainingTeacherAdmissionAuthority
               .IsCanonicalLocalOllamaArtifactIdentity(
                   passport.ModelVersion,
                   passport.ModelArtifactDigestKind,
                   passport.ModelArtifactDigest)
           && (!requireTrainingContentSha256 ||
               LegalTrainingTeacherAdmissionAuthority
                   .IsCanonicalSha256Digest(passport.TrainingContentSha256));

    internal static bool IsPermittedOpenTeacherPassportReference(
        TrainingTeacherProvenance? passport)
        => IsPermittedOpenTeacherPassport(
               passport,
               requireTrainingContentSha256: false)
           && (string.IsNullOrEmpty(passport!.TrainingContentSha256)
               || LegalTrainingTeacherAdmissionAuthority
                   .IsCanonicalSha256Digest(passport.TrainingContentSha256));

    internal static bool SamePassportIdentity(
        TrainingTeacherProvenance? left,
        TrainingTeacherProvenance? right,
        bool includeTrainingContentSha256)
        => left is not null
           && right is not null
           && left.SourceKind == right.SourceKind
           && left.Family == right.Family
           && left.AccessMode == right.AccessMode
           && left.HostingProvider == right.HostingProvider
           && StringComparer.Ordinal.Equals(left.ModelId, right.ModelId)
           && StringComparer.Ordinal.Equals(left.ModelVersion, right.ModelVersion)
           && left.ModelLicense == right.ModelLicense
           && left.ProviderTermsChecked == right.ProviderTermsChecked
           && left.ProviderTermsAllowTraining == right.ProviderTermsAllowTraining
           && left.ClosedProprietaryModel == right.ClosedProprietaryModel
           && left.TeacherOutputUsedForTraining == right.TeacherOutputUsedForTraining
           && StringComparer.Ordinal.Equals(left.RequestId, right.RequestId)
           && left.InvocationCount == right.InvocationCount
           && StringComparer.Ordinal.Equals(
               left.ModelArtifactDigestKind,
               right.ModelArtifactDigestKind)
           && StringComparer.Ordinal.Equals(
               left.ModelArtifactDigest,
               right.ModelArtifactDigest)
           && (!includeTrainingContentSha256 ||
               StringComparer.Ordinal.Equals(
                   left.TrainingContentSha256,
                   right.TrainingContentSha256));

    private static bool IsSafePersistedIdentity(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && value.Length <= 256
           && StringComparer.Ordinal.Equals(value, value.Trim())
           && !value.Any(char.IsControl);
}

// ─── DPO pair ─────────────────────────────────────────────────────────────────
// Chosen/rejected pair produced by FeedbackRouter for DPO fine-tuning.
// Created when user edits (Chosen=correction) or rejects (Chosen="[no preferred response]").
public sealed record DpoPair(
    string         Prompt,
    string         Chosen,       // preferred response — user's edit or placeholder
    string         Rejected,     // the model response the user did not accept
    string         Domain,
    DateTimeOffset CapturedAt)
{
    public TrainingTeacherProvenance? TeacherProvenance { get; init; }
    public TrainingDataGovernanceDecision? GovernanceDecision { get; init; }
    public DpoPairSideAuthorship? ChosenAuthorship { get; init; }
    public DpoPairSideAuthorship? RejectedAuthorship { get; init; }
}

// ─── Feedback signal ──────────────────────────────────────────────────────────
// Three signals a user can send about a model response.
public enum FeedbackSignal { Accepted, Edited, Rejected }

// ─── Feedback event ───────────────────────────────────────────────────────────
// Context needed by FeedbackRouter to convert one signal into a training record.
public sealed record FeedbackEvent(
    string         Prompt,
    string         ModelResponse,    // what the model produced
    string?        UserCorrection,   // non-null when Signal == Edited
    FeedbackSignal Signal,
    string         Domain)
{
    public TrainingTeacherProvenance? TeacherProvenance { get; init; }
    public TrainingDataGovernanceDecision? GovernanceDecision { get; init; }
}

// ─── Model adapter ────────────────────────────────────────────────────────────
// Represents one LoRA adapter file saved by IncrementalTrainer (~2 MB per adapter).
public sealed record ModelAdapter(
    string         Domain,            // which domain this adapter specialises for
    string         AdapterPath,       // fully-qualified absolute path to the .lora artifact
    int            TrainingExamples,  // how many examples produced this adapter
    DateTimeOffset TrainedAt,
    ModelAdapterProvenance? Provenance = null);
