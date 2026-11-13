// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text;
using System.Text.Json;
using SmartSkillsDomain;

[Collection(ExtensionRuntimeTokenQualityArtifactPaths.CollectionName)]
public sealed class ExtensionQualitySemanticGates19TargetLocatorPlumbingTests
{
    private const string PhaseId = "QUALITY-SEMANTIC-GATES-19";
    private const string PreviousPhaseId = "QUALITY-SEMANTIC-GATES-18";
    private const string NextPhaseId = "QUALITY-SEMANTIC-GATES-20";
    private const string Command = "dotnet test Tests\\LlmFromScratch.Tests.csproj --filter FullyQualifiedName~ExtensionQualitySemanticGates19TargetLocatorPlumbingTests --no-restore --nologo";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private static readonly JsonSerializerOptions JsonLineOptions = new(JsonSerializerDefaults.Web);

    private static readonly UTF8Encoding Utf8NoBom = new(false);

    private static readonly string[] ExpectedLocatorRefMachines =
    [
        "PromptToTaskMachine",
        "OrchestrationReasoningMachine",
        "SkillSelectionMachine"
    ];

    private static readonly string RepoRoot = ExtensionRuntimeTokenQualityArtifactPaths.RepoRoot;

    private const string Qsg19CanonicalProjectsParentWindows = @"C:\Projects";

    private const string Qsg19CoreRepoName = "llm_mvp_core";

    // Родитель runtime-проектов выводится из текущего RepoRoot, чтобы Windows и WSL читали один и тот же C:\Projects в своём формате.
    private static readonly string RuntimeProjectsParent = GetRuntimeProjectsParent();

