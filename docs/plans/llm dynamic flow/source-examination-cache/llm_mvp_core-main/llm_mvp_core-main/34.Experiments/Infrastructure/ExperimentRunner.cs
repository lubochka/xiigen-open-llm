using Tokenizer;

namespace Experiments;

// ─── ExperimentRunner: close the measurement loop ─────────────────────────────
//
// Tokenisation strategy:
//   E01, E04, E05 — BPE subwords (vocabSize 500): these experiments cluster or
//                   correlate tokens; ASCII chars carry no semantic structure.
//   E02, E03, E06 — char-level ASCII 0–127: n-grams and AST comparison are fine
//                   at the character level.
//
// Each Run() call writes RESULT-{EXX}-scale{N}.json and .md to outputDir.
// RunScaleLadder() promotes through scales until accuracy improvement < 5%.

public sealed class ExperimentRunner
{
    private readonly DatasetLoader         _loader;
    private readonly ResultWriter          _writer;
    private readonly ExperimentsDispatcher _dispatcher;
    private readonly string                _projectRoot;
    // Accumulates every result from this runner instance so progress.json stays complete.
    private readonly List<ExperimentResult> _allResults = new();

    // projectRoot — root directory of the llm_mvp_core repository.
    // outputDir   — where RESULT-*.json / RESULT-*.md files are written.
    public ExperimentRunner(string projectRoot, string outputDir)
    {
        _projectRoot = projectRoot;
        _loader      = new DatasetLoader();
        _writer      = new ResultWriter(outputDir);
        _dispatcher  = new ExperimentsDispatcher();
    }

    // Run one experiment at the specified scale.
    // Returns the patched ExperimentResult (Scale field overwritten from 0 to scale).
    public OperationResult<ExperimentResult> Run(string experimentId, int scale)
    {
        // E07 / E08 use Alice text — skip the C# code loader entirely.
        if (experimentId is "E07" or "E08")
            return RunNaturalLanguage(experimentId, scale);

        // Step 1: load dataset
        var loadResult = _loader.Load(scale, _projectRoot);
        if (!loadResult.IsSuccess)
            return OperationResult<ExperimentResult>.Fail(
                $"DatasetLoader: {loadResult.ErrorMessage}");

        DatasetContents dataset = loadResult.Value!;

        // Step 2: scale guards
        if (experimentId == "E04" && dataset.ApproxTokens < 50_000)
        {
            string reason = $"E04 requires Scale 3+ (~50K tokens for reliable PMI matrix). " +
                            $"Current: {dataset.ApproxTokens:N0} tokens. Run with scale: 3.";
            _writer.WriteSkipped(experimentId, scale, dataset, reason);
            return OperationResult<ExperimentResult>.Fail(reason);
        }
        if (experimentId == "E09" && scale < 1)
        {
            string reason = "E09 requires Scale 1+ (too few AST nodes at Scale 0). Run with scale: 1.";
            _writer.WriteSkipped(experimentId, scale, dataset, reason);
            return OperationResult<ExperimentResult>.Fail(reason);
        }
        if (experimentId == "E10" && dataset.ApproxTokens < 10_000)
        {
            string reason = $"E10 requires Scale 3+ (sparse PPMI at {dataset.ApproxTokens:N0} tokens). Run with scale: 3.";
            _writer.WriteSkipped(experimentId, scale, dataset, reason);
            return OperationResult<ExperimentResult>.Fail(reason);
        }

        // Step 3: run the experiment
        OperationResult<ExperimentResult> runResult = experimentId switch
        {
            "E01" => _dispatcher.RunE01(BpeTokenize(dataset.CombinedText, vocabSize: 500)),
            "E02" => _dispatcher.RunE02(CharLevelTokenize(dataset.CombinedText)),
            "E03" => _dispatcher.RunE03(CharLevelTokenize(dataset.CombinedText)),
            "E04" => _dispatcher.RunE04(BpeTokenize(dataset.CombinedText, vocabSize: 500)),
            "E05" => _dispatcher.RunE05(BpeTokenize(dataset.CombinedText, vocabSize: 500)),
            "E06" => _dispatcher.RunE06(CharLevelTokenize(dataset.CombinedText), dataset.CombinedText),
            "E09" => _dispatcher.RunE09(dataset.CombinedText, scale),
            "E10" => _dispatcher.RunE10(BpeTokenize(dataset.CombinedText, vocabSize: 500), scale),
            "E11" => _dispatcher.RunE11(dataset.FileTexts, scale),
            _     => OperationResult<ExperimentResult>.Fail(
                         $"Unknown experiment: {experimentId}"),
        };

        if (!runResult.IsSuccess) return runResult;

        // Step 4: fix Scale (experiments hard-code Scale=0)
        var result = runResult.Value! with { Scale = scale };

        // Step 5: write results
        var writeResult = _writer.Write(result, dataset);
        if (!writeResult.IsSuccess)
            Console.Error.WriteLine($"[ResultWriter] {writeResult.ErrorMessage}");

        // Step 6: keep progress summary current after every run
        _allResults.RemoveAll(r => r.ExperimentId == result.ExperimentId && r.Scale == result.Scale);
        _allResults.Add(result);
        _writer.UpdateProgress(_allResults);

        return OperationResult<ExperimentResult>.Ok(result);
    }

