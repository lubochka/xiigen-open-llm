# Phase 14.04 Codex UI Hook Resolution

packet_id: `p14-fix-06-codex-ui-hook-resolution-20260623-235636`
date: 2026-06-23
verdict: `NEEDS_PRODUCT_CHANGE_OR_EXPLICIT_DOWNSCOPE`

## CURRENT TASK

Resolve Phase 14.04 Codex UI blocker by doing a bounded sanctioned-hook research pass and producing a decision/evidence packet.

active_task_sentence_source: `Resolve Phase 14.04 Codex UI blocker by doing a bounded sanctioned-hook research pass and producing a decision/evidence packet.`

## NOT THE TASK

No implementation pass was performed. I did not edit source code, governance, skills, the main plan, or existing evidence. I did not stage, commit, push, or create/switch branches. I did not type a synthetic prompt into Luba's current live Codex conversation.

## EVIDENCE-ONLY

The prior board state, pointer state, previous adapter/core proof, and Lagrange's `NO_UI_HOOK_FOUND` result were treated as evidence to verify, not as authority to mark 14.04 done.

## Russian-First Decision

Phase 14.04 must remain `NEEDS_FIX` unless Luba explicitly accepts a downscope or Codex product/API support is added for a sanctioned visible UI proof path.

Reason: the installed XIIGen Codex skill/adapter/core path is real evidence for a thin plugin adapter calling `llm_mvp_core`, but it is not the same thing as a visible Codex chat/app/plugin UI submission and rendered response capture. The skill file itself states that its smoke proves only the installed skill surface plus adapter/core boundary, not full Codex chat/app readiness.

No safe already-installed hook was found that can:

1. Submit a redacted XIIGen prompt through an actual visible Codex chat/app/plugin UI surface.
2. Capture the rendered Codex response/error/debug result.
3. Correlate that visible response to a same-run `llm_mvp_core` trace.
4. Do all of the above without using Luba's current live thread or creating fake evidence.

Therefore a PASS would be an overclaim.

## Inspected Surfaces

Local installed XIIGen plugin package:

- `C:/Users/lubao/.codex/plugins/cache/xiigen-local/codex-xiigen-smart-skills/0.1.0+codex.runtime04e-20260616-231500/.codex-plugin/plugin.json`
- `C:/Users/lubao/.codex/plugins/cache/xiigen-local/codex-xiigen-smart-skills/0.1.0+codex.runtime04e-20260616-231500/skills/xiigen-smart-skills/SKILL.md`
- `C:/Users/lubao/.codex/plugins/cache/xiigen-local/codex-xiigen-smart-skills/0.1.0+codex.runtime04e-20260616-231500/package.json`
- `C:/Users/lubao/.codex/plugins/cache/xiigen-local/codex-xiigen-smart-skills/0.1.0+codex.runtime04e-20260616-231500/.codex/xiigen-smart-skills.json`
- default plugin hook path `.../hooks/hooks.json`

Local hook/config presence:

- `C:/Users/lubao/.codex/hooks.json`: absent
- `C:/Projects/llm_mvp_core/.codex/hooks.json`: absent
- `C:/Projects/llm_mvp_core/.codex/config.toml`: absent
- XIIGen plugin `hooks/` directory: absent
- user `C:/Users/lubao/.codex/config.toml`: present; sanitized keyword pass showed plugin marketplace/config entries and `node_repl` MCP entries, but no hook event keyword for `hooks`, `UserPromptSubmit`, `SessionStart`, or `Stop`

Official OpenAI docs checked:

- [Hooks - Codex](https://developers.openai.com/codex/hooks)
- [Advanced Configuration - Hooks](https://developers.openai.com/codex/config-advanced)
- [Plugins - Codex](https://developers.openai.com/codex/plugins)
- [Build plugins - Codex](https://developers.openai.com/codex/plugins/build)
- [Use Codex with the Agents SDK](https://developers.openai.com/codex/guides/agents-sdk)
- [Codex docs full export](https://developers.openai.com/codex/llms-full.txt)

Docs signal: official pages document lifecycle hooks, plugins with skills/apps/MCP/hooks, and Codex as an MCP server. I did not find an official Codex desktop/plugin API that safely drives a visible Codex app/plugin chat UI and exports rendered response capture for an isolated test thread.

## Why Adapter/Core Evidence Is Insufficient

The adapter/core smoke can show:

- installed skill surface loaded;
- adapter invoked `src/codex-entrypoint.js`;
- adapter produced request/response/trace artifacts;
- core endpoint boundary was reached.

It cannot honestly show:

- a real visible Codex chat/app/plugin UI submission;
- rendered Codex UI response capture;
- UI-visible error/debug display captured from Codex app;
- same-run correlation between a rendered Codex UI response and `llm_mvp_core` trace.

That means adapter/core proof is valuable but narrower than Phase 14.04's visible UI proof requirement.

## Product Change Needed

One of these product/API changes is needed to make 14.04 pass later:

1. Add a sanctioned Codex desktop/app test harness API that can create an isolated non-live test thread, submit a prompt with a selected plugin/skill, wait for completion, export rendered transcript/response/error/debug output, and expose a same-run correlation id or trace bridge.
2. Add a sanctioned plugin UI invocation/capture API that lets a plugin test submit to its visible Codex UI surface and export rendered output without mutating Luba's active conversation.
3. Add a first-party Codex evidence export for app/plugin messages, including rendered message content and a stable link to invoked tool/skill/core traces.

The minimum API shape needed for 14.04 is:

- `create_isolated_codex_ui_session(project, plugin_or_skill)`
- `submit_visible_ui_message(session_id, redacted_prompt)`
- `wait_for_rendered_response(session_id)`
- `export_rendered_response(session_id)`
- `export_invocation_trace(session_id)`
- `close_or_archive_test_session(session_id)`

## Honest Downscope Text

If Luba accepts a downscope, the honest replacement text would be:

```text
Phase 14.04 is explicitly downscoped from visible Codex chat/app/plugin UI proof to installed Codex skill adapter/core invocation proof. The accepted evidence proves the installed plugin skill can route a redacted request through the thin adapter into llm_mvp_core and produce redacted request/response/trace artifacts. It does not prove visible Codex chat/app UI submission, rendered response capture, or final Codex app/plugin readiness. Any final readiness claim must exclude visible Codex UI proof unless a sanctioned product hook is later added and exercised.
```

Without that explicit acceptance, 14.04 remains `NEEDS_FIX`.

## Boolean Result

- `hook_available`: false for the required visible Codex UI submission/capture hook.
- `hook_exercised`: false.
- `visible_codex_ui_submission_proven`: false.
- `rendered_response_capture_proven`: false.
- `same_run_core_correlation_proven`: false.
- `final_readiness_claimed`: false.

## No Overclaim Evidence

I did not verify a visible UI hook.
I did not run a visible Codex UI submission.
I did not capture a rendered Codex app/plugin response.
I did not use Luba's current live thread for synthetic input.
I did not claim final readiness.

## Changed Files

- `phase14-codex-ui-hook-resolution.md`
- `phase14-codex-ui-hook-resolution.json`
- `raw/local-surface-evidence.md`
- `raw/official-docs-evidence.md`

## Git

No git commands were run. No staging, commit, push, or branch action was performed.
