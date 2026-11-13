# Layer 0-A — Compiler Pairs (049)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-049-001 — gap: AI agent standalone

A: "The AI agent autonomously selects tools to complete multi-step tasks."
B: [] — "AI agent" = AI system type. No code verb. → gap-ai-ml-model-001.
CompiledPredicateIds: []

---

## C0A-049-002 — IMPLEMENT: build AI agent runtime

A: "Build an AI agent runtime that dispatches tool calls and collects results."
B: ["IMPLEMENT"] — "build" fires IMPLEMENT. "AI agent runtime" = software implementation artifact.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-049-003 — gap: multi-agent standalone

A: "Multi-agent coordination requires a shared message bus and conflict resolution."
B: [] — "multi-agent" = AI system architecture concept. No code verb. → gap-ai-ml-model-001.
CompiledPredicateIds: []

---

## C0A-049-004 — ARCHITECTURE: design multi-agent coordination platform

A: "Design the multi-agent coordination architecture with shared state management."
B: ["ARCHITECTURE"] — "design" + "architecture" fires ARCHITECTURE. "multi-agent coordination" = object context.
CompiledPredicateIds: ["ARCHITECTURE"]

---

## C0A-049-005 — IMPLEMENT+SECURITY: build multi-agent dispatcher + secure endpoints

A: "Build the multi-agent dispatcher and secure the agent communication endpoints against injection."
B: ["IMPLEMENT", "SECURITY"] — "build" fires IMPLEMENT; "secure" + injection context fires SECURITY. Two predicates.
CompiledPredicateIds: ["IMPLEMENT", "SECURITY"]
