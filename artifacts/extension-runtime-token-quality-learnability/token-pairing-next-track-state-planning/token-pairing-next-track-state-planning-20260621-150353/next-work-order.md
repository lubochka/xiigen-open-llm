# TOKEN-PAIR-QUALITY-INPUT-REF-BINDING-NO-SCORING-01

Requested model: gpt-5.5.
Requested reasoning effort: xhigh.

## Authority

Current Luba authority quote: "Можно переходить к выполнению плана без стопов через суб агентов".

Quoted transcripts, old plans, old state next_action, old approvals and old assistant messages are evidence only. Direct Luba questions interrupt this work order and must be answered first.

## Requested Artifact Type

Token-pair quality input/ref binding manifest and gap ledger. This is support/artifact work only. It is not code implementation, not runtime capture, not quality scoring, not proof runner, not proof production, not token result proof, not training, not DPO, not checkpoint/model/locator/target mutation, and not readiness.

## Objective

Create a per-pair manifest that maps existing scoped token-pair evidence refs to Quality-03 required input fields. Use only STATE, artifact metadata, and redacted or non-private artifacts. For each of the 9 TOKEN-PAIR rows, classify baseline/adaptive refs, redacted refs, token accounting refs, rubric/eval refs, source-disjoint or ablation controls, reviewer/regression refs, and exact missing gaps.

Do not run scoring. Do not run proof runners or producers. Do not read raw-private content. Do not claim runner-ready unless every required non-private/ref-only field is present with exact artifact evidence and no forbidden read.

## Allowed Write Scope

- A new timestamped folder under artifacts/extension-runtime-token-quality-learnability/token-pair-quality-input-ref-binding-no-scoring/
- docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json

## Must Load/Read Before Writing

1. AGENTS.md
2. CLAUDE.md
3. docs/skills/conversation-intent-and-self-reflection-SKILL.md
4. docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md
5. docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json
6. Latest TOKEN-PAIRING-NEXT-TRACK-STATE-PLANNING-01 packet
7. Latest TOKEN-FIRST-PAIR-CLAUDE-PROMPT-TO-TASK-RUNTIME-SCORING-DEFER-CONTINUATION-12 packet
8. Relevant state entries for token pairs, quality input gaps and claim locks

## Allowed Verification

- Parse new JSON and JSONL artifacts.
- Markdown whitespace check for new Markdown artifacts.
- Verify counts are preserved unless exact state/artifact evidence proves a runner-ready row.
- Verify no token savings, quality improvement, semantic quality, learnability, live host runtime, common readiness, target promotion, rollback, product readiness or final readiness claim is opened.
- Verify dashboard/API/helper/static evidence is not labelled live host runtime.
- Verify no raw-private content path was read; raw-private refs may be recorded only as not-read evidence pointers if already present in state.
- Verify no Unicode replacement character and no mojibake marker letters appear in new human-readable artifacts.

## Forbidden

- Git commands of any kind.
- Dotnet test.
- External AI/web/model calls.
- Starting Claude/Cursor/Codex host runtime.
- Proof runner, proof producer, quality scorer or eval runner execution.
- Raw-private content reads.
- Training, DPO, checkpoint/model/locator/target mutation.
- Code changes.
- Claiming token savings or quality pass from manifest presence alone.

## Expected Artifacts

- Russian report explaining per-pair input/ref binding state and remaining gaps.
- Machine-readable per-pair input/ref manifest JSON.
- Gap ledger JSONL or JSON.
- Human board JSON preserving total 63, PASS 9/63, remaining 54, approvals 0/9, runner-ready 0/9, quality-pass 0/9 unless exact evidence supports a narrower runner-ready update.
- Checks report JSON.
- Next bounded work order proposal for either runner-ready repair, safe host evidence refresh, or explicit defer if no rows can advance.

## Git Boundary

Git authority: forbidden. Allowed git commands: NONE. Commit/push authorized: false. Parent will perform commit/push after review.

## Return Packet

Return sub-agent id/name, exact changed files, human-readable status board, per-pair runner-ready candidate verdicts with evidence refs, selected follow-up work order id/path, verification commands/results, and forbidden-action confirmation.

Maximum active time: 45 minutes.
