// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using ExperimentsV2;
using System.Text.Json;

namespace LlmFromScratch.Tests;

// ─── Behavioral tests for 35.ExperimentsV2 Infrastructure + E07 ──────────────
// DatasetLoader, InputAdapters, ResultWriter, ExperimentRunner, E07.
// All I/O tests use temp directories; no real project files required.

public sealed class ExperimentsV2InfraTests
{
    // ── DatasetLoader ──────────────────────────────────────────────────────────

    [Fact]
    public void DatasetLoader_MissingRoot_ReturnsFail()
    {
        var r = DatasetLoader.Load(1, @"C:\nonexistent_path_xyz_abc");
        Assert.False(r.IsSuccess);
        Assert.Contains("not found", r.ErrorMessage);
    }

    [Fact]
    public void DatasetLoader_EmptyRoot_ReturnsFail()
    {
        string tmpRoot = Path.Combine(Path.GetTempPath(), "dl_empty_" + Guid.NewGuid());
        Directory.CreateDirectory(tmpRoot);
        try
        {
            var r = DatasetLoader.Load(1, tmpRoot);
            Assert.False(r.IsSuccess);
        }
        finally { Directory.Delete(tmpRoot, true); }
    }

    [Fact]
    public void DatasetLoader_WithCsFiles_ReturnsContents()
    {
        string tmpRoot   = Path.Combine(Path.GetTempPath(), "dl_cs_" + Guid.NewGuid());
        string tokenizer = Path.Combine(tmpRoot, "01.Tokenizer");
        Directory.CreateDirectory(tokenizer);
        File.WriteAllText(Path.Combine(tokenizer, "Foo.cs"), "public class Foo {}");
        File.WriteAllText(Path.Combine(tokenizer, "Bar.cs"), "public class Bar {}");
        try
        {
            var r = DatasetLoader.Load(1, tmpRoot);
            Assert.True(r.IsSuccess, r.ErrorMessage);
            Assert.Equal(2, r.Value!.FileCount);
            Assert.Equal(1, r.Value.Tier);
            Assert.Contains("Foo", r.Value.CombinedText);
        }
        finally { Directory.Delete(tmpRoot, true); }
    }

    [Fact]
    public void DatasetLoader_ObjFilesAreExcluded()
    {
        string tmpRoot   = Path.Combine(Path.GetTempPath(), "dl_obj_" + Guid.NewGuid());
        string tokenizer = Path.Combine(tmpRoot, "01.Tokenizer");
        string objDir    = Path.Combine(tokenizer, "obj", "Debug");
        Directory.CreateDirectory(tokenizer);
        Directory.CreateDirectory(objDir);
        File.WriteAllText(Path.Combine(tokenizer, "Real.cs"), "class Real {}");
        File.WriteAllText(Path.Combine(objDir, "Generated.cs"), "class Gen {}");
        try
        {
            var r = DatasetLoader.Load(1, tmpRoot);
            Assert.True(r.IsSuccess);
            Assert.Equal(1, r.Value!.FileCount);
        }
        finally { Directory.Delete(tmpRoot, true); }
    }

    [Fact]
    public void DatasetLoader_Tier3_UsesAllNumberedFolders()
    {
        string tmpRoot = Path.Combine(Path.GetTempPath(), "dl_t3_" + Guid.NewGuid());
        string folder1 = Path.Combine(tmpRoot, "01.Tokenizer");
        string folder2 = Path.Combine(tmpRoot, "02.DatasetPipeline");
        string ignored = Path.Combine(tmpRoot, "SomeName");
        Directory.CreateDirectory(folder1);
        Directory.CreateDirectory(folder2);
        Directory.CreateDirectory(ignored);
        File.WriteAllText(Path.Combine(folder1, "A.cs"), "class A {}");
        File.WriteAllText(Path.Combine(folder2, "B.cs"), "class B {}");
        File.WriteAllText(Path.Combine(ignored, "C.cs"), "class C {}");
        try
        {
            var r = DatasetLoader.Load(3, tmpRoot);
            Assert.True(r.IsSuccess);
            Assert.Equal(2, r.Value!.FileCount);
        }
        finally { Directory.Delete(tmpRoot, true); }
    }

