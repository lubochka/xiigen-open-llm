// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

public sealed record RepositoryModelIndexValidatedRow(
    string ModelId,
    string ManifestPath,
    string CheckpointPath,
    string ManifestSha256,
    string CheckpointSha256,
    string ManifestCheckpointSha256,
    string ManifestFullPath,
    string CheckpointFullPath);

public sealed record RepositoryModelIndexValidationReport(
    string RepositoryRoot,
    string IndexPath,
    IReadOnlyList<RepositoryModelIndexValidatedRow> Models)
{
    public bool TryGetByManifestPath(
        string manifestPath,
        out RepositoryModelIndexValidatedRow? row)
    {
        var normalized = RepositoryModelIndexIntegrityGuard.NormalizeManifestLookupRef(manifestPath);
        if (normalized is null)
        {
            row = null;
            return false;
        }

        row = Models.FirstOrDefault(candidate => string.Equals(
            candidate.ManifestPath,
            normalized,
            StringComparison.Ordinal));
        return row is not null;
    }
}

public sealed record RepositoryModelIndexLoadedJson(
    string RepositoryRoot,
    string IndexPath,
    string Json);

public static class RepositoryModelIndexIntegrityGuard
{
    public const long MaxIndexBytes = 1_048_576;

    public const string MissingIndexErrorCode = "REPOSITORY_MODEL_INDEX_MISSING";
    public const string InvalidErrorCode = "REPOSITORY_MODEL_INDEX_INVALID";
    public const string RequiredFieldMissingErrorCode = "REPOSITORY_MODEL_INDEX_REQUIRED_FIELD_MISSING";
    public const string DuplicateModelIdErrorCode = "REPOSITORY_MODEL_INDEX_DUPLICATE_MODEL_ID";
    public const string UnsafePathErrorCode = "REPOSITORY_MODEL_INDEX_UNSAFE_PATH";
    public const string PathOutsideRepositoryErrorCode = "REPOSITORY_MODEL_INDEX_PATH_OUTSIDE_REPOSITORY";
    public const string FileMissingErrorCode = "REPOSITORY_MODEL_INDEX_FILE_MISSING";
    public const string HashInvalidErrorCode = "REPOSITORY_MODEL_INDEX_HASH_INVALID";
    public const string HashMismatchErrorCode = "REPOSITORY_MODEL_INDEX_HASH_MISMATCH";
    public const string CheckpointBindingMismatchErrorCode = "REPOSITORY_MODEL_INDEX_CHECKPOINT_BINDING_MISMATCH";
    public const string RowNotFoundErrorCode = "REPOSITORY_MODEL_INDEX_ROW_NOT_FOUND";

    private static readonly StringComparison PathComparison = OperatingSystem.IsWindows()
        ? StringComparison.OrdinalIgnoreCase
        : StringComparison.Ordinal;

    public static OperationResult<RepositoryModelIndexValidationReport> Validate(
        string repositoryRoot,
        string? indexPath = null)
    {
        var loadedIndex = ReadBoundedIndexJson(repositoryRoot, indexPath);
        if (!loadedIndex.IsSuccess)
            return loadedIndex.MapError<RepositoryModelIndexValidationReport>();

        return ValidateLoadedIndex(
            loadedIndex.Value!.RepositoryRoot,
            loadedIndex.Value.IndexPath,
            loadedIndex.Value.Json);
    }

    public static OperationResult<RepositoryModelIndexLoadedJson> ReadBoundedIndexJson(
        string repositoryRoot,
        string? indexPath = null)
    {
        if (string.IsNullOrWhiteSpace(repositoryRoot))
            return OperationResult<RepositoryModelIndexLoadedJson>.Fail(
                "Repository root is required before model index rows can be trusted.",
                InvalidErrorCode);

        try
        {
            var prepared = PrepareIndexLocation(repositoryRoot, indexPath);
            if (!prepared.IsSuccess)
                return prepared.MapError<RepositoryModelIndexLoadedJson>();

            using var indexStream = new FileStream(
                prepared.Value!.IndexPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 4096,
                FileOptions.SequentialScan);
            if (indexStream.Length > MaxIndexBytes)
                return OperationResult<RepositoryModelIndexLoadedJson>.Fail(
                    $"Repository model index is too large to validate safely; max bytes is {MaxIndexBytes}.",
                    InvalidErrorCode);

            using var reader = new StreamReader(
                indexStream,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true,
                bufferSize: 4096,
                leaveOpen: false);
            var indexJson = reader.ReadToEnd();

            return OperationResult<RepositoryModelIndexLoadedJson>.Ok(
                new RepositoryModelIndexLoadedJson(
                    prepared.Value.RepositoryRoot,
                    prepared.Value.IndexPath,
                    indexJson));
        }
        catch (Exception ex) when (
            ex is IOException
            || ex is UnauthorizedAccessException
            || ex is ArgumentException
            || ex is NotSupportedException)
        {
            return OperationResult<RepositoryModelIndexLoadedJson>.Fail(
                $"Repository model index could not be loaded: {ex.Message}",
                InvalidErrorCode);
        }
    }

