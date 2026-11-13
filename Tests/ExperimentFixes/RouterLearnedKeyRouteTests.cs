// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using TrainableRouter;
using TrainableRouter.Contracts;
using TrainableRouter.Training;

// Что здесь проверяется, по-человечески.
//
// Это адресная приёмка переноса «имена языков и технологий записываются и ищутся одинаково»
// (строка работ R03 реестра переносов, код правки FIX-KR; карточка — файл 49 «одинаковое
// написание выученного ключа маршрутизации» в папке исправлений по опытам). Механизм в продукте
// уже принят и здесь не меняется ни на знак: читатель применяет те же две замены написания, что
// применил писатель, из собственных констант писателя и в том же порядке
// (26.TrainableRouter/Training/BaselineKeywordCandidateGenerator.cs, метод ScoreAsync,
// строки 37–39; константы писателя — RoutingProfileTrainer.cs, строка 17, применяются писателем
// в строках 96–97).
//
// Чего не хватало. Опыт мерил всё это в собственном стенде: он собирал распознаватель и поток
// маршрутизации у себя и смотрел на его ответ. До продуктового получателя — потока маршрутизации
// 26.TrainableRouter/Training/TrainableRoutingFlow.cs — ни одно из этих наблюдений не доходило.
// Поэтому никто не показал, что на восстановленной сохранённой памяти обычный маршрут продукта
// доводит запрос до нужного исполнителя, что охрана роли полномочий при этом жива и что
// происходит на границе, где выученное слово стоит внутри другого слова.
//
// Поэтому приёмка идёт через фактического получателя. Память берётся с диска готовой
// (Tests/Fixtures/ExperimentFixes/v1/RouterLearnedKey01/trained-profiles.json — копия поля
// профилей из сохранённых результатов опыта), обучение не запускается ни одного раза, а все
// ожидаемые значения записаны здесь руками дословными константами. Ни одно ожидаемое значение не
// берётся из того объекта, который проверяется.
//
// Какие числа подтверждены прогоном, а какие прочитаны из сохранённых улик.
// Прочитано из исходника продукта: обычный маршрут пускает выбор только при уверенности не ниже
// 0,40 и разрыве счёта не меньше 0,05 (26.TrainableRouter/Models/RoutingModels.cs: поля объявлены
// в строках 28–29, значения заданы в строке 32). Эти два числа повторены ниже дословными
// константами, и отдельная проверка сверяет, что продукт их не изменил.
// Прочитано из сохранённых результатов опыта: рукав с заменами отвечает на все четыре запроса
// верно с уверенностью 0,44, рукав без замен отказывает на двух пользовательских написаниях с
// уверенностью 0,20, а необученная память даёт уверенность 0,01
// (experiments-v2/RP01-MICRO/KR01/results/A.json и B.json, разделы «before» и «after»).
// Подтверждено прогоном этих проверок: всё, что утверждается ниже.
//
// Граница совпадения по подстроке — читать обязательно, иначе приёмка выйдет переоценённой.
// Базовый оценщик ищет выученное слово ПРОСТЫМ ВХОЖДЕНИЕМ ПОДСТРОКИ
// (BaselineKeywordCandidateGenerator.cs, строка 59: text.Contains(keyword)). Это его предел, а не
// семантическая правильность. Две последние проверки фиксируют наблюдённое поведение этого
// предела, а не желаемое: запрос из слов, которые лишь СОДЕРЖАТ выученные ключи, получает тот же
// маршрут и ту же уверенность, что настоящее имя технологии. Полной обученной маршрутизацией
// базовый оценщик здесь не объявляется, и роль полномочий он сам о себе сообщает как базовую, а
// не как обученную — это тоже проверено ниже.
public sealed class RouterLearnedKeyRouteTests
{
    // Опознавательное имя приспособления с сохранённой выученной памятью.
    private const string FixtureId = "RouterLearnedKey01";

