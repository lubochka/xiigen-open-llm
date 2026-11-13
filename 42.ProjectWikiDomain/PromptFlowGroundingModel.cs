// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

public sealed class PromptFlowGroundingModel
{
    private readonly OrchestrationKnnStore _knn = new();
    private readonly Dictionary<string, OrchestrationFlowGroundingWeights> _weights = new(StringComparer.Ordinal);

    public int KnnSize => _knn.Count;

    public int WeightsSize => _weights.Count;

    public void Train(OrchestrationTrainingPair pair)
    {
        if (pair.Mode != PairMode.Mode1LearnedFlowGrounding || string.IsNullOrWhiteSpace(pair.FlowRef))
            return;

        // FIX-RET: прежние уроки этого маршрута читаются ДО добавления нового —
        // иначе новый урок попал бы в этот же список и оказался совместимым сам
        // с собой, то есть допуск накопления перестал бы что-либо проверять.
        var storedSameFlowPairs = ExportPairs()
            .Where(stored => string.Equals(stored.FlowRef, pair.FlowRef, StringComparison.Ordinal))
            .ToArray();

        // Прежняя запись урока в память соседей, без изменений.
        _knn.Add(pair);

        // Свежие веса считаются как раньше: все поля записи берутся из этого расчёта.
        var fresh = BuildWeights(pair);

        // Объединять есть смысл только когда у маршрута уже есть обученные веса и
        // когда новый урок — совместимый парафраз прежних, а не их исправление.
        if (_weights.TryGetValue(pair.FlowRef, out var previous) &&
            CanAccumulateCompatibleInputFeatures(pair, storedSameFlowPairs))
        {
            // Прежний входной словарь берётся за основу: отсутствующий в свежем
            // расчёте ключ — это отсутствие, а не вычитание, поэтому прежде
            // выученные ключи просто остаются на месте и ответ не теряется.
            var mergedInputFeatures = previous.InputFeatureWeights.ToDictionary(
                feature => feature.Key,
                feature => feature.Value,
                StringComparer.OrdinalIgnoreCase);

            // По каждому ключу без учёта регистра берётся максимум старого и
            // нового: повторяющийся токен получает максимум, а не сумму.
            foreach (var feature in fresh.InputFeatureWeights)
                mergedInputFeatures[feature.Key] = Math.Max(
                    mergedInputFeatures.GetValueOrDefault(feature.Key),
                    feature.Value);

            // Объединяется ровно один словарь — входные признаки; остальные семь
            // полей записи весов остаются из свежего расчёта, как было раньше.
            fresh = fresh with { InputFeatureWeights = mergedInputFeatures };
        }

        // Запись в память весов: при совместимом парафразе здесь объединённые
        // признаки, в любом другом случае — прежняя полная замена.
        _weights[pair.FlowRef] = fresh;
    }

