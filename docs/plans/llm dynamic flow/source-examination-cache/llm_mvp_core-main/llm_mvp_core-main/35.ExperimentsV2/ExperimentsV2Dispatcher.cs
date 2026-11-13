namespace ExperimentsV2;

public sealed class ExperimentsV2Dispatcher
{
    private readonly E01AstTokenizer      _e01 = new();
    private readonly E02FrequencyRanking  _e02 = new();
    private readonly E04Bm25Context       _e04 = new();
    private readonly E05SyntaxFilter      _e05 = new();
    private readonly E06PageRank          _e06 = new();

    public OperationResult<ExperimentV2Result> RunE01(
        string csharpSource,
        int    bpeVocabSize = 512,
        int    tier         = 1)
        => _e01.Run(csharpSource, bpeVocabSize, tier);

    public OperationResult<ExperimentV2Result> RunE02(
        IReadOnlyList<string> sources,
        int tier = 1)
        => _e02.Run(sources, tier);

    public OperationResult<(ExperimentV2Result summary, List<NgramResultEntry> perNResults)>
        RunE03(IReadOnlyList<int> tokens, int maxN = 3, int tier = 1)
        => new E03NgramModel(maxN).Run(tokens, tier);

    public OperationResult<ExperimentV2Result> RunE04(
        IReadOnlyList<(string Id, string Source)> documents,
        IReadOnlyList<(string Query, string ExpectedId)> queries,
        int tier = 1)
        => _e04.Run(documents, queries, tier);

    public OperationResult<ExperimentV2Result> RunE05(
        string csharpSource,
        IReadOnlyList<string>? allSources = null,
        int    tier = 1)
        => _e05.Run(csharpSource, allSources, tier);

    public OperationResult<ExperimentV2Result> RunE06(
        IReadOnlyList<(string Caller, string Callee)> callPairs,
        int tier = 1)
        => _e06.Run(callPairs, tier);

    public OperationResult<ExperimentV2Result> RunE09(
        string source,
        IReadOnlyList<(string Path, string Text)> files,
        int k    = 5,
        int tier = 1)
        => new E09StructuralKnn(k).Run(source, files, tier);

    public OperationResult<ExperimentV2Result> RunE10(
        IReadOnlyList<(string Path, string Text)> files,
        int tier = 1)
        => new E10RegexPrep().Run(files, tier);

    public OperationResult<ExperimentV2Result> RunE11(string source, int tier = 1)
        => new E11StepTimer().Run(source, tier);

    public OperationResult<ExperimentV2Result> RunE12(
        IReadOnlyList<int> tokens,
        int tier = 1)
        => new E12MatrixOpt().Run(tokens, tier);

    public OperationResult<ExperimentV2Result> RunE08(string cacheDir, int tier = 1)
        => new E08NonCodeSyntax(cacheDir).Run(tier);

    public OperationResult<ExperimentV2Result> RunE13(
        IReadOnlyList<(string Path, string Text)> files,
        int k = 5, int tier = 1)
        => new E13MethodLevelKnn(k).Run(files, tier);

    public OperationResult<ExperimentV2Result> RunE14(
        IReadOnlyList<(string Path, string Text)> files,
        int k = 3, int tier = 1)
        => new E14ContextActionKnn(k).Run(files, tier);

    public OperationResult<ExperimentV2Result> RunE15(
        IReadOnlyList<(string Path, string Text)> files,
        int tier = 1)
        => new E15SemanticTokenizer().Run(files, tier);

    public OperationResult<ExperimentV2Result> RunE16(
        IReadOnlyList<(string Path, string Text)> files,
        int tier = 1)
        => new E16SynonymCollapse().Run(files, tier);

    public OperationResult<ExperimentV2Result> RunE17(
        IReadOnlyList<(string Path, string Text)> files,
        int tier = 1)
        => new E17LogicalPredicates().Run(files, tier);

    public OperationResult<ExperimentV2Result> RunE21(int tier = 1)
        => new E21PromptSplitter().Run(tier);

    public OperationResult<ExperimentV2Result> RunE22(
        IReadOnlyList<(string Path, string Text)> files,
        int tier = 1)
        => new E22DatasetRouter().Run(files, tier);

    public OperationResult<ExperimentV2Result> RunE23(
        IReadOnlyList<(string Path, string Text)> files,
        int tier = 1)
        => new E23Beautifier().Run(files, tier);

    public OperationResult<ExperimentV2Result> RunE24(
        IReadOnlyList<(string Path, string Text)> files,
        int tier = 1)
        => new E24AttributedRouter().Run(files, tier);

    public OperationResult<ExperimentV2Result> RunE18(int tier = 1)
        => new E18DomainClassifier().Run(tier);

    public OperationResult<ExperimentV2Result> RunE19(int tier = 1)
        => new E19ComplexityRouter().Run(tier);

    public OperationResult<ExperimentV2Result> RunE20(int tier = 1)
        => new E20StackDetector().Run(tier);

    public OperationResult<ExperimentV2Result> RunE25(int tier = 1)
        => new E25PatternClassifier().Run(tier);

    public OperationResult<ExperimentV2Result> RunE26(int tier = 1)
        => new E26ContextualDictionary().Run(tier);

