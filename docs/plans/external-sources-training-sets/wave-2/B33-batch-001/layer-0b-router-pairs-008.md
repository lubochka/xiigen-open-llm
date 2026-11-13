# Layer 0-B — Router Pairs (008)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: router

---

## C0B-008-001 — Full multi-predicate: IMPLEMENT+TEST+DOCUMENT

A: ["IMPLEMENT", "TEST", "DOCUMENT"] | "Build classifier, write tests, document taxonomy"
B: IMPLEMENT → 4.1; TEST → 5.1; DOCUMENT → DocumentationFlow62. Three flows. TransitionType: feature_addition.

---

## C0B-008-002 — Gap: "agent strategy" without design verb

A: [] | "Our agent strategy needs rethinking for enterprise use cases"
B: GapHandler
Reason: Strategic/business statement without code or architecture verb → gap.

---

## C0B-008-003 — ARCHITECTURE alone: "design the AI system"

A: ["ARCHITECTURE"] | "Design the AI-powered customer support platform architecture"
B: ARCHITECTURE → sub-phase based on vocabulary. TransitionType: documentation (no IMPLEMENT).

---

## C0B-008-004 — Gap: "agent performance" without code verb

A: [] | "The agent performance degrades on complex multi-turn conversations"
B: GapHandler
Reason: Performance observation without predicate-triggering verb → gap.

---

## C0B-008-005 — IMPLEMENT: "build guardrails"

A: ["IMPLEMENT"] | "Build content safety guardrails for the AI agent output"
B: IMPLEMENT → ImplementationFlow41 (4.1) | "build guardrails" = software feature implementation.
