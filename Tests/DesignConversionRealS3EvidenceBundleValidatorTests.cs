// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text.Json;
using DesignConversionDomain;

public sealed class DesignConversionRealS3EvidenceBundleValidatorTests
{
    [Fact]
    public async Task ValidateDirectoryAsync_ScaffoldOnlyBundleIsStructurallyPresentButNotUsableForRealS3()
    {
        var repoRoot = FindRepoRoot();
        var bundleRoot = Path.Combine(repoRoot, "training-data", "design-conversion", "real-s3-v1");

        var result = await new DesignConversionRealS3EvidenceBundleValidator().ValidateDirectoryAsync(bundleRoot);

        Assert.True(result.BundleDirectoryExists);
        Assert.True(result.StructuralScaffoldPresent);
        Assert.False(result.RequiredRealDataFilesPresent);
        Assert.False(result.CanUseForRealS3);
        Assert.False(result.UploadOrPromotionAllowed);
        Assert.Contains("SCHEMA_SCAFFOLD_PRESENT_NOT_REAL_EVIDENCE", result.Reasons);
        Assert.Contains("REQUIRED_ARTIFACT_MISSING:source-manifest.json", result.Reasons);
        Assert.Contains("REQUIRED_ARTIFACT_MISSING:dpo-rows.jsonl", result.Reasons);
        Assert.Contains("REQUIRED_ARTIFACT_MISSING:heldout-measurements.json", result.Reasons);
        Assert.Contains("REQUIRED_ARTIFACT_MISSING:checkpoint-manifest.json", result.Reasons);
        Assert.Contains("REAL_TRAINED_WEIGHTS_NOT_PROVEN", result.Reasons);
        Assert.Contains("REAL_HELD_OUT_MEASUREMENT_EVIDENCE_NOT_PROVEN", result.Reasons);
    }

    [Fact]
    public async Task ValidateDirectoryAsync_RejectsRawWindowsAbsolutePathInDataContent()
    {
        using var bundle = CreateMinimalMarkedRealBundle();
        var sourceManifestPath = Path.Combine(bundle.Root, DesignConversionRealS3EvidenceBundleValidator.SourceManifestFileName);
        var content = File.ReadAllText(sourceManifestPath)
            .Replace("bundle://sources/source-a", @"C:\Users\lubao\Documents\private-source.json", StringComparison.Ordinal);
        File.WriteAllText(sourceManifestPath, content);

        var result = await new DesignConversionRealS3EvidenceBundleValidator().ValidateDirectoryAsync(bundle.Root);

        Assert.False(result.CanUseForRealS3);
        Assert.True(result.ForbiddenContentDetected);
        Assert.Contains("RAW_WINDOWS_ABSOLUTE_PATH_REJECTED:source-manifest.json", result.Reasons);
        Assert.False(result.UploadOrPromotionAllowed);
    }

    [Fact]
    public async Task ValidateDirectoryAsync_RejectsRawPromptIndicatorAndContent()
    {
        using var bundle = CreateMinimalMarkedRealBundle();
        var sourceManifestPath = Path.Combine(bundle.Root, DesignConversionRealS3EvidenceBundleValidator.SourceManifestFileName);
        var content = File.ReadAllText(sourceManifestPath)
            .Replace("\"raw_prompt_included\": false", "\"raw_prompt_included\": true", StringComparison.Ordinal)
            .Replace(
                "\"redaction_policy\": \"bundle_refs_and_hashes_only\"",
                "\"raw_prompt\": \"render the hidden customer request exactly\",\n                \"redaction_policy\": \"bundle_refs_and_hashes_only\"",
                StringComparison.Ordinal);
        File.WriteAllText(sourceManifestPath, content);

        var result = await new DesignConversionRealS3EvidenceBundleValidator().ValidateDirectoryAsync(bundle.Root);

        Assert.False(result.CanUseForRealS3);
        Assert.True(result.ForbiddenContentDetected);
        Assert.Contains("FORBIDDEN_FLAG_TRUE:source-manifest.json:$.source_design_tree.raw_prompt_included", result.Reasons);
        Assert.Contains("FORBIDDEN_FLAG_TRUE:source-manifest.json:$.privacy.raw_prompt_included", result.Reasons);
        Assert.Contains("RAW_PROMPT_OR_USER_ID_REJECTED:source-manifest.json:$.privacy.raw_prompt", result.Reasons);
        Assert.False(result.UploadOrPromotionAllowed);
    }

