# Layer 0-D — Flow Factory Pairs (016)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## FF-016-001 — GapHandler: "prompt engineering" strategy

A: [] | "We need better prompt engineering to reduce hallucinations."
Factory: GapHandler | Gap ID: gap-ai-agent-llm-001
Reason: "prompt engineering" = AI vocabulary. "need" does not fire a code predicate.

---

## FF-016-002 — IMPLEMENT: "build prompt engineering toolkit"

A: ["IMPLEMENT"] | "Build a prompt engineering toolkit with a template manager."
Factory: [ImplementationFlow41] | TransitionType: feature_addition

---

## FF-016-003 — IMPLEMENT+TEST: create prompt template library + tests

A: ["IMPLEMENT", "TEST"] | "Create the prompt template library and write regression tests."
Factory: [ImplementationFlow41, E2ETestingFlow53] | TransitionType: feature_addition
Ordering: IMPLEMENT before TEST.

---

## FF-016-004 — SECURITY: prompt injection prevention

A: ["SECURITY"] | "Prevent prompt injection attacks on the agent input layer."
Factory: [SecurityAuditFlow] | TransitionType: feature_addition
Teaching: "prompt injection" = security vocabulary → SecurityAuditFlow regardless of prompt engineering context.

---

## FF-016-005 — Factory selection summary: prompt engineering vocabulary

| Request form | Factory selected | Reason |
|-------------|-----------------|--------|
| prompt engineering for the agent | GapHandler | AI vocabulary |
| build prompt engineering toolkit | ImplementationFlow41 | Code verb |
| create prompt template library | ImplementationFlow41 | Code verb |
| prevent prompt injection | SecurityAuditFlow | Injection = security |
| document prompt engineering guidelines | DocumentationFlow62 | Document verb |