    [Fact]
    public void DatasetLoader_CombinedTextJoinsAllFiles()
    {
        string tmpRoot   = Path.Combine(Path.GetTempPath(), "dl_comb_" + Guid.NewGuid());
        string tokenizer = Path.Combine(tmpRoot, "01.Tokenizer");
        Directory.CreateDirectory(tokenizer);
        File.WriteAllText(Path.Combine(tokenizer, "X.cs"), "class X {}");
        File.WriteAllText(Path.Combine(tokenizer, "Y.cs"), "class Y {}");
        try
        {
            var r = DatasetLoader.Load(1, tmpRoot);
            Assert.True(r.IsSuccess);
            Assert.Contains("class X", r.Value!.CombinedText);
            Assert.Contains("class Y", r.Value!.CombinedText);
        }
        finally { Directory.Delete(tmpRoot, true); }
    }

    // ── InputAdapters ─────────────────────────────────────────────────────────

    [Fact]
    public void InputAdapters_ForE01_ReturnsCombinedText()
    {
        var ds = MakeDataset("class Foo {}");
        Assert.Equal("class Foo {}", InputAdapters.ForE01(ds));
    }

    [Fact]
    public void InputAdapters_ForE05_ReturnsCombinedTextAndSources()
    {
        var ds = MakeDataset("class Bar {}");
        var (combined, allSources) = InputAdapters.ForE05(ds);
        Assert.Equal("class Bar {}", combined);
        Assert.True(allSources.Count >= 1);
    }

    [Fact]
    public void InputAdapters_ForE02_ReturnsOneEntryPerFile()
    {
        var ds = MakeDatasetMulti("class A {}", "class B {}");
        var texts = InputAdapters.ForE02(ds);
        Assert.Equal(2, texts.Count);
    }

    [Fact]
    public void InputAdapters_ForE03_ReturnsCharTokens()
    {
        string code = string.Join(" ", Enumerable.Repeat("class Foo { public void Bar() {} }", 30));
        var ds = MakeDataset(code);
        var r = InputAdapters.ForE03(ds);
        Assert.True(r.IsSuccess, r.ErrorMessage);
        Assert.True(r.Value!.Count >= 10);
        Assert.True(r.Value.Max() < 128); // char-level: all tokens are ASCII codes
    }

    [Fact]
    public void InputAdapters_ForE04_Tier1_FailsWithNotEnoughFiles()
    {
        var ds = MakeDataset("class Foo {}");
        var r = InputAdapters.ForE04(ds);
        Assert.False(r.IsSuccess);
        Assert.Contains("Tier 2+", r.ErrorMessage);
    }

    [Fact]
    public void InputAdapters_ForE06_WithMethodCalls_ReturnsPairs()
    {
        string src = @"
public class Calculator {
    public int Add(int a, int b) { return Compute(a, b); }
    private int Compute(int x, int y) { return x + y; }
}";
        var ds = MakeDataset(src);
        var r = InputAdapters.ForE06(ds);
        Assert.True(r.IsSuccess, r.ErrorMessage);
        Assert.True(r.Value!.Count > 0);
    }

    [Fact]
    public void InputAdapters_ForE06_EmptySource_Fails()
    {
        var ds = MakeDataset("  ");
        var r = InputAdapters.ForE06(ds);
        Assert.False(r.IsSuccess);
    }

    // ── ResultWriter ──────────────────────────────────────────────────────────

    [Fact]
    public void ResultWriter_Write_CreatesJsonFile()
    {
        string outputRoot = Path.Combine(Path.GetTempPath(), "rw_json_" + Guid.NewGuid());
        try
        {
            var r = MakeResult("E01-AST-TOKENIZER");
            var op = ResultWriter.Write(r, outputRoot);
            Assert.True(op.IsSuccess, op.ErrorMessage);
            Assert.True(File.Exists(op.Value!));
        }
        finally { Directory.Delete(outputRoot, true); }
    }

