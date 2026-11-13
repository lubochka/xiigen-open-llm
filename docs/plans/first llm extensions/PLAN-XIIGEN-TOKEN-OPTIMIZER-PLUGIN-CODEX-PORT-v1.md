# Plan — XIIGen Token Optimizer Plugin: Codex Port
# Version: 1.0 — 2026-05-07
# Session type: ARCHITECT
# Companion to: PLAN-XIIGEN-TOKEN-OPTIMIZER-PLUGIN-v1.md (Claude Code версия)
# Цель: портировать максимально возможную часть плагина на Codex

---

## Что можно портировать, что нельзя

Хуковые события в Codex **идентичны** Claude Code:
`SessionStart`, `UserPromptSubmit`, `PreToolUse`, `PostToolUse`, `PermissionRequest`, `Stop`.

Бинарные exe работают на любой платформе — они не зависят от хост-инструмента.
Разница только в структуре манифеста и способе установки.

| Хук | Claude Code | Codex | Статус |
|---|---|---|---|
| Context Injector (`SessionStart`) | ✅ | ✅ | **Полный порт** — STATE.json формат одинаков |
| Skill Selector (`UserPromptSubmit`) | ✅ | ⚠️ | **Частичный** — события одинаковы, путь к скилам разный |
| Orchestration Router (`UserPromptSubmit`) | ✅ | ✅ | **Полный порт** — промпт одинаков |
| Output Validator (`Stop`) | ✅ | ⚠️ | **Частичный** — DNA правила XIIGen-специфичны |
| Task Completion Guard (`Stop` + `UserPromptSubmit`) | ✅ | ✅ | **Полный порт** — логика не зависит от инструмента |

**Итого: 3 полных порта, 2 частичных.**

---

## Что отличается в Codex

### 1. Манифест плагина

```
Claude Code:  .claude-plugin/plugin.json
Codex:        .codex-plugin/plugin.json
```

Содержимое идентично — только имя папки разное.

### 2. Путь к скилам

```
Claude Code:  .claude/skills/**/*.md
Codex:        skills/**/*.md  (в папке плагина или репо)
```

Skill Selector должен знать правильный путь.
Решение: env переменная `SKILLS_DIR` — одна и та же логика, разный путь при запуске.

### 3. Способ инжекции контекста

Claude Code хук возвращает JSON в stdout и Claude Code сам инжектирует.
Codex работает аналогично — хук пишет в stdout, Codex читает и инжектирует.
Протокол передачи одинаков.

### 4. Marketplace

```
Claude Code:  platform.claude.com/plugins/submit  или GitHub
Codex:        $REPO_ROOT/.agents/plugins/marketplace.json
              ~/.agents/plugins/marketplace.json
```

Локальный marketplace в Codex проще для тестирования.

---

## Хук 1 — Orchestration Router: полный порт ✅

Логика полностью переносится. Промпт — это промпт в любом инструменте.
`RouteClassifier.ClassifyByKeywords` не знает откуда пришёл промпт.

**Изменений в коде: ноль.**
Один и тот же `orchestration-router.exe` работает в обоих инструментах.

---

## Хук 2 — Skill Selector: частичный порт ⚠️

### Проблема

В Claude Code скилы лежат в `.claude/skills/`.
В Codex скилы лежат в `skills/` внутри плагина или в корне репо.

### Решение

Уже заложено в дизайне — env переменная `SKILLS_DIR`.
Для Codex она указывает на правильный путь.

**Изменений в коде: ноль.**
Изменение только в конфигурации запуска хука.

```json
// hooks.json для Codex
{
  "event": "UserPromptSubmit",
  "command": "./bin/skill-selector.exe",
  "env": {
    "SKILLS_DIR": "./skills"
  }
}
```

---

## Хук 3 — Context Injector: полный порт ✅

`STATE.json` — это файл XIIGen проекта, не файл инструмента.
Хук ищет его вверх по дереву директорий — это одинаково в любом инструменте.

**Изменений в коде: ноль.**
Один и тот же `context-injector.exe`.

---

## Хук 4 — Output Validator: частичный порт ⚠️

### Что переносится полностью

Проверка наличия Gate B команд в плане — это XIIGen правило, не Claude Code правило.
Проверка на "заявлен результат без доказательства" — универсальна.

### Что не переносится

Специфичные DNA правила XIIGen (ClsService import, no SDK imports outside provider) —
это паттерны NestJS кодовой базы, не зависят от инструмента. Переносятся.

Правила специфичные для Claude Code сессионного протокола
(Stop here and wait for approval маркеры, STATE-Pn.json сохранение) —
в Codex другой сессионный протокол. Эти проверки нужно адаптировать или отключить.

**Решение:** флаг `--mode` при запуске.

```json
// Claude Code
"command": "./bin/output-validator.exe --mode claude-code"

// Codex
"command": "./bin/output-validator.exe --mode codex"
```

В режиме `codex` отключаются Claude Code-специфичные проверки.
DNA и Gate B проверки работают в обоих режимах.

**Изменений в коде: минимальные** — добавить `--mode` флаг и условную логику.

---

## Хук 5 — Task Completion Guard: полный порт ✅

Самый ценный хук. Логика полностью независима от инструмента:
- Захватывает требования из промпта
- Проверяет покрытие при завершении
- Сигнализирует через exit code

`.claude/task-state.json` → в Codex будет `.codex/task-state.json`.
Путь конфигурируется через env `TASK_STATE_PATH`.

**Изменений в коде: ноль** (если env переменная уже заложена).
Если не заложена — одна строка изменения в `TaskState.cs`.

