namespace TranslationDomain;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

public enum TranslationSeedCorpusSplit
{
    Train,
    HeldOut
}

public sealed record TranslationSeedTeacherProvenance(
    string SourceType,
    string Author,
    string ModelProvider,
    string ModelName,
    bool ExternalApiUsed,
    bool ClosedProprietaryTeacherUsed,
    string ProvenanceNote);

public sealed record TranslationSeedCorpusRow(
    string SeedId,
    string SourceLanguage,
    string TargetLanguage,
    string InputText,
    string ChosenTranslation,
    string RejectedTranslation,
    string? RejectedReason,
    string? WrongMarker,
    TranslationSeedCorpusSplit Split,
    string SourceCategory,
    string License,
    TranslationSeedTeacherProvenance TeacherProvenance,
    bool SemanticQualityClaimed,
    string? ExpectedLabel)
{
    public string Direction => TranslationSeedCorpusLoader.ToDirectionCode(SourceLanguage, TargetLanguage);
}

public sealed record TranslationSeedDpoPair(
    string PairId,
    string Direction,
    string Input,
    string Chosen,
    string Rejected,
    string RejectedReason,
    string WrongMarker,
    TranslationSeedCorpusSplit Split,
    string SourceCategory,
    string License,
    TranslationSeedTeacherProvenance TeacherProvenance,
    bool SemanticQualityClaimed);

public sealed record TranslationSeedCorpus(IReadOnlyList<TranslationSeedCorpusRow> Rows)
{
    public IReadOnlyList<TranslationSeedCorpusRow> TrainRows =>
        Rows.Where(row => row.Split == TranslationSeedCorpusSplit.Train).ToArray();

    public IReadOnlyList<TranslationSeedCorpusRow> HeldOutRows =>
        Rows.Where(row => row.Split == TranslationSeedCorpusSplit.HeldOut).ToArray();

    public IReadOnlyList<TranslationSeedDpoPair> ToDpoPairs(TranslationSeedCorpusSplit? split = null)
        => Rows
            .Where(row => split is null || row.Split == split)
            .Select(TranslationSeedCorpusLoader.ToDpoPair)
            .ToArray();

    public IReadOnlyList<TranslationTrainReadyPair> ToTrainReadyPairs(TranslationSeedCorpusSplit split)
        => Rows
            .Where(row => row.Split == split)
            .Select(TranslationSeedCorpusLoader.ToTrainReadyPair)
            .ToArray();

    public TranslationLearningBatch BuildStructuredBatch(
        string batchId,
        TranslationSeedCorpusSplit split,
        TranslationSignatureMode signatureMode = TranslationSignatureMode.Structural)
    {
        var pairs = ToTrainReadyPairs(split);
        return TranslationStructuredBatchBuilder.BuildGeneralBatch(
            batchId,
            pairs,
            heldOut: split == TranslationSeedCorpusSplit.HeldOut,
            signatureMode: signatureMode);
    }
}