    // Два исполнителя, между которыми выбирает маршрут: код и документация.
    private const string CodeExecutorId = "code";
    private const string DocsExecutorId = "docs";

    // Роль полномочий, которую базовый оценщик обязан сообщать о себе: базовый предлагатель
    // кандидатов, а не обученная единица. Строка записана руками, у продукта не запрашивается.
    private const string BaselineAuthorityRole = "baseline_candidate_generator";

    // Другая роль из того же закрытого словаря: обученная единица края знания. Она законна сама по
    // себе, но для ЭТОГО потока недопустима, и именно это проверяет охрана.
    private const string TrainedEdgeAuthorityRole = "trained_edge_unit";

    // Код отказа, которым поток встречает оценщика с чужой ролью полномочий.
    private const string AuthorityRoleRefusalCode = "ROUTING_SCORER_AUTHORITY_ROLE_INVALID";

    // Причина отказа «край знания», когда уверенность ниже порога.
    private const string ConfidenceBelowThresholdReason = "ROUTING_CONFIDENCE_BELOW_THRESHOLD";

    // Порог уверенности и наименьший разрыв счёта обычного маршрута. Прочитаны из исходника
    // продукта и записаны здесь руками; отдельная проверка в конце файла сверяет, что продукт их
    // не поменял, иначе смысл всех чисел ниже тихо поплыл бы.
    private const double MinimumRoutingConfidence = 0.40;
    private const double MinimumScoreGap = 0.05;

    // Уверенность при двух совпавших выученных словах: начальные 0,20 плюс два раза по 0,12.
    // Ровно это число записали сохранённые результаты опыта для всех четырёх верных ответов.
    private const double ConfidenceWhenTwoLearnedKeysMatch = 0.44;

    // Уверенность при одном совпавшем слове: 0,20 плюс 0,12. Ниже порога 0,40, то есть одного
    // совпадения для выбора маршрута не хватает.
    private const double ConfidenceWhenOneLearnedKeyMatches = 0.32;

    // Уверенность, когда выученной памяти нет вовсе: профиль не найден, и оценщик сообщает
    // наименьшее значение. Ровно это число записал раздел «до обучения» в улике опыта.
    private const double ConfidenceWithoutAnyLearnedMemory = 0.01;

    // Счёт при двух совпадениях и при полном промахе. Счёт — это число совпавших выученных слов.
    private const double ScoreWhenTwoLearnedKeysMatch = 2;
    private const double ScoreWhenOneLearnedKeyMatches = 1;
    private const double ScoreWhenNothingMatches = 0;

    // Размер сохранённой выученной памяти в сжатой записи, в байтах. Прочитан из поля
    // «profileUtf8Bytes» обоих рукавов опыта и записан здесь руками.
    private const int SavedLearnedMemoryCompactByteCount = 426;

    // Ссылка на источник урока внутри самих профилей: по ней видно, какой опыт их выучил.
    private const string LessonSourceRef = "KN-ROUTER-LEARNED-KEY-01";

    // ── Утверждение первое: обычный маршрут доводит исторические запросы до своих исполнителей ──

