# Plan — XIIGen Token Optimizer Plugin
# Version: 1.0 — 2026-05-07
# Session type: ARCHITECT
# Goal: снизить потребление токенов в Claude Code сессиях на ~50-60%
#       через детерминированные хуки, без участия модели

---

## Что это такое

Плагин для Claude Code, который запускает четыре детерминированных хука.
Каждый хук — отдельный exe, скомпилированный из существующего C# кода в
`llm_mvp_core`. Модель не тратит токены на то что хук делает сам.

Пятый компонент — Task Completion Validator — это отдельная идея которая
решает проблему потери фокуса: Claude говорит "готово" когда реально не готово,
или забывает важные пункты задачи. Это тоже `Stop` хук но с другой логикой.

---

## Структура плагина

```
xiigen-token-optimizer/
├── .claude-plugin/
│   └── plugin.json
├── hooks/
│   └── hooks.json
├── bin/
│   ├── orchestration-router.exe     ← хук 1
│   ├── skill-selector.exe           ← хук 2
│   ├── context-injector.exe         ← хук 3
│   ├── output-validator.exe         ← хук 4
│   └── task-completion-guard.exe    ← хук 5 (фокус на задаче)
├── build/
│   └── build-all.sh
└── 40.HookRunners/                  ← новый проект в llm_mvp_core
    ├── Shared/
    ├── OrchestrationRouter/
    ├── SkillSelector/
    ├── ContextInjector/
    ├── OutputValidator/
    └── TaskCompletionGuard/
```

---

## Хук 1 — Orchestration Router

### Что делает

Перехватывает промпт до модели. Классифицирует домен и субфазы.
Инжектирует минимальный контекст оркестрации в системный промпт.
Модель начинает уже зная тип задачи — не тратит токены на разведку.

### Откуда берём код

```
llm_mvp_core/29.SemanticCompiler/Routing/RouteClassifier.cs
  метод: ClassifyByKeywords(string prompt) → Domain

llm_mvp_core/21.AgentInfrastructure/Logical/PromptDomainClassifier.cs
  метод: Classify(string prompt) → PromptDomainClassification

llm_mvp_core/21.AgentInfrastructure/Logical/MultiSubPhaseRouterUnit.cs
  метод: ExecuteAsync → SubPhaseRoutingDecision[]
```

### Новый код

```
40.HookRunners/OrchestrationRouter/Program.cs
  Вход:  env HOOK_PROMPT (текст промпта)
  Вызов: RouteClassifier.ClassifyByKeywords → domain
         MultiSubPhaseRouterUnit → top-3 субфазы
  Выход: JSON в stdout
         { domain: "BUG", subPhases: ["4.1","4.4"], orchestrationHint: "..." }
  Время: ≤300ms
```

### Хук событие

`UserPromptSubmit`

### Экономия

~15-25% токенов — убираем рассуждения модели о типе задачи.

---

## Хук 2 — Skill Selector

### Что делает

По домену и субфазам от хука 1 выбирает 3-5 релевантных скилов.
Читает только frontmatter SKILL.md (100 слов, не тело).
В контекст Claude Code попадают только выбранные файлы.

### Откуда берём код

```
llm_mvp_core/22.AgentAlgorithms/Context/AstRelevanceRanker.cs
  метод: Rank(string query, IReadOnlyList<string> candidates) → RankedResult[]
  использует: BM25 по описаниям скилов

llm_mvp_core/22.AgentAlgorithms/Context/AstPackedContextBuilder.cs
  метод: BuildContext → упакованный контекст для инжекции
```

### Новый код

```
40.HookRunners/SkillSelector/Program.cs
  Вход:  env HOOK_DOMAIN, env HOOK_SUBPHASES (из хука 1)
         env SKILLS_DIR (путь к .claude/skills/)
  Шаги:  1. Читает frontmatter всех SKILL.md (только name + description)
         2. BM25 ранжирование по domain + subPhases как query
         3. Возвращает топ-5
  Выход: JSON в stdout
         { skills: ["path/to/SKILL.md", ...], count: 5 }
  Время: ≤400ms
```

### Хук событие

`UserPromptSubmit` — запускается после хука 1 (depends_on)

### Экономия

~30-40% токенов — вместо 20+ скилов грузятся 3-5.

---

## Хук 3 — Context Injector

### Что делает

