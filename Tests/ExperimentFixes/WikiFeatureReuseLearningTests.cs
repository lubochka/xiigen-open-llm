// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using ProjectWikiDomain;
using Xunit;

// Перенос FIX-WC по плану 33-wiki-training-feature-reuse.md: признаки урока считаются
// один раз на строку вместо одного раза на строку в каждой эпохе. План доказывает, что
// обученное состояние при этом обязано совпасть побитово, потому что Featurize зависит
// только от текста урока и порядок вычислений сохранён.
//
// Этот тест закрепляет именно это требование: он обучает настоящую продуктовую модель на
// восьми закреплённых строках и сверяет каноническое состояние с числами, записанными
// опытом WIKI-FEATURES-ONCE-01 ДО переноса. Совпадение означает, что ускорение досталось
// бесплатно: ни один вес не изменился.
//
// Реализована часть основного Fact плана §5: оракул состояния по публичному API. Пункты
// плана, требующие internal RouteForEvaluation, паспорта и FBE-квитанций (приобретение
// 0→4/4, cold restore, gate guard), здесь НЕ реализованы — для них нужен общий harness
// из планов 02/03, которого в репозитории ещё нет.
//
// > Пометка 10.11.2026 (исполнитель): абзац выше сохранён как исходный и не переписан, но
// > он больше не описывает файл полностью. Три дописанных ниже случая закрывают именно те
// > три части, которые он называл нереализованными: приобретение 0 из 4 → 4 из 4, холодное
// > восстановление и целость публичного заслона — все три через существующий внутренний путь
// > чтения RouteForEvaluation, открытый тестовой сборке записью о видимости внутреннего в
// > 42.ProjectWikiDomain/42.ProjectWikiDomain.csproj. Что из абзаца выше остаётся верным:
// > общая оснастка карточки (запускатели Run-NativeAB/Compare-NativeAB, паспорт набора,
// > копии набора в Tests/Fixtures, второй файл проверок совместимости) по-прежнему не
// > построена, поэтому закреплённые байты читаются прямо из каталога опыта.
public sealed class WikiFeatureReuseLearningTests
{
    // Байты набора закреплены планом §3; если файл подменят, тест упадёт на этой проверке,
    // а не молча обучится на других строках.
    private const string FixtureSha256 =
        "e684017bcf44cb96c2951e2a49926d08cc9a869f38e5f229bedcda8cc23f8210";

    // Канонический отпечаток обученного состояния из опыта WC01, рука A (до переноса).
    private const string TrainedStateSha256 =
        "e81f918fd69a627c474fd546b3c36e342ef63eadf1b611d674db6db90ef74c59";

    // Состояние необученной модели из того же опыта: нулевые веса и смещения.
    private const string UntrainedStateSha256 =
        "68d955181059b3b4962ecbdfbb5a11b726a85cb003bfac34bc5022a3652952aa";

    private const int ExpectedFeatureCount = 264;
    private const int ExpectedParameterCount = 3975;
    private const int ExpectedNonZeroParameterCount = 590;

