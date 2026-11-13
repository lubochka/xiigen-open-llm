# Layer 0-B — Router Pairs (049)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-049-001 — Gap: AI agent standalone

A: [] | "The AI agent autonomously selects tools to complete multi-step tasks."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "AI agent" = AI system type. No code verb. Gap ID: gap-ai-ml-model-001.

---

## C0B-049-002 — IMPLEMENT: build AI agent runtime

A: ["IMPLEMENT"] | "Build an AI agent runtime that dispatches tool calls and collects results."
B: IMPLEMENT → ImplementationFlow41.
Teaching: "build" fires IMPLEMENT. "AI agent runtime" = software implementation artifact.

---

## C0B-049-003 — Gap: multi-agent standalone

A: [] | "Multi-agent coordination requires a shared message bus and conflict resolution."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "multi-agent" = AI system architecture concept. No code verb. Gap ID: gap-ai-ml-model-001.

---

## C0B-049-004 — ARCHITECTURE: design multi-agent coordination platform

A: ["ARCHITECTURE"] | "Design the multi-agent coordination architecture with shared state management."
B: ARCHITECTURE → ArchitectureFlow31.
Teaching: "design" + "architecture" fires ARCHITECTURE. "multi-agent coordination" = object.

---

## C0B-049-005 — IMPLEMENT+SECURITY: build multi-agent dispatcher + secure endpoints

A: ["IMPLEMENT", "SECURITY"] | "Build the multi-agent dispatcher and secure the agent communication endpoints against injection."
B: IMPLEMENT → ImplementationFlow41; SECURITY → SecurityAuditFlow.
Teaching: "build" fires IMPLEMENT; "secure" + injection context fires SECURITY. Two flows.
