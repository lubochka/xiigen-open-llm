// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Experiments;

namespace LlmFromScratch.Tests.ExperimentFixes;

// Приёмка переноса «иерархический урок содержит правильную последнюю цель»
// (строка очереди по адресу R04, строка файла 200 реестра исправлений по опытам;
// блок измерений около строки 444; карточка 53-hierarchical-local-history.md).
//
// Что здесь доказывается человеческими словами. Двухуровневый предсказатель
// учит одну таблицу переходов на каждый кластер событий. Чтобы прочитать
// таблицу целевого кластера, нужен номер события ИЗ ЭТОГО ЖЕ кластера. Прежний
// код подавал туда локальный номер текущего события, а оно принадлежит другому
// кластеру: одинаковое целое число там не означает одинаковое событие.
// Исправление адресует таблицу цели последним событием, которое в этой цели
// действительно наблюдали. Счётчики не меняются — меняется только адрес.
//
// Историческая панель опыта: семь событий оркестрации, сто пятьдесят два
// обучающих токена, шестнадцать экзаменационных запросов. Закреплённые
// показания опыта: прежний адрес даёт десять правильных из шестнадцати и
// совпадение ответов при перестановке членов кластера лишь четыре из
// шестнадцати; исправленный адрес — шестнадцать из шестнадцати и полное
// совпадение шестнадцать из шестнадцати.
//
// Проверка вызывает родной шов проекта 34, а не свою копию алгоритма: обучение
// идёт тем же BigramMarkov, чтение — тем же E05HierarchyReader, которого
// вызывает E05HierarchicalPrediction.Run. Ожидаемые значения взяты из
// закреплённых показаний опыта и из смысла договора, а не подогнаны под
// фактический вывод кода.
public sealed class HierarchicalLocalHistoryLearningTests
{
    // Семь событий оркестрации из исходных данных опыта; имена нужны только для
    // человеческого чтения отказа.
    private const int EventCount = 7;

    // Число кластеров: проверка входа, сохранение и удар сердца.
    private const int ClusterCount = 3;

    // Отпечатки целых файлов приспособления из паспорта: байты на диске.
    private const string TrainWholeFileSha256 = "02ed0917a36c20346fbec7df62625eabc685a59ad777f09e207e2b110ac74520";
    private const string ExamWholeFileSha256 = "0c4aa79ee1720c071e775d89810d3b784a1944e8993f14b48cfaff13bf8272eb";

    // Отпечатки содержимого из показаний опыта: другая каноническая запись,
    // поэтому с отпечатками файлов они не сравниваются.
    private const string TrainBeforeContentSha256 = "67526de56dd834bdf5471b63fe283a2320c9cc10d8eed94c4356e2880586fd8c";
    private const string TrainAfterContentSha256 = "c26d1b8d463e948992e2753da6963ddbfd7c38e4c0d3dcd1832793f1f804f102";
    private const string ExamContentSha256 = "ddb65bdec57b5553e97887c1b9ad1c8467085d0c8334ab9e18799367c2b2a237";

    // Закреплённое показание: единственная строка обученной таблицы ударов
    // сердца до обучения полного набора и после него.
    private static readonly int[] HeartbeatRowBeforeTraining = [15];
    private static readonly int[] HeartbeatRowAfterTraining = [31];

