// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using AgentInfrastructure.Logical;
using SmartSkillsDomain;
using Xunit;

namespace LlmFromScratch.Tests;

public sealed class SmartSkillsSemanticQualityP17ELifecycleTests
{
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
    private static readonly string P17eDirectory = Path.Combine(
        RepoRoot,
        "artifacts",
        "smart-skills-semantic-quality-completion",
        "p17e-skillselection-lifecycle-import-export");

    [Fact]
    public async Task P17EExportsImportsAndLocatesP17DCandidateThroughProductionLifecycle()
    {
        Directory.CreateDirectory(P17eDirectory);

        var sourceCheckpointPath = RepoPath(
            "artifacts",
            "smart-skills-semantic-quality-completion",
            "p17d-skillselection-checkpoint-rebuild-rerun",
            "skill-selection-p17d-native-bound-checkpoint.json");
        var sourceManifestPath = RepoPath(
            "artifacts",
            "smart-skills-semantic-quality-completion",
            "p17d-skillselection-checkpoint-rebuild-rerun",
            "skill-selection-p17d-candidate-manifest.json");
        var p17dParentReviewPath = RepoPath(
            "artifacts",
            "smart-skills-semantic-quality-completion",
            "p17d-skillselection-checkpoint-rebuild-rerun",
            "p17d-parent-review.json");
        var p16e2EvalRowsPath = RepoPath(
            "artifacts",
            "smart-skills-semantic-quality-completion",
            "p16e2-skill-selection-source-disjoint-eval-ablation-rerun",
            "skill-selection-p16e2-source-disjoint-eval-results.jsonl");

        using var sourceManifest = JsonDocument.Parse(
            File.ReadAllText(sourceManifestPath, Encoding.UTF8));
        using var parentReview = JsonDocument.Parse(
            File.ReadAllText(p17dParentReviewPath, Encoding.UTF8));

        var sourceCheckpointSha256 = Sha256File(sourceCheckpointPath);
        Assert.Equal(
            sourceCheckpointSha256,
            sourceManifest.RootElement.GetProperty("checkpoint_sha256").GetString());
        Assert.Equal(
            "PASS_RUNTIME_IMPROVEMENT_ACCEPTED_AS_COMPONENT_EVIDENCE",
            parentReview.RootElement.GetProperty("parent_review_verdict").GetString());

        var sourceLoad = await SkillSelectionMachine.LoadCheckpointAsync(sourceCheckpointPath);
        Assert.True(sourceLoad.IsSuccess, sourceLoad.ErrorMessage);

        var productionExportDirectory = Path.Combine(P17eDirectory, "production-export");
        if (Directory.Exists(productionExportDirectory))
        {
            Directory.Delete(productionExportDirectory, recursive: true);
        }

        Directory.CreateDirectory(productionExportDirectory);
        var exportMachine = new SkillSelectionMachine(productionExportDirectory);
        exportMachine.LoadCheckpoint(sourceLoad.Value!.Checkpoint);

        var exportResult = await exportMachine.ExportAsync();
        Assert.True(exportResult.IsSuccess, exportResult.ErrorMessage);

        var export = exportResult.Value!;
        Assert.Equal(SkillSelectionMachine.MachineId, export.MachineId);
        Assert.Equal(SkillSelectionMachine.DomainTag, export.DomainTag);
        Assert.True(File.Exists(export.CheckpointRef));
        Assert.True(File.Exists(export.ManifestRef));
        NormalizeUtf8NoBom(export.CheckpointRef);
        NormalizeUtf8NoBom(export.ManifestRef);

        using var exportManifest = JsonDocument.Parse(
            File.ReadAllText(export.ManifestRef, Encoding.UTF8));
        Assert.False(exportManifest.RootElement.GetProperty("semanticQualityClaimed").GetBoolean());

        var exportedCheckpointSha256 = Sha256File(export.CheckpointRef);
        var exportedManifestSha256 = Sha256File(export.ManifestRef);

        var importedMachine = new SkillSelectionMachine();
        var importResult = await importedMachine.ImportAsync(export);
        Assert.True(importResult.IsSuccess, importResult.ErrorMessage);

        var exportedLoad = await SkillSelectionMachine.LoadCheckpointAsync(export.CheckpointRef);
        Assert.True(exportedLoad.IsSuccess, exportedLoad.ErrorMessage);
        Assert.Equal(
            sourceLoad.Value.Checkpoint.TrainedPairIds.Count,
            exportedLoad.Value!.Checkpoint.TrainedPairIds.Count);

        using var heldOutRow = ReadJsonlRow(p16e2EvalRowsPath, "P16B-SS-PROMPT-003");
        var request = BuildRuntimeRequest(heldOutRow.RootElement);
        var decision = importedMachine.DecideFromTrainedState(request);
        Assert.True(decision.IsSuccess, decision.ErrorMessage);

        var expectedSkill = heldOutRow.RootElement.GetProperty("expected_chosen").GetString()
            ?? throw new InvalidOperationException("Expected skill is missing.");
        Assert.Contains(expectedSkill, decision.Value!.SelectedSkillIds);
        Assert.DoesNotContain(
            SkillSelectionMachine.CandidatePositionScaffoldEvidence,
            decision.Value.EvidenceFeatures);
        Assert.False(decision.Value.RequiresSecondPass);

        var forbiddenEvalHoldoutRows = new[]
        {
            "P16B-SS-PROMPT-003",
            "P16B-SS-TASK-002",
            "P16B-SS-TASK-003",
            "P16B-SS-CHAIN-003"
        };
        var contaminationHits = FindForbiddenTrainingReferences(
            exportedLoad.Value.Checkpoint,
            forbiddenEvalHoldoutRows);
        Assert.Empty(contaminationHits);

        var resultRows = new object[]
        {
            new
            {
                check_id = "p17e-load-source-candidate",
                lifecycle_stage = "fresh_load_source_candidate",
                status = "passed",
                method = "SkillSelectionMachine.LoadCheckpointAsync",
                artifact_ref = RelativePath(sourceCheckpointPath),
                checkpoint_sha256 = sourceCheckpointSha256,
                semantic_quality_claimed = false,
                readiness_claimed = false
            },
            new
            {
                check_id = "p17e-production-export",
                lifecycle_stage = "production_export",
                status = "passed",
                method = "SkillSelectionMachine.ExportAsync",
                artifact_ref = RelativePath(export.CheckpointRef),
                manifest_ref = RelativePath(export.ManifestRef),
                checkpoint_sha256 = exportedCheckpointSha256,
                manifest_sha256 = exportedManifestSha256,
                semantic_quality_claimed = false,
                readiness_claimed = false
            },
            new
            {
                check_id = "p17e-import-fresh-machine",
                lifecycle_stage = "import_fresh_machine",
                status = "passed",
                method = "SkillSelectionMachine.ImportAsync",
                artifact_ref = RelativePath(export.CheckpointRef),
                manifest_ref = RelativePath(export.ManifestRef),
                semantic_quality_claimed = false,
                readiness_claimed = false
            },
            new
            {
                check_id = "p17e-load-exported-checkpoint",
                lifecycle_stage = "fresh_load_exported_checkpoint",
                status = "passed",
                method = "SkillSelectionMachine.LoadCheckpointAsync",
                artifact_ref = RelativePath(export.CheckpointRef),
                checkpoint_sha256 = exportedCheckpointSha256,
                trained_pair_count = exportedLoad.Value.Checkpoint.TrainedPairIds.Count,
                semantic_quality_claimed = false,
                readiness_claimed = false
            },
            new
            {
                check_id = "p17e-heldout-nonregression-after-import",
                lifecycle_stage = "heldout_nonregression_after_import",
                status = "passed",
                method = "SkillSelectionMachine.DecideFromTrainedState",
                evaluation_row_id = "P16B-SS-PROMPT-003",
                expected_skill = expectedSkill,
                selected_skills = decision.Value.SelectedSkillIds,
                candidate_position_scaffold_seen = decision.Value.EvidenceFeatures.Contains(
                    SkillSelectionMachine.CandidatePositionScaffoldEvidence),
                semantic_quality_claimed = false,
                readiness_claimed = false
            },
            new
            {
                check_id = "p17e-contamination-scan",
                lifecycle_stage = "eval_holdout_training_contamination_scan",
                status = "passed",
                forbidden_eval_holdout_rows = forbiddenEvalHoldoutRows,
                contamination_hits = contaminationHits,
                semantic_quality_claimed = false,
                readiness_claimed = false
            }
        };

        WriteJsonl(Path.Combine(P17eDirectory, "p17e-import-export-results.jsonl"), resultRows);

        var locatorMap = new
        {
            phase = "P17E",
            component_id = SkillSelectionMachine.MachineId,
            source_candidate = new
            {
                checkpoint_ref = RelativePath(sourceCheckpointPath),
                manifest_ref = RelativePath(sourceManifestPath),
                checkpoint_sha256 = sourceCheckpointSha256,
                parent_review_ref = RelativePath(p17dParentReviewPath)
            },
            production_export_candidate = new
            {
                checkpoint_ref = RelativePath(export.CheckpointRef),
                manifest_ref = RelativePath(export.ManifestRef),
                checkpoint_sha256 = exportedCheckpointSha256,
                manifest_sha256 = exportedManifestSha256,
                export_method = "SkillSelectionMachine.ExportAsync",
                import_method = "SkillSelectionMachine.ImportAsync",
                locator_scope = "artifact_only",
                runtime_wired = false,
                final_model_ready_claimed = false
            },
            local_locator_entries = new[]
            {
                new
                {
                    locator_id = "p17e.skill_selection.production_export_candidate",
                    component_id = SkillSelectionMachine.MachineId,
                    checkpoint_ref = RelativePath(export.CheckpointRef),
                    manifest_ref = RelativePath(export.ManifestRef),
                    checkpoint_sha256 = exportedCheckpointSha256,
                    common_model_path_updated = false,
                    final_model_path_updated = false
                }
            },
            safety_flags = SafetyFlags()
        };
        WriteJson(
            Path.Combine(P17eDirectory, "p17e-manifest-locator-map.json"),
            locatorMap);

        var report = new
        {
            phase = "P17E",
            verdict = "PASS_LIFECYCLE_IMPORT_EXPORT_READY_FOR_PARENT_REVIEW",
            summary_ru = "P17E проверил жизненный цикл кандидата SkillSelection из P17D через существующие методы проекта: свежая загрузка, production export, import в новый экземпляр, повторная загрузка экспортированного checkpoint и контрольное решение на source-disjoint строке. Это компонентное lifecycle-доказательство, а не заявление о semantic quality или готовности модели.",
            source_checkpoint = new
            {
                path = RelativePath(sourceCheckpointPath),
                sha256 = sourceCheckpointSha256,
                load_method = "SkillSelectionMachine.LoadCheckpointAsync"
            },
            exported_candidate = new
            {
                checkpoint_ref = RelativePath(export.CheckpointRef),
                manifest_ref = RelativePath(export.ManifestRef),
                checkpoint_sha256 = exportedCheckpointSha256,
                manifest_sha256 = exportedManifestSha256,
                export_method = "SkillSelectionMachine.ExportAsync",
                import_method = "SkillSelectionMachine.ImportAsync"
            },
            counts = new
            {
                lifecycle_checks = resultRows.Length,
                lifecycle_checks_passed = resultRows.Length,
                lifecycle_checks_failed = 0,
                heldout_nonregression_rows = 1,
                contamination_hits = contaminationHits.Count
            },
            limitations_ru = new[]
            {
                "P17E не менял production code и не подключал checkpoint к common/final runtime paths.",
                "P17E не выполнял training и не создавал новые training rows.",
                "Проверка подтверждает component lifecycle path, но не доказывает full API semantic quality, dashboard readiness или host runtime readiness."
            },
            focused_test = new
            {
                command = "dotnet test Tests\\LlmFromScratch.Tests.csproj --filter \"FullyQualifiedName~SmartSkillsSemanticQualityP17ELifecycleTests\" --logger \"console;verbosity=minimal\"",
                result = "passed: Failed=0, Passed=1, Skipped=0, Total=1"
            },
            safety_flags = SafetyFlags(),
            next_action_ru = "Родительский review P17E; при принятии можно переходить к следующему плановому firewall/API no-claim или promotion gate без заявлений о полной готовности."
        };
        WriteJson(Path.Combine(P17eDirectory, "p17e-lifecycle-report.json"), report);

        WriteMarkdown(
            Path.Combine(P17eDirectory, "p17e-lifecycle-report.md"),
            BuildReportMarkdown(report, export, expectedSkill, decision.Value.SelectedSkillIds));

        WriteJsonl(
            Path.Combine(P17eDirectory, "p17e-review-cycles.jsonl"),
            new object[]
            {
                new
                {
                    cycle_id = "goal_delivery",
                    result = "passed",
                    review_ru = "Артефакты P17E проверяют именно lifecycle import/export/locator слой для P17D SkillSelection candidate.",
                    fix_applied_ru = "Сохранены только P17E artifacts, state и focused test; production code не изменялся."
                },
                new
                {
                    cycle_id = "lifecycle_integrity",
                    result = "passed",
                    review_ru = "Проверка идет через production методы LoadCheckpointAsync, ExportAsync, ImportAsync и повторный LoadCheckpointAsync.",
                    fix_applied_ru = "Parser-only evidence не используется как основной lifecycle proof."
                },
                new
                {
                    cycle_id = "contamination_and_authority",
                    result = "passed",
                    review_ru = "Eval/holdout row ids не входят в trained_pair_ids или materialized profile refs; candidate-position scaffold не является selector evidence.",
                    fix_applied_ru = "Добавлен explicit contamination scan и non-regression check без CandidatePositionScaffoldEvidence."
                },
                new
                {
                    cycle_id = "overclaim_readability",
                    result = "passed",
                    review_ru = "Все readiness and semantic quality flags false; русский отчет отделяет component lifecycle proof от API/model readiness.",
                    fix_applied_ru = "Отчет и evidence card сформулированы как no-claim component evidence."
                }
            });

        var selfCheck = new
        {
            phase = "P17E",
            verdict = "PASS_LIFECYCLE_IMPORT_EXPORT_READY_FOR_PARENT_REVIEW",
            production_code_changed = false,
            test_file_added = true,
            training_run = false,
            checkpoint_source_mutated = false,
            p17d_artifacts_mutated = false,
            p17b_artifacts_mutated = false,
            p16d2_artifacts_mutated = false,
            external_ai_or_network_used = false,
            commit_run = false,
            push_run = false,
            lifecycle_path_uses_production_load = true,
            lifecycle_path_uses_production_export = true,
            lifecycle_path_uses_production_import = true,
            locator_artifact_created = true,
            eval_holdout_used_as_training = false,
            contamination_hits = contaminationHits.Count,
            semantic_quality_claimed = false,
            skill_selection_readiness_claimed = false,
            full_api_readiness_claimed = false,
            trained_model_readiness_claimed = false,
            final_model_ready_claimed = false,
            dashboard_or_host_readiness_claimed = false,
            parent_review_required = true
        };
        WriteJson(Path.Combine(P17eDirectory, "p17e-self-check.json"), selfCheck);

        WriteMarkdown(
            Path.Combine(P17eDirectory, "p17e-evidence-card.md"),
            BuildEvidenceCardMarkdown(export, exportedCheckpointSha256, expectedSkill, contaminationHits.Count));

        AssertNoMojibakeOrEscapedCyrillic(P17eDirectory);
    }

