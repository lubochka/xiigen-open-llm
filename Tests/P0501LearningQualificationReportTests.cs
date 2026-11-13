// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Проверки разделённой квалификации обучения опыта P05-PRIMARY-01.
//
// Закрываемый дефект: прежняя квитанция выводила годность учебного сравнения
// ТОЛЬКО из того, что веса обоих плеч сдвинулись, и поэтому объявила сравнение
// годным при нуле верных ответов до и нуле верных ответов после.
//
// Все числа берутся из уже сохранённого показания попытки: ни одного обращения
// к модели, ни одного обновления весов. Проверки вызывают ДЕЙСТВУЮЩИЙ
// построитель, который компилируется и в хост опыта, и сюда; вручную
// составленная ожидаемая квитанция здесь не сравнивается ни с чем.

namespace LlmFromScratch.Tests;

using System.Text.Json;
using P0501Experiment;

public sealed class P0501LearningQualificationReportTests
{
    /// <summary>Имя строки удержания, чей прежний верный ответ — знак «плюс».</summary>
    private const string DonorPlusRow = "R-T1";

    /// <summary>Имя строки удержания, чей прежний верный ответ — знак «минус».</summary>
    private const string DonorMinusRow = "R-T2";

    /// <summary>
    /// Первая требуемая проверка: обновления были, отпечатки содержимого разные,
    /// но все ответы нулевые — учебное сравнение НЕ годно. Это прямой
    /// контрпример прежнему правилу «веса сдвинулись, значит сравнение годно».
    /// </summary>
    [Fact]
    public void UpdatesAndDifferentDigestsWithAllZeroAnswersDoNotQualifyTheComparison()
    {
        var recorded = LoadRecordedAttempt();

        var receipt = P0501LearningQualificationReport.Build(
            "P05-PRIMARY-01", recorded.ExecutionStatus, recorded.Arms);

        // Обновления действительно были: это отдельное поле, а не годность.
        Assert.True(receipt.RealTrainingPerformed);
        Assert.Equal(8, receipt.CompletedUpdates);

        // Отпечатки содержимого весов после обучения отличаются от старта и друг от друга.
        foreach (var arm in receipt.Arms) Assert.True(arm.StateChangeEvidence.WeightsDigestChanged);
        Assert.NotEqual(
            receipt.Arms[0].StateChangeEvidence.WeightsSha256After,
            receipt.Arms[1].StateChangeEvidence.WeightsSha256After);

        // Веса обоих плеч действительно сдвинулись: прежнее правило объявило бы сравнение годным.
        foreach (var arm in receipt.Arms) Assert.True(arm.StateChangeEvidence.NamedWeightShiftMaxAbsolute > 0d);

        // При этом ни одна группа потребителей не дала ни одного верного ответа.
        foreach (var arm in receipt.Arms)
            foreach (var group in arm.Groups)
            {
                Assert.Equal(0, group.CorrectBefore);
                Assert.Equal(0, group.CorrectAfter);
            }

        // Годность выводится только из квалификации групп, поэтому сравнение негодно.
        Assert.NotEqual(P0501LearningQualificationReport.ValidForNamedUnit, receipt.LearningValidity);
        Assert.Equal("INVALID_UNQUALIFIED_BEFORE / INVALID_ZERO_TO_ZERO", receipt.LearningValidity);

        // Группы квалифицированы раздельно: у удержания своя причина, у остальных своя.
        foreach (var arm in receipt.Arms)
        {
            var retention = arm.Groups.Single(group => group.Group == P0501LearningQualificationReport.RetentionKind);
            Assert.Equal(P0501LearningQualificationReport.InvalidUnqualifiedBefore, retention.Qualification);
            foreach (var other in arm.Groups.Where(group => group.Group != P0501LearningQualificationReport.RetentionKind))
                Assert.Equal(P0501LearningQualificationReport.InvalidZeroToZero, other.Qualification);
        }
    }