При старте сессии читает STATE.json текущего потока.
Формирует снэпшот ≤500 токенов: фаза + активные задачи + последние решения.
Инжектирует как системный контекст. Убирает разведочные запросы в начале.

### Откуда берём код

```
llm_mvp_core/21.AgentInfrastructure/Logical/LogicalContracts.cs
  тип: LogicalContextPacket — структура контекстного снэпшота

Новый код читает STATE.json из xiigen репо напрямую.
STATE.json путь: ищется вверх по дереву от текущей директории.
```

### Новый код

```
40.HookRunners/ContextInjector/Program.cs
  Вход:  текущая рабочая директория
  Шаги:  1. Ищет STATE.json вверх по дереву (max 5 уровней)
         2. Читает: current_session, flow_id, last 3 decisions, active blockers
         3. Формирует снэпшот строго ≤500 токенов
         4. Если STATE.json не найден — выходит с кодом 0, ничего не инжектирует
  Выход: JSON в stdout
         { context: "Current: Phase B, Flow-35. Last: ...", tokenCount: 487 }
  Время: ≤500ms
```

### Хук событие

`SessionStart`

### Экономия

~20% токенов — убираем разведочные запросы в начале каждой сессии.

---

## Хук 4 — Output Validator

### Что делает

После ответа модели проверяет его по DNA правилам проекта.
Если нарушение — инжектирует флаг в следующий промпт автоматически.
Предотвращает дорогостоящие retry циклы.

### Откуда берём код

```
project knowledge: code-execution--dna-compliance-guard-SKILL.md
  Правила: portability check 1-5, authentication check 1-3

project knowledge: agent-constitution--rules-session-completeness.md
  Правила: что считается завершённым, что не считается
```

### Новый код

```
40.HookRunners/OutputValidator/Program.cs
  Вход:  env HOOK_OUTPUT (последний ответ модели, текст)
  Проверки:
    1. Если это план — есть ли Gate B команды?
    2. Нет ли запрещённых паттернов (ClsService импорт, прямые SDK вызовы)?
    3. Если заявлен результат — есть ли доказательство (тест, grep)?
  Выход: JSON в stdout
    { valid: true, violations: [], injectionForNextPrompt: "" }
    или
    { valid: false, violations: ["Plan missing Gate B commands"],
      injectionForNextPrompt: "Your plan is missing Gate B verification commands. Add them before stopping." }
  Время: ≤600ms
```

### Хук событие

`Stop`

### Экономия

Непрямая — предотвращает 1-2 retry цикла на сессию (~500-1000 токенов каждый).

---

## Хук 5 — Task Completion Guard (фокус на задаче)

### Проблема которую решает

Claude говорит "готово" или "это всё что нужно" когда:
- Реально выполнил только часть задачи
- Забыл про важные пункты из исходного запроса
- Потерял фокус и ушёл в детали не той части

Это не проблема качества ответа — это проблема потери контекста задачи
на длинных сессиях. Хук держит Claude привязанным к исходной задаче.

### Механизм

Хук работает в два шага:

**Шаг 1 — При получении задачи (`UserPromptSubmit`):**
Если промпт является новой задачей (не вопросом, не продолжением) —
хук извлекает структуру задачи: список требований, критерии завершения.
Сохраняет в `.claude/task-state.json`.

**Шаг 2 — При завершении (`Stop`):**
Читает `.claude/task-state.json`.
Сравнивает список требований с тем что Claude сделал.
Если покрытие < 100% — Claude Code получает сигнал продолжить,
а не отображает финальный ответ.

### Откуда берём код

```
project knowledge: agent-constitution--rules-session-completeness.md
  Логика: что считается завершённым — Build passes, Tests pass, Docs synced, STATE saved

project knowledge: code-execution--no-product-decisions-rules-session-completeness-rule.md
  Логика: Incomplete Session Protocol — список что осталось

llm_mvp_core/21.AgentInfrastructure/Logical/PromptRoutingInputCompilerUnit.cs
  Метод: ExtractPredicates — извлечение предикатов из текста задачи
```

### Новый код

