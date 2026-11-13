# Local Surface Evidence

packet_id: `p14-fix-06-codex-ui-hook-resolution-20260623-235636`

## Commands Run

No git commands were run.

Read-only/local inspection commands used:

- Read OpenAI-docs skill instructions.
- Attempted Codex manual helper:
  - `node C:/Users/lubao/.codex/skills/.system/openai-docs/scripts/fetch-codex-manual.mjs`
  - Result: failed before manual fetch due Windows TLS revocation check failure: `CRYPT_E_NO_REVOCATION_CHECK`.
- Listed existing Codex UI proof artifact directories.
- Listed files under the installed XIIGen plugin package.
- Read installed XIIGen plugin manifest, skill file, package metadata, and legacy `.codex` config.
- Checked hook/config file presence.
- Ran a sanitized keyword pass over user Codex config for hook/plugin/MCP/XIIGen references.

## Installed Plugin Manifest

Path:

`C:/Users/lubao/.codex/plugins/cache/xiigen-local/codex-xiigen-smart-skills/0.1.0+codex.runtime04e-20260616-231500/.codex-plugin/plugin.json`

Relevant observed fields:

```json
{
  "name": "codex-xiigen-smart-skills",
  "version": "0.1.0+codex.runtime04e-20260616-231500",
  "skills": "./skills/",
  "interface": {
    "displayName": "XIIGen Smart Skills for Codex",
    "shortDescription": "Thin Codex plugin shell for XIIGen smart-skills.",
    "capabilities": ["Interactive", "Write"]
  }
}
```

Finding: no `hooks`, `apps`, or `mcpServers` manifest entry was present.

## Installed Skill

Path:

`C:/Users/lubao/.codex/plugins/cache/xiigen-local/codex-xiigen-smart-skills/0.1.0+codex.runtime04e-20260616-231500/skills/xiigen-smart-skills/SKILL.md`

Observed boundary:

- The skill routes a bounded installed-plugin smoke through `src/codex-entrypoint.js`.
- It invokes `C:/Projects/llm_mvp_core/tools/xiigen-extension-core-api.ps1`.
- It records request/response/trace evidence.
- It explicitly limits the proof to installed skill surface plus adapter/core boundary, not full Codex chat/app readiness.

Finding: this is not a visible Codex app/plugin UI submission/capture hook.

## Package Metadata

Path:

`C:/Users/lubao/.codex/plugins/cache/xiigen-local/codex-xiigen-smart-skills/0.1.0+codex.runtime04e-20260616-231500/package.json`

Relevant observed fields:

```json
{
  "main": "src/codex-entrypoint.js",
  "codexXiigenSmartSkills": {
    "sourceEntrypoint": "src/codex-entrypoint.js",
    "sourceEntrypointInvokesCoreEndpoint": true,
    "coreEndpoint": "C:/Projects/llm_mvp_core/tools/xiigen-extension-core-api.ps1",
    "hostLogicBoundary": "thin-request-client-and-redacted-debug-display-only",
    "runtimeSmokeClaimed": false,
    "semanticQualityClaimed": false
  }
}
```

Finding: package metadata confirms a thin adapter/core boundary, not a visible UI capture mechanism.

## Legacy Codex Config In Plugin Package

Path:

`C:/Users/lubao/.codex/plugins/cache/xiigen-local/codex-xiigen-smart-skills/0.1.0+codex.runtime04e-20260616-231500/.codex/xiigen-smart-skills.json`

Relevant observed fields:

```json
{
  "entrypoint": {
    "type": "codex_command",
    "name": "xiigen.smartSkills.select",
    "command": "xiigen.smartSkills.select"
  },
  "phase40": {
    "scope": "manifest-package-skeleton-only"
  }
}
```

Finding: this names a command-style entrypoint but does not provide a sanctioned visible Codex chat/app UI submission plus rendered-response capture hook.

## Hook Presence Check

Sanitized result:

```json
[
  {
    "Path": "C:/Users/lubao/.codex/hooks.json",
    "Exists": false
  },
  {
    "Path": "C:/Users/lubao/.codex/config.toml",
    "Exists": true,
    "Length": 3571
  },
  {
    "Path": "C:/Projects/llm_mvp_core/.codex/hooks.json",
    "Exists": false
  },
  {
    "Path": "C:/Projects/llm_mvp_core/.codex/config.toml",
    "Exists": false
  },
  {
    "Path": "C:/Users/lubao/.codex/plugins/cache/xiigen-local/codex-xiigen-smart-skills/0.1.0+codex.runtime04e-20260616-231500/hooks/hooks.json",
    "Exists": false
  }
]
```

Additional check:

```text
NO_PLUGIN_HOOKS_DIRECTORY
```

## User Config Keyword Pass

Command pattern:

`hooks|plugins|mcp_servers|xiigen|codex-xiigen|UserPromptSubmit|SessionStart|Stop`

Sanitized hits:

```text
16:source = '\\?\C:\Users\lubao\.cache\codex-runtimes\codex-primary-runtime\plugins\openai-primary-runtime'
18:[marketplaces.xiigen-local]
21:source = '\\?\C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04D-codex\20260616-225247\xiigen-local'
23:[plugins."documents@openai-primary-runtime"]
26:[plugins."spreadsheets@openai-primary-runtime"]
29:[plugins."presentations@openai-primary-runtime"]
32:[plugins."github@openai-curated"]
35:[plugins."pdf@openai-primary-runtime"]
38:[plugins."browser@openai-bundled"]
41:[plugins."codex-xiigen-smart-skills@xiigen-local"]
81:[mcp_servers.node_repl]
86:[mcp_servers.node_repl.env]
```

Finding: plugin/MCP config exists, but no hook event keyword or visible UI submission/capture hook was found.
