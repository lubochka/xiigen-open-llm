// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer;

using System.Globalization;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using TrainableRouter.Training;

/// <summary>
/// Один серверный профиль экзамена сохранённой модели документации.
/// Внешний запрос выбирает только domainSlug; все поля корпуса и генерации живут
/// здесь, чтобы клиент не мог подменить закрытый экзамен или проверяющего.
/// </summary>
internal sealed record CodeDocumentationSavedModelAssessmentProfile(
    string DomainSlug,
    string DesignPatternSlug,
    string SubjectSlug,
    string CorpusVersion,
    string ExpectedCorpusManifestSha256,
    CodeDocumentationSavedModelAssessmentGenerationSettings GenerationSettings);

/// <summary>
/// Серверный профиль финальной сертификации сохранённой модели документации.
/// HTTP-клиент не приносит цель, корпус, проверяющего, exposure index или store:
/// эти параметры живут только здесь и в серверном checkpoint passport.
/// </summary>
internal sealed record CodeDocumentationCertificationProfile(
    string DomainSlug,
    string DesignPatternSlug,
    string SubjectSlug,
    string CorpusVersion,
    string ExpectedCorpusManifestSha256,
    string ScopeId,
    string ScopeVersion,
    string CoverageRevision,
    DateTimeOffset CoverageCutUtc,
    string OwnerSnapshotId,
    string OwnerSnapshotVersion,
    CodeDocumentationSavedModelAssessmentGenerationSettings GenerationSettings);

/// <summary>
/// Читает неизменяемые JSON-профили оценки без записи из серверной среды.
/// Рабочий сервер использует Environment.GetEnvironmentVariable; тесты могут
/// передать readEnvironment без подмены через запрос и без создания Qwen-клиента.
/// </summary>
internal static class CodeDocumentationSavedModelAssessmentProfileResolver
{
    internal const string EnvironmentName =
        "CONTINUOUS_LEARNING_CODE_DOCUMENTATION_MODEL_ASSESSMENT_PROFILES_JSON";
    internal const string ConfigurationRequiredErrorCode =
        "CODE_DOCUMENTATION_MODEL_ASSESSMENT_PROFILES_REQUIRED";
    internal const string ConfigurationInvalidErrorCode =
        "CODE_DOCUMENTATION_MODEL_ASSESSMENT_PROFILES_INVALID";
    internal const int MaximumProfileCount = 3;

    private const int MaximumJsonUtf8Bytes = 32_768;
    private const int MaximumIdentityLength = 128;
    private static readonly JsonDocumentOptions JsonOptions = new()
    {
        AllowTrailingCommas = false,
        CommentHandling = JsonCommentHandling.Disallow,
        MaxDepth = 8,
    };
    private static readonly string[] ProfileFields =
    [
        "domainSlug",
        "designPatternSlug",
        "subjectSlug",
        "corpusVersion",
        "expectedCorpusManifestSha256",
        "generation",
        "maxHeldOutCases",
        "maxAnswerChars",
    ];
    private static readonly string[] GenerationFields =
    [
        "maxNewTokens",
        "temperature",
        "topK",
        "topP",
        "seed",
    ];

    internal static OperationResult<IReadOnlyDictionary<string, CodeDocumentationSavedModelAssessmentProfile>>
        ResolveFromEnvironment()
        => Resolve(Environment.GetEnvironmentVariable);

    internal static OperationResult<IReadOnlyDictionary<string, CodeDocumentationSavedModelAssessmentProfile>>
        Resolve(Func<string, string?> readEnvironment)
    {
        ArgumentNullException.ThrowIfNull(readEnvironment);
        string? json;
        try
        {
            json = readEnvironment(EnvironmentName);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Required();
        }

        if (string.IsNullOrWhiteSpace(json))
            return Required();
        if (Encoding.UTF8.GetByteCount(json) > MaximumJsonUtf8Bytes)
            return Invalid();

        try
        {
            using var document = JsonDocument.Parse(json, JsonOptions);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
                return Invalid();

            var items = document.RootElement.EnumerateArray().ToArray();
            if (items.Length is <= 0 or > MaximumProfileCount)
                return Invalid();

            var profiles = new Dictionary<string, CodeDocumentationSavedModelAssessmentProfile>(
                StringComparer.Ordinal);
            foreach (var item in items)
            {
                var profile = ParseProfile(item);
                if (!profile.IsSuccess || profile.Value is null)
                    return profile.MapError<IReadOnlyDictionary<string, CodeDocumentationSavedModelAssessmentProfile>>();
                if (!profiles.TryAdd(profile.Value.DomainSlug, profile.Value))
                    return Invalid();
            }

            return OperationResult<IReadOnlyDictionary<string, CodeDocumentationSavedModelAssessmentProfile>>.Ok(
                profiles);
        }
        catch (JsonException)
        {
            return Invalid();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Invalid();
        }
    }