```
40.HookRunners/TaskCompletionGuard/Program.cs

Режим 1 — CAPTURE (UserPromptSubmit):
  Вход:  env HOOK_PROMPT
  Шаги:  1. Определяет: это новая задача или продолжение?
            (новая задача = содержит глаголы действия + нет "продолжи"/"также"/"ещё")
         2. Если новая задача — извлекает требования:
            - Императивные конструкции: "сделай X", "добавь Y", "проверь Z"
            - Список пунктов: "1. ... 2. ... 3. ..."
            - Условия: "если ... то ..."
         3. Сохраняет в .claude/task-state.json:
            { taskId, requirements: string[], capturedAt, status: "IN_PROGRESS" }
  Выход: JSON { captured: true, requirementCount: 3 }

Режим 2 — VERIFY (Stop):
  Вход:  env HOOK_OUTPUT (текст ответа Claude)
         читает .claude/task-state.json
  Шаги:  1. Если task-state.json нет или status != "IN_PROGRESS" — выход 0
         2. Проверяет каждое требование из requirements[]
            Для каждого: ищет в HOOK_OUTPUT упоминание или адресацию
         3. Вычисляет coverage = addressed / total
         4. Если coverage < 1.0:
            - Собирает список неадресованных требований
            - Возвращает injectionForNextPrompt с явным списком
            - Выставляет exit code 1 (сигнал Claude Code что нужно продолжить)
         5. Если coverage = 1.0:
            - Обновляет task-state.json { status: "COMPLETE" }
            - Выход 0
  Выход (неполная задача):
    {
      complete: false,
      coverage: 0.67,
      addressed: ["сделай X", "добавь Y"],
      missing: ["проверь Z"],
      injectionForNextPrompt: "Task not complete. Still required:\n- проверь Z\nAddress this before finishing."
    }
  Выход (полная задача):
    { complete: true, coverage: 1.0 }
  Время: ≤400ms
```

### Хук события

- `UserPromptSubmit` — режим CAPTURE (захват требований)
- `Stop` — режим VERIFY (проверка покрытия)

### Как Claude Code реагирует на exit code 1

Claude Code при `Stop` хуке с exit code 1 не отображает финальный ответ
как завершённый. Вместо этого читает `injectionForNextPrompt` из stdout хука
и автоматически добавляет его в следующий ход как системное сообщение.
Claude видит: "Task not complete. Still required: ..." и продолжает.

### Экономия

Непрямая но значительная — предотвращает:
- Повторные запросы "ты забыл про X" (~300-500 токенов каждый)
- Необходимость пересказывать контекст задачи (~200-400 токенов)
- Потерю сессии из-за дрейфа фокуса

---

## hooks.json

```json
{
  "hooks": [
    {
      "event": "SessionStart",
      "command": "./bin/context-injector.exe",
      "description": "Inject session context from STATE.json",
      "timeout_ms": 500,
      "on_failure": "continue"
    },
    {
      "event": "UserPromptSubmit",
      "command": "./bin/orchestration-router.exe",
      "description": "Classify domain and sub-phases",
      "timeout_ms": 300,
      "on_failure": "continue"
    },
    {
      "event": "UserPromptSubmit",
      "command": "./bin/skill-selector.exe",
      "description": "Select top-5 relevant skills",
      "timeout_ms": 400,
      "depends_on": "orchestration-router",
      "on_failure": "continue"
    },
    {
      "event": "UserPromptSubmit",
      "command": "./bin/task-completion-guard.exe --mode capture",
      "description": "Capture task requirements for completion tracking",
      "timeout_ms": 400,
      "on_failure": "continue"
    },
    {
      "event": "Stop",
      "command": "./bin/output-validator.exe",
      "description": "Validate output against DNA rules",
      "timeout_ms": 600,
      "on_failure": "continue"
    },
    {
      "event": "Stop",
      "command": "./bin/task-completion-guard.exe --mode verify",
      "description": "Verify all task requirements addressed",
      "timeout_ms": 400,
      "on_failure": "continue",
      "block_on_exit_nonzero": true
    }
  ]
}
```

Только `task-completion-guard --mode verify` имеет `block_on_exit_nonzero: true`.
Остальные хуки при сбое не блокируют работу Claude Code (`on_failure: continue`).

---

## plugin.json

```json
{
  "name": "xiigen-token-optimizer",
  "version": "1.0.0",
  "description": "Token reduction and task focus hooks for XIIGen development sessions",
  "hooks": "./hooks/hooks.json"
}
```

---

## Новый проект в solution

