# Plan Review Cycle 06 - Quality Improvement And Semantic Quality Claims

## Человеческий пакет

Стадия: цикл 6 из 10 перед любым продолжением runtime/code/training/host.

Фокус: quality improvement / semantic quality claims.

Reviewer/sub-agent id:
`sub-agent-cycle-06-current-session-quality-semantic-claims`.

Model/reasoning evidence: `requested_model=gpt-5.5`,
`requested_effort=xhigh`, `downgrade_warning_or_NONE=NONE`.

Ограничения выполнены: агентов не спавнил, ветку не менял, commit/push не
делал, runtime/source code не редактировал, runtime/training/host probes не
запускал.

## Что проверено

- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md`
  Section 0.1, Sections 4.2.2 and 4.3, Section 6 QUALITY rows, Section 7
  required artifacts, Section 8 pass 6 and visible review records, Section 9
  final acceptance and final readiness pass 6.
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`
  active plan-review cycle gate and top-level phase fields.
- Historical `plan-review-cycle-06.*` content was treated as stale evidence
  only because the current Luba instruction says the previous cycle 6 agent
  failed infrastructurally and does not count.

## Findings

1. PASS_WITH_PLAN_FIX: the plan had a human-readable quality rubric, but it
   needed a concrete machines/layers -> inputs -> scoring criteria -> fail
   labels -> artifacts matrix.
2. PASS_WITH_PAIRING_GATE: baseline/adaptive quality outputs now must be scored
   only on the same paired scenarios after Section 4.2.2 pairing integrity
   passes.
3. PASS_WITH_SOURCE_DISJOINT_HELD_OUT: quality eval now requires source-disjoint
   held-out prompts with no shared `source_ref`, path, title, prompt hash,
   correction text, near-duplicate text, or nearest-neighbour source with
   training/DPO rows.
4. PASS_WITH_REAL_REFERENCE_EXAMPLES: positive and negative examples must come
   from real reviewed references where available; synthetic-only examples cannot
   close a quality PASS.
5. PASS_WITH_REVIEWER_NOTES_AND_REGRESSION_GUARD: every scored row needs a
   human reviewer note, adjudication path where needed, and a regression guard.
6. PASS_WITH_ABLATION_NO_LEAKAGE_NO_MEMORIZATION: semantic quality claims now
   require common/no-trained-state baseline, exact prompt/path/title/source-id
   masking, nearest-neighbour checks, candidate-order shuffle and chosen/rejected
   shuffle where applicable.
7. PASS_WITH_NO_OVERCLAIM: full semantic quality, common readiness, target
   promotion and locator update remain closed if PromptToTask,
   OrchestrationReasoning or SkillSelection only show narrow staged, baseline,
   deterministic, test-only, one-host or one-machine improvements.

## Repair action

- Updated Section 0.1 board to current `Цикл 6/10` and next `Цикл 7/10`.
- Added Section 4.3.1 quality improvement / semantic quality claim matrix for
  `PromptToTaskMachine`, `OrchestrationReasoningMachine`,
  `SkillSelectionMachine` and final usefulness.
- Added fail labels:
  `FAIL_SOURCE_DISJOINT_SPLIT`, `FAIL_HELD_OUT_MISSING`,
  `FAIL_NO_MEMORIZATION_CONTROL`, `FAIL_ABLATION_EXPLAINS_GAIN`,
  `FAIL_DETERMINISTIC_BASELINE_ONLY`, `FAIL_NARROW_STAGED_ONLY`, and
  `SEMANTIC_CLAIM_BLOCKED_NARROW_LAYER_EVIDENCE`.
- Updated QUALITY-01 through QUALITY-03, required artifacts, pass 6, final
  acceptance and final readiness pass 6.
- Updated state to `PLAN-REVIEW-CYCLE-06_OF_10`, count 6/10, and next action
  cycle 7/10 while keeping execution locked.

## Recheck evidence

Post-patch verification status: PASS.

Проверки:

- PASS: execution state JSON parse.
- PASS: cycle 06 JSON parse.
- PASS: `git diff --check`.
- PASS: targeted `rg` for `Цикл 6/10`, `quality improvement`,
  `semantic quality`, `source-disjoint`, `ablation`, `no-memorization`,
  `PromptToTask`, `OrchestrationReasoning`, and `SkillSelection`.

No `dotnet test` was run because this phase authorizes only plan/state/artifact
review verification, not runtime/source code verification.

## Verdict

PASS_WITH_REPAIR_ACCEPTED for cycle 6.

cycle_counts_toward_10: true.

cycles_completed_after_this_packet: 6.

remaining_cycles_before_execution: 4.

product_readiness_proven_now: false.

Runtime/code/training/host execution, production locator update, checkpoint
mutation, commit and push remain forbidden in this sub-agent slice.
