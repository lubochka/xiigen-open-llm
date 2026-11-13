// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

public sealed class ExperimentRunner
{
    private readonly string _projectRoot;
    private readonly string _outputRoot;
    private readonly ExperimentsV2Dispatcher _d = new();

    public ExperimentRunner(string projectRoot, string outputRoot)
    {
        _projectRoot = projectRoot;
        _outputRoot  = outputRoot;
    }

    public OperationResult<ExperimentV2Result> Run(string id, int tier)
    {
        Console.Write($"[{id}] Tier {tier} загрузка... ");
        var loadOp = DatasetLoader.Load(tier, _projectRoot);
        if (!loadOp.IsSuccess)
        {
            Console.WriteLine($"ERROR: {loadOp.ErrorMessage}");
            return OperationResult<ExperimentV2Result>.Fail(loadOp.ErrorMessage);
        }
        var ds = loadOp.Value!;
        Console.WriteLine($"{ds.FileCount} файлов, {ds.CombinedText.Length:N0} симв.");

        Console.Write($"[{id}] Запуск... ");
        var expOp = Dispatch(id, ds, tier);
        if (!expOp.IsSuccess)
        {
            Console.WriteLine($"ERROR: {expOp.ErrorMessage}");
            return expOp;
        }
        var r = expOp.Value!;

        var writeOp = ResultWriter.Write(r, _outputRoot);
        string fileLine = writeOp.IsSuccess
            ? $"-> {writeOp.Value}"
            : $"WRITE FAILED: {writeOp.ErrorMessage}";

        string arrow  = r.LowerIsBetter ? "down" : "up";
        string status = r.Passed        ? "PASSED" :
                        r.ShouldScaleUp ? "SCALE_UP" : "FAILED";
        Console.WriteLine(
            $"OK {r.PrimaryMetricName} ({arrow}) baseline={r.BaselinePrimary:F2} " +
            $"enhanced={r.EnhancedPrimary:F2} delta={r.ImprovementPct:+0.0;-0.0}% | {status} | {r.NextAction}");
        Console.WriteLine($"  {fileLine}");

        return OperationResult<ExperimentV2Result>.Ok(r);
    }

    // Run tier ladder: starts at startTier, auto-escalates on SCALE_UP or "Tier 2+" errors.
    // Stops on PASSED (success) or FAILED (real failure, not a data-size issue).
    public OperationResult<ExperimentV2Result> RunTierLadder(string id, int startTier = 1)
    {
        OperationResult<ExperimentV2Result> last =
            OperationResult<ExperimentV2Result>.Fail("не запускался");

        for (int tier = startTier; tier <= 3; tier++)
        {
            Console.WriteLine();
            Console.WriteLine($"=== {id} -- Tier {tier} ===================================");
            var op = Run(id, tier);

            // "Tier 2+" in the error message means the experiment is simply not applicable
            // at this tier — skip rather than abort the ladder.
            if (!op.IsSuccess)
            {
                if (op.ErrorMessage.Contains("Tier 2") || op.ErrorMessage.Contains("Tier 2+"))
                {
                    Console.WriteLine($"  -> пропуск Tier {tier}: {op.ErrorMessage}");
                    last = op;
                    continue;
                }
                Console.WriteLine($"Лестница остановлена: {op.ErrorMessage}");
                return op;
            }

            last = op;
            var r = op.Value!;

            if (r.ShouldScaleUp)
            {
                Console.WriteLine($"  -> SCALE_UP: данных мало на Tier {tier}, переходим на Tier {tier + 1}");
                continue;
            }

            if (r.Passed)
            {
                if (tier == 3) Console.WriteLine($"ALL PASSED: {id} прошёл все три тира!");
                return op;
            }

            Console.WriteLine($"  -> FAILED на Tier {tier}: улучшение {r.ImprovementPct:+0.0;-0.0}%. Остановка.");
            return op;
        }

        return last;
    }

