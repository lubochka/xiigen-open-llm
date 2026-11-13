// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning; // Bind descriptive evaluator commitments to the existing corpus, assessment and profile owners.

using System.Globalization; // Require round-trip floating strings and signed seeds to be independent of machine culture.
using System.Security.Cryptography; // Hash the complete selected evaluator with its fixed commitment domain.
using System.Text; // Encode the domain separator before the same restricted canonical wire bytes.
using AgentInfrastructure; // Propagate pure admission and strict owner failures without returning an effect capability.

internal sealed record CodeDocumentationOneLessonGenerationV2( // Preserve all seven generation values, including exact SingleR spellings.
    int MaxNewTokens, string Temperature, int TopK, string TopP, // Pin generation bounds and the exact binary32 temperature/top-p representations.
    int Seed, int MaxHeldOutCases, int MaxAnswerChars); // Retain any signed Int32 seed and the full owner case/answer limits.

internal sealed record CodeDocumentationOneLessonNoiseV2( // Preserve the complete selected uncertainty policy independently of its legacy digest.
    int RepeatCount, string StandardDeviationMultiplier); // Pin repeats and exact finite binary64 round-trip multiplier.

internal sealed record CodeDocumentationOneLessonEvaluatorConfigurationV2( // Describe only the seven public immutable TP1 values; never expose the audit salt.
    string ModelId, string ExpectedOllamaManifestDigest, string ExpectedOllamaModelLayerDigest, // Pin the current local model tag, manifest and model layer.
    int MaxTeacherCalls, string LocalRetentionPolicyRef, string OutputLicenseRef, int DpoMaxStepCount); // Pin the public budget, legal evidence and existing DPO step contract.

internal sealed record CodeDocumentationOneLessonEvaluationProfileV2( // Commit every value of the fixed actual evaluation descriptor and its unchanged legacy evidence.
    string ProfileId, string Purpose, string FormatIdentity, string RequestFormatMode, // Pin the evaluation profile identity, purpose and explicit request format.
    int NumContext, string Temperature, int Seed, bool Think, int NumPredict, // Preserve context, exact NumberR temperature, signed seed and fixed evaluation controls.
    string RequestTimeoutSeconds, string ConfigurationSha256); // Preserve the exact timeout spelling and owner-computed length-framed profile digest.

internal sealed record CodeDocumentationOneLessonEvaluatorV2( // Keep the full selected evaluator preimage closed to exactly eleven properties.
    string EvaluatorId, string Version, string ConfigurationSha256, string ExamConditionSha256, // Pin evaluator identity/version and protected generic configuration/exam bindings.
    string GenerationSettingsSha256, string NoisePolicyIdentitySha256, HeldOutScorerIdentity ScorerIdentity, // Pin legacy generation/noise identities and the actual five-field scorer tuple.
    CodeDocumentationOneLessonGenerationV2 GenerationSettings, CodeDocumentationOneLessonNoiseV2 NoisePolicy, // Retain full generation and noise values instead of digest-only surrogates.
    CodeDocumentationOneLessonEvaluationProfileV2 EvaluationProfile, CodeDocumentationOneLessonEvaluatorConfigurationV2 CurrentConfiguration); // Retain the complete actual profile and public configuration snapshots.

/// <summary>Immutable descriptive evaluator identity; construction and Fresh grant no execution permission.</summary>
internal sealed class CodeDocumentationOneLessonEvaluatorSelection // Own the full evaluator commitment with its copied complete corpus and plan dependencies.
{ // Admit only closed typed values and compare actual owners before strict corpus I/O.
    internal const string InvalidErrorCode = "ONE_LESSON_EVALUATOR_SELECTION_INVALID"; // Identify evaluator admission or full-value disagreement through an ordinary failure result.
    private readonly CodeDocumentationOneLessonCorpusSelection _corpusSelection; // Retain the whole immutable corpus dependency freshly verified by the same copied plan.
    private readonly CodeDocumentationOneLessonEvaluationPlanSelection _planSelection; // Retain every case and exact plan arithmetic for the later strict owner reopen.
    private readonly CodeDocumentationSavedModelAssessmentGenerationSettings _settings; // Retain a private copied parsed generation snapshot, including signed seed and negative-zero bits.
    private readonly MeasurementNoisePolicyConfig _noise; // Retain a reconstructed immutable owner policy without imposing canonical certification scope.
    internal CodeDocumentationOneLessonEvaluatorV2 Evaluator { get; } // Expose the closed deeply copied full evaluator snapshot.
    internal string EvaluatorProfileSha256 { get; } // Expose the domain-separated digest of all eleven full evaluator fields.

