// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentAlgorithms.Skills;

using SemanticCompiler;

// Production skill retriever from E38V2: Jaccard KNN over collapsed trigger tags.
//
// Граница детерминированного и обученного. Мера Жаккара словесных признаков,
// порог 0,40, обязательные навыки и обрезание до пяти штук — это
// ДЕТЕРМИНИРОВАННОЕ БАЗОВОЕ ОСНОВАНИЕ (внутренняя деталь, baseline), а не
// обученная способность. Обученную власть над маршрутом несёт отдельная единица
// разрешённого уровня `ILogicalUnit<TInput,TOutput>` — обученная память
// ближайших соседей; её ответ приходит сюда как типизированный сигнал
// `LearnedSkillRouteSignal` и становится СЛАГАЕМЫМ к базовой мере. Сам этот
// класс обученным механизмом не является и таковым не объявляется.
public sealed class SkillRetriever
{
    private const double ConfidenceThreshold = 0.40;
    private const int TopK = 5;

    // Выбор и загрузка навыка. Необязательный обученный сигнал подключается ДО
    // сортировки и ДО порога: ключ ранжирования кандидата, которого назвала
    // обученная память, равен «базовая мера Жаккара + обученная оценка».
    // Отсутствующий или нулевой сигнал сохраняет прежнее поведение полностью.
    public (IReadOnlyList<SkillTriggerLibrary.SkillTrigger> Skills, bool IsFallback) Retrieve(
        string taskDescription,
        int? expectedCategory = null,
        LearnedSkillRouteSignal? learnedSignal = null,
        SkillRouteConfiguration? routeConfiguration = null)
    {
        // Некорректный сигнал отвергается ДО выдачи списка навыков: прямой вызов
        // с таким аргументом получает исключение о неверном аргументе и ни при
        // каких условиях не получает максимальный приоритет вместо проверки.
        RejectInvalidLearnedSignal(learnedSignal, routeConfiguration);

        var queryTags = FamilyCollapser.CollapseString(taskDescription ?? string.Empty).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var ranked = SkillTriggerLibrary.All
            .Where(trigger => !trigger.AlwaysOn)
            .Select(trigger => new RankedSkill(
                trigger,
                // Ключом служит базовая мера плюс обученное слагаемое этого кандидата:
                // проверять порог по старой нулевой оценке до применения сигнала нельзя,
                // иначе мост до выбора навыка просто не доходит.
                JaccardSim(queryTags, FamilyCollapser.CollapseString(trigger.Condition).ToHashSet(StringComparer.OrdinalIgnoreCase))
                    + LearnedScoreFor(learnedSignal, trigger)))
            .OrderByDescending(item => item.Score)
            .ToArray();
        var maxScore = ranked.Length == 0 ? 0.0 : ranked[0].Score;
        if (maxScore < ConfidenceThreshold)
        {
            var fallback = expectedCategory.HasValue
                ? SkillTriggerLibrary.All.Where(trigger => trigger.AlwaysOn || trigger.Category == expectedCategory.Value).ToArray()
                : SkillTriggerLibrary.AlwaysRequired.ToArray();
            return (fallback, true);
        }

        var selected = SkillTriggerLibrary.AlwaysRequired.ToList();
        selected.AddRange(ranked.Select(item => item.Trigger).Take(Math.Max(0, TopK - selected.Count)));
        return (selected, false);
    }

    // Обученное слагаемое для одного кандидата каталога: оценку получает только тот
    // навык, которого назвала обученная память; всем остальным достаётся ноль.
    private static double LearnedScoreFor(LearnedSkillRouteSignal? learnedSignal, SkillTriggerLibrary.SkillTrigger trigger)
    {
        // Без сигнала обученного слагаемого нет и ключ равен прежней базовой мере.
        if (learnedSignal is null)
            return 0.0;

        // Названный навык узнаётся по точному имени: похожее имя другим навыком не является.
        return string.Equals(trigger.SkillName, learnedSignal.SkillId, StringComparison.Ordinal)
            ? learnedSignal.Score
            : 0.0;
    }

