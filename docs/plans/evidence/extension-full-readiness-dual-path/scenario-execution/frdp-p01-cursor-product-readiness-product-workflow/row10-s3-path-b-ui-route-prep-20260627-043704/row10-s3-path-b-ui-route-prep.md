# Row10-S3 Path B Cursor UI-route prep

## Блок честности

claim_scope: Row10-S3 Path B UI-route prep packet only, paired to the already reviewed Row10-S2 Path A core/API boundary scenario.
evidence_scope: read/listed the three source folders named in the work order; parsed the Row10-S2 Path A request JSON; parsed the split/harness prep JSON rows for Row10-S2 and Row10-S3; inspected the reviewed Path A execution review text/JSON via targeted search; did not execute Cursor UI.
count_claimed: 1 prep folder, 1 Markdown prep packet, 1 JSON prep packet.
independent_evidence_count: source directory listings, source text scans, parsed Path A request JSON, parsed split-prep JSON rows, this Markdown packet, companion JSON packet, and post-write verification commands.
subagent_id_or_NONE: Row10-S3-PathB-prep.
downgrade_needed: yes for anything beyond prep. I did not run Cursor, click UI, type/send prompts, run product/runtime execution, run tests/builds, use web/external AI, train, mutate checkpoints, update locators, commit, or push.

## Русское резюме

Вердикт этого артефакта: `PASS_PREP`, если companion JSON парсится и проверка Markdown/JSON agreement проходит. Это не UI PASS и не readiness PASS.

Этот пакет готовит будущий реальный локальный Cursor Path B UI-route для того же Row10 scenario, который уже прошел Path A как `PASS_BOUNDARY`:

- `scenario_id`: `frdp-p01-cursor-product-readiness-product-workflow`
- `host`: `Cursor`
- prep stage: `Row10-S3 Path B UI-route prep`
- paired Path A slice: `Row10-S2`
- paired Path A verdict: `PASS_BOUNDARY`
- paired Path A review verdict: `PASS`
- prompt hash from Path A request: `e5334133911acf3638934d4a752e9924baf2a3209d5277df58b56cb1097f9506`

Важная граница: `Row10-S3` здесь является stage/folder для Path B UI-route prep после Row10-S2 Path A, а не доказательством token-accounting slice readiness. Будущий UI executor должен сохранять pairing с `slice=Row10-S2`, потому source Path A request именно такой.

## Источники

Прочитанные источники:

- `row10-s2-path-a-execution-20260627-042235`
- `row10-s2-path-a-execution-review-20260627-043226`
- `row10-split-harness-prep-20260627-033319`

Подтвержденные source facts:

- Path A request uses `HostName=Cursor`, `Method=prompt-to-task`, and the Row10 prompt below.
- Path A execution report says only `PASS_BOUNDARY`.
- Path A review says only automated core/API boundary evidence passed, with no Cursor UI, host runtime, product readiness, token savings, semantic quality, training, checkpoint promotion, locator update, or lifecycle readiness proof.
- Split/harness prep requires real visible Cursor XIIGen panel proof, not generic Cursor chat, New Agent, marker-only, source-only, JSON/log-only, or trace-only proof.
- Split/harness prep names textarea `xiigen-smart-skills-prompt` and submit button `xiigen-smart-skills-submit`.

## Same Prompt For Future Path B

The future UI executor must use this exact prompt text, copied from Path A request:

```text
FRDP-P01-ROW10-CURSOR-PRODUCT-WORKFLOW-V1
scenario_id=frdp-p01-cursor-product-readiness-product-workflow
slice=Row10-S2
host=Cursor
mode=no-send-local-extension-runtime
task=Adapter/core submit smoke through thin Cursor adapter. Process this as a boundary/API request only: keep product logic in llm_mvp_core, do not claim Cursor UI readiness, product readiness, token savings, semantic quality, training, checkpoint promotion, locator update, or lifecycle readiness. Return safe refs/status only.
```

The future UI executor must compute or record that this prompt is the same as Path A by using Path A hash `e5334133911acf3638934d4a752e9924baf2a3209d5277df58b56cb1097f9506`. If the exact prompt cannot be used, future Path B is `BLOCKED_PROMPT_MISMATCH`.

## Cursor Targeting And Multi-monitor Handling

Before any future click or typing, the UI executor must prove the target is the real local Cursor program/window:

