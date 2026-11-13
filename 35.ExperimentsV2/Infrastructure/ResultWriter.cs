// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ExperimentsV2;

public static class ResultWriter
{
    private static readonly JsonSerializerOptions Opts = new()
    {
        WriteIndented          = true,
        PropertyNamingPolicy   = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static OperationResult<string> Write(ExperimentV2Result r, string outputRoot)
    {
        try
        {
            string dir  = Path.Combine(outputRoot, FolderName(r.ExperimentId), "results");
            Directory.CreateDirectory(dir);

            string date     = DateTime.UtcNow.ToString("yyyy-MM-dd");
            int    seq      = NextSeq(dir, date);
            string baseName = $"run-{date}-{seq:D3}";
            string jsonPath = Path.Combine(dir, baseName + ".json");
            string mdPath   = Path.Combine(dir, baseName + ".md");

            File.WriteAllText(jsonPath, ToJson(r),              Encoding.UTF8);
            File.WriteAllText(mdPath,  ToMarkdown(r, baseName), Encoding.UTF8);
            WriteProgress(r, outputRoot);

            return OperationResult<string>.Ok(jsonPath);
        }
        catch (Exception ex)
        {
            return OperationResult<string>.Fail($"ResultWriter: {ex.Message}");
        }
    }

    private static string ToJson(ExperimentV2Result r)
    {
        var obj = new
        {
            experiment_id       = r.ExperimentId,
            tier                = r.Tier,
            run_at              = DateTime.UtcNow.ToString("O"),
            primary_metric      = r.PrimaryMetricName,
            lower_is_better     = r.LowerIsBetter,
            baseline_value      = Math.Round(r.BaselinePrimary, 4),
            enhanced_value      = Math.Round(r.EnhancedPrimary, 4),
            improvement_pct     = Math.Round(r.ImprovementPct,  2),
            passed              = r.Passed,
            should_scale_up     = r.ShouldScaleUp,
            next_action         = r.NextAction,
            scaling_signal      = Math.Round(r.ScalingSignal,   4),
            scaling_signal_note = r.ScalingSignalNote,
            baseline_time_ms    = Math.Round(r.BaselineTimeMs,  1),
            enhanced_time_ms    = Math.Round(r.EnhancedTimeMs,  1),
            notes               = r.Notes
        };
        return JsonSerializer.Serialize(obj, Opts);
    }

    private static string ToMarkdown(ExperimentV2Result r, string runName)
    {
        string arrow = r.LowerIsBetter ? "down" : "up";
        string delta = r.ImprovementPct >= 5.0
            ? $"**+{r.ImprovementPct:F1}% OK**"
            : r.ImprovementPct >= 0 ? $"+{r.ImprovementPct:F1}% low"
                                    : $"{r.ImprovementPct:F1}% worse";

        return new StringBuilder()
            .AppendLine($"# {r.ExperimentId} - Tier {r.Tier} - {runName}")
            .AppendLine($"Run at: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC")
            .AppendLine()
            .AppendLine($"## Result: {(r.Passed ? "PASSED" : r.ShouldScaleUp ? "SCALE_UP" : "FAILED")}")
            .AppendLine()
            .AppendLine($"| Metric | Baseline | Enhanced | Delta | >= 5%? |")
            .AppendLine($"|--------|----------|----------|-------|--------|")
            .AppendLine($"| {r.PrimaryMetricName} ({arrow}) | {r.BaselinePrimary:F4} | {r.EnhancedPrimary:F4} | {delta} | {(r.Passed ? "YES" : "NO")} |")
            .AppendLine($"| Baseline time | {r.BaselineTimeMs:F0} ms | - | - | - |")
            .AppendLine($"| Enhanced time | - | {r.EnhancedTimeMs:F0} ms | - | - |")
            .AppendLine()
            .AppendLine($"## Scaling signal")
            .AppendLine($"Value: {r.ScalingSignal:F4} -- {r.ScalingSignalNote}")
            .AppendLine()
            .AppendLine($"## Notes")
            .AppendLine(string.IsNullOrWhiteSpace(r.Notes) ? "none" : r.Notes)
            .AppendLine()
            .AppendLine($"## Next step")
            .AppendLine(r.Passed
                ? $"Run Tier {r.Tier + 1}: runner.Run(\"{r.ExperimentId.Split('-')[0]}\", tier: {r.Tier + 1})"
                : "Improvement below 5%. Tune or skip to next experiment.")
            .ToString();
    }

    private static void WriteProgress(ExperimentV2Result r, string outputRoot)
    {
        string dir  = Path.Combine(outputRoot, "progress");
        Directory.CreateDirectory(dir);
        string jsonPath = Path.Combine(dir, "progress.json");

        Dictionary<string, object> progress;
        try
        {
            progress = File.Exists(jsonPath)
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(jsonPath)) ?? new()
                : new();
        }
        catch { progress = new(); }

        progress[r.ExperimentId] = new
        {
            experiment_id   = r.ExperimentId,
            tier            = r.Tier,
            passed          = r.Passed,
            should_scale_up = r.ShouldScaleUp,
            improvement_pct = Math.Round(r.ImprovementPct, 2),
            next_action     = r.NextAction,
            scaling_signal  = Math.Round(r.ScalingSignal, 4),
            last_run        = DateTime.UtcNow.ToString("O")
        };

        File.WriteAllText(jsonPath, JsonSerializer.Serialize(progress, Opts), Encoding.UTF8);

        var sb = new StringBuilder();
        sb.AppendLine("# Experiment progress");
        sb.AppendLine($"Updated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC");
        sb.AppendLine();
        sb.AppendLine("| Experiment | Passed | Tier | Delta% | Signal | Next |");
        sb.AppendLine("|------------|--------|------|--------|--------|------|");

        foreach (var (id, _) in progress.OrderBy(kv => kv.Key))
        {
            var entry = progress[id];
            string entryJson = JsonSerializer.Serialize(entry);
            using var doc = JsonDocument.Parse(entryJson);
            var el = doc.RootElement;
            bool   p = el.TryGetProperty("passed",          out var pv) && pv.GetBoolean();
            int    t = el.TryGetProperty("tier",             out var tv) ? tv.GetInt32()  : 0;
            double i = el.TryGetProperty("improvement_pct",  out var iv) ? iv.GetDouble() : 0;
            double s = el.TryGetProperty("scaling_signal",   out var sv) ? sv.GetDouble() : 0;
            string n = el.TryGetProperty("next_action",      out var nv) ? nv.GetString()!: "";
            sb.AppendLine($"| {id} | {(p ? "YES" : "NO")} | {t} | {i:+0.0;-0.0}% | {s:F3} | {n} |");
        }

        File.WriteAllText(Path.Combine(dir, "progress.md"), sb.ToString(), Encoding.UTF8);
    }