```
llm_mvp_core/
  40.HookRunners/
    Shared/
      HookEnvironment.cs     ← читает env переменные хуков
      HookOutput.cs          ← общий JSON формат stdout
      TaskState.cs           ← структура .claude/task-state.json
    OrchestrationRouter/
      OrchestrationRouter.csproj
      Program.cs
    SkillSelector/
      SkillSelector.csproj
      Program.cs
      SkillFrontmatterReader.cs   ← читает только YAML frontmatter из SKILL.md
    ContextInjector/
      ContextInjector.csproj
      Program.cs
      StateJsonReader.cs          ← поиск и чтение STATE.json
    OutputValidator/
      OutputValidator.csproj
      Program.cs
      DnaRuleChecker.cs           ← правила из DNA compliance guard
    TaskCompletionGuard/
      TaskCompletionGuard.csproj
      Program.cs
      RequirementExtractor.cs     ← извлечение требований из промпта
      CoverageChecker.cs          ← сравнение требований с ответом
```

### Зависимости каждого проекта

| Проект | Зависит от |
|---|---|
| OrchestrationRouter | 29.SemanticCompiler, 21.AgentInfrastructure |
| SkillSelector | 22.AgentAlgorithms |
| ContextInjector | 21.AgentInfrastructure (только контракты) |
| OutputValidator | Shared только — standalone |
| TaskCompletionGuard | 12.SemanticCompiler (ExtractPredicates), Shared |

---

## Сборка

```bash
# build/build-all.sh
# Требует: .NET 8 SDK, Native AOT toolchain

TARGET_RID="${1:-win-x64}"   # передаётся как аргумент: win-x64 | osx-arm64 | linux-x64

RUNNERS=(
  "OrchestrationRouter:orchestration-router"
  "SkillSelector:skill-selector"
  "ContextInjector:context-injector"
  "OutputValidator:output-validator"
  "TaskCompletionGuard:task-completion-guard"
)

for ENTRY in "${RUNNERS[@]}"; do
  PROJECT="${ENTRY%%:*}"
  BINARY="${ENTRY##*:}"

  dotnet publish 40.HookRunners/$PROJECT \
    -c Release \
    -r $TARGET_RID \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:PublishAot=true \
    -o bin/ \
    -p:AssemblyName=$BINARY

  echo "Built: bin/$BINARY.exe"
done
```

---

## Тестирование хуков

Каждый хук тестируется изолированно через env переменные:

```bash
# Тест хука 1
HOOK_PROMPT="fix the null reference exception in UserService" \
  ./bin/orchestration-router.exe
# Ожидается: { "domain": "BUG", "subPhases": ["4.1","4.4"] }

# Тест хука 5 — capture
HOOK_PROMPT="добавь авторизацию, напиши тесты и обнови документацию" \
  ./bin/task-completion-guard.exe --mode capture
# Ожидается: { "captured": true, "requirementCount": 3 }

# Тест хука 5 — verify (неполный)
HOOK_OUTPUT="Я добавил авторизацию и написал тесты." \
  ./bin/task-completion-guard.exe --mode verify
# Ожидается: { "complete": false, "missing": ["обнови документацию"] }
```

---

## Порядок разработки

| Фаза | Что делаем | Почему этот порядок |
|---|---|---|
| 1 | TaskCompletionGuard | Наибольшая ценность, standalone — не нужны другие проекты |
| 2 | SkillSelector | Вторая по экономии, AstRelevanceRanker готов |
| 3 | ContextInjector | Простейшая логика, самостоятельный |
| 4 | OrchestrationRouter | Зависит от SemanticCompiler — Phase 0 consolidation нужна первой |
| 5 | OutputValidator | Последний — правила DNA нужно сначала формализовать |
| 6 | Сборка и плагин | Упаковка после всех хуков |

---

## Ожидаемая суммарная экономия

| Хук | Экономия токенов |
|---|---|
| Context Injector | ~20% — нет разведочных запросов |
| Orchestration Router | ~15-25% — нет классификации задачи |
| Skill Selector | ~30-40% — 3 скила вместо 20 |
| Output Validator | непрямая — предотвращает retry |
| Task Completion Guard | непрямая — предотвращает потерю фокуса и повторные уточнения |
| **Итого** | **~50-60% на типичную сессию** |

---

# Версия плана: 1.0
# Автор: ARCHITECT session 2026-05-07
# Следующий шаг: детальный план фаз для TaskCompletionGuard (фаза 1)
