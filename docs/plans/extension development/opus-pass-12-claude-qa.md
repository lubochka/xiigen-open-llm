# Pass P12 — Claude: QA + dual-path same-scenario (реальное состояние)

Честный фрейм: этот проход оценивает РЕАЛЬНУЮ QA-инфраструктуру и dual-path
(Path A автоматический boundary + Path B локальный Claude-desktop) доказательства
именно для Claude-расширения, по живому коду/артефактам, а не по copy-status
пакета. Дедуп: не повторяю P09 (entrypoint/settings/core-API), P10
(prompt→task→orch→skills конвейер), P11 (adaptive/lifecycle), P06 (package
dual-path join — бумажный), P08 (build/test asymmetry репо). Здесь — конкретно
QA-гейт и dual-path Path B по Claude.

## Что проверено (bounded reads)

Реальный Claude host-репо `C:\Projects\claude-xiigen-smart-skills`:
- `reports/HOST-08E-claude-hook-qa.json` (единственный QA-отчёт, прочитан целиком);
- `reports/RUNTIME-02-claude/claude-cli/*` (реальные прогоны `claude.exe` 2.1.170:
  command.json, command-error.json, positive/error stdout-стримы);
- `reports/RUNTIME-02-claude-live-runtime/.../runtime-02-claude-live-runtime-mirror.json`
  (verdict live-runtime);
- QA-выход в `C:\Projects\llm_mvp_core\artifacts\host-qa\HOST-08E\20260606-213928\*`
  (response.json/stdout.log — существуют, кросс-репо);
- `package.json` (scripts/devDependencies), `README.md` (honesty), структура `reports/`;
- пакет `extension development\supporting-files\*` (поиск claude-QA / ux-automation / cycle → 0 файлов).

## Ключевые факты

1. **QA-гейт Claude = ровно один отчёт `HOST-08E-claude-hook-qa.json`**, и он сам
   декларирует узкую область: `claude_cli_plugin_qa:false` (`:11`),
   `full_chat_ui_evidence:false` (`:12`), `ps1_accepted_as_implementation:false`.
   `hook_command` (`:15`) — это `node.exe "…\src\claude-entrypoint.js" -ConcreteTask … -Prompt … -CoreRoot …`,
   то есть **прямой вызов entrypoint через node.exe (Path A boundary), а не команда
   плагина, загруженного в Claude**. Все 4 кейса (prompt-to-task off/on,
   orchestration-plan, skill-selection) `exit_code:0`, `response_success:true`,
   `response_status: core_boundary_partial_no_semantic_quality_claim`. Поля
   `semantic_quality_claimed` в кейсах ПУСТЫ (`:72/:94/:116/:138` = `""`) — QA даже
   не ассертит семантику. Ассерты = `response_success`, `debug_shape_present/absent`,
   `sentinel_absent_from_display_response_trace`, `request_contains_prompt_by_contract`.
   Реальный QA-выход `…/prompt-to-task-debug-off/response.json`:
   `Status: core_boundary_partial_no_semantic_quality_claim`,
   `prompt_to_task_status: loaded_state_no_matching_learned_pattern`,
   `semantic_quality_claimed:false`. То есть даже «успешный» кейс = «загрузил
   состояние, совпадающего паттерна нет» — QA проверяет ПЛУМБИНГ тонкого адаптера
   (граница выполнилась, форма/redaction/exit-code), а не корректность/семантику ответа.

2. **Нулевое негативное покрытие.** Все 4 кейса — happy-path (`response_success:true`),
   `gaps:[]`. Нет кейсов: отсутствующий/битый core-root, невалидный метод, пустой
   каталог скиллов, известный рантайм-сбой `ORCHESTRATION_REASONING_MANIFEST_CHECKPOINT_REF_MISMATCH`
   (зафиксирован P11). Graceful-degradation / untrained-honesty не тестируются.

