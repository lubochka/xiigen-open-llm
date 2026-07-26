// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

public sealed class NegativeDtoDocumentationOrchestrator
{
    public IReadOnlyList<NegativeDtoDocumentationCandidate> GenerateCandidates(
        IReadOnlyList<ProjectDocumentationUnit> units)
        => units.Select((unit, index) => BuildCandidate(unit, index + 1)).ToArray();

    public IReadOnlyList<NegativeDtoDocumentationValidation> Validate(
        IReadOnlyList<NegativeDtoDocumentationCandidate> candidates)
        => candidates.Select(ValidateCandidate).ToArray();

    public IReadOnlyList<ProjectDocumentationDpoPair> BuildDpoPairs(
        IReadOnlyList<NegativeDtoDocumentationCandidate> candidates,
        IReadOnlyList<NegativeDtoDocumentationValidation> validations)
    {
        var accepted = validations
            .Where(validation => validation.IsAccepted)
            .Select(validation => validation.CandidateId)
            .ToHashSet(StringComparer.Ordinal);

        return candidates
            .Where(candidate => accepted.Contains(candidate.CandidateId))
            .Select(candidate => new ProjectDocumentationDpoPair(
                $"DPO-{candidate.CandidateId}",
                candidate.DocumentationUnitId,
                $"Document {candidate.DocumentationUnitId} bottom-up with location, meaning, parameters, return, callers, callees, and evidence.",
                candidate.ChosenDocumentation,
                candidate.RejectedDocumentation,
                candidate.NegativeType,
                candidate.EvidenceIds))
            .ToArray();
    }

    public NegativeDtoDocumentationBundle BuildBundle(IReadOnlyList<ProjectDocumentationUnit> units)
    {
        var candidates = GenerateCandidates(units);
        var validations = Validate(candidates);
        var pairs = BuildDpoPairs(candidates, validations);

        return new NegativeDtoDocumentationBundle(candidates, validations, pairs);
    }

    private NegativeDtoDocumentationCandidate BuildCandidate(ProjectDocumentationUnit unit, int index)
    {
        var chosen = BuildChosenDocumentation(unit);
        var negativeType = ChooseNegativeType(unit);
        var rejected = BuildRejectedDocumentation(unit, negativeType);

        return new NegativeDtoDocumentationCandidate(
            $"NEG-DOC-{index:000000}",
            unit.DocumentationUnitId,
            negativeType,
            chosen,
            rejected,
            unit.EvidenceIds);
    }

    private static string BuildChosenDocumentation(ProjectDocumentationUnit unit)
        => string.Join(" ", [
            "documentation_verdict: complete_unit_documentation.",
            "accepted_contracts: location, parameters, return, caller_path, callee_path, evidence all match code_trace.",
            $"{unit.SymbolName} is located at {unit.SourcePath}:{unit.SourceLine}.",
            "location_contract: source_path_and_line_present.",
            $"Meaning: {unit.Meaning}",
            $"parameter_contract: {ContractValue(unit.Parameters, "actual_parameters")}.",
            $"parameter_verification_status: {ParameterVerificationStatus(unit)}.",
            $"return_contract: {ContractValue(unit.ReturnValue, "actual_return")}.",
            $"caller_context_contract: {ContractValue(unit.Callers, "actual_callers")}.",
            $"caller_path_status: {CallerPathStatus(unit)}.",
            $"callee_transit_contract: {ContractValue(unit.Callees, "actual_callees")}.",
            $"dependency_path_status: {DependencyPathStatus(unit)}.",
            $"Evidence: {Format(unit.EvidenceIds)}."
        ]);

