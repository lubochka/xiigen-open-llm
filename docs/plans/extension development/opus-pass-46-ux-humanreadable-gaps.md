# Pass P46 — UX / человекочитаемость / статус / ясность ошибок в host-пути

Слайс: UX-automation layer — человекочитаемый статус, поверхность настроек/debug, ясность
ошибок именно в host-пути трёх расширений (Claude / Cursor / Codex). Оценка по РЕАЛЬНОМУ
коду хостов, не по копиям пакета. Только НОВЫЕ дыры (дедуп против P09-P20, P29).

## Что реально проверено (bounded reads)

- Cursor (единственная настоящая пользовательская UI-поверхность):
  `C:\Projects\cursor-xiigen-smart-skills\src\extension.ts` —
  command-обработчик `activate()` (`:121-147`), тост ответа (`:143` `showInformationMessage`),
  `getDisplayText`/`getBaseDisplayText` (`:1229-1251`), `buildCursorDisplayText` (`:1263-1273`),
  builders ошибок `buildCoreMissingResponse` (`:722-762`) / `buildCoreFailedResponse` (`:764-...`),
  webview-панель `buildPanelHtml` (`:1103-1147`) + `buildPanelPayload` (`:1056-1068`),
  `package.json` секция `contributes.configuration` (11 свойств, `:47-101`).
- Claude: `C:\Projects\claude-xiigen-smart-skills\src\claude-entrypoint.js` —
  `buildVisibleChatDisplay` (`:386-...`), error-display (`:404`), suppressed-debug текст (`:393`),
  debug-shape (`:440`), stdout-запись (`:525-531`).
- Codex: `C:\Projects\codex-xiigen-smart-skills\src\codex-entrypoint.js` —
  `createVisibleAnswerDisplay` (`:410-447`), статус-строка пользователю (`:430`).
- Тест-слой UX: grep `aria-label|accessible|role=alert|ux-automation|help text|human-readable`
  по `test/` и `reports/` трёх host-репо → единственный хит
  `cursor.../test/phase10/phase10CursorFullExtensionProof.js:91`.
- Пакет: `supporting-files/evidence-extension-full-readiness-dual-path/phase-14-user-visible-status-dashboard.{md,json}`.

Честная общая картина: у Claude и Codex пользовательской UI-поверхности нет вообще
(P09/P17), поэтому «человекочитаемый экран» для них к тому же неприменим по отсутствию.
У Cursor UI-поверхности ЕСТЬ (команда-тост + webview-панель), и именно там видно, что
пользователю показывают машинные токены/жаргон и невнятные ошибки. Единственная реальная
UX-автоматизация во всём проекте — внутренний WPF-дашборд (P29), а он к живым host-поверхностям
не привязан. То, что проект называет «user-visible status dashboard» (phase-14) — это
Luba/agent-facing карта фаз исполнения, а не продуктовый статус для конечного пользователя.

## НОВЫЕ дыры (детально)

### G46-1 — Ошибки Cursor показываются как INFO-тост, текст = двойной жаргон + абсолютный dev-путь, без подсказки что делать
- Реальный статус: UX-DEFECT / error-clarity.
- Evidence:
  - `extension.ts:143` — на command-пути ответ (в т.ч. ошибка) выводится через
    `vscode.window.showInformationMessage(getDisplayText(result))` — уровень INFO, не
    `showErrorMessage`; в extension.ts нет ни одного `showErrorMessage`/`showWarningMessage`.
  - `extension.ts:1263-1266` — `buildCursorDisplayText` на ошибке даёт
    `` `XIIGen Cursor error: ${getBaseDisplayText(response)}` ``, но `getBaseDisplayText`
    возвращает СЫРОЙ текст билдеров ошибок.
  - `extension.ts:723` + `:741-742` — `CONTRACT_ONLY_DOWNGRADE: <coreApiScript> is missing; ...`,
    где `<coreApiScript>` = абсолютный dev-путь (`package.json:56` default
    `C:\Projects\llm_mvp_core\tools\xiigen-extension-core-api.ps1`).
  - `extension.ts:780` — `CORE_API_FAILED: <message>`.
  - Итог для пользователя (missing core): тост INFO
    «XIIGen Cursor error: CONTRACT_ONLY_DOWNGRADE: C:\Projects\llm_mvp_core\tools\xiigen-extension-core-api.ps1 is missing; Cursor host entrypoint ran but no core runtime decision is claimed.»
