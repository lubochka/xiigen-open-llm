// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning; // Keep full startup decoding inside the existing catalog's assembly and ownership boundary.

using System.Globalization; // Require canonical integer spelling independently of startup culture.
using System.Text; // Compare strict UTF-8 input with complete existing typed canonical fragments.
using System.Text.Json; // Examine raw closed shapes before constructing any selected owner value.
using AgentInfrastructure; // Consume ordinary pure factory results without inventing a new authority.

/// <summary>Pure full-input decoding for the existing protected catalog; no root, file or effect capability.</summary>
internal static class CodeDocumentationOneLessonStartupSelectionDecoder // Bridge full selected values to the unchanged compact catalog representation.
{ // Confine all raw readers and full-input envelope assembly to this catalog helper.
    private static readonly UTF8Encoding Utf8 = new(false, true); // Reject malformed Unicode instead of replacing it in canonical comparison.

    internal static CodeDocumentationOneLessonGrantV1? Decode( // Return only a completely validated full grant or an ordinary invalid-input result.
        JsonElement descriptors, CodeDocumentationOneLessonGrantV1 scalarGrant) // Receive protected grant scalars, never request-owned selections or a repository root.
    { // Convert shape/primitive rejection to no full selection while retaining outer Load's existing failure boundary.
        try { return DecodeFull(descriptors, scalarGrant); } // Keep the single internal seam restricted to the full nine-field input.
        catch (JsonException) { return null; } // Reject a malformed full value without exposing partially built attachments.
    } // No file state or effect owner is consulted by this decoding boundary.

