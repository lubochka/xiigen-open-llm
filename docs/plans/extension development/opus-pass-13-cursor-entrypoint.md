# Pass P13 — Cursor: точка входа / настройки / core-API (реальное состояние)

Дата прохода: 2026-07-08. Оценщик: Opus 4.8 (REAL-STATE ASSESSOR). Слайс: РЕАЛЬНЫЙ
Cursor host-репозиторий `C:\Projects\cursor-xiigen-smart-skills` по разрезу
«entrypoint + settings/debug + core-API path» — аналог того, что P09 сделал для Claude.

## Что реально прочитано (bounded)

- `package.json` (5215 б, целиком) — манифест VS Code/Cursor extension.
- `src/extension.ts` (54868 б, **прочитан целиком**, 1346 строк) — единственный источник entrypoint.
- `dist/extension.js` (48148 б) — grep на строки транспорта.
- `xiigen-extension-contract.json` (целиком) — статический boundary-контракт хоста.
- `docs/PROJECT-DOC.md` — grep самопризнаний.
- `phase40-cursor-entrypoint-response-1.json`, `phase40-cursor-core-trace-1.json` — образцы runtime-evidence.
- Листинги `reports/`, `scripts/`, git-состояние, поиск build-конфигов (`tsconfig.json`/`esbuild.js`/`.vscodeignore` → отсутствуют).

## Общая картина (честно)

Cursor — самое зрелое из трёх расширений: **настоящий** VS Code extension package с
`activate()`, тремя контрибьютнутыми командами (`xiigen.smartSkills.select`,
`xiigenSmartSkills.panel`, `xiigenSmartSkills.runtimeProof.invoke`), chat participant
`xiigen-smart-skills`, реальным webview-панелью (textarea + Submit + postMessage round-trip),
секцией `configuration` из 11 свойств, и компилированным `dist/extension.js`. Это выше
Claude (только SessionStart-hook, P09) и Codex (рукописный `.js` без сборки, P08).

НО по разрезу entrypoint/settings/core-API вскрыты 8 НОВЫХ структурных дыр, которых нет
в реестре P01–P12. Они не про «host отсутствует» (P01, устарело), не про «transport =
absolute-path shim» (P08) и не про Claude (P09–P12). Каждая привязана к file:line живого
кода Cursor.

Сам хост это признаёт: `package.json` → `version:"0.0.0-xiigen-wiring"`,
`runtimeEvidence:false`, `fullHostImplementationEvidence:false`;
`xiigen-extension-contract.json` → `runtime_host_proof_performed:false`,
`host_prompt_to_task_implementation:false`, «This contract is static package/boundary
evidence only. It does not prove interactive Cursor runtime behavior.»

## НОВЫЕ дыры (детально)

### G1. Транспорт Windows-only: жёстко `spawn("powershell.exe", …)`
`src/extension.ts:661` `spawn("powershell.exe", ["-NoProfile","-ExecutionPolicy","Bypass","-File",coreApiScript, …])`.
`dist/extension.js` содержит `powershell.exe` (grep=1). Cursor кроссплатформенный
(Windows/macOS/Linux) — на macOS/Linux `powershell.exe` не спавнится, вся core-граница
недостижима даже при корректном пути. Отдельно от P08 (absolute-path shim к core-репо):
там проблема «путь на чужую машину», здесь — «даже с верным путём не-Windows Cursor не
может вызвать core». → **STUB TRANSPORT / WINDOWS-ONLY**.
Закрытие: заменить powershell-спавн на кроссплатформенный транспорт (Node child, stdio
JSON-RPC, HTTP), доказать вызов из Cursor на не-Windows; убрать `powershell.exe` из
runtime-пути.

