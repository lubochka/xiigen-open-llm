// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TranslationDomain;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using DPO.Models;

/// <summary>
/// The local open-weight teacher context used only when a repository-owned
/// Translation DPO dataset is missing. A sealed dataset hit must bypass this
/// context, so the teacher is never asked to recreate rows already on disk.
/// </summary>
public sealed record TranslationRepositoryOwnedDpoDatasetAuthoringContext(
    Func<OperationResult<ITrainingTeacherInferenceClient>> CreateTeacherClient,
    RepositoryOwnedDpoOpenTeacherIdentity OpenTeacher,
    RepositoryOwnedDpoGenerationPolicyIdentity GenerationPolicy,
    RepositoryOwnedDpoCreatorIdentity Creator,
    int MaxPairsToAuthor = 50)
{
    public OperationResult<bool> Validate()
    {
        if (CreateTeacherClient is null ||
            MaxPairsToAuthor is <= 0 or > 1_000)
        {
            return OperationResult<bool>.Fail(
                "Translation repository-owned DPO clean-miss authoring requires a local open-weight teacher client and a bounded positive pair count.",
                TranslationRepositoryOwnedDpoDatasetAuthoring.InvalidErrorCode);
        }

        // WHY: validate the immutable authoring passport without constructing
        // the network client. An exact sealed hit must remain creator-free as
        // well as teacher-call-free.
        var identity = RepositoryOwnedDpoDatasetManifestSealer
            .CreateRequestIdentity(
                [new string('0', 64)],
                OpenTeacher,
                GenerationPolicy,
                Creator);
        if (!identity.IsSuccess)
        {
            return OperationResult<bool>.Fail(
                "Translation repository-owned DPO authoring identities are invalid.",
                TranslationRepositoryOwnedDpoDatasetAuthoring.InvalidErrorCode);
        }

        return OperationResult<bool>.Ok(true);
    }
}

/// <summary>
/// Paid reference rows selected for one clean miss together with the exact
/// request identity that will be sealed beside the open-teacher DPO pairs.
/// </summary>
public sealed record TranslationRepositoryOwnedDpoDatasetAuthoringRequest(
    IReadOnlyList<TranslationTrainReadyPair> PaidReferenceRows,
    RepositoryOwnedDpoDatasetRequestIdentity RequestIdentity);

/// <summary>
/// Creates Translation DPO pairs with the allowed local open-weight teacher,
/// while keeping paid reference answers out of the persisted chosen/rejected
/// sides. The caller gives the paid answer to the teacher as a reference for
/// approximation; only the teacher-authored DPO sides are sealed in Git.
/// </summary>
public static class TranslationRepositoryOwnedDpoDatasetAuthoring
{
    public const string InvalidErrorCode =
        "TRANSLATION_REPOSITORY_OWNED_DPO_AUTHORING_INVALID";
    public const string TeacherFailedErrorCode =
        "TRANSLATION_REPOSITORY_OWNED_DPO_AUTHORING_TEACHER_FAILED";
    public const string TeacherOutputInvalidErrorCode =
        "TRANSLATION_REPOSITORY_OWNED_DPO_AUTHORING_OUTPUT_INVALID";
    public const string TeacherProvenanceMismatchErrorCode =
        "TRANSLATION_REPOSITORY_OWNED_DPO_AUTHORING_PROVENANCE_MISMATCH";

    private const string PairOutputInstruction =
        "Return only JSON with string fields: chosen, rejected, rejected_reason.";

    private static readonly JsonDocumentOptions JsonDocumentOptions = new()
    {
        AllowTrailingCommas = false,
        CommentHandling = JsonCommentHandling.Disallow,
        MaxDepth = 4
    };

    public static TranslationRepositoryOwnedDpoDatasetAuthoringContext
        CreateProductionContext(
            ContinuousLearningTp1DpoConfiguration configuration,
            Func<OperationResult<ITrainingTeacherInferenceClient>>
                createTeacherClient,
            int maxPairsToAuthor = 50)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(createTeacherClient);

