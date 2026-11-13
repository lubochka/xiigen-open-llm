namespace Experiments;

// ─── Single entry point for all six micro-experiments ─────────────────────────

// Callers use this dispatcher rather than instantiating experiment services
// directly.  Each Run* method returns an OperationResult<ExperimentResult>
// so that test and application code handle errors uniformly.
public sealed class ExperimentsDispatcher
{
    private readonly E01SemanticFamilies       _e01 = new();
    private readonly E02ShannonCompression     _e02 = new();
    private readonly E03SparseWindow           _e03 = new();
    private readonly E04PmiInit                _e04 = new();
    private readonly E05HierarchicalPrediction _e05 = new();
    private readonly E06AstRoslyn              _e06 = new();

    // E01: semantic K-Means families vs flat BPE.
    public OperationResult<ExperimentResult> RunE01(IReadOnlyList<int> bpeTokens)
        => _e01.Run(bpeTokens);

    // E02: Shannon entropy of BPE tokens vs cluster IDs.
    public OperationResult<ExperimentResult> RunE02(IReadOnlyList<int> bpeTokens)
        => _e02.Run(bpeTokens);

    // E03: trigram vs bigram Markov.
    public OperationResult<ExperimentResult> RunE03(IReadOnlyList<int> bpeTokens)
        => _e03.Run(bpeTokens);

    // E04: PMI-based predictor vs count-based Markov.
    public OperationResult<ExperimentResult> RunE04(IReadOnlyList<int> bpeTokens)
        => _e04.Run(bpeTokens);

    // E05: two-level hierarchical Markov vs flat bigram.
    public OperationResult<ExperimentResult> RunE05(IReadOnlyList<int> bpeTokens)
        => _e05.Run(bpeTokens);

    // E06: Roslyn AST node-type Markov vs flat BPE Markov.
    public OperationResult<ExperimentResult> RunE06(
        IReadOnlyList<int> bpeTokens,
        string             csharpSourceCode)
        => _e06.Run(bpeTokens, csharpSourceCode);

    // E07: bigram on natural-language words vs unigram baseline.
    public OperationResult<ExperimentResult> RunE07(AliceDataset data, int scale = 0)
        => new E07NaturalLanguageNgram().Run(data, scale);

    // E08: bigram + POS filter vs plain bigram on natural-language words.
    public OperationResult<ExperimentResult> RunE08(AliceDataset data, int scale = 0)
        => new E08NaturalLanguageGrammar().Run(data, scale);

    // E09: 5D structural vector + HNSW K-NN vs bigram on AST tokens.
    public OperationResult<ExperimentResult> RunE09(string csharpSource, int scale = 1)
        => new E09ContextVectorKnn().Run(csharpSource, scale);

    // E10: truncated SVD on PPMI co-occurrence matrix vs bigram.
    public OperationResult<ExperimentResult> RunE10(IReadOnlyList<int> bpeTokens, int scale = 3)
        => new E10SvdPmiEmbedding().Run(bpeTokens, scale);

    // E11: 8 compiled regex patterns vs Roslyn parse — time benchmark.
    public OperationResult<ExperimentResult> RunE11(
        IReadOnlyDictionary<string, string> fileTexts,
        int scale = 1)
        => new E11RegexDataPrep().Run(fileTexts, scale);
}