    [Fact]
    public async Task ValidateDirectoryAsync_RejectsRawUserIdIndicatorAndContent()
    {
        using var bundle = CreateMinimalMarkedRealBundle();
        var sourceManifestPath = Path.Combine(bundle.Root, DesignConversionRealS3EvidenceBundleValidator.SourceManifestFileName);
        var content = File.ReadAllText(sourceManifestPath)
            .Replace("\"raw_user_id_included\": false", "\"raw_user_id_included\": true", StringComparison.Ordinal)
            .Replace(
                "\"source_sha256\":",
                "\"raw_user_id\": \"user-12345-private\",\n                \"source_sha256\":",
                StringComparison.Ordinal);
        File.WriteAllText(sourceManifestPath, content);

        var result = await new DesignConversionRealS3EvidenceBundleValidator().ValidateDirectoryAsync(bundle.Root);

        Assert.False(result.CanUseForRealS3);
        Assert.True(result.ForbiddenContentDetected);
        Assert.Contains("FORBIDDEN_FLAG_TRUE:source-manifest.json:$.provenance.raw_user_id_included", result.Reasons);
        Assert.Contains("FORBIDDEN_FLAG_TRUE:source-manifest.json:$.privacy.raw_user_id_included", result.Reasons);
        Assert.Contains("RAW_PROMPT_OR_USER_ID_REJECTED:source-manifest.json:$.provenance.raw_user_id", result.Reasons);
        Assert.False(result.UploadOrPromotionAllowed);
    }

    [Fact]
    public async Task ValidateDirectoryAsync_RejectsPrivateKeyAndTokenLikeContent()
    {
        using var bundle = CreateMinimalMarkedRealBundle();
        var dpoRowsPath = Path.Combine(bundle.Root, DesignConversionRealS3EvidenceBundleValidator.DpoRowsFileName);
        var content = File.ReadAllText(dpoRowsPath)
            .Replace("\"local_paths_included\":\"False\"",
                "\"local_paths_included\":\"False\",\"access_token\":\"abcdef1234567890\",\"private_key\":\"-----BEGIN PRIVATE KEY-----abcdef1234567890-----END PRIVATE KEY-----\"",
                StringComparison.Ordinal);
        File.WriteAllText(dpoRowsPath, content);

        var result = await new DesignConversionRealS3EvidenceBundleValidator().ValidateDirectoryAsync(bundle.Root);

        Assert.False(result.CanUseForRealS3);
        Assert.True(result.ForbiddenContentDetected);
        Assert.Contains("SECRET_PRIVATE_KEY_REJECTED:dpo-rows.jsonl", result.Reasons);
        Assert.Contains(result.Reasons, reason =>
            reason.StartsWith("SECRET_TOKEN_ASSIGNMENT_REJECTED:dpo-rows.jsonl", StringComparison.Ordinal));
        Assert.False(result.UploadOrPromotionAllowed);
    }

    [Fact]
    public async Task ValidateDirectoryAsync_RejectsUnapprovedProvenanceAndLicenseIndicator()
    {
        using var bundle = CreateMinimalMarkedRealBundle();
        var sourceManifestPath = Path.Combine(bundle.Root, DesignConversionRealS3EvidenceBundleValidator.SourceManifestFileName);
        var content = File.ReadAllText(sourceManifestPath)
            .Replace(
                "\"source_manifest_ref\": \"real-s3-v1:source-a\"",
                "\"source_manifest_ref\": \"provenance_unknown\"",
                StringComparison.Ordinal)
            .Replace(
                "\"license_or_authority_ref\": \"authority-approved-real-s3-v1\"",
                "\"license_or_authority_ref\": \"unknown_license\"",
                StringComparison.Ordinal);
        File.WriteAllText(sourceManifestPath, content);

        var result = await new DesignConversionRealS3EvidenceBundleValidator().ValidateDirectoryAsync(bundle.Root);

        Assert.False(result.CanUseForRealS3);
        Assert.True(result.ForbiddenContentDetected);
        Assert.Contains(
            "UNAPPROVED_SOURCE_PROVENANCE_LICENSE_INDICATOR:source-manifest.json:$.provenance.source_manifest_ref",
            result.Reasons);
        Assert.Contains(
            "UNAPPROVED_SOURCE_PROVENANCE_LICENSE_INDICATOR:source-manifest.json:$.license.license_or_authority_ref",
            result.Reasons);
        Assert.False(result.UploadOrPromotionAllowed);
    }

