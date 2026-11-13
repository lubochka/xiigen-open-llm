# Pass P08 — Реальная базовая матрица (15 фаз): заявлено vs реально

Честный фрейм: этот проход СВОДИТ воедино находки P01–P07 и добавляет прямые
замеры по РЕАЛЬНЫМ host-репозиториям `C:\Projects\{claude,cursor,codex}-xiigen-smart-skills`
(существование которых установил P06) — то, чего ни один прошлый проход не
инспектировал на уровне их СОБСТВЕННЫХ закоммиченных артефактов, git-состояния,
транспорта и зрелости сборки. Оценка ведётся от реального кода/артефактов, а не
от самоотчёта пакета. Три уровня: COPIED_DOC_ONLY != BUILT_NOT_PROVEN != PROVEN.

Источник claimed-статуса: `EXTENSIONS-IMPLEMENTATION-STATUS-BOARD.md` (2026-07-08),
`REQUIREMENTS-COVERAGE-MATRIX.md`. Board: Total 15; Done 2; In progress 2;
Partial 8; Not started 1; Needs fix 2; Remaining 13.

## 1. Сводная матрица 15 фаз (claimed vs real)

| # | Фаза (человеч.) | Board claimed | РЕАЛЬНЫЙ статус | Опорное доказательство | Кто нашёл |
|---:|---|---|---|---|---|
| 1 | Package shell + copy-manifest | DONE | DONE (только copy) — файлы пакета есть | `extension development/*` присутствуют | P01 |
| 2 | Russian entry docs | DONE | DONE (только copy), часть копий устарела/mojibake | P02 (ORCHESTRATION-v3 mojibake; REALITY-MAP stale) | P02 |
| 3 | API/dashboard/supporting docs | IN_PROGRESS | BUILT_NOT_PROVEN — dashboard реальный, но гоняет ФЕЙКОВЫЙ `RecordingGateway` | `ApiDashboardWindowInteractiveTests.cs:27` | P04 |
| 4 | 50-pass package review | IN_PROGRESS | MISSING / ABANDONED — 0/50, сессия оборвалась на 1–10 | `...session.md:90761`; ledger 0/50 | P01/P07 |
| 5 | Claude same-scenario | PARTIAL | BUILT_NOT_PROVEN host — репо есть, но unbuilt, нет `test/`/`dist`, phase40 = smoke-маркер (stale Jun3), есть uncommitted | этот проход §2 | P06+**P08** |
| 6 | Cursor same-scenario | PARTIAL | BUILT_NOT_PROVEN — репо собран+`test/`, но row12-harness BLOCKED, token-delta 0 синтетика, untracked runtime-03 | P06; этот проход §2 | P06+**P08** |
| 7 | Codex same-scenario | PARTIAL | BUILT_NOT_PROVEN — репо hand-js+`test/`, без сборки; phase40 3/4 stdout=0 байт, run-4 = shell-smoke exit0 | этот проход §2 | **P08** |
| 8 | Prompt to task | PARTIAL | **STUB** — всегда `not_ready_no_semantic_task` | `PromptToTaskMachine.cs:90`; `README.md:52` | P02 |
| 9 | Task to plan orchestration | PARTIAL | **STUB** — всегда `not-ready-no-orchestration-inference` | `OrchestrationReasoningMachine.cs:109` | P02 |
| 10 | Task/plan to skills | PARTIAL | BUILT_NOT_PROVEN + order-leakage authority (позиция кандидата) | `SkillSelectionMachine.cs:65/:767/:113` | P02/P05 |
| 11 | DPO / local learning | PARTIAL | BUILT_NOT_PROVEN — реальный `DpoTrainer` ОТСОЕДИНЁН; «обучение» = словарь/счётчик | `11.DPO/DpoTrainer.cs`; `models/*/manifest.json semanticQualityClaimed:false` | P05 |
| 12 | Checkpoint / locator | PARTIAL | STUB TRANSPORT — `UploadAsync` no-op, locator in-memory | `LocalModelRepository.cs:32-77`; `InMemoryRemoteUnitLocator.cs:8` | P05 |
| 13 | Token savings | NEEDS_FIX | STUB EVIDENCE — delta 0, baseline≡adaptive байт-в-байт | `measurement.json:4-11`; MD5 совпал | P06 |
| 14 | Semantic quality | NEEDS_FIX | PARTIAL/failing — `semanticQualityClaimed=false` везде | `XiigenExtensionApiService.cs:337` и др. | P05/P04 |
| 15 | Real lifecycle | NOT_STARTED | MISSING — нет реального backup/export/import/share/continue/rollback | STATUS-BOARD:35 | P01 |

## 2. Прямые замеры по РЕАЛЬНЫМ host-репозиториям (новое в P08)

Все три целевых репо существуют (`C:\Projects\{claude,cursor,codex}-xiigen-smart-skills`),
это отдельные git-репозитории, НЕ упоминаемые board’ом 2026-07-08.