    public FlowGroundingResult? TryGround(string prompt, HumanIntentContract contract)
    {
        if (_knn.Count == 0 || _weights.Count == 0)
            return null;

        var queryText = BuildQueryText(prompt, contract);
        var tokens = OrchestrationKnnStore.Tokenize(queryText).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var best = _weights
            .Select(pair => new
            {
                FlowRef = pair.Key,
                InputScore = ScoreTokens(tokens, pair.Value.InputFeatureWeights),
                AllowedScopeScore = ScoreTokens(tokens, pair.Value.AllowedScopeFeatures),
                Weights = pair.Value
            })
            .Select(item => new
            {
                item.FlowRef,
                Score = item.InputScore + item.AllowedScopeScore,
                item.InputScore,
                item.AllowedScopeScore,
                item.Weights
            })
            .OrderByDescending(item => item.Score)
            .ThenByDescending(item => item.AllowedScopeScore)
            .ThenBy(item => item.FlowRef, StringComparer.Ordinal)
            .FirstOrDefault();

        // Отказ по причине «число не измерено» стоит ДО сравнения с порогом.
        // Оценка — сумма обученных весов по совпавшим токенам, деления в ней
        // нет, поэтому неизмеренное значение может прийти только из обученного
        // состояния: веса приходят с загруженной контрольной точкой и никакой
        // проверки по дороге не проходят. Честная граница: нечислового веса
        // никто не наблюдал — это защита от достижимого входа, а не описание
        // случившейся поломки. Одно сравнение с порогом здесь бессильно:
        // «не-число» ложно в любом сравнении, поэтому прежняя дверь «оценка не
        // больше нуля» молча отвечала «порог пройден», а плюс бесконечность
        // она и вовсе считала лучшим из кандидатов.
        if (best is null || !ProjectWikiMeasuredNumberRule.IsMeasured(best.Score))
            return null;

        // Прежнее сравнение с прежним порогом: ни значение, ни строгость не
        // менялись. Человеку два отказа различимы по месту: выше — «обученной
        // оценки нет», здесь — «оценка слишком мала»; первая беда лечится
        // починкой измерения или обученного состояния, вторая улучшением
        // модели. Пустая ссылка без причины сохранена, как и было; изменилось
        // только то, что при неизмеренном числе отказ наступает раньше.
        if (best.Score <= 0.0)
            return null;

        var nearest = _knn.FindNearestWithScore(queryText, PairMode.Mode1LearnedFlowGrounding);

        // Та же проверка измеренности у второго слагаемого уверенности — ради
        // одинаковой защиты обоих, а не потому, что здесь наблюдалась беда.
        // Честная граница: сегодня этой веткой пройти нельзя. Оценка соседа —
        // мера Жаккара (доля общих токенов среди всех), она считается рядом, в
        // хранилище обученных примеров, с явными защитами от деления на ноль,
        // поэтому «не-числом» или бесконечностью стать не может. Это защита на
        // будущее, на случай смены меры близости — например косинусной, где
        // нулевой вектор даёт деление на ноль, — и падающей проверки на неё не
        // существует, поэтому доказанной починкой она не является.
        if (nearest is null || !ProjectWikiMeasuredNumberRule.IsMeasured(nearest.Score))
            return null;

        // Прежнее сравнение с прежним порогом, без изменений.
        if (nearest.Score <= 0.0)
            return null;

        var confidence = ComputeInputOnlyConfidence(best.Score, nearest.Score);

        // Здесь уверенность измерена уже по построению: оба слагаемых прошли
        // проверку выше, а сама сборка отказывает на неизмеренных слагаемых до
        // арифметики. Опираться на взятие минимума было нельзя: минимум
        // единицы и «не-числа» — снова «не-число», поэтому эта дверь его
        // пропускала, а минимум единицы и плюс бесконечности молча даёт ровно
        // единицу, то есть наибольшую возможную уверенность из неизмеренного
        // веса.
        if (confidence < 0.50)
            return null;

        return new FlowGroundingResult(
            best.FlowRef,
            confidence,
            BuildRuntimeEvidenceFeatures(best.Weights),
            best.Weights.PhaseWeights.Keys.Order(StringComparer.Ordinal).ToArray(),
            best.Weights.ArtifactWeights.Keys.Order(StringComparer.Ordinal).ToArray(),
            best.Weights.GateWeights.Keys.Order(StringComparer.Ordinal).ToArray());
    }

    public OrchestrationTrainingPair? FindLearnedPair(
        string prompt,
        HumanIntentContract contract,
        FlowGroundingResult grounding)
    {
        var queryText = BuildLearnedPairQueryTextWithoutPredictedFlow(prompt, contract, grounding);

        // FIX-IM: the route is already decided by the learned weights above, so the body
        // must come from that same route. Without the restriction the global nearest
        // neighbour can win on token overlap alone and hand back another route's plan
        // under the chosen route's heading. The query itself is unchanged: the predicted
        // route is a filter here, never a feature added to the search text.
        return string.IsNullOrWhiteSpace(queryText)
            ? null
            : _knn.FindNearest(queryText, PairMode.Mode1LearnedFlowGrounding, grounding.FlowRef);
    }