    [Fact]
    public void ResultWriter_Write_CreatesMdFile()
    {
        string outputRoot = Path.Combine(Path.GetTempPath(), "rw_md_" + Guid.NewGuid());
        try
        {
            var r = MakeResult("E01-AST-TOKENIZER");
            var op = ResultWriter.Write(r, outputRoot);
            string mdPath = op.Value!.Replace(".json", ".md");
            Assert.True(File.Exists(mdPath));
        }
        finally { Directory.Delete(outputRoot, true); }
    }

    [Fact]
    public void ResultWriter_Write_JsonContainsCorrectFields()
    {
        string outputRoot = Path.Combine(Path.GetTempPath(), "rw_fields_" + Guid.NewGuid());
        try
        {
            var result = new ExperimentV2Result
            {
                ExperimentId      = "E01-AST-TOKENIZER",
                Tier              = 1,
                BaselinePrimary   = 185.0,
                EnhancedPrimary   = 42.0,
                PrimaryMetricName = "perplexity_3gram",
                LowerIsBetter     = true
            };
            var op = ResultWriter.Write(result, outputRoot);
            Assert.True(op.IsSuccess);
            using var doc = JsonDocument.Parse(File.ReadAllText(op.Value!));
            var el = doc.RootElement;
            Assert.Equal("E01-AST-TOKENIZER", el.GetProperty("experiment_id").GetString());
            Assert.Equal(1,    el.GetProperty("tier").GetInt32());
            Assert.True(       el.GetProperty("passed").GetBoolean());
            Assert.Equal("PROMOTE_TO_NEXT_TIER", el.GetProperty("next_action").GetString());
        }
        finally { Directory.Delete(outputRoot, true); }
    }

    [Fact]
    public void ResultWriter_Write_SecondCallIncrementsSeq()
    {
        string outputRoot = Path.Combine(Path.GetTempPath(), "rw_seq_" + Guid.NewGuid());
        try
        {
            var r = MakeResult("E03-NGRAM-MODEL");
            var op1 = ResultWriter.Write(r, outputRoot);
            var op2 = ResultWriter.Write(r, outputRoot);
            Assert.True(op1.IsSuccess);
            Assert.True(op2.IsSuccess);
            Assert.NotEqual(op1.Value, op2.Value);
        }
        finally { Directory.Delete(outputRoot, true); }
    }

    [Fact]
    public void ResultWriter_Write_CreatesProgressFiles()
    {
        string outputRoot = Path.Combine(Path.GetTempPath(), "rw_prog_" + Guid.NewGuid());
        try
        {
            var r = MakeResult("E05-SYNTAX-FILTER");
            ResultWriter.Write(r, outputRoot);
            Assert.True(File.Exists(Path.Combine(outputRoot, "progress", "progress.json")));
            Assert.True(File.Exists(Path.Combine(outputRoot, "progress", "progress.md")));
        }
        finally { Directory.Delete(outputRoot, true); }
    }

    [Fact]
    public void ResultWriter_Write_ProgressJsonContainsEntry()
    {
        string outputRoot = Path.Combine(Path.GetTempPath(), "rw_prog2_" + Guid.NewGuid());
        try
        {
            var r = MakeResult("E06-PAGERANK-RANKING");
            ResultWriter.Write(r, outputRoot);
            string json = File.ReadAllText(Path.Combine(outputRoot, "progress", "progress.json"));
            Assert.Contains("E06-PAGERANK-RANKING", json);
        }
        finally { Directory.Delete(outputRoot, true); }
    }

