// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

namespace DesignConversionDomain;

using AgentInfrastructure;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

/// <summary>
/// Rejection-only classifier for review-only teacher-authored records that
/// cannot use an ordinary training route. It observes record metadata without
/// minting, consuming, or modifying corpus-admission authority.
/// </summary>
internal static class DesignConversionDpoCorpusAdmissionRequirementPolicy
{
    internal static bool RequiresAdmission(
        IReadOnlyList<DesignConversionDpoTrainingRecord> records)
        => records is not null && records.Any(static record =>
        {
            var metadata = record?.Provenance?.Metadata;
            return IsTrue(Read(metadata, "runtime_teacher_authored_candidate")) &&
                   IsTrue(Read(metadata, "independent_preference_proven")) &&
                   IsTrue(Read(metadata, "runtime_rejected_independently_evaluated"));
        });

    private static string Read(
        IReadOnlyDictionary<string, string>? metadata,
        string key)
        => metadata is not null && metadata.TryGetValue(key, out var value)
            ? value ?? string.Empty
            : string.Empty;

    private static bool IsTrue(string value)
        => bool.TryParse(value, out var parsed) && parsed;
}

/// <summary>
/// Server-owned mint for the narrow teacher-authored DPO corpus boundary.
/// The local artifact store owns this object; neither record metadata nor an
/// evaluator result can reconstruct it.
/// </summary>
internal sealed class DesignConversionDpoCorpusAdmissionAuthority
{
    private static readonly ConditionalWeakTable<
        JsonlDesignConversionLocalArtifactDb,
        DesignConversionDpoCorpusAdmissionAuthority> Authorities = new();

    internal const string InvalidErrorCode =
        "DESIGN_CONVERSION_DPO_CORPUS_ADMISSION_INVALID";
    internal const string ForbiddenSelfLabelErrorCode = "forbidden_self_label";
    internal const string ReplayErrorCode =
        "DESIGN_CONVERSION_DPO_CORPUS_ADMISSION_REPLAY";
    internal const string RequiredErrorCode =
        "DESIGN_CONVERSION_DPO_CORPUS_ADMISSION_REQUIRED";

    private const string ReservationJournalFileName =
        "design-conversion-dpo-corpus-admission-reservations.v1.jsonl";
    private const string ReservationLeaseFileName =
        "design-conversion-dpo-corpus-admission-reservations.v1.lock";
    private const int ReservationLeaseAttempts = 200;
    private const int ReservationLeaseRetryMilliseconds = 5;
    private const int MaximumReservationJournalBytes = 4 * 1024 * 1024;

    private readonly JsonlDesignConversionLocalArtifactDb _artifactDb;
    private readonly object _sync = new();
    private readonly HashSet<string> _admittedReceipts = new(StringComparer.Ordinal);

    internal DesignConversionDpoCorpusAdmissionAuthority(
        JsonlDesignConversionLocalArtifactDb artifactDb)
    {
        _artifactDb = artifactDb ?? throw new ArgumentNullException(nameof(artifactDb));
    }

    internal static DesignConversionDpoCorpusAdmissionAuthority ForArtifactDb(
        JsonlDesignConversionLocalArtifactDb artifactDb)
        => Authorities.GetValue(
            artifactDb,
            static db => new DesignConversionDpoCorpusAdmissionAuthority(db));

    internal OperationResult<DesignConversionDpoCorpusAdmission> Issue(
        IReadOnlyList<DesignConversionDpoTrainingRecord> orderedRecords,
        DesignConversionCandidatePayloadCustodyStore.TrainingCapability custodyCapability,
        DesignConversionTeacherAuthoredEvaluationPair pair,
        DesignConversionIndependentPreferenceEvidence receipt,
        DesignConversionDpoDataset canonicalSourceProof,
        DateTimeOffset observedAtUtc,
        string requestBindingSha256)
    {
        var projection = ProjectAdmission(
            orderedRecords,
            custodyCapability,
            pair,
            receipt,
            canonicalSourceProof,
            observedAtUtc,
            requestBindingSha256);
        if (!projection.IsSuccess || projection.Value is null)
            return projection.MapError<DesignConversionDpoCorpusAdmission>();

        var admitted = projection.Value;
        var durableReservation = TryReserveDurably(
            receipt,
            admitted.SourceProofSha256,
            admitted.AggregateCommitmentSha256,
            admitted.RequestBindingSha256,
            observedAtUtc);
        if (!durableReservation.IsSuccess)
            return durableReservation.MapError<DesignConversionDpoCorpusAdmission>();

        lock (_sync)
        {
            if (!_admittedReceipts.Add(receipt.ReceiptSha256))
            {
                return OperationResult<DesignConversionDpoCorpusAdmission>.Fail(
                    "The independent preference receipt was already admitted.",
                    ReplayErrorCode);
            }
        }

        return MaterializeAdmission(admitted);
    }

