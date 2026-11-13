namespace ExperimentsV2;

// Result of attributed routing: each relevant dataset gets only its own slice of the prompt.
public sealed record AttributedRoutingResult
{
    public required IReadOnlyDictionary<string, string> DatasetToSubprompt { get; init; }

    public int ActiveDatasets => DatasetToSubprompt.Count;

    public int TotalSubpromptLength => DatasetToSubprompt.Values.Sum(v => v.Length);

    public string? ForDataset(string datasetName)
        => DatasetToSubprompt.TryGetValue(datasetName, out var v) ? v : null;
}
