// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Security.Cryptography;
using AgentInfrastructure;
using SmartSkillsDomain;

public sealed class ExtensionQuality03NonInterRaterProofProducerTests
{
    private static readonly string[] RequiredEvidenceFiles =
    [
        "row-specific-held-out-split.jsonl",
        "row-specific-source-disjoint-check.jsonl",
        "row-specific-nearest-neighbour-report.jsonl",
        "row-specific-ablation-report.jsonl",
        "row-specific-no-leakage-report.jsonl",
        "row-specific-no-memorization-report.jsonl",
        "row-specific-inter-rater-adjudication.jsonl",
        "per-row-blocking-proof-refs.jsonl"
    ];

    private static readonly CategoryFixture[] Categories =
    [
        new("held_out_split", "approved_held_out_source_corpus_manifest_ref"),
        new("source_disjoint", "row_specific_source_disjoint_pass_ref"),
        new("nearest_neighbour_similarity_risk", "row_specific_nearest_neighbour_pass_ref"),
        new("ablation", "row_specific_ablation_runner_pass_ref"),
        new("no_leakage", "row_specific_no_leakage_pass_ref"),
        new("no_memorization", "row_specific_no_memorization_pass_ref"),
        new("inter_rater_adjudication", "independent_second_rater_adjudication_pass_ref")
    ];

    private static readonly string[] NonInterRaterCategories =
    [
        "held_out_split",
        "source_disjoint",
        "nearest_neighbour_similarity_risk",
        "ablation",
        "no_leakage",
        "no_memorization"
    ];

    private static readonly string[] RequiredAblationControls =
    [
        "exact_prompt_lookup_ablation",
        "filename_or_bm25_route_ablation",
        "candidate_position_order_ablation",
        "nearest_only_retrieval_only_ablation"
    ];

    [Fact]
    public void Run_MissingCorpusAuthorityWritesConsumableDeferPackWithoutFabricatedPass()
    {
        using var workspace = TempWorkspace();
        var fixture = CreateFixture(workspace, includePromptAndOutputs: false, uniqueSources: false);
        var secondRaterPacket = CreateAuthorizedSecondRaterPacket(workspace);

        var produced = Produce(fixture, approvedRefs: false);

        Assert.True(produced.IsSuccess, produced.ErrorMessage);
        Assert.Equal(0, produced.Value!.NonInterRaterProofRefsPassProduced);
        Assert.Equal(9, produced.Value.BlockingProofRefsPassAfterProducer);
        Assert.Equal(0, produced.Value.RunnerReadyRows);
        Assert.False(produced.Value.RunnerReadyEvalInputsCreated);

        var runner = RunProofContractRunner(fixture.EvidencePackDirectory, fixture.OutputDirectory, secondRaterPacket);

        Assert.True(runner.IsSuccess, runner.ErrorMessage);
        Assert.Equal(9, runner.Value!.BlockingProofRefsPass);
        Assert.Equal(0, runner.Value.RunnerReadyRows);
        Assert.Equal(0, runner.Value.QualityPassRows);
    }

    [Fact]
    public void Run_ApprovedCorpusReplayTextAndOutputsProduceRunnerConsumableNonInterRaterPassRows()
    {
        using var workspace = TempWorkspace();
        var fixture = CreateFixture(workspace, includePromptAndOutputs: true, uniqueSources: true);
        var secondRaterPacket = CreateAuthorizedSecondRaterPacket(workspace);
        var proofBundleRoot = CreateValidProofBundle(workspace, fixture.Rows);

        var produced = Produce(fixture, approvedRefs: true, proofBundleRoot);

        Assert.True(produced.IsSuccess, produced.ErrorMessage);
        Assert.True(produced.Value!.ValidProofBundleLoaded);
        Assert.Equal(54, produced.Value!.NonInterRaterProofRefsPassProduced);
        Assert.Equal(63, produced.Value.BlockingProofRefsPassAfterProducer);
        Assert.All(produced.Value.CountsByCategory.Values, category => Assert.Equal(9, category.PassProduced));

        var runner = RunProofContractRunner(fixture.EvidencePackDirectory, fixture.OutputDirectory, secondRaterPacket);

        Assert.True(runner.IsSuccess, runner.ErrorMessage);
        Assert.Equal(63, runner.Value!.BlockingProofRefsPass);
        Assert.Equal(9, runner.Value.RunnerReadyRows);
        Assert.Equal(0, runner.Value.QualityPassRows);
        Assert.True(File.Exists(Path.Combine(fixture.RunnerOutputDirectory, "runner-ready-eval-inputs.jsonl")));
    }

