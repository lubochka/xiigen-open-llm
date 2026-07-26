# FAQ — LlmFromScratch and current AI trends

This FAQ maps 15 current (2025-2026) AI trends onto what this repository actually
does. Each page now leads with a direct answer to its question, then a
self-contained summary block, then the industry trend, then where in the code it
lives (`file:line` references), and finally an honest boundary that separates real
trained/implemented mechanisms from scaffolding, stubs, and deterministic gates.
Every code reference is verified against the current tree.

An extra note on the licensing choice (`AGPL-3.0-only`) is included on top of the
15 topics.

## Group A — Model / training capabilities

| # | Question | File |
|---|----------|------|
| A1 | Do the agents in LlmFromScratch check their own output? | [A1-self-checking-agents.md](A1-self-checking-agents.md) |
| A2 | How does LlmFromScratch handle continual learning and catastrophic forgetting? | [A2-hybrid-architectures-continual-learning.md](A2-hybrid-architectures-continual-learning.md) |
| A3 | Can LlmFromScratch invent new algorithms (AlphaEvolve-style)? | [A3-ai-inventing-algorithms.md](A3-ai-inventing-algorithms.md) |
| A4 | Which new training methods does LlmFromScratch implement? | [A4-new-training-methods.md](A4-new-training-methods.md) |
| A5 | How does LlmFromScratch compare to narrow-domain systems like AlphaGeometry? | [A5-progress-in-narrow-domains.md](A5-progress-in-narrow-domains.md) |

## Group B — Developer workflow / agentic tooling

| # | Question | File |
|---|----------|------|
| B1 | Is LlmFromScratch a multi-agent system? | [B1-agentic-ai-multi-agent.md](B1-agentic-ai-multi-agent.md) |
| B2 | How does LlmFromScratch reflect the shift from coding to orchestration? | [B2-developer-role-shift.md](B2-developer-role-shift.md) |
| B3 | How was LlmFromScratch built with agentic coding tools? | [B3-tooling-race.md](B3-tooling-race.md) |
| B4 | What does large-scale AI adoption mean for LlmFromScratch? | [B4-adoption-scale.md](B4-adoption-scale.md) |
| B5 | Does LlmFromScratch support vibe coding and natural-language intent? | [B5-vibe-coding.md](B5-vibe-coding.md) |

## Group C — Local-first / ecosystem

| # | Question | File |
|---|----------|------|
| C1 | Does LlmFromScratch run fully on-device, without the cloud? | [C1-on-device-no-cloud.md](C1-on-device-no-cloud.md) |
| C2 | Is LlmFromScratch a local LLM like llama.cpp or Ollama? | [C2-local-llms.md](C2-local-llms.md) |
| C3 | How does LlmFromScratch use CLAUDE.md-style behavioral rules? | [C3-claude-md-behavioral-rules.md](C3-claude-md-behavioral-rules.md) |
| C4 | Is LlmFromScratch an agentic framework like CrewAI or AutoGen? | [C4-agentic-frameworks.md](C4-agentic-frameworks.md) |
| C5 | How does LlmFromScratch relate to MCP servers and AI gateways? | [C5-better-ai-apps-mcp-gateways.md](C5-better-ai-apps-mcp-gateways.md) |

## Licensing note (on top of the 15 topics)

| Note | File |
|------|------|
| Why is LlmFromScratch licensed under AGPL-3.0? | [why-agpl.md](why-agpl.md) |

Last updated: 2026-07-24.
