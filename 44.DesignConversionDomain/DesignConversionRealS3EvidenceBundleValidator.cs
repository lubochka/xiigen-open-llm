// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

public sealed record DesignConversionRealS3EvidenceBundleArtifactResult(
    string ArtifactName,
    string ArtifactPath,
    bool Exists,
    bool IsSchemaScaffold,
    bool IsRequiredRealDataFile,
    bool JsonParsed,
    int RowCount,
    bool ShapeValid,
    IReadOnlyList<string> Reasons);

public sealed record DesignConversionRealS3ApprovalPacketValidationResult(
    string ApprovalPacketPath,
    bool PacketExists,
    bool JsonParsed,
    bool TemplateOrScaffoldDetected,
    bool RequiredFieldsPresent,
    bool ApprovalPacketValid,
    bool CanResumeRealDataArtifactConstruction,
    bool ProductReady,
    string RouteClassifier,
    IReadOnlyList<string> Reasons,
    IReadOnlyList<string> EvidenceRefs);

public sealed record DesignConversionRealS3EvidenceBundleValidationResult(
    string BundleRoot,
    bool BundleDirectoryExists,
    bool StructuralScaffoldPresent,
    bool RequiredRealDataFilesPresent,
    bool DataShapeValid,
    bool ForbiddenContentDetected,
    bool PlaceholderNotRealDetected,
    bool SourceManifestReady,
    bool DpoRowsReady,
    bool HeldOutMeasurementsReady,
    bool CheckpointManifestReady,
    bool RealTrainedWeightsPresent,
    bool RealHeldOutMeasurementEvidencePresent,
    bool TrainingUploadPromotionAttempted,
    bool CanUseForRealS3,
    bool UploadOrPromotionAllowed,
    IReadOnlyList<DesignConversionRealS3EvidenceBundleArtifactResult> Artifacts,
    IReadOnlyList<string> Reasons,
    IReadOnlyList<string> EvidenceRefs);

public sealed class DesignConversionRealS3EvidenceBundleValidator
{
    public const string SourceManifestFileName = "source-manifest.json";
    public const string DpoRowsFileName = "dpo-rows.jsonl";
    public const string CheckpointManifestFileName = "checkpoint-manifest.json";
    public const string ApprovalValidationEvidenceRef = "approval-validation";
    public const string ApprovalPacketMissingRoute = "approval_packet_missing";
    public const string TemplateNotApprovalRoute = "template_not_approval";
    public const string ApprovalValidationFailedRoute = "approval_validation_failed";
    public const string RealDataStillRequiredRoute = "real_data_still_required";
    public const string ApprovalValidButNotProductReadyRoute = "approval_valid_but_not_product_ready";
    public const string ScaffoldFocusedTestsDoNotProveRoute =
        "schema/template/scaffold/focused tests do not prove real S3 readiness";

    public static readonly IReadOnlyList<string> HeldOutMeasurementFileNames =
        ["heldout-measurements.json", "heldout-measurements.jsonl"];

    private static readonly IReadOnlyList<string> SchemaFileNames =
    [
        "source-manifest.schema.json",
        "dpo-rows.schema.json",
        "heldout-measurements.schema.json",
        "checkpoint-manifest.schema.json"
    ];

    private static readonly IReadOnlyList<string> ApprovalPacketRequiredFields =
    [
        "approver",
        "authority_basis",
        "approval_date",
        "scope",
        "allowed_scenes",
        "license_decision",
        "training_rights",
        "evaluation_rights",
        "provenance_source_authority",
        "screenshot_image_usage",
        "restrictions",
        "validation_required"
    ];

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static readonly Regex RawWindowsAbsolutePath = new(
        @"(?i)(?:file:///)?\b[a-z]:[\\/][^\s""'<>|]+",
        RegexOptions.Compiled);

    private static readonly Regex PrivateKeyBlock = new(
        @"-----BEGIN (?:[A-Z]+ )?PRIVATE KEY-----",
        RegexOptions.Compiled);

    private static readonly Regex SensitiveAssignment = new(
        @"(?im)[""']?\b(api[_-]?key|access[_-]?token|auth[_-]?token|bearer[_-]?token|client[_-]?secret|secret[_-]?key|private[_-]?key|password|token)\b[""']?\s*[:=]\s*[""']?[^""'\s,}]{8,}",
        RegexOptions.Compiled);

    private static readonly HashSet<string> JsonBooleanFlagsThatMustBeFalse = new(StringComparer.OrdinalIgnoreCase)
    {
        "placeholder_not_real",
        "raw_prompt_included",
        "raw_file_system_path_included",
        "raw_user_id_included",
        "absolute_path_included",
        "secrets_included",
        "contains_user_private_data",
        "raw_model_upload_performed",
        "leakage_detected",
        "hidden_determinism_detected"
    };

    private static readonly HashSet<string> JsonStringFalseFlags = new(StringComparer.OrdinalIgnoreCase)
    {
        "raw_source_request_id_included",
        "raw_source_manifest_ref_included",
        "raw_reviewer_ref_included",
        "raw_license_id_included",
        "raw_candidate_ids_included",
        "local_paths_included"
    };

