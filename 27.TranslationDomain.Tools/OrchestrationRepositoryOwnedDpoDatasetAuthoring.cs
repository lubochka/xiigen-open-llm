// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TranslationDomainTools;

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using ProjectWikiDomain;

/// <summary>
/// The local open-weight teacher context used only when the orchestration DPO
/// dataset is genuinely missing from the canonical repository. An exact sealed
/// hit must bypass this context so reruns load the same rows from disk instead
/// of asking Qwen to recreate them.
/// </summary>
public sealed record OrchestrationRepositoryOwnedDpoDatasetAuthoringContext(
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
                "Orchestration repository-owned DPO clean-miss authoring requires a local open-weight teacher client and a bounded positive pair count.",
                OrchestrationRepositoryOwnedDpoDatasetAuthoring.InvalidErrorCode);
        }

        var identity = RepositoryOwnedDpoDatasetManifestSealer
            .CreateRequestIdentity(
                [new string('0', 64)],
                OpenTeacher,
                GenerationPolicy,
                Creator);
        if (!identity.IsSuccess)
        {
            return OperationResult<bool>.Fail(
                "Orchestration repository-owned DPO authoring identities are invalid.",
                OrchestrationRepositoryOwnedDpoDatasetAuthoring.InvalidErrorCode);
        }

        return OperationResult<bool>.Ok(true);
    }
}

/// <summary>
/// Current orchestration reference rows and the exact identity that must match
/// the sealed dataset before the model is allowed to continue training.
/// </summary>
public sealed record OrchestrationRepositoryOwnedDpoDatasetAuthoringRequest(
    IReadOnlyList<OrchestrationTrainingPair> PaidReferenceRows,
    RepositoryOwnedDpoDatasetRequestIdentity RequestIdentity);

/// <summary>
/// Creates orchestration DPO pairs through the admitted local Qwen teacher while
/// keeping the paid/reference plan out of both stored DPO sides.
/// </summary>
public static class OrchestrationRepositoryOwnedDpoDatasetAuthoring
{
    public const string InvalidErrorCode =
        "ORCHESTRATION_REPOSITORY_OWNED_DPO_AUTHORING_INVALID";
    public const string TeacherFailedErrorCode =
        "ORCHESTRATION_REPOSITORY_OWNED_DPO_AUTHORING_TEACHER_FAILED";
    public const string TeacherOutputInvalidErrorCode =
        "ORCHESTRATION_REPOSITORY_OWNED_DPO_AUTHORING_OUTPUT_INVALID";
    public const string TeacherProvenanceMismatchErrorCode =
        "ORCHESTRATION_REPOSITORY_OWNED_DPO_AUTHORING_PROVENANCE_MISMATCH";

    private const string PairOutputInstruction =
        "Return only JSON with string fields: chosen, rejected, rejected_reason.";

    private static readonly JsonDocumentOptions JsonDocumentOptions = new()
    {
        AllowTrailingCommas = false,
        CommentHandling = JsonCommentHandling.Disallow,
        MaxDepth = 4
    };

