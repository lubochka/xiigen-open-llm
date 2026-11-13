# RUNTIME-01 host reality map

Generated: 2026-06-17T21:40:23+03:00
Workspace: `C:\Projects\llm_mvp_core`
Branch: `codex/chat-gpt-5.5`

## Scope

RUNTIME-01 создал карту реальности хостов Claude, Cursor и Codex. Это не live runtime probe, не token/quality/learnability gate и не final readiness. Свежие runtime-команды были только безопасные: `cursor --version` и `codex --version`. Всё остальное помечено как `INSPECTED_PATH_ONLY`, а старые QSG/phase40 файлы помечены как `HISTORICAL_NOT_RERUN`.

## Claude

- Runtime path: `INSPECTED_PATH_ONLY`. Найден Claude Appx `C:\Program Files\WindowsApps\Claude_1.13576.0.0_x64__pzs8sxrjxfjjc`, executable `app\claude.exe`, и Claude Code cache `C:\Users\lubao\AppData\Roaming\Claude\claude-code\2.1.177\claude.exe`.
- CLI path: `claude` не найден в PATH. Старый путь из metadata `2.1.170\claude.exe` отсутствует.
- Plugin repo/loadable path: `C:\Projects\claude-xiigen-smart-skills` существует. `.claude-plugin\plugin.json` содержит `SessionStart` command hook, но `hooks\hooks.json` пустой.
- Installed extension reality: в Claude AppData installed extensions найдены Figma/Filesystem/AWS/Kubernetes/Postman, но xiigen extension не найден.
- Entrypoint: `src\claude-entrypoint.js` и `src\claude-entrypoint.ps1` существуют; source поддерживает `DebugEnabled`, `RequestedSkills`, `AvailableSkills`, `TargetLocatorPath/Json`, `UserAdaptiveAssetRoot`, `RollbackAssetRoot`, `LocatorMode`.
- Debug setting: repo metadata объявляет `xiigenSmartSkills.debug` default=false method=hook; live AppData setting/install path для xiigen не найден.
- Core API / target locator: entrypoint умеет TargetLocator, core path существует; текущий plugin hook command target locator не передает.
- Gap: plugin load/install для Claude не доказан, live hook/chat runtime не доказан, metadata path надо обновить под 2.1.177.
- Next bounded work order: `HOST-IMPL-CLAUDE-01: reconcile Claude plugin metadata/install path to current Claude Desktop/Appx and Claude Code 2.1.177, prove xiigen SessionStart hook installed/loadable, and verify target-locator/debug setting wiring before RUNTIME-02.`

## Cursor

- Runtime path: `PASS_RUNTIME_PATH`. `cursor --version` вернул `3.5.38 / 009bb5a3600dd98fe1c1f25798f767f686e14750 / x64`.
- Extension repo/loadable path: `C:\Projects\cursor-xiigen-smart-skills` существует; `package.json` объявляет `main=./dist/extension.js`.
- Entrypoints: command `xiigen.smartSkills.select`, panel `xiigenSmartSkills.panel`, chat participant `xiigen-smart-skills`; source `src\extension.ts`, runtime artifact `dist\extension.js`.
- Installed extension reality: `C:\Users\lubao\.cursor\extensions\extensions.json` содержит `[]`; full installed Cursor extension не доказан.
- Debug setting: `package.json contributes.configuration xiigenSmartSkills.debug default=false`; user settings не задают этот key.
- Core API / target locator: package/source/dist поддерживают `targetLocatorPath`, `targetLocatorJson`, `userAdaptiveAssetRoot`, `rollbackAssetRoot`, `locatorMode`; core API exists.
- Fresh vs historical: свежий только path/version/manifest inspection; QSG-29 Cursor AvailableSkills/RequestedSkills binding остается historical narrow evidence.
- Gap: extension load/install and full Cursor chat UI runtime остаются не доказаны.
- Next bounded work order: `HOST-IMPL-CURSOR-01: install or dev-load Cursor extension and prove command, panel, chat participant, debug default, skill arrays, and target-locator pass-through without broad readiness claims.`

## Codex

- Runtime path: `PASS_RUNTIME_PATH`. `codex --version` завершился с exit 0; version text не вывел. Codex Appx: `OpenAI.Codex_26.609.4994.0_x64__2p2nqsd0c76g0`.
- Plugin repo/loadable path: `C:\Projects\codex-xiigen-smart-skills` существует.
- Installed plugin reality: `C:\Users\lubao\.codex\config.toml` включает `codex-xiigen-smart-skills@xiigen-local`; cache path существует: `C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500`.
- Entrypoint: current repo `src\codex-entrypoint.js` поддерживает `TargetLocator`, target roots и locator mode; installed cache source/package в RUNTIME-01 выглядит старее и не содержит TargetLocator support/metadata.
- Debug setting: repo package объявляет `xiigenSmartSkills.debug` default=false / `DebugEnabled`; explicit Codex config toggle не найден.
- Core API / target locator: current repo supports target locator and core API exists; installed cache must be refreshed before full target-locator runtime claim.
- Gap: installed plugin cache drift blocks full Codex extension readiness; installed skill/plugin smoke не запускался в RUNTIME-01.
- Next bounded work order: `HOST-IMPL-CODEX-01: refresh installed xiigen-local cache from current Codex repo to include TargetLocator, then run non-AI installed-plugin smoke for adapter/core boundary only.`

## Fresh vs historical

Fresh evidence: local command/path/config/package/source inspection, `cursor --version`, `codex --version`, Appx/StartApps inventory.
Historical evidence: phase40 reports and QSG20/QSG21/QSG28/QSG29 artifacts; they were referenced only as history and not used as fresh live runtime proof.

## Readiness blockers

1. Claude: xiigen plugin not found as installed in Claude AppData; stale Claude Code 2.1.170 metadata vs current 2.1.177.
2. Cursor: extension repo exists but user extension inventory is empty; live chat/UI readiness not proven.
3. Codex: enabled installed plugin cache exists but is stale for TargetLocator compared with current repo.
4. No token savings, broad semantic quality, learnability, production promotion, or final readiness claim is opened by RUNTIME-01.

## Next

First bounded work order in state: `HOST-IMPL-CLAUDE-01 first: reconcile/install/load Claude xiigen plugin against current Claude Desktop/Appx and Claude Code 2.1.177, prove hook plus target-locator/debug setting wiring without external AI/full runtime probe, then continue to Cursor and Codex host repair work orders listed in runtime-map.json.`
