// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning; // Keep full evaluation-plan identity beside its corpus and arithmetic owners.

using System.Collections.ObjectModel; // Expose all privately copied case IDs through a read-only wrapper.
using System.Security.Cryptography; // Compute the plain six-field evaluation-plan SHA-256.
using AgentInfrastructure; // Return plan admission and fresh owner-validation failures through the shared result contract.

/// <summary>Complete immutable evaluation-plan commitment; descriptive identity never grants an effect.</summary>
internal sealed class CodeDocumentationOneLessonEvaluationPlanSelection // Own the immutable complete plan and plain six-field plan commitment.
{ // Keep the full plan and its corpus dependency in one immutable selection.
    internal const string InvalidErrorCode = "ONE_LESSON_EVALUATION_PLAN_SELECTION_INVALID"; // Identify pure plan rejection and fresh plan disagreement with the stable selection code.
    private readonly CodeDocumentationOneLessonCorpusSelection _corpusSelection; // Retain the copied complete corpus dependency required for fresh owner reopen.
    private readonly string[] _caseIds; // Retain private ordered cases so external code cannot mutate the plan commitment.
    internal string PlanId { get; } // Expose opaque selected plan identity as part of the immutable owner contract.
    internal string PlanSha256 { get; } // Expose plain six-field plan hash as part of the immutable owner contract.
    internal string CorpusId { get; } // Expose opaque selected corpus key as part of the immutable owner contract.
    internal string EvaluatorId { get; } // Expose descriptive evaluator key as part of the immutable owner contract.
    internal IReadOnlyList<string> CaseIds { get; } // Expose the complete ordered held-out case sequence as part of the immutable owner contract.
    internal int RepeatCount { get; } // Expose selected repeat count as part of the immutable owner contract.
    internal int ExpectedEvaluationCallCount { get; } // Expose exact single-series call pin as part of the immutable owner contract.

    private CodeDocumentationOneLessonEvaluationPlanSelection( // Copy every selected case and hash the six full plan properties without a domain prefix.
        CodeDocumentationOneLessonCorpusSelection corpus, string planId, string evaluatorId, // Receive the complete corpus and the selected descriptive plan/evaluator keys.
        int repeatCount, int expectedEvaluationCallCount) // Receive the selected R and exact expected-call pin for full-case arithmetic.
    { // Copy all cases and retain the exact admitted plan arithmetic.
        _corpusSelection = corpus; // Retain the recopied full corpus selection for the later fresh owner reopen.
        _caseIds = corpus.CaseIds.ToArray(); // Copy all ordered owner case IDs into a private array.
        CaseIds = new ReadOnlyCollection<string>(_caseIds); // Expose the complete copied array through a mutation-rejecting wrapper.
        PlanId = planId; // Store the admitted opaque plan key unchanged.
        CorpusId = corpus.Corpus.CorpusId; // Derive the corpus key from its admitted descriptor rather than its human name.
        EvaluatorId = evaluatorId; // Store a descriptive evaluator key without granting evaluator authority.
        RepeatCount = repeatCount; // Store the selected repeat count without reduction.
        ExpectedEvaluationCallCount = expectedEvaluationCallCount; // Store the exact checked single-series call pin.
        PlanSha256 = Convert.ToHexString(SHA256.HashData( // Hash plain canonical plan bytes without a domain prefix.
            CodeDocumentationOneLessonV2Wire.CanonicalEvaluationPlanBytes(this))).ToLowerInvariant(); // Use the same typed six-field writer and lowercase the resulting digest.
    } // Finish code documentation one lesson evaluation plan selection.