    // Четыре запроса опыта идут через продуктовый поток маршрутизации на восстановленной
    // сохранённой памяти. Каждый обязан дойти до своего эталонного исполнителя без отказа, с той
    // же уверенностью 0,44, которую записал опыт, и с базовой — не обученной — ролью полномочий.
    [Theory]
    [InlineData("C# .NET", CodeExecutorId)]
    [InlineData(".NET C#", CodeExecutorId)]
    [InlineData("csharp dotnet", CodeExecutorId)]
    [InlineData("manual guide", DocsExecutorId)]
    public async Task RestoredLearnedMemory_RoutesEveryHistoricalQueryToItsOwnGoldExecutor(
        string query, string goldExecutorId)
    {
        var decision = await RouteThroughProductFlowAsync(SavedLearnedMemory(), query);

        // Уверенность сверяется ПЕРВОЙ намеренно: если механизм когда-нибудь сломается, журнал
        // прогона покажет само наблюдённое число, а не только флаг отказа. Значение историческое,
        // знак в знак.
        Assert.Equal(ConfidenceWhenTwoLearnedKeysMatch, decision.Confidence);

        // Отказа «край знания» нет: маршрут выбран.
        Assert.False(decision.KnowledgeEdge);
        Assert.Null(decision.KnowledgeEdgeEvidence);

        // Выбран именно эталонный исполнитель этого запроса.
        Assert.Equal(goldExecutorId, decision.ChosenUnitId);

        // Полномочия выбора остались базовыми: обученной единицей этот оценщик себя не называет.
        Assert.Equal(BaselineAuthorityRole, decision.AuthorityRole);

        // Разрыв счёта тоже тот, что записал опыт: два совпадения против нуля у соседа.
        Assert.NotNull(decision.ScoreGap);
        Assert.Equal(goldExecutorId, decision.ScoreGap!.TopUnitId);
        Assert.Equal(ScoreWhenTwoLearnedKeysMatch, decision.ScoreGap.TopScore);
        Assert.Equal(ScoreWhenNothingMatches, decision.ScoreGap.SecondScore!.Value);
        Assert.True(decision.ScoreGap.MeetsMinimumScoreGap);
    }

    // ── Утверждение второе: закрываемый пробел ──

    // Это тот самый пробел, ради которого делался перенос. Пользователь пишет имена технологий
    // так, как они пишутся в жизни — «C# .NET» и «.NET C#», — а урок записан в памяти в служебном
    // написании «csharp» и «dotnet». До правки читатель искал не то написание, которое записал
    // писатель, и оба этих запроса получали отказ. Теперь оба обязаны дойти до исполнителя кода,
    // причём порядок слов в запросе значения не имеет.
    [Fact]
    public async Task UserSpellingOfCSharpAndDotNet_ReachesTheCodeExecutorInEitherWordOrder()
    {
        var memory = SavedLearnedMemory();

        foreach (var userSpelling in new[] { "C# .NET", ".NET C#" })
        {
            var decision = await RouteThroughProductFlowAsync(memory, userSpelling);

            // Уверенность сверяется первой по той же причине: поломка обязана назвать число.
            Assert.Equal(ConfidenceWhenTwoLearnedKeysMatch, decision.Confidence);
            Assert.True(decision.Confidence >= MinimumRoutingConfidence);

            // Маршрут выбран, и это исполнитель кода.
            Assert.False(decision.KnowledgeEdge);
            Assert.Equal(CodeExecutorId, decision.ChosenUnitId);

            // Оба выученных слова опознаны: счёт равен двум, а не одному.
            Assert.Equal(ScoreWhenTwoLearnedKeysMatch, decision.ScoreGap!.TopScore);
        }
    }

    // ── Утверждение третье: охрана роли полномочий жива ──

    // Поток обязан отказать оценщику, который называет себя не базовым предлагателем кандидатов.
    // Проверяются оба входа: и прямая оценка кандидатов, и обычный маршрут. Подделки две: чужая,
    // но законная сама по себе роль обученной единицы, и пустая роль. Поддельный оценщик живёт
    // внутри этого файла и в продукт не попадает.
    [Fact]
    public async Task ScorerThatClaimsAnotherAuthorityRole_IsRefusedByBothEntryPoints()
    {
        foreach (var foreignRole in new[] { TrainedEdgeAuthorityRole, string.Empty })
        {
            var flow = new TrainableRoutingFlow(
                new ScorerWithForeignAuthorityRole(foreignRole),
                new UnusedDecisionLog());

            // Прямая оценка кандидатов отказывает с названным кодом.
            var scored = await flow.ScoreAsync(Ask("C# .NET"));
            Assert.False(scored.IsSuccess);
            Assert.Equal(AuthorityRoleRefusalCode, scored.ErrorCode);

            // Обычный маршрут отказывает тем же кодом: отказ не теряется по дороге.
            var routed = await flow.RouteAsync(Ask("C# .NET"));
            Assert.False(routed.IsSuccess);
            Assert.Equal(AuthorityRoleRefusalCode, routed.ErrorCode);
        }
    }