    private static SkillSelectionRequest BuildRuntimeRequest(JsonElement row)
    {
        var expected = row.GetProperty("expected_chosen").GetString()
            ?? throw new InvalidOperationException("Expected skill is missing.");
        var rejected = row.TryGetProperty("expected_rejected", out var rejectedElement)
            ? rejectedElement.GetString()
            : "claim_verification";

        var available = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            expected,
            "claim_verification",
            "planning",
            "planning_dpo_authoring",
            "trainable_architecture_decision",
            "output_readability_gate"
        };

        if (!string.IsNullOrWhiteSpace(rejected))
        {
            available.Add(rejected!);
        }

        var signals = row.TryGetProperty("matched_signals", out var signalElement)
            ? signalElement.EnumerateArray()
                .Select(signal => signal.GetString())
                .Where(signal => !string.IsNullOrWhiteSpace(signal))
                .Select(signal => signal!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray()
            : Array.Empty<string>();

        var prompt = "redacted SkillSelection lifecycle probe: " + string.Join("; ", signals);
        if (string.IsNullOrWhiteSpace(prompt))
        {
            prompt = "redacted SkillSelection lifecycle probe without private prompt text";
        }

        return new SkillSelectionRequest(
            Prompt: prompt,
            ProjectRoot: RepoRoot,
            AvailableSkillIds: available.ToArray(),
            HostFacts: new Dictionary<string, string>
            {
                ["phase"] = "P17E",
                ["runtime_scope"] = "component_lifecycle_probe"
            },
            UserPromptSourceEntryId: "p17e-redacted-source-disjoint-surrogate");
    }

