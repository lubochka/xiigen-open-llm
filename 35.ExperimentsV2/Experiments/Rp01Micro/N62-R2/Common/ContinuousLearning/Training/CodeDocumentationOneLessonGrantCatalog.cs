// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning; // Place catalog lookup and its typed commitment writer beside the ContinuousLearning owners.

using System.Text.Json; // Parse protected catalog JSON and project explicit typed commitment objects.
using System.Text.Json.Serialization; // Keep selector conversion and non-wire attachments under explicit serializer control.
using System.Text; // Build canonical strings and validate strict UTF-8 bytes.
using System.Globalization; // Parse startup budgets and format canonical integers independently of machine culture.
using AgentInfrastructure; // Return catalog lookup failures through the shared operation-result contract.

/// <summary>
/// Lookup-only selectors for a one-lesson objective. Neither value grants
/// authority unless the server-owned startup catalog resolves the exact pair.
/// </summary>
[JsonConverter(typeof(CodeDocumentationOneLessonSelectorV1JsonConverter))] // Keep exact selector decoding with the existing restricted converter.
public sealed record CodeDocumentationOneLessonSelectorV1( // Carry only request lookup keys; the request cannot supply grant binding values.
    string ObjectiveId, // Identify the requested allowlisted objective.
    string GrantSelector) // Select an existing startup-owned grant without creating one.
{ // Keep the request selector limited to its two lookup keys.
    public static bool IsValidSelectorShape( // Require the bounded objective and grant-selector tokens before lookup.
        CodeDocumentationOneLessonSelectorV1? selector) // Check both request lookup keys before protected allowlist resolution.
        => selector is not null && // Require an actual selector before examining either lookup key.
           ValidText(selector.ObjectiveId, 128) && // Reject blank or control-bearing identity text under the existing contract bounds.
           ValidText(selector.GrantSelector, 256); // Reject blank or control-bearing identity text under the existing contract bounds.

    private static bool ValidText(string? value, int maximumLength) // Reject blank or control-bearing identity text under the existing contract bounds.
        => !string.IsNullOrWhiteSpace(value) && // Reject absent or blank-only owner identity text.
           value.Length <= maximumLength && // Keep each selector key within its declared character ceiling.
           !value.Any(char.IsControl) && // Reject control characters in lookup keys.
           StringComparer.Ordinal.Equals(value, value.Trim()); // Reject leading or trailing whitespace instead of silently normalizing a selector.
} // Finish code documentation one lesson selector v1.

/// <summary>
/// Reads exactly one objective lookup ID and one grant lookup selector. Repeated
/// or unknown fields are rejected instead of being resolved by last-value wins.
/// </summary>
public sealed class CodeDocumentationOneLessonSelectorV1JsonConverter // Decode and encode exactly the existing two-field request selector wire shape.
    : JsonConverter<CodeDocumentationOneLessonSelectorV1> // Bind the selector wire type to its existing strict JSON converter.
{ // Parse and write only the closed request-selector object.
    public override CodeDocumentationOneLessonSelectorV1 Read( // Decode only the exact existing selector wire shape.
        ref Utf8JsonReader reader, // Advance the caller's token reader while decoding this selector.
        Type typeToConvert, // Accept the serializer type argument required by the converter API.
        JsonSerializerOptions options) // Accept the serializer options argument required by the converter API.
    { // Decode exactly two lookup keys and reject ambiguous selector JSON.
        if (reader.TokenType != JsonTokenType.StartObject) // Require the StartObject token at this position in the restricted selector object.
            throw new JsonException("The one-lesson selector must be an object."); // Reject a selector whose root is not a JSON object.

        string? objectiveId = null; // Keep the first decoded objective key until both required members are present.
        string? grantSelector = null; // Keep the first decoded grant selector until both required members are present.
        var hasObjectiveId = false; // Detect a repeated objectiveId before it can overwrite the first value.
        var hasGrantSelector = false; // Detect a repeated grantSelector before it can overwrite the first value.
        var ended = false; // Track whether the object actually reaches its closing token.

        while (reader.Read()) // Advance the restricted selector reader through its explicit shape.
        { // Decode exactly two lookup keys and reject ambiguous selector JSON.
            if (reader.TokenType == JsonTokenType.EndObject) // Require the EndObject token at this position in the restricted selector object.
            { // Stop selector decoding only at its explicit closing object token.
                ended = true; // Record that the selector reader reached its explicit closing object token.
                break; // Stop decoding at this selector's closing object token.
            } // Stop selector decoding only at its explicit closing object token.

            if (reader.TokenType != JsonTokenType.PropertyName) // Require the PropertyName token at this position in the restricted selector object.
                throw new JsonException("The one-lesson selector shape is invalid."); // Reject a token that cannot name one of the two selector members.

            var propertyName = reader.GetString(); // Read the member name that chooses one of the two allowed lookup keys.
            if (!reader.Read()) // Require a following value token for this member.
                throw new JsonException("The one-lesson selector is incomplete."); // Reject a selector member without a following value token.

            if (StringComparer.Ordinal.Equals(propertyName, "objectiveId")) // Recognize only the exact lower-camel objectiveId member.
            { // Decode the unique objectiveId member without overwriting its first value.
                if (hasObjectiveId) // Reject a second objectiveId member before replacing its first value.
                    throw new JsonException("The one-lesson selector repeats objectiveId."); // Reject a duplicate objectiveId rather than let it overwrite the first value.

                hasObjectiveId = true; // Record the first objectiveId member so a later duplicate is rejected.
                objectiveId = ReadString(ref reader, "objectiveId"); // Read the required selector identity without adding wire members.
            } // Decode the unique objectiveId member without overwriting its first value.
            else if (StringComparer.Ordinal.Equals(propertyName, "grantSelector")) // Recognize only the exact grantSelector member spelling.
            { // Decode the unique grantSelector member without overwriting its first value.
                if (hasGrantSelector) // Reject a second grantSelector member before replacing its first value.
                    throw new JsonException("The one-lesson selector repeats grantSelector."); // Reject a duplicate grantSelector rather than replace the first selector.

                hasGrantSelector = true; // Record the first grantSelector member so a later duplicate is rejected.
                grantSelector = ReadString(ref reader, "grantSelector"); // Read the required selector identity without adding wire members.
            } // Decode the unique grantSelector member without overwriting its first value.
            else // Reject every member outside the two selector keys.
            { // Reject a member outside the two allowed request lookup keys.
                throw new JsonException("The one-lesson selector contains an unsupported field."); // Reject a selector member outside objectiveId and grantSelector.
            } // Reject a member outside the two allowed request lookup keys.
        } // Decode exactly two lookup keys and reject ambiguous selector JSON.

        if (!ended) // Reject an incomplete selector whose object never closed.
            throw new JsonException("The one-lesson selector is incomplete."); // Reject a selector object that never reached its closing token.

        var selector = new CodeDocumentationOneLessonSelectorV1( // Construct the lookup-only selector after consuming the complete object.
            objectiveId ?? string.Empty, // Make an absent objective fail shape validation rather than invent a key.
            grantSelector ?? string.Empty); // Make an absent grant selector fail shape validation rather than invent a key.
        if (!hasObjectiveId || !hasGrantSelector || // Require both explicit selector members before validating their bounded text.
            !CodeDocumentationOneLessonSelectorV1.IsValidSelectorShape(selector)) // Require the bounded objective and grant-selector tokens before lookup.
        { // Reject an incomplete or invalid lookup key pair before returning a selector.
            throw new JsonException("The one-lesson selector fields are invalid."); // Reject missing or invalid objective/selector lookup text.
        } // Reject an incomplete or invalid lookup key pair before returning a selector.

        return selector; // Return the fully decoded and shape-checked request lookup keys.
    } // Decode exactly two lookup keys and reject ambiguous selector JSON.

    public override void Write( // Serialize the two selector fields in the existing fixed converter wire order.
        Utf8JsonWriter writer, // Write into the serializer's selector JSON writer.
        CodeDocumentationOneLessonSelectorV1 value, // Serialize only the shape-checked request lookup keys.
        JsonSerializerOptions options) // Accept the serializer options required by the converter API.
    { // Write only the validated V1 request lookup keys in their fixed wire order.
        if (!CodeDocumentationOneLessonSelectorV1.IsValidSelectorShape(value)) // Require the bounded objective and grant-selector tokens before lookup.
            throw new JsonException("The one-lesson selector fields are invalid."); // Reject an invalid selector before writing either lookup key.

        writer.WriteStartObject(); // Start the unchanged two-field selector object.
        writer.WriteString("objectiveId", value.ObjectiveId); // Write only the objective lookup key with its existing wire spelling.
        writer.WriteString("grantSelector", value.GrantSelector); // Write only the grant lookup key with its existing wire spelling.
        writer.WriteEndObject(); // Close the unchanged two-member selector object.
    } // Finish the unchanged V1 selector object after both lookup keys.

    private static string ReadString(ref Utf8JsonReader reader, string fieldName) // Read the required selector identity without adding wire members.
    { // Require a string value for the selected lookup key.
        if (reader.TokenType != JsonTokenType.String) // Require the String token at this position in the restricted selector object.
            throw new JsonException($"The one-lesson selector {fieldName} must be a string."); // Reject a non-string value for the selected lookup field.

        return reader.GetString() ?? // Return the selector string decoded by the required String token.
            throw new JsonException($"The one-lesson selector {fieldName} is required."); // Reject an absent decoded value for the required lookup field.
    } // Require a string value for the selected lookup key.
} // Finish code documentation one lesson selector v1 json converter.

