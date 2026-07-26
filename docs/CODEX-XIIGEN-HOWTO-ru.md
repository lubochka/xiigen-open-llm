# Codex XIIGen HOWTO

This document describes the intended Codex plugin/app/chat/app-server path. Current evidence is plugin-cache adapter smoke only unless a separate live Codex UI/app packet proves more. This doc does not claim full Codex desktop chat UI/app readiness.

## Total / Done / Now / Remaining

| Field | Status |
|---|---|
| Total | A Codex surface path is needed: plugin/app/chat/app-server -> thin adapter -> core request -> visible answer/error/debug/correlation evidence. |
| Done | Docs now separate installed plugin/cache adapter smoke from real Codex UI/app runtime proof. |
| Now | Codex full UI/app proof is not closed; token pairs 0/9; quality rows 0/9; DPO rows 0/9; lifecycle rows 0/9; final reviews 0/10. |
| Remaining | Live Codex UI/app packet, then token/quality/learnability tracks and final reviews after real evidence. |

## Intended Codex path

1. User triggers the Codex plugin, app command, chat integration or app-server surface.
2. Thin adapter creates `XiigenExtensionRequest` with host `Codex`, method id, prompt/task text, debug flag, provenance and correlation id.
3. Adapter calls `XiigenExtensionApiService` in `llm_mvp_core`.
4. Codex surface displays visible answer, warnings, errors and optional debug trace.
5. Evidence packet records plugin/app surface, request/response refs, correlation id, timestamp/freshness and redaction status.

## Plugin/cache smoke vs live UI/app runtime

Installed plugin/cache adapter smoke can show that a package or adapter boundary exists and can format a request. It cannot prove that the Codex desktop chat UI/app surface loaded it, displayed the answer, handled negative/error path or preserved debug evidence.

Safe wording: "Codex adapter smoke is narrow support evidence."

Unsafe wording: "Codex app/chat readiness is proven by plugin-cache smoke."

## Runtime proof checklist

- Named Codex surface: plugin, app, chat, app-server or command.
- User-visible answer or controlled error.
- Core request/response evidence tied to `XiigenExtensionApiService`.
- Correlation id in host output and debug/evidence packet.
- Redacted raw input/output or clear reason why raw view is suppressed.
- Fresh same-run timestamp.
- No hidden deterministic host-side semantic logic.

## Short code anchors

`43.SmartSkillsDomain/XiigenExtensionApiService.cs`:

```csharp
var debugTrace = BuildDebugTrace(request, promptSha256, responseWithoutTrace);
```

This anchor matters because Codex proof must preserve debug/correlation evidence from the core boundary. The code line itself does not prove Codex UI/app runtime.

## Current limitation

This docs-only slice does not prove full Codex desktop chat UI/app readiness, token savings, quality improvement, semantic quality, learnability, checkpoint promotion, target promotion, locator update or final readiness.
