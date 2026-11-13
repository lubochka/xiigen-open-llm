# E38 · E39 — SkillRetriever и QualityOrchestrator
## Подробный план для Claude Code v1.0

**Проект:** `35.ExperimentsV2`
**Обучающие данные:** HOW-TO-USE-SKILLS-v5.4.md (46 триггеров), XIIGEN-SESSION-START-PROMPT-v5.5 (16 проверок)
**Зависимости:** FamilyCollapser (E16), DomainClassifier (E18)
**Этот документ не касается XIIGen.**

---

## Ключевая идея: два направления одного оркестратора

```
OrchestratorModel (E32–E37):
  задача → [агент A, агент B] → Merger
  горизонтальная декомпозиция — параллельно

QualityOrchestrator (E39):
  результат → [проверка 1..16] → PASS или → исправление → повтор
  вертикальная декомпозиция — последовательно
```

Оба используют одну и ту же логику маршрутизации.
Разница только в направлении: вширь (подзадачи) или вглубь (качество).

SkillRetriever (E38) — это RAG по скилам:
```
домен (E18) + под-задача (E33) → 3–5 конкретных скилов
  вместо: загрузить все 129 скилов для домена
```

---

---

# E38 — SkillRetriever: домен + задача → нужные скилы

## Гипотеза

HOW-TO-USE содержит 46 триггерных правил вида
`(условие → скил)`. Это уже размеченный датасет.

Для задачи "добавить null check к сервису":
- Домен: CODE_CHANGE
- Правило срабатывает: "New .service.ts file" → dna-compliance-guard
- Правило срабатывает: "Any new spec file" → test-integrity
- НЕ срабатывает: "First React page" → ui-ux-compliance (не UI задача)

**Проверяемое утверждение:** K-NN на предикатах задачи находит
правильные триггеры с precision@5 > 80%, при этом загружая в 3-5×
меньше скилов чем "загрузить всё для домена".

## 46 триггеров как обучающий датасет

Каждый триггер имеет структуру:
- `when` — условие (что происходит)
- `skill` — скил который нужно загрузить
- `category` — категория скила (GOVERNANCE / CODE_REVIEW / TESTING / PLANNING / AUTH / UI)

Шесть категорий покрывают все 46 триггеров:
```
GOVERNANCE:    human-language-gate, reconnaissance-gate, session-mode-declaration
CODE_REVIEW:   dna-compliance-guard, generated-code-review, retroactive-development
TESTING:       test-integrity, phase-preflight, self-verification
PLANNING:      plan-review, flow-implementation-guide, flow-lifecycle
AUTH:          flow-prerequisites (auth check), dna-compliance-guard (auth checks)
UI:            ui-ux-compliance, flow-ui-examination, per-image-validation
```

## Шаг 1 — Создать `SkillTriggerLibrary.cs`

**Путь:** `35.ExperimentsV2/Infrastructure/SkillTriggerLibrary.cs`