    // Отклоняет некорректный обученный сигнал на границе выбора навыка.
    // Разбор тот же, что у привязки к выбранной конфигурации, поэтому обычный
    // потребитель и прямой вызов не могут разойтись в том, какой сигнал законен.
    private static void RejectInvalidLearnedSignal(
        LearnedSkillRouteSignal? learnedSignal,
        SkillRouteConfiguration? routeConfiguration)
    {
        // Отсутствующий сигнал законен: проверять нечего.
        if (learnedSignal is null)
            return;

        // Единый разбор возвращает код первой найденной причины отказа.
        var refusalCode = LearnedSkillRouteSignalBinder.Inspect(learnedSignal, routeConfiguration);

        // Законный сигнал проходит дальше без изменения поведения.
        if (refusalCode is null)
            return;

        // Человеческое объяснение причины готовится один раз для текста исключения.
        var explanation = LearnedSkillRouteSignalBinder.Describe(refusalCode, learnedSignal, routeConfiguration);

        // Бессмысленная оценка — это ошибка диапазона аргумента, и она называется именно так.
        if (string.Equals(refusalCode, LearnedSkillRouteSignalBinder.ScoreOutOfRangeCode, StringComparison.Ordinal))
            throw new ArgumentOutOfRangeException(nameof(learnedSignal), learnedSignal.Score, explanation);

        // Отсутствующая конфигурация — дефект именно того аргумента, который её несёт.
        if (string.Equals(refusalCode, LearnedSkillRouteSignalBinder.ConfigurationMissingCode, StringComparison.Ordinal))
            throw new ArgumentException(explanation, nameof(routeConfiguration));

        // Неизвестный навык и чужая модель или тема — дефект самого сигнала.
        throw new ArgumentException(explanation, nameof(learnedSignal));
    }

    public double ScoreForKnn(string taskDescription, string skillCondition)
    {
        var queryTags = FamilyCollapser.CollapseString(taskDescription ?? string.Empty).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var skillTags = FamilyCollapser.CollapseString(skillCondition ?? string.Empty).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var score = JaccardSim(queryTags, skillTags);
        return score >= ConfidenceThreshold ? score : 0.0;
    }

    private static double JaccardSim(HashSet<string> a, HashSet<string> b)
    {
        if (a.Count == 0 && b.Count == 0)
            return 1.0;
        var intersection = a.Count(b.Contains);
        var union = a.Count + b.Count - intersection;
        return union == 0 ? 0.0 : (double)intersection / union;
    }

    private sealed record RankedSkill(SkillTriggerLibrary.SkillTrigger Trigger, double Score);
}

// Static skill trigger catalogue promoted from the E38V2 experiment.
public static class SkillTriggerLibrary
{
    public const int GOVERNANCE = 0;
    public const int CODE_REVIEW = 1;
    public const int TESTING = 2;
    public const int PLANNING = 3;
    public const int AUTH = 4;
    public const int UI = 5;

    public static readonly string[] CategoryNames = ["GOVERNANCE", "CODE_REVIEW", "TESTING", "PLANNING", "AUTH", "UI"];

    public sealed record SkillTrigger(string Condition, string SkillName, int Category, bool AlwaysOn);