    public IReadOnlyList<OrchestrationTrainingPair> ExportPairs()
        => _knn.ExportPairs();

    public IReadOnlyDictionary<string, OrchestrationFlowGroundingWeights> ExportWeights()
        => _weights
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);

    public void ImportState(
        IEnumerable<OrchestrationTrainingPair> pairs,
        IReadOnlyDictionary<string, OrchestrationFlowGroundingWeights> weights)
    {
        _knn.ImportPairs(pairs);
        _weights.Clear();
        foreach (var pair in weights)
            _weights[pair.Key] = pair.Value;
    }

    public IReadOnlyDictionary<string, IReadOnlyList<string>> ExportPatternMap()
        => _knn.ExportPairs()
            .GroupBy(pair => pair.FlowRef ?? "UNKNOWN", StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<string>)group.Select(pair => pair.PairId).Order(StringComparer.Ordinal).ToArray(),
                StringComparer.Ordinal);

    // FIX-RET: допуск накопления решается сравнением положительных целей уроков, а
    // не похожестью текстов запроса — семантических догадок по словам запроса здесь
    // нет ни одной. Совместимым считается только парафраз, который учит тому же
    // маршруту и тому же телу плана; любое расхождение цели — это исправление, и
    // тогда сохраняется прежняя замена признаков.
    private static bool CanAccumulateCompatibleInputFeatures(
        OrchestrationTrainingPair candidate,
        IReadOnlyList<OrchestrationTrainingPair> storedSameFlowPairs)
    {
        // Накапливать не с чем, пока у маршрута нет ни одного прежнего урока.
        if (storedSameFlowPairs.Count == 0)
            return false;

        // Нормализованное тело положительного плана нового урока — единственная
        // величина, по которой сверяются цели уроков.
        var candidatePlanText = NormalizeWhitespace(candidate.Chosen.PlanText);

        // Достаточно одного расхождения цели, чтобы накопление не разрешать.
        foreach (var stored in storedSameFlowPairs)
        {
            // Тот же режим урока: накопление касается только обучения привязке.
            if (stored.Mode != PairMode.Mode1LearnedFlowGrounding)
                return false;

            // Тот же непустой маршрут урока.
            if (string.IsNullOrWhiteSpace(stored.FlowRef) ||
                !string.Equals(stored.FlowRef, candidate.FlowRef, StringComparison.Ordinal))
                return false;

            // Тот же маршрут у положительной цели.
            if (!string.Equals(stored.Chosen.FlowRef, candidate.Chosen.FlowRef, StringComparison.Ordinal))
                return false;

            // Тот же ожидаемый режим маршрута.
            if (stored.ExpectedRouteMode != candidate.ExpectedRouteMode)
                return false;

            // То же тело положительного плана после нормализации пробелов.
            if (!string.Equals(
                    NormalizeWhitespace(stored.Chosen.PlanText),
                    candidatePlanText,
                    StringComparison.Ordinal))
                return false;
        }

        // Все прежние уроки этого маршрута учат тому же — накопление разрешено.
        return true;
    }

    // Нормализация текста плана только по пробелам: регистр не приводится и
    // содержание положительной цели не меняется, чтобы несовместимый урок нельзя
    // было провести как совместимый.
    private static string NormalizeWhitespace(string text)
        => string.Join(" ", (text ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static OrchestrationFlowGroundingWeights BuildWeights(OrchestrationTrainingPair pair)
    {
        return new OrchestrationFlowGroundingWeights(
            BuildTokenWeights(BuildInputOnlyTrainingTokens(pair)),
            BuildTokenWeights(BuildAllowedScopeFeatureTokens(pair)),
            BuildTokenWeights(OrchestrationKnnStore.Tokenize(pair.Chosen.Title)),
            BuildTokenWeights(BuildEvidenceOnlyTargetTokens(pair)),
            new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase)
            {
                ["classification"] = 1.0f,
                ["plan"] = 1.0f,
                ["gate"] = 1.0f
            },
            new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase)
            {
                ["learned-flow-plan"] = 1.0f,
                ["state"] = 0.5f
            },
            new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase)
            {
                ["Gate B"] = 1.0f,
                ["approval"] = 0.5f
            },
            new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase)
            {
                ["project-wiki"] = 1.0f,
                ["orchestration"] = 1.0f
            });
    }

    private static IReadOnlyList<string> BuildInputOnlyTrainingTokens(OrchestrationTrainingPair pair)
        => OrchestrationKnnStore.Tokenize(pair.Prompt);

    private static IReadOnlyList<string> BuildAllowedScopeFeatureTokens(OrchestrationTrainingPair pair)
        => OrchestrationKnnStore.Tokenize(pair.FlowRef ?? string.Empty);

    private static IReadOnlyList<string> BuildEvidenceOnlyTargetTokens(OrchestrationTrainingPair pair)
        => OrchestrationKnnStore.Tokenize($"{pair.FlowRef} {pair.Chosen.FlowRef}");

    private static IReadOnlyDictionary<string, float> BuildTokenWeights(IEnumerable<string> tokens)
        => tokens
            .GroupBy(token => token, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => (float)Math.Min(3, group.Count()), StringComparer.OrdinalIgnoreCase);

    private static IReadOnlyList<string> BuildRuntimeEvidenceFeatures(OrchestrationFlowGroundingWeights weights)
        => weights.InputFeatureWeights.Keys
            .Concat(weights.AllowedScopeFeatures.Keys)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.Ordinal)
            .Take(8)
            .ToArray();

    private static string BuildQueryText(string prompt, HumanIntentContract contract)
        => string.Join(
            ' ',
            new[]
            {
                prompt,
                string.Join(' ', contract.AllowedFlowScope),
                string.Join(' ', contract.RequestedFlowTextSpans),
                string.Join(' ', contract.HumanConstraints)
            }.Where(part => !string.IsNullOrWhiteSpace(part)));

    private static string BuildLearnedPairQueryTextWithoutPredictedFlow(
        string prompt,
        HumanIntentContract contract,
        FlowGroundingResult grounding)
        => string.Join(
            ' ',
            new[]
            {
                prompt,
                string.Join(' ', contract.AllowedFlowScope),
                string.Join(' ', contract.RequestedFlowTextSpans),
                string.Join(' ', contract.HumanConstraints),
                string.Join(' ', grounding.EvidenceFeatures)
            }.Where(part => !string.IsNullOrWhiteSpace(part)));

    private static double ComputeInputOnlyConfidence(double inputOnlyScore, double nearestScore)
    {
        // Уверенность, собранная из неизмеренных слагаемых, отвергается до
        // всякого сравнения с порогом. Ноль здесь означает «уверенности нет» —
        // ровно то же значение, что и для неположительных оценок ниже, — и
        // прежняя дверь порога его отвергает. Так эта сборка не может вернуть
        // неизмеренное число ни одному будущему вызывающему. Честная граница:
        // сегодня этой веткой пройти нельзя, оба слагаемых проверены у
        // вызывающего, — это защита договора, а не доказанная починка.
        if (ProjectWikiMeasuredNumberRule.AnyNotMeasured(inputOnlyScore, nearestScore))
            return 0.0;

        // Прежние сравнения с прежними порогами и прежняя формула, без
        // единого изменения значений.
        return inputOnlyScore <= 0.0 || nearestScore <= 0.0
            ? 0.0
            : Math.Min(1.0, 0.70 + inputOnlyScore * 0.10 + nearestScore * 0.20);
    }

    private static double ScoreTokens(
        IReadOnlySet<string> tokens,
        IReadOnlyDictionary<string, float> weights)
        => weights
            .Where(pair => tokens.Contains(pair.Key))
            .Sum(pair => pair.Value);
}
