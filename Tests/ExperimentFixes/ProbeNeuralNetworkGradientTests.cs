// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Постоянное покрытие ОБРАТНОГО ПРОХОДА пробной нейросети
// (35.ExperimentsV2/Core/ProbeNeuralNetwork.cs) — работа «исправить весь
// обратный проход пробной сети» (раздел C04 адресного плана исправлений,
// номер 105 в папке docs/plans/replanning-possible-diffs/
// fixes-based-on-experiments).
//
// Что здесь проверяется по-человечески. Пробная сеть учится предсказывать
// следующий токен: ей показывают пару «текущий токен → следующий токен» и
// одним шагом двигают все веса в сторону правильного ответа. Величина и
// направление сдвига выводятся аналитически — по формулам обратного прохода.
// Чтобы такой шаг имел смысл, эти формулы обязаны давать ровно ту же
// величину, что и прямой пересчёт «в лоб»: берём один вес, двигаем его на
// крошечную величину в плюс и в минус, дважды считаем потери и делим
// разность на фактическую ширину шага. Если два числа совпали — обучение
// действительно идёт вниз по потерям; если разошлись — сеть двигается не
// туда, куда думает, и любой вывод о «скорости обучения» на ней
// недостоверен.
//
// Что именно было сломано. Все производные обязаны считаться относительно
// ОДНОГО состояния весов — того, что было ДО шага. В прежнем коде порядок
// был нарушен в двух местах сразу: производная по скрытому состоянию
// читала веса второго слоя уже ПОСЛЕ их обновления, а производная по
// эмбеддингу читала веса первого слоя уже ПОСЛЕ их обновления. В обе
// производные подмешивался посторонний член от только что применённого
// обновления. Починка одного места оставила бы второе, поэтому проверка
// разделена по семействам параметров: эмбеддинги, первый слой, второй слой —
// и отдельно полный вектор одного шага целиком.
//
// Откуда взят эталон. Из аудиторской оснастки
// experiments-v2/_audits/2026-10-30-transfer-execution-audit/native-probes:
// та же закреплённая маленькая сеть (словарь 3, скрытый слой 2, скорость
// обучения 0,1), та же пара токенов (1 → 2), тот же шаг возмущения 0,001 и
// те же центральные разности. Аудит измерил на неисправленном коде
// наибольшую абсолютную ошибку градиента: эмбеддинги 0,02029646, первый слой
// 0,01132778, второй слой 1,57e-7. Второй слой уже был верен; эмбеддинги и
// первый слой обязаны после ремонта опуститься до того же порядка.
//
// Почему эталон потерь считается здесь заново. Потери для конечной разности
// считает отдельная функция в двойной точности: продуктовый шаг обучения
// нельзя использовать как измеритель потерь, потому что он одновременно
// меняет веса. Чтобы этот отдельный измеритель не разошёлся с продуктовым
// прямым проходом незаметно, он отдельно сверяется с величиной потерь,
// которую возвращает сам продуктовый шаг на том же состоянии «до шага».
//
// Бюджет этой проверки. Ровно ОДИН настоящий шаг обучения на закреплённый
// набор; всё остальное — вычисления потерь без обновления весов (двадцать
// один параметр по два вычисления). Прежние 2400 обновлений старого опыта
// здесь не повторяются. Отдельная проверка нечисловых значений использует
// свой маленький набор и не более восьми шагов.
//
// Чего это покрытие НЕ доказывает. Проверена одна закреплённая маленькая
// сеть на одной паре токенов при одном шаге возмущения. Это не закон о всех
// размерах сети, не проверка оптимизатора и скорости обучения (их этот
// ремонт намеренно не трогает), не проверка многошаговой сходимости и не
// утверждение, что сеть чему-то полезному научилась. Совпадение градиентов
// говорит только одно: шаг обучения идёт в ту сторону и на ту величину,
// которую задаёт функция потерь.

using System.Reflection;
using System.Text.Json;
using ExperimentsV2;

namespace LlmFromScratch.Tests.ExperimentFixes;

