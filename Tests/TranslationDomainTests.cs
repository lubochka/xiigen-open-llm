// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure.Logical;
using TranslationDomain;

public sealed class TranslationDomainTests
{
    [Fact]
    public void TranslationMachine_ImplementsILearningMachineOfTranslationDomain()
    {
        var machine = new TranslationMachine();
        ILearningMachine<TranslationDomain> contract = machine;
        Assert.NotNull(contract);
    }

    [Fact]
    public void TranslationRoutingFlow_FlowId_IsTranslationRoutingFlow()
    {
        Assert.Equal("translation-routing-flow", TranslationRoutingFlow.FlowId);
    }

    [Fact]
    public async Task TrainBatchAsync_Returns_AccuracyAfterGreaterThanZero()
    {
        var machine = new TranslationMachine();
        var result = await machine.TrainBatchAsync(BuildTrainingBatch("batch-001", 50));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.True(result.Value!.AccuracyAfter > 0.0);
        Assert.True(result.Value.AccuracyAfter >= result.Value.AccuracyBefore);
        Assert.Equal(50, result.Value.RowsTrained);
    }

    [Fact]
    public async Task TrainBatchAsync_LegacyPath_ReturnsCompatibilityZeroMargin()
    {
        var machine = new TranslationMachine();

        var result = await machine.TrainBatchAsync(BuildTrainingBatch("r19-legacy-margin", 1));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(0.0, result.Value!.RewardMargin);
    }

