Сделан governance-only ремонт лазеек: старый критерий "только полный live host UI" закреплён как необязательное bonus/live proof, а рабочий gate снова явно задан как Dual-Path Same-Scenario Evidence. `NOT READY`, blocker, failed live host proof, missing evidence и impossible UI surface теперь должны превращаться в следующий bounded sub-agent work order, а не в финальную остановку.

Я не запускал product code, host UI, tests, builds, runtime probes, training, checkpoint mutation, locator update, commit или push. Этот packet доказывает только scoped governance/plan/skill wording repair.

```text
claim_scope: governance stop-loophole repair for obsolete product readiness and premature stop rules
evidence_scope: direct edits plus textual readback in AGENTS.md, CLAUDE.md, active plan, four directly relevant skill files, and this evidence packet folder
count_claimed: 9 changed files total; 7 governance/plan/skill files plus 2 packet files; 7 required repair categories mapped
independent_evidence_count: 7 command/tool groups after edit (apply_patch success, rg new headings, rg required phrases, git diff --stat, JSON parse, packet folder readback, packet content readback)
subagent_id_or_NONE: bounded_subagent_executor_CURRENT
downgrade_needed: yes for any product readiness, runtime, UI, token savings, semantic quality, training, checkpoint, locator, commit, or push claim
```

## Files Changed