    // Run all experiments, each auto-escalating through tiers until a definitive result.
    public void RunAll()
    {
        string[] ids = ["E01", "E02", "E03", "E04", "E05", "E06", "E07", "E08", "E09", "E10", "E11", "E12", "E13", "E14", "E15", "E16", "E17", "E18", "E19", "E20", "E21", "E22", "E23", "E24", "E25", "E26", "E27", "E28", "E29", "E30", "E31", "E32", "E33", "E34", "E34V2", "E35", "E36", "E37", "E38", "E39", "E40", "E41", "E42", "E43", "E44", "E45", "E28V2", "E29V2", "E30V2", "E31V2", "E40V2", "E42V2", "E44V2", "E45V2", "E46", "E47", "E48", "E49", "E50", "E51A", "E51B", "E52"];
        var results  = new List<(string Id, OperationResult<ExperimentV2Result> R)>();

        foreach (var id in ids)
        {
            Console.WriteLine();
            // E04 needs Tier 2+ (too few docs at Tier 1).
            // E09/E10/E11 need Tier 2 for stable timing / enough AST nodes.
            int startTier = id is "E04" or "E09" or "E10" or "E11" or "E13" or "E14" or "E15" or "E16" or "E17"
                               or "E22" or "E23" or "E24" or "E28" or "E29" or "E30" or "E31"
                               or "E43" or "E44" or "E45"
                               or "E28V2" or "E29V2" or "E31V2" or "E44V2" or "E45V2"
                               or "E48" ? 2 : 1;
            // E18/E19/E20/E25/E26/E27 use synthetic data — Tier 1 is fine
            results.Add((id, RunTierLadder(id, startTier)));
        }

        Console.WriteLine();
        Console.WriteLine(new string('=', 78));
        Console.WriteLine("SUMMARY -- auto tier (each experiment found its minimum viable tier)");
        Console.WriteLine(new string('-', 78));
        Console.WriteLine($"{"ID",-8} {"Metric",-22} {"Tier",4} {"Baseline",10} {"Enhanced",10} {"Delta%",7}  Status");
        Console.WriteLine(new string('-', 78));

        foreach (var (id, op) in results)
        {
            if (!op.IsSuccess)
            {
                Console.WriteLine($"{id,-8} {"--",-22} {"--",4} {"--",10} {"--",10} {"--",7}  ERROR: {op.ErrorMessage}");
                continue;
            }
            var v      = op.Value!;
            string st  = v.Passed ? "PASSED" : v.ShouldScaleUp ? "SCALE_UP" : "FAILED";
            Console.WriteLine($"{id,-8} {v.PrimaryMetricName,-22} {v.Tier,4} {v.BaselinePrimary,10:F3} " +
                              $"{v.EnhancedPrimary,10:F3} {v.ImprovementPct,7:+0.0;-0.0}  {st}");
        }

        Console.WriteLine(new string('-', 78));
        int passed  = results.Count(r => r.R.IsSuccess && r.R.Value!.Passed);
        int scaleUp = results.Count(r => r.R.IsSuccess && r.R.Value!.ShouldScaleUp);
        int failed  = results.Count(r => r.R.IsSuccess && !r.R.Value!.Passed && !r.R.Value!.ShouldScaleUp);
        Console.WriteLine($"PASSED: {passed}  SCALE_UP: {scaleUp}  FAILED: {failed}  из {ids.Length}");
        Console.WriteLine($"Результаты: {_outputRoot}");
        Console.WriteLine($"Сводка: {Path.Combine(_outputRoot, "progress", "progress.md")}");
        Console.WriteLine(new string('=', 78));
    }

