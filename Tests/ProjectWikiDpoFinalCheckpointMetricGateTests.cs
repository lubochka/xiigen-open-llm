// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using ContinuousLearning;
using DPO.Models;
using ProjectWikiDomain;

/// <summary>
/// Проверяемое поведение продукта: порог публикации обученной единицы судит
/// модель по тому, чему она научилась К КОНЦУ обучения, а не по её среднему
/// поведению по ходу обучения.
///
/// Почему это важно именно в двух направлениях. Траектория, которая выросла,
/// имеет среднее НИЖЕ конечного состояния — по среднему годная модель была бы
/// отвергнута. Траектория, которая деградировала, имеет среднее ВЫШЕ конечного
/// состояния — по среднему негодная модель была бы опубликована. Второй случай
/// опаснее: публиковалась ухудшившаяся модель.
///
/// Граница утверждения этих проверок. Здесь доказывается логика решения и смысл
/// полей: какое число попадает в публикуемые поля и какое решение принимает
/// общая дверь публикации. Сквозная цепочка внутри публикатора манифеста тестом
/// не проверяется и установлена чтением кода: проверка полномочия и источника
/// стоит раньше метрического порога и отклоняет любую партию, которую тест
/// способен построить, поэтому до порога живой публикатор из теста не доходит.
/// Ослаблять эту защиту ради достижимости теста нельзя.
/// </summary>
public sealed class ProjectWikiDpoFinalCheckpointMetricGateTests : IDisposable
{
    private readonly string _artifactRoot = Path.Combine(
        Path.GetTempPath(),
        $"project-wiki-final-metrics-{Guid.NewGuid():N}");

    public void Dispose()
    {
        if (Directory.Exists(_artifactRoot))
            Directory.Delete(_artifactRoot, recursive: true);
    }

    [Fact]
    public void DegradedTrajectory_PublishesNothing_BecauseFinalCheckpointIsBelowGate()
    {
        // Траектория шла вниз: начала хорошо и кончила плохо. Её среднее
        // остаётся выше порога 0,75, то есть прежняя логика пустила бы такую
        // модель в публикацию.
        var decliningTrajectory = Trajectory(
            accuracies: [1.00, 0.95, 0.90, 0.60],
            rewardMargins: [0.90, 0.60, 0.30, -0.20],
            losses: [0.10, 0.20, 0.35, 0.80]);
        // Конечная контрольная точка плохая: доля пар, где политика сместилась
        // в сторону предпочитаемого ответа сильнее эталона, упала до 0,40, а
        // конечный запас предпочтения стал отрицательным.
        var degradedFinalCheckpoint = Metrics(
            loss: 0.80f,
            accuracy: 0.40f,
            rewardMargin: -0.30f);

        var published = ProjectWikiDpoMetricRunner
            .BuildPublishedTrainingMetricsForTesting(
                decliningTrajectory,
                degradedFinalCheckpoint);

        // Два разных числа действительно разошлись, и разошлись в опасную
        // сторону: среднее проходит порог, конечное состояние — нет.
        Assert.True(
            published.TrajectoryAverageAccuracy >= 0.75,
            $"Среднее по траектории должно проходить порог, иначе проверка ничего не доказывает; получено {published.TrajectoryAverageAccuracy}.");
        Assert.Equal(0.40, published.FinalAccuracy, 6);
        Assert.Equal(-0.30, published.FinalRewardMargin, 6);

        var gate = ProjectWikiDpoPublicationMetricGate.Evaluate(
            MetricRunResult(published, finalHeldOutAccuracy: 0.90));

        // Публикация обязана быть отклонена: модель к концу обучения стала хуже.
        Assert.False(gate.IsSuccess);
        Assert.Equal(
            ProjectWikiDpoPublicationMetricGate.MetricsBelowGateErrorCode,
            gate.ErrorCode);
        Assert.False(ProjectWikiDpoPublicationMetricGate.Allows(
            MetricRunResult(published, finalHeldOutAccuracy: 0.90)));
    }