    private CodeDocumentationOneLessonEvaluatorSelection( // Construct only after complete dependency and owner admission succeeds.
        CodeDocumentationOneLessonCorpusSelection corpus, CodeDocumentationOneLessonEvaluationPlanSelection plan, // Retain recopied whole corpus and full plan rather than caller references.
        CodeDocumentationOneLessonEvaluatorV2 evaluator, CodeDocumentationSavedModelAssessmentGenerationSettings settings, // Retain the fully copied evaluator and parsed actual generation values.
        MeasurementNoisePolicyConfig noise) // Retain the reconstructed selected noise owner after its legacy identity matches.
    { // Freeze descriptive state and commit its complete canonical preimage without I/O.
        _corpusSelection = corpus; _planSelection = plan; // Store the independently recopied complete dependencies together.
        _settings = settings with { }; // Copy the parsed immutable settings rather than retain a caller generation record.
        _noise = new(noise.RepeatCount, noise.StandardDeviationMultiplier); // Copy the actual policy values through their constructor bounds.
        Evaluator = evaluator; // Retain only records that the factory deeply copied and fully admitted.
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256); // Allocate pure SHA-256 state for this full commitment.
        hash.AppendData(Encoding.UTF8.GetBytes("xiigen-s04-evaluatorprofile-v2\n")); // Prefix the exact normative evaluator-property domain and one LF.
        hash.AppendData(CodeDocumentationOneLessonV2Wire.CanonicalEvaluatorProfileBytes(this)); // Commit the complete typed eleven-field preimage through the existing writer.
        EvaluatorProfileSha256 = Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant(); // Finalize the full domain-separated lowercase digest.
    } // Leave only immutable descriptive values; no root, stream, client or receipt is retained.

    internal static OperationResult<CodeDocumentationOneLessonEvaluatorSelection> Create( // Purely validate all selected values and recopy complete dependencies before hashing.
        CodeDocumentationOneLessonCorpusSelection? corpus, CodeDocumentationOneLessonEvaluationPlanSelection? plan, // Require both complete selections rather than compact pins or fabricated owner entries.
        CodeDocumentationOneLessonEvaluatorV2? evaluator) // Require the exact full typed evaluator snapshot.
    { // Perform all selected-value admission without deriving a root or touching filesystem state.
        if (corpus is null || plan is null || evaluator is null) return Invalid(); // Reject missing complete dependencies before reading their values.
        var copiedCorpus = CodeDocumentationOneLessonCorpusSelection.Create(corpus.Corpus, corpus.CaseIds); // Revalidate and recopy every corpus pin and ordered case.
        if (!copiedCorpus.IsSuccess || copiedCorpus.Value is null) return copiedCorpus.MapError<CodeDocumentationOneLessonEvaluatorSelection>(); // Preserve the pure corpus failure unchanged.
        var copiedPlan = CodeDocumentationOneLessonEvaluationPlanSelection.Create( // Rebuild every full plan field from the recopied corpus dependency.
            copiedCorpus.Value, plan.PlanId, plan.EvaluatorId, plan.RepeatCount, plan.ExpectedEvaluationCallCount); // Preserve selected identity and exact full-case arithmetic.
        if (!copiedPlan.IsSuccess || copiedPlan.Value is null) return copiedPlan.MapError<CodeDocumentationOneLessonEvaluatorSelection>(); // Preserve the pure plan rejection without admitting an evaluator.
        if (copiedPlan.Value.PlanSha256 != plan.PlanSha256 || // Require the supplied complete plan hash to bind the same corpus and cases.
            !CodeDocumentationOneLessonV2Wire.CanonicalEvaluationPlanBytes(copiedPlan.Value).AsSpan().SequenceEqual( // Compare all plan bytes in addition to their digest.
                CodeDocumentationOneLessonV2Wire.CanonicalEvaluationPlanBytes(plan))) return Invalid(); // Reject any dependency mismatch instead of silently replacing its commitment.
        var e = evaluator; var g = e.GenerationSettings; var n = e.NoisePolicy; var p = e.EvaluationProfile; var c = e.CurrentConfiguration; // Read the selected generation, noise, profile and public configuration only after full dependencies admit.
        if (!Token(e.EvaluatorId, 128) || !Token(e.Version, 64) || e.EvaluatorId != copiedPlan.Value.EvaluatorId || // Require bounded evaluator identity/version and exact full-plan identity binding.
            !CodeDocumentationOneLessonV2Wire.Hash(e.ConfigurationSha256) || !CodeDocumentationOneLessonV2Wire.Hash(e.ExamConditionSha256) || // Validate protected generic pins without claiming their unavailable recomputation authority.
            !CodeDocumentationOneLessonV2Wire.Hash(e.GenerationSettingsSha256) || !CodeDocumentationOneLessonV2Wire.Hash(e.NoisePolicyIdentitySha256) || // Require bare lower64 legacy generation and noise pins.
            g is null || n is null || p is null || c is null || e.ScorerIdentity is null) return Invalid(); // Reject missing required nested values before parsing or hashing.
        if (!SingleR(g.Temperature, out var temperature) || !SingleR(g.TopP, out var topP) || // Require exact finite binary32 round-trip strings; do not normalize aliases or negative zero.
            !DoubleR(n.StandardDeviationMultiplier, out var multiplier) || n.RepeatCount < 2 || multiplier <= 0 || // Require the real noise constructor's positive finite multiplier and repeat bounds.
            !DoubleR(p.Temperature, out _) || !DoubleR(p.RequestTimeoutSeconds, out _)) return Invalid(); // Validate both profile binary64 strings before exact fixed-owner comparison.
        var settings = new CodeDocumentationSavedModelAssessmentGenerationSettings( // Reconstruct all seven actual owner settings from the admitted closed generation snapshot.
            g.MaxNewTokens, temperature, g.TopK, topP, g.Seed, g.MaxHeldOutCases, g.MaxAnswerChars); // Preserve any Int32 seed and exact parsed float bits.
        if (!CertificationValidation.Generation(settings) || !ConfigurationValid(c) || // Delegate generation and public configuration constraints to their actual owners.
            n.RepeatCount != copiedPlan.Value.RepeatCount || checked(2 * copiedPlan.Value.ExpectedEvaluationCallCount) > c.MaxTeacherCalls || // Bind selected noise R and both full evaluation series to the selected public budget.
            p != CurrentProfile() || !ScorerValid(e.ScorerIdentity) || // Require every profile field and bounded five-field scorer representation.
            e.ScorerIdentity != CodeDocumentationSavedModelAssessmentCalibrationService.BuildCurrentScorerIdentity( // Require the SAME actual scorer owner rather than duplicate its private metric identity.
                copiedCorpus.Value.Corpus.DomainSlug, c.ModelId, c.ExpectedOllamaManifestDigest)) return Invalid(); // Bind scorer domain/model/Ollama manifest to the selected complete owners.
        var noise = new MeasurementNoisePolicyConfig(n.RepeatCount, multiplier); // Reconstruct the real immutable policy after its constructor bounds are checked.
        if (noise.IdentitySha256 != e.NoisePolicyIdentitySha256) return Invalid(); // Require the unchanged legacy policy hash to bind the complete selected noise values.
        var copied = e with { GenerationSettings = g with { }, NoisePolicy = n with { }, // Copy each immutable nested record instead of retaining caller record identities.
            EvaluationProfile = p with { }, CurrentConfiguration = c with { }, ScorerIdentity = e.ScorerIdentity with { } }; // Complete deep copying for profile/public configuration and the existing scorer record.
        return OperationResult<CodeDocumentationOneLessonEvaluatorSelection>.Ok( // Return the full immutable selection only after every typed owner constraint succeeds.
            new(copiedCorpus.Value, copiedPlan.Value, copied, settings, noise)); // Hash the complete copied full values; the generation digest remains a pin until actual corpus Fresh.
    } // Pure admission never fabricates held-out entries to reproduce a generation hash.

    internal OperationResult<CodeDocumentationOneLessonEvaluatorSelectionOpenResult> OpenFresh( // Compare current actual owners before deriving a root, then delegate the whole corpus reopen.
        string repositoryRoot, CodeDocumentationSavedModelAssessmentGenerationSettings? settings, // Use the fixed repository root and all actual generation values.
        ContinuousLearningTp1DpoConfiguration? configuration, MeasurementNoisePolicyConfig? noise, // Use actual immutable public configuration and noise owners without accessing the private salt.
        string? protectedConfigurationSha256, string? protectedExamConditionSha256) // Compare protected scalar binding pins; these arguments do not supply generic hash authority.
    { // Reject owner disagreement before corpus I/O and return only descriptive snapshots after strict readback.
        if (!CertificationValidation.Generation(settings) || configuration is null || noise is null ||
            configuration.EvaluationProfile != LocalQwenOllamaInferenceProfileAuthority.Evaluation || // This legacy selection format cannot bind a manual evaluation profile.
            !CodeDocumentationOneLessonV2Wire.Hash(protectedConfigurationSha256) || !CodeDocumentationOneLessonV2Wire.Hash(protectedExamConditionSha256) || // Require canonical protected generic binding pins.
            protectedConfigurationSha256 != Evaluator.ConfigurationSha256 || protectedExamConditionSha256 != Evaluator.ExamConditionSha256) return InvalidOpen(); // Reject scalar pin disagreement without claiming generic owner recomputation.
        var observed = Evaluator with { GenerationSettings = Generation(settings!), NoisePolicy = Noise(noise), // Project actual generation R bytes and all noise owner values before I/O.
            CurrentConfiguration = Configuration(configuration), EvaluationProfile = CurrentProfile(), // Project all seven public configuration values and the actual fixed profile evidence.
            ScorerIdentity = CodeDocumentationSavedModelAssessmentCalibrationService.BuildCurrentScorerIdentity( // Obtain the actual current scorer tuple from its owner helper.
                _corpusSelection.Corpus.DomainSlug, configuration.ModelId, configuration.ExpectedOllamaManifestDigest), // Bind actual model and OLLAMA manifest to the retained selected corpus domain.
            NoisePolicyIdentitySha256 = noise.IdentitySha256 }; // Use the real noise owner's unchanged legacy identity.
        var beforeIo = Create(_corpusSelection, _planSelection, observed); // Validate the full current-owner snapshot against all existing typed constraints before touching the root.
        if (!beforeIo.IsSuccess || beforeIo.Value is null || !SameCommitment(beforeIo.Value) || // Compare full canonical bytes AND self hash, so -0 and +0 cannot pass scalar equality.
            _noise.IdentitySha256 != noise.IdentitySha256 || Generation(_settings) != observed.GenerationSettings) return InvalidOpen(); // Retain explicit private copied generation/noise agreement in the pre-I/O boundary.
        var opened = _planSelection.OpenFresh(repositoryRoot, settings, configuration, noise); // Let the SAME copied plan reopen the whole strict corpus and call the actual joint budget planner.
        if (!opened.IsSuccess || opened.Value is null) return opened.MapError<CodeDocumentationOneLessonEvaluatorSelectionOpenResult>(); // Propagate the actual owner failure without retry, truncation or fabricated entries.
        var heldOut = opened.Value.Corpus.Entries.Where(static e => e.Partition == FrozenReferencePartition.HeldOut) // Use all actual reopened held-out entry objects after whole payload/provenance validation.
            .OrderBy(static e => e.EntryId, StringComparer.Ordinal).ToArray(); // Preserve the complete owner case sequence for the existing legacy generation hash algorithm.
        var generationHash = CodeDocumentationSavedModelAssessmentService.ComputeGenerationSettingsSha256(settings!, heldOut); // Recompute through the actual assessment owner, including all real held-out IDs.
        if (generationHash != Evaluator.GenerationSettingsSha256) return InvalidOpen(); // Reject disagreement with the selected generation pin after real owner readback.
        observed = observed with { GenerationSettingsSha256 = generationHash }; // Carry the independently owner-recomputed generation identity into the full observed snapshot.
        var reconstructed = Create(_corpusSelection, opened.Value.Selection, observed); // Reuse the retained complete corpus already freshly confirmed by SAME plan; no accessor or second reopen.
        if (!reconstructed.IsSuccess || reconstructed.Value is null || !SameCommitment(reconstructed.Value)) return InvalidOpen(); // Require complete canonical bytes and hash to agree after actual owner reconstruction.
        return OperationResult<CodeDocumentationOneLessonEvaluatorSelectionOpenResult>.Ok( // Return successful descriptive identity and the actual immutable corpus/arithmetic snapshots.
            new(reconstructed.Value, opened.Value.Corpus, opened.Value.QualityGatePlan)); // Return no client, evaluator, lease, stream, effect receipt or execution permission.
    } // Strict readback can touch owner custody state but this value grants no downstream effect.

    private bool SameCommitment(CodeDocumentationOneLessonEvaluatorSelection other) // Compare complete preimages as well as hashes, including signed-zero float strings.
        => EvaluatorProfileSha256 == other.EvaluatorProfileSha256 && // Require the entire domain-separated full snapshot digest to match.
           CodeDocumentationOneLessonV2Wire.CanonicalEvaluatorProfileBytes(this).AsSpan().SequenceEqual( // Preserve the byte-exact floating and full nested field identity.
               CodeDocumentationOneLessonV2Wire.CanonicalEvaluatorProfileBytes(other)); // Compare against the same explicit eleven-field writer projection.

    private static CodeDocumentationOneLessonGenerationV2 Generation(CodeDocumentationSavedModelAssessmentGenerationSettings g) // Project all actual generation values with invariant round-trip float spellings.
        => new(g.MaxNewTokens, g.Temperature.ToString("R", CultureInfo.InvariantCulture), g.TopK, // Preserve max tokens, binary32 temperature and the actual top-k bound.
            g.TopP.ToString("R", CultureInfo.InvariantCulture), g.Seed, g.MaxHeldOutCases, g.MaxAnswerChars); // Preserve binary32 top-p, any signed Int32 seed and whole-case/answer limits.

    private static CodeDocumentationOneLessonNoiseV2 Noise(MeasurementNoisePolicyConfig n) // Project the actual complete selected owner policy without requiring canonical defaults.
        => new(n.RepeatCount, n.StandardDeviationMultiplier.ToString("R", CultureInfo.InvariantCulture)); // Preserve selected R and exact finite binary64 multiplier spelling.

    private static CodeDocumentationOneLessonEvaluatorConfigurationV2 Configuration(ContinuousLearningTp1DpoConfiguration c) // Project only the seven admitted public immutable owner values.
        => new(c.ModelId, c.ExpectedOllamaManifestDigest, c.ExpectedOllamaModelLayerDigest, // Preserve actual local model tag and both owner artifact pins.
            c.MaxTeacherCalls, c.LocalRetentionPolicyRef, c.OutputLicenseRef, c.DpoMaxStepCount); // Preserve public budget, legal evidence and DPO steps without private audit salt access.

    private static CodeDocumentationOneLessonEvaluationProfileV2 CurrentProfile() // Read every fixed evaluation value and legacy hash from the sole actual profile authority.
    { // Fail explicitly if the actual descriptor ever ceases to provide its required extended evaluation fields.
        var p = LocalQwenOllamaInferenceProfileAuthority.EvaluationDescriptor; // Use the real evaluation descriptor rather than a selectable registry or guessed owner.
        if (!p.Think.HasValue || !p.NumPredict.HasValue) throw new InvalidOperationException("The evaluation descriptor must supply extended controls."); // Do not invent defaults for required actual owner fields.
        return new(p.ProfileId, p.Purpose, p.FormatIdentity, p.RequestFormatMode.ToString(), // Preserve the actual profile's four identity/format values.
            p.NumContext, p.Temperature.ToString("R", CultureInfo.InvariantCulture), p.Seed, p.Think.Value, p.NumPredict.Value, // Preserve actual context, NumberR temperature, seed and present extended controls.
            p.RequestTimeout.TotalSeconds.ToString("R", CultureInfo.InvariantCulture), p.Evidence.ConfigurationSha256); // Preserve actual timeout and unchanged signed-length-framed owner hash.
    } // No duplicated profile hash algorithm or generic configuration identity is derived here.

    private static bool ConfigurationValid(CodeDocumentationOneLessonEvaluatorConfigurationV2 c) // Validate the selected public snapshot with actual owner grammars and step validator.
        => ContinuousLearningTp1DpoConfiguration.IsCanonicalOllamaModelId(c.ModelId) && // Require the actual tagged local Qwen model identity grammar.
           ContinuousLearningTp1DpoConfiguration.IsCanonicalOllamaManifestDigest(c.ExpectedOllamaManifestDigest) && // Require the actual owner's bare lower64 manifest pin.
           ContinuousLearningTp1DpoConfiguration.IsCanonicalOllamaModelLayerDigest(c.ExpectedOllamaModelLayerDigest) && // Require the actual owner's bare lower64 model-layer pin.
           ContinuousLearningTp1DpoConfiguration.IsCanonicalLegalEvidenceRef(c.LocalRetentionPolicyRef) && // Require canonical protected retention evidence through the existing owner.
           ContinuousLearningTp1DpoConfiguration.IsCanonicalLegalEvidenceRef(c.OutputLicenseRef) && // Require canonical protected output-license evidence through the existing owner.
           c.MaxTeacherCalls is > 0 and <= LegalTrainingTeacherAdmissionAuthority.HardMaximumTeacherCalls && // Preserve the actual hard call cap; selected joint fit is separately checked from the complete plan.
           StableDpoTrainingExecutor.ValidateDpoStepCount(c.DpoMaxStepCount).IsSuccess; // Reuse the actual DPO step admission without constructing an authority or exposing audit salt.

    private static bool ScorerValid(HeldOutScorerIdentity s) // Require all five existing scorer fields to use the selected bounded token grammar.
        => Token(s.DomainTag, 128) && Token(s.ScorerId, 128) && Token(s.ScorerVersion, 128) && // Bound domain, model and actual OLLAMA manifest representation.
           Token(s.NormalizationProfileId, 128) && Token(s.MetricProfileId, 128); // Bound the actual evaluation-profile and private-owner metric identities.

    private static bool Token(string? s, int maximum) // Require exact bounded ASCII opaque identities without aliases or whitespace normalization.
        => s is { Length: >= 1 } && s.Length <= maximum && // Enforce each exact declared width before indexing its first character.
           (s[0] is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9') && // Require an ASCII alphanumeric leading character.
           s.All(c => c is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9' or '.' or '_' or ':' or '-'); // Admit only the closed identity alphabet.

    private static bool FloatText(string? s) // Bound canonical floating identity strings before parsing their exact invariant spelling.
        => s is { Length: >= 1 and <= 32 } && s.All(c => c <= 0x7f); // Require one through thirty-two ASCII characters without silently normalizing text.

    private static bool SingleR(string? s, out float value) // Admit finite canonical binary32 round-trip strings including the distinct -0 spelling.
    { // Initialize the parsed value even when a missing or oversized string short-circuits admission.
        value = default; // Provide a defined out value for rejected strings without treating it as accepted generation state.
        return FloatText(s) && float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value) && // Parse exactly the selected text with invariant binary32 semantics.
            float.IsFinite(value) && value.ToString("R", CultureInfo.InvariantCulture) == s; // Reject NaN/Infinity, aliases and whitespace; preserve exact signed-zero spelling.
    } // Return a parsed float only together with canonical admission success.

    private static bool DoubleR(string? s, out double value) // Admit finite canonical binary64 round-trip policy/profile strings.
    { // Keep parsing bounded and culture-independent for every binary64 selected field.
        value = default; // Initialize rejected out values without inventing a selected policy default.
        return FloatText(s) && double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value) && // Parse the exact selected text under invariant binary64 rules.
            double.IsFinite(value) && value.ToString("R", CultureInfo.InvariantCulture) == s; // Require the exact finite NumberR spelling rather than normalize aliases.
    } // Leave owner range validation to the actual policy and fixed profile constraints.

    private static OperationResult<CodeDocumentationOneLessonEvaluatorSelection> Invalid() // Reject invalid pure descriptive evaluator input without admitting a selection.
        => OperationResult<CodeDocumentationOneLessonEvaluatorSelection>.Fail( // Return the shared ordinary failure contract without an effect-bearing value.
            "The complete selected evaluator is invalid.", InvalidErrorCode); // Identify pure full-value admission failure with the stable evaluator code.

    private static OperationResult<CodeDocumentationOneLessonEvaluatorSelectionOpenResult> InvalidOpen() // Reject full current-owner disagreement without returning Fresh snapshots.
        => OperationResult<CodeDocumentationOneLessonEvaluatorSelectionOpenResult>.Fail( // Return no usable descriptive result after selected/current mismatch.
            "Fresh evaluator owners do not match the complete selected evaluator.", InvalidErrorCode); // Distinguish the ordinary Fresh mismatch while retaining the same stable selection code.
} // End the immutable full evaluator owner comparison seam.

internal sealed record CodeDocumentationOneLessonEvaluatorSelectionOpenResult( // Return only descriptive full evaluator, corpus and actual joint arithmetic snapshots.
    CodeDocumentationOneLessonEvaluatorSelection Selection, FrozenReferenceCorpus Corpus, // Retain the complete fresh selected identity and strict owner corpus without a lease or stream.
    CodeDocumentationRepositoryOwnedDpoTrainingQualityGatePlan QualityGatePlan); // Retain the actual integer joint plan without granting evaluator execution.
