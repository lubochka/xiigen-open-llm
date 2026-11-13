# Pass P37 — Владение core-логикой (llm_mvp_core) и тонкость адаптеров (R6)

Дата: 2026-07-08. Ассессор: Opus 4.8. Слайс: «Core logic in llm_mvp_core, thin
adapters (R6)? Leakage into extension layer?» Метод: bounded reads реального
кода трёх host-репо `C:\Projects\{claude,cursor,codex}-xiigen-smart-skills` и
core-сервиса `C:\Projects\llm_mvp_core\43.SmartSkillsDomain\XiigenExtensionApiService.cs`
+ контракт `XiigenExtensionApiContracts.cs`. Никаких build/test/runtime не
запускалось — это статический архитектурный разрез границы.

## Что такое R6 (эталон для этого прохода)

CLAUDE.md, правило R6 (SIMPLE AGENT BOUNDARY): «Agent and extension logic must
be maximally simple: a set of API calls that send a concrete task and receive a
concrete answer. No internal algorithm logic may leak into the agent layer. Core
logic lives in llm_mvp_core.» То есть адаптер обязан: (1) собрать
`XiigenExtensionRequest`, (2) вызвать core, (3) показать `XiigenExtensionResponse`.
Любая иная логика (парсинг, редакция, композиция ответа, разрешение локатора,
адъюдикация evidence/claim) в host-слое = утечка (leakage).

## Общая картина (честно, в обе стороны)

ПОЛОЖИТЕЛЬНОЕ (не переоцениваю в минус): семантическое ядро правда живёт в core.
Все три адаптера ЯВНО декларируют отсутствие host-side семантики:
`host_semantic_ranking:false` (cursor `extension.ts:434`, claude `:336/:445`),
`semanticRankingInHost:false` / `deterministicSkillSelectionAuthorityInHost:false`
(codex `:463/:468`), `host_skill_selection_binding:"pass_through_only"`
(cursor `:444`). Скиллы/скоринг/DPO в host НЕ считаются — это соответствует R6 по
семантической оси. Это подтверждается тем, что реальный dispatch методов и
семантические строки живут в core (`XiigenExtensionApiService.cs:955-990`
`BuildMethodSpecificDisplayText`, `:168/:193` `BuildHostChatDisplayText`/
`XiigenExtensionApiContract.BuildChatDisplayText`, `:187` `BuildDebugTrace`).

ОТРИЦАТЕЛЬНОЕ (это и есть НОВЫЕ дыры прохода): несмотря на «pass-through»
декларации, host-слой НЕ «maximally simple». В нём триплицирована (по одному разу
на каждый хост, разными языками — TS у cursor, рукописный ESM JS у claude/codex,
без общей библиотеки) целая семья НЕ-семантической, но всё же алгоритмической
логики: редакция (privacy), адъюдикация evidence/claim (honesty-гарантия проекта),
фабрикация core-ответов при сбое, повторный парсинг+композиция видимого ответа,
разрешение target-locator/checkpoint-ref. Размеры «тонких» адаптеров:
`cursor/src/extension.ts` = 1345 строк / 54.9 КБ / ~62 функции;
`claude/src/claude-entrypoint.js` = 535 строк / 18.8 КБ / 22 функции;
`codex/src/codex-entrypoint.js` = 544 строки / 17.8 КБ / 30 функций. Итого 2424
строки bespoke host-логики без единого shared-SDK.

Дедуп относительно мастера: P02 зафиксировал ОТСУТСТВИЕ review-gate «thin-adapter
no-hidden-determinism» (трактуя адаптеры как ещё-не-созданные). P13:195 — узкую
cursor-специфичную дыру «видимый ответ = фабрикованный хостом шаблон, семантики
нет». Этот проход другой: адаптеры РЕАЛЬНО существуют, и в них найдена конкретная
НЕ-семантическая утечка core-обязанностей (редакция/honesty/locator/фабрикация/
презентация) с file:line, триплицированная и расходящаяся с core-владельцем.

## Найденные утечки (детально)