    [Fact]
    public async Task ValidateDirectoryAsync_RejectsMissingRequiredArtifact()
    {
        using var bundle = CreateMinimalMarkedRealBundle();
        File.Delete(Path.Combine(bundle.Root, DesignConversionRealS3EvidenceBundleValidator.CheckpointManifestFileName));

        var result = await new DesignConversionRealS3EvidenceBundleValidator().ValidateDirectoryAsync(bundle.Root);

        Assert.False(result.CanUseForRealS3);
        Assert.False(result.RequiredRealDataFilesPresent);
        Assert.False(result.CheckpointManifestReady);
        Assert.Contains("REQUIRED_ARTIFACT_MISSING:checkpoint-manifest.json", result.Reasons);
        Assert.Contains("CHECKPOINT_MANIFEST_NOT_READY", result.Reasons);
        Assert.Contains("REAL_TRAINED_WEIGHTS_NOT_PROVEN", result.Reasons);
        Assert.False(result.UploadOrPromotionAllowed);
    }

    [Fact]
    public async Task ValidateDirectoryAsync_RejectsDpoRowsWithPlaceholderRejectedQuality()
    {
        using var bundle = CreateMinimalMarkedRealBundle();
        var dpoRowsPath = Path.Combine(bundle.Root, DesignConversionRealS3EvidenceBundleValidator.DpoRowsFileName);
        var content = File.ReadAllText(dpoRowsPath)
            .Replace($"\"negative_quality_reason_hash\":\"{Hash('q')}\"",
                "\"negative_quality_reason_hash\":\"placeholder\"",
                StringComparison.Ordinal);
        File.WriteAllText(dpoRowsPath, content);

        var result = await new DesignConversionRealS3EvidenceBundleValidator().ValidateDirectoryAsync(bundle.Root);

        Assert.False(result.CanUseForRealS3);
        Assert.False(result.DpoRowsReady);
        Assert.Contains("NEGATIVE_PAIR_QUALITY_MISSING:dpo-rows.jsonl:negative_quality_reason_hash", result.Reasons);
        Assert.Contains("DPO_ROWS_NOT_READY", result.Reasons);
    }

    [Fact]
    public async Task ValidateDirectoryAsync_JsonlSkipsBlankLinesAndRejectsMalformedNonblankRowWithoutMutation()
    {
        using var bundle = CreateMinimalMarkedRealBundle();
        var dpoRowsPath = Path.Combine(bundle.Root, DesignConversionRealS3EvidenceBundleValidator.DpoRowsFileName);
        var train = BuildDpoRow("train", "row-train-a", Hash('e'), Hash('f'), Hash('1'), Hash('2'));
        var heldOut = BuildDpoRow("held_out", "row-held-out-a", Hash('3'), Hash('4'), Hash('5'), Hash('6'));
        var content = string.Join(
            Environment.NewLine,
            "",
            train,
            "   ",
            "{\"schema_version\":1",
            "\t",
            heldOut,
            "");
        await File.WriteAllTextAsync(dpoRowsPath, content);

        var result = await new DesignConversionRealS3EvidenceBundleValidator().ValidateDirectoryAsync(bundle.Root);

        var dpoRows = Assert.Single(
            result.Artifacts,
            artifact => artifact.ArtifactName == DesignConversionRealS3EvidenceBundleValidator.DpoRowsFileName);
        Assert.False(result.CanUseForRealS3);
        Assert.False(result.DpoRowsReady);
        Assert.False(dpoRows.JsonParsed);
        Assert.False(dpoRows.ShapeValid);
        Assert.Equal(2, dpoRows.RowCount);
        Assert.Contains("JSONL_ROW_PARSE_FAILED:dpo-rows.jsonl:2", result.Reasons);
        Assert.Contains("DPO_ROWS_NOT_READY", result.Reasons);
        Assert.Equal(content, await File.ReadAllTextAsync(dpoRowsPath));
    }

