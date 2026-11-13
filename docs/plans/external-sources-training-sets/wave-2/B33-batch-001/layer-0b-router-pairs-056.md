# Layer 0-B — Router Pairs (056)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-056-001 — Gap: multi-agent + knowledge graph combined

A: [] | "Multi-agent systems query the knowledge graph to share context across agents."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: Descriptive. No code verb. Both gap-ai-ml-model-001 terms. One event.

---

## C0B-056-002 — IMPLEMENT: implement multi-agent message broker

A: ["IMPLEMENT"] | "Implement a multi-agent message broker that routes events between agent instances."
B: IMPLEMENT → ImplementationFlow41.
Teaching: "implement" fires IMPLEMENT. "multi-agent message broker" = implementation artifact.

---

## C0B-056-003 — Gap: embedding model + vector database + semantic search chain

A: [] | "The embedding model produces vectors stored in the vector database for semantic search queries."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: Descriptive pipeline. No code verb. Three gap terms. Gap ID: gap-ai-ml-model-001. One event.

---

## C0B-056-004 — ARCHITECTURE+API_CONTRACT+IMPLEMENT: full three-predicate request

A: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT"] | "Design the embedding pipeline architecture, integrate the vector database API, and implement the semantic search handler."
B: ARCHITECTURE → ArchitectureFlow31; API_CONTRACT → APIContractFlow33; IMPLEMENT → ImplementationFlow41.
Teaching: Three code verbs, three flows. Standard canonical ordering.

---

## C0B-056-005 — IMPLEMENT: AI-powered adjective does not suppress build verb

A: ["IMPLEMENT"] | "Build an AI-powered semantic search service."
B: IMPLEMENT → ImplementationFlow41.
Teaching: "build" fires IMPLEMENT. "AI-powered" is adjective; never suppresses code verbs.
