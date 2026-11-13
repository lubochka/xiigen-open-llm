namespace SearchEngine;

// ─── Result envelope ────────────────────────────────────────────────────────

public sealed class OperationResult<T>
{
    public bool   IsSuccess { get; }
    public T?     Value     { get; }
    public string Error     { get; }

    private OperationResult(bool ok, T? value, string error)
        => (IsSuccess, Value, Error) = (ok, value, error);

    public static OperationResult<T> Ok(T value)        => new(true,  value, "");
    public static OperationResult<T> Fail(string error) => new(false, default, error);
}

// ─── Domain records ──────────────────────────────────────────────────────────

public sealed record AnalyzedToken(string Term, int Position);

public sealed record Posting(int DocId, int Frequency, IReadOnlyList<int> Positions);

public sealed record DocumentInfo(int DocId, string ExternalId, int TermCount);

public sealed record SearchResult(string ExternalId, float Score);

// ─── Fabric interfaces ───────────────────────────────────────────────────────

public interface IAnalyzer
{
    IReadOnlyList<AnalyzedToken> Analyze(string text);
}

public interface IIndexStorage
{
    OperationResult<bool>    Save(string path, IndexSnapshot snapshot);
    OperationResult<IndexSnapshot> Load(string path);
}

public interface ISearchEngine
{
    OperationResult<bool>               Add(string externalId, string text);
    OperationResult<IReadOnlyList<SearchResult>> Search(string query, int topK = 10);
}

// ─── Snapshot (serialisable index state) ────────────────────────────────────

public sealed class IndexSnapshot
{
    public List<DocumentInfo>                     Documents   { get; set; } = [];
    public Dictionary<string, List<PostingData>>  Postings    { get; set; } = [];
    public float                                  AvgDocLen   { get; set; }
}

public sealed class PostingData
{
    public int        DocId     { get; set; }
    public int        Frequency { get; set; }
    public List<int>  Positions { get; set; } = [];
}