/// <summary>
/// Immutable identity fields admitted by protected ModelServer startup
/// configuration. Source bytes and effect-stage receipts are added by their
/// later S04 owners; this record is only the C01 identity/preflight binding.
/// </summary>
public sealed record CodeDocumentationOneLessonGrantV1( // Carry the ten protected scalar grant bindings without request-owned authority.
    string GrantId, // Pin startup-owned grant identity in this complete immutable value.
    string GrantVersion, // Pin startup-owned grant version in this complete immutable value.
    string GrantSelector, // Pin allowlist lookup selector in this complete immutable value.
    string ObjectiveId, // Pin allowlisted objective identity in this complete immutable value.
    string TrainingRunId, // Pin typed training-attempt identity in this complete immutable value.
    string CandidateSha256, // Pin selected candidate digest in this complete immutable value.
    string ParentManifestSha256, // Pin selected parent manifest digest in this complete immutable value.
    string ParentWeightsSha256, // Pin selected parent weights digest in this complete immutable value.
    string ConfigurationSha256, // Pin protected configuration commitment in this complete immutable value.
    string ExamConditionSha256) // Pin protected exam-condition commitment in this complete immutable value.
{ // Attach trusted full selections without extending the serialized V1 grant.
    // In-memory discrimination only. The original V1 wire record is unchanged.
    [JsonIgnore] public int FormatVersion { get; init; } = 1; // Distinguish grant versions only in memory; keep V1 JSON unchanged.
    [JsonIgnore] public CodeDocumentationOneLessonDescriptorsV2? DescriptorsV2 { get; init; } // Retain compact V2 pins outside the legacy V1 JSON shape.
    [JsonIgnore] internal CodeDocumentationOneLessonSourceSelection? SourceSelection { get; init; } // Retain the complete copied source/pair selection without serializing it as V1.
    [JsonIgnore] internal CodeDocumentationOneLessonCheckpointSelection? CheckpointSelection { get; init; } // Retain the complete copied checkpoint selection without serializing it as V1.
    [JsonIgnore] internal CodeDocumentationOneLessonCorpusSelection? CorpusSelection { get; init; } // Retain the complete copied corpus/case selection without serializing it as V1.
    [JsonIgnore] internal CodeDocumentationOneLessonEvaluationPlanSelection? EvaluationPlanSelection { get; init; } // Retain the complete copied evaluation plan without serializing it as V1.
    [JsonIgnore] internal CodeDocumentationOneLessonEvaluatorSelection? EvaluatorSelection { get; init; } // Retain the complete copied evaluator without changing the legacy V1 wire shape.
    [JsonIgnore] internal CodeDocumentationOneLessonProfileSelection? ProfileSelection { get; init; } // Retain descriptive full routing inputs without granting effects or extending V1 serialization.
} // Finish code documentation one lesson grant v1.

/// <summary>
/// Compact startup commitments, not source bytes or a trusted baseline receipt.
/// Capacity validation cannot authorize effects; the full owner-selected values
/// must still be reopened and their commitments reproduced by the later owners.
/// </summary>
public sealed record CodeDocumentationOneLessonDescriptorsV2( // Carry the closed compact commitments for the complete V2 selections.
    int SchemaVersion, // Pin wire schema discriminator in this complete immutable value.
    string SourceSelectionSha256, // Pin full source/pair commitment in this complete immutable value.
    string ParentCheckpointSha256, // Pin full parent checkpoint commitment in this complete immutable value.
    string CandidateSelectionSha256, // Pin full candidate commitment in this complete immutable value.
    string CorpusDescriptorSha256, // Pin full corpus/case commitment in this complete immutable value.
    string EvaluationPlanSha256, // Pin full six-field plan commitment in this complete immutable value.
    int CaseCount, // Pin complete held-out case count in this complete immutable value.
    int RepeatCount, // Pin selected repeat count in this complete immutable value.
    string EvaluatorProfileSha256, // Pin evaluator-profile commitment in this complete immutable value.
    string ProfileSelectionSha256); // Pin profile-selection commitment in this complete immutable value.

/// <summary>
/// Startup-owned allowlist. The request can select an existing grant, but
/// cannot create an objective or supply any of its binding values.
/// </summary>
public sealed class CodeDocumentationOneLessonGrantCatalog // Own protected grant lookup and immutable attachment freezing.
{ // Own the protected selector allowlist and its frozen grant dependencies.
    public const string EnvironmentVariable = // Name the existing protected startup environment source for the allowlist.
        "XIIGEN_CODE_DOCUMENTATION_ONE_LESSON_GRANTS_JSON"; // Read grants from this existing key; requests cannot set a second catalog source.
    public const string GrantUnavailableErrorCode = "GRANT_UNAVAILABLE"; // Report absent or ambiguous grants with the existing unavailable code.
    public const string InvalidCatalogErrorCode = "GRANT_CATALOG_INVALID"; // Retain the existing invalid-catalog code so lookup fails closed.

    private static readonly JsonSerializerOptions JsonOptions = new() // Keep the legacy catalog deserializer exact-case and bounded.
    { // Configure only the existing legacy catalog parse policy.
        PropertyNameCaseInsensitive = false, // Require the exact case of the existing catalog wire member names.
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow, // Reject unrecognized catalog members instead of discarding them during decoding.
        MaxDepth = 8, // Bound catalog object nesting to the existing depth of eight.
    }; // Finish the existing case-sensitive, unknown-member-rejecting catalog policy.

    private readonly IReadOnlyDictionary<string, CodeDocumentationOneLessonGrantV1> // Index immutable admitted grants by exact selector spelling.
        _grantsBySelector; // Retain the selector index used by Resolve and replay revalidation.
    private readonly string? _configurationErrorCode; // Retain startup failure so a bad catalog cannot resolve a grant.
    internal int MaxTeacherCalls { get; } // Expose the existing bounded protected-startup teacher budget retained by this catalog.
    internal bool IsTeachingDisclosureV3 { get; private set; } // Mark only a fully validated V3 catalog for the protected disclosure branch.
    private IReadOnlyDictionary<(string, string), CodeDocumentationOneLessonTeachingDisclosureApproval> _teachingApprovals = // Retain immutable approval membership keyed by exact grant id and version.
        new Dictionary<(string, string), CodeDocumentationOneLessonTeachingDisclosureApproval>(); // Start with no approval rights until the V3 decoder validates the whole catalog.
    internal CodeDocumentationOneLessonTeachingDisclosureApproval? FindTeachingApproval(CodeDocumentationOneLessonGrantV1 grant) // Look up disclosure approval only for the exact grant object retained by this catalog.
        => _grantsBySelector.TryGetValue(grant.GrantSelector, out var retained) && ReferenceEquals(retained, grant) && // Reject a foreign grant even when its selector and scalar values are identical.
           _teachingApprovals.TryGetValue((grant.GrantId, grant.GrantVersion), out var approval) ? approval : null; // Return only the retained approval at that grant's exact identity/version key.
    internal CodeDocumentationOneLessonGrantV1? ResolveApprovalOwner(CodeDocumentationOneLessonTeachingDisclosureApproval approval) // Resolve the owner of an approval only from this catalog's retained objects.
        => _teachingApprovals.TryGetValue((approval.GrantId, approval.GrantVersion), out var retained) && ReferenceEquals(retained, approval) // Require reference identity of the retained approval rather than equivalent caller JSON.
            ? _grantsBySelector.Values.SingleOrDefault(g => g.GrantId == approval.GrantId && g.GrantVersion == approval.GrantVersion) : null; // Return its uniquely retained grant or refuse membership without manufacturing an owner.

    private CodeDocumentationOneLessonGrantCatalog( // Retain the immutable allowlist and its validation outcome.
        IReadOnlyDictionary<string, CodeDocumentationOneLessonGrantV1> grants, // Retain the completely validated selector map for subsequent lookup.
        string? configurationErrorCode, // Retain an invalid-catalog result instead of a partly usable allowlist.
        int maxTeacherCalls = 0) // Receive the protected-startup teacher budget; a failure catalog retains the default zero.
    { // Retain the validated allowlist, load error and startup-owned budget.
        _grantsBySelector = grants; // Retain the already frozen selector map.
        _configurationErrorCode = configurationErrorCode; // Retain validation failure rather than accepting a partly parsed catalog.
        MaxTeacherCalls = maxTeacherCalls; // Retain the already-admitted startup teacher budget without enlarging it from grant descriptors.
    } // Finish code documentation one lesson grant catalog.

