# Row19 prep: Codex product workflow

Статус prep-пакета: `ROW19_PREP_DONE_REVIEW_REQUIRED`.

Этот пакет готовит только Row19 `frdp-p01-codex-product-readiness-product-workflow`. Codex workflow не запускался, Row19 не принят, product readiness не открыт.

## Основание

Прочитанные источники:

- `product-readiness-scenario-board.md`
- `product-readiness-scenario-board.json`
- `EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`
- `codex-xiigen-smart-skills/.../skills/xiigen-smart-skills/SKILL.md`

Текущая строка board для Row19 говорит:

- automated/core/API: нужен concrete work; Phase 03 command evidence недостаточна для одного complete product workflow;
- local real program: blocked, потому что Codex packaged command path был `Access denied`, а protocol/file associations являются metadata-only;
- joined status: blocked with next action; full joined pass отсутствует;
- next concrete action: построить single non-mutating Codex product-workflow boundary harness или split workflow sub-scenarios, затем найти clean Codex route отдельно от active conversation.

## Что Row19 должен доказать

Row19 может считаться product-workflow pass только если один и тот же сценарий доказан двумя путями:

1. Path A automated/core/adapter: установленный Codex XIIGen plugin adapter вызывает `src/codex-entrypoint.js`, формирует `XiigenExtensionRequest`, вызывает `C:/Projects/llm_mvp_core/tools/xiigen-extension-core-api.ps1`, получает непустой redacted response/debug trace и показывает prompt->task, task->plan, task/plan->skills evidence.
2. Path B local Codex UI: реальный Codex desktop/app chat, в отдельном чистом Row19 thread/workspace surface, видимо принимает тот же prompt, показывает user-visible final response, и visual evidence связывает этот результат с Path A по тому же prompt/correlation/output contract.
3. Сравнение: Path A и Path B должны совпасть по scenario id, correlation id, normalized task, выбранному/запрошенному skill surface, основным plan steps, no-mutation flags и final answer boundary.
4. Product readiness дополнительно требует evidence для adapter/core, token, quality, lifecycle, checkpoint, locator и host. Если будущий run докажет только adapter/core smoke, это scoped adapter proof, а не product readiness.

## Что Row19 не может доказать один

Row19 сам по себе не доказывает:

- token savings;
- full semantic quality improvement;
- learnability/training readiness;
- Direct Preference Optimization quality;
- checkpoint mutation/promotion;
- production locator update;
- full Codex chat/app readiness;
- full extension readiness;
- product readiness, если нет одновременно Path A, Path B и строгого joined comparison.

`marker-only != full local desktop pass`. `adapter/core evidence != host readiness`. `trace != savings`. `schema != training`. `checkpoint candidate != promotion`.

## Exact same-scenario prompt

Этот prompt должен использоваться без изменений в Path A и Path B:

```text
ROW19-CODEX-PRODUCT-WORKFLOW-SAME-SCENARIO

Workspace/core root: C:\Projects\llm_mvp_core
Scenario id: frdp-p01-codex-product-readiness-product-workflow
Correlation id: ROW19-CODEX-PRODUCT-WORKFLOW-001

Задача пользователя:
Подготовь не-мутирующий Codex product-workflow ответ для проверки XIIGen Smart Skills. Выполни prompt->task, task->plan и task/plan->skills для безопасного запроса:
"Проверить, что для harmless support task про само-проверку и адаптивный рабочий цикл выбран или запланирован навык client-adaptive-learning-loop, а пользователь получает понятный финальный ответ без изменения файлов."

Ограничения:
- не редактируй файлы;
- не запускай training, eval, DPO generation или checkpoint/locator mutation;
- не вызывай hidden external AI из adapter/core;
- не утверждай token savings, semantic quality improvement, learnability, checkpoint promotion, locator update, host UI readiness или product readiness;
- если какой-то слой только planned/expected, явно пометь его как not_proven.

Верни user-visible ответ на русском языке со строго видимыми полями:
- scenario_id
- correlation_id
- normalized_task
- task_plan_steps
- requested_codex_skill_surface
- product_workflow_skills_considered
- selected_or_planned_skill
- why_selected_or_planned
- core_root
- local_user_model_root_status
- evidence_to_collect
- mutation_flags
- external_ai_hidden_usage
- non_claims
- final_user_visible_response

Финальная фраза должна сказать: "Это подготовка/маршрутизация Row19; readiness не доказана без dual-path same-scenario evidence."
```

