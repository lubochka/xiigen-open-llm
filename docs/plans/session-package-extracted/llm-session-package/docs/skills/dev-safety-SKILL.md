# dev-safety-SKILL.md — Build gate and git discipline

## When to load
Load at the start of every EXECUTOR session.

## Build gate — runs before every commit

    dotnet build LlmFromScratch.sln       # zero errors
    dotnet test Tests/LlmFromScratch.Tests.csproj  # zero failures
    git diff --name-only                   # list what changed

All three must pass. If any fails: fix before committing.

## Git discipline

- One commit per phase
- Commit message: "phase N: what changed — files changed"
- Never `git commit --no-verify`
- Never commit with failing tests

## Zero failures rule

"Zero failures" means zero. Not "zero new failures."
Pre-existing failures must be fixed in the current commit, not deferred.
