// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentAlgorithms;

// ExternalAiRequestPackage is the complete, user-visible reason for one external AI approval request.
public sealed record ExternalAiRequestPackage(
    // OriginalRequest keeps the user's own request so the external question stays traceable.
    string OriginalRequest,
    // CompiledLogicalStructure carries the local meaning tokens that the internal flow already produced.
    string CompiledLogicalStructure,
    // InternalAttempt records what the local model tried before asking for outside help.
    string InternalAttempt,
    // InternalConfidence records how strongly the local model trusted its own attempt.
    double InternalConfidence,
    // MissingCompetence names the exact skill gap instead of using a vague low-confidence label.
    string MissingCompetence,
    // RequiredAnswerShape tells the external model what kind of answer can be adapted back into the local flow.
    string RequiredAnswerShape,
    // ArbiterConstraints lists the quality rules that still govern any external answer.
    string[] ArbiterConstraints,
    // ApprovalScope makes approval single-use by default.
    string ApprovalScope = "single_request")
{
    // ToApprovalPrompt creates the exact text shown before a user approves an external call.
    public string ToApprovalPrompt() =>
        // Start by making the boundary crossing explicit to the user.
        $"External AI approval requested for one request.\n" +
        // Show the named competence gap that caused the request.
        $"Missing competence: {MissingCompetence}\n" +
        // Show the confidence score from the internal attempt.
        $"Internal confidence: {InternalConfidence:F2}\n" +
        // Show the answer format the external model must satisfy.
        $"Required answer shape: {RequiredAnswerShape}\n" +
        // Show the arbiter rules that external output cannot override.
        $"Arbiter constraints: {string.Join(", ", ArbiterConstraints)}\n" +
        // Show the original request so the user can judge the context.
        $"Original request: {OriginalRequest}\n" +
        // Show the local logical structure so the external request is not raw text only.
        $"Compiled structure: {CompiledLogicalStructure}\n" +
        // Show the internal attempt so the user sees what failed locally.
        $"Internal attempt: {InternalAttempt}";
}

// ResultExamination is the internal model's structured judgment about its own answer.
public sealed record ResultExamination(
    // Confidence is the score used to decide whether outside help may be needed.
    double Confidence,
    // PresentPredicates lists the required meaning markers found in the local answer.
    string[] PresentPredicates,
    // MissingPredicates lists the required meaning markers not found in the local answer.
    string[] MissingPredicates,
    // SatisfiedArbiterConstraints lists the quality rules the local answer appears to satisfy.
    string[] SatisfiedArbiterConstraints,
    // NeedsExternalAi is true only when the local answer is weak and the missing skill is named.
    bool NeedsExternalAi,
    // MissingCompetence explains the gap in human language for approval and later learning.
    string MissingCompetence,
    // CorrectedResponse is either the original answer or a local correction note.
    string CorrectedResponse);

