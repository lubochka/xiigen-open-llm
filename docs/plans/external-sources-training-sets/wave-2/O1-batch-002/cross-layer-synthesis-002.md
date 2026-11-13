# Cross-Layer Synthesis (002)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: cross_layer_synthesis

---

## Scenario: "The client portal is down" → knowledge_edge

---

### Layer 0-A: Compiler

A: "The client portal is down"

B: Predicates: [] | TransitionType: knowledge_edge
   Reasoning: "client portal" is colloquial. "portal" not in lexicon. "down" not in lexicon. Zero predicates detected.

---

### Layer 0-B: Router

A: Predicates: [] | TransitionType: knowledge_edge

B: RoutingDecisions: []
   Reasoning: No predicates → no routing decisions. knowledge_edge produces zero routing decisions.

---

### Layer 0-D: Factory

A: RoutingDecisions: []

B: FlowSelections: []
   Reasoning: Zero routing decisions → zero factory selections. No adapter instantiated.

---

### Layer 1: Orchestration

A: SelectedLogicalTransition: { predicates: [], transition_type: "knowledge_edge", routing: [] }

B: OrchestrationPlan: { flows: [], execution: "none", reason: "knowledge_edge_no_routing", action: "log_to_unroutable_corpus" }

---

### Layer 2: Nine-Node

A: (not reached — no flow selected)

B: No nine-node pipeline executes. The prompt is logged to the unroutable corpus.

---

### Cross-Layer Summary

"The client portal is down" passes through all layers without generating any routing, factory selection, or pipeline execution. At every layer the output is empty/none. This is the correct and expected behavior for a knowledge_edge prompt. The cross-layer trace confirms that a single colloquial prompt correctly produces zero output at every layer — and that this zero output is a successful outcome, not a system failure.