    [Fact]
    public async Task ValidateDirectoryAsync_JsonlNonObjectRowStillRunsForbiddenTextScanWithoutMutation()
    {
        using var bundle = CreateMinimalMarkedRealBundle();
        var dpoRowsPath = Path.Combine(bundle.Root, DesignConversionRealS3EvidenceBundleValidator.DpoRowsFileName);
        var train = BuildDpoRow("train", "row-train-a", Hash('e'), Hash('f'), Hash('1'), Hash('2'));
        var heldOut = BuildDpoRow("held_out", "row-held-out-a", Hash('3'), Hash('4'), Hash('5'), Hash('6'));
        var content = string.Join(
            Environment.NewLine,
            train,
            "\"access_token=abcdef1234567890\"",
            heldOut);
        await File.WriteAllTextAsync(dpoRowsPath, content);

        var result = await new DesignConversionRealS3EvidenceBundleValidator().ValidateDirectoryAsync(bundle.Root);

        Assert.False(result.CanUseForRealS3);
        Assert.False(result.DpoRowsReady);
        Assert.True(result.ForbiddenContentDetected);
        Assert.Contains("JSONL_ROW_OBJECT_REQUIRED:dpo-rows.jsonl:2", result.Reasons);
        Assert.Contains("SECRET_TOKEN_ASSIGNMENT_REJECTED:dpo-rows.jsonl:access_token", result.Reasons);
        Assert.Equal(content, await File.ReadAllTextAsync(dpoRowsPath));
    }

    [Fact]
    public async Task ValidateDirectoryAsync_MinimalMarkedRealBundlePassesLocalStructuralGateOnlyAndNeverAllowsUploadOrPromotion()
    {
        using var bundle = CreateMinimalMarkedRealBundle();

        var result = await new DesignConversionRealS3EvidenceBundleValidator().ValidateDirectoryAsync(bundle.Root);

        Assert.True(result.StructuralScaffoldPresent);
        Assert.True(result.RequiredRealDataFilesPresent);
        Assert.True(result.DataShapeValid);
        Assert.True(result.SourceManifestReady);
        Assert.True(result.DpoRowsReady);
        Assert.True(result.HeldOutMeasurementsReady);
        Assert.True(result.CheckpointManifestReady);
        Assert.True(result.RealTrainedWeightsPresent);
        Assert.True(result.RealHeldOutMeasurementEvidencePresent);
        Assert.True(result.CanUseForRealS3);
        Assert.False(result.TrainingUploadPromotionAttempted);
        Assert.False(result.UploadOrPromotionAllowed);
        Assert.Contains("DESIGN_CONVERSION_REAL_S3_LOCAL_VALIDATION_PASSED", result.EvidenceRefs);
        Assert.Contains("DESIGN_CONVERSION_REAL_S3_NO_UPLOAD_TRAINING_OR_PROMOTION", result.EvidenceRefs);
    }

    [Fact]
    public async Task ValidateDirectoryAsync_AlreadyCancelledTokenThrowsBeforeValidationAndDoesNotMutateBundle()
    {
        using var bundle = CreateMinimalMarkedRealBundle();
        var sourceManifestPath = Path.Combine(bundle.Root, DesignConversionRealS3EvidenceBundleValidator.SourceManifestFileName);
        var originalContent = await File.ReadAllTextAsync(sourceManifestPath);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            new DesignConversionRealS3EvidenceBundleValidator().ValidateDirectoryAsync(bundle.Root, cts.Token));

