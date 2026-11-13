// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
namespace TrainableRouter.Training;

using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;

/// <summary>Raw task adapter to the existing repository DPO resolver. It neither judges nor trains.</summary>
public static class CodeDocumentationRawDpoLessonAuthoring
{
    // This exact policy is part of the request identity, independently of the runtime profile.
    private const string Instruction = "Author BOTH answers for the same supplied raw C#12 documentation task. " +
        "Respect the requested answer language and requirements of raw_task. " +
        "chosen must explain the actual code accurately; rejected must make one plausible substantive semantic error. " +
        "Return only a JSON object with exactly two nonempty string fields: chosen and rejected. " +
        "Do not emit routing labels, a paid reference, judging labels, markdown fences or extra fields.";

    public static CodeDocumentationRepositoryOwnedDpoDatasetAuthoringContext CreateProductionContext(CodeDocumentationRawDpoLessonSource source)
        => CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CreateProductionContext(source.TeacherConfiguration,
            () => ContinuousLearningTp1AuthoringComposition.CreateClient(source.TeacherConfiguration), 1);

    public static OperationResult<RepositoryOwnedDpoDatasetRequestIdentity> PrepareRequest(
        CodeDocumentationRawDpoLessonSource source, CodeDocumentationRepositoryOwnedDpoDatasetAuthoringContext context)
    {
        if (source is null || context is null || context.MaxPairsToAuthor != 1 ||
            context.InferenceProfile != LocalQwenOllamaInferenceProfileAuthority.ManualCpuAuthoring ||
            context.DomainProfile != CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CSharpDomainProfile)
            return Invalid<RepositoryOwnedDpoDatasetRequestIdentity>();
        var unchanged = source.ValidateUnchanged();
        if (!unchanged.IsSuccess) return unchanged.MapError<RepositoryOwnedDpoDatasetRequestIdentity>();
        var valid = context.Validate();
        if (!valid.IsSuccess) return valid.MapError<RepositoryOwnedDpoDatasetRequestIdentity>();
        var teacher = source.TeacherConfiguration;
        if (context.OpenTeacher.ModelId != teacher.ModelId || context.OpenTeacher.ModelVersion != teacher.ExpectedOllamaManifestDigest ||
            context.OpenTeacher.ModelArtifactSha256 != teacher.ExpectedOllamaModelLayerDigest) return Invalid<RepositoryOwnedDpoDatasetRequestIdentity>();
        return RepositoryOwnedDpoDatasetManifestSealer.CreateRawRequestIdentity(source.Identity, context.OpenTeacher,
            new("raw-csharp-chinese-both-pair-generation", "v1", Hash(Instruction + "\n" + context.GenerationPolicy.ConfigSha256)),
            new("raw-code-documentation-repository-dpo-adapter", "v1", Hash(context.Creator.EffectiveConfigSha256 + "\n" + source.Identity.IntakeSha256)));
    }

    /// <summary>Strict disk reopen is factory-free. Missing/corrupt/conflicting datasets are never silently regenerated.</summary>
    public static OperationResult<RepositoryOwnedDpoDatasetOpenResult> Reopen(
        CodeDocumentationRawDpoLessonSource source, CodeDocumentationRepositoryOwnedDpoDatasetAuthoringContext context)
    {
        var request = PrepareRequest(source, context);
        if (!request.IsSuccess || request.Value is null) return request.MapError<RepositoryOwnedDpoDatasetOpenResult>();
        var opened = RepositoryOwnedDpoDatasetManifestSealer.OpenSealed(source.Address, request.Value);
        if (!opened.IsSuccess || opened.Value is null) return opened;
        return ValidateOpened(source, request.Value, opened.Value).IsSuccess ? opened : Invalid<RepositoryOwnedDpoDatasetOpenResult>();
    }

    public static async Task<OperationResult<RepositoryOwnedDpoDatasetResolution>> ResolveOrAuthorAsync(
        CodeDocumentationRawDpoLessonSource source, CodeDocumentationRepositoryOwnedDpoDatasetAuthoringContext context,
        CancellationToken cancellationToken = default)
    {
        var request = PrepareRequest(source, context);
        if (!request.IsSuccess || request.Value is null) return request.MapError<RepositoryOwnedDpoDatasetResolution>();
        var result = await RepositoryOwnedDpoDatasetResolver.OpenExistingOrCreateAndSealAsync(source.Address, request.Value,
            (_, token) => AuthorAsync(source, context, request.Value, token), cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess || result.Value is null) return result;
        var valid = ValidateOpened(source, request.Value, result.Value.Dataset);
        return valid.IsSuccess ? result : valid.MapError<RepositoryOwnedDpoDatasetResolution>();
    }

    private static OperationResult<bool> ValidateOpened(CodeDocumentationRawDpoLessonSource source,
        RepositoryOwnedDpoDatasetRequestIdentity request, RepositoryOwnedDpoDatasetOpenResult dataset)
    {
        if (dataset.Pairs.Count != 1 || dataset.Manifest.PairCount != 1 || dataset.Manifest.OrderedPairs.Count != 1 ||
            dataset.Manifest.RequestIdentity.RequestIdentitySha256 != request.RequestIdentitySha256) return Invalid<bool>();
        var pair = dataset.Pairs[0];
        if (pair.Prompt != source.OwnerInput || !ValidSides(pair.Chosen, pair.Rejected) ||
            pair.ChosenAuthorship?.AuthorKind != DPO.Models.DpoPairSideAuthorKind.OpenTeacher ||
            pair.RejectedAuthorship?.AuthorKind != DPO.Models.DpoPairSideAuthorKind.OpenTeacher ||
            !Governance(source, request).Equals(pair.GovernanceDecision) ||
            FileDpoPairStore.ComputeIdentity(pair, 5) != dataset.Manifest.OrderedPairs[0].PairId) return Invalid<bool>();
        return source.ValidateUnchanged();
    }