        var boundedPairCount = Math.Min(
            maxPairsToAuthor,
            configuration.MaxTeacherCalls);
        var profile = LocalQwenOllamaInferenceProfileAuthority.Authoring;
        var policyConfig = ComputeTextSha256(string.Join(
            "\n",
            "translation-repository-owned-dpo-generation-policy-v1",
            profile.ProfileId,
            profile.ConfigurationSha256,
            boundedPairCount.ToString(
                System.Globalization.CultureInfo.InvariantCulture)));
        var creatorConfig = ComputeTextSha256(string.Join(
            "\n",
            "translation-repository-owned-dpo-authoring-v1",
            configuration.ModelId,
            configuration.ExpectedOllamaManifestDigest,
            configuration.ExpectedOllamaModelLayerDigest,
            policyConfig));

        return new TranslationRepositoryOwnedDpoDatasetAuthoringContext(
            createTeacherClient,
            new RepositoryOwnedDpoOpenTeacherIdentity(
                OpenWeightTeacherFamily.Qwen.ToString(),
                configuration.ModelId,
                configuration.ExpectedOllamaManifestDigest,
                LegalTrainingTeacherAdmissionAuthority
                    .OllamaModelLayerSha256DigestKind,
                configuration.ExpectedOllamaModelLayerDigest),
            new RepositoryOwnedDpoGenerationPolicyIdentity(
                "translation-local-qwen-dpo-pair-generation",
                "v1",
                policyConfig),
            new RepositoryOwnedDpoCreatorIdentity(
                "translation-repository-owned-dpo-dataset-authoring",
                "v1",
                creatorConfig),
            boundedPairCount);
    }

    public static OperationResult<
        TranslationRepositoryOwnedDpoDatasetAuthoringRequest> PrepareRequest(
            IReadOnlyList<TranslationTrainReadyPair> paidReferenceRows,
            TranslationRepositoryOwnedDpoDatasetAuthoringContext
                authoringContext)
    {
        if (authoringContext is null)
        {
            return OperationResult<
                TranslationRepositoryOwnedDpoDatasetAuthoringRequest>.Fail(
                    "Translation repository-owned DPO authoring needs an open-teacher authoring context.",
                    InvalidErrorCode);
        }

        var validation = authoringContext.Validate();
        if (!validation.IsSuccess ||
            paidReferenceRows is null || paidReferenceRows.Count == 0)
        {
            return OperationResult<
                TranslationRepositoryOwnedDpoDatasetAuthoringRequest>.Fail(
                    "Translation repository-owned DPO authoring needs valid paid-reference rows and an open-teacher authoring context.",
                    InvalidErrorCode);
        }

        var rows = paidReferenceRows
            .Where(IsTrainReady)
            .Take(authoringContext.MaxPairsToAuthor)
            .ToArray();
        if (rows.Length == 0)
        {
            return OperationResult<
                TranslationRepositoryOwnedDpoDatasetAuthoringRequest>.Fail(
                    "Translation repository-owned DPO authoring found no train-ready paid-reference rows.",
                    InvalidErrorCode);
        }

        var identity = RepositoryOwnedDpoDatasetManifestSealer
            .CreateRequestIdentity(
                rows.Select(static (row, index) =>
                        ComputeTeacherRequestRowSha256(row, index))
                    .ToArray(),
                authoringContext.OpenTeacher,
                authoringContext.GenerationPolicy,
                authoringContext.Creator);
        return identity.IsSuccess && identity.Value is not null
            ? OperationResult<
                TranslationRepositoryOwnedDpoDatasetAuthoringRequest>.Ok(
                    new TranslationRepositoryOwnedDpoDatasetAuthoringRequest(
                        Array.AsReadOnly(rows),
                        identity.Value))
            : identity.MapError<
                TranslationRepositoryOwnedDpoDatasetAuthoringRequest>();
    }

    public static async Task<OperationResult<
        IReadOnlyList<ContinuousLearning.DpoPair>>>
        AuthorPairsAsync(
            RepositoryOwnedDpoDatasetAddress address,
            TranslationRepositoryOwnedDpoDatasetAuthoringRequest request,
            TranslationRepositoryOwnedDpoDatasetAuthoringContext
                authoringContext,
            CancellationToken cancellationToken = default)
    {
        var authoringValidation = authoringContext.Validate();
        if (!authoringValidation.IsSuccess)
            return authoringValidation.MapError<
                IReadOnlyList<ContinuousLearning.DpoPair>>();
        if (!RepositoryOwnedTrainingArtifactLayout.IsValidDpoDatasetAddress(
                address) ||
            request is null ||
            request.PaidReferenceRows is null ||
            request.PaidReferenceRows.Count == 0 ||
            !RepositoryOwnedDpoDatasetManifestSealer.IsValidRequestIdentity(
                request.RequestIdentity) ||
            request.RequestIdentity.OpenTeacher != authoringContext.OpenTeacher ||
            request.RequestIdentity.GenerationPolicy !=
                authoringContext.GenerationPolicy ||
            request.RequestIdentity.Creator != authoringContext.Creator)
        {
            return OperationResult<
                IReadOnlyList<ContinuousLearning.DpoPair>>.Fail(
                "Translation repository-owned DPO authoring request does not match the canonical address and open-teacher identity.",
                InvalidErrorCode);
        }

        OperationResult<ITrainingTeacherInferenceClient> createdClient;
        try
        {
            createdClient = authoringContext.CreateTeacherClient();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<
                IReadOnlyList<ContinuousLearning.DpoPair>>.Fail(
                "The local open-weight teacher client could not be created for Translation DPO authoring.",
                TeacherFailedErrorCode);
        }
        if (!createdClient.IsSuccess || createdClient.Value is null)
            return createdClient.MapError<
                IReadOnlyList<ContinuousLearning.DpoPair>>();

        using var teacherClient = createdClient.Value;
        var pairs = new List<ContinuousLearning.DpoPair>(
            request.PaidReferenceRows.Count);
        for (var index = 0; index < request.PaidReferenceRows.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var authored = await AuthorOnePairAsync(
                    address,
                    request.RequestIdentity,
                    request.PaidReferenceRows[index],
                    index,
                    teacherClient,
                    cancellationToken)
                .ConfigureAwait(false);
            if (!authored.IsSuccess || authored.Value is null)
                return authored.MapError<
                    IReadOnlyList<ContinuousLearning.DpoPair>>();

            pairs.Add(authored.Value);
        }

        return OperationResult<
            IReadOnlyList<ContinuousLearning.DpoPair>>.Ok(
            Array.AsReadOnly(pairs.ToArray()));
    }

    private static async Task<OperationResult<ContinuousLearning.DpoPair>>
        AuthorOnePairAsync(
        RepositoryOwnedDpoDatasetAddress address,
        RepositoryOwnedDpoDatasetRequestIdentity identity,
        TranslationTrainReadyPair paidReferenceRow,
        int ordinal,
        ITrainingTeacherInferenceClient teacherClient,
        CancellationToken cancellationToken)
    {
        var permit = new SingleUseTrainingTeacherCallPermit();
        var prompt = BuildTeacherPrompt(address, identity, paidReferenceRow, ordinal);
        OperationResult<TrainingTeacherInferenceResult> teacher;
        try
        {
            teacher = await teacherClient
                .InferAsync(prompt, permit, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<ContinuousLearning.DpoPair>.Fail(
                "The local open-weight teacher failed while authoring a Translation DPO pair.",
                TeacherFailedErrorCode);
        }

        if (!teacher.IsSuccess || teacher.Value is null)
        {
            return OperationResult<ContinuousLearning.DpoPair>.Fail(
                string.IsNullOrWhiteSpace(teacher.ErrorMessage)
                    ? "The local open-weight teacher did not return a Translation DPO pair."
                    : teacher.ErrorMessage,
                string.IsNullOrWhiteSpace(teacher.ErrorCode)
                    ? TeacherFailedErrorCode
                    : teacher.ErrorCode);
        }
        if (!permit.CallIssued)
        {
            return OperationResult<ContinuousLearning.DpoPair>.Fail(
                "The local open-weight teacher did not consume the one-call authoring permit.",
                TeacherFailedErrorCode);
        }

        var provenanceValidation = ValidateTeacherProvenance(
            teacher.Value,
            identity);
        if (!provenanceValidation.IsSuccess)
            return provenanceValidation.MapError<ContinuousLearning.DpoPair>();

        var parsed = ParseTeacherOutput(teacher.Value.Output, paidReferenceRow);
        if (!parsed.IsSuccess || parsed.Value is null)
            return parsed.MapError<ContinuousLearning.DpoPair>();

        var passport = teacher.Value.TrainingTeacherProvenance;
        var pair = new ContinuousLearning.DpoPair(
            Prompt: BuildTrainerPrompt(paidReferenceRow),
            Chosen: parsed.Value.Chosen,
            Rejected: BuildRejectedSide(
                parsed.Value.Rejected,
                parsed.Value.RejectedReason),
            Domain: address.DomainSlug,
            CapturedAt: DateTimeOffset.UnixEpoch)
        {
            TeacherProvenance = passport,
            GovernanceDecision = new TrainingDataGovernanceDecision(
                licenseAccepted: true,
                isQuarantined: false,
                evidenceRefs: BuildGovernanceEvidenceRefs(
                    address,
                    identity,
                    paidReferenceRow,
                    ordinal)),
            ChosenAuthorship = new DpoPairSideAuthorship(
                DpoPairSideAuthorKind.OpenTeacher,
                passport),
            RejectedAuthorship = new DpoPairSideAuthorship(
                DpoPairSideAuthorKind.OpenTeacher,
                passport)
        };

        // Bind after composing the final stored text. This writes the exact
        // Prompt/Chosen/Rejected digest into the teacher passport, so a later
        // trainer cannot swap in the paid reference answer unnoticed.
        var bound = DpoPairTrainingContentAuthority.Bind(pair);
        return bound.IsSuccess && bound.Value is not null
            ? bound
            : OperationResult<ContinuousLearning.DpoPair>.Fail(
                bound.ErrorMessage,
                string.IsNullOrWhiteSpace(bound.ErrorCode)
                    ? TeacherOutputInvalidErrorCode
                    : bound.ErrorCode);
    }

    private static bool IsTrainReady(TranslationTrainReadyPair row)
        => row is not null &&
           string.Equals(
               row.ValidationOutcome,
               "train_ready",
               StringComparison.Ordinal) &&
           row.ValidationErrors is not { Count: > 0 } &&
           IsBoundedText(row.Prompt) &&
           IsBoundedText(row.ASide) &&
           IsBoundedText(row.Chosen) &&
           IsBoundedText(row.Rejected);

    private static string BuildTeacherPrompt(
        RepositoryOwnedDpoDatasetAddress address,
        RepositoryOwnedDpoDatasetRequestIdentity identity,
        TranslationTrainReadyPair row,
        int ordinal)
        => string.Join(
            "\n",
            "You are the allowed local open-weight Chinese teacher for Translation DPO dataset authoring.",
            "Goal: create a DPO pair that helps the local model approach the paid reference answer without storing the paid reference as either DPO side.",
            PairOutputInstruction,
            "Rules:",
            "- chosen must be a teacher-authored answer, not a verbatim copy of paid_reference.",
            "- rejected must be a weaker answer, not a verbatim copy of paid_reference.",
            "- rejected_reason must explain the concrete weakness in human language.",
            "- do not include paths, secrets, or unrelated commentary.",
            "Repository dataset:",
            $"domain={address.DomainSlug}",
            $"design_pattern={address.DesignPatternSlug}",
            $"dataset_version={address.DatasetVersion}",
            $"request_identity_sha256={identity.RequestIdentitySha256}",
            $"row_ordinal={ordinal}",
            $"row_id={row.Id}",
            $"row_source={row.Source}",
            $"direction={row.Direction}",
            $"prompt={row.Prompt}",
            $"input={row.ASide}",
            $"paid_reference={row.Chosen}",
            $"paid_reference_sha256={ComputeTextSha256(row.Chosen)}",
            $"weak_candidate={row.Rejected}");

    private static OperationResult<AuthoredPair> ParseTeacherOutput(
        string output,
        TranslationTrainReadyPair paidReferenceRow)
    {
        if (!IsBoundedText(output))
        {
            return OperationResult<AuthoredPair>.Fail(
                "The local open-weight teacher output is empty or too large.",
                TeacherOutputInvalidErrorCode);
        }

        AuthoredPairDto? dto;
        try
        {
            using var document = JsonDocument.Parse(output, JsonDocumentOptions);
            if (document.RootElement.ValueKind is not JsonValueKind.Object ||
                document.RootElement.EnumerateObject().Count() != 3 ||
                !HasOnlyStringProperty(document.RootElement, "chosen") ||
                !HasOnlyStringProperty(document.RootElement, "rejected") ||
                !HasOnlyStringProperty(document.RootElement, "rejected_reason"))
            {
                return OperationResult<AuthoredPair>.Fail(
                    "The local open-weight teacher must return exactly the three required JSON string fields.",
                    TeacherOutputInvalidErrorCode);
            }

            dto = new AuthoredPairDto(
                document.RootElement.GetProperty("chosen").GetString(),
                document.RootElement.GetProperty("rejected").GetString(),
                document.RootElement.GetProperty("rejected_reason").GetString());
        }
        catch (JsonException)
        {
            return OperationResult<AuthoredPair>.Fail(
                "The local open-weight teacher must return JSON with chosen, rejected and rejected_reason.",
                TeacherOutputInvalidErrorCode);
        }

        var chosen = NormalizeTeacherSide(dto?.Chosen);
        var rejected = NormalizeTeacherSide(dto?.Rejected);
        var reason = NormalizeReason(dto?.RejectedReason);
        if (!IsBoundedText(chosen) ||
            !IsBoundedText(rejected) ||
            !IsBoundedText(reason) ||
            string.Equals(chosen, rejected, StringComparison.Ordinal) ||
            MatchesPaidReference(chosen, paidReferenceRow.Chosen) ||
            MatchesPaidReference(rejected, paidReferenceRow.Chosen))
        {
            return OperationResult<AuthoredPair>.Fail(
                "The local open-weight teacher must author two non-empty DPO sides, and neither side may be the paid reference answer.",
                TeacherOutputInvalidErrorCode);
        }

        return OperationResult<AuthoredPair>.Ok(new AuthoredPair(
            chosen!,
            rejected!,
            reason!));
    }

    private static bool HasOnlyStringProperty(
        JsonElement root,
        string propertyName)
    {
        var matches = root.EnumerateObject()
            .Where(property => string.Equals(
                property.Name,
                propertyName,
                StringComparison.Ordinal))
            .ToArray();
        return matches.Length == 1 &&
               matches[0].Value.ValueKind is JsonValueKind.String;
    }

    private static OperationResult<bool> ValidateTeacherProvenance(
        TrainingTeacherInferenceResult teacher,
        RepositoryOwnedDpoDatasetRequestIdentity identity)
    {
        var provenance = teacher.TrainingTeacherProvenance;
        var expectedTeacher = identity.OpenTeacher;
        if (!LocalQwenOllamaInferenceProfileAuthority.IsCanonicalAuthoring(
                teacher.InferenceProfile) ||
            provenance is null ||
            provenance.SourceKind is not TrainingTeacherSourceKind
                .ExternalTeacherOutput ||
            provenance.Family is not OpenWeightTeacherFamily.Qwen ||
            provenance.AccessMode is not TrainingTeacherAccessMode
                .LocalHostedOpenWeight ||
            provenance.HostingProvider is not TrainingTeacherHostingProvider
                .Ollama ||
            provenance.ModelLicense is not TrainingTeacherModelLicense.Apache20 ||
            provenance.ClosedProprietaryModel ||
            !provenance.TeacherOutputUsedForTraining ||
            provenance.InvocationCount <= 0 ||
            !string.Equals(
                provenance.ModelId,
                expectedTeacher.ModelId,
                StringComparison.Ordinal) ||
            !string.Equals(
                provenance.ModelVersion,
                expectedTeacher.ModelVersion,
                StringComparison.Ordinal) ||
            !string.Equals(
                provenance.ModelArtifactDigestKind,
                expectedTeacher.ModelArtifactDigestKind,
                StringComparison.Ordinal) ||
            !string.Equals(
                provenance.ModelArtifactDigest,
                expectedTeacher.ModelArtifactSha256,
                StringComparison.Ordinal))
        {
            return OperationResult<bool>.Fail(
                "The local open-weight teacher provenance does not match the sealed repository-owned DPO request identity.",
                TeacherProvenanceMismatchErrorCode);
        }

        return OperationResult<bool>.Ok(true);
    }

    private static string BuildTrainerPrompt(TranslationTrainReadyPair row)
        => $"{row.Prompt}\nInput: {row.ASide}";

    private static string BuildRejectedSide(string rejected, string reason)
        => rejected.Contains("[WRONG:", StringComparison.Ordinal)
            ? rejected
            : $"{rejected} [WRONG: open_teacher_dpo_rejected - {reason}]";

    private static IReadOnlyList<string> BuildGovernanceEvidenceRefs(
        RepositoryOwnedDpoDatasetAddress address,
        RepositoryOwnedDpoDatasetRequestIdentity identity,
        TranslationTrainReadyPair row,
        int ordinal)
        =>
        [
            "sha256:" + ComputeTextSha256("translation-dpo-authoring\n" +
                                          address.ManifestRelativePath),
            "sha256:" + ComputeTextSha256("paid-reference\n" + row.Chosen),
            "sha256:" + ComputeTextSha256("source-row\n" + row.Id),
            "sha256:" + ComputeTextSha256("source-input\n" + row.ASide),
            "sha256:" + ComputeTextSha256("row-ordinal\n" +
                                          ordinal.ToString(
                                              System.Globalization.CultureInfo
                                                  .InvariantCulture)),
            "sha256:" + identity.RequestIdentitySha256
        ];

    private static string? NormalizeTeacherSide(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();

    private static string? NormalizeReason(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();

    private static bool MatchesPaidReference(string? side, string paidReference)
        => string.Equals(
            side?.Trim(),
            paidReference?.Trim(),
            StringComparison.Ordinal);

    private static bool IsBoundedText(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           Encoding.UTF8.GetByteCount(value) <= 65_536 &&
           !value.Any(static character => char.IsControl(character) &&
                                          character is not '\n' and not '\r' and not '\t');

    private static string ComputeTextSha256(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant();

    private static string ComputeTeacherRequestRowSha256(
        TranslationTrainReadyPair row,
        int ordinal)
    {
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            writer.WriteNumber("row_ordinal", ordinal);
            writer.WriteString("row_id", row.Id);
            writer.WriteString("row_source", row.Source);
            writer.WriteString("direction", row.Direction);
            writer.WriteString("prompt", row.Prompt);
            writer.WriteString("input", row.ASide);
            writer.WriteString("paid_reference", row.Chosen);
            writer.WriteString("weak_candidate", row.Rejected);
            writer.WriteEndObject();
            writer.Flush();
        }

        return Convert.ToHexString(SHA256.HashData(buffer.ToArray()))
            .ToLowerInvariant();
    }

    private sealed record AuthoredPair(
        string Chosen,
        string Rejected,
        string RejectedReason);

    private sealed record AuthoredPairDto(
        [property: JsonPropertyName("chosen")] string? Chosen,
        [property: JsonPropertyName("rejected")] string? Rejected,
        [property: JsonPropertyName("rejected_reason")] string? RejectedReason);

    private sealed class SingleUseTrainingTeacherCallPermit
        : ITrainingTeacherCallPermit
    {
        private int _issued;

        public bool CallIssued => Volatile.Read(ref _issued) != 0;

        public OperationResult<int> TryIssue()
            => Interlocked.CompareExchange(ref _issued, 1, 0) == 0
                ? OperationResult<int>.Ok(1)
                : OperationResult<int>.Fail(
                    "The Translation DPO authoring permit allows exactly one local teacher call for one row.",
                    TeacherFailedErrorCode);
    }
}