    // Whitelist намеренно содержит только шесть source/launcher файлов трёх host-репозиториев, а не общий доступ к C:\Projects.
    private static readonly IReadOnlyDictionary<string, string> AllowedQsg19HostSourceTails =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [NormalizeQsg19WindowsHostSourceRef(@"C:\Projects\claude-xiigen-smart-skills\src\claude-entrypoint.js")] = "claude-xiigen-smart-skills/src/claude-entrypoint.js",
            [NormalizeQsg19WindowsHostSourceRef(@"C:\Projects\claude-xiigen-smart-skills\src\claude-entrypoint.ps1")] = "claude-xiigen-smart-skills/src/claude-entrypoint.ps1",
            [NormalizeQsg19WindowsHostSourceRef(@"C:\Projects\cursor-xiigen-smart-skills\src\extension.ts")] = "cursor-xiigen-smart-skills/src/extension.ts",
            [NormalizeQsg19WindowsHostSourceRef(@"C:\Projects\cursor-xiigen-smart-skills\dist\extension.js")] = "cursor-xiigen-smart-skills/dist/extension.js",
            [NormalizeQsg19WindowsHostSourceRef(@"C:\Projects\codex-xiigen-smart-skills\src\codex-entrypoint.js")] = "codex-xiigen-smart-skills/src/codex-entrypoint.js",
            [NormalizeQsg19WindowsHostSourceRef(@"C:\Projects\codex-xiigen-smart-skills\src\codex-entrypoint.ps1")] = "codex-xiigen-smart-skills/src/codex-entrypoint.ps1"
        };

    // Известные host repo отделены от whitelist, чтобы ошибка "repo известен, но файл не разрешён" оставалась fail-closed и понятной.
    private static readonly HashSet<string> KnownQsg19HostRepoNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "claude-xiigen-smart-skills",
        "cursor-xiigen-smart-skills",
        "codex-xiigen-smart-skills"
    };

    private static readonly string ArtifactDir = ExtensionRuntimeTokenQualityArtifactPaths.ArtifactDir;

    private static readonly string StatePath = ExtensionRuntimeTokenQualityArtifactPaths.StatePath;

    [Fact]
    public void QualitySemanticGates19_VerifiesTargetLocatorPlumbingAndKeepsReadinessClosed()
    {
        var activeLocatorPath = Path.Combine(ArtifactDir, "quality-semantic-gates-17-test-only-active-locator.json");
        var qsg18ReportPath = Path.Combine(ArtifactDir, "quality-semantic-gates-18-report.json");
        var activeLocator = ReadJson(activeLocatorPath);
        var targetRoot = activeLocator.RootElement.GetProperty("activated_asset_root").GetString()!;
        var rollbackRoot = targetRoot.Replace("/user-cache-continued", "/rollback", StringComparison.Ordinal);
        var targetAssetRootMaterialized = EnsureTargetAssetRootFromUserCache(targetRoot);
        var rollbackAssetRootMaterialized = EnsureRollbackAssetRootFromRepoCommon(rollbackRoot);
        var locator = BuildTargetLocator(activeLocator, targetRoot, rollbackRoot, "target");
        var rollbackLocator = BuildTargetLocator(activeLocator, targetRoot, rollbackRoot, "rollback");

        var service = new XiigenExtensionApiService(RepoRoot);
        var positive = service.ProcessHostExtensionRequest(CreateRequest("claude-code", "qsg19-positive", locator, true)).Value!;
        var missing = service.ProcessHostExtensionRequest(CreateRequest("cursor", "qsg19-missing", null, true)).Value!;
        var invalidLocator = locator with { UserAdaptiveAssetRoot = "artifacts/extension-runtime-token-quality-learnability/missing-qsg19-root" };
        var invalid = service.ProcessHostExtensionRequest(CreateRequest("codex", "qsg19-invalid", invalidLocator, true)).Value!;
        var rollback = service.ProcessHostExtensionRequest(CreateRequest("codex", "qsg19-rollback", rollbackLocator, true)).Value!;
        AssertLocatorRefsPreserved(activeLocator, positive.CheckSetResults);
        var hostRows = BuildHostRows(activeLocator, positive);
        var rollbackClosure = BuildRollbackClosure(rollback);
        var claims = BuildClaimRows(hostRows, positive, missing, invalid, rollback, rollbackClosure);

        Assert.True(positive.Success);
        Assert.Equal("true", positive.CheckSetResults["target_locator_bound"]);
        Assert.Equal("true", positive.CheckSetResults["user_adaptive_any_asset_loaded"]);
        Assert.Contains("qsg16-locator-update-proposal-v1", positive.DebugTrace!.InputPayloadText);
        Assert.DoesNotContain("QSG19 private prompt", positive.DebugTrace.InputPayloadText);

        Assert.False(missing.Success);
        Assert.Contains("target_locator_required", missing.Errors);
        Assert.False(invalid.Success);
        Assert.Contains("target_locator_user_adaptive_asset_root_not_found", invalid.Errors);
        Assert.True(rollback.Success);
        Assert.Equal("rollback", rollback.CheckSetResults["target_locator_mode"]);
        Assert.Equal("true", rollback.CheckSetResults["target_locator_rollback_requested"]);
        Assert.All(hostRows, row => Assert.True(row.plumbing_verified));
        Assert.All(hostRows, row => Assert.True(row.proposed_locator_refs_preserved));
        Assert.All(hostRows, row => Assert.True(row.locator_refs_not_object_object));
        Assert.All(hostRows, row => Assert.Equal(ExpectedLocatorRefMachines.Length, row.expected_locator_ref_count));

        var hostRowsPath = Path.Combine(ArtifactDir, "quality-semantic-gates-19-host-target-locator-plumbing-rows.jsonl");
        var negativeRowsPath = Path.Combine(ArtifactDir, "quality-semantic-gates-19-negative-paths.jsonl");
        var rollbackPath = Path.Combine(ArtifactDir, "quality-semantic-gates-19-rollback-closure.json");
        var readinessPath = Path.Combine(ArtifactDir, "quality-semantic-gates-19-final-readiness-checks.jsonl");
        var reportPath = Path.Combine(ArtifactDir, "quality-semantic-gates-19-report.json");
        var validationPath = Path.Combine(ArtifactDir, "quality-semantic-gates-19.validation.json");
        var markdownPath = Path.Combine(ArtifactDir, "quality-semantic-gates-19-report.md");
        var statePatchPath = ExtensionExecutionStatePatchWriter.DefaultPatchPath(ArtifactDir, PhaseId);

        WriteJsonl(hostRowsPath, hostRows);
        WriteJsonl(negativeRowsPath, BuildNegativeRows(missing, invalid, rollback));
        WriteJson(rollbackPath, rollbackClosure);
        WriteJsonl(readinessPath, claims);
        WriteJson(reportPath, new
        {
            schema = "xiigen.quality_semantic_gates_19.report.v1",
            phase_id = PhaseId,
            input_phase_id = PreviousPhaseId,
            qsg18_report_ref = Relative(qsg18ReportPath),
            active_locator_ref = Relative(activeLocatorPath),
            host_path_evidence_level = "handler_or_adapter_test_only_no_live_ui_chat_claim",
            core_target_locator_positive_passed = positive.Success,
            missing_locator_negative_passed = !missing.Success && missing.Errors.Contains("target_locator_required"),
            invalid_locator_negative_passed = !invalid.Success && invalid.Errors.Contains("target_locator_user_adaptive_asset_root_not_found"),
            rollback_load_probe_passed = rollback.Success && rollback.CheckSetResults["target_locator_mode"] == "rollback",
            proposed_locator_refs_expected = ExpectedLocatorRefMachines.Length,
            proposed_locator_refs_preserved = HasAllExpectedLocatorRefs(positive.CheckSetResults),
            object_object_locator_refs_rejected = !ContainsObjectObject(positive.CheckSetResults.Values),
            host_locator_ref_rows_passed = hostRows.All(row => row.proposed_locator_refs_preserved == true && row.locator_refs_not_object_object == true),
            target_asset_root_materialized_from_user_cache = targetAssetRootMaterialized,
            rollback_asset_root_materialized_from_repo_common = rollbackAssetRootMaterialized,
            live_ui_chat_claimed = false,
            production_locator_promotion_done = false,
            production_rollback_closure_done = false,
            rollback_closure_done = false,
            rollback_closure_reason = "rollback load probe exists, but QSG-19 did not run true live UI/chat production-locator closure.",
            semantic_quality_claimed = false,
            runtime_extension_claimed = false,
            final_readiness_claimed = false,
            next_phase_id = NextPhaseId,
            next_action = "Run true live Claude/Cursor/Codex UI/chat target-bound probes with QSG-19 TargetLocator and then promote/rollback only if production-like locator closure passes.",
            artifacts = new
            {
                host_rows = Relative(hostRowsPath),
                negative_rows = Relative(negativeRowsPath),
                rollback_closure = Relative(rollbackPath),
                final_readiness_rows = Relative(readinessPath),
                validation = Relative(validationPath),
                markdown = Relative(markdownPath),
                state_patch = Relative(statePatchPath)
            }
        });
        WriteJson(validationPath, new
        {
            schema = "xiigen.quality_semantic_gates_19.validation.v1",
            phase_id = PhaseId,
            command = Command,
            passed = true,
            evidence_level = "handler/test-only",
            proposed_locator_refs_preserved = HasAllExpectedLocatorRefs(positive.CheckSetResults),
            object_object_locator_refs_rejected = !ContainsObjectObject(positive.CheckSetResults.Values),
            dropped_locator_refs_rejected = hostRows.All(row => row.proposed_locator_refs_preserved == true),
            overclaim_closed = true,
            git_diff_check_required = true
        });
        File.WriteAllText(markdownPath, BuildMarkdown(hostRows, rollbackClosure, claims), Utf8NoBom);
        statePatchPath = UpdateState(reportPath, validationPath);

        using var writtenStatePatch = JsonDocument.Parse(File.ReadAllText(statePatchPath, Encoding.UTF8));
        Assert.False(writtenStatePatch.RootElement.GetProperty("giant_state_mutated").GetBoolean());
        var stateRoot = writtenStatePatch.RootElement.GetProperty("patch_fields");
        Assert.Equal(PhaseId, stateRoot.GetProperty("current_phase").GetString());
        Assert.False(stateRoot.GetProperty("semantic_quality_claimed").GetBoolean());
        Assert.False(stateRoot.GetProperty("runtime_extension_claimed").GetBoolean());
        Assert.False(stateRoot.GetProperty("final_readiness_claimed").GetBoolean());
    }

    [Fact]
    public void Qsg19HostSourceResolver_MapsAllowedWindowsHostRefsUnderRuntimeProjectsParent()
    {
        // Эти шесть строк — весь разрешённый surface для чтения host source в quality semantic gate 19.
        var allowedCases = new[]
        {
            (refValue: @"C:\Projects\claude-xiigen-smart-skills\src\claude-entrypoint.js", expectedTail: "claude-xiigen-smart-skills/src/claude-entrypoint.js"),
            (refValue: @"C:\Projects\claude-xiigen-smart-skills\src\claude-entrypoint.ps1", expectedTail: "claude-xiigen-smart-skills/src/claude-entrypoint.ps1"),
            (refValue: @"C:\Projects\cursor-xiigen-smart-skills\src\extension.ts", expectedTail: "cursor-xiigen-smart-skills/src/extension.ts"),
            (refValue: @"C:\Projects\cursor-xiigen-smart-skills\dist\extension.js", expectedTail: "cursor-xiigen-smart-skills/dist/extension.js"),
            (refValue: @"C:\Projects\codex-xiigen-smart-skills\src\codex-entrypoint.js", expectedTail: "codex-xiigen-smart-skills/src/codex-entrypoint.js"),
            (refValue: @"C:\Projects\codex-xiigen-smart-skills\src\codex-entrypoint.ps1", expectedTail: "codex-xiigen-smart-skills/src/codex-entrypoint.ps1")
        };

        // Нормализованный parent нужен для одинаковой проверки containment на Windows и Linux/WSL.
        var projectsParent = Path.TrimEndingDirectorySeparator(Path.GetFullPath(RuntimeProjectsParent));

        foreach (var allowedCase in allowedCases)
        {
            // Resolver возвращает runtime-путь для чтения, но не меняет исходный Windows ref, который попадёт в artifact rows.
            var resolved = ResolveQsg19HostSourceReadPath(allowedCase.refValue);
            var relativeTail = NormalizeQsg19Separators(Path.GetRelativePath(projectsParent, resolved));

            // Абсолютный relative result означал бы другой drive/root и нарушение runtime projects parent boundary.
            Assert.False(Path.IsPathRooted(relativeTail));
            // Выход через ".." означал бы, что whitelist tail или parent containment сломан.
            Assert.False(relativeTail.Equals("..", StringComparison.Ordinal) || relativeTail.StartsWith("../", StringComparison.Ordinal));
            Assert.Equal(allowedCase.expectedTail, relativeTail);
        }
    }

    [Fact]
    public void Qsg19HostSourceResolver_RejectsUnsafeOrNonWhitelistedHostRefs()
    {
        // Негативные кейсы покрывают все запрещённые классы из test-only policy, а не состояние файловой системы.
        var rejectedCases = new[]
        {
            (refValue: " ", reason: "blank host source ref"),
            (refValue: @"src\extension.ts", reason: "relative host source ref"),
            (refValue: @"..\claude-xiigen-smart-skills\src\claude-entrypoint.js", reason: "traversal segment in host source ref"),
            (refValue: @"C:\Projects\claude-xiigen-smart-skills\src\..\claude-entrypoint.js", reason: "traversal segment in host source ref"),
            (refValue: @"C:\Projects\llm_mvp_core\Tests\ExtensionQualitySemanticGates19TargetLocatorPlumbingTests.cs", reason: "current core repo is not an allowed host source repo"),
            (refValue: @"C:\Projects\unknown-xiigen-smart-skills\src\extension.ts", reason: "unknown host repo"),
            (refValue: @"C:\Projects\claude-xiigen-smart-skills\src\not-whitelisted.js", reason: "non-whitelisted host source file")
        };

        foreach (var rejectedCase in rejectedCases)
        {
            // Ошибка должна объяснить стабильную причину отказа без раскрытия локального полного пути владельцу.
            var exception = Assert.Throws<InvalidOperationException>(() => ResolveQsg19HostSourceReadPath(rejectedCase.refValue));

            Assert.Contains(rejectedCase.reason, exception.Message);
            Assert.DoesNotContain(Qsg19CanonicalProjectsParentWindows, exception.Message);
            Assert.DoesNotContain(RuntimeProjectsParent, exception.Message);
        }
    }

    private static XiigenExtensionRequest CreateRequest(
        string hostType,
        string correlationSuffix,
        XiigenTargetLocator? locator,
        bool requireLocator)
        => new(
            HostName: $"QSG19 {hostType}",
            HostType: hostType,
            Method: XiigenExtensionApiContract.PromptToTaskMethod,
            UserPromptOrTaskText: "QSG19 private prompt should be redacted while target locator plumbing is tested.",
            HostFacts: new Dictionary<string, string>
            {
                ["host_path"] = hostType,
                ["qsg_phase"] = PhaseId
            },
            DebugEnabled: true,
            RequestedSkills: [],
            AvailableSkills: ["skill-a", "skill-b", "skill-c", "skill-d"],
            CheckSetInputs: new Dictionary<string, string>
            {
                ["qsg19_target_locator_required"] = requireLocator.ToString().ToLowerInvariant()
            },
            Provenance: new Dictionary<string, string>
            {
                ["source"] = "qsg19-target-locator-plumbing-test"
            },
            CorrelationId: $"{PhaseId}:{correlationSuffix}",
            TargetLocator: locator);

    private static XiigenTargetLocator BuildTargetLocator(
        JsonDocument activeLocator,
        string targetRoot,
        string rollbackRoot,
        string mode)
        => new(
            LocatorCandidateId: activeLocator.RootElement.GetProperty("locator_candidate_id").GetString()!,
            TargetCandidateId: activeLocator.RootElement.GetProperty("target_candidate_id").GetString()!,
            RollbackCandidateId: activeLocator.RootElement.GetProperty("rollback_candidate_id").GetString()!,
            UserAdaptiveAssetRoot: targetRoot,
            RollbackAssetRoot: rollbackRoot,
            LocatorMode: mode,
            LocatorRefs: BuildLocatorRefs(activeLocator));

    private static IReadOnlyDictionary<string, string> BuildLocatorRefs(JsonDocument activeLocator)
    {
        var refs = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["source_locator"] = activeLocator.RootElement.GetProperty("source_locator_candidate_ref").GetString()!,
            ["activation_scope"] = activeLocator.RootElement.GetProperty("activation_scope").GetString()!
        };

        foreach (var locatorRef in activeLocator.RootElement.GetProperty("proposed_locator_refs").EnumerateArray())
        {
            var machineId = locatorRef.GetProperty("machine_id").GetString()!;
            refs[$"proposed_locator_refs.{machineId}"] = JsonSerializer.Serialize(locatorRef, JsonLineOptions);
        }

        return refs;
    }

    private static void AssertLocatorRefsPreserved(
        JsonDocument activeLocator,
        IReadOnlyDictionary<string, string> checkSetResults)
    {
        Assert.True(HasAllExpectedLocatorRefs(checkSetResults), "All proposed_locator_refs entries must reach core CheckSetResults.");
        Assert.False(ContainsObjectObject(checkSetResults.Values), "Locator refs must never be serialized as [object Object].");

        foreach (var locatorRef in activeLocator.RootElement.GetProperty("proposed_locator_refs").EnumerateArray())
        {
            var machineId = locatorRef.GetProperty("machine_id").GetString()!;
            var resultKey = $"target_locator_ref.proposed_locator_refs.{machineId}";
            Assert.True(checkSetResults.TryGetValue(resultKey, out var serialized), $"Missing locator ref {resultKey}.");
            using var parsed = JsonDocument.Parse(serialized!);
            Assert.Equal(machineId, parsed.RootElement.GetProperty("machine_id").GetString());
            Assert.False(string.IsNullOrWhiteSpace(parsed.RootElement.GetProperty("target_sha256").GetString()));
            Assert.False(string.IsNullOrWhiteSpace(parsed.RootElement.GetProperty("rollback_sha256").GetString()));
        }
    }

    private static bool HasAllExpectedLocatorRefs(IReadOnlyDictionary<string, string> checkSetResults)
        => ExpectedLocatorRefMachines.All(machine =>
            checkSetResults.ContainsKey($"target_locator_ref.proposed_locator_refs.{machine}"));

    private static bool ContainsObjectObject(IEnumerable<string> values)
        => values.Any(value => value.Contains("[object Object]", StringComparison.Ordinal));

    private static List<dynamic> BuildHostRows(
        JsonDocument activeLocator,
        XiigenExtensionResponse positive)
    {
        var expectedRefKeys = ExpectedLocatorRefMachines
            .Select(machine => $"proposed_locator_refs.{machine}")
            .ToArray();
        var hosts = new[]
        {
            new
            {
                host = "Claude",
                source = @"C:\Projects\claude-xiigen-smart-skills\src\claude-entrypoint.js",
                launcher = @"C:\Projects\claude-xiigen-smart-skills\src\claude-entrypoint.ps1",
                probe = Path.Combine(ArtifactDir, "qsg19-host-probes", "claude-response.json"),
                evidence = "handler-cli-source"
            },
            new
            {
                host = "Cursor",
                source = @"C:\Projects\cursor-xiigen-smart-skills\src\extension.ts",
                launcher = @"C:\Projects\cursor-xiigen-smart-skills\dist\extension.js",
                probe = "",
                evidence = "extension-handler-source-and-runtime-main"
            },
            new
            {
                host = "Codex",
                source = @"C:\Projects\codex-xiigen-smart-skills\src\codex-entrypoint.js",
                launcher = @"C:\Projects\codex-xiigen-smart-skills\src\codex-entrypoint.ps1",
                probe = Path.Combine(ArtifactDir, "qsg19-host-probes", "codex-response.json"),
                evidence = "handler-cli-source"
            }
        };

        return hosts.Select(host =>
        {
            var sourceText = File.ReadAllText(ResolveQsg19HostSourceReadPath(host.source), Encoding.UTF8);
            var launcherText = File.ReadAllText(ResolveQsg19HostSourceReadPath(host.launcher), Encoding.UTF8);
            var combinedText = sourceText + "\n" + launcherText;
            var hasSafeNormalizer = sourceText.Contains("normalizeLocatorRefs", StringComparison.Ordinal)
                && sourceText.Contains("JSON.stringify", StringComparison.Ordinal)
                && !sourceText.Contains("String(recordValue)", StringComparison.Ordinal)
                && !sourceText.Contains("Object.fromEntries(Object.entries(refs).map(([key, value]) => [key, String(value)]))", StringComparison.Ordinal)
                && !sourceText.Contains("asStringRecord(locatorRefs)", StringComparison.Ordinal);
            var sourceNamesProposedRefs = sourceText.Contains("proposed_locator_refs", StringComparison.Ordinal);
            var coreResultsPreserveRefs = HasAllExpectedLocatorRefs(positive.CheckSetResults)
                && !ContainsObjectObject(positive.CheckSetResults.Values);
            var verified = sourceText.Contains("TargetLocator", StringComparison.Ordinal)
                && sourceText.Contains("qsg19_target_locator_required", StringComparison.Ordinal)
                && launcherText.Contains("TargetLocator", StringComparison.Ordinal);
            return (dynamic)new
            {
                schema = "xiigen.quality_semantic_gates_19.host_row.v1",
                phase_id = PhaseId,
                host.host,
                source_ref = host.source,
                launcher_or_runtime_ref = host.launcher,
                handler_probe_response_ref = string.IsNullOrWhiteSpace(host.probe) ? "" : Relative(host.probe),
                handler_probe_response_success = ProbeSuccess(host.probe),
                evidence_level = host.evidence,
                live_ui_chat_evidence = false,
                handler_or_adapter_test_only = true,
                target_locator_request_field_visible = sourceText.Contains("TargetLocator", StringComparison.Ordinal),
                target_locator_required_gate_visible = sourceText.Contains("qsg19_target_locator_required", StringComparison.Ordinal),
                redacted_trace_visible = sourceText.Contains("redacted", StringComparison.OrdinalIgnoreCase),
                proposed_locator_refs_visible = sourceNamesProposedRefs,
                proposed_locator_refs_preserved = hasSafeNormalizer && sourceNamesProposedRefs && coreResultsPreserveRefs,
                locator_refs_not_object_object = hasSafeNormalizer && !ContainsObjectObject([combinedText]),
                expected_locator_ref_count = expectedRefKeys.Length,
                expected_locator_ref_keys = expectedRefKeys,
                host_semantic_logic_added = false,
                plumbing_verified = verified,
                live_gap = "True live UI/chat runtime was not run in this QSG-19 bounded worker pass."
            };
        }).ToList();
    }

    private static bool ProbeSuccess(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            return false;

        using var document = JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
        return document.RootElement.TryGetProperty("Success", out var success)
            && success.ValueKind == JsonValueKind.True;
    }

    private static List<dynamic> BuildNegativeRows(
        XiigenExtensionResponse missing,
        XiigenExtensionResponse invalid,
        XiigenExtensionResponse rollback)
        =>
        [
            new
            {
                phase_id = PhaseId,
                scenario = "missing_locator",
                passed = !missing.Success && missing.Errors.Contains("target_locator_required"),
                errors = missing.Errors,
                visible_redacted_debug = missing.ChatDisplayText.Contains("input:", StringComparison.Ordinal)
            },
            new
            {
                phase_id = PhaseId,
                scenario = "invalid_locator",
                passed = !invalid.Success && invalid.Errors.Contains("target_locator_user_adaptive_asset_root_not_found"),
                errors = invalid.Errors,
                visible_redacted_debug = invalid.ChatDisplayText.Contains("input:", StringComparison.Ordinal)
            },
            new
            {
                phase_id = PhaseId,
                scenario = "rollback_locator",
                passed = rollback.Success && rollback.CheckSetResults["target_locator_mode"] == "rollback",
                errors = rollback.Errors,
                rollback_requested = rollback.CheckSetResults["target_locator_rollback_requested"]
            }
        ];

    private static object BuildRollbackClosure(XiigenExtensionResponse rollback)
        => new
        {
            schema = "xiigen.quality_semantic_gates_19.rollback_closure.v1",
            phase_id = PhaseId,
            rollback_load_probe_passed = rollback.Success && rollback.CheckSetResults["target_locator_mode"] == "rollback",
            rollback_locator_mode = rollback.CheckSetResults["target_locator_mode"],
            rollback_locator_bound = rollback.CheckSetResults["target_locator_bound"],
            rollback_closure_done = false,
            promotion_ready = false,
            reason = "Handler/core rollback load path is represented, but true live UI/chat production-locator rollback closure is still missing."
        };

    private static List<dynamic> BuildClaimRows(
        IReadOnlyList<dynamic> hostRows,
        XiigenExtensionResponse positive,
        XiigenExtensionResponse missing,
        XiigenExtensionResponse invalid,
        XiigenExtensionResponse rollback,
        object rollbackClosure)
        =>
        [
            new { claim = "core_target_locator_positive", status = positive.Success ? "PASS" : "FAIL", opened = positive.Success },
            new { claim = "missing_locator_negative", status = !missing.Success ? "PASS" : "FAIL", opened = !missing.Success },
            new { claim = "invalid_locator_negative", status = !invalid.Success ? "PASS" : "FAIL", opened = !invalid.Success },
            new { claim = "rollback_load_probe", status = rollback.Success ? "PASS" : "FAIL", opened = rollback.Success },
            new { claim = "claude_live_ui_chat_target_bound", status = "OPEN", opened = false },
            new { claim = "cursor_live_ui_chat_target_bound", status = "OPEN", opened = false },
            new { claim = "codex_live_ui_chat_target_bound", status = "OPEN", opened = false },
            new { claim = "production_locator_promotion", status = "CLOSED", opened = false },
            new { claim = "semantic_quality_claim", status = "CLOSED", opened = false },
            new { claim = "final_readiness", status = "CLOSED", opened = false }
        ];

    private static string BuildMarkdown(
        IReadOnlyList<dynamic> hostRows,
        object rollbackClosure,
        IReadOnlyList<dynamic> claims)
        => $"""
        # QUALITY-SEMANTIC-GATES-19

        Evidence level: handler/test-only. No true live Claude/Cursor/Codex UI/chat readiness claim is opened.

        Host locator plumbing rows: {hostRows.Count}
        Rollback closure: load probe represented, promotion closure remains false.
        Claims opened: {claims.Count(row => row.opened == true)}

        Next: {NextPhaseId} must run true live target-bound host probes and production-like rollback closure before promotion.
        """;

    private static string UpdateState(string reportPath, string validationPath)
        => ExtensionExecutionStatePatchWriter.WritePatch(
            StatePath,
            ArtifactDir,
            PhaseId,
            new Dictionary<string, object?>
            {
                ["current_phase"] = PhaseId,
                ["phase_status"] = "HANDLER_TARGET_LOCATOR_PLUMBING_VERIFIED_LIVE_UI_CHAT_OPEN_ROLLBACK_CLOSURE_FALSE",
                ["implementation_authorized_scope"] = "QUALITY-SEMANTIC-GATES-19 bounded sub-agent slice: fix TargetLocator proposed_locator_refs normalization in thin Claude/Cursor/Codex host adapters, strengthen QSG-19 code/runtime/QA evidence, keep live UI/chat, semantic quality, promotion, production rollback closure, and final readiness claims closed; no commit/push, branch switch, production locator update, external AI run, training, checkpoint mutation, or readiness claim.",
                ["product_code_changed"] = true,
                ["last_completed_phase"] = PhaseId,
                ["last_report_ref"] = Relative(reportPath),
                ["last_validation_ref"] = Relative(validationPath),
                ["semantic_quality_claimed"] = false,
                ["runtime_extension_claimed"] = false,
                ["live_ui_chat_claimed"] = false,
                ["target_promotion_done"] = false,
                ["production_locator_promotion_done"] = false,
                ["product_runtime_semantic_quality_claimed"] = false,
                ["promotion_readiness_claimed"] = false,
                ["final_readiness_claimed"] = false,
                ["rollback_closure_done"] = false,
                ["production_rollback_closure_done"] = false,
                ["next_action"] = "QUALITY-SEMANTIC-GATES-20: run true live Claude/Cursor/Codex target-bound UI/chat probes with visible redacted traces, then rerun production-like rollback/load closure before any promotion/readiness claim."
            },
            new { report_ref = Relative(reportPath) },
            new { validation_ref = Relative(validationPath) },
            JsonOptions);

    private static JsonDocument ReadJson(string path)
        => JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));

    private static void WriteJson(string path, object value)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var json = JsonSerializer.Serialize(value, JsonOptions).Replace("\r\n", "\n", StringComparison.Ordinal);
        File.WriteAllText(path, json + "\n", Utf8NoBom);
    }

    private static void WriteJsonl(string path, IEnumerable<object> rows)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, string.Join("\n", rows.Select(row => JsonSerializer.Serialize(row, JsonLineOptions))) + "\n", Utf8NoBom);
    }

    private static bool EnsureTargetAssetRootFromUserCache(string targetRoot)
    {
        var targetAssetRoot = RepoPath(targetRoot);
        if (Directory.Exists(targetAssetRoot))
            return false;

        if (!string.Equals(Path.GetFileName(targetAssetRoot), "user-cache-continued", StringComparison.OrdinalIgnoreCase))
            return false;

        var parent = Directory.GetParent(targetAssetRoot)?.FullName;
        if (parent is null)
            return false;

        var sourceRoot = Path.Combine(parent, "user-cache");
        if (!Directory.Exists(sourceRoot))
            return false;

        CopyDirectory(sourceRoot, targetAssetRoot);
        return true;
    }

    private static bool EnsureRollbackAssetRootFromRepoCommon(string rollbackRoot)
    {
        var rollbackAssetRoot = RepoPath(rollbackRoot);
        if (Directory.Exists(rollbackAssetRoot))
            return false;

        if (!string.Equals(Path.GetFileName(rollbackAssetRoot), "rollback", StringComparison.OrdinalIgnoreCase))
            return false;

        var parent = Directory.GetParent(rollbackAssetRoot)?.FullName;
        if (parent is null)
            return false;

        var sourceRoot = Path.Combine(parent, "repo-common", "models");
        if (!Directory.Exists(sourceRoot))
            return false;

        CopyDirectory(sourceRoot, rollbackAssetRoot);
        return true;
    }

    private static void CopyDirectory(string sourceRoot, string targetRoot)
    {
        Directory.CreateDirectory(targetRoot);
        foreach (var directory in Directory.EnumerateDirectories(sourceRoot, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceRoot, directory);
            Directory.CreateDirectory(Path.Combine(targetRoot, relative));
        }

        foreach (var file in Directory.EnumerateFiles(sourceRoot, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceRoot, file);
            File.Copy(file, Path.Combine(targetRoot, relative), overwrite: true);
        }
    }

    private static string RepoPath(string relativePath)
        => Path.GetFullPath(Path.Combine(RepoRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));

    private static string ResolveQsg19HostSourceReadPath(string sourceRef)
    {
        if (string.IsNullOrWhiteSpace(sourceRef))
        {
            // Пустой ref не может быть восстановлен безопасно: это либо ошибка fixture-а, либо потерянный source.
            throw CreateQsg19RejectedHostSourceRefException("blank host source ref");
        }

        var normalizedWindowsRef = NormalizeQsg19WindowsHostSourceRef(sourceRef);
        if (ContainsQsg19TraversalSegment(normalizedWindowsRef))
        {
            // Даже для известного repo сегмент ".." запрещён, потому что whitelist должен быть exact, а не path-normalized guess.
            throw CreateQsg19RejectedHostSourceRefException("traversal segment in host source ref");
        }

        if (!IsQsg19WindowsAbsoluteRef(normalizedWindowsRef))
        {
            // Relative refs не принимаются: этот resolver переносит только старые canonical Windows literals из rows.
            throw CreateQsg19RejectedHostSourceRefException("relative host source ref");
        }

        if (!StartsWithQsg19WindowsRoot(normalizedWindowsRef, Qsg19CanonicalProjectsParentWindows))
        {
            // Другой drive/root — внешний surface, который не относится к трём host repo этой проверки.
            throw CreateQsg19RejectedHostSourceRefException("host source ref is outside canonical projects parent");
        }

        var hostRepoName = GetQsg19HostRepoName(normalizedWindowsRef);
        if (string.Equals(hostRepoName, Qsg19CoreRepoName, StringComparison.OrdinalIgnoreCase))
        {
            // Current core repo запрещён: это test-only inspection of host adapters, а не обход core artifact resolver.
            throw CreateQsg19RejectedHostSourceRefException("current core repo is not an allowed host source repo");
        }

        if (hostRepoName is null || !KnownQsg19HostRepoNames.Contains(hostRepoName))
        {
            // Unknown host repo запрещён отдельно от unknown file, чтобы расширение surface требовало явной правки whitelist.
            throw CreateQsg19RejectedHostSourceRefException("unknown host repo");
        }

        if (!AllowedQsg19HostSourceTails.TryGetValue(normalizedWindowsRef, out var hostRelativeTail))
        {
            // Известный repo сам по себе не даёт права читать любой соседний файл.
            throw CreateQsg19RejectedHostSourceRefException("non-whitelisted host source file");
        }

        var runtimeCandidate = Path.Combine(RuntimeProjectsParent, ToQsg19NativeSeparators(hostRelativeTail));
        return NormalizeQsg19PathUnderRuntimeProjectsParent(runtimeCandidate);
    }

    private static string GetRuntimeProjectsParent()
    {
        var fullRepoRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(RepoRoot));
        if (!string.Equals(Path.GetFileName(fullRepoRoot), Qsg19CoreRepoName, StringComparison.OrdinalIgnoreCase))
        {
            // Этот guard не раскрывает путь: важна причина, что test-only resolver запущен не из canonical core repo.
            throw new InvalidOperationException("Rejected quality semantic gate 19 host source resolver setup. Reason: RepoRoot must be the current core repo.");
        }

        // Родитель RepoRoot — runtime-представление C:\Projects: C:\Projects на Windows или, например, /mnt/c/Projects в WSL.
        return Directory.GetParent(fullRepoRoot)?.FullName
            ?? throw new InvalidOperationException("Rejected quality semantic gate 19 host source resolver setup. Reason: RepoRoot has no projects parent.");
    }

    private static string NormalizeQsg19PathUnderRuntimeProjectsParent(string path)
    {
        var fullPath = Path.GetFullPath(path);
        var fullProjectsParent = Path.TrimEndingDirectorySeparator(Path.GetFullPath(RuntimeProjectsParent));
        var relativePath = Path.GetRelativePath(fullProjectsParent, fullPath);

        if (relativePath == "."
            || (!Path.IsPathRooted(relativePath)
                && !relativePath.Equals("..", StringComparison.Ordinal)
                && !relativePath.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                && !relativePath.StartsWith("../", StringComparison.Ordinal)))
        {
            // Только путь, доказанно лежащий под runtime projects parent, можно передать в File.ReadAllText.
            return fullPath;
        }

        // Этот guard страхует будущую ошибку в whitelist tail, даже если текущие шесть tails безопасны.
        throw CreateQsg19RejectedHostSourceRefException("resolved host source escapes runtime projects parent");
    }

    private static string? GetQsg19HostRepoName(string normalizedWindowsRef)
    {
        // Host repo — это первый сегмент после canonical C:\Projects; больше строки в ошибку не выводятся.
        var suffix = normalizedWindowsRef[Qsg19CanonicalProjectsParentWindows.Length..].TrimStart('\\');
        if (string.IsNullOrWhiteSpace(suffix))
            return null;

        var separatorIndex = suffix.IndexOf('\\');
        return separatorIndex < 0 ? suffix : suffix[..separatorIndex];
    }

    private static bool IsQsg19WindowsAbsoluteRef(string refValue)
        => refValue.Length >= 3
           && char.IsLetter(refValue[0])
           && refValue[1] == ':'
           && refValue[2] == '\\';

    private static bool StartsWithQsg19WindowsRoot(string normalizedPath, string normalizedRoot)
        => normalizedPath.Equals(normalizedRoot, StringComparison.OrdinalIgnoreCase)
           || normalizedPath.StartsWith(normalizedRoot + "\\", StringComparison.OrdinalIgnoreCase);

    private static bool ContainsQsg19TraversalSegment(string normalizedWindowsRef)
        => normalizedWindowsRef.Split('\\').Any(segment => segment == "..");

    private static string NormalizeQsg19WindowsHostSourceRef(string refValue)
        => refValue.Trim().Replace('/', '\\');

    private static string NormalizeQsg19Separators(string value)
        => value.Replace('\\', '/');

    private static string ToQsg19NativeSeparators(string value)
        => value.Replace('/', Path.DirectorySeparatorChar);

    private static InvalidOperationException CreateQsg19RejectedHostSourceRefException(string reason)
        => new($"Rejected quality semantic gate 19 host source ref. Reason: {reason}.");

    private static string Relative(string path)
        => Path.GetRelativePath(RepoRoot, path).Replace(Path.DirectorySeparatorChar, '/');
}