    // Run experiment across all scales 0..maxScale, stopping when accuracy < 5%.
    // Writes progress.md / progress.json after the full ladder.
    public IReadOnlyList<ExperimentResult> RunScaleLadder(
        string experimentId,
        int    maxScale = 5)
    {
        var results = new List<ExperimentResult>();

        for (int scale = 0; scale <= maxScale; scale++)
        {
            Console.WriteLine(
                $"\n{experimentId} Scale {scale}: {DatasetLoader.TierNames[scale]}");

            var runResult = Run(experimentId, scale);

            if (!runResult.IsSuccess)
            {
                Console.WriteLine($"  Error: {runResult.ErrorMessage}");
                break;
            }

            ExperimentResult r = runResult.Value!;
            results.Add(r);

            Console.WriteLine($"  Baseline accuracy:  {r.Baseline.AccuracyFraction * 100:F1}%");
            Console.WriteLine($"  Enhanced accuracy:  {r.Enhanced.AccuracyFraction * 100:F1}%");
            Console.WriteLine($"  Delta:              {r.AccuracyDeltaPct:+0.0;-0.0}%");
            Console.WriteLine($"  Result:             {r.NextAction}");

            if (!r.Passed)
            {
                Console.WriteLine($"  Improvement < 5% — stopping at Scale {scale}.");
                break;
            }
        }

        _writer.UpdateProgress(results);
        return results;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    // E07 / E08: load Alice text, run natural-language experiment.
    private OperationResult<ExperimentResult> RunNaturalLanguage(string experimentId, int scale)
    {
        if (scale < 0 || scale > 3)
            return OperationResult<ExperimentResult>.Fail(
                $"Alice scale must be 0–3 for {experimentId}");

        Console.WriteLine($"  [{experimentId}] loading Alice text: {AliceTextLoader.TierNames[scale]}");

        var loadOp = AliceTextLoader.Load(scale, _projectRoot);
        if (!loadOp.IsSuccess)
            return OperationResult<ExperimentResult>.Fail(loadOp.ErrorMessage);

        var data = loadOp.Value!;

        var runResult = experimentId switch
        {
            "E07" => _dispatcher.RunE07(data, scale),
            "E08" => _dispatcher.RunE08(data, scale),
            _     => OperationResult<ExperimentResult>.Fail($"Unknown NL experiment: {experimentId}")
        };

        if (!runResult.IsSuccess) return runResult;
        var result = runResult.Value! with { Scale = scale };

        // Build a thin DatasetContents for the writer
        var fakeDataset = new DatasetContents
        {
            Scale        = scale,
            TierName     = AliceTextLoader.TierNames[scale],
            Files        = [],
            FileTexts    = new Dictionary<string, string>(),
            CombinedText = string.Join(" ", data.Words),
            TotalChars   = data.Words.Sum(w => w.Length + 1),
            ApproxTokens = data.Tokens.Count,
        };
        var writeResult = _writer.Write(result, fakeDataset);
        if (!writeResult.IsSuccess)
            Console.Error.WriteLine($"[ResultWriter] {writeResult.ErrorMessage}");

        _allResults.RemoveAll(r => r.ExperimentId == result.ExperimentId && r.Scale == result.Scale);
        _allResults.Add(result);
        _writer.UpdateProgress(_allResults);

        return OperationResult<ExperimentResult>.Ok(result);
    }

    // BPE subword tokenisation. Trains and encodes in one pass — suitable for
    // experiments that require semantic clusters (E01, E04, E05).
    private static IReadOnlyList<int> BpeTokenize(string text, int vocabSize)
    {
        var bpe = new BpeTokenizer();
        bpe.Train(text, Math.Max(256, vocabSize));
        return bpe.Encode(text);
    }

    // Convert text to char codes 0–127. Non-ASCII chars map to 127 (unknown).
    private static IReadOnlyList<int> CharLevelTokenize(string text)
    {
        var tokens = new List<int>(text.Length);
        foreach (char c in text)
            tokens.Add(c < 128 ? (int)c : 127);
        return tokens;
    }
}
