# Phase 12 required fixes before pass

Packet: PHASE_12_BLOCKED_BY_PRODUCT_QUALITY_FIXES
Blocking adaptive rows: 9

| Fix id | Target | Required change | Acceptance evidence |
|---|---|---|---|
| ORCH-12-01 | orchestration_reasoning model/checkpoint or adapter-to-core inference path | Produce orchestration_reasoning_status=orchestration_plan_produced for the Phase 12 held-out universal prompts, with review gates, rollback/evidence/stop-condition content, and no project-specific authority leakage. | Fresh Phase 12 run shows universal_orchestration_plan_quality passed for all 9 adaptive scenario/host pairs, no adaptive rubric failures, no quality regression, and token reduction still observed. |
| ORCH-12-02 | held-out orchestration evidence | Persist enough structured orchestration evidence for the rubric to distinguish a real universal plan from loaded-guide-state/no-matching-pattern fallback. | quality-scoring-report.json raw_scores contains score=2/passed=true for universal_orchestration_plan_quality in every adaptive pair. |

Forbidden shortcuts:
- Do not relabel loaded_guide_state_no_matching_learned_pattern as PASS.
- Do not claim semantic quality improvement, learnability, training, checkpoint promotion, full chat UI readiness, or final readiness without direct evidence.
- Do not treat lower heuristic token count as accepted while adaptive quality criteria fail.
