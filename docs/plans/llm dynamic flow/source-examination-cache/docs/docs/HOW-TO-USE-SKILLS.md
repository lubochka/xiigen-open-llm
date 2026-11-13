# HOW-TO-USE-SKILLS.md — When to load which skill

## Load order: always start here

    Step 1: cat CLAUDE.md + AGENTS.md      (every session)
    Step 2: dotnet test — record baseline   (every session)
    Step 3: load session-type skill         (see table below)

## Session type → skills

| Session type | Load before first output |
|---|---|
| EXECUTOR | dev-safety-SKILL.md + code-execution-SKILL.md |
| ARCHITECT | plan-review-SKILL.md + no-product-decisions-SKILL.md |
| EXPERIMENT | experiment-design-SKILL.md + code-execution-SKILL.md |
| REVIEW | plan-review-SKILL.md |

## Activation triggers

| When | Load |
|---|---|
| Any test fails unexpectedly | mental-debug-SKILL.md |
| Plan has no Gate B | plan-review-SKILL.md |
| Choosing between two approaches | no-product-decisions-SKILL.md |
| New experiment E-N designed | experiment-design-SKILL.md |
| Commit about to happen | dev-safety-SKILL.md |
