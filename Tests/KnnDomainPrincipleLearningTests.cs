// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using Tokenizer;
using TransformerModel;
using TransformerModel.Config;

// Transfer of experiment KNN-DOMAIN-PRINCIPLES-01: opt-in candidate ranking inside the native KNN unit.
// The index assigns node levels from a shared random source, so every A/B comparison reuses one trained unit.
public sealed class KnnDomainPrincipleLearningTests
{
    // An empty catalogue must leave selection, answer, confidence and evidence exactly as before the transfer.
    [Fact]
    public async Task EmptyCatalogue_KeepsNativeSelectionOutputConfidenceAndEvidence()
    {
        // Build the tiny tokenizer this panel is trained on.
        var tokenizer = BuildTokenizer();
        // Build the tiny transformer that produces the embeddings.
        var model = new LlmModel(BuildTinyConfig());
        // Load the four Russian rows of the measured panel.
        var examples = RussianExamples();

        // Train the reference unit on the default path, with no catalogue at all.
        var native = TransformerEmbeddingKnnLogicalUnit<string>.Train(
            "kdp-empty-native", "generated-capability", "RU", model, tokenizer, examples);
        // A failed training would make every later comparison meaningless, so stop here with its message.
        Assert.True(native.IsSuccess, native.ErrorMessage);

        // Build the empty catalogue, which is the declared default of the opt-in path.
        var emptyCatalog = KnnCandidatePrincipleCatalog.CreateEmpty("RU");
        // An empty catalogue must still pass admission, so a refusal here is a defect.
        Assert.True(emptyCatalog.IsSuccess, emptyCatalog.ErrorMessage);

        // The same trained memory and the same index are reused, so only the ranking policy differs.
        var optIn = native.Value!.WithCandidatePrinciples(emptyCatalog.Value!);
        // Attaching an empty catalogue to a matching profile must be accepted.
        Assert.True(optIn.IsSuccess, optIn.ErrorMessage);

        // Ask both units the same exam questions and compare them answer by answer.
        foreach (var query in RussianQueries())
        {
            // Run the question through the untouched default path.
            var fromNative = await native.Value!.ExecuteAsync(Request(query));
            // Run the same question through the unit that carries the empty catalogue.
            var fromOptIn = await optIn.Value!.ExecuteAsync(Request(query));

            // The default path must answer at all.
            Assert.True(fromNative.IsSuccess, fromNative.ErrorMessage);
            // The opt-in path must answer at all.
            Assert.True(fromOptIn.IsSuccess, fromOptIn.ErrorMessage);
            // The returned stored answer must be the same on both paths.
            Assert.Equal(fromNative.Value!.Output, fromOptIn.Value!.Output);
            // The confidence must not be re-rounded by the empty catalogue.
            Assert.Equal(fromNative.Value!.Confidence, fromOptIn.Value!.Confidence);
            // The evidence list must not gain a ranking marker that never ran.
            Assert.Equal(fromNative.Value!.EvidenceIds, fromOptIn.Value!.EvidenceIds);
        }

        // An empty catalogue must not advertise a ranking ingredient that never ran.
        Assert.DoesNotContain("KDP01:CANDIDATE_PRINCIPLE_RANKING", optIn.Value!.Manifest.IngredientIds);

        // The training entry point must also accept an empty catalogue without changing the ingredient list.
        var trainedWithEmpty = TransformerEmbeddingKnnLogicalUnit<string>.Train(
            "kdp-empty-trained", "generated-capability", "RU", model, tokenizer, examples, emptyCatalog.Value!);
        // Training with an empty catalogue must succeed exactly like training without one.
        Assert.True(trainedWithEmpty.IsSuccess, trainedWithEmpty.ErrorMessage);
        // The ingredient list must stay byte-identical to the default path.
        Assert.Equal(native.Value!.Manifest.IngredientIds, trainedWithEmpty.Value!.Manifest.IngredientIds);
    }

    // The opt-in ranking must be able to change the product answer, otherwise the transfer is dead code.
    [Fact]
    public async Task AdmittedPrinciple_ChangesTheAnswerOfTheRealUnit()
    {
        // Build the tiny tokenizer this panel is trained on.
        var tokenizer = BuildTokenizer();
        // Build the tiny transformer that produces the embeddings.
        var model = new LlmModel(BuildTinyConfig());

        // Two stored examples carry the same input text, so the native fusion scores them equally
        // and the stable identity order of project 18 selects the first identifier.
        var examples = new[]
        {
            // This row wins by identity order alone, before any principle runs.
            new TransformerKnnTrainingExample<string>("a-first", "Задание: продолжи. Префикс: вчера я", "FIRST", [], ["RU"]),
            // This row is the one the admitted principle will prefer.
            new TransformerKnnTrainingExample<string>("b-second", "Задание: продолжи. Префикс: вчера я", "SECOND", [], ["RU"])
        };

        // Train the reference unit on the default path, with no catalogue at all.
        var native = TransformerEmbeddingKnnLogicalUnit<string>.Train(
            "kdp-flip-native", "generated-capability", "RU", model, tokenizer, examples);
        // A failed training would make the before/after comparison meaningless.
        Assert.True(native.IsSuccess, native.ErrorMessage);

        // Record the answer of the default path before any ranking is attached.
        var beforeRanking = await native.Value!.ExecuteAsync(Request("Задание: продолжи. Префикс: вчера я"));
        // The default path must answer at all.
        Assert.True(beforeRanking.IsSuccess, beforeRanking.ErrorMessage);
        // Identity order alone selects the first stored row.
        Assert.Equal("FIRST", beforeRanking.Value!.Output);

        // One admitted principle supports the second example and argues against the first one.
        var catalog = KnnCandidatePrincipleCatalog.Create("RU", [new FixedChoicePrinciple("RU", "b-second")]);
        // A well formed catalogue must pass admission.
        Assert.True(catalog.IsSuccess, catalog.ErrorMessage);

        // Attach the catalogue to the very same trained memory, so only the ranking policy differs.
        var ranked = native.Value!.WithCandidatePrinciples(catalog.Value!);
        // Attaching a catalogue of the matching profile must be accepted.
        Assert.True(ranked.IsSuccess, ranked.ErrorMessage);

        // Ask the same question again, now through the unit that carries the admitted principle.
        var afterRanking = await ranked.Value!.ExecuteAsync(Request("Задание: продолжи. Префикс: вчера я"));
        // The opt-in path must answer at all.
        Assert.True(afterRanking.IsSuccess, afterRanking.ErrorMessage);

        // The stored answer of the newly preferred example is returned verbatim.
        Assert.Equal("SECOND", afterRanking.Value!.Output);
        // The evidence must name the stored row that actually won.
        Assert.Contains("b-second", afterRanking.Value!.EvidenceIds);
        // The evidence must also record that the opt-in ranking took part in the decision.
        Assert.Contains("KDP01:CANDIDATE_PRINCIPLE_RANKING", afterRanking.Value!.EvidenceIds);
        // The manifest must advertise the ranking ingredient now that it really runs.
        Assert.Contains("KDP01:CANDIDATE_PRINCIPLE_RANKING", ranked.Value!.Manifest.IngredientIds);

        // The unit that was given no catalogue keeps answering as before, so the change is opt-in only.
        var stillNative = await native.Value!.ExecuteAsync(Request("Задание: продолжи. Префикс: вчера я"));
        // The untouched unit must still answer at all.
        Assert.True(stillNative.IsSuccess, stillNative.ErrorMessage);
        // The untouched unit must still return its original answer.
        Assert.Equal("FIRST", stillNative.Value!.Output);
    }