---

## Структура Codex плагина

```
xiigen-token-optimizer/
├── .codex-plugin/
│   └── plugin.json               ← манифест Codex (отличается от Claude Code)
├── hooks/
│   └── hooks.json                ← Codex версия хуков
├── bin/
│   ├── orchestration-router.exe  ← тот же exe
│   ├── skill-selector.exe        ← тот же exe
│   ├── context-injector.exe      ← тот же exe
│   ├── output-validator.exe      ← тот же exe (с --mode codex)
│   └── task-completion-guard.exe ← тот же exe
└── marketplace.json              ← локальный marketplace entry для тестирования
```

---

## .codex-plugin/plugin.json

```json
{
  "name": "xiigen-token-optimizer",
  "version": "1.0.0",
  "description": "Token reduction and task focus hooks for XIIGen development sessions",
  "hooks": "./hooks/hooks.json"
}
```

---

## hooks/hooks.json (Codex версия)

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
      "env": { "SKILLS_DIR": "./skills" },
      "description": "Select top-5 relevant skills",
      "timeout_ms": 400,
      "depends_on": "orchestration-router",
      "on_failure": "continue"
    },
    {
      "event": "UserPromptSubmit",
      "command": "./bin/task-completion-guard.exe --mode capture",
      "env": { "TASK_STATE_PATH": ".codex/task-state.json" },
      "description": "Capture task requirements",
      "timeout_ms": 400,
      "on_failure": "continue"
    },
    {
      "event": "Stop",
      "command": "./bin/output-validator.exe --mode codex",
      "description": "Validate output against DNA rules",
      "timeout_ms": 600,
      "on_failure": "continue"
    },
    {
      "event": "Stop",
      "command": "./bin/task-completion-guard.exe --mode verify",
      "env": { "TASK_STATE_PATH": ".codex/task-state.json" },
      "description": "Verify all task requirements addressed",
      "timeout_ms": 400,
      "on_failure": "continue",
      "block_on_exit_nonzero": true
    }
  ]
}
```

---

## marketplace.json (для локального тестирования)

```json
{
  "plugins": [
    {
      "name": "xiigen-token-optimizer",
      "description": "Token reduction and task focus hooks",
      "path": "./xiigen-token-optimizer"
    }
  ]
}
```

Установка для тестирования:
```bash
# Репо-уровень (все в команде)
cp marketplace.json $REPO_ROOT/.agents/plugins/marketplace.json

# Персональный (только у тебя)
cp marketplace.json ~/.agents/plugins/marketplace.json
```

---

## Минимальные изменения в коде

Всё что нужно добавить в Claude Code версию чтобы она работала и в Codex:

### 1. OutputValidator — добавить `--mode` флаг

```csharp
// 40.HookRunners/OutputValidator/Program.cs
var mode = args.FirstOrDefault(a => a.StartsWith("--mode="))
               ?.Split('=')[1] ?? "claude-code";

var checks = mode == "codex"
    ? DnaRuleChecker.GetUniversalChecks()      // DNA + Gate B
    : DnaRuleChecker.GetAllChecks();           // + Claude Code специфичные
```

**Размер изменения: ~5 строк.**

### 2. TaskCompletionGuard — параметризовать путь к state файлу

```csharp
// 40.HookRunners/Shared/TaskState.cs
public static string GetStatePath()
{
    return Environment.GetEnvironmentVariable("TASK_STATE_PATH")
        ?? ".claude/task-state.json";   // дефолт для Claude Code
}
```

**Размер изменения: ~4 строки.**

### 3. SkillSelector — env переменная SKILLS_DIR уже заложена в дизайне

Ничего не меняется — путь читается из env.

---

## Что не портируется и почему

| Компонент | Причина |
|---|---|
| Claude Code-специфичные проверки в OutputValidator | Stop here and wait for approval маркеры, STATE-Pn.json — это Claude Code протокол |
| Путь `.claude/skills/` как дефолт в SkillSelector | В Codex другой путь — решается через env |
| platform.claude.com marketplace | Это только для Claude Code — у Codex свой marketplace |

---

## Итоговое сравнение

| | Claude Code плагин | Codex плагин |
|---|---|---|
| Exe файлы | те же | те же |
| hooks.json | `.claude-plugin/` | `.codex-plugin/` |
| Хуковые события | SessionStart, UserPromptSubmit, Stop | идентичны |
| SKILLS_DIR | `.claude/skills/` | `./skills/` через env |
| TASK_STATE_PATH | `.claude/task-state.json` | `.codex/task-state.json` через env |
| OutputValidator режим | `--mode claude-code` | `--mode codex` |
| Установка | `--plugin-dir` или marketplace | `marketplace.json` |
| Изменений в C# коде | — | ~9 строк |

---

## Порядок разработки

Codex порт делается **после** Claude Code версии, не параллельно.
Все пять exe уже готовы — нужно только:

1. Добавить `--mode` флаг в OutputValidator (~5 строк)
2. Параметризовать TASK_STATE_PATH в Shared/TaskState.cs (~4 строки)
3. Создать `.codex-plugin/plugin.json`
4. Создать Codex версию `hooks/hooks.json`
5. Создать `marketplace.json` для тестирования

**Итого работы на Codex порт: ~1 час после готовности Claude Code версии.**

---

# Версия плана: 1.0
# Автор: ARCHITECT session 2026-05-07
# Зависит от: PLAN-XIIGEN-TOKEN-OPTIMIZER-PLUGIN-v1.md (должен быть готов первым)
# Следующий шаг: реализация Claude Code версии → потом Codex порт за 1 час
