# Pass P18 — Codex: конвейер prompt-to-task → orchestration → skills (детали)

Дата: 2026-07-08. Ассессор: Opus 4.8. Слайс: РЕАЛЬНЫЙ конвейер обработки промпта
именно на Codex-пути (аналог P10 для Claude, P14 для Cursor). Оценка от живого кода
и закоммиченных артефактов, не от copy/status пакета.

## Что проверено (bounded reads)

- `C:\Projects\codex-xiigen-smart-skills\src\codex-entrypoint.js` целиком (545 строк):
  разбор CLI (`parseCliArgs:475-496`, `optionsFromCliArgs:498-526`), построение запроса
  (`createXiigenExtensionRequest:107-143`), метод по умолчанию (`:108`
  `options.method ?? 'prompt-to-task'`), skills-вход (`:136-137`
  `RequestedSkills/AvailableSkills = asArray(...)`), `runCli:528-539`.
- Ядро `43.SmartSkillsDomain\XiigenExtensionApiService.cs`: диспетчер
  `ProcessHostExtensionRequest:88-201`, `BuildMethodSpecificDisplayText:955-975`
  (ветвь `_ =>` для неизвестного метода `:972-973`), `NormalizeMethod:1000-1003`,
  `RequiresSkillSurface:995-998`.
- Контракт `43.SmartSkillsDomain\XiigenExtensionApiContracts.cs`: `MethodNames:67-74`
  (`prompt-to-task/orchestration-plan/skill-selection/feedback/debug-trace`),
  `ValidateRequest:97-163` (метод проверяется ТОЛЬКО на непустоту `:112-115`).
- Поставляемые Codex-поверхности, реально драйвящие конвейер:
  `skills/xiigen-smart-skills/SKILL.md` (smoke = только `--method prompt-to-task`,
  каталог скиллов = `["xiigen-smart-skills"]`), `.codex-plugin/plugin.json`
  (`capabilities:[Interactive,Write]`, `defaultPrompt` из 2 мета-инструкций).
- Закоммиченные прогоны с методами: `reports/TOKEN-03-live-codex-app-server/...`,
  `reports/TOKEN-03-full-hosts-...`, `reports/RUNTIME-04B-codex/...` — все через
  agent-shell `node .\src\codex-entrypoint.js --method X ...` (не через host-поверхность).
- Реальные записанные request/response.json для prompt-to-task / orchestration-plan /
  skill-selection на Codex-пути.

## Ключевые факты (evidence-bound)

1. **Метод пробрасывается сырым и валидируется только на непустоту.** Codex
   `optionsFromCliArgs:501` = `method: parsed.method` (без нормализации), затем
   `createXiigenExtensionRequest:108` = `options.method ?? 'prompt-to-task'`. Ядро
   `ValidateRequest:112-115` проверяет лишь `IsNullOrWhiteSpace(Method)` и НЕ сверяет
   с `MethodNames:67-74`. `NormalizeMethod:1000-1003` только тримит; неизвестный метод
   попадает в `_ =>` ветвь дисплея (`:972-973` «Method {method} produced task, plan,
   and skill evidence.»). Итог: `--method опечатка` из Codex принимается, тихо гоняет
   generic 3-стадийную цепочку, отдаёт «успех» без честной ошибки «unknown method».
   Codex — ЕДИНСТВЕННЫЙ хост, отдающий сырой метод пользователю (Cursor P14 —
   whitelist из 3; Claude P10 — метод не задаётся вовсе).

2. **Метод косметичен — диспетчер всегда одной формы.** `ProcessHostExtensionRequest:117-154`
   безусловно выполняет Translate → Plan → DecideSkills для ЛЮБОГО метода; метод меняет
   только display-текст и требование skill-surface (`RequiresSkillSurface:995-998`). Значит
   `feedback` и `debug-trace` из `MethodNames` тоже сваливаются в ту же task/plan/skills
   цепочку (нет ветки intake обратной связи) — уточняет P11 gap-2 на уровне структуры
   диспетчера: per-method ветвления нет в принципе, есть один фиксированный shape.

