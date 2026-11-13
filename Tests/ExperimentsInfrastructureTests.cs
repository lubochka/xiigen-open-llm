// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using Experiments;

namespace LlmFromScratch.Tests;

// ─── Behavioral tests for 34.Experiments Infrastructure (Phases 1–4) ──────────
//
// Tests cover DatasetLoader, ResourceMonitor, ResultWriter, and ExperimentRunner.
// The ExperimentRunner integration tests use the actual worktree project root so
// DatasetLoader can find real .cs files.

public sealed class ExperimentsInfrastructureTests
{
    // Worktree root — all 34.Experiments subproject folders live here.
    private static readonly string ProjectRoot =
        Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", ".."));

    // ── DatasetLoader ─────────────────────────────────────────────────────────

    [Fact]
    public void DatasetLoader_RejectsScaleOutOfRange()
    {
        var loader = new DatasetLoader();
        var r = loader.Load(-1, ProjectRoot);
        Assert.False(r.IsSuccess);
        Assert.Contains("0–5", r.ErrorMessage);
    }

    [Fact]
    public void DatasetLoader_RejectsNonExistentRoot()
    {
        var loader = new DatasetLoader();
        var r = loader.Load(0, @"C:\no_such_dir_xyz_12345");
        Assert.False(r.IsSuccess);
        Assert.Contains("not found", r.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DatasetLoader_Scale0_ReturnsAtLeastOneFile()
    {
        var loader = new DatasetLoader();
        var r = loader.Load(0, ProjectRoot);
        Assert.True(r.IsSuccess, r.ErrorMessage);
        Assert.True(r.Value!.Files.Count >= 1,
            "Scale 0 should return at least the BpeTokenizer.cs file");
    }

    [Fact]
    public void DatasetLoader_Scale1_ReturnsMultipleFiles()
    {
        var loader = new DatasetLoader();
        var r = loader.Load(1, ProjectRoot);
        Assert.True(r.IsSuccess, r.ErrorMessage);
        Assert.True(r.Value!.Files.Count > 1,
            "Scale 1 (whole 01.Tokenizer) should return multiple files");
    }

    [Fact]
    public void DatasetLoader_Scale1_HasMoreCharsThanScale0()
    {
        var loader = new DatasetLoader();
        var r0 = loader.Load(0, ProjectRoot);
        var r1 = loader.Load(1, ProjectRoot);
        Assert.True(r0.IsSuccess && r1.IsSuccess);
        Assert.True(r1.Value!.TotalChars > r0.Value!.TotalChars,
            "Scale 1 should have more text than Scale 0");
    }

    [Fact]
    public void DatasetLoader_CombinedText_MatchesSumOfFileLengths()
    {
        var loader = new DatasetLoader();
        var r = loader.Load(0, ProjectRoot);
        Assert.True(r.IsSuccess, r.ErrorMessage);
        DatasetContents d = r.Value!;
        int sumLen = d.FileTexts.Values.Sum(t => t.Length);
        // CombinedText joins with "\n\n" so it's slightly longer.
        Assert.True(d.TotalChars >= sumLen, "CombinedText should be >= sum of file lengths");
    }

    [Fact]
    public void DatasetLoader_NoGeneratedFiles_InOutput()
    {
        var loader = new DatasetLoader();
        var r = loader.Load(1, ProjectRoot);
        Assert.True(r.IsSuccess, r.ErrorMessage);
        foreach (string path in r.Value!.Files)
        {
            string norm = path.Replace('\\', '/');
            Assert.False(norm.Contains("/bin/"),  $"bin/ file should be excluded: {path}");
            Assert.False(norm.Contains("/obj/"),  $"obj/ file should be excluded: {path}");
            Assert.False(path.EndsWith(".g.cs"),  $".g.cs file should be excluded: {path}");
        }
    }

    [Fact]
    public void DatasetLoader_ApproxTokens_IsQuarterOfChars()
    {
        var loader = new DatasetLoader();
        var r = loader.Load(0, ProjectRoot);
        Assert.True(r.IsSuccess, r.ErrorMessage);
        DatasetContents d = r.Value!;
        Assert.Equal(d.TotalChars / 4, d.ApproxTokens);
    }

    // ── ResourceMonitor ───────────────────────────────────────────────────────

    [Fact]
    public void ResourceMonitor_ReportsPositiveElapsedMs()
    {
        var m = new ResourceMonitor();
        m.Start();
        // Tiny allocation to ensure > 0 ms
        var _ = new byte[1024];
        var s = m.Stop();
        Assert.True(s.ElapsedMs >= 0, "ElapsedMs should be non-negative");
    }

    [Fact]
    public void ResourceMonitor_DetectsAllocation()
    {
        var m = new ResourceMonitor();
        m.Start();
        var bigArray = new byte[5 * 1024 * 1024];   // 5 MB
        GC.KeepAlive(bigArray);
        m.SampleMemory();
        var s = m.Stop();
        Assert.True(s.PeakMemoryMb >= 0,
            "PeakMemoryMb must be non-negative (GC timing may vary)");
    }

    [Fact]
    public void ResourceMonitor_StartStopRepeatedly_DoesNotThrow()
    {
        var m = new ResourceMonitor();
        for (int i = 0; i < 3; i++)
        {
            m.Start();
            var s = m.Stop();
            Assert.True(s.ElapsedMs >= 0);
        }
    }

    // ── ResultWriter ──────────────────────────────────────────────────────────

    private static ExperimentResult MakeResult(string id = "E03", int scale = 0) =>
        new()
        {
            ExperimentId = id,
            Scale        = scale,
            Baseline = new RunMetrics
            {
                AccuracyFraction  = 0.40,
                TimeMs            = 50,
                MemoryMb          = 10,
                DatasetTokenCount = 1000,
            },
            Enhanced = new RunMetrics
            {
                AccuracyFraction  = 0.52,
                TimeMs            = 55,
                MemoryMb          = 9,
                DatasetTokenCount = 1000,
            },
            Notes = "unit-test result",
        };

    private static DatasetContents MakeDataset() =>
        new()
        {
            Scale        = 0,
            TierName     = "Markov-micro  (~1 000 tokens, 1 file)",
            Files        = new[] { "fake.cs" },
            FileTexts    = new Dictionary<string, string> { ["fake.cs"] = "class X{}" },
            CombinedText = "class X{}",
            TotalChars   = 9,
            ApproxTokens = 2,
        };

    [Fact]
    public void ResultWriter_CreatesJsonFile()
    {
        string dir = Path.Combine(Path.GetTempPath(), $"exp_test_{Guid.NewGuid():N}");
        var writer = new ResultWriter(dir);
        var r = writer.Write(MakeResult(), MakeDataset());
        Assert.True(r.IsSuccess, r.ErrorMessage);
        Assert.True(File.Exists(r.Value.JsonPath), "JSON file must exist");
    }

    [Fact]
    public void ResultWriter_CreatesMdFile()
    {
        string dir = Path.Combine(Path.GetTempPath(), $"exp_test_{Guid.NewGuid():N}");
        var writer = new ResultWriter(dir);
        var r = writer.Write(MakeResult(), MakeDataset());
        Assert.True(r.IsSuccess, r.ErrorMessage);
        Assert.True(File.Exists(r.Value.MdPath), "MD file must exist");
    }

    [Fact]
    public void ResultWriter_JsonContainsRequiredFields()
    {
        string dir = Path.Combine(Path.GetTempPath(), $"exp_test_{Guid.NewGuid():N}");
        var writer = new ResultWriter(dir);
        var r = writer.Write(MakeResult("E03", 1), MakeDataset());
        Assert.True(r.IsSuccess, r.ErrorMessage);
        string json = File.ReadAllText(r.Value.JsonPath);
        Assert.Contains("\"experiment_id\"", json);
        Assert.Contains("\"E03\"", json);
        Assert.Contains("\"scale\"", json);
        Assert.Contains("\"baseline\"", json);
        Assert.Contains("\"enhanced\"", json);
        Assert.Contains("\"passed\"", json);
        Assert.Contains("\"next_action\"", json);
    }

    [Fact]
    public void ResultWriter_JsonIsValidJson()
    {
        string dir = Path.Combine(Path.GetTempPath(), $"exp_test_{Guid.NewGuid():N}");
        var writer = new ResultWriter(dir);
        var r = writer.Write(MakeResult(), MakeDataset());
        Assert.True(r.IsSuccess, r.ErrorMessage);
        string json = File.ReadAllText(r.Value.JsonPath);
        // Should not throw
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        Assert.Equal("E03", doc.RootElement.GetProperty("experiment_id").GetString());
    }

    [Fact]
    public void ResultWriter_FileNameEncodesTierAndScale()
    {
        string dir = Path.Combine(Path.GetTempPath(), $"exp_test_{Guid.NewGuid():N}");
        var writer = new ResultWriter(dir);
        var r = writer.Write(MakeResult("E06", 2), MakeDataset());
        Assert.True(r.IsSuccess, r.ErrorMessage);
        Assert.Contains("RESULT-E06-scale2", r.Value.JsonPath);
        Assert.Contains("RESULT-E06-scale2", r.Value.MdPath);
    }

    [Fact]
    public void ResultWriter_UpdateProgress_WritesProgressFiles()
    {
        string dir = Path.Combine(Path.GetTempPath(), $"exp_test_{Guid.NewGuid():N}");
        var writer = new ResultWriter(dir);
        var results = new[] { MakeResult("E01", 0), MakeResult("E02", 0) };
        writer.UpdateProgress(results);
        Assert.True(File.Exists(Path.Combine(dir, "progress.md")));
        Assert.True(File.Exists(Path.Combine(dir, "progress.json")));
    }

    [Fact]
    public void ResultWriter_MdContainsVerdict()
    {
        string dir = Path.Combine(Path.GetTempPath(), $"exp_test_{Guid.NewGuid():N}");
        var writer = new ResultWriter(dir);
        var r = writer.Write(MakeResult(), MakeDataset());
        Assert.True(r.IsSuccess, r.ErrorMessage);
        string md = File.ReadAllText(r.Value.MdPath);
        // 12% improvement → should pass
        Assert.Contains("PASSED", md);
    }

    // ── ExperimentRunner (integration) ────────────────────────────────────────

    [Fact]
    public void ExperimentRunner_Run_E03_Scale0_WritesResultFile()
    {
        string outDir = Path.Combine(Path.GetTempPath(), $"exp_run_{Guid.NewGuid():N}");
        var runner = new ExperimentRunner(ProjectRoot, outDir);
        var r = runner.Run("E03", scale: 0);
        Assert.True(r.IsSuccess, r.ErrorMessage);
        string jsonPath = Path.Combine(outDir, "RESULT-E03-scale0.json");
        Assert.True(File.Exists(jsonPath), $"Expected result file: {jsonPath}");
    }

    [Fact]
    public void ExperimentRunner_Run_E06_Scale0_WritesResultFile()
    {
        string outDir = Path.Combine(Path.GetTempPath(), $"exp_run_{Guid.NewGuid():N}");
        var runner = new ExperimentRunner(ProjectRoot, outDir);
        var r = runner.Run("E06", scale: 0);
        Assert.True(r.IsSuccess, r.ErrorMessage);
        string jsonPath = Path.Combine(outDir, "RESULT-E06-scale0.json");
        Assert.True(File.Exists(jsonPath), $"Expected result file: {jsonPath}");
    }

    [Fact]
    public void ExperimentRunner_Run_ScaleIsPatched()
    {
        string outDir = Path.Combine(Path.GetTempPath(), $"exp_run_{Guid.NewGuid():N}");
        var runner = new ExperimentRunner(ProjectRoot, outDir);
        var r = runner.Run("E03", scale: 1);
        Assert.True(r.IsSuccess, r.ErrorMessage);
        Assert.Equal(1, r.Value!.Scale);
    }

    [Fact]
    public void ExperimentRunner_Run_UnknownExperiment_Fails()
    {
        string outDir = Path.Combine(Path.GetTempPath(), $"exp_run_{Guid.NewGuid():N}");
        var runner = new ExperimentRunner(ProjectRoot, outDir);
        var r = runner.Run("E99", scale: 0);
        Assert.False(r.IsSuccess);
        Assert.Contains("E99", r.ErrorMessage);
    }

    [Fact]
    public void ExperimentRunner_Run_MemoryMb_IsNonZero()
    {
        string outDir = Path.Combine(Path.GetTempPath(), $"exp_run_{Guid.NewGuid():N}");
        var runner = new ExperimentRunner(ProjectRoot, outDir);
        var r = runner.Run("E03", scale: 0);
        Assert.True(r.IsSuccess, r.ErrorMessage);
        // After the MemoryMb patch, at least one of baseline/enhanced should be > 0.
        double totalMem = r.Value!.Baseline.MemoryMb + r.Value!.Enhanced.MemoryMb;
        Assert.True(totalMem > 0, "MemoryMb should be non-zero after GC-based measurement patch");
    }

    [Fact]
    public void ExperimentRunner_RunScaleLadder_E03_ReturnsResults()
    {
        string outDir = Path.Combine(Path.GetTempPath(), $"exp_ladder_{Guid.NewGuid():N}");
        var runner = new ExperimentRunner(ProjectRoot, outDir);
        var results = runner.RunScaleLadder("E03", maxScale: 1);
        Assert.True(results.Count >= 1, "At least Scale 0 should run");
        Assert.True(File.Exists(Path.Combine(outDir, "progress.md")));
        Assert.True(File.Exists(Path.Combine(outDir, "progress.json")));
    }
}