- Дыра: ошибка неотличима от обычного инфо-сообщения (не тот канал severity), содержит два
  внутренних код-префикса, чужой абсолютный путь и НИ ОДНОЙ инструкции по исправлению
  (например «откройте настройки и укажите xiigenSmartSkills.coreRoot»).
- Дедуп: не дублирует P13 gap-5 (там про шаблонный ответ на success-пути), P16 (там про
  «panel/runtime не прогонялись»). Здесь — именно UX ошибок на command-поверхности.
- Закрытие: показывать ошибки через `showErrorMessage`; строить сообщение из
  `error.code → человекочитаемая фраза + конкретный next step` (без внутренних префиксов и
  без сырого абсолютного пути); добавить кнопку «Open Settings».

### G46-2 — По всем 3 хостам пользователю в статус/ответ утекают машинные токены и внутренний жаргон; нет слоя plain-language статуса
- Реальный статус: HUMAN-READABLE-STATUS MISSING (cross-host).
- Evidence:
  - Cursor `extension.ts:1246-1247` — fallback `getBaseDisplayText` возвращает
    `response.Status` дословно (например `core_endpoint_missing`,
    `core_boundary_partial_no_semantic_quality_claim`, `prompt_to_task_not_ready_degraded`).
  - Claude `claude-entrypoint.js:393` — «Core xiigen boundary executed. Debug trace suppressed
    because redaction flags were unsafe.»; типовой видимый текст — «Core xiigen boundary
    executed. Semantic quality is not claimed.» — внутренний жаргон («boundary executed»,
    «redaction flags»), показываемый пользователю.
  - Codex `codex-entrypoint.js:430` — `` `XIIGen ${method} completed with status ${status}.` ``,
    где `status` = сырой токен ядра (`core_api_failed`, `no_response_file`,
    `loaded_state_no_matching_learned_pattern`).
- Дыра: единственный «человеческий» слой отсутствует — то, что реально видит пользователь,
  это внутренние коды/жаргон, а не понятная фраза. Нарушение требования human-readable /
  no-internal-jargon (AR-22 / Plan testability UX layer).
- Дедуп: отличается от P10 / P13 gap-5 / P17 gap-2 (те — про ОТСУТСТВИЕ семантического ответа
  «by construction»). Здесь — про то, что ПОКАЗЫВАЕМЫЙ статус/фраза сам по себе не
  человекочитаем.
- Закрытие: ввести общий core-слой перевода `Status/errorCode → plain-language` (и RU/EN),
  который хосты рендерят вместо токенов; тест, что видимый текст не содержит snake_case-токенов
  и внутренних терминов.

### G46-3 — Webview-панель Cursor: как основной «результат» показывает внутренние пути к артефактам; статус — жаргон; нет области ошибок/role=alert; lang захардкожен; нет help/примеров
- Реальный статус: UI-CONTENT DEFECT (единственная реальная UI-поверхность).
- Evidence:
  - `extension.ts:1140-1145` — под ответом `<dl>` показывает пользователю
    `Response / Trace / Core trace / Entrypoint` = внутренние файловые пути
    (`coreResponseRef/hostTraceRef/coreTraceRef/entrypointResponseRef`).
  - `extension.ts:1056-1067` — `statusText` = «Core API response recorded for adapter
    functionality» / «Core API response missing or failed» (внутренний термин «adapter
    functionality»); `displayText` = `getDisplayText` (может быть сырой Status, см. G46-2).
  - `extension.ts:1137-1138` — статус в простом `<p class="status">` внутри
    `aria-live="polite"`, БЕЗ `role="alert"` и без визуального различения ошибки/успеха.
  - `extension.ts:1108` — `<html lang="en">` захардкожен (нет локализации).
  - `extension.ts:1132-1135` — форма только textarea+Submit, placeholder «Paste a prompt…»,
    без примеров валидной задачи, без селектора метода/скиллов (метод-выбор — угол P14, здесь
    UX help/examples).
