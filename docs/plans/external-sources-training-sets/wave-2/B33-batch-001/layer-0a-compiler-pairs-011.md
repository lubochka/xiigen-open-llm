# Layer 0-A — Compiler Pairs (011)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: compiler

---

## C0A-011-001 — gap: "AI orchestrator" as noun statement

A: "Our AI orchestrator is not scaling with load."
B: [] — observation about AI system; "AI orchestrator" (noun) + no code-writing verb → gap.
CompiledPredicateIds: []

---

## C0A-011-002 — ARCHITECTURE fires: "architect the orchestration layer"

A: "Architect the orchestration layer for the multi-agent system."
B: ["ARCHITECTURE"] — "architect" (verb) fires ARCHITECTURE. AI orchestration is the target, not the trigger.
CompiledPredicateIds: ["ARCHITECTURE"]

---

## C0A-011-003 — gap: "embedding model"

A: "Switch the embedding model from text-embedding-ada-002 to text-embedding-3-large."
B: [] — model configuration gap. No code-writing verb.
CompiledPredicateIds: []

---

## C0A-011-004 — IMPLEMENT fires: "build the embedding pipeline"

A: "Build the embedding pipeline for the RAG knowledge base."
B: ["IMPLEMENT"] — "build" fires IMPLEMENT. "embedding pipeline", "RAG" are AI vocabulary objects; code verb fires.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-011-005 — gap + IMPLEMENT: "train" is gap, "implement training pipeline" fires

A: "Train the agent on customer support conversations."
B: [] — "train" (ML/AI sense) → gap. Not a software implementation verb.

A: "Implement the training data pipeline for the support agent."
B: ["IMPLEMENT"] — "implement" fires IMPLEMENT. "training data pipeline" is software artifact.
CompiledPredicateIds for first: [] | For second: ["IMPLEMENT"]