    private static CodeDocumentationOneLessonGrantV1 DecodeFull( // Reconstruct every selected commitment using its existing pure factory.
        JsonElement value, CodeDocumentationOneLessonGrantV1 grant) // Carry the exact protected scalar grant through dependent full-value admission.
    { // Validate the complete raw graph before deriving compact pins and performing canonical comparison.
        var hasProfile = value.ValueKind == JsonValueKind.Object && value.TryGetProperty("ProfileSelection", out _); // Choose a disjoint exact full shape before reading descendants.
        var hasCurriculum = value.ValueKind == JsonValueKind.Object && value.TryGetProperty("TeachingCurriculum", out _); // Choose the exact optional full shape and commit every decoded curriculum field.
        string[] fields = ["SchemaVersion", "Sources", "PairSelections", "ParentCheckpoint", "Candidate", // Choose the exact optional full shape and commit every decoded curriculum field.
            "Corpus", "EvaluationPlan", "Evaluator", hasProfile ? "ProfileSelection" : "ProfileSelectionSha256"]; // Choose the exact optional full shape and commit every decoded curriculum field.
        Object(value, hasCurriculum ? [.. fields, "TeachingCurriculum"] : fields); // Choose the exact optional full shape and commit every decoded curriculum field.
        if (Integer(value, "SchemaVersion") != 2) throw Invalid(); // Admit only schema two in the disjoint full startup input.
        var profile = hasProfile ? CodeDocumentationOneLessonProfileSelection.Decode(value.GetProperty("ProfileSelection")) : null; // Pure descriptive decode performs no material IO.
        var profilePin = profile?.ProfileSelectionSha256 ?? Text(value, "ProfileSelectionSha256"); // Only the full value supplies a locally computed new pin.
        if (!CodeDocumentationOneLessonV2Wire.Hash(profilePin)) throw Invalid(); // Require the same bare lowercase digest grammar as compact history.
        var sources = Array(value, "Sources", 1, 8).EnumerateArray().Select(Source).ToArray(); // Decode all selected sources without sorting or truncation.
        var pairs = Array(value, "PairSelections", 1, 1).EnumerateArray().Select(Pair).ToArray(); // Decode exactly one explicit source/pair mapping.
        var parent = Parent(value.GetProperty("ParentCheckpoint")); // Decode every parent identity pin without deriving a checkpoint path or ID.
        var candidate = Candidate(value.GetProperty("Candidate")); // Decode the selected descriptive candidate independently of publication.
        var corpusValue = Corpus(value.GetProperty("Corpus")); // Decode all ten corpus pins without reading manifest or payload bytes.
        var inputPlan = Plan(value.GetProperty("EvaluationPlan")); // Retain every raw plan field, including supplied computed hash and complete cases.
        var evaluatorValue = Evaluator(value.GetProperty("Evaluator")); // Decode every nested evaluator field before pure owner admission.
        var curriculum = hasCurriculum ? Curriculum(value.GetProperty("TeachingCurriculum")) : null; // Choose the exact optional full shape and commit every decoded curriculum field.
        var source = Require(CodeDocumentationOneLessonSourceSelection.Create(sources, pairs, curriculum)); // Choose the exact optional full shape and commit every decoded curriculum field.
        var checkpoint = Require(CodeDocumentationOneLessonCheckpointSelection.Create(parent, candidate)); // Apply actual slug, step, weight and candidate bounds.
        var corpus = Require(CodeDocumentationOneLessonCorpusSelection.Create(corpusValue, inputPlan.CaseIds)); // Commit all cases from the single complete plan array.
        var plan = Require(CodeDocumentationOneLessonEvaluationPlanSelection.Create( // Reconstruct the full six-field commitment and checked complete N times R.
            corpus, inputPlan.PlanId, inputPlan.EvaluatorId, inputPlan.RepeatCount, inputPlan.ExpectedEvaluationCallCount)); // Preserve selected identities and exact arithmetic without repair.
        if (inputPlan.CorpusId != plan.CorpusId || inputPlan.PlanSha256 != plan.PlanSha256) throw Invalid(); // Reject supplied plan identity/hash disagreement with reconstructed full values.
        var evaluator = Require(CodeDocumentationOneLessonEvaluatorSelection.Create(corpus, plan, evaluatorValue)); // Reuse actual generation/noise/profile/scorer/configuration validators and full commitment.
        if (corpus.CaseIds.Count > evaluator.Evaluator.GenerationSettings.MaxHeldOutCases) throw Invalid(); // Apply the selected generation owner's full-case ceiling before startup admission.
        var result = grant with // Retain protected scalars while attaching all complete pure selections and their derived compact commitments.
        { // Populate only in-memory V2 attachments; the serialized V1 record stays unchanged.
            FormatVersion = 2, SourceSelection = source, CheckpointSelection = checkpoint, // Keep full source/pair and parent/candidate immutable values together.
            CorpusSelection = corpus, EvaluationPlanSelection = plan, EvaluatorSelection = evaluator, ProfileSelection = profile, // Preserve all complete attachments; opaque compatibility remains null.
            DescriptorsV2 = new(2, source.SourceSelectionSha256, checkpoint.ParentCheckpointSha256, // Derive schema/source/parent compact pins from their existing factories.
                checkpoint.CandidateSelectionSha256, corpus.CorpusDescriptorSha256, plan.PlanSha256, // Derive candidate/corpus/plain-plan commitments from complete selected values.
                corpus.CaseIds.Count, plan.RepeatCount, evaluator.EvaluatorProfileSha256, profilePin), // Retain complete N/R, full evaluator digest and explicitly opaque profile scalar.
        }; // The later SAME FromTrustedGrants chain revalidates all five attachments and scalar/protected-budget equality.
        if (!Utf8.GetBytes(value.GetRawText()).AsSpan().SequenceEqual(CanonicalFullInputBytes(result))) throw Invalid(); // Require exact full input bytes while preserving typed signed seeds and round-trip float strings.
        return result; // Return complete descriptive state; no owner Fresh, root resolution or effect is invoked.
    } // A single invalid descendant leaves the catalog unable to expose this grant.

    private sealed record FullPlanInput( // Hold only the temporary decoded seven-field input before immutable plan reconstruction.
        string PlanId, string PlanSha256, string CorpusId, string EvaluatorId, // Preserve every required plan key and the supplied computed commitment.
        string[] CaseIds, int RepeatCount, int ExpectedEvaluationCallCount); // Keep all decoded cases and exact arithmetic until the existing factory copies them.