### G2. Поставляемые дефолты настроек = абсолютные пути dev-машины
`package.json:49-58` `contributes.configuration.properties`:
`xiigenSmartSkills.coreRoot.default = "C:\\Projects\\llm_mvp_core"`,
`xiigenSmartSkills.coreApiScript.default = "C:\\Projects\\llm_mvp_core\\tools\\xiigen-extension-core-api.ps1"`.
Каждая установка расширения приезжает с дефолтом на конкретную dev-машину; нет discovery,
нет бандла core, нет относительных путей. `extension.ts:313-314` читает эти дефолты.
Отдельно от P08 (шим в коде) — здесь дефект в СХЕМЕ настроек манифеста. → **NON-SHIPPABLE DEFAULTS**.
Закрытие: убрать абсолютные дефолты, ввести discovery/бандл core или обязательную
пользовательскую настройку с валидацией; расширение не должно предполагать `C:\Projects\...`.

### G3. Дрейф схемы настроек: 3 ключа читаются, но не объявлены; 2 объявлены, но не читаются
Читаются кодом, но ОТСУТСТВУЮТ в `contributes.configuration.properties`:
`locatorCandidateId` (`extension.ts:500`), `targetCandidateId` (`:503`),
`rollbackCandidateId` (`:506`) — их нельзя задать через Settings UI (только env/JSON).
Объявлены в манифесте, но НИКОГДА не читаются `extension.ts`:
`xiigenSmartSkills.promptSuite` (`package.json:59`), `xiigenSmartSkills.traceReport`
(`package.json:64`) — мёртвые настройки. → **SETTINGS-SCHEMA DRIFT**.
Закрытие: объявить недостающие 3 ключа в схеме; удалить/подключить 2 мёртвых; добавить
тест паритета «config.get ⇔ contributes.configuration».

### G4. Тестовая harness-команда поставлена в продуктовый манифест + палитру
`package.json:34-38` `contributes.commands` содержит `xiigenSmartSkills.runtimeProof.invoke`
с title `"XIIGen: Runtime Proof Harness (Test Only)"` и `activationEvents:onCommand:…`
(`:18`). Конечный пользователь Cursor видит тест-харнес в Command Palette.
Плюс `extension.ts:180` регистрирует `xiigenSmartSkills.__test.chatHandlerLevel` в
`activate()`, но её НЕТ в манифесте — скрытая runtime-команда (self-flag
`fullChatUiEvidence:false`, `testOnlyCommandAdapter:true`). → **PRODUCTIZATION DEFECT**.
Закрытие: убрать test-only команды из продуктового активационного пути (dev-only сборка/флаг);
не экспонировать harness пользователю; синхронизировать зарегистрированные vs объявленные команды.

### G5. Chat participant объявлен, но регистрация — на proposed/unstable API и в Cursor не доказана
`package.json:40-45` `contributes.chatParticipants` объявляет `xiigen-smart-skills`, НО
`extension.ts:162-177` кастует `vscode as unknown` до `chat?.createChatParticipant` и
молча пропускает регистрацию, если API нет; манифест не объявляет `enabledApiProposals`.
`docs/PROJECT-DOC.md:50` «Если `vscode.chat.createChatParticipant` доступен …», `:600`
«Full interactive Cursor chat participant invocation remains distinct from handler-level and
discovery evidence»; QA `HOST-08C` — только discovery, явно запрещает подменять его
command/panel-доказательствами. → **UNSTABLE-API / UNPROVEN CHAT SURFACE**.
Закрытие: закрепить стабильный chat API (или объявить proposal + pin engine), доказать
реальную инвокацию chat participant в самом Cursor (не handler-level), снять discovery→invocation evidence.

### G6. Видимый «ответ» на success-пути — фабрикованный хостом шаблон, семантики нет по построению
`extension.ts:854-856` fallback concreteAnswer = «XIIGen Cursor `<type>` entrypoint invoked
llm_mvp_core core API; semantic quality is not claimed.»; `:879` `normalizeResponse`
безусловно ставит `semanticQualityClaimed:false`, `:872-873` `trueHostRuntimeEvidence:false`,
`:874` `runtimeEvidence: passed` (только adapter-functionality). Команда `xiigen.smartSkills.select`
показывает это через `vscode.window.showInformationMessage` (`:143`) — тост со статус-строкой,
а не семантический ответ модели. Даже «passed» прогон никогда не показывает семантику.
Отдельно от P10 (dead-gate внутри core-сервиса) — здесь выходной слой Cursor-хоста. → **BY-CONSTRUCTION NO SEMANTIC ANSWER (host layer)**.
Закрытие: после реального обучаемого inference в core — прокинуть настоящий ответ в видимый
текст; убрать безусловный `semanticQualityClaimed:false` из host-нормализации на success-пути.