    // The ranking only reorders learned candidates, so the answer always stays inside the trained memory.
    [Fact]
    public async Task AdmittedPrinciple_NeverInventsAnAnswerOutsideTheLearnedMemory()
    {
        // Build the tiny tokenizer this panel is trained on.
        var tokenizer = BuildTokenizer();
        // Build the tiny transformer that produces the embeddings.
        var model = new LlmModel(BuildTinyConfig());
        // Load the four Russian rows of the measured panel.
        var examples = RussianExamples();

        // Admit the real measured Russian time-frame rule, not a test double.
        var catalog = KnnCandidatePrincipleCatalog.Create("RU", [KnnDomainCandidatePrincipleProfiles.RussianTimeFrame()]);
        // The shipped profile must pass its own admission gate.
        Assert.True(catalog.IsSuccess, catalog.ErrorMessage);

        // Train the unit directly with the catalogue attached.
        var unit = TransformerEmbeddingKnnLogicalUnit<string>.Train(
            "kdp-memory-only", "generated-capability", "RU", model, tokenizer, examples, catalog.Value!);
        // Training with an admitted catalogue must succeed.
        Assert.True(unit.IsSuccess, unit.ErrorMessage);

        // Collect every answer the trained memory is allowed to return.
        var stored = examples.Select(example => example.Output).ToHashSet(StringComparer.Ordinal);
        // Ask every exam question and check that nothing outside the memory can come back.
        foreach (var query in RussianQueries())
        {
            // Run the question through the ranked unit.
            var answer = await unit.Value!.ExecuteAsync(Request(query));
            // The ranked unit must answer at all.
            Assert.True(answer.IsSuccess, answer.ErrorMessage);
            // The answer must be one of the stored outputs, never an invented one.
            Assert.Contains(answer.Value!.Output, stored);
        }
    }

    // The measured Russian time-frame rule must judge exactly as the experiment judged it.
    [Theory]
    // Same explicit past marker on both sides supports the candidate.
    [InlineData("Задание: продолжи. Префикс: вчера я", "Задание: продолжи. Префикс: вчера он", true, 1.0)]
    // A habitual request against a past candidate argues against the candidate.
    [InlineData("Задание: продолжи. Префикс: каждый день я", "Задание: продолжи. Префикс: вчера он", true, -1.0)]
    // A negated marker is not evidence, so the rule stays out of the decision.
    [InlineData("Задание: продолжи. Префикс: не вчера я", "Задание: продолжи. Префикс: вчера он", false, 0.0)]
    // Two conflicting markers in one sentence are a refusal, not a first-match choice.
    [InlineData("Задание: продолжи. Префикс: вчера и завтра я", "Задание: продолжи. Префикс: вчера он", false, 0.0)]
    // Without any marker there is nothing to judge.
    [InlineData("Задание: продолжи. Префикс: он", "Задание: продолжи. Префикс: вчера он", false, 0.0)]
    public void RussianTimeFrame_JudgesExactlyLikeTheExperiment(string queryText, string candidateText, bool applicable, double value)
    {
        // Take the shipped Russian rule exactly as the product would build it.
        var principle = KnnDomainCandidatePrincipleProfiles.RussianTimeFrame();
        // Judge this one request against this one stored example.
        var score = principle.Evaluate(new KnnPrincipleQuery("RU", queryText), new KnnPrincipleCandidate("e1", candidateText));

        // A missing judgement is not a passed check, so the applicability itself is asserted.
        Assert.Equal(applicable, score.Applicable);
        // The judgement value must match the measured experiment to ten decimal places.
        Assert.Equal(value, score.Value, 10);
    }

    // The words of the instruction must never leak into the time frame of the prefix.
    [Fact]
    public void RussianTimeFrame_ReadsOnlyThePrefixSegment()
    {
        // Take the shipped Russian rule exactly as the product would build it.
        var principle = KnnDomainCandidatePrincipleProfiles.RussianTimeFrame();

        // The instruction mentions tomorrow, the prefix mentions yesterday: only the prefix may vote.
        var score = principle.Evaluate(
            new KnnPrincipleQuery("RU", "Задание: расскажи про завтра. Префикс: вчера я"),
            new KnnPrincipleCandidate("e1", "Задание: продолжи. Префикс: вчера он"));

        // Both prefixes carry a decidable frame, so the rule must judge.
        Assert.True(score.Applicable);
        // Both prefixes are in the past, so the rule must support the candidate.
        Assert.Equal(1.0, score.Value, 10);
    }

    // The measured orchestration rule must judge exactly as the experiment judged it.
    [Theory]
    // Both rows stand at the start of the plan, so the order agrees.
    [InlineData("parent=p; contract=c; completed=пусто", "parent=p; contract=c; completed=пусто", 1.0)]
    // The candidate is a later step while the request completed nothing, so the precondition fails.
    [InlineData("parent=p; contract=c; completed=пусто", "parent=p; contract=c; completed=сбор", -1.0)]
    // The same completed tail supports the candidate.
    [InlineData("parent=p; contract=c; completed=сбор разбор", "parent=p; contract=c; completed=сбор разбор", 1.0)]
    // A different completed tail argues against the candidate.
    [InlineData("parent=p; contract=c; completed=сбор разбор", "parent=p; contract=c; completed=сбор проверка", -1.0)]
    public void OrchestrationPredecessorStage_JudgesExactlyLikeTheExperiment(string queryText, string candidateText, double value)
    {
        // Take the shipped orchestration rule exactly as the product would build it.
        var principle = KnnDomainCandidatePrincipleProfiles.OrchestrationPredecessorStage();
        // Judge this one request row against this one stored row.
        var score = principle.Evaluate(new KnnPrincipleQuery("OR", queryText), new KnnPrincipleCandidate("e1", candidateText));

        // Every row of this panel carries labelled stages, so the rule must always judge here.
        Assert.True(score.Applicable);
        // The judgement value must match the measured experiment to ten decimal places.
        Assert.Equal(value, score.Value, 10);
    }

    // The adjustment of one principle must stay inside the proven budget of two tenths.
    [Fact]
    public void Ranker_KeepsTheAdjustmentInsideTheProvenBudget()
    {
        // One test principle that supports the identity "win" and argues against everything else.
        var catalog = KnnCandidatePrincipleCatalog.Create("RU", [new FixedChoicePrinciple("RU", "win")]);
        // A well formed catalogue must pass admission.
        Assert.True(catalog.IsSuccess, catalog.ErrorMessage);

        // Rank a pair whose native scores are one tenth apart.
        var ranked = KnnCandidatePrincipleRanker.Rank(
            catalog.Value!,
            new KnnPrincipleQuery("RU", "любой запрос"),
            [
                new KnnNativeCandidate("win", "текст", 0.40),
                new KnnNativeCandidate("lose", "текст", 0.50)
            ]);

        // Ranking a valid pair must succeed.
        Assert.True(ranked.IsSuccess, ranked.ErrorMessage);
        // Index the result by identity so each candidate can be checked on its own.
        var byId = ranked.Value!.ToDictionary(candidate => candidate.ExampleId, StringComparer.Ordinal);
        // The supported candidate gains exactly the full two tenths of the budget.
        Assert.Equal(0.60, byId["win"].FinalScore, 10);
        // The opposed candidate loses exactly the full two tenths of the budget.
        Assert.Equal(0.30, byId["lose"].FinalScore, 10);
        // The native score is kept untouched next to the adjusted one, so the move stays auditable.
        Assert.Equal(0.40, byId["win"].NativeScore, 10);
        // The adjustment was large enough to reverse this pair.
        Assert.Equal("win", ranked.Value![0].ExampleId);
    }