    private static OperationResult<CodeDocumentationSavedModelAssessmentProfile>
        ParseProfile(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object ||
            !HasExactlyAllowedProperties(element, ProfileFields))
        {
            return InvalidProfile();
        }

        var domain = ReadString(element, "domainSlug");
        var designPattern = ReadString(element, "designPatternSlug");
        var subject = ReadString(element, "subjectSlug");
        var version = ReadString(element, "corpusVersion");
        var manifestHash = ReadString(element, "expectedCorpusManifestSha256");
        if (!IsBoundedIdentity(domain) ||
            !IsBoundedIdentity(designPattern) ||
            !IsBoundedIdentity(subject) ||
            !IsBoundedIdentity(version) ||
            !IsHexSha256(manifestHash))
        {
            return InvalidProfile();
        }

        var domainProfile = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
            .ResolveSupportedDomainProfile(domain!);
        if (!domainProfile.IsSuccess ||
            domainProfile.Value is null ||
            !StringComparer.Ordinal.Equals(
                domainProfile.Value.DesignPatternSlug,
                designPattern))
        {
            return InvalidProfile();
        }

        if (!element.TryGetProperty("generation", out var generation) ||
            generation.ValueKind != JsonValueKind.Object ||
            !HasExactlyAllowedProperties(generation, GenerationFields))
        {
            return InvalidProfile();
        }

        var maxNewTokens = ReadInt(generation, "maxNewTokens");
        var temperature = ReadFloat(generation, "temperature");
        var topK = ReadInt(generation, "topK");
        var topP = ReadFloat(generation, "topP");
        var seed = ReadInt(generation, "seed");
        var maxHeldOut = ReadInt(element, "maxHeldOutCases");
        var maxAnswerChars = ReadInt(element, "maxAnswerChars");
        if (maxNewTokens is null ||
            temperature is null ||
            topK is null ||
            topP is null ||
            seed is null ||
            maxHeldOut is null ||
            maxAnswerChars is null)
        {
            return InvalidProfile();
        }

        var settings = new CodeDocumentationSavedModelAssessmentGenerationSettings(
            maxNewTokens.Value,
            temperature.Value,
            topK.Value,
            topP.Value,
            seed.Value,
            maxHeldOut.Value,
            maxAnswerChars.Value);
        if (!SettingsAreWithinServerBounds(settings))
            return InvalidProfile();