    public static OperationResult<RepositoryModelIndexValidationReport> ValidateLoadedIndex(
        string repositoryRoot,
        string indexPath,
        string indexJson)
    {
        if (string.IsNullOrWhiteSpace(repositoryRoot))
            return OperationResult<RepositoryModelIndexValidationReport>.Fail(
                "Repository root is required before model index rows can be trusted.",
                InvalidErrorCode);

        if (string.IsNullOrWhiteSpace(indexJson))
            return OperationResult<RepositoryModelIndexValidationReport>.Fail(
                "Repository model index JSON text is required.",
                InvalidErrorCode);

        try
        {
            var prepared = PrepareIndexLocation(repositoryRoot, indexPath);
            if (!prepared.IsSuccess)
                return prepared.MapError<RepositoryModelIndexValidationReport>();

            if (Encoding.UTF8.GetByteCount(indexJson) > MaxIndexBytes)
                return OperationResult<RepositoryModelIndexValidationReport>.Fail(
                    $"Repository model index is too large to validate safely; max bytes is {MaxIndexBytes}.",
                    InvalidErrorCode);

            // Some callers need to clone and publish the same JSON snapshot.
            // This overload validates that immutable text rather than racing a
            // second file open for potentially different registry bytes.
            using var document = JsonDocument.Parse(indexJson);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
                return OperationResult<RepositoryModelIndexValidationReport>.Fail(
                    "Repository model index root must be a JSON object.",
                    InvalidErrorCode);

            if (!document.RootElement.TryGetProperty("models", out var models)
                || models.ValueKind != JsonValueKind.Array)
            {
                return OperationResult<RepositoryModelIndexValidationReport>.Fail(
                    "Repository model index must contain a models array.",
                    InvalidErrorCode);
            }

            var rows = new List<RepositoryModelIndexValidatedRow>();
            var seenModelIds = new HashSet<string>(StringComparer.Ordinal);
            var rowIndex = 0;
            foreach (var model in models.EnumerateArray())
            {
                rowIndex++;
                if (model.ValueKind != JsonValueKind.Object)
                    return OperationResult<RepositoryModelIndexValidationReport>.Fail(
                        $"Repository model index row {rowIndex} must be an object.",
                        InvalidErrorCode);

                var modelId = ReadRequiredString(model, "model_id", rowIndex);
                if (!modelId.IsSuccess)
                    return modelId.MapError<RepositoryModelIndexValidationReport>();

                if (!seenModelIds.Add(modelId.Value!))
                    return OperationResult<RepositoryModelIndexValidationReport>.Fail(
                        $"Repository model index contains duplicate model_id '{modelId.Value}'.",
                        DuplicateModelIdErrorCode);

                var validatedRow = ValidateModelRow(prepared.Value!.RepositoryRoot, model, rowIndex, modelId.Value!);
                if (!validatedRow.IsSuccess)
                    return validatedRow.MapError<RepositoryModelIndexValidationReport>();

                rows.Add(validatedRow.Value!);
            }

            return OperationResult<RepositoryModelIndexValidationReport>.Ok(
                new RepositoryModelIndexValidationReport(
                    RepositoryRoot: prepared.Value!.RepositoryRoot,
                    IndexPath: prepared.Value.IndexPath,
                    Models: rows));
        }
        catch (JsonException ex)
        {
            return OperationResult<RepositoryModelIndexValidationReport>.Fail(
                $"Repository model index JSON is invalid: {ex.Message}",
                InvalidErrorCode);
        }
        catch (Exception ex) when (
            ex is IOException
            || ex is UnauthorizedAccessException
            || ex is ArgumentException
            || ex is NotSupportedException)
        {
            return OperationResult<RepositoryModelIndexValidationReport>.Fail(
                $"Repository model index could not be validated: {ex.Message}",
                InvalidErrorCode);
        }
    }

