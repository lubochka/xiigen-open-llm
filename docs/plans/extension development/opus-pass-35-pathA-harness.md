# Opus Pass P35 — Реальность автоматической Path A харнесс-упряжи (adapter/API/core)

Фокус прохода: существует ли РЕАЛЬНАЯ автоматическая boundary-харнесс (adapter/API/core test),
которую можно вызвать СЕЙЧАС? Есть ли тест-проект? Может ли он запуститься?
Оценка от живого кода `C:\Projects\llm_mvp_core`, а не от copy/status пакета.

## Что реально есть (честная картина)

1. **Единственный .NET-тест-проект как харнесс — монолитный `Tests/LlmFromScratch.Tests.csproj`**
   (xUnit 2.9.2, net9.0, `Microsoft.NET.Test.Sdk 17.12.0`), **233 `*.cs`** тест-файла,
   ссылается на **40 проектов**, включая границу расширения `43.SmartSkillsDomain`,
   `15.OrchestratorModel`, `42.ProjectWikiDomain`, `25.ModelServer`, `11.DPO`,
   `43.SmartSkillsDomain.Dashboard`, `44.DesignConversionDomain`.
   Второй тест-проект — `Tests.ApiDashboardWpf` (WPF dashboard). В `LlmFromScratch.sln` 65 проектов;
   ровно 2 из них тестовые.

2. **Граница расширения покрыта в-процессе.** `Tests/XiigenExtensionApiContractsTests.cs` (1741 стр., 17 файлов
   ссылаются на `XiigenExtension*`) конструирует `XiigenExtensionRequest` и напрямую зовёт
   `service.ProcessHostExtensionRequest(request)` (напр. `:261`, `:307`, `:395`, `:473`). Это и есть
   основная «Path A automated boundary» — но это ИМЕННО тот in-process core-fallback, который
   P33 gap-2 / P34 gap-2 пометили как «не host-route». Тесты валидируют контракт/редакцию/
   no-hidden-determinism/skill-decoy-фильтры/candidate-order-leakage, а НЕ семантическую корректность
   (ядро by-construction возвращает `semantic_quality_claimed=false`).

3. **Есть один настоящий процесс-спавн-харнесс.** `XiigenExtensionApiContractsTests.cs:1531-1569`
   `RunFullHostScorer` спавнит `powershell` (`:1542` `FileName="powershell"`) на
   `tools/token-quality-full-host-scorer.ps1` (`:1539`), таймаут 60с (`:10`), коллекция
   `[Collection("ProcessSpawningTests")]` (`:7`). Ещё `DesignConversionLiveLocalServerTests.cs:395`
   спавнит `dotnet run`. В `tools/` 8 boundary/scorer-скриптов
   (`token-quality-full-host-scorer.ps1`, `token-quality-runner.ps1`, `xiigen-extension-host-gate.ps1`,
   `skill-selection-core-extension-boundary-gate.ps1`, `phase12-cross-host-token-quality-runner.ps1`,
   `frdp-row12-cursor-token-trace-harness.ps1`, `host-qa-HOST-08A.ps1`,
   `token-04-rerun-after-quality-gap-repair-01.ps1`) — все PowerShell (Windows).

4. **Единственный реально исполненный «Path A automated boundary» прогон** зафиксирован в
   `docs/plans/evidence/extension-full-readiness-dual-path/phase-03-automated-boundary-report.md`
   (+ `phase-03-command-output/01..11-*.txt`, `phase-03-automated-boundary-packets.json`).
   Это авторитетный самоотчёт того, что реально удалось прогнать как автоматическую границу.

## Ключевые дефекты реальной Path A харнесс-упряжи (НОВОЕ)

### 35-1. Нет единой воспроизводимой Path A boundary-харнесс: ~11 ручных `dotnet test --filter` + python-команд, результаты вручную переписаны в markdown; нет runner-скрипта, нет `.runsettings`, нет CI
- `phase-03-automated-boundary-report.md:52-67` — таблица из 11 отдельных command-id
  (`candidate_refs`, `api_contracts`, `token_trace`, `quality_scoring`, `dpo_dataset`,
  `checkpoint_locator`, `blocker_audit`, `phase02_validator`, `phase03_structural_validation`,
  `targeted_search`, `git_diff_check`), выводы скопированы вручную в `phase-03-command-output/*.txt`.
- `:57` «24 API/core/boundary contract tests passed; **excluded one in-memory training/mutation test**» —
  принятая граница = ad-hoc `dotnet test --filter` подмножество, набранное руками.
- `find *.runsettings`→0; `.github/workflows`→ОТСУТСТВУЕТ (нет CI-гейта в core-репо).
- Отличие от P07 (сессия narrated-not-executed) и P12/P16 (осиротевшие node-QA host-репо): здесь
  именно .NET Path A-граница core-репо не собрана в один воспроизводимый gate.
- Закрытие: единый runner (скрипт/`.runsettings`/`dotnet test --filter <boundary-trait>`),
  фиксирующий команду+stdout+exit, подключить к CI; заменить ручную транскрипцию машинным сбором.

### 35-2. 57% сценариев (16/28) НЕ имеют вызываемой автоматической boundary-команды
- `phase-03-automated-boundary-report.md:49-51` — `NO_BOUNDARY_COMMAND_DISCOVERED`=**7**,
  `BOUNDARY_COMMAND_INSUFFICIENT`=**9** (итого 16 из 28).
- `:72-99` per-row: `product-readiness`(1/10/19), `learnability-training`(5/14/24),
  `dpo-quality`(6/15/25), `asset-lifecycle`(7/16/26), `locator-update`(9/18/28),
  `codex-chat-ui`(21) — все `NEEDS_VERIFICATION`.
