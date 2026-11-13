// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using SemanticNlp;

namespace LlmFromScratch.Tests;

// ─── Unit + end-to-end tests for subprojects 28 through 32 ──────────────────
// Each algorithm has its own region of tests, then a final region exercises
// the SemanticNlpDispatcher to prove all five algorithms wire up correctly.

public sealed class SemanticNlpTests
{
    // Helper that builds a fresh storage + event-logger pair so each test is
    // independent and can inspect the recorded events.
    private static (InMemorySemanticStorage Storage, InMemorySemanticEventLogger Events) CreateFabric()
        => (new InMemorySemanticStorage(), new InMemorySemanticEventLogger());

    // ─── FrameNet ───────────────────────────────────────────────────────────

    [Fact]
    public async Task FrameNet_DetectsAttackFrame()
    {
        var lexicon  = FrameNetLexicon.CreateDefault();
        var detector = new FrameDetector(lexicon);

        var tokens = new[] { "The", "police", "attacked", "the", "protesters" };

        var result = await detector.DetectFrameAsync(tokens);

        Assert.True(result.IsSuccess);
        Assert.Equal("Attack", result.Value.FrameName);
        Assert.Equal("attacked", result.Value.TriggerWord);
    }

    [Fact]
    public async Task FrameNet_FillsAttackerAndVictimRoles()
    {
        var (storage, events) = CreateFabric();
        var lexicon   = FrameNetLexicon.CreateDefault();
        var detector  = new FrameDetector(lexicon);
        var filler    = new FrameElementFiller();
        var annotator = new FrameNetAnnotator(detector, filler);
        var service   = new FrameNetService(annotator, storage, events);

        var result = await service.AnnotateAsync("op1", "The police attacked the protesters");

        Assert.True(result.IsSuccess);
        Assert.Equal("FrameNet", result.Value!.AlgorithmName);
        Assert.Equal("Attack", result.Value.FrameName);
        Assert.Equal("attacked", result.Value.PredicateText);

        // Verify role assignment per the Attack frame definition.
        var byText = result.Value.Tokens.ToDictionary(t => t.OriginalText);
        Assert.Equal("Attacker", byText["police"].RoleWithinFamily);
        Assert.Equal("Victim",   byText["protesters"].RoleWithinFamily);
        Assert.Equal("Predicate", byText["attacked"].RoleWithinFamily);
    }

    [Fact]
    public async Task FrameNet_DetectsCommerceBuyFrame()
    {
        var (storage, events) = CreateFabric();
        var lexicon   = FrameNetLexicon.CreateDefault();
        var detector  = new FrameDetector(lexicon);
        var filler    = new FrameElementFiller();
        var annotator = new FrameNetAnnotator(detector, filler);
        var service   = new FrameNetService(annotator, storage, events);

        var result = await service.AnnotateAsync("op2", "John bought a book");

        Assert.True(result.IsSuccess);
        Assert.Equal("Commerce_buy", result.Value!.FrameName);
        Assert.Equal("bought", result.Value.PredicateText);

        var byText = result.Value.Tokens.ToDictionary(t => t.OriginalText);
        Assert.Equal("Buyer",     byText["John"].RoleWithinFamily);
        Assert.Equal("Goods",     byText["book"].RoleWithinFamily);
        Assert.Equal("Predicate", byText["bought"].RoleWithinFamily);
    }