    internal static OperationResult<CodeDocumentationOneLessonEvaluationPlanSelection> Create( // Purely derive the complete immutable plan and checked arithmetic from its corpus selection.
        CodeDocumentationOneLessonCorpusSelection? corpus, string? planId, string? evaluatorId, // Validate complete corpus dependency and descriptive plan/evaluator keys.
        int repeatCount, int expectedEvaluationCallCount) // Validate selected R against the exact complete N times R call pin.
    { // Admit a complete plan only after copied corpus and exact full-case arithmetic agree.
        if (corpus is null || !Token(planId) || !Token(evaluatorId) || repeatCount < 2) // Reject missing corpus, invalid keys or R below two before reconstruction.
            return Invalid(); // Return normal invalid-selection failure without a usable immutable value.
        var copied = CodeDocumentationOneLessonCorpusSelection.Create(corpus.Corpus, corpus.CaseIds); // Revalidate and recopy every corpus pin and ordered case through the existing pure owner.
        if (!copied.IsSuccess || copied.Value is null) // Require a successfully recopied complete corpus before arithmetic.
            return copied.MapError<CodeDocumentationOneLessonEvaluationPlanSelection>(); // Propagate the corpus admission failure without returning a plan.
        int expected; // Reserve an Int32 result for checked full-case arithmetic.
        try // Catch only overflow from the complete checked N times R arithmetic.
        { // Derive the full-case expected count with checked Int32 multiplication.
            expected = checked(copied.Value.CaseIds.Count * repeatCount); // Compute the complete checked N times R before admitting a plan.
        } // Keep the computed count available only after checked arithmetic completes.
        catch (OverflowException) // Convert arithmetic overflow into an ordinary invalid-selection result.
        { // Convert the actual arithmetic overflow to an ordinary invalid plan.
            return Invalid(); // Return normal invalid-selection failure without a usable immutable value.
        } // Leave overflow without any usable immutable plan.
        if (expected is < 1 or > 5000 || expected != expectedEvaluationCallCount) // The single series must fit half the joint hard cap and equal the supplied expected-call pin.
            return Invalid(); // Return normal invalid-selection failure without a usable immutable value.
        return OperationResult<CodeDocumentationOneLessonEvaluationPlanSelection>.Ok( // Return the validated typed owner value after the preceding admission checks.
            new(copied.Value, planId!, evaluatorId!, repeatCount, expected)); // Construct only from the complete copied corpus and validated exact arithmetic.
    } // Admit a complete plan only after copied corpus and exact full-case arithmetic agree.

    internal OperationResult<CodeDocumentationOneLessonEvaluationPlanSelectionOpenResult> OpenFresh( // Accept only current owner settings, evaluator configuration and noise for fresh descriptive validation.
        string repositoryRoot, CodeDocumentationSavedModelAssessmentGenerationSettings? settings, // Use the fixed root and actual generation settings for the strict corpus reopen.
        ContinuousLearningTp1DpoConfiguration? configuration, MeasurementNoisePolicyConfig? noise) // Use the actual joint budget and noise repeats for owner planning.
    { // Bind the full selected plan to strict corpus readback and actual joint budget planning.
        if (configuration is null || noise is null || noise.RepeatCount != RepeatCount) // Reject missing current owners or repeat disagreement before any corpus I/O.
            return InvalidOpen(); // Reject fresh disagreement without exposing a snapshot or effect authority.
        var opened = _corpusSelection.OpenFresh(repositoryRoot, settings); // Reopen the same complete corpus through its current generation and strict repository owner checks.
        if (!opened.IsSuccess || opened.Value is null) // Require actual strict owner readback before calling the arithmetic planner.
            return opened.MapError<CodeDocumentationOneLessonEvaluationPlanSelectionOpenResult>(); // Propagate the original corpus-owner failure without exposing a fresh snapshot.
        var planned = CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.PlanQualityGate( // Call the existing actual joint planner rather than substituting a fixed budget.
            opened.Value.Corpus, settings, configuration, noise); // Supply the actual reopened corpus and current server generation, configuration and noise.
        if (!planned.IsSuccess || planned.Value is null) // Require the actual configured budget to admit both evaluation series.
            return planned.MapError<CodeDocumentationOneLessonEvaluationPlanSelectionOpenResult>(); // Propagate the actual planner failure without reducing repeats or retrying.
        var baseline = planned.Value.BaselinePlan; // Read the full baseline arithmetic returned by the existing planner.
        if (baseline.RepeatCount != RepeatCount || baseline.HeldOutCaseCount != _caseIds.Length || // Require returned R and full held-out N to equal the selected commitment.
            baseline.ExpectedEvaluationCallCount != ExpectedEvaluationCallCount || // Require the returned single-series count to equal the exact selected call pin.
            planned.Value.ExpectedTotalEvaluationCallCount != checked(2 * ExpectedEvaluationCallCount)) // Compare the actual joint plan with two selected series; the validated pin makes this multiplication safe.
            return InvalidOpen(); // Reject fresh disagreement without exposing a snapshot or effect authority.
        var reconstructed = Create(opened.Value.Selection, PlanId, EvaluatorId, // Reconstruct the full commitment from the actual fresh corpus selection and the same descriptive IDs.
            baseline.RepeatCount, baseline.ExpectedEvaluationCallCount); // Use actual returned R and expected count for that reconstruction.
        if (!reconstructed.IsSuccess || reconstructed.Value is null) // Require a valid reconstruction before comparing complete canonical bytes.
            return reconstructed.MapError<CodeDocumentationOneLessonEvaluationPlanSelectionOpenResult>(); // Return reconstruction failure without a usable fresh result.
        var observed = reconstructed.Value; // Retain the successfully rederived immutable plan for complete equality checks.
        if (observed.PlanId != PlanId || observed.CorpusId != CorpusId || observed.EvaluatorId != EvaluatorId || // Compare all descriptive keys, including the owner-derived corpus identity.
            observed.RepeatCount != RepeatCount || observed.ExpectedEvaluationCallCount != ExpectedEvaluationCallCount || // Compare exact selected arithmetic without count aliases or fallback.
            !observed.CaseIds.SequenceEqual(_caseIds, StringComparer.Ordinal) || observed.PlanSha256 != PlanSha256 || // Require every case in its original order and the entire plain plan hash to match.
            !CodeDocumentationOneLessonV2Wire.CanonicalEvaluationPlanBytes(observed).AsSpan().SequenceEqual( // Compare complete six-field canonical preimages as well as their hashes.
                CodeDocumentationOneLessonV2Wire.CanonicalEvaluationPlanBytes(this))) // Use the original selected plan as the byte-exact comparison target.
            return InvalidOpen(); // Reject fresh disagreement without exposing a snapshot or effect authority.
        return OperationResult<CodeDocumentationOneLessonEvaluationPlanSelectionOpenResult>.Ok( // Return the validated typed owner value after the preceding admission checks.
            new(observed, opened.Value.Corpus, planned.Value)); // Return only immutable plan, existing corpus snapshot and integer arithmetic; no effect capability.
    } // Bind the full selected plan to strict corpus readback and actual joint budget planning.