    // ── Утверждение четвёртое: пустая память отказывает, а не отдаёт чужой маршрут ──

    // С пустым словарём памяти ни один профиль не находится. Поток обязан вернуть отказ «край
    // знания» и не назвать ни одного исполнителя. Чужой маршрут «на всякий случай» здесь был бы
    // самым опасным поведением, поэтому отсутствие выбранного исполнителя проверяется прямо.
    [Fact]
    public async Task EmptyLearnedMemory_RefusesWithKnowledgeEdgeAndNamesNoExecutor()
    {
        var decision = await RouteThroughProductFlowAsync(
            new Dictionary<string, UnitScoringProfile>(), "C# .NET");

        // Отказ «край знания» выставлен, и ни один исполнитель не выбран.
        Assert.True(decision.KnowledgeEdge);
        Assert.Null(decision.ChosenUnitId);

        // Причина отказа — уверенность ниже порога, и она названа явно.
        Assert.NotNull(decision.KnowledgeEdgeEvidence);
        Assert.Equal(ConfidenceBelowThresholdReason, decision.KnowledgeEdgeEvidence!.ReasonCode);

        // Уверенность равна наименьшей: ровно это число записал раздел «до обучения» улики опыта.
        Assert.Equal(ConfidenceWithoutAnyLearnedMemory, decision.Confidence);
        Assert.True(decision.Confidence < MinimumRoutingConfidence);

        // Оба кандидата вернулись без темы и роли: памяти о них нет вовсе.
        Assert.Equal(2, decision.RankedCandidates.Length);
        Assert.All(decision.RankedCandidates, candidate =>
        {
            Assert.Equal("unknown", candidate.UnitRole);
            Assert.Equal("unknown", candidate.DomainTag);
            Assert.Equal(ScoreWhenNothingMatches, candidate.Score);
        });
    }

    // ── Утверждение пятое: граница совпадения по подстроке ──

    // Выученное слово, стоящее ВНУТРИ другого слова, базовый оценщик всё равно считает
    // совпадением: «dotnetty» содержит «dotnet», «manuals» содержит «manual». Это наблюдённое
    // поведение, а не желаемое. Здесь одно совпадение даёт уверенность 0,32, а порог обычного
    // маршрута — 0,40, поэтому маршрут всё-таки не выбирается. Удерживает его именно порог
    // уверенности, а не разрыв счёта: разрыв как раз достаточен, и это проверяется отдельно,
    // чтобы не считать защиту более прочной, чем она есть.
    [Theory]
    [InlineData("dotnetty", CodeExecutorId)]
    [InlineData("manuals", DocsExecutorId)]
    public async Task LearnedKeyInsideAnotherWord_CountsAsAMatchButOneHitStaysBelowTheConfidenceFloor(
        string queryWithoutAnyRealTechnologyName, string executorWhoseKeyIsContained)
    {
        var decision = await RouteThroughProductFlowAsync(
            SavedLearnedMemory(), queryWithoutAnyRealTechnologyName);

        // Маршрут не выбран: отказ «край знания».
        Assert.True(decision.KnowledgeEdge);
        Assert.Null(decision.ChosenUnitId);
        Assert.Equal(ConfidenceBelowThresholdReason, decision.KnowledgeEdgeEvidence!.ReasonCode);

        // НАЗВАННЫЙ ПРЕДЕЛ: совпадение по подстроке всё-таки состоялось. Наверху списка стоит тот
        // исполнитель, чьё выученное слово спрятано внутри чужого, и его счёт равен единице, а не
        // нулю. Это и есть предел базового оценщика, видимый прямо в утверждении.
        Assert.Equal(executorWhoseKeyIsContained, decision.ScoreGap!.TopUnitId);
        Assert.Equal(ScoreWhenOneLearnedKeyMatches, decision.ScoreGap.TopScore);
        Assert.Equal(ConfidenceWhenOneLearnedKeyMatches, decision.Confidence);

        // Удержал отказ порог уверенности, а не разрыв счёта: разрыв требование выполняет.
        Assert.True(decision.Confidence < MinimumRoutingConfidence);
        Assert.True(decision.ScoreGap.MeetsMinimumScoreGap);
        Assert.True(decision.ScoreGap.ScoreGap >= MinimumScoreGap);
    }