    // Те же параметры сериализации, что у измерителя опыта (Program.cs:16–22): от них
    // зависит отпечаток, поэтому менять их нельзя.
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact]
    public void FeatureReuse_PreservesTrainedStateExactly()
    {
        // Набор читается там, где он закреплён планом, и сверяется по байтам.
        var fixturePath = ResolveFixturePath();
        var fixtureBytes = File.ReadAllBytes(fixturePath);
        Assert.Equal(FixtureSha256, Convert.ToHexString(SHA256.HashData(fixtureBytes)).ToLowerInvariant());

        var fixture = JsonSerializer.Deserialize<Fixture>(fixtureBytes, JsonOptions);
        Assert.NotNull(fixture);
        Assert.Equal(8, fixture!.Train.Length);

        // Пары строятся ровно так же, как в измерителе опыта (Program.cs:72–77): иначе
        // обученное состояние будет другим и оракул перестанет быть применимым.
        var rows = fixture.Train.Select(row => new ProjectWikiRequestRoutingDpoPair(
            row.Id,
            "isolated-project-wiki-route",
            row.Prompt,
            row.ExpectedRouteKind,
            row.Prompt,
            $"route_kind: {row.ExpectedRouteKind}\ntarget_unit_id: {row.ExpectedTargetUnitId}",
            $"[WRONG: wrong_training_unit]\nroute_kind: {(row.ExpectedRouteKind == ProjectWikiRequestRouteKind.NotProjectWiki ? ProjectWikiRequestRouteKind.ProjectWikiRequestRouterTraining : ProjectWikiRequestRouteKind.NotProjectWiki)}",
            "wrong_training_unit")).ToArray();

        // Пустое обучение даёт нулевое состояние: это контроль, что оракул вообще подходит
        // к текущему контракту меток и кодировке признаков.
        var untrained = ProjectWikiRequestRoutingTrainableModel.Train([]);
        Assert.Equal(UntrainedStateSha256, CanonicalStateHash(untrained.ToSnapshot()));

        // Единственный непустой вызов обучения: именно его стоимость и меняет перенос.
        var trained = ProjectWikiRequestRoutingTrainableModel.Train(rows);
        var snapshot = trained.ToSnapshot();

        // Главная проверка переноса: обученное состояние обязано совпасть с записанным до
        // переноса. Любое расхождение означает, что вынос признаков изменил обучение.
        Assert.Equal(TrainedStateSha256, CanonicalStateHash(snapshot));

        // Те же три счётчика, что записал опыт: число признаков, всего параметров и
        // ненулевых среди них.
        Assert.Equal(ExpectedFeatureCount, snapshot.TokenWeights.Count);
        var parameterCount = snapshot.TokenWeights.Count * snapshot.Labels.Length + snapshot.Biases.Length;
        Assert.Equal(ExpectedParameterCount, parameterCount);
        var nonZero = snapshot.TokenWeights.Values.Sum(weights => weights.Count(weight => weight != 0.0))
            + snapshot.Biases.Count(bias => bias != 0.0);
        Assert.Equal(ExpectedNonZeroParameterCount, nonZero);

        // Обучение обязано изменить состояние: иначе совпадение отпечатков ничего не значит.
        Assert.NotEqual(UntrainedStateSha256, CanonicalStateHash(snapshot));

        // Ремонт R4 по аудиту 27.10, строка WC. Две оси из трёх названных им
        // закрываются здесь публичным входом; третья названа ниже как открытая.
        //
        // Холодное восстановление: потребитель обязан поднять то же состояние,
        // а не другое. Отпечаток берётся тот же канонический.
        var restored = ProjectWikiRequestRoutingTrainableModel.FromSnapshot(snapshot);
        Assert.Equal(TrainedStateSha256, CanonicalStateHash(restored.ToSnapshot()));

        // Публичная допускающая проверка: у обученной модели без приложенного
        // свидетельства о проверке на отложенных строках публичный вход обязан
        // отложить ответ, а не выдать маршрут. Это ровно тот случай, который
        // аудит называет «проверки нет».
        foreach (var model in new[] { trained, restored })
        foreach (var row in fixture.Train)
        {
            var decision = model.Route(row.Prompt);
            Assert.Contains("request_routing_missing_or_failed_eval_gate", decision.Warnings);
        }
    }

    // ------------------------------------------------------------------------------------
    // Адресная приёмка FIX-WC, попытка 0001. Закрывается ровно один названный пробел: обычное
    // чтение обученного и восстановленного состояния через ВНУТРЕННИЙ путь оценки
    // RouteForEvaluation, при сохранённом публичном заслоне. Три случая ниже дописаны, случай
    // выше не менялся. Общая оснастка карточки (запускатели, паспорт набора, копии набора в
    // Tests/Fixtures, второй файл проверок совместимости) НЕ строилась: она вне этого наряда,
    // поэтому закреплённые байты читаются прямо из каталога опыта, как и в случае выше.
    // ------------------------------------------------------------------------------------

    // Закреплённый нулевой снимок руки A: с него поднимается состояние ДО обучения, без
    // пустого обучения — ровно как велит карточка, раздел 5, пункт 1.
    private const string HistoricalBeforeSnapshotSha256 =
        "7d227e2810b8505477c1c60ff74e4e2972e978f58bbd507f36c6730487a77c9c";

    // Закреплённый обученный снимок руки A. Байты руки B тождественны, это независимое
    // подтверждение, а не второй экземпляр для нового обучения.
    private const string HistoricalAfterSnapshotSha256 =
        "4950d3a7fedca6ca0f1709e3162cdd4aa934e02cf42e1300ad5cafdc773bde2c";

    // Размер состояния при выгрузке, записанный опытом: 59 834 байта в обеих руках.
    private const int ExpectedSnapshotBytes = 59834;

    // Допуск сличения дробных чисел с историческим ТЕКСТОВЫМ показанием задан карточкой,
    // раздел 5, пункт 5: абсолютный 1e-12. Широкий произвольный допуск здесь запрещён.
    private const double HistoricalReadoutTolerance = 1e-12;

    // Четыре экзаменационных показания после обучения, как их записал опыт в обеих руках.
    private static readonly HistoricalExamReadout[] HistoricalAfterReadouts =
    [
        new("C-Q1", ProjectWikiRequestRouteKind.CodeTraceTraining, "code-trace-builder-unit",
            0.9131705672206922, 0.34958483092206005),
        new("C-Q2", ProjectWikiRequestRouteKind.CodeTraceTraining, "code-trace-builder-unit",
            0.896643663439551, 0.24908708399450114),
        new("C-Q3", ProjectWikiRequestRouteKind.WikiQuestionAnswer, "project-local-wiki-orchestrator",
            0.873445444240434, 0.12929049529308048),
        new("C-Q4", ProjectWikiRequestRouteKind.WikiQuestionAnswer, "project-local-wiki-orchestrator",
            0.8581786070844133, 0.0542570429079874)
    ];

    /// <summary>
    /// Приобретение навыка, прочитанное внутренним путём оценки: 0 верных из 4 до обучения и
    /// 4 верных из 4 после одного обучения на восьми закреплённых строках.
    ///
    /// Зачем этот случай нужен. Публичный вход закрыт заслоном отложенной проверки, поэтому
    /// через него обученная модель отвечает «отложено» и приобретение навыка через него
    /// не видно вовсе. Именно поэтому заголовок файла честно говорил, что эта часть плана не
    /// реализована. Внутренний путь оценки существует в продукте рядом с публичным и читает те
    /// же веса без заслона — он и есть законный измеритель обучения.
    /// </summary>
    [Fact]
    public void FrozenPanel_AcquiresRoutesThroughEvaluationReader()
    {
        // Набор и закреплённый нулевой снимок сверяются по байтам до любых модельных действий.
        var fixture = LoadFixture();
        var beforeSnapshot = LoadFrozenSnapshot("A", "before-snapshot.json", HistoricalBeforeSnapshotSha256);

        // Состояние ДО обучения поднимается восстановлением из снимка, а не пустым обучением:
        // так проверяется именно чтение восстановленного состояния.
        var before = ProjectWikiRequestRoutingTrainableModel.FromSnapshot(beforeSnapshot);
        Assert.Equal(UntrainedStateSha256, CanonicalStateHash(before.ToSnapshot()));

        // Четыре экзаменационных строки, прочитанные внутренним путём: у необученного состояния
        // ни одного выбранного маршрута быть не может, все четыре ответа отложены.
        var correctBefore = 0;
        foreach (var row in fixture.Exam)
        {
            var decision = before.RouteForEvaluation(row.Prompt);

            // Отказ приходит самой первой дверью «обученного сигнала нет», поэтому и вид
            // маршрута, и целевая единица, и числа у него строго предсказуемы.
            Assert.Equal(ProjectWikiRequestRouteKind.NotProjectWiki, decision.RouteKind);
            Assert.Equal("main-orchestrator", decision.TargetUnitId);
            Assert.Equal(ProjectWikiRequestRoutingDecisionStatus.NeedsSecondPass, decision.DecisionStatus);
            Assert.False(decision.CanStartTargetRunner);
            Assert.Equal(0.0, decision.Confidence);
            Assert.Equal(0.0, decision.TopTwoMargin);
            Assert.Contains("request_routing_model_untrained", decision.Warnings);

            if (IsCorrectlySelected(decision, row))
                correctBefore++;
        }

        // Главное число «до»: верных ноль из четырёх.
        Assert.Equal(0, correctBefore);

        // Единственное обучение: восемь строк в исходном порядке, восемнадцать эпох внутри.
        var trained = ProjectWikiRequestRoutingTrainableModel.Train(BuildTrainingPairs(fixture.Train));
        var trainedSnapshot = trained.ToSnapshot();

        // Обученное состояние обязано совпасть с записанным опытом: иначе ниже сравнивались бы
        // ответы другой модели, и числа приобретения ничего не значили бы.
        Assert.Equal(TrainedStateSha256, CanonicalStateHash(trainedSnapshot));
        Assert.Equal(ExpectedFeatureCount, trainedSnapshot.TokenWeights.Count);

        // Те же четыре строки после обучения: каждая обязана дать свой верный маршрут с теми
        // же уверенностью и запасом, что записал опыт.
        var correctAfter = 0;
        for (var index = 0; index < fixture.Exam.Length; index++)
        {
            var row = fixture.Exam[index];
            var historical = HistoricalAfterReadouts[index];

            // Порядок строк и их имена закреплены набором: сверка не по месту в массиве, а по
            // имени строки, чтобы подмена порядка не прошла молча.
            Assert.Equal(historical.Id, row.Id);

            var decision = trained.RouteForEvaluation(row.Prompt);
            AssertMatchesHistoricalReadout(decision, row, historical);

            if (IsCorrectlySelected(decision, row))
                correctAfter++;
        }

        // Главное число «после»: верных четыре из четырёх. Вместе с нулём выше это и есть
        // приобретение навыка 0 из 4 → 4 из 4.
        Assert.Equal(fixture.Exam.Length, correctAfter);
        Assert.Equal(4, correctAfter);

        // Обучение обязано изменить состояние: иначе «приобретение» было бы совпадением.
        Assert.NotEqual(CanonicalStateHash(beforeSnapshot), CanonicalStateHash(trainedSnapshot));
    }

    /// <summary>
    /// Холодное восстановление: выгруженное состояние поднимается в НОВЫЙ объект модели без
    /// обучения, и ответы внутреннего пути совпадают с ответами обученной модели до последнего
    /// бита, а размер состояния остаётся тем же — 59 834 → 59 834 байта.
    ///
    /// Зачем этот случай нужен. Указание реестра требует закрыть чтение не только обученного,
    /// но и ВОССТАНОВЛЕННОГО состояния. Случай выше в файле сверяет у восстановленной модели
    /// только отпечаток параметров; сами ответы потребителя он не читает.
    /// </summary>
    [Fact]
    public void ColdRestoredSnapshot_AnswersIdenticallyWithoutTraining()
    {
        var fixture = LoadFixture();

        // Одно обучение — тот же единственный неизбежный fit, что и в случае приобретения.
        var trained = ProjectWikiRequestRoutingTrainableModel.Train(BuildTrainingPairs(fixture.Train));

        // Выгрузка состояния: именно эти байты и есть «сохранённая модель».
        var exportedBytes = JsonSerializer.SerializeToUtf8Bytes(trained.ToSnapshot(), JsonOptions);

        // Размер состояния — то число, которое записал опыт в обеих руках.
        Assert.Equal(ExpectedSnapshotBytes, exportedBytes.Length);

        // И это не просто совпавшая длина: байты обязаны совпасть с закреплённым снимком руки A.
        Assert.Equal(
            HistoricalAfterSnapshotSha256,
            Convert.ToHexString(SHA256.HashData(exportedBytes)).ToLowerInvariant());

        // Холодная загрузка: снимок читается обратно из выгруженных байтов и поднимается в
        // НОВЫЙ объект модели. Обучение при этом не вызывается ни разу.
        var reread = JsonSerializer.Deserialize<ProjectWikiRequestRoutingModelSnapshot>(exportedBytes, JsonOptions);
        Assert.NotNull(reread);
        var restored = ProjectWikiRequestRoutingTrainableModel.FromSnapshot(reread!);

        // Это действительно другой объект, а не тот же самый: иначе «холодная загрузка» была бы
        // словом без содержания.
        Assert.NotSame(trained, restored);

        // Параметры совпали, и размер состояния после восстановления тот же: 59 834 → 59 834.
        var restoredBytes = JsonSerializer.SerializeToUtf8Bytes(restored.ToSnapshot(), JsonOptions);
        Assert.Equal(exportedBytes.Length, restoredBytes.Length);
        Assert.Equal(ExpectedSnapshotBytes, restoredBytes.Length);
        Assert.Equal(TrainedStateSha256, CanonicalStateHash(restored.ToSnapshot()));

        // Главное в этом случае: ответы потребителя. Четыре строки читаются у обученной и у
        // восстановленной модели, и сличаются по БИТАМ, а не по широкому допуску — так велит
        // карточка, раздел 5, пункт 5.
        for (var index = 0; index < fixture.Exam.Length; index++)
        {
            var row = fixture.Exam[index];
            var historical = HistoricalAfterReadouts[index];

            var fromTrained = trained.RouteForEvaluation(row.Prompt);
            var fromRestored = restored.RouteForEvaluation(row.Prompt);

            // Восстановленная модель обязана совпасть и с обученной, и с историческим показанием.
            AssertMatchesHistoricalReadout(fromRestored, row, historical);
            Assert.Equal(fromTrained.RouteKind, fromRestored.RouteKind);
            Assert.Equal(fromTrained.TargetUnitId, fromRestored.TargetUnitId);
            Assert.Equal(fromTrained.DecisionStatus, fromRestored.DecisionStatus);
            Assert.Equal(
                BitConverter.DoubleToInt64Bits(fromTrained.Confidence),
                BitConverter.DoubleToInt64Bits(fromRestored.Confidence));
            Assert.Equal(
                BitConverter.DoubleToInt64Bits(fromTrained.TopTwoMargin),
                BitConverter.DoubleToInt64Bits(fromRestored.TopTwoMargin));
        }

        // Вторая, независимая опора: то же холодное чтение, но из закреплённых байтов опыта, а
        // не из свежей выгрузки. Если потребитель потеряет веса или кодировку при чтении чужого
        // сохранённого файла, это увидит именно здесь.
        var frozen = ProjectWikiRequestRoutingTrainableModel.FromSnapshot(
            LoadFrozenSnapshot("A", "after-snapshot.json", HistoricalAfterSnapshotSha256));
        Assert.Equal(TrainedStateSha256, CanonicalStateHash(frozen.ToSnapshot()));
        for (var index = 0; index < fixture.Exam.Length; index++)
            AssertMatchesHistoricalReadout(
                frozen.RouteForEvaluation(fixture.Exam[index].Prompt),
                fixture.Exam[index],
                HistoricalAfterReadouts[index]);
    }

    /// <summary>
    /// Публичный заслон цел, и внутренний путь оценки стоит РЯДОМ с публичным, а не вместо него.
    ///
    /// Зачем этот случай нужен. Открыв тестам внутреннее, легко незаметно превратить приёмку в
    /// обход заслона: достаточно, чтобы публичный вход однажды начал отвечать «выбрано» без
    /// проверки на отложенных строках. Этот случай требует обратного — доказательства, что
    /// публичный вход по-прежнему откладывает, а порог заслона не понижен.
    /// </summary>
    [Fact]
    public void PublicRoute_StaysGatedBesideInternalEvaluationReader()
    {
        // Порог точности на отложенных строках сверяется буквально: понижение порога — это
        // ослабление заслона, даже если все ответы остались прежними.
        Assert.Equal(0.95, ProjectWikiRequestRoutingTrainableModel.MinimumHeldOutRouteAccuracy);

        // Публичный вход обязан существовать и быть публичным.
        var publicRoute = typeof(ProjectWikiRequestRoutingTrainableModel).GetMethod(
            "Route",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public,
            [typeof(string)]);
        Assert.NotNull(publicRoute);
        Assert.True(publicRoute!.IsPublic);

        // Внутренний путь оценки обязан существовать и быть видимым только внутри сборки
        // домена и дружественной тестовой сборке: ни публичным, ни приватным.
        var internalRoute = typeof(ProjectWikiRequestRoutingTrainableModel).GetMethod(
            "RouteForEvaluation",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
            [typeof(string)]);
        Assert.NotNull(internalRoute);
        Assert.True(internalRoute!.IsAssembly);
        Assert.False(internalRoute.IsPublic);
        Assert.False(internalRoute.IsPrivate);

        // Оба входа — это два входа, а не один: разные методы одного типа.
        Assert.NotEqual(publicRoute.MetadataToken, internalRoute.MetadataToken);

        var fixture = LoadFixture();
        var trained = ProjectWikiRequestRoutingTrainableModel.Train(BuildTrainingPairs(fixture.Train));
        var restored = ProjectWikiRequestRoutingTrainableModel.FromSnapshot(trained.ToSnapshot());

        for (var index = 0; index < fixture.Exam.Length; index++)
        {
            var row = fixture.Exam[index];
            var historical = HistoricalAfterReadouts[index];

            // У свежеобученной модели свидетельства о проверке нет вовсе: публичный вход
            // откладывает и называет точную причину — заслон к снимку не приложен.
            var publicDecision = trained.Route(row.Prompt);
            Assert.Equal(ProjectWikiRequestRoutingDecisionStatus.NeedsSecondPass, publicDecision.DecisionStatus);
            Assert.False(publicDecision.CanStartTargetRunner);
            Assert.Contains("request_routing_missing_or_failed_eval_gate", publicDecision.Warnings);
            Assert.Contains("request_routing_snapshot_missing_eval_gate", publicDecision.Warnings);

            // У восстановленной из снимка заслон приложен, но сам снимок записал «не пройден»:
            // причина отказа другая, и это тоже проверяется по точной примете.
            var restoredDecision = restored.Route(row.Prompt);
            Assert.Equal(ProjectWikiRequestRoutingDecisionStatus.NeedsSecondPass, restoredDecision.DecisionStatus);
            Assert.False(restoredDecision.CanStartTargetRunner);
            Assert.Contains("request_routing_missing_or_failed_eval_gate", restoredDecision.Warnings);
            Assert.Contains("request_routing_gate_not_passed_in_snapshot", restoredDecision.Warnings);

            // И тот самый смысл слова «рядом»: на ТОМ ЖЕ запросе у ТОЙ ЖЕ модели внутренний путь
            // выбирает верный маршрут, пока публичный откладывает. Значит заслон стоит на
            // публичном входе, а не вырезан ради приёмки.
            var evaluationDecision = trained.RouteForEvaluation(row.Prompt);
            AssertMatchesHistoricalReadout(evaluationDecision, row, historical);
            Assert.True(evaluationDecision.CanStartTargetRunner);
            Assert.NotEqual(publicDecision.DecisionStatus, evaluationDecision.DecisionStatus);
        }
    }

    // Одно место, где решается «ответ верный»: выбран маршрут И вид маршрута, И целевая единица
    // совпали с ожиданием набора. Отложенный ответ верным не считается — так же, как в
    // измерителе опыта (Program.cs:245).
    private static bool IsCorrectlySelected(ProjectWikiRequestRoutingDecision decision, FixtureRow row)
        => decision.DecisionStatus == ProjectWikiRequestRoutingDecisionStatus.Selected
           && decision.RouteKind == row.ExpectedRouteKind
           && string.Equals(decision.TargetUnitId, row.ExpectedTargetUnitId, StringComparison.Ordinal);

    // Сличение одного ответа с историческим показанием: сохраняются все оси, а не только
    // агрегат — вид маршрута, целевая единица, состояние решения, уверенность и запас.
    private static void AssertMatchesHistoricalReadout(
        ProjectWikiRequestRoutingDecision decision,
        FixtureRow row,
        HistoricalExamReadout historical)
    {
        // Ожидание набора и историческое показание обязаны говорить об одном и том же маршруте:
        // это защита от рассогласования двух источников ожиданий.
        Assert.Equal(row.ExpectedRouteKind, historical.ExpectedRouteKind);
        Assert.Equal(row.ExpectedTargetUnitId, historical.ExpectedTargetUnitId);

        Assert.Equal(historical.ExpectedRouteKind, decision.RouteKind);
        Assert.Equal(historical.ExpectedTargetUnitId, decision.TargetUnitId);
        Assert.Equal(ProjectWikiRequestRoutingDecisionStatus.Selected, decision.DecisionStatus);

        // Выбранный ответ не несёт предупреждений: в историческом показании список пуст.
        Assert.Empty(decision.Warnings);

        // Дробные числа — с допуском, назначенным карточкой, и ни на порядок шире.
        Assert.Equal(historical.Confidence, decision.Confidence, HistoricalReadoutTolerance);
        Assert.Equal(historical.TopTwoMargin, decision.TopTwoMargin, HistoricalReadoutTolerance);
    }

    // Пары строятся дословно так же, как в существующем случае выше и в измерителе опыта
    // (Program.cs:72–77). Повторение сделано намеренно: существующий случай наряд править
    // запрещает, поэтому его тело не выносится в общий помощник.
    private static ProjectWikiRequestRoutingDpoPair[] BuildTrainingPairs(FixtureRow[] rows)
        => rows.Select(row => new ProjectWikiRequestRoutingDpoPair(
            row.Id,
            "isolated-project-wiki-route",
            row.Prompt,
            row.ExpectedRouteKind,
            row.Prompt,
            $"route_kind: {row.ExpectedRouteKind}\ntarget_unit_id: {row.ExpectedTargetUnitId}",
            $"[WRONG: wrong_training_unit]\nroute_kind: {(row.ExpectedRouteKind == ProjectWikiRequestRouteKind.NotProjectWiki ? ProjectWikiRequestRouteKind.ProjectWikiRequestRouterTraining : ProjectWikiRequestRouteKind.NotProjectWiki)}",
            "wrong_training_unit")).ToArray();

    // Набор читается там же, где его читает существующий случай, и так же сверяется по байтам.
    private static Fixture LoadFixture()
    {
        var fixtureBytes = File.ReadAllBytes(ResolveFixturePath());
        Assert.Equal(FixtureSha256, Convert.ToHexString(SHA256.HashData(fixtureBytes)).ToLowerInvariant());

        var fixture = JsonSerializer.Deserialize<Fixture>(fixtureBytes, JsonOptions);
        Assert.NotNull(fixture);
        Assert.Equal(8, fixture!.Train.Length);
        Assert.Equal(4, fixture.Exam.Length);
        return fixture;
    }

    // Закреплённый снимок руки опыта читается по байтам и сверяется по отпечатку ДО разбора:
    // подмена файла обязана упасть здесь, а не проявиться странными числами ниже. Переносы
    // строк в этих файлах не нормализуются — отпечаток считается по сырым байтам.
    private static ProjectWikiRequestRoutingModelSnapshot LoadFrozenSnapshot(
        string arm,
        string fileName,
        string expectedSha256)
    {
        var path = Path.Combine(
            ResolveRepositoryRoot(),
            "experiments-v2", "RP01-MICRO", "WC01", "results", "attempt-001", arm, fileName);
        Assert.True(File.Exists(path), $"Закреплённый снимок опыта не найден: {path}");

        var bytes = File.ReadAllBytes(path);
        Assert.Equal(expectedSha256, Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant());

        var snapshot = JsonSerializer.Deserialize<ProjectWikiRequestRoutingModelSnapshot>(bytes, JsonOptions);
        Assert.NotNull(snapshot);
        return snapshot!;
    }

    // Корень хранилища ищется подъёмом от каталога сборки по тому же признаку, что и у
    // существующего случая, чтобы проверки не зависели от рабочего каталога запуска.
    private static string ResolveRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "AGENTS.md")))
            directory = directory.Parent;

        Assert.NotNull(directory);
        return directory!.FullName;
    }

    // Одно историческое показание экзаменационной строки: имя строки, ожидаемый маршрут и
    // целевая единица, плюс записанные опытом уверенность и запас между двумя лучшими.
    private sealed record HistoricalExamReadout(
        string Id,
        ProjectWikiRequestRouteKind ExpectedRouteKind,
        string ExpectedTargetUnitId,
        double Confidence,
        double TopTwoMargin);

    // Каноническая проекция состояния повторяет StateHash измерителя (Program.cs:269–276):
    // метки, смещения и веса признаков, упорядоченные по ключу порядковым сравнением.
    private static string CanonicalStateHash(ProjectWikiRequestRoutingModelSnapshot state)
        => Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(new
        {
            state.Labels,
            state.Biases,
            tokenWeights = state.TokenWeights
                .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                .Select(pair => new { token = pair.Key, weights = pair.Value })
        }, JsonOptions))).ToLowerInvariant();

    // Набор лежит в закреплённом планом месте внутри репозитория; корень ищется подъёмом
    // от каталога сборки, чтобы тест не зависел от рабочего каталога запуска.
    private static string ResolveFixturePath()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "AGENTS.md")))
            directory = directory.Parent;

        Assert.NotNull(directory);
        var path = Path.Combine(directory!.FullName,
            "experiments-v2", "RP01-MICRO", "WC01", "fixture.json");
        Assert.True(File.Exists(path), $"Закреплённый планом набор не найден: {path}");
        return path;
    }

    private sealed record Fixture(string ExperimentId, FixtureRow[] Train, FixtureRow[] Exam);

    private sealed record FixtureRow(
        string Id,
        string Prompt,
        ProjectWikiRequestRouteKind ExpectedRouteKind,
        string ExpectedTargetUnitId,
        string Group);
}