    internal static CodeDocumentationOneLessonGrantCatalog Load( // Read the existing bounded environment catalog without creating request-owned grants.
        Func<string, string?> readEnvironment) // Read only the existing protected startup environment source.
    { // Admit only complete grants from the protected existing startup catalog.
        ArgumentNullException.ThrowIfNull(readEnvironment); // Reject a missing protected environment reader before loading catalog configuration.
        var json = readEnvironment(EnvironmentVariable); // Read the complete protected grant catalog from the existing environment key.
        if (string.IsNullOrWhiteSpace(json)) // Treat an absent catalog as an empty allowlist.
        { // Keep all grants unavailable when no protected catalog was configured.
            return new( // Create an empty valid catalog when protected configuration is absent.
                new Dictionary<string, CodeDocumentationOneLessonGrantV1>( // Keep no grant available in the empty-catalog case.
                    StringComparer.Ordinal), // Preserve exact selector spelling in the empty lookup map.
                configurationErrorCode: null); // An absent catalog is empty, without an invalid-configuration failure.
        } // Keep all grants unavailable when no protected catalog was configured.

        try // Bound nonfatal failures around load.
        { // Admit only complete grants from the protected existing startup catalog while retaining the existing exception boundary.
            using var raw = JsonDocument.Parse(json, new JsonDocumentOptions { MaxDepth = 9 }); // The complete V3 eligibility evidence path has nine containers.
            if (raw.RootElement.ValueKind != JsonValueKind.Object) // Reject a non-object catalog envelope before reading its version.
                return InvalidCatalog(); // Retain a failed immutable catalog so lookup cannot admit any grant.
            if (raw.RootElement.TryGetProperty("SchemaVersion", out var v3) && v3.ValueKind == JsonValueKind.Number && v3.TryGetInt32(out var schema3) && schema3 == 3) // Dispatch only explicit integer schema3 through the V3 structural-depth allowance.
                return LoadV3(raw.RootElement, json, readEnvironment); // Load the closed V3 envelope while preserving the existing protected budget reader.
            using var legacyDepth = JsonDocument.Parse(json, new JsonDocumentOptions { MaxDepth = 8 }); // V1/V2 must pass their unchanged depth bound before any legacy admission.
            if (raw.RootElement.TryGetProperty("SchemaVersion", out var version) && // Inspect the existing SchemaVersion member to choose the disjoint V2 parser.
                version.ValueKind == JsonValueKind.Number && version.TryGetInt32(out var number) && number == 2) // Dispatch to V2 only for an explicit integer version of two.
            { // Dispatch explicit version two to its disjoint strict catalog parser.
                return LoadV2(raw.RootElement, json, readEnvironment); // Decode disjoint compact/full startup inputs into the same compact V2 grant representation.
            } // Dispatch explicit version two to its disjoint strict catalog parser.
            // V1 cannot silently discard the new in-memory discriminator fields.
            if (raw.RootElement.TryGetProperty("Grants", out var oldGrants) && // Inspect legacy Grants before deserialization can ignore in-memory fields.
                oldGrants.ValueKind == JsonValueKind.Array && // Inspect only actual arrays in the legacy Grants member.
                oldGrants.EnumerateArray().Any(g => g.ValueKind == JsonValueKind.Object && // Check each object-shaped legacy grant for forbidden version-only members.
                    g.EnumerateObject().Any(p => p.Name is "DescriptorsV2" or "FormatVersion"))) // Reject V2 discriminator/descriptor members smuggled into a V1 grant.
                return InvalidCatalog(); // Retain a failed immutable catalog so lookup cannot admit any grant.
            var document = JsonSerializer.Deserialize<GrantCatalogDocument>( // Decode the existing catalog shape under its configured serializer contract.
                json, // Deserialize the same protected JSON text already examined above.
                JsonOptions); // Use the unchanged exact-case and unknown-member legacy policy.
            if (document is null || document.SchemaVersion != 1 || // Require the legacy schema version one after V2 dispatch is excluded.
                document.Grants is null || document.Grants.Length == 0) // Require at least one configured grant in a nonempty legacy document.
            { // Reject a non-V1 legacy document or an empty configured grant set.
                return InvalidCatalog(); // Retain a failed immutable catalog so lookup cannot admit any grant.
            } // Reject a non-V1 legacy document or an empty configured grant set.

            var grants = new Dictionary<string, CodeDocumentationOneLessonGrantV1>( // Collect only completely valid, uniquely selected legacy grants.
                StringComparer.Ordinal); // Preserve case-sensitive selector identity in the configured allowlist.
            foreach (var grant in document.Grants) // Validate every configured legacy grant and its unique selector.
            { // Admit each legacy grant only once under its exact selector.
                if (!ValidGrant(grant) || // Require all existing grant strings and hash pins before version-specific admission.
                    !grants.TryAdd(grant.GrantSelector, grant)) // Reject duplicate allowlist selectors rather than replace an existing grant.
                { // Reject invalid grant fields or a duplicate protected selector.
                    return InvalidCatalog(); // Retain a failed immutable catalog so lookup cannot admit any grant.
                } // Reject invalid grant fields or a duplicate protected selector.
            } // Admit each legacy grant only once under its exact selector.

            return new(grants, configurationErrorCode: null); // Return all validated legacy grants with no startup failure.
        } // Admit only complete grants from the protected existing startup catalog while retaining the existing exception boundary.
        catch (JsonException) // Reject malformed protected catalog JSON without retaining partial grants.
        { // Return the existing Load failure result after the permitted nonfatal exception.
            return InvalidCatalog(); // Retain a failed immutable catalog so lookup cannot admit any grant.
        } // Return the existing Load failure result after the permitted nonfatal exception.
        catch (ArgumentException exception) when (exception.InnerException is EncoderFallbackException) // JsonDocument wraps malformed UTF-16 transcoding in this specific argument failure.
        { // Fail the complete malformed catalog before any partial grant can escape.
            return InvalidCatalog(); // Preserve ordinary invalid-configuration handling for the wrapped Unicode failure.
        } // Valid JSON, version dispatch and legacy member grammar are unaffected by this narrow exception guard.
        catch (Exception exception) when (exception is InvalidOperationException or EncoderFallbackException) // Reject the permitted decoding/Unicode failures without retaining partial grants.
        { // Return the existing Load failure result after the permitted nonfatal exception.
            return InvalidCatalog(); // Retain a failed immutable catalog so lookup cannot admit any grant.
        } // Return the existing Load failure result after the permitted nonfatal exception.
    } // Admit only complete grants from the protected existing startup catalog.

