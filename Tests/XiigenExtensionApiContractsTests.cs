// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using AgentInfrastructure;
using SmartSkillsDomain;

[Collection("ProcessSpawningTests")]
public sealed class XiigenExtensionApiContractsTests
{
    private static readonly TimeSpan ProcessTimeout = TimeSpan.FromSeconds(60);

    [Fact]
    public void XiigenExtensionRequest_PreservesHostMethodFactsDebugAndCorrelation()
    {
        var request = CreateRequest(debugEnabled: true);

        Assert.Equal("codex", request.HostName);
        Assert.Equal("cli-agent", request.HostType);
        Assert.Equal("plan.review", request.Method);
        Assert.Equal("corr-001", request.CorrelationId);
        Assert.True(request.DebugEnabled);
        Assert.Equal("workspace:C:\\Projects\\llm_mvp_core", request.HostFacts["workspace"]);
        Assert.Contains("plan-review", request.RequestedSkills);
        Assert.Contains("orchestration", request.AvailableSkills);
        Assert.Equal("bounded", request.CheckSetInputs["scope"]);
        Assert.Equal("conversation:current", request.Provenance["source"]);
    }

    [Fact]
    public void XiigenExtensionResponse_CarriesThinClientResultRefsWithoutHostLogic()
    {
        var response = new XiigenExtensionResponse(
            Success: true,
            Status: "contract_only",
            ChatDisplayText: "Use WO-API-01 only.",
            SelectedSkills: ["plan-review", "plan-self-validation"],
            CheckSetResults: new Dictionary<string, string>
            {
                ["bounded_work_order"] = "pass",
                ["external_ai"] = "not_used"
            },
            StructuredTaskRefs: ["structured-task:prompt-001"],
            ScenarioRefs: ["scenario:orch-001"],
            Warnings: ["parent review required"],
            Errors: [],
            DebugTrace: null);

        Assert.True(response.Success);
        Assert.Equal("contract_only", response.Status);
        Assert.Contains("plan-review", response.SelectedSkills);
        Assert.Equal("pass", response.CheckSetResults["bounded_work_order"]);
        Assert.Equal(["structured-task:prompt-001"], response.StructuredTaskRefs);
        Assert.Equal(["scenario:orch-001"], response.ScenarioRefs);
        Assert.Contains("parent review required", response.Warnings);
        Assert.Empty(response.Errors);
    }

    [Fact]
    public void DebugDisabled_OmitsDebugTraceFromChatDisplay()
    {
        var trace = new XiigenDebugTrace(
            Method: "plan.review",
            InputPayloadText: "{redacted-input}",
            OutputPayloadText: "{redacted-output}");

        var text = XiigenExtensionApiContract.BuildChatDisplayText(
            chatDisplayText: "Review complete.",
            debugEnabled: false,
            debugTrace: trace);

        Assert.Equal("Review complete.", text);
        Assert.DoesNotContain("xiigen plan.review :", text);
        Assert.DoesNotContain("input:", text);
        Assert.DoesNotContain("output:", text);
    }

    [Fact]
    public void DebugEnabled_FormatsExactXiigenMethodInputOutputChatTrace()
    {
        var trace = new XiigenDebugTrace(
            Method: "plan.review",
            InputPayloadText: "{redacted-input}",
            OutputPayloadText: "{redacted-output}");

        var text = XiigenExtensionApiContract.FormatDebugTraceForChat(
            debugEnabled: true,
            debugTrace: trace);
        var expected = string.Join(
            Environment.NewLine,
            "xiigen plan.review :",
            "input: {redacted-input}",
            "output: {redacted-output}");

        Assert.Equal(expected, text);
    }

    [Fact]
    public void RequestValidation_MarksHostlessAndMethodlessRequestsInvalid()
    {
        var request = CreateRequest(debugEnabled: false) with
        {
            HostName = "",
            Method = " "
        };

        var result = XiigenExtensionApiContract.ValidateRequest(request);

        Assert.False(result.IsValid);
        Assert.Contains("host_name_required", result.Errors);
        Assert.Contains("method_required", result.Errors);
    }

    [Fact]
    public void DebugTrace_DoesNotRenderPrivateOrSecretPayloadsByDefault()
    {
        var trace = new XiigenDebugTrace(
            Method: "plan.review",
            InputPayloadText: "raw-private-corpus-fragment",
            OutputPayloadText: "secret-output",
            ContainsRawPrivateContent: true,
            ContainsSecrets: true);

        var text = XiigenExtensionApiContract.FormatDebugTraceForChat(
            debugEnabled: true,
            debugTrace: trace);

        Assert.Equal(string.Empty, text);
    }