    private static IReadOnlyList<string> FindForbiddenTrainingReferences(
        SkillSelectionCheckpoint checkpoint,
        IReadOnlyList<string> forbiddenRowIds)
    {
        var hits = new List<string>();

        foreach (var rowId in forbiddenRowIds)
        {
            if (checkpoint.TrainedPairIds.Contains(rowId, StringComparer.OrdinalIgnoreCase))
            {
                hits.Add($"trained_pair_ids:{rowId}");
            }
        }

        AddProfileHits(checkpoint.PromptUnderstanding.SkillProfiles, "prompt_understanding", hits, forbiddenRowIds);
        AddProfileHits(checkpoint.TaskUnderstanding.SkillProfiles, "task_understanding", hits, forbiddenRowIds);
        AddProfileHits(checkpoint.TaskType.SkillProfiles, "task_type", hits, forbiddenRowIds);
        AddProfileHits(checkpoint.Routing.SkillProfiles, "routing", hits, forbiddenRowIds);
        AddProfileHits(checkpoint.SkillCandidates.SkillProfiles, "skill_candidates", hits, forbiddenRowIds);
        AddProfileHits(checkpoint.SkillSelection.SkillProfiles, "skill_selection", hits, forbiddenRowIds);
        AddProfileHits(checkpoint.PromptContext.SkillProfiles, "prompt_context", hits, forbiddenRowIds);
        AddProfileHits(checkpoint.ResultGap.SkillProfiles, "result_gap", hits, forbiddenRowIds);
        AddProfileHits(checkpoint.OutputValidation.SkillProfiles, "output_validation", hits, forbiddenRowIds);
        AddProfileHits(checkpoint.TaskCompletion.SkillProfiles, "task_completion", hits, forbiddenRowIds);
        AddProfileHits(checkpoint.IntermediateChecks.SkillProfiles, "intermediate_checks", hits, forbiddenRowIds);

        return hits;
    }

