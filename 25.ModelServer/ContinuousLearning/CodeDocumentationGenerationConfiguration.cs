// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using LoRA;
using Tokenizer;
using TrainableRouter.Training;
using TransformerModel.Config;

namespace ModelServer;

internal sealed record CodeDocumentationGenerationConfigurationResult( // Existing server adapter сохраняет прежний internal result contract.
    CodeDocumentationGenerationOptions? Options, string? ErrorCode);

// Server configuration supplies no model or teaching authority. START still owns source validation.
public static class CodeDocumentationGenerationConfiguration // Используется server startup и отдельным offline control без дублирования parser.
{
    internal const string EnabledEnvironmentName = "XIIGEN_CODE_DOCUMENTATION_GENERATION_ENABLED";
    internal const string HashEnvironmentName = "XIIGEN_CODE_DOCUMENTATION_GENERATION_CONFIG_SHA256";
    internal const string Version = "code-documentation-generation-server-config-v1";
    internal const string NotConfigured = "CODE_DOCUMENTATION_GENERATION_NOT_CONFIGURED";
    internal const string Invalid = "CODE_DOCUMENTATION_GENERATION_CONFIGURATION_INVALID";
    internal const string Unavailable = "CODE_DOCUMENTATION_GENERATION_CONFIGURATION_UNAVAILABLE";
    internal const int MaximumConfigBytes = 65_536;
    // Same generation bounds as CodeDocumentationSavedModelAssessmentService.SettingsAreValid.
    private const int MaximumNewTokens = 2_048;
    private const int MaximumTopK = 10_000;
    private const int MaximumAnswerChars = 65_536;
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public static AgentInfrastructure.OperationResult<CodeDocumentationGenerationOptions> ReadVerified( // Offline caller получает existing OperationResult без Model либо дополнительной authority.
        Func<string, string?> readEnvironment, string trustedRoot, CancellationToken cancellationToken = default) // Параметры передаются настоящему existing strict reader.
    { // Wrapper не содержит собственного parser/custody/permission решения.
        var resolved = Resolve(readEnvironment, trustedRoot, cancellationToken); // Ровно server startup reader исполняет hash/schema/tokenizer/custody checks.
        return resolved.Options is { } options // Reader сохраняет Denied для ordinary START.
            ? AgentInfrastructure.OperationResult<CodeDocumentationGenerationOptions>.Ok(options) // Verified options не являются принятым model state.
            : AgentInfrastructure.OperationResult<CodeDocumentationGenerationOptions>.Fail("Generation configuration was refused by the production reader.", resolved.ErrorCode ?? Invalid); // Сохраняем фактический код отказа.
    } // Публичная read-only boundary не создаёт artifacts или training state.

    internal static CodeDocumentationGenerationConfigurationResult Resolve( // Legacy server/bootstrap callers сохраняют прежний internal API.
        Func<string, string?> readEnvironment, string trustedRoot,
        CancellationToken cancellationToken = default)
    {
        var enabled = readEnvironment(EnabledEnvironmentName);
        if (string.IsNullOrEmpty(enabled) || string.Equals(enabled, "false", StringComparison.OrdinalIgnoreCase))
            return new(null, NotConfigured);
        if (!string.Equals(enabled, "true", StringComparison.OrdinalIgnoreCase))
            return new(null, Invalid);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var expectedHash = readEnvironment(HashEnvironmentName);
            if (expectedHash is null || expectedHash.Length != 64 ||
                expectedHash.Any(c => c is not (>= '0' and <= '9') and not (>= 'a' and <= 'f')))
                return new(null, Invalid);
            var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(trustedRoot));
            var directory = Path.Combine(root, "config", "code-documentation");
            var bytes = ReadArtifact(root, directory, "generation-v1.json", MaximumConfigBytes, cancellationToken);
            if (!StringComparer.Ordinal.Equals(expectedHash, Hash(bytes))) return new(null, Invalid);
            _ = StrictUtf8.GetString(bytes);
            using var document = JsonDocument.Parse(bytes, new JsonDocumentOptions { MaxDepth = 32 });
            var data = document.RootElement;
            RequireObject(data,
                ["version", "domainSlug", "designPatternSlug", "modelIdSlug", "modelVersion", "modelConfig", "runtimeCompatibility", "settings"],
                ["loraConfig", "adapterSeedAuthority", "baseInitializationSeed"]); // Legacy config без base seed остаётся допустимым.
            if (data.GetProperty("version").GetString() != Version) return new(null, Invalid);
            var modelElement = data.GetProperty("modelConfig");
            RequireObject(modelElement, ["vocabSize", "contextLen", "hiddenDim", "numLayers", "numHeads", "numKvHeads", "ffnHiddenDim", "ropeTheta", "rmsNormEps"]);
            var modelConfig = modelElement.Deserialize<ModelConfig>(Json)!;
            var runtimeElement = data.GetProperty("runtimeCompatibility");
            RequireObject(runtimeElement, ["configSha256", "topologySha256", "tokenizerSha256", "trainerSha256", "schemaSha256", "compatibilityKey"]);
            var runtime = runtimeElement.Deserialize<RepositoryOwnedModelRuntimeCompatibility>(Json)!;
            var settingsElement = data.GetProperty("settings");
            RequireObject(settingsElement, ["maxNewTokens", "temperature", "topK", "topP", "seed", "maxAnswerChars"]);
            var settings = settingsElement.Deserialize<CodeDocumentationStudentGenerationSettings>(Json)!;
            if (settings.MaxNewTokens is <= 0 or > MaximumNewTokens || settings.MaxNewTokens > modelConfig.ContextLen ||
                !float.IsFinite(settings.Temperature) || settings.Temperature is < 0 or > 2 ||
                settings.TopK is < 0 or > MaximumTopK || !float.IsFinite(settings.TopP) || settings.TopP is <= 0 or > 1 ||
                settings.MaxAnswerChars is <= 0 or > MaximumAnswerChars)
                return new(null, Invalid);

