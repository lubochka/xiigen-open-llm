# S26 evidence-state lifecycle and selective impact — design, not existing API
## Events and lineage graph
Minimal proposed **compatibility information**, not asserted existing fields:
`evidence_id | source_family | source_hash | source_version | subject | task_intent | law_edition | tenant | project | rights/consent | time | feedback_event | pair_id_if_any | teacher/judge receipts_if_any | checkpoint_before/after | index/association/route_refs | ordinary consumer/version | validity_status | independent evidence`.
Capture **parent→material→trained_state→accepted_pointer→consumer** edges when they really exist. A single consent withdrawal may attach to numerous source copies. Nonexistent lineage means UNKNOWN_IMPACT, not silently erase.

State transitions: OBSERVED → SOURCE_VERIFIED → [QUARANTINED or ADMISSIBLE] → [(CODEX_CASE_SELECTED → OPEN_CHINESE_BOTH_FINAL_SIDES → CODEX_EXACT_JUDGED → TRAIN_ELIGIBLE) or NON_TRAIN_FEEDBACK] → STORED → [APPLIED_ONCE or NOT_APPLIED / UNKNOWN_STEP] → [INDEPENDENTLY_EXAMINED] → [ACCEPTED and COLD_USED or REJECTED].
Later CONTESTED → VERIFIED_AS_WRONG / TEMPORALLY_SUPERSEDED / RIGHTS_WITHDRAWN / AMBIGUOUS → IMPACT_TRACED → action → cold re-exam.
One state is NOT an optimizer receipt. Never convert a generic `FeedbackRouter.RouteEdited` user-correction pair to S04 Chinese-authored DPO by relabeling. Exact material after Compact/tokenization must still match what CODEX judged; low contrast / same tokens = not adequate data.

## Repair actions
- **PENDING_ONLY:** mark ineligible through current pair-store owner; no trained weights to undo, but preserve audit and permissions.
- **INDEX/MEMORY_ONLY:** suppress/rebuild stale eligible memory/index version; verify no invalid neighbor actually contributes after cold restore, preserve lawful negative/warning evidence as such.
- **UNIT/FLOW/GRAPH:** validate typed association/OutputRef and version on final use; cancel stale in-flight route before side-effect where possible; fallback to lawful full path.
- **TRAINED_LO RA:** if clean, compatible full parent snapshot exists, propose safe rollback with all optimizer/RNG/reference/index version pointers and host verification. No clean parent or mixed signals -> ERASURE_NOT_PROVEN, block unsafe accepted pointer, select safe fallback.
- **NEW LEARNING:** only for *different* provable residual weakness and unique granted pair, not reusing contaminated/retired pair or closed exam. Uncertainty may mean NO_TRAINING_NEEDED.

**Legal/right boundary:** rights withdrawal is not itself proof that system has purged already learned parameters, all backups, or Git history. Owner must define verifiable scope of access/retention/required deletion; no destructive cleanup from planning permission alone.

**Source-law authority remains deterministic; learned signals are not truth.** Sample failures: negation dropped in EN→RU; wrong SQL dialect indexed; C# nullable annotation confused with runtime guard; Graph path violates dependency; user's preference differs from factual correctness; index returns multiple neighbors but code consumes only first. Each case needs an examined real caller and actual source edition before claiming failure or cure.