    [Fact]
    public void GrownTrajectory_IsAllowed_BecauseFinalCheckpointIsAboveGate()
    {
        // Траектория шла вверх: началась почти с нуля и кончилась хорошо. Её
        // среднее остаётся НИЖЕ порога, то есть прежняя логика отвергла бы эту
        // годную модель.
        var risingTrajectory = Trajectory(
            accuracies: [0.10, 0.30, 0.60, 0.80],
            rewardMargins: [-0.60, -0.30, 0.10, 0.40],
            losses: [0.90, 0.70, 0.40, 0.20]);
        // Конечная контрольная точка хорошая.
        var grownFinalCheckpoint = Metrics(
            loss: 0.15f,
            accuracy: 0.96f,
            rewardMargin: 0.42f);

        var published = ProjectWikiDpoMetricRunner
            .BuildPublishedTrainingMetricsForTesting(
                risingTrajectory,
                grownFinalCheckpoint);

        // Обратная сторона того же расхождения: по среднему эта модель
        // провалилась бы, по конечному состоянию она годна.
        Assert.True(
            published.TrajectoryAverageAccuracy < 0.75,
            $"Среднее по траектории должно НЕ проходить порог, иначе проверка ничего не доказывает; получено {published.TrajectoryAverageAccuracy}.");
        Assert.True(published.TrajectoryAverageRewardMargin < 0);
        Assert.Equal(0.96, published.FinalAccuracy, 6);

        var candidate = MetricRunResult(published, finalHeldOutAccuracy: 0.90);
        var gate = ProjectWikiDpoPublicationMetricGate.Evaluate(candidate);

        // Публикация обязана быть разрешена: к концу обучения модель хороша.
        Assert.True(gate.IsSuccess, gate.ErrorMessage);
        Assert.True(ProjectWikiDpoPublicationMetricGate.Allows(candidate));
    }

    // Второй довод — историческое поведение прежних двух выражений на этом же
    // входе. Он разный по случаям, и это измерено прогоном, а не предположено:
    // «не-число» прежний конвейер отклонял правильно, а бесконечность —
    // пропускал, потому что бесконечность больше любого порога.
    [Theory]
    [InlineData("конечная точность на обучающем наборе не измерена", false)]
    [InlineData("конечная точность на обучающем наборе бесконечна", true)]
    [InlineData("конечный запас предпочтения не измерен", false)]
    [InlineData("конечная точность на отложенной выборке не измерена", false)]
    public void UnmeasuredFinalMetric_IsRefusedWithAClearReason(
        string caseName,
        bool historicalPipelineFormAlsoAllowedPublication)
    {
        // Здоровая основа, в которой испорчено ровно одно число. Так видно, что
        // отказ вызывает именно незамеренное значение, а не что-то ещё.
        var healthyTrajectory = Trajectory(
            accuracies: [0.80, 0.85, 0.90, 0.95],
            rewardMargins: [0.20, 0.30, 0.40, 0.50],
            losses: [0.40, 0.30, 0.20, 0.10]);
        var published = ProjectWikiDpoMetricRunner
            .BuildPublishedTrainingMetricsForTesting(
                healthyTrajectory,
                Metrics(loss: 0.10f, accuracy: 0.95f, rewardMargin: 0.50f));
        var finalHeldOutAccuracy = 0.90;
        switch (caseName)
        {
            case "конечная точность на обучающем наборе не измерена":
                published = published with { FinalAccuracy = double.NaN };
                break;
            case "конечная точность на обучающем наборе бесконечна":
                published = published with
                {
                    FinalAccuracy = double.PositiveInfinity
                };
                break;
            case "конечный запас предпочтения не измерен":
                published = published with { FinalRewardMargin = double.NaN };
                break;
            default:
                finalHeldOutAccuracy = double.NaN;
                break;
        }

        var candidate = MetricRunResult(published, finalHeldOutAccuracy);
        var gate = ProjectWikiDpoPublicationMetricGate.Evaluate(candidate);

        // Отказ обязателен, и причина обязана быть отличима от «не дотянул до
        // порога»: человек должен видеть, что числа просто нет.
        Assert.False(gate.IsSuccess);
        Assert.Equal(
            ProjectWikiDpoPublicationMetricGate.MetricsNotMeasuredErrorCode,
            gate.ErrorCode);
        Assert.False(ProjectWikiDpoPublicationMetricGate.Allows(candidate));

        // Сверка с историческим поведением двух прежних мест, по случаям.
        //
        // Публикатор манифеста отказывал по условию «меньше порога». Сравнение
        // и с «не-числом», и с бесконечностью даёт здесь ложь, поэтому отказа
        // не происходило НИ В ОДНОМ из четырёх случаев: дальше шли запись
        // весов, запись манифеста, выгрузка и регистрация. Это главная дыра.
        //
        // Конвейер обучения разрешал по обратному условию «не меньше порога», и
        // ведёт себя по-разному: на «не-числе» сравнение ложно и он отказывал
        // правильно, а на бесконечности оно ИСТИННО, потому что бесконечность
        // больше любого порога, — и он тоже пропускал. То есть в случае
        // бесконечности неправы были оба прежних места, а не одно.
        //
        // Новое общее правило отказывает во всех четырёх случаях: верное
        // поведение конвейера сохранено, а оба пропуска закрыты.
        var historicalPipelineFormAllows =
            candidate.FinalAccuracy >= 0.75 &&
            candidate.FinalRewardMargin > 0 &&
            candidate.HeldOutAccuracy >= 0.75;
        var historicalPublisherFormRefuses =
            candidate.FinalAccuracy < 0.75 ||
            candidate.HeldOutAccuracy < 0.75 ||
            candidate.FinalRewardMargin <= 0;
        Assert.Equal(
            historicalPipelineFormAlsoAllowedPublication,
            historicalPipelineFormAllows);
        Assert.False(historicalPublisherFormRefuses);
    }

