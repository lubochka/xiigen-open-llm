// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Inference;
using Tokenizer;
using TransformerModel;

namespace ContinuousLearning;

public sealed record CodeDocumentationStudentGenerationSettings(int MaxNewTokens,
    float Temperature, int TopK, float TopP, int Seed, int MaxAnswerChars);

public sealed record CodeDocumentationGenerationInput(string RequestId, string DomainSlug,
    string OwnerInput, string ContractVersion = CodeDocumentationStudentGeneration.ContractVersion);

public sealed record CodeDocumentationStudentSourceIdentity(string SourceKind, string DomainSlug,
    string ModelId, string ModelVersion, int? CheckpointStep, string? ManifestSha256, string? WeightsSha256);

// Issued only by already trusted loaded-source owners (14 and its existing IVT consumers).
public sealed class CodeDocumentationStudentHandle
{
    private CodeDocumentationStudentHandle(LlmModel model, BpeTokenizer tokenizer,
        CodeDocumentationStudentSourceIdentity identity)
    { Model = model; Tokenizer = tokenizer; Identity = identity; }
    internal LlmModel Model { get; }
    internal BpeTokenizer Tokenizer { get; }
    public CodeDocumentationStudentSourceIdentity Identity { get; }
    internal static CodeDocumentationStudentHandle BindVerified(LlmModel model, BpeTokenizer tokenizer,
        CodeDocumentationStudentSourceIdentity identity) => new(model, tokenizer, identity);
}

public sealed record CodeDocumentationGenerationResult(string RequestId, string DomainSlug,
    string ContractVersion, string FramingVersion, string InputSha256, string? Text,
    CodeDocumentationStudentSourceIdentity? Source, string? TokenizerSha256, string SettingsSha256,
    int? PromptTokenCount, int? GeneratedTokenCount, string StopReason,
    double LoadMilliseconds, double GenerationMilliseconds, double ElapsedMilliseconds, string? ErrorCode);

public static class CodeDocumentationStudentGeneration
{
    public const string ContractVersion = "code-documentation-student-generation-v1";
    public const string FramingVersion = "owner-input-verbatim-no-prefix-v1";
    public const string InvalidErrorCode = "CODE_DOCUMENTATION_GENERATION_INPUT_INVALID";
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    public static string InputSha256(string input)
        => Convert.ToHexString(SHA256.HashData(StrictUtf8.GetBytes(input))).ToLowerInvariant();
    public static string SettingsSha256(CodeDocumentationStudentGenerationSettings settings)
        => Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(settings))).ToLowerInvariant();

    internal static void ValidateInput(CodeDocumentationGenerationInput input, string domain)
    {
        if (input is null || string.IsNullOrWhiteSpace(input.RequestId) || input.RequestId.Length > 256 ||
            input.RequestId.Any(char.IsControl) || input.ContractVersion != ContractVersion ||
            input.DomainSlug != domain || string.IsNullOrEmpty(input.OwnerInput))
            throw new ArgumentException(InvalidErrorCode);
        _ = InputSha256(input.OwnerInput); // Invalid UTF16 is refused; raw whitespace remains unchanged.
    }

    public static TextGenerationReceipt Generate(CodeDocumentationStudentHandle handle,
        CodeDocumentationGenerationInput input, CodeDocumentationStudentGenerationSettings settings,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateInput(input, handle.Identity.DomainSlug);
        if (settings.MaxNewTokens < 0 || !float.IsFinite(settings.Temperature) || settings.Temperature < 0 ||
            settings.TopK < 0 || !float.IsFinite(settings.TopP) || settings.TopP <= 0 || settings.TopP > 1 || settings.MaxAnswerChars <= 0)
            throw new ArgumentException("CODE_DOCUMENTATION_GENERATION_SETTINGS_INVALID");
        var ids = handle.Tokenizer.Encode(input.OwnerInput);
        if (handle.Tokenizer.Decode(ids) != input.OwnerInput) throw new ArgumentException(InvalidErrorCode);
        var receipt = new TextGenerator(handle.Model, handle.Tokenizer).GenerateDetailed(input.OwnerInput,
            settings.MaxNewTokens, settings.Temperature, settings.TopK, settings.TopP, settings.Seed,
            verifiedEosToken: null, cancellationToken); // Current verified manifests declare no EOS.
        // Consumers retain their own existing output-bound/typed-refusal contract.
        return receipt;
    }

    internal static TextGenerationReceipt GeneratePrivate(LlmModel model, BpeTokenizer tokenizer,
        string exactInput, CodeDocumentationSavedModelAssessmentGenerationSettings settings, CancellationToken token)
    {
        var identity = new CodeDocumentationStudentSourceIdentity("private_verified_evaluation", "code-documentation",
            "private", "private", null, null, null);
        return Generate(CodeDocumentationStudentHandle.BindVerified(model, tokenizer, identity),
            new("private-generation", identity.DomainSlug, exactInput),
            new(settings.MaxNewTokens, settings.Temperature, settings.TopK, settings.TopP, settings.Seed, settings.MaxAnswerChars), token);
    }
}
