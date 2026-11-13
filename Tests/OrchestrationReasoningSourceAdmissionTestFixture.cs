// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using SmartSkillsDomain;

internal static class OrchestrationReasoningSourceAdmissionTestFixture
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    public static OrchestrationReasoningAdmittedLearnedState Admit(
        OrchestrationReasoningLearnedGuideStateDescriptor source,
        string seed = "fixture",
        string? root = null)
        => Admit(source.LearnedPatterns ?? [], source.StateId, seed, root: root);

    public static OrchestrationReasoningAdmittedLearnedState Admit(
        IReadOnlyList<OrchestrationReasoningLearnedPattern> patterns,
        string stateId = "fixture-state",
        string seed = "fixture",
        string sourceKind = OrchestrationReasoningSourceAdmission.ProjectInternalSelfAuthored,
        string? root = null)
    {
        if (patterns.Count == 0)
            throw new InvalidOperationException("A real evidence-backed fixture requires at least one learned pattern.");

        OrchestrationReasoningAdmittedLearnedState? admitted = null;
        for (var index = 0; index < patterns.Count; index++)
        {
            var itemSeed = $"{seed}-{index}";
            var corpus = CreateCorpus(
                root: root is null ? null : Path.Combine(root, $"source-{index + 1:D2}"),
                sourceKind: sourceKind,
                pairId: $"pair:{itemSeed}",
                sourceRowId: $"row:{itemSeed}",
                sourceId: $"source:{itemSeed}",
                sourceRef: $"fixture://source/{itemSeed}",
                sourceSha256: Hash($"source:{itemSeed}"),
                learnedPattern: patterns[index]);
            var materialized = new OrchestrationReasoningTrainer()
                .MaterializeAsync(
                    corpus.PayloadPath,
                    corpus.ManifestPath,
                    corpus.LedgerPath,
                    corpus.EvidencePath,
                    learnedStateManifestRef: CreateBundleLocalManifestRef(root, index))
                .GetAwaiter()
                .GetResult();
            if (!materialized.IsSuccess)
                throw new InvalidOperationException(materialized.ErrorMessage);
            if (admitted is null)
            {
                admitted = materialized.Value!.AdmittedState;
                continue;
            }

            var machine = new OrchestrationReasoningMachine();
            var imported = machine.ImportAdmittedState(admitted);
            if (!imported.IsSuccess)
                throw new InvalidOperationException(imported.ErrorMessage);
            var continued = machine.ContinueTrainingFromLearnedState(
                materialized.Value!.AdmittedState,
                materialized.Value.AdmittedState.Receipt.PairIds);
            if (!continued.IsSuccess)
                throw new InvalidOperationException(continued.ErrorMessage);
            admitted = continued.Value!.AdmittedState;
        }

        _ = stateId;
        return admitted!;
    }

    private static string? CreateBundleLocalManifestRef(string? sourceAdmissionRoot, int sourceIndex)
    {
        if (string.IsNullOrWhiteSpace(sourceAdmissionRoot))
            return null;

        var rootName = Path.GetFileName(Path.TrimEndingDirectorySeparator(sourceAdmissionRoot));
        if (string.IsNullOrWhiteSpace(rootName))
            return null;

        return Path.Combine(rootName, $"source-{sourceIndex + 1:D2}", "manifest.json")
            .Replace('\\', '/'); // The fixture stores the same bundle-local manifest ref that SaveCheckpoint will later bind into proof.
    }

    public static OrchestrationReasoningCheckpoint Checkpoint(
        OrchestrationReasoningAdmittedLearnedState admittedState,
        IReadOnlyList<string>? acceptedRefs = null)
        => new(
            SchemaVersion: 2,
            LearnedState: admittedState.State,
            AcceptedRefIds: acceptedRefs ?? admittedState.Receipt.PairIds,
            SourceManifestRef: admittedState.State.ManifestId,
            SemanticQualityClaimed: false,
            ContinueTrainingClaimed: false,
            HostRuntimeClaimed: false,
            SavedAtUtc: DateTimeOffset.UtcNow,
            AdmissionReceipt: admittedState.Receipt,
            AdmissionProofRef: "fixture.source-admission-proof.json",
            AdmissionProofSha256: admittedState.Receipt.ReceiptSha256);

    public static SyntheticCorpus CreateCorpus(
        string? root = null,
        string sourceKind = OrchestrationReasoningSourceAdmission.ProjectInternalSelfAuthored,
        string pairId = "fixture-pair-001",
        string sourceRowId = "fixture-row-001",
        string sourceId = "fixture-source-001",
        string sourceRef = "fixture://self-authored/source-001",
        string? sourceSha256 = null,
        OrchestrationReasoningLearnedPattern? learnedPattern = null)
    {
        root ??= Path.Combine(Path.GetTempPath(), $"or-source-admission-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        sourceSha256 ??= Hash($"source:{sourceId}");

        var payloadPath = Path.Combine(root, "payload.jsonl");
        var manifestPath = Path.Combine(root, "manifest.json");
        var ledgerPath = Path.Combine(root, "source-admission-ledger.json");
        var evidencePath = Path.Combine(root, "source-admission-evidence.json");
        var payload = new Dictionary<string, object?>
        {
            ["pair_id"] = pairId,
            ["source_row_id"] = sourceRowId,
            ["source_id"] = sourceId,
            ["approved_pair_ref"] = $"fixture://approved/{pairId}",
            ["error_code"] = "ORCH-FIXTURE",
            ["chosen_payload_ref"] = $"fixture://chosen-payload/{pairId}",
            ["rejected_payload_ref"] = $"fixture://rejected-payload/{pairId}",
            ["chosen_feature_contract_ref"] = $"fixture://chosen-features/{pairId}",
            ["rejected_feature_contract_ref"] = $"fixture://rejected-features/{pairId}",
            ["trainer_input_contract_ref"] = $"fixture://trainer-input/{pairId}",
            ["chosen_ref"] = $"fixture://scenario/{pairId}",
            ["rejected_ref"] = $"fixture://rejected/{pairId}",
            ["source_ref"] = sourceRef,
            ["source_sha256"] = sourceSha256,
            ["model_leg"] = "common",
            ["abstraction_layer"] = "universal_reasoning",
            ["raw_private_text_copied"] = false,
            ["technical_execution_leakage"] = false,
            ["external_ai_used"] = false,
            ["training_started"] = false,
            ["dpo_payload_authored"] = true,
            ["dpo_train_ready"] = false,
            ["excluded_technical_execution_refs"] = new[] { "host-command" },
            ["chosen_scenario_plan_features"] = new
            {
                intent_signals = new[] { "fixture_constraint" },
                reasoning_steps = new[] { "fixture_requirement" },
                evidence_expectations = new[] { "fixture_evidence" },
                decision_guards = new[] { "fixture_deliverable" }
            },
            ["rejected_scenario_plan_features"] = new
            {
                near_miss_signals = new[] { "fixture_near_miss" }
            },
            ["universal_reasoning_pattern"] = "fixture universal pattern"
        };
        if (learnedPattern is not null)
            payload["learned_pattern"] = learnedPattern;
        File.WriteAllText(
            payloadPath,
            JsonSerializer.Serialize(payload, JsonOptions) + "\n",
            new UTF8Encoding(false));
        var payloadSha = HashFile(payloadPath);

        var manifest = new
        {
            payloads_sha256 = payloadSha,
            source_pairs_sha256 = Hash("fixture-source-pairs"),
            row_count = 1,
            raw_private_text_copied = false,
            technical_execution_leakage = false,
            external_ai_used = false,
            training_started = false,
            training_allowed = true
        };
        File.WriteAllText(manifestPath, JsonSerializer.Serialize(manifest, JsonOptions), new UTF8Encoding(false));
        var manifestSha = HashFile(manifestPath);

        var ledger = new
        {
            schema_version = 1,
            entries = new[]
            {
                new
                {
                    source_kind = sourceKind,
                    row_id = sourceRowId,
                    source_row_id = sourceRowId,
                    source_id = sourceId,
                    pair_id = pairId,
                    source_ref = sourceRef,
                    source_sha256 = sourceSha256,
                    training_allowed = false
                }
            }
        };
        File.WriteAllText(ledgerPath, JsonSerializer.Serialize(ledger, JsonOptions), new UTF8Encoding(false));
        var ledgerSha = HashFile(ledgerPath);

        var evidence = new
        {
            schema_version = 1,
            verified_source_kind = sourceKind,
            payload_sha256 = payloadSha,
            manifest_sha256 = manifestSha,
            ledger_sha256 = ledgerSha,
            row_ids = new[] { sourceRowId },
            source_ids = new[] { sourceId },
            pair_ids = new[] { pairId },
            source_refs = new[] { sourceRef },
            source_sha256s = new[] { sourceSha256 },
            training_allowed = false
        };
        File.WriteAllText(evidencePath, JsonSerializer.Serialize(evidence, JsonOptions), new UTF8Encoding(false));

        return new SyntheticCorpus(root, payloadPath, manifestPath, ledgerPath, evidencePath);
    }

    public static string Hash(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    public static string HashFile(string path)
        => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

    public static AdaptiveEvidence CreateAdaptiveEvidence(string signalPath)
    {
        var root = Path.GetDirectoryName(signalPath)!;
        var manifestPath = Path.Combine(root, "adaptive-source-admission-manifest.json");
        var ledgerPath = Path.Combine(root, "adaptive-source-admission-ledger.json");
        var evidencePath = Path.Combine(root, "adaptive-source-admission-evidence.json");
        var rows = File.ReadLines(signalPath)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonNode.Parse(line)!.AsObject())
            .ToArray();
        var payloadSha = HashFile(signalPath);
        File.WriteAllText(
            manifestPath,
            JsonSerializer.Serialize(new
            {
                payloads_sha256 = payloadSha,
                row_count = rows.Length,
                source_kind = OrchestrationReasoningSourceAdmission.ILearningDynamicGraph,
                batch_id = $"adaptive-batch:{Hash(payloadSha)}"
            }, JsonOptions),
            new UTF8Encoding(false));
        var entries = rows.Select(row =>
        {
            var signalId = row["signalId"]!.GetValue<string>();
            var sourceId = row["sourcePromptLedgerId"]!.GetValue<string>();
            var rawHash = row["rawPromptSha256"]!.GetValue<string>();
            var chosen = row["chosenRef"]!.GetValue<string>();
            var rejected = row["rejectedRef"]!.GetValue<string>();
            return new
            {
                source_kind = OrchestrationReasoningSourceAdmission.ILearningDynamicGraph,
                pair_id = signalId,
                row_id = signalId,
                signal_id = signalId,
                source_id = sourceId,
                source_prompt_ledger_id = sourceId,
                source_ref = chosen,
                chosen_ref = chosen,
                rejected_ref = rejected,
                source_sha256 = rawHash,
                raw_prompt_sha256 = rawHash,
                row_sha256 = Hash(row.ToJsonString(JsonOptions))
            };
        }).ToArray();
        File.WriteAllText(
            ledgerPath,
            JsonSerializer.Serialize(new { schema_version = 1, entries }, JsonOptions),
            new UTF8Encoding(false));
        var ledgerSha = HashFile(ledgerPath);
        File.WriteAllText(
            evidencePath,
            JsonSerializer.Serialize(new
            {
                schema_version = 1,
                verified_source_kind = OrchestrationReasoningSourceAdmission.ILearningDynamicGraph,
                payload_sha256 = payloadSha,
                manifest_sha256 = HashFile(manifestPath),
                ledger_sha256 = ledgerSha,
                row_ids = entries.Select(entry => entry.row_id).ToArray(),
                source_ids = entries.Select(entry => entry.source_id).ToArray(),
                pair_ids = entries.Select(entry => entry.pair_id).ToArray(),
                source_refs = entries.Select(entry => entry.source_ref).ToArray(),
                source_sha256s = entries.Select(entry => entry.source_sha256).ToArray(),
                row_sha256s = entries.Select(entry => entry.row_sha256).ToArray(),
                chosen_refs = entries.Select(entry => entry.chosen_ref).ToArray(),
                rejected_refs = entries.Select(entry => entry.rejected_ref).ToArray()
            }, JsonOptions),
            new UTF8Encoding(false));
        return new AdaptiveEvidence(manifestPath, ledgerPath, evidencePath);
    }

    public static void RefreshBindings(SyntheticCorpus corpus)
    {
        var payloadRows = File.ReadLines(corpus.PayloadPath)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonNode.Parse(line)!.AsObject())
            .ToArray();
        var manifest = JsonNode.Parse(File.ReadAllText(corpus.ManifestPath))!.AsObject();
        manifest["payloads_sha256"] = HashFile(corpus.PayloadPath);
        manifest["row_count"] = payloadRows.Length;
        File.WriteAllText(
            corpus.ManifestPath,
            manifest.ToJsonString(JsonOptions),
            new UTF8Encoding(false));

        var evidence = JsonNode.Parse(File.ReadAllText(corpus.EvidencePath))!.AsObject();
        evidence["payload_sha256"] = HashFile(corpus.PayloadPath);
        evidence["manifest_sha256"] = HashFile(corpus.ManifestPath);
        evidence["ledger_sha256"] = HashFile(corpus.LedgerPath);
        evidence["row_ids"] = new JsonArray(payloadRows
            .Select(row => JsonValue.Create(row["source_row_id"]!.GetValue<string>()))
            .ToArray());
        evidence["source_ids"] = new JsonArray(payloadRows
            .Select(row => JsonValue.Create(row["source_id"]!.GetValue<string>()))
            .ToArray());
        evidence["pair_ids"] = new JsonArray(payloadRows
            .Select(row => JsonValue.Create(row["pair_id"]!.GetValue<string>()))
            .ToArray());
        evidence["source_refs"] = new JsonArray(payloadRows
            .Select(row => JsonValue.Create(row["source_ref"]!.GetValue<string>()))
            .ToArray());
        evidence["source_sha256s"] = new JsonArray(payloadRows
            .Select(row => JsonValue.Create(row["source_sha256"]!.GetValue<string>()))
            .ToArray());
        File.WriteAllText(
            corpus.EvidencePath,
            evidence.ToJsonString(JsonOptions),
            new UTF8Encoding(false));
    }

    internal sealed record SyntheticCorpus(
        string Root,
        string PayloadPath,
        string ManifestPath,
        string LedgerPath,
        string EvidencePath) : IDisposable
    {
        public void Dispose()
        {
            if (Directory.Exists(Root))
                Directory.Delete(Root, recursive: true);
        }
    }

    internal sealed record AdaptiveEvidence(
        string ManifestPath,
        string LedgerPath,
        string EvidencePath);
}