    internal static CodeDocumentationOneLessonGrantCatalog FromTrustedGrants( // Freeze source, checkpoint, corpus, plan and full evaluator in dependency order before allowlist admission.
        IEnumerable<CodeDocumentationOneLessonGrantV1> grants, // Freeze every startup-owned grant from this complete configured sequence.
        int maxTeacherCalls = 0) // Use the protected startup budget to validate both complete evaluation series.
    { // Revalidate and recopy all complete dependencies before freezing the full evaluator.
        ArgumentNullException.ThrowIfNull(grants); // Reject a missing trusted catalog input before freezing its members.
        var map = new Dictionary<string, CodeDocumentationOneLessonGrantV1>( // Build the frozen catalog only after each attachment is revalidated.
            StringComparer.Ordinal); // Prevent case aliases from collapsing distinct protected selectors.
        foreach (var grant in grants) // Visit the entire trusted grant sequence without implicit truncation.
        { // Freeze every trusted grant before admitting the selector map.
            if (!ValidGrant(grant) || !ValidVersion(grant)) // Require all existing grant strings and hash pins before version-specific admission.
                return InvalidCatalog(); // Retain a failed immutable catalog so lookup cannot admit any grant.
            var frozenGrant = grant; // Start from this grant and preserve every earlier dependency while copying later attachments.
            if (grant.SourceSelection is not null) // Revalidate a present complete source/pair attachment.
            { // Rebuild the complete source/pair attachment and compare its compact commitment.
                var frozen = CodeDocumentationOneLessonSourceSelection.Create( // Retain the full recopied selection before comparing its hash and dependent pins.
                    grant.SourceSelection.Sources, grant.SourceSelection.PairSelections, // Re-copy the full passive curriculum before existing whole SOURCE hash equality.
                    grant.SourceSelection.TeachingCurriculum); // Copy the complete passive curriculum through the same source owner.
                if (!frozen.IsSuccess || frozen.Value is null || // Require success of the newly validated immutable attachment before using its value.
                    !StringComparer.Ordinal.Equals(frozen.Value.SourceSelectionSha256, // Compare the rederived full source/pair hash with its compact grant pin.
                        grant.DescriptorsV2!.SourceSelectionSha256)) // Use the admitted compact source commitment as the equality target.
                    return InvalidCatalog(); // Retain a failed immutable catalog so lookup cannot admit any grant.
                frozenGrant = grant with { SourceSelection = frozen.Value }; // Begin the copied grant with the revalidated complete source attachment.
            } // Rebuild the complete source/pair attachment and compare its compact commitment.
            if (grant.CheckpointSelection is not null) // Revalidate a present complete parent/candidate attachment.
            { // Rebuild parent and candidate while retaining the frozen source attachment.
                var frozen = CodeDocumentationOneLessonCheckpointSelection.Create( // Retain the full recopied selection before comparing its hash and dependent pins.
                    grant.CheckpointSelection.Parent, grant.CheckpointSelection.Candidate); // Copy both parent and candidate through the checkpoint-selection owner.
                if (!frozen.IsSuccess || frozen.Value is null || // Require success of the newly validated immutable attachment before using its value.
                    frozen.Value.ParentCheckpointSha256 != grant.DescriptorsV2!.ParentCheckpointSha256 || // Bind the rederived full parent commitment to its compact grant pin.
                    frozen.Value.CandidateSelectionSha256 != grant.DescriptorsV2.CandidateSelectionSha256 || // Bind the rederived full candidate commitment to its compact grant pin.
                    frozen.Value.Candidate.CandidateSha256 != grant.CandidateSha256 || // Require the copied candidate digest to equal the scalar grant binding.
                    frozen.Value.Parent.ManifestSha256 != grant.ParentManifestSha256 || // Require the copied parent manifest digest to equal the scalar grant binding.
                    frozen.Value.Parent.WeightsSha256 != grant.ParentWeightsSha256) // Require the copied parent weights digest to equal the scalar grant binding.
                    return InvalidCatalog(); // Retain a failed immutable catalog so lookup cannot admit any grant.
                frozenGrant = frozenGrant with { CheckpointSelection = frozen.Value }; // Retain the frozen source while attaching the revalidated parent and candidate selection.
            } // Rebuild parent and candidate while retaining the frozen source attachment.
            if (grant.CorpusSelection is not null) // Revalidate a present complete corpus/case attachment.
            { // Rebuild the whole corpus and ordered cases while retaining earlier dependencies.
                var frozen = CodeDocumentationOneLessonCorpusSelection.Create( // Retain the full recopied selection before comparing its hash and dependent pins.
                    grant.CorpusSelection.Corpus, grant.CorpusSelection.CaseIds); // Copy all corpus descriptor pins and ordered cases through their existing owner.
                if (!frozen.IsSuccess || frozen.Value is null || // Require success of the newly validated immutable attachment before using its value.
                    frozen.Value.CorpusDescriptorSha256 != grant.DescriptorsV2!.CorpusDescriptorSha256 || // Require the copied full corpus commitment to equal the compact grant pin.
                    frozen.Value.CaseIds.Count != grant.DescriptorsV2.CaseCount) // Require all copied held-out cases to equal the compact case count.
                    return InvalidCatalog(); // Retain a failed immutable catalog so lookup cannot admit any grant.
                frozenGrant = frozenGrant with { CorpusSelection = frozen.Value }; // Retain source and checkpoint attachments while attaching the revalidated complete corpus.
            } // Rebuild the whole corpus and ordered cases while retaining earlier dependencies.
            if (grant.EvaluationPlanSelection is not null) // Freeze the optional full plan only when this V2 grant actually carries it.
            { // Rebuild the full plan from the frozen corpus and compare all compact pins.
                if (frozenGrant.CorpusSelection is null) // A full plan cannot be admitted without its freshly frozen complete corpus dependency.
                    return InvalidCatalog(); // Retain a failed immutable catalog so lookup cannot admit any grant.
                var selected = grant.EvaluationPlanSelection; // Read the supplied complete plan attachment after validating the grant version.
                var frozen = CodeDocumentationOneLessonEvaluationPlanSelection.Create( // Rebuild the complete plan from the already-frozen corpus rather than retaining a caller object.
                    frozenGrant.CorpusSelection, selected.PlanId, selected.EvaluatorId, // Bind the frozen corpus with the same selected descriptive plan and evaluator keys.
                    selected.RepeatCount, selected.ExpectedEvaluationCallCount); // Preserve the selected R and exact expected-call pin during reconstruction.
                if (!frozen.IsSuccess || frozen.Value is null || // Require success of the newly validated immutable attachment before using its value.
                    frozen.Value.PlanSha256 != selected.PlanSha256 || // Require the rebuilt full plan hash to equal the original complete selected commitment.
                    frozen.Value.PlanSha256 != grant.DescriptorsV2!.EvaluationPlanSha256 || // Require that same full hash to equal the compact startup descriptor pin.
                    frozen.Value.CaseIds.Count != grant.DescriptorsV2.CaseCount || // Compare the whole corpus case count with compact N; do not truncate the sequence.
                    frozen.Value.RepeatCount != grant.DescriptorsV2.RepeatCount) // Require compact R to equal the unchanged full selected repeat count.
                    return InvalidCatalog(); // Retain a failed immutable catalog so lookup cannot admit any grant.
                frozenGrant = frozenGrant with { EvaluationPlanSelection = frozen.Value }; // Attach the rebuilt plan while retaining all previously frozen source, checkpoint and corpus selections.
            } // Rebuild the full plan from the frozen corpus and compare all compact pins.
            if (grant.EvaluatorSelection is not null) // Freeze the optional full evaluator only after its complete corpus and plan dependencies.
            { // Rebuild the full evaluator and bind its full values to every protected compact/scalar pin.
                if (frozenGrant.CorpusSelection is null || frozenGrant.EvaluationPlanSelection is null) // Require both freshly frozen complete dependencies rather than retain caller-owned selections.
                    return InvalidCatalog(); // Reject a full evaluator with missing corpus or full plan.
                var selected = grant.EvaluatorSelection; // Retain the supplied immutable full evaluator as the exact preimage comparison target.
                var frozen = CodeDocumentationOneLessonEvaluatorSelection.Create( // Revalidate and recopy all full evaluator values through their actual owners.
                    frozenGrant.CorpusSelection, frozenGrant.EvaluationPlanSelection, selected.Evaluator); // Bind the full evaluator to the same freshly frozen corpus and plan.
                if (!frozen.IsSuccess || frozen.Value is null || // Require complete typed evaluator admission before comparing protected startup pins.
                    frozen.Value.EvaluatorProfileSha256 != selected.EvaluatorProfileSha256 || // Require reconstructed full values to reproduce the supplied self hash.
                    !CodeDocumentationOneLessonV2Wire.CanonicalEvaluatorProfileBytes(frozen.Value).AsSpan().SequenceEqual( // Compare the full evaluator preimage, including exact float strings, in addition to hashes.
                        CodeDocumentationOneLessonV2Wire.CanonicalEvaluatorProfileBytes(selected)) || // Use the supplied immutable full snapshot rather than a caller digest alone.
                    frozen.Value.EvaluatorProfileSha256 != grant.DescriptorsV2!.EvaluatorProfileSha256 || // Bind the complete full evaluator to the compact startup commitment.
                    frozenGrant.EvaluationPlanSelection.PlanSha256 != grant.DescriptorsV2.EvaluationPlanSha256 || // Preserve the full plan/compact digest dependency at evaluator attachment admission.
                    frozenGrant.EvaluationPlanSelection.CaseIds.Count != grant.DescriptorsV2.CaseCount || // Bind the complete selected held-out count to compact N.
                    frozenGrant.EvaluationPlanSelection.RepeatCount != grant.DescriptorsV2.RepeatCount || // Bind the full repeat count to compact R without lowering it.
                    frozen.Value.Evaluator.EvaluatorId != frozenGrant.EvaluationPlanSelection.EvaluatorId || // Require the full evaluator's descriptive identity to match the full plan.
                    frozen.Value.Evaluator.ConfigurationSha256 != grant.ConfigurationSha256 || // Compare the protected generic configuration pin without inventing a recomputation owner.
                    frozen.Value.Evaluator.ExamConditionSha256 != grant.ExamConditionSha256 || // Compare the protected generic exam pin under the same explicit authority limit.
                    frozen.Value.Evaluator.CurrentConfiguration.MaxTeacherCalls != maxTeacherCalls) // Require the selected public budget to equal the protected catalog argument.
                    return InvalidCatalog(); // Reject any full-value, dependency, scalar or protected budget disagreement.
                frozenGrant = frozenGrant with { EvaluatorSelection = frozen.Value }; // Attach the rebuilt evaluator while preserving ALL four earlier frozen selections.
            } // Complete the final dependent full evaluator freeze before selector-map admission.
            if (grant.ProfileSelection is not null) // A complete profile must retain every preceding full dependency.
            { // Preserve all earlier frozen attachments when admitting the complete profile.
                if (frozenGrant.SourceSelection is null || frozenGrant.CheckpointSelection is null || frozenGrant.CorpusSelection is null || // Preserve all earlier frozen attachments when admitting the complete profile.
                    frozenGrant.EvaluationPlanSelection is null || frozenGrant.EvaluatorSelection is null) return InvalidCatalog(); // Preserve all earlier frozen attachments when admitting the complete profile.
                var selected = grant.ProfileSelection; // Preserve all earlier frozen attachments when admitting the complete profile.
                var frozen = CodeDocumentationOneLessonProfileSelection.Create(selected.Snapshot); // Recopy all nested arrays/maps through the pure profile factory.
                if (!frozen.IsSuccess || frozen.Value is null || frozen.Value.ProfileSelectionSha256 != selected.ProfileSelectionSha256 || // Preserve all earlier frozen attachments when admitting the complete profile.
                    frozen.Value.ProfileSelectionSha256 != grant.DescriptorsV2!.ProfileSelectionSha256 || // Preserve all earlier frozen attachments when admitting the complete profile.
                    !CodeDocumentationOneLessonV2Wire.CanonicalProfileSelectionBytes(frozen.Value).AsSpan().SequenceEqual( // Preserve all earlier frozen attachments when admitting the complete profile.
                        CodeDocumentationOneLessonV2Wire.CanonicalProfileSelectionBytes(selected))) return InvalidCatalog(); // Preserve all earlier frozen attachments when admitting the complete profile.
                frozenGrant = frozenGrant with { ProfileSelection = frozen.Value }; // Keep all five already-frozen selections in the SAME catalog grant.
            } // Preserve all earlier frozen attachments when admitting the complete profile.
            if (!map.TryAdd(frozenGrant.GrantSelector, frozenGrant)) // Reject duplicate allowlist selectors rather than replace an existing grant.
                return InvalidCatalog(); // Retain a failed immutable catalog so lookup cannot admit any grant.
        } // Freeze every trusted grant before admitting the selector map.

        return map.Count == 0 // Reject an empty trusted allowlist before exposing lookup.
            ? InvalidCatalog() // Retain a failed immutable catalog so lookup cannot admit any grant.
            : map.Values.Select(g => g.FormatVersion).Distinct().Count() != 1 || // Reject a trusted allowlist containing more than one grant format.
              (map.Values.First().FormatVersion == 2 && // Read the already-validated common grant format from the nonempty allowlist.
               map.Values.Select(g => g.ObjectiveId).Distinct(StringComparer.Ordinal).Count() != map.Count) // Require every V2 objective to resolve to exactly one allowlisted grant.
                ? InvalidCatalog() // Retain a failed immutable catalog so lookup cannot admit any grant.
                : new(map, configurationErrorCode: null, maxTeacherCalls); // Retain the actual startup call budget alongside the fully frozen grant map.
    } // Finish dependency-ordered freezing of every present complete selection.