    public static readonly IReadOnlyList<SkillTrigger> All = new SkillTrigger[]
    {
        new("every output without exception", "human-language-gate", GOVERNANCE, true),
        new("every session start reconnaissance before anything", "reconnaissance-gate", GOVERNANCE, true),
        new("any session with declared mode", "response-construction-protocol", GOVERNANCE, true),
        new("any session with user-stated goal", "goal-context-persistence", GOVERNANCE, true),
        new("any session with user assertion about existing state", "claim-as-hypothesis", GOVERNANCE, false),
        new("any architect or plan-review output specificity", "specificity-calibration", GOVERNANCE, false),
        new("any ARCHITECT PLANNER REVIEWER MATERIALIZATION session", "architect-habits-guide", GOVERNANCE, false),
        new("any PLANNING MATERIALIZATION ARCHITECT REVIEW session mode", "session-mode-declaration", GOVERNANCE, false),
        new("any plan review goal delivery completeness first arbiter", "goal-delivery-check", GOVERNANCE, false),
        new("any session referencing design artifacts completeness", "design-artifact-completeness", GOVERNANCE, false),
        new("any tenant-facing work round-trip verification", "mvp-round-trip-verification", GOVERNANCE, false),
        new("any MATERIALIZATION session type", "materialization-session-type", GOVERNANCE, false),
        new("new service ts file in engine flows portability auth SDK imports", "dna-compliance-guard", CODE_REVIEW, false),
        new("after Phase B generates service code before DPO capture all 5 layers", "generated-code-review", CODE_REVIEW, false),
        new("portability or auth violation found fix propagation", "retroactive-development", CODE_REVIEW, false),
        new("auth fix applied to existing service verification category", "self-verification", CODE_REVIEW, false),
        new("new ES document TypeScript service file data connection", "data-connection-classification", CODE_REVIEW, false),
        new("controller generated without UseGuards or Roles auth declaration", "dna-compliance-guard auth checks", CODE_REVIEW, false),
        new("SDK direct import found in service file no SDK outside provider", "retroactive-development SDK fix", CODE_REVIEW, false),
        new("any new spec file in engine flows behavioral assertions Rule 7 auth", "test-integrity", TESTING, false),
        new("flow phase close before portability check state file", "flow-implementation-guide portability gate", TESTING, false),
        new("flow phase close before auth check state file authentication status", "flow-implementation-guide auth gate", TESTING, false),
        new("flow phase close before certification tier check", "flow-implementation-guide cert tier gate", TESTING, false),
        new("phase preflight default check auth infrastructure 4-component", "phase-preflight", TESTING, false),
        new("TEACH-QA R0 produced scope isolation count zero NDJSON coverage", "arbiter-ndjson-requirements", TESTING, false),
        new("Phase A NDJSON fixture produced scope isolation count", "arbiter-ndjson-requirements", TESTING, false),
        new("QA session runs on Phase G H I close protocol completeness", "qa-session-type", TESTING, false),
        new("active promotion distributable flow Phase E lifecycle writes", "flow-lifecycle", PLANNING, false),
        new("distributable flow reaching Phase F without tenant certification", "flow-implementation-guide Phase I cert", PLANNING, false),
        new("authentication deployment prerequisite check targeting Tier C", "flow-lifecycle auth deployment check", PLANNING, false),
        new("any stop with improvement metric measurement protocol", "improvement-measurement", PLANNING, false),
        new("any stop claiming fleet progress ui fleet completion", "ui-fleet-completion-criteria", PLANNING, false),
        new("any stop claiming result output skepticism", "output-skepticism", PLANNING, false),
        new("any multi-phase plan before Phase 1 self-validation", "plan-self-validation", PLANNING, false),
        new("flow prerequisites TIER 1 portability check 5 auth infrastructure", "flow-prerequisites auth check", AUTH, false),
        new("TIER 2 3 renumbered checks portability", "flow-prerequisites portability", AUTH, false),
        new("Phase H mandatory auth decoration checks Rule 7 tests", "flow-implementation-guide Phase H auth", AUTH, false),
        new("Phase I mandatory cert tier Tier C checklist auth deployment", "flow-implementation-guide Phase I", AUTH, false),
        new("session examining or repairing existing React pages UI examination", "flow-ui-examination", UI, false),
        new("first React page for flow impeccable md absent product design context", "product-design-context", UI, false),
        new("any session producing React pages UI UX compliance", "ui-ux-compliance", UI, false),
        new("Phase 7 Step 5 screen craft audit", "screen-craft-audit", UI, false),
        new("N 5 instances of same entity type per-entity examination", "per-entity-examination", UI, false),
        new("any UI UX session before first file work scope inventory", "work-scope-inventory", UI, false),
        new("any PNG examination per-image visual validation", "per-image-validation", UI, false),
        new("fleet level UI UX improvement visual examination round", "visual-examination-round", UI, false),
        new("any stop claiming visual improvement coverage matrix", "coverage-matrix", UI, false),
    }.AsReadOnly();

    public static IReadOnlyList<SkillTrigger> ForCategory(int category)
        => All.Where(trigger => trigger.Category == category || trigger.AlwaysOn).ToArray();

    public static IReadOnlyList<SkillTrigger> AlwaysRequired
        => All.Where(trigger => trigger.AlwaysOn).ToArray();
}