### G1 — Редакция (privacy) переизобретена в 3 адаптерах и РАСХОДИТСЯ с core
Core владеет редакцией: `RedactPath` (`XiigenExtensionApiService.cs:1591-1599`)
возвращает repo-ОТНОСИТЕЛЬНЫЙ путь (или filename, если вне репо),
`RedactPathForDebug` (`:1277-1285`), `raw_prompt_redacted` (`:678/:784/:1098`),
контракт объявляет `redacted_input_payload`/`redacted_output_payload`
(`XiigenExtensionApiContracts.cs:296-297`). НО каждый host редактирует САМ и по
ДРУГИМ правилам: cursor `redactedTargetLocator` (`extension.ts:986-1001`) режет
через `path.basename` (только последний сегмент) + `writeRedactedTrace`
(`:1003-1045`); claude `redactedLocatorTrace`/`debugTraceDisplayText`/
`hasUnsafeDebugTrace` (`claude-entrypoint.js:291-304/:362-376`); codex
`redactedTargetLocator`/`createRedactedDebugPayload` (`codex-entrypoint.js:321-334/:385-391`).
Core = repo-relative; host = basename-only ⇒ один и тот же asset root
редактируется по-разному в зависимости от слоя, единого владельца privacy-гарантии
нет. Security-critical логика в адаптере, дрейф-prone.

### G2 — Адъюдикация evidence/claim и «no-overclaim» решается в host-слое
Главную гарантию проекта (scope claim / отсутствие overclaim) вычисляет bespoke
host-код, а не core. Cursor `normalizeResponse` (`extension.ts:847-918`) сам
считает `passed` (`:848`), `sameRunCoreCorrelationProven` (`:871`),
`runtimeEvidence`/`coreApiRuntimeEvidence`/`trueHostRuntimeEvidence` (`:872-875`),
безусловный `semanticQualityClaimed:false` (`:879`); `buildHostTrace` пишет
`noOverclaimReview` — целый прозаический абзац-вердикт (`:982`) и
`sameRunCorrelationMatched` (`:972`); `writeRedactedTrace` хардкодит
`forbiddenClaims{}`/`forbiddenActionsPerformed{}` (`:1029-1043`);
`buildDecisionEvidence` (`:602-610`) сам штампует `masqueradeProhibited:true`/
`hostProvidedSemanticDecision:false`. Аналоги: claude `buildHostTrace`
(`claude-entrypoint.js:422-455`, `host_model_logic:false` и т.д.), codex
frozen-объект `xiigenCodexEntrypoint` (`codex-entrypoint.js:443-469`). Honesty-ось
проекта — не в core-authority, а в трёх расходящихся адаптерах.

### G3 — Host ФАБРИКУЕТ core-контрактные ответы при сбое/отсутствии core
При падении/отсутствии core-эндпоинта адаптер синтезирует объект в форме
core-ответа: claude `buildAdapterErrorResponse` (`claude-entrypoint.js:400-419`)
строит целый `XiigenExtensionResponse` c `CheckSetResults`/`Warnings`/`Errors`/
`ChatDisplayText`; cursor `buildCoreMissingResponse` (вызывается `extension.ts:655`,
`invokeCoreApi` `:652-656`) + `writeCoreNegativeArtifacts`; codex
`createVisibleAnswerDisplay` (`codex-entrypoint.js:410-441`) фабрикует
`XIIGen Codex adapter error (${status})`. Host изготавливает core-выходы —
core-контрактная логика в адаптере (шире, чем P13:195: там только cursor
success-шаблон; здесь — failure/missing-core фабрикация целого контракта в 3 хостах).