    private static bool Token(string? value) // Require the admitted opaque ASCII identity-token grammar.
        => value is { Length: >= 1 and <= 128 } && // Require the exact 1..128 opaque identity width.
           (value[0] is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9') && // Require an ASCII alphanumeric leading character.
           value.All(c => c is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9' // Admit only ASCII alphanumerics throughout the opaque identity.
               or '.' or '_' or ':' or '-'); // Allow only the specified punctuation after the first character.

    private static OperationResult<CodeDocumentationOneLessonEvaluationPlanSelection> Invalid() // Reject invalid descriptive input with the existing ordinary failure contract.
        => OperationResult<CodeDocumentationOneLessonEvaluationPlanSelection>.Fail( // Return an ordinary failed result without a usable admitted value.
            "The complete startup evaluation plan selection is invalid.", InvalidErrorCode); // Report invalid descriptive plan input with the existing selection failure code.

    private static OperationResult<CodeDocumentationOneLessonEvaluationPlanSelectionOpenResult> InvalidOpen() // Report disagreement with the complete selected commitment without returning authority.
        => OperationResult<CodeDocumentationOneLessonEvaluationPlanSelectionOpenResult>.Fail( // Return an ordinary failed result without a usable admitted value.
            "Fresh evaluation planning does not match the complete selected plan.", InvalidErrorCode); // Report fresh plan disagreement without returning an admitted descriptive snapshot.
} // Finish code documentation one lesson evaluation plan selection.

/// <summary>Fresh descriptive snapshots only; no evaluator, stream, lease, or effect receipt is returned.</summary>
internal sealed record CodeDocumentationOneLessonEvaluationPlanSelectionOpenResult( // Carry the immutable complete plan, reconstructed corpus and actual integer-only planner result.
    CodeDocumentationOneLessonEvaluationPlanSelection Selection, FrozenReferenceCorpus Corpus, // Retain the full selected plan and actual owner corpus snapshot.
    CodeDocumentationRepositoryOwnedDpoTrainingQualityGatePlan QualityGatePlan); // Retain the actual joint arithmetic result without evaluator or effect authority.
