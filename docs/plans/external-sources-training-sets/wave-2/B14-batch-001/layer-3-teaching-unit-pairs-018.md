# Layer 3 — Teaching Unit Pairs (018)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## T018-001 — "disaster recovery topology" fires ARCHITECTURE

A: "Design the disaster recovery topology." ARCHITECTURE fires?
B: Yes. "design" → ARCHITECTURE. "disaster recovery topology" → ARCHITECTURE via "topology". Route to 3.5.

---

## T018-002 — "test the DR failover" fires TEST, not ARCHITECTURE

A: "Test the disaster recovery failover." What predicate fires?
B: ["TEST"]. "test" → TEST. "disaster recovery" alone without design vocabulary does not fire ARCHITECTURE. Route to 5.1. Compare: "design the disaster recovery topology" → ARCHITECTURE → 3.5.

---

## T018-003 — "back up the database" is gap

A: "Back up the production database." Predicates?
B: Zero predicates. "back up" → operational action, gap. "production database" → gap. Result: [] → knowledge_edge. To fire ARCHITECTURE: "design the backup topology for the database layer."

---

## T018-004 — ARCHITECTURE+DOCUMENT for DR runbook

A: "Architect the failover procedure and document the DR runbook." Result?
B: ["ARCHITECTURE", "DOCUMENT"]. "architect" + "failover procedure" → ARCHITECTURE. "document" + "runbook" → DOCUMENT. TransitionType: documentation. Two flows: 3.5 + 6.2.

---

## T018-005 — RPO and RTO as design constraints fire ARCHITECTURE in design context

A: "Design the DR topology to achieve RPO = 0 and RTO < 5 minutes." Predicates?
B: ["ARCHITECTURE"]. "design" → ARCHITECTURE. "DR topology" → ARCHITECTURE via "topology". RPO and RTO are constraint specifications within the ARCHITECTURE design context; they reinforce but don't independently fire ARCHITECTURE. Route to 3.5.