    [Fact]
    public void Run_FakeNonEmptyApprovedRefsWithoutProofBundleProduceNoPassAndNotRunFlags()
    {
        using var workspace = TempWorkspace();
        var fixture = CreateFixture(workspace, includePromptAndOutputs: true, uniqueSources: true);
        var secondRaterPacket = CreateAuthorizedSecondRaterPacket(workspace);

        var produced = Produce(fixture, approvedRefs: true);

        Assert.True(produced.IsSuccess, produced.ErrorMessage);
        Assert.False(produced.Value!.ValidProofBundleLoaded);
        Assert.Equal(0, produced.Value.NonInterRaterProofRefsPassProduced);
        Assert.Equal(9, produced.Value.BlockingProofRefsPassAfterProducer);
        Assert.Contains(produced.Value.ProofBundleValidationMessages, message => message.Contains("INVALID_REF", StringComparison.Ordinal));

        var ablationRows = ReadJsonl(Path.Combine(fixture.OutputDirectory, "row-specific-ablation-runner-report.jsonl"));
        var replayRows = ReadJsonl(Path.Combine(fixture.OutputDirectory, "row-specific-no-leakage-pass-or-risk.jsonl"));
        Assert.All(ablationRows, row =>
        {
            Assert.False(row.RootElement.GetProperty("pass").GetBoolean());
            Assert.False(row.RootElement.GetProperty("runner_executed").GetBoolean());
            Assert.Equal("NOT_RUN_MISSING_INPUTS", row.RootElement.GetProperty("runner_status").GetString());
            Assert.Contains("INVALID_REF", row.RootElement.GetProperty("risk_codes").EnumerateArray().Select(item => item.GetString()));
        });
        Assert.All(replayRows, row =>
        {
            Assert.False(row.RootElement.GetProperty("pass").GetBoolean());
            Assert.False(row.RootElement.GetProperty("replay_probe_executed").GetBoolean());
            Assert.Contains("INVALID_REF", row.RootElement.GetProperty("risk_codes").EnumerateArray().Select(item => item.GetString()));
        });

        var runner = RunProofContractRunner(fixture.EvidencePackDirectory, fixture.OutputDirectory, secondRaterPacket);

        Assert.True(runner.IsSuccess, runner.ErrorMessage);
        Assert.Equal(9, runner.Value!.BlockingProofRefsPass);
        Assert.Equal(0, runner.Value.RunnerReadyRows);
        Assert.Equal(0, runner.Value.QualityPassRows);
    }