1. Identify the Cursor window title and process/window identity in the execution notes. Prefer an Extension Development Host or a run folder/session visibly dedicated to Row10.
2. If multiple monitors or overlapping windows are present, move or resize the target Cursor window so the full title bar and XIIGen panel area are visible in the screenshot. Do not click until the target window is unambiguous.
3. Capture `row10-s2-path-b-cursor-target-window-preflight.png` showing the Cursor window frame/title and enough non-secret context to prove it is the target.
4. If the target window cannot be made unambiguous, future Path B is `BLOCKED_TARGET_WINDOW_AMBIGUOUS`.
5. Global foreground/focus alone is insufficient. The screenshot/reference must show the target application and the intended extension surface area.

## Auth/session Preflight

Before opening or using the XIIGen Smart Skills panel, the UI executor must classify the session as signed-in/trusted or anonymous without capturing secrets.

Signed-in/trusted indicators may include non-secret account avatar/initials, account menu title, trusted workspace/account state, or extension panel available state. Do not capture email addresses, access tokens, cookies, authorization headers, one-time codes, passwords, or private chat history. Crop or redact any private identifier.

Anonymous/session-lost indicators include a visible Sign in button, unauthenticated banner, account menu showing no account, extension panel blocked by login, wrong account warning, or redirect/login prompt.

Required evidence:

- Capture `row10-s2-path-b-auth-preflight.png` if signed-in/trusted state is visible without secrets.
- If any secret or private chat area would be visible, crop/redact before committing.
- If auth state cannot be shown safely, record `BLOCKED_AUTH_PRECHECK_UNSAFE_TO_CAPTURE` and do not proceed.

## Browser Login Recovery If Cursor Opens Anonymous

If Cursor opens anonymous or session is lost, future execution must run login recovery without credential capture:

1. Record `LOGIN_RECOVERY_REQUIRED` in the future UI evidence JSON.
2. Use Cursor's visible sign-in/account recovery control or command. Do not type credentials into captured screenshots or logs.
3. If a browser opens, capture only non-secret origin/title/window state as `row10-s2-path-b-login-recovery-browser-nonsecret.png`. No password fields, token pages, cookies, one-time codes, recovery codes, or session secrets may be captured.
4. The human/operator may complete credentials outside captured evidence. The UI executor must pause capture while credentials or 2FA are visible.
5. Return to Cursor through the normal redirect/deep-link, then repeat auth/session preflight.
6. Expected return state: Cursor target window visible, non-secret signed-in/trusted indicator visible or safely documented, XIIGen Smart Skills panel accessible.
7. If signed-in/trusted state is not restored safely, future Path B is `BLOCKED_CURSOR_AUTH_NOT_RESTORED`.

## Future Path B UI Steps

These are future steps only. This prep did not execute them.

1. Open the real local Cursor target window identified in the preflight screenshot.
2. Open the XIIGen Smart Skills extension surface/panel through a visible Activity Bar/sidebar entry, existing XIIGen panel tab, or Command Palette command whose displayed name contains `XIIGen Smart Skills`. If the panel/command cannot be found, capture the non-secret missing-surface state and mark `BLOCKED_XIIGEN_PANEL_NOT_FOUND`.
3. Confirm the visible panel is XIIGen Smart Skills, not generic Cursor chat, not New Agent, and not a login textbox. Capture `row10-s2-path-b-xiigen-panel-before-prompt.png`.
4. Focus the prompt input/textarea `xiigen-smart-skills-prompt` or a visible label that clearly maps to that textarea. If the input cannot be identified, mark `BLOCKED_PROMPT_TEXTAREA_NOT_FOUND`.
5. Paste or type the exact same Row10 prompt from this prep. Do not type into generic Cursor chat/New Agent.
6. Capture `row10-s2-path-b-cursor-prompt-entered-before-submit.png` with the prompt marker/scenario visible or safely cropped enough to prove same scenario. No private chats may be captured.
7. Click the submit/send button `xiigen-smart-skills-submit` or a visible button that clearly maps to that id. If no submit/send action is visible, mark `BLOCKED_SUBMIT_BUTTON_NOT_FOUND`.
8. Wait only for the local extension visible result/error state required by the future work order. Do not use external AI, web AI, or generic Cursor chat as substitute.
9. Capture `row10-s2-path-b-cursor-visible-result-or-error.png` showing one of:
   - visible safe refs/status returned by the XIIGen panel;
   - visible local extension error/result state tied to the same prompt;
   - a clear safe blocked state if no result can be produced.
10. Write future execution evidence JSON with window target, auth result, prompt hash, panel route, before/after screenshots, result/error state, and claim locks.

