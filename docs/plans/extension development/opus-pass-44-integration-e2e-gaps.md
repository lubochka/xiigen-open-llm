# Pass P44 — Сквозная интеграция (end-to-end): какие звенья цепи user→host→extension→core→answer→host не собраны/не доказаны

Автор прохода: Opus 4.8 REAL-STATE ASSESSOR. Дата: 2026-07-09. Ветка core: DesignToCode-clean.
Оценка ведётся от РЕАЛЬНОГО кода/артефактов, не от самоотчёта пакета.

## 0. Что именно проверялось в этом проходе

Фокус P44 — не отдельные компоненты, а СВЯЗКИ (links) единой сквозной цепи:

```
пользователь → host UI (Claude/Cursor/Codex) → код расширения (адаптер)
    → транспорт (xiigen-extension-core-api.ps1) → core-сервис (XiigenExtensionApiService)
        → конвейер (prompt-to-task → orchestration → skill-selection)
    → ответ (XiigenExtensionResponse) → транспорт → host UI → пользователь
```

Bounded-reads реального кода:
- core: `C:\Projects\llm_mvp_core\43.SmartSkillsDomain\XiigenExtensionApiService.cs` (конструкторы `:51-86`, метод `ProcessHostExtensionRequest` `:88-188`, `ResolveRequestLoadState` `:279-312`, `LoadCommonThenUserAdaptiveAssets` `:1473+`);
- транспорт: `C:\Projects\llm_mvp_core\tools\xiigen-extension-core-api.ps1` (прочитан ЦЕЛИКОМ, 275 строк);
- host-адаптеры: `cursor-xiigen-smart-skills\src\extension.ts` (`activate` `:121`, `invokeCoreApi` `:652-689`, spawn powershell `:661`), `cursor\package.json` (манифест целиком), `claude-xiigen-smart-skills\.claude-plugin\plugin.json` + `hooks\hooks.json` + `package.json`, `codex-xiigen-smart-skills\.codex\xiigen-smart-skills.json`;
- дедуп-скан всего мастер-реестра P01…P43 (гряды по терминам correlation / schema_version / ILogicalOrchestrator / fabricat / available-skills / request-shape и др.).

## 1. ПОЛОЖИТЕЛЬНЫЕ факты (не переоцениваю в минус — многие звенья РЕАЛЬНО собраны в коде)

Чтобы честно очертить границу «дыр», сначала фиксирую, что в цепи ФАКТИЧЕСКИ соединено:

1. **core-сервис РЕАЛЬНО вызывает все 3 машины и цепляет их выходы вперёд.** `XiigenExtensionApiService.cs:118` `PromptToTaskMachine.Translate(...)` → `:130` `BuildOrchestrationRequest(request, promptToTask.StructuredTask)` → `:131` `OrchestrationReasoningMachine.Plan(...)` → `:143` `DecideSkills(...)`. То есть внутренний конвейер prompt→task→orchestration→skills СОБРАН (выход стадии 1 идёт во вход стадии 2). Это ВАЖНАЯ коррекция к P02 gap-5 «end-to-end как единый flow недостижим» — он достижим, см. дыру N3 ниже.
2. **Транспорт реально доходит до компилированного core.** `xiigen-extension-core-api.ps1:165` `new XiigenExtensionApiService(coreRoot)`, `:166` `ProcessHostExtensionRequest(request)`, `:184-185` `dotnet run` (не синтетика). Common-ассеты грузятся из coreRoot.
3. **Cursor host-адаптер РЕАЛЬНО собран как расширение.** `package.json` объявляет `activationEvents`/`contributes.commands`/`chatParticipants`, `main:./dist/extension.js`; `extension.ts:121` `activate`, `:124` `registerCommand("xiigen.smartSkills.select")` (показывает `showInputBox` пользователю `:132`, показывает ответ `showInformationMessage` `:143`), `:162-177` УСЛОВНО регистрирует `vscode.chat.createChatParticipant` (guard по feature-detect), `:206-217` `xiigenSmartSkills.panel` с `createWebviewPanel`; транспорт `:661` `spawn("powershell.exe", ...)` → core-ps1. Cursor — единственный host с реальной user-driven поверхностью (команда/чат/панель).
4. **User-adaptive нога МОЖЕТ достичь runtime.** `ResolveRequestLoadState:279-312`: при наличии `request.TargetLocator.UserAdaptiveAssetRoot` (`:289`) вызывается `LoadCommonThenUserAdaptiveAssets(FindRepoRoot(), resolvedRoot)` (`:312`). То есть проводка adaptive-leg в код есть (числовой прирост — отдельная незакрытая дыра P05/P43, НЕ дублирую).

Вывод по границе: «сборка звеньев» местами реальна; недоказанность — в качестве/семантике (машины-стабы, P02/P21) и в дальних Path B (P32-P34). Здесь, в P44, я добавляю ТОЛЬКО НОВЫЕ интеграционные разрывы самих связок, не пойманные P01-P43.