    private static CodeDocumentationOneLessonSourceV2 Source(JsonElement e) // Decode the exact eight-field source tuple before source-owner validation.
    { // Require every source field once before its primitive reader accesses it.
        Object(e, "SourceId", "Role", "DomainSlug", "DesignPatternSlug", "DatasetVersion", // Close source identity/address members without accepting aliases or paths.
            "ManifestFileSha256", "ManifestFileByteLength", "SealedManifestSha256"); // Require separate raw manifest and sealed manifest identities.
        return new(Text(e, "SourceId"), Text(e, "Role"), Text(e, "DomainSlug"), Text(e, "DesignPatternSlug"), // Preserve descriptive identity and actual safe-slug input without normalization.
            Text(e, "DatasetVersion"), Text(e, "ManifestFileSha256"), Long(e, "ManifestFileByteLength"), Text(e, "SealedManifestSha256")); // Read complete source version and file/sealed pins.
    } // Actual source bounds and mapping uniqueness remain with SourceSelection.Create.

    private static CodeDocumentationOneLessonPairSelectionV2 Pair(JsonElement e) // Decode the single explicit source-to-pair tuple.
    { // Reject any extra, duplicate, missing or null mapping member before reading values.
        Object(e, "TrainingPairId", "SourceId"); // Admit only the actual two-field selected mapping.
        return new(Text(e, "TrainingPairId"), Text(e, "SourceId")); // Preserve the hash/key pair for existing source resolution validation.
    } // No manifest-order pair selection is introduced.

    private static CodeDocumentationOneLessonTeachingCurriculumV2 Curriculum(JsonElement e) // Decode the closed complete curriculum without an evaluation count quota.
    { // Decode the closed complete curriculum without an evaluation count quota.
        Object(e, "PolicyId", "PolicyVersion", "Sources", "Items", "CategoryBindings"); // Decode the closed complete curriculum without an evaluation count quota.
        return new(Text(e, "PolicyId"), Text(e, "PolicyVersion"), // Decode the closed complete curriculum without an evaluation count quota.
            NonemptyArray(e, "Sources").EnumerateArray().Select(TeachingSource).ToArray(), // Decode the closed complete curriculum without an evaluation count quota.
            NonemptyArray(e, "Items").EnumerateArray().Select(TeachingItem).ToArray(), // Decode the closed complete curriculum without an evaluation count quota.
            NonemptyArray(e, "CategoryBindings").EnumerateArray().Select(TeachingBinding).ToArray()); // Decode the closed complete curriculum without an evaluation count quota.
    } // Decode the closed complete curriculum without an evaluation count quota.

    private static CodeDocumentationOneLessonTeachingSourceV2 TeachingSource(JsonElement e) // Read every declared reference address and independent raw byte identity.
    { // Read every declared reference address and independent raw byte identity.
        Object(e, "SourceId", "CorpusId", "DomainSlug", "SubjectSlug", "CorpusVersion", // Read every declared reference address and independent raw byte identity.
            "ManifestFileSha256", "ManifestFileByteLength", "CorpusSha256", "PayloadSha256", "PayloadByteLength"); // Read every declared reference address and independent raw byte identity.
        return new(Text(e, "SourceId"), Text(e, "CorpusId"), Text(e, "DomainSlug"), Text(e, "SubjectSlug"), // Read every declared reference address and independent raw byte identity.
            Text(e, "CorpusVersion"), Text(e, "ManifestFileSha256"), Long(e, "ManifestFileByteLength"), // Read every declared reference address and independent raw byte identity.
            Text(e, "CorpusSha256"), Text(e, "PayloadSha256"), Long(e, "PayloadByteLength")); // Read every declared reference address and independent raw byte identity.
    } // Read every declared reference address and independent raw byte identity.

    private static CodeDocumentationOneLessonTeachingItemV2 TeachingItem(JsonElement e) // Read the complete declared source-entry link without opening the entry.
    { // Read the complete declared source-entry link without opening the entry.
        Object(e, "ItemId", "SourceId", "ReferenceEntryId", "SourceEntityFingerprint"); // Read the complete declared source-entry link without opening the entry.
        return new(Text(e, "ItemId"), Text(e, "SourceId"), Text(e, "ReferenceEntryId"), Text(e, "SourceEntityFingerprint")); // Read the complete declared source-entry link without opening the entry.
    } // Read the complete declared source-entry link without opening the entry.