    // A native gap wider than four tenths cannot be reversed by one principle, exactly as the plan proves.
    [Fact]
    public void Ranker_CannotReverseAPairWhoseNativeGapExceedsFourTenths()
    {
        // One test principle that supports the identity "win" and argues against everything else.
        var catalog = KnnCandidatePrincipleCatalog.Create("RU", [new FixedChoicePrinciple("RU", "win")]);
        // A well formed catalogue must pass admission.
        Assert.True(catalog.IsSuccess, catalog.ErrorMessage);

        // Rank a pair whose native scores are forty-five hundredths apart, beyond the budget reach.
        var ranked = KnnCandidatePrincipleRanker.Rank(
            catalog.Value!,
            new KnnPrincipleQuery("RU", "любой запрос"),
            [
                new KnnNativeCandidate("win", "текст", 0.10),
                new KnnNativeCandidate("lose", "текст", 0.55)
            ]);

        // Ranking a valid pair must succeed.
        Assert.True(ranked.IsSuccess, ranked.ErrorMessage);
        // The native evidence still wins, which is what bounds the influence of the transfer.
        Assert.Equal("lose", ranked.Value![0].ExampleId);
    }

    // The summation order must not depend on the order in which the caller listed the principles.
    [Fact]
    public void Ranker_ProducesTheSameScoresForAnyCallerOrderOfPrinciples()
    {
        // The first test principle, whose identity sorts before the second one.
        var first = new FixedChoicePrinciple("RU", "win", "aa.first");
        // The second test principle, whose identity sorts after the first one.
        var second = new FixedChoicePrinciple("RU", "win", "bb.second");

        // Admit the pair in the natural caller order.
        var straight = KnnCandidatePrincipleCatalog.Create("RU", [first, second]);
        // Admit the same pair in the reversed caller order.
        var reversed = KnnCandidatePrincipleCatalog.Create("RU", [second, first]);
        // The straight catalogue must pass admission.
        Assert.True(straight.IsSuccess, straight.ErrorMessage);
        // The reversed catalogue must pass admission too.
        Assert.True(reversed.IsSuccess, reversed.ErrorMessage);

        // One pair of candidates that is close enough for the budget to matter.
        var candidates = new[]
        {
            // This candidate is the one both principles support.
            new KnnNativeCandidate("win", "текст", 0.42),
            // This candidate is the one both principles argue against.
            new KnnNativeCandidate("lose", "текст", 0.41)
        };

        // Rank with the straight catalogue.
        var a = KnnCandidatePrincipleRanker.Rank(straight.Value!, new KnnPrincipleQuery("RU", "запрос"), candidates);
        // Rank the same candidates with the reversed catalogue.
        var b = KnnCandidatePrincipleRanker.Rank(reversed.Value!, new KnnPrincipleQuery("RU", "запрос"), candidates);
        // The straight run must succeed.
        Assert.True(a.IsSuccess, a.ErrorMessage);
        // The reversed run must succeed.
        Assert.True(b.IsSuccess, b.ErrorMessage);

        // Identities and scores must agree to the last bit, not merely be close.
        Assert.Equal(
            a.Value!.Select(candidate => (candidate.ExampleId, candidate.FinalScore)),
            b.Value!.Select(candidate => (candidate.ExampleId, candidate.FinalScore)));

        // Both catalogues hold the same canonical order, which is what makes the float sum reproducible.
        Assert.Equal("aa.first", straight.Value!.Principles[0].Descriptor.Id);
        // The reversed catalogue was re-sorted into the very same canonical order.
        Assert.Equal("aa.first", reversed.Value!.Principles[0].Descriptor.Id);
        // Two supporting principles share the budget, so the winner gains the full two tenths.
        Assert.Equal(0.62, a.Value![0].FinalScore, 10);
    }

    // An unusable judgement contributes exactly zero and the weights of the others are not renormalised.
    [Fact]
    public void Ranker_TreatsAnInapplicableOrNonFiniteJudgementAsZero()
    {
        // Admit one useful principle next to one that refuses and one that returns a broken number.
        var catalog = KnnCandidatePrincipleCatalog.Create("RU",
        [
            // This principle judges normally and supports the candidate.
            new FixedChoicePrinciple("RU", "win", "aa.useful"),
            // This principle never judges, so it must contribute exactly zero.
            new SilentPrinciple("RU", "bb.inapplicable"),
            // This principle returns a non-finite number, so it must also contribute exactly zero.
            new NonFinitePrinciple("RU", "cc.broken")
        ]);
        // A catalogue of three well formed passports must pass admission.
        Assert.True(catalog.IsSuccess, catalog.ErrorMessage);

        // Rank a single candidate so the arithmetic can be checked directly.
        var ranked = KnnCandidatePrincipleRanker.Rank(
            catalog.Value!,
            new KnnPrincipleQuery("RU", "запрос"),
            [new KnnNativeCandidate("win", "текст", 0.30)]);

        // Ranking a valid candidate must succeed.
        Assert.True(ranked.IsSuccess, ranked.ErrorMessage);

        // One useful judgement out of three principles moves one third of the two tenths budget.
        Assert.Equal(0.30 + (0.20 * (1.0 / 3.0)), ranked.Value![0].FinalScore, 10);
    }

    // Every identity problem has to be refused before any answer is produced.
    [Theory]
    [InlineData("duplicate", "DUPLICATE_PRINCIPLE_ID")]
    [InlineData("version", "UNKNOWN_PRINCIPLE_VERSION")]
    [InlineData("scope", "UNKNOWN_PRINCIPLE_SCOPE")]
    [InlineData("profile", "FOREIGN_PRINCIPLE_PROFILE")]
    [InlineData("negative-weight", "INVALID_PRINCIPLE_WEIGHT")]
    [InlineData("nan-weight", "INVALID_PRINCIPLE_WEIGHT")]
    [InlineData("infinite-weight", "INVALID_PRINCIPLE_WEIGHT")]
    [InlineData("oversized-weight", "INVALID_PRINCIPLE_WEIGHT")]
    public void Catalogue_RefusesEveryIdentityProblem(string defect, string expectedCode)
    {
        // Build exactly one deliberately defective passport per named defect.
        var principles = defect switch
        {
            // Two principles that claim the same identity would double-count the same evidence.
            "duplicate" => new IKnnCandidatePrinciple[] { new FixedChoicePrinciple("RU", "a", "aa.same"), new FixedChoicePrinciple("RU", "b", "aa.same") },
            // A schema version the product never measured must not execute.
            "version" => new IKnnCandidatePrinciple[] { new DefectivePrinciple(new KnnCandidatePrincipleDescriptor("aa.x", "v2", "RU", "candidate-ranking", 1.0)) },
            // A job other than candidate reordering must not hide behind this seam.
            "scope" => new IKnnCandidatePrinciple[] { new DefectivePrinciple(new KnnCandidatePrincipleDescriptor("aa.x", "v1", "RU", "answer-writing", 1.0)) },
            // A principle of a foreign profile must never read the memory of this profile.
            "profile" => new IKnnCandidatePrinciple[] { new DefectivePrinciple(new KnnCandidatePrincipleDescriptor("aa.x", "v1", "HE", "candidate-ranking", 1.0)) },
            // A negative weight would invert the meaning of the budget.
            "negative-weight" => new IKnnCandidatePrinciple[] { new DefectivePrinciple(new KnnCandidatePrincipleDescriptor("aa.x", "v1", "RU", "candidate-ranking", -1.0)) },
            // A weight that is not a number would poison the whole sum.
            "nan-weight" => new IKnnCandidatePrinciple[] { new DefectivePrinciple(new KnnCandidatePrincipleDescriptor("aa.x", "v1", "RU", "candidate-ranking", double.NaN)) },
            // An infinite weight would break the proven bound of plus or minus two tenths.
            "infinite-weight" => new IKnnCandidatePrinciple[] { new DefectivePrinciple(new KnnCandidatePrincipleDescriptor("aa.x", "v1", "RU", "candidate-ranking", double.PositiveInfinity)) },
            // Anything above one would let a single rule exceed its share of the budget.
            _ => new IKnnCandidatePrinciple[] { new DefectivePrinciple(new KnnCandidatePrincipleDescriptor("aa.x", "v1", "RU", "candidate-ranking", 1.5)) }
        };

        // Offer the defective set to the admission gate.
        var catalog = KnnCandidatePrincipleCatalog.Create("RU", principles);

        // The gate must refuse rather than build a catalogue it cannot execute safely.
        Assert.False(catalog.IsSuccess);
        // The refusal must name the exact defect, so a caller can repair the right thing.
        Assert.Equal(expectedCode, catalog.ErrorCode);
    }