    [Fact]
    public async Task LiveRun_MeasuresTrainingSetOnceOnTheFinalCheckpoint_AndKeepsTrajectoryAverageSeparate()
    {
        var observations = new List<ProjectWikiDpoMetricBatchObservation>();
        var runner = new ProjectWikiDpoMetricRunner(
            CreateProjectWikiLegalTeacherAdmissionAuthority(),
            epochCount: 1,
            learningRate: 5e-2f,
            vocabSize: 128,
            contextLen: 16,
            hiddenDim: 16,
            numLayers: 1,
            numHeads: 4,
            numKvHeads: 1,
            ffnHiddenDim: 32,
            deterministicFreshInitialization: true,
            batchObserverForTesting: observations.Add);

        var result = await runner.TrainAndEvaluateAsync(
            "project-wiki-final-metrics-test-unit",
            "project-wiki-final-metrics-test-batch",
            BuildRows("train"),
            BuildRows("heldout"),
            _artifactRoot);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var run = result.Value!;

        // Замер обучающего набора на конечной контрольной точке действительно
        // произошёл: пятьдесят строк, по одной на каждую обучающую пару.
        var finalCheckpointEvaluation = observations
            .Where(observation => string.Equals(
                observation.Stage,
                ProjectWikiDpoMetricRunner
                    .FinalTrainingCheckpointEvaluationObservationStage,
                StringComparison.Ordinal))
            .ToArray();
        Assert.Equal(50, finalCheckpointEvaluation.Length);
        // И он идёт по оценочным партиям без допуска на мутацию весов: допуск
        // остаётся только у шагов обучения, этот инвариант не ослаблен.
        Assert.All(finalCheckpointEvaluation, observation =>
            Assert.Null(observation.Batch.Admission));

        // Существующие этапы не сдвинулись: столько же наблюдений, сколько было
        // до правки.
        Assert.Equal(
            50,
            CountStage(
                observations,
                ProjectWikiDpoMetricRunner.TrainingTokenizationObservationStage));
        Assert.Equal(
            50,
            CountStage(
                observations,
                ProjectWikiDpoMetricRunner.HeldOutTokenizationObservationStage));
        Assert.Equal(
            50,
            CountStage(
                observations,
                ProjectWikiDpoMetricRunner.StepManyTrainingObservationStage));
        Assert.Equal(
            100,
            CountStage(
                observations,
                ProjectWikiDpoMetricRunner
                    .PairDiagnosticEvaluationObservationStage));

        // Средние по траектории лежат в своих отдельных полях и равны ровно
        // средневзвешенному по шагам обучения.
        var trainedPairCount = Math.Max(
            1,
            run.BatchSummaries.Sum(item => item.PairCount));
        Assert.Equal(
            run.BatchSummaries.Sum(item => item.Accuracy * item.PairCount)
                / trainedPairCount,
            run.TrajectoryAverageAccuracy,
            12);
        Assert.Equal(
            run.BatchSummaries.Sum(item => item.RewardMargin * item.PairCount)
                / trainedPairCount,
            run.TrajectoryAverageRewardMargin,
            12);
        Assert.Equal(
            run.BatchSummaries.Sum(item => item.Loss * item.PairCount)
                / trainedPairCount,
            run.TrajectoryAverageLoss,
            12);

        // Публикуемые конечные числа измерены и конечны: именно это требует
        // условие приёмки перед порогом.
        Assert.False(double.IsNaN(run.FinalAccuracy));
        Assert.False(double.IsInfinity(run.FinalAccuracy));
        Assert.False(double.IsNaN(run.FinalRewardMargin));
        Assert.False(double.IsInfinity(run.FinalRewardMargin));
        Assert.False(double.IsNaN(run.FinalLoss));
        Assert.False(double.IsInfinity(run.FinalLoss));
    }

