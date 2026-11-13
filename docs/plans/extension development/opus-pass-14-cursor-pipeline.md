# Pass P14 — Cursor: конвейер prompt-to-task → orchestration → skills (реальное состояние)

Дата: 2026-07-08. Ассессор: Opus 4.8 (REAL-STATE). Срез: РЕАЛЬНАЯ обработка промпта
в Cursor-расширении — какие стадии конвейера (`prompt-to-task` → `orchestration` →
`skill-selection`) реально достижимы из Cursor, с каким входом, и является ли это
настоящим конвейером. Аналог P10 (Claude pipeline), но для Cursor-хоста и его
трёх пользовательских поверхностей.

Источники (bounded reads, прочитано целиком по нужным диапазонам):
- `C:\Projects\cursor-xiigen-smart-skills\src\extension.ts` (1346 строк) — команды
  `activate()`, `normalizeMethod`, `runCursorSmartSkillsRequest`, `buildCoreRequest`,
  chat participant, webview-панель, panel HTML.
- Ядро: `C:\Projects\llm_mvp_core\43.SmartSkillsDomain\XiigenExtensionApiService.cs`
  (`ProcessHostExtensionRequest:88-201`, `DecideSkills:356-399`,
  `BuildMethodSpecificDisplayText:955-975`, `RequiresSkillSurface:995-998`).
- Контракт: `43.SmartSkillsDomain\XiigenExtensionApiContracts.cs:61-72` (набор методов).

Дедуп: НЕ повторяю P02 (машины prompt-to-task/orchestration = стабы; pipeline не
собран в `15.OrchestratorModel`; order-leakage в SkillSelection), P10 (Claude:
dead-gate semantic-quality; empty-catalogue stage-3; 1/3 methods; arch-flat не через
`15.OrchestratorModel`), P13 (Cursor entrypoint: powershell-транспорт, non-shippable
defaults, settings-drift, test-harness команда, chat participant unstable-API,
display-text шаблон без семантики, non-reproducible build, divergent copy). Здесь —
только НОВЫЕ дыры именно конвейерного среза Cursor.

---

## Что реально устроено (честная картина)

1. **Три пользовательские поверхности Cursor гонят конвейер, но все — с дефолтами.**
   - Команда `xiigen.smartSkills.select` (`extension.ts:124-146`): интерактивно берёт
     промпт из `showInputBox` (:132-135); метод/скиллы читаются ТОЛЬКО если команда
     вызвана программно объектом (:126-128). При ручном вызове `method=undefined`.
   - Chat participant `xiigen-smart-skills` (`:171-177`): хендлер зовёт
     `handleCursorChatParticipantRequest(context, request.prompt, stream)` (:173-174)
     БЕЗ options → метод/скиллы пусты.
   - Webview-панель `xiigenSmartSkills.panel` (`:206-266`): submit-путь зовёт
     `runCursorSmartSkillsRequest(context, submittedPrompt, ..., "panel")` (:237) БЕЗ
     options → метод/скиллы пусты.
   Итог: `normalizeMethod(undefined)` (`:375-383`) = `"prompt-to-task"`. Значит из
   ЛЮБОЙ интерактивной поверхности Cursor исполняется только стадия `prompt-to-task`.
   Методы `orchestration-plan` и `skill-selection` достижимы лишь через
   тест/программный объект-вызов (`:126-128`, `__test.chatHandlerLevel:180-202`).

2. **`normalizeMethod` роняет `feedback` и `debug-trace`.** Разрешённый набор в
   Cursor — ровно `{prompt-to-task, orchestration-plan, skill-selection}` (:379);
   всё остальное молча приводится к `prompt-to-task` (:382). Контракт же объявляет
   пять методов, включая `FeedbackMethod="feedback"` (единственный online-сигнал
   adaptive DPO) и `DebugTraceMethod="debug-trace"`
   (`XiigenExtensionApiContracts.cs:64-65`, оба в `AllowedMethods :69-72`). Т.е.
   Cursor-хост физически не может отправить `feedback` в ядро — метод переписывается
   ДО построения запроса `buildCoreRequest`.

