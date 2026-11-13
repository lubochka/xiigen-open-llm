# LlmFromScratch — Session Start Prompt v1.0

Paste this at the beginning of every session.

---

## Step 1 — Read project rules

    cat CLAUDE.md      # 10 coding rules
    cat AGENTS.md      # 6 operational rules + Round Contract

## Step 2 — Read current state

    cat build-state.json              # last known test count
    dotnet test --no-build 2>&1 | tail -5   # actual current state

Record: total passing, total failing.
If failing > 0: stop and report before touching anything.

## Step 3 — Read the relevant skill

Load the skill for this session type:
- EXECUTOR:   docs/skills/dev-safety-SKILL.md + code-execution-SKILL.md
- ARCHITECT:  docs/skills/plan-review-SKILL.md
- EXPERIMENT: docs/skills/experiment-design-SKILL.md
- REVIEW:     docs/skills/plan-review-SKILL.md

## Step 4 — State where you are

Write one paragraph:
- session type (EXECUTOR / ARCHITECT / EXPERIMENT / REVIEW)
- branch name
- test baseline (passed / failed right now)
- what you are starting

Then proceed.

---

## Quick reference

    Subproject map:    docs/SUBPROJECT-MAP.md
    Experiment map:    docs/EXPERIMENT-RESULTS-MAP.md
    Locked decisions:  docs/DECISIONS-LOCKED.md
    Plans directory:   docs/plans/