    // Контроль 5: чтение паспорта у внешнего правила может упасть, и допуск обязан вернуть
    // конверт отказа, а не выбросить исключение наружу из публичного метода Create.
    [Fact]
    public void Catalogue_RefusesAPrincipleWhosePassportReadThrows()
    {
        // Это правило роняет само чтение свойства Descriptor, как вправе сделать любой внешний автор.
        var principles = new IKnnCandidatePrinciple[] { new ThrowingDescriptorPrinciple() };

        // Допуск вызывается ровно так же, как в обычном продуктовом пути.
        var catalog = KnnCandidatePrincipleCatalog.Create("RU", principles);

        // Правило 1 проекта: публичный метод возвращает отказ вместо исключения.
        Assert.False(catalog.IsSuccess);
        // Машинный код называет именно упавшее чтение паспорта, а не просто неверный паспорт.
        Assert.Equal("PRINCIPLE_DESCRIPTOR_READ_FAILED", catalog.ErrorCode);
        // Каталог не построен, поэтому непроверенное правило не может попасть в ранжирование.
        Assert.Null(catalog.Value);
    }

    // A catalogue of a foreign profile must never be attached to a trained unit.
    [Fact]
    public void Unit_RefusesACatalogueOfAForeignProfile()
    {
        // Build the tiny tokenizer this panel is trained on.
        var tokenizer = BuildTokenizer();
        // Build the tiny transformer that produces the embeddings.
        var model = new LlmModel(BuildTinyConfig());

        // This catalogue is well formed, but it serves the Hebrew profile.
        var catalog = KnnCandidatePrincipleCatalog.Create("HE", [new FixedChoicePrinciple("HE", "a")]);
        // The catalogue itself must be admissible, so the later refusal is about the profile only.
        Assert.True(catalog.IsSuccess, catalog.ErrorMessage);

        // Try to train a Russian unit with that Hebrew catalogue.
        var trained = TransformerEmbeddingKnnLogicalUnit<string>.Train(
            "kdp-foreign", "generated-capability", "RU", model, tokenizer, RussianExamples(), catalog.Value!);

        // Training must be refused before any memory of this profile is exposed.
        Assert.False(trained.IsSuccess);
        // The refusal must name the foreign profile explicitly.
        Assert.Equal("FOREIGN_PRINCIPLE_PROFILE", trained.ErrorCode);
    }

    // A non-finite native score must be refused instead of silently producing a meaningless order.
    [Fact]
    public void Ranker_RefusesANonFiniteNativeScore()
    {
        // One ordinary test principle, so only the broken native score is under test.
        var catalog = KnnCandidatePrincipleCatalog.Create("RU", [new FixedChoicePrinciple("RU", "win")]);
        // A well formed catalogue must pass admission.
        Assert.True(catalog.IsSuccess, catalog.ErrorMessage);

        // Offer a candidate whose native score is not a number.
        var ranked = KnnCandidatePrincipleRanker.Rank(
            catalog.Value!,
            new KnnPrincipleQuery("RU", "запрос"),
            [new KnnNativeCandidate("win", "текст", double.NaN)]);

        // The ranker must refuse rather than return an order nobody can trust.
        Assert.False(ranked.IsSuccess);
        // The refusal must name the broken native score explicitly.
        Assert.Equal("INVALID_NATIVE_SCORE", ranked.ErrorCode);
    }

    // Negative control (a) of repair item R27-K1: the admitted set must never be handed out as a writable array.
    // Before the capture repair the catalogue returned its own array behind IReadOnlyList, so a cast replaced
    // an already admitted rule; copying only the input list did not protect what was handed back out.
    [Fact]
    public void Catalogue_DoesNotHandOutTheInternalPrincipleArray()
    {
        // The caller keeps its own array, so the protection of the input list is checked in the same place.
        var input = new IKnnCandidatePrinciple[] { new FixedChoicePrinciple("RU", "win", "aa.admitted") };
        // Admit exactly that one rule, which supports the identity "win".
        var catalog = KnnCandidatePrincipleCatalog.Create("RU", input);
        // A well formed catalogue must pass admission.
        Assert.True(catalog.IsSuccess, catalog.ErrorMessage);

        // A legitimately admitted entry of another catalogue is the replacement an attacker would try to inject.
        var other = KnnCandidatePrincipleCatalog.Create("RU", [new FixedChoicePrinciple("RU", "lose", "aa.admitted")]);
        // That second catalogue must be admissible, so only the injection attempt is under test.
        Assert.True(other.IsSuccess, other.ErrorMessage);
        // Take its frozen entry, which is the only way a caller can hold a valid replacement object.
        var intruder = other.Value!.Principles[0];

        // Read the collection the catalogue hands out to the outside world.
        var exposed = catalog.Value!.Principles;
        // The backing array itself must not be reachable through a cast.
        Assert.Null(exposed as KnnAdmittedPrinciple[]);
        // A writable list must not be reachable through a cast either.
        Assert.Null(exposed as List<KnnAdmittedPrinciple>);
        // The exposed view does implement the list contract, so the refusal has to come from the view itself.
        var asList = Assert.IsAssignableFrom<IList<KnnAdmittedPrinciple>>(exposed);
        // The view must declare itself read-only rather than quietly accepting writes.
        Assert.True(asList.IsReadOnly);
        // Replacing an element through that cast must be refused instead of swapping the admitted rule.
        Assert.Throws<NotSupportedException>(() => { asList[0] = intruder; });

        // Replacing the element of the caller's own input array must not reach the catalogue either.
        input[0] = new FixedChoicePrinciple("RU", "lose", "aa.admitted");

        // Rank a pair where only the originally admitted rule can make "win" overtake "lose".
        var ranked = KnnCandidatePrincipleRanker.Rank(
            catalog.Value!,
            new KnnPrincipleQuery("RU", "запрос"),
            [
                new KnnNativeCandidate("win", "текст", 0.40),
                new KnnNativeCandidate("lose", "текст", 0.50)
            ]);

        // Ranking must still succeed after both injection attempts.
        Assert.True(ranked.IsSuccess, ranked.ErrorMessage);
        // The originally admitted rule still decides, so the injected one never ran.
        Assert.Equal("win", ranked.Value![0].ExampleId);
        // The arithmetic is the untouched one of the admitted rule: four tenths plus the full two tenths.
        Assert.Equal(0.60, ranked.Value![0].FinalScore, 10);
    }

