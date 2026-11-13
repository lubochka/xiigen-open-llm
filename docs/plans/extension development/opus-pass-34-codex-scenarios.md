# Pass P34 — Codex scenarios dual-path reality (по-сценарно Path A + Path B vs копии)

Дата: 2026-07-08. Ассессор: Opus 4.8. Срез: РЕАЛЬНЫЕ Codex Path A / Path B по 10
каноническим сценариям (board rows 19-28) + единственный Codex dual-path JOIN.
Только bounded reads живых артефактов в
`C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\`.

## Что реально проверено (файлы)

- Канонический board `product-readiness-scenario-board.json` строки 793-1136 — все
  10 Codex-рядов (19-28) целиком: статусы, joined_status, board_category, evidence_links,
  overclaim_locks, not_proven.
- Единственный Codex JOIN
  `scenario-execution/codex-same-scenario-dual-path-join-20260628-150753/{join-packet.json, verification.json}`.
- Path A `scenario-execution/codex-path-a-api-boundary-20260627-223748/{result.json, run-command.json, host-boundary-metadata.json}`.
- Path B `scenario-execution/codex-path-b-clean-ui-attempt-20260627-230843/{result.json, path-b-continuation-run.json}`
  (submit-нога) и `codex-path-b-response-capture-20260627-233415/{result.json, ui-state.json, verification.json}`
  (response-capture / re-photo).
- Per-scenario папки: `ls -1d frdp-p01-codex-*` (присутствуют 8: checkpoint-promotion-reload,
  codex-chat-ui-marker, full-extension-entrypoint, locator-update-reload,
  product-readiness-product-workflow, semantic-quality-heldout, token-savings-baseline,
  ui-input-surface-discovery-before-send). ОТСУТСТВУЮТ папки для learnability-training-continue (24),
  dpo-quality-provenance (25), asset-lifecycle-roundtrip (26).
- Row21 blocker `frdp-p01-codex-codex-chat-ui-marker/path-b-ui/row21-path-b-blocker.json`.
- Row19 `frdp-p01-codex-product-readiness-product-workflow/row19-status.json`.
- Проверка off-board: `grep -c full-extension-runtime product-readiness-scenario-board.json` = 0;
  `grep -c codex-full-extension-runtime ...` = 0.

## Ключевой вывод P34

Единственный Codex «joined» dual-path привязан к сценарию
`codex-full-extension-runtime-20260627-090817`, которого НЕТ среди 10 канонических Codex
scenario_id (grep=0). Его Path A — прямой in-process модуль-вызов из installed-cache копии,
обходящий поставляемый CLI (CLI дал ПУСТЫЕ skills из-за потери JSON-кавычек в PowerShell), с
подобранными строковыми HostFacts и само-ссылочным каталогом `["xiigen-smart-skills"]`. Его
Path B «accepted_visible_assistant_response_captured» ПРОТИВОРЕЧИТ собственному binding-review
submit-прогона, который классифицировал «visible response appeared» как OVERCLAIM (accepted:false,
PENDING_OR_TIMEOUT, Stop ещё виден). «Видимый ответ» в response-capture — это ре-фото окна без
нового промпта, а его текст — сам evidence-ledger XIIGen («Доказано/не доказано»,
`installed_skill_surface_loaded=true`, `TEXT_LEN_168`), не доказанный ответ модели Codex.
По board: 0/10 formal_joined_pass, 0/10 product_readiness_full_pass. «Accepted» Codex-ряды —
это принятый блокер (row21), 18-папочный ceremony при полном блоке (row23), staging/no-send без
Path B (rows 22/27/28), packaging-пакет без host-UI-действия (row19). 3 capability-ряда (24/25/26)
не имеют собственной Codex-папки вообще.

## НОВЫЕ дыры (не дублируют P17-P20 Codex, P31 инвентарь, P32/P33 Claude/Cursor)

### G1 — Codex JOIN off-board (0/10 канонических joined)
`join-packet.json:8` `scenario_id: codex-full-extension-runtime-20260627-090817`; board grep
`full-extension-runtime`=0. 10 канонических Codex scenario_id (rows 19-28) — все
`formal_joined_pass=false` (`:800,:865,:931,:992,:1011,:1091,:1117`). Единственный Codex
«same_scenario_dual_path_join» двигает off-28-board runtime-пробу; 10 типовых capability = 0
joined pass. Codex-аналог P32 gap-1 (Claude off-board) / P33 gap-1 (Cursor off-board), но свой
scenario_id и своя папка. Закрытие: привязать join к каждому из 10 канонических scenario_id
(тот же промпт из scenario-prep); off-board join не засчитывать; поднять Codex joined pass с 0.

### G2 — Codex JOIN Path A: обход поставляемого CLI + подделанные HostFacts + само-каталог
`codex-path-a-api-boundary-.../result.json` `technical_attempts`: (1) «CLI invocation … core
succeeded but skills were empty due Windows PowerShell CLI JSON quote loss»; (2) «direct module
… core rejected boolean HostFacts.samePrompt because core expects string»; (3) «final direct
module invocation with string host facts = accepted». `join-packet.json:path_a.route_used`
«direct exported Codex adapter function with string host facts», `ui_used:false`. `run-command.json`
`workingDirectory: C:\Users\lubao\.codex\plugins\cache\xiigen-local\...\0.1.0+codex.runtime04e-...`
(installed-cache копия). `available_skills=selected_skills=["xiigen-smart-skills"]` (плагин
выбирает сам себя). Отдельно от P17 gap-7 (cache-copy runtime, в host-репо), P18 gap-2
(само-каталог, в TOKEN-03), P33 gap-2 (Cursor in-process): здесь именно accepted dual-path JOIN
Path A на shipped CLI ПРОВАЛИЛСЯ (пустые skills), «PASS» получен обходом CLI + coerced string
HostFacts. Закрытие: прогнать Path A через настоящий поставляемый Codex CLI/host-путь с
непустым skills-каталогом из host-индекса; не засчитывать direct-module + рукотворные string
HostFacts как host Path A.

### G3 — Codex JOIN Path B accepted ПРОТИВОРЕЧИТ overclaim-repair submit-прогона
Submit-нога `codex-path-b-clean-ui-attempt-.../result.json`: `path_b.accepted:false`,
`verdict: PARTIAL_SUBMITTED_RESPONSE_PENDING_OR_TIMEOUT`, `visible_response_observed:false`,
`final_stop_button_still_visible:true`, `overclaim_repair.old_claim:"visible Codex UI response
appeared"→classification:overclaim`, `still_needed:"Capture visible assistant response …"`. (Причём
внутренний `path-b-continuation-run.json` писал `pathBAccepted:true` — тоже переопределён review в
accepted:false.) JOIN `join-packet.json:path_b.status:"accepted_visible_assistant_response_captured"`,
`visible_assistant_response_observed:true`, ссылаясь на ОТДЕЛЬНУЮ папку response-capture с
`new_prompt_submitted_by_response_capture:false`. Join разворачивает binding overclaim-repair в
принятие. Отдельно от P33 gap-3 (Cursor stale — там не было явного overclaim-repair, который
переворачивают). Закрытие: один свежий typed→submit→НОВЫЙ видимый ответ в одной сессии; join не
может принимать Path B, если исходный submit-прогон помечен overclaim/pending/timeout.