    [Fact]
    public void ResultWriter_Write_MdContainsPassedMarker()
    {
        string outputRoot = Path.Combine(Path.GetTempPath(), "rw_md2_" + Guid.NewGuid());
        try
        {
            var r = MakeResult("E02-FREQUENCY-RANKING");
            var op = ResultWriter.Write(r, outputRoot);
            string md = File.ReadAllText(op.Value!.Replace(".json", ".md"));
            Assert.Contains("PASSED", md);
        }
        finally { Directory.Delete(outputRoot, true); }
    }

    [Fact]
    public void ResultWriter_Write_UnknownId_UsesIdAsFolder()
    {
        string outputRoot = Path.Combine(Path.GetTempPath(), "rw_unk_" + Guid.NewGuid());
        try
        {
            var r = MakeResult("E99-CUSTOM");
            var op = ResultWriter.Write(r, outputRoot);
            Assert.True(op.IsSuccess);
            Assert.True(File.Exists(op.Value!));
        }
        finally { Directory.Delete(outputRoot, true); }
    }

    // ── Tier patch ────────────────────────────────────────────────────────────

    [Fact]
    public void E01_Run_TierParameterPropagates()
    {
        string src = string.Join(" ", Enumerable.Repeat("class T { void M() {} }", 30));
        var r = new E01AstTokenizer().Run(src, tier: 3);
        Assert.True(r.IsSuccess, r.ErrorMessage);
        Assert.Equal(3, r.Value!.Tier);
    }

    [Fact]
    public void E02_Run_TierParameterPropagates()
    {
        string src = string.Join(" ", Enumerable.Repeat("class T { void M() { list.Add(x); } }", 20));
        var r = new E02FrequencyRanking().Run([src], tier: 2);
        Assert.True(r.IsSuccess, r.ErrorMessage);
        Assert.Equal(2, r.Value!.Tier);
    }

    [Fact]
    public void E05_Run_TierParameterPropagates()
    {
        string src = string.Join(" ", Enumerable.Repeat("class T { void M() {} }", 30));
        var r = new E05SyntaxFilter().Run(src, tier: 2);
        Assert.True(r.IsSuccess, r.ErrorMessage);
        Assert.Equal(2, r.Value!.Tier);
    }

    [Fact]
    public void E06_Run_TierParameterPropagates()
    {
        var pairs = Enumerable.Range(0, 20)
            .Select(i => ($"Caller{i}", $"Callee{i % 5}"))
            .ToList();
        var r = new E06PageRank().Run(pairs, tier: 3);
        Assert.True(r.IsSuccess, r.ErrorMessage);
        Assert.Equal(3, r.Value!.Tier);
    }

    // ── Dispatcher tier ───────────────────────────────────────────────────────

    [Fact]
    public void Dispatcher_RunE01_PassesTierThrough()
    {
        string src = string.Join(" ", Enumerable.Repeat("class T { void M() {} }", 30));
        var d = new ExperimentsV2Dispatcher();
        var r = d.RunE01(src, tier: 2);
        Assert.True(r.IsSuccess, r.ErrorMessage);
        Assert.Equal(2, r.Value!.Tier);
    }

    [Fact]
    public void Dispatcher_RunE05_PassesTierThrough()
    {
        string src = string.Join(" ", Enumerable.Repeat("class T { void M() {} }", 30));
        var d = new ExperimentsV2Dispatcher();
        var r = d.RunE05(src, tier: 3);
        Assert.True(r.IsSuccess, r.ErrorMessage);
        Assert.Equal(3, r.Value!.Tier);
    }

    // ── E07 StructuralContextToken ────────────────────────────────────────────

    [Fact]
    public void E07_Run_WithValidSource_Succeeds()
    {
        string src = string.Join("\n", Enumerable.Repeat(
            "class Foo { public void Bar() { var x = 1; var y = x + 2; } }", 20));
        var files = new List<(string, string)> { ("Foo.cs", src) };
        var r = new E07StructuralContextToken().Run(src, files);
        Assert.True(r.IsSuccess, r.ErrorMessage);
        Assert.Equal("E07-STRUCTURAL-CONTEXT-TOKEN", r.Value!.ExperimentId);
    }

