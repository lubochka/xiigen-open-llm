# Pass P29 — Dashboard / docs + route-tied QA (реальное состояние)

Дата: 2026-07-08. Ассессор: Opus 4.8. Срез: РЕАЛЬНЫЙ API-dashboard (`43.SmartSkillsDomain.Dashboard`)
и его «route-tied QA» (`Tests.ApiDashboardWpf` + артефакты `artifacts/.../*dashboard*`), по живому
коду и артефактам `C:\Projects\llm_mvp_core`. Оценка — от реального кода/артефактов, не от copy-status пакета.

## Что прочитано целиком / bounded

- `43.SmartSkillsDomain.Dashboard\ApiDashboardRunner.cs` (285) — сам «раннер» дашборда.
- `ApiDashboardResultPresenter.cs` (174), `ApiDashboardViewModel.cs` (445), `ApiDashboardFixtureRunner.cs` (328),
  `ApiDashboardSourceDisjointGuard.cs` (316), `ApiDashboardCatalog.cs` (грепом), `.csproj` (обе).
- `Tests.ApiDashboardWpf\ApiDashboardWindowInteractiveTests.cs` (1758) — грепом + прочитаны блоки
  `1-145`, `920-1040`, `1738-1758` (RecordingGateway, BuildSuccessfulResponse, BuildPromptToTask*ScenarioResponse).
- Артефакты: `artifacts/smart-skills-semantic-quality-completion/{p33,p37,p38,p42}-*`,
  `artifacts/extension-runtime-token-quality-learnability/{phase-04/05/06 dashboard-qa-addition-result.json,
  docs-qa-dashboard-api-evidence-scope-refresh/*}`.
- Грепы: `liveDashboardRun:true` / `dashboard_runtime_called:true` по всем artifacts → **0 совпадений**;
  `Clipboard.` / `File.Write` / `SaveFileDialog` в проекте дашборда → **0**.

## Общая честная картина среза

Дашборд — это **внутреннее .NET WPF-окно** (`WinExe`, `net9.0-windows`), которое собирает
`XiigenExtensionRequest`, зовёт core-границу `XiigenExtensionApiService.ProcessHostExtensionRequest`
и рендерит `XiigenExtensionResponse` с честными boundary-флагами (`runtime_extension_claimed=false`,
`semantic_quality_claimed=false`, evidenceScope = «dashboard-to-core invocation only; not extension host
runtime proof»). Само по себе это добросовестный Path-A boundary-инструмент и он это **прямо декларирует**.

Реальная UX-автоматизация есть и она нетривиальная: `ApiDashboardWindowInteractiveTests` через
WPF AutomationPeer проверяет accessible-имена, tooltips, отсутствие jargon на primary-path,
неперекрывающиеся колонки, min-размеры, wrap. Это закрывает часть AR-22 (UX-слой) лучше, чем у Claude (P12).

НО «route-tied QA», которую пакет/план числит как доказательство (P01 phase-3), при проверке кода/артефактов
оказывается **не выполнением маршрута, а тремя parse/mock-слоями**, ни один из которых не гоняет реальный
prompt через реальный `XiigenExtensionApiService` с проверкой корректности ответа:

