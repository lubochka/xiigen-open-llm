# SESSION PROMPT AND SKILLS ADAPTATION PLAN — LlmFromScratch
## Адаптация XIIGEN-SESSION-START-PROMPT-v5.4 и скилов к проекту LlmFromScratch
## v1.0

---

## Что делаем и зачем

XIIGEN-SESSION-START-PROMPT-v5.4 — мощная система управления сессиями,
но содержит ~200 ссылок на XIIGen-специфичные концепции:
`DataProcessResult`, `MicroserviceBase`, `DNA patterns`, `scope_isolation`,
`Elasticsearch`, `NestJS`, `TypeScript`, `TIER-A/B/C`, `flow-slug`, портируемость, и т.д.

Всё это нерелевантно для LlmFromScratch (.NET 9, C#, from-scratch ML).

**Стратегия: не переписывать — вырезать и заменить.**
Ядро (Round Contract, Human Override, Human Language, Stop Format, Session Types)
остаётся идентичным. Меняются только домен-специфичные части.

**Итог:** три файла:
1. `docs/SESSION-START-PROMPT.md` v2.0 — полная замена текущего v1.0
2. `docs/HOW-TO-USE-SKILLS.md` v2.0 — расширение текущего (не замена)
3. `docs/skills/` — 3 новых скила + 3 обновлённых

---

## Карта изменений

### Что остаётся без изменений (из XIIGEN v5.4)

| Раздел | Почему оставить |
|---|---|
| Human Override Protocol (4 уровня) | Универсален — применим к любому проекту |
| Human Language rule (без shortcodes) | Универсален |
| Round Contract (5 проверок) | Универсален — уже есть в AGENTS.md |
| Mandatory Stop Format | Универсален |
| Behavioral corrections quick-reference | Применимы — goal drift, ignored feedback, etc. |
| Session type classification | Применимо: ARCHITECT / EXECUTOR / REVIEW / EXPERIMENT |
| Session Boot Protocol | Адаптировать структуру, заменить файлы |

### Что убрать полностью (XIIGEN-only)

| Убрать | Причина |
|---|---|
| PRE-FORK GATE | Нет форков, нет tenant-CI, нет GitHub Actions fork flow |
| Flow lifecycle (Phase G/H/I, Tier A/B/C/D) | Нет flow lifecycle — есть subproject lifecycle |
| Auth plan (Phases 1-4, JWT/RBAC) | Нет multi-tenant auth — нет смысла |
| `XIIGEN-DESIGN-ARCHITECT-SESSION-GUIDE` | Нет этого документа в проекте |
| DNA patterns 1-9 | Заменить на 10 Rules из CLAUDE.md |
| `scope_isolation`, `DataProcessResult`, `MicroserviceBase` | Нет этих концепций |
| Elasticsearch / SQS / Redis fabric | Нет этих зависимостей |
| `TRANSLATION-MAP.md` loading | Нет shortcodes в LlmFromScratch |
| NDJSON arbiter requirements | Нет арбитров |
| `flow-slug`, portability checks | Нет flows в этом смысле |

### Что заменить (адаптировать)

| XIIGEN концепция | LlmFromScratch эквивалент |
|---|---|
| `XIIGEN-DESIGN-ARCHITECT-SESSION-GUIDE` | `docs/SUBPROJECT-MAP.md` + `docs/DECISIONS-LOCKED.md` |
| DNA patterns 1-9 | 10 Rules из `CLAUDE.md` |
| `DataProcessResult<T>` | `OperationResult<T>` |
| `MicroserviceBase` | `BaseService` |
| `scope_isolation` arbiter | `NullSessionStorage` pattern |
| TIER-A/B/C/D certification | Subproject completion gate (Gate B) |
| `failures === 0` (NestJS jest) | `dotnet test → 0 failed` |
| `npx tsc --noEmit` | `dotnet build → 0 errors` |
| Flow sessions (GENERATION, QA, MAINTENANCE) | EXECUTOR, EXPERIMENT, REVIEW |
| HOW-TO-USE-SKILLS-v5.4.md (129 skills) | `docs/HOW-TO-USE-SKILLS.md` (6 skills) |
| SKILL-INDEX-v4.4.0 | `docs/HOW-TO-USE-SKILLS.md` (inline) |
| `build-state.json` stale count | `dotnet test` real count (already in v1.0) |

---

## Фаза 1 — Обновить SESSION-START-PROMPT.md (v1.0 → v2.0)

**Файл:** `docs/SESSION-START-PROMPT.md`
**Принцип:** взять структуру XIIGEN v5.4, заменить содержимое.

### Структура v2.0

```markdown
# LlmFromScratch — Session Start Prompt v2.0

---

## HUMAN OVERRIDE PROTOCOL (ALWAYS WINS)
[← взять verbatim из XIIGEN v5.4 — универсален]

---

## HUMAN LANGUAGE — NO SHORTCODES OUTSIDE CODE FENCES
[← взять verbatim из XIIGEN v5.4 — универсален]
Убрать ссылку на TRANSLATION-MAP.md (не нужна, нет shortcodes в LlmFromScratch)

---

## ROUND CONTRACT — Before every response
[← взять verbatim из XIIGEN v5.4 — универсален]
Behavioral corrections quick-reference table — оставить

---

## SESSION BOOT PROTOCOL — fires automatically

Когда Luba открывает сессию, автоматически:

Step 1 — Read project rules:
    cat CLAUDE.md      # 10 Rules, BaseService, OperationResult<T>
    cat AGENTS.md      # Human Override, Round Contract, 6 OP rules

Step 2 — Read current state:
    dotnet test Tests/LlmFromScratch.Tests.csproj --no-build 2>&1 | tail -5
    # Record: N passed, 0 failed — if failed > 0: stop and report

Step 3 — Read reference documents:
    cat docs/SUBPROJECT-MAP.md          # what each subproject does + status
    cat docs/DECISIONS-LOCKED.md        # D-01..D-12 locked decisions
    cat docs/EXPERIMENT-RESULTS-MAP.md  # E01-E50 results + SCALE_UP flags

Step 4 — Load session-type skill (see Session Type Classification below)

Step 5 — State where you are (one paragraph):
    - session type
    - branch name
    - test baseline right now
    - what you are starting

---

## SESSION TYPE CLASSIFICATION

### EXECUTOR — implement a plan
Load: docs/skills/dev-safety-SKILL.md + docs/skills/code-execution-SKILL.md
Gate: every phase ends with dotnet build (0 errors) + dotnet test (0 failed) + commit

### ARCHITECT — design, feasibility, structural thinking
Load: docs/skills/plan-review-SKILL.md + docs/skills/no-product-decisions-SKILL.md
Rule: produce a .md plan file, present it, stop. No code in ARCHITECT mode.
Wait for "go" or "proceed" before writing any code.

### EXPERIMENT — design or run an E-N experiment
Load: docs/skills/experiment-design-SKILL.md + docs/skills/code-execution-SKILL.md
Rule: every experiment has baseline, enhanced, metric, and tier declared.

### REVIEW — evaluate a plan or code output
Load: docs/skills/plan-review-SKILL.md
Rule: check all 5 FC questions before approval.

DEFAULT: every session starts as ARCHITECT unless declared otherwise.

---

## MANDATORY STOP FORMAT

Before every approval wait, produce:

    ROUND CONTRACT:
    1. Baseline:    [N tests passing, 0 failing — from dotnet test]
    2. Current:     [run dotnet test now — actual numbers]
    3. Produced:    [exactly what changed this turn — files, tests, commands]
    4. Phase gate:  [what "done" looks like — exact bash assertion]
    5. Gap:         [what is still missing between item 3 and item 4]

    SESSION GOAL: [verbatim goal from session opening]
    THIS ROUND PRODUCED: [named artifact or decision]
    OUTPUT CONTRACT: MET / PARTIAL / NOT MET
    LAST CORRECTION: [quote if Luba corrected anything, else NONE]

---

## QUICK REFERENCE

    10 Rules:              CLAUDE.md
    Subproject map:        docs/SUBPROJECT-MAP.md
    Locked decisions:      docs/DECISIONS-LOCKED.md
    Experiment results:    docs/EXPERIMENT-RESULTS-MAP.md
    Skills:                docs/HOW-TO-USE-SKILLS.md
    Session plans:         docs/plans/
    Build state:           dotnet test (always live — not build-state.json)

---

## THE 10 RULES — quick ref (full text in CLAUDE.md)

    Rule 1:  OperationResult<T> everywhere — no throw, no null
    Rule 2:  Fabric First — interfaces, not concrete types
    Rule 3:  Every line has a comment
    Rule 4:  Zero failures is a hard gate
    Rule 5:  Read before touching
    Rule 6:  One commit per phase
    Rule 7:  Test-before-after (metric measured before AND after)
    Rule 8:  Gate B per placement (measurable, not "works correctly")
    Rule 9:  No abbreviations without expansion
    Rule 10: No external ML frameworks
```

### Gate B Фазы 1

```bash
# Файл создан и в правильном месте:
cat docs/SESSION-START-PROMPT.md | grep "Session Start Prompt v2.0"
# Ожидание: одна строка

# Нет XIIGEN-специфичных слов:
grep -n "DNA pattern\|MicroserviceBase\|DataProcessResult\|scope_isolation\|TIER-A\|flow-slug\|NestJS\|Elasticsearch\|PRE-FORK GATE" \
  docs/SESSION-START-PROMPT.md
# Ожидание: 0 строк

# Есть ссылки на правильные файлы проекта:
grep -n "CLAUDE.md\|SUBPROJECT-MAP\|DECISIONS-LOCKED\|EXPERIMENT-RESULTS" \
  docs/SESSION-START-PROMPT.md | wc -l
# Ожидание: >= 4 строки
```

---

## Фаза 2 — Обновить HOW-TO-USE-SKILLS.md (v1.0 → v2.0)

**Файл:** `docs/HOW-TO-USE-SKILLS.md`
**Принцип:** расширить текущий (не сломать то что работает), добавить activation triggers.

### Новое содержимое v2.0

```markdown
# HOW-TO-USE-SKILLS.md — When to load which skill
## v2.0: activation triggers + experiment triggers added

---

## Load order: always start here

    Step 1: cat CLAUDE.md + AGENTS.md           (every session)
    Step 2: dotnet test — record baseline        (every session)
    Step 3: load session-type skill (table below)
    Step 4: load activation triggers if applicable

---

## Session type → skills

| Session type | Load before first output |
|---|---|
| EXECUTOR | dev-safety-SKILL.md + code-execution-SKILL.md |
| ARCHITECT | plan-review-SKILL.md + no-product-decisions-SKILL.md |
| EXPERIMENT | experiment-design-SKILL.md + code-execution-SKILL.md |
| REVIEW | plan-review-SKILL.md |

---

## Activation triggers — load when condition is true

| When | Load |
|---|---|
| Any test fails unexpectedly | mental-debug-SKILL.md |
| Plan has no Gate B | plan-review-SKILL.md |
| Choosing between two design approaches | no-product-decisions-SKILL.md |
| New experiment E-N designed | experiment-design-SKILL.md |
| Commit about to happen | dev-safety-SKILL.md |
| New .cs service file created | dev-safety-SKILL.md (Rule 1 + Rule 2 check) |
| Subproject phase completing | code-execution-SKILL.md (Gate B check) |
| SCALE_UP result in experiment | experiment-design-SKILL.md (Tier escalation) |
| D-09 max_weight used anywhere | experiment-design-SKILL.md (formula reminder) |
| ProbeNeuralNetwork used | experiment-design-SKILL.md (vocabSize=22 reminder) |
| E20 pipeline order referenced | experiment-design-SKILL.md (StackDetector first) |
| New subproject 12-17 started | code-execution-SKILL.md + plan-review-SKILL.md |
| ISemanticCompiler injected into new class | dev-safety-SKILL.md (Fabric First check) |

---

## Skill index

| Skill file | What it governs |
|---|---|
| code-execution-SKILL.md | Test gate, tier system, Gate B format |
| dev-safety-SKILL.md | Build gate, commit discipline, Rule 1+2 quick check |
| experiment-design-SKILL.md | Experiment structure, SCALE_UP, locked decisions D-03..D-10 |
| mental-debug-SKILL.md | Test failure triage, 4-step debug protocol |
| no-product-decisions-SKILL.md | When to stop and ask Luba vs self-resolve |
| plan-review-SKILL.md | FC-1..FC-12, Gate B format, 5 questions |
```

### Gate B Фазы 2

```bash
grep -c "activation trigger\|When.*Load\|Activation" docs/HOW-TO-USE-SKILLS.md
# Ожидание: >= 3 строки

grep -n "experiment-design-SKILL\|D-09\|vocabSize\|ProbeNeuralNetwork\|StackDetector" \
  docs/HOW-TO-USE-SKILLS.md
# Ожидание: activation triggers для D-09, vocabSize=22, E20 присутствуют
```

---

## Фаза 3 — Создать 3 новых скила + обновить 3 существующих

### Новый скил 1: `docs/skills/experiment-design-SKILL.md`

```markdown
# experiment-design-SKILL.md — Design and run experiments correctly

## When to load
Load for any EXPERIMENT session. Load when a SCALE_UP result is found.
Load when designing a new E-N experiment from scratch.

## Experiment structure — every experiment must have all five

    ExperimentId:         "E-N-NAME" (unique, from EXPERIMENT-RESULTS-MAP.md)
    Baseline:             what the system does without the change
    Enhanced:             what the system does with the change
    Metric:               one named, measurable number (not "better")
    Tier:                 1 (1-10 files), 2 (11-100), 3 (100+)

## Tier escalation rule
FAILED at Tier 2 does not mean the hypothesis is wrong.
It means: escalate to Tier 3 before final verdict.
SCALE_UP flag in EXPERIMENT-RESULTS-MAP = "needs more data, algorithm correct."

## Three critical rules — always active (from DECISIONS-LOCKED.md)

### D-07 — StackDetector before normalisation (E20)
    CORRECT:   StackDetector → RouteClassifier → SemanticNormalizer → FamilyCollapser → LogicExtractor
    WRONG:     SemanticNormalizer → StackDetector (destroys C#/TS/PY markers)

### D-09 — max_weight is dynamic, never a constant ×10
    maxWeight = totalExamples / (numClasses × rareExamples)
    // Example: 1000 examples, 5 domains, 50 security → maxWeight = 4.0
    // NEVER: const double maxWeight = 10.0;
    // Reason: E44v2 −98% — constant ×10 caused catastrophic forgetting

### D-08 — StratifiedSplit for all experiments
    Use StratifiedSplit, not RandomSplit(seed:42).
    Reason: E40 / E42 failed because seed-42 put all rare classes in val-set.
    StratifiedSplit.cs exists in 35.ExperimentsV2/Infrastructure/.

## ProbeNeuralNetwork rules
    vocabSize MUST be 22 — not 12, not 10.
    LogicExtractor returns predicates 0-21 (22 total).
    Wrong vocabSize → IndexOutOfRangeException on first TrainStep.

## Locked decisions quick reference (D-01..D-12)
Load docs/DECISIONS-LOCKED.md for full table.
D-01: OperationResult<T>  D-02: Fabric First  D-03: 5-level compression
D-04: Domain names (CODE_Q/BUG/PLAN/ARCH/REFACTOR — short form in code)
D-05: LoRA 2MB per project  D-06: 70/30 Replay Buffer  D-07: StackDetector first
D-08: StratifiedSplit  D-09: dynamic max_weight  D-10: BM25+KNN fusion
D-11: No external ML frameworks  D-12: Domain labels — short in code, long in comments
```

### Новый скил 2: `docs/skills/no-product-decisions-SKILL.md` (обновление существующего)

```markdown
# no-product-decisions-SKILL.md — When to stop vs self-resolve

## When to load
Load for ARCHITECT sessions. Load when a plan contains two design alternatives.

## The rule
If a plan requires choosing between two approaches that affect:
- user-facing behaviour
- public API shape
- architectural direction
- locked decision (D-01..D-12)

→ STOP. Present both options. Wait for Luba's decision.
→ Do not self-select. Do not pick the "obviously better" one.

## Exception: purely implementation details
Minor implementation choices (variable names, internal helper structure,
test helper naming) may be self-resolved without stopping.
The test is: "would changing this require updating DECISIONS-LOCKED.md?"
If yes → stop. If no → proceed.

## Three anti-patterns (from behavioral corrections)
1. Declaring a visual regression "correct behaviour" without asking Luba
2. Marking a phase done when Gate B is not met
3. Processing a mid-session file without asking whether to switch tasks
```

### Обновление: `docs/skills/plan-review-SKILL.md` (добавить FC проверки)

```markdown
# plan-review-SKILL.md — Check a plan before implementation
## v2.0: FC-1..FC-12 checks added

## When to load
Load for ARCHITECT sessions before writing a plan.
Load for REVIEW sessions when evaluating a submitted plan.

## The 12 FC checks — run before approving any plan

### FC-1 — Count drift
Every test count appears exactly once as a running total.
877 → 880 → 885. Each number in exactly one place. No contradictions.

### FC-2 — Path errors
Every file path is verified against the actual codebase.
grep -n "class BaseService" path/to/file.cs before writing "extends BaseService".

### FC-3 — Phantom components
Every component mentioned in the plan exists in exactly one phase.
No component appears in two phases. No phase creates a component not in the table.

### FC-4 — Duplicate test numbers
No test count appears twice (except baseline in pre-condition and Round Contract).

### FC-5 — Missing coverage
Every component in the component map has at least one test.
No phase completes without a test that verifies its output.

### FC-6 — Stale numbers
Baseline is read live from dotnet test — not copied from memory.
If a parallel plan ran, recalculate all target counts from the new baseline.

### FC-7 — Phase placement
Every phase has: deliverable + Gate B + commit message.
No phase ends without all three.

### FC-8 — Self-containment
No plan references "see master plan" or "per CLAUDE.md rule X" without inlining the rule.
Plans are self-contained documents.

### FC-9 — Gate B as literal bash
Every Gate B is a bash command that returns a number or string, not English prose.
"Verify it works" is not a Gate B. "Assert.Equal(4, result.Value!.Count)" is a Gate B.

### FC-10 — Type contract propagation
Every type used in the plan is verified to exist with that exact name and signature.
grep -rn "public interface ISemanticCompiler" before using ISemanticCompiler in a plan.

### FC-11 — Overview-detail consistency
The summary table and the phase detail sections must agree on file names and counts.
If the table says +5 tests but the detail shows 4 test methods → fail.

### FC-12 — Principles compliance
Rule 1: every public method returns OperationResult<T>.
Rule 2: every external dependency through an interface (Fabric First).
Rule 3: every BaseService subclass has (ISessionStorage storage) : base(storage).

## The 5 original questions (still required)
1. Does every placement have a Gate B?
2. Does every placement name exactly which file changes?
3. Are new tests listed by name and expected assertion?
4. Is the commit message specified?
5. Is the test count baseline stated?
```

### Обновление: `docs/skills/code-execution-SKILL.md` (добавить subproject gate)

```markdown
# code-execution-SKILL.md — Test gate, tier system, Gate B format
## v2.0: subproject gate + BaseService reminder added

## When to load
Load for any session that adds, modifies, or reviews code or tests.

## Zero failures is a hard gate — Rule 4
dotnet test must report 0 failed before every commit.
Not "only new tests fail." Not "pre-existing failures excluded." Zero.

## One test = one metric (Rule 8)
    // ✅ Assert.Equal(0.895, result.BoundaryAccuracy, precision: 2);
    // ✅ Assert.True(enhanced > baseline, $"expected enhanced > {baseline}, got {enhanced}");
    // ❌ Assert.True(result.IsSuccess);  // too vague — what succeeded?

## Tier system for experiments
    Tier 1: 1-10 files    (unit validation)
    Tier 2: 11-100 files  (integration signal)
    Tier 3: 100+ files    (production signal)
FAILED at Tier 2 → escalate to Tier 3 (load experiment-design-SKILL.md).
SCALE_UP at any tier → needs more data, algorithm is correct.

## Subproject completion gate
Before any subproject phase is marked done, verify:

    dotnet build LlmFromScratch.sln   # zero errors
    dotnet test Tests/LlmFromScratch.Tests.csproj  # zero failures, count >= baseline
    git diff --name-only              # only expected files changed

## BaseService rule — every new service class
    public sealed class MyService : BaseService, IMyInterface
    {
        public MyService(ISessionStorage storage) : base(storage) { }
    }
    // ❌ public sealed class MyService : BaseService  // missing constructor
    // ❌ public sealed class MyService : IMyInterface  // missing BaseService

## OperationResult<T> rule — every public method
    // ✅ public OperationResult<string> GetName() => ...
    // ✅ public Task<OperationResult<bool>> SaveAsync(...) => ...
    // ❌ public string GetName() => ...  // no OperationResult wrapper
    // ❌ public bool Save() => ...       // bool without context
```

### Gate B Фазы 3

```bash
# Все 6 файлов скилов существуют:
ls docs/skills/
# Ожидание: code-execution-SKILL.md, dev-safety-SKILL.md, experiment-design-SKILL.md,
#            mental-debug-SKILL.md, no-product-decisions-SKILL.md, plan-review-SKILL.md

# experiment-design-SKILL.md содержит три критических правила:
grep -n "D-07\|D-09\|D-08\|StackDetector\|max_weight\|StratifiedSplit\|vocabSize" \
  docs/skills/experiment-design-SKILL.md
# Ожидание: >= 6 строк (все три правила + их примеры)

# plan-review-SKILL.md содержит FC-1..FC-12:
grep -c "^### FC-" docs/skills/plan-review-SKILL.md
# Ожидание: 12

# code-execution-SKILL.md содержит BaseService шаблон:
grep -n "BaseService\|ISessionStorage\|: base(storage)" docs/skills/code-execution-SKILL.md
# Ожидание: >= 3 строки
```

---

## Фаза 4 — Удалить устаревшее, проверить целостность

```bash
# Убедиться что старый SESSION-START-PROMPT.md заменён
cat docs/SESSION-START-PROMPT.md | grep "v2.0"
# Ожидание: "Session Start Prompt v2.0"

# Нет XIIGEN слов в docs/
grep -rn "DNA pattern\|MicroserviceBase\|DataProcessResult\|scope_isolation\
\|NestJS\|TypeScript\|Elasticsearch\|TIER-A\|flow-slug\|portability tier\
\|TRANSLATION-MAP\|PRE-FORK GATE" docs/
# Ожидание: 0 строк

# Ссылки в SESSION-START-PROMPT корректны (файлы существуют):
for f in docs/SUBPROJECT-MAP.md docs/DECISIONS-LOCKED.md docs/EXPERIMENT-RESULTS-MAP.md \
  docs/HOW-TO-USE-SKILLS.md CLAUDE.md AGENTS.md; do
  ls "$f" && echo "OK: $f" || echo "MISSING: $f"
done
# Ожидание: все OK

# Тесты не сломались после изменения документов:
dotnet test Tests/LlmFromScratch.Tests.csproj --no-build 2>&1 | tail -3
# Ожидание: N passed, 0 failed (N = та же цифра что до фазы 1)
```

### Коммит Фазы 4

```bash
git add docs/SESSION-START-PROMPT.md docs/HOW-TO-USE-SKILLS.md docs/skills/
git commit -m "phase 4: SESSION-START-PROMPT v2.0, HOW-TO-USE-SKILLS v2.0, 3 skills updated — adapt from XIIGEN to LlmFromScratch"
git push
```

---

## Итог: что изменилось

| Файл | До | После |
|---|---|---|
| `docs/SESSION-START-PROMPT.md` | v1.0 (5 шагов, нет Round Contract, нет Stop Format) | v2.0 (Human Override + Round Contract + Stop Format + Session Types) |
| `docs/HOW-TO-USE-SKILLS.md` | v1.0 (4 типа сессий, 5 activation triggers) | v2.0 (4 типа + 13 activation triggers включая D-09/vocabSize/E20) |
| `docs/skills/experiment-design-SKILL.md` | Нет | Создан (D-07/D-08/D-09, vocabSize=22, StratifiedSplit, tier escalation) |
| `docs/skills/plan-review-SKILL.md` | v1.0 (5 вопросов) | v2.0 (5 вопросов + FC-1..FC-12) |
| `docs/skills/code-execution-SKILL.md` | v1.0 (тесты + тиры) | v2.0 (+ BaseService шаблон + subproject gate) |
| `docs/skills/no-product-decisions-SKILL.md` | Минимальный | v2.0 (когда останавливаться vs самостоятельно решать) |

**Что НЕ добавляется (сознательно):**
- PRE-FORK GATE — нет fork workflow
- TRANSLATION-MAP.md — нет shortcodes в LlmFromScratch
- Auth plan checks — нет multi-tenant auth
- 129 скилов из XIIGEN HOW-TO-USE — 6 скилов достаточно для этого проекта
- TIER-A/B/C certification — subproject Gate B покрывает то же самое

---

## Порядок выполнения в Claude Code

```bash
# Запустить после завершения ORCHESTRATOR-MODEL-SESSION-PLAN-v1.1

# 1. Проверить baseline:
dotnet test Tests/LlmFromScratch.Tests.csproj --no-build 2>&1 | tail -3

# 2. Создать SESSION-START-PROMPT.md v2.0
# 3. Обновить HOW-TO-USE-SKILLS.md v2.0
# 4. Обновить/создать 6 skill файлов
# 5. Запустить Gate B проверки из каждой фазы
# 6. Один коммит: phase 4 (все 4 фазы — документы, не код — не ломают тесты)

dotnet test Tests/LlmFromScratch.Tests.csproj --no-build 2>&1 | tail -3
# Ожидание: тот же счёт что в начале — документы не меняют тесты
```

---

*Если этот план расходится с содержимым текущих файлов в репозитории —
текущие файлы управляют. Прочитать CLAUDE.md и AGENTS.md перед адаптацией.*