    // Два слова, которые лишь СОДЕРЖАТ выученные ключи, дают уже два совпадения — и этого хватает,
    // чтобы обычный маршрут выбрал исполнителя с той же уверенностью 0,44, что у настоящего имени
    // технологии. Ни «dotnetty csharpy», ни «manuals guidelines» не являются именами технологий.
    // Это НАЗВАННЫЙ ПРЕДЕЛ базового оценщика, а не желаемое поведение продукта: отличить
    // настоящее имя от слова, которое его лишь содержит, он не умеет и полной обученной
    // маршрутизацией здесь не объявляется. Утверждение стоит в файле именно для того, чтобы этот
    // предел был виден, а не спрятан.
    [Theory]
    [InlineData("dotnetty csharpy", CodeExecutorId)]
    [InlineData("manuals guidelines", DocsExecutorId)]
    public async Task TwoWordsThatMerelyContainTheLearnedKeys_AreRoutedLikeRealTechnologyNames(
        string queryWithoutAnyRealTechnologyName, string executorThatWillBeChosen)
    {
        var decision = await RouteThroughProductFlowAsync(
            SavedLearnedMemory(), queryWithoutAnyRealTechnologyName);

        // Маршрут выбран, хотя ни одного настоящего имени технологии в запросе нет.
        Assert.False(decision.KnowledgeEdge);
        Assert.Equal(executorThatWillBeChosen, decision.ChosenUnitId);

        // И счёт, и уверенность совпали с историческим верным ответом знак в знак.
        Assert.Equal(ScoreWhenTwoLearnedKeysMatch, decision.ScoreGap!.TopScore);
        Assert.Equal(ConfidenceWhenTwoLearnedKeysMatch, decision.Confidence);

        // Полномочия всё те же базовые: обученной маршрутизацией этот ответ не является.
        Assert.Equal(BaselineAuthorityRole, decision.AuthorityRole);
    }

    // ── Утверждение шестое: продуктовый вход потока как единицы логики ──

    // Поток маршрутизации — это ещё и обычная единица логики продукта: её вызывают через
    // ExecuteAsync. Проверяется, что на том же восстановленном состоянии этот вход публикует
    // выбранного исполнителя, отсутствие края знания и базовую роль полномочий в своих
    // показателях. Так доказано, что до получателя доходит не только прямой вызов маршрута.
    [Fact]
    public async Task LogicalFlowEntryPoint_PublishesTheChosenExecutorAndItsBaselineAuthority()
    {
        var flow = FlowOverLearnedMemory(SavedLearnedMemory());
        var executed = await flow.ExecuteAsync(new LogicalFlowRequest<RoutingRequest>(
            "run-fix-kr-01",
            TrainableRoutingFlow.FlowId,
            "scenario-fix-kr-01",
            Ask("C# .NET"),
            new LogicalExecutionFlags(),
            new Dictionary<string, object>()));

        // Вызов прошёл без отказа.
        Assert.True(executed.IsSuccess, executed.ErrorMessage);

        // Вердикт говорит, что запрос направлен, а не отклонён на край знания.
        Assert.Equal("ROUTED", executed.Value!.OverallVerdict);

        // В показателях назван выбранный исполнитель, снят край знания и записана базовая роль.
        // Слово «False» записано здесь дословно, а не получено вызовом у продукта.
        Assert.Equal(CodeExecutorId, executed.Value.Metrics["chosen_unit_id"]);
        Assert.Equal("False", executed.Value.Metrics["knowledge_edge"]);
        Assert.Equal(
            BaselineAuthorityRole,
            executed.Value.Metrics[RoutingAuthorityVocabulary.MetadataKey]);
    }