1. **WPF-тесты** инъектируют фейковый `RecordingGateway`, отдающий рукописные `XiigenExtensionResponse`
   (P04 уже отметил фейк-gateway верхнеуровнево; здесь — точные synthetic-builder'ы и следствия).
2. **`ApiDashboardFixtureRunner`** — по имени «runner», по сути parse-only **guard авторизации**:
   всегда `EvalExecuted=false/TrainingExecuted=false`, `NextAction`=«P40 stops before eval/model execution»,
   собственный self-recorded `P37-GAP-02`.
3. **`ApiDashboardSourceDisjointGuard`** — parse-only pre-eval guard, жёстко `CanProceedToEvalRunner=false`,
   любую строку с `eval_execution_allowed`/`not_executed_yet=false` помечает как «unsafe execution flag».
4. **Артефакты p33/p38/p42** — node/PowerShell парс-валидация JSONL и wording-firewall:
   `validationOnly:true`, `liveDashboardRun:false`, `dashboard_runtime_called:false`, `eval_run:false`.
   Грепом по всем artifacts **нет ни одного** `liveDashboardRun:true` / `dashboard_runtime_called:true`.

Итог среза: реальный live route-run дашборда через настоящий core на диске **не зафиксирован ни разу**;
существующая «route-tied QA» = parse-only + fake-gateway rendering + wording-guard. Плюс два продуктовых
дефекта самого дашборда (Copy/Export — in-memory заглушки без буфера/файла; shipped default-путь к реальному
сервису не покрыт ни одним тестом) и один тест-инфраструктурный (WPF-QA требует STA + реальный desktop-Show,
не headless/CI-гейт).

## НОВЫЕ дыры (не дублируют P01 phase-3 / P02 AR-10/18/20/22 / P03 wrong-host / P04 fake-gateway верхнеуровнево)

### G29-1 — `ApiDashboardFixtureRunner` = parse-only guard, никогда не запускает eval
`ApiDashboardFixtureRunner.cs:38` `GuardEvidenceScope="dashboard fixture runner guard only; no model eval,
no training, no checkpoint mutation, no semantic-quality proof, no host runtime proof"`; `:215-219`
authorized-ветка возвращает `NextAction`=«Authorization metadata is accepted, but P40 still stops before
eval/model execution; a separate authorized eval slice must run any eval»; `BuildResult:235-238`
хардкод `TrainingExecuted:false, EvalExecuted:false, CheckpointMutated:false, SemanticQualityClaimed:false`;
`:39/:274` self-recorded `P37-GAP-02` (runner authorization gap). Семантик-качество «runner» дашборда —
проверяльщик манифеста авторизации, а не исполнитель маршрута/eval.
**Closure:** реализовать реальный eval-slice, который через дашборд/адаптер гоняет строки фикстур сквозь
`XiigenExtensionApiService` (или напрямую машину) и персистит per-row метрики; не выдавать guard за «runner».

### G29-2 — `ApiDashboardSourceDisjointGuard` структурно не может дойти до eval
`ApiDashboardSourceDisjointGuard.cs:27` `EvidenceScope="P41 parse-only source-disjoint pre-eval guard;
no model eval…"`; `BuildReport:133` хардкод `CanProceedToEvalRunner:false` **всегда**; `:176-178`
любая фикстура с `TrainingAllowed || EvalExecutionAllowed || !NotExecutedYet` заносится в problems как
«unsafe execution flag»; `:168-175` любой `evalRun/trainingRun/checkpointMutated/semanticQualityClaimed=true`
в манифесте → problem. Гейт спроектирован подтверждать, что НИЧЕГО не исполнено; он не является ступенью к
реальному source-disjoint прогону, лишь парс-сверкой фикстур vs манифест.
**Closure:** отделить «pre-eval parse gate» от настоящего source-disjoint eval-runner; последний должен
реально исполнить выбор/inference на held-out и вернуть accuracy, гейтя промоушен.

### G29-3 — Вся зафиксированная «route-tied dashboard QA» — parse-only/wording, ни одного live route-run
`p33-dashboard-api-validation-report.json` `validationOnly:true, liveDashboardRun:false, modelEvalRun:false,
scope:"artifact-level dashboard/API request-response design validation only"` (движок — node-скрипт
`p33-validate-dashboard-api-examples.js`, парсит JSONL); `p38-fixture-dashboard-qa-report.json`
`boundaries:{dashboard_runtime_called:false, extension_runtime_called:false, eval_run:false,
checkpoint_mutated:false}`, scope «P38 artifact-only fixture/dashboard QA automation»; `p42` = wording
overclaim-firewall (`p42-implementation-report.json` evidence_scope «dashboard documentation/source/
report-facing wording guard only»). Греп по всем artifacts: `liveDashboardRun:true`/`dashboard_runtime_called:true`
→ **0**. То есть route-tied QA из P01 phase-3 существует только как парс-валидация схем и guard-слова, а не
как исполнение маршрута дашборд→core с проверкой ответа. Конкретизирует P01 phase-3 доказательством, ЧТО
именно построено.
**Closure:** построить исполняемую route-tied QA: реальный прогон каждого метода каталога через
`XiigenExtensionApiService` из дашборда, снятие рендера/статусов, ассерты корректности; писать
`liveDashboardRun:true` с per-route evidence; парс-валидацию оставить вспомогательной.

### G29-4 — Per-phase «dashboard-qa-addition» отключается сам, когда меняется только ядро
`artifacts/extension-runtime-token-quality-learnability/phase-06-skillselection-trained-selector/
20260707-000750/dashboard-qa-addition-result.json`: `{"applicable":false, "reason":"SkillSelection core
train/eval/export behavior changed; API/dashboard behavior and UI surface were not changed in this bounded
slice.", "dashboardCodeChanged":false, "additionalScenarioAdded":false}` (тот же паттерн в phase-04/05).
Когда реально меняется поведение core-модели (обучение/eval/экспорт SkillSelection), дашборд-QA НЕ
перезапускается против нового поведения — сценарий не добавляется, потому что «UI-поверхность не менялась».
QA дашборда декуплена от реальных изменений маршрутов/ядра → не ловит регрессии core через дашборд.
**Closure:** привязать dashboard-QA к изменениям поведения core (а не только UI-кода): при изменении
train/eval/export любой машины перезапускать route-tied сценарии дашборда для затронутого метода.

### G29-5 — Copy/Export дашборда — in-memory заглушки (нет буфера обмена/файла), реального экспорта evidence нет
`ApiDashboardViewModel.cs:290-294` `CopyResultShell()` только `LastCopiedText=HumanResultText`;
`:296-302` `ExportHistoryShell()` только собирает строку в `LastExportText`; греп по проекту дашборда
`Clipboard.`/`File.Write`/`SaveFileDialog`/`StreamWriter` → **0**. Сами tooltip'ы это подтверждают
(тест `ApiDashboardWindowInteractiveTests.cs:97-98`: «does not claim clipboard automation» / «does not
claim file export automation»). Значит из дашборда нельзя реально скопировать результат или выгрузить
историю прогонов как evidence-файл — «Export history» не производит артефакт. Честно помечено, но
функционально это заглушки, а не действия.
**Closure:** реализовать реальный Clipboard.SetText и файловый экспорт истории (redaction-safe) с
сохранением артефакта; либо явно убрать кнопки из продуктовой поверхности как нереализованные.

### G29-6 — Shipped default-путь дашборд→реальный core не покрыт ни одним тестом; негативные статусы синтетические
`ApiDashboardViewModel()`→`new ApiDashboardRunner()`→`XiigenExtensionApiServiceGateway`→реальный
`XiigenExtensionApiService` (`ApiDashboardRunner.cs:16-27,49-52`) — это то, что реально едет в проде.
Но КАЖДЫЙ тест (`ApiDashboardWindowInteractiveTests.cs:27,146,195,290,356,467,559,647,693`) инъектирует
`new ApiDashboardRunner(gateway)` с фейковым `RecordingGateway(:1738-1758)`, отдающим рукописные ответы
(`BuildSuccessfulResponse:942`, `BuildPromptToTaskSemanticQualityScenarioResponse:968`,
`BuildPromptToTaskNotReadyScenarioResponse:1022`). Следствия, НЕ покрытые P04 (тот отметил лишь «фейк-gateway»):
(а) реальная связка дашборд→`XiigenExtensionApiService` не исполняется ни одним автотестом — shipped-wiring
не верифицирован; (б) «негативные» сценарии (checkpoint-mismatch, `Malformed OrchestrationReasoning`,
`Missing SkillSelection inventory`, `prompt_to_task_not_ready_degraded_dashboard_scenario`) — литеральные
константы в тесте, а не воспроизведённые из ядра коды отказа (напр. реальный
`ORCHESTRATION_REASONING_MANIFEST_CHECKPOINT_REF_MISMATCH`), значит regression на настоящие отказы ядра
отсутствует.
**Closure:** добавить интеграционные тесты на дефолтном `XiigenExtensionApiServiceGateway` (реальный сервис)
хотя бы для 5 методов каталога; негативные кейсы строить из реальных кодов отказа ядра, не из синтетических строк.

### G29-7 — WPF route-tied QA требует STA + реальный desktop `window.Show()`; не headless/кросс-платформенный CI-гейт
`ApiDashboardWindowInteractiveTests.cs:21` `StaTestTimeout=30s`, `:25` `RunOnStaThread(...)`, `:33`
`window.Show()`; `.csproj` `net9.0-windows`, `UseWPF=true`. Тесты создают и показывают настоящее WPF-окно на
STA-потоке → требуют интерактивной Windows-desktop-сессии; их нельзя прогнать в headless/Linux-CI-лейне, и
нет свидетельства, что они входят в баз. тест-гейт (≥1488). Единственная реальная route-tied QA-поверхность
дашборда не является воспроизводимым CI-гейтом. (Отдельно от P12 gap-6 — там non-repeatable Claude host-QA
через node.exe/кросс-репо; здесь WPF STA/desktop-Show ограничение.)
**Closure:** вынести логику в headless-тестируемый слой (ViewModel/presenter без Show), гонять на STA только
чистую UX-проверку; включить в явный CI-гейт с фиксацией, что окно-тесты требуют desktop-runner.

## Позитив (не дыры, для честности)
- Каталог дашборда покрывает все 5 контрактных методов (`ApiDashboardCatalog.cs:119-155`:
  PromptToTask/OrchestrationPlan/SkillSelection/Feedback/DebugTrace) — в отличие от Cursor-хоста (P14, feedback/
  debug-trace недостижимы).
- Boundary-флаги и evidenceScope честные и видимые в UI (`ApiDashboardResultPresenter.cs:115-132`).
- Реальная UX-автоматизация (accessible names/tooltips/no-jargon/layout) существует и проходит — часть AR-22.