3. **Каталог скиллов никогда не формируется.** `availableSkills`/`requestedSkills`
   берутся исключительно из options (`:127-128`, `:321-322`), интерактивные
   поверхности их не задают. Хост сам декларирует
   `host_skill_selection_binding: "pass_through_only"` (`:444`) и
   `cursor_skill_selection_binding_scope: "pass_through_to_XiigenExtensionRequest_no_host_selector"`
   (`:460`). В ядре пустой каталог → `DecideSkills` возвращает
   `NO_AVAILABLE_SKILLS_PROVIDED`, `Confidence 0.0`, `RequiresSecondPass true`
   (`XiigenExtensionApiService.cs:361-371`). Панельный HTML содержит только один
   `<textarea>` + `<button>Submit` (`extension.ts:1131-1136`) — нет ни селектора
   метода, ни поля каталога скиллов.

4. **Панель авто-гонит зашитый промпт как evidence.** При открытии
   `xiigenSmartSkills.panel` СРАЗУ выполняется core-запрос на ЖЁСТКО зашитый промпт
   «A Cursor user opens the XIIGen Smart Skills panel and asks for thin-client
   host-runtime evidence.» (`:207-208`) и пишется UI-evidence `recordPanelUiEvidence(result, false)`
   (:218) — ДО любого пользовательского ввода. Реальный промпт течёт только после
   Submit (:224-242).

5. **Стадия 3 конвейера не потребляет выход стадий 1-2 (дефект целостности).**
   В ядре стадия 2 действительно берёт выход стадии 1:
   `BuildOrchestrationRequest(request, promptToTask.StructuredTask)` (`:130`). Но
   стадия 3 — `DecideSkills(request, promptSha256, activeLoadState)` (`:143`) — не
   принимает ни `promptToTask.StructuredTask`, ни `orchestration.ScenarioPlan`.
   Внутри `SkillSelectionRequest.Prompt = request.UserPromptOrTaskText` (`:375`) —
   это СЫРОЙ промпт хоста, а не произведённый task/plan (`:374-390`). Значит
   «task/plan → skills» разорван на третьем хопе: выбор скиллов идёт от сырого
   промпта + host skill surface, а не от задачи/плана.

---

## НОВЫЕ дыры (P14)

