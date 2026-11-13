# Phase 14.13 Codex-only visible marker fix

Evidence root:

`C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-14-fixes\real-program-ui-proof\p14-fix-14-codex-visible-marker-20260624T044217Z`

## Общий статус

Финальный Codex status из разрешенного списка:

`DONE_REAL_APP_SCREENSHOT_AND_EXTENSION_TRACE`

Это означает только следующее: реальное окно Codex Desktop было видно на компьютере, свежий marker был виден внутри Codex-visible command/search palette, и same-run XIIGen installed-plugin/core trace использовал тот же marker как correlation id.

Это не доказывает full Codex chat UI readiness, не доказывает token savings, semantic quality improvement, learnability, training, eval, checkpoint mutation или внешний host prompt submission.

## Что было сделано на компьютере

1. Прочитан Phase 14 screen-control runbook, rejection criteria, human-readable status model и строка 14.13 в `docs\plans\XIIGEN-SEQUENTIAL-EXECUTION-CONTROL-PLAN-v1.md`.
2. Прочитан предыдущий p14-fix-13 report только как evidence того, что Codex оставался `NEEDS_FIX_EVIDENCE_INSUFFICIENT`.
3. Найдено реальное установленное окно Codex Desktop: `C:\Program Files\WindowsApps\OpenAI.Codex_26.616.6631.0_x64__2p2nqsd0c76g0\app\Codex.exe`, process `Codex`, title `Codex`.
4. Codex window был активирован через Windows foreground handle и снят baseline screenshot.
5. Попытка вставить marker в Plugins search field через обычный paste не сработала: marker не появился на screenshot.
6. Открыта Codex command/search palette через visible Codex UI.
7. Обычный `SendKeys` paste в palette не сработал: поле было сфокусировано, но marker не появился.
8. Низкоуровневый Windows `Ctrl+V` через `keybd_event` вставил marker в Codex command/search palette.
9. Снят screenshot реального Codex window, где видны Codex chrome, Plugins page под overlay, command/search palette и свежий marker.
10. Запущен installed XIIGen Codex plugin adapter/core smoke из установленного plugin root с тем же marker как `--correlation-id`.

## Реальный Codex app/window

Да, реальное окно Codex Desktop было видно и было foreground-capable.

Baseline screenshot:

`C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-14-fixes\real-program-ui-proof\p14-fix-14-codex-visible-marker-20260624T044217Z\raw\codex\baseline-codex-window.png`

Что видно: Codex Desktop app chrome, левый sidebar, Plugins surface, GitHub plugin page. Marker еще не показан.

## Marker

`P14-REAL-CODEX-20260624T044217Z`

## Screenshot evidence

Commit-safe review-safe Codex screenshot:

`C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-14-fixes\real-program-ui-proof\p14-fix-14-codex-visible-marker-20260624T044217Z\review-safe\codex-command-palette-visible-marker-review-safe.png`

Review-safe redaction summary: pinned conversation names and project/recent private names in the Codex sidebar were masked. Codex app chrome, navigation, Plugins surface, command/search palette, and marker `P14-REAL-CODEX-20260624T044217Z` remain visible.

Accepted Codex screenshot:

`C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-14-fixes\real-program-ui-proof\p14-fix-14-codex-visible-marker-20260624T044217Z\raw\codex\codex-command-palette-visible-marker-lowlevel-paste.png`

Raw screenshot status: retained as local/raw source evidence and not overwritten. The review-safe screenshot above is the preferred commit/review evidence path.

Что видно: реальный Codex Desktop app chrome, Codex Plugins page на фоне, Codex command/search palette, свежий marker `P14-REAL-CODEX-20260624T044217Z` в видимом поле.

Attempt screenshots, не принимаются как финальное marker evidence:

- `raw\codex\after-click-search.png`: Codex Plugins page visible, marker absent.
- `raw\codex\codex-plugin-search-visible-marker.png`: plugin search paste attempt, marker absent.
- `raw\codex\after-click-new-chat.png`: Codex command/search palette opened, marker absent.
- `raw\codex\codex-command-palette-visible-marker.png`: command palette focused after normal SendKeys attempt, marker absent.

## Trace evidence

Same-run installed-plugin/core trace:

- Request: `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-14-fixes\real-program-ui-proof\p14-fix-14-codex-visible-marker-20260624T044217Z\traces\codex\request.json`
- Response: `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-14-fixes\real-program-ui-proof\p14-fix-14-codex-visible-marker-20260624T044217Z\traces\codex\response.json`
- Trace: `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-14-fixes\real-program-ui-proof\p14-fix-14-codex-visible-marker-20260624T044217Z\traces\codex\trace.json`
- Adapter log: `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-14-fixes\real-program-ui-proof\p14-fix-14-codex-visible-marker-20260624T044217Z\traces\codex\adapter-stdout-stderr.log`

Trace marker/correlation:

- `correlation_id`: `P14-REAL-CODEX-20260624T044217Z`
- `started_utc`: `2026-06-24T04:47:44.1915988Z`
- `completed_utc`: `2026-06-24T04:47:52.5814277Z`
- `response_success`: `true`
- `response_status`: `core_boundary_partial_no_semantic_quality_claim`
- `external_ai_used`: `false`
- `host_runtime_claimed`: `false`
- `runtime_extension_claimed`: `false`
- `semantic_quality_claimed`: `false`
- `training_started`: `false`

Skill smoke booleans:

- `installed_skill_surface_loaded=true`
- `adapter_core_boundary_executed=true`
- `installed_plugin_runtime_invocation_proven=true`
- `full_codex_chat_ui_proven=false`
- `token_savings_proven=false`
- `quality_improvement_proven=false`
- `learnability_proven=false`
- `training_started=false`
- `eval_started=false`
- `checkpoint_mutation_started=false`

## Plan update summary

Updated only:

`C:\Projects\llm_mvp_core\docs\plans\XIIGEN-SEQUENTIAL-EXECUTION-CONTROL-PLAN-v1.md`

Change summary: recorded p14-fix-14 as the Codex-only Phase 14.13 fix, with Codex status `DONE_REAL_APP_SCREENSHOT_AND_EXTENSION_TRACE`, and kept the no-overclaim limitation that this is not full Codex chat UI readiness or product readiness.

## Verification

- `report.json` parse: PASS.
- Visual inspection of `review-safe\codex-command-palette-visible-marker-review-safe.png`: PASS, marker readable and Codex app chrome/Plugins/command palette identity remains visible.
- Non-git task-file trailing-whitespace check over `report.md`, `report.json`,
  and `docs\plans\XIIGEN-SEQUENTIAL-EXECUTION-CONTROL-PLAN-v1.md`: PASS.
- Task-scoped `git diff --check -- docs/plans/XIIGEN-SEQUENTIAL-EXECUTION-CONTROL-PLAN-v1.md artifacts/extension-runtime-token-quality-learnability/phase-14-fixes/real-program-ui-proof/p14-fix-13-real-desktop-screen-control-20260624T041937Z artifacts/extension-runtime-token-quality-learnability/phase-14-fixes/real-program-ui-proof/p14-fix-14-codex-visible-marker-20260624T044217Z`: PASS.
- Scoped `git status --short -- ...`: plan is modified and both evidence folders are currently untracked; no staging was used to make Git diff include untracked evidence files.
- Exact text trailing-whitespace check over the edited plan/report/json files: PASS.
- Repo-wide diff check was not run for this review-safe evidence task.

## Git boundary

No `git add`, `git stage`, `git commit`, `git push`, `checkout`, `reset`, `clean`, or delete operation was used.