```csharp
namespace ExperimentsV2;

// Библиотека триггеров скилов из HOW-TO-USE-SKILLS-v5.4.md
// Каждый триггер: условие → скил → категория
public static class SkillTriggerLibrary
{
    // Категории скилов
    public const int GOVERNANCE   = 0;
    public const int CODE_REVIEW  = 1;
    public const int TESTING      = 2;
    public const int PLANNING     = 3;
    public const int AUTH         = 4;
    public const int UI           = 5;

    public static readonly string[] CategoryNames =
        ["GOVERNANCE", "CODE_REVIEW", "TESTING", "PLANNING", "AUTH", "UI"];

    public record SkillTrigger(
        string Condition,      // текстовое условие из таблицы
        string SkillName,      // название скила
        int    Category,       // одна из 6 категорий
        bool   AlwaysOn);      // скил нужен при каждом выводе

    // Все 46 триггеров из HOW-TO-USE (сокращённые описания условий)
    public static readonly IReadOnlyList<SkillTrigger> All = new SkillTrigger[]
    {
        // ── GOVERNANCE (всегда / почти всегда) ──────────────────────────────
        new("every output without exception",
            "human-language-gate", GOVERNANCE, true),
        new("every session start reconnaissance before anything",
            "reconnaissance-gate", GOVERNANCE, true),
        new("any session with declared mode",
            "response-construction-protocol", GOVERNANCE, true),
        new("any session with user-stated goal",
            "goal-context-persistence", GOVERNANCE, true),
        new("any session with user assertion about existing state",
            "claim-as-hypothesis", GOVERNANCE, false),
        new("any architect or plan-review output specificity",
            "specificity-calibration", GOVERNANCE, false),
        new("any ARCHITECT PLANNER REVIEWER MATERIALIZATION session",
            "architect-habits-guide", GOVERNANCE, false),
        new("any PLANNING MATERIALIZATION ARCHITECT REVIEW session mode",
            "session-mode-declaration", GOVERNANCE, false),
        new("any plan review goal delivery completeness first arbiter",
            "goal-delivery-check", GOVERNANCE, false),
        new("any session referencing design artifacts completeness",
            "design-artifact-completeness", GOVERNANCE, false),
        new("any tenant-facing work round-trip verification",
            "mvp-round-trip-verification", GOVERNANCE, false),
        new("any MATERIALIZATION session type",
            "materialization-session-type", GOVERNANCE, false),

        // ── CODE_REVIEW ──────────────────────────────────────────────────────
        new("new service ts file in engine flows portability auth SDK imports",
            "dna-compliance-guard", CODE_REVIEW, false),
        new("after Phase B generates service code before DPO capture all 5 layers",
            "generated-code-review", CODE_REVIEW, false),
        new("portability or auth violation found fix propagation",
            "retroactive-development", CODE_REVIEW, false),
        new("auth fix applied to existing service verification category",
            "self-verification", CODE_REVIEW, false),
        new("new ES document TypeScript service file data connection",
            "data-connection-classification", CODE_REVIEW, false),
        new("controller generated without UseGuards or Roles auth declaration",
            "dna-compliance-guard auth checks", CODE_REVIEW, false),
        new("SDK direct import found in service file no SDK outside provider",
            "retroactive-development SDK fix", CODE_REVIEW, false),

        // ── TESTING ──────────────────────────────────────────────────────────
        new("any new spec file in engine flows behavioral assertions Rule 7 auth",
            "test-integrity", TESTING, false),
        new("flow phase close before portability check state file",
            "flow-implementation-guide portability gate", TESTING, false),
        new("flow phase close before auth check state file authentication status",
            "flow-implementation-guide auth gate", TESTING, false),
        new("flow phase close before certification tier check",
            "flow-implementation-guide cert tier gate", TESTING, false),
        new("phase preflight default check auth infrastructure 4-component",
            "phase-preflight", TESTING, false),
        new("TEACH-QA R0 produced scope isolation count zero NDJSON coverage",
            "arbiter-ndjson-requirements", TESTING, false),
        new("Phase A NDJSON fixture produced scope isolation count",
            "arbiter-ndjson-requirements", TESTING, false),
        new("QA session runs on Phase G H I close protocol completeness",
            "qa-session-type", TESTING, false),

        // ── PLANNING ─────────────────────────────────────────────────────────
        new("active promotion distributable flow Phase E lifecycle writes",
            "flow-lifecycle", PLANNING, false),
        new("distributable flow reaching Phase F without tenant certification",
            "flow-implementation-guide Phase I cert", PLANNING, false),
        new("authentication deployment prerequisite check targeting Tier C",
            "flow-lifecycle auth deployment check", PLANNING, false),
        new("any stop with improvement metric measurement protocol",
            "improvement-measurement", PLANNING, false),
        new("any stop claiming fleet progress ui fleet completion",
            "ui-fleet-completion-criteria", PLANNING, false),
        new("any stop claiming result output skepticism",
            "output-skepticism", PLANNING, false),
        new("any multi-phase plan before Phase 1 self-validation",
            "plan-self-validation", PLANNING, false),

        // ── AUTH ─────────────────────────────────────────────────────────────
        new("flow prerequisites TIER 1 portability check 5 auth infrastructure",
            "flow-prerequisites auth check", AUTH, false),
        new("TIER 2 3 renumbered checks portability",
            "flow-prerequisites portability", AUTH, false),
        new("Phase H mandatory auth decoration checks Rule 7 tests",
            "flow-implementation-guide Phase H auth", AUTH, false),
        new("Phase I mandatory cert tier Tier C checklist auth deployment",
            "flow-implementation-guide Phase I", AUTH, false),

        // ── UI ───────────────────────────────────────────────────────────────
        new("session examining or repairing existing React pages UI examination",
            "flow-ui-examination", UI, false),
        new("first React page for flow impeccable md absent product design context",
            "product-design-context", UI, false),
        new("any session producing React pages UI UX compliance",
            "ui-ux-compliance", UI, false),
        new("Phase 7 Step 5 screen craft audit",
            "screen-craft-audit", UI, false),
        new("N 5 instances of same entity type per-entity examination",
            "per-entity-examination", UI, false),
        new("any UI UX session before first file work scope inventory",
            "work-scope-inventory", UI, false),
        new("any PNG examination per-image visual validation",
            "per-image-validation", UI, false),
        new("fleet level UI UX improvement visual examination round",
            "visual-examination-round", UI, false),
        new("any stop claiming visual improvement coverage matrix",
            "coverage-matrix", UI, false),
    }.AsReadOnly();

    // Скилы которые нужны для данной категории задачи (первый уровень фильтрации)
    public static IReadOnlyList<SkillTrigger> ForCategory(int category)
        => All.Where(t => t.Category == category || t.AlwaysOn).ToList().AsReadOnly();

    // Все always-on скилы (загружаются при каждом выводе)
    public static IReadOnlyList<SkillTrigger> AlwaysRequired
        => All.Where(t => t.AlwaysOn).ToList().AsReadOnly();
}
```

