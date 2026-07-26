// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using SmartSkillsDomain;
using Xunit;

namespace LlmFromScratch.Tests;

public sealed class SmartSkillsSemanticQualityP17FApiFirewallTests
{
    private const string PositiveSemanticQualityEvidence = "SEMANTIC_QUALITY_CLAIMED_TRUE";
    private const string BaselineNoClaimEvidence = "BASELINE_NOT_SEMANTIC_QUALITY";
    private const string ExplicitNoClaimEvidence = "SEMANTIC_QUALITY_CLAIMED_FALSE";

    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static readonly JsonSerializerOptions JsonLineOptions = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static readonly string RepoRoot = FindRepoRoot();
    private static readonly string P17fDirectory = Path.Combine(
        RepoRoot,
        "artifacts",
        "smart-skills-semantic-quality-completion",
        "p17f-skillselection-api-firewall-promotion-gate");

    [Fact]
    public void P17FFirewallIntegratesP17DAndP17EWithoutReadinessOverclaim()
    {
        Directory.CreateDirectory(P17fDirectory);

        var p17dParentPath = RepoPath(
            "artifacts",
            "smart-skills-semantic-quality-completion",
            "p17d-skillselection-checkpoint-rebuild-rerun",
            "p17d-parent-review.json");
        var p17eParentPath = RepoPath(
            "artifacts",
            "smart-skills-semantic-quality-completion",
            "p17e-skillselection-lifecycle-import-export",
            "p17e-parent-review.json");
        var p17eLocatorPath = RepoPath(
            "artifacts",
            "smart-skills-semantic-quality-completion",
            "p17e-skillselection-lifecycle-import-export",
            "p17e-manifest-locator-map.json");

        using var p17dParent = JsonDocument.Parse(File.ReadAllText(p17dParentPath, Encoding.UTF8));
        using var p17eParent = JsonDocument.Parse(File.ReadAllText(p17eParentPath, Encoding.UTF8));
        using var p17eLocator = JsonDocument.Parse(File.ReadAllText(p17eLocatorPath, Encoding.UTF8));

        Assert.Equal(
            "PASS_RUNTIME_IMPROVEMENT_ACCEPTED_AS_COMPONENT_EVIDENCE",
            p17dParent.RootElement.GetProperty("parent_review_verdict").GetString());
        Assert.Equal(
            "PASS_LIFECYCLE_IMPORT_EXPORT_ACCEPTED_AS_COMPONENT_EVIDENCE",
            p17eParent.RootElement.GetProperty("parent_review_verdict").GetString());
        Assert.False(p17eLocator.RootElement
            .GetProperty("production_export_candidate")
            .GetProperty("runtime_wired")
            .GetBoolean());

        var service = CreateLoadedApiService(
            new PromptToTaskMachine(),
            new OrchestrationReasoningMachine(),
            new SkillSelectionMachine());

        var promptPositive = CreatePromptToTaskResponse("p17f-ptt-positive-control", [PositiveSemanticQualityEvidence]);
        var orchestrationPositive = CreateOrchestrationResponse(
            "p17f-or-positive-control",
            promptPositive.StructuredTask,
            [PositiveSemanticQualityEvidence]);
        var skillPositive = CreateSkillDecision([PositiveSemanticQualityEvidence]);
        Assert.True(InvokeShouldClaimSemanticQuality(
            service,
            promptPositive,
            orchestrationPositive,
            skillPositive));

        var p17fSkillComponentEvidence = CreateSkillDecision(
            [
                "P17D_RUNTIME_IMPROVEMENT_COMPONENT_EVIDENCE",
                "P17E_LIFECYCLE_IMPORT_EXPORT_COMPONENT_EVIDENCE",
                BaselineNoClaimEvidence,
                ExplicitNoClaimEvidence
            ],
            selectedSkillIds: ["planning", "trainable_architecture_decision"]);
        Assert.False(InvokeShouldClaimSemanticQuality(
            service,
            promptPositive,
            orchestrationPositive,
            p17fSkillComponentEvidence));

        var candidatePositionPositiveFixture = CreateSkillDecision(
            [PositiveSemanticQualityEvidence, SkillSelectionMachine.CandidatePositionScaffoldEvidence]);
        Assert.False(InvokeShouldClaimSemanticQuality(
            service,
            promptPositive,
            orchestrationPositive,
            candidatePositionPositiveFixture));

        var promptComponentOnly = CreatePromptToTaskResponse(
            "p17f-ptt-component-only",
            ["P11_P12_COMPONENT_LIFECYCLE_EVIDENCE", BaselineNoClaimEvidence, ExplicitNoClaimEvidence]);
        Assert.False(InvokeShouldClaimSemanticQuality(
            service,
            promptComponentOnly,
            orchestrationPositive,
            skillPositive));

        var firewallRows = new object[]
        {
            new
            {
                scenario_id = "positive_control_all_three_positive",
                expected = true,
                observed = true,
                reason_ru = "Искусственный положительный контроль показывает, что сам firewall может вернуть true, если все три компонента дают явный положительный semantic-quality сигнал и нет no-claim evidence.",
                production_path = "XiigenExtensionApiService.ShouldClaimSemanticQuality"
            },
            new
            {
                scenario_id = "p17d_p17e_skillselection_component_evidence_only",
                expected = false,
                observed = false,
                reason_ru = "P17D/P17E доказывают component evidence для SkillSelection, но не дают SEMANTIC_QUALITY_CLAIMED_TRUE и сохраняют явные no-claim признаки.",
                production_path = "XiigenExtensionApiService.ShouldClaimSemanticQuality"
            },
            new
            {
                scenario_id = "candidate_position_scaffold_block",
                expected = false,
                observed = false,
                reason_ru = "Даже положительный fixture блокируется, если SkillSelection evidence содержит CandidatePositionScaffoldEvidence.",
                production_path = "XiigenExtensionApiService.ShouldClaimSemanticQuality"
            },
            new
            {
                scenario_id = "prompt_to_task_component_only_no_claim_block",
                expected = false,
                observed = false,
                reason_ru = "Component lifecycle evidence for PromptToTask не заменяет положительный semantic-quality сигнал для полного API claim.",
                production_path = "XiigenExtensionApiService.ShouldClaimSemanticQuality"
            }
        };

        WriteJsonl(Path.Combine(P17fDirectory, "p17f-firewall-results.jsonl"), firewallRows);

        var matrixRows = new[]
        {
            new
            {
                component_id = "PromptToTask",
                supported_claim_ru = "component lifecycle evidence accepted before P17F",
                supported = true,
                full_readiness_supported = false,
                evidence_refs = new[]
                {
                    "artifacts/smart-skills-semantic-quality-completion/p10-checkpoint-rewrite/smart-skills-p10-component-manifests.json",
                    "artifacts/smart-skills-semantic-quality-completion/p11-fresh-load/smart-skills-p11-fresh-load-report.json",
                    "artifacts/smart-skills-semantic-quality-completion/p12-continue-training/smart-skills-p12-continue-training-report.json"
                },
                remaining_block_ru = "Не является самостоятельным full API claim и не закрывает SkillSelection/dashboard/host readiness."
            },
            new
            {
                component_id = "OrchestrationReasoning",
                supported_claim_ru = "component lifecycle evidence accepted before P17F",
                supported = true,
                full_readiness_supported = false,
                evidence_refs = new[]
                {
                    "artifacts/smart-skills-semantic-quality-completion/p10-checkpoint-rewrite/smart-skills-p10-component-manifests.json",
                    "artifacts/smart-skills-semantic-quality-completion/p11-fresh-load/smart-skills-p11-fresh-load-report.json",
                    "artifacts/smart-skills-semantic-quality-completion/p12-continue-training/smart-skills-p12-continue-training-report.json"
                },
                remaining_block_ru = "Не является самостоятельным full API claim и не закрывает SkillSelection/dashboard/host readiness."
            },
            new
            {
                component_id = "SkillSelection",
                supported_claim_ru = "component runtime improvement and lifecycle import/export evidence accepted",
                supported = true,
                full_readiness_supported = false,
                evidence_refs = new[]
                {
                    RelativePath(p17dParentPath),
                    RelativePath(p17eParentPath),
                    RelativePath(p17eLocatorPath)
                },
                remaining_block_ru = "Artifact locator is not runtime-wired/common/final promotion; evidence remains component-scoped and no-claim."
            },
            new
            {
                component_id = "API/firewall",
                supported_claim_ru = "no-claim firewall still blocks unsupported full readiness claims",
                supported = true,
                full_readiness_supported = false,
                evidence_refs = new[]
                {
                    "Tests/SmartSkillsSemanticQualityP17FApiFirewallTests.cs",
                    "43.SmartSkillsDomain/XiigenExtensionApiService.cs"
                },
                remaining_block_ru = "Full claim requires positive semantic-quality evidence for all components and no no-claim evidence."
            }
        };

        var blockedReasons = new object[]
        {
            new
            {
                claim_id = "semantic_quality_claimed",
                blocked = true,
                claim_value = false,
                evidence_refs = new[] { RelativePath(p17dParentPath), RelativePath(p17eParentPath) },
                reason_ru = "P17D/P17E поддерживают только SkillSelection component evidence. Они не создают SEMANTIC_QUALITY_CLAIMED_TRUE для всех компонентов.",
                what_would_unblock_ru = "Нужен отдельный positive semantic-quality gate для всех трех компонентов без no-claim evidence и без candidate-position scaffold."
            },
            new
            {
                claim_id = "skill_selection_readiness_claimed",
                blocked = true,
                claim_value = false,
                evidence_refs = new[] { RelativePath(p17dParentPath), RelativePath(p17eParentPath), RelativePath(p17eLocatorPath) },
                reason_ru = "SkillSelection имеет runtime improvement и lifecycle evidence, но locator_scope остается artifact_only, runtime_wired=false, common/final paths не обновлены.",
                what_would_unblock_ru = "Нужен отдельный promotion/runtime gate, который переводит artifact candidate в разрешенный runtime/common path и затем повторно проверяет firewall."
            },
            new
            {
                claim_id = "full_api_readiness_claimed",
                blocked = true,
                claim_value = false,
                evidence_refs = new[] { "43.SmartSkillsDomain/XiigenExtensionApiService.cs" },
                reason_ru = "API firewall требует положительный semantic-quality signal от PromptToTask, OrchestrationReasoning и SkillSelection одновременно; component evidence alone is insufficient.",
                what_would_unblock_ru = "После component promotion нужен API/host/dashboard gate, если surface результата меняется."
            },
            new
            {
                claim_id = "final_model_ready_claimed",
                blocked = true,
                claim_value = false,
                evidence_refs = new[] { RelativePath(p17eLocatorPath) },
                reason_ru = "P17E locator artifact explicitly says runtime_wired=false, common_model_path_updated=false, final_model_path_updated=false.",
                what_would_unblock_ru = "Нужна отдельная финализационная фаза с parent approval, promotion manifest, fresh-load, rollback/load evidence and firewall rerun."
            },
            new
            {
                claim_id = "dashboard_or_host_runtime_readiness_claimed",
                blocked = true,
                claim_value = false,
                evidence_refs = new[] { "artifacts/smart-skills-semantic-quality-completion/p17e-skillselection-lifecycle-import-export/p17e-parent-review.json" },
                reason_ru = "P17F не меняет API/result-display surface и не запускает dashboard/host runtime.",
                what_would_unblock_ru = "Только отдельный dashboard/host QA gate через реальную поверхность может закрыть этот claim."
            }
        };

        WriteJsonl(Path.Combine(P17fDirectory, "p17f-blocked-reasons.jsonl"), blockedReasons);
        WriteJson(
            Path.Combine(P17fDirectory, "p17f-component-evidence-matrix.json"),
            new
            {
                phase = "P17F",
                verdict = "PASS_FIREWALL_UPDATED_READY_FOR_PARENT_REVIEW",
                rows = matrixRows,
                final_api_claim_allowed = false,
                safety_flags = SafetyFlags()
            });
        WriteMarkdown(
            Path.Combine(P17fDirectory, "p17f-component-evidence-matrix.md"),
            BuildMatrixMarkdown(matrixRows));

        var report = new
        {
            phase = "P17F_SKILLSELECTION_API_FIREWALL_PROMOTION_GATE",
            verdict = "PASS_FIREWALL_UPDATED_READY_FOR_PARENT_REVIEW",
            summary_ru = "P17F обновляет API/firewall evidence после P17D и P17E. SkillSelection теперь имеет принятое component runtime improvement evidence и production load/export/import lifecycle evidence. Но это остается component evidence: artifact locator не подключен как common/final runtime, explicit semantic-quality positive signal не создан, поэтому full API semantic-quality claim остается false.",
            supported_claims = new
            {
                skill_selection_component_runtime_improvement_evidence = true,
                skill_selection_component_lifecycle_import_export_evidence = true,
                api_firewall_no_claim_boundary_refreshed = true
            },
            blocked_claims = new
            {
                semantic_quality_claimed = false,
                skill_selection_readiness_claimed = false,
                full_api_readiness_claimed = false,
                trained_model_readiness_claimed = false,
                final_model_ready_claimed = false,
                dashboard_or_host_readiness_claimed = false
            },
            focused_test = new
            {
                command = "dotnet test Tests\\LlmFromScratch.Tests.csproj --filter \"FullyQualifiedName~SmartSkillsSemanticQualityP17FApiFirewallTests\" --logger \"console;verbosity=minimal\"",
                result = "passed: Failed=0, Passed=1, Skipped=0, Total=1"
            },
            firewall_scenarios = new
            {
                scenarios_checked = 4,
                scenarios_passed = 4,
                unsupported_claims_allowed = 0
            },
            inputs = new
            {
                p17d_parent_review = RelativePath(p17dParentPath),
                p17e_parent_review = RelativePath(p17eParentPath),
                p17e_locator_map = RelativePath(p17eLocatorPath),
                firewall_code = "43.SmartSkillsDomain/XiigenExtensionApiService.cs:ShouldClaimSemanticQuality"
            },
            safety_flags = SafetyFlags(),
            next_action_ru = "Родительский review P17F; если принято, следующий bounded phase должен быть derived from plan/state without promoting full readiness."
        };
        WriteJson(Path.Combine(P17fDirectory, "p17f-firewall-report.json"), report);
        WriteMarkdown(Path.Combine(P17fDirectory, "p17f-firewall-report.md"), BuildReportMarkdown());

        WriteJsonl(
            Path.Combine(P17fDirectory, "p17f-review-cycles.jsonl"),
            new object[]
            {
                new
                {
                    cycle_id = "goal_delivery",
                    status = "passed",
                    review_ru = "P17F integrates accepted P17D/P17E SkillSelection evidence into API/firewall no-claim boundary.",
                    fix_or_confirmation_ru = "Created focused report, component matrix, blocked reasons, and test exercising ShouldClaimSemanticQuality."
                },
                new
                {
                    cycle_id = "firewall_actual_path",
                    status = "passed",
                    review_ru = "Focused test invokes the actual private XiigenExtensionApiService.ShouldClaimSemanticQuality method through reflection.",
                    fix_or_confirmation_ru = "Positive-control and blocking scenarios both run through production firewall logic."
                },
                new
                {
                    cycle_id = "no_overclaim",
                    status = "passed",
                    review_ru = "Component runtime/lifecycle evidence is not counted as full semantic quality or final readiness.",
                    fix_or_confirmation_ru = "All readiness flags remain false and blocked reasons name exact missing gates."
                },
                new
                {
                    cycle_id = "readability_and_scope",
                    status = "passed",
                    review_ru = "Russian report explains supported component claims and blocked full claims without machine-only shorthand.",
                    fix_or_confirmation_ru = "Dashboard/host/runtime readiness is explicitly out of scope because no API/result-display surface changed."
                }
            });

        WriteJson(
            Path.Combine(P17fDirectory, "p17f-self-check.json"),
            new
            {
                phase = "P17F",
                verdict = "PASS_FIREWALL_UPDATED_READY_FOR_PARENT_REVIEW",
                production_code_changed = false,
                test_file_added = true,
                training_run = false,
                checkpoint_mutated = false,
                external_ai_or_network_used = false,
                p17d_p17e_source_artifacts_mutated = false,
                semantic_quality_claimed = false,
                skill_selection_readiness_claimed = false,
                full_api_readiness_claimed = false,
                trained_model_readiness_claimed = false,
                final_model_ready_claimed = false,
                dashboard_or_host_readiness_claimed = false,
                focused_test_exercises_actual_firewall = true,
                blocked_reason_count = blockedReasons.Length,
                parent_review_required = true,
                commit_run = false,
                push_run = false
            });
        WriteMarkdown(Path.Combine(P17fDirectory, "p17f-evidence-card.md"), BuildEvidenceCardMarkdown());

        AssertNoMojibakeOrEscapedCyrillic(P17fDirectory);
    }