    public static OrchestrationRepositoryOwnedDpoDatasetAuthoringContext
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
            "orchestration-repository-owned-dpo-generation-policy-v1",
            profile.ProfileId,
            profile.ConfigurationSha256,
            boundedPairCount.ToString(CultureInfo.InvariantCulture)));
        var creatorConfig = ComputeTextSha256(string.Join(
            "\n",
            "orchestration-repository-owned-dpo-authoring-v1",
            configuration.ModelId,
            configuration.ExpectedOllamaManifestDigest,
            configuration.ExpectedOllamaModelLayerDigest,
            policyConfig));

        return new OrchestrationRepositoryOwnedDpoDatasetAuthoringContext(
            createTeacherClient,
            new RepositoryOwnedDpoOpenTeacherIdentity(
                OpenWeightTeacherFamily.Qwen.ToString(),
                configuration.ModelId,
                configuration.ExpectedOllamaManifestDigest,
                LegalTrainingTeacherAdmissionAuthority
                    .OllamaModelLayerSha256DigestKind,
                configuration.ExpectedOllamaModelLayerDigest),
            new RepositoryOwnedDpoGenerationPolicyIdentity(
                "orchestration-local-qwen-dpo-pair-generation",
                "v1",
                policyConfig),
            new RepositoryOwnedDpoCreatorIdentity(
                "orchestration-repository-owned-dpo-dataset-authoring",
                "v1",
                creatorConfig),
            boundedPairCount);
    }

    public static OperationResult<
        OrchestrationRepositoryOwnedDpoDatasetAuthoringRequest> PrepareRequest(
            IReadOnlyList<OrchestrationTrainingPair> paidReferenceRows,
            OrchestrationRepositoryOwnedDpoDatasetAuthoringContext
                authoringContext)
    {
        if (authoringContext is null)
        {
            return OperationResult<
                OrchestrationRepositoryOwnedDpoDatasetAuthoringRequest>.Fail(
                    "Orchestration repository-owned DPO authoring needs an open-teacher authoring context.",
                    InvalidErrorCode);
        }

        var validation = authoringContext.Validate();
        if (!validation.IsSuccess)
        {
            return validation.MapError<
                OrchestrationRepositoryOwnedDpoDatasetAuthoringRequest>();
        }

        return PrepareRequest(
            paidReferenceRows,
            authoringContext.OpenTeacher,
            authoringContext.GenerationPolicy,
            authoringContext.Creator,
            authoringContext.MaxPairsToAuthor);
    }

    public static OperationResult<
        OrchestrationRepositoryOwnedDpoDatasetAuthoringRequest> PrepareRequest(
            IReadOnlyList<OrchestrationTrainingPair> paidReferenceRows,
            RepositoryOwnedDpoOpenTeacherIdentity openTeacher,
            RepositoryOwnedDpoGenerationPolicyIdentity generationPolicy,
            RepositoryOwnedDpoCreatorIdentity creator,
            int maxPairsToAuthor)
    {
        if (paidReferenceRows is null ||
            maxPairsToAuthor is <= 0 or > 1_000)
        {
            return OperationResult<
                OrchestrationRepositoryOwnedDpoDatasetAuthoringRequest>.Fail(
                    "Orchestration repository-owned DPO authoring needs valid paid-reference rows and a bounded pair count.",
                    InvalidErrorCode);
        }

        var rows = paidReferenceRows
            .Where(IsTrainReady)
            .Take(maxPairsToAuthor)
            .ToArray();
        if (rows.Length == 0)
        {
            return OperationResult<
                OrchestrationRepositoryOwnedDpoDatasetAuthoringRequest>.Fail(
                    "Orchestration repository-owned DPO authoring found no train-ready reference rows.",
                    InvalidErrorCode);
        }

        var identity = RepositoryOwnedDpoDatasetManifestSealer
            .CreateRequestIdentity(
                rows.Select(static (row, index) =>
                        ComputeTeacherRequestRowSha256(row, index))
                    .ToArray(),
                openTeacher,
                generationPolicy,
                creator);
        return identity.IsSuccess && identity.Value is not null
            ? OperationResult<
                OrchestrationRepositoryOwnedDpoDatasetAuthoringRequest>.Ok(
                    new OrchestrationRepositoryOwnedDpoDatasetAuthoringRequest(
                        Array.AsReadOnly(rows),
                        identity.Value))
            : identity.MapError<
                OrchestrationRepositoryOwnedDpoDatasetAuthoringRequest>();
    }

    public static async Task<OperationResult<IReadOnlyList<DpoPair>>>
        AuthorPairsAsync(
            RepositoryOwnedDpoDatasetAddress address,
            OrchestrationRepositoryOwnedDpoDatasetAuthoringRequest request,
            OrchestrationRepositoryOwnedDpoDatasetAuthoringContext
                authoringContext,
            CancellationToken cancellationToken = default)
    {
        var authoringValidation = authoringContext.Validate();
        if (!authoringValidation.IsSuccess)
            return authoringValidation.MapError<IReadOnlyList<DpoPair>>();
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
            return OperationResult<IReadOnlyList<DpoPair>>.Fail(
                "Orchestration repository-owned DPO authoring request does not match the canonical address and open-teacher identity.",
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
            return OperationResult<IReadOnlyList<DpoPair>>.Fail(
                "The local open-weight teacher client could not be created for orchestration DPO authoring.",
                TeacherFailedErrorCode);
        }

        if (!createdClient.IsSuccess || createdClient.Value is null)
            return createdClient.MapError<IReadOnlyList<DpoPair>>();

        using var teacherClient = createdClient.Value;
        var pairs = new List<DpoPair>(request.PaidReferenceRows.Count);
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
                return authored.MapError<IReadOnlyList<DpoPair>>();

            pairs.Add(authored.Value);
        }

        return OperationResult<IReadOnlyList<DpoPair>>.Ok(
            Array.AsReadOnly(pairs.ToArray()));
    }

    private static async Task<OperationResult<DpoPair>> AuthorOnePairAsync(
        RepositoryOwnedDpoDatasetAddress address,
        RepositoryOwnedDpoDatasetRequestIdentity identity,
        OrchestrationTrainingPair paidReferenceRow,
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
            return OperationResult<DpoPair>.Fail(
                "The local open-weight teacher failed while authoring an orchestration DPO pair.",
                TeacherFailedErrorCode);
        }

        if (!teacher.IsSuccess || teacher.Value is null)
        {
            return OperationResult<DpoPair>.Fail(
                string.IsNullOrWhiteSpace(teacher.ErrorMessage)
                    ? "The local open-weight teacher did not return an orchestration DPO pair."
                    : teacher.ErrorMessage,
                string.IsNullOrWhiteSpace(teacher.ErrorCode)
                    ? TeacherFailedErrorCode
                    : teacher.ErrorCode);
        }

        if (!permit.CallIssued)
        {
            return OperationResult<DpoPair>.Fail(
                "The local open-weight teacher did not consume the one-call orchestration authoring permit.",
                TeacherFailedErrorCode);
        }

        var provenanceValidation = ValidateTeacherProvenance(
            teacher.Value,
            identity);
        if (!provenanceValidation.IsSuccess)
            return provenanceValidation.MapError<DpoPair>();

        var parsed = ParseTeacherOutput(teacher.Value.Output, paidReferenceRow);
        if (!parsed.IsSuccess || parsed.Value is null)
            return parsed.MapError<DpoPair>();

        var passport = teacher.Value.TrainingTeacherProvenance;
        var pair = new DpoPair(
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
                    ordinal))
        };

        // Человеческий смысл: file-store принимает rich DPO только когда паспорт
        // Qwen привязан к точному hash обучающего содержимого. Платный/reference
        // план уже отфильтрован выше и остаётся только в request-identity.
        var contentBound = DpoPairTrainingContentAuthority.Bind(pair);
        if (!contentBound.IsSuccess || contentBound.Value is null)
        {
            return OperationResult<DpoPair>.Fail(
                contentBound.ErrorMessage,
                string.IsNullOrWhiteSpace(contentBound.ErrorCode)
                    ? TeacherOutputInvalidErrorCode
                    : contentBound.ErrorCode);
        }

        // Человеческий смысл: авторство стороны можно записывать только после
        // того, как общий Bind добавил TrainingContentSha256 в паспорт Qwen.
        // Иначе сторона ссылалась бы на metadata-only паспорт без точных байтов.
        var boundPassport = contentBound.Value.TeacherProvenance!;
        var teacherAuthoredPair = contentBound.Value with
        {
            ChosenAuthorship = new DpoPairSideAuthorship(
                DPO.Models.DpoPairSideAuthorKind.OpenTeacher,
                boundPassport),
            RejectedAuthorship = new DpoPairSideAuthorship(
                DPO.Models.DpoPairSideAuthorKind.OpenTeacher,
                boundPassport)
        };
        var bound = DpoPairTrainingContentAuthority.Bind(teacherAuthoredPair);
        return bound.IsSuccess && bound.Value is not null
            ? bound
            : OperationResult<DpoPair>.Fail(
                bound.ErrorMessage,
                string.IsNullOrWhiteSpace(bound.ErrorCode)
                    ? TeacherOutputInvalidErrorCode
                    : bound.ErrorCode);
    }

    private static bool IsTrainReady(OrchestrationTrainingPair row)
        => row is not null &&
           !row.IsHeldOut &&
           IsBoundedText(row.PairId) &&
           IsBoundedText(row.Prompt) &&
           IsBoundedText(row.Chosen?.PlanText) &&
           IsBoundedText(row.RejectedText);

    private static string BuildTeacherPrompt(
        RepositoryOwnedDpoDatasetAddress address,
        RepositoryOwnedDpoDatasetRequestIdentity identity,
        OrchestrationTrainingPair row,
        int ordinal)
        => string.Join(
            "\n",
            "You are the allowed local open-weight Chinese teacher for programming-task orchestration DPO dataset authoring.",
            "Goal: create a DPO pair that helps the local orchestration model approach the paid/reference plan without storing that reference as either DPO side.",
            PairOutputInstruction,
            "Rules:",
            "- chosen must be a teacher-authored orchestration answer, not a verbatim copy of paid_reference_plan.",
            "- rejected must be a weaker orchestration answer, not a verbatim copy of paid_reference_plan.",
            "- rejected_reason must explain the concrete planning weakness in human language.",
            "- do not include paths, secrets, or unrelated commentary.",
            "Repository dataset:",
            $"domain={address.DomainSlug}",
            $"design_pattern={address.DesignPatternSlug}",
            $"dataset_version={address.DatasetVersion}",
            $"request_identity_sha256={identity.RequestIdentitySha256}",
            $"row_ordinal={ordinal}",
            $"pair_id={row.PairId}",
            $"mode={row.Mode}",
            $"expected_route={row.ExpectedRouteMode}",
            $"prompt={row.Prompt}",
            $"paid_reference_plan={row.Chosen.PlanText}",
            $"paid_reference_sha256={ComputeTextSha256(row.Chosen.PlanText)}",
            $"weak_candidate={row.RejectedText}");

    private static OperationResult<AuthoredPair> ParseTeacherOutput(
        string output,
        OrchestrationTrainingPair paidReferenceRow)
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
            MatchesPaidReference(chosen, paidReferenceRow.Chosen.PlanText) ||
            MatchesPaidReference(rejected, paidReferenceRow.Chosen.PlanText))
        {
            return OperationResult<AuthoredPair>.Fail(
                "The local open-weight teacher must author two non-empty orchestration DPO sides, and neither side may be the paid/reference plan.",
                TeacherOutputInvalidErrorCode);
        }

        return OperationResult<AuthoredPair>.Ok(new AuthoredPair(
            chosen!,
            rejected!,
            reason!));
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
                "The local open-weight teacher provenance does not match the sealed repository-owned orchestration DPO request identity.",
                TeacherProvenanceMismatchErrorCode);
        }

        return OperationResult<bool>.Ok(true);
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

    private static string BuildTrainerPrompt(OrchestrationTrainingPair row)
        => string.Join(
            "\n",
            row.Prompt,
            $"Mode: {row.Mode}",
            $"Expected route: {row.ExpectedRouteMode}",
            $"Flow: {row.FlowRef ?? row.Chosen.FlowRef}");

    private static string BuildRejectedSide(string rejected, string reason)
        => rejected.Contains("[WRONG:", StringComparison.Ordinal)
            ? rejected
            : $"{rejected} [WRONG: open_teacher_orchestration_rejected - {reason}]";

    // Человеческий смысл: команда продолжения должна строить тот же prompt,
    // который использовался при создании DPO-пары, но не должна копировать
    // внутренний алгоритм сборки и хэширования в Program.cs.
    internal static string BuildTrainerPromptForContinuationBinding(
        OrchestrationTrainingPair row)
        => BuildTrainerPrompt(row);

    // Человеческий смысл: проверочные ссылки связывают открытую DPO-пару с
    // исходной строкой. Обёртка намеренно оставляет один алгоритм ссылок и
    // хэшей внутри компонента, который создаёт DPO-набор.
    internal static IReadOnlyList<string> BuildGovernanceEvidenceRefsForContinuationBinding(
        RepositoryOwnedDpoDatasetAddress address,
        RepositoryOwnedDpoDatasetRequestIdentity identity,
        OrchestrationTrainingPair row,
        int ordinal)
        => BuildGovernanceEvidenceRefs(
            address,
            identity,
            row,
            ordinal);

    private static IReadOnlyList<string> BuildGovernanceEvidenceRefs(
        RepositoryOwnedDpoDatasetAddress address,
        RepositoryOwnedDpoDatasetRequestIdentity identity,
        OrchestrationTrainingPair row,
        int ordinal)
        =>
        [
            "sha256:" + ComputeTextSha256("orchestration-dpo-authoring\n" +
                                          address.ManifestRelativePath),
            "sha256:" + ComputeTextSha256("paid-reference-plan\n" +
                                          row.Chosen.PlanText),
            "sha256:" + ComputeTextSha256("source-pair\n" + row.PairId),
            "sha256:" + ComputeTextSha256("row-ordinal\n" +
                                          ordinal.ToString(
                                              CultureInfo.InvariantCulture)),
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

    private static string ComputeTeacherRequestRowSha256(
        OrchestrationTrainingPair row,
        int ordinal)
    {
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            writer.WriteNumber("row_ordinal", ordinal);
            writer.WriteString("pair_id", row.PairId);
            writer.WriteString("mode", row.Mode.ToString());
            writer.WriteString("prompt", row.Prompt);
            writer.WriteString("chosen_flow_ref", row.Chosen.FlowRef);
            writer.WriteString("chosen_title", row.Chosen.Title);
            writer.WriteString("chosen_source_path", row.Chosen.SourcePath);
            writer.WriteString("paid_reference_plan", row.Chosen.PlanText);
            writer.WriteString("weak_candidate", row.RejectedText);
            writer.WriteString("expected_route", row.ExpectedRouteMode.ToString());
            writer.WriteString("expected_boundary", row.ExpectedBoundarySource.ToString());
            writer.WriteString("expected_intent", row.ExpectedIntentKind.ToString());
            writer.WriteString("flow_ref", row.FlowRef);
            writer.WriteString("source_bundle_id", row.SourceBundleId);
            writer.WriteEndObject();
            writer.Flush();
        }

        return ComputeBytesSha256(buffer.ToArray());
    }

    private static string ComputeTextSha256(string value)
        => ComputeBytesSha256(Encoding.UTF8.GetBytes(value));

    private static string ComputeBytesSha256(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

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
                    "A single orchestration DPO teacher prompt may issue only one local open-weight teacher call.",
                    TeacherFailedErrorCode);
    }
}
