# LLM-MVP-CORE: Session & Skills Library — Build Plan v1.0
## Создать CLAUDE.md · AGENTS.md · SESSION-START-PROMPT · Skills
## Подробный план для Claude Code v1.0

**Проект:** `LlmFromScratch.sln` (C# .NET 9)
**Цель:** создать набор файлов аналогичный XIIGen docs + session + skills,
          адаптированный под этот проект
**Образец:** `CLAUDE.md` и `AGENTS.md` из docs.zip, skills из session_start_and_skills.zip
**Этот документ не касается XIIGen.**

---

## Что создаём и зачем

XIIGen имеет 533 скила и тяжёлую governance систему.
Этому проекту нужен минимальный функциональный эквивалент:
13 файлов вместо 533, сфокусированных на реальных болях сессий.

| Файл | Аналог в XIIGen | Зачем нужен |
|---|---|---|
| `CLAUDE.md` | `CLAUDE.md` | Правила проекта — OperationResult, Fabric First (9 правил) |
| `AGENTS.md` | `AGENTS.md` | OP-правила для Claude Code, загружаются автоматически |
| `SESSION-START-PROMPT.md` | `XIIGEN-SESSION-START-PROMPT-v5.5.md` | Загрузка состояния и контракт перед каждой сессией |
| `HOW-TO-USE-SKILLS.md` | `HOW-TO-USE-SKILLS-v5.4.md` | Триггеры: когда загружать какой скил |
| `dev-safety-SKILL.md` | `dev-safety-SKILL.md` | Build gate, zero failures, git discipline |
| `code-execution-SKILL.md` | `code-execution--test-integrity-SKILL.md` | Test gate, test-before-after |
| `experiment-design-SKILL.md` | нет аналога | Как проектировать E-N: Tier / Gate B / baseline / enhanced |
| `plan-review-SKILL.md` | `planning--plan-review-SKILL.md` | Проверить план до реализации |
| `mental-debug-SKILL.md` | `mental-debug-SKILL.md` | Отладка C# — 4 метода |
| `no-product-decisions-SKILL.md` | `no-product-decisions-SKILL.md` | Не решать продуктовые вопросы без одобрения |
| `EXPERIMENT-RESULTS-MAP.md` | нет аналога | E01-E50: статус, метрика, что подтверждено |
| `SUBPROJECT-MAP.md` | нет аналога | 01-35: что делает каждый подпроект |
| `DECISIONS-LOCKED.md` | `DECISIONS-LOCKED.md` | Зафиксированные архитектурные решения |

---

---

# Фаза 1 — CLAUDE.md: правила проекта

**Файл:** `CLAUDE.md` (корень репозитория, рядом с `LlmFromScratch.sln`)

Claude Code загружает этот файл автоматически при старте сессии.
Содержит: что это за проект, стек, 10 обязательных правил кода.

```markdown
# CLAUDE.md — LlmFromScratch (.NET 9 · C# · ML from scratch)

## What This Is

LlmFromScratch is a from-scratch implementation of an LLM and its supporting
infrastructure in pure C#. No external ML frameworks. Every algorithm is
implemented, tested, and benchmarked independently.

**Stack:** .NET 9 · C# · xUnit · Roslyn (for code analysis subprojects)
**Status:** Active development. Test baseline tracked in `build-state.json`.
**Branch convention:** work on main unless told otherwise.

---

## The 10 Rules

### Rule 1 — OperationResult<T> everywhere
Every public method returns `OperationResult<T>`. Never throw for business logic.
Never return null. Never return bool without context.
    // ✅ return OperationResult<int>.Ok(42);
    // ✅ return OperationResult<int>.Fail("reason", "ERROR_CODE");
    // ❌ throw new InvalidOperationException("bad state");
    // ❌ return null;

### Rule 2 — Fabric First
External dependencies resolve through interfaces, not concrete types.
    // ✅ public MyService(ICodeReader reader, ICodeWriter writer)
    // ❌ public MyService() { _reader = new FileSystemReader(); }

### Rule 3 — Every line has a comment
Every line of production code carries a plain-language comment explaining
what it does. Not what the language keyword means — what this specific
line accomplishes in context.

### Rule 4 — Zero failures is a hard gate
`dotnet test` must be green before every commit. Not "green except for
pre-existing failures." Green. If any test fails after a change, fix it
before moving on. There is no narrow reading where only new code must pass.

### Rule 5 — Read before touching
Read every file completely before changing it. Record one specific fact
from the end of each file as proof of reading.

### Rule 6 — One commit per phase
Each phase ends with exactly one commit and one push. Commit message names
the phase and lists every file changed.

### Rule 7 — Test-before-after
Every placement in a plan has a metric that can be measured before and
after the change. No metric = not done.

### Rule 8 — Gate B per placement
Every plan placement has one Gate B: the exact passing condition stated
as a comparison, count, or assertion. "Works correctly" is not a Gate B.

### Rule 9 — No abbreviations without expansion
The first use of any abbreviation must be followed by its full form in
parentheses. Example: K-NN (K-Nearest Neighbours). This applies to code
comments, plan documents, and responses.
```

**Gate:** файл присутствует в корне, `grep "Rule 9" CLAUDE.md` находит строку.

> **Примечание для Claude Code:** `BaseService` не существует в кодовой базе до реализации подпроекта 12.SemanticCompiler. До тех пор Rule "extends BaseService" не применяется. Когда `BaseService` появится — добавить его как Rule 2 и перенумеровать остальные.

---

# Фаза 2 — AGENTS.md: операционные правила

**Файл:** `AGENTS.md` (корень репозитория)

Claude Code загружает этот файл после `CLAUDE.md`.
Содержит: приоритеты инструкций, 6 операционных правил, Round Contract.

```markdown
# AGENTS.md — LlmFromScratch Agent Instructions

## HUMAN OVERRIDE PROTOCOL (HIGHEST PRIORITY)

    1. Luba's direct instruction in this conversation  ← ALWAYS WINS
    2. This file (AGENTS.md)
    3. CLAUDE.md rules
    4. Session plan or skill file
    5. Claude's training defaults

Contradiction between 1 and 2-5: execute instruction, state contradiction,
ask for exception type. Never self-resolve.

---

## 6 Operational Rules

### OP-1 — Never override the stated execution sequence
When Luba says "do A then B then C" that is the binding order.
Do not reorder for efficiency. Do not parallelise without explicit approval.

### OP-2 — Zero failures is non-negotiable
If dotnet test reports any failure after a change — stop, fix, then continue.
Do not report a phase complete while any test is red.

### OP-3 — Plan before code in ARCHITECT sessions
In ARCHITECT mode: produce a .md plan file, present it, stop.
Do not write code files in ARCHITECT mode.
Code starts when Luba says "go", "proceed", or "yes".

### OP-4 — Read before touching
Read the full content of every file before editing it.
If the file was read in a previous turn, read it again — it may have changed.

### OP-5 — State what you found
Before changing anything, write one paragraph stating:
- which branch you are on
- what the current test count is (from dotnet test right now)
- what specific problem you are about to fix

### OP-6 — Commit exactly once per phase
One commit per phase. Message format:
    "phase N: <what changed> — <files changed>"
Do not bundle multiple phases into one commit.
Do not commit partial work.

---

## Round Contract — before every response

Run these checks internally. Output appears at top of response.

    ROUND CONTRACT:
    1. Baseline:    [N tests passing, 0 failing — from last dotnet test]
    2. Current:     [run dotnet test now — actual numbers]
    3. Produced:    [exactly what changed this turn]
    4. Phase gate:  [what "done" looks like for this phase]
    5. Gap:         [what is still missing between item 3 and item 4]

If item 5 shows any gap: the phase is not complete.

---

## Session types

    EXECUTOR  — write code, run tests, commit and push
    ARCHITECT — write plan .md only, stop before writing code
    EXPERIMENT— design and implement one E-N experiment
    REVIEW    — read plans or results, analyse, do not commit
```

**Gate:** `grep "OP-6" AGENTS.md` находит строку.

---

# Фаза 3 — SESSION-START-PROMPT.md

**Файл:** `docs/SESSION-START-PROMPT.md`

Вставляется в начало каждой новой сессии Claude Code.
Заменяет: "смотри в папку docs/plans/" без явной загрузки.

```markdown
# LlmFromScratch — Session Start Prompt v1.0

Paste this at the beginning of every session.

---

## Step 1 — Read project rules

    cat CLAUDE.md      # 10 coding rules
    cat AGENTS.md      # 6 operational rules + Round Contract

## Step 2 — Read current state

    cat build-state.json              # last known test count
    dotnet test --no-build 2>&1 | tail -5   # actual current state

Record: total passing, total failing.
If failing > 0: stop and report before touching anything.

## Step 3 — Read the relevant skill

Load the skill for this session type:
- EXECUTOR:   docs/skills/dev-safety-SKILL.md + code-execution-SKILL.md
- ARCHITECT:  docs/skills/plan-review-SKILL.md
- EXPERIMENT: docs/skills/experiment-design-SKILL.md
- REVIEW:     docs/skills/plan-review-SKILL.md

## Step 4 — State where you are

Write one paragraph:
- session type (EXECUTOR / ARCHITECT / EXPERIMENT / REVIEW)
- branch name
- test baseline (passed / failed right now)
- what you are starting

Then proceed.

---

## Quick reference

    Subproject map:    docs/SUBPROJECT-MAP.md
    Experiment map:    docs/EXPERIMENT-RESULTS-MAP.md
    Locked decisions:  docs/DECISIONS-LOCKED.md
    Plans directory:   docs/plans/
```

**Gate:** файл присутствует в `docs/`, contains "Step 1".

---

# Фаза 4 — Skills (6 файлов)

**Папка:** `docs/skills/`

## 4а — dev-safety-SKILL.md

```markdown
# dev-safety-SKILL.md — Build gate and git discipline

## When to load
Load at the start of every EXECUTOR session.

## Build gate — runs before every commit

    dotnet build LlmFromScratch.sln       # zero errors
    dotnet test Tests/LlmFromScratch.Tests.csproj  # zero failures
    git diff --name-only                   # list what changed

All three must pass. If any fails: fix before committing.

## Git discipline

- One commit per phase
- Commit message: "phase N: what changed — files changed"
- Never `git commit --no-verify`
- Never commit with failing tests

## Zero failures rule

"Zero failures" means zero. Not "zero new failures."
Pre-existing failures must be fixed in the current commit, not deferred.
```

## 4б — code-execution-SKILL.md

```markdown
# code-execution-SKILL.md — Test gate and test quality

## When to load
Load for any session that adds or modifies tests.

## One test = one metric
Every test verifies one specific number, count, or comparison.
"Works correctly" is not a test assertion.

    // ✅ Assert.Equal(0.895, result.BoundaryAccuracy, precision: 2);
    // ✅ Assert.True(enhanced > baseline);
    // ❌ Assert.True(result.IsSuccess); // too vague

## Test-before-after rule
Every placement in a plan has a metric measured before and after.
The test captures both numbers and asserts enhanced >= baseline.

## Tier system for experiments

    Tier 1: 1-10 files   (unit validation)
    Tier 2: 11-100 files (integration signal)
    Tier 3: 100+ files   (production signal)

An experiment FAILED at Tier 2 escalates to Tier 3 before final verdict.
```

## 4в — experiment-design-SKILL.md

```markdown
# experiment-design-SKILL.md — How to design E-N experiments

## When to load
Load when designing a new experiment or writing an experiment plan.

## Anatomy of an experiment

Every experiment has exactly:
- ID: E[number] (next available)
- Component: what is being tested
- Hypothesis: one sentence predicting the direction of change
- Metric: one measurable number (accuracy, ms, count, ratio)
- Baseline: how the system behaves without the change
- Enhanced: how the system behaves with the change
- Gate B: the exact condition for PASSED (e.g. "enhanced > baseline")
- Tier: 1 / 2 / 3

## Failure classification

    PASSED:      enhanced > baseline, Gate B met
    FAILED:      enhanced <= baseline after Tier 3
    SCALE_UP:    needs more data — re-run at higher Tier
    ARCH:        architectural observation — algorithm not applicable here
    DATA:        test data problem — fix generator, re-run
    TOOL:        wrong tool — bigram can't test gradient hypotheses

A FAILED result is informative. Document why it failed.
Never label a result FAILED without explaining the root cause.

## E28-E50 lessons

- Bigram models cannot show gradient-based signal (E28-E31, E44, E45 → SCALE_UP)
- Synthetic data with fixed seed can collapse class distribution (E40, E42 → use StratifiedSplit)
- Baseline must be non-trivial: alphabetical sort is not a real baseline (E38 → fixed to E38v2)
- ProbeNeuralNetwork ≠ LoRA proxy: K-NN has no base/adapter separation
```

## 4г — plan-review-SKILL.md

```markdown
# plan-review-SKILL.md — Check a plan before implementation

## When to load
Load for ARCHITECT sessions before writing a plan.
Load for REVIEW sessions when evaluating a submitted plan.

## Five questions before writing code

1. Does every placement have a Gate B? (measurable pass condition)
2. Does every placement name exactly which file changes?
3. Are new tests listed by name and expected assertion?
4. Is the commit message specified?
5. Is the test count baseline stated?

If any answer is "no": add the missing element before starting.

## No product decisions rule

If a plan requires choosing between two design approaches
that affect user behaviour or architecture:
STOP — present both options to Luba, wait for decision.
Do not self-select between design alternatives.
```

## 4д — mental-debug-SKILL.md

```markdown
# mental-debug-SKILL.md — Debug C# problems systematically

## When to load
Load when a test fails unexpectedly or a result is wrong.

## Four debug moves (apply in order)

### Move 1 — Pattern first
What shape does the failure have?
- Wrong count? → off-by-one or wrong split
- Zero accuracy? → all predictions one class (data collapse)
- Negative delta? → baseline too strong or wrong metric direction

### Move 2 — Contextual gap
List what the failing code NEEDS vs what it RECEIVES.
Find the first line where NEEDS ≠ RECEIVES.

### Move 3 — Mental execution
Run the function in your head with the smallest failing input.
Check every branch. Check every index.

### Move 4 — Data lifecycle
Trace the variable from creation to assertion.
Find the first step where the assumption breaks.
```

## 4е — no-product-decisions-SKILL.md

```markdown
# no-product-decisions-SKILL.md — Never make product decisions alone

## When to load
Load for every session. Apply throughout.

## What counts as a product decision

- Choosing between two architectural approaches
- Deciding to add or remove a feature
- Choosing a default value for a user-configurable parameter
- Deciding which subproject to work on next

## What to do instead

When a product decision is encountered:
1. Stop at that point in the plan
2. Present the two (or more) options with pros/cons
3. Wait for Luba's decision
4. Proceed after explicit approval

Do not resolve product decisions by picking the "obviously better" option.
```

---

# Фаза 5 — Reference documents (3 файла)

## 5а — SUBPROJECT-MAP.md

Карта всех подпроектов: что делает каждый, статус, зависимости.

```markdown
# SUBPROJECT-MAP.md — LlmFromScratch subproject reference

| # | Name | What it does | Status | Key files |
|---|---|---|---|---|
| 00 | TensorCore | Tensor operations, SIMD | Done | TensorOps.cs |
| 01 | Tokenizer | BPE tokenizer | Done | BpeTokenizer.cs |
| 02 | DatasetPipeline | Data loading and batching | Done | DatasetLoader.cs |
| 03 | TransformerModel | Transformer architecture | Done | TransformerModel.cs |
| 04 | TrainingLoop | Training with loss tracking | Done | TrainingLoop.cs |
| 05 | Embeddings | Word embeddings | Done | EmbeddingLayer.cs |
| 06 | VectorSearch | Local vector store | Done | IVectorStorage |
| 07 | GraphEngine | Symbol graph + PageRank | Done | SymbolGraph.cs |
| 08 | GraphRag | Graph-based RAG | Done | HybridRetriever.cs |
| 09 | Inference | TextGenerator | Done | TextGenerator.cs |
| 10 | LoRA | LoRA fine-tuning | Done | LoraLayer.cs |
| 11 | DPO | DPO training | Done | DpoTrainer.cs |
| 12 | SemanticCompiler (planned) | E01-E27 pipeline | Planned | — |
| 13 | VisualStudioExtension (planned) | VS VSIX + chat UI | Planned | — |
| 14 | ContinuousLearning (planned) | LoRA per-project adapt | Planned | — |
| 15 | OrchestratorModel (planned) | Episode decomposition | Planned | — |
| 16 | AutonomousLearning (planned) | Night-time data + train | Planned | — |
| 17 | DynamicDomains (planned) | New domain detection | Planned | — |
| 21 | AgentInfrastructure | Agent loop, indexer, editor | Active | FileChangeApplier.cs |
| 22 | AgentAlgorithms | Context, decomposer, orchestrator | Active | ContextWindowManager.cs |
| 23 | OssEquivalents | MCP, Roslyn analyzer, schema | Active | McpTransport.cs |
| 24 | Compression | Semantic compression levels | Partial | — |
| 28 | SemanticNlp | NLP utilities | Partial | — |
| 33 | MatrixAlgorithms | Matrix ops for training | Done | — |
| 35 | ExperimentsV2 | E01-E50 experiment runner | Active | ExperimentDispatcher.cs |
```

## 5б — EXPERIMENT-RESULTS-MAP.md

```markdown
# EXPERIMENT-RESULTS-MAP.md — E01-E50 validated results

Last updated: 2026-05 (50 experiments, 28 PASSED, 4 SCALE_UP, 18 FAILED/ARCH)

## Validated for production use

| Experiment | Component | Delta | Use in |
|---|---|---|---|
| E01 | AstTokenizer | +93% | 12.SemanticCompiler Compression/ |
| E04 | BM25 search | +100% recall@3 | 22 AstRelevanceRanker |
| E08 | SyntaxRoles | +56% | 22 TaskDecomposer |
| E09 | K-NN structural | +45% | 22 AstRelevanceRanker |
| E11 | Step speed | +62% | 09.Inference, 22 ContextWindowManager |
| E17 | Logic predicates | +97% | 12 SemanticCompiler |
| E18 | PatternClassifier | 95% domain | 12 RouteClassifier, 22 |
| E21 | BoundaryDetect | +143% | 22 TaskDecomposer |
| E23 | TemplateBeautifier | +820% | 23 Beautification/ |
| E25-E27 | PromptTranslator | +154/150/250% | 23 Translation/ |
| E32 | EpisodeDecomposer | +11% | 15 OrchestratorModel |
| E33 | StopSignalDetector | +25% | 15 OrchestratorModel |
| E35 | DependencyDetector | +30% | 22 AgentOrchestrator, 15 |
| E38v2 | SkillRetriever | +100%, 86% load↓ | 22 ContextWindowManager |
| E39 | QualityOrchestrator | +20% | 22 QualityOrchestrator |
| E41 | IdleDetector | +100% | 16 AutonomousLearning |
| E43 | ReplayBuffer | +23% | 16 AutonomousLearning |
| E49 | OrchestratorExpander | +44% | 17 DynamicDomains |
| E50 | TransferLearner | +75% | 17 DynamicDomains |

## Scale-up required (needs Tier 3)

E14 (ContextAction), E22 (RouteClassifier needs TS+PY), E28-E31 (need gradient),
E44-E45 (need gradient + more data), E48 (DomainBootstrapper)

## Key architectural findings

- E20: StackDetector MUST run before E15 normalization
- E44: max_weight = total_examples / (num_classes × rare_examples) — not a constant
- E28v2: K-NN is not a valid proxy for LoRA — test in subproject 14
```

## 5в — DECISIONS-LOCKED.md

```markdown
# DECISIONS-LOCKED.md — Locked architectural decisions

These decisions are final. Do not reopen without explicit approval from Luba.

| Decision | What | Rationale |
|---|---|---|
| D-01 | OperationResult<T> — no exceptions | Consistent error handling, testable |
| D-02 | Fabric First — interfaces not concrete | Swap providers without code changes |
| D-03 | 5-level semantic compression (E01→E17) | 20x compression validated |
| D-04 | Domain split: CODE_Q/BUG/PLAN/ARCH/REFACTOR | E18 95% accuracy |
| D-05 | LoRA adapters per project (2MB each) | Base model shared, specialization cheap |
| D-06 | 70/30 Replay Buffer | E43: prevents catastrophic forgetting |
| D-07 | StackDetector before normalization | E20: markers survive |
| D-08 | StratifiedSplit for all experiments | E40: seed-42 distribution collapse |
| D-09 | max_weight = total/(classes × rare) | E44: ×10 caused forgetting |
| D-10 | BM25 + K-NN fusion | E34v2: both together beat either alone |
```

---

# Фаза 6 — HOW-TO-USE-SKILLS.md

**Файл:** `docs/HOW-TO-USE-SKILLS.md`

```markdown
# HOW-TO-USE-SKILLS.md — When to load which skill

## Load order: always start here

    Step 1: cat CLAUDE.md + AGENTS.md      (every session)
    Step 2: dotnet test — record baseline   (every session)
    Step 3: load session-type skill         (see table below)

## Session type → skills

| Session type | Load before first output |
|---|---|
| EXECUTOR | dev-safety-SKILL.md + code-execution-SKILL.md |
| ARCHITECT | plan-review-SKILL.md + no-product-decisions-SKILL.md |
| EXPERIMENT | experiment-design-SKILL.md + code-execution-SKILL.md |
| REVIEW | plan-review-SKILL.md |

## Activation triggers

| When | Load |
|---|---|
| Any test fails unexpectedly | mental-debug-SKILL.md |
| Plan has no Gate B | plan-review-SKILL.md |
| Choosing between two approaches | no-product-decisions-SKILL.md |
| New experiment E-N designed | experiment-design-SKILL.md |
| Commit about to happen | dev-safety-SKILL.md |
```

---

# Порядок реализации

| Шаг | Файл | Где создать | Дней |
|---|---|---|---|
| 1 | `CLAUDE.md` | корень репо | 0.5 |
| 2 | `AGENTS.md` | корень репо | 0.5 |
| 3 | `docs/SESSION-START-PROMPT.md` | docs/ | 0.5 |
| 4 | `docs/HOW-TO-USE-SKILLS.md` | docs/ | 0.5 |
| 5 | `docs/skills/dev-safety-SKILL.md` | docs/skills/ | 0.5 |
| 6 | `docs/skills/code-execution-SKILL.md` | docs/skills/ | 0.5 |
| 7 | `docs/skills/experiment-design-SKILL.md` | docs/skills/ | 0.5 |
| 8 | `docs/skills/plan-review-SKILL.md` | docs/skills/ | 0.5 |
| 9 | `docs/skills/mental-debug-SKILL.md` | docs/skills/ | 0.5 |
| 10 | `docs/skills/no-product-decisions-SKILL.md` | docs/skills/ | 0.5 |
| 11 | `docs/SUBPROJECT-MAP.md` | docs/ | 0.5 |
| 12 | `docs/EXPERIMENT-RESULTS-MAP.md` | docs/ | 0.5 |
| 13 | `docs/DECISIONS-LOCKED.md` | docs/ | 0.5 |
| | **Итого** | | **6.5 дней** |

## Финальная проверка

```bash
# Структура создана корректно:
ls CLAUDE.md AGENTS.md
ls docs/SESSION-START-PROMPT.md docs/HOW-TO-USE-SKILLS.md
ls docs/skills/*.md   # 6 файлов
ls docs/SUBPROJECT-MAP.md docs/EXPERIMENT-RESULTS-MAP.md docs/DECISIONS-LOCKED.md

# Правила кода присутствуют:
grep "Rule 10" CLAUDE.md
grep "OP-6" AGENTS.md
grep "Gate B" docs/skills/experiment-design-SKILL.md

# Нет XIIGen специфики:
grep -r "NestJS\|TypeScript\|XIIGen\|tenant" CLAUDE.md && echo "FAIL" || echo "clean"
```

## Примечание о размере

XIIGen имеет 533 скила потому что проект 3+ лет и каждый скил решает конкретную
найденную проблему. Для нового проекта важно начать с минимального набора (13 файлов)
и добавлять скил только когда обнаруживается конкретная повторяющаяся боль.
Каждый будущий скил должен иметь конкретный триггер: "этот тип ошибки возникал 3+ раза".