- Для этих сценариев Path A автоматическая харнесс попросту НЕ существует (не «прогнали и упало»).
- Закрытие: спроектировать non-mutating boundary-команду на каждый из 16 (или явный
  read-only срез), довести accepted boundary-packets с 12/28.

### 35-3. `product-readiness` (сквозной конвейер) Path A харнесс не существует — только фрагментарные guard-тесты
- `:112` «no single product workflow boundary command proved
  adapter/API/core/model/token/quality/lifecycle/checkpoint/locator together» (ряды 1/10/19 =
  `BOUNDARY_COMMAND_INSUFFICIENT`).
- То есть нет ни одной автокоманды, прогоняющей полный конвейер prompt→task→plan→skills→
  token→quality→lifecycle→checkpoint→locator как одно целое; «граница» — набор разрозненных
  guard-ассертов на отдельные юниты.
- Закрытие: end-to-end boundary-тест/раннер одного сценария сквозь весь конвейер с одним prompt-hash.

### 35-4. Принятая Path A-граница ПО ПОСТРОЕНИЮ исключает все train/mutation-тесты → learnability/dpo/lifecycle/checkpoint-promotion/locator не имеют вызываемой авто-границы
- `:57` «excluded one in-memory training/mutation test»;
  `:62` `blocker_audit` «Audited excluded probes that **train, save checkpoints, regenerate artifacts**»;
  `:114-117` learnability/dpo/asset-lifecycle/locator — все исключены, т.к. тренируют/мутируют/
  пишут вне разрешённого scope.
- Значит автоматическая харнесс структурно ограничена read-only guard-фрагментами; ровно те
  capability, что определяют «обучаемое расширение» (адаптивное обучение, DPO, lifecycle,
  promotion, locator), из авто-границы выпадают.
- Отличие от P11/P19 (сам lifecycle не доказан) — здесь именно ХАРНЕСС не покрывает их как Path A.
- Закрытие: ввести изолированную (temp-checkpoint/sandbox) mutating-boundary-харнесс, которую
  можно гонять автоматически без порчи общего состояния; включить в accepted boundary.

### 35-5. Единственная процесс-спавн Path A-харнесс (full-host-scorer) — Windows-only и завязана на закоммиченные артефакты ⇒ не запускается в CI/чистом клоне/не-Windows
- `XiigenExtensionApiContractsTests.cs:1542` `FileName="powershell"` (Windows PowerShell 5.1, не
  кроссплатформенный `pwsh`), `:1539` спавнит `tools/token-quality-full-host-scorer.ps1`.
- `:760` / `:842` `Assert.True(Directory.Exists(adaptiveRoot), "Missing adaptive test asset root")` —
  тест ЖЁСТКО падает, если закоммиченный артефакт
  `artifacts/extension-runtime-token-quality-learnability/adaptive-learnability-01/user-cache`
  отсутствует; `:219-226` тест читает с диска `artifacts/api-boundary/API-02-method-contract-schema.json`.
- Нет CI. Итог «может ли запуститься сейчас» = только на этой Windows-dev-машине при наличии
  закоммиченных артефактов; на чистом клоне/Linux-CI часть Path A-границы падает не по продукту,
  а по среде.
- Отличие от P16 (headless Cursor extensionTests) — здесь OS/artifact-coupling .NET xUnit харнесса.
- Закрытие: сделать scorer-путь кроссплатформенным (или пометить Windows-only и гейтить),
  генерировать/восстанавливать фикстуры в самом тесте, добавить CI-прогон на чистом клоне.

### 35-6. Собираемость/запускаемость Path A-харнесса СЕЙЧАС не доказана и под риском: тяжёлые незакоммиченные правки в `44.DesignConversionDomain` + `Tests`, TestResults устарели на 5 дней, зелёного билда под текущее дерево нет
- git status: `M` по ~20 файлам `44.DesignConversionDomain/*` и по нескольким `Tests/*.cs`
  (незакоммиченное рабочее дерево).
- `Tests/TestResults/*` — самые свежие Sequence-XML датированы **2026-07-03..07-04** (5 дней до
  board 2026-07-08); свежего прогона под текущее дерево нет.
- Монолитный `Tests` ссылается на 40 проектов, включая тяжело-модифицированный
  `44.DesignConversionDomain`: ошибка компиляции в этом домене заблокирует ВЕСЬ Path A-харнесс.
- Прямой `dotnet build` мне (read-only ассессор) запрещён (permission denied) — «может ли
  запуститься» осталось НЕ доказанным; честный статус = unverified/at-risk.
- Отличие от P07 (транскрипт сессии) — здесь именно текущая собираемость рабочего дерева харнесса.
- Закрытие: закоммитить/стабилизировать `44.DesignConversionDomain`+`Tests`; прогнать
  `dotnet build` + boundary `dotnet test --filter` под текущее дерево; сохранить команду+вывод+exit;
  привязать accepted boundary к конкретному commit-hash зелёного прогона.

## Дедуп (почему это НОВОЕ)
Не повторяю: P07 (сессия: 0 исполненных dotnet build/test в транскрипте — про транскрипт, не про
current-tree харнесс), P09 (core-ps1 temp-csproj `dotnet run` — host→core route, не xUnit-харнесс),
P12/P16 (осиротевшие node-QA host-репо claude/cursor без `npm test`/CI — другой репозиторий и стек),
P15 (callers `TokenTraceAccounting`=Tests+Program.cs — про arch-disconnect движка, не про
воспроизводимость/полноту Path A-харнесса), P31-P34 (scenario-execution ad-hoc PS/node Path A/Path B
папки и dual-path joins по-хостам — про конкретные прогоны сценариев, не про .NET xUnit boundary-упряжь
как gate: её единство, полноту 28-сценарного покрытия, train-exclusion, OS/artifact-coupling и
current-tree собираемость).