- Дыра: пользователь видит внутренние пути как «результат», жаргонный статус и не получает
  выделенной, доступной (a11y) индикации ошибки; нет help/примеров и локализации.
- Дедуп: отличается от P16 (panel не прогонялась в реальном Cursor) и P14 (auto-run
  зашитого промпта / нет селектора метода) — здесь UX-СОДЕРЖИМОЕ панели.
- Закрытие: показывать человекочитаемый результат, а пути — свернуть в «Details/Debug»;
  `role="alert"` + цветовая индикация для ошибок; help-текст с примерами; локализация lang.

### G46-4 — Честное «модель не обучена / degraded» не доносится до пользователя человеческим языком
- Реальный статус: UNTRAINED-HONESTY UX unmet.
- Evidence: честное untrained/degraded состояние эмитится как машинные токены
  `prompt_to_task_not_ready_degraded` / `loaded_state_no_matching_learned_pattern` /
  `core_boundary_partial_no_semantic_quality_claim` (см. QA-ответы Claude/Cursor: P12/P16) и
  прокидывается в видимый текст без перевода (`extension.ts:1246-1247`;
  Codex `:430` `status`). Нет ни одной фразы вида «локальная модель ещё не обучена; показан
  базовый результат».
- Дыра: у проекта есть легитимное состояние `PASS_AS_HONEST_UNTRAINED_SCAFFOLD`
  (CLAUDE.md), но оно НЕ доносится до пользователя человекочитаемо — пользователь видит
  токен, а не понятное объяснение деградации.
- Дедуп: ново — предыдущие проходы фиксировали отсутствие обучения/семантики, но не
  ПРЕЗЕНТАЦИЮ untrained-состояния конечному пользователю.
- Закрытие: маппинг untrained/degraded-кодов в явную человеческую фразу (что не готово, что
  показано вместо, что сделать), с тестом presence этой фразы на degraded-пути.

### G46-5 — UX-automation не привязана ни к одной живой host-поверхности; единственный «human-readable error» ассерт — поверхностный префикс-чек и никогда не исполнялся
- Реальный статус: MISSING UX-TEST LAYER (shipped surfaces) / dead+shallow assertion.
- Evidence:
  - Единственная реальная UX-автоматизация проекта — внутренний WPF-дашборд
    (`Tests.ApiDashboardWpf`, accessible-names/tooltips/no-jargon/layout — P29); это не
    host-поверхность расширения.
  - grep по `test/`+`reports/` трёх host-репо (`aria-label|accessible|role=alert|
    ux-automation|help text|human-readable`) → единственный хит
    `cursor.../test/phase10/phase10CursorFullExtensionProof.js:91`
    `assert(displayText.startsWith("XIIGen Cursor error:"), "... visible error text was not human-readable")`.
  - Этот ассерт (а) лишь проверяет ПРЕФИКС, тогда как обёрнутый текст всё равно несёт
    жаргон `CONTRACT_ONLY_DOWNGRADE`/`CORE_API_FAILED` + абсолютный путь (G46-1) — то есть
    «human-readable» проходит на нечеловекочитаемом тексте; (б) `phase10` ни разу не
    исполнялся и драйвит test-only команду (P16 gap-4) — то есть ассерт мёртв.
- Дыра: у живых пользовательских поверхностей (Cursor panel/command/chat) нет UX-автоматизации
  (accessible names, ясная ошибка, help/examples, локализация); единственная UX-проверка —
  поверхностная и неисполняемая. Plan testability UX-layer для shipped-поверхностей не
  выполнен.
- Дедуп: отличается от P12 gap-5 (у Claude нет UI → неприменимо) — здесь у Cursor UI ЕСТЬ,
  но UX-автоматизации нет, а дашбордная UX-автоматизация направлена на внутренний инструмент.