3. **Поставляемый skill-selection = вырожденный само-ссылочный каталог.** `SKILL.md`
   smoke передаёт `--available-skills-json '["xiigen-smart-skills"]'` и
   `--requested-skills-json '["xiigen-smart-skills"]'` — ИМЯ САМОГО ПЛАГИНА, не
   реальный индекс скиллов. Результат на TOKEN-03 (`TQ-007.../skill-selection/
   baseline_common_model_only/response.json`) — `SelectedSkills:[]`. Единственный
   прогон с настоящим каталогом — RUNTIME-04B: request `AvailableSkills:["architect",
   "planning","plan-review"]`, `RequestedSkills:["planning"]` → response
   `selectedSkills:["plan-review"]`, но каталог был вручную вписан в smoke-команду.
   Механизма перечисления реального repo/user skills-индекса на Codex-хосте нет —
   вход стадии 3 всегда только из CLI `--available-skills-json:506`.

4. **2 из 3 методов конвейера не покрыты ни одной поставляемой Codex-поверхностью.**
   `SKILL.md` smoke = только `prompt-to-task`; `defaultPrompt` без метода. Методы
   `orchestration-plan` и `skill-selection` встречаются только в agent-shell
   `command_args` внутри `TOKEN-03/RUNTIME-04` (`token03-live-child-result.json`
   `command_args: ["node",".\\src\\codex-entrypoint.js","--method","orchestration-plan",...]`).
   Т.е. мульти-метод конвейера гоняется лишь тем, что LLM-агент вручную вписывает
   `--method` — не через установленный плагин/skill/app-surface.

5. **Манифестный интерактивный вход не подключён к конвейеру.** `.codex-plugin/plugin.json`
   объявляет `capabilities:[Interactive,Write]` и `defaultPrompt`, но `grep
   defaultPrompt|capabilities|plugin.json` по `src/` = 0 — entrypoint НЕ читает манифест;
   вход конвейера только из CLI `--user-prompt-or-task-text:502`. Плюс сами `defaultPrompt`
   («Send this task to XIIGen smart skills.», «Show the XIIGen debug trace for this task.»)
   — мета-инструкции об использовании плагина, не текст задачи; при буквальном
   потреблении prompt-to-task классифицировал бы фразу-инструкцию, а не реальную задачу.

6. **Подтверждения (не новые дыры, для контекста):** на Codex-пути стадия 2 ломается тем
   же `ORCHESTRATION_REASONING_MANIFEST_CHECKPOINT_REF_MISMATCH` (RUNTIME-04B response
   warnings), что и на Claude (P11 gap-6); стадия 1 = `loaded_state_no_matching_learned_pattern`,
   стадия 2 = `not_trained_no_learned_guide_state`, `scenarioRefs: not-ready-no-orchestration-inference`
   (P02 stub-машины); авторитет выбора скиллов — счётчик позиции/контекста
   (`skill_selection_evidence: prompt_score=0, context_score=4, total_score=8`), т.е.
   order-leakage counter P02 подтверждён на Codex-пути. baseline vs adaptive пары семантически
   идентичны (одинаковый ChatDisplayText, оба `SelectedSkills:[]` для prompt-to-task),
   различие md5 только в метаданных — согласуется с P15 (нет независимой переменной).

## НОВЫЕ дыры (P18)