    [Fact]
    public void Run_GenericControlExecutedWithoutCategorySpecificFlagsCannotPassAblationLeakageOrMemorization()
    {
        using var workspace = TempWorkspace();
        var fixture = CreateFixture(workspace, includePromptAndOutputs: true, uniqueSources: true);
        var secondRaterPacket = CreateAuthorizedSecondRaterPacket(workspace);
        var proofBundleRoot = CreateValidProofBundle(workspace, fixture.Rows);
        RemoveCategorySpecificExecutionFlags(proofBundleRoot, "ablation", "no_leakage", "no_memorization");

        var produced = Produce(fixture, approvedRefs: true, proofBundleRoot);

        Assert.True(produced.IsSuccess, produced.ErrorMessage);
        Assert.Equal(27, produced.Value!.NonInterRaterProofRefsPassProduced);
        Assert.Equal(0, produced.Value.CountsByCategory["ablation"].PassProduced);
        Assert.Equal(0, produced.Value.CountsByCategory["no_leakage"].PassProduced);
        Assert.Equal(0, produced.Value.CountsByCategory["no_memorization"].PassProduced);

        var ablationRows = ReadJsonl(Path.Combine(fixture.OutputDirectory, "row-specific-ablation-runner-report.jsonl"));
        var leakageRows = ReadJsonl(Path.Combine(fixture.OutputDirectory, "row-specific-no-leakage-pass-or-risk.jsonl"));
        var memorizationRows = ReadJsonl(Path.Combine(fixture.OutputDirectory, "row-specific-no-memorization-pass-or-risk.jsonl"));
        Assert.All(ablationRows, row =>
        {
            Assert.False(row.RootElement.GetProperty("pass").GetBoolean());
            Assert.False(row.RootElement.GetProperty("runner_executed").GetBoolean());
            Assert.Contains("ABLATION_RUNNER_NOT_RUN", row.RootElement.GetProperty("risk_codes").EnumerateArray().Select(item => item.GetString()));
        });
        Assert.All(leakageRows, row =>
        {
            Assert.False(row.RootElement.GetProperty("pass").GetBoolean());
            Assert.False(row.RootElement.GetProperty("replay_probe_executed").GetBoolean());
            Assert.Contains("NO_LEAKAGE_REPLAY_PROBE_NOT_RUN", row.RootElement.GetProperty("risk_codes").EnumerateArray().Select(item => item.GetString()));
        });
        Assert.All(memorizationRows, row =>
        {
            Assert.False(row.RootElement.GetProperty("pass").GetBoolean());
            Assert.False(row.RootElement.GetProperty("replay_probe_executed").GetBoolean());
            Assert.Contains("NO_MEMORIZATION_REPLAY_PROBE_NOT_RUN", row.RootElement.GetProperty("risk_codes").EnumerateArray().Select(item => item.GetString()));
        });

        var runner = RunProofContractRunner(fixture.EvidencePackDirectory, fixture.OutputDirectory, secondRaterPacket);

        Assert.True(runner.IsSuccess, runner.ErrorMessage);
        Assert.Equal(36, runner.Value!.BlockingProofRefsPass);
        Assert.Equal(0, runner.Value.RunnerReadyRows);
        Assert.Equal(0, runner.Value.QualityPassRows);
    }

    [Fact]
    public void Run_AbsoluteOutOfBundleProofArtifactRefCannotPass()
    {
        using var workspace = TempWorkspace();
        var fixture = CreateFixture(workspace, includePromptAndOutputs: true, uniqueSources: true);
        var proofBundleRoot = CreateValidProofBundle(workspace, fixture.Rows);
        PointFirstProofRefOutsideBundle(workspace, proofBundleRoot, fixture.Rows[0], "source_disjoint");

        var produced = Produce(fixture, approvedRefs: true, proofBundleRoot);

        Assert.True(produced.IsSuccess, produced.ErrorMessage);
        Assert.Equal(53, produced.Value!.NonInterRaterProofRefsPassProduced);
        Assert.Equal(8, produced.Value.CountsByCategory["source_disjoint"].PassProduced);
        Assert.Contains(produced.Value.ProofBundleValidationMessages, message => message.Contains("escapes proof bundle root", StringComparison.Ordinal));

        var sourceRows = ReadJsonl(Path.Combine(fixture.OutputDirectory, "row-specific-source-disjoint-pass-or-risk.jsonl"));
        var firstSourceRow = sourceRows.Single(row => row.RootElement.GetProperty("row_id").GetString() == fixture.Rows[0].RowId);
        Assert.False(firstSourceRow.RootElement.GetProperty("pass").GetBoolean());
        Assert.Equal("INVALID_REF", firstSourceRow.RootElement.GetProperty("proof_validation_status").GetString());
        Assert.Contains("INVALID_REF", firstSourceRow.RootElement.GetProperty("risk_codes").EnumerateArray().Select(item => item.GetString()));
    }

    private static OperationResult<Quality03NonInterRaterProofProducerReport> Produce(
        FixturePaths fixture,
        bool approvedRefs,
        string proofBundleRoot = "")
        => new Quality03NonInterRaterProofProducer().Run(new Quality03NonInterRaterProofProducerOptions
        {
            EvidencePackDirectory = fixture.EvidencePackDirectory,
            SourceProofContractPackDirectory = fixture.SourceProofContractPackDirectory,
            OutputDirectory = fixture.OutputDirectory,
            RunId = "quality-03-non-inter-rater-proof-producer-test",
            ApprovedProofBundleRoot = proofBundleRoot,
            ApprovedTrainSourceCorpusManifestRef = approvedRefs ? "artifact://approved-train-source-corpus" : string.Empty,
            ApprovedHeldOutSplitManifestRef = approvedRefs ? "artifact://approved-held-out-split" : string.Empty,
            ApprovedRawPrivateCorpusRef = approvedRefs ? "user-local://approved-private-corpus" : string.Empty
        });