    private static CodeDocumentationOneLessonTeachingCategoryBindingV2 TeachingBinding(JsonElement e) // Retain the entire supplied category order without sorting or selecting an item.
    { // Retain the entire supplied category order without sorting or selecting an item.
        Object(e, "Category", "OrderedItemIds"); // Retain the entire supplied category order without sorting or selecting an item.
        return new(Text(e, "Category"), NonemptyArray(e, "OrderedItemIds").EnumerateArray().Select(String).ToArray()); // Retain the entire supplied category order without sorting or selecting an item.
    } // Retain the entire supplied category order without sorting or selecting an item.

    private static JsonElement NonemptyArray(JsonElement e, string field) // Require a real nonempty array and preserve every supplied element.
    { // Require a real nonempty array and preserve every supplied element.
        var value = e.GetProperty(field); // Require a real nonempty array and preserve every supplied element.
        if (value.ValueKind != JsonValueKind.Array || value.GetArrayLength() == 0) throw Invalid(); // Require a real nonempty array and preserve every supplied element.
        return value; // Require a real nonempty array and preserve every supplied element.
    } // Require a real nonempty array and preserve every supplied element.

    private static CodeDocumentationOneLessonParentCheckpointV2 Parent(JsonElement e) // Decode the complete eight-field parent identity without opening weights.
    { // Require the full parent tuple used by the existing checkpoint commitment.
        Object(e, "Step", "DomainSlug", "DesignPatternSlug", "ModelIdSlug", "ModelVersion", // Close the actual step and four safe address slugs.
            "ManifestSha256", "WeightsSha256", "WeightsByteLength"); // Require exact manifest/weight digest and length pins.
        return new(Integer(e, "Step"), Text(e, "DomainSlug"), Text(e, "DesignPatternSlug"), Text(e, "ModelIdSlug"), // Read bounded numeric step and descriptive slugs before owner admission.
            Text(e, "ModelVersion"), Text(e, "ManifestSha256"), Text(e, "WeightsSha256"), Long(e, "WeightsByteLength")); // Preserve version and every immutable parent byte identity.
    } // Actual positive step, safe slugs and 16GiB cap remain with the checkpoint factory.

    private static CodeDocumentationOneLessonCandidateSelectionV2 Candidate(JsonElement e) // Decode only descriptive candidate identity and digest.
    { // Keep candidate wire input closed and independent of effect/publication receipts.
        Object(e, "CandidateId", "CandidateSha256"); // Require both candidate fields exactly once.
        return new(Text(e, "CandidateId"), Text(e, "CandidateSha256")); // Let the existing checkpoint factory validate both immutable values.
    } // Later full freezing compares the candidate digest against the protected grant scalar.

    private static CodeDocumentationOneLessonCorpusV2 Corpus(JsonElement e) // Decode every complete corpus pin without creating an address.
    { // Close corpus identity, purpose, slugs and independent manifest/payload byte pins.
        Object(e, "CorpusId", "Purpose", "DomainSlug", "SubjectSlug", "CorpusVersion", // Require all descriptive corpus identity fields.
            "ManifestFileSha256", "CorpusSha256", "ManifestFileByteLength", "PayloadSha256", "PayloadByteLength"); // Require all whole/raw manifest/payload hashes and lengths.
        return new(Text(e, "CorpusId"), Text(e, "Purpose"), Text(e, "DomainSlug"), Text(e, "SubjectSlug"), // Preserve the selected opaque key, exact purpose and safe-slug inputs.
            Text(e, "CorpusVersion"), Text(e, "ManifestFileSha256"), Text(e, "CorpusSha256"), // Preserve the complete selected version and manifest/corpus identities.
            Long(e, "ManifestFileByteLength"), Text(e, "PayloadSha256"), Long(e, "PayloadByteLength")); // Read exact nonnegative Int64 byte lengths before actual owner caps.
    } // The existing corpus factory admits all pins together with the complete plan case sequence.

