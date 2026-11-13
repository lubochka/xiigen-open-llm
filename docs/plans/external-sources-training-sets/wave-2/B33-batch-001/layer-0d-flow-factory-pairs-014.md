# Layer 0-D — Flow Factory Pairs (014)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## FF-014-001 — GapHandler: "few-shot learning" strategy

A: [] | "Use few-shot learning to help the agent handle new query categories."
Factory: GapHandler | Gap ID: gap-ai-agent-llm-001
Reason: "few-shot learning" = AI training strategy. No code verb.

---

## FF-014-002 — GapHandler: "add few-shot examples to prompt"

A: [] | "Add a few-shot example block to the system prompt."
Factory: GapHandler | Gap ID: gap-ai-agent-llm-001
Reason: "add to system prompt" = AI prompt configuration. Not a code artifact.

---

## FF-014-003 — IMPLEMENT: "build few-shot example loader"

A: ["IMPLEMENT"] | "Build a few-shot example loader that reads from the database."
Factory: [ImplementationFlow41] | TransitionType: feature_addition

---

## FF-014-004 — IMPLEMENT+TEST: build + test few-shot retrieval

A: ["IMPLEMENT", "TEST"] | "Implement the few-shot retrieval service and write unit tests for edge cases."
Factory: [ImplementationFlow41, E2ETestingFlow53] | TransitionType: feature_addition
Ordering: IMPLEMENT before TEST.

---

## FF-014-005 — Factory selection summary: few-shot vocabulary

| Request form | Factory selected | Reason |
|-------------|-----------------|--------|
| use few-shot learning | GapHandler | AI strategy vocabulary |
| add few-shot examples to prompt | GapHandler | AI prompt config |
| build the few-shot example loader | ImplementationFlow41 | Code verb |
| integrate few-shot store with recommendation API | APIContractFlow33 | "integrate" always |
| write tests for few-shot retrieval | E2ETestingFlow53 | Test-writing verb |