        Assert.Equal(originalContent, await File.ReadAllTextAsync(sourceManifestPath));
    }

    [Fact]
    public async Task ValidateApprovalPacketAsync_MissingPacketClassifiesApprovalPacketMissing()
    {
        var root = Path.Combine(
            FindRepoRoot(),
            "Tests",
            "TestResults",
            "design-conversion-real-s3-approval-validator",
            Guid.NewGuid().ToString("N"));
        var path = Path.Combine(root, "approval-packet.json");

        var result = await new DesignConversionRealS3EvidenceBundleValidator().ValidateApprovalPacketAsync(path);

        Assert.False(result.PacketExists);
        Assert.False(result.ApprovalPacketValid);
        Assert.False(result.CanResumeRealDataArtifactConstruction);
        Assert.False(result.ProductReady);
        Assert.Equal("approval_packet_missing", result.RouteClassifier);
        Assert.Contains("approval_packet_missing", result.Reasons);
        Assert.Contains("real_data_still_required", result.Reasons);
    }

    [Fact]
    public async Task ValidateApprovalPacketAsync_TemplatePacketClassifiesTemplateNotApproval()
    {
        using var bundle = CreateApprovalPacketFixture();
        var packetPath = Path.Combine(bundle.Root, "approval-packet.json");
        File.WriteAllText(
            packetPath,
            """
            {
              "approval_packet_template": true,
              "approver": "<approver>",
              "authority_basis": "template only"
            }
            """);

        var result = await new DesignConversionRealS3EvidenceBundleValidator().ValidateApprovalPacketAsync(packetPath);

        Assert.True(result.PacketExists);
        Assert.True(result.JsonParsed);
        Assert.True(result.TemplateOrScaffoldDetected);
        Assert.False(result.ApprovalPacketValid);
        Assert.False(result.ProductReady);
        Assert.Equal("template_not_approval", result.RouteClassifier);
        Assert.Contains("template_not_approval", result.Reasons);
        Assert.Contains("schema/template/scaffold/focused tests do not prove real S3 readiness", result.Reasons);
    }

    [Fact]
    public async Task ValidateApprovalPacketAsync_MissingRequiredFieldsClassifiesValidationFailed()
    {
        using var bundle = CreateApprovalPacketFixture();
        var packetPath = Path.Combine(bundle.Root, "approval-packet.json");
        File.WriteAllText(
            packetPath,
            """
            {
              "approver": "local-fixture-reviewer",
              "training_rights": true
            }
            """);

        var result = await new DesignConversionRealS3EvidenceBundleValidator().ValidateApprovalPacketAsync(packetPath);

        Assert.True(result.PacketExists);
        Assert.True(result.JsonParsed);
        Assert.False(result.RequiredFieldsPresent);
        Assert.False(result.ApprovalPacketValid);
        Assert.False(result.ProductReady);
        Assert.Equal("approval_validation_failed", result.RouteClassifier);
        Assert.Contains("approval_validation_failed", result.Reasons);
        Assert.Contains("approval_required_field_missing:authority_basis", result.Reasons);
    }

    [Fact]
    public async Task ValidateApprovalPacketAsync_DeniedRightsAndBlockedDecisionCannotResumeArtifactConstruction()
    {
        using var bundle = CreateApprovalPacketFixture();
        var packetPath = Path.Combine(bundle.Root, "approval-packet.json");
        File.WriteAllText(
            packetPath,
            """
            {
              "approver": "local-fixture-reviewer",
              "authority_basis": "local structural approval fixture for blocked route tests only",
              "approval_date": "2026-06-30",
              "scope": "DesignConversion real-S3 source artifact construction",
              "allowed_scenes": ["source-manifest", "dpo-row-authoring"],
              "excluded_scenes": ["training-upload", "promotion-upload", "product-ready-claim"],
              "license_decision": "blocked pending legal review",
              "training_rights": false,
              "evaluation_rights": true,
              "provenance_source_authority": "approved",
              "screenshot_image_usage": "approved",
              "restrictions": ["redacted refs and hashes only", "no raw private paths"],
              "validation_required": true
            }
            """);

        var result = await new DesignConversionRealS3EvidenceBundleValidator().ValidateApprovalPacketAsync(packetPath);

        Assert.True(result.PacketExists);
        Assert.True(result.JsonParsed);
        Assert.False(result.RequiredFieldsPresent);
        Assert.False(result.ApprovalPacketValid);
        Assert.False(result.CanResumeRealDataArtifactConstruction);
        Assert.False(result.ProductReady);
        Assert.Equal("approval_validation_failed", result.RouteClassifier);
        Assert.Contains("approval_validation_failed", result.Reasons);
        Assert.Contains("approval_required_grant_missing:training_rights", result.Reasons);
        Assert.Contains("approval_required_decision_invalid:license_decision", result.Reasons);
        Assert.Contains("real_data_still_required", result.Reasons);
    }

    [Fact]
    public async Task ValidateApprovalPacketAsync_ValidLocalFixtureStillRequiresRealDataArtifactsAndIsNotProductReady()
    {
        using var bundle = CreateApprovalPacketFixture();
        var packetPath = Path.Combine(bundle.Root, "approval-packet.json");
        WriteLocalStructuralApprovalFixture(packetPath);

        var result = await new DesignConversionRealS3EvidenceBundleValidator().ValidateApprovalPacketAsync(packetPath);

        Assert.True(result.PacketExists);
        Assert.True(result.JsonParsed);
        Assert.True(result.RequiredFieldsPresent);
        Assert.True(result.ApprovalPacketValid);
        Assert.True(result.CanResumeRealDataArtifactConstruction);
        Assert.False(result.ProductReady);
        Assert.Equal("approval_valid_but_not_product_ready", result.RouteClassifier);
        Assert.Contains("approval_valid_but_not_product_ready", result.Reasons);
        Assert.Contains("real_data_still_required", result.Reasons);
        Assert.Contains("schema/template/scaffold/focused tests do not prove real S3 readiness", result.Reasons);
        Assert.Contains(
            "required_real_data_files:source-manifest.json,dpo-rows.jsonl,heldout-measurements.json,checkpoint-manifest.json",
            result.Reasons);
        Assert.Contains("approval-validation", result.EvidenceRefs);
    }

    [Fact]
    public async Task ValidateApprovalPacketAsync_AlreadyCancelledTokenThrowsAndDoesNotMutatePacket()
    {
        using var bundle = CreateApprovalPacketFixture();
        var packetPath = Path.Combine(bundle.Root, "approval-packet.json");
        WriteLocalStructuralApprovalFixture(packetPath);
        var originalContent = await File.ReadAllTextAsync(packetPath);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            new DesignConversionRealS3EvidenceBundleValidator().ValidateApprovalPacketAsync(packetPath, cts.Token));

        Assert.Equal(originalContent, await File.ReadAllTextAsync(packetPath));
    }

    private static BundleFixture CreateMinimalMarkedRealBundle()
    {
        var root = Path.Combine(
            FindRepoRoot(),
            "Tests",
            "TestResults",
            "design-conversion-real-s3-validator",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        CopySchemaFiles(root);
        WriteSourceManifest(root);
        WriteDpoRows(root);
        WriteHeldOutMeasurements(root);
        WriteCheckpointManifest(root);
        return new BundleFixture(root);
    }

    private static BundleFixture CreateApprovalPacketFixture()
    {
        var root = Path.Combine(
            FindRepoRoot(),
            "Tests",
            "TestResults",
            "design-conversion-real-s3-approval-validator",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return new BundleFixture(root);
    }

    private static void WriteLocalStructuralApprovalFixture(string path)
    {
        File.WriteAllText(
            path,
            """
            {
              "approver": "local-fixture-reviewer",
              "authority_basis": "local structural approval fixture for validator route tests only",
              "approval_date": "2026-06-30",
              "scope": "DesignConversion real-S3 source artifact construction",
              "allowed_scenes": ["source-manifest", "dpo-row-authoring", "held-out-measurement", "checkpoint-manifest"],
              "excluded_scenes": ["training-upload", "promotion-upload", "product-ready-claim"],
              "license_decision": "approved",
              "training_rights": true,
              "evaluation_rights": true,
              "provenance_source_authority": "approved",
              "screenshot_image_usage": "approved",
              "restrictions": ["redacted refs and hashes only", "no raw private paths", "no upload or promotion"],
              "validation_required": true
            }
            """);
    }

    private static void CopySchemaFiles(string root)
    {
        var sourceRoot = Path.Combine(FindRepoRoot(), "training-data", "design-conversion", "real-s3-v1");
        foreach (var schemaFile in Directory.GetFiles(sourceRoot, "*.schema.json"))
        {
            File.Copy(schemaFile, Path.Combine(root, Path.GetFileName(schemaFile)));
        }
    }

    private static void WriteSourceManifest(string root)
    {
        File.WriteAllText(
            Path.Combine(root, DesignConversionRealS3EvidenceBundleValidator.SourceManifestFileName),
            $$"""
            {
              "schema_version": 1,
              "bundle_id": "real-s3-v1:local-structural-fixture",
              "dataset_id": "dcm-real-s3-fixture",
              "created_utc": "2026-06-30T00:00:00Z",
              "source_design_id": "redacted-source-a",
              "source_kind": "other",
              "source_design_tree": {
                "tree_ref": "bundle://trees/source-a.json",
                "tree_format": "other",
                "tree_sha256": "{{Hash('a')}}",
                "source_identity_hash": "{{Hash('b')}}",
                "raw_prompt_included": false,
                "absolute_path_included": false
              },
              "screenshots": [
                {
                  "screenshot_ref": "bundle://screenshots/source-a.png",
                  "kind": "source_expected",
                  "width": 128,
                  "height": 96,
                  "sha256": "{{Hash('c')}}",
                  "captured_utc": "2026-06-30T00:00:01Z",
                  "license_or_authority_ref": "authority-approved-real-s3-v1"
                }
              ],
              "provenance": {
                "source_manifest_ref": "real-s3-v1:source-a",
                "source_uri_or_local_ref": "bundle://sources/source-a",
                "source_sha256": "{{Hash('d')}}",
                "provenance_refs": ["provenance-approved-real-s3-v1"],
                "reviewer_ref": "reviewer-role-redacted",
                "raw_user_id_included": false,
                "absolute_path_included": false
              },
              "license": {
                "license_or_authority_ref": "authority-approved-real-s3-v1",
                "rights_scope": "training-and-evaluation-approved",
                "allows_training": true,
                "allows_evaluation": true,
                "contains_user_private_data": false
              },
              "privacy": {
                "raw_prompt_included": false,
                "raw_file_system_path_included": false,
                "raw_user_id_included": false,
                "secrets_included": false,
                "redaction_policy": "bundle_refs_and_hashes_only"
              },
              "evidence_status": "real_source_manifest_ready"
            }
            """);
    }

    private static void WriteDpoRows(string root)
    {
        var train = BuildDpoRow("train", "row-train-a", Hash('e'), Hash('f'), Hash('1'), Hash('2'));
        var heldOut = BuildDpoRow("held_out", "row-held-out-a", Hash('3'), Hash('4'), Hash('5'), Hash('6'));
        File.WriteAllText(
            Path.Combine(root, DesignConversionRealS3EvidenceBundleValidator.DpoRowsFileName),
            string.Join(Environment.NewLine, train, heldOut));
    }

    private static string BuildDpoRow(
        string split,
        string rowId,
        string recordHash,
        string sourceHash,
        string chosenHash,
        string rejectedHash)
        => JsonSerializer.Serialize(new
        {
            schema_version = 1,
            dataset_id = "dcm-real-s3-fixture",
            split,
            row_id = rowId,
            record_identity_hash = recordHash,
            unit_id = "critic",
            domain_tag = "DesignConversionDomain",
            source_kind = "other",
            source_identity_hash = sourceHash,
            source_group_id = "dcm-source-fixture",
            pair_family = "visual-quality",
            usefulness_score = 0.96,
            prompt = new
            {
                domain_tag = "DesignConversionDomain",
                unit_id = "critic",
                target = new
                {
                    unit_id = "critic",
                    machine_part = "design_conversion_critic_unit"
                },
                source = new
                {
                    kind = "other",
                    identity_hash = sourceHash,
                    structure_hash = Hash('7')
                },
                internal_unit_state_hash = Hash('8'),
                internal_vector_dimensions = 3,
                result_features = new
                {
                    chosen_result_ref = "dcm-result-chosen",
                    rejected_result_ref = "dcm-result-rejected",
                    negative_quality_reason_hash = Hash('q'),
                    pair_family = "visual-quality"
                },
                pair_family = "visual-quality",
                raw_prompt_included = false
            },
            chosen = new
            {
                result_ref = "dcm-result-chosen",
                result_hash = chosenHash,
                visual_score = 0.95,
                accepted = true
            },
            rejected = new
            {
                result_ref = "dcm-result-rejected",
                result_hash = rejectedHash,
                visual_score = 0.25,
                accepted = false
            },
            candidate_result = new
            {
                chosen = new
                {
                    result_ref = "dcm-result-chosen",
                    result_hash = chosenHash,
                    visual_score = 0.95,
                    accepted = true
                },
                rejected = new
                {
                    result_ref = "dcm-result-rejected",
                    result_hash = rejectedHash,
                    visual_score = 0.25,
                    accepted = false
                }
            },
            teacher_labels = new
            {
                preferred_result_ref = "dcm-result-chosen",
                rejected_result_ref = "dcm-result-rejected",
                preference_label = "chosen_over_rejected",
                score_margin = 0.7,
                pair_family = "visual-quality",
                defect_categories = new[] { "layout" }
            },
            teaching_point = "prefer approved visual output over rejected output",
            curriculum_tier = "tier_1_high_confidence",
            validation_outcome = new
            {
                accepted_for_training = true,
                quarantined = false,
                license_allows_training = true,
                contains_user_private_data = false,
                unit_id_present = true,
                outcome = "accepted_record"
            },
            metadata = new
            {
                schema = "dcm-dpo-dataset-row-v2",
                record_identity_scope = "sha256",
                source_identity_scope = "sha256",
                result_identity_scope = "sha256",
                candidate_result_scope = "safe_hash_ref",
                teacher_labels_scope = "derived_from_accepted_record",
                validation_outcome_scope = "accepted_record_booleans",
                raw_source_request_id_included = "False",
                raw_source_manifest_ref_included = "False",
                raw_reviewer_ref_included = "False",
                raw_license_id_included = "False",
                raw_candidate_ids_included = "False",
                local_paths_included = "False"
            },
            leakage_guard = new
            {
                raw_prompt_included = false,
                raw_file_system_path_included = false,
                raw_user_id_included = false,
                secrets_included = false,
                redacted_refs_only = true
            }
        });

    private static void WriteHeldOutMeasurements(string root)
    {
        File.WriteAllText(
            Path.Combine(root, "heldout-measurements.json"),
            $$"""
            {
              "schema_version": 1,
              "measurement_id": "measurement-real-s3-fixture",
              "dataset_id": "dcm-real-s3-fixture",
              "unit_id": "critic",
              "checkpoint_ref": "bundle://checkpoints/design-conversion-fixture.json",
              "measurement_gate_status": "passed",
              "held_out_metrics": {
                "held_out_record_count": 1,
                "held_out_quality_score": 0.95,
                "required_held_out_quality_score": 0.9,
                "runtime_ms": 10,
                "cost_usd": 0,
                "peak_ram_mb": 32,
                "evidence_refs": ["heldout-measurement-real-s3-fixture"]
              },
              "integrity_proof": {
                "source_disjoint": true,
                "leakage_detected": false,
                "hidden_determinism_detected": false,
                "ablation_passed": true,
                "anti_leakage_evidence_refs": ["anti-leakage-real-s3-fixture"],
                "ablation_evidence_refs": ["ablation-real-s3-fixture"]
              },
              "thresholds": {
                "required_held_out_quality_score": 0.9,
                "max_runtime_ms": 100,
                "max_cost_usd": 1,
                "max_peak_ram_mb": 256
              },
              "runtime_cost": {
                "runtime_ms": 10,
                "cost_usd": 0,
                "peak_ram_mb": 32,
                "budget_passed": true
              },
              "evidence_status": "real_held_out_measurement_ready",
              "promotion_upload": "blocked"
            }
            """);
    }

    private static void WriteCheckpointManifest(string root)
    {
        File.WriteAllText(
            Path.Combine(root, DesignConversionRealS3EvidenceBundleValidator.CheckpointManifestFileName),
            $$"""
            {
              "schema_version": 1,
              "checkpoint_ref": "bundle://checkpoints/design-conversion-fixture.json",
              "checkpoint_sha256": "{{Hash('9')}}",
              "domain_tag": "DesignConversionDomain",
              "machine_id": "design-conversion-machine",
              "version": "0.1.0",
              "exported_utc": "2026-06-30T00:00:02Z",
              "checkpoint_contains_real_trained_weights": true,
              "weights": {
                "weights_ref": "bundle://weights/design-conversion-fixture.weights",
                "weights_sha256": "{{Hash('a')}}",
                "learned_weights_present": true,
                "unit_manifests": [
                  {
                    "unit_id": "critic",
                    "unit_role": "critic",
                    "implementation_status": "learned-weights-present",
                    "learned_weights_present": true,
                    "evidence_refs": ["weights-real-s3-fixture"]
                  }
                ],
                "raw_model_upload_performed": false
              },
              "fresh_load_probe": {
                "fresh_load_performed": true,
                "loaded_checkpoint_sha256": "{{Hash('9')}}",
                "metadata_continuity_passed": true,
                "checkpoint_contains_real_trained_weights": true,
                "evidence_refs": ["fresh-load-real-s3-fixture"]
              },
              "continue_training_probe": {
                "continue_training_performed": false,
                "real_trained_weights_continued": false,
                "train_rows_used": 0,
                "held_out_rows_used": 0,
                "measurement_gate_run": true,
                "evidence_refs": ["continue-training-not-attempted-local-fixture"]
              },
              "training_dataset_id": "dcm-real-s3-fixture",
              "train_row_count": 1,
              "held_out_row_count": 1,
              "measurement_gate_refs": ["measurement-real-s3-fixture"],
              "promotion_upload": "blocked",
              "evidence_status": "real_checkpoint_manifest_ready"
            }
            """);
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null &&
               !File.Exists(Path.Combine(directory.FullName, "LlmFromScratch.sln")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        return directory.FullName;
    }

    private static string Hash(char value)
        => new(value, 64);

    private sealed class BundleFixture(string root) : IDisposable
    {
        public string Root { get; } = root;

        public void Dispose()
        {
            if (Directory.Exists(Root))
                Directory.Delete(Root, recursive: true);
        }
    }
}
