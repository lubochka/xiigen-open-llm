namespace TranslationDomain;

using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;

public sealed record TranslationTrainingDataArtifactExportResult(
    string ArtifactRoot,
    string ManifestPath,
    string ManifestRepoPath,
    string CorpusPath,
    string CorpusSha256,
    int TrainingPairsCount,
    TranslationTrainingDataManifest Manifest);

public sealed record TranslationTrainingDataArtifactValidationResult(
    string ManifestPath,
    string CorpusPath,
    string CorpusSha256,
    int CorpusRowsCount,
    TranslationTrainingDataManifest Manifest);

public sealed record TranslationTrainingDataArtifactLoadResult(
    string ArtifactRoot,
    string ManifestPath,
    string ManifestRepoPath,
    string CorpusPath,
    string CorpusSha256,
    int CorpusRowsCount,
    TranslationTrainingDataManifest Manifest,
    IReadOnlyList<TranslationTrainReadyPair> TrainingPairs);

public static class TranslationTrainingDataArtifactStore
{
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    private static readonly JsonSerializerOptions ManifestJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = true
    };

    private static readonly JsonSerializerOptions CorpusJsonOptions = new()
    {
        WriteIndented = false
    };

    private static readonly string[] RequiredManifestProperties =
    [
        "schema_version",
        "linked_model_version",
        "corpus_filename",
        "corpus_ref",
        "corpus_sha256",
        "training_pairs_count",
        "compression",
        "content_type",
        "source_category",
        "data_class",
        "storage_scope",
        "privacy_scope",
        "common_train_ready",
        "dpo_train_ready",
        "provenance_ref",
        "split_policy",
        "source_disjoint_group",
        "license_or_project_internal_ref",
        "legal_verdict",
        "created_at_utc"
    ];

    public static async Task<OperationResult<TranslationTrainingDataArtifactExportResult>> ExportTrainReadyPairsAsync(
        string artifactRoot,
        IReadOnlyList<TranslationTrainReadyPair> trainingPairs,
        DateTimeOffset createdAtUtc,
        CancellationToken ct = default)
    {
        var pathResult = BuildArtifactPaths(artifactRoot);
        if (!pathResult.IsSuccess)
            return pathResult.MapError<TranslationTrainingDataArtifactExportResult>();

        var pairsValidation = ValidateTrainingPairs(trainingPairs);
        if (!pairsValidation.IsSuccess)
            return pairsValidation.MapError<TranslationTrainingDataArtifactExportResult>();

        try
        {
            var paths = pathResult.Value!;
            Directory.CreateDirectory(Path.GetDirectoryName(paths.ManifestPath)!);
            await WriteCorpusAsync(paths.CorpusPath, trainingPairs, ct);

            var corpusSha = ComputeSha256(paths.CorpusPath);
            var manifest = TranslationTrainingDataContract.CreateManifest(
                corpusSha,
                trainingPairs.Count,
                createdAtUtc);

            if (!TranslationTrainingDataContract.IsValidSharedTrainingDataManifest(manifest))
                return OperationResult<TranslationTrainingDataArtifactExportResult>.Fail(
                    "Translation training data artifact exporter produced an invalid manifest.",
                    "TRAINING_DATA_MANIFEST_INVALID");

            await WriteManifestAsync(paths.ManifestPath, manifest, ct);

            var validation = await LoadAndValidateArtifactAsync(paths.ArtifactRoot, ct);
            if (!validation.IsSuccess)
                return validation.MapError<TranslationTrainingDataArtifactExportResult>();

            return OperationResult<TranslationTrainingDataArtifactExportResult>.Ok(
                new TranslationTrainingDataArtifactExportResult(
                    ArtifactRoot: paths.ArtifactRoot,
                    ManifestPath: paths.ManifestPath,
                    ManifestRepoPath: TranslationTrainingDataContract.ManifestRepoPath,
                    CorpusPath: paths.CorpusPath,
                    CorpusSha256: corpusSha,
                    TrainingPairsCount: trainingPairs.Count,
                    Manifest: manifest));
        }
        catch (OperationCanceledException)
        {
            return OperationResult<TranslationTrainingDataArtifactExportResult>.Fail(
                "Translation training data artifact export cancelled.",
                "CANCELLED");
        }
        catch (Exception ex)
        {
            return OperationResult<TranslationTrainingDataArtifactExportResult>.Fail(
                $"Translation training data artifact export failed: {ex.Message}",
                "TRAINING_DATA_ARTIFACT_WRITE_FAILED");
        }
    }

    public static async Task<OperationResult<TranslationTrainingDataArtifactValidationResult>> LoadAndValidateArtifactAsync(
        string artifactRoot,
        CancellationToken ct = default)
    {
        var pathResult = BuildArtifactPaths(artifactRoot);
        if (!pathResult.IsSuccess)
            return pathResult.MapError<TranslationTrainingDataArtifactValidationResult>();

        var paths = pathResult.Value!;
        var manifestResult = await LoadAndValidateManifestAsync(paths.ManifestPath, ct);
        if (!manifestResult.IsSuccess)
            return manifestResult.MapError<TranslationTrainingDataArtifactValidationResult>();

        var manifest = manifestResult.Value!;
        var corpusPath = Path.Combine(Path.GetDirectoryName(paths.ManifestPath)!, manifest.CorpusRef);
        if (!File.Exists(corpusPath))
            return OperationResult<TranslationTrainingDataArtifactValidationResult>.Fail(
                "Translation training data corpus referenced by the manifest was not found.",
                "TRAINING_DATA_CORPUS_NOT_FOUND");

        var corpusSha = ComputeSha256(corpusPath);
        if (!string.Equals(corpusSha, manifest.CorpusSha256, StringComparison.Ordinal))
            return OperationResult<TranslationTrainingDataArtifactValidationResult>.Fail(
                "Translation training data corpus sha256 does not match the manifest.",
                "TRAINING_DATA_CORPUS_SHA_MISMATCH");

        var rowCount = await CountGzipJsonlRowsAsync(corpusPath, ct);
        if (rowCount != manifest.TrainingPairsCount)
            return OperationResult<TranslationTrainingDataArtifactValidationResult>.Fail(
                "Translation training data corpus row count does not match the manifest.",
                "TRAINING_DATA_CORPUS_ROW_COUNT_MISMATCH");

        return OperationResult<TranslationTrainingDataArtifactValidationResult>.Ok(
            new TranslationTrainingDataArtifactValidationResult(
                ManifestPath: paths.ManifestPath,
                CorpusPath: corpusPath,
                CorpusSha256: corpusSha,
                CorpusRowsCount: rowCount,
                Manifest: manifest));
    }

    public static async Task<OperationResult<TranslationTrainingDataArtifactLoadResult>> LoadValidatedTrainReadyPairsAsync(
        string artifactRoot,
        CancellationToken ct = default)
    {
        var validation = await LoadAndValidateArtifactAsync(artifactRoot, ct);
        if (!validation.IsSuccess)
            return validation.MapError<TranslationTrainingDataArtifactLoadResult>();

        var rows = await ReadGzipJsonlRowsAsync(validation.Value!.CorpusPath, ct);
        if (!rows.IsSuccess)
            return rows.MapError<TranslationTrainingDataArtifactLoadResult>();

        var pairs = rows.Value!;
        var pairsValidation = ValidateTrainingPairs(pairs);
        if (!pairsValidation.IsSuccess)
            return pairsValidation.MapError<TranslationTrainingDataArtifactLoadResult>();

        if (pairs.Count != validation.Value.CorpusRowsCount)
            return OperationResult<TranslationTrainingDataArtifactLoadResult>.Fail(
                "Translation training data parsed row count does not match the validated corpus row count.",
                "TRAINING_DATA_CORPUS_PARSED_ROW_COUNT_MISMATCH");

        return OperationResult<TranslationTrainingDataArtifactLoadResult>.Ok(
            new TranslationTrainingDataArtifactLoadResult(
                ArtifactRoot: Path.GetFullPath(artifactRoot),
                ManifestPath: validation.Value.ManifestPath,
                ManifestRepoPath: TranslationTrainingDataContract.ManifestRepoPath,
                CorpusPath: validation.Value.CorpusPath,
                CorpusSha256: validation.Value.CorpusSha256,
                CorpusRowsCount: validation.Value.CorpusRowsCount,
                Manifest: validation.Value.Manifest,
                TrainingPairs: pairs));
    }

    public static async Task<OperationResult<TranslationTrainingDataManifest>> LoadAndValidateManifestAsync(
        string manifestPath,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(manifestPath))
            return OperationResult<TranslationTrainingDataManifest>.Fail(
                "Translation training data manifest path is required.",
                "TRAINING_DATA_MANIFEST_PATH_REQUIRED");

        var fullPath = Path.GetFullPath(manifestPath);
        if (!File.Exists(fullPath))
            return OperationResult<TranslationTrainingDataManifest>.Fail(
                "Translation training data manifest file was not found.",
                "TRAINING_DATA_MANIFEST_NOT_FOUND");

        string json;
        try
        {
            json = await File.ReadAllTextAsync(fullPath, Encoding.UTF8, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return OperationResult<TranslationTrainingDataManifest>.Fail(
                $"Translation training data manifest could not be read: {ex.Message}",
                "TRAINING_DATA_MANIFEST_READ_FAILED");
        }

        TranslationTrainingDataManifest? manifest;
        try
        {
            using var document = JsonDocument.Parse(json);
            // ВИД значения корня проверяется ДО чтения свойств. Файл паспорта может быть
            // корректным документом и при этом не объектом — например пустым массивом или
            // пустым значением. Чтение свойства у такого корня по договору библиотеки разбора
            // выбрасывает исключение неверной операции, а перехвачено ниже только исключение
            // разбора текста, поэтому такой файл ронял бы весь вызов вместо отказа.
            if (document.RootElement.ValueKind != JsonValueKind.Object)
                return OperationResult<TranslationTrainingDataManifest>.Fail(
                    $"Translation training data manifest root is '{document.RootElement.ValueKind}', not an object.",
                    "TRAINING_DATA_MANIFEST_FIELD_MISSING");

            foreach (var propertyName in RequiredManifestProperties)
                if (!document.RootElement.TryGetProperty(propertyName, out _))
                    return OperationResult<TranslationTrainingDataManifest>.Fail(
                        $"Translation training data manifest missing required field '{propertyName}'.",
                        "TRAINING_DATA_MANIFEST_FIELD_MISSING");

            manifest = JsonSerializer.Deserialize<TranslationTrainingDataManifest>(json, ManifestJsonOptions);
        }
        catch (JsonException ex)
        {
            return OperationResult<TranslationTrainingDataManifest>.Fail(
                $"Translation training data manifest JSON is invalid: {ex.Message}",
                "TRAINING_DATA_MANIFEST_JSON_INVALID");
        }

        if (!TranslationTrainingDataContract.IsValidSharedTrainingDataManifest(manifest))
            return OperationResult<TranslationTrainingDataManifest>.Fail(
                "Translation training data manifest failed the shared legal/common/DPO contract.",
                "TRAINING_DATA_MANIFEST_INVALID");

        return OperationResult<TranslationTrainingDataManifest>.Ok(manifest!);
    }

    private static OperationResult<ArtifactPaths> BuildArtifactPaths(string artifactRoot)
    {
        if (string.IsNullOrWhiteSpace(artifactRoot))
            return OperationResult<ArtifactPaths>.Fail(
                "Translation training data artifact root is required.",
                "TRAINING_DATA_ARTIFACT_ROOT_REQUIRED");

        var fullRoot = Path.GetFullPath(artifactRoot);
        if (!Path.IsPathFullyQualified(fullRoot))
            return OperationResult<ArtifactPaths>.Fail(
                "Translation training data artifact root must be absolute.",
                "TRAINING_DATA_ARTIFACT_ROOT_MUST_BE_ABSOLUTE");

        if (File.Exists(fullRoot))
            return OperationResult<ArtifactPaths>.Fail(
                "Translation training data artifact root must be a directory.",
                "TRAINING_DATA_ARTIFACT_ROOT_IS_FILE");

        var manifestPath = Path.Combine(
            fullRoot,
            TranslationTrainingDataContract.ManifestRepoPath.Replace('/', Path.DirectorySeparatorChar));
        var manifestDirectory = Path.GetDirectoryName(manifestPath)!;
        var corpusPath = Path.Combine(manifestDirectory, TranslationTrainingDataContract.CorpusFilename);
        return OperationResult<ArtifactPaths>.Ok(new ArtifactPaths(fullRoot, manifestPath, corpusPath));
    }

    private static OperationResult<bool> ValidateTrainingPairs(IReadOnlyList<TranslationTrainReadyPair> trainingPairs)
    {
        if (trainingPairs is null || trainingPairs.Count == 0)
            return OperationResult<bool>.Fail(
                "Translation training data artifact export requires at least one train-ready pair.",
                "TRAINING_DATA_PAIRS_REQUIRED");

        var duplicateId = trainingPairs
            .Where(pair => pair is not null)
            .GroupBy(pair => pair.Id, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicateId is not null)
            return OperationResult<bool>.Fail(
                $"Translation training data artifact contains duplicate pair id '{duplicateId.Key}'.",
                "TRAINING_DATA_PAIR_DUPLICATE_ID");

        foreach (var pair in trainingPairs)
        {
            if (pair is null)
                return OperationResult<bool>.Fail(
                    "Translation training data artifact export does not accept null pairs.",
                    "TRAINING_DATA_PAIR_NULL");

            if (string.IsNullOrWhiteSpace(pair.Id) ||
                string.IsNullOrWhiteSpace(pair.ASide) ||
                string.IsNullOrWhiteSpace(pair.Chosen) ||
                string.IsNullOrWhiteSpace(pair.Rejected) ||
                string.IsNullOrWhiteSpace(pair.Direction))
                return OperationResult<bool>.Fail(
                    $"Translation training data pair '{pair.Id}' is missing required text or direction fields.",
                    "TRAINING_DATA_PAIR_FIELD_MISSING");

            if (!Enum.TryParse<TranslationDirection>(pair.Direction, ignoreCase: false, out _))
                return OperationResult<bool>.Fail(
                    $"Translation training data pair '{pair.Id}' has unsupported direction '{pair.Direction}'.",
                    "TRAINING_DATA_PAIR_DIRECTION_UNSUPPORTED");

            if (!string.Equals(pair.ValidationOutcome, "train_ready", StringComparison.Ordinal) ||
                pair.ValidationErrors is { Count: > 0 })
                return OperationResult<bool>.Fail(
                    $"Translation training data pair '{pair.Id}' is not train-ready.",
                    "TRAINING_DATA_PAIR_NOT_TRAIN_READY");
        }

        return OperationResult<bool>.Ok(true);
    }

    private static async Task WriteCorpusAsync(
        string corpusPath,
        IReadOnlyList<TranslationTrainReadyPair> trainingPairs,
        CancellationToken ct)
    {
        var tempPath = corpusPath + ".tmp";
        await using (var file = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
        await using (var gzip = new GZipStream(file, CompressionLevel.SmallestSize))
        await using (var writer = new StreamWriter(gzip, Utf8NoBom))
        {
            writer.NewLine = "\n";
            foreach (var pair in trainingPairs)
            {
                ct.ThrowIfCancellationRequested();
                await writer.WriteLineAsync(JsonSerializer.Serialize(pair, CorpusJsonOptions));
            }
        }

        File.Move(tempPath, corpusPath, overwrite: true);
    }

    private static async Task WriteManifestAsync(
        string manifestPath,
        TranslationTrainingDataManifest manifest,
        CancellationToken ct)
    {
        var json = NormalizeLineEndings(JsonSerializer.Serialize(manifest, ManifestJsonOptions)) + "\n";
        var tempPath = manifestPath + ".tmp";
        await File.WriteAllTextAsync(tempPath, json, Utf8NoBom, ct);
        File.Move(tempPath, manifestPath, overwrite: true);
    }

    private static string NormalizeLineEndings(string value)
        => value.Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\r", "\n", StringComparison.Ordinal);

    private static async Task<int> CountGzipJsonlRowsAsync(string corpusPath, CancellationToken ct)
    {
        var count = 0;
        await using var file = new FileStream(corpusPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
        await using var gzip = new GZipStream(file, CompressionMode.Decompress);
        using var reader = new StreamReader(gzip, Encoding.UTF8);
        while (await reader.ReadLineAsync(ct) is { } line)
            if (!string.IsNullOrWhiteSpace(line))
                count++;

        return count;
    }

    private static async Task<OperationResult<IReadOnlyList<TranslationTrainReadyPair>>> ReadGzipJsonlRowsAsync(
        string corpusPath,
        CancellationToken ct)
    {
        try
        {
            var rows = new List<TranslationTrainReadyPair>();
            await using var file = new FileStream(corpusPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
            await using var gzip = new GZipStream(file, CompressionMode.Decompress);
            using var reader = new StreamReader(gzip, Encoding.UTF8);

            var lineNumber = 0;
            while (await reader.ReadLineAsync(ct) is { } line)
            {
                lineNumber++;
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                TranslationTrainReadyPair? row;
                try
                {
                    row = JsonSerializer.Deserialize<TranslationTrainReadyPair>(line, CorpusJsonOptions);
                }
                catch (JsonException ex)
                {
                    return OperationResult<IReadOnlyList<TranslationTrainReadyPair>>.Fail(
                        $"Translation training data corpus line {lineNumber} is invalid JSON: {ex.Message}",
                        "TRAINING_DATA_CORPUS_JSON_INVALID");
                }

                if (row is null)
                    return OperationResult<IReadOnlyList<TranslationTrainReadyPair>>.Fail(
                        $"Translation training data corpus line {lineNumber} deserialized to null.",
                        "TRAINING_DATA_CORPUS_ROW_NULL");

                rows.Add(row);
            }

            return OperationResult<IReadOnlyList<TranslationTrainReadyPair>>.Ok(rows);
        }
        catch (OperationCanceledException)
        {
            return OperationResult<IReadOnlyList<TranslationTrainReadyPair>>.Fail(
                "Translation training data corpus load cancelled.",
                "CANCELLED");
        }
        catch (Exception ex)
        {
            return OperationResult<IReadOnlyList<TranslationTrainReadyPair>>.Fail(
                $"Translation training data corpus could not be loaded: {ex.Message}",
                "TRAINING_DATA_CORPUS_READ_FAILED");
        }
    }

    private static string ComputeSha256(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private sealed record ArtifactPaths(
        string ArtifactRoot,
        string ManifestPath,
        string CorpusPath);
}
