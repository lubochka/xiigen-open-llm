# Layer 0-C — Knowledge Edge Pairs (021)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E021-001 — Disaster recovery execution vocabulary is gap

DR gap terms: "trigger failover", "reroute traffic", "activate secondary region", "switch to DR site", "failover execution"
  Teaching: DR execution and operational switching → gap. "trigger failover" → gap (not IMPLEMENT).

---

## E021-002 — "design the DR failover architecture" fires ARCHITECTURE

A: "Design the DR (Disaster Recovery) failover architecture" → ARCHITECTURE (verb "design" + "architecture")
  Teaching: "design" + "architecture" fires ARCHITECTURE. "DR" and "failover" are modifiers.

---

## E021-003 — "build the failover service" fires IMPLEMENT

A: "Build the failover service" → IMPLEMENT (verb "build" + "service")
  Teaching: "service" fires IMPLEMENT. "failover" describes the service's purpose, not a separate predicate.

---

## E021-004 — "verify RTO is met" fires TEST

A: "Verify that RTO (Recovery Time Objective) is met" → TEST fires on "verify"
  Teaching: "verify" fires TEST. "RTO" metric context is gap but does not suppress TEST from "verify."

---

## E021-005 — "DR release criteria" fires DOCUMENT via "criteria"

A: "Document the disaster recovery release criteria" → DOCUMENT fires on "criteria"
  Teaching: "criteria" fires DOCUMENT. "disaster recovery" is a modifier.