    public OperationResult<CodeDocumentationOneLessonGrantV1> Resolve( // Resolve only the protected grant matching both request lookup keys.
        CodeDocumentationOneLessonSelectorV1? selector) // Accept request lookup keys without accepting request-owned binding fields.
    { // Resolve the exact request key pair against the protected allowlist.
        if (_configurationErrorCode is not null) // Fail lookup when startup retained an invalid-catalog result.
        { // Keep lookup closed after a protected catalog validation failure.
            return OperationResult<CodeDocumentationOneLessonGrantV1>.Fail( // Return an ordinary failed result without a usable admitted value.
                "The protected one-lesson grant catalog is invalid.", // Explain the protected-catalog failure without exposing grant content.
                _configurationErrorCode); // Preserve the startup failure code in the lookup result.
        } // Keep lookup closed after a protected catalog validation failure.

        if (selector is null || // Reject an absent selector before allowlist lookup.
            !CodeDocumentationOneLessonSelectorV1.IsValidSelectorShape(selector) || // Require the bounded objective and grant-selector tokens before lookup.
            !_grantsBySelector.TryGetValue( // Resolve the request selector only against this catalog's protected allowlist.
                selector.GrantSelector, // Lookup the exact requested selector in the protected catalog.
                out var grant) || // Use only the stored grant returned by that lookup.
            !StringComparer.Ordinal.Equals( // Require the selected grant's objective to equal the request objective.
                selector.ObjectiveId, // Compare the request's objective lookup key.
                grant.ObjectiveId)) // Use the startup-owned objective as the equality target.
        { // Reject keys that do not resolve one stored grant and its exact objective.
            return OperationResult<CodeDocumentationOneLessonGrantV1>.Fail( // Return an ordinary failed result without a usable admitted value.
                "No startup-owned one-lesson grant matches the supplied selector.", // Report that the request keys have no matching protected grant.
                GrantUnavailableErrorCode); // Return the existing unavailable-grant failure code.
        } // Reject keys that do not resolve one stored grant and its exact objective.

        return OperationResult<CodeDocumentationOneLessonGrantV1>.Ok(grant); // Return only the existing allowlisted grant after both request keys match.
    } // Resolve the exact request key pair against the protected allowlist.

    public OperationResult<CodeDocumentationOneLessonGrantV1> Revalidate( // Require the same stored grant identity and version on replay.
        string? grantId, // Require the same protected grant ID recorded by replay.
        string? grantVersion) // Require the same protected grant version recorded by replay.
    { // Require exactly one protected grant with this replay identity and version.
        if (_configurationErrorCode is not null) // Fail replay revalidation when startup retained an invalid catalog.
        { // Keep replay revalidation closed after a catalog validation failure.
            return OperationResult<CodeDocumentationOneLessonGrantV1>.Fail( // Return an ordinary failed result without a usable admitted value.
                "The protected one-lesson grant catalog is invalid.", // Explain the invalid protected catalog without accepting replay identity.
                _configurationErrorCode); // Preserve the original startup failure code on replay.
        } // Keep replay revalidation closed after a catalog validation failure.

        var matches = _grantsBySelector.Values // Find grants with exactly the supplied replay identity and version.
            .Where(grant => // Evaluate the identity/version predicate on each stored grant.
                StringComparer.Ordinal.Equals(grant.GrantId, grantId) && // Require byte-exact grant identity on replay.
                StringComparer.Ordinal.Equals(grant.GrantVersion, grantVersion)) // Require byte-exact grant version on replay.
            .Take(2) // Observe at most two matches to distinguish absence, uniqueness and ambiguity.
            .ToArray(); // Materialize those bounded matches before deciding replay admission.
        return matches.Length == 1 // Admit replay only when exactly one stored grant matches.
            ? OperationResult<CodeDocumentationOneLessonGrantV1>.Ok(matches[0]) // Return the unique stored grant without creating replacement authority.
            : OperationResult<CodeDocumentationOneLessonGrantV1>.Fail( // Return an ordinary failed result without a usable admitted value.
                "The startup-owned one-lesson grant is absent or ambiguous.", // Report both absence and ambiguity as unavailable replay identity.
                GrantUnavailableErrorCode); // Return the stable unavailable-grant code for absent or ambiguous replay identity.
    } // Require exactly one protected grant with this replay identity and version.

    private static bool ValidGrant(CodeDocumentationOneLessonGrantV1? grant) // Require all existing grant strings and hash pins before version-specific admission.
        => grant is not null && // Require an actual grant before validating its protected binding fields.
           ValidText(grant.GrantId, 128) && // Reject blank or control-bearing identity text under the existing contract bounds.
           ValidText(grant.GrantVersion, 64) && // Reject blank or control-bearing identity text under the existing contract bounds.
           ValidText(grant.GrantSelector, 256) && // Reject blank or control-bearing identity text under the existing contract bounds.
           ValidText(grant.ObjectiveId, 128) && // Reject blank or control-bearing identity text under the existing contract bounds.
           ValidText(grant.TrainingRunId, 128) && // Reject blank or control-bearing identity text under the existing contract bounds.
           Sha256Hex(grant.CandidateSha256) && // Require a bare lowercase SHA-256 commitment in the existing catalog grammar.
           Sha256Hex(grant.ParentManifestSha256) && // Require a bare lowercase SHA-256 commitment in the existing catalog grammar.
           Sha256Hex(grant.ParentWeightsSha256) && // Require a bare lowercase SHA-256 commitment in the existing catalog grammar.
           Sha256Hex(grant.ConfigurationSha256) && // Require a bare lowercase SHA-256 commitment in the existing catalog grammar.
           Sha256Hex(grant.ExamConditionSha256); // Require a bare lowercase SHA-256 commitment in the existing catalog grammar.

    private static bool ValidVersion(CodeDocumentationOneLessonGrantV1 grant) // Reject V1 attachments and require the existing bounded V2 descriptor contract.
        => grant.FormatVersion == 1 ? grant.DescriptorsV2 is null && grant.SourceSelection is null && // Keep compact V2 and source attachments out of V1 compatibility admission.
                                      grant.CheckpointSelection is null && grant.CorpusSelection is null && // Keep checkpoint and corpus attachments out of V1 compatibility admission.
                                      grant.EvaluationPlanSelection is null && grant.EvaluatorSelection is null && grant.ProfileSelection is null : // Keep all full selections outside legacy V1 compatibility admission.
           grant.FormatVersion == 2 && CodeDocumentationOneLessonV2Wire.ValidDescriptors(grant.DescriptorsV2) && // Require V2 discrimination and the exact compact schema and digest identities.
           RepositoryOwnedTrainingArtifactLayout.IsValidModelTrainingAttemptRunId(grant.TrainingRunId); // Require the existing typed training-attempt run identity.

    private static CodeDocumentationOneLessonGrantCatalog LoadV2( // Parse the existing exact V2 catalog envelope and disjoint compact/full startup descriptor inputs.
        JsonElement root, string rawJson, Func<string, string?> readEnvironment) // Read the exact protected V2 envelope and its existing budget source.
    { // Validate the closed V2 grant shape and retain the actual protected call budget.
        if (!CodeDocumentationOneLessonV2Wire.ValidUtf8Text(rawJson, 1_048_576) || // Require bounded UTF-8 identity text for the existing V2 wire contract.
            rawJson.StartsWith('\uFEFF') || // Reject a leading JSON BOM before admitting the exact V2 wire representation.
            !CodeDocumentationOneLessonV2Wire.ExactFields(root, "SchemaVersion", "Grants") || // Reject extra, duplicate or missing members in the restricted wire object.
            root.GetProperty("SchemaVersion").GetRawText() != "2" || // Require the schema token's exact integer spelling, without exponent aliases.
            root.GetProperty("Grants").ValueKind != JsonValueKind.Array || // Require the exact Array JSON primitive or container before reading this owner member.
            root.GetProperty("Grants").GetArrayLength() is < 1 or > 32) // Require the complete grant array to contain between one and thirty-two grants.
            return InvalidCatalog(); // Retain a failed immutable catalog so lookup cannot admit any grant.

        var map = new Dictionary<string, CodeDocumentationOneLessonGrantV1>(StringComparer.Ordinal); // Index each parsed protected grant by its exact ordinal selector key.
        var objectives = new HashSet<string>(StringComparer.Ordinal); // Track objectives so two V2 grants cannot ambiguously resolve one objective.
        foreach (var item in root.GetProperty("Grants").EnumerateArray()) // Validate every grant in the bounded array without dropping an invalid member.
        { // Validate every closed-shape V2 grant before adding its unique selector/objective.
            if (!CodeDocumentationOneLessonV2Wire.ExactFields(item, "GrantId", "GrantVersion", "GrantSelector", // Reject extra, duplicate or missing members in the restricted wire object.
                    "ObjectiveId", "TrainingRunId", "CandidateSha256", "ParentManifestSha256", "ParentWeightsSha256", // Require the existing objective, run, candidate and parent binding members.
                    "ConfigurationSha256", "ExamConditionSha256", "DescriptorsV2") || // Require protected exam/configuration pins and the member containing exact compact or full startup input.
                item.EnumerateObject().Any(p => p.Name != "DescriptorsV2" && p.Value.ValueKind != JsonValueKind.String)) // Require strings for all ten scalar binding members; only DescriptorsV2 is an object.
                return InvalidCatalog(); // Retain a failed immutable catalog so lookup cannot admit any grant.
            var descriptor = item.GetProperty("DescriptorsV2"); // Retain the protected raw descriptor until its complete compact or full field set is validated.
            var grant = new CodeDocumentationOneLessonGrantV1( // Construct a complete V2 protected grant from the already checked exact fields.
                item.GetProperty("GrantId").GetString()!, item.GetProperty("GrantVersion").GetString()!, // Copy the validated protected grant identity and version.
                item.GetProperty("GrantSelector").GetString()!, item.GetProperty("ObjectiveId").GetString()!, // Copy the validated protected selector and objective keys.
                item.GetProperty("TrainingRunId").GetString()!, item.GetProperty("CandidateSha256").GetString()!, // Copy the protected run identity and selected candidate digest.
                item.GetProperty("ParentManifestSha256").GetString()!, item.GetProperty("ParentWeightsSha256").GetString()!, // Copy both protected parent checkpoint digest pins.
                item.GetProperty("ConfigurationSha256").GetString()!, item.GetProperty("ExamConditionSha256").GetString()!) // Copy the protected configuration and exam-condition digest pins.
                { FormatVersion = 2 }; // Preserve the protected ten scalars while the disjoint descriptor branch derives its in-memory state.
            var parsed = CodeDocumentationOneLessonV2Wire.ReadDescriptors(descriptor); // Keep the existing exact ten-field compact parser and scalar capacity semantics.
            if (parsed is not null) // Admit compact input only through its unchanged restricted canonical seam.
            { // Compact-only startup values retain no fabricated full attachments.
                if (!CodeDocumentationOneLessonV2Wire.IsCanonical(descriptor)) return InvalidCatalog(); // Require exact existing compact descriptor bytes before retaining their pins.
                grant = grant with { DescriptorsV2 = parsed }; // Attach only the unchanged compact typed value for compatibility input.
            } // Full arrays and signed seeds never reach the default raw compact writer.
            else // A noncompact shape must satisfy the complete exact full startup field set or fail closed.
            { // Decode all five complete selections within the same pure catalog ownership boundary.
                var full = CodeDocumentationOneLessonStartupSelectionDecoder.Decode(descriptor, grant); // Reconstruct all full commitments and compare typed canonical bytes without a new owner or environment key.
                if (full is null) return InvalidCatalog(); // Reject hybrids, invalid descendants or incomplete full values without exposing a partial catalog.
                grant = full; // Retain the complete pure decoded grant for the SAME final dependency freeze.
            } // Both disjoint inputs produce FormatVersion2 and the same compact public descriptor representation.
            if (!ValidGrant(grant) || !ValidVersion(grant) || // Require all existing grant strings and hash pins before version-specific admission.
                !map.TryAdd(grant.GrantSelector, grant) || !objectives.Add(grant.ObjectiveId)) // Reject duplicate selectors or objectives instead of replacing an admitted grant.
                return InvalidCatalog(); // Retain a failed immutable catalog so lookup cannot admit any grant.
        } // Validate every closed-shape V2 grant before adding its unique selector/objective.
        // Use the same protected startup value and hard cap as the teacher owner.
        // No descriptor or request can supply or enlarge this budget.
        var configured = readEnvironment("XIIGEN_CONTINUOUS_LEARNING_MAX_TEACHER_CALLS"); // Read the existing server teacher-call budget; descriptors cannot enlarge it.
        if (!int.TryParse(configured, NumberStyles.None, CultureInfo.InvariantCulture, out var calls) || // Require invariant unsigned decimal text for the protected budget.
            calls is < 0 or > LegalTrainingTeacherAdmissionAuthority.HardMaximumTeacherCalls) // Keep the protected budget within the existing teacher authority's hard cap.
            return InvalidCatalog(); // Retain a failed immutable catalog so lookup cannot admit any grant.
        return FromTrustedGrants(map.Values, calls); // Recopy ALL present dependencies and bind full selected public budget to the same protected startup value.
    } // Validate the closed V2 grant shape and retain the actual protected call budget.

