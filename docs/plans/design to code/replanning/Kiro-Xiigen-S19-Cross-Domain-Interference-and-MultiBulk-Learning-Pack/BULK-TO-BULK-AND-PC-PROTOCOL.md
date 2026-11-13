# S19 — causal learning experiment: P/C0/T1/T2 and complete PC ledger
## Aim
Answer whether one **real new domain A lesson**, and optionally a **different domain B lesson**, improves actual system behavior across both lawful domains with acceptable negative transfer and local total cost. Measurement is not code coverage. Freeze panel/rubric/arms/cost budget/stop criteria and evaluator identity **before** optimizer updates.

## Frozen arms; correct attribution
P = accepted parent (or explicitly admitted first baseline) on fixed existing pipeline/state.
C0 = same parent on a deterministic compatibility/selection repair with **no new Student gradient**; compare P→C0 for engineering rule/route gain.
T1 = **identical repaired pipeline** + exactly one new authorized immutable, admitted Chinese-both-sides DPO pair for subject A; compare C0→T1 for Student update effect.
T2 = same pipeline + **another unique** authorized pair for subject B applied to T1 with a separate exact checkpoint; T1→T2 measures second bulk and possible forgetting of A. **If not actually run, T1/T2 = NOT_RUN, never copy baseline values.**
To isolate interference, matched one-factor probes with **fixed lawful candidate set and budget**: intent-only vs domain-aware selector; fixed/adaptive k with unchanged scorer; compatible vs mismatched vector/adapter eligibility; with/without allowed Unit/Flow or Graph preference, bounded to the existing owner. Oracle domain labels may be **offline diagnostic only**, not served.

## Evaluation scorecard — source-family macro and worst-domain
- domain_subject_accuracy and domain_confusion/UNKNOWN; taskIntent accuracy scored on separate D-04 axis.
- lawful_answer_rate, law_violation_rate, utility rubric per domain/edition; confidence intervals/uncertainty grouped by source family and N.
- wrong_domain_used_neighbor_rate, chosen_project_adapter_eligibility_rate, kNN neighbor contribution to actual typed output, correct InputRef→OutputRef many→many association.
- best_permissible_observed_path_hit_rate among **actually enumerated permissible candidates**, path outcome utility, all attempted branch count, cancellation and latency.
- negative_transfer_domain_B = utility_B(T1) − utility_B(C0) with uncertainty, plus retention_A(T2) − retention_A(T1). Record positive, zero, negative honestly.
- DPO: exact pair usable/preferred, one genuine optimizer application, correct response-target state owner, response-only mask/reference, state bytes/checkpoint journal, unchanged shared law.
- PC: wall time overall and per stage; CPU user time, RSS and peak memory, GPU VRAM reserved/used, energy if actually metered, disk/read/write/cache/reindex/trainer/teacher+judge/inference cost, **all graph branches**, crash recovery overhead; missing metric = NOT_MEASURED (not zero). Same machine and comparable load or confounder.

## Sequential new bulks vs repeated evaluation
Bulk 0 frozen panel P and C0. Bulk 1 distinct eligible pair A and T1. Bulk 2 distinct eligible pair B and T2, only if grants and measured hypothesis justify; optional further bulks are future work orders, not promised executions. Never repeat a gradient on the same pair_id as if it were a new bulk. Repeated inference with fixed accepted checkpoint estimates stochastic variability; do not accidentally update during evaluator repeat.

## Pre-registered promote/revert logic
Before data, domain-owner sets meaningful-effect margins, lawful hard-stop, worst-domain regression tolerance, uncertainty method, PC envelope and acceptable cold startup. Do not invent 95% or p-values. PROMOTE requires hard law and permissions PASS, measured sufficient utility on independent families and cost within envelope, complete reversible state + exact independent approval when required, ordinary cold consumer selected accepted state. Else REVERT or INCONCLUSIVE, preserve negative signal with evidence. If no authorized update, use measured C0 and leave learned claim OPEN.

## Crash and feedback
Journal unique pair, before/after state IDs, optimizer/LoRA/reference/index/Graph and RNG snapshots per actual owner; crash UNKNOWN blocks a second step until reconciled. New user feedback is an observation, not automatically a law update nor a training example; dedupe, provenance, consent, quarantine, CODEX selection + separate pair author/judge path. Canonical training data/weights stay in K's authorized paths/branch when actual implementation is granted.
