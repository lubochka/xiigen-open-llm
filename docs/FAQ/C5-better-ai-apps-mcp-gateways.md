# How does LlmFromScratch relate to MCP servers and AI gateways?

LlmFromScratch has a from-scratch MCP transport and a thin host-extension API boundary — the app-integration plumbing of the shift toward better AI apps, keeping core logic inside `llm_mvp_core`. These are real, implemented integration SEAMS, NOT a hosted AI-gateway product or an application marketplace — the repository ships the plumbing such apps connect through, not a running gateway.

LlmFromScratch has a from-scratch MCP transport and a thin host-extension API boundary — the app-integration plumbing of the shift toward better AI apps, keeping the core logic inside `llm_mvp_core`. `23.OssEquivalents/Mcp/McpTransport.cs:13` is `McpTransport`, a named-pipe MCP server that is the .NET equivalent of the `@modelcontextprotocol/sdk` transport; `43.SmartSkillsDomain/XiigenExtensionApiContracts.cs:9` (`record XiigenExtensionRequest`) is the extension API boundary a host calls. The honest boundary: the MCP transport and the extension API boundary are real, implemented pieces, but they are the integration SEAMS (an MCP server transport plus a request/response boundary), NOT a hosted AI-gateway product or a full application marketplace. The repository ships the plumbing that such apps and gateways would connect through, not a running gateway service. Note that `43.SmartSkillsDomain` also contains the `PromptToTaskMachine` and `OrchestrationReasoningMachine` SKELETONS — the extension boundary here is a request/response contract, not a trained skill engine.

### What is the industry trend?

Over the past months the focus shifted from building bigger LLMs toward building better AI applications — agent frameworks, MCP (Model Context Protocol) servers, AI gateways, and developer tooling.

### Where is this in the code?

`23.OssEquivalents/Mcp/McpTransport.cs:13` is `McpTransport`, a named-pipe MCP server that is the .NET equivalent of the `@modelcontextprotocol/sdk` transport; `43.SmartSkillsDomain/XiigenExtensionApiContracts.cs:9` (`record XiigenExtensionRequest`) is the extension API boundary a host calls.

These are transport / boundary seams, not orchestration-hierarchy levels: a client speaks to `McpTransport` over a named pipe, or a host sends an `XiigenExtensionRequest` across the extension API boundary, and the request is dispatched into the core; the core's own orchestration (the `OrchestratorFacade` pipeline) sits behind that seam. These seams do not touch the contract hierarchy levels.

### Honesty boundary

The MCP transport and the extension API boundary are real, implemented pieces, but they are the integration SEAMS (an MCP server transport plus a request/response boundary), NOT a hosted AI-gateway product or a full application marketplace. The repository ships the plumbing that such apps / gateways would connect through, not a running gateway service.

Last updated: 2026-07-24.