    [Fact]
    public async Task FrameNet_UnknownWordsProduceUnannotatedResult()
    {
        var (storage, events) = CreateFabric();
        var annotator = new FrameNetAnnotator(
            new FrameDetector(FrameNetLexicon.CreateDefault()),
            new FrameElementFiller());
        var service = new FrameNetService(annotator, storage, events);

        var result = await service.AnnotateAsync("op3", "qwerty asdfgh zxcvbn");

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value!.FrameName);
        Assert.Null(result.Value.PredicateText);
        Assert.All(result.Value.Tokens, t => Assert.Null(t.RoleWithinFamily));
    }

    [Fact]
    public async Task FrameNet_EmptySentenceFailsWithEmptyCode()
    {
        var (storage, events) = CreateFabric();
        var service = new FrameNetService(
            new FrameNetAnnotator(
                new FrameDetector(FrameNetLexicon.CreateDefault()),
                new FrameElementFiller()),
            storage, events);

        var result = await service.AnnotateAsync("op4", "   ");

        Assert.False(result.IsSuccess);
        Assert.Equal("EMPTY_SENTENCE", result.ErrorCode);
    }

    [Fact]
    public async Task FrameNet_IsIdempotentAcrossCallsWithSameOperationId()
    {
        var (storage, events) = CreateFabric();
        var service = new FrameNetService(
            new FrameNetAnnotator(
                new FrameDetector(FrameNetLexicon.CreateDefault()),
                new FrameElementFiller()),
            storage, events);

        var first  = await service.AnnotateAsync("dup-op", "John bought a book");
        var second = await service.AnnotateAsync("dup-op", "Different sentence — should be ignored");

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);

        // Second call must return the cached annotation from the first call,
        // so the original sentence is preserved.
        Assert.Equal(first.Value!.OriginalSentence, second.Value!.OriginalSentence);

        // Only one completion event recorded — the second call short-circuited.
        Assert.Single(events.Events, e => e.EventType == "semantic.annotation.completed");
    }

    [Fact]
    public async Task FrameNet_PersistsAnnotationBeforeEvent()
    {
        var (storage, events) = CreateFabric();
        var service = new FrameNetService(
            new FrameNetAnnotator(
                new FrameDetector(FrameNetLexicon.CreateDefault()),
                new FrameElementFiller()),
            storage, events);

        await service.AnnotateAsync("save-op", "John bought a book");

        // The annotation must be retrievable from storage (DNA rule 8).
        var loaded = await storage.LoadAnnotationAsync("save-op");
        Assert.True(loaded.IsSuccess);
        Assert.Equal("Commerce_buy", loaded.Value!.FrameName);

        // And exactly one completion event must have fired.
        Assert.Single(events.Events, e => e.EventType == "semantic.annotation.completed");
    }

    [Fact]
    public async Task FrameNetLexicon_GetFrameMembersFailsForUnknownFrame()
    {
        var lexicon = FrameNetLexicon.CreateDefault();

        var result = await lexicon.GetFrameMembersAsync("Nonexistent");

        Assert.False(result.IsSuccess);
        Assert.Equal("FRAME_NOT_FOUND", result.ErrorCode);
    }

    // ─── SRL ────────────────────────────────────────────────────────────────

    [Fact]
    public void Srl_PredicateFinder_LocatesVerbInSentence()
    {
        var finder = PredicateFinder.CreateDefault();

        var result = finder.FindPredicatePositions(
            new[] { "John", "quickly", "sold", "the", "book", "to", "Mary" });

        Assert.True(result.IsSuccess);
        Assert.Contains(2, result.Value!); // "sold" is at index 2
    }

    [Fact]
    public async Task Srl_LabelsAgentThemeAndManner()
    {
        var (storage, events) = CreateFabric();
        var annotator = new SrlAnnotator(PredicateFinder.CreateDefault(), new ArgumentLabeler());
        var service   = new SrlService(annotator, storage, events);

        var result = await service.AnnotateAsync("srl1", "John quickly sold the book to Mary");

        Assert.True(result.IsSuccess);
        Assert.Equal("SRL", result.Value!.AlgorithmName);
        Assert.Equal("sold", result.Value.PredicateText);

        var byText = result.Value.Tokens.ToDictionary(t => t.OriginalText);

        // John is before the verb → A0 (Agent)
        Assert.StartsWith("A0", byText["John"].RoleWithinFamily);
        // quickly is a manner adverb → AM-MNR
        Assert.StartsWith("AM-MNR", byText["quickly"].RoleWithinFamily);
        // book is after the verb and not a marker → A1 (Theme)
        Assert.StartsWith("A1", byText["book"].RoleWithinFamily);
        // "to" is a location marker → AM-LOC (the rule treats "to" as locative)
        Assert.StartsWith("AM-LOC", byText["to"].RoleWithinFamily);
        // The verb itself
        Assert.Equal("V", byText["sold"].RoleWithinFamily);
    }

    [Fact]
    public async Task Srl_NoVerbProducesUnlabelledAnnotation()
    {
        var (storage, events) = CreateFabric();
        var service = new SrlService(
            new SrlAnnotator(PredicateFinder.CreateDefault(), new ArgumentLabeler()),
            storage, events);

        var result = await service.AnnotateAsync("srl2", "qwerty asdfgh zxcvbn");

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value!.PredicateText);
        Assert.All(result.Value.Tokens, t => Assert.Null(t.RoleWithinFamily));
    }

    [Fact]
    public async Task Srl_EmptyInputFails()
    {
        var (storage, events) = CreateFabric();
        var service = new SrlService(
            new SrlAnnotator(PredicateFinder.CreateDefault(), new ArgumentLabeler()),
            storage, events);

        var result = await service.AnnotateAsync("srl3", "");

        Assert.False(result.IsSuccess);
        Assert.Equal("EMPTY_SENTENCE", result.ErrorCode);
    }

    // ─── WordNet ────────────────────────────────────────────────────────────

    [Fact]
    public async Task WordNet_FindsSynsetForKnownWord()
    {
        var database = SynsetDatabase.CreateDefault();

        var result = await database.FindSynsetsForWordAsync("dog");

        Assert.True(result.IsSuccess);
        Assert.Contains("dog.n.01", result.Value!);
    }

    [Fact]
    public void WordNet_HypernymChainGoesUpToRoot()
    {
        var database  = SynsetDatabase.CreateDefault();
        var navigator = new SynsetNavigator(database);

        var chain = navigator.GetHypernymChain("dog.n.01");

        Assert.True(chain.IsSuccess);
        Assert.Equal("dog.n.01",   chain.Value![0]);
        Assert.Contains("canine.n.01", chain.Value);
        Assert.Contains("mammal.n.01", chain.Value);
        Assert.Contains("animal.n.01", chain.Value);
        Assert.Contains("entity.n.01", chain.Value);
    }

    [Fact]
    public async Task WordNet_WuPalmerSimilarity_CloseWordsScoreHigh()
    {
        var database  = SynsetDatabase.CreateDefault();
        var navigator = new SynsetNavigator(database);

        var dogVsWolf = await navigator.ComputeSimilarityAsync("dog", "wolf");
        var dogVsCat  = await navigator.ComputeSimilarityAsync("dog", "cat");

        Assert.True(dogVsWolf.IsSuccess);
        Assert.True(dogVsCat.IsSuccess);

        // Dog and wolf share a closer ancestor (canine) than dog and cat (mammal),
        // so dog/wolf similarity must be strictly higher.
        Assert.True(dogVsWolf.Value > dogVsCat.Value,
            $"dog/wolf={dogVsWolf.Value} should exceed dog/cat={dogVsCat.Value}");

        // Similarity must lie in [0, 1].
        Assert.InRange(dogVsWolf.Value, 0.0, 1.0);
        Assert.InRange(dogVsCat.Value,  0.0, 1.0);
    }

    [Fact]
    public async Task WordNet_SimilarityFailsForUnknownWord()
    {
        var navigator = new SynsetNavigator(SynsetDatabase.CreateDefault());

        var result = await navigator.ComputeSimilarityAsync("dog", "nonexistentword");

        Assert.False(result.IsSuccess);
        Assert.Equal("WORD_NOT_FOUND", result.ErrorCode);
    }

    [Fact]
    public async Task WordNet_AnnotatorAttachesSynsetIdToKnownWords()
    {
        var (storage, events) = CreateFabric();
        var service = new WordNetService(
            new WordNetAnnotator(SynsetDatabase.CreateDefault()),
            storage, events);

        var result = await service.AnnotateAsync("wn1", "the dog is an animal");

        Assert.True(result.IsSuccess);
        var byText = result.Value!.Tokens.ToDictionary(t => t.OriginalText);
        Assert.Equal("synset:dog.n.01",    byText["dog"].SemanticFamily);
        Assert.Equal("synset:animal.n.01", byText["animal"].SemanticFamily);

        // The articles "the"/"an" are not in the lexicon — must have null family.
        Assert.Null(byText["the"].SemanticFamily);
        Assert.Null(byText["an"].SemanticFamily);
    }

    // ─── AMR ────────────────────────────────────────────────────────────────

    [Fact]
    public void Amr_PenmanParser_ParsesNestedNodes()
    {
        var parser = new AmrPenmanParser();

        var result = parser.Parse("(w / want-01 :ARG0 (b / boy) :ARG1 (g / girl))");

        Assert.True(result.IsSuccess);
        var graph = result.Value!;

        // Three nodes (w, b, g) and two edges (:ARG0 to b, :ARG1 to g).
        Assert.Equal(3, graph.Nodes.Count);
        Assert.Equal(2, graph.Edges.Count);
        Assert.Equal("want-01", graph.RootNode.Concept);
        Assert.Equal("w", graph.RootNode.VariableName);

        Assert.Contains(graph.Edges, e => e.RelationLabel == ":ARG0" && e.TargetVariable == "b");
        Assert.Contains(graph.Edges, e => e.RelationLabel == ":ARG1" && e.TargetVariable == "g");
    }

    [Fact]
    public void Amr_PenmanParser_FailsOnEmptyInput()
    {
        var result = new AmrPenmanParser().Parse("");

        Assert.False(result.IsSuccess);
        Assert.Equal("EMPTY_INPUT", result.ErrorCode);
    }

    [Fact]
    public void Amr_PenmanRoundTrip_PreservesStructure()
    {
        var original = "(w / want-01 :ARG0 (b / boy) :ARG1 (g / girl))";

        var parsed = new AmrPenmanParser().Parse(original);
        Assert.True(parsed.IsSuccess);

        var serialized = new AmrPenmanSerializer().Serialize(parsed.Value!);
        Assert.True(serialized.IsSuccess);

        // Re-parse the serialized output and confirm equivalent structure.
        var reparsed = new AmrPenmanParser().Parse(serialized.Value!);
        Assert.True(reparsed.IsSuccess);
        Assert.Equal(parsed.Value!.Nodes.Count, reparsed.Value!.Nodes.Count);
        Assert.Equal(parsed.Value.Edges.Count,  reparsed.Value.Edges.Count);
        Assert.Equal(parsed.Value.RootNode.Concept, reparsed.Value.RootNode.Concept);
    }

    [Fact]
    public void Amr_SimpleBuilder_UsesFirstVerbAsRoot()
    {
        var builder = new SimpleAmrBuilder(PredicateFinder.CreateDefault());

        var result = builder.BuildFromSentence("boy wants girl");

        Assert.True(result.IsSuccess);
        Assert.StartsWith("wants", result.Value!.RootNode.Concept); // "wants-01"
        Assert.Contains(result.Value.Edges, e => e.RelationLabel == ":ARG0");
        Assert.Contains(result.Value.Edges, e => e.RelationLabel == ":ARG1");
    }

    [Fact]
    public async Task Amr_AnnotatorEmitsTokensAndPenman()
    {
        var (storage, events) = CreateFabric();
        var annotator = new AmrAnnotator(
            new SimpleAmrBuilder(PredicateFinder.CreateDefault()),
            new AmrPenmanSerializer());
        var service = new AmrService(annotator, storage, events);

        var result = await service.AnnotateAsync("amr1", "John bought a book");

        Assert.True(result.IsSuccess);
        Assert.Equal("AMR", result.Value!.AlgorithmName);
        Assert.NotNull(result.Value.PredicateText); // serialized Penman string
        Assert.Contains("(", result.Value.PredicateText);
        Assert.Contains("bought-01", result.Value.FrameName);
    }

    // ─── Universal Dependencies ─────────────────────────────────────────────

    [Fact]
    public void Ud_ConlluParser_ReadsTextCommentAndTokens()
    {
        var conllu = "# text = The cat sat\n" +
                     "1\tThe\tthe\tDET\t_\t_\t2\tdet\t_\t_\n" +
                     "2\tcat\tcat\tNOUN\t_\t_\t3\tnsubj\t_\t_\n" +
                     "3\tsat\tsit\tVERB\t_\t_\t0\troot\t_\t_\n";

        var result = new ConlluParser().ParseBlock(conllu);

        Assert.True(result.IsSuccess);
        var tree = result.Value!;
        Assert.Equal("The cat sat", tree.OriginalSentence);
        Assert.Equal(3, tree.Tokens.Count);

        var sat = tree.Tokens.First(t => t.OriginalForm == "sat");
        Assert.Equal(0, sat.HeadTokenId); // root
        Assert.Equal("root", sat.DependencyRelation);

        var cat = tree.Tokens.First(t => t.OriginalForm == "cat");
        Assert.Equal("nsubj", cat.DependencyRelation);
    }

    [Fact]
    public void Ud_RuleBasedParser_AssignsNsubjAndObj()
    {
        var parser = new RuleBasedParser(PredicateFinder.CreateDefault());

        var result = parser.Parse("The cat chased the mouse");

        Assert.True(result.IsSuccess);

        var tokens = result.Value!.Tokens;
        var cat    = tokens.First(t => t.OriginalForm == "cat");
        var mouse  = tokens.First(t => t.OriginalForm == "mouse");
        var chased = tokens.First(t => t.OriginalForm == "chased");

        Assert.Equal("nsubj", cat.DependencyRelation);
        Assert.Equal("obj",   mouse.DependencyRelation);
        Assert.Equal("root",  chased.DependencyRelation);
        Assert.Equal(0,       chased.HeadTokenId);
    }

    [Fact]
    public void Ud_Navigator_FindsRootChildrenAndHead()
    {
        var parser = new RuleBasedParser(PredicateFinder.CreateDefault());
        var tree   = parser.Parse("The cat chased the mouse").Value!;
        var nav    = new DependencyTreeNavigator();

        var root = nav.GetRoot(tree);
        Assert.NotNull(root);
        Assert.Equal("chased", root!.OriginalForm);

        var children = nav.GetChildren(tree, root.TokenId);
        Assert.Contains(children, c => c.OriginalForm == "cat");
        Assert.Contains(children, c => c.OriginalForm == "mouse");

        var catHead = nav.GetHead(tree, children.First(c => c.OriginalForm == "cat").TokenId);
        Assert.NotNull(catHead);
        Assert.Equal("chased", catHead!.OriginalForm);
    }

    [Fact]
    public async Task Ud_AnnotatorEmitsRelationsAsRoles()
    {
        var (storage, events) = CreateFabric();
        var service = new UniversalDependenciesService(
            new UniversalDependenciesAnnotator(
                new RuleBasedParser(PredicateFinder.CreateDefault())),
            storage, events);

        var result = await service.AnnotateAsync("ud1", "The cat chased the mouse");

        Assert.True(result.IsSuccess);
        var byText = result.Value!.Tokens.ToDictionary(t => t.OriginalText);
        Assert.Equal("nsubj", byText["cat"].RoleWithinFamily);
        Assert.Equal("obj",   byText["mouse"].RoleWithinFamily);
        Assert.Equal("root",  byText["chased"].RoleWithinFamily);
        Assert.Equal("chased", result.Value.PredicateText);
    }

    // ─── Dispatcher (DNA rule 6) ────────────────────────────────────────────

    [Fact]
    public async Task Dispatcher_RoutesEachAlgorithmToTheRightService()
    {
        var (storage, events) = CreateFabric();

        var frameNet = new FrameNetService(
            new FrameNetAnnotator(
                new FrameDetector(FrameNetLexicon.CreateDefault()),
                new FrameElementFiller()),
            storage, events);

        var srl = new SrlService(
            new SrlAnnotator(PredicateFinder.CreateDefault(), new ArgumentLabeler()),
            storage, events);

        var wordNet = new WordNetService(
            new WordNetAnnotator(SynsetDatabase.CreateDefault()),
            storage, events);

        var amr = new AmrService(
            new AmrAnnotator(
                new SimpleAmrBuilder(PredicateFinder.CreateDefault()),
                new AmrPenmanSerializer()),
            storage, events);

        var ud = new UniversalDependenciesService(
            new UniversalDependenciesAnnotator(
                new RuleBasedParser(PredicateFinder.CreateDefault())),
            storage, events);

        var dispatcher = new SemanticNlpDispatcher(
            new ISemanticAnnotator[] { frameNet, srl, wordNet, amr, ud },
            events);

        // Every algorithm name must be discoverable.
        Assert.Contains("FrameNet",              dispatcher.AvailableAlgorithms);
        Assert.Contains("SRL",                   dispatcher.AvailableAlgorithms);
        Assert.Contains("WordNet",               dispatcher.AvailableAlgorithms);
        Assert.Contains("AMR",                   dispatcher.AvailableAlgorithms);
        Assert.Contains("UniversalDependencies", dispatcher.AvailableAlgorithms);

        var sentence = "John bought a book";

        var fnResult  = await dispatcher.AnnotateAsync("FrameNet", "d-fn",  sentence);
        var srlResult = await dispatcher.AnnotateAsync("SRL",      "d-srl", sentence);
        var wnResult  = await dispatcher.AnnotateAsync("WordNet",  "d-wn",  sentence);
        var amrResult = await dispatcher.AnnotateAsync("AMR",      "d-amr", sentence);
        var udResult  = await dispatcher.AnnotateAsync("UniversalDependencies", "d-ud", sentence);

        Assert.Equal("FrameNet",              fnResult.Value!.AlgorithmName);
        Assert.Equal("SRL",                   srlResult.Value!.AlgorithmName);
        Assert.Equal("WordNet",               wnResult.Value!.AlgorithmName);
        Assert.Equal("AMR",                   amrResult.Value!.AlgorithmName);
        Assert.Equal("UniversalDependencies", udResult.Value!.AlgorithmName);
    }

    [Fact]
    public async Task Dispatcher_FailsClearlyForUnknownAlgorithm()
    {
        var (storage, events) = CreateFabric();
        var dispatcher = new SemanticNlpDispatcher(
            Array.Empty<ISemanticAnnotator>(), events);

        var result = await dispatcher.AnnotateAsync("Bogus", "x", "anything");

        Assert.False(result.IsSuccess);
        Assert.Equal("ALGORITHM_NOT_FOUND", result.ErrorCode);
    }

    [Fact]
    public async Task Dispatcher_LogsRoutingEvent()
    {
        var (storage, events) = CreateFabric();
        var frameNet = new FrameNetService(
            new FrameNetAnnotator(
                new FrameDetector(FrameNetLexicon.CreateDefault()),
                new FrameElementFiller()),
            storage, events);

        var dispatcher = new SemanticNlpDispatcher(new[] { frameNet }, events);

        await dispatcher.AnnotateAsync("FrameNet", "log-op", "John bought a book");

        Assert.Contains(events.Events, e => e.EventType == "semantic.dispatch.requested");
        Assert.Contains(events.Events, e => e.EventType == "semantic.annotation.completed");
    }
}
