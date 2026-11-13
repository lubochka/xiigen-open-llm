# S23 state machine, crash boundaries, no-repeat and ordinary recovery

## State invariant
EXISTING_ACCEPTED → ELIGIBILITY_VERIFIED → PAIR_ADMITTED_IMMUTABLE → PRE_STEP_SNAPSHOT_VERIFIED → RESERVED → STEP_STARTED → STEP_EFFECT_KNOWN or UNKNOWN_STATE → TRIAL_SAVED_AND_READBACK → EXAMINED → ACCEPTED_POINTER_CAS or REVERTED → COLD_ORDINARY_READBACK → FEEDBACK_QUARANTINED.

- Never call optimizer Step for pair_id again after STEP_STARTED unless a source-backed proof proves zero effect. An ambiguous timeout/crash = UNKNOWN_STATE, not failed-and-retry. Reconcile read-only from durable attempt journal, weights, optimizer/scheduler/RNG/counters, pair ledger and process ownership.
- Baseline marker alone is insufficient. K/14.ContinuousLearning/Training/CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.cs explicitly states baseline.json omits weights/optimizer/random generators.
- Checkpoint compatibility ties accepted parent, trial and model config, tokenizer, encoder/vector codebook, ranker/index, Graph policy and DPO reference identity. Do not silently cold-start after a corrupt accepted checkpoint. Version migration and reindex are discrete authorized state changes.
- Pair provenance includes exact x/context/chosen/rejected bytes, complete hash/readback, teacher author and CODEX exact judge, domain/law/edition/project/tenant rights and immutable pair_id; generic user feedback source is not substituted.
- If training changes only Student project LoRA, do not claim Unit/Flow/Orchestrator/Graph were trained. Journal each mutable family independently, and link any learning policy selected in the request to actual loaded state.

## Fault-injection matrix (scoped, safe, no endless Windows runs)
1. PRE Step disk-full: no gradient, accepted state preserved, reserve released.
2. Mid-Step kill or HTTP timeout: UNKNOWN_STATE, exact pair blocked, no new trial accepted without reconciliation.
3. Post-Step pre-journal crash: investigate whether mutation durable; never optimistic retry.
4. Trial saved, exam fails or aborts: accepted pointer unchanged, trial quarantined.
5. Accept pointer write races with second actor: compare-and-swap/lease and readback; no split accepted state.
6. Disk-full during rollback: accepted parent persists, mandatory recovery route; no destructive retry.
7. Cold restart incompatible tokenizer/index/Graph/LoRA: reject incompatible candidate rather than silently score incorrect vectors.
8. User feedback duplicate, out-of-date law, cross-project, poison, missing consent: quarantine and dedupe; no Step.
9. Local checkpoint saved but remote Git/LFS absent: local adoption status != remote publication.
10. Canceled async graph branch left running: count CPU/VRAM and side effects; respect deadline and idempotency.

For deterministic safety invariants use tiny targeted fault checks or controlled simulation in an isolated test directory with known restore; these checks cannot prove improved learning. Any destructive failure experiment on real accepted artifacts requires an additional explicit effect grant.

## Consumer proof
New-process ordinary call must name selected accepted checkpoint+hash, actual project/subject/law, tokenizer/index/Graph compatibility, and what answer/evidence it used. One successful direct API test endpoint or a manifest does not establish host user adoption. Native DTC/IDE layers have their own owner and gates.