    private static string BuildRejectedDocumentation(ProjectDocumentationUnit unit, string negativeType)
        => negativeType switch
        {
            "missing_location" =>
                string.Join(" ", [
                    "documentation_verdict: incomplete_unit_documentation.",
                    "rejected_defect_type: missing_location.",
                    "affected_contract: source_location.",
                    $"{unit.SymbolName} is located at GeneratedDocumentation/WikiGuess.cs:1.",
                    "location_contract: source_path_and_line_unverified_placeholder.",
                    "location_verification_status: wrong_source_location_not_from_code_trace.",
                    $"Meaning: {unit.Meaning}",
                    $"parameter_contract: {ContractValue(unit.Parameters, "actual_parameters")}.",
                    $"return_contract: {ContractValue(unit.ReturnValue, "actual_return")}.",
                    $"caller_context_contract: {ContractValue(unit.Callers, "actual_callers")}.",
                    $"callee_transit_contract: {ContractValue(unit.Callees, "actual_callees")}.",
                    "[WRONG: missing_location - this replaces the real source path and line with an unverified placeholder, so a local wiki answer cannot cite or verify the documentation evidence.]"
                ]),
            "missing_parameters" =>
                string.Join(" ", [
                    "documentation_verdict: incomplete_unit_documentation.",
                    "rejected_defect_type: missing_parameters.",
                    "affected_contract: input_parameters.",
                    $"{unit.SymbolName} is located at {unit.SourcePath}:{unit.SourceLine}.",
                    "location_contract: source_path_and_line_present.",
                    $"Meaning: {unit.Meaning}",
                    "parameter_contract: wrong_or_guessed_parameters: requestId:string, userToken:string.",
                    "parameter_verification_status: actual_input_values_missing_from_code_trace.",
                    $"return_contract: {ContractValue(unit.ReturnValue, "actual_return")}.",
                    $"caller_context_contract: {ContractValue(unit.Callers, "actual_callers")}.",
                    $"callee_transit_contract: {ContractValue(unit.Callees, "actual_callees")}.",
                    "[WRONG: missing_parameters - this substitutes guessed parameters for the real input contract, so callers cannot learn what values actually enter the function.]"
                ]),
            "missing_return_contract" =>
                string.Join(" ", [
                    "documentation_verdict: incomplete_unit_documentation.",
                    "rejected_defect_type: missing_return_contract.",
                    "affected_contract: output_return_value.",
                    $"{unit.SymbolName} is located at {unit.SourcePath}:{unit.SourceLine}.",
                    "location_contract: source_path_and_line_present.",
                    $"Meaning: {unit.Meaning}",
                    $"parameter_contract: {ContractValue(unit.Parameters, "actual_parameters")}.",
                    "return_contract: wrong_or_guessed_return: bool success.",
                    "return_verification_status: actual_output_value_missing_from_code_trace.",
                    $"caller_context_contract: {ContractValue(unit.Callers, "actual_callers")}.",
                    $"callee_transit_contract: {ContractValue(unit.Callees, "actual_callees")}.",
                    "[WRONG: missing_return_contract - this substitutes a guessed return value for the real output contract, so caller-level documentation cannot propagate what the function actually produces.]"
                ]),
            "missing_callee_transit" =>
                string.Join(" ", [
                    "documentation_verdict: incomplete_unit_documentation.",
                    "rejected_defect_type: missing_callee_transit.",
                    "affected_contract: downstream_callee_path.",
                    $"{unit.SymbolName} is located at {unit.SourcePath}:{unit.SourceLine}.",
                    "location_contract: source_path_and_line_present.",
                    $"Meaning: {unit.Meaning}",
                    $"parameter_contract: {ContractValue(unit.Parameters, "actual_parameters")}.",
                    $"return_contract: {ContractValue(unit.ReturnValue, "actual_return")}.",
                    $"caller_context_contract: {ContractValue(unit.Callers, "actual_callers")}.",
                    "callee_transit_contract: wrong_dependency_path: CachePolicy.Resolve, AuditWriter.Append.",
                    "dependency_verification_status: actual_callees_missing_from_code_trace.",
                    "[WRONG: missing_callee_transit - this substitutes unsupported dependencies for the real callee path, so caller-level documentation cannot propagate dependency meaning.]"
                ]),
            "missing_caller_context" =>
                string.Join(" ", [
                    "documentation_verdict: incomplete_unit_documentation.",
                    "rejected_defect_type: missing_caller_context.",
                    "affected_contract: upstream_caller_path.",
                    $"{unit.SymbolName} is located at {unit.SourcePath}:{unit.SourceLine}.",
                    "location_contract: source_path_and_line_present.",
                    $"Meaning: {unit.Meaning}",
                    $"parameter_contract: {ContractValue(unit.Parameters, "actual_parameters")}.",
                    $"return_contract: {ContractValue(unit.ReturnValue, "actual_return")}.",
                    "caller_context_contract: wrong_callers: NightlyPermissionSync.Run, BackgroundAccessJob.Execute.",
                    "caller_path_status: caller_path_wrong.",
                    $"callee_transit_contract: {ContractValue(unit.Callees, "actual_callees")}.",
                    "[WRONG: missing_caller_context - this replaces the actual upstream controller workflow with an unsupported background workflow, so the wiki would route impact analysis through the wrong caller path.]"
                ]),
            _ =>
                $"{unit.SymbolName} has behavior worth documenting. [WRONG: incomplete_documentation - the rejected side omits location, parameters, return, callers, callees, and evidence.]"
        };

