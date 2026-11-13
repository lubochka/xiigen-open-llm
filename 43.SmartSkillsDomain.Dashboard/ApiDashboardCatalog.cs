// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SmartSkillsDomain.Dashboard;

using System.IO;
using System.Text.Json;
using AgentInfrastructure;
using SmartSkillsDomain;

public sealed record ApiDashboardCatalogField(
    string FieldName,
    string Description);

public sealed record ApiDashboardCatalogMetadata(
    string HumanName,
    string HumanDescription,
    string ExampleInputText,
    string InputFieldDescription,
    string OutputPreview,
    string Status,
    string SafetyBoundaryNote,
    bool IsPrimaryUserFacingAction);

public sealed record ApiDashboardFixtureExampleRow(
    string ScenarioId,
    string FixtureId,
    string Machine,
    string MethodId,
    string HumanName,
    string ExampleInputText,
    string ExpectedVisibleResult,
    IReadOnlyList<string> ExpectedDebugFields,
    string EvidenceScope,
    string HeldoutSourceFamily,
    bool TrainingAllowed,
    bool EvalExecutionAllowed,
    bool NotExecutedYet,
    bool RawPrivateTextCopied,
    bool ClaimsSemanticQuality,
    bool ClaimsRuntimeExtensionProof);

public sealed record ApiDashboardCatalogRow(
    string MethodId,
    string HumanName,
    string HumanDescription,
    string ExampleInputText,
    string InputFieldDescription,
    IReadOnlyList<ApiDashboardCatalogField> InputFields,
    string OutputPreview,
    IReadOnlyList<ApiDashboardCatalogField> OutputFields,
    string Status,
    string SafetyBoundaryNote,
    bool IsPrimaryUserFacingAction,
    bool ClaimsExternalAi,
    bool ClaimsRuntimeExtensionProof,
    IReadOnlyList<ApiDashboardFixtureExampleRow> FixtureExamples);

public static class ApiCatalogProvider
{
    public static OperationResult<IReadOnlyList<ApiDashboardCatalogRow>> BuildCatalog()
        => BuildCatalog(BuildDefaultMetadata());