    private static CodeDocumentationOneLessonGrantCatalog LoadV3(JsonElement root, string json, Func<string, string?> reader) // Load full approvals on top of the unchanged strict V2 grant decoder.
    { // Validate the V3 envelope before retaining any declaration or grant capability.
        if (!CodeDocumentationOneLessonV2Wire.ValidUtf8Text(json, 1_048_576) || json.StartsWith('\uFEFF') || // Keep the1MiB valid UTF-8 bound and reject a leading byte-order marker.
            !CodeDocumentationOneLessonV2Wire.ExactFields(root, "SchemaVersion", "Grants", "TeachingDisclosureApprovals") || root.GetProperty("SchemaVersion").GetRawText() != "3" || // Require exactly the V3 envelope fields and canonical schema spelling3.
            root.GetProperty("TeachingDisclosureApprovals").ValueKind != JsonValueKind.Array || root.GetProperty("TeachingDisclosureApprovals").GetArrayLength() > 32) return InvalidCatalog(); // Require an approval array with at most32 declarations; malformed envelopes fail closed.
        using var v2 = JsonDocument.Parse("{\"SchemaVersion\":2,\"Grants\":" + root.GetProperty("Grants").GetRawText() + "}", new JsonDocumentOptions { MaxDepth = 8 }); // Reparse only the grant envelope at the original V2 depth8 bound.
        var catalog = LoadV2(v2.RootElement, v2.RootElement.GetRawText(), reader); // Reuse all existing V2 grant/attachment/budget validation instead of adding a weaker decoder.
        if (catalog._configurationErrorCode is not null) return InvalidCatalog(); // Refuse the complete V3 catalog if any retained V2 grant is invalid.
        var approvals = new Dictionary<(string, string), CodeDocumentationOneLessonTeachingDisclosureApproval>(); // Collect uniquely validated approvals before publishing their membership map.
        foreach (var value in root.GetProperty("TeachingDisclosureApprovals").EnumerateArray()) // Validate every approval against its actual retained grant, including unused approvals.
        { // Authenticate this approval's owner before decoding the rest of its declaration.
            if (!value.TryGetProperty("GrantId", out var id) || !value.TryGetProperty("GrantVersion", out var version) || id.ValueKind != JsonValueKind.String || version.ValueKind != JsonValueKind.String) return InvalidCatalog(); // Require string grant id/version fields rather than accepting coercions or missing owners.
            var matches = catalog._grantsBySelector.Values.Where(g => g.GrantId == id.GetString() && g.GrantVersion == version.GetString()).ToArray(); // Locate grants by exact declared id/version within the validated catalog.
            if (matches.Length != 1) return InvalidCatalog(); // Reject absent or ambiguous grant ownership for an approval.
            var approval = CodeDocumentationOneLessonTeachingDisclosureApproval.Decode(value, matches[0]); // Run the closed full-graph approval decoder with that one retained grant.
            if (approval is null || !approvals.TryAdd((approval.GrantId, approval.GrantVersion), approval)) return InvalidCatalog(); // Reject malformed approvals and duplicate membership for the same grant id/version.
        } // Finish validating this approval before retaining the next one.
        catalog.IsTeachingDisclosureV3 = true; catalog._teachingApprovals = approvals; // Publish V3 status and the complete approval map only after every row validates.
        return catalog; // Return the validated catalog without turning a callback loader into protected provenance.
    } // Finish V3 loading under the existing catalog exception boundary.

    private static bool ValidText(string? value, int maximumLength) // Reject blank or control-bearing identity text under the existing contract bounds.
        => !string.IsNullOrWhiteSpace(value) && // Reject absent or blank-only owner identity text.
           value.Length <= maximumLength && // Enforce the caller-specified identity character ceiling.
           !value.Any(char.IsControl) && // Reject control-bearing protected identity text.
           StringComparer.Ordinal.Equals(value, value.Trim()); // Require already-trimmed protected text; do not normalize it during admission.

    private static bool Sha256Hex(string? value) // Require a bare lowercase SHA-256 commitment in the existing catalog grammar.
        => value is { Length: 64 } && // Require exactly 64 characters before testing lowercase digest alphabet.
           value.All(character => character is >= '0' and <= '9' or >= 'a' and <= 'f'); // Admit only ASCII digits and lowercase a through f in the bare grant digest.

    private static CodeDocumentationOneLessonGrantCatalog InvalidCatalog() // Retain a failed immutable catalog so lookup cannot admit any grant.
        => new( // Create a permanently failed empty catalog rather than retain partly admitted grants.
            new Dictionary<string, CodeDocumentationOneLessonGrantV1>( // Keep every selector unavailable after catalog validation fails.
                StringComparer.Ordinal), // Preserve exact selector spelling even in the empty failure map.
            InvalidCatalogErrorCode); // Retain the existing invalid-catalog code for subsequent lookup failures.

    private sealed record GrantCatalogDocument( // Represent the unchanged legacy catalog envelope for its existing deserializer.
        int SchemaVersion, // Pin wire schema discriminator in this complete immutable value.
        CodeDocumentationOneLessonGrantV1[]? Grants); // Read the protected catalog array before validating each grant and duplicate key.
} // Finish code documentation one lesson grant catalog.

/// <summary>Restricted JCS for the integer/string/Boolean V2 capacity wire subset.</summary>
internal static partial class CodeDocumentationOneLessonV2Wire // Own one restricted canonical writer for compact and explicitly typed full V2 preimages.
{ // Share one restricted canonical writer across admitted V2 commitments.
    private static readonly UTF8Encoding Utf8 = new(false, true); // Reject malformed Unicode and emit canonical bytes without a UTF-8 BOM.
    private static readonly string[] DescriptorFields = // Define the complete closed compact descriptor property set.
    ["SchemaVersion", "SourceSelectionSha256", "ParentCheckpointSha256", "CandidateSelectionSha256", // Include schema and all source/checkpoint/candidate commitments in the compact shape.
     "CorpusDescriptorSha256", "EvaluationPlanSha256", "CaseCount", "RepeatCount", // Include full corpus/plan commitments and exact case/repeat counts.
     "EvaluatorProfileSha256", "ProfileSelectionSha256"]; // Include evaluator/profile commitments; no extra descriptor member is admitted.

    internal static bool ValidUtf8Text(string text, int maximumBytes) // Reject malformed Unicode or UTF-8 bytes exceeding the selected cap.
    { // Keep malformed Unicode within this bounded encoding predicate.
        try { return Utf8.GetByteCount(text) <= maximumBytes; } // Reject malformed Unicode or bytes exceeding the configured UTF-8 cap while retaining the existing exception boundary.
        catch (EncoderFallbackException) { return false; } // Return the existing ValidUtf8Text failure result after the permitted nonfatal exception.
    } // Leave malformed or oversized UTF-8 text unadmitted.

    internal static bool Hash(string? value, int length = 64) // Check digest alphabet/width without deriving a hash or authority.
        => value is not null && value.Length == length && // Require the exact requested digest width and an actual string.
           value.All(c => c is >= '0' and <= '9' or >= 'a' and <= 'f'); // Permit only ASCII digits and lowercase a through f in a bare digest.