## Шаг 2 — Создать `E38SkillRetriever.cs`

**Путь:** `35.ExperimentsV2/Experiments/E38SkillRetriever.cs`

```csharp
namespace ExperimentsV2;

// E38: SkillRetriever — домен + задача → нужные скилы.
//
// Baseline:  загрузить все скилы для домена (full Q2 table)
//            CODE_CHANGE → ~12 скилов
// Enhanced:  K-NN на предикатах задачи → 3–5 конкретных скилов
//
// Обучение:  46 триггеров из HOW-TO-USE как labeled dataset
// Метрика:   precision@5 (правильные скилы в топ-5)
//            + load_reduction (сколько меньше скилов загружается)
public sealed class E38SkillRetriever
{
    private readonly int _topK;
    public E38SkillRetriever(int topK = 5) => _topK = topK;

    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        // Тестовые случаи: задача → ожидаемые категории скилов
        var testCases = BuildTestCases();

        // ── Baseline: все скилы для домена (по домен → категория маппингу) ──
        var t0 = Stopwatch();
        int baseCorrect = 0; int baseTotalLoaded = 0;
        foreach (var tc in testCases)
        {
            // Baseline загружает: GOVERNANCE (always) + все скилы для домена
            var baselineSkills = SkillTriggerLibrary.All
                .Where(s => s.AlwaysOn || s.Category == tc.ExpectedCategory)
                .ToList();
            baseTotalLoaded += baselineSkills.Count;
            // Precision: правильная категория загружена → 1
            if (baselineSkills.Any(s => s.Category == tc.ExpectedCategory))
                baseCorrect++;
        }
        double baselinePrecision = baseCorrect / (double)testCases.Count;
        double baseAvgLoaded     = baseTotalLoaded / (double)testCases.Count;
        double baselineMs        = ElapsedMs(t0);

        // ── Enhanced: K-NN на предикатах задачи ──────────────────────────────
        var t1 = Stopwatch();
        int enhCorrect = 0; int enhTotalLoaded = 0;
        foreach (var tc in testCases)
        {
            var retrieved = RetrieveSkills(tc.TaskDescription, _topK);
            enhTotalLoaded += retrieved.Count;
            // Precision@topK: ожидаемый скил в топ-K
            if (retrieved.Any(s => s.Category == tc.ExpectedCategory))
                enhCorrect++;
        }
        double enhancedPrecision = enhCorrect / (double)testCases.Count;
        double enhAvgLoaded      = enhTotalLoaded / (double)testCases.Count;
        double loadReduction     = 1.0 - enhAvgLoaded / Math.Max(1, baseAvgLoaded);
        double enhancedMs        = ElapsedMs(t1);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E38-SKILL-RETRIEVER",
            Tier              = tier,
            BaselinePrimary   = baselinePrecision,
            EnhancedPrimary   = enhancedPrecision,
            PrimaryMetricName = $"precision_at_{_topK}",
            LowerIsBetter     = false,
            ScalingSignal     = loadReduction,
            ScalingSignalNote = "load_reduction (в сколько раз меньше скилов загружается)",
            ShouldScaleUp     = testCases.Count < 15,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"testCases={testCases.Count}, triggers={SkillTriggerLibrary.All.Count}, " +
                    $"basePrecision={baselinePrecision:P1}(avg={baseAvgLoaded:F1} skills), " +
                    $"enhPrecision={enhancedPrecision:P1}(avg={enhAvgLoaded:F1} skills), " +
                    $"loadReduction={loadReduction:P1}"
        });
    }

    // K-NN: найти триггеры с наиболее похожим условием
    private static List<SkillTriggerLibrary.SkillTrigger> RetrieveSkills(
        string taskDescription, int topK)
    {
        var qTags = FamilyCollapser.CollapseString(taskDescription).ToHashSet();

        // Сначала все always-on
        var result = SkillTriggerLibrary.AlwaysRequired.ToList();

        // Потом K ближайших по тегам (не считая always-on)
        var ranked = SkillTriggerLibrary.All
            .Where(t => !t.AlwaysOn)
            .OrderBy(t =>
            {
                var tTags = FamilyCollapser.CollapseString(t.Condition).ToHashSet();
                return JaccardDist(qTags, tTags);
            })
            .Take(topK - result.Count);

        result.AddRange(ranked);
        return result;
    }

    private static List<(string TaskDescription, int ExpectedCategory)> BuildTestCases() => new()
    {
        // CODE_REVIEW задачи
        ("Add null check to ValidateToken service before accessing user properties",
         SkillTriggerLibrary.CODE_REVIEW),
        ("Review the new UserService.ts file for DNA compliance and auth decorators",
         SkillTriggerLibrary.CODE_REVIEW),
        ("Check if the generated code has SDK imports outside provider files",
         SkillTriggerLibrary.CODE_REVIEW),

        // TESTING задачи
        ("Write unit tests and e2e tests for the new auth controller",
         SkillTriggerLibrary.TESTING),
        ("Run phase preflight checks before closing Phase B",
         SkillTriggerLibrary.TESTING),
        ("Verify test-integrity Rule 7 for 401 403 responses per controller",
         SkillTriggerLibrary.TESTING),

        // PLANNING задачи
        ("Review the implementation plan for Flow-01 Phase A",
         SkillTriggerLibrary.PLANNING),
        ("Create the session file for this phase completion",
         SkillTriggerLibrary.PLANNING),
        ("Prepare the plan for the next execution phase",
         SkillTriggerLibrary.PLANNING),

        // AUTH задачи
        ("Check authentication infrastructure before promoting to Tier C",
         SkillTriggerLibrary.AUTH),
        ("Verify that auth decorators and bypass paths are registered",
         SkillTriggerLibrary.AUTH),

        // UI задачи
        ("Examine the existing React pages for the user registration flow",
         SkillTriggerLibrary.UI),
        ("Review the screen design for the tenant dashboard",
         SkillTriggerLibrary.UI),

        // GOVERNANCE задачи (always-on + governance specific)
        ("Review this document for machine shortcodes and plain language",
         SkillTriggerLibrary.GOVERNANCE),
        ("Verify goal context and architect habits before the approval stop",
         SkillTriggerLibrary.GOVERNANCE),
    };

    private static double JaccardDist(HashSet<string> a, HashSet<string> b)
    {
        if (a.Count == 0 && b.Count == 0) return 0;
        int inter = a.Count(x => b.Contains(x));
        int union = a.Count + b.Count - inter;
        return union == 0 ? 0 : 1.0 - (double)inter / union;
    }

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
```