    public static OperationResult<IReadOnlyList<ApiDashboardCatalogRow>> BuildCatalog(
        IReadOnlyDictionary<string, ApiDashboardCatalogMetadata> metadataByMethod)
    {
        var contracts = XiigenExtensionApiContract.GetMethodContracts();
        var contractMethods = contracts.Select(contract => contract.Method).ToHashSet(StringComparer.Ordinal);
        var metadataMethods = metadataByMethod.Keys.ToHashSet(StringComparer.Ordinal);
        var unknownMetadata = metadataMethods.Except(contractMethods, StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
        if (unknownMetadata.Length > 0)
        {
            return OperationResult<IReadOnlyList<ApiDashboardCatalogRow>>.Fail(
                $"Dashboard API catalog metadata contains unknown method ids: {string.Join(", ", unknownMetadata)}.",
                "UNKNOWN_API_DASHBOARD_METHOD_METADATA");
        }

        var missingMetadata = contractMethods.Except(metadataMethods, StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
        if (missingMetadata.Length > 0)
        {
            return OperationResult<IReadOnlyList<ApiDashboardCatalogRow>>.Fail(
                $"Dashboard API catalog metadata is missing method ids: {string.Join(", ", missingMetadata)}.",
                "MISSING_API_DASHBOARD_METHOD_METADATA");
        }

        var rows = new List<ApiDashboardCatalogRow>();
        var fixturesByMethod = SemanticQualityFixtureCatalogProvider
            .BuildFixtureExamplesByMethod()
            .Value ?? new Dictionary<string, IReadOnlyList<ApiDashboardFixtureExampleRow>>(StringComparer.Ordinal);
        foreach (var contract in contracts)
        {
            var metadata = metadataByMethod[contract.Method];
            var validation = ValidateMetadata(contract.Method, metadata);
            if (!validation.IsSuccess)
                return validation.MapError<IReadOnlyList<ApiDashboardCatalogRow>>();

            rows.Add(new ApiDashboardCatalogRow(
                MethodId: contract.Method,
                HumanName: metadata.HumanName,
                HumanDescription: metadata.HumanDescription,
                ExampleInputText: metadata.ExampleInputText,
                InputFieldDescription: metadata.InputFieldDescription,
                InputFields: BuildFields(contract.AllowedRequestFields),
                OutputPreview: metadata.OutputPreview,
                OutputFields: BuildFields(contract.AllowedResponseFields),
                Status: metadata.Status,
                SafetyBoundaryNote: BuildSafetyBoundaryNote(metadata, contract),
                IsPrimaryUserFacingAction: metadata.IsPrimaryUserFacingAction,
                ClaimsExternalAi: false,
                ClaimsRuntimeExtensionProof: false,
                FixtureExamples: fixturesByMethod.TryGetValue(contract.Method, out var fixtures)
                    ? fixtures
                    : []));
        }

        return OperationResult<IReadOnlyList<ApiDashboardCatalogRow>>.Ok(rows);
    }

    public static IReadOnlyDictionary<string, ApiDashboardCatalogMetadata> BuildDefaultMetadata()
        => new Dictionary<string, ApiDashboardCatalogMetadata>(StringComparer.Ordinal)
        {
            [XiigenExtensionApiContract.PromptToTaskMethod] = new(
                HumanName: "Turn a user prompt into a structured task",
                HumanDescription: "Use this when a host has raw user text and needs xiigen core to identify the concrete task without exposing model internals.",
                ExampleInputText: "Please turn this extension request into a bounded implementation task with constraints and expected evidence.",
                InputFieldDescription: "The dashboard sends host facts, provenance, correlation id, and the user prompt text through the thin extension request contract.",
                OutputPreview: "Structured task references, check-set results, warnings, and a human chat summary.",
                Status: "primary user-facing core action",
                SafetyBoundaryNote: "Core boundary only; no external AI call, raw corpus row, host-side training logic, or host runtime proof is claimed.",
                IsPrimaryUserFacingAction: true),
            [XiigenExtensionApiContract.OrchestrationPlanMethod] = new(
                HumanName: "Find an orchestration plan for a structured task",
                HumanDescription: "Use this after a prompt has been translated into a task and the host needs scenario references and orchestration check results.",
                ExampleInputText: "Given structured-task:demo-001, choose the orchestration scenario and evidence checks for this bounded work order.",
                InputFieldDescription: "The dashboard sends the selected method, host facts, provenance, correlation id, and structured task references through check-set inputs.",
                OutputPreview: "Scenario references, check-set results, warnings, errors, and a human orchestration summary.",
                Status: "primary user-facing core action",
                SafetyBoundaryNote: "Dashboard evidence proves only dashboard-to-core invocation; host runtime and semantic quality remain separately gated.",
                IsPrimaryUserFacingAction: true),
            [XiigenExtensionApiContract.SkillSelectionMethod] = new(
                HumanName: "Select relevant skills for a task",
                HumanDescription: "Use this when a host has available skill ids and needs xiigen core to select the relevant set for the task.",
                ExampleInputText: "Available skills: plan-review, code-examination, test-integrity. Select what this dashboard work order should load.",
                InputFieldDescription: "The dashboard sends available skills, requested skills, host facts, provenance, correlation id, and prompt hash/check-set inputs.",
                OutputPreview: "Selected skills, check-set results, warnings, errors, and a human explanation of why those skills were chosen.",
                Status: "primary user-facing core action",
                SafetyBoundaryNote: "Skill selection may report low confidence or second-pass requirements; it must not expose model internals or private prompt text.",
                IsPrimaryUserFacingAction: true),
            [XiigenExtensionApiContract.FeedbackMethod] = new(
                HumanName: "Record correction feedback for local learning",
                HumanDescription: "Use this for refs-only feedback after the host has a chosen and rejected answer reference; it is not a primary runtime command.",
                ExampleInputText: "Feedback refs: chosen_ref=answer-good-001, rejected_ref=answer-bad-001, prompt_sha256=<hash>, feedback_class=missing-gate.",
                InputFieldDescription: "The dashboard sends feedback class and references only; raw private correction text and common model mutation are forbidden.",
                OutputPreview: "Feedback acknowledgement, check-set results, warnings, and errors.",
                Status: "non-primary feedback signal path",
                SafetyBoundaryNote: "Feedback routes refs only to user-local adaptive signal creation and must not mutate common models.",
                IsPrimaryUserFacingAction: false),
            [XiigenExtensionApiContract.DebugTraceMethod] = new(
                HumanName: "Show redacted xiigen input and output trace",
                HumanDescription: "Use this only when programmer debug is enabled and the trace is redacted; it is not semantic runtime proof.",
                ExampleInputText: "Debug trace request with redacted_input_payload and redacted_output_payload for the current correlation id.",
                InputFieldDescription: "The dashboard sends debug enabled, correlation id, and redacted payload fields; raw private content and secrets suppress the trace.",
                OutputPreview: "Chat-formatted xiigen method trace with input and output lines, or no trace when unsafe flags are present.",
                Status: "non-primary debug trace path",
                SafetyBoundaryNote: "Debug output is redacted display evidence only; it does not prove external host runtime or model quality.",
                IsPrimaryUserFacingAction: false)
        };

    private static OperationResult<bool> ValidateMetadata(
        string methodId,
        ApiDashboardCatalogMetadata metadata)
    {
        var missing = new List<string>();
        if (string.IsNullOrWhiteSpace(metadata.HumanName))
            missing.Add(nameof(metadata.HumanName));
        if (string.IsNullOrWhiteSpace(metadata.HumanDescription))
            missing.Add(nameof(metadata.HumanDescription));
        if (string.IsNullOrWhiteSpace(metadata.ExampleInputText))
            missing.Add(nameof(metadata.ExampleInputText));
        if (string.IsNullOrWhiteSpace(metadata.InputFieldDescription))
            missing.Add(nameof(metadata.InputFieldDescription));
        if (string.IsNullOrWhiteSpace(metadata.OutputPreview))
            missing.Add(nameof(metadata.OutputPreview));
        if (string.IsNullOrWhiteSpace(metadata.Status))
            missing.Add(nameof(metadata.Status));
        if (string.IsNullOrWhiteSpace(metadata.SafetyBoundaryNote))
            missing.Add(nameof(metadata.SafetyBoundaryNote));

        return missing.Count == 0
            ? OperationResult<bool>.Ok(true)
            : OperationResult<bool>.Fail(
                $"Dashboard API catalog metadata for method '{methodId}' is missing: {string.Join(", ", missing)}.",
                "INCOMPLETE_API_DASHBOARD_METHOD_METADATA");
    }

    private static IReadOnlyList<ApiDashboardCatalogField> BuildFields(IReadOnlyList<string> fields)
        => fields
            .Select(field => new ApiDashboardCatalogField(field, DescribeField(field)))
            .ToArray();

    private static string BuildSafetyBoundaryNote(
        ApiDashboardCatalogMetadata metadata,
        XiigenExtensionApiMethodContract contract)
        => string.Join(
            " ",
            metadata.SafetyBoundaryNote,
            $"Forbidden payloads: {string.Join(", ", contract.ForbiddenPayloads)}.",
            $"Boundary rules: {string.Join(", ", contract.BoundaryRules)}.");

    private static string DescribeField(string field)
        => field switch
        {
            nameof(XiigenExtensionRequest.HostName) => "Name of the calling host such as Claude, Cursor, Codex, or dashboard.",
            nameof(XiigenExtensionRequest.HostType) => "Host client category used for boundary checks and display.",
            nameof(XiigenExtensionRequest.Method) => "Xiigen extension method id selected from the real core contract.",
            nameof(XiigenExtensionRequest.UserPromptOrTaskText) => "User prompt or task text; core responses must not echo raw private prompt text in unsafe outputs.",
            nameof(XiigenExtensionRequest.HostFacts) => "Small host facts such as workspace and client flags.",
            nameof(XiigenExtensionRequest.DebugEnabled) => "Programmer debug switch for redacted xiigen input/output trace display.",
            nameof(XiigenExtensionRequest.RequestedSkills) => "Optional skill ids explicitly requested by the host or user.",
            nameof(XiigenExtensionRequest.AvailableSkills) => "Skill ids visible to the host for choosing relevant skills.",
            nameof(XiigenExtensionRequest.CheckSetInputs) => "Named inputs and refs used by core check sets.",
            nameof(XiigenExtensionRequest.Provenance) => "Source refs and ledger refs; raw private content should stay out of this field.",
            nameof(XiigenExtensionRequest.CorrelationId) => "Stable request id used to connect dashboard input, output, and debug trace.",
            nameof(XiigenExtensionResponse.Success) => "Whether the core boundary returned a successful response envelope.",
            nameof(XiigenExtensionResponse.Status) => "Core boundary status such as partial, invalid request, or error.",
            nameof(XiigenExtensionResponse.ChatDisplayText) => "Human-facing response text safe for the host chat.",
            nameof(XiigenExtensionResponse.SelectedSkills) => "Skill ids selected by core logic.",
            nameof(XiigenExtensionResponse.CheckSetResults) => "Named evidence fields returned by the core boundary.",
            nameof(XiigenExtensionResponse.StructuredTaskRefs) => "References to structured task outputs, not raw task internals.",
            nameof(XiigenExtensionResponse.ScenarioRefs) => "References to orchestration scenarios selected by core logic.",
            nameof(XiigenExtensionResponse.Warnings) => "Boundary warnings, including semantic-quality and host-runtime disclaimers.",
            nameof(XiigenExtensionResponse.Errors) => "Validation or core boundary error messages.",
            nameof(XiigenExtensionResponse.DebugTrace) => "Optional redacted debug payload shown only when safe and requested.",
            "structured_task_ref" => "Reference to an existing structured task produced from the user prompt.",
            "prompt_sha256" => "Hash of the prompt, used instead of raw private prompt text.",
            "feedback_class" => "Feedback category for user-local adaptive signal creation.",
            "chosen_ref" => "Reference to the accepted answer or plan.",
            "rejected_ref" => "Reference to the rejected answer or plan.",
            "response_ref" => "Reference to the response that feedback is about.",
            "correction_ref" => "Reference to correction text stored outside the common model payload.",
            "redacted_input_payload" => "Already-redacted input payload for debug display.",
            "redacted_output_payload" => "Already-redacted output payload for debug display.",
            _ => $"Contract field '{field}' from XiigenExtensionApiContract."
        };
}

public static class SemanticQualityFixtureCatalogProvider
{
    public const string FixtureEvidenceScope = "fixture-design/test-only preview; not model eval, not trained semantic quality, not extension host runtime";
    private const string FixtureRowsPath = "artifacts/smart-skills-semantic-quality-completion/p36-eval-fixture-rows/p36-eval-fixtures.jsonl";
    private const string DashboardScenariosPath = "artifacts/smart-skills-semantic-quality-completion/p36-eval-fixture-rows/p36-dashboard-api-scenarios.jsonl";

    public static OperationResult<IReadOnlyDictionary<string, IReadOnlyList<ApiDashboardFixtureExampleRow>>> BuildFixtureExamplesByMethod(
        string? workspaceRoot = null)
    {
        var rows = BuildFixtureExamples(workspaceRoot);
        if (!rows.IsSuccess || rows.Value is null)
            return rows.MapError<IReadOnlyDictionary<string, IReadOnlyList<ApiDashboardFixtureExampleRow>>>();

        var byMethod = rows.Value
            .GroupBy(row => row.MethodId, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<ApiDashboardFixtureExampleRow>)group
                    .OrderBy(row => row.ScenarioId, StringComparer.Ordinal)
                    .ToArray(),
                StringComparer.Ordinal);

        return OperationResult<IReadOnlyDictionary<string, IReadOnlyList<ApiDashboardFixtureExampleRow>>>.Ok(byMethod);
    }

    public static OperationResult<IReadOnlyList<ApiDashboardFixtureExampleRow>> BuildFixtureExamples(
        string? workspaceRoot = null)
    {
        var root = ResolveWorkspaceRoot(workspaceRoot);
        var fixturePath = Path.Combine(root, FixtureRowsPath);
        var scenarioPath = Path.Combine(root, DashboardScenariosPath);
        if (!File.Exists(fixturePath) || !File.Exists(scenarioPath))
            return OperationResult<IReadOnlyList<ApiDashboardFixtureExampleRow>>.Ok([]);

        try
        {
            var fixtures = ReadJsonLines(fixturePath)
                .ToDictionary(
                    row => RequireString(row, "fixture_id"),
                    row => row,
                    StringComparer.Ordinal);
            var examples = new List<ApiDashboardFixtureExampleRow>();
            foreach (var scenario in ReadJsonLines(scenarioPath))
            {
                var fixtureId = RequireString(scenario, "fixture_id");
                if (!fixtures.TryGetValue(fixtureId, out var fixture))
                {
                    return OperationResult<IReadOnlyList<ApiDashboardFixtureExampleRow>>.Fail(
                        $"P36 dashboard scenario '{RequireString(scenario, "scenario_id")}' references missing fixture '{fixtureId}'.",
                        "SEMANTIC_FIXTURE_SCENARIO_MISSING_FIXTURE");
                }

                examples.Add(BuildExample(scenario, fixture));
            }

            return OperationResult<IReadOnlyList<ApiDashboardFixtureExampleRow>>.Ok(examples);
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or IOException)
        {
            return OperationResult<IReadOnlyList<ApiDashboardFixtureExampleRow>>.Fail(
                $"Semantic-quality fixture catalog could not read P36/P37 rows: {ex.Message}",
                "SEMANTIC_FIXTURE_CATALOG_READ_FAILED");
        }
    }

    private static ApiDashboardFixtureExampleRow BuildExample(
        JsonElement scenario,
        JsonElement fixture)
    {
        var machine = RequireString(fixture, "machine");
        var methodId = MapMachineToMethod(machine);
        var sourceFamily = RequireString(fixture, "heldout_source_family");
        return new ApiDashboardFixtureExampleRow(
            ScenarioId: RequireString(scenario, "scenario_id"),
            FixtureId: RequireString(scenario, "fixture_id"),
            Machine: machine,
            MethodId: methodId,
            HumanName: BuildHumanName(machine, sourceFamily),
            ExampleInputText: RequireString(scenario, "input_field_example"),
            ExpectedVisibleResult: BuildExpectedVisibleResult(fixture),
            ExpectedDebugFields: ReadStringArray(scenario, "expected_debug_fields"),
            EvidenceScope: RequireString(scenario, "evidence_scope"),
            HeldoutSourceFamily: sourceFamily,
            TrainingAllowed: ReadBool(fixture, "training_allowed"),
            EvalExecutionAllowed: ReadBool(fixture, "eval_execution_allowed"),
            NotExecutedYet: ReadBool(fixture, "not_executed_yet"),
            RawPrivateTextCopied: ReadBool(fixture, "raw_private_text_copied"),
            ClaimsSemanticQuality: false,
            ClaimsRuntimeExtensionProof: false);
    }

    private static string BuildExpectedVisibleResult(JsonElement fixture)
    {
        var slots = fixture.GetProperty("expected_output_slots");
        var parts = new List<string>();
        foreach (var property in slots.EnumerateObject())
        {
            var value = property.Value.ValueKind == JsonValueKind.Array
                ? string.Join("; ", property.Value.EnumerateArray().Select(item => item.GetString() ?? string.Empty))
                : property.Value.ToString();
            parts.Add($"{HumanizeToken(property.Name)}: {value}");
        }

        return $"Fixture preview only. Expected fields: {string.Join(" | ", parts)}. This row is not executed yet and does not prove semantic quality.";
    }

    private static string BuildHumanName(
        string machine,
        string sourceFamily)
    {
        var topic = sourceFamily
            .Replace("p36_eval_", string.Empty, StringComparison.Ordinal)
            .Replace(machine.ToLowerInvariant(), string.Empty, StringComparison.Ordinal)
            .Trim('_');
        return $"Fixture preview: {machine} - {HumanizeToken(topic)}";
    }

    private static string MapMachineToMethod(string machine)
        => machine switch
        {
            "PromptToTask" => XiigenExtensionApiContract.PromptToTaskMethod,
            "OrchestrationReasoning" => XiigenExtensionApiContract.OrchestrationPlanMethod,
            "SkillSelection" => XiigenExtensionApiContract.SkillSelectionMethod,
            _ => throw new InvalidOperationException($"Unknown semantic-quality fixture machine '{machine}'.")
        };

    private static IReadOnlyList<JsonElement> ReadJsonLines(string path)
        => File.ReadLines(path)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonDocument.Parse(line).RootElement.Clone())
            .ToArray();

    private static string RequireString(
        JsonElement row,
        string propertyName)
        => row.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : throw new InvalidOperationException($"Fixture row is missing string property '{propertyName}'.");

    private static bool ReadBool(
        JsonElement row,
        string propertyName)
        => row.TryGetProperty(propertyName, out var value)
            && value.ValueKind == JsonValueKind.True;

    private static IReadOnlyList<string> ReadStringArray(
        JsonElement row,
        string propertyName)
        => row.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Array
            ? value.EnumerateArray().Select(item => item.GetString() ?? string.Empty).ToArray()
            : [];

    private static string HumanizeToken(string value)
        => string.Join(
            " ",
            value.Split('_', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

    private static string ResolveWorkspaceRoot(string? workspaceRoot)
    {
        if (!string.IsNullOrWhiteSpace(workspaceRoot))
            return workspaceRoot;

        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "43.SmartSkillsDomain.Dashboard", "ApiDashboardCatalog.cs")))
                return current.FullName;

            current = current.Parent;
        }

        return Directory.GetCurrentDirectory();
    }
}