    internal static bool ValidDescriptors(CodeDocumentationOneLessonDescriptorsV2? value) // Validate the compact schema and all seven digest identities; count bounds belong to ReadDescriptors.
        => value is { SchemaVersion: 2 } && Hash(value.SourceSelectionSha256) && // Require schema two and the complete source commitment.
           Hash(value.ParentCheckpointSha256) && Hash(value.CandidateSelectionSha256) && // Validate both parent and candidate compact digests.
           Hash(value.CorpusDescriptorSha256) && Hash(value.EvaluationPlanSha256) && // Validate both corpus and full-plan compact digests.
           Hash(value.EvaluatorProfileSha256) && Hash(value.ProfileSelectionSha256); // Validate evaluator and profile compact digests.

    internal static CodeDocumentationOneLessonDescriptorsV2? ReadDescriptors(JsonElement value) // Decode only the exact existing compact V2 descriptor fields.
    { // Validate every compact pin and both full-plan count pins before construction.
        if (!ExactFields(value, DescriptorFields) || // Reject extra, duplicate or missing members in the restricted wire object.
            value.GetProperty("SchemaVersion").ValueKind != JsonValueKind.Number || // Require a Number token for the compact schema version.
            value.GetProperty("CaseCount").ValueKind != JsonValueKind.Number || // Require a Number token for the complete held-out count.
            value.GetProperty("RepeatCount").ValueKind != JsonValueKind.Number || // Require a Number token for the selected repeat count.
            !value.GetProperty("SchemaVersion").TryGetInt32(out var version) || // Require the schema discriminator to fit Int32.
            !value.GetProperty("CaseCount").TryGetInt32(out var cases) || // Require the case-count pin to fit Int32.
            !value.GetProperty("RepeatCount").TryGetInt32(out var repeats) || // Require the repeat-count pin to fit Int32.
            cases is < 1 or > 200 || repeats is < 2 or > 5000 || // Enforce the existing compact descriptor public N and R bounds without deriving plan admission.
            value.EnumerateObject().Any(p => p.Name.EndsWith("Sha256", StringComparison.Ordinal) && // Check every digest-suffixed member in the already closed descriptor object.
                (p.Value.ValueKind != JsonValueKind.String || !Hash(p.Value.GetString())))) // Reject any digest that is not a string of bare lowercase SHA-256 hex.
            return null; // Return no typed descriptor after any shape, count or digest failure.
        var result = new CodeDocumentationOneLessonDescriptorsV2(version, // Construct the compact descriptor only from completely validated fields.
            value.GetProperty("SourceSelectionSha256").GetString()!, value.GetProperty("ParentCheckpointSha256").GetString()!, // Copy the validated source and parent compact commitments.
            value.GetProperty("CandidateSelectionSha256").GetString()!, value.GetProperty("CorpusDescriptorSha256").GetString()!, // Copy the validated candidate and corpus compact commitments.
            value.GetProperty("EvaluationPlanSha256").GetString()!, cases, repeats, // Copy the full-plan digest and exact complete case/repeat pins.
            value.GetProperty("EvaluatorProfileSha256").GetString()!, value.GetProperty("ProfileSelectionSha256").GetString()!); // Copy the validated evaluator and profile compact commitments.
        return ValidDescriptors(result) && CanonicalBytes(value).Length <= 697 ? result : null; // Admit the parsed compact descriptor only when its schema, digests and canonical 697-byte capacity all match.
    } // Validate every compact pin and both full-plan count pins before construction.

    internal static bool ExactFields(JsonElement value, params string[] names) // Reject extra, duplicate or missing members in the restricted wire object.
    { // Require a closed property set with no duplicates, missing values or null members.
        if (value.ValueKind != JsonValueKind.Object) return false; // Reject a non-object before inspecting exact field membership.
        var seen = new HashSet<string>(StringComparer.Ordinal); // Track each exact member spelling to detect duplicates.
        foreach (var p in value.EnumerateObject()) // Inspect every supplied member before declaring the object complete.
            if (!names.Contains(p.Name, StringComparer.Ordinal) || !seen.Add(p.Name) || p.Value.ValueKind == JsonValueKind.Null) // Reject unknown names, duplicate names and null values before reading members.
                return false; // Reject unknown, repeated or null members without a typed value.
        return seen.Count == names.Length; // Require every permitted member to occur exactly once.
    } // Require a closed property set with no duplicates, missing values or null members.

    internal static bool IsCanonical(JsonElement value) // Require supplied JSON text to equal the same restricted canonical writer output.
        => Utf8.GetBytes(value.GetRawText()).AsSpan().SequenceEqual(CanonicalBytes(value)); // Retain the exact supplied JSON text for canonicality or schema comparison.

    internal static byte[] CanonicalBytes(JsonElement value) // Serialize the existing restricted integer, string, Boolean and object subset with arrays disabled.
    { // Serialize the default restricted V2 subset with arrays disabled.
        var output = new StringBuilder(); // Allocate the same canonical writer buffer used by the existing restricted wire seams.
        Write(value, output, selectionArrays: false); // Use the same canonical writer with array admission disabled for the default wire seam.
        return Utf8.GetBytes(output.ToString()); // Return the exact UTF-8 bytes emitted by the same restricted canonical writer.
    } // Serialize the default restricted V2 subset with arrays disabled.

    internal static byte[] CanonicalTeachingDisclosureBytes(JsonElement value) // Canonicalize the closed full disclosure declaration using the existing restricted writer.
    { // Build one full declaration preimage without exposing a mutable serializer option.
        var output = new StringBuilder(); Write(value, output, selectionArrays: true); // Include the validated selection arrays so every approved source/entry contributes bytes.
        return Utf8.GetBytes(output.ToString()); // Return the exact UTF-8 canonical declaration bytes for equality and evidence checks.
    } // Finish serialization without substituting a digest for the complete declaration.

    // WHY: arrays are admitted only for the validated full SOURCE/CORPUS/PLAN preimages.
    // Existing descriptor/history entry points retain their restricted subset.
    internal static byte[] CanonicalSourceSelectionBytes( // Commit the complete immutable sources and pair map through the same restricted writer.
        CodeDocumentationOneLessonSourceSelection selection) // Serialize the immutable full source selection, never a caller digest.
    { // Commit every selected source and the complete pair ownership map.
        var curriculum = selection.TeachingCurriculum; // Preserve legacy absent bytes and commit the present curriculum through the same writer.
        var value = curriculum is null // Preserve legacy absent bytes and commit the present curriculum through the same writer.
            ? JsonSerializer.SerializeToElement(new { selection.Sources, selection.PairSelections }) // Preserve legacy absent bytes and commit the present curriculum through the same writer.
            : JsonSerializer.SerializeToElement(new // Preserve legacy absent bytes and commit the present curriculum through the same writer.
            { selection.Sources, selection.PairSelections, TeachingCurriculum = curriculum }); // Preserve legacy absent bytes and commit the present curriculum through the same writer.
        var output = new StringBuilder(); // Allocate the same canonical writer buffer used by the existing restricted wire seams.
        Write(value, output, selectionArrays: true); // Enable arrays only behind this strongly typed complete-selection projection.
        return Utf8.GetBytes(output.ToString()); // Return the exact UTF-8 bytes emitted by the same restricted canonical writer.
    } // Commit every selected source and the complete pair ownership map.

    internal static byte[] CanonicalCorpusSelectionBytes( // Commit the full immutable corpus descriptor and complete ordered case sequence.
        CodeDocumentationOneLessonCorpusSelection selection) // Serialize the immutable full corpus/case selection.
    { // Commit every corpus descriptor pin and every ordered held-out case.
        var value = JsonSerializer.SerializeToElement(new { selection.CaseIds, selection.Corpus }); // Project the explicit typed contract through the same existing JSON representation seam.
        var output = new StringBuilder(); // Allocate the same canonical writer buffer used by the existing restricted wire seams.
        Write(value, output, selectionArrays: true); // Enable arrays only behind this strongly typed complete-selection projection.
        return Utf8.GetBytes(output.ToString()); // Return the exact UTF-8 bytes emitted by the same restricted canonical writer.
    } // Commit every corpus descriptor pin and every ordered held-out case.

    internal static byte[] CanonicalEvaluationPlanBytes( // Commit exactly the six immutable plan fields, omitting only its computed hash.
        CodeDocumentationOneLessonEvaluationPlanSelection selection) // Serialize the immutable full evaluation plan.
    { // Commit exactly the six complete plan fields, excluding PlanSha256.
        var value = JsonSerializer.SerializeToElement(new // Project the explicit typed contract through the same existing JSON representation seam.
        { // Admit the complete case array only for the typed evaluation-plan projection.
            selection.PlanId, selection.CorpusId, selection.EvaluatorId, selection.CaseIds, // Commit descriptive plan/corpus/evaluator IDs and every ordered held-out case.
            selection.RepeatCount, selection.ExpectedEvaluationCallCount, // Project the exact selected arithmetic; PlanSha256 is deliberately omitted.
        }); // Finish canonical evaluation plan bytes.
        var output = new StringBuilder(); // Allocate the same canonical writer buffer used by the existing restricted wire seams.
        Write(value, output, selectionArrays: true); // Enable arrays only behind this strongly typed complete-selection projection.
        return Utf8.GetBytes(output.ToString()); // Return the exact UTF-8 bytes emitted by the same restricted canonical writer.
    } // Commit exactly the six complete plan fields, excluding PlanSha256.