    private OperationResult<ExperimentV2Result> Dispatch(
        string id, DatasetContents ds, int tier)
    {
        return id.ToUpperInvariant() switch
        {
            "E01" => _d.RunE01(InputAdapters.ForE01(ds), tier: tier),
            "E02" => _d.RunE02(InputAdapters.ForE02(ds), tier: tier),
            "E03" => DispatchE03(ds, tier),
            "E04" => DispatchE04(ds, tier),
            "E05" => DispatchE05(ds, tier),
            "E06" => DispatchE06(ds, tier),
            "E07" => DispatchE07(ds, tier),
            "E08" => _d.RunE08(Path.Combine(_projectRoot, ".e08-cache"), tier),
            "E09" => DispatchE09(ds, tier),
            "E10" => DispatchE10(ds, tier),
            "E11" => _d.RunE11(ds.CombinedText, tier),
            "E12" => DispatchE12(ds, tier),
            "E13" => _d.RunE13(ds.Files, k: 5, tier: tier),
            "E14" => _d.RunE14(ds.Files, k: 3, tier: tier),
            "E15" => _d.RunE15(ds.Files, tier: tier),
            "E16" => _d.RunE16(ds.Files, tier: tier),
            "E17" => _d.RunE17(ds.Files, tier: tier),
            "E18" => _d.RunE18(tier: tier),
            "E19" => _d.RunE19(tier: tier),
            "E20" => _d.RunE20(tier: tier),
            "E21" => _d.RunE21(tier: tier),
            "E22" => _d.RunE22(ds.Files, tier: tier),
            "E23" => _d.RunE23(ds.Files, tier: tier),
            "E24" => _d.RunE24(ds.Files, tier: tier),
            "E25" => _d.RunE25(tier: tier),
            "E26" => _d.RunE26(tier: tier),
            "E27" => _d.RunE27(tier: tier),
            "E32" => _d.RunE32(tier: tier),
            "E33" => _d.RunE33(tier: tier),
            "E34" => _d.RunE34(tier: tier),
            "E35" => _d.RunE35(ds.Files, tier: tier),
            "E36" => _d.RunE36(tier: tier),
            "E37" => _d.RunE37(tier: tier),
            "E38" => _d.RunE38(tier: tier),
            "E39" => _d.RunE39(tier: tier),
            "E28" => _d.RunE28(ds.Files, tier: tier),
            "E29" => _d.RunE29(ds.Files, tier: tier),
            "E30" => _d.RunE30(ds.Files, tier: tier),
            "E31" => _d.RunE31(ds.Files, tier: tier),
            "E40" => _d.RunE40(tier: tier),
            "E41" => _d.RunE41(tier: tier),
            "E42" => _d.RunE42(tier: tier),
            "E43" => _d.RunE43(ds.Files, tier: tier),
            "E44" => _d.RunE44(ds.Files, tier: tier),
            "E45"   => _d.RunE45(ds.Files, tier: tier),
            "E34V2" => _d.RunE34V2(tier: tier),
            "E28V2" => _d.RunE28V2(ds.Files, tier: tier),
            "E29V2" => _d.RunE29V2(ds.Files, tier: tier),
            "E30V2" => _d.RunE30V2(tier: tier),
            "E31V2" => _d.RunE31V2(ds.Files, tier: tier),
            "E40V2" => _d.RunE40V2(tier: tier),
            "E42V2" => _d.RunE42V2(tier: tier),
            "E44V2" => _d.RunE44V2(ds.Files, tier: tier),
            "E45V2" => _d.RunE45V2(ds.Files, tier: tier),
            "E46"   => _d.RunE46(tier: tier),
            "E47"   => _d.RunE47(tier: tier),
            "E48"   => _d.RunE48(ds.Files, tier: tier),
            "E49"   => _d.RunE49(tier: tier),
            "E50"   => _d.RunE50(tier: tier),
            "E51A"  => _d.RunE51A(tier: tier),
            "E51B"  => _d.RunE51B(tier: tier),
            "E52"   => _d.RunE52(tier: tier),
            _       => OperationResult<ExperimentV2Result>.Fail($"Unknown experiment: {id}")
        };
    }

    private OperationResult<ExperimentV2Result> DispatchE03(DatasetContents ds, int tier)
    {
        var adapt = InputAdapters.ForE03(ds);
        if (!adapt.IsSuccess)
            return OperationResult<ExperimentV2Result>.Fail(adapt.ErrorMessage);
        var r = _d.RunE03(adapt.Value!, tier: tier);
        return r.IsSuccess
            ? OperationResult<ExperimentV2Result>.Ok(r.Value!.summary)
            : OperationResult<ExperimentV2Result>.Fail(r.ErrorMessage);
    }

