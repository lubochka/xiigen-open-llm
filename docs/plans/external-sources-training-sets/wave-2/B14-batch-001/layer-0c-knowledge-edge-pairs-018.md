# Layer 0-C — Knowledge Edge Pairs (018)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-018-001 — "active-active" pattern fires ARCHITECTURE

A: "Architect the active-active failover topology — synchronous replication, split-brain prevention, and quorum-based arbitration."
  "architect" → ARCHITECTURE | "active-active failover topology" → ARCHITECTURE ("topology", "pattern") | "split-brain prevention" → ARCHITECTURE (distributed systems design) | "quorum-based arbitration" → ARCHITECTURE ("pattern")
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: Distributed systems patterns (active-active, split-brain, quorum) fire ARCHITECTURE via "pattern" and architectural structural nouns.

---

## B14-edge-018-002 — "backup" alone is gap

A: "Back up the production database."
  "back up" → gap (operational action) | "production database" → gap
  CompiledPredicateIds: []
  Gap signal: gap-devops-dr-001 — "backup" as an operational action is gap.
  Teaching: To fire ARCHITECTURE: "design the backup topology and restoration strategy for the database layer."

---

## B14-edge-018-003 — "RPO" and "RTO" targets fire ARCHITECTURE in design context

A: "Design the DR (Disaster Recovery) topology to achieve RPO (Recovery Point Objective) = 0 and RTO (Recovery Time Objective) < 5 minutes."
  "design" → ARCHITECTURE | "DR topology" → ARCHITECTURE ("topology") | "RPO", "RTO" → ARCHITECTURE (design constraint vocabulary in an architecture context)
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: RPO and RTO as design constraints fire ARCHITECTURE when paired with design verbs and topology nouns.

---

## B14-edge-018-004 — "replication topology" fires ARCHITECTURE

A: "Define the cross-region replication topology for the UserProfile data store."
  "define" → ARCHITECTURE | "cross-region replication topology" → ARCHITECTURE ("topology") | "data store" → ARCHITECTURE
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "replication topology" is an ARCHITECTURE phrase via "topology". "cross-region" is gap but does not suppress ARCHITECTURE.