        return OperationResult<CodeDocumentationSavedModelAssessmentProfile>.Ok(
            new CodeDocumentationSavedModelAssessmentProfile(
                domain!,
                designPattern!,
                subject!,
                version!,
                manifestHash!,
                settings));
    }

    private static bool SettingsAreWithinServerBounds(
        CodeDocumentationSavedModelAssessmentGenerationSettings settings)
        => settings.MaxNewTokens is > 0 and <= 2_048 &&
           settings.Temperature is >= 0f and <= 2f &&
           !float.IsNaN(settings.Temperature) &&
           !float.IsInfinity(settings.Temperature) &&
           settings.TopK is >= 0 and <= 10_000 &&
           settings.TopP is > 0f and <= 1f &&
           !float.IsNaN(settings.TopP) &&
           !float.IsInfinity(settings.TopP) &&
           settings.MaxHeldOutCases is >= 1 and <= 200 &&
           settings.MaxAnswerChars is >= 1 and <= 65_536;

    private static bool HasExactlyAllowedProperties(
        JsonElement element,
        IReadOnlyList<string> allowed)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in element.EnumerateObject())
        {
            if (!allowed.Contains(property.Name, StringComparer.Ordinal) ||
                !seen.Add(property.Name))
            {
                return false;
            }
        }

        return seen.Count == allowed.Count;
    }

    private static string? ReadString(JsonElement element, string name)
        => element.TryGetProperty(name, out var value) &&
           value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static int? ReadInt(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value) ||
            value.ValueKind != JsonValueKind.Number ||
            !value.TryGetInt32(out var number))
        {
            return null;
        }

        return number;
    }

    private static float? ReadFloat(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value) ||
            value.ValueKind != JsonValueKind.Number)
        {
            return null;
        }

        var text = value.GetRawText();
        return float.TryParse(
            text,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out var number)
            ? number
            : null;
    }

    private static bool IsBoundedIdentity(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           value.Length <= MaximumIdentityLength &&
           StringComparer.Ordinal.Equals(value, value.Trim()) &&
           !value.Any(char.IsControl);

    private static bool IsHexSha256(string? value)
        => value is { Length: 64 } &&
           value.All(static character =>
               character is >= '0' and <= '9' or >= 'a' and <= 'f');

    private static OperationResult<IReadOnlyDictionary<string, CodeDocumentationSavedModelAssessmentProfile>>
        Required()
        => OperationResult<IReadOnlyDictionary<string, CodeDocumentationSavedModelAssessmentProfile>>.Fail(
            "Профили оценки сохранённой модели документации без записи не настроены на сервере.",
            ConfigurationRequiredErrorCode);

    private static OperationResult<IReadOnlyDictionary<string, CodeDocumentationSavedModelAssessmentProfile>>
        Invalid()
        => OperationResult<IReadOnlyDictionary<string, CodeDocumentationSavedModelAssessmentProfile>>.Fail(
            "JSON профилей оценки сохранённой модели документации без записи некорректен.",
            ConfigurationInvalidErrorCode);

    private static OperationResult<CodeDocumentationSavedModelAssessmentProfile>
        InvalidProfile()
        => OperationResult<CodeDocumentationSavedModelAssessmentProfile>.Fail(
            "Один из профилей оценки сохранённой модели документации без записи некорректен.",
            ConfigurationInvalidErrorCode);
}

/// <summary>
/// Читает отдельные server-owned профили финального clean-exam маршрута.
/// Формат намеренно отделён от diagnostic assessment profile, чтобы route
/// /certify не наследовал teacher-visible authority молча.
/// </summary>
internal static class CodeDocumentationCertificationProfileResolver
{
    internal const string EnvironmentName =
        "CONTINUOUS_LEARNING_CODE_DOCUMENTATION_MODEL_CERTIFICATION_PROFILES_JSON";
    internal const string ConfigurationRequiredErrorCode =
        "CODE_DOCUMENTATION_MODEL_CERTIFICATION_PROFILES_REQUIRED";
    internal const string ConfigurationInvalidErrorCode =
        "CODE_DOCUMENTATION_MODEL_CERTIFICATION_PROFILES_INVALID";
    internal const int MaximumProfileCount = 3;

    private const int MaximumJsonUtf8Bytes = 32_768;
    private const int MaximumIdentityLength = 128;
    private static readonly JsonDocumentOptions JsonOptions = new()
    {
        AllowTrailingCommas = false,
        CommentHandling = JsonCommentHandling.Disallow,
        MaxDepth = 8,
    };
    private static readonly string[] ProfileFields =
    [
        "domainSlug",
        "designPatternSlug",
        "subjectSlug",
        "corpusVersion",
        "expectedCorpusManifestSha256",
        "scopeId",
        "scopeVersion",
        "coverageRevision",
        "coverageCutUtc",
        "ownerSnapshotId",
        "ownerSnapshotVersion",
        "generation",
        "maxHeldOutCases",
        "maxAnswerChars",
    ];
    private static readonly string[] GenerationFields =
    [
        "maxNewTokens",
        "temperature",
        "topK",
        "topP",
        "seed",
    ];