    /// <summary>
    /// Вторая требуемая проверка: донорский знак «плюс» при сыром ответе «+x…» —
    /// это ошибка полного восьмизначного читателя, но отдельный ПРЕЖНИЙ
    /// однознаковый читатель видит «+» во всех трёх состояниях.
    /// </summary>
    [Fact]
    public void DonorPlusIsSeenByRestoredSingleCharacterReaderThoughFullReaderFails()
    {
        var recorded = LoadRecordedAttempt();

        foreach (var state in recorded.DonorStates)
        {
            var plusRow = state.Rows.Single(row => row.RowId == DonorPlusRow);

            // Полный восьмизначный читатель признал ответ неверным.
            Assert.False(plusRow.FullReaderCorrect);
            // Сырой ответ действительно начинается со знака «плюс», а дальше идёт мусор.
            Assert.StartsWith("+", plusRow.ProducedRaw, StringComparison.Ordinal);
            Assert.True(plusRow.ProducedRaw.Length > 1);

            var restored = P0501LearningQualificationReport.RestoreSingleCharacterReading(
                plusRow.RowId, state.State, plusRow.Gold, plusRow.ProducedRaw);

            // Прежний однознаковый читатель восстановлен однозначно и видит «+».
            Assert.True(restored.Resolved);
            Assert.Equal("+", restored.FirstCharacter);
            Assert.Equal("U+002B", restored.FirstCharacterCodePoint);
            Assert.Equal("+", restored.GoldSingleCharacter);
            // По прежнему договору донора этот ответ верен, хотя полный читатель сказал «неверно».
            Assert.True(restored.SingleCharacterReaderCorrect);
            Assert.NotEqual(plusRow.FullReaderCorrect, restored.SingleCharacterReaderCorrect);
        }
    }

    /// <summary>
    /// Та же вторая проверка с другой стороны: вторая строка удержания тоже
    /// начинается со знака «плюс», а её прежний верный ответ — «минус». Прежний
    /// однознаковый читатель обязан назвать её неверной, то есть восстановление
    /// не выдаёт удержание там, где его не было.
    /// </summary>
    [Fact]
    public void DonorMinusRowStaysWrongUnderTheRestoredSingleCharacterReader()
    {
        var recorded = LoadRecordedAttempt();

        foreach (var state in recorded.DonorStates)
        {
            var minusRow = state.Rows.Single(row => row.RowId == DonorMinusRow);

            var restored = P0501LearningQualificationReport.RestoreSingleCharacterReading(
                minusRow.RowId, state.State, minusRow.Gold, minusRow.ProducedRaw);

            // Знак восстановлен однозначно, но он не тот, которого требует прежний эталон.
            Assert.True(restored.Resolved);
            Assert.Equal("+", restored.FirstCharacter);
            Assert.Equal("-", restored.GoldSingleCharacter);
            Assert.False(restored.SingleCharacterReaderCorrect);
        }
    }

    /// <summary>
    /// Третья требуемая проверка: знак замены кодировки не раскрывает исходный
    /// номер знака, поэтому восстановление обязано записать UNKNOWN и не
    /// угадывать номер байта. Берётся настоящая сохранённая строка, чей сырой
    /// ответ начинается именно со знака замены.
    /// </summary>
    [Fact]
    public void ReplacementCharacterDoesNotRevealTheOriginalTokenAndIsRecordedAsUnknown()
    {
        var recorded = LoadRecordedAttempt();

        // Настоящая сохранённая строка, чей первый знак — знак замены кодировки.
        var row = recorded.DonorStates[0].AllRows.First(candidate =>
            candidate.ProducedRaw.Length > 0 && candidate.ProducedRaw[0] == '�');

        var restored = P0501LearningQualificationReport.RestoreSingleCharacterReading(
            row.RowId, "S0-СТАРТ", row.Gold, row.ProducedRaw);

        // Первый знак однозначно не восстановлен: номер знака не угадывается.
        Assert.False(restored.Resolved);
        Assert.Null(restored.FirstCharacter);
        Assert.Equal("UNKNOWN", restored.FirstCharacterCodePoint);
        // Вердикта прежнего читателя нет: нераскрытый знак не становится ни верным, ни неверным.
        Assert.Null(restored.SingleCharacterReaderCorrect);
        Assert.Equal(P0501LearningQualificationReport.ReplacementNote, restored.Note);
    }