- Закрытие: UX-automation-набор против реального Cursor (panel+command): accessible names,
  видимая ошибка `role=alert`, help/examples, отсутствие жаргона/токенов, локализация;
  сделать ассерты содержательными (plain-language + actionability), а не префиксными; включить
  в исполняемый гейт.

### G46-6 — Тексты-описания настроек — внутренний жаргон, часть указывает на мёртвые «Future» настройки; у Claude/Codex настроек-поверхности нет вовсе
- Реальный статус: SETTINGS HELP-TEXT DEFECT.
- Evidence (Cursor `package.json` `contributes.configuration`):
  - `:52` coreRoot description «llm_mvp_core root used for the host-neutral API/check-set contract.»
  - `:57` coreApiScript «Core-owned XIIGen API script. Cursor must call core instead of embedding semantic or skill-selection logic.»
  - `:72` debug «…redacted xiigen exe method/input/output payload.»
  - `:77` targetLocatorPath «Optional QSG target-locator JSON path passed unchanged to llm_mvp_core.»
  - `:62` promptSuite «Future Cursor prompt-suite/check-set file.» и `:67` installSmokeReport
    «Future install-smoke report…» — мёртвые/будущие настройки (dead per P13 gap-3).
  - Claude/Codex: секции settings в манифестах нет (P09 gap-3 / P17 gap-4) → help-текста нет
    вообще.
- Дыра: описания настроек написаны на внутреннем жаргоне («check-set contract», «QSG
  target-locator», «xiigen exe … payload»), непонятны непрограммисту; часть настроек
  документирует несуществующую функциональность. UX-качество поверхности настроек низкое.
- Дедуп: отличается от P13 gap-2 (абсолютные дефолты) и gap-3 (schema drift) — здесь именно
  ясность/жаргон help-текста.
- Закрытие: переписать описания на человеческий язык с примером значения; убрать/пометить
  «Future» настройки; для Claude/Codex ввести настоящую settings-поверхность с help-текстом.

### G46-7 — То, что проект называет «user-visible status dashboard», — это Luba/agent-facing карта фаз исполнения, а не продуктовый статус для конечного пользователя
- Реальный статус: DELIVERABLE-CONFLATION (user-visible status).
- Evidence: `supporting-files/evidence-extension-full-readiness-dual-path/phase-14-user-visible-status-dashboard.md`
  — «Build a human-readable status dashboard/summary from Phase 01-13 evidence», «собрать для
  Luba понятную карту текущего состояния», «Фаза 15 из 17…», «Шаг 14.01…». Это статус
  исполнения сессии/фаз для Luba, а не статус-поверхность внутри host-расширения для
  конечного пользователя. Внутри хостов человеческий end-user статус отсутствует (G46-2).
- Дыра: требование «user-visible status» закрыто agent-facing документом; продуктовой
  end-user статус-поверхности (in-host, человекочитаемой) нет.
- Дедуп: ново — предыдущие проходы не отмечали подмену продуктового «user-visible status»
  на Luba-facing execution-progress.
- Закрытие: развести два артефакта: (1) Luba-facing execution status (уже есть) и (2)
  продуктовый in-host end-user статус (что сделала модель, готова/не готова, что показано),
  человекочитаемый; последний покрыть UX-автоматизацией (G46-5).

## Вывод P46

Все семь дыр — про UX-презентацию host-пути: пользователю показывают машинные токены/жаргон
вместо человеческого статуса (G46-2), ошибки идут не тем каналом severity и без remediation
(G46-1), панель Cursor выдаёт внутренние пути за результат и лишена доступной индикации ошибок
(G46-3), честное untrained-состояние не объяснено человеку (G46-4), UX-автоматизации на живых
поверхностях нет, а единственный «human-readable» ассерт поверхностен и мёртв (G46-5), тексты
настроек жаргонны и частью мертвы (G46-6), а «user-visible status» подменён agent-facing
документом (G46-7). Ни одна не заявляет проект готовым; каждая привязана к file:line реального
кода хостов.