    private static OperationResult<Quality03ProofContractRunnerReport> RunProofContractRunner(
        string evidencePackDirectory,
        string proofContractPackDirectory,
        string secondRaterPacketPath)
        => new Quality03ProofContractRunner().Run(new Quality03ProofContractRunnerOptions
        {
            EvidencePackDirectory = evidencePackDirectory,
            ProofContractPackDirectory = proofContractPackDirectory,
            SecondRaterPacketPath = secondRaterPacketPath,
            OutputDirectory = Path.Combine(Path.GetDirectoryName(proofContractPackDirectory)!, "runner-output"),
            RunId = "quality-03-non-inter-rater-proof-producer-runner-test"
        });

    private static FixturePaths CreateFixture(
        string workspace,
        bool includePromptAndOutputs,
        bool uniqueSources)
    {
        var evidencePack = Path.Combine(workspace, "evidence-pack");
        var sourceProofPack = Path.Combine(workspace, "source-proof-pack");
        var output = Path.Combine(workspace, "output-proof-pack");
        var runnerOutput = Path.Combine(workspace, "runner-output");
        Directory.CreateDirectory(evidencePack);
        Directory.CreateDirectory(sourceProofPack);
        Directory.CreateDirectory(output);
        Directory.CreateDirectory(runnerOutput);

        var rows = Enumerable.Range(1, 9).Select(index => Row(index, includePromptAndOutputs, uniqueSources)).ToArray();
        foreach (var fileName in RequiredEvidenceFiles)
            WriteJsonl(Path.Combine(evidencePack, fileName), rows.Select(GenericEvidenceRow));

        WriteJsonl(Path.Combine(sourceProofPack, "per-row-blocking-proof-refs.jsonl"), rows.Select(BlockingProofRow));
        WriteJsonl(Path.Combine(sourceProofPack, "row-specific-independent-second-rater-adjudication.jsonl"), rows.Select(row => CategoryRow(row, "inter_rater_adjudication", false)));
        return new FixturePaths(evidencePack, sourceProofPack, output, runnerOutput, rows);
    }

    private static string CreateValidProofBundle(string workspace, IReadOnlyList<RowFixture> rows)
    {
        var bundleDirectory = Path.Combine(workspace, "valid-proof-bundle");
        var artifactsDirectory = Path.Combine(bundleDirectory, "artifacts");
        Directory.CreateDirectory(artifactsDirectory);
        var bundleRows = new JsonArray();
        foreach (var row in rows)
        {
            var proofs = new JsonObject();
            foreach (var category in NonInterRaterCategories)
            {
                var artifactName = $"{row.RowId}-{category}.json";
                var artifactPath = Path.Combine(artifactsDirectory, artifactName);
                WriteJson(artifactPath, ProofArtifact(row, category));
                proofs[category] = new JsonObject
                {
                    ["artifact_ref"] = $"artifacts/{artifactName}",
                    ["artifact_sha256"] = Sha256File(artifactPath),
                    ["status"] = "APPROVED"
                };
            }

            var bundleRow = CommonRow(row);
            bundleRow["proofs"] = proofs;
            bundleRows.Add(bundleRow);
        }

        WriteJson(
            Path.Combine(bundleDirectory, "quality-03-non-inter-rater-proof-bundle.json"),
            new JsonObject
            {
                ["schema"] = Quality03NonInterRaterProofProducer.ProofBundleSchema,
                ["status"] = "APPROVED",
                ["rows"] = bundleRows
            });
        return bundleDirectory;
    }

    private static void RemoveCategorySpecificExecutionFlags(string proofBundleRoot, params string[] categoryIds)
    {
        var bundlePath = Path.Combine(proofBundleRoot, "quality-03-non-inter-rater-proof-bundle.json");
        var bundle = JsonNode.Parse(File.ReadAllText(bundlePath, Encoding.UTF8))!.AsObject();
        foreach (var bundleRow in bundle["rows"]!.AsArray())
        {
            var proofs = bundleRow!["proofs"]!.AsObject();
            foreach (var categoryId in categoryIds)
            {
                var proofRef = proofs[categoryId]!.AsObject();
                var artifactPath = Path.GetFullPath(Path.Combine(proofBundleRoot, proofRef["artifact_ref"]!.GetValue<string>()));
                var artifact = JsonNode.Parse(File.ReadAllText(artifactPath, Encoding.UTF8))!.AsObject();
                artifact["control_executed"] = true;
                artifact.Remove("runner_executed");
                artifact.Remove("replay_probe_executed");
                WriteJson(artifactPath, artifact);
                proofRef["artifact_sha256"] = Sha256File(artifactPath);
            }
        }

        WriteJson(bundlePath, bundle);
    }

