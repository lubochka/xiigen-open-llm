# Row10-S5b launch diagnostic details prep

claim_scope:
Row10-S5b prep/details only: launch diagnostic details for a future clean Cursor run. No Cursor launch, no UI automation, no live process manipulation, no product/runtime execution, no prompt typed/sent, no readiness claim.
evidence_scope:
Read provided S5a prep, S5a PASS review, and S5 salvage evidence files. Used existing evidence only for Cursor executable, extension path, route command, launch args, storage failure, process/window inventory, and lock/PIN overlay metadata. Did not verify live Cursor, live desktop, live processes, or current filesystem existence of Cursor.exe.
count_claimed:
3 source folders listed; key Markdown/JSON source packets and salvage JSON evidence files read; 1 S5b prep folder created; this Markdown packet and the paired JSON packet created.
independent_evidence_count:
Directory listing/read/search tool outputs from the three provided source folders, plus packet creation and post-write verification outputs reported in the final response.
subagent_id_or_NONE:
bounded-subagent-row10-s5b-details-prep
downgrade_needed:
Yes for anything beyond prep/details. This packet does not prove Cursor readiness, desktop readiness, XIIGen panel readiness, storage runtime recovery, or a full no-send UI pass.

## Вердикт

`PASS_DETAILS_PREP`

Этот пакет заполняет недостающие launch diagnostic details для будущего короткого запуска Cursor, но сам ничего не запускает. Узкий вывод: по сохраненным S5 evidence уже известны executable path, extension root, route command и исторический argument set; будущий запуск должен использовать новый isolated evidence root с чистыми `user-data`, `extensions` и `temp`, заранее записать exact args, выполнить desktop/secret preflight и не закрывать stale Cursor processes без отдельного разрешения.

## Что известно из evidence

- Cursor executable/path strategy: использовать путь из сохраненных `launch-command.json`, `launch-command-quoted-retry.json` и process inventory: `C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe`.
- Этот путь не проверялся live в S5b. Перед будущим запуском нужен no-launch path preflight: `Test-Path`, `Get-Item`, `Get-FileHash`, запись результата в evidence.
- Extension Development Host нужен для будущего запуска, потому что обе сохраненные S5 попытки запускались с `--extensionDevelopmentPath` и тем же extension root.
- Extension root из evidence: `C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills`. Написание `reasearch` сохранено как evidence path; не исправлять на `research` без отдельного подтверждения.
- Route command из evidence: `xiigenSmartSkills.panel`. `route-manifest-summary.json` показывает его как contributed command, activation event и no-send route candidate.
- Исторический storage failure: `Storage Error` window и повторяющийся `SQLITE_CANTOPEN: unable to open database file` в S5 `user-data` и `user-data-quoted-retry` logs.
- Исторический desktop blocker: сохраненные screenshot metadata/redaction note говорят о Windows lock/PIN overlay; credential/PIN content redacted, credentials not captured and not entered.
- `prompt_typed = 0` и `prompt_sent = 0` поддержаны сохраненными packet fields, но это не low-level UI input audit.

## Exact future argument array

Нормативная future argument array для записи в `s5c-05-launch-command.json`:

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

Derived PowerShell-safe argument line for a future launcher that uses `Start-Process -ArgumentList` as one string:

```powershell
--new-window --disable-workspace-trust --user-data-dir "<future-run-root>\user-data-clean" --extensions-dir "<future-run-root>\extensions-clean" --extensionDevelopmentPath "C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills" --command xiigenSmartSkills.panel "C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills"
```

If a future executor uses `System.Diagnostics.ProcessStartInfo.ArgumentList`, pass the array elements directly and also save the derived quoted line for human review. If any future work order removes `--extensionDevelopmentPath`, it must record an explicit reviewed not-needed decision first; S5b evidence supports `extensionDevelopmentPath_required = true`.

## Isolated storage/temp strategy

