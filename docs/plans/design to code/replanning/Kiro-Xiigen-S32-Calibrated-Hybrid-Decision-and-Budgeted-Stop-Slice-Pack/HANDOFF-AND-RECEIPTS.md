# S32 — compulsory bounded handoff, one recipient
Each task must return a **human readable** summary AND the following machine-readable fields. Do not create fake completed records to satisfy the template:
```yaml
task: S32.XX
candidate_ref: { planning_head: ACTUAL_SHA, core_head: ACTUAL_SHA }
execution: { status: NOT_RUN, actual_actor: NONE, model_calls: 0, code_effects: 0 }
scope: { human_outcome: "", one_seam: "", not_owned: "" }
source_ledger: [{ repo: "", ref: "", path: "", blob_sha: "", method: "", read_depth: READ_SCOPED, claim: "" }]
owned_observations: [{ evidence_id: "", real_consumer: "", before_state: "", after_state: "", qualified: false }]
law: { subject: "", version: "", intent: "", rights: "", result: UNKNOWN }
science: { arm: "", split: "", n: null, uncertainty: NOT_MEASURED, accuracy: null, selective_risk: null, coverage: null, p95_ms: null }
pc: { max_rss_mb: null, peak_vram_mb: null, cpu_s: null, whole_wall_s: null, branch_costs_included: false }
dpo: { actor_grants: NONE, pair_ref: NONE, judge_ref: NONE, state_update: NONE }
decision: { finding: OPEN, verdict: NOT_MEASURED, falsifier: "", wrong_alternative: "" }
changes: { paths: [], hashes: [], actual_runtime_effect: false }
next: { owner: "", single_task: "", authorization_required: "", blocked_by: [] }
```
Receipt classes: `SOURCE`, `CALL_GRAPH`, `PARENT_BASELINE`, `NO_GRADIENT`, `BLIND_LABELS`, `CALIBRATION_CURVE`, `STATE_DELTA`, `COLD_CONSUMER`, `WHOLE_PC`, `ASTRA_REVIEW`. **Missing receipt cannot be replaced with another class**, especially `CODE_COMPILES` or `WINDOWS_GREEN`.

Special return cases: `NOT_CALIBRATED` if no source-family-disjoint observation; `REUSE_NO_CHANGE` if equivalent existing producer→consumer bridge proven; `UNCOMPARABLE` if version/target mismatch; `NO_LEGAL_PATH` if all paths vetoed; `NOT_AUTHORIZED` if provider/training/effect rights missing; `INCONCLUSIVE` if improvement not distinguishable from noise; `INVALID_EVIDENCE` if leakage. Child/sub-agent output without actual invocation remains a proposed plan, not completed scientific review.

**Independent Astra reviewer packet:** exact Git candidate SHA, owner+rights, code before/after with FULL-call-graph provenance, law passports, frozen panel, P/C0/E/Tcal/Tstudent (only executed arms), actual PC receipt, source-family splits, negative and counterfactual examples, security/rollback and unsolved claims. Reviewer must find a counterexample; `NOT_REVIEWABLE` is legitimate. A subsequent planning commit changes SHA identity; do not inherit an earlier PASS.