    private static FullPlanInput Plan(JsonElement e) // Decode the seven supplied plan fields without trusting its computed hash.
    { // Require the full plan including its one complete CaseIds array.
        Object(e, "PlanId", "PlanSha256", "CorpusId", "EvaluatorId", "CaseIds", "RepeatCount", "ExpectedEvaluationCallCount"); // Reject partial plans, unknown fields and duplicate members before reconstruction.
        var cases = Array(e, "CaseIds", 1, 200).EnumerateArray().Select(String).ToArray(); // Preserve every supplied case in original order for corpus-owner validation.
        var hash = Text(e, "PlanSha256"); // Read the supplied computed plain-plan commitment as an equality target only.
        if (!CodeDocumentationOneLessonV2Wire.Hash(hash)) throw Invalid(); // Require bare lowercase SHA-256 spelling before reconstruction.
        return new(Text(e, "PlanId"), hash, Text(e, "CorpusId"), Text(e, "EvaluatorId"), // Preserve every descriptive identity required for reconstructed plan equality.
            cases, Integer(e, "RepeatCount"), Integer(e, "ExpectedEvaluationCallCount")); // Read complete selected arithmetic without coercion or truncation.
    } // Existing plan Create remains the sole plain six-field hash and checked arithmetic owner.

    private static CodeDocumentationOneLessonEvaluatorV2 Evaluator(JsonElement e) // Decode the exact eleven-field full evaluator before actual owner validators run.
    { // Require all full nested selections rather than digest-only representations.
        Object(e, "EvaluatorId", "Version", "ConfigurationSha256", "ExamConditionSha256", // Close the selected evaluator identity and protected generic scalar pins.
            "GenerationSettingsSha256", "NoisePolicyIdentitySha256", "ScorerIdentity", "GenerationSettings", // Require owner identity pins plus full scorer/generation values.
            "NoisePolicy", "EvaluationProfile", "CurrentConfiguration"); // Require complete noise, actual profile and public configuration snapshots.
        return new(Text(e, "EvaluatorId"), Text(e, "Version"), Text(e, "ConfigurationSha256"), Text(e, "ExamConditionSha256"), // Preserve exact descriptive identity and generic protected pins.
            Text(e, "GenerationSettingsSha256"), Text(e, "NoisePolicyIdentitySha256"), Scorer(e.GetProperty("ScorerIdentity")), // Preserve legacy pins and the actual five-field scorer snapshot.
            Generation(e.GetProperty("GenerationSettings")), Noise(e.GetProperty("NoisePolicy")), // Read complete generation and selected policy values without private generic writer access.
            Profile(e.GetProperty("EvaluationProfile")), Configuration(e.GetProperty("CurrentConfiguration"))); // Read all actual profile/public configuration values without accessing private audit salt.
    } // Existing evaluator Create remains the full selected-value validation and commitment owner.

    private static HeldOutScorerIdentity Scorer(JsonElement e) // Decode all five actual scorer fields without inventing an alternate scorer registry.
    { // Reject unknown, aliased or absent scorer members before owner-tuple comparison.
        Object(e, "DomainTag", "ScorerId", "ScorerVersion", "NormalizationProfileId", "MetricProfileId"); // Preserve the exact actual scorer representation.
        return new(Text(e, "DomainTag"), Text(e, "ScorerId"), Text(e, "ScorerVersion"), // Read domain/model/Ollama manifest identity in the existing five-field order.
            Text(e, "NormalizationProfileId"), Text(e, "MetricProfileId")); // Preserve actual profile and metric pins for the owner helper comparison.
    } // Evaluator Create obtains the exact current tuple from BuildCurrentScorerIdentity.