3. **Единственный реальный Path B прогон (`claude.exe`) завершился ошибкой и провалил
   собственный маркер, но помечен PASS.** `runtime02-positive-verbose-20260616-204952.stdout.jsonl`:
   SessionStart-hook реально сработал в настоящем Claude и выдал
   `hook_response.output = "Core xiigen boundary executed. Semantic quality is not claimed.\nxiigen prompt-to-task :\ninput: <redacted>\noutput: <redacted>\n"`;
   НО итоговый `result` = `subtype:"error_max_budget_usd"`, `is_error:true`,
   `errors:["Reached maximum budget ($0.05)"]`; модель ответила
   `RUNTIME_02_CLAUDE_VISIBLE_SMOKE`, тогда как `command.json` требовал «Reply with
   exactly **XIIGEN_RUNTIME_02_VISIBLE**» (grep этого маркера в стриме = 0).
   При этом `runtime-02-claude-live-runtime-mirror.json` штампует
   `verdict: PASS_NARROW_CLAUDE_CODE_PLUGIN_LOAD_AND_HOOK_CORE_BOUNDARY_RUNTIME_QA`.
   Verdict переоценивает stream: реально доказано лишь «плагин загрузился через
   `--plugin-dir` + hook сработал», а прогон терминировался ошибкой и маркер не совпал.

4. **Path B у Claude ограничен фиксированной hook-строкой — same-scenario нет.** Вывод
   hook’а — константа, не зависящая от `-p` промпта пользователя; ответ модели с xiigen
   не связан. `mirror.json scope`: «Claude Code --plugin-dir plugin load plus local
   SessionStart hook/core-boundary path only; full Claude chat UI/Appx proof remains
   unproven». `README.md:77`: «Full interactive Claude host runtime is not proven».
   Значит Path A (boundary тонкого адаптера) и Path B (hook) проверяют РАЗНЫЕ вещи —
   нет ни одного сценария, где XIIGen меняет ВИДИМЫЙ ответ Claude на вопрос пользователя,
   т.е. dual-path «same-scenario» по определению не собирается.

5. **UX-automation для Claude отсутствует и неприменима.** В пакете 0 файлов
   claude-QA/ux-automation/review-cycle; UX-automation упоминается только для
   dashboard/правил CLAUDE.md. Claude-манифест (`.claude-plugin/plugin.json`) — только
   SessionStart hook, 0 пользовательской UI-поверхности (P09), поэтому UX-слой
   (accessible names/help/examples/visible-error/stable layout по AR-22 / Plan
   testability UX-layer) физически негде прогнать.

6. **QA невоспроизводима как гейт.** `package.json:45` `devDependencies:{}`,
   scripts={install-smoke} (нет `test`); нет `test/`-каталога, нет jest/mocha/vitest;
   выход QA пишется в `C:\Projects\llm_mvp_core\artifacts\host-qa\…` — ДРУГОЙ репо,
   абсолютные пути, не self-contained; дата 2026-06-06 (месяц назад vs 2026-07-08).

## Коррекция мастера (нюанс, не отмена дыр)

RUNTIME-02 — это реальный, более поздний (Jun 16-17) прогон настоящего `claude.exe`,
где плагин ГРУЗИТСЯ через `--plugin-dir` и SessionStart-hook РЕАЛЬНО срабатывает и
доходит до core-boundary. Это сильнее, чем phase40 echo-run (P08/P09). Но это по-прежнему
hook-load-boundary, а не user prompt→xiigen-answer, и единственная попытка добавить
видимый ответ модели дала error+marker-mismatch. Так что honesty-граница пакета
(«full interactive Claude host runtime not proven») остаётся верной.

## НОВЫЕ дыры P12 (Claude QA + dual-path)