## Path A: automated/core или adapter path

Исполнитель должен запускать не workflow acceptance, а bounded installed-plugin adapter/core smoke через Codex XIIGen smart-skills adapter. Команда ниже является execution work-order template; в этом prep-пакете она не запускалась.

```powershell
$pluginRoot = "C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500"
$runRoot = "C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-product-readiness-product-workflow\run-<UTC>\path-a-adapter"
New-Item -ItemType Directory -Force -Path $runRoot
Set-Location $pluginRoot
node .\src\codex-entrypoint.js `
  --method prompt-to-task `
  --user-prompt-or-task-text "<EXACT SAME-SCENARIO PROMPT ABOVE>" `
  --host-facts-json '{"surface":"installed-codex-skill","host":"Codex","scope":"ROW19","workspace":"C:\\Projects\\llm_mvp_core","local_user_model_root":"C:\\Projects\\llm_mvp_core","mutation_allowed":false}' `
  --requested-skills-json '["xiigen-smart-skills"]' `
  --available-skills-json '["xiigen-smart-skills"]' `
  --check-set-inputs-json '{"row19":"codex-product-workflow-prep","non_mutating":true}' `
  --provenance-json '{"source":"installed-plugin-skill","plugin":"codex-xiigen-smart-skills","scenario":"frdp-p01-codex-product-readiness-product-workflow"}' `
  --correlation-id "ROW19-CODEX-PRODUCT-WORKFLOW-001" `
  --debug-enabled `
  --core-root "C:\Projects\llm_mvp_core" `
  --extension-root "." `
  --request-path "$runRoot\request.json" `
  --response-path "$runRoot\response.json" `
  --trace-path "$runRoot\trace.json" *> "$runRoot\adapter-command-output.txt"
```

Expected Path A evidence:

- `request.json` exists and contains the exact prompt, method `prompt-to-task`, requested skill surface `xiigen-smart-skills`, correlation id, workspace/core root, no-mutation host facts.
- `response.json` exists, is redacted, non-empty, and includes or can be validated into the visible fields requested by the prompt.
- `trace.json` exists and shows adapter -> `src/codex-entrypoint.js` -> `xiigen-extension-core-api.ps1` boundary, correlation id, no hidden external AI, no training/eval/checkpoint/locator mutation.
- `adapter-command-output.txt` exists and does not contain secrets.
- `path-a-validation.json` records booleans:
  - `installed_skill_surface_loaded`
  - `adapter_core_boundary_executed`
  - `installed_plugin_runtime_invocation_proven`
  - `request_response_trace_written`
  - `same_prompt_preserved`
  - `mutation_flags_all_false`
  - `hidden_external_ai_used`
  - `token_savings_proven`
  - `quality_improvement_proven`
  - `learnability_proven`
  - `training_started`
  - `eval_started`
  - `checkpoint_mutation_started`
  - `full_codex_chat_ui_proven`

For this skill-based smoke, only `installed_skill_surface_loaded`, `adapter_core_boundary_executed`, `installed_plugin_runtime_invocation_proven`, `request_response_trace_written`, `same_prompt_preserved`, and `mutation_flags_all_false` may become true unless additional independent evidence exists.

## Path B: local Codex UI path

В этом prep-пакете local Codex UI path не выполнялся. Для execution нужен отдельный clean route, не эта активная planning conversation.

Техническая автоматизация Codex desktop UI в этом prep-пакете не доказана. Поэтому допустимый путь исполнения: sub-agent-with-computer или manual visual evidence runner. Fake substitute запрещен: нельзя заменить UI path открытым JSON/log файлом, marker-only screenshot, protocol/file association metadata или helper-only proof.

Exact user-visible steps:

1. Открыть активное Codex desktop app.
2. Создать или открыть отдельный clean Row19 thread/workspace surface, не текущий planning/prep thread.
3. Выполнить auth/session preflight: видимо подтвердить, что пользовательский Codex session активен. Секреты, email/token и приватные данные закрыть/redact.
4. Зафиксировать screenshot `path-b-00-auth-preflight-redacted.png` и `path-b-auth-preflight.json` со статусом `SIGNED_IN_READY`, `LOGIN_RECOVERY_REQUIRED`, `HUMAN_LOGIN_REQUIRED` или `AUTH_BLOCKED_WITH_EVIDENCE`.
5. Если login recovery нужен, остановить UI execution как blocked evidence; credentials не вводить и не записывать.
6. В clean thread вставить exact same-scenario prompt из этого prep-файла.
7. Снять screenshot `path-b-01-prompt-before-submit.png`, где видны scenario id, correlation id и ключевые no-mutation ограничения.
8. Нажать submit/send один раз.
9. Дождаться final Codex response. Не выполнять code edits, git, training/eval/checkpoint/locator actions.
10. Снять screenshot `path-b-02-final-response-visible.png`, где видны поля `scenario_id`, `correlation_id`, `normalized_task`, `task_plan_steps`, `requested_codex_skill_surface`, `selected_or_planned_skill`, `mutation_flags`, `non_claims`, `final_user_visible_response`.
11. Сохранить redacted transcript/readback `path-b-ui-transcript-redacted.md` и structured readback `path-b-ui-readback.json`.
12. Сформировать `path-b-ui-validation.json` с pass/fail по exact prompt, visible fields, no mutation, no hidden substitute, no product-readiness overclaim.

Expected visible result:

- Codex UI показывает user-visible final response на русском языке.
- Response явно говорит, что это подготовка/маршрутизация Row19.
- Response не утверждает product readiness.
- Response сохраняет correlation id `ROW19-CODEX-PRODUCT-WORKFLOW-001`.
- Response помечает token savings, semantic quality, learnability/training, checkpoint promotion, locator update and full host readiness как not proven.

## Skills, guides, QA expectations

Required to load/use before execution:

- `codex-xiigen-smart-skills:xiigen-smart-skills` skill from `C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500\skills\xiigen-smart-skills\SKILL.md`.
- Board rules from `product-readiness-scenario-board.md/json`:
  - Dual-Path Same-Scenario UI Proof;
  - Host Session Auth Preflight and Browser Login Recovery;
  - No-Blind-Spot UI Scenario Script;
  - 50-Pass Scenario Clarity Review;
  - Plan-Not-Ready Is Repair Loop, Not Stop.
- QA/UI testing expectation: screenshots must show the actual Codex UI interaction, not logs opened in an editor. Exact clicks/typing/expected UI state must be followed. Agent judgment is not a scenario step.
- No external hidden AI usage inside adapter/core. Codex host UI is the visible host under test; adapter/core logs must not show hidden external AI calls.

## PASS/FAIL gates

Scoped adapter proof PASS only if:

- Path A command exits successfully;
- request/response/trace/command-output files are created under the Row19 run root;
- request contains the exact prompt and correlation id;
- response is redacted and non-empty;
- trace proves adapter/core boundary execution;
- no hidden external AI, training, eval, DPO generation, checkpoint mutation, locator mutation, production locator update, git, or code edit occurred;
- validation states clearly that this is scoped adapter/core proof only.

Scoped adapter proof FAIL if:

- command hits `Access denied`;
- request/response/trace missing;
- prompt differs from Path B prompt;
- only marker/log metadata exists;
- any mutation/training/eval/checkpoint/locator/git/code edit starts;
- report calls adapter/core evidence product readiness.

Local Codex UI proof PASS only if:

- clean Codex UI route separate from this active conversation is used;
- auth/session preflight is captured or a login blocker is recorded honestly;
- exact same prompt is submitted visibly;
- final Codex response is visible in screenshots and redacted transcript;
- output contract fields are visible/read back;
- no fake substitute is used.

Local Codex UI proof FAIL if:

- no screenshot/video of actual Codex UI interaction exists;
- evidence is only JSON/log/marker/protocol/file-association metadata;
- current prep conversation is reused as the proof route;
- prompt was edited without recording mismatch;
- final response overclaims readiness.

