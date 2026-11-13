# S25 — evidence-to-accepted-full-state identity contract

## Required bindable record, adapt to existing native owner contracts
- experiment_id, attempt_id, parent_run, treatment_id {P,C0,T1,T2}, pair_id/admission_id if trained, authorized trainer and immutable one-Step journal status (APPLIED_ONCE/NOT_APPLIED/UNKNOWN_STATE), exact model/provider grants.
- full source identities: sample family digests (TRAIN/DEV/SEALED_CERT), task intent, subject, stack, edition/version, source rights/tenant/project and independently frozen rubric.
- code versions: planning P commit, runtime K commit, actual code diff, DTC/IDE host commit/build, instruction/skill versions applicable to interpretation.
- representation: IR schema, tokenizer/codebook/vector projector and semantic version, index/corpus and reindexability, metric/dynamic k/rerank/pool fingerprints; show USED vs NOT_USED, not absent=default.
- learning states: base Student/reference, trainable parameter names/LoRA scope, Unit/Flow/Orchestrator/Graph/Builder state fingerprints **only when used**, optimizer/RNG/counters/graph/index snapshots for recoverable restart.
- trial observation: per-case lawful rubric scores, abstention/wrong-law, selected permissible path ID vs feasible set, actual branch costs, latency warm/cold, whole-PC CPU/RAM/VRAM/storage, known leak/confound flags and paired-family uncertainty.
- examiner receipt: independently sealed panel ID and model/operator (or NOT_INVOKED), result IMPROVE/REGRESS/INCONCLUSIVE/INVALID_EVIDENCE with preregistered promotion rule and old-domain retention.
- adoption receipt: accepted pointer / old pointer, atomic cutover/rollback identity and time; **new process** actual ordinary DTC/IDE/local request ID with selected full state+answer+human utility; FAIL_CLOSED on incompatible version or missing pointer, no silent parent/neighbor fallback.
- skeptical review: exact candidate HEAD/blob set and **actual** independent CODEX Astra invocation identity/model/effort/result or NOT_PERFORMED; no file author self-certification.

## Identity mismatch examples
GOOD: C# nullable docs P/C0/T hold law+tokenizer+retrieval constant; T truly updates allowed project LoRA; independent family rubric shows gain; RP10 installed T full snapshot, cold local request points to same T, old-domain retention and peak hardware fit recorded.
BAD: examiner compares T with different index than P, promoter installs only LoRA while cold consumer loads stale domain projection; claims learned gain.
EDGE: authorized one Step returns timeout UNKNOWN_STATE. Hold old accepted pointer; reconcile journal and full snapshot; never rerun same pair to make outcome known.

## A safe decision with partial evidence
If no train grant, run P vs C0 real ordinary baseline with source receipts only. If no sealed independent panel, return EVIDENCE_INSUFFICIENT, do not promote. If no actual host, report SCIENCE_EXAMINED_BUT_NOT_ADOPTED. If no independent Astra model, report NOT_PERFORMED; neither production state nor review result can be fabricated.