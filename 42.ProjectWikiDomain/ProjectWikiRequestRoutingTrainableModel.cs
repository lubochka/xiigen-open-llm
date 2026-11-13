// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Text;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed class ProjectWikiRequestRoutingTrainableModel
{
    public const double MinimumHeldOutRouteAccuracy = 0.95;
    public const double MinimumWrongAnnotationRate = 1.0;
    public const double MinimumAverageConfidence = 0.85;
    public const double MinimumTopTwoMargin = 0.02;
    public const double MinimumSelectionConfidence = 0.85;

    private const int EpochCount = 18;
    private const double LearningRate = 0.14;
    private const double LowConfidenceCeiling = 0.49;
    private const double NumericZero = 0.000000001;

    private static readonly ProjectWikiRequestRouteKind[] Labels =
    [
        ProjectWikiRequestRouteKind.ProjectWikiRequestRouterTraining,
        ProjectWikiRequestRouteKind.NegativeDpoTraining,
        ProjectWikiRequestRouteKind.ProjectValidation,
        ProjectWikiRequestRouteKind.AnswerComposerTraining,
        ProjectWikiRequestRouteKind.DocumentationGeneratorTraining,
        ProjectWikiRequestRouteKind.CodeTraceTraining,
        ProjectWikiRequestRouteKind.EntityImpactTraining,
        ProjectWikiRequestRouteKind.DocumentationBuild,
        ProjectWikiRequestRouteKind.StackVersionTraining,
        ProjectWikiRequestRouteKind.AnswerVerifierTraining,
        ProjectWikiRequestRouteKind.TraceAggregatorTraining,
        ProjectWikiRequestRouteKind.QueryPlanningTraining,
        ProjectWikiRequestRouteKind.ContextRoutingTraining,
        ProjectWikiRequestRouteKind.WikiQuestionAnswer,
        ProjectWikiRequestRouteKind.NotProjectWiki
    ];

    private readonly Dictionary<string, double[]> _tokenWeights;
    private readonly double[] _biases;
    private readonly ProjectWikiRequestRoutingGateEvidence? _gateEvidence;

    private ProjectWikiRequestRoutingTrainableModel(
        Dictionary<string, double[]> tokenWeights,
        double[] biases,
        ProjectWikiRequestRoutingGateEvidence? gateEvidence = null)
    {
        _tokenWeights = tokenWeights;
        _biases = biases;
        _gateEvidence = gateEvidence;
    }

    public static ProjectWikiRequestRoutingTrainableModel Train(IReadOnlyList<ProjectWikiRequestRoutingDpoPair> rows)
    {
        var model = new ProjectWikiRequestRoutingTrainableModel([], new double[Labels.Length]);
        // Признаки урока зависят только от его текста, поэтому они одинаковы во всех
        // эпохах: считаем их один раз на строку вместо EpochCount раз. Массив живёт
        // только внутри этого вызова, по одному элементу на строку, без группировки
        // одинаковых текстов — порядок строк, число предъявлений и веса не меняются.
        var prepared = rows.Select(row => (Row: row, Features: Featurize(row.Prompt))).ToArray();
        for (var epoch = 0; epoch < EpochCount; epoch++)
        {
            foreach (var item in prepared)
            {
                var row = item.Row;
                var features = item.Features;
                var expected = LabelIndex(row.ExpectedRouteKind);

                // Если ни одна оценка не измерена, предсказания нет и двигать
                // веса некуда: прежний помощник вернул бы в этом случае
                // нечисловой маршрут, и правка весов пошла бы по произвольному
                // ярлыку, портя состояние ещё сильнее. Такая строка урока
                // пропускается. Честная граница: сегодня этой веткой пройти
                // нельзя — обучение начинается с нулевых весов, а признаки
                // урока конечны, поэтому оценки здесь всегда измерены; это
                // защита договора, а не доказанная починка.
                if (!model.TryPredictIndex(features, out var predicted))
                    continue;

                if (predicted == expected)
                    continue;

                foreach (var (token, value) in features)
                {
                    var weights = model.GetWeights(token);
                    weights[expected] += LearningRate * value;
                    weights[predicted] -= LearningRate * value;
                }

                model._biases[expected] += LearningRate;
                model._biases[predicted] -= LearningRate;
            }
        }

        return model;
    }

    public static ProjectWikiRequestRoutingTrainableModel FromSnapshot(ProjectWikiRequestRoutingModelSnapshot snapshot)
        => new(
            snapshot.TokenWeights.ToDictionary(
                item => item.Key,
                item => NormalizeVector(item.Value),
                StringComparer.Ordinal),
            NormalizeVector(snapshot.Biases),
            snapshot.ToGateEvidence());

    public ProjectWikiRequestRoutingDecision Route(string prompt)
        => DecideOrDefer(prompt, requireEvaluationGate: true);

    internal ProjectWikiRequestRoutingDecision RouteForEvaluation(string prompt)
        => DecideOrDefer(prompt, requireEvaluationGate: false);

    public ProjectWikiRequestRoutingTrainableModel WithGateEvidence(ProjectWikiRequestRoutingGateEvidence gateEvidence)
        => new(CopyTokenWeights(_tokenWeights), _biases.ToArray(), gateEvidence);

    public ProjectWikiRequestRoutingModelSnapshot ToSnapshot(ProjectWikiRequestRoutingGateEvidence? gateEvidence = null)
    {
        var selectedGate = gateEvidence ?? _gateEvidence ?? MissingEvaluationGate();
        return new ProjectWikiRequestRoutingModelSnapshot(
            "averaged-token-perceptron-v1",
            Labels.Select(label => label.ToString()).ToArray(),
            CopyTokenWeights(_tokenWeights),
            _biases.ToArray(),
            EpochCount,
            _tokenWeights.Count,
            selectedGate.TrainingRowCount,
            selectedGate.HeldOutRowCount,
            selectedGate.EvaluationId,
            selectedGate.RouteAccuracy,
            selectedGate.WrongAnnotationRate,
            selectedGate.AverageConfidence,
            selectedGate.SemanticQualityClaimed,
            selectedGate.HeldOutGatePassed,
            selectedGate.MinimumRouteAccuracy,
            selectedGate.MinimumWrongAnnotationRate,
            selectedGate.MinimumAverageConfidence,
            selectedGate.MinimumTopTwoMargin,
            selectedGate.MinimumSelectionConfidence,
            selectedGate.PublishAllowed,
            selectedGate.Warnings);
    }

    public static ProjectWikiRequestRoutingDecision BuildLowConfidenceDecision(
        string reason,
        ProjectWikiRequestRoutingDecisionStatus status = ProjectWikiRequestRoutingDecisionStatus.NeedsSecondPass,
        ProjectWikiRequestRoutingGateEvidence? gateEvidence = null,
        double topTwoMargin = 0,
        string[]? warnings = null)
    {
        var selectedWarnings = warnings is { Length: > 0 }
            ? warnings
            : ["request_routing_deferred_needs_second_pass"];
        return new ProjectWikiRequestRoutingDecision(
            ProjectWikiRequestRouteKind.NotProjectWiki,
            "main-orchestrator",
            reason,
            LowConfidenceFromMargin(topTwoMargin),
            [],
            selectedWarnings)
        {
            DecisionStatus = status,
            GateEvidence = gateEvidence,
            TopTwoMargin = topTwoMargin
        };
    }

    private ProjectWikiRequestRoutingDecision DecideOrDefer(string prompt, bool requireEvaluationGate)
    {
        var features = Featurize(prompt ?? "");
        var scores = Score(features);

        // Договор помощника выбора изменился, и это главное в этой правке:
        // прежде он обещал «лучший есть всегда», теперь честно отвечает, что
        // выбирать было не из чего. Почему прежнее обещание для неизмеренной
        // оценки неверно — подробно объяснено у самого помощника ниже.
        var hasMeasuredScore = TryMaxMeasuredIndex(scores, out var predicted);
        var topTwoMargin = ComputeTopTwoMargin(scores);

        if (!HasTrainedSignal())
            return BuildLowConfidenceDecision(
                "Request routing deferred for second pass because this trainable model has no trained route weights.",
                ProjectWikiRequestRoutingDecisionStatus.NeedsSecondPass,
                _gateEvidence,
                topTwoMargin,
                ["request_routing_model_untrained"]);

        // Отказ по причине «ни одна оценка маршрута не измерена» стоит до
        // сравнений с порогами: сравнивать попросту нечего. Проверку «все
        // оценки нули» он не подменяет и её смысл не меняет — неизмеренная
        // оценка нулём не является, поэтому та дверь этого случая не видела и
        // видеть не могла. Человеку три причины отказа различимы: «модель не
        // обучена», «обученных оценок нет вовсе» и «все оценки нули»; первая
        // лечится обучением, вторая починкой измерения или обученного
        // состояния, третья улучшением модели.
        if (!hasMeasuredScore)
            return BuildLowConfidenceDecision(
                "Request routing deferred for second pass because no route score is a measured finite number.",
                ProjectWikiRequestRoutingDecisionStatus.NeedsSecondPass,
                _gateEvidence,
                topTwoMargin,
                ["request_routing_scores_" + ProjectWikiMeasuredNumberRule.NotMeasuredReasonSuffix]);

        if (scores.All(IsZero))
            return BuildLowConfidenceDecision(
                "Request routing deferred for second pass because every route score is zero.",
                ProjectWikiRequestRoutingDecisionStatus.NeedsSecondPass,
                _gateEvidence,
                topTwoMargin,
                ["request_routing_scores_all_zero"]);

        // Отказ по причине «запас между двумя лучшими не измерен» стоит ДО
        // сравнения с порогом запаса. Этот случай достижим и при состоявшемся
        // выборе: неизмеренная оценка в выборе не участвует, но в подсчёте
        // запаса она остаётся, и плюс бесконечность наверху даёт бесконечный
        // запас. Одно сравнение с порогом здесь бессильно: «не-число» ложно в
        // любом сравнении, а бесконечность больше любого порога, поэтому
        // прежняя дверь молча отвечала «запас достаточен».
        if (!ProjectWikiMeasuredNumberRule.IsMeasured(topTwoMargin))
            return BuildLowConfidenceDecision(
                "Request routing deferred for second pass because the top-two route margin is not a measured finite number.",
                ProjectWikiRequestRoutingDecisionStatus.NeedsSecondPass,
                _gateEvidence,
                topTwoMargin,
                ["request_routing_margin_" + ProjectWikiMeasuredNumberRule.NotMeasuredReasonSuffix]);

        // Прежнее сравнение с прежним порогом запаса, ни значение, ни строгость
        // не менялись.
        if (topTwoMargin < MinimumTopTwoMargin)
            return BuildLowConfidenceDecision(
                "Request routing deferred for second pass because the top-two route margin is below the selection threshold.",
                ProjectWikiRequestRoutingDecisionStatus.NeedsSecondPass,
                _gateEvidence,
                topTwoMargin,
                ["request_routing_margin_below_gate"]);

        // Заслон отложенной проверки. Решение принимает САМА проверка по ДОВЕРЕННЫМ порогам этого
        // файла, а не прочитанный из снимка признак прохождения и не пороги, приехавшие вместе со
        // снимком. Причина отказа называет точное поле, а не только общую примету.
        var gateRefusal = requireEvaluationGate
            ? DescribeEvaluationGateRefusal(_gateEvidence)
            : null;
        if (gateRefusal is not null)
            return BuildLowConfidenceDecision(
                gateRefusal.Value.Reason,
                ProjectWikiRequestRoutingDecisionStatus.NeedsSecondPass,
                _gateEvidence,
                topTwoMargin,
                // Общая примета сохранена первой, чтобы прежние читатели предупреждений
                // продолжали её видеть; вторая называет точное поле.
                ["request_routing_missing_or_failed_eval_gate", gateRefusal.Value.Warning]);

        var confidence = CalibratedConfidence(scores, predicted, topTwoMargin);

        // Здесь уверенность измерена уже по построению: выбранный маршрут
        // указывает на измеренную оценку, запас проверен выше, а сама сборка
        // отказывает на неизмеренных слагаемых до арифметики. Опереться на
        // ограничение отрезком было нельзя: ограничение отрезком «не-числа» —
        // снова «не-число», поэтому эта дверь его пропускала и маршрут
        // выбирался с неизмеренной уверенностью.
        if (confidence < MinimumSelectionConfidence)
            return BuildLowConfidenceDecision(
                "Request routing deferred for second pass because calibrated confidence is below the selection threshold.",
                ProjectWikiRequestRoutingDecisionStatus.NeedsSecondPass,
                _gateEvidence,
                topTwoMargin,
                ["request_routing_confidence_below_gate"]);

        var routeKind = Labels[predicted];
        return routeKind switch
        {
            ProjectWikiRequestRouteKind.ProjectWikiRequestRouterTraining => Decision(routeKind, ProjectWikiRequestRouterUnit.UnitId, "The held-out-gated request routing baseline selected request-router training.", confidence, ["developer_prompt", "route_kind_taxonomy"], topTwoMargin),
            ProjectWikiRequestRouteKind.NegativeDpoTraining => Decision(routeKind, NegativeDpoGeneratorTrainingRunner.UnitId, "The held-out-gated request routing baseline selected negative DPO training.", confidence, ["accepted_documentation_units", "negative_type_taxonomy"], topTwoMargin),
            ProjectWikiRequestRouteKind.ProjectValidation => Decision(routeKind, nameof(ProjectWikiRealProjectValidationRunner), "The held-out-gated request routing baseline selected real project wiki validation.", confidence, ["project_roots", "wiki_domain_root", "validation_question_set", "exact_evidence_gate"], topTwoMargin),
            ProjectWikiRequestRouteKind.AnswerComposerTraining => Decision(routeKind, ProjectWikiAnswerComposerTrainingRunner.UnitId, "The held-out-gated request routing baseline selected answer composition training.", confidence, ["retrieved_evidence", "answer_contract"], topTwoMargin),
            ProjectWikiRequestRouteKind.DocumentationGeneratorTraining => Decision(routeKind, DocumentationGeneratorUnit.UnitId, "The held-out-gated request routing baseline selected documentation generator training.", confidence, ["documentation_unit", "required_fields"], topTwoMargin),
            ProjectWikiRequestRouteKind.CodeTraceTraining => Decision(routeKind, CodeTraceBuilderUnit.UnitId, "The held-out-gated request routing baseline selected code trace training.", confidence, ["source_text", "expected_call_edges", "expected_value_flows"], topTwoMargin),
            ProjectWikiRequestRouteKind.EntityImpactTraining => Decision(routeKind, EntityImpactMapperUnit.UnitId, "The held-out-gated request routing baseline selected entity impact training.", confidence, ["entity_name", "evidence_records", "touchpoint_taxonomy"], topTwoMargin),
            ProjectWikiRequestRouteKind.DocumentationBuild => Decision(routeKind, ProjectDocumentationPipelineOrchestrator.FlowId, "The held-out-gated request routing baseline selected documentation build.", confidence, ["project_root", "documentation_version"], topTwoMargin),
            ProjectWikiRequestRouteKind.StackVersionTraining => Decision(routeKind, StackAndVersionClassifierUnit.UnitId, "The held-out-gated request routing baseline selected stack/version training.", confidence, ["csproj_text", "stack_version_taxonomy"], topTwoMargin),
            ProjectWikiRequestRouteKind.AnswerVerifierTraining => Decision(routeKind, ProjectAnswerVerifier.UnitId, "The held-out-gated request routing baseline selected answer verifier training.", confidence, ["candidate_answer", "evidence_index", "expected_project_id"], topTwoMargin),
            ProjectWikiRequestRouteKind.TraceAggregatorTraining => Decision(routeKind, WikiTraceAggregatorUnit.UnitId, "The held-out-gated request routing baseline selected trace aggregation training.", confidence, ["wiki_query_traces", "wiki_unit_traces"], topTwoMargin),
            ProjectWikiRequestRouteKind.QueryPlanningTraining => Decision(routeKind, ProjectWikiQueryPlanningFlow.FlowId, "The held-out-gated request routing baseline selected query planning training.", confidence, ["developer_prompt", "query_plan_taxonomy"], topTwoMargin),
            ProjectWikiRequestRouteKind.ContextRoutingTraining => Decision(routeKind, WikiContextRoutingUnit.UnitId, "The held-out-gated request routing baseline selected context routing training.", confidence, ["query_plan", "retriever_availability"], topTwoMargin),
            ProjectWikiRequestRouteKind.WikiQuestionAnswer => Decision(routeKind, ProjectLocalWikiOrchestrator.FlowId, "The held-out-gated request routing baseline selected project-local wiki answering.", confidence, ["project_id", "wiki_domain_root", "question"], topTwoMargin),
            _ => Decision(ProjectWikiRequestRouteKind.NotProjectWiki, "main-orchestrator", "The held-out-gated request routing baseline selected the main orchestrator.", confidence, [], topTwoMargin)
        };
    }

    private ProjectWikiRequestRoutingDecision Decision(
        ProjectWikiRequestRouteKind routeKind,
        string targetUnitId,
        string reason,
        double confidence,
        string[] requiredInputs,
        double topTwoMargin)
        => new(routeKind, targetUnitId, reason, confidence, requiredInputs, [])
        {
            DecisionStatus = ProjectWikiRequestRoutingDecisionStatus.Selected,
            GateEvidence = _gateEvidence,
            TopTwoMargin = topTwoMargin
        };

    private bool TryPredictIndex(Dictionary<string, double> features, out int predicted)
        // Тот же изменённый договор, что и у выбора лучшего: предсказания может
        // не быть вовсе, если ни одна оценка не измерена.
        => TryMaxMeasuredIndex(Score(features), out predicted);

    private double[] Score(Dictionary<string, double> features)
    {
        var scores = _biases.ToArray();
        foreach (var (token, value) in features)
        {
            if (!_tokenWeights.TryGetValue(token, out var weights))
                continue;

            for (var index = 0; index < scores.Length; index++)
                scores[index] += weights[index] * value;
        }

        return scores;
    }

    private double[] GetWeights(string token)
    {
        if (_tokenWeights.TryGetValue(token, out var weights))
            return weights;

        weights = new double[Labels.Length];
        _tokenWeights[token] = weights;
        return weights;
    }

    private bool HasTrainedSignal()
        => _biases.Any(value => !IsZero(value)) ||
           _tokenWeights.Values.Any(weights => weights.Any(value => !IsZero(value)));

    // GateRefusal — одна причина отказа заслона: человеческая фраза и примета с точным полем.
    private readonly record struct GateRefusal(string Reason, string Warning);

    // DescribeEvaluationGateRefusal — ЕДИНСТВЕННАЯ проверка заслона отложенной проверки. Пусто
    // означает «заслон пройден»; иначе названа причина и точное поле.
    //
    // Что было неправильно, по-человечески. Прежняя проверка сравнивала три наблюдения с
    // порогами, ПРИЕХАВШИМИ ВМЕСТЕ СО СНИМКОМ (`gateEvidence.Minimum...`), и дополнительно
    // требовала сохранённый в снимке признак прохождения. Доверенные пороги этого файла — 0.95,
    // 1.0 и 0.85 — в сравнении не участвовали вовсе: они заполняли только пустой заслон. Поэтому
    // снимок, принёсший свои нулевые пороги и признак прохождения, открывал маршрут при любых
    // наблюдениях. Конечность чисел не проверялась ни у наблюдений, ни у порогов: плюс
    // бесконечность больше любого порога, то есть неизмеренное наблюдение проходило заслон, а
    // «не-число» давало отказ без названия поля.
    //
    // Что стало. Сначала проверяется, что все три наблюдения — настоящие измеренные числа. Затем
    // что сохранённые пороги измерены И совпадают с действующей политикой: конечность сама по
    // себе разрешённым порогом не является. Только потом идут сравнения, и сравниваются
    // наблюдения с ДОВЕРЕННЫМИ порогами. Сохранённый признак прохождения остался, но он может
    // только ЗАПРЕТИТЬ: разрешение даёт сравнение, а не флаг из снимка.
    //
    // Чего здесь нет намеренно. Ни приведения бесконечности к единице, ни подстановки нуля, ни
    // снижения действующих порогов: неизмеренное значение — это отказ, а не повод смягчить порог.
    private static GateRefusal? DescribeEvaluationGateRefusal(
        ProjectWikiRequestRoutingGateEvidence? gateEvidence)
    {
        // Заслона нет вовсе: снимок пришёл без свидетельства о проверке на отложенных строках.
        if (gateEvidence is null)
            return new GateRefusal(
                "Request routing deferred for second pass because no held-out evaluation gate is attached to this snapshot.",
                "request_routing_snapshot_missing_eval_gate");

        // Шаг 1: все три сравниваемых наблюдения обязаны быть настоящими измеренными числами.
        if (!ProjectWikiMeasuredNumberRule.IsMeasured(gateEvidence.RouteAccuracy))
            return new GateRefusal(
                "Request routing deferred for second pass because the held-out route accuracy of this snapshot is not a measured number.",
                "request_routing_gate_route_accuracy_" + ProjectWikiMeasuredNumberRule.NotMeasuredReasonSuffix);
        if (!ProjectWikiMeasuredNumberRule.IsMeasured(gateEvidence.WrongAnnotationRate))
            return new GateRefusal(
                "Request routing deferred for second pass because the wrong-annotation rate of this snapshot is not a measured number.",
                "request_routing_gate_wrong_annotation_rate_" + ProjectWikiMeasuredNumberRule.NotMeasuredReasonSuffix);
        if (!ProjectWikiMeasuredNumberRule.IsMeasured(gateEvidence.AverageConfidence))
            return new GateRefusal(
                "Request routing deferred for second pass because the average confidence of this snapshot is not a measured number.",
                "request_routing_gate_average_confidence_" + ProjectWikiMeasuredNumberRule.NotMeasuredReasonSuffix);

        // Шаг 2: сохранённые пороги обязаны совпадать с действующей политикой этого файла.
        // Измеренность порога проверяется этим же сравнением: неизмеренное число не равно
        // доверенному порогу ни при каком его значении.
        if (gateEvidence.MinimumRouteAccuracy != MinimumHeldOutRouteAccuracy)
            return new GateRefusal(
                "Request routing deferred for second pass because the stored route-accuracy threshold of this snapshot does not match the active policy threshold.",
                "request_routing_gate_route_accuracy_threshold_untrusted");
        if (gateEvidence.MinimumWrongAnnotationRate != MinimumWrongAnnotationRate)
            return new GateRefusal(
                "Request routing deferred for second pass because the stored wrong-annotation threshold of this snapshot does not match the active policy threshold.",
                "request_routing_gate_wrong_annotation_rate_threshold_untrusted");
        if (gateEvidence.MinimumAverageConfidence != MinimumAverageConfidence)
            return new GateRefusal(
                "Request routing deferred for second pass because the stored average-confidence threshold of this snapshot does not match the active policy threshold.",
                "request_routing_gate_average_confidence_threshold_untrusted");

        // Шаг 3: сохранённый признак прохождения может только запретить. Разрешения он не даёт:
        // ниже всё равно идут сравнения с доверенными порогами.
        if (!gateEvidence.HeldOutGatePassed)
            return new GateRefusal(
                "Request routing deferred for second pass because the snapshot itself records that its held-out evaluation gate did not pass.",
                "request_routing_gate_not_passed_in_snapshot");

        // Шаг 4: сравнения с ДОВЕРЕННЫМИ порогами этого файла, а не со снимочными.
        if (gateEvidence.RouteAccuracy < MinimumHeldOutRouteAccuracy)
            return new GateRefusal(
                "Request routing deferred for second pass because the held-out route accuracy is below the active policy threshold.",
                "request_routing_gate_route_accuracy_below_gate");
        if (gateEvidence.WrongAnnotationRate < MinimumWrongAnnotationRate)
            return new GateRefusal(
                "Request routing deferred for second pass because the wrong-annotation rate is below the active policy threshold.",
                "request_routing_gate_wrong_annotation_rate_below_gate");
        if (gateEvidence.AverageConfidence < MinimumAverageConfidence)
            return new GateRefusal(
                "Request routing deferred for second pass because the average confidence is below the active policy threshold.",
                "request_routing_gate_average_confidence_below_gate");

        // Все четыре шага пройдены: заслон открыт самой проверкой.
        return null;
    }

    private static ProjectWikiRequestRoutingGateEvidence MissingEvaluationGate()
        => new(
            "missing-held-out-evaluation-gate",
            0,
            0,
            0,
            0,
            0,
            false,
            false,
            false,
            MinimumHeldOutRouteAccuracy,
            MinimumWrongAnnotationRate,
            MinimumAverageConfidence,
            MinimumTopTwoMargin,
            MinimumSelectionConfidence,
            ["request_routing_snapshot_missing_eval_gate"]);

    private static int LabelIndex(ProjectWikiRequestRouteKind routeKind)
    {
        var index = Array.IndexOf(Labels, routeKind);
        return index < 0 ? Labels.Length - 1 : index;
    }

    /// <summary>
    /// Выбирает самую высокую ИЗМЕРЕННУЮ оценку маршрута и честно сообщает
    /// вызывающему, когда измеренных оценок нет вовсе.
    ///
    /// Почему прежний договор «лучший есть всегда» для неизмеренной оценки
    /// неверен. Прежний помощник начинал с нулевого индекса и вытеснял его
    /// только по условию «строго больше текущего лучшего». Для «не-числа»
    /// любое сравнение ложно, поэтому нечисловая нулевая оценка не вытеснялась
    /// ничем и возвращалась как лучшая: модель выбирала именно тот маршрут,
    /// оценки которого у неё нет.
    ///
    /// Почему защищать нужно сам выбор, а не только дверь запаса. Дальше эта
    /// беда не ловилась ничем. Запас между двумя лучшими считается сортировкой
    /// по убыванию, а «не-число» в этом сравнителе уходит в конец, поэтому при
    /// трёх и более маршрутах верхние два конечны и дверь запаса проходит
    /// ЗАКОННО. Проверка «все оценки нули» тоже молчит: «не-число» нулём не
    /// является. В итоге маршрут выбирался, а уверенность, собранная из
    /// нечисловой оценки, проходила и дверь порога выбора.
    ///
    /// Что изменилось в поведении. Неизмеренные оценки в выборе не участвуют.
    /// Для набора, в котором все оценки измерены, ответ дословно тот же, что и
    /// раньше: тот же порядок обхода и то же строгое сравнение, поэтому при
    /// равных оценках по-прежнему выигрывает более ранний маршрут.
    /// </summary>
    private static bool TryMaxMeasuredIndex(double[] values, out int best)
    {
        // Пока измеренной оценки не встретилось, лучшего нет: отрицательный
        // индекс означает ровно «ещё ничего не выбрано».
        best = -1;
        for (var index = 0; index < values.Length; index++)
        {
            // Неизмеренная оценка не может ни победить, ни проиграть: в
            // сравнениях она молчит, поэтому её просто нет в выборе.
            if (!ProjectWikiMeasuredNumberRule.IsMeasured(values[index]))
                continue;

            // Первая встреченная измеренная оценка становится лучшей; дальше —
            // прежнее строгое сравнение, сохраняющее прежний разрыв ничьей в
            // пользу более раннего маршрута.
            if (best < 0 || values[index] > values[best])
                best = index;
        }

        // Ответ «да» означает, что выбранный индекс указывает на измеренную
        // оценку; ответ «нет» — что выбирать было не из чего.
        return best >= 0;
    }

    private static double ComputeTopTwoMargin(double[] scores)
    {
        if (scores.Length == 0)
            return 0;

        var ordered = scores.OrderByDescending(score => score).Take(2).ToArray();
        return ordered.Length < 2 ? Math.Max(0, ordered[0]) : Math.Max(0, ordered[0] - ordered[1]);
    }

    private static double CalibratedConfidence(double[] scores, int predicted, double topTwoMargin)
    {
        // Уверенность, собранная из неизмеренного слагаемого, отвергается до
        // всякого сравнения с порогом — и до порога запаса здесь, и до порога
        // выбора у вызывающего. Ноль означает «уверенности нет» и надёжно ниже
        // любого порога выбора, поэтому эта сборка не может вернуть
        // неизмеренное число ни одному будущему вызывающему. Честная граница:
        // сегодня этой веткой пройти нельзя — и запас, и оценка выбранного
        // маршрута проверены у вызывающего выше, — это защита договора, а не
        // доказанная починка.
        if (ProjectWikiMeasuredNumberRule.AnyNotMeasured(topTwoMargin, scores[predicted]))
            return 0.0;

        // Прежнее сравнение с прежним порогом и прежняя формула, без
        // изменений.
        if (topTwoMargin < MinimumTopTwoMargin)
            return LowConfidenceFromMargin(topTwoMargin);

        var marginConfidence = 1.0 / (1.0 + Math.Exp(-4.0 * topTwoMargin));
        var scoreConfidence = 1.0 / (1.0 + Math.Exp(-Math.Max(0, scores[predicted])));
        return Math.Clamp(0.70 + marginConfidence * 0.20 + scoreConfidence * 0.09, MinimumSelectionConfidence, 0.99);
    }

    private static double LowConfidenceFromMargin(double topTwoMargin)
    {
        // Даже ОТКАЗ не имеет права нести неизмеренную уверенность. Этот
        // помощник вызывается из общедоступной сборки отложенного решения, куда
        // запас приходит аргументом, поэтому неизмеренное число попадает сюда
        // достижимо. Для «не-числа» и деление, и ограничение отрезком снова
        // дают «не-число», и отложенное решение уходило бы к читателю с
        // уверенностью, которую нельзя сравнить ни с одним порогом. Ноль
        // означает «уверенности нет». Сам наблюдённый запас в решении при этом
        // сохраняется как есть: читателю важно видеть, что именно пришло.
        if (!ProjectWikiMeasuredNumberRule.IsMeasured(topTwoMargin))
            return 0.0;

        // Прежняя формула с прежними порогами, без изменений.
        var normalized = Math.Max(0, topTwoMargin) / Math.Max(MinimumTopTwoMargin, NumericZero);
        return Math.Clamp(normalized * LowConfidenceCeiling, 0, LowConfidenceCeiling);
    }

    private static bool IsZero(double value)
        => Math.Abs(value) <= NumericZero;

    private static Dictionary<string, double> Featurize(string text)
    {
        var tokens = RegexLikeWords(text).ToArray();
        var features = new Dictionary<string, double>(StringComparer.Ordinal);
        foreach (var token in tokens)
            features[token] = features.TryGetValue(token, out var value) ? value + 1.0 : 1.0;
        for (var index = 0; index + 1 < tokens.Length; index++)
        {
            var bigram = $"{tokens[index]}_{tokens[index + 1]}";
            features[bigram] = features.TryGetValue(bigram, out var value) ? value + 1.0 : 1.0;
        }

        var normalizer = Math.Max(1.0, Math.Sqrt(features.Values.Sum(value => value * value)));
        foreach (var key in features.Keys.ToArray())
            features[key] /= normalizer;
        return features;
    }

    private static IEnumerable<string> RegexLikeWords(string text)
    {
        var builder = new StringBuilder();
        foreach (var character in text)
        {
            if (char.IsLetterOrDigit(character) || character == '_' || character == '-')
            {
                builder.Append(char.ToLowerInvariant(character));
                continue;
            }

            if (builder.Length == 0)
                continue;

            yield return builder.ToString();
            builder.Clear();
        }

        if (builder.Length > 0)
            yield return builder.ToString();
    }

    private static Dictionary<string, double[]> CopyTokenWeights(Dictionary<string, double[]> tokenWeights)
        => tokenWeights.ToDictionary(item => item.Key, item => item.Value.ToArray(), StringComparer.Ordinal);

    private static double[] NormalizeVector(double[] values)
    {
        var normalized = new double[Labels.Length];
        Array.Copy(values, normalized, Math.Min(values.Length, normalized.Length));
        return normalized;
    }
}