### G4 — «Видимый ответ» Codex Path B = сам evidence-ledger XIIGen, провенанс модели не доказан
`codex-path-b-response-capture-.../ui-state.json` visibleTexts: «Доказано:
installed_skill_surface_loaded=true, adapter_core_boundary_executed=true,
installed_plugin_runtime_invocation_proven=true», «Не доказано: full_codex_chat_ui_proven=false,
token_savings_proven=false, quality_improvement_proven=false, learnability_proven=false,
training_started=false, eval_started=false, checkpoint_mutation_started=false», «Evidence run:
TEXT_LEN_168» + ListItems «task-to-plan…/task-to-skills…/documentation-coverage-review…».
`result.json:new_prompt_submitted:false` (ре-фото). Т.е. «visible assistant response» — это
собственный статус-леджер/выход XIIGen, вставленный в композер и сфотографированный, а не
доказанная генерация модели Codex/GPT; UIA-текст-оракул, сверяющий со свежей генерацией,
отсутствует. Отдельно от P32 gap-4 (Claude нативный ответ модели): здесь текст = вокабуляр
самого пакета (циклично, вход≈выход). Закрытие: снять ТЕКСТ настоящего ответа модели после
свежего submit; сверить с prompt-hash; не засчитывать re-photo status-ledger как ответ ассистента.

### G5 — Row21 (10-й тип chat-ui-marker): блокер принят как pass
`board:926-983` board_category `scoped_accepted`, `joined_status: PASS_SCOPED_BLOCKER`.
`row21-path-b-blocker.json` `blocker_class:UI_INPUT_CONTROL_NOT_FOUND`, `prompt_typed:false`,
`prompt_pasted:false`, `prompt_sent:false`, «no confirmed composer/input control was exposed».
Единственный «видимый Codex chat UI» тип (лишний 10-й, P31 gap-6) «принят» тем, что композер не
найден и ничего не введено/отправлено — блокер как scoped-pass, ноль UI-взаимодействия. Отдельно
от P20 gap-6 (там weak-negative в HOST-IMPL). Закрытие: не засчитывать UI_INPUT_CONTROL_NOT_FOUND
как accepted; найти подтверждённый композер и провести реальный typed→send→visible marker.

