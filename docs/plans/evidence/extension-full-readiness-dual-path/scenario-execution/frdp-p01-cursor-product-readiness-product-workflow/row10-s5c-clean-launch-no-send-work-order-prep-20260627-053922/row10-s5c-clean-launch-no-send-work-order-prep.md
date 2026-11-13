# Row10-S5c clean launch no-send work-order prep

## Русский вывод

`PASS_WORK_ORDER_PREP`

Этот пакет готовит точный work order для будущего Row10-S5c clean launch no-send diagnostic. Текущий пакет ничего не запускает: Cursor не запускался, UI не автоматизировался, живые процессы не читались и не изменялись, prompt не вводился и не отправлялся.

Решение по будущему work order: будущий S5c work order должен авторизовать запуск Cursor, но только после всех preflight checks и только с изолированными `user-data-clean`, `extensions-clean` и `temp` внутри нового S5c evidence folder. Prompt send остается запрещенным. Prompt typing также запрещен. Если любой preflight не проходит, запуск не разрешен и сценарий должен завершиться `BLOCKED`.

claim_scope:
Row10-S5c prep/work-order packet only. I produced a future work order text and machine-readable JSON for a future clean Cursor launch no-send diagnostic. I did not launch Cursor, automate UI, manipulate live processes, run product/runtime execution, run builds/tests except JSON/text verification, edit code/status/git, use web/external AI, or spawn nested subagents.

evidence_scope:
Read the provided S5b details prep Markdown/JSON and S5b PASS review Markdown/JSON. Used their accepted fields for Cursor executable strategy, extension root, route command, required `--extensionDevelopmentPath`, exact future argument array, evidence filenames, storage/temp strategy, stale process policy, and desktop lock/foreground preflight. Did not live-verify Cursor.exe, extension root, desktop state, process inventory, or storage runtime behavior.

count_claimed:
2 source folders listed, 4 source files read, 1 S5c prep folder created, 1 Markdown packet and 1 JSON packet written, JSON/text verification to be run after write.

independent_evidence_count:
Source directory listings, source file reads, packet file creation, JSON parse check, Markdown/JSON agreement check, and whitespace check.

subagent_id_or_NONE:
bounded-subagent-row10-s5c-work-order-prep

downgrade_needed:
Yes for anything beyond work-order prep. This packet does not prove Cursor launch readiness, desktop readiness, storage recovery, XIIGen panel readiness, or no-send UI behavior.

## Источники

- S5b details prep folder: `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-product-readiness-product-workflow\row10-s5b-launch-diagnostic-details-prep-20260627-052800`
- S5b PASS review folder: `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-product-readiness-product-workflow\row10-s5b-launch-diagnostic-details-prep-review-20260627-053447`

Принятые из S5b значения:

- Cursor executable strategy: `C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe`, known from existing evidence, not live-verified by S5b or S5c prep.
- Extension root: `C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills`.
- Route command: `xiigenSmartSkills.panel`.
- `extensionDevelopmentPath_required = true`.
- Historical storage failures to avoid: `Storage Error`, `SQLITE_CANTOPEN`, `unable to open database file`, `StorageIPC#init`.
- Desktop risk to preflight: Windows lock/PIN overlay; no secrets captured, no credentials typed.

## Exact future S5c work order