| Component | Real-status | Evidence file:line | Gap | Closure |
|---|---|---|---|---|
| Единственный Claude QA-гейт HOST-08E = Path-A boundary-only (node.exe), не плагин в Claude; ассертит плумбинг, не корректность | BUILT_NOT_PROVEN / boundary-only QA | `reports/HOST-08E-claude-hook-qa.json:11-12` `claude_cli_plugin_qa:false/full_chat_ui_evidence:false`, `:15` hook_command=node.exe direct, кейсы `:72/:94/:116/:138` `semantic_quality_claimed:""`; QA `…/response.json` `prompt_to_task_status:loaded_state_no_matching_learned_pattern` | Единственная Claude «QA-автоматизация» гоняет entrypoint через node.exe (Path A), сама декларирует, что это не QA плагина в Claude и не chat UI; проверяет форму/redaction/exit, а не ответ (даже «успех»=no-match) | QA через реальную plugin-команду в Claude (Path B) + ассерты корректности на известных входах; node.exe-boundary не засчитывать как host-QA |
| HOST-08E не имеет негативных/ошибочных кейсов (happy-path-only) | STUB QA-COVERAGE | `HOST-08E-claude-hook-qa.json:64-153` все 4 кейса `response_success:true/exit_code:0`, `:171 gaps:[]`; нет кейса checkpoint-mismatch (P11) / bad core / bad method / empty catalogue | QA проверяет только успешные пути; нет негативных/деградационных ассертов и regression на известный сбой; untrained-honesty/graceful-degradation не тестируются | Добавить негативные кейсы (missing/invalid core, bad method, empty catalogue, checkpoint mismatch) с ассертом честного degraded-ответа |
| Единственный реальный claude.exe Path B прогон завершился error+marker-mismatch, но помечен PASS | OVERCLAIMED-PASS | `reports/RUNTIME-02-claude/claude-cli/runtime02-positive-verbose-20260616-204952.stdout.jsonl` result `subtype:error_max_budget_usd,is_error:true`; ответ `RUNTIME_02_CLAUDE_VISIBLE_SMOKE` vs запрос `XIIGEN_RUNTIME_02_VISIBLE` (grep=0); `RUNTIME-02-claude-live-runtime/.../mirror.json` `verdict:PASS_NARROW_...` | Verdict переоценивает stream: прогон терминировался `error_max_budget_usd` и маркер-ассерт провалился; error-run выдан за PASS | Переклассифицировать в honest-narrow (только plugin-load+hook-fire, run errored); перегнать с адекватным бюджетом и корректным маркером; не штамповать error/mismatch как PASS |
| Реальное Claude Path B = фиксированная SessionStart hook-строка; нет same-scenario, где XIIGen меняет видимый ответ | SCENARIO GAP / no same-answer dual-path | positive stream `hook_response.output`=константа «Core xiigen boundary executed…» (не зависит от `-p`); `mirror.json scope`=«SessionStart hook/core-boundary path only; full Claude chat UI…unproven»; `README.md:77` | Path A (boundary) и Path B (hook) проверяют разные вещи; ни один сценарий не показывает XIIGen-улучшенный видимый ответ Claude на вопрос пользователя ⇒ dual-path same-scenario не собирается по построению | Ввести user-driven команду/skill в Claude с видимым ответом; снять Path A и Path B на ОДНОМ сценарии с одним проверяемым ответом |
| Нет UX-automation QA для Claude host-поверхности | MISSING QA-LAYER | пакет `find *claude*qa*/*ux-automation*`→0; `.claude-plugin/plugin.json` только SessionStart hook (0 UI, P09); AR-22 / Plan testability UX-layer требуют UX-автоматизацию | Требуемый UX-слой (accessible names/help/examples/visible-error/stable layout/safe raw-debug) для Claude не существует и не может, пока нет пользовательской UI-поверхности | Сначала ввести Claude user-facing surface, затем UX-automation по AR-22; зафиксировать метки/help/ошибки/layout |
| Claude QA невоспроизводима как гейт: нет тест-фреймворка/CI, выход кросс-репо по абсолютным путям, устарел | NON-REPEATABLE QA / no harness | `package.json:45` `devDependencies:{}`, scripts={install-smoke} (нет `test`); нет `test/`/jest/mocha/vitest; `HOST-08E:8` `run_root:"C:\Projects\llm_mvp_core\artifacts\host-qa\..."`; дата 2026-06-06 | Единственный QA-гейт — ad-hoc PowerShell/JS proof, вывод пишется в другой репо (не self-contained, транзиентно), без раннера/CI; нельзя перезапустить как повторяемый gate внутри host-репо, устарел на месяц | Ввести node --test/vitest + `test/` в host-репо, писать QA-артефакты относительными путями внутрь репо, подключить к CI как гейт |
