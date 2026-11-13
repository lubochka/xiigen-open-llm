// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Inference;
using LoRA;
using TensorCore.Core;
using TensorCore.Init;
using TensorCore.Layers;
using Tokenizer;
using TrainingLoop;
using TrainingLoop.Optimizers;
using TrainingLoop.Schedulers;
using TransformerModel;
using TransformerModel.Config;

// Перенос ремонта первого урока дообучения (строка работ «первый шаг с разогревом»,
// номер 29-W1 в плане 88 той же папки).
//
// Что проверяется по-человечески. Перед самым первым уроком расписание скорости
// обучения с разогревом выдаёт ровно ноль. Продуктовый шаг обучения справедливо
// отказывается двигать веса при неположительной скорости, поэтому первый урок
// раньше выполнялся целиком и выбрасывался: прямой и обратный проход были, а
// изменения весов не было. Ремонт меняет ровно одно число — скорость НУЛЕВОГО
// шага дообучения, — и больше ничего в расписании не трогает.
//
// Как это доказывается здесь. Восстанавливается тот же обученный старт, на
// котором работал опыт, и два равнодозовых плеча получают одни и те же четыре
// урока в порядке «первый, второй, первый, второй»:
//   плечо A — продуктовый путь обучения с разогревом 4 и исправленным нулевым шагом;
//   плечо B — независимый эталон: на каждый урок берётся своя постоянная скорость,
//             равная измеренной опытом политике, и разогрев в ней выключен, поэтому
//             поправка нулевого шага в плече B вообще не участвует.
// Совпадение плеч число в число и есть доказательство, что продуктовый путь
// применил именно измеренную политику, а не какую-то другую траекторию.
public sealed class WarmupFirstSupervisedUpdateTests
{
    // ── Паспортные опознаватели общего обученного старта ─────────────────────────
    private const string StartWeightsSha256 =
        "8721dd57b11fa61b11acf37c337611129891a1d3096ae0392a89455d6d58fdec"; // Отпечаток весов старта.
    private const int DonorOptimizerStep = 400; // Счётчик шага донорского оптимизатора: происхождение, не доза.
    private const int ExpectedOptimizerStepAfter = 404; // Четыре урока поднимают счётчик до 404.
    private const int TrainableCoordinateCount = 896; // Сколько чисел адаптеров разрешено менять.
    private const ulong BaseSeed = 62001UL; // Зерно замороженной основы набора.
    private const ulong AdapterSeed = 62002UL; // Зерно адаптеров набора.
    private const string AdapterStream = "rms-row-reduce-probe-v1"; // Поток случайных чисел адаптеров.
    private const string TopologyLiteral =
        "RD01|V256|H64|L1|Q4|KV2|FF176|C64|R4|q_proj,v_proj"; // Текст топологии набора.
    private const string LoraLiteral =
        "RD01|alpha8|dropout0|seed62002|rms-row-reduce-probe-v1"; // Текст настройки адаптеров.

    // ── Расписание скорости, назначенное этому опыту ─────────────────────────────
    private const int ScheduleWarmupSteps = 4; // Разогрев: четыре шага.
    private const int ScheduleTotalSteps = 8; // Всего шагов расписания: восемь.
    private const float ScheduleMaxLr = 0.001f; // Верхняя скорость обучения.
    private const float ScheduleMinLr = 0.0001f; // Нижняя скорость обучения.

    // ── Настройки чтения панели, назначенные этому опыту ─────────────────────────
    private const int ReadoutSeed = 20261028; // Зерно выдачи; при нулевой температуре на выбор не влияет.
    private const int ReadoutMaxNewTokens = 1; // Ровно один знак ответа.
    private const float ReadoutTemperature = 0f; // Жадная выдача: случайность не участвует.
    private const int ReadoutTopK = 0; // Отсечение по числу кандидатов выключено.
    private const float ReadoutTopP = 1f; // Отсечение по суммарной вероятности выключено.

    // ── Учебная доза, назначенная планом ремонта ─────────────────────────────────
    private const int LessonsPerArm = 4; // Четыре урока в каждом плече.
    private const int PlannedGenerations = 12; // Двенадцать жадных выдач: 4 до и 8 после.
    private const int PlannedScoringForwards = 12; // Столько же оценивающих прямых проходов.
    private const int PlannedTrainForwards = 8; // Восемь учебных прямых проходов: по четыре на плечо.
    private const int PlannedTrainBackwards = 8; // Восемь учебных обратных проходов.
    private const int PlannedUpdates = 8; // Восемь обновлений оптимизатора.

    // Счётчики фактической работы: их значения сверяются с назначенной дозой.
    private int _generations; // Фактические жадные выдачи.
    private int _scoringForwards; // Фактические оценивающие прямые проходы.
    private int _trainSteps; // Фактические учебные шаги; у каждого один прямой и один обратный проход.

