namespace Experiments;

using System.Text.Json;

// ─── ResultWriter: write JSON + MD after every experiment run ─────────────────
//
// Two files per run, one progress roll-up:
//   RESULT-{EXX}-scale{N}.json   — machine-readable, matches experiment schema
//   RESULT-{EXX}-scale{N}.md     — human-readable comparison table
//   progress.md / progress.json  — updated by UpdateProgress() after a ladder run

public sealed class ResultWriter
{
    private readonly string _outputDir;

    // outputDir is created automatically if it does not exist.
    public ResultWriter(string outputDir)
    {
        _outputDir = outputDir;
        Directory.CreateDirectory(outputDir);
    }

    // Write JSON and MD files for one experiment result.
    // Returns the two file paths on success.
    public OperationResult<(string JsonPath, string MdPath)> Write(
        ExperimentResult result,
        DatasetContents  dataset)
    {
        string baseName = $"RESULT-{result.ExperimentId}-scale{result.Scale}";
        string jsonPath = Path.Combine(_outputDir, baseName + ".json");
        string mdPath   = Path.Combine(_outputDir, baseName + ".md");

        try
        {
            File.WriteAllText(jsonPath, BuildJson(result, dataset));
            File.WriteAllText(mdPath,   BuildMarkdown(result, dataset));
            return OperationResult<(string, string)>.Ok((jsonPath, mdPath));
        }
        catch (Exception ex)
        {
            return OperationResult<(string, string)>.Fail(
                $"Write failed: {ex.Message}");
        }
    }

    // Write a SKIPPED sentinel when an experiment cannot run at this scale.
    // Produces the same file names as Write() so progress dashboards can find it.
    public void WriteSkipped(string experimentId, int scale, DatasetContents dataset, string reason)
    {
        string baseName = $"RESULT-{experimentId}-scale{scale}";
        string jsonPath = Path.Combine(_outputDir, baseName + ".json");
        string mdPath   = Path.Combine(_outputDir, baseName + ".md");

        var obj = new
        {
            experiment_id = experimentId,
            scale         = scale,
            status        = "SKIPPED",
            reason        = reason,
            next_action   = "RUN_AT_SCALE_2",
            run_at        = DateTime.UtcNow.ToString("o"),
            dataset = new
            {
                files_count   = dataset.Files.Count,
                total_chars   = dataset.TotalChars,
                approx_tokens = dataset.ApproxTokens,
            }
        };

        try
        {
            File.WriteAllText(jsonPath,
                JsonSerializer.Serialize(obj,
                    new JsonSerializerOptions { WriteIndented = true }));
            File.WriteAllText(mdPath,
                $"# {experimentId} — Scale {scale}: SKIPPED\n\n" +
                $"**Reason:** {reason}\n\n" +
                "**Next action:** RUN_AT_SCALE_2\n");
        }
        catch { /* best-effort; silent on write errors */ }
    }