### 2.1 Транспорт — жёстко зашитый локальный PowerShell к core-репо
Все три entrypoint-файла (`claude src/claude-entrypoint.js`, `codex src/codex-entrypoint.js`,
`cursor dist/extension.js`) содержат жёстко зашитый абсолютный путь
`C:\Projects\llm_mvp_core` и вызывают `tools/xiigen-extension-core-api.ps1`
через `spawnSync` (дочерний процесс PowerShell). Подтверждено grep’ом:
- claude: `C:\\Projects\\llm_mvp_core` + `xiigen-extension-core-api.ps1` (×1);
- codex: `DEFAULT_CORE_ROOT='C:/Projects/llm_mvp_core'`, `DEFAULT_CORE_ENDPOINT=.../tools/xiigen-extension-core-api.ps1`;
- cursor `dist/extension.js`: `C:\\Projects\\llm_mvp_core` (×2) + `xiigen-extension-core-api.ps1` (×2).
`tools/xiigen-extension-core-api.ps1` существует (9825 байт, Jun 26). По P05/P06 это
repo-local **no-send / синтетический** путь. Значит расширения НЕ самодостаточны и
НЕ поставляемы: требуют core-репо на той же машине по абсолютному пути + PowerShell.

### 2.2 Зрелость сборки — асимметрична, claude беднейший
| Репо | dist | `test/` | `*.ts` в src | git last-commit | working-tree |
|---|---|---|---|---|---|
| claude | НЕТ | **НЕТ** | 0 (hand-js) | 2026-06-23 | M README.md, M package.json (uncommitted) |
| cursor | `dist/extension.js` | есть | 1 (`extension.ts`) | 2026-06-28 | M+untracked runtime-03 evidence |
| codex | НЕТ | есть | 0 (hand-js) | 2026-06-18 | clean |
Cursor — единственный с реальной сборкой TS→JS. Claude/codex — рукописный ESM `.js`
без шага компиляции и без `dist`; у claude вдобавок нет `test/` вовсе → наименее зрелый.

### 2.3 Runtime-артефакты host-репо = smoke/marker, устаревшие, пустой stdout
Собственные `phase40-*` артефакты репозиториев (все датированы 2026-06-03,
5 недель до board 2026-07-08):
- codex: `phase40-codex-host-session-run{,-2,-3}.stdout.jsonl` = **0 байт**; только
  `run-4.stdout.jsonl` = 3610 байт, но это Codex-CLI, вызывающий
  `powershell.exe -Command powershell.exe -NoProfile ...` (двойной shell-out);
  `run-4.last-message.txt` = «Shell exit code 0; XIIGEN_CODEX_HOST_SMOKE_DONE» — smoke-маркер;
- claude: `phase40-claude-host-session-run.stdout.log` = «XIIGEN_PHASE40_SMOKE» (UTF-16 маркер), stderr=0;
- нет ни одного prompt→answer round-trip в закоммиченных артефактах репо.
Т.е. и в РЕАЛЬНЫХ репо host-runtime доказан лишь install-smoke + session-smoke маркером,
а не сквозным ответом; артефакты 5-недельной давности.

### 2.4 Board-дисконнект (провенанс базовой матрицы)
Board 2026-07-08 для фаз 5/6/7 указывает evidence ТОЛЬКО «copied/supporting files»
и НИ РАЗУ не ссылается на реальные host-репо `C:\Projects\*-xiigen-smart-skills`,
где лежит настоящий собранный/рукописный код и phase40-артефакты. Реальные репо
дрейфнули после последних коммитов (Jun 18–28) и имеют uncommitted/untracked
изменения, не отражённые board’ом (Jul 8). Базовая матрица пакета мис-локализует
собственный deliverable.

## 3. Консолидированный вывод P08
- Реальный product-DONE = **0/15**; реальный PROVEN = **0/15**. Обе board-«DONE» —
  copy/doc, не продукт (подтверждает P01 honesty).
- Board переоценивает фазы 8 и 9 как PARTIAL, тогда как реальный код — STUB
  (жёсткая not-ready-константа); это claimed>real на уровне самого board.
- Единственный настоящий транспорт трёх расширений — локальный PowerShell-шим к
  core-репо (не поставляемый), а host-runtime доказан smoke-маркером.
- `REQUIREMENTS-COVERAGE-MATRIX.md` сам подтверждает scope = package/status:
  COVERED_FOR_PACKAGE ×8, PREPARED_NOT_RUN ×1, product-ready claims = 0.

## 4. НОВЫЕ дыры этого прохода (не дублируют P01–P07)
См. блок «Pass P08» в `opus-PROJECT-CLOSURE-PLAN.md`. Кратко: (G1) board-дисконнект
от реальных host-репо + невыраженный working-tree дрейф; (G2) собственные phase40
host-репо = smoke/marker, stale Jun3, codex 3/4 stdout пусты; (G3) транспорт всех
трёх = жёстко зашитый локальный PowerShell к core-репо → не самодостаточны/не
поставляемы; (G4) асимметрия зрелости: claude без `dist`/`test/`/`.ts` и uncommitted,
codex без сборки — не паритет с cursor.
