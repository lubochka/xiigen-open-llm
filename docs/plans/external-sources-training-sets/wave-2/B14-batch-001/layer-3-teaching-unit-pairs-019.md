# Layer 3 — Teaching Unit Pairs (019)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## T019-001 — "multi-region deployment topology" fires ARCHITECTURE

A: "Design the multi-region deployment topology." ARCHITECTURE fires?
B: Yes. "design" → ARCHITECTURE. "multi-region deployment topology" → ARCHITECTURE via "topology" and "deployment" (in design context). Route to 3.5.

---

## T019-002 — "deploy to eu-west-1" is operational gap

A: "Deploy the application to eu-west-1." Predicates?
B: Zero predicates. "deploy" standalone → operational gap. "eu-west-1" → gap. "application" → gap. Result: [] → knowledge_edge. To fire IMPLEMENT: "implement the deployment pipeline configuration for the eu-west-1 region."

---

## T019-003 — "data residency boundary" fires ARCHITECTURE via "boundary"

A: "Define the data residency boundary for the EU region." Predicates?
B: ["ARCHITECTURE"]. "define" → ARCHITECTURE. "data residency boundary" → ARCHITECTURE via "boundary" (structural architectural noun). Route to 3.5.

---

## T019-004 — ARCHITECTURE+IMPLEMENT for multi-region DNS config

A: "Architect the multi-region failover topology and implement the Route53 health check configuration." Predicates?
B: ["ARCHITECTURE", "IMPLEMENT"]. "architect" + "multi-region failover topology" → ARCHITECTURE. "implement" + "Route53 health check configuration" → IMPLEMENT. TransitionType: feature_addition. Two flows: 3.5 + 4.1.

---

## T019-005 — "active-active pattern" fires ARCHITECTURE

A: "Design the active-active deployment pattern for three regions." Predicates?
B: ["ARCHITECTURE"]. "design" → ARCHITECTURE. "active-active deployment pattern" → ARCHITECTURE via "pattern" (architectural deployment pattern) and "deployment" (design context). Route to 3.5.
