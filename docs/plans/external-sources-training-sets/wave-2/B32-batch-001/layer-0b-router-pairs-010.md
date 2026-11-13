# Layer 0-B — Router Pairs (010)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-0b-router-pairs

---

## B32-0B-010-001 — Seven-predicate routing: all flows selected

**Predicates received:** `["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "SECURITY", "TEST", "DOCUMENT", "QA"]`

**Source request:** "Quick vibe session: architect the auth layer, integrate the OAuth provider, build the login service, secure the tokens, write unit tests, document the flow, and QA the implementation."

**Gap warnings present:** gap-vibe-coding-style-001

**Router output:**
```
RouteTarget: [ArchitectureFlow31, APIContractFlow, BackendServiceFlow41, SecurityFlow, TestFlow, DocumentFlow, QAFlow]
FlowSelected: [ArchitectureFlow31, APIContractFlow, BackendServiceFlow41, SecurityFlow, TestFlow, DocumentFlow, QAFlow]
ParallelEligible: true (subset of flows can proceed in parallel)
OrderConstraint: QA must run last (after IMPLEMENT and TEST)
TransitionType: feature_addition
GapWarnings: [gap-vibe-coding-style-001]
DomainId: "software-development"
```

---

## B32-0B-010-002 — Ordering constraints for seven-predicate routing

**Ordering rules that apply regardless of vibe framing:**

1. ARCHITECTURE can start first (no dependency)
2. API_CONTRACT can proceed in parallel with ARCHITECTURE
3. IMPLEMENT depends on ARCHITECTURE being available
4. SECURITY can proceed after IMPLEMENT
5. TEST follows IMPLEMENT
6. DOCUMENT follows IMPLEMENT (can be parallel with TEST)
7. QA must be last (requires TEST and IMPLEMENT to be complete)

Gap warnings have zero effect on these ordering constraints.

---

## B32-0B-010-003 — Five-predicate vibe session routing

**Predicates received:** `["ARCHITECTURE", "IMPLEMENT", "API_CONTRACT", "TEST", "DOCUMENT"]`

**Source request:** "Vibe coding mode: design the architecture, implement the core service, integrate the payment gateway, test the flows, and document the system."

**Router output:**
```
RouteTarget: [ArchitectureFlow31, BackendServiceFlow41, APIContractFlow, TestFlow, DocumentFlow]
FlowSelected: [ArchitectureFlow31, BackendServiceFlow41, APIContractFlow, TestFlow, DocumentFlow]
TransitionType: feature_addition
GapWarnings: [gap-vibe-coding-style-001]
DomainId: "software-development"
```

---

## B32-0B-010-004 — Complete B32 routing decision table

| Predicates | RouteTarget | TransitionType | Notes |
|---|---|---|---|
| [] | knowledge_edge | unknown | SCHEDULE_CAPABILITY_LEARNING emitted |
| [IMPLEMENT] | BackendServiceFlow41 | feature_addition | |
| [UI] | UIFlow | feature_addition | |
| [UI, IMPLEMENT] | UIFlow + BackendServiceFlow41 | feature_addition | parallel-eligible |
| [ARCHITECTURE] | ArchitectureFlow31 | documentation | |
| [ARCHITECTURE, IMPLEMENT] | ArchitectureFlow31 + BackendServiceFlow41 | feature_addition | |
| [API_CONTRACT] | APIContractFlow | feature_addition | |
| [SECURITY] | SecurityFlow | feature_addition | |
| [TEST] | TestFlow | no contribution | |
| [DOCUMENT] | DocumentFlow | documentation | |
| [QA] | QAFlow | no contribution | QA last |
| [all 7] | all flows | feature_addition | QA ordering constraint |

---