    internal OperationResult<DesignConversionDpoCorpusAdmission> RecoverReserved(
        IReadOnlyList<DesignConversionDpoTrainingRecord> orderedRecords,
        DesignConversionCandidatePayloadCustodyStore.TrainingCapability custodyCapability,
        DesignConversionTeacherAuthoredEvaluationPair pair,
        DesignConversionIndependentPreferenceEvidence receipt,
        DesignConversionDpoDataset canonicalSourceProof,
        DateTimeOffset observedAtUtc,
        string requestBindingSha256)
    {
        var projection = ProjectAdmission(
            orderedRecords,
            custodyCapability,
            pair,
            receipt,
            canonicalSourceProof,
            observedAtUtc,
            requestBindingSha256);
        if (!projection.IsSuccess || projection.Value is null)
            return projection.MapError<DesignConversionDpoCorpusAdmission>();

        var admitted = projection.Value;
        var reservation = TryFindExactDurableReservation(
            receipt,
            admitted.SourceProofSha256,
            admitted.AggregateCommitmentSha256,
            admitted.RequestBindingSha256);
        return reservation.IsSuccess
            ? MaterializeAdmission(admitted)
            : reservation.MapError<DesignConversionDpoCorpusAdmission>();
    }

    private OperationResult<AdmissionProjection> ProjectAdmission(
        IReadOnlyList<DesignConversionDpoTrainingRecord> orderedRecords,
        DesignConversionCandidatePayloadCustodyStore.TrainingCapability custodyCapability,
        DesignConversionTeacherAuthoredEvaluationPair pair,
        DesignConversionIndependentPreferenceEvidence receipt,
        DesignConversionDpoDataset canonicalSourceProof,
        DateTimeOffset observedAtUtc,
        string requestBindingSha256)
    {
        if (orderedRecords is null || orderedRecords.Count != 2 ||
            pair is null || receipt is null || canonicalSourceProof is null ||
            custodyCapability is null)
        {
            return InvalidProjection(
                "Exactly one custody-bound Critic/Improver aggregate is required.");
        }

        var critic = orderedRecords[0];
        var improver = orderedRecords[1];
        if (critic is null || improver is null ||
            !string.Equals(critic.UnitId, DesignConversionDpoUnits.CriticUnit, StringComparison.Ordinal) ||
            !string.Equals(improver.UnitId, DesignConversionDpoUnits.ImproverUnit, StringComparison.Ordinal) ||
            ReferenceEquals(critic, improver) ||
            string.Equals(critic.RecordId, improver.RecordId, StringComparison.Ordinal))
        {
            return InvalidProjection(
                "Corpus admission requires ordered Critic then Improver records.");
        }

        foreach (var record in orderedRecords)
        {
            var metadata = record.Provenance?.Metadata;
            if (metadata is not null &&
                metadata.TryGetValue("runtime_teacher_selected_preference", out var selected))
            {
                return IsTrue(selected)
                    ? OperationResult<AdmissionProjection>.Fail(
                        "A teacher-selected preference cannot enter the corpus.",
                        ForbiddenSelfLabelErrorCode)
                    : InvalidProjection(
                        "The self-label marker may be stamped only by corpus admission.");
            }

            if (!IsTrue(Read(metadata, "runtime_teacher_authored_candidate")) ||
                !IsTrue(Read(metadata, "independent_preference_proven")) ||
                !IsTrue(Read(metadata, "runtime_rejected_independently_evaluated")) ||
                !IsTrue(Read(metadata, "approved_for_training")) ||
                !string.Equals(Read(metadata, "approval_status"), "approved", StringComparison.Ordinal) ||
                !IsTrue(Read(metadata, "training_candidate_only")) ||
                IsTrue(Read(metadata, "training_grade")) ||
                !IsTrue(Read(metadata, "not_training_grade")) ||
                !IsTrue(Read(metadata, "product_not_ready")) ||
                !IsFalse(Read(metadata, "raw_independent_scorer_request_persisted")) ||
                !IsFalse(Read(metadata, "raw_independent_scorer_output_persisted")) ||
                !string.Equals(
                    Read(metadata, "independent_preference_receipt_sha256"),
                    receipt.ReceiptSha256,
                    StringComparison.Ordinal) ||
                !int.TryParse(
                    Read(metadata, "independent_preference_attempt_number"),
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out var attempt) ||
                attempt != pair.AttemptNumber)
            {
                return InvalidProjection(
                    "The review-only teacher aggregate is not exactly admission eligible.");
            }

            var materialized = JsonlDesignConversionLocalArtifactDb
                .ValidateAndMaterializePayloadCustody(record);
            if (!materialized.IsSuccess || materialized.Value is null ||
                !DesignConversionCandidatePayloadCustodyStore.Authorizes(
                    custodyCapability,
                    record))
            {
                return InvalidProjection(
                    "Exact committed payload custody authority is required.");
            }
        }

        var artifactRootBinding =
            DesignConversionIndependentPreferenceEvidenceAssociation
                .BuildArtifactRootBinding(_artifactDb);
        if (!DesignConversionDpoGenerator
                .TryValidateIndependentPreferenceEvidenceForRuntime(
                    pair,
                    receipt,
                    artifactRootBinding,
                    observedAtUtc,
                    out _))
        {
            return InvalidProjection(
                "Fresh independently-issued preference evidence is required.");
        }

        if (!HasExactPairBinding(critic, improver, pair, receipt) ||
            !DesignConversionDpoDatasetBuilder.TryAuthorizeCanonicalSourceProof(
                canonicalSourceProof,
                orderedRecords,
                out var sourceProofSha256))
        {
            return InvalidProjection(
                "The exact independently-ranked aggregate and canonical source proof are required.");
        }

        var effectiveRequestBindingSha256 = string.IsNullOrWhiteSpace(requestBindingSha256)
            ? DesignConversionDpoGenerator.Sha256Ref(string.Join(
                "|",
                "design-conversion-dpo-corpus-admission-direct-request-v1",
                artifactRootBinding,
                receipt.ReceiptSha256))
            : requestBindingSha256;
        var aggregateCommitmentSha256 = BuildAggregateCommitmentSha256(
            orderedRecords,
            pair,
            receipt,
            sourceProofSha256,
            effectiveRequestBindingSha256);
        var derivatives = orderedRecords.Select(record =>
        {
            var materialized = JsonlDesignConversionLocalArtifactDb
                .ValidateAndMaterializePayloadCustody(record).Value!;
            var originalSecret = materialized.PayloadCustodySecret!;
            var metadata = new Dictionary<string, string>(
                materialized.Provenance.Metadata,
                StringComparer.Ordinal)
            {
                ["runtime_teacher_selected_preference"] = bool.FalseString,
                ["corpus_admission_authority"] = "server_owned_one_shot",
                ["corpus_admission_receipt_sha256"] = receipt.ReceiptSha256,
                ["corpus_admission_source_proof_sha256"] = sourceProofSha256
            };
            var derivative = materialized with
            {
                Provenance = materialized.Provenance with { Metadata = metadata }
            };
            var governance = DesignConversionDpoGenerator
                .BuildPayloadCustodyGovernanceSha256(derivative);
            var binding = DesignConversionDpoGenerator.BuildPayloadCustodyBinding(
                derivative,
                originalSecret.PromptSha256,
                originalSecret.ChosenResponseSha256,
                originalSecret.RejectedResponseSha256,
                governance);
            metadata[DesignConversionDpoGenerator
                .PayloadCustodyGovernanceSha256MetadataKey] = governance;
            metadata[DesignConversionDpoGenerator
                .PayloadCustodyBindingSha256MetadataKey] = binding;
            return derivative with
            {
                Provenance = derivative.Provenance with { Metadata = metadata },
                PayloadCustodySecret = new DesignConversionDpoPayloadCustodySecret(
                    originalSecret.Version,
                    originalSecret.Prompt,
                    originalSecret.ChosenResponse,
                    originalSecret.RejectedResponse,
                    originalSecret.PromptSha256,
                    originalSecret.ChosenResponseSha256,
                    originalSecret.RejectedResponseSha256,
                    governance,
                    binding)
            };
        }).ToArray();

        return OperationResult<AdmissionProjection>.Ok(
            new AdmissionProjection(
                orderedRecords.ToArray(),
                derivatives,
                canonicalSourceProof,
                receipt.ReceiptSha256,
                sourceProofSha256,
                aggregateCommitmentSha256,
                effectiveRequestBindingSha256));
    }

