// N30 native source-level executable micro-probe. STANDALONE, NOT RUN HERE.
// Provenance: core codex/dpo-implementation@79082340f01b744cdf08185616f2bd818dbd06b8.
// Keep this file in planning/possibleDifs. Copy into a disposable C# console
// project that references 27.TranslationDomain/27.TranslationDomain.csproj.
// Does not modify production core and does not call GitHub Windows CI.
// L0 only: confirms lost KNN reach and direction leakage; not an end-to-end
// TranslationRoutingFlow/ML quality experiment or independent bilingual grade.

using System;
using System.Text;
using System.Text.Json;
using TranslationDomain;

public static class MicroN30
{
    public static int Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        try
        {
            var okA = ProbeOrdinaryReachability();
            var okB = ProbeSharedDirectionLeakage();
            Console.WriteLine($"N30_L0_RESULT: ordinary_gate={(okA ? "CONFIRMED" : "NOT_REPRODUCED")}; cross_direction={(okB ? "CONFIRMED" : "NOT_REPRODUCED")}");
            Console.WriteLine("L1_LIVE_CONSUMER=NOT_RUN; L2_BLIND_QUALITY=NOT_RUN; COLD_RESTORE=NOT_RUN");
            return okA && okB ? 0 : 2;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"N30_PROBE_FAILED: {ex}");
            return 3;
        }
    }

    private static bool ProbeOrdinaryReachability()
    {
        // Same template family, different source strings: this isolates reachability,
        // not generalization to an unseen semantic family.
        const string source = "large garden window";
        const string query = "big garden window";
        const string chosen = "ventana grande del jardín";
        var store = new TranslationKnnStore();
        var enEs = new ExecutorUnit(TranslationDirection.EN_ES, store);
        Teach(enEs, source, "en", chosen);
        var tokens = new TranslationSemanticTokenizer().Tokenize(query, "en");

        var rawMemory = store.FindNearestCascade(
            tokens.Signature, tokens.PrefixSignature, tokens.StructureSignature,
            k: 5, querySourceText: query);
        var reachableByRawStore = ContainsTarget(rawMemory, chosen);
        var actual = enEs.Execute(tokens, "general");
        var ignoredByOrdinaryGate = actual.AdapterId.EndsWith("-fallback", StringComparison.Ordinal);

        // A broad retrieval policy can also be harmful. This negative shows why
        // blindly enabling KNN is not a proven quality improvement.
        var negative = new TranslationSemanticTokenizer().Tokenize("small garden window", "en");
        var falseNeighborRisk = ContainsTarget(store.FindNearestCascade(
            negative.Signature, negative.PrefixSignature, negative.StructureSignature,
            k: 5, querySourceText: "small garden window"), chosen);

        Console.WriteLine($"N30A: signature={tokens.Signature}; hasConcept={HasConcept(tokens)}; storeCount={store.Count}; rawCandidate={reachableByRawStore}; actual={actual.Text}; adapter={actual.AdapterId}; confidence={actual.Confidence}; negativeNeighborRisk={falseNeighborRisk}");

        // This expectation is source-level. If changed, inspect the exact current
        // C# method and do not rewrite source/fixture to manufacture a PASS.
        return !HasConcept(tokens)
            && reachableByRawStore
            && ignoredByOrdinaryGate
            && falseNeighborRisk;
    }

    private static bool ProbeSharedDirectionLeakage()
    {
        const string spanishSource = "null property";
        const string spanishTarget = "propiedad nula";
        const string hebrewSource = "null attribute";
        const string hebrewTarget = "מאפיין null";
        const string query = "null property"; // trained for EN_ES, not EN_HE.

        var shared = new TranslationKnnStore();
        var spanish = new ExecutorUnit(TranslationDirection.EN_ES, shared);
        var hebrew = new ExecutorUnit(TranslationDirection.EN_HE, shared);
        Teach(spanish, spanishSource, "en", spanishTarget);
        Teach(hebrew, hebrewSource, "en", hebrewTarget);

        // Treatment: equal lawful EN_HE training, but no EN_ES rows in this
        // Executor's KNN namespace; FIFO/rank within EN_HE remains unchanged.
        var isolated = new TranslationKnnStore();
        var hebrewScoped = new ExecutorUnit(TranslationDirection.EN_HE, isolated);
        Teach(hebrewScoped, hebrewSource, "en", hebrewTarget);

        var tokens = new TranslationSemanticTokenizer().Tokenize(query, "en");
        var sharedAnswer = hebrew.Execute(tokens, "general");
        var scopedAnswer = hebrewScoped.Execute(tokens, "general");
        var verifier = new QualityVerifierUnit();
        var wrongAccepted = verifier.Verify(sharedAnswer.Text, tokens).Pass;

        Console.WriteLine($"N30B: signature={tokens.Signature}; shared={sharedAnswer.Text}; sharedAdapter={sharedAnswer.AdapterId}; isolated={scopedAnswer.Text}; isolatedAdapter={scopedAnswer.AdapterId}; falseVerifierPass={wrongAccepted}");

        return HasConcept(tokens)
            && sharedAnswer.AdapterId.EndsWith("-knn", StringComparison.Ordinal)
            && scopedAnswer.AdapterId.EndsWith("-knn", StringComparison.Ordinal)
            && string.Equals(sharedAnswer.Text, spanishTarget, StringComparison.Ordinal)
            && string.Equals(scopedAnswer.Text, hebrewTarget, StringComparison.Ordinal)
            && wrongAccepted;
    }

    private static bool HasConcept(SemanticTokenSequence sequence)
    {
        foreach (var token in sequence.Tokens)
            if (token.Concept != SemanticConcept.UNKNOWN)
                return true;
        return false;
    }

    private static bool ContainsTarget(
        System.Collections.Generic.IReadOnlyList<(string Source, string Target)> rows,
        string target)
    {
        foreach (var row in rows)
            if (string.Equals(row.Target, target, StringComparison.Ordinal))
                return true;
        return false;
    }

    private static void Teach(
        ExecutorUnit unit, string source, string sourceLang, string target)
    {
        var sequence = new TranslationSemanticTokenizer().Tokenize(source, sourceLang);
        var pair = new TranslationLearningExample(
            ExampleId: "micro-n30-" + unit.UnitId + "-" + source,
            UnitId: unit.UnitId,
            FlowId: TranslationRoutingFlow.FlowId,
            InputJson: JsonSerializer.Serialize(new ExecutorInput(source, sourceLang, "general")),
            ChosenOutputJson: JsonSerializer.Serialize(new ExecutorOutput(target, 0.93, null, null)),
            RejectedOutputJson: JsonSerializer.Serialize(new ExecutorOutput("WRONG", 0.10, null, null)),
            RejectedType: "wrong_target_language",
            SemanticSignature: sequence.Signature,
            SourceId: "n30-bounded-legal-synthetic-teaching-fixture",
            HeldOut: false);
        unit.TrainOnExample(pair);
    }
}