    [Fact]
    public void FixedClusters_ReusesTrainedStatesAndPreservesSemanticPermutation()
    {
        // ── Шаг 0. Происхождение исходника и приспособления до первого обучения ──

        // Сборка, в которой лежит проверяемый читатель: приёмка обязана назвать,
        // какой именно двоичный файл она проверяла.
        var nativeAssembly = typeof(BigramMarkov).Assembly;
        // Имя сборки фиксируется, чтобы случайная одноимённая копия из чужого
        // проекта не прошла за родной шов проекта 34.
        Assert.Equal("Experiments", nativeAssembly.GetName().Name);
        // Уникальный номер сборки записывается как происхождение показаний.
        var nativeModuleVersionId = nativeAssembly.ManifestModule.ModuleVersionId;
        Assert.NotEqual(Guid.Empty, nativeModuleVersionId);
        // Путь к собранному файлу подтверждает, что читался результат текущей сборки.
        Assert.False(string.IsNullOrWhiteSpace(nativeAssembly.Location));

        // Каталог приспособления с точными сохранёнными данными опыта.
        var fixtureDirectory = FixtureDirectory();
        var trainPath = Path.Combine(fixtureDirectory, "train.json");
        var examPath = Path.Combine(fixtureDirectory, "exam.json");
        var membersPath = Path.Combine(fixtureDirectory, "members.json");

        // Байты файлов сверяются с паспортом: дрейф данных — дефект постановки,
        // а не повод собрать новый набор.
        Assert.Equal(TrainWholeFileSha256, WholeFileSha256(trainPath));
        Assert.Equal(ExamWholeFileSha256, WholeFileSha256(examPath));

        // ── Шаг 1. Неизменяемые входные данные ───────────────────────────────

        using var trainDocument = JsonDocument.Parse(File.ReadAllBytes(trainPath));
        using var examDocument = JsonDocument.Parse(File.ReadAllBytes(examPath));
        using var membersDocument = JsonDocument.Parse(File.ReadAllBytes(membersPath));
        var trainRoot = trainDocument.RootElement;
        var examRoot = examDocument.RootElement;
        var membersRoot = membersDocument.RootElement;

        // Обучающая последовательность до обучения полного набора: шестнадцать ударов сердца.
        var trainBefore = ReadIntArray(trainRoot.GetProperty("train_before"));
        // Обучающая последовательность полного набора: сто пятьдесят два токена.
        var trainAfter = ReadIntArray(trainRoot.GetProperty("train_after"));

        // Та самая историческая панель, которую требует строка очереди.
        Assert.Equal(EventCount, trainRoot.GetProperty("event_names").GetArrayLength());
        Assert.Equal(16, trainBefore.Length);
        Assert.Equal(152, trainAfter.Length);

        // Отпечатки содержимого сверяются в своей, отдельной записи.
        Assert.Equal(TrainBeforeContentSha256, SequenceSha256(trainBefore));
        Assert.Equal(TrainAfterContentSha256, SequenceSha256(trainAfter));

        // Соответствие токена кластеру одинаково во всех руках проверки.
        var tokenToCluster = ReadIntMap(trainRoot.GetProperty("token_to_cluster"));
        // Канонический и переставленный состав членов кластеров.
        var canonicalMembers = ReadMembers(membersRoot.GetProperty("canonical_members"));
        var permutedMembers = ReadMembers(membersRoot.GetProperty("permuted_members"));
        // Локальная нумерация выводится из состава так же, как это делает обучение.
        var canonicalLocals = LocalNumbering(canonicalMembers);
        var permutedLocals = LocalNumbering(permutedMembers);

        // Переставлен ровно один кластер — сохранение; остальные не тронуты.
        Assert.Equal(new[] { 3, 4, 5 }, canonicalMembers[1].ToArray());
        Assert.Equal(new[] { 5, 3, 4 }, permutedMembers[1].ToArray());
        Assert.Equal(canonicalMembers[0].ToArray(), permutedMembers[0].ToArray());
        Assert.Equal(canonicalMembers[2].ToArray(), permutedMembers[2].ToArray());

        // Два независимых экзаменационных потока: межкластерный и удар сердца.
        var crossStream = ReadStream(examRoot, "cross_cluster");
        var heartbeatStream = ReadStream(examRoot, "same_cluster_control");
        // Всего шестнадцать запросов: двенадцать межкластерных и четыре удара сердца.
        Assert.Equal(12, crossStream.QueryCount);
        Assert.Equal(4, heartbeatStream.QueryCount);
        Assert.Equal(16, examRoot.GetProperty("total_queries").GetInt32());
        Assert.Equal(ExamContentSha256, ExamSha256(examRoot));

        // Правильные ответы экзаменатора: следующее событие после запрошенного.
        var crossGold = GoldAnswers(crossStream);
        var heartbeatGold = GoldAnswers(heartbeatStream);
        // Межкластерный поток чередует проверку входа и сохранение.
        Assert.Equal(new[] { 0, 3, 1, 4, 2, 5, 0, 3, 1, 4, 2, 5 }, crossGold);
        Assert.Equal(new[] { 6, 6, 6, 6 }, heartbeatGold);

        // ── Шаг 2. Состояние до обучения полного набора и два его прочтения ──

        // Одно обучение на шестнадцати ударах сердца.
        var stateBefore = E05LocalHierarchy.Fit(trainBefore, tokenToCluster, canonicalMembers, ClusterCount);

        // Предпосылки подмены паспорта: у состояния до обучения нет таблиц
        // проверки входа и сохранения, а нумерация ударов сердца не меняется.
        // Только поэтому переставленное прочтение допустимо делать тем же
        // обученным состоянием без повторного обучения.
        Assert.Null(stateBefore.WithinFor(0));
        Assert.Null(stateBefore.WithinFor(1));
        Assert.NotNull(stateBefore.WithinFor(2));
        Assert.Equal(canonicalLocals[6], permutedLocals[6]);

        // Единственная строка обученной таблицы ударов сердца до обучения полного набора.
        Assert.Equal(HeartbeatRowBeforeTraining, NativeCounts(stateBefore.WithinFor(2)!)[0]);

        // Переставленный взгляд на то же обученное состояние: те же экземпляры
        // таблиц, другая карта членов и локальных номеров, обучения нет.
        var stateBeforePermutedView = new E05HierarchyState(
            stateBefore.ClusterBigram,
            stateBefore.WithinBigrams,
            tokenToCluster,
            permutedLocals,
            permutedMembers);

        // Четыре прочтения состояния до обучения: два потока в двух нумерациях.
        var beforeCanonicalCross = RunStream(stateBefore, crossStream);
        var beforeCanonicalHeartbeat = RunStream(stateBefore, heartbeatStream);
        var beforePermutedCross = RunStream(stateBeforePermutedView, crossStream);
        var beforePermutedHeartbeat = RunStream(stateBeforePermutedView, heartbeatStream);

        // ── Шаг 3. Обучение полного набора в каноническом порядке ────────────

        var stateAfterCanonical = E05LocalHierarchy.Fit(trainAfter, tokenToCluster, canonicalMembers, ClusterCount);
        // После обучения полного набора есть таблица каждого кластера.
        Assert.NotNull(stateAfterCanonical.WithinFor(0));
        Assert.NotNull(stateAfterCanonical.WithinFor(1));
        Assert.NotNull(stateAfterCanonical.WithinFor(2));
        // Строка ударов сердца выросла с пятнадцати переходов до тридцати одного.
        Assert.Equal(HeartbeatRowAfterTraining, NativeCounts(stateAfterCanonical.WithinFor(2)!)[0]);

        var afterCanonicalCross = RunStream(stateAfterCanonical, crossStream);
        var afterCanonicalHeartbeat = RunStream(stateAfterCanonical, heartbeatStream);

        // ── Шаг 4. Обучение того же набора с переставленной нумерацией ───────

        // Это отдельное нужное обучение, а не подмена паспорта: таблицы с
        // непустыми счётчиками так перенумеровывать нельзя.
        var stateAfterPermuted = E05LocalHierarchy.Fit(trainAfter, tokenToCluster, permutedMembers, ClusterCount);
        Assert.Equal(HeartbeatRowAfterTraining, NativeCounts(stateAfterPermuted.WithinFor(2)!)[0]);

        var afterPermutedCross = RunStream(stateAfterPermuted, crossStream);
        var afterPermutedHeartbeat = RunStream(stateAfterPermuted, heartbeatStream);

        // ── Шаг 5. Два ограниченных договора на готовом каноническом состоянии ──

        // Отпечатки таблиц снимаются до и после этих чтений: счётчики не должны
        // измениться, потому что чтение ничего не обучает.
        var tablesBeforeContracts = TableFingerprints(stateAfterCanonical);

        // Истории целевого кластера ещё нет: сохраняется прежнее родное поведение.
        var noTargetHistoryReader = new E05HierarchyReader(stateAfterCanonical);
        noTargetHistoryReader.Reset();
        noTargetHistoryReader.Observe(0);
        var noTargetHistoryRead = ReadOrFail(noTargetHistoryReader);

        // История не протекает между независимыми потоками.
        var resetBetweenStreamsReader = new E05HierarchyReader(stateAfterCanonical);
        resetBetweenStreamsReader.Reset();
        resetBetweenStreamsReader.Observe(5);
        resetBetweenStreamsReader.Reset();
        resetBetweenStreamsReader.Observe(0);
        var resetBetweenStreamsRead = ReadOrFail(resetBetweenStreamsReader);

        var tablesAfterContracts = TableFingerprints(stateAfterCanonical);

        // ── Шаг 6. Приёмка по записанным показаниям ──────────────────────────

        // До обучения полного набора: межкластерные ответы всегда откатываются
        // на первого члена кластера проверки входа, поэтому правильны только два
        // запроса, где правильным ответом и была проверка входа.
        Assert.Equal(new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, Answers(beforeCanonicalCross));
        Assert.Equal(2, CorrectCount(beforeCanonicalCross, crossGold));
        // Удар сердца — настоящий заученный урок: четыре из четырёх.
        Assert.Equal(new[] { 6, 6, 6, 6 }, Answers(beforeCanonicalHeartbeat));
        Assert.Equal(4, CorrectCount(beforeCanonicalHeartbeat, heartbeatGold));
        // Итог до обучения: шесть из шестнадцати, и это не приобретённый навык.
        Assert.Equal(6, CorrectCount(beforeCanonicalCross, crossGold) + CorrectCount(beforeCanonicalHeartbeat, heartbeatGold));
        // До обучения перестановка ничего не меняет: совпадение шестнадцать из шестнадцати.
        Assert.Equal(16, InvarianceCount(beforeCanonicalCross, beforePermutedCross) + InvarianceCount(beforeCanonicalHeartbeat, beforePermutedHeartbeat));

        // После обучения, канонический порядок: все двенадцать межкластерных
        // ответов правильные, построчно, а не только по общей доле.
        Assert.Equal(crossGold, Answers(afterCanonicalCross));
        Assert.Equal(12, CorrectCount(afterCanonicalCross, crossGold));
        // Прежний заученный урок не сломан: четыре из четырёх.
        Assert.Equal(heartbeatGold, Answers(afterCanonicalHeartbeat));
        Assert.Equal(4, CorrectCount(afterCanonicalHeartbeat, heartbeatGold));

        // После обучения, переставленный порядок: те же правильные ответы.
        Assert.Equal(crossGold, Answers(afterPermutedCross));
        Assert.Equal(12, CorrectCount(afterPermutedCross, crossGold));
        Assert.Equal(heartbeatGold, Answers(afterPermutedHeartbeat));
        Assert.Equal(4, CorrectCount(afterPermutedHeartbeat, heartbeatGold));

        // Шестнадцать из шестнадцати в каждой нумерации и полное совпадение
        // глобальных ответов между нумерациями: это и есть сохранение смысла.
        Assert.Equal(16, CorrectCount(afterCanonicalCross, crossGold) + CorrectCount(afterCanonicalHeartbeat, heartbeatGold));
        Assert.Equal(16, CorrectCount(afterPermutedCross, crossGold) + CorrectCount(afterPermutedHeartbeat, heartbeatGold));
        Assert.Equal(16, InvarianceCount(afterCanonicalCross, afterPermutedCross) + InvarianceCount(afterCanonicalHeartbeat, afterPermutedHeartbeat));

        // Защита от пустой перестановки: обученная таблица сохранения обязана
        // реально отличаться между нумерациями, иначе совпадение ответов
        // ничего не доказывало бы.
        Assert.NotEqual(
            TableFingerprint(stateAfterCanonical.WithinFor(1)),
            TableFingerprint(stateAfterPermuted.WithinFor(1)));
        // А непереставленные кластеры и переходы между кластерами обязаны совпасть.
        Assert.Equal(
            TableFingerprint(stateAfterCanonical.WithinFor(0)),
            TableFingerprint(stateAfterPermuted.WithinFor(0)));
        Assert.Equal(
            TableFingerprint(stateAfterCanonical.WithinFor(2)),
            TableFingerprint(stateAfterPermuted.WithinFor(2)));
        Assert.Equal(
            TableFingerprint(stateAfterCanonical.ClusterBigram),
            TableFingerprint(stateAfterPermuted.ClusterBigram));

        // Главный разбираемый признак: в основном окне каждый межкластерный
        // запрос читает таблицу цели по последнему событию, наблюдённому именно
        // в этой цели. Независимый учёт наблюдений считает этот адрес сам, не
        // повторяя обученную таблицу.
        AssertTargetHistoryAddressing(afterCanonicalCross, crossStream, tokenToCluster, canonicalLocals);
        AssertTargetHistoryAddressing(afterPermutedCross, crossStream, tokenToCluster, permutedLocals);
        AssertTargetHistoryAddressing(afterCanonicalHeartbeat, heartbeatStream, tokenToCluster, canonicalLocals);
        AssertTargetHistoryAddressing(afterPermutedHeartbeat, heartbeatStream, tokenToCluster, permutedLocals);

        // Основное окно не содержит случая отсутствующей истории цели: иначе
        // непроверенная опытом ветвь молча попала бы в измеренное качество.
        Assert.Equal(0, MissingTargetHistoryCount(afterCanonicalCross, crossStream, tokenToCluster));
        Assert.Equal(0, MissingTargetHistoryCount(afterPermutedCross, crossStream, tokenToCluster));

        // Первый ограниченный договор: истории цели нет, адрес остаётся прежним,
        // ответ — сохранение схемы, как в прежнем родном поведении.
        Assert.Equal(1, noTargetHistoryRead.PredictedCluster);
        Assert.True(noTargetHistoryRead.WithinPredictCalled);
        Assert.Equal(0, noTargetHistoryRead.ReaderInputLocal);
        Assert.True(noTargetHistoryRead.HasGlobal);
        Assert.Equal(4, noTargetHistoryRead.Global);

        // Второй ограниченный договор: прежнее сохранение политики не протекает
        // в новый поток. Утечка дала бы сохранение входа вместо схемы.
        Assert.Equal(1, resetBetweenStreamsRead.PredictedCluster);
        Assert.Equal(0, resetBetweenStreamsRead.ReaderInputLocal);
        Assert.True(resetBetweenStreamsRead.HasGlobal);
        Assert.Equal(4, resetBetweenStreamsRead.Global);
        Assert.NotEqual(3, resetBetweenStreamsRead.Global);

        // Чтение ничего не обучает: отпечатки таблиц до и после договоров равны.
        Assert.Equal(tablesBeforeContracts, tablesAfterContracts);
    }

