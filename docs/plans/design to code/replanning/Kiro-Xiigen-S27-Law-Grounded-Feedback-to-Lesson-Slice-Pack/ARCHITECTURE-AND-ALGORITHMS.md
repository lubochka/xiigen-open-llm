# S27 — One actual feedback-evidence seam in a deterministic/learned architecture

## Route, from normal user to next lawful learning event

    ordinary local user request
      → RP02 domain/intent/subject/stack/law-version/rights
      → RP02 semantic tokens + domain vector + eligible BM25/dynamic multi-hit kNN
      → RP03 ILearningUnit<TIn,TOut> associated many→many typed candidates
      → RP03 ILearningFlow learned correction of output→next input relations
      → RP04 ILearningOrchestrator bounded async fanout/fallback/cancellation
      → RP04 ILearningDynamicGraph order-2/order-3 pattern relations
      → D-15 deterministic arbiter → chosen Student/LoRA → ordinary response
        ↓ accepted/edited/rejected user feedback + same selected state
      S27: preserve LAW-GROUNDED FEEDBACK EVIDENCE at real ordinary caller
        ├→ NO_TRAIN/CLARIFY/QUARANTINE (insufficient law, rights, drift, OOD)
        ├→ RP12 deterministic law/type repair or RP02/18 representation+kNN
        ├→ RP03/04/11/13 typed-flow/graph/path owner
        └→ RP05 Student weakness candidate → CODEX Astra choice
              → authorized open Chinese chosen+rejected
              → separate CODEX Astra exact-pair judge → RP01 Student once
              → RP09 heldout → RP10 promote/revert → cold user → next bulk

This diagram is a **proposed architecture/ownership contract, not a discovered end-to-end K call**. Every edge must be shown in real production caller/consumer code or marked UNBOUND. Existing Machine governs training, not four new trainers. No hidden shadow DPO pipeline.

## Proposed bounded S27 typed evidence record — NOT a claimed K API

    FeedbackEvidenceV1 {
      immutable_feedback_id, original_request_sha, original_answer_sha, user_text_sha,
      ordinary_consumer_id, selected_parent_state_id, feedback_event_time,
      tenant/project, consent/rights/source_license, source_family,
      D04_task_intent, subject_domains[], language, stack, SQL_dialect,
      domain_law_ids_and_versions[], rule_primary_source_refs[],
      encoder/tokenizer/codebook/index/vector_space/LoRA_state_ids,
      ordered_path_and_branch_ids, Unit_many_to_many_associations, typed_OutputRefs,
      signal: ACCEPTED|EDITED|REJECTED|CLARIFY,
      hypothesis: LAW_VIOLATION|STYLE_PREFERENCE|CHANGED_TASK|DOMAIN_MAP|
        TOKEN_SPACE|KNN_CANDIDATES|UNIT_FLOW|ORCH_GRAPH|STUDENT_WEAKNESS|
        UNSAFE|UNKNOWN,
      evidence: negative_warning/contrast/law_source/uncertainty,
      decision: NO_TRAIN|NEEDS_FACTS|SEND_TO_EXISTING_OWNER|LESSON_CANDIDATE,
      recipient_owner, full_pc_cost_ref, isolated_exam_panel_id
    }

Do not bind schema to public APIs until full code/consumer read and minimal owner-approved seam proof. Exact law, negation, condition, scopes, nullability, units, rights, time/order, project isolation and typed OutputRefs are **deterministic admissibility**, not learnable preference. ILearningUnit returns a real typed candidate relation, not merely an ID list; Flow association may not be positional Zip; async and Graph must account for every attempted/discarded branch and final arbiter veto.

## What really learns

RP02 domain-specific k function may be k_d(x) = clip(g_d(ambiguity,density,margin,lawRisk,latencyBudget),1,Kmax) over rights/law-eligible neighbors; weighted feature group rank may be learned on labeled independent feedback, but same-size vectors need same coordinate semantic space. ANN is a candidate generator; nonmetric rerank must not be inserted into HNSW distance and called equivalent. RP02 owns all changes. RP03 Flow corrects typed relations; RP04 Orch/Graph learned paths; Student RP01 DPO changes response-only recipient weights; RP18 explicit projection/quantizer is required if LoRA outputs need discrete domain tokens. S27 **captures the causal counterexample and target owner**; it does not learn new parameters on its own.

## Novel seam and decision tree

1. First prove existing ordinary feedback path and actual current validation, using full FeedbackRouter, its interfaces and callers and the cold selected state. Generic edited→DpoPair is not automatically wrong or automatically approved Chinese final pair: authorship/content governance may prevent admission. Check the complete chain.
2. A true subject-law contradiction? Validate independent law/version/source, classify whether deterministic correction beats learning. A correct deterministic result cannot be a negative merely because it is deterministic.
3. If mismatch is semantic/vector/kNN/typed/path, send an evidence receipt to the existing owner; **never build another algorithm in S27**.
4. If residual Student weakness remains after matched corrected no-update C0 and teaching is allowed, CODEX chooses the lesson, approved Chinese authors both sides, separate CODEX judges exact bytes.
5. If no absent S27 boundary exists return REUSE_NO_CHANGE. If no real host exists return INTEGRATION_OPEN, not product gain. Cold actual consumption, independent measured effect and whole-PC budget decide next bulk.
