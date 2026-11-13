# opus-pass-03 — Реальный host/extension код (P03 из 50)

Дата: 2026-07-08. Ассессор: Opus 4.8.

Фокус прохода: найти в НАСТОЯЩИХ репозиториях реальный host-код трёх расширений
(Claude, Cursor, Codex) — VS Code extension, Visual Studio, chat participant,
command, MCP-сервер, Codex CLI/desktop — и отделить то, что реально существует
как код, от того, что только запланировано/скопировано. Оценка ведётся от кода в
`C:\Projects\llm_mvp_core`, а не от самоотчёта пакета `extension development`.

Дедуп-граница: P01 уже занёс фазовую дыру «Host entrypoints (Claude/Cursor/Codex
shells) MISSING». P02 уже занёс «thin-adapter no-hidden-determinism gate MISSING»
и «pipeline не собран в 15.OrchestratorModel», а также что PromptToTask/
OrchestrationReasoning — стабы. В P03 добавлены только НОВЫЕ, более гранулярные
host-code дыры с конкретными file:line, которых нет в реестре.

## 1. Что и как проверено (bounded, tracked-only)

1. `git ls-files "*.ts" "*.tsx" "*package.json" "*.vsix" "*.vsixmanifest"
   "*.vsct" "*extension.js"` (без node_modules) →
   - `.ts`/`.tsx`: 0.
   - `.vsix`/`.vsixmanifest`/`.vsct`: 0.
   - `extension.js`: 0.
   - `package.json`: только 4 файла, и ВСЕ — это model-asset backup-пакеты, не
     extension-манифесты:
     `artifacts/adaptive-learning/AD-06-user-adaptive-backup-package.json`,
     `artifacts/adaptive-learning/OR-AD-03-orchestration-reasoning-backup-package.json`,
     `artifacts/adaptive-learning/PTT-AD-03-prompt-to-task-backup-package.json`,
     `training-data/.../q6-05-redacted-adaptive-export-package.json`.
   → Вывод: НИ ОДНОГО VS Code / Cursor extension-манифеста в трекнутом репо.

2. `git grep -E "class .*(Claude|Cursor|Codex)(Adapter|Host|Extension|Bridge|
   Plugin|Entry|Command|Participant)" -- '*.cs'` → 0 совпадений.
   → Нет host-adapter-классов для трёх расширений.

3. grep `vscode|VSCode|chatParticipant|registerCommand|activate\(|
   vscode\.commands|ExtensionContext|contributes` по репо → совпадения ТОЛЬКО в:
   - `docs\skills\*` (skill-документы: quality-improvement-cycle,
     learning-signal-capture, ai-model-selection);
   - `docs\plans\evidence\extension-full-readiness-dual-path\*` — это захваченные
     логи РЕАЛЬНОГО Cursor (`cursor-user-data-cmd\logs\...`, `exthost.log`,
     `renderer.log`, `Cursor Structured Logs.log`) и один драйвер-скрипт
     `row12-host-visible-command-runner.test.js`.
   → В ИСХОДНИКАХ (`*.ts`/`*.cs` продукта) этих API нет вообще. Совпадения — это
     доки и артефакты запуска чужого приложения, а не host-код продукта.

4. Прочитан единственный «extension-border» файл продукта:
   `43.SmartSkillsDomain\XiigenExtensionApiService.cs` (1930 строк) и его контракт
   `43.SmartSkillsDomain\XiigenExtensionApiContracts.cs`.
   - Это чистая in-process C#-библиотека: публичный метод
     `ProcessHostExtensionRequest(XiigenExtensionRequest) ->
     OperationResult<XiigenExtensionResponse>` (`:88`).
   - Статус ответа всегда `PartialBoundaryStatus =
     "core_boundary_partial_no_semantic_quality_claim"` (`:11`, устанавливается
     `:178`). Успешные и ошибочные ветки всегда пишут
     `runtime_extension_claimed = "false"` (`:216`, `:257`, `:560`).
   - Контракт-коммент `:3`: «Request sent by thin Claude/Cursor/Codex host clients
     to xiigen exe.» — то есть подразумевается некий «xiigen exe», к которому
     обращаются тонкие host-клиенты.

