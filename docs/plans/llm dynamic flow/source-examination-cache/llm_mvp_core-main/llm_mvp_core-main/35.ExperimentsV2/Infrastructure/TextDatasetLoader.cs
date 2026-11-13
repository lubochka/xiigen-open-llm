namespace ExperimentsV2;

// TextDatasetLoader загружает текстовые файлы для E08.
// Разбивает на предложения по точкам/восклицательным/вопросительным знакам.
public static class TextDatasetLoader
{
    // Загрузить первые maxSentences предложений из текстового файла.
    public static OperationResult<TextDatasetContents> Load(
        string textFilePath, int maxSentences)
    {
        if (!File.Exists(textFilePath))
            return OperationResult<TextDatasetContents>.Fail(
                $"Файл не найден: {textFilePath}");

        string raw = File.ReadAllText(textFilePath, System.Text.Encoding.UTF8);

        var sentences = raw
            .Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s =>
            {
                int words = s.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
                return words >= 5 && words <= 50;
            })
            .Take(maxSentences)
            .ToList()
            .AsReadOnly();

        if (sentences.Count == 0)
            return OperationResult<TextDatasetContents>.Fail(
                "Нет подходящих предложений в файле");

        return OperationResult<TextDatasetContents>.Ok(new TextDatasetContents
        {
            FilePath     = textFilePath,
            Sentences    = sentences,
            CombinedText = string.Join(" ", sentences)
        });
    }
}

public sealed record TextDatasetContents
{
    public required string FilePath      { get; init; }
    public required IReadOnlyList<string> Sentences { get; init; }
    public required string CombinedText  { get; init; }
    public int SentenceCount => Sentences.Count;
}