    private static OperationResult<DesignConversionDpoCorpusAdmission>
        MaterializeAdmission(AdmissionProjection admitted)
        => OperationResult<DesignConversionDpoCorpusAdmission>.Ok(
            new DesignConversionDpoCorpusAdmission(
                admitted.OriginalRecords,
                admitted.Derivatives,
                admitted.CanonicalSourceProof,
                admitted.ReceiptSha256,
                admitted.SourceProofSha256));

    private static OperationResult<AdmissionProjection> InvalidProjection(
        string message)
        => OperationResult<AdmissionProjection>.Fail(message, InvalidErrorCode);

    private sealed record AdmissionProjection(
        DesignConversionDpoTrainingRecord[] OriginalRecords,
        DesignConversionDpoTrainingRecord[] Derivatives,
        DesignConversionDpoDataset CanonicalSourceProof,
        string ReceiptSha256,
        string SourceProofSha256,
        string AggregateCommitmentSha256,
        string RequestBindingSha256);

    private OperationResult<bool> TryFindExactDurableReservation(
        DesignConversionIndependentPreferenceEvidence receipt,
        string sourceProofSha256,
        string aggregateCommitmentSha256,
        string requestBindingSha256)
    {
        var root = Path.GetFullPath(_artifactDb.AllowedArtifactRootDirectory);
        var rootBinding = DesignConversionIndependentPreferenceEvidenceAssociation
            .BuildArtifactRootBinding(_artifactDb);
        var journalPath = Path.Combine(root, ReservationJournalFileName);
        if (!File.Exists(journalPath))
        {
            return OperationResult<bool>.Fail(
                "The durable corpus admission reservation is missing.",
                ReplayErrorCode);
        }

        var journal = new FileInfo(journalPath);
        if (journal.Length > MaximumReservationJournalBytes)
        {
            return OperationResult<bool>.Fail(
                "The durable corpus admission reservation journal is invalid or full.",
                InvalidErrorCode);
        }

        var matchingReceiptCount = 0;
        var exactMatch = false;
        foreach (var line in File.ReadLines(journalPath, Encoding.UTF8))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            DurableReservationRow? existing;
            try
            {
                existing = JsonSerializer.Deserialize<DurableReservationRow>(
                    line);
            }
            catch (JsonException)
            {
                return OperationResult<bool>.Fail(
                    "The durable corpus admission reservation journal is invalid.",
                    InvalidErrorCode);
            }

            if (existing is null ||
                !string.Equals(existing.Schema, DurableReservationRow.SchemaVersion,
                    StringComparison.Ordinal) ||
                !string.Equals(existing.ArtifactRootBinding, rootBinding,
                    StringComparison.Ordinal) ||
                !DateTimeOffset.TryParse(
                    existing.ReservedAtUtc,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out _))
            {
                return OperationResult<bool>.Fail(
                    "The durable corpus admission reservation journal is invalid.",
                    InvalidErrorCode);
            }

            if (!string.Equals(
                    existing.ReceiptSha256,
                    receipt.ReceiptSha256,
                    StringComparison.Ordinal))
            {
                continue;
            }

            matchingReceiptCount++;
            exactMatch = string.Equals(
                             existing.SourceProofSha256,
                             sourceProofSha256,
                             StringComparison.Ordinal) &&
                         string.Equals(
                             existing.AggregateCommitmentSha256,
                             aggregateCommitmentSha256,
                             StringComparison.Ordinal) &&
                         string.Equals(
                             existing.RequestBindingSha256,
                             requestBindingSha256,
                             StringComparison.Ordinal);
        }

