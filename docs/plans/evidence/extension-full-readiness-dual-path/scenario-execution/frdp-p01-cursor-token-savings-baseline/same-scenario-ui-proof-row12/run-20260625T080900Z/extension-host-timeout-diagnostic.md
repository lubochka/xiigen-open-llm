# CURSOR_EXTHOST_TIMEOUT_DIAGNOSTIC_PACKET

status: HOST_STARTS_LATE_LAUNCHER_WAIT_FIX

## Scope

- Active task source: CURRENT TASK: Diagnose the Cursor extension-host startup timeout under the repaired short-temp launch. Determine exact cause and repair if narrow/safe. Do not run row12 UI proof.
- Git authority: forbidden.
- Row12 UI proof run: false.
- Login/browser recovery run: false.
- External AI used: false.
- Readiness claimed: false.

## Diagnosis

The reported timeout is a late-start condition, not evidence that the extension host never started.

- Timeout: `renderer.log:119` has `Extension host did not start in 10 seconds`.
- Host start: `exthost.log:1` has `Extension host with pid 69432 started` at `2026-06-25 11:42:30.554`.
- Responsive: `renderer.log:129` has `Extension host (LocalProcess [role: user] pid: 69432) is responsive` at `2026-06-25 11:42:39.990`.
- Launch record time: `2026-06-25T08:42:11.4226937Z`.

The previous launcher slept only 8 seconds after invoking Cursor. In this observed run, the extension host started about 19 seconds after the launch record and became responsive about 28 seconds after the launch record, so the launcher wait/check was too short.

## Diagnostic Answers

1. Fresh timeout/crash/error files:
   `renderer.log` contains the 10-second timeout, unresponsive/responsive markers, unauthenticated Cursor service errors, and extension-host stderr warnings. `exthost.log` contains the extension-host start and activation timeline. `main.log` contains `Error mutex already exists`.
2. Extension host eventually started:
   yes. It started after Cursor's built-in 10-second timeout and later became responsive.
3. Log path still too deep/invalid:
   no fresh evidence. The short-temp logs were created successfully under `C:\Users\lubao\AppData\Local\Temp\xiigen-row12-cursor-devhost-20260625114202\user-data\logs\20260625T114206`, including `window1\exthost\exthost.log`. No fresh `SQLITE_CANTOPEN` finding was found in the inspected short-temp logs.
4. Launch flags missing required variants:
   not proven. `renderer.log:104` says Cursor loaded the development extension path. The workspace folder argument was present. I did not add `--disable-extensions` because no exact evidence showed it was required, and it could alter extension discovery.
5. Dist load under Node/V8:
   `node --check` passed for `dist\extension.js`. Top-level requires are `vscode`, `fs`, `path`, `crypto`, and `child_process`; `vscode` is expected inside the extension host.
6. Package main resolvable:
   yes, `package.json` main is `./dist/extension.js` and resolves to an existing file.
7. Cursor-specific devhost flags:
   not proven by this timeout diagnostic. Cursor recognized the existing development path.

## Fix Summary

Updated `launch-visible-cursor-row12-corrected.ps1` to replace the fixed `Start-Sleep -Seconds 8` with a bounded wait for extension-host log signals:

- scans fresh `renderer.log` and `exthost.log` files under the launch's short `user-data\logs`;
- waits up to 45 seconds for the renderer `is responsive` signal;
- records timeout/start/unresponsive/responsive signals into `corrected-launch-record.json`.

## Verification Results

- PowerShell AST parse OK for `launch-visible-cursor-row12-corrected.ps1`.
- `node --check` passed for `C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\dist\extension.js`.
- `package.json` main resolved to an existing `dist\extension.js`.
- Existing `command-availability-after-short-temp-launch.json` parsed successfully.
- `extension-host-timeout-diagnostic.json` parsed successfully after creation.

## Next Smallest Action

Rerun the corrected launcher only when authorized for the narrow command-palette availability check. Do not treat this timeout repair as row12 readiness. If the XIIGen command remains absent after the host-responsive wait, split to extension-discovery/manifest validation because the timeout itself is no longer the blocker.