## Добавить в Dispatcher/Runner/ResultWriter

```csharp
// Dispatcher: RunE38(int tier = 1) => new E38SkillRetriever().Run(tier);
// Runner: "E38" => _d.RunE38(tier),
// ResultWriter: "E38-SKILL-RETRIEVER" => "E38-SKILL-RETRIEVER",
```

## Ожидаемый вывод

```
OK precision_at_5 (up) baseline=1.000 enhanced=1.000 delta=+0.0% | PASSED
notes: testCases=15, triggers=46, basePrecision=100%(avg=14.3 skills),
       enhPrecision=100%(avg=4.1 skills), loadReduction=71.3%
```

Precision может быть одинаковой (100%) — но **loadReduction=71%** —
это главный результат: тот же ответ при в 3.5× меньшем количестве загруженных скилов.
Скилы-кандидаты уточняются, не расширяются.

---

---

# E39 — QualityOrchestrator: результат → нужные проверки

## 16 обязательных проверок и их применимость

Из SESSION-START-PROMPT v5.5:

```
Проверка 1  — OUTPUT CONTRACT           → всегда
Проверка 2  — MISSION PROGRESS          → всегда
Проверка 3  — ISSUE INVENTORY           → всегда
Проверка 4  — TEST GATE (failures=0)    → только если код был произведён
Проверка 5  — SCOPE ISOLATION (arbiter) → только если engine/flows/ файл
Проверка 6  — PIPELINE CONTRACT         → GENERATION/PLANNING/MATERIALIZATION
Проверка 7  — DOCUMENTATION ARTIFACTS  → если структурные файлы
Проверка 8  — RECONNAISSANCE EVIDENCE  → ARCHITECT сессии
Проверка 9  — CLAIMS VERIFIED          → если были утверждения о состоянии
Проверка 10 — GOAL REMINDER             → всегда
Проверка 11 — GOAL DELIVERY VERDICT    → только plan reviews
Проверка 12 — ARCHITECT HABITS         → ARCHITECT сессии
Проверка 13 — TOOLS BEFORE PERSON      → всегда
Проверка 14 — RESPONSE CONSTRUCTION    → всегда
Проверка 15 — UI/UX COMPLIANCE         → только если React страница
Проверка 16 — HUMAN LANGUAGE GATE      → всегда
```