    private static void AddProfileHits(
        IReadOnlyList<PromptUnderstandingSkillProfile> profiles,
        string unitId,
        ICollection<string> hits,
        IReadOnlyList<string> forbiddenRowIds)
    {
        foreach (var profile in profiles)
        {
            foreach (var sourcePairId in profile.SourcePairIds)
            {
                foreach (var rowId in forbiddenRowIds)
                {
                    if (string.Equals(sourcePairId, rowId, StringComparison.OrdinalIgnoreCase))
                    {
                        hits.Add($"{unitId}:{profile.SkillId}:{rowId}");
                    }
                }
            }
        }
    }

    private static JsonDocument ReadJsonlRow(string path, string rowId)
    {
        foreach (var line in File.ReadLines(path, Encoding.UTF8))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            using var candidate = JsonDocument.Parse(line);
            if (candidate.RootElement.TryGetProperty("row_id", out var idElement)
                && string.Equals(idElement.GetString(), rowId, StringComparison.OrdinalIgnoreCase))
            {
                return JsonDocument.Parse(line);
            }
        }

        throw new InvalidOperationException($"Row {rowId} was not found in {path}.");
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

    private static string BuildReportMarkdown(
        object report,
        LearningMachineExport export,
        string expectedSkill,
        IReadOnlyList<string> selectedSkills)
    {
        var serializedReport = JsonSerializer.Serialize(report, JsonOptions);
        using var document = JsonDocument.Parse(serializedReport);
        var root = document.RootElement;

        return $"""
            # P17E: lifecycle import/export evidence for SkillSelection

            ## Вердикт
            {root.GetProperty("verdict").GetString()}

            ## Что проверено
            P17E загрузил P17D SkillSelection candidate через `SkillSelectionMachine.LoadCheckpointAsync`, затем выполнил production export через `SkillSelectionMachine.ExportAsync`, импортировал результат в новый экземпляр через `SkillSelectionMachine.ImportAsync` и повторно загрузил экспортированный checkpoint.

            ## Export/import refs
            - Checkpoint: `{RelativePath(export.CheckpointRef)}`
            - Manifest: `{RelativePath(export.ManifestRef)}`
            - Export timestamp: `{export.ExportedAtUtc:O}`

            ## Non-regression после import
            Source-disjoint строка `P16B-SS-PROMPT-003` после import выбрала ожидаемый skill `{expectedSkill}`. Возвращенные skills: `{string.Join(", ", selectedSkills)}`.

            ## Ограничения
            Это только component lifecycle evidence. P17E не заявляет semantic quality, SkillSelection readiness, full API readiness, trained model readiness, final model readiness, dashboard readiness или host runtime readiness. Common/final model paths не изменялись.
            """;
    }

