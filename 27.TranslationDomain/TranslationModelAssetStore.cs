namespace TranslationDomain;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AgentInfrastructure;

public sealed record TranslationModelAssetExportOptions(
    string CheckpointPath,
    string RunnerReportPath,
    string OutputDirectory,
    string TrainingDataArtifactRoot,
    double AccuracyAtExport,
    string SourceBaseCommit,
    DateTimeOffset CreatedAtUtc);

public sealed record TranslationModelAssetExportResult(
    string ManifestPath,
    string CheckpointPath,
    string RunnerReportPath,
    TranslationModelManifest Manifest);

public sealed record TranslationModelAssetPublishResult(
    string RegistryPath,
    string ManifestPath,
    TranslationModelManifest Manifest,
    DateTimeOffset PublishedAtUtc);

public static class TranslationModelAssetStore
{
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    private static readonly JsonSerializerOptions ManifestJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = true
    };

    public static async Task<OperationResult<TranslationModelAssetExportResult>> ExportAsync(
        TranslationModelAssetExportOptions options,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(options.CheckpointPath))
            return OperationResult<TranslationModelAssetExportResult>.Fail(
                "Translation model checkpoint path is required.",
                "TRANSLATION_MODEL_CHECKPOINT_PATH_REQUIRED");

        if (string.IsNullOrWhiteSpace(options.RunnerReportPath))
            return OperationResult<TranslationModelAssetExportResult>.Fail(
                "Translation model runner report path is required.",
                "TRANSLATION_MODEL_RUNNER_REPORT_PATH_REQUIRED");

        if (string.IsNullOrWhiteSpace(options.OutputDirectory))
            return OperationResult<TranslationModelAssetExportResult>.Fail(
                "Translation model output directory is required.",
                "TRANSLATION_MODEL_OUTPUT_DIRECTORY_REQUIRED");

        var checkpointSource = Path.GetFullPath(options.CheckpointPath);
        if (!File.Exists(checkpointSource))
            return OperationResult<TranslationModelAssetExportResult>.Fail(
                "Translation model checkpoint file was not found.",
                "TRANSLATION_MODEL_CHECKPOINT_NOT_FOUND");

        var runnerReportSource = Path.GetFullPath(options.RunnerReportPath);
        if (!File.Exists(runnerReportSource))
            return OperationResult<TranslationModelAssetExportResult>.Fail(
                "Translation model runner report file was not found.",
                "TRANSLATION_MODEL_RUNNER_REPORT_NOT_FOUND");

        var trainingData = await TranslationTrainingDataArtifactStore.LoadAndValidateArtifactAsync(
            options.TrainingDataArtifactRoot,
            ct);
        if (!trainingData.IsSuccess)
            return trainingData.MapError<TranslationModelAssetExportResult>();

        var trainingDataEvidence = TranslationModelManifestContract.CreateTrainingDataEvidence(trainingData.Value!);
        var evidenceValidation = TranslationModelManifestContract.ValidateTrainingDataEvidenceAgainstArtifact(
            trainingDataEvidence,
            trainingData.Value);
        if (!evidenceValidation.IsSuccess)
            return evidenceValidation.MapError<TranslationModelAssetExportResult>();

        var manifest = TranslationModelManifestContract.Create(
            accuracyAtExport: options.AccuracyAtExport,
            sourceBaseCommit: options.SourceBaseCommit,
            createdAtUtc: options.CreatedAtUtc,
            trainingDataEvidence: trainingDataEvidence);
        if (!TranslationModelManifestContract.IsValidSharedAssetManifest(manifest))
            return OperationResult<TranslationModelAssetExportResult>.Fail(
                "Translation model manifest contract validation failed.",
                "TRANSLATION_MODEL_MANIFEST_INVALID");

        try
        {
            var outputDirectory = Path.GetFullPath(options.OutputDirectory);
            Directory.CreateDirectory(outputDirectory);

            var checkpointFile = Path.Combine(outputDirectory, TranslationModelManifestContract.CheckpointFilename);
            var runnerReportFile = Path.Combine(outputDirectory, TranslationModelManifestContract.RunnerReportRef);
            var manifestFile = Path.Combine(outputDirectory, "manifest.json");

            File.Copy(checkpointSource, checkpointFile, overwrite: true);
            File.Copy(runnerReportSource, runnerReportFile, overwrite: true);
            await WriteManifestAsync(manifestFile, manifest, ct);

            return OperationResult<TranslationModelAssetExportResult>.Ok(
                new TranslationModelAssetExportResult(
                    ManifestPath: manifestFile,
                    CheckpointPath: checkpointFile,
                    RunnerReportPath: runnerReportFile,
                    Manifest: manifest));
        }
        catch (OperationCanceledException)
        {
            return OperationResult<TranslationModelAssetExportResult>.Fail(
                "Translation model asset export cancelled.",
                "CANCELLED");
        }
        catch (Exception ex)
        {
            return OperationResult<TranslationModelAssetExportResult>.Fail(
                $"Translation model asset export failed: {ex.Message}",
                "TRANSLATION_MODEL_ASSET_EXPORT_FAILED");
        }
    }

    public static async Task<OperationResult<TranslationModelManifest>> LoadAndValidateManifestForPublishAsync(
        string manifestPath,
        string trainingDataArtifactRoot,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(manifestPath))
            return OperationResult<TranslationModelManifest>.Fail(
                "Translation model manifest path is required.",
                "TRANSLATION_MODEL_MANIFEST_PATH_REQUIRED");

        var fullManifestPath = Path.GetFullPath(manifestPath);
        if (!File.Exists(fullManifestPath))
            return OperationResult<TranslationModelManifest>.Fail(
                "Translation model manifest file was not found.",
                "TRANSLATION_MODEL_MANIFEST_NOT_FOUND");

        var trainingData = await TranslationTrainingDataArtifactStore.LoadAndValidateArtifactAsync(
            trainingDataArtifactRoot,
            ct);
        if (!trainingData.IsSuccess)
            return trainingData.MapError<TranslationModelManifest>();

        TranslationModelManifest? manifest;
        try
        {
            manifest = JsonSerializer.Deserialize<TranslationModelManifest>(
                await File.ReadAllTextAsync(fullManifestPath, Encoding.UTF8, ct),
                ManifestJsonOptions);
        }
        catch (JsonException ex)
        {
            return OperationResult<TranslationModelManifest>.Fail(
                $"Translation model manifest JSON is invalid: {ex.Message}",
                "TRANSLATION_MODEL_MANIFEST_JSON_INVALID");
        }

        if (manifest is null || !TranslationModelManifestContract.IsValidSharedAssetManifest(manifest))
            return OperationResult<TranslationModelManifest>.Fail(
                "Translation model manifest failed the shared asset contract.",
                "TRANSLATION_MODEL_MANIFEST_INVALID");

        var evidenceValidation = TranslationModelManifestContract.ValidateTrainingDataEvidenceAgainstArtifact(
            manifest.TrainingDataEvidence,
            trainingData.Value);
        if (!evidenceValidation.IsSuccess)
            return evidenceValidation.MapError<TranslationModelManifest>();

        return OperationResult<TranslationModelManifest>.Ok(manifest);
    }

    public static async Task<OperationResult<TranslationModelAssetPublishResult>> PublishAsync(
        string repositoryRoot,
        string manifestPath,
        string trainingDataArtifactRoot,
        DateTimeOffset publishedAtUtc,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(repositoryRoot))
            return OperationResult<TranslationModelAssetPublishResult>.Fail(
                "Translation model repository root is required.",
                "TRANSLATION_MODEL_REPOSITORY_ROOT_REQUIRED");

        var manifest = await LoadAndValidateManifestForPublishAsync(manifestPath, trainingDataArtifactRoot, ct);
        if (!manifest.IsSuccess)
            return manifest.MapError<TranslationModelAssetPublishResult>();

        try
        {
            var fullRepositoryRoot = Path.GetFullPath(repositoryRoot);
            var fullManifestPath = Path.GetFullPath(manifestPath);
            var manifestRef = ToRelativeRef(fullRepositoryRoot, fullManifestPath);
            if (!TranslationModelManifestContract.IsSafeRelativeRef(manifestRef))
                return OperationResult<TranslationModelAssetPublishResult>.Fail(
                    "Translation model manifest must be inside the repository root.",
                    "TRANSLATION_MODEL_MANIFEST_OUTSIDE_REPOSITORY");

            var indexPath = Path.Combine(fullRepositoryRoot, "models", "index.json");
            Directory.CreateDirectory(Path.GetDirectoryName(indexPath)!);
            var checkpointPath = Path.GetFullPath(Path.Combine(
                Path.GetDirectoryName(fullManifestPath)!,
                manifest.Value!.CheckpointFilename));
            var checkpointRef = ToRelativeRef(fullRepositoryRoot, checkpointPath);
            if (!TranslationModelManifestContract.IsSafeRelativeRef(checkpointRef))
                return OperationResult<TranslationModelAssetPublishResult>.Fail(
                    "Translation model checkpoint must be inside the repository root.",
                    "TRANSLATION_MODEL_CHECKPOINT_OUTSIDE_REPOSITORY");

            if (!File.Exists(checkpointPath))
                return OperationResult<TranslationModelAssetPublishResult>.Fail(
                    "Translation model checkpoint file was not found.",
                    "TRANSLATION_MODEL_CHECKPOINT_NOT_FOUND");

            var manifestSha256 = Sha256File(fullManifestPath);
            var checkpointSha256 = Sha256File(checkpointPath);
            var registry = LoadRegistry(fullRepositoryRoot, indexPath, ct);
            if (!registry.IsSuccess)
                return registry.MapError<TranslationModelAssetPublishResult>();

            var updatedRegistry = registry.Value!.DeepClone().AsObject();
            var update = UpsertTranslationRegistryEntry(
                updatedRegistry,
                manifestRef,
                checkpointRef,
                manifest.Value.AccuracyAtExport,
                publishedAtUtc,
                manifestSha256,
                checkpointSha256);
            if (!update.IsSuccess)
                return update.MapError<TranslationModelAssetPublishResult>();

            await WriteJsonAsync(indexPath, updatedRegistry, ct);
            return OperationResult<TranslationModelAssetPublishResult>.Ok(
                new TranslationModelAssetPublishResult(
                    RegistryPath: indexPath,
                    ManifestPath: fullManifestPath,
                    Manifest: manifest.Value,
                    PublishedAtUtc: publishedAtUtc));
        }
        catch (OperationCanceledException)
        {
            return OperationResult<TranslationModelAssetPublishResult>.Fail(
                "Translation model asset publish cancelled.",
                "CANCELLED");
        }
        catch (Exception ex)
        {
            return OperationResult<TranslationModelAssetPublishResult>.Fail(
                $"Translation model asset publish failed: {ex.Message}",
                "TRANSLATION_MODEL_ASSET_PUBLISH_FAILED");
        }
    }

    private static async Task WriteManifestAsync(
        string manifestPath,
        TranslationModelManifest manifest,
        CancellationToken ct)
        => await WriteJsonAsync(manifestPath, manifest, ct);

    private static async Task WriteJsonAsync(
        string path,
        object value,
        CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(value, ManifestJsonOptions)
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\r", "\n", StringComparison.Ordinal) + "\n";
        await File.WriteAllTextAsync(path, json, Utf8NoBom, ct);
    }

    private static string ToRelativeRef(string root, string path)
        => Path.GetRelativePath(root, path).Replace('\\', '/');

    private static string Sha256File(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static OperationResult<JsonObject> LoadRegistry(
        string repositoryRoot,
        string indexPath,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var loadedIndex = RepositoryModelIndexIntegrityGuard.ReadBoundedIndexJson(
            repositoryRoot,
            indexPath);
        if (!loadedIndex.IsSuccess)
        {
            if (string.Equals(
                    loadedIndex.ErrorCode,
                    RepositoryModelIndexIntegrityGuard.MissingIndexErrorCode,
                    StringComparison.Ordinal))
            {
                return OperationResult<JsonObject>.Ok(new JsonObject { ["models"] = new JsonArray() });
            }

            return OperationResult<JsonObject>.Fail(
                $"Translation model registry could not be loaded: {loadedIndex.ErrorMessage}",
                "TRANSLATION_MODEL_INDEX_INVALID");
        }

        var indexJson = loadedIndex.Value!.Json;
        if (string.IsNullOrWhiteSpace(indexJson))
        {
            return OperationResult<JsonObject>.Fail(
                "Translation model registry JSON is invalid: empty JSON text.",
                "TRANSLATION_MODEL_INDEX_INVALID");
        }

        JsonNode? parsed;
        try
        {
            parsed = JsonNode.Parse(indexJson);
        }
        catch (JsonException ex)
        {
            return OperationResult<JsonObject>.Fail(
                $"Translation model registry JSON is invalid: {ex.Message}",
                "TRANSLATION_MODEL_INDEX_INVALID");
        }

        if (parsed is not JsonObject registry)
            return OperationResult<JsonObject>.Fail(
                "Translation model registry JSON root must be an object.",
                "TRANSLATION_MODEL_INDEX_INVALID");

        var models = registry["models"];
        if (models is not JsonArray modelsArray)
            return OperationResult<JsonObject>.Fail(
                "Translation model registry models field must be an array.",
                "TRANSLATION_MODEL_INDEX_INVALID");

        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var model in modelsArray)
        {
            if (model is not JsonObject modelObject)
                return OperationResult<JsonObject>.Fail(
                    "Translation model registry entries must be objects.",
                    "TRANSLATION_MODEL_INDEX_INVALID");

            if (!TryGetString(modelObject, "model_id", out var modelId) || string.IsNullOrWhiteSpace(modelId))
                return OperationResult<JsonObject>.Fail(
                    "Translation model registry entries must include a model_id string.",
                    "TRANSLATION_MODEL_INDEX_INVALID");

            if (!seen.Add(modelId))
                return OperationResult<JsonObject>.Fail(
                    $"Translation model registry contains duplicate model_id '{modelId}'.",
                    "TRANSLATION_MODEL_INDEX_DUPLICATE_MODEL_ID");
        }

        // Publication mutates a cloned JsonObject, so this object must be built
        // from the same immutable bytes that the integrity guard accepts.
        var integrity = RepositoryModelIndexIntegrityGuard.ValidateLoadedIndex(
            loadedIndex.Value.RepositoryRoot,
            loadedIndex.Value.IndexPath,
            indexJson);
        if (!integrity.IsSuccess)
            return OperationResult<JsonObject>.Fail(
                $"Translation model registry contains an untrusted existing row: {integrity.ErrorMessage}",
                "TRANSLATION_MODEL_INDEX_EXISTING_ROW_INTEGRITY_FAILED");

        return OperationResult<JsonObject>.Ok(registry);
    }

    private static OperationResult<bool> UpsertTranslationRegistryEntry(
        JsonObject registry,
        string manifestRef,
        string checkpointRef,
        double accuracy,
        DateTimeOffset publishedAtUtc,
        string manifestSha256,
        string checkpointSha256)
    {
        var models = registry["models"]?.AsArray();
        if (models is null)
            return OperationResult<bool>.Fail(
                "Translation model registry models field must be an array.",
                "TRANSLATION_MODEL_INDEX_INVALID");

        JsonObject? existing = null;
        foreach (var model in models)
        {
            var modelObject = model!.AsObject();
            if (TryGetString(modelObject, "model_id", out var modelId) &&
                string.Equals(modelId, TranslationModelManifestContract.ModelId, StringComparison.Ordinal))
            {
                existing = modelObject;
                break;
            }
        }

        var entry = existing ?? new JsonObject
        {
            ["model_id"] = TranslationModelManifestContract.ModelId,
            ["domain"] = TranslationModelManifestContract.Domain,
            ["manifest_path"] = manifestRef,
            ["checkpoint_path"] = checkpointRef,
            ["version"] = TranslationModelManifestContract.Version,
            ["accuracy"] = accuracy,
            ["checkpoint_sha256"] = checkpointSha256,
            ["manifest_sha256"] = manifestSha256,
            ["manifest_checkpoint_sha256"] = checkpointSha256,
            ["published_at_utc"] = publishedAtUtc
        };

        entry["model_id"] = TranslationModelManifestContract.ModelId;
        entry["domain"] = TranslationModelManifestContract.Domain;
        entry["manifest_path"] = manifestRef;
        entry["checkpoint_path"] = checkpointRef;
        entry["version"] = TranslationModelManifestContract.Version;
        entry["accuracy"] = accuracy;
        entry["checkpoint_sha256"] = checkpointSha256;
        entry["manifest_sha256"] = manifestSha256;
        entry["manifest_checkpoint_sha256"] = checkpointSha256;
        entry["published_at_utc"] = publishedAtUtc;

        if (existing is null)
            models.Add(entry);

        return OperationResult<bool>.Ok(true);
    }

    private static bool TryGetString(
        JsonObject value,
        string propertyName,
        out string result)
    {
        result = "";
        if (!value.TryGetPropertyValue(propertyName, out var node) ||
            node is not JsonValue jsonValue ||
            !jsonValue.TryGetValue<string>(out var stringValue))
            return false;

        result = stringValue;
        return true;
    }
}