    internal static OperationResult<IReadOnlyDictionary<string, CodeDocumentationCertificationProfile>>
        ResolveFromEnvironment()
        => Resolve(Environment.GetEnvironmentVariable);

    internal static OperationResult<IReadOnlyDictionary<string, CodeDocumentationCertificationProfile>>
        Resolve(Func<string, string?> readEnvironment)
    {
        ArgumentNullException.ThrowIfNull(readEnvironment);
        string? json;
        try
        {
            json = readEnvironment(EnvironmentName);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Required();
        }

        if (string.IsNullOrWhiteSpace(json))
            return Required();
        if (Encoding.UTF8.GetByteCount(json) > MaximumJsonUtf8Bytes)
            return Invalid();

        try
        {
            using var document = JsonDocument.Parse(json, JsonOptions);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
                return Invalid();

            var items = document.RootElement.EnumerateArray().ToArray();
            if (items.Length is <= 0 or > MaximumProfileCount)
                return Invalid();

            var profiles = new Dictionary<string, CodeDocumentationCertificationProfile>(
                StringComparer.Ordinal);
            foreach (var item in items)
            {
                var profile = ParseProfile(item);
                if (!profile.IsSuccess || profile.Value is null)
                    return profile.MapError<IReadOnlyDictionary<string, CodeDocumentationCertificationProfile>>();
                if (!profiles.TryAdd(profile.Value.DomainSlug, profile.Value))
                    return Invalid();
            }

            return OperationResult<IReadOnlyDictionary<string, CodeDocumentationCertificationProfile>>.Ok(
                profiles);
        }
        catch (JsonException)
        {
            return Invalid();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Invalid();
        }
    }

    private static OperationResult<CodeDocumentationCertificationProfile>
        ParseProfile(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object ||
            !HasExactlyAllowedProperties(element, ProfileFields))
        {
            return InvalidProfile();
        }

        var domain = ReadString(element, "domainSlug");
        var designPattern = ReadString(element, "designPatternSlug");
        var subject = ReadString(element, "subjectSlug");
        var version = ReadString(element, "corpusVersion");
        var manifestHash = ReadString(element, "expectedCorpusManifestSha256");
        var scopeId = ReadString(element, "scopeId");
        var scopeVersion = ReadString(element, "scopeVersion");
        var coverageRevision = ReadString(element, "coverageRevision");
        var coverageCutText = ReadString(element, "coverageCutUtc");
        var ownerSnapshotId = ReadString(element, "ownerSnapshotId");
        var ownerSnapshotVersion = ReadString(element, "ownerSnapshotVersion");
        if (!IsBoundedIdentity(domain) ||
            !IsBoundedIdentity(designPattern) ||
            !IsBoundedIdentity(subject) ||
            !IsBoundedIdentity(version) ||
            !IsHexSha256(manifestHash) ||
            !IsBoundedIdentity(scopeId) ||
            !IsBoundedIdentity(scopeVersion) ||
            !IsBoundedIdentity(coverageRevision) ||
            !IsBoundedIdentity(ownerSnapshotId) ||
            !IsBoundedIdentity(ownerSnapshotVersion) ||
            !DateTimeOffset.TryParse(
                coverageCutText,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var coverageCutUtc) ||
            coverageCutUtc.Offset != TimeSpan.Zero)
        {
            return InvalidProfile();
        }

        var domainProfile = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
            .ResolveSupportedDomainProfile(domain!);
        if (!domainProfile.IsSuccess ||
            domainProfile.Value is null ||
            !StringComparer.Ordinal.Equals(
                domainProfile.Value.DesignPatternSlug,
                designPattern))
        {
            return InvalidProfile();
        }

        if (!element.TryGetProperty("generation", out var generation) ||
            generation.ValueKind != JsonValueKind.Object ||
            !HasExactlyAllowedProperties(generation, GenerationFields))
        {
            return InvalidProfile();
        }

