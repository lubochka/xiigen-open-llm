# P20 evidence card

## Scope

P20 is a blocker repair gate for SkillSelection promotion. It does not promote P17E candidate, does not mutate checkpoint files, and does not change runtime behavior.

## Main result

Direct promotion remains blocked. P20 turns the P19 blockers into an executable evidence plan:

- compatibility/non-regression;
- rollback/load proof;
- fresh-load through `XiigenExtensionApiService`;
- truthful manifest and locator rewrite;
- API/firewall rerun;
- no-overclaim;
- state/doc sync.

## Evidence files

- Human report: `p20-promotion-blocker-repair-plan.md`
- Machine plan: `p20-promotion-blocker-repair-plan.json`
- Gate checklist: `p20-gate-checklist.jsonl`
- Review cycles: `p20-review-cycles.jsonl`
- Self-check: `p20-self-check.json`
- Focused test: `Tests/SmartSkillsSemanticQualityP20PromotionBlockerRepairTests.cs`

## What this proves

P20 proves that the next promotion work cannot hide behind artifact existence or direct copy. Every blocker now has a required proof path and a forbidden shortcut.

## What this does not prove

P20 does not prove semantic quality, SkillSelection readiness, final model readiness, full API readiness, dashboard readiness, host runtime readiness, or trained-model readiness.