Future run root template:

`C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-product-readiness-product-workflow\row10-s5c-clean-launch-no-send-YYYYMMDD-HHMMSS`

Required child paths:

- `user-data-clean`
- `extensions-clean`
- `temp`
- `logs`
- `screenshots-redacted` only if a future UI/screenshot work order explicitly allows screenshots

Rules:

- Do not reuse S5 `user-data`, `user-data-quoted-retry`, `extensions`, or `extensions-quoted-retry`.
- Before launch, create the future root and the three clean directories.
- Run write/delete probes only inside the new future root paths and save the result.
- For the future launch process only, set `TEMP` and `TMP` to `<future-run-root>\temp` if the future work order permits environment override.
- Save path lengths and resolved paths. If any path cannot be created, written, deleted, or resolved, block before launch.
- After launch, search only the new future root logs for `Storage Error`, `SQLITE_CANTOPEN`, `unable to open database file`, and `StorageIPC#init`.

## Stale Cursor process policy

S5b did not inspect live processes and did not stop anything.

Future policy:

- Future execution may discover Cursor processes only if the future work order explicitly allows process discovery.
- Discovery output must include process id, executable path, command line, main window title if available, and whether the process uses the future clean dirs, prior S5 dirs, default Cursor dirs, or unknown storage.
- Stop/kill/close is forbidden unless a future work order explicitly authorizes it.
- If stale Cursor processes are discovered and the future run would require closing or reusing them, stop the scenario with `BLOCKED_STALE_PROCESS_POLICY_MISSING`.
- If the future work order explicitly accepts coexistence with unrelated Cursor processes and the launch uses clean isolated dirs, record that acceptance before launch.

Blocked marker:

`BLOCKED_STALE_PROCESS_POLICY_MISSING`

## Desktop foreground/lock overlay preflight

Before any future launch/focus/UI capture:

1. Confirm the desktop is unlocked.
2. Confirm no Windows lock, PIN, account, password, token, or secret-bearing surface is visible.
3. Confirm the intended monitor/window target is known; S5 evidence used `\\.\DISPLAY2` with 1920x1080 bounds.
4. If screenshots are authorized, capture only redacted screenshots and save a redaction note. If screenshot safety is uncertain, do not capture.
5. If a lock/PIN overlay appears, block with `BLOCKED_DESKTOP_LOCK_OVERLAY` and do not type credentials.
6. No prompt may be typed or sent. No model-backed response may be invoked.

## Future evidence folder and filenames

Future execution packet folder:

`C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-product-readiness-product-workflow\row10-s5c-clean-launch-no-send-YYYYMMDD-HHMMSS`

Required filenames:

- `s5c-00-source-readback.json`
- `s5c-01-cursor-path-validation.json`
- `s5c-02-storage-temp-preflight.json`
- `s5c-03-stale-process-discovery.json`
- `s5c-04-desktop-foreground-preflight.json`
- `s5c-05-launch-command.json`
- `s5c-06-start-process-result.json`
- `s5c-07-window-process-inventory.json`
- `s5c-08-storage-log-search.txt`
- `s5c-09-no-send-route-discovery-redacted.png` only if future UI/screenshot scope explicitly allows and screenshot is safe
- `s5c-09-screenshot-redaction-note.json` only if screenshot is captured
- `s5c-10-run-summary.md`
- `s5c-10-run-summary.json`

## Future PASS criteria

Prep/details PASS for this S5b packet requires:

- Cursor executable strategy is filled from existing evidence and downgraded as not live-verified.
- Future argument array includes clean `--user-data-dir`, clean `--extensions-dir`, required `--extensionDevelopmentPath`, `--command xiigenSmartSkills.panel`, and the extension root final path argument.
- Storage/temp strategy avoids prior failed S5 storage dirs and names exact future directories.
- Stale process policy is discover-only and blocks if stop authority is missing.
- Desktop foreground/lock overlay preflight is explicit.
- Future evidence folder and filenames are explicit.
- Safety guards say no prompt typed/sent, no secrets, no readiness claims.
- JSON parses and agrees with this Markdown.