### G7. `dist/extension.js` невоспроизводим из репозитория
Нет `tsconfig.json`, нет бандлер-конфига (`esbuild.js`), нет `.vscodeignore`, нет
`@types/vscode`; `package.json:150-151` `dependencies:{}`,`devDependencies:{}`; `scripts`
(`:106-109`) = только `install-smoke` + `verify-docs-final-coverage-nonhost` (нет
`compile`/`build`/`watch`). Компилированный 48 КБ-артефакт собран внешним/глобальным
тулчейном; чистый checkout пересобрать `dist` не может. Отдельно от P08 («cursor —
единственный со сборкой»): здесь именно НЕВОСПРОИЗВОДИМОСТЬ. → **NON-REPRODUCIBLE BUILD**.
Закрытие: добавить `tsconfig.json` + бандлер (esbuild/tsc) + `@types/vscode` в devDependencies
+ `compile`/`build` npm-скрипт; доказать `npm ci && npm run build` даёт текущий `dist` из чистого клона.

### G8. Дивергентная Documents-копия; committed phase40-evidence снят из неё и против ДРУГОГО скрипта
`phase40-cursor-core-trace-1.json`: `requestPath/responsePath/tracePath` =
`C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\…` (не репо
под ревью `C:\Projects\…`); `scriptPath` = `C:\Projects\llm_mvp_core\tools\`**`phase40-extension-core-api.ps1`** —
ДРУГОЙ скрипт, чем поставляемый в конфиге extension (`xiigen-extension-core-api.ps1`,
`package.json:56`). `phase40-cursor-entrypoint-response-1.json`: `hostRuntimeInvoked:false`,
`fullHostRuntimeEvidence:false`, `evidenceScope:"cursor_install_smoke_core_api_runtime_not_full_host"`.
Зеркалит P09 (Claude divergent copies), но Cursor-специфично + несоответствие скрипта.
→ **DIVERGENT COPY / STALE EVIDENCE / WRONG SCRIPT**.
Закрытие: свести к одной канонической копии под git; перегенерировать phase40-evidence из
репо под ревью против ИМЕННО поставляемого `xiigen-extension-core-api.ps1`; архивировать
Documents-копию; синхронизировать скрипт evidence ↔ скрипт конфига.

## Замечания-подтверждения (не новые дыры)

- Core-скрипт `xiigen-extension-core-api.ps1` действительно доходит до компилированного
  `XiigenExtensionApiService` через `dotnet run` temp-csproj (уже зафиксировано P09 для того же
  скрипта) — то есть Cursor на Windows реально касается настоящего сервиса, но с
  компиляцией на каждый вызов (непоставляемо, P09).
- `normalizeMethod` (`:375-383`) молча коэрсит неизвестный метод в `prompt-to-task`; видимые
  пользователю поверхности (input-box команда, панель, chat) не задают метод → `orchestration-plan`
  и `skill-selection` из пользовательских поверхностей Cursor недостижимы (только через
  программный object-arg / test-команду). Это Cursor-эхо P10-паттерна (Claude 1/3 методов);
  фиксирую как подтверждение, не как отдельную новую строку.
- Панель (`buildPanelHtml`, `:1103-1194`) — настоящий webview с textarea+Submit+CSP+nonce и
  postMessage round-trip: сильнейшая user-facing поверхность тройки, но её реальный прогон в
  Cursor не доказан (evidence = install-smoke/blocked harness, P06/P08); при открытии панель
  сперва шлёт ЗАШИТЫЙ промпт (`:207`), пользовательский путь — только через Submit.
