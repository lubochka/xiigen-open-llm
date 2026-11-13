# Official Docs Evidence

packet_id: `p14-fix-06-codex-ui-hook-resolution-20260623-235636`

## Source Route

OpenAI-docs skill was loaded. The Codex manual helper was attempted first, but failed before fetching the manual because Windows TLS revocation checking failed with `CRYPT_E_NO_REVOCATION_CHECK`.

Because the docs MCP tools were not available in this session and the task did not authorize modifying Codex MCP configuration, official OpenAI web docs were used as fallback.

## Official Pages Checked

1. [Hooks - Codex](https://developers.openai.com/codex/hooks)
   - Finding: documents lifecycle hooks for deterministic scripts during Codex events.
   - Relevance: hooks can log, scan prompts, validate turns, and customize context, but this page did not show a supported visible Codex app/plugin UI submission plus rendered response capture hook.

2. [Advanced Configuration - Codex](https://developers.openai.com/codex/config-advanced)
   - Finding: documents loading lifecycle hooks from `hooks.json` or `[hooks]` tables in `config.toml`.
   - Relevance: confirms hook configuration locations and event schema reuse; did not establish a UI invocation/rendered capture API.

3. [Plugins - Codex](https://developers.openai.com/codex/plugins)
   - Finding: documents plugins as bundles of skills, app connections, and MCP servers.
   - Relevance: installed plugins can be used from the prompt window or selected via `@`, but no programmatic visible UI capture hook was found.

4. [Build plugins - Codex](https://developers.openai.com/codex/plugins/build)
   - Finding: documents plugin manifest structure, bundled MCP servers, and bundled lifecycle hooks.
   - Relevance: plugin hooks use the same event schema as regular hooks and must be trusted; no plugin UI submission/rendered-response export API was found.

5. [Use Codex with the Agents SDK](https://developers.openai.com/codex/guides/agents-sdk)
   - Finding: documents `codex mcp-server` so Codex can be invoked from other MCP clients.
   - Relevance: this may start Codex conversations through MCP, but it is not evidence of a visible Codex desktop/app/plugin UI rendered response capture path and was not exercised in this pass.

6. [Codex docs full export](https://developers.openai.com/codex/llms-full.txt)
   - Finding: documents Codex app as a desktop experience with project threads, skills support, and app features.
   - Relevance: useful surface map for Codex app behavior, but no sanctioned automated visible UI capture hook was found.

## Search Queries

Official-domain searches used:

```text
site:developers.openai.com/codex hooks plugins MCP Codex manual
site:developers.openai.com/codex Codex plugin hooks config.toml
site:developers.openai.com Codex desktop plugin API chat UI invocation rendered response capture
```

## Decision Impact

Official docs establish that Codex has lifecycle hooks, plugins, skills, apps, MCP support, and an MCP server mode. They do not establish the specific proof hook Phase 14.04 requires: a sanctioned path to submit a redacted XIIGen prompt through a visible Codex chat/app/plugin UI, capture the rendered response/error/debug output, and correlate it to a same-run `llm_mvp_core` trace without touching Luba's current live thread.