    private static int CountStage(
        IEnumerable<ProjectWikiDpoMetricBatchObservation> observations,
        string stage)
        => observations.Count(observation => string.Equals(
            observation.Stage,
            stage,
            StringComparison.Ordinal));

    private static ProjectWikiDpoMetricBatchSummary[] Trajectory(
        double[] accuracies,
        double[] rewardMargins,
        double[] losses)
        => Enumerable
            .Range(0, accuracies.Length)
            .Select(index => new ProjectWikiDpoMetricBatchSummary(
                index + 1,
                $"trajectory-step-{index + 1}-first",
                $"trajectory-step-{index + 1}-last",
                5,
                losses[index],
                accuracies[index],
                rewardMargins[index],
                1.0,
                1.0,
                0.0))
            .ToArray();

    private static DpoMetrics Metrics(
        float loss,
        float accuracy,
        float rewardMargin)
        => new()
        {
            Step = 999,
            Loss = loss,
            UnscaledLoss = loss,
            Accuracy = accuracy,
            RewardMargin = rewardMargin
        };

    private static ProjectWikiDpoMetricRunResult MetricRunResult(
        ProjectWikiDpoMetricPublishedTrainingMetrics published,
        double finalHeldOutAccuracy)
        => new(
            "project-wiki-final-metrics-gate-run",
            "project-wiki-final-metrics-gate-unit",
            "project-wiki-final-metrics-gate-batch",
            "",
            "",
            "",
            1,
            0.01,
            "deterministic_fresh_policy",
            "copied_from_deterministic_fresh_policy",
            0,
            "",
            "",
            50,
            50,
            published.FinalLoss,
            published.FinalAccuracy,
            published.FinalRewardMargin,
            0.10,
            finalHeldOutAccuracy,
            0.40,
            0,
            0,
            1,
            0,
            "",
            [])
        {
            TrajectoryAverageLoss = published.TrajectoryAverageLoss,
            TrajectoryAverageAccuracy = published.TrajectoryAverageAccuracy,
            TrajectoryAverageRewardMargin =
                published.TrajectoryAverageRewardMargin
        };

    private static ProjectWikiDpoMetricPair[] BuildRows(string split)
        => Enumerable
            .Range(1, 50)
            .Select(index => new ProjectWikiDpoMetricPair(
                $"project-wiki-final-metrics-{split}-pair-{index:000}",
                $"wiki {split} prompt {index}",
                $"GOOD_PROJECT_WIKI_SIGNAL CHOSEN_IS_CORRECT split {split} row {index}",
                $"BAD_PROJECT_WIKI_SIGNAL [WRONG: {split}_defect - row {index}] REJECTED_IS_WRONG"))
            .ToArray();

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
        Assert.True(configuration.IsSuccess, configuration.ErrorMessage);

        return ServerOwnedLegalTrainingTeacherAdmissionAuthority
            .IssueFromAdmittedConfiguration(
                configuration.Value!,
                static () => true);
    }
}