    // Negative control (b) of repair item R27-K1: ranking must use the passport captured at admission.
    // Before the repair the ranker read the live Descriptor again, so a rule could switch to a foreign
    // profile and an infinite weight after it had already been admitted.
    [Fact]
    public void Catalogue_RanksWithThePassportCapturedAtAdmission_NotWithALaterRewrite()
    {
        // This rule reports a valid Russian passport at admission time and can rewrite it afterwards.
        var shifty = new MutableDescriptorPrinciple("RU", "win", "aa.mutable");
        // Admit it while its passport is still the valid one.
        var catalog = KnnCandidatePrincipleCatalog.Create("RU", [shifty]);
        // Admission must succeed, so only the later rewrite is under test.
        Assert.True(catalog.IsSuccess, catalog.ErrorMessage);

        // After admission the very same object starts reporting a foreign profile and an unbounded weight.
        shifty.Rewrite(new KnnCandidatePrincipleDescriptor("aa.mutable", "v1", "HE", "candidate-ranking", double.PositiveInfinity));

        // The catalogue must still show the frozen profile, not the rewritten one.
        Assert.Equal("RU", catalog.Value!.Principles[0].Descriptor.Profile);
        // The catalogue must still show the frozen weight, not the infinite one.
        Assert.Equal(1.0, catalog.Value!.Principles[0].Descriptor.Weight, 10);

        // Rank a pair that the frozen weight can reverse and the infinite weight would blow up.
        var ranked = KnnCandidatePrincipleRanker.Rank(
            catalog.Value!,
            new KnnPrincipleQuery("RU", "запрос"),
            [
                new KnnNativeCandidate("win", "текст", 0.40),
                new KnnNativeCandidate("lose", "текст", 0.50)
            ]);

        // Ranking must succeed, because the frozen passport is still a valid one.
        Assert.True(ranked.IsSuccess, ranked.ErrorMessage);
        // Index the result by identity so each candidate can be checked on its own.
        var byId = ranked.Value!.ToDictionary(candidate => candidate.ExampleId, StringComparer.Ordinal);
        // The supported candidate moved by exactly the frozen two tenths, not by an infinite amount.
        Assert.Equal(0.60, byId["win"].FinalScore, 10);
        // The opposed candidate lost exactly the frozen two tenths, not an infinite amount.
        Assert.Equal(0.30, byId["lose"].FinalScore, 10);
        // Both numbers stayed finite, which is what the infinite weight was trying to break.
        Assert.True(double.IsFinite(byId["win"].FinalScore) && double.IsFinite(byId["lose"].FinalScore));

        // A fresh admission of the same object now reads the rewritten passport and refuses it.
        var reAdmission = KnnCandidatePrincipleCatalog.Create("RU", [shifty]);
        // The gate must refuse, which proves the rewrite is visible to admission but not to an old catalogue.
        Assert.False(reAdmission.IsSuccess);
        // The refusal must name the foreign profile the rule switched to.
        Assert.Equal("FOREIGN_PRINCIPLE_PROFILE", reAdmission.ErrorCode);
    }

    // Negative control (c) of repair item R27-K1: a built-in rule must copy the caller-owned word tables.
    // Before the repair the rule held the caller arrays, so replacing one word after construction
    // silently changed a model that had already been admitted.
    [Fact]
    public void BuiltInRule_IgnoresALaterRewriteOfTheCallerOwnedMarkerArrays()
    {
        // These four tables stay owned by this test, exactly as a product caller would own them.
        var habitual = new[] { "каждый день" };
        // The table of finished-action markers.
        var past = new[] { "вчера" };
        // The table of planned-action markers.
        var future = new[] { "завтра" };
        // The table of words that cancel a marker.
        var negations = new[] { "не" };
        // Build the real shipped rule class over those caller-owned tables.
        var rule = new KnnTimeFrameCandidatePrinciple(
            "RU", KnnDomainCandidatePrincipleProfiles.RussianPrefixMarker, habitual, past, future, negations);

        // One request whose prefix is in the past.
        var query = new KnnPrincipleQuery("RU", "Задание: продолжи. Префикс: вчера я");
        // One stored example whose prefix is in the same past frame.
        var candidate = new KnnPrincipleCandidate("e1", "Задание: продолжи. Префикс: вчера он");

        // Record the verdict while the tables still hold their original words.
        var before = rule.Evaluate(query, candidate);
        // Both prefixes carry a decidable frame, so the rule must judge.
        Assert.True(before.Applicable);
        // Equal frames support the candidate with the full value.
        Assert.Equal(1.0, before.Value, 10);

        // Now rewrite every table the caller still owns, including turning the past marker into a negation.
        habitual[0] = "вчера";
        // Move the finished-action marker to a word that appears nowhere in the panel.
        past[0] = "позавчера";
        // Point the planned-action marker at the same word as the past one.
        future[0] = "вчера";
        // Make the past marker also count as a cancelling word, which would make the rule refuse to judge.
        negations[0] = "вчера";

        // Ask the very same rule object the very same question again.
        var after = rule.Evaluate(query, candidate);
        // The applicability must be byte-identical to the verdict given before the rewrite.
        Assert.Equal(before.Applicable, after.Applicable);
        // The value must be byte-identical to the verdict given before the rewrite.
        Assert.Equal(before.Value, after.Value, 10);
        // Stated directly as well, so a future change of the "before" branch cannot make this check vacuous.
        Assert.True(after.Applicable);
        // The admitted model still supports the candidate with the full value.
        Assert.Equal(1.0, after.Value, 10);
    }

    // Negative control (d) of repair item R27-K1: a not-a-number verdict contributes zero and the
    // final score stays finite, so no refusal of a rule can smuggle a broken number into the confidence.
    [Fact]
    public void Ranker_TreatsANotANumberJudgementAsZeroAndKeepsTheFinalScoreFinite()
    {
        // One rule judges normally, the other returns a value that is not a number at all.
        var catalog = KnnCandidatePrincipleCatalog.Create("RU",
        [
            // This rule supports the candidate with the full value.
            new FixedChoicePrinciple("RU", "win", "aa.useful"),
            // This rule claims to judge but returns a value no sum can use.
            new NotANumberPrinciple("RU", "bb.nan")
        ]);
        // A catalogue of two well formed passports must pass admission.
        Assert.True(catalog.IsSuccess, catalog.ErrorMessage);

        // Rank a single candidate so the arithmetic can be checked directly.
        var ranked = KnnCandidatePrincipleRanker.Rank(
            catalog.Value!,
            new KnnPrincipleQuery("RU", "запрос"),
            [new KnnNativeCandidate("win", "текст", 0.30)]);

        // Ranking a valid candidate must succeed rather than fail on the broken verdict.
        Assert.True(ranked.IsSuccess, ranked.ErrorMessage);
        // The result must be a usable number.
        Assert.True(double.IsFinite(ranked.Value![0].FinalScore));
        // The broken rule contributed exactly zero, so only half of the two tenths was moved.
        Assert.Equal(0.30 + (0.20 * 0.5), ranked.Value![0].FinalScore, 10);

        // The same neutral-zero rule must hold when a passport weight is rewritten to infinity after admission.
        var shifty = new MutableDescriptorPrinciple("RU", "win", "aa.mutable");
        // Admit the rewritable rule next to the broken one while both passports are still valid.
        var second = KnnCandidatePrincipleCatalog.Create("RU", [shifty, new NotANumberPrinciple("RU", "bb.nan")]);
        // Admission must succeed, so only the later rewrite is under test.
        Assert.True(second.IsSuccess, second.ErrorMessage);
        // Rewrite the live passport weight to infinity, which is the exact bypass the audit found.
        shifty.Rewrite(new KnnCandidatePrincipleDescriptor("aa.mutable", "v1", "RU", "candidate-ranking", double.PositiveInfinity));

        // Rank the same single candidate through the catalogue whose live passport is now broken.
        var guarded = KnnCandidatePrincipleRanker.Rank(
            second.Value!,
            new KnnPrincipleQuery("RU", "запрос"),
            [new KnnNativeCandidate("win", "текст", 0.30)]);

        // Ranking must succeed, because the frozen weight is still the valid one.
        Assert.True(guarded.IsSuccess, guarded.ErrorMessage);
        // The result must still be a usable number rather than an infinity.
        Assert.True(double.IsFinite(guarded.Value![0].FinalScore));
        // The arithmetic is unchanged: one useful rule out of two moves half of the two tenths.
        Assert.Equal(0.40, guarded.Value![0].FinalScore, 10);
    }

