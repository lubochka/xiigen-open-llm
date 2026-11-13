# Layer 0-C — Knowledge Edge Pairs (010)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## E010-001 — "pub/sub" (publish/subscribe) fires ARCHITECTURE

A: "Design the pub/sub (publish/subscribe) messaging topology for the NotificationService."
  "design" → ARCHITECTURE | "pub/sub messaging topology" → ARCHITECTURE ("topology") | "NotificationService" → domain gap
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "pub/sub" fires ARCHITECTURE as a messaging architecture pattern.

---

## E010-002 — "CDC" (Change Data Capture) fires ARCHITECTURE

A: "Architect the CDC (Change Data Capture) pipeline topology — define the event stream and downstream consumer boundaries."
  "architect" → ARCHITECTURE | "CDC pipeline topology" → ARCHITECTURE | "define" → ARCHITECTURE | "event stream boundaries" → ARCHITECTURE ("boundaries")
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "CDC" fires ARCHITECTURE as a data integration architectural pattern.

---

## E010-003 — "namespace" alone is gap; "namespace boundary" fires ARCHITECTURE

A: "Create a namespace for the payment services."
  "create" → gap (operational command, not architecture design) | "namespace" → gap (operational Kubernetes concept alone)
  CompiledPredicateIds: [] → gap
  vs "Design the namespace boundary architecture for multi-tenant workload isolation."
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "namespace" alone is operational gap. "namespace boundary" in architectural design context fires ARCHITECTURE.

---

## E010-004 — "RBAC" fires ARCHITECTURE+AUTHORIZATION

A: "Design the RBAC (Role-Based Access Control) model for the Kubernetes cluster — define namespace-scoped roles and cluster-admin boundaries."
  "design" → ARCHITECTURE | "RBAC model" → ARCHITECTURE + AUTHORIZATION | "namespace-scoped roles" → AUTHORIZATION ("roles") | "cluster-admin boundaries" → AUTHORIZATION ("boundaries")
  CompiledPredicateIds: ["ARCHITECTURE", "AUTHORIZATION"]
  Teaching: "RBAC" fires ARCHITECTURE (structural access control design) and AUTHORIZATION (permission model). Both fire independently.

---

## E010-005 — "observability" fires ARCHITECTURE in infra design

A: "Architect the observability stack — define the distributed tracing, metrics aggregation, and log shipping layers."
  "architect" → ARCHITECTURE | "observability stack" → ARCHITECTURE | "define" → ARCHITECTURE | "distributed tracing, metrics aggregation, log shipping layers" → ARCHITECTURE ("layers", "stack")
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "observability" in infrastructure architecture design fires ARCHITECTURE. "observability stack" is an architectural component.