// Фикстура класса ниже даёт этому испытанию ранний захват попытки: оснастка создаёт её ДО
// конструктора тестового класса и до любой подготовки данных, а освобождает ОДИН раз после
// последнего теста класса.
//
// Честная граница этого испытания. Путь расписки он берёт у приёмника, а сами байты пишет своим
// сырым вызовом записи. Поэтому здесь закрываются ровно два разрыва владения — ранний захват и
// явное завершение попытки; перевод самой записи на приёмник остаётся отдельной работой и этим
// слайсом не выполняется.
public sealed class ProbeNeuralNetworkGradientTests
    : IClassFixture<ProbeNeuralNetworkGradientTests.AttemptScopeFixture>
{
    // Размер словаря закреплённой маленькой сети: три токена, как в аудите.
    private const int VocabularySize = 3;

    // Размер скрытого слоя закреплённой маленькой сети: два нейрона, как в аудите.
    private const int HiddenSize = 2;

    // Скорость обучения закреплённой сети: аналитический градиент получается
    // делением применённого обновления именно на это число.
    private const float LearningRate = 0.1f;

    // Входной токен обучающей пары.
    private const int CurrentToken = 1;

    // Целевой токен обучающей пары: именно его вероятность должна расти.
    private const int NextToken = 2;

    // Шаг возмущения для центральной конечной разности, как в аудите.
    private const float PerturbationStep = 0.001f;

    // Порог приёмки: выше этой абсолютной ошибки градиент считается неверным.
    // Взят по точности чисел с плавающей точкой (около 1e-7 относительной) и
    // остатку усечения центральной разности при шаге 0,001 (тоже около 1e-7).
    private const double CorrectGradientTolerance = 1e-5;

    // Порог «сломано»: аудит видел на неисправленном коде ошибки порядка 1e-2.
    // Нужен, чтобы красный прогон падал по существу, а не по случайному шуму.
    private const double BrokenGradientThreshold = 1e-3;

    // Допуск на сверку отдельного измерителя потерь с продуктовым прямым
    // проходом: продуктовый считает в одинарной точности, измеритель — в двойной.
    private const double LossAgreementTolerance = 1e-5;

    // Предел шагов в проверке нечисловых значений: больше не нужно, разрушение
    // при такой скорости обучения наступает на третьем шаге.
    private const int NonFiniteStepBudget = 8;

    // Человеческие имена семейств параметров: они же попадают в расписку.
    private const string EmbeddingFamily = "эмбеддинги";
    private const string FirstLayerFamily = "первый слой";
    private const string SecondLayerFamily = "второй слой";

    // ГЛАВНЫЙ СЛУЧАЙ ОДИН. Обновление эмбеддингов обязано совпасть с
    // производной потерь по эмбеддингам, посчитанной на состоянии ДО шага.
    // Именно здесь прежний код читал веса первого слоя уже после обновления.
    [Fact]
    public void EmbeddingUpdate_MatchesTheDerivativeTakenBeforeTheStep()
    {
        // Один шаг обучения и полный набор сравнений по всем параметрам.
        var measurement = MeasureOneStep();

        // Наибольшая абсолютная ошибка только по семейству эмбеддингов.
        double worst = WorstAbsoluteError(measurement, EmbeddingFamily);

        // Ошибка обязана опуститься до порядка точности чисел, а не остаться на 1e-2.
        Assert.True(
            worst <= CorrectGradientTolerance,
            $"Обновление эмбеддингов не совпало с производной до шага: наибольшая абсолютная ошибка {worst:G9} при допуске {CorrectGradientTolerance:G9}. {FamilyDetail(measurement, EmbeddingFamily)}");
    }

    // ГЛАВНЫЙ СЛУЧАЙ ДВА. Обновление первого слоя (веса и сдвиги) обязано
    // совпасть с производной потерь по нему на состоянии ДО шага.
    [Fact]
    public void FirstLayerUpdate_MatchesTheDerivativeTakenBeforeTheStep()
    {
        // Один шаг обучения и полный набор сравнений по всем параметрам.
        var measurement = MeasureOneStep();

        // Наибольшая абсолютная ошибка только по семейству первого слоя.
        double worst = WorstAbsoluteError(measurement, FirstLayerFamily);

        // До ремонта аудит видел здесь 0,01132778; после ремонта обязано стать около 1e-7.
        Assert.True(
            worst <= CorrectGradientTolerance,
            $"Обновление первого слоя не совпало с производной до шага: наибольшая абсолютная ошибка {worst:G9} при допуске {CorrectGradientTolerance:G9}. {FamilyDetail(measurement, FirstLayerFamily)}");
    }

    // КОНТРОЛЬ. Второй слой был верен уже до ремонта: его производная не
    // читает ничего обновлённого. Эта проверка обязана быть зелёной В ОБОИХ
    // прогонах — и до правки, и после. Если она покраснеет на неисправленном
    // коде, значит измеритель ловит не тот дефект, и выводам верить нельзя.
    [Fact]
    public void SecondLayerUpdate_MatchesTheDerivativeTakenBeforeTheStep()
    {
        // Один шаг обучения и полный набор сравнений по всем параметрам.
        var measurement = MeasureOneStep();

        // Наибольшая абсолютная ошибка только по семейству второго слоя.
        double worst = WorstAbsoluteError(measurement, SecondLayerFamily);

        // Аудит измерил здесь 1,57e-7 ещё до ремонта — порядок точности чисел.
        Assert.True(
            worst <= CorrectGradientTolerance,
            $"Обновление второго слоя не совпало с производной до шага: наибольшая абсолютная ошибка {worst:G9} при допуске {CorrectGradientTolerance:G9}. {FamilyDetail(measurement, SecondLayerFamily)}");
    }

    // ГЛАВНЫЙ СЛУЧАЙ ТРИ. Полный вектор одного шага целиком: все двадцать один
    // параметр сразу. Проверка по семействам не поймала бы перекос, при котором
    // каждое семейство в отдельности близко, а шаг в целом смотрит не туда.
    [Fact]
    public void WholeOneStepVector_MatchesTheGradientTakenBeforeTheStep()
    {
        // Один шаг обучения и полный набор сравнений по всем параметрам.
        var measurement = MeasureOneStep();

        // Наибольшая абсолютная ошибка по всему вектору обновления.
        double worst = measurement.Probes.Max(probe => probe.AbsoluteError);

        // Относительная длина расхождения ко всей длине градиента: общая мера перекоса.
        double relative = RelativeVectorError(measurement);

        // Обе меры обязаны быть на уровне точности чисел, а не на уровне 1e-2.
        Assert.True(
            worst <= CorrectGradientTolerance && relative <= CorrectGradientTolerance,
            $"Полный вектор одного шага не совпал с градиентом до шага: наибольшая абсолютная ошибка {worst:G9}, относительная ошибка вектора {relative:G9}, допуск {CorrectGradientTolerance:G9}.");
    }

    // ОПОРА ИЗМЕРИТЕЛЯ. Отдельная функция потерь в двойной точности, по которой
    // считаются конечные разности, обязана давать то же число, что и продуктовый
    // прямой проход на том же состоянии «до шага». Без этой сверки можно было бы
    // «доказать» совпадение градиентов с чужой функцией потерь.
    [Fact]
    public void LossUsedForFiniteDifferences_AgreesWithTheProductionForwardPass()
    {
        // Один шаг обучения: он же возвращает продуктовые потери до обновления.
        var measurement = MeasureOneStep();

        // Расхождение двух способов посчитать одни и те же потери.
        double gap = Math.Abs(measurement.OracleLossAtPreStep - measurement.NativeLoss);

        // Расхождение допустимо только на уровне округления одинарной точности.
        Assert.True(
            gap <= LossAgreementTolerance,
            $"Измеритель потерь разошёлся с продуктовым прямым проходом: продуктовые потери {measurement.NativeLoss:G9}, измеритель {measurement.OracleLossAtPreStep:G9}, расхождение {gap:G9}.");
    }

    // РАСПИСКА. Отдельная работа без приёмочного утверждения: сохранить на диск
    // шаг возмущения и обе ошибки — абсолютную и относительную — по каждому
    // параметру и по каждому семейству. Расписка пишется ДО любых приёмочных
    // проверок выше, поэтому она существует и в красном прогоне тоже.
    [Fact]
    public void GradientComparisonReceipt_IsWrittenForBothTheRedAndTheGreenRun()
    {
        // Один шаг обучения и полный набор сравнений по всем параметрам.
        var measurement = MeasureOneStep();

        // Путь расписки: имя задаёт запускатель, чтобы красный и зелёный прогоны не затирали друг друга.
        string receiptPath = ReceiptPath();

        // Папка улик этой работы создаётся, если её ещё нет.
        Directory.CreateDirectory(Path.GetDirectoryName(receiptPath)!);

        // Содержимое расписки: закреплённый набор, шаг возмущения, сводка по семействам и каждый параметр.
        var receipt = new
        {
            work = "C04. Исправить весь обратный проход пробной сети",
            owner = "35.ExperimentsV2/Core/ProbeNeuralNetwork.cs",
            fixture = new { vocabularySize = VocabularySize, hiddenSize = HiddenSize, learningRate = LearningRate, currentToken = CurrentToken, nextToken = NextToken },
            method = "центральная конечная разность на состоянии до шага против применённого обновления, делённого на скорость обучения",
            perturbationStep = PerturbationStep,
            trainStepsExecuted = 1,
            lossEvaluationsWithoutUpdate = measurement.Probes.Count * 2,
            nativeLossAtPreStep = measurement.NativeLoss,
            oracleLossAtPreStep = measurement.OracleLossAtPreStep,
            tolerance = CorrectGradientTolerance,
            families = new[] { EmbeddingFamily, FirstLayerFamily, SecondLayerFamily }.Select(family => new
            {
                family,
                worstAbsoluteError = WorstAbsoluteError(measurement, family),
                worstRelativeError = measurement.Probes.Where(probe => probe.Family == family).Max(probe => probe.RelativeError),
                verdict = WorstAbsoluteError(measurement, family) <= CorrectGradientTolerance ? "совпадает" : "не совпадает",
            }).ToArray(),
            wholeVector = new
            {
                worstAbsoluteError = measurement.Probes.Max(probe => probe.AbsoluteError),
                relativeVectorError = RelativeVectorError(measurement),
            },
            parameters = measurement.Probes.Select(probe => new
            {
                probe.Family,
                probe.Name,
                analyticUpdateOverLearningRate = probe.Analytic,
                finiteDifferenceDerivative = probe.Finite,
                absoluteError = probe.AbsoluteError,
                relativeError = probe.RelativeError,
            }).ToArray(),
        };

        // Расписка сохраняется с переводом строки LF, как требуют текстовые правила хранилища.
        string json = JsonSerializer.Serialize(receipt, new JsonSerializerOptions { WriteIndented = true }).Replace("\r\n", "\n");

        // Запись без метки порядка байтов: то же правило, что у журналов прогонов.
        File.WriteAllText(receiptPath, json + "\n", new System.Text.UTF8Encoding(false));

        // Единственное утверждение этой работы: расписка действительно оказалась на диске.
        Assert.True(File.Exists(receiptPath), $"Расписка сравнения градиентов не записана по пути {receiptPath}.");
    }

    // ОТДЕЛЬНАЯ РАБОТА, НЕ ПРО ПРАВИЛЬНОСТЬ ПРОИЗВОДНЫХ. При заведомо
    // разрушительной скорости обучения веса рано или поздно перестают быть
    // числами. Важно не то, что они испортились, а то, что видно КОГДА:
    // фиксируется номер первого нечислового шага, сохраняется последнее
    // конечное состояние и текущее. По одному последнему снимку вывод
    // «разрушилось раньше» делать нельзя — последнее конечное состояние прямо
    // показывает, что шагом ранее всё ещё было в порядке.
    [Fact]
    public void FirstNonFiniteStep_IsLocatedAndTheLastFiniteStateIsKept()
    {
        // Та же маленькая сеть, но с заведомо разрушительной скоростью обучения.
        var network = BuildFixtureNetwork(1e9f, out _, out _, out _, out _, out _);

        // Номер первого шага, после которого хотя бы один вес перестал быть числом.
        int firstNonFiniteStep = -1;

        // Последнее состояние, в котором все веса ещё были числами; до первого шага это начальное.
        float[] lastFiniteState = Snapshot(network);

        // Текущее состояние: на выходе из цикла оно либо нечисловое, либо последнее конечное.
        float[] currentState = lastFiniteState;

        // Шаги идут по кругу по парам токенов, пока бюджет не исчерпан или пока не сломалось.
        for (int step = 1; step <= NonFiniteStepBudget; step++)
        {
            // Один шаг обучения на очередной паре соседних токенов словаря.
            network.TrainStep(step % VocabularySize, (step + 1) % VocabularySize);

            // Снимок всех весов сразу после шага.
            currentState = Snapshot(network);

            // Первое же появление нечислового веса запоминает номер шага и прекращает цикл.
            if (currentState.Any(value => !float.IsFinite(value))) { firstNonFiniteStep = step; break; }

            // Пока всё числовое, этот снимок становится последним конечным состоянием.
            lastFiniteState = currentState;
        }

        // Разрушение обязано быть обнаружено и локализовано номером шага, иначе измерять нечего.
        Assert.True(firstNonFiniteStep >= 1, $"За {NonFiniteStepBudget} шагов нечисловое состояние не наступило: проверка не измеряет то, что заявляет.");

        // Последнее конечное состояние обязано быть целиком числовым: это и есть доказательство «шагом раньше было в порядке».
        Assert.All(lastFiniteState, value => Assert.True(float.IsFinite(value), "Последнее конечное состояние содержит нечисловой вес: граница разрушения определена неверно."));

        // Текущее состояние обязано содержать нечисловой вес: иначе номер первого плохого шага записан ошибочно.
        Assert.Contains(currentState, value => !float.IsFinite(value));
    }

    // ОТДЕЛЬНАЯ РАБОТА. Конечность чисел не заменяет полезный ответ: сеть с
    // полностью числовыми весами может отвечать одним и тем же токеном на всё.
    // Поэтому «все веса — числа» никогда не является приёмкой обучения.
    [Fact]
    public void AllWeightsFinite_IsNotEvidenceThatTheAnswerIsUseful()
    {
        // Сеть с закреплёнными весами; дальше второй слой делается вырожденным.
        var network = BuildFixtureNetwork(LearningRate, out _, out _, out _, out var secondLayerWeights, out var secondLayerBias);

        // Все веса второго слоя обнуляются: вклад скрытого слоя в ответ исчезает.
        for (int i = 0; i < VocabularySize; i++) for (int j = 0; j < HiddenSize; j++) secondLayerWeights[i, j] = 0f;

        // Сдвиг нулевого токена делается наибольшим: сеть теперь всегда отвечает токеном 0.
        secondLayerBias[0] = 1f;

        // Короткая контрольная последовательность: пары 0→1, 1→2, 2→0.
        var controlSequence = new[] { 0, 1, 2, 0 };

        // Доля верных ответов на этой последовательности: верен только последний переход.
        double accuracy = network.Evaluate(controlSequence);

        // Все веса при этом остаются числами — формальная проверка конечности проходит.
        Assert.All(Snapshot(network), value => Assert.True(float.IsFinite(value)));

        // И тем не менее ответ бесполезен: доля верных строго меньше единицы.
        Assert.True(accuracy < 1.0, $"Вырожденная сеть неожиданно ответила верно на всё: доля верных {accuracy:G9}.");
    }

    // Один шаг обучения на закреплённом наборе плюс все конечные разности к нему.
    private static OneStepMeasurement MeasureOneStep()
    {
        // Сеть с закреплёнными весами; наружу отдаются живые массивы, которые шаг обучения изменит на месте.
        var network = BuildFixtureNetwork(LearningRate, out var embedding, out var firstWeights, out var firstBias, out var secondWeights, out var secondBias);

        // Снимки «до шага»: именно на них считаются все конечные разности.
        var embeddingBefore = (float[,])embedding.Clone();
        var firstWeightsBefore = (float[,])firstWeights.Clone();
        var firstBiasBefore = (float[])firstBias.Clone();
        var secondWeightsBefore = (float[,])secondWeights.Clone();
        var secondBiasBefore = (float[])secondBias.Clone();

        // Потери на состоянии «до шага», посчитанные отдельным измерителем в двойной точности.
        double oracleLoss = Loss(embeddingBefore, firstWeightsBefore, firstBiasBefore, secondWeightsBefore, secondBiasBefore);

        // Центральная разность по одной ячейке матрицы: возмущение применяется к снимку «до шага».
        double DerivativeOfMatrix(float[,] matrix, int row, int column)
        {
            // Исходное значение ячейки запоминается, чтобы вернуть его обратно.
            float original = matrix[row, column];

            // Сдвиг вверх; фактическое значение читается обратно, потому что одинарная точность округляет.
            matrix[row, column] = original + PerturbationStep;
            float high = matrix[row, column];
            double lossHigh = Loss(embeddingBefore, firstWeightsBefore, firstBiasBefore, secondWeightsBefore, secondBiasBefore);

            // Сдвиг вниз на ту же величину.
            matrix[row, column] = original - PerturbationStep;
            float low = matrix[row, column];
            double lossLow = Loss(embeddingBefore, firstWeightsBefore, firstBiasBefore, secondWeightsBefore, secondBiasBefore);

            // Ячейка возвращается в исходное состояние, чтобы следующие разности шли от того же набора.
            matrix[row, column] = original;

            // Делим на фактическую ширину шага, а не на заявленную: так уходит ошибка округления шага.
            return (lossHigh - lossLow) / (high - low);
        }

        // Центральная разность по одной ячейке вектора сдвигов: та же схема, что и для матрицы.
        double DerivativeOfVector(float[] vector, int index)
        {
            // Исходное значение ячейки запоминается, чтобы вернуть его обратно.
            float original = vector[index];

            // Сдвиг вверх и фактически полученное значение.
            vector[index] = original + PerturbationStep;
            float high = vector[index];
            double lossHigh = Loss(embeddingBefore, firstWeightsBefore, firstBiasBefore, secondWeightsBefore, secondBiasBefore);

            // Сдвиг вниз на ту же величину.
            vector[index] = original - PerturbationStep;
            float low = vector[index];
            double lossLow = Loss(embeddingBefore, firstWeightsBefore, firstBiasBefore, secondWeightsBefore, secondBiasBefore);

            // Возврат ячейки в исходное состояние.
            vector[index] = original;

            // Деление на фактическую ширину шага.
            return (lossHigh - lossLow) / (high - low);
        }

        // Производные по всем двадцати одному параметру собираются ДО единственного шага обучения.
        var finiteEmbedding = new double[VocabularySize, HiddenSize];
        for (int i = 0; i < VocabularySize; i++) for (int j = 0; j < HiddenSize; j++) finiteEmbedding[i, j] = DerivativeOfMatrix(embeddingBefore, i, j);

        // Производные по весам первого слоя.
        var finiteFirstWeights = new double[HiddenSize, HiddenSize];
        for (int i = 0; i < HiddenSize; i++) for (int j = 0; j < HiddenSize; j++) finiteFirstWeights[i, j] = DerivativeOfMatrix(firstWeightsBefore, i, j);

        // Производные по сдвигам первого слоя.
        var finiteFirstBias = new double[HiddenSize];
        for (int i = 0; i < HiddenSize; i++) finiteFirstBias[i] = DerivativeOfVector(firstBiasBefore, i);

        // Производные по весам второго слоя.
        var finiteSecondWeights = new double[VocabularySize, HiddenSize];
        for (int i = 0; i < VocabularySize; i++) for (int j = 0; j < HiddenSize; j++) finiteSecondWeights[i, j] = DerivativeOfMatrix(secondWeightsBefore, i, j);

        // Производные по сдвигам второго слоя.
        var finiteSecondBias = new double[VocabularySize];
        for (int i = 0; i < VocabularySize; i++) finiteSecondBias[i] = DerivativeOfVector(secondBiasBefore, i);

        // РОВНО ОДИН настоящий шаг обучения: он меняет живые массивы на месте и возвращает потери до обновления.
        float nativeLoss = network.TrainStep(CurrentToken, NextToken);

        // Сравнения собираются в один плоский список: семейство, имя параметра, аналитика, конечная разность.
        var probes = new List<ParameterProbe>();

        // Аналитический градиент получается делением применённого обновления на скорость обучения.
        for (int i = 0; i < VocabularySize; i++)
            for (int j = 0; j < HiddenSize; j++)
                probes.Add(new ParameterProbe(EmbeddingFamily, $"_embed[{i},{j}]", (embeddingBefore[i, j] - embedding[i, j]) / LearningRate, finiteEmbedding[i, j]));

        // Веса первого слоя.
        for (int i = 0; i < HiddenSize; i++)
            for (int j = 0; j < HiddenSize; j++)
                probes.Add(new ParameterProbe(FirstLayerFamily, $"_w1[{i},{j}]", (firstWeightsBefore[i, j] - firstWeights[i, j]) / LearningRate, finiteFirstWeights[i, j]));

        // Сдвиги первого слоя.
        for (int i = 0; i < HiddenSize; i++)
            probes.Add(new ParameterProbe(FirstLayerFamily, $"_b1[{i}]", (firstBiasBefore[i] - firstBias[i]) / LearningRate, finiteFirstBias[i]));

        // Веса второго слоя.
        for (int i = 0; i < VocabularySize; i++)
            for (int j = 0; j < HiddenSize; j++)
                probes.Add(new ParameterProbe(SecondLayerFamily, $"_w2[{i},{j}]", (secondWeightsBefore[i, j] - secondWeights[i, j]) / LearningRate, finiteSecondWeights[i, j]));

        // Сдвиги второго слоя.
        for (int i = 0; i < VocabularySize; i++)
            probes.Add(new ParameterProbe(SecondLayerFamily, $"_b2[{i}]", (secondBiasBefore[i] - secondBias[i]) / LearningRate, finiteSecondBias[i]));

        // Готовое измерение одного шага целиком.
        return new OneStepMeasurement(nativeLoss, oracleLoss, probes);
    }

    // Потери на произвольном состоянии весов, посчитанные в двойной точности и
    // БЕЗ какого-либо обновления: эмбеддинг → первый слой → срез отрицательных →
    // второй слой → перекрёстная энтропия на целевом токене.
    private static double Loss(float[,] embedding, float[,] firstWeights, float[] firstBias, float[,] secondWeights, float[] secondBias)
    {
        // Скрытый слой: взвешенная сумма эмбеддинга входного токена со срезом отрицательных значений.
        var hidden = new double[HiddenSize];
        for (int i = 0; i < HiddenSize; i++)
        {
            // Сумма начинается со сдвига первого слоя.
            double sum = firstBias[i];

            // К ней прибавляются произведения весов на координаты эмбеддинга ВХОДНОГО токена.
            for (int j = 0; j < HiddenSize; j++) sum += firstWeights[i, j] * (double)embedding[CurrentToken, j];

            // Отрицательные значения срезаются в ноль — та же нелинейность, что в продуктовом коде.
            hidden[i] = Math.Max(0.0, sum);
        }

        // Выходной слой: по одному числу на каждый токен словаря.
        var logits = new double[VocabularySize];
        for (int i = 0; i < VocabularySize; i++)
        {
            // Сумма начинается со сдвига второго слоя.
            logits[i] = secondBias[i];

            // К ней прибавляются произведения весов второго слоя на скрытое состояние.
            for (int j = 0; j < HiddenSize; j++) logits[i] += secondWeights[i, j] * hidden[j];
        }

        // Наибольшее выходное число вычитается перед возведением в степень: иначе степень переполняется.
        double max = logits.Max();

        // Перекрёстная энтропия на целевом токене в устойчивой форме.
        return Math.Log(logits.Sum(value => Math.Exp(value - max))) + max - logits[NextToken];
    }

    // Сеть с закреплёнными весами аудита: наружу отдаются те же живые массивы,
    // которые лежат внутри сети, поэтому шаг обучения изменит именно их.
    private static ProbeNeuralNetwork BuildFixtureNetwork(
        float learningRate,
        out float[,] embedding,
        out float[,] firstWeights,
        out float[] firstBias,
        out float[,] secondWeights,
        out float[] secondBias)
    {
        // Сеть создаётся обычным продуктовым конструктором; случайные веса сразу заменяются закреплёнными.
        var network = new ProbeNeuralNetwork(VocabularySize, HiddenSize, learningRate, seed: 42);

        // Закреплённые значения из аудиторской оснастки: три токена по две координаты.
        embedding = new float[,] { { .1f, .2f }, { .2f, .4f }, { .3f, .1f } };

        // Веса первого слоя два на два.
        firstWeights = new float[,] { { .6f, .2f }, { .3f, .5f } };

        // Сдвиги первого слоя.
        firstBias = new float[] { .1f, .2f };

        // Веса второго слоя три на два.
        secondWeights = new float[,] { { .1f, .2f }, { .3f, .1f }, { .2f, .4f } };

        // Сдвиги второго слоя нулевые.
        secondBias = new float[VocabularySize];

        // Закреплённые массивы ставятся на место внутренних полей сети.
        SetPrivateField(network, "_embed", embedding);
        SetPrivateField(network, "_w1", firstWeights);
        SetPrivateField(network, "_b1", firstBias);
        SetPrivateField(network, "_w2", secondWeights);
        SetPrivateField(network, "_b2", secondBias);

        // Готовая сеть с предсказуемым начальным состоянием.
        return network;
    }

    // Подстановка значения во внутреннее поле сети: иначе закрепить веса нечем.
    private static void SetPrivateField(ProbeNeuralNetwork network, string fieldName, object value)
    {
        // Поле ищется среди внутренних полей экземпляра.
        var field = typeof(ProbeNeuralNetwork).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingFieldException($"PROBE_NETWORK_FIELD_NOT_FOUND:{fieldName}");

        // Значение подставляется напрямую.
        field.SetValue(network, value);
    }

    // Чтение значения внутреннего поля сети для снимка состояния.
    private static object GetPrivateField(ProbeNeuralNetwork network, string fieldName)
    {
        // Поле ищется среди внутренних полей экземпляра.
        var field = typeof(ProbeNeuralNetwork).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingFieldException($"PROBE_NETWORK_FIELD_NOT_FOUND:{fieldName}");

        // Значение возвращается как есть.
        return field.GetValue(network)!;
    }

    // Плоский снимок всех весов сети одним массивом: нужен для проверки конечности чисел.
    private static float[] Snapshot(ProbeNeuralNetwork network)
    {
        // Собираем значения всех пяти наборов параметров подряд.
        var values = new List<float>();

        // Матрицы раскладываются по строкам, векторы — как есть.
        foreach (var name in new[] { "_embed", "_w1", "_b1", "_w2", "_b2" })
        {
            // Значение поля может быть двумерной матрицей или одномерным вектором.
            object raw = GetPrivateField(network, name);

            // Любая из этих форм перечисляется одинаково как последовательность чисел.
            foreach (float value in (System.Collections.IEnumerable)raw) values.Add(value);
        }

        // Плоский снимок готов.
        return values.ToArray();
    }

    // Наибольшая абсолютная ошибка внутри одного семейства параметров.
    private static double WorstAbsoluteError(OneStepMeasurement measurement, string family) =>
        measurement.Probes.Where(probe => probe.Family == family).Max(probe => probe.AbsoluteError);

    // Относительная длина расхождения ко всей длине градиента: мера перекоса всего шага.
    private static double RelativeVectorError(OneStepMeasurement measurement)
    {
        // Квадрат длины вектора расхождений между аналитикой и конечными разностями.
        double errorSquared = measurement.Probes.Sum(probe => (probe.Analytic - probe.Finite) * (probe.Analytic - probe.Finite));

        // Квадрат длины самого градиента, посчитанного конечными разностями.
        double gradientSquared = measurement.Probes.Sum(probe => probe.Finite * probe.Finite);

        // Нулевой градиент означал бы нечего сравнивать; тогда возвращается сама длина расхождения.
        return gradientSquared > 1e-24 ? Math.Sqrt(errorSquared / gradientSquared) : Math.Sqrt(errorSquared);
    }

    // Человеческая расшифровка по семейству для текста упавшей проверки.
    private static string FamilyDetail(OneStepMeasurement measurement, string family) =>
        "Худшие пары (аналитика против конечной разности): " + string.Join("; ", measurement.Probes
            .Where(probe => probe.Family == family)
            .OrderByDescending(probe => probe.AbsoluteError)
            .Take(3)
            .Select(probe => $"{probe.Name} {probe.Analytic:G9} против {probe.Finite:G9}"))
        + $". Порог «сломано» {BrokenGradientThreshold:G9}.";

    // Назначенная папка улик этой работы, рядом с журналами прогонов.
    private static string DeclaredReceiptDirectory() => Path.Combine(
        RepositoryRoot(), "experiments-v2", "_runners", "C04-PROBENN-BACKWARD-attempt-0001");

    // Куда этому прогону разрешено писать расписку. Решение принимает общий приёмник
    // доказательств по одному факту: отслеживает ли хранилище файлы назначенной папки улик.
    //
    // Почему это нужно здесь. Имя расписки по умолчанию — `gradient-receipt.json`, и этот
    // файл в назначенной папке уже отслеживается хранилищем. Значит каждый обычный прогон
    // проверки перезаписывал опубликованную расписку. Пока папка не опубликована, прогон
    // пишет в неё и остаётся в хранилище. После публикации тот же прогон становится
    // проверочным и заметно уходит в игнорируемый каталог результатов испытаний.
    //
    // Почему ленивое поле заменено фикстурой класса. Ленивое поле рождало приёмник при ПЕРВОМ
    // обращении за путём расписки, то есть уже после измеренной работы. Фикстура класса создаётся
    // раньше конструктора теста, поэтому захват адреса стал первым действием попытки, а завершение
    // попытки ставится ровно один раз.
    public sealed class AttemptScopeFixture : ExperimentAttemptScope
    {
        /// <summary>Захватывает попытку этого испытания и ставит её в поле класса.</summary>
        public AttemptScopeFixture()
            : base(RepositoryRoot(), DeclaredReceiptDirectory(), nameof(ProbeNeuralNetworkGradientTests))
            // Поле класса заполняется только после успешного захвата: иначе области не существует.
            => AttemptScope = this;
    }

    /// <summary>Захваченная попытка этого класса: её ставит фикстура до первого теста.</summary>
    private static AttemptScopeFixture? AttemptScope;

    /// <summary>Приёмник доказательств захваченной попытки этого класса.</summary>
    private static ExperimentEvidenceSink Evidence =>
        (AttemptScope ?? throw new InvalidOperationException(
            "Область попытки не захвачена: фикстура класса " +
            nameof(ProbeNeuralNetworkGradientTests) + " не была создана.")).Evidence;

    // Путь расписки: имя файла задаёт запускатель через переменную окружения.
    private static string ReceiptPath()
    {
        // Имя по умолчанию используется, когда проверка запущена не через запускатель.
        string name = Environment.GetEnvironmentVariable("C04_PROBENN_RECEIPT_NAME") is { Length: > 0 } fromRunner
            ? fromRunner
            : "gradient-receipt.json";

        // Путь выдаёт приёмник: он же сообщает, запись это опыта или проверочный прогон.
        return Evidence.ResolvePath(name);
    }

    // Корень хранилища: ищется подъёмом от папки собранных проверок до файла решения.
    private static string RepositoryRoot()
    {
        // Начинаем от папки, куда собраны проверки.
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        // Поднимаемся вверх, пока не найдём файл решения.
        while (current is not null)
        {
            // Файл решения лежит ровно в корне хранилища.
            if (File.Exists(Path.Combine(current.FullName, "LlmFromScratch.sln"))) return current.FullName;

            // Иначе переходим на уровень выше.
            current = current.Parent;
        }

        // Без корня расписку класть некуда — это ошибка окружения, а не продукта.
        throw new DirectoryNotFoundException("PROBE_NETWORK_GRADIENT_REPOSITORY_ROOT_NOT_FOUND");
    }

    // Одно сравнение по одному параметру: аналитика против конечной разности.
    private sealed record ParameterProbe(string Family, string Name, double Analytic, double Finite)
    {
        // Абсолютная ошибка: насколько аналитический градиент отличается от прямого пересчёта.
        public double AbsoluteError => Math.Abs(Analytic - Finite);

        // Относительная ошибка: та же разница в долях от самой производной.
        public double RelativeError => Math.Abs(Finite) > 1e-12 ? AbsoluteError / Math.Abs(Finite) : AbsoluteError;
    }

    // Результат одного измеренного шага: потери двумя способами и все сравнения.
    private sealed record OneStepMeasurement(float NativeLoss, double OracleLossAtPreStep, IReadOnlyList<ParameterProbe> Probes);
}