    private OperationResult<ExperimentV2Result> DispatchE04(DatasetContents ds, int tier)
    {
        var adapt = InputAdapters.ForE04(ds);
        if (!adapt.IsSuccess)
        {
            Console.WriteLine($"  NOTE: {adapt.ErrorMessage}");
            return OperationResult<ExperimentV2Result>.Fail(adapt.ErrorMessage);
        }
        return _d.RunE04(adapt.Value!.Documents, adapt.Value!.Queries, tier: tier);
    }

    private OperationResult<ExperimentV2Result> DispatchE05(DatasetContents ds, int tier)
    {
        var (combined, allSources) = InputAdapters.ForE05(ds);
        return _d.RunE05(combined, allSources, tier: tier);
    }

    private OperationResult<ExperimentV2Result> DispatchE06(DatasetContents ds, int tier)
    {
        var adapt = InputAdapters.ForE06(ds);
        return adapt.IsSuccess
            ? _d.RunE06(adapt.Value!, tier: tier)
            : OperationResult<ExperimentV2Result>.Fail(adapt.ErrorMessage);
    }

    private OperationResult<ExperimentV2Result> DispatchE07(DatasetContents ds, int tier)
    {
        var e07 = new E07StructuralContextToken();
        return e07.Run(ds.CombinedText, ds.Files, tier);
    }

    private OperationResult<ExperimentV2Result> DispatchE09(DatasetContents ds, int tier)
        => _d.RunE09(ds.CombinedText, ds.Files, k: 5, tier: tier);

    private OperationResult<ExperimentV2Result> DispatchE10(DatasetContents ds, int tier)
        => _d.RunE10(ds.Files, tier);

    private OperationResult<ExperimentV2Result> DispatchE12(DatasetContents ds, int tier)
    {
        var adapt = InputAdapters.ForE03(ds);
        return adapt.IsSuccess
            ? _d.RunE12(adapt.Value!, tier)
            : OperationResult<ExperimentV2Result>.Fail(adapt.ErrorMessage);
    }

    // Запустить E08 с текстовым датасетом.
    public OperationResult<ExperimentV2Result> RunE08Text(
        string textFilePath,
        string scriptPath,
        int maxSentences = 500,
        int tier = 1)
    {
        Console.Write($"[E08] Загрузка текста ({maxSentences} предложений)... ");
        var loadResult = TextDatasetLoader.Load(textFilePath, maxSentences);
        if (!loadResult.IsSuccess)
        {
            Console.WriteLine($"ERROR: {loadResult.ErrorMessage}");
            return OperationResult<ExperimentV2Result>.Fail(loadResult.ErrorMessage);
        }
        var dataset = loadResult.Value!;
        Console.WriteLine($"{dataset.SentenceCount} предложений загружено.");

        Console.Write($"[E08] Запуск (POS-теги через SpaCy)... ");
        var experiment = new E08TextSyntax(scriptPath);
        var result     = experiment.Run(dataset, tier);

        if (!result.IsSuccess)
        {
            Console.WriteLine($"ERROR: {result.ErrorMessage}");
            return result;
        }

        var writeOp  = ResultWriter.Write(result.Value!, _outputRoot);
        string fileLine = writeOp.IsSuccess ? $"-> {writeOp.Value}" : "WRITE FAILED";

        var v      = result.Value!;
        string st  = v.Passed ? "PASSED" : (v.ShouldScaleUp ? "SCALE_UP" : "FAILED");
        Console.WriteLine(
            $"perplexity down baseline={v.BaselinePrimary:F2} " +
            $"enhanced={v.EnhancedPrimary:F2} delta={v.ImprovementPct:+0.0;-0.0}% | {st}");
        Console.WriteLine($"  {fileLine}");

        return result;
    }
}