## 2. НОВЫЕ дыры сквозной интеграции (3)

### N3 — Поставляемая E2E-цепь ОБХОДИТ ILogicalOrchestrator / 15.OrchestratorModel (жёсткий линейный вызов в API-сервисе) + КОРРЕКЦИЯ P02

- **Real-status:** ARCH BYPASS / shipped-path routes around orchestration layer (+ correction P02 gap-5).
- **Evidence:** `43.SmartSkillsDomain\XiigenExtensionApiService.cs:117-154` — жёстко зашитая линейная цепочка `PromptToTaskMachine.Translate` (`:118`) → `OrchestrationReasoningMachine.Plan` (`:131`) → `DecideSkills`/`SkillSelectionMachine.DecideFromTrainedState` (`:143`/`:392`); конструкторы `:66-78` принимают ТРИ машины напрямую (`PromptToTaskMachine`/`OrchestrationReasoningMachine`/`SkillSelectionMachine`), НЕ `ILogicalOrchestrator`; grep `ILogicalOrchestrator|ILogicalFlow` в `XiigenExtensionApiService.cs` → 0; P02 gap-5 отдельно подтверждает grep `15.OrchestratorModel` на 3 машины → 0.
- **Gap:** Требование «task→plan orchestration» (v3 #4) предписывает маршрутизацию через `ILogicalOrchestrator` (A-to-B пары, language/repo fan-out, arbiter/reprocessing пути). Реальная поставляемая цепь их НЕ исполняет: это фиксированный прямой 3-вызов внутри API-сервиса, без оркестратора, без регистрации `ILogicalFlow`, без fan-out/arbiter. То есть даже если бы стадии были зарегистрированы в `15.OrchestratorModel`, поставляемый путь всё равно их не использует. КОРРЕКТИРУЕТ вывод P02 gap-5 «end-to-end как единый flow недостижим»: цепь ДОСТИЖИМА end-to-end, но собрана в обход обязательной абстракции оркестрации. Отдельно от P02 gap-5 (там — отсутствие регистрации + ошибочный вывод о недостижимости); здесь — фактический runtime-путь маршрутизирован мимо оркестратора и не даёт fan-out/A-to-B/arbiter.
- **Closure steps:** Либо провести поставляемый E2E-путь через `ILogicalOrchestrator` (зарегистрировать 3 стадии как `ILogicalFlow` в `15.OrchestratorModel`, доказать A-to-B/fan-out/arbiter/reprocessing на реальном прогоне), либо явно задокументировать, что расширения используют fixed-linear-путь БЕЗ оркестрации, и убрать требование orchestration-layer из def-of-done расширений. Обновить P02 gap-5 (снять формулировку «недостижим»).

### N1 — Контракт ответа имеет ТРЕТЬЕГО владельца на слое ТРАНСПОРТА: PowerShell фабрикует XiigenExtensionResponse на ошибке; нет versioned wire-contract и cross-layer conformance-теста

- **Real-status:** CONTRACT-FRAGMENTATION-AT-TRANSPORT / third divergent owner + unversioned wire.
- **Evidence:** `tools\xiigen-extension-core-api.ps1:229-244` — в catch-ветке PowerShell РУЧНО собирает целый объект формы `XiigenExtensionResponse` (`Success=$false`, `Status="core_endpoint_error"`, `ChatDisplayText="Xiigen core endpoint failed..."`, `CheckSetResults=[ordered]@{core_boundary_executed;semantic_quality_claimed;runtime_extension_claimed}` строками, `Warnings/Errors` массивами); `:191-192` транспорт также ПЕРЕ-парсит `CheckSetResults.runtime_extension_claimed/semantic_quality_claimed` из ответа core строковыми ключами; сам контракт request/response БЕЗ версии — `XiigenExtensionResponse` (поля `:176-186`: Success/Status/ChatDisplayText/SelectedSkills/CheckSetResults/StructuredTaskRefs/ScenarioRefs/Warnings/Errors/DebugTrace) не несёт `schema_version` (версия есть только у trace-объекта `:196`, не у контракта); реальный дрейф формы по хостам: codex `reports\HOST-08A-codex-request-shape-repair.json`, `reports\HOST-REPAIR-CODEX-AVAILABLE-SKILLS-PARSER\` (отдельные ремонты формы запроса и парсера available-skills).
- **Gap:** Контракт ответа расширения имеет как минимум ТРИ независимых владельца вдоль E2E-цепи: (а) C#-core (`XiigenExtensionResponse`), (б) PowerShell-транспорт (фабрикует контракт на ошибке, свой строковый `CheckSetResults`-schema), (в) host-JS/TS-адаптеры (синтез контракта — уже зафиксировано P37). При этом нет ни `schema_version` на самом request/response, ни единого cross-layer conformance-теста, доказывающего, что три слоя сериализуют/парсят одну и ту же форму. Ремонты формы запроса и парсера у Codex — прямое доказательство дрейфа. Отдельно от P37 (там host-JS синтез контракта) — здесь ТРАНСПОРТ (PowerShell) как третий владелец + отсутствие версии/конформанс-теста. Отдельно от P39 (там contract-тесты доказывают ЧЕСТНОСТЬ, а не кросс-слойную идентичность формы).
- **Closure steps:** Ввести один versioned wire-contract (schema_version на request И response), единый источник формы; запретить транспорту и host-адаптерам синтезировать контракт (core обязан вернуть детерминированный error/missing-ответ в контракте — уже closure P37); PowerShell на ошибке лишь транслирует core-ответ или строго генерирует по общей схеме; добавить cross-layer conformance-тест «один и тот же request/response round-trip идентичен через host→PS→core→PS→host для всех 3 хостов»; убрать строковый re-parse `CheckSetResults` из транспорта.

### N2 — Round-trip не несёт in-payload корреляции: request имеет CorrelationId, а XiigenExtensionResponse — нет; связь ответ↔запрос доказуема только вне контракта

- **Real-status:** CONTRACT GAP / no response-level correlation binding.
- **Evidence:** `xiigen-extension-core-api.ps1:207` транспорт ЧИТАЕТ `request.CorrelationId` (для trace); но контракт ответа `XiigenExtensionResponse` (`XiigenExtensionApiService.cs:176-186`) НЕ содержит поля CorrelationId/prompt-hash — корреляция присутствует только в отдельном trace-JSON (`:207`), который не является телом ответа, показываемым хосту; host-адаптеры рендерят `ChatDisplayText`/`SelectedSkills` из тела ответа, где привязки к запросу нет.
- **Gap:** В E2E round-trip нет привязки уровня полезной нагрузки между запросом и ответом: хост, показывая ответ пользователю, не может через тело ответа удостовериться, что этот ответ — на ИМЕННО этот отправленный промпт (корреляция живёт в out-of-band trace). При текущем файловом handshake (один запрос — один файл ответа) корреляция неявна через путь файла, но: (1) любой поставляемый тёплый/мультиплексный/async-транспорт (обязателен по P41 вместо per-call compile) останется без response-level привязки; (2) невозможно машинно доказать «этот видимый ответ = ответ на этот prompt-hash» из контракта — что как раз и подрывало Path B (P06/P32/P33 gap-3 фиксировали это на уровне ЕVIDENCE-артефактов Path B; здесь корень — сам КОНТРАКТ ответа без correlation/prompt-hash). Отдельно от P06/P32/P33 (там про Path B evidence без prompt-hash) — здесь про отсутствие поля в контракте ответа.
- **Closure steps:** Добавить в `XiigenExtensionResponse` обязательные `CorrelationId` (эхо запроса) и `UserPromptSha256`; хост валидирует `response.CorrelationId == request.CorrelationId` и `response.UserPromptSha256 == sha256(prompt)` до показа; Path A/Path B join строить на равенстве этих полей контракта, а не на приписывании хеша из одной ноги.

## 3. Что сознательно НЕ занесено (во избежание дублей/оверклейма)

- Claude «только SessionStart-hook / зашитый smoke / вывод в чужой репо» → уже P09 gap-1/gap-4.
- Codex «skeleton_only / entrypoint_implemented=false» → уже P34 + P08/P17-P20 (host-repo зрелость). Проверено: `codex-entrypoint.ps1` И `.js` ОБА существуют (манифест не «битый»), поэтому дыру о несуществующем файле НЕ завожу.
- Cursor «нет tsconfig/@types/vscode/.vsix/пустые devDependencies» → термины tsconfig/devDependencies/.vsix уже присутствуют в реестре (P08 gap-3 build-asymmetry, P09/P13); chatParticipant РЕАЛЬНО регистрируется (`extension.ts:162-177`) — НЕ дыра.
- Транспорт «per-call compile / SDK / absolute path / не поставляем» → P08 gap-1, P41 gap-1.
- Status всегда PartialBoundaryStatus / машины-стабы / dead semantic-gate → P02/P03/P10/P21.
- Adaptive-leg числовой прирост / continue-training → P05/P43 (проводка в код есть — п.1.4 выше).

## 4. Итог P44

Новые дыры: 3 (N1 контракт-фрагментация на транспорте + unversioned wire; N2 отсутствие correlation в контракте ответа; N3 обход ILogicalOrchestrator поставляемым путём + коррекция P02). Все три — про САМИ связки E2E-цепи, а не про качество отдельных стадий. Плюс зафиксирована коррекция P02 gap-5 (цепь достижима end-to-end, но мимо оркестратора). Проект НЕ объявляется завершённым.
