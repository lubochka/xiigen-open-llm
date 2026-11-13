// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure.Logical;

namespace ExperimentsV2.LogicalFlow;

// This record carries applier output into the node-06 result translator.
public sealed record SecurityResultTranslationInput(
    string ScenarioId,
    SecurityArbiterSelection Selection,
    SecurityAppliedResult AppliedResult);

// This record carries a translated formal result fragment from node-06 to node-09.
public sealed record SecurityTranslatedResult(
    string ScenarioId,
    string Verdict,
    string FormalResultFragment,
    SecurityAppliedResult AppliedResult,
    string SourcePairId,
    string[] RequiredResultSignals,
    string[] ForbiddenResultSignals);

// This unit is the CB4 pair-backed node-06 result translator for the security-verification flow.
public sealed class SecurityResultTranslatorUnit : ILogicalUnit<SecurityResultTranslationInput, SecurityTranslatedResult>
{
    // This id identifies the node-06 result translator in evidence and later unit registries.
    public const string Id = "unit-cb4-security-result-translator-v1";

    // This role names the station responsibility the unit fills.
    public const string Role = "node-06-result-translator";

    // This domain tag scopes the unit to the CB4 security-verification flow.
    public const string DomainTag = "security-verification-3.4";

    // This confidence remains above the replacement floor used by the prior proofs.
    public const double Confidence = 0.92;

    // Execute result translation for one arbiter verdict.
    public Task<AgentInfrastructure.OperationResult<LogicalUnitResult<SecurityTranslatedResult>>> ExecuteAsync(
        LogicalUnitRequest<SecurityResultTranslationInput> request,
        CancellationToken cancellationToken = default)
    {
        // Return cancellation through the shared operation envelope.
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<SecurityTranslatedResult>>.Fail("Cancelled.", "CANCELLED"));

        // Refuse empty translator input because a fragment cannot be built without an applied result.
        if (request.Input is null || request.Input.AppliedResult is null)
            return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<SecurityTranslatedResult>>.Fail("Security result translation input is required.", "EMPTY_RESULT_TRANSLATION_INPUT"));

        // Build the translated formal result fragment from the arbiter verdict.
        var translated = CreateAuthoritativeResultTranslation(request.Input);

        // Build the logical-unit result around the translated fragment.
        var result = new LogicalUnitResult<SecurityTranslatedResult>(
            request.UnitId,
            request.UnitRole,
            request.DomainTag,
            translated,
            Confidence,
            BuildPredicateIds(translated),
            [DomainTag],
            [translated.SourcePairId],
            []);

        // Return the translated result through the async unit contract.
        return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<SecurityTranslatedResult>>.Ok(result));
    }

    // Expose the single deterministic translation oracle to the in-assembly flow admission boundary.
    internal static SecurityTranslatedResult CreateAuthoritativeResultTranslation(SecurityResultTranslationInput input)
        => Translate(input);

    // Translate the arbiter verdict into a formal result fragment.
    private static SecurityTranslatedResult Translate(SecurityResultTranslationInput input)
    {
        // Build the fragment according to the applied verdict class.
        var fragment = input.AppliedResult.Verdict switch
        {
            // REPROCESS fragments must keep corrective language and the blocking rule id.
            "REPROCESS" => $"Formal result: REPROCESS. {input.AppliedResult.AppliedAction}",

            // CONCERN fragments must keep the boundary visible without converting it to a block.
            "CONCERN" => $"Formal result: CONCERN. {input.AppliedResult.AppliedAction}",

            // PASS fragments must keep the accepted evidence concise.
            _ => $"Formal result: PASS. {input.AppliedResult.AppliedAction}"
        };

        // Return the translated result with the applied result attached for formatter traceability.
        return new SecurityTranslatedResult(
            input.ScenarioId,
            input.AppliedResult.Verdict,
            fragment,
            input.AppliedResult,
            input.AppliedResult.SourcePairId,
            input.AppliedResult.RequiredResultSignals,
            input.AppliedResult.ForbiddenResultSignals);
    }

    // Build predicate ids that describe the result translation in the shared logical vocabulary.
    private static string[] BuildPredicateIds(SecurityTranslatedResult translated)
    {
        // REPROCESS translation carries a failed validation forward.
        if (string.Equals(translated.Verdict, "REPROCESS", StringComparison.Ordinal))
            return ["CONVERT_OP", "FAIL", "RETURN"];

        // CONCERN translation carries a branch condition forward.
        if (string.Equals(translated.Verdict, "CONCERN", StringComparison.Ordinal))
            return ["CONVERT_OP", "BRANCH", "RETURN"];

        // PASS translation carries accepted validation forward.
        return ["CONVERT_OP", "VALIDATE_OP", "RETURN"];
    }
}