5. Кто реально вызывает `ProcessHostExtensionRequest` (`git grep`):
   - `43.SmartSkillsDomain.Dashboard\ApiDashboardRunner.cs:26` и `:139` — это
     WPF-дашборд (`43.SmartSkillsDomain.Dashboard.csproj:17 <OutputType>WinExe`);
   - множество xUnit-тестов (`Tests\Extension*Tests.cs`,
     `Tests\ApiDashboard*Tests.cs`, `Tests.ApiDashboardWpf\...`).
   → НЕ существует CLI/stdio/JSON-RPC/HTTP/MCP entrypoint, вызывающего сервис. Ни
     один из трёх target-host (Claude/Cursor/Codex) не является вызывателем.

6. Host-специфичность: `HostName`/`HostType` (`XiigenExtensionApiContracts.cs:5-6`)
   валидируются ТОЛЬКО на непустоту (`:102` `host_name_required`, `:107`
   `host_type_required`) и эхо-копируются в debug/refs. `git grep`
   `"(claude|cursor|codex)"` (строковые литералы) по `43.SmartSkillsDomain/*.cs`
   → 0. Ветвления по host НЕТ.

7. MCP: единственный MCP-код в репо — `23.OssEquivalents\Mcp\McpToolDefinition.cs`
   и `23.OssEquivalents\Mcp\McpTransport.cs`. `git grep`
   `"XiigenExtension|SmartSkillsDomain|ProcessHostExtensionRequest"` по
   `23.OssEquivalents/**/*.cs` → 0. То есть MCP-модуль НЕ является мостом для
   extension API (это отдельный OSS-equivalents эксперимент).