    // ── Утверждение седьмое: приспособление несёт именно сохранённую память опыта ──

    // Если память в приспособлении молча подменить, все числа выше поплывут, а проверки останутся
    // зелёными. Поэтому содержимое памяти сверяется с дословными константами: два профиля, их
    // роли, темы, выученные слова, нижняя граница уверенности и ссылка на источник урока. Размер
    // сжатой записи сверяется с размером, который записал сам опыт.
    [Fact]
    public void FixtureMemoryIsExactlyTheSavedExperimentMemory()
    {
        var memory = SavedLearnedMemory();

        // Ровно два профиля: код и документация.
        Assert.Equal(2, memory.Count);

        // Профиль кода: роль исполнителя, тема программирования, два выученных слова.
        var code = memory[CodeExecutorId];
        Assert.Equal("executor", code.UnitRole);
        Assert.Equal("programming", code.DomainTag);
        Assert.Equal(new[] { "csharp", "dotnet" }, code.Keywords);
        Assert.Equal(new[] { "text" }, code.AllowedInputs);
        Assert.Equal(0.2, code.ConfidenceFloor);
        Assert.Equal(0.0, code.BaseScore);
        Assert.Equal(LessonSourceRef, code.MetadataSourceRef);

        // Профиль документации: та же роль, своя тема, свои два выученных слова.
        var docs = memory[DocsExecutorId];
        Assert.Equal("executor", docs.UnitRole);
        Assert.Equal("documentation", docs.DomainTag);
        Assert.Equal(new[] { "guide", "manual" }, docs.Keywords);
        Assert.Equal(new[] { "text" }, docs.AllowedInputs);
        Assert.Equal(0.2, docs.ConfidenceFloor);
        Assert.Equal(0.0, docs.BaseScore);
        Assert.Equal(LessonSourceRef, docs.MetadataSourceRef);

        // Сжатая запись памяти весит ровно столько, сколько записал опыт.
        Assert.Equal(
            SavedLearnedMemoryCompactByteCount,
            JsonSerializer.SerializeToUtf8Bytes(memory).Length);
    }

    // ── Утверждение восьмое: продукт не сдвинул те два числа, на которых стоят все остальные ──

    // Порог уверенности и наименьший разрыв счёта прочитаны из исходника продукта и повторены в
    // этом файле руками. Если продукт их поменяет, смысл чисел выше изменится молча: например
    // отказ при уверенности 0,32 станет выбором маршрута. Поэтому сверка стоит отдельным
    // утверждением, и ожидаемые значения здесь — дословные константы, а не чтение у продукта.
    [Fact]
    public void ProductDefaultThresholdsStillMatchTheNumbersThisAcceptanceReliesOn()
    {
        // Порог уверенности обычного маршрута.
        Assert.Equal(MinimumRoutingConfidence, RoutingFlowThresholds.Default.MinimumRoutingConfidence);

        // Наименьший разрыв счёта между первым и вторым кандидатом.
        Assert.Equal(MinimumScoreGap, RoutingFlowThresholds.Default.MinimumScoreGap);
    }

    // ── Общая часть: сборка получателя и чтение сохранённой памяти ──

    // Один запрос к маршруту. Поля повторяют то, что подавал хозяин опыта: сам запрос, пустой
    // отрезок задачи, неназванное направление, неназванная тема и два кандидата.
    private static RoutingRequest Ask(string query)
        => new(
            "request-fix-kr",
            query,
            string.Empty,
            null,
            null,
            [CodeExecutorId, DocsExecutorId],
            new Dictionary<string, string>());

