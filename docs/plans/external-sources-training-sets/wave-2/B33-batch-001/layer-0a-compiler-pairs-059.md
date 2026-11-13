# Layer 0-A — Compiler Pairs (059)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-059-001 — gap: AI vocabulary negative test — adjective form does not trigger gap

A: "We need an AI-powered knowledge graph connector."
B: ["IMPLEMENT"] — Wait: "need" is not a code verb. But "connector" implies build? No. No code verb fires. Correct: [] — "need an AI-powered knowledge graph connector" is a requirement statement. No code verb. → gap-ai-ml-model-001.
CompiledPredicateIds: []

---

## C0A-059-002 — IMPLEMENT: adjective "AI-powered" does not suppress build verb

A: "Build an AI-powered knowledge graph connector."
B: ["IMPLEMENT"] — "build" fires IMPLEMENT. "AI-powered" = adjective; does not suppress code verb.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-059-003 — gap: multi-agent + training loop combination

A: "Multi-agent systems with shared training loops can adapt collectively through reinforcement."
B: [] — Descriptive. No code verb. Multiple gap terms across gap IDs. → gap-ai-ml-model-001 (multi-agent dominant). One event.
CompiledPredicateIds: []

---

## C0A-059-004 — ARCHITECTURE+IMPLEMENT: design + build multi-agent system

A: "Design the multi-agent system architecture and implement the shared memory store."
B: ["ARCHITECTURE", "IMPLEMENT"] — "design" + "architecture" fires ARCHITECTURE; "implement" fires IMPLEMENT. Two predicates.
CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT"]

---

## C0A-059-005 — DOCUMENT: document complete AI gap vocabulary boundary

A: "Document the complete AI/ML vocabulary gap boundary for developer onboarding."
B: ["DOCUMENT"] — "document" fires DOCUMENT. "AI/ML vocabulary gap boundary" = object context.
CompiledPredicateIds: ["DOCUMENT"]
