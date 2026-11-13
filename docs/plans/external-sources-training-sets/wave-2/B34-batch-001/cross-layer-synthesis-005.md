# Cross-Layer Synthesis (005)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: cross_layer_synthesis

---

## CLS-005 — Pure gap: "Explain the architecture to the team"

**Request**: "Explain the event-driven architecture to the new engineering team."

### Layer 0-A (Compiler)
Tokens: explain → teaching vocabulary → gap; event-driven architecture → topic noun (no design verb).
Predicate set = []
CapabilityGapSignal: gap-skills-guidance-train-001 | DomainId: software-development

### Layer 0-B (Router)
Predicates = [] → GapHandler → gap-skills-guidance-train-001
No flow selected.

### Layer 0-C (Knowledge Edge)
"explain" = always gap, no exception. Architecture as topic ≠ ARCHITECTURE predicate. ARCHITECTURE fires on design/architect verbs. Explain = teaching = gap-skills-guidance-train-001.

### Layer 0-D (Flow Factory)
Predicates = [] → GapHandler factory selected. No code flow instantiated.
TransitionType: none.

### Layer 1 (Transition Orchestrator)
GapHandler invoked. SCHEDULE_CAPABILITY_LEARNING emitted.
{gap_id: "gap-skills-guidance-train-001", domainId: "software-development", request: "explain the event-driven architecture to the new engineering team"}

### Layer 2 (Flow Execution)
GapHandler response: "Capability not available. Request logged for learning pipeline."
Verdict: GAP_LOGGED

### Layer 3 (Teaching)
Key rule: "explain" = always gap, regardless of topic. Architecture as subject does not activate ARCHITECTURE. The requester's action is "explain" — a teaching verb. Teaching verbs produce capability gap signals, not technical flow execution.

**TransitionType**: none | **Gap logged**: gap-skills-guidance-train-001 | SCHEDULE_CAPABILITY_LEARNING emitted.