// InternalResultExaminer turns a local answer into a measurable confidence and gap report.
public sealed class InternalResultExaminer
{
    // Examine checks the local answer against required predicates and arbiter constraints.
    public OperationResult<ResultExamination> Examine(
        // originalRequest is the user request that the answer is supposed to satisfy.
        string originalRequest,
        // internalResponse is the answer produced by the internal model.
        string internalResponse,
        // requiredPredicates are the meaning markers that should appear in the answer.
        string[] requiredPredicates,
        // arbiterConstraints are the quality rules the answer should satisfy.
        string[] arbiterConstraints,
        // confidenceThreshold is the minimum acceptable internal confidence.
        double confidenceThreshold = 0.50)
    {
        // Reject an empty request because there is no user intent to examine.
        if (string.IsNullOrWhiteSpace(originalRequest))
            // Return a structured failure instead of throwing.
            return OperationResult<ResultExamination>.Fail("Original request is required.", "EMPTY_REQUEST");
        // Reject an empty response because there is no answer to score.
        if (string.IsNullOrWhiteSpace(internalResponse))
            // Return a structured failure instead of throwing.
            return OperationResult<ResultExamination>.Fail("Internal response is required.", "EMPTY_RESPONSE");

        // Normalize the response once so all later checks use the same case-insensitive view.
        var response = internalResponse.ToLowerInvariant();
        // Keep every required predicate whose text appears in the response.
        var present = requiredPredicates
            // Compare each predicate against the normalized response.
            .Where(p => response.Contains(p.ToLowerInvariant(), StringComparison.Ordinal))
            // Keep each detected predicate only once.
            .Distinct(StringComparer.Ordinal)
            // Materialize the detected predicates for the result record.
            .ToArray();
        // Keep every required predicate that was not detected.
        var missing = requiredPredicates
            // Treat a predicate as missing when it did not appear in the present list.
            .Where(p => !present.Contains(p, StringComparer.Ordinal))
            // Keep each missing predicate only once.
            .Distinct(StringComparer.Ordinal)
            // Materialize the missing predicates for the result record.
            .ToArray();
        // Keep every arbiter constraint whose name appears in the response.
        var satisfied = arbiterConstraints
            // Compare each arbiter name against the normalized response.
            .Where(a => response.Contains(a.ToLowerInvariant(), StringComparison.Ordinal))
            // Keep each satisfied arbiter only once.
            .Distinct(StringComparer.Ordinal)
            // Materialize the satisfied arbiters for the result record.
            .ToArray();

        // Count all required checks while preventing a divide-by-zero for empty criteria.
        int totalChecks = Math.Max(1, requiredPredicates.Length + arbiterConstraints.Length);
        // Score confidence as the fraction of required checks that were found.
        double confidence = (present.Length + satisfied.Length) / (double)totalChecks;
        // External help is needed only when confidence is low and something specific is missing.
        bool needsExternal = confidence < confidenceThreshold && missing.Length > 0;
        // Name the missing competence only when the result actually needs outside help.
        string missingCompetence = needsExternal
            // Explain exactly which required predicates lacked coverage.
            ? $"The internal model is missing coverage for {string.Join(", ", missing)}."
            // Leave the field empty when no outside help is needed.
            : "";
        // Add a correction note only when missing coverage must be resolved.
        string corrected = needsExternal
            // Preserve the original answer and append the missing coverage list.
            ? $"{internalResponse}\n\nMissing coverage to resolve before final answer: {string.Join(", ", missing)}."
            // Preserve the original answer unchanged when it is good enough.
            : internalResponse;

        // Return the complete examination so the caller can decide whether to ask for help.
        return OperationResult<ResultExamination>.Ok(new ResultExamination(
            // Store the computed confidence score.
            confidence,
            // Store the predicates found in the answer.
            present,
            // Store the predicates missing from the answer.
            missing,
            // Store the arbiter constraints that appeared satisfied.
            satisfied,
            // Store whether external help is needed.
            needsExternal,
            // Store the named missing competence.
            missingCompetence,
            // Store the corrected or original response.
            corrected));
    }
}

// PreprocessedLearningData is the local learning record produced from an approved external answer.
public sealed record PreprocessedLearningData(
    // Domain says which local model or learning set should receive this record.
    string Domain,
    // OriginalRequest preserves the user's request for later training context.
    string OriginalRequest,
    // InternalAttempt preserves the local answer that needed help.
    string InternalAttempt,
    // ExternalAnswer preserves the outside answer before local adaptation.
    string ExternalAnswer,
    // AdaptedAnswer is the local-form answer or the arbiter rejection reason.
    string AdaptedAnswer,
    // Predicates lists the local meaning markers extracted for learning.
    string[] Predicates,
    // SignalType says whether this became a learning pair or an arbiter rejection.
    string SignalType,
    // Source records that this came from one approved external AI request.
    string Source);