| # | Component | Real-status | Evidence file:line | Gap | Closure steps |
|---|---|---|---|---|---|
| 1 | Cursor `normalizeMethod`: методы `feedback`/`debug-trace` недостижимы | HOST-LAYER METHOD DROP | `cursor…\src\extension.ts:375-383` (набор из 3, else→prompt-to-task); контракт `XiigenExtensionApiContracts.cs:64-65` `FeedbackMethod`/`DebugTraceMethod`, `:69-72` в `AllowedMethods` | Cursor-хост молча переписывает любой метод вне 3-набора в `prompt-to-task` ДО построения запроса; `feedback` (единственный online-сигнал adaptive DPO) и `debug-trace` недостижимы из Cursor by construction. Отдельно от P11 (ядро не обрабатывает feedback) — здесь хост роняет метод ещё до ядра. | Расширить `normalizeMethod` до всех контрактных методов (или дать отдельную feedback-команду); прокинуть `feedback`→ядро; доказать, что feedback из Cursor создаёт accepted adaptive DPO-пару в `.xiigen`. |
| 2 | Интерактивное покрытие методов Cursor (3 поверхности → 1/3) | PARTIAL (1/3 methods) | InputBox `extension.ts:132-141` (нет method); chat `:173-174` (нет options); panel `:207-208`,`:237` (нет options) → `normalizeMethod(undefined)`=prompt-to-task; метод задаётся только тест/объект-путём `:126-128`,`:180-194` | Все три пользовательские Cursor-поверхности (команда-InputBox, chat participant, webview-панель) гонят только `prompt-to-task`; `orchestration-plan` и `skill-selection` недостижимы интерактивно. Cursor-аналог P10 (Claude 1/3), но другой хост и ТРИ поверхности без селектора метода. | Дать метод-селектор в панели/командах; прогнать `orchestration-plan` и `skill-selection` end-to-end из Cursor с реальными входами; собрать per-method evidence. |
| 3 | Стадия 3 (task/plan→skills) на Cursor-пути — постоянный no-op, «no host selector» | NO-OP / empty-catalogue (self-declared) | `extension.ts:444` `host_skill_selection_binding:"pass_through_only"`, `:460` `..._no_host_selector`; availableSkills только из options; ядро `XiigenExtensionApiService.cs:361-371` `NO_AVAILABLE_SKILLS_PROVIDED` conf 0 `RequiresSecondPass true`; panel HTML `:1131-1136` только textarea+Submit | Cursor никогда не формирует каталог доступных скиллов и сам декларирует «no host selector» → третья стадия конвейера — постоянный no-op из любой Cursor-поверхности. Cursor-специфичная само-декларация (vs P10 Claude empty-catalogue). | Подать реальный каталог (из repo skills index) в `availableSkills`; прогнать skill-selection с непустым входом; ablation, что это не позиционный lookup. |
| 4 | Webview-панель Cursor: авто-прогон зашитого промпта + UI-evidence до пользовательского ввода | SYNTHETIC AUTO-RUN / canned prompt | `extension.ts:207` зашитый prompt «A Cursor user opens…evidence.», `:208` core-запрос на открытии, `:218` `recordPanelUiEvidence(result,false)` до Submit; нет контролов метода/скиллов `:1131-1136` | Панель на открытии авто-гонит ЗАШИТЫЙ промпт и пишет UI-evidence ДО любого пользовательского ввода; реальный ввод только после Submit. Зеркалит Claude-smoke (P09/P10), но панель-специфично для Cursor и с записью synthetic UI-evidence. | Не авто-гнать canned-промпт как evidence; собирать UI-evidence только на реальном submit; убрать hardcoded prompt из инициализации панели. |
| 5 | Целостность конвейера: `DecideSkills` игнорирует выход task+plan | BROKEN PIPELINE HOP / stage-3 disconnected | `XiigenExtensionApiService.cs:130` стадия 2 = `Plan(BuildOrchestrationRequest(request, promptToTask.StructuredTask))` (берёт выход стадии 1); `:143` `DecideSkills(request, promptSha256, activeLoadState)` (не берёт ни task, ни plan); `:375` `SkillSelectionRequest.Prompt = request.UserPromptOrTaskText` (сырой промпт) | «task/plan → skills» разорван на третьем хопе: skill-selection работает от СЫРОГО промпта + host skill surface, а не от произведённых `StructuredTask`/`ScenarioPlan`. Конвейер не является настоящим prompt→task→plan→skills. Ново: P10 фиксировал flat-chain-не-через-15, но не то, что стадия 3 игнорирует выход стадий 1-2. | Подавать `StructuredTask`+`ScenarioPlan` во вход skill-selection; доказать зависимость выбора скиллов от task/plan (ablation: разные task→разные скиллы); при обёртке в `15.OrchestratorModel` смоделировать A-to-B поток стадий. |

---

## Вывод P14

Cursor — самая зрелая host-поверхность (реальные `activate()`, команды, chat
participant, webview-панель с submit-round-trip), но её КОНВЕЙЕР сильно урезан на
уровне хоста: (а) интерактивно достижима лишь 1 из 3 стадий-методов
(`prompt-to-task`); (б) `feedback`/`debug-trace` роняются `normalizeMethod` ещё до
ядра — петля online-обучения из Cursor недостижима by construction; (в) каталог
скиллов никогда не подаётся, хост сам декларирует «no host selector», так что
стадия 3 — постоянный no-op; (г) панель авто-гонит зашитый промпт как evidence до
ввода пользователя; (д) даже во внутренней цепочке ядра стадия 3 не потребляет
task/plan, т.е. это не настоящий prompt→task→plan→skills конвейер. Ни одна из этих
дыр не закрыта; ни один интерактивный Cursor-прогон полного трёх-стадийного
конвейера с непустым каталогом и семантическим выбором не доказан. Все выводы
привязаны к file:line живого кода.