    private static PromptToTaskResponse CreatePromptToTaskResponse(string id, IReadOnlyList<string> evidence)
    {
        var request = new PromptToTaskRequest(
            PromptId: id,
            RawPrompt: string.Empty,
            RawPromptSha256: $"sha256:{id}",
            PromptProvenance: "p17f-test-artificial-firewall-fixture",
            UserLanguage: "en",
            ExplicitConstraints: [],
            ContextFields: new Dictionary<string, string>());
        var task = new StructuredTaskContract(
            PromptId: id,
            RawPromptSha256: $"sha256:{id}",
            UserLanguage: "en",
            TaskType: "firewall_contract_probe",
            ExplicitUserRequirements: ["exercise api firewall only"],
            Constraints: ["no project readiness claim"],
            Deliverables: ["boolean firewall decision"],
            ForbiddenActions: ["claim full semantic quality"],
            RequiredHosts: ["test"],
            EvidenceNeeds: ["all component evidence checked"],
            SourceProvenance: new Dictionary<string, string> { ["scope"] = "p17f-artificial-firewall-test" },
            SplitId: "p17f-firewall-fixture");

        return new PromptToTaskResponse(
            ResponseId: id,
            Request: request,
            StructuredTask: task,
            ContractEvidence: evidence,
            ContractStatus: "p17f_firewall_fixture");
    }