    internal static byte[] CanonicalEvaluatorProfileBytes( // Commit exactly the eleven full typed evaluator fields through the same restricted writer.
        CodeDocumentationOneLessonEvaluatorSelection selection) // Accept only a validated closed immutable evaluator; no raw-JSON signed-path API is exposed.
    { // Project all nested full values explicitly and enable signed Int32 only at the two declared seed paths.
        var e = selection.Evaluator; // Read the deeply copied full snapshot without using its computed self hash.
        var g = e.GenerationSettings; var n = e.NoisePolicy; var p = e.EvaluationProfile; var c = e.CurrentConfiguration; var s = e.ScorerIdentity; // Read every closed nested value required by the full commitment.
        var value = JsonSerializer.SerializeToElement(new // Build the exact eleven-property preimage independently of serializer expansion of the selection type.
        { // Include all selected full values and protected pins; omit only the computed self hash.
            e.EvaluatorId, e.Version, e.ConfigurationSha256, e.ExamConditionSha256, // Commit the exact descriptive identity/version and protected generic pins.
            e.GenerationSettingsSha256, e.NoisePolicyIdentitySha256, // Commit both existing owner identity pins without replacing their algorithms.
            ScorerIdentity = new { s.DomainTag, s.ScorerId, s.ScorerVersion, s.NormalizationProfileId, s.MetricProfileId }, // Commit exactly the actual owner's five scorer fields.
            GenerationSettings = new { g.MaxNewTokens, g.Temperature, g.TopK, g.TopP, g.Seed, g.MaxHeldOutCases, g.MaxAnswerChars }, // Commit all seven full generation fields with binary32 values remaining strings.
            NoisePolicy = new { n.RepeatCount, n.StandardDeviationMultiplier }, // Commit both full noise values, including the exact binary64 multiplier string.
            EvaluationProfile = new { p.ProfileId, p.Purpose, p.FormatIdentity, p.RequestFormatMode, p.NumContext, p.Temperature, p.Seed, p.Think, p.NumPredict, p.RequestTimeoutSeconds, p.ConfigurationSha256 }, // Commit all eleven actual fixed-profile values and legacy evidence.
            CurrentConfiguration = new { c.ModelId, c.ExpectedOllamaManifestDigest, c.ExpectedOllamaModelLayerDigest, c.MaxTeacherCalls, c.LocalRetentionPolicyRef, c.OutputLicenseRef, c.DpoMaxStepCount }, // Commit exactly the seven public configuration values; never access private audit salt.
        }); // Close the full typed evaluator preimage without arrays or extra members.
        var output = new StringBuilder(); // Allocate the same canonical buffer used by every existing restricted commitment seam.
        Write(value, output, selectionArrays: false, evaluatorSeeds: true); // Enable the PRIVATE signed seed policy only behind this closed typed evaluator projection.
        return Utf8.GetBytes(output.ToString()); // Return complete canonical bytes preserving exact round-trip float strings and signed seed spelling.
    } // Leave every existing default/source/corpus/plan entry point under its original numeric grammar.

    internal static byte[] CanonicalProfileSelectionBytes(CodeDocumentationOneLessonProfileSelection selection) // Commit every typed field through the existing restricted canonical writer.
    { // Commit every typed field through the existing restricted canonical writer.
        var output = new StringBuilder(); // Only a fully validated typed preimage can enable complete profile arrays.
        Write(JsonSerializer.SerializeToElement(selection.Snapshot), output, selectionArrays: true); // Commit every typed field through the existing restricted canonical writer.
        return Utf8.GetBytes(output.ToString()); // Omit the computed digest and keep the existing restricted primitive grammar.
    } // Commit every typed field through the existing restricted canonical writer.

    private static void Write(JsonElement value, StringBuilder output, bool selectionArrays, // Preserve the existing restricted array policy independently of the evaluator-only seed scope.
        bool evaluatorSeeds = false, string path = "") // Track exact root-relative member paths privately; default entry points cannot enable signed seed admission.
    { // Write the complete V2 value through its existing restricted canonical grammar.
        switch (value.ValueKind) // Choose only object, admitted array, string, integer and Boolean wire forms.
        { // Dispatch this JSON value within the selected restricted wire grammar.
            case JsonValueKind.Object: // Handle only the restricted writer object branch under its existing primitive policy.
                output.Append('{'); // Start the canonical object without whitespace.
                var properties = value.EnumerateObject().OrderBy(p => p.Name, StringComparer.Ordinal).ToArray(); // Collect all object members in strict ordinal key order for canonical serialization.
                if (properties.Select(p => p.Name).Distinct(StringComparer.Ordinal).Count() != properties.Length) // Reject any duplicate object member before writing an ambiguous canonical representation.
                    throw new JsonException("Repeated V2 member."); // Reject this exact invalid canonical wire representation instead of emitting or accepting bytes.
                for (var i = 0; i < properties.Length; i++) // Write every object member in the established ordinal key order.
                { // Write each object member in strict ordinal order through the same writer.
                    if (i != 0) output.Append(','); // Separate object members only after the first member.
                    WriteString(properties[i].Name, output); output.Append(':'); // Preserve the existing canonical object key and colon spelling.
                    var childPath = path.Length == 0 ? properties[i].Name : path + "." + properties[i].Name; // Derive the exact root-relative path for the two closed evaluator seed fields.
                    Write(properties[i].Value, output, selectionArrays, evaluatorSeeds, childPath); // Recursively retain the same grammar and private exact seed scope for this complete member.
                } // Write each object member in strict ordinal order through the same writer.
                output.Append('}'); break; // Close the complete object and finish this value branch.
            case JsonValueKind.Array when selectionArrays: // Permit arrays only through a typed complete-selection entry point.
                output.Append('['); // Start the complete selected array without whitespace.
                var first = true; // Track the first member to avoid a leading separator.
                foreach (var item in value.EnumerateArray()) // Visit the entire supplied sequence without implicit truncation.
                { // Write every admitted selected array member without sorting or truncation.
                    if (!first) output.Append(','); // Separate array members only after the first member.
                    first = false; // Mark the first array member written so later members receive a comma separator.
                    Write(item, output, selectionArrays, evaluatorSeeds, path + "[]"); // Preserve existing array serialization while preventing any array item from aliasing an exact evaluator seed path.
                } // Write every admitted selected array member without sorting or truncation.
                output.Append(']'); break; // Close the complete selected array and finish this branch.
            case JsonValueKind.String: WriteString(value.GetString()!, output); break; // Handle only the restricted writer string branch under its existing primitive policy.
            case JsonValueKind.Number: // Handle only the restricted writer number branch under its existing primitive policy.
                if (evaluatorSeeds && (path == "GenerationSettings.Seed" || path == "EvaluationProfile.Seed")) // Admit signed numbers only at the two exact typed evaluator seed locations.
                { // Keep defensive Int32 and exact-text guards behind the closed typed projection.
                    if (!value.TryGetInt32(out var seed) || value.GetRawText() != seed.ToString(CultureInfo.InvariantCulture)) // Require the entire signed Int32 domain with canonical invariant decimal spelling.
                        throw new JsonException("Only canonical signed Int32 evaluator seeds are supported."); // Reject a malformed private representation without broadening the public/default grammar.
                    output.Append(seed.ToString(CultureInfo.InvariantCulture)); break; // Preserve legitimate negative/min/max seed values exactly in the full evaluator preimage.
                } // Every other numeric path continues through the unchanged nonnegative Int64 branch.
                if (!value.TryGetInt64(out var number) || number < 0) // Reject fractional, out-of-Int64 or negative numbers before canonical integer serialization.
                    throw new JsonException("Only bounded unsigned integer V2 numbers are supported."); // Reject this exact invalid canonical wire representation instead of emitting or accepting bytes.
                output.Append(number.ToString(CultureInfo.InvariantCulture)); break; // Write the admitted nonnegative Int64 in invariant decimal spelling.
            case JsonValueKind.True: output.Append("true"); break; // Handle only the restricted writer true branch under its existing primitive policy.
            case JsonValueKind.False: output.Append("false"); break; // Handle only the restricted writer false branch under its existing primitive policy.
            default: throw new JsonException("Unsupported V2 capacity value."); // Reject null, default arrays and every other unsupported primitive.
        } // Finish write.
    } // Write the complete V2 value through its existing restricted canonical grammar.

    private static void WriteString(string value, StringBuilder output) // Escape UTF-8 wire strings through the existing canonical escape rules.
    { // Emit strict canonical JSON escapes without replacing malformed Unicode.
        if (!ValidUtf8Text(value, 1_048_576)) throw new JsonException("Invalid V2 Unicode."); // Require bounded UTF-8 identity text for the existing V2 wire contract.
        output.Append('"'); // Open the canonical JSON string.
        foreach (var c in value) // Escape every UTF-16 character after strict Unicode admission.
        { // Emit one canonical spelling for every admitted string character.
            switch (c) // Choose the exact canonical JSON escape for this character.
            { // Choose one exact JSON escaping rule for this character.
                case '"': output.Append("\\\""); break; // Escape a quotation mark so it cannot terminate the string.
                case '\\': output.Append("\\\\"); break; // Escape a backslash so it cannot introduce an unintended escape.
                case '\b': output.Append("\\b"); break; // Use the short JSON escape for backspace.
                case '\t': output.Append("\\t"); break; // Use the short JSON escape for tab.
                case '\n': output.Append("\\n"); break; // Use the short JSON escape for line feed.
                case '\f': output.Append("\\f"); break; // Use the short JSON escape for form feed.
                case '\r': output.Append("\\r"); break; // Use the short JSON escape for carriage return.
                default: // Handle characters not covered by an explicit JSON escape case.
                    if (c < 0x20) output.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture)); // Encode other controls as lowercase four-digit Unicode escapes.
                    else output.Append(c); // Preserve admitted non-control characters without HTML escaping.
                    break; // Finish this character after its chosen escape or literal spelling.
            } // Finish this character after its canonical escape or literal spelling.
        } // Emit one canonical spelling for every admitted string character.
        output.Append('"'); // Close the canonical JSON string.
    } // Emit strict canonical JSON escapes without replacing malformed Unicode.
} // Finish code documentation one lesson v2 wire.