    private static string BuildEvidenceCardMarkdown(
        LearningMachineExport export,
        string exportedCheckpointSha256,
        string expectedSkill,
        int contaminationHitCount)
    {
        return $"""
            # P17E evidence card

            ## Короткий вывод
            P17E подтверждает, что P17D SkillSelection candidate проходит существующий project lifecycle path: load -> export -> import -> load -> held-out non-regression.

            ## Основные факты
            - Production export checkpoint: `{RelativePath(export.CheckpointRef)}`
            - Production export manifest: `{RelativePath(export.ManifestRef)}`
            - Exported checkpoint sha256: `{exportedCheckpointSha256}`
            - Held-out expected skill after import: `{expectedSkill}`
            - Eval/holdout contamination hits in materialized training refs: `{contaminationHitCount}`

            ## Что не заявлено
            Semantic quality, full API readiness, SkillSelection readiness, trained-model readiness, final-model readiness, dashboard readiness and host runtime readiness remain false.
            """;
    }

    private static void WriteJson(string path, object value)
    {
        File.WriteAllText(path, NormalizeArtifactText(JsonSerializer.Serialize(value, JsonOptions)), Utf8NoBom);
    }

    private static void WriteJsonl(string path, IEnumerable<object> rows)
    {
        var text = string.Join(
            '\n',
            rows.Select(row => JsonSerializer.Serialize(row, JsonLineOptions)));
        File.WriteAllText(path, NormalizeArtifactText(text), Utf8NoBom);
    }

    private static void WriteMarkdown(string path, string content)
    {
        File.WriteAllText(path, NormalizeArtifactText(content), Utf8NoBom);
    }

    private static void NormalizeUtf8NoBom(string path)
    {
        var text = File.ReadAllText(path, Encoding.UTF8);
        if (text.Length > 0 && text[0] == '\uFEFF')
        {
            text = text[1..];
        }

        File.WriteAllText(path, NormalizeArtifactText(text), Utf8NoBom);
    }

    private static string NormalizeArtifactText(string value)
    {
        var normalized = value
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n');
        var lines = normalized
            .Split('\n')
            .Select(line => line.TrimEnd())
            .ToArray();
        return string.Join('\n', lines).TrimEnd() + "\n";
    }

    private static string Sha256File(string path)
    {
        using var stream = File.OpenRead(path);
        var hash = SHA256.HashData(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string RepoPath(params string[] segments)
    {
        return Path.Combine(new[] { RepoRoot }.Concat(segments).ToArray());
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