Future launch/run PASS, not claimed here, would require:

- No-launch path validation passed immediately before launch.
- Storage/temp write-delete probes passed inside the future root.
- Stale process discovery either found no blocking stale process or recorded an explicit coexistence/close authority from the future work order.
- Desktop preflight passed immediately before launch/focus.
- Exact args were saved before launch.
- No `Storage Error`, no `SQLITE_CANTOPEN`, and no `unable to open database file` in future saved evidence.
- Route discovery reached `xiigenSmartSkills.panel` without prompt typed and without prompt sent.
- Any screenshot is redacted and documented as no secrets captured.
- Claim remains route/no-send diagnostic only, not product readiness.

## Future BLOCKED criteria

- Cursor exe path is absent in no-launch future validation: `BLOCKED_CURSOR_EXE_NOT_FOUND`.
- Extension root is absent in no-launch future validation: `BLOCKED_EXTENSION_ROOT_NOT_FOUND`.
- The future work order removes `--extensionDevelopmentPath` without explicit reviewed not-needed decision: `BLOCKED_EXTENSION_DEVELOPMENT_PATH_DECISION_MISSING`.
- Clean dirs cannot be created or write/delete probes fail: `BLOCKED_STORAGE_PREFLIGHT_FAILED`.
- Stale Cursor process handling would require stop/close and no explicit stop authority exists: `BLOCKED_STALE_PROCESS_POLICY_MISSING`.
- Desktop is locked or a PIN/account/password overlay is visible: `BLOCKED_DESKTOP_LOCK_OVERLAY`.
- Screenshot would capture secrets or cannot be safely redacted: `BLOCKED_SCREENSHOT_SECRET_RISK`.
- Cursor shows `Storage Error` or logs contain `SQLITE_CANTOPEN`: `BLOCKED_CURSOR_STORAGE_ERROR`.
- Any prompt would need to be typed or sent: `BLOCKED_NO_SEND_GUARD`.
- Any future evidence filename is missing: `BLOCKED_EVIDENCE_INCOMPLETE`.

## Safety boundary

- No Cursor launch happened in S5b.
- No UI automation happened in S5b.
- No live process discovery, stop, kill, or close happened in S5b.
- No product/runtime execution happened in S5b.
- No tests/builds happened in S5b except allowed JSON/text checks after packet creation.
- No prompt was typed or sent.
- No secrets were captured.
- No product readiness, desktop readiness, storage recovery, or XIIGen panel readiness is claimed.
- No code/status/git edits were made by this packet.
- No web/external AI and no nested sub-agents were used.

## Machine fields

- `packet_type`: `row10_s5b_launch_diagnostic_details_prep`
- `verdict`: `PASS_DETAILS_PREP`
- `prep_only`: `true`
- `cursor_exe_strategy_status`: `KNOWN_FROM_EXISTING_EVIDENCE_NOT_LIVE_VERIFIED`
- `cursor_exe`: `C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe`
- `extensionDevelopmentPath_required`: `true`
- `extension_root`: `C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills`
- `route_command`: `xiigenSmartSkills.panel`
- `future_run_root_template`: `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-product-readiness-product-workflow\row10-s5c-clean-launch-no-send-YYYYMMDD-HHMMSS`
- `stale_process_blocked_marker`: `BLOCKED_STALE_PROCESS_POLICY_MISSING`
- `desktop_lock_blocked_marker`: `BLOCKED_DESKTOP_LOCK_OVERLAY`
- `storage_error_blocked_marker`: `BLOCKED_CURSOR_STORAGE_ERROR`
- `prompt_typed_allowed`: `false`
- `prompt_sent_allowed`: `false`
- `readiness_claim_allowed`: `false`