    private static void PointFirstProofRefOutsideBundle(
        string workspace,
        string proofBundleRoot,
        RowFixture row,
        string categoryId)
    {
        var outsideDirectory = Path.Combine(workspace, "outside-proof-artifacts");
        Directory.CreateDirectory(outsideDirectory);
        var outsideArtifactPath = Path.Combine(outsideDirectory, $"{row.RowId}-{categoryId}.json");
        WriteJson(outsideArtifactPath, ProofArtifact(row, categoryId));

        var bundlePath = Path.Combine(proofBundleRoot, "quality-03-non-inter-rater-proof-bundle.json");
        var bundle = JsonNode.Parse(File.ReadAllText(bundlePath, Encoding.UTF8))!.AsObject();
        var firstRow = bundle["rows"]!.AsArray()
            .Select(item => item!.AsObject())
            .Single(item => item["row_id"]!.GetValue<string>() == row.RowId);
        var proofRef = firstRow["proofs"]![categoryId]!.AsObject();
        proofRef["artifact_ref"] = Path.GetFullPath(outsideArtifactPath).Replace('\\', '/');
        proofRef["artifact_sha256"] = Sha256File(outsideArtifactPath);
        WriteJson(bundlePath, bundle);
    }

    private static JsonObject ProofArtifact(RowFixture row, string categoryId)
    {
        var json = CommonRow(row);
        json["schema"] = Quality03NonInterRaterProofProducer.ProofArtifactSchema;
        json["category_id"] = categoryId;
        json["status"] = "APPROVED";
        json["pass"] = true;
        json["proof_status"] = "PASS";
        json["blocking_ref_resolved"] = true;
        json["control_executed"] = true;
        json["external_ai_model_called"] = false;
        json["training_dpo_checkpoint_model_locator_mutation"] = false;
        switch (categoryId)
        {
            case "held_out_split":
                json["held_out_split_executed"] = true;
                json["approved_assignment"] = "held_out";
                json["train_source_corpus_manifest_ref"] = "file:///approved/train-source-corpus-manifest.json";
                json["held_out_split_manifest_ref"] = "file:///approved/held-out-split-manifest.json";
                break;
            case "source_disjoint":
                json["source_disjoint_check_executed"] = true;
                json["source_disjoint_pass"] = true;
                json["overlap_count"] = 0;
                break;
            case "nearest_neighbour_similarity_risk":
                json["nearest_neighbour_check_executed"] = true;
                json["index_built"] = true;
                json["index_contract_present"] = true;
                json["max_similarity"] = 0.25;
                json["top_k_results"] = JsonArrayFrom(new[] { $"{row.RowId}-neighbour:0.25" });
                break;
            case "ablation":
                json["runner_executed"] = true;
                json["runner_contract_available_for_current_rows"] = true;
                json["shortcut_only"] = false;
                json["runner_status"] = "EXECUTED_PASS";
                json["controls_run"] = JsonArrayFrom(RequiredAblationControls);
                break;
            case "no_leakage":
                json["replay_probe_executed"] = true;
                json["approved_raw_private_refs"] = JsonArrayFrom(new[] { "user-local://approved-private-corpus" });
                json["leakage_found"] = false;
                json["exact_match_count"] = 0;
                break;
            case "no_memorization":
                json["replay_probe_executed"] = true;
                json["approved_raw_private_refs"] = JsonArrayFrom(new[] { "user-local://approved-private-corpus" });
                json["memorization_found"] = false;
                json["longest_common_fragment_token_count"] = 3;
                break;
        }

        return json;
    }

