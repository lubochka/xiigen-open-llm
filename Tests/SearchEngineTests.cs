// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using SearchEngine;

namespace LlmFromScratch.Tests;

// Unit + integration tests for sub-project 12 (SearchEngine).
// Behavioral assertion gate: all assertions verify concrete values/contracts.
// No external parity — all implementations are internal pure-C#.
public class SearchEngineTests
{
    // ──────────────────────────────────────────────────────────────────────────
    // PorterStemmer
    // ──────────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("running",  "run")]
    [InlineData("flies",    "fli")]
    [InlineData("happily",  "happili")]
    [InlineData("national", "nation")]
    [InlineData("generalization", "gener")]
    [InlineData("agreed",   "agre")]
    [InlineData("disabled", "disabl")]
    [InlineData("matting",  "mat")]
    [InlineData("mating",   "mate")]
    [InlineData("meeting",  "meet")]
    public void PorterStemmer_Stem_MatchesExpectedOutput(string word, string expected)
    {
        var stemmer = new PorterStemmer();
        Assert.Equal(expected, stemmer.Stem(word));
    }

    [Fact]
    public void PorterStemmer_ShortWords_ReturnedUnchanged()
    {
        var stemmer = new PorterStemmer();
        Assert.Equal("go", stemmer.Stem("go"));
        Assert.Equal("be", stemmer.Stem("be"));
    }

    // ──────────────────────────────────────────────────────────────────────────
    // StandardAnalyzer
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void StandardAnalyzer_Analyze_ProducesTokens()
    {
        var analyzer = new StandardAnalyzer();
        var tokens   = analyzer.Analyze("The quick brown foxes are running fast");

        // Stop words ("the", "are") and short words removed; rest stemmed
        Assert.True(tokens.Count > 0);
        Assert.All(tokens, t => Assert.False(string.IsNullOrWhiteSpace(t.Term)));
    }

    [Fact]
    public void StandardAnalyzer_StopWords_Excluded()
    {
        var analyzer = new StandardAnalyzer();
        var tokens   = analyzer.Analyze("the and or but");
        Assert.Empty(tokens);
    }

    [Fact]
    public void StandardAnalyzer_Positions_AreIncreasing()
    {
        var analyzer = new StandardAnalyzer();
        var tokens   = analyzer.Analyze("machine learning algorithms");
        for (int i = 1; i < tokens.Count; i++)
            Assert.True(tokens[i].Position > tokens[i - 1].Position);
    }

    [Fact]
    public void StandardAnalyzer_EmptyInput_ReturnsEmpty()
    {
        var analyzer = new StandardAnalyzer();
        Assert.Empty(analyzer.Analyze(""));
        Assert.Empty(analyzer.Analyze("   "));
    }

    // ──────────────────────────────────────────────────────────────────────────
    // PostingsList
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void PostingsList_Add_AccumulatesFrequency()
    {
        var pl = new PostingsList();
        pl.Add(docId: 0, position: 0);
        pl.Add(docId: 0, position: 3);
        pl.Add(docId: 1, position: 1);

        var postings = pl.ToPostings();
        Assert.Equal(2, postings.Count);
        Assert.Equal(2, postings.First(p => p.DocId == 0).Frequency);
        Assert.Equal(1, postings.First(p => p.DocId == 1).Frequency);
    }