    public static OperationResult<RepositoryModelIndexValidatedRow> FindValidatedRowByManifestPath(
        string repositoryRoot,
        string manifestPath)
    {
        var report = Validate(repositoryRoot);
        if (!report.IsSuccess)
            return report.MapError<RepositoryModelIndexValidatedRow>();

        if (report.Value!.TryGetByManifestPath(manifestPath, out var row))
            return OperationResult<RepositoryModelIndexValidatedRow>.Ok(row!);

        return OperationResult<RepositoryModelIndexValidatedRow>.Fail(
            $"Repository model index does not contain a validated row for manifest '{manifestPath}'.",
            RowNotFoundErrorCode);
    }

    internal static string? NormalizeManifestLookupRef(string manifestPath)
    {
        var normalized = NormalizeRepoRelativePath(manifestPath);
        return normalized.IsSuccess ? normalized.Value : null;
    }

    private static OperationResult<PreparedIndexLocation> PrepareIndexLocation(
        string repositoryRoot,
        string? indexPath)
    {
        var fullRepositoryRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(repositoryRoot));
        if (!Directory.Exists(fullRepositoryRoot))
            return OperationResult<PreparedIndexLocation>.Fail(
                $"Repository root does not exist: {repositoryRoot}.",
                InvalidErrorCode);

        var fullIndexPath = Path.GetFullPath(indexPath ?? Path.Combine(fullRepositoryRoot, "models", "index.json"));
        if (!IsInsideRoot(fullRepositoryRoot, fullIndexPath))
            return OperationResult<PreparedIndexLocation>.Fail(
                "Repository model index path must stay inside the repository root.",
                PathOutsideRepositoryErrorCode);

        var indexChain = EnsurePlainExistingPathInsideRoot(
            fullRepositoryRoot,
            fullIndexPath,
            "models/index.json",
            MissingIndexErrorCode);
        if (!indexChain.IsSuccess)
            return indexChain.MapError<PreparedIndexLocation>();

