# Layer 0-B — Router Pairs (059)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-059-001 — Gap: requirement statement without code verb

A: [] | "We need an AI-powered knowledge graph connector."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: Requirement statement. No code verb fires. "knowledge graph" = gap-ai-ml-model-001.

---

## C0B-059-002 — IMPLEMENT: AI-powered adjective + build verb

A: ["IMPLEMENT"] | "Build an AI-powered knowledge graph connector."
B: IMPLEMENT → ImplementationFlow41.
Teaching: "build" fires IMPLEMENT. "AI-powered" = adjective; does not suppress.

---

## C0B-059-003 — Gap: multi-agent + training loop combination

A: [] | "Multi-agent systems with shared training loops can adapt collectively through reinforcement."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: Descriptive. No code verb. Multiple gap terms across gap IDs. → gap-ai-ml-model-001 (dominant). One event.

---

## C0B-059-004 — ARCHITECTURE+IMPLEMENT: design + build multi-agent system

A: ["ARCHITECTURE", "IMPLEMENT"] | "Design the multi-agent system architecture and implement the shared memory store."
B: ARCHITECTURE → ArchitectureFlow31; IMPLEMENT → ImplementationFlow41.
Teaching: Two code verbs, two flows. ARCHITECTURE before IMPLEMENT per canonical ordering.

---

## C0B-059-005 — DOCUMENT: document AI gap vocabulary boundary

A: ["DOCUMENT"] | "Document the complete AI/ML vocabulary gap boundary for developer onboarding."
B: DOCUMENT → DocumentationFlow62.
Teaching: "document" fires DOCUMENT. "AI/ML vocabulary gap boundary" = object context.