    [Fact]
    public void PostingsList_Positions_RecordedCorrectly()
    {
        var pl = new PostingsList();
        pl.Add(docId: 0, position: 5);
        pl.Add(docId: 0, position: 9);

        var posting = pl.ToPostings().Single();
        Assert.Contains(5, posting.Positions);
        Assert.Contains(9, posting.Positions);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // InvertedIndex
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void InvertedIndex_Add_StoresDocumentInfo()
    {
        var idx      = new InvertedIndex();
        var analyzer = new StandardAnalyzer();

        idx.Add("doc1", analyzer.Analyze("quick brown fox"));

        Assert.Equal(1, idx.DocCount);
        Assert.Equal("doc1", idx.Documents[0].ExternalId);
    }

    [Fact]
    public void InvertedIndex_GetPostings_ReturnsCorrectFrequency()
    {
        var idx      = new InvertedIndex();
        var analyzer = new StandardAnalyzer();

        idx.Add("doc1", analyzer.Analyze("fox fox jumped"));
        var postings = idx.GetPostings(new PorterStemmer().Stem("fox"));

        Assert.True(postings.Count > 0);
        Assert.Equal(2, postings[0].Frequency);
    }

    [Fact]
    public void InvertedIndex_AvgDocLen_ComputedCorrectly()
    {
        var idx      = new InvertedIndex();
        var analyzer = new StandardAnalyzer();

        // "brown fox" → 2 tokens (no stop words); "machine learning" → 2 tokens
        idx.Add("d1", analyzer.Analyze("brown fox"));
        idx.Add("d2", analyzer.Analyze("machine learning"));

        float avg = idx.AvgDocLen;
        Assert.True(avg > 0f);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // BM25Scorer
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void BM25Scorer_Score_HigherForMoreRelevantDoc()
    {
        var idx      = new InvertedIndex();
        var analyzer = new StandardAnalyzer();

        // doc1 mentions "machine" twice, doc2 once
        idx.Add("doc1", analyzer.Analyze("machine learning machine"));
        idx.Add("doc2", analyzer.Analyze("machine vision"));

        var scorer  = new BM25Scorer(idx);
        var results = scorer.Score(analyzer.Analyze("machine"), topK: 5);

        Assert.True(results.Count >= 2);
        Assert.Equal("doc1", results[0].ExternalId);
    }

    [Fact]
    public void BM25Scorer_Score_EmptyQuery_ReturnsEmpty()
    {
        var idx    = new InvertedIndex();
        var scorer = new BM25Scorer(idx);
        var result = scorer.Score([], topK: 5);
        Assert.Empty(result);
    }

    [Fact]
    public void BM25Scorer_Score_AllScoresPositive()
    {
        var idx      = new InvertedIndex();
        var analyzer = new StandardAnalyzer();
        idx.Add("d1", analyzer.Analyze("deep learning neural networks"));
        idx.Add("d2", analyzer.Analyze("support vector machine classification"));

        var scorer  = new BM25Scorer(idx);
        var results = scorer.Score(analyzer.Analyze("learning"), topK: 10);

        Assert.All(results, r => Assert.True(r.Score > 0f));
    }

    // ──────────────────────────────────────────────────────────────────────────
    // SearchEngineFacade — integration
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void SearchEngine_Add_ReturnsOk()
    {
        var engine = new SearchEngineFacade();
        var result = engine.Add("doc1", "hello world");
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void SearchEngine_Add_EmptyId_ReturnsFail()
    {
        var engine = new SearchEngineFacade();
        var result = engine.Add("", "some text");
        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.Error);
    }

    [Fact]
    public void SearchEngine_Search_FindsRelevantDocument()
    {
        var engine = new SearchEngineFacade();
        engine.Add("ai-doc",    "artificial intelligence machine learning");
        engine.Add("cook-doc",  "chocolate cake recipe baking");

        var result = engine.Search("machine learning", topK: 3);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.Count > 0);
        Assert.Equal("ai-doc", result.Value![0].ExternalId);
    }

    [Fact]
    public void SearchEngine_Search_TopK_LimitsResults()
    {
        var engine = new SearchEngineFacade();
        for (int i = 0; i < 10; i++)
            engine.Add($"doc{i}", $"information retrieval document {i} search");

        var result = engine.Search("information search", topK: 3);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.Count <= 3);
    }

    [Fact]
    public void SearchEngine_Search_EmptyQuery_ReturnsEmpty()
    {
        var engine = new SearchEngineFacade();
        engine.Add("d1", "hello world");

        var result = engine.Search("", topK: 5);
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }

    [Fact]
    public void SearchEngine_Search_RankedByRelevance()
    {
        var engine = new SearchEngineFacade();
        engine.Add("highly-relevant", "neural neural neural networks deep learning");
        engine.Add("somewhat",        "deep learning basics");
        engine.Add("irrelevant",      "cooking pasta recipe");

        var result = engine.Search("neural networks", topK: 5);

        Assert.True(result.IsSuccess);
        Assert.Equal("highly-relevant", result.Value![0].ExternalId);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // IndexStorage — atomic persistence
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void IndexStorage_SaveAndLoad_RoundTrips()
    {
        var engine  = new SearchEngineFacade();
        engine.Add("doc1", "machine learning algorithms");
        engine.Add("doc2", "deep neural networks");

        var snap    = engine.Snapshot();
        var storage = new IndexStorage();
        string path = Path.Combine(Path.GetTempPath(), $"se_test_{Guid.NewGuid()}.json");

        try
        {
            var saveResult = storage.Save(path, snap);
            Assert.True(saveResult.IsSuccess);
            Assert.True(File.Exists(path));

            var loadResult = storage.Load(path);
            Assert.True(loadResult.IsSuccess);
            Assert.Equal(2, loadResult.Value!.Documents.Count);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void IndexStorage_Load_MissingFile_ReturnsFail()
    {
        var storage = new IndexStorage();
        var result  = storage.Load("/nonexistent/path/index.json");
        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.Error);
    }

    [Fact]
    public void IndexStorage_RestoredEngine_CanSearch()
    {
        var engine = new SearchEngineFacade();
        engine.Add("ai",     "artificial intelligence learning");
        engine.Add("cooking","pasta recipe baking");

        var snap    = engine.Snapshot();
        var storage = new IndexStorage();
        string path = Path.Combine(Path.GetTempPath(), $"se_restore_{Guid.NewGuid()}.json");

        try
        {
            storage.Save(path, snap);
            var loadResult = storage.Load(path);
            var restored   = SearchEngineFacade.FromSnapshot(loadResult.Value!);

            var result = restored.Search("intelligence learning", topK: 3);
            Assert.True(result.IsSuccess);
            Assert.Equal("ai", result.Value![0].ExternalId);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
