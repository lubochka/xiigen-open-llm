# Cursor Real-Program UI Preflight Packet

## Human-readable status

Total steps: 1
Done: 1
In progress: 0
Remaining: 0

The Cursor window was found and its monitor/bounds were captured. The UI route to XIIGen was not attempted beyond safe focus and screenshots because the visible local desktop was at Windows PIN entry, so typing or prompt submission would be unsafe and impossible to verify.

## Exact windows found

- Cursor target: title "[Extension Development Host] cursor-xiigen-smart-skills - Cursor"; process "Cursor"; handle "0x2205FE" / 2229758; bounds left 460, top 140, right 1460, bottom 940, width 1000, height 800.
- Cursor dialog: title "Storage Error"; process "Cursor"; handle "0x2072E" / 132910; bounds left 666, top 374, right 1238, bottom 596, width 572, height 222.
- Monitor \\.\DISPLAY1: non-primary; bounds left -1920, top 0, width 1280, height 720.
- Monitor \\.\DISPLAY2: primary; bounds left 0, top 0, width 1920, height 1080.

## Session observation

Cursor app login state could not be inspected. The local desktop appeared at Windows PIN entry. No credentials were typed and no secrets were captured intentionally.

Redacted screenshot: C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-full-extension-runtime\cursor-preflight-20260627-071934\03-redacted-session-lock.png

## XIIGen route attempted

Attempted route: identify Cursor extension-development-host window, focus it by handle, capture target-window screenshots.

Not attempted: Ctrl+Shift+X, extension search typing, command-palette route, chat prompt entry, or dialog dismissal. Reason: visible surface was Windows PIN entry, so the real Cursor UI was not available.

Screenshots:
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-full-extension-runtime\cursor-preflight-20260627-071934\01-cursor-window-initial.png
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-full-extension-runtime\cursor-preflight-20260627-071934\02-storage-error-dialog.png
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-full-extension-runtime\cursor-preflight-20260627-071934\03-redacted-session-lock.png

## Verdict

BLOCKED_LOGIN_REQUIRED

## Concrete next repair

After Luba unlocks the Windows session, run a new bounded preflight on the same target: focus [Extension Development Host] cursor-xiigen-smart-skills - Cursor, confirm the visible Cursor UI, dismiss only non-secret blocking dialogs if present, open Extensions with Ctrl+Shift+X or route through Command Palette, search for XIIGen without submitting a prompt, and capture redacted route screenshots.

## Changed evidence files

- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-full-extension-runtime\cursor-preflight-20260627-071934\initial-processes.json
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-full-extension-runtime\cursor-preflight-20260627-071934\top-level-windows.json
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-full-extension-runtime\cursor-preflight-20260627-071934\01-cursor-window-initial.png
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-full-extension-runtime\cursor-preflight-20260627-071934\02-storage-error-dialog.png
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-full-extension-runtime\cursor-preflight-20260627-071934\03-redacted-session-lock.png
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-full-extension-runtime\cursor-preflight-20260627-071934\session-preflight-facts.json
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-full-extension-runtime\cursor-preflight-20260627-071934\preflight-packet.json
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-full-extension-runtime\cursor-preflight-20260627-071934\preflight-packet.md
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-full-extension-runtime\cursor-preflight-20260627-071934\finalize-cursor-preflight.ps1

## Evidence boundary

Marker-only/window-only is not extension readiness. The evidence supports only window discovery plus local login/session block.