    /// <summary>
    /// Четвёртая требуемая проверка: терминальная квитанция не закрывает учебную
    /// цель. Исполнение дошло до конца, обучение было настоящим — цель всё равно
    /// остаётся открытой, пока заявленный потребитель не дал квалифицированных
    /// показаний.
    /// </summary>
    [Fact]
    public void TerminalReceiptWithRealTrainingDoesNotCloseTheLearningGoal()
    {
        var recorded = LoadRecordedAttempt();

        var receipt = P0501LearningQualificationReport.Build(
            "P05-PRIMARY-01", recorded.ExecutionStatus, recorded.Arms);

        // Исполнение терминально успешно и обучение настоящее.
        Assert.Equal("TERMINAL_OK", receipt.ExecutionStatus);
        Assert.True(receipt.RealTrainingPerformed);
        // И всё равно учебная цель остаётся открытой.
        Assert.Equal(P0501LearningQualificationReport.LearningGoalOpen, receipt.LearningGoal);
    }

    /// <summary>
    /// Прямой отрицательный контроль запрещённого переименования: сколь угодно
    /// большой сдвиг весов не превращает ноль-в-ноль в успех.
    /// </summary>
    [Fact]
    public void HugeWeightShiftStillCannotRenameZeroToZeroAsSuccess()
    {
        var recorded = LoadRecordedAttempt();

        // Те же сохранённые нулевые панели, но с заведомо огромным сдвигом весов.
        var exaggerated = recorded.Arms.ToDictionary(
            pair => pair.Key,
            pair => pair.Value with { NamedWeightShiftMaxAbsolute = 1_000d },
            StringComparer.Ordinal);

        var receipt = P0501LearningQualificationReport.Build("P05-PRIMARY-01", "TERMINAL_OK", exaggerated);

        // Годность не изменилась: она не зависит от сдвига весов вообще.
        Assert.Equal("INVALID_UNQUALIFIED_BEFORE / INVALID_ZERO_TO_ZERO", receipt.LearningValidity);
        Assert.Equal(P0501LearningQualificationReport.LearningGoalOpen, receipt.LearningGoal);
    }

    /// <summary>
    /// Положительный контроль: правило не всегда отвечает «негодно». Если на тех
    /// же сохранённых панелях хотя бы один ответ после обучения станет верным,
    /// сравнение квалифицируется как приобретение навыка от нулевого старта.
    /// </summary>
    [Fact]
    public void OneCorrectAnswerAfterZeroStartQualifiesTheComparison()
    {
        var recorded = LoadRecordedAttempt();

        // На тех же записях один учебный ответ объявляется верным после обучения.
        var improved = recorded.Arms.ToDictionary(
            pair => pair.Key,
            pair => pair.Value with
            {
                After = pair.Value.After
                    .Select(row => row.RowId == "T1" ? row with { FullReaderCorrect = true } : row)
                    .ToArray()
            },
            StringComparer.Ordinal);

        var receipt = P0501LearningQualificationReport.Build("P05-PRIMARY-01", "TERMINAL_OK", improved);

        // Сравнение стало годным, а учебная группа получила вердикт приобретения навыка.
        Assert.Equal(P0501LearningQualificationReport.ValidForNamedUnit, receipt.LearningValidity);
        foreach (var arm in receipt.Arms)
            Assert.Equal(
                P0501LearningQualificationReport.QualifiedAcquiredFromZero,
                arm.Groups.Single(group => group.RowIds.Contains("T1")).Qualification);
    }

    /// <summary>Одно состояние панели: имя состояния, строки удержания и все строки.</summary>
    private sealed record DonorState(string State, IReadOnlyList<P0501PanelReading> Rows, IReadOnlyList<P0501PanelReading> AllRows);

