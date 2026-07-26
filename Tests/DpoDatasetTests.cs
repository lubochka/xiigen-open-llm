// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text.Json;
using DPO;
using DPO.Models;

namespace LlmFromScratch.Tests;

/// <summary>
/// Unit tests for DpoDataset — JSONL loading and shuffling.
/// Behavioral assertion gate: asserts specific field values, not just "loaded without exception".
/// </summary>
public class DpoDatasetTests
{
    private static string WriteTempJsonl(IEnumerable<DpoPair> pairs)
    {
        var path = Path.GetTempFileName() + ".jsonl";
        File.WriteAllLines(path, pairs.Select(p => JsonSerializer.Serialize(p)));
        return path;
    }

    [Fact]
    public void Load_ValidFile_CountMatchesLineCount()
    {
        var pairs = new[]
        {
            new DpoPair { Prompt = "p1", Chosen = "c1", Rejected = "r1" },
            new DpoPair { Prompt = "p2", Chosen = "c2", Rejected = "r2" },
            new DpoPair { Prompt = "p3", Chosen = "c3", Rejected = "r3" },
        };
        var path = WriteTempJsonl(pairs);
        var ds = DpoDataset.Load(path);
        File.Delete(path);

        Assert.Equal(3, ds.Count);
    }

    [Fact]
    public void Load_ValidFile_FieldsPreserved()
    {
        var pair = new DpoPair { Prompt = "What is 2+2?", Chosen = "4", Rejected = "5" };
        var path = WriteTempJsonl([pair]);
        var ds = DpoDataset.Load(path);
        File.Delete(path);

        Assert.Equal("What is 2+2?", ds[0].Prompt);
        Assert.Equal("4", ds[0].Chosen);
        Assert.Equal("5", ds[0].Rejected);
    }

    [Fact]
    public void Load_EmptyFile_CountIsZero()
    {
        var path = Path.GetTempFileName() + ".jsonl";
        File.WriteAllText(path, "");
        var ds = DpoDataset.Load(path);
        File.Delete(path);

        Assert.Equal(0, ds.Count);
    }

    [Fact]
    public void Load_FileWithBlankLines_IgnoresBlanks()
    {
        var path = Path.GetTempFileName() + ".jsonl";
        File.WriteAllLines(path, new[]
        {
            JsonSerializer.Serialize(new DpoPair { Prompt = "p", Chosen = "c", Rejected = "r" }),
            "",
            "   ",
            JsonSerializer.Serialize(new DpoPair { Prompt = "p2", Chosen = "c2", Rejected = "r2" }),
        });
        var ds = DpoDataset.Load(path);
        File.Delete(path);

        Assert.Equal(2, ds.Count);
    }

    [Fact]
    public void Shuffle_DeterministicWithSeed_ProducesSameOrder()
    {
        var pairs = Enumerable.Range(0, 10)
            .Select(i => new DpoPair { Prompt = $"p{i}", Chosen = $"c{i}", Rejected = $"r{i}" });
        var path = WriteTempJsonl(pairs);
        var ds = DpoDataset.Load(path);
        File.Delete(path);

        var order1 = ds.Shuffle(seed: 42).Select(p => p.Prompt).ToList();
        var order2 = ds.Shuffle(seed: 42).Select(p => p.Prompt).ToList();

        Assert.Equal(order1, order2);
    }

    [Fact]
    public void Shuffle_ContainsAllOriginalItems()
    {
        var pairs = Enumerable.Range(0, 5)
            .Select(i => new DpoPair { Prompt = $"p{i}", Chosen = $"c{i}", Rejected = $"r{i}" });
        var path = WriteTempJsonl(pairs);
        var ds = DpoDataset.Load(path);
        File.Delete(path);

        var shuffled = ds.Shuffle(seed: 7).ToList();

        Assert.Equal(5, shuffled.Count);
        for (int i = 0; i < 5; i++)
            Assert.Contains(shuffled, p => p.Prompt == $"p{i}");
    }

    [Fact]
    public void Indexer_OutOfRange_Throws()
    {
        var pair = new DpoPair { Prompt = "p", Chosen = "c", Rejected = "r" };
        var path = WriteTempJsonl([pair]);
        var ds = DpoDataset.Load(path);
        File.Delete(path);

        Assert.Throws<ArgumentOutOfRangeException>(() => _ = ds[5]);
    }
}