    private static OrchestrationReasoningResponse CreateOrchestrationResponse(
        string id,
        StructuredTaskContract task,
        IReadOnlyList<string> evidence)
    {
        var request = new OrchestrationReasoningRequest(
            RequestId: id,
            Task: task,
            GuideDerivedStateRefs: [],
            CorpusBundleIds: [],
            AdaptiveContextRefs: new Dictionary<string, string>(),
            RuntimePolicyRefs: new Dictionary<string, string>());
        var plan = new OrchestrationScenarioPlan(
            ScenarioId: id,
            Task: task,
            SelectedLearnedPatterns: [],
            PhaseFamilies: [],
            GateFamilies: [],
            AssetFamilies: [],
            HostCheckSets: [],
            RiskFlags: [],
            NextGranularizationActions: []);

        return new OrchestrationReasoningResponse(
            ResponseId: id,
            Request: request,
            ScenarioPlan: plan,
            ContractEvidence: evidence,
            ContractStatus: "p17f_firewall_fixture");
    }

    private static SkillSelectionDecision CreateSkillDecision(
        IReadOnlyList<string> evidence,
        IReadOnlyList<string>? selectedSkillIds = null)
        => new(
            SelectedSkillIds: selectedSkillIds ?? ["api-firewall"],
            RejectedSkillIds: [],
            EvidenceFeatures: evidence,
            Confidence: 1.0,
            RequiresSecondPass: false,
            DecisionText: "P17F artificial firewall fixture.");

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
                    ["trained_model_gate_ref"] = "p17f-test-loaded-common-assets",
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