    /// <summary>Уже сохранённая попытка опыта целиком, в виде, который принимает построитель.</summary>
    private sealed record RecordedAttempt(
        string ExecutionStatus,
        IReadOnlyDictionary<string, P0501ArmReadings> Arms,
        IReadOnlyList<DonorState> DonorStates);

    /// <summary>
    /// Читает уже сохранённое показание попытки. Это чтение выполненного учебного
    /// опыта: ни одного обращения к модели и ни одного обновления весов здесь нет.
    /// </summary>
    private static RecordedAttempt LoadRecordedAttempt()
    {
        var resultRoot = Path.Combine(
            FindRepositoryRoot(),
            "experiments-v2", "RP01-MICRO", "P0501", "results", "attempt-0001");
        Assert.True(Directory.Exists(resultRoot), "сохранённое показание опыта не найдено: " + resultRoot);

        using var before = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(resultRoot, "before.json")));
        using var after = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(resultRoot, "after.json")));
        using var arms = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(resultRoot, "arms.json")));
        using var status = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(resultRoot, "status.json")));

        var startDigest = before.RootElement.GetProperty("weights_sha256").GetString()!;

        var readings = new Dictionary<string, P0501ArmReadings>(StringComparer.Ordinal)
        {
            ["A"] = ReadArm(before.RootElement, after.RootElement, arms.RootElement, "arm_a", startDigest),
            ["B"] = ReadArm(before.RootElement, after.RootElement, arms.RootElement, "arm_b", startDigest)
        };

        // Три состояния сохранённых панелей: старт и конец обучения каждого плеча.
        var states = new List<DonorState>
        {
            BuildState("S0-СТАРТ", ReadPanel(before.RootElement.GetProperty("arm_a"))),
            BuildState("A4", ReadPanel(after.RootElement.GetProperty("arm_a").GetProperty("rows"))),
            BuildState("B4", ReadPanel(after.RootElement.GetProperty("arm_b").GetProperty("rows")))
        };

        return new RecordedAttempt(
            status.RootElement.GetProperty("execution_status").GetString()!,
            readings,
            states);
    }

    /// <summary>Одно состояние: строки удержания отбираются по тому же виду, который пишет опыт.</summary>
    private static DonorState BuildState(string state, IReadOnlyList<P0501PanelReading> rows)
        => new(
            state,
            rows.Where(row => row.Kind == P0501LearningQualificationReport.RetentionKind).ToArray(),
            rows);

    /// <summary>Собрать показания одного плеча из сохранённых файлов попытки.</summary>
    private static P0501ArmReadings ReadArm(
        JsonElement before, JsonElement after, JsonElement arms, string armProperty, string startDigest)
    {
        var arm = arms.GetProperty(armProperty);
        return new P0501ArmReadings
        {
            Before = ReadPanel(before.GetProperty(armProperty)),
            After = ReadPanel(after.GetProperty(armProperty).GetProperty("rows")),
            CompletedUpdates = arm.GetProperty("step_records").GetArrayLength(),
            WeightsSha256Before = startDigest,
            WeightsSha256After = arm.GetProperty("weights_sha256_after").GetString()!,
            NamedWeightShiftMaxAbsolute = arm.GetProperty("max_absolute_weight_delta").GetDouble()
        };
    }

    /// <summary>Прочитать одно снятие панели из сохранённого массива строк.</summary>
    private static IReadOnlyList<P0501PanelReading> ReadPanel(JsonElement rows)
        => rows.EnumerateArray().Select(row => new P0501PanelReading
        {
            RowId = row.GetProperty("Id").GetString()!,
            Kind = row.GetProperty("Kind").GetString()!,
            Gold = row.GetProperty("Gold").EnumerateArray().Select(value => value.GetString()!).ToArray(),
            ProducedRaw = row.GetProperty("ProducedRaw").GetString() ?? string.Empty,
            FullReaderCorrect = row.GetProperty("Correct").GetBoolean()
        }).ToArray();

    /// <summary>Корень хранилища находится по файлу решения вверх от каталога сборки.</summary>
    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "LlmFromScratch.sln")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Корень хранилища не найден.");
    }
}
