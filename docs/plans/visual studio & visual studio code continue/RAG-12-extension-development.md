# RAG-12 — «extension development» (папка #12/12, САМАЯ ТЕКУЩАЯ — фундамент активной сборки)

Источник: `C:/Users/lubao/Documents/xiigen/llm mvp/extension development` (200 файлов, последний отчёт 2026-07-08).
Назначение RAG: питать план ПРОДОЛЖЕНИЯ расширений Visual Studio и VS Code. Это единственная папка, которая
показывает, ГДЕ РЕАЛЬНО находится сборка прямо сейчас — она уже содержит 50-проходный Opus-аудит именно статуса
расширений (opus-pass-01..50) и свежий статус-пакет 2026-07-08.

Автор RAG: Claude Opus 4.8 (Ultracode), одиночный bounded-агент. Читал и писал сам, без суб-агентов.

> **[BRANCH-STATUS OVERLAY 2026-07-28]** (оверлей C3-R12, Цикл 3 документного обновления; добавка — ничего ниже не удалено и не переписано). Этот файл называет папку «САМОЙ ТЕКУЩЕЙ — фундамент активной сборки». На 2026-07-28 фундамент под ней съехал; авторитетная таблица — `BRANCH-STATUS-CURRENT-2026-07-28.md` в этой же папке.
> 1. **Хост-репозитории исчезли с диска.** `C:\Projects\{claude,cursor,codex}-xiigen-smart-skills`, чьё существование и состояние этот индекс фиксирует по-репозиторно, на 2026-07-28 ОТСУТСТВУЮТ в `C:\Projects` (ни базовых копий, ни worktree). Аудиты хостов в этом файле — исторический срез; повторная проверка возможна только против серверных (GitHub) репозиториев.
> 2. **Worktree `C:\Projects\llm_mvp_core-orchestrator-extensions` больше не существует.** Сегодняшний набор привязанных рабочих копий ядра — 11 записей, все DPO/GPL-тематики. Утверждение «worktree уже созданы — первый шаг M0 выполнен» (транскрипт-2, §10.1), унаследованное планом, на сегодняшней машине ложно. Судьба 331-файловой незакоммиченной extension-дельты, жившей в том worktree, из документов не выводится — требуется отдельный спасательный аудит.
> 3. **Гейтовая арифметика слияний мертва.** Latest-база `codex/chat-gpt-5.5` откатана переписью истории (~19 июля) к `5084f355e`; якорь `37fe98ac0` больше не существует как git-объект. 24 июля `main` (вершина `df379c624`) впитал заявки на слияние PR #38 (`DesignToCode-clean`) и PR #39 (`codex/dpo-implementation`) — записи «в main extension-кода 0» и «чистый fast-forward доступен» устарели. Ветка `orchestrator-extensions` (`b293045bd`, статична с 10 июля) по слиянию «на бумаге» даёт теперь 0 конфликтов в latest и 28 конфликтующих файлов в `main`; `main` больше не строгий предок (общий предок `6624161be`, ветка +41 / main +688). Литерал «Gate M0: merge-tree чисто в codex/chat-gpt-5.5 И main» читать только через аддендум-оверлей OP-67 (пункт «(г)» в §1.1 файла `PLAN-VS-VSCODE-CONTINUE-v1.md`).
> 4. **Волна GPL/AGPL 23–26 июля** (ветка `PrepareToGPL` в ядре = main + 19 коммитов; «Prepare to AGPL» в `xiigen-open-llm`; «prepare to gpl» в `xiigen-mvp`) — новый дистрибутивный контекст, которого статус-пакет 2026-07-08 и 50-проходный аудит не знают. Ветки офлайн-документации `local_wiki_planning`/`claude/sonnet-local-wiki` статичны с мая (переподтверждено 2026-07-28).

---

## Pass 1/10 — инвентарь 200 файлов + ТЕКУЩИЙ статус сборки (2026-07-08) + карта 50-pass аудита

Границы прохода 1: инвентарь всех 200 файлов; глубокое чтение 6 статус-документов + ledger; для «где сборка сейчас»
дополнительно прочитаны опорные Opus-проходы P01 (claimed-vs-package), P03 (real-host-code), P08 (real-baseline-matrix),
P48 (priority-sequencing), P50 (roadmap). Глубокое извлечение выводов каждого из 50 проходов и большого closure-плана —
это проходы 2-9 (см. распределение в конце). Тесты/сборки/git/runtime НЕ запускались (read-only).

---

## 1. Инвентарь 200 файлов (по категориям)

### 1a. Верхнеуровневые статус/навигация (8 файлов — прочитаны глубоко в этом проходе)
| Файл | Роль |
|---|---|
| `CURRENT-EXTENSION-DEVELOPMENT-REPORT-2026-07-08.md` (7 KB) | ГЛАВНЫЙ отчёт статуса пакета: done/partial/needs-fix + честные границы. |
| `EXTENSIONS-IMPLEMENTATION-STATUS-BOARD.md` (3.6 KB) | Доска 15 фаз с human-counts. |
| `REQUIREMENTS-COVERAGE-MATRIX.md` (2.8 KB) | Требования Luba → package evidence/статус. |
| `README-EXTENSION-DEVELOPMENT-STATUS.md` (2.3 KB) | С чего начинать читать пакет. |
| `00-PATTERN-AND-SOURCE-INVENTORY.md` (2.3 KB) | Какие pattern-папки (dpo legal build, design to code) использованы. |
| `PLANS-AND-SUPPORTING-FILES-INVENTORY.md` (32 KB) | Инвентарь всех скопированных планов и supporting-files (для прохода 10). |
| `50-PASS-SUBAGENT-REVIEW-LEDGER.md` (24 KB) | Ledger ревью САМОГО статус-пакета (см. §5 — это НЕ индекс 50 opus-проходов). |
| `copy-manifest.json` (106 KB) | Машинный source→destination манифест копирования. |

### 1b. 50-проходный Opus-аудит (51 файл — «мясо», проходы 2-9)
- `opus-PROJECT-CLOSURE-PLAN.md` (548 KB) — накопительный реестр ~245 дыр (P01-P49) + консолидированная roadmap. Проходы 8-9.
- `opus-pass-01..50-*.md` (50 файлов, ~6-32 KB) — по-проходный аудит реального состояния сборки. Карта в §5.

### 1c. Транскрипты сессий (2 файла — проход 10)
- `extension development session 2.md` (95 KB) — основной транскрипт.
- `Extension development session.md` (4 KB) — короткий транскрипт: Luba ловит модель на «пакет вместо действующего изменения»
  (project-AGENTS.md/project-CLAUDE.md/skills положены в supporting-files, но НЕ применены в активные управляющие файлы).

### 1d. copied-plans/ (30 файлов — структурно; проход 10)
Копии планов/состояний из `C:\Projects\llm_mvp_core\docs\plans`. Ключевые семейства:
- Extension-runtime: `EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md` (+ STATE.json, REVIEW.jsonl),
  `EXTENSION-FULL-READINESS-DUAL-PATH-PLAN-v1.md` (+ EXECUTION-LOG, EXECUTION-STATE, review-ledger),
  `EXTENSION-IMPLEMENTATION-PLAN-v1.md`, `EXTENSION-REALITY-MAP-v1.md`.
- Orchestration/prompt-to-task: `ORCHESTRATION-AND-PROMPT-TO-TASK-FULL-EXTENSIONS-PLAN-v3.md` (+ STATE.json),
  `ORCHESTRATION-MACHINE-TRAINING-PLAN-v17.md`, `-v20.md` (+ STATE.json).
- API/dashboard: `API-DASHBOARD-PLAN-v1.md` (+ STATE.json).
- DPO/skills: `DPO-KNOWLEDGE-MAP-v1.md`, `SKILL-SELECTION-DPO-NEGATIVE-STRATEGY-v1.md`.
- Semantic quality: `SMART-SKILLS-SEMANTIC-QUALITY-COMPLETION-PLAN-v1.md` (+ STATE), `-UPLIFT-PLAN-v1.md` (+ STATE).
- Trainable-architecture-audit: `TRAINABLE-ARCHITECTURE-AUDIT-v1..v4.md` (+ STATE.json каждый) — 4 версии аудита архитектуры.

### 1e. supporting-files/ (108 файлов рекурсивно — структурно; проход 10)
- 15 HOWTO/док верхнего уровня: `CLAUDE-XIIGEN-HOWTO-ru.md`, `CURSOR-XIIGEN-HOWTO-ru.md`, `CODEX-XIIGEN-HOWTO-ru.md`,
  `API-DASHBOARD-QA-HOWTO-ru.md`, `EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-HOWTO-ru.md`, `LOCAL-MODEL-LIFECYCLE-HOWTO-ru.md`,
  `EVIDENCE-SCOPES-HOWTO-ru.md`, `XIIGEN-DEBUG-TRACE-HOWTO-ru.md`, `43-smart-skills-domain*.md` (×3),
  `HOW-TO-USE-SKILLS.md`, `PROJECT-DOCUMENTATION-INDEX.md`, `SUBPROJECT-MAP.md`, `SESSION-START-PROMPT.md`,
  `RUSSIAN-STATUS-AND-EXECUTION-LOG-TEMPLATE.md`.
- `project-AGENTS.md`, `project-CLAUDE.md` — КОПИИ правил проекта (не активные; см. §1c короткий транскрипт).
- `skills/` (6 SKILL): audit-protocol, claim-verification, client-adaptive-learning-loop, design-conversion-dpo-authoring,
  dpo-triple-format, planning-dpo-authoring.
- `evidence-extension-full-readiness-dual-path/` (84 файла) — evidence-пакеты dual-path (все датированы 2026-06-28):
  phase-05..16 readiness-матрицы + intake-downgrade’ы (json+md парами); scenario-execution join-пакеты для claude/cursor/codex;
  token-savings пакеты (cursor row12 — с baseline/adaptive traces); quality-semantic held-out/ablation/freshload;
  status-board; product-readiness-scenario-board + action-plan. Это ГЛАВНЫЙ источник «фактических» evidence по dual-path.

### 1f. Мусор (игнорировать)
- `grep.exe.stackdump` (1 KB) — краш-дамп, не контент.

Подсчёт: 8 + 51 (50 pass + closure) + 2 сессии + 4 (manifest/2 session уже учтены… ) → фактически 200 = 8 статус + 50 pass + 1 closure
+ 1 ledger(в 8) … итог по `find`: **200 файлов** (12 верхнеуровневых + 50 opus-pass + 30 copied-plans + 108 supporting-files).

---

## 2. ТЕКУЩИЙ статус пакета (самоотчёт 2026-07-08) — что заявляет сам пакет

Свежесобранный СТАТУС-пакет. Прямая цитата границы: «This package shows status and evidence. It does not claim product
readiness.» Только package/copy-работа завершена; продукт — нет. Рабочая единица для host/runtime — dual-path
same-scenario coverage (Path A автоматический boundary-proof + Path B локальный desktop-proof для ОДНОГО сценария).

Human phase counts: **Total 15 · Done 2 · In progress 2 · Partial 8 · Not started 1 · Needs fix 2 · Remaining/не закрыто 13.**
«Done 2» = ТОЛЬКО shell пакета + русские докы. Ни одна из 13 продуктовых фаз не закрыта.

### Доска 15 фаз (claimed самоотчётом)
| # | Фаза | Claimed | Реальное основание (кратко) |
|---:|---|---|---|
| 1 | Package shell + copy-manifest | DONE | только копирование |
| 2 | Русские entry-докы + навигация | DONE | только копии, часть устарела/mojibake |
| 3 | API/dashboard/supporting докы | IN_PROGRESS | HOWTO/dashboard-планы скопированы |
| 4 | 50-pass package review stage | IN_PROGRESS | ledger 0/50 pending |
| 5 | Claude same-scenario evidence | PARTIAL | narrow join-evidence |
| 6 | Cursor same-scenario evidence | PARTIAL / NEEDS_FIX | narrow join + row12 |
| 7 | Codex same-scenario evidence | PARTIAL | narrow join-evidence |
| 8 | Prompt to task | PARTIAL | план/state скопированы |
| 9 | Task to plan orchestration | PARTIAL | план скопирован |
| 10 | Task/plan to skills | PARTIAL | skill-selection/DPO-докы |
| 11 | DPO / local learning | PARTIAL | DPO-карты, learnability-план |
| 12 | Checkpoint / locator | PARTIAL | candidate/audit материалы |
| 13 | Token savings | NEEDS_FIX | прошлое evidence не доказало reduction |
| 14 | Semantic quality | NEEDS_FIX | прошлый пакет partial с фейлами |
| 15 | Real lifecycle operation | NOT_STARTED | только докы/runbooks |

### Requirements-coverage (матрица требований Luba)
9 требований задания → 8 `COVERED_FOR_PACKAGE/STATUS`, 1 `PREPARED_NOT_RUN` (50-pass ledger), 1 `COVERED_AS_BOUNDARY`
(no-overclaim). **Product-ready claims = 0.** Т.е. матрица подтверждает: покрыто только копирование/статус, не реализация.

Explicit not-proven (дословно из отчёта): product readiness, all-host readiness, full Claude/Cursor/Codex readiness,
token savings, semantic quality uplift, learnability uplift, accepted DPO training, model/checkpoint promotion, locator
update, full local desktop readiness, full live host readiness, broad tests, git commit/push, branch readiness, PR,
deployment, external provider/API execution.

---

## 3. РЕАЛЬНЫЙ статус сборки (Opus-аудит от кода, а не от самоотчёта) — АВТОРИТЕТНО

Статус-пакет намеренно уклончив («status only»). Opus-проходы измеряют РЕАЛЬНЫЙ код в `C:\Projects\llm_mvp_core`
и в host-репозиториях. Ключевой вывод P08/P50: **продуктовый DONE = 0/15; PROVEN dual-path = 0/28 сценариев**
(`product-readiness-scenario-board.json`: `full_joined_pass:0`; все 14 «accepted» — narrow/scoped, `formal_joined_pass=false`).

### 3a. Три РЕАЛЬНЫХ host-репозитория СУЩЕСТВУЮТ (коррекция раннего «host MISSING»)
Отдельные git-репо, НЕ упомянутые board’ом 2026-07-08 (P06 нашёл, P08 замерил, P50 подтвердил):
| Репо (`C:\Projects\...`) | Зрелость | dist | test/ | *.ts | last commit | working-tree |
|---|---|---|---|---|---|---|
| `cursor-xiigen-smart-skills` | САМЫЙ зрелый: реальный `activate()`, 3 команды, chat participant, webview-панель, собранный `dist/extension.js` | ЕСТЬ | есть | 1 (`extension.ts`) | 2026-06-28 | M + untracked runtime-03 |
| `claude-xiigen-smart-skills` | СКЕЛЕТ: один SessionStart-hook, рукописный `.js`, нет сборки | НЕТ | НЕТ | 0 | 2026-06-23 | M README/package.json (uncommitted) |
| `codex-xiigen-smart-skills` | рукописный `.js`, НЕ зарегистрирован в реальном marketplace Codex; нет сборки | НЕТ | есть | 0 | 2026-06-18 | clean |

Транспорт всех трёх: жёстко зашитый абсолютный путь `C:\Projects\llm_mvp_core` + вызов `tools/xiigen-extension-core-api.ps1`
через `spawnSync` PowerShell (per-call `dotnet run`-компиляция всего `43.SmartSkillsDomain`). **Непоставляемо**: требует
core-репо на той же машине + .NET SDK. Host-runtime доказан только install/session-smoke маркером (`XIIGEN_PHASE40_SMOKE`,
codex 3/4 stdout = 0 байт), артефакты 5-недельной давности (2026-06-03). Ни одного prompt→answer round-trip в коммитах.

### 3b. Core-сторона (`llm_mvp_core`) — частично BUILT
Реальные .NET проекты: `43.SmartSkillsDomain`, `43.SmartSkillsDomain.Dashboard` (WPF WinExe), `44.DesignConversionDomain`,
`42.ProjectWikiDomain`, `15.OrchestratorModel`, `11.DPO`, `25.ModelServer`, `Tests.ApiDashboardWpf`.
Единственная extension-граница: `43.SmartSkillsDomain\XiigenExtensionApiService.cs` (1930 строк) — чистая in-process
C#-библиотека. `ProcessHostExtensionRequest(...)` всегда возвращает `Status="core_boundary_partial_no_semantic_quality_claim"`,
`runtime_extension_claimed="false"` (зашито, нет code-path в true). Вызывается ТОЛЬКО из WPF-дашборда + xUnit-тестов.
НЕТ CLI/stdio/JSON-RPC/HTTP/MCP entrypoint. `HostType` — просто строка, нигде не ветвится (три расширения неотличимы в коде).

### 3c. Реальный статус 15 фаз (claimed vs real, из P08 §1)
| # | Фаза | Board | РЕАЛЬНО | Опорное file:line |
|---:|---|---|---|---|
| 3 | Dashboard | IN_PROGRESS | BUILT, но гоняет ФЕЙКОВЫЙ `RecordingGateway` | `ApiDashboardWindowInteractiveTests.cs:27` |
| 4 | 50-pass review | IN_PROGRESS | ABANDONED — сессия оборвалась на 1-10 из 50 | `...session.md:90761`; ledger 0/50 |
| 8 | Prompt to task | PARTIAL | **STUB** — всегда `not_ready_no_semantic_task` | `PromptToTaskMachine.cs:90` |
| 9 | Task to plan | PARTIAL | **STUB** — всегда `not-ready-no-orchestration-inference` | `OrchestrationReasoningMachine.cs:109` |
| 10 | Task→skills | PARTIAL | BUILT + order-leakage authority (позиция кандидата = авторитет) | `SkillSelectionMachine.cs:65/767/113` |
| 11 | DPO/local learning | PARTIAL | реальный `DpoTrainer` ОТСОЕДИНЁН; «обучение» = словарь/счётчик | `11.DPO/DpoTrainer.cs`; `manifest.json semanticQualityClaimed:false` |
| 12 | Checkpoint/locator | PARTIAL | STUB TRANSPORT — `UploadAsync` no-op, locator in-memory | `LocalModelRepository.cs:32-77`; `InMemoryRemoteUnitLocator.cs:8` |
| 13 | Token savings | NEEDS_FIX | STUB EVIDENCE — delta 0, baseline≡adaptive байт-в-байт (MD5 совпал) | `measurement.json:4-11` |
| 14 | Semantic quality | NEEDS_FIX | failing — `semanticQualityClaimed=false` везде; агрегирующий гейт мёртв | `XiigenExtensionApiService.cs:337` |
| 15 | Real lifecycle | NOT_STARTED | MISSING — нет реального backup/export/import/share/continue/rollback | STATUS-BOARD:35 |

### 3d. Обучающий движок — критический дефект
Настоящий градиентный DPO-движок (`11.DPO`) существует, но ОТСОЕДИНЁН от расширений. Единственный исполненный
градиентный run **РЕГРЕССИРОВАЛ**: loss вырос к ln2≈0.693, reward-margin сжался ~3×, held-out≈0.56 (шум) — P25-1/P05-3.
Три «обучаемые» машины (prompt-to-task / orchestration / skill-selection) на деле — детерминированные keyword/lexical-lookup
(нарушение правила R4 «no hidden determinism»). `semantic_quality_claimed` зашит в `false` → семантический гейт мёртв по построению.

---

## 4. Карта 50-pass Opus-аудита (топики + известные выводы)

Важное различие (обнаружено в этом проходе):
- **`50-PASS-SUBAGENT-REVIEW-LEDGER.md`** (24 KB) — это ledger ревью САМИХ статус-документов пакета (report/README),
  а НЕ индекс 50 opus-проходов. В нём заголовок «0/50 pending», но приписаны реальные секции **Pass 01-10** (проверяют
  только читаемость отчёта: entry summary, counts, no-overclaim wording — НЕ продукт). Passes 11-50 = PENDING-заглушки.
  Т.е. это ОТДЕЛЬНЫЙ, оборванный на 10/50 doc-review, не путать с opus-pass-NN.
- **`opus-pass-01..50-*.md`** — настоящий аудит реального билда (это и есть 50 проходов). Мастер-реестр = `opus-PROJECT-CLOSURE-PLAN.md`.

### Карта opus-pass-01..50 (топик = из имени файла; вывод = где уже прочитан/выведен в P08/P50)
| Pass | Топик | Известный вывод (проходы 2-7 углубят) |
|---:|---|---|
| 01 | claimed-vs-package (SEED) | Пакет = copy/status; завёл 13 фаз-дыр + host-entrypoint MISSING; создал мастер-реестр. |
| 02 | definition-of-done | ORCHESTRATION-v3 mojibake, REALITY-MAP stale; PromptToTask/Orchestration = стабы. |
| 03 | real-host-code | В `llm_mvp_core` НЕТ host-кода (0 манифестов/entrypoint/adapter); только core-boundary lib. |
| 04 | real-api | Dashboard гоняет ФЕЙКОВЫЙ `RecordingGateway`. |
| 05 | real-dpo-lifecycle | DpoTrainer отсоединён; UploadAsync no-op; locator in-memory; order-leakage; локатор не переживает рестарт. |
| 06 | evidence-reality | Установил существование 3 host-репо; token delta 0, baseline≡adaptive (MD5). |
| 07 | session-actual-work | Сессия 50-pass оборвалась на 1-10 (session.md:90761). |
| 08 | real-baseline-matrix | Свод: product-DONE 0/15, PROVEN 0/15; замер 3 host-репо; board мис-локализует deliverable. |
| 09 | claude-entrypoint | Только SessionStart-hook, не user-точка; нет settings-поверхности; test-only локатор. |
| 10 | claude-pipeline | 1/3 методов достижимы; каталог скиллов пустой/само-ссылочный; мёртвый semantic-quality гейт. |
| 11 | claude-learning | DpoTrainer отсоединён; нет online write-back; manifest checkpoint-ref mismatch. |
| 12 | claude-qa | QA-покрытие claude-хоста неполное. |
| 13 | cursor-entrypoint | absolute-path дефолты; PowerShell-спавн (не кросс-платформенно); test-only команды в прод-активации. |
| 14 | cursor-pipeline | canned auto-run панели; стадия-3 берёт сырой промпт; feedback-метод не реализован. |
| 15 | cursor-token | row12-harness BLOCKED; token-delta 0 синтетика. |
| 16 | cursor-qa | QA cursor-хоста неполное. |
| 17 | codex-entrypoint | НЕ зарегистрирован в реальном marketplace Codex; runCli пустой stdout; ≥3 расходящихся манифеста, ≥5 бинарей. |
| 18 | codex-pipeline | Method не валидируется против MethodNames; 1/3 методов достижимы. |
| 19 | codex-learning | learnability codex-хоста не доказана. |
| 20 | codex-qa | QA codex-хоста неполное; ≥5 версий бинаря. |
| 21 | prompt-to-task | hardcoded alias-словарь (не выученный); пороги 0.01/0.01 (гейт всегда открыт); append вместо training-update. |
| 22 | task-to-plan | словарь стадий 1↔2 рассинхрон (12/15 паттернов недостижимы); нет reward-scalarization; replay-baseline. |
| 23 | task-to-skills | lexical bag-of-words argmax (не эмбеддинг); нет rejected-стороны; held-out=1 (circular); нет source-disjoint eval. |
| 24 | dpo-rows | DPO-пары не материализованы (`chosen_ref/rejected_ref`); orchestration 0/12 пар. |
| 25 | dpo-training | 88 report’ов все UNKNOWN delta; единственный реальный run регрессировал. |
| 26 | checkpoint-locator | `IRemoteUnitLocator.LocateAsync` обойдён; версия — литерал в коде; index.json руками; нет integrity-гейта. |
| 27 | token-savings | delta 0; baseline≡adaptive. |
| 28 | semantic-quality | `semanticQualityClaimed=false` везде; гейт мёртв. |
| 29 | dashboard-docs | dashboard/докы — route-tied QA отсутствует. |
| 30 | lifecycle-ops | export/import/backup/share/continue/rollback только в тестах; payload = 555-байт огрызок. |
| 31 | scenario-inventory | 28 сценариев × хост; full_joined_pass 0; 14 scoped accepted. |
| 32 | claude-scenarios | Claude-сценарии joined-pass 0. |
| 33 | cursor-scenarios | Cursor-сценарии joined-pass 0. |
| 34 | codex-scenarios | Codex-сценарии joined-pass 0. |
| 35 | pathA-harness | Path A обходит расширение (in-process fallback); нужен зелёный build под текущим деревом. |
| 36 | pathB-desktop | Path B = re-photo/stale/чужая поверхность; 0 видео. |
| 37 | core-boundary | Редакция/honesty/presentation/locator триплицированы в 3 адаптерах, расходятся (нарушение R6). |
| 38 | trainable-audit | R4 governance-downgrade «детерминизм ок если claimed=false»; эталон integrity в `42.ProjectWikiDomain` не портирован. |
| 39 | tests-qa-state | Нет записанного полного `.trx` baseline (только узкие filtered). |
| 40 | git-branch-state | Extension-дельта ядра НЕ закоммичена (331 файл/+18 574/−3 576, 545 untracked); 9 расходящихся worktree. |
| 41 | adversarial-recheck | Транспорт = per-call компиляция; ядро не first-class в `LlmFromScratch.sln`. |
| 42 | hidden-overclaims | Скрытые оверклеймы в статус-файлах. |
| 43 | missing-scenarios | Недостающие сценарии. |
| 44 | integration-e2e-gaps | Нет единого versioned wire-contract; нет CorrelationId/UserPromptSha256 в ответе. |
| 45 | data-provenance-legal | Нет license/provenance-гейта; sonnet-derived строки в common (distillation без teacher-гейта). |
| 46 | ux-humanreadable-gaps | UX/human-readable дыры в host-выводе. |
| 47 | perf-token-quality-proof | Нет реального perf/token/quality proof. |
| 48 | priority-sequencing | Критический путь M0→M6; узкое место M1→M2 (реальный обучаемый инференс), от него ~60% доказательных дыр. |
| 49 | risks-decisions-external | RCE-вектор в host-адаптерах; нет устанавливаемого/публикуемого артефакта; decision-ledger для Luba. |
| 50 | roadmap | Консолидированная дорожная карта M0-M6 (см. §6). Новых дыр не добавляет. |

---

## 5. Дорожная карта закрытия (из opus-pass-50 + P48) — для плана продолжения VS/VSCode

Ярусы: P0 фундамент (блокирует всё) · P1 продуктовый хребет (host+транспорт+wiring) · P2 доказательство/dual-path ·
P3 честность/гигиена (параллельно). Порядок capability-first (по зависимостям), НЕ coverage-first.

- **M0 — Сохранить и собрать (P0):** закоммитить extension-дельту ядра фазовыми коммитами (P40-1); добавить
  `43.SmartSkillsDomain`+`44.DesignConversionDomain` как `Project()` в `LlmFromScratch.sln` (P41-2); зелёный `dotnet build`
  под текущим деревом (P35-6); записать полный `.trx` baseline (P39-4); стабилизировать 9 worktree (P40-5).
  Gate M0: build + full .trx + все extension-исходники под git + `git merge-tree` чисто в `codex/chat-gpt-5.5` И `main`.
- **M1 — De-risk обучающего движка (P0, критический узел ПЕРЕД стройкой):** bounded feasibility-спайк — реальный токенизатор
  + модель адекватного размера на ОДНОЙ extension-задаче; требовать монотонное loss↓ + held-out ≫0.5 (чинит регрессию P25-1).
  Gate M1: ≥1 успешная кривая обучения. Если from-scratch (Rule 10) не тянет качество → ЭСКАЛАЦИЯ Luba (scope/model-решение).
- **M2 — Реальный обучаемый инференс в 3 машинах (P0, УЗКОЕ МЕСТО, зависит M1):** prompt-to-task (эмбеддинг вместо
  alias-словаря, калибровка порогов); orchestration (согласовать словарь стадий, reward-scalarization, обучаемый planner);
  skill-selection (эмбеддинг, rejected-сторона, ablation, leakage-guard, source-disjoint eval); оживить semantic-quality гейт;
  материализовать DPO-пары; исполнить training с loss/reward-margin/delta; снять R4 governance-downgrade.
  Gate M2: измеренный held-out uplift на ЗАГРУЖЕННОМ checkpoint + ablation + source-disjoint + независимое sub-agent-ревью.
- **M3 — Продуктовый хребет (P1, зависит M2):** тёплый поставляемый транспорт (stdio/HTTP, без per-call компиляции/absolute-path);
  единый versioned wire-contract (schema_version, CorrelationId, UserPromptSha256); реальные host-точки входа
  (Claude: slash/skill/MCP; Cursor/VS Code: `package.json contributes`+`extension.ts activate()`+command+chat participant;
  Codex: регистрация в marketplace); pipeline-wiring через `ILogicalOrchestrator`; lifecycle/locator/promotion wiring; thin-adapter SDK (R6).
- **M4-M6:** доказательство/измерение/dual-path (28×хост), token/quality/learnability/lifecycle proof, provenance/legal, UX,
  RCE-fix, устанавливаемый/публикуемый артефакт (детали — в opus-pass-50, проход 8-9).

Ключевой риск для VS Code/Cursor-плана: **Cursor-репо самый зрелый** (реальный extension) — это лучшая стартовая точка для
Path B desktop, но его token-harness BLOCKED и транспорт непоставляем. **Claude/Codex-хосты — скелеты.** Всё доказательство
downstream (M2+) заблокировано de-risk-спайком обучения (M1).

---

## 6. Границы и честность этого прохода

- claim_scope: инвентарь 200 файлов + извлечение текущего статуса (пакет 2026-07-08) + реального статуса (Opus P01/03/08/48/50)
  + карта 50 проходов. Русский, read-only.
- evidence_scope: прочитаны 6 статус-докумен­тов + ledger полностью; opus-pass-01/03/08 полностью; opus-pass-48/50 частично
  (начало); имена всех 200 файлов через `find`; выводы passes 02/04/05/06/07/09-49 взяты из сводных P08/P50, НЕ из полного
  чтения каждого (это проходы 2-7). Тесты/сборки/git/runtime НЕ запускались. Реальные host-репо и `llm_mvp_core` НЕ читались
  напрямую в этом проходе (цитируются по Opus-аудиту).
- НЕ доказано этим проходом: любые продукт-readiness утверждения; точность каждого per-pass вывода (нужна верификация родителя).
- subagent_id: UNKNOWN_IN_SUBAGENT. requested_model: gpt-5.5-equivalent (Opus 4.8 Ultracode). requested_reasoning_effort: xhigh.

<!-- APPEND PASS 2+ BELOW THIS LINE -->

## Pass 2/10 — opus-pass 01-08: claimed-vs-package + DoD + real host-code + real API + real DPO-lifecycle + evidence-reality + baseline

Границы прохода 2: прочитаны ПОЛНОСТЬЮ восемь файлов opus-pass-01..08 (фундамент/ground-truth аудит). Это детальное
извлечение доказательств file:line из каждого прохода, а не сводка. Read-only; ничего не запускалось. Автор: Claude Opus 4.8
(Ultracode), одиночный bounded-агент, читал и писал сам. Ниже — по одному разделу на проход.

---

### P01 — claimed-vs-package (SEED): дельта оверклейма

**Что физически прочитано (bounded):** 7 статус-докумен­тов пакета целиком + листинг `copied-plans/` и `supporting-files/`;
быстрый зонд репо `C:\Projects\llm_mvp_core` (top-level дерево 67 записей).

**Заявлено (самоотчёт board):** 15 фаз; Done 2; In progress 2; Partial 8; Not started 1; Needs fix 2; **не закрыто 13**.
Критично: «Done 2» = ТОЛЬКО package/report/copy работа (STATUS-BOARD:37-45, CURRENT-REPORT:34). Ни одна из 13 продуктовых
фаз не закрыта — это признаёт сам пакет.

**Honesty-границы пакета (дословно, CURRENT-REPORT стр. 80-90):**
- claim_scope: «fresh package/report/copy refresh for extension development status package only».
- evidence_scope: «current-turn filesystem copy/refresh … **No product runtime, no tests/builds, no git, no external AI/web, no 50-pass review execution**».
- downgrade_needed: «yes for any product readiness or 50-pass review claim».
- Explicit not-proven (стр. 70): product readiness, all-host readiness, full Claude/Cursor/Codex readiness, token savings,
  semantic quality/learnability uplift, accepted DPO training, model/checkpoint promotion, locator update, full local
  desktop/live host readiness, broad tests, git commit/push, branch readiness, PR, deployment, external provider/API.
- 50-PASS ledger: «Accepted review pass records 0/50», «count_claimed 0» (стр. 11-16); слоты 01-50 PENDING; записанные
  Pass 01-10 проверяют ТОЛЬКО читаемость самого отчёта (entry summary, counts, no-overclaim wording) — НЕ продукт.

**ДЕЛЬТА ОВЕРКЛЕЙМА (главный вывод SEED, от кода, не от board):** планы и пакет говорят о «трёх полноценных расширениях»,
но в реальном репо `llm_mvp_core` зонды дали:
- `find package.json` (без node_modules) → **пусто**;
- `find extension.ts / extension.js` → **пусто**;
- grep `class .*(Claude|Cursor|Codex).*(Adapter|Host|Extension|Bridge|Plugin)` в `*.cs` → **No files found**;
- grep `XiigenExtensionApiService` → единственная extension-граница `43.SmartSkillsDomain\XiigenExtensionApiService.cs`.
→ По Full Extension Definition ОТСУТСТВУЕТ первый обязательный компонент — реальная host-точка входа на каждый хост. Заведено
как отдельная MISSING-дыра над остальными 13. Core-сторона частично BUILT (реально существуют .NET проекты
`44.DesignConversionDomain`, `43.SmartSkillsDomain`, `42.ProjectWikiDomain`, `15.OrchestratorModel`, `11.DPO`, `25.ModelServer`,
`43.SmartSkillsDomain.Dashboard`, `Tests.ApiDashboardWpf`), host-сторона MISSING. **Разрыв host↔core — центральная тема плана.**

**Три уровня зрелости, введённые SEED:** COPIED_DOC_ONLY (только скопированный план/статус) · BUILT_NOT_PROVEN (код есть,
не доказан) · PROVEN (**ни одна фаза**) · MISSING (host-shells, Phase 15 lifecycle, Phase 4 review 0/50).

---

### P02 — definition-of-done: планка приёмки («что значит done»)

**Прочитано:** шапки/gate-секции 7 планов (runtime-token-quality, ORCHESTRATION-v3, API-DASHBOARD, DPO-KNOWLEDGE-MAP,
TRAINABLE-ARCHITECTURE-AUDIT-v1, EXTENSION-FULL-READINESS-DUAL-PATH, EXTENSION-REALITY-MAP) + bounded греп живого кода.

**ПЛАНКА DONE — сводный 15-пунктовый чек-лист (пересечение всех планов).** Для КАЖДОГО из 3 хостов (Claude, Cursor, Codex)
полная реализация = всё доказано dual-path (Path A boundary + Path B local desktop):
1. **Host entrypoint** — реальный код точки входа + manifest/package wiring (v3 #11).
2. **Settings/debug surface** — debug setting, видимый raw/redacted I/O, error path, свежий correlation id (runtime-plan :119).
3. **Target-locator / core-API path** — тонкий adapter строит `XiigenExtensionRequest`, вызывает `XiigenExtensionApiService`,
   показывает `XiigenExtensionResponse`; НИКАКОЙ логики в адаптере (runtime-plan :120).
4. **Prompt → task** — `PromptToTaskMachine` (v3 #3).
5. **Task → orchestration/plan** — `OrchestrationReasoningMachine`/`OrchestrationPlanMachine` (v3 #4).
6. **Task/plan → skills** — `SkillSelectionMachine` (v3 #5).
7. **Local adaptive learning** — DPO из user-approved/source-grounded данных, две ноги common/adaptive (v3 #6-10, :123).
8. **Export/import/backup/continue-training + rollback** — lifecycle доказан (:123; DPO-map held-out/fresh-load/continue).
9. **Token before/after** — на парных промптах (одинаковый hash/host/tokenizer), без регрессии качества (:121).
10. **Semantic quality** — послойные скоры (PromptToTask/OrchestrationReasoning/SkillSelection) + held-out + ablation +
    no-memorization (:122, audit-v1).
11. **Checkpoint/locator** — promotion через `IModelRepository`/`IRemoteUnitLocator` где применимо (:123).
12. **QA** — через живой host ИЛИ scoped handler; для dashboard — full WPF UI automation + UX automation (AR-08/18/22).
13. **Dual-path** — Path A (boundary) И Path B (local desktop) для ОДНОГО и того же сценария/prompt.
14. **Архитектурная легитимность** — обучаемые единицы в разрешённых уровнях, без скрытого детерминизма (audit-v1 :37-56).
15. **Метод** — только bounded sub-agents; **≥10 plan reviews ДО** + **≥10 dual-path проверок ПОСЛЕ** (:124-126).

**Сверка планки с живым кодом (новые якоря, коррекция P01):**
- Prompt→task и Task→orchestration — это **СТАБЫ**, не «BUILT_NOT_PROVEN»: `PromptToTaskMachine.cs:90`
  `NotReadyTaskType="not_ready_no_semantic_task"`; `43.SmartSkillsDomain\README.md:52` «Skeleton implementation. No semantic
  classification or inference is implemented.»; `OrchestrationReasoningMachine.cs:109`
  `NoInferenceScenarioId="not-ready-no-orchestration-inference"`. → пункты done #4/#5 не реализованы даже на уровне inference.
- SkillSelection обучаема, но решает по ПОЗИЦИИ кандидата (order leakage): `SkillSelectionMachine.cs:65`
  `Dictionary<int,int> _candidatePositionWeights`; `:767` обучение по индексу кандидата; `:113` решение стартует от
  `_candidatePositionWeights.Count`. Греп `CandidateOrderLeakageGuard` → только в `docs/skills/*`, `CLAUDE.md`, `AGENTS.md`,
  `44.DesignConversionDomain\*`; в `43.SmartSkillsDomain` guard ОТСУТСТВУЕТ.
- Конвейер не собран: греп `15.OrchestratorModel` на `PromptToTask|OrchestrationReasoning|SkillSelection` → **No files found**.
- Копи-рекон УСТАРЕЛ: `EXTENSION-REALITY-MAP-v1.md` (2026-06-04) утверждает «SkillSelection Export/Import return NOT_READY»,
  но реальный `SkillSelectionMachine.cs:397/:412` — полноценные реализации (Export пишет checkpoint+manifest; Import валидирует
  MachineId/DomainTag/CheckpointRef/ManifestRef + LoadCheckpoint). → package-артефакты нельзя брать за current-state.
- Two-leg lifecycle scaffold есть (`Phase08CommonUserTwoLegLifecycle.cs`, `ILearningMachinePair`/anonymised) — BUILT_NOT_PROVEN.

**11 новых дыр P02** (кратко): thin-adapter no-determinism review gate MISSING; prompt→task STUB; orchestration STUB;
SkillSelection order-leakage + нет guard в 43; конвейер не зарегистрирован; two-leg pair не доказан; DPO admission gate не
выполнен; v3 десять сквозных gate не доказаны; метод-гейт ≥10/≥10 = 0/0; dashboard AR-10/18/20/22; source integrity (v3
mojibake + устаревший рекон).

---

### P03 — real host-code: файл-карта host-слоя

**Инструменты (bounded, tracked-only, `git ls-files`/`git grep`):**
- `git ls-files "*.ts" "*.tsx" "*package.json" "*.vsix" "*.vsixmanifest" "*.vsct" "*extension.js"` (без node_modules):
  `.ts/.tsx`=**0**; `.vsix/.vsixmanifest/.vsct`=**0**; `extension.js`=**0**; `package.json`=**только 4**, и все — model-asset
  backup-пакеты, не extension-манифесты (`artifacts/adaptive-learning/AD-06-…backup-package.json`, `OR-AD-03-…`, `PTT-AD-03-…`,
  `training-data/…/q6-05-redacted-adaptive-export-package.json`).
- `git grep "class .*(Claude|Cursor|Codex)(Adapter|Host|Extension|Bridge|Plugin|Entry|Command|Participant)" -- '*.cs'` → **0**.
- grep `vscode|VSCode|chatParticipant|registerCommand|activate\(|ExtensionContext|contributes` → совпадения ТОЛЬКО в
  `docs\skills\*` и `docs\plans\evidence\…\*` (захваченные логи реального Cursor: `exthost.log`, `renderer.log`,
  `Cursor Structured Logs.log`, драйвер `row12-host-visible-command-runner.test.js`). В исходниках продукта этих API нет.

**Единственный extension-border файл продукта — `43.SmartSkillsDomain\XiigenExtensionApiService.cs` (1930 строк)** + контракт
`XiigenExtensionApiContracts.cs`:
- Публичный метод `ProcessHostExtensionRequest(XiigenExtensionRequest) -> OperationResult<XiigenExtensionResponse>` (`:88`).
- Статус ответа ВСЕГДА `PartialBoundaryStatus="core_boundary_partial_no_semantic_quality_claim"` (`:11`, ставится `:178`).
- Обе ветки (успех/ошибка) всегда пишут `runtime_extension_claimed="false"` (`:216`, `:257`, `:560`) — зашито, нет code-path в true.
- Контракт-коммент `:3`: «Request sent by thin Claude/Cursor/Codex host clients to xiigen exe» — подразумеваемый «xiigen exe».

**Кто реально вызывает** `ProcessHostExtensionRequest`: WPF-дашборд `ApiDashboardRunner.cs:26/:139`
(`43.SmartSkillsDomain.Dashboard.csproj:17 <OutputType>WinExe`) + множество xUnit-тестов. → **НЕТ CLI/stdio/JSON-RPC/HTTP/MCP
entrypoint**; ни один из трёх target-host не вызыватель.

**Host-специфичность:** `HostName`/`HostType` (`Contracts:5-6`) валидируются ТОЛЬКО на непустоту (`:102` host_name_required,
`:107` host_type_required), эхо-копируются в debug/refs. Греп литералов `(claude|cursor|codex)` по `43.SmartSkillsDomain/*.cs`
→ **0**. Ветвления по host НЕТ — один generic boundary выдаётся за три расширения.

**MCP:** единственный MCP-код `23.OssEquivalents\Mcp\McpToolDefinition.cs`/`McpTransport.cs`; греп
`XiigenExtension|SmartSkillsDomain|ProcessHostExtensionRequest` по `23.OssEquivalents/**` → 0 (не мост).

**6 новых дыр P03:** (1) Host↔core process transport («xiigen exe») MISSING — нет stdio/JsonRpc/HttpListener/MapPost;
(2) per-host differentiation MISSING (HostType не ветвится); (3) host-runtime phase MISSING — `runtime_extension_claimed` зашит
в false, нет code-path в true (строго сильнее BUILT_NOT_PROVEN); (4) демонстрируемый caller = внутренний WPF WinExe (WRONG-HOST,
по AR-10 dashboard→core ≠ host UI); (5) Cursor «evidence» = внешний UI-driver `.test.js` + логи (MISSING extension-пакет);
(6) три HOWTO = COPIED_DOC_ONLY (сами признают «intended path / not a proof packet»; треки token 0/9, quality 0/9, DPO 0/9,
lifecycle 0/9, final review 0/10).
**Честная оговорка P03:** греп вёлся по `llm_mvp_core`; внешние host-репо (`*-xiigen-smart-skills`) в фокус P03 НЕ входили —
это коррекция P06.

---

### P04 — real API: где реально собранный extension-продукт (для ДРУГИХ хостов)

**Прочитано целиком/частично:** `42.ProjectWikiDomain/ProjectWikiPluginOrchestrationAdapter.cs` (293 стр.);
`Tests.ApiDashboardWpf/ApiDashboardWindowInteractiveTests.cs` (стр. 1-960 из 1758); `43.LearningLocalWikiData/tools/
WikiContextHook/Program.cs`; `ProjectWikiTrainingRunner/Program.cs:700-790`; `reports/PLUGIN-RUNTIME-INTEGRATION-GATE.json`.

**ГЛАВНЫЙ ВЫВОД P04:** существует НАСТОЯЩИЙ собранный XIIGen extension-продукт — но для НЕ целевых хостов.
`PLUGIN-RUNTIME-INTEGRATION-GATE.json` (`:14-56`) показывает два реальных собранных плагина:
- **Visual Studio**: `Xiigen.VisualStudio.SmartCodeWiki.vsix` (2.85 МБ) + `_42.ProjectWikiDomain.dll` (net461); контракты
  `_42_ProjectWikiDomain.Contracts.IWikiOrchestrator/WikiAnswer/WikiReadinessStatus`.
- **VS Code**: `out/hookRunner.js` экспортирует `checkReadiness`, `getMap`, `queryWiki`, `resetBinaryCache` + `package.json`.
- Оба читают readiness из XIIGen-runtime-отчёта для целевого репо `C:/Projects/askapediaweb`.
→ `queryWiki` — метод VS Code плагина во ВНЕШНЕМ репо `xiigen_visual_code`, НЕ C#-API в `llm_mvp_core`. В самом `llm_mvp_core`
от транспорта только (а) Node-проба, грузящая внешний `hookRunner.js` против ПОДДЕЛЬНОГО `vscode`-стаба
(`43.LearningLocalWikiData/artifacts/plugin-runtime-node-stub/node_modules/vscode/index.js`), и (б) CLI-хук `WikiContextHook`,
идущий по BOOTSTRAP_FALLBACK. **Целевые Claude/Cursor/Codex — host-артефактов НОЛЬ** (репо `xiigen_claude/cursor/codex` в этом
проходе искались по НЕВЕРНЫМ именам и объявлены отсутствующими — коррекция P06).

**Dashboard `Tests.ApiDashboardWpf` — НАСТОЯЩАЯ WPF UI-автоматизация** (STA-поток, драйв реальных контролов через AutomationPeer,
проверка AutomationProperties-имён/подсказок, обход фокуса, непересекающиеся колонки). НО гоняет **ПОДДЕЛЬНЫЙ RecordingGateway**:
`ApiDashboardWindowInteractiveTests.cs:27` `new RecordingGateway(request => BuildSuccessfulResponse(request))`; `:942-960`
возвращает синтетический «Mapped WPF dashboard response.»; `:251` ассерт `runtime_extension_claimed=false`; `:337`
`semantic_quality_claimed=false`. → UI-обвязка доказана, интеграция с ядром и семантика — нет.

**8 новых дыр P04:** (1) реальные плагины = VS + VS Code (askapediaweb), WRONG-TARGET; (2) `queryWiki`/`hookRunner.js` транспорт
внешний + in-repo MOCK-стаб; (3) `WikiContextHook` CLI = BOOTSTRAP_FALLBACK (orchestrator=null `:68-69`→`:143`; `--ensure-ready`
захардкожен `isReady=true`); (4) `ProjectWikiPluginOrchestrationAdapter` поле `PluginHost` (`:17`) нигде не читается (STUB
host-differentiation); (5) plugin runtime gate = LOAD-PROBE ONLY (`entryCount:0`, `passed:true`, generated 2026-05-25 — 6+ недель
несвежий); (6) dashboard fake RecordingGateway (BUILT UI / NOT core-integrated); (7) `xiigen-platform`/`xiigen-flows` ссылаются на
XIIGen только через copied docs/skills/plans (COPIED_DOC_ONLY, grep кода-интеграции = 0); (8) VS-VSIX поставляет ДИВЕРГЕНТНЫЙ
net461-контракт (`_42_ProjectWikiDomain.Contracts.IWikiOrchestrator`), которого нет в текущем net9.0-core (STALE PORT).

---

### P05 — real DPO-lifecycle: что за обучение реально существует и что отсоединено

**Прочитано целиком:** `11.DPO/DpoTrainer.cs` (333 стр.); `42.ProjectWikiDomain/ProjectWikiDpoMetricRunner.cs` (499 стр.);
`25.ModelServer/Contracts/IModelRepository.cs` + `Local/LocalModelRepository.cs`; `IRemoteUnitLocator.cs` +
`Remote/InMemoryRemoteUnitLocator.cs`; manifest-паблишеры 42-домена.

- **Реальный движок ЕСТЬ:** `11.DPO/DpoTrainer.cs` — НАСТОЯЩИЙ from-scratch DPO тренер: policy/reference `LlmModel`, `AdamW`,
  ручной backward через logits, DPO-loss `-log σ(β·margin)`, метрики `dpo_step_NNNNNN.json` через atomic write (`.tmp`→`Move`),
  Beta=0.1, lr=1e-5, есть `Step/StepMany/EvaluateMany/StepVector`. **Не стаб.**
- **НО тренировочный runner на игрушке:** `ProjectWikiDpoMetricRunner.cs` — реальный runner поверх `DpoTrainer` (пишет
  `dpo-pairs.jsonl`, `policy_final.bin`, `reference_base.bin`, `project-wiki-dpo-metric-summary.json`; меряет CPU/RAM/elapsed;
  50 train + 50 held-out; требует `[WRONG:` в rejected). НО токенайзер **фейковый**: `StableHash(token)%(vocabSize-1)`
  (`:352`, `:357`) — хеш-коллизии убивают семантику; модель игрушечная: ctor `vocabSize=2048, contextLen=192, hiddenDim=64,
  numLayers=1, numHeads=4, ffnHidden=128` (`:39-44`). → loss/accuracy/reward-margin меряют предсказание хеш-токенов на
  1-слойном трансформере, не семантику ответа.
- **Транспорт lifecycle no-op:** `LocalModelRepository.UploadAsync` **ничего не передаёт** — проверяет `PolicyCheckpointPath`,
  считает SHA256, возвращает `RemoteUnitManifest` В ПАМЯТИ (`:50-77`). HTTP/HttpListener/WebApplication/MapPost в
  `25.ModelServer/*.cs` — grep 0. Реального model-server-процесса нет.
- **Локатор в памяти:** `InMemoryRemoteUnitLocator` — `Dictionary<string,RemoteUnitManifest>` в памяти процесса (`:8`);
  `RegisterAsync` кладёт в словарь, теряемый при выходе. Персистентного реестра/маркетплейса нет. Manifest-паблишеры (реальные
  классы, подключены к `*TrainingPipeline.cs`, напр. `CodeTraceBuilderTrainingPipeline.cs:37`) опираются на эти no-op транспорты.
- **Промоушен физически произошёл, но модели = словари:** `git ls-files models/*` = 15 файлов
  (`models/{prompt-to-task,orchestration-reasoning,orchestration,skill-selection,translation}/*-v1/{manifest.json,
  *-checkpoint.json, runner-report.json}` + `models/index.json`). Runtime грузит их — `XiigenExtensionApiService.LoadCommonAssets`
  (`:1389-1469`). Содержимое: `prompt-to-task-v1-checkpoint.json` (18 796 байт) `learnedState.learnedPatterns[]` = списки
  **ключевых слов** («approval», «plan», «planning», «scope_creep» …) — обученный СЛОВАРЬ, не нейромодель; manifest
  `semanticQualityClaimed:false`, `learnedPatternCount:12`, `dpoPairCount:8` (обучено на 8 парах), `heldOutRowCount:4`;
  `skill-selection/runner-report.json` `command:"regenerate-skill-selection-model-from-training-data"`,
  `trained_pair_count:900` — профильные СЧЁТЧИКИ по 11 суб-юнитам (dictionary/counter state), не DPO поверх LlmModel.
- **Ключевой разрыв:** `git grep -c "DpoTrainer|LlmModel"` в `43.SmartSkillsDomain/*.cs` = **0**. Реальный трансформерный DPO
  из 11.DPO **НЕ используется** тремя extension-машинами. Вызыватели `DpoTrainer`: 11.DPO, 18.CapabilityTraining,
  42.ProjectWikiDpoMetricRunner, experiments-v2.
- **`.xiigen`-writer СУЩЕСТВУЕТ** (опровергает P02 «no writer»): `artifacts/adaptive-learning/AD-02/04/07/09A/.xiigen/
  smart-skills/*/{adaptive-checkpoint.json, adaptive-training-report.json, continue-probe-report.json, restore-import-report.json}`.

**6 дыр P05:** P05-1 extension trained-models = словари/счётчики (`semanticQualityClaimed=false`); P05-2 реальный 11.DPO
отсоединён от ответного пути расширений; P05-3 хеш-токенайзер + игрушечная модель → метрики не про семантику; P05-4 UploadAsync
no-op, нет model-server/HTTP; P05-5 locator только in-memory; P05-6 `.xiigen` writer есть, но числовой adaptive/continue-training
прирост не доказан.

---

### P06 — evidence-reality: какое доказательство настоящее vs синтетика/stale

**Что физически в `supporting-files/evidence-extension-full-readiness-dual-path/`:** 84 файла, 9 подпапок,
**скриншотов/видео/изображений — 0** (`find -iname *.png/*.jpg/*.mp4` → пусто). Всё pixel-доказательство Path B — только по
абсолютным ссылкам в реальном репо `C:/Projects/llm_mvp_core/docs/plans/evidence/…` (там скриншоты реально есть). Содержимое
пакета — JSON/MD борды/матрицы/«intake-downgrade» фаз 05-16, три «dual-path join»-пакета, два token-набора, heldout-ablation.

1. **Token-savings = синтетика, дельта СТРУКТУРНО нулевая:** `token-savings-cursor-row12-20260628-153837/measurement.json`
   `before_tokens=6209`, `after_tokens=6209`, `delta_tokens=0`, `token_reduction_observed=false`, `synthetic_no_send_only`.
   `baseline-response.json` и `adaptive-response.json` **байт-в-байт идентичны** (один MD5 `fc5135b6eb699e759a715184fe0cad51`,
   оба 119855 байт). → пара baseline↔adaptive не может показать дельту ПО ПОСТРОЕНИЮ.
2. **Реальный Cursor host-harness ПРОВАЛИЛСЯ, логи пусты:** `token-host-visible-cursor-row12-…160311/result.json`
   `status=BLOCKED_CURSOR_HOST_VISIBLE_HARNESS_FAILED`; `runner_report_created:false`; 6 отсутствующих обязательных trace-ref;
   `cursor-cmd-stdout.log`=0 байт, `cursor-cmd-stderr.log`=0 байт, `adaptive/baseline-command-output.txt`=1 байт (пусто).
3. **«Dual-path join» = бумажный join, не живой прогон:** Claude `…145508/result.json` `ui_work_run:false`, `adapter_run:false`,
   `broad_tests_or_builds_run:false`, `proves_only`=«прочитаны два ДРУГИХ result.json и пересказаны их статусы»; Cursor
   `join-packet.json` Path A `ui_used:false`, `exact_cursor_adapter_cli_used:false`, `route_used="direct XiigenExtensionRequest
   API/core boundary fallback"`; Codex `join-packet.json` Path A `route_used="direct exported Codex adapter function with string
   host facts"`, `ui_used:false`. → ни один живой прогон не гоняет host→core→host для одного промпта.
4. **Path B «visible response» = скриншот + человеческое утверждение, машинная выемка ПРОВАЛИЛАСЬ:** Cursor
   `cursor-path-b-final-response-debug-…/result.json` `uia_capture_error="Argument types do not match"` (UIA-текст не извлечён);
   Codex `codex-path-b-response-capture-…/result.json` `new_prompt_submitted:false`, `thinking_visible:false`,
   `exact_prompt_previously_submitted:true` (пере-фотография ранее отправленного промпта, не свежий submit→answer).
5. **phase-16-propagation-evidence = governance-литералы, не продукт:** проверяет, что 10 anti-overclaim строк
   («trace != savings», «marker-only != full local desktop pass» …) присутствуют как подстроки в AGENTS.md/CLAUDE.md/skills —
   88 term-file пар, `PASS 88/88`, `tests_builds_probes_training_evals_run:0`. Лежит внутри readiness-evidence, читается как
   завершение фазы, но продуктовой готовности ноль.
6. **КОРРЕКЦИЯ P03/P04 — три ЦЕЛЕВЫХ host-репо РЕАЛЬНО существуют** (пропущены из-за неверных имён):
   `C:\Projects\{claude,cursor,codex}-xiigen-smart-skills` существуют. `cursor-xiigen-smart-skills` — настоящий VS Code/Cursor
   extension: `package.json` (`publisher:xiigen`, `engines`, `activationEvents`, `contributes`, `main:./dist/extension.js`),
   `src/extension.ts` **1345 строк**, собранный `dist/extension.js` **48148 байт**, команда `xiigenSmartSkills.runtimeProof.invoke`
   (`src/extension.ts:116/:148/:598`). `codex-xiigen-smart-skills` `package.json main:"src/codex-entrypoint.js"` (Codex CLI-стиль).
   `claude-xiigen-smart-skills` — `package.json` есть, но `dist/extension.js`/`src/extension.ts` не найдены (наименее зрелый).
   → «Host entrypoints MISSING» как абсолют неверно: host-код ЕСТЬ (Cursor собран), но принятое evidence в пакете доказывает его
   лишь синтетикой Path A + скриншот/заблокированным Path B; сквозного машинно-проверенного prompt→answer через собранную команду
   в принятых артефактах НЕТ. **Ни одно доказательство не поднимает статус выше BUILT_NOT_PROVEN, а token = STUB EVIDENCE.**

---

### P07 — session-actual-work: что сессия реально исполнила vs нарратив

**Метод:** `Extension development session.md` = **90 761 строка / 8 942 802 байта**, один длинный многотемный chat (не сырой
tool-log). Исполнения показаны двумя способами: развёрнутые `<details>Ran N commands</details>` с bullet `- Ran \`<команда>\``
(инспектируемых строк ровно **57**) и свёрнутые сводки `> Ran 19 commands…` (**37**, текст команд скрыт).

**Ключевые числа (grep, нарратив vs исполнено-инспектируемо):** `dotnet build` 109/**0**; `dotnet test` 233/**0**; `dotnet run`
125/**0**; `git commit` 24/**0**; `git push` 15/**0**; `git add` 20/**0**; npm/node/vsce/tsc/esbuild ~/**0**. Мутирующие git в
развёрнутых строках → **0**. → в инспектируемой части НЕТ ни одного product-build/test/runtime/git-мутации.

**Все 57 развёрнутых команд — read-only:** `Get-ChildItem`/`Get-Item`/`Get-Content` (листинг/чтение); `Select-String` (подсчёт
mojibake/conflict-маркеров, pending-строк ledger, product-ready-claims); `rg -n "FIRST LAW|TOO_FAST_HIGH_PASS|…"` (верификация
governance-литералов); `git status --short`/`git diff --check` (read-only). → бюджет ушёл на governance-литерал-верификацию +
проверку читаемости пакета = process-about-process.

**Опорные строки транскрипта:**
- **стр. 10216** (self-flag): `| FC-6 | Stale numbers | ❌ FAIL | Baseline не получен через живой dotnet test. Откуда "48 tests
  passing" — в плане не написано.` → даже цитируемые числа тестов не получены живым прогоном.
- **стр. 84469** (потолок host-исполнения): `Runner теперь работает технически: exit code 0, extension виден, команда
  xiigenSmartSkills.panel резолвится, TEST_ONLY не использован, submit не нажимался.`
- **стр. 87713**: `Claude was rerun with actual HostName=Claude, HostType=claude, exit code 0.` → команда расширения резолвится +
  exit 0, но SUBMIT (реальный prompt→answer) НЕ нажимался ни разу.
- **стр. 90761** (конец файла): `Leibniz делает Pass 01–10` — транскрипт ОБРЫВАЕТСЯ на первом куске 1-10, БЕЗ единого
  возвращённого review-packet; ledger 0/50.

**5 дыр P07:** product build/test/runtime/git в сессии = NARRATED_NOT_EXECUTED; тест-базлайн STALE/UNSOURCED (self-flagged
стр.10216); host-runner потолок = резолв команды + exit 0 без submit (нет e2e prompt→answer); 50-pass package review
ABANDONED_MID_CHUNK (0 packets) → серия opus P01-P50 = фактическая замена; git-провенанс продукта тройки MISSING (все in-session
commit-упоминания — про core/governance/JSON-evidence, не про host-продукт; closure-артефакт собран ВНЕ git).

---

### P08 — real baseline matrix: свод 15 фаз + прямой замер host-репо

**Матрица 15 фаз (Board claimed → РЕАЛЬНЫЙ статус · опорное доказательство · кто нашёл):**
| # | Фаза | Board | РЕАЛЬНО | Доказательство |
|---:|---|---|---|---|
| 1 | Package shell + copy | DONE | DONE (только copy) | `extension development/*` |
| 2 | Russian entry docs | DONE | DONE (только copy), часть stale/mojibake | ORCHESTRATION-v3 mojibake; REALITY-MAP stale |
| 3 | API/dashboard docs | IN_PROGRESS | BUILT_NOT_PROVEN — dashboard реальный, гоняет ФЕЙКОВЫЙ RecordingGateway | `ApiDashboardWindowInteractiveTests.cs:27` |
| 4 | 50-pass package review | IN_PROGRESS | MISSING/ABANDONED — 0/50, оборвалось на 1-10 | `session.md:90761`; ledger 0/50 |
| 5 | Claude same-scenario | PARTIAL | BUILT_NOT_PROVEN — репо unbuilt, нет `test/`/`dist`, phase40 smoke (stale Jun3), uncommitted | P08 §2 |
| 6 | Cursor same-scenario | PARTIAL | BUILT_NOT_PROVEN — собран+`test/`, но row12 BLOCKED, token-delta 0 синтетика, untracked runtime-03 | P06/P08 §2 |
| 7 | Codex same-scenario | PARTIAL | BUILT_NOT_PROVEN — hand-js+`test/`, без сборки; phase40 3/4 stdout=0, run-4 shell-smoke exit0 | P08 §2 |
| 8 | Prompt to task | PARTIAL | **STUB** — `not_ready_no_semantic_task` | `PromptToTaskMachine.cs:90`; `README.md:52` |
| 9 | Task to plan | PARTIAL | **STUB** — `not-ready-no-orchestration-inference` | `OrchestrationReasoningMachine.cs:109` |
| 10 | Task→skills | PARTIAL | BUILT_NOT_PROVEN + order-leakage authority | `SkillSelectionMachine.cs:65/:767/:113` |
| 11 | DPO/local learning | PARTIAL | BUILT_NOT_PROVEN — `DpoTrainer` отсоединён; «обучение»=словарь/счётчик | `11.DPO/DpoTrainer.cs`; `models/*/manifest.json semanticQualityClaimed:false` |
| 12 | Checkpoint/locator | PARTIAL | STUB TRANSPORT — `UploadAsync` no-op, locator in-memory | `LocalModelRepository.cs:32-77`; `InMemoryRemoteUnitLocator.cs:8` |
| 13 | Token savings | NEEDS_FIX | STUB EVIDENCE — delta 0, baseline≡adaptive байт-в-байт | `measurement.json:4-11`; MD5 совпал |
| 14 | Semantic quality | NEEDS_FIX | PARTIAL/failing — `semanticQualityClaimed=false` везде | `XiigenExtensionApiService.cs:337` |
| 15 | Real lifecycle | NOT_STARTED | MISSING — нет backup/export/import/share/continue/rollback | STATUS-BOARD:35 |

**Прямые замеры по РЕАЛЬНЫМ host-репо (новое в P08):**
- **§2.1 Транспорт** — все три entrypoint (`claude src/claude-entrypoint.js`, `codex src/codex-entrypoint.js`,
  `cursor dist/extension.js`) содержат жёстко зашитый абсолютный путь `C:\Projects\llm_mvp_core` + вызов
  `tools/xiigen-extension-core-api.ps1` через `spawnSync` PowerShell. `.ps1` существует (9825 байт, Jun 26) — по P05/P06 это
  repo-local **no-send/синтетический** путь. → расширения НЕ самодостаточны и НЕ поставляемы (нужен core-репо на той же машине +
  PowerShell/.NET SDK).
- **§2.2 Зрелость сборки асимметрична:** claude — dist НЕТ, `test/` **НЕТ**, 0 `.ts` (hand-js), last-commit 2026-06-23,
  working-tree M README+package.json (uncommitted); cursor — `dist/extension.js` ЕСТЬ, `test/` есть, 1 `.ts`, 2026-06-28,
  M+untracked runtime-03; codex — dist НЕТ, `test/` есть, 0 `.ts` (hand-js), 2026-06-18, clean. **Cursor — единственный с реальной
  сборкой TS→JS; claude беднейший (нет dist/test/.ts + uncommitted).**
- **§2.3 Runtime-артефакты = smoke/marker, устаревшие:** собственные `phase40-*` все от 2026-06-03 (5 недель до board 07-08):
  codex `phase40-…run{,-2,-3}.stdout.jsonl`=**0 байт**, только `run-4.stdout.jsonl`=3610 байт (двойной shell-out
  `powershell.exe -Command powershell.exe …`), `run-4.last-message.txt`=«Shell exit code 0; XIIGEN_CODEX_HOST_SMOKE_DONE»;
  claude `phase40-…run.stdout.log`=«XIIGEN_PHASE40_SMOKE» (UTF-16 маркер), stderr=0. Ни одного prompt→answer round-trip.
- **§2.4 Board-дисконнект:** board 07-08 для фаз 5/6/7 указывает evidence ТОЛЬКО «copied/supporting files» и НИ РАЗУ не ссылается
  на реальные host-репо, где лежит настоящий код + phase40-артефакты. Репо дрейфнули после Jun 18-28, имеют uncommitted/untracked,
  не отражённые board’ом. **Базовая матрица пакета мис-локализует собственный deliverable.**

**Консолидированный вывод P08:** реальный **product-DONE = 0/15**, реальный **PROVEN = 0/15**. Board переоценивает фазы 8/9 как
PARTIAL, тогда как реальный код — STUB (claimed>real на уровне board). Единственный транспорт = локальный PowerShell-шим
(непоставляемый), host-runtime доказан smoke-маркером. `REQUIREMENTS-COVERAGE-MATRIX.md` сам подтверждает scope=package/status
(COVERED_FOR_PACKAGE ×8, PREPARED_NOT_RUN ×1, product-ready claims = 0). **4 дыры:** G1 board-дисконнект + working-tree дрейф;
G2 phase40 = smoke/marker, stale, codex 3/4 stdout пусты; G3 транспорт всех трёх = зашитый локальный PowerShell (не поставляемы);
G4 асимметрия зрелости (claude без dist/test/.ts + uncommitted, codex без сборки — не паритет с cursor).

---

### Итог прохода 2 (что дают P01-P08 для плана продолжения VS/VS Code)

- **Дельта оверклейма (P01):** «три полноценных расширения» заявлены, но первый обязательный компонент (host-точка входа) в
  `llm_mvp_core` MISSING; core частично BUILT, host MISSING.
- **Планка done (P02):** 15-пунктовый чек-лист × 3 хоста × dual-path; ≥10 plan-review ДО + ≥10 dual-path ПОСЛЕ. По коду: 2/3
  стадии конвейера = СТАБЫ, 3-я с order-leakage, конвейер не собран в оркестраторе.
- **Файл-карта host (P03):** 0 `.ts`/vsix/extension.js в трекнутом `llm_mvp_core`; единственная граница —
  `XiigenExtensionApiService.cs` (1930 стр., in-process, `runtime_extension_claimed=false` зашит), вызывается только WPF+тестами;
  host-ветвления нет.
- **Реальный API (P04):** собранный XIIGen-продукт СУЩЕСТВУЕТ, но для Visual Studio + VS Code над `askapediaweb` (VSIX 2.85 МБ +
  `hookRunner.js queryWiki`) — **готовый паттерн для портирования на целевую тройку**. Dashboard = реальная WPF UI-автоматизация,
  но против фейкового RecordingGateway. VS-VSIX = дивергентный net461-контракт.
- **Реальный DPO (P05):** движок `11.DPO/DpoTrainer.cs` настоящий, но отсоединён от расширений (grep 0); extension-«модели» =
  словари ключевых слов/счётчики (`semanticQualityClaimed=false`); ProjectWiki-runner на хеш-токенайзере+игрушке; upload/locate
  = no-op/in-memory.
- **Evidence-reality (P06):** token delta 0 + baseline≡adaptive (MD5); Cursor harness BLOCKED, логи пусты; join'ы бумажные;
  Path B = скриншот без машинной выемки; целевые host-репо `*-xiigen-smart-skills` РЕАЛЬНО существуют (cursor собран 1345 стр.
  .ts / 48148 байт dist, claude скелет, codex hand-js).
- **Baseline (P08):** product-DONE 0/15, PROVEN 0/15; транспорт = зашитый локальный PowerShell (непоставляем); зрелость
  cursor > codex > claude; runtime = smoke-маркеры 5-недельной давности; board мис-локализует deliverable.

**Что остаётся на проходы 3-10:** P09-10 (claude host entrypoint/pipeline), P11-12 (claude learning/QA), P13-16 (cursor
entrypoint/pipeline/token/QA), P17-20 (codex entrypoint/pipeline/learning/QA), P21-30 (три машины prompt-to-task/task-to-plan/
task-to-skills + dpo-rows/training/checkpoint-locator/token/semantic/dashboard/lifecycle), P31-40 (сценарии inventory + per-host
scenarios + pathA-harness + pathB-desktop + core-boundary + trainable-audit + tests-qa + git-branch), P41-49 (adversarial-recheck,
hidden-overclaims, missing-scenarios, integration-e2e, provenance-legal, ux, perf, priority-sequencing, risks), P50 (roadmap
M0-M6) + `opus-PROJECT-CLOSURE-PLAN.md` (~245 дыр). Проход 8-9 читает closure-план и roadmap; проход 10 — copied-plans +
supporting-files + транскрипты.

**Границы честности прохода 2:** прочитаны ПОЛНОСТЬЮ opus-pass-01..08; выводы привязаны к file:line из этих проходов. Сам код
`llm_mvp_core`/host-репо в этом проходе НЕ читался напрямую (цитируется по Opus-аудиту — родитель верифицирует). Тесты/сборки/
git/runtime НЕ запускались. subagent_id: UNKNOWN_IN_SUBAGENT. requested_model: gpt-5.5-equivalent (Opus 4.8 Ultracode).
requested_reasoning_effort: xhigh.

## Pass 3/10 — opus-pass 09-16: Claude host (entrypoint/pipeline/learning/qa) + Cursor host (entrypoint/pipeline/token/qa)

Границы прохода 3: прочитаны ПОЛНОСТЬЮ восемь файлов opus-pass-09..16 — детальная per-host build-reality двух из трёх хостов.
Claude (P09-12) и Cursor (P13-16). Cursor = форк VS Code, поэтому наиболее релевантен плану продолжения VS/VS Code. Это
извлечение доказательств file:line из каждого прохода, не сводка. Read-only; ничего не запускалось. Автор: Claude Opus 4.8
(Ultracode), одиночный bounded-агент, читал и писал сам. Все file:line-ссылки — из Opus-аудита (родитель верифицирует).

**Оценщик каждого прохода читал ЖИВОЙ код host-репо и `llm_mvp_core`, не копии пакета.** Итоговая шкала зрелости повторяется:
`MISSING` → `COPIED_DOC_ONLY` → `BUILT_NOT_PROVEN` → `PROVEN`. Ни один из двух хостов не поднимается выше **BUILT_NOT_PROVEN**;
Claude беднее (скелет), Cursor богаче (реальный extension), но оба провалены на одном и том же: нет ни одного machine-verified
prompt→answer, где XIIGen меняет ВИДИМЫЙ ответ хоста, и агрегирующий semantic-quality гейт мёртв в ядре по построению.

---

### CLAUDE HOST — 4 прохода (P09 entrypoint · P10 pipeline · P11 learning · P12 qa)

Репо: `C:\Projects\claude-xiigen-smart-skills` (последний коммит `e4eaac7 phase 09: normalize claude proof evidence`,
рабочее дерево грязное: `M README.md`, `M package.json`). Версия плагина — `0.0.0-phase40-skeleton` (само-признанный СКЕЛЕТ).

#### P09 — Claude entrypoint / настройки / core-API

**Что реально есть:**
- **Manifest-surface = ТОЛЬКО SessionStart-hook.** `.claude-plugin/plugin.json` top-level ключи = `name/displayName/version/
  description/author/hooks`. **Нет** `commands`, `skills`, `mcpServers`, `agents`, `settings`. Единственный крючок
  `hooks.SessionStart[0]` запускает `node.exe src\claude-entrypoint.js` с ЗАШИТЫМ smoke-промптом «HOST-IMPL-CLAUDE-01 redacted
  local hook wiring smoke.» + абсолютными путями в `C:\Projects\llm_mvp_core`.
- **core-API реально доходит до .NET-ядра, но непереносимо.** `claude-entrypoint.js:495` `spawnSync("powershell.exe", …
  coreEndpoint)`, где `coreEndpoint = tools/xiigen-extension-core-api.ps1`. Этот PS-скрипт (`:112-187`) генерирует ВРЕМЕННЫЙ
  csproj, пишет C#-Program `new XiigenExtensionApiService(coreRoot); service.ProcessHostExtensionRequest(request)`, делает
  `dotnet run` и удаляет temp. **Каждый вызов = отдельная сборка+прогон .NET** → нужен .NET SDK + весь core-репо на той же
  машине + Windows PowerShell. Реальная граница `XiigenExtensionApiService` вызывается, но не как поставляемый сервис.
- **Claude-сторона не делает семантики** (корректно по R6/thin-adapter): `buildRequest` только собирает `XiigenExtensionRequest`
  (`HostName:"Claude"`, `HostType:"claude-code"`, `Method:"prompt-to-task"`, `host_semantic_ranking:false`), пишет request-JSON,
  зовёт core, печатает `ChatDisplayText` в stdout. Host-ветвления нет (как и в ядре, P03).

**6 дыр P09:**
- **G1 (MISSING USER SURFACE)** — манифест экспонирует только SessionStart-hook; нет пользовательской команды/skill/MCP/настройки.
  Пользователь Claude физически не может запустить выбор smart-skill на СВОЁМ запросе. Ev: `plugin.json` (только
  `hooks.SessionStart`); `docs/PROJECT-DOC.md:102` (самопризнание, нет активной регистрации `xiigen.smartSkills.select`).
- **G2 (PHANTOM ENTRYPOINT)** — имя `xiigen.smartSkills.select` заявлено во ВСЕХ evidence-JSON (`phase40-claude-*`, install-smoke,
  contract) и в `prompts/claude-smoke-prompts.jsonl` (`entrypoint_name`), но НИГДЕ не зарегистрировано (`plugin.json`/`hooks.json`
  такой команды не содержат). «Entrypoint name без регистрации».
- **G3 (DEAD DEBUG SETTING)** — `package.json:24` объявляет `xiigenSmartSkills.debug`, но `plugin.json` нет `settings`;
  entrypoint читает `DebugEnabled` ТОЛЬКО из CLI-флага `-DebugEnabled`, а поставляемый hook его НЕ передаёт → вся ветка
  debug-дисплея (`claude-entrypoint.js:386-398`) недостижима. Ev: `scripts/phase09-claude-proof.js:650`.
- **G4 (OUTPUT В ЧУЖОЙ РЕПО)** — hook пишет `-OutputPath/-TracePath` в АБСОЛЮТНЫЕ пути внутри
  `C:\Projects\llm_mvp_core\artifacts\...` + фиксированный smoke-промпт → ответ НИКОГДА не появляется в сессии/workspace Claude
  (stdout = только `chatDisplayText`, `:530-532`).
- **G5 (TEST-ONLY LOCATOR)** — `plugin.json` зашит на `qsg28-two-unit-test-locator.json` (`activation_scope:"test_only_qsg28"`,
  `production_locator_update_done:false`, `irreversible_production_promotion_done:false`). Refs = staged
  `prompt-to-task-v1-checkpoint.json` (keyword-словарь, P05) + `skill-selection-v1-checkpoint.json` (счётчики, P05);
  OrchestrationReasoning ЯВНО исключён. Покрывает 2 из 3 машин, обе — словарь/счётчик. Entrypoint только нормализует refs
  (`:234-278`, thin-adapter ok).
- **G6 (ДВЕ РАСХОДЯЩИЕСЯ КОПИИ + ECHO-PROOF)** — помимо репо под ревью существует ВТОРАЯ копия
  `C:\Users\lubao\Documents\xiigen\extensions reasearch\claude-xiigen-smart-skills` (свой `.git`, другой README/package.json, без
  `src/`). Единственный записанный host-session run гонял ЭТУ Documents-копию с промптом «Print XIIGEN_PHASE40_SMOKE only»,
  `stdout_length:22` (UTF-16 marker) — доказан лишь **string-echo `claude.exe`**, не исполнение node-entrypoint/hook/ядра.
  Version-pin’ы `claude.exe` устарели (`2.1.160`/`2.1.187` в артефактах vs установлены `2.1.197`/`2.1.202`).

**Вывод P09:** Claude-«расширение» = СКЕЛЕТ-плагин. Thin-adapter архитектурно корректен и реально доходит до
`ProcessHostExtensionRequest`, но непереносим и его OUTPUT пишется в чужой core-репо. Статус — **BUILT_NOT_PROVEN скелет**.

#### P10 — Claude pipeline (prompt→task→orchestration→skills)

**КОРРЕКЦИЯ P02** («pipeline не собран/недостижим»): конвейер СУЩЕСТВУЕТ и СВЯЗАН, но ПЛОСКО в `43`, не через
`15.OrchestratorModel`. `XiigenExtensionApiService.ProcessHostExtensionRequest` руками сцепляет:
`:117` BuildPromptToTaskRequest → `:118` `PromptToTaskMachine.Translate` → `:130` BuildOrchestrationRequest →
`:131` `OrchestrationReasoningMachine.Plan` → `:143` `DecideSkills` (SkillSelectionMachine) → `:155` `ShouldClaimSemanticQuality`.
Grep 3 машин в `15.OrchestratorModel` → **0**. То есть путь ДОСТИЖИМ, но arch-flat: жёсткий трёхшаговый вызов, минуя
разрешённые Level-4/Level-3, без A-to-B пар/fan-out/arbiter (def-of-done v3 #3-5), не композируем как flow, не обнаруживаем локатором.

**Три критических факта Claude-конвейера:**
1. **Гейт `semantic_quality_claimed` НЕДОСТИЖИМ по построению (DEAD-GATE / false-by-construction).** `ShouldClaimSemanticQuality`
   (`:1364-1379`) → true только если ВСЕ три машины дают токен `SEMANTIC_QUALITY_CLAIMED_TRUE` (`:17`) И ни одного no-claim. Grep
   `SEMANTIC_QUALITY_CLAIMED_TRUE` по всему репо = ТОЛЬКО `Tests/*`, `tools/*Runner`, `docs/plans/*` — **ни в одной машине**. Хуже:
   каждая машина вшивает no-claim даже на УСПЕШНОМ пути — `PromptToTaskMachine.cs:206-207` (`BASELINE_NOT_SEMANTIC_QUALITY` +
   `SEMANTIC_QUALITY_CLAIMED_FALSE`), `OrchestrationReasoningMachine.cs:599-601`, `SkillSelectionMachine`
   (`CANDIDATE_POSITION_WEIGHT_SCAFFOLD_ONLY`, блокирует `:1373`). Семантика Claude-ответа недостижима МАТЕМАТИЧЕСКИ, а не «пока
   не доказана» — сильнее P02 (stub) и P05 (checkpoint=словарь).
2. **Claude кормит конвейер ПУСТЫМ каталогом скиллов.** Default request (`claude-entrypoint.js:8-18`): `RequestedSkills:[]`,
   `AvailableSkills:[]`. Hook не передаёт `-Method/-AvailableSkills/-RequestedSkills`. → в ядре `DecideSkills:361-371` при
   `AvailableSkills.Count==0` сразу `NO_AVAILABLE_SKILLS_PROVIDED`, `SelectedSkillIds:[]`, `Confidence:0.0`,
   `RequiresSecondPass:true`. Третья стадия («task/plan→skills») — **постоянный no-op** на Claude. Пустой каталог + second-pass
   снова роняют `ShouldClaimSemanticQuality` (`:1371`).
3. **Из Claude драйвится только `prompt-to-task` (1/3 методов).** Hook без `-Method` → всегда `prompt-to-task`. Хостовые методы
   `orchestration-plan`/`skill-selection` (`:966-971`) никогда не запускаются end-to-end из Claude. Внутри сервиса все 3 стадии
   всегда прогоняются, но Claude показывает только prompt-to-task-ветку.

#### P11 — Claude learning / adaptive / lifecycle

- **Claude-host НЕ содержит ни одной команды export/import/backup/share/continue-training.** `package.json` scripts = только
  `install-smoke`; `plugin.json` только `hooks.SessionStart`; в `claude-entrypoint.js` нет таких функций.
- **Core two-leg lifecycle РЕАЛЬНО реализован, но вызывается ТОЛЬКО тестами.** `Phase08CommonUserTwoLegLifecycle.cs:505` пишет
  `personal-adaptive-checkpoint.json`, `:583/:588` читает `imported-adaptive-checkpoint.json`, `:668`
  `ContinueTrainingFromCheckpoint(...)`. Вызыватели (git grep) = только `Tests/Phase08CommonUserTwoLegLifecycleTests.cs`,
  `Tests/SkillSelectionPhase06TrainedSelectorEvidenceTests.cs`. Граница расширения `XiigenExtensionApiService.cs` ссылается на
  Phase08/ContinueTraining/export/import **0 раз** — оттестированная библиотека без единого runtime/host-вызывателя.
- **Метод `feedback` — единственный online-сигнал пользователя в adaptive-модель — объявлен, но НЕ реализован.** Контракт
  `XiigenExtensionApiContracts.cs:64` `FeedbackMethod="feedback"`, `:248-263` схема (`feedback_class`/`chosen_ref`/`rejected_ref`/
  `correction_ref`), `:282` boundary-rule `feedback_routes_refs_only_to_user_local_adaptive_dpo_signal_creation`. Сервис grep
  `feedback` = **0**; диспетчер `:962-974` для незнакомого метода даёт дефолт «Method {method} produced task, plan, and skill
  evidence.» → для feedback вернётся task/plan/skills, НЕ создание DPO-сигнала. Claude без `-Method` → feedback никогда не
  вызывается. **Цикла локального обучения нет.**
- **Рантайм Claude read-only, ноль мутации adaptive-модели по построению.** `:822` «makes no learnability claim», `:26-27`
  `USER_ADAPTIVE_SKILL_SELECTION_FALLBACK_NO_MODEL_MUTATION`; `reports/CL-03:144`, `reports/CL-04:134`.
- **Конфляция common-promotion staged root и per-user adaptive root.** `claude-entrypoint.js:239-243` `userRoot = … ||
  raw?.activated_asset_root`; поставляемый локатор не имеет `UserAdaptiveAssetRoot`, а имеет `activated_asset_root` (staged
  ОБЩИЙ two-unit target). Реального per-user adaptive-состояния не подключено.
- **Расширение само декларирует adaptive lifecycle ВНЕ своей области** (противоречит R5 + Full Extension Definition):
  `phase40-claude-core-request.json:36` `adaptiveAssetRef:"...not-owned-by-extension"`, `:38` `adaptiveOwnedByHostOrProject:true`;
  install-smoke `:57-58` «...outside-package», «project-local:...outside-host-package».
- **Рантайм-загрузка checkpoint ЛОМАЕТСЯ для 1 из 3 машин:** `reports/HOST-IMPL-01-probe/.../xiigen-response.json:22`
  `orchestration_reasoning_load_error:"ORCHESTRATION_REASONING_MANIFEST_CHECKPOINT_REF_MISMATCH"` (в debug-on/off + install-smoke).
  Загрузка существующего checkpoint (предусловие continue-training/fresh-load) сломана для OrchestrationReasoning.

#### P12 — Claude QA / dual-path

- **QA-гейт Claude = ровно ОДИН отчёт `HOST-08E-claude-hook-qa.json`,** и он сам декларирует узость: `:11`
  `claude_cli_plugin_qa:false`, `:12` `full_chat_ui_evidence:false`, `ps1_accepted_as_implementation:false`. `hook_command`
  (`:15`) = `node.exe "…claude-entrypoint.js" …` — **прямой вызов entrypoint через node.exe (Path A boundary), НЕ команда плагина
  в Claude.** Все 4 кейса (prompt-to-task off/on, orchestration-plan, skill-selection) `exit_code:0`, `response_success:true`,
  status `core_boundary_partial_no_semantic_quality_claim`; поля `semantic_quality_claimed` ПУСТЫ (`:72/:94/:116/:138` = `""`).
  Ассертит плумбинг (форма/redaction/exit-code), НЕ корректность. Даже «успешный» кейс = `loaded_state_no_matching_learned_pattern`.
- **Нулевое негативное покрытие** — все 4 кейса happy-path, `gaps:[]`. Нет кейсов bad core-root, invalid method, empty catalogue,
  checkpoint-mismatch (P11).
- **Единственный реальный Path B прогон `claude.exe` завершился ОШИБКОЙ, маркер не совпал, но помечен PASS.**
  `RUNTIME-02-claude/.../runtime02-positive-verbose-20260616-204952.stdout.jsonl`: SessionStart-hook реально сработал в настоящем
  Claude (`hook_response.output` = «Core xiigen boundary executed. Semantic quality is not claimed…»), НО `result` =
  `subtype:"error_max_budget_usd"`, `is_error:true`, «Reached maximum budget ($0.05)»; модель ответила
  `RUNTIME_02_CLAUDE_VISIBLE_SMOKE` вместо требуемого `XIIGEN_RUNTIME_02_VISIBLE` (grep=0). При этом `mirror.json` штампует
  `verdict:PASS_NARROW_CLAUDE_CODE_PLUGIN_LOAD_AND_HOOK_CORE_BOUNDARY_RUNTIME_QA` → **OVERCLAIMED-PASS**.
- **Path B ограничен фиксированной hook-строкой — same-scenario НЕТ.** Вывод hook’а — константа, не зависит от `-p` промпта.
  Нет ни одного сценария, где XIIGen меняет ВИДИМЫЙ ответ Claude на вопрос пользователя. `README.md:77` «Full interactive Claude
  host runtime is not proven».
- **UX-automation для Claude отсутствует и неприменима** (0 UI-поверхности, P09).
- **QA невоспроизводима как гейт:** `package.json:45` `devDependencies:{}`, scripts={install-smoke} (нет `test`), нет `test/`/
  jest/mocha/vitest; выход QA пишется в ДРУГОЙ репо (`C:\Projects\llm_mvp_core\artifacts\host-qa\…`), дата 2026-06-06.
- **Коррекция мастера (нюанс):** RUNTIME-02 (Jun 16-17) — реальный, более поздний прогон `claude.exe`, где плагин ГРУЗИТСЯ через
  `--plugin-dir` и SessionStart-hook РЕАЛЬНО срабатывает и доходит до core-boundary. Сильнее phase40 echo-run (P08/P09). Но
  по-прежнему hook-load-boundary, а не user prompt→answer; единственная попытка видимого ответа дала error+marker-mismatch.

---

### CURSOR HOST — 4 прохода (P13 entrypoint · P14 pipeline · P15 token · P16 qa)

Репо: `C:\Projects\cursor-xiigen-smart-skills`. **САМОЕ зрелое из трёх** — настоящий VS Code/Cursor extension package:
`activate()`, 3 контрибьютнутые команды (`xiigen.smartSkills.select`, `xiigenSmartSkills.panel`,
`xiigenSmartSkills.runtimeProof.invoke`), chat participant `xiigen-smart-skills`, реальный webview-панель
(textarea + Submit + postMessage round-trip, CSP+nonce), `configuration` из 11 свойств, компилированный `dist/extension.js`
(48148 байт из `src/extension.ts` 1346 строк). Self-flag: `package.json` `version:"0.0.0-xiigen-wiring"`,
`runtimeEvidence:false`, `fullHostImplementationEvidence:false`; contract `runtime_host_proof_performed:false`,
`host_prompt_to_task_implementation:false`, «static package/boundary evidence only». **ЭТО ЛУЧШАЯ СТАРТОВАЯ ТОЧКА для плана
продолжения VS/VS Code** (реальный extension.ts + собранный dist), но с 8 структурными дырами и провальным token/QA-контуром.

#### P13 — Cursor entrypoint / настройки / core-API — 8 НОВЫХ дыр

- **G1 (STUB TRANSPORT / WINDOWS-ONLY)** — `extension.ts:661` `spawn("powershell.exe", ["-NoProfile","-ExecutionPolicy","Bypass",
  "-File",coreApiScript, …])`; `dist/extension.js` содержит `powershell.exe` (grep=1). Cursor кроссплатформенный — на
  macOS/Linux `powershell.exe` не спавнится, вся core-граница недостижима даже при верном пути. (Отдельно от P08 «путь на чужую
  машину» — здесь «даже с верным путём не-Windows не может».)
- **G2 (NON-SHIPPABLE DEFAULTS)** — `package.json:49-58` `contributes.configuration`: `coreRoot.default="C:\\Projects\\llm_mvp_core"`,
  `coreApiScript.default="C:\\Projects\\llm_mvp_core\\tools\\xiigen-extension-core-api.ps1"`. Каждая установка приезжает с дефолтом
  на конкретную dev-машину; нет discovery/бандла core. `extension.ts:313-314` читает эти дефолты.
- **G3 (SETTINGS-SCHEMA DRIFT)** — 3 ключа читаются кодом, но НЕ объявлены в манифесте: `locatorCandidateId` (`:500`),
  `targetCandidateId` (`:503`), `rollbackCandidateId` (`:506`) — их нельзя задать через Settings UI. 2 объявлены, но НИКОГДА не
  читаются: `promptSuite` (`package.json:59`), `traceReport` (`:64`) — мёртвые настройки.
- **G4 (PRODUCTIZATION DEFECT)** — тест-харнес в продуктовом манифесте+палитре: `package.json:34-38`
  `xiigenSmartSkills.runtimeProof.invoke` title «XIIGen: Runtime Proof Harness (Test Only)» + `activationEvents:onCommand`
  (`:18`) → конечный пользователь видит харнес в Command Palette. Плюс `extension.ts:180` регистрирует скрытую
  `xiigenSmartSkills.__test.chatHandlerLevel` (нет в манифесте).
- **G5 (UNSTABLE-API / UNPROVEN CHAT)** — `package.json:40-45` объявляет chat participant `xiigen-smart-skills`, но
  `extension.ts:162-177` кастует `vscode as unknown` до `chat?.createChatParticipant` и МОЛЧА пропускает регистрацию, если API
  нет; манифест не объявляет `enabledApiProposals`. `docs/PROJECT-DOC.md:600` «Full interactive Cursor chat participant
  invocation remains distinct…». QA `HOST-08C` — только discovery.
- **G6 (BY-CONSTRUCTION NO SEMANTIC ANSWER, host layer)** — `extension.ts:854-856` fallback concreteAnswer = «XIIGen Cursor
  `<type>` entrypoint invoked llm_mvp_core core API; semantic quality is not claimed.»; `:879` безусловно
  `semanticQualityClaimed:false`, `:872-873` `trueHostRuntimeEvidence:false`. Показ через `showInformationMessage` (`:143`) —
  тост со статус-строкой, не семантический ответ. (Отдельно от P10 dead-gate в ядре — здесь ВЫХОДНОЙ слой Cursor.)
- **G7 (NON-REPRODUCIBLE BUILD)** — нет `tsconfig.json`, `esbuild.js`, `.vscodeignore`, `@types/vscode`; `package.json:150-151`
  `dependencies:{}`,`devDependencies:{}`; scripts (`:106-109`) = только `install-smoke` + `verify-docs-final-coverage-nonhost`
  (нет `compile`/`build`). 48 КБ `dist` собран внешним/глобальным тулчейном — чистый checkout пересобрать не может.
- **G8 (DIVERGENT COPY / STALE / WRONG SCRIPT)** — committed `phase40-cursor-core-trace-1.json`: пути =
  `C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\…` (не репо под ревью); `scriptPath` =
  `…tools\`**`phase40-extension-core-api.ps1`** — ДРУГОЙ скрипт, чем поставляемый в конфиге (`xiigen-extension-core-api.ps1`,
  `package.json:56`). `phase40-cursor-entrypoint-response-1.json`: `hostRuntimeInvoked:false`.

**Подтверждения (не новые дыры):** core-скрипт доходит до `XiigenExtensionApiService` через `dotnet run` temp-csproj
(компиляция на вызов, непоставляемо, P09); `normalizeMethod` (`:375-383`) молча коэрсит неизвестный метод в `prompt-to-task`;
панель (`buildPanelHtml:1103-1194`) — настоящий webview, но реальный прогон в Cursor не доказан, при открытии шлёт ЗАШИТЫЙ
промпт (`:207`).

#### P14 — Cursor pipeline

- **Три пользовательские поверхности гонят конвейер, но ВСЕ с дефолтами → только `prompt-to-task`.** Команда
  `xiigen.smartSkills.select` (`:124-146`) берёт промпт из `showInputBox`, метод/скиллы только если вызвана ПРОГРАММНО
  (`:126-128`); при ручном вызове `method=undefined`. Chat participant (`:171-177`) и панель submit (`:237`) зовут БЕЗ options.
  → `normalizeMethod(undefined)`=`prompt-to-task`. `orchestration-plan`/`skill-selection` достижимы лишь тест/объект-вызовом.
- **`normalizeMethod` РОНЯЕТ `feedback` и `debug-trace`.** Разрешённый набор Cursor = ровно `{prompt-to-task, orchestration-plan,
  skill-selection}` (`:379`); всё остальное → `prompt-to-task` (`:382`). Контракт объявляет 5 методов, включая
  `FeedbackMethod="feedback"` (единственный online-сигнал adaptive DPO) и `DebugTraceMethod` (`Contracts.cs:64-65`, `:69-72`).
  Cursor физически НЕ может отправить `feedback` в ядро — метод переписывается ДО `buildCoreRequest`. (Отдельно от P11 «ядро не
  обрабатывает feedback» — здесь хост роняет метод ещё ДО ядра.)
- **Каталог скиллов никогда не формируется.** `availableSkills`/`requestedSkills` только из options; хост сам декларирует
  `host_skill_selection_binding:"pass_through_only"` (`:444`), `..._no_host_selector` (`:460`). Ядро → `NO_AVAILABLE_SKILLS_
  PROVIDED`, conf 0, `RequiresSecondPass true` (`:361-371`). Панель HTML (`:1131-1136`) — только `<textarea>` + `Submit`,
  нет селектора метода/каталога.
- **Панель авто-гонит зашитый промпт как evidence.** При открытии `xiigenSmartSkills.panel` СРАЗУ core-запрос на зашитый
  «A Cursor user opens the XIIGen Smart Skills panel…» (`:207-208`) + `recordPanelUiEvidence(result,false)` (`:218`) — ДО любого
  ввода пользователя. Реальный промпт течёт только после Submit.
- **Стадия 3 не потребляет выход стадий 1-2 (дефект целостности).** Стадия 2 берёт выход стадии 1:
  `BuildOrchestrationRequest(request, promptToTask.StructuredTask)` (`:130`). Но стадия 3 `DecideSkills(request, promptSha256,
  activeLoadState)` (`:143`) НЕ принимает `StructuredTask`/`ScenarioPlan`; `SkillSelectionRequest.Prompt =
  request.UserPromptOrTaskText` (`:375`) = СЫРОЙ промпт. «task/plan→skills» разорван на 3-м хопе — это НЕ настоящий
  prompt→task→plan→skills конвейер.

#### P15 — Cursor token savings (row12) + quality guard — ГЛАВНАЯ token-находка

**Ключевой вывод:** на уровне КОДА в core РЕАЛЬНО есть движок учёта токенов (`TokenTraceAccounting`) и рубричный скорер
качества (`ExtensionQualityScoring`) — но оба отсоединены, а эксперимент по построению не может показать дельту.

1. **Движок учёта токенов ЭВРИСТИЧЕСКИЙ и отсоединён.** `TokenTraceAccounting.cs:60-62`
   `HeuristicTokenizerName="xiigen-heuristic-word-punctuation"`; `:101-127` `CountHeuristicTokens` считает переходы
   «слово/пунктуация» — **НЕ токенизатор модели/хоста** (`trace != savings`); `:66-99` `CreateHeuristicTrace` возвращает
   `QualityScore:null`, `TokenSavingsClaimed:false`. Callers (git grep) = только `Tests` + одноразовая
   `measurement-20260625T004435/Program.cs`. `git grep TokenTraceAccounting -- XiigenExtensionApiService.cs` → **0**; grep в
   `cursor…/src` и в `core-api.ps1` → **0**. **Поставляемый Cursor row12/TOKEN-03-путь не эмитит ни одного числа токенов.**
2. **baseline↔adaptive БЕЗ независимой переменной ⇒ delta структурно 0.** `Program.cs:66-67` `baseline=InvokeMode("baseline");
   adaptive=InvokeMode("adaptive")`; обе ветви (`:247-393`) строят запрос с ОДНИМ И ТЕМ ЖЕ `promptText` (`:261`), method,
   skills; различие ТОЛЬКО `HostFacts["token_mode"]=mode` (`:269`). Ядро НЕ ветвится на `token_mode` (grep=0) →
   `token_reduction_observed=false`, `VisibleAnswerText` — константа в обеих ветвях. Тот же дефект в host-QA:
   `token03-extensionHostFullPackQA.js:5` цикл по `["baseline_common_model_only","adaptive_common_plus_user_model"]`, но
   `executeCommand('xiigen.smartSkills.select',{prompt,method})` получает ТОЛЬКО `{prompt, method}` — `mode` в команду не
   передаётся. Заголовки раннеров жёстко пишут `token_savings_claim_allowed:false`, `adaptive_effect_claimed:false`.
   **Нужна РАЗНАЯ величина входа (baseline=сырой промпт vs adaptive=xiigen-сжатый контекст); сейчас идентичный вход дважды.**
3. **14-assertion quality-guard row12 НЕ может пройти по построению.** `Program.cs:546-562` требует, чтобы ответ ядра содержал
   marker/scenario_id/`task_id`/3 упорядоченных шага/per-step verification/`SelectedSkills.Count>0`/секции references. Реальный
   ответ = not-ready-граница (`NO_AVAILABLE_SKILLS_PROVIDED`, `loaded_state_no_matching_learned_pattern`, `SelectedSkills:[]`).
   Ассерты 3-14 падают; `:97-104` `finalStatus=MEASUREMENT_NEEDS_FIX`; `:217-222` «Do not increment passed scenario count for
   row 12 … keep in needs-fix/blocked». **Guard стоит вниз по потоку от not-ready-стаба ядра (P02/P10/P14).**
4. **Reusable quality-скорер — человеческая рубрика, eval-only; heuristic quality-gate — вечный fail.**
   `ExtensionQualityScoring.cs:45-48` требует ЧЕЛОВЕЧЕСКУЮ заметку ревьюера на каждый из 4 слоёв (ручной процесс); `:66-78`
   `QualityRegressed` — булев ВХОД, не измеряется. Callers = eval-only-runner’ы + Tests, в рантайм Cursor не подключён.
   `TokenTraceAccounting.CreateHeuristicTrace:87-88` всегда null QualityScore/Regression → `EvaluateQualityGate:179-201`
   всегда добавляет `token_delta_alone_cannot_pass_quality` + `quality_regression_requires_approved_quality_rubric` ⇒
   `CanPass=false` ВСЕГДА.

Doc-подтверждение (`docs/CURSOR-DOCS-FINAL-COVERAGE-TOKEN-QUALITY-NONHOST.md`): `:28` `token_savings_claimed=false`, `:30`
«trace != savings», `:44` `quality_improvement_claimed=false`; Token accounting boundary = PARTIAL, Quality non-regression =
PARTIAL. Будущий token-PASS требует same-pair (same host/prompt/scenario/method/tokenizer-version + baseline/adaptive refs +
non-regression) — НИЧЕГО из этого не доказано.

#### P16 — Cursor QA / dual-path — 7 дыр

- **Path B «live desktop» = headless extensionTests, НЕ видимая сессия.** Единственный реальный запуск `Cursor.exe` (RUNTIME-03,
  2026-06-18): `Cursor.exe --extensionDevelopmentPath … --extensionTestsPath …runtime03-extensionHostQA.js --user-data-dir …
  --headless --new-window` = Extension **Test** Host в `--headless` (авто JS-прогон без видимого chat UI). `mirror.json`
  `full_cursor_chat_ui_proven:false`, verdict `PASS_NARROW_…_PARENT_REVIEW_PENDING`. По ВСЕМУ репо `find *.png/*.jpg/*.mp4/*.gif`
  → **0**. Требование Path B (видимый маркер + скрин/видео) не выполнено конструктивно.
- **Сильнейший «прошедший» Cursor host-QA исполнен в VS CODE, не в Cursor (WRONG-HOST SUBSTITUTE).**
  `host-impl-cursor-01-summary.json` `successful_runner:"VS Code Extension Host dev-load path for Cursor extension repository"`;
  чек `code.cmd --extensionDevelopmentPath … --headless` = `PASS_BY_EXTENSION_HOST_REPORT`; прямой `Cursor.exe
  --extensionTestsPath … --headless` = `NO_REPORT_NOT_COUNTED_AS_PASS`. `claims_not_proven[0]`=«Full Cursor UI/chat participant
  invocation through a user-visible Cursor chat surface».
- **Панель (единственная видимая UI) НИ РАЗУ не гонялась в реальном Cursor.** RUNTIME-03 `extension-host-report.json`
  `panel_case:null`. Артефакты `xiigenSmartSkills.panel-*` есть ТОЛЬКО под `HOST-IMPL-CURSOR-01/vscode-extension-host/…` (VS Code).
- **«Phase 10 Cursor full extension live proof» НИКОГДА не исполнялась и гоняет test-only команду.**
  `test/phase10/phase10CursorFullExtensionProof.js:54` ассертит `entrypointName==="xiigenSmartSkills.runtimeProof.invoke"` (это
  «Runtime Proof Harness (Test Only)», P13 G4), а не пользовательские `xiigen.smartSkills.select`/`panel`. Выходная папка
  `reports/PHASE-10-cursor-proof/` ОТСУТСТВУЕТ → харнес не прогнан. Даже прогнав: `assertCommonResult:51-138` проверяет
  entrypointInvoked/имя/тип/`semanticQualityClaimed===false`/correlation/redaction, но НЕ сверяет содержимое с эталоном. Verdict
  `PHASE_10_CURSOR_PROOF_READY_FOR_REVIEW`, `passed:true`.
- **Ни одного ORACLE корректности ответа во всей Cursor-QA.** grep `test/`: единственный ассерт о содержимом =
  `assert.strictEqual(result.semanticQualityClaimed, false)` (`HOST-08B:40`, `HOST-08D:48`, `HOST-08H:48`,
  `HOST-IMPL-CURSOR-01:90`, `FRDP-row12:123/183/187`). QA доказывает, что расширение НЕ заявляет ум, а не что ответ ВЕРЕН.
  checks-mirror RUNTIME-03 = 7 плумбинг-проверок. Видимый `concreteAnswer` = редакт-дамп «Adaptive task+plan+skills ready.
  Semantic quality is not claimed… selectedSkills:[]… No available skills…».
- **Все Cursor-QA вердикты — `PASS_PENDING_PARENT_REVIEW` / `PARENT_REVIEW_PENDING` (НЕ приняты).** `HOST-08B:5`, `HOST-08H:5`,
  RUNTIME-03 `mirror.json`, phase10 `READY_FOR_REVIEW`. Ни один не имеет записанной приёмки; board (STATUS-BOARD:26, фаза 6
  PARTIAL) опирается на непринятые pending-review проходы.
- **QA-набор ОСИРОТЕВШИЙ (нельзя перезапустить как гейт).** `package.json` scripts={install-smoke, verify-docs-...},
  deps/devDeps `{}`, `.github` нет, нет `test`-скрипта и раннера (node --test/vitest/jest/mocha). 10 QA-файлов запускаются только
  ad-hoc через host `--extensionTestsPath`/`node --check`. Контраст с Claude (P12: файлов не было вовсе) — здесь файлы ЕСТЬ, но
  без упряжи/CI.
- **Нюанс негатива:** у Cursor ЕСТЬ один controlled-error кейс (`CORE_ENDPOINT_MISSING`) в RUNTIME-03 и HOST-IMPL — поэтому «нет
  негативных кейсов» (как у Claude P12) для Cursor не заявляется; дыра точнее — негатив только ТРАНСПОРТНЫЙ, не
  семантический/degraded-answer.

---

### Real-vs-stub свод по двум хостам (для плана VS/VS Code)

| Слой | Claude (P09-12) | Cursor (P13-16) |
|---|---|---|
| Host entrypoint | СКЕЛЕТ — только SessionStart-hook, нет user-команды/skill/MCP/settings (P09 G1) | РЕАЛЬНЫЙ — `activate()`, 3 команды, chat participant, webview-панель, 11 настроек (самый зрелый) |
| Manifest version | `0.0.0-phase40-skeleton` (само-скелет) | `0.0.0-xiigen-wiring`, `runtimeEvidence:false` |
| Собранный артефакт | нет `dist`, 0 `.ts` (hand-js) | `dist/extension.js` 48 КБ, но НЕвоспроизводим из репо (P13 G7) |
| Транспорт к ядру | `spawnSync powershell.exe` → temp-csproj `dotnet run` (компиляция на вызов, Windows-only, абсолютные пути) | `spawn powershell.exe` (Windows-only, P13 G1) + абсолютные дефолты `C:\Projects\...` (G2) |
| Семантика в ответе | dead-gate в ядре (P10) + hook echo | dead-gate в ядре + фабрикованный шаблон в host-слое (P13 G6) |
| Каталог скиллов / стадия 3 | пустой каталог → постоянный no-op (P10) | «no host selector», pass-through → постоянный no-op (P14) |
| Методы конвейера из хоста | 1/3 (только prompt-to-task) | 1/3 интерактивно; `feedback`/`debug-trace` роняются `normalizeMethod` (P14) |
| Online-обучение (`feedback`) | метод не реализован в ядре + не вызывается (P11) | роняется хостом ДО ядра (P14) |
| Lifecycle export/import/backup | 0 команд; core two-leg lifecycle есть, но только в тестах (P11) | 0 команд (та же картина) |
| Token savings | — (не срез Claude) | движок эвристический + отсоединён; baseline≡adaptive без независимой переменной → delta 0; guard=needs-fix (P15) |
| QA | 1 отчёт, node.exe boundary (не плагин), happy-path, 1 error-run помечен PASS (P12) | 10 файлов, реальный Cursor.exe, но headless без UI; strongest QA в VS Code-суррогате; нет oracle; все pending-review (P16) |
| Path B (видимый desktop) | фиксированная hook-строка, error+marker-mismatch, 0 скриншотов | headless extensionTests, панель не покрыта в Cursor, 0 скриншотов |
| Итоговый статус среза | **BUILT_NOT_PROVEN скелет** | **BUILT_NOT_PROVEN (зрелый extension, провальный proof-контур)** |

**Сквозной вывод для VS/VS Code-плана:** Cursor — форк VS Code и единственный хост с реальным `extension.ts` + собранным
`dist`, поэтому это правильный старт для порта на Visual Studio Code. Но его нужно: (1) отвязать от Windows-only PowerShell-
транспорта и абсолютных путей (тёплый stdio/HTTP-транспорт вместо per-call `dotnet run`); (2) починить дрейф схемы настроек,
убрать test-only команду из палитры, дать метод-селектор в панели/командах; (3) сделать `dist` воспроизводимым (`tsconfig`+
`esbuild`+`@types/vscode`+CI); (4) подать реальный каталог скиллов, чтобы стадия 3 перестала быть no-op; (5) подключить настоящий
токенизатор и разные baseline/adaptive-входы для token-дельты; (6) ввести oracle корректности + видимый Cursor-прогон (не
headless). **Но всё это downstream заблокировано мёртвым semantic-quality гейтом в ядре (P10) и словарь/счётчик-моделями (P05)** —
пока ядро false-by-construction, ни один host не даст семантического ответа. Claude-хост придётся строить почти с нуля (реальная
команда/skill/MCP), используя Cursor как паттерн, ЛИБО принять решение Luba о суженной области Claude-расширения.

---

### Границы честности прохода 3

- claim_scope: полное чтение opus-pass-09..16 + извлечение per-host build-reality (Claude 4 прохода, Cursor 4 прохода) с
  file:line; real-vs-stub свод; token-находка Cursor. Русский, read-only.
- evidence_scope: прочитаны ПОЛНОСТЬЮ 8 файлов opus-pass-09..16; выводы привязаны к file:line ИЗ ЭТИХ ПРОХОДОВ. Сам код
  host-репо и `llm_mvp_core` в этом проходе НЕ читался напрямую (цитируется по Opus-аудиту — родитель верифицирует). Codex-хост
  (P17-20) — вне слайса прохода 3. Тесты/сборки/git/runtime НЕ запускались.
- НЕ доказано этим проходом: любые product-readiness утверждения; точность каждого per-pass file:line-вывода (нужна верификация
  родителя против живого кода).
- subagent_id: UNKNOWN_IN_SUBAGENT. requested_model: gpt-5.5-equivalent (Opus 4.8 Ultracode). requested_reasoning_effort: xhigh.

## Pass 4/10 — opus-pass 17-24: Codex host (entrypoint/pipeline/learning/qa) + machine-pipeline (prompt-to-task/task-to-plan/task-to-skills/dpo-rows)

Границы прохода 4: прочитаны ПОЛНОСТЬЮ восемь файлов opus-pass-17..24. Первая четвёрка (P17-20) закрывает ТРЕТИЙ host —
Codex (repo `C:\Projects\codex-xiigen-smart-skills`); вторая (P21-24) — впервые вскрывает РЕАЛЬНОСТЬ core-«машинного» конвейера
prompt→task→plan→skills→dpo на уровне живого C#-кода `43.SmartSkillsDomain` + промоутнутых ассетов `models/*` + датасетов на
диске `training-data/*`. Это извлечение доказательств file:line из каждого прохода, не сводка. Read-only; ничего не запускалось.
Автор: Claude Opus 4.8 (Ultracode), одиночный bounded-агент, читал и писал сам. Все file:line-ссылки — из Opus-аудита (родитель
верифицирует против живого кода). Шкала зрелости прежняя: `MISSING` → `COPIED_DOC_ONLY` → `BUILT_NOT_PROVEN` → `PROVEN`.

**Главный итог прохода 4 одной фразой:** Codex — САМЫЙ незрелый host по host-достижимости (несмотря на JS-entrypoint и `test/`),
а core-«машины» prompt→task→plan→skills на РЕАЛЬНОМ коде оказались тремя детерминированными lexical/lookup-механизмами со
структурным разрывом словарей между стадиями и почти нулём реальных online-принятых DPO-строк. Ни один из 8 проходов не поднимает
статус выше **BUILT_NOT_PROVEN**; PROVEN dual-path для Codex = 0; семантический task→plan «мёртв на прибытии» на реальном промпте.

---

### CODEX HOST — 4 прохода (P17 entrypoint · P18 pipeline · P19 learning · P20 qa)

Репо: `C:\Projects\codex-xiigen-smart-skills`. Единственный «current» entrypoint — рукописный `src/codex-entrypoint.js`
(545 строк, JS ESM, `type:module`) + legacy `src/codex-entrypoint.ps1` (90 строк, шеллит `node.exe codex-entrypoint.js`). Три
расходящихся манифеста. Тот же непоставляемый транспорт (PowerShell-шим → per-call `dotnet run` core), что у Claude/Cursor.

#### P17 — Codex entrypoint / desktop / chat / app + настройки / core-API — 8 НОВЫХ дыр

- **G1 (HOST-REGISTRATION MISSING — центральный факт Codex).** У Codex ЕСТЬ настоящая плагин-система, но она **marketplace-only**,
  и XIIGen в ней НЕТ. Реальный `codex-cli 0.136.0-alpha.2` (`CX-04R-host-runtime-path-report.json`) имеет
  `codex plugin add/list/marketplace/remove` («Install a plugin from a configured marketplace snapshot»). `codex plugin list`
  (decoded `CX-04R-probe/discovery/.../local-codex-plugin-list.stdout.log`) перечисляет `documents/spreadsheets/presentations@
  openai-primary-runtime`, `browser@openai-bundled` + сотни `@openai-curated` — **`xiigen`/`codex-xiigen-smart-skills` отсутствует
  полностью** (grep xiigen → 0). `HOST-08F-codex-plugin-app-qa.json` сам фиксирует
  `codex_plugin_command_scope:"marketplace_management_only_from_help"`, `direct_local_plugin_app_invoke_path_found:false`. Значит:
  (а) команды «invoke локально-авторского плагина» в Codex НЕТ, только управление marketplace; (б) `.codex-plugin/plugin.json` —
  НЕ тот формат, что Codex потребляет (он читает marketplace-snapshot `marketplace.json` под `~/.cache/codex-runtimes/.../.agents/
  plugins/`); (в) XIIGen-плагин никогда не был зарегистрирован/установлен. Собственный вердикт сессии:
  `CX-04R...passed:false`, `verdict:"BLOCKED_OP13_NO_SAFE_NON_AI_CODEX_PLUGIN_HOST_INVOCATION_PATH"`, `plugin_loaded_invoked:false`,
  `true_codex_host_runtime_path_found:false`. → «Codex extension» как загруженный/вызванный host-плагин НЕ существует.
- **G2 (DEAD VISIBLE-ANSWER / runCli не выводит ответ).** `codex-entrypoint.js:528-539` `runCli` пишет только `result.stderr`
  (`:531`) и `result.stdout` (`:535`) — сырой (пустой) stdout дочернего `powershell.exe`. НИКОГДА не зовёт
  `createVisibleAnswerDisplay` (`:410-441`) или `createDebugTraceDisplay` (`:393-400`). А `xiigen-extension-core-api.ps1` пишет
  ответ в ФАЙЛ `-ResponsePath`, не в stdout ⇒ прямое объяснение P08-факта `phase40-codex-host-session-run{,-2,-3}.stdout.jsonl=0
  байт`: пользователь Codex не увидел бы НИЧЕГО. Хуже Claude P09 (там stdout=chatDisplayText, но в чужой файл) — здесь видимый
  ответ не рендерится вообще, весь код видимого ответа/redacted-debug экспортирован, но мёртв.
- **G3 (MANIFEST-TRIPLICATION / противоречие статуса).** `package.json` (v0.1.1, `requestWiringImplemented:true`,
  `sourceEntrypoint:"src/codex-entrypoint.js"`, `powershellEntrypointAcceptedAsImplementation:false`); `.codex-plugin/plugin.json`
  (v`0.1.0+codex.runtime04e-20260616-231500`, `skills:./skills/`, `capabilities:[Interactive,Write]`, `defaultPrompt`);
  `.codex/xiigen-smart-skills.json` (v`0.0.0-phase40-skeleton`, `implementation_status:"skeleton_only"`,
  `entrypoint_implemented:false`, `install_smoke_status:"not_run"`, `entrypoint.implementation_file:"src/codex-entrypoint.ps1"` —
  тот PS1, что package.json объявляет legacy). Три версии + прямое противоречие «реализовано vs skeleton_only/not_run»; читатель
  не может определить каноничный статус/точку входа.
- **G4 (DEAD DEBUG SETTING, Codex-специфично).** `package.json:56-63` debugSetting + `xiigen-extension-contract.json:53-62`, но
  `DebugEnabled` читается ИСКЛЮЧИТЕЛЬНО из CLI-флага `--debug-enabled` (`:504`); ни в `.codex-plugin/plugin.json`, ни в `.codex/`
  нет секции settings; host-командной поверхности, чтобы пробросить флаг, нет; дисплей всё равно мёртв (G2). → redacted debug
  недостижим для пользователя Codex.
- **G5 («INSTALLED PLUGIN» = агентский SKILL.md).** «Инсталлированный плагин» доказывается тем, что LLM-агент ВРУЧНУЮ шеллит
  `node .\src\codex-entrypoint.js --method prompt-to-task ...` по инструкции `skills/xiigen-smart-skills/SKILL.md` «Smoke
  Invocation» (зависит от `C:\Projects\llm_mvp_core`). Declared `skills:./skills/`, `capabilities:[Interactive,Write]`,
  `defaultPrompt` не потребляются ни одним доказанным host-путём (`HOST-08F` `full_plugin_ui_evidence:false`).
- **G6 (STALE/DIVERGENT PARAM-КОНТРАКТ + 3/4 упали).** Единственный непустой `phase40-codex-host-session-run-4.stdout.jsonl` (UTF-16)
  — Codex-CLI `command_execution` запускает `powershell.exe -Command powershell.exe ... -File .\src\codex-entrypoint.ps1
  -ConcreteTask ... -Prompt ... -OutputPath ... -TracePath ...`. Но committed `src/codex-entrypoint.ps1` (`:2-26`) объявляет
  `-Method/-UserPromptOrTaskText/-RequestPath/-ResponsePath/-TracePath` — параметров `-ConcreteTask/-Prompt/-OutputPath` там НЕТ;
  `aggregated_output:""`, `exit_code:null`; `-4.last-message.txt`=«Shell exit code 0; XIIGEN_CODEX_HOST_SMOKE_DONE» (внешний echo).
  Runs 1/2/3 `stderr.log`=`codex.exe : error: unexpected argument '-a' found`. → double-shell echo-smoke против расходящегося
  контракта ps1, 3/4 прогонов — ошибка codex.exe.
- **G7 (CACHE-COPY RUNTIME / ACCESS-DENIED / version divergence).** `RUNTIME-04-codex-live-runtime-mirror.json`:
  `codex_executable:"...WindowsApps\OpenAI.Codex_26.609.4994.0...\codex.exe"` (иной, чем discovery `...AppData\Local\OpenAI\Codex\
  bin\...` v0.136.0-alpha.2); `installed_cache_root:"...\.codex\plugins\cache\xiigen-local\...\0.1.0+codex.runtime04e..."`,
  `cache_entrypoint` (node из копии); `codex_cli_shell_scope:"Access denied; not used for model-backed send-message."`;
  `proof_scope:"narrow installed-cache/plugin-entrypoint runtime evidence only; full Codex chat UI unproven"`. → «инсталляция» =
  копия файлов репо в cache + ручной node, а НЕ загрузка/инвокация Codex-хостом; model-backed путь Access denied.
- **G8 (inferAssetRoot — per-user adaptive конфляция).** `codex-entrypoint.js:279-284` `userRoot = ... ?? raw?.
  activated_asset_root ?? raw?.UserAdaptiveAssetRoot ?? ... ?? inferAssetRoot(targetRefs)`; `inferAssetRoot` (`:211-220`) режет
  первый строковой ref по `/(prompt-to-task|orchestration-reasoning|skill-selection)/`. → personal adaptive leg склеивается с
  common promotion target (`activated_asset_root`) и/или выводится из COMMON target-refs; реального per-user adaptive-состояния
  не подключено (Codex-инстанс дефекта Claude P11 gap-4, усиленный эвристикой).

**Вывод P17:** Codex — самое незрелое из трёх по host-достижимости. Есть marketplace-плагин-система, но XIIGen не зарегистрирован
(отсутствует в `codex plugin list`), `.codex-plugin/plugin.json` — не тот формат; runtime-path-report сессии
`BLOCKED...plugin_loaded_invoked:false`. Ни один слайс не доказывает prompt→submit→visible answer через реальный Codex-хост.

#### P18 — Codex pipeline (prompt→task→orchestration→skills) — 4 НОВЫЕ дыры

- **PIPELINE-DEFECT (метод не валидируется, косметичен).** Codex `optionsFromCliArgs:501` `method:parsed.method` (без нормализации)
  → `createXiigenExtensionRequest:108` `?? 'prompt-to-task'`. Ядро `XiigenExtensionApiContracts.cs:112-115` проверяет ЛИШЬ
  `IsNullOrWhiteSpace(Method)` и НЕ сверяет с `MethodNames:67-74`; `NormalizeMethod:1000-1003` только тримит; неизвестный/опечатанный
  метод падает в `_ =>` ветвь дисплея (`:972-973` «Method {method} produced task, plan, and skill evidence.»). **Codex — ЕДИНСТВЕННЫЙ
  хост, отдающий сырой метод пользователю** (Cursor P14 — whitelist из 3; Claude P10 — метод не задаётся вовсе). Диспетчер
  `ProcessHostExtensionRequest:117-154` безусловно гонит Translate→Plan→DecideSkills для ЛЮБОГО метода; метод меняет только
  display-текст → `feedback`/`debug-trace` тоже сваливаются в ту же task/plan/skills-цепочку (нет intake обратной связи).
- **NO-OP SKILLS STAGE (само-ссылочный каталог).** `SKILL.md` smoke передаёт `--available-skills-json '["xiigen-smart-skills"]'` +
  `--requested-skills-json '["xiigen-smart-skills"]'` — ИМЯ САМОГО ПЛАГИНА, не индекс скиллов → `SelectedSkills:[]` (TOKEN-03
  `TQ-007.../skill-selection/baseline_common_model_only/response.json`). Единственный прогон с настоящим каталогом — RUNTIME-04B:
  `AvailableSkills:["architect","planning","plan-review"]`, `RequestedSkills:["planning"]` → `selectedSkills:["plan-review"]`, но
  каталог был ВРУЧНУЮ вписан в smoke-команду. Механизма перечисления реального repo/user skills-индекса на Codex-хосте нет — вход
  стадии 3 всегда только из CLI `--available-skills-json:506`. (Отдельно от P10 Claude пустой каталог и P14 Cursor no-selector.)
- **PARTIAL (1/3 методов поставляемы).** `SKILL.md` smoke = только `prompt-to-task`; `defaultPrompt` без метода. `orchestration-plan`
  и `skill-selection` встречаются только в agent-shell `command_args` внутри TOKEN-03/RUNTIME-04
  (`node .\src\codex-entrypoint.js --method orchestration-plan`). Мульти-метод гоняется лишь тем, что LLM-агент руками впишет `--method`.
- **DECLARED-NOT-WIRED (манифестный интерактивный вход).** `.codex-plugin/plugin.json` объявляет `capabilities:[Interactive,Write]`
  и `defaultPrompt`, но `grep defaultPrompt|capabilities|plugin.json` по `src/` = 0 — entrypoint НЕ читает манифест; вход конвейера
  только из CLI `--user-prompt-or-task-text:502`. Сами `defaultPrompt` («Send this task to XIIGen smart skills.», «Show the XIIGen
  debug trace for this task.») — мета-инструкции, не текст задачи; при потреблении prompt-to-task классифицировал бы фразу-инструкцию.

**Подтверждения P18 (не новые дыры) на Codex-пути:** стадия 2 ломается тем же `ORCHESTRATION_REASONING_MANIFEST_CHECKPOINT_REF_
MISMATCH` (RUNTIME-04B); стадия 1 = `loaded_state_no_matching_learned_pattern`, стадия 2 = `not_trained_no_learned_guide_state`;
авторитет выбора скиллов — счётчик позиции/контекста (`skill_selection_evidence: prompt_score=0, context_score=4, total_score=8`),
т.е. order-leakage counter (P02) подтверждён на Codex-пути; baseline vs adaptive семантически идентичны (P15).

#### P19 — Codex learning (learnability) + locator update — 4 НОВЫЕ дыры

- **MISSING WIRING (host lifecycle).** У Codex НЕТ ни одной поверхности локального обучения/жизненного цикла. scripts = только
  `install-smoke`/`source-wiring-check`; `skills/xiigen-smart-skills/SKILL.md:3` прямо пишет «does not prove ... learnability,
  training, DPO, checkpoint mutation», `:42/:47` booleans `learnability_proven/training_started/checkpoint_mutation_started` — «Only
  the first two may be true». Манифесты: `host_training_allowed:false`, `modelTrainingInHost:false`, `dpoGenerationInHost:false`,
  `learnedModelMutationInHost:false`. Рантайм read-only (`codex-entrypoint.js:380/:467`). В `src` НЕТ export/import/backup/
  continue-training/feedback-функций; в репо НЕТ `.xiigen`/adaptive-checkpoint/continue-probe (find→0).
- **DECLARED-OUT-OF-PACKAGE (adaptive-state символическая строка).** `.codex/xiigen-smart-skills.json:39`
  `adaptive_state_root_ref:"codex-host:adaptive-state-runtime-root-outside-package"`, `:41` `adaptive_state_separated_from_host_
  package:true`. Реального per-user adaptive-пути нет.
- **DEAD LOCATOR PATH (never-exercised).** `createTargetLocator` (`:268-318`) строит объект `TargetLocator`, но КАЖДЫЙ поставляемый
  Codex-запрос шлёт `TargetLocator:null` / `target_locator_present:"false"` (`HOST-IMPL-CODEX-01/component-qa/20260617-235258/*/
  xiigen-codex-core-request.json:10,:38`). В отличие от Claude (P09 вшивает test-фикстуру `qsg28-two-unit-test-locator.json`), у
  Codex НЕТ ни одной поставляемой locator-фикстуры (find *locator* → 0). Локатор из Codex не упражняется вообще.
- **NAMING-VS-CAPABILITY (all hosts): «locator update» = закрытый non-claim.** Поле `TargetLocator` в ядре
  (`XiigenExtensionApiService.cs:279-317 ResolveRequestLoadState`) используется ТОЛЬКО чтобы выбрать, из какого каталога ЗАГРУЖАТЬ
  ассеты (`LoadCommonThenUserAdaptiveAssets`); никакой регистрации в `IRemoteUnitLocator`/promotion/мутации реестра нет (grep
  `RegisterAsync`/`IRemoteUnitLocator` в сервисе → 0). Граница печатает `"no training, checkpoint promotion, or locator update
  performed"` (`:818`), `"...locator update claims closed"` (`:664`), `"no checkpoint promotion"` (`:796`). Слово «locator» в
  extension-контракте означает «грузи из этой папки», а не «обнови реестр юнитов» — по построению на любом host-пути.
- **(доп.) STUB ROLLBACK.** rollback-нога = swap каталога через `inferAssetRoot(rollbackRefs)` (`:285-289`), всегда null на
  поставляемом пути, без rollback-фикстуры; live-mirror `production_locator_or_checkpoint_mutated:false`; find *rollback* → 0.

**Коррекция-нюанс к P11/P18 (не новая дыра):** на БОЛЕЕ НОВОМ Codex-probe (`HOST-IMPL-CODEX-01/.../20260617-235258`, 2026-06-17)
OrchestrationReasoning грузится БЕЗ mismatch (`orchestration_reasoning_checkpoint_sha256:dcd7d02b...`, warning отсутствует) — на
Codex-пути load-failure частично починен между probe-датами; старый `HOST-IMPL-03` (Jun16) ещё несёт mismatch. `semanticQualityClaimed
=false` при этом остаётся, learnability не доказана.

#### P20 — Codex QA + dual-path — 7 НОВЫХ дыр

- **G1 (JSON-PARSE-GATE).** Топ-вердикт `HOST-IMPL-CODEX-01/validation.json:4` PASS выведен из `json_parse` 39 файлов (`:6`
  target_count 39, failure_count 0) + `required_evidence` булевы (`:208-227`: `external_ai_false`, `training_started_false`,
  `debug_redaction_passed`, `target_locator_bound`, `git diff --check`). Ни одной проверки корректности/семантики runtime.
- **G2 (PLUMBING-ONLY + тесты на мёртвых функциях).** Единственный исполняемый QA `HOST-08F-codexComponentQA.js` зовёт
  `invokeXiigenCore()` напрямую из `src` (import, не через Codex-хост); `passed:true`, но `codex_plugin_runtime_qa:false`,
  `full_plugin_ui_evidence:false`, `codex_exec_model_backed_qa:false`. Оракул `:141-143` = `exitCode`+`responseSuccess`+
  `exactDebugShape`+`!sentinelLeaked`; ноль ассертов корректности task/plan/skill; `visible-answer.txt` ИДЕНТИЧЕН для
  `prompt-to-task` и `skill-selection` («Core task+plan+skills ready. Semantic quality is not claimed.»). Тест импортирует и зовёт
  `createVisibleAnswerDisplay`/`createDebugTraceDisplay` (`:114-127`), которые runCli НИКОГДА не вызывает (P17 G2) — QA зелёный на
  функциях вне поставляемого runtime-пути.
- **G3 (OVERCLAIMED-VERDICT app-server).** Сильнейший «PROVEN» `RUNTIME-04G-codex/.../summary.json:6`
  `verdict:PASS_APP_SERVER_CHAT_INSTALLED_SKILL_ADAPTER_CORE_BOUNDARY_PROVEN_WITH_NEGATIVE_PATH`, но `:15`
  `positive_turn_wrapper_timed_out_after_positive_artifacts:true`; «финальный ответ» (`:17`) — сама МОДЕЛЬ Codex нарративно
  подтверждает вердикт («Verdict: Yes, within the evidence scope...») = self-judge; boundary всё тот же
  `core_boundary_partial_no_semantic_quality_claim`, `semantic_quality_claimed:false`; send-message-v2 attempt2/3 упали `codex.exe :
  error: unexpected argument 'redacted' found`.
- **G4 (EVIDENCE-HYGIENE / secret leak).** RUNTIME-04G positive: `raw_private_prompt_persisted:true` (`:42`), `contains_secrets:true`
  (`:43`) — сырой приватный промпт с секретами закоммичен в дерево доказательств, при том что HOST-08F гордо ассертит redaction.
- **G5 (MISSING PATH B).** 0 скриншотов/видео во всём репо (`find *.png/*.jpg/*.mp4`→0); 0 артефактов
  `*dual*/*same-scenario*/*path-b*/*join*`; вся «runtime» = CLI/app-server-пробы или node из cache; RUNTIME-04 mirror «full Codex
  chat UI unproven». Path A и Path B не сходятся на одном сценарии/ответе.
- **G6 (WEAK-NEGATIVE / дефолт-как-негатив).** `negative-missing-target-locator/case-summary.json` `exit_code:0`,
  `include_target_locator:false` — но по P19 `TargetLocator` и так всегда null на поставляемом пути, т.е. «негатив» проверяет
  ШТАТНОЕ состояние (успех, не-AI), а не деградацию; единственный настоящий error-негатив — транспортный (missing core-api script).
- **G7 (UNPINNED HOST PROVENANCE).** Набор доказательств смешивает ≥5 версий Codex-бинаря: `0.130.0-alpha.5` (RUNTIME-04G),
  `0.136.0-alpha.2` (CX-04R), `0.140.0-alpha.2`, `OpenAI.Codex_26.601.2237.0`, `26.609.4994.0` (RUNTIME-04). Воспроизводимость и
  валидность вердиктов подорваны. QA невоспроизводима: `package.json` scripts=install-smoke/source-wiring-check, `devDependencies:{}`,
  нет `test`-скрипта/раннера/CI; 2 тест-файла запускаются только ad-hoc.

**Вывод P20:** Codex QA богаче Claude (P12: 1 отчёт), сопоставим с Cursor (P16), НО: JSON-parse-вердикт + плумбинг-оракул на мёртвых
функциях + самопровозглашённый app-server-PROVEN с таймаутом/само-вердиктом/утёкшими секретами + отсутствие Path B (0 pixel) +
вырожденные негативы + ≥5 версий бинаря. Реальный статус Codex QA+dual-path: **BUILT_NOT_PROVEN** с overclaim/evidence-hygiene
дефектами; PROVEN дуал-путь для Codex = 0.

---

### MACHINE PIPELINE — 4 прохода (P21 prompt→task · P22 task→plan · P23 task→skills · P24 dpo-rows)

Здесь оценивается РЕАЛЬНЫЙ core-конвейер трёх машин в `43.SmartSkillsDomain` (общий для всех трёх хостов) + промоутнутые ассеты
`models/*` + датасеты `training-data/*`. **Важнейшая коррекция к P02:** в P02 обе первые машины назывались «always not-ready
STUB». P21/P22 уточняют: `not_ready` — это состояние БЕЗ загруженного чекпойнта; С промоутнутым ассетом путь ПРОХОДИТ, но выдаёт
не обучение, а **детерминированный lookup/replay**. То есть это не пустой стаб, а «hidden determinism» (нарушение R4), что во
многом хуже: путь зелёный, но семантики нет.

#### P21 — prompt-to-task (стадия 1): keyword-словарь, не обучение

- **Механизм = подстрочный словарь ключевых слов.** `PromptToTaskMachine.Translate` (`:112`) → `PromptToTaskTaskPatternUnit.Decide`
  (`Unit.cs:154`). Сигналы: `ExtractPromptSignals` (`:339`) = токены 4-40 символов **плюс** `BuildUniversalSignalAliases` (`:443-490`)
  — **жёстко зашитый словарь ~40 правил**: `Contains("plan")→"plan"`, `"approval"/"approve"→"approval"`, `"do not implement"→
  "implementation_authorized_false"`, `"gate"→"missing_gates"`, `"audit"→"audit"`, `"deliverable"/"artifact"→"deliverable"` и т.д.
- **Круг подтверждён промоутнутым чекпойнтом.** `BuildNearestPattern` (`:502-529`): `coverage=matched/patternSignals`,
  `confidence=pattern.Confidence*coverage`; `pattern.Confidence` — **сохранённая КОНСТАНТА** (0.95/0.94…), ничем не обновляется.
  `prompt-to-task-v1-checkpoint.json` `learnedPatterns[*].learnedSignals` **буквально равны** alias-токенам из кода: паттерн
  `q3-01-prototype-planning-approval-boundary` (`checkpoint:18-56`) → `learnedSignals=[approval, deterministic_shortcut,
  implementation_authorized_false, missing_gates, phase, plan, planning, scope_creep]`, `taskType=plan_only_or_approval_gated_task`,
  `confidence=0.95`. «Обучение» не выучило словарь — он захардкожен; «паттерны» лишь группируют его в 12 taskType. Собственный
  `PromptToTaskPhase04Runner/Program.cs:683` признаёт: «прозрачный nearest-signal baseline… не semantic quality claim».
- **Held-out — циркулярный, не гейтит промоушен.** `EvaluateHeldOutRows` (`Unit.cs:247-287`, порог `MinimumEvaluationAccuracy=0.8`)
  есть, но callers = только `Tests` (синтетика, ассерт «below threshold») + `Phase04Runner` (5 сценариев, **авторски написанных в
  самом раннере** под те же 5 границ taskType). Ни один раннер не подаёт **реальный source-disjoint** набор и не гейтит промоушен
  на ≥0.8. Манифест: `dpoPairCount:8`, `heldOutRowCount:4` — крошечный корпус.
- **Ablation — структурный no-op.** `PromptToTaskAblationHarness.cs:79` `_translate ?? (request => new PromptToTaskMachine().
  Translate(request))` — машина БЕЗ loader → всегда not-trained → not_ready. Отчёт хардкодит `TrainingRun/EvalRun/
  CheckpointOrModelChanged/SemanticQualityClaimed/TrainingReady=false` (`:132-136`); self-check ждёт `NotReadyCount==
  PlannedProbeCount` (`:447`). Мандатный ablation-gate (CLAUDE.md) никогда не гоняет ЗАГРУЖЕННОЕ решение.
- **Continue-training = append.** `ContinueWithAcceptedPattern:254-290` берёт ГОТОВЫЙ паттерн (с проставленными Confidence/
  LearnedSignals) и конкатенирует. Ни DPO/градиента, ни counter-update.
- **Пороги тривиальны — гейт всегда открыт.** `FromLearnedState` (`:99-100`) `minimumConfidence=0.01`, `minimumTopTwoMargin=0.01`;
  при `confidence=0.94*coverage` один совпавший ключ из 8-сигнального паттерна даёт ≈0.118 ≫ 0.01 → «ready». Один keyword →
  уверенная задача.
- **Two-leg схлопнут в single-state.** Unit поддерживает two-leg (`LoadState=loadUserState()??loadCommonState()`, `:289-290`), но
  Machine строит single-state (`Machine.cs:124-125`→`Unit.cs:90-93` `this(()=>state,()=>null)`), `loadCommonState` всегда null,
  ветка user→common мертва; адаптация делается подменой всей машины на уровне сервиса (`LoadCommonThenUserAdaptiveAssets`).
- **Fresh-load** — единственный gate с реальным покрытием (`LoadCheckpointAsync` + `PromptToTaskMachineAssetLifecycleTests`,
  `SmartSkillsSemanticQualityP11FreshLoadTests`), но доказывает лишь байтовую переустановку состояния, НЕ generalization.

#### P22 — task-to-plan (стадия 2): lookup/replay + разрыв словарей — 6 НОВЫХ дыр

- **G1 (главная) — РАЗРЫВ СЛОВАРЕЙ стадия1→стадия2: ready-путь недостижим end-to-end.** `OrchestrationReasoningScenarioUnit.Decide`
  выдаёт план ТОЛЬКО при `ExactSignalMatchKind` (`requiredSatisfied = requiredSignals.All(requestSignals.Contains)`, `:553-554,:569`);
  transferable/структурный overlap → вечный NotReady (`:340-347`). Запрос строится из выхода стадии 1:
  `BuildOrchestrationRequest(request, promptToTask.StructuredTask)` (`XiigenExtensionApiService.cs:130,:527`); сигналы из
  `Task.TaskType/Constraints/...` (`BuildSignalSet:604-627`). **Вычислительная сверка словарей:** гистограмма `requiredTaskTypes` по
  15 orchestration-паттернам = **`universal_reasoning`×12**, `plan_only_or_approval_gated_task`×1, `structured_audit_task`×1,
  `deliverable_task`×1 (тренер зашивает `RequiredTaskTypes:["universal_reasoning"]` каждому паттерну, `Trainer.cs:171`). Но
  **prompt-to-task НИКОГДА не эмитит `universal_reasoning`**: его 8 типов = `{audit_task, bounded_review_or_consultation_task,
  deliverable_task, plan_only_or_approval_gated_task, planning_task, scope_task, source_resolution_required_task,
  structured_audit_task}`. Обязательные content-сигналы 12 «настоящих» reasoning-паттернов (`evidence_linkage_reasoning`,
  `link_source_quality_risk_and_consequence`, `multi_axis_decomposition_reasoning`, `authority_boundary_reasoning`,
  `deterministic_shortcut_firewall`…) ОТСУТСТВУЮТ во всём словаре, который умеет эмитить стадия 1. Тест достижимости: **12/15
  паттернов недостижимы**; достижимы лишь 3/15 (на generic-токенах). → семантическое ядро task→plan **dead-on-arrival** на реальном
  промпте: стадия 1 эмитит task-framing вокабуляр, стадия 2 требует reasoning-axis вокабуляр; пересечение — только generic-токены.
- **G2 — тренер выбрасывает rejected-сторону DPO; confidence 0.8 константа.** `Trainer.MaterializeAsync→BuildPattern` копирует
  авторские поля payload прямо в паттерн (`:159-175`). Rejected-сторона `near_miss_signals` (`:164 rejectedSignals`) валидируется на
  непустоту и НИКОГДА не используется — DPO chosen-vs-rejected margin отсутствует. `Confidence:0.8` зашита (`:182`). Отчёт сам
  ставит `TrainingStarted:false, ModelAssetCreated:false, CheckpointCreated:false` (`:92-94`). Нарушение «reward-scalarization
  rule» + «acceptance-signal independence» именно для стадии task→plan.
- **G3 — DEAD ROUTE слоя 15: `ProjectWikiOrchestrationFlow` зарегистрирован, но не маршрутизируется.** `LogicalOrchestratorAssistant.
  CreateDefault` регистрирует flow (`:132-138`), но pre-route (`ProjectWikiRequestRouterUnit().Route`) может переключить сценарий
  ТОЛЬКО на `ProjectWikiTrainingFlowAdapter`/`ProjectDocumentationPipelineFlowAdapter` (`:233-243`) — `ProjectWikiOrchestrationFlow.
  ScenarioId` адаптивным роутером НЕ выбирается НИКОГДА. Вызыватели = сам ассистент + `TeachingSessionBuilder` + `Tests`. **Граница
  трёх расширений `XiigenExtensionApiService` имеет 0 ссылок на `OrchestratorModel`/`LogicalOrchestratorAssistant`/
  `AdaptiveLogicalOrchestrator`** — ни одно из трёх расширений не доходит до слоя 15 вообще.
- **G4 — выход «плана» = replay паттерна ЛИБО константный baseline-план.** `Machine.Plan` строит `OrchestrationScenarioPlan`
  буквально из полей выбранного паттерна (`PhaseFamilies/GateFamilies/... = chosenPattern.*`, `Machine.cs:176-185`) — exact-replay;
  non-exact → вечный NotReady. Когда стадия 1 ушла в baseline (`Task.SourceProvenance["semantic_translation"]==
  "baseline_internal_runtime_fallback"`, `:560-563`), оркестрация возвращает ПОЛНОСТЬЮ ЗАШИТЫЙ константный план (`PhaseFamilies:
  ["runtime-quality-gates"]`, `GateFamilies:[source-disjoint-eval, ablation-control, leakage-control, fresh-load, continue-training]`,
  `:489-515`) — одинаковый для ЛЮБОЙ задачи. Обобщения нет (R4 hidden determinism стадии 2).
- **G5 — нет runner-report/held-out-артефакта для `orchestration-reasoning-v1`.** `find models/orchestration-reasoning -name
  runner-report.json` → пусто (у prompt-to-task/skill-selection есть). Manifest заявляет `heldOutRowCount:4`/`dpoPairCount:8`, но
  ни одного исполненного `EvaluateHeldOutRows` (порог 0.8) не персистировано.
- **G6 — continue-training = self-конкатенация; удвоенный stateId.** `ContinueTrainingFromLearnedState = existingPatterns.Concat(
  deltaState.LearnedPatterns)` + `StateId="{existing}:continued:{delta}"` (`Machine.cs:287-297`); shipped `checkpoint.json` показывает
  self-continued state (тот же базовый stateId с обеих сторон `...:continued:...same...`).

#### P23 — task-to-skills (стадия 3): lexical bag-of-words argmax — 6 НОВЫХ дыр + коррекция P02 gap-4

- **Как работает.** `SkillSelectionRequest` → `DecideFromTrainedState` (`:107`) → СНАЧАЛА `TryDecideWithSemanticUnitsOnly` (`:173`) —
  каскад из 11 «семантических» юнитов по порядку (`skill_selection → prompt_context → … → prompt_understanding`); **первый юнит с
  непустым решением ПОБЕЖДАЕТ** (short-circuit). Каждый юнит (`SkillSelectionLogicalHierarchy.cs:441-492`): `requestSignals=
  ExtractPromptUnderstandingSignals(prompt)`; `Score=Σ_{signal∈requestSignals} profile.PromptFeatureWeights[signal]` (`:454-455`) =
  сумма выученных весов СЛОВ; `argmax` (`:458`); `Score≤0→no decision` (`:462`); `SelectedSkillIds=[argmax skill]` (ровно ОДИН),
  `Confidence=Math.Min(0.95, 0.50+Score*0.05)` (`:492`). `ExtractPromptUnderstandingSignals` (`Machine.cs:2726`): **сырые слова
  промпта** — split whitespace, буквы/цифры, lower, длина ≥4, distinct, sort. НИКАКОГО выученного словаря/алиасов/эмбеддинга/
  стемминга: пространство признаков = буквальные ≥4-символьные слова промпта.
- **Тренер игнорирует rejected.** `LearnPromptFeatureWeights` (`:1370-1382`) для КАЖДОГО chosen-скилла инкрементит `skillProfile[word]
  +=1` по всем словам промпта; **`pair.Rejected` не используется вообще** — нет маржи chosen−rejected, нет down-weight. Это
  положительно-счётная таблица частот слов (Naive-Bayes-подобная), не preference-optimization.
- **КОРРЕКЦИЯ P02 gap-4:** `_candidatePositionWeights` (позиционный счётчик) БОЛЬШЕ НЕ авторитет выбора — используется только когда
  семантический каскад вернул null, и тогда возвращается ПУСТОЙ выбор (`SelectedSkillIds:[]`) с leakage-guard evidence (`:128-142`);
  реализован `CandidateOrderLeakageGuardUnit` (`:145-148`), которого P02 не нашёл. Позиционный счётчик низведён до
  diagnostics/leakage-only. Резидуал — нет полноценного ablation-harness (см. ниже).
- **Source-disjoint runtime eval — центральный вопрос, ОТСУТСТВУЕТ (само-признано).** `runner-report.json`: `passed:true`,
  `held_out_accuracy:1`, `accuracy_at_export:1`, `exact_prompt_leakage:0`, `eligible_pair_count:75`, `trained_pair_count:900`
  (=75×12 адаптеров), `semantic_quality_claimed:false`, `coverage_type:"s14_contract_catalog_not_runtime_semantic_quality"`.
  `semantic_domain_matrix[*].held_out_fixture` — строки КАТАЛОГА (`SmartSkillsSemanticUnitCatalog`), не исполненные per-unit
  прогоны. Собственный тест `SmartSkillsSemanticQualityP22SourceDisjointRuntimeEvalTests` — это НЕ оценка, а **фиксация отсутствия
  оценки**: `acceptedRuntimeRows=Array.Empty` (`:88`), verdict `NO_TRUE_SOURCE_DISJOINT_RUNTIME_EVAL_FOUND` (`:205`), scenario
  `not_run_no_true_source_disjoint_runtime_eval` (`:156/:278`); контролирующие `p16e/p16e2`→`candidate_schema_eval_only:true`,
  `p21`→`sourceDisjointProven:false`.
- **`held_out_accuracy:1` НЕ source-disjoint.** Для лексического bag-of-words argmax 100% на held-out = лексическая утечка слов из
  train-корпуса (память кооккурренса), не генерализация. `exact_prompt_leakage:0` проверяет лишь ТОЧНЫЙ реплей промпта (SHA256
  `_trainedPromptKeys`) + перестановку кандидатов, но НЕ лексическое перекрытие ≥4-символьных слов train↔eval — реальный источник
  утечки bag-of-words модели.
- **6 дыр:** (1) source-disjoint runtime eval MISSING (само-записанная дыра); (2) `held_out_accuracy:1` циркулярный/in-sample, не
  гейтит промоушен; (3) авторитет = выученный lexical bag-of-words argmax, «11 семантических юнитов» — misnomer (одинаковый argmax
  поверх разных весовых таблиц, синоним/парафраз без общего ≥4-симв. слова → Score=0 → нет решения); (4) тренер игнорирует
  rejected-сторону — нет DPO-маржи; (5) нет ablation-harness для skill-selection (только candidate-order guard); (6)
  `exact_prompt_leakage:0` переоценивает «нет утечки» (нет проверки перекрытия ≥4-симв. слов).

#### P24 — DPO-строки/датасеты НА ДИСКЕ (реальные количества/происхождение/honesty) — 5 дыр (в мастере)

Не код тренера (P05) и не чекпойнт-заявки (P21-23), а сами данные на диске. Измерено `git ls-files` + `wc -l` + чтение карточек:

| Датасет | строк | honesty-флаги |
|---|---:|---|
| `seed-v1/smart-skills-dpo-seed.jsonl` | 8 | seed (PTT3/OR3/SS2); `PASS_SEED_DATASET_CARD_STAGED_ONLY`, `training_allowed:false`, `seed_smoke_only:true`, `source_disjoint:false` |
| `prompt-to-task-expanded-v1/...-dpo.jsonl` | 12 | train8/heldout4; `training_allowed=0/12`, `semantic_quality_claimed=0/12`, **`chosen_rejected_pair_present=12/12`**, `materialized_only=3/12` |
| `orchestration-reasoning-expanded-v1/...-dpo.jsonl` | 12 | train8/heldout4; «material readiness only; not model training»; **`chosen_rejected_pair_present=0/12`** (!) |
| `skill-selection-training-v1/...q5-04r-...jsonl` | 38 | СИРОТА (divergent root — GAP-5) |
| `skill-selection-heldout-v1/...jsonl` | 12 | SS held-out |
| `skill-selection-inventory-v1/skill-inventory.jsonl` | 175 | инвентарь скиллов, **НЕ DPO-пары** |
| `cross-project-transfer-fixtures-v1/...jsonl` | 8 | fixtures |
| **`training-data/skill-selection/skill-selection-v1/corpus-snapshot.jsonl`** | **88** | **авторитетный** корпус (его берёт runner); `training_pair_count:75`, `raw_private_corpus_committed:false`, `adaptive_private_rows_included:false` |

- **12× inflation.** skill-selection «900 обученных пар» = `eligible 75 × 12 адаптеров` = те же 75 пар, размноженные, а НЕ 900
  различных принятых строк.
- **schema-pair-absence (OR).** У orchestration-reasoning-expanded rejected-сторона отсутствует на уровне ДАННЫХ:
  `chosen_rejected_pair_present=0/12` (у prompt-to-task 12/12). То есть OR-датасет физически не несёт rejected-строк для DPO-маржи —
  это данные-подтверждение кодовой дыры P22 G2.
- **near-zero online-accepted.** Adaptive/online DPO-сигналов (`.xiigen/*/adaptive-dpo-signals.jsonl`) во всём репо = **4 файла,
  каждый по 1 строке**; skill-selection — **0 файлов**. Единственная строка — синтетический probe (`signalId:"ptt-ad-02-probe-
  signal-001"`, chosen/rejected refs = плейсхолдеры, `containsRawPromptText:false`, `semanticQualityClaimed:false`).
- **origin.** Все строки — АВТОРСКИЕ «materialized» примеры, не принятые из рантайма/фидбэка: seed card `source_refs →
  artifacts/dpo-quality/smart-skills-dpo-materialized.jsonl` («reviewed materialized seed rows copied exactly»); схема несёт
  `chosen_ref/chosen_summary/rejected_ref/rejected_summary`, `approved_behavior/rejected_near_miss_behavior` — авторская
  near-miss-таксономия по ссылке, а не захваченные chosen/rejected выходы модели.
- **divergent roots.** Два корня SS-корпуса: `training-data/smart-skills/...` (сирота 38 строк) и `training-data/skill-selection/
  skill-selection-v1/corpus-snapshot.jsonl` (88, авторитетный). `orchestration-reasoning-v1`: 12 строк на диске, но runner-report
  ОТСУТСТВУЕТ (подтверждение P22 G5 уже на уровне данных).

**Сверка «заявлено vs диск»:** PTT `dpoPairCount:8+heldOutRowCount:4=12` — сходится; SS `900`=75×12 — inflation; OR 12 строк без
runner-report. Реальный порядок величины ВСЕГО обучающего сигнала трёх расширений: ~десятки авторских materialized-строк на машину
+ 4 синтетических online-probe-строки — не корпус для настоящего DPO/generalization.

---

### СВОД по трём хостам (теперь ПОЛНЫЙ: Claude · Cursor · Codex) — для плана VS/VS Code

| Слой | Claude (P09-12) | Cursor (P13-16) | Codex (P17-20) |
|---|---|---|---|
| Host entrypoint | СКЕЛЕТ — только SessionStart-hook | РЕАЛЬНЫЙ — `activate()`, 3 команды, chat participant, webview, 11 настроек (самый зрелый) | Рукописный `codex-entrypoint.js`, НЕ зарегистрирован в реальном Codex marketplace (`codex plugin list` без xiigen) |
| Регистрация в host | plugin.json только hooks (нет команды/skill/MCP) | package.json contributes (реальный extension-манифест) | `.codex-plugin/plugin.json` — НЕ тот формат; host `BLOCKED...plugin_loaded_invoked:false` |
| Собранный артефакт | нет `dist`, 0 `.ts` (hand-js) | `dist/extension.js` 48 КБ, НЕвоспроизводим (P13 G7) | нет `dist`, 0 `.ts` (hand-js) |
| Видимый ответ | hook echo в чужой файл | тост со статус-строкой (`showInformationMessage`) | **runCli эхит ПУСТОЙ PS stdout — ответ не рендерится вообще** (P17 G2) |
| Транспорт к ядру | `spawnSync powershell.exe`→temp-csproj `dotnet run` | `spawn powershell.exe` (Windows-only) + абсолютные дефолты | тот же PS-шим; «runtime» = node из cache-копии, model-backed Access denied (P17 G7) |
| Методы конвейера из хоста | 1/3 (только prompt-to-task) | 1/3 интерактивно; `feedback`/`debug-trace` роняются | 1/3 поставляемо; метод СЫРОЙ, не валидируется (P18) — обратная крайность Cursor |
| Каталог скиллов / стадия 3 | пустой каталог → no-op | «no host selector», pass-through → no-op | **само-ссылочный `["xiigen-smart-skills"]`** → no-op (P18) |
| Online-обучение (`feedback`) | метод не реализован в ядре | роняется хостом ДО ядра | нет lifecycle-поверхности; adaptive-root = символическая строка вне пакета (P19) |
| Locator | test-only фикстура qsg28 | — | `TargetLocator:null` всегда; нет фикстуры (P19 DEAD LOCATOR) |
| QA | 1 отчёт, node.exe boundary, 1 error помечен PASS | 10 файлов, реальный Cursor.exe headless, нет oracle, все pending | JSON-parse-вердикт + плумбинг на мёртвых функциях + app-server PROVEN с таймаутом/само-вердиктом/утёкшими секретами (P20) |
| Path B (видимый desktop) | фикс. hook-строка, error+mismatch, 0 pixel | headless extensionTests, 0 pixel | **0 pixel, 0 same-scenario join** (P20 G5) |
| Провенанс бинаря | claude.exe version-pin устарел | — | **≥5 версий Codex-бинаря** в наборе (P20 G7) |
| Итоговый статус среза | **BUILT_NOT_PROVEN скелет** | **BUILT_NOT_PROVEN (зрелый extension, провальный proof)** | **BUILT_NOT_PROVEN (наименее host-достижимый)** |

**Зрелость host-достижимости: Cursor > Claude ≈ Codex.** Cursor — единственный с реальным `extension.ts`+`dist` и настоящей
host-регистрацией через `package.json contributes`; Claude застрял на SessionStart-hook; Codex формально имеет marketplace-систему,
но XIIGen туда не попал и его runCli даже не показывает ответ. **Для порта на VS Code Cursor остаётся правильной стартовой точкой**
(см. свод прохода 3), а Codex — отдельный трек (либо упаковка в реальный Codex marketplace-snapshot, либо решение Luba о сужении).

### MACHINE-PIPELINE real-vs-stub свод (стадии 1→2→3→DPO)

| Стадия | Класс | Ключевой дефект | Опорное file:line |
|---|---|---|---|
| 1 prompt→task | HIDDEN DETERMINISM / keyword-словарь | alias-словарь ~40 правил как authority; confidence-константа; пороги 0.01 (гейт открыт); held-out циркулярный; ablation no-op; continue=append | `PromptToTaskTaskPatternUnit.cs:443-490/:502-529`; `checkpoint:18-56`; `AblationHarness.cs:79` |
| 2 task→plan | PIPELINE BREAK + REPLAY | 12/15 паттернов недостижимы (universal_reasoning×12 vs 8 taskType стадии 1); тренер выбрасывает rejected; план = exact-replay ИЛИ константный baseline; слой 15 мёртв на маршруте | `Trainer.cs:171/:159-175`; `Machine.cs:176-185/:489-515`; `LogicalOrchestratorAssistant.cs:233-243` |
| 3 task→skills | LEXICAL BAG-OF-WORDS | argmax по частотам ≥4-симв. слов; rejected не используется; source-disjoint eval ОТСУТСТВУЕТ (само-признано); held-out=1 циркулярный | `SkillSelectionLogicalHierarchy.cs:454-492`; `SkillSelectionMachine.cs:1370-1382/:2726`; `P22SourceDisjoint...Tests.cs:205` |
| DPO-данные | ~ДЕСЯТКИ АВТОРСКИХ + 4 online | 900=75×12 inflation; OR rejected 0/12; online-accepted 4 файла×1 строка (SS=0); все materialized, не captured | `corpus-snapshot.jsonl`; OR-split `chosen_rejected_pair_present=0/12`; `.xiigen/*/adaptive-dpo-signals.jsonl` |

**Сквозной вывод машинного конвейера:** это НЕ обучаемый семантический prompt→task→plan→skills, а цепочка трёх детерминированных
lexical/lookup-механизмов со СТРУКТУРНЫМ разрывом словарей между стадиями 1 и 2 (G1 P22 — семантическое ядро недостижимо на
реальном промпте) и без rejected-стороны DPO ни в одной стадии. `semanticQualityClaimed=false` — честная самооценка, но и trainable
lifecycle (learning-signal → state-mutation → held-out uplift → ablation-инвариантность) не доказан ни одним прогоном. Это прямо
питает roadmap-узел **M1 (de-risk обучающего движка) → M2 (реальный обучаемый инференс в 3 машинах)** из прохода 1: пока стадии 1↔2
говорят на разных сигнальных словарях и rejected-сторона отброшена, никакой host (в т.ч. будущий VS Code-порт) не даст
семантического ответа.

### ORDER-LEAKAGE finding (консолидировано, для VS/VS Code-плана)

Требование задания — отдельно зафиксировать order-leakage. Полная картина по проходам 4:
- **Историческая дыра (P02 gap-4):** `SkillSelectionMachine._candidatePositionWeights` (`Dictionary<int,int>`) обучался по ИНДЕКСУ
  кандидата и стартовал решение от `_candidatePositionWeights.Count` — позиция кандидата в списке была АВТОРИТЕТОМ выбора скилла
  (нарушение «Order-features carrier mandatory rule» / candidate-order leakage). Guard `CandidateOrderLeakageGuardUnit` в
  `43.SmartSkillsDomain` тогда ОТСУТСТВОВАЛ (был только в `44.DesignConversionDomain`/docs).
- **Коррекция P23 (текущее состояние кода):** guard РЕАЛИЗОВАН (`SkillSelectionMachine.cs:145-148`); `_candidatePositionWeights`
  низведён до diagnostics/leakage-only — используется лишь когда семантический каскад вернул null, и тогда выбор ПУСТОЙ
  (`SelectedSkillIds:[]`) с leakage-guard evidence (`:128-142`). Позиция кандидата БОЛЬШЕ не авторитет.
- **НО подтверждено на Codex-пути (P18):** в реальном RUNTIME-04B evidence авторитетом выбора всё ещё выступает счётчик
  позиции/контекста — `skill_selection_evidence: prompt_score=0, context_score=4, total_score=8` (выбор driven контекст-счётчиком,
  а `prompt_score=0`), т.е. на живом Codex-прогоне решение принято НЕ по семантике промпта.
- **РЕЗИДУАЛ (P23 gap-5):** полноценного ablation-harness (word-permutation / synonym-substitution / leakage-removal на ЗАГРУЖЕННОМ
  checkpoint), доказывающего, что выбор НЕ сводится к позиции/общим словам, для skill-selection НЕТ (есть только candidate-order
  guard). `exact_prompt_leakage:0` ловит лишь точный реплей + перестановку кандидатов, но не лексическую утечку ≥4-символьных слов.
- **Итог order-leakage:** структурный candidate-index-authority дефект в коде ЗАКРЫТ guard-ом, но (а) на Codex-runtime всё ещё
  видно context/position-счётчик как драйвер, (б) остаётся ЛЕКСИЧЕСКАЯ утечка (bag-of-words по общим словам) без ablation-контроля —
  это следующий класс leakage, который план VS/VS Code должен закрыть ablation-harness + Jaccard-word-overlap-контролем.

### Что остаётся на проходы 5-10

- **Пройдено к концу прохода 4:** opus-pass 01-24. Три host-среза (Claude P09-12, Cursor P13-16, Codex P17-20) + ядро
  машинного конвейера (P21-24) — ЗАКРЫТЫ.
- **P25-30 (проход 5):** dpo-training (88 report’ов все UNKNOWN delta; единственный реальный run регрессировал — P25-1/P05-3),
  checkpoint-locator (`IRemoteUnitLocator.LocateAsync` обойдён; версия — литерал; index.json руками), token-savings (delta 0;
  baseline≡adaptive), semantic-quality (dead-gate), dashboard-docs, lifecycle-ops (export/import/backup/share/continue/rollback
  только в тестах; payload 555-байт).
- **P31-40 (проходы 6-7):** scenario-inventory (28×host, full_joined_pass 0), per-host scenarios (claude/cursor/codex joined-pass 0),
  pathA-harness, pathB-desktop, core-boundary (триплицированные адаптеры, R6), trainable-audit (R4 downgrade), tests-qa-state,
  git-branch-state (331 файл дельты не закоммичены, 9 worktree).
- **P41-49 (проходы 7-8):** adversarial-recheck, hidden-overclaims, missing-scenarios, integration-e2e (нет versioned wire-contract),
  provenance-legal (sonnet-derived строки, distillation без teacher-гейта), ux, perf, priority-sequencing, risks (RCE-вектор).
- **P50 + closure (проходы 8-9):** roadmap M0-M6 (уже суммирована в §5 прохода 1) + `opus-PROJECT-CLOSURE-PLAN.md` (~245 дыр — полный
  накопительный реестр всех P01-P49).
- **Проход 10:** copied-plans (30) + supporting-files (108) + 2 транскрипта — для source-addressability и evidence-инвентаря.

### Границы честности прохода 4

- claim_scope: полное чтение opus-pass-17..24 + извлечение Codex host-reality (4 прохода) и machine-pipeline reality (4 прохода,
  стадии 1→2→3→DPO) с file:line; полный трёх-host свод; консолидированный order-leakage; machine real-vs-stub. Русский, read-only.
- evidence_scope: прочитаны ПОЛНОСТЬЮ 8 файлов opus-pass-17..24; выводы привязаны к file:line ИЗ ЭТИХ ПРОХОДОВ. Сам код
  `codex-xiigen-smart-skills`, `43.SmartSkillsDomain`, промоутнутые `models/*` и датасеты `training-data/*` в этом проходе НЕ
  читались напрямую (цитируются по Opus-аудиту — родитель верифицирует против живого кода). P24-таблица дыр (5 шт.) вынесена
  автором в мастер `opus-PROJECT-CLOSURE-PLAN.md`; здесь извлечены дисковые факты самого прохода. Тесты/сборки/git/runtime НЕ
  запускались.
- НЕ доказано этим проходом: любые product-readiness утверждения; точность каждого per-pass file:line-вывода (нужна верификация
  родителя против живого кода `C:\Projects\...`).
- subagent_id: UNKNOWN_IN_SUBAGENT. requested_model: gpt-5.5-equivalent (Opus 4.8 Ultracode). requested_reasoning_effort: xhigh.

## Pass 5/10 — opus-pass 25-32: dpo-training + checkpoint-locator + token-savings + semantic-quality + dashboard + lifecycle-ops + scenario-inventory + claude-scenarios

Границы прохода 5: прочитаны ПОЛНОСТЬЮ восемь файлов opus-pass-25..32 — это ядро «обучение + жизненный цикл + качество +
сценарии». Извлечены исполненные ЧИСЛА обучения, реальная persistence чекпойнта/локатора, корневая причина token-delta=0,
причина мёртвости semantic-quality-гейта, статус lifecycle-операций, полный 28-сценарный инвентарь и по-сценарный разбор Claude.
Read-only; тесты/сборки/git/runtime НЕ запускались. Автор: Claude Opus 4.8 (Ultracode), одиночный bounded-агент, читал и писал сам.

Человеческий смысл этого блока для плана VS/VS Code: **это доказательная сердцевина, почему все downstream-фазы (token/quality/
lifecycle/scenario) закрыты нулём.** Реальный градиентный DPO есть, но он (а) отсоединён от расширений и (б) в единственном
исполненном прогоне регрессировал. Всё, что выше него по стеку, — token-savings, semantic-quality, lifecycle, dual-path — либо
структурно запрещено схемой ядра, либо существует как fixture/parse-only/manifest-огрызок. Ни один из 28 сценариев не собран.

---

### P25 — dpo-training: ИСПОЛНЕННЫЕ числа обучения (регрессия), fresh-load, continue-training

**Прочитано (bounded):** первый/последний шаг эталонного 50-step прогона + агрегатный section22-report; свод по 88 section22;
распределение held-out; runner-report'ы промоутнутых ассетов; все `continue-probe`/`fresh-load` артефакты.

**Мета-факт (важно для плана):** настоящий градиентный DPO РЕАЛЬНО запускался — но только для юнитов домена `42.ProjectWikiDomain`
(`code-trace-builder-unit`, `negative-dpo-generator`), НЕ для трёх extension-машин. На диске: **4263** `.bin`, **25488**
`dpo_step_*.json`, **88** section22 runner-report'ов (`43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-15..19-*`).
То есть: где есть настоящий DPO — это wiki-домен, ОТСОЕДИНЁННЫЙ от расширений (P05 gap-2); где расширения — настоящего DPO нет вовсе.

- **G1 — единственный настоящий градиентный DPO-run РЕГРЕССИРОВАЛ.** Эталон `code-trace-builder-flow-batch-001-...20260515143246`:
  `dpo_step_000001.json` `Loss 0.68713, RewardMargin 0.01208, Accuracy 1` → `dpo_step_000050.json` `Loss 0.69122,
  RewardMargin 0.00387, Accuracy 0.6`. DPO-loss при случайной инициализации = `-log σ(0) = ln2 ≈ 0.693147`. За 50 шагов loss не
  упал, а **вырос ОБРАТНО к ln2**, reward-margin **сжался ~3×**, train-accuracy упала 1→0.6. Агрегат section22: `weighted_accuracy 1,
  reward_margin 0.005325, loss 0.690509, heldout_accuracy 0.56` (чуть выше монетки), **`heldout_loss 0.691449` ≈ ln2 (на held-out
  модель не научилась ничему)**, `heldout_reward_margin 0.00345, dpo_accuracy 0.66`. Сами ИСПОЛНЕННЫЕ числа доказывают отсутствие
  обучения, независимо от объяснения P05 gap-3 (хеш-токенайзер/toy-1-слойная модель).
- **G2 — ни один из 88 runner-report'ов не считает delta улучшения (before/after).** `loss_delta`/`reward_margin_delta`/
  `weighted_accuracy_delta` = **UNKNOWN во всех 88** (0 не-UNKNOWN). Отчёты явно перечисляют `missing_runner_metrics:
  ["reward_margin_delta","loss_delta"]`; `completion_claim_allowed:false` — во всех 88. Системно: «обучение улучшило модель» нигде
  численно не подтверждено, и раннер сам это фиксирует.
- **G3 — held-out разбросан 0…1 с массой на/ниже монетки.** По 88 report'ам: `1.0`×10, `UNKNOWN`×9, `0.5`×6, `0.4`×5, `0.64`×4,
  `0.62`×4, `0.52`×4, `0.42`×4, `0.56`×3, … вплоть до `0.18/0.16/0.0`. «1.0» = крошечные held-out (`heldOutRowCount≈4`), не
  генерализация. Held-out ведёт себя как шум.
- **G4 — промоутнутые extension-ассеты не несут DPO loss/reward-margin; у prompt-to-task runner-report НЕТ вовсе (коррекция P22 gap-5).**
  `models/prompt-to-task/prompt-to-task-v1/` = только manifest+checkpoint; `git ls-files | grep prompt-to-task.*runner-report` → **0**.
  `models/skill-selection/.../runner-report.json`: `accuracy_at_export:1, held_out_accuracy:1` и **НИ ОДНОГО** ключа loss/reward/
  margin/delta. orchestration-reasoning runner-report отсутствует. Trainable Unit pattern требует persistить accuracy **И** loss/
  reward-margin — ни один из трёх ассетов не несёт DPO-оптимизационной метрики.
- **G5 — все continue-training пробы extension-машин = no-op со SKIP replay (before==after).** `PTT-AD-04`/`OR-AD-04`: `no_op:true`,
  `metric_before:1==metric_after:1`, reason «continue probe **skipped replay explicitly**»; `AD-07` sha before==after байт-в-байт;
  `SS-13` «deterministic no-op». Единственный не-no-op — игрушечный `q6/...adaptive-continue-probe-report.json` `metric 0→1` (бинарный
  флип одного сигнала). Код-путь continue-training-UPDATE **ни разу не исполнялся на реальных строках** (прямая связь с P24 gap-4:
  online-принятых строк ≈ 0). Сильнее P05 gap-6: механизм не «не доказал прирост», а **никогда не запускается** (replay skipped by design).
- **G6 — fresh-load доказывает лишь loadability 2 паттернов и сам декларирует «no continue-training / no improvement».** `OR-04B-
  CONTINUE-TRAINING-FRESH-LOAD-01.json`: `learned_pattern_count:4`, прогоняются ровно base+delta (2 паттерна),
  `checkpoint_unchanged_by_probe:true`, `semantic_quality_claimed:false`. Нет before/after свежезагруженной модели против пустой на held-out.

**Вывод P25 (реальность обучения):** для расширений реального обучения нет; единственный настоящий DPO-прогон (wiki-домен,
отсоединён) регрессировал; ни delta-метрик, ни исполненного continue-training. Это прямо питает roadmap-узел **M1 (de-risk
обучающего движка)** — без него всё выше по стеку недоказуемо.

---

### P26 — checkpoint / locator: persistence, integrity, promotion-гейт (8 новых дыр G1-G8)

**Прочитано:** `25.ModelServer/Local/LocalModelRepository.cs`, `Remote/InMemoryRemoteUnitLocator.cs`, контракты
`IModelRepository/IModelServer`, `Http/ModelEndpointRegistry.cs`, `Remote/RemoteLogicalUnitProxy.cs`, `models/index.json` (131),
`44.DesignConversionDomain/DesignConversionPromotion.cs` (410), срез `XiigenExtensionApiService.cs:1389-1475` (`LoadCommonAssets`),
`Phase08CommonUserTwoLegLifecycle.cs:740-800` (rollback).

**Центральный вывод:** ВСЯ инфраструктура model-server (`25.ModelServer`: repository, locator, publish, proxy) **НЕ подключена к
рантайму трёх расширений вообще**. Три промоутнутые модели грузятся по ЖЁСТКО ЗАШИТЫМ файловым путям, минуя `IModelRepository`/
`IRemoteUnitLocator`/`index.json`. **Ответ на вопрос «переживает ли рестарт»: promotion/locator у расширений не механизирован —
переживает только сырой файл на диске по хардкод-пути; смена активной версии требует пересборки, а не записи в реестр.**

| # | Дыра | file:line | Суть для плана |
|---|---|---|---|
| G1 | Потребление promoted-checkpoint минует весь `25.ModelServer` | `XiigenExtensionApiService.cs:1403-1404,:1422-1424,:1442-1444` `Path.Combine(root,"models",...)`; grep `IModelRepository\|IRemoteUnitLocator\|index.json\|UploadAsync\|LocateAsync` в `43.SmartSkillsDomain/*.cs`→0 | Слой репозитория/локатора не используется ни на записи, ни на чтении. «Promotion через 25.ModelServer» и «discoverable через IRemoteUnitLocator» не выполняются. |
| G2 | Версия checkpoint зашита в код; version-indirection нет | `:1403 "prompt-to-task-v1"`, `:1422 "orchestration-reasoning-v1"`, `:1442 "skill-selection-v1"` — строковые литералы | Промоушен `v2` требует правки кода и **пересборки**, а не записи указателя. Рантайм не может сменить активный checkpoint. |
| G3 | `models/index.json` — рукотворный каталог, продовым рантаймом не читается | `:52-67,:100-119` цепочки `metadata_hash_repair` (руками правленные sha); читатели = `27.TranslationDomain.Tools` + одноразовые пробы; программного writer'а нет | Единственный persistent «реестр» правится руками и рантаймом не читается. (Контраст P05: там in-memory локатор не персистентен; тут наоборот — persistent, но unwired.) |
| G4 | Нет integrity-гейта на загрузке checkpoint | `:1389-1465` `LoadCommonAssets` пишет `refs["*_checkpoint_sha256"]=Manifest.CheckpointSha256` (provenance), но **НЕ пересчитывает хеш файла**; единственное sha-сравнение сверяет две ОБЪЯВЛЕННЫЕ sha, не байты | Повреждённый/подменённый checkpoint загрузится молча, если десериализуется. |
| G5 | У трёх расширений НЕТ промоушен-гейта в проде | grep `PromotionDecision\|MeetsPromotion\|PromotionGate` в `43.SmartSkillsDomain/*.cs`→0; контраст `DesignConversionPromotion.cs:57-121` `DesignConversionPromotionGateEvaluator.Decide` (held-out≥порог, source-disjoint, shortcut-detect, fresh-load, budget-tier, quarantine) | Промоушен = ручная выкладка файлов без гейт-решения. Проект УМЕЕТ строить гейт (DesignConversion), но shipped-расширения его не используют. |
| G6 | Сильный гейт DesignConversion — test-only, by-design не заявляет model-server promotion | `:276-288` `_uploader is null`→`DESIGN_CONVERSION_PROMOTION_UPLOADER_REQUIRED`; `:326-330` `ModelServerUploadAttempted=false`/`PromotionReceiptKind="local_receipt_only"`; `:372-380` `MODEL_SERVER_PROMOTION_NOT_CLAIMED_LOCAL_RECEIPT`; нет прод-вызывателя/uploader'а | Даже лучший гейт не вызывается из прода и всегда падает в local-receipt. |
| G7 | Решения `Rollback`/`ContinueTraining`/`Quarantine` ПРИНИМАЮТСЯ, но не ИСПОЛНЯЮТСЯ | `:96` `DecisionKind.Rollback` только производится; `:270-274` при `!AllowsUpload` возвращается отчёт с `null` receipt, без restore/quarantine | «rollback» = метка отчёта, не операция. |
| G8 | `IModelServer` — мёртвый контракт; `RemoteLogicalUnitProxy` эхоит вход | grep `: IModelServer`→0; `RemoteLogicalUnitProxy.cs:34` output=`request.Input` (эхо, confidence 1.0); `LocalModelRepository.cs:10-30` `LoadWeightsAsync` игнорирует `serverBaseUrl` | «remote/marketplace»-нога lifecycle — фикция на уровне контрактов. |

Cross-ref: единственный ИСПОЛНЯЕМЫЙ core-rollback — `Phase08.RollbackCommonUpgrade:789`, но восстанавливает ТЕКСТ манифеста
без post-rollback load/inference проверки, вызыватели — только `Tests/*`.

---

### P27 — token-savings: КОРНЕВАЯ причина delta=0 + что чинит

**Прочитано целиком:** `43.SmartSkillsDomain/TokenTraceAccounting.cs` (211), `ExtensionQualityScoring.cs` (191); срезы
`XiigenExtensionApiService.cs:620-699`, `ExtensionEvalOnlyQualityRunner.cs:339-399`; грепы вызывателей по всему репо.

**Реальной СИСТЕМЫ экономии токенов нет ни на одном из трёх хостов.** Есть два несведённых полу-механизма в ядре, а схема пары
ядра **структурно запрещает** и input-reduction, и сам savings-claim. Шесть корневых находок:

1. **Кросс-хостовый корень: общий core-сервис не эмитит НИ ОДНОГО токен-трейса ни для одного хоста.** `XiigenExtensionApiService`
   не зовёт ни `TokenTraceAccounting.CreateHeuristicTrace`, ни `ExtensionQualityScoring.Score` — только строковые не-заявления
   (`:632 token_savings_claimed=false`, `:691`, `:664/:792/:816`). Вызыватели `CreateHeuristicTrace` → только Tests + одноразовая
   `measurement-.../Program.cs:415`; `ExtensionQualityScoring.Score` → только `ExtensionEvalOnlyQualityRunner.cs:371` + Tests.
   Claude и Codex-пути токенов НЕ учитывают вовсе (уточняет P15, который был Cursor-scoped).
2. **Нет арифметики редукции, а схема НАВСЕГДА запрещает claim.** `TokenTraceAccounting` хранит `TokenCountInput/Output/Total`, но
   НИГДЕ не считает `baseline−adaptive` и не выводит ratio/percent. `TokenSavingsClaimed` захардкожен `false` (`:97`), а ЛЮБОЕ `true` =
   ошибка `token_savings_claim_forbidden_in_trace_schema_phase` (`:161-164`). Пути ACCEPTANCE не существует.
3. **Валидатор пары требует ИДЕНТИЧНЫЙ `prompt_hash` → структурно запрещает input-reduction.** `:146` `RequireEqual("prompt_hash",
   baseline,adaptive)`. Реальная экономия — это сокращение ВХОДА (контекста); adaptive-вход обязан ОТЛИЧАТЬСЯ → другой hash → провал
   `prompt_hash_must_match`. Схема допускает различие только на ВЫХОДЕ. Это корень под наблюдением P15 «нет независимой переменной».
4. **Считается НЕ ТА величина.** `ExtensionTokenTraceInput` = только `PromptText`/`VisibleAnswerText` (`:13-14`); трейс считает
   токены пользовательского промпта и видимого текста XIIGen (`:68-69`), а НЕ «токены контекста к платной хостовой LLM». Вместе с
   правилом идентичного prompt_hash вход по счёту равен по построению — варьирует лишь длина ответа (это не «savings»).
5. **Token+quality никогда не соединяются в рантайме.** `QualityRegression:null` всегда (`:88,:36-37`); единственный не-тестовый
   вызыватель скорера передаёт `TokenComparison:null` (`ExtensionEvalOnlyQualityRunner.cs:377`). Ветка token-comparison скорера
   (`ExtensionQualityScoring.cs:66-78`) МЁРТВА у каждого реального вызывателя.
6. **Pass-путь скорера НЕ требует редукции и пропустил бы РОСТ токенов.** Вердикт (`:95-110`) зависит только от layer-скоров;
   пакет с `AdaptiveTokenTotal >= BaselineTokenTotal` при ≥4 слоях и `QualityRegressed=false` возвращает **Pass**.

**Корневая причина delta=0 + фикс (для плана):** delta структурно 0, потому что (а) общий сервис не эмитит трейс, (б) арифметики
редукции нет, (в) схема запрещает различие входа (идентичный prompt_hash) и запрещает claim, (г) меряется не та величина (видимый
ответ, а не контекст к LLM). **Фикс:** мерить downstream model-context токены (не видимый ответ); разрешить adaptive-входу
отличаться, сохранив task-identity в отдельном поле (снять правило идентичного prompt_hash); добавить delta-арифметику + позитивный
reduction-гейт, связанный с non-regression качества; подключить реальный вызов трейса в общий сервис. Claude/Codex token-evidence —
MISSING (в пакете только `*cursor-row12*`).

---

### P28 — semantic-quality: почему гейт мёртв / false-by-construction (5 новых дыр)

**Прочитано:** `Quality03ProofContractRunner.cs` (955+, по срезам), контракты, `ExtensionEvalOnlyQualityRunner.cs`/
`ExtensionQualityScoring.cs`; артефакты приёмки `parent-review-QUALITY-03.json`, `quality-comparison-report.json` (96 rows),
`quality-scores-adaptive/baseline.jsonl`; «починенные» source-disjoint прогоны phase-04 + QSG-24..27; `acquire-defer-matrix.json`.

**Реального ИЗМЕРЕННОГО семантического качества трёх расширений нет.** QA-слой двухъярусный, и оба яруса не производят числа качества:

- **Ярус 1 — `Quality03ProofContractRunner`** — по собственной метке (`:14`) «local deterministic QA/proof-contract artifact checker;
  **not semantic model, trainable unit, product readiness, or quality claim authority**». Он лишь агрегирует ПРИСУТСТВИЕ 63 blocking
  proof-ref строк (7 категорий × 9 rows). Зашито: `QualityPassRows:0` (`:304`), `RunnerScorerRun:false` (`:307`),
  `RunnerReadyEvalInputsCreated:false` (`:306`); `WhatThisDoesNotProve` (`:316`) прямо отрицает semantic quality. Даже на 63/63 он лишь
  «unlocks» отдельный eval-only scorer (`:280`), который тут НЕ запускается.
- **Ярус 2 — фактический scorer** — на диске только как FIXTURE: 96-строчный `quality-comparison-report.json` — константный
  `baseline=adaptive=2/5`, `delta 0`, `verdict FAIL_INCONCLUSIVE_REVIEW`, `reviewer_id "QUALITY-03-fixture-comparison-reviewer"`, каждая
  из 4 rubric-слоёв с note «No real … output evidence was evaluated». Родитель при этом `verdict ACCEPTED_FOR_PHASE_COMMIT`, но
  `semantic_quality_claimed:false`, `quality_result_claimed:false`.

**Почему гейт мёртв/false-by-construction (три причины):**
1. Агрегирующий гейт Quality03 по собственной метке — presence-checker, `QualityPassRows` зашит в `0`; фактический scorer — отдельный
   несделанный шаг.
2. Очередь proof-contract застряла: `acquire-defer-matrix.json` (2026-06-21) `pass 9 / missing 45 / deferred 9` из 63, `approvals 0/9`,
   `runner_ready 0/9`, `quality_pass 0/9`. Проходят лишь 9 held_out_split (авто-PASS по наличию corpus authority); остальное — «no-scoring»,
   заблокированное **отсутствием человеческого одобрения** redacted eval-входов ⇒ scorer никогда не запускался.
3. «Починенные» source-disjoint прогоны формально `PASS`/`accuracy 1`, НО проходят ИМЕННО потому, что качество НЕ заявляется: QSG-24
   `evaluator_kind material_only_..._not_model_runtime`, `staged_checkpoint_used_for_generation:false`, `real_training_or_continue_
   training_run:false`; «score» = бинарное покрытие материалом `without_adaptive 0 → with_adaptive 1` (тавтология); gate
   `semantic_quality_claimed_false:true` ОБЯЗАН быть true. phase-04 — `baselineInternalOnly:true`, детерминированный keyword-путь (P21).

Пять новых дыр (в мастере): (1) QUALITY-03 96-строчный отчёт = COPIED_DOC_ONLY/FIXTURE, принят под phase-commit, но «PASS» относится к
форме отчёта, не к качеству; (2) Quality03ProofContractRunner как «semantic quality gate» = presence-checker, не измерение; (3) очередь
9/63, `quality_pass 0/9`, `approvals 0/9` — eval-входы не одобрены (человеческий блокер); (4) «починенный» source-disjoint eval (QSG-24)
= material-coverage тавтология, модель не запускалась; (5) inter-rater адъюдикация — только одиночный fixture-ревьюер (proof-ref, не
quality-pass). **Числа:** `semantic_quality_claimed:true` из реального прогона = **0 файлов** (все «true» — исправляемые на `false`
provenance-repair-флаги; `false` — **2321 файл**); 96/96 rows `FAIL_INCONCLUSIVE` константа 2/5.

---

### P29 — dashboard + docs: route-tied QA (7 новых дыр)

**Прочитано:** `43.SmartSkillsDomain.Dashboard/ApiDashboardRunner.cs` (285), `ApiDashboardViewModel.cs` (445),
`ApiDashboardFixtureRunner.cs` (328), `ApiDashboardSourceDisjointGuard.cs` (316), `ApiDashboardResultPresenter.cs`, `.csproj`;
`Tests.ApiDashboardWpf/ApiDashboardWindowInteractiveTests.cs` (1758, срезы); артефакты `p33/p38/p42`, `phase-04/05/06 dashboard-qa-
addition-result.json`. Грепы `liveDashboardRun:true`/`dashboard_runtime_called:true` по всем artifacts → **0**.

**Честная картина:** дашборд — внутреннее .NET WPF-окно (`WinExe net9.0-windows`), строит `XiigenExtensionRequest`, зовёт core-границу
`ProcessHostExtensionRequest`, рендерит ответ с честными boundary-флагами. Добросовестный Path-A boundary-инструмент. **Реальная
UX-автоматизация есть и нетривиальна** (AutomationPeer: accessible-имена, tooltips, no-jargon, неперекрывающиеся колонки) — закрывает
часть AR-22 лучше, чем Claude (P12). НО «route-tied QA», которую пакет числит доказательством (P01 phase-3), при проверке — не
выполнение маршрута, а три parse/mock-слоя:

- **G29-1** — `ApiDashboardFixtureRunner` = parse-only guard, никогда не запускает eval (`:38` guard-scope; `:215-219` NextAction «P40
  stops before eval/model execution»; `:235-238` хардкод `TrainingExecuted:false/EvalExecuted:false`; self-recorded `P37-GAP-02`).
- **G29-2** — `ApiDashboardSourceDisjointGuard` структурно не доходит до eval (`:133` `CanProceedToEvalRunner:false` всегда; любую
  реальную execution-строку помечает «unsafe execution flag»).
- **G29-3** — вся зафиксированная route-tied QA = parse-only/wording, ни одного live route-run (`p33 validationOnly:true,
  liveDashboardRun:false`; `p38 dashboard_runtime_called:false, eval_run:false`; `p42` — wording-firewall; грепом live-флагов → 0).
- **G29-4** — per-phase «dashboard-qa-addition» сам отключается, когда меняется только ядро (`phase-06 .../dashboard-qa-addition-
  result.json` `applicable:false, reason "SkillSelection core train/eval/export changed; UI surface not changed"`). QA декуплена от
  поведения core → не ловит core-регрессии через дашборд.
- **G29-5** — Copy/Export дашборда = in-memory заглушки (нет буфера/файла): `ViewModel:290-294` `CopyResultShell` только
  `LastCopiedText`; `:296-302` `ExportHistoryShell` только строка; грепом `Clipboard.`/`File.Write`/`SaveFileDialog` → **0**.
  «Export history» не производит артефакт.
- **G29-6** — shipped default-путь дашборд→реальный core не покрыт ни одним тестом; каждый тест инъектирует фейковый
  `RecordingGateway` (`:27,:146,…:1738-1758`); «негативные» сценарии — литеральные константы, а не воспроизведённые коды отказа ядра.
- **G29-7** — WPF route-tied QA требует STA + реальный desktop `window.Show()` (`:21,:25,:33`, `net9.0-windows/UseWPF`); не
  headless/CI-гейт, нет свидетельства входа в baseline ≥1488.

Позитив: каталог дашборда покрывает все 5 контрактных методов (PromptToTask/OrchestrationPlan/SkillSelection/Feedback/DebugTrace) —
в отличие от Cursor (P14); boundary-флаги честные и видимые в UI.

---

### P30 — lifecycle-ops: что реально работает из export/import/backup/rollback/continue (8 новых дыр)

**Прочитано:** `Phase08CommonUserTwoLegLifecycle.cs` целиком по срезам (методы `ExportUserModel:492`, `RestoreUserModel:562`,
`CreateApprovedShareCopy:623`, `ContinueTrainingFromCheckpoint:668`, `ApplyCommonUpgrade:728`, `RollbackCommonUpgrade:789`);
вызыватели (grep → только `Tests/Phase08...Tests.cs`); контракт хоста; артефакт-пакет `phase-08-common-user-two-leg-lifecycle/*`;
второй lifecycle-family `p16f-skill-selection-lifecycle-evidence/*`.

**Реальной ОПЕРАЦИИ жизненного цикла у трёх расширений нет.** То, что выглядит как «executable lifecycle probe», — это
юнит-тестовый прогон доменного сервиса над синтетическими данными, который: (1) round-trip'ит **555-байтный metadata-огрызок**
(список signal-id), не обученную модель; (2) запускается только из xUnit (в контракте хоста нет lifecycle-операции); (3) называет
«continue-training» инкремент счётчика строк (2→3), а «share» — манифест без переноса/анонимизации; (4) закрыт лишь для 1 из 4 сфер,
и собственная матрица честно ставит `passed:false`. Пакет сам НЕ заявляет готовности (`explicitNonClaims`) — это добросовестный
scaffold; дыры P30 — про дистанцию до реального пакета Board Phase 15 и про overclaim в ИМЕНАХ полей.

| # | Операция | Реальный статус | file:line |
|---|---|---|---|
| G30-1 | lifecycle-операция на границе хоста | **MISSING SURFACE** — в `XiigenExtensionRequest` нет типа backup/export/import/share/continue/rollback; вызыватели сервиса = только Tests | grep lifecycle-op в контрактах→0 |
| G30-2 | что переносит export/import | **STUB PAYLOAD** — `Phase08PersonalCheckpoint` = `TrainedSignalIds(list<string>)`; `personal-adaptive-checkpoint.json` = **555 bytes**; рантайм грузит ДРУГОЙ файл | `:56-67`; пакет 555b sha `99c8809f` |
| G30-3 | «continue-training» (не-no_op ветка) | **STUB METRIC** — `trained = Concat(New).Distinct()`, `MetricImproved = len>before` тривиально true; отчёт `metricBefore:2→after:3` | `:695-720` |
| G30-4 | поле `SourceHashesVerified` | **METRIC-HONESTY** — имя лжёт: НЕ сверяет хеши, лишь проверяет отсутствие абсолютного пути | `:559` `!files.Any(ContainsAbsoluteUserPath)` |
| G30-5 | «approved anonymized share» | **STUB** — `Anonymized:true`/`ContainsRawPrivateText:false` — захардкоженные булевы; файл не копируется, пишется только манифест | `:623-665` |
| G30-6 | common-upgrade + rollback | **DISCONNECTED TARGET** — прогнаны над одноразовым `test-work/.../common-upgrade-sandbox` манифестом, реальный `models/.../manifest.json` не тронут | `commonManifestPortableRef=…sandbox…` |
| G30-7 | personal-нога 3 из 4 сфер | **MISSING ARTIFACT** — `sha256:"not-created-yet"` для prompt-to-task/orchestration/result-gap; probe только у skill-selection; матрица `passed:false` | `evidence.json:132,142,162,85,117` |
| G30-8 | два расходящихся lifecycle-«доказательства» skill-selection | **PROVENANCE DIVERGENCE** — Phase08 (555b, continue 2→3) vs p16f (`2507AB03`, continue `delta_applied:false` no-op, fresh-load «не production/не common»); ни один не трогает shipped-runtime checkpoint | Phase08 vs p16f |

**Определение НАСТОЯЩЕГО lifecycle-пакета (10 требований, для плана):** реальные байты модели (боевой checkpoint, byte-identity +
функц. эквивалентность); триггер с host/extension-поверхности (Path A+B); continue-training = реальное обучение (held-out после>до);
fresh-load improvement измерен; rollback над боевым ассетом с post-rollback load-probe; share = реальная анонимизация с проверкой
сканом; integrity на import/load; все 4 сферы; единый source-of-truth; cross-device импорт в свежий профиль.

---

### P31 — scenario-inventory: полный 28-сценарный инвентарь + счётчики (0/28)

**Канонический инвентарь:** `product-readiness-scenario-board.json` (1138 строк) — существует и в пакете, и в РЕАЛЬНОМ репо
(`docs/plans/evidence/extension-full-readiness-dual-path/`), обе копии идентичны: `generated_at_utc 2026-06-27T00:13:32Z`,
`counts.total 28`, `full_joined_pass 0`, `product_readiness_full_pass 0`, `remaining 14`. Последняя активность цепочки
(`next-scenario-selection-after-row28-...012724`) = `decision_status NEEDS_SCENARIO_REPAIR`, `selected_next.scenario_number 1` —
исполнение ушло НАЗАД в planning-repair (пере-декомпозиция Row1). Ничего новее 2026-06-27.

**Таксономия — 9 типов сценариев на хост + 1 лишний у Codex:**
1. product-readiness-product-workflow · 2. full-extension-entrypoint · 3. token-savings-baseline · 4. semantic-quality-heldout ·
5. learnability-training-continue · 6. dpo-quality-provenance · 7. asset-lifecycle-roundtrip · 8. checkpoint-promotion-reload ·
9. locator-update-reload · **+ Codex 10-й:** `frdp-p01-codex-codex-chat-ui-marker` (row 21). Итого **9 Claude + 9 Cursor + 10 Codex = 28**.

**Счётчики покрытия (пересчитаны по board_category строкам):**

| Категория | Claude 1-9 | Cursor 10-18 | Codex 19-28 | ИТОГО /28 |
|---|---|---|---|---|
| full_joined_pass (Path A+B same-scenario) | 0/9 | 0/9 | 0/10 | **0/28** |
| product_readiness_full_pass | 0/9 | 0/9 | 0/10 | **0/28** |
| blocked_with_next_action | 9 | 0 | 3 (24,25,26) | 12 |
| partial | 0 | 5 (11,12,16,17,18) | 2 (19,20) | 7 |
| scoped_accepted | 0 | 1 (15) | 5 (21,22,23,27,28) | 6 |
| accepted_narrow_scoped | 0 | 2 (13,14) | 0 | 2 |
| not_moved | 0 | 1 (10) | 0 | 1 |
| Path B с любым визуальным доказательством | 0/9 | ~4/9 (screenshot 11; webview 12,13) | 0/10 (0 pixel, P20) | << 28 |

**Итог: 0 из 28** достигли full joined pass. 14 «accepted» рядов (12,13,14,15,16,17,18,19,20,21,22,23,27,28) — ВСЕ
`formal_joined_pass=false`, narrow/scoped. Настоящая dual-path готовность = **0/28**. Summary-объект `counts` в board внутренне
противоречив (заявляет `partial 14/blocked 14/scoped_accepted 14/accepted_narrow_scoped 14` — не раскладывает 28) и раздут ~40
кумулятивно-историческими ключами.

**7 новых дыр G31-1..7:** (1) 0 accepted по обеим осям; (2) канонический 28-сценарный инвентарь НЕ выведен в пакетный board 2026-07-08
(свёрнут в 3 фазовых ряда 5/6/7 «PARTIAL»), сам инвентарь устарел (2026-06-27) и застрял в NEEDS_SCENARIO_REPAIR; (3) `counts`
внутренне противоречив + денормализован; (4) Claude 0/9 по обеим осям, Path B = 0/9; (5) «same-scenario» не выполнен даже в accepted
рядах — НИ ОДИН ряд не содержит поля сравнения Path A↔Path B (board определяет `required_match`, но accepted Cursor ряды сравнивают
РАЗНОЕ); (6) асимметричная матрица host×type (Codex=10) → знаменатели несопоставимы; (7) mojibake в рядах 6, 11, 15.

---

### P32 — claude-scenarios: по-сценарный разбор (0/9, join off-board)

**9 канонических Claude-сценариев (board rows 1-9) — ВСЕ `blocked_with_next_action`; Path B по всем 9 = «not proven/not available».
0/9 joined dual-path pass.** Ключевые разоблачения по-сценарно:

- **Единственный существующий Claude dual-path JOIN — ОФФ-БОРД, generic prompt.** `claude-same-scenario-dual-path-join-20260628-
  145508/result.json`: `status CLAUDE_SAME_SCENARIO_DUAL_PATH_JOINED`, `same_prompt` = «create a safe plan for documenting an API
  dashboard» (generic prompt-to-task проба), `ui_work_run:false, adapter_run:false`. На board НЕ отражён (все 9 blocked). Единственный
  «joined» артефакт и 9 требуемых scenario_id **ДИЗЪЮНКТНЫ**.
- **Path A «PASS» = bespoke `node.exe` CLI с рукотворным каталогом скиллов.** `command-log.md`: `node.exe src\claude-entrypoint.js
  -Method prompt-to-task -RequestedSkills ... -AvailableSkills xiigen-smart-skills,api-documentation,planning,documentation`.
  Непустой `selected_skills` получен ТОЛЬКО потому, что драйвер вручную подал `-AvailableSkills`; shipped Claude-поверхность
  (SessionStart-hook, P09) его НЕ передаёт (P10 gap-2: пустой каталог → `NO_AVAILABLE_SKILLS_PROVIDED`). **Невоспроизводим через реальный Claude-хост.**
- **Path B = re-photograph уже открытого окна, БЕЗ промпта и хеша.** `claude-path-b-prompt-submission-...`: `status
  CLAUDE_PROMPT_SUBMITTED_FINAL_VISIBLE_RESPONSE`, `no_new_prompt_submitted:true`, нет полей prompt/prompt_hash; action-log —
  «packetization worker» перефотографировал `current-claude-window`. Ни OCR, ни submit.
- **Path B ведёт НАТИВНЫЙ Claude Desktop композер, не XIIGen-расширение.** `claude-path-b-composer-input-proof-...`:
  `BLOCKED_UIA_HELPER_ARGUMENT_TYPES`, `route_selected:false, composer_found:false, prompt_sent:false`; blocker — `Get-RedactedUiaSummary`
  упал в `run/claude-composer-input-proof.ps1:220/:377`. Механизм — драйв собственного чат-поля Claude Desktop; XIIGen-хук в петле
  Path B НЕ участвует ⇒ видимый ответ = собственная модель Claude, доказательства участия XIIGen нет.
- **Разные промпты.** Join/Path A hash `305fb4c0` («document an API dashboard») vs типовой entrypoint Path B hash `52846bd2` (460 симв.).
  Ни один единый scenario-промпт не имеет одновременно валидного Path A и реального Path B.
- **В пакет скопирована ТОЛЬКО join-папка** (2.4КБ result.json + evidence-map); НЕ скопированы ни Path A `response.json` (36КБ), ни
  8 Path B PNG. Единственный Claude dual-path артефакт в пакете — бумажный status-matcher, ссылающийся на абсолютные пути реального репо, которых в пакете нет.

**5 новых дыр:** off-board generic join ≠ 9 канонических; Path A через сфабрикованный `-AvailableSkills`; A↔B same-prompt identity
недоказуема (Path B без промпта/хеша); Path B ведёт нативный Claude, не XIIGen; Path B = сломанная UIA-цепочка ремонта, typed+sent
прогон не исполнен.

---

### Сквозной вывод прохода 5 (для плана VS/VS Code)

Это доказательный «пол» всей сборки. По зависимостям:
- **Обучение (M1):** реальный DPO существует, но отсоединён от расширений и в единственном исполненном прогоне регрессировал
  (loss↑ к ln2, margin↓ ~3×, held-out ≈ шум); delta-метрик нет во всех 88; continue-training никогда не исполнялся на реальных строках.
- **Checkpoint/locator:** весь `25.ModelServer` обойдён; версия зашита в код (v2 = пересборка); integrity-гейта нет; promotion-гейта у
  расширений нет; rollback как операция отсутствует; `IModelServer`/proxy — мёртвые/эхо.
- **Token-savings:** delta структурно 0 — сервис не эмитит трейс, нет арифметики редукции, схема запрещает разный вход (идентичный
  prompt_hash) и запрещает claim, меряется видимый ответ вместо контекста к LLM.
- **Semantic-quality:** гейт мёртв/false-by-construction — presence-checker с `QualityPassRows` зашитым в 0; единственные числа —
  константный FAIL-fixture 2/5 и бинарное покрытие материалом; PASS обусловлен `semantic_quality_claimed=false`; scorer заблокирован на
  неполученном человеческом одобрении eval-входов (`approvals 0/9`).
- **Lifecycle:** реальной операции нет — 555-байтный metadata-огрызок, вызов только из xUnit, «continue» = счётчик строк, «share» =
  манифест без анонимизации; 3 из 4 сфер checkpoint `not-created-yet`.
- **Сценарии:** 28 = 9 Claude + 9 Cursor + 10 Codex; **0/28** full joined pass; 14 «accepted» все `formal_joined_pass=false`; Claude
  0/9 по обеим осям, Path A через сфабрикованный каталог, Path B = нативный Claude/сломанный UIA.

**Прямое следствие для VS/VS Code-плана:** порт на VS Code упирается в тот же обучающий узел (M1→M2). Даже самый зрелый Cursor-хост
не даст доказуемого token/quality/lifecycle/scenario-результата, пока (а) обучение не де-рискнуто, (б) схема token-trace не разрешает
разный вход и не считает контекст к LLM, (в) semantic-quality не отвязан от `claim=false`, (г) checkpoint/locator/lifecycle не
подключены к рантайму через `25.ModelServer` с integrity-гейтом.

### Что остаётся на проходы 6-10

- **Пройдено к концу прохода 5:** opus-pass 01-32. Ядро обучения/lifecycle/качества (P25-30) + инвентарь сценариев (P31) + Claude
  по-сценарно (P32) — ЗАКРЫТЫ.
- **Проход 6 (opus-pass 33-40):** cursor-scenarios (P33), codex-scenarios (P34), pathA-harness (P35 — Path A обходит расширение,
  in-process fallback), pathB-desktop (P36 — re-photo/stale/0 видео), core-boundary (P37 — редакция/honesty/locator триплицированы в 3
  адаптерах, R6), trainable-audit (P38 — R4 governance-downgrade), tests-qa-state (P39 — нет полного `.trx` baseline), git-branch-state
  (P40 — 331 файл дельты не закоммичен, 9 worktree).
- **Проход 7 (opus-pass 41-49):** adversarial-recheck (P41), hidden-overclaims (P42), missing-scenarios (P43), integration-e2e (P44 —
  нет versioned wire-contract/CorrelationId/UserPromptSha256), provenance-legal (P45 — sonnet-derived строки, distillation без
  teacher-гейта), ux (P46), perf (P47), priority-sequencing (P48 — критический путь M0→M6), risks (P49 — RCE-вектор).
- **Проход 8-9 (opus-pass-50 + `opus-PROJECT-CLOSURE-PLAN.md`):** консолидированная roadmap M0-M6 + накопительный реестр ~245 дыр.
- **Проход 10:** copied-plans (30) + supporting-files (108) + 2 транскрипта — source-addressability и evidence-инвентарь.

### Границы честности прохода 5

- claim_scope: полное чтение opus-pass-25..32 + извлечение исполненных чисел обучения, механики checkpoint/locator, корня token-delta=0,
  причины мёртвости semantic-quality, статуса lifecycle-операций, 28-сценарного инвентаря и Claude по-сценарно — с file:line. Русский, read-only.
- evidence_scope: прочитаны ПОЛНОСТЬЮ 8 файлов opus-pass-25..32; выводы привязаны к file:line ИЗ ЭТИХ ПРОХОДОВ. Сам живой код
  (`43.SmartSkillsDomain`, `25.ModelServer`, `11.DPO`, `44.DesignConversionDomain`), промоутнутые `models/*`, артефакты
  `artifacts/*`, датасеты и `product-readiness-scenario-board.json` в этом проходе НЕ читались напрямую (цитируются по Opus-аудиту —
  родитель верифицирует против живого кода). Таблицы дыр P26/P30 вынесены авторами в мастер `opus-PROJECT-CLOSURE-PLAN.md`; здесь
  извлечены их file:line-факты. Тесты/сборки/git/runtime НЕ запускались.
- НЕ доказано этим проходом: любые product-readiness утверждения; точность каждого per-pass file:line-вывода (нужна верификация
  родителя против живого кода `C:\Projects\...`).
- subagent_id: UNKNOWN_IN_SUBAGENT. requested_model: gpt-5.5-equivalent (Opus 4.8 Ultracode). requested_reasoning_effort: xhigh.

## Pass 6/10 — opus-pass 33-40: cursor/codex scenarios + pathA-harness + pathB-desktop + core-boundary R6 + trainable-audit R4 + tests-qa + git-branch

Источник: 8 файлов opus-pass-33..40 прочитаны ПОЛНОСТЬЮ (Opus 4.8, дата аудита 2026-07-08). Все выводы
привязаны к file:line из этих проходов; родитель верифицирует против живого кода `C:\Projects\...` перед
проходом 7. Это архитектурно-граничный + proof-harness + git-реальностный срез. Дедуп: не повторяю
P25-P32 (обучение/lifecycle/качество/инвентарь/Claude по-сценарно, уже в проходах 1-5).

### 1. Cursor scenario coverage (P33) — 0/9 full joined pass

Cursor = 9 канонических рядов board (rows 10-18), файл `product-readiness-scenario-board.json`. Реальные статусы:
- **row10** product-workflow — `not_moved`; Path A «insufficient», Path B «Not run locally».
- **row11** full-extension-entrypoint — `partial`; Path A: panel-команда из Cursor Extension Host, но **core response ПУСТОЙ**; Path B: скриншот Cursor с XIIGen-панелью **без промпта/ответа**. `formal_joined_pass=false`.
- **row12** token-savings-baseline — `partial`; token delta 0, reduction false; Path B submit сделан **программным `HTMLElement.click`, не ручным** («Manual human click is not proven»).
- **row13** semantic-quality-heldout — `accepted_narrow_scoped`; proxy permutation 12/12 с оговоркой **«exact held-out prompt text was unavailable»**; UI-прогон выбрал ДРУГОЙ промпт, не held-out.
- **rows 14-18** (learnability, dpo, lifecycle, checkpoint, locator) — все `scoped_accepted`/`partial` на **temp-folder core-only**, каждый явно «No UI run is claimed» / «Not run locally».

Единственный Cursor dual-path JOIN (`cursor-same-scenario-dual-path-join-20260628-150216`) привязан к сценарию `cursor-full-extension-runtime-after-unlock`, которого **НЕТ в 28-рядном board** (`grep -c full-extension-runtime`=0). 6 новых дыр P33:
- **JOIN off-board**: единственный «joined» двигает off-board runtime-пробу; 9 канонических Cursor = 0 joined.
- **Path A обходит расширение**: `route_used:"direct XiigenExtensionRequest API/core boundary fallback"`, `exact_cursor_adapter_cli_used:false`, `ui_used:false` — это in-process core-fallback (тот же, что в тестах), НЕ поставляемый `extension.ts`→powershell adapter.
- **Path B stale-response**: submit-нога сама пишет «блок ответа был на экране ДО submit → новый ответ не доказан»; final-response-debug **не шлёт нового промпта**, а ре-фотографирует старую панель.
- **Ноги сшиты из разных сессий/дней**: на момент Path A (06-27 23:59) его Path B была ЗАБЛОКИРОВАНА Storage modal; join через сутки сшил отдельный Path B (06-28). Не один связный прогон.
- **Dev-host + эфемерное хранилище + нет text-oracle**: Path B — это F5 dev-load `--extensionDevelopmentPath`, не установленный `.vsix`; payload ответа в volatile `AppData\Roaming\Cursor\...\globalStorage\` ВНЕ пакета; UIA-снятие текста провалилось (`"Argument types do not match"`) → текст ответа машинно НЕ captured.
- **Rows 14-18 = 0/5 Path B UI**: 5 capability-сценариев вообще без локального UI-прогона, только temp-folder core.

### 2. Codex scenario coverage (P34) — 0/10 full joined pass

Codex = 10 канонических рядов board (rows 19-28). Единственный Codex JOIN привязан к off-board `codex-full-extension-runtime` (`grep`=0). 8 новых дыр (G1-G8):
- **G1 off-board JOIN**: 10 канонических = 0 joined pass.
- **G2 Path A обходит поставляемый CLI**: shipped CLI дал **ПУСТЫЕ skills** (потеря JSON-кавычек в Windows PowerShell); «PASS» получен обходом CLI через direct-module + **рукотворные string HostFacts** (core отверг boolean `samePrompt`); workingDirectory = installed-cache копия `.codex\plugins\cache\...`; `available_skills=selected_skills=["xiigen-smart-skills"]` — **плагин выбирает сам себя** (само-каталог).
- **G3 Path B accepted ПРОТИВОРЕЧИТ own overclaim-repair**: submit-нога `accepted:false`, `PARTIAL_SUBMITTED_RESPONSE_PENDING_OR_TIMEOUT`, `final_stop_button_still_visible:true`, старый claim «visible response appeared» помечен как OVERCLAIM. JOIN всё равно объявил `accepted_visible_assistant_response_captured` — развернул binding overclaim-repair в принятие.
- **G4 «видимый ответ» = сам evidence-ledger XIIGen**: UIA-текст = «Доказано: installed_skill_surface_loaded=true… Не доказано: full_codex_chat_ui_proven=false…», `TEXT_LEN_168` — это статус-леджер самого пакета, вставленный в композер и сфотографированный, **не генерация модели Codex** (вход≈выход, циклично); `new_prompt_submitted:false`.
- **G5 row21** (chat-ui-marker): `blocker_class:UI_INPUT_CONTROL_NOT_FOUND`, `prompt_typed:false/sent:false` — блокер (композер не найден, ничего не введено) принят как `scoped_accepted`/`PASS_SCOPED_BLOCKER`.
- **G6 row23** semantic-quality: **18 evidence-папок (ceremony), 0 семантического deliverable**; фактически `BLOCKED_NO_APPROVED_INPUT_PATH`, approved inputs=0 — но board_category `scoped_accepted` завышает полностью заблокированный ряд.
- **G7 rows 24/25/26** (learnability/dpo/asset-lifecycle) — **нет собственной Codex-папки вообще**; их «evidence» = общие phase-03/04 blocker-audit артефакты.
- **G8 row19** «JOINED_COMPARISON_PASS» — на деле packaging-пакет БЕЗ host-UI-действия (`host_ui_action_by_this_packet:false`, «did not run host UI actions»), ответ не полностью видим; rows 22/27/28 `scoped_accepted` на staging/no-send с Path B not_proven/blocked.

**Итог сценариев (P33+P34, свод с P31/P32):** 28 сценариев = 9 Claude + 9 Cursor + 10 Codex; **0/28 formal_joined_pass, 0/28 product_readiness_full_pass**. Все три «единственных» dual-path JOIN — off-board пробы одного и того же несуществующего в board сценария `*-full-extension-runtime`, у каждого Path A обходит реальный host-путь, а Path B либо stale re-photo, либо противоречит собственному overclaim-repair, либо фотографирует статус-леджер вместо ответа модели.

### 3. Path A харнесс-реальность (P35) — автоматическая boundary существует, но раздроблена и НЕ покрывает обучаемость

- Единственный .NET-тест-проект — **монолитный `Tests/LlmFromScratch.Tests.csproj`** (xUnit, 233 `*.cs`, ссылается на 40 проектов). Граница расширения покрыта **в-процессе**: `XiigenExtensionApiContractsTests.cs` (1741 стр.) зовёт `service.ProcessHostExtensionRequest(request)` напрямую — это и есть «Path A boundary», но это **ровно тот in-process core-fallback**, что P33/P34 пометили «не host-route». Тесты валидируют контракт/редакцию/no-hidden-determinism, НЕ семантическую корректность (ядро by-construction `semantic_quality_claimed=false`).
- **35-1**: нет единой воспроизводимой Path A харнесс — ~11 ручных `dotnet test --filter` + python-команд, результаты вручную переписаны в markdown; **нет runner-скрипта, нет `.runsettings`, нет CI** (`.github/workflows` отсутствует).
- **35-2**: **57% сценариев (16/28) НЕ имеют вызываемой авто-boundary-команды** (`NO_BOUNDARY_COMMAND_DISCOVERED`=7, `BOUNDARY_COMMAND_INSUFFICIENT`=9); accepted boundary-packets только 12/28.
- **35-3**: сквозной `product-readiness`-конвейер Path A харнесс **не существует** — только фрагментарные guard-тесты, нет команды, гоняющей prompt→task→plan→skills→token→quality→lifecycle→checkpoint→locator как одно целое.
- **35-4**: принятая граница **ПО ПОСТРОЕНИЮ исключает все train/mutation-тесты** → именно обучаемость/DPO/lifecycle/checkpoint-promotion/locator выпадают из авто-границы (харнесс структурно ограничен read-only guard-фрагментами).
- **35-5**: единственный процесс-спавн (full-host-scorer) — **Windows-only** (`FileName="powershell"` 5.1, не `pwsh`) и жёстко зависит от закоммиченных артефактов (`Assert.True(Directory.Exists(adaptiveRoot))`) → на чистом клоне/Linux падает по среде, не по продукту.
- **35-6**: **собираемость харнесса СЕЙЧАС не доказана и под риском** — тяжёлые незакоммиченные правки в `44.DesignConversionDomain` + `Tests`, TestResults устарели на 5 дней, зелёного билда под текущее дерево нет. Ошибка компиляции в `44.` заблокирует ВЕСЬ Path A-харнесс.

### 4. Path B десктоп-реальность (P36) — фактически отсутствует для Claude/Codex, marker-only кроп для Cursor

Агрегат по всему дереву `extension-full-readiness-dual-path\`:
- **Видео = 0** во всём дереве (`*.mp4/webm/gif`=0), хотя схема требует `screenshot_video_log_evidence`. Видео-нога Path B никогда не производилась.
- **288 PNG**, но каноническая автоматическая local-desktop-папка (`phase-04-local-desktop-evidence/`) содержит **claude=0, codex=0, cursor=1 PNG**. Для всех трёх хостов `safe_marker_smoke_status: "NOT_RUN"` (причина: «GUI может раскрыть аккаунт/историю/воркспейс»). Слайс нашёл только пути/процессы/ярлыки, **0 захватов host-UI**.
- **Единственный принятый канонический скриншот = кроп текстового РЕДАКТОРА Cursor** (`PASS_LOCAL_DESKTOP_MARKER_ONLY`, «marker text visible in Cursor editor crop»; «no Cursor chat, no prompt/message send») — доказывает лишь, что `Cursor.exe` установлен, НЕ что расширение запускается/отвечает.
- **Инструмент захвата однажды снял СОБСТВЕННЫЙ Codex-чат ассессора** вместо целевого хоста (`rejection_reason: "screen capture returned the current Codex conversation surface"`, `REJECTED_AND_DELETED_PRIVATE_SURFACE`) — провенанс окна ненадёжен, нет управляемого monitor/window-overlap fallback.
- Все 288 PNG живут в ad-hoc `scenario-execution/*`, уже даунгрейднутых P32-P34 как stale/re-photo/dev-host. В коммитнутый local-desktop deliverable попал **1 PNG**.

**Точные блокеры почему Path B UI-proof проваливается:** (1) политика приватности запрещает marker-smoke (риск раскрытия аккаунта/истории) → `NOT_RUN`; (2) захват экрана не наводится надёжно на целевой хост (снял чат самого агента); (3) UIA text-capture ломается (`"Argument types do not match"`) → нет текстового oracle; (4) payload ответа в volatile globalStorage вне пакета; (5) прогоны идут через dev-host (F5), не установленный `.vsix`; (6) видео-захват отсутствует полностью.

### 5. Core-boundary R6 verdict (P37) — семантика в core (✓), но НЕ-семантическая логика триплицирована в 3 адаптерах (✗)

**Положительное (R6 держится по семантической оси):** скиллы/скоринг/DPO/оркестрация правда в core. Все 3 адаптера честно декларируют `host_semantic_ranking:false` / `deterministicSkillSelectionAuthorityInHost:false` / `host_skill_selection_binding:"pass_through_only"`. Реальный dispatch и семантические строки в `XiigenExtensionApiService.cs`.

**Отрицательное (R6 нарушен по НЕ-семантической оси):** host-слой НЕ «maximally simple». **2424 строки bespoke host-логики без единого shared-SDK**, три языка/копии (cursor `extension.ts` 1345 стр/~62 функции; claude `claude-entrypoint.js` 535 стр/22 функции; codex `codex-entrypoint.js` 544 стр/30 функций). 6 утечек core-обязанностей:
- **G1 Редакция (privacy) переизобретена в 3 адаптерах и РАСХОДИТСЯ с core**: core `RedactPath` = repo-относительный путь; host = `path.basename` (только последний сегмент) → один asset root редактируется по-разному, единого владельца privacy-гарантии нет. Security-critical дрейф.
- **G2 Адъюдикация evidence/claim + «no-overclaim»** (главная honesty-гарантия проекта) вычисляется bespoke host-кодом (`normalizeResponse` сам считает `passed`, `noOverclaimReview` абзац-вердикт, хардкод `forbiddenClaims{}`), а не core-authority — в трёх расходящихся адаптерах.
- **G3 Host ФАБРИКУЕТ core-контрактные ответы при сбое** (`buildAdapterErrorResponse` строит целый `XiigenExtensionResponse`; codex `createVisibleAnswerDisplay`) — core-контрактная логика в адаптере.
- **G4 Видимый ответ ПОВТОРНО парсится/композируется в 3 хостах**, хотя core уже владеет `BuildChatDisplayText` (claude `stripCoreDebugDisplay` regex-режет core-вывод; codex `slice(0,markerIndex)`) — презентация триплицирована.
- **G5 Разрешение target-locator/checkpoint-ref в адаптере**, дублируя `IRemoteUnitLocator` (core/25.ModelServer) — каждый host сам `JSON.parse(fs.readFileSync(...))` локатора + `inferAssetRoot`.
- **G6 Нет общего adapter-SDK**; плюс `normalizeMethod` в host **МОЛЧА приводит неизвестный метод к `prompt-to-task`** — host принимает routing-решение и маскирует core-default.

**Вердикт R6:** BUILT_NOT_PROVEN архитектурный дефект границы (код реально существует, цитируется file:line). Не «logic leak семантики», а leak НЕ-семантических core-обязанностей (privacy/honesty/locator/фабрикация/презентация). Закрытие: вынести в core или общий тонкий SDK, чтобы host стал набором API-вызовов.

### 6. Trainable-audit R4 verdict (P38) — консолидированный hidden-determinism список

**Ключевой факт:** аудит trainable-архитектуры (`TRAINABLE-ARCHITECTURE-AUDIT-v1..v4` + EXECUTION-STATE, SS-01..SS-12 all `accepted`/`pushed`, `goal_reached:true`) «завершён» тем, что **перенёс сырые словари/счётчики в типизированные `ILogicalUnit`-state-записи** — исполнена БУКВА «allowed-level wrapper», а не суть R4. Сам поздний v3/v4 признаёт: три расширения остаются «честно промаркированными pattern-match baselines», `SemanticQualityClaimed=false`, «ещё не полноценные trainable semantic models».

**Консолидированный список hidden-determinism (что осталось детерминированным под меткой trainable):**
- **SkillSelectionMachine**: runtime-authority ЛЕКСИЧЕСКИЙ — `Score = Σ per-word learned weights`, argmax (из P23). Много typed dictionary/KNN state, «допустимо только потому что SemanticQualityClaimed=false».
- **PromptToTaskMachine**: ablation = структурный no-op (P21 gap-2) — обнуление весов НЕ деградирует выбор.
- **OrchestrationReasoning / TaskCompletionSkillUnit** (введён самим аудитом SS-12): несёт KNN (`FindNearestTaskCompletionExamples`, `ComputeTaskCompletionSupport`) + `LearnCompletionFeatureWeights` (лексические веса), но **без ablation-harness и без независимой leakage-проверки** — риск, что wrapper просто переупаковал ту же лексическую детерминированность.

5 новых дыр (G38-1..5):
- **G38-1**: `goal_reached=true` достигнут через relocation словарей; `actionable_gaps:[]` и `remaining_known_gap:"None"` **ПРОТИВОРЕЧАТ** v4 «Remaining work» — false-completion на уровне управляющего артефакта.
- **G38-2 governance downgrade**: аудит **переопределил R4** — детерминизм как final authority разрешён бессрочно «если SemanticQualityClaimed=false». Это ослабление требования Luba (NO HIDDEN DETERMINISM), а не выполнение; escape-hatch без срока перехода baseline→trainable.
- **G38-3**: §4-repair (ProjectWiki KNN role-labeling `knn_role`) **НЕ исполнен** — `git grep knn_role -- 42.ProjectWikiDomain/`=0; ни один SS-slice не касался `42.ProjectWikiDomain`.
- **G38-4 арх-асимметрия**: **эталон trainable-integrity УЖЕ есть в `42.ProjectWikiDomain`** (`OrchestrationPlanMachine`/`PromptFlowGroundingModel`): реальный weights-ablation тест (`Mode1Grounding_Ablation_EmptyWeightsDegrade`), input-only-feature leakage-guard, exact-prompt-leakage gate, target-label-leakage rejection. Аудит НЕ потребовал довести 3 расширения до этого паритета — closure не «изобрести», а ПОРТировать существующий паттерн.
- **G38-5**: юниты, введённые самим аудитом, прошли только parent-self-review, не независимый ablation/leakage-аудит.

### 7. Tests/QA state (P39) — обширна по объёму, но HONESTY-ONLY, без correctness-oracle и без e2e

- Extension-QA в ядре **существует и обширна** (десятки файлов, 236 ассертов в одном `XiigenExtensionApiContractsTests.cs`), но по содержанию — **про ЧЕСТНОСТЬ, не про СПОСОБНОСТЬ**.
- `Assert.False(...SemanticQualityClaimed)` — десятки; **`Assert.True(...SemanticQualityClaimed)` = 0** во всём наборе. Положительная ветка гейта semantic-quality **не имеет ни одного проходящего теста** (даже «positive-evidence» тест кладёт синтетический 1/3 токен и всё равно `Assert.False`).
- Флагманский `ExtensionToken04QualityScoringArtifactsTests.cs` (**222 КБ**) — **не запускает скоринг**: читает committed `token-04-*.jsonl`, ассертит counts (108 row/54 pair, по 36 на хост) и что ВСЕ флаги (`quality/semantic/learnability/token_savings *_claimed`)=false, `token_savings_claim_allowed_pairs=0`, `adaptive_effect_proven_pairs=0` — статический **null-result assertion**. Жёсткая привязка к артефактам: на чистом клоне коллекция падает на fixture-init (`FindRepoRoot` бросает `DirectoryNotFoundException`).
- **QA-integrity дефект**: в тест-сборке живёт не-тестовый governance-writer `ExtensionExecutionStatePatchWriter` — тесты ПИШУТ/мутируют `docs/plans` + `artifacts/` execution-state, а не только верифицируют.
- **e2e/host-слой для 3 расширений в ядре ОТСУТСТВУЕТ** — единственный `*E2e*` файл принадлежит домену design-to-code; вершина тест-пирамиды пуста; единственный process-spawn = локальный powershell scorer.
- **Зелёного полного baseline-прогона на диске НЕТ**: все `.trx` — узкие filtered (`total` максимум 5); CLAUDE.md-baseline «≥1488 passed, 0 failed» **не подтверждён ни одним воспроизводящим артефактом**; CI отсутствует.

**Вывод:** даже полностью «зелёный» extension-QA-прогон доказывает только «no overclaim», не «работает».

### 8. Git/branch/mergeability state (P40)

- **Положительный факт:** закоммиченное дерево `DesignToCode-clean` мержится ЧИСТО в обе базы: в original `codex/chat-gpt-5.5` (`git merge-tree --write-tree` exit 0, 0 CONFLICT) и в `main` (HEAD +2893/−0 ⇒ fast-forward). Инвариант «clean-mergeable в original И main» на уровне КОММИТОВ держится.
- **Центральная дыра (§1):** extension-продукт ЯДРА **НЕ закоммичен** — `git diff --shortstat` = **331 файл / +18 574 / −3 576**; `status --porcelain`=876 записей, 545 untracked. В незакоммиченной дельте — ключевые исходники: `XiigenExtensionApiService.cs` (сердце всех 3 расширений), `PromptToTaskMachine.cs`, весь `44.DesignConversionDomain/*` (22 файла +4717/−245), `42.ProjectWikiDomain/*`, `11.DPO/DpoTrainer.cs`. **«Чистая мержабельность» посчитана по committed-дереву БЕЗ этих 18,5 тыс. строк → бессодержательна.**
- **§2:** верхушка ветки **не запушена** (2 коммита), HEAD-коммит = governance; три+ подряд коммита «latest» без provenance — нарушение Rule 6.
- **§3:** инвариант мержабельности только СКОПИРОВАН в пакет (хиты лишь в `project-CLAUDE.md`/`project-AGENTS.md`/skills), **ни одной фактической evidence-строки** `mergeability: clean into ... AND main` для extension-работы.
- **§4:** ветка НЕ интегрирована — original `codex/chat-gpt-5.5` на **32 коммита впереди** (не втянуты), `main` на 2893 позади (чистый FF доступен, **никогда не сделан** — в `main` extension-кода 0).
- **§5:** **9 worktree/checkout'ов одного ядра** (DesignToCode vs DesignToCode-clean vs codex_sync + временные mergeability-worktree) — нет единого канонического extension-checkout, правки дрейфуют.
- **§6:** 3 host-расширения — **3 отдельных несвязанных GitHub-репо** (`lubochka/{claude,cursor,codex}-xiigen-smart-skills`), каждый свой `.git` на своём `main` (claude HEAD 06-23 dirty 2; cursor 06-28 dirty 7; codex 06-18 clean), отсоединены от истории ядра. **Нет атомарного версионного юнита core+host** — инвариант «clean into original AND main» неприменим через границу репо; host-репо устарели (Jun18-28 vs board 07-08).

### Что остаётся на проходы 7-10

- **Пройдено к концу прохода 6:** opus-pass 01-40. Сценарии Cursor/Codex по-сценарно, Path A/B харнесс, R6 core-boundary, R4 trainable-audit, tests-qa, git-branch — ЗАКРЫТЫ.
- **Проход 7 (opus-pass 41-49):** adversarial-recheck (41), hidden-overclaims (42), missing-scenarios (43), integration-e2e-gaps (44 — versioned wire-contract/CorrelationId/UserPromptSha256), data-provenance-legal (45 — sonnet-derived строки, distillation teacher-гейт), ux-humanreadable (46), perf-token-quality-proof (47), priority-sequencing (48 — критический путь M0→M6), risks-decisions-external (49 — RCE-вектор из §G3/G6 P37).
- **Проход 8-9 (opus-pass-50 + `opus-PROJECT-CLOSURE-PLAN.md`):** консолидированная roadmap M0-M6 + накопительный реестр дыр.
- **Проход 10:** copied-plans (30) + supporting-files (108) + 2 транскрипта — source-addressability и evidence-инвентарь.

### Границы честности прохода 6

- claim_scope: полное чтение opus-pass-33..40 + извлечение по-сценарного Cursor/Codex покрытия, реальности Path A харнесса и Path B десктопа, вердикта R6, консолидированного R4 hidden-determinism, состояния tests/QA и git/branch/mergeability — с file:line. Русский, read-only, только RAG-файл записан.
- evidence_scope: прочитаны ПОЛНОСТЬЮ 8 файлов opus-pass-33..40; выводы привязаны к их file:line. Живой код `C:\Projects\llm_mvp_core` и host-репо, board `product-readiness-scenario-board.json`, артефакты и git в этом проходе НЕ запускались/не читались напрямую (цитируются по Opus-аудиту — родитель верифицирует).
- НЕ доказано этим проходом: любые product-readiness утверждения; точность каждого per-pass file:line-вывода (нужна верификация родителя против живого кода).
- subagent_id: UNKNOWN_IN_SUBAGENT. requested_model: gpt-5.5-equivalent (Opus 4.8 Ultracode). requested_reasoning_effort: xhigh.

## Pass 7/10 — opus-pass 41-49: adversarial-recheck + hidden-overclaims + missing-scenarios + integration-e2e + provenance-legal + ux + perf-proof + priority-sequencing (M0-M6) + risks-decisions

Источник: 9 файлов `opus-pass-41..49-*.md` (папка `extension development`). Роль автора этих файлов — Opus 4.8 REAL-STATE ASSESSOR, оценка от РЕАЛЬНОГО кода `C:\Projects\llm_mvp_core` и host-репо, не от самоотчёта пакета. Рамка честности: `COPIED_DOC_ONLY != BUILT_NOT_PROVEN != PROVEN`. Ни один из 9 проходов не объявляет проект готовым; все строки ниже — блокеры до FULL implementation. Это **adversarial-верифицированный набор дыр + приоритезированный roadmap M0-M6 + решения-Luba**.

### 7.1 — P41 adversarial recheck: какие прежние находки ВЫЖИЛИ / скорректированы + 2 новые дыры

Метод: перепроверка НЕСУЩИХ (load-bearing) прежних дыр против ЖИВОГО дерева ветки `DesignToCode-clean` bounded-грепами. Итог части-recheck: **6 из 6 проверенных дыр подтверждены как всё ещё реальные; 1 коррекция (P23-закрытие P02 gap-4) подтверждена валидной**. Оверклеймов в выборке нет; anchor'ы (file:line) точны.

| Прежняя дыра | Вердикт recheck | Доказательство (P41) |
|---|---|---|
| P10 gap-1 dead semantic-quality gate (ни одна из 3 машин не эмитит `SEMANTIC_QUALITY_CLAIMED_TRUE`) | **CONFIRMED — всё ещё реально** | токен найден в 17 файлах = только Tests/tools/docs/artifacts + сам `XiigenExtensionApiService.cs` (const+проверка); НЕТ в `PromptToTaskMachine.cs`/`OrchestrationReasoningMachine.cs`/`SkillSelectionMachine.cs`. Гейт недостижим по построению. |
| P26/P05 promoted-checkpoint минует весь `25.ModelServer` | **CONFIRMED** | grep `IModelRepository\|IRemoteUnitLocator\|UploadAsync\|LocateAsync\|LoadWeightsAsync\|index.json` в `43.SmartSkillsDomain` → 0 хитов (обойдён и на записи, и на чтении). |
| P02 gap-4 order-leakage authority → **P23-коррекция** | **REFUTATION VALID — P02 gap-4 ЗАКРЫТ кодом** | `SkillSelectionMachine.cs:113-142` fallback возвращает `SelectedSkillIds:[]`,`Confidence:0.0`,`RequiresSecondPass:true`; `CandidateOrderLeakageGuardUnit` реально существует `SkillSelectionLogicalHierarchy.cs:271`, подключён `:146`. Позиционный счётчик БОЛЬШЕ НЕ авторитет выбора. Повторно не открывать. |
| P11/P14 метод `feedback` не обрабатывается сервисом | **CONFIRMED** | grep `feedback` в `XiigenExtensionApiService.cs` → 0; единственный online-сигнал adaptive DPO падает в дефолт task/plan/skills. |
| P40 gap-1 незакоммиченное грязное дерево ядра | **CONFIRMED — без изменений vs P40** | `git diff --shortstat` = 331 files, +18574/−3576; 545 untracked; 876 porcelain. Продукт не сохранён в коммитах. |
| P02/P21 anchor `PromptToTaskMachine.cs:90` `NotReadyTaskType="not_ready_no_semantic_task"` | **CONFIRMED — anchor точен** | точное чтение `:90`. |
| P20 gap-5 утечка секрета в закоммиченном evidence | **CONFIRMED, но НЕ систем­но** | ровно 1 файл (`reports/RUNTIME-04G-codex/.../summary.json`); раздувать в «системный» нельзя — честно ограничено единичным случаем. |

**НОВЫЕ дыры P41 (2 — инфраструктурные слепые зоны, которых не касался ни один из 40 проходов):**

- **G41-1 — Транспорт core-API компилирует ВЕСЬ `43.SmartSkillsDomain` через `dotnet run` на КАЖДЫЙ host-запрос.** `tools/xiigen-extension-core-api.ps1` — единственный НЕ-синтетический транспорт всех 3 хостов: `:111-131` создаёт временный `XiigenExtensionCoreApiInvoker.csproj` с `ProjectReference` на `43.SmartSkillsDomain.csproj`; `:133-181` пишет временный `Program.cs` (`new XiigenExtensionApiService(coreRoot).ProcessHostExtensionRequest`); `:184-185` `dotnet run --project <temp>`. Следствия: КАЖДЫЙ prompt→answer = полный restore+build+run (cold-start латентность секунды-десятки секунд); требует .NET **SDK** (не только runtime) + дерево исходников `43.SmartSkillsDomain` у пользователя; нет тёплого/persistent сервис-процесса — «скомпилируй-и-запусти по требованию», непригодно для продакшена. Отдельно от P08 gap-1 (absolute-path shim) и P05/P06 (synthetic no-send). **Closure:** заменить per-request `dotnet run` предсобранным долгоживущим сервис-процессом (stdio/HTTP) без компиляции на запрос; требовать только runtime; замерить cold-vs-warm латентность.
- **G41-2 — Ядро `43.SmartSkillsDomain` и `44.DesignConversionDomain` НЕ являются first-class проектами в `LlmFromScratch.sln`.** `LlmFromScratch.sln:134` содержит только `43.SmartSkillsDomain.Dashboard`; сами ядро-проекты как `Project()` **отсутствуют** (grep exit 1), хотя git-tracked на диске. Честная граница (не переоценивать в минус): оба ВСЁ РАВНО собираются **транзитивно** через `Tests/LlmFromScratch.Tests.csproj:60/:62`, `25.ModelServer.csproj:10`, `Dashboard.csproj:5` — это НЕ «никогда не собирается». Дыра: асимметрия (потребитель `…Dashboard` зарегистрирован, а основной продукт — нет), нарушены навигация/выборочная CI-сборка/build-graph целостность. **Closure:** добавить оба как `Project()`-записи (GUID/build-config); тест паритета «каждый git-tracked продуктовый .csproj присутствует в sln»; связать с коммитом дельты (P40).

### 7.2 — P42 скрытые оверклеймы (2 новые, обе в status-файлах ПАКЕТА; session.md сам-честен)

Общий вывод: `Extension development session.md` по completion-токенам (`goal_reached=true`×38, `TASK_FULLY_COMPLETE`×4, `PROVEN`×179, `ARCHITECT_VERDICT_ALLOW_FINAL`×5) в инспектируемых местах **сам-честен** — это либо historical/superseded state-блоки (само-коррекция), либо шаблоны governance-плана, либо `provenance` (не `proven`). Реальные capability-оверклеймы транскрипта УЖЕ в P12/P20/P34/P38. Новые дыры — только в двух status-файлах:

- **G42-1 — 50-pass ledger: 10 самозаверенных pass-записей против собственного «0/50».** `50-PASS-SUBAGENT-REVIEW-LEDGER.md`: заголовок `Accepted review pass records 0/50` (`:11`), `Package reviewed No` (`:10`), слоты 01-10 `PENDING`; НО `:80-268` реально написаны 10 секций `Pass 01/50`…`Pass 10/50`, каждая штампует `count_claimed: 1 fresh review pass record`, «требование выполнено», `independent_evidence_count: 1` = ЕЁ ЖЕ секция. Собственное `Review rule` файла (`:76`): «a ledger row by itself is not a cycle, not a review, and not product readiness». Все status-файлы дублируют «0/50» (`README:13`, `CURRENT-REPORT:20`, `STATUS-BOARD:24`). Тип дефекта: внутри-файловое противоречие 0↔10 (**stale-status-carrier / dual-live-canon**) + само-референтное «independent evidence» + предмет всех 10 пассов = читаемость самого CURRENT-REPORT/README (**process-about-process**) + placeholder-identity `UNKNOWN_IN_SUBAGENT`×10, нулевые timestamps, идентичный boilerplate. **Корректирует P07 gap-4** («ABANDONED_MID_CHUNK / 0 packets» — на деле 10 секций написаны).
- **G42-2 — `REQUIREMENTS-COVERAGE-MATRIX.md` подменяет ПРОДУКТОВЫЕ требования УПАКОВОЧНЫМИ.** Все 10 строк (`:7-18`) = требования упаковочной команды («Make a report», «Copy plans», «Do not claim product readiness»…). 8 помечены `COVERED_FOR_PACKAGE/STATUS`; summary «8 covered / 0 product-ready claims». **НИ ОДНОЙ строки Full Extension Definition** (host entrypoint, prompt-to-task, orchestration, skill-selection, adaptive learning, export/import, checkpoint/locator, token savings, semantic quality, dual-path). Оверклейм-подстановкой на уровне артефакта: читатель считывает «8 covered» как почти полное покрытие требований, тогда как продуктовые требования вообще отсутствуют как строки. Нарушает CLAUDE.md «Completion coverage matrix pattern».

### 7.3 — P43 пропущенные сценарии/измерения требований (4 новые — дыры ИНВЕНТАРЯ, не переоценка кода)

Реестр P01-P42 глубоко разобрал **3 хоста × single-project**, но вне поля зрения остались целые измерения требований, названные планами/CLAUDE.md:

- **G43-1 — Мультипроектность / cross-project transfer (второй target-project `xiigen-platform-dot-net-angular`).** Требование: `SMART-SKILLS-SEMANTIC-QUALITY-UPLIFT-PLAN-v1.md:664` (Q6-03), `:668-676` (Q7), `:831` (QC-07). Реально: `...UPLIFT-STATE.json` Q7-01 `:300` PASS, но `:302` `artifact_kind=report_only_common_assets_contract_evaluation_not_runtime_model_inference`, `:328` `model_decision_path_invoked:false`; оценено всего 4 строки/2-на-проект; «adaptive improves» = контракт-скор `2→4`, не runtime-метрика. Board `product-readiness-scenario-board.md:23` = 28 рядов, **все single-project (0 для второго проекта)**. Статус: PARTIAL (report-only contract eval, не runtime, вне board). **Closure:** cross-project ряды в dual-path board (хост × второй проект); реальный runtime cross-project inference Path A+B на `xiigen-platform`; common-only vs common+adaptive доказать held-out-метриками; разделить personal-ногу по project-root-hash.
- **G43-2 — Plugin-orchestration хост-семья (VS Code / Visual Studio project-wiki plugin) + orchestrator scenario-классы.** `ORCHESTRATOR-PROMPT-PLUGIN-EXPECTED-RESULTS-v1.json:41-51` ждёт `xiigen.openWikiPanel`/`ShowWikiPanelCommand`/`CopyWikiContextCommand`/Copilot-chat-participant→`queryWiki`; `:164` `if_plugin_entrypoint_missing_block_plugin_orchestration_claim`. `ORCHESTRATOR-PROMPT-PLANNING-SUITE-v1.json:6` `scenario_classes`=`figma_to_full_system_orchestration`,`figma_into_existing_system`,`system_from_user_explanation_without_complete_design`,`multi_user_diet_application_with_daily_feedback`. Host-entrypoints отсутствуют ⇒ по собственному правилу suite plugin-orchestration claim заблокирован. Реестр/board трактуют «3 расширения = Claude/Cursor/Codex» и эту семью игнорируют. **Closure:** решение Luba по scope — если plugin в поставке, построить точки входа и прогнать suite через host против repository scopes, покрыть каждый `scenario_class`; иначе явно вынести из scope.
- **G43-3 — Advanced wiki question classes (`42.ProjectWikiDomain`): 14 подклассов, не 7 бакетов.** `CLAUDE.md:1375` «every required advanced wiki question class and subtype is present, not only the top-level buckets»; `:1388` «seven category buckets when the plan requires fourteen underlying classes». Реально: `ProjectWikiDomainModels.cs:95` `ProjectWikiQueryIntent`, `:459` `ProjectWikiRequestRouteKind`, DPO-генераторы (`ProjectWikiRequestRoutingDpoGenerator.cs` и др.). Код есть, но class-by-class coverage-матрицы нет. **Closure:** перечислить канонический список (проверить =14); матрица «класс → runtime-invoked ответ + evidence»; поймать «7 vs 14».
- **G43-4 — Замкнутый local learning-loop как named-сценарий (Q6-04).** `SMART-SKILLS-...PLAN-v1.md:665`: «AI output observed → gap detected → DPO row → adaptive continue-training → local held-out improves over pre-adaptive baseline» — ядро adaptive-value-prop. Реально: adaptive-принятых DPO-строк для skill-selection ≈0 (P24); feedback-ingestion не реализован (grep `feedback`→0); ни одного ряда loop в 28-board. **Closure:** добавить Q6-04 как отдельный dual-path сценарий per host; реализовать ingestion наблюдённого вывода→детект→DPO→continue-training; доказать held-out ПОСЛЕ > pre-adaptive baseline числом, Path A+B.

### 7.4 — P44 сквозная интеграция (3 новые дыры связок + КОРРЕКЦИЯ P02 gap-5)

Положительные факты (звенья РЕАЛЬНО собраны в коде, не переоценивать в минус): core-сервис реально зовёт все 3 машины и цепляет выходы вперёд (`XiigenExtensionApiService.cs:118`→`:130/:131`→`:143`); транспорт реально доходит до компилированного core (`ps1:165/:166/:184-185`); Cursor host-адаптер реально собран как расширение (`extension.ts:121` activate, `:124` registerCommand, `:162-177` chatParticipant условно, `:206-217` webview-панель, `:661` spawn powershell); user-adaptive нога МОЖЕТ достичь runtime (`ResolveRequestLoadState:279-312`). **Недоказанность — в качестве/семантике (машины-стабы) и дальних Path B, не в самой сборке.**

- **N3 (arch bypass) + КОРРЕКЦИЯ P02 gap-5 — Поставляемая E2E-цепь ОБХОДИТ `ILogicalOrchestrator`/`15.OrchestratorModel`.** `XiigenExtensionApiService.cs:117-154` — жёстко зашитая линейная цепочка `PromptToTaskMachine.Translate`→`OrchestrationReasoningMachine.Plan`→`SkillSelectionMachine.DecideFromTrainedState`; конструкторы `:66-78` принимают ТРИ машины напрямую, НЕ `ILogicalOrchestrator`; grep `ILogicalOrchestrator\|ILogicalFlow` → 0. **Коррекция P02 gap-5:** цепь ДОСТИЖИМА end-to-end (не «недостижима»), но собрана В ОБХОД обязательной абстракции оркестрации — без fan-out / A-to-B / arbiter / reprocessing. **Closure:** либо провести поставляемый путь через `ILogicalOrchestrator` (зарегистрировать 3 стадии как `ILogicalFlow` в `15`, доказать fan-out/A-to-B/arbiter), либо явно задокументировать fixed-linear и убрать orchestration-layer из def-of-done; обновить P02 gap-5.
- **N1 — Контракт ответа имеет ТРЕТЬЕГО владельца на слое ТРАНСПОРТА + нет versioned wire-contract.** `ps1:229-244` в catch-ветке PowerShell РУЧНО фабрикует объект формы `XiigenExtensionResponse` (`Success=$false`,`Status="core_endpoint_error"`, свой строковый `CheckSetResults`-schema); `:191-192` транспорт ПЕРЕ-парсит `CheckSetResults` строковыми ключами; сам контракт `XiigenExtensionResponse` (`:176-186`) БЕЗ `schema_version` (версия только у trace-объекта `:196`). Дрейф формы по хостам доказан: codex `HOST-08A-codex-request-shape-repair.json`, `HOST-REPAIR-CODEX-AVAILABLE-SKILLS-PARSER`. Три владельца контракта: C#-core, PowerShell-транспорт, host-JS/TS (P37). **Closure:** один versioned wire-contract (`schema_version` на request И response), единый источник формы; запретить транспорту/host синтезировать контракт; cross-layer conformance round-trip тест для всех 3 хостов; убрать строковый re-parse.
- **N2 — Round-trip не несёт in-payload корреляции.** `ps1:207` читает `request.CorrelationId` только для trace; контракт ответа `XiigenExtensionResponse` (`:176-186`) НЕ содержит `CorrelationId`/prompt-hash — корреляция живёт в out-of-band trace-JSON, не в теле ответа, показываемом хосту. Невозможно машинно доказать «этот видимый ответ = ответ на этот prompt-hash» из контракта (корень того, что подрывало Path B P06/P32/P33 на уровне EVIDENCE; здесь корень — сам КОНТРАКТ). Любой будущий тёплый/async-транспорт (обязателен по P41) останется без response-level привязки. **Closure:** добавить в `XiigenExtensionResponse` обязательные `CorrelationId` (эхо) + `UserPromptSha256`; хост валидирует равенство ДО показа; Path A/B join строить на этих полях контракта.

### 7.5 — P45 провенанс/легал обучающих данных (5 новых G1-G5) — самый слабый слой, cross-ref RAG-05/07

Положительные факты: license-source гейт СУЩЕСТВУЕТ в 2 формах — (а) исполненный LEARN-02 `source-license-redaction-boundary-review.v1.json` (классифицировал 12 кандидатов; `common_source_rule`=«MIT/Apache-2.0 verification или Luba approval + redaction + source-disjoint split»; вердикт `PASS_BOUNDARY_REVIEW_COMMON_TRAINING_BLOCKED`); (б) код `ResultGapDpoWriters.cs:7-26` (`ResultGapSourceDescriptor(LicenseExpression,PrivacyScope,UserApprovalRecorded)` / `ResultGapSourceRoute(CommonTrainingAllowed,...,Quarantined)`) + исполненный `provenance-privacy-source-route.json`. Redaction/privacy-самофлаги проставлены во всех 3 корпусах. Домен DesignConversion имеет полноценную схему источника `license`+`provenance`+`privacy`. **Но ни один из этих фактов не покрывает фактически промоутнутые common-корпуса расширений.**

Центральные факты: в коде обучающего пути расширений (`43.SmartSkillsDomain`/`11.DPO`) grep `canonical_license\|terms_snapshot\|teacher_allowlist\|distillation` → **0** (вся C4/C5/`family->canonical_license`/Moonshot-Kimi-vs-DeepSeek-vs-Z.ai/terms_snapshot машинерия CLAUDE.md для extension-пути НЕ реализована).

- **G1 — Промоутнутые common-корпуса 3 расширений не имеют лицензионно-исходного манифеста.** `smart-skills-dpo-seed-dataset-card.json:23` `common_or_adaptive:"common"`, `:51-107` `source_refs`=только внутренние `artifacts/dpo-quality/*` (нет license/terms/source_uri); `skill-selection-v1/training-manifest.json:88-119` без license/terms/source_owner, `:134` `common_leg_row_count:88`; `orchestration-reasoning-common-corpus-manifest.json:31-47` без license. Контраст `design-conversion/real-s3-v1/source-manifest.schema.json:195-224` требует блок `license{...allows_training...}`. **Closure:** обязательный source-license+provenance манифест на каждый common-корпус; портировать схему DesignConversion; блокировать промоушен без license-гейта.
- **G2 — Distillation-teacher-гейт отсутствует; sonnet(vendor-model)-derived строки в COMMON-корпусе нарушают собственное правило карантина.** `orchestration-reasoning-common-corpus-manifest.json:33/38/43` `source_ref:"sonnet-local-analyzes:*"`,`model_leg:"common"` (3 строки в common) — вывод модели **Sonnet (Claude) = дистилляция**; vs `provenance-privacy-source-route.json:33-56` `external_vendor_output`+`unknown`→`trainingRoute:"quarantine"`,`commonTrainingAllowed:false`. Вывод модели-вендора использован как COMMON-обучение против собственного правила проекта. **Closure:** реализовать teacher-allowlist `family->canonical_license`+`terms_snapshot`+provider-disambiguation; переклассифицировать sonnet-строки в quarantine ЛИБО записать terms/approval. (Cross-ref RAG-05/07 legal.)
- **G3 — Существующий license-гейт покрывает только 12 session-derived кандидатов, не применён к промоутнутым/обученным корпусам.** LEARN-02 `:6` `row_count:12`, `:8` `common_training_allowed:false` (0 допущено в common); grep имён promoted-corpus по boundary-review → No matches; seed/orchestration/skill-selection корпуса — статичный committed JSONL, грузимый жёстким путём МИМО `ResultGapSourceRoute`. Гейт декоративен относительно поставляемых данных. **Closure:** прогнать boundary review + source-route по seed-v1/orchestration/skill-selection; блокировать промоушен без PASS.
- **G4 — Локально-проектные файлы (.clinerules/.gitignore/.gitattributes analyses) отправлены в COMMON-ногу без записанного «redacted AND approved for common».** `orchestration-reasoning-common-corpus-manifest.json:17-21`/`:33-46` `model_leg:"common"`; только `.cursorrules` отложен в adaptive (`:22-30`). CLAUDE.md: локальные проектные файлы тренируют adaptive-ногу, если явно не redacted+approved для common. Артефакта common-approval нет ⇒ риск приватного/локального материала в общей модели. **Closure:** per-row redaction+common-approval или перенос в adaptive; leg-assignment гейт по умолчанию в adaptive.
- **G5 — Единственный домен с license/provenance-аппаратом (DesignConversion) сам КРАСНЫЙ и не разделяется с расширениями.** `codex-source-manifest.authority-review.json` `allows_training:false` (`:95/:161/:217/:278`), `license_or_authority_ref:"not_proven"` (`:176/:237/:293/:329`), есть `codex-source-owner-license-blocker.json`; `source-manifest.schema.json:8-11` `placeholder_not_real:true`/`evidence_status:"schema_only_not_real_evidence"`. Эталонный аппарат (а) сам не прошёл лицензию, (б) domain-local (не подключён к 3 расширениям). **Closure:** закрыть DesignConversion source-license гейт или пометить non-training; вынести схему в общий контракт; тест паритета «каждый промоутнутый корпус имеет проходной source-license манифест».

### 7.6 — P46 UX / человекочитаемость host-пути (7 новых G46-1..7)

Честная картина: у Claude и Codex пользовательской UI-поверхности **нет вообще** (P09/P17), поэтому «человеческий экран» для них неприменим по отсутствию. У Cursor UI ЕСТЬ (команда-тост + webview-панель) — именно там пользователю показывают машинные токены/жаргон и невнятные ошибки. Единственная реальная UX-автоматизация проекта — внутренний WPF-дашборд (P29), к живым host-поверхностям НЕ привязан.

- **G46-1 — Ошибки Cursor = INFO-тост, текст = двойной жаргон + абсолютный dev-путь, без remediation.** `extension.ts:143` `showInformationMessage` (не `showErrorMessage`; ни одного `showErrorMessage/showWarningMessage` в файле); `:723/:741-742` `CONTRACT_ONLY_DOWNGRADE: <абс.путь> is missing`; `:780` `CORE_API_FAILED`. Пользователь видит: «XIIGen Cursor error: CONTRACT_ONLY_DOWNGRADE: C:\Projects\llm_mvp_core\tools\xiigen-extension-core-api.ps1 is missing; …». **Closure:** `showErrorMessage`; `error.code → человеческая фраза + next step`; кнопка «Open Settings».
- **G46-2 — По всем 3 хостам машинные токены/жаргон утекают пользователю; нет plain-language слоя.** Cursor `:1246-1247` возвращает `response.Status` дословно (`core_endpoint_missing`, `core_boundary_partial_no_semantic_quality_claim`, `prompt_to_task_not_ready_degraded`); Claude `:393` «Core xiigen boundary executed. Debug trace suppressed because redaction flags were unsafe»; Codex `:430` `status`=сырой токен ядра. **Closure:** общий core-слой перевода `Status/errorCode → plain-language (RU/EN)`; тест «видимый текст без snake_case-токенов».
- **G46-3 — Webview-панель Cursor как «результат» показывает внутренние пути к артефактам; статус-жаргон; нет `role=alert`; `lang` захардкожен; нет help/примеров.** `:1140-1145` `<dl>` показывает `Response/Trace/Core trace/Entrypoint` (внутренние file-пути); `:1056-1067` `statusText`=«…adapter functionality»; `:1137-1138` `<p class="status">` в `aria-live="polite"` БЕЗ `role="alert"`; `:1108` `<html lang="en">`; `:1132-1135` только textarea+Submit без примеров. **Closure:** человеческий результат, пути — свернуть в «Details»; `role=alert`+цвет ошибки; help/примеры; локализация.
- **G46-4 — Честное «модель не обучена/degraded» не доносится человеческим языком.** Токены `prompt_to_task_not_ready_degraded`/`loaded_state_no_matching_learned_pattern`/`core_boundary_partial_no_semantic_quality_claim` прокидываются в видимый текст без перевода. Легитимное `PASS_AS_HONEST_UNTRAINED_SCAFFOLD` (CLAUDE.md) не презентовано человеку. **Closure:** маппинг untrained/degraded-кодов в явную фразу («локальная модель ещё не обучена; показан базовый результат»)+тест presence.
- **G46-5 — UX-automation не привязана к живой host-поверхности; единственный «human-readable error» ассерт — поверхностный префикс-чек и никогда не исполнялся.** grep host-репо → единственный хит `cursor.../test/phase10/phase10CursorFullExtensionProof.js:91` `assert(displayText.startsWith("XIIGen Cursor error:"))` — проверяет ТОЛЬКО префикс (обёрнутый текст всё равно жаргон+абс.путь, G46-1); `phase10` ни разу не исполнялся (P16 gap-4) ⇒ ассерт мёртв. **Closure:** UX-automation-набор против реального Cursor (accessible names, `role=alert`, help/examples, отсутствие жаргона, локализация); содержательные (не префиксные) ассерты в исполняемом гейте.
- **G46-6 — Тексты настроек = внутренний жаргон, часть указывает на мёртвые «Future» настройки; у Claude/Codex settings-поверхности нет.** `package.json` `contributes.configuration`: `:52` coreRoot, `:57` coreApiScript, `:72` debug, `:77` targetLocatorPath (жаргон «check-set contract», «QSG target-locator», «xiigen exe payload»); `:62` promptSuite/`:67` installSmokeReport = мёртвые «Future» (P13 gap-3). **Closure:** переписать на человеческий язык с примером; убрать/пометить «Future»; для Claude/Codex ввести настоящую settings-поверхность.
- **G46-7 — То, что проект называет «user-visible status dashboard», — это Luba/agent-facing карта фаз, не end-user статус.** `supporting-files/.../phase-14-user-visible-status-dashboard.md` — «собрать для Luba понятную карту», «Фаза 15 из 17…». Deliverable-conflation. **Closure:** развести (1) Luba-facing execution status и (2) продуктовый in-host end-user статус (что сделала модель, готова/не готова, что показано), человекочитаемый; покрыть UX-автоматизацией.

### 7.7 — P47 perf/token/quality: СТАНДАРТ ДОКАЗАТЕЛЬСТВА улучшения (7 находок)

Угол, которого нет ни в P15/P27/P28 (те — про МЕХАНИЗМ токенов/качества): здесь отсутствует САМ стандарт «улучшает vs НЕ улучшает». **Ни на одном хосте нет измеренного доказательства, что расширение что-либо УЛУЧШАЕТ.**

1. **Задержки (latency added-hop) не меряет НИКТО.** `XiigenExtensionApiService.cs` grep `Stopwatch\|Elapsed\|latency\|duration\|cpu\|ram` → 0 тайминга; единственная реально исполненная измерительная `Program.cs` (frdp-p01-cursor-token-savings-baseline) grep stopwatch → 0; план трактует latency как опциональное (`...PLAN-v1.md:1066` «latency if available»). Экономия токенов не отвечает, не стало ли пользователю МЕДЛЕННЕЕ.
2. **CpuMs/PeakRamMb — подменные прокси.** `SkillSelectionMachine.cs:333` `CpuMs=sw.ElapsedMilliseconds` ТОЖДЕСТВЕН `:335` `ElapsedMs` (wall-clock, не CPU-время); `:334` `PeakRamMb=GC.GetTotalMemory(false)/(1024*1024)` = мгновенный снимок кучи, не пик/не working-set; поля принадлежат `LearningBatchResult` (обучение), не per-request инференсу; через боундари не проходят. CLAUDE.md запрещает `cpu_ms/ram_mb=UNKNOWN` — здесь хуже: значения ЕСТЬ, но семантически неверны.
3. **Нет числового ЦЕЛЕВОГО порога улучшения.** `TokenTraceAccounting.cs`/`ExtensionQualityScoring.cs` grep `threshold\|target.reduction\|success.criteri` → 0. Нет продуктового критерия («≥X% редукции токенов при дельте качества ≥ −Y%»). Без цели «улучшает» неопровержима и недоказуема. (Отличие от P27 gap-6: там дефект кода-скорера; здесь отсутствует сам продуктовый порог.)
4. **Статистическая строгость не исполнена.** План требует repeat counts + CI (`:1278`,`:1316`); исполнено `...STATE.md:315` `repeat_count:null`, `savings_calculation:NOT_CALCULATED`; grep `repeat\|variance\|stddev` в `TokenTraceAccounting.cs` → 0; единственный quality-артефакт — 96-строчный fixture, один прогон. Single-shot без воспроизводимости/разброса.
5. **Нет контрфактного контроля «без XIIGen».** Все baseline'ы XIIGen-ВНУТРЕННИЕ (baseline↔adaptive обе ноги зовут ядро); `XiigenExtensionApiService` — единственный путь, no-extension арма нет. Единственный контроль, отвечающий на продуктовый вопрос об улучшении, отсутствует.
6. **Нет персистентного baseline не-регрессии между версиями.** grep `regression.baseline\|golden.baseline\|non.regression` → 0. Нет эталона поймать регресс при выкате нового checkpoint.
7. **Нет денежной экономии ($) от редукции токенов.** Единственные $-значения — бюджет-гард хостового CLI на ОДНУ команду (`total_cost_usd 0.04564375`, `--max-budget-usd 0.05`), не baseline-vs-adaptive экономия.

### 7.8 — P48 ПРИОРИТЕТ И ПОСЛЕДОВАТЕЛЬНОСТЬ: критический путь M0→M6 (рекомендованный порядок стройки) + 4 новые дыры секвенса

Реестр P01-P47 (~230 дыр) не несёт ни поля приоритета, ни `blocks/blocked_by`, ни критпути. Единственный план `product-readiness-action-plan.md` упорядочен ПО СТРОКАМ scenario board, а не по зависимостям, и ставит доказательную работу (Фаза 2/3) ПЕРЕД перестройкой инференса, от которого она зависит (счётчики: 28 строк, full joined pass **0**, product-ready **0**, scoped accepted 14).

**Ярусы:** **P0** фундамент (блокирует всё: сохранить продукт + сделать возможность реальной); **P1** продуктовый хребет (host+транспорт+wiring); **P2** доказательство/измерение/QA/dual-path; **P3** честность/гигиена/governance (параллелизуемо, F0-правило).

**Критический путь (рекомендованный порядок build):**
```
M0  СОХРАНИТЬ ПРОДУКТ + BUILD-GRAPH:
    commit extension-дельты ядра (P40-1) + добавить 43/44 в .sln (P41-2)
    + зелёный build (P35-6) + записанный full .trx baseline (P39-4)
        ▼
M1  ФИЗАБИЛИТИ-СПАЙК обучающего движка (de-risk P0-B):
    реальный токенизатор + модель адекватного размера на ОДНОЙ extension-задаче;
    требовать монотонное loss↓ + held-out ≫0.5
    (сейчас: единственный реальный градиентный DPO РЕГРЕССИРОВАЛ P25-1; toy hash-tokenizer/1-слой P05-3)
    → если from-scratch недостижимо при нужном качестве → ЭСКАЛАЦИЯ Luba ДО стройки
        ▼
M2  РЕАЛЬНЫЙ ОБУЧАЕМЫЙ ИНФЕРЕНС в 3 машинах:
    заменить keyword/lexical authority (P21-1/P22-1/P23-3) обучаемым;
    оживить dead semantic-quality gate (P10-1);
    DPO с реальным run + held-out + source-disjoint + ablation (P24/P25/P21-2/P23-1/P23-5);
    снять governance-downgrade R4 (P38-1/P38-2)
        ▼
M3  ПРОДУКТОВЫЙ ХРЕБЕТ:
    тёплый транспорт (P41-1/P08-1/P13-1) + versioned wire-contract единого владельца (P44-2/P37/P44-3)
    + реальные host-точки входа (Claude P09-1, Codex P17-1, Cursor productization)
    + конвейер через ILogicalOrchestrator/15 (P10-6/P44-1) + skill-каталог/feedback/stage-3
    + lifecycle/locator/promotion/integrity wiring (P26-1..8/P05/P30-1)
        ▼
M4  ИЗМЕРЕНИЕ ПОЗАДИ РЕАЛЬНОГО ИНФЕРЕНСА:
    token-savings (P27/P15/P47-1/2/6/7) + семантическое качество (P28/held-out)
    + lifecycle round-trip (P30/P11/P19) + перф/цель/статистика/контроль/не-регрессия (P47-3/4/5)
    + провенанс/легал (P45)
        ▼
M5  DUAL-PATH SAME-SCENARIO ПОКРЫТИЕ (28 × 3 хоста):
    Path A через собранную команду расширения, НЕ in-process fallback (P32-2/P33-2/P34-2);
    Path B видимый desktop с prompt-hash-привязкой (P36/P32/P33/P34);
    correctness-oracle + e2e (P39/P12/P16/P20); A↔B comparison-поле (P31-5/P44-3)
        ▼
M6  ФИНАЛ: ≥10 независимых execution-review (P02-8) + completion coverage matrix
    по Full Extension Definition (P42-2) + git-интеграция/мержабельность (P40)
```
**Узкое место = M1→M2 (реальный обучаемый инференс): от него зависит ~60% дыр P2.** Всё, что построено ПЕРЕД M2, вернётся `not-proven` — что и подтверждает 0/28 joined после 14 «scoped accepted».

**Новые дыры секвенса:**
- **G48-1 — Нет capability-dependency / critical-path-ordered closure-roadmap.** Реестр = инвентарь без порядка; единственный план coverage-ordered и инвертирует порядок. **Closure:** принять P0/P1/P2/P3 + DAG (§выше) как мастер-порядок; каждой дыре — tier + blocks/blocked-by.
- **G48-2 — Инверсия «доказательство раньше возможности» закреплена планом (структурная трата).** `product-readiness-action-plan.md` §4 ставит token/semantic/learnability/DPO/lifecycle proof-packets downstream от стаб-машин. Нет правила, гейтящего capability-зависимую proof/scenario-работу за M2. **Closure:** заморозить всю capability-зависимую proof/scenario-работу до прохождения M2; до M2 разрешены только host-route + не-capability scaffolding.
- **G48-3 — Не инвентаризован набор Luba-only продуктовых решений; он ветвит критпуть.** Разбросаны: P05-2 (трансформерный DPO vs словарь), P26-6/8+P05 (remote/marketplace vs local-only lifecycle), P44-1/P10-6 (маршрут через оркестратор vs fixed-linear), P43-1 (мультипроектность xiigen-platform в scope?), P43-2 (VS Code/Visual Studio wiki-plugin как 4-я host-семья?), P40-6 (репо-топология monorepo/submodule vs 3 отдельных), P45-2 (teacher/distillation: sonnet-common с terms vs карантин). **Closure:** собрать decision-gate ledger (вопрос/опции/радиус влияния/заблокированные ветки); ответы Luba; ТОЛЬКО потом финализировать секвенс.
- **G48-4 — Фундаментальный узел критпути (реальный обучаемый инференс) непроверен по физабилити; нет de-risk-спайка ПЕРЕД стройкой.** M1→M2 опирается на from-scratch движок (Rule 10, no external ML), который НИ РАЗУ не показал успешной кривой обучения: единственный реальный градиентный DPO РЕГРЕССИРОВАЛ (P25-1: loss↑ к ln2, reward-margin ↓~3×, held-out ≈0.56 на toy hash-tokenizer/1-слой P05-3); held-out ≈ шум (P25-3). **Closure:** bounded de-risk-спайк ПЕРВЫМ как входной gate всего capability-трека; при недостижимости from-scratch на нужном качестве — эскалация Luba (scope/model-решение, связка с G48-3).

**Рекомендованный сжатый порядок исполнения:** (1) решить G48-3 (decision-gate ledger → ответы Luba); (2) M0 сохранность/build (параллельно P3-гигиена); (3) M1 de-risk-спайк — go/no-go; (4) M2 реальный инференс (снять инверсию G48-2); (5) M3→M4→M5→M6; (6) весь путь — с человекочитаемым phase/step-отчётом и per-milestone entry/exit-критериями.

### 7.9 — P49 риски + внешние зависимости + решения-Luba (6 новых G49-1..6)

Три недостающих управленческих слоя. Положительные факты: Cursor engine-пин есть (`package.json:9-11` `engines.vscode ^1.90.0`); у Codex реальная marketplace-плагин-система вендора существует (канал в принципе есть); транспорт использует `-NoProfile`+`windowsHide:true`; redaction-самофлаги проставлены (но RCE не покрывают).

- **G49-1 — Формального risk-реестра нет ни в пакете, ни в репо.** grep `risk register\|probability\|mitigation\|risk owner` → 0; только `product-readiness-action-plan.md:213-224` §12 = список пробелов без вероятности/воздействия/митигации/владельца/триггера; P48 добавил ярусы, но приоритет ≠ риск. **Closure:** risk register (id/риск/вероятность/воздействие/экспозиция/митигация/контингенс/владелец/триггер/статус); засеять R-01..R-05 (см. ниже).
- **G49-2 — Внешняя зависимость от host-API вендоров: нет compat-матрицы/мониторинга дрейфа; Codex-поверхность может быть недоступна извне.** Cursor пин есть, но compat-теста/матрицы нет; Claude `plugin.json:4` `0.0.0-phase40-skeleton` (пина host-API нет); Codex тоже skeleton. Острейший случай P17: `HOST-08F-codex-plugin-app-qa.json` `codex_plugin_command_scope:"marketplace_management_only_from_help"`, `direct_local_plugin_app_invoke_path_found:false`; `CX-04R-host-runtime-path-report.json` `true_codex_host_runtime_path_found:false` — **вендор может НЕ давать пути инвокации локально-авторского плагина вовсе** ⇒ один из 3 обязательных хостов может оказаться неосуществим без OpenAI-курации/одобрения. **Closure:** пин+тест мин. host-API версии per host; compat-матрица; подписка на changelog/deprecation; контингенс; эскалация Luba внешнего ограничения Codex.
- **G49-3 — Security: RCE-поверхность в host-адаптерах, нет threat-model/подписи/workspace-trust gating.** Cursor `extension.ts:661-675` `spawn("powershell.exe",["-NoProfile","-ExecutionPolicy","Bypass","-File",coreApiScript,...,"-CoreRoot",coreRoot])`, где `coreApiScript/coreRoot` — из настроек ВОРКСПЕЙСА; `ps1:114` `ProjectReference` из `$resolvedCoreRoot`, `:181` генерирует `Program.cs`, `:184` `dotnet run` — КОМПИЛИРУЕТ и ЗАПУСКАЕТ C# из переданного `-CoreRoot`; Claude `plugin.json:15` на SessionStart запускает `node.exe "<абс.путь>"`. Нет подписи/интегрити/валидации пути/threat-model. Классический вектор: вредоносный воркспейс (свой `.vscode/settings.json`) → выполнение кода без gating доверия. **Closure:** threat-model; gate через Workspace Trust; вайтлист пути, запрет произвольного `-CoreRoot`; убрать `-ExecutionPolicy Bypass`; подпись/интегрити runner+entrypoint; убрать runtime-компиляцию-из-пользовательского-пути (предсобранный подписанный бинарь — связка с warm-transport P41); security-review как блокирующий гейт completion.
- **G49-4 — Нет устанавливаемого/публикуемого артефакта расширения и канала дистрибуции (внешнее действие).** Cursor `package.json:5` `version "0.0.0-xiigen-wiring"`, `:6` `private:true`, `:7` `license:"UNLICENSED"`, `:8` publisher xiigen; нет `repository/icon`/packaging (grep `vsce\|ovsx` → 0); во всех 3 host-репо нет `LICENSE/CHANGELOG/.vscodeignore/icon`; Claude/Codex манифесты skeleton с зашитыми абс. dev-путями. `private:true`+`UNLICENSED` блокируют маркетплейс. **Closure:** выбрать лицензию + канал (решение Luba, G49-6); добавить `repository/icon/CHANGELOG/LICENSE/.vscodeignore` + packaging-скрипт (`vsce package`/`ovsx` Cursor; аналог Claude/Codex); убрать абс.пути; аккаунт издателя; ревью маркетплейса; доказать установку на чистой машине (Path B); внешние шаги — в risk register.
- **G49-5 — Внешне-правовая зависимость: ToS вендора-учителя для distillation не прочитан/не записан.** Промоутнутый common-корпус построен из вывода Sonnet (P45 gap-2); CLAUDE.md требует `terms_snapshot`, но grep `terms of service\|ToS\|anthropic.*terms\|openai.*terms` → 0. Многие вендорские ToS прямо запрещают обучение конкурирующих моделей на выводе — это может ОБНУЛИТЬ весь distillation-value-prop, а не «не пройти внутренний гейт». Отдельно от P45 gap-2 (там внутренний teacher-gate) — здесь ВНЕШНЯЯ правовая зависимость + go/no-go. **Closure:** прочитать+записать фактический `terms_snapshot` каждого вендора; юридическое ревью; решение Luba go/no-go; если запрещено — исключить vendor-derived из common и пересобрать из permissive (MIT/Apache-2.0); risk register R-05.
- **G49-6 — Новые решения только-Luba, отсутствующие в списке P48 gap-3 (добавляет 5).** (а) лицензия + канал дистрибуции (G49-4); (б) security/workspace-trust модель + кто вправе задавать пути ядра (G49-3); (в) приём внешне-правового риска ToS/distillation (G49-5); (г) минимально-жизнеспособный набор хостов — выпускать подмножество (напр. **только Cursor как единственную реальную UI-поверхность**, P46) vs требовать все 3 при внешнем блокере Codex (P17); (д) кто платит/владеет хостингом при remote-lifecycle (P26 gap-8). **Closure:** добавить 5 строк в decision-gate ledger; ответы Luba; финализировать секвенс только после.

**Топ-риски для risk register (P49):** R-01 неспособность from-scratch движка (P48-4); R-02 host-платформа вендора меняет/закрывает поверхность (G49-2, острейший — Codex); R-03 RCE/провал security-review (G49-3); R-04 маркетплейс отклоняет публикацию (G49-4); R-05 ToS вендора запрещает distillation (G49-5).

### 7.10 — Сводка решений, которые нужны от Luba (консолидировано P48 gap-3 + P49 G49-6)

12 продуктово-правово-безопасностных решений, каждое переформирует scope/критпуть; финальную последовательность нельзя зафиксировать, пока открыты:
1. Движок инференса: трансформерный DPO from-scratch vs словарь/детерминизм (P05-2). Связано с G48-4 (физабилити-спайк ДО стройки).
2. Lifecycle: remote/marketplace (25.ModelServer) vs local-only (P26-6/8+P05).
3. Маршрут конвейера: через `ILogicalOrchestrator`/15 vs fixed-linear (P44-1/P10-6).
4. Мультипроектность: `xiigen-platform-dot-net-angular` в scope? (P43-1).
5. 4-я host-семья: VS Code/Visual Studio project-wiki plugin в поставке? (P43-2).
6. Репо-топология: monorepo/submodule vs 3 отдельных репо (P40-6).
7. Teacher/distillation: sonnet-derived common допустим с записанными terms vs карантин (P45-2).
8. Лицензия + канал дистрибуции расширения (сейчас `UNLICENSED`/`private:true`) (G49-4).
9. Security/workspace-trust модель + кто вправе задавать пути ядра (G49-3).
10. Приём внешне-правового риска ToS/distillation, go/no-go (G49-5).
11. Минимально-жизнеспособный набор хостов: подмножество (только Cursor) vs все 3 при внешнем блокере Codex (G49-6/P17).
12. Кто платит/владеет хостингом при remote-lifecycle (G49-6/P26-8).

### Что остаётся на проходы 8-10

- **Пройдено к концу прохода 7:** opus-pass 01-49. Adversarial-recheck (6/6 дыр выжили, P02 gap-4 закрыт), hidden-overclaims (2), missing-scenarios (4), integration/wire-contract e2e (3 + коррекция P02 gap-5), provenance-legal (5), UX (7), perf/token/quality proof-standard (7), приоритет/секвенс M0-M6 (4 новые), риски/внешние/решения-Luba (6). — ЗАКРЫТЫ.
- **Проход 8-9 (opus-pass-50 + `opus-PROJECT-CLOSURE-PLAN.md` 548 КБ):** консолидированная roadmap M0-M6 + накопительный реестр ~230+ дыр (source-of-truth мастер); финальная сводка/дедуп/приоритеты.
- **Проход 10:** `copied-plans/` (30 файлов) + `PLANS-AND-SUPPORTING-FILES-INVENTORY.md` (108 supporting) + `copy-manifest.json` + 2 транскрипта (`Extension development session.md` 4 КБ, `extension development session 2.md` 92 КБ) — source-addressability и evidence-инвентарь для плана continue.

### Границы честности прохода 7

- **claim_scope:** полное чтение 9 файлов `opus-pass-41..49` + извлечение по-категориям: adversarial-recheck (выжившие/скорректированные), hidden-overclaims, missing-scenarios, integration/wire-contract gaps (CorrelationId/UserPromptSha256/versioned wire/ILogicalOrchestrator bypass), provenance-legal (teacher/distillation/sonnet-common/DesignConversion RED), UX gaps, perf/token/quality proof-design, приоритет M0-M6, риски+решения-Luba — с file:line. Русский, read-only, записан только RAG-файл.
- **evidence_scope:** прочитаны ПОЛНОСТЬЮ 9 файлов `opus-pass-41..49`; выводы привязаны к их file:line/цитатам. Живой код `C:\Projects\llm_mvp_core`, host-репо, board, артефакты и git в этом проходе НЕ запускались/не читались напрямую (цитируются по Opus-аудиту — родитель верифицирует).
- **НЕ доказано этим проходом:** любые product-readiness утверждения; точность каждого per-pass file:line-вывода Opus (нужна верификация родителя против живого кода/артефактов); что P02 gap-4 действительно закрыт (Opus утверждает REFUTATION VALID — требует независимой проверки родителем).
- **subagent_id:** UNKNOWN_IN_SUBAGENT. **requested_model:** gpt-5.5-equivalent (Claude Opus 4.8, Ultracode). **requested_reasoning_effort:** xhigh.


## Pass 8/10 — opus-PROJECT-CLOSURE-PLAN ч.1: структура + executive verdict + M0/M1/M2 детальный backbone

Источники этого прохода (read-only): `opus-pass-50-roadmap.md` (31.6 КБ, консолидированная карта M0-M6) и `opus-PROJECT-CLOSURE-PLAN.md` (548 КБ, ~760 строк — мастер-реестр). Прочитаны: executive frame (стр.3-5), полностью структура реестра P01-P05 (образцы таблиц), полный список заголовков проходов P01-P50, вся секция `## CONSOLIDATED CLOSURE ROADMAP` (P50, стр.681-734) и **оба ADDENDUM A1/A2 — решения Luba 2026-07-09** (стр.736-758). 548 КБ середины (per-pass таблицы P06-P49) НЕ читаны целиком — это задача проходов 9-10; здесь взяты структура + verdict + критпуть-хребет M0/M1/M2.

### 8.1 — Структура мастер-плана закрытия (как организованы ~245 дыр)

- **Файл:** `opus-PROJECT-CLOSURE-PLAN.md`, накопительный реестр РЕАЛЬНОГО состояния проекта «три расширения XIIGen (Claude/Cursor/Codex поверх `llm_mvp_core`)».
- **Механизм роста:** по проходам P01…P50; каждый проход дописывает ТОЛЬКО НОВЫЕ дыры над маркером `<!-- APPEND-NEW-GAPS-ABOVE-THIS-LINE -->` (стр.760). Оценка ведётся от РЕАЛЬНОГО кода/артефактов в настоящих репо, а НЕ от самоотчёта пакета `extension development` (который «в основном COPY/STATUS работа», стр.3).
- **Три уровня зрелости (честный фрейм, стр.3), жёстко различаются:** `COPIED_DOC_ONLY` (только скопированный план/док/статус) != `BUILT_NOT_PROVEN` (код есть в репо, не доказан тестами/runtime/метриками) != `PROVEN` (доказано held-out метриками + dual-path same-scenario: Path A автоматический boundary + Path B локальный desktop). Правило: каждый real-status обязан иметь artifact/file:line ИЛИ явное «not found (searched X)».
- **Определение «FULL implementation» (стр.3):** Full Extension Definition (host entrypoint + settings/debug + core-API path + prompt-to-task + orchestration + skills + adaptive learning + export/import/backup/continue-training + QA) ПЛЮС dual-path same-scenario coverage. Ни один проход НЕ объявляет проект завершённым.
- **Формат каждого прохода:** заголовок `## Pass Pnn — <тема> (new gaps: N)`, абзац «Что проверено в этом проходе» (bounded-грепы/чтения с file:line), затем либо таблица `| Component | Real-status | Evidence file:line | Gap | Closure steps |` (P01-P47), либо нарративные G-блоки (P48 → дыры `G48-1..4`, P49 → `G49-1..6`).
- **Верхняя структура файла:** (1) честный фрейм + baseline claimed-статус пакета (стр.3-5: Total phases 15; Done 2 = только package/report/copy; Remaining/not closed = 13); (2) `## Running gap register`; (3) 49 содержательных проходов P01-P49; (4) `## CONSOLIDATED CLOSURE ROADMAP (Pass P50)` — синтез в один dependency-ordered план M0-M6, 0 новых дыр; (5) `## ADDENDUM A1` и `## ADDENDUM A2` (решения Luba 2026-07-09); (6) маркер аппенда.

**Тематическая карта проходов (P01-P49) — навигация по реестру:**
P01 claimed-state/package-honesty (SEED, 14) · P02 definition-of-done + real-code anchoring (11) · P03 real host/extension code (6) · P04 real API surfaces (8) · P05 DPO/training/lifecycle code (6) · P06 evidence-packet reality dual-path (7) · P07 что сессия РЕАЛЬНО делала build/test/runtime/git vs copy (5) · P08 базовая матрица 15 фаз заявлено-vs-реально (4) · **P09-P12 Claude** (entry/settings/core-API; pipeline; adaptive/export/import/continue; QA+dual-path) · **P13-P16 Cursor** (то же) · **P17-P20 Codex** (то же) · **P21-P23 Pipeline** (prompt-to-task; task-to-plan orchestration; task/plan-to-skills) · **P24-P25 DPO** (accepted rows/data; исполненный training-run loss/margin+fresh-load+continue) · **P26 checkpoint/locator lifecycle** (promotion+locator+rollback) · P27 экономия токенов · P28 семантическое качество · P29 dashboard/docs+route-QA · P30 реальная lifecycle-операция · **P31-P36 dual-path** (scenario inventory 28; Claude/Cursor/Codex по-сценарно; Path A харнесс; Path B десктоп) · P37 core-ownership+тонкость адаптеров R6 · P38 trainable-архитектура аудит vs код · P39 Tests/QA · P40 git/branch/mergeability · P41 adversarial-recheck+blind-spot (2) · P42 скрытые оверклеймы (2) · P43 пропущенные сценарии/требования scope (4) · P44 сквозная E2E-цепь (3) · P45 data/provenance/legal (5) · P46 UX/человекочитаемость (7) · P47 perf/token/quality proof-of-improvement (7) · P48 priority/sequencing (4, G48-1..4) · P49 риски+решения-Luba+external (6, G49-1..6).

**Счёт дыр (для верификации родителем):** сумма деклараций «new gaps: N» в заголовках P01-P49 ≈ **293**; собственная консолидация плана (P50, стр.683 и 686) заявляет «**~245 дыр**». Расхождение объяснимо adversarial-дедупом (P41 — 6/6 выжили, но были correction-строки как P05 «`.xiigen`-writer СУЩЕСТВУЕТ, P02 устарел» и P02 «reality-map устарел») — часть «новых» дыр являются коррекциями/уточнениями ранее заявленных, а не строго-новыми. Обе цифры фиксирую честно; точный уникальный счёт — задача дедупа проходов 9-10.

### 8.2 — Executive verdict (единый честный baseline проекта, стр.686 + roadmap §1)

Дословный смысл одним абзацем (source-of-truth для «где стоит проект расширений»):
- **Продуктовый DONE = 0/15 фаз. PROVEN dual-path = 0/28 сценариев** (собственный счёт `product-readiness-scenario-board.json`: `full_joined_pass:0`; 14 «accepted» — все narrow/scoped, `formal_joined_pass=false`; P31).
- **Host-репо реально существуют** (коррекция ранних «host MISSING»): `C:\Projects\{claude,cursor,codex}-xiigen-smart-skills`. **Cursor** — самый зрелый (реальный `activate()`, 3 команды, chat participant, webview-панель, собранный `dist/extension.js`); **Claude** = скелет из одного SessionStart-hook; **Codex** = рукописный `.js`, НЕ зарегистрирован в реальном marketplace Codex.
- **Транспорт:** ядро имеет in-process boundary `XiigenExtensionApiService`, но единственный реальный транспорт — **per-call `dotnet run`-компиляция** всего `43.SmartSkillsDomain` через PowerShell-шим с абсолютными путями (непоставляемо, требует .NET SDK; P41-1/P08-1).
- **«Обучаемые» машины — фикция обучения:** три машины конвейера (prompt-to-task/orchestration/skill-selection) — **детерминированные keyword/lexical-lookup** (нарушение R4); `semantic_quality_claimed` зашит в `false`, агрегирующий гейт семантики мёртв по построению (P10-1/P21/P22/P23).
- **DPO отсоединён и регрессировал:** настоящий градиентный DPO (`11.DPO`) существует, но **отсоединён** от расширений; единственный исполненный градиентный run **РЕГРЕССИРОВАЛ** (loss вырос к ln2≈0.693, reward-margin сжался ~3×, held-out≈0.56; P25-1/P05-3).
- **STUB/BUILT_NOT_PROVEN:** экономия токенов, семантическое качество, learnability, lifecycle, promotion, locator (P24-P30).
- **Dual-path = 0/28 joined:** Path A обходит расширение (in-process fallback), Path B = re-photo/stale/чужая поверхность, **0 видео** (P31-P36).
- **Гигиена/git:** extension-дельта ядра **НЕ закоммичена** (331 файл/+18 574/−3 576, 545 untracked), ядро не first-class member `.sln`, нет зелёного `.trx` baseline (P39/P40/P41).
- **Legal/security:** нет license/provenance-гейта на промоутнутых common-корпусах; **sonnet-derived строки в common** (distillation без teacher-гейта); **RCE-вектор** в host-адаптерах; **нет устанавливаемого/публикуемого артефакта** (P45/P49).
- **ИТОГ:** пакет `extension development` доказывает только copy/status; реальный код — честный untrained scaffold с host-скелетами; до FULL implementation — дистанция во ВСЕХ измерениях.
- **Ключевое узкое место (P48):** критпуть `M1→M2` (реальный обучаемый инференс) — от него зависит **~60% дыр слоя доказательство/сценарии**; всё, построенное ПЕРЕД M2, возвращается `not-proven` (что и даёт 0/28 joined при 14 «scoped accepted»). Поэтому порядок — capability-first (dependency-ordered), а не coverage-ordered.

### 8.3 — Хребет M0-M6 (ярусы + критпуть)

Легенда ярусов: **P0** фундамент/блокирует всё · **P1** продуктовый хребет (зависит P0) · **P2** доказательство/измерение/QA/dual-path (зависит P0+P1) · **P3** честность/гигиена/governance (параллелизуемо).
Критпуть: `M0(commit+build+trx) → M1(de-risk движка) → M2(реальный инференс 3 машин) → M3(host+транспорт+wiring) → M4(измерение) → M5(dual-path 28×3) → M6(приёмка+публикация)`.
Правило порядка (P48-2, hard): **заморозить всю capability-зависимую proof/scenario-работу (token/quality/learnability/lifecycle/dual-path) до прохождения M2**; до M2 разрешены только host-route scaffolding + не-capability гигиена/доки/тесты.

### 8.4 — M0 — Фундамент: сохранить и собрать (P0, блокирует всё) [git/build/tests]

Дыры/работы (gap-ID → работа):
- **P40-1** — закоммитить extension-дельту ядра фазовыми коммитами (Rule 6); сейчас **331 файл незакоммичен** (+18 574/−3 576, 545 untracked).
- **P41-2** — добавить `43.SmartSkillsDomain` И `44.DesignConversionDomain` как `Project()` в `LlmFromScratch.sln` (ядро не first-class member `.sln`).
- **P35-6** — прогнать зелёный `dotnet build` под текущее (тяжело-модифицированное) дерево.
- **P39-4** — записать полный `.trx` baseline (total/passed/failed + commit-hash); сейчас есть только узкие filtered-прогоны, нет записанного зелёного полного baseline.
- **P40-5** — стабилизировать worktree/checkout ядра (**9 расходящихся**) до одного канонического.
- **Gate M0 (acceptance):** зелёный `build` + сохранённый full `.trx` + ВСЕ extension-исходники под git + `git merge-tree` чисто в `codex/chat-gpt-5.5` И `main` **ПОСЛЕ** коммита реальной дельты (**P40-3**: текущий merge-tree считался по дереву БЕЗ 18,5 тыс. строк — бессодержателен).
- **Зависимости:** нет входящих (M0 — корень критпути, блокирует всё). ⚠️ **Модифицирован ADDENDUM A1** — см. 8.7.

### 8.5 — M1 — De-risk обучающего движка (P0, feasibility-спайк ПЕРЕД стройкой) [DPO/движок]

- **P48-4** (корневая) — bounded de-risk-спайк: реальный токенизатор (НЕ hash-токены P05-3) + модель адекватного размера на ОДНОЙ extension-задаче; требовать **монотонное loss↓ + held-out ≫ 0.5**. Чинит корень: **P25-1** (единственный градиентный run регрессировал), **P05-3** (toy: хеш-токенайзер, 1 слой, 64-dim), **P25-3** (held-out ≈ шум).
- **Gate M1 (acceptance):** **≥1 успешная кривая обучения** на extension-задаче (loss↓, held-out значимо > 0.5). Если from-scratch (Rule 10, no external ML) НЕ достигает качества — **эскалация Luba** (scope/model-решение; decision-ledger).
- **Зависимости:** зависит от M0 (нужно собираемое дерево). ⚠️ **Блокирует весь M2:** без прохождения M1 весь capability-трек строится на неспособном движке — **запрет строить downstream** (P48-2). Главный риск проекта **R-01** живёт здесь.
- ⚠️ **Существенно переопределён ADDENDUM A2** — M1 теперь = feasibility/имплементация внешнего DPO-build (`ILearningDynamicGraph`), НЕ отдельный toy-движок; см. 8.7.

### 8.6 — M2 — Реальный обучаемый инференс в 3 машинах (P0, УЗКОЕ МЕСТО, зависит M1) [pipeline+DPO]

Область: три машины (prompt-to-task / orchestration / skill-selection) + DPO. Дыры по машинам:

**prompt-to-task:**
- **P21-1** — заменить hardcoded alias-словарь на выученный/эмбеддинговый экстрактор.
- **P21-5** — калибровать пороги (сейчас 0.01/0.01 — гейт всегда открыт).
- **P21-4** — реальный training-update/continue (сейчас append готового паттерна).
- **P21-6** — подключить two-leg (personal/common).

**orchestration (task→plan):**
- **P22-1** — согласовать словарь стадий 1↔2 (12/15 паттернов недостижимы).
- **P22-2** — реальная DPO reward-scalarization, использовать rejected-сторону.
- **P22-4** — обучаемый planner вместо replay/константного baseline.
- **P22-6** — continue-training update вместо append.

**skill-selection (task/plan→skills):**
- **P23-3** — эмбеддинг вместо lexical bag-of-words argmax.
- **P23-4** — использовать rejected-сторону (DPO-маржа).
- **P23-5** — ablation-harness.
- **P23-6** — лексический leakage-guard (Jaccard).
- **P23-1** — построить настоящий source-disjoint runtime eval (сейчас self-recorded `NO_TRUE_SOURCE_DISJOINT_...`).
- **P23-2** — снять circular held-out=1.

**Сквозное (все 3 машины + DPO):**
- **P10-1** — оживить мёртвый semantic-quality гейт (эмитить позитивную evidence).
- **P24-1/P24-3/P24-4** — материализовать настоящие DPO-пары (сейчас `chosen_ref/rejected_ref`; orchestration 0/12 пар).
- **P25-2/P25-4/P25-5** — исполнить training с loss/reward-margin/DELTA (сейчас 88 report'ов — все UNKNOWN delta).
- **P38-4** — портировать эталон trainable-integrity из `42.ProjectWikiDomain` (ablation/leakage/exact-prompt gate там уже реализованы).
- **P38-1/P38-2** — снять governance-downgrade R4 «детерминизм ок если claimed=false».
- **P38-3** — §4 `knn_role`.
- **P38-5** — независимый (НЕ parent-self) audit введённых SS-юнитов.
- **Gate M2 (acceptance):** на КАЖДОЙ из 3 машин — измеренный **held-out semantic uplift на ЗАГРУЖЕННОМ checkpoint** + **ablation** (обнуление весов → деградация) + **source-disjoint eval** + **независимое sub-agent-ревью**; `claimed=false` снимается ТОЛЬКО после реального прироста; promotion гейтится по loss/reward-margin, НЕ по факту прогона.
- **Зависимости:** зависит от M1 (способный движок). Блокирует M3/M4/M5 продуктово-содержательно (~60% доказательных дыр). ⚠️ **Переопределён ADDENDUM A2** — M2 потребляет DPO-build API из xiigen-platform/flows; см. 8.7.

### 8.7 — ⚠️ ADDENDUM A1 + A2 (решения Luba 2026-07-09) — критически меняют M0/M1/M2

Эти два аддендума приписаны в КОНЦЕ мастер-плана (стр.736-758, над маркером) и ОТМЕНЯЮТ/переопределяют формулировки M0-M2 выше. Continue-план ОБЯЗАН исполнять M0-M2 в редакции аддендумов, а не исходной.

**ADDENDUM A1 — ветка реализации и локальный worktree (меняет M0):**
- **Ветка реализации: `orchestrator-extensions`** (Luba написала «orchistrator-extensions»; план использует исправленное — требует подтверждения орфографии до создания веток). Вся реализация — ТОЛЬКО на этой ветке в КАЖДОМ участвующем репо, НЕ на `main`/latest.
- **Ветка = КОПИЯ самой свежей ветки каждого репо** (последняя по дате коммита).
- **Worktree по шаблону `C:\Projects\<repo>-orchestrator-extensions`**, одна команда: `git worktree add "C:\Projects\<repo>-orchestrator-extensions" -b orchestrator-extensions <latest-branch>`.
- **Участвующие репо (свой worktree у каждого):** `llm_mvp_core` (сюда коммитится M0-дельта 331 файл, НЕ в main) · `claude-xiigen-smart-skills` · `cursor-xiigen-smart-skills` · `codex-xiigen-smart-skills` · **`xiigen-platform-dot-net-angular`** и **`xiigen-flows-dot-net-angular`** (В SCOPE — см. A2).
- **Мержабельность (OP-67, hard-гейт):** `orchestrator-extensions` обязана В ЛЮБОЙ момент оставаться чисто-мержабельной обратно в свою latest-ветку И в `main` (additive, координация общих файлов, без add/add на генерируемых evidence; перед каждым коммитом no-write `git merge-tree` в обе базы, 0 конфликтов).
- **Изменённый первый шаг M0:** создать `orchestrator-extensions` + worktree в КАЖДОМ участвующем репо ДО любого коммита; **Gate M0 дополнен:** «worktree `<repo>-orchestrator-extensions` создан из latest во всех участвующих репо; дельта закоммичена в `orchestrator-extensions`; `merge-tree` чист в latest И main».

**ADDENDUM A2 — обучение ЧЕРЕЗ DPO-build; platform/flows в scope (переопределяет M1/M2):**
- **Открытый scope-вопрос закрыт:** `xiigen-platform-dot-net-angular` и `xiigen-flows-dot-net-angular` **ВХОДЯТ в scope** как участвующие репо.
- **Расширения обучаются ЧЕРЕЗ новый DPO-build проект** = `ILearningDynamicGraph` DPO plan из `C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build` (статус `PLAN_READY_FOR_LUBA_REVIEW (design complete)`; мастер `claude-ILEARNINGDYNAMICGRAPH-PLAN-v1.md`, readiness-заголовки `active-codex-execution-17/40`). Расширения — **КЛИЕНТЫ** его «better-model-output → learning-DPO» API.
- **DPO-build реализуется ИМЕННО в `xiigen-platform` + `xiigen-flows`** — там открывается learning-DPO API, который вызывают клиенты (расширения).
- **Следствие для критпути M1→M2:** НЕ строить отдельный/игрушечный тренинг-движок в extension-репо. **M1** (de-risk движка) = feasibility/имплементация `ILearningDynamicGraph` DPO-build; **M2** (реальный обучаемый инференс 3 машин) = ПОТРЕБЛЕНИЕ его через API. Дыры DPO/training (P24-P30) и риск R-01 (регрессировавший toy-DPO P25-1/P05-3) закрываются ЭТИМ проектом, а не заново.
- **Жёсткая зависимость:** DPO-build — ОТДЕЛЬНЫЙ проект со своим планом/гейтами/своим «реализуй»; extension closure plan теперь имеет **hard-зависимость** от него (M2 нельзя закрыть, пока DPO-build не реализован в platform/flows и его API не открыт). DPO-build = критическая внешняя-для-этого-плана зависимость на критпути M1→M2. Синхронизировать «реализуй» обоих проектов.

### 8.8 — Разбивка M0-M2: needs реализуй / Luba decision / external

- **Needs РЕАЛИЗУЙ (в нашем контроле):** весь M0 (commit/sln/build/trx/worktree); M1/M2 инженерия — но через консюмирование DPO-build (A2), а не собственный движок.
- **Needs LUBA DECISION (частично закрыты аддендумами; остаток ветвит M1→M2):** (1) трансформерный DPO vs словарь для инференса (P05-1/2) — направляется в DPO-build; (13) эскалация feasibility from-scratch если M1-спайк провалится (P48-4). Орфография ветки `orchestrator-extensions` — открытый micro-decision (A1).
- **EXTERNAL для M0-M2:** зависимость от .NET SDK у текущего транспорта (устранимо реинженирингом, P41-1); готовность/статус DPO-build `ILearningDynamicGraph` (design complete, но НЕ реализован — блокирует M2).

### Что остаётся на проход 9 (и 10)

- **Проход 9:** вторая половина `opus-PROJECT-CLOSURE-PLAN.md` — **детальный backbone M3 (host+транспорт+wiring+lifecycle+R6) · M4 (измерение token/quality/perf/legal) · M5 (dual-path 28×3) · M6 (приёмка/git/security/distribution)** + кросс-секущее (UX/hygiene/governance) + P43 пропущенные измерения; плюс **хвост реестра** — per-pass таблицы P06-P49, которые в проходах 1-7 суммированы, но не процитированы по file:line из мастер-файла (дедуп ~293-декларации vs ~245-консолидация).
- **Проход 10:** `copied-plans/` (30 файлов) + `PLANS-AND-SUPPORTING-FILES-INVENTORY.md` (108 supporting) + `copy-manifest.json` + 2 транскрипта — source-addressability и evidence-инвентарь.

### Границы честности прохода 8

- **claim_scope:** извлечена СТРУКТУРА мастер-плана закрытия (уровни зрелости, формат проходов, тематическая карта P01-P49, счёт дыр), единый executive verdict (baseline), и ДЕТАЛЬНЫЙ backbone M0/M1/M2 (gap-ID + acceptance-гейты + зависимости), плюс **оба ADDENDUM A1/A2 (решения Luba 2026-07-09), которые переопределяют M0/M1/M2**. Русский, read-only, записан только RAG-файл.
- **evidence_scope:** прочитаны полностью `opus-pass-50-roadmap.md`; в `opus-PROJECT-CLOSURE-PLAN.md` — стр.3-95 (frame+P01-P05 образцы структуры), полный индекс заголовков P01-P50 + их «new gaps», и стр.681-760 (P50 CONSOLIDATED ROADMAP + ADDENDUM A1/A2 + маркер). Выводы привязаны к file:line внутри мастер-файла. Живой код `C:\Projects\*` в этом проходе НЕ запускался (gap-факты цитируются по Opus-реестру — родитель верифицирует против кода).
- **НЕ доказано этим проходом:** любые product-readiness утверждения; точность каждого per-pass file:line-факта Opus; точный уникальный счёт дыр (293-декларация vs 245-консолидация — требует дедупа проходов 9-10); фактический статус реализации `ILearningDynamicGraph` DPO-build (заявлен «design complete», НЕ реализован).
- **subagent_id:** UNKNOWN_IN_SUBAGENT. **requested_model:** gpt-5.5-equivalent (Claude Opus 4.8, Ultracode). **requested_reasoning_effort:** xhigh.


## Pass 9/10 — opus-PROJECT-CLOSURE-PLAN ч.2: M3/M4/M5/M6 detailed backbone + cross-cutting + registry tail + gap-count reconciliation

Источник этого прохода: `C:\Users\lubao\Documents\xiigen\llm mvp\extension development\opus-PROJECT-CLOSURE-PLAN.md` — CONSOLIDATED CLOSURE ROADMAP (стр.681-735) для вех M3-M6/кросс-секущего/разбивки/критпути + реестровый хвост P40-P49 (стр.556-680) для file:line-детализации дыр, кормящих эти вехи. Проход 8 закрыл M0/M1/M2 + оба ADDENDUM A1/A2; этот проход достраивает вторую половину backbone (M3→M6) и сверяет счёт дыр. Всё read-only; записан только RAG-файл. Ярусы: **P0** фундамент (M0/M1/M2) · **P1** хребет (M3) · **P2** доказательство (M4/M5) · **P3** гигиена/приёмка (M6+кросс-секущее).

### Веха M3 — Продуктовый хребет: host+транспорт+wiring+lifecycle+R6 (ярус P1, зависит от M2) — стр.702-704

Смысл вехи: превратить «честный untrained scaffold с host-скелетами» в поставляемый продукт — реальные точки входа хостов, тёплый транспорт, версионированный контракт, маршрут через оркестрацию, и полный lifecycle/locator. Это первый ярус, который делает расширения продуктом, но он **capability-заблокирован**: до прохождения M2 (реальный обучаемый инференс) любой хребет отдаёт `not-proven`.

- **Транспорт (сейчас единственный реальный путь = per-call компиляция):** заменить per-request `dotnet run`-компиляцию тёплым предсобранным сервис-процессом без компиляции на запрос (**P41-1** = G-транспорт `tools/xiigen-extension-core-api.ps1:111-131` temp-csproj+ProjectReference, `:133-181` temp Program.cs, `:184-185` `dotnet run`; **P08-1** absolute-path shim; **P13-1** кросс-платформенность). Требовать только .NET runtime, не SDK; убрать генерацию temp-csproj; замерить cold-vs-warm латентность.
- **Versioned wire-contract (сейчас 3 расходящихся владельца формы):** ввести один `schema_version` на request И response + `CorrelationId`-эхо + `UserPromptSha256` в теле ответа, один владелец (**P44-2** транспорт `ps1:229-244` фабрикует `XiigenExtensionResponse` на ошибке своим строковым `CheckSetResults`, контракт `XiigenExtensionApiService.cs:176-186` без `schema_version`; **P44-3** round-trip без in-payload корреляции — ответ не несёт CorrelationId/prompt-hash; **P37-1..6** host-JS синтез контракта). Cross-layer conformance-тест round-trip host→PS→core→PS→host для 3 хостов.
- **Claude:** реальная user-точка входа (**P09-1**), settings-поверхность (**P09-3**), вывод в рабочую область (**P09-4**), production-локатор (**P09-5**), одна копия (**P09-6**), fix ref-mismatch (**P11-6**), синхронизировать имя (**P09-2**). Сейчас Claude = SessionStart-hook скелет (`.claude-plugin/plugin.json:15` `node.exe "<абс.путь>"`).
- **Cursor:** absolute-path дефолты убрать (**P13-2** `package.json:56` default `C:\Projects\...ps1`), settings-схема (**P13-3**), убрать test-команды (**P13-4**), стабильный chat API (**P13-5**), воспроизводимая сборка (**P13-7**), одна копия (**P13-8**), убрать canned auto-run (**P14-4**). Cursor — единственная зрелая поверхность (`activate()`/команды/chat participant/webview/собранный `dist`).
- **Codex:** регистрация в реальной плагин-системе вендора (**P17-1** — внешний блокер: `direct_local_plugin_app_invoke_path_found:false`), fix `runCli` видимый ответ (**P17-2**), один манифест (**P17-3**), settings (**P17-4**), один бинарь/версия (**P17-7/P20-7**), param-контракт (**P17-6**). Сейчас Codex = рукописный `.js`, не в marketplace.
- **Wiring:** провести поставляемый E2E через `ILogicalOrchestrator`/`15.OrchestratorModel` ЛИБО явно задокументировать fixed-linear (**P10-6/P44-1** — сейчас `XiigenExtensionApiService.cs:117-154` жёсткая линейная цепь Translate→Plan→DecideSkills в обход оркестратора; **P22-3**); handler для `feedback` (**P11-2/P14-1** — grep `feedback`→0); каталог скиллов из индекса (**P10-2/P14-3/P18-2**); стадия-3 берёт task+plan (**P14-5**); все 3 метода из каждого хоста (**P10-3/P14-2/P18-3**); валидация `Method` (**P18-1**).
- **Lifecycle/locator/promotion:** lifecycle-операции в контракт (**P11-1/P19-1/P30-1**); load через `IRemoteUnitLocator` (**P26-1** — grep `IModelRepository|IRemoteUnitLocator|UploadAsync|LocateAsync|LoadWeightsAsync|index.json` в `43.SmartSkillsDomain`→0); version-indirection (**P26-2**); программный `index.json` (**P26-3**); integrity-гейт (**P26-4**); promotion-гейт 3 машин (**P26-5**); реальный uploader/честный local-only (**P26-6/P05-4**); исполнители Rollback/Quarantine/Continue (**P26-7**); реальный payload (**P30-2**); per-user adaptive-root (**P11-4/P17-8**); online write-back (**P11-3**); persistent локатор (**P05-5/6**).
- **R6 (тонкость адаптеров):** общий тонкий SDK; редакция/honesty/презентация/locator во владении core (**P37-1..6**).
- **Gate M3 (acceptance):** реальный prompt→answer ЧЕРЕЗ команду каждого хоста (НЕ fallback), тёплый транспорт, conformance-тест контракта пройден, 3 машины грузятся без failure.

### Веха M4 — Измерение позади реального инференса: token/semantic/perf/lifecycle-proof/legal (ярус P2, зависит от M2+M3) — стр.706-708

Смысл вехи: доказать, что расширение что-либо УЛУЧШАЕТ — числами над контролем, а не наличием артефакта. Проход P47 показал: **стандарт доказательства улучшения отсутствует на всех хостах**.

- **token:** эмиссия трейса кросс-хостово (**P27-1**), арифметика редукции+acceptance (**P27-2**), fix `prompt_hash`-инвариант (**P27-3**), считать downstream-токены (**P27-4**), join token+quality (**P27-5**), позитивный гейт редукции (**P27-6**), реальный токенизатор (**P15-1**), независимая переменная (**P15-2/P15-3**).
- **semantic:** реальный per-pair scorer (**P28-1/2**), одобренные eval-входы (**P28-3**), прогон ЗАГРУЖЕННОГО checkpoint source-disjoint (**P28-4**), capture 4 слоёв (**P28-5**), inter-rater (**P28-6**).
- **perf (все НОВЫЕ в P47):** latency (**P47-1** — `XiigenExtensionApiService` grep `Stopwatch|Elapsed|latency`→0; план `EXTENSION-RUNTIME-...-PLAN-v1.md:1066` «latency if available» = опционально), реальные CPU/RAM (**P47-2** — `SkillSelectionMachine.cs:333` `CpuMs==ElapsedMs` обе wall-clock, `:334` `PeakRamMb=GC.GetTotalMemory` мгновенный снимок; «хуже чем UNKNOWN»), числовой порог улучшения (**P47-3** — grep `threshold|target.*reduction`→0, продуктового критерия успеха нет), статистика/повторы (**P47-4** — `STATE.md:315` `repeat_count:null`, single-shot), **no-XIIGen контрольная рука** (**P47-5** — все baseline'ы XIIGen-внутренние, внешнего контроля нет), non-regression baseline (**P47-6** — grep `regression.baseline|golden.baseline`→0), $-экономия (**P47-7** — только host CLI бюджет одной команды `total_cost_usd 0.04564375`, не эффект расширения).
- **lifecycle-proof:** continue-training uplift (**P30-3/P25-6/P05-8**), реальные share/anonymize (**P30-5/6**), `SourceHashesVerified` (**P30-4**), personal-нога 4 сфер (**P30-7**), upgrade/rollback над боевым checkpoint (**P30-8/P26-8**).
- **legal (все НОВЫЕ в P45):** license-манифест на каждый common-корпус (**P45-1** — seed/orchestration/skill-selection промоутнуты как `common` без license/terms/source_uri; эталон только `design-conversion/real-s3-v1/source-manifest.schema.json:195-224`), teacher/distillation-гейт (**P45-2** — **LEGAL VIOLATION**: `orchestration-reasoning-common-corpus-manifest.json:33/38/43` `source_ref:"sonnet-local-analyzes:*"`, `model_leg:"common"` = дистилляция вывода Sonnet в COMMON против собственного правила `external_vendor_output→quarantine`), применить гейт к промо-данным (**P45-3** — LEARN-02 boundary-review покрыл лишь 12 session-строк, 0 в common; гейт декоративен), leg-assignment (**P45-4** — локальные `.clinerules/.gitignore` в common-ногу без redaction+approval), закрыть DesignConversion license (**P45-5** — эталонный домен сам КРАСНЫЙ: `allows_training:false`, `license_or_authority_ref:"not_proven"`).
- **Gate M4 (acceptance):** измеренное улучшение над **no-XIIGen контролем** с объявленным числовым порогом + статистической значимостью + non-regression; каждый common-корпус с проходным license-манифестом.

### Веха M5 — Dual-path same-scenario 28×3 (ярус P2, зависит от M2+M3+M4) — стр.710-712

Смысл вехи: honest baseline = **0/28 joined** (собственный `product-readiness-scenario-board.json` `full_joined_pass:0`; 14 «accepted» = narrow/scoped, P31). Веха доказывает каждый сценарий обеими ногами на ОДНОМ prompt-hash.

- **Path A (через команду расширения, не fallback/fabricated/direct-module):** **P32-2/P33-2/P34-2**; единая Path A харнесс + 16/28 недостающих команд (**P35-1/2**), end-to-end product-workflow (**P35-3**), mutation-boundary харнесс (**P35-4**), CI (**P35-5**).
- **Path B (видимый desktop):** prompt-hash+видео (**P36-1/2** — сейчас 0 видео, re-photo/stale), маркер на поверхности РАСШИРЕНИЯ, не чужой (**P36-3/P32-4/P33-5**), детерминированное наведение окна (**P36-4**), marker-smoke Claude/Codex (**P36-5**), свежий typed→submit→новый ответ (**P33-3/P34-3/P32-5**).
- **Oracle+e2e:** correctness-oracle (**P39-1/P12/P16/P20**), e2e-слой в .NET (**P39-5**), исполняющий token-04 (**P39-2**).
- **A↔B:** поле сравнения по prompt-hash, одна сессия (**P31-5/P44-3/P33-4**).
- **Board:** нормализованный вектор/дата/mojibake/matrix/count-carrier (**P31-2/3/6/7**), переклассифицировать blocker-as-pass/ceremony/label-inflation (**P34-5/6/8**), Codex-папки 24/25/26 (**P34-7**).
- **Gate M5 (acceptance):** ≥1 реальный full joined pass на хост (Path A команда + Path B видимый + A↔B по prompt-hash), рост до 28×3 accepted.

### Веха M6 — Приёмка/интеграция/публикация (ярус P3+external, зависит от M5) — стр.714-716

Смысл вехи: финальная приёмка + внешние действия (публикация, безопасность, дистрибуция).

- **Reviews/matrix:** ≥10 независимых sub-agent execution-review (**P02-8/P42-1** — сейчас 50-pass ledger само-противоречив: заголовок `0/50` при 10 самозаверенных pass-секциях, `50-PASS-SUBAGENT-REVIEW-LEDGER.md:11` vs `:80-268`); completion-матрица по Full Extension Definition (**P42-2** — сейчас `REQUIREMENTS-COVERAGE-MATRIX.md:7-18` покрывает только упаковочную задачу, 0 продуктовых строк, рапортует «8 covered»).
- **git-интеграция:** push/фазовые сообщения/32 base-коммита/FF в main/mergeability-строка (**P40-2/3/4**), версионирование core+host (**P40-6**).
- **security (все НОВЫЕ в P49):** threat-model/Workspace-Trust/убрать `-ExecutionPolicy Bypass`+компиляцию из user-пути/подпись (**P49-3** — **RCE-вектор**: `extension.ts:661-675` `spawn("powershell.exe",["-ExecutionPolicy","Bypass","-File",coreApiScript,...,"-CoreRoot",coreRoot])` где путь из настроек воркспейса → открытие вредоносного воркспейса = выполнение произвольного C#).
- **distribution (НОВОЕ в P49):** лицензия+канал/устанавливаемый артефакт/установка на чистой машине (**P49-4** — `package.json:6-7` `"private":true`/`"license":"UNLICENSED"` блокируют marketplace; нет `.vsix`/vsce/icon/CHANGELOG/LICENSE; абс. dev-пути непортируемы).
- **host-API compat:** compat-матрица+мониторинг deprecation (**P49-2**); risk register (**P49-1** — формального риск-слоя нет, grep `risk register|probability|mitigation`→0).
- **Gate M6 (acceptance):** устанавливаемый артефакт доказан на чистой машине + dual-path принят + все ревью PASS + git-интеграция/мержабельность зафиксированы.

### Кросс-секущее (ярус P3, параллелизуемо, НЕ на критпути) — стр.718

- **UX (P46, 7 дыр):** ошибки Cursor через `showInformationMessage` не тот severity-канал (**P46-1** `extension.ts:143`, ни одного `showErrorMessage`); видимый статус = машинные токены/жаргон на всех 3 хостах (**P46-2** `core_endpoint_missing`/`prompt_to_task_not_ready_degraded`); webview показывает внутренние пути как «результат» + нет `role=alert`/локализации (**P46-3** `:1140-1145`); untrained/degraded не донесён человекочитаемо (**P46-4**); UX-automation на живых host-поверхностях отсутствует, единственный ассерт мёртв+поверхностный (**P46-5** `phase10CursorFullExtensionProof.js:91` проверяет лишь префикс); settings help-текст на жаргоне (**P46-6**); «user-visible status dashboard» phase-14 = agent-facing, не end-user (**P46-7**).
- **evidence-hygiene:** секрет+pre-commit скан (**P20-4**).
- **governance:** 50-pass ledger/requirements-matrix (**P42-1/2**).
- **порядок как живой артефакт** с tier+blocked-by (**P48-1/2**).

### Пропущенные измерения (требуют scope-решения Luba, P43) — стр.720

Целые ИЗМЕРЕНИЯ требований, отсутствующие в реестре P01-P42 (не переоценка кода, а поиск дыр покрытия):
- **P43-1** мультипроектность / cross-project transfer (второй target `xiigen-platform-dot-net-angular`, Q6-03/Q7/QC-07) — доказано лишь report-only contract-eval на 4 fixture, `model_decision_path_invoked:false`, 0 рядов в 28-board.
- **P43-2** VS Code/Visual Studio wiki-plugin как **4-я host-семья** + orchestrator prompt-suite (`ORCHESTRATOR-PROMPT-PLUGIN-EXPECTED-RESULTS-v1.json`, scenario_classes `figma_to_full_system_orchestration` и др.) — suite есть как expected-results JSON, host-точек входа нет, вся семья вне реестра/board.
- **P43-3** advanced wiki question classes 42-домена: **14 подклассов, не 7 бакетов** (`CLAUDE.md:1375/:1388`; `ProjectWikiDomainModels.cs:95/:459`) — class-by-class матрицы нет.
- **P43-4** замкнутый Q6-04 learning-loop (наблюдение вывода AI → gap → DPO → continue-training → held-out > pre-adaptive baseline) — назван планом, нет ряда в board, нет runtime-цикла.

### Реестровый хвост P40-P49 — детальные дыры с file:line (то, что проходы 1-7 суммировали, но не цитировали)

- **P40** Git/branch/mergeability (6): extension-дельта НЕ закоммичена (331 файл/+18574/−3576, 545 untracked, 876 porcelain), нет фазовых сообщений/32 base-коммитов/FF в main, worktrees не сведены (9→1).
- **P41** Adversarial recheck (2 НОВЫЕ + 6/6 подтверждений): рекчек 6 несущих дыр против живого дерева (ветка `DesignToCode-clean`) — P10 dead-gate CONFIRMED, P26/P05 model-server bypass CONFIRMED, P11/P14 feedback unhandled CONFIRMED, P40 uncommitted CONFIRMED, P02/P21 anchor CONFIRMED, P20 secret-leak CONFIRMED-но-единичный; 1 коррекция (P23 leakage-guard закрыт кодом `SkillSelectionMachine.cs:113-142`, `CandidateOrderLeakageGuardUnit` подключён `:146`). **2 слепые зоны:** транспорт как per-call compile (`ps1:111-185`); `43.SmartSkillsDomain`+`44.DesignConversionDomain` НЕ first-class в `LlmFromScratch.sln` (`:134` только `.Dashboard`, оба собираются лишь транзитивно).
- **P42** Скрытые оверклеймы (2): 50-pass ledger 10 самозаверенных секций vs заголовок «0/50» (`:11` vs `:80-268`, само-референтное доказательство, деливербл само-ревьюит своё оформление); REQUIREMENTS-COVERAGE-MATRIX подменяет НАБОР требований упаковочными (`:7-18`, 0 продуктовых строк, «8 covered»). **Вывод по session.md (не новые дыры):** транскрипт сам-честен — `TASK_FULLY_COMPLETE` помечен historical, `goal_reached=true` = шаблон/«код не менялся».
- **P43** Пропущенные сценарии (4) — см. выше.
- **P44** Сквозная E2E-интеграция (3): shipped-путь обходит `ILogicalOrchestrator` (`:117-154`, коррекция P02 gap-5 «недостижим»→достижим, но в обход абстракции); транспорт = 3-й владелец контракта, нет versioned wire/conformance (`ps1:229-244`); нет in-payload корреляции ответ↔запрос (`:176-186` без CorrelationId/prompt-hash).
- **P45** Data/provenance/legal (5) — см. M4 legal выше (license-манифест/teacher-gate/применение/leg-assignment/DesignConversion красный).
- **P46** UX (7) — см. кросс-секущее выше.
- **P47** Perf/token/quality measured proof (7) — см. M4 perf выше (latency/CPU-RAM-интегрити/порог/статистика/no-XIIGen-контроль/non-regression/$).
- **P48** Priority/sequencing (4): dependency-ordered roadmap отсутствовал (существующий `product-readiness-action-plan.md` упорядочен по board-рядам, не по зависимостям); секвенс-инверсия «доказательство раньше возможности» закодирована планом (`:64-65` proof-packets перед перестройкой инференса); Luba-only decision-set не инвентаризован (7 решений разбросаны); фундаментальный узел критпути (реальный инференс) — feasibility непроверена, нет de-risk-спайка (единственный градиентный DPO РЕГРЕССИРОВАЛ P25-1).
- **P49** Риски/решения/external (6): формальный risk-реестр MISSING; host-API vendor-dependency без compat-матрицы/мониторинга (Codex внешне заблокирован); RCE security-вектор; нет устанавливаемого/публикуемого артефакта; ToS-зависимость distillation (может обнулить value-prop); 5 новых Luba-only решений вне P48-списка.

### Разбивка: needs реализуй / needs Luba decision (13) / external — стр.722-728

- **Needs РЕАЛИЗУЙ (~90% реестра, внутри контроля):** весь M0..M6 кроме отмеченного — commit/build/sln/trx (M0); обучаемый инференс+DPO (M1/M2, но ЧЕРЕЗ DPO-build platform/flows по A2); host/транспорт/wiring/lifecycle/R6 (M3); измерение token/quality/perf/legal-манифесты (M4); dual-path harness/oracle/e2e/board (M5); reviews/matrix/UX/hygiene (M6).
- **Needs LUBA DECISION (13; свести в один decision-gate ledger P48-3+P49-6):** (1) трансформерный DPO vs словарь (P05-1/2); (2) remote/marketplace vs local-only lifecycle (P26-6/8); (3) маршрут через `ILogicalOrchestrator`/15 vs fixed-linear (P44-1/P10-6); (4) мультипроектность `xiigen-platform` в scope (P43-1); (5) VS Code/VS wiki-plugin 4-я host-семья в scope (P43-2); (6) репо-топология monorepo/submodule vs 3 репо (P40-6); (7) teacher/distillation sonnet-common vs карантин (P45-2); (8) лицензия+канал дистрибуции (P49-4); (9) security/Workspace-Trust модель + кто задаёт core-пути (P49-3); (10) приём внешне-правового ToS-риска distillation (P49-5); (11) минимальный набор хостов (напр. только Cursor) vs все 3 при внешнем блокере Codex (P17/P46); (12) кто платит/владеет remote-хостингом (P26-8); (13) эскалация feasibility from-scratch если M1-спайк провалится (P48-4). _(Плюс открытый micro-decision A1: орфография ветки `orchestrator-extensions`.)_
- **EXTERNAL (вне контроля):** регистрация Codex-плагина в marketplace / vendor-supported local-invocation (реальный внешний блокер, 1 из 3 хостов под вопросом, P17-1/P49-2); стабильность host-API вендоров + deprecation-мониторинг (P49-2); аккаунты издателя + ревью маркетплейса (P49-4); ToS вендора-учителя для distillation (может обнулить value-prop, P49-5); зависимость от .NET SDK у текущего транспорта — устранимо реинженирингом (P41-1).

### Критпуть и главный риск — стр.730

`M0 → M1 → M2 → M3 → M4 → M5 → M6`. **Узкое место M1→M2:** ~60% дыр яруса P2 зависят от реального инференса; всё, построенное перед M2, возвращается `not-proven` (что подтверждает 0/28 joined при 14 «scoped accepted»). **Главный риск R-01:** единственный градиентный DPO РЕГРЕССИРОВАЛ (P25-1: loss↑ к ln2≈0.6931, reward-margin ↓~3×, held-out≈0.56), движок игрушечный (P05-3: toy hash-tokenizer/1-слой/64-dim), feasibility не доказана. **Правило порядка (P48-2):** заморозить всю capability-зависимую proof/scenario-работу до M2; до M2 — только host-route scaffolding + не-capability гигиена/доки/тесты. По A2 M1/M2 удовлетворяются реализацией/консюмированием `ILearningDynamicGraph` DPO-build в `xiigen-platform`/`xiigen-flows` — extension closure plan имеет **жёсткую зависимость** от того отдельного проекта.

### Сверка счёта дыр: 293 декларация vs ~245 консолидация (дедуп)

**Арифметика подтверждена точно** (grep+sum по мастер-файлу):
- Сумма всех per-pass деклараций `new gaps: N` за P01-P49 = **293 ровно** (49 проходов; 14+11+6+8+6+7+5+4+6+4+6+6+8+5+4+7+8+4+4+7+6+6+6+5+6+8+6+6+7+8+7+5+6+8+6+5+6+5+5+6+2+2+4+3+5+7+7+4+6 = 293). P50 добавляет 0 (это консолидация, а не новый инвентарь).
- P50 (стр.683) заявляет консолидированный реестр как **«~245 дыр»**; P48 (стр.654, срез через P47) заявлял **«~230 дыр»**.
- **Дельта 293 − 245 ≈ 48** = дедуп: элементы, задекларированные проходом как `new`, но которые консолидация трактует как коррекции/рекчеки/пересечения существующих gap-ID, а не как отдельные новые дыры. **Прямые доказательства дедупа в самом файле:** P41 честно фиксирует «6/6 перепроверенных дыр всё ещё реальны» + только 2 truly-new (рекчеки НЕ считаются новыми); P42 прямо пишет «Вывод по session.md (**не новые дыры**)» и заносит лишь 2 в status-файлы; десятки строк несут пометки `КОРРЕКТИРУЕТ P0x` / `Отдельно от P0x` / `Ново vs P0x`, указывающие на перекрытие-смежность, а не полную дизъюнктность (напр. P44 gap-1 корректирует вывод P02 gap-5; P42 корректирует P07 gap-4; P23 закрывает P02 gap-4).

**Граница честности сверки (важно, не оверклеймить):** «~245» и «~230» — это ПРИБЛИЗИТЕЛЬНЫЕ (`~`) консолидационные оценки самого Opus, а НЕ построчный дедуп-леджер. Мастер-файл **не содержит** таблицы «gap-ID → duplicate-of», которая перечислила бы конкретные ~48 дублей. Проход 9 подтверждает: (1) 293 — точная сумма деклараций; (2) ~245 — заявленная Opus консолидация; (3) точный уникальный счёт и построчное сопоставление 48 дублей **НЕ доказаны файлом** — это аппроксимация, а не верифицированный 1:1 дедуп. Полный дедуп остаётся семейным открытым пунктом (согласуется с caveat проходов 1-8).

### Границы честности прохода 9

- **claim_scope:** извлечён ДЕТАЛЬНЫЙ backbone второй половины плана — M3 (host/транспорт/wire-contract/wiring/lifecycle/R6), M4 (token/semantic/perf/lifecycle-proof/legal), M5 (dual-path 28×3 Path A/B/oracle/board), M6 (reviews/matrix/git/security/distribution/risk) с их gap-ID и acceptance-гейтами; кросс-секущее (UX/hygiene/governance); пропущенные измерения P43 (cross-project/plugin-host-family/wiki-classes/Q6-04); реестровый хвост P40-P49 с file:line; разбивка realize/Luba-13/external; критпуть+R-01; и **сверка счёта дыр (293 точно vs ~245 консолидация)**. Русский, read-only, записан только RAG-файл.
- **evidence_scope:** прочитаны полностью в `opus-PROJECT-CLOSURE-PLAN.md` стр.681-735 (CONSOLIDATED ROADMAP M0-M6+разбивка+критпуть) и стр.556-680 (реестровый хвост P40-P49 целиком, с таблицами дыр). Счёт дыр подтверждён `grep -oE "new gaps: [0-9]+" | awk sum` = 293 (49 проходов). Живой код `C:\Projects\*` в этом проходе НЕ запускался — gap-факты и file:line цитируются по Opus-реестру (родитель верифицирует против кода).
- **НЕ доказано этим проходом:** любые product-readiness утверждения; точность каждого per-pass file:line-факта Opus против текущего дерева; **точный уникальный дедуп 48 дублей** (файл несёт только приблизительное «~245», без построчной таблицы duplicate-of); фактический статус реализации `ILearningDynamicGraph` DPO-build (заявлен design-complete, НЕ реализован — жёстко блокирует M2 по A2).
- **subagent_id:** UNKNOWN_IN_SUBAGENT. **requested_model:** gpt-5.5-equivalent (Claude Opus 4.8, Ultracode). **requested_reasoning_effort:** xhigh.


## Pass 10/10 — copied-plans + transcripts + supporting-files + навигатор RAG-12 + transfer-checklist + финальный вердикт + cross-folder связка + честное покрытие 200 файлов

> ФИНАЛЬНЫЙ проход папки 12 И всего 12-папочного RAG фазы 1. Здесь дочитаны оставшиеся файлы (2 транскрипта, 30 copied-plans, 108 supporting-files, copy-manifest) и собрана итоговая сборка: навигатор RAG-12, transfer-checklist для плана продолжения VS/VSCode, финальный вердикт по папке, cross-folder связка со всеми RAG-01..11, честное покрытие 200 файлов. Более ранние секции (Pass 1/10 … Pass 9/10) НЕ тронуты.

---

### 10.1 — Транскрипты сессий: что реально делали (самая свежая работа проекта)

Два транскрипта — это НЕ дубликат аудита, а **живая хроника последних действий**. Они важнее статус-пакета, потому что показывают реальный код, а не самоотчёт.

#### Транскрипт 1 — `Extension development session.md` (4 KB) — governance-урок, НЕ код

Короткий диалог-разбор. Luba: «ещё одна проблема модели — ответ без доказательств. Почему? Где у нас проблема в настройках?». Контекст-цитата: агент раньше положил `project-AGENTS.md`, `project-CLAUDE.md` и 6 skill-файлов в `supporting-files/`, а Luba спросила «почему эти файлы не в проекте?».

Разбор (что честно признал агент):
- Файлы физически ЕСТЬ в `C:\Users\lubao\Documents\xiigen\llm mvp\extension development\supporting-files`, но их **нет** в активном `C:\Projects\llm_mvp_core`. Они названы `project-AGENTS.md`/`project-CLAUDE.md` (не активные `AGENTS.md`/`CLAUDE.md`), а skills лежат в `supporting-files/skills`, а не в активной `skills`-папке проекта — поэтому они **не управляют агентом**.
- Корневой дефект (слова агента): «я сложил инструкции рядом с проектом, как приложение, вместо того чтобы встроить их туда, где Codex реально их читает. Это та же самая проблема, которую ты ругаешь: **статус/пакет вместо действующего изменения**».
- Второй дефект: агент ответил «слишком быстро», проверив только текущее наличие/отсутствие, а не git-историю «кто и когда положил» → нарушение `DIAGNOSTIC_STATUS_CLAIM_REQUIRES_VISIBLE_EVIDENCE` (вывод без доказательства).

Значение для плана: **весь `supporting-files/` — это package-материал, а не активная конфигурация**. При переносе в VS/VSCode-план нельзя считать, что копия skill/AGENTS/CLAUDE «действует» — она действует только если установлена в активные управляющие файлы целевого репо.

#### Транскрипт 2 — `extension development session 2.md` (95 KB) — ДВА реальных кодовых среза + method-pivot

Это **самая свежая настоящая реализационная работа** проекта, выполнена в worktree `C:\Projects\llm_mvp_core-orchestrator-extensions` (ветка `orchestrator-extensions`). Хроника:

**(0) Setup M0 (первый шаг вехи M0 выполнен):** созданы 6 worktree `*-orchestrator-extensions` из свежайшей ветки каждого репо: `llm_mvp_core` (из `DesignToCode-clean`, 85 877 файлов, заполнение ~18 мин через фоновый parallel checkout — foreground не тянул 65k+ файлов), `claude/cursor/codex-xiigen-smart-skills` (из `main`), `xiigen-platform-dot-net-angular` (из `DesignToCode-pass75`), `xiigen-flows-dot-net-angular` (из `DesignToCode-pass70`). Push НЕ делали; мержабельность в latest+main сохранена (OP-67). Конвенция вписана в план как ADDENDUM A1/A2.

**(1) Кодовый срез №1 — РЕАЛЬНЫЙ product fix (broken pipeline hop stage-1/2 → stage-3):**
- Дыра: `SkillSelection` (стадия 3) вызывалась ПОСЛЕ `PromptToTask` (стадия 1) и `Orchestration` (стадия 2), но фактически получала **только сырой prompt** — стадия 3 была изолирована от 1/2. Это ровно машинно-конвейерная дыра из P22/P23 (RAG-12 Pass 4).
- Правка: `43.SmartSkillsDomain/XiigenExtensionApiService.cs` — `DecideSkills` теперь вызывается с `promptToTask.StructuredTask` + `orchestration.ScenarioPlan` (line ~143); добавлены builders `BuildSkillSelectionPrompt`/`BuildSkillSelectionHostFacts` (line ~554). Публичный контракт `SkillSelectionRequest` НЕ менялся (наполнили существующие поля richer context-ом → меньше риск для остальных репо).
- Тест: `Tests/ExtensionQualityGapRepairTests.cs` — новый регресс `XiigenExtensionApiService_SkillSelectionUsesStructuredTaskAndScenarioPlanHandoff`, сделанный «жёстким» к старой ошибке (обученная пара опирается только на новые host-facts из structured task/scenario plan; старый raw-prompt-only вызов не набрал бы порог context score).
- Проверка (честная): `dotnet restore` прошёл; `dotnet build … -v minimal` → 0 errors, 44 existing warnings; новый тест 1/1; соседний `ExtensionQualityGapRepairTests` 7/7; `git diff --check` чистый. Коммит/стейдж НЕ делали. `subagent_id_or_NONE: NONE` (parent-direct). Честная граница агента: «это не полное завершение Design To Code».

**(2) Кодовый срез №2 — РЕАЛЬНЫЙ product fix (Design Conversion live-local: fake-PNG + timeout + race):**
- Разведка нашла продуктовую дыру `artifact://`/fake screenshot proof около live-local/PNG пути в `44.DesignConversionDomain`. Живой прогон `DesignConversionLiveLocalServerTests` дал **реальный сбой 3/4** (один тест таймаутил до ready-файла за 75 сек).
- Корень таймаута — не бизнес-логика, а `dotnet run` внутри каждого теста (пересборка попадает в ready-timeout). Правка launcher-а: строить tool-проект один раз и запускать собранную DLL (`Tests/DesignConversionLiveLocalServerTests.cs` +59−4), без ослабления assertions/timeout.
- Затем закрыт сам fake-artifact след: machine-backed live-local startup probe больше не подсовывает `artifact://...png`, а генерирует **реальные deterministic local PNG-файлы** (`file:///...png` + sha256) через уже существующий локальный provider (`tools/DesignConversionLiveLocalServer/Program.cs` +76−24). Добавлены assertions на реальные PNG под `render-artifacts`, проходящие `DesignConversionBrowserPngEvidenceInspector`.
- Пойман ещё один реальный race: `ready.json` читался во время записи → починен **атомарной записью** (`File.Move` из temp-файла, +3−1).
- Проверка: live-local suite зелёный **4/4**; `ExtensionQualityGapRepairTests` 7/7; diff whitespace-clean.

**СВЯЗКА С ЖИВЫМ РЕПО (важно для плана):** этот второй срез — это тот самый Design-Conversion trust-boundary/PNG слой, который в моём базовом git-статусе `DesignToCode-clean` — активные modified-файлы `44.DesignConversionDomain/DesignConversionBrowserQaProviders.cs`, `DesignConversionQaContracts.cs`, `DesignConversionRenderEvidenceTrustBoundary.cs`, `Tests/DesignConversionRenderEvidenceTrustBoundaryTests.cs`, и последний коммит `Preserve measured PNG dimensions at trust boundary`. То есть **Design Conversion PNG-trust-boundary — это реально строящийся прямо сейчас продуктовый спайн**, а не бумага.

**(3) Method-pivot (Luba):** «я одного не понимаю — почему ты реализуешь сам, а не через суб агентов с опус 5.5 extra high???». Агент классифицировал: claims про код/тесты verified для parent-direct работы, но **метод** был governance gap (надо было поднять sub-agent route `gpt-5.5/xhigh`, parent — только оркестратор). Нашёл callable tool `multi_agent_v1.spawn_agent` с поддержкой `gpt-5.5` + `xhigh` (техпрепятствия НЕ было → неправильный выбор метода). Внёс в `AGENTS.md` новый gate `DESIGN_TO_CODE_REPEATED_TASK_SUBAGENT_ROUTE_GATE` (+11), верифицировал marker+diff.
**(4) Эскалация (Luba):** «НЕ НАДО ПРАВИТЬ СКИЛЫ!!! ТОЛЬКО РЕАЛИЗАЦИЯ ЧЕРЕЗ СУБ АГЕНТОВ» → агент остановил meta/governance + parent-direct, заспавнил worker `Banach` (`agent_id=019f45a7-3344-7871-b077-8dacf49d646e`, route `gpt-5.5/xhigh`). Транскрипт обрывается на «worker запущен, жду inspectable packet» — **возвращённого пакета в транскрипте НЕТ** (незакрытый хвост).

Итог транскриптов: **реально реализованы 2 кодовых среза (SkillSelection handoff + Design Conversion PNG trust-boundary), оба зелёные, оба UNCOMMITTED в orchestrator-extensions worktree.** Это первое живое движение по M2/M3 после всей плановой работы. Плюс живой governance-урок: parent должен идти через sub-agent `gpt-5.5/xhigh`, не реализовывать сам, и не превращать «через суб-агентов» в правку скилов.

---

### 10.2 — copied-plans/ (30 файлов): категоризация + какие authoritative

30 файлов — это **копии из `C:\Projects\llm_mvp_core\docs\plans`** (read/copy only, по `00-PATTERN-AND-SOURCE-INVENTORY.md`). НЕ оригиналы: источник authority — сами `docs/plans`, копии могут отставать. Категории:

| Категория | Файлы | Роль / authority |
|---|---|---|
| **КАНОНИЧНЫЙ extension-план** | `ORCHESTRATION-AND-PROMPT-TO-TASK-FULL-EXTENSIONS-PLAN-v3.md` (212 KB) + `…-EXTENSIONS-STATE.json` (66 MB) | Единственный источник authority по extension-реализации (v3). Явно supersedes v1. Статус в шапке: **PLAN-ONLY, реализация не разрешена без нового явного разрешения Luba**. ⚠️ Копия .md — **mojibake** (Cyrillic побит), читается только через оригинал в docs/plans. |
| **Companion (SUPERSEDED)** | `EXTENSION-IMPLEMENTATION-PLAN-v1.md` (48 KB) | Явно помечен `СТАТУС: СУПЕРСЕДЕД` — короткий companion, authority = только v3. Part A = текущая реализация, Part B = целевое (не реализовано). |
| **Ground-truth код-карта (⭐ самая полезная)** | `EXTENSION-REALITY-MAP-v1.md` (8 KB) | Read-only recon 3 способностей в `43.SmartSkillsDomain` с вердиктами + 6 GAP. См. 10.2a ниже — это лучшая стартовая карта для плана. |
| **Trainable-architecture audits (прогрессия)** | `TRAINABLE-ARCHITECTURE-AUDIT-v1/v2/v3/v4.md` + 4 STATE.json | Последовательные аудиты обучаемой архитектуры. **v4 = свежайший вердикт `PASS_WITH_LIMITATIONS`**: Q9 (baseline `e57aee28…`) заставил `PromptToTaskMachine`/`OrchestrationReasoningMachine` делегировать runtime-выбор в отдельные typed `ILogicalUnit` (а не держать скрытый private scorer/dictionary внутри machine). НО semantic quality **не заявлена**, host runtime **не заявлен**, checkpoint rewrite + fresh-load/continue-training gates — будущая работа. |
| **Обучение оркестрации (крупные)** | `ORCHESTRATION-MACHINE-TRAINING-PLAN-v17.md` + `-v20.md` (172 KB) + STATE (1.9 MB) | v20 = integrated replacement для v17/v19. Ключ: Phase 39 = trainable smart-skills readiness/package boundary; **Phase 40 = реальная устанавливаемая host-реализация** в `claude/codex/cursor-xiigen-smart-skills`, вся семантика/trainable-логика **остаётся в `llm_mvp_core`** (= R5/R6). |
| **Semantic quality** | `SMART-SKILLS-SEMANTIC-QUALITY-UPLIFT-PLAN-v1.md` + STATE, `-COMPLETION-PLAN-v1.md` + STATE (515 KB) | Планы подъёма семантического качества smart-skills (перекликаются с P28 «gate мёртв/false-by-construction»). |
| **Runtime/token/quality/learnability** | `EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md` (352 KB) + STATE **(467 MB!)** + `-REVIEW.jsonl` | Target-Outcome-Matrix план: live host runtime, token before/after, quality improvement, learnability, backup/export/import/share, continue-training, DPO, rollback, promotion/locator. Запрещает сужать цель до «честных ограничений». ⚠️ STATE = 467 MB — раздутый ledger-артефакт (provenance-флаг, не продукт). |
| **Dual-path readiness** | `EXTENSION-FULL-READINESS-DUAL-PATH-PLAN-v1.md` (86 KB) + EXECUTION-LOG (121 KB) + EXECUTION-STATE (124 KB) + `.review-ledger.md` | Dual-path 28×3 план. Статус: `DONE_FOR_EXECUTION_WITH_REVIEW_SCOPE` — прошёл plan-review как **артефакт-план**, НЕ доказывает product readiness. Источник 28-сценарного инвентаря (P31) и 84-файлового evidence-дерева. |
| **API/dashboard** | `API-DASHBOARD-PLAN-v1.md` (113 KB) + STATE (67 KB) | User-facing dashboard внутри core: пользователь вводит параметр → вызывает API → видит результат; человекочитаемые имена API + примеры. AR-01..AR-10 ledger. |
| **DPO governance/strategy** | `DPO-KNOWLEDGE-MAP-v1.md`, `SKILL-SELECTION-DPO-NEGATIVE-STRATEGY-v1.md` | Source-map: какие DPO skills/guides грузить на домен (Generic/Translation/Orchestration/PromptToTask/ArchitectBehavior); стратегия негативных DPO-пар. Gate: «DPO later» при claimed trainable readiness = план падает. |

**10.2a — `EXTENSION-REALITY-MAP-v1.md` — ground-truth 3 способностей (для плана самое ценное):**
- **PromptToTask** (стадия 1): `PromptToTaskMachine.Translate(…)` — **DETERMINISTIC stub**, plain sealed class, НЕ на иерархии ILearningMachine, всегда возвращает `TaskType="not_ready_no_semantic_task"`. Не wired в 15.OrchestratorModel. Training runner MISSING. (GAP-1)
- **OrchestrationReasoning** (стадия 2): `OrchestrationReasoningMachine.Plan(…)` — **DETERMINISTIC stub**, всегда `ScenarioId="not-ready-no-orchestration-inference"`, RiskFlags всегда `NO_ORCHESTRATION_INFERENCE_IMPLEMENTED`. Не wired. Runner MISSING. (GAP-2)
- **SkillSelection** (стадия 3): `SkillSelectionMachine` — **TRAINABLE partially**: реально `ILearningMachine<SkillSelectionDomain>`, есть настоящее обучение весов `_candidatePositionWeights` через `LearnPair()` (`ContractOnly:false`), checkpoint-record + manifest. НО **`ExportAsync`/`ImportAsync` → `*_NOT_READY`** (нет persistence — checkpoint не переживает рестарт), runner MISSING, регистрация в 15.OrchestratorModel не подтверждена. (GAP-3, GAP-6)
- GAP-4: DPO signal builders у всех 3 — contract-only stubs, реальные chosen/rejected пары не текут в pipeline. GAP-5: PromptToTask/OrchestrationReasoning вообще без ILogicalUnit/ILogicalFlow-обёртки.
- ⚠️ Заметка: `EXTENSION-REALITY-MAP-v1` (2026-06-04) частично устарел — транскрипт-2 и `TRAINABLE-ARCHITECTURE-AUDIT-v4` показывают, что Q9 уже добавил typed `ILogicalUnit`-делегацию для PromptToTask/OrchestrationReasoning, а срез №1 транскрипта-2 добавил handoff structured-task/scenario-plan в SkillSelection. То есть реальный код УЖЕ немного впереди reality-map. **Для плана: перепроверить reality-map против `llm_mvp_core-orchestrator-extensions` перед стройкой.**

---

### 10.3 — supporting-files/ (108 файлов) + copy-manifest provenance

108 файлов рекурсивно = **24 прямых supporting + 84-файловое evidence-дерево** (`copy-manifest.json` v3, `fresh_execution:true`, `requested_model:gpt-5.5/xhigh`, `missing_expected_count:0`).

- **24 прямых** (HOWTO/доки/skills/rules): `CLAUDE-XIIGEN-HOWTO-ru`, `CURSOR-XIIGEN-HOWTO-ru`, `CODEX-XIIGEN-HOWTO-ru`, `API-DASHBOARD-QA-HOWTO-ru`, `EVIDENCE-SCOPES-HOWTO-ru`, `EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-HOWTO-ru`, `LOCAL-MODEL-LIFECYCLE-HOWTO-ru`, `XIIGEN-DEBUG-TRACE-HOWTO-ru`; проектные `SUBPROJECT-MAP`, `PROJECT-DOCUMENTATION-INDEX`, `43-smart-skills-domain(+dashboard,+semantic-quality-authorization)`, `HOW-TO-USE-SKILLS`, `SESSION-START-PROMPT`, `RUSSIAN-STATUS-AND-EXECUTION-LOG-TEMPLATE`; **`project-AGENTS.md` + `project-CLAUDE.md`** (копии, НЕ активные — урок транскрипта-1); 6 skills (`audit-protocol`, `claim-verification`, `client-adaptive-learning-loop`, `design-conversion-dpo-authoring`, `dpo-triple-format`, `planning-dpo-authoring`).
- **84-файловое `evidence-extension-full-readiness-dual-path/`**: матрицы фаз `phase-05..16` (readiness matrices + `*-intake-downgrade` для token/semantic/learnability/lifecycle/checkpoint-locator; `phase-13-hidden-deterministic-shortcut-review`; `phase-14-user-visible-status-dashboard`; `phase-16-propagation-evidence`); `product-readiness-{matrix,action-plan,scenario-board}`; и 8 sub-folder: `scenario-execution__{claude,cursor,codex}-same-scenario-dual-path-join-*`, `token-savings-*`, `token-host-visible-cursor-row12-sanitized-*` (29 файлов), `quality-semantic-row12-heldout-ablation-freshload-*`, `status-board-*`. Это **evidence-снимки на 2026-06-28**, из которых P31-P36 вывели 0/28 joined + Path B фактически отсутствует. **Спот-чек: структура совпадает с манифестом (source==destination counts).**
- **Provenance-флаг:** 3 огромных STATE.json (467 MB / 66 MB / 1.9 MB) скопированы дословно — это раздутые ledger-состояния, НЕ продукт; в этом проходе намеренно НЕ читались целиком.

---

### 10.4 — НАВИГАТОР RAG-12 (карта собственных секций на один экран)

| Проход | Строки | Что внутри (topic) | Ключевой вывод |
|---|---|---|---|
| **Pass 1/10** | 12-273 | Инвентарь 200 файлов + ТЕКУЩИЙ статус сборки + карта 50-pass аудита | 3 реальных host-репо существуют; 15 фаз claimed vs real; обучающий движок — критдефект |
| **Pass 2/10** | 274-633 | opus-pass 01-08: claimed-vs-package · DoD · real host-code · real API · real DPO-lifecycle · evidence-reality · session-work · baseline matrix | Оверклейм-дельта; планка «done»; где реально собран продукт для ДРУГИХ хостов |
| **Pass 3/10** | 634-963 | opus-pass 09-16: **Claude host** (entry/pipeline/learn/qa) + **Cursor host** (entry/pipeline/token/qa) | Cursor зрелее; главная token-находка (row12); real-vs-stub по 2 хостам |
| **Pass 4/10** | 964-1380 | opus-pass 17-24: **Codex host** (entry/pipeline/learn/qa) + machine-pipeline (prompt→task→plan→skills→dpo-rows) | Полный свод 3 хостов; стадии 1→2→3 = keyword/lookup/bag-of-words; order-leakage; DPO-строки на диске |
| **Pass 5/10** | 1381-1727 | opus-pass 25-32: dpo-training · checkpoint-locator · token-savings · semantic-quality · dashboard · lifecycle-ops · scenario-inventory · claude-scenarios | DPO РЕГРЕССИРОВАЛ; token delta=0 корень; semantic gate мёртв; 0/28 инвентарь |
| **Pass 6/10** | 1728-1851 | opus-pass 33-40: cursor/codex scenarios · pathA-harness · pathB-desktop · core-boundary R6 · trainable-audit R4 · tests-qa · git-branch | 0/9+0/10 joined; Path B отсутствует; семантика в core но триплицирована в 3 адаптерах; тесты honesty-only |
| **Pass 7/10** | 1852-2030 | opus-pass 41-49: adversarial · overclaims · missing-scenarios · integration-e2e · provenance-legal · ux · perf-proof · **priority M0→M6** · risks-decisions | Критпуть M0→M6; провенанс — слабейший слой; 13 Luba-решений; RCE-риск |
| **Pass 8/10** | 2031-2160 | opus-PROJECT-CLOSURE-PLAN ч.1: структура + executive verdict + **M0/M1/M2** backbone + **ADDENDUM A1/A2** | M1/M2 = узкое место; A1 (6-репо orchestrator-extensions worktree) + A2 (обучение ЧЕРЕЗ ILearningDynamicGraph DPO-build) |
| **Pass 9/10** | 2161-2264 | opus-PROJECT-CLOSURE-PLAN ч.2: **M3/M4/M5/M6** backbone + cross-cutting + registry tail + сверка счёта дыр | 293 деклар. vs ~245 консолид. (дедуп ~48); критпуть M1→M2; R-01 главный риск |
| **Pass 10/10** | (эта секция) | copied-plans + транскрипты + supporting-files + навигатор + transfer-checklist + финальный вердикт + cross-folder + честное покрытие 200 | Собран перенос в план VS/VSCode; закрыта папка 12 и весь 12-папочный RAG |

**Быстрый доступ по темам:** статус-пакет самоотчёта → Pass 1 §2; РЕАЛЬНЫЙ статус от кода → Pass 1 §3; host-разборы → Pass 3 (Claude/Cursor), Pass 4 (Codex); машинный конвейер стадий → Pass 4 §P21-24; token/semantic/DPO обучение → Pass 5; сценарии 28×3 + Path A/B → Pass 5-6; критпуть M0-M6 + 13 решений → Pass 7-9; A1/A2 (worktree + DPO-build связка) → Pass 8 §8.7.

---

### 10.5 — TRANSFER CHECKLIST для плана продолжения VS/VSCode

Каждый актив папки 12 + как он ложится в план продолжения VS/VS Code:

| # | Актив (где в RAG-12 / в папке) | Как маппится в continue-план |
|---|---|---|
| 1 | **Хребет M0-M6** (Pass 8-9; `opus-PROJECT-CLOSURE-PLAN.md`) | Это **execution-backbone** плана продолжения. Порядок: M0 (save/build/tests) → M1 (de-risk обучения) → M2 (реальный инференс — УЗКОЕ МЕСТО) → M3 (host+транспорт+wiring+lifecycle+R6) → M4 (измерение) → M5 (dual-path 28×3) → M6 (приёмка/публикация). VS/VSCode-план стартует с M0/M1, не с украшения. |
| 2 | **3 host-репо** (Pass 1 §3a; Pass 3-4) | Реальные адаптеры: **Cursor mature** (полнее всех, token row12), **Claude skeleton**, **Codex handwritten** (внешне заблокирован marketplace). VS/VSCode — потенциально **4-я host-семья** (Luba-решение P43-2), строится по тому же тонкому-адаптеру шаблону. |
| 3 | **293-gap реестр** (Pass 1 §1b; Pass 2-9; `opus-pass-01..50`) | **Backlog** плана. ~245 после дедупа; ~90% «needs реализуй» (внутри контроля). Реестровый хвост P40-49 с file:line (Pass 9). |
| 4 | **ILearningDynamicGraph DPO-build зависимость** (A2, Pass 8 §8.7) | **Жёсткая зависимость на критпути.** Расширения обучаются НЕ через свой движок, а через `ILearningDynamicGraph` DPO-build (проект `dpo legal build`), реализуемый в `xiigen-platform`/`xiigen-flows`; расширения — его клиенты по «better-output→learning-DPO» API. M1/M2 закрываются ИМ, не строятся заново. |
| 5 | **Ветка + 6-репо worktree топология** (A1, Pass 8 §8.7; транскрипт-2) | Реализация ТОЛЬКО на ветке `orchestrator-extensions` в каждом из 6 репо (`llm_mvp_core` + claude/cursor/codex-smart-skills + xiigen-platform + xiigen-flows), worktree `C:\Projects\<repo>-orchestrator-extensions`, мержабельность в latest+main (OP-67). **Worktree уже созданы** (первый шаг M0 выполнен). |
| 6 | **13 Luba-решений** (Pass 7 §7.10; Pass 9 хвост) | **Decision-gate ledger** плана: трансформерный DPO vs словарь; remote/marketplace vs local-only; маршрут через ILogicalOrchestrator/15 vs fixed-linear; scope xiigen-platform/flows; **VS/VSCode 4-я host-семья**; репо-топология; teacher/distillation; лицензия+канал; security/Workspace-Trust; ToS-риск; мин. набор хостов; кто платит remote; эскалация from-scratch если M1 провалится. |
| 7 | **RCE + distillation-legal must-fix** (Pass 7 §7.5/§7.9; P45) | Security-вектор RCE (core-пути от расширения) + ToS-зависимость distillation (может обнулить value-prop). Обязательные строки риск-реестра M6 + provenance-манифесты M4. Cross-ref RAG-05/07. |
| 8 | **Реальный trainable reference: OrchestrationPlanMachine в 42.ProjectWikiDomain** (Pass 4-5; A-LM-03 Bootstrap units) | **Шаблон-порт**: единственный настоящий обучаемый ILearningMachine с Bootstrap-units + DPO starting point. Порт его паттерна (train/export/import/locator/checkpoint) на PromptToTask/OrchestrationReasoning/SkillSelection в M2. |
| 9 | **Реальный VS+VSCode SmartCodeWiki vsix** (host-шаблон, cross-ref RAG-10/11) | **Host-template** для VS/VSCode entrypoint: registered command / chat participant / tool window / panel. Ядро-логика остаётся в `llm_mvp_core` (R6 — тонкий адаптер). |
| 10 | **`EXTENSION-REALITY-MAP-v1.md` + `TRAINABLE-ARCHITECTURE-AUDIT-v4`** (10.2a) | **Ground-truth код-карта** 3 способностей (стадия 1/2 = stub, стадия 3 = trainable но Export NOT_READY). Стартовая точка M2; перепроверить против orchestrator-extensions worktree. |
| 11 | **2 UNCOMMITTED кодовых среза** (транскрипт-2, 10.1) | Уже реализованы: SkillSelection handoff fix (`XiigenExtensionApiService.cs`) + Design Conversion PNG trust-boundary (`DesignConversionLiveLocalServer`, `44.DesignConversionDomain`). **Первое M2/M3-движение — план должен продолжить отсюда, а не с нуля.** |
| 12 | **28-сценарный dual-path инвентарь + 84-файл evidence** (Pass 5-6; supporting-files) | **M5 harness-каркас**: 28 сценариев × Path A (boundary) + Path B (desktop). 0/28 joined сейчас → цель M5. Evidence-снимки 2026-06-28 = baseline. |
| 13 | **API-DASHBOARD-PLAN** (10.2; copied-plans) | User-facing тест-панель API внутри core — часть M4/M6 (пользователь видит/тестирует API расширений человекочитаемо). |

---

### 10.6 — ФИНАЛЬНЫЙ ВЕРДИКТ по папке 12

**Что такое папка «extension development»:** это **три слоя в одном** — (1) ТЕКУЩЕЕ состояние сборки расширений (самоотчёт-пакет 2026-07-08: доска 15 фаз, requirements-matrix, copy-manifest), (2) **50-проходный Opus-аудит от кода** (293→~245 дыр, `opus-PROJECT-CLOSURE-PLAN.md` + 50 pass-файлов) — авторитетный слой, и (3) **closure-roadmap M0-M6** с ADDENDUM A1/A2 и 13 Luba-решениями. Плюс живые транскрипты с 2 реальными кодовыми срезами.

**Единственная честная сводка (не оверклеймить):**
- **0/15 фаз реально done** как продукт (2 «done» — только package/copy-работа; 13 не закрыты). **0/28 сценариев** joined dual-path (14 «scoped accepted», но joined = 0). **Реальный продукт UNCOMMITTED** (2 зелёных среза в orchestrator-extensions worktree, без коммита).
- **Стадии машинного конвейера:** стадия 1 (PromptToTask) и 2 (OrchestrationReasoning) — **детерминированные stub-ы** («not_ready»); стадия 3 (SkillSelection) — **обучаема в памяти, но Export/Import NOT_READY** (не переживает рестарт). Обучающий движок игрушечный и **РЕГРЕССИРОВАЛ** (R-01: единственный градиентный DPO, loss↑→ln2, held-out≈0.56).
- **Узкое место = M1→M2** (реальный обучаемый инференс). Всё, построенное до M2, возвращается `not-proven` — это и объясняет «0/28 при 14 scoped accepted».

**Что continue-план должен сделать ПЕРВЫМ:**
1. **M0:** закоммитить 2 зелёных среза (SkillSelection handoff + Design Conversion PNG trust-boundary) в `orchestrator-extensions`, обеспечить build+tests+sln+trx, проверить мержабельность в latest+main (OP-67). Реальный продукт не должен оставаться uncommitted.
2. **M1 (de-risk, ПЕРЕД стройкой):** feasibility-спайк обучающего движка — но по A2 это = реализация/консюмирование `ILearningDynamicGraph` DPO-build (RAG-05), НЕ новый движок. Если M1-спайк провалится → эскалация from-scratch (Luba-решение #13).
3. Не строить proof/scenario-работу до M2 (правило P48-2: заморозить capability-зависимое до реального инференса).
4. Свести 13 Luba-решений в один decision-gate ledger и получить ответы (особенно: **VS/VSCode как 4-я host-семья** — прямо релевантно цели этого RAG).
5. Идти через sub-agent route `gpt-5.5/xhigh` (урок транскрипта-2), parent — оркестратор.

---

### 10.7 — CROSS-FOLDER связка (как папка 12 соединяется с RAG-01..11) — закрывает весь 12-папочный RAG

- **A2 → RAG-05 (`dpo legal build` / ILearningDynamicGraph) + RAG-06 (`llm dynamic flow`):** самая жёсткая связка. Обучающий движок расширений (M1/M2) **не строится в папке 12** — это `ILearningDynamicGraph` DPO-build из RAG-05, потребляемый по API. RAG-06 (dynamic flow) — механика того же графа. Extension closure имеет **hard dependency** от RAG-05/06 на критпути.
- **Design→Code сторона → RAG-04 (`44.DesignConversionDomain` / design-to-code session) + RAG-11 (FigmaToCode / extensions-research):** второй кодовый срез транскрипта-2 (Design Conversion live-local PNG trust-boundary) — **прямо тот же домен `44.DesignConversionDomain`, что в RAG-04**; это и есть живой modified working-set моей ветки. RAG-11 (Figma→Code исследование) даёт design-input сторону того же продукта.
- **Общий центр 11.DPO/DpoTrainer → RAG-05 / RAG-08 / RAG-10 / RAG-12:** все четыре папки крутятся вокруг одного тренера `11.DPO/DpoTrainer` (checkpoint save/load, held-out eval, continue-training). RAG-08 (orchestrator-experiment) + RAG-10 (extensions-orchestration-based-on-our-data) — эксперименты того же обучения; папка 12 — их продуктовое применение в расширениях.
- **Host-адаптер шаблон → RAG-10 / RAG-11:** тонкий host-адаптер (registered command / chat participant / vsix) для VS/VSCode берётся из RAG-10/11 (реальный SmartCodeWiki vsix как template), а вся семантика остаётся в `llm_mvp_core` (R6). Папка 12 даёт 3 существующих адаптера (Claude/Cursor/Codex) как reference.
- **Prompt→task машины → RAG-10:** стадии PromptToTask/OrchestrationReasoning/SkillSelection (43.SmartSkillsDomain) — те же машины, что RAG-10 разбирал как «orchestration based on our data»; папка 12 даёт их ground-truth reality-map + trainable-audit v4.
- **Провенанс/легал → RAG-05 + RAG-07 (external-sources):** distillation-ToS + data-provenance риск (P45) — тот же слой, что RAG-05 (legal build) и RAG-07 (external sources лицензии). Слабейший слой во всех трёх.
- **Итог 12-папочного RAG:** папки 1-3 = сравнение веток/оркестратор/перевод-обучение; 4 = design-conversion; 5-8 = обучающее ядро (DPO legal build / dynamic flow / external sources / orchestrator experiment); 9 = claude-sessions; 10-11 = extensions-оркестрация + research; **12 = продуктовая интеграция расширений + closure-roadmap.** Continue-план VS/VSCode = M0-M6 (папка 12) как backbone, обучение через RAG-05/06, host-template из RAG-10/11, design-сторона из RAG-04/11, provenance-gate из RAG-05/07.

---

### 10.8 — Честное покрытие всех 200 файлов + границы прохода 10

**200 файлов ровно** (`find . -type f` = 200): 62 корневых + 30 copied-plans + 108 supporting-files.

| Слой | Файлов | Как покрыто |
|---|---|---|
| Верхнеуровневый статус/навигация | 8 | **Глубоко прочитаны** (Pass 1 + Pass 10: README, CURRENT-REPORT, STATUS-BOARD, REQUIREMENTS-MATRIX, PLANS-INVENTORY, PATTERN-INVENTORY, 50-PASS-LEDGER, 00-PATTERN) |
| 50-проходный Opus-аудит | 51 (closure + pass-01..50) | **Глубоко прочитаны** по темам Pass 2-9 (closure целиком в Pass 8-9; каждый pass-файл сведён) |
| Транскрипты | 2 | **Глубоко прочитаны целиком** (Pass 10) — 2 кодовых среза + governance-урок |
| copied-plans | 30 | **Категоризированы + headers/ключевые прочитаны** (Pass 10): reality-map + dpo-knowledge-map целиком; v3/v4/v20/runtime/dual-path/API/impl-v1 headers; copy-manifest head |
| supporting-files | 108 | **Структурно + спот-чек** (Pass 10): 24 прямых по манифесту, 84-файл evidence-дерево по структуре (source==destination counts) |
| Мусор | 1 | `grep.exe.stackdump` — игнорируется (не входит в смысловой инвентарь) |

**Границы честности прохода 10 (не оверклеймить):**
- `claim_scope`: дочитаны 2 транскрипта целиком, 30 copied-plans категоризированы (2 прочитаны целиком, ~7 по headers), supporting-files по структуре+манифесту; собраны навигатор/transfer-checklist/финальный-вердикт/cross-folder/покрытие. Русский, read-only, записан ТОЛЬКО RAG-файл (append), ранние секции не тронуты.
- `evidence_scope`: прямые Read статус-файлов, обоих транскриптов (1048 строк session-2 + session-1), reality-map + dpo-knowledge-map + copy-manifest head + 7 plan-headers; `find`/`ls`/`grep` по деревьям для counts (200 = 62+30+108).
- **НЕ доказано этим проходом:** любые product-readiness утверждения; возвращённый пакет worker-а `Banach` (транскрипт обрывается на «жду packet» — не виден); построчное чтение 3 гигантских STATE.json (467 MB/66 MB/1.9 MB — provenance-флаг, намеренно не читались); фактический статус реализации `ILearningDynamicGraph` DPO-build (design-complete по A2, НЕ реализован — блокирует M2); точность каждого file:line Opus-реестра против текущего дерева (родитель верифицирует против кода); коммит/push 2 UNCOMMITTED срезов.
- `subagent_id`: UNKNOWN_IN_SUBAGENT. `requested_model`: gpt-5.5-equivalent (Claude Opus 4.8, Ultracode). `requested_reasoning_effort`: xhigh.

**Закрытие:** этим проходом закрыта папка 12/12 И весь 12-папочный Phase-1 RAG. Все 200 файлов папки 12 инвентаризованы и покрыты; собран перенос в план продолжения VS/VS Code (backbone M0-M6, обучение через RAG-05/06, host-template RAG-10/11, design-сторона RAG-04/11, provenance-gate RAG-05/07). Следующий шаг вне RAG — авторинг самого continue-плана (Fable пишет, Opus верифицирует) в папку `visual studio & visual studio code continue`.