    // Supporting control for the explicit callback contract: a crashing rule is an explicit refusal,
    // never a silent zero and never an exception escaping the shared result envelope.
    [Fact]
    public void Ranker_RefusesTheWholeRankingWhenAnAdmittedRuleThrows()
    {
        // One rule that passes admission and then throws while judging.
        var catalog = KnnCandidatePrincipleCatalog.Create("RU", [new ThrowingPrinciple("RU", "aa.throws")]);
        // A well formed passport must pass admission, so only the crash is under test.
        Assert.True(catalog.IsSuccess, catalog.ErrorMessage);

        // Rank a single candidate, which forces the crashing rule to run.
        var ranked = KnnCandidatePrincipleRanker.Rank(
            catalog.Value!,
            new KnnPrincipleQuery("RU", "запрос"),
            [new KnnNativeCandidate("win", "текст", 0.30)]);

        // The ranker must refuse through the shared envelope instead of letting the exception escape.
        Assert.False(ranked.IsSuccess);
        // The refusal must name the failed evaluation explicitly.
        Assert.Equal("PRINCIPLE_EVALUATION_FAILED", ranked.ErrorCode);
    }

    // Positive control of repair item R27-K1: one shared trained unit, the same exam, the stored answers,
    // the empty catalogue and the ordinary consumer all survive every post-admission attack unchanged.
    [Fact]
    public async Task SharedTrainedUnit_KeepsItsStoredAnswersAfterEveryPostAdmissionAttack()
    {
        // Reuse the one trained unit of this test process; no assertion here retrains the model.
        var native = SharedRussianUnit.Value;
        // Collect every answer the trained memory is allowed to return.
        var stored = RussianExamples().Select(example => example.Output).ToHashSet(StringComparer.Ordinal);

        // The empty catalogue is the declared default of the opt-in path.
        var empty = KnnCandidatePrincipleCatalog.CreateEmpty("RU");
        // An empty catalogue must still pass admission.
        Assert.True(empty.IsSuccess, empty.ErrorMessage);
        // Attach it to the same trained memory, so only the ranking policy differs.
        var withEmpty = native.WithCandidatePrinciples(empty.Value!);
        // Attaching an empty catalogue to a matching profile must be accepted.
        Assert.True(withEmpty.IsSuccess, withEmpty.ErrorMessage);

        // These four tables stay owned by this test, exactly as a product caller would own them.
        var habitual = new[] { "каждый день", "ежедневно" };
        // The table of finished-action markers of the measured panel.
        var past = new[] { "вчера", "прошлой ночью" };
        // The table of planned-action markers of the measured panel.
        var future = new[] { "завтра" };
        // The table of words that cancel a marker.
        var negations = new[] { "не" };
        // Build the real shipped rule class over those caller-owned tables, not a test double.
        var rule = new KnnTimeFrameCandidatePrinciple(
            "RU", KnnDomainCandidatePrincipleProfiles.RussianPrefixMarker, habitual, past, future, negations);
        // Admit it for the Russian profile of the trained unit.
        var catalog = KnnCandidatePrincipleCatalog.Create("RU", [rule]);
        // The shipped rule must pass its own admission gate.
        Assert.True(catalog.IsSuccess, catalog.ErrorMessage);
        // Attach the catalogue to the very same trained memory, again without re-embedding anything.
        var ranked = native.WithCandidatePrinciples(catalog.Value!);
        // Attaching a catalogue of the matching profile must be accepted.
        Assert.True(ranked.IsSuccess, ranked.ErrorMessage);

        // Remember what the default path answered before any attack.
        var beforeNative = new List<(string Output, double Confidence)>();
        // Remember what the empty-catalogue path answered before any attack.
        var beforeEmpty = new List<(string Output, double Confidence)>();
        // Remember what the ranked path answered before any attack.
        var beforeRanked = new List<(string Output, double Confidence)>();
        // Ask every exam question once on each of the three paths.
        foreach (var query in RussianQueries())
        {
            // Run the question through the untouched default path.
            var fromNative = await native.ExecuteAsync(Request(query));
            // The default path must answer at all.
            Assert.True(fromNative.IsSuccess, fromNative.ErrorMessage);
            // Run the same question through the empty-catalogue path.
            var fromEmpty = await withEmpty.Value!.ExecuteAsync(Request(query));
            // The empty-catalogue path must answer at all.
            Assert.True(fromEmpty.IsSuccess, fromEmpty.ErrorMessage);
            // Run the same question through the path that carries the admitted rule.
            var fromRanked = await ranked.Value!.ExecuteAsync(Request(query));
            // The ranked path must answer at all.
            Assert.True(fromRanked.IsSuccess, fromRanked.ErrorMessage);

            // An empty catalogue must keep the stored answer of the default path.
            Assert.Equal(fromNative.Value!.Output, fromEmpty.Value!.Output);
            // An empty catalogue must not re-round the confidence of the default path.
            Assert.Equal(fromNative.Value!.Confidence, fromEmpty.Value!.Confidence);
            // An empty catalogue must not add a ranking marker that never ran.
            Assert.Equal(fromNative.Value!.EvidenceIds, fromEmpty.Value!.EvidenceIds);
            // The ranked answer must still come out of the trained memory, never be invented.
            Assert.Contains(fromRanked.Value!.Output, stored);
            // The ranking really ran, so the later comparison cannot pass by the rule being inert.
            Assert.Contains("KDP01:CANDIDATE_PRINCIPLE_RANKING", fromRanked.Value!.EvidenceIds);

            // Store the default answer for the comparison after the attack.
            beforeNative.Add((fromNative.Value!.Output, fromNative.Value!.Confidence));
            // Store the empty-catalogue answer for the comparison after the attack.
            beforeEmpty.Add((fromEmpty.Value!.Output, fromEmpty.Value!.Confidence));
            // Store the ranked answer for the comparison after the attack.
            beforeRanked.Add((fromRanked.Value!.Output, fromRanked.Value!.Confidence));
        }

        // First attack: rewrite every word table the caller still owns.
        habitual[0] = "вчера";
        // Move the finished-action marker to a word that appears nowhere in the panel.
        past[0] = "позавчера";
        // Point the planned-action marker at the same word as the past one.
        future[0] = "вчера";
        // Make a panel word count as a cancelling word, which would make the rule refuse to judge.
        negations[0] = "вчера";

        // Second attack: try to replace the admitted entry inside the collection the catalogue hands out.
        var other = KnnCandidatePrincipleCatalog.Create("RU", [new FixedChoicePrinciple("RU", "S01", "ru.time-frame")]);
        // That replacement catalogue must itself be admissible, so only the injection is under test.
        Assert.True(other.IsSuccess, other.ErrorMessage);
        // Read the exposed collection of the live catalogue.
        var exposed = Assert.IsAssignableFrom<IList<KnnAdmittedPrinciple>>(catalog.Value!.Principles);
        // Replacing an admitted entry must be refused instead of swapping the rule under the trained unit.
        Assert.Throws<NotSupportedException>(() => { exposed[0] = other.Value!.Principles[0]; });

        // Ask the very same exam again on all three paths and compare answer by answer.
        for (var index = 0; index < RussianQueries().Length; index++)
        {
            // Take the question at this position of the exam.
            var query = RussianQueries()[index];
            // Run the question through the untouched default path again.
            var fromNative = await native.ExecuteAsync(Request(query));
            // The default path must still answer at all.
            Assert.True(fromNative.IsSuccess, fromNative.ErrorMessage);
            // Run the question through the empty-catalogue path again.
            var fromEmpty = await withEmpty.Value!.ExecuteAsync(Request(query));
            // The empty-catalogue path must still answer at all.
            Assert.True(fromEmpty.IsSuccess, fromEmpty.ErrorMessage);
            // Run the question through the ranked path again.
            var fromRanked = await ranked.Value!.ExecuteAsync(Request(query));
            // The ranked path must still answer at all.
            Assert.True(fromRanked.IsSuccess, fromRanked.ErrorMessage);

            // The default path is unaffected by anything that happened to the catalogue.
            Assert.Equal(beforeNative[index], (fromNative.Value!.Output, fromNative.Value!.Confidence));
            // The empty-catalogue path keeps the unchanged native behaviour.
            Assert.Equal(beforeEmpty[index], (fromEmpty.Value!.Output, fromEmpty.Value!.Confidence));
            // The ranked path answers exactly as it did before the attack, down to the confidence.
            Assert.Equal(beforeRanked[index], (fromRanked.Value!.Output, fromRanked.Value!.Confidence));
            // The ranked answer still comes out of the trained memory.
            Assert.Contains(fromRanked.Value!.Output, stored);
        }
    }

