# opus-pass-04 — Реальные API-поверхности (P04 из 50)

Честный фрейм прохода. Здесь я оцениваю НЕ самоотчёт пакета, а фактический код/артефакты в реальных
репозиториях. Задача P04 — найти реальные API/plugin-поверхности: `ProjectWikiPluginOrchestrationAdapter`,
API-дашборд (`Tests.ApiDashboardWpf`, `43.SmartSkillsDomain.Dashboard`), `queryWiki`, `xiigen-platform` API —
и определить, это настоящий код или только план. Все статусы привязаны к file:line или к явному «не найдено (искал X)».

Дедуп: P03 уже зафиксировал по домену `43.SmartSkillsDomain` (`XiigenExtensionApiService`): транспорта нет,
host-дифференциации нет, `runtime_extension_claimed` захардкожен в false, единственный вызыватель — WPF-дашборд.
P04 смотрит ДРУГИЕ поверхности (домен `42.ProjectWikiDomain` + `43.LearningLocalWikiData` + внешние репозитории),
поэтому находки новые.

## Что реально проверено (инструменты, bounded)

1. `42.ProjectWikiDomain/ProjectWikiPluginOrchestrationAdapter.cs` (293 строки) — прочитан целиком.
2. `Tests.ApiDashboardWpf/ApiDashboardWindowInteractiveTests.cs` (1758 строк) — прочитаны строки 1-960 (структура тестов + фабрики ответов).
3. `43.LearningLocalWikiData/tools/WikiContextHook/Program.cs` — прочитан целиком (CLI-хук).
4. `43.LearningLocalWikiData/tools/ProjectWikiTrainingRunner/Program.cs:700-790` — секция VS Code runtime-probe с `queryWiki`.
5. `43.LearningLocalWikiData/reports/PLUGIN-RUNTIME-INTEGRATION-GATE.json` — прочитан целиком.
6. Внешние репозитории проверены на существование: `xiigen_visual_studio`, `xiigen_visual_code`, `askapediaweb` — ВСЕ существуют; `xiigen_claude`/`xiigen_cursor`/`xiigen_codex` — НЕ существуют.
7. `xiigen-platform-dot-net-angular` / `xiigen-flows-dot-net-angular` — grep XiigenExtension/queryWiki по `*.cs`/`*.ts` → 0 (только docs/skills/plans).
8. Контрактный дрейф: `IWikiOrchestrator`/`_42_ProjectWikiDomain.Contracts` в net461 VSIX vs net9.0 core.

## Ключевой вывод P04 (человеческим языком)

Существует НАСТОЯЩИЙ, собранный XIIGen-продукт расширений — но для ДРУГИХ хостов, чем целевые три.
`PLUGIN-RUNTIME-INTEGRATION-GATE.json` показывает два реальных собранных плагина:
- **Visual Studio**: `Xiigen.VisualStudio.SmartCodeWiki.vsix` (2.85 МБ) + `_42.ProjectWikiDomain.dll` (net461),
  контракты `_42_ProjectWikiDomain.Contracts.IWikiOrchestrator / WikiAnswer / WikiReadinessStatus`.
- **VS Code**: `out/hookRunner.js` экспортирует `checkReadiness`, `getMap`, `queryWiki`, `resetBinaryCache` + `package.json`.
Оба читают readiness из XIIGen-runtime-отчёта для целевого репо `C:/Projects/askapediaweb`.

То есть `queryWiki` — это метод VS Code плагина в ВНЕШНЕМ репозитории `xiigen_visual_code`, а не C#-API в `llm_mvp_core`.
В самом `llm_mvp_core` от этого транспорта есть только (а) Node-проба, которая грузит внешний `hookRunner.js`
против ПОДДЕЛЬНОГО `vscode`-стаба, и (б) CLI-хук `WikiContextHook`, который к тому же ходит по BOOTSTRAP_FALLBACK.

Целевые расширения Claude/Cursor/Codex — host-артефактов НОЛЬ (нет `xiigen_claude`/`xiigen_cursor`/`xiigen_codex`).
P03 искал только внутри `llm_mvp_core` и сделал вывод «host-кода нет»; на самом деле реальный host-код есть, но
для Visual Studio + VS Code, то есть для НЕ ТЕХ хостов. Это самое важное уточнение картины: продукт-расширение
доказан для одной линейки хостов и полностью отсутствует для целевой тройки.

Дашборд `Tests.ApiDashboardWpf` — это НАСТОЯЩАЯ WPF UI-автоматизация (STA-поток, драйв реальных контролов через
AutomationPeer, проверка AutomationProperties-имён/подсказок, обход фокуса, непересекающиеся колонки). Но все тесты
гоняют ПОДДЕЛЬНЫЙ `RecordingGateway`, возвращающий синтетический «Mapped WPF dashboard response.», а не реальный
`XiigenExtensionApiService`. И сами тесты явно утверждают `runtime_extension_claimed=false` и
`semantic_quality_claimed=false`. Значит UI-обвязка доказана, но интеграция с ядром и семантика — нет.