    public OperationResult<ExperimentV2Result> RunE27(int tier = 1)
        => new E27PatternReorderer().Run(tier);

    public OperationResult<ExperimentV2Result> RunE32(int tier = 1)
        => new E32EpisodeDecomposer().Run(tier);

    public OperationResult<ExperimentV2Result> RunE33(int tier = 1)
        => new E33StopSignalDetector().Run(tier);

    public OperationResult<ExperimentV2Result> RunE34(int tier = 1)
        => new E34EpisodeQueryMatcher().Run(tier);

    public OperationResult<ExperimentV2Result> RunE35(
        IReadOnlyList<(string Path, string Text)> files, int tier = 1)
        => new E35DependencyDetector().Run(tier);

    public OperationResult<ExperimentV2Result> RunE36(int tier = 1)
        => new E36FeedbackCorrector().Run(tier);

    public OperationResult<ExperimentV2Result> RunE37(int tier = 1)
        => new E37MergerSynthesizer().Run(tier);

    public OperationResult<ExperimentV2Result> RunE38(int tier = 1)
        => new E38SkillRetriever().Run(tier);

    public OperationResult<ExperimentV2Result> RunE39(int tier = 1)
        => new E39QualityOrchestrator().Run(tier);

    public OperationResult<ExperimentV2Result> RunE28(
        IReadOnlyList<(string Path, string Text)> files, int tier = 1)
        => new E28ProjectAdaptor().Run(files, tier);

    public OperationResult<ExperimentV2Result> RunE29(
        IReadOnlyList<(string Path, string Text)> files, int tier = 1)
        => new E29DpoPairValue().Run(files, tier);

    public OperationResult<ExperimentV2Result> RunE30(
        IReadOnlyList<(string Path, string Text)> files, int tier = 1)
        => new E30PredicateCache().Run(files, tier);

    public OperationResult<ExperimentV2Result> RunE31(
        IReadOnlyList<(string Path, string Text)> files, int tier = 1)
        => new E31ContinuousLoop().Run(files, tier);

    public OperationResult<ExperimentV2Result> RunE40(int tier = 1)
        => new E40TrustScorer().Run(tier);

    public OperationResult<ExperimentV2Result> RunE41(int tier = 1)
        => new E41IdleDetector().Run(tier);

    public OperationResult<ExperimentV2Result> RunE42(int tier = 1)
        => new E42DiskManager().Run(tier);

    public OperationResult<ExperimentV2Result> RunE43(
        IReadOnlyList<(string Path, string Text)> files, int tier = 1)
        => new E43ReplayBuffer().Run(files, tier);

    public OperationResult<ExperimentV2Result> RunE44(
        IReadOnlyList<(string Path, string Text)> files, int tier = 1)
        => new E44DatasetWeighter().Run(files, tier);

    public OperationResult<ExperimentV2Result> RunE45(
        IReadOnlyList<(string Path, string Text)> files, int tier = 1)
        => new E45CurriculumOrganizer().Run(files, tier);

    public OperationResult<ExperimentV2Result> RunE34V2(int tier = 1)
        => new E34HybridMatcher().Run(tier);

    public OperationResult<ExperimentV2Result> RunE28V2(
        IReadOnlyList<(string Path, string Text)> files, int tier = 1)
        => new E28ProjectAdaptorV2().Run(files, tier);

    public OperationResult<ExperimentV2Result> RunE29V2(
        IReadOnlyList<(string Path, string Text)> files, int tier = 1)
        => new E29DpoPairValueV2().Run(files, tier);

    public OperationResult<ExperimentV2Result> RunE30V2(int tier = 1)
        => new E30PredicateCacheV2().Run(tier);

    public OperationResult<ExperimentV2Result> RunE31V2(
        IReadOnlyList<(string Path, string Text)> files, int tier = 1)
        => new E31ContinuousLoopV2().Run(files, tier);

    public OperationResult<ExperimentV2Result> RunE40V2(int tier = 1)
        => new E40TrustScorerV2().Run(tier);

    public OperationResult<ExperimentV2Result> RunE42V2(int tier = 1)
        => new E42DiskManagerV2().Run(tier);

    public OperationResult<ExperimentV2Result> RunE44V2(
        IReadOnlyList<(string Path, string Text)> files, int tier = 1)
        => new E44DatasetWeighterV2().Run(files, tier);

    public OperationResult<ExperimentV2Result> RunE45V2(
        IReadOnlyList<(string Path, string Text)> files, int tier = 1)
        => new E45CurriculumOrganizerV2().Run(files, tier);

    public OperationResult<ExperimentV2Result> RunE46(int tier = 1)
        => new E46ConfidenceGate().Run(tier);

    public OperationResult<ExperimentV2Result> RunE47(int tier = 1)
        => new E47ModelHubSearcher().Run(tier);

    public OperationResult<ExperimentV2Result> RunE48(
        IReadOnlyList<(string Path, string Text)> files, int tier = 1)
        => new E48DomainBootstrapper().Run(files, tier);

    public OperationResult<ExperimentV2Result> RunE49(int tier = 1)
        => new E49OrchestratorExpander().Run(tier);

    public OperationResult<ExperimentV2Result> RunE50(int tier = 1)
        => new E50DomainGraphClosure().Run(tier);
}