### G6 — Row23 semantic-quality: 18-папочный ceremony при полном блоке, но board_category «scoped_accepted»
`board:1005-1035`: 18 evidence-link папок (status, status-update, followup, evidence-path-design,
runner-contract, heldout-blocker, authority-input-intake, blocker-packaging-status …), но
фактически `automated_core_api_status`: «PASS_SCOPED_BLOCKER … BLOCKED_NO_APPROVED_INPUT_PATH.
Approved Row23 input paths found: 0 … No Row23 semantic-quality run, heldout pass, host-visible
Codex answer … is proven»; `board_category: scoped_accepted`. 18 process-пакетов, 0 семантического
deliverable — ceremony-over-work на уровне одного сценария; категория «scoped_accepted» завышает
полностью заблокированный ряд. Отдельно от P28 (semantic reality на pipeline-уровне). Закрытие:
переклассифицировать row23 в blocked_no_input; получить 1 одобренный held-out вход с провенансом;
прогнать реальный scorer/leakage-gate; не считать 18 blocker-папок прогрессом покрытия.

### G7 — Rows 24/25/26 не имеют собственной Codex-папки; ссылаются на общие phase-03/04 артефакты
`ls -1d frdp-p01-codex-*` = 8 папок; НЕТ папок для learnability-training-continue (24),
dpo-quality-provenance (25), asset-lifecycle-roundtrip (26). Их evidence_links: row24
(`:1046-1048`) → `phase-03-command-output/07-blocker-audit`, `phase-03-automated-boundary-report`,
`phase-04-claude-codex-route-establishment`; row25 (`:1062-1064`) →
`phase-03-command-output/05-dotnet-dpo-dataset`, `07-blocker-audit`; row26 (`:1077-1080`) →
`07-blocker-audit`, `phase-04`. Это ОБЩИЕ кросс-хостовые фазовые артефакты, не Codex-сценарные
Path A/Path B. 3 из 10 Codex capability-сценариев не имеют ни одного собственного прогона; их
«evidence» — generic pipeline-blocker-audit. Отдельно от P33 gap-6 (Cursor rows 14-18 — там были
temp-folder core-прогоны). Закрытие: создать Codex-сценарные папки для 24/25/26 с реальными
non-mutating Path A + Path B; не засчитывать общие phase-03/04 blocker-audit как Codex per-scenario
evidence.

### G8 — Row19 «JOINED_COMPARISON_PASS» — packaging-пакет без host-UI-действия и без видимого ответа
`row19-status.json:8` `joined_comparison_verdict: ROW19_JOINED_COMPARISON_PASS_PACKAGE_ROW19`,
но `path_b:22-24` `same_row19_codex_ui_thread_correlation_captured:true`,
`non_mutating_user_visible_response:true`, `full_final_response_uia_readback_fully_screenshot_visible:false`;
`overclaim_locks.host_ui_action_by_this_packet:false`; `caveats:29` «this packaging packet did not
run host UI actions». Единственный Codex-ряд с меткой «JOINED_COMPARISON_PASS» на деле — packaging
packet, не выполнявший host-UI-действий, с не-видимым полностью ответом; `formal_joined_pass=false`.
Плюс rows 22/27/28 `scoped_accepted` на staging/no-send Path A с Path B `not_proven`/blocked
(`:991,:1090,:1116`) — ни один accepted Codex-ряд не несёт настоящего dual-path с видимым host-ответом.
Отдельно от P31 gap-1 (агрегатный 0/28). Закрытие: не выдавать packaging/label «JOINED_COMPARISON_PASS»
за joined pass; требовать реальное host-UI-действие + видимый ответ + A↔B сравнение на один prompt-hash.

## Границы данного прохода (честно)

- Не полностью читал 8.9MB session.md (по инструкции). Читал только целевые board-ряды и
  scenario-execution артефакты.
- Скриншоты (`*.png`) не открывал попиксельно; провенанс «видимого ответа» оценён по UIA-текстовому
  дампу `ui-state.json` и по флагам `new_prompt_submitted:false` / overclaim_repair, а не по картинке.
- Проверка «off-board» — точный grep по каноническому board; отсутствие папок 24/25/26 — точный `ls`.
- Не запускал ни build/test/runtime, ни git; оценка чисто по артефактам на диске (evidence-bound).