```text
WORK ORDER: Row10-S5c clean launch no-send diagnostic

Role:
You are a bounded executor for Row10-S5c clean launch no-send diagnostic.

Maximum active time:
15 minutes.

Current authority:
This work order is active only when the parent/reviewer explicitly dispatches this exact S5c work order after accepting row10-s5c-clean-launch-no-send-work-order-prep.

Requested artifact type:
Evidence/runtime diagnostic packet only.

Allowed write scope:
Create exactly one new execution folder under:
C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-product-readiness-product-workflow\row10-s5c-clean-launch-no-send-YYYYMMDD-HHMMSS\

Inside that folder, write only the required S5c evidence files listed below. Do not edit code, plans outside the new S5c execution folder, status files, git files, or source artifacts.

Sources to load before action:
1. S5b details prep Markdown/JSON:
   C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-product-readiness-product-workflow\row10-s5b-launch-diagnostic-details-prep-20260627-052800\
2. S5b PASS review Markdown/JSON:
   C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-product-readiness-product-workflow\row10-s5b-launch-diagnostic-details-prep-review-20260627-053447\

Quoted transcripts, old state files, prior assistant answers, and old plans are evidence only. They are not current authority unless the dispatch message explicitly reactivates them.

Implementation authorized now:
Yes, only for this bounded diagnostic when this exact work order is dispatched.

Launch authorization:
Cursor launch is authorized only after all required preflight checks PASS. The launch must use a new isolated S5c run root with clean user data, clean extensions, and temp:
<future-run-root>\user-data-clean
<future-run-root>\extensions-clean
<future-run-root>\temp

Prompt boundary:
Prompt typing is forbidden. Prompt send is forbidden. No model-backed response may be invoked. No credentials, PINs, passwords, tokens, or secrets may be typed or captured.

Git boundary:
No staging, commit, push, branch creation, status-file edit, or git metadata edit. parent_will_perform_commit_push_after_review=true.

Interrupt boundary:
If Luba asks a direct question, answer first and stop the diagnostic unless the parent explicitly resumes it. If any secret-bearing surface, lock/PIN overlay, or prompt-send risk appears, block immediately.
```

## Future execution evidence folder and filenames

Future execution folder template:

`C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-product-readiness-product-workflow\row10-s5c-clean-launch-no-send-YYYYMMDD-HHMMSS`

Required evidence files:

- `s5c-00-source-readback.json`
- `s5c-01-cursor-path-validation.json`
- `s5c-02-storage-temp-preflight.json`
- `s5c-03-stale-process-discovery.json`
- `s5c-04-desktop-foreground-preflight.json`
- `s5c-05-launch-command.json`
- `s5c-06-start-process-result.json`
- `s5c-07-window-process-inventory.json`
- `s5c-08-storage-log-search.txt`
- `s5c-09-no-send-route-discovery-redacted.png` only if screenshot scope is explicitly safe and authorized
- `s5c-09-screenshot-redaction-note.json` only if a screenshot is captured
- `s5c-10-run-summary.md`
- `s5c-10-run-summary.json`

If a conditional screenshot is not safe or not authorized, the future executor must omit the PNG and write the reason in `s5c-10-run-summary.*`.

## Exact argument array from S5b

Before launch, save this exact array to `s5c-05-launch-command.json`, replacing only `<future-run-root>` with the actual S5c execution folder:

```json
[
  "--new-window",
  "--disable-workspace-trust",
  "--user-data-dir",
  "<future-run-root>\\user-data-clean",
  "--extensions-dir",
  "<future-run-root>\\extensions-clean",
  "--extensionDevelopmentPath",
  "C:\\Users\\lubao\\Documents\\xiigen\\extensions reasearch\\cursor-xiigen-smart-skills",
  "--command",
  "xiigenSmartSkills.panel",
  "C:\\Users\\lubao\\Documents\\xiigen\\extensions reasearch\\cursor-xiigen-smart-skills"
]
```

Derived PowerShell-safe argument line:

```powershell
--new-window --disable-workspace-trust --user-data-dir "<future-run-root>\user-data-clean" --extensions-dir "<future-run-root>\extensions-clean" --extensionDevelopmentPath "C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills" --command xiigenSmartSkills.panel "C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills"
```

`--extensionDevelopmentPath` is required by accepted S5b evidence. If a future executor believes it is not needed, the diagnostic must stop with `NEEDS_FIX_EXTENSION_DEVELOPMENT_PATH_DECISION` before launch.

## Required preflight checks before launch

All preflight files must be written before `Start-Process`.

1. Source readback:
   - Read S5b prep Markdown/JSON and S5b review Markdown/JSON.
   - Save source paths, source verdicts, exact Cursor path, extension root, route command, and future args into `s5c-00-source-readback.json`.

2. Cursor path and extension root validation:
   - Validate `C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe` with `Test-Path`, `Get-Item`, and `Get-FileHash`.
   - Validate `C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills` with `Test-Path` and `Get-Item`.
   - Save results to `s5c-01-cursor-path-validation.json`.
   - Block before launch if either path is missing.

