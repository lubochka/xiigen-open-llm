# Phase 14 real-program screenshot proof packet

## Вердикт

PARTIAL_VISIBLE_SCREEN_CONTROL_PACKET_READY_FOR_REVIEW_NOT_FINAL_READY.

План исправлен: да. В активный Phase 14 добавлены Tier A/Tier B/Tier C для real-program UI/screenshot proof, включая authorized visible current real program UI / sub-agent visible runtime surface + screenshot + same-run trace как возможный DONE_WITH_SCOPE. Adapter/component/core smoke оставлен только как Tier C supporting evidence, не как полный UI proof.

Финальная готовность не заявлена. 14.04 можно передавать на повторный review этого packet, но packet сам не закрывает все строки автоматически: Claude и Codex имеют явные ограничения, описанные ниже.

## Host status

| Host | Visible program/control | Screenshot | Visible result | Same-run trace | Verdict |
| --- | --- | --- | --- | --- | --- |
| Claude | Да: Start-Process explorer.exe shell:AppsFolder\Claude_pzs8sxrjxfjjc!Claude, затем ShowWindow/SetForegroundWindow для claude.exe pid 46572 | Да, raw private + redacted review copy | Нет: prompt/result не вводился, чтобы не отправлять запрос во внешний AI/private live chat | Да: P14-FIX-08B-CLAUDE-20260624T024334Z | PARTIAL_VISIBLE_CLAUDE_UI_CAPTURE_NO_BOUNDED_VISIBLE_RESULT |
| Cursor | Да: real Cursor.exe isolated scratch window; чистый capture через SetWindowPos(HWND_TOPMOST) и release | Да | Да: visible file/title/content show P14-FIX-08B-CURSOR-20260624T024334Z and PASS_WITH_SCOPE | Да: P14-FIX-08B-CURSOR-20260624T024334Z | DONE_WITH_SCOPE_REAL_CURSOR_UI_SCREENSHOT_AND_CORE_TRACE |
| Codex | Да: visible Codex app/sub-agent runtime surface; adapter route invoked through installed xiigen smart-skills Codex entrypoint | Да, raw private + redacted review copy | Да, как текущая Codex runtime surface, но не isolated desktop chat | Да: P14-FIX-08B-CODEX-20260624T030000Z | PARTIAL_VISIBLE_CODEX_RUNTIME_SURFACE_ADAPTER_TRACE_CORRELATION_NOT_VISIBLE_IN_SCREENSHOT |

## Ограничения без overclaim

- inal_readiness_claimed=false.
- Semantic quality, token savings, learnability, training, DPO, checkpoint promotion, and full app internals are not claimed.
- Claude screenshot proves real visible Claude/Claude Code surface control, but not a current bounded visible answer.
- Cursor screenshot is the strongest host row in this packet: real Cursor UI with correlation visible and same-run core trace, but not a manual Cursor chat proof.
- Codex screenshot proves a real visible Codex runtime/sub-agent surface and the same run has adapter/core trace, but the screenshot does not visibly show the same correlation id.
- Raw Claude/Codex screenshots include unrelated live session content and are marked private; redacted copies are provided for review-safe inspection.

## Evidence files

- Packet JSON: phase14-real-program-screenshot-proof.json
- Claude redacted screenshot:
aw/claude/redacted-visible-claude-ui.png
- Claude raw private screenshot:
aw/claude/retry-after-foreground-window.png
- Claude trace:
aw/claude/same-run-core-trace/request.json,
esponse.json, 	race.json
- Cursor clean screenshot:
aw/cursor/retry-correlation-window-topmost-clean.png
- Cursor visible scratch file:
aw/cursor/visible-scratch/P14-FIX-08B-CURSOR-20260624T024334Z-PASS.md
- Cursor trace:
aw/cursor/same-run-core-trace/request.json,
esponse.json, 	race.json
- Codex redacted screenshot:
aw/codex/redacted-visible-codex-surface.png
- Codex raw private screenshot:
aw/codex/retry-visible-runtime-marker-window.png
- Codex adapter/core trace:
aw/codex/same-run-codex-adapter-trace/request.json,
esponse.json, 	race.json, codex-adapter-output.log

## Git boundary

No git commands were run. git_authority=forbidden, llowed_git_commands=NONE, commit_push_authorized=false.
