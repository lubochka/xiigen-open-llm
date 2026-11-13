using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using SmartSkillsDomain;

internal static class Program
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private static readonly JsonSerializerOptions LineOptions = new(JsonSerializerDefaults.Web);

    public static async Task<int> Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.Error.WriteLine("Expected args: <repoRoot> <artifactLeaf>");
            return 2;
        }

        var repoRoot = Path.GetFullPath(args[0]);
        var leaf = Path.GetFullPath(args[1]);
        Directory.CreateDirectory(leaf);

        var stopwatch = Stopwatch.StartNew();
        using var process = Process.GetCurrentProcess();
        var cpuStart = process.TotalProcessorTime.TotalMilliseconds;

        var datasetPath = Path.Combine(repoRoot, "training-data", "smart-skills", "semantic-quality", "orchestration-reasoning-expanded-v1", "orchestration-reasoning-expanded-dpo.jsonl");
        var splitPath = Path.Combine(repoRoot, "training-data", "smart-skills", "semantic-quality", "orchestration-reasoning-expanded-v1", "split.json");
        var schemaPath = Path.Combine(repoRoot, "training-data", "smart-skills", "semantic-quality", "orchestration-reasoning-expanded-v1", "schema.json");
        var datasetCardPath = Path.Combine(repoRoot, "training-data", "smart-skills", "semantic-quality", "orchestration-reasoning-expanded-v1", "dataset-card.json");

        var rows = File.ReadLines(datasetPath, Encoding.UTF8)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonSerializer.Deserialize<ExpandedRow>(line, LineOptions)!)
            .ToArray();
        var trainRows = rows.Where(row => string.Equals(row.SplitRole, "train", StringComparison.Ordinal)).ToArray();
        var heldOutRows = rows.Where(row => string.Equals(row.SplitRole, "held_out", StringComparison.Ordinal)).ToArray();
        if (trainRows.Length != 8 || heldOutRows.Length != 4)
            throw new InvalidOperationException("Expanded-v1 split is not 8 train / 4 held-out.");

        var trainSourceRefs = trainRows.Select(row => row.SourceManifestRef).ToHashSet(StringComparer.Ordinal);
        var heldSourceRefs = heldOutRows.Select(row => row.SourceManifestRef).ToHashSet(StringComparer.Ordinal);
        var sourceOverlap = trainSourceRefs.Intersect(heldSourceRefs, StringComparer.Ordinal).ToArray();
        if (sourceOverlap.Length != 0)
            throw new InvalidOperationException("Train and held-out source refs overlap.");

        var datasetSha = Sha256File(datasetPath);
        var splitSha = Sha256File(splitPath);
        var schemaSha = Sha256File(schemaPath);
        var datasetCardSha = Sha256File(datasetCardPath);

        var patterns = trainRows
            .Select((row, index) => BuildTrainOnlyPattern(row, index + 1))
            .ToArray();
        var learnedState = BuildLearnedState(patterns, datasetSha, splitSha, schemaSha);

        var modelDir = Path.Combine(leaf, "model-candidate");
        var checkpointPath = Path.Combine(modelDir, "checkpoint.json");
        var manifestPath = Path.Combine(modelDir, "manifest.json");
        var saveMachine = new OrchestrationReasoningMachine(new StaticLoader(learnedState));
        var saveResult = await saveMachine.SaveCheckpointAsync(
            checkpointPath,
            manifestPath,
            Rel(repoRoot, datasetCardPath),
            trainRows.Select(row => row.TrainingPairId).ToArray());
        if (!saveResult.IsSuccess)
            throw new InvalidOperationException("SaveCheckpointAsync failed: " + saveResult.ErrorCode + " " + saveResult.ErrorMessage);

        var checkpointSha = Sha256File(checkpointPath);
        var manifestSha = Sha256File(manifestPath);
        CopyPair(checkpointPath, manifestPath, Path.Combine(leaf, "backup"));
        CopyPair(checkpointPath, manifestPath, Path.Combine(leaf, "rollback"));

        var loadResult = await OrchestrationReasoningMachine.LoadCheckpointAsync(checkpointPath, manifestPath);
        if (!loadResult.IsSuccess)
            throw new InvalidOperationException("LoadCheckpointAsync failed: " + loadResult.ErrorCode + " " + loadResult.ErrorMessage);

        var importMachine = new OrchestrationReasoningMachine();
        var importResult = await importMachine.ImportAsync(saveResult.Value!);
        if (!importResult.IsSuccess)
            throw new InvalidOperationException("ImportAsync failed: " + importResult.ErrorCode + " " + importResult.ErrorMessage);

        var heldOutEvalRows = EvaluateHeldOut(repoRoot, loadResult.Value!.Machine, heldOutRows).ToArray();
        WriteJsonLines(Path.Combine(leaf, "orchestration-runtime-eval-results.jsonl"), heldOutEvalRows);

        var ablationRows = RunAblations(repoRoot, learnedState, heldOutRows, heldOutEvalRows).ToArray();
        WriteJsonLines(Path.Combine(leaf, "orchestration-leakage-ablation-checks.jsonl"), ablationRows);

        var passedCount = heldOutEvalRows.Count(row => row.Passed);
        var accuracy = Math.Round((double)passedCount / heldOutRows.Length, 4);
        var noStatePassed = ablationRows.Count(row => row.AblationKind == "no_trained_state" && row.ShortcutLeak);
        var sourceIdLeaks = ablationRows.Count(row => row.AblationKind == "source_id_only" && row.ShortcutLeak);
        var rowIdLeaks = ablationRows.Count(row => row.AblationKind == "row_id_only" && row.ShortcutLeak);
        var exactPromptLeaks = ablationRows.Count(row => row.AblationKind == "exact_prompt_hash_only" && row.ShortcutLeak);
        var candidateOrderChanges = ablationRows.Count(row => row.AblationKind == "candidate_order_reversed" && row.ShortcutLeak);
        var stackMarkerLeaks = ablationRows.Count(row => row.AblationKind == "project_stack_marker_only" && row.ShortcutLeak);

        var learnedPatternJson = JsonSerializer.Serialize(learnedState.LearnedPatterns, JsonOptions);
        var leakageReport = new
        {
            schema = "orchestration-reasoning-expanded-v1-leakage-ablation-report-v1",
            work_order_id = "QUALITY-PRODUCT-ORCHESTRATION-EXPANDED-V1-TRAINING-AUTHORIZATION-EVAL-02",
            generated_at_utc = DateTimeOffset.UtcNow,
            dataset_ref = Rel(repoRoot, datasetPath),
            train_rows = trainRows.Length,
            held_out_rows = heldOutRows.Length,
            source_disjoint = true,
            trained_runtime_eval_executed = true,
            train_only_candidate_checkpoint = Rel(repoRoot, checkpointPath),
            held_out_accuracy = accuracy,
            held_out_pass_count = passedCount,
            held_out_total = heldOutRows.Length,
            pass_as_trained_feature = false,
            pass_as_trained_feature_reason = "Runtime ran through a leaf-local train-only checkpoint, but exact held-out scenario labels are unseen by the current architecture, so trained feature readiness is disproven rather than proven.",
            no_leakage_checks = new
            {
                learned_patterns_exclude_held_out_training_pair_ids = heldOutRows.All(row => !learnedPatternJson.Contains(row.TrainingPairId, StringComparison.Ordinal)),
                learned_patterns_exclude_held_out_source_row_ids = heldOutRows.All(row => !learnedPatternJson.Contains(row.SourceRowId, StringComparison.Ordinal)),
                learned_patterns_exclude_held_out_source_manifest_refs = heldOutRows.All(row => !learnedPatternJson.Contains(row.SourceManifestRef, StringComparison.Ordinal)),
                learned_patterns_exclude_held_out_expected_scenario_refs = heldOutRows.All(row => !learnedState.LearnedPatterns!.Any(pattern => string.Equals(pattern.ScenarioRef, row.ChosenScenarioRef, StringComparison.Ordinal))),
                source_id_only_shortcut_leaks = sourceIdLeaks,
                row_id_only_shortcut_leaks = rowIdLeaks,
                exact_prompt_hash_only_shortcut_leaks = exactPromptLeaks,
                candidate_order_shortcut_changes = candidateOrderChanges,
                project_stack_marker_shortcut_leaks = stackMarkerLeaks,
                no_trained_state_false_passes = noStatePassed
            },
            forbidden_oracle_note = "A checkpoint that copies held-out expected scenario refs into learned patterns would pass by leakage; this runner did not create that checkpoint and records exact held-out label generation as the next architecture gap.",
            semantic_quality_claimed = false,
            product_ready_claimed = false,
            production_locator_updated = false
        };
        WriteJson(Path.Combine(leaf, "orchestration-leakage-ablation-checks.json"), leakageReport);

        var deltaPattern = patterns[0] with
        {
            PatternId = "or-expanded-v1-continue-probe-pattern-001",
            ScenarioRef = patterns[0].ScenarioRef + ":continue-probe"
        };
        var deltaState = BuildLearnedState([deltaPattern], datasetSha, splitSha, schemaSha) with
        {
            StateId = learnedState.StateId + ":continue-probe-delta",
            GuideStateRef = learnedState.GuideStateRef + ":continue-probe-delta"
        };
        var continueResult = importMachine.ContinueTrainingFromLearnedState(
            deltaState,
            ["continue-probe:train-row-001"]);
        if (!continueResult.IsSuccess)
            throw new InvalidOperationException("ContinueTrainingFromLearnedState failed: " + continueResult.ErrorCode + " " + continueResult.ErrorMessage);
        var continuedDir = Path.Combine(leaf, "continued-candidate");
        var continuedCheckpointPath = Path.Combine(continuedDir, "checkpoint.json");
        var continuedManifestPath = Path.Combine(continuedDir, "manifest.json");
        var continuedSaveResult = await importMachine.SaveCheckpointAsync(
            continuedCheckpointPath,
            continuedManifestPath,
            Rel(repoRoot, datasetCardPath),
            continueResult.Value!.MergedAcceptedRefIds);
        if (!continuedSaveResult.IsSuccess)
            throw new InvalidOperationException("Continued SaveCheckpointAsync failed: " + continuedSaveResult.ErrorCode + " " + continuedSaveResult.ErrorMessage);

        var rollbackLoadResult = await OrchestrationReasoningMachine.LoadCheckpointAsync(
            Path.Combine(leaf, "rollback", "checkpoint.json"),
            Path.Combine(leaf, "rollback", "manifest.json"));
        if (!rollbackLoadResult.IsSuccess)
            throw new InvalidOperationException("Rollback LoadCheckpointAsync failed: " + rollbackLoadResult.ErrorCode + " " + rollbackLoadResult.ErrorMessage);

        var lifecycle = new
        {
            schema = "orchestration-reasoning-expanded-v1-leaf-local-lifecycle-evidence-v1",
            work_order_id = "QUALITY-PRODUCT-ORCHESTRATION-EXPANDED-V1-TRAINING-AUTHORIZATION-EVAL-02",
            generated_at_utc = DateTimeOffset.UtcNow,
            checkpoint_ref = Rel(repoRoot, checkpointPath),
            manifest_ref = Rel(repoRoot, manifestPath),
            checkpoint_sha256 = checkpointSha,
            manifest_sha256 = manifestSha,
            backup_checkpoint_ref = Rel(repoRoot, Path.Combine(leaf, "backup", "checkpoint.json")),
            rollback_checkpoint_ref = Rel(repoRoot, Path.Combine(leaf, "rollback", "checkpoint.json")),
            backup_checkpoint_sha256 = Sha256File(Path.Combine(leaf, "backup", "checkpoint.json")),
            rollback_checkpoint_sha256 = Sha256File(Path.Combine(leaf, "rollback", "checkpoint.json")),
            export_probe_passed = saveResult.IsSuccess,
            import_probe_passed = importResult.IsSuccess,
            fresh_load_probe_passed = loadResult.IsSuccess,
            rollback_load_probe_passed = rollbackLoadResult.IsSuccess,
            continue_training_probe_passed = continueResult.IsSuccess,
            continued_checkpoint_ref = Rel(repoRoot, continuedCheckpointPath),
            continued_checkpoint_sha256 = Sha256File(continuedCheckpointPath),
            continue_training_run_claimed = continueResult.Value.ContinueTrainingRunClaimed,
            checkpoint_rewritten_by_continue_result = continueResult.Value.CheckpointRewritten,
            production_locator_updated = false,
            production_model_promoted = false,
            semantic_quality_claimed = false,
            host_runtime_claimed = false,
            note = "All lifecycle probes are leaf-local. They prove boundaries exist for this candidate only; they do not promote a common production model."
        };
        WriteJson(Path.Combine(leaf, "model-lifecycle-evidence.json"), lifecycle);

        stopwatch.Stop();
        process.Refresh();
        var summary = new
        {
            schema = "orchestration-reasoning-expanded-v1-runtime-runner-output-v1",
            work_order_id = "QUALITY-PRODUCT-ORCHESTRATION-EXPANDED-V1-TRAINING-AUTHORIZATION-EVAL-02",
            dataset_sha256 = datasetSha,
            split_sha256 = splitSha,
            schema_sha256 = schemaSha,
            dataset_card_sha256 = datasetCardSha,
            train_rows = trainRows.Length,
            held_out_rows = heldOutRows.Length,
            learned_pattern_count = patterns.Length,
            source_disjoint = true,
            checkpoint_ref = Rel(repoRoot, checkpointPath),
            manifest_ref = Rel(repoRoot, manifestPath),
            checkpoint_sha256 = checkpointSha,
            manifest_sha256 = manifestSha,
            held_out_pass_count = passedCount,
            held_out_total = heldOutRows.Length,
            held_out_accuracy = accuracy,
            trained_runtime_eval_executed = true,
            trained_feature_claimed = false,
            semantic_quality_claimed = false,
            product_ready_claimed = false,
            production_locator_updated = false,
            elapsed_ms = Math.Round(stopwatch.Elapsed.TotalMilliseconds, 3),
            cpu_ms = Math.Round(process.TotalProcessorTime.TotalMilliseconds - cpuStart, 3),
            ram_mb = Math.Round(process.WorkingSet64 / 1024.0 / 1024.0, 3)
        };
        WriteJson(Path.Combine(leaf, "runtime-runner-output.json"), summary);
        Console.WriteLine(JsonSerializer.Serialize(summary, JsonOptions));
        return 0;
    }

    private static OrchestrationReasoningLearnedGuideStateDescriptor BuildLearnedState(
        IReadOnlyList<OrchestrationReasoningLearnedPattern> patterns,
        string datasetSha,
        string splitSha,
        string schemaSha)
    {
        var stateMaterial = JsonSerializer.Serialize(patterns, JsonOptions);
        return new OrchestrationReasoningLearnedGuideStateDescriptor(
            StateId: "orchestration-reasoning-state:expanded-v1:leaf-local-train-only",
            GuideStateRef: "leaf-local:orchestration-reasoning-expanded-v1-train-only",
            ManifestId: "leaf-local-manifest-created-after-save",
            ManifestSha256: datasetSha,
            MachineType: nameof(OrchestrationReasoningMachine),
            SchemaVersion: "orchestration-reasoning-state-v1",
            SourceBaseCommit: "dataset-sha256:" + datasetSha + ";split-sha256:" + splitSha + ";schema-sha256:" + schemaSha,
            StateSha256: Sha256Text(stateMaterial),
            LearnedPatterns: patterns);
    }

    private static OrchestrationReasoningLearnedPattern BuildTrainOnlyPattern(ExpandedRow row, int ordinal)
    {
        var signals = BuildSemanticSignals(row);
        return new OrchestrationReasoningLearnedPattern(
            PatternId: "or-expanded-v1-train-pattern-" + ordinal.ToString("000"),
            ScenarioRef: row.ChosenScenarioRef,
            RequiredTaskTypes: ["universal_reasoning"],
            RequiredConstraintSignals: signals,
            RequiredRequirementSignals: [],
            RequiredDeliverableSignals: [],
            RequiredEvidenceSignals: [],
            PhaseFamilies: ["orchestration-reasoning", "expanded-v1-local-eval"],
            GateFamilies: ["source-disjoint", "leakage-ablation", row.ErrorCode],
            AssetFamilies: ["leaf-local-candidate", "llm_mvp_core"],
            HostCheckSets: ["core-only-no-host-runtime"],
            RiskFlags: ["semantic_quality_claimed_false", "no_production_locator_update", "train_only_patterns_no_heldout_labels"],
            NextGranularizationActions: ["repair scenario generalization before readiness claim"],
            Confidence: Math.Min(0.92, 0.7 + (signals.Count * 0.025)));
    }

    private static IReadOnlyList<RuntimeEvalRow> EvaluateHeldOut(
        string repoRoot,
        OrchestrationReasoningMachine machine,
        IReadOnlyList<ExpandedRow> heldOutRows)
    {
        var rows = new List<RuntimeEvalRow>();
        for (var index = 0; index < heldOutRows.Count; index++)
        {
            var sourceRow = heldOutRows[index];
            var request = CreateRuntimeRequest(sourceRow, index + 1, "semantic-signal-proxy");
            var response = machine.Plan(request);
            if (!response.IsSuccess)
                throw new InvalidOperationException("Plan failed: " + response.ErrorCode + " " + response.ErrorMessage);

            var observedScenario = response.Value!.ScenarioPlan.ScenarioId;
            var selectedPattern = response.Value.ScenarioPlan.SelectedLearnedPatterns.FirstOrDefault() ?? "";
            var passed = string.Equals(observedScenario, sourceRow.ChosenScenarioRef, StringComparison.Ordinal);
            rows.Add(new RuntimeEvalRow(
                Type: "held_out_runtime_eval",
                HeldOutCase: "heldout-case-" + (index + 1).ToString("000"),
                SourceDisjointFromTrain: true,
                NoLeakageFeatures: true,
                RuntimeMethod: "OrchestrationReasoningMachine.LoadCheckpointAsync + OrchestrationReasoningMachine.Plan",
                InputFeatureScope: "semantic signal proxy from held-out row teaching/reason text; excludes TrainingPairId, SourceRowId, SourceManifestRef, SourceGuideSha256, ChosenScenarioRef, RejectedScenarioRef, row order, file path, guide title, and raw prompt text",
                ExpectedScenarioRefSha256: Sha256Text(sourceRow.ChosenScenarioRef),
                ObservedScenarioRefSha256: Sha256Text(observedScenario),
                ObservedContractStatus: response.Value.ContractStatus,
                ObservedSelectedPatternSha256: string.IsNullOrWhiteSpace(selectedPattern) ? "" : Sha256Text(selectedPattern),
                Passed: passed,
                Reasoning: passed
                    ? "The leaf-local train-only checkpoint selected the held-out expected scenario without source/id/prompt/order features."
                    : "The current architecture selects only scenario refs present in learned train patterns or returns not-ready, so source-disjoint held-out scenario labels are not proven.",
                SemanticQualityClaimed: false,
                ProductReadyClaimed: false));
        }

        return rows;
    }

    private static IReadOnlyList<AblationRow> RunAblations(
        string repoRoot,
        OrchestrationReasoningLearnedGuideStateDescriptor learnedState,
        IReadOnlyList<ExpandedRow> heldOutRows,
        IReadOnlyList<RuntimeEvalRow> baselineRows)
    {
        var rows = new List<AblationRow>();
        var trainedMachine = new OrchestrationReasoningMachine(new StaticLoader(learnedState));
        var reversedState = learnedState with
        {
            StateId = learnedState.StateId + ":candidate-order-reversed",
            LearnedPatterns = learnedState.LearnedPatterns!.Reverse().ToArray()
        };
        var reversedMachine = new OrchestrationReasoningMachine(new StaticLoader(reversedState));

        for (var index = 0; index < heldOutRows.Count; index++)
        {
            var sourceRow = heldOutRows[index];
            var heldOutCase = "heldout-case-" + (index + 1).ToString("000");
            var baseline = baselineRows[index];
            rows.Add(RunAblationCase("no_trained_state", heldOutCase, new OrchestrationReasoningMachine(), CreateRuntimeRequest(sourceRow, index + 1, "semantic-signal-proxy"), baseline.ObservedScenarioRefSha256, expectSameAsBaseline: false));
            rows.Add(RunAblationCase("source_id_only", heldOutCase, trainedMachine, CreateMarkerOnlyRequest(sourceRow, index + 1, "source", sourceRow.SourceManifestRef), baseline.ObservedScenarioRefSha256, expectSameAsBaseline: false));
            rows.Add(RunAblationCase("row_id_only", heldOutCase, trainedMachine, CreateMarkerOnlyRequest(sourceRow, index + 1, "row", sourceRow.TrainingPairId + "|" + sourceRow.SourceRowId), baseline.ObservedScenarioRefSha256, expectSameAsBaseline: false));
            rows.Add(RunAblationCase("exact_prompt_hash_only", heldOutCase, trainedMachine, CreateMarkerOnlyRequest(sourceRow, index + 1, "prompt", "sha256:" + Sha256Text(sourceRow.TrainingPairId)), baseline.ObservedScenarioRefSha256, expectSameAsBaseline: false));
            rows.Add(RunAblationCase("project_stack_marker_only", heldOutCase, trainedMachine, CreateMarkerOnlyRequest(sourceRow, index + 1, "stack", "llm_mvp_core|project-governance|codex"), baseline.ObservedScenarioRefSha256, expectSameAsBaseline: false));
            rows.Add(RunAblationCase("candidate_order_reversed", heldOutCase, reversedMachine, CreateRuntimeRequest(sourceRow, index + 1, "semantic-signal-proxy"), baseline.ObservedScenarioRefSha256, expectSameAsBaseline: true));
        }

        return rows;
    }

    private static AblationRow RunAblationCase(
        string kind,
        string heldOutCase,
        OrchestrationReasoningMachine machine,
        OrchestrationReasoningRequest request,
        string baselineScenarioHash,
        bool expectSameAsBaseline)
    {
        var response = machine.Plan(request);
        if (!response.IsSuccess)
            throw new InvalidOperationException("Ablation Plan failed: " + response.ErrorCode + " " + response.ErrorMessage);
        var observedScenarioHash = Sha256Text(response.Value!.ScenarioPlan.ScenarioId);
        var selectedPatterns = response.Value.ScenarioPlan.SelectedLearnedPatterns.Count;
        var sameAsBaseline = string.Equals(observedScenarioHash, baselineScenarioHash, StringComparison.Ordinal);
        var leak = expectSameAsBaseline ? !sameAsBaseline : selectedPatterns > 0;
        return new AblationRow(
            Type: "ablation_control",
            HeldOutCase: heldOutCase,
            AblationKind: kind,
            ObservedContractStatus: response.Value.ContractStatus,
            ObservedScenarioRefSha256: observedScenarioHash,
            SelectedPatternCount: selectedPatterns,
            ShortcutLeak: leak,
            Passed: !leak,
            Reasoning: leak
                ? "Control changed or selected a learned pattern using a forbidden marker."
                : "Control did not create a forbidden shortcut; candidate-order reversal preserved the same decision when applicable.");
    }

    private static OrchestrationReasoningRequest CreateRuntimeRequest(
        ExpandedRow row,
        int ordinal,
        string inputScope)
    {
        var signals = BuildSemanticSignals(row);
        return new OrchestrationReasoningRequest(
            RequestId: "expanded-v1-heldout-runtime-" + ordinal.ToString("000"),
            Task: new StructuredTaskContract(
                PromptId: "expanded-v1-heldout-redacted-" + ordinal.ToString("000"),
                RawPromptSha256: Sha256Text("heldout-semantic-proxy:" + ordinal.ToString("000") + ":" + string.Join("|", signals)),
                UserLanguage: "en",
                TaskType: "universal_reasoning",
                ExplicitUserRequirements: [],
                Constraints: signals,
                Deliverables: [],
                ForbiddenActions: ["source_id_lookup", "row_id_lookup", "exact_prompt_lookup", "candidate_order_authority", "project_stack_marker_authority"],
                RequiredHosts: ["core-only"],
                EvidenceNeeds: ["source_disjoint_eval_evidence", "no_leakage_ablation_evidence"],
                SourceProvenance: new Dictionary<string, string>
                {
                    ["input_scope"] = inputScope,
                    ["raw_prompt_text"] = "not_stored",
                    ["row_ids"] = "redacted"
                },
                SplitId: "orchestration-reasoning-expanded-v1-heldout"),
            GuideDerivedStateRefs: [],
            CorpusBundleIds: ["orchestration-reasoning-expanded-v1"],
            AdaptiveContextRefs: new Dictionary<string, string>(),
            RuntimePolicyRefs: new Dictionary<string, string>
            {
                ["deterministic_shortcut"] = "forbidden",
                ["production_locator_update"] = "forbidden"
            });
    }

    private static OrchestrationReasoningRequest CreateMarkerOnlyRequest(
        ExpandedRow row,
        int ordinal,
        string markerKind,
        string marker)
        => new(
            RequestId: "expanded-v1-ablation-" + markerKind + "-" + ordinal.ToString("000"),
            Task: new StructuredTaskContract(
                PromptId: "expanded-v1-ablation-redacted-" + ordinal.ToString("000"),
                RawPromptSha256: Sha256Text("heldout-marker:" + markerKind + ":" + ordinal.ToString("000")),
                UserLanguage: "en",
                TaskType: "universal_reasoning",
                ExplicitUserRequirements: [],
                Constraints: [Normalize(marker)],
                Deliverables: [],
                ForbiddenActions: ["source_id_lookup", "row_id_lookup", "exact_prompt_lookup", "candidate_order_authority", "project_stack_marker_authority"],
                RequiredHosts: ["core-only"],
                EvidenceNeeds: [],
                SourceProvenance: new Dictionary<string, string>
                {
                    ["ablation_marker_kind"] = markerKind,
                    ["marker_value"] = "redacted"
                },
                SplitId: "orchestration-reasoning-expanded-v1-heldout-ablation"),
            GuideDerivedStateRefs: [],
            CorpusBundleIds: ["orchestration-reasoning-expanded-v1-ablation"],
            AdaptiveContextRefs: new Dictionary<string, string>(),
            RuntimePolicyRefs: new Dictionary<string, string>
            {
                ["deterministic_shortcut"] = "forbidden"
            });

    private static IReadOnlyList<string> BuildSemanticSignals(ExpandedRow row)
    {
        var text = string.Join(" ", new[]
        {
            Get(row.ProvenanceRefs, "teaches_boundary"),
            Get(row.ProvenanceRefs, "chosen_quality_reason"),
            Get(row.ProvenanceRefs, "rejected_quality_reason"),
            row.NearMissStrategyRef
        }).ToLowerInvariant();
        var signals = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        AddIf(signals, text, ["interrupt", "answer first", "continuation", "role boundary", "subagent", "sub-agent", "bounded executor", "parent review"], "authority_boundary_reasoning");
        AddIf(signals, text, ["evidence", "verification", "claim", "static", "json", "report", "gate", "metric"], "evidence_and_claim_scope_reasoning");
        AddIf(signals, text, ["source quality", "curation", "material", "dpo", "pair creation", "provenance"], "source_curation_reasoning");
        AddIf(signals, text, ["deterministic", "shortcut", "filename", "title", "static route", "bm25", "ablation"], "deterministic_shortcut_firewall_reasoning");
        AddIf(signals, text, ["plan", "approval", "artifact type", "implementation", "fresh acceptance"], "plan_approval_boundary_reasoning");
        AddIf(signals, text, ["representation", "storage", "compatibility", "axis", "decompose", "single policy"], "multi_axis_decomposition_reasoning");
        AddIf(signals, text, ["common", "adaptive", "user-local", "user local", "local preference"], "common_adaptive_boundary_reasoning");
        AddIf(signals, text, ["held-out", "heldout", "source-disjoint", "source disjoint", "split"], "source_disjoint_split_reasoning");
        AddIf(signals, text, ["product decision", "decision gap", "operator decision", "safe independent"], "product_decision_gap_reasoning");
        if (signals.Count == 0)
            signals.Add("general_orchestration_boundary_reasoning");
        return signals.Select(Normalize).ToArray();
    }

    private static void AddIf(SortedSet<string> signals, string text, IReadOnlyList<string> needles, string signal)
    {
        if (needles.Any(needle => text.Contains(needle, StringComparison.OrdinalIgnoreCase)))
            signals.Add(signal);
    }

    private static string Normalize(string value)
        => value.Trim().ToLowerInvariant();

    private static string Get(IReadOnlyDictionary<string, string> values, string key)
        => values.TryGetValue(key, out var value) ? value : string.Empty;

    private static void CopyPair(string checkpointPath, string manifestPath, string targetDir)
    {
        Directory.CreateDirectory(targetDir);
        File.Copy(checkpointPath, Path.Combine(targetDir, "checkpoint.json"), overwrite: true);
        File.Copy(manifestPath, Path.Combine(targetDir, "manifest.json"), overwrite: true);
    }

    private static void WriteJson(string path, object value)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(value, JsonOptions).Replace("\r\n", "\n") + "\n", new UTF8Encoding(false));
    }

    private static void WriteJsonLines(string path, IEnumerable<object> rows)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, string.Join("\n", rows.Select(row => JsonSerializer.Serialize(row, LineOptions))) + "\n", new UTF8Encoding(false));
    }

    private static string Rel(string repoRoot, string path)
        => Path.GetRelativePath(repoRoot, path).Replace('\\', '/');

    private static string Sha256File(string path)
        => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

    private static string Sha256Text(string text)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();

    private sealed class StaticLoader(OrchestrationReasoningLearnedGuideStateDescriptor descriptor)
        : IOrchestrationReasoningLearnedGuideStateLoader
    {
        public AgentInfrastructure.OperationResult<OrchestrationReasoningLearnedGuideStateDescriptor> Load()
            => AgentInfrastructure.OperationResult<OrchestrationReasoningLearnedGuideStateDescriptor>.Ok(descriptor);
    }

    private sealed record ExpandedRow(
        string TrainingPairId,
        string SourceRowId,
        string ApprovedPairRef,
        string ChosenScenarioRef,
        string RejectedScenarioRef,
        string ErrorCode,
        string ErrorTaxonomyRef,
        string SourceGuideSha256,
        string SourceManifestRef,
        string SplitId,
        string SplitRole,
        string HeldOutSetId,
        string NearMissStrategyRef,
        bool ContainsRawPrivateText,
        IReadOnlyDictionary<string, string> ProvenanceRefs);

    private sealed record RuntimeEvalRow(
        string Type,
        string HeldOutCase,
        bool SourceDisjointFromTrain,
        bool NoLeakageFeatures,
        string RuntimeMethod,
        string InputFeatureScope,
        string ExpectedScenarioRefSha256,
        string ObservedScenarioRefSha256,
        string ObservedContractStatus,
        string ObservedSelectedPatternSha256,
        bool Passed,
        string Reasoning,
        bool SemanticQualityClaimed,
        bool ProductReadyClaimed);

    private sealed record AblationRow(
        string Type,
        string HeldOutCase,
        string AblationKind,
        string ObservedContractStatus,
        string ObservedScenarioRefSha256,
        int SelectedPatternCount,
        bool ShortcutLeak,
        bool Passed,
        string Reasoning);
}