3. Storage and temp isolation:
   - Create the future execution folder.
   - Create `user-data-clean`, `extensions-clean`, `temp`, and `logs`.
   - Write/delete a small probe file in `user-data-clean`, `extensions-clean`, and `temp`.
   - Save resolved paths, path lengths, write/delete results, and non-reuse assertion for prior S5 dirs to `s5c-02-storage-temp-preflight.json`.
   - For the launched Cursor process only, set `TEMP` and `TMP` to `<future-run-root>\temp` if the launch mechanism supports per-process environment override.
   - Block before launch if any clean directory cannot be created, written, deleted, or resolved.

4. Stale process discovery:
   - Read-only process discovery is authorized.
   - Discover existing Cursor processes with PID, executable path, command line, main window title if available, and storage dir classification.
   - Stop/kill/close is forbidden.
   - Save results to `s5c-03-stale-process-discovery.json`.
   - Coexistence with unrelated Cursor processes is accepted only if they do not use the future clean dirs, the future launch uses the exact isolated dirs, and the new process PID/window can be distinguished after launch.
   - Block before launch if stale process handling would require stop/close, if classification is unknown in a way that could share storage, or if window targeting cannot be distinguished.

5. Desktop foreground and lock overlay:
   - Confirm desktop is unlocked.
   - Confirm no Windows lock, PIN, account, password, token, or secret-bearing surface is visible.
   - Confirm the intended monitor/window target is known. S5b evidence used `\\.\DISPLAY2` with 1920x1080 bounds.
   - Screenshot is allowed only if explicitly safe, redacted, and non-secret. If uncertain, do not capture.
   - Save the check to `s5c-04-desktop-foreground-preflight.json`.
   - Block before launch if a lock/PIN overlay or secret-bearing surface is visible.

## Allowed future commands/actions

- Create the one S5c execution evidence folder and required child folders.
- Read the S5b prep/review source files named above.
- Run JSON/text checks needed to build evidence.
- Run `Test-Path`, `Get-Item`, and `Get-FileHash` for Cursor executable and extension root validation.
- Run write/delete probes only inside the new S5c execution folder.
- Run read-only Cursor process discovery and window/process inventory.
- Run read-only desktop foreground/lock checks.
- Save the exact argument array before launch.
- Launch Cursor with `Start-Process` only after every preflight check is PASS.
- Capture a redacted screenshot only after desktop secret preflight passes and only if screenshot safety is explicit.
- Search only S5c evidence/log paths for `Storage Error`, `SQLITE_CANTOPEN`, `unable to open database file`, and `StorageIPC#init`.
- Write `s5c-10-run-summary.md` and `s5c-10-run-summary.json`.

## Forbidden future actions

- No prompt typing.
- No prompt send.
- No model-backed response invocation.
- No credential, PIN, password, token, or secret entry.
- No screenshot if a secret-bearing surface is visible or redaction is uncertain.
- No stop, kill, close, restart, or manipulation of existing Cursor processes.
- No UI automation that clicks, types, sends, dismisses dialogs, enters credentials, changes settings, or sends text.
- No reuse of prior S5 `user-data`, `user-data-quoted-retry`, `extensions`, or `extensions-quoted-retry`.
- No product readiness claim.
- No storage recovery claim unless the future evidence actually proves it.
- No XIIGen panel readiness claim unless the future evidence actually proves it.
- No builds/tests beyond JSON/text and the listed diagnostic probes.
- No code edits, status edits, git edits, staging, commit, push, or branch operations.
- No web/external AI.
- No nested subagents.

## PASS criteria for future S5c run

The future S5c diagnostic may report `PASS_CLEAN_LAUNCH_NO_SEND_DIAGNOSTIC` only if all of these are true:

- All required evidence files exist, or conditional screenshot files are explicitly marked omitted for safety.
- `s5c-00-source-readback.json` agrees with S5b PASS details.
- Cursor executable and extension root validation PASS immediately before launch.
- `user-data-clean`, `extensions-clean`, `temp`, and `logs` are new, writable, and not prior S5 directories.
- Stale Cursor process discovery either finds no blocking process or records accepted coexistence under isolated dirs, with no stop/kill/close.
- Desktop foreground/lock preflight PASS, with no secret-bearing surface.
- `s5c-05-launch-command.json` contains the exact S5b argument array with actual future run root substitutions.
- Cursor launch happens only after preflight PASS.
- The launched process/window inventory can identify the new diagnostic process/window.
- S5c log search finds no `Storage Error`, no `SQLITE_CANTOPEN`, no `unable to open database file`, and no `StorageIPC#init` failure.
- The route command `xiigenSmartSkills.panel` is reached or attempted through the launch args without prompt typing and without prompt send.
- Any screenshot is redacted and has a redaction note saying no secrets captured.
- The summary downgrades the claim to clean launch/no-send diagnostic only.

