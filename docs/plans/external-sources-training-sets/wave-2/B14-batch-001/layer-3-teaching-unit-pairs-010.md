# Layer 3 — Teaching Unit Pairs (010)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## T010-001 — "failover" fires ARCHITECTURE in DR (Disaster Recovery) topology design

Rule: "failover topology/strategy" + design verb → ARCHITECTURE
  "design the failover topology" → ARCHITECTURE
  "define the failover strategy" → ARCHITECTURE
  Teaching: "failover" fires ARCHITECTURE in topology/strategy design. Operational failover actions (trigger failover, test failover) may be gap.

---

## T010-002 — "canary" fires ARCHITECTURE in deployment architecture

Rule: "canary deployment architecture/topology" → ARCHITECTURE
  "architect the canary deployment topology" → ARCHITECTURE
  Teaching: Deployment patterns (canary, blue/green, rolling) fire ARCHITECTURE in design context.

---

## T010-003 — "rate limiting" fires ARCHITECTURE in design; IMPLEMENT in implementation

Rule: "design the rate limiting architecture" → ARCHITECTURE
  "implement rate limiting" → IMPLEMENT
  "add rate limiting" (code context) → IMPLEMENT
  Teaching: Rate limiting is ARCHITECTURE when designed; IMPLEMENT when built. Context determines predicate.

---

## T010-004 — "backpressure" fires ARCHITECTURE

Rule: "backpressure mechanism/pattern" → ARCHITECTURE
  "design the backpressure propagation mechanism" → ARCHITECTURE
  Teaching: "backpressure" fires ARCHITECTURE as a structural flow-control design concept.

---

## T010-005 — "disaster recovery" fires ARCHITECTURE in topology design

Rule: "disaster recovery topology/architecture" + design verb → ARCHITECTURE
  "architect the DR topology" → ARCHITECTURE
  "test disaster recovery" → TEST (not ARCHITECTURE)
  Teaching: "disaster recovery" in design context fires ARCHITECTURE. Testing DR fires TEST independently.