    private static readonly HashSet<string> RawPromptOrUserIdProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "raw_prompt",
        "exact_prompt",
        "prompt_text",
        "user_prompt",
        "raw_user_id",
        "user_id",
        "userids",
        "user_ids"
    };

    private static readonly HashSet<string> SensitiveJsonPropertyNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "api_key",
        "apikey",
        "access_token",
        "auth_token",
        "bearer_token",
        "client_secret",
        "secret_key",
        "private_key",
        "password",
        "token"
    };

    public async Task<DesignConversionRealS3EvidenceBundleValidationResult> ValidateDirectoryAsync(
        string bundleRoot,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var root = string.IsNullOrWhiteSpace(bundleRoot)
            ? string.Empty
            : Path.GetFullPath(bundleRoot);
        var bundleExists = !string.IsNullOrWhiteSpace(root) && Directory.Exists(root);
        var artifacts = new List<DesignConversionRealS3EvidenceBundleArtifactResult>();
        var reasons = new List<string>();
        var evidenceRefs = new List<string>
        {
            "DESIGN_CONVERSION_REAL_S3_LOCAL_VALIDATION_ONLY",
            "DESIGN_CONVERSION_REAL_S3_NO_UPLOAD_TRAINING_OR_PROMOTION"
        };

        if (!bundleExists)
        {
            reasons.Add("BUNDLE_DIRECTORY_MISSING");
            return BuildResult(
                root,
                bundleExists,
                structuralScaffoldPresent: false,
                sourceManifestReady: false,
                dpoRowsReady: false,
                heldOutMeasurementsReady: false,
                checkpointManifestReady: false,
                realTrainedWeightsPresent: false,
                realHeldOutMeasurementEvidencePresent: false,
                trainingUploadPromotionAttempted: false,
                artifacts,
                reasons,
                evidenceRefs);
        }

        foreach (var schemaFileName in SchemaFileNames)
        {
            artifacts.Add(await ValidateSchemaFileAsync(root, schemaFileName, ct));
        }

        var structuralScaffoldPresent = SchemaFileNames.All(fileName =>
            artifacts.Any(artifact =>
                artifact.ArtifactName == fileName &&
                artifact.Exists &&
                artifact.JsonParsed &&
                artifact.ShapeValid));
        if (structuralScaffoldPresent)
        {
            reasons.Add("SCHEMA_SCAFFOLD_PRESENT_NOT_REAL_EVIDENCE");
            evidenceRefs.Add("DESIGN_CONVERSION_REAL_S3_SCHEMA_SCAFFOLD_PRESENT");
        }

        var sourceManifest = await ValidateJsonObjectArtifactAsync(
            root,
            SourceManifestFileName,
            true,
            ValidateSourceManifestShape,
            ct);
        artifacts.Add(sourceManifest.Artifact);
        reasons.AddRange(sourceManifest.Reasons);

        var dpoRows = await ValidateJsonlArtifactAsync(root, DpoRowsFileName, ValidateDpoRowShape, ct);
        artifacts.Add(dpoRows.Artifact);
        reasons.AddRange(dpoRows.Reasons);

        var heldOutFileName = HeldOutMeasurementFileNames.FirstOrDefault(fileName => File.Exists(Path.Combine(root, fileName)))
                              ?? HeldOutMeasurementFileNames[0];
        var heldOutMeasurements = heldOutFileName.EndsWith(".jsonl", StringComparison.OrdinalIgnoreCase)
            ? await ValidateJsonlArtifactAsync(root, heldOutFileName, ValidateHeldOutMeasurementShape, ct)
            : await ValidateJsonObjectArtifactAsync(root, heldOutFileName, true, ValidateHeldOutMeasurementShape, ct);
        artifacts.Add(heldOutMeasurements.Artifact);
        reasons.AddRange(heldOutMeasurements.Reasons);

        var checkpointManifest = await ValidateJsonObjectArtifactAsync(
            root,
            CheckpointManifestFileName,
            true,
            ValidateCheckpointManifestShape,
            ct);
        artifacts.Add(checkpointManifest.Artifact);
        reasons.AddRange(checkpointManifest.Reasons);

        var sourceManifestReady = sourceManifest.Ready;
        var dpoRowsReady = dpoRows.Ready;
        var heldOutMeasurementsReady = heldOutMeasurements.Ready;
        var checkpointManifestReady = checkpointManifest.Ready;
        var realTrainedWeightsPresent = checkpointManifest.Flags.Contains("REAL_TRAINED_WEIGHTS_PRESENT", StringComparer.Ordinal);
        var realHeldOutMeasurementEvidencePresent =
            heldOutMeasurements.Flags.Contains("REAL_HELD_OUT_MEASUREMENT_EVIDENCE_PRESENT", StringComparer.Ordinal);
        var trainingUploadPromotionAttempted =
            sourceManifest.Flags.Contains("UPLOAD_OR_PROMOTION_ATTEMPTED", StringComparer.Ordinal) ||
            dpoRows.Flags.Contains("UPLOAD_OR_PROMOTION_ATTEMPTED", StringComparer.Ordinal) ||
            heldOutMeasurements.Flags.Contains("UPLOAD_OR_PROMOTION_ATTEMPTED", StringComparer.Ordinal) ||
            checkpointManifest.Flags.Contains("UPLOAD_OR_PROMOTION_ATTEMPTED", StringComparer.Ordinal);

        return BuildResult(
            root,
            bundleExists,
            structuralScaffoldPresent,
            sourceManifestReady,
            dpoRowsReady,
            heldOutMeasurementsReady,
            checkpointManifestReady,
            realTrainedWeightsPresent,
            realHeldOutMeasurementEvidencePresent,
            trainingUploadPromotionAttempted,
            artifacts,
            reasons,
            evidenceRefs);
    }

    private static DesignConversionRealS3EvidenceBundleValidationResult BuildResult(
        string root,
        bool bundleExists,
        bool structuralScaffoldPresent,
        bool sourceManifestReady,
        bool dpoRowsReady,
        bool heldOutMeasurementsReady,
        bool checkpointManifestReady,
        bool realTrainedWeightsPresent,
        bool realHeldOutMeasurementEvidencePresent,
        bool trainingUploadPromotionAttempted,
        IReadOnlyList<DesignConversionRealS3EvidenceBundleArtifactResult> artifacts,
        IReadOnlyList<string> reasons,
        IReadOnlyList<string> evidenceRefs)
    {
        var requiredRealDataFilesPresent = sourceManifestReady || ArtifactExists(artifacts, SourceManifestFileName);
        requiredRealDataFilesPresent &=
            dpoRowsReady || ArtifactExists(artifacts, DpoRowsFileName);
        requiredRealDataFilesPresent &=
            heldOutMeasurementsReady || HeldOutMeasurementFileNames.Any(fileName => ArtifactExists(artifacts, fileName));
        requiredRealDataFilesPresent &=
            checkpointManifestReady || ArtifactExists(artifacts, CheckpointManifestFileName);

        var allReasons = new List<string>(reasons);
        if (!requiredRealDataFilesPresent)
            allReasons.Add("REQUIRED_REAL_DATA_FILES_MISSING");
        if (!sourceManifestReady)
            allReasons.Add("SOURCE_MANIFEST_NOT_READY");
        if (!dpoRowsReady)
            allReasons.Add("DPO_ROWS_NOT_READY");
        if (!heldOutMeasurementsReady)
            allReasons.Add("HELD_OUT_MEASUREMENTS_NOT_READY");
        if (!checkpointManifestReady)
            allReasons.Add("CHECKPOINT_MANIFEST_NOT_READY");
        if (!realTrainedWeightsPresent)
            allReasons.Add("REAL_TRAINED_WEIGHTS_NOT_PROVEN");
        if (!realHeldOutMeasurementEvidencePresent)
            allReasons.Add("REAL_HELD_OUT_MEASUREMENT_EVIDENCE_NOT_PROVEN");
        if (trainingUploadPromotionAttempted)
            allReasons.Add("UPLOAD_OR_PROMOTION_ATTEMPT_REJECTED");

        var distinctReasons = allReasons
            .Distinct(StringComparer.Ordinal)
            .OrderBy(reason => reason, StringComparer.Ordinal)
            .ToArray();
        var forbiddenContentDetected = distinctReasons.Any(reason =>
            reason.Contains("FORBIDDEN", StringComparison.Ordinal) ||
            reason.Contains("RAW_", StringComparison.Ordinal) ||
            reason.Contains("SECRET", StringComparison.Ordinal) ||
            reason.Contains("PRIVATE_KEY", StringComparison.Ordinal) ||
            reason.Contains("WINDOWS_ABSOLUTE_PATH", StringComparison.Ordinal) ||
            reason.Contains("UNAPPROVED", StringComparison.Ordinal));
        var placeholderNotRealDetected = distinctReasons.Any(reason =>
            reason.Contains("PLACEHOLDER_NOT_REAL_PRESENT", StringComparison.Ordinal));
        var dataShapeValid =
            sourceManifestReady &&
            dpoRowsReady &&
            heldOutMeasurementsReady &&
            checkpointManifestReady;
        var canUseForRealS3 =
            bundleExists &&
            requiredRealDataFilesPresent &&
            dataShapeValid &&
            !forbiddenContentDetected &&
            !placeholderNotRealDetected &&
            realTrainedWeightsPresent &&
            realHeldOutMeasurementEvidencePresent &&
            !trainingUploadPromotionAttempted;

        var finalEvidenceRefs = evidenceRefs.ToList();
        finalEvidenceRefs.Add(canUseForRealS3
            ? "DESIGN_CONVERSION_REAL_S3_LOCAL_VALIDATION_PASSED"
            : "DESIGN_CONVERSION_REAL_S3_LOCAL_VALIDATION_REJECTED");

        return new DesignConversionRealS3EvidenceBundleValidationResult(
            BundleRoot: root,
            BundleDirectoryExists: bundleExists,
            StructuralScaffoldPresent: structuralScaffoldPresent,
            RequiredRealDataFilesPresent: requiredRealDataFilesPresent,
            DataShapeValid: dataShapeValid,
            ForbiddenContentDetected: forbiddenContentDetected,
            PlaceholderNotRealDetected: placeholderNotRealDetected,
            SourceManifestReady: sourceManifestReady,
            DpoRowsReady: dpoRowsReady,
            HeldOutMeasurementsReady: heldOutMeasurementsReady,
            CheckpointManifestReady: checkpointManifestReady,
            RealTrainedWeightsPresent: realTrainedWeightsPresent,
            RealHeldOutMeasurementEvidencePresent: realHeldOutMeasurementEvidencePresent,
            TrainingUploadPromotionAttempted: trainingUploadPromotionAttempted,
            CanUseForRealS3: canUseForRealS3,
            UploadOrPromotionAllowed: false,
            Artifacts: artifacts,
            Reasons: distinctReasons,
            EvidenceRefs: finalEvidenceRefs.Distinct(StringComparer.Ordinal).ToArray());
    }

    public async Task<DesignConversionRealS3ApprovalPacketValidationResult> ValidateApprovalPacketAsync(
        string approvalPacketPath,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var path = string.IsNullOrWhiteSpace(approvalPacketPath)
            ? string.Empty
            : Path.GetFullPath(approvalPacketPath);
        var evidenceRefs = new List<string> { ApprovalValidationEvidenceRef };

        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return BuildApprovalPacketResult(
                path,
                packetExists: false,
                jsonParsed: false,
                templateOrScaffoldDetected: false,
                requiredFieldsPresent: false,
                approvalPacketValid: false,
                routeClassifier: ApprovalPacketMissingRoute,
                reasons: [ApprovalPacketMissingRoute],
                evidenceRefs);
        }

        var reasons = new List<string>();
        JsonDocument? document = null;
        try
        {
            var approvalPacketJson = await File.ReadAllTextAsync(path, ct);
            document = JsonDocument.Parse(approvalPacketJson, new JsonDocumentOptions { AllowTrailingCommas = false });
        }
        catch (JsonException)
        {
            reasons.Add(ApprovalValidationFailedRoute);
            reasons.Add("approval_packet_json_parse_failed");
            return BuildApprovalPacketResult(
                path,
                packetExists: true,
                jsonParsed: false,
                templateOrScaffoldDetected: false,
                requiredFieldsPresent: false,
                approvalPacketValid: false,
                routeClassifier: ApprovalValidationFailedRoute,
                reasons,
                evidenceRefs);
        }

        using (document)
        {
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                reasons.Add(ApprovalValidationFailedRoute);
                reasons.Add("approval_packet_object_required");
                return BuildApprovalPacketResult(
                    path,
                    packetExists: true,
                    jsonParsed: true,
                    templateOrScaffoldDetected: false,
                    requiredFieldsPresent: false,
                    approvalPacketValid: false,
                    routeClassifier: ApprovalValidationFailedRoute,
                    reasons,
                    evidenceRefs);
            }

            var templateOrScaffoldDetected = ApprovalPacketLooksLikeTemplate(document.RootElement);
            if (templateOrScaffoldDetected)
            {
                reasons.Add(TemplateNotApprovalRoute);
                return BuildApprovalPacketResult(
                    path,
                    packetExists: true,
                    jsonParsed: true,
                    templateOrScaffoldDetected: true,
                    requiredFieldsPresent: false,
                    approvalPacketValid: false,
                    routeClassifier: TemplateNotApprovalRoute,
                    reasons,
                    evidenceRefs);
            }

            var requiredFieldsPresent = ValidateApprovalPacketRequiredFields(document.RootElement, reasons);
            if (!requiredFieldsPresent)
            {
                reasons.Add(ApprovalValidationFailedRoute);
                return BuildApprovalPacketResult(
                    path,
                    packetExists: true,
                    jsonParsed: true,
                    templateOrScaffoldDetected: false,
                    requiredFieldsPresent: false,
                    approvalPacketValid: false,
                    routeClassifier: ApprovalValidationFailedRoute,
                    reasons,
                    evidenceRefs);
            }

            reasons.Add(ApprovalValidButNotProductReadyRoute);
            return BuildApprovalPacketResult(
                path,
                packetExists: true,
                jsonParsed: true,
                templateOrScaffoldDetected: false,
                requiredFieldsPresent: true,
                approvalPacketValid: true,
                routeClassifier: ApprovalValidButNotProductReadyRoute,
                reasons,
                evidenceRefs);
        }
    }

    private static bool ArtifactExists(IReadOnlyList<DesignConversionRealS3EvidenceBundleArtifactResult> artifacts, string fileName)
        => artifacts.Any(artifact => artifact.ArtifactName == fileName && artifact.Exists);

    private static DesignConversionRealS3ApprovalPacketValidationResult BuildApprovalPacketResult(
        string path,
        bool packetExists,
        bool jsonParsed,
        bool templateOrScaffoldDetected,
        bool requiredFieldsPresent,
        bool approvalPacketValid,
        string routeClassifier,
        IReadOnlyList<string> reasons,
        IReadOnlyList<string> evidenceRefs)
    {
        var allReasons = reasons.ToList();
        allReasons.Add(RealDataStillRequiredRoute);
        allReasons.Add(ScaffoldFocusedTestsDoNotProveRoute);
        allReasons.Add(
            $"required_real_data_files:{SourceManifestFileName},{DpoRowsFileName},{HeldOutMeasurementFileNames[0]},{CheckpointManifestFileName}");

        return new DesignConversionRealS3ApprovalPacketValidationResult(
            ApprovalPacketPath: path,
            PacketExists: packetExists,
            JsonParsed: jsonParsed,
            TemplateOrScaffoldDetected: templateOrScaffoldDetected,
            RequiredFieldsPresent: requiredFieldsPresent,
            ApprovalPacketValid: approvalPacketValid,
            CanResumeRealDataArtifactConstruction: approvalPacketValid,
            ProductReady: false,
            RouteClassifier: routeClassifier,
            Reasons: allReasons.Distinct(StringComparer.Ordinal).OrderBy(reason => reason, StringComparer.Ordinal).ToArray(),
            EvidenceRefs: evidenceRefs.Distinct(StringComparer.Ordinal).ToArray());
    }

    private static bool ValidateApprovalPacketRequiredFields(JsonElement root, List<string> reasons)
    {
        var ok = true;
        foreach (var field in ApprovalPacketRequiredFields)
        {
            if (!root.TryGetProperty(field, out var value) || !ApprovalFieldHasValue(value))
            {
                reasons.Add($"approval_required_field_missing:{field}");
                ok = false;
            }
        }

        if (!root.TryGetProperty("excluded_scenes", out var excludedScenes) ||
            !ApprovalFieldHasValue(excludedScenes))
        {
            if (!root.TryGetProperty("mail_decision", out var mailDecision) ||
                !ApprovalFieldHasValue(mailDecision))
            {
                reasons.Add("approval_required_field_missing:excluded_scenes_or_mail_decision");
                ok = false;
            }
        }

        ok &= RequireApprovalGranted(root, "training_rights", reasons);
        ok &= RequireApprovalGranted(root, "evaluation_rights", reasons);
        ok &= RequireApprovalGranted(root, "validation_required", reasons);
        ok &= RequireApprovalDecisionNotBlocked(root, "license_decision", reasons);
        ok &= RequireApprovalDecisionNotBlocked(root, "provenance_source_authority", reasons);
        ok &= RequireApprovalDecisionNotBlocked(root, "screenshot_image_usage", reasons);
        return ok;
    }

    private static bool RequireApprovalGranted(JsonElement root, string propertyName, List<string> reasons)
    {
        if (root.TryGetProperty(propertyName, out var value) && IsApprovalGranted(value))
            return true;

        reasons.Add($"approval_required_grant_missing:{propertyName}");
        return false;
    }

    private static bool RequireApprovalDecisionNotBlocked(JsonElement root, string propertyName, List<string> reasons)
    {
        if (!root.TryGetProperty(propertyName, out var value) ||
            value.ValueKind != JsonValueKind.String ||
            ApprovalValueLooksBlockedOrPlaceholder(value.GetString()))
        {
            reasons.Add($"approval_required_decision_invalid:{propertyName}");
            return false;
        }

        return true;
    }

    private static bool ApprovalFieldHasValue(JsonElement value)
        => value.ValueKind switch
        {
            JsonValueKind.String => !string.IsNullOrWhiteSpace(value.GetString()) &&
                                    !ApprovalValueLooksBlockedOrPlaceholder(value.GetString()),
            JsonValueKind.Array => value.EnumerateArray().Any(),
            JsonValueKind.Object => value.EnumerateObject().Any(),
            JsonValueKind.True => true,
            JsonValueKind.False => true,
            JsonValueKind.Number => true,
            _ => false
        };

    private static bool IsApprovalGranted(JsonElement value)
        => value.ValueKind == JsonValueKind.True ||
           (value.ValueKind == JsonValueKind.String &&
            IsAllowedApprovalToken(value.GetString()));

    private static bool IsAllowedApprovalToken(string? value)
        => value is not null &&
           (value.Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase) ||
            value.Equals("approved", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("allowed", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("granted", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("yes", StringComparison.OrdinalIgnoreCase));

    private static bool ApprovalPacketLooksLikeTemplate(JsonElement root)
        => HasTrueProperty(root, "approval_packet_template") ||
           HasTrueProperty(root, "placeholder_not_real") ||
           HasTrueProperty(root, "template_not_approval") ||
           HasTrueProperty(root, "x_scaffold_status") ||
           AnyStringValueLooksLikeTemplate(root);

    private static bool HasTrueProperty(JsonElement root, string propertyName)
        => root.ValueKind == JsonValueKind.Object &&
           root.TryGetProperty(propertyName, out var property) &&
           IsBooleanLikeTrue(property);

    private static bool AnyStringValueLooksLikeTemplate(JsonElement value)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.Object:
                return value.EnumerateObject().Any(property => AnyStringValueLooksLikeTemplate(property.Value));
            case JsonValueKind.Array:
                return value.EnumerateArray().Any(AnyStringValueLooksLikeTemplate);
            case JsonValueKind.String:
                var text = value.GetString();
                return text is not null &&
                       (text.Contains("{{", StringComparison.Ordinal) ||
                        text.Contains("<approver>", StringComparison.OrdinalIgnoreCase) ||
                        text.Contains("todo", StringComparison.OrdinalIgnoreCase) ||
                        text.Contains("placeholder", StringComparison.OrdinalIgnoreCase) ||
                        text.Contains("template", StringComparison.OrdinalIgnoreCase) ||
                        text.Contains("scaffold", StringComparison.OrdinalIgnoreCase));
            default:
                return false;
        }
    }

    private static bool ApprovalValueLooksBlockedOrPlaceholder(string? value)
        => value is null ||
           value.Contains("denied", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("blocked", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("missing", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("unknown", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("todo", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("placeholder", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("template", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("scaffold", StringComparison.OrdinalIgnoreCase);

    private static async Task<DesignConversionRealS3EvidenceBundleArtifactResult> ValidateSchemaFileAsync(
        string root,
        string fileName,
        CancellationToken ct)
    {
        var path = Path.Combine(root, fileName);
        if (!File.Exists(path))
        {
            return MissingArtifact(fileName, path, isSchemaScaffold: true);
        }

        var reasons = new List<string>();
        var content = await File.ReadAllTextAsync(path, ct);
        var parsed = TryParseJsonObject(content, fileName, reasons, out var document);
        using (document)
        {
            var shapeValid = parsed;
            if (parsed)
            {
                if (!document!.RootElement.TryGetProperty("x_scaffold_status", out var scaffold) ||
                    !IsTrue(scaffold, "placeholder_not_real"))
                    reasons.Add($"SCHEMA_SCAFFOLD_MARKER_MISSING:{fileName}");
                else
                    reasons.Add($"SCHEMA_PLACEHOLDER_NOT_REAL:{fileName}");
            }

            return new DesignConversionRealS3EvidenceBundleArtifactResult(
                fileName,
                path,
                Exists: true,
                IsSchemaScaffold: true,
                IsRequiredRealDataFile: false,
                JsonParsed: parsed,
                RowCount: parsed ? 1 : 0,
                ShapeValid: shapeValid,
                Reasons: reasons.Distinct(StringComparer.Ordinal).ToArray());
        }
    }

    private static async Task<(DesignConversionRealS3EvidenceBundleArtifactResult Artifact, bool Ready, IReadOnlyList<string> Reasons, IReadOnlyList<string> Flags)>
        ValidateJsonObjectArtifactAsync(
            string root,
            string fileName,
            bool required,
            Func<JsonElement, string, List<string>, List<string>, bool> validateShape,
            CancellationToken ct)
    {
        var path = Path.Combine(root, fileName);
        if (!File.Exists(path))
        {
            var reason = required
                ? $"REQUIRED_ARTIFACT_MISSING:{fileName}"
                : $"OPTIONAL_ARTIFACT_MISSING:{fileName}";
            return (
                MissingArtifact(fileName, path, isSchemaScaffold: false, reason),
                Ready: false,
                Reasons: [reason],
                Flags: []);
        }

        var reasons = new List<string>();
        var flags = new List<string>();
        var content = await File.ReadAllTextAsync(path, ct);
        var parsed = TryParseJsonObject(content, fileName, reasons, out var document);
        using (document)
        {
            var shapeValid = false;
            if (parsed)
            {
                InspectJsonForForbiddenValues(document!.RootElement, fileName, reasons, flags);
                shapeValid = validateShape(document.RootElement, fileName, reasons, flags);
            }

            AddForbiddenTextReasons(fileName, content, reasons, flags);
            shapeValid &= !HasBlockingArtifactReasons(reasons);

            return (
                new DesignConversionRealS3EvidenceBundleArtifactResult(
                    fileName,
                    path,
                    Exists: true,
                    IsSchemaScaffold: false,
                    IsRequiredRealDataFile: true,
                    JsonParsed: parsed,
                    RowCount: parsed ? 1 : 0,
                    ShapeValid: shapeValid,
                    Reasons: reasons.Distinct(StringComparer.Ordinal).ToArray()),
                Ready: shapeValid,
                Reasons: reasons,
                Flags: flags);
        }
    }

    private static async Task<(DesignConversionRealS3EvidenceBundleArtifactResult Artifact, bool Ready, IReadOnlyList<string> Reasons, IReadOnlyList<string> Flags)>
        ValidateJsonlArtifactAsync(
            string root,
            string fileName,
            Func<JsonElement, string, List<string>, List<string>, bool> validateShape,
            CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var path = Path.Combine(root, fileName);
        if (!File.Exists(path))
        {
            var reason = $"REQUIRED_ARTIFACT_MISSING:{fileName}";
            return (
                MissingArtifact(fileName, path, isSchemaScaffold: false, reason),
                Ready: false,
                Reasons: [reason],
                Flags: []);
        }

        var reasons = new List<string>();
        var flags = new List<string>();
        var nonBlankRows = 0;
        var parsedRows = 0;
        var trainRows = 0;
        var heldOutRows = 0;

        ct.ThrowIfCancellationRequested();
        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            options: FileOptions.Asynchronous | FileOptions.SequentialScan);
        using var reader = new StreamReader(
            stream,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true);

        while (await reader.ReadLineAsync(ct) is { } line)
        {
            ct.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(line))
                continue;

            nonBlankRows++;
            try
            {
                using var document = JsonDocument.Parse(line, new JsonDocumentOptions { AllowTrailingCommas = false });
                if (document.RootElement.ValueKind != JsonValueKind.Object)
                {
                    reasons.Add($"JSONL_ROW_OBJECT_REQUIRED:{fileName}:{parsedRows + 1}");
                }
                else
                {
                    parsedRows++;
                    InspectJsonForForbiddenValues(document.RootElement, fileName, reasons, flags);
                    if (document.RootElement.TryGetProperty("split", out var split) &&
                        split.ValueKind == JsonValueKind.String)
                    {
                        if (StringEquals(split, "train"))
                            trainRows++;
                        if (StringEquals(split, "held_out"))
                            heldOutRows++;
                    }

                    validateShape(document.RootElement, fileName, reasons, flags);
                }
            }
            catch (JsonException)
            {
                reasons.Add($"JSONL_ROW_PARSE_FAILED:{fileName}:{parsedRows + 1}");
            }

            AddForbiddenTextReasons(fileName, line, reasons, flags);
        }

        if (nonBlankRows == 0)
            reasons.Add($"JSONL_ROWS_MISSING:{fileName}");

        if (fileName.Equals(DpoRowsFileName, StringComparison.OrdinalIgnoreCase))
        {
            if (trainRows == 0)
                reasons.Add($"DPO_TRAIN_ROWS_MISSING:{fileName}");
            if (heldOutRows == 0)
                reasons.Add($"DPO_HELD_OUT_ROWS_MISSING:{fileName}");
        }

        var shapeValid = parsedRows == nonBlankRows &&
                         parsedRows > 0 &&
                         !HasBlockingArtifactReasons(reasons);
        return (
            new DesignConversionRealS3EvidenceBundleArtifactResult(
                fileName,
                path,
                Exists: true,
                IsSchemaScaffold: false,
                IsRequiredRealDataFile: true,
                JsonParsed: parsedRows == nonBlankRows,
                RowCount: parsedRows,
                ShapeValid: shapeValid,
                Reasons: reasons.Distinct(StringComparer.Ordinal).ToArray()),
            Ready: shapeValid,
            Reasons: reasons,
            Flags: flags);
    }

    private static DesignConversionRealS3EvidenceBundleArtifactResult MissingArtifact(
        string fileName,
        string path,
        bool isSchemaScaffold,
        string? reason = null)
        => new(
            fileName,
            path,
            Exists: false,
            IsSchemaScaffold: isSchemaScaffold,
            IsRequiredRealDataFile: !isSchemaScaffold,
            JsonParsed: false,
            RowCount: 0,
            ShapeValid: false,
            Reasons: [reason ?? $"ARTIFACT_MISSING:{fileName}"]);

    private static bool ValidateSourceManifestShape(
        JsonElement root,
        string fileName,
        List<string> reasons,
        List<string> flags)
    {
        var ok = RequireNumber(root, "schema_version", 1, fileName, reasons);
        ok &= RequireString(root, "bundle_id", fileName, reasons);
        ok &= RequireString(root, "dataset_id", fileName, reasons);
        ok &= RequireString(root, "source_design_id", fileName, reasons);
        ok &= RequireString(root, "source_kind", fileName, reasons);
        ok &= RequireEvidenceStatus(root, "real_source_manifest_ready", fileName, reasons);
        ok &= RequireObject(root, "source_design_tree", fileName, reasons, out var tree);
        if (tree.HasValue)
        {
            ok &= RequireString(tree.Value, "tree_ref", fileName, reasons);
            ok &= RequireSha256(tree.Value, "tree_sha256", fileName, reasons);
            ok &= RequireSha256(tree.Value, "source_identity_hash", fileName, reasons);
            ok &= RequireFalse(tree.Value, "raw_prompt_included", fileName, reasons);
            ok &= RequireFalse(tree.Value, "absolute_path_included", fileName, reasons);
        }

        ok &= RequireNonEmptyArray(root, "screenshots", fileName, reasons);
        ok &= RequireObject(root, "provenance", fileName, reasons, out var provenance);
        if (provenance.HasValue)
        {
            ok &= RequireString(provenance.Value, "source_manifest_ref", fileName, reasons);
            ok &= RequireString(provenance.Value, "source_uri_or_local_ref", fileName, reasons);
            ok &= RequireSha256(provenance.Value, "source_sha256", fileName, reasons);
            ok &= RequireNonEmptyArray(provenance.Value, "provenance_refs", fileName, reasons);
            ok &= RequireString(provenance.Value, "reviewer_ref", fileName, reasons);
            ok &= RequireFalse(provenance.Value, "raw_user_id_included", fileName, reasons);
            ok &= RequireFalse(provenance.Value, "absolute_path_included", fileName, reasons);
        }

        ok &= RequireObject(root, "license", fileName, reasons, out var license);
        if (license.HasValue)
        {
            ok &= RequireString(license.Value, "license_or_authority_ref", fileName, reasons);
            ok &= RequireTrue(license.Value, "allows_training", fileName, reasons);
            ok &= RequireTrue(license.Value, "allows_evaluation", fileName, reasons);
            ok &= RequireFalse(license.Value, "contains_user_private_data", fileName, reasons);
        }

        ok &= RequireObject(root, "privacy", fileName, reasons, out var privacy);
        if (privacy.HasValue)
        {
            ok &= RequireFalse(privacy.Value, "raw_prompt_included", fileName, reasons);
            ok &= RequireFalse(privacy.Value, "raw_file_system_path_included", fileName, reasons);
            ok &= RequireFalse(privacy.Value, "raw_user_id_included", fileName, reasons);
            ok &= RequireFalse(privacy.Value, "secrets_included", fileName, reasons);
            ok &= RequireStringEquals(privacy.Value, "redaction_policy", "bundle_refs_and_hashes_only", fileName, reasons);
        }

        return ok;
    }

    private static bool ValidateDpoRowShape(
        JsonElement root,
        string fileName,
        List<string> reasons,
        List<string> flags)
    {
        var ok = RequireNumber(root, "schema_version", 1, fileName, reasons);
        ok &= RequireString(root, "dataset_id", fileName, reasons);
        ok &= RequireStringIn(root, "split", ["train", "held_out"], fileName, reasons);
        ok &= RequireString(root, "row_id", fileName, reasons);
        ok &= RequireSha256(root, "record_identity_hash", fileName, reasons);
        ok &= RequireString(root, "unit_id", fileName, reasons);
        ok &= RequireStringEquals(root, "domain_tag", DesignConversionMachine.DomainTag, fileName, reasons);
        ok &= RequireStringIn(root, "source_kind", ["figma", "canva", "other"], fileName, reasons);
        ok &= RequireSha256(root, "source_identity_hash", fileName, reasons);
        ok &= RequireString(root, "source_group_id", fileName, reasons);
        ok &= RequireString(root, "pair_family", fileName, reasons);
        ok &= RequireString(root, "teaching_point", fileName, reasons);
        ok &= RequireObject(root, "prompt", fileName, reasons, out var prompt);
        if (prompt.HasValue)
        {
            ok &= RequireFalse(prompt.Value, "raw_prompt_included", fileName, reasons);
            ok &= RequireObject(prompt.Value, "target", fileName, reasons, out var target);
            if (target.HasValue)
            {
                ok &= RequireString(target.Value, "unit_id", fileName, reasons);
                ok &= RequireString(target.Value, "machine_part", fileName, reasons);
            }

            ok &= RequireObject(prompt.Value, "source", fileName, reasons, out var source);
            if (source.HasValue)
            {
                ok &= RequireStringIn(source.Value, "kind", ["figma", "canva", "other"], fileName, reasons);
                ok &= RequireSha256(source.Value, "identity_hash", fileName, reasons);
                ok &= RequireSha256(source.Value, "structure_hash", fileName, reasons);
            }

            ok &= RequireObject(prompt.Value, "result_features", fileName, reasons, out var resultFeatures);
            if (resultFeatures.HasValue)
            {
                ok &= RequireString(resultFeatures.Value, "chosen_result_ref", fileName, reasons);
                ok &= RequireString(resultFeatures.Value, "rejected_result_ref", fileName, reasons);
                ok &= RequireString(resultFeatures.Value, "negative_quality_reason_hash", fileName, reasons);
                ok &= RequireString(resultFeatures.Value, "pair_family", fileName, reasons);
                if (StringLooksPlaceholder(resultFeatures.Value, "negative_quality_reason_hash"))
                {
                    reasons.Add($"NEGATIVE_PAIR_QUALITY_MISSING:{fileName}:negative_quality_reason_hash");
                    ok = false;
                }
            }
        }

        ok &= RequireObject(root, "chosen", fileName, reasons, out var chosen);
        if (chosen.HasValue)
            ok &= RequireSafeCandidateResult(chosen.Value, fileName, "chosen", expectedAccepted: true, reasons);

        ok &= RequireObject(root, "rejected", fileName, reasons, out var rejected);
        if (rejected.HasValue)
            ok &= RequireSafeCandidateResult(rejected.Value, fileName, "rejected", expectedAccepted: false, reasons);

        ok &= RequireObject(root, "candidate_result", fileName, reasons, out var candidateResult);
        if (candidateResult.HasValue)
        {
            ok &= RequireObject(candidateResult.Value, "chosen", fileName, reasons, out var candidateChosen);
            if (candidateChosen.HasValue)
                ok &= RequireSafeCandidateResult(candidateChosen.Value, fileName, "candidate_result.chosen", expectedAccepted: true, reasons);
            ok &= RequireObject(candidateResult.Value, "rejected", fileName, reasons, out var candidateRejected);
            if (candidateRejected.HasValue)
                ok &= RequireSafeCandidateResult(candidateRejected.Value, fileName, "candidate_result.rejected", expectedAccepted: false, reasons);
        }

        ok &= RequireObject(root, "teacher_labels", fileName, reasons, out var teacherLabels);
        if (teacherLabels.HasValue)
        {
            ok &= RequireStringEquals(teacherLabels.Value, "preference_label", "chosen_over_rejected", fileName, reasons);
            ok &= RequireString(teacherLabels.Value, "rejected_result_ref", fileName, reasons);
            ok &= RequireString(teacherLabels.Value, "pair_family", fileName, reasons);
        }

        ok &= RequireObject(root, "validation_outcome", fileName, reasons, out var validationOutcome);
        if (validationOutcome.HasValue)
        {
            ok &= RequireTrue(validationOutcome.Value, "accepted_for_training", fileName, reasons);
            ok &= RequireFalse(validationOutcome.Value, "quarantined", fileName, reasons);
            ok &= RequireTrue(validationOutcome.Value, "license_allows_training", fileName, reasons);
            ok &= RequireFalse(validationOutcome.Value, "contains_user_private_data", fileName, reasons);
            ok &= RequireTrue(validationOutcome.Value, "unit_id_present", fileName, reasons);
        }

        ok &= RequireObject(root, "metadata", fileName, reasons, out var metadata);
        if (metadata.HasValue)
        {
            foreach (var propertyName in JsonStringFalseFlags)
                ok &= RequireStringEquals(metadata.Value, propertyName, bool.FalseString, fileName, reasons);
        }

        ok &= RequireObject(root, "leakage_guard", fileName, reasons, out var leakageGuard);
        if (leakageGuard.HasValue)
        {
            ok &= RequireFalse(leakageGuard.Value, "raw_prompt_included", fileName, reasons);
            ok &= RequireFalse(leakageGuard.Value, "raw_file_system_path_included", fileName, reasons);
            ok &= RequireFalse(leakageGuard.Value, "raw_user_id_included", fileName, reasons);
            ok &= RequireFalse(leakageGuard.Value, "secrets_included", fileName, reasons);
            ok &= RequireTrue(leakageGuard.Value, "redacted_refs_only", fileName, reasons);
        }

        return ok;
    }

    private static bool RequireSafeCandidateResult(
        JsonElement root,
        string fileName,
        string propertyPath,
        bool expectedAccepted,
        List<string> reasons)
    {
        var ok = RequireString(root, "result_ref", fileName, reasons);
        ok &= RequireSha256(root, "result_hash", fileName, reasons);
        ok &= RequireScore(root, "visual_score", fileName, reasons);
        if (!root.TryGetProperty("accepted", out var accepted) ||
            accepted.ValueKind is not (JsonValueKind.True or JsonValueKind.False) ||
            accepted.GetBoolean() != expectedAccepted)
        {
            reasons.Add($"REQUIRED_ACCEPTED_FLAG_MISMATCH:{fileName}:{propertyPath}:{expectedAccepted}");
            ok = false;
        }

        return ok;
    }

    private static bool ValidateHeldOutMeasurementShape(
        JsonElement root,
        string fileName,
        List<string> reasons,
        List<string> flags)
    {
        var ok = RequireNumber(root, "schema_version", 1, fileName, reasons);
        ok &= RequireString(root, "measurement_id", fileName, reasons);
        ok &= RequireString(root, "dataset_id", fileName, reasons);
        ok &= RequireString(root, "unit_id", fileName, reasons);
        ok &= RequireString(root, "checkpoint_ref", fileName, reasons);
        ok &= RequireStringEquals(root, "measurement_gate_status", DesignConversionMeasurementGateStatus.Passed, fileName, reasons);
        ok &= RequireEvidenceStatus(root, "real_held_out_measurement_ready", fileName, reasons);
        ok &= RequirePromotionBlocked(root, fileName, reasons, flags);

        ok &= RequireObject(root, "held_out_metrics", fileName, reasons, out var metrics);
        if (metrics.HasValue)
        {
            ok &= RequirePositiveInteger(metrics.Value, "held_out_record_count", fileName, reasons);
            ok &= RequireScore(metrics.Value, "held_out_quality_score", fileName, reasons);
            ok &= RequireScore(metrics.Value, "required_held_out_quality_score", fileName, reasons);
            ok &= RequireNonEmptyArray(metrics.Value, "evidence_refs", fileName, reasons);
        }

        ok &= RequireObject(root, "integrity_proof", fileName, reasons, out var proof);
        if (proof.HasValue)
        {
            ok &= RequireTrue(proof.Value, "source_disjoint", fileName, reasons);
            ok &= RequireFalse(proof.Value, "leakage_detected", fileName, reasons);
            ok &= RequireFalse(proof.Value, "hidden_determinism_detected", fileName, reasons);
            ok &= RequireTrue(proof.Value, "ablation_passed", fileName, reasons);
            ok &= RequireNonEmptyArray(proof.Value, "anti_leakage_evidence_refs", fileName, reasons);
            ok &= RequireNonEmptyArray(proof.Value, "ablation_evidence_refs", fileName, reasons);
        }

        ok &= RequireObject(root, "thresholds", fileName, reasons, out _);
        ok &= RequireObject(root, "runtime_cost", fileName, reasons, out var runtimeCost);
        if (runtimeCost.HasValue)
            ok &= RequireTrue(runtimeCost.Value, "budget_passed", fileName, reasons);

        if (ok)
            flags.Add("REAL_HELD_OUT_MEASUREMENT_EVIDENCE_PRESENT");
        return ok;
    }

    private static bool ValidateCheckpointManifestShape(
        JsonElement root,
        string fileName,
        List<string> reasons,
        List<string> flags)
    {
        var ok = RequireNumber(root, "schema_version", 1, fileName, reasons);
        ok &= RequireString(root, "checkpoint_ref", fileName, reasons);
        ok &= RequireSha256(root, "checkpoint_sha256", fileName, reasons);
        ok &= RequireStringEquals(root, "domain_tag", DesignConversionMachine.DomainTag, fileName, reasons);
        ok &= RequireString(root, "machine_id", fileName, reasons);
        ok &= RequireTrue(root, "checkpoint_contains_real_trained_weights", fileName, reasons);
        ok &= RequireEvidenceStatus(root, "real_checkpoint_manifest_ready", fileName, reasons);
        ok &= RequirePositiveInteger(root, "train_row_count", fileName, reasons);
        ok &= RequirePositiveInteger(root, "held_out_row_count", fileName, reasons);
        ok &= RequireNonEmptyArray(root, "measurement_gate_refs", fileName, reasons);
        ok &= RequirePromotionBlocked(root, fileName, reasons, flags);

        ok &= RequireObject(root, "weights", fileName, reasons, out var weights);
        if (weights.HasValue)
        {
            ok &= RequireString(weights.Value, "weights_ref", fileName, reasons);
            ok &= RequireSha256(weights.Value, "weights_sha256", fileName, reasons);
            ok &= RequireTrue(weights.Value, "learned_weights_present", fileName, reasons);
            ok &= RequireFalse(weights.Value, "raw_model_upload_performed", fileName, reasons);
            ok &= RequireNonEmptyArray(weights.Value, "unit_manifests", fileName, reasons);
            if (weights.Value.TryGetProperty("unit_manifests", out var units) &&
                units.ValueKind == JsonValueKind.Array)
            {
                foreach (var unit in units.EnumerateArray())
                {
                    ok &= RequireString(unit, "unit_id", fileName, reasons);
                    ok &= RequireTrue(unit, "learned_weights_present", fileName, reasons);
                    ok &= RequireNonEmptyArray(unit, "evidence_refs", fileName, reasons);
                }
            }
        }

        ok &= RequireObject(root, "fresh_load_probe", fileName, reasons, out var freshLoadProbe);
        if (freshLoadProbe.HasValue)
        {
            ok &= RequireTrue(freshLoadProbe.Value, "fresh_load_performed", fileName, reasons);
            ok &= RequireSha256(freshLoadProbe.Value, "loaded_checkpoint_sha256", fileName, reasons);
            ok &= RequireTrue(freshLoadProbe.Value, "metadata_continuity_passed", fileName, reasons);
            ok &= RequireTrue(freshLoadProbe.Value, "checkpoint_contains_real_trained_weights", fileName, reasons);
            ok &= RequireNonEmptyArray(freshLoadProbe.Value, "evidence_refs", fileName, reasons);
        }

        ok &= RequireObject(root, "continue_training_probe", fileName, reasons, out var continueProbe);
        if (continueProbe.HasValue)
        {
            ok &= RequireTrue(continueProbe.Value, "measurement_gate_run", fileName, reasons);
            ok &= RequireNumber(continueProbe.Value, "held_out_rows_used", 0, fileName, reasons);
            ok &= RequireNonEmptyArray(continueProbe.Value, "evidence_refs", fileName, reasons);
        }

        if (ok)
            flags.Add("REAL_TRAINED_WEIGHTS_PRESENT");
        return ok;
    }

    private static bool TryParseJsonObject(
        string content,
        string fileName,
        List<string> reasons,
        out JsonDocument? document)
    {
        document = null;
        try
        {
            document = JsonDocument.Parse(content, new JsonDocumentOptions { AllowTrailingCommas = false });
            if (document.RootElement.ValueKind == JsonValueKind.Object)
                return true;

            reasons.Add($"JSON_OBJECT_REQUIRED:{fileName}");
            return false;
        }
        catch (JsonException)
        {
            reasons.Add($"JSON_PARSE_FAILED:{fileName}");
            return false;
        }
    }

    private static void AddForbiddenTextReasons(
        string fileName,
        string content,
        List<string> reasons,
        List<string> flags)
    {
        if (RawWindowsAbsolutePath.IsMatch(content))
            reasons.Add($"RAW_WINDOWS_ABSOLUTE_PATH_REJECTED:{fileName}");
        if (PrivateKeyBlock.IsMatch(content))
            reasons.Add($"SECRET_PRIVATE_KEY_REJECTED:{fileName}");
        foreach (Match match in SensitiveAssignment.Matches(content))
            reasons.Add($"SECRET_TOKEN_ASSIGNMENT_REJECTED:{fileName}:{match.Groups[1].Value}");
        if (content.Contains("\"placeholder_not_real\"", StringComparison.OrdinalIgnoreCase) &&
            content.Contains("true", StringComparison.OrdinalIgnoreCase))
            reasons.Add($"PLACEHOLDER_NOT_REAL_PRESENT:{fileName}");

        if (reasons.Any(reason =>
                reason.Contains("UPLOAD", StringComparison.Ordinal) ||
                reason.Contains("PROMOTION", StringComparison.Ordinal)))
            flags.Add("UPLOAD_OR_PROMOTION_ATTEMPTED");
    }

    private static void InspectJsonForForbiddenValues(
        JsonElement element,
        string fileName,
        List<string> reasons,
        List<string> flags,
        string path = "$")
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    var propertyPath = $"{path}.{property.Name}";
                    if (JsonBooleanFlagsThatMustBeFalse.Contains(property.Name) &&
                        IsBooleanLikeTrue(property.Value))
                    {
                        reasons.Add($"FORBIDDEN_FLAG_TRUE:{fileName}:{propertyPath}");
                    }

                    if (JsonStringFalseFlags.Contains(property.Name) &&
                        property.Value.ValueKind == JsonValueKind.String &&
                        !StringEquals(property.Value, bool.FalseString))
                    {
                        reasons.Add($"FORBIDDEN_FLAG_NOT_FALSE:{fileName}:{propertyPath}");
                    }

                    if (RawPromptOrUserIdProperties.Contains(property.Name) &&
                        property.Value.ValueKind == JsonValueKind.String &&
                        !string.IsNullOrWhiteSpace(property.Value.GetString()))
                    {
                        reasons.Add($"RAW_PROMPT_OR_USER_ID_REJECTED:{fileName}:{propertyPath}");
                    }

                    if (SensitiveJsonPropertyNames.Contains(property.Name) &&
                        property.Value.ValueKind == JsonValueKind.String &&
                        !string.IsNullOrWhiteSpace(property.Value.GetString()))
                    {
                        reasons.Add($"SECRET_TOKEN_ASSIGNMENT_REJECTED:{fileName}:{property.Name}");
                    }

                    if (property.Name.Contains("source_kind", StringComparison.OrdinalIgnoreCase) &&
                        StringEquals(property.Value, "unknown"))
                    {
                        reasons.Add($"UNAPPROVED_SOURCE_INDICATOR:{fileName}:{propertyPath}");
                    }

                    if (NameLooksLikeSourceProvenanceOrLicense(property.Name) &&
                        property.Value.ValueKind == JsonValueKind.String &&
                        ContainsUnapprovedIndicator(property.Value.GetString()))
                    {
                        reasons.Add($"UNAPPROVED_SOURCE_PROVENANCE_LICENSE_INDICATOR:{fileName}:{propertyPath}");
                    }

                    InspectJsonForForbiddenValues(property.Value, fileName, reasons, flags, propertyPath);
                }

                break;
            case JsonValueKind.Array:
                var index = 0;
                foreach (var item in element.EnumerateArray())
                {
                    InspectJsonForForbiddenValues(item, fileName, reasons, flags, $"{path}[{index}]");
                    index++;
                }

                break;
        }
    }

    private static bool HasBlockingArtifactReasons(IReadOnlyList<string> reasons)
        => reasons.Any(reason =>
            reason.Contains("MISSING", StringComparison.Ordinal) ||
            reason.Contains("FAILED", StringComparison.Ordinal) ||
            reason.Contains("REJECTED", StringComparison.Ordinal) ||
            reason.Contains("FORBIDDEN", StringComparison.Ordinal) ||
            reason.Contains("NOT_FALSE", StringComparison.Ordinal) ||
            reason.Contains("NOT_READY", StringComparison.Ordinal) ||
            reason.Contains("PLACEHOLDER_NOT_REAL", StringComparison.Ordinal) ||
            reason.Contains("UNAPPROVED", StringComparison.Ordinal) ||
            reason.Contains("REQUIRED", StringComparison.Ordinal) ||
            reason.Contains("INVALID", StringComparison.Ordinal));

    private static bool RequireObject(
        JsonElement root,
        string propertyName,
        string fileName,
        List<string> reasons,
        out JsonElement? value)
    {
        value = null;
        if (!root.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.Object)
        {
            reasons.Add($"REQUIRED_OBJECT_MISSING:{fileName}:{propertyName}");
            return false;
        }

        value = property;
        return true;
    }

    private static bool RequireString(JsonElement root, string propertyName, string fileName, List<string> reasons)
    {
        if (!root.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(property.GetString()))
        {
            reasons.Add($"REQUIRED_STRING_MISSING:{fileName}:{propertyName}");
            return false;
        }

        return true;
    }

    private static bool RequireStringEquals(
        JsonElement root,
        string propertyName,
        string expected,
        string fileName,
        List<string> reasons)
    {
        if (!root.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.String ||
            !StringEquals(property, expected))
        {
            reasons.Add($"REQUIRED_STRING_VALUE_MISMATCH:{fileName}:{propertyName}:{expected}");
            return false;
        }

        return true;
    }

    private static bool RequireStringIn(
        JsonElement root,
        string propertyName,
        IReadOnlyList<string> expectedValues,
        string fileName,
        List<string> reasons)
    {
        if (!root.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.String ||
            !expectedValues.Contains(property.GetString(), StringComparer.Ordinal))
        {
            reasons.Add($"REQUIRED_STRING_VALUE_NOT_ALLOWED:{fileName}:{propertyName}");
            return false;
        }

        return true;
    }

    private static bool RequireNumber(
        JsonElement root,
        string propertyName,
        int expected,
        string fileName,
        List<string> reasons)
    {
        if (!root.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.Number ||
            !property.TryGetInt32(out var actual) ||
            actual != expected)
        {
            reasons.Add($"REQUIRED_NUMBER_VALUE_MISMATCH:{fileName}:{propertyName}:{expected}");
            return false;
        }

        return true;
    }

    private static bool RequirePositiveInteger(
        JsonElement root,
        string propertyName,
        string fileName,
        List<string> reasons)
    {
        if (!root.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.Number ||
            !property.TryGetInt32(out var actual) ||
            actual <= 0)
        {
            reasons.Add($"REQUIRED_POSITIVE_INTEGER_MISSING:{fileName}:{propertyName}");
            return false;
        }

        return true;
    }

    private static bool RequireScore(JsonElement root, string propertyName, string fileName, List<string> reasons)
    {
        if (!root.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.Number ||
            !property.TryGetDouble(out var actual) ||
            actual is < 0 or > 1)
        {
            reasons.Add($"REQUIRED_SCORE_INVALID:{fileName}:{propertyName}");
            return false;
        }

        return true;
    }

    private static bool RequireSha256(JsonElement root, string propertyName, string fileName, List<string> reasons)
    {
        if (!root.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.String ||
            !IsSha256(property.GetString()))
        {
            reasons.Add($"REQUIRED_SHA256_MISSING:{fileName}:{propertyName}");
            return false;
        }

        return true;
    }

    private static bool RequireNonEmptyArray(JsonElement root, string propertyName, string fileName, List<string> reasons)
    {
        if (!root.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.Array ||
            !property.EnumerateArray().Any())
        {
            reasons.Add($"REQUIRED_ARRAY_EMPTY_OR_MISSING:{fileName}:{propertyName}");
            return false;
        }

        return true;
    }

    private static bool RequireTrue(JsonElement root, string propertyName, string fileName, List<string> reasons)
    {
        if (!IsTrue(root, propertyName))
        {
            reasons.Add($"REQUIRED_TRUE_MISSING:{fileName}:{propertyName}");
            return false;
        }

        return true;
    }

    private static bool RequireFalse(JsonElement root, string propertyName, string fileName, List<string> reasons)
    {
        if (!root.TryGetProperty(propertyName, out var property) ||
            !IsBooleanLikeFalse(property))
        {
            reasons.Add($"REQUIRED_FALSE_MISSING:{fileName}:{propertyName}");
            return false;
        }

        return true;
    }

    private static bool RequireEvidenceStatus(
        JsonElement root,
        string expected,
        string fileName,
        List<string> reasons)
        => RequireStringEquals(root, "evidence_status", expected, fileName, reasons);

    private static bool RequirePromotionBlocked(
        JsonElement root,
        string fileName,
        List<string> reasons,
        List<string> flags)
    {
        if (!root.TryGetProperty("promotion_upload", out var property) ||
            property.ValueKind != JsonValueKind.String)
        {
            reasons.Add($"REQUIRED_STRING_MISSING:{fileName}:promotion_upload");
            return false;
        }

        var value = property.GetString();
        if (value is "blocked" or "not_attempted")
            return true;

        reasons.Add($"UPLOAD_OR_PROMOTION_ATTEMPT_REJECTED:{fileName}:promotion_upload");
        flags.Add("UPLOAD_OR_PROMOTION_ATTEMPTED");
        return false;
    }

    private static bool IsTrue(JsonElement root, string propertyName)
        => root.TryGetProperty(propertyName, out var property) && IsBooleanLikeTrue(property);

    private static bool IsBooleanLikeTrue(JsonElement value)
        => value.ValueKind == JsonValueKind.True ||
           (value.ValueKind == JsonValueKind.String &&
            value.GetString()?.Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase) == true);

    private static bool IsBooleanLikeFalse(JsonElement value)
        => value.ValueKind == JsonValueKind.False ||
           (value.ValueKind == JsonValueKind.String &&
            value.GetString()?.Equals(bool.FalseString, StringComparison.OrdinalIgnoreCase) == true);

    private static bool StringEquals(JsonElement value, string expected)
        => value.ValueKind == JsonValueKind.String &&
           value.GetString()?.Equals(expected, StringComparison.Ordinal) == true;

    private static bool StringLooksPlaceholder(JsonElement root, string propertyName)
        => root.TryGetProperty(propertyName, out var property) &&
           property.ValueKind == JsonValueKind.String &&
           ApprovalValueLooksBlockedOrPlaceholder(property.GetString());

    private static bool IsSha256(string? value)
        => value is { Length: 64 } &&
           value.All(character =>
               character is >= '0' and <= '9' ||
               character is >= 'a' and <= 'f');

    private static bool NameLooksLikeSourceProvenanceOrLicense(string propertyName)
        => propertyName.Contains("source", StringComparison.OrdinalIgnoreCase) ||
           propertyName.Contains("provenance", StringComparison.OrdinalIgnoreCase) ||
           propertyName.Contains("license", StringComparison.OrdinalIgnoreCase) ||
           propertyName.Contains("authority", StringComparison.OrdinalIgnoreCase);

    private static bool ContainsUnapprovedIndicator(string? value)
        => value is not null &&
           (value.Contains("unapproved", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("not_approved", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("unknown_license", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("provenance_unknown", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("license_missing", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("placeholder", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("todo", StringComparison.OrdinalIgnoreCase));
}