            LoraConfig? loraConfig = null;
            LoraPairRandomSeedAuthority? authority = null;
            var hasLora = data.TryGetProperty("loraConfig", out var loraElement);
            var hasAuthority = data.TryGetProperty("adapterSeedAuthority", out var authorityElement);
            if (hasLora != hasAuthority) return new(null, Invalid);
            if (hasLora)
            {
                RequireObject(loraElement, ["rank", "alpha", "dropout", "targetModules"]);
                RequireObject(authorityElement, ["rootInitializationSeed", "policyDropoutStreamDomain", "referenceDropoutStreamDomain"]);
                loraConfig = LoraApplicator.ValidateAndCanonicalize(loraElement.Deserialize<LoraConfig>(Json)!);
                authority = authorityElement.Deserialize<LoraPairRandomSeedAuthority>(Json)!;
                _ = LoraAdapterRandomSeedDerivation.Derive(0, 0, "q_proj", authority.PolicyDropoutStreamDomain);
                _ = LoraAdapterRandomSeedDerivation.Derive(0, 0, "q_proj", authority.ReferenceDropoutStreamDomain);
            }

            ulong? baseSeed = data.TryGetProperty("baseInitializationSeed", out var baseSeedElement) // Принимаем только явный unsigned seed нового clean start.
                ? baseSeedElement.GetUInt64() : null; // Отсутствующее поле не меняет legacy initialization.
            var start = new CodeDocumentationRepositoryOwnedModelCheckpointStartRequest(root,
                data.GetProperty("domainSlug").GetString()!, data.GetProperty("designPatternSlug").GetString()!,
                data.GetProperty("modelIdSlug").GetString()!, data.GetProperty("modelVersion").GetString()!,
                modelConfig, runtime, RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied,
                LoraConfig: loraConfig, AdapterSeedAuthority: authority, BaseInitializationSeed: baseSeed); // Обычный START всё ещё Denied.
            if (!CodeDocumentationRepositoryOwnedModelCheckpointStartService.ValidateRequest(start).IsSuccess)
                return new(null, Invalid);
            var tokenizerBytes = ReadArtifact(root, directory, "tokenizer.json", BpeTokenizer.MaximumCanonicalArtifactBytes, cancellationToken);
            using var tokenizerStream = new MemoryStream(tokenizerBytes, writable: false);
            var tokenizer = BpeTokenizer.ValidateAndLoadCanonicalArtifact(tokenizerStream, modelConfig.VocabSize, cancellationToken);
            if (!StringComparer.Ordinal.Equals(runtime.TokenizerSha256, Hash(tokenizerBytes)) ||
                !StringComparer.Ordinal.Equals(runtime.TokenizerSha256, tokenizer.ComputeStateSha256()))
                return new(null, Invalid);
            cancellationToken.ThrowIfCancellationRequested();
            return new(new(start, tokenizer, settings), null);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return new(null, Unavailable);
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            // No paths or exception details cross the local endpoint boundary.
            return new(null, Invalid);
        }
    }

    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static byte[] ReadArtifact(string root, string directory, string name, long maximumBytes, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var path = Path.GetFullPath(Path.Combine(directory, name));
        if (!HasCustody(root, directory, path)) throw new UnauthorizedAccessException();
        using var stream = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(path, FileMode.Open, FileAccess.Read,
            FileShare.Read, FileOptions.SequentialScan, bufferSize: 4096);
        if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, path))
            throw new UnauthorizedAccessException();
        var bytes = SecureLocalFileBoundary.ReadBoundedStableBytes(stream, maximumBytes);
        if (bytes is null) throw new InvalidDataException();
        token.ThrowIfCancellationRequested();
        if (!HasCustody(root, directory, path) ||
            !SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, path))
            throw new UnauthorizedAccessException();
        return bytes;
    }

    private static bool HasCustody(string root, string directory, string path)
    {
        var relative = Path.GetRelativePath(root, path);
        return SecureLocalFileBoundary.IsSupportedPlatform() && Directory.Exists(root) &&
            !Path.IsPathRooted(relative) && relative != ".." && !relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal) &&
            !SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path) &&
            Directory.Exists(directory) && SecureLocalFileBoundary.HasCustodyOwnerOnlyDirectoryMode(directory) &&
            File.Exists(path) && (File.GetAttributes(path) & FileAttributes.Directory) == 0 &&
            SecureLocalFileBoundary.HasCustodyOwnerOnlyFileMode(path);
    }

    private static void RequireObject(JsonElement element, string[] required, string[]? optional = null)
    {
        if (element.ValueKind != JsonValueKind.Object) throw new JsonException();
        var allowed = new HashSet<string>(required, StringComparer.Ordinal);
        if (optional is not null) allowed.UnionWith(optional);
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in element.EnumerateObject())
            if (!allowed.Contains(property.Name) || !seen.Add(property.Name)) throw new JsonException();
        if (required.Any(name => !seen.Contains(name))) throw new JsonException();
    }
}
