# Claude XIIGen HOWTO

This document describes the intended Claude path for XIIGen smart skills. It is not a proof packet. The current Claude evidence boundary stays narrow/no-eval unless a separate live Claude runtime packet proves the exact Claude Desktop/chat/command surface.

## Total / Done / Now / Remaining

| Field | Status |
|---|---|
| Total | A live Claude path is needed: Claude surface -> thin adapter -> `XiigenExtensionRequest` -> `XiigenExtensionApiService` -> visible answer/error/debug/correlation evidence. |
| Done | Docs now explain the intended Claude boundary and what evidence must exist. |
| Now | Full UI/chat readiness is not closed. Token pairs 0/9, quality rows 0/9, trainable DPO rows 0/9, lifecycle rows 0/9, final reviews 0/10. |
| Remaining | A separate Claude runtime proof packet is needed, then token/quality/learnability tracks. |

## Intended Claude path

1. The user invokes a Claude command, plugin hook or chat action.
2. Claude adapter reads the user prompt and host context.
3. Adapter creates `XiigenExtensionRequest` with host name `Claude`, host type, method id, prompt/task text, debug flag, provenance and correlation id.
4. Adapter calls `XiigenExtensionApiService.ProcessHostExtensionRequest`.
5. Adapter displays `XiigenExtensionResponse.ChatDisplayText`, warnings, errors and optional redacted debug trace in Claude-visible form.
6. Evidence packet records same-run input/output refs, correlation id, timestamp/freshness and redaction status.

## Runtime proof checklist

A future Claude proof packet must contain:

- visible Claude answer or controlled error from the real Claude surface;
- input prompt or redacted input ref;
- returned result or redacted output ref;
- warnings/errors/debug trace when relevant;
- correlation id present in request, response and evidence;
- freshness: one run, one timestamped packet, no stale transcript authority;
- proof that logic stayed in `llm_mvp_core`, not in Claude-side deterministic shortcuts.

## Debug/error/correlation evidence

Debug evidence should show method id, redacted input/output, correlation id, timestamp/freshness and redaction status. A controlled error is useful only when it is visible to Claude user surface and tied to the same correlation id. Debug evidence alone is not readiness proof.

## Short code anchors

`43.SmartSkillsDomain/XiigenExtensionApiContracts.cs`:

```csharp
public sealed record XiigenExtensionRequest(
public sealed record XiigenExtensionResponse(
```

`43.SmartSkillsDomain/XiigenExtensionApiService.cs`:

```csharp
public OperationResult<XiigenExtensionResponse> ProcessHostExtensionRequest(
```

These are core anchors. This docs-only slice did not confirm an exact Claude adapter file path inside this repository. A future runtime-map work order must name the actual Claude adapter file/entrypoint instead of inventing one.

## Current limitation

Current Claude evidence is narrow/no-eval and cannot be upgraded silently to full Claude Desktop/chat UI readiness. It does not prove token savings, semantic quality, learnability, DPO training, checkpoint promotion, target promotion, locator update or final readiness.
