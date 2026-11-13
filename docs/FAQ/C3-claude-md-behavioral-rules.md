# C3 — CLAUDE.md as behavioral rules

**Trend.** Using a single behavioral-rules file (`CLAUDE.md`) to steer an AI coding agent went viral in 2025-2026 as a dependency-free way to encode good agent behavior.

**How this project relates.** LlmFromScratch leans heavily on exactly this pattern: it ships an extensive governance system of behavioral rules — `CLAUDE.md` and `AGENTS.md` at the repository root, plus a large `docs/skills/` library (193 files) of skill / rule files that steer how the project's agents plan, review, and self-check.

**Where in the code.** `CLAUDE.md` (root), `AGENTS.md` (root), and `docs/skills/*.md` (193 files). These are behavioral-rule documents, not runtime code, so this answer cites them by path rather than by `file:line`.

**Orchestration.** None — these are governance / behavioral-rule text files read by agents, not a runtime orchestration path; these files do not touch the contract hierarchy levels.

**Honesty boundary.** Before release, incidental personal paths in the governance files (a handful of occurrences in the root rule files and in `docs/skills/`) are scrubbed as part of the release checklist; whether the internal governance system itself is published is a separate maintainer decision. The pattern is genuinely used here — the governance files are real and central to how the project works.
