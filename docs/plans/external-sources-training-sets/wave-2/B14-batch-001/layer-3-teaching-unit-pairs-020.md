# Layer 3 — Teaching Unit Pairs (020)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## T020-001 — "observability infrastructure" fires ARCHITECTURE

A: "Design the observability infrastructure." ARCHITECTURE fires?
B: Yes. "design" → ARCHITECTURE. "observability infrastructure" → ARCHITECTURE via "infrastructure" (structural noun). Route to 3.5.

---

## T020-002 — "add a Prometheus alert" is operational gap

A: "Add a Prometheus alert rule." Predicates?
B: Zero predicates. "add" → operational gap. "Prometheus alert rule" → gap (tool configuration). Result: [] → knowledge_edge. To fire IMPLEMENT: "implement the Prometheus alerting configuration."

---

## T020-003 — "distributed tracing topology" fires ARCHITECTURE

A: "Architect the distributed tracing topology." Predicates?
B: ["ARCHITECTURE"]. "architect" → ARCHITECTURE. "distributed tracing topology" → ARCHITECTURE via "topology" and "distributed" (ARCHITECTURE trigger noun). Route to 3.5.

---

## T020-004 — "configure Prometheus" fires IMPLEMENT

A: "Configure Prometheus scrape endpoints for all services." Predicates?
B: ["IMPLEMENT"]. "configure" in concrete configuration/IaC context → IMPLEMENT. "Prometheus" → gap but doesn't suppress IMPLEMENT. Route to 4.1.

---

## T020-005 — ARCHITECTURE+IMPLEMENT+DOCUMENT for full observability setup

A: "Design the observability stack topology, implement the OTel collector pipeline, and document the sampling strategy." Predicates?
B: ["ARCHITECTURE", "IMPLEMENT", "DOCUMENT"]. "design" + "topology" → ARCHITECTURE. "implement" + "OTel collector pipeline" → IMPLEMENT. "document" + "sampling strategy" → DOCUMENT. TransitionType: feature_addition (IMPLEMENT present). Three flows: 3.5 + 4.1 + 6.2.