    private static string CreateAuthorizedSecondRaterPacket(string workspace)
    {
        var packetDirectory = Path.Combine(workspace, "authorized-second-rater-packet");
        Directory.CreateDirectory(packetDirectory);
        WriteJsonl(
            Path.Combine(packetDirectory, "per-row-authorized-second-rater-adjudication.jsonl"),
            Enumerable.Range(1, 9).Select(index => AuthorizedSecondRaterRow(Row(index, includePromptAndOutputs: true, uniqueSources: true), packetDirectory)));
        return packetDirectory;
    }

    private static JsonObject AuthorizedSecondRaterRow(RowFixture row, string packetDirectory)
    {
        var json = CategoryRow(row, "inter_rater_adjudication", true);
        var packetRef = Path.GetFullPath(Path.Combine(packetDirectory, "per-row-authorized-second-rater-adjudication.jsonl")).Replace('\\', '/') + "#" + row.RowId;
        json["artifact_ref"] = packetRef;
        json["concrete_proof_ref"] = packetRef;
        json["proof_status"] = "PROOF_PASS";
        json["independent_second_rater_available"] = true;
        json["reviewer_id_1"] = "reviewer-1";
        json["reviewer_id_2"] = "authorized-independent-reviewer-2";
        json["adjudication_status"] = "ADJUDICATED_PASS_LIMITED_TO_INTER_RATER_PROOF_REF";
        json["faked_independence"] = false;
        json["pass_scope"] = "INTER_RATER_ADJUDICATION_PROOF_REF_ONLY_NOT_QUALITY_PASS";
        json["can_satisfy_inter_rater_adjudication_proof"] = true;
        json["runner_ready_after_this_item"] = false;
        json["quality_pass_after_this_item"] = false;
        return json;
    }

    private static JsonObject BlockingProofRow(RowFixture row)
    {
        var categories = new JsonArray();
        foreach (var category in Categories)
        {
            categories.Add(new JsonObject
            {
                ["category_id"] = category.CategoryId,
                ["required_proof_ref"] = category.RequiredProofRef,
                ["runner_blocking"] = true,
                ["pass_or_risk"] = "RISK",
                ["proof_status"] = "RISK_DEFERRED_NOT_PASS",
                ["blocking_ref_resolved"] = false
            });
        }

        var json = CommonRow(row);
        json["category_results"] = categories;
        return json;
    }

    private static JsonObject GenericEvidenceRow(RowFixture row)
    {
        var json = CommonRow(row);
        json["artifact_row_produced"] = true;
        return json;
    }

    private static JsonObject CategoryRow(RowFixture row, string categoryId, bool pass)
    {
        var json = CommonRow(row);
        json["category_id"] = categoryId;
        json["artifact_ref"] = $"artifact://{categoryId}#{row.RowId}";
        json["pass"] = pass;
        json["pass_or_risk"] = pass ? "PASS" : "RISK";
        json["proof_status"] = pass ? "PASS" : "RISK_DEFERRED_NOT_PASS";
        json["blocking_ref_resolved"] = pass;
        json["external_ai_model_called"] = false;
        json["training_dpo_checkpoint_model_locator_mutation"] = false;
        return json;
    }

    private static JsonObject CommonRow(RowFixture row)
    {
        var json = new JsonObject
        {
            ["schema"] = "test.quality03.non_inter_rater.row.v1",
            ["row_id"] = row.RowId,
            ["pair_sequence_id"] = row.PairSequenceId,
            ["pair_id"] = row.PairId,
            ["scenario_id"] = row.ScenarioId,
            ["source_scenario_id"] = row.SourceScenarioId,
            ["host"] = row.Host,
            ["method"] = row.Method,
            ["evidence_class"] = "test_fixture",
            ["prompt_sha256"] = row.PromptSha256
        };
        if (!string.IsNullOrWhiteSpace(row.PromptText))
            json["prompt_text"] = row.PromptText;
        if (!string.IsNullOrWhiteSpace(row.BaselineOutputText))
            json["baseline_output_text"] = row.BaselineOutputText;
        if (!string.IsNullOrWhiteSpace(row.AdaptiveOutputText))
            json["adaptive_output_text"] = row.AdaptiveOutputText;
        return json;
    }

    private static JsonArray JsonArrayFrom(IEnumerable<string> values)
    {
        var array = new JsonArray();
        foreach (var value in values)
            array.Add(value);
        return array;
    }