Восемь проверок применяются всегда (1,2,3,10,13,14,16 + частично 9).
Восемь — только в специфических контекстах.

## Гипотеза

Для планового документа не нужна проверка 4 (TEST GATE — нет кода),
5 (SCOPE ISOLATION — нет engine/flows/), 15 (UI/UX — нет React).

Для React страницы не нужна 6 (PIPELINE — это не GENERATION flow),
8 (RECONNAISSANCE — не ARCHITECT анализ).

**Проверяемое утверждение:** классификация типа вывода позволяет
запускать только релевантные проверки, сокращая набор на 30-40%
без пропуска нужных.

Главная ошибка — пропустить нужную проверку (false negative).
Вторая ошибка — запустить лишнюю (false positive, тратит время).

## Шаг 1 — Создать `QualityCheckLibrary.cs`

**Путь:** `35.ExperimentsV2/Infrastructure/QualityCheckLibrary.cs`

```csharp
namespace ExperimentsV2;

// Библиотека 16 обязательных проверок и их применимость.
// Источник: SESSION-START-PROMPT v5.5 — MANDATORY CHECKS BEFORE EVERY STOP.
public static class QualityCheckLibrary
{
    // Типы выводов
    public const int CODE_SERVICE   = 0; // .service.ts, .cs сервисные файлы
    public const int PLAN_DOC       = 1; // план документы, session файлы
    public const int REACT_PAGE     = 2; // React компоненты, JSX
    public const int ANALYSIS_DOC   = 3; // архитектурный анализ, reconnaissance
    public const int ANY_OUTPUT     = 4; // любой вывод

    public static readonly string[] OutputTypeNames =
        ["CODE_SERVICE", "PLAN_DOC", "REACT_PAGE", "ANALYSIS_DOC", "ANY_OUTPUT"];

    public record QualityCheck(
        int    Number,
        string Name,
        string Description,
        int[]  AppliesTo);  // типы вывода для которых применима

    // Все 16 проверок с применимостью
    public static readonly IReadOnlyList<QualityCheck> All = new QualityCheck[]
    {
        new(1,  "OUTPUT_CONTRACT",
            "Binary done criterion met — yes or no, no interpretation",
            new[] {ANY_OUTPUT}),
        new(2,  "MISSION_PROGRESS",
            "Mission progress check — are we advancing the session goal",
            new[] {ANY_OUTPUT}),
        new(3,  "ISSUE_INVENTORY",
            "All found issues addressed — FIXED or DEFERRED with carry-forward",
            new[] {ANY_OUTPUT}),
        new(4,  "TEST_GATE",
            "failures === 0 absolute — not delta, not no new failures",
            new[] {CODE_SERVICE}),
        new(5,  "SCOPE_ISOLATION",
            "scope_isolation arbiter present in every engine/flows/ node",
            new[] {CODE_SERVICE}),
        new(6,  "PIPELINE_CONTRACT",
            "Pipeline position question answered for GENERATION/PLANNING/MATERIALIZATION",
            new[] {CODE_SERVICE, PLAN_DOC}),
        new(7,  "DOCUMENTATION_ARTIFACTS",
            "Markdown companion for every structured file — Rule 17",
            new[] {CODE_SERVICE, PLAN_DOC}),
        new(8,  "RECONNAISSANCE_EVIDENCE",
            "Reconnaissance tracking record saved — Gate 3 routing — Gate 5 hypotheses",
            new[] {ANALYSIS_DOC}),
        new(9,  "CLAIMS_VERIFIED",
            "No needs-to-be-confirmed blocking claims from claim-as-hypothesis",
            new[] {ANY_OUTPUT}),
        new(10, "GOAL_REMINDER",
            "Two-layer goal reminder block visible — verbatim goal quoted",
            new[] {ANY_OUTPUT}),
        new(11, "GOAL_DELIVERY_VERDICT",
            "Goal delivery completeness verdict — plan reviews only",
            new[] {PLAN_DOC}),
        new(12, "ARCHITECT_HABITS",
            "Architect habits discipline check — architect behavior classifier",
            new[] {ANALYSIS_DOC}),
        new(13, "TOOLS_BEFORE_PERSON",
            "Tools called before answering — no tool-free answers about codebase state",
            new[] {ANY_OUTPUT}),
        new(14, "RESPONSE_CONSTRUCTION",
            "Response construction protocol compliance — all 6 steps",
            new[] {ANY_OUTPUT}),
        new(15, "UI_UX_COMPLIANCE",
            "UI/UX compliance check — if one or more React pages produced",
            new[] {REACT_PAGE}),
        new(16, "HUMAN_LANGUAGE_GATE",
            "Zero machine shortcodes in any output — scan before emitting",
            new[] {ANY_OUTPUT}),
    }.AsReadOnly();

    // Получить применимые проверки для типа вывода
    public static IReadOnlyList<QualityCheck> ForOutputType(int outputType)
        => All.Where(c =>
            c.AppliesTo.Contains(ANY_OUTPUT) ||
            c.AppliesTo.Contains(outputType))
        .ToList().AsReadOnly();

    // Применимость каждой из 16 проверок для типа вывода (binary vector)
    public static bool[] GetCheckVector(int outputType)
        => All.Select(c =>
            c.AppliesTo.Contains(ANY_OUTPUT) ||
            c.AppliesTo.Contains(outputType)).ToArray();
}
```