## Required Evidence Or BLOCKED

Future Path B must produce these screenshot/reference files or mark the exact blocker:

- `row10-s2-path-b-cursor-target-window-preflight.png`
- `row10-s2-path-b-auth-preflight.png` or `row10-s2-path-b-login-recovery-browser-nonsecret.png` plus repeated auth preflight
- `row10-s2-path-b-xiigen-panel-before-prompt.png`
- `row10-s2-path-b-cursor-prompt-entered-before-submit.png`
- `row10-s2-path-b-cursor-visible-result-or-error.png`
- `row10-s2-path-b-ui-route-evidence.json`

Optional screenshot wording is forbidden. Missing required screenshot/reference without a named blocker is `NEEDS_FIX` for the execution packet and cannot become Path B PASS.

## PASS / BLOCKED / NEEDS_FIX Criteria

`PASS_PREP` for this prep packet requires:

- Markdown and JSON agree on scenario id, paired Path A slice, prompt hash, Path B UI steps, auth recovery, evidence requirements, and claim locks.
- JSON parses.
- No Cursor UI/product/runtime execution occurred during prep.
- Future Path B steps require screenshot/reference or explicit BLOCKED.

Future Path B `PASS_UI_ROUTE` requires:

- Same scenario id and exact prompt hash as Path A.
- Real local Cursor target window evidence.
- Signed-in/trusted auth preflight or successful secret-safe recovery.
- XIIGen Smart Skills panel evidence before prompt entry.
- Prompt entered into `xiigen-smart-skills-prompt`.
- Submit via `xiigen-smart-skills-submit`.
- Required before/after screenshot/reference exists.
- No generic Cursor chat/New Agent substitution.
- No secrets/private chats committed.

Future Path B is `BLOCKED` if:

- Cursor target window cannot be safely identified.
- Auth/session cannot be safely verified or restored.
- XIIGen Smart Skills panel/command cannot be found.
- `xiigen-smart-skills-prompt` or `xiigen-smart-skills-submit` cannot be identified.
- Required screenshot/reference cannot be produced safely.
- Exact prompt/hash cannot be preserved.

Future execution packet is `NEEDS_FIX` if:

- It claims UI/product readiness from prep alone.
- It omits a required screenshot/reference without a specific blocker.
- Markdown/JSON disagree.
- It captures or commits secrets/private chat screenshots.
- It uses generic Cursor chat, New Agent, source-only proof, marker-only proof, JSON/log-only proof, or trace-only proof as Path B UI evidence.

## Claim Locks

All claim locks remain closed:

- `product_readiness_claim=false`
- `cursor_ui_readiness_claim=false`
- `cursor_host_readiness_claim=false`
- `host_runtime_claim=false`
- `token_savings_claim=false`
- `semantic_quality_claim=false`
- `training_or_learnability_claim=false`
- `checkpoint_promotion_claim=false`
- `locator_update_claim=false`
- `lifecycle_readiness_claim=false`
- `runtime_execution_claim=false`
- `test_execution_claim=false`

No product readiness can be claimed until Path A and Path B same-scenario evidence both pass and a final joined review accepts the evidence. Path A boundary/API evidence is not Path B UI proof. Path B prep is not Path B execution proof.

## Safety And Redaction

- No secrets captured.
- No private chat screenshots committed.
- Crop/redact account identifiers, chat content, tokens, cookies, password fields, one-time codes, recovery codes, auth headers, and browser session details.
- Screenshots must show only enough non-secret UI to prove route, target, auth/session state, prompt marker, submit action, and visible result/error state.
- If safe capture is impossible, mark the exact `BLOCKED_*` reason instead of taking unsafe screenshots.

## Forbidden In This Prep

This prep did not and must not claim:

- launching/focusing/clicking Cursor;
- typing/sending prompts;
- UI automation;
- product/runtime execution;
- tests/builds;
- code/board/status edits;
- training/checkpoint/locator/lifecycle actions;
- commit/push;
- web/external AI;
- nested subagents.

## Exact Next Review Task

Review `row10-s3-path-b-ui-route-prep.md` and `row10-s3-path-b-ui-route-prep.json` for JSON parse, Markdown/JSON agreement, same-scenario pairing with Row10-S2 Path A, auth recovery completeness, screenshot/reference-or-BLOCKED requirements, and closed claim locks. If review passes, the next separate work order may authorize real local Cursor Path B UI execution for the same prompt; this prep alone does not authorize or perform that execution.