    // ── Прогон одного экзаменационного потока ────────────────────────────────

    // Один независимый поток читается новым читателем и всегда начинается со
    // сброса. Предсказателю не передаётся правильный ответ и не передаётся
    // будущая часть последовательности: наблюдается только дошедший токен.
    private static List<ReaderObservation> RunStream(E05HierarchyState state, ExamStream stream)
    {
        var reader = new E05HierarchyReader(state);
        reader.Reset();
        var records = new List<ReaderObservation>();
        for (var index = 0; index < stream.Tokens.Length && records.Count < stream.QueryCount; index++)
        {
            // Событие произошло — читатель узнаёт о нём, и только о нём.
            reader.Observe(stream.Tokens[index]);
            // До назначенной первой позиции запроса экзаменатор ничего не спрашивает.
            if (index < stream.FirstQueryIndex) continue;
            var read = ReadOrFail(reader);
            records.Add(new ReaderObservation(
                index,
                stream.Tokens[index],
                read.HasGlobal,
                read.Global,
                read.SourceCluster,
                read.PredictedCluster,
                read.ReaderInputLocal,
                read.WithinPredictCalled));
        }

        // Назначенное число запросов обязано быть выполнено целиком.
        Assert.Equal(stream.QueryCount, records.Count);
        return records;
    }