public sealed class ProjectWikiRequestRoutingUnit : ILogicalUnit<ProjectWikiRequestRoutingInput, ProjectWikiRequestRoutingDecision>
{
    public const string UnitId = "project-wiki-request-routing-unit";
    public const string UnitRole = "project_wiki_request_routing_confidence_decision";
    public const string DomainTag = "project-wiki";

    private readonly ProjectWikiRequestRoutingTrainableModel? _model;

    public ProjectWikiRequestRoutingUnit(ProjectWikiRequestRoutingTrainableModel? model = null)
    {
        _model = model;
    }

    public Task<OperationResult<LogicalUnitResult<ProjectWikiRequestRoutingDecision>>> ExecuteAsync(
        LogicalUnitRequest<ProjectWikiRequestRoutingInput> request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(OperationResult<LogicalUnitResult<ProjectWikiRequestRoutingDecision>>.Fail("Request routing cancelled.", "REQUEST_ROUTING_CANCELLED"));

        if (request is null)
            return Task.FromResult(OperationResult<LogicalUnitResult<ProjectWikiRequestRoutingDecision>>.Fail("Logical unit request is required.", "LOGICAL_UNIT_REQUEST_REQUIRED"));

        if (string.IsNullOrWhiteSpace(request.Input.Prompt))
            return Task.FromResult(OperationResult<LogicalUnitResult<ProjectWikiRequestRoutingDecision>>.Fail("Prompt is required.", "PROMPT_REQUIRED"));

        var model = request.Input.ModelSnapshot is null
            ? _model
            : ProjectWikiRequestRoutingTrainableModel.FromSnapshot(request.Input.ModelSnapshot);
        var decision = model is null
            ? ProjectWikiRequestRoutingTrainableModel.BuildLowConfidenceDecision(
                "Request routing deferred for second pass because no trained request-routing snapshot is loaded.",
                ProjectWikiRequestRoutingDecisionStatus.NoRoute,
                warnings: ["request_routing_snapshot_missing"])
            : model.Route(request.Input.Prompt);
        var evidenceIds = string.IsNullOrWhiteSpace(decision.GateEvidence?.EvaluationId)
            ? []
            : new[] { decision.GateEvidence.EvaluationId };
        var result = new LogicalUnitResult<ProjectWikiRequestRoutingDecision>(
            string.IsNullOrWhiteSpace(request.UnitId) ? UnitId : request.UnitId,
            UnitRole,
            DomainTag,
            decision,
            decision.Confidence,
            ["project_wiki_request_routing_gate"],
            [DomainTag, "request-routing"],
            evidenceIds,
            decision.Warnings);

        return Task.FromResult(OperationResult<LogicalUnitResult<ProjectWikiRequestRoutingDecision>>.Ok(result));
    }
}