## Новые дыры (не дублируют P01-P03)

| # | Component | Real-status | Evidence file:line | Gap | Closure steps |
|---|---|---|---|---|---|
| 1 | Реальные собранные host-плагины = Visual Studio + VS Code (askapediaweb), НЕ Claude/Cursor/Codex | BUILT (внешне) / WRONG-TARGET | `PLUGIN-RUNTIME-INTEGRATION-GATE.json:14-56`; `/c/Projects/xiigen_visual_studio/.../Xiigen.VisualStudio.SmartCodeWiki.vsix` есть; `/c/Projects/xiigen_visual_code/out/hookRunner.js` есть; `xiigen_claude/cursor/codex` → No such file | Есть доказанный XIIGen extension-продукт, но для VS+VS Code над `askapediaweb`. Целевая тройка Claude/Cursor/Codex — 0 host-артефактов. P03 (искал только в `llm_mvp_core`) пропустил внешние host-репозитории. | Решить: портировать проверенный VS/VS Code-паттерн (readiness-report + транспорт) на Claude/Cursor/Codex; собрать 3 целевых host-пакета; НЕ засчитывать VS/VS Code-плагины как покрытие целевой тройки. |
| 2 | `queryWiki`/`hookRunner.js` транспорт — внешний (`xiigen_visual_code`); в repo только MOCK `vscode`-стаб + проба | EXTERNAL / in-repo MOCK-only | `ProjectWikiTrainingRunner/Program.cs:704-772` (грузит внешний `out/hookRunner.js` против стаба); `43.LearningLocalWikiData/artifacts/plugin-runtime-node-stub/node_modules/vscode/index.js` (поддельный `workspace`/`window`/`Uri`); `git ls-files | grep hookRunner` → пусто | Транспорт `queryWiki`, который CLAUDE.md называет host-транспортом, физически вне `llm_mvp_core`; в core — только Node-проба + сфабрикованный `vscode`-стаб. Core не может пересобрать/поставить транспорт. | Внести транспорт (или эквивалент stdio/CLI) в версионируемый пакет, принадлежащий `llm_mvp_core`, ИЛИ формально задокументировать внешний репо как поставляемый host с закреплённым билдом; заменить mock-`vscode`, используемый в пробе, на реальную загрузку. |
| 3 | `WikiContextHook` CLI использует BOOTSTRAP_FALLBACK (оркестратор не внедрён) | BUILT_NOT_PROVEN / degraded path | `WikiContextHook/Program.cs:12` `new ProjectWikiPluginOrchestrationAdapter()` без оркестратора → `ProjectWikiPluginOrchestrationAdapter.cs:68-69` orchestrator=null → `:143` BOOTSTRAP_FALLBACK; `Program.cs:24-34` `--ensure-ready` возвращает захардкоженный `isReady=true` | Единственный in-repo CLI-транспорт строит адаптер без `ILogicalOrchestrator`, поэтому всегда идёт по прямому `SubmitUserTaskApi` (сам код помечает это warning «should not be active after TD2»). Оркестрированный `project-wiki-orchestration-flow` через CLI недостижим; `--ensure-ready` — фикция. | Внедрить реальный `ILogicalOrchestrator` в адаптер CLI; доказать оркестрированный путь end-to-end из CLI; заменить захардкоженный `--ensure-ready` на реальный расчёт готовности. |
| 4 | `ProjectWikiPluginOrchestrationAdapter` имеет поле `PluginHost`, но нигде на хост не ветвится | STUB host-differentiation | `ProjectWikiPluginOrchestrationAdapter.cs:17` поле `PluginHost` в request; в файле оно НИГДЕ не читается (используются PromptText/ActiveRepoPath/LinkedRepoPaths/UserFacingEntrypoint); `:128` UserFacingEntrypoint в metadata без host-ветвления | Адаптер wiki-плагина принимает идентификатор `PluginHost`, но игнорирует его — поведение одинаково для любого хоста. Дублирует класс дефекта P03 (SmartSkills), но для ОТДЕЛЬНОГО адаптера, реально подключённого к flow-оркестратору. Один generic-путь выдаётся за мульти-host. | Либо убрать `PluginHost` (честный одиночный путь), либо реализовать host-специфичную композицию; задокументировать, что host-дифференциация в wiki-пути не реализована. |
| 5 | Plugin runtime gate доказывает ЗАГРУЗКУ readiness-отчёта (entryCount 0), а не семантические wiki-ответы; отчёт устарел | LOAD-PROBE ONLY / STALE | `PLUGIN-RUNTIME-INTEGRATION-GATE.json:52` readiness `entryCount:0`, «Readiness loaded from XIIGen runtime report»; `:62` `passed:true`; `:6` generated 2026-05-25; LIVE-gate тоже 2026-05-25 vs сегодня 2026-07-08 | `passed:true`-gate доказывает, что два внешних плагина умеют ОТКРЫТЬ артефакты и прочитать readiness-JSON (entryCount 0, ноль реальных wiki-записей/ответов). По completion-evidence pattern «load metadata без выполненного семантического ответа != host readiness». Отчёт 6+ недель несвежий. | Перезапустить с entryCount>0 и реальным round-trip prompt→answer через хост; ассертить настоящий wiki-ответ, а не только загрузку readiness; обновить gate. |
| 6 | `Tests.ApiDashboardWpf` UI-автоматизация гоняет ПОДДЕЛЬНЫЙ `RecordingGateway`, не реальный core | BUILT (UI-обвязка) / NOT core-integrated | `ApiDashboardWindowInteractiveTests.cs:27` `new RecordingGateway(request => BuildSuccessfulResponse(request))`; `:942-960` BuildSuccessfulResponse возвращает синтетический ответ; `:251` ассерт `runtime_extension_claimed`=false; `:337` `semantic_quality_claimed`=false | Тесты AR-18/AR-22 существуют как реальный код (это уточняет P02 «UNMET GATE» — код-то ЕСТЬ, BUILT), но драйвят фейковый gateway с заготовленными ответами; доказывают WPF-обвязку и UX-текст, а не интеграцию с ядром/семантику; и сами утверждают, что host-runtime/semantic-quality НЕ заявляются. Прогон (pass/fail) не доказан. | Добавить вариант dashboard-теста, драйвящий реальный `ApiDashboardRunner`→`XiigenExtensionApiService` (не RecordingGateway); прогнать suite, зафиксировать pass/fail; fake-gateway-тесты оставить как UI-only. |
| 7 | `xiigen-platform` / `xiigen-flows` ссылаются на XIIGen только через copied docs/skills/plans; кода-интеграции API — ноль | COPIED_DOC_ONLY | `xiigen-platform-dot-net-angular` `git grep -E "XiigenExtension|ProcessHostExtensionRequest|queryWiki" -- *.cs *.ts` → 0; совпадения только `docs/skills/*.md`, `docs/plans/*`; аналогично flows | Названный в P04 «xiigen-platform API» не содержит кода, который вызывает XIIGen extension-границу; только зеркалированные skill/plan-доки. Платформа не является API-потребителем extension-ядра. | Если интеграция платформы в scope — добавить реальный service/API-код, вызывающий extension-границу; иначе явно пометить платформу вне scope для три-extension deliverable. |
| 8 | VS-VSIX поставляет ДИВЕРГЕНТНЫЙ net461-контракт (`_42_ProjectWikiDomain.Contracts.IWikiOrchestrator`), которого нет в текущем net9.0-core | STALE PORT / DIVERGENT | `PLUGIN-RUNTIME-INTEGRATION-GATE.json:20-24` символы `_42_ProjectWikiDomain.Contracts.IWikiOrchestrator/WikiAnswer/WikiReadinessStatus` в net461-dll; `42.ProjectWikiDomain.csproj` TargetFramework `net9.0`; namespace `ProjectWikiDomain`; grep `_42_ProjectWikiDomain.Contracts` в core → 0; `interface IWikiOrchestrator` в core → нет | Поставленный VS-плагин встраивает отдельный net461-порт (`_42.ProjectWikiDomain.dll`, `IWikiOrchestrator`), не соответствующий никакому текущему .NET 9 контракту в `llm_mvp_core`. Две дивергентные контракт-поверхности; поставленная — устаревший форк. Изменения ядра не доходят до плагина. | Согласовать runtime-библиотеку VS-плагина с текущим net9.0-контрактом core (или версионированным shared-contract-пакетом); задокументировать mapping framework/namespace; пересобрать VSIX из текущего core. |

## Примечание о зрелости

- Настоящие BUILT-артефакты (VSIX, hookRunner.js) существуют — но (а) для НЕ тех хостов и (б) доказаны только на уровне load-probe (entryCount 0), а не семантического ответа.
- `ProjectWikiPluginOrchestrationAdapter` — настоящий in-process класс (BUILT), корректно ветвит orchestrator/fallback, но host-поле игнорирует и в CLI никогда не получает оркестратор.
- Дашборд-UI-автоматизация — настоящий код (BUILT), но против фейкового gateway (NOT core-integrated, NOT PROVEN).
- Целевые Claude/Cursor/Codex — по-прежнему MISSING на уровне host-артефактов (подтверждает P01/P03, добавляя, что аналог для VS/VS Code уже собран).