    // Один вызов читателя; неуспех показывается с родным сообщением отказа.
    private static E05HierarchyRead ReadOrFail(E05HierarchyReader reader)
    {
        var read = reader.PredictNext();
        Assert.True(read.IsSuccess, read.ErrorMessage);
        return read.Value!;
    }

    // ── Независимый учёт наблюдений вместо второй копии алгоритма ────────────

    // Проверяет, что поданный в таблицу цели адрес равен локальному номеру
    // последнего события, наблюдённого в предсказанном кластере. Этот учёт
    // использует только состав кластеров и уже наблюдённые токены; обученную
    // таблицу он не пересчитывает.
    private static void AssertTargetHistoryAddressing(
        List<ReaderObservation> records,
        ExamStream stream,
        IReadOnlyDictionary<int, int> tokenToCluster,
        IReadOnlyDictionary<int, int> tokenToLocal)
    {
        foreach (var record in records)
        {
            // Если таблицы предсказанного кластера нет, адрес не выбирался.
            if (!record.WithinPredictCalled) continue;
            var expected = LastObservedLocalInCluster(stream, record.LastObservedIndex, record.PredictedCluster, tokenToCluster, tokenToLocal)
                           ?? tokenToLocal[record.LastObservedToken];
            Assert.Equal(expected, record.ReaderInputLocal);
        }
    }