Full joined product-readiness PASS only if:

- Path A PASS;
- Path B PASS;
- joined comparison PASS for same prompt/correlation/output contract;
- adapter/core, token, quality, lifecycle, checkpoint, locator, and host evidence are all present and independently validated;
- every non-claim remains false unless supported by exact evidence.

Full joined product-readiness FAIL if any of these are true:

- Path A only;
- Path B only;
- marker-only local UI;
- adapter/core evidence treated as host readiness;
- trace treated as token savings;
- schema treated as training;
- checkpoint candidate treated as promotion;
- no token/quality/lifecycle/checkpoint/locator evidence for the product workflow.

## Evidence files future execution must create

Expected execution root:

`docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-codex-product-readiness-product-workflow/run-<UTC>/`

Required files:

- `path-a-adapter/request.json`
- `path-a-adapter/response.json`
- `path-a-adapter/trace.json`
- `path-a-adapter/adapter-command-output.txt`
- `path-a-adapter/path-a-validation.json`
- `path-a-adapter/path-a-human-report.md`
- `path-b-codex-ui/path-b-auth-preflight.json`
- `path-b-codex-ui/path-b-00-auth-preflight-redacted.png` or honest blocker evidence
- `path-b-codex-ui/path-b-01-prompt-before-submit.png`
- `path-b-codex-ui/path-b-02-final-response-visible.png`
- `path-b-codex-ui/path-b-ui-transcript-redacted.md`
- `path-b-codex-ui/path-b-ui-readback.json`
- `path-b-codex-ui/path-b-ui-validation.json`
- `joined/row19-dual-path-comparison.json`
- `joined/row19-dual-path-comparison.md`
- `joined/row19-50-clarity-review.json`
- `row19-execution-status.json`
- `row19-human-status.md`

## Non-claims locked false unless proven

- `product_readiness_claimed=false`
- `full_joined_pass=false`
- `full_codex_chat_ui_proven=false`
- `token_savings_proven=false`
- `quality_improvement_proven=false`
- `learnability_proven=false`
- `training_started=false`
- `eval_started=false`
- `dpo_generation_started=false`
- `checkpoint_mutation_started=false`
- `checkpoint_promotion_started=false`
- `locator_mutation_started=false`
- `production_locator_update_started=false`
- `external_ai_hidden_usage=false`
- `row19_accepted=false`

## Next work order if prep is accepted

```text
Ты bounded sub-agent executor для Row19 frdp-p01-codex-product-readiness-product-workflow в C:\Projects\llm_mvp_core.

Authority: execute only the accepted Row19 prep package:
C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-product-readiness-product-workflow\row19-prep-codex-product-workflow.md
and
C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-product-readiness-product-workflow\row19-prep-codex-product-workflow.json

Allowed write scope: create a new run-<UTC> folder only under:
C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-product-readiness-product-workflow\

Run Path A adapter/core using the exact same-scenario prompt and command template from the prep. Do not mutate code, git, training/eval, checkpoint, locator, or production assets. If Path A is blocked by Access denied, record exact blocker evidence and stop that path as failed, not accepted.

For Path B, use a clean Codex desktop/app UI route separate from the active prep conversation. If automation is not technically available, use sub-agent-with-computer/manual visual evidence. Capture auth preflight, prompt-before-submit, final response screenshots, redacted transcript, and structured readback. Do not use JSON/log/marker/protocol metadata as a fake UI substitute.

Run joined comparison only after both paths have evidence. Product readiness may be marked PASS only if Path A, Path B, joined comparison, and adapter/core/token/quality/lifecycle/checkpoint/locator/host evidence all pass. Otherwise report the narrowest supported claim.

Return ROW19_EXECUTION_DONE_REVIEW_REQUIRED or ROW19_EXECUTION_BLOCKED_REVIEW_REQUIRED with changed files, exact commands/actions, evidence paths, pass/fail gates, and narrow claim. No git.
```

## Narrow claim from this prep packet

This prep packet defines the Row19 same-scenario prompt, dual-path execution plan, strict pass/fail gates, evidence file contract, non-claims, and next execution work order. It does not execute Codex workflow and does not accept Row19.