// ExternalAiResultAdapter converts external answers into local learning data under arbiter control.
public sealed class ExternalAiResultAdapter
{
    // Adapt checks the external answer, enforces arbiter constraints, and prepares local learning data.
    public OperationResult<PreprocessedLearningData> Adapt(
        // package is the approved external request context.
        ExternalAiRequestPackage package,
        // externalAnswer is the natural-language answer returned by the external model.
        string externalAnswer,
        // domain is the local subject area that should learn from this answer.
        string domain)
    {
        // Reject an empty external answer because there is nothing to adapt or learn from.
        if (string.IsNullOrWhiteSpace(externalAnswer))
            // Return a structured failure instead of throwing.
            return OperationResult<PreprocessedLearningData>.Fail("External answer is required.", "EMPTY_EXTERNAL_ANSWER");
        // Reject an empty domain because the learning record would have no owner.
        if (string.IsNullOrWhiteSpace(domain))
            // Return a structured failure instead of throwing.
            return OperationResult<PreprocessedLearningData>.Fail("Domain is required.", "EMPTY_DOMAIN");

        // Ask the named arbiter rules whether the external answer violates any locked constraint.
        var rejection = FindArbiterRejection(package, externalAnswer);
        // Stop adaptation when an arbiter rejects the external answer.
        if (rejection is not null)
            // Return a learning record that preserves the rejection reason as a warning example.
            return OperationResult<PreprocessedLearningData>.Ok(new PreprocessedLearningData(
                // Store the domain that would have received the adapted answer.
                domain,
                // Preserve the original user request.
                package.OriginalRequest,
                // Preserve the internal attempt that triggered the external call.
                package.InternalAttempt,
                // Preserve the external answer that was rejected.
                externalAnswer,
                // Store the arbiter rejection message as the adapted result.
                rejection,
                // Rejected external answers do not contribute positive predicates.
                [],
                // Mark this record as rejected by arbiter authority.
                "external_answer_rejected_by_arbiter",
                // Keep the single-request source marker for auditability.
                "external_ai_approved_single_request"));

        // Split the logical structure into individual local meaning tokens.
        var predicates = package.CompiledLogicalStructure
            // Break on separators used by the compact logical-structure text.
            .Split([' ', ',', ';', '|'], StringSplitOptions.RemoveEmptyEntries)
            // Keep only operation predicates and explicit failure markers.
            .Where(t => t.EndsWith("_OP", StringComparison.OrdinalIgnoreCase) || t.Equals("FAIL", StringComparison.OrdinalIgnoreCase))
            // Normalize predicates so future matching is stable.
            .Select(t => t.ToUpperInvariant())
            // Keep each predicate only once.
            .Distinct(StringComparer.Ordinal)
            // Materialize the predicate list for the learning record.
            .ToArray();

        // Wrap the external answer in the local domain wording used by downstream learning.
        string adapted = $"Adapted external answer for {domain}: {externalAnswer.Trim()}";
        // Return the adapted external answer as a DPO-ready learning signal.
        return OperationResult<PreprocessedLearningData>.Ok(new PreprocessedLearningData(
            // Store the local learning domain.
            domain,
            // Preserve the original user request.
            package.OriginalRequest,
            // Preserve the internal attempt that will become the rejected side of the learning pair.
            package.InternalAttempt,
            // Preserve the external answer that will become the correction source.
            externalAnswer,
            // Store the adapted answer that local learning can consume.
            adapted,
            // Store the extracted local predicates.
            predicates,
            // Mark this as an adapted answer ready for feedback learning.
            "external_answer_adapted_for_dpo",
            // Keep the single-request source marker for auditability.
            "external_ai_approved_single_request"));
    }

    // FindArbiterRejection checks only the named arbiter constraints supplied in the approved package.
    private static string? FindArbiterRejection(ExternalAiRequestPackage package, string externalAnswer)
    {
        // Examine each requested arbiter constraint in the package order.
        foreach (var constraint in package.ArbiterConstraints)
        {
            // Look up the named rule and run it only when this adapter knows that rule.
            if (KnownArbiterRules.TryGetValue(constraint, out var rule) &&
                // Ask the known rule whether this external answer violates it.
                rule.Contradicts(package, externalAnswer))
                // Return the human-readable rejection reason from the first violated arbiter.
                return rule.RejectionMessage;
        }

        // Unknown constraints or non-violated known constraints do not reject the answer.
        return null;
    }

    // ArbiterRule binds one named quality rule to its detector and rejection message.
    private sealed record ArbiterRule(
        // Contradicts decides whether an external answer violates this arbiter.
        Func<ExternalAiRequestPackage, string, bool> Contradicts,
        // RejectionMessage explains the violation in language suitable for a learning record.
        string RejectionMessage);

