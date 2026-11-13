// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Text.Json;
using ContinuousLearning;
using ProjectWikiDomain;
using Xunit.Abstractions;

// Проверка переноса находки опыта о потере содержания учебной пары.
//
// По-человечески. Конвейер обучения сводки следов берёт готовую учебную пару
// (исходный след, правильная сводка, неправильная сводка) и превращает её в
// строку измерения, на которой учится модель предпочтения. Преобразователь
// читал у пары только номер и вид дефекта, поэтому все уроки с одинаковым
// видом дефекта становились побайтово одной и той же строкой: разные исходные
// ситуации сливались в одну задачу ещё до разбора на токены.
//
// Эти проверки идут через настоящий продуктовый путь: запускается настоящий
// конвейер, а затем читаются строки, которые настоящий модуль измерения сам
// сохранил на диск. Ни заглушек, ни повторения логики преобразователя здесь нет.
public sealed class WikiTraceAggregatorMetricPairDonorFieldsTests : IDisposable
{
    // Номер партии такой же, как у существующей проверки конвейера: так
    // сравнение с её поведением остаётся честным.
    private const string BatchId = "wiki-trace-aggregator-batch-001";

    // Настройки разбора на токены по умолчанию у продуктового модуля измерения
    // (размер словаря 2048, длина контекста 192). Различимость уроков глазами
    // модели проверяется именно на продуктовых значениях, а не на уменьшенных.
    private const int ProductionVocabSize = 2048;
    private const int ProductionContextLen = 192;

    private readonly ITestOutputHelper _output;

    // Отдельный временный каталог на каждый прогон: чужие результаты не тронуты.
    private readonly string _artifactRoot = Path.Combine(
        Path.GetTempPath(),
        $"wiki-trace-donor-fields-{Guid.NewGuid():N}");

    public WikiTraceAggregatorMetricPairDonorFieldsTests(ITestOutputHelper output)
    {
        _output = output;
    }

    public void Dispose()
    {
        // Временная поверхность удаляется сразу после проверки.
        if (Directory.Exists(_artifactRoot))
            Directory.Delete(_artifactRoot, recursive: true);
    }