| Компонент | Реальный статус | Evidence file:line | Дыра | Шаги закрытия |
|---|---|---|---|---|
| Валидация метода конвейера на Codex-пути (host raw + core non-empty-only) | PIPELINE DEFECT / метод не валидируется, косметичен | `codex-entrypoint.js:501` `method: parsed.method`, `:108` `?? 'prompt-to-task'`; ядро `XiigenExtensionApiContracts.cs:112-115` (только непустота), `:67-74` `MethodNames` не используется в `ValidateRequest`; `XiigenExtensionApiService.cs:1000-1003` `NormalizeMethod` тримит, `:972-973` `_ =>` default, `:117-154` все 3 стадии при любом методе | Codex — единственный хост, отдающий сырой `--method` пользователю; неизвестный/опечатанный метод принимается и тихо отдаёт generic 3-стадийный ответ без честной ошибки; метод никогда не гейтит стадии (косметичен); `feedback`/`debug-trace` сваливаются в ту же task/plan/skills-цепочку | Валидировать `Method` против `MethodNames` в `ValidateRequest` (честная ошибка на unknown); в Codex-хосте whitelist `--method`; добавить ветки диспетчера для `feedback`/`debug-trace` или отклонять до реализации |
| Поставляемый Codex skill-selection каталог | NO-OP SKILLS STAGE (shipped) / вырожденный само-ссылочный каталог | `SKILL.md` smoke `--available-skills-json '["xiigen-smart-skills"]'`+`--requested-skills-json '["xiigen-smart-skills"]'`; TOKEN-03 `TQ-007-skill-architecture-audit/skill-selection/baseline_common_model_only/response.json` `SelectedSkills:[]`; RUNTIME-04B `positive/request.json` `AvailableSkills:["architect","planning","plan-review"]`→`response.json` `selectedSkills:["plan-review"]`; entrypoint читает каталог только из `--available-skills-json` `codex-entrypoint.js:506`, нет discovery | Отдельно от P10 (Claude пустой каталог) и P14 (Cursor no host selector): здесь каталог НЕ пуст, но само-ссылочен (имя плагина) → стадия 3 «выбирает» из фейкового 1-элементного списка = самого себя, реальный выбор только когда человек руками вписал каталог в CLI; механизма перечисления реального skills-индекса нет | Перечислять реальный skills-индекс в `AvailableSkills` из Codex-хоста; запретить само-ссылочный каталог; доказать skill-selection на реальном каталоге + ablation, что не позиционный |
| Покрытие методов конвейера поставляемой Codex-поверхностью (1/3) | PARTIAL (1/3 shipped) / мульти-метод только через ручной node-shell | `SKILL.md` smoke `--method prompt-to-task` (только он); `.codex-plugin/plugin.json` `defaultPrompt` без метода; `reports/TOKEN-03-live-codex-app-server/20260617-010251/token03-live-child-result.json` command_args `node .\src\codex-entrypoint.js --method orchestration-plan\|skill-selection`; те же методы только в TOKEN-03/RUNTIME-04 | Codex-аналог P10 gap-3 / P14 gap-2, но другой механизм: единственная поставляемая skill-инвокация = prompt-to-task; `orchestration-plan` и `skill-selection` достижимы лишь когда агент руками впишет `--method` — нет поставляемой интерактивной Codex-поверхности, гоняющей эти 2 метода end-to-end | Добавить поставляемые Codex skill/command-пути для orchestration-plan и skill-selection с реальными входами; прогнать каждый end-to-end с настоящей Codex-поверхности; per-method evidence |
| Манифестный интерактивный вход конвейера (`capabilities`/`defaultPrompt`) не подключён | DECLARED-NOT-WIRED / мета-инструкции вместо задачи | `.codex-plugin/plugin.json` `capabilities:[Interactive,Write]`,`defaultPrompt:["Send this task to XIIGen smart skills.","Show the XIIGen debug trace for this task."]`; `grep defaultPrompt\|capabilities\|plugin.json` в `src/` = 0; вход только из CLI `codex-entrypoint.js:502` `--user-prompt-or-task-text` | Интерактивная входная поверхность, обещанная манифестом, не связана с кодом: нет пути, превращающего интерактивный Codex-промпт или объявленный `defaultPrompt` в `UserPromptOrTaskText`; сам `defaultPrompt` мета-инструкционен, так что даже при потреблении в prompt-to-task пошла бы фраза об использовании плагина, а не задача. Отдельно от P17 (host-регистрация/runCli-вывод) — здесь вход конвейера | Подключить интерактивный Codex-промпт к `UserPromptOrTaskText`; заменить мета-инструкции реальными примерами задач или удалить; доказать, что реальный интерактивный промпт драйвит конвейер |

## Вывод P18

Конвейер prompt→task→orchestration→skills на Codex-пути = тонкий CLI-адаптер поверх того
же ядра, что Claude/Cursor. Специфика Codex, ранее не зафиксированная: (1) метод
пробрасывается СЫРЫМ и не валидируется ни хостом, ни ядром (только непустота) — обратная
крайность к Cursor-whitelist; (2) поставляемый skill-selection кормится само-ссылочным
каталогом `["xiigen-smart-skills"]` → всегда пустой выбор, реальный выбор лишь на
руками-вписанном каталоге RUNTIME-04B; (3) 2/3 методов конвейера покрыты только agent-shell
`node --method X`, не поставляемой поверхностью; (4) манифестный интерактивный вход
(`capabilities`/`defaultPrompt`) не подключён к коду и мета-инструкционен. Подтверждены (не
новые) на Codex-пути: order-leakage counter как авторитет выбора (P02), сломанная загрузка
checkpoint OrchestrationReasoning (P11 gap-6), stub-машины (P02), отсутствие независимой
переменной baseline/adaptive (P15).