    // Сколько запросов окна прошли без истории предсказанного кластера.
    private static int MissingTargetHistoryCount(
        List<ReaderObservation> records,
        ExamStream stream,
        IReadOnlyDictionary<int, int> tokenToCluster)
    {
        var missing = 0;
        foreach (var record in records)
        {
            // Ищем в уже наблюдённом отрезке хоть одно событие целевого кластера.
            var seen = false;
            for (var index = record.LastObservedIndex; index >= 0; index--)
                if (tokenToCluster[stream.Tokens[index]] == record.PredictedCluster) { seen = true; break; }
            if (!seen) missing++;
        }

        return missing;
    }

    // Локальный номер последнего наблюдённого события заданного кластера.
    private static int? LastObservedLocalInCluster(
        ExamStream stream,
        int lastObservedIndex,
        int cluster,
        IReadOnlyDictionary<int, int> tokenToCluster,
        IReadOnlyDictionary<int, int> tokenToLocal)
    {
        for (var index = lastObservedIndex; index >= 0; index--)
        {
            var token = stream.Tokens[index];
            if (tokenToCluster[token] == cluster) return tokenToLocal[token];
        }

        return null;
    }

    // ── Счёт правильных ответов и совпадений между нумерациями ──────────────

    // Выданные глобальные ответы в порядке запросов; неуспешный ответ обозначается минусом.
    private static int[] Answers(List<ReaderObservation> records)
        => records.Select(record => record.HasGlobal ? record.Global : -1).ToArray();

