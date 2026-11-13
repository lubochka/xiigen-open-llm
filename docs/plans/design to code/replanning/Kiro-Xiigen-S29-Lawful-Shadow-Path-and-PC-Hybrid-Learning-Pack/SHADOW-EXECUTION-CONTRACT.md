# S29 — side-effect-safe ordinary shadow execution protocol
**Hypothesis, not claim of an existing implementation.** S29 owns a missing observation/consumer seam if one is discovered. Current interface and deployment must be verified by 00B before designing code.

## Admission
shadow_eligible = current ordinary request + verified subject/intent + law version + same approved parent/model/index/project/tenant state + typed route compatibility + deterministic D-15 arbiter + side_effect_class in READ_ONLY or actually isolated SNAPSHOT + bounded total cost + consent. Failed or unknown conjunct => SKIP with reason, not an executed pair. A candidate that is unlawful remains useful as a veto warning, never an execution target or admitted hard negative.

## Typed minimum immutable receipt
Receipt fields: request_id; ordinary_caller_ref; subject_domain_id/version; task_intent; law_passport_refs; tenant/project/right_ids; source_family; accepted_parent_state_sha; LoRA/tokenizer/index/projector IDs; route_id/graph_order/Unit/Flow refs; selected_vs_shadow; execution_mode; actual_started_and_ended timestamps; actual output refs/content digest and retained permissible bytes; side_effect_class/isolation_proof; abort reason; hard arbiter verdict; per-branch CPU/wall/RAM/VRAM/storage/external-cost; cancelled/failed attempt cost; comparator/split ID; independent judge version/verdict; producer and consumer ACK. Data minimizing user content and provenance consent are mandatory.

## Lifecycle
1. Pre-admit shadow rights, domain law, type, same source family, accepted state, cost reserve and no business writes.
2. Execute selected user path normally. An eligible existing alternative route may be run privately by **the same existing owner**, bounded by cancellation/deadline.
3. Non-publishing result cannot call user side effects, publish artifacts, advance Student/Graph state or change current answer. Cancel unknown/over-budget branches.
4. Persist only permitted receipt via existing evidence owner. Produce whole PC ledger; compare outputs only when both completed and law-comparable.
5. Independent domain scoring: both feasible, strict lawful utility difference, no contamination => route path preference S28. Invalid law => veto; tie/both-bad => NO_PREF. Recipient RP11/RP13 receives admitted path evidence; Student DPO is a separate authorized branch.

## Stop matrix
| Condition | Required behavior |
| --- | --- |
| Wrong subject or mixed domain not covered by passport | abstain/fallback; no shadow preference |
| Tenant/project or input/output schema mismatch | reject alternative before execution |
| External API/provider lacks one-use consent | no provider shadow call |
| IDE edit / SQL UPDATE / mutation endpoint | no shadow unless real isolated safe snapshot execution proven |
| PC reserve, timeout or cold checkpoint not available | sequential P/C0 only; no H0 gain |
| No real second legal path or consumer | mark INTEGRATION_OPEN or REUSE_NO_CHANGE, do not manufacture |
| One legal route fails; other runs | record failure cost; no false complete pair |
| Both paths equally correct or equally wrong | abstain from strict preference |
| Side effect detected after preflight | stop, preserve user accepted state, report safety failure |
| No trainable policy owner | do not train; leave evidence with correct existing-owner handoff |

**Minimal code direction only if authorized:** existing ordinary host, same RP04 execution envelope, one side-effect-classification gate and append-only comparison receipt. No new shadow scheduler service. Deterministic integrity checks for gating/idempotency only; scientific success requires independent quality and PC benefit.