    private static RowFixture Row(
        int index,
        bool includePromptAndOutputs,
        bool uniqueSources)
    {
        var promptText = includePromptAndOutputs
            ? DistinctPrompt(index)
            : string.Empty;
        var sourceScenarioId = uniqueSources
            ? $"SOURCE-SCENARIO-UNIQUE-{index:000}"
            : index == 1 ? "NO_SOURCE_SCENARIO_ID_IN_TOKEN_PAIR_ARTIFACT" : "SOURCE-SCENARIO-DUPLICATE";
        return new RowFixture(
            RowId: $"QUALITY-03-REVIEW-ROW-{index:000}",
            PairSequenceId: $"token-pair-{index:000}",
            PairId: $"TOKEN-PAIR-TEST-{index:000}",
            ScenarioId: $"TOKEN-SCENARIO-TEST-{index:000}",
            SourceScenarioId: sourceScenarioId,
            Host: index <= 3 ? "Claude" : index <= 6 ? "Cursor" : "Codex",
            Method: index % 3 == 1 ? "prompt-to-task" : index % 3 == 2 ? "orchestration-plan" : "skill-selection",
            PromptSha256: Quality03NonInterRaterProofProducer.Sha256(string.IsNullOrWhiteSpace(promptText) ? $"prompt-{index}" : promptText),
            PromptText: promptText,
            BaselineOutputText: includePromptAndOutputs ? $"Baseline response {index} stays generic and omits private prompt replay." : string.Empty,
            AdaptiveOutputText: includePromptAndOutputs ? $"Adaptive response {index} stays generic and uses the approved local contract." : string.Empty);
    }

    private static string DistinctPrompt(int index)
        => index switch
        {
            1 => "Summarize keyboard focus handling for a small command palette using accessibility vocabulary.",
            2 => "Plan a rollback checklist for a portable checkpoint bundle with manifest verification.",
            3 => "Classify documentation routing evidence for a project wiki answer without private text.",
            4 => "Explain safe debug logging for an extension adapter that forwards a request to core.",
            5 => "Compare token accounting records for a baseline trace and an adaptive trace pair.",
            6 => "Review a skill selection artifact for deterministic shortcut leakage and source provenance.",
            7 => "Describe a no-overclaim report that keeps runtime readiness separate from static checks.",
            8 => "Outline a source-disjoint held-out split for local quality evaluation rows.",
            9 => "Inspect a replay probe summary for memorization risk using fragment length limits.",
            _ => $"Unique quality prompt {index}"
        };

    private static void WriteJsonl(string path, IEnumerable<JsonObject> rows)
        => File.WriteAllLines(path, rows.Select(row => row.ToJsonString(SerializerOptions())), Encoding.UTF8);

    private static void WriteJson(string path, JsonObject root)
        => File.WriteAllText(path, root.ToJsonString(SerializerOptions()), Encoding.UTF8);

    private static IReadOnlyList<JsonDocument> ReadJsonl(string path)
        => File.ReadLines(path, Encoding.UTF8)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonDocument.Parse(line))
            .ToArray();

    private static string Sha256File(string path)
    {
        using var stream = File.OpenRead(path);
        var bytes = SHA256.HashData(stream);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static JsonSerializerOptions SerializerOptions()
        => new()
        {
            WriteIndented = false,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

    private static TempDirectory TempWorkspace()
        => new();

    private sealed record FixturePaths(
        string EvidencePackDirectory,
        string SourceProofContractPackDirectory,
        string OutputDirectory,
        string RunnerOutputDirectory,
        IReadOnlyList<RowFixture> Rows);

    private sealed record RowFixture(
        string RowId,
        string PairSequenceId,
        string PairId,
        string ScenarioId,
        string SourceScenarioId,
        string Host,
        string Method,
        string PromptSha256,
        string PromptText,
        string BaselineOutputText,
        string AdaptiveOutputText);

    private sealed record CategoryFixture(string CategoryId, string RequiredProofRef);

    private sealed class TempDirectory : IDisposable
    {
        private readonly string _path = Path.Combine(Path.GetTempPath(), $"quality03-non-inter-rater-tests-{Guid.NewGuid():N}");

        public TempDirectory()
        {
            Directory.CreateDirectory(_path);
        }

        public static implicit operator string(TempDirectory directory)
            => directory._path;

        public void Dispose()
        {
            if (Directory.Exists(_path))
                Directory.Delete(_path, recursive: true);
        }
    }
}
