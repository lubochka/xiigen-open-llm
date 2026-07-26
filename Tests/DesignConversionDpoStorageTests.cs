// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text.Json.Nodes;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using DesignConversionDomain;
using ModelServer.DesignConversion;

public sealed class DesignConversionDpoStorageTests
{
    [Fact]
    public async Task LocalArtifactDb_RoundTripsConversionRenderJudgeAndTrainingCandidateWithoutPrivateFields()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local_artifact_db");
        var db = new JsonlDesignConversionLocalArtifactDb(root);
        const string rawHtml = "<main data-user-id=\"user-luba\" data-path=\"C:/Users/private-user/private.html\">exact_prompt private html</main>";
        const string rawCss = ".private-user{background:url('C:/Users/private-user/private.png');}";
        var record = BuildTrainingRecord("allowed", DesignConversionDpoUnits.ImproverUnit, usefulness: 0.94) with
        {
            Chosen = DesignConversionDpoGeneratorTests.CreateOutput(
                "allowed-chosen-private",
                accepted: true,
                score: 0.91,
                htmlSuffix: "allowed") with
            {
                Html = rawHtml,
                Css = rawCss
            },
            Provenance = new DesignConversionDpoProvenance(
                SourceRequestId: "request-from-user-luba-exact_prompt",
                SourceKind: DesignConversionSourceKind.Figma.ToString(),
                SourceManifestRef: "file:///C:/Users/private-user/Documents/private.fig",
                ReviewerRef: "reviewer:user-luba",
                CreatedAtUtc: DateTimeOffset.UnixEpoch,
                Metadata: new Dictionary<string, string>
                {
                    ["source_design_id"] = "C:/Users/private-user/Documents/private.fig",
                    ["raw_prompt"] = "exact_prompt private html"
                })
        };
        var artifactRecord = BuildArtifactRecord(
            "storage-record-allowed",
            "job-allowed",
            "request-allowed",
            DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            [record],
            rawHtml,
            rawCss);

        var append = await db.AppendRecordAsync(artifactRecord);
        var reloaded = new JsonlDesignConversionLocalArtifactDb(root);
        var listed = await reloaded.ListRecordsAsync();
        var approved = await reloaded.QueryApprovedTrainingCandidatesAsync();
        var display = await reloaded.QueryDisplayRowsAsync(new DesignConversionDpoDisplayQuery(IncludeBlocked: false));
        var datasetOptions = new DesignConversionDpoDatasetOptions(
            DatasetId: "local-artifact-db-training-read",
            MinimumTrainRecords: 1,
            MinimumHeldOutRecords: 1,
            Preflight: DesignConversionDpoDatasetPreflight.LocalFixtureTrainingCandidateOnly());
        var trainingBatches = new DesignConversionTrainingPipeline().BuildBatches(
            approved.Value!,
            new DesignConversionTrainingPipelineOptions(
                "local-artifact-db-training-read",
                BatchSize: 2,
                RequireDatasetCountGate: false,
                DatasetOptions: datasetOptions));
        var dataset = new DesignConversionDpoDatasetBuilder().BuildDataset(approved.Value!, datasetOptions);
        var persistedText = File.ReadAllText(Path.Combine(root, "conversion-results.jsonl"));
        var indexText = File.ReadAllText(Path.Combine(root, "conversion-results.index.json"));