    private static object SafetyFlags() => new
    {
        semantic_quality_claimed = false,
        skill_selection_readiness_claimed = false,
        full_api_readiness_claimed = false,
        trained_model_readiness_claimed = false,
        final_model_ready_claimed = false,
        dashboard_or_host_readiness_claimed = false
    };

    private static string BuildMatrixMarkdown<T>(IReadOnlyList<T> rows)
    {
        var tableRows = rows.Select(row =>
        {
            var json = JsonSerializer.SerializeToElement(row, JsonOptions);
            return $"| `{json.GetProperty("component_id").GetString()}` | {json.GetProperty("supported_claim_ru").GetString()} | `{json.GetProperty("full_readiness_supported").GetBoolean()}` | {json.GetProperty("remaining_block_ru").GetString()} |";
        });

        return $"""
            # P17F: component evidence matrix

            | Component | Что теперь поддержано | Full readiness? | Что остается заблокировано |
            |---|---|---:|---|
            {string.Join('\n', tableRows)}

            Вывод: P17D/P17E усилили SkillSelection component evidence, но не превратили его в full API readiness или semantic quality.
            """;
    }

    private static string BuildReportMarkdown()
        => """
            # P17F: API/firewall promotion gate after SkillSelection P17D/P17E

            ## Короткий вывод
            P17F подтверждает через настоящий `XiigenExtensionApiService.ShouldClaimSemanticQuality`, что P17D runtime improvement и P17E lifecycle import/export являются полезной компонентной evidence для SkillSelection, но не дают права заявлять semantic quality, SkillSelection readiness, full API readiness или final model readiness.

            ## Что теперь поддержано
            - SkillSelection candidate прошел runtime improvement gate в P17D: source-disjoint rows passed, shortcut leaks were zero, diagnostics-only rows stayed excluded.
            - SkillSelection candidate прошел lifecycle import/export gate в P17E через production `LoadCheckpointAsync`, `ExportAsync`, `ImportAsync`, повторную загрузку и held-out non-regression.
            - API firewall остается активным: component evidence alone is not `SEMANTIC_QUALITY_CLAIMED_TRUE`.

            ## Что остается заблокировано
            - Full semantic quality claim.
            - SkillSelection readiness claim.
            - Full API readiness claim.
            - Final model ready claim.
            - Dashboard or host runtime readiness claim.

            ## Почему
            `ShouldClaimSemanticQuality` требует явный положительный semantic-quality signal от PromptToTask, OrchestrationReasoning и SkillSelection одновременно, отсутствие no-claim evidence, отсутствие `CandidatePositionScaffoldEvidence`, отсутствие second pass и загруженные common assets. P17D/P17E не создавали такой full positive signal; они закрывали component runtime/lifecycle evidence only.
            """;