    // Positive control of repair item R27-K1: a larger set of rules must not open a wider budget.
    [Fact]
    public void GrowingTheSetOfRulesDoesNotCreateAHiddenBudget()
    {
        // Check one, two, three and five supporting rules over the identical candidate pair.
        foreach (var count in new[] { 1, 2, 3, 5 })
        {
            // Build that many rules, all of which support the identity "win".
            var principles = Enumerable.Range(0, count)
                .Select(index => (IKnnCandidatePrinciple)new FixedChoicePrinciple("RU", "win", $"aa.{index:D2}"))
                .ToArray();
            // Admit the whole set for the Russian profile.
            var catalog = KnnCandidatePrincipleCatalog.Create("RU", principles);
            // Every passport is well formed, so admission must succeed.
            Assert.True(catalog.IsSuccess, catalog.ErrorMessage);

            // Rank one supported and one opposed candidate that start from the very same native score.
            var ranked = KnnCandidatePrincipleRanker.Rank(
                catalog.Value!,
                new KnnPrincipleQuery("RU", "запрос"),
                [
                    new KnnNativeCandidate("win", "текст", 0.40),
                    new KnnNativeCandidate("lose", "текст", 0.40)
                ]);

            // Ranking a valid pair must succeed for every set size.
            Assert.True(ranked.IsSuccess, ranked.ErrorMessage);
            // Index the result by identity so each candidate can be checked on its own.
            var byId = ranked.Value!.ToDictionary(candidate => candidate.ExampleId, StringComparer.Ordinal);
            // The supported candidate gains exactly the two tenths, no matter how many rules agree.
            Assert.Equal(0.60, byId["win"].FinalScore, 10);
            // The opposed candidate loses exactly the two tenths, no matter how many rules disagree.
            Assert.Equal(0.20, byId["lose"].FinalScore, 10);
            // The widest reachable gap between two candidates stays at four tenths.
            Assert.Equal(0.40, byId["win"].FinalScore - byId["lose"].FinalScore, 10);
        }
    }

    // Positive control of repair item R27-K1: an exact tie keeps the stable identity order of project 18.
    [Fact]
    public void Ranker_KeepsAStableTieOnTheExampleIdentity()
    {
        // One rule that never judges, so every candidate keeps its native score untouched.
        var catalog = KnnCandidatePrincipleCatalog.Create("RU", [new SilentPrinciple("RU", "aa.silent")]);
        // A well formed passport must pass admission.
        Assert.True(catalog.IsSuccess, catalog.ErrorMessage);

        // Offer three candidates whose native scores are exactly equal, listed out of identity order.
        var ranked = KnnCandidatePrincipleRanker.Rank(
            catalog.Value!,
            new KnnPrincipleQuery("RU", "запрос"),
            [
                new KnnNativeCandidate("b-second", "текст", 0.42),
                new KnnNativeCandidate("c-third", "текст", 0.42),
                new KnnNativeCandidate("a-first", "текст", 0.42)
            ]);

        // Ranking a valid set must succeed.
        Assert.True(ranked.IsSuccess, ranked.ErrorMessage);
        // The tie is resolved by the stored identity, not by the order in which the caller listed them.
        Assert.Equal(
            new[] { "a-first", "b-second", "c-third" },
            ranked.Value!.Select(candidate => candidate.ExampleId));
    }

    // One trained unit for the whole test process, so the ownership controls never retrain the model.
    private static readonly Lazy<TransformerEmbeddingKnnLogicalUnit<string>> SharedRussianUnit = new(() =>
    {
        // Build the tiny tokenizer this panel is trained on.
        var tokenizer = BuildTokenizer();
        // Build the tiny transformer that produces the embeddings.
        var model = new LlmModel(BuildTinyConfig());
        // Train the reference unit on the default path, with no catalogue at all.
        var trained = TransformerEmbeddingKnnLogicalUnit<string>.Train(
            "kdp-shared-native", "generated-capability", "RU", model, tokenizer, RussianExamples());
        // A failed training would make every comparison built on this unit meaningless.
        return trained.IsSuccess
            ? trained.Value!
            : throw new InvalidOperationException($"Shared Russian unit failed to train: {trained.ErrorMessage}");
    });

    // The four Russian rows of the measured panel.
    private static TransformerKnnTrainingExample<string>[] RussianExamples() =>
    [
        // A repeated action in the present.
        new("S01", "Задание: продолжи действие. Префикс: каждый день я", "хожу", [], ["RU"]),
        // A finished action in the past.
        new("S02", "Задание: продолжи действие. Префикс: вчера я", "ходил", [], ["RU"]),
        // A planned action in the future.
        new("S03", "Задание: продолжи действие. Префикс: завтра я", "пойду", [], ["RU"]),
        // A second finished action in the past, phrased with another marker.
        new("S04", "Задание: продолжи действие. Префикс: прошлой ночью я", "спал", [], ["RU"])
    ];

    // The exam questions used to compare the default path with the opt-in path.
    private static string[] RussianQueries() =>
    [
        // Asks for a repeated action in the present.
        "Задание: продолжи действие. Префикс: каждый день он",
        // Asks for a finished action in the past.
        "Задание: продолжи действие. Префикс: вчера он",
        // Asks for a planned action in the future.
        "Задание: продолжи действие. Префикс: завтра он",
        // Asks for a finished action phrased with the second past marker.
        "Задание: продолжи действие. Префикс: прошлой ночью он"
    ];

    // Build the standard input envelope for the Russian panel.
    private static LogicalUnitRequest<string> Request(string input)
        // Every field except the text itself is fixed, so only the question varies between runs.
        => new("run-kdp-transfer", "kdp-unit", "generated-capability", "RU", input, new LogicalExecutionFlags(), new Dictionary<string, object>());

    // Build the byte-pair tokenizer shared by every test in this class.
    private static BpeTokenizer BuildTokenizer()
    {
        // Train a tiny byte-pair tokenizer over the words of this panel only.
        var tokenizer = new BpeTokenizer();
        // Feed it exactly the vocabulary this panel uses, so no unrelated word influences the split.
        tokenizer.Train(
            "Задание продолжи действие Префикс каждый день вчера завтра прошлой ночью я он хожу ходил пойду спал текст запрос",
            320);
        // Hand the trained tokenizer to the caller.
        return tokenizer;
    }