public static partial class TranslationSeedCorpusLoader
{
    private const string SelfAuthoredCategory = "synthetic_self_authored";
    private const string InternalLicense = "project_internal_self_authored";
    private const string NoTeacherValue = "none";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = false
    };

    public static TranslationSeedCorpus Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (!File.Exists(path))
            throw new FileNotFoundException("Translation seed corpus file not found.", path);

        return ParseJsonl(File.ReadAllText(path, Encoding.UTF8), path);
    }

    public static async Task<TranslationSeedCorpus> LoadAsync(
        string path,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (!File.Exists(path))
            throw new FileNotFoundException("Translation seed corpus file not found.", path);

        var jsonl = await File.ReadAllTextAsync(path, Encoding.UTF8, ct);
        return ParseJsonl(jsonl, path);
    }

    public static TranslationSeedCorpus ParseJsonl(string jsonl, string sourceName = "translation-seed-corpus")
    {
        if (string.IsNullOrWhiteSpace(jsonl))
            throw new InvalidOperationException($"{sourceName}: seed corpus JSONL is empty.");

        var rows = new List<TranslationSeedCorpusRow>();
        var lines = jsonl.Split(["\r\n", "\n"], StringSplitOptions.None);
        for (var index = 0; index < lines.Length; index++)
        {
            var line = lines[index];
            if (string.IsNullOrWhiteSpace(line))
                continue;

            TranslationSeedCorpusJsonRow? dto;
            try
            {
                using var document = JsonDocument.Parse(line);
                // ВИД значения корня проверяется ДО чтения свойств. Строка файла может быть
                // корректным документом и при этом не объектом — например пустым массивом или
                // пустым значением. Чтение свойства у такого корня по договору библиотеки
                // разбора выбрасывает исключение неверной операции, а перехвачено ниже только
                // исключение разбора текста, поэтому такая строка ронял бы загрузку с совсем
                // другой причиной вместо понятного сообщения о негодной строке. Вид корня выше
                // по потоку ничем не гарантирован: это текст из файла.
                if (document.RootElement.ValueKind != JsonValueKind.Object)
                    throw new InvalidOperationException(
                        $"{sourceName}: JSONL line {index + 1} root is " +
                        $"'{document.RootElement.ValueKind}', not an object.");

                EnsureRequiredJsonProperties(document.RootElement, sourceName, index + 1);
                dto = document.RootElement.Deserialize<TranslationSeedCorpusJsonRow>(JsonOptions);
            }
            catch (JsonException exception)
            {
                throw new InvalidOperationException(
                    $"{sourceName}: JSONL line {index + 1} is invalid JSON.",
                    exception);
            }

            if (dto is null)
                throw new InvalidOperationException($"{sourceName}: JSONL line {index + 1} deserialized to null.");

            rows.Add(ToValidatedRow(dto, sourceName, index + 1));
        }

        ValidateCorpus(rows, sourceName);
        return new TranslationSeedCorpus(rows);
    }

    private static void EnsureRequiredJsonProperties(JsonElement root, string sourceName, int lineNumber)
    {
        RequireProperty(root, "seed_id", sourceName, lineNumber);
        RequireProperty(root, "source_language", sourceName, lineNumber);
        RequireProperty(root, "target_language", sourceName, lineNumber);
        RequireProperty(root, "input_text", sourceName, lineNumber);
        RequireProperty(root, "chosen_translation", sourceName, lineNumber);
        RequireProperty(root, "rejected_translation", sourceName, lineNumber);
        RequireProperty(root, "split", sourceName, lineNumber);
        RequireProperty(root, "source_category", sourceName, lineNumber);
        RequireProperty(root, "license", sourceName, lineNumber);
        RequireProperty(root, "teacher_provenance", sourceName, lineNumber);
        RequireProperty(root, "semantic_quality_claimed", sourceName, lineNumber);

        var provenance = root.GetProperty("teacher_provenance");
        if (provenance.ValueKind != JsonValueKind.Object)
            throw RequiredFieldError("teacher_provenance", sourceName, lineNumber);

        RequireProperty(provenance, "source_type", sourceName, lineNumber, "teacher_provenance.source_type");
        RequireProperty(provenance, "author", sourceName, lineNumber, "teacher_provenance.author");
        RequireProperty(provenance, "model_provider", sourceName, lineNumber, "teacher_provenance.model_provider");
        RequireProperty(provenance, "model_name", sourceName, lineNumber, "teacher_provenance.model_name");
        RequireProperty(provenance, "external_api_used", sourceName, lineNumber, "teacher_provenance.external_api_used");
        RequireProperty(provenance, "closed_proprietary_teacher_used", sourceName, lineNumber, "teacher_provenance.closed_proprietary_teacher_used");
        RequireProperty(provenance, "provenance_note", sourceName, lineNumber, "teacher_provenance.provenance_note");
    }

    public static TranslationSeedDpoPair ToDpoPair(TranslationSeedCorpusRow row)
    {
        ArgumentNullException.ThrowIfNull(row);
        return new TranslationSeedDpoPair(
            PairId: row.SeedId,
            Direction: row.Direction,
            Input: row.InputText,
            Chosen: row.ChosenTranslation,
            Rejected: row.RejectedTranslation,
            RejectedReason: RequiredReason(row),
            WrongMarker: row.WrongMarker ?? string.Empty,
            Split: row.Split,
            SourceCategory: row.SourceCategory,
            License: row.License,
            TeacherProvenance: row.TeacherProvenance,
            SemanticQualityClaimed: row.SemanticQualityClaimed);
    }

    public static TranslationTrainReadyPair ToTrainReadyPair(TranslationSeedCorpusRow row)
    {
        ArgumentNullException.ThrowIfNull(row);
        var reason = RequiredReason(row);
        var marker = string.IsNullOrWhiteSpace(row.WrongMarker)
            ? "seed_translation_quality_miss"
            : row.WrongMarker!.Trim();
        return new TranslationTrainReadyPair(
            Id: row.SeedId,
            Source: row.SeedId,
            Direction: row.Direction,
            PairFamily: "language-translate-seed-v1",
            Prompt: $"Translate from {row.SourceLanguage} to {row.TargetLanguage}.",
            ASide: row.InputText,
            Chosen: row.ChosenTranslation,
            Rejected: $"{row.RejectedTranslation} [WRONG: {marker} - {reason}]",
            ValidationOutcome: "train_ready",
            ValidationErrors: []);
    }

    public static string ToDirectionCode(string sourceLanguage, string targetLanguage)
    {
        var source = NormalizeLanguageCode(sourceLanguage);
        var target = NormalizeLanguageCode(targetLanguage);
        return $"{source}_{target}".ToUpperInvariant();
    }

    private static TranslationSeedCorpusRow ToValidatedRow(
        TranslationSeedCorpusJsonRow dto,
        string sourceName,
        int lineNumber)
    {
        Require(dto.SeedId, "seed_id", sourceName, lineNumber);
        Require(dto.SourceLanguage, "source_language", sourceName, lineNumber);
        Require(dto.TargetLanguage, "target_language", sourceName, lineNumber);
        Require(dto.InputText, "input_text", sourceName, lineNumber);
        Require(dto.ChosenTranslation, "chosen_translation", sourceName, lineNumber);
        Require(dto.RejectedTranslation, "rejected_translation", sourceName, lineNumber);
        Require(dto.Split, "split", sourceName, lineNumber);
        Require(dto.SourceCategory, "source_category", sourceName, lineNumber);
        Require(dto.License, "license", sourceName, lineNumber);

        if (string.IsNullOrWhiteSpace(dto.RejectedReason) && string.IsNullOrWhiteSpace(dto.WrongMarker))
            throw RequiredFieldError("rejected_reason_or_wrong_marker", sourceName, lineNumber);

        if (!TryParseSplit(dto.Split!, out var split))
            throw new InvalidOperationException(
                $"{sourceName}: line {lineNumber} field 'split' must be 'train' or 'held_out'.");

        if (split == TranslationSeedCorpusSplit.HeldOut)
        {
            Require(dto.ExpectedLabel, "expected_label", sourceName, lineNumber);
            if (!string.Equals(
                    NormalizeComparable(dto.ExpectedLabel!),
                    NormalizeComparable(dto.ChosenTranslation!),
                    StringComparison.Ordinal))
                throw new InvalidOperationException(
                    $"{sourceName}: line {lineNumber} expected_label must match chosen_translation.");
        }

        if (dto.TeacherProvenance is null)
            throw RequiredFieldError("teacher_provenance", sourceName, lineNumber);

        var provenance = ToValidatedProvenance(dto.TeacherProvenance, sourceName, lineNumber);
        ValidateLegalFields(dto, provenance, sourceName, lineNumber);

        return new TranslationSeedCorpusRow(
            SeedId: dto.SeedId!.Trim(),
            SourceLanguage: NormalizeLanguageCode(dto.SourceLanguage!),
            TargetLanguage: NormalizeLanguageCode(dto.TargetLanguage!),
            InputText: dto.InputText!.Trim(),
            ChosenTranslation: dto.ChosenTranslation!.Trim(),
            RejectedTranslation: dto.RejectedTranslation!.Trim(),
            RejectedReason: dto.RejectedReason?.Trim(),
            WrongMarker: dto.WrongMarker?.Trim(),
            Split: split,
            SourceCategory: dto.SourceCategory!.Trim(),
            License: dto.License!.Trim(),
            TeacherProvenance: provenance,
            SemanticQualityClaimed: dto.SemanticQualityClaimed,
            ExpectedLabel: dto.ExpectedLabel?.Trim());
    }

    private static TranslationSeedTeacherProvenance ToValidatedProvenance(
        TranslationSeedTeacherProvenanceJson dto,
        string sourceName,
        int lineNumber)
    {
        Require(dto.SourceType, "teacher_provenance.source_type", sourceName, lineNumber);
        Require(dto.Author, "teacher_provenance.author", sourceName, lineNumber);
        Require(dto.ModelProvider, "teacher_provenance.model_provider", sourceName, lineNumber);
        Require(dto.ModelName, "teacher_provenance.model_name", sourceName, lineNumber);
        Require(dto.ProvenanceNote, "teacher_provenance.provenance_note", sourceName, lineNumber);

        return new TranslationSeedTeacherProvenance(
            SourceType: dto.SourceType!.Trim(),
            Author: dto.Author!.Trim(),
            ModelProvider: dto.ModelProvider!.Trim(),
            ModelName: dto.ModelName!.Trim(),
            ExternalApiUsed: dto.ExternalApiUsed,
            ClosedProprietaryTeacherUsed: dto.ClosedProprietaryTeacherUsed,
            ProvenanceNote: dto.ProvenanceNote!.Trim());
    }

    private static void ValidateLegalFields(
        TranslationSeedCorpusJsonRow dto,
        TranslationSeedTeacherProvenance provenance,
        string sourceName,
        int lineNumber)
    {
        if (!string.Equals(dto.SourceCategory, SelfAuthoredCategory, StringComparison.Ordinal))
            throw new InvalidOperationException(
                $"{sourceName}: line {lineNumber} source_category must be '{SelfAuthoredCategory}'.");

        if (!string.Equals(dto.License, InternalLicense, StringComparison.Ordinal))
            throw new InvalidOperationException(
                $"{sourceName}: line {lineNumber} license must be '{InternalLicense}'.");

        if (!string.Equals(provenance.SourceType, SelfAuthoredCategory, StringComparison.Ordinal) ||
            !string.Equals(provenance.ModelProvider, NoTeacherValue, StringComparison.Ordinal) ||
            !string.Equals(provenance.ModelName, NoTeacherValue, StringComparison.Ordinal) ||
            provenance.ExternalApiUsed ||
            provenance.ClosedProprietaryTeacherUsed)
            throw new InvalidOperationException(
                $"{sourceName}: line {lineNumber} must preserve self-authored/internal provenance with no external or closed proprietary teacher.");

        if (dto.SemanticQualityClaimed)
            throw new InvalidOperationException(
                $"{sourceName}: line {lineNumber} semantic_quality_claimed must be false for seed ingestion.");
    }

    private static void ValidateCorpus(IReadOnlyList<TranslationSeedCorpusRow> rows, string sourceName)
    {
        if (rows.Count == 0)
            throw new InvalidOperationException($"{sourceName}: seed corpus has no rows.");

        var duplicateId = rows
            .GroupBy(row => row.SeedId, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicateId is not null)
            throw new InvalidOperationException($"{sourceName}: duplicate seed_id '{duplicateId.Key}'.");

        var trainInputs = rows
            .Where(row => row.Split == TranslationSeedCorpusSplit.Train)
            .Select(row => NormalizeComparable(row.InputText))
            .ToHashSet(StringComparer.Ordinal);
        var leaked = rows
            .Where(row => row.Split == TranslationSeedCorpusSplit.HeldOut)
            .FirstOrDefault(row => trainInputs.Contains(NormalizeComparable(row.InputText)));
        if (leaked is not null)
            throw new InvalidOperationException(
                $"{sourceName}: held-out row '{leaked.SeedId}' duplicates a train input.");
    }

    private static string RequiredReason(TranslationSeedCorpusRow row)
        => string.IsNullOrWhiteSpace(row.RejectedReason)
            ? row.WrongMarker ?? "seed_translation_quality_miss"
            : row.RejectedReason!;

    private static bool TryParseSplit(string value, out TranslationSeedCorpusSplit split)
    {
        switch ((value ?? string.Empty).Trim())
        {
            case "train":
                split = TranslationSeedCorpusSplit.Train;
                return true;
            case "held_out":
                split = TranslationSeedCorpusSplit.HeldOut;
                return true;
            default:
                split = default;
                return false;
        }
    }

    private static string NormalizeLanguageCode(string value)
        => (value ?? string.Empty).Trim().ToLowerInvariant();

    private static string NormalizeComparable(string value)
        => Regex.Replace((value ?? string.Empty).Normalize(), @"\s+", " ").Trim();

    private static void Require(string? value, string fieldName, string sourceName, int lineNumber)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw RequiredFieldError(fieldName, sourceName, lineNumber);
    }

    private static void RequireProperty(
        JsonElement element,
        string propertyName,
        string sourceName,
        int lineNumber,
        string? fieldName = null)
    {
        if (!element.TryGetProperty(propertyName, out _))
            throw RequiredFieldError(fieldName ?? propertyName, sourceName, lineNumber);
    }

    private static InvalidOperationException RequiredFieldError(
        string fieldName,
        string sourceName,
        int lineNumber)
        => new($"{sourceName}: line {lineNumber} missing required field '{fieldName}'.");

    private sealed record TranslationSeedCorpusJsonRow(
        [property: JsonPropertyName("seed_id")] string? SeedId,
        [property: JsonPropertyName("source_language")] string? SourceLanguage,
        [property: JsonPropertyName("target_language")] string? TargetLanguage,
        [property: JsonPropertyName("input_text")] string? InputText,
        [property: JsonPropertyName("chosen_translation")] string? ChosenTranslation,
        [property: JsonPropertyName("rejected_translation")] string? RejectedTranslation,
        [property: JsonPropertyName("rejected_reason")] string? RejectedReason,
        [property: JsonPropertyName("wrong_marker")] string? WrongMarker,
        [property: JsonPropertyName("split")] string? Split,
        [property: JsonPropertyName("source_category")] string? SourceCategory,
        [property: JsonPropertyName("license")] string? License,
        [property: JsonPropertyName("teacher_provenance")] TranslationSeedTeacherProvenanceJson? TeacherProvenance,
        [property: JsonPropertyName("semantic_quality_claimed")] bool SemanticQualityClaimed,
        [property: JsonPropertyName("expected_label")] string? ExpectedLabel);

    private sealed record TranslationSeedTeacherProvenanceJson(
        [property: JsonPropertyName("source_type")] string? SourceType,
        [property: JsonPropertyName("author")] string? Author,
        [property: JsonPropertyName("model_provider")] string? ModelProvider,
        [property: JsonPropertyName("model_name")] string? ModelName,
        [property: JsonPropertyName("external_api_used")] bool ExternalApiUsed,
        [property: JsonPropertyName("closed_proprietary_teacher_used")] bool ClosedProprietaryTeacherUsed,
        [property: JsonPropertyName("provenance_note")] string? ProvenanceNote);
}
