# Layer 0-B — Router Pairs (051)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-051-001 — Gap: AI agent + multi-agent combined

A: [] | "AI agents and multi-agent systems share memory through a common context store."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: Descriptive. No code verb. Both gap-ai-ml-model-001 terms. One event.

---

## C0B-051-002 — IMPLEMENT: implement AI agent tool dispatcher

A: ["IMPLEMENT"] | "Implement the AI agent tool dispatcher that routes calls to registered tool handlers."
B: IMPLEMENT → ImplementationFlow41.
Teaching: "implement" fires IMPLEMENT. "AI agent tool dispatcher" = implementation artifact.

---

## C0B-051-003 — Gap: knowledge graph + semantic search combined

A: [] | "Knowledge graph enrichment improves semantic search recall by adding entity links."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: Descriptive. No code verb. Both gap-ai-ml-model-001 terms. One event.

---

## C0B-051-004 — IMPLEMENT+TEST: build knowledge graph adapter + write tests

A: ["IMPLEMENT", "TEST"] | "Build the knowledge graph adapter and write integration tests for the query layer."
B: IMPLEMENT → ImplementationFlow41; TEST → TestFlow51.
Teaching: "build" fires IMPLEMENT; "write integration tests" fires TEST. Standard order.

---

## C0B-051-005 — DOCUMENT: document AI agent architecture

A: ["DOCUMENT"] | "Document the AI agent architecture including tool registry and memory management design."
B: DOCUMENT → DocumentationFlow62.
Teaching: "document" fires DOCUMENT. "AI agent architecture" = object context.