- `AGENTS.md`
- `CLAUDE.md`
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md`
- `docs/skills/claim-verification-SKILL.md`
- `docs/skills/output-readability-gate-SKILL.md`
- `docs/skills/plan-review-SKILL.md`
- `docs/skills/ui-testing-SKILL.md`
- `docs/plans/evidence/extension-full-readiness-dual-path/governance-stop-loophole-repair-20260628-180433/governance-stop-loophole-repair-20260628-180433.md`
- `docs/plans/evidence/extension-full-readiness-dual-path/governance-stop-loophole-repair-20260628-180433/governance-stop-loophole-repair-20260628-180433.json`

## Required Repairs 1-7

1. Replace obsolete Product Ready/live-host-only gate.
   - `AGENTS.md:506`: "Old product-readiness definition is closed: the chain `user in host UI -> extension -> core -> answer in host UI` is optional live/bonus proof only."
   - `CLAUDE.md:1252`: "product readiness must not be reduced back to the old single chain"
   - `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md:149`: "`user in host UI -> extension -> core -> answer in host UI`. That chain is a bonus/live proof when available."
   - `docs/skills/ui-testing-SKILL.md:125`: "the old single chain `user in host UI -> extension -> core -> answer in host UI` is optional bonus/live evidence only"

2. Add NOT READY/blocker/missing evidence as next work order, not final stop.
   - `AGENTS.md:498`: "any returned `NOT READY`, blocker, failed live host proof, missing evidence, impossible UI surface, stale state, or obsolete readiness wording is a work queue item"
   - `CLAUDE.md:1260`: "`NOT READY`, blockers, impossible UI surface, failed live host proof, missing evidence, stale packet, or old `product readiness count 0` wording is a bounded sub-agent work queue"
   - `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md:180`: "Stop rule: `NOT READY`, blockers, impossible UI surface, failed live host proof, missing evidence..."
   - `docs/skills/claim-verification-SKILL.md:104`: "reject any report that treats `NOT READY`, blocked Path B, failed live host proof, missing evidence... as a final stop"

3. Add human-readable Russian status rule before machine labels.
   - `AGENTS.md:515`: "Human-readable status hard gate: every Luba-facing report... must begin in plain Russian before any machine labels."
   - `CLAUDE.md:1267`: "Every user-visible status must be human-readable Russian before machine labels"
   - `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md:185`: "Human-readable reporting rule: every report must start in Russian with total phases/steps..."
   - `docs/skills/output-readability-gate-SKILL.md:392`: "every report that says `NOT READY`... must immediately translate that state into plain Russian continuation wording."

4. Add sub-agent-only enforcement.
   - `AGENTS.md:522`: "Sub-agent-only enforcement hard gate: under `Parent Architect Subagent-Only Firewall`, the parent may answer Luba and orchestrate bounded workers..."
   - `CLAUDE.md:1272`: "Under sub-agent-only authority, the parent answers and orchestrates but does not directly edit..."
   - `docs/skills/plan-review-SKILL.md:2836`: "Under sub-agent-only authority, the parent may answer and orchestrate but must not directly edit/read/verify governed artifacts..."

5. Add why/почему trigger: answer, patch through subagents, continue.
   - `AGENTS.md:529`: "Why-trigger continuation hard gate: \"why did you stop\", \"why\", \"почему\"..."
   - `CLAUDE.md:1274`: "A \"why did you stop\"/\"why\"/\"почему\" challenge is answer-first, repair-through-sub-agents, then continue..."
   - `docs/skills/output-readability-gate-SKILL.md:378`: "Why Question Root-Cause Repair Trigger readability rule..."

6. Add evidence wording to prevent overclaims.
   - `AGENTS.md:536`: "Evidence-scope hard gate: API/dashboard/runner evidence proves only the named boundary..."
   - `CLAUDE.md:1279`: "Evidence claim boundaries are literal: API/dashboard/runner evidence proves only that boundary..."
   - `docs/skills/claim-verification-SKILL.md:114`: "API/dashboard/runner evidence proves only the named boundary/API/runner behavior, not host readiness."
   - `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md:189`: "Evidence-scope rule: API/dashboard/runner evidence, screenshots, token traces, semantic eval, training, checkpoint promotion, and locator update..."

7. Make active plan phase sequence readable and target explicit.
   - `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md:141`: "Governance stop-loophole repair: readable phase sequence and target"
   - `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md:157`: "The full target remains open until packet-backed evidence proves all of these rows in plain language"
   - `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md:160`: "Cursor/Claude/Codex dual-path host proof"
   - `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md:162`: "Token comparison evidence"
   - `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md:163`: "Semantic quality eval"
   - `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md:164`: "Local learning lifecycle"
   - `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md:165`: "Checkpoint mutation/promotion/locator update"

## Textual Checks Run

- `rg -n "Stop-Loophole Repair Gate|Stop-loophole repair hardening|Governance stop-loophole repair|Stop-loophole evidence wording gate|Stop-loophole human-readable status repair|Governance stop-loophole review addendum|Stop-loophole UI gate" ...`
  - Result: found one new stop-loophole section in each of the 7 scoped files.
- `rg -n "user in host UI -> extension -> core -> answer in host UI|NOT READY is a bounded sub-agent work queue, not a stop|total phases/steps, done, in progress, remaining|parent may answer|why did you stop|API/dashboard/runner evidence|Token comparison evidence|Semantic quality eval|Checkpoint mutation/promotion/locator update" ...`
  - Result: found direct evidence for repairs 1-7 in scoped files.
- `git diff -- ... --stat`
  - Result: diff is limited to the 7 scoped governance/plan/skill files before packet creation.

## Scope Downgrades

- I did not verify product readiness.
- I did not run product code.
- I did not run tests/builds/runtime probes.
- I did not use nested Codex/AI agents.
- I did not commit or push.
- The evidence supports only: governance/plan/skill wording repair and textual readback.

## Next Bounded Work Order

Рабочая цепочка должна продолжаться не финальным статусом, а следующим bounded sub-agent work order:

```text
requested_artifact_type: semantic-quality row12/active-plan continuation packet
allowed_write_scope:
  - docs/plans/evidence/extension-full-readiness-dual-path/quality-semantic-row12-cursor-prep-20260628T145342Z/**
  - docs/plans/evidence/extension-full-readiness-dual-path/<new semantic-quality-row12 execution/review folder>/**
  - docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json only if the work order explicitly authorizes state update
exact_current_luba_quote: "Исправь все лазейки позволившие тебе остановиться через суб агентов и продолжать до полного выполнения - через суб агентов!!!!!!!"
goal_slice: resume the next active plan step after governance repair, likely semantic-quality row12 eval/continuation, without claiming product readiness
required_first_status_language: Russian human-readable phase counts before execution continuation
required_evidence:
  - identify current active row/phase from returned packets/state without treating NOT READY as stop
  - if row12 semantic quality is next, prepare or run only the authorized semantic-quality gate
  - preserve exact claim scope: semantic eval only, not token savings, product readiness, training, checkpoint promotion, or locator update
forbidden:
  - no parent direct execution under sub-agent-only
  - no product readiness claim
  - no host UI secret capture
  - no commit/push unless a separate commit boundary work order authorizes it
return_packet_must_include:
  - human Russian explanation first
  - exact files changed or NONE
  - commands run and outputs summarized
  - claim_scope/evidence_scope/count_claimed/independent_evidence_count/subagent_id_or_NONE/downgrade_needed
  - next bounded work order if anything remains NOT READY
```
