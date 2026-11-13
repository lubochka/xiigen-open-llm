# R14 RUN LOG

## Current instruction

Proceed to R14 and improve the replanning so that it is whole-program, learning-first, based on deterministic+learned hybrid algorithms, correct DPO construction/evaluation, domain-sensitive token/vector/kNN/LoRA behavior, fast transition to real teaching, bulk-to-bulk quality/time measurement and preserved DTC/IDE/native obligations.

GitHub publication is authorized.

## Governance repair performed first

The prior R14 card was too short to satisfy the requested model-task quality and could allow the same failure mode as the first planning cycle: narrow to one slice, consume sibling summaries instead of primary sources, and omit explicit algorithm connections.

Published governing repair:
- `R14-REPAIR-WORK-ORDER.md`
- commit `e065e88b2069f6330dc759bdba97c7ce35af5f71`

The repaired work order contains:
- explicit role/skill stack;
- DPO role lock;
- Unit/Flow/Orchestrator/DynamicGraph contracts;
- R14-A..F decomposition;
- context/source requirements;
- good/bad/edge cases;
- capability milestones;
- deterministic+learned matrix;
- learning-result branching;
- domain/vector/kNN/LoRA requirements;
- R18 packet schema;
- documentary acceptance rules.

## Evidence consumed

Direct current reads included:
- `previous rounds thinking results.md`
- `PARALLEL-R01-R03-JOIN.md`
- `COMMON-CONTRACT.md`
- `README.md`
- current R14 cards in `HYBRID-ALGORITHMS-AND-RUNS.md` and `MODEL-RUN-PACKETS-CP5-11-25.md`
- current R01/R02 and R04–R09 decision/change carriers where available
- R11 findings/decisions
- R12 current repair findings/decisions/change-map/run-log/sources
- core planning/plan-review/authority skills directly fetched from `codex/dpo-implementation`

## Important upstream status

- R12 = REEXECUTION_REQUIRED.
- `results/R13` is now present and consumed; its protocol is COMPLETE at architecture-neutral level.
- Therefore exact first implementation seam and exact first experimental recipe remain provisional.
- R14 continued on all independent whole-program questions; no sibling wait cycle was introduced.

## What R14 changed conceptually

1. Replaced slice-number priority with capability priority.
2. Made M0–M7 explicit.
3. Kept all delivery lines.
4. Separated deterministic correctness evidence from learning efficacy.
5. Kept domain mapping/vectorization/dynamic kNN/LoRA as explicit early algorithmic capabilities.
6. Preserved four learning roles as separate causal mechanisms.
7. Added learning-result branches so REGRESS/INCONCLUSIVE does not restart the entire program.
8. Prepared the shape of the smallest R18 packet without inventing missing R12/R13 fields.

## What was not performed

No model training, Chinese-model generation, CODEX/Astra pair assessment, Windows/GitHub Actions, tests, CI, code changes, dataset/weight mutation or independent R15 review was performed.

All learning improvement claims remain NOT_MEASURED.

## Current R14 status

**R14 capability reordering = PUBLISHED.**  
**Exact first implementation packet = PROVISIONAL_PENDING_R12_R13.**

Next consumers:
- R15: challenge the capability order and exact causal assumptions once R12/R13 candidate exists.
- R18: fill exact file/class/method work packet from final R12/R13.
- R19: apply old-ID → capability migration to live roadmap/start queues.
- R21/R22: domain curriculum and algorithm experiments.
- R23: PC/full-cost continual-learning envelope.
- R24: user/client/DTC/IDE delivery impact.

## Repair V2 — current-turn correction

A later review found two R14 defects and repaired them:
1. the published R14 interpreted M0–M7 too linearly and effectively biased execution toward DPO/E1 first;
2. R14's upstream-status section was stale because `results/R13` is now present.

Current readback consumed R13 FINDINGS/DECISIONS/CHANGE-MAP/RUN-LOG plus current R04–R06 algorithm decisions/findings. R13's E1–E4 protocol now governs candidate selection. The first executable observation is ranked by expected information gain / implementation cost and causal validity; E2/E3/E4 can precede E1 when they remove the dominant confounder or architecture uncertainty.

This repair does not claim a learning gain and does not run training/tests/Windows/Actions. It repairs planning artifacts only.