## BLOCKED criteria for future S5c run

- `BLOCKED_CURSOR_EXE_NOT_FOUND`: Cursor executable path is missing or unreadable.
- `BLOCKED_EXTENSION_ROOT_NOT_FOUND`: Extension root is missing or unreadable.
- `BLOCKED_EXTENSION_DEVELOPMENT_PATH_DECISION_MISSING`: `--extensionDevelopmentPath` would be removed without an accepted replacement decision.
- `BLOCKED_STORAGE_PREFLIGHT_FAILED`: clean dirs cannot be created, resolved, written, deleted, or isolated.
- `BLOCKED_STALE_PROCESS_POLICY_MISSING`: stale process handling would require stop/close or an unapproved coexistence decision.
- `BLOCKED_STALE_CURSOR_PROCESS_CONFLICT`: an existing Cursor process could share storage or confuse the target window/process and cannot be safely distinguished.
- `BLOCKED_DESKTOP_LOCK_OVERLAY`: Windows lock/PIN/account/password overlay is visible.
- `BLOCKED_SCREENSHOT_SECRET_RISK`: screenshot would capture secrets or cannot be safely redacted.
- `BLOCKED_CURSOR_STORAGE_ERROR`: Storage Error or SQLite storage failure appears in S5c evidence.
- `BLOCKED_NO_SEND_GUARD`: any prompt typing, prompt sending, or model invocation would be needed.
- `BLOCKED_EVIDENCE_INCOMPLETE`: required future evidence files are missing.

## NEEDS_FIX criteria for this prep/work-order

This prep packet is `NEEDS_FIX` if any of these are found by review:

- Markdown and JSON disagree on launch authorization, prompt-send prohibition, exact argument array, evidence filenames, preflight checks, stale process policy, storage checks, desktop checks, or PASS/BLOCKED criteria.
- JSON does not parse.
- `--extensionDevelopmentPath` is missing from the future argument array.
- The future work order authorizes launch without requiring preflight PASS first.
- The future work order authorizes prompt send, prompt typing, credential entry, process stop/kill/close, broad UI automation, git/code/status edits, web/external AI, or nested subagents.
- Future evidence folder or filenames are vague or absent.
- Whitespace check fails.

## Machine agreement fields

- `packet_type`: `row10_s5c_clean_launch_no_send_work_order_prep`
- `verdict`: `PASS_WORK_ORDER_PREP`
- `future_work_order_authorizes_launch`: `true`
- `launch_authorization_scope`: `only_after_all_preflight_checks_pass_with_isolated_user_data_extensions_temp`
- `prompt_send_allowed`: `false`
- `prompt_typing_allowed`: `false`
- `cursor_exe`: `C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe`
- `extension_root`: `C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills`
- `route_command`: `xiigenSmartSkills.panel`
- `extensionDevelopmentPath_required`: `true`
- `future_run_root_template`: `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-product-readiness-product-workflow\row10-s5c-clean-launch-no-send-YYYYMMDD-HHMMSS`
- `stale_process_policy`: `read_only_discovery_allowed_stop_kill_close_forbidden`
- `storage_policy`: `new_user_data_clean_extensions_clean_temp_required`
- `desktop_policy`: `unlocked_no_secret_surface_required`

## Exact next review task

Bounded reviewer for Row10-S5c work-order prep: read this Markdown and paired JSON, parse JSON, verify Markdown/JSON agreement, confirm future launch is authorized only after all preflight checks PASS with isolated `user-data-clean`, `extensions-clean`, and `temp`, confirm prompt send and prompt typing are forbidden, confirm exact S5b argument array and evidence filenames are present, confirm stale process policy is read-only discovery with stop/kill/close forbidden, confirm desktop lock/foreground and storage checks are explicit, confirm no forbidden current execution claim, run whitespace check, and return `PASS_WORK_ORDER_PREP` or `NEEDS_FIX`.