        return OperationResult<PreparedIndexLocation>.Ok(
            new PreparedIndexLocation(fullRepositoryRoot, fullIndexPath));
    }

    private static OperationResult<RepositoryModelIndexValidatedRow> ValidateModelRow(
        string fullRepositoryRoot,
        JsonElement model,
        int rowIndex,
        string modelId)
    {
        var manifestPath = ReadRequiredString(model, "manifest_path", rowIndex);
        if (!manifestPath.IsSuccess)
            return manifestPath.MapError<RepositoryModelIndexValidatedRow>();

        var checkpointPath = ReadRequiredString(model, "checkpoint_path", rowIndex);
        if (!checkpointPath.IsSuccess)
            return checkpointPath.MapError<RepositoryModelIndexValidatedRow>();

        var manifestSha256 = ReadRequiredSha256(model, "manifest_sha256", rowIndex);
        if (!manifestSha256.IsSuccess)
            return manifestSha256.MapError<RepositoryModelIndexValidatedRow>();

        var checkpointSha256 = ReadRequiredSha256(model, "checkpoint_sha256", rowIndex);
        if (!checkpointSha256.IsSuccess)
            return checkpointSha256.MapError<RepositoryModelIndexValidatedRow>();

        var manifestCheckpointSha256 = ReadRequiredSha256(model, "manifest_checkpoint_sha256", rowIndex);
        if (!manifestCheckpointSha256.IsSuccess)
            return manifestCheckpointSha256.MapError<RepositoryModelIndexValidatedRow>();

        if (!string.Equals(manifestCheckpointSha256.Value, checkpointSha256.Value, StringComparison.Ordinal))
            return OperationResult<RepositoryModelIndexValidatedRow>.Fail(
                $"Repository model index row '{modelId}' binds manifest_checkpoint_sha256 to a different checkpoint hash.",
                CheckpointBindingMismatchErrorCode);

        var manifestFullPath = ResolvePlainRepoRelativeFile(
            fullRepositoryRoot,
            manifestPath.Value!,
            "manifest_path",
            modelId);
        if (!manifestFullPath.IsSuccess)
            return manifestFullPath.MapError<RepositoryModelIndexValidatedRow>();

        var checkpointFullPath = ResolvePlainRepoRelativeFile(
            fullRepositoryRoot,
            checkpointPath.Value!,
            "checkpoint_path",
            modelId);
        if (!checkpointFullPath.IsSuccess)
            return checkpointFullPath.MapError<RepositoryModelIndexValidatedRow>();

        // The registry is only metadata until it is tied back to the exact bytes
        // in this checkout; callers must not treat a manifest pointer as a model.
        var actualManifestSha256 = Sha256File(manifestFullPath.Value!);
        if (!string.Equals(actualManifestSha256, manifestSha256.Value, StringComparison.Ordinal))
            return OperationResult<RepositoryModelIndexValidatedRow>.Fail(
                $"Repository model index row '{modelId}' manifest hash does not match the file bytes.",
                HashMismatchErrorCode);

        var actualCheckpointSha256 = Sha256File(checkpointFullPath.Value!);
        if (!string.Equals(actualCheckpointSha256, checkpointSha256.Value, StringComparison.Ordinal))
            return OperationResult<RepositoryModelIndexValidatedRow>.Fail(
                $"Repository model index row '{modelId}' checkpoint hash does not match the file bytes.",
                HashMismatchErrorCode);

        return OperationResult<RepositoryModelIndexValidatedRow>.Ok(
            new RepositoryModelIndexValidatedRow(
                ModelId: modelId,
                ManifestPath: NormalizeRepoRelativePath(manifestPath.Value!).Value!,
                CheckpointPath: NormalizeRepoRelativePath(checkpointPath.Value!).Value!,
                ManifestSha256: manifestSha256.Value!,
                CheckpointSha256: checkpointSha256.Value!,
                ManifestCheckpointSha256: manifestCheckpointSha256.Value!,
                ManifestFullPath: manifestFullPath.Value!,
                CheckpointFullPath: checkpointFullPath.Value!));
    }

    private static OperationResult<string> ReadRequiredString(
        JsonElement model,
        string propertyName,
        int rowIndex)
    {
        if (!model.TryGetProperty(propertyName, out var property)
            || property.ValueKind != JsonValueKind.String)
        {
            return OperationResult<string>.Fail(
                $"Repository model index row {rowIndex} must include a string '{propertyName}'.",
                RequiredFieldMissingErrorCode);
        }

        var value = property.GetString();
        if (string.IsNullOrWhiteSpace(value))
            return OperationResult<string>.Fail(
                $"Repository model index row {rowIndex} has a blank '{propertyName}'.",
                RequiredFieldMissingErrorCode);

        return OperationResult<string>.Ok(value);
    }

    private static OperationResult<string> ReadRequiredSha256(
        JsonElement model,
        string propertyName,
        int rowIndex)
    {
        var value = ReadRequiredString(model, propertyName, rowIndex);
        if (!value.IsSuccess)
            return value;

        if (!IsLowercaseSha256(value.Value!))
            return OperationResult<string>.Fail(
                $"Repository model index row {rowIndex} has an invalid lowercase SHA-256 field '{propertyName}'.",
                HashInvalidErrorCode);

        return value;
    }

    private static OperationResult<string> ResolvePlainRepoRelativeFile(
        string fullRepositoryRoot,
        string path,
        string fieldName,
        string modelId)
    {
        var normalized = NormalizeRepoRelativePath(path);
        if (!normalized.IsSuccess)
            return normalized;

        var candidate = Path.GetFullPath(Path.Combine(
            fullRepositoryRoot,
            normalized.Value!.Replace('/', Path.DirectorySeparatorChar)));
        if (!IsInsideRoot(fullRepositoryRoot, candidate))
            return OperationResult<string>.Fail(
                $"Repository model index row '{modelId}' {fieldName} must stay inside the repository.",
                PathOutsideRepositoryErrorCode);

        var chain = EnsurePlainExistingPathInsideRoot(fullRepositoryRoot, candidate, normalized.Value);
        if (!chain.IsSuccess)
            return chain;

        return OperationResult<string>.Ok(candidate);
    }

    private static OperationResult<string> NormalizeRepoRelativePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return OperationResult<string>.Fail(
                "Repository model index paths must be non-empty repository-relative paths.",
                RequiredFieldMissingErrorCode);

        var normalized = path.Replace('\\', '/');
        if (Path.IsPathRooted(normalized)
            || normalized.Contains(':', StringComparison.Ordinal)
            || normalized.StartsWith("../", StringComparison.Ordinal)
            || normalized.EndsWith("/..", StringComparison.Ordinal)
            || normalized.Contains("/../", StringComparison.Ordinal)
            || normalized.StartsWith("./", StringComparison.Ordinal)
            || normalized.EndsWith("/.", StringComparison.Ordinal)
            || normalized.Contains("/./", StringComparison.Ordinal)
            || normalized.Contains("//", StringComparison.Ordinal))
        {
            return OperationResult<string>.Fail(
                $"Repository model index path '{path}' is not a safe repository-relative path.",
                PathOutsideRepositoryErrorCode);
        }

        var segments = normalized.Split('/');
        if (segments.Length == 0 || segments.Any(segment => string.IsNullOrWhiteSpace(segment)))
            return OperationResult<string>.Fail(
                $"Repository model index path '{path}' contains an empty path segment.",
                UnsafePathErrorCode);

        return OperationResult<string>.Ok(normalized);
    }

    private static OperationResult<string> EnsurePlainExistingPathInsideRoot(
        string fullRepositoryRoot,
        string fullPath,
        string displayPath,
        string missingErrorCode = FileMissingErrorCode)
    {
        var relative = Path.GetRelativePath(fullRepositoryRoot, fullPath).Replace('\\', '/');
        var normalized = NormalizeRepoRelativePath(relative);
        if (!normalized.IsSuccess)
            return normalized;

        var current = fullRepositoryRoot;
        var segments = normalized.Value!.Split('/');
        for (var i = 0; i < segments.Length; i++)
        {
            current = Path.Combine(current, segments[i]);
            FileAttributes attributes;
            try
            {
                attributes = File.GetAttributes(current);
            }
            catch (FileNotFoundException)
            {
                return OperationResult<string>.Fail(
                    $"Repository model index file '{displayPath}' does not exist.",
                    missingErrorCode);
            }
            catch (DirectoryNotFoundException)
            {
                return OperationResult<string>.Fail(
                    $"Repository model index path '{displayPath}' has a missing parent directory.",
                    missingErrorCode);
            }

            // A checked path is still untrusted if a symlink/reparse point can
            // redirect a later open to bytes outside the repository.
            if ((attributes & FileAttributes.ReparsePoint) != 0)
                return OperationResult<string>.Fail(
                    $"Repository model index path '{displayPath}' crosses a reparse point.",
                    UnsafePathErrorCode);

            var isDirectory = (attributes & FileAttributes.Directory) != 0;
            var isLeaf = i == segments.Length - 1;
            if (!isLeaf && !isDirectory)
                return OperationResult<string>.Fail(
                    $"Repository model index path '{displayPath}' has a non-directory parent segment.",
                    UnsafePathErrorCode);

            if (isLeaf && isDirectory)
                return OperationResult<string>.Fail(
                    $"Repository model index file '{displayPath}' must be a regular file, not a directory.",
                    UnsafePathErrorCode);
        }

        return OperationResult<string>.Ok(fullPath);
    }

    private static bool IsInsideRoot(string fullRepositoryRoot, string fullPath)
    {
        var rootWithSeparator = fullRepositoryRoot.EndsWith(Path.DirectorySeparatorChar)
            ? fullRepositoryRoot
            : fullRepositoryRoot + Path.DirectorySeparatorChar;
        return fullPath.StartsWith(rootWithSeparator, PathComparison);
    }

    private static bool IsLowercaseSha256(string value)
        => value.Length == 64
           && value.All(character =>
               character is >= '0' and <= '9' or >= 'a' and <= 'f');

    private static string Sha256File(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private sealed record PreparedIndexLocation(
        string RepositoryRoot,
        string IndexPath);
}