    private static async Task<OperationResult<IReadOnlyList<DpoPair>>> AuthorAsync(CodeDocumentationRawDpoLessonSource source,
        CodeDocumentationRepositoryOwnedDpoDatasetAuthoringContext context, RepositoryOwnedDpoDatasetRequestIdentity request,
        CancellationToken token)
    {
        var unchanged = source.ValidateUnchanged();
        if (!unchanged.IsSuccess) return unchanged.MapError<IReadOnlyList<DpoPair>>();
        token.ThrowIfCancellationRequested();
        // The complete raw input is JSON-escaped for transport only; the trainable prompt remains byte-identical.
        var prompt = JsonSerializer.Serialize(new { instruction = Instruction, raw_task = source.OwnerInput });
        try
        {
            var created = context.CreateTeacherClient();
            if (!created.IsSuccess || created.Value is null) return created.MapError<IReadOnlyList<DpoPair>>();
            using var client = created.Value;
            if (client.AuthoringProfile != context.InferenceProfile) return Invalid<IReadOnlyList<DpoPair>>();
            unchanged = source.ValidateUnchanged();
            if (!unchanged.IsSuccess) return unchanged.MapError<IReadOnlyList<DpoPair>>();
            var permit = new CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SingleUseTrainingTeacherCallPermit();
            var response = await client.InferAsync(prompt, permit, token).ConfigureAwait(false);
            if (!response.IsSuccess || response.Value is null) return response.MapError<IReadOnlyList<DpoPair>>();
            if (!permit.CallIssued) return Invalid<IReadOnlyList<DpoPair>>();
            var provenance = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.ValidateTeacherProvenance(response.Value,
                request, CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CSharpDomainProfile, context.InferenceProfile);
            if (!provenance.IsSuccess) return provenance.MapError<IReadOnlyList<DpoPair>>();
            var sides = Parse(response.Value.Output);
            if (sides is null) return Invalid<IReadOnlyList<DpoPair>>();
            var passport = response.Value.TrainingTeacherProvenance;
            var pair = new DpoPair(source.OwnerInput, sides.Value.Chosen, sides.Value.Rejected, source.Address.DomainSlug, DateTimeOffset.UnixEpoch)
            {
                TeacherProvenance = passport, GovernanceDecision = Governance(source, request),
                ChosenAuthorship = new(DPO.Models.DpoPairSideAuthorKind.OpenTeacher, passport),
                RejectedAuthorship = new(DPO.Models.DpoPairSideAuthorKind.OpenTeacher, passport)
            };
            // Bind precisely the decoded author bytes; do not normalize or append a reason to either side.
            var bound = DpoPairTrainingContentAuthority.Bind(pair);
            if (!bound.IsSuccess || bound.Value is null) return bound.MapError<IReadOnlyList<DpoPair>>();
            unchanged = source.ValidateUnchanged();
            return unchanged.IsSuccess ? OperationResult<IReadOnlyList<DpoPair>>.Ok(Array.AsReadOnly(new[] { bound.Value }))
                : unchanged.MapError<IReadOnlyList<DpoPair>>();
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        { return OperationResult<IReadOnlyList<DpoPair>>.Fail("The raw lesson authoring call failed; no implicit retry is permitted.", CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.TeacherFailedErrorCode); }
    }

    private static (string Chosen, string Rejected)? Parse(string output)
    {
        try
        {
            using var json = JsonDocument.Parse(output);
            var root = json.RootElement;
            if (root.ValueKind != JsonValueKind.Object) return null;
            var fields = root.EnumerateObject().ToArray();
            if (fields.Length != 2 || fields.Count(p => p.Name == "chosen" && p.Value.ValueKind == JsonValueKind.String) != 1 ||
                fields.Count(p => p.Name == "rejected" && p.Value.ValueKind == JsonValueKind.String) != 1) return null;
            var chosen = root.GetProperty("chosen").GetString()!;
            var rejected = root.GetProperty("rejected").GetString()!;
            return ValidSides(chosen, rejected) ? (chosen, rejected) : null;
        }
        catch (JsonException) { return null; }
    }

    private static bool ValidSides(string chosen, string rejected)
    {
        try
        {
            var strict = new UTF8Encoding(false, true);
            if (string.IsNullOrWhiteSpace(chosen) || string.IsNullOrWhiteSpace(rejected) ||
                strict.GetByteCount(chosen) is <= 0 or > 65_536 || strict.GetByteCount(rejected) is <= 0 or > 65_536) return false;
            // Operators, case and Unicode form can carry meaning; only the separate judge decides semantic contrast.
            return !StringComparer.Ordinal.Equals(chosen, rejected);
        }
        catch (EncoderFallbackException) { return false; }
    }

    private static TrainingDataGovernanceDecision Governance(CodeDocumentationRawDpoLessonSource source, RepositoryOwnedDpoDatasetRequestIdentity request)
        => new(true, false, new[] { "sha256:" + source.Identity.OperatorAuthorizationSha256,
            "sha256:" + source.Identity.SelectionReceiptSha256, "sha256:" + source.Identity.OwnerInputSha256,
            "sha256:" + request.RequestIdentitySha256, source.TeacherConfiguration.OutputLicenseRef });
    private static string Hash(string text) => CodeDocumentationRawDpoLessonSource.Hash(new UTF8Encoding(false, true).GetBytes(text));
    private static OperationResult<T> Invalid<T>() => OperationResult<T>.Fail("The raw C# lesson, profile, request or authored pair does not match the admitted source.",
        CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.TeacherOutputInvalidErrorCode);
}
