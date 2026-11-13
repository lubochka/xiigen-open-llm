// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

// ЧТО ЗДЕСЬ ПРОВЕРЯЕТСЯ, ПО-ЧЕЛОВЕЧЕСКИ
//
// Переводчик умеет собирать перевод из выученных фраз. Раньше он шёл по словам
// источника слева направо и на каждом месте брал самую длинную выученную фразу,
// которая там начиналась, и этот выбор больше не пересматривал. На трёх уроках
// «close user → закрой пользователя», «user session → сеанс пользователя» и
// «close → закрой» экзамен «close user session» терял слово «session»: длинная
// фраза «close user» забирала первые два слова, на хвост «session» выученной
// фразы не было, покрытие оставалось два слова из трёх, порог 0,85 не
// проходился, и вся фразовая ветка отказывалась отвечать. После отказа ответ
// давала ассоциативная память — «закрой пользователя», то есть уверенно
// неправильный перевод вместо отказа, и никакого признака потери слова.
//
// Теперь в узкой разрешённой области разбиение выбирается целиком: из всех
// способов пройти по словам источника выбирается тот, который покрывает больше
// слов, при равном покрытии — собранный из меньшего числа фраз, а при полном
// равенстве — зафиксированный письменный порядок, чтобы ответ всегда был один и
// тот же. На том же экзамене выбирается «close» + «user session», покрытие
// становится три слова из трёх, и ответ «закрой сеанс пользователя» приходит
// именно из фразовой ветки.
//
// ОБЛАСТЬ РАЗРЕШЕНИЯ, ровно три условия: направление с английского на русский;
// не больше восьми нормализованных слов источника; ни у одного слова источника
// нет принятого выученного однословного перевода. Вне этой области остаётся
// прежний сборщик, и ни порог 0,85, ни условие повторной фразы 0,50, ни проверки
// целевого языка, ни приоритет точной памяти не меняются.
//
// ЧЕГО ЭТИ ТЕСТЫ НЕ ДОКАЗЫВАЮТ: что наибольшее покрытие слов означает понимание
// роли каждого слова — два разбиения с равным покрытием для этого выбора
// одинаково хороши; качество перевода на живых данных; время и память продукта.
// Поведение алгоритма отдельно проверено исполняемым оракулом
// Tests/ExperimentFixes/oracle-phrase-coverage-planner-109-p54.py.

namespace LlmFromScratch.Tests;

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure.Logical;
using LlmFromScratch.Tests.ExperimentFixes;
using TranslationDomain;
// PhraseEntry — та же форма записи выученной фразы, которую продукт строит для обоих своих
// сборщиков (TranslationTrainedUnits.cs:1025-1032): адрес, адрес по словам, выученный перевод.
using PhraseEntry = (string SourceKey, string[] SourceTokens, string Target);

