# HOST-IMPL-CLAUDE-01 Claude install/load map

Verdict: `PASS_INSTALL_LOAD_SESSION_ONLY_HOOK_WIRING`.

Это узкий `PASS_INSTALL_LOAD` для Claude Code 2.1.177: session-only load через `--plugin-dir` и локальная проверка `SessionStart` hook wiring. Это не AppData install, не Claude chat UI proof, не token savings, не broad semantic quality и не final readiness.

## Что найдено

- Claude Desktop/Appx: `Claude_1.13576.0.0_x64__pzs8sxrjxfjjc`, version `1.13576.0.0`, executable exists: `true`.
- Claude Code: `C:/Users/lubao/AppData/Roaming/Claude/claude-code/2.1.177/claude.exe`, `2.1.177 (Claude Code)`.
- Старый путь `C:/Users/lubao/AppData/Roaming/Claude/claude-code/2.1.170/claude.exe` отсутствует.
- Claude extension repo: `C:/Projects/claude-xiigen-smart-skills`, plugin manifest: `.claude-plugin/plugin.json`.
- Claude AppData extensions: xiigen не найден; обнаружены Figma, Filesystem, AWS API MCP Server, Kubernetes MCP Server, Postman MCP Server (Minimal).

## Что исправлено

- `.claude-plugin/plugin.json`: `SessionStart` command теперь передает `-TargetLocatorPath`, `-TargetLocatorRequired`, `-LocatorMode target`, `-DebugEnabled`, текущий `C:\Projects\llm_mvp_core`, и пишет fresh evidence в `artifacts/extension-runtime-token-quality-learnability/host-impl-claude-01/local-hook`.
- `package.json`: stale Claude Code `2.1.170` заменен на `2.1.177`; добавлены session load command, artifact root, target locator path/mode и debug flag.
- `README.md`: зафиксирован текущий session-only load path и честный scope.

## Доказательства

- `claude.exe plugin validate C:\Projects\claude-xiigen-smart-skills`: passed.
- `claude.exe --plugin-dir C:\Projects\claude-xiigen-smart-skills plugin list --json`: `claude-xiigen-smart-skills@inline`, scope `session`, enabled `true`.
- `claude.exe --plugin-dir C:\Projects\claude-xiigen-smart-skills plugin details claude-xiigen-smart-skills`: `Hooks (1) SessionStart`, always-on `~0 tok`.
- Локальный hook command: response success `true`, target locator bound `true`, debug redacted displayed `true`, external AI `false`, training `false`.
- Thin adapter evidence: request/host trace records `host_semantic_ranking=false`, `host_training=false`, `external_ai=false`, `host_model_logic=false`.

## Scope

Открыто:

- Claude Code 2.1.177 path discovered and version checked.
- Claude Desktop/Appx path discovered.
- Claude xiigen plugin repo and `.claude-plugin` manifest discovered.
- Claude Code session-only `--plugin-dir` load recognized one inline `SessionStart` hook.
- Plugin hook command references current core API, target locator path, `TargetLocatorRequired`, `LocatorMode target`, and `DebugEnabled`.
- Local hook command invoked core boundary and bound `TargetLocator` without external AI/full Claude prompt.

Закрыто:

- xiigen installed into Claude AppData.
- full Claude chat UI readiness.
- full Claude Desktop/Appx runtime proof.
- full Claude Code AI prompt proof.
- token savings.
- broad semantic quality.
- learnability/final readiness.
- production locator promotion.

Следующий bounded work order: `HOST-IMPL-CURSOR-01: install or dev-load Cursor extension and prove command, panel, chat participant, debug default, skill arrays, and target-locator pass-through without broad readiness claims.`