    [Fact]
    public void E07_Run_BaselineAndEnhancedInValidRange()
    {
        string src = string.Join("\n", Enumerable.Repeat(
            "class Foo { public void Bar() { var x = 1; var y = x + 2; } }", 20));
        var files = new List<(string, string)> { ("Foo.cs", src) };
        var r = new E07StructuralContextToken().Run(src, files).Value!;
        Assert.InRange(r.BaselinePrimary, 0.0, 1.0);
        Assert.InRange(r.EnhancedPrimary, 0.0, 1.0);
    }

    [Fact]
    public void E07_Run_TierPropagatesToResult()
    {
        string src = string.Join("\n", Enumerable.Repeat(
            "class Foo { public void Bar() { var x = 1; } }", 20));
        var files = new List<(string, string)> { ("Foo.cs", src) };
        var r = new E07StructuralContextToken().Run(src, files, tier: 3);
        Assert.True(r.IsSuccess, r.ErrorMessage);
        Assert.Equal(3, r.Value!.Tier);
    }

    [Fact]
    public void E07_Run_NotesContainContextInfo()
    {
        string src = string.Join("\n", Enumerable.Repeat(
            "class Foo { public void Bar() { var x = 1; } }", 20));
        var files = new List<(string, string)> { ("Foo.cs", src) };
        var r = new E07StructuralContextToken().Run(src, files).Value!;
        Assert.Contains("tier=", r.Notes);
        Assert.Contains("bpeTokens=", r.Notes);
    }

    [Fact]
    public void E07_Run_EmptySource_Fails()
    {
        var r = new E07StructuralContextToken().Run("", []);
        Assert.False(r.IsSuccess);
    }

    [Fact]
    public void StructuralToken_DefaultValues_AreZero()
    {
        var t = new StructuralToken();
        Assert.Equal(0, t.BpeId);
        Assert.Equal(0, t.AstNodeType);
        Assert.Equal(0, t.AstDepth);
    }

    [Fact]
    public void MultidimensionalNGram_Train_And_Predict_ReturnsBpeId()
    {
        var tokens = Enumerable.Range(0, 50)
            .Select(i => new StructuralToken
            {
                BpeId       = i % 5,
                AstNodeType = (short)(i % 3),
                AstDepth    = (byte)(i % 4)
            })
            .ToList();

        var ngram = new MultidimensionalNGram();
        ngram.Train(tokens);
        int pred = ngram.Predict(0, 0, 0);
        Assert.InRange(pred, 0, 4);
    }

    [Fact]
    public void MultidimensionalNGram_UniqueContexts_IsPositive()
    {
        var tokens = Enumerable.Range(0, 30)
            .Select(i => new StructuralToken
            {
                BpeId       = i % 4,
                AstNodeType = (short)(i % 3),
                AstDepth    = (byte)(i % 2)
            })
            .ToList();

        var ngram = new MultidimensionalNGram();
        ngram.Train(tokens);
        Assert.True(ngram.UniqueContexts > 0);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static DatasetContents MakeDataset(string text) =>
        new DatasetContents
        {
            Tier         = 1,
            ProjectRoot  = "/fake",
            Files        = new List<(string, string)> { ("Foo.cs", text) }.AsReadOnly(),
            CombinedText = text
        };

    private static DatasetContents MakeDatasetMulti(params string[] texts) =>
        new DatasetContents
        {
            Tier         = 1,
            ProjectRoot  = "/fake",
            Files        = texts.Select((t, i) => ($"File{i}.cs", t)).ToList().AsReadOnly(),
            CombinedText = string.Join("\n", texts)
        };

    private static ExperimentV2Result MakeResult(string id) =>
        new ExperimentV2Result
        {
            ExperimentId      = id,
            Tier              = 1,
            BaselinePrimary   = 100.0,
            EnhancedPrimary   = 50.0,
            PrimaryMetricName = "perplexity",
            LowerIsBetter     = true,
            Notes             = "test"
        };
}