    private static string FolderName(string id) => id.ToUpperInvariant() switch
    {
        "E01-AST-TOKENIZER"     => "E01-AST-TOKENIZER",
        "E02-FREQUENCY-RANKING" => "E02-FREQUENCY-RANKING",
        "E03-NGRAM-MODEL"       => "E03-NGRAM-MODEL",
        "E04-BM25-CONTEXT"      => "E04-BM25-CONTEXT",
        "E05-SYNTAX-FILTER"     => "E05-SYNTAX-FILTER",
        "E06-PAGERANK-RANKING"  => "E06-PAGERANK-RANKING",
        "E07-STRUCTURAL-CONTEXT-TOKEN" => "E07-STRUCTURAL-CONTEXT-TOKEN",
        "E08-TEXT-SYNTAX"              => "E08-TEXT-SYNTAX",
        "E08-NON-CODE-SYNTAX"          => "E08-NON-CODE-SYNTAX",
        "E09-STRUCTURAL-KNN"           => "E09-STRUCTURAL-KNN",
        "E10-REGEX-PREP"               => "E10-REGEX-PREP",
        "E11-STEP-TIMER"               => "E11-STEP-TIMER",
        "E12-MATRIX-OPT"               => "E12-MATRIX-OPT",
        "E13-METHOD-KNN"               => "E13-METHOD-KNN",
        "E14-CONTEXT-ACTION-KNN"       => "E14-CONTEXT-ACTION-KNN",
        "E15-SEMANTIC-TOKENIZER"       => "E15-SEMANTIC-TOKENIZER",
        "E16-SYNONYM-COLLAPSE"         => "E16-SYNONYM-COLLAPSE",
        "E17-LOGICAL-PREDICATES"       => "E17-LOGICAL-PREDICATES",
        "E18-DOMAIN-CLASSIFIER"        => "E18-DOMAIN-CLASSIFIER",
        "E19-COMPLEXITY-ROUTER"        => "E19-COMPLEXITY-ROUTER",
        "E20-STACK-DETECTOR"           => "E20-STACK-DETECTOR",
        "E21-PROMPT-SPLITTER"          => "E21-PROMPT-SPLITTER",
        "E22-DATASET-ROUTER"           => "E22-DATASET-ROUTER",
        "E23-BEAUTIFIER"               => "E23-BEAUTIFIER",
        "E24-ATTRIBUTED-ROUTER"        => "E24-ATTRIBUTED-ROUTER",
        "E25-PATTERN-CLASSIFIER"       => "E25-PATTERN-CLASSIFIER",
        "E26-CONTEXTUAL-DICTIONARY"    => "E26-CONTEXTUAL-DICTIONARY",
        "E27-PATTERN-REORDERER"        => "E27-PATTERN-REORDERER",
        "E28-PROJECT-ADAPTOR"          => "E28-PROJECT-ADAPTOR",
        "E29-DPO-PAIR-VALUE"           => "E29-DPO-PAIR-VALUE",
        "E30-PREDICATE-CACHE"          => "E30-PREDICATE-CACHE",
        "E31-CONTINUOUS-LOOP"          => "E31-CONTINUOUS-LOOP",
        "E32-EPISODE-DECOMPOSER"       => "E32-EPISODE-DECOMPOSER",
        "E33-STOP-SIGNAL-DETECTOR"     => "E33-STOP-SIGNAL-DETECTOR",
        "E34-EPISODE-QUERY-MATCHER"    => "E34-EPISODE-QUERY-MATCHER",
        "E35-DEPENDENCY-DETECTOR"      => "E35-DEPENDENCY-DETECTOR",
        "E36-FEEDBACK-CORRECTOR"       => "E36-FEEDBACK-CORRECTOR",
        "E37-MERGER-SYNTHESIZER"       => "E37-MERGER-SYNTHESIZER",
        "E38-SKILL-RETRIEVER"          => "E38-SKILL-RETRIEVER",
        "E39-QUALITY-ORCHESTRATOR"     => "E39-QUALITY-ORCHESTRATOR",
        "E40-TRUST-SCORER"             => "E40-TRUST-SCORER",
        "E41-IDLE-DETECTOR"            => "E41-IDLE-DETECTOR",
        "E42-DISK-MANAGER"             => "E42-DISK-MANAGER",
        "E43-REPLAY-BUFFER"            => "E43-REPLAY-BUFFER",
        "E44-DATASET-WEIGHTER"         => "E44-DATASET-WEIGHTER",
        "E45-CURRICULUM-ORGANIZER"     => "E45-CURRICULUM-ORGANIZER",
        "E34V2-HYBRID-MATCHER"         => "E34V2-HYBRID-MATCHER",
        "E38V2-SKILL-RETRIEVER"        => "E38V2-SKILL-RETRIEVER",
        "E28V2-PROJECT-ADAPTOR"        => "E28V2-PROJECT-ADAPTOR",
        "E29V2-DPO-PAIR-VALUE"         => "E29V2-DPO-PAIR-VALUE",
        "E30V2-PREDICATE-CACHE"        => "E30V2-PREDICATE-CACHE",
        "E31V2-CONTINUOUS-LOOP"        => "E31V2-CONTINUOUS-LOOP",
        "E40V2-TRUST-SCORER"           => "E40V2-TRUST-SCORER",
        "E42V2-DISK-MANAGER"           => "E42V2-DISK-MANAGER",
        "E44V2-DATASET-WEIGHTER"       => "E44V2-DATASET-WEIGHTER",
        "E45V2-CURRICULUM-ORGANIZER"   => "E45V2-CURRICULUM-ORGANIZER",
        "E46-CONFIDENCE-GATE"          => "E46-CONFIDENCE-GATE",
        "E47-MODEL-HUB-SEARCHER"       => "E47-MODEL-HUB-SEARCHER",
        "E48-DOMAIN-BOOTSTRAPPER"      => "E48-DOMAIN-BOOTSTRAPPER",
        "E49-ORCHESTRATOR-EXPANDER"    => "E49-ORCHESTRATOR-EXPANDER",
        "E50-DOMAIN-GRAPH-CLOSURE"     => "E50-DOMAIN-GRAPH-CLOSURE",
        "E51A-CODE-CONTEXT-PIPELINE"   => "E51A-CODE-CONTEXT-PIPELINE",
        "E51B-DYNAMIC-FLOW-MINI-E2E"   => "E51B-DYNAMIC-FLOW-MINI-E2E",
        "E52-PROJECTS-21-23-CYCLE-COMPARISON" => "E52-PROJECTS-21-23-CYCLE-COMPARISON",
        _                              => id
    };

    private static int NextSeq(string dir, string date)
    {
        int max = Directory.GetFiles(dir, $"run-{date}-*.json")
            .Select(f => Path.GetFileNameWithoutExtension(f).Split('-').LastOrDefault())
            .Select(s => int.TryParse(s, out int v) ? v : 0)
            .DefaultIfEmpty(0).Max();
        return max + 1;
    }
}