    // Build the transformer configuration shared by every test in this class.
    private static ModelConfig BuildTinyConfig()
    {
        // Keep the encoder tiny so the test proves wiring without stressing the computer.
        return new ModelConfig
        {
            // Room for the byte-pair vocabulary trained above.
            VocabSize = 512,
            // The panel sentences are short, so a small context window is enough.
            ContextLen = 64,
            // A narrow hidden width keeps the embedding cheap.
            HiddenDim = 16,
            // One layer is enough to prove the wiring.
            NumLayers = 1,
            // Four attention heads over the narrow hidden width.
            NumHeads = 4,
            // Two key/value heads, which is the grouped-attention shape used elsewhere.
            NumKvHeads = 2,
            // A small feed-forward width to match the narrow hidden width.
            FfnHiddenDim = 32
        };
    }

    // A test principle that supports exactly one example identity and argues against every other one.
    private sealed class FixedChoicePrinciple : IKnnCandidatePrinciple
    {
        // This field is the one identity this test principle supports.
        private readonly string _preferredExampleId;

        // This constructor builds a valid passport so only the judgement itself is under test.
        public FixedChoicePrinciple(string profile, string preferredExampleId, string id = "aa.fixed-choice")
        {
            // Remember which identity has to win.
            _preferredExampleId = preferredExampleId;
            // Build a passport that passes every admission check.
            Descriptor = new KnnCandidatePrincipleDescriptor(id, "v1", profile, "candidate-ranking", 1.0);
        }

        // This property is the immutable passport of the test principle.
        public KnnCandidatePrincipleDescriptor Descriptor { get; }

        // Judge one candidate by comparing its identity with the preferred identity.
        public KnnPrincipleScore Evaluate(KnnPrincipleQuery query, KnnPrincipleCandidate candidate)
            // Full support for the preferred identity, full opposition for every other one.
            => new(true, string.Equals(candidate.ExampleId, _preferredExampleId, StringComparison.Ordinal) ? 1.0 : -1.0, "fixed test choice");
    }

    // A test principle that never judges, used to prove that a refusal contributes zero.
    private sealed class SilentPrinciple(string profile, string id) : IKnnCandidatePrinciple
    {
        // This passport is valid, so only the refusal to judge is under test.
        public KnnCandidatePrincipleDescriptor Descriptor { get; } = new(id, "v1", profile, "candidate-ranking", 1.0);

        // Refuse to judge any candidate at all.
        public KnnPrincipleScore Evaluate(KnnPrincipleQuery query, KnnPrincipleCandidate candidate)
            // A refusal must be reported as not applicable, never as a neutral judgement.
            => new(false, 0.0, "never judges");
    }

    // A test principle that returns a broken number, used to prove that it cannot poison the order.
    private sealed class NonFinitePrinciple(string profile, string id) : IKnnCandidatePrinciple
    {
        // This passport is valid, so only the broken judgement value is under test.
        public KnnCandidatePrincipleDescriptor Descriptor { get; } = new(id, "v1", profile, "candidate-ranking", 1.0);

        // Claim to judge, but return a value no sum can use.
        public KnnPrincipleScore Evaluate(KnnPrincipleQuery query, KnnPrincipleCandidate candidate)
            // The ranker has to treat this as zero instead of letting it reach the final score.
            => new(true, double.PositiveInfinity, "broken judgement");
    }

    // A test principle whose passport can be rewritten after admission, used to prove the capture repair.
    private sealed class MutableDescriptorPrinciple : IKnnCandidatePrinciple
    {
        // This field is the one identity this test principle supports.
        private readonly string _preferredExampleId;

        // This field is the live passport, which the test can replace at any moment.
        private KnnCandidatePrincipleDescriptor _descriptor;

        // This constructor builds a valid passport, so admission succeeds before the rewrite happens.
        public MutableDescriptorPrinciple(string profile, string preferredExampleId, string id = "aa.mutable")
        {
            // Remember which identity has to win.
            _preferredExampleId = preferredExampleId;
            // Start from a passport that passes every admission check.
            _descriptor = new KnnCandidatePrincipleDescriptor(id, "v1", profile, "candidate-ranking", 1.0);
        }

        // This property hands out whatever passport is current right now, which is the attack vector.
        public KnnCandidatePrincipleDescriptor Descriptor => _descriptor;

        // Replace the live passport, simulating a rule that changes its own identity after admission.
        public void Rewrite(KnnCandidatePrincipleDescriptor replacement)
            // Swap the field so every later read of the property returns the new passport.
            => _descriptor = replacement;

        // Judge one candidate by comparing its identity with the preferred identity.
        public KnnPrincipleScore Evaluate(KnnPrincipleQuery query, KnnPrincipleCandidate candidate)
            // Full support for the preferred identity, full opposition for every other one.
            => new(true, string.Equals(candidate.ExampleId, _preferredExampleId, StringComparison.Ordinal) ? 1.0 : -1.0, "mutable test choice");
    }

    // A test principle that returns a value which is not a number, used to prove the neutral-zero rule.
    private sealed class NotANumberPrinciple(string profile, string id) : IKnnCandidatePrinciple
    {
        // This passport is valid, so only the not-a-number verdict is under test.
        public KnnCandidatePrincipleDescriptor Descriptor { get; } = new(id, "v1", profile, "candidate-ranking", 1.0);

        // Claim to judge, but return a value that no sum and no comparison can use.
        public KnnPrincipleScore Evaluate(KnnPrincipleQuery query, KnnPrincipleCandidate candidate)
            // The ranker has to treat this as zero instead of letting it reach the final score.
            => new(true, double.NaN, "not a number");
    }

    // A test principle that crashes while judging, used to prove the explicit refusal path.
    private sealed class ThrowingPrinciple(string profile, string id) : IKnnCandidatePrinciple
    {
        // This passport is valid, so only the crash during evaluation is under test.
        public KnnCandidatePrincipleDescriptor Descriptor { get; } = new(id, "v1", profile, "candidate-ranking", 1.0);

        // Throw instead of returning a verdict, which an arbitrary external callback is free to do.
        public KnnPrincipleScore Evaluate(KnnPrincipleQuery query, KnnPrincipleCandidate candidate)
            // The ranker must turn this into a named refusal rather than let it escape to the caller.
            => throw new InvalidOperationException("deliberate test crash");
    }

    // Это испытательное правило роняет чтение собственного паспорта: внешнему автору ничто это не запрещает.
    private sealed class ThrowingDescriptorPrinciple : IKnnCandidatePrinciple
    {
        // Чтение паспорта бросает исключение вместо возврата значения.
        public KnnCandidatePrincipleDescriptor Descriptor
            // Допуск обязан превратить это падение в именованный отказ, а не пропустить его наружу.
            => throw new InvalidOperationException("deliberate passport read crash");

        // Это правило никогда не допускается, поэтому его суждение не должно быть достигнуто.
        public KnnPrincipleScore Evaluate(KnnPrincipleQuery query, KnnPrincipleCandidate candidate)
            // Возвращаем отказ, чтобы случайное исполнение стало видимым, а не молчаливым.
            => new(false, 0.0, "not admitted");
    }

    // A test principle that carries a deliberately defective passport.
    private sealed class DefectivePrinciple(KnnCandidatePrincipleDescriptor descriptor) : IKnnCandidatePrinciple
    {
        // The caller supplies the defective passport, so each admission check can be tested on its own.
        public KnnCandidatePrincipleDescriptor Descriptor { get; } = descriptor;

        // This principle is never admitted, so its judgement must never be reached.
        public KnnPrincipleScore Evaluate(KnnPrincipleQuery query, KnnPrincipleCandidate candidate)
            // Return a refusal, which makes an accidental execution visible instead of silent.
            => new(false, 0.0, "not admitted");
    }
}
