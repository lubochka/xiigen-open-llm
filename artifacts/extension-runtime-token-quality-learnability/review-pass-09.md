# Review/Fix Pass 09 - Sub-agent execution packet and parent review loop

Verdict after fix: `PASS_WITH_FIX`.

This is a real review/fix pass. The review ledger row is an index only and does not count as evidence.

## Before finding

Section 6 said phases are bounded and run through one sub-agent at a time, with parent review. That was directionally correct but too thin.

The plan did not force:

- an exact work-order packet for every sub-agent;
- explicit skill/guide loading per phase;
- clear allowed and forbidden write scope;
- explicit runtime/training/eval permission per track;
- packet-backed evidence from the sub-agent;
- separate parent review packet;
- repair loop before the next phase;
- commit/push boundary after accepted file changes.

Without those details, a sub-agent summary could be mistaken for completed execution.

## Concrete fix

Patched `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md`:

- Added Section 6.1 with required sub-agent work-order packet.
- Added parent review packet requirements.
- Added repair-loop and commit/push boundary requirements.
- Updated Section 8 row 9 to point to this packet.

## Exact changed sections

- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md` Section 6.1
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md` Section 8 row 9
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1-REVIEW.jsonl` row 9

## Recheck evidence

The plan now requires every sub-agent work order to name:

- phase id;
- artifact type;
- allowed/forbidden write scope;
- maximum active time;
- whether implementation is authorized;
- exact Luba approval quote or `NONE`;
- required skills/guides/project rules to load;
- whether runtime/training/eval is allowed;
- whether external AI/paid tools are allowed;
- evidence packet requirements;
- transcript quarantine and direct-question interrupt behavior.

The parent must now review skills, scope, gate evidence, overclaim risk, code review need, architecture review need, documentation need, repair need, and commit/push boundary before allowing the next phase.

## Scope

No product code, runtime, training, build, tests, git, commit, or push were run in this pass.

`overall_complete_claimed=false`.