        if (matchingReceiptCount == 0)
        {
            return OperationResult<bool>.Fail(
                "The durable corpus admission reservation is missing.",
                ReplayErrorCode);
        }

        // Повтор допустим только при единственной записи этого чека. Даже две
        // одинаковые строки делают полномочие неоднозначным и закрывают путь.
        return matchingReceiptCount == 1 && exactMatch
            ? OperationResult<bool>.Ok(true)
            : OperationResult<bool>.Fail(
                "The durable corpus admission reservation is ambiguous or does not match the current request.",
                InvalidErrorCode);
    }

    private OperationResult<bool> TryReserveDurably(
        DesignConversionIndependentPreferenceEvidence receipt,
        string sourceProofSha256,
        string aggregateCommitmentSha256,
        string requestBindingSha256,
        DateTimeOffset observedAtUtc)
    {
        var root = Path.GetFullPath(_artifactDb.AllowedArtifactRootDirectory);
        var rootBinding = DesignConversionIndependentPreferenceEvidenceAssociation
            .BuildArtifactRootBinding(_artifactDb);
        var journalPath = Path.Combine(root, ReservationJournalFileName);
        var leasePath = Path.Combine(root, ReservationLeaseFileName);
        Directory.CreateDirectory(root);

        FileStream? lease = null;
        for (var attempt = 0; attempt < ReservationLeaseAttempts; attempt++)
        {
            try
            {
                lease = new FileStream(
                    leasePath,
                    FileMode.OpenOrCreate,
                    FileAccess.ReadWrite,
                    FileShare.None,
                    bufferSize: 1,
                    FileOptions.WriteThrough);
                break;
            }
            catch (IOException) when (attempt + 1 < ReservationLeaseAttempts)
            {
                Thread.Sleep(ReservationLeaseRetryMilliseconds);
            }
        }

        if (lease is null)
        {
            return OperationResult<bool>.Fail(
                "The durable corpus admission reservation lease is unavailable.",
                InvalidErrorCode);
        }

        using (lease)
        {
            if (File.Exists(journalPath))
            {
                var journal = new FileInfo(journalPath);
                if (journal.Length > MaximumReservationJournalBytes)
                {
                    return OperationResult<bool>.Fail(
                        "The durable corpus admission reservation journal is invalid or full.",
                        InvalidErrorCode);
                }

                foreach (var line in File.ReadLines(journalPath, Encoding.UTF8))
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    DurableReservationRow? existing;
                    try
                    {
                        existing = JsonSerializer.Deserialize<DurableReservationRow>(line);
                    }
                    catch (JsonException)
                    {
                        return OperationResult<bool>.Fail(
                            "The durable corpus admission reservation journal is invalid.",
                            InvalidErrorCode);
                    }

                    if (existing is null ||
                        !string.Equals(existing.Schema, DurableReservationRow.SchemaVersion,
                            StringComparison.Ordinal) ||
                        !string.Equals(existing.ArtifactRootBinding, rootBinding,
                            StringComparison.Ordinal))
                    {
                        return OperationResult<bool>.Fail(
                            "The durable corpus admission reservation journal is invalid.",
                            InvalidErrorCode);
                    }

                    if (string.Equals(existing.ReceiptSha256, receipt.ReceiptSha256,
                            StringComparison.Ordinal))
                    {
                        return OperationResult<bool>.Fail(
                            "The independent preference receipt was already durably admitted.",
                            ReplayErrorCode);
                    }
                }
            }

            var row = new DurableReservationRow(
                DurableReservationRow.SchemaVersion,
                rootBinding,
                receipt.ReceiptSha256,
                sourceProofSha256,
                aggregateCommitmentSha256,
                requestBindingSha256,
                observedAtUtc.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture));
            var encoded = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(row) + "\n");
            var existingLength = File.Exists(journalPath)
                ? new FileInfo(journalPath).Length
                : 0;
            if (existingLength > MaximumReservationJournalBytes - encoded.Length)
            {
                return OperationResult<bool>.Fail(
                    "The durable corpus admission reservation journal is invalid or full.",
                    InvalidErrorCode);
            }

            using var stream = new FileStream(
                journalPath,
                FileMode.Append,
                FileAccess.Write,
                FileShare.Read,
                bufferSize: 4096,
                FileOptions.WriteThrough);
            stream.Write(encoded);
            stream.Flush(flushToDisk: true);
            return OperationResult<bool>.Ok(true);
        }
    }

    private static string BuildAggregateCommitmentSha256(
        IReadOnlyList<DesignConversionDpoTrainingRecord> orderedRecords,
        DesignConversionTeacherAuthoredEvaluationPair pair,
        DesignConversionIndependentPreferenceEvidence receipt,
        string sourceProofSha256,
        string requestBindingSha256)
        => DesignConversionDpoGenerator.Sha256Ref(string.Join(
            "|",
            new[]
            {
                "design-conversion-dpo-corpus-admission-aggregate-v1",
                sourceProofSha256,
                requestBindingSha256,
                receipt.ReceiptSha256,
                receipt.ScorerRequestSha256,
                receipt.ScorerOutputSha256,
                receipt.EvaluationProfileConfigurationSha256,
                receipt.EvaluationModelSha256,
                pair.AttemptNumber.ToString(CultureInfo.InvariantCulture),
                pair.AuthoringEvidence.OutputHash
            }.Concat(orderedRecords.SelectMany(record => new[]
            {
                record.RecordId,
                record.UnitId,
                DesignConversionDpoDatasetBuilder.ComputeRecordIdentityHash(record),
                Read(record.Provenance.Metadata,
                    DesignConversionDpoGenerator.PayloadCustodyGovernanceSha256MetadataKey),
                Read(record.Provenance.Metadata,
                    DesignConversionDpoGenerator.PayloadCustodyBindingSha256MetadataKey)
            }))));

    private sealed record DurableReservationRow(
        string Schema,
        string ArtifactRootBinding,
        string ReceiptSha256,
        string SourceProofSha256,
        string AggregateCommitmentSha256,
        string RequestBindingSha256,
        string ReservedAtUtc)
    {
        internal const string SchemaVersion =
            "design_conversion_dpo_corpus_admission_reservation.v1";
    }

    private static bool HasExactPairBinding(
        DesignConversionDpoTrainingRecord critic,
        DesignConversionDpoTrainingRecord improver,
        DesignConversionTeacherAuthoredEvaluationPair pair,
        DesignConversionIndependentPreferenceEvidence receipt)
    {
        var criticItem = DesignConversionTrainingPipeline.ToCanonicalTrainingItem(critic);
        var improverItem = DesignConversionTrainingPipeline.ToCanonicalTrainingItem(improver);
        if (!string.Equals(criticItem.ChosenResponse, improverItem.ChosenResponse, StringComparison.Ordinal) ||
            !string.Equals(criticItem.RejectedResponse, improverItem.RejectedResponse, StringComparison.Ordinal))
            return false;

        foreach (var record in new[] { critic, improver })
        {
            var metadata = record.Provenance.Metadata;
            if (!string.Equals(Read(metadata, "teacher_authoring_output_hash"),
                    pair.AuthoringEvidence.OutputHash, StringComparison.Ordinal) ||
                !string.Equals(Read(metadata, "teacher_authoring_incumbent_ref"),
                    pair.AuthoringEvidence.IncumbentRef, StringComparison.Ordinal) ||
                !string.Equals(Read(metadata, "teacher_authoring_generated_ref"),
                    pair.AuthoringEvidence.GeneratedRef, StringComparison.Ordinal) ||
                !string.Equals(Read(metadata, "independent_preference_generated_evaluation_identity"),
                    receipt.GeneratedEvaluationIdentity, StringComparison.Ordinal) ||
                !string.Equals(Read(metadata, "independent_preference_incumbent_evaluation_identity"),
                    receipt.IncumbentEvaluationIdentity, StringComparison.Ordinal) ||
                !string.Equals(Read(metadata, "independent_preference_artifact_root_binding"),
                    receipt.ArtifactRootBinding, StringComparison.Ordinal))
                return false;
        }

        return true;
    }

    private static OperationResult<DesignConversionDpoCorpusAdmission> Invalid(string message)
        => OperationResult<DesignConversionDpoCorpusAdmission>.Fail(message, InvalidErrorCode);

    private static string Read(IReadOnlyDictionary<string, string>? metadata, string key)
        => metadata is not null && metadata.TryGetValue(key, out var value)
            ? value ?? string.Empty
            : string.Empty;

    private static bool IsTrue(string value)
        => bool.TryParse(value, out var parsed) && parsed;

    private static bool IsFalse(string value)
        => bool.TryParse(value, out var parsed) && !parsed;
}