    private static CodeDocumentationOneLessonGenerationV2 Generation(JsonElement e) // Decode all seven selected generation fields with Seed as signed Int32 only.
    { // Keep binary32 values as canonical strings for the existing generation owner seam.
        Object(e, "MaxNewTokens", "Temperature", "TopK", "TopP", "Seed", "MaxHeldOutCases", "MaxAnswerChars"); // Require the complete generation contract before primitive reads.
        return new(Integer(e, "MaxNewTokens"), Text(e, "Temperature"), Integer(e, "TopK"), Text(e, "TopP"), // Preserve numeric bounds and exact SingleR text without locale conversion.
            Integer(e, "Seed", signed: true), Integer(e, "MaxHeldOutCases"), Integer(e, "MaxAnswerChars")); // Admit any canonical signed Int32 seed while other fields remain nonnegative.
    } // No seed range beyond Int32 or generic raw signed serialization is added.

    private static CodeDocumentationOneLessonNoiseV2 Noise(JsonElement e) // Decode the full selected uncertainty policy rather than its identity alone.
    { // Require both policy fields before the real constructor and identity validation.
        Object(e, "RepeatCount", "StandardDeviationMultiplier"); // Close the actual two-field noise selection.
        return new(Integer(e, "RepeatCount"), Text(e, "StandardDeviationMultiplier")); // Preserve R and exact NumberR multiplier without forcing canonical default policy.
    } // Evaluator Create verifies actual noise bounds and its unchanged legacy identity.

    private static CodeDocumentationOneLessonEvaluationProfileV2 Profile(JsonElement e) // Decode every field of the sole actual fixed evaluation descriptor.
    { // Require present Boolean and numeric extended controls without inventing nullable defaults.
        Object(e, "ProfileId", "Purpose", "FormatIdentity", "RequestFormatMode", "NumContext", // Close the actual identity/format/context fields.
            "Temperature", "Seed", "Think", "NumPredict", "RequestTimeoutSeconds", "ConfigurationSha256"); // Require every fixed control and unchanged legacy evidence pin.
        var think = e.GetProperty("Think"); // Read the required control only after its exact membership is established.
        if (think.ValueKind is not (JsonValueKind.True or JsonValueKind.False)) throw Invalid(); // Reject null/string/numeric aliases instead of coercing an evaluation control.
        return new(Text(e, "ProfileId"), Text(e, "Purpose"), Text(e, "FormatIdentity"), Text(e, "RequestFormatMode"), // Preserve actual descriptor identity and enum spelling.
            Integer(e, "NumContext"), Text(e, "Temperature"), Integer(e, "Seed", signed: true), think.GetBoolean(), // Preserve NumberR temperature and typed signed seed before fixed-owner equality.
            Integer(e, "NumPredict"), Text(e, "RequestTimeoutSeconds"), Text(e, "ConfigurationSha256")); // Preserve all actual extended controls and legacy evidence without new authority.
    } // Nonzero fixed-profile Seed is rejected by existing actual-owner equality, not a fabricated generic seed rule.

    private static CodeDocumentationOneLessonEvaluatorConfigurationV2 Configuration(JsonElement e) // Decode exactly the seven public selected configuration values.
    { // Exclude any private salt, path or additional configuration authority from startup full input.
        Object(e, "ModelId", "ExpectedOllamaManifestDigest", "ExpectedOllamaModelLayerDigest", // Require actual tagged model and both public artifact identity pins.
            "MaxTeacherCalls", "LocalRetentionPolicyRef", "OutputLicenseRef", "DpoMaxStepCount"); // Require public budget, legal evidence and actual DPO-step selection.
        return new(Text(e, "ModelId"), Text(e, "ExpectedOllamaManifestDigest"), Text(e, "ExpectedOllamaModelLayerDigest"), // Preserve all public model identity strings for actual owner validation.
            Integer(e, "MaxTeacherCalls"), Text(e, "LocalRetentionPolicyRef"), Text(e, "OutputLicenseRef"), Integer(e, "DpoMaxStepCount")); // Read selected budget/legal/step values; protected budget equality runs in SAME freeze.
    } // Actual resolved configuration reconciliation remains a later composition boundary.