    [Fact]
    public async Task TrainStructuredBatchAsync_WrongAnnotations_DoNotCreateRewardMargin()
    {
        var machine = new TranslationMachine();
        var example = BuildExecutorExample(
            "r19-structured-margin",
            "source command",
            "R19|SOURCE|COMMAND",
            "target command",
            "wrong target",
            "wrong_target");
        Assert.Contains("[WRONG:", example.RejectedOutputJson, StringComparison.Ordinal);

        var result = await machine.TrainStructuredBatchAsync(new TranslationLearningBatch(
            BatchId: "r19-structured-margin",
            DomainTag: TranslationMachine.DomainTag,
            Examples: [example],
            SourceIds: ["r19-test"]));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, result.Value!.RowsTrained);
        Assert.True(result.Value.AccuracyAfter > 0.0);
        Assert.Contains(TranslationUnitIds.ExecutorRuEn, result.Value.PromotedUnitIds);
        Assert.Equal(0.0, result.Value.RewardMargin);
    }

    [Fact]
    public async Task EvaluateAsync_HeldOutAccuracyPassesDomainGateWithoutRewardMargin()
    {
        var machine = new TranslationMachine();
        var source = "r19 evaluation source";
        var expected = "r19 evaluation target";
        var train = await machine.TrainStructuredBatchAsync(new TranslationLearningBatch(
            BatchId: "r19-evaluation-margin",
            DomainTag: TranslationMachine.DomainTag,
            Examples:
            [
                BuildExecutorExample(
                    "r19-evaluation-margin",
                    source,
                    "R19|EVALUATION|SOURCE",
                    expected,
                    "wrong target",
                    "wrong_target")
            ],
            SourceIds: ["r19-test"]));
        Assert.True(train.IsSuccess, train.ErrorMessage);

        var result = await machine.EvaluateAsync(new LearningEvaluationSet<TranslationDomain>(
            EvaluationId: "r19-evaluation-margin",
            DomainTag: TranslationMachine.DomainTag,
            EvaluationItems: [new LearningEvaluationItem("r19-row", source, expected)]));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1.0, result.Value!.Accuracy);
        Assert.Equal(0.0, result.Value.RewardMargin);
        Assert.True(result.Value.PassesGate);
        Assert.StartsWith("REWARD_MARGIN_UNAVAILABLE", result.Value.GateNote, StringComparison.Ordinal);
        Assert.Contains("held_out_accuracy_measured=true", result.Value.GateNote, StringComparison.Ordinal);
    }

    [Fact]
    public async Task EvaluateAsync_HeldOutAccuracyMissFailsDomainGateWithoutRewardMargin()
    {
        var machine = new TranslationMachine();
        const string trainedSource = "r19 evaluation trained source";
        const string trainedTarget = "r19 evaluation trained target";
        var train = await machine.TrainStructuredBatchAsync(new TranslationLearningBatch(
            BatchId: "r19-evaluation-accuracy-miss",
            DomainTag: TranslationMachine.DomainTag,
            Examples:
            [
                BuildExecutorExample(
                    "r19-evaluation-accuracy-miss",
                    trainedSource,
                    "R19|EVALUATION|TRAINED",
                    trainedTarget,
                    "wrong target",
                    "wrong_target")
            ],
            SourceIds: ["r19-test"]));
        Assert.True(train.IsSuccess, train.ErrorMessage);

        var result = await machine.EvaluateAsync(new LearningEvaluationSet<TranslationDomain>(
            EvaluationId: "r19-evaluation-accuracy-miss",
            DomainTag: TranslationMachine.DomainTag,
            EvaluationItems:
            [
                new LearningEvaluationItem("trained", trainedSource, trainedTarget),
                new LearningEvaluationItem("miss", "untrained source", "unreachable expected target")
            ]));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(0.5, result.Value!.Accuracy);
        Assert.Equal(0.0, result.Value.RewardMargin);
        Assert.False(result.Value.PassesGate);
        Assert.StartsWith("REWARD_MARGIN_UNAVAILABLE", result.Value.GateNote, StringComparison.Ordinal);
        Assert.Contains("held_out_accuracy_measured=true", result.Value.GateNote, StringComparison.Ordinal);
    }

    [Fact]
    public async Task EvaluateAsync_LegacyScaffoldCannotPassHeldOutGate()
    {
        var machine = new TranslationMachine();
        var train = await machine.TrainBatchAsync(BuildTrainingBatch("legacy-unmeasured-evaluation", 1));
        Assert.True(train.IsSuccess, train.ErrorMessage);
        Assert.True(train.Value!.AccuracyAfter > 0.0);

        var result = await machine.EvaluateAsync(new LearningEvaluationSet<TranslationDomain>(
            EvaluationId: "legacy-unmeasured-evaluation",
            DomainTag: TranslationMachine.DomainTag,
            EvaluationItems:
            [
                new LearningEvaluationItem(
                    "deliberate-miss",
                    "legacy scaffold has no held-out execution path",
                    "this deliberately wrong expectation must not pass")
            ]));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(0.0, result.Value!.Accuracy);
        Assert.Equal(0.0, result.Value.RewardMargin);
        Assert.False(result.Value.PassesGate);
        Assert.StartsWith("REWARD_MARGIN_UNAVAILABLE", result.Value.GateNote, StringComparison.Ordinal);
        Assert.Contains("legacy scaffold", result.Value.GateNote, StringComparison.Ordinal);
        Assert.Contains("held_out_accuracy_measured=false", result.Value.GateNote, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("ru", "en", "RU_EN")]
    [InlineData("en", "ru", "EN_RU")]
    [InlineData("he", "en", "HE_EN")]
    [InlineData("en", "he", "EN_HE")]
    public async Task TranslationRoutingFlow_RoutesSupportedDirections(
        string sourceLang,
        string targetLang,
        string expectedDirection)
    {
        var flow = new TranslationRoutingFlow();
        var result = await flow.ExecuteAsync(BuildFlowRequest("route-test", sourceLang, targetLang, "sample text"));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(expectedDirection, result.Value!.Metrics["direction"]);
    }

    [Fact]
    public async Task TranslationRoutingFlow_UnsupportedDirection_ReturnsFailure()
    {
        var flow = new TranslationRoutingFlow();
        var result = await flow.ExecuteAsync(BuildFlowRequest("unsupported-test", "fr", "de", "Bonjour monde"));

        Assert.False(result.IsSuccess);
        Assert.Equal("UNSUPPORTED_DIRECTION", result.ErrorCode);
    }

    [Fact]
    public async Task BootstrapAdapter_TranslateAsync_ReturnsStructuredResult()
    {
        var adapter = new BootstrapTranslationAdapter(TranslationDirection.RU_EN);
        var result = await adapter.TranslateAsync(new TranslationRoutingRequest("ru", "en", "general", "Privet"));

        Assert.True(result.IsSuccess);
        Assert.Equal(TranslationDirection.RU_EN, result.Value!.Direction);
        Assert.Equal("bootstrap-ru_en", result.Value.AdapterId);
        Assert.Contains("[RU_EN:BOOTSTRAP]", result.Value.TranslatedText);
        Assert.Equal(0.5, result.Value.Confidence);
    }

    [Fact]
    public async Task TranslationMachine_ExportImportCycle_Succeeds()
    {
        var machine = new TranslationMachine();
        await machine.TrainBatchAsync(BuildTrainingBatch("batch-export-001", 10));

        var exportResult = await machine.ExportAsync();
        Assert.True(exportResult.IsSuccess, exportResult.ErrorMessage);
        var export = exportResult.Value!;

        Assert.Equal(TranslationMachine.MachineId, export.MachineId);
        Assert.Equal(TranslationMachine.DomainTag, export.DomainTag);
        Assert.True(export.AccuracyAtExport > 0.0);

        var freshMachine = new TranslationMachine();
        var importResult = await freshMachine.ImportAsync(export);

        Assert.True(importResult.IsSuccess, importResult.ErrorMessage);
        Assert.True(importResult.Value);
    }

    [Fact]
    public async Task TranslationMachine_GetOrchestratorAsync_ReturnsOrchestrator()
    {
        var machine = new TranslationMachine();
        var orchestratorResult = await machine.GetOrchestratorAsync();

        Assert.True(orchestratorResult.IsSuccess, orchestratorResult.ErrorMessage);
        Assert.NotNull(orchestratorResult.Value);
    }

    [Fact]
    public async Task TrainStructuredBatchAsync_PerUnitPairs_ChangeRuntimeOutputForThreeSets()
    {
        var machine = new TranslationMachine();

        var chatSource = "Брат, я уже третий день мучаюсь с этим роутером 😅";
        var technicalSource = "добавь проверку null в метод ValidateToken и не переименовывай ProcessQueue";
        var businessSource = "Сегодня я закрыла сделку на 150к$ без инвесторов 🔥 Клиент в восторге.";

        var before = await RunTranslationAsync(machine, chatSource);
        Assert.Contains("[RU_EN:BOOTSTRAP]", before);

        var examples = new[]
        {
            BuildSemanticExample("chat-semantic", chatSource, "CHAT|FRIENDLY_ADDRESS|STRUGGLE|ROUTER|EMOJI"),
            BuildExecutorExample(
                "chat-executor",
                chatSource,
                "CHAT|FRIENDLY_ADDRESS|STRUGGLE|ROUTER|EMOJI",
                "Bro, I've been stuck with this router for three days now 😅",
                "Brother, I am suffering with this router for the third day 😅",
                "unnatural_chat_register"),
            BuildVerifierExample("chat-verifier", chatSource, "Bro, I've been stuck with this router for three days now 😅"),

            BuildSemanticExample("technical-semantic", technicalSource, "TECH|ADD_NULL_CHECK|METHOD|NEGATION|CODE_IDS"),
            BuildExecutorExample(
                "technical-executor",
                technicalSource,
                "TECH|ADD_NULL_CHECK|METHOD|NEGATION|CODE_IDS",
                "Add a null check to the ValidateToken method and do not rename ProcessQueue.",
                "Add checking null to the ValidateToken method and do not rename process queue.",
                "term_error_and_entity_loss"),
            BuildVerifierFailureExample(
                "technical-verifier",
                technicalSource,
                "TECH|ADD_NULL_CHECK|METHOD|NEGATION|CODE_IDS",
                "Add checking null to the ValidateToken method and do not rename process queue."),

            BuildSemanticExample("business-semantic", businessSource, "SOCIAL|CLOSED_DEAL|MONEY|BOOTSTRAPPED|CLIENT_THRILLED|EMOJI"),
            BuildExecutorExample(
                "business-executor",
                businessSource,
                "SOCIAL|CLOSED_DEAL|MONEY|BOOTSTRAPPED|CLIENT_THRILLED|EMOJI",
                "Today I closed a $150k deal without investors 🔥 The client is thrilled.",
                "Today I closed the transaction for 150 dollars without investors. The client is in delight.",
                "entity_loss_and_unnatural_business_english"),
            BuildVerifierExample(
                "business-verifier",
                businessSource,
                "Today I closed a $150k deal without investors 🔥 The client is thrilled.")
        };

        var trainResult = await machine.TrainStructuredBatchAsync(new TranslationLearningBatch(
            BatchId: "translation-structured-unit-proof-001",
            DomainTag: TranslationMachine.DomainTag,
            Examples: examples,
            SourceIds: ["unit-proof"]));

        Assert.True(trainResult.IsSuccess, trainResult.ErrorMessage);
        Assert.Equal(9, trainResult.Value!.RowsTrained);
        Assert.Equal(1.0, trainResult.Value.AccuracyAfter);
        Assert.Contains(TranslationUnitIds.SemanticTokenizer, trainResult.Value.PromotedUnitIds);
        Assert.Contains(TranslationUnitIds.ExecutorRuEn, trainResult.Value.PromotedUnitIds);
        Assert.Contains(TranslationUnitIds.QualityVerifier, trainResult.Value.PromotedUnitIds);

        var chat = await RunTranslationAsync(machine, chatSource);
        var technical = await RunTranslationAsync(machine, technicalSource);
        var business = await RunTranslationAsync(machine, businessSource);

        Assert.Equal("Bro, I've been stuck with this router for three days now 😅", chat);
        Assert.Equal("Add a null check to the ValidateToken method and do not rename ProcessQueue.", technical);
        Assert.Equal("Today I closed a $150k deal without investors 🔥 The client is thrilled.", business);

        var eval = await machine.EvaluateAsync(new LearningEvaluationSet<TranslationDomain>(
            EvaluationId: "translation-structured-unit-proof-eval",
            DomainTag: TranslationMachine.DomainTag,
            EvaluationItems:
            [
                new LearningEvaluationItem("chat", chatSource, "Bro, I've been stuck with this router for three days now 😅"),
                new LearningEvaluationItem("technical", technicalSource, "Add a null check to the ValidateToken method and do not rename ProcessQueue."),
                new LearningEvaluationItem("business", businessSource, "Today I closed a $150k deal without investors 🔥 The client is thrilled.")
            ]));

        Assert.True(eval.IsSuccess, eval.ErrorMessage);
        Assert.True(eval.Value!.PassesGate);
        Assert.StartsWith("REWARD_MARGIN_UNAVAILABLE", eval.Value.GateNote, StringComparison.Ordinal);
        Assert.Equal(1.0, eval.Value.Accuracy);
    }

    [Fact]
    public void TranslationStructuredBatchBuilder_BuildsThreeUnitExamplesPerTrainReadyPair()
    {
        var pair = BuildTrainReadyPairSample();
        var batch = TranslationStructuredBatchBuilder.BuildRuEnGeneralBatch(
            "ru-en-general-l1-b029-structured-sample",
            [pair]);

        Assert.Single(batch.SourceIds);
        Assert.Equal(3, batch.Examples.Count);
        Assert.Contains(batch.Examples, example => example.UnitId == TranslationUnitIds.SemanticTokenizer);
        Assert.Contains(batch.Examples, example => example.UnitId == TranslationUnitIds.ExecutorRuEn);
        Assert.Contains(batch.Examples, example => example.UnitId == TranslationUnitIds.QualityVerifier);
        Assert.All(batch.Examples, example => Assert.Contains("[WRONG:", example.RejectedOutputJson));

        var executor = batch.Examples.Single(example => example.UnitId == TranslationUnitIds.ExecutorRuEn);
        var chosen = JsonSerializer.Deserialize<ExecutorOutput>(executor.ChosenOutputJson, JsonOptions);
        var rejected = JsonSerializer.Deserialize<ExecutorOutput>(executor.RejectedOutputJson, JsonOptions);

        Assert.Equal("He intrepidly advanced through the hostile crowd, betraying not a trace of fear!", chosen!.Text);
        Assert.NotNull(chosen.WordMappings);
        Assert.NotEmpty(chosen.WordMappings!);
        Assert.DoesNotContain("intrepidly", rejected!.Text, StringComparison.OrdinalIgnoreCase);
        // Происхождение урока по-прежнему названо полностью, но живёт отдельным полем строки, а
        // не внутри рабочего ключа поиска: ключ обязан зависеть только от смысла и структуры
        // текста. Смысл этого утверждения не изменился — изменилось только место, где
        // происхождение хранится (ремонт «единый ключ урока», пункт M03b адресного плана).
        Assert.Equal("ru-en-general-l1-b029-001", executor.PairId);
        Assert.DoesNotContain("PAIR:", executor.SemanticSignature);
    }

    [Fact]
    public async Task TrainStructuredBatchAsync_WordMapFallback_UsesLearnedMappingsWithoutExactKnn()
    {
        var machine = new TranslationMachine();
        var source = "добавь проверку null в метод ValidateToken";

        var batch = new TranslationLearningBatch(
            BatchId: "translation-wordmap-fallback-proof-001",
            DomainTag: TranslationMachine.DomainTag,
            Examples:
            [
                BuildExecutorExampleWithMappings(
                    "wordmap-executor",
                    source,
                    "TRAIN_ONLY_SIGNATURE_WITH_NO_RUNTIME_OVERLAP",
                    "Add a null check to the ValidateToken method",
                    "literal_word_order_no_article",
                    [
                        new ExecutorWordMapping("добавь", SemanticConcept.ADD_OP, "Add"),
                        new ExecutorWordMapping("добавь", SemanticConcept.UNKNOWN, "Add"),
                        new ExecutorWordMapping("проверку", SemanticConcept.VALIDATE_CHECK, "check"),
                        new ExecutorWordMapping("проверку", SemanticConcept.UNKNOWN, "check"),
                        new ExecutorWordMapping("null", SemanticConcept.NULL_LITERAL, "null"),
                        new ExecutorWordMapping("метод", SemanticConcept.METHOD_REF, "method")
                    ])
            ],
            SourceIds: ["wordmap-fallback-proof"]);

        var trainResult = await machine.TrainStructuredBatchAsync(batch);
        Assert.True(trainResult.IsSuccess, trainResult.ErrorMessage);

        var actual = await RunTranslationAsync(machine, source);

        Assert.DoesNotContain("[RU_EN:BOOTSTRAP]", actual);
        Assert.Contains("Add", actual);
        Assert.Contains("null check", actual);
        Assert.Contains("ValidateToken", actual);
    }

    [Fact]
    public async Task ExecutorUnit_EnRuWordMap_ComposesCyrillicTranslationWithoutBootstrap()
    {
        var machine = new TranslationMachine();
        var source = "good morning friend";
        var batch = new TranslationLearningBatch(
            BatchId: "en-ru-cyrillic-wordmap-proof",
            DomainTag: TranslationMachine.DomainTag,
            Examples:
            [
                BuildDirectionalExecutorExampleWithMappings(
                    "en-ru-cyrillic-wordmap",
                    TranslationDirection.EN_RU,
                    "en",
                    source,
                    "SVO|EN_RU_WORDMAP",
                    "доброе утро друг",
                    "latin_identity_output",
                    [
                        new ExecutorWordMapping("good", SemanticConcept.UNKNOWN, "доброе"),
                        new ExecutorWordMapping("morning", SemanticConcept.UNKNOWN, "утро"),
                        new ExecutorWordMapping("friend", SemanticConcept.UNKNOWN, "друг")
                    ])
            ],
            SourceIds: ["en-ru-cyrillic-wordmap-proof"]);

        var trainResult = await machine.TrainStructuredBatchAsync(batch);
        Assert.True(trainResult.IsSuccess, trainResult.ErrorMessage);

        var actual = await RunTranslationAsync(machine, source, "en", "ru");

        Assert.DoesNotContain("[EN_RU:BOOTSTRAP]", actual);
        Assert.Equal("доброе утро друг", actual);
    }

    [Fact]
    public async Task TranslationMachine_LoadCheckpoint_PrunesPollutedEnRuWordAndSurfaceMaps()
    {
        var tmpPath = Path.GetTempFileName() + ".json";
        try
        {
            var checkpoint = BuildCheckpointWithExecutorMaps(
                TranslationDirection.EN_RU,
                [
                    new ExecutorWordMapEntryCheckpoint("able", SemanticConcept.UNKNOWN, "не"),
                    new ExecutorWordMapEntryCheckpoint("boston", SemanticConcept.UNKNOWN, "том"),
                    new ExecutorWordMapEntryCheckpoint("accept", SemanticConcept.UNKNOWN, "Пожалуйста"),
                    new ExecutorWordMapEntryCheckpoint("the", SemanticConcept.UNKNOWN, "\u043c\u043e\u0435\u0439"),
                    new ExecutorWordMapEntryCheckpoint("a", SemanticConcept.UNKNOWN, "\u0417\u0434\u0435\u0441\u044c"),
                    new ExecutorWordMapEntryCheckpoint("be", SemanticConcept.UNKNOWN, "\u043d\u0435"),
                    new ExecutorWordMapEntryCheckpoint("whole", SemanticConcept.UNKNOWN, "\u043c\u043e\u0435\u0439"),
                    new ExecutorWordMapEntryCheckpoint("only", SemanticConcept.UNKNOWN, "\u0422\u043e\u043b\u044c\u043a\u043e"),
                    new ExecutorWordMapEntryCheckpoint("fire", SemanticConcept.UNKNOWN, "\u043e\u0433\u043e\u043d\u044c"),
                    new ExecutorWordMapEntryCheckpoint("city", SemanticConcept.UNKNOWN, "city"),
                    new ExecutorWordMapEntryCheckpoint("mixed", SemanticConcept.UNKNOWN, "helloПривет"),
                    new ExecutorWordMapEntryCheckpoint("not", SemanticConcept.UNKNOWN, "не"),
                    new ExecutorWordMapEntryCheckpoint("not", SemanticConcept.CODE_IDENTIFIER, "\u044f"),
                    new ExecutorWordMapEntryCheckpoint("no", SemanticConcept.UNKNOWN, "не"),
                    new ExecutorWordMapEntryCheckpoint("no", SemanticConcept.CODE_IDENTIFIER, "\u041d\u0435\u0442"),
                    new ExecutorWordMapEntryCheckpoint("cannot", SemanticConcept.UNKNOWN, "не"),
                    new ExecutorWordMapEntryCheckpoint("tom", SemanticConcept.UNKNOWN, "Том"),
                    new ExecutorWordMapEntryCheckpoint("door", SemanticConcept.UNKNOWN, "дверь"),
                    new ExecutorWordMapEntryCheckpoint("good", SemanticConcept.UNKNOWN, "доброе"),
                    new ExecutorWordMapEntryCheckpoint("morning", SemanticConcept.UNKNOWN, "утро"),
                    new ExecutorWordMapEntryCheckpoint("friend", SemanticConcept.UNKNOWN, "друг")
                ],
                [
                    new ExecutorStringMapEntryCheckpoint("able", "не"),
                    new ExecutorStringMapEntryCheckpoint("boston", "том"),
                    new ExecutorStringMapEntryCheckpoint("accept", "Пожалуйста"),
                    new ExecutorStringMapEntryCheckpoint("the", "\u043c\u043e\u0435\u0439"),
                    new ExecutorStringMapEntryCheckpoint("a", "\u0417\u0434\u0435\u0441\u044c"),
                    new ExecutorStringMapEntryCheckpoint("be", "\u043d\u0435"),
                    new ExecutorStringMapEntryCheckpoint("whole", "\u043c\u043e\u0435\u0439"),
                    new ExecutorStringMapEntryCheckpoint("only", "\u0422\u043e\u043b\u044c\u043a\u043e"),
                    new ExecutorStringMapEntryCheckpoint("fire", "\u043e\u0433\u043e\u043d\u044c"),
                    new ExecutorStringMapEntryCheckpoint("city", "city"),
                    new ExecutorStringMapEntryCheckpoint("mixed", "helloПривет"),
                    new ExecutorStringMapEntryCheckpoint("not", "не"),
                    new ExecutorStringMapEntryCheckpoint("not", "\u044f"),
                    new ExecutorStringMapEntryCheckpoint("no", "не"),
                    new ExecutorStringMapEntryCheckpoint("no", "\u041d\u0435\u0442"),
                    new ExecutorStringMapEntryCheckpoint("cannot", "не"),
                    new ExecutorStringMapEntryCheckpoint("tom", "Том"),
                    new ExecutorStringMapEntryCheckpoint("door", "дверь"),
                    new ExecutorStringMapEntryCheckpoint("good", "доброе"),
                    new ExecutorStringMapEntryCheckpoint("morning", "утро"),
                    new ExecutorStringMapEntryCheckpoint("friend", "друг")
                ],
                [new ExecutorStringMapEntryCheckpoint("good morning", "доброе утро")],
                // Исходный запрос этого набора данных действительно общий: ниже он
                // переводится без названной темы. Поэтому точная память задаётся
                // явной темой «general», а эталон перевода остался прежним.
                [new ExecutorTopicSourceMapEntryCheckpoint("general", "exact hello", "точный привет")]);
            await File.WriteAllTextAsync(
                tmpPath,
                JsonSerializer.Serialize(checkpoint, TranslationCheckpointJsonOptions.Standard),
                System.Text.Encoding.UTF8);

            var loadResult = await TranslationMachine.LoadCheckpointAsync(tmpPath);
            Assert.True(loadResult.IsSuccess, loadResult.ErrorMessage);

            var exported = loadResult.Value!.Machine.ExportCheckpoint();
            Assert.True(exported.IsSuccess, exported.ErrorMessage);
            var enRuExecutor = exported.Value!.Executors.Single(e => e.Direction == TranslationDirection.EN_RU);

            Assert.DoesNotContain(enRuExecutor.WordMap, entry => entry.Lemma == "able" && entry.Target == "не");
            Assert.DoesNotContain(enRuExecutor.WordMap, entry => entry.Lemma == "boston" && entry.Target == "том");
            Assert.DoesNotContain(enRuExecutor.WordMap, entry => entry.Lemma == "accept" && entry.Target == "Пожалуйста");
            Assert.DoesNotContain(enRuExecutor.WordMap, entry => entry.Lemma == "the" && entry.Target == "\u043c\u043e\u0435\u0439");
            Assert.DoesNotContain(enRuExecutor.WordMap, entry => entry.Lemma == "a" && entry.Target == "\u0417\u0434\u0435\u0441\u044c");
            Assert.DoesNotContain(enRuExecutor.WordMap, entry => entry.Lemma == "be" && entry.Target == "\u043d\u0435");
            Assert.DoesNotContain(enRuExecutor.WordMap, entry => entry.Lemma == "whole" && entry.Target == "\u043c\u043e\u0435\u0439");
            Assert.DoesNotContain(enRuExecutor.WordMap, entry => entry.Lemma == "only" && entry.Target == "\u0422\u043e\u043b\u044c\u043a\u043e");
            Assert.DoesNotContain(enRuExecutor.WordMap, entry => entry.Lemma == "not" && entry.Target == "\u044f");
            Assert.DoesNotContain(enRuExecutor.WordMap, entry => entry.Lemma == "no" && entry.Target == "\u041d\u0435\u0442");
            Assert.DoesNotContain(enRuExecutor.WordMap, entry => entry.Lemma == "city" && entry.Target == "city");
            Assert.DoesNotContain(enRuExecutor.WordMap, entry => entry.Lemma == "mixed" && entry.Target == "helloПривет");
            Assert.DoesNotContain(enRuExecutor.SurfaceWordMap, entry => entry.Key == "able" && entry.Value == "не");
            Assert.DoesNotContain(enRuExecutor.SurfaceWordMap, entry => entry.Key == "boston" && entry.Value == "том");
            Assert.DoesNotContain(enRuExecutor.SurfaceWordMap, entry => entry.Key == "accept" && entry.Value == "Пожалуйста");
            Assert.DoesNotContain(enRuExecutor.SurfaceWordMap, entry => entry.Key == "the" && entry.Value == "\u043c\u043e\u0435\u0439");
            Assert.DoesNotContain(enRuExecutor.SurfaceWordMap, entry => entry.Key == "a" && entry.Value == "\u0417\u0434\u0435\u0441\u044c");
            Assert.DoesNotContain(enRuExecutor.SurfaceWordMap, entry => entry.Key == "be" && entry.Value == "\u043d\u0435");
            Assert.DoesNotContain(enRuExecutor.SurfaceWordMap, entry => entry.Key == "whole" && entry.Value == "\u043c\u043e\u0435\u0439");
            Assert.DoesNotContain(enRuExecutor.SurfaceWordMap, entry => entry.Key == "only" && entry.Value == "\u0422\u043e\u043b\u044c\u043a\u043e");
            Assert.DoesNotContain(enRuExecutor.SurfaceWordMap, entry => entry.Key == "not" && entry.Value == "\u044f");
            Assert.DoesNotContain(enRuExecutor.SurfaceWordMap, entry => entry.Key == "no" && entry.Value == "\u041d\u0435\u0442");

            Assert.Contains(enRuExecutor.WordMap, entry => entry.Lemma == "not" && entry.Target == "не");
            Assert.Contains(enRuExecutor.WordMap, entry => entry.Lemma == "no" && entry.Target == "не");
            Assert.Contains(enRuExecutor.WordMap, entry => entry.Lemma == "cannot" && entry.Target == "не");
            Assert.Contains(enRuExecutor.WordMap, entry => entry.Lemma == "tom" && entry.Target == "Том");
            Assert.Contains(enRuExecutor.WordMap, entry => entry.Lemma == "door" && entry.Target == "дверь");
            Assert.Contains(enRuExecutor.WordMap, entry => entry.Lemma == "fire" && entry.Target == "\u043e\u0433\u043e\u043d\u044c");
            Assert.Contains(enRuExecutor.WordMap, entry => entry.Lemma == "good" && entry.Target == "доброе");
            Assert.Contains(enRuExecutor.SurfaceWordMap, entry => entry.Key == "not" && entry.Value == "не");
            Assert.Contains(enRuExecutor.SurfaceWordMap, entry => entry.Key == "no" && entry.Value == "не");
            Assert.Contains(enRuExecutor.SurfaceWordMap, entry => entry.Key == "cannot" && entry.Value == "не");
            Assert.Contains(enRuExecutor.SurfaceWordMap, entry => entry.Key == "tom" && entry.Value == "Том");
            Assert.Contains(enRuExecutor.SurfaceWordMap, entry => entry.Key == "door" && entry.Value == "дверь");
            Assert.Contains(enRuExecutor.SurfaceWordMap, entry => entry.Key == "fire" && entry.Value == "\u043e\u0433\u043e\u043d\u044c");
            Assert.Contains(enRuExecutor.SurfaceWordMap, entry => entry.Key == "good" && entry.Value == "доброе");
            Assert.Contains(enRuExecutor.PhraseMap, entry => entry.Key == "good morning" && entry.Value == "доброе утро");
            // Точная память читается по текущему адресу «тема и источник»; эталон
            // перевода прежний, изменился только адрес ячейки.
            Assert.Contains(
                enRuExecutor.TopicSourceMap ?? [],
                entry => entry.TopicDomain == "general" &&
                         entry.Source == "exact hello" &&
                         entry.Target == "точный привет");

            var polluted = await RunTranslationAsync(loadResult.Value!.Machine, "able boston accept", "en", "ru");
            Assert.Contains("[EN_RU:BOOTSTRAP]", polluted);

            var valid = await RunTranslationAsync(loadResult.Value!.Machine, "good morning friend", "en", "ru");
            Assert.Equal("доброе утро друг", valid);
        }
        finally
        {
            if (File.Exists(tmpPath)) File.Delete(tmpPath);
        }
    }

    [Fact]
    public async Task ExecutorUnit_EnRuPhraseFirstComposition_DoesNotEmitFunctionWordJunk()
    {
        var machine = new TranslationMachine();
        var batch = new TranslationLearningBatch(
            BatchId: "en-ru-phrase-first-function-junk-proof",
            DomainTag: TranslationMachine.DomainTag,
            Examples:
            [
                BuildDirectionalExecutorExampleWithWordAndPhraseMappings(
                    "en-ru-phrase-first-function-junk",
                    TranslationDirection.EN_RU,
                    "en",
                    "phrase training seed",
                    "SVO|EN_RU_PHRASE_FIRST",
                    "\u0422\u043e\u043c \u0431\u044b\u0441\u0442\u0440\u043e \u043f\u043e\u0442\u0443\u0448\u0438\u043b \u043e\u0433\u043e\u043d\u044c",
                    "phrase_first_required",
                    [
                        new ExecutorWordMapping("tom", SemanticConcept.UNKNOWN, "\u0422\u043e\u043c"),
                        new ExecutorWordMapping("quickly", SemanticConcept.UNKNOWN, "\u0431\u044b\u0441\u0442\u0440\u043e"),
                        new ExecutorWordMapping("put", SemanticConcept.UNKNOWN, "\u043f\u043e\u043b\u043e\u0436\u0438\u043b"),
                        new ExecutorWordMapping("out", SemanticConcept.UNKNOWN, "\u043d\u0430\u0440\u0443\u0436\u0443"),
                        new ExecutorWordMapping("the", SemanticConcept.UNKNOWN, "\u043c\u043e\u0435\u0439"),
                        new ExecutorWordMapping("fire", SemanticConcept.UNKNOWN, "\u043e\u0433\u043e\u043d\u044c")
                    ],
                    [
                        new ExecutorPhraseMapping("put out", "\u043f\u043e\u0442\u0443\u0448\u0438\u043b"),
                        new ExecutorPhraseMapping("the fire", "\u043e\u0433\u043e\u043d\u044c")
                    ])
            ],
            SourceIds: ["en-ru-phrase-first-function-junk-proof"]);

        var trainResult = await machine.TrainStructuredBatchAsync(batch);
        Assert.True(trainResult.IsSuccess, trainResult.ErrorMessage);

        var actual = await RunTranslationAsync(machine, "Tom quickly put out the fire", "en", "ru");

        Assert.DoesNotContain("[EN_RU:BOOTSTRAP]", actual);
        Assert.Contains("\u043f\u043e\u0442\u0443\u0448\u0438\u043b", actual);
        Assert.Contains("\u043e\u0433\u043e\u043d\u044c", actual);
        Assert.DoesNotContain("\u043c\u043e\u0435\u0439", actual);
        Assert.DoesNotContain("\u043f\u043e\u043b\u043e\u0436\u0438\u043b", actual);
        Assert.DoesNotContain("\u043d\u0430\u0440\u0443\u0436\u0443", actual);
    }

    [Fact]
    public async Task ExecutorUnit_EnRuPhraseFirstComposition_ReplacesRepeatedPhraseBeforeUnigrams()
    {
        var machine = new TranslationMachine();
        var batch = new TranslationLearningBatch(
            BatchId: "en-ru-repeated-phrase-first-proof",
            DomainTag: TranslationMachine.DomainTag,
            Examples:
            [
                BuildDirectionalExecutorExampleWithWordAndPhraseMappings(
                    "en-ru-repeated-phrase-first",
                    TranslationDirection.EN_RU,
                    "en",
                    "repeated phrase seed",
                    "SVO|EN_RU_REPEATED_PHRASE_FIRST",
                    "\u043f\u043e\u0442\u0443\u0448\u0438\u043b",
                    "phrase_first_required",
                    [
                        new ExecutorWordMapping("put", SemanticConcept.UNKNOWN, "\u043f\u043e\u043b\u043e\u0436\u0438\u043b"),
                        new ExecutorWordMapping("out", SemanticConcept.UNKNOWN, "\u043d\u0430\u0440\u0443\u0436\u0443"),
                        new ExecutorWordMapping("and", SemanticConcept.UNKNOWN, "\u0438")
                    ],
                    [new ExecutorPhraseMapping("put out", "\u043f\u043e\u0442\u0443\u0448\u0438\u043b")])
            ],
            SourceIds: ["en-ru-repeated-phrase-first-proof"]);

        var trainResult = await machine.TrainStructuredBatchAsync(batch);
        Assert.True(trainResult.IsSuccess, trainResult.ErrorMessage);

        var actual = await RunTranslationAsync(machine, "put out and put out", "en", "ru");

        Assert.DoesNotContain("[EN_RU:BOOTSTRAP]", actual);
        Assert.Equal("\u043f\u043e\u0442\u0443\u0448\u0438\u043b \u043f\u043e\u0442\u0443\u0448\u0438\u043b", actual);
        Assert.DoesNotContain("\u043f\u043e\u043b\u043e\u0436\u0438\u043b", actual);
        Assert.DoesNotContain("\u043d\u0430\u0440\u0443\u0436\u0443", actual);
    }

    [Fact]
    public async Task TranslationMachine_LoadCheckpoint_DoesNotPruneRuEnEnglishWordMaps()
    {
        var tmpPath = Path.GetTempFileName() + ".json";
        try
        {
            var checkpoint = BuildCheckpointWithExecutorMaps(
                TranslationDirection.RU_EN,
                [
                    new ExecutorWordMapEntryCheckpoint("добавить", SemanticConcept.UNKNOWN, "add"),
                    new ExecutorWordMapEntryCheckpoint("проверку", SemanticConcept.UNKNOWN, "check")
                ],
                [
                    new ExecutorStringMapEntryCheckpoint("добавить", "add"),
                    new ExecutorStringMapEntryCheckpoint("проверку", "check")
                ],
                [],
                []);
            await File.WriteAllTextAsync(
                tmpPath,
                JsonSerializer.Serialize(checkpoint, TranslationCheckpointJsonOptions.Standard),
                System.Text.Encoding.UTF8);

            var loadResult = await TranslationMachine.LoadCheckpointAsync(tmpPath);
            Assert.True(loadResult.IsSuccess, loadResult.ErrorMessage);

            var exported = loadResult.Value!.Machine.ExportCheckpoint();
            Assert.True(exported.IsSuccess, exported.ErrorMessage);
            var ruEnExecutor = exported.Value!.Executors.Single(e => e.Direction == TranslationDirection.RU_EN);
            Assert.Contains(ruEnExecutor.WordMap, entry => entry.Lemma == "добавить" && entry.Target == "add");
            Assert.Contains(ruEnExecutor.WordMap, entry => entry.Lemma == "проверку" && entry.Target == "check");
            Assert.Contains(ruEnExecutor.SurfaceWordMap, entry => entry.Key == "добавить" && entry.Value == "add");

            var actual = await RunTranslationAsync(loadResult.Value!.Machine, "добавить проверку", "ru", "en");
            Assert.Equal("add check", actual);
        }
        finally
        {
            if (File.Exists(tmpPath)) File.Delete(tmpPath);
        }
    }

    [Fact]
    public async Task ExecutorUnit_RuEnWordMap_RejectsCyrillicTargetComposition()
    {
        var machine = new TranslationMachine();
        var trainSource = "добавить запись";
        var runtimeSource = "добавить запись сейчас";
        var batch = new TranslationLearningBatch(
            BatchId: "ru-en-cyrillic-rejection-proof",
            DomainTag: TranslationMachine.DomainTag,
            Examples:
            [
                BuildDirectionalExecutorExampleWithMappings(
                    "ru-en-cyrillic-target",
                    TranslationDirection.RU_EN,
                    "ru",
                    trainSource,
                    "SVO|RU_EN_WORDMAP",
                    "добавить запись",
                    "copied_source_output",
                    [
                        new ExecutorWordMapping("добавить", SemanticConcept.UNKNOWN, "добавить"),
                        new ExecutorWordMapping("запись", SemanticConcept.UNKNOWN, "запись")
                    ])
            ],
            SourceIds: ["ru-en-cyrillic-rejection-proof"]);

        var trainResult = await machine.TrainStructuredBatchAsync(batch);
        Assert.True(trainResult.IsSuccess, trainResult.ErrorMessage);

        var actual = await RunTranslationAsync(machine, runtimeSource, "ru", "en");

        Assert.Contains("[RU_EN:BOOTSTRAP]", actual);
        Assert.Contains(runtimeSource, actual);
    }

    [Fact]
    public void TranslationStructuredBatchBuilder_EnRuSkipsLatinIdentityWordMappings()
    {
        var pair = new TranslationTrainReadyPair(
            Id: "en-ru-latin-identity-row-001",
            Source: "en-ru-latin-identity-proof",
            Direction: "EN_RU",
            PairFamily: "latin-identity-guard",
            Prompt: "Translate the following English sentence to Russian:",
            ASide: "ordinary word",
            Chosen: "ordinary слово",
            Rejected: "[WRONG: latin_identity_target - copies an ordinary English word into Russian output.]",
            ValidationOutcome: "train_ready",
            ValidationErrors: []);

        var batch = TranslationStructuredBatchBuilder.BuildGeneralBatch(
            "en-ru-latin-identity-proof",
            [pair],
            signatureMode: TranslationSignatureMode.Structural);
        var executor = batch.Examples.Single(example => example.UnitId == TranslationUnitIds.ExecutorEnRu);
        var chosen = JsonSerializer.Deserialize<ExecutorOutput>(executor.ChosenOutputJson, JsonOptions);

        Assert.NotNull(chosen!.WordMappings);
        Assert.DoesNotContain(
            chosen.WordMappings!,
            mapping => string.Equals(mapping.SourceWord, "ordinary", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(mapping.TargetWord, "ordinary", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void TatoebaPilotLoopRunner_EnRuMixedScriptRowFailsScriptAndProductMatch()
    {
        var row = new TatoebaPrivateSentencePair(
            Direction: "EN_RU",
            Word: "hello",
            Language: "en",
            SourceSentenceId: 1,
            TargetSentenceId: 101,
            SourceText: "hello tom",
            TargetText: "Привет Tom",
            SourcePattern: "SVO",
            TargetPattern: "SVO",
            SourceId: "test",
            License: "cc_by_2fr");
        var method = typeof(TatoebaPilotLoopRunner).GetMethod(
            "BuildRowResult",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);
        var result = Assert.IsType<TatoebaPilotLoopRowResult>(method!.Invoke(null, [row, "Привет Tom"]));

        Assert.True(result.ExactMatch);
        Assert.False(result.TargetScriptOk);
        Assert.False(result.ProductMatch);
    }

    [Fact]
    public void TatoebaPilotLoopRunner_InduceLexicon_RejectsLowConfidenceCooccurrence()
    {
        var rows = new[]
        {
            BuildPrivatePair(1, 101, "valuable carpets arrived", "ценные ковры прибыли", "LEX_A", "EN_RU"),
            BuildPrivatePair(2, 102, "valuable time mattered", "ценное время важно", "LEX_B", "EN_RU"),
            BuildPrivatePair(3, 103, "valuable advice helped", "ценный совет помог", "LEX_C", "EN_RU"),
            BuildPrivatePair(4, 104, "able see clearly", "не вижу ясно", "LEX_D", "EN_RU"),
            BuildPrivatePair(5, 105, "able see tomorrow", "не увижу завтра", "LEX_E", "EN_RU"),
            BuildPrivatePair(6, 106, "able see everything", "не видно всё", "LEX_F", "EN_RU"),
            BuildPrivatePair(7, 107, "people under bridge", "Люди под мостом", "LEX_G", "EN_RU"),
            BuildPrivatePair(8, 108, "children under roof", "Люди под крышей", "LEX_H", "EN_RU"),
            BuildPrivatePair(9, 109, "workers under pressure", "Люди под давлением", "LEX_I", "EN_RU"),
            BuildPrivatePair(10, 110, "Boston met Alice", "Том встретил Алису", "LEX_J", "EN_RU"),
            BuildPrivatePair(11, 111, "Boston saw rain", "Том увидел дождь", "LEX_K", "EN_RU"),
            BuildPrivatePair(12, 112, "Boston heard music", "Том услышал музыку", "LEX_L", "EN_RU")
        };
        var method = typeof(TatoebaPilotLoopRunner).GetMethod(
            "InduceLexicon",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);
        var lexicon = Assert.IsType<Dictionary<string, string>>(method!.Invoke(null, [rows]));

        Assert.False(lexicon.TryGetValue("valuable", out var valuable) && string.Equals(valuable, "ковры", StringComparison.OrdinalIgnoreCase));
        Assert.False(lexicon.TryGetValue("able", out var able) && string.Equals(able, "не", StringComparison.OrdinalIgnoreCase));
        Assert.False(lexicon.TryGetValue("see", out var see) && string.Equals(see, "не", StringComparison.OrdinalIgnoreCase));
        Assert.False(lexicon.TryGetValue("under", out var under) && string.Equals(under, "Люди", StringComparison.OrdinalIgnoreCase));
        Assert.False(lexicon.TryGetValue("boston", out var boston) && string.Equals(boston, "том", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void TatoebaPilotLoopRunner_EmitsRepeatedPhraseMappings_ForHouseArrest()
    {
        var rows = new[]
        {
            BuildPrivatePair(1, 101, "the judge ordered house arrest today", "судья назначил домашний арест сегодня", "PHRASE_A", "EN_RU"),
            BuildPrivatePair(2, 102, "the court ordered house arrest yesterday", "суд назначил домашний арест вчера", "PHRASE_B", "EN_RU"),
            BuildPrivatePair(3, 103, "police kept him under arrest tonight", "полиция держала его под арестом сегодня", "PHRASE_C", "EN_RU"),
            BuildPrivatePair(4, 104, "guards kept her under arrest overnight", "охрана держала её под арестом ночью", "PHRASE_D", "EN_RU"),
            BuildPrivatePair(5, 105, "please unlock the door now", "пожалуйста открой дверь сейчас", "PHRASE_E", "EN_RU"),
            BuildPrivatePair(6, 106, "can you unlock the door quietly", "можешь открой дверь тихо", "PHRASE_F", "EN_RU")
        };
        var phraseMethod = typeof(TatoebaPilotLoopRunner).GetMethod(
            "InducePhraseMappings",
            BindingFlags.NonPublic | BindingFlags.Static);
        var trainReadyMethod = typeof(TatoebaPilotLoopRunner).GetMethod(
            "ToTrainReadyPair",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(phraseMethod);
        Assert.NotNull(trainReadyMethod);
        var phraseMappings = phraseMethod!.Invoke(null, [rows]);
        var emitted = Assert.IsType<TranslationTrainReadyPair>(trainReadyMethod!.Invoke(null, [
            rows[0],
            1,
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
            phraseMappings!
        ]));

        Assert.Contains("[PHRASE: house arrest => домашний арест]", emitted.Rejected);
        Assert.DoesNotContain($"[PHRASE: {rows[0].SourceText} =>", emitted.Rejected);
    }

    [Fact]
    public void ExecutorUnit_EnRu_DoesNotCopySentenceInitialEnglishAsCodeIdentifier()
    {
        var tokenizer = new TranslationSemanticTokenizer();

        var sequence = tokenizer.Tokenize("People unlock the door", "en");

        Assert.NotEmpty(sequence.Tokens);
        Assert.Equal("People", sequence.Tokens[0].Word);
        Assert.NotEqual(TranslationPos.CODE_IDENTIFIER, sequence.Tokens[0].Pos);
    }

    [Fact]
    public void TranslationKnnStore_SourceAwareFallback_DoesNotReturnTomWalkedForArrest()
    {
        var store = new TranslationKnnStore();
        var broadSignature = "SVO|NOUN|VERB|PREP|NOUN";
        store.AddPair(broadSignature, "Tom walked across the street", "Том пошёл через дорогу");

        var unrelated = store.FindNearestCascade(
            broadSignature,
            "SVO|NOUN|VERB",
            broadSignature,
            k: 3,
            querySourceText: "the suspect is under arrest");
        var related = store.FindNearestCascade(
            broadSignature,
            "SVO|NOUN|VERB",
            broadSignature,
            k: 3,
            querySourceText: "Tom walked across the street");

        Assert.Empty(unrelated);
        Assert.NotEmpty(related);
    }

    [Fact]
    public async Task ExecutorUnit_EnRuOrdinaryNoConcept_UsesSourceAwareKnnBeforeBootstrap()
    {
        var machine = new TranslationMachine();
        var trainSource = "the blue house is empty";
        var runtimeSource = "the red house is empty";
        var learnedRussian = "\u0441\u0438\u043d\u0438\u0439 \u0434\u043e\u043c \u043f\u0443\u0441\u0442\u043e\u0439";

        var trainResult = await machine.TrainStructuredBatchAsync(new TranslationLearningBatch(
            BatchId: "en-ru-ordinary-source-aware-knn-001",
            DomainTag: TranslationMachine.DomainTag,
            Examples:
            [
                BuildDirectionalExecutorExampleWithMappings(
                    "en-ru-ordinary-source-aware-knn",
                    TranslationDirection.EN_RU,
                    "en",
                    trainSource,
                    "TRAIN_ONLY_SIGNATURE_WITH_NO_RUNTIME_OVERLAP",
                    learnedRussian,
                    "ordinary_knn_needed",
                    [])
            ],
            SourceIds: ["en-ru-ordinary-source-aware-knn"]));

        Assert.True(trainResult.IsSuccess, trainResult.ErrorMessage);

        var actual = await RunTranslationAsync(machine, runtimeSource, "en", "ru");

        Assert.Equal(learnedRussian, actual);
        Assert.DoesNotContain("[EN_RU:BOOTSTRAP]", actual);
    }

    [Fact]
    public async Task ExecutorUnit_EnRuPhraseMapping_WinsOverOrdinaryKnn()
    {
        var machine = new TranslationMachine();

        var trainResult = await machine.TrainStructuredBatchAsync(new TranslationLearningBatch(
            BatchId: "en-ru-phrase-before-ordinary-knn-001",
            DomainTag: TranslationMachine.DomainTag,
            Examples:
            [
                BuildDirectionalExecutorExampleWithWordAndPhraseMappings(
                    "en-ru-phrase-before-ordinary-knn",
                    TranslationDirection.EN_RU,
                    "en",
                    "the blue house is empty",
                    "TRAIN_ONLY_SIGNATURE_WITH_NO_RUNTIME_OVERLAP",
                    "\u0441\u0438\u043d\u0438\u0439 \u0434\u043e\u043c \u043f\u0443\u0441\u0442\u043e\u0439",
                    "phrase_must_win",
                    [],
                    [
                        new ExecutorPhraseMapping("red house", "\u043a\u0440\u0430\u0441\u043d\u044b\u0439 \u0434\u043e\u043c"),
                        new ExecutorPhraseMapping("is empty", "\u043f\u0443\u0441\u0442\u043e\u0439")
                    ])
            ],
            SourceIds: ["en-ru-phrase-before-ordinary-knn"]));

        Assert.True(trainResult.IsSuccess, trainResult.ErrorMessage);

        var actual = await RunTranslationAsync(machine, "red house is empty", "en", "ru");

        Assert.Equal("\u043a\u0440\u0430\u0441\u043d\u044b\u0439 \u0434\u043e\u043c \u043f\u0443\u0441\u0442\u043e\u0439", actual);
        Assert.DoesNotContain("\u0441\u0438\u043d\u0438\u0439", actual);
    }

    [Fact]
    public async Task ExecutorUnit_EnRuOrdinaryKnn_RejectsLatinOrSourceCopyCandidate()
    {
        var machine = new TranslationMachine();

        var trainResult = await machine.TrainStructuredBatchAsync(new TranslationLearningBatch(
            BatchId: "en-ru-ordinary-knn-latin-copy-rejection-001",
            DomainTag: TranslationMachine.DomainTag,
            Examples:
            [
                BuildDirectionalExecutorExampleWithMappings(
                    "en-ru-ordinary-knn-latin-copy-rejection",
                    TranslationDirection.EN_RU,
                    "en",
                    "the blue house is empty",
                    "TRAIN_ONLY_SIGNATURE_WITH_NO_RUNTIME_OVERLAP",
                    "the blue house is empty",
                    "latin_source_copy_rejected",
                    [])
            ],
            SourceIds: ["en-ru-ordinary-knn-latin-copy-rejection"]));

        Assert.True(trainResult.IsSuccess, trainResult.ErrorMessage);

        var actual = await RunTranslationAsync(machine, "the red house is empty", "en", "ru");

        Assert.Contains("[EN_RU:BOOTSTRAP]", actual);
        Assert.DoesNotContain("the blue house is empty", actual);
    }

    [Fact]
    public async Task ExecutorUnit_EnRuOrdinaryKnn_RejectsUnrelatedBroadUnlockNeighbor()
    {
        var machine = new TranslationMachine();
        var unrelatedRussian = "\u0422\u043e\u043c \u043f\u043e\u0448\u0435\u043b \u0447\u0435\u0440\u0435\u0437 \u0434\u043e\u0440\u043e\u0433\u0443";

        var trainResult = await machine.TrainStructuredBatchAsync(new TranslationLearningBatch(
            BatchId: "en-ru-ordinary-knn-broad-unlock-rejection-001",
            DomainTag: TranslationMachine.DomainTag,
            Examples:
            [
                BuildDirectionalExecutorExampleWithMappings(
                    "en-ru-ordinary-knn-broad-unlock-rejection",
                    TranslationDirection.EN_RU,
                    "en",
                    "Tom walked across the street",
                    "TRAIN_ONLY_SIGNATURE_WITH_NO_RUNTIME_OVERLAP",
                    unrelatedRussian,
                    "broad_knn_collision_rejected",
                    [])
            ],
            SourceIds: ["en-ru-ordinary-knn-broad-unlock-rejection"]));

        Assert.True(trainResult.IsSuccess, trainResult.ErrorMessage);

        var actual = await RunTranslationAsync(machine, "Tom walked to unlock the door", "en", "ru");

        Assert.Contains("[EN_RU:BOOTSTRAP]", actual);
        Assert.DoesNotContain(unrelatedRussian, actual);
    }

    [Fact]
    public async Task ExecutorUnit_EnRuOrdinaryKnn_HospitalRegressionRetrievesRussianNeighbor()
    {
        var machine = new TranslationMachine();
        var trainSource = "How many days were you in the hospital?";
        var runtimeSource = "How many weeks were you in the hospital?";
        var hospitalRussian = "\u0421\u043a\u043e\u043b\u044c\u043a\u043e \u0434\u043d\u0435\u0439 \u0442\u044b \u0431\u044b\u043b \u0432 \u0431\u043e\u043b\u044c\u043d\u0438\u0446\u0435?";

        var trainResult = await machine.TrainStructuredBatchAsync(new TranslationLearningBatch(
            BatchId: "en-ru-hospital-ordinary-knn-regression-001",
            DomainTag: TranslationMachine.DomainTag,
            Examples:
            [
                BuildDirectionalExecutorExampleWithMappings(
                    "en-ru-hospital-ordinary-knn-regression",
                    TranslationDirection.EN_RU,
                    "en",
                    trainSource,
                    "TRAIN_ONLY_SIGNATURE_WITH_NO_RUNTIME_OVERLAP",
                    hospitalRussian,
                    "hospital_neighbor_required",
                    [])
            ],
            SourceIds: ["en-ru-hospital-ordinary-knn-regression"]));

        Assert.True(trainResult.IsSuccess, trainResult.ErrorMessage);

        var actual = await RunTranslationAsync(machine, runtimeSource, "en", "ru");

        Assert.DoesNotContain("[EN_RU:BOOTSTRAP]", actual);
        Assert.Equal(hospitalRussian, actual);
    }

    [Fact]
    public void TatoebaProductMetric_DoesNotAcceptExtraNegationJunkAtSimilarityOne()
    {
        var row = new TatoebaPrivateSentencePair(
            Direction: "EN_RU",
            Word: "see",
            Language: "en",
            SourceSentenceId: 20,
            TargetSentenceId: 120,
            SourceText: "I see the door",
            TargetText: "я вижу дверь",
            SourcePattern: "SVO",
            TargetPattern: "SVO",
            SourceId: "test",
            License: "cc_by_2fr");
        var method = typeof(TatoebaPilotLoopRunner).GetMethod(
            "BuildRowResult",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);
        var result = Assert.IsType<TatoebaPilotLoopRowResult>(method!.Invoke(null, [row, "я не вижу дверь"]));

        Assert.Equal(1.0, result.TokenSimilarity);
        Assert.True(result.TargetScriptOk);
        Assert.False(result.ProductMatch);
    }

    [Fact]
    public async Task ExecutorUnit_RuEnWordMap_StillComposesLatinMappings()
    {
        var machine = new TranslationMachine();
        var source = "добавить проверку";
        var batch = new TranslationLearningBatch(
            BatchId: "ru-en-latin-wordmap-regression",
            DomainTag: TranslationMachine.DomainTag,
            Examples:
            [
                BuildDirectionalExecutorExampleWithMappings(
                    "ru-en-latin-wordmap",
                    TranslationDirection.RU_EN,
                    "ru",
                    source,
                    "SVO|RU_EN_LATIN_WORDMAP",
                    "add check",
                    "latin_regression",
                    [
                        new ExecutorWordMapping("добавить", SemanticConcept.UNKNOWN, "add"),
                        new ExecutorWordMapping("проверку", SemanticConcept.UNKNOWN, "check")
                    ])
            ],
            SourceIds: ["ru-en-latin-wordmap-regression"]);

        var trainResult = await machine.TrainStructuredBatchAsync(batch);
        Assert.True(trainResult.IsSuccess, trainResult.ErrorMessage);

        var actual = await RunTranslationAsync(machine, source, "ru", "en");

        Assert.DoesNotContain("[RU_EN:BOOTSTRAP]", actual);
        Assert.Equal("add check", actual);
    }

    [Fact]
    public async Task TranslationStructuredBatchBuilder_LearnsPrefixWordMappingForFrequencyRows()
    {
        var machine = new TranslationMachine();
        var pair = new TranslationTrainReadyPair(
            Id: "prefix-frequency-row-001",
            Source: "prefix-frequency-proof",
            Direction: "RU_EN",
            PairFamily: "frequency-prefix",
            Prompt: "Translate the following Russian sentence to English:",
            ASide: "foo alpha",
            Chosen: "Bar alpha",
            Rejected: "[WRONG: prefix_target_lost - drops the learned target prefix.]",
            ValidationOutcome: "train_ready",
            ValidationErrors: []);
        var batch = TranslationStructuredBatchBuilder.BuildRuEnGeneralBatch(
            "prefix-frequency-proof-001",
            [pair],
            signatureMode: TranslationSignatureMode.Structural);

        var trainResult = await machine.TrainStructuredBatchAsync(batch);
        Assert.True(trainResult.IsSuccess, trainResult.ErrorMessage);

        var actual = await RunTranslationAsync(machine, "foo beta");

        Assert.DoesNotContain("[RU_EN:BOOTSTRAP]", actual);
        Assert.Contains("Bar", actual);
    }

    [Fact]
    public async Task TranslationStructuredBatchBuilder_LearnsSourceMarkerMappingAfterSubject()
    {
        var machine = new TranslationMachine();
        var pair = new TranslationTrainReadyPair(
            Id: "source-marker-frequency-row-001",
            Source: "source-marker-frequency-proof",
            Direction: "RU_EN",
            PairFamily: "frequency-marker",
            Prompt: "Translate the following Russian sentence to English:",
            ASide: "ÐžÐ½ markerly alpha",
            Chosen: "He Targetly alpha",
            Rejected: "[WRONG: source_marker_target_lost - drops the learned source-marker adverb.]",
            ValidationOutcome: "train_ready",
            ValidationErrors: []);
        var batch = TranslationStructuredBatchBuilder.BuildRuEnGeneralBatch(
            "source-marker-frequency-proof-001",
            [pair],
            signatureMode: TranslationSignatureMode.Structural);

        var trainResult = await machine.TrainStructuredBatchAsync(batch);
        Assert.True(trainResult.IsSuccess, trainResult.ErrorMessage);

        var actual = await RunTranslationAsync(machine, "ÐžÐ½ markerly beta");

        Assert.DoesNotContain("[RU_EN:BOOTSTRAP]", actual);
        Assert.Contains("Targetly", actual);
    }

    [Fact]
    public async Task TranslationStructuredBatchBuilder_LearnsAgreementMappingsFromRejectedMetadata()
    {
        var machine = new TranslationMachine();
        var pair = new TranslationTrainReadyPair(
            Id: "agreement-row-001",
            Source: "agreement-proof",
            Direction: "RU_EN",
            PairFamily: "agreement-context",
            Prompt: "Translate the following Russian sentence to English:",
            ASide: "Она пошла",
            Chosen: "She went home",
            Rejected: "[WRONG: agreement_context_lost - subject gender is not connected to the verb form.] [AGREEMENT: Она => She] [AGREEMENT: пошла => went] [AGREEMENT: домой => home]",
            ValidationOutcome: "train_ready",
            ValidationErrors: []);
        var batch = TranslationStructuredBatchBuilder.BuildRuEnGeneralBatch(
            "agreement-proof-001",
            [pair],
            signatureMode: TranslationSignatureMode.Structural);

        var trainResult = await machine.TrainStructuredBatchAsync(batch);
        Assert.True(trainResult.IsSuccess, trainResult.ErrorMessage);

        var actual = await RunTranslationAsync(machine, "Она пошла домой");

        Assert.DoesNotContain("[RU_EN:BOOTSTRAP]", actual);
        Assert.Contains("She", actual);
        Assert.Contains("went", actual);
    }

    [Fact]
    public void TranslationSemanticTokenizer_AddsLanguageRuleTagsToKnnSignature()
    {
        var tokenizer = new TranslationSemanticTokenizer();

        var sequence = tokenizer.Tokenize("Она пошла к морю", "ru");

        Assert.Contains("RU:SUBJECT:FEM", sequence.RuleTags);
        Assert.Contains("RU:TENSE:PAST", sequence.RuleTags);
        Assert.Contains("RU:VERB_GENDER:FEM", sequence.RuleTags);
        Assert.Contains("RU:AGREEMENT:SUBJECT_FEM_PAST_FEM", sequence.RuleTags);
        Assert.Contains("RU:CASE:DATIVE", sequence.RuleTags);
        Assert.Contains("RU:TENSE:PAST", sequence.Signature);
        Assert.Contains("RU:CASE:DATIVE", sequence.StructureSignature);
    }

    [Fact]
    public void TranslationStructuredBatchBuilder_AppendsVerifiedRuleTagsToUnitSignatures()
    {
        // The source sentence is in the past tense, and the verified row says so explicitly.
        // The rejected variant deliberately claims the opposite tense: if the builder ever read
        // the "how it is wrong" side again, the stored lesson would carry the present-tense mark
        // and this test would fail.
        var pair = new TranslationTrainReadyPair(
            Id: "rule-tag-row-001",
            Source: "rule-tag-proof",
            Direction: "RU_EN",
            PairFamily: "agreement-context",
            Prompt: "Translate the following Russian sentence to English:",
            ASide: "Она пошла к морю",
            Chosen: "She went to the sea",
            Rejected: "[WRONG: agreement_context_lost.] [RULE: tense=present] [AGREEMENT: Она => She] [AGREEMENT: пошла => went]",
            ValidationOutcome: "train_ready",
            ValidationErrors: [],
            VerifiedRuleTags: ["TENSE=PAST", "CASE=DATIVE"]);

        var batch = TranslationStructuredBatchBuilder.BuildRuEnGeneralBatch(
            "rule-tag-proof-001",
            [pair],
            signatureMode: TranslationSignatureMode.Structural);

        Assert.All(batch.Examples, example =>
        {
            Assert.Contains("RULE:TENSE=PAST", example.SemanticSignature);
            Assert.Contains("RULE:CASE=DATIVE", example.SemanticSignature);
            Assert.DoesNotContain("RULE:TENSE=PRESENT", example.SemanticSignature);
        });
    }

    [Fact]
    public void ExecutorUnit_RuleTaggedInput_PrefersCurrentWordMappingsOverKnnNeighbor()
    {
        var knn = new TranslationKnnStore();
        var executor = new ExecutorUnit(TranslationDirection.RU_EN, knn);
        var signature = "SVO|RETURN_OP|RULE:AGREEMENT=SUBJECT_MASC";

        executor.TrainOnExample(new TranslationLearningExample(
            ExampleId: "rule-knn-neighbor",
            UnitId: TranslationUnitIds.ExecutorRuEn,
            FlowId: TranslationRoutingFlow.FlowId,
            InputJson: JsonSerializer.Serialize(new ExecutorInput("he returned home", "ru", "general"), JsonOptions),
            ChosenOutputJson: JsonSerializer.Serialize(new ExecutorOutput("He returned home", 0.93, null), JsonOptions),
            RejectedOutputJson: AddWrong(JsonSerializer.Serialize(new ExecutorOutput("returned home", 0.31, null), JsonOptions), "subject_lost"),
            RejectedType: "subject_lost",
            SemanticSignature: signature,
            SourceId: "rule-wordmap-proof",
            HeldOut: false));

        executor.TrainOnExample(new TranslationLearningExample(
            ExampleId: "rule-wordmap-cat",
            UnitId: TranslationUnitIds.ExecutorRuEn,
            FlowId: TranslationRoutingFlow.FlowId,
            InputJson: JsonSerializer.Serialize(new ExecutorInput("cat returned", "ru", "general"), JsonOptions),
            ChosenOutputJson: JsonSerializer.Serialize(new ExecutorOutput(
                "The cat returned",
                0.93,
                [
                    new ExecutorWordMapping("cat", SemanticConcept.UNKNOWN, "The cat"),
                    new ExecutorWordMapping("returned", SemanticConcept.RETURN_OP, "returned")
                ]), JsonOptions),
            RejectedOutputJson: AddWrong(JsonSerializer.Serialize(new ExecutorOutput("He returned", 0.31, null), JsonOptions), "wrong_subject"),
            RejectedType: "wrong_subject",
            SemanticSignature: "OTHER|RULE:AGREEMENT=SUBJECT_NOUN",
            SourceId: "rule-wordmap-proof",
            HeldOut: false));

        var sequence = new SemanticTokenSequence(
            Pattern: SentencePattern.SVO,
            Tokens:
            [
                new SemanticToken("cat", "cat", TranslationPos.NOUN, "SUBJECT", SemanticConcept.UNKNOWN),
                new SemanticToken("returned", "returned", TranslationPos.NOUN, "PREDICATE", SemanticConcept.RETURN_OP),
                new SemanticToken("river", "river", TranslationPos.NOUN, "OBJECT", SemanticConcept.UNKNOWN)
            ],
            PosBigrams: [(TranslationPos.NOUN, TranslationPos.NOUN), (TranslationPos.NOUN, TranslationPos.NOUN)],
            Signature: signature,
            PrefixSignature: signature,
            StructureSignature: "SVO|NOUN|NOUN|NOUN|RULE:AGREEMENT=SUBJECT_MASC",
            RuleTags: ["RULE:AGREEMENT=SUBJECT_MASC"]);

        var actual = executor.Execute(sequence, "general");

        Assert.Equal("executor-ru_en-rule-wordmap", actual.AdapterId);
        Assert.StartsWith("The cat returned", actual.Text, StringComparison.Ordinal);
    }

    [Fact]
    public void QualityVerifier_DoesNotBindQualityMarkerToBroadRuleSignature()
    {
        var verifier = new QualityVerifierUnit();
        var signature = "SVO|RETURN_OP|RU:SUBJECT:FEM|RU:CASE:DATIVE";

        verifier.TrainOnExample(new TranslationLearningExample(
            ExampleId: "broad-rule-verifier",
            UnitId: TranslationUnitIds.QualityVerifier,
            FlowId: TranslationRoutingFlow.FlowId,
            InputJson: JsonSerializer.Serialize(new VerifierInput("girl returned home", "bad candidate"), JsonOptions),
            ChosenOutputJson: JsonSerializer.Serialize(new VerifierOutput(
                0.42,
                ["expected 'The' for translated quality marker"],
                false,
                "The girl returned home"), JsonOptions),
            RejectedOutputJson: AddWrong(JsonSerializer.Serialize(new VerifierOutput(0.91, [], true, null), JsonOptions), "verifier_false_acceptance"),
            RejectedType: "verifier_false_acceptance",
            SemanticSignature: signature,
            SourceId: "rule-verifier-proof",
            HeldOut: false));

        var sequence = new SemanticTokenSequence(
            Pattern: SentencePattern.SVO,
            Tokens:
            [
                new SemanticToken("she", "she", TranslationPos.PRON, "SUBJECT", SemanticConcept.UNKNOWN),
                new SemanticToken("returned", "returned", TranslationPos.NOUN, "PREDICATE", SemanticConcept.RETURN_OP)
            ],
            PosBigrams: [(TranslationPos.PRON, TranslationPos.NOUN)],
            Signature: signature,
            PrefixSignature: signature,
            StructureSignature: "SVO|PRON|NOUN|RU:SUBJECT:FEM|RU:CASE:DATIVE",
            RuleTags: ["RU:SUBJECT:FEM", "RU:CASE:DATIVE"]);

        var result = verifier.Verify("She returned home", sequence);

        Assert.True(result.Pass, string.Join("; ", result.Issues));
    }

    [Fact]
    public async Task TranslationBatch029Runner_RejectsHeldOutRowsAboveSeventyPercentSimilarity()
    {
        var path = Path.Combine(Path.GetTempPath(), $"translation-similarity-contamination-{Guid.NewGuid():N}.jsonl");
        var rows = new[]
        {
            new TranslationTrainReadyPair(
                Id: "similarity-train-001",
                Source: "similarity-proof",
                Direction: "RU_EN",
                PairFamily: "agreement-context",
                Prompt: "Translate the following Russian sentence to English:",
                ASide: "Она пошла домой",
                Chosen: "She went home",
                Rejected: "[WRONG: agreement_context_lost.] [AGREEMENT: Она => She] [AGREEMENT: пошла => went]",
                ValidationOutcome: "train_ready",
                ValidationErrors: []),
            new TranslationTrainReadyPair(
                Id: "similarity-heldout-001",
                Source: "similarity-proof",
                Direction: "RU_EN",
                PairFamily: "agreement-context",
                Prompt: "Translate the following Russian sentence to English:",
                ASide: "Она пошла домой быстро",
                Chosen: "She went home quickly",
                Rejected: "[WRONG: agreement_context_lost.] [AGREEMENT: Она => She] [AGREEMENT: пошла => went]",
                ValidationOutcome: "train_ready",
                ValidationErrors: [])
        };

        try
        {
            await File.WriteAllLinesAsync(path, rows.Select(row => JsonSerializer.Serialize(
                row,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower })));

            var result = await TranslationBatch029Runner.RunAsync(path, trainRows: 1, heldOutRows: 1);

            Assert.False(result.IsSuccess);
            Assert.Equal("TRAIN_HELDOUT_SIMILARITY_OVERLAP", result.ErrorCode);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void TranslationStructuredBatchBuilder_ComputeTokenOverlap_ReturnsExpectedJaccard()
    {
        // Identical strings → overlap = 1.0.
        Assert.Equal(1.0, TranslationStructuredBatchBuilder.ComputeTokenOverlap("Он пошёл домой", "Он пошёл домой"));

        // No shared tokens → overlap = 0.0.
        Assert.Equal(0.0, TranslationStructuredBatchBuilder.ComputeTokenOverlap("абвгд", "ежзик"));

        // Partial overlap: "Он пошёл домой" (3 tokens) vs "Он пошёл домой вчера" (4 tokens).
        // Intersection = 3, Union = 4 → Jaccard = 0.75.
        var overlap = TranslationStructuredBatchBuilder.ComputeTokenOverlap(
            "Он пошёл домой",
            "Он пошёл домой вчера");
        Assert.Equal(0.75, overlap, precision: 4);
    }

    [Fact]
    public void TranslationStructuredBatchBuilder_FilterByTrainingHistoryUniqueness_RemovesPairsAboveThreshold()
    {
        // Two history sentences already trained.
        var trainedASides = new List<string>
        {
            "Он пошёл домой",
            "Кот спал на диване"
        };

        // Candidates: one too similar to history (75% overlap), two genuinely novel.
        var candidates = new[]
        {
            new TranslationTrainReadyPair(
                Id: "near-dup-001",
                Source: "test",
                Direction: "RU_EN",
                PairFamily: "test",
                Prompt: "Translate:",
                ASide: "Он пошёл домой вчера",   // 75% overlap with history[0] → filtered
                Chosen: "He went home yesterday",
                Rejected: "[WRONG: x.]",
                ValidationOutcome: "train_ready",
                ValidationErrors: []),
            new TranslationTrainReadyPair(
                Id: "novel-001",
                Source: "test",
                Direction: "RU_EN",
                PairFamily: "test",
                Prompt: "Translate:",
                ASide: "Она открыла дверь",       // completely different → kept
                Chosen: "She opened the door",
                Rejected: "[WRONG: x.]",
                ValidationOutcome: "train_ready",
                ValidationErrors: []),
            new TranslationTrainReadyPair(
                Id: "novel-002",
                Source: "test",
                Direction: "RU_EN",
                PairFamily: "test",
                Prompt: "Translate:",
                ASide: "Разработчик исправил ошибку",  // completely different → kept
                Chosen: "The developer fixed the bug",
                Rejected: "[WRONG: x.]",
                ValidationOutcome: "train_ready",
                ValidationErrors: [])
        };

        var result = TranslationStructuredBatchBuilder.FilterByTrainingHistoryUniqueness(
            candidates,
            trainedASides,
            maxOverlapThreshold: 0.70);

        // The near-duplicate should be filtered; the two novel pairs should remain.
        Assert.Equal(2, result.Count);
        Assert.DoesNotContain(result, p => p.Id == "near-dup-001");
        Assert.Contains(result, p => p.Id == "novel-001");
        Assert.Contains(result, p => p.Id == "novel-002");
    }

    [Fact]
    public async Task ExecutorUnit_GenderAlternateForm_FeminineVerbTransfersFromMasculineTraining()
    {
        // Train on masculine verb form "ответил" via AGREEMENT annotation.
        // The gender alternate derivation should store "ответила" → "answered" automatically,
        // enabling inference on the feminine form without a separate training pair.
        var machine = new TranslationMachine();
        var mascPair = new TranslationTrainReadyPair(
            Id: "gender-alternate-test-001",
            Source: "gender-alternate-proof",
            Direction: "RU_EN",
            PairFamily: "gender-agreement",
            Prompt: "Translate the following Russian sentence to English:",
            ASide: "Он ответил быстро",
            Chosen: "He answered quickly",
            Rejected: "[WRONG: gender_agreement_lost.] [AGREEMENT: Он => He] [AGREEMENT: Она => She] [AGREEMENT: ответил => answered]",
            ValidationOutcome: "train_ready",
            ValidationErrors: []);

        var batch = TranslationStructuredBatchBuilder.BuildRuEnGeneralBatch(
            "gender-alternate-proof-001",
            [mascPair],
            signatureMode: TranslationSignatureMode.Structural);

        var trainResult = await machine.TrainStructuredBatchAsync(batch);
        Assert.True(trainResult.IsSuccess, trainResult.ErrorMessage);

        // Now run inference on the FEMININE form ("ответила") which was NOT in training data.
        // Gender alternate derivation: "ответила" ends in -ла → stored as "ответил" → "answered".
        var actual = await RunTranslationAsync(machine, "Она ответила");

        // The verb "ответила" (feminine past) should transfer "answered" from trained "ответил".
        Assert.DoesNotContain("[RU_EN:BOOTSTRAP]", actual);
        Assert.DoesNotContain("Она", actual);
        Assert.DoesNotContain("ответила", actual);
        Assert.Contains("She", actual, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("answered", actual, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FrequencyQuoteMapBatchBuilder_BuildsThreeUnitExamplesPerEntry()
    {
        var batch = FrequencyQuoteMapBatchBuilder.BuildBatch(
            "frequency-quote-proof-001",
            [
                new FrequencyQuoteEntry(
                    SourceWord: "null",
                    SourceLang: "ru",
                    GrammaticalRole: "TECH_TERM",
                    SourcePrefix: "null ValidateToken",
                    TargetPrefix: "Use null",
                    FullSourceSentence: "null ValidateToken",
                    FullTargetSentence: "Use null for ValidateToken",
                    SourceId: "frequency-quote-test-001")
            ]);

        Assert.Equal(TranslationMachine.DomainTag, batch.DomainTag);
        Assert.Equal(3, batch.Examples.Count);
        Assert.Contains(batch.Examples, example => example.UnitId == TranslationUnitIds.SemanticTokenizer);
        Assert.Contains(batch.Examples, example => example.UnitId == TranslationUnitIds.ExecutorRuEn);
        Assert.Contains(batch.Examples, example => example.UnitId == TranslationUnitIds.QualityVerifier);

        var executor = batch.Examples.Single(example => example.UnitId == TranslationUnitIds.ExecutorRuEn);
        var chosen = JsonSerializer.Deserialize<ExecutorOutput>(executor.ChosenOutputJson, JsonOptions);

        Assert.Equal("Use null for ValidateToken", chosen!.Text);
        Assert.NotNull(chosen.WordMappings);
        Assert.Contains(chosen.WordMappings!, mapping =>
            mapping.SourceWord == "null" &&
            mapping.Concept == SemanticConcept.UNKNOWN &&
            mapping.TargetWord == "null");
    }

    [Fact]
    public async Task TrainStructuredBatchAsync_PrefixKnn_FindsSimilarStructureWithoutExactMatch()
    {
        var machine = new TranslationMachine();
        var source = "null ValidateToken";

        var batch = new TranslationLearningBatch(
            BatchId: "translation-prefix-knn-proof-001",
            DomainTag: TranslationMachine.DomainTag,
            Examples:
            [
                BuildExecutorExampleWithMappings(
                    "prefix-knn-executor",
                    source,
                    "TRAIN_ONLY_SIGNATURE_WITH_NO_RUNTIME_OVERLAP",
                    "Use null for ValidateToken",
                    "prefix_knn_needed",
                    [])
            ],
            SourceIds: ["prefix-knn-proof"]);

        var trainResult = await machine.TrainStructuredBatchAsync(batch);
        Assert.True(trainResult.IsSuccess, trainResult.ErrorMessage);

        var actual = await RunTranslationAsync(machine, source);

        Assert.Equal("Use null for ValidateToken", actual);
        Assert.DoesNotContain("[RU_EN:BOOTSTRAP]", actual);
    }

    [Fact]
    public async Task TrainStructuredBatchAsync_DoesNotUseLowInformationStructureKnnForUnseenText()
    {
        var machine = new TranslationMachine();

        var trainResult = await machine.TrainStructuredBatchAsync(new TranslationLearningBatch(
            BatchId: "translation-low-information-knn-guard-001",
            DomainTag: TranslationMachine.DomainTag,
            Examples:
            [
                BuildExecutorExample(
                    "low-information-executor",
                    "source one",
                    "SVO",
                    "I am twenty years old.",
                    "I am old.",
                    "low_information_structure_collision")
            ],
            SourceIds: ["low-information-knn-guard"]));

        Assert.True(trainResult.IsSuccess, trainResult.ErrorMessage);

        var trainedSource = await RunTranslationAsync(machine, "source one");
        var unseenSource = await RunTranslationAsync(machine, "maybe");

        Assert.Equal("I am twenty years old.", trainedSource);
        Assert.NotEqual("I am twenty years old.", unseenSource);
        Assert.Contains("[RU_EN:BOOTSTRAP]", unseenSource);
    }

    [Fact]
    public async Task TrainStructuredBatchAsync_LocalBatch029Smoke_WhenAvailable()
    {
        const string localBatchPath =
            @"C:\Users\lubao\Documents\xiigen\llm mvp\language translate learning\translation-runs\2026-05-11\ru-en-general\level-1\batch-029\training-pairs-train-ready.jsonl";

        if (!File.Exists(localBatchPath))
            return;

        var jsonl = await File.ReadAllTextAsync(localBatchPath);
        var rows = TranslationStructuredBatchBuilder.ParseJsonl(jsonl).Take(50).ToArray();
        var batch = TranslationStructuredBatchBuilder.BuildRuEnGeneralBatch(
            "ru-en-general-l1-b029-local-smoke",
            rows);

        var machine = new TranslationMachine();
        var trainResult = await machine.TrainStructuredBatchAsync(batch);

        Assert.True(trainResult.IsSuccess, trainResult.ErrorMessage);
        Assert.Equal(150, trainResult.Value!.RowsTrained);
        Assert.Equal(1.0, trainResult.Value.AccuracyAfter);

        var evalItems = rows.Take(5)
            .Select(row => new LearningEvaluationItem(row.Id, row.ASide, row.Chosen))
            .ToArray();
        var eval = await machine.EvaluateAsync(new LearningEvaluationSet<TranslationDomain>(
            EvaluationId: "ru-en-general-l1-b029-local-smoke-eval",
            DomainTag: TranslationMachine.DomainTag,
            EvaluationItems: evalItems));

        Assert.True(eval.IsSuccess, eval.ErrorMessage);
        Assert.Equal(1.0, eval.Value!.Accuracy);
    }

    [Fact]
    public async Task TranslationBatch029Runner_LocalHeldOutReport_WhenAvailable()
    {
        const string localBatchPath =
            @"C:\Users\lubao\Documents\xiigen\llm mvp\language translate learning\translation-runs\2026-05-11\ru-en-general\level-1\batch-029\training-pairs-train-ready.jsonl";

        if (!File.Exists(localBatchPath))
            return;

        var reportResult = await TranslationBatch029Runner.RunAsync(localBatchPath, trainRows: 40, heldOutRows: 10);

        Assert.True(reportResult.IsSuccess, reportResult.ErrorMessage);
        var report = reportResult.Value!;
        Assert.Equal(50, report.SourceRows);
        Assert.Equal(40, report.TrainRows);
        Assert.Equal(10, report.HeldOutRows);
        Assert.Equal(120, report.UnitExamplesTrained);
        Assert.Equal(30, report.UnitExamplesHeldOut);
        Assert.InRange(report.UnitAccuracyAfter, 0.0, 1.0);
        Assert.InRange(report.RuntimeExactAccuracy, 0.0, 1.0);
        Assert.NotEmpty(report.CheckpointRef);
        Assert.NotEmpty(report.ManifestRef);
        Assert.Equal(10, report.HeldOutResults.Count);

        var tempReportPath = Path.Combine(Path.GetTempPath(), $"translation-batch029-report-{Guid.NewGuid():N}.json");
        await TranslationBatch029Runner.WriteReportAsync(report, tempReportPath);

        Assert.True(File.Exists(tempReportPath));
        var persisted = JsonSerializer.Deserialize<TranslationBatch029RunReport>(
            await File.ReadAllTextAsync(tempReportPath),
            JsonOptions);
        Assert.NotNull(persisted);
        Assert.Equal(report.BatchId, persisted!.BatchId);
    }

    [Fact]
    public async Task TranslationBatch029Runner_DoesNotTrainOnHeldOutRows()
    {
        var train = new TranslationTrainReadyPair(
            Id: "leak-guard-train-001",
            Source: "leak-guard",
            Direction: "RU_EN",
            PairFamily: "heldout-leak-guard",
            Prompt: "Translate the following Russian sentence to English:",
            ASide: "train source alpha",
            Chosen: "Train Target Alpha",
            Rejected: "[WRONG: train_target_alpha_lost - near miss for the training row only.]",
            ValidationOutcome: "train_ready",
            ValidationErrors: []);
        var heldOut = new TranslationTrainReadyPair(
            Id: "leak-guard-heldout-001",
            Source: "leak-guard",
            Direction: "RU_EN",
            PairFamily: "heldout-leak-guard",
            Prompt: "Translate the following Russian sentence to English:",
            ASide: "held source beta",
            Chosen: "Held Target Beta",
            Rejected: "[WRONG: held_target_beta_lost - near miss for the held-out row only.]",
            ValidationOutcome: "train_ready",
            ValidationErrors: []);

        var tempPath = Path.Combine(Path.GetTempPath(), $"translation-heldout-leak-guard-{Guid.NewGuid():N}.jsonl");
        try
        {
            await File.WriteAllTextAsync(
                tempPath,
                string.Join(
                    Environment.NewLine,
                    JsonSerializer.Serialize(train, JsonOptions),
                    JsonSerializer.Serialize(heldOut, JsonOptions)));

            var reportResult = await TranslationBatch029Runner.RunAsync(tempPath, trainRows: 1, heldOutRows: 1);

            Assert.True(reportResult.IsSuccess, reportResult.ErrorMessage);
            var report = reportResult.Value!;
            Assert.Equal(3, report.UnitExamplesTrained);
            Assert.Equal(3, report.UnitExamplesHeldOut);

            var heldOutResult = Assert.Single(report.HeldOutResults);
            Assert.Equal(heldOut.Id, heldOutResult.PairId);
            Assert.False(heldOutResult.ExactMatch);
            Assert.False(heldOutResult.ProductMatch);
            var independentlyRecomputedAccuracyTargets =
                report.UnitAccuracyAfter >= report.UnitAccuracyBefore + 0.02 &&
                report.RuntimeProductAccuracy >= 0.95 &&
                report.RuntimeBootstrapRate <= 0.15;
            Assert.Equal(independentlyRecomputedAccuracyTargets, report.MeetsAccuracyTargets);
            Assert.Null(report.RewardMargin);
            Assert.False(report.RewardMarginAvailable);
            Assert.Equal(0, report.RewardMarginEvidenceCount);
            Assert.Null(report.RewardMarginSource);
            Assert.Equal(independentlyRecomputedAccuracyTargets, report.PassesGate);
            Assert.False(report.PassesPromotionGate);
            Assert.StartsWith("REWARD_MARGIN_UNAVAILABLE", report.GateNote, StringComparison.Ordinal);
            Assert.NotEqual(heldOut.Chosen, heldOutResult.ActualTranslation);

            var roundTrip = JsonSerializer.Deserialize<TranslationBatch029RunReport>(
                JsonSerializer.Serialize(report, JsonOptions),
                JsonOptions);
            Assert.NotNull(roundTrip);
            Assert.Null(roundTrip!.RewardMargin);
            Assert.False(roundTrip.RewardMarginAvailable);
            Assert.Equal(0, roundTrip.RewardMarginEvidenceCount);
            Assert.Null(roundTrip.RewardMarginSource);
            Assert.Equal(roundTrip.MeetsAccuracyTargets, roundTrip.PassesGate);
            Assert.False(roundTrip.PassesPromotionGate);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    [Fact]
    public async Task TranslationBatch029Runner_LocalTrainReadyCorpus_RejectsExactHeldOutContamination_WhenAvailable()
    {
        const string localBatchPath =
            @"C:\Users\lubao\Documents\xiigen\llm mvp\language translate learning\translation-runs\2026-05-11\ru-en-general\level-1\batch-029\training-pairs-train-ready.jsonl";
        const string localCorpusRoot =
            @"C:\Users\lubao\Documents\xiigen\llm mvp\language translate learning";

        if (!File.Exists(localBatchPath) || !Directory.Exists(localCorpusRoot))
            return;

        var reportResult = await TranslationBatch029Runner.RunAsync(
            localBatchPath,
            trainRows: 40,
            heldOutRows: 10,
            additionalTrainingRoot: localCorpusRoot);

        Assert.False(reportResult.IsSuccess);
        Assert.Equal("TRAIN_HELDOUT_OVERLAP", reportResult.ErrorCode);
        Assert.Contains("contamination", reportResult.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Use a non-overlapping 100+100 split", reportResult.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task TranslationBatch029Runner_StrictCorpusWithSupplementalRepairRows_DoesNotPromoteBelow95_WhenAvailable()
    {
        const string localBatchPath =
            @"C:\Users\lubao\Documents\xiigen\llm mvp\language translate learning\translation-runs\2026-05-11\ru-en-general\level-1\batch-029\training-pairs-train-ready.jsonl";
        const string strictCorpusRoot =
            @"C:\Users\lubao\Documents\xiigen\llm mvp\language translate learning\translation-runs\2026-05-11\ru-en-general\level-1";

        if (!File.Exists(localBatchPath) || !Directory.Exists(strictCorpusRoot))
            return;

        var reportResult = await TranslationBatch029Runner.RunAsync(
            localBatchPath,
            trainRows: 40,
            heldOutRows: 10,
            additionalTrainingRoot: strictCorpusRoot,
            supplementalTrainingPairs: BuildBatch029AdverbRepairRows());

        Assert.True(reportResult.IsSuccess, reportResult.ErrorMessage);
        var report = reportResult.Value!;

        Assert.Equal(report.MeetsAccuracyTargets, report.PassesGate);
        Assert.False(report.PassesPromotionGate);
        Assert.StartsWith("REWARD_MARGIN_UNAVAILABLE", report.GateNote, StringComparison.Ordinal);
        Assert.True(report.RuntimeProductAccuracy < 0.95, $"Product accuracy unexpectedly reached {report.RuntimeProductAccuracy:0.000}");
        Assert.True(report.RuntimeExactAccuracy < 0.95, "This repair should not pretend to solve exact sentence translation.");
        Assert.All(report.HeldOutResults, row => Assert.DoesNotContain("I am twenty years old.", row.ActualTranslation));
    }

    [Fact]
    public async Task TranslationPhraseRepairArtifact_IsTrainReady()
    {
        var artifactPath = GetRepoTrainingDataPath("ru-en-general-l1-b029-phrase-repair-train-ready.jsonl");

        Assert.True(File.Exists(artifactPath), $"Missing train-ready artifact: {artifactPath}");

        var rows = TranslationStructuredBatchBuilder.ParseJsonl(await File.ReadAllTextAsync(artifactPath));

        Assert.Equal(10, rows.Count);
        Assert.All(rows, row =>
        {
            Assert.Equal("RU_EN", row.Direction);
            Assert.Equal("train_ready", row.ValidationOutcome);
            Assert.Empty(row.ValidationErrors ?? []);
            Assert.Contains("[PHRASE:", row.Rejected);
            Assert.Contains("[WRONG: phrase_boundary_lost", row.Rejected);
        });
    }

    [Fact]
    public async Task TranslationBatch029Runner_StrictCorpusWithSupplementalPhraseArtifact_RejectsSimilarityContamination_WhenAvailable()
    {
        const string localBatchPath =
            @"C:\Users\lubao\Documents\xiigen\llm mvp\language translate learning\translation-runs\2026-05-11\ru-en-general\level-1\batch-029\training-pairs-train-ready.jsonl";
        const string strictCorpusRoot =
            @"C:\Users\lubao\Documents\xiigen\llm mvp\language translate learning\translation-runs\2026-05-11\ru-en-general\level-1";
        var phraseArtifactPath = GetRepoTrainingDataPath("ru-en-general-l1-b029-phrase-repair-train-ready.jsonl");

        if (!File.Exists(localBatchPath) || !Directory.Exists(strictCorpusRoot) || !File.Exists(phraseArtifactPath))
            return;

        var reportResult = await TranslationBatch029Runner.RunAsync(
            localBatchPath,
            trainRows: 40,
            heldOutRows: 10,
            additionalTrainingRoot: strictCorpusRoot,
            supplementalTrainingPath: phraseArtifactPath);

        Assert.False(reportResult.IsSuccess);
        Assert.Equal("TRAIN_HELDOUT_SIMILARITY_OVERLAP", reportResult.ErrorCode);
        Assert.Contains("more than 70% token overlap", reportResult.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Replace those held-out rows", reportResult.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task TranslationProductPromptArtifacts_AreTrainReady()
    {
        var trainPath = GetRepoTrainingDataPath("ru-en-product-prompt-b001-train-ready.jsonl");
        var heldOutPath = GetRepoTrainingDataPath("tr-ho-ru-en-product-heldout-v1.jsonl");

        Assert.True(File.Exists(trainPath), $"Missing train-ready artifact: {trainPath}");
        Assert.True(File.Exists(heldOutPath), $"Missing held-out artifact: {heldOutPath}");

        var trainRows = TranslationStructuredBatchBuilder.ParseJsonl(await File.ReadAllTextAsync(trainPath));

        Assert.Equal(8, trainRows.Count);
        Assert.All(trainRows, row =>
        {
            Assert.Equal("RU_EN", row.Direction);
            Assert.Equal("train_ready", row.ValidationOutcome);
            Assert.Empty(row.ValidationErrors ?? []);
            Assert.Contains("[PHRASE:", row.Rejected);
            Assert.Contains("[WRONG: product_phrase_boundary_lost", row.Rejected);
        });
    }

    [Fact]
    public async Task TranslationProductHeldOutRunner_RuEnProductPrompts_Reaches95ProductAccuracy()
    {
        var trainPath = GetRepoTrainingDataPath("ru-en-product-prompt-b001-train-ready.jsonl");
        var heldOutPath = GetRepoTrainingDataPath("tr-ho-ru-en-product-heldout-v1.jsonl");

        var reportResult = await TranslationProductHeldOutRunner.RunRuEnAsync(trainPath, heldOutPath);

        Assert.True(reportResult.IsSuccess, reportResult.ErrorMessage);
        var report = reportResult.Value!;

        Assert.True(report.MeetsAccuracyTargets, report.GateNote);
        Assert.Null(report.RewardMargin);
        Assert.False(report.RewardMarginAvailable);
        Assert.Equal(0, report.RewardMarginEvidenceCount);
        Assert.Null(report.RewardMarginSource);
        Assert.True(report.PassesGate);
        Assert.False(report.PassesPromotionGate);
        Assert.StartsWith("REWARD_MARGIN_UNAVAILABLE", report.GateNote, StringComparison.Ordinal);
        Assert.Equal(8, report.HeldOutRows);
        Assert.True(report.RuntimeProductAccuracy >= 0.95, $"Product accuracy was {report.RuntimeProductAccuracy:0.000}");
        Assert.True(report.RuntimeExactAccuracy >= 0.95, $"Exact accuracy was {report.RuntimeExactAccuracy:0.000}");
        Assert.Equal(0.0, report.RuntimeBootstrapRate);
        Assert.Equal(1.0, report.TargetScriptAccuracy);
        Assert.Equal(0.0, report.CopiedSourceRate);
        Assert.Equal(0.0, report.WrongDirectionRate);
        Assert.Equal(1.0, report.EmojiPreservationAccuracy);
        Assert.Equal(1.0, report.NamedEntityPreservationAccuracy);
        Assert.All(report.CaseResults, row => Assert.True(row.ProductMatch, row.CaseId));

        var roundTrip = JsonSerializer.Deserialize<TranslationProductHeldOutRunReport>(
            JsonSerializer.Serialize(report, JsonOptions),
            JsonOptions);
        Assert.NotNull(roundTrip);
        Assert.True(roundTrip!.MeetsAccuracyTargets);
        Assert.Null(roundTrip.RewardMargin);
        Assert.False(roundTrip.RewardMarginAvailable);
        Assert.Equal(0, roundTrip.RewardMarginEvidenceCount);
        Assert.Null(roundTrip.RewardMarginSource);
        Assert.True(roundTrip.PassesGate);
        Assert.False(roundTrip.PassesPromotionGate);
    }

    [Fact]
    public async Task TranslationProductPromptAllDirectionArtifacts_AreTrainReady()
    {
        var trainPath = GetRepoTrainingDataPath("product-prompt-b001-all-directions-train-ready.jsonl");
        var heldOutPath = GetRepoTrainingDataPath("tr-ho-product-heldout-v2.jsonl");

        Assert.True(File.Exists(trainPath), $"Missing train-ready artifact: {trainPath}");
        Assert.True(File.Exists(heldOutPath), $"Missing held-out artifact: {heldOutPath}");

        var trainRows = TranslationStructuredBatchBuilder.ParseJsonl(await File.ReadAllTextAsync(trainPath));

        Assert.Equal(16, trainRows.Count);
        Assert.Contains(trainRows, row => row.Direction == "RU_EN");
        Assert.Contains(trainRows, row => row.Direction == "EN_RU");
        Assert.Contains(trainRows, row => row.Direction == "HE_EN");
        Assert.Contains(trainRows, row => row.Direction == "EN_HE");
        Assert.All(trainRows, row =>
        {
            Assert.Equal("train_ready", row.ValidationOutcome);
            Assert.Empty(row.ValidationErrors ?? []);
            Assert.Contains("[PHRASE:", row.Rejected);
            Assert.Contains("[WRONG: product_phrase_boundary_lost", row.Rejected);
        });

        var heldOutRows = File.ReadAllLines(heldOutPath)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonSerializer.Deserialize<TranslationProductHeldOutCase>(line, JsonOptions))
            .Where(row => row is not null)
            .Select(row => row!)
            .ToArray();

        Assert.Equal(16, heldOutRows.Length);
        Assert.Contains(heldOutRows, row => row.Direction == "RU_EN");
        Assert.Contains(heldOutRows, row => row.Direction == "EN_RU");
        Assert.Contains(heldOutRows, row => row.Direction == "HE_EN");
        Assert.Contains(heldOutRows, row => row.Direction == "EN_HE");
        Assert.Equal(16, CountTrainHeldOutOverlaps(trainRows, heldOutRows));
    }

    [Fact]
    public async Task TranslationProductHeldOutRunner_AllDirections_RejectsContaminatedHeldOut()
    {
        var trainPath = GetRepoTrainingDataPath("product-prompt-b001-all-directions-train-ready.jsonl");
        var heldOutPath = GetRepoTrainingDataPath("tr-ho-product-heldout-v2.jsonl");

        var reportResult = await TranslationProductHeldOutRunner.RunAllDirectionsAsync(trainPath, heldOutPath);

        Assert.False(reportResult.IsSuccess);
        Assert.Equal("TRAIN_HELDOUT_OVERLAP", reportResult.ErrorCode);
    }

    [Fact]
    public async Task TranslationProductPromptB002ExpandedArtifacts_AreTrainReady()
    {
        var trainPath = GetRepoTrainingDataPath("product-prompt-b002-expanded-train-ready.jsonl");
        var contaminatedHeldOutPath = GetRepoTrainingDataPath("tr-ho-product-heldout-v3.jsonl");
        var composedHeldOutPath = GetRepoTrainingDataPath("tr-ho-product-heldout-v4-composed-unseen.jsonl");

        Assert.True(File.Exists(trainPath), $"Missing train-ready artifact: {trainPath}");
        Assert.True(File.Exists(contaminatedHeldOutPath), $"Missing contaminated held-out artifact: {contaminatedHeldOutPath}");
        Assert.True(File.Exists(composedHeldOutPath), $"Missing composed held-out artifact: {composedHeldOutPath}");

        var trainRows = TranslationStructuredBatchBuilder.ParseJsonl(await File.ReadAllTextAsync(trainPath));

        Assert.Equal(16, trainRows.Count);
        Assert.Contains(trainRows, row => row.Direction == "RU_EN");
        Assert.Contains(trainRows, row => row.Direction == "EN_RU");
        Assert.Contains(trainRows, row => row.Direction == "HE_EN");
        Assert.Contains(trainRows, row => row.Direction == "EN_HE");
        Assert.All(trainRows, row =>
        {
            Assert.Equal("train_ready", row.ValidationOutcome);
            Assert.Empty(row.ValidationErrors ?? []);
            Assert.Contains("[PHRASE:", row.Rejected);
            Assert.Contains("[WRONG: product_phrase_boundary_lost", row.Rejected);
        });

        var contaminatedHeldOutRows = await LoadProductHeldOutCasesAsync(contaminatedHeldOutPath);
        Assert.Equal(16, contaminatedHeldOutRows.Length);
        Assert.Equal("TR-HO-17", contaminatedHeldOutRows.First().CaseId);
        Assert.Equal("TR-HO-32", contaminatedHeldOutRows.Last().CaseId);
        Assert.Equal(16, CountTrainHeldOutOverlaps(trainRows, contaminatedHeldOutRows));

        var heldOutRows = await LoadProductHeldOutCasesAsync(composedHeldOutPath);
        Assert.Equal(16, heldOutRows.Length);
        Assert.Equal("TR-HO-33", heldOutRows.First().CaseId);
        Assert.Equal("TR-HO-48", heldOutRows.Last().CaseId);
        Assert.Contains(heldOutRows, row => row.Direction == "RU_EN");
        Assert.Contains(heldOutRows, row => row.Direction == "EN_RU");
        Assert.Contains(heldOutRows, row => row.Direction == "HE_EN");
        Assert.Contains(heldOutRows, row => row.Direction == "EN_HE");
        Assert.Equal(0, CountTrainHeldOutOverlaps(trainRows, heldOutRows));
    }

    [Fact]
    public async Task TranslationProductHeldOutRunner_B002Expanded_RejectsContaminatedHeldOut()
    {
        var trainPath = GetRepoTrainingDataPath("product-prompt-b002-expanded-train-ready.jsonl");
        var heldOutPath = GetRepoTrainingDataPath("tr-ho-product-heldout-v3.jsonl");

        var reportResult = await TranslationProductHeldOutRunner.RunAllDirectionsAsync(trainPath, heldOutPath);

        Assert.False(reportResult.IsSuccess);
        Assert.Equal("TRAIN_HELDOUT_OVERLAP", reportResult.ErrorCode);
    }

    [Fact]
    public async Task TranslationProductHeldOutRunner_B002ComposedUnseen_Reaches95ProductAccuracy()
    {
        var trainPath = GetRepoTrainingDataPath("product-prompt-b002-expanded-train-ready.jsonl");
        var heldOutPath = GetRepoTrainingDataPath("tr-ho-product-heldout-v4-composed-unseen.jsonl");

        var reportResult = await TranslationProductHeldOutRunner.RunAllDirectionsAsync(trainPath, heldOutPath);

        Assert.True(reportResult.IsSuccess, reportResult.ErrorMessage);
        var report = reportResult.Value!;

        Assert.True(report.MeetsAccuracyTargets, BuildProductGateFailureMessage(report));
        Assert.Null(report.RewardMargin);
        Assert.False(report.RewardMarginAvailable);
        Assert.Equal(0, report.RewardMarginEvidenceCount);
        Assert.Null(report.RewardMarginSource);
        Assert.True(report.PassesGate);
        Assert.False(report.PassesPromotionGate);
        Assert.StartsWith("REWARD_MARGIN_UNAVAILABLE", report.GateNote, StringComparison.Ordinal);
        Assert.Equal("product-prompt-b002-expanded-train-ready", report.BatchId);
        Assert.Equal(16, report.HeldOutRows);
        Assert.True(report.RuntimeProductAccuracy >= 0.95, $"Product accuracy was {report.RuntimeProductAccuracy:0.000}");
        Assert.True(report.RuntimeExactAccuracy >= 0.95, $"Exact accuracy was {report.RuntimeExactAccuracy:0.000}");
        Assert.Equal(0.0, report.RuntimeBootstrapRate);
        Assert.Equal(1.0, report.TargetScriptAccuracy);
        Assert.Equal(0.0, report.CopiedSourceRate);
        Assert.Equal(0.0, report.WrongDirectionRate);
        Assert.Equal(1.0, report.EmojiPreservationAccuracy);
        Assert.Equal(1.0, report.NamedEntityPreservationAccuracy);
        Assert.All(report.CaseResults, row => Assert.True(row.ProductMatch, row.CaseId));
    }

    [Fact]
    public void FrequencyWordListBuilder_FiltersToCommercialCorePosAndDeduplicatesLemma()
    {
        var rows = new[]
        {
            new FrequencyWordSourceRow("run", "run", "VERB", 1, "gsl-001", "public-domain", true),
            new FrequencyWordSourceRow("the", "the", "DET", 2, "gsl-002", "public-domain", true),
            new FrequencyWordSourceRow("runner", "run", "NOUN", 3, "gsl-003", "public-domain", true),
            new FrequencyWordSourceRow("private", "private", "ADJ", 4, "unknown-004", "unknown", true),
            new FrequencyWordSourceRow("market", "market", "NOUN", 5, "gsl-005", "cc-by", true),
            new FrequencyWordSourceRow("share", "share", "VERB", 6, "nc-006", "cc-by-nc", false)
        };

        var result = FrequencyWordListBuilder.Build(
            rows,
            new FrequencyWordListBuildOptions("en", Limit: 3));

        Assert.Equal(2, result.Entries.Count);
        Assert.Equal("run", result.Entries[0].Lemma);
        Assert.Equal("market", result.Entries[1].Lemma);
        Assert.Equal("VERB", result.Entries[0].Pos);
        Assert.Equal("public_domain", result.Entries[0].License);
        Assert.Equal(1, result.RejectionCounts["pos_not_allowed"]);
        Assert.Equal(1, result.RejectionCounts["duplicate_lemma"]);
        Assert.Equal(1, result.RejectionCounts["license_not_allowed"]);
        Assert.Equal(1, result.RejectionCounts["commercial_not_allowed"]);
        Assert.Contains(result.RejectedRows, row => row.Word == "share");
    }

    [Fact]
    public void FrequencyWordListBuilder_ParsesDelimitedSourceAndWritesGitSafeJsonl()
    {
        var source = string.Join(
            "\n",
            [
                "word\tlemma\tpos\trank\tsource_id\tlicense\tcommercial_allowed",
                "start\tstart\tVERB\t1\tgsl-001\tpublic_domain\ttrue",
                "careful\tcareful\tADJ\t2\tgsl-002\tcc_by\ttrue",
                "only\tonly\tADV\t3\tgsl-003\tcc_by_2fr\ttrue"
            ]);

        var rows = FrequencyWordListBuilder.ParseDelimited(source);
        var result = FrequencyWordListBuilder.Build(
            rows,
            new FrequencyWordListBuildOptions("en", Limit: 3));
        var jsonl = FrequencyWordListBuilder.ToJsonl(result.Entries);

        Assert.Equal(3, rows.Count);
        Assert.Equal(3, result.Entries.Count);
        Assert.Contains("\"word\":\"start\"", jsonl);
        Assert.Contains("\"language\":\"en\"", jsonl);
        Assert.Contains("\"source_id\":\"gsl-001\"", jsonl);
        Assert.DoesNotContain("chosen", jsonl, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("expected_translation", jsonl, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task TatoebaPilotLoopRunner_FiltersTestRowsAboveSeventyPercentTrainingOverlap()
    {
        var root = Path.Combine(Path.GetTempPath(), "xiigen-tatoeba-loop-test", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var privatePairsPath = Path.Combine(root, "sentence-pairs-ru-en-pilot.jsonl");
        var serializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
        var rows = new[]
        {
            BuildPrivatePair(1, 101, "alpha beta gamma", "Alpha beta gamma", "OLD"),
            BuildPrivatePair(2, 102, "orange river stone", "Orange river stone", "OLD"),
            BuildPrivatePair(3, 103, "alpha beta gamma", "Duplicate should be skipped", "OLD"),
            BuildPrivatePair(4, 104, "alpha beta gamma delta", "Similar should be skipped", "OLD2"),
            BuildPrivatePair(5, 105, "silver moon rises", "Silver moon rises", "NEW1"),
            BuildPrivatePair(6, 106, "green field waits", "Green field waits", "NEW2")
        };

        await File.WriteAllLinesAsync(
            privatePairsPath,
            rows.Select(row => JsonSerializer.Serialize(row, serializerOptions)));

        var report = await TatoebaPilotLoopRunner.RunAsync(new TatoebaPilotLoopRunOptions(
            PrivatePairsPath: privatePairsPath,
            TrainSetSize: 2,
            TestSetSize: 2,
            MaxCycles: 1,
            TargetAccuracy: 0.95));

        var cycle = Assert.Single(report.Cycles);
        Assert.Equal(2, cycle.TestRows);
        Assert.Equal(4, cycle.CandidateTestRowsScanned);
        Assert.Equal(1, cycle.FilteredExactOverlapRows);
        Assert.Equal(1, cycle.FilteredSimilarityOverlapRows);
        Assert.Equal(0, cycle.TestSelectionShortfall);
        Assert.Equal(1.0, cycle.NewPatternRate);
    }

    [Fact]
    public async Task TatoebaPilotLoopRunner_AdaptiveGatePromotesWhenNewPatternAboveThresholdAndSeenTokensBelowThreshold()
    {
        var root = Path.Combine(Path.GetTempPath(), "xiigen-tatoeba-loop-test", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var privatePairsPath = Path.Combine(root, "sentence-pairs-ru-en-pilot.jsonl");
        var serializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
        var rows = new[]
        {
            BuildPrivatePair(1, 101, "alpha beta gamma delta", "Alpha beta gamma delta", "OLD"),
            BuildPrivatePair(2, 102, "orange river stone cloud", "Orange river stone cloud", "OLD"),
            BuildPrivatePair(3, 103, "alpha beta gamma delta", "Duplicate should be skipped", "OLD"),
            BuildPrivatePair(4, 104, "alpha beta silver moon", "Alpha beta silver moon", "NEW1"),
            BuildPrivatePair(5, 105, "gamma delta silver comet", "Gamma delta silver comet", "NEW2"),
            BuildPrivatePair(6, 106, "orange river silver moon", "Orange river silver moon", "OLD"),
            BuildPrivatePair(7, 107, "alpha gamma orange comet", "Alpha gamma orange comet", "OLD")
        };

        await File.WriteAllLinesAsync(
            privatePairsPath,
            rows.Select(row => JsonSerializer.Serialize(row, serializerOptions)));

        var report = await TatoebaPilotLoopRunner.RunAsync(new TatoebaPilotLoopRunOptions(
            PrivatePairsPath: privatePairsPath,
            TrainSetSize: 2,
            TestSetSize: 4,
            MaxCycles: 1,
            TargetAccuracy: 0.75));

        var cycle = Assert.Single(report.Cycles);
        Assert.Equal(4, cycle.TestRows);
        Assert.True(cycle.NewPatternRate > 0.22);
        Assert.True(cycle.SourceTokenSeenRate < 0.78);
        Assert.True(cycle.PromotedToTraining);
        Assert.Equal("PROMOTE_NEW_PATTERN_GT_22_AND_SEEN_TOKENS_LT_78", cycle.PromotionDecision);
    }

    [Fact]
    public async Task TatoebaPilotLoopRunner_AdaptiveSelectorSkipsRejectedCandidateAndPromotesLaterSet()
    {
        var root = Path.Combine(Path.GetTempPath(), "xiigen-tatoeba-loop-test", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var privatePairsPath = Path.Combine(root, "sentence-pairs-en-ru-pilot.jsonl");
        var checkpointPath = Path.Combine(root, "translation-checkpoint.json");
        var serializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
        try
        {
            var rows = new[]
            {
                BuildPrivatePair(1, 101, "known alpha one", "Known alpha one", "OLD_A", "EN_RU"),
                BuildPrivatePair(2, 102, "known beta two", "Known beta two", "OLD_B", "EN_RU"),
                BuildPrivatePair(3, 103, "known beta alpha", "Known beta alpha", "OLD_A", "EN_RU"),
                BuildPrivatePair(4, 104, "one two alpha beta", "One two alpha beta", "OLD_B", "EN_RU"),
                BuildPrivatePair(5, 105, "silver moon rises", "Silver moon rises", "NEW_C", "EN_RU"),
                BuildPrivatePair(6, 106, "green field waits", "Green field waits", "NEW_D", "EN_RU")
            };
            await File.WriteAllLinesAsync(
                privatePairsPath,
                rows.Select(row => JsonSerializer.Serialize(row, serializerOptions)));

            var report = await TatoebaPilotLoopRunner.RunAsync(new TatoebaPilotLoopRunOptions(
                PrivatePairsPath: privatePairsPath,
                TrainSetSize: 2,
                TestSetSize: 2,
                MaxCycles: 1,
                TargetAccuracy: 0.95,
                CheckpointPath: checkpointPath));

            var cycle = Assert.Single(report.Cycles);
            Assert.Equal(4, cycle.CandidateTestRowsScanned);
            Assert.Equal(2, cycle.NoveltyScanSkippedRows);
            Assert.Equal("adaptive_promotable_scan", cycle.CandidateSelectionMode);
            Assert.True(cycle.PromotedToTraining);
            Assert.Equal("PROMOTE_NEW_PATTERN_GT_22_AND_SEEN_TOKENS_LT_78", cycle.PromotionDecision);
            Assert.Equal(1.0, cycle.NewPatternRate);
            Assert.Equal(0.0, cycle.SourceTokenSeenRate);
            Assert.DoesNotContain(cycle.Rows, row => row.SourceSentenceId is 3 or 4);
            Assert.Contains(cycle.Rows, row => row.SourceSentenceId == 5);
            Assert.Contains(cycle.Rows, row => row.SourceSentenceId == 6);
            Assert.Equal(4, report.AcceptedTrainingRows);
            Assert.Equal(2, report.SkippedRows);
            Assert.Equal(6, report.EndCursor);

            var checkpointJson = await File.ReadAllTextAsync(checkpointPath, System.Text.Encoding.UTF8);
            var checkpoint = JsonSerializer.Deserialize<TranslationMachineCheckpoint>(
                checkpointJson,
                TranslationCheckpointJsonOptions.Standard);
            Assert.NotNull(checkpoint?.TatoebaLoop);
            Assert.Contains(checkpoint!.TatoebaLoop!.SkippedRows, row => row.SourceSentenceId == 3);
            Assert.Contains(checkpoint.TatoebaLoop.SkippedRows, row => row.SourceSentenceId == 4);
            Assert.DoesNotContain(checkpoint.TatoebaLoop.AcceptedTrainingRows, row => row.SourceSentenceId is 3 or 4);
            Assert.Contains(checkpoint.TatoebaLoop.AcceptedTrainingRows, row => row.SourceSentenceId == 5);
            Assert.Contains(checkpoint.TatoebaLoop.AcceptedTrainingRows, row => row.SourceSentenceId == 6);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task TatoebaPilotLoopRunner_AdaptiveGateSkipsWhenNewPatternPassesButSeenTokensFails()
    {
        var root = Path.Combine(Path.GetTempPath(), "xiigen-tatoeba-loop-test", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var privatePairsPath = Path.Combine(root, "sentence-pairs-ru-en-pilot.jsonl");
        var serializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
        var rows = new[]
        {
            BuildPrivatePair(1, 101, "alpha beta gamma delta", "Alpha beta gamma delta", "OLD"),
            BuildPrivatePair(2, 102, "orange river stone cloud", "Orange river stone cloud", "OLD"),
            BuildPrivatePair(3, 103, "alpha orange beta river", "Alpha orange beta river", "NEW1"),
            BuildPrivatePair(4, 104, "gamma stone delta cloud", "Gamma stone delta cloud", "OLD"),
            BuildPrivatePair(5, 105, "alpha stone beta cloud", "Alpha stone beta cloud", "OLD"),
            BuildPrivatePair(6, 106, "gamma orange delta river", "Gamma orange delta river", "OLD")
        };

        await File.WriteAllLinesAsync(
            privatePairsPath,
            rows.Select(row => JsonSerializer.Serialize(row, serializerOptions)));

        var report = await TatoebaPilotLoopRunner.RunAsync(new TatoebaPilotLoopRunOptions(
            PrivatePairsPath: privatePairsPath,
            TrainSetSize: 2,
            TestSetSize: 4,
            MaxCycles: 1,
            TargetAccuracy: 0.75));

        var cycle = Assert.Single(report.Cycles);
        Assert.Equal(4, cycle.TestRows);
        Assert.Equal(0.25, cycle.NewPatternRate);
        Assert.Equal(1.0, cycle.SourceTokenSeenRate);
        Assert.False(cycle.PromotedToTraining);
        Assert.Equal("SKIP_REQUIRES_NEW_PATTERN_GT_22_AND_SEEN_TOKENS_LT_78", cycle.PromotionDecision);
    }

    [Fact]
    public async Task TatoebaPilotLoopRunner_AdaptiveGateSkipsWhenNewPatternFailsButSeenTokensPasses()
    {
        var root = Path.Combine(Path.GetTempPath(), "xiigen-tatoeba-loop-test", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var privatePairsPath = Path.Combine(root, "sentence-pairs-ru-en-pilot.jsonl");
        var serializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
        var rows = new[]
        {
            BuildPrivatePair(1, 101, "alpha beta gamma delta", "Alpha beta gamma delta", "OLD"),
            BuildPrivatePair(2, 102, "orange river stone cloud", "Orange river stone cloud", "OLD"),
            BuildPrivatePair(3, 103, "alpha beta silver moon", "Alpha beta silver moon", "OLD"),
            BuildPrivatePair(4, 104, "gamma delta silver comet", "Gamma delta silver comet", "OLD"),
            BuildPrivatePair(5, 105, "orange river silver moon", "Orange river silver moon", "OLD"),
            BuildPrivatePair(6, 106, "alpha gamma orange comet", "Alpha gamma orange comet", "OLD")
        };

        await File.WriteAllLinesAsync(
            privatePairsPath,
            rows.Select(row => JsonSerializer.Serialize(row, serializerOptions)));

        var report = await TatoebaPilotLoopRunner.RunAsync(new TatoebaPilotLoopRunOptions(
            PrivatePairsPath: privatePairsPath,
            TrainSetSize: 2,
            TestSetSize: 4,
            MaxCycles: 1,
            TargetAccuracy: 0.75));

        var cycle = Assert.Single(report.Cycles);
        Assert.Equal(4, cycle.TestRows);
        Assert.True(cycle.NewPatternRate <= 0.22);
        Assert.True(cycle.SourceTokenSeenRate < 0.78);
        Assert.False(cycle.PromotedToTraining);
        Assert.Equal("SKIP_REQUIRES_NEW_PATTERN_GT_22_AND_SEEN_TOKENS_LT_78", cycle.PromotionDecision);
    }

    [Fact]
    public async Task TatoebaPilotLoopRunner_AdaptiveGateSkipsWhenBothThresholdsFail()
    {
        var root = Path.Combine(Path.GetTempPath(), "xiigen-tatoeba-loop-test", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var privatePairsPath = Path.Combine(root, "sentence-pairs-ru-en-pilot.jsonl");
        var serializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
        var rows = new[]
        {
            BuildPrivatePair(1, 101, "alpha beta gamma delta", "Alpha beta gamma delta", "OLD"),
            BuildPrivatePair(2, 102, "orange river stone cloud", "Orange river stone cloud", "OLD"),
            BuildPrivatePair(3, 103, "alpha orange beta river", "Alpha orange beta river", "OLD"),
            BuildPrivatePair(4, 104, "gamma stone delta cloud", "Gamma stone delta cloud", "OLD")
        };

        await File.WriteAllLinesAsync(
            privatePairsPath,
            rows.Select(row => JsonSerializer.Serialize(row, serializerOptions)));

        var report = await TatoebaPilotLoopRunner.RunAsync(new TatoebaPilotLoopRunOptions(
            PrivatePairsPath: privatePairsPath,
            TrainSetSize: 2,
            TestSetSize: 2,
            MaxCycles: 1,
            TargetAccuracy: 0.75));

        var cycle = Assert.Single(report.Cycles);
        Assert.Equal(2, cycle.TestRows);
        Assert.True(cycle.NewPatternRate <= 0.22);
        Assert.True(cycle.SourceTokenSeenRate >= 0.78);
        Assert.False(cycle.PromotedToTraining);
        Assert.Equal("SKIP_REQUIRES_NEW_PATTERN_GT_22_AND_SEEN_TOKENS_LT_78", cycle.PromotionDecision);
    }

    [Fact]
    public async Task TatoebaPilotLoopRunner_AdaptiveGateRestoresBestAccuracyFromCheckpoint()
    {
        var root = Path.Combine(Path.GetTempPath(), "xiigen-tatoeba-loop-test", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var privatePairsPath = Path.Combine(root, "sentence-pairs-ru-en-pilot.jsonl");
        var checkpointPath = Path.Combine(root, "translation-checkpoint.json");
        var serializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
        var rows = new[]
        {
            BuildPrivatePair(1, 101, "alpha beta gamma delta", "Alpha beta gamma delta", "OLD"),
            BuildPrivatePair(2, 102, "orange river stone cloud", "Orange river stone cloud", "OLD"),
            BuildPrivatePair(3, 103, "alpha beta silver moon", "Alpha beta silver moon", "OLD"),
            BuildPrivatePair(4, 104, "gamma delta silver comet", "Gamma delta silver comet", "NEW1"),
            BuildPrivatePair(5, 105, "orange river silver moon", "Orange river silver moon", "OLD"),
            BuildPrivatePair(6, 106, "alpha gamma orange comet", "Alpha gamma orange comet", "OLD"),
            BuildPrivatePair(7, 107, "beta stone amber field", "Beta stone amber field", "OLD")
        };

        await File.WriteAllLinesAsync(
            privatePairsPath,
            rows.Select(row => JsonSerializer.Serialize(row, serializerOptions)));

        var initialReport = await TatoebaPilotLoopRunner.RunAsync(new TatoebaPilotLoopRunOptions(
            PrivatePairsPath: privatePairsPath,
            TrainSetSize: 2,
            TestSetSize: 5,
            MaxCycles: 0,
            TargetAccuracy: 0.95,
            CheckpointPath: checkpointPath));

        Assert.Equal(0.78, initialReport.BestRuntimeProductAccuracy);

        var checkpointJson = await File.ReadAllTextAsync(checkpointPath, System.Text.Encoding.UTF8);
        var checkpoint = JsonSerializer.Deserialize<TranslationMachineCheckpoint>(
            checkpointJson,
            TranslationCheckpointJsonOptions.Standard);
        Assert.NotNull(checkpoint?.TatoebaLoop);
        var restoredBestCheckpoint = checkpoint! with
        {
            TatoebaLoop = checkpoint.TatoebaLoop! with { BestRuntimeProductAccuracy = 0.82 }
        };
        await File.WriteAllTextAsync(
            checkpointPath,
            JsonSerializer.Serialize(restoredBestCheckpoint, TranslationCheckpointJsonOptions.Standard),
            System.Text.Encoding.UTF8);

        var resumedReport = await TatoebaPilotLoopRunner.RunAsync(new TatoebaPilotLoopRunOptions(
            PrivatePairsPath: privatePairsPath,
            TrainSetSize: 2,
            TestSetSize: 5,
            MaxCycles: 1,
            TargetAccuracy: 0.95,
            CheckpointPath: checkpointPath,
            RequireCheckpointResume: true));

        Assert.True(resumedReport.CheckpointLoaded);
        var cycle = Assert.Single(resumedReport.Cycles);
        Assert.Equal(0.20, cycle.NewPatternRate);
        Assert.Equal(0.82, cycle.PromotionMaxAccuracy);
        Assert.Equal(0.18, cycle.MinimumPromotionNewPatternRate, precision: 10);
        Assert.Equal(0.82, cycle.MaximumPromotionSeenTokenRate);
        Assert.True(cycle.PromotedToTraining);
        Assert.Equal("PROMOTE_NEW_PATTERN_GT_18_AND_SEEN_TOKENS_LT_82", cycle.PromotionDecision);

        var savedCheckpointJson = await File.ReadAllTextAsync(checkpointPath, System.Text.Encoding.UTF8);
        var savedCheckpoint = JsonSerializer.Deserialize<TranslationMachineCheckpoint>(
            savedCheckpointJson,
            TranslationCheckpointJsonOptions.Standard);
        Assert.NotNull(savedCheckpoint?.TatoebaLoop);
        Assert.True(savedCheckpoint!.TatoebaLoop!.BestRuntimeProductAccuracy >= 0.82);
    }

    [Fact]
    public async Task TatoebaPilotLoopRunner_SkipCycles_RehydratesTrainingBeforeEmitting()
    {
        var root = Path.Combine(Path.GetTempPath(), "xiigen-tatoeba-loop-test", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var privatePairsPath = Path.Combine(root, "sentence-pairs-ru-en-pilot.jsonl");
        var serializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
        var rows = new[]
        {
            BuildPrivatePair(1, 101, "alpha beta", "Alpha beta", "OLD1"),
            BuildPrivatePair(2, 102, "orange river", "Orange river", "OLD2"),
            BuildPrivatePair(3, 103, "silver moon", "Silver moon", "NEW1"),
            BuildPrivatePair(4, 104, "green field", "Green field", "NEW2"),
            BuildPrivatePair(5, 105, "winter candle", "Winter candle", "NEW3"),
            BuildPrivatePair(6, 106, "summer window", "Summer window", "NEW4")
        };

        await File.WriteAllLinesAsync(
            privatePairsPath,
            rows.Select(row => JsonSerializer.Serialize(row, serializerOptions)));

        var report = await TatoebaPilotLoopRunner.RunAsync(new TatoebaPilotLoopRunOptions(
            PrivatePairsPath: privatePairsPath,
            TrainSetSize: 2,
            TestSetSize: 2,
            MaxCycles: 1,
            SkipCompletedCycles: 1,
            TargetAccuracy: 0.75));

        var cycle = Assert.Single(report.Cycles);
        Assert.Equal(1, report.SkipCompletedCycles);
        Assert.Equal(2, cycle.Cycle);
        Assert.Equal(4, cycle.TrainRows);
    }

    [Fact]
    public async Task TatoebaPilotCorpusBuilder_WritesGitSafeIndexesAndPrivatePairs()
    {
        var root = Path.Combine(Path.GetTempPath(), "xiigen-tatoeba-pilot-test", Guid.NewGuid().ToString("N"));
        var privateRoot = Path.Combine(root, "private");
        var outputRoot = Path.Combine(root, "source-data");
        Directory.CreateDirectory(root);

        var rusPath = Path.Combine(root, "rus_sentences.tsv");
        var engPath = Path.Combine(root, "eng_sentences.tsv");
        var linksPath = Path.Combine(root, "rus-eng_links.tsv");

        await File.WriteAllTextAsync(
            rusPath,
            string.Join(
                "\n",
                [
                    "1\trus\tОна быстро открыла дверь",
                    "2\trus\tОна быстро закрыла дверь",
                    "3\trus\tКот тихо открыл дверь",
                    "4\trus\tКот тихо закрыл дверь"
                ]));
        await File.WriteAllTextAsync(
            engPath,
            string.Join(
                "\n",
                [
                    "10\teng\tShe quickly opened the door",
                    "11\teng\tShe quickly closed the door",
                    "12\teng\tThe cat quietly opened the door",
                    "13\teng\tThe cat quietly closed the door"
                ]));
        await File.WriteAllTextAsync(
            linksPath,
            string.Join("\n", ["1\t10", "2\t11", "3\t12", "4\t13"]));

        var result = await TatoebaPilotCorpusBuilder.BuildAsync(new TatoebaPilotBuildOptions(
            RussianSentencesPath: rusPath,
            EnglishSentencesPath: engPath,
            RussianEnglishLinksPath: linksPath,
            OutputDirectory: outputRoot,
            PrivateOutputDirectory: privateRoot,
            WordLimit: 2,
            SentencesPerWord: 2));

        Assert.True(result.MeetsPilotGate);
        Assert.Equal(2, result.EnglishWords);
        Assert.Equal(2, result.RussianWords);
        Assert.Equal(4, result.EnglishSentenceIndexRows);
        Assert.Equal(4, result.RussianSentenceIndexRows);

        var index = await File.ReadAllTextAsync(result.OutputPaths["sentence_index_ru_en"]);
        Assert.Contains("\"source_sentence_id\":1", index);
        Assert.Contains("\"target_sentence_id\":10", index);
        Assert.DoesNotContain("Она быстро открыла дверь", index);
        Assert.DoesNotContain("She quickly opened the door", index);

        var privatePairs = await File.ReadAllTextAsync(result.OutputPaths["private_pairs_ru_en"]);
        Assert.Contains("Она быстро открыла дверь", privatePairs);
        Assert.Contains("She quickly opened the door", privatePairs);

        var attribution = await File.ReadAllTextAsync(result.OutputPaths["attribution_manifest"]);
        Assert.Contains("Tatoeba", attribution);
        Assert.Contains("CC BY 2.0 France", attribution);
    }

    private static LearningBatch<TranslationDomain> BuildTrainingBatch(string batchId, int rowCount)
    {
        var items = Enumerable.Range(1, rowCount)
            .Select(i => new LearningTrainingItem(
                ItemId: $"item-{i:D3}",
                Prompt: $"Translate to English: sample {i}",
                ChosenResponse: $"Sample {i}",
                RejectedResponse: $"Bad {i}"))
            .ToList();

        return new LearningBatch<TranslationDomain>(
            BatchId: batchId,
            DomainTag: TranslationMachine.DomainTag,
            TrainingItems: items,
            SourceIds: ["test-source"]);
    }

    private static async Task<string> RunTranslationAsync(
        TranslationMachine machine,
        string prompt,
        string sourceLanguage = "ru",
        string targetLanguage = "en")
    {
        var orchestratorResult = await machine.GetOrchestratorAsync();
        Assert.True(orchestratorResult.IsSuccess, orchestratorResult.ErrorMessage);

        var result = await orchestratorResult.Value!.RunAsync(new LogicalOrchestratorRequest(
            RunId: $"translation-proof-{Guid.NewGuid():N}",
            ScenarioId: TranslationRoutingFlow.ScenarioId,
            UserRequest: prompt,
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>
            {
                ["source_language"] = sourceLanguage,
                ["target_language"] = targetLanguage,
                ["topic_domain"] = "general"
            }));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        return result.Value!.FinalHumanResult;
    }

    private static TranslationMachineCheckpoint BuildCheckpointWithExecutorMaps(
        TranslationDirection direction,
        IReadOnlyList<ExecutorWordMapEntryCheckpoint> wordMap,
        IReadOnlyList<ExecutorStringMapEntryCheckpoint> surfaceWordMap,
        IReadOnlyList<ExecutorStringMapEntryCheckpoint> phraseMap,
        // Точная память задаётся парой «тема и источник»: это текущая форма
        // сохранённого состояния. Прежний список, адресованный только текстом
        // источника, здесь больше не принимается — отдельные устаревшие наборы
        // данных строят его сами и проверяют установленный отказ загрузки.
        IReadOnlyList<ExecutorTopicSourceMapEntryCheckpoint> topicSourceMap)
    {
        var export = new TranslationMachine().ExportCheckpoint();
        Assert.True(export.IsSuccess, export.ErrorMessage);

        var executors = export.Value!.Executors
            .Select(executor => executor.Direction == direction
                ? executor with
                {
                    WordMap = wordMap,
                    SurfaceWordMap = surfaceWordMap,
                    PhraseMap = phraseMap,
                    // Старая карта источника у текущей формы обязана быть пустой:
                    // авторитетный набор точной памяти должен быть один.
                    ExactSourceMap = [],
                    TopicSourceMap = topicSourceMap
                }
                : executor)
            .ToList();

        return export.Value! with
        {
            HasStructuredTraining = true,
            Executors = executors
        };
    }

    private static TranslationLearningExample BuildSemanticExample(
        string id,
        string sourceText,
        string signature)
    {
        return BuildExample(
            id,
            TranslationUnitIds.SemanticTokenizer,
            new TokenizerInput(sourceText, "ru"),
            new TokenizerOutput(SentencePattern.SVO, signature),
            AddWrong(
                JsonSerializer.Serialize(new TokenizerOutput(SentencePattern.UNKNOWN, $"WRONG|{signature}"), JsonOptions),
                "semantic_signature_loss"),
            "semantic_signature_loss",
            signature);
    }

    private static TranslationLearningExample BuildExecutorExample(
        string id,
        string sourceText,
        string signature,
        string chosen,
        string rejected,
        string rejectedType)
    {
        return BuildExample(
            id,
            TranslationUnitIds.ExecutorRuEn,
            new ExecutorInput(sourceText, "ru", "general"),
            new ExecutorOutput(chosen, 0.93, null),
            AddWrong(
                JsonSerializer.Serialize(new ExecutorOutput(rejected, 0.31, null), JsonOptions),
                rejectedType),
            rejectedType,
            signature);
    }

    private static TranslationLearningExample BuildExecutorExampleWithMappings(
        string id,
        string sourceText,
        string signature,
        string chosen,
        string rejectedType,
        IReadOnlyList<ExecutorWordMapping> wordMappings)
    {
        return BuildExample(
            id,
            TranslationUnitIds.ExecutorRuEn,
            new ExecutorInput(sourceText, "ru", "general"),
            new ExecutorOutput(chosen, 0.93, wordMappings),
            AddWrong(
                JsonSerializer.Serialize(new ExecutorOutput("add check null to method ValidateToken", 0.31, null), JsonOptions),
                rejectedType),
            rejectedType,
            signature);
    }

    private static TranslationLearningExample BuildDirectionalExecutorExampleWithMappings(
        string id,
        TranslationDirection direction,
        string sourceLang,
        string sourceText,
        string signature,
        string chosen,
        string rejectedType,
        IReadOnlyList<ExecutorWordMapping> wordMappings)
    {
        return BuildExample(
            id,
            TranslationUnitIds.ExecutorFor(direction),
            new ExecutorInput(sourceText, sourceLang, "general"),
            new ExecutorOutput(chosen, 0.93, wordMappings),
            AddWrong(
                JsonSerializer.Serialize(new ExecutorOutput("wrong target-script output", 0.31, null), JsonOptions),
                rejectedType),
            rejectedType,
            signature);
    }

    private static TranslationLearningExample BuildDirectionalExecutorExampleWithWordAndPhraseMappings(
        string id,
        TranslationDirection direction,
        string sourceLang,
        string sourceText,
        string signature,
        string chosen,
        string rejectedType,
        IReadOnlyList<ExecutorWordMapping> wordMappings,
        IReadOnlyList<ExecutorPhraseMapping> phraseMappings)
    {
        return BuildExample(
            id,
            TranslationUnitIds.ExecutorFor(direction),
            new ExecutorInput(sourceText, sourceLang, "general"),
            new ExecutorOutput(chosen, 0.93, wordMappings, phraseMappings),
            AddWrong(
                JsonSerializer.Serialize(new ExecutorOutput("wrong target-script output", 0.31, null), JsonOptions),
                rejectedType),
            rejectedType,
            signature);
    }

    private static TranslationLearningExample BuildVerifierExample(
        string id,
        string sourceText,
        string candidate)
    {
        return BuildExample(
            id,
            TranslationUnitIds.QualityVerifier,
            new VerifierInput(sourceText, candidate),
            new VerifierOutput(0.96, [], true, null),
            AddWrong(
                JsonSerializer.Serialize(new VerifierOutput(0.42, ["missed_quality"], false, candidate), JsonOptions),
                "verifier_false_rejection"),
            "verifier_false_rejection",
            $"VERIFY|{id}");
    }

    private static TranslationLearningExample BuildVerifierFailureExample(
        string id,
        string sourceText,
        string signature,
        string rejectedCandidate)
    {
        return BuildExample(
            id,
            TranslationUnitIds.QualityVerifier,
            new VerifierInput(sourceText, rejectedCandidate),
            new VerifierOutput(
                0.42,
                ["expected 'ProcessQueue' for concept CODE_IDENTIFIER in context UNKNOWN"],
                false,
                "Add a null check to the ValidateToken method and do not rename ProcessQueue."),
            AddWrong(
                JsonSerializer.Serialize(new VerifierOutput(0.91, [], true, null), JsonOptions),
                "verifier_false_acceptance"),
            "verifier_false_acceptance",
            signature);
    }

    private static TranslationLearningExample BuildExample<TInput, TOutput>(
        string id,
        string unitId,
        TInput input,
        TOutput chosen,
        string rejectedJson,
        string rejectedType,
        string signature)
    {
        return new TranslationLearningExample(
            ExampleId: id,
            UnitId: unitId,
            FlowId: TranslationRoutingFlow.FlowId,
            InputJson: JsonSerializer.Serialize(input, JsonOptions),
            ChosenOutputJson: JsonSerializer.Serialize(chosen, JsonOptions),
            RejectedOutputJson: rejectedJson,
            RejectedType: rejectedType,
            SemanticSignature: signature,
            SourceId: "unit-proof",
            HeldOut: false);
    }

    private static string AddWrong(string json, string rejectedType)
    {
        return json.TrimEnd('}') + $",\"wrong\":\"[WRONG: {rejectedType} - near miss for unit-level learning.]\"}}";
    }

    private static LogicalFlowRequest<TranslationRoutingRequest> BuildFlowRequest(
        string runId,
        string sourceLang,
        string targetLang,
        string inputText)
    {
        var translationRequest = new TranslationRoutingRequest(
            SourceLanguage: sourceLang,
            TargetLanguage: targetLang,
            TopicDomain: "general",
            InputText: inputText);

        return new LogicalFlowRequest<TranslationRoutingRequest>(
            RunId: runId,
            FlowId: TranslationRoutingFlow.FlowId,
            ScenarioId: TranslationRoutingFlow.ScenarioId,
            Input: translationRequest,
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>());
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private static TranslationTrainReadyPair BuildTrainReadyPairSample()
    {
        return new TranslationTrainReadyPair(
            Id: "ru-en-general-l1-b029-001",
            Source: "ru-en-general",
            Direction: "RU_EN",
            PairFamily: "general-adverb-translation",
            Prompt: "Translate the following Russian sentence to English:",
            ASide: "Он бесстрашно двинулся через враждебную толпу, не выдав ни малейшего страха.",
            Chosen: "He intrepidly advanced through the hostile crowd, betraying not a trace of fear!",
            Rejected: "[WRONG: fearless_quality_missed - lost the quality of fearless, undaunted determination.]",
            ValidationOutcome: "train_ready",
            ValidationErrors: []);
    }

    private static IReadOnlyList<TranslationTrainReadyPair> BuildBatch029AdverbRepairRows()
    {
        return
        [
            new TranslationTrainReadyPair(
                Id: "ru-en-general-l1-repair-astutely-001",
                Source: "ru-en-general-l1-repair-adverb-markers",
                Direction: "RU_EN",
                PairFamily: "general-adverb-repair",
                Prompt: "Translate the following Russian sentence to English:",
                ASide: "Инвестор дальновидно оценил риск до подписания договора.",
                Chosen: "The investor astutely assessed the risk before signing the contract!",
                Rejected: "[WRONG: strategic_foresight_lost - drops the astute, forward-looking manner marker carried by 'дальновидно'.]",
                ValidationOutcome: "train_ready",
                ValidationErrors: []),
            new TranslationTrainReadyPair(
                Id: "ru-en-general-l1-repair-pompously-001",
                Source: "ru-en-general-l1-repair-adverb-markers",
                Direction: "RU_EN",
                PairFamily: "general-adverb-repair",
                Prompt: "Translate the following Russian sentence to English:",
                ASide: "Докладчик напыщенно представил свой план перед аудиторией.",
                Chosen: "The speaker pompously presented his plan before the audience!",
                Rejected: "[WRONG: self_importance_register_lost - drops the inflated self-important manner marker carried by 'напыщенно'.]",
                ValidationOutcome: "train_ready",
                ValidationErrors: []),
            new TranslationTrainReadyPair(
                Id: "ru-en-general-l1-repair-peevishly-001",
                Source: "ru-en-general-l1-repair-adverb-markers",
                Direction: "RU_EN",
                PairFamily: "general-adverb-repair",
                Prompt: "Translate the following Russian sentence to English:",
                ASide: "Она раздражённо ответила на замечание и закрыла ноутбук.",
                Chosen: "She peevishly answered the remark and closed the laptop!",
                Rejected: "[WRONG: irritable_quality_lost - drops the touchy irritated manner marker carried by 'раздражённо'.]",
                ValidationOutcome: "train_ready",
                ValidationErrors: [])
        ];
    }

    private static string GetRepoTrainingDataPath(string fileName)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(
                directory.FullName,
                "27.TranslationDomain",
                "TrainingData",
                fileName);
            if (File.Exists(candidate))
                return candidate;

            directory = directory.Parent;
        }

        return Path.Combine(
            AppContext.BaseDirectory,
            "27.TranslationDomain",
            "TrainingData",
            fileName);
    }

    private static TatoebaPrivateSentencePair BuildPrivatePair(
        int sourceSentenceId,
        int targetSentenceId,
        string sourceText,
        string targetText,
        string sourcePattern,
        string direction = "RU_EN")
    {
        var sourceLanguage = direction.Equals("EN_RU", StringComparison.OrdinalIgnoreCase) ? "en" : "ru";
        return new TatoebaPrivateSentencePair(
            Direction: direction,
            Word: sourceText.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0],
            Language: sourceLanguage,
            SourceSentenceId: sourceSentenceId,
            TargetSentenceId: targetSentenceId,
            SourceText: sourceText,
            TargetText: targetText,
            SourcePattern: sourcePattern,
            TargetPattern: sourcePattern,
            SourceId: "test",
            License: "cc_by_2fr");
    }

    private static async Task<TranslationProductHeldOutCase[]> LoadProductHeldOutCasesAsync(string path)
    {
        return (await File.ReadAllLinesAsync(path))
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonSerializer.Deserialize<TranslationProductHeldOutCase>(line, JsonOptions))
            .Where(row => row is not null)
            .Select(row => row!)
            .ToArray();
    }

    private static int CountTrainHeldOutOverlaps(
        IReadOnlyList<TranslationTrainReadyPair> trainRows,
        IReadOnlyList<TranslationProductHeldOutCase> heldOutRows)
    {
        var trainInputs = trainRows
            .Select(row => NormalizeOverlapKey(row.ASide))
            .Where(key => !string.IsNullOrWhiteSpace(key))
            .ToHashSet(StringComparer.Ordinal);

        return heldOutRows.Count(row => trainInputs.Contains(NormalizeOverlapKey(row.InputText)));
    }

    private static string NormalizeOverlapKey(string value)
        => string.Join(" ", (value ?? string.Empty)
                .Trim()
                .ToLowerInvariant()
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static string BuildProductGateFailureMessage(TranslationProductHeldOutRunReport report)
    {
        var failedRows = report.CaseResults
            .Where(row => !row.ProductMatch || row.BootstrapFallback || !row.ExactMatch)
            .Select(row =>
                $"{row.CaseId} {row.Direction}: exact={row.ExactMatch}, product={row.ProductMatch}, bootstrap={row.BootstrapFallback}, script={row.TargetScriptOk}, copied={row.CopiedSource}, actual='{row.ActualTranslation}'")
            .ToArray();

        return $"{report.GateNote} exact={report.RuntimeExactAccuracy:0.000}, product={report.RuntimeProductAccuracy:0.000}, bootstrap={report.RuntimeBootstrapRate:0.000}, script={report.TargetScriptAccuracy:0.000}, copied={report.CopiedSourceRate:0.000}, wrong={report.WrongDirectionRate:0.000}. Failed: {string.Join(" | ", failedRows)}";
    }

    // SaveAndReloadAsync saves the machine to path and returns path; used by checkpoint round-trip tests.
    // Asserts save and load both succeed so failing tests surface a clear message.
    private static async Task<string> SaveAndReloadAsync(TranslationMachine machine, string path)
    {
        // Save the current machine state to the given path.
        var saveResult = await machine.SaveCheckpointAsync(path);
        Assert.True(saveResult.IsSuccess, $"Save failed: {saveResult.ErrorMessage}");
        // Load a fresh machine from the saved checkpoint.
        var loadResult = await TranslationMachine.LoadCheckpointAsync(path);
        Assert.True(loadResult.IsSuccess, $"Load failed: {loadResult.ErrorMessage}");
        // Return path for re-use in further assertions.
        return path;
    }

    [Fact]
    public async Task TranslationMachine_CheckpointRoundTrip_KnnStorePair()
    {
        // Test 1: trained word mapping survives save→load cycle with no BOOTSTRAP fallback.
        var tmpPath = Path.GetTempFileName() + ".json";
        try
        {
            // Train a machine with one executor example carrying word mappings.
            var machine = new TranslationMachine();
            var source = "добавить проверку";
            var batch = new TranslationLearningBatch(
                "checkpoint-knn-001", TranslationMachine.DomainTag,
                [BuildExecutorExampleWithMappings("knn-pair", source,
                    "IMPERATIVE_V|ADD_OP|VALIDATE_CHECK", "add check",
                    "wrong_word",
                    [new ExecutorWordMapping("добавить", SemanticConcept.ADD_OP, "add"),
                     new ExecutorWordMapping("проверку", SemanticConcept.VALIDATE_CHECK, "check")])],
                ["checkpoint-test"]);
            Assert.True((await machine.TrainStructuredBatchAsync(batch)).IsSuccess);
            // Save then load from the checkpoint file.
            await SaveAndReloadAsync(machine, tmpPath);
            var loadResult = await TranslationMachine.LoadCheckpointAsync(tmpPath);
            Assert.True(loadResult.IsSuccess);
            // Run translation on the loaded machine — must not fall back to Bootstrap.
            var actual = await RunTranslationAsync(loadResult.Value!.Machine, source);
            Assert.DoesNotContain("[RU_EN:BOOTSTRAP]", actual);
            Assert.Contains("add", actual, StringComparison.OrdinalIgnoreCase);
        }
        finally { if (File.Exists(tmpPath)) File.Delete(tmpPath); }
    }

    [Fact]
    public async Task TranslationMachine_CheckpointRoundTrip_ExecutorWordMap()
    {
        // Test 2: "запустить поток" → "launch thread" word mapping survives save→load.
        var tmpPath = Path.GetTempFileName() + ".json";
        try
        {
            // Train a machine with a unique source sentence word mapping.
            var machine = new TranslationMachine();
            var source = "запустить поток";
            var batch = new TranslationLearningBatch(
                "checkpoint-wordmap-001", TranslationMachine.DomainTag,
                [BuildExecutorExampleWithMappings("wordmap-pair", source,
                    "IMPERATIVE_V|LAUNCH_OP|THREAD_REF", "launch thread",
                    "wrong_word",
                    [new ExecutorWordMapping("запустить", SemanticConcept.LAUNCH_OP, "launch"),
                     new ExecutorWordMapping("поток", SemanticConcept.THREAD_REF, "thread")])],
                ["checkpoint-test"]);
            Assert.True((await machine.TrainStructuredBatchAsync(batch)).IsSuccess);
            // Save and load from checkpoint.
            await SaveAndReloadAsync(machine, tmpPath);
            var loadResult = await TranslationMachine.LoadCheckpointAsync(tmpPath);
            Assert.True(loadResult.IsSuccess);
            // Translation on loaded machine must preserve the trained word.
            var actual = await RunTranslationAsync(loadResult.Value!.Machine, source);
            Assert.DoesNotContain("[RU_EN:BOOTSTRAP]", actual);
            Assert.Contains("launch", actual, StringComparison.OrdinalIgnoreCase);
        }
        finally { if (File.Exists(tmpPath)) File.Delete(tmpPath); }
    }

    [Fact]
    public async Task TranslationMachine_CheckpointRoundTrip_ExecutorPhraseMap()
    {
        // Test 3: phrase mapping survives save→load and produces non-Bootstrap output.
        var tmpPath = Path.GetTempFileName() + ".json";
        try
        {
            // Build a batch via TranslationStructuredBatchBuilder so phrase paths are exercised.
            var machine = new TranslationMachine();
            var pair = new TranslationTrainReadyPair(
                Id: "ckpt-phrase-001",
                Source: "checkpoint-phrase",
                Direction: "RU_EN",
                PairFamily: "checkpoint-test",
                Prompt: "Translate the following Russian sentence to English:",
                ASide: "добавить запись данных",
                Chosen: "add data record",
                Rejected: "[PHRASE: добавить запись данных => add data record] [WRONG: phrase_boundary_lost - literal boundary missed.]",
                ValidationOutcome: "train_ready",
                ValidationErrors: []);
            var batch = TranslationStructuredBatchBuilder.BuildRuEnGeneralBatch(
                "checkpoint-phrase-batch", [pair], heldOut: false);
            Assert.True((await machine.TrainStructuredBatchAsync(batch)).IsSuccess);
            // Save and load checkpoint.
            await SaveAndReloadAsync(machine, tmpPath);
            var loadResult = await TranslationMachine.LoadCheckpointAsync(tmpPath);
            Assert.True(loadResult.IsSuccess);
            // Translate the same source — must not use Bootstrap fallback.
            var actual = await RunTranslationAsync(loadResult.Value!.Machine, "добавить запись данных");
            Assert.DoesNotContain("[RU_EN:BOOTSTRAP]", actual);
        }
        finally { if (File.Exists(tmpPath)) File.Delete(tmpPath); }
    }

    [Fact]
    public async Task TranslationMachine_CheckpointRoundTrip_ExecutorExactSource()
    {
        // Test 4: exact-source sentence mapping survives save→load and returns trained translation.
        var tmpPath = Path.GetTempFileName() + ".json";
        try
        {
            // Train with an exact source sentence so the exact-source map is populated.
            var machine = new TranslationMachine();
            var source = "записать данные в файл";
            var batch = new TranslationLearningBatch(
                "checkpoint-exact-001", TranslationMachine.DomainTag,
                [BuildExecutorExampleWithMappings("exact-pair", source,
                    "IMPERATIVE_V|WRITE_OP|DATA_ENTITY", "write data to file",
                    "wrong_word",
                    [new ExecutorWordMapping("записать", SemanticConcept.WRITE_OP, "write"),
                     new ExecutorWordMapping("данные", SemanticConcept.DATA_ENTITY, "data")])],
                ["checkpoint-test"]);
            Assert.True((await machine.TrainStructuredBatchAsync(batch)).IsSuccess);
            // Save and load checkpoint.
            await SaveAndReloadAsync(machine, tmpPath);
            var loadResult = await TranslationMachine.LoadCheckpointAsync(tmpPath);
            Assert.True(loadResult.IsSuccess);
            // Translate the exact source — must not return Bootstrap.
            var actual = await RunTranslationAsync(loadResult.Value!.Machine, source);
            Assert.DoesNotContain("[RU_EN:BOOTSTRAP]", actual);
        }
        finally { if (File.Exists(tmpPath)) File.Delete(tmpPath); }
    }

    [Fact]
    public async Task TranslationMachine_CheckpointRoundTrip_QualityVerifierConceptRule()
    {
        // Test 5: verifier rules survive save→load and the loaded machine translates without Bootstrap.
        var tmpPath = Path.GetTempFileName() + ".json";
        try
        {
            // Train a machine with both an executor example and a verifier example.
            var machine = new TranslationMachine();
            var source = "обработать поток";
            var executorBatch = new TranslationLearningBatch(
                "checkpoint-verifier-exec-001", TranslationMachine.DomainTag,
                [BuildExecutorExampleWithMappings("verifier-exec", source,
                    "IMPERATIVE_V|HANDLE_OP|STREAM_REF", "handle stream",
                    "wrong_word",
                    [new ExecutorWordMapping("обработать", SemanticConcept.HANDLE_OP, "handle"),
                     new ExecutorWordMapping("поток", SemanticConcept.STREAM_REF, "stream")])],
                ["checkpoint-test"]);
            Assert.True((await machine.TrainStructuredBatchAsync(executorBatch)).IsSuccess);
            // Add a verifier example so verifier rules are populated.
            var verifierBatch = new TranslationLearningBatch(
                "checkpoint-verifier-rule-001", TranslationMachine.DomainTag,
                [BuildVerifierExample("verifier-rule", source, "handle stream")],
                ["checkpoint-test"]);
            Assert.True((await machine.TrainStructuredBatchAsync(verifierBatch)).IsSuccess);
            // Save and load checkpoint.
            await SaveAndReloadAsync(machine, tmpPath);
            var loadResult = await TranslationMachine.LoadCheckpointAsync(tmpPath);
            Assert.True(loadResult.IsSuccess);
            // Translate on loaded machine — must not return Bootstrap.
            var actual = await RunTranslationAsync(loadResult.Value!.Machine, source);
            Assert.DoesNotContain("[RU_EN:BOOTSTRAP]", actual);
        }
        finally { if (File.Exists(tmpPath)) File.Delete(tmpPath); }
    }

    [Fact]
    public async Task TranslationMachine_LoadCheckpoint_FailsOnWrongSchemaVersion()
    {
        // Test 6: loading a checkpoint with schema_version != 1 must fail with CHECKPOINT_SCHEMA_UNSUPPORTED.
        var tmpPath = Path.GetTempFileName() + ".json";
        try
        {
            // Save a valid checkpoint from a fresh machine.
            var machine = new TranslationMachine();
            var saveResult = await machine.SaveCheckpointAsync(tmpPath);
            Assert.True(saveResult.IsSuccess);
            // Overwrite schema_version with an unsupported value in the raw JSON.
            var json = await File.ReadAllTextAsync(tmpPath, System.Text.Encoding.UTF8);
            json = json.Replace("\"schema_version\": 1", "\"schema_version\": 999");
            await File.WriteAllTextAsync(tmpPath, json, System.Text.Encoding.UTF8);
            // Load must fail with the correct error code.
            var loadResult = await TranslationMachine.LoadCheckpointAsync(tmpPath);
            Assert.False(loadResult.IsSuccess);
            Assert.Equal("CHECKPOINT_SCHEMA_UNSUPPORTED", loadResult.ErrorCode);
        }
        finally { if (File.Exists(tmpPath)) File.Delete(tmpPath); }
    }

    [Fact]
    public async Task TranslationMachine_LoadCheckpoint_FailsOnWrongMachineId()
    {
        // Test 7: loading a checkpoint with wrong machine_id must fail with MACHINE_ID_MISMATCH.
        var tmpPath = Path.GetTempFileName() + ".json";
        try
        {
            // Save a valid checkpoint from a fresh machine.
            var machine = new TranslationMachine();
            var saveResult = await machine.SaveCheckpointAsync(tmpPath);
            Assert.True(saveResult.IsSuccess);
            // Replace machine_id with a wrong value in the raw JSON.
            var json = await File.ReadAllTextAsync(tmpPath, System.Text.Encoding.UTF8);
            json = json.Replace("\"machine_id\": \"translation-machine\"", "\"machine_id\": \"wrong-machine\"");
            await File.WriteAllTextAsync(tmpPath, json, System.Text.Encoding.UTF8);
            // Load must fail with the correct error code.
            var loadResult = await TranslationMachine.LoadCheckpointAsync(tmpPath);
            Assert.False(loadResult.IsSuccess);
            Assert.Equal("MACHINE_ID_MISMATCH", loadResult.ErrorCode);
        }
        finally { if (File.Exists(tmpPath)) File.Delete(tmpPath); }
    }

    [Fact]
    public async Task TatoebaPilotLoopRunner_WritesCheckpointAfterPromotion()
    {
        // Test 8: runner writes a valid checkpoint file when CheckpointPath is configured.
        var tmpPairs = Path.GetTempFileName() + ".jsonl";
        var tmpCheckpoint = Path.GetTempFileName() + ".json";
        try
        {
            // Build a minimal pairs file with enough rows for TrainSetSize + TestSetSize.
            var pairs = Enumerable.Range(1, 20).Select(i => new TatoebaPrivateSentencePair(
                Direction: "RU_EN", Word: "добавить", Language: "ru",
                SourceSentenceId: i, TargetSentenceId: i + 1000,
                SourceText: $"добавить тест {i}", TargetText: $"add test {i}",
                SourcePattern: "IMPERATIVE_V", TargetPattern: "IMPERATIVE_V",
                SourceId: "test-pairs", License: "cc0"));
            // Serialize pairs as snake_case JSONL matching the runner's reader expectations.
            var serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            };
            var jsonLines = pairs.Select(p => JsonSerializer.Serialize(p, serializerOptions));
            await File.WriteAllLinesAsync(tmpPairs, jsonLines, System.Text.Encoding.UTF8);

            // Run the loop with a checkpoint path configured.
            await TatoebaPilotLoopRunner.RunAsync(
                new TatoebaPilotLoopRunOptions(
                    PrivatePairsPath: tmpPairs,
                    TrainSetSize: 5,
                    TestSetSize: 5,
                    MaxCycles: 2,
                    CheckpointPath: tmpCheckpoint));

            // Assert: checkpoint file was written at some point during the run.
            Assert.True(File.Exists(tmpCheckpoint), "Checkpoint file should exist after run.");

            // Deserialize and validate basic structure.
            var json = await File.ReadAllTextAsync(tmpCheckpoint, System.Text.Encoding.UTF8);
            var checkpoint = System.Text.Json.JsonSerializer.Deserialize<TranslationMachineCheckpoint>(
                json, TranslationCheckpointJsonOptions.Standard);
            Assert.NotNull(checkpoint);
            Assert.Equal("translation-machine", checkpoint!.MachineId);
            Assert.Equal(1, checkpoint.SchemaVersion);
        }
        finally
        {
            if (File.Exists(tmpPairs)) File.Delete(tmpPairs);
            if (File.Exists(tmpCheckpoint)) File.Delete(tmpCheckpoint);
        }
    }

    [Fact]
    public async Task TatoebaPilotLoopRunner_CheckpointPersistsAcceptedAndSkippedHistory()
    {
        var tmpPairs = Path.GetTempFileName() + ".jsonl";
        var promotedCheckpoint = Path.GetTempFileName() + ".promoted.json";
        var skippedCheckpoint = Path.GetTempFileName() + ".skipped.json";
        try
        {
            var serializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
            var promotedRows = new[]
            {
                BuildPrivatePair(1, 101, "alpha beta gamma delta", "Alpha beta gamma delta", "OLD"),
                BuildPrivatePair(2, 102, "orange river stone cloud", "Orange river stone cloud", "OLD"),
                BuildPrivatePair(3, 103, "alpha beta gamma delta", "Duplicate should be skipped", "OLD"),
                BuildPrivatePair(4, 104, "alpha beta silver moon", "Alpha beta silver moon", "NEW1"),
                BuildPrivatePair(5, 105, "gamma delta silver comet", "Gamma delta silver comet", "NEW2"),
                BuildPrivatePair(6, 106, "orange river silver moon", "Orange river silver moon", "OLD"),
                BuildPrivatePair(7, 107, "alpha gamma orange comet", "Alpha gamma orange comet", "OLD")
            };
            await File.WriteAllLinesAsync(
                tmpPairs,
                promotedRows.Select(pair => JsonSerializer.Serialize(pair, serializerOptions)));

            var promotedReport = await TatoebaPilotLoopRunner.RunAsync(
                new TatoebaPilotLoopRunOptions(
                    PrivatePairsPath: tmpPairs,
                    TrainSetSize: 2,
                    TestSetSize: 4,
                    MaxCycles: 1,
                    TargetAccuracy: 0.75,
                    CheckpointPath: promotedCheckpoint));

            Assert.Equal(6, promotedReport.AcceptedTrainingRows);
            Assert.Equal(0, promotedReport.SkippedRows);
            var promotedCheckpointJson = await File.ReadAllTextAsync(promotedCheckpoint, System.Text.Encoding.UTF8);
            var promotedMachineCheckpoint = JsonSerializer.Deserialize<TranslationMachineCheckpoint>(
                promotedCheckpointJson,
                TranslationCheckpointJsonOptions.Standard);
            Assert.NotNull(promotedMachineCheckpoint?.TatoebaLoop);
            Assert.Equal(6, promotedMachineCheckpoint!.TatoebaLoop!.AcceptedTrainingRows.Count);
            Assert.Empty(promotedMachineCheckpoint.TatoebaLoop.SkippedRows);

            var skippedRows = new[]
            {
                BuildPrivatePair(11, 111, "alpha beta gamma delta", "Alpha beta gamma delta", "OLD_A"),
                BuildPrivatePair(12, 112, "orange river stone cloud", "Orange river stone cloud", "OLD_B"),
                BuildPrivatePair(13, 113, "alpha orange beta river", "Alpha orange beta river", "OLD_A"),
                BuildPrivatePair(14, 114, "gamma stone delta cloud", "Gamma stone delta cloud", "OLD_B")
            };
            await File.WriteAllLinesAsync(
                tmpPairs,
                skippedRows.Select(pair => JsonSerializer.Serialize(pair, serializerOptions)));

            var skippedReport = await TatoebaPilotLoopRunner.RunAsync(
                new TatoebaPilotLoopRunOptions(
                    PrivatePairsPath: tmpPairs,
                    TrainSetSize: 2,
                    TestSetSize: 2,
                    MaxCycles: 1,
                    TargetAccuracy: 0.95,
                    CheckpointPath: skippedCheckpoint));

            Assert.Equal(2, skippedReport.AcceptedTrainingRows);
            Assert.Equal(2, skippedReport.SkippedRows);
            var skippedCheckpointJson = await File.ReadAllTextAsync(skippedCheckpoint, System.Text.Encoding.UTF8);
            var skippedMachineCheckpoint = JsonSerializer.Deserialize<TranslationMachineCheckpoint>(
                skippedCheckpointJson,
                TranslationCheckpointJsonOptions.Standard);
            Assert.NotNull(skippedMachineCheckpoint?.TatoebaLoop);
            Assert.Equal(2, skippedMachineCheckpoint!.TatoebaLoop!.AcceptedTrainingRows.Count);
            Assert.Equal(2, skippedMachineCheckpoint.TatoebaLoop.SkippedRows.Count);
        }
        finally
        {
            if (File.Exists(tmpPairs)) File.Delete(tmpPairs);
            if (File.Exists(promotedCheckpoint)) File.Delete(promotedCheckpoint);
            if (File.Exists(skippedCheckpoint)) File.Delete(skippedCheckpoint);
        }
    }

    [Fact]
    public async Task TatoebaPilotLoopRunner_ResumeUsesCheckpointWithoutReplayOrSkippedTrainingHistory()
    {
        var tmpPairs = Path.GetTempFileName() + ".jsonl";
        var tmpCheckpoint = Path.GetTempFileName() + ".json";
        try
        {
            var pairs = new[]
            {
                BuildPrivatePair(1, 101, "known alpha one", "Known alpha one", "OLD_A", "EN_RU"),
                BuildPrivatePair(2, 102, "known beta two", "Known beta two", "OLD_B", "EN_RU"),
                BuildPrivatePair(3, 103, "known beta alpha", "Known beta alpha", "OLD_A", "EN_RU"),
                BuildPrivatePair(4, 104, "one two alpha beta", "One two alpha beta", "OLD_B", "EN_RU"),
                BuildPrivatePair(5, 105, "silver moon rises", "Silver moon rises", "NEW_C", "EN_RU"),
                BuildPrivatePair(6, 106, "green field waits", "Green field waits", "NEW_D", "EN_RU"),
                BuildPrivatePair(7, 107, "winter candle glows", "Winter candle glows", "NEW_E", "EN_RU"),
                BuildPrivatePair(8, 108, "summer window opens", "Summer window opens", "NEW_F", "EN_RU")
            };
            var serializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
            await File.WriteAllLinesAsync(
                tmpPairs,
                pairs.Select(pair => JsonSerializer.Serialize(pair, serializerOptions)));

            var firstReport = await TatoebaPilotLoopRunner.RunAsync(
                new TatoebaPilotLoopRunOptions(
                    PrivatePairsPath: tmpPairs,
                    TrainSetSize: 2,
                    TestSetSize: 2,
                    MaxCycles: 1,
                    TargetAccuracy: 0.95,
                    CheckpointPath: tmpCheckpoint));

            Assert.False(firstReport.CheckpointLoaded);
            Assert.True(firstReport.CheckpointSaved);
            Assert.Equal("fresh", firstReport.ResumeMode);
            Assert.Equal(0, firstReport.ReplayedCycles);
            var firstCycle = Assert.Single(firstReport.Cycles);
            Assert.True(firstCycle.PromotedToTraining);
            Assert.Equal(4, firstCycle.CandidateTestRowsScanned);
            Assert.Equal(2, firstCycle.NoveltyScanSkippedRows);
            Assert.Equal("adaptive_promotable_scan", firstCycle.CandidateSelectionMode);
            Assert.DoesNotContain(firstCycle.Rows, row => row.SourceSentenceId is 3 or 4);
            Assert.Equal(4, firstReport.AcceptedTrainingRows);
            Assert.Equal(2, firstReport.SkippedRows);

            var firstCheckpointJson = await File.ReadAllTextAsync(tmpCheckpoint, System.Text.Encoding.UTF8);
            var firstCheckpoint = JsonSerializer.Deserialize<TranslationMachineCheckpoint>(
                firstCheckpointJson,
                TranslationCheckpointJsonOptions.Standard);
            Assert.NotNull(firstCheckpoint?.TatoebaLoop);
            var savedLoop = firstCheckpoint!.TatoebaLoop!;
            Assert.Equal(6, savedLoop.Cursor);
            Assert.Equal(1, savedLoop.LastCompletedCycle);
            Assert.Equal(4, savedLoop.AcceptedTrainingRows.Count);
            Assert.Equal(2, savedLoop.SkippedRows.Count);
            Assert.Contains(savedLoop.SkippedRows, row => row.SourceSentenceId == 3);
            Assert.Contains(savedLoop.SkippedRows, row => row.SourceSentenceId == 4);
            Assert.DoesNotContain(savedLoop.AcceptedTrainingRows, row => row.SourceSentenceId is 3 or 4);

            var resumedReport = await TatoebaPilotLoopRunner.RunAsync(
                new TatoebaPilotLoopRunOptions(
                    PrivatePairsPath: tmpPairs,
                    TrainSetSize: 2,
                    TestSetSize: 2,
                    MaxCycles: 1,
                    SkipCompletedCycles: 99,
                    TargetAccuracy: 0.95,
                    CheckpointPath: tmpCheckpoint,
                    RequireCheckpointResume: true));

            Assert.True(resumedReport.CheckpointLoaded);
            Assert.True(resumedReport.CheckpointSaved);
            Assert.Equal("checkpoint", resumedReport.ResumeMode);
            Assert.Equal(0, resumedReport.ReplayedCycles);
            Assert.True(resumedReport.InitialTrainingSkippedDueCheckpoint);
            Assert.Equal(savedLoop.Cursor, resumedReport.StartCursor);
            Assert.Equal(savedLoop.LastCompletedCycle + 1, resumedReport.StartCycle);
            Assert.Equal(8, resumedReport.EndCursor);
            Assert.Equal(2, resumedReport.LastCompletedCycle);
            Assert.Equal(6, resumedReport.AcceptedTrainingRows);
            Assert.Equal(2, resumedReport.SkippedRows);

            var resumedCycle = Assert.Single(resumedReport.Cycles);
            Assert.Equal(savedLoop.LastCompletedCycle + 1, resumedCycle.Cycle);
            Assert.Equal(4, resumedCycle.TrainRows);
            Assert.Equal(2, resumedCycle.TestRows);
            Assert.Equal(2, resumedCycle.CandidateTestRowsScanned);
            Assert.Equal(0, resumedCycle.NoveltyScanSkippedRows);
            Assert.True(resumedCycle.PromotedToTraining);
            Assert.Contains(resumedCycle.Rows, row => row.SourceSentenceId == 7);
            Assert.Contains(resumedCycle.Rows, row => row.SourceSentenceId == 8);
        }
        finally
        {
            if (File.Exists(tmpPairs)) File.Delete(tmpPairs);
            if (File.Exists(tmpCheckpoint)) File.Delete(tmpCheckpoint);
        }
    }

    [Fact]
    public async Task TatoebaPilotLoopRunner_SourceRolloverLoadsCheckpointWeightsAndStartsNewSourceWithoutReplay()
    {
        var root = Path.Combine(Path.GetTempPath(), "xiigen-tatoeba-loop-test", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var oldPairsPath = Path.Combine(root, "sentence-pairs-old.jsonl");
        var newPairsPath = Path.Combine(root, "sentence-pairs-new.jsonl");
        var checkpointPath = Path.Combine(root, "translation-checkpoint.json");
        var serializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
        try
        {
            var oldRows = new[]
            {
                BuildPrivatePair(1, 101, "legacy alpha source", "старый альфа перевод", "OLD_A", "EN_RU"),
                BuildPrivatePair(2, 102, "legacy beta source", "старый бета перевод", "OLD_B", "EN_RU")
            };
            await File.WriteAllLinesAsync(
                oldPairsPath,
                oldRows.Select(pair => JsonSerializer.Serialize(pair, serializerOptions)));

            var initialReport = await TatoebaPilotLoopRunner.RunAsync(
                new TatoebaPilotLoopRunOptions(
                    PrivatePairsPath: oldPairsPath,
                    TrainSetSize: 2,
                    TestSetSize: 2,
                    MaxCycles: 0,
                    TargetAccuracy: 0.95,
                    CheckpointPath: checkpointPath));

            Assert.False(initialReport.CheckpointLoaded);
            Assert.Equal(2, initialReport.AcceptedTrainingRows);

            var checkpointJson = await File.ReadAllTextAsync(checkpointPath, System.Text.Encoding.UTF8);
            var checkpoint = JsonSerializer.Deserialize<TranslationMachineCheckpoint>(
                checkpointJson,
                TranslationCheckpointJsonOptions.Standard);
            Assert.NotNull(checkpoint?.TatoebaLoop);
            var patchedCheckpoint = checkpoint! with
            {
                TatoebaLoop = checkpoint.TatoebaLoop! with { BestRuntimeProductAccuracy = 0.82 }
            };
            await File.WriteAllTextAsync(
                checkpointPath,
                JsonSerializer.Serialize(patchedCheckpoint, TranslationCheckpointJsonOptions.Standard),
                System.Text.Encoding.UTF8);

            var newRows = new[]
            {
                BuildPrivatePair(11, 111, "fresh gamma source", "свежий гамма перевод", "NEW_A", "EN_RU"),
                BuildPrivatePair(12, 112, "fresh delta source", "свежий дельта перевод", "NEW_B", "EN_RU"),
                BuildPrivatePair(1, 101, "legacy alpha source", "старый альфа перевод", "OLD_A", "EN_RU"),
                BuildPrivatePair(13, 113, "fresh epsilon source", "свежий эпсилон перевод", "NEW_C", "EN_RU"),
                BuildPrivatePair(14, 114, "fresh zeta source", "свежий дзета перевод", "NEW_D", "EN_RU")
            };
            await File.WriteAllLinesAsync(
                newPairsPath,
                newRows.Select(pair => JsonSerializer.Serialize(pair, serializerOptions)));

            var rolloverReport = await TatoebaPilotLoopRunner.RunAsync(
                new TatoebaPilotLoopRunOptions(
                    PrivatePairsPath: newPairsPath,
                    TrainSetSize: 2,
                    TestSetSize: 2,
                    MaxCycles: 1,
                    TargetAccuracy: 0.95,
                    CheckpointPath: checkpointPath,
                    RequireCheckpointResume: true,
                    StartNewSourceFromCheckpoint: true));

            Assert.True(rolloverReport.CheckpointLoaded);
            Assert.True(rolloverReport.CheckpointSaved);
            Assert.Equal("source-rollover", rolloverReport.ResumeMode);
            Assert.Equal(0, rolloverReport.ReplayedCycles);
            Assert.False(rolloverReport.InitialTrainingSkippedDueCheckpoint);
            Assert.Equal(Path.GetFullPath(oldPairsPath), rolloverReport.SourceRolloverFrom);
            Assert.Equal(Path.GetFullPath(newPairsPath), rolloverReport.SourceRolloverTo);
            Assert.Equal(0, rolloverReport.SourceRolloverStartCursor);
            Assert.Equal(2, rolloverReport.StartCursor);
            Assert.Equal(1, rolloverReport.StartCycle);
            Assert.Equal(0.82, rolloverReport.BestRuntimeProductAccuracy);

            var cycle = Assert.Single(rolloverReport.Cycles);
            Assert.Equal(1, cycle.Cycle);
            Assert.Equal(2, cycle.TrainRows);
            Assert.Equal(2, cycle.TestRows);
            Assert.Equal(3, cycle.CandidateTestRowsScanned);
            Assert.Equal(1, cycle.FilteredCheckpointMemoryOverlapRows);
            Assert.DoesNotContain(cycle.Rows, row => row.SourceSentenceId == 1);

            Assert.Contains(cycle.Rows, row => row.SourceSentenceId == 13);
            Assert.Contains(cycle.Rows, row => row.SourceSentenceId == 14);
            Assert.False(rolloverReport.ReachedTarget);

            var savedJson = await File.ReadAllTextAsync(checkpointPath, System.Text.Encoding.UTF8);
            var savedCheckpoint = JsonSerializer.Deserialize<TranslationMachineCheckpoint>(
                savedJson,
                TranslationCheckpointJsonOptions.Standard);
            Assert.NotNull(savedCheckpoint?.TatoebaLoop);
            Assert.Equal(Path.GetFullPath(newPairsPath), savedCheckpoint!.TatoebaLoop!.PrivatePairsPath);
            Assert.True(savedCheckpoint.TatoebaLoop.AcceptedTrainingRows.Count >= 2);
            Assert.Contains(savedCheckpoint.TatoebaLoop.AcceptedTrainingRows, row => row.SourceSentenceId == 11);
            Assert.DoesNotContain(savedCheckpoint.TatoebaLoop.AcceptedTrainingRows, row => row.SourceSentenceId == 1);
            Assert.DoesNotContain(savedCheckpoint.TatoebaLoop.AcceptedTrainingRows, row => row.SourceSentenceId == 2);
            Assert.True(savedCheckpoint.TatoebaLoop.Cursor >= 4);
            Assert.Equal(0.82, savedCheckpoint.TatoebaLoop.BestRuntimeProductAccuracy);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task TatoebaPilotLoopRunner_SourceRolloverCannotReachTargetFromCheckpointExactRows()
    {
        var root = Path.Combine(Path.GetTempPath(), "xiigen-tatoeba-loop-test", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var oldPairsPath = Path.Combine(root, "sentence-pairs-old.jsonl");
        var newPairsPath = Path.Combine(root, "sentence-pairs-new.jsonl");
        var checkpointPath = Path.Combine(root, "translation-checkpoint.json");
        var serializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
        try
        {
            var oldRows = new[]
            {
                BuildPrivatePair(1, 101, "known alpha sentence", "известное альфа предложение", "OLD_A", "EN_RU"),
                BuildPrivatePair(2, 102, "known beta sentence", "известное бета предложение", "OLD_B", "EN_RU")
            };
            await File.WriteAllLinesAsync(
                oldPairsPath,
                oldRows.Select(pair => JsonSerializer.Serialize(pair, serializerOptions)));

            await TatoebaPilotLoopRunner.RunAsync(
                new TatoebaPilotLoopRunOptions(
                    PrivatePairsPath: oldPairsPath,
                    TrainSetSize: 2,
                    TestSetSize: 2,
                    MaxCycles: 0,
                    TargetAccuracy: 0.50,
                    CheckpointPath: checkpointPath));

            var newRows = new[]
            {
                BuildPrivatePair(21, 121, "fresh starter one", "свежий старт один", "NEW_A", "EN_RU"),
                BuildPrivatePair(22, 122, "fresh starter two", "свежий старт два", "NEW_B", "EN_RU"),
                BuildPrivatePair(1, 101, "known alpha sentence", "известное альфа предложение", "OLD_A", "EN_RU"),
                BuildPrivatePair(2, 102, "known beta sentence", "известное бета предложение", "OLD_B", "EN_RU"),
                BuildPrivatePair(23, 123, "novel gamma sentence", "новое гамма предложение", "NEW_C", "EN_RU")
            };
            await File.WriteAllLinesAsync(
                newPairsPath,
                newRows.Select(pair => JsonSerializer.Serialize(pair, serializerOptions)));

            var rolloverReport = await TatoebaPilotLoopRunner.RunAsync(
                new TatoebaPilotLoopRunOptions(
                    PrivatePairsPath: newPairsPath,
                    TrainSetSize: 2,
                    TestSetSize: 2,
                    MaxCycles: 1,
                    TargetAccuracy: 0.50,
                    CheckpointPath: checkpointPath,
                    RequireCheckpointResume: true,
                    StartNewSourceFromCheckpoint: true));

            Assert.False(rolloverReport.ReachedTarget);
            var cycle = Assert.Single(rolloverReport.Cycles);
            Assert.Equal(1, cycle.TestRows);
            Assert.Equal(3, cycle.CandidateTestRowsScanned);
            Assert.Equal(2, cycle.FilteredCheckpointMemoryOverlapRows);
            Assert.Equal(1, cycle.TestSelectionShortfall);
            Assert.False(cycle.PassesTarget);
            Assert.DoesNotContain(cycle.Rows, row => row.SourceSentenceId == 1);
            Assert.DoesNotContain(cycle.Rows, row => row.SourceSentenceId == 2);
            Assert.Contains(cycle.Rows, row => row.SourceSentenceId == 23);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task TatoebaPilotLoopRunner_ResumeRestoresCursorFromCheckpoint()
    {
        // Test 9: LoadCheckpointAsync returns the TatoebaLoop cursor saved by SaveCheckpointAsync.
        var tmpPath = Path.GetTempFileName() + ".json";
        try
        {
            // Save a real machine checkpoint that includes a TatoebaLoop cursor.
            var machine = new TranslationMachine();
            var loopState = new TatoebaPilotLoopCheckpoint(
                PrivatePairsPath: "test-pairs.jsonl",
                TrainSetSize: 100,
                TestSetSize: 100,
                LastCompletedCycle: 4,
                Cursor: 999,
                TrainRows: 400,
                BestRuntimeProductAccuracy: 0.82,
                AcceptedTrainingRows: [],
                SkippedRows: []);
            var saveResult = await machine.SaveCheckpointAsync(tmpPath, loopState);
            Assert.True(saveResult.IsSuccess, saveResult.ErrorMessage);

            // Load checkpoint and verify the loop cursor is preserved exactly.
            var loadResult = await TranslationMachine.LoadCheckpointAsync(tmpPath);
            Assert.True(loadResult.IsSuccess, loadResult.ErrorMessage);
            Assert.NotNull(loadResult.Value!.TatoebaLoop);
            Assert.Equal(999, loadResult.Value!.TatoebaLoop!.Cursor);
            Assert.Equal(4, loadResult.Value!.TatoebaLoop!.LastCompletedCycle);
            Assert.Equal(0.82, loadResult.Value!.TatoebaLoop!.BestRuntimeProductAccuracy);
        }
        finally { if (File.Exists(tmpPath)) File.Delete(tmpPath); }
    }

    [Fact]
    public async Task TranslationMachine_SaveCheckpoint_AtomicWriteUsesTmpFile()
    {
        // Test 10: SaveCheckpointAsync writes to .tmp then moves; no .tmp remains after success.
        var tmpPath = Path.GetTempFileName() + ".json";
        var tmpFilePath = tmpPath + ".tmp";
        try
        {
            // Save checkpoint from a fresh machine.
            var machine = new TranslationMachine();
            var result = await machine.SaveCheckpointAsync(tmpPath);
            Assert.True(result.IsSuccess);
            // Assert: final checkpoint file exists and contains valid JSON.
            Assert.True(File.Exists(tmpPath), "Final checkpoint file must exist.");
            var json = await File.ReadAllTextAsync(tmpPath, System.Text.Encoding.UTF8);
            Assert.False(string.IsNullOrWhiteSpace(json));
            var checkpoint = System.Text.Json.JsonSerializer.Deserialize<TranslationMachineCheckpoint>(
                json, TranslationCheckpointJsonOptions.Standard);
            Assert.NotNull(checkpoint);
            // Assert: .tmp file was cleaned up after the atomic move completed.
            Assert.False(File.Exists(tmpFilePath), ".tmp file must not remain after successful save.");
            // Assert: ManifestRef equals the absolute path of the checkpoint file.
            Assert.Equal(Path.GetFullPath(tmpPath), result.Value!.ManifestRef);
            Assert.Equal(Path.GetFullPath(tmpPath), result.Value!.CheckpointRef);
        }
        finally
        {
            if (File.Exists(tmpPath)) File.Delete(tmpPath);
            if (File.Exists(tmpFilePath)) File.Delete(tmpFilePath);
        }
    }

    [Fact]
    public async Task TranslationMachine_CheckpointRoundTrip_ContinuedTrainingPreservesOldAndNewKnowledge()
    {
        // Test 11: train A, save, load, train B, save again — both A and B survive the second load.
        var tmpPath = Path.GetTempFileName() + ".json";
        var tmpPath2 = tmpPath + ".2.json";
        try
        {
            // Train pair A on a fresh machine.
            var machine = new TranslationMachine();
            var batchA = new TranslationLearningBatch(
                "ckpt-continued-a", TranslationMachine.DomainTag,
                [BuildExecutorExampleWithMappings("pair-a", "добавить запись",
                    "IMPERATIVE_V|ADD_OP|DATA_ENTITY", "add record",
                    "wrong_word",
                    [new ExecutorWordMapping("добавить", SemanticConcept.ADD_OP, "add"),
                     new ExecutorWordMapping("запись", SemanticConcept.DATA_ENTITY, "record")])],
                ["continued-test"]);
            Assert.True((await machine.TrainStructuredBatchAsync(batchA)).IsSuccess);
            // Verify A translates before save to confirm training worked.
            var resultABefore = await RunTranslationAsync(machine, "добавить запись");
            Assert.DoesNotContain("[RU_EN:BOOTSTRAP]", resultABefore);

            // Save checkpoint from the machine that knows A.
            var saveResult = await machine.SaveCheckpointAsync(tmpPath);
            Assert.True(saveResult.IsSuccess);

            // Load a fresh machine from the checkpoint.
            var loadResult = await TranslationMachine.LoadCheckpointAsync(tmpPath);
            Assert.True(loadResult.IsSuccess);
            var loadedMachine = loadResult.Value!.Machine;

            // Train pair B on the loaded machine so both A and B are now learned.
            var batchB = new TranslationLearningBatch(
                "ckpt-continued-b", TranslationMachine.DomainTag,
                [BuildExecutorExampleWithMappings("pair-b", "запустить поток",
                    "IMPERATIVE_V|LAUNCH_OP|THREAD_REF", "launch thread",
                    "wrong_word",
                    [new ExecutorWordMapping("запустить", SemanticConcept.LAUNCH_OP, "launch"),
                     new ExecutorWordMapping("поток", SemanticConcept.THREAD_REF, "thread")])],
                ["continued-test"]);
            Assert.True((await loadedMachine.TrainStructuredBatchAsync(batchB)).IsSuccess);

            // Assert A still works on the loaded machine (old knowledge preserved).
            var resultAAfter = await RunTranslationAsync(loadedMachine, "добавить запись");
            Assert.DoesNotContain("[RU_EN:BOOTSTRAP]", resultAAfter);
            Assert.Contains("add", resultAAfter, StringComparison.OrdinalIgnoreCase);

            // Assert B also works (new knowledge learned after checkpoint load).
            var resultB = await RunTranslationAsync(loadedMachine, "запустить поток");
            Assert.DoesNotContain("[RU_EN:BOOTSTRAP]", resultB);
            Assert.Contains("launch", resultB, StringComparison.OrdinalIgnoreCase);

            // Save a second checkpoint and reload — both A and B must survive.
            var save2 = await loadedMachine.SaveCheckpointAsync(tmpPath2);
            Assert.True(save2.IsSuccess);
            var load2 = await TranslationMachine.LoadCheckpointAsync(tmpPath2);
            Assert.True(load2.IsSuccess);
            var m2 = load2.Value!.Machine;
            Assert.DoesNotContain("[RU_EN:BOOTSTRAP]", await RunTranslationAsync(m2, "добавить запись"));
            Assert.DoesNotContain("[RU_EN:BOOTSTRAP]", await RunTranslationAsync(m2, "запустить поток"));
        }
        finally
        {
            if (File.Exists(tmpPath)) File.Delete(tmpPath);
            if (File.Exists(tmpPath2)) File.Delete(tmpPath2);
        }
    }

    [Fact]
    public async Task TranslationMachine_Resume_CacheFirstThenDiskFallback()
    {
        // Test 12: TranslationCheckpointCache returns the same object on cache hit; invalidate clears it.
        var tmpPath = Path.GetTempFileName() + ".json";
        try
        {
            // Prepare a fresh machine and save a checkpoint to disk.
            var machine = new TranslationMachine();
            var saveResult = await machine.SaveCheckpointAsync(tmpPath);
            Assert.True(saveResult.IsSuccess);
            var absolutePath = Path.GetFullPath(tmpPath);

            // First load: cache miss — load from disk and store in cache.
            var cache = new TranslationCheckpointCache();
            var load1 = await TranslationMachine.LoadCheckpointAsync(tmpPath);
            Assert.True(load1.IsSuccess);
            cache.Store(absolutePath, load1.Value!);

            // Second load: cache hit — same object reference returned without disk read.
            Assert.True(cache.TryGet(absolutePath, out var cachedResult));
            Assert.NotNull(cachedResult);
            // The cached machine must be the exact same instance as the one stored.
            Assert.Same(load1.Value!.Machine, cachedResult!.Machine);

            // After invalidation TryGet must return false (entry removed from cache).
            cache.Invalidate(absolutePath);
            Assert.False(cache.TryGet(absolutePath, out _));

            // After invalidation: a new disk read produces a different machine object instance.
            var load2 = await TranslationMachine.LoadCheckpointAsync(tmpPath);
            Assert.True(load2.IsSuccess);
            // Different object instance — deserialized freshly, not from cache.
            Assert.NotSame(load1.Value!.Machine, load2.Value!.Machine);
        }
        finally { if (File.Exists(tmpPath)) File.Delete(tmpPath); }
    }

    [Fact]
    public void TranslationRunnerReport_CommittedArtifact_QuarantinesRewardMarginAndPreservesHashes()
    {
        var reportPath = GetRepoPath("models", "translation", "translation-v1", "runner-report.json");
        using var document = JsonDocument.Parse(File.ReadAllText(reportPath));
        var root = document.RootElement;

        Assert.Equal(JsonValueKind.Null, root.GetProperty("reward_margin").ValueKind);
        Assert.False(root.GetProperty("reward_margin_available").GetBoolean());
        Assert.Equal(0, root.GetProperty("reward_margin_evidence_count").GetInt32());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("reward_margin_source").ValueKind);
        Assert.False(root.GetProperty("passes_promotion_gate").GetBoolean());
        Assert.Equal("REWARD_MARGIN_UNAVAILABLE", root.GetProperty("promotion_gate_reason").GetString());
        Assert.Equal(
            "artifact_regeneration_only_not_model_promotion",
            root.GetProperty("pass_scope").GetString());
        Assert.True(root.GetProperty("passed").GetBoolean());
        Assert.Equal(
            "53559e02f6f0a5eb54d85e6e424afb97c8bd1d364f5a7c8aa10c891430abd9ac",
            root.GetProperty("corpus_sha256").GetString());
        Assert.Equal(
            "95b5d1c6de1d70a6bba4adbafdf57ffafa0735b7c52d193e79f64acad7e84887",
            root.GetProperty("checkpoint_sha256").GetString());
        Assert.Equal(
            "95b5d1c6de1d70a6bba4adbafdf57ffafa0735b7c52d193e79f64acad7e84887",
            root.GetProperty("export_input_sha256").GetString());
    }

    [Fact]
    public async Task LoadCheckpointAsync_Schema1LegacyMargin_NormalizesTelemetryAndPreservesState()
    {
        var (machine, source, expectedTranslation) = await BuildR19LearnedMachineAsync();
        var path = Path.GetTempFileName() + ".json";
        try
        {
            var field = typeof(TranslationMachine).GetField(
                "LegacyUnverifiedRewardMargin",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(field);
            Assert.True(field!.IsLiteral);
            Assert.False(field.IsInitOnly);
            var legacyUnverifiedMargin = (double)field.GetRawConstantValue()!;

            var loop = new TatoebaPilotLoopCheckpoint(
                PrivatePairsPath: "r19-private-pairs.jsonl",
                TrainSetSize: 17,
                TestSetSize: 19,
                LastCompletedCycle: 23,
                Cursor: 29,
                TrainRows: 31,
                BestRuntimeProductAccuracy: 0.87,
                AcceptedTrainingRows:
                [
                    new TatoebaPilotPairRefCheckpoint("RU_EN", 101, 201, "R19_ACCEPTED_A"),
                    new TatoebaPilotPairRefCheckpoint("EN_RU", 102, 202, "R19_ACCEPTED_B")
                ],
                SkippedRows:
                [
                    new TatoebaPilotPairRefCheckpoint("HE_EN", 103, 203, "R19_SKIPPED_A")
                ]);

            var save = await machine.SaveCheckpointAsync(path, loop);
            Assert.True(save.IsSuccess, save.ErrorMessage);
            var saved = JsonSerializer.Deserialize<TranslationMachineCheckpoint>(
                await File.ReadAllTextAsync(path),
                TranslationCheckpointJsonOptions.Standard);
            Assert.NotNull(saved);
            Assert.NotEmpty(saved!.TrainingHistory);

            var legacyHistory = saved.TrainingHistory
                .Select((entry, index) => index == 0
                    ? entry with { RewardMargin = legacyUnverifiedMargin }
                    : entry)
                .ToList();
            var legacyCheckpoint = saved with { TrainingHistory = legacyHistory };
            var expectedMachineState = legacyCheckpoint with
            {
                TrainingHistory = legacyHistory
                    .Select((entry, index) => index == 0
                        ? entry with { RewardMargin = 0.0 }
                        : entry)
                    .ToList(),
                TatoebaLoop = null
            };
            await File.WriteAllTextAsync(
                path,
                JsonSerializer.Serialize(legacyCheckpoint, TranslationCheckpointJsonOptions.Standard));

            var load = await TranslationMachine.LoadCheckpointAsync(path);

            Assert.True(load.IsSuccess, load.ErrorMessage);
            var loadedLoop = load.Value!.TatoebaLoop;
            Assert.NotNull(loadedLoop);
            Assert.Equal(loop.PrivatePairsPath, loadedLoop!.PrivatePairsPath);
            Assert.Equal(loop.TrainSetSize, loadedLoop.TrainSetSize);
            Assert.Equal(loop.TestSetSize, loadedLoop.TestSetSize);
            Assert.Equal(loop.LastCompletedCycle, loadedLoop.LastCompletedCycle);
            Assert.Equal(loop.Cursor, loadedLoop.Cursor);
            Assert.Equal(loop.TrainRows, loadedLoop.TrainRows);
            Assert.Equal(loop.BestRuntimeProductAccuracy, loadedLoop.BestRuntimeProductAccuracy);
            Assert.Equal(loop.AcceptedTrainingRows, loadedLoop.AcceptedTrainingRows);
            Assert.Equal(loop.SkippedRows, loadedLoop.SkippedRows);

            var reExport = load.Value.Machine.ExportCheckpoint();
            Assert.True(reExport.IsSuccess, reExport.ErrorMessage);
            Assert.Equal(1, reExport.Value!.SchemaVersion);
            Assert.All(reExport.Value.TrainingHistory, entry => Assert.Equal(0.0, entry.RewardMargin));
            Assert.Equal(
                CanonicalCheckpointHash(expectedMachineState),
                CanonicalCheckpointHash(reExport.Value));

            var actualTranslation = await RunTranslationAsync(load.Value.Machine, source);
            Assert.Equal(expectedTranslation, actualTranslation);
            Assert.DoesNotContain("[RU_EN:BOOTSTRAP]", actualTranslation, StringComparison.Ordinal);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public async Task ImportCheckpoint_MissingExecutorDirection_FailsBeforeMutation()
    {
        var state = await BuildR19LearnedMachineAsync();
        var checkpoint = ExportCheckpoint(new TranslationMachine());
        var malformed = checkpoint with { Executors = checkpoint.Executors.Skip(1).ToList() };

        await AssertRejectedCheckpointDoesNotMutateAsync(
            state,
            malformed,
            "EXECUTOR_DIRECTIONS_INCOMPLETE");
    }

    [Fact]
    public async Task ImportCheckpoint_DuplicateExecutorDirection_FailsBeforeMutation()
    {
        var state = await BuildR19LearnedMachineAsync();
        var checkpoint = ExportCheckpoint(state.Machine);
        var malformed = checkpoint with
        {
            Executors = checkpoint.Executors.Concat([checkpoint.Executors[0]]).ToList()
        };

        await AssertRejectedCheckpointDoesNotMutateAsync(
            state,
            malformed,
            "EXECUTOR_DIRECTION_DUPLICATE");
    }

    [Fact]
    public async Task ImportCheckpoint_UnknownExecutorDirection_FailsBeforeMutation()
    {
        var state = await BuildR19LearnedMachineAsync();
        var checkpoint = ExportCheckpoint(state.Machine);
        var unknown = checkpoint.Executors[0] with
        {
            Direction = (TranslationDirection)int.MaxValue
        };
        var malformed = checkpoint with
        {
            Executors = checkpoint.Executors.Select((entry, index) => index == 0 ? unknown : entry).ToList()
        };

        await AssertRejectedCheckpointDoesNotMutateAsync(
            state,
            malformed,
            "EXECUTOR_DIRECTION_UNKNOWN");
    }

    [Fact]
    public async Task ImportCheckpoint_NullNestedCollectionFromSchema1Json_FailsBeforeMutation()
    {
        var state = await BuildR19LearnedMachineAsync();
        var checkpoint = ExportCheckpoint(state.Machine);
        var node = System.Text.Json.Nodes.JsonNode.Parse(
            JsonSerializer.Serialize(checkpoint, TranslationCheckpointJsonOptions.Standard))!.AsObject();
        node["knn_store"]!.AsObject()["entries"] = null;
        var malformed = JsonSerializer.Deserialize<TranslationMachineCheckpoint>(
            node.ToJsonString(),
            TranslationCheckpointJsonOptions.Standard);
        Assert.NotNull(malformed);

        await AssertRejectedCheckpointDoesNotMutateAsync(
            state,
            malformed!,
            "CHECKPOINT_STRUCTURE_INVALID");
    }

    [Fact]
    public async Task ImportCheckpoint_NonFiniteHistoryMetric_FailsBeforeMutation()
    {
        var state = await BuildR19LearnedMachineAsync();
        var checkpoint = ExportCheckpoint(state.Machine);
        Assert.NotEmpty(checkpoint.TrainingHistory);
        var malformed = checkpoint with
        {
            TrainingHistory = checkpoint.TrainingHistory
                .Select((entry, index) => index == 0
                    ? entry with { AccuracyAfter = double.NaN }
                    : entry)
                .ToList()
        };

        await AssertRejectedCheckpointDoesNotMutateAsync(
            state,
            malformed,
            "CHECKPOINT_STRUCTURE_INVALID");
    }

    [Fact]
    public void TranslationRewardMarginQuarantine_StaticGuardsHold()
    {
        var machinePath = GetRepoPath("27.TranslationDomain", "TranslationMachine.cs");
        var batchRunnerPath = GetRepoPath("27.TranslationDomain", "TranslationBatch029Runner.cs");
        var productRunnerPath = GetRepoPath("27.TranslationDomain", "TranslationProductHeldOutRunner.cs");
        var machineSource = File.ReadAllText(machinePath);
        var batchRunnerSource = File.ReadAllText(batchRunnerPath);
        var productRunnerSource = File.ReadAllText(productRunnerPath);

        var literalMatches = System.Text.RegularExpressions.Regex.Matches(
            machineSource,
            @"0\.03d?");
        var literal = Assert.Single(literalMatches.Cast<System.Text.RegularExpressions.Match>());
        var declarationLine = machineSource
            .Split('\n')
            .Single(line => line.Contains(literal.Value, StringComparison.Ordinal));
        Assert.Contains(
            "LegacyUnverifiedRewardMargin = 0.03d",
            declarationLine,
            StringComparison.Ordinal);

        var nameMatches = System.Text.RegularExpressions.Regex.Matches(
            machineSource,
            "LegacyUnverifiedRewardMargin");
        Assert.Equal(2, nameMatches.Count);
        Assert.DoesNotContain(
            "LegacyUnverifiedRewardMargin",
            SourceBetween(machineSource, "TrainBatchAsync(", "TrainStructuredBatchAsync("),
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "LegacyUnverifiedRewardMargin",
            SourceBetween(machineSource, "TrainStructuredBatchAsync(", "EvaluateAsync("),
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "LegacyUnverifiedRewardMargin",
            SourceBetween(machineSource, "EvaluateAsync(", "ExportAsync("),
            StringComparison.Ordinal);
        Assert.DoesNotContain("LegacyUnverifiedRewardMargin", batchRunnerSource, StringComparison.Ordinal);
        Assert.DoesNotContain("LegacyUnverifiedRewardMargin", productRunnerSource, StringComparison.Ordinal);
        Assert.DoesNotMatch(@"RewardMargin\s*>=\s*0\.025", batchRunnerSource);
        Assert.DoesNotMatch(@"RewardMargin\s*>=\s*0\.025", productRunnerSource);
    }

    private static async Task<(
        TranslationMachine Machine,
        string Source,
        string ExpectedTranslation)> BuildR19LearnedMachineAsync()
    {
        const string source = "r19 learned source";
        const string expectedTranslation = "r19 learned target";
        var machine = new TranslationMachine();
        var train = await machine.TrainStructuredBatchAsync(new TranslationLearningBatch(
            BatchId: "r19-checkpoint-state",
            DomainTag: TranslationMachine.DomainTag,
            Examples:
            [
                BuildExecutorExample(
                    "r19-checkpoint-state",
                    source,
                    "R19|CHECKPOINT|STATE",
                    expectedTranslation,
                    "wrong target",
                    "wrong_target")
            ],
            SourceIds: ["r19-test"]));
        Assert.True(train.IsSuccess, train.ErrorMessage);
        var actual = await RunTranslationAsync(machine, source);
        Assert.Equal(expectedTranslation, actual);
        Assert.DoesNotContain("[RU_EN:BOOTSTRAP]", actual, StringComparison.Ordinal);
        return (machine, source, expectedTranslation);
    }

    private static TranslationMachineCheckpoint ExportCheckpoint(TranslationMachine machine)
    {
        var export = machine.ExportCheckpoint();
        Assert.True(export.IsSuccess, export.ErrorMessage);
        return export.Value!;
    }

    private static async Task AssertRejectedCheckpointDoesNotMutateAsync(
        (TranslationMachine Machine, string Source, string ExpectedTranslation) state,
        TranslationMachineCheckpoint malformed,
        string expectedErrorCode)
    {
        var beforeHash = CanonicalCheckpointHash(ExportCheckpoint(state.Machine));
        var beforeTranslation = await RunTranslationAsync(state.Machine, state.Source);

        var result = state.Machine.ImportCheckpoint(malformed);

        Assert.False(result.IsSuccess);
        Assert.Equal(expectedErrorCode, result.ErrorCode);
        Assert.Equal(beforeHash, CanonicalCheckpointHash(ExportCheckpoint(state.Machine)));
        var afterTranslation = await RunTranslationAsync(state.Machine, state.Source);
        Assert.Equal(beforeTranslation, afterTranslation);
        Assert.Equal(state.ExpectedTranslation, afterTranslation);
        Assert.DoesNotContain("[RU_EN:BOOTSTRAP]", afterTranslation, StringComparison.Ordinal);
    }

    private static string CanonicalCheckpointHash(TranslationMachineCheckpoint checkpoint)
    {
        var normalized = checkpoint with
        {
            ExportedAtUtc = DateTimeOffset.UnixEpoch,
            Executors = checkpoint.Executors.OrderBy(executor => executor.Direction).ToList(),
            TatoebaLoop = null
        };
        var json = JsonSerializer.Serialize(normalized, TranslationCheckpointJsonOptions.Standard);
        return Convert.ToHexString(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(json)))
            .ToLowerInvariant();
    }

    private static string GetRepoPath(params string[] components)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = components.Aggregate(
                directory.FullName,
                Path.Combine);
            if (File.Exists(candidate))
                return candidate;
            directory = directory.Parent;
        }

        return components.Aggregate(AppContext.BaseDirectory, Path.Combine);
    }

    private static string SourceBetween(string source, string startMarker, string endMarker)
    {
        var start = source.IndexOf(startMarker, StringComparison.Ordinal);
        var end = source.IndexOf(endMarker, start + startMarker.Length, StringComparison.Ordinal);
        Assert.True(start >= 0, $"Missing start marker: {startMarker}");
        Assert.True(end > start, $"Missing end marker after {startMarker}: {endMarker}");
        return source[start..end];
    }
}