        Assert.True(append.IsSuccess, append.ErrorMessage);
        Assert.True(File.Exists(Path.Combine(root, "conversion-results.jsonl")));
        Assert.True(File.Exists(Path.Combine(root, "conversion-results.index.json")));
        var stored = Assert.Single(listed.Value!);
        Assert.Equal(JsonlDesignConversionLocalArtifactDb.LocalArtifactDbKind, stored.DatabaseKind);
        Assert.Equal("job-allowed", stored.JobId);
        Assert.Equal("request-allowed", stored.RequestId);
        Assert.Equal("figma", stored.SourceKind, ignoreCase: true);
        Assert.Equal("pro", stored.Tier);
        Assert.StartsWith("dcm-html_result-", stored.ConversionResult.HtmlRef, StringComparison.Ordinal);
        Assert.StartsWith("dcm-css_result-", stored.ConversionResult.CssRef, StringComparison.Ordinal);
        Assert.StartsWith("sha256:", stored.ConversionResult.HtmlSha256, StringComparison.Ordinal);
        Assert.StartsWith("sha256:", stored.ConversionResult.CssSha256, StringComparison.Ordinal);
        var render = Assert.Single(stored.RenderArtifactRefs);
        Assert.Equal("desktop", render.ViewportId);
        Assert.Equal("sha256:render-desktop", render.Sha256);
        Assert.StartsWith("dcm-artifact-ref-", render.UriRef, StringComparison.Ordinal);
        Assert.Equal(1440, render.Width);
        Assert.Equal(900, render.Height);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker, stored.VisualJudgeStatus.Status);
        Assert.Equal("cost_cap", stored.VisualJudgeStatus.TierDecision);
        var candidate = Assert.Single(stored.DpoCandidates);
        Assert.True(candidate.TrainingCandidateOnly);
        Assert.True(candidate.ProductNotReady);
        Assert.Equal(0.94, candidate.UsefulnessScore);
        Assert.Empty(candidate.BlockReasons);

        var trainingRecord = Assert.Single(approved.Value!);
        Assert.Equal(candidate.RecordId, trainingRecord.RecordId);
        Assert.StartsWith("dcm-chosen_html-", trainingRecord.Chosen.Html, StringComparison.Ordinal);
        Assert.StartsWith("dcm-chosen_css-", trainingRecord.Chosen.Css, StringComparison.Ordinal);
        Assert.DoesNotContain("<main", trainingRecord.Chosen.Html, StringComparison.Ordinal);
        Assert.True(trainingBatches.IsSuccess, trainingBatches.ErrorMessage);
        var batch = Assert.Single(trainingBatches.Value!);
        var trainingItem = Assert.Single(batch.TrainingItems);
        Assert.Equal(trainingRecord.RecordId, trainingItem.ItemId);
        Assert.Contains("dcm-chosen_html-", trainingItem.ChosenResponse, StringComparison.Ordinal);
        Assert.True(dataset.IsSuccess, dataset.ErrorMessage);
        var datasetRow = Assert.Single(dataset.Value!.Rows);
        var promptJson = JsonNode.Parse(datasetRow.Prompt)!.AsObject();
        var resultFeatures = promptJson["result_features"]!.AsObject();
        Assert.Equal("result_features", resultFeatures["namespace"]!.GetValue<string>());
        Assert.False(resultFeatures["source_authority"]!.GetValue<bool>());
        Assert.Equal("outcome_label_aggregate_not_source_authority", resultFeatures["role"]!.GetValue<string>());
        Assert.NotNull(resultFeatures["visual_score_margin"]);
        Assert.Equal("result_outcome_label_aggregate_not_source_encoder", datasetRow.Metadata["result_features_scope"]);
        Assert.Equal(bool.FalseString, datasetRow.Metadata["source_encoder_result_features_in_vector"]);
        Assert.Equal(bool.FalseString, datasetRow.Metadata["source_encoder_visual_score_margin_in_vector"]);
        Assert.Equal(bool.FalseString, datasetRow.Metadata["source_encoder_usefulness_score_in_vector"]);
        Assert.Equal(bool.FalseString, datasetRow.Metadata["source_encoder_answer_payload_in_vector"]);
        Assert.DoesNotContain("exact_prompt", datasetRow.Prompt, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/Users/", datasetRow.Prompt, StringComparison.OrdinalIgnoreCase);
        Assert.True(display.IsSuccess, display.ErrorMessage);
        var row = Assert.Single(display.Value!.Rows);
        Assert.Equal("high_usefulness", row.UsefulnessLabel);
        Assert.True(row.TrainingCandidateOnly);
        Assert.True(row.ProductNotReady);

        Assert.Contains(JsonlDesignConversionLocalArtifactDb.StorageTrainingArtifactDbMarker, persistedText);
        Assert.Contains(JsonlDesignConversionLocalArtifactDb.LocalArtifactDbKind, persistedText);
        Assert.Contains("conversion_result", persistedText);
        Assert.Contains("render_artifact_refs", persistedText);
        Assert.Contains("visual_judge_status", persistedText);
        Assert.Contains("dpo_candidate", persistedText);
        Assert.Contains("usefulness_score", persistedText);
        Assert.Contains("training_candidate_only", persistedText);
        Assert.Contains("training_grade", persistedText);
        Assert.Contains("not_training_grade", persistedText);
        Assert.Contains("product_not_ready", persistedText);
        Assert.Contains("production_db_ready", indexText);
        Assert.DoesNotContain(rawHtml, persistedText, StringComparison.Ordinal);
        Assert.DoesNotContain(rawCss, persistedText, StringComparison.Ordinal);
        Assert.DoesNotContain("C:/Users/", persistedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("user-luba", persistedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("exact_prompt", persistedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("private.fig", persistedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("raw_prompt", persistedText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LocalArtifactDb_AppendsRecordsAndWritesSchemaVersionIndex()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local_artifact_db_append_schema");
        var db = new JsonlDesignConversionLocalArtifactDb(root);
        const string firstRawHtml = "<main data-private=\"append-private-a\">append exact_prompt A</main>";
        const string secondRawHtml = "<main data-private=\"append-private-b\">append exact_prompt B</main>";

        var firstAppend = await db.AppendRecordAsync(BuildArtifactRecord(
            "storage-record-append-a",
            "job-append-a",
            "request-append-a",
            DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            [BuildTrainingRecord("append-a", DesignConversionDpoUnits.ImproverUnit, usefulness: 0.91)],
            firstRawHtml,
            ".append-private-a{background:url('C:/Users/private-user/private-a.png');}"));
        var secondAppend = await db.AppendRecordAsync(BuildArtifactRecord(
            "storage-record-append-b",
            "job-append-b",
            "request-append-b",
            DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            [BuildTrainingRecord("append-b", DesignConversionDpoUnits.CriticUnit, usefulness: 0.92)],
            secondRawHtml,
            ".append-private-b{background:url('C:/Users/private-user/private-b.png');}"));

        var reloaded = new JsonlDesignConversionLocalArtifactDb(root);
        var listed = await reloaded.ListRecordsAsync();
        var persistedLines = await File.ReadAllLinesAsync(Path.Combine(root, "conversion-results.jsonl"));
        var indexText = await File.ReadAllTextAsync(Path.Combine(root, "conversion-results.index.json"));
        var index = System.Text.Json.JsonSerializer.Deserialize<DesignConversionLocalArtifactDbIndex>(
            indexText,
            DesignConversionCheckpointJsonOptions.Standard);
        var persistedText = string.Join(Environment.NewLine, persistedLines);

        Assert.True(firstAppend.IsSuccess, firstAppend.ErrorMessage);
        Assert.True(secondAppend.IsSuccess, secondAppend.ErrorMessage);
        Assert.True(listed.IsSuccess, listed.ErrorMessage);
        Assert.Equal(2, listed.Value!.Count);
        Assert.Equal(2, persistedLines.Count(line => !string.IsNullOrWhiteSpace(line)));
        Assert.NotNull(index);
        Assert.Equal("design-conversion-local-artifact-db-v1", index!.SchemaVersion);
        Assert.Equal(2, index.RecordCount);
        Assert.Equal(JsonlDesignConversionLocalArtifactDb.LocalArtifactDbKind, index.DatabaseKind);
        Assert.Contains(JsonlDesignConversionLocalArtifactDb.StorageTrainingArtifactDbMarker, index.EvidenceMarkers);
        Assert.Contains(JsonlDesignConversionLocalArtifactDb.ConversionResultMarker, index.EvidenceMarkers);
        Assert.Contains(JsonlDesignConversionLocalArtifactDb.DpoCandidateMarker, index.EvidenceMarkers);
        Assert.Contains(listed.Value, record => record.JobId == "job-append-a" && record.DpoCandidates.Count == 1);
        Assert.Contains(listed.Value, record => record.JobId == "job-append-b" && record.DpoCandidates.Count == 1);
        Assert.All(listed.Value, record =>
        {
            Assert.StartsWith("dcm-html_result-", record.ConversionResult.HtmlRef, StringComparison.Ordinal);
            Assert.StartsWith("sha256:", record.ConversionResult.HtmlSha256, StringComparison.Ordinal);
            Assert.True(record.DpoCandidates.Single().TrainingCandidateOnly);
            Assert.True(record.DpoCandidates.Single().ProductNotReady);
            Assert.StartsWith("dcm-chosen_html-", record.DpoCandidates.Single().TrainingRecord.Chosen.Html, StringComparison.Ordinal);
            Assert.StartsWith("dcm-rejected_html-", record.DpoCandidates.Single().TrainingRecord.Rejected.Html, StringComparison.Ordinal);
        });
        Assert.DoesNotContain(firstRawHtml, persistedText, StringComparison.Ordinal);
        Assert.DoesNotContain(secondRawHtml, persistedText, StringComparison.Ordinal);
        Assert.DoesNotContain("append-private-a", persistedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("append-private-b", persistedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/Users/", persistedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("exact_prompt", persistedText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LocalArtifactDb_SteadyStateAppendUsesCachedIndexWithoutRereadingRecordsAcrossInstances()
    {
        if (!OperatingSystem.IsWindows())
            return;

        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local_artifact_db_append_cached_index");
        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            await Save(db, BuildArtifactRecord(
                "storage-record-cached-index-a",
                "job-cached-index-a",
                "request-cached-index-a",
                DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                [BuildTrainingRecord("cached-index-a", DesignConversionDpoUnits.ImproverUnit, usefulness: 0.91)],
                "<main>cached index A</main>",
                ".cached-index-a{}"));

            var recordsPath = Path.Combine(root, "conversion-results.jsonl");
            var indexPath = Path.Combine(root, "conversion-results.index.json");
            await using (var readHandle = new FileStream(
                recordsPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Write,
                bufferSize: 4096,
                FileOptions.Asynchronous))
            {
                Assert.Throws<IOException>(() =>
                {
                    using var deniedReader = new FileStream(
                        recordsPath,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.Read);
                });

                var appendDb = new JsonlDesignConversionLocalArtifactDb(root);
                var append = await appendDb.AppendRecordAsync(BuildArtifactRecord(
                    "storage-record-cached-index-b",
                    "job-cached-index-b",
                    "request-cached-index-b",
                    DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                    [BuildTrainingRecord("cached-index-b", DesignConversionDpoUnits.CriticUnit, usefulness: 0.92)],
                    "<main>cached index B</main>",
                    ".cached-index-b{}"));

                Assert.True(append.IsSuccess, append.ErrorMessage);
            }

            var reloaded = new JsonlDesignConversionLocalArtifactDb(root);
            var listed = await reloaded.ListRecordsAsync();
            var indexText = await File.ReadAllTextAsync(indexPath);
            var index = System.Text.Json.JsonSerializer.Deserialize<DesignConversionLocalArtifactDbIndex>(
                indexText,
                DesignConversionCheckpointJsonOptions.Standard);

            Assert.True(listed.IsSuccess, listed.ErrorMessage);
            Assert.Equal(2, listed.Value!.Count);
            Assert.Contains(listed.Value, record => record.RecordId == "storage-record-cached-index-a");
            Assert.Contains(listed.Value, record => record.RecordId == "storage-record-cached-index-b");
            Assert.NotNull(index);
            Assert.Equal(2, index!.RecordCount);
            Assert.Equal(2, index.ApprovedTrainingCandidateCount);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LocalArtifactDb_AppendInvalidatesCachedIndexAndLogicallyQuarantinesExternallyMalformedJsonl()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local_artifact_db_append_cache_invalidation");
        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            await Save(db, BuildArtifactRecord(
                "storage-record-cache-invalidation-a",
                "job-cache-invalidation-a",
                "request-cache-invalidation-a",
                DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                [BuildTrainingRecord("cache-invalidation-a", DesignConversionDpoUnits.ImproverUnit, usefulness: 0.91)],
                "<main>cache invalidation A</main>",
                ".cache-invalidation-a{}"));

            var recordsPath = Path.Combine(root, "conversion-results.jsonl");
            await File.AppendAllTextAsync(recordsPath, "{malformed-json" + Environment.NewLine);
            var beforeBytes = await File.ReadAllBytesAsync(recordsPath);

            var append = await db.AppendRecordAsync(BuildArtifactRecord(
                "storage-record-cache-invalidation-b",
                "job-cache-invalidation-b",
                "request-cache-invalidation-b",
                DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                [BuildTrainingRecord("cache-invalidation-b", DesignConversionDpoUnits.CriticUnit, usefulness: 0.92)],
                "<main>cache invalidation B</main>",
                ".cache-invalidation-b{}"));
            var afterBytes = await File.ReadAllBytesAsync(recordsPath);
            var listed = await new JsonlDesignConversionLocalArtifactDb(root).ListRecordsAsync();

            Assert.True(append.IsSuccess, append.ErrorMessage);
            Assert.Equal(beforeBytes, afterBytes.Take(beforeBytes.Length).ToArray());
            Assert.True(listed.IsSuccess, listed.ErrorMessage);
            Assert.Equal(2, listed.Value!.Count);
            Assert.Contains(listed.Value, record => record.RecordId == "storage-record-cache-invalidation-a");
            Assert.Contains(listed.Value, record => record.RecordId == "storage-record-cache-invalidation-b");
            Assert.All(listed.Value, record =>
            {
                Assert.Contains(
                    JsonlDesignConversionLocalArtifactDb.LogicalQuarantinedAuthorityLineMarker,
                    record.EvidenceRefs);
                Assert.Contains(
                    JsonlDesignConversionLocalArtifactDb.QuarantinedAuthorityBytesPreservedMarker,
                    record.EvidenceRefs);
                Assert.Equal("1", record.Metadata["local_artifact_db_quarantined_authority_line_count"]);
            });
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LocalArtifactDb_NormalMultiRecordRoundtripSurvivesFreshReconstruction()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local_artifact_db_normal_roundtrip_fresh");
        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            await Save(db, BuildArtifactRecord(
                "storage-record-normal-a",
                "job-normal-a",
                "request-normal-a",
                DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                [BuildTrainingRecord("normal-a", DesignConversionDpoUnits.ImproverUnit, usefulness: 0.91)],
                "<main>normal roundtrip A</main>",
                ".normal-a{}"));
            await Save(db, BuildArtifactRecord(
                "storage-record-normal-b",
                "job-normal-b",
                "request-normal-b",
                DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                [BuildTrainingRecord("normal-b", DesignConversionDpoUnits.CriticUnit, usefulness: 0.92)],
                "<main>normal roundtrip B</main>",
                ".normal-b{}"));

            var recordsPath = Path.Combine(root, "conversion-results.jsonl");
            var indexPath = Path.Combine(root, "conversion-results.index.json");
            var lines = await File.ReadAllLinesAsync(recordsPath);
            var reloaded = new JsonlDesignConversionLocalArtifactDb(root);
            var listed = await reloaded.ListRecordsAsync();
            var index = System.Text.Json.JsonSerializer.Deserialize<DesignConversionLocalArtifactDbIndex>(
                await File.ReadAllTextAsync(indexPath),
                DesignConversionCheckpointJsonOptions.Standard);

            Assert.True(listed.IsSuccess, listed.ErrorMessage);
            Assert.Equal(2, lines.Count(line => !string.IsNullOrWhiteSpace(line)));
            Assert.Equal(
                ["storage-record-normal-a", "storage-record-normal-b"],
                listed.Value!.Select(record => record.RecordId).ToArray());
            Assert.NotNull(index);
            Assert.Equal(2, index!.RecordCount);
            Assert.Equal(2, index.RecordLocators.Count);
            Assert.DoesNotContain(JsonlDesignConversionLocalArtifactDb.UncommittedFinalRecordRecoveredMarker, index.EvidenceMarkers);
            Assert.DoesNotContain(JsonlDesignConversionLocalArtifactDb.UncommittedFinalRecordNotCommittedMarker, index.EvidenceMarkers);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LocalArtifactDb_TruncatedFinalTailIsRecoveredAndNextAppendDropsUncommittedTail()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local_artifact_db_truncated_tail_recovery");
        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            await Save(db, BuildArtifactRecord(
                "storage-record-tail-a",
                "job-tail-a",
                "request-tail-a",
                DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                [BuildTrainingRecord("tail-a", DesignConversionDpoUnits.ImproverUnit, usefulness: 0.91)],
                "<main>tail A</main>",
                ".tail-a{}"));
            await Save(db, BuildArtifactRecord(
                "storage-record-tail-b",
                "job-tail-b",
                "request-tail-b",
                DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                [BuildTrainingRecord("tail-b", DesignConversionDpoUnits.CriticUnit, usefulness: 0.92)],
                "<main>tail B</main>",
                ".tail-b{}"));

            var recordsPath = Path.Combine(root, "conversion-results.jsonl");
            var indexPath = Path.Combine(root, "conversion-results.index.json");
            var committedLines = await File.ReadAllLinesAsync(recordsPath);
            var uncommittedLine = committedLines[0]
                .Replace("storage-record-tail-a", "storage-record-tail-uncommitted", StringComparison.Ordinal)
                .Replace("job-tail-a", "job-tail-uncommitted", StringComparison.Ordinal)
                .Replace("request-tail-a", "request-tail-uncommitted", StringComparison.Ordinal);
            await File.AppendAllTextAsync(
                recordsPath,
                uncommittedLine,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            var recovered = await new JsonlDesignConversionLocalArtifactDb(root).ListRecordsAsync();

            Assert.True(recovered.IsSuccess, recovered.ErrorMessage);
            Assert.Equal(
                ["storage-record-tail-a", "storage-record-tail-b"],
                recovered.Value!.Select(record => record.RecordId).ToArray());
            Assert.All(recovered.Value!, record =>
            {
                Assert.Contains(JsonlDesignConversionLocalArtifactDb.UncommittedFinalRecordRecoveredMarker, record.EvidenceRefs);
                Assert.Contains(JsonlDesignConversionLocalArtifactDb.UncommittedFinalRecordNotCommittedMarker, record.EvidenceRefs);
                Assert.Equal(bool.TrueString, record.Metadata["local_artifact_db_uncommitted_final_record_recovered"]);
                Assert.Equal(bool.FalseString, record.Metadata["local_artifact_db_uncommitted_final_record_committed"]);
                Assert.Equal("3", record.Metadata["local_artifact_db_uncommitted_final_record_line"]);
            });

            var recoveredIndex = System.Text.Json.JsonSerializer.Deserialize<DesignConversionLocalArtifactDbIndex>(
                await File.ReadAllTextAsync(indexPath),
                DesignConversionCheckpointJsonOptions.Standard);
            Assert.NotNull(recoveredIndex);
            Assert.Contains(
                JsonlDesignConversionLocalArtifactDb.UncommittedFinalRecordRecoveredMarker,
                recoveredIndex!.EvidenceMarkers);
            Assert.Contains(
                JsonlDesignConversionLocalArtifactDb.UncommittedFinalRecordNotCommittedMarker,
                recoveredIndex.EvidenceMarkers);

            var appendAfterTail = await new JsonlDesignConversionLocalArtifactDb(root).AppendRecordAsync(BuildArtifactRecord(
                "storage-record-tail-c",
                "job-tail-c",
                "request-tail-c",
                DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                [BuildTrainingRecord("tail-c", DesignConversionDpoUnits.CriticUnit, usefulness: 0.94)],
                "<main>tail C</main>",
                ".tail-c{}"));
            var finalText = await File.ReadAllTextAsync(recordsPath);
            var finalRecords = await new JsonlDesignConversionLocalArtifactDb(root).ListRecordsAsync();

            Assert.True(appendAfterTail.IsSuccess, appendAfterTail.ErrorMessage);
            Assert.DoesNotContain("storage-record-tail-uncommitted", finalText, StringComparison.Ordinal);
            Assert.True(finalRecords.IsSuccess, finalRecords.ErrorMessage);
            Assert.Equal(
                ["storage-record-tail-a", "storage-record-tail-b", "storage-record-tail-c"],
                finalRecords.Value!.Select(record => record.RecordId).ToArray());
            Assert.All(finalRecords.Value!, record =>
                Assert.DoesNotContain(JsonlDesignConversionLocalArtifactDb.UncommittedFinalRecordRecoveredMarker, record.EvidenceRefs));
            var finalIndex = System.Text.Json.JsonSerializer.Deserialize<DesignConversionLocalArtifactDbIndex>(
                await File.ReadAllTextAsync(indexPath),
                DesignConversionCheckpointJsonOptions.Standard);
            Assert.NotNull(finalIndex);
            Assert.DoesNotContain(JsonlDesignConversionLocalArtifactDb.UncommittedFinalRecordRecoveredMarker, finalIndex!.EvidenceMarkers);
            Assert.DoesNotContain(JsonlDesignConversionLocalArtifactDb.UncommittedFinalRecordNotCommittedMarker, finalIndex.EvidenceMarkers);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LocalArtifactDb_TailOnlyFileReturnsEmptyAndIndexesRecoveryEvidence()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local_artifact_db_tail_only_recovery");
        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            await Save(db, BuildArtifactRecord(
                "storage-record-tail-only",
                "job-tail-only",
                "request-tail-only",
                DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                [BuildTrainingRecord("tail-only", DesignConversionDpoUnits.ImproverUnit, usefulness: 0.91)],
                "<main>tail only</main>",
                ".tail-only{}"));

            var recordsPath = Path.Combine(root, "conversion-results.jsonl");
            var indexPath = Path.Combine(root, "conversion-results.index.json");
            var persistedLines = await File.ReadAllLinesAsync(recordsPath);
            var line = Assert.Single(persistedLines, line => !string.IsNullOrWhiteSpace(line));
            await File.WriteAllTextAsync(
                recordsPath,
                line,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            var listed = await new JsonlDesignConversionLocalArtifactDb(root).ListRecordsAsync();
            var healedIndex = System.Text.Json.JsonSerializer.Deserialize<DesignConversionLocalArtifactDbIndex>(
                await File.ReadAllTextAsync(indexPath),
                DesignConversionCheckpointJsonOptions.Standard);
            var reconstructed = await new JsonlDesignConversionLocalArtifactDb(root).ListRecordsAsync();

            Assert.True(listed.IsSuccess, listed.ErrorMessage);
            Assert.Empty(listed.Value!);
            Assert.NotNull(healedIndex);
            Assert.Equal(0, healedIndex!.RecordCount);
            Assert.Empty(healedIndex.RecordLocators);
            Assert.Contains(
                JsonlDesignConversionLocalArtifactDb.UncommittedFinalRecordRecoveredMarker,
                healedIndex.EvidenceMarkers);
            Assert.Contains(
                JsonlDesignConversionLocalArtifactDb.UncommittedFinalRecordNotCommittedMarker,
                healedIndex.EvidenceMarkers);
            Assert.True(reconstructed.IsSuccess, reconstructed.ErrorMessage);
            Assert.Empty(reconstructed.Value!);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LocalArtifactDb_MalformedCompleteMiddleRecordIsLogicallyQuarantinedWithoutDroppingLaterRecord()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local_artifact_db_malformed_middle");
        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            await Save(db, BuildArtifactRecord(
                "storage-record-malformed-a",
                "job-malformed-a",
                "request-malformed-a",
                DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                [BuildTrainingRecord("malformed-a", DesignConversionDpoUnits.ImproverUnit, usefulness: 0.91)],
                "<main>malformed A</main>",
                ".malformed-a{}"));

            var recordsPath = Path.Combine(root, "conversion-results.jsonl");
            var later = BuildArtifactRecord(
                "storage-record-malformed-later",
                "job-malformed-later",
                "request-malformed-later",
                DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                [BuildTrainingRecord("malformed-later", DesignConversionDpoUnits.CriticUnit, usefulness: 0.92)],
                "<main>malformed later</main>",
                ".malformed-later{}");
            await File.AppendAllTextAsync(
                recordsPath,
                "{malformed-json" + Environment.NewLine
                                  + System.Text.Json.JsonSerializer.Serialize(
                                      later,
                                      new System.Text.Json.JsonSerializerOptions(
                                          DesignConversionCheckpointJsonOptions.Standard)
                                      {
                                          WriteIndented = false
                                      })
                                  + Environment.NewLine,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            var textAfterPoison = await File.ReadAllTextAsync(recordsPath);
            var listed = await new JsonlDesignConversionLocalArtifactDb(root).ListRecordsAsync();
            var laterQuery = await new JsonlDesignConversionLocalArtifactDb(root).ListRecordsAsync(
                new DesignConversionLocalArtifactDbQuery(JobId: "job-malformed-later"));

            Assert.Contains("storage-record-malformed-later", textAfterPoison, StringComparison.Ordinal);
            Assert.True(listed.IsSuccess, listed.ErrorMessage);
            Assert.Equal(2, listed.Value!.Count);
            Assert.Contains(listed.Value, record => record.RecordId == "storage-record-malformed-a");
            Assert.Contains(listed.Value, record => record.RecordId == "storage-record-malformed-later");
            Assert.All(listed.Value, record =>
            {
                Assert.Contains(
                    JsonlDesignConversionLocalArtifactDb.LogicalQuarantinedAuthorityLineMarker,
                    record.EvidenceRefs);
                Assert.Contains(
                    JsonlDesignConversionLocalArtifactDb.QuarantinedAuthorityBytesPreservedMarker,
                    record.EvidenceRefs);
                Assert.Equal("1", record.Metadata["local_artifact_db_quarantined_authority_line_count"]);
                Assert.Contains(
                    "line=2",
                    record.Metadata["local_artifact_db_quarantined_authority_lines"],
                    StringComparison.Ordinal);
            });
            Assert.True(laterQuery.IsSuccess, laterQuery.ErrorMessage);
            Assert.Equal("storage-record-malformed-later", Assert.Single(laterQuery.Value!).RecordId);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LocalArtifactDb_AppendFailureDoesNotAdvanceIndexOrExposePhantomRecord()
    {
        if (!OperatingSystem.IsWindows())
            return;

        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local_artifact_db_append_failure_no_phantom");
        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            await Save(db, BuildArtifactRecord(
                "storage-record-append-failure-a",
                "job-append-failure-a",
                "request-append-failure-a",
                DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                [BuildTrainingRecord("append-failure-a", DesignConversionDpoUnits.ImproverUnit, usefulness: 0.91)],
                "<main>append failure A</main>",
                ".append-failure-a{}"));

            var recordsPath = Path.Combine(root, "conversion-results.jsonl");
            var indexPath = Path.Combine(root, "conversion-results.index.json");
            var beforeIndexText = await File.ReadAllTextAsync(indexPath);
            var beforeBytes = await File.ReadAllBytesAsync(recordsPath);

            await using (var lockedRecords = new FileStream(
                recordsPath,
                FileMode.Open,
                FileAccess.ReadWrite,
                FileShare.None,
                bufferSize: 4096,
                FileOptions.Asynchronous))
            {
                var append = await new JsonlDesignConversionLocalArtifactDb(root).AppendRecordAsync(BuildArtifactRecord(
                    "storage-record-append-failure-phantom",
                    "job-append-failure-phantom",
                    "request-append-failure-phantom",
                    DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                    [BuildTrainingRecord("append-failure-phantom", DesignConversionDpoUnits.CriticUnit, usefulness: 0.92)],
                    "<main>append failure phantom</main>",
                    ".append-failure-phantom{}"));

                Assert.False(append.IsSuccess);
                Assert.Equal(JsonlDesignConversionLocalArtifactDb.LocalArtifactDbWriteFailedErrorCode, append.ErrorCode);
                Assert.Equal(beforeIndexText, await File.ReadAllTextAsync(indexPath));
            }

            var afterBytes = await File.ReadAllBytesAsync(recordsPath);
            var reloaded = await new JsonlDesignConversionLocalArtifactDb(root).ListRecordsAsync();
            var index = System.Text.Json.JsonSerializer.Deserialize<DesignConversionLocalArtifactDbIndex>(
                await File.ReadAllTextAsync(indexPath),
                DesignConversionCheckpointJsonOptions.Standard);

            Assert.Equal(beforeBytes, afterBytes);
            Assert.True(reloaded.IsSuccess, reloaded.ErrorMessage);
            var reloadedRecords = reloaded.Value!;
            Assert.Equal(["storage-record-append-failure-a"], reloadedRecords.Select(record => record.RecordId).ToArray());
            Assert.DoesNotContain(reloadedRecords, record => record.RecordId == "storage-record-append-failure-phantom");
            Assert.NotNull(index);
            Assert.Equal(1, index!.RecordCount);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LocalArtifactDb_ApprovalRewriteWritesJsonlLinesAndPreservesRecords()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local_artifact_db_approval_streaming_rewrite");
        var db = new JsonlDesignConversionLocalArtifactDb(root);
        var firstRuntime = ToRuntimePendingCandidate(
            BuildTrainingRecord("runtime-streaming-rewrite-a", DesignConversionDpoUnits.ImproverUnit, usefulness: 0.94));
        var secondRuntime = ToRuntimePendingCandidate(
            BuildTrainingRecord("runtime-streaming-rewrite-b", DesignConversionDpoUnits.CriticUnit, usefulness: 0.93));

        var firstArtifactRecord = BuildArtifactRecord(
            "storage-record-streaming-rewrite-a",
            "job-streaming-rewrite-a",
            "request-streaming-rewrite-a",
            DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            [firstRuntime],
            "<main>streaming rewrite A</main>",
            ".streaming-rewrite-a{}");
        await Save(db, firstArtifactRecord);
        await Save(db, BuildArtifactRecord(
            "storage-record-streaming-rewrite-b",
            "job-streaming-rewrite-b",
            "request-streaming-rewrite-b",
            DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            [secondRuntime],
            "<main>streaming rewrite B</main>",
            ".streaming-rewrite-b{}"));

        var approval = await db.ApproveRuntimeCreatedDpoCandidateAsync(
            "storage-record-streaming-rewrite-a",
            firstRuntime.RecordId,
            "reviewer:streaming-rewrite",
            "streaming rewrite approval evidence",
            DateTimeOffset.UnixEpoch.AddMinutes(114));
        var replay = await db.AppendRecordAsync(firstArtifactRecord);
        var recordsPath = Path.Combine(root, "conversion-results.jsonl");
        var indexPath = Path.Combine(root, "conversion-results.index.json");
        var persistedText = await File.ReadAllTextAsync(recordsPath);
        var nonblankLines = (await File.ReadAllLinesAsync(recordsPath))
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToArray();
        var parsedRecords = nonblankLines
            .Select(line =>
            {
                Assert.NotNull(JsonNode.Parse(line));
                var record = System.Text.Json.JsonSerializer.Deserialize<DesignConversionLocalArtifactDbRecord>(
                    line,
                    DesignConversionCheckpointJsonOptions.Standard);
                Assert.NotNull(record);
                return record!;
            })
            .ToArray();
        var reloaded = new JsonlDesignConversionLocalArtifactDb(root);
        var listed = await reloaded.ListRecordsAsync();
        var approved = await reloaded.QueryApprovedTrainingCandidatesAsync();
        var indexText = await File.ReadAllTextAsync(indexPath);
        var index = System.Text.Json.JsonSerializer.Deserialize<DesignConversionLocalArtifactDbIndex>(
            indexText,
            DesignConversionCheckpointJsonOptions.Standard);

        Assert.True(approval.IsSuccess, approval.ErrorMessage);
        Assert.True(replay.IsSuccess, replay.ErrorMessage);
        var replayedCandidate = Assert.Single(replay.Value!.DpoCandidates);
        Assert.Equal("approved", replayedCandidate.TrainingRecord.Provenance.Metadata["approval_status"]);
        Assert.Equal(bool.TrueString, replayedCandidate.TrainingRecord.Provenance.Metadata["approved_for_training"]);
        Assert.EndsWith(Environment.NewLine, persistedText, StringComparison.Ordinal);
        Assert.Equal(2, nonblankLines.Length);
        Assert.Equal(2, parsedRecords.Select(record => record.RecordId).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(
            ["storage-record-streaming-rewrite-a", "storage-record-streaming-rewrite-b"],
            parsedRecords.Select(record => record.RecordId).ToArray());
        Assert.True(listed.IsSuccess, listed.ErrorMessage);
        Assert.Equal(
            ["storage-record-streaming-rewrite-a", "storage-record-streaming-rewrite-b"],
            listed.Value!.Select(record => record.RecordId).ToArray());
        var storedFirst = Assert.Single(
            listed.Value!,
            record => string.Equals(record.RecordId, "storage-record-streaming-rewrite-a", StringComparison.Ordinal));
        var storedCandidate = Assert.Single(storedFirst.DpoCandidates);
        Assert.Equal(firstRuntime.RecordId, storedCandidate.TrainingRecord.RecordId);
        Assert.Equal("approved", storedCandidate.TrainingRecord.Provenance.Metadata["approval_status"]);
        Assert.Equal(bool.TrueString, storedCandidate.TrainingRecord.Provenance.Metadata["approved_for_training"]);
        Assert.Equal("reviewer:streaming-rewrite", storedCandidate.TrainingRecord.Provenance.Metadata["approved_by_ref"]);
        Assert.Equal("streaming rewrite approval evidence", storedCandidate.TrainingRecord.Provenance.Metadata["approval_reason"]);
        Assert.Equal(
            approval.Value!.ApprovedAtUtc.ToString("O"),
            storedCandidate.TrainingRecord.Provenance.Metadata["approved_at_utc"]);
        Assert.True(approved.IsSuccess, approved.ErrorMessage);
        var approvedCandidate = Assert.Single(approved.Value!);
        Assert.Equal(firstRuntime.RecordId, approvedCandidate.RecordId);
        Assert.NotNull(index);
        Assert.Equal(2, index!.RecordCount);
        Assert.Equal(1, index.ApprovedTrainingCandidateCount);
        Assert.Empty(EnumerateRecordsRewriteTempFiles(root));
    }

    [Fact]
    public async Task LocalArtifactDb_ApprovalRewriteFailureKeepsAuthorityAndIndexAndAllowsRetry()
    {
        if (!OperatingSystem.IsWindows())
            return;

        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "local_artifact_db_approval_rewrite_failure_retry");
        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var pendingRuntime = ToRuntimePendingCandidate(
                BuildTrainingRecord("runtime-rewrite-failure", DesignConversionDpoUnits.ImproverUnit, usefulness: 0.94));
            var stableRuntime = ToRuntimePendingCandidate(
                BuildTrainingRecord("runtime-rewrite-stable", DesignConversionDpoUnits.CriticUnit, usefulness: 0.93));

            await Save(db, BuildArtifactRecord(
                "storage-record-rewrite-failure",
                "job-rewrite-failure",
                "request-rewrite-failure",
                DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                [pendingRuntime],
                "<main>rewrite failure private payload</main>",
                ".rewrite-failure{}"));
            await Save(db, BuildArtifactRecord(
                "storage-record-rewrite-stable",
                "job-rewrite-stable",
                "request-rewrite-stable",
                DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                [stableRuntime],
                "<main>rewrite stable private payload</main>",
                ".rewrite-stable{}"));

            var recordsPath = Path.Combine(root, "conversion-results.jsonl");
            var indexPath = Path.Combine(root, "conversion-results.index.json");
            var originalAuthorityBytes = await File.ReadAllBytesAsync(recordsPath);
            var originalIndexBytes = await File.ReadAllBytesAsync(indexPath);
            var originalRecordIds = await ReadRecordIdsFromJsonlAsync(recordsPath);

            await using (var lockHandle = new FileStream(
                recordsPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 4096,
                FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                var failedApproval = await db.ApproveRuntimeCreatedDpoCandidateAsync(
                    "storage-record-rewrite-failure",
                    pendingRuntime.RecordId,
                    "reviewer:rewrite-failure",
                    "rewrite failure retry evidence",
                    DateTimeOffset.UnixEpoch.AddMinutes(214));
                var authorityBytesAfterFailure = await File.ReadAllBytesAsync(recordsPath);
                var indexBytesAfterFailure = await File.ReadAllBytesAsync(indexPath);

                Assert.False(failedApproval.IsSuccess);
                Assert.Equal(JsonlDesignConversionLocalArtifactDb.LocalArtifactDbWriteFailedErrorCode, failedApproval.ErrorCode);
                Assert.Equal(JsonlDesignConversionLocalArtifactDb.LocalArtifactDbWriteFailedSafeMessage, failedApproval.ErrorMessage);
                var failureText = $"{failedApproval.ErrorCode}|{failedApproval.ErrorMessage}";
                Assert.DoesNotContain(root, failureText, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(recordsPath, failureText, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("conversion-results.jsonl", failureText, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(".tmp", failureText, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("rewrite", failureText, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("IOException", failureText, StringComparison.Ordinal);
                Assert.DoesNotContain("UnauthorizedAccessException", failureText, StringComparison.Ordinal);
                Assert.DoesNotContain("<main", failureText, StringComparison.OrdinalIgnoreCase);
                Assert.Equal(originalAuthorityBytes, authorityBytesAfterFailure);
                Assert.Equal(originalIndexBytes, indexBytesAfterFailure);
                Assert.Empty(EnumerateRecordsRewriteTempFiles(root));

                var gateProbeTask = db.ApproveRuntimeCreatedDpoCandidateAsync(
                    "missing-record-after-rewrite-failure",
                    pendingRuntime.RecordId,
                    "reviewer:rewrite-failure-gate-probe",
                    "rewrite failure gate release probe",
                    DateTimeOffset.UnixEpoch.AddMinutes(215));
                var completed = await Task.WhenAny(gateProbeTask, Task.Delay(TimeSpan.FromSeconds(5)));
                Assert.Same(gateProbeTask, completed);
                var gateProbe = await gateProbeTask;
                Assert.False(gateProbe.IsSuccess);
                Assert.Equal("DESIGN_CONVERSION_LOCAL_ARTIFACT_RECORD_NOT_FOUND", gateProbe.ErrorCode);
            }

            var retry = await db.ApproveRuntimeCreatedDpoCandidateAsync(
                "storage-record-rewrite-failure",
                pendingRuntime.RecordId,
                "reviewer:rewrite-failure",
                "rewrite failure retry evidence",
                DateTimeOffset.UnixEpoch.AddMinutes(214));
            var reloaded = new JsonlDesignConversionLocalArtifactDb(root);
            var listed = await reloaded.ListRecordsAsync();
            var approved = await reloaded.QueryApprovedTrainingCandidatesAsync();
            var finalRecordIds = await ReadRecordIdsFromJsonlAsync(recordsPath);
            var finalIndexText = await File.ReadAllTextAsync(indexPath);
            var finalIndex = System.Text.Json.JsonSerializer.Deserialize<DesignConversionLocalArtifactDbIndex>(
                finalIndexText,
                DesignConversionCheckpointJsonOptions.Standard);

            Assert.True(retry.IsSuccess, retry.ErrorMessage);
            Assert.True(listed.IsSuccess, listed.ErrorMessage);
            Assert.True(approved.IsSuccess, approved.ErrorMessage);
            Assert.Equal(originalRecordIds, finalRecordIds);
            Assert.Equal(2, listed.Value!.Count);
            Assert.Equal(
                ["storage-record-rewrite-failure", "storage-record-rewrite-stable"],
                listed.Value.Select(record => record.RecordId).ToArray());
            var approvedRecord = Assert.Single(
                listed.Value,
                record => string.Equals(record.RecordId, "storage-record-rewrite-failure", StringComparison.Ordinal));
            var approvedCandidate = Assert.Single(approvedRecord.DpoCandidates);
            Assert.Equal("approved", approvedCandidate.TrainingRecord.Provenance.Metadata["approval_status"]);
            Assert.Equal(bool.TrueString, approvedCandidate.TrainingRecord.Provenance.Metadata["approved_for_training"]);
            Assert.Equal("reviewer:rewrite-failure", approvedCandidate.TrainingRecord.Provenance.Metadata["approved_by_ref"]);
            Assert.Equal("rewrite failure retry evidence", approvedCandidate.TrainingRecord.Provenance.Metadata["approval_reason"]);
            var approvedTrainingRecord = Assert.Single(approved.Value!);
            Assert.Equal(pendingRuntime.RecordId, approvedTrainingRecord.RecordId);
            Assert.NotNull(finalIndex);
            Assert.Equal(2, finalIndex!.RecordCount);
            Assert.Equal(1, finalIndex.ApprovedTrainingCandidateCount);
            Assert.Empty(EnumerateRecordsRewriteTempFiles(root));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LocalArtifactDb_ApprovalRewriteLockedIndexReturnsDeferredSuccessAndReplayHealsIndex()
    {
        if (!OperatingSystem.IsWindows())
            return;

        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "local_artifact_db_approval_index_deferred_heal");
        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var pendingRuntime = ToRuntimePendingCandidate(
                BuildTrainingRecord("runtime-index-deferred", DesignConversionDpoUnits.ImproverUnit, usefulness: 0.94));
            var originalArtifactRecord = BuildArtifactRecord(
                "storage-record-index-deferred",
                "job-index-deferred",
                "request-index-deferred",
                DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                [pendingRuntime],
                "<main>index deferred private payload</main>",
                ".index-deferred{}");
            await Save(db, originalArtifactRecord);

            var recordsPath = Path.Combine(root, "conversion-results.jsonl");
            var indexPath = Path.Combine(root, "conversion-results.index.json");
            var originalIndexBytes = await File.ReadAllBytesAsync(indexPath);
            var originalIndexText = await File.ReadAllTextAsync(indexPath);
            var originalIndex = System.Text.Json.JsonSerializer.Deserialize<DesignConversionLocalArtifactDbIndex>(
                originalIndexText,
                DesignConversionCheckpointJsonOptions.Standard);

            await using (var lockHandle = new FileStream(
                indexPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 4096,
                FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                var approval = await db.ApproveRuntimeCreatedDpoCandidateAsync(
                    originalArtifactRecord.RecordId,
                    pendingRuntime.RecordId,
                    "reviewer:index-deferred",
                    "index deferred approval evidence",
                    DateTimeOffset.UnixEpoch.AddMinutes(216));
                var indexBytesAfterApproval = await File.ReadAllBytesAsync(indexPath);
                var authorityLines = (await File.ReadAllLinesAsync(recordsPath))
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .ToArray();
                var persistedRecord = System.Text.Json.JsonSerializer.Deserialize<DesignConversionLocalArtifactDbRecord>(
                    Assert.Single(authorityLines),
                    DesignConversionCheckpointJsonOptions.Standard);

                Assert.True(approval.IsSuccess, approval.ErrorMessage);
                Assert.NotNull(approval.Value);
                Assert.Contains(
                    JsonlDesignConversionLocalArtifactDb.AuthorityRecordCommittedMarker,
                    approval.Value!.EvidenceRefs);
                Assert.Contains(
                    JsonlDesignConversionLocalArtifactDb.DerivedIndexUpdateDeferredMarker,
                    approval.Value.EvidenceRefs);
                Assert.Contains(
                    JsonlDesignConversionLocalArtifactDb.DerivedIndexDurableFalseMarker,
                    approval.Value.EvidenceRefs);
                Assert.Equal(
                    bool.TrueString,
                    approval.Value.BoundaryMetadata["local_artifact_db_authority_record_committed"]);
                Assert.Equal(
                    "deferred",
                    approval.Value.BoundaryMetadata["local_artifact_db_derived_index_update"]);
                Assert.Equal(
                    bool.FalseString,
                    approval.Value.BoundaryMetadata["local_artifact_db_derived_index_durable"]);
                Assert.Equal(
                    bool.FalseString,
                    approval.Value.BoundaryMetadata["local_artifact_db_cross_file_atomicity_claimed"]);
                Assert.Equal(bool.FalseString, approval.Value.BoundaryMetadata["product_ready"]);
                Assert.Equal(bool.FalseString, approval.Value.BoundaryMetadata["model_readiness_claimed"]);
                var returnedEvidenceText = string.Join(
                    "|",
                    approval.Value.EvidenceRefs.Concat(
                        approval.Value.BoundaryMetadata.Select(pair => $"{pair.Key}={pair.Value}")));
                Assert.DoesNotContain(root, returnedEvidenceText, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(recordsPath, returnedEvidenceText, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(indexPath, returnedEvidenceText, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(".tmp", returnedEvidenceText, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("IOException", returnedEvidenceText, StringComparison.Ordinal);
                Assert.DoesNotContain("UnauthorizedAccessException", returnedEvidenceText, StringComparison.Ordinal);
                Assert.Equal(originalIndexBytes, indexBytesAfterApproval);
                Assert.NotNull(persistedRecord);
                Assert.Equal(originalArtifactRecord.RecordId, persistedRecord!.RecordId);
                var persistedCandidate = Assert.Single(persistedRecord.DpoCandidates);
                Assert.Equal("approved", persistedCandidate.TrainingRecord.Provenance.Metadata["approval_status"]);
                Assert.Equal(bool.TrueString, persistedCandidate.TrainingRecord.Provenance.Metadata["approved_for_training"]);
                Assert.DoesNotContain(
                    JsonlDesignConversionLocalArtifactDb.AuthorityRecordCommittedMarker,
                    persistedRecord.EvidenceRefs);
                Assert.False(persistedRecord.Metadata.ContainsKey("local_artifact_db_derived_index_update"));
                Assert.Empty(EnumerateRecordsRewriteTempFiles(root));
            }

            var replay = await db.AppendRecordAsync(originalArtifactRecord);
            var healedIndexText = await File.ReadAllTextAsync(indexPath);
            var healedIndex = System.Text.Json.JsonSerializer.Deserialize<DesignConversionLocalArtifactDbIndex>(
                healedIndexText,
                DesignConversionCheckpointJsonOptions.Standard);
            var finalRecordIds = await ReadRecordIdsFromJsonlAsync(recordsPath);

            Assert.True(replay.IsSuccess, replay.ErrorMessage);
            var replayedCandidate = Assert.Single(replay.Value!.DpoCandidates);
            Assert.Equal("approved", replayedCandidate.TrainingRecord.Provenance.Metadata["approval_status"]);
            Assert.Equal(bool.TrueString, replayedCandidate.TrainingRecord.Provenance.Metadata["approved_for_training"]);
            Assert.DoesNotContain(
                JsonlDesignConversionLocalArtifactDb.DerivedIndexUpdateDeferredMarker,
                replay.Value.EvidenceRefs);
            Assert.NotNull(originalIndex);
            Assert.NotNull(healedIndex);
            Assert.Equal(originalIndex!.RecordCount, healedIndex!.RecordCount);
            Assert.Equal(1, healedIndex.RecordCount);
            Assert.Equal(1, healedIndex.ApprovedTrainingCandidateCount);
            Assert.Equal([originalArtifactRecord.RecordId], finalRecordIds);
            Assert.Empty(EnumerateRecordsRewriteTempFiles(root));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LocalArtifactDb_SerializesParallelApprovalRewriteAndAppendsAcrossInstances()
    {
        const int Iterations = 12;

        for (var iteration = 0; iteration < Iterations; iteration++)
        {
            var root = Path.Combine(
                Path.GetTempPath(),
                Guid.NewGuid().ToString("N"),
                "local_artifact_db_parallel_record_gate");
            try
            {
                var setupDb = new JsonlDesignConversionLocalArtifactDb(root);
                var approvalRuntime = ToRuntimePendingCandidate(
                    BuildTrainingRecord($"parallel-approval-{iteration}", DesignConversionDpoUnits.ImproverUnit, usefulness: 0.94));
                var stableRuntime = ToRuntimePendingCandidate(
                    BuildTrainingRecord($"parallel-stable-{iteration}", DesignConversionDpoUnits.CriticUnit, usefulness: 0.93));

                await Save(setupDb, BuildArtifactRecord(
                    $"storage-record-parallel-approval-{iteration}",
                    $"job-parallel-approval-{iteration}",
                    $"request-parallel-approval-{iteration}",
                    DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                    [approvalRuntime],
                    "<main>parallel approval</main>",
                    ".parallel-approval{}"));
                await Save(setupDb, BuildArtifactRecord(
                    $"storage-record-parallel-stable-{iteration}",
                    $"job-parallel-stable-{iteration}",
                    $"request-parallel-stable-{iteration}",
                    DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                    [stableRuntime],
                    "<main>parallel stable</main>",
                    ".parallel-stable{}"));

                var approvalDb = new JsonlDesignConversionLocalArtifactDb(root);
                var appendDb = new JsonlDesignConversionLocalArtifactDb(root);
                var appendedRuntime = ToRuntimePendingCandidate(
                    BuildTrainingRecord($"parallel-append-{iteration}", DesignConversionDpoUnits.CriticUnit, usefulness: 0.92));
                var appendedRecord = BuildArtifactRecord(
                    $"storage-record-parallel-append-{iteration}",
                    $"job-parallel-append-{iteration}",
                    $"request-parallel-append-{iteration}",
                    DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                    [appendedRuntime],
                    "<main>parallel append</main>",
                    ".parallel-append{}");
                var startGate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                var approvalTask = Task.Run(async () =>
                {
                    await startGate.Task;
                    await Task.Yield();
                    return await approvalDb.ApproveRuntimeCreatedDpoCandidateAsync(
                        $"storage-record-parallel-approval-{iteration}",
                        approvalRuntime.RecordId,
                        "reviewer:parallel-record-gate",
                        "parallel approval rewrite evidence",
                        DateTimeOffset.UnixEpoch.AddMinutes(200 + iteration));
                });
                var appendTask = Task.Run(async () =>
                {
                    await startGate.Task;
                    await Task.Yield();
                    return await appendDb.AppendRecordAsync(appendedRecord);
                });

                startGate.SetResult(true);
                var approval = await approvalTask;
                var append = await appendTask;

                var appendDbA = new JsonlDesignConversionLocalArtifactDb(root);
                var appendDbB = new JsonlDesignConversionLocalArtifactDb(root);
                var parallelAppendA = BuildArtifactRecord(
                    $"storage-record-parallel-append-a-{iteration}",
                    $"job-parallel-append-a-{iteration}",
                    $"request-parallel-append-a-{iteration}",
                    DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                    [ToRuntimePendingCandidate(BuildTrainingRecord($"parallel-append-a-{iteration}", DesignConversionDpoUnits.ImproverUnit, usefulness: 0.91))],
                    "<main>parallel append A</main>",
                    ".parallel-append-a{}");
                var parallelAppendB = BuildArtifactRecord(
                    $"storage-record-parallel-append-b-{iteration}",
                    $"job-parallel-append-b-{iteration}",
                    $"request-parallel-append-b-{iteration}",
                    DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                    [ToRuntimePendingCandidate(BuildTrainingRecord($"parallel-append-b-{iteration}", DesignConversionDpoUnits.CriticUnit, usefulness: 0.90))],
                    "<main>parallel append B</main>",
                    ".parallel-append-b{}");
                var appendStartGate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                var appendTasks = new[]
                {
                    Task.Run(async () =>
                    {
                        await appendStartGate.Task;
                        await Task.Yield();
                        return await appendDbA.AppendRecordAsync(parallelAppendA);
                    }),
                    Task.Run(async () =>
                    {
                        await appendStartGate.Task;
                        await Task.Yield();
                        return await appendDbB.AppendRecordAsync(parallelAppendB);
                    })
                };

                appendStartGate.SetResult(true);
                var appendResults = await Task.WhenAll(appendTasks);

                var recordsPath = Path.Combine(root, "conversion-results.jsonl");
                var indexPath = Path.Combine(root, "conversion-results.index.json");
                var persistedText = await File.ReadAllTextAsync(recordsPath);
                var rawLines = await File.ReadAllLinesAsync(recordsPath);
                var nonblankLines = rawLines.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
                var parsedRecords = nonblankLines
                    .Select(line =>
                    {
                        Assert.NotNull(JsonNode.Parse(line));
                        var record = System.Text.Json.JsonSerializer.Deserialize<DesignConversionLocalArtifactDbRecord>(
                            line,
                            DesignConversionCheckpointJsonOptions.Standard);
                        Assert.NotNull(record);
                        return record!;
                    })
                    .ToArray();
                var reloaded = new JsonlDesignConversionLocalArtifactDb(root);
                var listed = await reloaded.ListRecordsAsync();
                var indexText = await File.ReadAllTextAsync(indexPath);
                var index = System.Text.Json.JsonSerializer.Deserialize<DesignConversionLocalArtifactDbIndex>(
                    indexText,
                    DesignConversionCheckpointJsonOptions.Standard);
                var recordIds = parsedRecords.Select(record => record.RecordId).ToArray();

                Assert.True(approval.IsSuccess, approval.ErrorMessage);
                Assert.True(append.IsSuccess, append.ErrorMessage);
                Assert.All(appendResults, result => Assert.True(result.IsSuccess, result.ErrorMessage));
                Assert.EndsWith(Environment.NewLine, persistedText, StringComparison.Ordinal);
                Assert.Equal(rawLines.Length, nonblankLines.Length);
                Assert.Equal(5, nonblankLines.Length);
                Assert.Equal($"storage-record-parallel-approval-{iteration}", recordIds[0]);
                Assert.Equal($"storage-record-parallel-stable-{iteration}", recordIds[1]);
                Assert.Equal(5, recordIds.Distinct(StringComparer.Ordinal).Count());
                Assert.Contains($"storage-record-parallel-append-{iteration}", recordIds);
                Assert.Contains($"storage-record-parallel-append-a-{iteration}", recordIds);
                Assert.Contains($"storage-record-parallel-append-b-{iteration}", recordIds);

                Assert.True(listed.IsSuccess, listed.ErrorMessage);
                Assert.Equal(5, listed.Value!.Count);
                Assert.All(recordIds, recordId => Assert.Contains(
                    listed.Value!,
                    record => string.Equals(record.RecordId, recordId, StringComparison.Ordinal)));
                var approvedRecord = Assert.Single(
                    listed.Value!,
                    record => string.Equals(record.RecordId, $"storage-record-parallel-approval-{iteration}", StringComparison.Ordinal));
                var approvedCandidate = Assert.Single(approvedRecord.DpoCandidates);
                Assert.Equal("approved", approvedCandidate.TrainingRecord.Provenance.Metadata["approval_status"]);
                Assert.Equal(bool.TrueString, approvedCandidate.TrainingRecord.Provenance.Metadata["approved_for_training"]);
                Assert.Equal("reviewer:parallel-record-gate", approvedCandidate.TrainingRecord.Provenance.Metadata["approved_by_ref"]);
                Assert.Equal("parallel approval rewrite evidence", approvedCandidate.TrainingRecord.Provenance.Metadata["approval_reason"]);
                Assert.Equal(
                    DateTimeOffset.UnixEpoch.AddMinutes(200 + iteration).ToString("O"),
                    approvedCandidate.TrainingRecord.Provenance.Metadata["approved_at_utc"]);
                Assert.NotNull(index);
                Assert.Equal(5, index!.RecordCount);
                Assert.Equal(1, index.ApprovedTrainingCandidateCount);
            }
            finally
            {
                if (Directory.Exists(root))
                {
                    Directory.Delete(root, recursive: true);
                }
            }
        }
    }

    [Fact]
    public async Task LocalArtifactDb_ExcludesProviderMissingAndQuarantinedRowsFromApprovedTraining()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local_artifact_db");
        var db = new JsonlDesignConversionLocalArtifactDb(root);
        var allowed = BuildTrainingRecord("allowed-train", DesignConversionDpoUnits.CriticUnit, usefulness: 0.82);
        var providerMissing = BuildTrainingRecord("provider-missing", DesignConversionDpoUnits.CriticUnit, usefulness: 0.99);
        var quarantined = BuildTrainingRecord("quarantined", DesignConversionDpoUnits.ImproverUnit, usefulness: 0.88) with
        {
            Quarantine = new DesignConversionQuarantineState(
                IsQuarantined: true,
                Reason: "manual_quarantine",
                EvidenceRefs: ["quarantine:evidence"])
        };

        await Save(db, BuildArtifactRecord(
            "storage-record-allowed",
            "job-allowed",
            "request-allowed",
            DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            [allowed],
            "<main>allowed</main>",
            ".allowed{}"));
        await Save(db, BuildArtifactRecord(
            "storage-record-provider",
            "job-provider",
            "request-provider",
            DesignConversionVisualQaBoundaryPolicy.ProviderRequired,
            [providerMissing],
            "<main>provider</main>",
            ".provider{}"));
        await Save(db, BuildArtifactRecord(
            "storage-record-quarantine",
            "job-quarantine",
            "request-quarantine",
            DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            [quarantined],
            "<main>quarantine</main>",
            ".quarantine{}"));

        var approved = await db.QueryApprovedTrainingCandidatesAsync();
        var display = await db.QueryDisplayRowsAsync(new DesignConversionDpoDisplayQuery(IncludeBlocked: true));
        var stored = await db.ListRecordsAsync();

        Assert.True(approved.IsSuccess, approved.ErrorMessage);
        var approvedRecord = Assert.Single(approved.Value!);
        Assert.Equal("allowed-train", approvedRecord.RecordId);
        Assert.True(display.IsSuccess, display.ErrorMessage);
        Assert.Equal(3, display.Value!.Rows.Count);
        Assert.Equal(1, display.Value.TrainingCandidateCount);
        Assert.Equal(2, display.Value.BlockedCount);
        Assert.Contains(display.Value.Rows, row => row.BlockedReason.Contains("provider_missing_or_blocked", StringComparison.Ordinal));
        Assert.Contains(display.Value.Rows, row => row.BlockedReason.Contains("manual_quarantine", StringComparison.Ordinal));
        Assert.Contains(stored.Value!.SelectMany(record => record.DpoCandidates), candidate =>
            candidate.RecordId == "provider-missing"
            && candidate.BlockReasons.Contains("provider_missing_or_blocked"));
        Assert.Contains(stored.Value!.SelectMany(record => record.DpoCandidates), candidate =>
            candidate.RecordId == "quarantined"
            && candidate.BlockReasons.Contains("manual_quarantine"));
    }

    [Fact]
    public async Task LocalArtifactDb_ApprovalRejectsQuarantinedAndNonRuntimeCandidatesWithoutMutation()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local_artifact_db_approval_rejection");
        var db = new JsonlDesignConversionLocalArtifactDb(root);
        var nonRuntime = BuildTrainingRecord("non-runtime", DesignConversionDpoUnits.ImproverUnit, usefulness: 0.91);
        var quarantinedRuntime = ToRuntimePendingCandidate(
            BuildTrainingRecord("runtime-quarantined", DesignConversionDpoUnits.CriticUnit, usefulness: 0.89)) with
        {
            Quarantine = new DesignConversionQuarantineState(
                IsQuarantined: true,
                Reason: "manual_quarantine",
                EvidenceRefs: ["quarantine:evidence"])
        };
        var artifactRecord = BuildArtifactRecord(
            "storage-record-approval-rejection",
            "job-approval-rejection",
            "request-approval-rejection",
            DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            [nonRuntime, quarantinedRuntime],
            "<main>approval rejection</main>",
            ".approval-rejection{}");
        var append = await db.AppendRecordAsync(artifactRecord);
        Assert.True(append.IsSuccess, append.ErrorMessage);
        var beforeText = await File.ReadAllTextAsync(Path.Combine(root, "conversion-results.jsonl"));

        var nonRuntimeApproval = await db.ApproveRuntimeCreatedDpoCandidateAsync(
            "storage-record-approval-rejection",
            nonRuntime.RecordId,
            "reviewer:pass106-local",
            "local evidence-bound review");
        var quarantinedApproval = await db.ApproveRuntimeCreatedDpoCandidateAsync(
            "storage-record-approval-rejection",
            quarantinedRuntime.RecordId,
            "reviewer:pass106-local",
            "local evidence-bound review");
        var afterText = await File.ReadAllTextAsync(Path.Combine(root, "conversion-results.jsonl"));
        var listed = await db.ListRecordsAsync(new DesignConversionLocalArtifactDbQuery(JobId: "job-approval-rejection"));

        Assert.False(nonRuntimeApproval.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_DPO_APPROVAL_NON_RUNTIME_CANDIDATE", nonRuntimeApproval.ErrorCode);
        Assert.False(quarantinedApproval.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_DPO_APPROVAL_QUARANTINED", quarantinedApproval.ErrorCode);
        Assert.Equal(beforeText, afterText);
        Assert.True(listed.IsSuccess, listed.ErrorMessage);
        var stored = Assert.Single(listed.Value!);
        var storedRuntime = Assert.Single(stored.DpoCandidates, candidate => candidate.RecordId == quarantinedRuntime.RecordId);
        Assert.Equal("pending", storedRuntime.TrainingRecord.Provenance.Metadata["approval_status"]);
        Assert.Equal(bool.FalseString, storedRuntime.TrainingRecord.Provenance.Metadata["approved_for_training"]);
        Assert.DoesNotContain("approved_by_ref", storedRuntime.TrainingRecord.Provenance.Metadata.Keys);
    }

    [Fact]
    public async Task LocalArtifactDb_HandoffExportRequiresApprovedRuntimeCandidateWithReviewerAndReason()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local_artifact_db_handoff_filter");
        var db = new JsonlDesignConversionLocalArtifactDb(root);
        var approvedRuntime = ToRuntimeApprovedCandidate(
            WithRuntimeArtifactProofRefs(
                BuildTrainingRecord("runtime-approved", DesignConversionDpoUnits.ImproverUnit, usefulness: 0.94),
                "handoff-filter-approved"));
        var pendingRuntime = ToRuntimePendingCandidate(
            BuildTrainingRecord("runtime-pending", DesignConversionDpoUnits.CriticUnit, usefulness: 0.93));
        var missingReviewer = ToRuntimeApprovedCandidate(
            BuildTrainingRecord("runtime-approved-missing-reviewer", DesignConversionDpoUnits.CriticUnit, usefulness: 0.92),
            includeReviewer: false,
            includeReason: true);
        var missingReason = ToRuntimeApprovedCandidate(
            BuildTrainingRecord("runtime-approved-missing-reason", DesignConversionDpoUnits.CriticUnit, usefulness: 0.91),
            includeReviewer: true,
            includeReason: false);
        var quarantinedRuntime = ToRuntimeApprovedCandidate(
            BuildTrainingRecord("runtime-approved-quarantined", DesignConversionDpoUnits.CriticUnit, usefulness: 0.90)) with
        {
            Quarantine = new DesignConversionQuarantineState(
                IsQuarantined: true,
                Reason: "manual_quarantine",
                EvidenceRefs: ["quarantine:evidence"])
        };
        var nonRuntime = BuildTrainingRecord("non-runtime-approved-looking", DesignConversionDpoUnits.CriticUnit, usefulness: 0.99);
        await Save(db, BuildArtifactRecord(
            "storage-record-handoff-filter",
            "job-handoff-filter",
            "request-handoff-filter",
            DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            [approvedRuntime, pendingRuntime, missingReviewer, missingReason, quarantinedRuntime, nonRuntime],
            "<main data-user-id=\"user-luba\">exact_prompt private html</main>",
            ".private-user{background:url('C:/Users/private-user/private.png');}") with
        {
            SanitizedSourceRefs = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["source_manifest_ref"] = "manifest://design-conversion/handoff-filter",
                ["source_kind"] = "figma",
                ["design_tree_payload_ref"] = "sha256:design-tree-handoff-filter",
                ["original_tree_payload_ref"] = "sha256:original-tree-handoff-filter",
                ["source_artifact:approved_expected"] = "sha256:handoff-filter-approved-chosen-expected",
                ["source_artifact:approved_actual"] = "sha256:handoff-filter-approved-chosen-actual"
            }
        });

        var export = await db.ExportApprovedRuntimeDpoCandidatesAsync(
            new DesignConversionLocalArtifactDbQuery(JobId: "job-handoff-filter"),
            DateTimeOffset.UnixEpoch.AddMinutes(107));
        var serialized = System.Text.Json.JsonSerializer.Serialize(export.Value);

        Assert.True(export.IsSuccess, export.ErrorMessage);
        Assert.Equal(1, export.Value!.ApprovedCandidateCount);
        var row = Assert.Single(export.Value.Rows);
        Assert.Equal("runtime-approved", row.TrainingRecordId);
        Assert.Equal("approved", row.ApprovalStatus);
        Assert.True(row.TrainingHandoffOnly);
        Assert.Equal("reviewer:pass107-local", row.ApprovedByRef);
        Assert.Equal("local approved candidate handoff", row.ApprovalReason);
        Assert.DoesNotContain(export.Value.Rows, item => item.TrainingRecordId == "runtime-pending");
        Assert.DoesNotContain(export.Value.Rows, item => item.TrainingRecordId == "runtime-approved-missing-reviewer");
        Assert.DoesNotContain(export.Value.Rows, item => item.TrainingRecordId == "runtime-approved-missing-reason");
        Assert.DoesNotContain(export.Value.Rows, item => item.TrainingRecordId == "runtime-approved-quarantined");
        Assert.DoesNotContain(export.Value.Rows, item => item.TrainingRecordId == "non-runtime-approved-looking");
        Assert.Contains("training_handoff_only", export.Value.Warnings);
        Assert.Contains("not_trained", export.Value.Warnings);
        Assert.Equal(bool.TrueString, export.Value.BoundaryMetadata["training_handoff_only"]);
        Assert.Equal(bool.FalseString, export.Value.BoundaryMetadata["training_grade"]);
        Assert.Equal(bool.TrueString, export.Value.BoundaryMetadata["not_training_grade"]);
        Assert.Equal(bool.TrueString, export.Value.BoundaryMetadata["not_trained"]);
        Assert.Equal(bool.FalseString, export.Value.BoundaryMetadata["trained_model_ready"]);
        Assert.Equal(bool.FalseString, row.BoundaryMetadata["model_readiness_claimed"]);
        Assert.Equal(bool.FalseString, row.BoundaryMetadata["provider_payload_included"]);
        Assert.StartsWith("dpo-handoff-internal-vector-", row.BoundaryMetadata["internal_vector_hash"], StringComparison.Ordinal);
        Assert.True(int.Parse(row.BoundaryMetadata["internal_vector_dimensions"]) > 0);
        Assert.StartsWith("dcm-source-encoder-vector-", row.BoundaryMetadata["source_encoder_vector_hash"], StringComparison.Ordinal);
        Assert.Contains("source_json_node_count", row.BoundaryMetadata["source_encoder_feature_names"]);
        Assert.DoesNotContain("visual_score_margin", row.BoundaryMetadata["source_encoder_feature_names"]);
        Assert.DoesNotContain("chosen_css_rule_count", row.BoundaryMetadata["source_encoder_feature_names"]);
        Assert.Contains("visual_score_margin=", row.BoundaryMetadata["result_signal_summary"]);
        Assert.Equal(bool.FalseString, row.BoundaryMetadata["training_grade"]);
        Assert.Equal(bool.TrueString, row.BoundaryMetadata["not_training_grade"]);
        Assert.Equal("source_kind_source_id_not_vector_features", row.BoundaryMetadata["source_encoder_shortcut_guard"]);
        Assert.Equal(bool.FalseString, row.BoundaryMetadata["source_kind_in_vector"]);
        Assert.Equal(bool.FalseString, row.BoundaryMetadata["source_id_in_vector"]);
        Assert.DoesNotContain("<main", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".private-user", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/Users/", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("user-luba", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("exact_prompt", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("private.png", serialized, StringComparison.OrdinalIgnoreCase);

        var intake = await db.CreateTrainingIntakeAsync(
            export.Value,
            "requester:pass108-local",
            "future ILearningMachine intake only; no training started",
            DateTimeOffset.UnixEpoch.AddMinutes(108));
        var listedIntakes = await db.ListTrainingIntakeRequestsAsync();
        var intakeText = await File.ReadAllTextAsync(Path.Combine(root, "training-intake-requests.jsonl"));
        var serializedIntake = System.Text.Json.JsonSerializer.Serialize(intake.Value);

        Assert.True(intake.IsSuccess, intake.ErrorMessage);
        Assert.StartsWith("training-intake-", intake.Value!.IntakeId, StringComparison.Ordinal);
        Assert.StartsWith("memory://design-conversion/training-intake/training-intake-", intake.Value.IntakeRef, StringComparison.Ordinal);
        Assert.Equal(export.Value.ArtifactId, intake.Value.SourceHandoffArtifactId);
        Assert.Equal(1, intake.Value.ApprovedCandidateCount);
        Assert.Equal(1, intake.Value.AcceptedCandidateCount);
        Assert.True(intake.Value.TrainingIntakeOnly);
        Assert.True(intake.Value.TrainingNotStarted);
        Assert.True(intake.Value.NotTrained);
        Assert.True(intake.Value.WeightsNotProduced);
        Assert.True(intake.Value.CheckpointNotMutated);
        Assert.True(intake.Value.ModelUploadNotReady);
        Assert.True(intake.Value.ProductNotReady);
        Assert.True(intake.Value.HeldOutNotProven);
        Assert.True(intake.Value.SourceDisjointNotProven);
        Assert.Contains("training_intake_only", intake.Value.Warnings);
        Assert.Contains("training_not_started", intake.Value.Warnings);
        Assert.Contains("checkpoint_not_mutated", intake.Value.Warnings);
        Assert.Equal(bool.TrueString, intake.Value.BoundaryMetadata["training_intake_only"]);
        Assert.Equal(bool.TrueString, intake.Value.BoundaryMetadata["training_not_started"]);
        Assert.Equal(bool.TrueString, intake.Value.BoundaryMetadata["not_trained"]);
        Assert.Equal(bool.TrueString, intake.Value.BoundaryMetadata["weights_not_produced"]);
        Assert.Equal(bool.TrueString, intake.Value.BoundaryMetadata["checkpoint_not_mutated"]);
        Assert.Equal(bool.TrueString, intake.Value.BoundaryMetadata["model_upload_not_ready"]);
        Assert.Equal(bool.FalseString, intake.Value.BoundaryMetadata["training_started"]);
        Assert.Equal(bool.FalseString, intake.Value.BoundaryMetadata["trained_model_ready"]);
        Assert.Equal(bool.FalseString, intake.Value.BoundaryMetadata["model_readiness_claimed"]);
        Assert.Equal("future_ILearningMachine", intake.Value.BoundaryMetadata["target_training_system"]);
        var intakeCandidate = Assert.Single(intake.Value.CandidateRefs);
        Assert.Equal(row.CandidateId, intakeCandidate.CandidateId);
        Assert.Equal(row.TrainingRecordId, intakeCandidate.TrainingRecordId);
        Assert.Equal(row.RecordHash, intakeCandidate.HandoffRecordHash);
        Assert.Equal(row.SourceManifestRef, intakeCandidate.SourceManifestRef);
        Assert.StartsWith("sha256:", intakeCandidate.SourceManifestHash, StringComparison.Ordinal);
        Assert.StartsWith("sha256:", intakeCandidate.SourceIdentityHash, StringComparison.Ordinal);
        Assert.Equal(row.ChosenPayloadHash, intakeCandidate.ChosenPayloadHash);
        Assert.Equal(bool.TrueString, intakeCandidate.BoundaryMetadata["training_not_started"]);
        Assert.Equal(row.BoundaryMetadata["internal_vector_hash"], intakeCandidate.BoundaryMetadata["internal_vector_hash"]);
        Assert.Equal(row.BoundaryMetadata["internal_vector_dimensions"], intakeCandidate.BoundaryMetadata["internal_vector_dimensions"]);
        Assert.Equal(row.BoundaryMetadata["source_encoder_vector_hash"], intakeCandidate.BoundaryMetadata["source_encoder_vector_hash"]);
        Assert.Contains("source_json_node_count", intakeCandidate.BoundaryMetadata["source_encoder_feature_names"]);
        Assert.DoesNotContain("chosen_css_rule_count", intakeCandidate.BoundaryMetadata["source_encoder_feature_names"]);
        Assert.DoesNotContain("visual_score_margin", intakeCandidate.BoundaryMetadata["source_encoder_feature_names"]);
        Assert.Equal(bool.FalseString, intakeCandidate.BoundaryMetadata["source_kind_in_vector"]);
        Assert.Equal(bool.FalseString, intakeCandidate.BoundaryMetadata["source_id_in_vector"]);
        Assert.Equal("proven", intake.Value.BoundaryMetadata["source_manifest_status"]);
        Assert.Equal("blocked_same_source", intake.Value.BoundaryMetadata["source_disjoint_status"]);
        Assert.True(listedIntakes.IsSuccess, listedIntakes.ErrorMessage);
        Assert.Single(listedIntakes.Value!);

        var beforePreflightText = await File.ReadAllTextAsync(Path.Combine(root, "training-intake-requests.jsonl"));
        var preflight = await db.RunTrainingIntakePreflightAsync(intake.Value.IntakeId);
        var afterPreflightText = await File.ReadAllTextAsync(Path.Combine(root, "training-intake-requests.jsonl"));
        var serializedPreflight = System.Text.Json.JsonSerializer.Serialize(preflight.Value);
        Assert.True(preflight.IsSuccess, preflight.ErrorMessage);
        Assert.StartsWith("training-intake-preflight-", preflight.Value!.PreflightId, StringComparison.Ordinal);
        Assert.Equal(intake.Value.IntakeId, preflight.Value.TrainingIntakeId);
        Assert.Equal(1, preflight.Value.AcceptedCandidateCount);
        Assert.False(preflight.Value.MayStartTraining);
        Assert.Equal("proven", preflight.Value.SourceManifestStatus);
        Assert.Equal("blocked_same_source", preflight.Value.SourceDisjointStatus);
        Assert.False(preflight.Value.TrainingStarted);
        Assert.False(preflight.Value.CheckpointMutated);
        Assert.False(preflight.Value.WeightsProduced);
        Assert.False(preflight.Value.ModelUploadReady);
        Assert.Contains("source_disjoint_blocked_same_source", preflight.Value.BlockReasons);
        Assert.Equal(bool.TrueString, preflight.Value.BoundaryMetadata["preflight_only"]);
        Assert.Equal(bool.TrueString, preflight.Value.BoundaryMetadata["future_training_run_permission_only"]);
        Assert.Equal(bool.FalseString, preflight.Value.BoundaryMetadata["may_start_training"]);
        Assert.Equal("proven", preflight.Value.BoundaryMetadata["source_manifest_status"]);
        Assert.Equal("blocked_same_source", preflight.Value.BoundaryMetadata["source_disjoint_status"]);
        Assert.Equal(bool.FalseString, preflight.Value.BoundaryMetadata["training_started"]);
        Assert.Equal(bool.FalseString, preflight.Value.BoundaryMetadata["checkpoint_mutated"]);
        Assert.Equal(bool.FalseString, preflight.Value.BoundaryMetadata["weights_produced"]);
        Assert.Equal(bool.FalseString, preflight.Value.BoundaryMetadata["model_upload_ready"]);
        Assert.Equal("local_preflight_only_no_training_runner", preflight.Value.BoundaryMetadata["training_execution_boundary"]);
        Assert.Equal(beforePreflightText, afterPreflightText);
        Assert.Contains("training_not_started", intakeText, StringComparison.Ordinal);
        Assert.Contains("checkpoint_not_mutated", intakeText, StringComparison.Ordinal);
        Assert.DoesNotContain("<main", serializedIntake, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".private-user", serializedIntake, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/Users/", serializedIntake, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("exact_prompt", serializedIntake, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("private.png", serializedIntake, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<main", intakeText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".private-user", intakeText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/Users/", intakeText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("exact_prompt", intakeText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("private.png", intakeText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<main", serializedPreflight, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".private-user", serializedPreflight, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/Users/", serializedPreflight, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("exact_prompt", serializedPreflight, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("private.png", serializedPreflight, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LocalArtifactDb_DesignConversionDpoRecordCarriesLearningFieldsIntoTrainingIntake()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local_artifact_db_learning_fields");
        var db = new JsonlDesignConversionLocalArtifactDb(root);
        var baseRecord = BuildTrainingRecord("runtime-learning-fields", DesignConversionDpoUnits.ImproverUnit, usefulness: 0.96);
        var withComparison = baseRecord with
        {
            Chosen = baseRecord.Chosen with
            {
                QaResults =
                [
                    BuildQaResult(
                        "learning-fields-chosen",
                        "sha256:expected-design-render-learning-fields",
                        "sha256:actual-browser-render-learning-fields")
                ]
            },
            Rejected = baseRecord.Rejected with
            {
                QaResults =
                [
                    BuildQaResult(
                        "learning-fields-rejected",
                        "sha256:expected-design-render-learning-fields",
                        "sha256:actual-browser-render-learning-fields")
                ]
            },
            Provenance = baseRecord.Provenance with
            {
                SourceManifestRef = "manifest://design-conversion/learning-fields",
                Metadata = new Dictionary<string, string>(baseRecord.Provenance.Metadata, StringComparer.Ordinal)
                {
                    ["source_design_id"] = "learning-fields-source"
                }
            }
        };
        var approvedRuntime = ToRuntimeApprovedCandidate(withComparison);
        var artifactRecord = BuildArtifactRecord(
            "storage-record-learning-fields",
            "job-learning-fields",
            "request-learning-fields",
            DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            [approvedRuntime],
            "<main class=\"learning-fields\">chosen artifact</main>",
            ".learning-fields{display:grid;gap:12px;}") with
        {
            SanitizedSourceRefs = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["source_manifest_ref"] = "manifest://design-conversion/learning-fields",
                ["source_kind"] = "figma",
                ["design_tree_payload_ref"] = "sha256:design-tree-learning-fields",
                ["original_tree_payload_ref"] = "sha256:original-tree-learning-fields",
                ["source_artifact:expected_desktop"] = "sha256:expected-design-render-learning-fields",
                ["source_artifact:actual_desktop"] = "sha256:actual-browser-render-learning-fields"
            },
            Metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["attempt_count"] = "2",
                ["stop_reason"] = "quality_threshold_reached",
                ["quality_reached"] = bool.TrueString,
                ["final_candidate_from_loop"] = bool.TrueString,
                ["comparison_diff_metadata_ref"] = "sha256:comparison-diff-metadata-learning-fields",
                ["runtime_created_dpo_candidate_count"] = "1",
                ["runtime_created_dpo_candidate"] = bool.TrueString,
                ["improvement_loop_to_local_training_store"] = bool.TrueString,
                ["local_training_store_dpo_source"] = "runtime_visual_qa_loop",
                ["refs_or_hashes_only"] = bool.TrueString,
                ["product_not_ready"] = bool.TrueString,
                ["production_db_ready"] = bool.FalseString,
                ["model_readiness_claimed"] = bool.FalseString
            }
        };
        await Save(db, artifactRecord);

        var listed = await db.ListRecordsAsync(new DesignConversionLocalArtifactDbQuery(JobId: "job-learning-fields"));
        var handoff = await db.ExportApprovedRuntimeDpoCandidatesAsync(
            new DesignConversionLocalArtifactDbQuery(JobId: "job-learning-fields"),
            DateTimeOffset.UnixEpoch.AddMinutes(207));
        var intake = await db.CreateTrainingIntakeAsync(
            handoff.Value!,
            "requester:learning-fields",
            "future ILearningMachine intake only; no training started",
            DateTimeOffset.UnixEpoch.AddMinutes(208));

        Assert.True(listed.IsSuccess, listed.ErrorMessage);
        var stored = Assert.Single(listed.Value!);
        Assert.StartsWith("dcm-html_result-", stored.ConversionResult.HtmlRef, StringComparison.Ordinal);
        Assert.StartsWith("dcm-css_result-", stored.ConversionResult.CssRef, StringComparison.Ordinal);
        Assert.Equal(bool.FalseString, stored.Metadata["product_ready"]);
        Assert.Equal(bool.TrueString, stored.Metadata["product_not_ready"]);
        Assert.Equal("sha256:comparison-diff-metadata-learning-fields", stored.Metadata["comparison_diff_metadata_ref"]);
        var storedCandidate = Assert.Single(stored.DpoCandidates);
        Assert.True(storedCandidate.ProductNotReady);
        Assert.True(storedCandidate.TrainingRecord.Chosen.Accepted);
        Assert.False(storedCandidate.TrainingRecord.Rejected.Accepted);

        Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
        var row = Assert.Single(handoff.Value!.Rows);
        Assert.StartsWith("dcm-chosen_html-", row.ChosenHtmlRef, StringComparison.Ordinal);
        Assert.StartsWith("dcm-chosen_css-", row.ChosenCssRef, StringComparison.Ordinal);
        Assert.StartsWith("dcm-rejected_html-", row.RejectedHtmlRef, StringComparison.Ordinal);
        Assert.StartsWith("dcm-rejected_css-", row.RejectedCssRef, StringComparison.Ordinal);
        Assert.Equal("sha256:design-tree-learning-fields", row.DesignTreeRef);
        Assert.Equal("sha256:original-tree-learning-fields", row.OriginalTreeRef);
        Assert.StartsWith("dpo-handoff-source-structure-", row.SourceStructureHash, StringComparison.Ordinal);
        Assert.StartsWith("dpo-handoff-chosen-payload-", row.ChosenPayloadHash, StringComparison.Ordinal);
        Assert.StartsWith("dpo-handoff-rejected-payload-", row.RejectedPayloadHash, StringComparison.Ordinal);
        Assert.NotEqual(row.ChosenPayloadHash, row.RejectedPayloadHash);
        Assert.Contains("sha256:expected-design-render-learning-fields", row.SourceArtifactRefs);
        Assert.Contains("sha256:actual-browser-render-learning-fields", row.SourceArtifactRefs);
        Assert.Contains("sha256:render-desktop", row.RenderArtifactRefs);
        Assert.Contains("sha256:expected-design-render-learning-fields", row.ComparisonArtifactRefs);
        Assert.Contains("sha256:actual-browser-render-learning-fields", row.ComparisonArtifactRefs);
        Assert.Equal(row.ChosenPayloadHash, row.BoundaryMetadata["chosen_payload_hash"]);
        Assert.Equal(row.RejectedPayloadHash, row.BoundaryMetadata["rejected_payload_hash"]);
        Assert.Equal("2", row.BoundaryMetadata["attempt_count"]);
        Assert.Equal("quality_threshold_reached", row.BoundaryMetadata["stop_reason"]);
        Assert.Equal(bool.TrueString, row.BoundaryMetadata["quality_reached"]);
        Assert.Equal(bool.TrueString, row.BoundaryMetadata["final_candidate_from_loop"]);
        Assert.Equal("sha256:comparison-diff-metadata-learning-fields", row.BoundaryMetadata["comparison_diff_metadata_ref"]);
        Assert.Equal(bool.FalseString, row.BoundaryMetadata["product_ready"]);
        Assert.Equal(bool.TrueString, row.BoundaryMetadata["product_not_ready"]);

        Assert.True(intake.IsSuccess, intake.ErrorMessage);
        Assert.True(intake.Value!.ProductNotReady);
        Assert.Equal(bool.FalseString, intake.Value.BoundaryMetadata["product_ready"]);
        var intakeCandidate = Assert.Single(intake.Value.CandidateRefs);
        Assert.Equal(row.RecordHash, intakeCandidate.HandoffRecordHash);
        Assert.Equal(row.ChosenHtmlRef, intakeCandidate.ChosenHtmlRef);
        Assert.Equal(row.ChosenCssRef, intakeCandidate.ChosenCssRef);
        Assert.Equal(row.RejectedHtmlRef, intakeCandidate.RejectedHtmlRef);
        Assert.Equal(row.RejectedCssRef, intakeCandidate.RejectedCssRef);
        Assert.Equal(row.DesignTreeRef, intakeCandidate.DesignTreeRef);
        Assert.Equal(row.OriginalTreeRef, intakeCandidate.OriginalTreeRef);
        Assert.Equal(row.SourceStructureHash, intakeCandidate.SourceStructureHash);
        Assert.Equal(row.ChosenPayloadHash, intakeCandidate.ChosenPayloadHash);
        Assert.Equal(row.RejectedPayloadHash, intakeCandidate.RejectedPayloadHash);
        Assert.Contains("sha256:render-desktop", intakeCandidate.RenderArtifactRefs);
        Assert.Contains("sha256:expected-design-render-learning-fields", intakeCandidate.ComparisonArtifactRefs);
        Assert.Contains("sha256:actual-browser-render-learning-fields", intakeCandidate.ComparisonArtifactRefs);
        Assert.Equal(row.BoundaryMetadata["comparison_diff_metadata_ref"], intakeCandidate.BoundaryMetadata["comparison_diff_metadata_ref"]);
        Assert.Equal(row.BoundaryMetadata["attempt_count"], intakeCandidate.BoundaryMetadata["attempt_count"]);
        Assert.Equal(row.BoundaryMetadata["stop_reason"], intakeCandidate.BoundaryMetadata["stop_reason"]);
        Assert.Equal(bool.FalseString, intakeCandidate.BoundaryMetadata["product_ready"]);
        Assert.Equal(bool.TrueString, intakeCandidate.BoundaryMetadata["product_not_ready"]);

        static RenderQaResultRecord BuildQaResult(string id, string expectedSha, string actualSha)
        {
            var resolution = new RenderQaResolution("desktop", 1440, 900, 1);
            var expected = new RenderQaImageRecord(
                ImageId: $"{id}:expected",
                Label: "expected-design-render",
                Resolution: resolution,
                Uri: $"artifact://design-conversion/{id}/expected.png",
                Sha256: expectedSha,
                Metadata: new Dictionary<string, string>
                {
                    ["role"] = "expected_design_render"
                });
            var actual = new RenderQaImageRecord(
                ImageId: $"{id}:actual",
                Label: "actual-browser-render",
                Resolution: resolution,
                Uri: $"artifact://design-conversion/{id}/actual.png",
                Sha256: actualSha,
                Metadata: new Dictionary<string, string>
                {
                    ["role"] = "actual_browser_render"
                });
            var verdict = new VisualVerdict(
                VerdictId: $"{id}:comparison",
                Score: 0.91,
                Defects:
                [
                    new VisualDefect(
                        DefectId: $"{id}:spacing",
                        Category: "spacing",
                        Severity: "minor",
                        Description: "Expected and actual renders differ by a small spacing margin.",
                        X: 16,
                        Y: 24,
                        Width: 80,
                        Height: 20,
                        EvidenceImageIds: ["expected", "actual"])
                ],
                EvidenceRefs: [$"artifact://design-conversion/{id}/diff.png"],
                Passed: true);

            return new RenderQaResultRecord(
                ResultId: $"{id}:render-qa",
                RequestId: $"{id}:request",
                Expected: expected,
                Actual: actual,
                Verdict: verdict,
                Metrics: new Dictionary<string, double>
                {
                    ["visual_distance"] = 0.09
                },
                Warnings: []);
        }
    }

    [Fact]
    public async Task LocalArtifactDb_TrainingIntakeRejectsEmptyPendingRawAndBadBoundaryHandoffs()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local_artifact_db_training_intake_filter");
        var db = new JsonlDesignConversionLocalArtifactDb(root);
        var pendingRuntime = ToRuntimePendingCandidate(
            BuildTrainingRecord("runtime-pending-intake", DesignConversionDpoUnits.CriticUnit, usefulness: 0.93));
        await Save(db, BuildArtifactRecord(
            "storage-record-intake-pending",
            "job-intake-pending",
            "request-intake-pending",
            DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            [pendingRuntime],
            "<main>pending intake</main>",
            ".pending-intake{}"));

        var emptyHandoff = await db.ExportApprovedRuntimeDpoCandidatesAsync(
            new DesignConversionLocalArtifactDbQuery(JobId: "job-intake-pending"),
            DateTimeOffset.UnixEpoch.AddMinutes(107));
        var zeroIntake = await db.CreateTrainingIntakeAsync(
            emptyHandoff.Value!,
            "requester:pass108-local",
            "future ILearningMachine intake only; no training started",
            DateTimeOffset.UnixEpoch.AddMinutes(108));
        var zeroListed = await db.ListTrainingIntakeRequestsAsync();

        Assert.True(emptyHandoff.IsSuccess, emptyHandoff.ErrorMessage);
        Assert.Equal(0, emptyHandoff.Value!.ApprovedCandidateCount);
        Assert.True(zeroIntake.IsSuccess, zeroIntake.ErrorMessage);
        Assert.Equal(0, zeroIntake.Value!.AcceptedCandidateCount);
        Assert.Empty(zeroIntake.Value.CandidateRefs);
        Assert.Equal(string.Empty, zeroIntake.Value.IntakeRef);
        Assert.Contains("no_approved_candidates", zeroIntake.Value.Warnings);
        Assert.Contains("training_intake_not_enqueued", zeroIntake.Value.Warnings);
        Assert.True(zeroIntake.Value.TrainingNotStarted);
        Assert.True(zeroIntake.Value.NotTrained);
        Assert.True(File.Exists(Path.Combine(root, "training-intake-requests.jsonl")));
        Assert.True(zeroListed.IsSuccess, zeroListed.ErrorMessage);
        var zeroAuditRecord = Assert.Single(zeroListed.Value!);
        Assert.Equal(zeroIntake.Value.IntakeId, zeroAuditRecord.IntakeId);
        Assert.Equal(0, zeroAuditRecord.AcceptedCandidateCount);
        Assert.Empty(zeroAuditRecord.CandidateRefs);
        Assert.Equal(string.Empty, zeroAuditRecord.IntakeRef);
        Assert.True(zeroAuditRecord.TrainingNotStarted);
        Assert.True(zeroAuditRecord.NotTrained);

        var approvedRuntime = ToRuntimeApprovedCandidate(
            WithRuntimeArtifactProofRefs(
                BuildTrainingRecord("runtime-approved-intake", DesignConversionDpoUnits.ImproverUnit, usefulness: 0.94),
                "runtime-approved-intake"));
        await Save(db, BuildArtifactRecord(
            "storage-record-intake-approved",
            "job-intake-approved",
            "request-intake-approved",
            DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            [approvedRuntime],
            "<main data-user-id=\"user-luba\">exact_prompt private html</main>",
            ".private-user{background:url('C:/Users/private-user/private.png');}") with
        {
            SanitizedSourceRefs = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["source_manifest_ref"] = "manifest://design-conversion/runtime-approved-intake",
                ["source_kind"] = "figma",
                ["design_tree_payload_ref"] = "sha256:design-tree-runtime-approved-intake",
                ["original_tree_payload_ref"] = "sha256:original-tree-runtime-approved-intake",
                ["source_artifact:expected_desktop"] = "sha256:runtime-approved-intake-source-expected",
                ["source_artifact:actual_desktop"] = "sha256:runtime-approved-intake-source-actual"
            }
        });
        var goodHandoff = await db.ExportApprovedRuntimeDpoCandidatesAsync(
            new DesignConversionLocalArtifactDbQuery(JobId: "job-intake-approved"),
            DateTimeOffset.UnixEpoch.AddMinutes(107));
        Assert.True(goodHandoff.IsSuccess, goodHandoff.ErrorMessage);
        var goodRow = Assert.Single(goodHandoff.Value!.Rows);
        var goodIntake = await db.CreateTrainingIntakeAsync(
            goodHandoff.Value,
            "requester:pass110-local",
            "future ILearningMachine preflight only; no training started",
            DateTimeOffset.UnixEpoch.AddMinutes(110));
        Assert.True(goodIntake.IsSuccess, goodIntake.ErrorMessage);
        var trainingIntakePath = Path.Combine(root, "training-intake-requests.jsonl");
        var zeroQueuedIntake = goodIntake.Value! with
        {
            IntakeId = "training-intake-zero-preflight",
            IntakeRef = string.Empty,
            ApprovedCandidateCount = 0,
            AcceptedCandidateCount = 0,
            CandidateRefs = [],
            BoundaryMetadata = new Dictionary<string, string>(
                goodIntake.Value.BoundaryMetadata,
                StringComparer.Ordinal)
            {
                ["accepted_candidate_count"] = "0",
                ["training_execution_boundary"] = "zero_intake_queued_for_preflight_negative_test"
            }
        };
        var badBoundaryIntake = goodIntake.Value with
        {
            IntakeId = "training-intake-bad-boundary-preflight",
            TrainingNotStarted = false,
            BoundaryMetadata = new Dictionary<string, string>(
                goodIntake.Value.BoundaryMetadata,
                StringComparer.Ordinal)
            {
                ["training_not_started"] = bool.FalseString,
                ["training_started"] = bool.TrueString
            }
        };
        var rawQueuedIntake = goodIntake.Value with
        {
            IntakeId = "training-intake-raw-preflight",
            RequestReason = "exact_prompt C:/Users/private-user/private.fig"
        };
        await File.AppendAllTextAsync(
            trainingIntakePath,
            string.Join(
                Environment.NewLine,
                [
                    System.Text.Json.JsonSerializer.Serialize(zeroQueuedIntake),
                    System.Text.Json.JsonSerializer.Serialize(badBoundaryIntake),
                    System.Text.Json.JsonSerializer.Serialize(rawQueuedIntake),
                    string.Empty
                ]));
        var preflightBeforeText = await File.ReadAllTextAsync(trainingIntakePath);

        var zeroPreflight = await db.RunTrainingIntakePreflightAsync(zeroQueuedIntake.IntakeId);
        var badBoundaryPreflight = await db.RunTrainingIntakePreflightAsync(badBoundaryIntake.IntakeId);
        var rawPreflight = await db.RunTrainingIntakePreflightAsync(rawQueuedIntake.IntakeId);
        var zeroAuditPreflight = await db.RunTrainingIntakePreflightAsync(zeroIntake.Value!.IntakeId);
        var preflightAfterText = await File.ReadAllTextAsync(trainingIntakePath);

        Assert.True(zeroPreflight.IsSuccess, zeroPreflight.ErrorMessage);
        Assert.False(zeroPreflight.Value!.MayStartTraining);
        Assert.False(zeroPreflight.Value.TrainingStarted);
        Assert.False(zeroPreflight.Value.CheckpointMutated);
        Assert.False(zeroPreflight.Value.WeightsProduced);
        Assert.False(zeroPreflight.Value.ModelUploadReady);
        Assert.Contains("no_accepted_candidates", zeroPreflight.Value.BlockReasons);
        Assert.True(badBoundaryPreflight.IsSuccess, badBoundaryPreflight.ErrorMessage);
        Assert.False(badBoundaryPreflight.Value!.MayStartTraining);
        Assert.Contains("training_not_started_required", badBoundaryPreflight.Value.BlockReasons);
        Assert.Contains("boundary_metadata_training_not_started_required", badBoundaryPreflight.Value.BlockReasons);
        Assert.Contains("boundary_metadata_training_started_must_be_false", badBoundaryPreflight.Value.BlockReasons);
        Assert.True(rawPreflight.IsSuccess, rawPreflight.ErrorMessage);
        Assert.False(rawPreflight.Value!.MayStartTraining);
        Assert.Contains("raw_or_private_marker_detected", rawPreflight.Value.BlockReasons);
        Assert.True(zeroAuditPreflight.IsSuccess, zeroAuditPreflight.ErrorMessage);
        Assert.False(zeroAuditPreflight.Value!.MayStartTraining);
        Assert.Equal(0, zeroAuditPreflight.Value.AcceptedCandidateCount);
        Assert.Contains("no_accepted_candidates", zeroAuditPreflight.Value.BlockReasons);
        Assert.Equal(preflightBeforeText, preflightAfterText);

        var badBoundaryMetadata = new Dictionary<string, string>(
            goodHandoff.Value.BoundaryMetadata,
            StringComparer.Ordinal)
        {
            ["not_trained"] = bool.FalseString
        };
        var badBoundary = goodHandoff.Value with
        {
            BoundaryMetadata = badBoundaryMetadata
        };
        var rawHandoff = goodHandoff.Value with
        {
            Rows =
            [
                goodRow with
                {
                    SourceManifestRef = "C:/Users/private-user/private.fig"
                }
            ]
        };
        var missingReviewerHandoff = goodHandoff.Value with
        {
            Rows =
            [
                goodRow with
                {
                    ApprovedByRef = ""
                }
            ]
        };

        var badBoundaryResult = await db.CreateTrainingIntakeAsync(
            badBoundary,
            "requester:pass108-local",
            "future ILearningMachine intake only; no training started",
            DateTimeOffset.UnixEpoch.AddMinutes(108));
        var rawResult = await db.CreateTrainingIntakeAsync(
            rawHandoff,
            "requester:pass108-local",
            "future ILearningMachine intake only; no training started",
            DateTimeOffset.UnixEpoch.AddMinutes(108));
        var missingReviewerResult = await db.CreateTrainingIntakeAsync(
            missingReviewerHandoff,
            "requester:pass108-local",
            "future ILearningMachine intake only; no training started",
            DateTimeOffset.UnixEpoch.AddMinutes(108));

        Assert.False(badBoundaryResult.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_TRAINING_INTAKE_BOUNDARY_METADATA_REQUIRED", badBoundaryResult.ErrorCode);
        Assert.False(rawResult.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_TRAINING_INTAKE_HANDOFF_UNSANITIZED", rawResult.ErrorCode);
        Assert.False(missingReviewerResult.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_TRAINING_INTAKE_APPROVAL_METADATA_REQUIRED", missingReviewerResult.ErrorCode);
    }

    [Fact]
    public async Task LocalArtifactDb_RunTrainingIntakeAsync_BlocksMissingEmptyPreflightAndUnapprovedBeforeRunner()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local_artifact_db_training_intake_run_blocks");
        var db = new JsonlDesignConversionLocalArtifactDb(root);
        var missing = await db.RunTrainingIntakeAsync("training-intake-missing-run");

        Assert.False(missing.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_TRAINING_INTAKE_PREFLIGHT_NOT_FOUND", missing.ErrorCode);

        var approvedRuntime = ToRuntimeApprovedCandidate(
            WithRuntimeArtifactProofRefs(
                BuildTrainingRecord("runtime-approved-run-blocks", DesignConversionDpoUnits.CriticUnit, usefulness: 0.94),
                "runtime-approved-run-blocks"));
        var pendingRuntime = ToRuntimePendingCandidate(
            BuildTrainingRecord("runtime-pending-run-blocks", DesignConversionDpoUnits.CriticUnit, usefulness: 0.93));
        await Save(db, BuildArtifactRecord(
            "storage-record-run-blocks",
            "job-run-blocks",
            "request-run-blocks",
            DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            [approvedRuntime, pendingRuntime],
            "<main data-user-id=\"user-luba\">exact_prompt private html</main>",
            ".private-user{background:url('C:/Users/private-user/private.png');}") with
        {
            SanitizedSourceRefs = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["source_manifest_ref"] = "manifest://design-conversion/runtime-approved-run-blocks",
                ["source_kind"] = "figma",
                ["design_tree_payload_ref"] = "sha256:design-tree-runtime-approved-run-blocks",
                ["original_tree_payload_ref"] = "sha256:original-tree-runtime-approved-run-blocks",
                ["source_artifact:expected_desktop"] = "sha256:runtime-approved-run-blocks-source-expected",
                ["source_artifact:actual_desktop"] = "sha256:runtime-approved-run-blocks-source-actual"
            }
        });
        var handoff = await db.ExportApprovedRuntimeDpoCandidatesAsync(
            new DesignConversionLocalArtifactDbQuery(JobId: "job-run-blocks"),
            DateTimeOffset.UnixEpoch.AddMinutes(111));
        var intake = await db.CreateTrainingIntakeAsync(
            handoff.Value!,
            "requester:pass111-local",
            "local runner bridge request; no training readiness claimed",
            DateTimeOffset.UnixEpoch.AddMinutes(111));
        var listed = await db.ListRecordsAsync(new DesignConversionLocalArtifactDbQuery(JobId: "job-run-blocks"));
        var stored = Assert.Single(listed.Value!);
        var pendingCandidate = stored.DpoCandidates.Single(candidate =>
            candidate.TrainingRecord.RecordId == "runtime-pending-run-blocks");
        var intakePath = Path.Combine(root, "training-intake-requests.jsonl");
        var zeroQueuedIntake = intake.Value! with
        {
            IntakeId = "training-intake-empty-run-blocked",
            IntakeRef = string.Empty,
            ApprovedCandidateCount = 0,
            AcceptedCandidateCount = 0,
            CandidateRefs = [],
            BoundaryMetadata = new Dictionary<string, string>(
                intake.Value.BoundaryMetadata,
                StringComparer.Ordinal)
            {
                ["accepted_candidate_count"] = "0",
                ["training_execution_boundary"] = "zero_intake_queued_for_run_negative_test"
            }
        };
        var badBoundaryIntake = intake.Value with
        {
            IntakeId = "training-intake-bad-boundary-run-blocked",
            TrainingNotStarted = false,
            BoundaryMetadata = new Dictionary<string, string>(
                intake.Value.BoundaryMetadata,
                StringComparer.Ordinal)
            {
                ["training_not_started"] = bool.FalseString,
                ["training_started"] = bool.TrueString
            }
        };
        var unapprovedIntake = intake.Value with
        {
            IntakeId = "training-intake-unapproved-run-blocked",
            ApprovedCandidateCount = 2,
            AcceptedCandidateCount = 2,
            CandidateRefs =
            [
                intake.Value.CandidateRefs.Single(),
                intake.Value.CandidateRefs.Single() with
                {
                    CandidateId = pendingCandidate.CandidateId,
                    TrainingRecordId = pendingCandidate.TrainingRecord.RecordId,
                    SourceManifestRef = "manifest://design-conversion/pass112-pending-run-blocks",
                    SourceManifestHash = "sha256:pass112-pending-run-blocks-manifest",
                    SourceIdentityHash = "sha256:pass112-pending-run-blocks-source"
                }
            ],
            BoundaryMetadata = new Dictionary<string, string>(
                intake.Value.BoundaryMetadata,
                StringComparer.Ordinal)
            {
                ["approved_candidate_count"] = "2",
                ["accepted_candidate_count"] = "2",
                ["source_manifest_status"] = "proven",
                ["source_disjoint_status"] = "proven"
            }
        };
        await File.AppendAllTextAsync(
            intakePath,
            string.Join(
                Environment.NewLine,
                [
                    System.Text.Json.JsonSerializer.Serialize(zeroQueuedIntake),
                    System.Text.Json.JsonSerializer.Serialize(badBoundaryIntake),
                    System.Text.Json.JsonSerializer.Serialize(unapprovedIntake),
                    string.Empty
                ]));

        var emptyRun = await db.RunTrainingIntakeAsync(zeroQueuedIntake.IntakeId);
        var badBoundaryRun = await db.RunTrainingIntakeAsync(badBoundaryIntake.IntakeId);
        var unapprovedRun = await db.RunTrainingIntakeAsync(unapprovedIntake.IntakeId);

        Assert.True(emptyRun.IsSuccess, emptyRun.ErrorMessage);
        Assert.False(emptyRun.Value!.MayStartTraining);
        Assert.False(emptyRun.Value.TrainingRunnerInvoked);
        Assert.False(emptyRun.Value.TrainingStarted);
        Assert.False(emptyRun.Value.CheckpointMutated);
        Assert.False(emptyRun.Value.WeightsProduced);
        Assert.False(emptyRun.Value.ModelUploadReady);
        Assert.Contains("no_accepted_candidates", emptyRun.Value.BlockReasons);
        Assert.Equal("blocked_before_runner_by_training_intake_preflight", emptyRun.Value.TrainingExecutionBoundary);

        Assert.True(badBoundaryRun.IsSuccess, badBoundaryRun.ErrorMessage);
        Assert.False(badBoundaryRun.Value!.TrainingRunnerInvoked);
        Assert.Contains("training_not_started_required", badBoundaryRun.Value.BlockReasons);
        Assert.Contains("boundary_metadata_training_started_must_be_false", badBoundaryRun.Value.BlockReasons);
        Assert.False(badBoundaryRun.Value.TrainingStarted);
        Assert.False(badBoundaryRun.Value.ModelUploadReady);

        Assert.True(unapprovedRun.IsSuccess, unapprovedRun.ErrorMessage);
        Assert.False(unapprovedRun.Value!.MayStartTraining);
        Assert.False(unapprovedRun.Value.TrainingRunnerInvoked);
        Assert.False(unapprovedRun.Value.TrainingStarted);
        Assert.False(unapprovedRun.Value.CheckpointMutated);
        Assert.False(unapprovedRun.Value.WeightsProduced);
        Assert.False(unapprovedRun.Value.ModelUploadReady);
        Assert.Contains(
            "approved_runtime_handoff_candidate_required:runtime-pending-run-blocks",
            unapprovedRun.Value.BlockReasons);
        Assert.Equal("blocked_before_runner_by_training_intake_resolution", unapprovedRun.Value.TrainingExecutionBoundary);
    }

    [Fact]
    public async Task LocalArtifactDb_RunTrainingIntakeAsync_InvokesRunnerForApprovedPreflightWithoutReadinessClaims()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local_artifact_db_training_intake_run_bridge");
        var db = new JsonlDesignConversionLocalArtifactDb(root);
        var approvedRuntime = WithRuntimeArtifactProofRefs(
            ToRuntimeApprovedCandidate(
                WithSourceIdentity(
                    BuildTrainingRecord("runtime-approved-run-bridge-a", DesignConversionDpoUnits.ImproverUnit, usefulness: 0.95),
                    "pass112-source-a")),
            "pass112-run-bridge-train");
        var heldOutApprovedRuntime = WithRuntimeArtifactProofRefs(
            ToRuntimeApprovedCandidate(
                WithSourceIdentity(
                    BuildTrainingRecord("runtime-approved-run-bridge-b", DesignConversionDpoUnits.CriticUnit, usefulness: 0.94),
                    "pass112-source-b")),
            "pass112-run-bridge-heldout");
        await Save(db, BuildArtifactRecord(
            "storage-record-run-bridge",
            "job-run-bridge",
            "request-run-bridge",
            DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            [approvedRuntime, heldOutApprovedRuntime],
            "<main data-user-id=\"user-luba\">exact_prompt private html</main>",
            ".private-user{background:url('C:/Users/private-user/private.png');}") with
        {
            SanitizedSourceRefs = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["source_manifest_ref"] = "manifest://design-conversion/pass112-run-bridge",
                ["source_kind"] = "figma",
                ["design_tree_payload_ref"] = "sha256:design-tree-pass112-run-bridge",
                ["original_tree_payload_ref"] = "sha256:original-tree-pass112-run-bridge",
                ["source_artifact:expected_desktop"] = "sha256:pass112-run-bridge-expected-design-render",
                ["source_artifact:actual_desktop"] = "sha256:pass112-run-bridge-actual-browser-render"
            }
        });
        var api = new DesignConversionDpoUsefulnessApi(db);
        var handoff = await api.ExportApprovedRuntimeDpoCandidatesAsync(
            new DesignConversionLocalArtifactDbQuery(JobId: "job-run-bridge"),
            DateTimeOffset.UnixEpoch.AddMinutes(111));
        var intake = await api.CreateTrainingIntakeAsync(
            handoff.Value!,
            "requester:pass111-local",
            "local runner bridge request; no training readiness claimed",
            DateTimeOffset.UnixEpoch.AddMinutes(111));

        var run = await api.RunTrainingIntakeAsync(intake.Value!.IntakeId);
        var serializedRun = System.Text.Json.JsonSerializer.Serialize(run.Value);

        Assert.True(run.IsSuccess, run.ErrorMessage);
        Assert.StartsWith("training-intake-run-", run.Value!.RunId, StringComparison.Ordinal);
        Assert.Equal(intake.Value.IntakeId, run.Value.TrainingIntakeId);
        Assert.True(run.Value.MayStartTraining);
        Assert.Equal("proven", run.Value.SourceManifestStatus);
        Assert.Equal("proven", run.Value.SourceDisjointStatus);
        Assert.True(run.Value.TrainingRunnerInvoked);
        Assert.Equal(2, run.Value.ResolvedTrainingRecordCount);
        Assert.False(run.Value.LocalLifecycleHookAttempted);
        Assert.False(run.Value.TrainingStarted);
        Assert.False(run.Value.CheckpointMutated);
        Assert.False(run.Value.CheckpointCandidateCreated);
        Assert.False(run.Value.WeightsProduced);
        Assert.False(run.Value.ModelUploadReady);
        Assert.Equal("runner_invoked_count_or_source_manifest_gate_no_training_started", run.Value.TrainingExecutionBoundary);
        Assert.Equal("runner_invoked_training_blocked_dataset_count_gate", run.Value.TrainingExecutionStatus);
        Assert.Equal("dataset_count_gate_not_passed", run.Value.RunnerBlockedReason);
        Assert.Equal("not_proven", run.Value.CheckpointStatus);
        Assert.Equal("not_proven", run.Value.CheckpointMutationStatus);
        Assert.Equal("real_trained_weights_not_proven", run.Value.WeightProofStatus);
        Assert.Equal(DesignConversionMeasurementGateStatus.NotRun, run.Value.MeasurementGateStatus);
        Assert.Equal("PROMOTION_BLOCKED_MEASUREMENT_GATE_NOT_PASSED", run.Value.PromotionGateStatus);
        Assert.Equal("upload_blocked_real_trained_weights_not_proven", run.Value.UploadReadinessStatus);
        Assert.Contains("real_trained_weights_not_proven", run.Value.PromotionBlockedReasons);
        Assert.Contains("measurement_gate_not_passed:not_run", run.Value.PromotionBlockedReasons);
        Assert.Contains(
            "promotion_policy_not_passed:PROMOTION_BLOCKED_MEASUREMENT_GATE_NOT_PASSED",
            run.Value.PromotionBlockedReasons);
        var runnerEvidence = Assert.IsType<DesignConversionTrainingIntakeRunnerEvidence>(run.Value.RunnerEvidence);
        Assert.Equal(DesignConversionTrainingBoundaryDecisionKind.CountGateFailed.ToString(), runnerEvidence.BoundaryDecisionKind);
        Assert.False(runnerEvidence.DatasetCountGatePassed);
        Assert.True(runnerEvidence.SourceDisjoint);
        Assert.Equal("proven", runnerEvidence.SourceManifestStatus);
        Assert.Equal("proven", runnerEvidence.SourceDisjointStatus);
        Assert.False(runnerEvidence.MachineLifecycleHookAttempted);
        Assert.False(runnerEvidence.TrainingStarted);
        Assert.Equal("runner_invoked_training_blocked_dataset_count_gate", runnerEvidence.TrainingExecutionStatus);
        Assert.Equal("dataset_count_gate_not_passed", runnerEvidence.RunnerBlockedReason);
        Assert.Equal("not_proven", runnerEvidence.CheckpointCandidateStatus);
        Assert.False(runnerEvidence.CheckpointContainsRealTrainedWeights);
        Assert.Equal("not_proven", runnerEvidence.CheckpointMutationStatus);
        Assert.False(runnerEvidence.CheckpointMutationEvidenceProven);
        Assert.Equal("real_trained_weights_not_proven", runnerEvidence.WeightProofStatus);
        Assert.False(runnerEvidence.PromotionUploadAllowed);
        Assert.DoesNotContain("source_manifest_preflight_missing", runnerEvidence.FailureReasons);
        Assert.Contains("real_training_not_claimed", run.Value.BlockReasons);
        Assert.Contains("training_not_started", run.Value.BlockReasons);
        Assert.Contains("checkpoint_mutation_not_proven", run.Value.BlockReasons);
        Assert.Contains("real_trained_weights_not_proven", run.Value.BlockReasons);
        Assert.Contains("model_upload_not_ready", run.Value.BlockReasons);
        Assert.Equal(bool.TrueString, run.Value.BoundaryMetadata["may_start_training"]);
        Assert.Equal("proven", run.Value.BoundaryMetadata["source_manifest_status"]);
        Assert.Equal("proven", run.Value.BoundaryMetadata["source_disjoint_status"]);
        Assert.Equal(bool.TrueString, run.Value.BoundaryMetadata["training_runner_invoked"]);
        Assert.Equal(bool.FalseString, run.Value.BoundaryMetadata["training_started"]);
        Assert.Equal(bool.FalseString, run.Value.BoundaryMetadata["checkpoint_mutated"]);
        Assert.Equal(bool.FalseString, run.Value.BoundaryMetadata["weights_produced"]);
        Assert.Equal(bool.FalseString, run.Value.BoundaryMetadata["model_upload_ready"]);
        Assert.Equal("runner_invoked_training_blocked_dataset_count_gate", run.Value.BoundaryMetadata["training_execution_status"]);
        Assert.Equal("dataset_count_gate_not_passed", run.Value.BoundaryMetadata["runner_blocked_reason"]);
        Assert.Equal("not_proven", run.Value.BoundaryMetadata["checkpoint_status"]);
        Assert.Equal("not_proven", run.Value.BoundaryMetadata["checkpoint_mutation_status"]);
        Assert.Equal("real_trained_weights_not_proven", run.Value.BoundaryMetadata["weight_proof_status"]);
        Assert.Equal(DesignConversionMeasurementGateStatus.NotRun, run.Value.BoundaryMetadata["measurement_gate_status"]);
        Assert.Equal("PROMOTION_BLOCKED_MEASUREMENT_GATE_NOT_PASSED", run.Value.BoundaryMetadata["promotion_gate_status"]);
        Assert.Equal("upload_blocked_real_trained_weights_not_proven", run.Value.BoundaryMetadata["upload_readiness_status"]);
        Assert.Contains("measurement_gate_not_passed:not_run", run.Value.BoundaryMetadata["promotion_blocked_reasons"]);
        Assert.Equal(bool.FalseString, run.Value.BoundaryMetadata["promotion_upload_attempted"]);
        Assert.Equal(bool.FalseString, run.Value.BoundaryMetadata["promotion_claimed"]);
        Assert.Contains("training_runner_invoked", run.Value.Warnings);
        Assert.Contains("not_trained", run.Value.Warnings);
        Assert.Contains("weights_not_produced", run.Value.Warnings);
        Assert.Contains("model_upload_not_ready", run.Value.Warnings);
        Assert.DoesNotContain("<main", serializedRun, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".private-user", serializedRun, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/Users/", serializedRun, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("exact_prompt", serializedRun, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("private.png", serializedRun, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("external-provider-payload", serializedRun, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void LocalArtifactDb_PublicConstructorSurface_PreservesFourFiveAndSixParameterQuotaSignatures()
    {
        var dbType = typeof(JsonlDesignConversionLocalArtifactDb);

        Assert.NotNull(dbType.GetConstructor(
            [
                typeof(string),
                typeof(IDesignConversionPromotionUploader),
                typeof(long),
                typeof(long)
            ]));
        Assert.NotNull(dbType.GetConstructor(
            [
                typeof(string),
                typeof(IDesignConversionPromotionUploader),
                typeof(long),
                typeof(long),
                typeof(long)
            ]));
        Assert.NotNull(dbType.GetConstructor(
            [
                typeof(string),
                typeof(IDesignConversionPromotionUploader),
                typeof(long),
                typeof(long),
                typeof(long),
                typeof(long)
            ]));
    }

    [Fact]
    public async Task LocalArtifactDb_RunTrainingIntakeAsync_FailsClosedBeforeRunnerWhenRunJournalCannotReserveMaximumRow()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "local_artifact_db_training_intake_run_quota_reservation");
        var runRecordsPath = Path.Combine(root, "training-intake-runs.jsonl");
        const string privatePathMarker = "C:/Users/private-user/private/run-quota-marker.txt";
        const string privatePayloadMarker = "RUN_QUOTA_PRIVATE_SENTINEL";
        var tenantRef = $"tenant:{privatePathMarker}:{privatePayloadMarker}";
        var idempotencyKey = $"idempotency:{privatePathMarker}:{privatePayloadMarker}";
        var quotaOptions = new DesignConversionTrainingPipelineOptions(
            RunId: "run-private-path-marker",
            BatchSize: 1,
            DatasetOptions: new DesignConversionDpoDatasetOptions(
                DatasetId: "dataset-private-path-marker",
                MinimumTrainRecords: 100,
                MinimumHeldOutRecords: 100,
                Preflight: DesignConversionDpoDatasetPreflight.LocalTrainingIntakeSourceManifestDisjointProof(
                    ["quota-reservation-controlled-source-disjoint-intake"])));

        static async Task<(byte[] Bytes, int Count)> ReadRunJournalStateAsync(string path)
        {
            var bytes = await File.ReadAllBytesAsync(path);
            var count = (await File.ReadAllLinesAsync(path))
                .Count(line => !string.IsNullOrWhiteSpace(line));
            return (bytes, count);
        }

        try
        {
            var setupDb = new JsonlDesignConversionLocalArtifactDb(root);
            var trainApprovedRuntime = WithRuntimeArtifactProofRefs(
                ToRuntimeApprovedCandidate(
                    WithSourceIdentity(
                        BuildTrainingRecord("runtime-approved-run-quota-a", DesignConversionDpoUnits.ImproverUnit, usefulness: 0.96),
                        "run-quota-source-a")),
                "run-quota-train");
            var heldOutApprovedRuntime = WithRuntimeArtifactProofRefs(
                ToRuntimeApprovedCandidate(
                    WithSourceIdentity(
                        BuildTrainingRecord("runtime-approved-run-quota-b", DesignConversionDpoUnits.CriticUnit, usefulness: 0.95),
                        "run-quota-source-b")),
                "run-quota-heldout");
            await Save(setupDb, BuildArtifactRecord(
                "storage-record-run-quota",
                "job-run-quota",
                "request-run-quota",
                DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                [trainApprovedRuntime, heldOutApprovedRuntime],
                "<main data-user-id=\"user-luba\">exact_prompt private html</main>",
                ".private-user{background:url('C:/Users/private-user/private.png');}") with
            {
                SanitizedSourceRefs = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["source_manifest_ref"] = "manifest://design-conversion/run-quota",
                    ["source_kind"] = "figma",
                    ["design_tree_payload_ref"] = "sha256:design-tree-run-quota",
                    ["original_tree_payload_ref"] = "sha256:original-tree-run-quota",
                    ["source_artifact:expected_desktop"] = "sha256:run-quota-expected-design-render",
                    ["source_artifact:actual_desktop"] = "sha256:run-quota-actual-browser-render"
                }
            });
            var handoff = await setupDb.ExportApprovedRuntimeDpoCandidatesAsync(
                new DesignConversionLocalArtifactDbQuery(JobId: "job-run-quota"),
                DateTimeOffset.UnixEpoch.AddMinutes(214));
            Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
            var intake = await setupDb.CreateTrainingIntakeAsync(
                handoff.Value!,
                "requester:run-quota-local",
                "run quota reservation request; no training readiness claimed",
                DateTimeOffset.UnixEpoch.AddMinutes(214));
            Assert.True(intake.IsSuccess, intake.ErrorMessage);

            var seedAdmission = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["training_intake_tenant_id_ref"] = "tenant:run-quota-seed",
                ["training_intake_idempotency_key_ref"] = "idempotency:run-quota-seed"
            };
            var seed = await setupDb.RunTrainingIntakeAsync(
                intake.Value!.IntakeId,
                quotaOptions,
                admissionBoundaryMetadata: seedAdmission);
            Assert.True(seed.IsSuccess, seed.ErrorMessage);
            Assert.True(seed.Value!.TrainingRunnerInvoked);
            Assert.False(seed.Value.LocalLifecycleHookAttempted);
            Assert.False(seed.Value.CheckpointMutated);
            Assert.False(seed.Value.WeightsProduced);

            var seedState = await ReadRunJournalStateAsync(runRecordsPath);
            Assert.Equal(1, seedState.Count);
            var snapshotBeforeQuota = await ReadFileSha256SnapshotAsync(root);
            Assert.Empty(EnumerateCopyOnWriteTempFiles(root));

            var lowQuota = seedState.Bytes.LongLength
                + JsonlDesignConversionLocalArtifactDb.MaxTrainingIntakeRunJournalRowByteLength
                - 1;
            var runnerInvocations = 0;
            var quotaAdmission = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["training_intake_tenant_id_ref"] = tenantRef,
                ["training_intake_idempotency_key_ref"] = idempotencyKey,
                ["training_intake_private_path_marker_ref"] = privatePathMarker,
                ["training_intake_private_payload_marker_ref"] = privatePayloadMarker
            };
            var quotaDb = new JsonlDesignConversionLocalArtifactDb(
                root,
                createTrainingIntakeJournalWaitTaskObserverForTesting: null,
                createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
                trainingIntakeRunsMaxBytes: lowQuota,
                runTrainingIntakeRunnerBeforeInvokeObserverForTesting: () => Interlocked.Increment(ref runnerInvocations));

            var quota = await quotaDb.RunTrainingIntakeAsync(
                intake.Value.IntakeId,
                quotaOptions,
                admissionBoundaryMetadata: quotaAdmission);

            Assert.Equal(0, runnerInvocations);
            Assert.False(quota.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_TRAINING_INTAKE_RUNS_QUOTA_EXCEEDED", quota.ErrorCode);
            Assert.Equal("DesignConversion training intake run journal quota is exhausted.", quota.ErrorMessage);
            var quotaErrorEnvelope = System.Text.Json.JsonSerializer.Serialize(
                new { quota.ErrorCode, quota.ErrorMessage });
            var quotaErrorText = string.Join('|', quota.ErrorCode, quota.ErrorMessage, quotaErrorEnvelope);
            foreach (var forbidden in new[]
                     {
                         root,
                         intake.Value.IntakeId,
                         privatePathMarker,
                         privatePayloadMarker,
                         tenantRef,
                         idempotencyKey,
                         quotaOptions.RunId,
                         "IOException",
                         "UnauthorizedAccessException",
                         "Exception"
                     })
            {
                Assert.DoesNotContain(forbidden, quotaErrorText, StringComparison.OrdinalIgnoreCase);
            }

            var failedState = await ReadRunJournalStateAsync(runRecordsPath);
            Assert.Equal(seedState.Count, failedState.Count);
            Assert.Equal(seedState.Bytes, failedState.Bytes);
            Assert.Equal(snapshotBeforeQuota, await ReadFileSha256SnapshotAsync(root));
            Assert.Empty(EnumerateCopyOnWriteTempFiles(root));
            Assert.DoesNotContain(
                await ReadFileSha256SnapshotAsync(root),
                pair => pair.Key.Contains("checkpoint", StringComparison.OrdinalIgnoreCase)
                        || pair.Key.Contains("artifact", StringComparison.OrdinalIgnoreCase));

            var capacityAtReservation = seedState.Bytes.LongLength
                + JsonlDesignConversionLocalArtifactDb.MaxTrainingIntakeRunJournalRowByteLength;
            var appendDb = new JsonlDesignConversionLocalArtifactDb(
                root,
                createTrainingIntakeJournalWaitTaskObserverForTesting: null,
                createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
                trainingIntakeRunsMaxBytes: capacityAtReservation,
                runTrainingIntakeRunnerBeforeInvokeObserverForTesting: () => Interlocked.Increment(ref runnerInvocations));
            var appended = await appendDb.RunTrainingIntakeAsync(
                intake.Value.IntakeId,
                quotaOptions,
                admissionBoundaryMetadata: quotaAdmission);
            var appendedState = await ReadRunJournalStateAsync(runRecordsPath);

            Assert.True(appended.IsSuccess, appended.ErrorMessage);
            Assert.True(appended.Value!.TrainingRunnerInvoked);
            Assert.Equal(1, runnerInvocations);
            Assert.False(appended.Value!.LocalLifecycleHookAttempted);
            Assert.False(appended.Value.CheckpointMutated);
            Assert.False(appended.Value.WeightsProduced);
            Assert.Equal(seedState.Count + 1, appendedState.Count);
            Assert.True(appendedState.Bytes.LongLength > seedState.Bytes.LongLength);
            Assert.Empty(EnumerateCopyOnWriteTempFiles(root));

            var replayDb = new JsonlDesignConversionLocalArtifactDb(
                root,
                createTrainingIntakeJournalWaitTaskObserverForTesting: null,
                createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
                trainingIntakeRunsMaxBytes: appendedState.Bytes.LongLength - 1,
                runTrainingIntakeRunnerBeforeInvokeObserverForTesting: () => Interlocked.Increment(ref runnerInvocations));
            var replay = await replayDb.RunTrainingIntakeAsync(
                intake.Value.IntakeId,
                quotaOptions,
                admissionBoundaryMetadata: quotaAdmission);
            var replayState = await ReadRunJournalStateAsync(runRecordsPath);

            Assert.True(replay.IsSuccess, replay.ErrorMessage);
            Assert.Equal(1, runnerInvocations);
            Assert.Equal(bool.TrueString, replay.Value!.BoundaryMetadata["training_intake_idempotency_replay_hit"]);
            Assert.Equal(bool.FalseString, replay.Value.BoundaryMetadata["training_intake_idempotency_created_new_side_effect"]);
            Assert.Equal(appendedState.Count, replayState.Count);
            Assert.Equal(appendedState.Bytes, replayState.Bytes);
            Assert.Empty(EnumerateCopyOnWriteTempFiles(root));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LocalArtifactDb_RunTrainingIntakeAndPromoteLocalAsync_FailsClosedBeforeRunnerWhenLocalPromotionJournalCannotReserveMaximumRow()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "local_artifact_db_training_intake_local_promotion_quota_reservation");
        var promotionRecordsPath = Path.Combine(root, "training-intake-local-promotions.jsonl");
        var seedOutputDirectory = Path.Combine(root, "seed-promotion-output");
        var quotaOutputDirectory = Path.Combine(root, "quota-promotion-output");
        const string privatePathMarker = "C:/Users/private-user/private/local-promotion-quota-marker.txt";
        const string privatePayloadMarker = "LOCAL_PROMOTION_QUOTA_PRIVATE_SENTINEL";
        var tenantRef = $"tenant:{privatePathMarker}:{privatePayloadMarker}";
        var idempotencyKey = $"idempotency:{privatePathMarker}:{privatePayloadMarker}";

        static async Task<(byte[] Bytes, int Count)> ReadPromotionJournalStateAsync(string path)
        {
            var bytes = await File.ReadAllBytesAsync(path);
            var count = (await File.ReadAllLinesAsync(path))
                .Count(line => !string.IsNullOrWhiteSpace(line));
            return (bytes, count);
        }

        try
        {
            var setupDb = new JsonlDesignConversionLocalArtifactDb(root, new CountingLocalOnlyPromotionUploader());
            var setupApi = new DesignConversionDpoUsefulnessApi(setupDb);
            var trainApprovedRuntime = WithRuntimeArtifactProofRefs(
                ToRuntimeApprovedCandidate(
                    WithSourceIdentity(
                        BuildTrainingRecord("runtime-approved-local-promotion-quota-a", DesignConversionDpoUnits.ImproverUnit, usefulness: 0.96),
                        "local-promotion-quota-source-a")),
                "local-promotion-quota-train");
            var heldOutApprovedRuntime = WithRuntimeArtifactProofRefs(
                ToRuntimeApprovedCandidate(
                    WithSourceIdentity(
                        BuildTrainingRecord("runtime-approved-local-promotion-quota-b", DesignConversionDpoUnits.CriticUnit, usefulness: 0.95),
                        "local-promotion-quota-source-b")),
                "local-promotion-quota-heldout");
            await Save(setupDb, BuildArtifactRecord(
                "storage-record-local-promotion-quota",
                "job-local-promotion-quota",
                "request-local-promotion-quota",
                DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                [trainApprovedRuntime, heldOutApprovedRuntime],
                "<main data-user-id=\"user-luba\">exact_prompt private html</main>",
                ".private-user{background:url('C:/Users/private-user/private.png');}") with
            {
                SanitizedSourceRefs = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["source_manifest_ref"] = "manifest://design-conversion/local-promotion-quota",
                    ["source_kind"] = "figma",
                    ["design_tree_payload_ref"] = "sha256:design-tree-local-promotion-quota",
                    ["original_tree_payload_ref"] = "sha256:original-tree-local-promotion-quota",
                    ["source_artifact:expected_desktop"] = "sha256:local-promotion-quota-expected-design-render",
                    ["source_artifact:actual_desktop"] = "sha256:local-promotion-quota-actual-browser-render"
                }
            });
            var handoff = await setupApi.ExportApprovedRuntimeDpoCandidatesAsync(
                new DesignConversionLocalArtifactDbQuery(JobId: "job-local-promotion-quota"),
                DateTimeOffset.UnixEpoch.AddMinutes(216));
            Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
            var intake = await setupApi.CreateTrainingIntakeAsync(
                handoff.Value!,
                "requester:local-promotion-quota",
                "local promotion quota reservation request; no training readiness claimed",
                DateTimeOffset.UnixEpoch.AddMinutes(216));
            Assert.True(intake.IsSuccess, intake.ErrorMessage);
            var intakeRecord = intake.Value!;
            var seedOptions = Pass157LocalPromotionOptions(
                "local-promotion-quota-seed",
                BuildPassingMeasurementGateRequest("local-promotion-quota-seed-gate"));
            var quotaOptions = Pass157LocalPromotionOptions(
                "local-promotion-quota-second",
                BuildPassingMeasurementGateRequest("local-promotion-quota-second-gate"));

            var seedAdmission = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["training_intake_tenant_id_ref"] = "tenant:local-promotion-quota-seed",
                ["training_intake_idempotency_key_ref"] = "idempotency:local-promotion-quota-seed"
            };
            var seed = await DesignConversionTrainingIntakeRouteAdmissionContext.RunAsync(
                seedAdmission,
                () => setupApi.RunTrainingIntakeAndPromoteLocalAsync(
                    intakeRecord.IntakeId,
                    seedOutputDirectory,
                    seedOptions));
            Assert.True(seed.IsSuccess, seed.ErrorMessage);
            Assert.True(seed.Value!.LocalPromotionReady);
            Assert.True(seed.Value.Publication!.Manifest.LocalPromotionReceiptWritten);

            var seedState = await ReadPromotionJournalStateAsync(promotionRecordsPath);
            Assert.Equal(1, seedState.Count);
            var snapshotBeforeQuota = await ReadFileSha256SnapshotAsync(root);
            var seedCowTempFiles = EnumerateCopyOnWriteTempFiles(root);
            Assert.Empty(seedCowTempFiles);
            Assert.False(Directory.Exists(quotaOutputDirectory));

            var lowQuota = seedState.Bytes.LongLength
                + JsonlDesignConversionLocalArtifactDb.MaxTrainingIntakeLocalPromotionJournalRowByteLength
                - 1;
            var runnerBeforeInvokeCount = 0;
            var countingUploader = new CountingLocalOnlyPromotionUploader();
            var quotaAdmission = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["training_intake_tenant_id_ref"] = tenantRef,
                ["training_intake_idempotency_key_ref"] = idempotencyKey,
                ["training_intake_private_path_marker_ref"] = privatePathMarker,
                ["training_intake_private_payload_marker_ref"] = privatePayloadMarker
            };
            var quotaDb = new JsonlDesignConversionLocalArtifactDb(
                root,
                createTrainingIntakeJournalWaitTaskObserverForTesting: null,
                createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
                promotionUploader: countingUploader,
                trainingIntakeLocalPromotionsMaxBytes: lowQuota,
                runTrainingIntakeLocalPromotionRunnerBeforeInvokeObserverForTesting:
                    () => Interlocked.Increment(ref runnerBeforeInvokeCount));

            var quota = await DesignConversionTrainingIntakeRouteAdmissionContext.RunAsync(
                quotaAdmission,
                () => new DesignConversionDpoUsefulnessApi(quotaDb).RunTrainingIntakeAndPromoteLocalAsync(
                    intakeRecord.IntakeId,
                    quotaOutputDirectory,
                    quotaOptions));

            Assert.Equal(0, runnerBeforeInvokeCount);
            Assert.Equal(0, countingUploader.Count);
            Assert.False(quota.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_TRAINING_INTAKE_LOCAL_PROMOTIONS_QUOTA_EXCEEDED", quota.ErrorCode);
            Assert.Equal(
                "DesignConversion training intake local promotion journal quota is exhausted.",
                quota.ErrorMessage);
            var quotaErrorEnvelope = System.Text.Json.JsonSerializer.Serialize(
                new { quota.ErrorCode, quota.ErrorMessage });
            var quotaErrorText = string.Join('|', quota.ErrorCode, quota.ErrorMessage, quotaErrorEnvelope);
            foreach (var forbidden in new[]
                     {
                         root,
                         intakeRecord.IntakeId,
                         privatePathMarker,
                         privatePayloadMarker,
                         tenantRef,
                         idempotencyKey,
                         quotaOptions.RunId,
                         "IOException",
                         "UnauthorizedAccessException",
                         "Exception"
                     })
            {
                Assert.DoesNotContain(forbidden, quotaErrorText, StringComparison.OrdinalIgnoreCase);
            }

            Assert.False(Directory.Exists(quotaOutputDirectory));
            var failedState = await ReadPromotionJournalStateAsync(promotionRecordsPath);
            Assert.Equal(seedState.Count, failedState.Count);
            Assert.Equal(seedState.Bytes, failedState.Bytes);
            Assert.Equal(snapshotBeforeQuota, await ReadFileSha256SnapshotAsync(root));
            Assert.Empty(EnumerateCopyOnWriteTempFiles(root));

            var capacityAtReservation = seedState.Bytes.LongLength
                + JsonlDesignConversionLocalArtifactDb.MaxTrainingIntakeLocalPromotionJournalRowByteLength;
            var appendDb = new JsonlDesignConversionLocalArtifactDb(
                root,
                createTrainingIntakeJournalWaitTaskObserverForTesting: null,
                createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
                promotionUploader: countingUploader,
                trainingIntakeLocalPromotionsMaxBytes: capacityAtReservation,
                runTrainingIntakeLocalPromotionRunnerBeforeInvokeObserverForTesting:
                    () => Interlocked.Increment(ref runnerBeforeInvokeCount));
            var appended = await DesignConversionTrainingIntakeRouteAdmissionContext.RunAsync(
                quotaAdmission,
                () => new DesignConversionDpoUsefulnessApi(appendDb).RunTrainingIntakeAndPromoteLocalAsync(
                    intakeRecord.IntakeId,
                    quotaOutputDirectory,
                    quotaOptions));
            var appendedState = await ReadPromotionJournalStateAsync(promotionRecordsPath);
            var appendedArtifactSnapshot = await ReadFileSha256SnapshotAsync(quotaOutputDirectory);

            Assert.True(appended.IsSuccess, appended.ErrorMessage);
            Assert.True(appended.Value!.LocalPromotionReady);
            Assert.NotNull(appended.Value.PromotedLocalModel);
            Assert.True(appended.Value.Publication!.Manifest.LocalPromotionReceiptWritten);
            Assert.Equal(1, runnerBeforeInvokeCount);
            Assert.Equal(1, countingUploader.Count);
            Assert.Equal(seedState.Count + 1, appendedState.Count);
            Assert.True(appendedState.Bytes.LongLength > seedState.Bytes.LongLength);
            Assert.Single(appendedArtifactSnapshot, artifact => artifact.Key.EndsWith("-promoted-checkpoint.json", StringComparison.Ordinal));
            Assert.Single(appendedArtifactSnapshot, artifact => artifact.Key.EndsWith("-tokenizer.json", StringComparison.Ordinal));
            Assert.Single(appendedArtifactSnapshot, artifact => artifact.Key.EndsWith("-policy.json", StringComparison.Ordinal));
            Assert.DoesNotContain(appendedArtifactSnapshot, artifact => artifact.Key.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase));
            Assert.Empty(EnumerateCopyOnWriteTempFiles(root));

            var replayRootSnapshotBefore = await ReadFileSha256SnapshotAsync(root);
            var replayDb = new JsonlDesignConversionLocalArtifactDb(
                root,
                createTrainingIntakeJournalWaitTaskObserverForTesting: null,
                createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
                promotionUploader: countingUploader,
                trainingIntakeLocalPromotionsMaxBytes: appendedState.Bytes.LongLength - 1,
                runTrainingIntakeLocalPromotionRunnerBeforeInvokeObserverForTesting:
                    () => Interlocked.Increment(ref runnerBeforeInvokeCount));
            var replay = await DesignConversionTrainingIntakeRouteAdmissionContext.RunAsync(
                quotaAdmission,
                () => new DesignConversionDpoUsefulnessApi(replayDb).RunTrainingIntakeAndPromoteLocalAsync(
                    intakeRecord.IntakeId,
                    quotaOutputDirectory,
                    quotaOptions));
            var replayState = await ReadPromotionJournalStateAsync(promotionRecordsPath);
            var replayArtifactSnapshot = await ReadFileSha256SnapshotAsync(quotaOutputDirectory);

            Assert.True(replay.IsSuccess, replay.ErrorMessage);
            Assert.Equal(1, runnerBeforeInvokeCount);
            Assert.Equal(1, countingUploader.Count);
            Assert.Equal(bool.TrueString, replay.Value!.BoundaryMetadata["training_intake_idempotency_replay_hit"]);
            Assert.Equal(bool.FalseString, replay.Value.BoundaryMetadata["training_intake_idempotency_created_new_side_effect"]);
            Assert.Equal(appendedState.Count, replayState.Count);
            Assert.Equal(appendedState.Bytes, replayState.Bytes);
            Assert.Equal(appendedArtifactSnapshot, replayArtifactSnapshot);
            Assert.Equal(replayRootSnapshotBefore, await ReadFileSha256SnapshotAsync(root));
            Assert.Empty(EnumerateCopyOnWriteTempFiles(root));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LocalArtifactDb_RunTrainingIntakeAsync_StartsLocalLifecycleWithRealWeightsWithoutUploadClaims()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local_artifact_db_training_intake_lifecycle_start");
        var db = new JsonlDesignConversionLocalArtifactDb(root);
        var trainApprovedRuntime = WithRuntimeArtifactProofRefs(
            ToRuntimeApprovedCandidate(
                WithSourceIdentity(
                    BuildTrainingRecord("runtime-approved-lifecycle-start-a", DesignConversionDpoUnits.ImproverUnit, usefulness: 0.96),
                    "pass113-lifecycle-source-a")),
            "pass113-lifecycle-start-train");
        var heldOutApprovedRuntime = WithRuntimeArtifactProofRefs(
            ToRuntimeApprovedCandidate(
                WithSourceIdentity(
                    BuildTrainingRecord("runtime-approved-lifecycle-start-b", DesignConversionDpoUnits.CriticUnit, usefulness: 0.95),
                    "pass113-lifecycle-source-b")),
            "pass113-lifecycle-start-heldout");
        await Save(db, BuildArtifactRecord(
            "storage-record-lifecycle-start",
            "job-lifecycle-start",
            "request-lifecycle-start",
            DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            [trainApprovedRuntime, heldOutApprovedRuntime],
            "<main data-user-id=\"user-luba\">exact_prompt private html</main>",
            ".private-user{background:url('C:/Users/private-user/private.png');}") with
        {
            SanitizedSourceRefs = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["source_manifest_ref"] = "manifest://design-conversion/pass113-lifecycle-start",
                ["source_kind"] = "figma",
                ["design_tree_payload_ref"] = "sha256:design-tree-pass113-lifecycle-start",
                ["original_tree_payload_ref"] = "sha256:original-tree-pass113-lifecycle-start",
                ["source_artifact:expected_desktop"] = "sha256:pass113-lifecycle-start-expected-design-render",
                ["source_artifact:actual_desktop"] = "sha256:pass113-lifecycle-start-actual-browser-render"
            }
        });
        var handoff = await db.ExportApprovedRuntimeDpoCandidatesAsync(
            new DesignConversionLocalArtifactDbQuery(JobId: "job-lifecycle-start"),
            DateTimeOffset.UnixEpoch.AddMinutes(113));
        var intake = await db.CreateTrainingIntakeAsync(
            handoff.Value!,
            "requester:pass113-local",
            "local lifecycle start semantics; no real weights or upload claimed",
            DateTimeOffset.UnixEpoch.AddMinutes(113));
        var options = new DesignConversionTrainingPipelineOptions(
            RunId: "pass113-local-lifecycle-start",
            BatchSize: 1,
            DatasetOptions: new DesignConversionDpoDatasetOptions(
                DatasetId: "pass113-local-lifecycle-start",
                MinimumTrainRecords: 1,
                MinimumHeldOutRecords: 1,
                Preflight: DesignConversionDpoDatasetPreflight.LocalTrainingIntakeSourceManifestDisjointProof(
                    ["pass113_controlled_source_disjoint_intake"])));

        var run = await db.RunTrainingIntakeAsync(intake.Value!.IntakeId, options);
        var serializedRun = System.Text.Json.JsonSerializer.Serialize(run.Value);

        Assert.True(run.IsSuccess, run.ErrorMessage);
        Assert.True(run.Value!.MayStartTraining);
        Assert.Equal("proven", run.Value.SourceManifestStatus);
        Assert.Equal("proven", run.Value.SourceDisjointStatus);
        Assert.True(run.Value.TrainingRunnerInvoked);
        Assert.True(run.Value.LocalLifecycleHookAttempted);
        Assert.True(run.Value.TrainingStarted);
        Assert.Equal("local_lifecycle_training_started_with_real_learned_weights", run.Value.TrainingExecutionStatus);
        Assert.Equal("none", run.Value.RunnerBlockedReason);
        Assert.True(run.Value.CheckpointCandidateCreated);
        Assert.Equal("local_checkpoint_candidate_with_real_learned_weights", run.Value.CheckpointStatus);
        Assert.True(run.Value.CheckpointMutated);
        Assert.Equal("local_continue_checkpoint_mutated_with_real_learned_weights", run.Value.CheckpointMutationStatus);
        Assert.True(run.Value.WeightsProduced);
        Assert.Equal("real_trained_weights_proven", run.Value.WeightProofStatus);
        Assert.False(run.Value.ModelUploadReady);
        Assert.Equal(DesignConversionMeasurementGateStatus.NotRun, run.Value.MeasurementGateStatus);
        Assert.Equal("PROMOTION_BLOCKED_MEASUREMENT_GATE_NOT_PASSED", run.Value.PromotionGateStatus);
        Assert.Equal("upload_blocked_measurement_gate_not_passed", run.Value.UploadReadinessStatus);
        Assert.DoesNotContain("real_trained_weights_not_proven", run.Value.PromotionBlockedReasons);
        Assert.Contains("measurement_gate_not_passed:not_run", run.Value.PromotionBlockedReasons);
        Assert.Contains(
            "promotion_policy_not_passed:PROMOTION_BLOCKED_MEASUREMENT_GATE_NOT_PASSED",
            run.Value.PromotionBlockedReasons);
        Assert.Equal("runner_invoked_local_lifecycle_real_weight_artifact_no_external_training_claim", run.Value.TrainingExecutionBoundary);

        var runnerEvidence = Assert.IsType<DesignConversionTrainingIntakeRunnerEvidence>(run.Value.RunnerEvidence);
        Assert.True(runnerEvidence.DatasetCountGatePassed);
        Assert.True(runnerEvidence.MachineLifecycleHookAttempted);
        Assert.True(runnerEvidence.TrainingStarted);
        Assert.Equal("local_lifecycle_training_started_with_real_learned_weights", runnerEvidence.TrainingExecutionStatus);
        Assert.Equal("none", runnerEvidence.RunnerBlockedReason);
        Assert.True(runnerEvidence.CheckpointCandidateCreated);
        Assert.Equal("local_continue_checkpoint_mutated_with_real_learned_weights", runnerEvidence.CheckpointMutationStatus);
        Assert.True(runnerEvidence.CheckpointMutationEvidenceProven);
        Assert.True(runnerEvidence.CheckpointContainsRealTrainedWeights);
        Assert.Equal("real_trained_weights_proven", runnerEvidence.WeightProofStatus);
        Assert.False(runnerEvidence.PromotionUploadAllowed);
        Assert.Contains("real_training_not_claimed", run.Value.BlockReasons);
        Assert.DoesNotContain("training_not_started", run.Value.BlockReasons);
        Assert.DoesNotContain("checkpoint_mutation_not_proven", run.Value.BlockReasons);
        Assert.DoesNotContain("real_trained_weights_not_proven", run.Value.BlockReasons);
        Assert.Contains("model_upload_not_ready", run.Value.BlockReasons);
        Assert.Equal(bool.TrueString, run.Value.BoundaryMetadata["training_started"]);
        Assert.Equal(bool.TrueString, run.Value.BoundaryMetadata["checkpoint_mutated"]);
        Assert.Equal(bool.TrueString, run.Value.BoundaryMetadata["checkpoint_contains_real_trained_weights"]);
        Assert.Equal(bool.TrueString, run.Value.BoundaryMetadata["weights_produced"]);
        Assert.Equal(bool.FalseString, run.Value.BoundaryMetadata["model_upload_ready"]);
        Assert.Equal("local_lifecycle_training_started_with_real_learned_weights", run.Value.BoundaryMetadata["training_execution_status"]);
        Assert.Equal("none", run.Value.BoundaryMetadata["runner_blocked_reason"]);
        Assert.Equal("local_checkpoint_candidate_with_real_learned_weights", run.Value.BoundaryMetadata["checkpoint_status"]);
        Assert.Equal("local_continue_checkpoint_mutated_with_real_learned_weights", run.Value.BoundaryMetadata["checkpoint_mutation_status"]);
        Assert.Equal("real_trained_weights_proven", run.Value.BoundaryMetadata["weight_proof_status"]);
        Assert.Equal(DesignConversionMeasurementGateStatus.NotRun, run.Value.BoundaryMetadata["measurement_gate_status"]);
        Assert.Equal("PROMOTION_BLOCKED_MEASUREMENT_GATE_NOT_PASSED", run.Value.BoundaryMetadata["promotion_gate_status"]);
        Assert.Equal("upload_blocked_measurement_gate_not_passed", run.Value.BoundaryMetadata["upload_readiness_status"]);
        Assert.Contains("measurement_gate_not_passed:not_run", run.Value.BoundaryMetadata["promotion_blocked_reasons"]);
        Assert.Contains("training_started_local_lifecycle_real_weight_artifact_only", run.Value.Warnings);
        Assert.DoesNotContain("not_trained", run.Value.Warnings);
        Assert.DoesNotContain("weights_not_produced", run.Value.Warnings);
        Assert.Contains("model_upload_not_ready", run.Value.Warnings);
        Assert.DoesNotContain("<main", serializedRun, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".private-user", serializedRun, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/Users/", serializedRun, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("exact_prompt", serializedRun, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("private.png", serializedRun, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LocalArtifactDb_RunTrainingIntakeAsync_BlocksUploadWhenMeasurementGateFailsAfterRealWeights()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local_artifact_db_training_intake_measurement_fail");
        var db = new JsonlDesignConversionLocalArtifactDb(root);
        var trainApprovedRuntime = WithRuntimeArtifactProofRefs(
            ToRuntimeApprovedCandidate(
                WithSourceIdentity(
                    BuildTrainingRecord("runtime-approved-measurement-fail-a", DesignConversionDpoUnits.ImproverUnit, usefulness: 0.96),
                    "pass115-measurement-fail-source-a")),
            "pass115-measurement-fail-train");
        var heldOutApprovedRuntime = WithRuntimeArtifactProofRefs(
            ToRuntimeApprovedCandidate(
                WithSourceIdentity(
                    BuildTrainingRecord("runtime-approved-measurement-fail-b", DesignConversionDpoUnits.CriticUnit, usefulness: 0.95),
                    "pass115-measurement-fail-source-b")),
            "pass115-measurement-fail-heldout");
        await Save(db, BuildArtifactRecord(
            "storage-record-measurement-fail",
            "job-measurement-fail",
            "request-measurement-fail",
            DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            [trainApprovedRuntime, heldOutApprovedRuntime],
            "<main data-user-id=\"user-luba\">exact_prompt private html</main>",
            ".private-user{background:url('C:/Users/private-user/private.png');}") with
        {
            SanitizedSourceRefs = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["source_manifest_ref"] = "manifest://design-conversion/pass115-measurement-fail",
                ["source_kind"] = "figma",
                ["design_tree_payload_ref"] = "sha256:design-tree-pass115-measurement-fail",
                ["original_tree_payload_ref"] = "sha256:original-tree-pass115-measurement-fail",
                ["source_artifact:expected_desktop"] = "sha256:pass115-measurement-fail-expected-design-render",
                ["source_artifact:actual_desktop"] = "sha256:pass115-measurement-fail-actual-browser-render"
            }
        });
        var handoff = await db.ExportApprovedRuntimeDpoCandidatesAsync(
            new DesignConversionLocalArtifactDbQuery(JobId: "job-measurement-fail"),
            DateTimeOffset.UnixEpoch.AddMinutes(115));
        var intake = await db.CreateTrainingIntakeAsync(
            handoff.Value!,
            "requester:pass115-local",
            "local lifecycle measurement fail blocks upload",
            DateTimeOffset.UnixEpoch.AddMinutes(115));
        var options = new DesignConversionTrainingPipelineOptions(
            RunId: "pass115-local-measurement-fail",
            BatchSize: 1,
            DatasetOptions: new DesignConversionDpoDatasetOptions(
                DatasetId: "pass115-local-measurement-fail",
                MinimumTrainRecords: 1,
                MinimumHeldOutRecords: 1,
                Preflight: DesignConversionDpoDatasetPreflight.LocalTrainingIntakeSourceManifestDisjointProof(
                    ["pass115_controlled_source_disjoint_intake"])),
            MeasurementGateRequest: BuildFailingMeasurementGateRequest("pass115-measurement-fail"));

        var run = await db.RunTrainingIntakeAsync(intake.Value!.IntakeId, options);
        var serializedRun = System.Text.Json.JsonSerializer.Serialize(run.Value);

        Assert.True(run.IsSuccess, run.ErrorMessage);
        Assert.True(run.Value!.WeightsProduced);
        Assert.False(run.Value.ModelUploadReady);
        Assert.Equal(DesignConversionMeasurementGateStatus.Failed, run.Value.MeasurementGateStatus);
        Assert.Equal("PROMOTION_BLOCKED_MEASUREMENT_GATE_NOT_PASSED", run.Value.PromotionGateStatus);
        Assert.Equal("upload_blocked_measurement_gate_not_passed", run.Value.UploadReadinessStatus);
        Assert.DoesNotContain("real_trained_weights_not_proven", run.Value.PromotionBlockedReasons);
        Assert.Contains("measurement_gate_not_passed:failed", run.Value.PromotionBlockedReasons);
        Assert.Contains(
            "promotion_policy_not_passed:PROMOTION_BLOCKED_MEASUREMENT_GATE_NOT_PASSED",
            run.Value.PromotionBlockedReasons);
        Assert.Contains("HELD_OUT_QUALITY_SCORE_BELOW_THRESHOLD:0.81<0.9", run.Value.BlockReasons);
        Assert.Contains("model_upload_not_ready", run.Value.BlockReasons);
        Assert.Equal(DesignConversionMeasurementGateStatus.Failed, run.Value.BoundaryMetadata["measurement_gate_status"]);
        Assert.Equal("upload_blocked_measurement_gate_not_passed", run.Value.BoundaryMetadata["upload_readiness_status"]);
        Assert.Contains("measurement_gate_not_passed:failed", run.Value.BoundaryMetadata["promotion_blocked_reasons"]);
        Assert.Contains("promotion_blocked_reason:measurement_gate_not_passed:failed", run.Value.Warnings);
        Assert.DoesNotContain("<main", serializedRun, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".private-user", serializedRun, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/Users/", serializedRun, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("exact_prompt", serializedRun, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("private.png", serializedRun, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LocalArtifactDb_RunTrainingIntakeAsync_AllowsLocalUploadReadinessOnlyAfterMeasurementAndPromotionGatePass()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local_artifact_db_training_intake_measurement_pass");
        var db = new JsonlDesignConversionLocalArtifactDb(root);
        var trainApprovedRuntime = WithRuntimeArtifactProofRefs(
            ToRuntimeApprovedCandidate(
                WithSourceIdentity(
                    BuildTrainingRecord("runtime-approved-measurement-pass-a", DesignConversionDpoUnits.ImproverUnit, usefulness: 0.96),
                    "pass115-measurement-pass-source-a")),
            "pass115-measurement-pass-train");
        var heldOutApprovedRuntime = WithRuntimeArtifactProofRefs(
            ToRuntimeApprovedCandidate(
                WithSourceIdentity(
                    BuildTrainingRecord("runtime-approved-measurement-pass-b", DesignConversionDpoUnits.CriticUnit, usefulness: 0.95),
                    "pass115-measurement-pass-source-b")),
            "pass115-measurement-pass-heldout");
        await Save(db, BuildArtifactRecord(
            "storage-record-measurement-pass",
            "job-measurement-pass",
            "request-measurement-pass",
            DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            [trainApprovedRuntime, heldOutApprovedRuntime],
            "<main data-user-id=\"user-luba\">exact_prompt private html</main>",
            ".private-user{background:url('C:/Users/private-user/private.png');}") with
        {
            SanitizedSourceRefs = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["source_manifest_ref"] = "manifest://design-conversion/pass115-measurement-pass",
                ["source_kind"] = "figma",
                ["design_tree_payload_ref"] = "sha256:design-tree-pass115-measurement-pass",
                ["original_tree_payload_ref"] = "sha256:original-tree-pass115-measurement-pass",
                ["source_artifact:expected_desktop"] = "sha256:pass115-measurement-pass-expected-design-render",
                ["source_artifact:actual_desktop"] = "sha256:pass115-measurement-pass-actual-browser-render"
            }
        });
        var handoff = await db.ExportApprovedRuntimeDpoCandidatesAsync(
            new DesignConversionLocalArtifactDbQuery(JobId: "job-measurement-pass"),
            DateTimeOffset.UnixEpoch.AddMinutes(115));
        var intake = await db.CreateTrainingIntakeAsync(
            handoff.Value!,
            "requester:pass115-local",
            "local lifecycle measurement pass allows local readiness only",
            DateTimeOffset.UnixEpoch.AddMinutes(115));
        Assert.True(intake.IsSuccess, intake.ErrorMessage);
        var options = new DesignConversionTrainingPipelineOptions(
            RunId: "pass115-local-measurement-pass",
            BatchSize: 1,
            DatasetOptions: new DesignConversionDpoDatasetOptions(
                DatasetId: "pass115-local-measurement-pass",
                MinimumTrainRecords: 1,
                MinimumHeldOutRecords: 1,
                Preflight: DesignConversionDpoDatasetPreflight.LocalTrainingIntakeSourceManifestDisjointProof(
                    ["pass115_controlled_source_disjoint_intake"])),
            MeasurementGateRequest: BuildPassingMeasurementGateRequest("pass115-measurement-pass"));

        var run = await db.RunTrainingIntakeAsync(intake.Value!.IntakeId, options);
        var serializedRun = System.Text.Json.JsonSerializer.Serialize(run.Value);

        Assert.True(run.IsSuccess, run.ErrorMessage);
        Assert.True(run.Value!.WeightsProduced);
        Assert.True(run.Value.ModelUploadReady);
        Assert.Equal(DesignConversionMeasurementGateStatus.Passed, run.Value.MeasurementGateStatus);
        Assert.Equal("PROMOTION_ALLOWED_BY_MEASUREMENT_GATE_AND_REAL_WEIGHTS", run.Value.PromotionGateStatus);
        Assert.Equal("upload_ready_local_promotion_gate_passed_no_external_upload", run.Value.UploadReadinessStatus);
        Assert.Empty(run.Value.PromotionBlockedReasons);
        Assert.DoesNotContain("model_upload_not_ready", run.Value.BlockReasons);
        Assert.Equal(bool.TrueString, run.Value.BoundaryMetadata["model_upload_ready"]);
        Assert.Equal(bool.FalseString, run.Value.BoundaryMetadata["model_upload_not_ready"]);
        Assert.Equal(DesignConversionMeasurementGateStatus.Passed, run.Value.BoundaryMetadata["measurement_gate_status"]);
        Assert.Equal("PROMOTION_ALLOWED_BY_MEASUREMENT_GATE_AND_REAL_WEIGHTS", run.Value.BoundaryMetadata["promotion_gate_status"]);
        Assert.Equal("upload_ready_local_promotion_gate_passed_no_external_upload", run.Value.BoundaryMetadata["upload_readiness_status"]);
        Assert.Equal(string.Empty, run.Value.BoundaryMetadata["promotion_blocked_reasons"]);
        Assert.Equal(bool.FalseString, run.Value.BoundaryMetadata["promotion_upload_attempted"]);
        Assert.Equal(bool.FalseString, run.Value.BoundaryMetadata["promotion_claimed"]);
        Assert.Contains("model_server_upload_not_attempted", run.Value.Warnings);
        Assert.DoesNotContain("model_upload_not_ready", run.Value.Warnings);
        Assert.DoesNotContain("<main", serializedRun, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".private-user", serializedRun, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/Users/", serializedRun, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("exact_prompt", serializedRun, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("private.png", serializedRun, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LocalArtifactDb_RunTrainingIntakeAsync_SerializesConcurrentIdempotentRunsAcrossInstances()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local_artifact_db_training_intake_run_idempotency_gate");
        Directory.CreateDirectory(root);
        try
        {
            var setupDb = new JsonlDesignConversionLocalArtifactDb(root);
            var trainApprovedRuntime = WithRuntimeArtifactProofRefs(
                ToRuntimeApprovedCandidate(
                    WithSourceIdentity(
                        BuildTrainingRecord("runtime-approved-concurrent-run-a", DesignConversionDpoUnits.ImproverUnit, usefulness: 0.96),
                        "n9-3-concurrent-source-a")),
                "n9-3-concurrent-run-train");
            var heldOutApprovedRuntime = WithRuntimeArtifactProofRefs(
                ToRuntimeApprovedCandidate(
                    WithSourceIdentity(
                        BuildTrainingRecord("runtime-approved-concurrent-run-b", DesignConversionDpoUnits.CriticUnit, usefulness: 0.95),
                        "n9-3-concurrent-source-b")),
                "n9-3-concurrent-run-heldout");
            await Save(setupDb, BuildArtifactRecord(
                "storage-record-n9-3-concurrent-run",
                "job-n9-3-concurrent-run",
                "request-n9-3-concurrent-run",
                DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                [trainApprovedRuntime, heldOutApprovedRuntime],
                "<main data-user-id=\"user-luba\">exact_prompt private html</main>",
                ".private-user{background:url('C:/Users/private-user/private.png');}") with
            {
                SanitizedSourceRefs = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["source_manifest_ref"] = "manifest://design-conversion/n9-3-concurrent-run",
                    ["source_kind"] = "figma",
                    ["design_tree_payload_ref"] = "sha256:design-tree-n9-3-concurrent-run",
                    ["original_tree_payload_ref"] = "sha256:original-tree-n9-3-concurrent-run",
                    ["source_artifact:expected_desktop"] = "sha256:n9-3-concurrent-run-expected-design-render",
                    ["source_artifact:actual_desktop"] = "sha256:n9-3-concurrent-run-actual-browser-render"
                }
            });
            var handoff = await setupDb.ExportApprovedRuntimeDpoCandidatesAsync(
                new DesignConversionLocalArtifactDbQuery(JobId: "job-n9-3-concurrent-run"),
                DateTimeOffset.UnixEpoch.AddMinutes(193));
            Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
            var intake = await setupDb.CreateTrainingIntakeAsync(
                handoff.Value!,
                "requester:n9-3-local",
                "concurrent local run idempotency gate proof",
                DateTimeOffset.UnixEpoch.AddMinutes(193));
            Assert.True(intake.IsSuccess, intake.ErrorMessage);
            var intakeRecord = intake.Value!;
            var options = new DesignConversionTrainingPipelineOptions(
                RunId: "n9-3-concurrent-run-idempotent",
                BatchSize: 1,
                DatasetOptions: new DesignConversionDpoDatasetOptions(
                    DatasetId: "n9-3-concurrent-run-idempotent",
                    MinimumTrainRecords: 1,
                    MinimumHeldOutRecords: 1,
                    Preflight: DesignConversionDpoDatasetPreflight.LocalTrainingIntakeSourceManifestDisjointProof(
                        ["n9-3-concurrent-controlled-source-disjoint-intake"])));
            var admissionBoundaryMetadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["training_intake_tenant_id_ref"] = "tenant:n9-3-concurrent-run",
                ["training_intake_idempotency_key_ref"] = "idempotency:n9-3-concurrent-run"
            };
            const int callerCount = 12;
            var runDbs = Enumerable.Range(0, callerCount)
                .Select(_ => new JsonlDesignConversionLocalArtifactDb(root))
                .ToArray();
            using var ready = new CountdownEvent(callerCount);
            var startGate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var tasks = Enumerable.Range(0, callerCount)
                .Select(callerIndex => Task.Run(async () =>
                {
                    ready.Signal();
                    await startGate.Task;
                    await Task.Yield();
                    return await runDbs[callerIndex].RunTrainingIntakeAsync(
                        intakeRecord.IntakeId,
                        options,
                        admissionBoundaryMetadata: new Dictionary<string, string>(
                            admissionBoundaryMetadata,
                            StringComparer.Ordinal));
                }))
                .ToArray();

            var callersReady = ready.Wait(TimeSpan.FromSeconds(5));
            startGate.TrySetResult(true);
            var results = await Task.WhenAll(tasks);
            Assert.True(callersReady);
            var runs = results.Select(result =>
            {
                Assert.True(result.IsSuccess, result.ErrorMessage);
                return result.Value!;
            }).ToArray();
            var runRecordsPath = Path.Combine(root, "training-intake-runs.jsonl");
            var persistedText = await File.ReadAllTextAsync(runRecordsPath);
            var nonblankLines = (await File.ReadAllLinesAsync(runRecordsPath))
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToArray();
            var persistedRun = System.Text.Json.JsonSerializer.Deserialize<DesignConversionTrainingIntakeRunResult>(
                Assert.Single(nonblankLines),
                DesignConversionCheckpointJsonOptions.Standard);
            Assert.NotNull(persistedRun);
            var serializedRuns = System.Text.Json.JsonSerializer.Serialize(
                runs,
                DesignConversionCheckpointJsonOptions.Standard);

            Assert.All(runs, run =>
            {
                Assert.Equal(options.RunId, run.RunId);
                Assert.Equal(intakeRecord.IntakeId, run.TrainingIntakeId);
                Assert.True(run.MayStartTraining);
                Assert.Equal("proven", run.SourceManifestStatus);
                Assert.Equal("proven", run.SourceDisjointStatus);
                Assert.True(run.TrainingRunnerInvoked);
                Assert.True(run.LocalLifecycleHookAttempted);
                Assert.True(run.TrainingStarted);
                Assert.True(run.CheckpointMutated);
                Assert.True(run.WeightsProduced);
                Assert.Equal("local_lifecycle_training_started_with_real_learned_weights", run.TrainingExecutionStatus);
                Assert.Equal("runner_invoked_local_lifecycle_real_weight_artifact_no_external_training_claim", run.TrainingExecutionBoundary);
                Assert.Equal(bool.TrueString, run.BoundaryMetadata["training_intake_replay_cache_checked"]);
                Assert.Equal(
                    run.BoundaryMetadata["training_intake_idempotency_replay_hit"],
                    run.BoundaryMetadata["training_intake_idempotent_replay_result"]);
            });
            Assert.Equal(
                persistedRun!.BoundaryMetadata["training_intake_run_idempotency_payload_hash"],
                Assert.Single(runs.Select(run => run.BoundaryMetadata["training_intake_run_idempotency_payload_hash"]).Distinct(StringComparer.Ordinal)));
            Assert.Equal(options.RunId, persistedRun.RunId);
            Assert.True(persistedRun.TrainingRunnerInvoked);
            Assert.True(persistedRun.LocalLifecycleHookAttempted);
            Assert.True(persistedRun.TrainingStarted);
            Assert.True(persistedRun.CheckpointMutated);
            Assert.True(persistedRun.WeightsProduced);
            Assert.Equal(bool.FalseString, persistedRun.BoundaryMetadata["training_intake_idempotency_replay_hit"]);
            Assert.Equal(bool.TrueString, persistedRun.BoundaryMetadata["training_intake_idempotency_created_new_side_effect"]);
            Assert.Equal(bool.FalseString, persistedRun.BoundaryMetadata["training_intake_idempotent_replay_result"]);
            Assert.Equal("run_training_intake_append_only_once", persistedRun.BoundaryMetadata["training_intake_idempotency_side_effect_boundary"]);
            Assert.Equal(1, runs.Count(run =>
                run.BoundaryMetadata["training_intake_idempotency_replay_hit"] == bool.FalseString
                && run.BoundaryMetadata["training_intake_idempotency_created_new_side_effect"] == bool.TrueString));
            Assert.Equal(callerCount - 1, runs.Count(run =>
                run.BoundaryMetadata["training_intake_idempotency_replay_hit"] == bool.TrueString
                && run.BoundaryMetadata["training_intake_idempotency_created_new_side_effect"] == bool.FalseString));
            Assert.Single(nonblankLines, line => line.Contains(options.RunId, StringComparison.Ordinal));
            Assert.DoesNotContain("<main", persistedText, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(".private-user", persistedText, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("C:/Users/", persistedText, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("exact_prompt", persistedText, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("private.png", persistedText, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("<main", serializedRuns, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(".private-user", serializedRuns, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("C:/Users/", serializedRuns, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("exact_prompt", serializedRuns, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("private.png", serializedRuns, StringComparison.OrdinalIgnoreCase);

            var followUp = await new JsonlDesignConversionLocalArtifactDb(root).RunTrainingIntakeAsync(
                intakeRecord.IntakeId,
                options,
                admissionBoundaryMetadata: admissionBoundaryMetadata);
            var linesAfterFollowUp = (await File.ReadAllLinesAsync(runRecordsPath))
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToArray();

            Assert.True(followUp.IsSuccess, followUp.ErrorMessage);
            Assert.Equal(options.RunId, followUp.Value!.RunId);
            Assert.Equal(bool.TrueString, followUp.Value.BoundaryMetadata["training_intake_idempotency_replay_hit"]);
            Assert.Equal(bool.FalseString, followUp.Value.BoundaryMetadata["training_intake_idempotency_created_new_side_effect"]);
            Assert.Equal(nonblankLines, linesAfterFollowUp);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LocalArtifactDb_RunTrainingIntakeAndPromoteLocalAsync_SerializesConcurrentIdempotentPromotionsAcrossInstances()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local_artifact_db_training_intake_local_promotion_idempotency_gate");
        Directory.CreateDirectory(root);
        try
        {
            var setupDb = new JsonlDesignConversionLocalArtifactDb(root);
            var setupApi = new DesignConversionDpoUsefulnessApi(setupDb);
            var trainApprovedRuntime = WithRuntimeArtifactProofRefs(
                ToRuntimeApprovedCandidate(
                    WithSourceIdentity(
                        BuildTrainingRecord("runtime-approved-concurrent-promotion-a", DesignConversionDpoUnits.ImproverUnit, usefulness: 0.96),
                        "n9-3-concurrent-promotion-source-a")),
                "n9-3-concurrent-promotion-train");
            var heldOutApprovedRuntime = WithRuntimeArtifactProofRefs(
                ToRuntimeApprovedCandidate(
                    WithSourceIdentity(
                        BuildTrainingRecord("runtime-approved-concurrent-promotion-b", DesignConversionDpoUnits.CriticUnit, usefulness: 0.95),
                        "n9-3-concurrent-promotion-source-b")),
                "n9-3-concurrent-promotion-heldout");
            await Save(setupDb, BuildArtifactRecord(
                "storage-record-n9-3-concurrent-promotion",
                "job-n9-3-concurrent-promotion",
                "request-n9-3-concurrent-promotion",
                DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                [trainApprovedRuntime, heldOutApprovedRuntime],
                "<main data-user-id=\"user-luba\">exact_prompt private html</main>",
                ".private-user{background:url('C:/Users/private-user/private.png');}") with
            {
                SanitizedSourceRefs = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["source_manifest_ref"] = "manifest://design-conversion/n9-3-concurrent-promotion",
                    ["source_kind"] = "figma",
                    ["design_tree_payload_ref"] = "sha256:design-tree-n9-3-concurrent-promotion",
                    ["original_tree_payload_ref"] = "sha256:original-tree-n9-3-concurrent-promotion",
                    ["source_artifact:expected_desktop"] = "sha256:n9-3-concurrent-promotion-expected-design-render",
                    ["source_artifact:actual_desktop"] = "sha256:n9-3-concurrent-promotion-actual-browser-render"
                }
            });
            var handoff = await setupApi.ExportApprovedRuntimeDpoCandidatesAsync(
                new DesignConversionLocalArtifactDbQuery(JobId: "job-n9-3-concurrent-promotion"),
                DateTimeOffset.UnixEpoch.AddMinutes(194));
            Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
            Assert.Equal(2, handoff.Value!.Rows.Count);
            var intake = await setupApi.CreateTrainingIntakeAsync(
                handoff.Value,
                "requester:n9-3-local-promotion",
                "concurrent local promotion idempotency gate proof",
                DateTimeOffset.UnixEpoch.AddMinutes(195));
            Assert.True(intake.IsSuccess, intake.ErrorMessage);
            var intakeRecord = intake.Value!;
            var options = Pass157LocalPromotionOptions(
                "n9-3-concurrent-local-promotion",
                BuildPassingMeasurementGateRequest("n9-3-concurrent-local-promotion-gate"));
            var promotionOutputDirectory = Path.Combine(root, "shared-promotion-output");
            var admissionBoundaryMetadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["training_intake_tenant_id_ref"] = "tenant:n9-3-concurrent-promotion",
                ["training_intake_idempotency_key_ref"] = "idempotency:n9-3-concurrent-promotion"
            };
            const int callerCount = 4;
            var promotionApis = Enumerable.Range(0, callerCount)
                .Select(_ => new DesignConversionDpoUsefulnessApi(new JsonlDesignConversionLocalArtifactDb(root)))
                .ToArray();
            using var ready = new CountdownEvent(callerCount);
            var startGate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var tasks = Enumerable.Range(0, callerCount)
                .Select(callerIndex => Task.Run(async () =>
                {
                    ready.Signal();
                    await startGate.Task;
                    await Task.Yield();
                    return await DesignConversionTrainingIntakeRouteAdmissionContext.RunAsync(
                        new Dictionary<string, string>(admissionBoundaryMetadata, StringComparer.Ordinal),
                        () => promotionApis[callerIndex].RunTrainingIntakeAndPromoteLocalAsync(
                            intakeRecord.IntakeId,
                            promotionOutputDirectory,
                            options));
                }))
                .ToArray();

            var callersReady = false;
            try
            {
                callersReady = ready.Wait(TimeSpan.FromSeconds(10));
            }
            finally
            {
                startGate.TrySetResult(true);
            }

            var results = await Task.WhenAll(tasks);
            Assert.True(callersReady);
            var promotions = results.Select(result =>
            {
                Assert.True(result.IsSuccess, result.ErrorMessage);
                return result.Value!;
            }).ToArray();
            var createdPromotion = Assert.Single(promotions, promotion =>
                promotion.BoundaryMetadata["training_intake_idempotency_replay_hit"] == bool.FalseString
                && promotion.BoundaryMetadata["training_intake_idempotency_created_new_side_effect"] == bool.TrueString);
            var promotionRecordsPath = Path.Combine(root, "training-intake-local-promotions.jsonl");
            var persistedText = await File.ReadAllTextAsync(promotionRecordsPath);
            var nonblankLines = (await File.ReadAllLinesAsync(promotionRecordsPath))
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToArray();
            var persistedPromotion = System.Text.Json.JsonSerializer.Deserialize<DesignConversionTrainingIntakeLocalPromotionResult>(
                Assert.Single(nonblankLines),
                DesignConversionCheckpointJsonOptions.Standard);
            Assert.NotNull(persistedPromotion);
            var serializedPromotions = System.Text.Json.JsonSerializer.Serialize(
                promotions,
                DesignConversionCheckpointJsonOptions.Standard);
            var manifestArtifactSets = promotions
                .Select(promotion =>
                {
                    var artifacts = promotion.Publication!.Manifest.PromotionArtifacts!;
                    return string.Join("|", artifacts.CheckpointPath, artifacts.TokenizerPath, artifacts.PolicyPath);
                })
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            var artifactSnapshot = await ReadFileSha256SnapshotAsync(promotionOutputDirectory);

            Assert.All(promotions, promotion =>
            {
                Assert.Equal(options.RunId, promotion.TrainingRun.RunId);
                Assert.Equal(intakeRecord.IntakeId, promotion.TrainingRun.TrainingIntakeId);
                Assert.True(promotion.TrainingRun.ModelUploadReady);
                Assert.True(promotion.LocalPromotionReady);
                Assert.Equal(DesignConversionMeasurementGateStatus.Passed, promotion.TrainingRun.MeasurementGateStatus);
                Assert.Equal(createdPromotion.PromotionOutputDirectory, promotion.PromotionOutputDirectory);
                Assert.Equal(createdPromotion.PromotedCheckpointPath, promotion.PromotedCheckpointPath);
                Assert.Equal(createdPromotion.Publication!.Manifest.ManifestId, promotion.Publication!.Manifest.ManifestId);
                Assert.Equal(createdPromotion.Publication.Manifest.PromotionUploadRef, promotion.Publication.Manifest.PromotionUploadRef);
                Assert.Equal(createdPromotion.PromotedLocalModel!.ModelId, promotion.PromotedLocalModel!.ModelId);
                Assert.Equal(createdPromotion.PromotedLocalModel.CheckpointSha256, promotion.PromotedLocalModel.CheckpointSha256);
                Assert.Equal(
                    promotion.BoundaryMetadata["training_intake_idempotency_replay_hit"],
                    promotion.BoundaryMetadata["training_intake_idempotent_replay_result"]);
                Assert.Equal(
                    "promote_local_append_and_local_artifacts_once",
                    promotion.BoundaryMetadata["training_intake_idempotency_side_effect_boundary"]);
            });
            Assert.Equal(
                persistedPromotion!.BoundaryMetadata["training_intake_promote_local_idempotency_payload_hash"],
                Assert.Single(promotions
                    .Select(promotion => promotion.BoundaryMetadata["training_intake_promote_local_idempotency_payload_hash"])
                    .Distinct(StringComparer.Ordinal)));
            Assert.Equal(bool.FalseString, persistedPromotion.BoundaryMetadata["training_intake_idempotency_replay_hit"]);
            Assert.Equal(bool.TrueString, persistedPromotion.BoundaryMetadata["training_intake_idempotency_created_new_side_effect"]);
            Assert.Equal(callerCount - 1, promotions.Count(promotion =>
                promotion.BoundaryMetadata["training_intake_idempotency_replay_hit"] == bool.TrueString
                && promotion.BoundaryMetadata["training_intake_idempotency_created_new_side_effect"] == bool.FalseString));
            Assert.Single(manifestArtifactSets);
            Assert.Single(nonblankLines, line => line.Contains(options.RunId, StringComparison.Ordinal));
            Assert.Single(artifactSnapshot, artifact => artifact.Key.EndsWith("-promoted-checkpoint.json", StringComparison.Ordinal));
            Assert.Single(artifactSnapshot, artifact => artifact.Key.EndsWith("-tokenizer.json", StringComparison.Ordinal));
            Assert.Single(artifactSnapshot, artifact => artifact.Key.EndsWith("-policy.json", StringComparison.Ordinal));
            var policyCheckpointDirectories = artifactSnapshot
                .Select(artifact => artifact.Key.Split('/')[0])
                .Where(directory => directory.EndsWith("-dpo-policy-checkpoints", StringComparison.Ordinal))
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            Assert.Single(policyCheckpointDirectories);
            Assert.DoesNotContain(artifactSnapshot, artifact => artifact.Key.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase));
            AssertDoesNotContainRawPrivatePayload(persistedText);
            AssertDoesNotContainRawPrivatePayload(serializedPromotions);

            var followUpArtifactSnapshotBefore = await ReadFileSha256SnapshotAsync(promotionOutputDirectory);
            var followUp = await DesignConversionTrainingIntakeRouteAdmissionContext.RunAsync(
                admissionBoundaryMetadata,
                () => new DesignConversionDpoUsefulnessApi(new JsonlDesignConversionLocalArtifactDb(root))
                    .RunTrainingIntakeAndPromoteLocalAsync(
                        intakeRecord.IntakeId,
                        promotionOutputDirectory,
                        options));
            var linesAfterFollowUp = (await File.ReadAllLinesAsync(promotionRecordsPath))
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToArray();
            var followUpArtifactSnapshotAfter = await ReadFileSha256SnapshotAsync(promotionOutputDirectory);

            Assert.True(followUp.IsSuccess, followUp.ErrorMessage);
            Assert.Equal(options.RunId, followUp.Value!.TrainingRun.RunId);
            Assert.Equal(createdPromotion.PromotedCheckpointPath, followUp.Value.PromotedCheckpointPath);
            Assert.Equal(bool.TrueString, followUp.Value.BoundaryMetadata["training_intake_idempotency_replay_hit"]);
            Assert.Equal(bool.FalseString, followUp.Value.BoundaryMetadata["training_intake_idempotency_created_new_side_effect"]);
            Assert.Equal(nonblankLines, linesAfterFollowUp);
            Assert.Equal(followUpArtifactSnapshotBefore, followUpArtifactSnapshotAfter);
            AssertDoesNotContainRawPrivatePayload(System.Text.Json.JsonSerializer.Serialize(
                followUp.Value,
                DesignConversionCheckpointJsonOptions.Standard));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Pass157_LocalArtifactDb_UserRequestedRetrainPromotesLocalModelServerCheckpointOnlyAfterGates()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "pass157_training_intake_local_promotion");
        Directory.CreateDirectory(root);
        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var api = new DesignConversionDpoUsefulnessApi(db);
            var trainApprovedRuntime = WithRuntimeArtifactProofRefs(
                ToRuntimeApprovedCandidate(
                    WithSourceIdentity(
                        BuildTrainingRecord("pass157-approved-train-a", DesignConversionDpoUnits.ImproverUnit, usefulness: 0.96),
                        "pass157-source-a")),
                "pass157-train");
            var heldOutApprovedRuntime = WithRuntimeArtifactProofRefs(
                ToRuntimeApprovedCandidate(
                    WithSourceIdentity(
                        BuildTrainingRecord("pass157-approved-heldout-b", DesignConversionDpoUnits.CriticUnit, usefulness: 0.95),
                        "pass157-source-b")),
                "pass157-heldout");
            var artifactRecord = BuildArtifactRecord(
                "storage-record-pass157",
                "job-pass157",
                "request-pass157",
                DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                [trainApprovedRuntime, heldOutApprovedRuntime],
                "<main data-user-id=\"user-luba\">exact_prompt private html</main>",
                ".private-user{background:url('C:/Users/private-user/private.png');}") with
            {
                SanitizedSourceRefs = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["source_manifest_ref"] = "manifest://design-conversion/pass157",
                    ["source_kind"] = "figma",
                    ["design_tree_payload_ref"] = "sha256:design-tree-pass157",
                    ["original_tree_payload_ref"] = "sha256:original-tree-pass157",
                    ["source_artifact:expected_desktop"] = "sha256:pass157-expected-design-render",
                    ["source_artifact:actual_desktop"] = "sha256:pass157-actual-browser-render"
                }
            };
            await Save(db, artifactRecord);

            var handoff = await api.ExportApprovedRuntimeDpoCandidatesAsync(
                new DesignConversionLocalArtifactDbQuery(JobId: "job-pass157"),
                DateTimeOffset.UnixEpoch.AddMinutes(157));
            Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
            Assert.Equal(2, handoff.Value!.Rows.Count);

            var intake = await api.CreateTrainingIntakeAsync(
                handoff.Value,
                "architect:pass157-local",
                "Pass157 approved source-disjoint retrain and local ModelServer promotion boundary",
                DateTimeOffset.UnixEpoch.AddMinutes(158));
            Assert.True(intake.IsSuccess, intake.ErrorMessage);
            Assert.Equal("architect:pass157-local", intake.Value!.RequestedByRef);
            Assert.Equal(2, intake.Value.AcceptedCandidateCount);

            var baselineCheckpointPath = Path.Combine(root, "baseline-checkpoint.json");
            var baselineSave = await new DesignConversionMachine().SaveCheckpointAsync(baselineCheckpointPath);
            Assert.True(baselineSave.IsSuccess, baselineSave.ErrorMessage);
            var baselineSha = await ReadSha256Async(baselineCheckpointPath);

            var blocked = await api.RunTrainingIntakeAndPromoteLocalAsync(
                intake.Value.IntakeId,
                Path.Combine(root, "blocked-promotion"),
                Pass157LocalPromotionOptions(
                    "pass157-local-promotion-measurement-fail",
                    BuildFailingMeasurementGateRequest("pass157-measurement-fail")));
            Assert.True(blocked.IsSuccess, blocked.ErrorMessage);
            Assert.True(blocked.Value!.TrainingRun.WeightsProduced);
            Assert.False(blocked.Value.TrainingRun.ModelUploadReady);
            Assert.Equal(DesignConversionMeasurementGateStatus.Failed, blocked.Value.TrainingRun.MeasurementGateStatus);
            Assert.False(blocked.Value.LocalPromotionReady);
            Assert.False(blocked.Value.ExternalCloudUploadAttempted);
            Assert.Null(blocked.Value.Publication);
            Assert.Null(blocked.Value.PromotedLocalModel);
            Assert.Equal(bool.FalseString, blocked.Value.BoundaryMetadata["local_promotion_ready"]);
            Assert.Equal(bool.FalseString, blocked.Value.BoundaryMetadata["external_cloud_upload_attempted"]);
            Assert.Equal(bool.TrueString, blocked.Value.BoundaryMetadata["external_cloud_upload_not_attempted"]);
            Assert.Equal(bool.FalseString, blocked.Value.BoundaryMetadata["cloud_model_upload_attempted"]);
            Assert.Equal(bool.TrueString, blocked.Value.BoundaryMetadata["cloud_model_upload_not_attempted"]);
            Assert.Equal(bool.TrueString, blocked.Value.BoundaryMetadata["model_server_upload_not_attempted_local_only"]);
            Assert.Equal(bool.TrueString, blocked.Value.BoundaryMetadata["model_server_promotion_not_claimed_local_only"]);
            Assert.Contains("local_promotion_blocked_model_upload_not_ready", blocked.Value.BlockReasons);

            var promoted = await api.RunTrainingIntakeAndPromoteLocalAsync(
                intake.Value.IntakeId,
                Path.Combine(root, "ready-promotion"),
                Pass157LocalPromotionOptions(
                    "pass157-local-promotion-measurement-pass",
                    BuildPassingMeasurementGateRequest("pass157-measurement-pass")));
            Assert.True(promoted.IsSuccess, promoted.ErrorMessage);
            Assert.True(promoted.Value!.TrainingRun.MayStartTraining);
            Assert.Equal("proven", promoted.Value.TrainingRun.SourceManifestStatus);
            Assert.Equal("proven", promoted.Value.TrainingRun.SourceDisjointStatus);
            Assert.True(promoted.Value.TrainingRun.TrainingStarted);
            Assert.True(promoted.Value.TrainingRun.CheckpointMutated);
            Assert.True(promoted.Value.TrainingRun.CheckpointCandidateCreated);
            Assert.True(promoted.Value.TrainingRun.WeightsProduced);
            Assert.True(promoted.Value.TrainingRun.ModelUploadReady);
            Assert.Equal(DesignConversionMeasurementGateStatus.Passed, promoted.Value.TrainingRun.MeasurementGateStatus);
            Assert.Equal("PROMOTION_ALLOWED_BY_MEASUREMENT_GATE_AND_REAL_WEIGHTS", promoted.Value.TrainingRun.PromotionGateStatus);
            Assert.Equal("upload_ready_local_promotion_gate_passed_no_external_upload", promoted.Value.TrainingRun.UploadReadinessStatus);
            Assert.True(promoted.Value.LocalPromotionReady);
            Assert.False(promoted.Value.ExternalCloudUploadAttempted);
            Assert.NotNull(promoted.Value.Publication);
            Assert.NotNull(promoted.Value.Publication!.UploadReceipt);
            Assert.StartsWith(
                DesignConversionPromotedLocalModelLoader.LocalOnlyPromotionRefPrefix,
                promoted.Value.Publication.Manifest.PromotionUploadRef,
                StringComparison.Ordinal);
            Assert.NotNull(promoted.Value.PromotedLocalModel);
            Assert.True(File.Exists(promoted.Value.PromotedCheckpointPath));
            Assert.NotEqual(baselineSha, promoted.Value.PromotedLocalModel!.CheckpointSha256);
            Assert.NotEqual("0.1.0", promoted.Value.PromotedLocalModel.Version);
            Assert.True(promoted.Value.PromotedLocalModel.LearnedWeightArtifactCount > 0);
            Assert.True(promoted.Value.PromotedLocalModel.LearnedWeightObservationCount > 0);
            Assert.True(promoted.Value.PromotedLocalModel.CheckpointFreshLoadVerified);
            Assert.True(promoted.Value.PromotedLocalModel.CheckpointContainsRealTrainedWeights);
            Assert.True(promoted.Value.PromotedLocalModel.LocalOnly);
            Assert.Equal(bool.TrueString, promoted.Value.BoundaryMetadata["local_promotion_ready"]);
            Assert.Equal(bool.FalseString, promoted.Value.BoundaryMetadata["external_cloud_upload_attempted"]);
            Assert.Equal(bool.TrueString, promoted.Value.BoundaryMetadata["external_cloud_upload_not_attempted"]);
            Assert.Equal(bool.FalseString, promoted.Value.BoundaryMetadata["cloud_model_upload_attempted"]);
            Assert.Equal(bool.TrueString, promoted.Value.BoundaryMetadata["cloud_model_upload_not_attempted"]);
            Assert.Equal(bool.TrueString, promoted.Value.BoundaryMetadata["model_server_upload_not_attempted_local_only"]);
            Assert.Equal(bool.TrueString, promoted.Value.BoundaryMetadata["model_server_promotion_not_claimed_local_only"]);
            Assert.Equal(bool.TrueString, promoted.Value.BoundaryMetadata["local_promotion_receipt_written"]);
            Assert.Equal("local_receipt_only", promoted.Value.BoundaryMetadata["promotion_receipt_kind"]);
            Assert.Equal(bool.TrueString, promoted.Value.BoundaryMetadata["model_server_upload_not_attempted_local_receipt"]);
            Assert.Equal(bool.TrueString, promoted.Value.BoundaryMetadata["model_server_promotion_not_claimed_local_receipt"]);
            Assert.DoesNotContain("model_server_upload_attempted_local_only", promoted.Value.BoundaryMetadata.Keys);
            Assert.DoesNotContain("model_server_promotion_claimed_local_only", promoted.Value.BoundaryMetadata.Keys);

            var baselineModelServer = DesignConversionModelServerApi.CreateMachineBackedLocal(
                new DesignConversionMachine(),
                new Pass157RecordingRenderProvider(),
                new Pass157RecordingComparisonProvider(CreatePass157Verdict("pass157-baseline-modelserver-verdict", 0.93, passed: true)),
                utcNow: () => DateTimeOffset.Parse("2026-07-03T12:00:00Z"));
            var baselineResponse = await baselineModelServer.SubmitAndEvaluateAsync(CreatePass157Request("pass157-baseline-modelserver-request"));
            Assert.True(baselineResponse.IsSuccess, baselineResponse.ErrorMessage);
            Assert.False(baselineResponse.Value!.LearnedInferenceApplied);
            Assert.DoesNotContain(DesignConversionMachine.LearnedInferenceAppliedEvidence, baselineResponse.Value.EvidenceRefs);

            var promotedApi = await DesignConversionModelServerApi.CreateFromTrainingIntakeLocalPromotionAsync(
                promoted.Value,
                new Pass157RecordingRenderProvider(),
                new Pass157RecordingComparisonProvider(CreatePass157Verdict("pass157-modelserver-verdict", 0.93, passed: true)),
                utcNow: () => DateTimeOffset.Parse("2026-07-03T12:00:00Z"),
                trainingReadbackApi: api);
            Assert.True(promotedApi.IsSuccess, promotedApi.ErrorMessage);
            Assert.Contains(
                "DESIGN_CONVERSION_MODEL_SERVER_API_CREATED_FROM_TRAINING_INTAKE_LOCAL_PROMOTION",
                promotedApi.Value!.EvidenceRefs);
            Assert.Contains(
                "DESIGN_CONVERSION_MODEL_SERVER_API_FRESH_LOADED_PROMOTED_LOCAL_CHECKPOINT",
                promotedApi.Value.EvidenceRefs);
            Assert.Equal(promoted.Value.PromotedLocalModel.ModelId, promotedApi.Value.PromotedLoad.Model.ModelId);
            var modelServer = promotedApi.Value.Api;
            var promotedLoad = promotedApi.Value.PromotedLoad;

            var response = await modelServer.SubmitAndEvaluateAsync(CreatePass157Request("pass157-promoted-modelserver-request"));
            Assert.True(response.IsSuccess, response.ErrorMessage);
            Assert.True(response.Value!.LearnedInferenceApplied);
            Assert.NotEqual(baselineResponse.Value.LearnedInferenceApplied, response.Value.LearnedInferenceApplied);
            Assert.True(response.Value.ModelTrainingVerified);
            Assert.False(response.Value.ModelUploadReady);
            Assert.False(response.Value.DeploymentVerified);
            Assert.False(response.Value.ProductReady);
            Assert.Equal("promoted_local_checkpoint_loaded", response.Value.TrainingReadiness.Status);
            Assert.Equal("local_promoted_checkpoint_loaded", response.Value.TrainingReadiness.PromotionStatus);
            Assert.Equal(promoted.Value.PromotedLocalModel.ModelId, response.Value.PromotedLocalModel!.ModelId);
            Assert.Equal(promoted.Value.PromotedLocalModel.ModelId, response.Value.BoundaryMetadata["local_promoted_model_id"]);
            Assert.Equal(promoted.Value.PromotedLocalModel.Version, response.Value.BoundaryMetadata["local_promoted_model_version"]);
            Assert.Equal(bool.TrueString, response.Value.BoundaryMetadata["local_model_promotion_verified"]);
            Assert.Equal(bool.TrueString, response.Value.BoundaryMetadata["local_promoted_checkpoint_loaded"]);
            Assert.Equal(bool.TrueString, response.Value.BoundaryMetadata["local_promoted_checkpoint_real_weights"]);
            Assert.Equal(bool.FalseString, response.Value.BoundaryMetadata["cloud_model_upload_attempted"]);
            Assert.Equal(bool.FalseString, response.Value.BoundaryMetadata["raw_html_included"]);
            Assert.Equal(bool.FalseString, response.Value.BoundaryMetadata["raw_css_included"]);
            Assert.Equal(bool.FalseString, response.Value.BoundaryMetadata["raw_user_private_data_included"]);
            Assert.Contains(DesignConversionMachine.LearnedInferenceAppliedEvidence, response.Value.EvidenceRefs);

            var status = modelServer.GetJobResult(response.Value.JobId);
            Assert.True(status.IsSuccess, status.ErrorMessage);
            Assert.True(status.Value!.LearnedInferenceApplied);
            Assert.Equal(promoted.Value.PromotedLocalModel.ModelId, status.Value.BoundaryMetadata["local_promoted_model_id"]);
            Assert.False(status.Value.DeploymentVerified);
            Assert.False(status.Value.ProductReady);

            await WritePass189RPromotionManifestIfRequestedAsync(root, promoted.Value, promotedLoad, response.Value, status.Value);

            var serializedRun = System.Text.Json.JsonSerializer.Serialize(promoted.Value.TrainingRun);
            var serializedBoundary = System.Text.Json.JsonSerializer.Serialize(response.Value.BoundaryMetadata);
            Assert.DoesNotContain("<main", serializedRun, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(".private-user", serializedRun, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("C:/Users/", serializedRun, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("exact_prompt", serializedRun, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("private.png", serializedRun, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("external-provider-payload", serializedRun, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("<main", serializedBoundary, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(".private-user", serializedBoundary, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("exact_prompt", serializedBoundary, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("private.png", serializedBoundary, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("external-provider-payload", serializedBoundary, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ProductTrainingIntake_FreshLoadsPromotedCheckpointIntoRuntimeFacingModelServer()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "product_training_intake_fresh_load_runtime");
        Directory.CreateDirectory(root);
        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var api = new DesignConversionDpoUsefulnessApi(db);
            var trainApprovedRuntime = WithRuntimeArtifactProofRefs(
                ToRuntimeApprovedCandidate(
                    WithSourceIdentity(
                        BuildTrainingRecord("product-intake-freshload-train", DesignConversionDpoUnits.ImproverUnit, usefulness: 0.97),
                        "product-intake-freshload-source-train")),
                "product-intake-freshload-train");
            var heldOutApprovedRuntime = WithRuntimeArtifactProofRefs(
                ToRuntimeApprovedCandidate(
                    WithSourceIdentity(
                        BuildTrainingRecord("product-intake-freshload-heldout", DesignConversionDpoUnits.CriticUnit, usefulness: 0.96),
                        "product-intake-freshload-source-heldout")),
                "product-intake-freshload-heldout");
            var artifactRecord = BuildArtifactRecord(
                "storage-record-product-intake-freshload",
                "job-product-intake-freshload",
                "request-product-intake-freshload",
                DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                [trainApprovedRuntime, heldOutApprovedRuntime],
                "<main data-user-id=\"user-luba\">exact_prompt private html</main>",
                ".private-user{background:url('C:/Users/private-user/private.png');}") with
            {
                SanitizedSourceRefs = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["source_manifest_ref"] = "manifest://design-conversion/product-intake-freshload",
                    ["source_kind"] = "figma",
                    ["design_tree_payload_ref"] = "sha256:design-tree-product-intake-freshload",
                    ["original_tree_payload_ref"] = "sha256:original-tree-product-intake-freshload",
                    ["source_artifact:expected_desktop"] = "sha256:product-intake-freshload-expected-design-render",
                    ["source_artifact:actual_desktop"] = "sha256:product-intake-freshload-actual-browser-render"
                }
            };
            await Save(db, artifactRecord);

            var handoff = await api.ExportApprovedRuntimeDpoCandidatesAsync(
                new DesignConversionLocalArtifactDbQuery(JobId: "job-product-intake-freshload"),
                DateTimeOffset.UnixEpoch.AddMinutes(211));
            Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
            Assert.Equal(2, handoff.Value!.ApprovedCandidateCount);

            var intake = await api.CreateTrainingIntakeAsync(
                handoff.Value,
                "architect:product-intake-freshload",
                "Focused product intake runner checkpoint fresh-load runtime proof",
                DateTimeOffset.UnixEpoch.AddMinutes(212));
            Assert.True(intake.IsSuccess, intake.ErrorMessage);

            var promoted = await api.RunTrainingIntakeAndPromoteLocalAsync(
                intake.Value!.IntakeId,
                Path.Combine(root, "promotion"),
                Pass157LocalPromotionOptions(
                    "product-training-intake-fresh-load-runtime",
                    BuildPassingMeasurementGateRequest("product-training-intake-fresh-load-runtime-gate")));
            Assert.True(promoted.IsSuccess, promoted.ErrorMessage);

            var trainingRun = promoted.Value!.TrainingRun;
            var runnerEvidence = Assert.IsType<DesignConversionTrainingIntakeRunnerEvidence>(trainingRun.RunnerEvidence);
            Assert.True(trainingRun.TrainingRunnerInvoked);
            Assert.True(trainingRun.TrainingStarted);
            Assert.True(trainingRun.CheckpointMutated);
            Assert.True(trainingRun.CheckpointCandidateCreated);
            Assert.True(trainingRun.WeightsProduced);
            Assert.True(trainingRun.ModelUploadReady);
            Assert.True(runnerEvidence.DatasetCountGatePassed);
            Assert.True(runnerEvidence.SourceDisjoint);
            Assert.True(runnerEvidence.MachineLifecycleHookAttempted);
            Assert.True(runnerEvidence.CheckpointMutationEvidenceProven);
            Assert.True(runnerEvidence.CheckpointContainsRealTrainedWeights);
            Assert.Equal(DesignConversionLocalFreshLoadProbe.VerifiedStatus, runnerEvidence.FreshLoadStatus);
            Assert.Equal(DesignConversionLocalContinueTrainingProbe.VerifiedStatus, runnerEvidence.ContinueTrainingStatus);
            Assert.Equal("local_checkpoint_candidate_with_real_learned_weights", runnerEvidence.CheckpointCandidateStatus);
            Assert.Equal("local_continue_checkpoint_mutated_with_real_learned_weights", runnerEvidence.CheckpointMutationStatus);
            Assert.Equal("real_trained_weights_proven", runnerEvidence.WeightProofStatus);
            Assert.Contains("DESIGN_CONVERSION_LEARNED_WEIGHTS_PRESENT_TRUE_AFTER_FRESH_LOAD", runnerEvidence.EvidenceRefs);
            Assert.Contains(DesignConversionMachine.LearnedWeightsPersistedEvidence, runnerEvidence.EvidenceRefs);
            Assert.Equal(bool.TrueString, trainingRun.BoundaryMetadata["training_runner_invoked"]);
            Assert.Equal(bool.TrueString, trainingRun.BoundaryMetadata["checkpoint_mutated"]);
            Assert.Equal(bool.TrueString, trainingRun.BoundaryMetadata["checkpoint_contains_real_trained_weights"]);
            Assert.Equal(bool.FalseString, trainingRun.BoundaryMetadata["promotion_upload_attempted"]);
            Assert.Equal(bool.FalseString, trainingRun.BoundaryMetadata["product_ready"]);

            Assert.True(promoted.Value.LocalPromotionReady);
            Assert.False(promoted.Value.ExternalCloudUploadAttempted);
            Assert.True(File.Exists(promoted.Value.PromotedCheckpointPath));
            Assert.NotNull(promoted.Value.PromotedLocalModel);
            Assert.True(promoted.Value.PromotedLocalModel!.CheckpointFreshLoadVerified);
            Assert.True(promoted.Value.PromotedLocalModel.CheckpointContainsRealTrainedWeights);
            Assert.True(promoted.Value.PromotedLocalModel.LearnedWeightArtifactCount > 0);
            Assert.True(promoted.Value.PromotedLocalModel.LearnedWeightObservationCount > 0);

            var baselineModelServer = DesignConversionModelServerApi.CreateMachineBackedLocal(
                new DesignConversionMachine(),
                new Pass157RecordingRenderProvider(),
                new Pass157RecordingComparisonProvider(CreatePass157Verdict("product-intake-baseline-verdict", 0.93, passed: true)),
                utcNow: () => DateTimeOffset.Parse("2026-07-03T12:00:00Z"));
            var baselineResponse = await baselineModelServer.SubmitAndEvaluateAsync(
                CreatePass157Request("product-intake-baseline-runtime-request"));
            Assert.True(baselineResponse.IsSuccess, baselineResponse.ErrorMessage);
            Assert.False(baselineResponse.Value!.LearnedInferenceApplied);

            var promotedApi = await DesignConversionModelServerApi.CreateFromTrainingIntakeLocalPromotionAsync(
                promoted.Value,
                new Pass157RecordingRenderProvider(),
                new Pass157RecordingComparisonProvider(CreatePass157Verdict("product-intake-promoted-verdict", 0.93, passed: true)),
                utcNow: () => DateTimeOffset.Parse("2026-07-03T12:00:00Z"),
                trainingReadbackApi: api);
            Assert.True(promotedApi.IsSuccess, promotedApi.ErrorMessage);
            Assert.Contains(
                "DESIGN_CONVERSION_MODEL_SERVER_API_FRESH_LOADED_PROMOTED_LOCAL_CHECKPOINT",
                promotedApi.Value!.EvidenceRefs);
            Assert.Equal(promoted.Value.PromotedLocalModel.ModelId, promotedApi.Value.PromotedLoad.Model.ModelId);

            var response = await promotedApi.Value.Api.SubmitAndEvaluateAsync(
                CreatePass157Request("product-intake-promoted-runtime-request"));
            Assert.True(response.IsSuccess, response.ErrorMessage);
            Assert.True(response.Value!.LearnedInferenceApplied);
            Assert.NotEqual(baselineResponse.Value.LearnedInferenceApplied, response.Value.LearnedInferenceApplied);
            Assert.True(response.Value.ModelTrainingVerified);
            Assert.False(response.Value.ModelUploadReady);
            Assert.False(response.Value.DeploymentVerified);
            Assert.False(response.Value.ProductReady);
            Assert.Equal("promoted_local_checkpoint_loaded", response.Value.TrainingReadiness.Status);
            Assert.Equal("local_promoted_checkpoint_loaded", response.Value.TrainingReadiness.PromotionStatus);
            Assert.Equal(promoted.Value.PromotedLocalModel.ModelId, response.Value.PromotedLocalModel!.ModelId);
            Assert.Equal(bool.TrueString, response.Value.BoundaryMetadata["local_promoted_checkpoint_loaded"]);
            Assert.Equal(bool.TrueString, response.Value.BoundaryMetadata["local_promoted_checkpoint_real_weights"]);
            Assert.Equal(bool.FalseString, response.Value.BoundaryMetadata["cloud_model_upload_attempted"]);
            Assert.Contains(DesignConversionMachine.LearnedInferenceAppliedEvidence, response.Value.EvidenceRefs);

            var status = promotedApi.Value.Api.GetJobResult(response.Value.JobId);
            Assert.True(status.IsSuccess, status.ErrorMessage);
            Assert.True(status.Value!.LearnedInferenceApplied);
            Assert.Equal(promoted.Value.PromotedLocalModel.ModelId, status.Value.BoundaryMetadata["local_promoted_model_id"]);
            Assert.False(status.Value.DeploymentVerified);
            Assert.False(status.Value.ProductReady);

            var serializedRun = System.Text.Json.JsonSerializer.Serialize(trainingRun);
            var serializedBoundary = System.Text.Json.JsonSerializer.Serialize(response.Value.BoundaryMetadata);
            Assert.DoesNotContain("<main", serializedRun, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(".private-user", serializedRun, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("C:/Users/", serializedRun, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("exact_prompt", serializedRun, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("private.png", serializedRun, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("external-provider-payload", serializedRun, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("<main", serializedBoundary, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(".private-user", serializedBoundary, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("exact_prompt", serializedBoundary, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("private.png", serializedBoundary, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("external-provider-payload", serializedBoundary, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LocalArtifactDb_RunTrainingIntakeAsync_BlocksMissingSourceManifestBeforeRunner()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local_artifact_db_training_intake_missing_source_manifest");
        var db = new JsonlDesignConversionLocalArtifactDb(root);
        var approvedRuntime = WithRuntimeArtifactProofRefs(
            ToRuntimeApprovedCandidate(
                WithSourceIdentity(
                    BuildTrainingRecord("runtime-approved-missing-source-manifest", DesignConversionDpoUnits.ImproverUnit, usefulness: 0.95),
                    "pass112-missing-source")),
            "pass112-missing-source-manifest");
        await Save(db, BuildArtifactRecord(
            "storage-record-missing-source-manifest",
            "job-missing-source-manifest",
            "request-missing-source-manifest",
            DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            [approvedRuntime],
            "<main data-user-id=\"user-luba\">exact_prompt private html</main>",
            ".private-user{background:url('C:/Users/private-user/private.png');}") with
        {
            SanitizedSourceRefs = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["source_manifest_ref"] = "manifest://design-conversion/pass112-missing-source",
                ["source_kind"] = "figma",
                ["design_tree_payload_ref"] = "sha256:design-tree-pass112-missing-source",
                ["original_tree_payload_ref"] = "sha256:original-tree-pass112-missing-source",
                ["source_artifact:expected_desktop"] = "sha256:pass112-missing-source-expected-design-render",
                ["source_artifact:actual_desktop"] = "sha256:pass112-missing-source-actual-browser-render"
            }
        });
        var handoff = await db.ExportApprovedRuntimeDpoCandidatesAsync(
            new DesignConversionLocalArtifactDbQuery(JobId: "job-missing-source-manifest"),
            DateTimeOffset.UnixEpoch.AddMinutes(112));
        Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
        var intake = await db.CreateTrainingIntakeAsync(
            handoff.Value!,
            "requester:pass112-local",
            "source manifest proof negative",
            DateTimeOffset.UnixEpoch.AddMinutes(112));
        Assert.True(intake.IsSuccess, intake.ErrorMessage);
        var candidate = Assert.Single(intake.Value!.CandidateRefs);
        var missingSourceManifestIntake = intake.Value with
        {
            IntakeId = "training-intake-pass112-missing-source-manifest",
            CandidateRefs =
            [
                candidate with
                {
                    SourceManifestRef = "",
                    SourceManifestHash = "",
                    SourceIdentityHash = ""
                }
            ]
        };
        await File.AppendAllTextAsync(
            Path.Combine(root, "training-intake-requests.jsonl"),
            System.Text.Json.JsonSerializer.Serialize(missingSourceManifestIntake) + Environment.NewLine);

        var run = await db.RunTrainingIntakeAsync(missingSourceManifestIntake.IntakeId);
        var serializedRun = System.Text.Json.JsonSerializer.Serialize(run.Value);

        Assert.True(run.IsSuccess, run.ErrorMessage);
        Assert.False(run.Value!.MayStartTraining);
        Assert.Equal("missing", run.Value.SourceManifestStatus);
        Assert.Equal("missing", run.Value.SourceDisjointStatus);
        Assert.False(run.Value.TrainingRunnerInvoked);
        Assert.False(run.Value.TrainingStarted);
        Assert.False(run.Value.CheckpointMutated);
        Assert.False(run.Value.WeightsProduced);
        Assert.False(run.Value.ModelUploadReady);
        Assert.Contains("source_manifest_missing", run.Value.BlockReasons);
        Assert.Contains("source_disjoint_missing", run.Value.BlockReasons);
        Assert.Equal("blocked_before_runner_by_training_intake_preflight", run.Value.TrainingExecutionBoundary);
        Assert.DoesNotContain("<main", serializedRun, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".private-user", serializedRun, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/Users/", serializedRun, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("exact_prompt", serializedRun, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("private.png", serializedRun, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LocalArtifactDb_RunTrainingIntakeAsync_BlocksSameSourceIdsBeforeRunner()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local_artifact_db_training_intake_same_source");
        var db = new JsonlDesignConversionLocalArtifactDb(root);
        var firstApprovedRuntime = WithRuntimeArtifactProofRefs(
            ToRuntimeApprovedCandidate(
                WithSourceIdentity(
                    BuildTrainingRecord("runtime-approved-same-source-a", DesignConversionDpoUnits.ImproverUnit, usefulness: 0.95),
                    "pass112-same-source")),
            "pass112-same-source-a");
        var secondApprovedRuntime = WithRuntimeArtifactProofRefs(
            ToRuntimeApprovedCandidate(
                WithSourceIdentity(
                    BuildTrainingRecord("runtime-approved-same-source-b", DesignConversionDpoUnits.CriticUnit, usefulness: 0.94),
                    "pass112-same-source")),
            "pass112-same-source-b");
        await Save(db, BuildArtifactRecord(
            "storage-record-same-source",
            "job-same-source",
            "request-same-source",
            DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            [firstApprovedRuntime, secondApprovedRuntime],
            "<main data-user-id=\"user-luba\">exact_prompt private html</main>",
            ".private-user{background:url('C:/Users/private-user/private.png');}") with
        {
            SanitizedSourceRefs = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["source_manifest_ref"] = "manifest://design-conversion/pass112-same-source",
                ["source_kind"] = "figma",
                ["design_tree_payload_ref"] = "sha256:design-tree-pass112-same-source",
                ["original_tree_payload_ref"] = "sha256:original-tree-pass112-same-source",
                ["source_artifact:expected_desktop"] = "sha256:pass112-same-source-expected-design-render",
                ["source_artifact:actual_desktop"] = "sha256:pass112-same-source-actual-browser-render"
            }
        });
        var handoff = await db.ExportApprovedRuntimeDpoCandidatesAsync(
            new DesignConversionLocalArtifactDbQuery(JobId: "job-same-source"),
            DateTimeOffset.UnixEpoch.AddMinutes(112));
        var intake = await db.CreateTrainingIntakeAsync(
            handoff.Value!,
            "requester:pass112-local",
            "same source proof negative",
            DateTimeOffset.UnixEpoch.AddMinutes(112));

        var run = await db.RunTrainingIntakeAsync(intake.Value!.IntakeId);

        Assert.True(run.IsSuccess, run.ErrorMessage);
        Assert.False(run.Value!.MayStartTraining);
        Assert.Equal("proven", run.Value.SourceManifestStatus);
        Assert.Equal("blocked_same_source", run.Value.SourceDisjointStatus);
        Assert.False(run.Value.TrainingRunnerInvoked);
        Assert.False(run.Value.TrainingStarted);
        Assert.False(run.Value.CheckpointMutated);
        Assert.False(run.Value.WeightsProduced);
        Assert.False(run.Value.ModelUploadReady);
        Assert.Contains("source_disjoint_blocked_same_source", run.Value.BlockReasons);
        Assert.Equal("blocked_before_runner_by_training_intake_preflight", run.Value.TrainingExecutionBoundary);
    }

    [Fact]
    public async Task JsonlStorage_RoundTripsExamplesWithDeterministicOrdering()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "design-dpo.jsonl");
        var storage = new JsonlDesignConversionDpoStorage(path);

        await Save(storage, DesignConversionDpoGeneratorTests.CreateExample("medium", DesignConversionDpoUnits.CriticUnit, usefulness: 0.60));
        await Save(storage, DesignConversionDpoGeneratorTests.CreateExample("high", DesignConversionDpoUnits.CriticUnit, usefulness: 0.95));
        await Save(storage, DesignConversionDpoGeneratorTests.CreateExample("low", DesignConversionDpoUnits.ImproverUnit, usefulness: 0.30));

        var reloaded = new JsonlDesignConversionDpoStorage(path);
        var listed = await reloaded.ListExamplesAsync();
        var queried = await reloaded.QueryExamplesAsync(new DesignConversionDpoStorageQuery(UnitId: "critic"));

        Assert.True(listed.IsSuccess, listed.ErrorMessage);
        Assert.Equal(["high", "low", "medium"], listed.Value!.Select(example => example.ExampleId).ToArray());
        Assert.True(queried.IsSuccess, queried.ErrorMessage);
        Assert.Equal(["high", "medium"], queried.Value!.Select(example => example.ExampleId).ToArray());
    }

    [Fact]
    public async Task QueryExamples_OrdersByUsefulnessAndFiltersByPrivateDataAndUsefulness()
    {
        var storage = new InMemoryDesignConversionDpoStorage();
        await Save(storage, DesignConversionDpoGeneratorTests.CreateExample("medium", DesignConversionDpoUnits.CriticUnit, usefulness: 0.60));
        await Save(storage, DesignConversionDpoGeneratorTests.CreateExample("high-private", DesignConversionDpoUnits.CriticUnit, usefulness: 0.95) with
        {
            License = DesignConversionDpoGeneratorTests.CreateLicense(allowsTraining: true, containsPrivateData: true)
        });
        await Save(storage, DesignConversionDpoGeneratorTests.CreateExample("low", DesignConversionDpoUnits.CriticUnit, usefulness: 0.20));

        var highValue = await storage.QueryExamplesAsync(new DesignConversionDpoStorageQuery(
            UnitId: DesignConversionDpoUnits.CriticUnit,
            ContainsUserPrivateData: false,
            MinimumUsefulnessScore: 0.50));

        Assert.True(highValue.IsSuccess, highValue.ErrorMessage);
        Assert.Equal(["medium"], highValue.Value!.Select(example => example.ExampleId).ToArray());
    }

    [Fact]
    public async Task QueryTrainReadyRecords_ExcludesQuarantinedAndPrivateDataExamples()
    {
        var storage = new InMemoryDesignConversionDpoStorage();
        await Save(storage, DesignConversionDpoGeneratorTests.CreateExample("trainable", DesignConversionDpoUnits.CriticUnit, usefulness: 0.70));
        await Save(storage, DesignConversionDpoGeneratorTests.CreateExample("private", DesignConversionDpoUnits.CriticUnit, usefulness: 0.99) with
        {
            License = DesignConversionDpoGeneratorTests.CreateLicense(allowsTraining: true, containsPrivateData: true)
        });
        await Save(storage, DesignConversionDpoGeneratorTests.CreateExample("blocked", DesignConversionDpoUnits.ImproverUnit, usefulness: 0.90) with
        {
            License = DesignConversionDpoGeneratorTests.CreateLicense(allowsTraining: false, containsPrivateData: false)
        });

        var allVisible = await storage.QueryExamplesAsync(new DesignConversionDpoStorageQuery());
        var quarantinedVisible = await storage.QueryExamplesAsync(new DesignConversionDpoStorageQuery(IsQuarantined: true));
        var trainReady = await storage.QueryTrainReadyRecordsAsync();

        Assert.True(allVisible.IsSuccess, allVisible.ErrorMessage);
        Assert.Equal(3, allVisible.Value!.Count);
        Assert.True(quarantinedVisible.IsSuccess, quarantinedVisible.ErrorMessage);
        Assert.Equal(["private", "blocked"], quarantinedVisible.Value!.Select(example => example.ExampleId).ToArray());
        Assert.True(trainReady.IsSuccess, trainReady.ErrorMessage);
        var record = Assert.Single(trainReady.Value!);
        Assert.Equal("trainable", record.RecordId);
        Assert.False(record.Quarantine.IsQuarantined);
        Assert.False(record.License.ContainsUserPrivateData);
    }

    [Fact]
    public async Task QueryTrainReadyRecords_ExcludesProviderMissingRows()
    {
        var storage = new InMemoryDesignConversionDpoStorage();
        await Save(storage, DesignConversionDpoGeneratorTests.CreateExample(
            "provider-missing",
            DesignConversionDpoUnits.CriticUnit,
            usefulness: 0.95) with
        {
            Provenance = new DesignConversionDpoProvenance(
                SourceRequestId: "design-request-redacted",
                SourceKind: DesignConversionSourceKind.Figma.ToString(),
                SourceManifestRef: "manifest://design-conversion/source-redacted",
                ReviewerRef: "teacher:redacted",
                CreatedAtUtc: DateTimeOffset.UnixEpoch,
                Metadata: new Dictionary<string, string>
                {
                    ["visual_judge_status"] = DesignConversionVisualQaBoundaryPolicy.ProviderRequired,
                    ["provider_boundary"] = DesignConversionVisualQaBoundaryPolicy.ExternalProviderNotConfiguredMarker
                })
        });

        var records = await storage.QueryTrainReadyRecordsAsync();
        var trainableExamples = await storage.QueryExamplesAsync(new DesignConversionDpoStorageQuery(TrainableOnly: true));

        Assert.True(records.IsSuccess, records.ErrorMessage);
        Assert.Empty(records.Value!);
        Assert.True(trainableExamples.IsSuccess, trainableExamples.ErrorMessage);
        Assert.Empty(trainableExamples.Value!);
    }

    private static async Task Save(IDesignConversionDpoStorage storage, DesignConversionStoredDpoExample example)
    {
        var result = await storage.SaveExampleAsync(example);
        Assert.True(result.IsSuccess, result.ErrorMessage);
    }

    private static async Task Save(
        IDesignConversionLocalArtifactDb db,
        DesignConversionLocalArtifactDbRecord record)
    {
        var result = await db.AppendRecordAsync(record);
        Assert.True(result.IsSuccess, result.ErrorMessage);
    }

    private sealed class CountingLocalOnlyPromotionUploader : IDesignConversionPromotionUploader
    {
        private int _count;

        public int Count => Interlocked.CompareExchange(ref _count, 0, 0);

        public Task<OperationResult<DesignConversionPromotionUploadReceipt>> UploadAsync(
            DesignConversionTrainingManifest manifest,
            DesignConversionPromotionGateReport gateReport,
            CancellationToken ct = default)
        {
            Interlocked.Increment(ref _count);
            return Task.FromResult(OperationResult<DesignConversionPromotionUploadReceipt>.Ok(
                new DesignConversionPromotionUploadReceipt(
                    UploadId: $"{manifest.ManifestId}:local-only",
                    RemoteRef: DesignConversionPromotedLocalModelLoader.LocalOnlyPromotionRefPrefix +
                               Uri.EscapeDataString(manifest.ManifestId),
                    UploadedAtUtc: DateTimeOffset.UtcNow,
                    EvidenceRefs:
                    [
                        "DESIGN_CONVERSION_TRAINING_INTAKE_LOCAL_ONLY_PROMOTION_UPLOADER_CALLED",
                        "EXTERNAL_CLOUD_UPLOAD_NOT_ATTEMPTED",
                        "DESIGN_CONVERSION_TRAINING_INTAKE_EXTERNAL_CLOUD_UPLOAD_NOT_ATTEMPTED"
                    ])
                {
                    PublicationKind = DesignConversionPromotionPublicationKind.LocalOnlyNoRemoteAttempt,
                    UploadAttempted = false,
                    UploadSucceeded = false,
                    VerificationSucceeded = false,
                    PublicationStatus = DesignConversionPromotionPublicationStatus.NotUploaded
                }));
        }
    }

    private static string[] EnumerateRecordsRewriteTempFiles(string root)
        => Directory.Exists(root)
            ? Directory.EnumerateFiles(root, "conversion-results.jsonl.*.rewrite.tmp").ToArray()
            : [];

    private static string[] EnumerateCopyOnWriteTempFiles(string root)
        => Directory.Exists(root)
            ? Directory.EnumerateFiles(root, "*.copyonwrite.tmp", SearchOption.AllDirectories).ToArray()
            : [];

    private static async Task<string[]> ReadRecordIdsFromJsonlAsync(string path)
    {
        var lines = await File.ReadAllLinesAsync(path);
        return lines
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line =>
            {
                var record = System.Text.Json.JsonSerializer.Deserialize<DesignConversionLocalArtifactDbRecord>(
                    line,
                    DesignConversionCheckpointJsonOptions.Standard);
                Assert.NotNull(record);
                return record!.RecordId;
            })
            .ToArray();
    }

    private static DesignConversionDpoTrainingRecord BuildTrainingRecord(
        string id,
        string unitId,
        double usefulness)
    {
        var build = new DesignConversionDpoGenerator().BuildTrainReadyRecords(
            [DesignConversionDpoGeneratorTests.CreateExample(id, unitId, usefulness)]);
        Assert.True(build.IsSuccess, build.ErrorMessage);
        Assert.Empty(build.Value!.QuarantinedRecords);
        return Assert.Single(build.Value.AcceptedRecords);
    }

    private static DesignConversionDpoTrainingRecord WithSourceIdentity(
        DesignConversionDpoTrainingRecord record,
        string sourceId)
    {
        var metadata = new Dictionary<string, string>(record.Provenance.Metadata, StringComparer.Ordinal)
        {
            ["source_design_id"] = sourceId
        };

        return record with
        {
            Provenance = record.Provenance with
            {
                SourceRequestId = $"request://design-conversion/{sourceId}",
                SourceManifestRef = $"manifest://design-conversion/{sourceId}",
                Metadata = metadata
            }
        };
    }

    private static DesignConversionDpoTrainingRecord WithRuntimeArtifactProofRefs(
        DesignConversionDpoTrainingRecord record,
        string id)
        => record with
        {
            Chosen = record.Chosen with
            {
                QaResults =
                [
                    BuildRuntimeQaResult(
                        $"{id}-chosen",
                        $"sha256:{id}-chosen-expected",
                        $"sha256:{id}-chosen-actual")
                ]
            },
            Rejected = record.Rejected with
            {
                QaResults =
                [
                    BuildRuntimeQaResult(
                        $"{id}-rejected",
                        $"sha256:{id}-rejected-expected",
                        $"sha256:{id}-rejected-actual")
                ]
            }
        };

    private static RenderQaResultRecord BuildRuntimeQaResult(
        string id,
        string expectedSha,
        string actualSha)
    {
        var resolution = new RenderQaResolution("desktop", 1440, 900, 1);
        var expected = new RenderQaImageRecord(
            ImageId: $"{id}:expected",
            Label: "expected-design-render",
            Resolution: resolution,
            Uri: $"artifact://design-conversion/{id}/expected.png",
            Sha256: expectedSha,
            Metadata: new Dictionary<string, string>
            {
                ["role"] = "expected_design_render"
            });
        var actual = new RenderQaImageRecord(
            ImageId: $"{id}:actual",
            Label: "actual-browser-render",
            Resolution: resolution,
            Uri: $"artifact://design-conversion/{id}/actual.png",
            Sha256: actualSha,
            Metadata: new Dictionary<string, string>
            {
                ["role"] = "actual_browser_render"
            });
        var verdict = new VisualVerdict(
            VerdictId: $"{id}:comparison",
            Score: 0.91,
            Defects: [],
            EvidenceRefs: [$"artifact://design-conversion/{id}/diff.png"],
            Passed: true);

        return new RenderQaResultRecord(
            ResultId: $"{id}:render-qa",
            RequestId: $"{id}:request",
            Expected: expected,
            Actual: actual,
            Verdict: verdict,
            Metrics: new Dictionary<string, double>
            {
                ["visual_distance"] = 0.09
            },
            Warnings: []);
    }

    private static DesignConversionDpoTrainingRecord ToRuntimePendingCandidate(
        DesignConversionDpoTrainingRecord record)
    {
        var metadata = new Dictionary<string, string>(record.Provenance.Metadata, StringComparer.Ordinal)
        {
            ["runtime_created_dpo_candidate"] = bool.TrueString,
            ["approval_required"] = bool.TrueString,
            ["review_required"] = bool.TrueString,
            ["approval_status"] = "pending",
            ["review_status"] = "runtime-candidate-review-pending",
            ["approved_for_training"] = bool.FalseString,
            ["training_candidate_only"] = bool.TrueString,
            ["production_dpo"] = bool.FalseString,
            ["product_not_ready"] = bool.TrueString,
            ["model_readiness_claimed"] = bool.FalseString,
            ["held_out_not_proven"] = bool.TrueString,
            ["source_disjoint_not_proven"] = bool.TrueString,
            ["split"] = "runtime_training_candidate_review_pending"
        };
        var licenseMetadata = new Dictionary<string, string>(record.License.Metadata, StringComparer.Ordinal)
        {
            ["approval_required"] = bool.TrueString,
            ["review_required"] = bool.TrueString,
            ["approval_status"] = "pending",
            ["approved_for_training"] = bool.FalseString,
            ["product_not_ready"] = bool.TrueString,
            ["model_readiness_claimed"] = bool.FalseString,
            ["held_out_not_proven"] = bool.TrueString,
            ["source_disjoint_not_proven"] = bool.TrueString
        };

        return record with
        {
            Provenance = record.Provenance with
            {
                Metadata = metadata
            },
            License = record.License with
            {
                Metadata = licenseMetadata
            }
        };
    }

    private static DesignConversionDpoTrainingRecord ToRuntimeApprovedCandidate(
        DesignConversionDpoTrainingRecord record,
        bool includeReviewer = true,
        bool includeReason = true)
    {
        var metadata = new Dictionary<string, string>(record.Provenance.Metadata, StringComparer.Ordinal)
        {
            ["runtime_created_dpo_candidate"] = bool.TrueString,
            ["approval_required"] = bool.TrueString,
            ["review_required"] = bool.TrueString,
            ["approval_status"] = "approved",
            ["review_status"] = "runtime-candidate-review-approved",
            ["approved_for_training"] = bool.TrueString,
            ["approved_at_utc"] = DateTimeOffset.UnixEpoch.AddMinutes(107).ToString("O"),
            ["training_candidate_only"] = bool.TrueString,
            ["production_dpo"] = bool.FalseString,
            ["product_not_ready"] = bool.TrueString,
            ["model_readiness_claimed"] = bool.FalseString,
            ["trained_model_ready"] = bool.FalseString,
            ["held_out_not_proven"] = bool.TrueString,
            ["source_disjoint_not_proven"] = bool.TrueString,
            ["split"] = "runtime_training_candidate_review_approved"
        };
        if (includeReviewer)
            metadata["approved_by_ref"] = "reviewer:pass107-local";
        if (includeReason)
            metadata["approval_reason"] = "local approved candidate handoff";

        var licenseMetadata = new Dictionary<string, string>(record.License.Metadata, StringComparer.Ordinal)
        {
            ["approval_required"] = bool.TrueString,
            ["review_required"] = bool.TrueString,
            ["approval_status"] = "approved",
            ["approved_for_training"] = bool.TrueString,
            ["product_not_ready"] = bool.TrueString,
            ["model_readiness_claimed"] = bool.FalseString,
            ["trained_model_ready"] = bool.FalseString,
            ["held_out_not_proven"] = bool.TrueString,
            ["source_disjoint_not_proven"] = bool.TrueString
        };

        return record with
        {
            Provenance = record.Provenance with
            {
                ReviewerRef = includeReviewer ? "reviewer:pass107-local" : "",
                Metadata = metadata
            },
            License = record.License with
            {
                Metadata = licenseMetadata
            }
        };
    }

    private static DesignConversionMeasurementGateRequest BuildPassingMeasurementGateRequest(string gateId)
        => new(
            GateId: gateId,
            UnitId: DesignConversionDpoUnits.CriticUnit,
            HeldOutMetrics: new DesignConversionHeldOutMeasurementMetrics(
                HeldOutRecordCount: 1,
                HeldOutQualityScore: 0.94,
                RuntimeMs: 320,
                CostUsd: 0.25m,
                PeakRamMb: 256,
                EvidenceRefs: [$"{gateId}:heldout:pass"]),
            IntegrityProof: new DesignConversionMeasurementIntegrityProof(
                SourceDisjoint: true,
                LeakageDetected: false,
                HiddenDeterminismDetected: false,
                AblationPassed: true,
                EvidenceRefs: [$"{gateId}:integrity:pass"]),
            Thresholds: new DesignConversionMeasurementGateThresholds(
                RequiredHeldOutQualityScore: 0.90,
                MaxRuntimeMs: 800,
                MaxCostUsd: 1.00m,
                MaxPeakRamMb: 512));

    private static DesignConversionMeasurementGateRequest BuildFailingMeasurementGateRequest(string gateId)
        => BuildPassingMeasurementGateRequest(gateId) with
        {
            HeldOutMetrics = new DesignConversionHeldOutMeasurementMetrics(
                HeldOutRecordCount: 1,
                HeldOutQualityScore: 0.81,
                RuntimeMs: 320,
                CostUsd: 0.25m,
                PeakRamMb: 256,
                EvidenceRefs: [$"{gateId}:heldout:below-threshold"])
        };

    private static DesignConversionTrainingPipelineOptions Pass157LocalPromotionOptions(
        string runId,
        DesignConversionMeasurementGateRequest measurementGate)
        => new(
            RunId: runId,
            BatchSize: 1,
            DatasetOptions: new DesignConversionDpoDatasetOptions(
                DatasetId: runId,
                MinimumTrainRecords: 1,
                MinimumHeldOutRecords: 1,
                Preflight: DesignConversionDpoDatasetPreflight.LocalTrainingIntakeSourceManifestDisjointProof(
                    [$"{runId}:controlled_source_disjoint_intake"])),
            MeasurementGateRequest: measurementGate);

    private static async Task<string> ReadSha256Async(string path)
    {
        var bytes = await File.ReadAllBytesAsync(path);
        return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }

    private static async Task<KeyValuePair<string, string>[]> ReadFileSha256SnapshotAsync(string root)
    {
        if (!Directory.Exists(root))
            return [];

        var snapshot = new List<KeyValuePair<string, string>>();
        foreach (var path in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
                     .OrderBy(path => path, StringComparer.Ordinal))
        {
            var relativePath = Path.GetRelativePath(root, path)
                .Replace(Path.DirectorySeparatorChar, '/')
                .Replace(Path.AltDirectorySeparatorChar, '/');
            snapshot.Add(new KeyValuePair<string, string>(relativePath, await ReadSha256Async(path)));
        }

        return snapshot
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .ToArray();
    }

    private static void AssertDoesNotContainRawPrivatePayload(string payload)
    {
        Assert.DoesNotContain("<main", payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".private-user", payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/Users/", payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("exact_prompt", payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("private.png", payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("external-provider-payload", payload, StringComparison.OrdinalIgnoreCase);
    }

    private static async Task WritePass189RPromotionManifestIfRequestedAsync(
        string testRoot,
        DesignConversionTrainingIntakeLocalPromotionResult promoted,
        DesignConversionPromotedLocalModelLoadResult promotedLoad,
        DesignConversionModelServerApiResult response,
        DesignConversionModelServerStatusResponse status)
    {
        var manifestPath = Environment.GetEnvironmentVariable("DESIGN_CONVERSION_CAPTURE_PROMOTION_MANIFEST_PATH");
        if (string.IsNullOrWhiteSpace(manifestPath))
            return;

        var fullManifestPath = Path.GetFullPath(manifestPath);
        var manifestDirectory = Path.GetDirectoryName(fullManifestPath);
        if (!string.IsNullOrWhiteSpace(manifestDirectory))
            Directory.CreateDirectory(manifestDirectory);

        var artifacts = promoted.Publication!.Manifest.PromotionArtifacts!;
        var manifest = new Dictionary<string, object?>
        {
            ["schema_id"] = "design-conversion-pass189r-real-checkpoint-producer-manifest-v1",
            ["produced_at_utc"] = DateTimeOffset.UtcNow,
            ["test_name"] = nameof(Pass157_LocalArtifactDb_UserRequestedRetrainPromotesLocalModelServerCheckpointOnlyAfterGates),
            ["evidence_scope"] = "bounded-local-focused-test",
            ["promotion_directory_class"] = "pass157-test-temp-ready-promotion",
            ["promotion_directory_relative_locator"] = Path.GetRelativePath(testRoot, promoted.PromotionOutputDirectory),
            ["checkpoint_relative_locator"] = Path.GetRelativePath(testRoot, promoted.PromotedCheckpointPath),
            ["checkpoint_file_name"] = Path.GetFileName(promoted.PromotedCheckpointPath),
            ["tokenizer_file_name"] = Path.GetFileName(artifacts.TokenizerPath),
            ["policy_file_name"] = Path.GetFileName(artifacts.PolicyPath),
            ["artifacts"] = new[]
            {
                await BuildPass189RArtifactManifestItemAsync("checkpoint", promoted.PromotedCheckpointPath, testRoot),
                await BuildPass189RArtifactManifestItemAsync("tokenizer", artifacts.TokenizerPath, testRoot),
                await BuildPass189RArtifactManifestItemAsync("policy", artifacts.PolicyPath, testRoot)
            },
            ["candidate_and_training_counts"] = new Dictionary<string, object?>
            {
                ["accepted_candidate_count"] = promoted.TrainingRun.AcceptedCandidateCount,
                ["resolved_training_record_count"] = promoted.TrainingRun.ResolvedTrainingRecordCount,
                ["required_train_records"] = promoted.TrainingRun.RunnerEvidence?.RequiredTrainRecords,
                ["required_held_out_records"] = promoted.TrainingRun.RunnerEvidence?.RequiredHeldOutRecords,
                ["actual_train_records"] = promoted.TrainingRun.RunnerEvidence?.ActualTrainRecords,
                ["actual_held_out_records"] = promoted.TrainingRun.RunnerEvidence?.ActualHeldOutRecords,
                ["dataset_count_gate_passed"] = promoted.TrainingRun.RunnerEvidence?.DatasetCountGatePassed,
                ["source_disjoint"] = promoted.TrainingRun.RunnerEvidence?.SourceDisjoint
            },
            ["fresh_load_success"] = promotedLoad.Model.CheckpointFreshLoadVerified,
            ["fresh_load_contains_real_trained_weights"] = promotedLoad.Model.CheckpointContainsRealTrainedWeights,
            ["checkpoint_sha256_from_loader"] = promotedLoad.Model.CheckpointSha256,
            ["checkpoint_machine_id"] = promotedLoad.Model.MachineId,
            ["checkpoint_domain_tag"] = promotedLoad.Model.DomainTag,
            ["checkpoint_version"] = promotedLoad.Model.Version,
            ["learned_weight_artifact_count"] = promotedLoad.Model.LearnedWeightArtifactCount,
            ["learned_weight_observation_count"] = promotedLoad.Model.LearnedWeightObservationCount,
            ["deterministic_logic_claimed_as_learning"] = promoted.Publication.Manifest.DeterministicLogicClaimedAsLearning,
            ["model_server_response_success"] = response.LearnedInferenceApplied && response.ModelTrainingVerified,
            ["model_server_status_success"] = status.LearnedInferenceApplied,
            ["product_readiness_exclusions"] = new[]
            {
                "bounded_local_focused_test_only",
                "product_ready_not_claimed",
                "production_checkpoint_readiness_not_claimed",
                "production_db_not_claimed",
                "cloud_upload_not_attempted",
                "provider_execution_not_attempted",
                "deploy_public_endpoint_not_attempted",
                "ci_as_a_whole_not_claimed",
                "commit_push_merge_pr_not_claimed"
            },
            ["source_evidence_refs"] = new[]
            {
                "docs/evidence/design-conversion/codex-pass157-retrain-promotion-boundary.md",
                "docs/evidence/design-conversion/codex-pass187-training-artifact-reopen-manifest.md",
                "docs/evidence/design-conversion/codex-pass188-reusable-training-artifact-payloads.md",
                "docs/evidence/design-conversion/codex-pass189-real-checkpoint-producer-capture.md"
            },
            ["privacy_boundary"] = new Dictionary<string, object?>
            {
                ["raw_html_included"] = false,
                ["raw_css_included"] = false,
                ["raw_design_tree_included"] = false,
                ["raw_user_private_data_included"] = false,
                ["absolute_temp_paths_included"] = false
            }
        };

        var json = System.Text.Json.JsonSerializer.Serialize(
            manifest,
            new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower
            }).Replace("\r\n", "\n", StringComparison.Ordinal);
        await File.WriteAllTextAsync(fullManifestPath, json);
    }

    private static async Task<Dictionary<string, object?>> BuildPass189RArtifactManifestItemAsync(
        string role,
        string path,
        string testRoot)
    {
        var exists = File.Exists(path);
        var info = exists ? new FileInfo(path) : null;
        return new Dictionary<string, object?>
        {
            ["role"] = role,
            ["file_name"] = Path.GetFileName(path),
            ["relative_locator"] = Path.GetRelativePath(testRoot, path),
            ["exists_at_capture"] = exists,
            ["size_bytes"] = info?.Length,
            ["sha256"] = exists ? await ReadSha256Async(path) : null
        };
    }

    private static DesignConversionModelServerApiRequest CreatePass157Request(string requestId)
        => new(
            RequestId: requestId,
            SourceKind: DesignConversionSourceKind.Figma,
            DesignTree: new DesignConversionModelServerPayloadRef(
                RefId: "pass157-figma-tree-ref",
                Kind: "figma-json",
                Sha256: "sha256:pass157-figma-tree",
                Payload: "{\"type\":\"FRAME\",\"name\":\"safe\"}",
                Format: "figma-json"),
            OriginalTree: new DesignConversionModelServerPayloadRef(
                RefId: "pass157-original-tree-ref",
                Kind: "figma-json",
                Sha256: "sha256:pass157-original-tree",
                Payload: "{\"type\":\"FRAME\",\"name\":\"original\"}",
                Format: "figma-json"),
            Html: "<main class=\"screen\"><h1>Dashboard</h1></main>",
            Css: ".screen{display:grid;gap:16px;}",
            Screenshots:
            [
                new DesignConversionModelServerArtifactRef(
                    ArtifactId: "pass157-screenshot-desktop",
                    Kind: "screenshot",
                    Width: 1440,
                    Height: 900,
                    Uri: "artifact://pass157/screenshot-desktop.png",
                    Sha256: "sha256:pass157-screenshot-desktop")
            ],
            DesignRenders:
            [
                new DesignConversionModelServerArtifactRef(
                    ArtifactId: "pass157-design-render-desktop",
                    Kind: "design-render",
                    Width: 1440,
                    Height: 900,
                    Uri: "artifact://pass157/design-render-desktop.png",
                    Sha256: "sha256:pass157-design-render-desktop")
            ],
            Budget: new DesignConversionModelServerBudget(
                UserId: "user-redacted",
                Tier: "local",
                MaxImprovementCandidates: 2,
                MaxRenderQaRuns: 1,
                BudgetUsd: 0m),
            Metadata: new Dictionary<string, string>
            {
                ["source_manifest_ref"] = "pass157-local-source-manifest",
                ["caller"] = "pass157-local-modelserver-proof"
            });

    private static VisualVerdict CreatePass157Verdict(string verdictId, double score, bool passed)
        => new(
            VerdictId: verdictId,
            Score: score,
            Defects: passed
                ? []
                :
                [
                    new VisualDefect(
                        DefectId: $"{verdictId}:spacing",
                        Category: "spacing",
                        Severity: "major",
                        Description: "Spacing differs from expected render.",
                        X: 16,
                        Y: 24,
                        Width: 80,
                        Height: 20,
                        EvidenceImageIds: ["expected", "actual"])
                ],
            EvidenceRefs: [$"verdict:{verdictId}"],
            Passed: passed);

    private sealed class Pass157RecordingRenderProvider : IDesignConversionRenderProvider
    {
        public Task<OperationResult<DesignConversionRenderResult>> RenderAsync(
            DesignConversionRenderRequest request,
            CancellationToken cancellationToken = default)
        {
            var image = new RenderQaImageRecord(
                ImageId: $"{request.RenderRequestId}:actual",
                Label: "actual-render",
                Resolution: request.Resolution,
                Uri: $"artifact://pass157/render/{request.RenderRequestId}.png",
                Sha256: $"sha256:{request.RenderRequestId}:actual",
                Metadata: request.Metadata);

            return Task.FromResult(OperationResult<DesignConversionRenderResult>.Ok(new DesignConversionRenderResult(
                RenderResultId: $"{request.RenderRequestId}:result",
                RenderRequestId: request.RenderRequestId,
                Image: image,
                ElapsedMs: 7,
                Warnings: ["pass157-recording-render-provider"])));
        }
    }

    private sealed class Pass157RecordingComparisonProvider(VisualVerdict verdict) : IDesignConversionComparisonProvider
    {
        public Task<OperationResult<DesignConversionQaComparisonArtifact>> CompareAsync(
            DesignConversionQaComparisonRequest request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(OperationResult<DesignConversionQaComparisonArtifact>.Ok(new DesignConversionQaComparisonArtifact(
                ArtifactId: $"{request.ComparisonRequestId}:artifact",
                ComparisonRequestId: request.ComparisonRequestId,
                Expected: request.Expected,
                Actual: request.Actual,
                Verdict: verdict,
                DiffImageUri: $"artifact://pass157/diff/{request.ComparisonRequestId}.png",
                DiffImageSha256: $"sha256:{request.ComparisonRequestId}:diff",
                Metrics: new Dictionary<string, double> { ["visual_distance"] = 1.0 - verdict.Score },
                EvidenceRefs: ["pass157-comparison-provider"])));
    }

    private static DesignConversionLocalArtifactDbRecord BuildArtifactRecord(
        string recordId,
        string jobId,
        string requestId,
        string visualJudgeStatus,
        IReadOnlyList<DesignConversionDpoTrainingRecord> records,
        string html,
        string css)
        => new(
            RecordId: recordId,
            DatabaseKind: "local_artifact_db",
            JobId: jobId,
            RequestId: requestId,
            SourceKind: DesignConversionSourceKind.Figma.ToString(),
            Tier: "pro",
            SanitizedSourceRefs: new Dictionary<string, string>
            {
                ["source_manifest_ref"] = "file:///C:/Users/private-user/Documents/private.fig",
                ["source_kind"] = "figma"
            },
            ConversionResult: new DesignConversionConversionResultArtifact(
                ResultId: $"{recordId}:conversion-result",
                HtmlRef: html,
                HtmlSha256: string.Empty,
                CssRef: css,
                CssSha256: string.Empty,
                Metadata: new Dictionary<string, string>
                {
                    ["conversion_result"] = "test"
                }),
            RenderArtifactRefs:
            [
                new DesignConversionRenderArtifactDbRef(
                    ViewportId: "desktop",
                    ArtifactId: $"{recordId}:render:desktop",
                    Kind: "browser_screenshot",
                    UriRef: "file:///C:/Users/private-user/private/render-desktop.png",
                    Sha256: "sha256:render-desktop",
                    ByteLength: 1234,
                    Width: 1440,
                    Height: 900,
                    Metadata: new Dictionary<string, string>
                    {
                        ["render_artifact_refs"] = "browser_png"
                    })
            ],
            VisualJudgeStatus: new DesignConversionVisualJudgeArtifactDbStatus(
                Status: visualJudgeStatus,
                ProviderBoundary: visualJudgeStatus == DesignConversionVisualQaBoundaryPolicy.ProviderRequired
                    ? DesignConversionVisualQaBoundaryPolicy.ProviderRequired
                    : DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                ExternalCallExecuted: false,
                EstimatedCostUsd: 0.01m,
                TierDecision: visualJudgeStatus == DesignConversionVisualQaBoundaryPolicy.ProviderRequired
                    ? DesignConversionVisualQaBoundaryPolicy.ProviderRequired
                    : DesignConversionVisualQaBoundaryPolicy.CostCapMarker,
                EvidenceRefs:
                [
                    "visual_judge_status",
                    visualJudgeStatus,
                    DesignConversionVisualQaBoundaryPolicy.ProductNotReadyMarker
                ],
                Metadata: new Dictionary<string, string>
                {
                    ["visual_judge_status"] = visualJudgeStatus
                }),
            DpoCandidates: records
                .Select(record => DesignConversionLocalArtifactDbDpoCandidate.FromTrainingRecord(record))
                .ToArray(),
            CreatedAtUtc: DateTimeOffset.UnixEpoch,
            EvidenceRefs:
            [
                "storage_training_artifact_db",
                "local_artifact_db",
                "conversion_result",
                "render_artifact_refs",
                "visual_judge_status",
                "dpo_candidate",
                "usefulness_score",
                "quarantine",
                "training_candidate_only",
                "product_not_ready"
            ],
            Metadata: new Dictionary<string, string>
            {
                ["remaining_queue"] = "real_provider_approval_training_upload"
            });
}