        var maxNewTokens = ReadInt(generation, "maxNewTokens");
        var temperature = ReadFloat(generation, "temperature");
        var topK = ReadInt(generation, "topK");
        var topP = ReadFloat(generation, "topP");
        var seed = ReadInt(generation, "seed");
        var maxHeldOut = ReadInt(element, "maxHeldOutCases");
        var maxAnswerChars = ReadInt(element, "maxAnswerChars");
        if (maxNewTokens is null ||
            temperature is null ||
            topK is null ||
            topP is null ||
            seed is null ||
            maxHeldOut is null ||
            maxAnswerChars is null)
        {
            return InvalidProfile();
        }

        var settings = new CodeDocumentationSavedModelAssessmentGenerationSettings(
            maxNewTokens.Value,
            temperature.Value,
            topK.Value,
            topP.Value,
            seed.Value,
            maxHeldOut.Value,
            maxAnswerChars.Value);
        if (!SettingsAreWithinServerBounds(settings))
            return InvalidProfile();

        return OperationResult<CodeDocumentationCertificationProfile>.Ok(
            new CodeDocumentationCertificationProfile(
                domain!,
                designPattern!,
                subject!,
                version!,
                manifestHash!,
                scopeId!,
                scopeVersion!,
                coverageRevision!,
                coverageCutUtc,
                ownerSnapshotId!,
                ownerSnapshotVersion!,
                settings));
    }

    private static bool SettingsAreWithinServerBounds(
        CodeDocumentationSavedModelAssessmentGenerationSettings settings)
        => settings.MaxNewTokens is > 0 and <= 2_048 &&
           settings.Temperature is >= 0f and <= 2f &&
           !float.IsNaN(settings.Temperature) &&
           !float.IsInfinity(settings.Temperature) &&
           settings.TopK is >= 0 and <= 10_000 &&
           settings.TopP is > 0f and <= 1f &&
           !float.IsNaN(settings.TopP) &&
           !float.IsInfinity(settings.TopP) &&
           settings.MaxHeldOutCases is >= 1 and <= 200 &&
           settings.MaxAnswerChars is >= 1 and <= 65_536;

    private static bool HasExactlyAllowedProperties(
        JsonElement element,
        IReadOnlyList<string> allowed)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in element.EnumerateObject())
        {
            if (!allowed.Contains(property.Name, StringComparer.Ordinal) ||
                !seen.Add(property.Name))
            {
                return false;
            }
        }

        return seen.Count == allowed.Count;
    }

    private static string? ReadString(JsonElement element, string name)
        => element.TryGetProperty(name, out var value) &&
           value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static int? ReadInt(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value) ||
            value.ValueKind != JsonValueKind.Number ||
            !value.TryGetInt32(out var number))
        {
            return null;
        }

        return number;
    }

    private static float? ReadFloat(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value) ||
            value.ValueKind != JsonValueKind.Number)
        {
            return null;
        }

        var text = value.GetRawText();
        return float.TryParse(
            text,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out var number)
            ? number
            : null;
    }

    private static bool IsBoundedIdentity(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           value.Length <= MaximumIdentityLength &&
           StringComparer.Ordinal.Equals(value, value.Trim()) &&
           !value.Any(char.IsControl);

    private static bool IsHexSha256(string? value)
        => value is { Length: 64 } &&
           value.All(static character =>
               character is >= '0' and <= '9' or >= 'a' and <= 'f');

    private static OperationResult<IReadOnlyDictionary<string, CodeDocumentationCertificationProfile>>
        Required()
        => OperationResult<IReadOnlyDictionary<string, CodeDocumentationCertificationProfile>>.Fail(
            "Профили финальной сертификации сохранённой модели документации не настроены на сервере.",
            ConfigurationRequiredErrorCode);

    private static OperationResult<IReadOnlyDictionary<string, CodeDocumentationCertificationProfile>>
        Invalid()
        => OperationResult<IReadOnlyDictionary<string, CodeDocumentationCertificationProfile>>.Fail(
            "JSON профилей финальной сертификации сохранённой модели документации некорректен.",
            ConfigurationInvalidErrorCode);

    private static OperationResult<CodeDocumentationCertificationProfile>
        InvalidProfile()
        => OperationResult<CodeDocumentationCertificationProfile>.Fail(
            "Один из профилей финальной сертификации сохранённой модели документации некорректен.",
            ConfigurationInvalidErrorCode);
}