    [Fact]
    public void ContractTypes_DoNotExposeHostRuntimeExternalAiOrTrainingExecution()
    {
        var contractTypeNames = typeof(XiigenExtensionRequest).Assembly.GetTypes()
            .Where(type => type.Namespace == "SmartSkillsDomain"
                && type.Name.Contains("XiigenExtension", StringComparison.Ordinal))
            .Select(type => type.Name)
            .ToArray();

        Assert.Contains(nameof(XiigenExtensionRequest), contractTypeNames);
        Assert.Contains(nameof(XiigenExtensionResponse), contractTypeNames);
        Assert.DoesNotContain(contractTypeNames, name => name.Contains("Trainer", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(contractTypeNames, name => name.Contains("Runtime", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(contractTypeNames, name => name.Contains("ExternalAi", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(contractTypeNames, name => name.Contains("HostRepo", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void XiigenExtensionApiContract_DefinesExactlyFiveMethodContracts()
    {
        var contracts = XiigenExtensionApiContract.GetMethodContracts();

        Assert.Equal(
            [
                XiigenExtensionApiContract.PromptToTaskMethod,
                XiigenExtensionApiContract.OrchestrationPlanMethod,
                XiigenExtensionApiContract.SkillSelectionMethod,
                XiigenExtensionApiContract.FeedbackMethod,
                XiigenExtensionApiContract.DebugTraceMethod
            ],
            contracts.Select(contract => contract.Method).ToArray());
        Assert.All(contracts, contract =>
        {
            Assert.NotEmpty(contract.AllowedRequestFields);
            Assert.NotEmpty(contract.AllowedResponseFields);
            Assert.Contains("model_internals", contract.ForbiddenPayloads);
            Assert.Contains("raw_corpus_data", contract.ForbiddenPayloads);
            Assert.Contains("host_side_training_logic", contract.ForbiddenPayloads);
            Assert.Contains("external_ai_call", contract.ForbiddenPayloads);
            Assert.Contains("host_runtime_claim", contract.ForbiddenPayloads);
            Assert.Contains("no_model_internals_or_corpus_rows_in_response", contract.BoundaryRules);
        });
    }

    [Fact]
    public void XiigenExtensionApiContract_FeedbackContractIsRefsOnlyAndUserLocal()
    {
        var contract = XiigenExtensionApiContract.FindMethodContract(XiigenExtensionApiContract.FeedbackMethod);

        Assert.NotNull(contract);
        Assert.Contains("chosen_ref", contract!.AllowedRequestFields);
        Assert.Contains("rejected_ref", contract.AllowedRequestFields);
        Assert.Contains("prompt_sha256", contract.AllowedRequestFields);
        Assert.Contains("raw_private_correction_text", contract.ForbiddenPayloads);
        Assert.Contains("common_model_mutation", contract.ForbiddenPayloads);
        Assert.Contains("feedback_routes_refs_only_to_user_local_adaptive_dpo_signal_creation", contract.BoundaryRules);
        Assert.Contains("feedback_must_not_mutate_common_models", contract.BoundaryRules);
        Assert.DoesNotContain("UserPromptOrTaskText", contract.AllowedRequestFields);
    }

    [Fact]
    public void XiigenExtensionApiContract_DebugContractPreservesExactRedactedChatFormat()
    {
        var contract = XiigenExtensionApiContract.FindMethodContract(XiigenExtensionApiContract.DebugTraceMethod);
        var trace = new XiigenDebugTrace(
            Method: XiigenExtensionApiContract.DebugTraceMethod,
            InputPayloadText: "{redacted-input}",
            OutputPayloadText: "{redacted-output}");

        var text = XiigenExtensionApiContract.FormatDebugTraceForChat(true, trace);

        Assert.NotNull(contract);
        Assert.Contains("debug_trace_suppressed_if_raw_private_content_or_secrets_true", contract!.BoundaryRules);
        Assert.Contains("chat_format_exact:xiigen <method> :\\ninput: <redacted>\\noutput: <redacted>", contract.BoundaryRules);
        Assert.Equal(
            string.Join(
                Environment.NewLine,
                "xiigen debug-trace :",
                "input: {redacted-input}",
                "output: {redacted-output}"),
            text);
        Assert.Equal(
            string.Empty,
            XiigenExtensionApiContract.FormatDebugTraceForChat(
                true,
                trace with { ContainsRawPrivateContent = true }));
    }

    [Fact]
    public void XiigenExtensionApiContract_SchemaArtifactMatchesMethodContracts()
    {
        var artifactPath = Path.Combine(
            FindRepoRoot(),
            "artifacts",
            "api-boundary",
            "API-02-method-contract-schema.json");
        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(artifactPath));
        var methods = document.RootElement.GetProperty("methods").EnumerateArray().ToArray();
        var methodNames = methods.Select(method => method.GetProperty("method").GetString() ?? string.Empty).ToArray();

        Assert.Equal(XiigenExtensionApiContract.MethodNames, methodNames);
        Assert.True(document.RootElement.GetProperty("thin_client_boundary").GetBoolean());
        Assert.False(document.RootElement.GetProperty("host_runtime_claimed").GetBoolean());
        Assert.False(document.RootElement.GetProperty("external_ai_used").GetBoolean());
        Assert.All(methods, method =>
        {
            var forbidden = method.GetProperty("forbidden_payloads").EnumerateArray()
                .Select(item => item.GetString())
                .ToArray();
            Assert.Contains("model_internals", forbidden);
            Assert.Contains("raw_corpus_data", forbidden);
            Assert.Contains("host_runtime_claim", forbidden);
        });
    }

    [Fact]
    public void XiigenExtensionApiService_UsesOperationResultEnvelope()
    {
        var method = typeof(XiigenExtensionApiService)
            .GetMethod(nameof(XiigenExtensionApiService.ProcessHostExtensionRequest));

        Assert.NotNull(method);
        Assert.Equal(typeof(OperationResult<XiigenExtensionResponse>), method!.ReturnType);
    }

    [Fact]
    public void XiigenExtensionApiService_DefaultBoundaryKeepsSemanticClaimGatedAndScopesLoadedStateWording()
    {
        var service = new XiigenExtensionApiService();
        var request = CreateRequest(debugEnabled: false);

        var result = service.ProcessHostExtensionRequest(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var response = result.Value!;
        Assert.True(response.Success);
        Assert.Equal(XiigenExtensionApiService.PartialBoundaryStatus, response.Status);
        Assert.Equal("API-01", response.CheckSetResults["trained_model_gate_ref"]);
        Assert.Contains("all_common_assets_loaded", response.CheckSetResults.Keys);
        Assert.Contains("prompt_to_task_asset_loaded", response.CheckSetResults.Keys);
        Assert.Contains("orchestration_reasoning_asset_loaded", response.CheckSetResults.Keys);
        Assert.Contains("skill_selection_asset_loaded", response.CheckSetResults.Keys);
        Assert.Equal("false", response.CheckSetResults["semantic_quality_claimed"]);
        Assert.Equal("false", response.CheckSetResults["runtime_extension_claimed"]);
        Assert.Contains(response.StructuredTaskRefs, taskRef => taskRef.StartsWith("structured-task:corr-001:", StringComparison.Ordinal));
        Assert.Contains(response.ScenarioRefs, scenarioRef => scenarioRef.StartsWith("scenario:", StringComparison.Ordinal));
        Assert.Contains(response.Warnings, warning => warning.Contains("semantic_quality_claimed=false", StringComparison.Ordinal));
        Assert.Contains(response.Warnings, warning => warning.Contains("SkillSelectionMachine used loaded baseline/typed trained state; semantic quality remains unclaimed.", StringComparison.Ordinal));
        Assert.DoesNotContain("trained common asset evidence", response.ChatDisplayText, StringComparison.Ordinal);
        Assert.DoesNotContain(response.Warnings, warning => warning.Contains("used available trained state", StringComparison.Ordinal));
        Assert.DoesNotContain(response.Warnings, warning => warning.Contains("trained common asset evidence", StringComparison.Ordinal));
        Assert.Null(response.DebugTrace);
    }

    [Fact]
    public void XiigenExtensionApiService_BaselinePromptAndOrchestrationUnitsKeepSemanticClaimFalse()
    {
        var promptToTaskMachine = CreatePromptToTaskMachineWithLearnedBaselinePattern();
        var orchestrationMachine = CreateOrchestrationReasoningMachineWithLearnedBaselinePattern();
        var service = CreateLoadedApiService(
            promptToTaskMachine,
            orchestrationMachine,
            new SkillSelectionMachine());
        var request = CreateRequest(debugEnabled: false) with
        {
            CheckSetInputs = new Dictionary<string, string>
            {
                ["constraint.universal_reasoning"] = "present"
            },
            HostFacts = new Dictionary<string, string>
            {
                ["workspace"] = "workspace:C:\\Projects\\llm_mvp_core",
                ["host_client"] = "thin"
            },
            AvailableSkills = ["orchestration", "plan-review"]
        };

        var result = service.ProcessHostExtensionRequest(request);
        var response = result.Value!;

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.True(response.Success);
        Assert.Equal(PromptToTaskMachine.LearnedPatternStatus, response.CheckSetResults["prompt_to_task_status"]);
        Assert.Equal(OrchestrationReasoningMachine.LearnedPatternStatus, response.CheckSetResults["orchestration_reasoning_status"]);
        Assert.Equal("true", response.CheckSetResults["all_common_assets_loaded"]);
        Assert.Equal("false", response.CheckSetResults["semantic_quality_claimed"]);
        Assert.Contains(response.Warnings, warning => warning.Contains("semantic_quality_claimed=false", StringComparison.Ordinal));
        Assert.Contains(response.Warnings, warning => warning.Contains(PromptToTaskMachine.LearnedPatternStatus, StringComparison.Ordinal));
        Assert.Contains(response.Warnings, warning => warning.Contains(OrchestrationReasoningMachine.LearnedPatternStatus, StringComparison.Ordinal));
        Assert.Contains("Semantic quality is not claimed", response.ChatDisplayText, StringComparison.Ordinal);
    }

    [Fact]
    public void XiigenExtensionApiService_SemanticClaimRequiresPositiveEvidenceFromEveryComponent()
    {
        var service = CreateLoadedApiService(
            new PromptToTaskMachine(),
            new OrchestrationReasoningMachine(),
            new SkillSelectionMachine());
        var promptToTask = new PromptToTaskResponse(
            ResponseId: "ptt-baseline-response",
            Request: new PromptToTaskRequest(
                PromptId: "ptt-baseline",
                RawPrompt: string.Empty,
                RawPromptSha256: "sha256:ptt-baseline",
                PromptProvenance: "test",
                UserLanguage: "en",
                ExplicitConstraints: [],
                ContextFields: new Dictionary<string, string>()),
            StructuredTask: CreateStructuredTaskForApiBaseline(),
            ContractEvidence:
            [
                "PROMPT_TO_TASK_TASK_PATTERN_UNIT_USED",
                "LEARNED_PATTERN_MATCHED",
                "BASELINE_NOT_SEMANTIC_QUALITY",
                "SEMANTIC_QUALITY_CLAIMED_FALSE"
            ],
            ContractStatus: PromptToTaskMachine.LearnedPatternStatus);
        var orchestration = new OrchestrationReasoningResponse(
            ResponseId: "or-baseline-response",
            Request: new OrchestrationReasoningRequest(
                RequestId: "or-baseline",
                Task: promptToTask.StructuredTask,
                GuideDerivedStateRefs: [],
                CorpusBundleIds: [],
                AdaptiveContextRefs: new Dictionary<string, string>(),
                RuntimePolicyRefs: new Dictionary<string, string>()),
            ScenarioPlan: new OrchestrationScenarioPlan(
                ScenarioId: "scenario:baseline",
                Task: promptToTask.StructuredTask,
                SelectedLearnedPatterns: ["or-baseline-pattern"],
                PhaseFamilies: [],
                GateFamilies: [],
                AssetFamilies: [],
                HostCheckSets: [],
                RiskFlags: [],
                NextGranularizationActions: []),
            ContractEvidence:
            [
                "ORCHESTRATION_REASONING_SCENARIO_UNIT_USED",
                "LEARNED_GUIDE_PATTERN_MATCHED",
                "BASELINE_NOT_SEMANTIC_QUALITY",
                "SEMANTIC_QUALITY_CLAIMED_FALSE"
            ],
            ContractStatus: OrchestrationReasoningMachine.LearnedPatternStatus);
        var skillDecision = new SkillSelectionDecision(
            SelectedSkillIds: ["plan-review"],
            RejectedSkillIds: [],
            EvidenceFeatures: ["SEMANTIC_QUALITY_CLAIMED_TRUE"],
            Confidence: 1.0,
            RequiresSecondPass: false,
            DecisionText: "Future positive skill evidence fixture.");

        var shouldClaim = InvokeShouldClaimSemanticQuality(service, promptToTask, orchestration, skillDecision);

        Assert.False(shouldClaim);
    }

    [Fact]
    public void XiigenExtensionApiService_MissingDefaultAssetsDegradeWithoutSemanticClaim()
    {
        var missingRoot = Path.Combine(Path.GetTempPath(), $"xiigen-api-missing-assets-{Guid.NewGuid():N}");
        var service = new XiigenExtensionApiService(missingRoot);
        var request = CreateRequest(debugEnabled: true);

        var result = service.ProcessHostExtensionRequest(request);
        var response = result.Value!;

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.True(response.Success);
        Assert.Equal("false", response.CheckSetResults["all_common_assets_loaded"]);
        Assert.Equal("false", response.CheckSetResults["prompt_to_task_asset_loaded"]);
        Assert.Equal("false", response.CheckSetResults["orchestration_reasoning_asset_loaded"]);
        Assert.Equal("false", response.CheckSetResults["skill_selection_asset_loaded"]);
        Assert.Equal("false", response.CheckSetResults["semantic_quality_claimed"]);
        Assert.Equal("false", response.CheckSetResults["runtime_extension_claimed"]);
        Assert.Equal(PromptToTaskMachine.NotTrainedStatus, response.CheckSetResults["prompt_to_task_status"]);
        Assert.Equal(OrchestrationReasoningMachine.NotTrainedStatus, response.CheckSetResults["orchestration_reasoning_status"]);
        Assert.Contains(response.Warnings, warning => warning.Contains("common asset load skipped/failed", StringComparison.Ordinal));
        Assert.NotNull(response.DebugTrace);
        Assert.False(response.DebugTrace!.ContainsRawPrivateContent);
        Assert.DoesNotContain(request.UserPromptOrTaskText, response.DebugTrace.InputPayloadText, StringComparison.Ordinal);
        Assert.DoesNotContain(request.UserPromptOrTaskText, response.DebugTrace.OutputPayloadText, StringComparison.Ordinal);
    }

    [Fact]
    public void XiigenExtensionApiService_DebugTraceRedactsRawPromptAndShowsMethodInputOutput()
    {
        const string rawPrivatePrompt = "raw private prompt: implement my secret local workflow";
        var service = new XiigenExtensionApiService();
        var request = CreateRequest(debugEnabled: true) with
        {
            UserPromptOrTaskText = rawPrivatePrompt
        };

        var result = service.ProcessHostExtensionRequest(request);
        var response = result.Value!;

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.NotNull(response.DebugTrace);
        Assert.Contains("xiigen plan.review :", response.ChatDisplayText);
        Assert.Contains("input:", response.ChatDisplayText);
        Assert.Contains("output:", response.ChatDisplayText);
        Assert.Contains("\"userPromptOrTaskText\":\"[redacted]\"", response.DebugTrace!.InputPayloadText);
        Assert.Contains("\"semanticQualityClaimed\":\"false\"", response.DebugTrace.OutputPayloadText);
        Assert.False(response.DebugTrace.ContainsRawPrivateContent);
        Assert.DoesNotContain(rawPrivatePrompt, response.DebugTrace.InputPayloadText, StringComparison.Ordinal);
        Assert.DoesNotContain(rawPrivatePrompt, response.DebugTrace.OutputPayloadText, StringComparison.Ordinal);
        Assert.DoesNotContain(rawPrivatePrompt, response.ChatDisplayText, StringComparison.Ordinal);
    }

    [Fact]
    public void XiigenExtensionApiService_MethodSpecificResponsesCarrySkillEvidenceCounts()
    {
        var service = new XiigenExtensionApiService();
        var methods = new[]
        {
            XiigenExtensionApiContract.PromptToTaskMethod,
            XiigenExtensionApiContract.OrchestrationPlanMethod,
            XiigenExtensionApiContract.SkillSelectionMethod
        };

        foreach (var method in methods)
        {
            var request = CreateRequest(debugEnabled: true) with
            {
                HostName = "Cursor",
                HostType = "cursor-extension",
                Method = method,
                UserPromptOrTaskText = "Prepare a bounded review plan, select implementation and test skills, and keep private paths redacted.",
                RequestedSkills = ["plan-review", "implementation-integrity", "test-integrity"],
                AvailableSkills =
                [
                    "test-integrity",
                    "implementation-integrity",
                    "plan-review",
                    "privacy-redaction",
                    "shortcut-audit",
                    "skill-selection-invariance"
                ],
                CorrelationId = $"method-specific-{method}"
            };

            var result = service.ProcessHostExtensionRequest(request);
            var response = result.Value!;

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.True(response.Success);
            Assert.NotEmpty(response.SelectedSkills);
            Assert.Equal("3", response.CheckSetResults["requested_skills_count"]);
            Assert.Equal(response.SelectedSkills.Count.ToString(System.Globalization.CultureInfo.InvariantCulture), response.CheckSetResults["selected_skills_count"]);
            Assert.Equal("false", response.CheckSetResults["required_skill_selection_absent"]);
            Assert.Contains("SEMANTIC_SKILL_SELECTION_PIPELINE_BOUNDARY", response.CheckSetResults["skill_selection_evidence"], StringComparison.Ordinal);
            Assert.Contains($"method={method}", response.CheckSetResults["skill_selection_method_rationale"], StringComparison.Ordinal);
            Assert.Contains(method switch
            {
                XiigenExtensionApiContract.PromptToTaskMethod => "Prompt-to-task produced",
                XiigenExtensionApiContract.OrchestrationPlanMethod => "Orchestration-plan produced",
                XiigenExtensionApiContract.SkillSelectionMethod => "Skill-selection chose",
                _ => "Method"
            }, response.ChatDisplayText, StringComparison.Ordinal);
            Assert.NotNull(response.DebugTrace);
            Assert.False(response.DebugTrace!.ContainsRawPrivateContent);
            Assert.DoesNotContain(request.UserPromptOrTaskText, response.DebugTrace.InputPayloadText, StringComparison.Ordinal);
            Assert.DoesNotContain(request.UserPromptOrTaskText, response.DebugTrace.OutputPayloadText, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void XiigenExtensionApiService_Row12PromptToTaskNoSendResponseCarriesStructuredQualityProjection()
    {
        var service = new XiigenExtensionApiService(
            new PromptToTaskMachine(),
            new OrchestrationReasoningMachine(),
            new SkillSelectionMachine());
        const string marker = "FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1";
        const string scenarioId = "frdp-p01-cursor-token-savings-baseline";
        var allowedSkills = new[]
        {
            "prompt-to-task",
            "task-to-plan",
            "task-to-skills",
            "evidence-ledger"
        };
        var request = CreateRequest(debugEnabled: true) with
        {
            HostName = "Cursor",
            HostType = "cursor-no-send-local-extension-runtime-candidate",
            Method = XiigenExtensionApiContract.PromptToTaskMethod,
            UserPromptOrTaskText = string.Join(
                Environment.NewLine,
                $"[{marker}]",
                "Host: Cursor no-send local extension runtime.",
                $"Scenario: {scenarioId}.",
                "User request: Convert this synthetic request into a bounded Smart Skills execution plan without calling external AI.",
                "Requirements:",
                "1. Preserve the marker and scenario id in references.",
                "2. Produce a prompt_to_task result with task_id, task_title, constraints, and acceptance_gates.",
                "3. Produce a task_to_plan result with three ordered steps and one verification gate per step.",
                "4. Produce a task_to_skills result selecting skills from this allowed set only: prompt-to-task, task-to-plan, task-to-skills, evidence-ledger.",
                "5. State non_claims: no token savings claim, no semantic quality claim, no readiness claim, no training, no checkpoint promotion, no locator update.",
                "6. Return deterministic structured text with sections: task, plan, skills, references, non_claims."),
            RequestedSkills = allowedSkills,
            AvailableSkills = allowedSkills,
            CheckSetInputs = new Dictionary<string, string>
            {
                ["scenario_id"] = scenarioId,
                ["marker"] = marker,
                ["pair_id"] = "frdp-p01-row12-cursor-token-baseline-v1",
                ["external_ai_send"] = "false",
                ["token_savings_claimed"] = "false",
                ["semantic_quality_claimed"] = "false",
                ["readiness_claimed"] = "false",
                ["training_started"] = "false",
                ["checkpoint_mutation"] = "false",
                ["locator_update"] = "false",
                ["constraint.no_external_ai"] = "true",
                ["constraint.no_checkpoint_mutation"] = "true"
            },
            Provenance = new Dictionary<string, string>
            {
                ["source"] = "row12-workload-spec"
            },
            CorrelationId = $"frdp-p01-row12-cursor-token-baseline-v1|{marker}|{scenarioId}"
        };

        var result = service.ProcessHostExtensionRequest(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var response = result.Value!;
        Assert.True(response.Success);
        Assert.Equal("false", response.CheckSetResults["semantic_quality_claimed"]);
        Assert.Equal("false", response.CheckSetResults["runtime_extension_claimed"]);
        Assert.Contains("prompt-to-task", response.SelectedSkills);
        Assert.Contains("task-to-plan", response.SelectedSkills);
        Assert.Contains("task-to-skills", response.SelectedSkills);
        Assert.Contains("evidence-ledger", response.SelectedSkills);
        Assert.Equal("false", response.CheckSetResults["required_skill_selection_absent"]);
        Assert.Contains("prompt_to_task", response.CheckSetResults.Keys);
        Assert.Contains("task_to_plan", response.CheckSetResults.Keys);
        Assert.Contains("task_to_skills", response.CheckSetResults.Keys);
        Assert.Contains("non_claims", response.CheckSetResults.Keys);
        Assert.Contains("row12_core_api_quality_projection", response.CheckSetResults.Keys);

        using var promptToTask = JsonDocument.Parse(response.CheckSetResults["prompt_to_task"]);
        var promptRoot = promptToTask.RootElement;
        Assert.True(promptRoot.TryGetProperty("task_id", out _));
        Assert.True(promptRoot.TryGetProperty("task_title", out _));
        Assert.NotEmpty(promptRoot.GetProperty("constraints").EnumerateArray());
        Assert.NotEmpty(promptRoot.GetProperty("acceptance_gates").EnumerateArray());

        using var taskToPlan = JsonDocument.Parse(response.CheckSetResults["task_to_plan"]);
        var steps = taskToPlan.RootElement.GetProperty("ordered_steps").EnumerateArray().ToArray();
        Assert.Equal(3, steps.Length);
        Assert.All(steps, step =>
            Assert.Contains("verification gate", step.GetProperty("verification_gate").GetString() ?? string.Empty, StringComparison.OrdinalIgnoreCase));

        using var taskToSkills = JsonDocument.Parse(response.CheckSetResults["task_to_skills"]);
        var selectedRequiredSkills = taskToSkills.RootElement.GetProperty("selected_required_skills")
            .EnumerateArray()
            .Select(item => item.GetString())
            .ToArray();
        Assert.Contains("prompt-to-task", selectedRequiredSkills);
        Assert.Contains("task-to-plan", selectedRequiredSkills);
        Assert.Contains("task-to-skills", selectedRequiredSkills);
        Assert.Contains("evidence-ledger", selectedRequiredSkills);

        using var nonClaimsDocument = JsonDocument.Parse(response.CheckSetResults["non_claims"]);
        var nonClaims = nonClaimsDocument.RootElement.EnumerateArray()
            .Select(item => item.GetString())
            .ToArray();
        Assert.Contains("no token savings claim", nonClaims);
        Assert.Contains("no semantic quality claim", nonClaims);
        Assert.Contains("no readiness claim", nonClaims);
        Assert.Contains("no training", nonClaims);
        Assert.Contains("no checkpoint promotion", nonClaims);
        Assert.Contains("no locator update", nonClaims);

        var fullProjection = response.CheckSetResults["row12_core_api_quality_projection"];
        Assert.Contains(marker, fullProjection, StringComparison.Ordinal);
        Assert.Contains(scenarioId, fullProjection, StringComparison.Ordinal);
        Assert.Contains("synthetic no-send workload", fullProjection, StringComparison.Ordinal);
        Assert.Contains("heuristic tokenizer", fullProjection, StringComparison.Ordinal);
        Assert.Contains("no external AI", fullProjection, StringComparison.Ordinal);
        Assert.Contains("token delta alone", fullProjection, StringComparison.Ordinal);
        Assert.NotNull(response.DebugTrace);
        Assert.Contains("qualityProjection", response.DebugTrace!.OutputPayloadText, StringComparison.Ordinal);
        Assert.Contains("row12_core_api_quality_projection", response.DebugTrace.OutputPayloadText, StringComparison.Ordinal);
        Assert.DoesNotContain(request.UserPromptOrTaskText, response.DebugTrace.OutputPayloadText, StringComparison.Ordinal);
    }

    [Fact]
    public void XiigenExtensionApiService_UniversalTaskFamilyPlansExposePublicPlanProducedStatus()
    {
        var service = new XiigenExtensionApiService();
        var cases = new[]
        {
            new
            {
                Method = XiigenExtensionApiContract.PromptToTaskMethod,
                Prompt = "Convert this messy request into an executable task: a support team wants an intake assistant that classifies incoming customer messages, asks one clarifying question only when required, and writes an auditable next-step record.",
                Requested = new[] { "plan-review", "authority-governance", "documentation-sync" }
            },
            new
            {
                Method = XiigenExtensionApiContract.OrchestrationPlanMethod,
                Prompt = "Build a vendor-neutral orchestration plan for replacing a recurring manual approval workflow with a tool-assisted process that has review gates, rollback, evidence capture, and clear stop conditions.",
                Requested = new[] { "plan-review", "quality-review", "runtime-QA", "rollback-safety" }
            },
            new
            {
                Method = XiigenExtensionApiContract.SkillSelectionMethod,
                Prompt = "Given a documentation QA task and a short plan, select reusable skills for checking requirement coverage, running safe verification, detecting overclaims, and returning a review packet without changing the product.",
                Requested = new[] { "documentation-sync", "quality-review", "test-integrity", "shortcut-audit" }
            }
        };

        foreach (var item in cases)
        {
            var request = CreateRequest(debugEnabled: false) with
            {
                HostName = "Codex",
                HostType = "codex",
                Method = item.Method,
                UserPromptOrTaskText = item.Prompt,
                RequestedSkills = item.Requested,
                AvailableSkills =
                [
                    "authority-governance",
                    "documentation-sync",
                    "held-out-ablation",
                    "plan-review",
                    "quality-review",
                    "rollback-safety",
                    "runtime-QA",
                    "shortcut-audit",
                    "skill-selection-invariance",
                    "test-integrity",
                    "token-measurement"
                ],
                CheckSetInputs = new Dictionary<string, string>(),
                Provenance = new Dictionary<string, string>
                {
                    ["source"] = "universal-task-family-api-test"
                },
                CorrelationId = $"universal-task-family-api-test-{item.Method}"
            };

            var result = service.ProcessHostExtensionRequest(request);
            var response = result.Value!;

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.True(response.Success);
            Assert.Equal("false", response.CheckSetResults["orchestration_reasoning_asset_loaded"]);
            Assert.Equal(
                OrchestrationReasoningSourceAdmission.ProvenanceRebuildRequiredCode,
                response.CheckSetResults["orchestration_reasoning_load_error"]);
            Assert.Equal(OrchestrationReasoningMachine.NotTrainedStatus, response.CheckSetResults["orchestration_reasoning_status"]);
            Assert.Equal(OrchestrationReasoningMachine.NotTrainedStatus, response.CheckSetResults["orchestration_reasoning_internal_status"]);
            Assert.Equal("0", response.CheckSetResults["orchestration_reasoning_selected_pattern_count"]);
            Assert.Equal("0", response.CheckSetResults["orchestration_reasoning_phase_family_count"]);
            Assert.Equal("0", response.CheckSetResults["orchestration_reasoning_gate_family_count"]);
            Assert.Contains(OrchestrationReasoningMachine.NoInferenceScenarioId, string.Join(" ", response.ScenarioRefs), StringComparison.Ordinal);
            Assert.Equal("false", response.CheckSetResults["semantic_quality_claimed"]);
            Assert.Equal("false", response.CheckSetResults["runtime_extension_claimed"]);
            Assert.Contains("Semantic quality is not claimed", response.ChatDisplayText, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void XiigenExtensionApiService_SkillSelectionIgnoresRowScenarioPairOutputHashAndCandidateOrder()
    {
        var service = new XiigenExtensionApiService();
        var orderedRequest = CreateRequest(debugEnabled: false) with
        {
            HostName = "Claude",
            HostType = "claude-local",
            Method = XiigenExtensionApiContract.SkillSelectionMethod,
            UserPromptOrTaskText = "Review implementation integrity, test integrity, privacy redaction, and shortcut safety from task semantics only.",
            RequestedSkills = ["implementation-integrity", "test-integrity", "privacy-redaction"],
            AvailableSkills =
            [
                "row-id-decoy",
                "implementation-integrity",
                "test-integrity",
                "privacy-redaction",
                "shortcut-audit",
                "plan-review"
            ],
            Provenance = new Dictionary<string, string>
            {
                ["scenario_id"] = "TQ-999-must-not-drive-selection",
                ["pair_id"] = "PAIR-999-must-not-drive-selection",
                ["output_hash"] = "sha256:must-not-drive-selection",
                ["source_prompt_entry_id"] = "UPS-row-scenario-pair-decoy"
            },
            CorrelationId = "quality-row-999"
        };
        var reversedRequest = orderedRequest with
        {
            AvailableSkills = orderedRequest.AvailableSkills.Reverse().ToArray(),
            CorrelationId = "quality-row-1000"
        };

        var orderedResult = service.ProcessHostExtensionRequest(orderedRequest);
        var reversedResult = service.ProcessHostExtensionRequest(reversedRequest);
        var orderedResponse = orderedResult.Value!;
        var reversedResponse = reversedResult.Value!;

        Assert.True(orderedResult.IsSuccess, orderedResult.ErrorMessage);
        Assert.True(reversedResult.IsSuccess, reversedResult.ErrorMessage);
        Assert.NotEmpty(orderedResponse.SelectedSkills);
        Assert.Equal(
            orderedResponse.SelectedSkills.Order(StringComparer.Ordinal).ToArray(),
            reversedResponse.SelectedSkills.Order(StringComparer.Ordinal).ToArray());
        Assert.DoesNotContain("row-id-decoy", orderedResponse.SelectedSkills);
        Assert.DoesNotContain("TQ-999", orderedResponse.CheckSetResults["skill_selection_evidence"], StringComparison.Ordinal);
        Assert.DoesNotContain("PAIR-999", orderedResponse.CheckSetResults["skill_selection_evidence"], StringComparison.Ordinal);
        Assert.DoesNotContain("output_hash", orderedResponse.CheckSetResults["skill_selection_evidence"], StringComparison.Ordinal);
        Assert.DoesNotContain("candidate_index=", orderedResponse.CheckSetResults["skill_selection_evidence"], StringComparison.Ordinal);
        Assert.DoesNotContain("candidate_weight=", orderedResponse.CheckSetResults["skill_selection_evidence"], StringComparison.Ordinal);
        Assert.Equal("false", orderedResponse.CheckSetResults["required_skill_selection_absent"]);
    }

    [Fact]
    public async Task XiigenExtensionApiService_UserAdaptiveSkillSelectionPreservesRequestedSemanticSurfaceWhenEmpty()
    {
        var root = Path.Combine(Path.GetTempPath(), $"xiigen-api-common-semantic-{Guid.NewGuid():N}");
        var commonRoot = Path.Combine(root, "common");
        var adaptiveRoot = Path.Combine(root, "user");
        const string SemanticPrompt =
            "Review private output boundaries and redact unsafe disclosure before returning extension API evidence.";
        var commonMachine = new SkillSelectionMachine();
        var availableSkills = new[]
        {
            "documentation-sync",
            "plan-review",
            "privacy-redaction",
            "test-integrity"
        };
        var trained = await commonMachine.TrainMachineBatchAsync(
        [
            new SkillSelectionMachinePair(
                PairId: "xiigen-api-common-semantic-pair",
                Request: new SkillSelectionRequest(
                    Prompt: SemanticPrompt,
                    ProjectRoot: "repo:xiigen-api-common-semantic",
                    AvailableSkillIds: availableSkills,
                    HostFacts: new Dictionary<string, string>
                    {
                        ["host"] = "cursor",
                        ["route"] = "common-semantic-fixture"
                    },
                    UserPromptSourceEntryId: "xiigen-api-common-semantic-source"),
                Chosen: new SkillSelectionDecision(
                    SelectedSkillIds: ["privacy-redaction"],
                    RejectedSkillIds: ["documentation-sync", "plan-review", "test-integrity"],
                    EvidenceFeatures: ["XIIGEN_API_COMMON_SEMANTIC_CHOSEN"],
                    Confidence: 0.9,
                    RequiresSecondPass: false,
                    DecisionText: "Privacy boundary skill selected from prompt/context evidence."),
                Rejected: new SkillSelectionDecision(
                    SelectedSkillIds: ["plan-review"],
                    RejectedSkillIds: ["privacy-redaction"],
                    EvidenceFeatures: ["XIIGEN_API_COMMON_SEMANTIC_REJECTED"],
                    Confidence: 0.1,
                    RequiresSecondPass: true,
                    DecisionText: "Rejected decision ignores the privacy boundary."),
                RejectedType: "missing_privacy_boundary_skill",
                SourcePromptEntryId: "xiigen-api-common-semantic-source",
                TargetTrainingAdapter: SkillSelectionMachine.PromptContextTrainingAdapter)
        ]);
        Assert.True(trained.IsSuccess, trained.ErrorMessage);
        var semanticProbeRequest = new SkillSelectionRequest(
            Prompt: SemanticPrompt,
            ProjectRoot: "repo:xiigen-api-common-semantic",
            AvailableSkillIds: availableSkills,
            HostFacts: new Dictionary<string, string>
            {
                ["host"] = "cursor",
                ["route"] = "common-semantic-fixture"
            },
            UserPromptSourceEntryId: "xiigen-api-common-semantic-source");
        var commonProbe = commonMachine.DecideFromTrainedState(semanticProbeRequest);
        Assert.True(commonProbe.IsSuccess, commonProbe.ErrorMessage);
        Assert.Equal(["privacy-redaction"], commonProbe.Value!.SelectedSkillIds);
        await WriteSkillSelectionAssetAsync(Path.Combine(commonRoot, "models"), commonMachine);
        var loadedCommon = await SkillSelectionMachine.LoadCheckpointAsync(Path.Combine(
            commonRoot,
            "models",
            "skill-selection",
            "skill-selection-v1",
            "skill-selection-v1-checkpoint.json"));
        Assert.True(loadedCommon.IsSuccess, loadedCommon.ErrorMessage);
        var loadedCommonProbe = loadedCommon.Value!.Machine.DecideFromTrainedState(semanticProbeRequest);
        Assert.True(loadedCommonProbe.IsSuccess, loadedCommonProbe.ErrorMessage);
        Assert.Equal(["privacy-redaction"], loadedCommonProbe.Value!.SelectedSkillIds);
        await WriteSkillSelectionAssetAsync(adaptiveRoot, await BuildActiveUserNoDecisionSkillSelectionMachineAsync());

        try
        {
            var service = new XiigenExtensionApiService(commonRoot, adaptiveRoot);
            var composedRefs = GetComposedCheckSetRefs(service);
            Assert.Equal("true", composedRefs["skill_selection_asset_loaded"]);
            Assert.Equal("true", composedRefs["skill_selection_user_asset_loaded_first"]);
            var activeUserProbe = GetComposedSkillSelectionMachine(service, "SkillSelectionMachine")
                .DecideFromTrainedState(semanticProbeRequest);
            Assert.True(activeUserProbe.IsSuccess, activeUserProbe.ErrorMessage);
            Assert.Empty(activeUserProbe.Value!.SelectedSkillIds);
            var composedCommonProbe = GetComposedSkillSelectionMachine(service, "CommonSkillSelectionMachine")
                .DecideFromTrainedState(semanticProbeRequest);
            Assert.True(composedCommonProbe.IsSuccess, composedCommonProbe.ErrorMessage);
            Assert.Equal(["privacy-redaction"], composedCommonProbe.Value!.SelectedSkillIds);
            var request = CreateRequest(debugEnabled: false) with
            {
                HostName = "Cursor",
                HostType = "cursor-local",
                Method = XiigenExtensionApiContract.SkillSelectionMethod,
                UserPromptOrTaskText = SemanticPrompt,
                HostFacts = new Dictionary<string, string>
                {
                    ["workspace"] = "repo:xiigen-api-common-semantic",
                    ["host"] = "cursor",
                    ["route"] = "common-semantic-fixture"
                },
                RequestedSkills = ["plan-review"],
                AvailableSkills = availableSkills,
                Provenance = new Dictionary<string, string>
                {
                    ["source_prompt_entry_id"] = "xiigen-api-common-semantic-source",
                    ["mode"] = "adaptive_common_plus_user_model"
                },
                CorrelationId = "xiigen-api-common-semantic-source"
            };

            var result = service.ProcessHostExtensionRequest(request);
            var response = result.Value!;

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.True(response.Success);
            Assert.Equal("true", response.CheckSetResults["user_adaptive_any_asset_loaded"]);
            Assert.Equal(["privacy-redaction"], response.SelectedSkills);
            Assert.DoesNotContain("plan-review", response.SelectedSkills);
            Assert.Equal("false", response.CheckSetResults["required_skill_selection_absent"]);
            Assert.Equal(response.SelectedSkills.Count.ToString(System.Globalization.CultureInfo.InvariantCulture), response.CheckSetResults["selected_skills_count"]);
            Assert.Contains("USER_ADAPTIVE_SKILL_SELECTION_EMPTY_COMMON_HOST_SURFACE_FALLBACK", response.CheckSetResults["skill_selection_evidence"], StringComparison.Ordinal);
            Assert.Contains("USER_ADAPTIVE_SKILL_SELECTION_FALLBACK_NO_MODEL_MUTATION", response.CheckSetResults["skill_selection_evidence"], StringComparison.Ordinal);
            Assert.Contains(PromptContextSkillSelectionUnit.UnitExecutedEvidence, response.CheckSetResults["skill_selection_evidence"], StringComparison.Ordinal);
            Assert.Contains(SkillSelectionMachine.PromptContextCheckpointEvidence, response.CheckSetResults["skill_selection_evidence"], StringComparison.Ordinal);
            Assert.DoesNotContain("HOST_REQUESTED_SKILL_SURFACE_FALLBACK_NO_SEMANTIC_AUTHORITY", response.CheckSetResults["skill_selection_evidence"], StringComparison.Ordinal);
            Assert.Equal("false", response.CheckSetResults["semantic_quality_claimed"]);
            Assert.Equal("false", response.CheckSetResults["runtime_extension_claimed"]);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task XiigenExtensionApiService_RequestedSkillShortcutDecoysCannotBecomeSelectedAuthority()
    {
        const string BareSha256SkillDecoy = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";
        var root = Path.Combine(Path.GetTempPath(), $"xiigen-api-empty-skill-roots-{Guid.NewGuid():N}");
        var commonRoot = Path.Combine(root, "common");
        var adaptiveRoot = Path.Combine(root, "user");
        await WriteSkillSelectionAssetAsync(Path.Combine(commonRoot, "models"), new SkillSelectionMachine());
        await WriteSkillSelectionAssetAsync(adaptiveRoot, new SkillSelectionMachine());

        try
        {
        var service = new XiigenExtensionApiService(commonRoot, adaptiveRoot);
        var request = CreateRequest(debugEnabled: false) with
        {
            HostName = "Cursor",
            HostType = "cursor-local",
            Method = XiigenExtensionApiContract.SkillSelectionMethod,
            UserPromptOrTaskText = "Turn this request into a universal task: create human-readable documentation for an extension API with a simple diagram, no-code algorithms, short code quotations, and clear boundaries about what the evidence proves.",
            HostFacts = new Dictionary<string, string>
            {
                ["workspace"] = "repo:xiigen-api-empty-skill-roots",
                ["host"] = "cursor",
                ["source"] = "requested-skill-decoy-regression"
            },
            RequestedSkills =
            [
                "row-id-decoy",
                "row_id-decoy",
                "scenario_id:TQ-999-must-not-drive-selection",
                "scenario-id:TQ-HYPHEN-must-not-drive-selection",
                "pair_id:PAIR-999-must-not-drive-selection",
                "pair-id:PAIR-HYPHEN-must-not-drive-selection",
                "exact_prompt:Turn this request into a universal task",
                "exact-prompt:Turn this request into a universal task",
                "output_hash:sha256:must-not-drive-selection",
                "output-hash:sha256:must-not-drive-selection",
                "prompt_hash:sha256:must-not-drive-selection",
                "prompt-hash:sha256:must-not-drive-selection",
                "sha256:requested-skill-decoy",
                "sha256-requested-skill-decoy",
                "sha256_requested_skill_decoy",
                BareSha256SkillDecoy,
                "plan-review",
                "privacy-redaction",
                "code-reference-grounding"
            ],
            AvailableSkills =
            [
                "row-id-decoy",
                "row_id-decoy",
                "scenario_id:TQ-999-must-not-drive-selection",
                "scenario-id:TQ-HYPHEN-must-not-drive-selection",
                "pair_id:PAIR-999-must-not-drive-selection",
                "pair-id:PAIR-HYPHEN-must-not-drive-selection",
                "exact_prompt:Turn this request into a universal task",
                "exact-prompt:Turn this request into a universal task",
                "output_hash:sha256:must-not-drive-selection",
                "output-hash:sha256:must-not-drive-selection",
                "prompt_hash:sha256:must-not-drive-selection",
                "prompt-hash:sha256:must-not-drive-selection",
                "sha256:requested-skill-decoy",
                "sha256-requested-skill-decoy",
                "sha256_requested_skill_decoy",
                BareSha256SkillDecoy,
                "plan-review",
                "privacy-redaction",
                "code-reference-grounding",
                "shortcut-audit",
                "skill-selection-invariance"
            ],
            Provenance = new Dictionary<string, string>
            {
                ["scenario_id"] = "TQ-999-must-not-drive-selection",
                ["pair_id"] = "PAIR-999-must-not-drive-selection",
                ["output_hash"] = "sha256:must-not-drive-selection",
                ["source_prompt_entry_id"] = "requested-skill-decoy-regression",
                ["mode"] = "adaptive_common_plus_user_model"
            },
            CorrelationId = "requested-skill-decoy-regression"
        };

        var result = service.ProcessHostExtensionRequest(request);
        var response = result.Value!;

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.True(response.Success);
        Assert.Equal("true", response.CheckSetResults["skill_selection_asset_loaded"]);
        Assert.Equal("true", response.CheckSetResults["skill_selection_user_asset_loaded_first"]);
        Assert.Equal(
            ["plan-review", "privacy-redaction", "code-reference-grounding"],
            response.SelectedSkills);
        Assert.DoesNotContain(response.SelectedSkills, skill => skill.Contains("row-id", StringComparison.Ordinal));
        Assert.DoesNotContain(response.SelectedSkills, skill => skill.Contains("row_id", StringComparison.Ordinal));
        Assert.DoesNotContain(response.SelectedSkills, skill => skill.Contains("scenario-id", StringComparison.Ordinal));
        Assert.DoesNotContain(response.SelectedSkills, skill => skill.Contains("scenario_id", StringComparison.Ordinal));
        Assert.DoesNotContain(response.SelectedSkills, skill => skill.Contains("pair-id", StringComparison.Ordinal));
        Assert.DoesNotContain(response.SelectedSkills, skill => skill.Contains("pair_id", StringComparison.Ordinal));
        Assert.DoesNotContain(response.SelectedSkills, skill => skill.Contains("exact-prompt", StringComparison.Ordinal));
        Assert.DoesNotContain(response.SelectedSkills, skill => skill.Contains("exact_prompt", StringComparison.Ordinal));
        Assert.DoesNotContain(response.SelectedSkills, skill => skill.Contains("output-hash", StringComparison.Ordinal));
        Assert.DoesNotContain(response.SelectedSkills, skill => skill.Contains("output_hash", StringComparison.Ordinal));
        Assert.DoesNotContain(response.SelectedSkills, skill => skill.Contains("prompt_hash", StringComparison.Ordinal));
        Assert.DoesNotContain(response.SelectedSkills, skill => skill.Contains("prompt-hash", StringComparison.Ordinal));
        Assert.DoesNotContain(response.SelectedSkills, skill => skill.StartsWith("sha256:", StringComparison.Ordinal));
        Assert.DoesNotContain(response.SelectedSkills, skill => skill.StartsWith("sha256-", StringComparison.Ordinal));
        Assert.DoesNotContain(response.SelectedSkills, skill => skill.StartsWith("sha256_", StringComparison.Ordinal));
        Assert.DoesNotContain(response.SelectedSkills, skill => string.Equals(skill, BareSha256SkillDecoy, StringComparison.Ordinal));

        var finalAuthoritySurface = string.Join(
            " ",
            response.ChatDisplayText,
            response.CheckSetResults["skill_selection_evidence"]);
        Assert.Contains("HOST_REQUESTED_SKILL_SURFACE_SHORTCUT_DECOYS_FILTERED=16", finalAuthoritySurface, StringComparison.Ordinal);
        Assert.DoesNotContain("row-id-decoy", finalAuthoritySurface, StringComparison.Ordinal);
        Assert.DoesNotContain("row_id-decoy", finalAuthoritySurface, StringComparison.Ordinal);
        Assert.DoesNotContain("TQ-999", finalAuthoritySurface, StringComparison.Ordinal);
        Assert.DoesNotContain("TQ-HYPHEN", finalAuthoritySurface, StringComparison.Ordinal);
        Assert.DoesNotContain("PAIR-999", finalAuthoritySurface, StringComparison.Ordinal);
        Assert.DoesNotContain("PAIR-HYPHEN", finalAuthoritySurface, StringComparison.Ordinal);
        Assert.DoesNotContain("exact_prompt", finalAuthoritySurface, StringComparison.Ordinal);
        Assert.DoesNotContain("exact-prompt", finalAuthoritySurface, StringComparison.Ordinal);
        Assert.DoesNotContain("output_hash", finalAuthoritySurface, StringComparison.Ordinal);
        Assert.DoesNotContain("output-hash", finalAuthoritySurface, StringComparison.Ordinal);
        Assert.DoesNotContain("prompt_hash", finalAuthoritySurface, StringComparison.Ordinal);
        Assert.DoesNotContain("prompt-hash", finalAuthoritySurface, StringComparison.Ordinal);
        Assert.DoesNotContain("sha256:must-not-drive-selection", finalAuthoritySurface, StringComparison.Ordinal);
        Assert.DoesNotContain("sha256:requested-skill-decoy", finalAuthoritySurface, StringComparison.Ordinal);
        Assert.DoesNotContain("sha256-requested-skill-decoy", finalAuthoritySurface, StringComparison.Ordinal);
        Assert.DoesNotContain("sha256_requested_skill_decoy", finalAuthoritySurface, StringComparison.Ordinal);
        Assert.DoesNotContain(BareSha256SkillDecoy, finalAuthoritySurface, StringComparison.Ordinal);
        Assert.Contains("USER_ADAPTIVE_SKILL_SELECTION_EMPTY_COMMON_HOST_SURFACE_FALLBACK_UNAVAILABLE", finalAuthoritySurface, StringComparison.Ordinal);
        Assert.Contains("HOST_REQUESTED_SKILL_SURFACE_FALLBACK_NO_MODEL_MUTATION", finalAuthoritySurface, StringComparison.Ordinal);
        Assert.Equal("false", response.CheckSetResults["required_skill_selection_absent"]);
        Assert.Equal("false", response.CheckSetResults["semantic_quality_claimed"]);
        Assert.Equal("false", response.CheckSetResults["runtime_extension_claimed"]);
        AssertFallbackWordingIsNonSemantic(response);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void XiigenExtensionApiService_InvalidRequestReturnsResponseErrors()
    {
        var service = new XiigenExtensionApiService();
        var request = CreateRequest(debugEnabled: false) with
        {
            HostName = "",
            Method = "",
            CorrelationId = ""
        };

        var result = service.ProcessHostExtensionRequest(request);
        var response = result.Value!;

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.False(response.Success);
        Assert.Equal(XiigenExtensionApiService.InvalidRequestStatus, response.Status);
        Assert.Contains("host_name_required", response.Errors);
        Assert.Contains("method_required", response.Errors);
        Assert.Contains("correlation_id_required", response.Errors);
        Assert.Equal("false", response.CheckSetResults["semantic_quality_claimed"]);
    }

    [Fact]
    public async Task XiigenExtensionApiService_TreatsCandidatePositionStateAsSemanticBoundaryScaffold()
    {
        var skillMachine = new SkillSelectionMachine();
        var skillRequest = new SkillSelectionRequest(
            Prompt: "sha256:training-fixture",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["orchestration", "plan-review"],
            HostFacts: new Dictionary<string, string> { ["host"] = "codex" },
            UserPromptSourceEntryId: "UPS-XIIGEN-001");
        var chosen = new SkillSelectionDecision(
            SelectedSkillIds: ["plan-review"],
            RejectedSkillIds: ["orchestration"],
            EvidenceFeatures: ["TRAINING_PAIR_SELECTED_PLAN_REVIEW"],
            Confidence: 1.0,
            RequiresSecondPass: false,
            DecisionText: "Human-corrected pair prefers plan-review.");
        var rejected = new SkillSelectionDecision(
            SelectedSkillIds: ["orchestration"],
            RejectedSkillIds: ["plan-review"],
            EvidenceFeatures: ["REJECTED_MISSING_REVIEW"],
            Confidence: 0.1,
            RequiresSecondPass: true,
            DecisionText: "Rejected because review skill was missing.");
        var training = await skillMachine.TrainMachineBatchAsync(
            [
                new SkillSelectionMachinePair(
                    PairId: "xiigen-api-skill-pair-001",
                    Request: skillRequest,
                    Chosen: chosen,
                    Rejected: rejected,
                    RejectedType: "missing_review_skill",
                    SourcePromptEntryId: "UPS-XIIGEN-001",
                    TargetTrainingAdapter: "ISkillSelectionTraining")
            ]);
        var service = new XiigenExtensionApiService(
            new PromptToTaskMachine(),
            new OrchestrationReasoningMachine(),
            skillMachine);

        var result = service.ProcessHostExtensionRequest(CreateRequest(debugEnabled: false));
        var response = result.Value!;

        Assert.True(training.IsSuccess, training.ErrorMessage);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(["plan-review"], response.SelectedSkills);
        Assert.Equal("false", response.CheckSetResults["semantic_quality_claimed"]);
        Assert.Equal("false", response.CheckSetResults["skill_selection_requires_second_pass"]);
        Assert.Equal("false", response.CheckSetResults["required_skill_selection_absent"]);
        Assert.Contains("HOST_REQUESTED_SKILL_SURFACE_FALLBACK_NO_SEMANTIC_AUTHORITY", response.CheckSetResults["skill_selection_evidence"]);
        Assert.Contains("HOST_REQUESTED_SKILL_SURFACE_FALLBACK_NO_MODEL_MUTATION", response.CheckSetResults["skill_selection_evidence"]);
        Assert.Contains(SkillSelectionMachine.SemanticPipelineBoundaryEvidence, response.CheckSetResults["skill_selection_evidence"]);
        Assert.Contains(SkillSelectionMachine.CandidatePositionScaffoldEvidence, response.CheckSetResults["skill_selection_evidence"]);
        Assert.Contains(CandidateOrderLeakageGuardUnit.DiagnosticsEvidence, response.CheckSetResults["skill_selection_evidence"]);
        AssertFallbackWordingIsNonSemantic(response);
        Assert.DoesNotContain("candidate_position_suggestion=", response.CheckSetResults["skill_selection_evidence"]);
        Assert.DoesNotContain("candidate_index=", response.CheckSetResults["skill_selection_evidence"]);
        Assert.DoesNotContain("candidate_weight=", response.CheckSetResults["skill_selection_evidence"]);
        Assert.Contains("candidate_position_role=diagnostics_provenance_only", response.CheckSetResults["skill_selection_evidence"]);
        Assert.Contains("candidate_order_probe_source=semantic_units_without_candidate_position", response.CheckSetResults["skill_selection_evidence"]);
        Assert.Contains("candidate_order_permutation_count=2", response.CheckSetResults["skill_selection_evidence"]);
        Assert.Contains("candidate_order_sensitivity_detected=False", response.CheckSetResults["skill_selection_evidence"]);
        Assert.Contains("SS_02_TO_SS_12_REQUIRED_FOR_FINAL_AUTHORITY", response.CheckSetResults["skill_selection_evidence"]);
        Assert.Contains(response.Warnings, warning => warning.Contains("semantic_quality_claimed=false", StringComparison.Ordinal));
        Assert.Contains(response.Warnings, warning => warning.Contains(PromptToTaskMachine.NotTrainedStatus, StringComparison.Ordinal));
        Assert.Contains(response.Warnings, warning => warning.Contains(OrchestrationReasoningMachine.NotTrainedStatus, StringComparison.Ordinal));
    }

    [Fact]
    public void XiigenExtensionApiService_ScorerTreatsCandidatePositionProvenanceAsNonAuthority()
    {
        var repoRoot = FindRepoRoot();
        var workspace = Path.Combine(Path.GetTempPath(), $"xiigen-scorer-candidate-position-{Guid.NewGuid():N}");
        Directory.CreateDirectory(workspace);
        var outputDir = Path.Combine(workspace, "out");
        var requestPath = Path.Combine(workspace, "request.json");
        var provenanceResponsePath = Path.Combine(workspace, "provenance-response.json");
        var rawAuthorityResponsePath = Path.Combine(workspace, "raw-authority-response.json");
        var orderAuthorityResponsePath = Path.Combine(workspace, "order-authority-response.json");
        var unlabelledResponsePath = Path.Combine(workspace, "unlabelled-response.json");
        var tracePath = Path.Combine(workspace, "trace.json");
        var inputPath = Path.Combine(workspace, "quality-input.jsonl");
        var pairInputPath = Path.Combine(workspace, "pair-input.jsonl");
        var statePath = Path.Combine(workspace, "state.json");
        var outputPrefix = "candidate-position-scorer-regression";

        var provenanceEvidence = string.Join(
            ",",
            SkillSelectionMachine.SemanticPipelineBoundaryEvidence,
            CandidateOrderLeakageGuardUnit.DiagnosticsEvidence,
            SkillSelectionMachine.CandidatePositionScaffoldEvidence,
            "candidate_position_role=diagnostics_provenance_only",
            "candidate_order_guard_status=selection_authority_blocked",
            "candidate_order_permutation_selection_authority=false",
            "candidate_order_probe_source=semantic_units_without_candidate_position",
            "candidate_order_permutation_count=2",
            "candidate_order_sensitivity_detected=False",
            "semantic_quality_claimed=false");
        var rawAuthorityEvidence = string.Join(
            ",",
            SkillSelectionMachine.SemanticPipelineBoundaryEvidence,
            SkillSelectionMachine.CandidatePositionScaffoldEvidence,
            "candidate_position_suggestion=plan-review",
            "candidate_index=0",
            "candidate_weight=9",
            "semantic_quality_claimed=false");
        var orderAuthorityEvidence = string.Join(
            ",",
            SkillSelectionMachine.SemanticPipelineBoundaryEvidence,
            SkillSelectionMachine.CandidatePositionScaffoldEvidence,
            CandidateOrderLeakageGuardUnit.DiagnosticsEvidence,
            "candidate_position_role=diagnostics_provenance_only",
            "candidate_order_guard_status=selection_authority_blocked",
            "candidate_order_permutation_selection_authority=true",
            "candidate_order_sensitivity_detected=True",
            "semantic_quality_claimed=false");
        var unlabelledEvidence = string.Join(
            ",",
            SkillSelectionMachine.SemanticPipelineBoundaryEvidence,
            SkillSelectionMachine.CandidatePositionScaffoldEvidence,
            "semantic_quality_claimed=false");

        WriteJsonDocument(
            requestPath,
            new
            {
                RequestedSkills = new[] { "plan-review" },
                AvailableSkills = new[] { "plan-review", "orchestration" }
            });
        WriteJsonDocument(
            provenanceResponsePath,
            CreateScorerResponse(provenanceEvidence));
        WriteJsonDocument(
            rawAuthorityResponsePath,
            CreateScorerResponse(rawAuthorityEvidence));
        WriteJsonDocument(
            orderAuthorityResponsePath,
            CreateScorerResponse(orderAuthorityEvidence));
        WriteJsonDocument(
            unlabelledResponsePath,
            CreateScorerResponse(unlabelledEvidence));
        WriteJsonDocument(tracePath, new { trace = "synthetic" });
        WriteJsonDocument(statePath, new { schema = "test-state.v1" });

        var rows = new List<object>();
        var pairRows = new List<object>();
        for (var pairIndex = 0; pairIndex < 54; pairIndex++)
        {
            var responsePath = pairIndex switch
            {
                < 18 => provenanceResponsePath,
                < 30 => rawAuthorityResponsePath,
                < 42 => orderAuthorityResponsePath,
                _ => unlabelledResponsePath
            };
            var pairId = pairIndex switch
            {
                < 18 => $"PAIR-PROVENANCE-{pairIndex:00}",
                < 30 => $"PAIR-RAW-AUTHORITY-{pairIndex:00}",
                < 42 => $"PAIR-ORDER-AUTHORITY-{pairIndex:00}",
                _ => $"PAIR-UNLABELLED-{pairIndex:00}"
            };
            pairRows.Add(new { pair_id = pairId, pairing_fields_preserved = true });

            rows.Add(CreateQualityRow(
                pairId,
                $"TQ-{pairIndex:00}-BASELINE",
                "baseline_common_model_only",
                responsePath,
                requestPath,
                tracePath,
                promptTokens: 200,
                outputTokens: 100));
            rows.Add(CreateQualityRow(
                pairId,
                $"TQ-{pairIndex:00}-ADAPTIVE",
                "adaptive_common_plus_user_model",
                responsePath,
                requestPath,
                tracePath,
                promptTokens: 180,
                outputTokens: 95));
        }

        WriteJsonl(inputPath, rows);
        WriteJsonl(pairInputPath, pairRows);

        var result = RunFullHostScorer(
            repoRoot,
            inputPath,
            pairInputPath,
            outputDir,
            statePath,
            outputPrefix);

        Assert.Equal(0, result.ExitCode);

        var rowScores = ReadJsonlDocuments(Path.Combine(outputDir, $"{outputPrefix}-row-scores.jsonl"));
        Assert.Equal(108, rowScores.Count);
        var provenanceRows = rowScores
            .Where(row => row.GetProperty("pair_id").GetString()!.StartsWith("PAIR-PROVENANCE-", StringComparison.Ordinal))
            .ToArray();
        var rawAuthorityRows = rowScores
            .Where(row => row.GetProperty("pair_id").GetString()!.StartsWith("PAIR-RAW-AUTHORITY-", StringComparison.Ordinal))
            .ToArray();
        var orderAuthorityRows = rowScores
            .Where(row => row.GetProperty("pair_id").GetString()!.StartsWith("PAIR-ORDER-AUTHORITY-", StringComparison.Ordinal))
            .ToArray();
        var unlabelledRows = rowScores
            .Where(row => row.GetProperty("pair_id").GetString()!.StartsWith("PAIR-UNLABELLED-", StringComparison.Ordinal))
            .ToArray();

        Assert.Equal(36, provenanceRows.Length);
        Assert.Equal(24, rawAuthorityRows.Length);
        Assert.Equal(24, orderAuthorityRows.Length);
        Assert.Equal(24, unlabelledRows.Length);
        Assert.All(provenanceRows, row =>
        {
            Assert.Equal(4, row.GetProperty("dimension_scores").GetProperty("no_deterministic_shortcut").GetInt32());
            Assert.True(row.GetProperty("candidate_position_evidence_present").GetBoolean());
            Assert.True(row.GetProperty("candidate_position_provenance_only").GetBoolean());
            Assert.False(row.GetProperty("candidate_position_authority_detected").GetBoolean());
            Assert.False(row.GetProperty("candidate_position_unlabelled").GetBoolean());
            Assert.True(row.GetProperty("quality_gate_passed").GetBoolean());
            Assert.Equal("PASS", row.GetProperty("verdict").GetString());
            Assert.DoesNotContain(
                row.GetProperty("forbidden_failures").EnumerateArray(),
                failure => failure.GetString()!.Contains("candidate_position", StringComparison.Ordinal));
        });
        Assert.All(rawAuthorityRows.Concat(orderAuthorityRows), row =>
        {
            Assert.Equal(0, row.GetProperty("dimension_scores").GetProperty("no_deterministic_shortcut").GetInt32());
            Assert.True(row.GetProperty("candidate_position_evidence_present").GetBoolean());
            Assert.False(row.GetProperty("candidate_position_provenance_only").GetBoolean());
            Assert.True(row.GetProperty("candidate_position_authority_detected").GetBoolean());
            Assert.False(row.GetProperty("candidate_position_unlabelled").GetBoolean());
            Assert.False(row.GetProperty("quality_gate_passed").GetBoolean());
            Assert.Equal("FAIL", row.GetProperty("verdict").GetString());
            Assert.Contains(
                row.GetProperty("forbidden_failures").EnumerateArray(),
                failure => string.Equals(
                    failure.GetString(),
                    "candidate_position_used_as_runtime_authority",
                    StringComparison.Ordinal));
        });
        Assert.All(unlabelledRows, row =>
        {
            Assert.Equal(0, row.GetProperty("dimension_scores").GetProperty("no_deterministic_shortcut").GetInt32());
            Assert.True(row.GetProperty("candidate_position_evidence_present").GetBoolean());
            Assert.False(row.GetProperty("candidate_position_provenance_only").GetBoolean());
            Assert.False(row.GetProperty("candidate_position_authority_detected").GetBoolean());
            Assert.True(row.GetProperty("candidate_position_unlabelled").GetBoolean());
            Assert.False(row.GetProperty("quality_gate_passed").GetBoolean());
            Assert.Equal("FAIL", row.GetProperty("verdict").GetString());
            Assert.Contains(
                row.GetProperty("forbidden_failures").EnumerateArray(),
                failure => string.Equals(
                    failure.GetString(),
                    "candidate_position_used_as_unlabelled_authority",
                    StringComparison.Ordinal));
        });
    }

    private static XiigenExtensionRequest CreateRequest(bool debugEnabled)
        => new(
            HostName: "codex",
            HostType: "cli-agent",
            Method: "plan.review",
            UserPromptOrTaskText: "Review exactly one bounded work order.",
            HostFacts: new Dictionary<string, string>
            {
                ["workspace"] = "workspace:C:\\Projects\\llm_mvp_core",
                ["host_client"] = "thin"
            },
            DebugEnabled: debugEnabled,
            RequestedSkills: ["plan-review"],
            AvailableSkills: ["orchestration", "plan-review"],
            CheckSetInputs: new Dictionary<string, string>
            {
                ["scope"] = "bounded",
                ["external_ai"] = "forbidden_without_approval"
            },
            Provenance: new Dictionary<string, string>
            {
                ["source"] = "conversation:current",
                ["work_order"] = "WO-API-01"
            },
            CorrelationId: "corr-001");

    private static XiigenExtensionApiService CreateLoadedApiService(
        PromptToTaskMachine promptToTaskMachine,
        OrchestrationReasoningMachine orchestrationReasoningMachine,
        SkillSelectionMachine skillSelectionMachine)
    {
        var serviceType = typeof(XiigenExtensionApiService);
        var stateType = serviceType.GetNestedType("CommonAssetLoadState", BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("CommonAssetLoadState type was not found.");
        var state = Activator.CreateInstance(
            stateType,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            args:
            [
                promptToTaskMachine,
                orchestrationReasoningMachine,
                skillSelectionMachine,
                new Dictionary<string, string>
                {
                    ["trained_model_gate_ref"] = "test-loaded-common-assets",
                    ["all_common_assets_loaded"] = "true",
                    ["prompt_to_task_asset_loaded"] = "true",
                    ["orchestration_reasoning_asset_loaded"] = "true",
                    ["skill_selection_asset_loaded"] = "true"
                },
                Array.Empty<string>(),
                true
            ],
            culture: null)
            ?? throw new InvalidOperationException("CommonAssetLoadState could not be created.");
        var constructor = serviceType.GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            types: [stateType],
            modifiers: null)
            ?? throw new InvalidOperationException("XiigenExtensionApiService state constructor was not found.");

        return (XiigenExtensionApiService)constructor.Invoke([state]);
    }

    private static bool InvokeShouldClaimSemanticQuality(
        XiigenExtensionApiService service,
        PromptToTaskResponse promptToTask,
        OrchestrationReasoningResponse orchestration,
        SkillSelectionDecision skillDecision)
    {
        var method = typeof(XiigenExtensionApiService).GetMethod(
            "ShouldClaimSemanticQuality",
            BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("ShouldClaimSemanticQuality was not found.");

        return (bool)method.Invoke(service, [promptToTask, orchestration, skillDecision])!;
    }

    private static PromptToTaskMachine CreatePromptToTaskMachineWithLearnedBaselinePattern()
    {
        var machine = new PromptToTaskMachine();
        var checkpoint = new PromptToTaskCheckpoint(
            SchemaVersion: 1,
            LearnedState: new PromptToTaskLearnedStateDescriptor(
                StateId: "ptt-api-baseline-state",
                ManifestId: "ptt-api-baseline-manifest",
                ManifestSha256: "sha256:ptt-api-baseline-manifest",
                MachineType: "PromptToTaskMachine",
                SchemaVersion: "prompt-to-task-state-v1",
                SourceBaseCommit: "commit:test",
                StateSha256: "sha256:ptt-api-baseline-state",
                LearnedPatterns:
                [
                    new PromptToTaskLearnedPattern(
                        PatternId: "ptt-api-baseline-pattern",
                        TaskType: "universal_reasoning",
                        LearnedSignals: ["constraint.universal_reasoning=present"],
                        ExplicitUserRequirements: ["classify document purpose"],
                        Constraints:
                        [
                            "governance_intent_identified",
                            "audience_boundary_identified",
                            "risk_impact_linked"
                        ],
                        Deliverables:
                        [
                            "reason over governance intent, audience impact, risk, and contradiction links before selecting an action"
                        ],
                        ForbiddenActions: ["semantic quality claim"],
                        RequiredHosts: ["codex"],
                        EvidenceNeeds:
                        [
                            "source_ref_present",
                            "related_ref_check_required",
                            "risk_consequence_recorded"
                        ],
                        SplitId: "ptt-api-baseline-split",
                        Confidence: 0.9)
                ]),
            AcceptedRefIds: ["ptt-api-baseline-accepted"],
            SourceManifestRef: "ptt-api-baseline-source",
            SemanticQualityClaimed: false,
            SavedAtUtc: DateTimeOffset.UtcNow);
        var imported = machine.ImportLearnedState(checkpoint);
        Assert.True(imported.IsSuccess, imported.ErrorMessage);
        return machine;
    }

    private static async Task WriteSkillSelectionAssetAsync(
        string root,
        SkillSelectionMachine machine)
    {
        var assetDirectory = Path.Combine(root, "skill-selection", "skill-selection-v1");
        var saved = await machine.SaveCheckpointAsync(
            Path.Combine(assetDirectory, "skill-selection-v1-checkpoint.json"),
            Path.Combine(assetDirectory, "manifest.json"));
        Assert.True(saved.IsSuccess, saved.ErrorMessage);
    }

    private static SkillSelectionMachine GetComposedSkillSelectionMachine(
        XiigenExtensionApiService service,
        string propertyName)
    {
        var state = typeof(XiigenExtensionApiService)
            .GetField("_commonAssetLoadState", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(service)!;
        return (SkillSelectionMachine)state.GetType()
            .GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public)!
            .GetValue(state)!;
    }

    private static IReadOnlyDictionary<string, string> GetComposedCheckSetRefs(
        XiigenExtensionApiService service)
    {
        var state = typeof(XiigenExtensionApiService)
            .GetField("_commonAssetLoadState", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(service)!;
        return (IReadOnlyDictionary<string, string>)state.GetType()
            .GetProperty("CheckSetRefs", BindingFlags.Instance | BindingFlags.Public)!
            .GetValue(state)!;
    }

    private static async Task<SkillSelectionMachine> BuildActiveUserNoDecisionSkillSelectionMachineAsync()
    {
        var machine = new SkillSelectionMachine();
        var trained = await machine.TrainMachineBatchAsync(
        [
            new SkillSelectionMachinePair(
                PairId: "xiigen-api-active-user-no-context-pair",
                Request: new SkillSelectionRequest(
                    Prompt: "Unrelated user-local checkpoint row.",
                    ProjectRoot: string.Empty,
                    AvailableSkillIds: ["unrelated-user-skill", "unrelated-rejected-skill"],
                    HostFacts: new Dictionary<string, string>(),
                    UserPromptSourceEntryId: string.Empty),
                Chosen: new SkillSelectionDecision(
                    SelectedSkillIds: ["unrelated-user-skill"],
                    RejectedSkillIds: ["unrelated-rejected-skill"],
                    EvidenceFeatures: ["XIIGEN_API_USER_NO_CONTEXT_CHOSEN"],
                    Confidence: 0.9,
                    RequiresSecondPass: false,
                    DecisionText: "Unrelated user-local training row."),
                Rejected: new SkillSelectionDecision(
                    SelectedSkillIds: ["unrelated-rejected-skill"],
                    RejectedSkillIds: ["unrelated-user-skill"],
                    EvidenceFeatures: ["XIIGEN_API_USER_NO_CONTEXT_REJECTED"],
                    Confidence: 0.1,
                    RequiresSecondPass: true,
                    DecisionText: "Rejected unrelated user-local row."),
                RejectedType: "unrelated_user_local_row",
                SourcePromptEntryId: "xiigen-api-active-user-no-context-source",
                TargetTrainingAdapter: SkillSelectionMachine.PromptContextTrainingAdapter)
        ]);
        Assert.True(trained.IsSuccess, trained.ErrorMessage);
        return machine;
    }

    private static OrchestrationReasoningMachine CreateOrchestrationReasoningMachineWithLearnedBaselinePattern()
    {
        var machine = new OrchestrationReasoningMachine();
        var checkpoint = new OrchestrationReasoningCheckpoint(
            SchemaVersion: 1,
            LearnedState: new OrchestrationReasoningLearnedGuideStateDescriptor(
                StateId: "or-api-baseline-state",
                GuideStateRef: "or-api-baseline-guide",
                ManifestId: "or-api-baseline-manifest",
                ManifestSha256: "sha256:or-api-baseline-manifest",
                MachineType: "OrchestrationReasoningMachine",
                SchemaVersion: "orchestration-reasoning-state-v1",
                SourceBaseCommit: "commit:test",
                StateSha256: "sha256:or-api-baseline-state",
                LearnedPatterns:
                [
                    new OrchestrationReasoningLearnedPattern(
                        PatternId: "or-api-baseline-pattern",
                        ScenarioRef: "scenario:api-baseline",
                        RequiredTaskTypes: ["universal_reasoning"],
                        RequiredConstraintSignals:
                        [
                            "governance_intent_identified",
                            "audience_boundary_identified",
                            "risk_impact_linked"
                        ],
                        RequiredRequirementSignals: ["classify document purpose"],
                        RequiredDeliverableSignals:
                        [
                            "reason over governance intent, audience impact, risk, and contradiction links before selecting an action"
                        ],
                        RequiredEvidenceSignals:
                        [
                            "source_ref_present",
                            "related_ref_check_required",
                            "risk_consequence_recorded"
                        ],
                        PhaseFamilies: ["orchestration-reasoning"],
                        GateFamilies: ["api-baseline-no-claim"],
                        AssetFamilies: ["llm_mvp_core"],
                        HostCheckSets: ["core-only"],
                        RiskFlags: ["no_semantic_quality_metric_claim"],
                        NextGranularizationActions: ["keep semantic quality unclaimed"],
                        Confidence: 0.9)
                ]),
            AcceptedRefIds: ["or-api-baseline-accepted"],
            SourceManifestRef: "or-api-baseline-source",
            SemanticQualityClaimed: false,
            ContinueTrainingClaimed: false,
            HostRuntimeClaimed: false,
            SavedAtUtc: DateTimeOffset.UtcNow);
        var legacyImport = machine.ImportLearnedState(checkpoint);
        Assert.False(legacyImport.IsSuccess);
        Assert.Equal(OrchestrationReasoningSourceAdmission.StateAdmissionInvalidCode, legacyImport.ErrorCode);

        var admitted = OrchestrationReasoningSourceAdmissionTestFixture.Admit(
            checkpoint.LearnedState,
            "or-api-baseline-r17-admission");
        var imported = machine.ImportAdmittedState(admitted);
        Assert.True(imported.IsSuccess, imported.ErrorMessage);
        return machine;
    }

    private static StructuredTaskContract CreateStructuredTaskForApiBaseline()
        => new(
            PromptId: "ptt-baseline",
            RawPromptSha256: "sha256:ptt-baseline",
            UserLanguage: "en",
            TaskType: "universal_reasoning",
            ExplicitUserRequirements: ["classify document purpose"],
            Constraints:
            [
                "governance_intent_identified",
                "audience_boundary_identified",
                "risk_impact_linked"
            ],
            Deliverables:
            [
                "reason over governance intent, audience impact, risk, and contradiction links before selecting an action"
            ],
            ForbiddenActions: ["semantic quality claim"],
            RequiredHosts: ["codex"],
            EvidenceNeeds:
            [
                "source_ref_present",
                "related_ref_check_required",
                "risk_consequence_recorded"
            ],
            SourceProvenance: new Dictionary<string, string>(),
            SplitId: "ptt-api-baseline-split");

    private static object CreateScorerResponse(string skillSelectionEvidence)
        => new
        {
            Success = true,
            Status = "partial_boundary",
            ChatDisplayText = "Synthetic response with honest closed claims.",
            SelectedSkills = new[] { "plan-review" },
            CheckSetResults = new Dictionary<string, string>
            {
                ["semantic_quality_claimed"] = "false",
                ["runtime_extension_claimed"] = "false",
                ["prompt_to_task_status"] = "semantic task produced",
                ["orchestration_reasoning_status"] = "semantic plan produced",
                ["orchestration_reasoning_load_error"] = "",
                ["skill_selection_requires_second_pass"] = "false",
                ["skill_selection_confidence"] = "0.75",
                ["skill_selection_evidence"] = skillSelectionEvidence,
                ["requested_skills_count"] = "1",
                ["selected_skills_count"] = "1",
                ["available_skills_count"] = "2",
                ["required_skill_selection_absent"] = "false"
            },
            StructuredTaskRefs = new[] { "structured-task:synthetic" },
            ScenarioRefs = new[] { "scenario:synthetic" },
            Warnings = Array.Empty<string>(),
            Errors = Array.Empty<string>(),
            DebugTrace = (object?)null
        };

    private static object CreateQualityRow(
        string pairId,
        string scenarioId,
        string mode,
        string responsePath,
        string requestPath,
        string tracePath,
        int promptTokens,
        int outputTokens)
        => new
        {
            scenario_id = scenarioId,
            pair_id = pairId,
            host = "codex",
            mode,
            method = "skill-selection",
            debug_enabled = true,
            prompt_hash = $"sha256:prompt-{scenarioId.ToLowerInvariant()}",
            output_hash = $"sha256:output-{scenarioId.ToLowerInvariant()}",
            tokenizer_name = "synthetic",
            tokenizer_version = "test",
            tokenizer_accounting_type = "synthetic",
            prompt_tokens = promptTokens,
            output_tokens = outputTokens,
            total_tokens = promptTokens + outputTokens,
            raw_output_ref_private = responsePath,
            host_runtime_request_ref = requestPath,
            raw_prompt_ref_private = requestPath,
            trace_ref = tracePath,
            adaptive_effect_classification = mode == "adaptive_common_plus_user_model"
                ? "observable_adaptive_effect"
                : "baseline_common",
            user_adaptive_any_asset_loaded = mode == "adaptive_common_plus_user_model" ? "true" : "false",
            user_adaptive_asset_root_used = mode == "adaptive_common_plus_user_model"
        };

    private static (int ExitCode, string StandardOutput, string StandardError) RunFullHostScorer(
        string repoRoot,
        string inputPath,
        string pairInputPath,
        string outputDir,
        string statePath,
        string outputPrefix)
    {
        var scriptPath = Path.Combine(repoRoot, "tools", "token-quality-full-host-scorer.ps1");
        var startInfo = global::PowerShellTestHarness.CreateScriptStartInfo(
            scriptPath,
            repoRoot);
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;
        startInfo.ArgumentList.Add("-InputPath");
        startInfo.ArgumentList.Add(inputPath);
        startInfo.ArgumentList.Add("-PairInputPath");
        startInfo.ArgumentList.Add(pairInputPath);
        startInfo.ArgumentList.Add("-OutputDir");
        startInfo.ArgumentList.Add(outputDir);
        startInfo.ArgumentList.Add("-StatePath");
        startInfo.ArgumentList.Add(statePath);
        startInfo.ArgumentList.Add("-PhaseId");
        startInfo.ArgumentList.Add("TEST-CANDIDATE-POSITION-SCORER-REGRESSION");
        startInfo.ArgumentList.Add("-OutputPrefix");
        startInfo.ArgumentList.Add(outputPrefix);

        return RunProcessWithTimeoutAsync(startInfo, ProcessTimeout)
            .GetAwaiter()
            .GetResult();
    }

    private static async Task<(int ExitCode, string StandardOutput, string StandardError)> RunProcessWithTimeoutAsync(
        ProcessStartInfo startInfo,
        TimeSpan timeout)
    {
        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Process could not start: {DescribeCommand(startInfo)}");
        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();

        using var timeoutCts = new CancellationTokenSource(timeout);
        try
        {
            await process.WaitForExitAsync(timeoutCts.Token);
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
        {
            await KillProcessTreeAsync(process);
            var stdoutTail = await ReadOutputTailAsync(stdoutTask);
            var stderrTail = await ReadOutputTailAsync(stderrTask);
            throw new TimeoutException(
                $"Process timed out after {timeout.TotalSeconds:0} seconds.{Environment.NewLine}" +
                $"Command: {DescribeCommand(startInfo)}{Environment.NewLine}" +
                $"Stdout tail:{Environment.NewLine}{stdoutTail}{Environment.NewLine}" +
                $"Stderr tail:{Environment.NewLine}{stderrTail}");
        }

        var stdout = await stdoutTask;
        var stderr = await stderrTask;
        return (process.ExitCode, stdout, stderr);
    }

    private static async Task KillProcessTreeAsync(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                try
                {
                    process.Kill(entireProcessTree: true);
                }
                catch (NotSupportedException)
                {
                    if (!process.HasExited)
                        process.Kill();
                }
            }
        }
        catch (InvalidOperationException)
        {
            return;
        }

        try
        {
            await Task.WhenAny(process.WaitForExitAsync(), Task.Delay(TimeSpan.FromSeconds(5)));
        }
        catch (InvalidOperationException)
        {
        }
    }

    private static async Task<string> ReadOutputTailAsync(Task<string> outputTask)
    {
        var completedTask = await Task.WhenAny(outputTask, Task.Delay(TimeSpan.FromSeconds(5)));
        if (completedTask != outputTask)
            return "<output was not fully drained after process kill>";

        try
        {
            return Tail(await outputTask);
        }
        catch (Exception ex)
        {
            return $"<output read failed: {ex.Message}>";
        }
    }

    private static string DescribeCommand(ProcessStartInfo startInfo)
    {
        var arguments = startInfo.ArgumentList.Count > 0
            ? string.Join(" ", startInfo.ArgumentList.Select(QuoteArgument))
            : startInfo.Arguments;
        return $"{startInfo.FileName} {arguments}".Trim();
    }

    private static string QuoteArgument(string argument)
    {
        if (argument.Length == 0)
            return "\"\"";

        if (!argument.Any(char.IsWhiteSpace) && !argument.Contains('"'))
            return argument;

        return $"\"{argument.Replace("\"", "\\\"")}\"";
    }

    private static string Tail(string value, int maxChars = 4000)
        => value.Length <= maxChars ? value : value[^maxChars..];

    private static void WriteJsonDocument(string path, object value)
        => File.WriteAllText(
            path,
            JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true }));

    private static void WriteJsonl(string path, IEnumerable<object> rows)
        => File.WriteAllText(
            path,
            string.Join(
                Environment.NewLine,
                rows.Select(row => JsonSerializer.Serialize(row))));

    private static IReadOnlyList<JsonElement> ReadJsonlDocuments(string path)
    {
        var rows = new List<JsonElement>();
        foreach (var line in File.ReadLines(path))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            using var document = JsonDocument.Parse(line);
            rows.Add(document.RootElement.Clone());
        }

        return rows;
    }

    private static void AssertFallbackWordingIsNonSemantic(XiigenExtensionResponse response)
    {
        var evidence = response.CheckSetResults["skill_selection_evidence"];
        Assert.Contains("HOST_REQUESTED_SKILL_SURFACE_FALLBACK_NO_SEMANTIC_AUTHORITY", evidence, StringComparison.Ordinal);
        Assert.Contains("HOST_REQUESTED_SKILL_SURFACE_FALLBACK_NO_MODEL_MUTATION", evidence, StringComparison.Ordinal);
        Assert.DoesNotContain("HOST_REQUESTED_SKILL_SURFACE_FROM_TASK_SEMANTICS", evidence, StringComparison.Ordinal);
        Assert.DoesNotContain("semantic_task", evidence, StringComparison.OrdinalIgnoreCase);

        var rationale = response.CheckSetResults["skill_selection_method_rationale"];
        Assert.Contains("host_requested_available_skill_surface_fallback_no_semantic_authority", rationale, StringComparison.Ordinal);
        Assert.Contains("HOST_REQUESTED_SKILL_SURFACE_FALLBACK_NO_MODEL_MUTATION", rationale, StringComparison.Ordinal);
        Assert.DoesNotContain("semantic_task", rationale, StringComparison.OrdinalIgnoreCase);

        Assert.DoesNotContain("semantic skills", response.ChatDisplayText, StringComparison.OrdinalIgnoreCase);
        if (response.ChatDisplayText.Contains("Skill-selection", StringComparison.OrdinalIgnoreCase))
            Assert.Contains("host-requested available skills", response.ChatDisplayText, StringComparison.OrdinalIgnoreCase);

        var taskToSkills = response.CheckSetResults["task_to_skills"];
        Assert.Contains("no semantic authority", taskToSkills, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no model mutation", taskToSkills, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("semantic_task", taskToSkills, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("semantic skills", taskToSkills, StringComparison.OrdinalIgnoreCase);
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current is not null)
        {
            if (Directory.Exists(Path.Combine(current.FullName, "43.SmartSkillsDomain"))
                && Directory.Exists(Path.Combine(current.FullName, "artifacts")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        return Directory.GetCurrentDirectory();
    }
}
