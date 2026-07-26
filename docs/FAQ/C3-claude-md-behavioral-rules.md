# How does LlmFromScratch use CLAUDE.md-style behavioral rules?

LlmFromScratch leans heavily on the CLAUDE.md pattern: it ships `CLAUDE.md` and `AGENTS.md` at the repository root plus a large `docs/skills/` library that steers how the project's agents plan, review, and self-check. These are governance and behavioral-rule text documents, NOT runtime code — the pattern is genuinely used here, but it is documentation, not an executed mechanism.

LlmFromScratch leans heavily on exactly this pattern: it ships an extensive governance system of behavioral rules — `CLAUDE.md` and `AGENTS.md` at the repository root, plus a large `docs/skills/` library (193 files) of skill / rule files that steer how the project's agents plan, review, and self-check. These are behavioral-rule documents, not runtime code, so they are cited by path rather than by `file:line`. The honest boundary: the pattern is genuinely used here — the governance files are real and central to how the project works — but they steer agents through text, they are not an executed orchestration path and do not touch the contract hierarchy levels. Before release, incidental personal paths in the governance files (a handful of occurrences in the root rule files and in `docs/skills/`) are scrubbed as part of the release checklist; whether the internal governance system itself is published is a separate maintainer decision.

### What is the industry trend?

Using a single behavioral-rules file (`CLAUDE.md`) to steer an AI coding agent went viral in 2025-2026 as a dependency-free way to encode good agent behavior.

### Where is this in the code?

`CLAUDE.md` (root), `AGENTS.md` (root), and `docs/skills/*.md` (193 files). These are behavioral-rule documents, not runtime code, so this answer cites them by path rather than by `file:line`.

Orchestration: none — these are governance / behavioral-rule text files read by agents, not a runtime orchestration path; these files do not touch the contract hierarchy levels.

### Honesty boundary

Before release, incidental personal paths in the governance files (a handful of occurrences in the root rule files and in `docs/skills/`) are scrubbed as part of the release checklist; whether the internal governance system itself is published is a separate maintainer decision. The pattern is genuinely used here — the governance files are real and central to how the project works.

Last updated: 2026-07-24.
