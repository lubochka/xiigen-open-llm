// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

using System.Text.Json;
using DesignConversionDomain;

public sealed class DesignConversionOptimizerPromptIsolationTests
{
    [Fact]
    public void CanonicalPrompt_ContainsOnlySemanticAllowlistedSchema()
    {
        var record = CreateRecord();
        var hostileMetadata = new Dictionary<string, string>(record.Provenance.Metadata, StringComparer.Ordinal)
        {
            ["source_encoder_feature_names"] =
                "source_json_node_count,source_layout_hint_count,checkpoint_ref,reviewerScore,artifactDigest,grid_density",
            ["source_structure_signal_summary"] =
                "nodes=12;depth=4;properties=9;checkpoint=777;visual_score=1;artifactDigest=42",
            ["source_encoder_shortcut_guard"] = "checkpoint://attacker/guard"
        };
        record = record with
        {
            SourceStructurePayload = """
                {
                  "type":"FRAME",
                  "name":"semantic-card",
                  "grid":"12-column",
                  "profile":"compact-ui",
                  "manifestation":"visible-animation",
                  "manifest":"OPAQUE-MANIFEST",
                  "sourceIdentity":"OPAQUE-IDENTITY",
                  "payload-checksum":"OPAQUE-CHECKSUM",
                  "artifact.digest":"OPAQUE-DIGEST",
                  "nested":{
                    "manifestUri":"manifest://private/source",
                    "payloadChecksum":"0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef",
                    "artifactDigest":"sha256:abcdefabcdefabcdefabcdefabcdefabcdefabcdefabcdefabcdefabcdefabcd",
                    "note":"artifact://private/note",
                    "description":"https://private.example/source",
                    "local":"C:/private/source.fig",
                    "children":[{"type":"TEXT","characters":"Ordinary UI copy"}]
                  }
                }
                """,
            Provenance = record.Provenance with { Metadata = hostileMetadata }
        };
        var prompt = DesignConversionDpoDatasetBuilder.BuildOptimizerPromptPayload(
            record,
            DesignConversionMachine.DomainTag);

        using var document = JsonDocument.Parse(prompt);
        var root = document.RootElement;
        Assert.Equal(
            ["domain_tag", "unit_id", "source_kind", "internal_vector", "target", "source", "source_encoder"],
            root.EnumerateObject().Select(property => property.Name).ToArray());
        Assert.Equal(
            ["unit_id", "machine_part"],
            root.GetProperty("target").EnumerateObject().Select(property => property.Name).ToArray());
        Assert.Equal(
            ["kind", "structure_payload"],
            root.GetProperty("source").EnumerateObject().Select(property => property.Name).ToArray());
        Assert.Equal(
            ["feature_names", "structure_summary", "vector_prefix", "dimensions", "shortcut_guard"],
            root.GetProperty("source_encoder").EnumerateObject().Select(property => property.Name).ToArray());

        Assert.Equal("figma", root.GetProperty("source").GetProperty("kind").GetString());
        Assert.Contains("semantic-card", root.GetProperty("source").GetProperty("structure_payload").GetString(), StringComparison.Ordinal);
        Assert.Contains("12-column", root.GetProperty("source").GetProperty("structure_payload").GetString(), StringComparison.Ordinal);
        Assert.Contains("compact-ui", root.GetProperty("source").GetProperty("structure_payload").GetString(), StringComparison.Ordinal);
        Assert.Contains("visible-animation", root.GetProperty("source").GetProperty("structure_payload").GetString(), StringComparison.Ordinal);
        Assert.Contains("Ordinary UI copy", root.GetProperty("source").GetProperty("structure_payload").GetString(), StringComparison.Ordinal);
        var featureNames = root.GetProperty("source_encoder").GetProperty("feature_names")
            .EnumerateArray().Select(item => item.GetString()).ToArray();
        Assert.Equal(["source_json_node_count", "source_layout_hint_count"], featureNames);
        Assert.Equal(
            "nodes=12;depth=4;properties=9",
            root.GetProperty("source_encoder").GetProperty("structure_summary").GetString());
        Assert.Equal(
            "source_kind_source_id_not_vector_features",
            root.GetProperty("source_encoder").GetProperty("shortcut_guard").GetString());
        Assert.DoesNotContain("result_ref", prompt, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("identity", prompt, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("checkpoint", prompt, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("reviewer", prompt, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("score", prompt, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("verdict", prompt, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("accepted", prompt, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("output", prompt, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("OPAQUE-ID", prompt, StringComparison.Ordinal);
        Assert.DoesNotContain("OPAQUE-MANIFEST", prompt, StringComparison.Ordinal);
        Assert.DoesNotContain("OPAQUE-IDENTITY", prompt, StringComparison.Ordinal);
        Assert.DoesNotContain("OPAQUE-CHECKSUM", prompt, StringComparison.Ordinal);
        Assert.DoesNotContain("OPAQUE-DIGEST", prompt, StringComparison.Ordinal);
        Assert.DoesNotContain("manifest://private/source", prompt, StringComparison.Ordinal);
        Assert.DoesNotContain("artifact://private/note", prompt, StringComparison.Ordinal);
        Assert.DoesNotContain("https://private.example/source", prompt, StringComparison.Ordinal);
        Assert.DoesNotContain("C:/private/source.fig", prompt, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef", prompt, StringComparison.Ordinal);
        Assert.DoesNotContain("abcdefabcdefabcdefabcdefabcdefabcdefabcdefabcdefabcdefabcdefabcd", prompt, StringComparison.Ordinal);
        Assert.DoesNotContain("checkpoint://attacker/guard", prompt, StringComparison.Ordinal);
        Assert.DoesNotContain("reviewerScore", prompt, StringComparison.Ordinal);
        Assert.DoesNotContain("artifactDigest", prompt, StringComparison.Ordinal);
        Assert.DoesNotContain("C:/", prompt, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CanonicalPrompt_IsInvariantToCustodyOutcomeAndOpaqueIdentityChanges()
    {
        var record = CreateRecord();
        var baseline = Prompt(record);
        var changedMetadata = new Dictionary<string, string>(record.Provenance.Metadata, StringComparer.Ordinal)
        {
            ["source_design_id"] = "different-design-identity",
            ["source_encoder_vector_hash"] = "different-vector-hash",
            ["checkpoint_ref"] = "checkpoint://different",
            ["reviewer_verdict"] = "different"
        };
        var mutated = record with
        {
            RecordId = "different-record-id",
            InternalUnitStatePayload = "{\"result_ref\":\"different\",\"verdict\":\"failed\"}",
            Chosen = record.Chosen with
            {
                OutputId = "different-output-id",
                CandidateId = "different-candidate-id",
                Accepted = false,
                Verdict = record.Chosen.Verdict with { Score = 0.01 }
            },
            Rejected = record.Rejected with
            {
                OutputId = "another-output-id",
                CandidateId = "another-candidate-id",
                Accepted = true,
                Verdict = record.Rejected.Verdict with { Score = 0.99 }
            },
            Provenance = record.Provenance with
            {
                SourceRequestId = "different-request-id",
                SourceManifestRef = "manifest://different",
                ReviewerRef = "reviewer://different",
                Metadata = changedMetadata
            }
        };

        Assert.Equal(baseline, Prompt(mutated));
    }

    [Fact]
    public void CanonicalPrompt_ChangesOnlyForLegitimateSemanticInputs()
    {
        var record = CreateRecord();
        var baseline = Prompt(record);

        Assert.NotEqual(baseline, Prompt(record with
        {
            Provenance = record.Provenance with { SourceKind = DesignConversionSourceKind.Canva.ToString() }
        }));
        Assert.NotEqual(baseline, Prompt(record with
        {
            SourceStructurePayload = "{\"type\":\"FRAME\",\"name\":\"semantic-hero\",\"children\":[{\"type\":\"TEXT\"}]}"
        }));
        Assert.NotEqual(baseline, Prompt(record with
        {
            InternalVectorPayload = [0.125, 0.5, 0.875]
        }));
        Assert.NotEqual(baseline, Prompt(record with
        {
            UnitId = DesignConversionDpoUnits.CriticUnit
        }));
        var summaryMetadata = new Dictionary<string, string>(record.Provenance.Metadata, StringComparer.Ordinal)
        {
            ["source_structure_signal_summary"] = "nodes=99;depth=7;properties=101"
        };
        Assert.NotEqual(baseline, Prompt(record with
        {
            Provenance = record.Provenance with { Metadata = summaryMetadata }
        }));
    }

    [Fact]
    public void DatasetBuilderAndPipeline_UseByteIdenticalPromptWhileSidecarCustodyChanges()
    {
        var record = CreateRecord();
        var directPrompt = Prompt(record);
        var first = Assert.Single(new DesignConversionTrainingPipeline().BuildBatches(
            [record],
            PipelineOptions()).Value!);
        Assert.Equal(directPrompt, Assert.Single(first.TrainingItems).Prompt);
        Assert.Contains(record.Provenance.SourceManifestRef, first.SourceIds);

        var identityChanged = record with
        {
            RecordId = "different-record-id",
            Provenance = record.Provenance with { SourceManifestRef = "manifest://different-custody" }
        };
        var second = Assert.Single(new DesignConversionTrainingPipeline().BuildBatches(
            [identityChanged],
            PipelineOptions()).Value!);
        Assert.Equal(directPrompt, Assert.Single(second.TrainingItems).Prompt);
        Assert.NotEqual(first.SourceIds, second.SourceIds);
        Assert.Contains("manifest://different-custody", second.SourceIds);
    }

    [Fact]
    public void RuntimeGenerator_UsesCanonicalRedactedSourceStructureAndDropsSubmittedContentOrReferences()
    {
        const string semanticContent = "{\"type\":\"FRAME\",\"name\":\"runtime-semantic-card\",\"result_ref\":\"OUTCOME-REF\",\"design_tree_hash\":\"OPAQUE-HASH\",\"children\":[{\"type\":\"TEXT\",\"characters\":\"Hello\"}]}";
        var evaluation = DesignConversionDpoGeneratorTests.CreateServerOwnedSingleEvaluation();
        evaluation = evaluation with
        {
            SubmittedRequest = evaluation.SubmittedRequest with
            {
                DesignTreePayload = evaluation.SubmittedRequest.DesignTreePayload with
                {
                    Content = semanticContent,
                    Sha256 = "different-design-tree-hash"
                }
            }
        };
        var result = new DesignConversionDpoGenerator()
            .BuildRuntimeCreatedTrainingCandidates(evaluation);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var record = Assert.Single(result.Value!.AcceptedRecords);
        Assert.Empty(result.Value.QuarantinedRecords);
        Assert.False(record.Quarantine.IsQuarantined);
        Assert.Equal("runtime-candidate-review-pending", record.Quarantine.Reason);
        Assert.Equal(
            bool.FalseString,
            record.Provenance.Metadata["approved_for_training"]);
        Assert.Equal(
            bool.FalseString,
            record.Provenance.Metadata["training_grade"]);
        Assert.Equal(
            bool.FalseString,
            record.Provenance.Metadata["raw_payloads_included"]);

        var payload = record.SourceStructurePayload;
        using var document = JsonDocument.Parse(payload);
        var root = document.RootElement;
        Assert.Equal(
            [
                "schema",
                "tree",
                "route_admission_tenant_bound",
                "baseline_ref",
                "generated_ref",
                "redacted"
            ],
            root.EnumerateObject().Select(property => property.Name).ToArray());
        Assert.Equal(
            "design-conversion-server-owned-baseline-source-structure-v1",
            root.GetProperty("schema").GetString());
        Assert.Equal(
            ["frame", "candidate"],
            root.GetProperty("tree")
                .EnumerateArray()
                .Select(item => item.GetString()!)
                .ToArray());
        Assert.True(root.GetProperty("route_admission_tenant_bound").GetBoolean());
        Assert.Equal(
            "dcm-runtime-server-owned-baseline-source-4bf1d4237a5b5701",
            root.GetProperty("baseline_ref").GetString());
        Assert.Equal(
            "dcm-runtime-server-owned-baseline-generated-12544a302cf6ba49",
            root.GetProperty("generated_ref").GetString());
        Assert.True(root.GetProperty("redacted").GetBoolean());
        Assert.DoesNotContain("runtime-semantic-card", payload, StringComparison.Ordinal);
        Assert.DoesNotContain("Hello", payload, StringComparison.Ordinal);
        Assert.DoesNotContain("OUTCOME-REF", payload, StringComparison.Ordinal);
        Assert.DoesNotContain("OPAQUE-HASH", payload, StringComparison.Ordinal);
        Assert.DoesNotContain("result_ref", payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("design_tree_hash", payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(evaluation.RuntimeFlowResult.Output.OutputId, payload, StringComparison.Ordinal);
    }

    private static string Prompt(DesignConversionDpoTrainingRecord record)
        => DesignConversionDpoDatasetBuilder.BuildOptimizerPromptPayload(
            record,
            DesignConversionMachine.DomainTag);

    private static DesignConversionDpoTrainingRecord CreateRecord()
    {
        var example = DesignConversionDpoGeneratorTests.CreateExample(
            "semantic-prompt-isolation",
            DesignConversionDpoUnits.ImproverUnit) with
        {
            SourceStructurePayload = "{\"type\":\"FRAME\",\"name\":\"semantic-card\",\"grid\":\"12-column\",\"path\":\"C:/private.fig\",\"result_ref\":\"OUTCOME-REF\",\"outcomeScore\":0.99,\"artifactId\":\"OPAQUE-ID\",\"children\":[{\"type\":\"TEXT\",\"characters\":\"Save\"}]}"
        };
        var build = new DesignConversionDpoGenerator().BuildTrainReadyRecords([example]);
        Assert.True(build.IsSuccess, build.ErrorMessage);
        return Assert.Single(build.Value!.AcceptedRecords);
    }

    private static DesignConversionTrainingPipelineOptions PipelineOptions()
        => new(
            "semantic-prompt-parity",
            BatchSize: 1,
            CallMachineLifecycleHook: false,
            RequireDatasetCountGate: false,
            DatasetOptions: new DesignConversionDpoDatasetOptions(
                DatasetId: "semantic-prompt-parity",
                MinimumTrainRecords: 1,
                MinimumHeldOutRecords: 1,
                Preflight: DesignConversionDpoDatasetPreflight.LocalFixtureTrainingCandidateOnly()));
}