    // KnownArbiterRules is the local whitelist of arbiter constraints this adapter can enforce.
    private static readonly IReadOnlyDictionary<string, ArbiterRule> KnownArbiterRules =
        // Use case-insensitive names so packages do not fail because of casing differences.
        new Dictionary<string, ArbiterRule>(StringComparer.OrdinalIgnoreCase)
        {
            // identity_consistency keeps repeated provider callbacks from creating duplicate accounts.
            ["identity_consistency"] = new(
                // Use the duplicate-account detector for this arbiter.
                ContradictsLockedDuplicateRule,
                // Explain the identity consistency rejection in human language.
                "Rejected external answer because it contradicts the identity_consistency arbiter: repeated provider callbacks must not create duplicate accounts."),
            // token_safety keeps session tokens out of unsafe storage and transport locations.
            ["token_safety"] = new(
                // Use the token exposure detector for this arbiter.
                ContradictsTokenSafetyRule,
                // Explain the token safety rejection in human language.
                "Rejected external answer because it contradicts the token_safety arbiter: session tokens must not be exposed through local storage, query strings, or non-httpOnly cookies.")
        };

    // ContradictsLockedDuplicateRule detects the CH-02 duplicate-account failure pattern.
    private static bool ContradictsLockedDuplicateRule(ExternalAiRequestPackage package, string externalAnswer)
    {
        // Combine request and answer because the unsafe meaning can be split across both.
        string combined = $"{package.OriginalRequest} {externalAnswer}".ToLowerInvariant();
        // Detect that the case is about the same provider callback arriving more than once.
        bool duplicateCallback = combined.Contains("callback", StringComparison.Ordinal) &&
                                 // Treat twice, repeated, and same-provider wording as duplicate callback signals.
                                 (combined.Contains("twice", StringComparison.Ordinal) ||
                                  // Treat explicit repeated wording as duplicate callback evidence.
                                  combined.Contains("repeated", StringComparison.Ordinal) ||
                                  // Treat same-provider wording as duplicate callback evidence.
                                  combined.Contains("same provider", StringComparison.Ordinal));
        // Detect that the external answer would create an additional account.
        bool createsSecondAccount = combined.Contains("second account", StringComparison.Ordinal) ||
                                    // Treat duplicate-account wording as additional account creation.
                                    combined.Contains("duplicate account", StringComparison.Ordinal) ||
                                    // Treat create-another-account wording as additional account creation.
                                    combined.Contains("create another account", StringComparison.Ordinal);
        // Reject only when both the repeated callback and duplicate account creation are present.
        return duplicateCallback && createsSecondAccount;
    }

    // ContradictsTokenSafetyRule detects unsafe handling of session or identity tokens.
    private static bool ContradictsTokenSafetyRule(ExternalAiRequestPackage package, string externalAnswer)
    {
        // Combine request and answer because the token subject can appear in one and the unsafe storage in the other.
        string combined = $"{package.OriginalRequest} {externalAnswer}".ToLowerInvariant();
        // Detect that the answer is discussing a session, token, or JWT value.
        bool mentionsToken = combined.Contains("token", StringComparison.Ordinal) ||
                             // Treat session wording as token-related for signup flows.
                             combined.Contains("session", StringComparison.Ordinal) ||
                             // Treat JWT wording as token-related.
                             combined.Contains("jwt", StringComparison.Ordinal);
        // Detect storage or transport locations that expose the token to unsafe access.
        bool unsafeStorage = combined.Contains("localstorage", StringComparison.Ordinal) ||
                             // Detect spaced local storage wording.
                             combined.Contains("local storage", StringComparison.Ordinal) ||
                             // Detect query-string transport.
                             combined.Contains("query string", StringComparison.Ordinal) ||
                             // Detect URL-parameter transport.
                             combined.Contains("url parameter", StringComparison.Ordinal) ||
                             // Detect hyphenated non-httpOnly cookie wording.
                             combined.Contains("non-httponly", StringComparison.Ordinal) ||
                             // Detect spaced non-httpOnly cookie wording.
                             combined.Contains("non httponly", StringComparison.Ordinal);
        // Reject only when the answer is about tokens and also recommends unsafe exposure.
        return mentionsToken && unsafeStorage;
    }
}