    [Fact]
    public async Task WikiTraceAggregatorMetricRows_CarryTraceChosenAndRejectedOfEachPair()
    {
        // Быстрый настоящий прогон: одна эпоха нужна только чтобы продуктовый
        // модуль измерения сам записал строки на диск. Ни одно утверждение ниже
        // не зависит от точности обучения, поэтому прогон можно держать коротким.
        var runner = new ProjectWikiDpoMetricRunner(
            CreateProjectWikiLegalTeacherAdmissionAuthority(),
            epochCount: 1,
            learningRate: 5e-2f,
            vocabSize: 256,
            contextLen: 16,
            hiddenDim: 16,
            numLayers: 1,
            numHeads: 4,
            numKvHeads: 1,
            ffnHiddenDim: 32,
            deterministicFreshInitialization: true);

        var result = await new WikiTraceAggregatorTrainingPipeline(dpoMetricRunner: runner)
            .RunOneBatchAsync(_artifactRoot, BatchId, "https://models.local");

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.NotNull(result.Value!.DpoMetrics);
        var metrics = result.Value.DpoMetrics!;

        // Исходные пары берутся прямо из результата конвейера: это ровно те
        // объекты, которые преобразователь и получил на вход.
        var donorPairs = result.Value.TrainingBatch.Pairs
            .Concat(result.Value.HeldOutBatch.Pairs)
            .ToDictionary(pair => pair.PairId, StringComparer.Ordinal);
        var trainingPairIds = result.Value.TrainingBatch.Pairs
            .Select(pair => pair.PairId)
            .ToHashSet(StringComparer.Ordinal);

        // Строки измерения читаются из файла, который записал сам модуль измерения.
        var persistedRows = ReadPersistedRows(metrics.SourcePath);
        Assert.Equal(100, persistedRows.Count);

        // Вид дефекта, восстановленный продуктовым кодом, читается из его же
        // файла диагностик: так видно, что пометка неправильного ответа цела.
        var persistedRejectedTypes = ReadPersistedRejectedTypes(metrics.PairDiagnosticsPath);

        var violations = new List<string>();

        // Условие 1. Каждое из трёх содержательных полей пары обязано
        // фактически участвовать в соответствующем поле строки измерения.
        var rowsMissingTrace = 0;
        var rowsMissingChosen = 0;
        var rowsMissingRejected = 0;
        foreach (var row in persistedRows)
        {
            if (!donorPairs.TryGetValue(row.PairId, out var pair))
            {
                violations.Add($"строка измерения {row.PairId} не сопоставлена ни одной исходной паре");
                continue;
            }

            if (!row.Prompt.Contains(pair.A, StringComparison.Ordinal))
                rowsMissingTrace++;
            if (!row.Chosen.Contains(pair.Chosen, StringComparison.Ordinal))
                rowsMissingChosen++;
            if (!row.Rejected.Contains(pair.Rejected, StringComparison.Ordinal))
                rowsMissingRejected++;
        }

        _output.WriteLine($"строк измерения всего: {persistedRows.Count}");
        _output.WriteLine($"строк без исходного следа пары: {rowsMissingTrace}");
        _output.WriteLine($"строк без правильной сводки пары: {rowsMissingChosen}");
        _output.WriteLine($"строк без неправильной сводки пары: {rowsMissingRejected}");

        if (rowsMissingTrace > 0)
            violations.Add($"исходный след пары отсутствует в запросе у {rowsMissingTrace} из {persistedRows.Count} строк");
        if (rowsMissingChosen > 0)
            violations.Add($"правильная сводка пары отсутствует в принятом ответе у {rowsMissingChosen} из {persistedRows.Count} строк");
        if (rowsMissingRejected > 0)
            violations.Add($"неправильная сводка пары отсутствует в отклонённом ответе у {rowsMissingRejected} из {persistedRows.Count} строк");

        // Условие 2. Уроки с одинаковым видом дефекта, но разным исходным
        // следом обязаны давать РАЗНЫЕ строки измерения.
        var groups = persistedRows
            .Where(row => donorPairs.ContainsKey(row.PairId))
            .GroupBy(row => donorPairs[row.PairId].RejectedType, StringComparer.Ordinal)
            .ToArray();
        var collapsedGroups = 0;
        var comparedGroups = 0;
        foreach (var group in groups)
        {
            var rows = group.ToArray();
            // Предусловие доказательства: внутри группы исходные следы обязаны
            // различаться, иначе сравнение ничего не доказывало бы.
            var distinctTraces = rows
                .Select(row => donorPairs[row.PairId].A)
                .Distinct(StringComparer.Ordinal)
                .Count();
            if (rows.Length < 2 || distinctTraces < 2)
                continue;

            comparedGroups++;
            var distinctRows = rows
                .Select(row => $"{row.Prompt}\u001f{row.Chosen}\u001f{row.Rejected}")
                .Distinct(StringComparer.Ordinal)
                .Count();
            // Граница сравнения — число РАЗНЫХ исходных следов в группе, а не
            // число уроков: сам создатель пар повторяет один и тот же след,
            // потому что обучающая и проверочная части пересекаются по общим
            // единицам. Больше различных строк, чем разных следов, преобразователь
            // дать не может и не должен; проверяется именно отсутствие слипания —
            // на каждый разный след обязана быть своя строка измерения.
            if (distinctRows < distinctTraces)
            {
                collapsedGroups++;
                violations.Add(
                    $"вид дефекта {group.Key}: уроков {rows.Length} с {distinctTraces} разными исходными следами; " +
                    $"ожидалось различных строк измерения столько же, сколько разных следов ({distinctTraces}), " +
                    $"получено {distinctRows}");
            }
        }

        _output.WriteLine($"групп одного вида дефекта сравнено: {comparedGroups}; групп со слипшимися строками: {collapsedGroups}");

        // Условие 3. Условия допуска строк измерения продолжают выполняться:
        // пометка неправильного ответа на месте, стороны не совпадают, и
        // продуктовый код по-прежнему восстанавливает точный вид дефекта.
        var rowsWithoutWrongMarker = persistedRows
            .Count(row => !row.Rejected.Contains("[WRONG:", StringComparison.Ordinal));
        var rowsWithEqualSides = persistedRows
            .Count(row => string.Equals(row.Chosen.Trim(), row.Rejected.Trim(), StringComparison.Ordinal));
        var rowsWithWrongDefectType = persistedRows.Count(row =>
            donorPairs.ContainsKey(row.PairId) &&
            persistedRejectedTypes.TryGetValue(row.PairId, out var recovered) &&
            !string.Equals(recovered, donorPairs[row.PairId].RejectedType, StringComparison.Ordinal));

        _output.WriteLine($"строк без пометки неправильного ответа: {rowsWithoutWrongMarker}");
        _output.WriteLine($"строк, где обе стороны совпали: {rowsWithEqualSides}");
        _output.WriteLine($"строк с неверно восстановленным видом дефекта: {rowsWithWrongDefectType}");

        if (rowsWithoutWrongMarker > 0)
            violations.Add($"пометка неправильного ответа потеряна у {rowsWithoutWrongMarker} строк");
        if (rowsWithEqualSides > 0)
            violations.Add($"принятый и отклонённый ответы совпали у {rowsWithEqualSides} строк");
        if (rowsWithWrongDefectType > 0)
            violations.Add($"вид дефекта восстановлен неверно у {rowsWithWrongDefectType} строк");

        // Условие 4. Измеренное следствие из опыта: глазами модели уроки должны
        // перестать быть одной и той же задачей. Пока преобразователь читает
        // только вид дефекта, число различимых векторов РАВНО числу видов
        // дефекта. После починки различимых векторов обязано стать больше.
        var trainingRows = persistedRows
            .Where(row => trainingPairIds.Contains(row.PairId))
            .ToArray();
        var distinctDefectTypes = trainingRows
            .Select(row => donorPairs[row.PairId].RejectedType)
            .Distinct(StringComparer.Ordinal)
            .Count();
        var distinctModelVectors = trainingRows
            .Select(ModelVisibleVectorKey)
            .Distinct(StringComparer.Ordinal)
            .Count();

        _output.WriteLine(
            $"уроков обучения: {trainingRows.Length}; видов дефекта: {distinctDefectTypes}; " +
            $"различимых векторов глазами модели: {distinctModelVectors}");

        if (distinctModelVectors <= distinctDefectTypes)
        {
            violations.Add(
                $"глазами модели уроки всё ещё слиты: уроков {trainingRows.Length}, видов дефекта {distinctDefectTypes}, " +
                $"различимых векторов {distinctModelVectors}; ожидалось больше, чем видов дефекта");
        }

        Assert.True(
            violations.Count == 0,
            "Строка измерения обязана нести все три содержательных поля исходной пары и различать уроки:" +
            Environment.NewLine +
            string.Join(Environment.NewLine, violations.Select((text, index) => $"{index + 1}. {text}")));
    }

