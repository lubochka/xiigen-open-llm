# Layer 0-D — Flow Factory Pairs (013)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## FF-013-001 — GapHandler: "fine-tune the model"

A: [] | "We need to fine-tune the model on internal support tickets."
Factory: GapHandler | Gap ID: gap-ai-agent-llm-001
Reason: "fine-tune" = AI training vocabulary. No code verb. GapHandler emits SCHEDULE_CAPABILITY_LEARNING.

---

## FF-013-002 — IMPLEMENT: "build fine-tuning pipeline"

A: ["IMPLEMENT"] | "Build a fine-tuning pipeline that ingests support tickets and triggers retraining."
Factory: [ImplementationFlow41] | TransitionType: feature_addition
Reason: "build" = code verb. "fine-tuning pipeline" = software artifact. GapHandler not invoked.

---

## FF-013-003 — IMPLEMENT+DOCUMENT: build + document fine-tuning

A: ["IMPLEMENT", "DOCUMENT"] | "Implement the fine-tuning job scheduler and document the tuning configuration schema."
Factory: [ImplementationFlow41, DocumentationFlow62] | TransitionType: feature_addition
Ordering: IMPLEMENT before DOCUMENT (document the implemented artifact).

---

## FF-013-004 — GapHandler: "adapt the model via fine-tuning"

A: [] | "Adapt the model by fine-tuning it on medical records."
Factory: GapHandler | Gap ID: gap-ai-agent-llm-001
Reason: Both "adapt" and "fine-tune" are AI vocabulary. Double AI vocabulary still yields GapHandler.

---

## FF-013-005 — Factory selection summary: fine-tune vocabulary

| Request form | Factory selected | Reason |
|-------------|-----------------|--------|
| fine-tune the model | GapHandler | AI verb |
| build fine-tuning pipeline | ImplementationFlow41 | Code verb |
| integrate fine-tuning with registry API | APIContractFlow33 | "integrate" always |
| document the fine-tuning schema | DocumentationFlow62 | Document verb |
| adapt via fine-tuning | GapHandler | Both AI verbs |