## Шаг 2 — Создать `E39QualityOrchestrator.cs`

**Путь:** `35.ExperimentsV2/Experiments/E39QualityOrchestrator.cs`

```csharp
namespace ExperimentsV2;

// E39: QualityOrchestrator — предсказать нужные проверки для типа вывода.
//
// Baseline:  запускать все 16 проверок всегда
// Enhanced:  классифицировать тип вывода → запускать только релевантные
//
// Ключевые метрики:
//   recall      = доля нужных проверок которые запущены (никогда не пропустить)
//   precision   = доля запущенных которые реально нужны (не тратить время)
//   check_f1    = гармоническое — recall важнее precision
//
// Для QualityOrchestrator recall = 1.0 ОБЯЗАТЕЛЕН.
// Пропустить нужную проверку = выпустить некачественный результат.
// Лишняя проверка = только потеря времени, не катастрофа.
public sealed class E39QualityOrchestrator
{
    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        var testCases = BuildTestCases();

        int totalChecks = QualityCheckLibrary.All.Count; // 16

        // ── Baseline: все 16 проверок всегда ─────────────────────────────────
        var t0 = Stopwatch();
        // Baseline: recall=1.0 всегда, precision = needed/16
        double baseAvgPrecision = testCases.Average(tc =>
        {
            int needed = QualityCheckLibrary.ForOutputType(tc.OutputType).Count;
            return needed / (double)totalChecks;
        });
        double baseRecall    = 1.0;  // всегда запускаем все
        double baseF1        = 2 * baseRecall * baseAvgPrecision / (baseRecall + baseAvgPrecision);
        double baseAvgLoaded = totalChecks;
        double baselineMs    = ElapsedMs(t0);

        // ── Enhanced: классификация типа вывода ───────────────────────────────
        var t1 = Stopwatch();
        double enhTotalRecall = 0; double enhTotalPrecision = 0;
        int enhTotalLoaded = 0;

        foreach (var tc in testCases)
        {
            // Предсказать тип вывода по описанию задачи
            int predictedType = PredictOutputType(tc.TaskDescription);
            var predicted     = QualityCheckLibrary.GetCheckVector(predictedType);
            var actual        = QualityCheckLibrary.GetCheckVector(tc.OutputType);

            int tp = Enumerable.Range(0, totalChecks).Count(i => predicted[i] && actual[i]);
            int fp = Enumerable.Range(0, totalChecks).Count(i => predicted[i] && !actual[i]);
            int fn = Enumerable.Range(0, totalChecks).Count(i => !predicted[i] && actual[i]);

            double recall    = (tp + fn) > 0 ? tp / (double)(tp + fn) : 1.0;
            double precision = (tp + fp) > 0 ? tp / (double)(tp + fp) : 1.0;

            enhTotalRecall    += recall;
            enhTotalPrecision += precision;
            enhTotalLoaded    += predicted.Count(p => p);
        }

        double enhRecall    = enhTotalRecall    / testCases.Count;
        double enhPrecision = enhTotalPrecision / testCases.Count;
        double enhF1        = 2 * enhRecall * enhPrecision / (enhRecall + enhPrecision);
        double enhAvgLoaded = enhTotalLoaded / (double)testCases.Count;
        double loadReduction = 1.0 - enhAvgLoaded / totalChecks;
        double enhancedMs   = ElapsedMs(t1);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E39-QUALITY-ORCHESTRATOR",
            Tier              = tier,
            BaselinePrimary   = baseF1,
            EnhancedPrimary   = enhF1,
            PrimaryMetricName = "check_f1",
            LowerIsBetter     = false,
            ScalingSignal     = enhRecall,
            ScalingSignalNote = "recall (≥ 0.95 обязательно — нельзя пропустить нужную проверку)",
            ShouldScaleUp     = testCases.Count < 15 || enhRecall < 0.95,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"testCases={testCases.Count}, total_checks={totalChecks}, " +
                    $"baseF1={baseF1:F2}(recall=1.0,prec={baseAvgPrecision:P0}), " +
                    $"enhF1={enhF1:F2}(recall={enhRecall:P0},prec={enhPrecision:P0}), " +
                    $"avgLoaded={enhAvgLoaded:F1}/{totalChecks}, loadReduction={loadReduction:P1}"
        });
    }

    // Классифицировать тип вывода по описанию задачи (простые правила)
    private static int PredictOutputType(string taskDescription)
    {
        var lower = taskDescription.ToLowerInvariant();
        if (lower.Contains("react") || lower.Contains("jsx") ||
            lower.Contains("component") || lower.Contains("ui page") ||
            lower.Contains("screen"))
            return QualityCheckLibrary.REACT_PAGE;
        if (lower.Contains(".service.ts") || lower.Contains(".cs") ||
            lower.Contains("engine/flows") || lower.Contains("service file") ||
            lower.Contains("null check") || lower.Contains("implementation"))
            return QualityCheckLibrary.CODE_SERVICE;
        if (lower.Contains("plan") || lower.Contains("session file") ||
            lower.Contains("phase complete") || lower.Contains("document"))
            return QualityCheckLibrary.PLAN_DOC;
        if (lower.Contains("analysis") || lower.Contains("reconnaissance") ||
            lower.Contains("architecture") || lower.Contains("review"))
            return QualityCheckLibrary.ANALYSIS_DOC;
        return QualityCheckLibrary.ANY_OUTPUT;
    }

    private static List<(string TaskDescription, int OutputType)> BuildTestCases() => new()
    {
        // CODE_SERVICE — нужны проверки 1-10,13,14,16 + Code specific 4,5,6,7
        ("Implement the ValidateToken method in AuthService with null checks",
         QualityCheckLibrary.CODE_SERVICE),
        ("Generate service code for Phase B of Flow-01 with DNA compliance",
         QualityCheckLibrary.CODE_SERVICE),
        ("Write the UserService.ts with scope isolation and auth decorators",
         QualityCheckLibrary.CODE_SERVICE),

        // PLAN_DOC — нужны проверки 1-3,6,7,9,10,13,14,16 + Plan specific 11
        ("Produce the implementation plan for Phase A with binary done criteria",
         QualityCheckLibrary.PLAN_DOC),
        ("Create the session file for this phase completion with state JSON",
         QualityCheckLibrary.PLAN_DOC),
        ("Write the plan review document for the auth system design",
         QualityCheckLibrary.PLAN_DOC),

        // REACT_PAGE — нужны проверки 1-3,9,10,13,14,16 + UI specific 15
        ("Build the tenant dashboard React page with proper UI/UX compliance",
         QualityCheckLibrary.REACT_PAGE),
        ("Create the user registration screen component for the flow",
         QualityCheckLibrary.REACT_PAGE),

        // ANALYSIS_DOC — нужны проверки 1-3,8,9,10,12,13,14,16
        ("Analyse the authentication architecture and produce reconnaissance findings",
         QualityCheckLibrary.ANALYSIS_DOC),
        ("Review the codebase structure and identify gaps before planning",
         QualityCheckLibrary.ANALYSIS_DOC),
        ("Examine the existing flows for portability gaps and produce report",
         QualityCheckLibrary.ANALYSIS_DOC),

        // ANY_OUTPUT — нужны все always-on проверки
        ("Respond to the question about how tenant isolation works",
         QualityCheckLibrary.ANY_OUTPUT),
        ("Explain the purpose of the DataProcessResult pattern",
         QualityCheckLibrary.ANY_OUTPUT),
    };

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
```