/// <summary>
/// Process-only carrier for the real local-intake admission path.  Public
/// request and JSON contracts cannot reconstruct the independent receipt or
/// the builder-owned canonical split proof.
/// </summary>
internal sealed class DesignConversionDpoCorpusAdmissionRequest
{
    internal DesignConversionDpoCorpusAdmissionRequest(
        DesignConversionTeacherAuthoredEvaluationPair pair,
        DesignConversionIndependentPreferenceEvidence receipt,
        DesignConversionDpoDataset canonicalSourceProof,
        DateTimeOffset observedAtUtc)
    {
        Pair = pair;
        Receipt = receipt;
        CanonicalSourceProof = canonicalSourceProof;
        ObservedAtUtc = observedAtUtc;
    }

    internal DesignConversionTeacherAuthoredEvaluationPair Pair { get; }
    internal DesignConversionIndependentPreferenceEvidence Receipt { get; }
    internal DesignConversionDpoDataset CanonicalSourceProof { get; }
    internal DateTimeOffset ObservedAtUtc { get; }
}

/// <summary>
/// Opaque process capability. Dataset metadata is evidence about an admission;
/// it is never the capability itself.
/// </summary>
internal sealed class DesignConversionDpoCorpusAdmission
{
    private readonly object _sync = new();
    private int _state;
    private DesignConversionDpoAdmittedCorpus? _corpus;

