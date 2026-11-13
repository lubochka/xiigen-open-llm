namespace SmartSkillsDomain;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;

internal static class SkillSelectionCommonManifestGuard
{
    public static OperationResult<bool> ValidateCheckpointManifest(
        string checkpointPath,
        string manifestPath)
    {
        try
        {
            if (!File.Exists(checkpointPath))
                return OperationResult<bool>.Fail("Checkpoint file not found.", "CHECKPOINT_NOT_FOUND");

            if (!File.Exists(manifestPath))
                return OperationResult<bool>.Fail("Manifest file not found.", "MANIFEST_NOT_FOUND");

            using var document = JsonDocument.Parse(File.ReadAllText(manifestPath, Encoding.UTF8));
            var root = document.RootElement;
            var expectedHash = ReadStringProperty(root, "checkpointSha256", "checkpoint_sha256");
            if (string.IsNullOrWhiteSpace(expectedHash))
                return OperationResult<bool>.Fail("Manifest checkpoint hash is required.", "MANIFEST_CHECKPOINT_SHA256_REQUIRED");

            var fullCheckpointPath = Path.GetFullPath(checkpointPath);
            var fullManifestPath = Path.GetFullPath(manifestPath);
            var manifestDirectory = Path.GetDirectoryName(fullManifestPath) ?? string.Empty;
            var manifestCheckpointRef = ReadStringProperty(root, "checkpointRef", "checkpoint_filename", "checkpointFilename");
            if (!string.IsNullOrWhiteSpace(manifestCheckpointRef))
            {
                var resolvedCheckpointRef = ResolveManifestReference(manifestDirectory, manifestCheckpointRef);
                if (!string.Equals(resolvedCheckpointRef, fullCheckpointPath, StringComparison.OrdinalIgnoreCase))
                    return OperationResult<bool>.Fail("Manifest checkpoint ref mismatch.", "MANIFEST_CHECKPOINT_REF_MISMATCH");
            }

            var manifestRef = ReadStringProperty(root, "manifestRef", "manifest_ref", "manifestFilename", "manifest_filename");
            if (!string.IsNullOrWhiteSpace(manifestRef))
            {
                var resolvedManifestRef = ResolveManifestReference(manifestDirectory, manifestRef);
                if (!string.Equals(resolvedManifestRef, fullManifestPath, StringComparison.OrdinalIgnoreCase))
                    return OperationResult<bool>.Fail("Manifest ref mismatch.", "MANIFEST_REF_MISMATCH");
            }

            if (ReadBoolProperty(root, "semanticQualityClaimed", "semantic_quality_claimed") == true)
                return OperationResult<bool>.Fail("SkillSelection persistence import must not claim full semantic quality.", "SEMANTIC_QUALITY_CLAIM_UNSUPPORTED");

            var actualHash = Sha256File(fullCheckpointPath);
            return string.Equals(expectedHash, actualHash, StringComparison.Ordinal)
                ? OperationResult<bool>.Ok(true)
                : OperationResult<bool>.Fail("Manifest checkpoint hash mismatch.", "MANIFEST_CHECKPOINT_HASH_MISMATCH");
        }
        catch (Exception ex)
        {
            return OperationResult<bool>.Fail(ex.Message, "SKILL_SELECTION_MANIFEST_READ_FAILED");
        }
    }

    private static string? ReadStringProperty(JsonElement root, params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            if (root.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String)
                return value.GetString();
        }

        return null;
    }

    private static bool? ReadBoolProperty(JsonElement root, params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            if (root.TryGetProperty(propertyName, out var value)
                && (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False))
            {
                return value.GetBoolean();
            }
        }

        return null;
    }

    private static string ResolveManifestReference(string manifestDirectory, string reference)
        => Path.GetFullPath(Path.IsPathRooted(reference) ? reference : Path.Combine(manifestDirectory, reference));

    private static string Sha256File(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }
}