    // Ключ модельного взгляда на строку: настоящий продуктовый разбор на токены
    // через точку входа модуля измерения, которой пользуются и существующие
    // проверки допуска. Совпадение ключей значит, что для модели это один урок.
    private static string ModelVisibleVectorKey(PersistedMetricRow row)
    {
        var batch = ProjectWikiDpoMetricRunner.BuildTrainingBatchPreviewForTesting(
            new ProjectWikiDpoMetricPair(row.PairId, row.Prompt, row.Chosen, row.Rejected),
            ProductionVocabSize,
            ProductionContextLen);
        return string.Join(",", batch.ChosenIds.Data) +
               "|" + string.Join(",", batch.RejectedIds.Data) +
               "|" + string.Join(",", batch.ChosenMask.Data) +
               "|" + string.Join(",", batch.RejectedMask.Data);
    }

    // Чтение строк измерения из файла, записанного продуктовым модулем измерения.
    private static List<PersistedMetricRow> ReadPersistedRows(string sourcePath)
    {
        var rows = new List<PersistedMetricRow>();
        foreach (var line in File.ReadAllLines(sourcePath))
        {
            using var document = JsonDocument.Parse(line);
            var root = document.RootElement;
            rows.Add(new PersistedMetricRow(
                root.GetProperty("id").GetString()!,
                root.GetProperty("prompt").GetString()!,
                root.GetProperty("chosen").GetString()!,
                root.GetProperty("rejected").GetString()!));
        }

        return rows;
    }

    // Вид дефекта, который продуктовый код восстановил сам и записал в диагностики.
    private static Dictionary<string, string> ReadPersistedRejectedTypes(string diagnosticsPath)
    {
        var types = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var line in File.ReadAllLines(diagnosticsPath))
        {
            using var document = JsonDocument.Parse(line);
            var root = document.RootElement;
            types[root.GetProperty("pairId").GetString()!] =
                root.GetProperty("rejectedType").GetString()!;
        }

        return types;
    }

    // Допуск учителя для модуля измерения: та же сборка, что и в существующих
    // проверках этого модуля, иначе прогон отказывает ещё до обучения.
    private static ServerOwnedLegalTrainingTeacherAdmissionAuthority
        CreateProjectWikiLegalTeacherAdmissionAuthority()
    {
        var configuration = ContinuousLearningTp1DpoConfiguration.Create(
            "qwen3.5:9b",
            new string('d', 64),
            new string('c', 64),
            LegalTrainingTeacherAdmissionAuthority.MaxBatchRows,
            $"sha256:{new string('a', 64)}",
            $"sha256:{new string('b', 64)}",
            Enumerable.Range(1, 32)
                .Select(static value => checked((byte)value))
                .ToArray());
        if (!configuration.IsSuccess || configuration.Value is null)
        {
            throw new InvalidOperationException(
                $"ProjectWiki legal-teacher test configuration admission failed: {configuration.ErrorCode}");
        }

        return ServerOwnedLegalTrainingTeacherAdmissionAuthority
            .IssueFromAdmittedConfiguration(
                configuration.Value,
                static () => true);
    }

    // Одна сохранённая строка измерения в человеческих названиях полей.
    private sealed record PersistedMetricRow(
        string PairId,
        string Prompt,
        string Chosen,
        string Rejected);
}