// Фикстура класса ниже даёт этому испытанию ранний захват попытки: оснастка создаёт её ДО
// конструктора тестового класса и до любого урока обучения, а освобождает ОДИН раз после
// последнего теста класса.
public sealed class TranslationPhraseCoverageLearningTests
    : IClassFixture<TranslationPhraseCoverageLearningTests.AttemptScopeFixture>
{
    // Три урока опыта 54: у каждого ровно одно фразовое соответствие и ни одного
    // однословного, поэтому область разрешения планировщика выполняется.
    private const string LessonOneId = "R54-t01";
    private const string LessonOneSource = "close user";
    private const string LessonOneTarget = "закрой пользователя";

    private const string LessonTwoId = "R54-t02";
    private const string LessonTwoSource = "user session";
    private const string LessonTwoTarget = "сеанс пользователя";

    private const string LessonThreeId = "R54-t03";
    private const string LessonThreeSource = "close";
    private const string LessonThreeTarget = "закрой";

    // Экзаменационная строка и её правильный перевод.
    private const string ExamSource = "close user session";
    private const string ExamTarget = "закрой сеанс пользователя";

    // Края: восемь слов внутри области разрешения и девять слов вне неё.
    private const string EdgeEightSource = "close user session now please again today safely";
    private const string EdgeNineSource = "close user session now please again today safely tomorrow";

    private const string SourceId = "r54-native-acceptance-v1";

    [Fact]
    public async Task PhraseComposition_ExamRow_KeepsTheRequiredWordAfterTraining()
    {
        // Этот тест падает на прежнем сборщике: он отвечал «закрой пользователя».
        var machine = new TranslationMachine();
        var train = await machine.TrainStructuredBatchAsync(BuildLessonBatch());
        Assert.True(train.IsSuccess, train.ErrorMessage);

        var actual = await TranslateEnRuAsync(machine, ExamSource);

        // Полный перевод пришёл целиком, обязательное слово не потеряно.
        Assert.Equal(ExamTarget, actual);
        Assert.Contains("сеанс", actual);
        // Прежний неполный ответ больше не выдаётся.
        Assert.NotEqual(LessonOneTarget, actual);
        // Отказа тоже нет: это не откат к дословной подстановке.
        Assert.DoesNotContain("[EN_RU:BOOTSTRAP]", actual);
    }

    [Fact]
    public async Task PhraseComposition_ThreeTrainedRows_StayCorrect()
    {
        // Три учебные строки сами по себе отвечает точная память; перенос их не
        // касается, и они обязаны остаться правильными.
        var machine = new TranslationMachine();
        var train = await machine.TrainStructuredBatchAsync(BuildLessonBatch());
        Assert.True(train.IsSuccess, train.ErrorMessage);

        Assert.Equal(LessonOneTarget, await TranslateEnRuAsync(machine, LessonOneSource));
        Assert.Equal(LessonTwoTarget, await TranslateEnRuAsync(machine, LessonTwoSource));
        Assert.Equal(LessonThreeTarget, await TranslateEnRuAsync(machine, LessonThreeSource));
    }

    [Fact]
    public async Task PhraseComposition_EightWordEdge_StillRefusesAndFallsBack()
    {
        // Восемь слов попадают в область разрешения, покрытие растёт с двух слов до
        // трёх, но порог 0,85 всё равно не пройден: договорённый отказ сохраняется и
        // улучшение покрытия не выдаётся за правильный перевод.
        var machine = new TranslationMachine();
        var train = await machine.TrainStructuredBatchAsync(BuildLessonBatch());
        Assert.True(train.IsSuccess, train.ErrorMessage);

        var actual = await TranslateEnRuAsync(machine, EdgeEightSource);

        Assert.Contains("[EN_RU:BOOTSTRAP]", actual);
        Assert.DoesNotContain("сеанс", actual);
    }

    [Fact]
    public async Task PhraseComposition_NineWordEdge_StaysOutsideTheAdmittedArea()
    {
        // Девять слов длиннее разрешённого предела, поэтому работает прежний
        // сборщик и ответ остаётся прежним договорённым отказом.
        var machine = new TranslationMachine();
        var train = await machine.TrainStructuredBatchAsync(BuildLessonBatch());
        Assert.True(train.IsSuccess, train.ErrorMessage);

        var actual = await TranslateEnRuAsync(machine, EdgeNineSource);

        Assert.Contains("[EN_RU:BOOTSTRAP]", actual);
    }

    [Fact]
    public void Planner_EmptyLearnedMap_CoversNothing()
    {
        // Чистая проверка алгоритма без обучения: пустая выученная память ничего не
        // покрывает и ничего не предлагает.
        var plan = TranslationPhraseCompositionPlanner.Plan(
            ["close", "user", "session"],
            []);

        Assert.Equal(0, plan.CoveredSourceTokens);
        Assert.Equal(0, plan.PhraseSpanCount);
        Assert.Empty(plan.Parts);
        Assert.Empty(plan.ChosenSourceKeys);
    }

    [Fact]
    public void Planner_ExamRow_ChoosesTheFullCoverageSplit()
    {
        // Та же экзаменационная строка на уровне чистого алгоритма: выбирается
        // «close» + «user session», а не длинная «close user».
        var plan = TranslationPhraseCompositionPlanner.Plan(
            ["close", "user", "session"],
            LessonEdges());

        Assert.Equal(3, plan.CoveredSourceTokens);
        Assert.Equal(2, plan.PhraseSpanCount);
        Assert.Equal(new[] { LessonThreeTarget, LessonTwoTarget }, plan.Parts);
        Assert.Equal(new[] { "close", "user session" }, plan.ChosenSourceKeys);
    }

    [Fact]
    public void Planner_DenseLearnedMap_FindsFullCoverageWithFewestPhrases()
    {
        // Плотная память: восемь слов источника, две длинные фразы и все короткие
        // одновременно подходят. Поиск, который хранил бы только первые найденные
        // варианты, мог бы не дойти до полного покрытия; здесь оно обязательно.
        var plan = TranslationPhraseCompositionPlanner.Plan(
            ["a1", "a2", "a3", "a4", "a5", "a6", "a7", "a8"],
            DenseEdges());

        Assert.Equal(8, plan.CoveredSourceTokens);
        Assert.Equal(2, plan.PhraseSpanCount);
        Assert.Equal(new[] { "T1234", "T5678" }, plan.Parts);
    }

    [Fact]
    public void Planner_SameLearnedMapInReverseOrder_GivesTheSameAnswer()
    {
        // Тот же ответ при обратном порядке выученных записей: ответ не зависит от
        // того, в каком порядке память перечисляется.
        var forward = DenseEdges();
        var reversed = Enumerable.Reverse(forward).ToArray();

        var forwardPlan = TranslationPhraseCompositionPlanner.Plan(
            ["a1", "a2", "a3", "a4", "a5", "a6", "a7", "a8"], forward);
        var reversedPlan = TranslationPhraseCompositionPlanner.Plan(
            ["a1", "a2", "a3", "a4", "a5", "a6", "a7", "a8"], reversed);

        Assert.Equal(forwardPlan.CoveredSourceTokens, reversedPlan.CoveredSourceTokens);
        Assert.Equal(forwardPlan.PhraseSpanCount, reversedPlan.PhraseSpanCount);
        Assert.Equal(forwardPlan.Parts, reversedPlan.Parts);
        Assert.Equal(forwardPlan.ChosenSourceKeys, reversedPlan.ChosenSourceKeys);
    }

    [Fact]
    public void Planner_AlgorithmIdentity_IsRecorded()
    {
        // Имя правила выбора зафиксировано, чтобы отчёт мог сказать, какое правило
        // дало ответ, вместо того чтобы заставлять читателя догадываться.
        Assert.Equal("phrase-coverage-canonical-v1", TranslationPhraseCompositionPlanner.AlgorithmIdentity);
        Assert.Equal(8, TranslationPhraseCompositionPlanner.MaxPlannedSourceTokens);
    }

    [Fact]
    public void Planner_LongerSentenceThanAdmitted_IsNotTheProductScope()
    {
        // Сам алгоритм длину не ограничивает — ограничение стоит в продукте. Этот
        // тест фиксирует именно это распределение обязанностей.
        var plan = TranslationPhraseCompositionPlanner.Plan(
            ["close", "user", "session", "now", "please", "again", "today", "safely", "tomorrow"],
            LessonEdges());

        Assert.Equal(3, plan.CoveredSourceTokens);
        Assert.Equal(2, plan.PhraseSpanCount);
    }

    [Fact]
    public async Task PhraseComposition_RowWithLearnedSingleWords_KeepsThePreviousAssembler()
    {
        // У этой строки есть принятые однословные переводы, поэтому планировщик в неё
        // не заходит и прежний сборщик отвечает как раньше: выученные фразы вместо
        // отвергнутых однословных вариантов.
        var machine = new TranslationMachine();
        var train = await machine.TrainStructuredBatchAsync(new TranslationLearningBatch(
            BatchId: "r54-word-and-phrase-row",
            DomainTag: TranslationMachine.DomainTag,
            Examples:
            [
                BuildExecutorExample(
                    "r54-word-and-phrase",
                    "phrase training seed",
                    "Том быстро потушил огонь",
                    [
                        new ExecutorWordMapping("tom", SemanticConcept.UNKNOWN, "Том"),
                        new ExecutorWordMapping("quickly", SemanticConcept.UNKNOWN, "быстро"),
                        new ExecutorWordMapping("put", SemanticConcept.UNKNOWN, "положил"),
                        new ExecutorWordMapping("out", SemanticConcept.UNKNOWN, "наружу"),
                        new ExecutorWordMapping("the", SemanticConcept.UNKNOWN, "моей"),
                        new ExecutorWordMapping("fire", SemanticConcept.UNKNOWN, "огонь")
                    ],
                    [
                        new ExecutorPhraseMapping("put out", "потушил"),
                        new ExecutorPhraseMapping("the fire", "огонь")
                    ])
            ],
            SourceIds: [SourceId]));
        Assert.True(train.IsSuccess, train.ErrorMessage);

        var actual = await TranslateEnRuAsync(machine, "Tom quickly put out the fire");

        Assert.DoesNotContain("[EN_RU:BOOTSTRAP]", actual);
        Assert.Contains("потушил", actual);
        Assert.Contains("огонь", actual);
        Assert.DoesNotContain("моей", actual);
        Assert.DoesNotContain("положил", actual);
        Assert.DoesNotContain("наружу", actual);
    }

    [Fact]
    public async Task PhraseComposition_PhraseOnlyRow_KeepsItsPublishedAnswer()
    {
        // У этой строки однословных переводов нет, поэтому она идёт через
        // планировщик и обязана дать тот же ответ, что был опубликован раньше.
        var machine = new TranslationMachine();
        var train = await machine.TrainStructuredBatchAsync(new TranslationLearningBatch(
            BatchId: "r54-phrase-only-row",
            DomainTag: TranslationMachine.DomainTag,
            Examples:
            [
                BuildExecutorExample(
                    "r54-phrase-only",
                    "the blue house is empty",
                    "синий дом пустой",
                    [],
                    [
                        new ExecutorPhraseMapping("red house", "красный дом"),
                        new ExecutorPhraseMapping("is empty", "пустой")
                    ])
            ],
            SourceIds: [SourceId]));
        Assert.True(train.IsSuccess, train.ErrorMessage);

        var actual = await TranslateEnRuAsync(machine, "red house is empty");

        Assert.Equal("красный дом пустой", actual);
        Assert.DoesNotContain("синий", actual);
    }

    [Fact]
    public async Task PhraseComposition_RemovedLearnedPhrase_ChangesTheAnswer()
    {
        // Проба чувствительности: если убрать из выученной памяти один урок, ответ
        // обязан измениться. Значит ответ действительно зависит от выученного
        // содержимого, а не восстанавливается из ожидаемого текста.
        var machine = new TranslationMachine();
        var train = await machine.TrainStructuredBatchAsync(BuildLessonBatchWithout(LessonThreeId));
        Assert.True(train.IsSuccess, train.ErrorMessage);

        var actual = await TranslateEnRuAsync(machine, ExamSource);

        Assert.NotEqual(ExamTarget, actual);
    }

    [Fact]
    public async Task PhraseComposition_ReplacedLearnedTarget_ShowsInTheAnswer()
    {
        // Вторая проба чувствительности: заменённый выученный перевод обязан
        // появиться в ответе дословно.
        var machine = new TranslationMachine();
        var train = await machine.TrainStructuredBatchAsync(new TranslationLearningBatch(
            BatchId: "r54-replaced-target",
            DomainTag: TranslationMachine.DomainTag,
            Examples:
            [
                BuildPhraseLesson(LessonOneId, LessonOneSource, LessonOneTarget),
                BuildPhraseLesson(LessonTwoId, LessonTwoSource, "СЕАНСЗАМЕНА"),
                BuildPhraseLesson(LessonThreeId, LessonThreeSource, LessonThreeTarget)
            ],
            SourceIds: [SourceId]));
        Assert.True(train.IsSuccess, train.ErrorMessage);

        var actual = await TranslateEnRuAsync(machine, ExamSource);

        Assert.Contains("СЕАНСЗАМЕНА", actual);
        Assert.DoesNotContain("сеанс пользователя", actual);
    }

    // ── Восстановление: сохранённое состояние загружается в чистую машину ──────
    //
    // Остаток строки переносов «выбор выученных фраз покрывает весь исходный текст»
    // (FIX-P54-PHRASE-COVERAGE) требует квалифицировать восстановление: три урока учатся одной
    // партией, состояние сохраняется и загружается в НОВУЮ машину без переобучения, и после
    // загрузки главная строка панели и три учебные строки дают 4 из 4. Чувствительность
    // проверяется на копиях ЗАГРУЖЕННОГО состояния, живое загруженное состояние не портится.

    [Fact]
    public async Task Restore_ExportedStateImportedIntoFreshMachine_AnswersFourOfFourWithoutRetraining()
    {
        // Исходная машина учит три урока одной партией — это единственное обучение в этой проверке.
        var trained = new TranslationMachine();
        // Обучение обязано пройти, иначе сохранять нечего.
        var train = await trained.TrainStructuredBatchAsync(BuildLessonBatch());
        Assert.True(train.IsSuccess, train.ErrorMessage);

        // Чистая машина, в которую будет загружено состояние; до загрузки она ничего не знает.
        var fresh = new TranslationMachine();
        // Панель из четырёх строк читается на чистой машине, чтобы зафиксировать отсутствие знания.
        var before = await ReadRestorePanelAsync(fresh);
        // Ни одна строка не отвечена правильно: 0 из 4.
        Assert.Equal(0, before.Count(answer => answer.Correct));
        // Каждая строка получила договорённый отказ, а не случайно совпавший ответ.
        Assert.All(before, answer => Assert.Contains("[EN_RU:BOOTSTRAP]", answer.Actual));

        // Состояние обученной машины выгружается в запись контрольной точки (в памяти, без диска).
        var exported = trained.ExportCheckpoint();
        Assert.True(exported.IsSuccess, exported.ErrorMessage);
        // Та же запись загружается в чистую машину: нового обучения здесь нет.
        var imported = fresh.ImportCheckpoint(exported.Value!);
        Assert.True(imported.IsSuccess, imported.ErrorMessage);

        // Та же панель читается после загрузки.
        var after = await ReadRestorePanelAsync(fresh);
        // Все четыре строки правильны: главная и три учебные, 0 потерянных.
        Assert.Equal(4, after.Count(answer => answer.Correct));
        // Главная строка приходит целиком, с обязательным словом «сеанс».
        Assert.Equal(ExamTarget, after[0].Actual);

        // Показания прогона записываются в каталог улик этой попытки.
        WriteReceipt("restore-export-import-panel.json", new
        {
            Path = "ExportCheckpoint -> ImportCheckpoint into new TranslationMachine, no retraining",
            BeforeLoad = before,
            AfterLoad = after,
            CorrectBeforeLoad = before.Count(answer => answer.Correct),
            CorrectAfterLoad = after.Count(answer => answer.Correct)
        });
    }

    [Fact]
    public async Task Restore_SavedCheckpointFileLoadedFresh_AnswersFourOfFourWithoutRetraining()
    {
        // Дисковый путь восстановления — тот, которым продукт пользуется после перезапуска.
        var trained = new TranslationMachine();
        // Единственное обучение проверки — три урока одной партией.
        var train = await trained.TrainStructuredBatchAsync(BuildLessonBatch());
        Assert.True(train.IsSuccess, train.ErrorMessage);

        // Уникальный временный файл контрольной точки; после проверки он удаляется.
        var path = Path.Combine(Path.GetTempPath(), $"fix-p54-restore-{Guid.NewGuid():N}.json");
        try
        {
            // Состояние сохраняется на диск продуктовым способом.
            var saved = await trained.SaveCheckpointAsync(path);
            Assert.True(saved.IsSuccess, saved.ErrorMessage);
            // Файл загружается в полностью новую машину: ни один урок заново не проигрывается.
            var loaded = await TranslationMachine.LoadCheckpointAsync(path);
            Assert.True(loaded.IsSuccess, loaded.ErrorMessage);

            // Та же панель из четырёх строк читается на загруженной машине.
            var after = await ReadRestorePanelAsync(loaded.Value!.Machine);
            // Все четыре правильны, 0 потерянных.
            Assert.Equal(4, after.Count(answer => answer.Correct));
            // Главная строка — золотой ответ целиком.
            Assert.Equal(ExamTarget, after[0].Actual);

            // Показания: путь, размер файла на диске и ответы панели.
            WriteReceipt("restore-save-load-file-panel.json", new
            {
                Path = "SaveCheckpointAsync -> LoadCheckpointAsync (file on disk), no retraining",
                CheckpointFileBytes = new FileInfo(path).Length,
                AfterLoad = after,
                CorrectAfterLoad = after.Count(answer => answer.Correct)
            });
        }
        finally
        {
            // Временный файл не остаётся на диске независимо от исхода.
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public async Task Restore_CopiesOfLoadedStateWithoutThePhrase_TurnThePanelRedWhileTheLiveStateHolds()
    {
        // Обучение и загрузка в чистую машину — ровно как в проверке восстановления выше.
        var trained = new TranslationMachine();
        var train = await trained.TrainStructuredBatchAsync(BuildLessonBatch());
        Assert.True(train.IsSuccess, train.ErrorMessage);
        // Выгрузка обученного состояния.
        var exported = trained.ExportCheckpoint();
        Assert.True(exported.IsSuccess, exported.ErrorMessage);
        // Загрузка в новую машину без переобучения.
        var loaded = new TranslationMachine();
        var imported = loaded.ImportCheckpoint(exported.Value!);
        Assert.True(imported.IsSuccess, imported.ErrorMessage);

        // Копии строятся из состояния ЗАГРУЖЕННОЙ машины, а не исходной обученной.
        var loadedState = loaded.ExportCheckpoint();
        Assert.True(loadedState.IsSuccess, loadedState.ErrorMessage);
        // Фразовая память загруженного состояния обязана держать ровно три урока, иначе копии пусты.
        var loadedPhraseMap = EnRuExecutor(loadedState.Value!).PhraseMap;
        Assert.Equal(3, loadedPhraseMap.Count);
        // Урок «close» в ней есть — именно его удаляет первая копия.
        Assert.Contains(loadedPhraseMap, entry => string.Equals(entry.Key, LessonThreeSource, StringComparison.OrdinalIgnoreCase));

        // Копия 1: без выученной фразы «close». На ней утверждение «4 из 4» обязано стать ложным.
        var withoutClose = new TranslationMachine();
        // Копия загружается в отдельную машину; живая загруженная машина не трогается.
        var withoutCloseImport = withoutClose.ImportCheckpoint(WithEnRuPhraseMap(loadedState.Value!,
            map => map.Where(entry => !string.Equals(entry.Key, LessonThreeSource, StringComparison.OrdinalIgnoreCase)).ToList()));
        Assert.True(withoutCloseImport.IsSuccess, withoutCloseImport.ErrorMessage);
        // Та же панель читается на копии без «close».
        var panelWithoutClose = await ReadRestorePanelAsync(withoutClose);
        // Главная строка больше не золотая: без «close» полного покрытия нет.
        Assert.NotEqual(ExamTarget, panelWithoutClose[0].Actual);
        // Утверждение проверки восстановления на этой копии красное: правильных меньше четырёх.
        Assert.True(panelWithoutClose.Count(answer => answer.Correct) < 4,
            "панель на копии без «close» обязана потерять хотя бы одну строку");

        // Копия 2: заменённый выученный перевод «user session» обязан попасть в ответ дословно.
        var replaced = new TranslationMachine();
        // Замена делается только в копии записи: Value заменяется, Key остаётся.
        var replacedImport = replaced.ImportCheckpoint(WithEnRuPhraseMap(loadedState.Value!,
            map => map.Select(entry => string.Equals(entry.Key, LessonTwoSource, StringComparison.OrdinalIgnoreCase)
                ? entry with { Value = "СЕАНСЗАМЕНА" }
                : entry).ToList()));
        Assert.True(replacedImport.IsSuccess, replacedImport.ErrorMessage);
        // Главная строка на копии с заменой.
        var mainReplaced = await TranslateEnRuAsync(replaced, ExamSource);
        // Ответ зависит от загруженного выученного значения, а не восстанавливается из ожидания.
        Assert.Contains("СЕАНСЗАМЕНА", mainReplaced);
        Assert.DoesNotContain("сеанс пользователя", mainReplaced);

        // Копия 3: пустая фразовая память — главная строка не золотая.
        var cleared = new TranslationMachine();
        var clearedImport = cleared.ImportCheckpoint(WithEnRuPhraseMap(loadedState.Value!, _ => []));
        Assert.True(clearedImport.IsSuccess, clearedImport.ErrorMessage);
        // Главная строка на копии без фраз.
        var mainCleared = await TranslateEnRuAsync(cleared, ExamSource);
        Assert.NotEqual(ExamTarget, mainCleared);

        // Живая загруженная машина не пострадала от построения копий: по-прежнему 4 из 4.
        var live = await ReadRestorePanelAsync(loaded);
        Assert.Equal(4, live.Count(answer => answer.Correct));

        // Показания: ответы всех трёх копий и живого состояния.
        WriteReceipt("restore-sensitivity-on-loaded-copies.json", new
        {
            CopiesMadeFrom = "ExportCheckpoint of the LOADED machine (import of the trained export), not of the trained machine",
            CopyWithoutClose = new { Panel = panelWithoutClose, Correct = panelWithoutClose.Count(answer => answer.Correct) },
            CopyWithReplacedTarget = new { MainRow = mainReplaced },
            CopyWithClearedPhraseMap = new { MainRow = mainCleared },
            LiveLoadedState = new { Panel = live, Correct = live.Count(answer => answer.Correct) }
        });
    }

    // ── Расход памяти установленного планировщика в назначенной области ────────
    //
    // Число +75,18 % из карточки — расход ОТВЕРГНУТОГО прототипа (прямой обход в глубину, который
    // держал в памяти все готовые варианты). Расход установленного планировщика до этих проверок
    // не измерялся. Здесь на одном и том же наборе чтений и одной и той же выученной памяти
    // сравниваются два плеча: вызов установленного планировщика и копия фразового цикла прежнего
    // левонаправленного сборщика из продуктовой ветки вне области (TranslationTrainedUnits.cs:
    // 1070-1091) без однословного запасного пути — в области планировщика однословных переводов
    // нет по определению самой области, поэтому пропуск запасного пути только удешевляет плечо без
    // планировщика, и смещение сравнения направлено против планировщика. Общая для обеих веток
    // подготовка записей памяти и общий хвост (склейка частей, нормализация, проверка
    // совместимости) в измерение не входят.

    // Бюджеты выделенной памяти плеча с планировщиком — документированное кратное наблюдённого.
    // Прежний общий порог 262 144 байта был дымовой проверкой: он выше наблюдённого в 28,3 и 56,3
    // раза и не покраснел бы даже при десятикратном росте. Теперь каждый случай держит свой бюджет,
    // равный удвоенному наблюдённому числу, и сравнение строгое: ровно удвоение уже регрессия.
    // Точные наблюдённые числа по-прежнему идут в улики, не в утверждение.
    // Та же мерка, что и к прежнему порогу — иначе написанное выше несимметрично в пользу продукта:
    // строгая мерка приложена к прежнему порогу и не приложена к новому. Класс этой проверки —
    // предохранитель от катастрофы, а не охрана на уровне существенности владельца пять процентов.
    // Зелёным проходит любой рост меньше чем вдвое, включая плюс пятьдесят процентов: наибольшее
    // проходящее значение 18 495 байт против наблюдённых 9 248, а для плотного чтения 9 311 против
    // 4 656. Пять процентов существенности — это 9 710 и 4 888 байт, то есть полоса шире
    // существенности примерно в двадцать раз. Слово «строгое» выше относится только к тому, что
    // ровно удвоение уже красное, и не означает охраны на уровне существенности. Ограничение целиком:
    // artifacts/fbe/FIX-P54-PHRASE-COVERAGE/attempt-0001/allocation-measurement.md,
    // раздел «Вердикт по порогу».

    // Шесть чтений панели: наблюдено 9 248 байт при нулевом разбросе по семи повторам
    // (прогон 10.11.2026, независимо воспроизведено); удвоение считаем регрессией.
    private const long PlannerAllocationBudgetBytesSixReadings = 18_496;

    // Плотное чтение из восьми слов с четырнадцатью фразами: наблюдено 4 656 байт при нулевом
    // разбросе по семи повторам (тот же прогон, независимо воспроизведено); удвоение считаем регрессией.
    private const long PlannerAllocationBudgetBytesDenseReading = 9_312;

    [Fact]
    public async Task Allocation_InstalledPlannerVersusPreviousAssembler_SameSixReadings_IsMeasuredAndBounded()
    {
        // Выученная память берётся из настоящего обученного состояния, а не набирается руками.
        var trained = new TranslationMachine();
        var train = await trained.TrainStructuredBatchAsync(BuildLessonBatch());
        Assert.True(train.IsSuccess, train.ErrorMessage);
        // Выгрузка состояния даёт фразовую память исполнителя направления EN_RU как она есть.
        var exported = trained.ExportCheckpoint();
        Assert.True(exported.IsSuccess, exported.ErrorMessage);
        // Три выученные фразы в том виде, в каком их хранит исполнитель.
        var phraseMap = EnRuExecutor(exported.Value!).PhraseMap;
        Assert.Equal(3, phraseMap.Count);
        // Записи памяти в той же форме и порядке, что продукт строит для обоих сборщиков.
        var phraseEntries = BuildPhraseEntriesLikeTheProduct(phraseMap);

        // Шесть чтений панели карточки: главная, три учебные, край из 8 слов, край из 9 слов.
        string[] readings = [ExamSource, LessonOneSource, LessonTwoSource, LessonThreeSource, EdgeEightSource, EdgeNineSource];
        // Слова чтений нормализованы тем же правилом адреса, которым пользуется исполнитель.
        var readingTokens = readings.Select(NormalizeReadingLikeTheProduct).ToArray();

        // Поведенческая привязка на главной строке: планировщик даёт полное покрытие...
        var plannerExam = PlanWithInstalledPlanner(readingTokens[0], phraseEntries);
        Assert.Equal(new[] { LessonThreeTarget, LessonTwoTarget }, plannerExam.Parts);
        Assert.Equal(3, plannerExam.Covered);
        // ...а копия прежнего сборщика теряет «session» — ровно прежний контрпример.
        var previousExam = AssembleWithPreviousAssembler(readingTokens[0], phraseEntries);
        Assert.Equal(new[] { LessonOneTarget }, previousExam.Parts);
        Assert.Equal(2, previousExam.Covered);

        // Измерение: прогрев по разу на плечо, затем семь чередующихся повторов, медиана.
        var measurement = MeasureArms(readingTokens, phraseEntries, repeats: 7);

        // Оба плеча действительно что-то выделяют: измеритель живой, а не обнулён.
        Assert.True(measurement.PreviousMedianBytes > 0, "плечо прежнего сборщика выделило 0 байт — измеритель не живой");
        Assert.True(measurement.PlannerMedianBytes > 0, "плечо планировщика выделило 0 байт — измеритель не живой");
        // Расход планировщика на весь набор из шести чтений строго ниже удвоенного наблюдённого:
        // ровно удвоение (18 496) уже красное, потому что удвоение считаем регрессией.
        Assert.True(measurement.PlannerMedianBytes < PlannerAllocationBudgetBytesSixReadings,
            $"планировщик выделил {measurement.PlannerMedianBytes} байт на шесть чтений, а это не ниже бюджета {PlannerAllocationBudgetBytesSixReadings} (удвоенное наблюдённое 9 248)");

        // Показания: абсолютные байты обоих плеч по повторам, медианы, разница в процентах.
        WriteReceipt("allocation-planner-vs-previous-assembler-six-readings.json", new
        {
            // Плечо «без планировщика» — копия фразового цикла прежнего сборщика без однословного
            // запасного пути; это смещает сравнение против планировщика, что и записано в показании.
            Compared = "installed TranslationPhraseCompositionPlanner.Plan (+edge building, +parts) versus a copy of the previous left-to-right assembler's phrase loop WITHOUT its single-word fallback (TranslationTrainedUnits.cs:1070-1091); the omission biases the comparison against the planner; the same six readings and the same three learned phrases",
            Readings = readings,
            LearnedPhrases = phraseMap,
            Measurement = measurement,
            BudgetBytes = PlannerAllocationBudgetBytesSixReadings,
            ForeignNumber = "+75.18 % belongs to the REJECTED prototype (direct depth-first enumeration keeping every finished variant); it is not the installed planner's number and is not reproduced here",
            NotIncluded = "shared phraseEntries preparation; shared tail (string.Join, NormalizeComposedTranslation, target compatibility); the previous assembler's single-word fallback lookups, which cannot apply inside the planner's admitted area"
        });
    }

    [Fact]
    public void Allocation_DensestAdmittedCase_EightWordsFourteenPhrases_IsMeasuredAndBounded()
    {
        // Самый плотный случай внутри области: восемь слов источника и четырнадцать выученных фраз.
        var phraseEntries = DenseEdges()
            .Select(edge => (SourceKey: edge.SourceKey, SourceTokens: edge.SourceTokens.ToArray(), Target: edge.Target))
            .OrderByDescending(entry => entry.SourceTokens.Length)
            .ToArray();
        // Одно чтение из восьми слов — ровно предел области планировщика.
        string[][] readingTokens = [["a1", "a2", "a3", "a4", "a5", "a6", "a7", "a8"]];

        // Поведение обоих плеч на этом чтении записывается рядом с числами.
        var plannerDense = PlanWithInstalledPlanner(readingTokens[0], phraseEntries);
        Assert.Equal(8, plannerDense.Covered);
        var previousDense = AssembleWithPreviousAssembler(readingTokens[0], phraseEntries);

        // То же измерение: прогрев, семь чередующихся повторов, медиана.
        var measurement = MeasureArms(readingTokens, phraseEntries, repeats: 7);

        // Измеритель живой и расход строго ниже удвоенного наблюдённого: ровно удвоение (9 312) красное.
        Assert.True(measurement.PreviousMedianBytes > 0, "плечо прежнего сборщика выделило 0 байт — измеритель не живой");
        Assert.True(measurement.PlannerMedianBytes > 0, "плечо планировщика выделило 0 байт — измеритель не живой");
        Assert.True(measurement.PlannerMedianBytes < PlannerAllocationBudgetBytesDenseReading,
            $"планировщик выделил {measurement.PlannerMedianBytes} байт на плотное чтение, а это не ниже бюджета {PlannerAllocationBudgetBytesDenseReading} (удвоенное наблюдённое 4 656)");

        // Показания плотного случая.
        WriteReceipt("allocation-planner-vs-previous-assembler-dense-eight-words.json", new
        {
            Compared = "same two arms as the six-readings receipt, on one eight-word reading with fourteen learned phrases (DenseEdges)",
            PlannerParts = plannerDense.Parts,
            PlannerCovered = plannerDense.Covered,
            PreviousAssemblerParts = previousDense.Parts,
            PreviousAssemblerCovered = previousDense.Covered,
            Measurement = measurement,
            // Бюджет плотного чтения — удвоенное наблюдённое 4 656, см. константу выше.
            BudgetBytes = PlannerAllocationBudgetBytesDenseReading
        });
    }

    [Fact]
    public async Task Allocation_WholeProductReadingPathOnLoadedState_SixReadings_IsRecorded()
    {
        // Контекст для чисел планировщика: сколько выделяет ЦЕЛОЕ продуктовое чтение на загруженном
        // состоянии. Это не сравнение плеч: планировщик внутри области включён у пяти чтений из шести.
        var trained = new TranslationMachine();
        var train = await trained.TrainStructuredBatchAsync(BuildLessonBatch());
        Assert.True(train.IsSuccess, train.ErrorMessage);
        // Чтения идут на загруженной, а не на обученной машине — как после восстановления.
        var exported = trained.ExportCheckpoint();
        Assert.True(exported.IsSuccess, exported.ErrorMessage);
        var loaded = new TranslationMachine();
        var imported = loaded.ImportCheckpoint(exported.Value!);
        Assert.True(imported.IsSuccess, imported.ErrorMessage);

        // Те же шесть чтений панели.
        string[] readings = [ExamSource, LessonOneSource, LessonTwoSource, LessonThreeSource, EdgeEightSource, EdgeNineSource];
        // Прогрев: каждое чтение один раз до измерения.
        foreach (var reading in readings)
            await TranslateEnRuAsync(loaded, reading);

        // Семь повторов; считается память всего процесса, потому что продуктовое чтение
        // асинхронно и потоковый счётчик мог бы потерять часть выделений при смене потока.
        const int repeats = 7;
        var processBytes = new long[repeats];
        for (var repeat = 0; repeat < repeats; repeat++)
        {
            // Начало окна измерения.
            var start = GC.GetTotalAllocatedBytes(precise: true);
            // Шесть чтений подряд через весь продуктовый путь.
            foreach (var reading in readings)
                await TranslateEnRuAsync(loaded, reading);
            // Конец окна измерения.
            processBytes[repeat] = GC.GetTotalAllocatedBytes(precise: true) - start;
        }

        // Медиана по повторам; измеритель живой.
        var median = Median(processBytes);
        Assert.True(median > 0, "целое продуктовое чтение выделило 0 байт — измеритель не живой");

        // Показания: абсолютные байты шести продуктовых чтений, включая оснастку проверки.
        WriteReceipt("allocation-whole-product-reading-path-six-readings.json", new
        {
            Scope = "whole product reading path (orchestrator request, routing, tokenizer, executor) on the LOADED state, six readings per repeat, process-wide allocation; includes the test harness per call (Guid run id, metadata dictionary, request record); only this test class runs in the filtered run, and tests inside one class run sequentially",
            Readings = readings,
            ProcessBytesPerRepeat = processBytes,
            MedianBytes = median,
            MinBytes = processBytes.Min(),
            MaxBytes = processBytes.Max()
        });
    }

    // ── вспомогательное ────────────────────────────────────────────────────────

    // BuildLessonBatch собирает партию ровно из трёх уроков опыта 54.
    private static TranslationLearningBatch BuildLessonBatch()
        => new(
            BatchId: "r54-phrase-coverage-batch",
            DomainTag: TranslationMachine.DomainTag,
            Examples:
            [
                BuildPhraseLesson(LessonOneId, LessonOneSource, LessonOneTarget),
                BuildPhraseLesson(LessonTwoId, LessonTwoSource, LessonTwoTarget),
                BuildPhraseLesson(LessonThreeId, LessonThreeSource, LessonThreeTarget)
            ],
            SourceIds: [SourceId]);

    // BuildLessonBatchWithout собирает ту же партию без одного названного урока.
    private static TranslationLearningBatch BuildLessonBatchWithout(string skippedId)
    {
        var lessons = new List<TranslationLearningExample>();
        if (skippedId != LessonOneId)
            lessons.Add(BuildPhraseLesson(LessonOneId, LessonOneSource, LessonOneTarget));
        if (skippedId != LessonTwoId)
            lessons.Add(BuildPhraseLesson(LessonTwoId, LessonTwoSource, LessonTwoTarget));
        if (skippedId != LessonThreeId)
            lessons.Add(BuildPhraseLesson(LessonThreeId, LessonThreeSource, LessonThreeTarget));

        return new TranslationLearningBatch(
            BatchId: "r54-phrase-coverage-batch-without-" + skippedId,
            DomainTag: TranslationMachine.DomainTag,
            Examples: lessons,
            SourceIds: [SourceId]);
    }

    // BuildPhraseLesson строит один урок: ровно одно фразовое соответствие, ни
    // одного однословного.
    private static TranslationLearningExample BuildPhraseLesson(string id, string source, string target)
        => BuildExecutorExample(id, source, target, [], [new ExecutorPhraseMapping(source, target)]);

    // BuildExecutorExample строит учебную строку исполнителя с настоящей подписью
    // продуктового разборщика, чтобы адрес урока совпадал с адресом чтения.
    private static TranslationLearningExample BuildExecutorExample(
        string id,
        string source,
        string target,
        IReadOnlyList<ExecutorWordMapping> wordMappings,
        IReadOnlyList<ExecutorPhraseMapping> phraseMappings)
    {
        var signature = new TranslationSemanticTokenizer().Tokenize(source, "en").Signature;
        var chosen = new ExecutorOutput(target, 0.95, wordMappings, phraseMappings);
        var rejected = JsonSerializer.Serialize(
            new ExecutorOutput("wrong target-script output", 0.31, null), JsonOptions);

        return new TranslationLearningExample(
            ExampleId: id,
            UnitId: TranslationUnitIds.ExecutorFor(TranslationDirection.EN_RU),
            FlowId: TranslationRoutingFlow.FlowId,
            InputJson: JsonSerializer.Serialize(new ExecutorInput(source, "en", "general"), JsonOptions),
            ChosenOutputJson: JsonSerializer.Serialize(chosen, JsonOptions),
            RejectedOutputJson: rejected.TrimEnd('}')
                + ",\"wrong\":\"[WRONG: phrase_coverage_lost - required source word dropped.]\"}",
            RejectedType: "phrase_coverage_lost",
            SemanticSignature: signature,
            SourceId: SourceId,
            HeldOut: false);
    }

    // LessonEdges отдаёт три выученные фразы в виде входа планировщика.
    private static TranslationPhraseEdge[] LessonEdges()
        =>
        [
            new("close user", ["close", "user"], LessonOneTarget),
            new("user session", ["user", "session"], LessonTwoTarget),
            new("close", ["close"], LessonThreeTarget)
        ];

    // DenseEdges отдаёт плотную память: две длинные фразы, четыре парные и восемь
    // одиночных на одни и те же восемь слов источника.
    private static TranslationPhraseEdge[] DenseEdges()
        =>
        [
            new("a1 a2 a3 a4", ["a1", "a2", "a3", "a4"], "T1234"),
            new("a5 a6 a7 a8", ["a5", "a6", "a7", "a8"], "T5678"),
            new("a1 a2", ["a1", "a2"], "T12"),
            new("a3 a4", ["a3", "a4"], "T34"),
            new("a5 a6", ["a5", "a6"], "T56"),
            new("a7 a8", ["a7", "a8"], "T78"),
            new("a1", ["a1"], "T1"),
            new("a2", ["a2"], "T2"),
            new("a3", ["a3"], "T3"),
            new("a4", ["a4"], "T4"),
            new("a5", ["a5"], "T5"),
            new("a6", ["a6"], "T6"),
            new("a7", ["a7"], "T7"),
            new("a8", ["a8"], "T8")
        ];

    // TranslateEnRuAsync прогоняет строку через продуктовый путь целиком.
    private static async Task<string> TranslateEnRuAsync(TranslationMachine machine, string prompt)
    {
        var orchestratorResult = await machine.GetOrchestratorAsync();
        Assert.True(orchestratorResult.IsSuccess, orchestratorResult.ErrorMessage);

        var result = await orchestratorResult.Value!.RunAsync(new LogicalOrchestratorRequest(
            RunId: $"r54-phrase-coverage-{Guid.NewGuid():N}",
            ScenarioId: TranslationRoutingFlow.ScenarioId,
            UserRequest: prompt,
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>
            {
                ["source_language"] = "en",
                ["target_language"] = "ru",
                ["topic_domain"] = "general"
            }));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        return result.Value!.FinalHumanResult;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    // ── вспомогательное для восстановления ─────────────────────────────────────

    // RestoreAnswer — одно чтение панели восстановления: источник, ожидание, факт, совпало ли.
    private sealed record RestoreAnswer(string Source, string Expected, string Actual, bool Correct);

    // Панель восстановления: главная строка и три учебные — ровно четыре чтения, по которым
    // карточка требует 4 из 4 до и после загрузки сохранённого состояния.
    private static readonly (string Source, string Target)[] RestorePanel =
    [
        (ExamSource, ExamTarget),
        (LessonOneSource, LessonOneTarget),
        (LessonTwoSource, LessonTwoTarget),
        (LessonThreeSource, LessonThreeTarget)
    ];

    // ReadRestorePanelAsync читает четыре строки панели через продуктовый путь и сверяет их
    // с золотыми ответами дословно.
    private static async Task<IReadOnlyList<RestoreAnswer>> ReadRestorePanelAsync(TranslationMachine machine)
    {
        // Ответы накапливаются в порядке панели: главная строка всегда первая.
        var answers = new List<RestoreAnswer>(RestorePanel.Length);
        foreach (var (source, target) in RestorePanel)
        {
            // Одно продуктовое чтение.
            var actual = await TranslateEnRuAsync(machine, source);
            // Правильность — точное совпадение с золотым ответом.
            answers.Add(new RestoreAnswer(source, target, actual, string.Equals(actual, target, StringComparison.Ordinal)));
        }

        return answers;
    }

    // EnRuExecutor отдаёт запись исполнителя направления EN_RU из записи контрольной точки.
    private static ExecutorUnitCheckpoint EnRuExecutor(TranslationMachineCheckpoint checkpoint)
        => checkpoint.Executors.Single(executor => executor.Direction == TranslationDirection.EN_RU);

    // WithEnRuPhraseMap строит КОПИЮ записи контрольной точки, в которой фразовая память
    // исполнителя EN_RU переписана заданным правилом; исходная запись не меняется.
    private static TranslationMachineCheckpoint WithEnRuPhraseMap(
        TranslationMachineCheckpoint checkpoint,
        Func<IReadOnlyList<ExecutorStringMapEntryCheckpoint>, IReadOnlyList<ExecutorStringMapEntryCheckpoint>> rewrite)
    {
        // Только исполнитель EN_RU получает новую фразовую память; остальные копируются как есть.
        var executors = checkpoint.Executors
            .Select(executor => executor.Direction == TranslationDirection.EN_RU
                ? executor with { PhraseMap = rewrite(executor.PhraseMap) }
                : executor)
            .ToList();

        // Запись-копия с подменённым списком исполнителей.
        return checkpoint with { Executors = executors };
    }

    // ── вспомогательное для измерения памяти ───────────────────────────────────

    // ArmMeasurement — показания одного сравнения двух плеч: байты по повторам и медианы.
    private sealed record ArmMeasurement(
        long[] PlannerBytesPerRepeat,
        long[] PreviousAssemblerBytesPerRepeat,
        long PlannerMedianBytes,
        long PreviousMedianBytes,
        long PlannerMinBytes,
        long PlannerMaxBytes,
        long PreviousMinBytes,
        long PreviousMaxBytes,
        double PlannerMinusPreviousPercent);

    // ArmResult — поведение одного плеча на одном чтении: части ответа и покрытые слова.
    private sealed record ArmResult(IReadOnlyList<string> Parts, int Covered);

    // Приёмник возвращаемого значения плеча: чтобы работу плеча нельзя было счесть мёртвой.
    private static volatile int ArmChecksumSink;

    // BuildPhraseEntriesLikeTheProduct повторяет подготовку записей памяти из продукта
    // (TranslationTrainedUnits.cs:1025-1032): общий шаг обоих сборщиков, вне измерения.
    private static PhraseEntry[] BuildPhraseEntriesLikeTheProduct(IReadOnlyList<ExecutorStringMapEntryCheckpoint> phraseMap)
        => phraseMap
            .Select(entry => (
                SourceKey: entry.Key,
                SourceTokens: entry.Key.Split(' ', StringSplitOptions.RemoveEmptyEntries),
                Target: entry.Value))
            .Where(entry => entry.SourceTokens.Length > 0 && !string.IsNullOrWhiteSpace(entry.Target))
            .OrderByDescending(entry => entry.SourceTokens.Length)
            .ToArray();

    // NormalizeReadingLikeTheProduct режет чтение по пробелам и нормализует каждое слово тем же
    // правилом адреса, что и исполнитель (ExecutorUnit.NormalizeMapKey); для этих чтений без
    // знаков препинания это те же слова, что даёт разборщик.
    private static string[] NormalizeReadingLikeTheProduct(string reading)
        => reading
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(ExecutorUnit.NormalizeMapKey)
            .Where(token => !string.IsNullOrWhiteSpace(token))
            .ToArray();

    // PlanWithInstalledPlanner — плечо «с планировщиком» на одном чтении, ровно как продуктовая
    // ветка внутри области (TranslationTrainedUnits.cs:1050-1063): рёбра из записей памяти,
    // вызов планировщика, перенос частей ответа.
    private static ArmResult PlanWithInstalledPlanner(string[] sourceTokens, PhraseEntry[] phraseEntries)
    {
        // Части ответа — тот же список, что продукт заводит до ветвления.
        var parts = new List<string>();
        // Вызов установленного планировщика на рёбрах, построенных из записей памяти.
        var plan = TranslationPhraseCompositionPlanner.Plan(
            sourceTokens,
            phraseEntries
                .Select(entry => new TranslationPhraseEdge(entry.SourceKey, entry.SourceTokens, entry.Target))
                .ToArray());
        // Выбранные переведённые части становятся ответом в порядке планировщика.
        parts.AddRange(plan.Parts);
        // Покрытие берётся из фактического результата планировщика.
        return new ArmResult(parts, plan.CoveredSourceTokens);
    }

    // AssembleWithPreviousAssembler — плечо «без планировщика»: копия фразового цикла прежнего
    // левонаправленного сборщика (TranslationTrainedUnits.cs:1070-1091) без однословного запасного
    // пути; пропуск запасного пути смещает сравнение против планировщика.
    private static ArmResult AssembleWithPreviousAssembler(string[] sourceTokens, PhraseEntry[] phraseEntries)
    {
        // Части ответа — тот же список, что продукт заводит до ветвления.
        var parts = new List<string>();
        // Счётчик покрытых слов прежнего сборщика.
        var matchedTokens = 0;
        // Прежний сборщик идёт по словам слева направо.
        var i = 0;
        while (i < sourceTokens.Length)
        {
            // На каждом месте берётся первая (самая длинная) выученная фраза, которая здесь начинается.
            var phrase = phraseEntries.FirstOrDefault(entry => MatchesAt(sourceTokens, i, entry.SourceTokens));
            if (phrase.SourceTokens is { Length: > 0 })
            {
                // Фраза найдена: её перевод идёт в ответ, её слова считаются покрытыми.
                parts.Add(phrase.Target);
                matchedTokens += phrase.SourceTokens.Length;
                i += phrase.SourceTokens.Length;
                continue;
            }

            // В области планировщика ни у одного слова нет принятого однословного перевода, поэтому
            // однословный запасной путь прежнего сборщика (строки 1084-1089) здесь ничего не добавил бы:
            // слово просто пропускается.
            i++;
        }

        // Покрытие прежнего сборщика на этом чтении.
        return new ArmResult(parts, matchedTokens);
    }

    // MatchesAt — копия продуктовой проверки совпадения фразы на позиции (TranslationTrainedUnits.cs:1188-1198).
    private static bool MatchesAt(IReadOnlyList<string> sourceTokens, int start, IReadOnlyList<string> phraseTokens)
    {
        // Фраза, выходящая за конец источника, совпасть не может.
        if (start + phraseTokens.Count > sourceTokens.Count)
            return false;

        // Слова фразы сравниваются с источником по одному, без учёта регистра.
        for (var j = 0; j < phraseTokens.Count; j++)
            if (!string.Equals(sourceTokens[start + j], phraseTokens[j], StringComparison.OrdinalIgnoreCase))
                return false;

        return true;
    }

    // RunPlannerArm прогоняет плечо «с планировщиком» по всему набору чтений и отдаёт контрольную сумму.
    private static int RunPlannerArm(string[][] readings, PhraseEntry[] phraseEntries)
    {
        // Контрольная сумма складывается из покрытий и числа частей, чтобы работа была наблюдаема.
        var checksum = 0;
        foreach (var sourceTokens in readings)
        {
            // Одно чтение плеча.
            var result = PlanWithInstalledPlanner(sourceTokens, phraseEntries);
            checksum += result.Covered * 31 + result.Parts.Count;
        }

        return checksum;
    }

    // RunPreviousAssemblerArm прогоняет плечо «без планировщика» по тому же набору чтений.
    private static int RunPreviousAssemblerArm(string[][] readings, PhraseEntry[] phraseEntries)
    {
        // Та же контрольная сумма для того же набора чтений.
        var checksum = 0;
        foreach (var sourceTokens in readings)
        {
            // Одно чтение плеча.
            var result = AssembleWithPreviousAssembler(sourceTokens, phraseEntries);
            checksum += result.Covered * 31 + result.Parts.Count;
        }

        return checksum;
    }

    // MeasureThreadAllocatedBytes измеряет байты, выделенные ТЕКУЩИМ потоком внутри действия.
    // Действие синхронно и не меняет поток, иначе показание нельзя было бы приписать ему.
    private static long MeasureThreadAllocatedBytes(Func<int> action)
    {
        // Начальное показание счётчика потока; делегат уже создан вызывающим кодом до этой строки.
        var start = GC.GetAllocatedBytesForCurrentThread();
        // Само измеряемое действие.
        ArmChecksumSink = action();
        // Разница показаний — байты действия.
        return GC.GetAllocatedBytesForCurrentThread() - start;
    }

    // MeasureArms — прогрев по разу на плечо, затем чередующиеся повторы, медианы и разница.
    private static ArmMeasurement MeasureArms(string[][] readings, PhraseEntry[] phraseEntries, int repeats)
    {
        // Повторов не меньше пяти и нечётное число, чтобы медиана была одним наблюдённым значением.
        Assert.True(repeats >= 5 && repeats % 2 == 1, "повторов должно быть не меньше пяти и нечётное число");

        // Прогрев: первый вызов каждого плеча несёт цену первой компиляции и не считается.
        ArmChecksumSink = RunPlannerArm(readings, phraseEntries);
        ArmChecksumSink = RunPreviousAssemblerArm(readings, phraseEntries);

        // Байты по повторам для каждого плеча.
        var plannerBytes = new long[repeats];
        var previousBytes = new long[repeats];
        for (var repeat = 0; repeat < repeats; repeat++)
        {
            // Плечи чередуются, чтобы дрейф состояния процесса не доставался одному плечу.
            plannerBytes[repeat] = MeasureThreadAllocatedBytes(() => RunPlannerArm(readings, phraseEntries));
            previousBytes[repeat] = MeasureThreadAllocatedBytes(() => RunPreviousAssemblerArm(readings, phraseEntries));
        }

        // Медианы обоих плеч.
        var plannerMedian = Median(plannerBytes);
        var previousMedian = Median(previousBytes);
        // Разница в процентах относительно плеча без планировщика.
        var percent = previousMedian == 0
            ? double.NaN
            : Math.Round((plannerMedian - previousMedian) * 100.0 / previousMedian, 2);

        return new ArmMeasurement(
            plannerBytes,
            previousBytes,
            plannerMedian,
            previousMedian,
            plannerBytes.Min(),
            plannerBytes.Max(),
            previousBytes.Min(),
            previousBytes.Max(),
            percent);
    }

    // Median отдаёт медиану: для нечётного числа значений — среднее по порядку.
    private static long Median(long[] values)
    {
        // Значения упорядочиваются по возрастанию.
        var sorted = values.OrderBy(value => value).ToArray();
        // Середина упорядоченного ряда (для чётного числа — верхняя из двух средних).
        return sorted[sorted.Length / 2];
    }

    // ── вспомогательное для улик ───────────────────────────────────────────────

    // Приёмник улик этой попытки: назначенный карточкой (строка 170) каталог
    // artifacts/fbe/FIX-P54-PHRASE-COVERAGE/attempt-0001, подкаталог receipts. Пока каталог не
    // опубликован, прогон пишет в него; после публикации общий приёмник уводит прогон в свежий
    // каталог результатов испытаний и не трогает опубликованные байты.
    //
    // Почему ленивое поле заменено фикстурой класса. Ленивое поле рождало приёмник при ПЕРВОЙ записи
    // улики, то есть уже после уроков обучения этого испытания. Фикстура класса создаётся раньше
    // конструктора теста, поэтому захват адреса стал первым действием попытки, а завершение попытки
    // ставится ровно один раз.

    /// <summary>Корень рабочей копии для попытки этого испытания; его отсутствие — настоящий отказ.</summary>
    private static string ReceiptRepositoryRoot()
        // Отсутствие корня означает, что улики писать некуда, и это называется прямо.
        => RepositoryRootOrNull()
            ?? throw new InvalidOperationException("корень хранилища не найден подъёмом от " + AppContext.BaseDirectory);

    /// <summary>Назначенный каталог улик этого наряда: адрес не меняется этим ремонтом.</summary>
    private static string ReceiptDeclaredDirectory()
        // Тот же адрес, что был у ленивого поля: artifacts/fbe/FIX-P54-PHRASE-COVERAGE/attempt-0001/receipts.
        => Path.Combine(
            ReceiptRepositoryRoot(), "artifacts", "fbe", "FIX-P54-PHRASE-COVERAGE", "attempt-0001", "receipts");

    /// <summary>Захваченная попытка этого испытания: оснастка создаёт её до первого теста.</summary>
    public sealed class AttemptScopeFixture : ExperimentAttemptScope
    {
        /// <summary>Захватывает попытку, ставит её в поле класса и создаёт каталог записи.</summary>
        public AttemptScopeFixture()
            : base(
                ReceiptRepositoryRoot(),
                ReceiptDeclaredDirectory(),
                nameof(TranslationPhraseCoverageLearningTests))
        {
            // Поле класса заполняется только после успешного захвата: иначе области не существует.
            AttemptScope = this;

            // Каталог записи создаётся сразу, как это делало прежнее ленивое поле.
            Evidence.EnsureWriteRootDirectory();
        }
    }

    /// <summary>Захваченная попытка этого класса: её ставит фикстура до первого теста.</summary>
    private static AttemptScopeFixture? AttemptScope;

    /// <summary>Приёмник улик захваченной попытки этого класса.</summary>
    private static ExperimentFixes.ExperimentEvidenceSink ReceiptSink =>
        (AttemptScope ?? throw new InvalidOperationException(
            "Область попытки не захвачена: фикстура класса " +
            nameof(TranslationPhraseCoverageLearningTests) + " не была создана.")).Evidence;

    // RepositoryRootOrNull поднимается от каталога сборки до каталога с продуктом перевода и уликами.
    private static string? RepositoryRootOrNull()
    {
        // Старт — каталог, из которого загружена сборка проверок.
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            // Корень опознаётся по двум каталогам сразу: продукт перевода и каталог улик.
            if (Directory.Exists(Path.Combine(directory.FullName, "27.TranslationDomain")) &&
                Directory.Exists(Path.Combine(directory.FullName, "artifacts")))
                return directory.FullName;

            directory = directory.Parent;
        }

        return null;
    }

    // Запись улик: читаемый JSON с кириллицей как есть, без экранирования.
    private static readonly JsonSerializerOptions ReceiptJsonOptions = new()
    {
        WriteIndented = true,
        // Перевод строки между строками отступа — только «\n»: без этой настройки сериализатор на
        // Windows ставит «\r\n», и показания получают возвраты каретки, которых в принятом формате
        // общего приёмника улик нет (опубликованные показания несут ноль возвратов каретки).
        NewLine = "\n",
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() }
    };

    // WriteReceipt записывает одну улику в приёмник этой попытки.
    private static void WriteReceipt(string fileName, object payload)
    {
        // Улика сериализуется целиком до записи.
        var json = JsonSerializer.Serialize(payload, ReceiptJsonOptions);
        // Страховка: если возврат каретки всё же попал внутрь строкового значения, концы строк
        // приводятся к одному переводу строки до записи — нормализацию делает испытание, не приёмник.
        var normalized = json.Replace("\r\n", "\n");
        // Приёмник пишет байты как есть: UTF-8 без метки порядка байтов и без возвратов каретки —
        // ровно тот формат, в котором лежат опубликованные показания общего приёмника.
        ReceiptSink.WriteTextExactly(fileName, normalized + "\n");
    }
}
