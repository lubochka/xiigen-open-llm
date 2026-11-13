# Layer 3 — Teaching Unit Pairs (017)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## T017-001 — "scalability" noun fires ARCHITECTURE

A: "Design the scalability architecture." Predicates?
B: ["ARCHITECTURE"]. "design" → ARCHITECTURE. "scalability architecture" → ARCHITECTURE via "scalability" (ARCHITECTURE trigger noun) and "architecture". Doubly confirmed.

---

## T017-002 — "scale the pods" is operational gap

A: "Scale the pods to 20 replicas." Predicates?
B: Zero predicates. "scale" → operational gap. "pods", "replicas" → Kubernetes operational vocabulary, gap. Result: [] → knowledge_edge. Compare: "design the scaling architecture" → ARCHITECTURE.

---

## T017-003 — "HPA design" fires ARCHITECTURE via "design" + "strategy"

A: "Design the HPA configuration strategy for CPU-based scaling." Predicates?
B: ["ARCHITECTURE"]. "design" → ARCHITECTURE. "HPA configuration strategy" → ARCHITECTURE via "strategy". "HPA" → gap (tool name) but doesn't suppress.

---

## T017-004 — ARCHITECTURE+DOCUMENT for scaling ADR

A: "Architect the auto-scaling topology and document the scale-out triggers as an ADR." Predicates? TransitionType?
B: ["ARCHITECTURE", "DOCUMENT"]. "architect" + "auto-scaling topology" → ARCHITECTURE. "document" + "ADR" → DOCUMENT. TransitionType: documentation. Two flows: 3.5 + 6.2.

---

## T017-005 — "capacity planning model" fires ARCHITECTURE

A: "Define the capacity planning model for the infrastructure layer." Predicates?
B: ["ARCHITECTURE"]. "define" → ARCHITECTURE. "capacity planning model" → ARCHITECTURE via "model". "infrastructure layer" → ARCHITECTURE via "layer". Multiple ARCHITECTURE vocabulary items. Route to 3.5.
