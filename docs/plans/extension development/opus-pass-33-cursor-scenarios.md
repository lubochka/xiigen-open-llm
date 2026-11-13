# Pass P33 — Cursor scenarios dual-path reality (по-сценарно Path A + Path B vs копии)

Дата: 2026-07-08. Ассессор: Opus 4.8 (real-state). Срез: **Cursor**, по-сценарно реальные Path A / Path B
против канонического 28-рядного board (Cursor = ряды 10-18) и единственного Cursor dual-path JOIN.
Аналог P32 (Claude scenarios), но для Cursor. Все выводы — от живых артефактов
`C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\`.

## Что реально проверено (bounded reads живых артефактов)

1. **Канонический board** `product-readiness-scenario-board.json`, ряды 10-18 (Cursor):
   - row10 `frdp-p01-cursor-product-readiness-product-workflow` — `board_category:not_moved` (`:322`);
     Path A «insufficient», Path B «Not run locally … existing Cursor evidence is only row 11 marker-only» (`:319-321`).
   - row11 `frdp-p01-cursor-full-extension-entrypoint` — `partial` (`:338`); Path A: panel-команда из локального
     Cursor Extension Host, marker/correlation через extension→core, **но core response пустой** (`:335`, mojibake);
     Path B: «clean narrow visible evidence» — скриншот реального Cursor с XIIGen-панелью **без промпта/ответа**
     (`:336`, run-20260625T000253). `formal_joined_pass=false` (`:337`).
   - row12 `frdp-p01-cursor-token-savings-baseline` — `partial` (`:373`); Path A baseline-only, token delta 0,
     reduction false (`:370`); Path B: real Cursor webview, текст введён, **scoped `HTMLElement.click` submit (не ручной клик)**,
     core вызван, результат отрисован; «Manual human click is not proven» (`:371`). `formal_joined_pass=false` (`:372`).
   - row13 `frdp-p01-cursor-semantic-quality-heldout` — `accepted_narrow_scoped` (`:444`); Row13.1 staged candidate
     0/4→4/4, Row13.2 **proxy permutation 12/12 с оговоркой «exact held-out prompt text was unavailable»** (`:441`,`:486`),
     Row13.3d real Cursor UI **TargetLocator** выбрал output-skepticism (`:442`, другой промпт, не held-out).
   - row14 `…learnability-training-continue` — `accepted_narrow_scoped` (`:546`); Row14.1-14.4 всё core-only,
     **«No UI run is claimed»** (`:544`).
   - row15 `…dpo-quality-provenance` — `scoped_accepted` (`:573`); non-mutating audit, **«Not run locally»** (`:571`).
   - row16 `…asset-lifecycle-roundtrip` — `partial` (`:636`); core roundtrip PASS, **«Not run locally»** (`:634`),
     packaging review pending.
   - row17 `…checkpoint-promotion-reload` — `partial` (`:691`); **temp-only** core (`:688`), **«Not run locally»** (`:689`),
     packaging pending_current.
   - row18 `…locator-update-reload` — `partial` (`:745`); **temp-run-folder-only** core (`:742`), **«Not run locally»** (`:743`).

2. **Единственный Cursor dual-path JOIN** `scenario-execution/cursor-same-scenario-dual-path-join-20260628-150216/`
   (`join-packet.json`, `verification.json`, `README.md`):
   - `same_scenario.scenario_id = "cursor-full-extension-runtime-after-unlock-20260627-090159"` (`:8`) —
     **OFF-BOARD**: `grep -c full-extension-runtime` по board = **0**; 9 канонических Cursor рядов используют
     `full-extension-entrypoint`, а «full-extension-runtime» вообще НЕТ в 28-рядном board.
   - `verification.json` только JSON-parse + `git diff --check` самой папки join (`:5-15`) — бумажная сверка,
     не проверяет ни host-route Path A, ни причинность Path B.

3. **Path A leg** `cursor-path-a-api-boundary-20260627-235243/{result,verification}.json`:
   - `route_used:"direct XiigenExtensionRequest API/core boundary fallback"` (`:17`),
     `exact_cursor_adapter_cli_used:false` (`:18`), `ui_used:false` (`:21`) — Path A **обошёл само расширение**,
     это in-process core-fallback (тот же, что в тестах).
   - `selected_skills:[prompt-to-task, orchestration-reasoning, skill-selection, api-dashboard-qa]` (`:28-33`),
     но `response_status:"core_boundary_partial_no_semantic_quality_claim"` (`:24`).
   - `path_b_current.status:"blocked_current_storage_modal"`, `prompt_submitted:false`, `response_visible:false`
     (`:37-43`) — **на момент Path A (06-27 23:59) его собственная Path B-нога была заблокирована** Storage Error modal.
   - `verification.json:44` `git_diff_check.exit_code_recorded_in_this_file:null` (код выхода отложен на «финальный отчёт»).

4. **Path B legs** (сшиты в join):
   - `cursor-path-b-prompt-response-after-panel-input-20260628-034910/result.json`: `host:"Cursor Extension Development Host"` (`:3`),
     `prompt_typed:true`,`submit_clicked:true` (`:7-8`), но **`visible_response_seen:false`** (`:9`) и явно:
     «Screenshots 01 and 02 **already show the Core API response block BEFORE this submit**, so a new final core
     answer caused by this submit is **not proven**» (`:10`); `claims_not_made` включает «final core answer caused by this submit» (`:24`).
   - `cursor-path-b-final-response-debug-20260628-040425/result.json`: **`no_new_prompt_submitted:true`** (`:9`),
     `visible_final_response_seen:true` (`:12`) — но это **пере-фотография существующей панели** (`:4` «existing … window screenshot»,
     `:26` «Existing Cursor panel visibly shows the post-submit final response text»); windowTitle `[Extension Development Host] … Cursor` (`:11`);
     **`uia_capture_error_if_any:"Argument types do not match"`** (`:15`) — текст ответа машинно НЕ снят;
     `runtimeRefs` (coreResponse/hostTrace/entrypointResponse) все под
     `C:\Users\lubao\AppData\Roaming\Cursor\User\globalStorage\xiigen.cursor-xiigen-smart-skills\…` (`:17-24`) — **вне коммитнутого пакета**.

5. **Off-board runtime-сценарий** `scenario-execution/frdp-p01-cursor-full-extension-runtime/` содержит только
   `cursor-preflight-*` / `cursor-session-preflight-*` — execution-report/response НЕТ (preflight-only, брошен).

## Ключевой вывод P33

Единственный Cursor «JOINED» dual-path — **off-board** проба сценария `full-extension-runtime-after-unlock`,
которого нет ни среди 9 канонических Cursor рядов, ни в 28-рядном board. Его **Path A обходит расширение**
(in-process core fallback, `exact_cursor_adapter_cli_used:false`), а **Path B не доказывает причинность**:
submit-нога сама пишет, что блок ответа был на экране ДО submit, а final-response-debug не шлёт нового промпта
и пере-фотографирует старую панель. Ноги **временно разорваны** (Path A писал path_b=blocked; Path B из другого дня),
payload ответа лежит в **volatile user globalStorage вне пакета**, текст ответа **не снят** (UIA fail). Для
**5 из 9** Cursor capability-сценариев (ряды 14-18) **вообще нет Path B UI** — только temp-folder core. Все 9
канонических Cursor рядов остаются **0 full joined pass**.

## Дедуп (что НЕ повторяю)

- P06 — три join-пакета «бумажные» / token synthetic / pixel вне пакета (общий уровень, без по-сценарной сверки join↔board).
- P13 — Cursor entrypoint: powershell-транспорт, absolute-path defaults, settings drift, chat-unstable, non-reproducible build, divergent Documents-копия.
- P14 — конвейер 1/3, panel auto-run canned prompt, stage-3 disconnect, feedback/debug-trace drop.
- P15 — token accounting (heuristic, arch-disconnect), baseline≡adaptive no independent variable, quality-guard coupled-to-notready.
- P16 — Cursor QA: осиротевший suite, **headless extensionTests** Path B (RUNTIME-03, 0 скринов), wrong-host VS Code substitute,
  phase10 unrun, no correctness oracle, panel `panel_case:null` в RUNTIME-03, provisional verdicts. **Здесь другой прогон**:
  scenario-execution Path B (06-28), где скрины ЕСТЬ и host = Extension Development Host (не headless), а новые дефекты — stale-response + dev-host + globalStorage.
- P31 — инвентарь/board-счётчики, A↔B comparison-поле отсутствует (board-level), mojibake рядов 6/11/15, host×type асимметрия.
- P32 — Claude scenarios (off-board generic join, fabricated skills catalogue via node-CLI, A↔B paper mapping, native-app Path B, broken UIA composer). **Здесь Cursor-специфика с другими механизмами.**

## Новые дыры P33 (6)

| Component | Real-status | Evidence file:line | Gap | Closure steps |
|---|---|---|---|---|
| Cursor dual-path JOIN scenario vs 9 канонических Cursor-рядов | COVERAGE DISJUNCTION / off-board join, 0/9 typed | `cursor-same-scenario-dual-path-join-20260628-150216/join-packet.json:8` scenario_id `cursor-full-extension-runtime-after-unlock-20260627-090159`; board `grep -c full-extension-runtime`=0; board rows 10-18 все `formal_joined_pass=false` (`:337,:372,:443…`); off-board folder `frdp-p01-cursor-full-extension-runtime/`=preflight-only | Единственный Cursor «same_scenario_dual_path_join» привязан к сценарию, которого НЕТ среди 9 канонических `frdp-p01-cursor-*` scenario_id и вообще нет в 28-рядном board (там `full-extension-entrypoint`, не `full-extension-runtime`). 9 типовых Cursor capability = 0 joined pass; join двигает off-board runtime-пробу (у которой есть лишь preflight-папки). Отдельно от P32 gap-1 (Claude off-board generic) и P31 (board-счётчики) — здесь по-сценарная сверка join↔board для Cursor + факт off-28-board. | Привязать dual-path join к КАЖДОМУ из 9 канонических `frdp-p01-cursor-*` scenario_id (тот же промпт из scenario-prep); off-board runtime-join не засчитывать в покрытие 9; довести Cursor joined pass с 0. |
| Cursor JOIN Path A route (через расширение) | NOT-HOST-ROUTE / in-process core fallback, adapter CLI unused | `cursor-path-a-api-boundary-20260627-235243/result.json:17` `route_used:"direct XiigenExtensionRequest API/core boundary fallback"`, `:18` `exact_cursor_adapter_cli_used:false`, `:21` `ui_used:false`; `join-packet.json:19` `exact_cursor_adapter_cli_used:false` | «Path A через границу расширения» на деле — прямой in-process вызов core (тот же fallback, что и в тестах), НЕ через поставляемый Cursor `extension.ts`→`powershell` adapter. Path A не воспроизводит реальный host-путь Cursor; расширение в петле Path A отсутствует. Отдельно от P32 gap-2 (Claude node-CLI + fabricated `-AvailableSkills`) и P13 gap (powershell-транспорт) — здесь именно `exact_cursor_adapter_cli_used:false` в accepted join. | Прогнать scenario Path A ЧЕРЕЗ поставляемый Cursor adapter CLI/extension boundary (не in-process fallback); не засчитывать `exact_cursor_adapter_cli_used:false` как host Path A; связать Path A с тем же extension-путём, что и Path B. |
| Cursor JOIN Path B: видимый ответ СТАРЫЙ, не вызван submit | STALE RESPONSE / no submit→response causation | `cursor-path-b-prompt-response-after-panel-input-20260628-034910/result.json:9` `visible_response_seen:false`, `:10` «Screenshots 01 and 02 already show the Core API response block **before** this submit, so a new final core answer caused by this submit is not proven», `:24` claims_not_made «final core answer caused by this submit»; `cursor-path-b-final-response-debug-20260628-040425/result.json:9` `no_new_prompt_submitted:true`, `:12` `visible_final_response_seen:true`, `:26` «Existing Cursor panel visibly shows the post-submit final response text» | Обе Path B-ноги join НЕ доказывают причинность prompt→visible-response: submit-нога сама пишет, что блок ответа был на экране ДО submit (новый ответ не доказан), а final-response-debug НЕ шлёт нового промпта и пере-фотографирует существующую панель со старым ответом. `visible_final_response_seen:true` в join собран из re-photo устаревшей панели. Отдельно от P16 (headless, 0 скринов) и P32 gap-5 (Claude broken composer) — Cursor-специфично: submit-нога явно опровергает причинность, а «успех» = re-photograph. | Один свежий прогон typed→submit→НОВЫЙ видимый ответ в ОДНОЙ сессии с очищенной панелью; не засчитывать re-photograph существующего ответа; привязать видимый ответ к prompt-hash отправленного текста (сверка A↔B по хешу). |
| Cursor JOIN: ноги временно разорваны / сшиты из разных сессий | STITCHED-ACROSS-SESSIONS / not one coherent run | `cursor-path-a-api-boundary-20260627-235243/result.json:37-43` `path_b_current.status:"blocked_current_storage_modal"`,`prompt_submitted:false`,`response_visible:false` (Path A создан 06-27T23:59, `:4`); Path B submit-нога 06-28T03:49 (folder `…034910`), final-response-debug 06-28T04:04 (`…040425`); `join-packet.json:4` join создан 06-28T15:02 | Path A и Path B никогда не выполнялись как один связный прогон одного сценария: на момент Path A (06-27 23:59) его СОБСТВЕННАЯ Path B-нога была ЗАБЛОКИРОВАНА (Storage Error modal, промпт не отправлен), а join через сутки сшивает ОТДЕЛЬНЫЙ Path B (06-28). «Same-scenario dual-path» собран из разрозненных сессий/дней. Ново (P32/P31 этого temporal-disjoint факта не фиксировали). | Выполнять Path A и Path B в ОДНОЙ связной сессии/окне для одного prompt-hash; запретить сшивку ног из разных дней/сессий как «same-scenario»; требовать поле session-id/temporal-proximity в join. |
| Cursor Path B host + provenance ответа | DEV-HOST / EPHEMERAL STORAGE / NO TEXT ORACLE | `cursor-path-b-prompt-response-after-panel-input-20260628-034910/result.json:3` `host:"Cursor Extension Development Host"`; `cursor-path-b-final-response-debug-20260628-040425/result.json:11` windowTitle `[Extension Development Host] … Cursor`, `:15` `uia_capture_error_if_any:"Argument types do not match"`, `:17-24` runtimeRefs coreResponse/hostTrace/entrypointResponse под `C:\Users\lubao\AppData\Roaming\Cursor\User\globalStorage\xiigen.cursor-xiigen-smart-skills\…` | «Реальный Cursor UI Path B» — это debug-сессия РАСПАКОВАННОГО расширения через Extension Development Host (F5 dev-load `--extensionDevelopmentPath`), не установленный `.vsix`-продукт; реальный payload ответа (core-response/host-trace/entrypoint-response) лежит в VOLATILE user `globalStorage` ВНЕ коммитнутого evidence-пакета; UIA-снятие текста провалилось → в пакете только скриншот существующей панели, текст ответа машинно НЕ captured (нет correctness-oracle). Отдельно от P16 (headless extensionTests) и P13 (build) — здесь dev-host + эфемерное хранилище + отсутствие текстового oracle. | Прогнать Path B на УСТАНОВЛЕННОМ Cursor `.vsix`-расширении (не dev-host); копировать core-response/trace payload в коммитнутый пакет; починить UIA text-capture и снять ТЕКСТ ответа для сверки с промптом (golden-oracle). |
| Cursor rows 14-18 (5 capability-сценариев): 0 Path B UI, только temp-folder core | SINGLE-PATH / Cursor Path B 0/5 lifecycle+learning | board `product-readiness-scenario-board.json` row14 `:544` «No UI run is claimed», row15 `:571` «Not run locally … no host UI readiness», row16 `:634` «Not run locally … no host UI/product readiness», row17 `:689` «Not run locally for checkpoint reload» + `:688` «temp-only», row18 `:743` «Not run locally for locator-selected candidate» + `:742` «temp-run-folder-only» | 5 из 9 Cursor capability-сценариев (learnability 14, dpo 15, lifecycle 16, checkpoint 17, locator 18) не имеют НИ ОДНОГО локального Path B UI-прогона — все «scoped accepted» на temp-folder core-only; dual-path для них фактически single-path (только Path A ядро). Даже 3 сценария с UI-ногой (11 marker-only, 12 stale/programmatic-click, 13 substitute-prompt TargetLocator) не дают full joined. Cursor Path B UI-покрытие capability = 0/5. Отдельно от P31 gap (Claude 0/9 / board-счётчики) и P16 (QA-упряжь) — здесь по-Cursor-capability отсутствие Path B + temp-folder-only ядро. | Для rows 14-18 добавить реальный Cursor UI Path B (видимый прогон восстановленного/обученного/промотированного поведения) на НЕ-temp путях; поднять Cursor Path B capability-coverage с 0/5; не засчитывать temp-run-folder core как dual-path. |
