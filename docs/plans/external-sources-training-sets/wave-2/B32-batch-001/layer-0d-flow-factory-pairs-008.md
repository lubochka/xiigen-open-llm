# Layer 0-D — Flow Factory Pairs (008)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-0d-flow-factory-pairs

---

## B32-0D-008-001 — v6 factory corruption: VIBE_CODING flow invented

**CORRUPTED factory output (v6-01):**

```
Request: "Vibe code a quick service — just make it work."
Predicates: ["VIBE_CODING"]
Factory: VibeCodingFlow
FlowInstance: VibeCodingFlowInstance
```

**WHY THIS IS WRONG:**

1. "VIBE_CODING" is not a registered predicate — the compiler should never emit it
2. "VibeCodingFlow" is not a registered factory — the factory layer should never create it
3. Even if the compiler emitted "VIBE_CODING" (corrupt), the factory must reject unregistered predicates
4. The correct result: IMPLEMENT fires (from "code" root), BackendServiceFlow41 is created

**CORRECT factory output:**

```
Predicates: ["IMPLEMENT"]
Factory: BackendServiceFlow41
FlowInstance: BackendServiceFlow41Instance
GapMetadata: {vibe-style: true, just-works: true}
```

---

## B32-0D-008-002 — Factory registry: closed-world assumption

**The factory registry contains exactly these entries:**

```
{
  "IMPLEMENT": BackendServiceFlow41,
  "UI": UIFlow,
  "ARCHITECTURE": ArchitectureFlow31,
  "API_CONTRACT": APIContractFlow,
  "SECURITY": SecurityFlow,
  "TEST": TestFlow,
  "DOCUMENT": DocumentFlow,
  "QA": QAFlow
}
```

Any predicate not in this registry:
- Cannot be resolved to a factory
- Must be rejected at the factory layer
- Triggers a fallback to knowledge_edge with an error log

---

## B32-0D-008-003 — Factory error handling for unregistered predicates

**When factory receives unregistered predicate (e.g., "VIBE_CODING"):**

```
factory.resolve("VIBE_CODING")
→ throws FactoryRegistryException("VIBE_CODING is not a registered predicate")
→ log: error_code = PREDICATE_NOT_IN_REGISTRY
→ fallback: knowledge_edge
→ learning_signal: SCHEDULE_CAPABILITY_LEARNING (because routing ultimately failed)
```

This prevents v6-01 corruptions from propagating to flow execution.

---

## B32-0D-008-004 — Distinguishing factory corruption from valid routing

| Scenario | Factory outcome | Notes |
|---|---|---|
| Valid predicates, vibe framing | Factory created, gap metadata attached | Correct |
| Invalid predicate (VIBE_CODING) | Factory rejected, error logged | v6-01 detection |
| Zero predicates | Factory bypassed, knowledge_edge | Correct B32 gap behavior |
| Factory Gate B failure | Factory rejected, probationary_rejected | Factory-internal failure |

---