    private static NegativeDtoDocumentationValidation ValidateCandidate(NegativeDtoDocumentationCandidate candidate)
    {
        var errors = new List<string>();
        if (string.Equals(candidate.ChosenDocumentation.Trim(), candidate.RejectedDocumentation.Trim(), StringComparison.Ordinal))
            errors.Add("chosen_equals_rejected");

        if (!candidate.RejectedDocumentation.Contains("[WRONG:", StringComparison.Ordinal))
            errors.Add("missing_wrong_annotation");

        if (candidate.EvidenceIds.Length == 0)
            errors.Add("missing_evidence_ids");

        if (string.IsNullOrWhiteSpace(candidate.DocumentationUnitId))
            errors.Add("missing_documentation_unit_id");

        return new NegativeDtoDocumentationValidation(
            candidate.CandidateId,
            candidate.DocumentationUnitId,
            candidate.NegativeType,
            errors.Count == 0,
            errors.ToArray());
    }

    private string ChooseNegativeType(ProjectDocumentationUnit unit)
    {
        if (unit.SymbolName.Contains("ResolveOrder", StringComparison.OrdinalIgnoreCase))
            return "missing_callee_transit";

        if (unit.SymbolName.Contains("CanEditEntity", StringComparison.OrdinalIgnoreCase))
            return "missing_caller_context";

        if (unit.SymbolName.Contains("MapRequest", StringComparison.OrdinalIgnoreCase))
            return "missing_parameters";

        if (unit.SymbolName.Contains("MapEntityUpdate", StringComparison.OrdinalIgnoreCase))
            return "missing_parameters";

        if (unit.SymbolName.Contains("BuildSearchQuery", StringComparison.OrdinalIgnoreCase))
            return "missing_parameters";

        if (unit.SymbolName.Contains("CreateReportFilter", StringComparison.OrdinalIgnoreCase))
            return "missing_parameters";

        if (unit.SymbolName.Contains("CreateCommand", StringComparison.OrdinalIgnoreCase))
            return "missing_parameters";

        if (unit.SymbolName.Contains("CalculateTotal", StringComparison.OrdinalIgnoreCase))
            return "missing_return_contract";

        if (unit.SymbolName.Contains("Normalize", StringComparison.OrdinalIgnoreCase))
            return "missing_location";

        if (unit.Callees.Length > 0)
            return "missing_callee_transit";

        if (unit.Callers.Length > 0)
            return "missing_caller_context";

        if (unit.Parameters.Length > 0)
            return "missing_parameters";

        if (!string.IsNullOrWhiteSpace(unit.ReturnValue))
            return "missing_return_contract";

        return "missing_location";
    }

    private static string Format(string[] values)
        => values.Length == 0 ? "none recorded" : string.Join(", ", values);

    private static string ContractValue(string[] values, string label)
        => values.Length == 0 ? "none recorded" : $"{label}: {string.Join(", ", values)}";

    private static string ContractValue(string value, string label)
        => string.IsNullOrWhiteSpace(value) ? "none recorded" : $"{label}: {value}";

    private static string DependencyPathStatus(ProjectDocumentationUnit unit)
        => unit.Callees.Length == 0 ? "no_lower_level_dependency" : "dependency_path_complete";

    private static string CallerPathStatus(ProjectDocumentationUnit unit)
        => unit.Callers.Length == 0 ? "no_upstream_workflow" : "caller_path_complete";

    private static string ParameterVerificationStatus(ProjectDocumentationUnit unit)
        => unit.Parameters.Length == 0 ? "no_input_parameters" : "actual_input_values_present_in_code_trace";
}

public sealed record NegativeDtoDocumentationBundle(
    IReadOnlyList<NegativeDtoDocumentationCandidate> Candidates,
    IReadOnlyList<NegativeDtoDocumentationValidation> Validations,
    IReadOnlyList<ProjectDocumentationDpoPair> DpoPairs);