    private static void Object(JsonElement e, params string[] fields) // Reject all raw shape ambiguity before any descendant property access.
    { // Apply the catalog's existing exact-case, duplicate/null/missing/unknown guard at every full object level.
        if (!CodeDocumentationOneLessonV2Wire.ExactFields(e, fields)) throw Invalid(); // Keep one closed-member guard instead of allowing serializer defaults or last-value wins.
    } // Every full reader calls this before reading any of its required values.

    private static JsonElement Array(JsonElement e, string field, int minimum, int maximum) // Require complete bounded arrays before traversing their elements.
    { // Admit only actual JSON arrays within the selected complete-vector bounds.
        var value = e.GetProperty(field); // Read a member whose presence was established by the containing object guard.
        if (value.ValueKind != JsonValueKind.Array || value.GetArrayLength() < minimum || value.GetArrayLength() > maximum) throw Invalid(); // Reject wrong container kind, empty/overwide vectors and implicit prefixes.
        return value; // Preserve the full original array for exhaustive element decoding.
    } // Array contents are decoded completely and then validated by their existing owners.

    private static string Text(JsonElement e, string field) => String(e.GetProperty(field)); // Read required strings only after the containing exact-field guard.
    private static string String(JsonElement e) // Reject null and all non-string primitives without converting input types.
    { // Retain decoded text unchanged for exact owner grammar and typed canonical comparison.
        if (e.ValueKind != JsonValueKind.String) throw Invalid(); // Reject a missing string primitive rather than coerce numbers or Booleans.
        return e.GetString() ?? throw Invalid(); // Require a real decoded string; malformed Unicode remains inside Load's existing exception boundary.
    } // No trimming, alias repair or alternative Unicode normalization is applied.