    // Главная учебная регрессия переноса: продуктовый путь с разогревом доводит
    // первый урок до весов и совпадает с независимым эталоном измеренной политики.
    [Fact]
    public void RestoredWarmupStart_FirstSupervisedLessonReachesWeights_AndEqualsMeasuredRatePolicy()
    {
        // ── 1. Общий обученный старт опознаётся по отпечатку, а не по имени файла ──
        var bundle = SharedLearnedStartDirectory(); // Папка общего обученного старта.
        var weightsPath = Path.Combine(bundle, "S0.weights.bin"); // Файл сохранённых весов старта.
        Assert.Equal(
            StartWeightsSha256,
            Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(weightsPath))).ToLowerInvariant());

        // ── 2. Численная настройка модели читается из описания порождения набора ──
        using var generationDocument = JsonDocument.Parse(
            File.ReadAllBytes(Path.Combine(bundle, "generation.json"))); // Описание порождения набора.
        var modelBlock = generationDocument.RootElement.GetProperty("model"); // Раздел настройки модели.
        var config = new ModelConfig
        {
            VocabSize = modelBlock.GetProperty("VocabSize").GetInt32(), // Размер словаря.
            ContextLen = modelBlock.GetProperty("ContextLen").GetInt32(), // Длина окна внимания.
            HiddenDim = modelBlock.GetProperty("HiddenDim").GetInt32(), // Ширина скрытого слоя.
            NumLayers = modelBlock.GetProperty("NumLayers").GetInt32(), // Число слоёв.
            NumHeads = modelBlock.GetProperty("NumHeads").GetInt32(), // Число голов запроса.
            NumKvHeads = modelBlock.GetProperty("NumKvHeads").GetInt32(), // Число голов ключей и значений.
            FfnHiddenDim = modelBlock.GetProperty("FfnHiddenDim").GetInt32(), // Ширина полносвязного слоя.
            RopeTheta = modelBlock.GetProperty("RopeTheta").GetSingle(), // Основание поворотных позиций.
            RmsNormEps = modelBlock.GetProperty("RmsNormEps").GetSingle() // Сглаживание нормировки.
        };

        // ── 3. Настройки оптимизатора берутся у самого набора, а не выдумываются ──
        var optimizerBlock = generationDocument.RootElement.GetProperty("optimizer"); // Раздел оптимизатора.
        var beta1 = optimizerBlock.GetProperty("beta1").GetSingle(); // Первый коэффициент моментов.
        var beta2 = optimizerBlock.GetProperty("beta2").GetSingle(); // Второй коэффициент моментов.
        var optimizerDecay = optimizerBlock.GetProperty("weightDecay").GetSingle(); // Затухание весов.
        var optimizerEps = optimizerBlock.GetProperty("eps").GetSingle(); // Сглаживание знаменателя.
        var trainingBlock = generationDocument.RootElement.GetProperty("training"); // Раздел обучения набора.

        // ── 4. Словарь: тот самый побайтовый переходник, которым читал опыт ───────
        var tokenizer = BpeTokenizer.Load(Path.Combine(
            FindRepositoryRoot(),
            "experiments-v2", "RP01-MICRO", "P1201", "src", "byte-identity-tokenizer.json"));
        Assert.Equal(config.VocabSize, tokenizer.VocabSize); // Ширина словаря обязана совпасть с моделью.
        var plusTokenId = tokenizer.Encode("+").Single(); // Номер знака «плюс».
        var minusTokenId = tokenizer.Encode("-").Single(); // Номер знака «минус».

        // ── 5. Уроки и экзамен — донорские байты набора, а не переписанный текст ──
        var donorRows = File.ReadAllLines(Path.Combine(bundle, "data.source.jsonl"))
            .Where(line => line.Trim().Length > 0)
            .Select(line =>
            {
                using var row = JsonDocument.Parse(line); // Каждая донорская строка разбирается отдельно.
                return (
                    Id: row.RootElement.GetProperty("id").GetString()!,
                    Prompt: row.RootElement.GetProperty("prompt").GetString()!,
                    Answer: row.RootElement.GetProperty("answer").GetString()!);
            })
            .ToArray(); // Донорские байты двух учебных строк.
        Assert.Equal(2, donorRows.Length); // Набор обязан содержать ровно две учебные строки.
        Assert.Equal("T1", donorRows[0].Id); // Первая учебная строка.
        Assert.Equal("C# result sign: 9-4 =", donorRows[0].Prompt); // Её запрос.
        Assert.Equal("+", donorRows[0].Answer); // Её верный ответ.
        Assert.Equal("T2", donorRows[1].Id); // Вторая учебная строка.
        Assert.Equal("C# result sign: 4-9 =", donorRows[1].Prompt); // Её запрос.
        Assert.Equal("-", donorRows[1].Answer); // Её верный ответ.

        // Панель чтения: две учебные строки и две экзаменационные, эталон счётом из запроса.
        var panel = new[]
        {
            (Id: "T1", Prompt: donorRows[0].Prompt, Gold: "+"), // Урок, который надо удержать.
            (Id: "T2", Prompt: donorRows[1].Prompt, Gold: "-"), // Второй урок панели.
            (Id: "E1", Prompt: "C# result sign: 8-3 =", Gold: "+"), // Первый экзамен.
            (Id: "E2", Prompt: "C# result sign: 3-8 =", Gold: "-") // Второй экзамен.
        };

        // Урок целиком: запрос и ответ подряд, без выдуманного перевода строки.
        var lessonTokens = new[]
        {
            tokenizer.Encode(donorRows[0].Prompt + donorRows[0].Answer).ToArray(), // Первый урок в номерах знаков.
            tokenizer.Encode(donorRows[1].Prompt + donorRows[1].Answer).ToArray() // Второй урок в номерах знаков.
        };
        var lessonOrder = new[] { 0, 1, 0, 1 }; // Порядок уроков: первый, второй, первый, второй.
        Assert.Equal(LessonsPerArm, lessonOrder.Length); // Доза обоих плеч: ровно четыре урока.

        // ── 6. Настройки обучения: от набора, но с расписанием именно этого опыта ─
        var trainingConfig = new TrainingConfig
        {
            BatchSize = trainingBlock.GetProperty("batchSize").GetInt32(), // Размер партии донора.
            SeqLen = trainingBlock.GetProperty("seqLen").GetInt32(), // Длина последовательности донора.
            MaxLr = ScheduleMaxLr, // Верхняя скорость: назначена этому опыту.
            MinLr = ScheduleMinLr, // Нижняя скорость: назначена этому опыту.
            WarmupSteps = ScheduleWarmupSteps, // Разогрев: назначен этому опыту.
            TotalSteps = ScheduleTotalSteps, // Длина расписания: назначена этому опыту.
            GradAccumSteps = trainingBlock.GetProperty("gradAccumSteps").GetInt32(), // Накопление градиента донора.
            GradClipNorm = trainingBlock.GetProperty("gradClipNorm").GetSingle(), // Обрезка нормы градиента донора.
            WeightDecay = trainingBlock.GetProperty("weightDecay").GetSingle(), // Затухание расписания донора.
            CheckpointDir = Path.GetTempPath(), // Шаг обучения с учителем эту папку не использует.
            CheckpointEvery = int.MaxValue, // Контрольные точки здесь не сохраняются.
            LogEvery = int.MaxValue // Журнал продуктового цикла здесь не используется.
        };

        // ── 7. Измеренная политика скорости: единственное изменение — нулевой шаг ─
        var productSchedule = new CosineWithWarmup(
            ScheduleWarmupSteps, ScheduleTotalSteps, ScheduleMaxLr, ScheduleMinLr); // Продуктовое расписание.
        Assert.Equal(0f, productSchedule.GetLr(0)); // До ремонта нулевой шаг давал ровно ноль.
        var measuredRates = new[]
        {
            productSchedule.GetLr(1), // Нулевой шаг берёт скорость первого шага: верх, делённый на разогрев.
            productSchedule.GetLr(1), // Первый шаг остаётся продуктовым.
            productSchedule.GetLr(2), // Второй шаг остаётся продуктовым.
            productSchedule.GetLr(3) // Третий шаг остаётся продуктовым.
        };
        // Числа измеренной политики закреплены планом: .00025, .00025, .0005, .00075.
        Assert.Equal(ScheduleMaxLr / 4f, measuredRates[0]);
        Assert.Equal(ScheduleMaxLr / 4f, measuredRates[1]);
        Assert.Equal(ScheduleMaxLr / 2f, measuredRates[2]);
        Assert.Equal(ScheduleMaxLr * 3f / 4f, measuredRates[3]);

        // ── 8. Два равнодозовых плеча из одного и того же восстановленного старта ─
        var randomIdentity = new LlmModelRandomStateIdentity(
            Sha256Text(TopologyLiteral), Sha256Text(LoraLiteral)); // Пара опознавателей снимка генератора.
        var randomSnapshot = JsonSerializer.Deserialize<LlmModelRandomStateSnapshot>(
            File.ReadAllBytes(Path.Combine(bundle, "S0.rng.json")))
            ?? throw new InvalidOperationException("WARMUP_FIRST_UPDATE_RANDOM_STATE_UNREADABLE");

        LlmModel BuildRestoredModel()
        {
            LlmModel built; // Модель объявлена снаружи блока зерна, чтобы жить дальше.
            using (Initializers.UseDeterministicSeed(BaseSeed)) built = new LlmModel(config); // Основа тем же зерном.
            LoraApplicator.ApplySeeded(
                built,
                new LoraConfig { Rank = 4, Alpha = 8f, Dropout = 0f, TargetModules = ["q_proj", "v_proj"] },
                AdapterSeed,
                AdapterStream); // Адаптеры запроса и значения тем же зерном и потоком.
            built.LoadCheckpoint(weightsPath); // Поверх начальных значений кладутся сохранённые веса старта.
            built.RestoreRandomState(randomSnapshot, randomIdentity); // Генератор восстановлен с проверкой опознавателей.
            return built; // Готовая копия общего старта.
        }

        var armA = BuildRestoredModel(); // Модель плеча A: продуктовый путь с разогревом.
        var armB = BuildRestoredModel(); // Модель плеча B: независимый эталон измеренной политики.

        var bindingsA = AdapterBindings(armA); // Именованные обучаемые массивы плеча A.
        var bindingsB = AdapterBindings(armB); // Именованные обучаемые массивы плеча B.
        Assert.Equal(
            bindingsA.Select(binding => binding.Key).ToArray(),
            bindingsB.Select(binding => binding.Key).ToArray()); // Получатели урока обязаны совпасть.
        Assert.Equal(TrainableCoordinateCount, bindingsA.Sum(binding => binding.Parameter.Value.Data.Length));

        var donorOptimizer = AdamWPortableState.FromBytes(
            File.ReadAllBytes(Path.Combine(bundle, "S0.optimizer.bin")))
            ?? throw new InvalidOperationException("WARMUP_FIRST_UPDATE_DONOR_OPTIMIZER_UNREADABLE");
        Assert.Equal(DonorOptimizerStep, donorOptimizer.Step); // Паспорт назвал донорский счётчик шага 400.

        var optimizerA = new AdamW(beta1, beta2, optimizerDecay, optimizerEps); // Оптимизатор плеча A.
        var optimizerB = new AdamW(beta1, beta2, optimizerDecay, optimizerEps); // Оптимизатор плеча B.
        Assert.True(optimizerA.TryImportPortableState(bindingsA, donorOptimizer)); // Донорские моменты в плечо A.
        Assert.True(optimizerB.TryImportPortableState(bindingsB, donorOptimizer)); // Те же моменты в плечо B.

        // Снимки «до»: числа копируются, чтобы сравнение не смотрело в ту же память.
        var weightsBeforeA = NamedCopy(bindingsA); // Обучаемые числа плеча A до уроков.
        var frozenBeforeA = FrozenHash(armA); // Отпечаток замороженной основы плеча A до уроков.
        var frozenBeforeB = FrozenHash(armB); // Отпечаток замороженной основы плеча B до уроков.
        Assert.Equal(frozenBeforeA, frozenBeforeB); // Плечи стартуют с одной и той же замороженной основы.
        Assert.Equal(
            NamedHash(weightsBeforeA),
            NamedHash(NamedCopy(bindingsB))); // И с одними и теми же обучаемыми числами.

        // ── 9. Общая панель ДО обучения: четыре строки, по одной жадной выдаче ────
        var panelBefore = panel
            .Select(row => ReadRow(armA, tokenizer, row.Id, row.Prompt, row.Gold, plusTokenId, minusTokenId))
            .ToArray(); // Панель до обучения читается один раз: состояния плеч совпадают.
        Assert.Equal(2, panelBefore.Count(row => row.Correct)); // До уроков верны ровно два ответа из четырёх.

        // ── 10. Плечо A: продуктовый путь обучения, четыре урока подряд ──────────
        var trainerA = new Trainer(trainingConfig); // У плеча A свой счётчик шага расписания.
        var lossesA = new float[lessonOrder.Length]; // Потери каждого урока плеча A.
        for (var index = 0; index < lessonOrder.Length; index++)
        {
            lossesA[index] = trainerA.TrainSupervisedNextTokenStep(
                armA, optimizerA, lessonTokens[lessonOrder[index]], 1f); // Настоящий продуктовый шаг.
            _trainSteps++; // Один учебный прямой и один обратный проход.
        }

        // ── 11. Плечо B: тот же урок, но скорость каждого шага задана явно ───────
        var lossesB = new float[lessonOrder.Length]; // Потери каждого урока плеча B.
        for (var index = 0; index < lessonOrder.Length; index++)
        {
            var rate = measuredRates[index]; // Скорость этого шага по измеренной политике.
            // Разогрев выключен, верх и низ равны: расписание отдаёт ровно эту скорость
            // на своём нулевом шаге, и поправка нулевого шага разогрева не участвует.
            var constantRateTrainer = new Trainer(trainingConfig with
            {
                WarmupSteps = 0,
                MaxLr = rate,
                MinLr = rate
            });
            lossesB[index] = constantRateTrainer.TrainSupervisedNextTokenStep(
                armB, optimizerB, lessonTokens[lessonOrder[index]], 1f); // Тот же урок на той же скорости.
            _trainSteps++; // Ещё один учебный прямой и обратный проход.
        }

        // Потери каждого урока совпали у обоих плеч: это та же учебная траектория.
        Assert.Equal(lossesA, lossesB);
        // Число разрешённых обновлений — ровно четыре на каждое плечо, не больше.
        Assert.Equal(LessonsPerArm * 2, _trainSteps);

        // ── 12. Первый урок дошёл до весов: состояние сдвинулось, а не осталось ──
        var weightsAfterA = NamedCopy(bindingsA); // Обучаемые числа плеча A после уроков.
        var weightsAfterB = NamedCopy(bindingsB); // Обучаемые числа плеча B после уроков.
        var changedCoordinates = weightsBeforeA
            .Sum(entry => entry.Value
                .Where((value, index) => value != weightsAfterA[entry.Key][index])
                .Count()); // Сколько разрешённых чисел действительно изменилось.
        Assert.Equal(TrainableCoordinateCount, changedCoordinates); // Сдвинулись все 896 разрешённых чисел.

        // ── 13. Продуктовый путь совпал с эталоном измеренной политики число в число ─
        Assert.Equal(NamedHash(weightsAfterA), NamedHash(weightsAfterB)); // Веса обоих плеч совпали.
        Assert.Equal(
            MomentsHash(optimizerA.ExportPortableState(bindingsA)),
            MomentsHash(optimizerB.ExportPortableState(bindingsB))); // И моменты оптимизатора совпали.
        Assert.Equal(ExpectedOptimizerStepAfter, optimizerA.ExportPortableState(bindingsA).Step); // 400 → 404.
        Assert.Equal(ExpectedOptimizerStepAfter, optimizerB.ExportPortableState(bindingsB).Step); // И в плече B.

        // ── 14. Уроки меняли только разрешённые числа: основа цела в обоих плечах ─
        Assert.Equal(frozenBeforeA, FrozenHash(armA)); // Замороженная основа плеча A не изменилась.
        Assert.Equal(frozenBeforeB, FrozenHash(armB)); // Замороженная основа плеча B не изменилась.

        // ── 15. Панели ПОСЛЕ: по четыре строки на каждое плечо ───────────────────
        var panelAfterA = panel
            .Select(row => ReadRow(armA, tokenizer, row.Id, row.Prompt, row.Gold, plusTokenId, minusTokenId))
            .ToArray(); // Панель плеча A после уроков.
        var panelAfterB = panel
            .Select(row => ReadRow(armB, tokenizer, row.Id, row.Prompt, row.Gold, plusTokenId, minusTokenId))
            .ToArray(); // Панель плеча B после уроков.

        for (var index = 0; index < panel.Length; index++)
        {
            Assert.Equal(panelAfterA[index].Answer, panelAfterB[index].Answer); // Ответы плеч совпали.
            Assert.Equal(panelAfterA[index].LogGold, panelAfterB[index].LogGold); // И числа уверенности тоже.
            Assert.Equal(panelAfterA[index].LogOther, panelAfterB[index].LogOther);
        }

        // Измеренный итог опыта — сохранение двух верных ответов из четырёх, а не
        // прирост точности. Это записывается как есть и не улучшается задним числом.
        Assert.Equal(2, panelAfterA.Count(row => row.Correct));
        Assert.Equal("+", panelAfterA[0].Answer); // Первый урок удержан: ответ «плюс».
        Assert.True(panelAfterA[0].Correct); // И он верный, как и до обучения.
        Assert.Equal(panelBefore.Select(row => row.Answer), panelAfterA.Select(row => row.Answer));

        // Измеренное ухудшение первой экзаменационной строки сохраняется в панели:
        // уверенность в верном знаке упала, и запас между верным и неверным сузился.
        Assert.True(panelAfterA[2].LogGold < panelBefore[2].LogGold);
        Assert.True(
            panelAfterA[2].LogGold - panelAfterA[2].LogOther
            < panelBefore[2].LogGold - panelBefore[2].LogOther);

        // ── 16. Назначенная доза выдержана ровно, без скрытых лишних вызовов ─────
        Assert.Equal(PlannedGenerations, _generations); // Двенадцать жадных выдач.
        Assert.Equal(PlannedScoringForwards, _scoringForwards); // Двенадцать оценивающих проходов.
        Assert.Equal(PlannedTrainForwards, _trainSteps); // Восемь учебных прямых проходов.
        Assert.Equal(PlannedTrainBackwards, _trainSteps); // Столько же обратных проходов.
        Assert.Equal(PlannedUpdates, _trainSteps); // И столько же обновлений оптимизатора.

        // ── 17. Полученное состояние сохраняется один раз по назначенному пути ───
        SaveTrainedStateOnce(
            armA,
            optimizerA,
            bindingsA,
            lessonOrder.Length,
            measuredRates,
            panelAfterA);
    }

    // Сохранение полученного состояния один раз по пути, назначенному планом
    // ремонта. Повторный прогон не переписывает байты: он пересчитывает их и
    // сверяет отпечатки, поэтому сохранение остаётся единственным.
    private static void SaveTrainedStateOnce(
        LlmModel model,
        AdamW optimizer,
        IReadOnlyList<AdamWParameterBinding> bindings,
        int supervisedStepsPerformed,
        IReadOnlyList<float> appliedRates,
        IReadOnlyList<PanelReadout> panelAfter)
    {
        var target = Path.Combine(
            FindRepositoryRoot(),
            "experiments-v2", "RP01-MICRO", "_shared-learned-starts", "p12-warmup4-r29", "v1");
        var scratch = Path.Combine(Path.GetTempPath(), $"p12-warmup4-r29-{Guid.NewGuid():N}");
        Directory.CreateDirectory(scratch); // Черновая папка: байты сначала считаются, потом сверяются.
        try
        {
            // Веса полученного состояния в продуктовом формате контрольной точки.
            var weightsPath = Path.Combine(scratch, "S1.weights.bin");
            model.SaveCheckpoint(weightsPath);
            // Моменты оптимизатора и его счётчик шага в переносимом виде.
            var optimizerState = optimizer.ExportPortableState(bindings);
            File.WriteAllBytes(Path.Combine(scratch, "S1.optimizer.bin"), optimizerState.ToBytes());
            // Снимок генератора случайных чисел с теми же опознавателями набора.
            var randomState = model.CaptureRandomState(
                Sha256Text(TopologyLiteral), Sha256Text(LoraLiteral), 0);
            WriteText(
                Path.Combine(scratch, "S1.rng.json"),
                JsonSerializer.Serialize(randomState, new JsonSerializerOptions { WriteIndented = true }));

            // Паспорт состояния: откуда взят старт, что именно сделано и чего это не доказывает.
            var manifest = new
            {
                origin_bundle = "experiments-v2/RP01-MICRO/_shared-learned-starts/h64-rms-row/v1",
                origin_weights_sha256 = StartWeightsSha256,
                load_compatible_tokenizer =
                    "experiments-v2/RP01-MICRO/P1201/src/byte-identity-tokenizer.json",
                lesson_order = new[] { "T1", "T2", "T1", "T2" },
                applied_learning_rates = appliedRates,
                schedule = new
                {
                    warmup_steps = ScheduleWarmupSteps,
                    total_steps = ScheduleTotalSteps,
                    max_lr = ScheduleMaxLr,
                    min_lr = ScheduleMinLr
                },
                optimizer_step_before = DonorOptimizerStep,
                optimizer_step_after = optimizerState.Step,
                trainable_coordinates = TrainableCoordinateCount,
                supervised_steps_performed = supervisedStepsPerformed,
                supervised_counter_note =
                    "число выполненных шагов дообучения записано как метаданные; "
                    + "продуктового способа восстановить приватный счётчик расписания нет",
                panel_after = panelAfter.Select(row => new
                {
                    id = row.Id,
                    answer = row.Answer,
                    correct = row.Correct
                }).ToArray(),
                evidence_note =
                    "состояние получено проверкой WarmupFirstSupervisedUpdateTests; "
                    + "два верных ответа из четырёх — сохранение, а не прирост точности",
                files = new[] { "S1.weights.bin", "S1.optimizer.bin", "S1.rng.json" }
                    .Select(name => new
                    {
                        name,
                        sha256 = Convert
                            .ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(scratch, name))))
                            .ToLowerInvariant()
                    }).ToArray()
            };
            WriteText(
                Path.Combine(scratch, "manifest.json"),
                JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true }));

            var names = new[] { "S1.weights.bin", "S1.optimizer.bin", "S1.rng.json", "manifest.json" };

            // Вердикт опубликованности обученного старта берётся у хранилища.
            //
            // Почему прежней проверки «каталога ещё нет» недостаточно. Она защищает только
            // пока каталог лежит на диске. Отслеживаемый хранилищем каталог можно удалить
            // локально, и тогда условие снова становится истинным, а прогон записывает в
            // ОПУБЛИКОВАННЫЙ путь другие байты. Опубликованный обученный старт не
            // перезаписывается и не воссоздаётся никогда: его отсутствие на диске — это
            // повод honestly упасть с понятным сообщением, а не повод записать своё.
            var published = ExperimentFixes.ExperimentEvidenceSink.IsPublishedEvidenceDirectory(
                FindRepositoryRoot(), target, out var publishedVerdict);

            // Первое и единственное сохранение: только когда каталога нет И он не опубликован.
            if (!Directory.Exists(target) && !published)
            {
                Directory.CreateDirectory(target); // Первое и единственное сохранение.
                foreach (var name in names)
                    File.Copy(Path.Combine(scratch, name), Path.Combine(target, name));
            }

            // Независимо от того, писали ли сейчас, сохранённые байты обязаны совпасть.
            foreach (var name in names)
            {
                var savedPath = Path.Combine(target, name);
                Assert.True(File.Exists(savedPath),
                    $"WARMUP_FIRST_UPDATE_SAVED_STATE_MISSING:{name}; каталог обученного старта {target};"
                    + $" вердикт хранилища: {publishedVerdict}");
                Assert.Equal(
                    Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(scratch, name))))
                        .ToLowerInvariant(),
                    Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(savedPath))).ToLowerInvariant());
            }
        }
        finally
        {
            if (Directory.Exists(scratch)) Directory.Delete(scratch, recursive: true); // Черновик не остаётся.
        }
    }

    // Запись текста по правилам хранилища: UTF-8 без метки, перевод строки LF,
    // без хвостовых пробелов.
    private static void WriteText(string path, string text)
    {
        var normalized = text.Replace("\r\n", "\n").Replace('\r', '\n'); // Переводы строк приводятся к LF.
        var lines = normalized.Split('\n').Select(line => line.TrimEnd(' ', '\t')); // Хвостовые пробелы срезаются.
        var payload = string.Join("\n", lines).TrimEnd('\n') + "\n"; // Ровно один перевод строки в конце.
        File.WriteAllText(path, payload, new UTF8Encoding(false)); // Метка порядка байтов не пишется.
    }

    // Одно чтение панели: жадная выдача продуктовым порождателем плюс отдельный
    // оценивающий прямой проход, из которого берутся числа уверенности.
    private PanelReadout ReadRow(
        LlmModel model,
        BpeTokenizer tokenizer,
        string id,
        string prompt,
        string gold,
        int plusTokenId,
        int minusTokenId)
    {
        var generator = new TextGenerator(model, tokenizer); // Продуктовый порождатель на текущем состоянии.
        var receipt = generator.GenerateDetailed(
            prompt, ReadoutMaxNewTokens, ReadoutTemperature, ReadoutTopK, ReadoutTopP,
            ReadoutSeed, null, CancellationToken.None); // Чтение ровно одного знака ответа.
        _generations++; // Одна жадная выдача.

        var promptIds = tokenizer.Encode(prompt).ToArray(); // Номера знаков запроса.
        var scored = model.Forward(
            new Tensor(promptIds.Select(value => (float)value).ToArray(), new TensorShape(1, promptIds.Length)),
            training: false); // Оценивающий прямой проход на том же запросе.
        _scoringForwards++; // Один оценивающий проход.

        var vocabulary = scored.Shape.Dimensions[2]; // Ширина словаря в выходных числах.
        var lastPosition = promptIds.Length - 1; // Последняя позиция запроса решает следующий знак.
        var lastLogits = new float[vocabulary]; // Числа последней позиции.
        Array.Copy(scored.Data, lastPosition * vocabulary, lastLogits, 0, vocabulary); // Копия этих чисел.
        var logSoftmax = LogSoftmax(lastLogits); // Логарифмические вероятности всех знаков.
        var goldIsPlus = gold == "+"; // Какой знак считается верным для этой строки.
        return new PanelReadout(
            id,
            receipt.Text,
            string.Equals(receipt.Text, gold, StringComparison.Ordinal),
            goldIsPlus ? logSoftmax[plusTokenId] : logSoftmax[minusTokenId],
            goldIsPlus ? logSoftmax[minusTokenId] : logSoftmax[plusTokenId]);
    }

    // Одна строка панели: ответ, верность и две логарифмические вероятности.
    private sealed record PanelReadout(
        string Id, string Answer, bool Correct, double LogGold, double LogOther);

    // Логарифмические вероятности по всем кандидатам, считанные в двойной точности.
    private static double[] LogSoftmax(IReadOnlyList<float> logits)
    {
        var maximum = double.NegativeInfinity; // Наибольшее число: вычитается ради устойчивости.
        foreach (var value in logits) maximum = Math.Max(maximum, value);
        var exponentialSum = 0d; // Сумма показательных функций сдвинутых чисел.
        foreach (var value in logits) exponentialSum += Math.Exp(value - maximum);
        var logSum = Math.Log(exponentialSum) + maximum; // Логарифм суммы без потери точности.
        var result = new double[logits.Count]; // Готовые логарифмические вероятности.
        for (var index = 0; index < logits.Count; index++) result[index] = logits[index] - logSum;
        return result;
    }

    // Именованные обучаемые массивы адаптеров запроса и значения в закреплённом порядке.
    private static IReadOnlyList<AdamWParameterBinding> AdapterBindings(LlmModel model)
    {
        var attached = Enumerable.Range(0, model.Blocks.Count)
            .SelectMany(block => new[] { "q_proj", "v_proj" }
                .Select(target => model.GetAttachedProjectionAdapter(block, target)!))
            .Cast<LoraLinear>()
            .ToArray(); // Все присоединённые адаптеры всех блоков.
        var allowed = attached
            .SelectMany(adapter => adapter.AdapterParameters)
            .Select(entry => entry.Parameter)
            .ToHashSet(ReferenceEqualityComparer.Instance); // Их параметры по ссылочной тождественности.
        var bindings = model.GetCheckpointParameterEntries()
            .Where(entry => allowed.Contains(entry.Parameter))
            .Select(entry => new AdamWParameterBinding(entry.Key, entry.Parameter))
            .OrderBy(binding => binding.Key, StringComparer.Ordinal)
            .ToArray(); // Именованные привязки в закреплённом порядке.
        Assert.NotEmpty(bindings); // Пустой набор получателей урока недопустим.
        return bindings;
    }

    // Полные копии чисел обучаемых массивов: сравнение «до и после» не смотрит в одну память.
    private static Dictionary<string, float[]> NamedCopy(IReadOnlyList<AdamWParameterBinding> bindings)
        => bindings.ToDictionary(
            binding => binding.Key,
            binding => (float[])binding.Parameter.Value.Data.Clone(),
            StringComparer.Ordinal);

    // Отпечаток именованных чисел: имена и все числа в закреплённом порядке.
    private static string NamedHash(Dictionary<string, float[]> named)
    {
        using var stream = new MemoryStream(); // Буфер для имён и чисел.
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true); // Двоичная запись.
        foreach (var (key, values) in named.OrderBy(entry => entry.Key, StringComparer.Ordinal))
        {
            writer.Write(key); // Имя массива.
            foreach (var value in values) writer.Write(value); // Все его числа.
        }

        writer.Flush(); // Буфер дописывается до конца перед отпечатком.
        return Convert.ToHexString(SHA256.HashData(stream.ToArray())).ToLowerInvariant();
    }

    // Отпечаток моментов оптимизатора вместе со счётчиком его шага.
    private static string MomentsHash(AdamWPortableState state)
    {
        using var stream = new MemoryStream(); // Буфер для моментов и счётчика.
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true); // Двоичная запись.
        writer.Write(state.Step); // Счётчик шага оптимизатора.
        foreach (var entry in state.Entries.OrderBy(entry => entry.Key, StringComparer.Ordinal))
        {
            writer.Write(entry.Key); // Имя массива.
            foreach (var value in entry.FirstMoment) writer.Write(value); // Первый момент.
            foreach (var value in entry.SecondMoment) writer.Write(value); // Второй момент.
        }

        writer.Flush(); // Буфер дописывается до конца перед отпечатком.
        return Convert.ToHexString(SHA256.HashData(stream.ToArray())).ToLowerInvariant();
    }

    // Отпечаток замороженной основы: всё, что урокам менять запрещено.
    private static string FrozenHash(LlmModel model)
    {
        using var stream = new MemoryStream(); // Буфер для чисел основы.
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true); // Двоичная запись.
        foreach (var parameter in model.Parameters().Where(parameter => !parameter.Value.RequiresGrad))
            foreach (var value in parameter.Value.Data) writer.Write(value); // Все числа замороженного массива.

        writer.Flush(); // Буфер дописывается до конца перед отпечатком.
        return Convert.ToHexString(SHA256.HashData(stream.ToArray())).ToLowerInvariant();
    }

    // Отпечаток строки: им подписан снимок генератора случайных чисел набора.
    private static string Sha256Text(string text)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();

    // Путь к общему сохранённому обученному старту, из которого начинался опыт.
    private static string SharedLearnedStartDirectory() => Path.Combine(
        FindRepositoryRoot(),
        "experiments-v2", "RP01-MICRO", "_shared-learned-starts", "h64-rms-row", "v1");

    // Подъём от папки сборки до корня хранилища: пути не зависят от места запуска.
    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory); // Папка собранных проверок.
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "LlmFromScratch.sln")))
                return current.FullName; // Корень найден по файлу решения.

            current = current.Parent; // Иначе поднимаемся ещё на уровень вверх.
        }

        throw new DirectoryNotFoundException("WARMUP_FIRST_UPDATE_REPOSITORY_ROOT_NOT_FOUND");
    }
}