8. Host-HOWTO в пакете (`supporting-files\`): `CLAUDE-XIIGEN-HOWTO-ru.md`,
   `CURSOR-XIIGEN-HOWTO-ru.md`, `CODEX-XIIGEN-HOWTO-ru.md`. Сэмплированы Claude и
   Cursor: оба прямо заявляют «intended path» / «documentation coverage only» /
   «Это не proof packet»; таблицы статуса: token pairs 0/9, quality rows 0/9,
   trainable DPO rows 0/9, lifecycle rows 0/9, final reviews 0/10. Описанный
   «adapter» вызывает `XiigenExtensionApiService.ProcessHostExtensionRequest`
   напрямую (шаг 4 в обоих), но самого adapter-кода в репо нет.

## 2. Итог реального состояния host-слоя

- Реального host/extension-КОДА для Claude, Cursor, Codex нет ни на одном уровне
  (нет манифестов, нет entrypoint, нет adapter-классов, нет транспорта).
- Есть ОДИН host-агностичный core-boundary `XiigenExtensionApiService`, который:
  - живёт только in-process;
  - вызывается только из WPF-дашборда (внутренний диагностический WinExe) и тестов;
  - никогда не заявляет runtime/семантику (`Status` фиксирован в partial,
    `runtime_extension_claimed=false` зашит).
- «Три расширения» в коде неотличимы друг от друга: `HostType` — просто строка.

Это соответствует честному блоку пакета («No product runtime») и опровергает
любую трактовку, будто host-адаптеры «BUILT_NOT_PROVEN»: на host-уровне это
COPIED_DOC_ONLY (HOWTO) + MISSING (код). BUILT_NOT_PROVEN относится только к
core-boundary библиотеке, не к host-слою.

## 3. НОВЫЕ дыры P03 (6)

| # | Component | Real-status | Evidence file:line | Gap |
|---|---|---|---|---|
| 1 | Host↔core process transport («xiigen exe») | MISSING | contracts `:3`; service `:88`; callers `ApiDashboardRunner.cs:26/:139` (WinExe)+tests; нет stdio/JsonRpc/HttpListener/WebApplication/MapPost в 43-домене | «xiigen exe» из контракта не существует; нет процессного транспорта, реальный host не может вызвать core через границу процесса. |
| 2 | Per-host code differentiation | MISSING | contracts `:5-6`, валидация `:102`/`:107`; grep литералов host в 43 → 0 | `HostType` нигде не ветвится; host-специфичного кода (Claude MCP vs Cursor VS Code API vs Codex CLI) нет — один generic boundary выдаётся за три расширения. |
| 3 | Host-runtime phase (`runtime_extension_claimed`) | MISSING (нет code-path) | service `:216/:257/:560` = false безусловно; `:11`/`:178` status; boundary-rule contracts `:93` | Флаг зашит в false; нет пути кода, который мог бы выставить true. Фаза host-runtime не реализована, а не просто не доказана (строго сильнее BUILT_NOT_PROVEN). |
| 4 | Демонстрируемый caller = внутренний WPF-дашборд | WRONG-HOST | `ApiDashboardRunner.cs:26/:139`; `...Dashboard.csproj:17 WinExe` | Единственный не-тестовый вызыватель — .NET WPF-окно, не Claude/Cursor/Codex; по AR-10 dashboard→core ≠ host UI. Не засчитывается как host-entrypoint. |
| 5 | Cursor extension package (row12 = driver) | MISSING (только driver) | `docs\plans\evidence\...\row12-host-visible-command-runner.test.js`; captured `cursor-user-data-cmd\logs\...`; `git ls-files` extension-манифестов → 0 | Cursor-«evidence» — внешний UI-driver `.test.js` + логи запуска реального Cursor, а не поставляемое VS Code/Cursor extension-расширение (нет `package.json contributes`, `extension.ts`, `activate()`). |
| 6 | Host adapter HOWTOs vs code | COPIED_DOC_ONLY | `supporting-files\CLAUDE-XIIGEN-HOWTO-ru.md`, `CURSOR-...`, `CODEX-...` (все «intended path / not a proof packet»; треки 0/9…0/10) | Три HOWTO описывают только intended-path без единого поставленного host-клиента и сами признают, что это не proof packet. Doc-vs-code разрыв именно на host-адаптерах. |

## 4. Что нужно для закрытия host-слоя (в итоговый план)

1. Core-side транспорт: отдельный CLI/stdio JSON-сервер (в собственном namespace,
   не в дашборде), сериализующий `XiigenExtensionRequest/Response`; контрактные
   тесты round-trip (Path A boundary).
2. Три РАЗДЕЛЬНЫХ host-клиента, каждый — тонкий adapter, только строит request /
   вызывает транспорт / показывает `ChatDisplayText`+warnings+errors+debug:
   - Claude: MCP-сервер или Claude plugin/skill hook;
   - Cursor/VS Code: extension-пакет (`package.json` с `contributes`+`engines.vscode`,
     `extension.ts` с `activate()`, command + chat participant);
   - Codex: CLI/desktop route.
3. Architecture-review каждого adapter на отсутствие скрытой семантики (grep
   scorer/lookup/regex/model — покрывает P02 thin-adapter gate).
4. Host-runtime фаза: путь кода, выставляющий `runtime_extension_claimed=true`
   только при доказанном реальном host round-trip.
5. Dual-path proof на КАЖДЫЙ host: Path A (boundary через транспорт) + Path B
   (локальный desktop-запуск реального host с маркером/скриншотом/логом).
6. Только после этого — token/quality/learnability/lifecycle треки (0/9…0/10 →
   заполнить).

## 5. Честные оговорки этого прохода

- Я НЕ запускал сборку/тесты/runtime и не проверял платформенные репозитории
  `xiigen-platform-dot-net-angular` / `xiigen-flows-dot-net-angular` (в фокус P03
  не входили; grep вёлся по `llm_mvp_core`). Если host-код лежит там — это
  отдельная проверка следующим проходом; но контракт и HOWTO указывают, что host
  вызывает `XiigenExtensionApiService` в `llm_mvp_core`, а не отдельный
  платформенный сервис.
- Все статусы выше подтверждены `git ls-files`/`git grep`/чтением файлов;
  «0/пусто» означает «not found (searched: указанный git-паттерн/путь)».
- `Extension development session.md` (8.9 МБ) НЕ читался целиком по правилу пакета.
- Существующий `opus-pass-03-real-host-code.md` (timestamp 16:50, старше мастера
  17:10 и не отражён в реестре) перезаписан этим свежим файлом.
