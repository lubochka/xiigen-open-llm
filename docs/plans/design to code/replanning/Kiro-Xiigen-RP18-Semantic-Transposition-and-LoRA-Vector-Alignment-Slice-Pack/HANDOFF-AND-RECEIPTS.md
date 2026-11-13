# RP18 — receipts and owner handoff

Each task returns a **single bounded receipt**, not a synthesized claim that 25 research rounds were executed.

Required keys:
- `task_id`, `scope_current`, `scope_excluded`, `actor_role/model`, `actually_loaded_skills_sha`, `start_branch_sha` and `end_branch_sha`.
- `source_ledger`: `repo@commit:path#lines + blobSha + depth (INDEX_ONLY | PRIMARY_READ | CODE_TRACE | RUNTIME_OBSERVED)`, and observed vs inherited claims separately. Include own question-specific RAG MD/NDJSON→primary route.
- `owner/effects`: allowed target files, code/data/model/exam/cost/native grants; unique write lease; actual actions and effect IDs or `NO_EFFECT`.
- `representation_contract`: raw exact input and domain-law refs, source encoder/tokenizer/LoRA/space/codebook IDs, target index/typed consumer identity, geometry/semantics, migration/read-only decision, no hidden state.
- `implementation`: current caller→method→downstream consumer, hypothesis, minimal proposed or actual diff, state hash before/after, no-repeat/rollback receipt, result `REUSE_NO_CHANGE` if equivalent.
- `science`: preregistered law passports, family splits/heldout policy, arms and only-one-variable delta, frozen evaluator, true n, empirical data URLs, accuracy/critical errors/route/time/whole-PC/retention/uncertainty and negative results. Absent run = `NOT_MEASURED`.
- `governance`: CODEX case/teacher-both-sides/exact-pair judge Student/independent reviewer receipts as applicable, exact SHA and review/authority status; ungranted stages `NOT_PERFORMED`.
- `decision`: `IMPROVE | REGRESS | INCONCLUSIVE | INVALID_EVIDENCE | NOT_MEASURED | NO_TRAINING_NEEDED | REUSE_NO_CHANGE | SOURCE_REQUIRED | AUTHORITY_REQUIRED` + next owner and first falsifier.

**Handoff map:** RP18→RP02 for source domain law, vector/token/index native internals and adaptive k; RP18→RP03 for actual Unit/Flow association; RP18→RP04 for bounded async/higher-order; RP18→RP01/R18 for Student DPO target; RP18→RP05/RP10 for full-state pointer/recovery; RP18→RP09 for independent exams; RP18→RP14 for law changes; RP18→RP06/S* for host; RP18→RP11/12/13 for attribution/repair. Do not call a handoff an applied code patch. Link the originating observed method, needed missing seam, allowed owner and falsifier.

**Human summary template (Russian, four sentences):**
1. Что новый путь действительно делает/не делает для пользователя.
2. Какой конкретный исходный закон и кодовый consumer подтвердили это.
3. Что измерено vs не измерено, включая стоимость и отрицательные результаты.
4. Следующий узкий владелец/разрешение, без предложения начать 25 проходов заново.
