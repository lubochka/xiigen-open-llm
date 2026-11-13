# B3 — The tooling race (Claude Code / Copilot / Cursor)

**Trend.** AI coding tools — Claude Code, GitHub Copilot, Cursor — are in a fast-moving race, and many developers now work across several of them in a single multi-tool workflow.

**How this project relates.** LlmFromScratch exposes a thin host-extension API boundary so an external editor/agent host can send a concrete request to the core and get a concrete answer back — keeping the core logic inside `llm_mvp_core` and the host integration outside it.

**Where in the code.** `43.SmartSkillsDomain/XiigenExtensionApiContracts.cs:9` defines `record XiigenExtensionRequest` (with `HostName` / `HostType` / `Method`), and `43.SmartSkillsDomain/XiigenExtensionApiService.cs` is the service that serves that request. This is the request/response boundary, not the host integration.

**Orchestration.** A host request enters through `XiigenExtensionApiService`, is dispatched by its `Method` into the core, and the answer is returned across the same boundary; the extension layer stays a thin transport (a concrete task in, a concrete answer out) with all algorithm logic behind the boundary in the core.

**Honesty boundary.** The three host extensions (`claude-xiigen-smart-skills`, `codex-xiigen-smart-skills`, `cursor-xiigen-smart-skills`) are separate EXTERNAL repositories, NOT part of `llm_mvp_core`; the core ships only the request/response API boundary they call, not the extensions themselves. There is no full Claude / Copilot / Cursor integration inside this repository.