    // Продуктовый поток маршрутизации, собранный на базовом оценщике с переданной памятью.
    // Пороги не подменяются: берутся те, что продукт применяет по умолчанию.
    private static TrainableRoutingFlow FlowOverLearnedMemory(
        IReadOnlyDictionary<string, UnitScoringProfile> memory)
        => new(new BaselineKeywordCandidateGenerator(memory), new UnusedDecisionLog());

    // Обычный маршрут продукта от запроса до решения. Отказ выдачи здесь был бы дефектом
    // постановки, а не ответом, поэтому успех вызова проверяется до разбора решения.
    private static async Task<RoutingDecision> RouteThroughProductFlowAsync(
        IReadOnlyDictionary<string, UnitScoringProfile> memory, string query)
    {
        var routed = await FlowOverLearnedMemory(memory).RouteAsync(Ask(query));
        Assert.True(routed.IsSuccess, routed.ErrorMessage);
        return routed.Value!;
    }

    // Сохранённая выученная память читается с диска как есть. Обучение не запускается ни одного
    // раза: это копия поля профилей из запечатанных результатов опыта, а не новый урок.
    private static IReadOnlyDictionary<string, UnitScoringProfile> SavedLearnedMemory()
    {
        var path = Path.Combine(
            RepositoryRoot(), "Tests", "Fixtures", "ExperimentFixes", "v1", FixtureId,
            "trained-profiles.json");
        if (!File.Exists(path))
            throw new FileNotFoundException("ROUTER_LEARNED_KEY_FIXTURE_NOT_FOUND:" + path, path);

        return JsonSerializer.Deserialize<Dictionary<string, UnitScoringProfile>>(
                   File.ReadAllBytes(path))
               ?? throw new InvalidOperationException("ROUTER_LEARNED_KEY_FIXTURE_UNREADABLE:" + path);
    }

    // Подъём от папки сборки до корня хранилища: путь приспособления не зависит от места запуска.
    private static string RepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "LlmFromScratch.sln")))
                return current.FullName;
            current = current.Parent;
        }

        throw new DirectoryNotFoundException("ROUTER_LEARNED_KEY_REPOSITORY_ROOT_NOT_FOUND");
    }

    // Поддельный оценщик для охраны роли полномочий: он возвращает одного годного кандидата, но
    // называет о себе чужую роль. Больше он не делает ничего и в продукт не попадает.
    private sealed class ScorerWithForeignAuthorityRole(string foreignAuthorityRole) : IUnitScorer
    {
        public Task<OperationResult<UnitScoringResult[]>> ScoreAsync(
            RoutingRequest request, CancellationToken ct = default)
            => Task.FromResult(OperationResult<UnitScoringResult[]>.Ok(
                [
                    new UnitScoringResult(CodeExecutorId, "executor", "programming", 2, 0.44)
                    {
                        AuthorityRole = foreignAuthorityRole
                    }
                ]));
    }

    // Журнал решений в этой приёмке не используется: запись решения лежит за её границей. Любой
    // вызов — дефект постановки, и он обязан быть громким, а не тихо зелёным.
    private sealed class UnusedDecisionLog : IRoutingDecisionLog
    {
        public Task<OperationResult<bool>> AppendAsync(
            RoutingDecisionRecord decision, CancellationToken ct = default)
            => throw new InvalidOperationException(
                "ROUTER_LEARNED_KEY_DECISION_LOG_IS_OUTSIDE_THIS_ACCEPTANCE");

        public Task<OperationResult<IReadOnlyList<RoutingDecisionRecord>>> ListAsync(
            CancellationToken ct = default)
            => throw new InvalidOperationException(
                "ROUTER_LEARNED_KEY_DECISION_LOG_IS_OUTSIDE_THIS_ACCEPTANCE");
    }
}