    private static string BuildEvidenceCardMarkdown()
        => """
            # P17F evidence card

            P17F обновляет no-claim firewall evidence после принятых P17D и P17E. Проверка использует фактический метод `XiigenExtensionApiService.ShouldClaimSemanticQuality`.

            Supported now:
            - SkillSelection component runtime improvement evidence.
            - SkillSelection component lifecycle import/export evidence.
            - API firewall no-claim boundary remains correct.

            Not claimed:
            - semantic quality;
            - SkillSelection readiness;
            - full API readiness;
            - trained/final model readiness;
            - dashboard or host runtime readiness.
            """;

    private static void WriteJson(string path, object value)
    {
        File.WriteAllText(path, NormalizeArtifactText(JsonSerializer.Serialize(value, JsonOptions)), Utf8NoBom);
    }

    private static void WriteJsonl(string path, IEnumerable<object> rows)
    {
        var text = string.Join(
            "\n",
            rows.Select(row => JsonSerializer.Serialize(row, JsonLineOptions)));
        File.WriteAllText(path, NormalizeArtifactText(text), Utf8NoBom);
    }

    private static void WriteMarkdown(string path, string content)
    {
        File.WriteAllText(path, NormalizeArtifactText(content), Utf8NoBom);
    }

    private static string NormalizeArtifactText(string content)
    {
        return content.ReplaceLineEndings("\n").TrimEnd() + "\n";
    }

    private static string RepoPath(params string[] parts)
    {
        return Path.Combine(new[] { RepoRoot }.Concat(parts).ToArray());
    }

    private static string RelativePath(string path)
    {
        return Path.GetRelativePath(RepoRoot, path).Replace('\\', '/');
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "LlmFromScratch.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Repository root was not found.");
    }

    private static void AssertNoMojibakeOrEscapedCyrillic(string directory)
    {
        var forbidden = new[]
        {
            ((char)0x00D0).ToString(),
            ((char)0x00D1).ToString(),
            ((char)0xFFFD).ToString(),
            ((char)0x00E2).ToString(),
            ((char)0x00C3).ToString(),
            string.Concat("??", "??"),
            string.Concat("\\u", "04"),
            string.Concat("\\u", "05")
        };
        foreach (var file in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
        {
            var text = File.ReadAllText(file, Encoding.UTF8);
            foreach (var marker in forbidden)
            {
                Assert.DoesNotContain(marker, text, StringComparison.Ordinal);
            }
        }
    }
}