    // Сверка с правильными ответами происходит только после получения ответа.
    private static int CorrectCount(List<ReaderObservation> records, int[] gold)
    {
        var correct = 0;
        for (var index = 0; index < records.Count; index++)
            if (records[index].HasGlobal && records[index].Global == gold[index]) correct++;
        return correct;
    }

    // Сколько ответов совпали по глобальному событию между двумя нумерациями.
    private static int InvarianceCount(List<ReaderObservation> canonical, List<ReaderObservation> permuted)
    {
        var same = 0;
        for (var index = 0; index < canonical.Count; index++)
        {
            var left = canonical[index];
            var right = permuted[index];
            if (left.HasGlobal && right.HasGlobal && left.Global == right.Global) same++;
        }

        return same;
    }

    // ── Чтение обученных таблиц только для показаний ────────────────────────

    // Заранее назначенное чтение закрытого поля счётчиков, как в программе
    // опыта. Записи в состояние здесь нет; строки копируются независимо.
    // Отсутствие поля или другой его тип — несовместимость, а не мягкий пропуск.
    private static int[][] NativeCounts(BigramMarkov model)
    {
        var field = typeof(BigramMarkov).GetField("_counts", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        var raw = field!.GetValue(model);
        var jagged = Assert.IsType<int[][]>(raw);
        return jagged.Select(row => row.ToArray()).ToArray();
    }

    // Отпечаток обученной таблицы: показывает, менялись ли счётчики.
    private static string TableFingerprint(BigramMarkov? model)
    {
        // Необученного кластера у состояния может не быть вовсе.
        if (model is null) return "none";
        var text = string.Join("|", NativeCounts(model).Select(row => string.Join(",", row)));
        return Sha256Hex(Encoding.UTF8.GetBytes(text));
    }

    // Отпечатки всех таблиц состояния: переходы между кластерами и внутри них.
    private static string TableFingerprints(E05HierarchyState state)
        => string.Join(
            ";",
            new[] { TableFingerprint(state.ClusterBigram) }
                .Concat(Enumerable.Range(0, ClusterCount).Select(cluster => TableFingerprint(state.WithinFor(cluster)))));

    // ── Чтение приспособления ────────────────────────────────────────────────

    // Подъём от папки сборки до корня хранилища: путь не зависит от места запуска.
    private static string FixtureDirectory()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "LlmFromScratch.sln")))
                return Path.Combine(current.FullName, "Tests", "Fixtures", "ExperimentFixes", "v1", "HierarchicalLocalHistory01");
            current = current.Parent;
        }

        throw new DirectoryNotFoundException("HIERARCHICAL_LOCAL_HISTORY_FIXTURE_ROOT_NOT_FOUND");
    }

    // Последовательность целых чисел из массива показаний.
    private static int[] ReadIntArray(JsonElement element)
        => element.EnumerateArray().Select(item => item.GetInt32()).ToArray();

    // Карта «токен — число» из объекта показаний.
    private static Dictionary<int, int> ReadIntMap(JsonElement element)
        => element.EnumerateObject().ToDictionary(pair => int.Parse(pair.Name), pair => pair.Value.GetInt32());

    // Состав членов каждого кластера в заданном порядке.
    private static Dictionary<int, List<int>> ReadMembers(JsonElement element)
        => element.EnumerateObject().ToDictionary(pair => int.Parse(pair.Name), pair => ReadIntArray(pair.Value).ToList());

    // Локальная нумерация: номер события равен его месту в списке своего кластера.
    private static Dictionary<int, int> LocalNumbering(Dictionary<int, List<int>> members)
    {
        var locals = new Dictionary<int, int>();
        foreach (var (_, clusterMembers) in members)
            for (var index = 0; index < clusterMembers.Count; index++)
                locals[clusterMembers[index]] = index;
        return locals;
    }

    // Один названный экзаменационный поток с его окном запросов.
    private static ExamStream ReadStream(JsonElement examRoot, string name)
    {
        foreach (var sequence in examRoot.GetProperty("sequences").EnumerateArray())
        {
            if (sequence.GetProperty("name").GetString() != name) continue;
            return new ExamStream(
                name,
                ReadIntArray(sequence.GetProperty("tokens")),
                sequence.GetProperty("first_query_index").GetInt32(),
                sequence.GetProperty("query_count").GetInt32());
        }

        throw new InvalidDataException($"HIERARCHICAL_LOCAL_HISTORY_EXAM_STREAM_NOT_FOUND:{name}");
    }

    // Правильные ответы экзаменатора: событие, следующее за запрошенным.
    private static int[] GoldAnswers(ExamStream stream)
        => Enumerable.Range(0, stream.QueryCount)
            .Select(offset => stream.Tokens[stream.FirstQueryIndex + offset + 1])
            .ToArray();

    // ── Отпечатки ────────────────────────────────────────────────────────────

    // Отпечаток байтов файла как они лежат на диске.
    private static string WholeFileSha256(string path) => Sha256Hex(File.ReadAllBytes(path));

    // Отпечаток содержимого последовательности: числа через запятую, затем UTF-8.
    private static string SequenceSha256(int[] tokens)
        => Sha256Hex(Encoding.UTF8.GetBytes(string.Join(",", tokens)));

    // Отпечаток содержимого экзамена: имя потока, двоеточие, числа через запятую,
    // потоки через вертикальную черту. Это запись программы опыта, а не файла.
    private static string ExamSha256(JsonElement examRoot)
    {
        var parts = examRoot.GetProperty("sequences").EnumerateArray()
            .Select(sequence => sequence.GetProperty("name").GetString()
                                + ":"
                                + string.Join(",", ReadIntArray(sequence.GetProperty("tokens"))));
        return Sha256Hex(Encoding.UTF8.GetBytes(string.Join("|", parts)));
    }

    // Шестнадцатеричный отпечаток в нижнем регистре.
    private static string Sha256Hex(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    // ── Записи показаний ─────────────────────────────────────────────────────

    // Что именно сделало одно чтение: позиция наблюдения, выданный ответ,
    // кластеры и фактически поданный в таблицу адрес.
    private sealed record ReaderObservation(
        int LastObservedIndex,
        int LastObservedToken,
        bool HasGlobal,
        int Global,
        int SourceCluster,
        int PredictedCluster,
        int ReaderInputLocal,
        bool WithinPredictCalled);

    // Один экзаменационный поток и его окно запросов.
    private sealed record ExamStream(string Name, int[] Tokens, int FirstQueryIndex, int QueryCount);
}