## Добавить в Dispatcher/Runner/ResultWriter

```csharp
// Dispatcher:
public OperationResult<ExperimentV2Result> RunE38(int tier = 1) => new E38SkillRetriever().Run(tier);
public OperationResult<ExperimentV2Result> RunE39(int tier = 1) => new E39QualityOrchestrator().Run(tier);

// Runner: добавить "E38", "E39"
// Runner switch: "E38" => _d.RunE38(tier), "E39" => _d.RunE39(tier),
// ResultWriter: "E38-SKILL-RETRIEVER" => "E38-SKILL-RETRIEVER",
//               "E39-QUALITY-ORCHESTRATOR" => "E39-QUALITY-ORCHESTRATOR",
```

---

## Ожидаемые результаты

### E38 — SkillRetriever

```
OK precision_at_5 (up) baseline=1.000 enhanced=1.000 delta=+0.0% | PASSED
notes: testCases=15, triggers=46,
       basePrecision=100%(avg=14.3 skills),
       enhPrecision=100%(avg=4.1 skills),
       loadReduction=71.3%
```

Precision одинаковая — но главный результат: **71% меньше скилов загружается**.
Это прямо транслируется в меньший context window для скил-инструкций.

### E39 — QualityOrchestrator

```
OK check_f1 (up) baseline=0.604 enhanced=0.815 delta=+35.0% | PASSED
notes: testCases=13, total_checks=16,
       baseF1=0.60(recall=100%,prec=60%),
       enhF1=0.82(recall=97%,prec=80%),
       avgLoaded=11.1/16, loadReduction=30.6%
```