    internal DesignConversionDpoCorpusAdmission(
        IReadOnlyList<DesignConversionDpoTrainingRecord> originals,
        IReadOnlyList<DesignConversionDpoTrainingRecord> derivatives,
        DesignConversionDpoDataset sourceProof,
        string receiptSha256,
        string sourceProofSha256)
    {
        Originals = originals;
        Derivatives = derivatives;
        SourceProof = sourceProof;
        ReceiptSha256 = receiptSha256;
        SourceProofSha256 = sourceProofSha256;
    }

    internal IReadOnlyList<DesignConversionDpoTrainingRecord> Originals { get; }
    internal IReadOnlyList<DesignConversionDpoTrainingRecord> Derivatives { get; }
    internal DesignConversionDpoDataset SourceProof { get; }
    internal string ReceiptSha256 { get; }
    internal string SourceProofSha256 { get; }

    internal bool TryBindCorpus(DesignConversionDpoAdmittedCorpus corpus)
    {
        lock (_sync)
        {
            if (_state != 0 || corpus is null)
                return false;
            _corpus = corpus;
            _state = 1;
            return true;
        }
    }

    internal bool Authorizes(
        DesignConversionDpoAdmittedCorpus corpus,
        IReadOnlyList<DesignConversionDpoTrainingRecord> records,
        DesignConversionDpoDataset dataset)
    {
        lock (_sync)
        {
            return _state is 1 or 2 &&
                   ReferenceEquals(_corpus, corpus) &&
                   ReferenceEquals(corpus.Dataset, dataset) &&
                   records.Count == Derivatives.Count &&
                   records.Select((record, index) =>
                           ReferenceEquals(record, Derivatives[index]))
                       .All(static exact => exact);
        }
    }

    internal bool TryConsumeForPipeline()
    {
        lock (_sync)
        {
            if (_state != 1)
                return false;
            _state = 2;
            return true;
        }
    }
}

internal sealed class DesignConversionDpoAdmittedCorpus
{
    internal DesignConversionDpoAdmittedCorpus(
        DesignConversionDpoCorpusAdmission admission,
        DesignConversionDpoDataset dataset,
        DesignConversionTrainingPipelineOptions options)
    {
        Admission = admission;
        Dataset = dataset;
        Options = options;
    }

    internal DesignConversionDpoCorpusAdmission Admission { get; }
    internal DesignConversionDpoDataset Dataset { get; }
    internal DesignConversionTrainingPipelineOptions Options { get; }
    internal IReadOnlyList<DesignConversionDpoTrainingRecord> Records =>
        Admission.Derivatives;
}
