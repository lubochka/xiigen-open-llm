# S32 — causal experiments: is a calibrated hybrid decision useful?
Run only experiment arms that truly exist and are authorized; freeze base K commit, state, law/passports, data rights, evaluator, user workload distribution and PC envelope BEFORE reading outcomes.

| Arm | Change from same parent | Valid claim |
|---|---|---|
| P | current actual cold ordinary consumer / unmodified scoring | lawful quality, raw risk, answered fraction, full cost |
| C0 | deterministic lawful fixes, exact same learned state | ENGINEERING_GAIN only |
| E | S32 proposed/comparable evidence envelope, **no gradient update** | effect of observability / deterministic normalization only |
| R | optional bounded additional eligible evidence branch vs abstain; fixed policy | engineering value-of-information, not learning |
| Tcal | RP11 owner actually updates existing bounded calibration/credit state on TRAIN distinct bulk | conditional learned decision/calibration gain if state+next-cold use observed |
| Tstudent | RP01 actual separately authorized genuine DPO/LoRA on distinct Student weakness | Student update only; not proof S32 calibration learned |
| T2 | new source-family legal bulk, changed state again with durable parent/restore | continual improvement/retention if actually applied |

Use paired inputs/same seeds where possible; preserve precise actual variable changed. If arms differ in retrieval corpus, law, source family, parent weights, cold memory or whole-PC envelope, the comparison is confounded. Separate `P→C0`, `C0→E`, `E→Tcal`, `E→Tstudent`, `T1→T2`. `E` and `R` need not wait for Chinese Teacher. Never assert it improves merely because an adapter exists.

## Independent primary metrics
1. **Decision reliability:** wrong-answer rate among *answered*, answered coverage, domain-wise selective risk-coverage (AURC), error at fixed coverage; calibration ECE/Brier where meaningful probability and sample support exist. Both wrong-answer rate and fraction answered are mandatory; always-abstain is not a win.
2. **Actual lawful utility:** task score, hard-law violations (vetoed vs exposed), best *feasible enumerated* path@budget, regret vs that set, human-correction need, correct type and source authority.
3. **Mechanism interactions:** domain+intent confusion, law-feature retention, token/IR coordinate/version errors, actual distinct kNN hits **consumed**, dynamic k policy, Unit many→many links, Flow correction, async cancellations/Graph pattern use, final D-15 outcome.
4. **Learning:** exact source-family disjoint parent vs Tcal/Tstudent on blind data, macro/worst-domain quality, uncertainty/CI, negative transfer, feedback T2 novelty, no-repeat, state readback and actual next cold inference.
5. **Whole PC:** wall p50/p95/total, CPU time, peak RSS/VRAM, index/disk/checkpoints, cancelled/failed branch cost, extra branch latency, local power if measured, external tokens/financial cost, checkpoint+rollback reserve and per-domain quality-per-whole-PC-budget.

## Adversarial falsifiers / verdict
F01 artificially high reliability but wrong law; F02 calibrated ECE improves only because answered fraction collapsed; F03 extra branch breaks budget; F04 confidence is borrowed across domain+model/version; F05 gradient did not change actual state; F06 no cold ordinary consumer; F07 eval leakage or altered rubric; F08 error caused by security permission not statistics. Any F01/F03 is hard stop for affected promotion, with fail-closed path.

Decision: `IMPROVE` only if independent quality/selective risk AND coverage/resource criteria are met with no law violation and credible uncertainty; `REGRESS`/ `INCONCLUSIVE` keep parent; `INVALID_EVIDENCE` if leak/confounded; `REUSE_NO_CHANGE` for existing equivalent path; `NOT_MEASURED` if not run. Long-term owner 95% oracle target remains goal, not empirical evidence.

Narrow deterministic checks (type safety, law veto, authorization, unique update, checkpoint/restart) remain appropriate. Do NOT substitute unit test coverage, Windows GitHub Actions, changed confidence output, DPO loss alone or GitHub commit for these measurements.