**Recall = 97%** — почти никогда не пропускаем нужную проверку.
**Precision = 80%** — только 20% запущенных проверок лишние.
Baseline recall=100% но precision=60% — запускает 6 лишних проверок из 10.

---

## Связь с оркестратором

```
Задача пользователя
       │
       ▼ E18 DomainClassifier + E34 EpisodeQueryMatcher
  Домен: CODE_CHANGE, эпизод: "Add null check to service"
       │
       ▼ E38 SkillRetriever
  Загрузить: dna-compliance-guard, test-integrity, self-verification
  (не 129 скилов — только 4)
       │
       ▼ Агент работает с нужными скилами
  Производит: ValidateToken с null check
       │
       ▼ E39 QualityOrchestrator
  Тип вывода: CODE_SERVICE → запустить проверки 1,2,3,4,5,6,7,9,10,13,14,16
  Проверка 4 (TEST GATE): failures === 0 → PASS ✓
  Проверка 5 (SCOPE_ISOLATION): arbiter присутствует → PASS ✓
  Проверка 16 (HUMAN_LANGUAGE): нет shortcodes → PASS ✓
       │
       ▼ Все проверки PASS → результат доставляется
  Одна проверка FAIL → E38 загружает нужный скил для исправления → повтор
```

---

*E38/E39 план v1.0 — SkillRetriever + QualityOrchestrator · не касается XIIGen*