    // Merge new results into progress.json/md, preserving prior runs across sessions.
    public void UpdateProgress(IReadOnlyList<ExperimentResult> newResults)
    {
        string jsonPath = Path.Combine(_outputDir, "progress.json");

        // Load existing entries (survive corrupt/absent file).
        var existing = new List<(string Id, int Scale, double Delta, bool Passed, string Next)>();
        if (File.Exists(jsonPath))
        {
            try
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(jsonPath));
                if (doc.RootElement.TryGetProperty("experiments", out var arr))
                {
                    foreach (var e in arr.EnumerateArray())
                    {
                        existing.Add((
                            e.GetProperty("id").GetString()!,
                            e.GetProperty("scale").GetInt32(),
                            e.GetProperty("accuracy_delta_pct").GetDouble(),
                            e.GetProperty("passed").GetBoolean(),
                            e.GetProperty("next_action").GetString()!));
                    }
                }
            }
            catch { /* start fresh if file is damaged */ }
        }

        // Drop stale entries that are being replaced.
        var newKeys = newResults.Select(r => (r.ExperimentId, r.Scale)).ToHashSet();
        existing.RemoveAll(e => newKeys.Contains((e.Id, e.Scale)));

        // Merge old + new, ordered.
        var merged = existing
            .Select(e => new { id = e.Id, scale = e.Scale,
                               accuracy_delta_pct = e.Delta,
                               passed = e.Passed, next_action = e.Next })
            .Concat(newResults.Select(r => new
            {
                id                 = r.ExperimentId,
                scale              = r.Scale,
                accuracy_delta_pct = Math.Round(r.AccuracyDeltaPct, 2),
                passed             = r.Passed,
                next_action        = r.NextAction,
            }))
            .OrderBy(e => e.id).ThenBy(e => e.scale)
            .ToList();

        File.WriteAllText(jsonPath, JsonSerializer.Serialize(
            new { updated_at = DateTime.UtcNow.ToString("o"), experiments = merged },
            new JsonSerializerOptions { WriteIndented = true }));

        // Rebuild progress.md from the full merged data (avoid reconstructing fake records).
        var mdSb = new System.Text.StringBuilder();
        mdSb.AppendLine("# Experiment Progress");
        mdSb.AppendLine($"**Updated:** {DateTime.UtcNow:dd MMM yyyy HH:mm} UTC");
        mdSb.AppendLine();
        mdSb.AppendLine("| Exp. | Scale | Accuracy Δ | Result |");
        mdSb.AppendLine("|---|---|---|---|");
        foreach (var e in merged)
            mdSb.AppendLine($"| {e.id} | {e.scale} | {FormatDelta(e.accuracy_delta_pct)} | {(e.passed ? "✅ PASS" : "❌ FAIL")} |");
        File.WriteAllText(Path.Combine(_outputDir, "progress.md"), mdSb.ToString());
    }

    // ── JSON ──────────────────────────────────────────────────────────────────

    private static string BuildJson(ExperimentResult r, DatasetContents d)
    {
        var obj = new
        {
            experiment_id = r.ExperimentId,
            scale         = r.Scale,
            tier_name     = d.TierName,
            run_at        = DateTime.UtcNow.ToString("o"),
            dataset = new
            {
                files_count   = d.Files.Count,
                total_chars   = d.TotalChars,
                approx_tokens = d.ApproxTokens,
            },
            baseline = new
            {
                accuracy_pct = Math.Round(r.Baseline.AccuracyFraction * 100, 2),
                time_ms      = Math.Round(r.Baseline.TimeMs,  1),
                memory_mb    = Math.Round(r.Baseline.MemoryMb, 2),
                tokens       = r.Baseline.DatasetTokenCount,
            },
            enhanced = new
            {
                accuracy_pct = Math.Round(r.Enhanced.AccuracyFraction * 100, 2),
                time_ms      = Math.Round(r.Enhanced.TimeMs,  1),
                memory_mb    = Math.Round(r.Enhanced.MemoryMb, 2),
                tokens       = r.Enhanced.DatasetTokenCount,
            },
            delta = new
            {
                accuracy_pct_delta = Math.Round(r.AccuracyDeltaPct, 2),
                time_pct_saved     = TimeSaved(r),
                memory_pct_saved   = MemorySaved(r),
            },
            passed      = r.Passed,
            next_action = r.NextAction,
            notes       = r.Notes,
        };

        return JsonSerializer.Serialize(obj,
            new JsonSerializerOptions { WriteIndented = true });
    }

    // ── Markdown ──────────────────────────────────────────────────────────────

    private static string BuildMarkdown(ExperimentResult r, DatasetContents d)
    {
        string verdict  = r.Passed ? "✅ PASSED — PROMOTE_TO_NEXT_SCALE"
                                   : "❌ FAILED — ABANDON or TUNE";
        double baseAcc  = Math.Round(r.Baseline.AccuracyFraction * 100, 1);
        double enhAcc   = Math.Round(r.Enhanced.AccuracyFraction * 100, 1);
        double accDelta = Math.Round(r.AccuracyDeltaPct, 1);
        double timeSave = TimeSaved(r);
        double memSave  = MemorySaved(r);

        string accMark  = accDelta >= 5 ? "✅" : accDelta >= 0 ? "⚠️" : "❌";
        string timeMark = timeSave >= 5 ? "✅" : timeSave >= 0 ? "⚠️" : "❌";
        string memMark  = memSave  >= 5 ? "✅" : memSave  >= 0 ? "⚠️" : "❌";

        string nextStep = r.Passed
            ? $"Promote {r.ExperimentId} to Scale {r.Scale + 1}"
            : $"Improvement {accDelta:+0.0;-0.0}% is below +5% threshold. Abandon or tune.";

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"# {r.ExperimentId} — Scale {r.Scale}: {d.TierName}");
        sb.AppendLine($"**Run:** {DateTime.UtcNow:dd MMM yyyy HH:mm} UTC");
        sb.AppendLine();
        sb.AppendLine($"## Result: {verdict}");
        sb.AppendLine();
        sb.AppendLine("| Metric | Baseline | Enhanced | Δ |");
        sb.AppendLine("|---|---|---|---|");
        sb.AppendLine($"| Accuracy | {baseAcc}% | {enhAcc}% | {FormatDelta(accDelta)} {accMark} |");
        sb.AppendLine($"| Time (ms) | {Math.Round(r.Baseline.TimeMs, 0)} ms | {Math.Round(r.Enhanced.TimeMs, 0)} ms | {FormatSave(timeSave)} {timeMark} |");
        sb.AppendLine($"| Memory (MB) | {Math.Round(r.Baseline.MemoryMb, 1)} MB | {Math.Round(r.Enhanced.MemoryMb, 1)} MB | {FormatSave(memSave)} {memMark} |");
        sb.AppendLine();
        sb.AppendLine("## Dataset");
        sb.AppendLine($"- Tier: {d.TierName}");
        sb.AppendLine($"- Files: {d.Files.Count}");
        sb.AppendLine($"- Characters: {d.TotalChars:N0}");
        sb.AppendLine($"- Approx tokens: {d.ApproxTokens:N0}");
        sb.AppendLine();
        sb.AppendLine("## Notes");
        sb.AppendLine(r.Notes);
        sb.AppendLine();
        sb.AppendLine("## Next step");
        sb.AppendLine(nextStep);
        return sb.ToString();
    }

    // ── Progress builders ─────────────────────────────────────────────────────

    private static string BuildProgressMarkdown(IReadOnlyList<ExperimentResult> results)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("# Experiment Progress");
        sb.AppendLine($"**Updated:** {DateTime.UtcNow:dd MMM yyyy HH:mm} UTC");
        sb.AppendLine();
        sb.AppendLine("| Exp. | Scale | Accuracy Δ | Time Δ | Memory Δ | Result |");
        sb.AppendLine("|---|---|---|---|---|---|");

        foreach (var r in results.OrderBy(r => r.ExperimentId).ThenBy(r => r.Scale))
        {
            sb.AppendLine(
                $"| {r.ExperimentId} | {r.Scale} | " +
                $"{FormatDelta(r.AccuracyDeltaPct)} | " +
                $"{FormatSave(TimeSaved(r))} | " +
                $"{FormatSave(MemorySaved(r))} | " +
                $"{(r.Passed ? "✅ PASS" : "❌ FAIL")} |");
        }

        return sb.ToString();
    }

    private static string BuildProgressJson(IReadOnlyList<ExperimentResult> results)
    {
        var summary = results
            .OrderBy(r => r.ExperimentId).ThenBy(r => r.Scale)
            .Select(r => new
            {
                id                 = r.ExperimentId,
                scale              = r.Scale,
                accuracy_delta_pct = Math.Round(r.AccuracyDeltaPct, 2),
                passed             = r.Passed,
                next_action        = r.NextAction,
            })
            .ToList();

        return JsonSerializer.Serialize(
            new { updated_at = DateTime.UtcNow.ToString("o"), experiments = summary },
            new JsonSerializerOptions { WriteIndented = true });
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    // Positive = enhanced is faster. Negative = enhanced is slower.
    private static double TimeSaved(ExperimentResult r)
    {
        if (r.Baseline.TimeMs <= 0) return 0;
        return Math.Round((r.Baseline.TimeMs - r.Enhanced.TimeMs)
                          / r.Baseline.TimeMs * 100, 1);
    }

    // Positive = enhanced uses less memory.
    private static double MemorySaved(ExperimentResult r)
    {
        if (r.Baseline.MemoryMb <= 0) return 0;
        return Math.Round((r.Baseline.MemoryMb - r.Enhanced.MemoryMb)
                          / r.Baseline.MemoryMb * 100, 1);
    }

    private static string FormatDelta(double delta)
        => delta >= 0 ? $"+{delta:0.0}%" : $"{delta:0.0}%";

    private static string FormatSave(double saved)
        => saved >= 0 ? $"saved {saved:0.0}%" : $"cost {-saved:0.0}%";
}