    private static int Integer(JsonElement e, string field, bool signed = false) // Parse Int32 under an explicitly private two-Seed admission policy.
    { // Keep raw fractional/overflow/spelling rejection reachable in the new decoder without exposing the private canonical writer.
        var value = e.GetProperty(field); // Read only a known required integer field in a closed full object.
        if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out var n) || (!signed && n < 0) || // Require exact numeric kind and Int32 range; negative values are limited to the two declared calls.
            value.GetRawText() != n.ToString(CultureInfo.InvariantCulture)) throw Invalid(); // Reject fractional/exponent/negative-zero aliases even when numeric conversion might otherwise succeed.
        return n; // Preserve the admitted integer without cast, rounding or a fabricated seed bound.
    } // The current typed evaluator writer's exact two seed paths remain unchanged.

    private static long Long(JsonElement e, string field) // Parse exact nonnegative Int64 raw byte-length fields before actual owner caps.
    { // Keep every length integral and canonical without narrowing the existing owner representation.
        var value = e.GetProperty(field); // Read only a known required length member of a closed tuple.
        if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt64(out var n) || n < 0 || // Reject fractional/overflow/negative byte lengths at raw primitive admission.
            value.GetRawText() != n.ToString(CultureInfo.InvariantCulture)) throw Invalid(); // Reject exponent or other spelling aliases before canonical selected-value use.
        return n; // Let the existing source/checkpoint/corpus factory apply its actual positive upper bound.
    } // Default canonical writer and owner length algorithms are not broadened or replaced.

    private static T Require<T>(OperationResult<T> result) // Consume only actual successful existing pure-owner results.
    { // Propagate ordinary factory rejection into a failed full decode without retaining partial grants.
        if (!result.IsSuccess || result.Value is null) throw Invalid(); // Reject owner admission failure before any dependent selected value is used.
        return result.Value; // Retain the actual validated immutable owner value rather than a caller digest.
    } // No synthetic owner or bypass substitutes for pure selected-value validation.

    private static JsonException Invalid() => new("The full startup selection is invalid."); // Keep rejection free of protected catalog contents.

    private static byte[] CanonicalFullInputBytes(CodeDocumentationOneLessonGrantV1 decoded) // Assemble only fixed full-input syntax around existing validated typed canonical fragments.
    { // Never send caller raw arrays or legitimate negative evaluator seeds through the default raw writer.
        var source = decoded.SourceSelection!; var checkpoint = decoded.CheckpointSelection!; // Read the complete attachments created by the preceding pure admission sequence.
        var corpus = decoded.CorpusSelection!; var plan = decoded.EvaluationPlanSelection!; var evaluator = decoded.EvaluatorSelection!; // Retain full dependency values for typed fragment generation.
        using var sources = JsonDocument.Parse(CodeDocumentationOneLessonV2Wire.CanonicalSourceSelectionBytes(source)); // Obtain already-canonical complete source/pair array fragments from the existing typed seam.
        using var corpora = JsonDocument.Parse(CodeDocumentationOneLessonV2Wire.CanonicalCorpusSelectionBytes(corpus)); // Obtain the complete corpus object from its existing typed preimage.
        using var plans = JsonDocument.Parse(CodeDocumentationOneLessonV2Wire.CanonicalEvaluationPlanBytes(plan)); // Obtain every six-field plan value from the existing plain-hash preimage.
        string P(string name) => plans.RootElement.GetProperty(name).GetRawText(); // Preserve exact already-canonical plan values, including the whole CaseIds array.
        string C<T>(T item) => Utf8.GetString(CodeDocumentationOneLessonV2Wire.CanonicalBytes(JsonSerializer.SerializeToElement(item))); // Reuse unchanged default escaping/integers for immutable scalar or parent/candidate values only.
        var fullPlan = "{\"CaseIds\":" + P("CaseIds") + ",\"CorpusId\":" + P("CorpusId") + // Begin the fixed seven-member input plan in exact ordinal property order.
            ",\"EvaluatorId\":" + P("EvaluatorId") + ",\"ExpectedEvaluationCallCount\":" + P("ExpectedEvaluationCallCount") + // Preserve the actual evaluator key and exact selected arithmetic.
            ",\"PlanId\":" + P("PlanId") + ",\"PlanSha256\":" + C(plan.PlanSha256) + ",\"RepeatCount\":" + P("RepeatCount") + "}"; // Insert only computed hash encoded by the same writer into the unchanged complete plan values.
        var full = "{\"Candidate\":" + C(checkpoint.Candidate) + ",\"Corpus\":" + corpora.RootElement.GetProperty("Corpus").GetRawText() + // Start the fixed full descriptor with two existing canonical selected objects.
            ",\"EvaluationPlan\":" + fullPlan + ",\"Evaluator\":" + Utf8.GetString(CodeDocumentationOneLessonV2Wire.CanonicalEvaluatorProfileBytes(evaluator)) + // Reuse the exact typed evaluator writer, preserving private signed Seed paths and float strings.
            ",\"PairSelections\":" + sources.RootElement.GetProperty("PairSelections").GetRawText() + ",\"ParentCheckpoint\":" + C(checkpoint.Parent) + // Preserve the complete existing pair array and parent preimage without raw JSON splicing.
            (decoded.ProfileSelection is null ? ",\"ProfileSelectionSha256\":" + C(decoded.DescriptorsV2!.ProfileSelectionSha256) : // Decode only the closed passive profile graph at startup.
                ",\"ProfileSelection\":" + Utf8.GetString(CodeDocumentationOneLessonV2Wire.CanonicalProfileSelectionBytes(decoded.ProfileSelection))) + // Branch only on the declared complete attachment, preserving opaque wire bytes.
            ",\"SchemaVersion\":" + C(2) + // Preserve the same schema scalar and existing typed seed writer.
            ",\"Sources\":" + sources.RootElement.GetProperty("Sources").GetRawText() + // Reuse the canonical curriculum fragment from the SAME typed SOURCE writer.
            (sources.RootElement.TryGetProperty("TeachingCurriculum", out var curriculum) // Reuse the canonical curriculum fragment from the SAME typed SOURCE writer.
                ? ",\"TeachingCurriculum\":" + curriculum.GetRawText() : "") + "}"; // Reuse the canonical curriculum fragment from the SAME typed SOURCE writer.
        return Utf8.GetBytes(full); // Return only the closed assembled UTF-8 envelope for exact raw-input comparison.
    } // Dispose every temporary generated-byte document; retain no JsonElement or source document in the grant.
} // Keep full startup parsing pure and downstream Fresh/effect admission outside this helper.