### G4 — Видимый ответ ПОВТОРНО парсится и композируется в 3 хостах, хотя core уже владеет BuildChatDisplayText
Core строит канонический `ChatDisplayText` (`XiigenExtensionApiService.cs:168/:193`
через `XiigenExtensionApiContract.BuildChatDisplayText`) и `DebugTrace` с
`InputPayloadText`/`OutputPayloadText`; контракт объявляет ТОЧНЫЙ формат
`xiigen <method> :\ninput: <redacted>\noutput: <redacted>`
(`XiigenExtensionApiContracts.cs:316`). Тем не менее каждый host сам ПАРСИТ и
ПЕРЕСОБИРАЕТ видимый текст: claude `stripCoreDebugDisplay` режет core-вывод regex
`split(/\r?\nxiigen\s+/u)` (`claude-entrypoint.js:378-383`) + `buildVisibleChatDisplay`
(`:386-398`) + `debugTraceDisplayText` (`:367-376`); codex `createVisibleAnswerDisplay`
ищет маркер `indexOf(debugMarker)` и режет `slice(0,markerIndex)`
(`codex-entrypoint.js:418-422`) + `createDebugTraceDisplay` (`:393-400`); cursor
`buildCursorDisplayText`/`getDisplayText` (`extension.ts:329/:1062/:1081`) +
concreteAnswer-выбор (`:850-856`). Презентационная логика (в т.ч. хардкод того же
debug-формата, что уже в core) триплицирована; адаптер не «показывает» core-ответ,
а re-деривит его.

### G5 — Разрешение target-locator/checkpoint-ref сделано в адаптере, дублируя IRemoteUnitLocator (core/25.ModelServer)
CLAUDE.md: юнит обнаруживается через `IRemoteUnitLocator`, lifecycle-пути
принадлежат core. Но каждый host сам читает JSON-локатор, нормализует refs и
резолвит/редактирует asset-roots: cursor читает файл локатора
`JSON.parse(fs.readFileSync(resolveTargetPath(...)))` (`extension.ts:485-487`),
`resolveTargetPath` (`:546`), `normalizeLocatorRefs`/`normalizeTargetLocator`
(`:525` и рядом); claude `normalizeTargetLocator`/`resolveXiigenPath`/
`inferAssetRoot` (`claude-entrypoint.js:216-287`, маркер-поиск
`/(prompt-to-task|orchestration-reasoning|skill-selection)/`); codex тот же
`normalizeLocatorRefs`/`inferAssetRoot` (`codex-entrypoint.js:217-317`).
Locator-резолюция (core lifecycle) переизобретена в host-JS 3 раза.

### G6 — Нет общего adapter-SDK; 3 bespoke адаптера + host coercion метода
Никакой shared-библиотеки: 2424 строки host-логики (1345+535+544), 62/22/30
функций, три языка/копии одной и той же неявно-общей логики (parseArgs,
normalizeMethod, locator, redaction, request-build, spawn core, response-normalize,
evidence-judgment, display). Плюс метод-нормализация ДУБЛИРОВАНА в host и core:
core `NormalizeMethod` со switch и default-веткой `_` (`XiigenExtensionApiService.cs:961-974`),
host `normalizeMethod` МОЛЧА приводит неизвестный метод к `prompt-to-task`
(cursor `extension.ts:375-383`; аналоги в claude/codex) — host принимает routing-
решение и МАСКИРУЕТ core-default. R6 «maximally simple: a set of API calls»
нарушено: адаптер — это ~1300-строчный слой с собственной логикой, а не набор
API-вызовов.

## Вывод P37

Семантика (скиллы/скоринг/DPO/оркестрация) — в core, и адаптеры это честно
декларируют; по семантической оси R6 держится. НО «тонкость» R6 нарушена по
НЕ-семантической оси: редакция (privacy), honesty/claim-адъюдикация, фабрикация
core-ответов, презентация видимого ответа и разрешение локатора — всё это
триплицировано в host-слое, расходится с core-владельцами (RedactPath,
BuildChatDisplayText, контрактный формат, IRemoteUnitLocator) и не имеет общего
SDK. Это BUILT_NOT_PROVEN архитектурный дефект границы, а не COPIED_DOC: код
реально существует и его можно процитировать. Закрытие — вынести эти обязанности
в core (или общий тонкий SDK), чтобы host стал набором API-вызовов; затем
architecture-review каждого адаптера (закрывает MISSING gate из P02) с grep-
доказательством отсутствия host-side privacy/claim/locator/presentation-логики.
